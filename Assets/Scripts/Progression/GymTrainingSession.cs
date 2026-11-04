using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Prescription = GymProgramming.Prescription;

/// <summary>
/// UNA SESIÓN DE GIMNASIO DE VERDAD (docs/gimnasio-maquinas.md §15): la persona sigue la rutina de su plan para hoy
/// (<see cref="GymProgramming.Prescribe"/>) en orden:
///  1. CALENTAMIENTO: la clase del entrenador (<see cref="GymCoach"/>) si la hay; si no, 5 min de cardio suave en una máquina.
///  2. Cada EJERCICIO: busca una copia libre de su máquina (la del mismo piso primero); si todas están ocupadas, la cambia por una
///     alternativa del mismo músculo; si tampoco, ESPERA (como mucho <see cref="MaxWait"/> s) y luego lo deja para el final de la
///     rutina; si a la segunda tampoco, lo salta. Anda hasta ella, se coloca y hace las SERIES: cada repetición es A → B → A
///     (cuenta al volver a A); entre series DESCANSA en la máquina (postura A) el descanso real pasado a tiempo de juego
///     (90 s = 1,5 min de juego). La primera serie del primer multiarticular es de APROXIMACIÓN (50 % de la carga).
///  3. VUELTA A LA CALMA: 5 min de estiramientos (en la máquina de isquios o, si está ocupada, de pie donde esté).
/// Cada serie deja una línea en el registro: persona · ejercicio · serie i/n · reps · carga. El ritmo de cada repetición se
/// ajusta para que la rutina quepa en el tiempo que queda (entre 0,3 s y el ritmo real del equipo por mitad).
/// Sin escena propia: la conduce <see cref="WorkdayDirector"/> (14–16 h) o una prueba (<see cref="GymTrainingTest"/>).
/// </summary>
public class GymTrainingSession
{
    public enum Phase { Warmup, Walk, Wait, Set, Rest, InPlace, Done }
    public enum Kind { Station, Swap, Wait, Defer, Skip, Set, Rest, Done, Stop, Class }

    /// <summary>Un suceso de la sesión (para las pruebas y el panel).</summary>
    public struct Event
    {
        public Kind kind; public string exercise; public int set, sets, reps; public float load, time; public GymStation st;
        public override string ToString() => $"{time:0.0}s {kind} {exercise} {set}/{sets} {reps}";
    }

    public static float MaxWait = 8f;        // s (de juego) esperando máquina antes de dejar el ejercicio para el final
    public static float WalkTimeout = 45f;   // s andando hacia una máquina antes de buscar otra
    public const float MinHalf = 0.3f;

    public readonly GameObject g;
    public readonly Anima a;
    public readonly GymProgramming.DayRoutine routine;
    public readonly List<Prescription> queue;
    public readonly List<Event> events = new List<Event>();
    public int idx, set, rep;
    public Phase phase { get; private set; }
    public WorkdayDirector.GymUse use;
    public float walkSpeed = 2.6f;
    public float half;                       // s por mitad de repetición (A → B)
    public string stopReason;
    float _phaseAt, _until, _waitSince, _walkSince;
    readonly HashSet<Prescription> _deferred = new HashSet<Prescription>();
    readonly HashSet<GymStation> _unreachable = new HashSet<GymStation>();   // no llegó andando: no vuelve a intentarlo hoy
    AsanaPoseSpell _inPlace;

    /// <summary>Las últimas líneas del registro de series (todas las personas), para el panel.</summary>
    public static readonly List<string> Recent = new List<string>();
    static float MinuteSecs => TimeController.timeController != null && TimeController.timeController.TimeSpeedMinuteSecs > 0.01f ? TimeController.timeController.TimeSpeedMinuteSecs : 2.5f;

    public Prescription Current => idx < queue.Count ? queue[idx] : null;
    public GymStation Station => use != null ? use.st : null;
    public bool Done => phase == Phase.Done;
    public string Who => g != null ? g.name : routine.who;
    public int TotalSets(Prescription x) => x.timed ? 1 : x.sets + x.rampSets;
    public bool Ramp(Prescription x, int s) => !x.timed && s <= x.rampSets;
    public bool InSet => phase == Phase.Set && Current != null && !Current.timed;

    /// <summary>Empieza la sesión de <paramref name="g"/> con <paramref name="r"/>; <paramref name="budget"/> = s que quedan.</summary>
    public GymTrainingSession(GameObject g, GymProgramming.DayRoutine r, float budget)
    {
        this.g = g; a = g != null ? g.GetComponent<Anima>() : null; routine = r;
        queue = r.items.Select(x => x).ToList();
        foreach (Prescription x in queue) if (x.timed) x.origin = x.station;   // (la máquina del plan; en los de por tiempo no viene puesta)
        float ms = MinuteSecs, rest = 0f, timed = 0f; int halves = 0, main = 0;
        foreach (Prescription x in queue)
        {
            if (x.timed) { timed += TimedSeconds(x); continue; }
            main++; halves += (x.sets + x.rampSets) * x.reps * 2; rest += (x.sets + x.rampSets - 1) * RestSeconds(x);
        }
        half = halves > 0 ? Mathf.Clamp((budget - rest - timed - main * 8f) / halves, MinHalf, 1.5f) : 1f;
        phase = Phase.Warmup; _phaseAt = Time.time;
        Note($"empieza «{r.name}» (semana {r.week + 1}{(r.deload ? ", descarga" : "")}): {string.Join(" → ", r.Main.Select(x => $"{x.Name} {x.sets}×{x.reps}"))}");
    }

    /// <summary>Descanso entre series en s de juego: el real (90 s) pasado a minutos de juego (1,5 min). 2–10 s.</summary>
    public static float RestSeconds(Prescription x) => Mathf.Clamp(x.restSec / 60f * MinuteSecs, 2f, 10f);
    /// <summary>Duración de lo que va por tiempo (5 min de juego); 6–20 s.</summary>
    public static float TimedSeconds(Prescription x) => Mathf.Clamp(x.minutes * MinuteSecs, 6f, 20f);

    void Note(string s)
    {
        string line = $"[Gimnasio] {Who} · {s}";
        Debug.Log(line);
        Recent.Add(line); if (Recent.Count > 40) Recent.RemoveAt(0);
    }
    void Ev(Kind k, Prescription x, GymStation st = null) => events.Add(new Event { kind = k, exercise = x != null ? x.Name : "", set = set, sets = x != null ? TotalSets(x) : 0, reps = rep, load = x != null ? x.load : 0f, time = Time.time, st = st ?? Station });

    /// <summary>Un fotograma. <paramref name="coach"/>: el entrenador (su clase es el calentamiento). false = ya terminó.</summary>
    public bool Tick(GymCoach coach)
    {
        if (g == null || phase == Phase.Done) return false;
        Prescription x = Current;
        if (x == null) { Finish(); return false; }
        switch (phase)
        {
            case Phase.Warmup:
                if (x.part == "calentamiento" && coach != null && coach.g != g && coach.ClassRunning)
                {
                    if (!_classJoined) { _classJoined = true; Ev(Kind.Class, x); Note($"calentamiento en la clase de {coach.g.name}"); }
                    coach.Attend(this);
                    return true;
                }
                if (_classJoined) { Ev(Kind.Done, x); Next(); return true; }   // la clase terminó: ese fue su calentamiento
                phase = Phase.Walk; _walkSince = Time.time; return Tick(coach);
            case Phase.Walk: return WalkStep(x);
            case Phase.Wait:
                if (Pick(x) != null) { phase = Phase.Walk; _walkSince = Time.time; return true; }
                if (Time.time - _waitSince < MaxWait) return true;
                if (x.timed) { StartInPlace(x); return true; }
                if (!_deferred.Contains(x) && idx < queue.Count - 1 && queue.Skip(idx + 1).Any(q => !q.timed))
                {
                    _deferred.Add(x); queue.RemoveAt(idx); queue.Insert(LastMainIndex() + 1, x);
                    Ev(Kind.Defer, x); Note($"{x.Name}: todas ocupadas, lo deja para el final");
                }
                else { Ev(Kind.Skip, x); MarkMissed(x); Note($"{x.Name}: sin máquina libre, lo salta"); idx++; }
                phase = Phase.Walk; _walkSince = Time.time; return true;
            case Phase.Set: return SetStep(x);
            case Phase.Rest:
                WorkdayDirector.GymHold(g, a, use);
                if (Time.time >= _until) { set++; rep = 0; use.b = false; use.phaseAt = Time.time; phase = Phase.Set; }
                return true;
            case Phase.InPlace:
                if (a != null && _inPlace != null) _inPlace.Tick(a);
                if (Time.time >= _until) { Ev(Kind.Done, x); Note($"{x.part}: {x.Name.ToLowerInvariant()} de pie, {x.minutes:0} min"); _inPlace = null; DailyPoses.Stand().ApplyFinal(a); Next(); }
                return true;
        }
        return true;
    }
    bool _classJoined;

    int LastMainIndex() { for (int i = queue.Count - 1; i >= 0; i--) if (!queue[i].timed) return i; return queue.Count - 1; }

    /// <summary>La máquina para <paramref name="x"/>: la suya o una alternativa, libre (o ya suya), la del piso más cercano.</summary>
    GymStation Pick(Prescription x)
    {
        Vector3 me = g.transform.position;
        GymMachineType[] alts = GymProgramming.Ex(x.origin).alt;
        IEnumerable<GymMachineType> types = new[] { x.station }.Concat(alts ?? new GymMachineType[0]);
        foreach (GymMachineType t in types)
        {
            GymStation best = null; float bd = float.MaxValue;
            foreach (GymStation s in GymStation.All)
            {
                if (s == null || s.type != t || !s.isActiveAndEnabled || _unreachable.Contains(s) || !(s.Free || s.Occupant == Who)) continue;
                Vector3 d = s.ApproachPoint - me; float cost = Mathf.Abs(d.y) * 10f + new Vector2(d.x, d.z).magnitude;
                if (cost < bd) { bd = cost; best = s; }
            }
            if (best != null) return best;
        }
        return null;
    }

    bool WalkStep(Prescription x)
    {
        if (use == null)
        {
            GymStation st = Pick(x);
            if (st == null) { if (phase != Phase.Wait) { phase = Phase.Wait; _waitSince = Time.time; Ev(Kind.Wait, x); } return true; }
            if (!st.Claim(Who)) return true;
            if (st.type != x.station)
            {
                Ev(Kind.Swap, x, st);
                Note($"{x.Name} ocupada: la cambia por {GymStation.Label(st.type)}");
                Prescription sw = Swap(x, st.type);
                queue[idx] = sw; x = sw;
            }
            use = new WorkdayDirector.GymUse { st = st, since = Time.time };
            _walkSince = Time.time;
        }
        if (!WorkdayDirector.GymArrived(g, use.st))
        {
            if (Time.time - _walkSince > WalkTimeout) { _unreachable.Add(use.st); Release(); phase = Phase.Wait; _waitSince = Time.time - MaxWait * 0.5f; return true; }   // no llega: otra copia
            PathMover.Of(g.transform).StepToward(WorkdayDirector.GymApproach(use.st), walkSpeed);
            return true;
        }
        WorkdayDirector.GymSit(use);
        Ev(Kind.Station, x);
        set = 1; rep = 0; phase = Phase.Set; _phaseAt = Time.time;
        if (x.timed) _until = Time.time + TimedSeconds(x);
        return true;
    }

    /// <summary>La misma prescripción en otra máquina (la carga, en proporción a la carga base de cada una).</summary>
    static Prescription Swap(Prescription x, GymMachineType t)
    {
        GymProgramming.Exercise from = GymProgramming.Ex(x.station), to = GymProgramming.Ex(t);
        float load = x.load <= 0f || to.Bodyweight ? 0f : from.Bodyweight ? to.baseKg : Mathf.Round(x.load * to.baseKg / from.baseKg / 1.25f) * 1.25f;
        return new Prescription { station = t, origin = x.origin, sets = x.sets, reps = x.reps, rampSets = x.rampSets, load = load, restSec = x.restSec, rpe = x.rpe, timed = x.timed, minutes = x.minutes, part = x.part };
    }

    bool SetStep(Prescription x)
    {
        float h = x.timed ? GymPoses.RepHalf(use.st.type) : Mathf.Min(GymPoses.RepHalf(use.st.type), half);
        bool flip = WorkdayDirector.GymRep(g, a, use, h);
        if (x.timed)
        {
            if (Time.time >= _until) { Ev(Kind.Done, x); Note($"{x.part}: {x.Name} {x.minutes:0} min (RPE {x.rpe:0})"); Next(); }
            return true;
        }
        if (flip && !use.b)
        {
            rep++;
            if (rep >= x.reps - 1) g.GetComponentInChildren<CubeFace>()?.Show(CubeFace.Expression.Angry, h * 2f);   // las últimas: cara de esfuerzo
        }
        if (rep < x.reps) return true;
        bool ramp = Ramp(x, set);
        float load = ramp ? Mathf.Round(x.load * 0.5f / 1.25f) * 1.25f : x.load;
        Ev(Kind.Set, x);
        int n = TotalSets(x), shown = ramp ? set : set - x.rampSets, of = x.sets;
        Note($"{routine.name} · {x.Name} · {(ramp ? "aproximación" : $"serie {shown}/{of}")} · {rep} reps · {(load <= 0f ? "peso corporal" : $"{load:0.##} kg")}{(ramp ? "" : $" · RPE {x.rpe:0}")}");
        if (set < n) { phase = Phase.Rest; _until = Time.time + RestSeconds(x); Ev(Kind.Rest, x); return true; }
        Ev(Kind.Done, x);
        Next();
        return true;
    }

    void StartInPlace(Prescription x)
    {
        Release();
        _inPlace = DailyPoses.Make("estiramiento de pie", -1f, (BodyPart.Back, new Vector3(55f, 0f, 0f)), (BodyPart.Chest, new Vector3(20f, 0f, 0f)),
            (BodyPart.ShoulderLeft, new Vector3(-70f, 0f, 0f)), (BodyPart.ShoulderRight, new Vector3(-70f, 0f, 0f)));
        _inPlace.duration = 1.5f; _inPlace.reachFraction = 1f;
        _until = Time.time + TimedSeconds(x); phase = Phase.InPlace;
    }

    void Next()
    {
        Release();
        idx++; set = 0; rep = 0;
        if (idx >= queue.Count) { Finish(); return; }
        phase = Phase.Walk; _walkSince = Time.time;
    }

    void Finish()
    {
        Release();
        if (phase == Phase.Done) return;
        phase = Phase.Done;
        GymProgramming.TrainingPlan p = GymProgramming.For(routine.who);
        if (p != null) p.SessionsDone++;
        Ev(Kind.Done, null);
        Note($"termina «{routine.name}»: {events.Count(e => e.kind == Kind.Set)} series");
    }

    void MarkMissed(Prescription x) { GymProgramming.TrainingPlan p = GymProgramming.For(routine.who); if (p != null && !x.timed) p.missed.Add(x.origin); }

    /// <summary>Se levanta y suelta la máquina (la reserva de <see cref="GymStation"/> y el ajuste de GymFit).</summary>
    public void Release()
    {
        if (use == null) return;
        GymStation.Release(Who);
        WorkdayDirector.GymStandUp(g, use);
        use = null;
    }

    /// <summary>Lo deja a medias (una necesidad: comer, el baño): suelta la máquina; al volver repite la serie en curso.</summary>
    public void Pause()
    {
        if (phase == Phase.Done) return;
        Release();
        if (phase == Phase.Set || phase == Phase.Rest || phase == Phase.Wait || phase == Phase.InPlace) { phase = Phase.Walk; rep = 0; if (set < 1) set = 0; }
        _walkSince = Time.time;
    }

    /// <summary>Se acaba el tiempo: lo que no hizo queda como series sin acabar (el entrenador no sube esa carga).</summary>
    public void Stop(string why)
    {
        if (phase == Phase.Done) return;
        stopReason = why;
        for (int i = idx; i < queue.Count; i++) MarkMissed(queue[i]);
        Ev(Kind.Stop, Current);
        Note($"{why}: deja {queue.Skip(idx).Count(q => !q.timed)} ejercicios sin hacer");
        Release();
        phase = Phase.Done;
    }

    /// <summary>Una línea con lo que hace ahora (el panel y la etiqueta).</summary>
    public string Status()
    {
        Prescription x = Current;
        if (phase == Phase.Done) return stopReason != null ? $"Gimnasio: {stopReason}" : "Gimnasio: rutina terminada";
        if (x == null) return "Gimnasio";
        switch (phase)
        {
            case Phase.Warmup: return "Calentamiento";
            case Phase.Walk: return $"Va a {x.Name}";
            case Phase.Wait: return $"Espera máquina: {x.Name}";
            case Phase.Rest: return $"{x.Name} · descansa ({set}/{TotalSets(x)})";
            case Phase.InPlace: return $"{x.part}: de pie";
            default: return x.timed ? $"{x.part}: {x.Name}" : $"{x.Name} · serie {set}/{TotalSets(x)} · rep {rep}/{x.reps}";
        }
    }
}
