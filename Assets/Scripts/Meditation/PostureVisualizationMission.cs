using UnityEngine;

/// <summary>
/// Visualization mission (docs §7): the player visualizes a yoga posture they barely master while
/// distracting thoughts (EphemeralThoughtMob) chase them. The player FLEES each so it fails to
/// latch and dissolves. When targetCount thoughts have dissolved, the mind is quiet → the posture
/// is visualized → the mission ends and the reward applies (asana mastery + Observation).
///
/// Spawn / progress / reward / cleanup all live in MeditationMissionBase; this class only spawns
/// the ephemeral-thought archetype.
/// </summary>
public class PostureVisualizationMission : MeditationMissionBase
{
    [Header("Thoughts")]
    [Tooltip("Prefab with an EphemeralThoughtMob (added if missing). If null, a placeholder sphere " +
             "is spawned so the mission is testable without art.")]
    public GameObject thoughtPrefab;

    [Tooltip("Applied to each spawned thought. Keep thoughtSpeed BELOW the player's speed so " +
             "fleeing is viable.")]
    public float thoughtSpeed  = 3f;
    public float dissolveDelay = 3f;
    public float touchRadius   = 1.2f;

    protected override string BeginMessage =>
        $"[Postura] Visualización iniciada. Huye de {targetCount} pensamientos para disolverlos.";

    protected override string CompleteMessage => "[Postura] ✅ Postura visualizada. La mente se aquietó.";

    protected override MeditationMob CreateMob(Vector3 position)
    {
        EphemeralThoughtMob mob;
        if (thoughtPrefab != null)
        {
            GameObject go = Instantiate(thoughtPrefab, position, Quaternion.identity);
            mob = go.GetComponent<EphemeralThoughtMob>() ?? go.AddComponent<EphemeralThoughtMob>();
        }
        else
        {
            GameObject go = CreatePlaceholderSphere(position, new Color(0.22f, 0.12f, 0.30f), 0.6f);
            go.name = "ThoughtMob (auto)";
            mob = go.AddComponent<EphemeralThoughtMob>();
        }

        mob.moveSpeed     = thoughtSpeed;
        mob.dissolveDelay = dissolveDelay;
        mob.touchRadius   = touchRadius;
        return mob;
    }
}
