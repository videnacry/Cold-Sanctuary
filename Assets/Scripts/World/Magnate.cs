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

    Anima _a;
    PacifySpell _pacify;
    float _nextTeach, _nextPacify;
    Anima _student;
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

        if (Time.time >= _nextTeach) { _nextTeach = Time.time + teachInterval; _student = FindLaggard(); }
        if (_student != null) GoTeach();
    }

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
