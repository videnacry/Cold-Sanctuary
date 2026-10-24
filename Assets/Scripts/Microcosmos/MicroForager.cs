using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RECOLECTORA de la tribu (los mirmidones): busca lo más cercano que una hormiga real llevaría al nido —un resto de
/// comida, una SEMILLA de la flora (las hormigas granívoras cosechan semillas: MacMahon et al. 2000) o la MELAZA de un
/// pulgón que ordeña (Stadler & Dixon 2005)—, lo carga sobre la cabeza y lo lleva a la guarida; vuelta a empezar.
/// La flora y los pulgones son <see cref="Affordance"/> (docs/natural-interactions.md): se gastan y rebrotan.
/// </summary>
public class MicroForager : MonoBehaviour
{
    public float speed = 3f;
    public int delivered;
    [Tooltip("Hasta dónde va a ordeñar un pulgón aunque haya semillas más cerca.")]
    public float milkRadius = 250f;   // todo el microcosmos: un pulgón con gota compensa la caminata
    MicroFood _target;
    Affordance _source;
    GameObject _load;

    // INALCANZABLES: por NavMesh el agente se para donde puede (tras una pared, en otro piso) y no llegaba nunca → la
    // recolectora se quedaba plantada para siempre (0 entregas en Hospital/Patanjali). Si no avanza en `stallSeconds`,
    // abandona ese objetivo y lo evita un rato.
    [Tooltip("Segundos sin acercarse a su objetivo antes de darlo por inalcanzable.")]
    public float stallSeconds = 6f;
    readonly Dictionary<Object, float> _skip = new Dictionary<Object, float>();
    Object _goal;
    float _bestDist, _stallSince, _goalSince;

    // Un paso hacia el objetivo, vigilando que se acerque. null = inalcanzable (lo apunta y lo suelta el llamador).
    bool? Go(Object goal, Vector3 to, float arrive)
    {
        if (goal != _goal) { _goal = goal; _bestDist = float.MaxValue; _stallSince = _goalSince = Time.time; }
        if (MicroMove.Step(transform, to, speed, arrive)) { _goal = null; return true; }
        Vector3 d = to - transform.position; d.y = 0f;
        if (d.magnitude < _bestDist - 0.3f) { _bestDist = d.magnitude; _stallSince = Time.time; }
        // Sin avanzar en stallSeconds, o demasiado rato con el mismo (resbalando contra una pared el agente «avanza» a ratos).
        else if (Time.time - _stallSince > stallSeconds || Time.time - _goalSince > stallSeconds * 8f)
        {
            _skip[goal] = Time.time + 90f; _goal = null;
            if (++_fails >= 3) CrossCountry();
            return null;
        }
        return false;
    }

    int _fails;
    static int _milked;

    // Tres objetivos seguidos sin camino por el NavMesh (su trozo de suelo está aislado: Patanjali, Hospital): deja el
    // NavMesh y va a CAMPO TRAVÉS, en línea recta, como un insecto que trepa por encima de lo que encuentra.
    void CrossCountry()
    {
        var ag = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (ag == null || !ag.enabled) return;
        ag.enabled = false;
        _skip.Clear();
        Debug.Log($"[Micro] «{name}» no encuentra camino por el suelo: va a campo través.");
    }

    bool Skipped(Object o) => o != null && _skip.TryGetValue(o, out float until) && Time.time < until;

    void Update()
    {
        if (_load != null) { Deliver(); return; }
        if (_source != null) { Harvest(); return; }
        if (_target == null || (_target.carrier != null && _target.carrier != transform)) { if (!Pick()) return; if (_source != null) return; }
        if (_target.carrier == null)
        {
            bool? got = Go(_target, _target.transform.position, 1.2f);
            if (got == null) { _target = null; return; }
            if (got == false) return;
            _target.carrier = transform; _target.transform.SetParent(transform, true);
            _target.transform.localPosition = Vector3.up * 2.1f;   // sobre la cabeza
            return;
        }
        MicroLair lair = MicroLair.Nearest(transform.position);
        if (lair == null) return;
        if (Go(lair, lair.transform.position, 3f) != true) return;
        lair.stock++; delivered++; _fails = 0;
        _target.Respawn(); _target = null;
    }

    // Ir a la planta/pulgón y llevarse una porción.
    void Harvest()
    {
        if (!_source.isActiveAndEnabled || _source.Stock < 0.2f) { _source = null; return; }
        bool? at = Go(_source, _source.transform.position, 2.5f);
        if (at == null) { _source = null; return; }
        if (at == false) return;
        if (_source.Take(_source.kind == AffordanceKind.Honeydew ? 0.3f : 1f) <= 0f) { _source = null; return; }
        if (_source.kind == AffordanceKind.Honeydew && _milked++ < 5) Debug.Log($"[Micro] «{name}» ordeña la melaza de «{_source.name}» y la lleva a la guarida.");
        _load = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _load.name = _source.kind == AffordanceKind.Honeydew ? "Gota_melaza" : "Semilla";
        Destroy(_load.GetComponent<Collider>());
        _load.transform.SetParent(transform, false);
        _load.transform.localPosition = Vector3.up * 2.1f; _load.transform.localScale = new Vector3(0.5f, 0.4f, 0.6f);
        _load.GetComponent<Renderer>().material.color = _source.kind == AffordanceKind.Honeydew ? new Color(0.95f, 0.75f, 0.2f) : new Color(0.75f, 0.6f, 0.35f);
        _source = null;
    }

    void Deliver()
    {
        MicroLair lair = MicroLair.Nearest(transform.position);
        if (lair == null) return;
        if (Go(lair, lair.transform.position, 3f) != true) return;
        lair.stock++; delivered++; _fails = 0;
        Destroy(_load); _load = null;
    }

    bool Pick()
    {
        MicroFood best = null; float bd = float.MaxValue;
        foreach (MicroFood f in MicroFood.All)
        {
            if (f.carrier != null || f.leaf || Skipped(f)) continue;
            float d = (f.transform.position - transform.position).sqrMagnitude;
            if (d < bd) { bd = d; best = f; }
        }
        Affordance seed = null;
        foreach (Affordance a in Affordance.Of(AffordanceKind.Seed))
        {
            if (a == null || !a.isActiveAndEnabled || !a.Available(null) || Skipped(a)) continue;
            float d = (a.transform.position - transform.position).sqrMagnitude;
            if (d < bd) { bd = d; seed = a; }
        }
        // La melaza vale más que una semilla (azúcar líquido, recurso renovable que la colonia defiende): se va a por un
        // pulgón con gota aunque haya semillas más cerca, dentro de un radio de ordeño.
        // Solo desde el Neolítico (los mirmidones son humanos: ordeñar = ganadería lechera, Eras.Capability.Dairying).
        Affordance milk = null; float md = Mathf.Max(bd, milkRadius * milkRadius);
        foreach (Affordance a in Affordance.Of(AffordanceKind.Honeydew))
        {
            if (!Eras.Allows(Eras.Capability.Dairying)) break;
            if (a == null || !a.isActiveAndEnabled || a.Stock < 0.3f || !a.Free(null) || Skipped(a)) continue;
            float d = (a.transform.position - transform.position).sqrMagnitude;
            if (d < md) { md = d; milk = a; }
        }
        _source = milk != null ? milk : seed;
        _target = _source != null ? null : best;
        return _source != null || _target != null;
    }
}
