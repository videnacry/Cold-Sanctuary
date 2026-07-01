using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all BondActivities for the player.
/// Handles passive activation when context tags are set,
/// daily trauma recovery, and provides the active-practice API.
///
/// Attach to the Player GameObject.
/// </summary>
public class BondActivityManager : MonoBehaviour
{
    [Header("Activities")]
    [Tooltip("All BondActivities the player has access to (unlocked or not).")]
    public List<BondActivity> activities = new List<BondActivity>();

    [Header("References")]
    public PlayerStats playerStats;

    // Active context tags this frame (set by missions, zones, time-of-day systems)
    readonly HashSet<string> _activeContextTags = new HashSet<string>();

    // IBondable registry — other scripts register targets here by ID
    readonly Dictionary<string, IBondable> _bondables = new Dictionary<string, IBondable>();

    // ── Unity ─────────────────────────────────────────────────────────────────

    void Start()
    {
        foreach (BondActivity a in activities)
            a.InitRuntime();
    }

    void Update()
    {
        TickPassiveActivities();
        TickBlockCheck();
    }

    // ── Context tags ──────────────────────────────────────────────────────────

    /// <summary>Add a context tag this frame (e.g. "aquatic_mission", "sunrise").</summary>
    public void SetContextTag(string tag)   => _activeContextTags.Add(tag);

    /// <summary>Remove a context tag (e.g. when leaving a zone).</summary>
    public void ClearContextTag(string tag) => _activeContextTags.Remove(tag);

    // ── Bondable registry ─────────────────────────────────────────────────────

    /// <summary>Register an IBondable so BondActivities can find it by targetId.</summary>
    public void RegisterBondable(string id, IBondable bondable) =>
        _bondables[id] = bondable;

    public void UnregisterBondable(string id) => _bondables.Remove(id);

    // ── Active practice ───────────────────────────────────────────────────────

    /// <summary>
    /// Player deliberately chooses to practice an activity.
    /// Returns true if successful.
    /// </summary>
    public bool Practice(BondActivity activity)
    {
        IBondable target = ResolveBondable(activity.targetId);
        return activity.TryPractice(playerStats, target);
    }

    /// <summary>Practice by display name (convenience for UI).</summary>
    public bool Practice(string activityName)
    {
        BondActivity a = activities.Find(x => x.displayName == activityName);
        return a != null && Practice(a);
    }

    // ── Passive activation ────────────────────────────────────────────────────

    void TickPassiveActivities()
    {
        if (_activeContextTags.Count == 0) return;

        foreach (BondActivity a in activities)
        {
            if (!a.isUnlocked || !a.canActivatePassively) continue;

            foreach (string trigger in a.passiveTriggers)
            {
                if (_activeContextTags.Contains(trigger))
                {
                    IBondable target = ResolveBondable(a.targetId);
                    bool practiced = a.TryPractice(playerStats, target);
                    if (practiced)
                        Debug.Log($"[BondActivity] Passive practice: {a.displayName} (trigger: {trigger})");
                    break;
                }
            }
        }
    }

    // ── Block check ───────────────────────────────────────────────────────────

    /// <summary>Each frame: if blocked and satisfaction now sufficient, auto-unblock.</summary>
    void TickBlockCheck()
    {
        foreach (BondActivity a in activities)
        {
            if (a.isBlocked && playerStats.satisfaction >= a.UnblockThreshold)
                a.isBlocked = false;
        }
    }

    // ── Daily tick ────────────────────────────────────────────────────────────

    /// <summary>Call once per in-game day (from your time system).</summary>
    public void OnNewDay()
    {
        foreach (BondActivity a in activities)
            a.TickTraumaRecovery();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    IBondable ResolveBondable(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        _bondables.TryGetValue(id, out IBondable b);
        return b;
    }
}
