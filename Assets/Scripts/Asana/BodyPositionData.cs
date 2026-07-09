using UnityEngine;

/// <summary>
/// Data-only tag for one canonical BodyPosition — lives on a non-rendered GameObject,
/// used as the data source for the Yoga hologram panel (one per BodyPart).
/// </summary>
public class BodyPositionData : MonoBehaviour
{
    public BodyPosition bodyPosition;
}
