using UnityEngine;

/// <summary>
/// Holds all runtime stats for the player.
/// Other systems read these values to drive effects, UI, and mechanics.
/// </summary>
public class PlayerStats : MonoBehaviour
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
