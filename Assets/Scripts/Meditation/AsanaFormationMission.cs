using UnityEngine;

/// <summary>
/// Formation mission (docs §7/§8): the postures themselves are mobs that FLEE; the player chases
/// and holds each to "form" it. Completing targetCount formations masters the posture (reward).
///
/// Pairs naturally with PostureVisualizationMission in the same yoga area: distractions chase you,
/// postures flee from you. Spawn / progress / reward live in MeditationMissionBase.
/// </summary>
public class AsanaFormationMission : MeditationMissionBase
{
    [Header("Postures")]
    [Tooltip("Prefab with a PostureFormMob (added if missing). If null, a placeholder sphere is used.")]
    public GameObject posturePrefab;

    [Tooltip("Applied to each spawned posture. Keep postureSpeed at/below player speed so it's catchable.")]
    public float postureSpeed = 2.5f;
    public float fleeDistance = 6f;
    public float holdRadius   = 1.5f;
    public float holdTime     = 2f;

    protected override string BeginMessage =>
        $"[Postura] Forma {targetCount} posturas: persíguelas y sostenlas.";

    protected override string CompleteMessage => "[Postura] ✅ Posturas formadas. El cuerpo recuerda.";

    protected override MeditationMob CreateMob(Vector3 position)
    {
        PostureFormMob mob;
        if (posturePrefab != null)
        {
            GameObject go = Instantiate(posturePrefab, position, Quaternion.identity);
            mob = go.GetComponent<PostureFormMob>() ?? go.AddComponent<PostureFormMob>();
        }
        else
        {
            GameObject go = CreatePlaceholderSphere(position, new Color(0.30f, 0.55f, 0.35f), 0.7f);
            go.name = "PostureMob (auto)";
            mob = go.AddComponent<PostureFormMob>();
        }

        mob.moveSpeed    = postureSpeed;
        mob.fleeDistance = fleeDistance;
        mob.holdRadius   = holdRadius;
        mob.holdTime     = holdTime;
        return mob;
    }
}
