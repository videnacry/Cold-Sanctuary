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
        AiBrain ai = GetComponent<AiBrain>() ?? gameObject.AddComponent<AiBrain>();
        ai.selfRelevance = 99999f;   // nadie la posee
        _pacify = GetComponent<PacifySpell>() ?? gameObject.AddComponent<PacifySpell>();
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
        foreach (SanctuaryOnboarding o in FindObjectsOfType<SanctuaryOnboarding>())
        {
            if (o == null || !o.apt || o.presented) continue;
            float sq = (o.transform.position - transform.position).sqrMagnitude;
            if (sq < bestSq) { bestSq = sq; best = o; }
        }
        return best;
    }

    void GoPresent()
    {
        if (_newcomer == null) return;
        Anima t = _newcomer.GetComponentInParent<Anima>();
        if (t == null) { _newcomer.presented = true; _newcomer = null; return; }
        Vector3 to = t.transform.position - transform.position; to.y = 0f;
        if (to.magnitude > teachRadius) { transform.position += to.normalized * moveSpeed * Time.deltaTime; return; }

        // Ceremonia: la maestra lo presenta como nuevo miembro, le da el upa-yoga y lo ENCAMINA a aprender un área (cocina).
        Debug.Log($"[Magnate] presenta a «{t.name}» como nuevo miembro del santuario. Le enseña upa-yoga y lo lleva a la cocina.");
        Teach(t);                              // upa-yoga: el recuerdo del Saludo al Sol + ánimo (ver Teach)
        Mind m = t.GetComponent<Mind>();
        if (m != null) m.humores.Produce(Humor.Serotonina, 0.3f);   // orgullo/bienvenida
        LeadToYogaThenKitchen(t);              // y lo LLEVA: camina a la sala de yoga, practica, y va a la cocina
        _newcomer.presented = true;
        _newcomer = null;
    }

    // Superhechizo de GUÍA: encola en el nuevo caminar→sala de yoga (upa-yoga: Saludo al Sol)→caminar→cocina.
    // Reutiliza ActionSpells.Walk (= levantarse + acercarse) y SunSalutation. En el carril propio (prioritario) del cuerpo.
    void LeadToYogaThenKitchen(Anima t)
    {
        ActionQueue q = t.GetComponent<ActionQueue>() ?? t.gameObject.AddComponent<ActionQueue>();
        Transform yoga = yogaSpot != null ? yogaSpot : FindSpot("SalaYoga");
        Transform kitchen = kitchenSpot != null ? kitchenSpot : FindSpot("CocinaSpot");
        if (yoga != null) q.EnqueueOwn(ActionSpells.Walk(yoga));
        foreach (QueuedSpell s in SunSalutation.Sequence()) q.EnqueueOwn(s);   // al llegar, practica el Saludo al Sol
        if (kitchen != null) q.EnqueueOwn(ActionSpells.Walk(kitchen));
        Debug.Log($"[Magnate] guía a «{t.name}»: sala de yoga → upa-yoga → cocina.");
    }

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
            if (cl.SoulLevels < worstLv) { worstLv = cl.SoulLevels; worst = a; }
        }
        return worst;
    }

    void GoTeach()
    {
        Vector3 to = _student.transform.position - transform.position; to.y = 0f;
        if (to.magnitude > teachRadius)
        {
            transform.position += to.normalized * moveSpeed * Time.deltaTime;   // la guardiana se acerca
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

        Grimoire g = t.GetComponent<Grimoire>() ?? t.gameObject.AddComponent<Grimoire>();
        g.Learn("yoga");                                             // el recuerdo de la asana
        ActionQueue q = t.GetComponent<ActionQueue>() ?? t.gameObject.AddComponent<ActionQueue>();
        foreach (QueuedSpell s in SunSalutation.Sequence()) q.EnqueuePlayer(s);
        CharacterLevel cl = t.GetComponent<CharacterLevel>();
        if (cl != null) cl.GainYogaXp(10f);                          // potencia el avance (marga de Yoga)
        Mind m = t.GetComponent<Mind>();
        if (m != null) m.humores.Produce(Humor.Serotonina, 0.2f);   // el ánimo de practicar sube (ThoughtSpell-like)
        Debug.Log($"[Magnate] enseña el Saludo al Sol a «{t.name}».");
    }
}
