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
public abstract class CompanionBase : MonoBehaviour, IBondable, IMindSimple
{
    // ── Inspector ────────────────────────────────────────────────────────────
    [Header("Identity")]
    public string companionName;

    [Header("Internal State (0–1)")]
    [field: SerializeField, Range(0f, 1f)] public float fatigue { get; set; }
    [field: SerializeField, Range(0f, 1f)] public float stress  { get; set; }
    [field: SerializeField, Range(0f, 1f)] public float mood    { get; set; } = 0.7f;

    // ── Aptitudes corporales/mentales (1.0 = media real humana) ────────────────
    // Se fijan en Start() desde las propiedades Base*; escalan con tareas/origen.
    // Ver docs/creature-stats.md.
    [HideInInspector] public float agility;
    [HideInInspector] public float perception;
    [HideInInspector] public float strength;
    [HideInInspector] public float bodyMass;

    protected virtual float BaseAgility    => 1f;
    protected virtual float BasePerception => 1f;
    protected virtual float BaseStrength   => 1f;
    protected virtual float BaseBodyMass   => 1f;

    [Header("Bond — Player (initial value)")]
    [Tooltip("Starting bond strength with the player. Other bonds start at 0 and grow through interaction.")]
    [Range(0f, 100f)] public float bondWithPlayer;

    [Header("Proximity")]
    [Tooltip("Radius in world units within which the companion affects the player.")]
    public float proximityRadius = 4f;

    [Tooltip("Base restoration per second at full bond and good mood.")]
    public float baseRestorationRate = 0.02f;

    [Tooltip("Which mind channel this companion primarily restores.")]
    public MindChannel primaryChannel = MindChannel.MentalFatigue;

    [Header("Thought Anchors")]
    public List<ThoughtAnchor> anchors = new List<ThoughtAnchor>();

    // ── IBondable ────────────────────────────────────────────────────────────

    // Non-player bonds (NPC↔NPC, NPC↔object). Player bond lives in bondWithPlayer.
    readonly System.Collections.Generic.Dictionary<EntityId, float> _otherBonds =
        new System.Collections.Generic.Dictionary<EntityId, float>();

    public float GetBondStrength(MonoBehaviour source)
    {
        if (source == _playerEntity) return bondWithPlayer;
        _otherBonds.TryGetValue(source.GetEntityId(), out float val);
        return val;
    }

    public void GrowBond(MonoBehaviour source, float amount)
    {
        if (source == _playerEntity)
        {
            bondWithPlayer = Mathf.Clamp(bondWithPlayer + amount, 0f, 100f);
            return;
        }
        EntityId id = source.GetEntityId();
        _otherBonds.TryGetValue(id, out float cur);
        _otherBonds[id] = Mathf.Clamp(cur + amount, 0f, 100f);
    }

    /// <summary>
    /// Net effect per second on source's mind channel when nearby.
    /// Driven by: bond strength + companion mood + fatigue/stress + personality modifier.
    /// Positive = restores source. Negative = drains source.
    /// </summary>
    public float GetProximityEffect(MonoBehaviour source, MindChannel channel)
    {
        if (channel != primaryChannel) return 0f;

        float bondFactor  = GetBondStrength(source) / 100f;
        float stateFactor = mood - (fatigue * 0.5f) - (stress * 0.3f);
        stateFactor = Mathf.Clamp(stateFactor, -0.5f, 1f);

        return baseRestorationRate * bondFactor * stateFactor * GetMoodModifier();
    }

    // ── Runtime ──────────────────────────────────────────────────────────────
    protected IMind _playerMind;
    protected Transform _playerTransform;
    protected MonoBehaviour _playerEntity; // identity token — used as bond key, no direct stat access
    bool _playerInRange;

    protected virtual void Start()
    {
        agility    = BaseAgility;
        perception = BasePerception;
        strength   = BaseStrength;
        bodyMass   = BaseBodyMass;
        SetupAnchors();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerMind      = player.GetComponent<IMind>();
            _playerTransform = player.transform;
            _playerEntity    = player.GetComponent<PlayerStats>();
        }
    }

    protected virtual void Update()
    {
        CheckPlayerProximity();
        SimulateInternalState();
    }

    void CheckPlayerProximity()
    {
        if (_playerMind == null) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);
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
        if (_playerEntity == null) return;
        float effect = GetProximityEffect(_playerEntity, primaryChannel);
        if (Mathf.Approximately(effect, 0f)) return;

        if (effect > 0f)
            _playerMind.RestoreMind( effect * Time.deltaTime, primaryChannel);
        else
            _playerMind.DrainMind  (-effect * Time.deltaTime, primaryChannel);
    }

    void SimulateInternalState()
    {
        fatigue = Mathf.Clamp01(fatigue + FatigueRatePerSecond() * Time.deltaTime);
        mood    = Mathf.MoveTowards(mood, GetRestingMood(), 0.001f * Time.deltaTime);
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
