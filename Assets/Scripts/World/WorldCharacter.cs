using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to any entity that participates in the sanctuary's world simulation:
/// the player, NPCs, and companions.
///
/// Responsibilities:
///   - Tracks current area and progression level.
///   - Runs an autonomous task loop (executes area tasks, applies stat effects).
///   - Notifies SanctuaryDirector when promotion thresholds are met.
///   - Bridges to PlayerStats (if present) or manages lightweight own stats for NPCs.
///
/// The player and NPCs are equals in this system — only isPlayer distinguishes
/// who controls moment-to-moment actions.
/// </summary>
public class WorldCharacter : MonoBehaviour
{
    // ── Identity ──────────────────────────────────────────────────────────────

    [Header("Identity")]
    public string characterName;

    [Tooltip("True for the human-controlled player. Autonomous loop still runs, " +
             "but task effects apply only when the player is idle (not explicitly overridden yet).")]
    public bool isPlayer;

    // ── World state ───────────────────────────────────────────────────────────

    [Header("World State")]
    [Tooltip("Set true on first arrival in the sanctuary. " +
             "SanctuaryDirector will intercept and run the assessment sequence.")]
    public bool isNewArrival = true;

    [Tooltip("Current area this character is assigned to.")]
    public SanctuaryArea currentArea;

    [Tooltip("World progression level (0 = newly arrived). " +
             "Advanced by SanctuaryDirector after each promotion.")]
    public int progressionLevel;

    // ── Promotion thresholds ──────────────────────────────────────────────────

    [Header("Promotion Thresholds")]
    [Tooltip("physicalResistance / strength required to be considered for promotion.")]
    public float promotionStrength     = 0.3f;

    [Tooltip("satisfaction level required.")]
    public float promotionSatisfaction = 0.2f;

    [Tooltip("observationRadius (world units) required.")]
    public float promotionObservation  = 2f;

    // ── Lightweight NPC stats (used when PlayerStats is absent) ───────────────

    [Header("NPC Stats (ignored when PlayerStats is present on this GameObject)")]
    [Range(0f, 1f)] public float strength     = 0f;
    [Range(0f, 1f)] public float satisfaction = 0f;
    public float observation  = 1f;
    [Range(0f, 1f)] public float mentalFatigue;
    [Range(0f, 1f)] public float stress;
    public float velocity = 1f;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired when all promotion thresholds are met. SanctuaryDirector listens here.</summary>
    public event Action<WorldCharacter> OnReadyForPromotion;

    /// <summary>Fired when a periodic-table element is discovered through a task.</summary>
    public event Action<WorldCharacter, string> OnElementDiscovered;

    // ── Runtime ───────────────────────────────────────────────────────────────

    PlayerStats _playerStats;
    bool        _taskLoopRunning;
    bool        _promotionFired;   // guard: fire once per area

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
    }

    void Start()
    {
        SanctuaryDirector.Instance?.Register(this);
    }

    // ── Stat accessors ────────────────────────────────────────────────────────

    /// <summary>Strength / physicalResistance — reads PlayerStats if present.</summary>
    public float Strength
    {
        get => _playerStats != null ? _playerStats.physicalResistance : strength;
        set { if (_playerStats != null) _playerStats.physicalResistance = value; else strength = Mathf.Clamp01(value); }
    }

    /// <summary>Satisfaction — reads PlayerStats if present.</summary>
    public float Satisfaction
    {
        get => _playerStats != null ? _playerStats.satisfaction : satisfaction;
        set { if (_playerStats != null) _playerStats.satisfaction = Mathf.Clamp01(value); else satisfaction = Mathf.Clamp01(value); }
    }

    /// <summary>ObservationRadius — reads PlayerStats if present.</summary>
    public float Observation
    {
        get => _playerStats != null ? _playerStats.observationRadius : observation;
        set { if (_playerStats != null) _playerStats.observationRadius = Mathf.Max(0f, value); else observation = Mathf.Max(0f, value); }
    }

    // ── Promotion ─────────────────────────────────────────────────────────────

    /// <summary>True when all thresholds for this character's current area are met.</summary>
    public bool IsReadyForPromotion()
        => !_promotionFired
        && Strength     >= promotionStrength
        && Satisfaction >= promotionSatisfaction
        && Observation  >= promotionObservation;

    // ── Area placement ────────────────────────────────────────────────────────

    /// <summary>
    /// Called by SanctuaryDirector to move this character to a new area.
    /// Restarts the autonomous task loop for the new context.
    /// </summary>
    public void PlaceInArea(SanctuaryArea area, int spawnIndex = 0)
    {
        if (currentArea != null)
            currentArea.RemoveResident(this);

        currentArea     = area;
        _promotionFired = false;
        area.AddResident(this);

        // Reposition
        transform.position = area.GetSpawnPosition(spawnIndex);

        // Restart autonomous loop
        StopAllCoroutines();
        _taskLoopRunning = false;
        isNewArrival     = false;

        StartCoroutine(AutonomousTaskLoop());
    }

    // ── Autonomous task loop ──────────────────────────────────────────────────

    IEnumerator AutonomousTaskLoop()
    {
        _taskLoopRunning = true;

        while (true)
        {
            if (currentArea == null)
            {
                yield return new WaitForSeconds(5f);
                continue;
            }

            AreaTask task = currentArea.GetTask(progressionLevel);
            if (task == null)
            {
                yield return new WaitForSeconds(5f);
                continue;
            }

            // Work the task
            yield return new WaitForSeconds(task.duration);
            ApplyTaskEffects(task);

            // Periodic table discovery
            if (!string.IsNullOrEmpty(task.elementSymbol)
                && UnityEngine.Random.value < task.elementDiscoveryChance)
            {
                OnElementDiscovered?.Invoke(this, task.elementSymbol);
                if (!string.IsNullOrEmpty(task.discoveryFlavor))
                    Debug.Log($"[{characterName}] Descubrimiento: {task.elementSymbol} — {task.discoveryFlavor}");
            }

            // Check promotion (once per area cycle)
            if (IsReadyForPromotion())
            {
                _promotionFired = true;
                OnReadyForPromotion?.Invoke(this);
                _taskLoopRunning = false;
                yield break;   // Director will call PlaceInArea again after promotion
            }
        }
    }

    void ApplyTaskEffects(AreaTask task)
    {
        if (_playerStats != null)
        {
            // Route through PlayerStats so restorationMultiplier and clamps apply
            if (task.mindEffect > 0)
                _playerStats.RestoreMind(Mathf.Abs(task.mindEffect), task.mindChannel);
            else if (task.mindEffect < 0)
                _playerStats.DrainMind(Mathf.Abs(task.mindEffect), task.mindChannel);

            _playerStats.physicalResistance =
                Mathf.Clamp01(_playerStats.physicalResistance + task.strengthDelta);
            _playerStats.observationRadius =
                Mathf.Max(0f, _playerStats.observationRadius + task.observationDelta);
            _playerStats.velocity =
                Mathf.Clamp(_playerStats.velocity + task.velocityDelta, 0.1f, 5f);
        }
        else
        {
            // Lightweight NPC path
            if (task.mindEffect > 0)
                satisfaction  = Mathf.Clamp01(satisfaction  + Mathf.Abs(task.mindEffect));
            else
                mentalFatigue = Mathf.Clamp01(mentalFatigue - task.mindEffect);

            strength    = Mathf.Clamp01(strength    + task.strengthDelta);
            observation = Mathf.Max(0f, observation + task.observationDelta);
            velocity    = Mathf.Clamp(velocity      + task.velocityDelta, 0.1f, 5f);
        }
    }
}
