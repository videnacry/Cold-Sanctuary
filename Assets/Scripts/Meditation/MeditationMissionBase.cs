using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// How a mission finishes.
/// </summary>
public enum MissionEndMode
{
    /// In-place Microcosmos: MeditationSession fades to black and RealityShiftController snaps size back.
    Session,
    /// Mob-world scene (MobWorldLoader): there is no reality shift to undo — cleanup runs immediately
    /// and the player leaves the scene via the YogaPortal.
    Standalone
}

/// <summary>
/// Shared logic for every Microcosmos mob-mission: spawn N mobs one by one, count them as they
/// resolve (dissolve), apply the reward and end the session when the target is reached, and clean
/// up leftovers. Subclasses only decide WHICH archetype to spawn (CreateMob).
///
/// This is the enabler for "one mission per area" (docs §7 matrix): each area gets a MobMission +
/// one of these subclasses configured for its biome. Auto-wires via MobMission.OnBegan/OnEnded.
/// </summary>
[RequireComponent(typeof(MobMission))]
public abstract class MeditationMissionBase : MonoBehaviour
{
    [Header("Mission")]
    [Tooltip("The MobMission descriptor. Auto-found on this GameObject if left empty.")]
    public MobMission mission;

    [Header("Spawning")]
    [Tooltip("How many mobs must be resolved to complete the mission.")]
    [Min(1)] public int targetCount = 5;

    [Tooltip("Seconds between spawns.")]
    [Min(0f)] public float spawnInterval = 2f;

    [Tooltip("Mobs spawn in a ring around the player, between these radii.")]
    public float minSpawnDistance = 4f;
    public float spawnRadius     = 8f;

    [Header("Reward")]
    [Tooltip("Applied on completion: asana mastery + Observation (+ coins).")]
    public MeditationReward reward = new MeditationReward();

    [Tooltip("Extra hook fired after the reward is applied (VFX, dialogue, unlocks…).")]
    public UnityEvent onCompleted;

    [Header("End mode")]
    [Tooltip("Session = in-place Microcosmos (fade + RealityShiftController snaps size back). " +
             "Standalone = mob-world scene (no shift; cleanup runs now, player exits via YogaPortal).")]
    public MissionEndMode endMode = MissionEndMode.Session;

    // ── Runtime ───────────────────────────────────────────────────────────────

    protected Transform player;
    int       _resolved;
    int       _spawned;
    bool      _running;
    Coroutine _spawnRoutine;
    readonly List<MeditationMob> _mobs = new List<MeditationMob>();

    protected virtual void Awake()
    {
        if (mission == null) mission = GetComponent<MobMission>();
    }

    void OnEnable()
    {
        if (mission == null) return;
        mission.OnBegan += Begin;
        mission.OnEnded += Cleanup;
    }

    void OnDisable()
    {
        if (mission == null) return;
        mission.OnBegan -= Begin;
        mission.OnEnded -= Cleanup;
    }

    // ── Subclass contract ───────────────────────────────────────────────────────

    /// <summary>Create and return this mission's archetype mob at the given position.</summary>
    protected abstract MeditationMob CreateMob(Vector3 position);

    /// <summary>Console message logged when the mission begins. Override for flavor.</summary>
    protected virtual string BeginMessage => $"[Meditación] Misión iniciada: resuelve {targetCount}.";

    /// <summary>Console message logged on completion. Override for flavor.</summary>
    protected virtual string CompleteMessage => "[Meditación] ✅ Misión completada.";

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Begin()
    {
        if (_running) return;
        _running   = true;
        _resolved  = 0;
        _spawned   = 0;

        var p  = GameObject.FindGameObjectWithTag("Player");
        player = p != null ? p.transform : null;

        _spawnRoutine = StartCoroutine(SpawnLoop());
        Debug.Log(BeginMessage);
    }

    IEnumerator SpawnLoop()
    {
        while (_running && _spawned < targetCount)
        {
            SpawnOne();
            _spawned++;
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnOne()
    {
        MeditationMob mob = CreateMob(SpawnPosition());
        if (mob == null) return;
        // Parent under the mission so the spawned mob belongs to the mission's scene. In a mob-world
        // scene (Standalone) this guarantees the mob unloads with that scene instead of leaking into
        // the base world; worldPositionStays keeps its spawn position intact.
        mob.transform.SetParent(transform, worldPositionStays: true);
        if (player != null) mob.SetPlayer(player);
        mob.OnDissolved += HandleResolved;
        _mobs.Add(mob);
    }

    protected Vector3 SpawnPosition()
    {
        Vector3 center = player != null ? player.position : transform.position;
        float angle = Random.value * Mathf.PI * 2f;
        float d     = Mathf.Lerp(minSpawnDistance, spawnRadius, Random.value);
        return center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * d;
    }

    void HandleResolved(MeditationMob m)
    {
        _mobs.Remove(m);
        _resolved++;
        Debug.Log($"[Meditación] {_resolved}/{targetCount} resuelto.");
        if (_resolved >= targetCount) Complete();
    }

    void Complete()
    {
        if (!_running) return;
        _running = false;
        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);

        Debug.Log(CompleteMessage);
        reward.Apply(player);
        onCompleted?.Invoke();

        if (endMode == MissionEndMode.Session)
        {
            // In-place: fade to black, snap back to normal size, fade in. Fires MobMission.OnEnded → Cleanup().
            MeditationSession.Instance.EndMission();
        }
        else
        {
            // Standalone (mob-world scene): no reality shift to undo. Clean up leftovers now; the player
            // leaves the scene via the YogaPortal (MobWorldLoader.ExitMobWorld).
            Debug.Log("[Meditación] Misión de mundo mob completada — sal por la sala de yoga cuando quieras.");
            mission.RaiseEnd();
        }
    }

    void Cleanup()
    {
        _running = false;
        if (_spawnRoutine != null) { StopCoroutine(_spawnRoutine); _spawnRoutine = null; }

        foreach (var m in _mobs)
            if (m != null) Destroy(m.gameObject);
        _mobs.Clear();
    }

    // ── Helper for subclasses without a prefab ──────────────────────────────────

    /// <summary>Spawn a collider-less placeholder sphere so a mission is playable before art exists.</summary>
    protected GameObject CreatePlaceholderSphere(Vector3 pos, Color color, float scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.position   = pos;
        go.transform.localScale = Vector3.one * scale;
        var col = go.GetComponent<Collider>(); if (col != null) Destroy(col);
        var rend = go.GetComponent<Renderer>(); if (rend != null) rend.material.color = color;
        return go;
    }
}
