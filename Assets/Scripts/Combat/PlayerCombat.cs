using UnityEngine;

/// <summary>
/// Handles the player's melee attack in combat zones (kitchen, alchemy lab, etc.).
///
/// Attack flow:
///   Input (mouse0 / tap) → cooldown check → SphereCast in front of player
///   → TakeDamage() on all IngredientMobs in range → fragment drops handled by mob.
///
/// Damage scales with the player's Strength (physicalResistance) stat so that
/// upgrading via garden/workshop tasks has a direct impact on combat efficiency.
///
/// Equipment slots (armor/tool) modify incoming damage and outgoing damage respectively.
/// </summary>
[RequireComponent(typeof(PlayerStats))]
public class PlayerCombat : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Attack")]
    [Tooltip("Base damage before Strength scaling.")]
    public float baseDamage = 10f;

    [Tooltip("Attack range — radius of the forward sphere cast.")]
    public float attackRange = 2f;

    [Tooltip("Sphere radius for hit detection.")]
    public float attackRadius = 0.6f;

    [Tooltip("Seconds between attacks.")]
    public float attackCooldown = 0.8f;

    [Header("Layers")]
    [Tooltip("LayerMask for IngredientMob colliders.")]
    public LayerMask mobLayer;

    [Header("Input")]
    [Tooltip("KeyCode for attack. Mouse0 by default.")]
    public KeyCode attackKey = KeyCode.Mouse0;

    // ── Equipment (set by Inventory) ──────────────────────────────────────────

    /// <summary>Tool equipped — multiplies outgoing damage.</summary>
    [HideInInspector] public float toolDamageMultiplier = 1f;

    /// <summary>Armor equipped — reduces incoming damage (0 = no armor, 0.5 = half damage).</summary>
    [HideInInspector] public float armorDamageReduction = 0f;

    // ── Events ────────────────────────────────────────────────────────────────

    public System.Action<IngredientMob, float> OnHitMob;   // (mob, damage)
    public System.Action<float>                OnTookDamage; // (damage after armor)

    // ── Runtime ───────────────────────────────────────────────────────────────

    PlayerStats _stats;
    float       _lastAttackTime = -99f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake() => _stats = GetComponent<PlayerStats>();

    void Update()
    {
        if (Input.GetKeyDown(attackKey)) TryAttack();
    }

    // ── Attack ────────────────────────────────────────────────────────────────

    void TryAttack()
    {
        if (Time.time - _lastAttackTime < attackCooldown) return;
        _lastAttackTime = Time.time;

        // TODO: Animator.SetTrigger("attack");

        float damage = CalculateDamage();
        HitMobsInFront(damage);
    }

    float CalculateDamage()
    {
        // Strength (physicalResistance 0–1) scales damage linearly: 0.5 = base, 1.0 = 2× base
        float strengthMult = 0.5f + _stats.physicalResistance;
        return baseDamage * strengthMult * toolDamageMultiplier;
    }

    void HitMobsInFront(float damage)
    {
        Vector3 origin    = transform.position + Vector3.up * 0.8f;
        Vector3 direction = transform.forward;

        // Detect all mobs in a sphere cast in front of the player
        var hits = Physics.SphereCastAll(origin, attackRadius, direction, attackRange, mobLayer);

        bool hitSomething = false;
        foreach (var hit in hits)
        {
            var mob = hit.collider.GetComponentInParent<IngredientMob>();
            if (mob == null) continue;

            mob.TakeDamage(damage);
            OnHitMob?.Invoke(mob, damage);
            hitSomething = true;

            // TODO: spawn hit VFX at hit.point
            Debug.Log($"[PlayerCombat] Golpeó {mob.ingredientName} por {damage:F1} daño.");
        }

        if (!hitSomething)
            Debug.Log("[PlayerCombat] Golpe en el aire.");
    }

    // ── Receive damage (called by IngredientMob) ──────────────────────────────

    /// <summary>
    /// Called by mobs when they hit this player. Armor reduces the incoming value.
    /// </summary>
    public void ReceiveDamage(float rawDamage)
    {
        float reduced = rawDamage * (1f - Mathf.Clamp01(armorDamageReduction));
        _stats.DrainMind(reduced * 0.01f, MindChannel.MentalFatigue);
        OnTookDamage?.Invoke(reduced);

        // TODO: hit flash / screen vignette
        Debug.Log($"[PlayerCombat] Recibió {reduced:F1} daño (armadura redujo {armorDamageReduction * 100f:F0}%).");
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 origin = transform.position + Vector3.up * 0.8f;
        Gizmos.DrawWireSphere(origin + transform.forward * attackRange, attackRadius);
        Gizmos.DrawRay(origin, transform.forward * attackRange);
    }
}
