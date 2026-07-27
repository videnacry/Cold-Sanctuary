using UnityEngine;

/// <summary>
/// Holds all runtime stats for the player.
/// Implements IBody (per-limb physical stats + posture stress) and IMind (mental/emotional stats).
/// </summary>
public class PlayerStats : Anima, IBody, IMind
{
    // Migración 2026-07-28: PlayerStats ahora ES un Anima (clase única de ser).
    //   - `stress` se HEREDA de Anima (ya no campo propio).
    //   - Las 12 aptitudes se HEREDAN de Anima (implementa IAptitudes); se retira el mapeo explícito
    //     velocity→Agility (el jugador tiene aptitudes reales, default 1; el builder puede fijarlas).
    //   - `velocity`/`physicalResistance` quedan como valores de movimiento/combate propios.
    //   - Se implementan los 3 hooks abstractos de Anima (stubs).
    // ── Drives heredados de Anima ─────────────────────────────────────────────
    // `satisfaction`, `mentalFatigue`, `sleepiness`, `stress`, `observationRadius`, `velocity`,
    // `physicalResistance` y las 12 aptitudes se HEREDAN de Anima (migración 2026-07-28). `playerStats.X`
    // sigue funcionando (heredado). Aquí quedan solo los parámetros PROPIOS del jugador:
    [Header("Satisfaction (parámetros del jugador)")]
    [Tooltip("Max size of the satisfaction bar. Grows by spending time with Gohageneis.")]
    public float satisfactionCapacity = 1f;

    [Tooltip("Passive fill rate per second at full capacity.")]
    public float satisfactionPassiveRate = 0f;

    [Tooltip("Multiplier applied to all external restoration sources at high satisfaction.")]
    public float restorationMultiplier = 1f;

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

    // ── Hooks de Anima (drives físicos) ───────────────────────────────────────
    // El jugador no reacciona por estos hooks (lo conduce el input/PlayerController); stubs seguros.
    protected override void RespondToHunger() { }
    protected override float EvaluateThreat(GameObject source) => 0f;
    public    override void RespondToThreat(GameObject threat) { }
}
