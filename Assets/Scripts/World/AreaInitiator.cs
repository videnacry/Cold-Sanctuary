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
    public enum Kind { Yoga, Kitchen, Garden }
    public enum Phase { None, Approach, Escort, Lesson, Cook, Soil, ToTank, Done }

    public const string YogaArea = "Sala de Yoga", KitchenArea = "Cocina", GardenArea = "Huerto";
    /// <summary>El microcosmos del huerto: La Sembradora (Ruth renacida) y las recolectoras que aprenden a cultivar.</summary>
    public const string GardenMicrocosmos = "Microcosmos_Huerto";
    /// <summary>El primer microcosmos de la cocina (la era del fuego: las reencarnaciones; AreaMicrocosmos.KitchenStory).</summary>
    public const string KitchenMicrocosmos = "Microcosmos_Cocina";

    public Kind kind;
    public SanctuaryOnboarding student;
    public float moveSpeed = 3.2f;
    [Tooltip("Paso de andar dentro de la orquesta (m/s): el ciclo de andar mueve las piernas.")]
    public float walkSpeed = 1.8f;
    [Tooltip("Segundos que el trabajador espera a que el jugador lance cada paso de cocina; después lo hace con él.")]
    public float stepTimeout = 30f;
    [Tooltip("Segundos por paso cuando el alumno es la IA (lo hace solo).")]
    public float aiStepSeconds = 2.5f;
    [Tooltip("Tope de toda la iniciación (s de juego): pase lo que pase, termina.")]
    public float maxSeconds = 2400f;

    /// <summary>Con las pruebas corriendo, la iniciación espera (como la de la enfermería); el test de la cocina la deja actuar.</summary>
    public static bool AllowDuringTests;

    public Phase Now { get; private set; }
    /// <summary>¿Está esperando a que el alumno lance el paso actual? (el test emula al jugador con esto)</summary>
    public bool WaitingForStudent => (Now == Phase.Cook && _promptedIdx >= 0 && !_cookDone) || (Now == Phase.Soil && _soilPrompted >= 0 && !_soilCast && _soilIdx < SoilSteps.Length);

    /// <summary>Lo que dejó la última iniciación de área (para los tests y el log).</summary>
    public class Report
    {
        public Kind kind; public string worker, student, dish, scene; public int servings, stockBefore, stockAfter, stepsByStudent, stepsHelped;
        public bool lessonDone, cooked, soilPrepared, enteredMicrocosmos;
        public string plot;
    }
    public static Report Last { get; private set; }
    Report _r;

    static readonly List<AreaInitiator> _all = new List<AreaInitiator>();
    float _phaseDeadline, _endAt, _stepDeadline, _nextFrame, _approachCap, _apLog, _apBest = float.MaxValue; Vector3 _apAnchor;
    bool _cookDone, _addedStation, _frameB;
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
        string area = AreaOf(k);
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
        // Si estaban en la clase de yoga en grupo (YogaClass), la dejan: la clase movía y ponía en postura a los mismos cuerpos
        // que la iniciación (el maestro no hacía los pasos del cuello que pedía).
        YogaClass yc = YogaClass.Current;
        if (yc != null) { yc.Leave(student != null ? student.GetComponentInParent<Anima>() : null); if (YogaClass.Current != null) YogaClass.Current.Leave(GetComponent<Anima>()); }
        Now = Phase.Approach; _phaseDeadline = Time.time + Orchestra.StuckSeconds; _approachCap = Time.time + 300f; _apBest = float.MaxValue;
        Debug.Log($"[Iniciación {Label}] {Who} (de turno) lanza «iniciar» sobre «{_r.student}».");
    }

    string Label => kind == Kind.Yoga ? "sala de yoga" : kind == Kind.Kitchen ? "cocina" : "huerto";
    static string AreaOf(Kind k) => k == Kind.Yoga ? YogaArea : k == Kind.Kitchen ? KitchenArea : GardenArea;

    void Update()
    {
        if (ColdSanctuary.TestRunner.Running && !AllowDuringTests) return;
        if (student == null) { Finish("el alumno ya no está"); return; }
        if (Time.time > _endAt) { Finish($"tope de {maxSeconds:0} s: se da por hecha"); return; }
        if (Now == Phase.Approach) { Approach(); return; }
        if (_orch != null) _orch.Tick();
        if (this == null || Now == Phase.Done) return;
        switch (Now)
        {
            case Phase.Lesson: Lesson(); break;
            case Phase.Cook: Cook(); break;
            case Phase.Soil: Soil(); break;
        }
    }

    void LateUpdate() { if (_orch != null) _orch.LateTick(); }

    /// <summary>La orquesta de esta iniciación (null antes de empezarla).</summary>
    public Orchestra Score => _orch;
    Orchestra _orch;
    Orchestra.Role _rWorker, _rStudent;
    /// <summary>Corta la iniciación (pruebas): suelta al alumno y se va.</summary>
    public void Cancel(string why) => Finish(why);

    // 1. Va hacia el recién llegado (la Magnate lo ha dejado en la puerta del área) hasta 2 m: ahí empieza la ORQUESTA.
    void Approach()
    {
        Vector3 d = student.transform.position - transform.position;
        // Tope por ATASCO (40 s sin acercarse; en alfombra cuenta como avance), no por reloj: desde otra torre se tarda más de
        // 40 s y se le teletransportaba aunque viniera andando (Berón 2026-10-29: nada de teletransportes en una partida normal).
        float gapNow = d.magnitude;
        bool movedAp = (transform.position - _apAnchor).sqrMagnitude > 0.09f;
        if (gapNow < _apBest - 0.3f || FlyingCarpet.Riding(gameObject) || (movedAp && gapNow > 3f)) { _apBest = Mathf.Min(_apBest, gapNow); _phaseDeadline = Time.time + Orchestra.StuckSeconds; }   // (lejos, andar hacia el andén de alfombras también es avanzar)
        if (movedAp) _apAnchor = transform.position;
        if (Time.time > _phaseDeadline - Orchestra.StuckSeconds + 5f && Time.time >= _apLog) { _apLog = Time.time + 10f; Debug.Log($"[Iniciación {Label}] {Who} no avanza hacia el recién llegado ({gapNow:0} m, en ({transform.position.x:0.0},{transform.position.y:0.0},{transform.position.z:0.0})); {Orchestra.Movers(transform)}"); }
        if (d.magnitude > 2f && Time.time < _phaseDeadline && Time.time < _approachCap) { Step(student.transform.position); return; }
        if (d.magnitude > 2f) Orchestra.Emergency(transform, student.transform.position - Flat(d) * 1.5f, $"[Iniciación {Label}] {Who} no llegaba andando al recién llegado");
        Say(kind == Kind.Garden
            ? "Bienvenido al huerto. Hoy preparas conmigo el suelo del bancal: abono del compostero, esparcirlo, la azada y los surcos. Ponte en el borde del bancal."
            : kind == Kind.Yoga
            ? "Bienvenido a la sala de yoga. Tu primera clase es obligatoria: upa-yoga de cuello. Ve a la esterilla del centro: te la doy yo, delante de ti."
            : "Bienvenido a la cocina. Aquí cocinamos POR MAYOR para que coma todo el santuario: hoy haces conmigo el platillo que más falta. Ponte en la mesa de emplatado.", 7f);
        _orch = Build();
        Now = Phase.Escort;
    }

    // LA ORQUESTA del área: cada uno anda SOLO a su sitio (el alumno delante del sitio de trabajo, el trabajador frente a él o
    // a su lado); el alumno avisa «en su sitio», el trabajador empieza la clase o la tanda y los dos se quedan ahí hasta que
    // acaba; después, los dos andan al tanque y, con el alumno delante, el trabajador lo mete dentro.
    Orchestra Build()
    {
        var o = new Orchestra($"iniciación de {Label}", walkSpeed);
        Transform p = student.transform;
        _rStudent = o.Join(student.name, p); _rWorker = o.Join(Who, transform);
        Transform place = WorkPlace();
        o.Movement(kind == Kind.Yoga ? "a la esterilla" : kind == Kind.Kitchen ? "a la mesa" : "al bancal");
        o.GoTo(_rStudent, () => Spot(place, false), () => transform.position, "va solo a su sitio");
        o.Emit(_rStudent, "en_su_sitio");
        o.Until(_rStudent, () => Now == Phase.ToTank, () => transform.position, kind == Kind.Yoga ? "la clase" : kind == Kind.Kitchen ? "cocina" : "prepara el suelo");
        o.GoTo(_rWorker, () => Spot(place, true), () => p.position, "va a su sitio");
        o.WaitFor(_rWorker, "en_su_sitio").Do(_rWorker, BeginWork, kind == Kind.Yoga ? "empieza la clase" : kind == Kind.Kitchen ? "empieza la tanda" : "empieza a preparar el suelo");
        o.Until(_rWorker, () => Now == Phase.ToTank, () => p.position, kind == Kind.Yoga ? "da la clase" : kind == Kind.Kitchen ? "cocina con él" : "prepara el suelo con él");

        o.Movement("al tanque");
        VirtualizationMachine vm = Machine();
        string scene = Scene(vm);
        bool isPlayer = p.GetComponent<PlayerController>() != null;
        if (vm == null || !isPlayer || !Application.CanStreamedLevelBeLoaded(scene))
        {
            o.Do(_rWorker, () =>
            {
                if (isPlayer) Debug.LogWarning($"[Iniciación {Label}] sin tanque o sin la escena «{scene}» en Build Settings: la iniciación termina sin microcosmos.");
                Complete(false, scene);
            }, "termina (sin tanque)");
            return o;
        }
        o.GoTo(_rStudent, () => TankSpot(vm, false), () => vm.transform.position, "va solo al tanque");
        o.Emit(_rStudent, "en_el_tanque").WaitFor(_rStudent, "dentro");
        o.GoTo(_rWorker, () => TankSpot(vm, true), () => p.position, "va al tanque");
        o.WaitFor(_rWorker, "en_el_tanque").Do(_rWorker, () => Handover(vm, scene), "lo mete en el tanque").Emit(_rWorker, "dentro");
        return o;
    }

    // Dónde trabajan: la esterilla de la sala de yoga o la mesa de emplatado (o el punto «CocinaSpot»).
    Transform WorkPlace()
    {
        if (kind == Kind.Yoga) { GameObject s = GameObject.Find("SalaYoga"); return s != null ? s.transform : null; }
        if (kind == Kind.Garden) { GardenPlot gp = Plot(); if (gp != null) return gp.transform; GameObject ga = GameObject.Find("Garden_Area"); return ga != null ? ga.transform : null; }
        RestaurantKitchen rk = Kitchen();
        Transform t = rk != null ? VirtualInPlace.FindDeep(rk.transform, "Mesa de emplatado") : null;
        if (t == null) { GameObject s = GameObject.Find("CocinaSpot"); t = s != null ? s.transform : null; }
        return t;
    }

    // Yoga: el alumno en la esterilla y el maestro DELANTE, a 1,8 m, de cara a él. Cocina: los dos delante de la mesa, uno al
    // lado del otro (a 1 m).
    Vector3 Spot(Transform place, bool worker)
    {
        if (place == null) return worker ? student.transform.position + student.transform.forward * 1.8f : student.transform.position;
        Vector3 fr = ArrivalExam.Front(place), side = Vector3.Cross(Vector3.up, fr);
        if (kind == Kind.Yoga) return place.position + (worker ? fr * 1.8f : Vector3.zero);
        if (kind == Kind.Garden && place.TryGetComponent(out GardenPlot gp))
        {
            // En el BORDE del bancal que da al centro del área (no encima de las plantas): uno al lado del otro, a 1,2 m.
            float ext = Mathf.Abs(Vector3.Dot(fr, Flat(place.right))) * gp.size.x * 0.5f + Mathf.Abs(Vector3.Dot(fr, Flat(place.forward))) * gp.size.y * 0.5f;
            return place.position + fr * (ext + 0.8f) + (worker ? side * 1.2f : Vector3.zero);
        }
        return place.position + fr * 0.8f + (worker ? side * 1.0f : Vector3.zero);
    }

    static Vector3 TankSpot(VirtualizationMachine vm, bool worker)
    {
        Vector3 fr = ArrivalExam.Front(vm.transform), side = Vector3.Cross(Vector3.up, fr);
        return vm.transform.position + fr * (worker ? 3.0f : 2.4f) + side * (worker ? 1.2f : 0f);
    }

    RestaurantKitchen Kitchen() => _rk != null ? _rk : (_rk = FindAnyObjectByType<RestaurantKitchen>());

    void BeginWork()
    {
        if (kind == Kind.Yoga) BeginLesson(); else if (kind == Kind.Kitchen) BeginCook(); else BeginSoil();
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
        _phaseDeadline = Time.time + UpaYoga.NeckSequence().Count * (14f + UpaYoga.Breath + 4f) + 30f;
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
            Grimoire g = student.gameObject.GetOrAdd<Grimoire>();
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

    // ── HUERTO: PREPARAR EL SUELO del bancal (docs/garden-simulation.md §8, receta base: 1 abonar · 2 arar) ─────────────
    /// <summary>Un paso de la preparación del suelo: lo que dice el trabajador, la herramienta y los dos fotogramas del
    /// gesto (el trabajador lo demuestra y el alumno lo repite al lanzarlo).</summary>
    public class SoilStep { public string say, tool; public System.Func<AsanaPoseSpell> a, b; }

    static AsanaPoseSpell G(string n, float root, params (BodyPart, Vector3)[] s) => DailyPoses.Make(n, root, s);
    /// <summary>Los pasos, en orden: abono del compostero → esparcirlo → la azada → los surcos → rastrillar.</summary>
    public static readonly SoilStep[] SoilSteps =
    {
        new SoilStep { say = "Coge abono del compostero con la pala y llena la carretilla (es el nitrógeno del suelo).", tool = "Pala",
            a = () => G("hunde la pala en el compost", -1f, (BodyPart.Back, new Vector3(40, 0, 0)), (BodyPart.ShoulderRight, new Vector3(-70, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-60, 0, 0)), (BodyPart.ForearmRight, new Vector3(-20, 0, 0))),
            b = () => G("levanta la pala llena", -1f, (BodyPart.Back, new Vector3(15, 0, 0)), (BodyPart.ShoulderRight, new Vector3(-45, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-50, 0, 0)), (BodyPart.ForearmRight, new Vector3(-50, 0, 0))) },
        new SoilStep { say = "Esparce el abono por todo el bancal, a voleo, una capa fina.", tool = "Pala",
            a = () => G("esparce a la derecha", -1f, (BodyPart.Back, new Vector3(20, -20, 0)), (BodyPart.ShoulderRight, new Vector3(-60, 0, -30)), (BodyPart.ShoulderLeft, new Vector3(-50, 0, 10))),
            b = () => G("esparce a la izquierda", -1f, (BodyPart.Back, new Vector3(20, 20, 0)), (BodyPart.ShoulderRight, new Vector3(-60, 0, 20)), (BodyPart.ShoulderLeft, new Vector3(-50, 0, 30))) },
        new SoilStep { say = "Ahora la azada: levántala y clávala, para soltar la tierra con el abono.", tool = "Azada",
            a = () => G("azada arriba", -1f, (BodyPart.Back, new Vector3(-5, 0, 0)), (BodyPart.ShoulderRight, new Vector3(-150, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-140, 0, 0)), (BodyPart.ForearmRight, new Vector3(-20, 0, 0))),
            b = () => G("azada clavada", -1f, (BodyPart.Back, new Vector3(35, 0, 0)), (BodyPart.ShoulderRight, new Vector3(-40, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-35, 0, 0))) },
        new SoilStep { say = "Abre los surcos: tira de la azada hacia ti, en líneas rectas, a un palmo uno de otro.", tool = "Azada",
            a = () => G("azada adelante", -1f, (BodyPart.Back, new Vector3(30, 0, 0)), (BodyPart.ShoulderRight, new Vector3(-75, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-70, 0, 0))),
            b = () => G("tira del surco", -1f, (BodyPart.Back, new Vector3(20, 0, 0)), (BodyPart.ShoulderRight, new Vector3(-35, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-30, 0, 0)), (BodyPart.ForearmRight, new Vector3(-40, 0, 0))) },
        new SoilStep { say = "Rastrilla para dejarlo llano y sin terrones: el suelo ya está listo para sembrar.", tool = "Rastrillo",
            a = () => G("rastrillo adelante", -1f, (BodyPart.Back, new Vector3(25, 0, 0)), (BodyPart.ShoulderRight, new Vector3(-70, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-65, 0, 0))),
            b = () => G("rastrillo atrás", -1f, (BodyPart.Back, new Vector3(15, 0, 0)), (BodyPart.ShoulderRight, new Vector3(-40, 0, 0)), (BodyPart.ShoulderLeft, new Vector3(-35, 0, 0)), (BodyPart.ForearmRight, new Vector3(-30, 0, 0))) },
    };

    GardenPlot _plot;
    int _soilIdx, _soilPrompted = -1;
    bool _soilCast, _plotTool;
    float _soilDeadline, _soilDoneAt, _soilFrame;
    bool _soilFrameB, _soilHelped;

    // El bancal donde se inicia: el de hortalizas del área del huerto (si no, el más cercano).
    GardenPlot Plot()
    {
        if (_plot != null) return _plot;
        GameObject area = GameObject.Find("Garden_Area");
        GardenPlot best = null; float bd = float.MaxValue;
        foreach (GardenPlot gp in GardenPlot.All)
        {
            if (gp == null) continue;
            float d = (gp.transform.position - (area != null ? area.transform.position : transform.position)).sqrMagnitude;
            if (area != null && gp.transform.IsChildOf(area.transform)) d -= 1e6f;          // el del área, primero
            if (gp.guild == GardenPlot.Guild.Hortalizas) d -= 1e5f;
            if (d < bd) { bd = d; best = gp; }
        }
        return _plot = best;
    }

    /// <summary>El alumno lanza el paso que se le pide en el huerto (el hechizo `step`, como en la cocina).</summary>
    public static bool StudentStep(Anima who)
    {
        SanctuaryOnboarding o = who != null ? who.GetComponent<SanctuaryOnboarding>() : null;
        AreaInitiator ai = o != null ? Of(o) : null;
        if (ai == null || ai.Now != Phase.Soil || ai._soilCast || ai._soilIdx >= SoilSteps.Length) return false;
        ai._soilCast = true; ai._soilHelped = false; ai._soilDoneAt = Time.time + 2.5f;
        return true;
    }

    void BeginSoil()
    {
        _plot = Plot();
        _soilIdx = 0; _soilPrompted = -1; _soilCast = false;
        _r.plot = _plot != null ? _plot.name : "(sin bancal)";
        Now = Phase.Soil;
        Say($"Lo primero en un huerto es el SUELO: hoy preparamos «{(_plot != null ? GardenPlot.Title(_plot.guild) : "el bancal")}». Hazlo conmigo, paso a paso.", 7f);
        Debug.Log($"[Iniciación huerto] {Who} y «{student.name}» preparan el suelo de «{_r.plot}» ({SoilSteps.Length} pasos).");
    }

    // Cada paso: el trabajador lo demuestra (sus dos fotogramas) y se lo PIDE; el alumno lo lanza (`step`) y su cuerpo lo
    // hace 2,5 s; si no lo lanza a tiempo, el trabajador lo hace CON él (su cuerpo lo hace igual) para no atascarse nunca.
    void Soil()
    {
        if (_soilIdx >= SoilSteps.Length) { SoilDone(); return; }
        SoilStep s = SoilSteps[_soilIdx];
        bool isPlayer = student.GetComponent<PlayerController>() != null;
        Anima me = GetComponent<Anima>(), st = student.GetComponentInParent<Anima>();
        if (_soilPrompted != _soilIdx)
        {
            _soilPrompted = _soilIdx; _soilCast = false;
            _soilDeadline = Time.time + (isPlayer ? stepTimeout : aiStepSeconds);
            ArrivalExam.Hold(gameObject, s.tool, BodyPart.HandRight, "Herramienta"); _plotTool = true;
            if (isPlayer)
            {
                LessonPrompt.Show($"{Who} (huerto, paso {_soilIdx + 1}/{SoilSteps.Length}): {s.say}", "step", stepTimeout + 2f);
                SpellConsole.Propose("step", $"{Who} te pide: {s.say}", stepTimeout);
            }
            Debug.Log($"[Iniciación huerto] paso {_soilIdx + 1}/{SoilSteps.Length}: «{s.say}» — {Who} lo demuestra y se lo pide a «{student.name}».");
        }
        // El gesto, alternando los dos fotogramas: el trabajador siempre; el alumno, mientras lo hace.
        if (Time.time >= _soilFrame)
        {
            _soilFrame = Time.time + 0.8f; _soilFrameB = !_soilFrameB;
            if (me != null) (_soilFrameB ? s.b : s.a)().ApplyFinal(me);
            if (_soilCast && st != null) (_soilFrameB ? s.b : s.a)().ApplyFinal(st);
        }
        if (!_soilCast && Time.time >= _soilDeadline)
        {
            // Venció el tiempo: lo hace con él (una IA, en cambio, lo hace sola: cuenta como suyo).
            _soilCast = true; _soilHelped = isPlayer; _soilDoneAt = Time.time + 2.5f;
            if (isPlayer) Debug.Log($"[Iniciación huerto] «{student.name}» no lanzó «{s.say}» a tiempo: {Who} lo hace con él.");
        }
        if (!_soilCast || Time.time < _soilDoneAt) return;
        if (_soilHelped) _r.stepsHelped++; else _r.stepsByStudent++;
        if (_plot != null) _plot.PrepareSoil(_soilIdx + 1, SoilSteps.Length);
        _soilIdx++;
    }

    void SoilDone()
    {
        _r.soilPrepared = _plot == null || _plot.SoilPrepared;
        ArrivalExam.Hold(gameObject, null, BodyPart.HandRight, "Herramienta"); _plotTool = false;
        Stand(GetComponent<Anima>()); Stand(student.GetComponentInParent<Anima>());
        student.gardenTrained = true;
        Grimoire g = student.gameObject.GetOrAdd<Grimoire>();
        g.Learn("preparar-suelo");
        LessonPrompt.Hide();
        Say("¡Suelo listo para sembrar! Ahora ven al tanque del huerto: tu primer viaje desde aquí.", 7f);
        Debug.Log($"[Iniciación huerto] suelo preparado en «{_r.plot}» (pasos del alumno {_r.stepsByStudent}, ayudados {_r.stepsHelped}).");
        Now = Phase.ToTank;
    }

    // ── AL TANQUE de su área y a su primer microcosmos ───────────────────────────────────────────────────────────────
    VirtualizationMachine Machine()
    {
        string area = AreaOf(kind);
        foreach (VirtualizationMachine vm in SceneCache.All<VirtualizationMachine>(2f)) if (vm != null && vm.areaName == area) return vm;
        return null;
    }

    string Scene(VirtualizationMachine vm)
    {
        if (kind == Kind.Yoga) return Magnate.RiverMicrocosmos;
        return vm != null && !string.IsNullOrEmpty(vm.mobWorldSceneName) ? vm.mobWorldSceneName : kind == Kind.Garden ? GardenMicrocosmos : KitchenMicrocosmos;
    }

    // Con el alumno delante del tanque: lo mete dentro y lo envía a su primer microcosmos de esta área.
    void Handover(VirtualizationMachine vm, string scene)
    {
        _orch.Release(_rStudent);
        if (MobWorldLoader.HasInstance && (MobWorldLoader.Instance.IsBusy || MobWorldLoader.Instance.IsInMobWorld)) { Complete(false, scene); return; }
        string who = kind == Kind.Yoga ? "al río: allí hay alguien que te necesita" : kind == Kind.Kitchen ? "a la cocina de la era del fuego: la tribu lleva restos a su guarida; ayuda a Nasatya" : "al huerto de las hormigas: La Sembradora enseña a las recolectoras a cultivar el hongo";
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
        if (kind == Kind.Garden) { student.gardenTrained = true; PlayerWorkday.NotifyOnboardingComplete(); }   // la iniciación termina: su jornada empieza (al volver del microcosmos del huerto)
        Finish(null);
    }

    void Finish(string why)
    {
        if (_r == null) _r = new Report { kind = kind, worker = Who };
        Last = _r;
        if (why != null) Debug.Log($"[Iniciación {Label}] termina: {why}.");
        if (why != null && kind == Kind.Kitchen && student != null) student.kitchenTrained = true;   // por tope: no se repite
        if (why != null && kind == Kind.Garden && student != null) student.gardenTrained = true;
        if (_plotTool) ArrivalExam.Hold(gameObject, null, BodyPart.HandRight, "Herramienta");
        _orch?.Stop();   // suelta al alumno (la orquesta le sujetaba el cuerpo)
        if (_addedStation && _cs != null) Destroy(_cs);
        if (_ownCook != null) _ownCook.enabled = _ownCookWasOn;
        MagnateWarden.Lock(this, false);
        Now = Phase.Done;
        Destroy(this);
    }

    void OnDestroy() { MagnateWarden.Lock(this, false); _orch?.Stop(); }

    // ── Andar, llevar de la mano, hablar ─────────────────────────────────────────────────────────────────────────
    void Step(Vector3 target)
    {
        Vector3 to = target;
        if (CarpetPad.Route(gameObject, ref to)) return;
        PathMover.Of(this).StepToward(to, moveSpeed);
    }


    void Say(string text, float secs)
    {
        Debug.Log($"[Iniciación {Label}] {Who}: «{text}»");
        if (student != null && student.GetComponent<PlayerController>() != null) LessonPrompt.Show($"{Who}: {text}", null, secs + 2f);
    }

    static void Stand(Anima a) { if (a != null) DailyPoses.Stand().ApplyFinal(a); }
    static Vector3 Flat(Vector3 v) { v.y = 0f; return v.sqrMagnitude > 1e-4f ? v.normalized : Vector3.forward; }
}
