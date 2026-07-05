using UnityEngine;

/// <summary>
/// A collectible fragment dropped by an IngredientMob when processed.
/// When the player picks it up, it attempts to discover the element
/// in PeriodicTableManager.
///
/// Attach to a small pickup prefab with a Collider set to IsTrigger.
/// Visual: a glowing orb or crystalline shard with the element symbol floating above it.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ElementFragment : MonoBehaviour
{
    [Header("Element")]
    [Tooltip("Chemical symbol this fragment carries.")]
    public string elementSymbol = "C";

    [Tooltip("Probability that picking up this fragment discovers the element.")]
    [Range(0f, 1f)]
    public float discoveryChance = 0.5f;

    [Header("Pickup")]
    [Tooltip("Seconds before auto-despawn if not collected.")]
    public float lifetime = 30f;

    [Tooltip("Upward float speed (cosmetic).")]
    public float floatSpeed = 0.3f;

    [Tooltip("Float amplitude.")]
    public float floatAmplitude = 0.1f;

    // ── Runtime ───────────────────────────────────────────────────────────────

    float _spawnTime;
    float _baseY;
    bool  _collected;

    void Start()
    {
        _spawnTime = Time.time;
        _baseY     = transform.position.y;
    }

    void Update()
    {
        if (_collected) return;

        // Float animation
        float y = _baseY + Mathf.Sin((Time.time - _spawnTime) * floatSpeed * Mathf.PI * 2f)
                         * floatAmplitude;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);

        // Lifetime
        if (Time.time - _spawnTime > lifetime)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (_collected) return;
        if (!other.CompareTag("Player")) return;

        _collected = true;
        Collect();
    }

    void Collect()
    {
        if (!string.IsNullOrEmpty(elementSymbol)
            && UnityEngine.Random.value < discoveryChance)
        {
            PeriodicTableManager.Instance?.Discover(elementSymbol);
        }

        // TODO: play pickup sound + VFX
        Destroy(gameObject);
    }
}
