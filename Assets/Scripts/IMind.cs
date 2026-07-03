/// <summary>
/// Mental and emotional state contract for any entity with an inner life.
/// Implemented by PlayerStats. Companions implement the lighter IMindSimple.
///
/// Systems that restore or drain player stats should depend on IMind,
/// not on PlayerStats, to stay decoupled from the concrete type.
/// </summary>
public interface IMind
{
    float satisfaction        { get; }
    float satisfactionCapacity { get; }
    float mentalFatigue       { get; }
    float stress              { get; }
    float sleepiness          { get; }
    float observationRadius   { get; }

    /// <summary>Apply restoration from an external source. restorationMultiplier is applied internally.</summary>
    void RestoreMind(float amount, MindChannel channel);

    /// <summary>Apply drain or damage to a mental stat.</summary>
    void DrainMind(float amount, MindChannel channel);
}
