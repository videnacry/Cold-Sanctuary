using UnityEngine;

/// <summary>
/// Hechizo de ÁCIDO FÓRMICO — defensa AoE de Kushal hormiga (Nivel 1, Microcosmos).
///
/// Las hormigas obreras (Formica, Camponotus…) rocían ácido fórmico para disuadir
/// depredadores. Como hechizo extiende <see cref="SpellBase"/>:
///   • <c>range</c>  = radio del spray (reemplaza el antiguo <c>sprayRadius</c>).
///   • <c>force</c>  = aumento de stress por hit (reemplaza <c>stressIncrease</c>).
///
/// Target ignorado en AoE: el efecto cae sobre todos los <see cref="Anima"/> cercanos
/// al caster excepto aliados de la misma facción (si <see cref="sprayFriendly"/> = false).
///
/// El stress acumulado en el depredador hace que <see cref="Predation"/> / <c>EvaluateThreat</c>
/// ya lo hagan huir — el sistema de amenaza existente hace el trabajo.
///
/// Cargas limitadas (<see cref="maxCharges"/>); se recargan de una en una.
/// Coste de energía opcional: campo <see cref="SpellBase.energyCost"/> (heredado).
/// </summary>
public class FormicAcidSpray : SpellBase
{
    [Header("Spray — parámetros extra")]
    [Tooltip("Si false, no afecta a aliados de la misma facción que el caster.")]
    public bool sprayFriendly = false;

    [Header("Cargas")]
    [Tooltip("Cargas máximas disponibles.")]
    [Min(1)] public int maxCharges = 3;

    [Tooltip("Segundos entre la recarga de cada carga.")]
    [Min(1f)] public float rechargeTime = 12f;

    [Header("Input")]
    [Tooltip("Tecla para activar el spray.")]
    public KeyCode sprayKey = KeyCode.Q;

    // ── Estado ────────────────────────────────────────────────────────────────

    public int CurrentCharges => _charges;
    public float RechargeProgress => _rechargeTimer / rechargeTime;

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

        if (_self != null && Input.GetKeyDown(sprayKey) && CanCast(_self, null))
        {
            if (!PayEnergy(_self)) { Debug.Log("[Ácido] Sin energía para lanzar."); return; }
            Cast(_self, null);
        }
    }

    // ── SpellBase ─────────────────────────────────────────────────────────────

    /// <summary>AoE: target ignorado (el efecto es esférico sobre el caster). Requiere cargas y energía.</summary>
    public override bool CanCast(Anima caster, ITarget target)
        => _charges > 0 && HasEnergy(caster);

    /// <summary>Aplica el spray AoE. Usa <see cref="SpellBase.range"/> como radio y <see cref="SpellBase.force"/> como stress.</summary>
    public override void Cast(Anima caster, ITarget target)
    {
        if (!CanCast(caster, target)) return;
        _charges--;
        _rechargeTimer = 0f;

        float sprayRadius = range > 0f ? range : 1.5f;
        float stressIncrease = force;

        int affected = 0;
        var hits = Physics.OverlapSphere(caster.transform.position, sprayRadius);
        foreach (var col in hits)
        {
            var anima = col.GetComponent<Anima>();
            if (anima == null || anima == caster) continue;

            if (!sprayFriendly && _myFaction != '\0')
            {
                var tgt = col.GetComponent<ITarget>();
                if (tgt != null && tgt.Faction == _myFaction) continue;
            }

            anima.stress = Mathf.Min(1f, anima.stress + stressIncrease);
            affected++;
        }

        Debug.Log($"[Ácido] Spray activado. Afectados: {affected}. Cargas: {_charges}/{maxCharges}.");
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0.1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, range > 0f ? range : 1.5f);
    }
}
