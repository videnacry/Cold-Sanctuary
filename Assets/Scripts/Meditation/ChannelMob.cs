using UnityEngine;

/// <summary>
/// Universal "channel" archetype (docs §8): resolved by PRESENCE, not violence — the player stays
/// within channelRadius and attends to it for channelTime seconds, then it resolves (processed /
/// calmed / tamed / neutralized). Reskins per area:
///   Kitchen → procesar un ingrediente · Submarine → calmar una criatura ·
///   Garden → limpiar una plaga · AlchemyLab → neutralizar un radical.
///
/// Optionally drifts around its spawn point so the player has to follow. Non-hostile by default.
/// </summary>
public class ChannelMob : MeditationMob
{
    [Header("Channel (stay near to resolve)")]
    [Tooltip("The player must stay within this distance to channel.")]
    [Min(0.1f)] public float channelRadius = 2f;

    [Tooltip("Seconds of channeling before it resolves.")]
    [Min(0.1f)] public float channelTime = 3f;

    [Tooltip("Channel progress lost per second when the player steps out (× real time).")]
    [Min(0f)] public float decayRate = 1f;

    [Header("Idle drift (optional)")]
    public bool  wanders = true;
    [Min(0f)] public float wanderRadius = 3f;

    float   _channel;
    Vector3 _home;
    Vector3 _wanderTarget;

    protected override void Awake()
    {
        base.Awake();
        _home = transform.position;
        _wanderTarget = _home;
    }

    /// <summary>Channel progress 0–1 (for a future UI ring / VFX).</summary>
    public float ChannelProgress => Mathf.Clamp01(_channel / channelTime);

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= channelRadius)
        {
            _channel += Time.deltaTime;
            if (_channel >= channelTime) { Dissolve(); return; }
        }
        else
        {
            _channel = Mathf.Max(0f, _channel - Time.deltaTime * decayRate);
            if (wanders) Wander();
        }
    }

    void Wander()
    {
        Vector3 flat = transform.position; flat.y = _home.y;
        if (Vector3.Distance(flat, _wanderTarget) < 0.3f)
        {
            Vector2 r = Random.insideUnitCircle * wanderRadius;
            _wanderTarget = _home + new Vector3(r.x, 0f, r.y);
        }
        Vector3 dir = _wanderTarget - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
            transform.position += dir.normalized * (moveSpeed * 0.5f) * Time.deltaTime;
    }
}
