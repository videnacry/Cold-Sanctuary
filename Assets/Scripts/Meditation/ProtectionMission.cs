using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// "A proteger" mission (docs §7/§8) — bespoke escort/defense, not a MeditationMissionBase subclass
/// because it has two mob types, counts only repels, and can FAIL:
///   A fragile ward (ProtectMob) sits at the area anchor. Waves of WardAttackerMob march toward it;
///   the player interposes to repel them. Survive the whole wave with the ward alive → success
///   (reward). If attackers deplete the ward's integrity → failure (no reward). Either way the
///   session ends and the world is restored.
///
/// Auto-wires via MobMission.OnBegan/OnEnded, mirroring MeditationMissionBase's lifecycle.
/// </summary>
[RequireComponent(typeof(MobMission))]
public class ProtectionMission : MonoBehaviour
{
    [Header("Mission")]
    public MobMission mission;

    [Header("Waves")]
    [Tooltip("Total attackers in the wave. Survive them all (ward alive) to win.")]
    [Min(1)] public int attackersTotal = 6;
    [Min(0f)] public float spawnInterval = 1.5f;
    [Tooltip("Attackers spawn on a ring around the ward.")]
    public float spawnRadius = 9f;

    [Header("Ward")]
    [Min(1)] public int wardIntegrity = 3;
    public GameObject wardPrefab;
    public Color wardColor = new Color(0.85f, 0.85f, 0.55f);

    [Header("Attackers")]
    public GameObject attackerPrefab;
    public Color attackerColor = new Color(0.55f, 0.20f, 0.25f);
    public float attackerSpeed   = 2f;
    public float interceptRadius = 1.3f;
    public float hitRadius       = 1.2f;

    [Header("Reward / hooks")]
    public MeditationReward reward = new MeditationReward();
    public UnityEvent onCompleted;
    public UnityEvent onFailed;

    // ── Runtime ───────────────────────────────────────────────────────────────

    Transform  _player;
    ProtectMob _ward;
    int        _repelled;
    int        _spawned;
    bool       _running;
    Coroutine  _routine;
    readonly List<WardAttackerMob> _attackers = new List<WardAttackerMob>();

    void Awake() { if (mission == null) mission = GetComponent<MobMission>(); }

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

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Begin()
    {
        if (_running) return;
        _running  = true;
        _repelled = 0;
        _spawned  = 0;

        var p = GameObject.FindGameObjectWithTag("Player");
        _player = p != null ? p.transform : null;

        SpawnWard();
        _routine = StartCoroutine(SpawnLoop());
        Debug.Log($"[Proteger] Defiende al pupilo de {attackersTotal} amenazas. Interponte para dispersarlas.");
    }

    void SpawnWard()
    {
        GameObject go;
        if (wardPrefab != null)
        {
            go = Instantiate(wardPrefab, transform.position, Quaternion.identity);
            _ward = go.GetComponent<ProtectMob>() ?? go.AddComponent<ProtectMob>();
        }
        else
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Ward (auto)";
            go.transform.position = transform.position;
            go.transform.localScale = Vector3.one * 0.8f;
            var col = go.GetComponent<Collider>(); if (col != null) Destroy(col);
            var rend = go.GetComponent<Renderer>(); if (rend != null) rend.material.color = wardColor;
            _ward = go.AddComponent<ProtectMob>();
        }

        _ward.integrity = wardIntegrity;
        _ward.OnIntegrityDepleted += Fail;
    }

    IEnumerator SpawnLoop()
    {
        while (_running && _spawned < attackersTotal)
        {
            SpawnAttacker();
            _spawned++;
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnAttacker()
    {
        if (_ward == null) return;

        float angle = Random.value * Mathf.PI * 2f;
        Vector3 pos = _ward.transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * spawnRadius;

        WardAttackerMob mob;
        if (attackerPrefab != null)
        {
            GameObject go = Instantiate(attackerPrefab, pos, Quaternion.identity);
            mob = go.GetComponent<WardAttackerMob>() ?? go.AddComponent<WardAttackerMob>();
        }
        else
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "WardAttacker (auto)";
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.6f;
            var col = go.GetComponent<Collider>(); if (col != null) Destroy(col);
            var rend = go.GetComponent<Renderer>(); if (rend != null) rend.material.color = attackerColor;
            mob = go.AddComponent<WardAttackerMob>();
        }

        mob.moveSpeed       = attackerSpeed;
        mob.interceptRadius = interceptRadius;
        mob.hitRadius       = hitRadius;
        mob.SetWard(_ward);
        if (_player != null) mob.SetPlayer(_player);
        mob.OnDissolved += HandleAttackerResolved;
        _attackers.Add(mob);
    }

    void HandleAttackerResolved(MeditationMob m)
    {
        var atk = m as WardAttackerMob;
        _attackers.Remove(atk);
        if (atk != null && !atk.ReachedWard) _repelled++;

        Debug.Log($"[Proteger] Repelidas {_repelled}. Pupilo: {(_ward != null ? _ward.integrity : 0)}.");

        // Survived the whole wave with the ward alive → success.
        if (_running && _spawned >= attackersTotal && _attackers.Count == 0)
            Complete();
    }

    void Complete()
    {
        if (!_running) return;
        _running = false;
        if (_routine != null) StopCoroutine(_routine);

        Debug.Log("[Proteger] ✅ El pupilo está a salvo.");
        reward.Apply(_player);
        onCompleted?.Invoke();
        MeditationSession.Instance.EndMission();
    }

    void Fail()
    {
        if (!_running) return;
        _running = false;
        if (_routine != null) StopCoroutine(_routine);

        Debug.Log("[Proteger] ✖ El pupilo fue superado. Vuelve a intentarlo más fuerte.");
        onFailed?.Invoke();
        MeditationSession.Instance.EndMission(); // sin recompensa
    }

    void Cleanup()
    {
        _running = false;
        if (_routine != null) { StopCoroutine(_routine); _routine = null; }

        foreach (var a in _attackers)
            if (a != null) Destroy(a.gameObject);
        _attackers.Clear();

        if (_ward != null)
        {
            _ward.OnIntegrityDepleted -= Fail;
            Destroy(_ward.gameObject);
            _ward = null;
        }
    }
}
