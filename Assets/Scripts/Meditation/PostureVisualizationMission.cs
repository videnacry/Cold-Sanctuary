using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// First concrete mob-mission (docs §7 Visualización): the player visualizes a yoga posture they
/// barely master while distracting thoughts (EphemeralThoughtMob) appear and chase them.
///
/// Loop: while visualizing, N thoughts spawn one by one; the player must FLEE each so it fails to
/// latch and dissolves. When <see cref="thoughtsToDissolve"/> thoughts have dissolved, the mind is
/// quiet → the posture is visualized → the mission ends (MeditationSession.EndMission) and the
/// reward hook fires (wire asana-mastery / Observation gains via onCompleted).
///
/// Sits on the same GameObject as its MobMission. Wires itself via MobMission's C# events, so no
/// Inspector event wiring is required — just add both components and list the MobMission on the
/// area's VirtualizationMachine.
/// </summary>
[RequireComponent(typeof(MobMission))]
public class PostureVisualizationMission : MonoBehaviour
{
    [Header("Mission")]
    [Tooltip("The MobMission descriptor. Auto-found on this GameObject if left empty.")]
    public MobMission mission;

    [Header("Thoughts")]
    [Tooltip("Prefab with an EphemeralThoughtMob (added if missing). If null, a placeholder " +
             "sphere is spawned so the mission is testable without art.")]
    public GameObject thoughtPrefab;

    [Min(1)] public int thoughtsToDissolve = 5;

    [Tooltip("Seconds between spawns.")]
    [Min(0f)] public float spawnInterval = 2f;

    [Tooltip("Thoughts spawn in a ring around the player, between these radii.")]
    public float minSpawnDistance = 4f;
    public float spawnRadius     = 8f;

    [Header("Thought tuning (applied to each spawned thought)")]
    public float thoughtSpeed  = 3f;
    public float dissolveDelay = 3f;
    public float touchRadius   = 1.2f;

    [Header("Reward")]
    [Tooltip("Applied when the posture is fully visualized: asana mastery + Observation (+ coins).")]
    public MeditationReward reward = new MeditationReward();

    [Tooltip("Extra hook fired after the reward is applied (VFX, dialogue, unlocks…).")]
    public UnityEvent onCompleted;

    // ── Runtime ───────────────────────────────────────────────────────────────

    Transform _player;
    int       _dissolved;
    int       _spawned;
    bool      _running;
    Coroutine _spawnRoutine;
    readonly List<MeditationMob> _mobs = new List<MeditationMob>();

    void Awake()
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

    // ── Mission lifecycle ───────────────────────────────────────────────────────

    void Begin()
    {
        if (_running) return;
        _running   = true;
        _dissolved = 0;
        _spawned   = 0;

        var p   = GameObject.FindGameObjectWithTag("Player");
        _player = p != null ? p.transform : null;

        _spawnRoutine = StartCoroutine(SpawnLoop());
        Debug.Log($"[Postura] Visualización iniciada. Huye de {thoughtsToDissolve} pensamientos " +
                  "para que se desvanezcan.");
    }

    IEnumerator SpawnLoop()
    {
        while (_running && _spawned < thoughtsToDissolve)
        {
            SpawnThought();
            _spawned++;
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnThought()
    {
        Vector3 pos = SpawnPosition();

        EphemeralThoughtMob mob;
        if (thoughtPrefab != null)
        {
            GameObject go = Instantiate(thoughtPrefab, pos, Quaternion.identity);
            mob = go.GetComponent<EphemeralThoughtMob>() ?? go.AddComponent<EphemeralThoughtMob>();
        }
        else
        {
            // Placeholder so the mission is playable before art exists.
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "ThoughtMob (auto)";
            go.transform.position   = pos;
            go.transform.localScale = Vector3.one * 0.6f;
            var col = go.GetComponent<Collider>(); if (col != null) Destroy(col);
            var rend = go.GetComponent<Renderer>();
            if (rend != null) rend.material.color = new Color(0.22f, 0.12f, 0.30f);
            mob = go.AddComponent<EphemeralThoughtMob>();
        }

        mob.moveSpeed     = thoughtSpeed;
        mob.dissolveDelay = dissolveDelay;
        mob.touchRadius   = touchRadius;
        if (_player != null) mob.SetPlayer(_player);

        mob.OnDissolved += HandleDissolved;
        _mobs.Add(mob);
    }

    Vector3 SpawnPosition()
    {
        Vector3 center = _player != null ? _player.position : transform.position;
        float angle = Random.value * Mathf.PI * 2f;
        float d     = Mathf.Lerp(minSpawnDistance, spawnRadius, Random.value);
        return center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * d;
    }

    void HandleDissolved(MeditationMob m)
    {
        _mobs.Remove(m);
        _dissolved++;
        Debug.Log($"[Postura] Pensamiento disuelto {_dissolved}/{thoughtsToDissolve}.");
        if (_dissolved >= thoughtsToDissolve) Complete();
    }

    void Complete()
    {
        if (!_running) return;
        _running = false;
        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);

        Debug.Log("[Postura] ✅ Postura visualizada. La mente se aquietó.");

        // Real reward: raise mastery of the visualized asana + grow Observation.
        reward.Apply(_player);
        onCompleted?.Invoke();

        // Fade to black, snap back to normal size, fade in. This also fires MobMission.OnEnded
        // → Cleanup() below, which removes any thoughts still in the scene.
        MeditationSession.Instance.EndMission();
    }

    void Cleanup()
    {
        _running = false;
        if (_spawnRoutine != null) { StopCoroutine(_spawnRoutine); _spawnRoutine = null; }

        foreach (var m in _mobs)
            if (m != null) Destroy(m.gameObject);
        _mobs.Clear();
    }
}
