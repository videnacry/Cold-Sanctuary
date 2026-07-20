using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Type of world-changing event in a mob world (docs mob-world-architecture §6).
/// </summary>
public enum MobWorldEventType
{
    Migration, // parte de los habitantes se mueven a otros puestos y suben de nivel
    Invasion,  // todos se movilizan a defender y suben de nivel (spawn de enemigos vía hook)
    Festival   // todos suben de nivel y vuelven a su puesto
}

/// <summary>
/// Keeps a mob world alive through SCRIPTED EVENTS instead of per-agent simulation (docs §6):
/// migrations, invasions and festivals move residents, level them up and reshuffle the world so
/// "everything is new" — without the cost of the human SanctuaryDirector's autonomous loops.
///
/// Singleton with auto-bootstrap. Events are usually story-triggered via <see cref="TriggerEvent"/>;
/// <see cref="autoEvents"/> can fire random ones on a timer for prototyping. Other systems subscribe
/// to <see cref="OnEvent"/> to reshuffle missions/enemies.
/// </summary>
public class MobWorldDirector : MonoBehaviour
{
    static MobWorldDirector _instance;
    public static bool HasInstance => _instance != null;
    public static MobWorldDirector Instance => _instance != null ? _instance : Bootstrap();

    /// <summary>Fired after an event resolves, so scenes/systems can reshuffle content.</summary>
    public event Action<MobWorldEventType> OnEvent;

    [Header("Auto events (prototipado)")]
    [Tooltip("Si true, dispara eventos aleatorios cada autoInterval segundos.")]
    public bool autoEvents = false;
    [Min(5f)] public float autoInterval = 60f;

    readonly List<MobResident> _residents = new List<MobResident>();

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
    }

    void Start()
    {
        if (autoEvents) StartCoroutine(AutoEventLoop());
    }

    // ── Registry ────────────────────────────────────────────────────────────────

    public void Register(MobResident r)   { if (r != null && !_residents.Contains(r)) _residents.Add(r); }
    public void Unregister(MobResident r) { _residents.Remove(r); }
    public IReadOnlyList<MobResident> Residents => _residents;

    // ── Events ──────────────────────────────────────────────────────────────────

    /// <summary>Trigger a world event. Moves/levels residents and notifies subscribers.</summary>
    public void TriggerEvent(MobWorldEventType type)
    {
        switch (type)
        {
            case MobWorldEventType.Migration:
                // ~half the residents relocate to another resident's post and grow.
                for (int i = 0; i < _residents.Count; i++)
                {
                    var r = _residents[i];
                    if (r == null) continue;
                    if (UnityEngine.Random.value < 0.5f && _residents.Count > 1)
                    {
                        var other = _residents[UnityEngine.Random.Range(0, _residents.Count)];
                        if (other != null && other != r) r.MoveTo(other.HomePost);
                        r.LevelUp();
                    }
                }
                break;

            case MobWorldEventType.Invasion:
                // Everyone rallies (levels up). Enemy spawning is left to OnEvent subscribers.
                foreach (var r in _residents) if (r != null) r.LevelUp();
                break;

            case MobWorldEventType.Festival:
                // Everyone grows and returns to their post.
                foreach (var r in _residents) { if (r == null) continue; r.LevelUp(); r.ReturnHome(); }
                break;
        }

        Debug.Log($"[MobWorld] Evento: {type}. Habitantes afectados: {_residents.Count}.");
        OnEvent?.Invoke(type);
    }

    IEnumerator AutoEventLoop()
    {
        while (autoEvents)
        {
            yield return new WaitForSeconds(autoInterval);
            TriggerEvent((MobWorldEventType)UnityEngine.Random.Range(0, 3));
        }
    }

    // ── Auto-bootstrap ────────────────────────────────────────────────────────────

    static MobWorldDirector Bootstrap()
    {
        var go = new GameObject("MobWorldDirector (auto)");
        _instance = go.AddComponent<MobWorldDirector>();
        return _instance;
    }
}
