/// <summary>
/// Anything that can form a bond — companions, NPCs, animals, objects, places.
/// Bonds are bilateral and generic: Goluis stores her bond with Gohageneis the same
/// way she stores her bond with the player or a favourite spot. No entity gets
/// special treatment in the interface.
/// </summary>
public interface IBondable
{
    /// <summary>0–100. Bond strength with source. 0 = strangers, 100 = unconditional.</summary>
    float GetBondStrength(UnityEngine.MonoBehaviour source);

    /// <summary>Grow (or shrink) the bond with source. Clamped to 0–100.</summary>
    void GrowBond(UnityEngine.MonoBehaviour source, float amount);

    /// <summary>
    /// Net restoration per second on channel when source is nearby.
    /// Positive = restores source. Negative = drains source.
    /// Driven by bond strength + this entity's current internal state.
    /// </summary>
    float GetProximityEffect(UnityEngine.MonoBehaviour source, MindChannel channel);
}
