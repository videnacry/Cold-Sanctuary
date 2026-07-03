using System;
using UnityEngine;

/// <summary>
/// Represents a single body part placement that composes an Asana or Exercise.
/// Stat requirements define the minimum IBody values needed to execute correctly.
/// A value of 0 means any player can attempt it regardless of stats.
/// </summary>
[Serializable]
public class BodyPosition
{
    public BodyPart bodyPart;
    public string description;   // e.g. "Pie de atrás girado en 45°"
    public KeyCode hotkey;       // desktop key aligned to typing position

    [Header("Stat Requirements (0 = no requirement)")]
    [Range(0f, 1f)] public float requiredFlexibility;
    [Range(0f, 1f)] public float requiredStrength;
    [Range(0f, 1f)] public float requiredStability;

    public BodyPosition(BodyPart pBodyPart, string pDescription, KeyCode pHotkey)
    {
        bodyPart    = pBodyPart;
        description = pDescription;
        hotkey      = pHotkey;
    }

    /// <summary>
    /// Evaluate how well a player with the given stats can execute this position.
    /// Returns the quality and the gap+dimension that limits them most.
    /// </summary>
    public PositionEvaluation Evaluate(BodyPartStats stats)
    {
        float flexGap = Mathf.Max(0f, requiredFlexibility - stats.flexibility);
        float strGap  = Mathf.Max(0f, requiredStrength    - stats.strength);
        float stabGap = Mathf.Max(0f, requiredStability   - stats.stability);

        float totalGap = Mathf.Max(flexGap, Mathf.Max(strGap, stabGap));

        BodyStatDimension limiting =
            flexGap >= strGap && flexGap >= stabGap ? BodyStatDimension.Flexibility :
            strGap  >= stabGap                      ? BodyStatDimension.Strength    :
                                                      BodyStatDimension.Stability;

        PositionQuality quality =
            totalGap == 0f    ? PositionQuality.Correct            :
            totalGap <= 0.7f  ? PositionQuality.DirectionOkLowStat :
                                PositionQuality.Impossible;

        return new PositionEvaluation
        {
            part         = bodyPart,
            quality      = quality,
            statGap      = totalGap,
            limitingDim  = limiting,
        };
    }
}

public enum BodyPart
{
    Elbows,
    Hands,
    Knees,
    Feet,
    Hips,
    Back,
    Shoulders,
    Head
}
