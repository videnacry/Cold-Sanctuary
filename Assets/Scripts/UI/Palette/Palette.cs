using System;
using System.Collections.Generic;
using UnityEngine;

// Extiende FollowingArrays con soporte para PaletteConfig:
// modo Direct, Formula (con acumulación de ingredientes) y Dialogue.
// Uso: llama a Open(config, contexto) desde el sistema que quiere abrir el menú.
public class Palette : FollowingArrays
{
    [SerializeField] GameObject elementPrefab;

    PaletteConfig            config;
    List<PaletteElementData> formula      = new List<PaletteElementData>();
    List<PaletteElement>     spawnedElements = new List<PaletteElement>();

    public event Action<PaletteResult>      OnFormulaEvaluated;
    public event Action<PaletteElementData> OnActionFired;

    // ──────────────────────────────────────────────
    // API pública
    // ──────────────────────────────────────────────

    public void Open(PaletteConfig cfg, GameObject ctx)
    {
        config = cfg;
        user   = ctx;
        formula.Clear();
        ShowArrays();
    }

    // ──────────────────────────────────────────────
    // Renderizado
    // ──────────────────────────────────────────────

    public override void ShowArrays()
    {
        if (config == null || elementPrefab == null) return;

        spawnedElements.Clear();

        foreach (PaletteElementData data in config.elements)
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

            case PaletteConfig.Mode.Dialogue:
                if (data.type == PaletteElementType.DialogueChoice)
                    OnActionFired?.Invoke(data);
                break;
        }
    }

    void AddToFormula(PaletteElementData data)
    {
        // no repetir el mismo ingrediente
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
            OnFormulaEvaluated?.Invoke(result);
        }

        formula.Clear();
        foreach (PaletteElement el in spawnedElements)
            el.SetSelected(false);
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
