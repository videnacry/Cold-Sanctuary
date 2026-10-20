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

    Anima _a;
    PacifySpell _pacify;
    float _nextTeach, _nextPacify;
    Anima _student;
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

    // HECHIZO DE INICIACIÓN DE LA MAGNATE (cuando Linduarda la llama: el aspirante ya es apto): va a buscarlo a la
    // enfermería, se lo LLEVA (camina con él) a la SALA DE YOGA, allí le da su primera clase interactiva de upa-yoga
    // (TeachingSession: ella hace cada paso y le pide el suyo) y al terminar lo encamina a la cocina. Hasta entonces el
    // recién llegado NO es del santuario y ella no lo tiene en cuenta para enseñar al rezagado.
    Anima _initiating;
    float _logAt;
    int _initPhase;   // 0 = ir a buscarlo · 1 = caminar juntos a la sala de yoga · 2 = clase · 3 = a la cocina

    void GoPresent()
    {
        if (_newcomer == null) return;
        Anima t = _newcomer.GetComponentInParent<Anima>();
        if (t == null) { _newcomer.presented = true; _newcomer = null; return; }
        Transform yoga = yogaSpot != null ? yogaSpot : FindSpot("SalaYoga");
        Transform kitchen = kitchenSpot != null ? kitchenSpot : FindSpot("CocinaSpot");

        if (_initiating != t) { _initiating = t; _initPhase = 0; Debug.Log($"[Magnate] Linduarda la llama: va a iniciar a «{t.name}»."); }
        switch (_initPhase)
        {
            case 0:   // ir a buscarlo
                if (Flat(t.transform.position - transform.position) > teachRadius)
                {
                    float left = PathMover.Of(this).StepToward(t.transform.position, moveSpeed);
                    if (Time.time >= _logAt) { _logAt = Time.time + 10f; Debug.Log($"[Magnate] va a buscar a «{t.name}» ({left:0} m).");}
                    return;
                }
                Debug.Log($"[Magnate] da la bienvenida a «{t.name}» y se lo lleva a la sala de yoga (hechizo de iniciación).");
                Mind m = t.GetComponent<Mind>();
                if (m != null) m.humores.Produce(Humor.Serotonina, 0.3f);   // orgullo/bienvenida
                if (yoga != null) Queue(t).EnqueueOwn(ActionSpells.Walk(yoga));
                _initPhase = 1;
                return;
            case 1:   // caminar juntos (ella delante, a su paso)
                if (yoga == null) { _initPhase = 2; return; }
                float mLeft = PathMover.Of(this).StepToward(yoga.position, moveSpeed * 0.6f);
                if (Time.time >= _logAt) { _logAt = Time.time + 10f; Debug.Log($"[Magnate] camino a la sala de yoga con «{t.name}»: ella a {mLeft:0} m, él a {Flat(t.transform.position - yoga.position):0} m."); }
                if (Flat(t.transform.position - yoga.position) < 4f && Flat(transform.position - yoga.position) < 5f)
                {
                    _newcomer.presented = true;   // ya es del santuario: su primera clase empieza aquí
                    Teach(t);
                    _initPhase = 2;
                }
                return;
            case 2:   // clase interactiva en curso
                if (TeachingSession.For(t) != null) return;
                if (kitchen != null) Queue(t).EnqueueOwn(ActionSpells.Walk(kitchen));
                Debug.Log($"[Magnate] «{t.name}» terminó su primera clase: ahora va a la cocina a aprender un oficio.");
                _initPhase = 3;
                _newcomer = null; _initiating = null;
                return;
        }
    }

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
            if (!IsMember(a)) continue;   // un recién llegado sin la iniciación de Linduarda aún NO es del santuario
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
        Teach(_student);
        _student = null;
    }

    // Enseñar: primero mira si le falta algo vital; si no, le da el RECUERDO del yoga + el Saludo al Sol + XP/ánimo.
    void Teach(Anima t)
    {
        if (t.sleepiness > 0.7f || t.stress > 0.7f || (t is Animal an && an.hungry > 0.7f))
        { Debug.Log($"[Magnate] «{t.name}» necesita comer/descansar antes de aprender."); return; }

        // Ya hay una clase en curso: no se reinicia ni se apila (la TeachingSession se destruye al terminar).
        if (GetComponent<TeachingSession>() != null) return;

        // POR NIVEL: lo PRIMERO que se enseña es upa-yoga de cuello; luego Yoga Namaskar; luego el Saludo al Sol.
        // (El Grimoire se aprende al COMPLETAR la lección, en TeachingSession.Finish, no al empezarla.)
        if (!t.TryGetComponent(out Grimoire g)) g = t.gameObject.AddComponent<Grimoire>();
        System.Func<List<QueuedSpell>> builder; string learn, label;
        if (!g.Knows("upa-yoga"))           { builder = () => UpaYoga.NeckSequence();   learn = "upa-yoga";      label = "upa-yoga de cuello"; }
        else if (!g.Knows("yoga-namaskar")) { builder = () => YogaNamaskar.Sequence();  learn = "yoga-namaskar"; label = "Yoga Namaskar"; }
        else                                { builder = () => SunSalutation.Sequence(); learn = "yoga";          label = "el Saludo al Sol"; }

        // ENSEÑANZA INTERACTIVA: la Magnate demuestra cada paso, se lo PIDE al alumno y ESPERA a que lo realice.
        TeachingSession.Begin(_a, t, learn, builder);
        CharacterLevel cl = t.GetComponent<CharacterLevel>();
        if (cl != null) cl.GainYogaXp(10f);                          // potencia el avance (marga de Yoga)
        Mind m = t.GetComponent<Mind>();
        if (m != null) m.humores.Produce(Humor.Serotonina, 0.2f);   // el ánimo de practicar sube (ThoughtSpell-like)
        Debug.Log($"[Magnate] enseña {label} a «{t.name}» (interactivo).");
    }
}
