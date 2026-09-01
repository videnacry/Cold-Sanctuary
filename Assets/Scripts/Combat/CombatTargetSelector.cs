using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema de selección de target (Tab-targeting + clic).
///
/// Generalizado de <c>IngredientMob</c> a <see cref="ITarget"/> para soportar hechizos
/// que se lanzan sobre cualquier ser vivo, objeto del entorno o NPC (no solo mobs de combate).
///
/// Compatibilidad hacia atrás: <see cref="CurrentIngredientMob"/> devuelve el target actual
/// casteado a <c>IngredientMob</c> (null si el target es otro tipo); <see cref="SelectMob"/>
/// y <see cref="SelectAndOpenPalette"/> mantienen la firma original.
///
/// Uso:
///   - Tab (teclado)          → ciclar al siguiente ITarget en rango
///   - Clic en un objeto      → seleccionarlo directamente (el objeto llama <see cref="Select"/>)
///   - Escape                 → deseleccionar
/// </summary>
public class CombatTargetSelector : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────

    public static CombatTargetSelector Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Targeting")]
    [Tooltip("Max range to consider a target as selectable (m).")]
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
    public event Action<ITarget> OnTargetChanged;

    /// <summary>Fired when the selected target is an IngredientMob (backward compat).</summary>
    public event Action<IngredientMob> OnMobTargetChanged;

    // ── State ─────────────────────────────────────────────────────────────────

    /// <summary>Target actual (cualquier ITarget). Null = sin selección.</summary>
    public ITarget CurrentTarget { get; private set; }

    /// <summary>Shortcut: CurrentTarget casteado a IngredientMob (null si es otro tipo).</summary>
    public IngredientMob CurrentIngredientMob => CurrentTarget as IngredientMob;

    int        _currentIndex;
    GameObject _highlightInstance;

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
        if (Input.GetKeyDown(cycleKey))    CycleTarget();
        if (Input.GetKeyDown(deselectKey)) Deselect();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Selecciona cualquier ITarget (Anima, FoodItem, Swarm, etc.).</summary>
    public void Select(ITarget target)
    {
        if (target == null || target.Dead || target.Consumed) { Deselect(); return; }

        var targets = GetSortedTargets();
        _currentIndex = targets.IndexOf(target);
        if (_currentIndex < 0) _currentIndex = 0;

        ApplyTarget(target);
    }

    /// <summary>Backward compat: seleccionar un IngredientMob directamente.</summary>
    public void SelectMob(IngredientMob mob) => Select(mob);

    /// <summary>Deseleccionar el target actual.</summary>
    public void Deselect()
    {
        CurrentTarget = null;
        _currentIndex = 0;
        PositionHighlight(null);
        OnTargetChanged?.Invoke(null);
        OnMobTargetChanged?.Invoke(null);
    }

    // ── Cycle ─────────────────────────────────────────────────────────────────

    void CycleTarget()
    {
        var targets = GetSortedTargets();
        if (targets.Count == 0) { Deselect(); return; }
        _currentIndex = (_currentIndex + 1) % targets.Count;
        ApplyTarget(targets[_currentIndex]);
    }

    void ApplyTarget(ITarget target)
    {
        CurrentTarget = target;
        PositionHighlight((target as MonoBehaviour)?.transform);
        OnTargetChanged?.Invoke(target);
        OnMobTargetChanged?.Invoke(target as IngredientMob); // null si no es mob
        Debug.Log($"[CombatTarget] → {(target as MonoBehaviour)?.name ?? "?"}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    List<ITarget> GetSortedTargets()
    {
        var result = new List<ITarget>();

        // Buscar todos los MonoBehaviour que implementen ITarget en escena
        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb == null || !mb.gameObject.activeSelf) continue;
            if (mb is ITarget t && !t.Dead && !t.Consumed)
            {
                float dist = Vector3.Distance(transform.position, t.transform.position);
                if (dist <= maxTargetRange) result.Add(t);
            }
        }

        result.Sort((a, b) =>
        {
            float da = Vector3.Distance(transform.position, (a as MonoBehaviour).transform.position);
            float db = Vector3.Distance(transform.position, (b as MonoBehaviour).transform.position);
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

    // ── Mouse click on mob (backward compat) ─────────────────────────────────

    /// <summary>IngredientMob calls this from OnMouseDown to select itself and open the Palette.</summary>
    public void SelectAndOpenPalette(IngredientMob mob)
    {
        Select(mob);
        CombatAbilityBar.Instance?.OpenAbilityPalette();
    }
}
