using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all companions (Panterilia, Goluis, Gohageneis, etc.).
/// Simulates internal state (fatigue, stress, mood) and applies proximity
/// restoration or drain to the player based on Bond + current state.
///
/// Subclass this for each companion and override:
///   - SetupAnchors()      → define their ThoughtAnchors
///   - GetMoodModifier()   → how their personality biases the restoration
///   - OnPlayerNearby()    → optional dialogue/reaction hooks
/// </summary>
public abstract class CompanionBase : MonoBehaviour, IBondable
{
    // ── Inspector ────────────────────────────────────────────────────────────
    [Header("Identity")]
    public string companionName;

    [Header("Internal State (0–1)")]
    [Range(0f, 1f)] public float fatigue;
    [Range(0f, 1f)] public float stress;
    [Range(0f, 1f)] public float mood = 0.7f;   // 0 = very low, 1 = very good

    [Header("Bond with Player")]
    [Range(0f, 100f)] public float bondWithPlayer;

    [Header("Proximity")]
    [Tooltip("Radius in world units within which the companion affects the player.")]
    public float proximityRadius = 4f;

    [Tooltip("Base restoration per second at full bond and good mood.")]
    public float baseRestorationRate = 0.02f;

    [Tooltip("Which stat channel this companion primarily restores.")]
    public StatChannel primaryChannel = StatChannel.MentalFatigue;

    [Header("Thought Anchors")]
    public List<ThoughtAnchor> anchors = new List<ThoughtAnchor>();

    // ── IBondable ────────────────────────────────────────────────────────────
    public float BondWithPlayer => bondWithPlayer;

    public void GrowBondWithPlayer(float amount)
    {
        bondWithPlayer = Mathf.Clamp(bondWithPlayer + amount, 0f, 100f);
    }

    /// <summary>
    /// Net effect per second on the given stat channel.
    /// Driven by: bond level + companion mood + fatigue/stress + personality modifier.
    /// Positive = restores player. Negative = drains player.
    /// </summary>
    public float GetProximityEffect(StatChannel channel)
    {
        if (channel != primaryChannel) return 0f;

        // Bond factor: 0 bond = no effect, 100 bond = full effect
        float bondFactor = bondWithPlayer / 100f;

        // State factor: tired/stressed companions give less (or drain at extremes)
        float stateFactor = mood - (fatigue * 0.5f) - (stress * 0.3f);
        stateFactor = Mathf.Clamp(stateFactor, -0.5f, 1f);

        // Personality modifier (overridden per companion)
        float moodMod = GetMoodModifier();

        float effect = baseRestorationRate * bondFactor * stateFactor * moodMod;
        return effect;
    }

    // ── Runtime ──────────────────────────────────────────────────────────────
    protected PlayerStats _playerStats;
    bool _playerInRange;

    protected virtual void Start()
    {
        SetupAnchors();
        // Find player stats — assumes a single player in scene tagged "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _playerStats = player.GetComponent<PlayerStats>();
    }

    protected virtual void Update()
    {
        CheckPlayerProximity();
        SimulateInternalState();
    }

    void CheckPlayerProximity()
    {
        if (_playerStats == null) return;

        float dist = Vector3.Distance(transform.position, _playerStats.transform.position);
        bool inRange = dist <= proximityRadius;

        if (inRange && !_playerInRange)
        {
            _playerInRange = true;
            OnPlayerNearby();
        }
        else if (!inRange && _playerInRange)
        {
            _playerInRange = false;
            OnPlayerLeft();
        }

        if (_playerInRange)
            ApplyProximityEffect();
    }

    void ApplyProximityEffect()
    {
        float effect = GetProximityEffect(primaryChannel);
        if (Mathf.Approximately(effect, 0f)) return;

        if (effect > 0f)
            _playerStats.Restore(effect * Time.deltaTime, primaryChannel);
        else
            _playerStats.Drain(-effect * Time.deltaTime, primaryChannel);
    }

    /// <summary>Simulate slow changes to fatigue/stress/mood over time.</summary>
    void SimulateInternalState()
    {
        // Fatigue builds slowly — subclasses can override the rate
        fatigue = Mathf.Clamp01(fatigue + FatigueRatePerSecond() * Time.deltaTime);

        // Mood drifts toward a resting value based on anchors
        float restingMood = GetRestingMood();
        mood = Mathf.MoveTowards(mood, restingMood, 0.001f * Time.deltaTime);
    }

    // ── Abstract / virtual hooks for subclasses ──────────────────────────────

    /// <summary>Define this companion's ThoughtAnchors here.</summary>
    protected abstract void SetupAnchors();

    /// <summary>Personality multiplier on restoration. Override per companion.</summary>
    protected virtual float GetMoodModifier() => 1f;

    /// <summary>How fast this companion tires per second. Override per companion.</summary>
    protected virtual float FatigueRatePerSecond() => 0.0001f;

    /// <summary>Resting mood value this companion drifts toward over time.</summary>
    protected virtual float GetRestingMood() => 0.7f;

    /// <summary>Called once when the player enters proximity range.</summary>
    protected virtual void OnPlayerNearby() { }

    /// <summary>Called once when the player leaves proximity range.</summary>
    protected virtual void OnPlayerLeft() { }

    // ── Anchor helpers ───────────────────────────────────────────────────────

    /// <summary>Get the weight of a named anchor. Returns 0 if not found.</summary>
    public float GetAnchor(string key)
    {
        foreach (ThoughtAnchor a in anchors)
            if (a.key == key) return a.weight;
        return 0f;
    }

    /// <summary>Shift a named anchor toward a target weight (arc progression).</summary>
    public void ShiftAnchor(string key, float targetWeight, float deltaTime)
    {
        foreach (ThoughtAnchor a in anchors)
            if (a.key == key) { a.ShiftToward(targetWeight, deltaTime); return; }
    }

    // ── Gizmos ───────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, proximityRadius);
    }
}
