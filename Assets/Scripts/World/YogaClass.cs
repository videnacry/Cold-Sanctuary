using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CLASE DE YOGA EN GRUPO en una SALA de los pisos de yoga (<see cref="YogaHall"/>; torre del Entrenamiento; Berón 2026-10-29 y
/// 2026-11-04). Un MAESTRO (el guía de la clase, o el primero que llega a entrenar) sube a la «Tarima del maestro» del centro de
/// su sala y guía una práctica de <see cref="YogaLibrary"/> paso a paso; cada PRACTICANTE anda (con <see cref="PathMover"/>)
/// hasta SU esterilla —las alfombras-esterilla «Esterilla_N» de esa sala; una persona por esterilla—, se queda en ella mirando
/// al maestro y repite cada postura un instante después de él. En la VERSIÓN TRADICIONAL, antes de ir a su esterilla, cada
/// practicante entra en el KUND de la sala y se sumerge (<see cref="YogaHall.ImmersionSeconds"/>; como en los Theerthakund de Isha
/// antes del Dhyanalinga). El maestro ESPERA a que todos los que están en su esterilla hagan el paso (huesos dentro de la
/// tolerancia, como <see cref="TeachingSession.PoseMatch"/>) antes del siguiente, con un tope para no atascarse nunca. Al acabar
/// todos se ponen de pie y la clase desaparece. Cada sala tiene como mucho una clase (<see cref="On"/>): el Hatha y el Ashtanga
/// pueden darse a la vez, cada uno en su piso.
/// </summary>
public class YogaClass : MonoBehaviour
{
    static readonly List<YogaClass> _active = new List<YogaClass>();
    /// <summary>Las clases que se están dando (una por sala como mucho).</summary>
    public static IReadOnlyList<YogaClass> Active { get { _active.RemoveAll(c => c == null); return _active; } }
    /// <summary>La primera clase en marcha (compatibilidad: antes solo había una sala).</summary>
    public static YogaClass Current { get { foreach (YogaClass c in Active) if (c.Now != Phase.Done) return c; return null; } }
    /// <summary>La clase que se da en la sala <paramref name="h"/> (o null).</summary>
    public static YogaClass On(YogaHall h) { foreach (YogaClass c in Active) if (c.hall == h && c.Now != Phase.Done) return c; return null; }
    /// <summary>La clase en que está <paramref name="a"/> (de maestro o de practicante), o null.</summary>
    public static YogaClass ClassOf(Anima a) { if (a == null) return null; foreach (YogaClass c in Active) if (c.Now != Phase.Done && c.Has(a)) return c; return null; }
    /// <summary>Termina todas las clases (las pruebas, al empezar y al acabar).</summary>
    public static void EndAll() { foreach (YogaClass c in new List<YogaClass>(Active)) if (c != null) c.End(); }

    /// <summary>Las inmersiones en el kund (quién, en qué sala, cuándo): la prueba mira que van antes de la clase tradicional.</summary>
    public static readonly List<(string who, string hall, float at)> Immersions = new List<(string, string, float)>();

    public Anima teacher;
    public YogaHall hall;
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
        // La INMERSIÓN en el kund (versión tradicional): a qué sitio del agua va, si está dentro, cuándo sale y si la hizo.
        public bool immerse, bathing, bathed, bathSkipped; public int bathSpot; public float bathEnd, bathDeadline = -1f, bathedAt = -1f;
    }
    readonly List<Practitioner> _ps = new List<Practitioner>();
    public IReadOnlyList<Practitioner> Practitioners => _ps;

    List<AsanaPoseSpell> _teacherSteps;
    YogaSession _session;
    int _i = -1;
    float _stepStart, _since;
    /// <summary>Hasta cuándo espera a los que vienen (2026-11-01): <see cref="gatherTimeout"/> o, si alguien viene de otro árbol,
    /// su viaje estimado (<see cref="ArbolCampos.TripSeconds"/>, ×1,3 + 20 s; en la tradicional, + el kund). Con todos los
    /// edificios hechos árboles, de Servicios a la sala de yoga hay 35–95 s de nado y vuelo: la clase empezaba sin ellos.</summary>
    float _gatherUntil;
    float GatherEnd => Mathf.Max(_since + gatherTimeout, _gatherUntil);
    bool _teacherIdleWas;

    public int StepIndex => _i;
    public int StepCount => _teacherSteps != null ? _teacherSteps.Count : 0;
    public float StepStart => _stepStart;
    public AsanaPoseSpell CurrentStep => _teacherSteps != null && _i >= 0 && _i < _teacherSteps.Count ? _teacherSteps[_i] : null;
    public bool TeacherOnTarima { get; private set; }
    /// <summary>Cuándo empezó a guiar (fin de la espera), o −1.</summary>
    public float TeachingSince { get; private set; } = -1f;

    static float WalkSpeed => WorkdayDirector.Instance != null ? WorkdayDirector.Instance.walkSpeed : 2.6f;

    // ── La sala ──────────────────────────────────────────────────────────────────────────────────

    static Transform _room;
    /// <summary>El área de la Sala de Yoga (el piso de Hatha; el de Ashtanga es su planta propia).</summary>
    public static Transform Room { get { if (_room == null) { GameObject y = GameObject.Find("YogaRoom_Area"); _room = y != null ? y.transform : null; } return _room; } }

    /// <summary>La tarima del maestro de su sala (con su collider: sin él se le veía de pie DENTRO de la tarima).</summary>
    public Transform Tarima
    {
        get
        {
            Transform t = hall != null ? hall.tarima : null;
            if (t != null && t.GetComponent<Collider>() == null) t.gameObject.AddComponent<BoxCollider>();   // escenas construidas antes del collider
            return t;
        }
    }

    /// <summary>Altura de lo alto de la tarima (mundo).</summary>
    public float TarimaTop => hall != null ? hall.TarimaTop : 0f;

    /// <summary>¿Está <paramref name="g"/> de pie sobre la tarima de esta clase (dentro de su huella y con los pies en lo alto)?</summary>
    public bool IsOnTarima(GameObject g) => OnTarima(hall, g);

    /// <summary>¿Está <paramref name="g"/> de pie sobre la tarima de la sala <paramref name="h"/>?</summary>
    public static bool OnTarima(YogaHall h, GameObject g)
    {
        Transform t = h != null ? h.tarima : null; if (t == null || g == null) return false;
        Vector3 l = t.InverseTransformPoint(g.transform.position);
        return Mathf.Abs(l.x) <= 0.5f && Mathf.Abs(l.z) <= 0.5f && Mathf.Abs(FeetY(g) - h.TarimaTop) < 0.2f;
    }

    /// <summary>Los pies: la base del cuerpo de cubos (o el pivote menos un metro).</summary>
    public static float FeetY(GameObject g)
    {
        Transform body = g.transform.Find(CubeHumanoid.BodyName);
        return body != null ? body.position.y : g.transform.position.y - 1f;
    }

    /// <summary>Las esterillas de su sala.</summary>
    public List<CarpetPad> Mats() => hall != null ? hall.Mats() : new List<CarpetPad>();

    // ── Empezar, unirse, irse ────────────────────────────────────────────────────────────────────

    /// <summary>Empieza una clase de <paramref name="session"/> con <paramref name="teacher"/> de maestro en la sala
    /// <paramref name="hall"/> (si no se da, la de su estilo y la versión de hoy: <see cref="YogaHall.For"/>), o devuelve la que ya
    /// hay en esa sala (o aquella en la que ya está el maestro).</summary>
    public static YogaClass Begin(Anima teacher, string session = "Saludo al Sol", YogaHall hall = null)
    {
        if (teacher == null || YogaLibrary.Find(session) == null) return null;
        YogaClass mine = ClassOf(teacher);
        if (mine != null) return mine;
        if (hall == null) hall = YogaHall.For(session, YogaHall.Today);
        if (hall == null || hall.tarima == null) return null;
        YogaClass there = On(hall);
        if (there != null) return there;
        var go = new GameObject("Clase de yoga");
        go.transform.SetParent(hall.transform, false);
        YogaClass c = go.AddComponent<YogaClass>();
        c.teacher = teacher; c.hall = hall; c.sessionName = session; c._session = YogaLibrary.Find(session);
        c._teacherSteps = OnlyPoses(c._session.Build());
        c._since = Time.time;
        IdleBehavior idle = teacher.GetComponent<IdleBehavior>();
        c._teacherIdleWas = idle != null && idle.enabled; if (idle != null) idle.enabled = false;
        if (teacher.TryGetComponent(out ActionQueue q)) q.ClearOwn();
        _active.Add(c);
        Debug.Log($"[Clase de yoga] «{teacher.name}» guía «{session}» desde la tarima de la sala {hall.Label} ({c.StepCount} pasos{(hall.Immersion ? "; antes, al kund" : "")}).");
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

    /// <summary>Se une como practicante: le toca la primera esterilla libre de su sala. false si no queda ninguna (o ya está en
    /// otra clase).</summary>
    public bool Join(Anima a)
    {
        if (a == null || a == teacher || Now == Phase.Done) return false;
        if (Of(a) != null) return true;
        YogaClass other = ClassOf(a);
        if (other != null && other != this) return false;
        CarpetPad free = null;
        foreach (CarpetPad m in Mats())
        {
            bool taken = false;
            foreach (Practitioner p in _ps) if (p.mat == m) { taken = true; break; }
            if (!taken) { free = m; break; }
        }
        if (free == null) return false;
        IdleBehavior idle = a.GetComponent<IdleBehavior>();
        var pr = new Practitioner { a = a, mat = free, steps = OnlyPoses(_session.Build()), idleWas = idle != null && idle.enabled, immerse = hall != null && hall.Immersion, bathSpot = _ps.Count };
        _gatherUntil = Mathf.Max(_gatherUntil, Time.time + ArbolCampos.TripSeconds(a.transform.position, free.transform.position) * 1.3f + 20f
            + (pr.immerse ? YogaHall.ImmersionSeconds + 30f : 0f));   // (la tradicional: antes, el kund)
        if (idle != null) idle.enabled = false;
        if (a.TryGetComponent(out ActionQueue q)) q.ClearOwn();
        _ps.Add(pr);
        PathMover.Of(a.transform).Restart();
        Debug.Log($"[Clase de yoga] «{a.name}» va a la {free.name} de la sala {hall.Label}{(pr.immerse ? " (antes, al kund)" : "")}.");
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
        Debug.Log($"[Clase de yoga] Termina «{sessionName}» en la sala {(hall != null ? hall.Label : "?")} ({Mathf.Min(_i + 1, StepCount)}/{StepCount} pasos, {_ps.Count} practicantes).");
        _active.Remove(this);
        Destroy(gameObject);
    }

    void OnDestroy() { _active.Remove(this); }

    // ── Cada fotograma ───────────────────────────────────────────────────────────────────────────

    void Update()
    {
        if (Now == Phase.Done) return;
        if (teacher == null || Tarima == null) { End(); return; }
        _ps.RemoveAll(p => p.a == null);

        // El maestro, a la tarima (sube el escalón andando).
        Vector3 top = Tarima.position; top.y = TarimaTop;
        TeacherOnTarima = IsOnTarima(teacher.gameObject);
        if (!TeacherOnTarima && Flat(top - teacher.transform.position) > 0.3f) PathMover.Of(teacher.transform).StepToward(top + Vector3.up, WalkSpeed);
        else if (!TeacherOnTarima) TeacherOnTarima = IsOnTarima(teacher.gameObject);

        // Cada practicante (en la tradicional, antes por el kund) a su esterilla; ya en ella, mirando al maestro.
        int onMats = 0;
        foreach (Practitioner p in _ps)
        {
            Vector3 m = p.mat.transform.position;
            Vector3 d = m - p.a.transform.position;
            bool sameFloor = Mathf.Abs(m.y - FeetY(p.a.gameObject)) < 1f;
            if (!p.onMat && p.immerse && !p.bathed) { Bathe(p); continue; }
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
            if (TeacherOnTarima && (all || (onMats > 0 && Time.time > GatherEnd))) { Now = Phase.Teaching; TeachingSince = Time.time; Next(); }
            else if (Time.time - _since > (GatherEnd - _since) * 2f) End();   // nadie llega: no se queda esperando para siempre
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
            if (!Matches(p.a, step, matchTolerance)) allMatch = false;
        }
        if ((t >= minStep && allMatch) || t > stepTimeout + step.duration * step.reachFraction) Next();
    }

    /// <summary>LA INMERSIÓN (versión tradicional): andar a su sitio en el agua del kund, sumergirse sentado
    /// <see cref="YogaHall.ImmersionSeconds"/> s y salir de pie hacia su esterilla. Acotado: si no llega al kund a tiempo, va a su
    /// esterilla sin bañarse (y lo dice).</summary>
    void Bathe(Practitioner p)
    {
        GameObject g = p.a.gameObject;
        Vector3 k = hall.KundSpot(p.bathSpot);
        if (p.bathDeadline < 0f) p.bathDeadline = Time.time + ArbolCampos.TripSeconds(g.transform.position, k) * 1.3f + 60f;
        if (!p.bathing)
        {
            Vector3 d = k - g.transform.position;
            if (Mathf.Abs(k.y - FeetY(g)) < 1f && Flat(d) < 0.6f)
            {
                p.bathing = true; p.bathEnd = Time.time + YogaHall.ImmersionSeconds;
                Debug.Log($"[Clase de yoga] «{g.name}» entra en el kund de la sala {hall.Label} antes de la práctica.");
            }
            else if (Time.time > p.bathDeadline)
            {
                p.bathed = p.bathSkipped = true;
                Debug.LogWarning($"[Clase de yoga] «{g.name}» no llega al kund de la sala {hall.Label}: va a su esterilla sin sumergirse.");
            }
            else PathMover.Of(g.transform).StepToward(k + Vector3.up, WalkSpeed);
            return;
        }
        if (Time.time < p.bathEnd) { DailyPoses.Meditate().ApplyFinal(p.a); return; }   // sentado en el agua: le cubre hasta el pecho
        DailyPoses.Stand().ApplyFinal(p.a);
        p.bathing = false; p.bathed = true; p.bathedAt = Time.time;
        Immersions.Add((g.name, hall.Label, Time.time));
        PathMover.Of(g.transform).Restart();
        Debug.Log($"[Clase de yoga] «{g.name}» sale del kund y va a su {p.mat.name}.");
    }

    /// <summary>Huesos que la postura lleva por IK (alcance: manos juntas al pecho, al suelo…): su ángulo depende de las
    /// proporciones de cada cuerpo, así que no se comparan con el de la postura.</summary>
    public static bool ByIK(AsanaPoseSpell step, BodyPart part)
    {
        if (step.reaches == null) return false;
        foreach (var rc in step.reaches)
        {
            bool hand = rc.end == BodyPart.HandLeft || rc.end == BodyPart.HandRight, left = rc.end == BodyPart.HandLeft || rc.end == BodyPart.FootLeft;
            if (part == rc.end) return true;
            if (hand && part == (left ? BodyPart.ShoulderLeft : BodyPart.ShoulderRight)) return true;
            if (hand && part == (left ? BodyPart.ForearmLeft : BodyPart.ForearmRight)) return true;
            if (!hand && part == (left ? BodyPart.ThighLeft : BodyPart.ThighRight)) return true;
            if (!hand && part == (left ? BodyPart.ShinLeft : BodyPart.ShinRight)) return true;
        }
        return false;
    }

    /// <summary>¿El cuerpo de <paramref name="a"/> hace la postura del paso? Como <see cref="TeachingSession.PoseMatch"/>, sin los
    /// huesos que van por IK.</summary>
    public static bool Matches(Anima a, AsanaPoseSpell step, float tol)
    {
        if (a == null || step == null) return true;
        CreatureRig rig = a.GetComponent<CreatureRig>(); if (rig == null) return true;
        foreach (var p in step.parts)
        {
            if (ByIK(step, p.Item1)) continue;
            Transform b = rig.Get(p.Item1); if (b == null) continue;
            if (Quaternion.Angle(b.localRotation, Quaternion.Euler(p.Item2)) > tol) return false;
        }
        return true;
    }

    void Next()
    {
        _i++;
        if (_teacherSteps == null || _i >= _teacherSteps.Count) { End(); return; }
        _stepStart = Time.time;
    }

    static float Flat(Vector3 v) { v.y = 0f; return v.magnitude; }
}
