using UnityEngine;

/// <summary>
/// Defines one mission available in the sanctuary.
/// Create via Assets > Create > Cold Sanctuary > Mission.
///
/// Mission types:
///   IngredientCollection — collect N fragments of a specific element
///   YeastControl         — keep yeast population below a threshold for T seconds
///   AreaClear            — process all mobs in the area
///
/// Rewards: coins + optional item drop.
/// Excess fragments collected beyond the mission requirement can be sold.
/// </summary>
[CreateAssetMenu(menuName = "Cold Sanctuary/Mission", fileName = "NewMission")]
public class SanctuaryMission : ScriptableObject
{
    // ── Identity ──────────────────────────────────────────────────────────────

    [Header("Identity")]
    public string missionName;

    [TextArea(2, 4)]
    public string description;

    public SanctuaryAreaType area;

    // ── Type ──────────────────────────────────────────────────────────────────

    [Header("Type")]
    public MissionType missionType;

    [Tooltip("IngredientCollection: element to collect (e.g. 'S', 'Na').")]
    public string targetElement;

    [Tooltip("IngredientCollection: number of fragments required. " +
             "YeastControl: max allowed yeast population.")]
    public int targetCount = 5;

    [Tooltip("YeastControl: seconds the population must stay below targetCount.")]
    public float controlDuration = 30f;

    // ── Reward ────────────────────────────────────────────────────────────────

    [Header("Reward")]
    [Tooltip("Coins awarded on completion.")]
    public int coinReward = 20;

    [Tooltip("Optional item awarded on completion. Leave null for coin-only reward.")]
    public ItemData itemReward;

    // ── Unlock ────────────────────────────────────────────────────────────────

    [Header("Unlock")]
    [Tooltip("Minimum player progression level to receive this mission.")]
    public int minProgressionLevel;
}

public enum MissionType
{
    IngredientCollection,   // collect N element fragments
    YeastControl,           // keep yeast population < targetCount for controlDuration seconds
    AreaClear,              // process every mob currently in the area
}
