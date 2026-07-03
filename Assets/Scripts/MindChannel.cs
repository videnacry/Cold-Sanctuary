/// <summary>
/// Identifies which mental/emotional stat to target when restoring or draining.
/// Used by IMind, IBondable, CompanionBase, and all effect systems.
/// Replaces the former StatChannel enum.
/// </summary>
public enum MindChannel
{
    Satisfaction,
    MentalFatigue,
    Stress,
    Sleepiness,
    Observation
}
