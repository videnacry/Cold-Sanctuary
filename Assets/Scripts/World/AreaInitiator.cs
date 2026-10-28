using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// INICIACIÓN DE ÁREA (Berón 2026-10-28): tras el examen de llegada, la Magnate ya NO da ella las iniciaciones; solo LLEVA
/// al recién llegado a cada área y allí lo inicia EL TRABAJADOR DE TURNO de esa área, como en la enfermería
/// (<see cref="NewcomerInitiator"/>): el jugador queda OBLIGADO a hacer cada paso (se le pide; si no lo hace a tiempo, el
/// trabajador lo hace con él para que nunca se atasque) y al acabar el trabajador lo lleva al TANQUE de su área y lo mete
/// en su primer microcosmos. Un solo hechizo compuesto con dos variantes:
/// - <see cref="Kind.Yoga"/> — la sala de yoga: su primera clase, upa-yoga de cuello (<see cref="TeachingSession"/> con el
///   trabajador de maestro; el jugador lanza cada asana) → tanque → el RÍO de Sakshi (<see cref="Magnate.RiverMicrocosmos"/>).
/// - <see cref="Kind.Kitchen"/> — la cocina: cocinan JUNTOS el platillo con MENOS stock del restaurante, POR MAYOR (para que
///   coma todo el personal: las cantidades se multiplican y salen <see cref="StaffServings"/> raciones de una tanda; las
///   individuales son para los turistas), el jugador lanza cada paso (`step`) → stock +N → tanque → el primer microcosmos de
///   la cocina, la era del fuego: las reencarnaciones (<see cref="KitchenMicrocosmos"/>, docs/microcosmos-areas.md).
/// Todas las esperas tienen tope (docs: no stalls): cada tramo andando, cada paso y la iniciación entera.
/// </summary>
public class AreaInitiator : MonoBehaviour
{
    public enum Kind { Yoga, Kitchen }
    public enum Phase { None, Approach, Escort, Lesson, Cook, ToTank, Done }

    public const string YogaArea = "Sala de Yoga", KitchenArea = "Cocina";
    /// <summary>El primer microcosmos de la cocina (la era del fuego: las reencarnaciones; AreaMicrocosmos.KitchenStory).</summary>
    public const string KitchenMicrocosmos = "Microcosmos_Cocina";

    public Kind kind;
    public SanctuaryOnboarding student;
    public float moveSpeed = 3.2f;
    [Tooltip("Segundos que el trabajador espera a que el jugador lance cada paso de cocina; después lo hace con él.")]
    public float stepTimeout = 30f;
    [Tooltip("Segundos por paso cuando el alumno es la IA (lo hace solo).")]
    public float aiStepSeconds = 2.5f;
    [Tooltip("Tope de toda la iniciación (s de juego): pase lo que pase, termina.")]
    public float maxSeconds = 900f;

    /// <summary>Con las pruebas corriendo, la iniciación espera (como la de la enfermería); el test de la cocina la deja actuar.</summary>
    public static bool AllowDuringTests;

    public Phase Now { get; private set; }
    /// <summary>¿Está esperando a que el alumno lance el paso actual? (el test emula al jugador con esto)</summary>
    public bool WaitingForStudent => Now == Phase.Cook && _promptedIdx >= 0 && !_cookDone;

    /// <summary>Lo que dejó la última iniciación de área (para los tests y el log).</summary>
    public class Report
    {
        public Kind kind; public string worker, student, dish, scene; public int servings, stockBefore, stockAfter, stepsByStudent, stepsHelped;
        public bool lessonDone, cooked, enteredMicrocosmos;
    }
    public static Report Last { get; private set; }
    Report _r;

    static readonly List<AreaInitiator> _all = new List<AreaInitiator>();
    float _phaseDeadline, _endAt, _stepDeadline, _nextFrame;
    bool _held, _cookDone, _addedStation, _frameB;
    int _promptedIdx = -1, _seenIdx;
    CookingRecipe _recipe;
    CookingStation _cs;
    RestaurantKitchen _rk;
    KitchenCook _ownCook;
    bool _ownCookWasOn;

    // ── API ───────────────────────────────────────────────────────────────────────────────────────────────────────
    /// <summary>Empieza la iniciación de <paramref name="k"/> para <paramref name="o"/>: la da el trabajador de turno del área
    /// (si no hay, quien tenga misión allí hoy; en la cocina, su jefe; y si no, <paramref name="fallback"/>, la Magnate).</summary>
    public static AreaInitiator Begin(Kind k, SanctuaryOnboarding o, Anima fallback)
    {
        if (o == null) return null;
        AreaInitiator cur = Of(o);
        if (cur != null) return cur;
        GameObject w = PickWorker(k, fallback);
        if (w == null) return null;
        AreaInitiator ai = w.AddComponent<AreaInitiator>();
        ai.kind = k; ai.student = o;
        return ai;
    }

    public static AreaInitiator Of(SanctuaryOnboarding o)
    {
        for (int i = _all.Count - 1; i >= 0; i--) if (_all[i] != null && _all[i].student == o) return _all[i];
        return null;
    }
    public static bool Owns(SanctuaryOnboarding o) => Of(o) != null;
    /// <summary>¿Está <paramref name="g"/> iniciando a alguien? (la jornada no lo mueve entonces)</summary>
    public static bool Leading(GameObject g)
    {
        if (g == null) return false;
        foreach (AreaInitiator a in _all) if (a != null && a.gameObject == g) return true;
        return false;
    }

    /// <summary>Raciones de una tanda por mayor: una por persona del personal del santuario (que coman todos).</summary>
    public static int StaffServings()
    {
        WorkdayDirector wd = WorkdayDirector.Instance;
        int n = wd != null && wd.staff != null ? wd.staff.Length : 8;
        return Mathf.Clamp(n, 2, 40);
    }

    /// <summary>El trabajador que inicia en el área: el de turno ahora → quien tenga misión allí hoy → (cocina) su jefe → la Magnate.</summary>
    static GameObject PickWorker(Kind k, Anima fallback)
    {
        string area = k == Kind.Yoga ? YogaArea : KitchenArea;
        if (WorkdayDirector.Instance != null)
            foreach (GameObject g in WorkdayDirector.Instance.InArea(area)) if (Usable(g)) return g;
        if (k == Kind.Kitchen)
        {
            KitchenBoss kb = FindAnyObjectByType<KitchenBoss>();
            if (kb != null && Usable(kb.gameObject)) return kb.gameObject;
        }
        return fallback != null ? fallback.gameObject : null;
    }

    // Un cuerpo con Anima (los clones de trabajo son solo cubos) que no esté ya iniciando a otro.
    static bool Usable(GameObject g) =>
        g != null && !g.CompareTag("Player") && g.TryGetComponent(out Anima a) && !a.death && !Leading(g);

    public string Who
    {
        get { Mind m = GetComponent<Mind>(); return m != null && !string.IsNullOrEmpty(m.identity) ? m.identity : name; }
    }

    // ── Ciclo ─────────────────────────────────────────────────────────────────────────────────────────────────────
    void OnEnable() { _all.Add(this); }
    void OnDisable() { _all.Remove(this); }

    void Start()
    {
        _endAt = Time.time + maxSeconds;
        _r = new Report { kind = kind, worker = Who, student = student != null ? student.name : "?" };
        MagnateWarden.Lock(this, true);   // mientras lo inicia, la Magnate no lo devuelve a «su puesto»
        _ownCook = GetComponent<KitchenCook>();
        if (_ownCook != null) { _ownCookWasOn = _ownCook.enabled; _ownCook.enabled = false; }   // deja su puesto un rato: cocina con el nuevo
        Now = Phase.Approach; _phaseDeadline = Time.time + 40f;
        Debug.Log($"[Iniciación {Label}] {Who} (de turno) lanza «iniciar» sobre «{_r.student}».");
    }

    string Label => kind == Kind.Yoga ? "sala de yoga" : "cocina";

    void Update()
    {
        if (ColdSanctuary.TestRunner.Running && !AllowDuringTests) return;
        if (student == null) { Finish("el alumno ya no está"); return; }
        if (Time.time > _endAt) { Finish($"tope de {maxSeconds:0} s: se da por hecha"); return; }
        switch (Now)
        {
            case Phase.Approach: Approach(); break;
            case Phase.Escort: EscortTo(WorkSpot(), BeginWork); break;
            case Phase.Lesson: Lesson(); break;
            case Phase.Cook: Cook(); break;
            case Phase.ToTank: ToTank(); break;
        }
    }

    // 1. Va hacia el recién llegado (la Magnate lo ha dejado en la puerta del área).
    void Approach()
    {
        Vector3 d = student.transform.position - transform.position;
        if (d.magnitude > 2f && Time.time < _phaseDeadline) { Step(student.transform.position); return; }
        if (d.magnitude > 2f) WorkdayDirector.Teleport(transform, student.transform.position - Flat(d) * 1.5f);   // no llegaba andando: se coloca
        Say(kind == Kind.Yoga
            ? "Bienvenido a la sala de yoga. Tu primera clase es obligatoria: upa-yoga de cuello. Ven, te la doy yo."
            : "Bienvenido a la cocina. Aquí cocinamos POR MAYOR para que coma todo el santuario: hoy haces conmigo el platillo que más falta.", 7f);
        Now = Phase.Escort; _phaseDeadline = Time.time + 60f;
    }

    // Andar juntos (el alumno detrás, de la mano) hasta un sitio; con tope: si no llegan, se colocan.
    void EscortTo(Vector3 at, System.Action arrived)
    {
        Transform p = student.transform;
        Vector3 d = at - transform.position; d.y = 0f;
        if (d.magnitude > 1.5f && Time.time < _phaseDeadline) { Hold(true); Escort(p, Time.deltaTime); Step(at); return; }
        if (d.magnitude > 1.5f)
        {
            WorkdayDirector.Teleport(transform, new Vector3(at.x, transform.position.y, at.z));
            WorkdayDirector.Teleport(p, at - Flat(at - p.position) * 1.0f + Vector3.right * 0.8f);
            Debug.Log($"[Iniciación {Label}] {Who} no llegaba andando: se colocan junto al sitio.");
        }
        Hold(false);
        arrived();
    }

    Vector3 WorkSpot()
    {
        if (kind == Kind.Yoga)
        {
            GameObject s = GameObject.Find("SalaYoga");
            return s != null ? s.transform.position : student.transform.position;
        }
        RestaurantKitchen rk = Kitchen();
        Transform t = rk != null ? VirtualInPlace.FindDeep(rk.transform, "Mesa de emplatado") : null;
        if (t == null) { GameObject s = GameObject.Find("CocinaSpot"); t = s != null ? s.transform : null; }
        return t != null ? t.position : student.transform.position;
    }

    RestaurantKitchen Kitchen() => _rk != null ? _rk : (_rk = FindAnyObjectByType<RestaurantKitchen>());

    void BeginWork()
    {
        if (kind == Kind.Yoga) BeginLesson(); else BeginCook();
    }

    // ── YOGA: la primera clase (upa-yoga de cuello), interactiva ─────────────────────────────────────────────────────
    void BeginLesson()
    {
        Anima me = GetComponent<Anima>(), st = student.GetComponentInParent<Anima>();
        if (me == null || st == null) { Now = Phase.ToTank; _phaseDeadline = Time.time + 60f; return; }
        // Si estaba dando otra clase, la deja DE VERDAD (si no, seguía con sus pasos en cola): la iniciación va antes.
        TeachingSession busy = GetComponent<TeachingSession>();
        if (busy != null) { Destroy(busy); if (TryGetComponent(out ActionQueue q)) q.ClearOwn(); DailyPoses.Stand().ApplyFinal(me); }
        TeachingSession.Begin(me, st, "upa-yoga", () => UpaYoga.NeckSequence());
        CharacterLevel cl = st.GetComponent<CharacterLevel>(); if (cl != null) cl.GainYogaXp(10f);
        // Tope de la clase: cada paso ya tiene su piso de seguridad (TeachingSession.stepTimeout); esto por si algo falla.
        _phaseDeadline = Time.time + UpaYoga.NeckSequence().Count * 16f + 30f;
        Now = Phase.Lesson;
        Debug.Log($"[Iniciación sala de yoga] {Who} da a «{st.name}» su primera clase: upa-yoga de cuello (interactiva).");
    }

    void Lesson()
    {
        Anima st = student.GetComponentInParent<Anima>();
        TeachingSession ts = st != null ? TeachingSession.For(st) : null;
        if (ts != null && Time.time < _phaseDeadline) return;
        if (ts != null) { Destroy(ts); Debug.Log($"[Iniciación sala de yoga] la clase pasó de su tope: {Who} la da por hecha."); }
        _r.lessonDone = true;
        Say("Muy bien. Ahora, tu primer viaje desde esta sala: al río. Allí hay alguien que te necesita.", 6f);
        Now = Phase.ToTank; _phaseDeadline = Time.time + 60f;
    }

    // ── COCINA: la tanda POR MAYOR del platillo con menos stock ──────────────────────────────────────────────────────
    void BeginCook()
    {
        RestaurantKitchen rk = Kitchen();
        RestaurantKitchen.DishStock dish = rk != null ? rk.LowestStock() : null;
        if (dish == null) { Debug.Log("[Iniciación cocina] no hay restaurante con platillos: directo al tanque."); Now = Phase.ToTank; _phaseDeadline = Time.time + 60f; return; }
        int n = StaffServings();
        _recipe = dish.recipe.ForServings(n);
        _r.dish = _recipe.dishName; _r.servings = n; _r.stockBefore = dish.Have;
        _cs = student.GetComponent<CookingStation>();
        if (_cs == null)
        {
            _cs = student.gameObject.AddComponent<CookingStation>(); _addedStation = true;
            _cs.kitchen = rk; _cs.playerDriven = student.GetComponent<PlayerController>() != null;
            _cs.externallyDriven = !_cs.playerDriven;   // una IA no avanza sola por tiempo: los pasos los marca la iniciación
        }
        _cs.Assign(_recipe);
        _seenIdx = 0; _promptedIdx = -1; _cookDone = false;
        Now = Phase.Cook;
        Say($"El que más falta es «{_recipe.dishName}» (quedan {dish.Have}). Lo hacemos por mayor: {n} raciones, una por persona del personal. " +
            $"Cantidades: {_recipe.Quantities()}.", 9f);
        Debug.Log($"[Iniciación cocina] {Who} y «{student.name}» cocinan «{_recipe.dishName}» POR MAYOR ×{n} (stock {dish.Have}; {_recipe.Quantities()}).");
    }

    // Índice del paso en que va el alumno (al terminar la receta su estación pasa a otra: = todos hechos).
    int StudentIdx() => _cs == null ? _recipe.steps.Length : (_cs.Recipe == _recipe ? _cs.StepIndex : _recipe.steps.Length);

    void Cook()
    {
        bool isPlayer = student.GetComponent<PlayerController>() != null;
        int idx = StudentIdx();
        if (idx > _seenIdx)   // el alumno lanzó el paso (o se lo hicieron al vencer el tiempo: ya contado en stepsHelped)
        {
            if (!_helping) _r.stepsByStudent += idx - _seenIdx;
            _helping = false; _seenIdx = idx;
        }
        if (idx >= _recipe.steps.Length) { CookDone(); return; }

        if (idx != _promptedIdx)   // paso nuevo: el trabajador lo hace a su lado y se lo PIDE
        {
            _promptedIdx = idx;
            _stepDeadline = Time.time + (isPlayer ? stepTimeout : aiStepSeconds);
            string step = _recipe.steps[idx];
            string qty = step.Trim().ToLowerInvariant().StartsWith("toma ") ? $" — para {_recipe.servings} raciones: {_recipe.Quantities()}" : "";
            if (isPlayer)
            {
                LessonPrompt.Show($"{Who} (por mayor ×{_recipe.servings}): {step}{qty}", "step", stepTimeout + 2f);
                SpellConsole.Propose("step", $"{Who} te pide: {step}", stepTimeout);
            }
            Debug.Log($"[Iniciación cocina] paso {idx + 1}/{_recipe.steps.Length}: «{step}» — {Who} lo hace y se lo pide a «{student.name}».");
        }
        Gesture(_recipe.steps[idx]);
        if (Time.time < _stepDeadline) return;
        // Venció el tiempo: el trabajador lo hace CON él (no se atasca nunca). Una IA, en cambio, lo hace sola.
        if (isPlayer) { _r.stepsHelped++; _helping = true; Debug.Log($"[Iniciación cocina] «{student.name}» no lanzó «{_recipe.steps[idx]}» a tiempo: {Who} lo hace con él."); }
        _cs.AdvanceStep();
    }
    bool _helping;

    void CookDone()
    {
        _cookDone = true; _r.cooked = true;
        RestaurantKitchen.DishStock d = Kitchen() != null ? Kitchen().Of(_recipe.dishName) : null;
        _r.stockAfter = d != null ? d.Have : _r.stockBefore;
        if (!string.IsNullOrEmpty(_recipe.id))   // aprende la receta al hacerla (Goluis ya no se la enseña aparte)
        {
            Grimoire g = student.GetComponent<Grimoire>() ?? student.gameObject.AddComponent<Grimoire>();
            g.Learn(_recipe.id);
        }
        if (_addedStation && _cs != null) Destroy(_cs);
        LessonPrompt.Hide();
        Stand(GetComponent<Anima>());
        Say($"¡Listo! {_r.servings} raciones de «{_recipe.dishName}» para el personal (stock {_r.stockBefore} → {_r.stockAfter}). Ahora ven al tanque de la cocina: tu primer viaje desde aquí.", 7f);
        Debug.Log($"[Iniciación cocina] tanda por mayor terminada: «{_recipe.dishName}» ×{_r.servings} (stock {_r.stockBefore} → {_r.stockAfter}; pasos del alumno {_r.stepsByStudent}, ayudados {_r.stepsHelped}).");
        Now = Phase.ToTank; _phaseDeadline = Time.time + 60f;
    }

    // Brazos del trabajador mientras demuestra (como KitchenCook): remover, cortar, coger.
    void Gesture(string step)
    {
        CreatureRig rig = GetComponent<CreatureRig>();
        if (rig == null || Time.time < _nextFrame) return;
        _nextFrame = Time.time + 0.25f; _frameB = !_frameB;
        string s = step.ToLowerInvariant();
        float x = s.Contains("corta") || s.Contains("pica") ? (_frameB ? -60f : -30f)
                : s.Contains("toma") || s.Contains("emplata") || s.Contains("sirve") ? -60f
                : (_frameB ? -65f : -45f);
        Bone(rig, BodyPart.ShoulderRight, Quaternion.Euler(x, 0f, _frameB ? 10f : -10f));
        Bone(rig, BodyPart.ForearmRight, Quaternion.Euler(-30f, 0f, 0f));
        Bone(rig, BodyPart.ShoulderLeft, Quaternion.Euler(-25f, 0f, 0f));
    }
    static void Bone(CreatureRig rig, BodyPart p, Quaternion q) { Transform t = rig.Get(p); if (t == null) return; rig.LockPose(p, 0.35f); t.localRotation = q; }

    // ── AL TANQUE de su área y a su primer microcosmos ───────────────────────────────────────────────────────────────
    VirtualizationMachine Machine()
    {
        string area = kind == Kind.Yoga ? YogaArea : KitchenArea;
        foreach (VirtualizationMachine vm in SceneCache.All<VirtualizationMachine>(2f)) if (vm != null && vm.areaName == area) return vm;
        return null;
    }

    string Scene(VirtualizationMachine vm)
    {
        if (kind == Kind.Yoga) return Magnate.RiverMicrocosmos;
        return vm != null && !string.IsNullOrEmpty(vm.mobWorldSceneName) ? vm.mobWorldSceneName : KitchenMicrocosmos;
    }

    void ToTank()
    {
        VirtualizationMachine vm = Machine();
        string scene = Scene(vm);
        bool isPlayer = student.GetComponent<PlayerController>() != null;
        if (vm == null || !isPlayer || !Application.CanStreamedLevelBeLoaded(scene))
        {
            if (isPlayer) Debug.LogWarning($"[Iniciación {Label}] sin tanque o sin la escena «{scene}» en Build Settings: la iniciación termina sin microcosmos.");
            Complete(false, scene);
            return;
        }
        Vector3 at = vm.transform.position;
        Vector3 d = at - transform.position; d.y = 0f;
        if (d.magnitude > 4f && Time.time < _phaseDeadline) { Hold(true); Escort(student.transform, Time.deltaTime); Step(at); return; }
        if (d.magnitude > 4f) { WorkdayDirector.Teleport(transform, at - Flat(d) * 3f); WorkdayDirector.Teleport(student.transform, at - Flat(d) * 2.5f + Vector3.right); }
        Hold(false);
        if (MobWorldLoader.HasInstance && (MobWorldLoader.Instance.IsBusy || MobWorldLoader.Instance.IsInMobWorld)) { Complete(false, scene); return; }
        string who = kind == Kind.Yoga ? "al río: allí hay alguien que te necesita" : "a la cocina de la era del fuego: la tribu lleva restos a su guarida; ayuda a Nasatya";
        PlaneMessenger.Send(Who, $"Ponte la máscara y entra al agua: vas {who}.", 6f);
        LessonPrompt.Show($"{Who}: ponte la máscara y entra al agua — vas {who}.", null, 6f);
        Debug.Log($"[Iniciación {Label}] {Who} mete a «{student.name}» en el tanque y lo envía a «{scene}».");
        vm.EnterWithTank(student.transform, scene);
        Complete(true, scene);
    }

    void Complete(bool entered, string scene)
    {
        _r.enteredMicrocosmos = entered; _r.scene = entered ? scene : null;
        if (kind == Kind.Kitchen) student.kitchenTrained = true;
        Finish(null);
    }

    void Finish(string why)
    {
        if (_r == null) _r = new Report { kind = kind, worker = Who };
        Last = _r;
        if (why != null) Debug.Log($"[Iniciación {Label}] termina: {why}.");
        if (why != null && kind == Kind.Kitchen && student != null) student.kitchenTrained = true;   // por tope: no se repite
        Hold(false);
        if (_addedStation && _cs != null) Destroy(_cs);
        if (_ownCook != null) _ownCook.enabled = _ownCookWasOn;
        MagnateWarden.Lock(this, false);
        Now = Phase.Done;
        Destroy(this);
    }

    void OnDestroy() { MagnateWarden.Lock(this, false); Hold(false); }

    // ── Andar, llevar de la mano, hablar ─────────────────────────────────────────────────────────────────────────
    void Step(Vector3 target)
    {
        Vector3 to = target;
        if (CarpetPad.Route(gameObject, ref to)) return;
        PathMover.Of(this).StepToward(to, moveSpeed);
    }

    // El alumno anda detrás del trabajador (como en la enfermería; el jugador no lo controla mientras tanto).
    void Escort(Transform p, float dt)
    {
        Vector3 behind = transform.position - transform.forward * 1.0f + transform.right * 0.4f; behind.y = p.position.y;
        Vector3 d = behind - p.position; d.y = 0f;
        if (d.magnitude > 0.6f) WorkdayDirector.Teleport(p, Vector3.MoveTowards(p.position, behind, moveSpeed * 1.15f * dt));
        Vector3 look = d.magnitude > 0.6f ? d : transform.position - p.position; look.y = 0f;
        if (look.sqrMagnitude > 0.01f) p.rotation = Quaternion.Slerp(p.rotation, Quaternion.LookRotation(look), dt * 6f);
    }

    void Hold(bool on)
    {
        PlayerController pc = student != null ? student.GetComponent<PlayerController>() : null;
        if (pc == null) { _held = false; return; }
        if (on && !_held) { pc.HoldBody(true); _held = true; }
        else if (!on && _held) { pc.HoldBody(false); _held = false; }
    }

    void Say(string text, float secs)
    {
        Debug.Log($"[Iniciación {Label}] {Who}: «{text}»");
        if (student != null && student.GetComponent<PlayerController>() != null) LessonPrompt.Show($"{Who}: {text}", null, secs + 2f);
    }

    static void Stand(Anima a) { if (a != null) DailyPoses.Stand().ApplyFinal(a); }
    static Vector3 Flat(Vector3 v) { v.y = 0f; return v.sqrMagnitude > 1e-4f ? v.normalized : Vector3.forward; }
}
