using UnityEngine;

/// <summary>
/// CULTIVADORA DE HONGO (microcosmos del Huerto): lleva hojas al bancal de hongos y lo abona; los hongos crecen. Es lo
/// que La Sembradora enseña a la tribu: pasar de recoger a cultivar. Las hojas las recoge del suelo o las corta de los
/// HELECHOS de la flora (Affordance Leaf), como las hormigas cortadoras que cultivan su hongo con hojas frescas.
/// </summary>
public class MicroGardener : MonoBehaviour
{
    public float speed = 3f;
    public Transform bed;
    MicroFood _leaf;
    Affordance _fern;
    GameObject _cut;

    void Update()
    {
        if (bed == null) return;
        if (_cut != null) { if (MicroMove.Step(transform, bed.position, speed, 3f)) { Feed(); Destroy(_cut); _cut = null; } return; }
        if (_fern != null)
        {
            if (!_fern.isActiveAndEnabled || _fern.Stock < 0.5f) { _fern = null; return; }
            if (!MicroMove.Step(transform, _fern.transform.position, speed, 2.5f)) return;
            if (_fern.Take(1f) > 0f)
            {
                _cut = GameObject.CreatePrimitive(PrimitiveType.Cube); _cut.name = "Hoja_cortada";
                Destroy(_cut.GetComponent<Collider>());
                _cut.transform.SetParent(transform, false); _cut.transform.localPosition = Vector3.up * 2.1f; _cut.transform.localScale = new Vector3(1.6f, 0.08f, 1f);
                _cut.GetComponent<Renderer>().material.color = new Color(0.3f, 0.55f, 0.25f);
            }
            _fern = null;
            return;
        }
        if (_leaf == null || (_leaf.carrier != null && _leaf.carrier != transform))
        {
            float bd = float.MaxValue; _leaf = null;
            foreach (MicroFood f in MicroFood.All)
            {
                if (!f.leaf || f.carrier != null) continue;
                float d = (f.transform.position - transform.position).sqrMagnitude;
                if (d < bd) { bd = d; _leaf = f; }
            }
            Affordance fern = Affordance.Nearest(AffordanceKind.Leaf, transform.position, Mathf.Sqrt(bd));
            if (fern != null) { _fern = fern; _leaf = null; return; }
            if (_leaf == null) return;
        }
        if (_leaf.carrier == null)
        {
            if (!MicroMove.Step(transform, _leaf.transform.position, speed)) return;
            _leaf.carrier = transform; _leaf.transform.SetParent(transform, true); _leaf.transform.localPosition = Vector3.up * 2.1f;
            return;
        }
        if (!MicroMove.Step(transform, bed.position, speed, 3f)) return;
        Feed();
        _leaf.Respawn(); _leaf = null;
    }

    void Feed()
    {
        foreach (Transform m in bed) if (m.name.StartsWith("Hongo")) m.localScale = Vector3.Min(m.localScale * 1.04f, Vector3.one * 3f);   // abona: crecen
    }
}
