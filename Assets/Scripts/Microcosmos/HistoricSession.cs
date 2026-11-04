using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Prescription = HistoricTraining.Prescription;

/// <summary>
/// UNA SESIÓN DE ENTRENAMIENTO DE ÉPOCA en el edificio de entrenamiento de un pueblo (docs/gimnasio-maquinas.md §16): la misma lógica
/// que la del gimnasio del santuario (<see cref="GymTrainingSession"/>), con los ejercicios de su época
/// (<see cref="HistoricTraining"/>). En orden: calentamiento (la clase del maestro, si la hay), cada ejercicio —coge su equipo del
/// armario, va a un SITIO libre del suelo de entrenar (un sitio por persona, a 2 m: nunca dos en el mismo; si no hay, espera ≤ 8 s y
/// lo deja para el final; a la segunda, lo salta), series × repeticiones contadas (A → B → A) con su descanso— y vuelta a la calma.
/// Anima el cuerpo de insecto (<see cref="CubeInsect.Martial"/>/<see cref="CubeInsect.Push"/>) o el humano (posturas de cubos).
/// </summary>
public class HistoricSession
{
    public enum Phase { Warmup, Fetch, Walk, Wait, Set, Rest, Done }
    public enum Kind { Spot, Wait, Defer, Skip, Set, Rest, Done, Class, Stop }
    public struct Event { public Kind kind; public string drill; public int set, sets, reps; public float time; public int spot; }

    public static float MaxWait = 8f, WalkTimeout = 30f;
    public readonly GameObject g;
    public readonly HistoricClass cls;
    public readonly HistoricTraining.Routine routine;
    public readonly List<Prescription> queue;
    public readonly List<Event> events = new List<Event>();
    public int idx, set, rep, spot = -1;
    public Phase phase { get; private set; }
    public float half, speed = 2.4f;
    public string stopReason;
    bool _b, _joined; float _phaseAt, _until, _waitSince, _walkSince;
    readonly HashSet<Prescription> _deferred = new HashSet<Prescription>();
    readonly CubeInsect _ci; readonly Anima _a;

    public static readonly List<string> Recent = new List<string>();
    static float MinuteSecs => TimeController.timeController != null && TimeController.timeController.TimeSpeedMinuteSecs > 0.01f ? TimeController.timeController.TimeSpeedMinuteSecs : 2.5f;
    public Prescription Current => idx < queue.Count ? queue[idx] : null;
    public bool Done => phase == Phase.Done;
    public string Who => g != null ? g.name : routine.who;
    public static int TotalSets(Prescription x) => x.Timed ? 1 : x.sets;

    public HistoricSession(GameObject g, HistoricClass cls, HistoricTraining.Routine r, float budget)
    {
        this.g = g; this.cls = cls; routine = r; queue = r.items.ToList();
        _ci = g != null ? g.GetComponent<CubeInsect>() : null; _a = g != null ? g.GetComponent<Anima>() : null;
        float rest = 0f, timed = 0f; int halves = 0, main = 0;
        foreach (Prescription x in queue)
        {
            if (x.Timed) { timed += TimedSeconds(x); continue; }
            main++; halves += x.sets * x.reps * 2; rest += (x.sets - 1) * RestSeconds(x);
        }
        half = halves > 0 ? Mathf.Clamp((budget - rest - timed - main * 6f) / halves, 0.3f, 1.2f) : 1f;
        phase = Phase.Warmup; _phaseAt = Time.time;
        Note($"empieza «{r.name}» ({cls.tradition.place}): {string.Join(" → ", r.Main.Select(x => $"{x.drill.name} {x.sets}×{x.reps}"))}");
    }

    public static float RestSeconds(Prescription x) => Mathf.Clamp(x.restSec / 60f * MinuteSecs, 2f, 10f);
    public static float TimedSeconds(Prescription x) => Mathf.Clamp(x.minutes * MinuteSecs, 5f, 15f);

    void Note(string s) { string l = $"[Entreno micro] {Who} · {s}"; Debug.Log(l); Recent.Add(l); if (Recent.Count > 40) Recent.RemoveAt(0); }
    void Ev(Kind k, Prescription x) => events.Add(new Event { kind = k, drill = x != null ? x.drill.name : "", set = set, sets = x != null ? TotalSets(x) : 0, reps = rep, time = Time.time, spot = spot });

    /// <summary>Un fotograma; false = ya terminó.</summary>
    public bool Tick()
    {
        if (g == null || Done) return false;
        Prescription x = Current;
        if (x == null) { Finish(); return false; }
        HistoricCoach coach = cls.coach;
        switch (phase)
        {
            case Phase.Warmup:
                if (x.part == "calentamiento" && coach != null && coach.g != g && coach.ClassRunning)
                {
                    if (!_joined) { _joined = true; Ev(Kind.Class, x); Note($"calienta en la clase de {coach.g.name} ({cls.tradition.trainer})"); }
                    coach.Attend(this); return true;
                }
                if (_joined) { Ev(Kind.Done, x); Next(); return true; }
                phase = Phase.Fetch; _walkSince = Time.time; return true;
            case Phase.Fetch:   // el equipo del armario (halteres, mazas, la girya…): ir a por él, como mucho 15 s
                {
                    AreaFunctionStation eq = cls.Equipment;
                    bool rack = eq != null && x.drill.equipment != null && HistoricTraining.KeysOf(x.drill).Contains("armero");
                    if (!rack || Time.time - _walkSince > 15f || MicroMove.Step(g.transform, eq.transform.position, speed, 1.6f))
                    { if (rack) Note($"coge {x.drill.equipment} del armero"); phase = Phase.Walk; _walkSince = Time.time; }
                    return true;
                }
            case Phase.Walk:
                if (spot < 0)
                {
                    spot = cls.Claim(this);
                    if (spot < 0) { phase = Phase.Wait; _waitSince = Time.time; Ev(Kind.Wait, x); return true; }
                }
                if (!MicroMove.Step(g.transform, cls.SpotPos(spot), speed, 0.35f) && Time.time - _walkSince < WalkTimeout) return true;
                if (Time.time - _walkSince >= WalkTimeout) g.transform.position = cls.SpotPos(spot);   // (acotado: no se queda dando vueltas)
                Ev(Kind.Spot, x);
                set = 1; rep = 0; _b = false; _phaseAt = Time.time; phase = Phase.Set;
                if (x.Timed) _until = Time.time + TimedSeconds(x);
                return true;
            case Phase.Wait:
                if (cls.HasFreeFor(x.drill)) { phase = Phase.Walk; _walkSince = Time.time; return true; }
                if (Time.time - _waitSince < MaxWait) return true;
                if (!x.Timed && !_deferred.Contains(x) && queue.Skip(idx + 1).Any(q => !q.Timed))
                {
                    _deferred.Add(x); queue.RemoveAt(idx);
                    int last = queue.FindLastIndex(q => !q.Timed); queue.Insert(last + 1, x);
                    Ev(Kind.Defer, x); Note($"{x.drill.name}: no hay sitio, lo deja para el final");
                }
                else { Ev(Kind.Skip, x); if (!x.Timed) cls.plans[Who].missed.Add(x.drill.id); Note($"{x.drill.name}: sin sitio, lo salta"); idx++; }
                phase = Phase.Walk; _walkSince = Time.time; return true;
            case Phase.Set:
                {
                    float h = x.Timed ? 0.6f : half;
                    if (Time.time - _phaseAt >= h) { _b = !_b; _phaseAt = Time.time; if (!_b && !x.Timed) rep++; }
                    Animate(x.drill.motion, _b, Mathf.Clamp01((Time.time - _phaseAt) / h));
                    if (x.Timed) { if (Time.time >= _until) { Ev(Kind.Done, x); Note($"{x.part}: {x.drill.name} {x.minutes:0} min"); Next(); } return true; }
                    if (rep < x.reps) return true;
                    Ev(Kind.Set, x);
                    Note($"{routine.name} · {x.drill.name} · serie {set}/{x.sets} · {rep} reps · {x.LoadText}");
                    if (set < x.sets) { phase = Phase.Rest; _until = Time.time + RestSeconds(x); Ev(Kind.Rest, x); }
                    else { Ev(Kind.Done, x); Next(); }
                    return true;
                }
            case Phase.Rest:
                Animate(DrillMotion.Sostener, false, 0f);
                if (Time.time >= _until) { set++; rep = 0; _b = false; _phaseAt = Time.time; phase = Phase.Set; }
                return true;
        }
        return true;
    }

    /// <summary>La postura del ejercicio: A (<paramref name="b"/> = false) o B, con <paramref name="k"/> de 0 a 1 en la transición.</summary>
    public void Animate(DrillMotion m, bool b, float k) => AnimateBody(_ci, _a, m, b, k);

    /// <summary>Lo mismo para cualquier cuerpo (el maestro en la clase).</summary>
    public static void AnimateBody(CubeInsect _ci, Anima _a, DrillMotion m, bool b, float k)
    {
        float t = b ? Mathf.SmoothStep(0f, 1f, k) : 1f - Mathf.SmoothStep(0f, 1f, k);
        if (_ci != null)
        {
            switch (m)
            {
                case DrillMotion.Levantar: _ci.Martial(0.2f, 0.2f, t, 0f, 0.5f); break;
                case DrillMotion.Empujar: _ci.Martial(t, t, 0.5f, 0f, t); break;
                case DrillMotion.Balancear: _ci.Martial(0.5f, 0.5f, 0.3f + 0.6f * t, t * 2f - 1f, 0.4f); break;
                case DrillMotion.Luchar: _ci.Martial(t, 1f - t, 0.4f, 0f, 1f); break;
                case DrillMotion.Correr: case DrillMotion.Saltar: _ci.Push(t); break;
                case DrillMotion.Lanzar: _ci.Martial(0.1f, t, 0.5f + 0.5f * t, 0f, 0.3f); break;
                case DrillMotion.Golpear: _ci.Martial(t, 1f - t, 0.6f, Mathf.Sin(Time.time) * 0.3f, 0.3f); break;
                default: _ci.Martial(0.3f, 0.3f, 0.5f, 0f, 0.3f); break;
            }
            return;
        }
        if (_a == null) return;
        Vector3 arm = m == DrillMotion.Levantar || m == DrillMotion.Lanzar ? new Vector3(-170f * t, 0f, 0f) : m == DrillMotion.Sostener ? new Vector3(-90f, 0f, 0f) : new Vector3(-90f * t, 0f, 0f);
        float thigh = m == DrillMotion.Saltar || m == DrillMotion.Sostener || m == DrillMotion.Luchar ? -70f * (m == DrillMotion.Sostener ? 1f : t) : m == DrillMotion.Correr ? -40f * t : 0f;
        DailyPoses.Make(m.ToString(), -1f, (BodyPart.ShoulderLeft, arm), (BodyPart.ShoulderRight, arm),
            (BodyPart.ThighLeft, new Vector3(thigh, 0f, 0f)), (BodyPart.ThighRight, new Vector3(m == DrillMotion.Correr ? 0f : thigh, 0f, 0f)),
            (BodyPart.ShinLeft, new Vector3(-thigh, 0f, 0f)), (BodyPart.ShinRight, new Vector3(m == DrillMotion.Correr ? 0f : -thigh, 0f, 0f))).ApplyFinal(_a);
    }

    void Next()
    {
        cls.Release(this);
        idx++; set = 0; rep = 0;
        if (idx >= queue.Count) { Finish(); return; }
        phase = Phase.Fetch; _walkSince = Time.time;
    }

    void Finish() { cls.Release(this); if (Done) return; phase = Phase.Done; Ev(Kind.Done, null); Note($"termina «{routine.name}»: {events.Count(e => e.kind == Kind.Set)} series"); }

    public void Pause() { if (Done) return; cls.Release(this); if (phase != Phase.Warmup) { phase = Phase.Walk; rep = 0; } _walkSince = Time.time; }

    public void Stop(string why)
    {
        if (Done) return;
        stopReason = why;
        for (int i = idx; i < queue.Count; i++) if (!queue[i].Timed) cls.plans[Who].missed.Add(queue[i].drill.id);
        Ev(Kind.Stop, Current); Note($"{why}: deja {queue.Skip(idx).Count(q => !q.Timed)} ejercicios");
        cls.Release(this); phase = Phase.Done;
    }

    public string Status()
    {
        Prescription x = Current;
        if (Done) return "Entrenamiento terminado";
        if (x == null) return "Entrenando";
        return phase == Phase.Set && !x.Timed ? $"{x.drill.name} · serie {set}/{x.sets} · rep {rep}/{x.reps}" : phase == Phase.Rest ? $"{x.drill.name} · descansa" : phase == Phase.Warmup ? "Calentamiento" : $"{x.part}: {x.drill.name}";
    }
}

/// <summary>EL MAESTRO DEL PUEBLO (paidotribes, guru, shifu, maestro de armas, Turnwart…): clase de calentamiento y rondas de correcciones.</summary>
public class HistoricCoach
{
    public readonly GameObject g; readonly HistoricClass _cls;
    public float speed = 2.4f;
    public int classReps, corrections, attended;
    public readonly List<string> notes = new List<string>();
    float _classUntil, _phaseAt, _atSince = -1f, _targetSince; bool _inClass = true; int _key = -1;
    HistoricSession _target; readonly Dictionary<HistoricSession, float> _visited = new Dictionary<HistoricSession, float>();
    readonly List<HistoricSession> _att = new List<HistoricSession>();
    readonly CubeInsect _ci; readonly Anima _a;

    public HistoricCoach(GameObject g, HistoricClass cls, float classSec)
    {
        this.g = g; _cls = cls; _classUntil = Time.time + classSec; _phaseAt = Time.time; _ci = g.GetComponent<CubeInsect>(); _a = g.GetComponent<Anima>();
        Note($"abre la clase de calentamiento de {cls.tradition.place}");
    }

    public bool ClassRunning => _inClass && Time.time < _classUntil;
    void Note(string s) { string l = $"[Entreno micro] {g.name} ({_cls.tradition.trainer}) {s}"; notes.Add(l); Debug.Log(l); HistoricSession.Recent.Add(l); }

    DrillMotion ClassMotion => HistoricTraining.D(_cls.tradition.warmup.FirstOrDefault() ?? "carrera_suave")?.motion ?? DrillMotion.Correr;
    bool Beat(out float k) { float t = Time.time - _phaseAt; int n = Mathf.FloorToInt(t / 0.7f); k = (t % 0.7f) / 0.7f; return n % 2 == 1; }

    public void Attend(HistoricSession s)
    {
        int i = _att.IndexOf(s); if (i < 0) { _att.Add(s); i = _att.Count - 1; attended++; }
        Vector3 to = g.transform.position + g.transform.forward * (2f + 2f * (i / 4)) + g.transform.right * (((i % 4) - 1.5f) * 2f);   // filas de 4, a 2 m
        if (!MicroMove.Step(s.g.transform, to, s.speed, 0.4f)) return;
        Vector3 look = g.transform.position - s.g.transform.position; look.y = 0f;
        if (look.sqrMagnitude > 0.01f) s.g.transform.rotation = Quaternion.LookRotation(look);
        bool b = Beat(out float k); s.Animate(ClassMotion, b, k);
    }

    public void Tick(IEnumerable<HistoricSession> sessions)
    {
        if (_inClass)
        {
            if (Time.time >= _classUntil) { _inClass = false; Note($"termina la clase: {_att.Count} alumnos, {classReps} repeticiones"); return; }
            bool b = Beat(out float k); int key = b ? 1 : 0;
            if (key != _key) { if (_key == 1 && key == 0) classReps++; _key = key; }
            HistoricSession.AnimateBody(_ci, _a, ClassMotion, b, k);
            return;
        }
        if (_target == null || _target.Done || _target.phase != HistoricSession.Phase.Set && _target.phase != HistoricSession.Phase.Rest && _atSince < 0f)
        {
            _target = sessions.Where(s => s != null && !s.Done && s.g != g && (s.phase == HistoricSession.Phase.Set || s.phase == HistoricSession.Phase.Rest) && s.Current != null && !s.Current.Timed
                                     && (!_visited.TryGetValue(s, out float at) || Time.time - at > 20f))
                               .OrderBy(s => (s.g.transform.position - g.transform.position).sqrMagnitude).FirstOrDefault();
            _atSince = -1f; _targetSince = Time.time;
            if (_target == null) return;
            _visited[_target] = Time.time;
        }
        Vector3 to = _target.g.transform.position + _target.g.transform.right * 1.2f;
        if (_atSince < 0f)
        {
            if (!MicroMove.Step(g.transform, to, speed, 0.6f)) { if (Time.time - _targetSince > 20f) _target = null; return; }
            _atSince = Time.time; corrections++;
            string cue = _cls.tradition.cues.Length > 0 ? _cls.tradition.cues[corrections % _cls.tradition.cues.Length] : "más despacio";
            Note($"corrige a {_target.Who} en {_target.Current?.drill.name}: «{cue}»");
        }
        Vector3 look = _target.g.transform.position - g.transform.position; look.y = 0f;
        if (look.sqrMagnitude > 0.01f) g.transform.rotation = Quaternion.LookRotation(look);
        if (Time.time - _atSince > 4f) { _target = null; _atSince = -1f; }
    }
}

/// <summary>
/// LA CLASE DEL DÍA en un edificio de entrenamiento de un pueblo: la tradición (por su estilo y año), los SITIOS del suelo de entrenar
/// (cada puesto grande —tatami, pista, foso— da una rejilla de sitios a 2 m; si no hay, un círculo alrededor del edificio), el
/// armario del equipo, el maestro y las sesiones. <see cref="Drive"/> lo llama la jornada del insecto-humano en el bloque de la práctica.
/// </summary>
public class HistoricClass
{
    public readonly AreaFunctionSite site; public readonly HistoricTraining.Tradition tradition; public readonly int year, day; public readonly string style;
    public HistoricCoach coach;
    public readonly Dictionary<GameObject, HistoricSession> sessions = new Dictionary<GameObject, HistoricSession>();
    public readonly Dictionary<string, HistoricTraining.Plan> plans = new Dictionary<string, HistoricTraining.Plan>();
    readonly List<Vector3> _spots = new List<Vector3>(); readonly List<string> _keys = new List<string>(); readonly List<HistoricSession> _occ = new List<HistoricSession>();
    public AreaFunctionStation Equipment { get; private set; }
    public int SpotCount => _spots.Count;
    public string SpotKey(int i) => i >= 0 && i < _keys.Count ? _keys[i] : null;

    /// <summary>Los SITIOS de entrenar: los puestos de suelo (tatami, foso, pista, campo, sala de esgrima) dan una rejilla de sitios a 2 m
    /// (la distancia de dos personas, regla del pueblo); cada aparato (halteres, mazas, saco, barra fija, diana…), un sitio delante. Si
    /// el edificio no tiene puestos, 8 sitios en círculo.</summary>
    public HistoricClass(AreaFunctionSite site, HistoricTraining.Tradition t, int year, string style, int day)
    {
        this.site = site; tradition = t; this.year = year; this.style = style; this.day = day;
        foreach (AreaFunctionStation st in site.GetComponentsInChildren<AreaFunctionStation>(true))
        {
            BoxCollider bc = st.GetComponent<BoxCollider>();
            Vector3 size = bc != null ? Vector3.Scale(bc.size, st.transform.lossyScale) : Vector3.zero;
            bool floor = System.Array.IndexOf(HistoricTraining.FloorKeys, st.station) >= 0 || (Mathf.Abs(size.x) >= 1.5f && Mathf.Abs(size.z) >= 1.5f);
            if (floor && bc != null)
            {
                int nx = Mathf.Max(1, Mathf.FloorToInt(Mathf.Abs(size.x) / 2f)), nz = Mathf.Max(1, Mathf.FloorToInt(Mathf.Abs(size.z) / 2f));
                for (int i = 0; i < nx; i++) for (int j = 0; j < nz; j++)
                {
                    Vector3 local = bc.center + new Vector3((i + 0.5f) / nx - 0.5f, 0f, (j + 0.5f) / nz - 0.5f).Scale3(bc.size);
                    Vector3 p = st.transform.TransformPoint(local); p.y = bc.bounds.max.y; _spots.Add(p); _keys.Add(st.station);
                }
            }
            else
            {
                _spots.Add(MicroJornada.WorkSpotOf(st, site.transform.position)); _keys.Add(st.station);
                if (st.station == "armero" && Equipment == null) Equipment = st;
            }
        }
        if (_spots.Count == 0) for (int i = 0; i < 8; i++) { _spots.Add(site.transform.position + Quaternion.Euler(0f, i * 45f, 0f) * Vector3.forward * 3f); _keys.Add("tatami"); }
        for (int i = 0; i < _spots.Count; i++) _occ.Add(null);
    }

    public Vector3 SpotPos(int i) => _spots[Mathf.Clamp(i, 0, _spots.Count - 1)];
    public HistoricSession OccupantOf(int i) => i >= 0 && i < _occ.Count ? _occ[i] : null;

    /// <summary>Los sitios que valen para <paramref name="d"/>: los de sus claves (<see cref="HistoricTraining.KeysOf"/>); si el edificio
    /// no tiene ninguno de esos aparatos, cualquier sitio de suelo; y si tampoco, cualquiera.</summary>
    List<int> Candidates(HistoricTraining.Drill d)
    {
        string[] keys = d != null ? HistoricTraining.KeysOf(d) : HistoricTraining.FloorKeys;
        var l = Enumerable.Range(0, _spots.Count).Where(i => keys.Contains(_keys[i])).ToList();
        if (l.Count == 0) l = Enumerable.Range(0, _spots.Count).Where(i => HistoricTraining.FloorKeys.Contains(_keys[i])).ToList();
        if (l.Count == 0) l = Enumerable.Range(0, _spots.Count).ToList();
        return l;
    }

    public bool HasFreeFor(HistoricTraining.Drill d) => Candidates(d).Any(i => _occ[i] == null);

    /// <summary>Reserva para <paramref name="s"/> el sitio libre más cercano de su ejercicio actual (−1 si todos están ocupados).</summary>
    public int Claim(HistoricSession s)
    {
        int best = -1; float bd = float.MaxValue;
        foreach (int i in Candidates(s.Current?.drill))
        {
            if (_occ[i] != null && _occ[i] != s) continue;
            float d = (_spots[i] - s.g.transform.position).sqrMagnitude; if (d < bd) { bd = d; best = i; }
        }
        if (best >= 0) { _occ[best] = s; s.spot = best; }
        return best;
    }
    public void Release(HistoricSession s) { if (s.spot >= 0 && s.spot < _occ.Count && _occ[s.spot] == s) _occ[s.spot] = null; s.spot = -1; }
    /// <summary>Ocupa un sitio a propósito (las pruebas: «todo lleno»).</summary>
    public void Block(int i, HistoricSession dummy) { if (i >= 0 && i < _occ.Count) _occ[i] = dummy; }
    public void Unblock(int i) { if (i >= 0 && i < _occ.Count) _occ[i] = null; }

    public HistoricTraining.Plan PlanOf(string who, string role) { if (!plans.TryGetValue(who, out var p)) plans[who] = p = HistoricTraining.PlanFor(who, tradition, year, style, role, day); return p; }

    /// <summary>Empieza (o devuelve) la sesión de <paramref name="g"/>; <paramref name="budget"/> s que quedan.</summary>
    public HistoricSession SessionOf(GameObject g, string role, float budget)
    {
        if (sessions.TryGetValue(g, out HistoricSession s)) return s;
        HistoricTraining.Plan p = PlanOf(g.name, role);
        if (p == null) return null;
        sessions[g] = s = new HistoricSession(g, this, HistoricTraining.Prescribe(p, day), budget);
        return s;
    }

    // ── El registro por edificio y la llamada desde la jornada del pueblo ─────────────────────────
    static readonly Dictionary<AreaFunctionSite, HistoricClass> _classes = new Dictionary<AreaFunctionSite, HistoricClass>();
    public static HistoricClass Of(AreaFunctionSite s) => s != null && _classes.TryGetValue(s, out HistoricClass c) ? c : null;

    /// <summary>EN EL BLOQUE DE LA PRÁCTICA (14–16 h): si su sitio de práctica es un edificio de entrenamiento de época, lo lleva por
    /// su rutina (o, si es el maestro, por la clase y las correcciones). false = no (la jornada sigue como siempre).</summary>
    public static bool Drive(MicroJornada j)
    {
        if (j == null || j.follow != null || !HistoricTraining.IsTrainingSite(j.practice)) return false;
        int year = Eras.Now, day = TimeController.timeController != null ? TimeController.timeController.Day : 0;
        if (!_classes.TryGetValue(j.practice, out HistoricClass c) || c.day != day)
        {
            HistoricTraining.Tradition t = HistoricTraining.ForSite(j.practice, year);
            if (t == null) return false;
            if (c != null) c.EndAll("otro día");
            _classes[j.practice] = c = new HistoricClass(j.practice, t, year, HistoricTraining.KindOf(j.practice, year), day);
            // EL MAESTRO: quien trabaja en este edificio; si nadie, el primero por nombre de los que practican aquí.
            MicroJornada[] here = Object.FindObjectsByType<MicroJornada>(FindObjectsSortMode.None).Where(x => x != null && x.practice == j.practice && x.follow == null).OrderBy(x => x.name, System.StringComparer.Ordinal).ToArray();
            MicroJornada m = here.FirstOrDefault(x => x.work == j.practice) ?? here.FirstOrDefault();
            float ms = TimeController.timeController != null ? TimeController.timeController.TimeSpeedMinuteSecs : 2.5f;
            if (m != null) c.coach = new HistoricCoach(m.gameObject, c, Mathf.Max(0f, (WorkRoster.StartHour + 6f + 10f / 60f - MicroJornada.Hour) * 60f) * ms);
        }
        if (c.coach != null && c.coach.g == j.gameObject) { c.coach.Tick(c.sessions.Values); return true; }
        float budget = Mathf.Max(30f, (WorkRoster.StartHour + 8f - 5f / 60f - MicroJornada.Hour) * 60f * (TimeController.timeController != null ? TimeController.timeController.TimeSpeedMinuteSecs : 2.5f));
        HistoricSession s = c.SessionOf(j.gameObject, j.role, budget);
        return s != null && s.Tick();
    }

    /// <summary>Fuera del bloque de la práctica: deja su sitio (y lo que no acabó, sin acabar).</summary>
    public static void Leave(MicroJornada j)
    {
        HistoricClass c = j != null ? Of(j.practice) : null;
        if (c != null && c.sessions.TryGetValue(j.gameObject, out HistoricSession s) && !s.Done) s.Stop("se acaba la práctica");
    }

    /// <summary>Lo deja a medias por una necesidad: suelta su sitio y, al volver, repite la serie.</summary>
    public static void Pause(MicroJornada j)
    {
        HistoricClass c = j != null ? Of(j.practice) : null;
        if (c != null && c.sessions.TryGetValue(j.gameObject, out HistoricSession s)) s.Pause();
    }

    public void EndAll(string why) { foreach (HistoricSession s in sessions.Values) s.Stop(why); }

    /// <summary>Lo que hace ahora (el rótulo de la jornada del pueblo).</summary>
    public static string StatusOf(MicroJornada j)
    {
        HistoricClass c = j != null ? Of(j.practice) : null;
        if (c == null) return null;
        if (c.coach != null && c.coach.g == j.gameObject) return c.coach.ClassRunning ? $"{c.tradition.trainer}: clase de calentamiento" : $"{c.tradition.trainer}: corrige";
        return c.sessions.TryGetValue(j.gameObject, out HistoricSession s) ? s.Status() : null;
    }
}

static class Vec3Ext { public static Vector3 Scale3(this Vector3 a, Vector3 b) => Vector3.Scale(a, b); }
