using UnityEngine;

/// <summary>
/// SPRAY DE ÁCIDO FÓRMICO — defensa de Kushal hormiga (Nivel 1, Microcosmos).
///
/// Las hormigas obreras (Formica, Camponotus…) rocían ácido fórmico para disuadir
/// depredadores. En Cold Sanctuary: AoE corto alcance que sube el <c>stress</c> de
/// todos los <see cref="Anima"/> cercanos (excepto el propio Kushal y los aliados de
/// su facción si <see cref="sprayFriendly"/> es false).
///
/// Cuando el stress del depredador sube de cierto umbral, <see cref="Animal.EvaluateThreat"/>
/// / <see cref="Predation"/> ya lo hacen huir (el sistema de amenaza existente hace el trabajo).
///
/// Cargas limitadas (<see cref="maxCharges"/>); se recargan de una en una cada
/// <see cref="rechargeTime"/> segundos. El jugador activa con la tecla Q (configurable).
///
/// Sin CharacterLevel ni MagicReserves: es una habilidad corporal del insecto, no magia.
/// </summary>
public class FormicAcidSpray : MonoBehaviour
{
    [Header("Alcance y efecto")]
    [Tooltip("Radio del spray (m). A escala insecto, 1-2 m representa distancia de varios cuerpos.")]
    [Min(0.1f)] public float sprayRadius = 1.5f;

    [Tooltip("Aumento de stress aplicado a cada Anima en rango (0–1).")]
    [Range(0f, 1f)] public float stressIncrease = 0.35f;

    [Tooltip("Si false, no afecta a aliados de la misma facción que Kushal.")]
    public bool sprayFriendly = false;

    [Header("Cargas")]
    [Tooltip("Cargas máximas disponibles.")]
    [Min(1)] public int maxCharges = 3;

    [Tooltip("Segundos entre la recarga de cada carga.")]
    [Min(1f)] public float rechargeTime = 12f;

    [Header("Input")]
    [Tooltip("Tecla para activar el spray (Q por defecto — libre en los controles documentados).")]
    public KeyCode sprayKey = KeyCode.Q;

    // ── Estado ────────────────────────────────────────────────────────────────

    public int CurrentCharges => _charges;
    public float RechargeProgress => _rechargeTimer / rechargeTime; // 0..1

    int   _charges;
    float _rechargeTimer;
    Anima _self;
    char  _myFaction;

    void Awake()
    {
        _charges   = maxCharges;
        _self      = GetComponent<Anima>();
        _myFaction = _self != null ? _self.GetComponent<ITarget>()?.Faction ?? '\0' : '\0';
    }

    void Update()
    {
        // Recarga gradual (una carga a la vez).
        if (_charges < maxCharges)
        {
            _rechargeTimer += Time.deltaTime;
            if (_rechargeTimer >= rechargeTime)
            {
                _charges++;
                _rechargeTimer = 0f;
                Debug.Log($"[Ácido] Carga recargada. Cargas: {_charges}/{maxCharges}.");
            }
        }

        if (Input.GetKeyDown(sprayKey)) Spray();
    }

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>Lanza el spray si hay cargas disponibles.</summary>
    public void Spray()
    {
        if (_charges <= 0)
        {
            Debug.Log("[Ácido] Sin cargas. Espera a recargar.");
            return;
        }

        _charges--;
        _rechargeTimer = 0f;

        int affected = 0;
        var hits = Physics.OverlapSphere(transform.position, sprayRadius);
        foreach (var col in hits)
        {
            var anima = col.GetComponent<Anima>();
            if (anima == null || anima == _self) continue;

            // Respetar facción si sprayFriendly = false.
            if (!sprayFriendly && _myFaction != '\0')
            {
                var tgt = col.GetComponent<ITarget>();
                if (tgt != null && tgt.Faction == _myFaction) continue;
            }

            anima.stress = Mathf.Min(1f, anima.stress + stressIncrease);
            affected++;
            Debug.Log($"[Ácido] «{anima.name}» stress +{stressIncrease:0.0} → {anima.stress:0.0}.");
        }

        Debug.Log($"[Ácido] Spray activado. Afectados: {affected}. Cargas restantes: {_charges}/{maxCharges}.");
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0.1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, sprayRadius);
    }
}
