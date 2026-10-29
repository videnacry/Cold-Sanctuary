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
        if (FlyingCarpet.Riding(gameObject)) return;   // volando en alfombra
        if (SwimBody.Swimming(gameObject)) return;     // nadando (el mar, la columna de agua del árbol): ni se le baja al fondo ni se le sube
        if (TryGetComponent(out PlayerController pc) && pc.BodyHeld) return;   // lo sujeta alguien (lo llevan de la mano, sentado en el examen…): lo coloca quien lo sujeta
        if (GymStation.Seated(gameObject)) return;   // en una máquina del gimnasio (el asiento de la prensa y el banco del curl están altos): lo coloca la jornada

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
        if (_cc == null && feet > best + 0.4f && best > PolarWaterY - 0.3f && !GroundUnderRoot(p) && !OnAgentMesh())   // sobre el agua no: ahí nada (no baja al fondo del mar)
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

    // ¿Hay suelo justo bajo su origen? (los pies estimados del cuerpo a veces quedan por debajo y el techo de alfombras, a
    // ras de los pies, no contaba: lo «bajaba» al piso de abajo nada más aterrizar.)
    // Con su agente sobre el NavMesh, el agente lo tiene en su piso: no se le «baja» (pero si está enterrado, sí se le sube).
    bool OnAgentMesh() => TryGetComponent(out UnityEngine.AI.NavMeshAgent ag) && ag.enabled && ag.isOnNavMesh;

    bool GroundUnderRoot(Vector3 p)
    {
        foreach (RaycastHit h in Physics.RaycastAll(p + Vector3.up * 0.5f, Vector3.down, 1.2f, ~0, QueryTriggerInteraction.Ignore))
            if (!h.transform.IsChildOf(transform) && h.collider.GetComponentInParent<Anima>() == null) return true;
        return false;
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
