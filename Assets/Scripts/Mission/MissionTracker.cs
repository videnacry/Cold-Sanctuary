using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton that manages active missions, tracks progress, and distributes rewards.
///
/// How missions flow:
///   1. StartMission(mission) — activates the mission, subscribes to relevant events.
///   2. Progress updates arrive via:
///        - PeriodicTableManager.OnElementCollected (IngredientCollection)
///        - IngredientMob.OnProcessed              (YeastControl, AreaClear)
///   3. On completion → award coins + item → fire OnMissionCompleted.
///   4. Caller (UI/MagnateDialogue) then tells Inventory to add the award.
///
/// Multiple missions can be active simultaneously (one per area is a good design constraint).
/// </summary>
public class MissionTracker : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────

    public static MissionTracker Instance { get; private set; }

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired when a mission's progress changes. (mission, current, target)</summary>
    public event Action<SanctuaryMission, int, int> OnMissionProgress;

    /// <summary>Fired when a mission is completed successfully.</summary>
    public event Action<SanctuaryMission>            OnMissionCompleted;

    /// <summary>Fired when a mission fails (YeastControl breach, etc.).</summary>
    public event Action<SanctuaryMission>            OnMissionFailed;

    // ── Runtime ───────────────────────────────────────────────────────────────

    class ActiveMission
    {
        public SanctuaryMission mission;
        public int              progress;
        public bool             completed;
        public Coroutine        routine;  // for YeastControl timer
    }

    readonly List<ActiveMission> _active = new List<ActiveMission>();

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        if (PeriodicTableManager.Instance != null)
            PeriodicTableManager.Instance.OnElementCollected += HandleElementCollected;
    }

    void OnDisable()
    {
        if (PeriodicTableManager.Instance != null)
            PeriodicTableManager.Instance.OnElementCollected -= HandleElementCollected;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Activate a mission. Does nothing if it's already active or completed.</summary>
    public void StartMission(SanctuaryMission mission)
    {
        if (mission == null) return;
        if (_active.Exists(a => a.mission == mission)) return;

        var am = new ActiveMission { mission = mission };
        _active.Add(am);

        if (mission.missionType == MissionType.YeastControl)
            am.routine = StartCoroutine(YeastControlRoutine(am));

        Debug.Log($"[MissionTracker] Misión iniciada: {mission.missionName}");
    }

    /// <summary>
    /// Call this when an IngredientMob is processed in a given area.
    /// Used for AreaClear and YeastControl tracking.
    /// </summary>
    public void ReportMobProcessed(IngredientMob mob, SanctuaryAreaType area)
    {
        foreach (var am in _active)
        {
            if (am.completed || am.mission.area != area) continue;

            if (am.mission.missionType == MissionType.AreaClear)
            {
                am.progress++;
                FireProgress(am);
                // AreaClear completes when no mobs remain — checked externally
                // via CheckAreaClear(area) or driven by KitchenCombatManager
            }
        }
    }

    /// <summary>
    /// Call from KitchenCombatManager when all mobs in an area are processed.
    /// Completes any active AreaClear mission for that area.
    /// </summary>
    public void ReportAreaCleared(SanctuaryAreaType area)
    {
        foreach (var am in _active)
        {
            if (am.completed || am.mission.area != area) continue;
            if (am.mission.missionType == MissionType.AreaClear)
                CompleteMission(am);
        }
    }

    /// <summary>
    /// Returns current progress for a mission (0 if not active).
    /// </summary>
    public int GetProgress(SanctuaryMission mission)
    {
        var am = _active.Find(a => a.mission == mission);
        return am?.progress ?? 0;
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    void HandleElementCollected(string symbol, bool isNew)
    {
        foreach (var am in _active)
        {
            if (am.completed) continue;
            if (am.mission.missionType != MissionType.IngredientCollection) continue;
            if (am.mission.targetElement != symbol) continue;

            am.progress++;
            FireProgress(am);

            if (am.progress >= am.mission.targetCount)
                CompleteMission(am);
        }
    }

    // ── YeastControl coroutine ────────────────────────────────────────────────

    IEnumerator YeastControlRoutine(ActiveMission am)
    {
        float elapsed = 0f;
        var mission   = am.mission;

        while (elapsed < mission.controlDuration && !am.completed)
        {
            // Count active yeast mobs in the scene
            int yeastCount = CountYeastMobs();

            if (yeastCount >= mission.targetCount)
            {
                // Exceeded threshold — mission fails
                am.completed = true;
                OnMissionFailed?.Invoke(mission);
                Debug.Log($"[MissionTracker] Misión fallida: {mission.missionName} " +
                          $"(levadura: {yeastCount}/{mission.targetCount})");
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!am.completed)
            CompleteMission(am);
    }

    int CountYeastMobs()
    {
        // "Yeast" mobs are IngredientMobs that can reproduce (canReproduce flag)
        var mobs = FindObjectsByType<IngredientMob>(FindObjectsSortMode.None);
        int count = 0;
        foreach (var m in mobs)
            if (m.canReproduce) count++;
        return count;
    }

    // ── Completion ────────────────────────────────────────────────────────────

    void CompleteMission(ActiveMission am)
    {
        if (am.completed) return;
        am.completed = true;

        var mission = am.mission;

        // Award coins
        CoinWallet.Instance?.Earn(mission.coinReward);

        // Award item
        if (mission.itemReward != null)
            Inventory.Instance?.AddItem(mission.itemReward, 1);

        OnMissionCompleted?.Invoke(mission);

        Debug.Log($"[MissionTracker] ✅ Misión completada: {mission.missionName}. " +
                  $"Recompensa: {mission.coinReward} monedas" +
                  (mission.itemReward != null ? $" + {mission.itemReward.itemName}" : "") + ".");
    }

    void FireProgress(ActiveMission am)
    {
        OnMissionProgress?.Invoke(am.mission, am.progress, am.mission.targetCount);
    }
}
