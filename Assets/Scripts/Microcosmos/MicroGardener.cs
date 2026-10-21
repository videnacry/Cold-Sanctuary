using UnityEngine;

/// <summary>
/// CULTIVADORA DE HONGO (microcosmos del Huerto): lleva hojas al bancal de hongos y lo abona; los hongos crecen. Es lo
/// que La Sembradora enseña a la tribu: pasar de recoger a cultivar.
/// </summary>
public class MicroGardener : MonoBehaviour
{
    public float speed = 3f;
    public Transform bed;
    MicroFood _leaf;

    void Update()
    {
        if (bed == null) return;
        if (_leaf == null || (_leaf.carrier != null && _leaf.carrier != transform))
        {
            float bd = float.MaxValue; _leaf = null;
            foreach (MicroFood f in MicroFood.All)
            {
                if (!f.leaf || f.carrier != null) continue;
                float d = (f.transform.position - transform.position).sqrMagnitude;
                if (d < bd) { bd = d; _leaf = f; }
            }
            if (_leaf == null) return;
        }
        if (_leaf.carrier == null)
        {
            if (!MicroMove.Step(transform, _leaf.transform.position, speed)) return;
            _leaf.carrier = transform; _leaf.transform.SetParent(transform, true); _leaf.transform.localPosition = Vector3.up * 2.1f;
            return;
        }
        if (!MicroMove.Step(transform, bed.position, speed, 3f)) return;
        foreach (Transform m in bed) if (m.name.StartsWith("Hongo")) m.localScale = Vector3.Min(m.localScale * 1.04f, Vector3.one * 3f);   // abona: crecen
        _leaf.Respawn(); _leaf = null;
    }
}
