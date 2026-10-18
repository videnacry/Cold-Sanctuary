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
        // PIES: con CharacterController, su base; sin él (compañeros), la base de su cuerpo de cubos o de su cápsula. Antes
        // se tomaba el pivote (≈1 m sobre el suelo) como pies: cada raíz o tronco "por debajo" lo subía un poco más y los
        // compañeros acababan trepando por los árboles hasta quedarse flotando a 14 m.
        float feet = _cc != null ? p.y + _cc.center.y - _cc.height * 0.5f : FeetY(p);
        float reach = _cc != null ? 3.5f : 0.6f;   // un compañero solo "sube" lo que sube un escalón
        RaycastHit[] hits = Physics.RaycastAll(new Vector3(p.x, p.y + 60f, p.z), Vector3.down, 200f, ~0, QueryTriggerInteraction.Ignore);
        float best = float.NegativeInfinity;
        foreach (RaycastHit h in hits)   // lo más alto, pero no un techo/árbol por encima de la cabeza
            if (!h.transform.IsChildOf(transform) && h.collider.GetComponentInParent<Anima>() == null && h.point.y <= feet + reach && h.point.y > best) best = h.point.y;
        if (float.IsNegativeInfinity(best)) return;

        // FLOTANDO (sin CharacterController no hay gravedad): baja al suelo que tiene debajo.
        if (_cc == null && feet > best + 0.4f && best > PolarWaterY - 0.3f)   // sobre el agua no: ahí nada (no baja al fondo del mar)
        {
            transform.position = p + Vector3.down * (feet - best);
            Debug.Log($"[Suelo] «{name}» flotaba {(feet - best):0.0} m sobre el suelo en ({p.x:0},{p.z:0}) → baja al suelo.");
            return;
        }
        if (feet >= best - buriedBelow) return;

        float lift = best - feet + 0.05f;
        if (_cc != null) _cc.enabled = false;
        transform.position = p + Vector3.up * lift;
        if (_cc != null) _cc.enabled = true;
        Debug.Log($"[Suelo] «{name}» estaba {(best - feet):0.0} m bajo el suelo en ({p.x:0},{p.z:0}) → recolocado encima.");
    }

    const float PolarWaterY = -0.25f;

    float FeetY(Vector3 p)
    {
        Transform body = transform.Find(CubeHumanoid.BodyName);
        if (body != null) return body.position.y;
        if (TryGetComponent(out CapsuleCollider c)) return transform.TransformPoint(c.center).y - c.height * 0.5f * transform.lossyScale.y;
        return p.y - 1f;
    }
}
