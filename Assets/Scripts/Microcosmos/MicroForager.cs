using UnityEngine;

/// <summary>
/// RECOLECTORA de la tribu (microcosmos de la Cocina): busca el resto de comida más cercano, lo carga sobre la cabeza y
/// lo lleva a la guarida; vuelta a empezar.
/// </summary>
public class MicroForager : MonoBehaviour
{
    public float speed = 3f;
    public int delivered;
    MicroFood _target;

    void Update()
    {
        if (_target == null || (_target.carrier != null && _target.carrier != transform)) { _target = Pick(); if (_target == null) return; }
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

    MicroFood Pick()
    {
        MicroFood best = null; float bd = float.MaxValue;
        foreach (MicroFood f in MicroFood.All)
        {
            if (f.carrier != null || f.leaf) continue;
            float d = (f.transform.position - transform.position).sqrMagnitude;
            if (d < bd) { bd = d; best = f; }
        }
        return best;
    }
}
