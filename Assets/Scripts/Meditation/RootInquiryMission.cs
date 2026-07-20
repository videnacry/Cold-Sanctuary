using UnityEngine;

/// <summary>
/// "Buscar la raíz" mission (docs §7/§8): obsessive thoughts (AbsorbentThoughtMob) chase and stick
/// to the player, who resolves each by following its thread to its root. Each thought's root is set
/// at its spawn point — the player must return to where the thought was born to see through it.
/// </summary>
public class RootInquiryMission : MeditationMissionBase
{
    [Header("Absorbent thoughts")]
    [Tooltip("Prefab with an AbsorbentThoughtMob (added if missing). Null → placeholder sphere.")]
    public GameObject absorbentPrefab;

    [Tooltip("Keep ABOVE the player's speed — you shouldn't be able to just flee it.")]
    public float speed = 4.5f;
    public float stickRadius = 1.5f;
    public float rootRadius  = 2f;

    protected override string BeginMessage =>
        $"[Raíz] {targetCount} pensamientos obsesivos te persiguen. No puedes huir: sigue el hilo " +
        "hasta su raíz para disolverlos.";

    protected override string CompleteMessage =>
        "[Raíz] ✅ Los viste a través desde su origen. Se disolvieron.";

    protected override MeditationMob CreateMob(Vector3 position)
    {
        AbsorbentThoughtMob mob;
        if (absorbentPrefab != null)
        {
            GameObject go = Instantiate(absorbentPrefab, position, Quaternion.identity);
            mob = go.GetComponent<AbsorbentThoughtMob>() ?? go.AddComponent<AbsorbentThoughtMob>();
        }
        else
        {
            GameObject go = CreatePlaceholderSphere(position, new Color(0.50f, 0.15f, 0.20f), 0.65f);
            go.name = "AbsorbentMob (auto)";
            mob = go.AddComponent<AbsorbentThoughtMob>();
        }

        mob.moveSpeed   = speed;
        mob.stickRadius = stickRadius;
        mob.rootRadius  = rootRadius;
        mob.SetRoot(position); // root = spawn point; the player must return here
        return mob;
    }
}
