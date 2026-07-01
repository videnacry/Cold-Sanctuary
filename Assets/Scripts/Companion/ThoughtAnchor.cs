using System;
using UnityEngine;

/// <summary>
/// A belief or pattern that biases a companion's behavior.
/// Positive weight = pulls toward the anchor's direction.
/// Negative weight = blocks or resists the anchor's direction.
///
/// Examples:
///   "yoga_skepticism"  weight=-0.9  → Goluis resists yoga learning
///   "work_hard"        weight= 0.8  → Panterilia prioritizes tasks
///   "celebrate_life"   weight= 0.9  → Gohageneis leans into joy
///
/// Anchors soften over time through arc events (story moments, bond milestones).
/// </summary>
[Serializable]
public class ThoughtAnchor
{
    [Tooltip("Unique key used by other systems to query this anchor.")]
    public string key;

    [Tooltip("–1 = strong block/resistance. +1 = strong pull/drive.")]
    [Range(-1f, 1f)]
    public float weight;

    [Tooltip("How much this anchor can shift per in-game day (arc speed).")]
    public float changeRatePerDay = 0.01f;

    public ThoughtAnchor(string pKey, float pWeight, float pChangeRate = 0.01f)
    {
        key            = pKey;
        weight         = Mathf.Clamp(pWeight, -1f, 1f);
        changeRatePerDay = pChangeRate;
    }

    /// <summary>Shift the anchor weight toward a target value (arc progression).</summary>
    public void ShiftToward(float targetWeight, float deltaTime)
    {
        weight = Mathf.MoveTowards(weight, targetWeight, changeRatePerDay * deltaTime);
    }
}
