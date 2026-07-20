using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The "to heal" archetype (docs §8): a hurt/sick mob that LIMPS TOWARD the player seeking comfort
/// (the opposite of fleeing). You soothe it by staying close; the heal fills while you're within
/// range and decays if you leave. When full, it is healed and dissolves (resolved). Ties to Bond
/// and Satisfaction. Technique: bondad amorosa (metta).
/// </summary>
public class HealMob : MeditationMob
{
    [Header("Heal (stay near to soothe)")]
    [Tooltip("The player must stay within this distance to soothe it.")]
    [Min(0.1f)] public float healRadius = 2f;

    [Tooltip("Seconds of closeness before it is healed.")]
    [Min(0.1f)] public float healTime = 3f;

    [Tooltip("Heal lost per second when the player steps away (× real time).")]
    [Min(0f)] public float decayRate = 0.5f;

    [Tooltip("Fired when it is fully healed (VFX / bond gain).")]
    public UnityEvent onHealed;

    float _heal;

    /// <summary>Heal progress 0–1 (for a future UI ring / VFX).</summary>
    public float HealProgress => Mathf.Clamp01(_heal / healTime);

    void Update()
    {
        if (player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;

        // Limp toward the player seeking comfort, but stop once within healing range.
        if (dist > healRadius && dist > 0.001f)
            transform.position += (toPlayer / dist) * moveSpeed * Time.deltaTime;

        if (dist <= healRadius)
        {
            _heal += Time.deltaTime;
            if (_heal >= healTime) { onHealed?.Invoke(); Dissolve(); }
        }
        else
        {
            _heal = Mathf.Max(0f, _heal - Time.deltaTime * decayRate);
        }
    }
}
