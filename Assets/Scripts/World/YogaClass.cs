using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CLASE DE YOGA EN GRUPO en la Sala de Yoga (torre del Entrenamiento; Berón 2026-10-29). Un MAESTRO (el guía de la clase,
/// o el primero que llega a entrenar yoga) sube a la «Tarima del maestro» del centro y guía una práctica de
/// <see cref="YogaLibrary"/> paso a paso; cada PRACTICANTE anda (con <see cref="PathMover"/>, en alfombra si viene de otro
/// piso) hasta SU esterilla —las alfombras-esterilla «Esterilla_N» de la sala; una persona por esterilla—, se queda en ella
/// mirando al maestro y repite cada postura un instante después de él. El maestro ESPERA a que todos los que están en su
/// esterilla la hagan (huesos dentro de la tolerancia, como <see cref="TeachingSession.PoseMatch"/>) antes del paso
/// siguiente, con un tope para no atascarse nunca. Al acabar todos se ponen de pie y la clase desaparece.
/// La sala solo tiene una clase a la vez (<see cref="Current"/>).
/// </summary>
public class YogaClass : MonoBehaviour
{
    public static YogaClass Current { get; private set; }

    public Anima teacher;
    public string sessionName = "Saludo al Sol";
    [Tooltip("Lo mínimo que dura un paso (s): el maestro lo demuestra aunque todos lo hagan ya.")]
    public float minStep = 4f;
    [Tooltip("Grados de margen para dar una postura por copiada.")]
    public float matchTolerance = 30f;
    [Tooltip("Tope de un paso (s) si alguien no llega a copiarlo.")]
    public float stepTimeout = 20f;
    [Tooltip("Lo que tardan los practicantes en empezar la postura que ven hacer al maestro (s).")]
    public float reaction = 0.4f;
    [Tooltip("Tope de la espera inicial (s): con el maestro en la tarima y al menos uno en su esterilla, empieza aunque falte alguien.")]
    public float gatherTimeout = 120f;

    public enum Phase { Gathering, Teaching, Done }
    public Phase Now { get; private set; } = Phase.Gathering;

    public class Practitioner
    {
        public Anima a; public CarpetPad mat; public bool onMat; public float arrivedAt = -1f;
        public List<AsanaPoseSpell> steps; public bool idleWas;
    }
    readonly List<Practitioner> _ps = new List<Practitioner>();
    public IReadOnlyList<Practitioner> Practitioners => _ps;

    List<AsanaPoseSpell> _teacherSteps;
    YogaSession _session;
    int _i = -1;
    float _stepStart, _since;
    bool _teacherIdleWas;

    public int StepIndex => _i;
    public int StepCount => _teacherSteps != null ? _teacherSteps.Count : 0;
    public float StepStart => _stepStart;
    public AsanaPoseSpell CurrentStep => _teacherSteps != null && _i >= 0 && _i < _teacherSteps.Count ? _teacherSteps[_i] : null;
    public bool TeacherOnTarima { get; private set; }

    static float WalkSpeed => WorkdayDirector.Instance != null ? WorkdayDirector.Instance.walkSpeed : 2.6f;

    // ── La sala ──────────────────────────────────────────────────────────────────────────────────

    static Transform _room, _tarima;
    public static Transform Room { get { if (_room == null) { GameObject y = GameObject.Find("YogaRoom_Area"); _room = y != null ? y.transform : null; } return _room; } }

    /// <summary>La tarima del maestro (con su collider: sin él se le veía de pie DENTRO de la tarima).</summary>
    public static Transform Tarima
    {
        get
        {
            if (_tarima == null && Room != null)
                foreach (Transform t in Room.GetComponentsInChildren<Transform>(true))
                    if (t.name == "Tarima del maestro") { _tarima = t; break; }
            if (_tarima != null && _tarima.GetComponent<Collider>() == null) _tarima.gameObject.AddComponent<BoxCollider>();   // escenas construidas antes del collider
            return _tarima;
        }
    }

    /// <summary>Altura de lo alto de la tarima (mundo).</summary>
    public static float TarimaTop => Tarima != null ? Tarima.position.y + Tarima.lossyScale.y * 0.5f : 0f;

    /// <summary>¿Está <paramref name="g"/> de pie sobre la tarima (dentro de su huella y con los pies en lo alto)?</summary>
    public static bool OnTarima(GameObject g)
    {
        Transform t = Tarima; if (t == null || g == null) return false;
        Vector3 l = t.InverseTransformPoint(g.transform.position);
        return Mathf.Abs(l.x) <= 0.5f && Mathf.Abs(l.z) <= 0.5f && Mathf.Abs(FeetY(g) - TarimaTop) < 0.2f;
    }

    /// <summary>Los pies: la base del cuerpo de cubos (o el pivote menos un metro).</summary>
    public static float FeetY(GameObject g)
    {
        Transform body = g.transform.Find(CubeHumanoid.BodyName);
        return body != null ? body.position.y : g.transform.position.y - 1f;
    }

    /// <summary>Las esterillas de la sala: primero las 6 del suelo (Esterilla_1…6) y después las del anillo.</summary>
    public static List<CarpetPad> Mats()
    {
        var l = new List<CarpetPad>();
        if (Room == null) return l;
        foreach (CarpetPad p in CarpetPad.All)
            if (p != null && p.name.StartsWith("Esterilla_") && p.transform.IsChildOf(Room)) l.Add(p);
        l.Sort((x, y) => Index(x).CompareTo(Index(y)));
        return l;
    }
    static int Index(CarpetPad p) => int.TryParse(p.name.Substring("Esterilla_".Length), out int n) ? n : 999;

    // ── Empezar, unirse, irse ────────────────────────────────────────────────────────────────────

    /// <summary>Empieza una clase con <paramref name="teacher"/> de maestro (o devuelve la que ya hay).</summary>
    public static YogaClass Begin(Anima teacher, string session = "Saludo al Sol")
    {
        if (Current != null) return Current;
        if (teacher == null || YogaLibrary.Find(session) == null || Room == null) return null;
        var go = new GameObject("Clase de yoga");
        go.transform.SetParent(Room, false);
        YogaClass c = go.AddComponent<YogaClass>();
        c.teacher = teacher; c.sessionName = session; c._session = YogaLibrary.Find(session);
        c._teacherSteps = OnlyPoses(c._session.Build());
        c._since = Time.time;
        IdleBehavior idle = teacher.GetComponent<IdleBehavior>();
        c._teacherIdleWas = idle != null && idle.enabled; if (idle != null) idle.enabled = false;
        if (teacher.TryGetComponent(out ActionQueue q)) q.ClearOwn();
        Current = c;
        Debug.Log($"[Clase de yoga] «{teacher.name}» guía «{session}» desde la tarima ({c.StepCount} pasos).");
        return c;
    }

    static List<AsanaPoseSpell> OnlyPoses(List<QueuedSpell> l)
    {
        var r = new List<AsanaPoseSpell>();
        if (l != null) foreach (QueuedSpell s in l) if (s is AsanaPoseSpell a) r.Add(a);
        return r;
    }

    public bool Has(Anima a) => a != null && (a == teacher || Of(a) != null);
    public Practitioner Of(Anima a) { foreach (Practitioner p in _ps) if (p.a == a) return p; return null; }
    public CarpetPad MatOf(Anima a) => Of(a)?.mat;

    /// <summary>Se une como practicante: le toca la primera esterilla libre. false si no queda ninguna.</summary>
    public bool Join(Anima a)
    {
        if (a == null || a == teacher || Now == Phase.Done) return false;
        if (Of(a) != null) return true;
        CarpetPad free = null;
        foreach (CarpetPad m in Mats())
        {
            bool taken = false;
            foreach (Practitioner p in _ps) if (p.mat == m) { taken = true; break; }
            if (!taken) { free = m; break; }
        }
        if (free == null) return false;
        IdleBehavior idle = a.GetComponent<IdleBehavior>();
        var pr = new Practitioner { a = a, mat = free, steps = OnlyPoses(_session.Build()), idleWas = idle != null && idle.enabled };
        if (idle != null) idle.enabled = false;
        if (a.TryGetComponent(out ActionQueue q)) q.ClearOwn();
        _ps.Add(pr);
        PathMover.Of(a.transform).Restart();
        Debug.Log($"[Clase de yoga] «{a.name}» va a la {free.name}.");
        return true;
    }

    /// <summary>Deja la clase (se pone de pie y vuelve a su conducta libre).</summary>
    public void Leave(Anima a)
    {
        if (a == null) return;
        if (a == teacher) { End(); return; }
        Practitioner p = Of(a); if (p == null) return;
        _ps.Remove(p);
        StandUp(a, p.idleWas);
    }

    static void StandUp(Anima a, bool idleWas)
    {
        if (a == null) return;
        DailyPoses.Stand().ApplyFinal(a);
        IdleBehavior idle = a.GetComponent<IdleBehavior>();
        if (idle != null && idleWas) idle.enabled = true;
    }

    /// <summary>Termina la clase: todos de pie.</summary>
    public void End()
    {
        if (Now == Phase.Done) return;
        Now = Phase.Done;
        foreach (Practitioner p in _ps) StandUp(p.a, p.idleWas);
        StandUp(teacher, _teacherIdleWas);
        Debug.Log($"[Clase de yoga] Termina «{sessionName}» ({Mathf.Min(_i + 1, StepCount)}/{StepCount} pasos, {_ps.Count} practicantes).");
        if (Current == this) Current = null;
        Destroy(gameObject);
    }

    void OnDestroy() { if (Current == this) Current = null; }

    // ── Cada fotograma ───────────────────────────────────────────────────────────────────────────

    void Update()
    {
        if (Now == Phase.Done) return;
        if (teacher == null || Tarima == null) { End(); return; }
        _ps.RemoveAll(p => p.a == null);

        // El maestro, a la tarima (sube el escalón andando).
        Vector3 top = Tarima.position; top.y = TarimaTop;
        TeacherOnTarima = OnTarima(teacher.gameObject);
        if (!TeacherOnTarima && Flat(top - teacher.transform.position) > 0.3f) PathMover.Of(teacher.transform).StepToward(top + Vector3.up, WalkSpeed);
        else if (!TeacherOnTarima) TeacherOnTarima = OnTarima(teacher.gameObject);

        // Cada practicante, a su esterilla; ya en ella, mirando al maestro.
        int onMats = 0;
        foreach (Practitioner p in _ps)
        {
            Vector3 m = p.mat.transform.position;
            Vector3 d = m - p.a.transform.position;
            bool sameFloor = Mathf.Abs(m.y - FeetY(p.a.gameObject)) < 1f;
            if (!p.onMat)
            {
                if (sameFloor && Flat(d) < 0.5f) { p.onMat = true; p.arrivedAt = Time.time; }
                else { PathMover.Of(p.a.transform).StepToward(m + Vector3.up, WalkSpeed); continue; }
            }
            Vector3 face = teacher.transform.position - p.a.transform.position; face.y = 0f;
            if (face.sqrMagnitude > 0.01f) p.a.transform.rotation = Quaternion.LookRotation(face);
            onMats++;
        }

        if (Now == Phase.Gathering)
        {
            bool all = _ps.Count > 0 && onMats == _ps.Count;
            if (TeacherOnTarima && (all || (onMats > 0 && Time.time - _since > gatherTimeout))) { Now = Phase.Teaching; Next(); }
            else if (Time.time - _since > gatherTimeout * 2f) End();   // nadie llega: no se queda esperando para siempre
            return;
        }

        // ── Guiando: el maestro demuestra el paso; los de su esterilla lo repiten un instante después ──
        AsanaPoseSpell step = CurrentStep;
        if (step == null) { End(); return; }
        float t = Time.time - _stepStart;
        if (TeacherOnTarima) step.Tick(teacher);
        bool allMatch = true;
        foreach (Practitioner p in _ps)
        {
            if (!p.onMat) { allMatch = false; continue; }
            if (t >= reaction && _i < p.steps.Count) p.steps[_i].Tick(p.a);
            if (!TeachingSession.PoseMatch(p.a, step, matchTolerance)) allMatch = false;
        }
        if ((t >= minStep && allMatch) || t > stepTimeout + step.duration * step.reachFraction) Next();
    }

    void Next()
    {
        _i++;
        if (_teacherSteps == null || _i >= _teacherSteps.Count) { End(); return; }
        _stepStart = Time.time;
    }

    static float Flat(Vector3 v) { v.y = 0f; return v.magnitude; }
}
