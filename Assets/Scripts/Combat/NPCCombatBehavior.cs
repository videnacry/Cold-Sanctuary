using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Adds autonomous combat capability to any WorldCharacter that is in a combat zone.
///
/// Attach alongside WorldCharacter. When mobs are present in the same SanctuaryArea,
/// the NPC will automatically engage the nearest one, dealing damage based on their
/// Strength stat. Fragments collected by NPCs go into the shared Inventory.
///
/// This makes the kitchen (and other areas) feel like a co-op dungeon: the player
/// fights alongside companions, each contributing their stats.
/// </summary>
[RequireComponent(typeof(WorldCharacter))]
public class NPCCombatBehavior : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Combat")]
    [Tooltip("Base damage before Strength scaling (same formula as PlayerCombat).")]
    public float baseDamage = 8f;

    [Tooltip("Range at which the NPC detects and engages mobs.")]
    public float aggroRange = 5f;

    [Tooltip("Range at which the NPC can land a hit.")]
    public float attackRange = 1.5f;

    [Tooltip("Seconds between NPC attacks.")]
    public float attackCooldown = 1.2f;

    [Tooltip("LayerMask for IngredientMob colliders.")]
    public LayerMask mobLayer;

    // ── Runtime ───────────────────────────────────────────────────────────────

    WorldCharacter  _character;
    NavMeshAgent    _agent;
    IngredientMob   _currentTarget;
    float           _lastAttackTime;
    float           _lastDestinationUpdate;
    bool            _combatActive;

    const float DestinationUpdateInterval = 0.3f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        _character = GetComponent<WorldCharacter>();
        _agent     = GetComponent<NavMeshAgent>(); // optional — may not have one
    }

    void Update()
    {
        if (!_combatActive) return;

        // Re-acquire target if current one is gone
        if (_currentTarget == null || !_currentTarget.gameObject.activeSelf)
            _currentTarget = FindNearestMob();

        if (_currentTarget == null) return;

        float dist = Vector3.Distance(transform.position, _currentTarget.transform.position);

        if (dist <= attackRange)
        {
            StopMoving();
            FaceTarget(_currentTarget.transform);
            TryAttack();
        }
        else
        {
            MoveToward(_currentTarget.transform.position);
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Call this when the NPC enters a combat zone (e.g., the kitchen).</summary>
    public void EnterCombatMode()
    {
        _combatActive  = true;
        _currentTarget = FindNearestMob();
        if (_agent != null) _agent.isStopped = false;
        Debug.Log($"[{_character.characterName}] Entrando en modo combate.");
    }

    /// <summary>Call this when the NPC leaves the combat zone.</summary>
    public void ExitCombatMode()
    {
        _combatActive  = false;
        _currentTarget = null;
        StopMoving();
        Debug.Log($"[{_character.characterName}] Saliendo de modo combate.");
    }

    // ── Combat ────────────────────────────────────────────────────────────────

    void TryAttack()
    {
        if (Time.time - _lastAttackTime < attackCooldown) return;
        _lastAttackTime = Time.time;

        float damage = CalculateDamage();
        _currentTarget.TakeDamage(damage);

        // TODO: Animator.SetTrigger("attack");
        Debug.Log($"[{_character.characterName}] Golpeó {_currentTarget.ingredientName} " +
                  $"por {damage:F1} daño.");
    }

    float CalculateDamage()
    {
        // Same formula as PlayerCombat — NPCs with higher Strength hit harder
        float strengthMult = 0.5f + _character.Strength;
        return baseDamage * strengthMult;
    }

    IngredientMob FindNearestMob()
    {
        // Overlap sphere to find all mobs in range
        var cols = Physics.OverlapSphere(transform.position, aggroRange, mobLayer);
        IngredientMob nearest = null;
        float minDist = float.MaxValue;

        foreach (var col in cols)
        {
            var mob = col.GetComponentInParent<IngredientMob>();
            if (mob == null) continue;

            float dist = Vector3.Distance(transform.position, mob.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = mob;
            }
        }

        return nearest;
    }

    // ── Movement helpers ──────────────────────────────────────────────────────

    void MoveToward(Vector3 destination)
    {
        if (_agent != null)
        {
            // Throttle destination updates
            if (Time.time - _lastDestinationUpdate >= DestinationUpdateInterval)
            {
                _agent.isStopped = false;
                _agent.SetDestination(destination);
                _lastDestinationUpdate = Time.time;
            }
        }
        else
        {
            // Fallback — no NavMeshAgent: manual facing only
            FaceTarget(_currentTarget.transform);
        }
    }

    void StopMoving()
    {
        if (_agent != null && !_agent.isStopped)
            _agent.isStopped = true;
    }

    void FaceTarget(Transform target)
    {
        // When using NavMeshAgent this is handled by its angularSpeed.
        // Keep as fallback for non-agent NPCs.
        if (_agent != null && !_agent.isStopped) return;

        Vector3 dir = (target.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 8f);
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
