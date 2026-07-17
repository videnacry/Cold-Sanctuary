using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The "distraction / ephemeral thought" archetype (docs §8, confirmed jugable by the user).
///
/// It CHASES the player, trying to touch (latch onto) them. Every touch "feeds" it and resets
/// its fade timer. If the player keeps their distance — flees, doesn't feed it — the thought
/// fails to latch and DISSOLVES after <see cref="dissolveDelay"/> seconds without contact.
///
/// Mechanic in one line: don't engage → it fades. Engaging (letting it touch you) keeps it alive.
/// Movement is planar (keeps its spawn height) so thoughts float around the player.
/// </summary>
public class EphemeralThoughtMob : MeditationMob
{
    [Header("Ephemeral")]
    [Tooltip("Seconds WITHOUT touching the player before the thought dissolves.")]
    [Min(0.1f)] public float dissolveDelay = 3f;

    [Tooltip("Distance at which the thought counts as touching the player (resets its timer).")]
    [Min(0.1f)] public float touchRadius = 1.2f;

    [Tooltip("Fired each time the thought manages to touch the player (hook for penalty/VFX).")]
    public UnityEvent onTouchedPlayer;

    float _timeAway;

    void Update()
    {
        if (player == null)
        {
            // No player to latch onto → it simply fades.
            _timeAway += Time.deltaTime;
            if (_timeAway >= dissolveDelay) Dissolve();
            return;
        }

        // Chase on the horizontal plane (keep our own height).
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;

        if (dist > 0.001f)
        {
            Vector3 dir = toPlayer / dist;
            transform.position += dir * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(
                transform.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);
        }

        if (dist <= touchRadius)
        {
            _timeAway = 0f;                 // fed — reset the fade timer
            onTouchedPlayer?.Invoke();
        }
        else
        {
            _timeAway += Time.deltaTime;
            if (_timeAway >= dissolveDelay) Dissolve();
        }
    }
}
