using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data asset for an Asana (yoga pose) or basic exercise.
/// Create via: right-click in Project → Create → ColdSanctuary → Asana
/// </summary>
[CreateAssetMenu(fileName = "NewAsana", menuName = "ColdSanctuary/Asana")]
public class Asana : ScriptableObject
{
    [Header("Identity")]
    public string displayName;
    public bool isExercise;       // true = push-up / legs / abs
    public bool hasTwoSides;
    public bool isUnlocked = false;

    [Header("Required Body Positions")]
    public BodyPosition[] requiredPositions;

    [Header("Stat")]
    public StatType statType;
    [Tooltip("Which PlayerStats channel this asana restores when practiced.")]
    public MindChannel channel = MindChannel.MentalFatigue;
    public float containerBase = 50f;
    public float containerMax  = 100f;

    [Header("Execute Button Labels by Mastery")]
    public string labelBeginner    = "Respirar";   // mastery 0-2
    public string labelIntermediate = "Relajar";   // mastery 3-5
    public string labelExpert      = "Meditar";    // mastery 6+

    // Runtime state (not serialized to asset — lives in AsanaQueue)
    [System.NonSerialized] public int  practiceCount   = 0;
    [System.NonSerialized] public int  masteryLevel    = 0;
    [System.NonSerialized] public float containerCurrent;

    public const int PracticesPerMasteryTick    = 5;
    public const int ShortcutUnlockThreshold    = 10;

    public bool   ShortcutUnlocked  => practiceCount >= ShortcutUnlockThreshold;
    public float  ErrorProbability  => Mathf.Max(0f, 0.4f - (masteryLevel * 0.05f));

    public string ExecuteLabel => masteryLevel >= 6 ? labelExpert
                                : masteryLevel >= 3 ? labelIntermediate
                                : labelBeginner;

    public void InitRuntime()
    {
        containerCurrent = containerBase;
    }

    public void RegisterPractice()
    {
        practiceCount++;
        int newMastery = practiceCount / PracticesPerMasteryTick;
        if (newMastery > masteryLevel)
        {
            masteryLevel     = newMastery;
            containerCurrent = Mathf.Min(containerBase + masteryLevel, containerMax);
        }
    }

    /// <summary>Returns which positions are "wrong" when using shortcut.</summary>
    public List<BodyPosition> RollErrors()
    {
        var errors = new List<BodyPosition>();
        foreach (var pos in requiredPositions)
            if (Random.value < ErrorProbability) errors.Add(pos);
        return errors;
    }
}

public enum StatType
{
    StrengthAndGrounding,
    Flexibility,
    Balance,
    Endurance
}
