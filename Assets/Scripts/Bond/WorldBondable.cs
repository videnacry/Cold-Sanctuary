using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to any world object or place that can form a bond:
/// a yoga mat, the sun, a mountain, a street, a ring.
/// Any entity (player, NPC, companion) can build bond with this object.
///
/// Register with BondActivityManager.RegisterBondable(id, this) on Start.
/// </summary>
public class WorldBondable : MonoBehaviour, IBondable
{
    [Header("Identity")]
    [Tooltip("Unique ID used by BondActivities to find this bondable.")]
    public string bondableId;

    public string displayName;

    [Header("Bond — Player (initial value)")]
    [Tooltip("Starting bond strength with the player. Other bonds start at 0.")]
    [Range(0f, 100f)] public float bondWithPlayer;

    [Header("Proximity Restoration (optional)")]
    [Tooltip("If > 0, restores this mind channel when an entity with bond is nearby.")]
    public MindChannel restorationChannel = MindChannel.Satisfaction;
    public float restorationRatePerSecond;
    public float proximityRadius = 3f;

    IMind _playerMind;
    Transform _playerTransform;
    MonoBehaviour _playerEntity;
    BondActivityManager _manager;

    readonly Dictionary<int, float> _otherBonds = new Dictionary<int, float>();

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerMind      = player.GetComponent<IMind>();
            _playerTransform = player.transform;
            _playerEntity    = player.GetComponent<PlayerStats>();
            _manager         = player.GetComponent<BondActivityManager>();
        }

        if (_manager != null && !string.IsNullOrEmpty(bondableId))
            _manager.RegisterBondable(bondableId, this);
    }

    void OnDestroy()
    {
        if (_manager != null && !string.IsNullOrEmpty(bondableId))
            _manager.UnregisterBondable(bondableId);
    }

    void Update()
    {
        if (_playerMind == null || restorationRatePerSecond <= 0f) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);
        if (dist <= proximityRadius)
        {
            float effect = GetProximityEffect(_playerEntity, restorationChannel);
            if (effect > 0f)
                _playerMind.RestoreMind(effect * Time.deltaTime, restorationChannel);
        }
    }

    // ── IBondable ─────────────────────────────────────────────────────────────

    public float GetBondStrength(MonoBehaviour source)
    {
        if (source == _playerEntity) return bondWithPlayer;
        _otherBonds.TryGetValue(source.GetInstanceID(), out float val);
        return val;
    }

    public void GrowBond(MonoBehaviour source, float amount)
    {
        if (source == _playerEntity)
        {
            bondWithPlayer = Mathf.Clamp(bondWithPlayer + amount, 0f, 100f);
            return;
        }
        int id = source.GetInstanceID();
        _otherBonds.TryGetValue(id, out float cur);
        _otherBonds[id] = Mathf.Clamp(cur + amount, 0f, 100f);
    }

    public float GetProximityEffect(MonoBehaviour source, MindChannel channel)
    {
        if (channel != restorationChannel) return 0f;
        return restorationRatePerSecond * (GetBondStrength(source) / 100f);
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        if (proximityRadius <= 0f) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, proximityRadius);
    }
}
