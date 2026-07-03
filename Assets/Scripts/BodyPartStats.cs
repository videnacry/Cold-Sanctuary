using UnityEngine;

/// <summary>
/// Physical stats for a single body part.
/// All dimensions go 0–1. Initial values are low — they grow through practice.
/// Lives in IBody, indexed by BodyPart enum.
/// </summary>
[System.Serializable]
public class BodyPartStats
{
    [Range(0f, 1f)]
    [Tooltip("Range of motion. Low = can't reach the required position.")]
    public float flexibility;

    [Range(0f, 1f)]
    [Tooltip("Load-bearing capacity. Low = can't hold the position under weight.")]
    public float strength;

    [Range(0f, 1f)]
    [Tooltip("Balance and control in this zone. Low = unstable, wobbles.")]
    public float stability;

    public float Get(BodyStatDimension dim)
    {
        switch (dim)
        {
            case BodyStatDimension.Flexibility: return flexibility;
            case BodyStatDimension.Strength:    return strength;
            case BodyStatDimension.Stability:   return stability;
            default: return 0f;
        }
    }

    public void Train(BodyStatDimension dim, float delta)
    {
        switch (dim)
        {
            case BodyStatDimension.Flexibility:
                flexibility = Mathf.Clamp01(flexibility + delta); break;
            case BodyStatDimension.Strength:
                strength    = Mathf.Clamp01(strength    + delta); break;
            case BodyStatDimension.Stability:
                stability   = Mathf.Clamp01(stability   + delta); break;
        }
    }
}

public enum BodyStatDimension { Flexibility, Strength, Stability }
