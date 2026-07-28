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

    // Migración 2026-07-28: los stats ya NO viven aquí. WorldCharacter lee/escribe el `Anima` del objeto
    // (todo ser lo tiene: jugador=PlayerStats:Anima, compañeros=CompanionBase:Anima). Sin duplicación.

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired when all promotion thresholds are met. SanctuaryDirector listens here.</summary>
    public event Action<WorldCharacter> OnReadyForPromotion;

    /// <summary>Fired when a periodic-table element is discovered through a task.</summary>
    public event Action<WorldCharacter, string> OnElementDiscovered;

    // ── Runtime ───────────────────────────────────────────────────────────────

    Anima _anima;                  // hogar de stats del ser (jugador/compañero/…)
    bool  _taskLoopRunning;
    bool  _promotionFired;   // guard: fire once per area

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        _anima = GetComponent<Anima>();
    }

    void Start()
    {
        SanctuaryDirector.Instance?.Register(this);
    }

    // ── Stat accessors ────────────────────────────────────────────────────────

    // Stats leídos/escritos en el Anima del objeto (fuente única).
    public float Strength
    {
        get => _anima != null ? _anima.physicalResistance : 0f;
        set { if (_anima != null) _anima.physicalResistance = Mathf.Clamp01(value); }
    }

    public float Satisfaction
    {
        get => _anima != null ? _anima.satisfaction : 0f;
        set { if (_anima != null) _anima.satisfaction = Mathf.Clamp01(value); }
    }

    public float Observation
    {
        get => _anima != null ? _anima.observationRadius : 0f;
        set { if (_anima != null) _anima.observationRadius = Mathf.Max(0f, value); }
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
        if (_anima == null) return;

        // Efecto de mente: por IMind (jugador → aplica multiplicador/clamps) o directo sobre el Anima.
        if (_anima is IMind mind)
        {
            if (task.mindEffect > 0)      mind.RestoreMind(Mathf.Abs(task.mindEffect), task.mindChannel);
            else if (task.mindEffect < 0) mind.DrainMind(Mathf.Abs(task.mindEffect), task.mindChannel);
        }
        else
        {
            if (task.mindEffect > 0)
                _anima.satisfaction  = Mathf.Clamp01(_anima.satisfaction + Mathf.Abs(task.mindEffect));
            else
                _anima.mentalFatigue = Mathf.Clamp01(_anima.mentalFatigue - task.mindEffect);
        }

        // Físico: sobre el Anima (fuente única).
        _anima.physicalResistance = Mathf.Clamp01(_anima.physicalResistance + task.strengthDelta);
        _anima.observationRadius  = Mathf.Max(0f, _anima.observationRadius + task.observationDelta);
        _anima.velocity           = Mathf.Clamp(_anima.velocity + task.velocityDelta, 0.1f, 5f);
    }
}
