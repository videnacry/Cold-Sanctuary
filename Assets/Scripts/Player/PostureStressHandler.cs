using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Reads IBody.postureStress every frame and applies physical consequences:
///   > StumbleThreshold (0.5) → velocity penalty + camera shake
///   > FallThreshold    (1.0) → trigger fall, interrupt active asana
///
/// Attach to the Player GameObject alongside PlayerStats.
/// Requires: PlayerStats (IBody), CameraManager.Instance (optional — for shake).
/// </summary>
[RequireComponent(typeof(PlayerStats))]
public class PostureStressHandler : MonoBehaviour
{
    [Header("Velocity penalty at stumble")]
    [Tooltip("Multiplier applied to PlayerStats.velocity when stumbling. 0.5 = half speed.")]
    public float stumbleSpeedMultiplier = 0.55f;

    [Tooltip("Seconds before postureStress auto-drains after a bad posture is released.")]
    public float stressDrainDelay = 1.5f;

    [Tooltip("Rate at which postureStress drains per second when no asana is active.")]
    public float stressDrainRate = 0.15f;

    [Header("Fall")]
    [Tooltip("Seconds the player is incapacitated after falling.")]
    public float fallDuration = 1.8f;

    // ── Runtime ───────────────────────────────────────────────────────────────
    PlayerStats _stats;
    AsanaQueue  _queue;
    bool        _isFalling;
    float       _drainTimer;

    // Original velocity — restored after stumble ends
    float _baseVelocity;

    void Awake()
    {
        _stats        = GetComponent<PlayerStats>();
        _queue        = GetComponent<AsanaQueue>();
        _baseVelocity = _stats.velocity;
    }

    void Update()
    {
        float stress = _stats.postureStress;

        if (_isFalling) return;

        // ── Fall ─────────────────────────────────────────────────────────────
        if (stress >= PlayerStats.FallThreshold)
        {
            StartCoroutine(TriggerFall());
            return;
        }

        // ── Stumble ──────────────────────────────────────────────────────────
        if (stress >= PlayerStats.StumbleThreshold)
        {
            float t = (stress - PlayerStats.StumbleThreshold)
                    / (PlayerStats.FallThreshold - PlayerStats.StumbleThreshold);

            _stats.velocity = Mathf.Lerp(_baseVelocity, _baseVelocity * stumbleSpeedMultiplier, t);
            RequestCameraShake(t);
            _drainTimer = 0f;
        }
        else
        {
            // Restore velocity
            _stats.velocity = _baseVelocity;

            // Auto-drain stress when below stumble threshold and no asana is active
            bool asanaActive = _queue != null && _queue.activeAsana != null;
            if (!asanaActive)
            {
                _drainTimer += Time.deltaTime;
                if (_drainTimer >= stressDrainDelay)
                    _stats.ReleasePostureStress(stressDrainRate * Time.deltaTime);
            }
        }
    }

    IEnumerator TriggerFall()
    {
        _isFalling = true;

        // Interrupt active asana
        if (_queue != null && _queue.activeAsana != null)
            _queue.ForceEnd();

        // Full stress drain on fall
        _stats.ReleasePostureStress(1f);

        // Hook: play fall animation, sound
        // e.g. GetComponent<Animator>().SetTrigger("Fall");
        Debug.Log("[PostureStress] Player fell — posture stress critical.");

        RequestCameraShake(1f);

        yield return new WaitForSeconds(fallDuration);

        _isFalling = false;
        _stats.velocity = _baseVelocity;
    }

    void RequestCameraShake(float intensity)
    {
        // CameraManager applies fatigue shake — postureStress piggybacks on it
        // by temporarily boosting mentalFatigue for the camera effect.
        // Replace with a dedicated shake call when CameraManager exposes one.
        if (CameraManager.Instance != null && intensity > 0.3f)
            _stats.DrainMind(intensity * 0.02f * Time.deltaTime, MindChannel.MentalFatigue);
    }
}
