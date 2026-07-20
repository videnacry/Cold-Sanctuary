using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The fragile ward of the "a proteger" archetype (docs §8): a stationary mob that other mobs
/// besiege. It doesn't fight or flee — the player must interpose. Each attacker that reaches it
/// removes one point of integrity; if integrity hits 0 the ward is lost (mission fails).
///
/// Plain MonoBehaviour (not a MeditationMob): it is protected, not "resolved by attention".
/// </summary>
public class ProtectMob : MonoBehaviour
{
    [Header("Ward")]
    [Tooltip("Hits it can take before it is lost.")]
    [Min(1)] public int integrity = 3;

    [Tooltip("Fired when the ward is lost (integrity reaches 0).")]
    public UnityEvent onWardLost;

    /// <summary>Raised once when integrity reaches 0 (for the mission to detect failure).</summary>
    public event Action OnIntegrityDepleted;

    public bool IsAlive => integrity > 0;

    public void TakeHit()
    {
        if (integrity <= 0) return;
        integrity--;
        if (integrity <= 0)
        {
            onWardLost?.Invoke();
            OnIntegrityDepleted?.Invoke();
        }
    }
}
