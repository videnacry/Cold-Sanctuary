using UnityEngine;

/// <summary>
/// LA LÍDER de la tribu (Medea renacida, microcosmos de la Cocina): una hormiga muy poderosa que manda desde la guarida;
/// cada cierto tiempo va a ver a NASATYA (Ambrosio renacido) y se queda con él un rato: con él es inexplicablemente blanda.
/// </summary>
public class MicroLeader : MonoBehaviour
{
    public Transform lair, nasatya;
    public float speed = 3.5f;
    float _next, _stayUntil = -1f;
    bool _going;

    void Start() { _next = Time.time + 12f; }

    void Update()
    {
        if (_going && nasatya != null)
        {
            if (_stayUntil < 0f)
            {
                if (MicroMove.Step(transform, nasatya.position, speed, 2.5f)) { _stayUntil = Time.time + 6f; Debug.Log("[Microcosmos] La líder se queda junto a Nasatya: con él es blanda."); }
                return;
            }
            if (Time.time < _stayUntil) return;
            _going = false; _stayUntil = -1f; _next = Time.time + 25f;
            return;
        }
        if (lair != null) MicroMove.Step(transform, lair.position + lair.forward * 4f, speed);
        if (Time.time >= _next) _going = true;
    }
}
