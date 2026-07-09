using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tab-targeting system: cycles through IngredientMobs from nearest to farthest.
///
/// Usage:
///   - Tab (keyboard)     → cycle to next mob
///   - Click on a mob     → select it directly (mob's collider calls Select(mob))
///   - Escape             → deselect
///
/// CurrentTarget is read by CombatAbilityBar (keyboard shortcuts) and
/// by the Palette system (mouse-click ability use).
///
/// Visual feedback: attach a highlight effect prefab — it will be reparented to
/// the selected mob each time the target changes.
/// </summary>
public class CombatTargetSelector : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────

    public static CombatTargetSelector Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Targeting")]
    [Tooltip("Max range to consider a mob as targetable.")]
    public float maxTargetRange = 20f;

    [Tooltip("Key to cycle to the next target.")]
    public KeyCode cycleKey = KeyCode.Tab;

    [Tooltip("Key to deselect.")]
    public KeyCode deselectKey = KeyCode.Escape;

    [Header("Highlight")]
    [Tooltip("Prefab that visually marks the selected target (e.g. glow ring). " +
             "Reparented to the active target each cycle.")]
    public GameObject highlightPrefab;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired whenever the selected target changes (null = deselected).</summary>
    public event Action<IngredientMob> OnTargetChanged;

    // ── State ─────────────────────────────────────────────────────────────────

    public IngredientMob CurrentTarget { get; private set; }

    int         _currentIndex;
    GameObject  _highlightInstance;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (highlightPrefab != null)
            _highlightInstance = Instantiate(highlightPrefab);
    }

    void Update()
    {
        if (Input.GetKeyDown(cycleKey))   CycleTarget();
        if (Input.GetKeyDown(deselectKey)) Deselect();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Select a specific mob (e.g. from a mouse click on its collider).</summary>
    public void Select(IngredientMob mob)
    {
        if (mob == null) { Deselect(); return; }

        // Find its index in the sorted list so Tab continues from here
        var mobs = GetSortedMobs();
        _currentIndex = mobs.IndexOf(mob);
        if (_currentIndex < 0) _currentIndex = 0;

        ApplyTarget(mob);
    }

    /// <summary>Deselect the current target.</summary>
    public void Deselect()
    {
        CurrentTarget = null;
        _currentIndex = 0;
        PositionHighlight(null);
        OnTargetChanged?.Invoke(null);
    }

    // ── Cycle ─────────────────────────────────────────────────────────────────

    void CycleTarget()
    {
        var mobs = GetSortedMobs();
        if (mobs.Count == 0) { Deselect(); return; }

        // Advance index, wrap around
        _currentIndex = (_currentIndex + 1) % mobs.Count;
        ApplyTarget(mobs[_currentIndex]);
    }

    void ApplyTarget(IngredientMob mob)
    {
        CurrentTarget = mob;
        PositionHighlight(mob.transform);
        OnTargetChanged?.Invoke(mob);
        Debug.Log($"[CombatTarget] → {mob.ingredientName}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    List<IngredientMob> GetSortedMobs()
    {
        var all = FindObjectsByType<IngredientMob>();
        var result = new List<IngredientMob>();

        foreach (var m in all)
        {
            if (m == null || !m.gameObject.activeSelf) continue;
            float dist = Vector3.Distance(transform.position, m.transform.position);
            if (dist <= maxTargetRange) result.Add(m);
        }

        // Sort nearest → farthest
        result.Sort((a, b) =>
        {
            float da = Vector3.Distance(transform.position, a.transform.position);
            float db = Vector3.Distance(transform.position, b.transform.position);
            return da.CompareTo(db);
        });

        return result;
    }

    void PositionHighlight(Transform parent)
    {
        if (_highlightInstance == null) return;
        if (parent == null)
        {
            _highlightInstance.SetActive(false);
            return;
        }
        _highlightInstance.SetActive(true);
        _highlightInstance.transform.SetParent(parent, false);
        _highlightInstance.transform.localPosition = Vector3.zero;
    }

    // ── Mouse click on mob (call from IngredientMob's OnMouseDown) ────────────

    /// <summary>
    /// IngredientMob can call this from OnMouseDown to select themselves.
    /// Also triggers Palette to open with combat abilities.
    /// </summary>
    public void SelectAndOpenPalette(IngredientMob mob)
    {
        Select(mob);
        CombatAbilityBar.Instance?.OpenAbilityPalette();
    }
}
