using System;
using System.Collections.Generic;
using UnityEngine;

// Extiende FollowingArrays con soporte para PaletteConfig:
// modos Direct, Formula, Hybrid y Dialogue.
// Soporta agrupación por categoría cuando el número de elementos supera un umbral.
// Uso: llama a Open(config, contexto) desde el sistema que quiere abrir el menú.
public class Palette : FollowingArrays
{
    [SerializeField] GameObject elementPrefab;

    PaletteConfig            config;
    List<PaletteElementData> formula         = new List<PaletteElementData>();
    List<PaletteElement>     spawnedElements = new List<PaletteElement>();

    PaletteDisplayMode displayMode    = PaletteDisplayMode.All;
    int                activeGroup    = 0;

    public event Action<PaletteResult>      OnFormulaEvaluated;
    public event Action<PaletteElementData> OnActionFired;

    // ──────────────────────────────────────────────
    // API pública
    // ──────────────────────────────────────────────

    public void Open(PaletteConfig cfg, GameObject ctx)
    {
        config      = cfg;
        user        = ctx;
        displayMode = cfg.defaultDisplay;
        activeGroup = 0;
        formula.Clear();
        ShowArrays();
    }

    public void SetDisplayMode(PaletteDisplayMode mode)
    {
        displayMode = mode;
        Refresh();
    }

    public void NavigateGroup(int direction)
    {
        if (config?.groups == null || config.groups.Length == 0) return;
        activeGroup = (activeGroup + direction + config.groups.Length) % config.groups.Length;
        Refresh();
    }

    // Elementos en estado UI disponibles para materializar
    public List<PaletteElement> GetMaterializableElements() =>
        spawnedElements.FindAll(e => e.state == ElementState.UI && !e.isLocked);

    public List<PaletteElement> GetAllElements() => new List<PaletteElement>(spawnedElements);

    // ──────────────────────────────────────────────
    // Renderizado
    // ──────────────────────────────────────────────

    public override void ShowArrays()
    {
        if (config == null || elementPrefab == null) return;

        spawnedElements.Clear();

        foreach (PaletteElementData data in GetVisibleElements())
        {
            GameObject go = Instantiate(elementPrefab, user.transform);
            PaletteElement el = go.GetComponent<PaletteElement>() ?? go.AddComponent<PaletteElement>();
            el.Configure(data, GetPlayerBond());
            el.OnActivated += HandleElementActivated;
            spawnedElements.Add(el);
            uiElements.Enqueue(go);
        }

        AddCloseButton();

        if (parentFollowingArrays != null)
        {
            DestroyUIElements().ExceptGameObject(gameObject).Execute();
            parentFollowingArrays.uiElements.Clear();
        }
        SetWholeGameObjectRenderer(false);
    }

    // ──────────────────────────────────────────────
    // Lógica de activación
    // ──────────────────────────────────────────────

    void HandleElementActivated(PaletteElementData data)
    {
        switch (config.mode)
        {
            case PaletteConfig.Mode.Direct:
                OnActionFired?.Invoke(data);
                break;

            case PaletteConfig.Mode.Formula:
                AddToFormula(data);
                break;

            case PaletteConfig.Mode.Hybrid:
                // desbloqueado por práctica → directo (sin bonus)
                // sin desbloquear → acumula; al completar aplica formulaMultiplier
                if (data.isDirectUnlocked)
                    OnActionFired?.Invoke(data);
                else
                    AddToFormula(data);
                break;

            case PaletteConfig.Mode.Dialogue:
                if (data.type == PaletteElementType.DialogueChoice)
                    OnActionFired?.Invoke(data);
                break;
        }
    }

    void AddToFormula(PaletteElementData data)
    {
        if (formula.Contains(data)) return;
        if (formula.Count >= config.maxSelection) return;

        formula.Add(data);
        foreach (PaletteElement el in spawnedElements)
            if (el.data == data) el.SetSelected(true);

        if (formula.Count == config.maxSelection)
            EvaluateFormula();
    }

    void EvaluateFormula()
    {
        if (config.evaluator != null)
        {
            PaletteResult result = config.evaluator.Evaluate(formula, user);
            result.magnitude *= config.formulaMultiplier;
            OnFormulaEvaluated?.Invoke(result);
        }

        formula.Clear();
        foreach (PaletteElement el in spawnedElements)
            el.SetSelected(false);
    }

    // ──────────────────────────────────────────────
    // Agrupación
    // ──────────────────────────────────────────────

    PaletteElementData[] GetVisibleElements()
    {
        bool hasGroups = config.groups != null && config.groups.Length > 0;

        // Si hay grupos y el modo es ByGroup, mostrar solo el grupo activo
        if (hasGroups && displayMode == PaletteDisplayMode.ByGroup)
        {
            int idx = Mathf.Clamp(activeGroup, 0, config.groups.Length - 1);
            return config.groups[idx].elements ?? new PaletteElementData[0];
        }

        // Sugerir ByGroup automáticamente si se supera el umbral
        if (hasGroups && config.elements != null && config.elements.Length > config.groupThreshold
            && displayMode == PaletteDisplayMode.All)
        {
            // TODO: mostrar aviso al jugador de que puede cambiar a vista por grupo
        }

        return config.elements ?? new PaletteElementData[0];
    }

    void Refresh()
    {
        // Destruir botones actuales y re-renderizar con el nuevo grupo/modo
        foreach (GameObject go in uiElements) Destroy(go);
        uiElements.Clear();
        formula.Clear();
        ShowArrays();
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    int GetPlayerBond()
    {
        // TODO: leer bond real cuando exista el sistema de stats del jugador
        return 0;
    }
}
