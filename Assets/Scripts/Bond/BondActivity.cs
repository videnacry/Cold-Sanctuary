using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A practice that builds bond between the player and an IBondable target
/// (companion, animal, object, place, natural element).
///
/// Can be practiced actively (player chooses it) or passively (context triggers it).
/// Trauma from stressful contexts can block the activity until the player's
/// Satisfaction reaches the required threshold.
///
/// See docs/bond-activity-system.md for full design rationale.
/// </summary>
[CreateAssetMenu(fileName = "NewBondActivity", menuName = "Cold Sanctuary/Bond Activity")]
public class BondActivity : ScriptableObject
{
    // ── Identity ─────────────────────────────────────────────────────────────
    [Header("Identity")]
    public string displayName;

    [TextArea(2, 4)]
    [Tooltip("How this activity was acquired — shown to the player as memory/context.")]
    public string acquisitionNote;

    // ── Target ───────────────────────────────────────────────────────────────
    [Header("Bond Target")]
    [Tooltip("What this activity builds bond with. Assign the scene object at runtime.")]
    public string targetId; // IBondable looked up by ID at runtime

    [Tooltip("Bond gained by the target per practice.")]
    public float bondGainPerPractice = 5f;

    // ── Unlock ───────────────────────────────────────────────────────────────
    [Header("Unlock")]
    public bool isUnlocked;

    // ── Passive activation ───────────────────────────────────────────────────
    [Header("Passive Activation")]
    public bool canActivatePassively;

    [Tooltip("Context tags that trigger passive practice (e.g. 'aquatic_mission', 'sunrise', 'mat_idle').")]
    public List<string> passiveTriggers = new List<string>();

    // ── Trauma & blocking ────────────────────────────────────────────────────
    [Header("Trauma")]
    [Range(0f, 1f)]
    public float traumaAccumulated;

    [Tooltip("Trauma level at which the activity becomes blocked.")]
    public float blockThreshold = 0.5f;

    [Tooltip("Base Satisfaction required to unblock (no trauma scaling).")]
    public float baseUnlockSatisfaction = 0.3f;

    [Tooltip("Extra Satisfaction required per unit of trauma accumulated.")]
    public float traumaToSatisfactionRatio = 0.5f;

    [Tooltip("How much trauma fades per in-game day without negative reinforcement.")]
    public float traumaRecoveryRatePerDay = 0.02f;

    // ── Runtime state ─────────────────────────────────────────────────────────
    [HideInInspector] public bool isBlocked;
    [HideInInspector] public int  practiceCount;

    // ── Computed ──────────────────────────────────────────────────────────────

    /// <summary>Satisfaction level the player needs to unblock this activity.</summary>
    public float UnblockThreshold =>
        baseUnlockSatisfaction + traumaAccumulated * traumaToSatisfactionRatio;

    // ── Practice ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Attempt to practice this activity.
    /// Returns true if the practice was executed (not blocked, is unlocked).
    /// Registers the player's stress context for trauma calculation.
    /// </summary>
    public bool TryPractice(PlayerStats player, IBondable target)
    {
        if (!isUnlocked) return false;

        // Check if blocked — can we re-open it?
        if (isBlocked)
        {
            if (player.satisfaction >= UnblockThreshold)
                isBlocked = false;
            else
                return false; // still blocked
        }

        // Register experience
        RegisterExperience(player);

        // Grow bond with target
        if (target != null)
            target.GrowBondWithPlayer(bondGainPerPractice);

        practiceCount++;
        return true;
    }

    /// <summary>
    /// Record this practice context. High stress → accumulate trauma → may block.
    /// </summary>
    void RegisterExperience(PlayerStats player)
    {
        float stressThreshold = 0.6f; // could be exposed per-activity if needed

        if (player.stress > stressThreshold)
        {
            float traumaDelta = (player.stress - stressThreshold) * 0.1f;
            traumaAccumulated = Mathf.Clamp01(traumaAccumulated + traumaDelta);

            if (traumaAccumulated >= blockThreshold)
                isBlocked = true;
        }
        else
        {
            // Positive experience — slowly reduces trauma
            traumaAccumulated = Mathf.Clamp01(traumaAccumulated - 0.01f);
        }
    }

    /// <summary>
    /// Call once per in-game day to naturally recover trauma when not reinforced.
    /// </summary>
    public void TickTraumaRecovery()
    {
        if (traumaAccumulated > 0f)
        {
            traumaAccumulated = Mathf.Clamp01(traumaAccumulated - traumaRecoveryRatePerDay);
            if (isBlocked && traumaAccumulated < blockThreshold)
                isBlocked = false;
        }
    }

    /// <summary>Initialise runtime state (call when loading a save).</summary>
    public void InitRuntime()
    {
        isBlocked     = traumaAccumulated >= blockThreshold;
        practiceCount = 0;
    }
}
