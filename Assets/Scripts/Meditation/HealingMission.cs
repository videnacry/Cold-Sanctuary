using UnityEngine;

/// <summary>
/// "A curar" mission (docs §7/§8): soothe targetCount hurt mobs (HealMob) by staying close to each
/// until it is healed. Reward wires Bond/Satisfaction via MeditationReward + onCompleted.
/// Fits Infirmary, VeterinaryClinic and (as "calm the cub") CubCare.
/// </summary>
public class HealingMission : MeditationMissionBase
{
    [Header("Heal mobs")]
    [Tooltip("Prefab with a HealMob (added if missing). Null → placeholder sphere.")]
    public GameObject healMobPrefab;

    [Tooltip("Placeholder tint when no prefab is set (reskin per area).")]
    public Color placeholderColor = new Color(0.55f, 0.80f, 0.75f);

    [Tooltip("Applied to each spawned mob. Keep slow — it limps toward the player.")]
    public float mobSpeed   = 1.2f;
    public float healRadius = 2f;
    public float healTime   = 3f;

    protected override string BeginMessage =>
        $"[Curar] Consuela a {targetCount}: quédate cerca de cada uno hasta que sane.";

    protected override string CompleteMessage => "[Curar] ✅ Todos calmados. La presencia cura.";

    protected override MeditationMob CreateMob(Vector3 position)
    {
        HealMob mob;
        if (healMobPrefab != null)
        {
            GameObject go = Instantiate(healMobPrefab, position, Quaternion.identity);
            mob = go.GetComponent<HealMob>() ?? go.AddComponent<HealMob>();
        }
        else
        {
            GameObject go = CreatePlaceholderSphere(position, placeholderColor, 0.7f);
            go.name = "HealMob (auto)";
            mob = go.AddComponent<HealMob>();
        }

        mob.moveSpeed  = mobSpeed;
        mob.healRadius = healRadius;
        mob.healTime   = healTime;
        return mob;
    }
}
