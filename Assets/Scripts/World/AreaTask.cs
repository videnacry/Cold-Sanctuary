using UnityEngine;

/// <summary>
/// Defines one task available inside a SanctuaryArea.
///
/// Tasks encode:
///   - How long they take
///   - Which stats they affect (mind + physical)
///   - Which periodic-table element they can reveal (for the enchantment system)
///   - Any prerequisites (vehicle, progression level)
///
/// Serializable so they can be configured directly in the SanctuaryArea inspector.
/// </summary>
[System.Serializable]
public class AreaTask
{
    // ── Identity ─────────────────────────────────────────────────────────────

    [Tooltip("Short name shown in logs and future UI.")]
    public string taskName = "Unnamed Task";

    [TextArea(2, 4)]
    [Tooltip("Description of what the character does and learns.")]
    public string description;

    // ── Duration ─────────────────────────────────────────────────────────────

    [Header("Duration")]
    [Tooltip("Real-time seconds per task cycle. Scale via TimeController for compressed game time.")]
    public float duration = 15f;

    // ── Mind effects ─────────────────────────────────────────────────────────

    [Header("Mind Effects (applied on completion)")]
    [Tooltip("Which mental channel this task primarily affects.")]
    public MindChannel mindChannel = MindChannel.MentalFatigue;

    [Tooltip("Per-completion effect. Positive = restore (reduces fatigue / raises satisfaction). " +
             "Negative = drain (adds fatigue / lowers satisfaction).")]
    public float mindEffect = 0.02f;

    // ── Physical effects ─────────────────────────────────────────────────────

    [Header("Physical Effects (applied on completion)")]
    [Tooltip("Delta on physicalResistance / strength. Clamped to [0,1] by WorldCharacter.")]
    public float strengthDelta;

    [Tooltip("Delta on observationRadius. Unclamped — radius can grow beyond 1.")]
    public float observationDelta;

    [Tooltip("Delta on velocity. Clamped to [0.1, 5] by WorldCharacter.")]
    public float velocityDelta;

    // ── Periodic table ────────────────────────────────────────────────────────

    [Header("Periodic Table Discovery")]
    [Tooltip("Chemical symbol (e.g. 'C', 'Na', 'H2') this task can reveal. Leave empty for no discovery.")]
    public string elementSymbol;

    [Tooltip("Probability per completion that the element is discovered (if not known yet).")]
    [Range(0f, 1f)]
    public float elementDiscoveryChance = 0.25f;

    [Tooltip("Optional flavor text shown when the element is discovered.")]
    [TextArea(1, 3)]
    public string discoveryFlavor;

    // ── Requirements ─────────────────────────────────────────────────────────

    [Header("Requirements")]
    [Tooltip("Character must be at or above this progression level to receive this task.")]
    public int minProgressionLevel;

    [Tooltip("Task requires a vehicle (tractor / boat / submarine) to be operational.")]
    public bool requiresVehicle;
}
