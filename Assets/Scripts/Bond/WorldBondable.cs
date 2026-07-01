using UnityEngine;

/// <summary>
/// Attach to any world object or place that can form a bond with the player:
/// a yoga mat, the sun, a mountain, a street, a ring.
///
/// Register with BondActivityManager.RegisterBondable(id, this) on Start.
/// </summary>
public class WorldBondable : MonoBehaviour, IBondable
{
    [Header("Identity")]
    [Tooltip("Unique ID used by BondActivities to find this bondable.")]
    public string bondableId;

    public string displayName;

    [Header("Bond")]
    [Range(0f, 100f)] public float bondWithPlayer;

    [Header("Proximity Restoration (optional)")]
    [Tooltip("If > 0, restores this stat when the player is nearby (like a companion).")]
    public StatChannel restorationChannel = StatChannel.Satisfaction;
    public float restorationRatePerSecond;
    public float proximityRadius = 3f;

    PlayerStats _playerStats;
    BondActivityManager _manager;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerStats = player.GetComponent<PlayerStats>();
            _manager     = player.GetComponent<BondActivityManager>();
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
        if (_playerStats == null || restorationRatePerSecond <= 0f) return;

        float dist = Vector3.Distance(transform.position, _playerStats.transform.position);
        if (dist <= proximityRadius)
        {
            // Scale restoration by bond — low bond = minimal effect
            float bondFactor = bondWithPlayer / 100f;
            _playerStats.Restore(restorationRatePerSecond * bondFactor * Time.deltaTime, restorationChannel);
        }
    }

    // ── IBondable ─────────────────────────────────────────────────────────────

    public float BondWithPlayer => bondWithPlayer;

    public void GrowBondWithPlayer(float amount)
        => bondWithPlayer = Mathf.Clamp(bondWithPlayer + amount, 0f, 100f);

    public float GetProximityEffect(StatChannel channel)
    {
        if (channel != restorationChannel) return 0f;
        return restorationRatePerSecond * (bondWithPlayer / 100f);
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        if (proximityRadius <= 0f) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, proximityRadius);
    }
}
