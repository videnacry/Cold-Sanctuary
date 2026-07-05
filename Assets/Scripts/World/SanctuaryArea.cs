using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A zone inside Cold Sanctuary. Each area has:
///   - Entry requirements (minimum stats to be placed here)
///   - A set of tasks characters do autonomously
///   - Spawn points where characters are positioned on arrival
///   - A resident list maintained at runtime
///
/// SanctuaryDirector reads progressionTier to determine promotion order.
/// </summary>
public class SanctuaryArea : MonoBehaviour
{
    // ── Identity ──────────────────────────────────────────────────────────────

    [Header("Identity")]
    public SanctuaryAreaType areaType;
    public string displayName;
    [TextArea(2, 3)]
    public string description;

    [Header("Progression")]
    [Tooltip("Lower number = earlier in the progression chain. " +
             "Director promotes characters to the next-lowest tier they qualify for.")]
    public int progressionTier;

    // ── Entry requirements ────────────────────────────────────────────────────

    [Header("Entry Requirements (0 = no requirement)")]
    [Tooltip("Minimum physicalResistance / strength.")]
    public float minStrength;

    [Tooltip("Minimum satisfaction level (0–1).")]
    public float minSatisfaction;

    [Tooltip("Minimum observationRadius (in world units).")]
    public float minObservation;

    // ── Tasks ─────────────────────────────────────────────────────────────────

    [Header("Tasks")]
    [Tooltip("Pool of tasks available in this area. Characters pick randomly from tasks " +
             "whose minProgressionLevel they meet.")]
    public AreaTask[] availableTasks;

    // ── Spawn points ──────────────────────────────────────────────────────────

    [Header("Spawn Points")]
    [Tooltip("Where characters are placed when assigned here. " +
             "Director cycles through these so characters spread out.")]
    public Transform[] spawnPoints;

    // ── Residents (runtime) ───────────────────────────────────────────────────

    readonly List<WorldCharacter> _residents = new List<WorldCharacter>();

    /// <summary>Characters currently assigned to this area.</summary>
    public IReadOnlyList<WorldCharacter> Residents => _residents;

    public void AddResident(WorldCharacter c)    { if (!_residents.Contains(c)) _residents.Add(c); }
    public void RemoveResident(WorldCharacter c) { _residents.Remove(c); }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if the given stat values meet this area's entry requirements.
    /// </summary>
    public bool MeetsRequirements(float strength, float satisfaction, float observation)
        => strength     >= minStrength
        && satisfaction >= minSatisfaction
        && observation  >= minObservation;

    /// <summary>
    /// Returns a random task appropriate for a character at the given progression level.
    /// Falls back to any task if none match the level filter.
    /// </summary>
    public AreaTask GetTask(int progressionLevel)
    {
        if (availableTasks == null || availableTasks.Length == 0) return null;

        // Collect tasks the character is qualified for
        var eligible = System.Array.FindAll(availableTasks,
            t => t.minProgressionLevel <= progressionLevel && !t.requiresVehicle);

        if (eligible.Length == 0)
            eligible = availableTasks;   // fallback: any task

        return eligible[Random.Range(0, eligible.Length)];
    }

    /// <summary>
    /// World-space position of the spawn point at the given index (round-robin safe).
    /// Falls back to this transform's position if no spawn points are configured.
    /// </summary>
    public Vector3 GetSpawnPosition(int index)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
            return spawnPoints[index % spawnPoints.Length].position;
        return transform.position;
    }
}
