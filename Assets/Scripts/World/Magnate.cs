using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA MAGNATE (docs/anyma-factory-and-control.md §Magnate) — la maestra del santuario. **Estado "infinito"**: stats y
/// margas al tope (no hay límite numérico definido → se modela como un estado), **sin necesidades** (no come ni
/// duerme), **imposeíble de poseer** (relevancia altísima). No la mueven apremios sino sus **pensamientos custom**: su
/// trabajo de GUARDIANA es (1) **proteger la vida** —al aparecer, pacifica a los depredadores cercanos (todas las
/// animas sienten su poder; el lobo deja de atacar)— y (2) **hacer crecer a todos como hechiceros**: cada cierto
/// tiempo busca al que se está **quedando atrás** (menor nivel integral), va a él y le **enseña yoga** (le da el
/// recuerdo en el `Grimoire`, le encola el Saludo al Sol y potencia su XP/pensamiento). Existe (clonada) en todos los
/// sanctuary como una compañera más. Primera rebanada: estado infinito + pacificar + enseñar al rezagado.
/// </summary>
[RequireComponent(typeof(Anima))]
public class Magnate : MonoBehaviour
{
    [Tooltip("Cada cuántos segundos de juego revisa el avance del grupo y va a ayudar al rezagado.")]
    [Min(1f)] public float teachInterval = 30f;
    [Tooltip("Una clase por alumno cada tanto (s reales; un día del santuario = 1 h). Antes repetía clase al rezagado cada 30 s y parecía hacer yoga todo el tiempo.")]
    [Min(0f)] public float classCooldown = 1200f;
    [Tooltip("Radio en el que pacifica (los depredadores dejan de estar 'aware').")]
    [Min(0f)] public float pacifyRadius = 12f;
    [Tooltip("Distancia a la que considera que ya está junto al alumno para enseñar.")]
    [Min(0.3f)] public float teachRadius = 2.5f;
    [Min(0f)] public float moveSpeed = 4f;

    [Header("Guía del nuevo integrante (sala de yoga → cocina)")]
    [Tooltip("Dónde lleva al nuevo a hacer upa-yoga. Si está vacío, busca un GameObject llamado 'SalaYoga'.")]
    public Transform yogaSpot;
    [Tooltip("Dónde lleva al nuevo a aprender un área. Si está vacío, busca un GameObject llamado 'CocinaSpot'.")]
    public Transform kitchenSpot;
    [Tooltip("Dónde lleva al nuevo después de la cocina: el huerto. Si está vacío, el área «Garden_Area».")]
    public Transform gardenSpot;

    Anima _a;
    PacifySpell _pacify;
    float _nextTeach, _nextPacify;
    Anima _student;
    readonly Dictionary<Anima, float> _taught = new Dictionary<Anima, float>();   // a quién dio clase y cuándo
    SanctuaryOnboarding _newcomer;
    static readonly Collider[] _hits = new Collider[32];

    void Awake()
    {
        _a = GetComponent<Anima>();
        MaxOut();
        AiBrain ai = this.GetOrAdd<AiBrain>();
        ai.selfRelevance = 99999f;   // nadie la posee
        _pacify = this.GetOrAdd<PacifySpell>();
        _pacify.force = 2f;
        _nextTeach = Time.time + 3f;
    }

    // Estado infinito: stats al tope (aptitudes ~0-2 normalmente → 999 = "sin límite").
    void MaxOut()
    {
        _a.strength = _a.bodyMass = _a.endurance = _a.agility = _a.perception = 999f;
        _a.composure = _a.reasoning = _a.memory = _a.creativity = _a.sociability = _a.discipline = _a.adaptability = 999f;
    }

    void Update()
    {
        if (_a == null) return;
        // Sin necesidades: nunca tiene hambre/sueño/estrés y está llena de reservas.
        _a.sleepiness = 0f; _a.stress = 0f; _a.fatReserves = 1f; _a.satisfaction = 1f; _a.sickness = 0f;
        if (_a is Animal an) an.hungry = 0f;

        Pacify();

        // Pensamiento PRIORITARIO: ¿hay un aspirante recién certificado apto por Linduarda y sin presentar?
        // Entonces lo presenta ante el santuario y lo lleva a yoga (upa-yoga) y a la cocina. Va antes que enseñar al rezagado.
        if (_newcomer == null) _newcomer = FindNewcomer();
        if (_newcomer != null) { GoPresent(); return; }

        // NO INTERRUMPE UNA JORNADA: si a quien enseña le empieza el turno, corta la clase; y no busca alumno entre quienes
        // están trabajando (le pedía pasos de upa-yoga a Kushal en plena misión: JornadaTest 2026-10-29).
        WorkdayDirector wd = WorkdayDirector.Instance;
        TeachingSession mine = GetComponent<TeachingSession>();
        if (mine != null && wd != null && mine.student != null && wd.OnShift(mine.student.gameObject)) mine.Cancel();
        if (_student != null && wd != null && wd.OnShift(_student.gameObject)) _student = null;
        // Dando una clase: se queda en ella (antes salía tras otro alumno a mitad y hacía las posturas andando o volando).
        if (GetComponent<TeachingSession>() != null) { _student = null; return; }
        if (Time.time >= _nextTeach) { _nextTeach = Time.time + teachInterval; _student = FindLaggard(); }
        if (_student != null) GoTeach();
    }

    // El aspirante apto que todavía no ha sido presentado (el más cercano).
    SanctuaryOnboarding FindNewcomer()
    {
        SanctuaryOnboarding best = null; float bestSq = float.MaxValue;
        foreach (SanctuaryOnboarding o in SceneCache.All<SanctuaryOnboarding>(0.5f))
        {
            if (o == null || !o.apt || (o.presented && o != _newcomer)) continue;
            float sq = (o.transform.position - transform.position).sqrMagnitude;
            if (sq < bestSq) { bestSq = sq; best = o; }
        }
        return best;
    }

    // HECHIZO DE INICIACIÓN DE LA MAGNATE (cuando Linduarda la llama: el aspirante ya es apto). Berón 2026-10-28: ella YA NO
    // da las iniciaciones; solo ACOMPAÑA al recién llegado de un área a otra y lo entrega al TRABAJADOR DE TURNO de cada una
    // (AreaInitiator), que lo inicia: en la SALA DE YOGA, su primera clase (upa-yoga de cuello) y el tanque al río de Sakshi;
    // a la vuelta, ella lo lleva a la COCINA, donde cocinan juntos por mayor el platillo con menos stock y el tanque lo lleva
    // al primer microcosmos de la cocina. Hasta entonces el recién llegado NO es del santuario y ella no lo tiene en cuenta
    // para enseñar al rezagado. Todas las esperas tienen tope.
    Anima _initiating;
    float _logAt, _phaseDeadline;
    // 0 = ir a buscarlo · 1 = caminar juntos a la sala de yoga · 2 = la inicia el de turno de la sala (clase + tanque)
    // 3 = en el río · 4 = caminar juntos a la cocina · 5 = la inicia el de turno de la cocina · 6 = en la cocina y su
    // microcosmos · 7 = caminar juntos al huerto · 8 = la inicia el de turno del huerto (preparar el suelo) · 9 = hecho
    int _initPhase;
    public const string RiverMicrocosmos = "Microcosmos_Scene0_SakshiRio";
    static readonly HashSet<SanctuaryOnboarding> _guiding = new HashSet<SanctuaryOnboarding>();
    /// <summary>¿La Magnate está acompañando a <paramref name="o"/> por sus iniciaciones de área? (el jefe de cocina espera)</summary>
    public static bool Guiding(SanctuaryOnboarding o) { _guiding.RemoveWhere(x => x == null); return o != null && _guiding.Contains(o); }

    void GoPresent()
    {
        if (_newcomer == null) return;
        Anima t = _newcomer.GetComponentInParent<Anima>();
        if (t == null) { _newcomer.presented = true; _guiding.Remove(_newcomer); _newcomer = null; return; }
        Transform yoga = yogaSpot != null ? yogaSpot : FindSpot("SalaYoga");
        Transform kitchen = kitchenSpot != null ? kitchenSpot : FindSpot("CocinaSpot");

        if (_initiating != t) { _initiating = t; _initPhase = 0; _guiding.Add(_newcomer); Debug.Log($"[Magnate] Linduarda la llama: va a buscar a «{t.name}» para llevarlo por sus iniciaciones."); }
        switch (_initPhase)
        {
            case 0:   // ir a buscarlo, hasta 2 m: ahí empieza la orquesta
                if (Flat(t.transform.position - transform.position) > Mathf.Min(teachRadius, 2f))
                {
                    float left = PathMover.Of(this).StepToward(t.transform.position, moveSpeed);
                    if (Time.time >= _logAt) { _logAt = Time.time + 10f; Debug.Log($"[Magnate] va a buscar a «{t.name}» ({left:0} m).");}
                    return;
                }
                Debug.Log($"[Magnate] da la bienvenida a «{t.name}» y van (cada uno andando) a la sala de yoga.");
                Mind m = t.GetComponent<Mind>();
                if (m != null) m.humores.Produce(Humor.Serotonina, 0.3f);   // orgullo/bienvenida
                if (yoga == null) { _newcomer.presented = true; _initPhase = 3; return; }
                _orch = WalkTogether("a la sala de yoga", t.transform, yoga);
                _initPhase = 1;
                return;
            case 1:   // la orquesta: cada uno anda SOLO a la sala (ella a un lado de la esterilla, él al otro) y se esperan
                if (yoga == null) { _initPhase = 3; return; }
                if (_orch == null) _orch = WalkTogether("a la sala de yoga", t.transform, yoga);   // (si la desactivaron a medias)
                _orch.Tick();
                if (Time.time >= _logAt) { _logAt = Time.time + 10f; Debug.Log($"[Magnate] camino a la sala de yoga con «{t.name}»: ella a {Flat(transform.position - yoga.position):0} m, él a {Flat(t.transform.position - yoga.position):0} m."); }
                if (!_orch.Done) return;
                _orch = null;
                _newcomer.presented = true;   // ya es del santuario: su iniciación de la sala empieza aquí
                // Lo ENTREGA al de turno de la sala: su primera clase OBLIGADA (docs/novela.md, «5. Sala de yoga»: upa-yoga de
                // cuello, paso a paso) y después el tanque al río de Sakshi (Berón 2026-10-27).
                AreaInitiator ya = AreaInitiator.Begin(AreaInitiator.Kind.Yoga, _newcomer, _a);
                Debug.Log($"[Magnate] entrega a «{t.name}» a {(ya != null ? ya.Who : "nadie")} (de turno en la sala de yoga) para su iniciación.");
                _initPhase = ya != null ? 2 : 4; _phaseDeadline = Time.time + 1200f; _logAt = Time.time + 8f;
                return;
            case 2:   // lo inicia el de turno de la sala (clase + tanque); ella espera cerca (con tope)
                if (AreaInitiator.Owns(_newcomer) && Time.time < _phaseDeadline) return;
                _initPhase = 3; _logAt = Time.time + 8f; _phaseDeadline = Time.time + 3600f;
                return;
            case 3:   // en el río: espera a que vuelva (lo juega el jugador; tope de una hora real = un día del santuario)
                if (Time.time < _phaseDeadline && (Time.time < _logAt || (MobWorldLoader.HasInstance && (MobWorldLoader.Instance.IsInMobWorld || MobWorldLoader.Instance.IsBusy)))) return;
                Debug.Log($"[Magnate] «{t.name}» vuelve del río: lo acompaña a la cocina.");
                _initPhase = 4; _orch = null;
                return;
            case 4:   // a la cocina: primero va hasta él (2 m) y después cada uno anda solo (orquesta)
                {
                    if (kitchen == null) { _initPhase = 5; return; }
                    if (_orch == null)
                    {
                        if (Flat(t.transform.position - transform.position) > 2f) { PathMover.Of(this).StepToward(t.transform.position, moveSpeed); return; }
                        _orch = WalkTogether("a la cocina", t.transform, kitchen);
                    }
                    _orch.Tick();
                    if (!_orch.Done) return;
                    _orch = null;
                    _initPhase = 5;
                    return;
                }
            case 5:   // lo ENTREGA al de turno de la cocina: la tanda por mayor y el tanque a su primer microcosmos
                {
                    AreaInitiator ki = AreaInitiator.Begin(AreaInitiator.Kind.Kitchen, _newcomer, _a);
                    if (ki == null && kitchen != null) Queue(t).EnqueueOwn(ActionSpells.Walk(kitchen));   // sin nadie: el jefe de cocina lo recibe (KitchenBoss)
                    Debug.Log($"[Magnate] deja a «{t.name}» en la cocina con {(ki != null ? ki.Who : "el jefe de cocina")} para su iniciación.");
                    _initPhase = 6; _phaseDeadline = Time.time + 3600f; _logAt = Time.time + 8f;
                    return;
                }
            case 6:   // la cocina (tanda + tanque + su microcosmos): espera a que vuelva (con tope: un día del santuario)
                {
                    bool away = AreaInitiator.Owns(_newcomer) || (MobWorldLoader.HasInstance && (MobWorldLoader.Instance.IsInMobWorld || MobWorldLoader.Instance.IsBusy));
                    if ((away || Time.time < _logAt) && Time.time < _phaseDeadline) return;
                    Debug.Log($"[Magnate] «{t.name}» vuelve de la cocina: lo lleva al HUERTO (preparar el suelo).");
                    _initPhase = 7; _orch = null;
                    return;
                }
            case 7:   // al huerto: primero va hasta él (2 m) y después cada uno anda solo (orquesta)
                {
                    Transform garden = gardenSpot != null ? gardenSpot : FindSpot("Garden_Area");
                    if (garden == null) { _initPhase = 8; return; }
                    if (_orch == null)
                    {
                        if (Flat(t.transform.position - transform.position) > 2f) { PathMover.Of(this).StepToward(t.transform.position, moveSpeed); return; }
                        _orch = WalkTogether("al huerto", t.transform, garden);
                    }
                    _orch.Tick();
                    if (!_orch.Done) return;
                    _orch = null;
                    _initPhase = 8;
                    return;
                }
            case 8:   // lo ENTREGA al de turno del huerto: preparar el suelo del bancal y el tanque al microcosmos del huerto
                {
                    AreaInitiator gi = _newcomer.gardenTrained ? null : AreaInitiator.Begin(AreaInitiator.Kind.Garden, _newcomer, _a);
                    Debug.Log($"[Magnate] deja a «{t.name}» en el huerto con {(gi != null ? gi.Who : "nadie")} para su iniciación (preparar el suelo).");
                    _initPhase = 9;
                    _guiding.Remove(_newcomer);
                    _newcomer = null; _initiating = null;
                    return;
                }
        }
    }

    /// <summary>¿Va andando con el recién llegado de un área a otra (o hacia él)? Entonces nadie más la conduce.</summary>
    public bool IsEscorting => _newcomer != null && (_initPhase == 0 || _initPhase == 1 || _initPhase == 4 || _initPhase == 7);

    // ORQUESTA de la Magnate (Berón 2026-10-29): ya no lo lleva a tirones ni le encola «andar»; cada uno anda SOLO a su sitio
    // junto al destino (él a un lado, ella al otro, a 2,4 m); él avisa al llegar y ella, ya en el suyo, lo espera.
    Orchestra _orch;
    Orchestra WalkTogether(string label, Transform who, Transform to)
    {
        var o = new Orchestra($"la Magnate {label}", 2f);
        Orchestra.Role nw = o.Join(who.name, who), me = o.Join("la Magnate", transform);
        Vector3 side = Vector3.ProjectOnPlane(to.right, Vector3.up).normalized;
        if (side.sqrMagnitude < 0.01f) side = Vector3.right;
        o.Movement(label);
        o.GoTo(nw, () => to.position + side * 1.2f, () => transform.position, $"va solo {label}");
        o.Emit(nw, "llegó");
        o.GoTo(me, () => to.position - side * 1.2f, () => who.position, $"va {label}");
        o.WaitFor(me, "llegó");
        return o;
    }

    void LateUpdate() { if (_orch != null) _orch.LateTick(); }
    void OnDisable() { _orch?.Stop(); _orch = null; }

    static ActionQueue Queue(Anima t) => t.GetOrAdd<ActionQueue>();
    static float Flat(Vector3 v) { v.y = 0f; return v.magnitude; }

    static Transform FindSpot(string n) { GameObject g = GameObject.Find(n); return g != null ? g.transform : null; }

    // Proteger la vida: a su alrededor lanza PACIFICACIÓN (quita humores negativos, sube positivos, quita `aware`) →
    // el depredador suelta a su presa. Throttled (~0.5 s) para no castear cada frame sobre muchos.
    void Pacify()
    {
        if (Time.time < _nextPacify) return;
        _nextPacify = Time.time + 0.5f;
        int n = Physics.OverlapSphereNonAlloc(transform.position, pacifyRadius, _hits);
        for (int i = 0; i < n; i++)
        {
            Anima a = _hits[i] != null ? _hits[i].GetComponentInParent<Anima>() : null;
            if (a == null || a == _a) continue;
            if (a is ITarget it && _pacify.CanCast(_a, it)) _pacify.Cast(_a, it);
            else a.aware = false;
        }
    }

    // El que se queda atrás = menor nivel integral (SoulLevels). Deja fuera a la propia Magnate.
    Anima FindLaggard()
    {
        Anima worst = null; int worstLv = int.MaxValue;
        foreach (CharacterLevel cl in FindObjectsOfType<CharacterLevel>())
        {
            Anima a = cl.GetComponent<Anima>();
            if (a == null || a == _a || a.death || a.GetComponent<Magnate>() != null) continue;
            if (FlyingCarpet.Riding(a.gameObject)) continue;   // volando en alfombra no se da clase
            if (_taught.TryGetValue(a, out float at) && Time.time - at < classCooldown) continue;   // ya tuvo su clase hace poco
            if (!IsMember(a)) continue;   // un recién llegado sin la iniciación de Linduarda aún NO es del santuario
            if (WorkdayDirector.Instance != null && WorkdayDirector.Instance.OnShift(a.gameObject)) continue;   // en su jornada: no se le interrumpe
            if (cl.SoulLevels < worstLv) { worstLv = cl.SoulLevels; worst = a; }
        }
        return worst;
    }

    /// <summary>¿Es ya del santuario? Un aspirante (SanctuaryOnboarding) lo es cuando Linduarda lo certifica y la Magnate
    /// lo ha iniciado; los demás personajes ya lo son.</summary>
    public static bool IsMember(Anima a)
    {
        SanctuaryOnboarding o = a != null ? a.GetComponent<SanctuaryOnboarding>() : null;
        return o == null || (o.apt && o.presented);
    }

    void GoTeach()
    {
        Vector3 to = _student.transform.position - transform.position; to.y = 0f;
        if (to.magnitude > teachRadius)
        {
            PathMover.Of(this).StepToward(_student.transform.position, moveSpeed);   // la guardiana se acerca (por el camino, nadando entre islas)
            return;
        }
        if (FlyingCarpet.Riding(gameObject)) return;   // ella en una alfombra: primero aterriza
        Teach(_student);
        _student = null;
    }

    // Enseñar: primero mira si le falta algo vital; si no, le da el RECUERDO del yoga + el Saludo al Sol + XP/ánimo.
    void Teach(Anima t, bool onboarding = false)
    {
        if (!onboarding && (t.sleepiness > 0.7f || t.stress > 0.7f || (t is Animal an && an.hungry > 0.7f)))
        { Debug.Log($"[Magnate] «{t.name}» necesita comer/descansar antes de aprender."); return; }

        // Ya hay una clase en curso: no se reinicia ni se apila (la TeachingSession se destruye al terminar).
        // La PRIMERA clase del recién llegado (iniciación) va antes que cualquier otra: la que hubiera, se deja para luego.
        TeachingSession busy = GetComponent<TeachingSession>();
        if (busy != null && onboarding && TeachingSession.For(t) == null)
        {
            // Deja la otra clase DE VERDAD: sin esto seguía con sus pasos en cola (quedaba en la postura del saludo al sol,
            // sanchalanasana) y no demostraba el upa-yoga al recién llegado.
            Destroy(busy); busy = null;
            if (TryGetComponent(out ActionQueue mq)) mq.ClearOwn();
            DailyPoses.Stand().ApplyFinal(_a);
        }
        if (busy != null) return;

        // POR NIVEL: lo PRIMERO que se enseña es upa-yoga de cuello; luego Yoga Namaskar; luego el Saludo al Sol.
        // (El Grimoire se aprende al COMPLETAR la lección, en TeachingSession.Finish, no al empezarla.)
        if (!t.TryGetComponent(out Grimoire g)) g = t.gameObject.AddComponent<Grimoire>();
        System.Func<List<QueuedSpell>> builder; string learn, label;
        if (!g.Knows("upa-yoga"))           { builder = () => UpaYoga.NeckSequence();   learn = "upa-yoga";      label = "upa-yoga de cuello"; }
        else if (!g.Knows("yoga-namaskar")) { builder = () => YogaNamaskar.Sequence();  learn = "yoga-namaskar"; label = "Yoga Namaskar"; }
        else                                { builder = () => SunSalutation.Sequence(); learn = "yoga";          label = "el Saludo al Sol"; }

        // ENSEÑANZA INTERACTIVA: la Magnate demuestra cada paso, se lo PIDE al alumno y ESPERA a que lo realice.
        _taught[t] = Time.time;
        TeachingSession.Begin(_a, t, learn, builder);
        CharacterLevel cl = t.GetComponent<CharacterLevel>();
        if (cl != null) cl.GainYogaXp(10f);                          // potencia el avance (marga de Yoga)
        Mind m = t.GetComponent<Mind>();
        if (m != null) m.humores.Produce(Humor.Serotonina, 0.2f);   // el ánimo de practicar sube (ThoughtSpell-like)
        Debug.Log($"[Magnate] enseña {label} a «{t.name}» (interactivo).");
    }
}
