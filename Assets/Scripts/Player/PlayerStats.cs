using UnityEngine;

/// <summary>
/// Holds all runtime stats for the player.
/// Implements IBody (per-limb physical stats + posture stress) and IMind (mental/emotional stats).
/// </summary>
public class PlayerStats : MonoBehaviour, IBody, IMind
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

    // ── IMind explicit implementation ────────────────────────────────────────────

    float IMind.satisfaction         => satisfaction;
    float IMind.satisfactionCapacity => satisfactionCapacity;
    float IMind.mentalFatigue        => mentalFatigue;
    float IMind.stress               => stress;
    float IMind.sleepiness           => sleepiness;
    float IMind.observationRadius    => observationRadius;
    void  IMind.RestoreMind(float amount, MindChannel channel) => RestoreMind(amount, channel);
    void  IMind.DrainMind  (float amount, MindChannel channel) => DrainMind  (amount, channel);

    // ── Public helpers ───────────────────────────────────────────────────────

    /// <summary>Restore a mental stat from an external source (food, rest, companion proximity).
    /// restorationMultiplier scales the gain at high satisfaction levels.</summary>
    public void RestoreMind(float amount, MindChannel channel)
    {
        float scaled = amount * restorationMultiplier;
        switch (channel)
        {
            case MindChannel.Satisfaction:
                satisfaction  = Mathf.Clamp01(satisfaction + scaled / satisfactionCapacity);
                break;
            case MindChannel.MentalFatigue:
                mentalFatigue = Mathf.Clamp01(mentalFatigue - scaled);
                break;
            case MindChannel.Stress:
                stress        = Mathf.Clamp01(stress - scaled);
                break;
            case MindChannel.Sleepiness:
                sleepiness    = Mathf.Clamp01(sleepiness - scaled);
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

    // ── Drain ────────────────────────────────────────────────────────────────

    /// <summary>Apply damage or drain to a mental stat.</summary>
    public void DrainMind(float amount, MindChannel channel)
    {
        switch (channel)
        {
            case MindChannel.MentalFatigue:
                mentalFatigue = Mathf.Clamp01(mentalFatigue + amount);
                break;
            case MindChannel.Stress:
                stress        = Mathf.Clamp01(stress + amount);
                break;
            case MindChannel.Sleepiness:
                sleepiness    = Mathf.Clamp01(sleepiness + amount);
                break;
            case MindChannel.Satisfaction:
                satisfaction  = Mathf.Clamp01(satisfaction - amount / satisfactionCapacity);
                break;
        }
    }
}
