using UnityEngine;

/// <summary>
/// The "posture" archetype (docs §8): the inverse of EphemeralThoughtMob. It FLEES from the
/// player, who must chase and corner it. Holding it within holdRadius for holdTime "forms" the
/// posture → it dissolves (mastered). If you lose it, the hold progress decays.
///
/// Mechanic in one line: catch and hold → it forms. Movement is planar.
/// Keep moveSpeed at or below the player's speed so it's catchable (corner it against geometry).
/// </summary>
public class PostureFormMob : MeditationMob
{
    [Header("Posture (flee & form)")]
    [Tooltip("It flees while the player is closer than this.")]
    [Min(0.1f)] public float fleeDistance = 6f;

    [Tooltip("The player must stay within this distance to hold (form) the posture.")]
    [Min(0.1f)] public float holdRadius = 1.5f;

    [Tooltip("Seconds of holding before the posture forms and dissolves.")]
    [Min(0.1f)] public float holdTime = 2f;

    [Tooltip("How fast hold progress decays when the player loses it (× real time).")]
    [Min(0f)] public float holdDecayRate = 0.5f;

    float _held;

    /// <summary>Hold progress 0–1 (for a future UI ring / VFX).</summary>
    public float HoldProgress => Mathf.Clamp01(_held / holdTime);

    void Update()
    {
        if (player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;

        // Flee while the player is within flee range.
        if (dist < fleeDistance && dist > 0.001f)
        {
            Vector3 away = -toPlayer / dist;
            transform.position += away * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(
                transform.rotation, Quaternion.LookRotation(away), 8f * Time.deltaTime);
        }

        // Hold while cornered; decay otherwise.
        if (dist <= holdRadius)
        {
            _held += Time.deltaTime;
            if (_held >= holdTime) Dissolve(); // posture formed
        }
        else
        {
            _held = Mathf.Max(0f, _held - Time.deltaTime * holdDecayRate);
        }
    }
}
