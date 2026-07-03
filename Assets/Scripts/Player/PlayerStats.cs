using UnityEngine;

/// <summary>
/// Holds all runtime stats for the player.
/// Implements IBody (per-limb physical stats + posture stress).
/// Candidate to also implement IMind when that interface is defined — see DEVLOG §IBody/IMind.
/// </summary>
public class PlayerStats : MonoBehaviour, IBody
{
    // ── Satisfaction ────────────────────────────────────────────────────────
    [Header("Satisfaction")]
    [Tooltip("Current satisfaction level. Grows passively when capacity is high.")]
    [Range(0f, 1f)] public float satisfaction;

    [Tooltip("Max size of the satisfaction bar. Grows by spending time with Gohageneis.")]
    public float satisfactionCapacity = 1f;

    [Tooltip("Passive fill rate per second at full capacity.")]
    public float satisfactionPassiveRate = 0f;

    [Tooltip("Multiplier applied to all external restoration sources at high satisfaction.")]
    public float restorationMultiplier = 1f;

    // ── Mental fatigue ───────────────────────────────────────────────────────
    [Header("Mental Fatigue")]
    [Tooltip("0 = fresh, 1 = at limit. High values cause screen effects and stumbling.")]
    [Range(0f, 1f)] public float mentalFatigue;

    // ── Stress ──────────────────────────────────────────────────────────────
    [Header("Stress")]
    [Tooltip("0 = calm, 1 = critical. Raised by Goluis pressure, lowered by rest.")]
    [Range(0f, 1f)] public float stress;

    // ── Sleepiness ──────────────────────────────────────────────────────────
    [Header("Sleepiness")]
    [Tooltip("0 = awake, 1 = about to black out.")]
    [Range(0f, 1f)] public float sleepiness;

    // ── Observation ─────────────────────────────────────────────────────────
    [Header("Observation")]
    [Tooltip("Radius of consciousness. Observable objects glow/emit cues within this range.")]
    public float observationRadius = 3f;

    // ── Velocity / Resistance ───────────────────────────────────────────────
    [Header("Physical Stats")]
    public float velocity = 1f;
    public float physicalResistance = 1f;

    // ── IBody — per-limb stats ───────────────────────────────────────────────
    // Array indexed by (int)BodyPart: Elbows=0, Hands=1, Knees=2, Feet=3, Hips=4, Back=5, Shoulders=6, Head=7
    [Header("Per-Limb Stats (IBody)")]
    public BodyPartStats[] bodyStats = new BodyPartStats[8];

    [HideInInspector] public float postureStress { get; private set; }

    // ── PostureStress thresholds ─────────────────────────────────────────────
    public const float StumbleThreshold = 0.5f;
    public const float FallThreshold    = 1.0f;

    void Update()
    {
        // Passive satisfaction fill
        if (satisfactionPassiveRate > 0f)
        {
            float gain = satisfactionPassiveRate * Time.deltaTime;
            satisfaction = Mathf.Clamp01(satisfaction + gain / satisfactionCapacity);
        }
    }

    // ── Public helpers ───────────────────────────────────────────────────────

    /// <summary>Apply restoration from an external source (food, rest, companion proximity).
    /// The restorationMultiplier scales the gain at high satisfaction levels.</summary>
    public void Restore(float amount, StatChannel channel)
    {
        float scaled = amount * restorationMultiplier;
        switch (channel)
        {
            case StatChannel.Satisfaction:
                satisfaction = Mathf.Clamp01(satisfaction + scaled / satisfactionCapacity);
                break;
            case StatChannel.MentalFatigue:
                mentalFatigue = Mathf.Clamp01(mentalFatigue - scaled);
                break;
            case StatChannel.Stress:
                stress = Mathf.Clamp01(stress - scaled);
                break;
            case StatChannel.Sleepiness:
                sleepiness = Mathf.Clamp01(sleepiness - scaled);
                break;
        }
    }

    // ── IBody implementation ─────────────────────────────────────────────────

    public BodyPartStats GetBodyPartStats(BodyPart part)
    {
        int idx = (int)part;
        if (idx < 0 || idx >= bodyStats.Length) return new BodyPartStats();
        return bodyStats[idx];
    }

    public void TrainBodyPart(BodyPart part, BodyStatDimension dimension, float delta)
    {
        int idx = (int)part;
        if (idx < 0 || idx >= bodyStats.Length) return;
        bodyStats[idx].Train(dimension, delta);
    }

    public void AccumulatePostureStress(float amount)
        => postureStress = Mathf.Clamp01(postureStress + amount);

    public void ReleasePostureStress(float amount)
        => postureStress = Mathf.Clamp01(postureStress - amount);

    // ── Apply damage/drain ───────────────────────────────────────────────────

    /// <summary>Apply damage/drain to a stat channel.</summary>
    public void Drain(float amount, StatChannel channel)
    {
        switch (channel)
        {
            case StatChannel.MentalFatigue:
                mentalFatigue = Mathf.Clamp01(mentalFatigue + amount);
                break;
            case StatChannel.Stress:
                stress = Mathf.Clamp01(stress + amount);
                break;
            case StatChannel.Sleepiness:
                sleepiness = Mathf.Clamp01(sleepiness + amount);
                break;
            case StatChannel.Satisfaction:
                satisfaction = Mathf.Clamp01(satisfaction - amount / satisfactionCapacity);
                break;
        }
    }
}

public enum StatChannel
{
    Satisfaction,
    MentalFatigue,
    Stress,
    Sleepiness
}
