using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the active asana and the queue of upcoming ones.
/// Handles side switching for bilateral poses and benefit delivery.
/// </summary>
public class AsanaQueue : MonoBehaviour
{
    public Asana activeAsana        { get; private set; }
    public bool  onSecondSide       { get; private set; }
    public float benefitAccumulated { get; private set; }

    private Queue<Asana> _queue = new Queue<Asana>();

    // Stat containers (key = StatType)
    private Dictionary<StatType, float> _containers = new Dictionary<StatType, float>();

    // Benefit delivery rate per second (tune in inspector via a wrapper MonoBehaviour)
    public float benefitRate = 1f;

    private PlayerStats _playerStats;

    void Start()
    {
        _playerStats = GetComponent<PlayerStats>();
        if (_playerStats == null)
            Debug.LogWarning("[AsanaQueue] No PlayerStats found on this GameObject.");
    }

    void Update()
    {
        if (activeAsana == null) return;
        DeliverBenefit(Time.deltaTime);
    }

    /// <summary>Enqueue an asana to practice next.</summary>
    public void Enqueue(Asana asana) => _queue.Enqueue(asana);

    /// <summary>
    /// Call when the player completes the position match for the active asana
    /// (or after error corrections). Starts benefit delivery.
    /// </summary>
    public void StartActive(Asana asana)
    {
        activeAsana     = asana;
        onSecondSide    = false;
        benefitAccumulated = 0f;
    }

    private void DeliverBenefit(float delta)
    {
        float cap = activeAsana.containerCurrent;
        float current = GetContainer(activeAsana.statType);

        if (current >= cap)
        {
            OnLimitReached();
            return;
        }

        float gain = benefitRate * delta;
        SetContainer(activeAsana.statType, Mathf.Min(current + gain, cap));
        benefitAccumulated += gain;

        if (_playerStats != null)
            _playerStats.RestoreMind(gain, activeAsana.channel);
    }

    private void OnLimitReached()
    {
        if (activeAsana.hasTwoSides && !onSecondSide)
        {
            onSecondSide = true;
            // Reset accumulation for the second side — container keeps filling
            benefitAccumulated = 0f;
            return;
        }

        // Pose complete
        activeAsana.RegisterPractice();
        AdvanceQueue();
    }

    private void AdvanceQueue()
    {
        activeAsana = _queue.Count > 0 ? _queue.Dequeue() : null;
        onSecondSide = false;
        benefitAccumulated = 0f;
    }

    /// <summary>Immediately ends the active asana without completing it (e.g. on fall).</summary>
    public void ForceEnd()
    {
        activeAsana        = null;
        onSecondSide       = false;
        benefitAccumulated = 0f;
        _queue.Clear();
    }

    // --- Container helpers ---
    public float GetContainer(StatType stat)
    {
        _containers.TryGetValue(stat, out float val);
        return val;
    }
    private void SetContainer(StatType stat, float val) => _containers[stat] = val;
}
