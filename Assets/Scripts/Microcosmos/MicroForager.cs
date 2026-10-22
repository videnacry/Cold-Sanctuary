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
    MicroFood _target;
    Affordance _source;
    GameObject _load;

    void Update()
    {
        if (_load != null) { Deliver(); return; }
        if (_source != null) { Harvest(); return; }
        if (_target == null || (_target.carrier != null && _target.carrier != transform)) { if (!Pick()) return; if (_source != null) return; }
        if (_target.carrier == null)
        {
            if (!MicroMove.Step(transform, _target.transform.position, speed)) return;
            _target.carrier = transform; _target.transform.SetParent(transform, true);
            _target.transform.localPosition = Vector3.up * 2.1f;   // sobre la cabeza
            return;
        }
        MicroLair lair = MicroLair.Nearest(transform.position);
        if (lair == null) return;
        if (!MicroMove.Step(transform, lair.transform.position, speed, 3f)) return;
        lair.stock++; delivered++;
        _target.Respawn(); _target = null;
    }

    // Ir a la planta/pulgón y llevarse una porción.
    void Harvest()
    {
        if (!_source.isActiveAndEnabled || _source.Stock < 0.2f) { _source = null; return; }
        if (!MicroMove.Step(transform, _source.transform.position, speed, 2.5f)) return;
        if (_source.Take(_source.kind == AffordanceKind.Honeydew ? 0.3f : 1f) <= 0f) { _source = null; return; }
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
        if (!MicroMove.Step(transform, lair.transform.position, speed, 3f)) return;
        lair.stock++; delivered++;
        Destroy(_load); _load = null;
    }

    bool Pick()
    {
        MicroFood best = null; float bd = float.MaxValue;
        foreach (MicroFood f in MicroFood.All)
        {
            if (f.carrier != null || f.leaf) continue;
            float d = (f.transform.position - transform.position).sqrMagnitude;
            if (d < bd) { bd = d; best = f; }
        }
        Affordance seed = Affordance.Nearest(AffordanceKind.Seed, transform.position, Mathf.Sqrt(bd));
        if (seed != null) bd = (seed.transform.position - transform.position).sqrMagnitude;
        Affordance milk = Affordance.Nearest(AffordanceKind.Honeydew, transform.position, Mathf.Sqrt(bd));
        if (milk != null && milk.Stock < 0.3f) milk = null;
        _source = milk != null ? milk : seed;
        _target = _source != null ? null : best;
        return _source != null || _target != null;
    }
}
