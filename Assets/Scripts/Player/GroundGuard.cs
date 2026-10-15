using UnityEngine;

/// <summary>
/// SALVAGUARDA DE SUELO: si el cuerpo queda por DEBAJO del suelo (atravesó un collider, lo empujó un movimiento por
/// transform, aparece dentro de un objeto…), lo recoloca encima del suelo más cercano en su vertical y lo registra.
/// Visto en pruebas en vivo: el jugador acabó en y = −2,7 bajo el Huerto y ya no podía moverse con normalidad.
/// </summary>
public class GroundGuard : MonoBehaviour
{
    [Tooltip("Por debajo de esta altura respecto al suelo detectado se considera enterrado.")]
    public float buriedBelow = 0.5f;
    public float checkEvery = 0.5f;

    float _next;
    CharacterController _cc;

    void Awake() => _cc = GetComponent<CharacterController>();

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + checkEvery;
        if (_cc != null && !_cc.enabled) return;   // trepando / en transición de escena: no tocar

        Vector3 p = transform.position;
        // El suelo REAL de esta columna: el primer collider sólido bajando desde muy arriba (ignora triggers y a sí mismo).
        RaycastHit[] hits = Physics.RaycastAll(new Vector3(p.x, p.y + 60f, p.z), Vector3.down, 200f, ~0, QueryTriggerInteraction.Ignore);
        float feet = _cc != null ? p.y + _cc.center.y - _cc.height * 0.5f : p.y;
        float best = float.NegativeInfinity;
        foreach (RaycastHit h in hits)   // lo más alto, pero no un techo/árbol muy por encima de la cabeza
            if (!h.transform.IsChildOf(transform) && h.point.y <= feet + 3.5f && h.point.y > best) best = h.point.y;
        if (float.IsNegativeInfinity(best)) return;

        if (feet >= best - buriedBelow) return;

        float lift = best - feet + 0.05f;
        if (_cc != null) _cc.enabled = false;
        transform.position = p + Vector3.up * lift;
        if (_cc != null) _cc.enabled = true;
        Debug.Log($"[Suelo] «{name}» estaba {(best - feet):0.0} m bajo el suelo en ({p.x:0},{p.z:0}) → recolocado encima.");
    }
}
