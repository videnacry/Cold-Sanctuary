/// <summary>
/// Anything that can form a bond with the player — companions, animals, objects, places.
/// The bond value drives proximity restoration effects on PlayerStats.
/// </summary>
public interface IBondable
{
    /// <summary>0–100. 100 = unconditional bond.</summary>
    float BondWithPlayer { get; }

    /// <summary>Grow the bond by amount (clamped to 100).</summary>
    void GrowBondWithPlayer(float amount);

    /// <summary>
    /// Net restoration per second this bondable applies to the player when nearby.
    /// Positive = restores. Negative = drains.
    /// Implement based on bond value + internal state.
    /// </summary>
    float GetProximityEffect(StatChannel channel);
}
