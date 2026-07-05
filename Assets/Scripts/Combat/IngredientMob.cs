using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// An ingredient that becomes a mob when the player enters the kitchen at miniaturized scale.
///
/// Core loop:
///   Player attacks (or uses a "process" action) → health drops → on death: spawn ElementFragment,
///   play processed animation, send OnProcessed event, optionally reproduce (Yeast).
///
/// The mob does NOT die in the traditional sense — it becomes "processed":
/// the ingredient is consumed and transformed, consistent with the sanctuary's no-kill ethos.
///
/// Attach this alongside a NavMeshAgent + Animator. The attack + movement logic is
/// driven by this component; physics + collision handled by a CharacterController or Rigidbody.
/// </summary>
[RequireComponent(typeof(Collider))]
public class IngredientMob : MonoBehaviour
{
    // ── Identity ──────────────────────────────────────────────────────────────

    [Header("Identity")]
    public string ingredientName = "Ingrediente";

    [TextArea(1, 2)]
    public string flavorText;

    // ── Element ───────────────────────────────────────────────────────────────

    [Header("Element Drop")]
    [Tooltip("Chemical element this mob reveals when processed.")]
    public string elementSymbol = "C";

    [Tooltip("Probability that processing this mob discovers the element " +
             "(if the player doesn't already know it). Stacks per drop.")]
    [Range(0f, 1f)]
    public float discoveryChance = 0.5f;

    [Tooltip("Number of ElementFragment objects dropped on death.")]
    [Min(1)] public int fragmentDropCount = 3;

    [Tooltip("Prefab for the fragment pickup. Must have an ElementFragment component.")]
    public GameObject fragmentPrefab;

    // ── Stats ─────────────────────────────────────────────────────────────────

    [Header("Stats")]
    [Min(1)] public float maxHealth = 30f;
    float _health;

    [Tooltip("Base damage dealt per hit to the player.")]
    public float attackDamage = 5f;

    [Tooltip("Seconds between attacks.")]
    [Min(0.1f)] public float attackCooldown = 2f;

    [Tooltip("Distance at which the mob starts chasing the player.")]
    public float aggroRange = 8f;

    [Tooltip("Distance at which the mob can hit the player.")]
    public float attackRange = 1.5f;

    // ── Reproduction (Yeast / Levadura mechanic) ──────────────────────────────

    [Header("Reproduction (optional — enable for Yeast)")]
    public bool canReproduce;

    [Tooltip("Seconds between reproduction attempts.")]
    public float reproduceInterval = 10f;

    [Tooltip("Max total instances of this mob type in the scene before reproduction stops.")]
    public int maxPopulation = 6;

    [Tooltip("Prefab to spawn (can be self).")]
    public GameObject spawnPrefab;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired when this mob is fully processed (health → 0).</summary>
    public event Action<IngredientMob> OnProcessed;

    // ── Runtime ───────────────────────────────────────────────────────────────

    Transform   _playerTransform;
    float       _lastAttackTime;
    bool        _isProcessed;
    MobState    _state = MobState.Idle;

    enum MobState { Idle, Aggro, Attack, Processed }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        _health = maxHealth;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;

        if (canReproduce)
            StartCoroutine(ReproductionLoop());
    }

    void Update()
    {
        if (_isProcessed || _playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);

        switch (_state)
        {
            case MobState.Idle:
                if (dist <= aggroRange) EnterAggro();
                break;

            case MobState.Aggro:
                if (dist > aggroRange * 1.5f)
                    _state = MobState.Idle;
                else if (dist <= attackRange)
                    EnterAttack();
                else
                    ChasePlayer();
                break;

            case MobState.Attack:
                if (dist > attackRange)
                    _state = MobState.Aggro;
                else
                    TryAttack();
                break;
        }
    }

    // ── Movement / attack stubs ───────────────────────────────────────────────

    void EnterAggro()
    {
        _state = MobState.Aggro;
        // TODO: NavMeshAgent.SetDestination(_playerTransform.position);
        // TODO: Animator.SetBool("aggro", true);
    }

    void ChasePlayer()
    {
        // TODO: NavMeshAgent.SetDestination(_playerTransform.position);
        transform.LookAt(new Vector3(_playerTransform.position.x,
                                     transform.position.y,
                                     _playerTransform.position.z));
    }

    void EnterAttack()
    {
        _state = MobState.Attack;
        // TODO: Animator.SetTrigger("attack_start");
    }

    void TryAttack()
    {
        if (Time.time - _lastAttackTime < attackCooldown) return;
        _lastAttackTime = Time.time;

        // TODO: Animator.SetTrigger("attack");
        // Apply damage to player
        var playerStats = _playerTransform.GetComponent<PlayerStats>();
        if (playerStats != null)
            playerStats.DrainMind(attackDamage * 0.01f, MindChannel.MentalFatigue);

        Debug.Log($"[{ingredientName}] Ataca al jugador — {attackDamage} daño.");
    }

    // ── Damage / Processing ───────────────────────────────────────────────────

    /// <summary>
    /// Call this when the player's attack lands on this mob.
    /// damage: raw damage value from the player's attack.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (_isProcessed) return;

        _health -= damage;
        // TODO: Animator.SetTrigger("hit");
        // TODO: Spawn hit particles

        Debug.Log($"[{ingredientName}] Recibió {damage} daño. Vida restante: {_health:F1}");

        if (_health <= 0f)
            StartCoroutine(ProcessSequence());
    }

    IEnumerator ProcessSequence()
    {
        _isProcessed = true;
        _state       = MobState.Processed;

        // TODO: Animator.SetTrigger("processed");
        Debug.Log($"[{ingredientName}] Procesado.");

        // Brief pause for the animation
        yield return new WaitForSeconds(0.5f);

        // Drop fragments
        SpawnFragments();

        // Notify listeners
        OnProcessed?.Invoke(this);

        // Optionally discover element
        if (!string.IsNullOrEmpty(elementSymbol)
            && UnityEngine.Random.value < discoveryChance)
        {
            PeriodicTableManager.Instance?.Discover(elementSymbol);
        }

        // Small delay then despawn
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    void SpawnFragments()
    {
        if (fragmentPrefab == null) return;

        for (int i = 0; i < fragmentDropCount; i++)
        {
            Vector3 offset = UnityEngine.Random.insideUnitSphere * 0.5f;
            offset.y = Mathf.Abs(offset.y);
            GameObject frag = Instantiate(fragmentPrefab,
                transform.position + offset, Quaternion.identity);

            var ef = frag.GetComponent<ElementFragment>();
            if (ef != null)
            {
                ef.elementSymbol   = elementSymbol;
                ef.discoveryChance = discoveryChance;
            }
        }
    }

    // ── Reproduction (Yeast) ──────────────────────────────────────────────────

    IEnumerator ReproductionLoop()
    {
        while (!_isProcessed)
        {
            yield return new WaitForSeconds(reproduceInterval);
            if (_isProcessed || spawnPrefab == null) yield break;

            // Count existing mobs of the same type
            var existing = FindObjectsByType<IngredientMob>(FindObjectsSortMode.None);
            int count = 0;
            foreach (var m in existing)
                if (m.ingredientName == ingredientName && !m._isProcessed) count++;

            if (count >= maxPopulation) continue;

            Vector3 spawnPos = transform.position
                             + UnityEngine.Random.insideUnitSphere * 1.5f;
            spawnPos.y = transform.position.y;

            Instantiate(spawnPrefab, spawnPos, Quaternion.identity);
            Debug.Log($"[{ingredientName}] Se reprodujo. Población: {count + 1}/{maxPopulation}");
        }
    }

    // ── Mouse selection ───────────────────────────────────────────────────────

    /// <summary>
    /// Clicking the mob selects it as the combat target AND opens the ability palette.
    /// Works on desktop. On mobile, use a screen-space raycast → Select().
    /// </summary>
    void OnMouseDown()
    {
        CombatTargetSelector.Instance?.SelectAndOpenPalette(this);
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
