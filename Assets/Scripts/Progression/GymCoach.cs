using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// EL ENTRENADOR DEL GIMNASIO en el bloque de las 14 h (docs/gimnasio-maquinas.md §15.5; el papel es de
/// <see cref="GymProgramming.Trainer"/>, Goluis). Con los mismos datos de rutina que todos:
///  1. CLASE DE CALENTAMIENTO (los primeros 10 min de juego): se pone en la zona libre y dirige sentadillas con peso corporal y
///     saltos de tijera; quien llega a calentar (<see cref="GymTrainingSession"/> en su calentamiento) se coloca en una fila
///     delante de él y repite cada movimiento a su compás.
///  2. RONDAS: va de máquina en máquina a quien está haciendo una serie (primero a quien mueve peso libre pesado), le CORRIGE
///     la técnica (<see cref="GymProgramming.Cue"/>) y, en banca y sentadilla pesadas, hace de SPOTTER hasta que acaba la serie.
///  3. Los días con gimnasio en su propio plan, la última parte del bloque hace SU rutina.
/// Lo de escribir y ajustar los planes es su misión de la jornada («gym_planes», <see cref="GymProgramming.Review"/>).
/// Todas las esperas están acotadas (reunirse 75 s, andar 60 s, corregir 5 s, hacer de spotter 20 s).
/// </summary>
public class GymCoach
{
    public enum Phase { ToClass, Class, Rounds, Own, Done }

    public readonly GameObject g;
    public readonly Anima a;
    public Phase phase { get; private set; }
    public Vector3 classPoint; public Quaternion classFacing;
    public float walkSpeed = 2.6f;
    public const float MoveHalf = 0.8f, WalkLimit = 60f, CorrectFor = 5f, SpotLimit = 20f;

    public int classReps, corrections, spots, attended;
    /// <summary>¿Empezó la clase de verdad en su punto del gimnasio? (No donde estuviera: JornadaTest, ronda 7.)</summary>
    public bool classAtPoint;
    public readonly List<string> notes = new List<string>();
    public GymTrainingSession own;

    float _classSec, _hardEnd, _classUntil, _roundsUntil, _ownBudget, _phaseAt;
    readonly HashSet<GymTrainingSession> _inPlace = new HashSet<GymTrainingSession>();
    readonly GymProgramming.DayRoutine _ownRoutine;
    readonly List<GymTrainingSession> _attendees = new List<GymTrainingSession>();
    readonly Dictionary<GymTrainingSession, (int key, AsanaPoseSpell pose)> _attPose = new Dictionary<GymTrainingSession, (int, AsanaPoseSpell)>();
    readonly Dictionary<GymTrainingSession, float> _visited = new Dictionary<GymTrainingSession, float>();
    GymTrainingSession _target; GymStation _targetSt; float _targetSince, _atTargetSince = -1f; bool _spotting;
    AsanaPoseSpell _pose; int _poseKey = -1;

    /// <summary><paramref name="classSec"/>, <paramref name="roundsSec"/>: duración (s) de la clase y de las rondas;
    /// <paramref name="own"/> (puede ser null): su rutina de hoy, con <paramref name="ownBudget"/> s para hacerla.</summary>
    public GymCoach(GameObject g, Vector3 classPoint, Quaternion facing, float classSec, float roundsSec, GymProgramming.DayRoutine own, float ownBudget, float gatherSec = 75f)
    {
        this.g = g; a = g.GetComponent<Anima>();
        this.classPoint = classPoint; classFacing = facing;
        // LA CLASE EMPIEZA CUANDO ÉL LLEGA, NO A LAS 14:00: de otra torre se tarda 35–95 s y la clase dura 25 s. Antes el reloj corría
        // desde el principio: a los 25 s «daba la clase» donde estuviera y se quedaba allí (Andrea, parada en Cría a las 14:17, sin
        // alumnos: JornadaTest, ronda 7). Tope de todo: duración + gatherSec para reunirse.
        _classSec = Mathf.Max(0f, classSec);
        _hardEnd = _classUntil = Time.time + _classSec + Mathf.Max(0f, gatherSec);
        _roundsUntil = Time.time + _classSec + Mathf.Max(0f, roundsSec);
        _ownRoutine = own; _ownBudget = ownBudget;
        phase = classSec > 0f ? Phase.ToClass : Phase.Rounds; _phaseAt = Time.time;
        Note(classSec > 0f ? "abre la clase de calentamiento" : "empieza las rondas por las máquinas");
    }

    /// <summary>¿Hay clase ahora? (acotada por su hora de fin: si el entrenador se va a comer, los alumnos no esperan para siempre)</summary>
    public bool ClassRunning => (phase == Phase.ToClass || phase == Phase.Class) && Time.time < _hardEnd;
    /// <summary>¿Está en ese punto? En el MISMO PISO: los pisos de una torre comparten x y z.</summary>
    static bool At(Transform t, Vector3 to, float r) { Vector3 d = to - t.position; bool floor = Mathf.Abs(d.y) < 1.5f; d.y = 0f; return floor && d.sqrMagnitude <= r * r; }
    public string Status => phase == Phase.ToClass || phase == Phase.Class ? $"Entrenador: clase de calentamiento ({_attendees.Count})"
        : phase == Phase.Rounds ? (_target != null ? $"Entrenador: {(_spotting ? "hace de spotter a" : "corrige a")} {_target.Who}" : "Entrenador: rondas por las máquinas")
        : phase == Phase.Own ? (own != null ? own.Status() : "Su rutina") : "Entrenador";

    void Note(string s) { string l = $"[Entrenador] {g.name} {s}"; notes.Add(l); Debug.Log(l); GymTrainingSession.Recent.Add(l); if (GymTrainingSession.Recent.Count > 40) GymTrainingSession.Recent.RemoveAt(0); }

    // ── La clase ────────────────────────────────────────────────────────────────────────────────
    /// <summary>El movimiento de la clase ahora: 0 = sentadilla, 1 = saltos de tijera; <paramref name="b"/> = fase B.</summary>
    public int Move(out bool b)
    {
        float t = Mathf.Max(0f, Time.time - _phaseAt);
        int k = Mathf.FloorToInt(t / MoveHalf);
        b = k % 2 == 1;
        float total = Mathf.Max(1f, _classSec);   // (los movimientos se reparten en lo que dura; si espera a los que llegan tarde, sigue con el último)
        return t < total * 0.5f ? 0 : 1;
    }

    public static AsanaPoseSpell ClassPose(int move, bool b)
    {
        AsanaPoseSpell s;
        if (move == 0)
            s = b ? DailyPoses.Make("sentadilla (abajo)", 0.55f, (BodyPart.Back, new Vector3(25f, 0f, 0f)), (BodyPart.ShoulderLeft, new Vector3(-90f, 0f, 0f)), (BodyPart.ShoulderRight, new Vector3(-90f, 0f, 0f)),
                        (BodyPart.ThighLeft, new Vector3(-80f, 0f, 8f)), (BodyPart.ThighRight, new Vector3(-80f, 0f, -8f)), (BodyPart.ShinLeft, new Vector3(85f, 0f, 0f)), (BodyPart.ShinRight, new Vector3(85f, 0f, 0f)))
                  : DailyPoses.Make("sentadilla (arriba)", -1f, (BodyPart.ShoulderLeft, new Vector3(-90f, 0f, 0f)), (BodyPart.ShoulderRight, new Vector3(-90f, 0f, 0f)));
        else
            s = b ? DailyPoses.Make("tijera (abierta)", -1f, (BodyPart.ShoulderLeft, new Vector3(0f, 0f, 150f)), (BodyPart.ShoulderRight, new Vector3(0f, 0f, -150f)),
                        (BodyPart.ThighLeft, new Vector3(0f, 0f, 14f)), (BodyPart.ThighRight, new Vector3(0f, 0f, -14f)))
                  : DailyPoses.Make("tijera (cerrada)", -1f);
        s.duration = MoveHalf; s.reachFraction = 0.8f;
        return s;
    }

    /// <summary>Dónde se pone el alumno <paramref name="k"/>: filas de 4 delante del entrenador, a 1,1 m entre sí.</summary>
    public Vector3 Slot(int k)
    {
        Vector3 f = classFacing * Vector3.forward, r = classFacing * Vector3.right;
        Vector3 p = classPoint + f * (2f + 1.2f * (k / 4)) + r * (((k % 4) - 1.5f) * 1.1f);
        if (UnityEngine.AI.NavMesh.SamplePosition(p + Vector3.down, out UnityEngine.AI.NavMeshHit h, 1.2f, UnityEngine.AI.NavMesh.AllAreas) && Mathf.Abs(h.position.y - (p.y - 1f)) < 0.8f) p = h.position + Vector3.up;
        return p;
    }

    /// <summary>Un fotograma de un alumno en la clase: anda a su sitio de la fila y repite el movimiento del entrenador.</summary>
    public void Attend(GymTrainingSession s)
    {
        int k = _attendees.IndexOf(s);
        if (k < 0) { _attendees.Add(s); k = _attendees.Count - 1; attended++; }
        Vector3 to = Slot(k);
        // Anda hasta su sitio todo lo que haga falta (viene de otra torre); la clase ya está acotada por _hardEnd. Solo repite los
        // movimientos en su sitio: antes, a los 25 s, hacía sentadillas donde estuviera.
        if (!At(s.g.transform, to, 0.6f)) { _inPlace.Remove(s); PathMover.Of(s.g.transform).StepToward(to, walkSpeed); return; }
        _inPlace.Add(s);
        if (phase != Phase.Class) return;   // ya en su sitio, espera a que llegue el entrenador
        Vector3 look = classPoint - s.g.transform.position; look.y = 0f;
        if (look.sqrMagnitude > 0.01f) s.g.transform.rotation = Quaternion.LookRotation(look);
        int key = Move(out bool b) * 2 + (b ? 1 : 0);
        if (!_attPose.TryGetValue(s, out var ap) || ap.key != key) _attPose[s] = ap = (key, ClassPose(key / 2, b));
        AsanaPoseSpell p = ap.pose;
        if (s.a != null) p.Tick(s.a);
    }

    // ── Un fotograma del entrenador ─────────────────────────────────────────────────────────────
    /// <summary>true mientras lo conduce; <paramref name="sessions"/>: las de los demás hoy (para las rondas).</summary>
    public bool Tick(IEnumerable<GymTrainingSession> sessions)
    {
        if (g == null) return false;
        switch (phase)
        {
            case Phase.ToClass:
                {
                    if (Time.time >= _hardEnd) { Note("no llega a tiempo: hoy no hay clase de calentamiento"); EndClass(); return true; }
                    if (!At(g.transform, classPoint, 1f)) { PathMover.Of(g.transform).StepToward(classPoint, walkSpeed); return true; }
                    phase = Phase.Class; _phaseAt = Time.time; classAtPoint = true;
                    _classUntil = Mathf.Min(_hardEnd, Time.time + _classSec);   // desde que llega
                    Note("dirige el calentamiento: sentadillas con peso corporal y saltos de tijera");
                    return true;
                }
            case Phase.Class:
                {
                    // Dura lo suyo desde que él llega y espera a quien viene de camino (hasta el tope): los alumnos salen de otras torres.
                    if (Time.time >= _hardEnd || (Time.time >= _classUntil && _attendees.All(s => _inPlace.Contains(s)))) { EndClass(); return true; }
                    g.transform.rotation = Quaternion.LookRotation(classFacing * Vector3.forward);
                    int key = Move(out bool b) * 2 + (b ? 1 : 0);
                    if (key != _poseKey) { if (_poseKey >= 0 && (_poseKey & 1) == 1 && !b) classReps++; _poseKey = key; _pose = ClassPose(key / 2, b); }
                    if (a != null && _pose != null) _pose.Tick(a);
                    return true;
                }
            case Phase.Rounds:
                if (Time.time >= _roundsUntil && _ownRoutine != null && !_spotting) { DropTarget(); phase = Phase.Own; own = new GymTrainingSession(g, _ownRoutine, _ownBudget); Note($"hace su rutina: «{_ownRoutine.name}»"); return true; }
                Rounds(sessions);
                return true;
            case Phase.Own:
                if (own == null || !own.Tick(null)) { phase = Phase.Done; return false; }
                return true;
        }
        return false;
    }

    void EndClass()
    {
        Note($"termina la clase: {_attendees.Count} alumnos, {classReps} repeticiones");
        foreach (GymTrainingSession s in _attendees) if (s.a != null) DailyPoses.Stand().ApplyFinal(s.a);
        if (a != null) DailyPoses.Stand().ApplyFinal(a);
        phase = Phase.Rounds; _phaseAt = Time.time;
    }

    void Rounds(IEnumerable<GymTrainingSession> sessions)
    {
        if (_target == null || _target.Done || _target.Station == null || _target.Station != _targetSt)
        {
            if (_spotting && _target != null) Note($"termina de hacer de spotter a {_target.Who}");
            DropTarget();
            // El siguiente: alguien en plena serie (o descansando), que no haya visto hace poco; primero el peso libre pesado.
            GymTrainingSession best = null; float bs = float.MinValue;
            foreach (GymTrainingSession s in sessions)
            {
                if (s == null || s == own || s.g == g || s.Station == null || (s.phase != GymTrainingSession.Phase.Set && s.phase != GymTrainingSession.Phase.Rest) || s.Current == null || s.Current.timed) continue;
                if (_visited.TryGetValue(s, out float at) && Time.time - at < 20f) continue;
                float score = (GymProgramming.NeedsSpotter(s.Station.type, s.Current.rpe) ? 100f : 0f) - Vector3.Distance(s.g.transform.position, g.transform.position);
                if (score > bs) { bs = score; best = s; }
            }
            if (best == null)
            {
                // SIN NADIE A QUIEN CORREGIR: al gimnasio, no parado donde esté (en Cría, si la clase no se dio; JornadaTest, ronda 7).
                if (a != null && _poseKey != -2) { DailyPoses.Stand().ApplyFinal(a); _poseKey = -2; }
                if (!At(g.transform, classPoint, 1.5f)) PathMover.Of(g.transform).StepToward(classPoint, walkSpeed);
                return;
            }
            _target = best; _targetSt = best.Station; _targetSince = Time.time; _atTargetSince = -1f; _visited[best] = Time.time;
        }
        Vector3 to = WorkdayDirector.GymApproach(_targetSt);
        if (_atTargetSince < 0f)
        {
            if (!WorkdayDirector.GymArrived(g, _targetSt))
            {
                if (Time.time - _targetSince > WalkLimit) { DropTarget(); return; }
                PathMover.Of(g.transform).StepToward(to, walkSpeed);
                return;
            }
            _atTargetSince = Time.time;
            GymProgramming.Prescription x = _target.Current;
            _spotting = x != null && GymProgramming.NeedsSpotter(_targetSt.type, x.rpe);
            if (_spotting) { spots++; Note($"hace de spotter a {_target.Who} en {GymStation.Label(_targetSt.type)} ({x.load:0.##} kg)"); }
            else { corrections++; Note($"corrige a {_target.Who} en {GymStation.Label(_targetSt.type)}: «{GymProgramming.Cue(_targetSt.type)}»"); }
        }
        // Junto a la máquina: lo mira; corrigiendo, señala con el brazo.
        Vector3 look = _target.g.transform.position - g.transform.position; look.y = 0f;
        if (look.sqrMagnitude > 0.01f) g.transform.rotation = Quaternion.LookRotation(look);
        if (a != null && _poseKey != -3) { (_spotting ? DailyPoses.Make("spotter", -1f, (BodyPart.ShoulderLeft, new Vector3(-70f, 0f, 0f)), (BodyPart.ShoulderRight, new Vector3(-70f, 0f, 0f)), (BodyPart.ForearmLeft, new Vector3(-40f, 0f, 0f)), (BodyPart.ForearmRight, new Vector3(-40f, 0f, 0f)))
                                                     : DailyPoses.Make("señala", -1f, (BodyPart.ShoulderRight, new Vector3(-80f, 0f, -10f)))).ApplyFinal(a); _poseKey = -3; }
        bool setOver = _target.phase != GymTrainingSession.Phase.Set;
        if (_spotting ? (setOver && Time.time - _atTargetSince > 1f) || Time.time - _atTargetSince > SpotLimit : Time.time - _atTargetSince > CorrectFor) DropTarget();
    }

    void DropTarget()
    {
        if (_target != null && a != null) { DailyPoses.Stand().ApplyFinal(a); _poseKey = -2; }
        _target = null; _targetSt = null; _spotting = false; _atTargetSince = -1f;
    }

    /// <summary>Se va (fin del bloque o una necesidad): deja libre su máquina si estaba en su rutina.</summary>
    public void Stop(string why)
    {
        DropTarget();
        if (own != null) own.Stop(why);
        if (phase == Phase.Class || phase == Phase.ToClass) EndClass();
        phase = Phase.Done;
    }
}
