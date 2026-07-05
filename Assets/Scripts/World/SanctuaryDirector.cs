using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Magnate's world management component — the brain of the sanctuary's
/// autonomous simulation.
///
/// Responsibilities:
///   - Singleton: all WorldCharacters call Register() in their Start().
///   - Intercepts new arrivals, assesses their stats, and assigns them to
///     the appropriate starting area.
///   - Listens for OnReadyForPromotion on every WorldCharacter.
///   - Groups characters that are ready for the same next tier and
///     orchestrates individual or group promotions.
///   - Exposes query helpers (GetArea, GetCharactersInArea) for other systems.
///
/// Setup in the inspector:
///   1. Drag all SanctuaryArea GameObjects into allAreas[].
///   2. (Optional) Assign assessmentPoint and groupMeetingPoint transforms.
///   3. Assign magnateCharacter if the Magnate is herself a WorldCharacter.
/// </summary>
public class SanctuaryDirector : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────

    public static SanctuaryDirector Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("The Magnate")]
    [Tooltip("WorldCharacter component on the Magnate herself (optional).")]
    public WorldCharacter magnateCharacter;

    [Header("World Areas")]
    [Tooltip("All SanctuaryArea instances in the scene. " +
             "Will be sorted by progressionTier at runtime.")]
    public SanctuaryArea[] allAreas;

    [Header("Assessment")]
    [Tooltip("Transform the Magnate stands at when greeting a new arrival. " +
             "Used as a cinematic anchor; actual movement is not handled here.")]
    public Transform assessmentPoint;

    [Tooltip("Seconds to wait before assigning a new arrival. " +
             "Gives cutscenes and transitions time to play.")]
    public float assessmentDelay = 2f;

    [Header("Group Promotion")]
    [Tooltip("World position where characters gather before a group promotion ceremony.")]
    public Transform groupMeetingPoint;

    [Tooltip("Seconds the system waits after the first promotion request, " +
             "collecting other characters that become ready in the same window.")]
    public float groupGatherWindow = 5f;

    [Tooltip("Seconds between the gathering animation and the actual area placement.")]
    public float groupCeremonyDuration = 3f;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired after a character is placed in a new area (initial or promoted).</summary>
    public event Action<WorldCharacter, SanctuaryArea> OnCharacterPlaced;

    /// <summary>Fired when two or more characters are promoted together.</summary>
    public event Action<List<WorldCharacter>, SanctuaryArea> OnGroupPromotion;

    // ── Runtime ───────────────────────────────────────────────────────────────

    readonly List<WorldCharacter> _allCharacters  = new List<WorldCharacter>();
    readonly List<WorldCharacter> _promotionQueue = new List<WorldCharacter>();
    bool _promotionRoutineRunning;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Sort areas by progression tier so DetermineStartArea and DetermineNextArea
        // can walk the array in order without extra sorting each call.
        if (allAreas != null && allAreas.Length > 0)
            Array.Sort(allAreas, (a, b) => a.progressionTier.CompareTo(b.progressionTier));
    }

    void OnDestroy()
    {
        foreach (var c in _allCharacters)
            if (c != null) c.OnReadyForPromotion -= HandleReadyForPromotion;

        if (Instance == this) Instance = null;
    }

    // ── Registration ──────────────────────────────────────────────────────────

    /// <summary>
    /// Called by every WorldCharacter in their Start(). Safe to call multiple times.
    /// New arrivals are automatically routed through the assessment sequence.
    /// </summary>
    public void Register(WorldCharacter character)
    {
        if (_allCharacters.Contains(character)) return;
        _allCharacters.Add(character);
        character.OnReadyForPromotion += HandleReadyForPromotion;

        if (character.isNewArrival)
            StartCoroutine(AssessArrival(character));
    }

    // ── Assessment ────────────────────────────────────────────────────────────

    IEnumerator AssessArrival(WorldCharacter character)
    {
        yield return new WaitForSeconds(assessmentDelay);

        SanctuaryArea startArea = DetermineStartArea(character);

        if (startArea == null)
        {
            Debug.LogWarning(
                $"[SanctuaryDirector] Sin área disponible para '{character.characterName}' " +
                $"(Fuerza={character.Strength:F2}, Satisfacción={character.Satisfaction:F2}, " +
                $"Observación={character.Observation:F2}). Asegúrate de que allAreas está configurado.");
            yield break;
        }

        int spawnIdx = startArea.Residents.Count;
        character.progressionLevel = 0;
        character.PlaceInArea(startArea, spawnIdx);

        OnCharacterPlaced?.Invoke(character, startArea);

        Debug.Log(
            $"[Magnate] Evaluación de '{character.characterName}' completa. " +
            $"Área inicial: {startArea.displayName} (Tier {startArea.progressionTier}).");
    }

    /// <summary>
    /// Returns the highest-tier area the character currently qualifies for.
    /// Falls back to allAreas[0] (the lowest tier) if nothing matches.
    /// </summary>
    SanctuaryArea DetermineStartArea(WorldCharacter character)
    {
        if (allAreas == null || allAreas.Length == 0) return null;

        SanctuaryArea best = null;
        foreach (var area in allAreas)   // sorted ascending by tier
        {
            if (area.MeetsRequirements(character.Strength, character.Satisfaction, character.Observation))
                best = area;
        }

        // Newcomers with near-zero stats won't qualify for anything → place in Tier 1
        return best ?? allAreas[0];
    }

    // ── Promotion ─────────────────────────────────────────────────────────────

    void HandleReadyForPromotion(WorldCharacter character)
    {
        if (!_promotionQueue.Contains(character))
            _promotionQueue.Add(character);

        if (!_promotionRoutineRunning)
            StartCoroutine(ProcessPromotionQueue());
    }

    IEnumerator ProcessPromotionQueue()
    {
        _promotionRoutineRunning = true;

        // Wait for the gather window — other characters may become ready in this time.
        yield return new WaitForSeconds(groupGatherWindow);

        if (_promotionQueue.Count == 0)
        {
            _promotionRoutineRunning = false;
            yield break;
        }

        // Group characters by their destination tier.
        var byDestinationTier = new Dictionary<int, (List<WorldCharacter> chars, SanctuaryArea area)>();

        foreach (var character in _promotionQueue)
        {
            SanctuaryArea next = DetermineNextArea(character);
            if (next == null)
            {
                Debug.Log(
                    $"[Magnate] '{character.characterName}' ha completado todas las áreas disponibles. " +
                    $"Nivel máximo alcanzado.");
                continue;
            }

            int tier = next.progressionTier;
            if (!byDestinationTier.ContainsKey(tier))
                byDestinationTier[tier] = (new List<WorldCharacter>(), next);

            byDestinationTier[tier].chars.Add(character);
        }

        _promotionQueue.Clear();

        // Process each destination group.
        foreach (var kv in byDestinationTier)
        {
            var group = kv.Value.chars;
            var nextArea = kv.Value.area;

            bool isGroup = group.Count > 1;

            if (isGroup)
            {
                // Announce the group gathering (cinematic hook).
                Debug.Log(
                    $"[Magnate] ¡{group.Count} personajes listos para avanzar! " +
                    $"Punto de reunión → {nextArea.displayName}.");
                OnGroupPromotion?.Invoke(group, nextArea);
                yield return new WaitForSeconds(groupCeremonyDuration);
            }

            // Promote each character.
            foreach (var character in group)
            {
                character.progressionLevel++;
                int spawnIdx = nextArea.Residents.Count;
                character.PlaceInArea(nextArea, spawnIdx);
                OnCharacterPlaced?.Invoke(character, nextArea);

                Debug.Log(
                    $"[Magnate] '{character.characterName}' promovido/a → " +
                    $"{nextArea.displayName} (Nivel {character.progressionLevel}).");
            }
        }

        _promotionRoutineRunning = false;
    }

    /// <summary>
    /// Finds the lowest-tier area ABOVE the character's current tier that they qualify for.
    /// Returns null if the character has reached the highest available area.
    /// </summary>
    SanctuaryArea DetermineNextArea(WorldCharacter character)
    {
        if (character.currentArea == null) return DetermineStartArea(character);

        int currentTier = character.currentArea.progressionTier;

        foreach (var area in allAreas)   // sorted ascending by tier
        {
            if (area.progressionTier > currentTier
                && area.MeetsRequirements(character.Strength, character.Satisfaction, character.Observation))
                return area;
        }

        return null;   // already at the highest tier
    }

    // ── Public queries ────────────────────────────────────────────────────────

    /// <summary>All characters registered with the Director (player + NPCs).</summary>
    public IReadOnlyList<WorldCharacter> AllCharacters => _allCharacters;

    /// <summary>Characters currently assigned to a specific area type.</summary>
    public List<WorldCharacter> GetCharactersInArea(SanctuaryAreaType type)
    {
        var result = new List<WorldCharacter>();
        foreach (var c in _allCharacters)
            if (c.currentArea != null && c.currentArea.areaType == type)
                result.Add(c);
        return result;
    }

    /// <summary>First SanctuaryArea instance matching the given type, or null.</summary>
    public SanctuaryArea GetArea(SanctuaryAreaType type)
    {
        if (allAreas == null) return null;
        foreach (var a in allAreas)
            if (a.areaType == type) return a;
        return null;
    }

    /// <summary>
    /// Manually trigger an assessment for a character that was already registered
    /// (e.g., after a scene reload or a respawn).
    /// </summary>
    public void Reassess(WorldCharacter character)
    {
        character.isNewArrival = true;
        StartCoroutine(AssessArrival(character));
    }
}
