using UnityEngine;

/// <summary>
/// Universal channel mission (docs §7 matrix): resolve targetCount ChannelMobs by presence.
/// One component, reskinned per area via labels/colors and the MobMission descriptor:
///   Kitchen (procesar) · Submarine (calmar) · Garden (limpiar) · AlchemyLab (neutralizar).
///
/// Gives most areas "at least one mission" without bespoke code — just add a MobMission + this.
/// </summary>
public class ChannelMission : MeditationMissionBase
{
    [Header("Channel mobs")]
    [Tooltip("Prefab with a ChannelMob (added if missing). If null, a placeholder sphere is used.")]
    public GameObject channelMobPrefab;

    [Tooltip("Placeholder color when no prefab is set — tint per area for readability.")]
    public Color placeholderColor = new Color(0.35f, 0.45f, 0.65f);

    [Tooltip("Applied to each spawned mob.")]
    public float mobSpeed     = 1.5f;
    public float channelRadius = 2f;
    public float channelTime   = 3f;

    protected override string BeginMessage =>
        $"[Canalizar] Atiende a {targetCount}: acércate y mantente presente hasta resolverlos.";

    protected override string CompleteMessage => "[Canalizar] ✅ Todo resuelto con presencia.";

    protected override MeditationMob CreateMob(Vector3 position)
    {
        ChannelMob mob;
        if (channelMobPrefab != null)
        {
            GameObject go = Instantiate(channelMobPrefab, position, Quaternion.identity);
            mob = go.GetComponent<ChannelMob>() ?? go.AddComponent<ChannelMob>();
        }
        else
        {
            GameObject go = CreatePlaceholderSphere(position, placeholderColor, 0.8f);
            go.name = "ChannelMob (auto)";
            mob = go.AddComponent<ChannelMob>();
        }

        mob.moveSpeed     = mobSpeed;
        mob.channelRadius = channelRadius;
        mob.channelTime   = channelTime;
        return mob;
    }
}
