/// <summary>
/// Physical body interface — per-limb stats and posture stress.
/// Implemented by the player (PlayerStats) and eventually by NPCs/animals.
///
/// Focused on what the asana system needs today.
/// Will be extended when IAnimal is renamed to IBody (separate task — see DEVLOG §IBody/IMind).
/// </summary>
public interface IBody
{
    /// <summary>Stats (flexibility/strength/stability) for a specific body part.</summary>
    BodyPartStats GetBodyPartStats(BodyPart part);

    /// <summary>Grow a specific dimension for a body part. delta is typically small (0.001–0.01).</summary>
    void TrainBodyPart(BodyPart part, BodyStatDimension dimension, float delta);

    /// <summary>0–1. Accumulated structural stress from bad posture. Triggers stumbling at 0.5, fall at 1.</summary>
    float postureStress { get; }

    void AccumulatePostureStress(float amount);
    void ReleasePostureStress(float amount);
}
