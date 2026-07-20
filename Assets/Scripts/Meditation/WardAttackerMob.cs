using UnityEngine;

/// <summary>
/// Attacker of the "a proteger" archetype (docs §8): moves toward the ward. The player REPELS it by
/// getting between it and the ward (within interceptRadius) → it dissolves (repelled, counts as
/// progress). If it reaches the ward first it lands a hit and dissolves (does NOT count) — the
/// mission distinguishes the two via <see cref="ReachedWard"/>.
/// </summary>
public class WardAttackerMob : MeditationMob
{
    [Header("Attacker")]
    [Tooltip("Player within this distance repels it.")]
    [Min(0.1f)] public float interceptRadius = 1.3f;

    [Tooltip("Reaching this close to the ward lands a hit.")]
    [Min(0.1f)] public float hitRadius = 1.2f;

    /// <summary>True when it dissolved by reaching the ward (a hit), false when repelled by the player.</summary>
    public bool ReachedWard { get; private set; }

    Transform _wardT;
    ProtectMob _ward;

    public void SetWard(ProtectMob ward)
    {
        _ward  = ward;
        _wardT = ward != null ? ward.transform : null;
    }

    void Update()
    {
        if (_wardT == null) return;

        // Repelled if the player interposes.
        if (player != null &&
            Vector3.Distance(player.position, transform.position) <= interceptRadius)
        {
            ReachedWard = false;
            Dissolve();
            return;
        }

        // March toward the ward (planar).
        Vector3 toWard = _wardT.position - transform.position;
        toWard.y = 0f;
        float dist = toWard.magnitude;
        if (dist > 0.001f)
        {
            Vector3 dir = toWard / dist;
            transform.position += dir * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(
                transform.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);
        }

        if (dist <= hitRadius)
        {
            _ward?.TakeHit();
            ReachedWard = true;
            Dissolve();
        }
    }
}
