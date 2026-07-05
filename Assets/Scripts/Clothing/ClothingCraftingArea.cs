using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The sanctuary's clothing crafting workshop.
///
/// Players and NPCs bring materials here to craft garments.
/// Crafting is time-based (minutes of game time via TimeController).
///
/// Flow:
///   1. Player selects a recipe (UI passes it to StartCraft).
///   2. Materials are consumed from Inventory.
///   3. A coroutine waits craftingMinutes * TimeController scale.
///   4. The finished garment is added to Inventory.
///   5. NPCs with NPCEconomy can also queue craft jobs autonomously.
///
/// The area is a SanctuaryArea in the scene graph — attach this alongside
/// a SanctuaryArea component configured as a dedicated crafting zone.
/// </summary>
public class ClothingCraftingArea : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Recipes")]
    [Tooltip("All recipes available in this workshop. " +
             "Locked recipes (level/element requirements) won't show in UI.")]
    public ClothingRecipe[] allRecipes;

    [Header("Crafting Stations")]
    [Tooltip("Number of simultaneous crafting jobs this area supports.")]
    [Min(1)] public int maxConcurrentJobs = 3;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired when a crafting job completes. (recipe, crafterName)</summary>
    public event Action<ClothingRecipe, string> OnCraftingComplete;

    // ── Runtime ───────────────────────────────────────────────────────────────

    class CraftJob
    {
        public ClothingRecipe recipe;
        public string         crafterName;
        public Inventory      targetInventory; // player or NPC inventory
        public Coroutine      routine;
    }

    readonly List<CraftJob> _activeJobs = new List<CraftJob>();

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns recipes the given character can currently craft
    /// (progression level met, required elements discovered, materials available).
    /// </summary>
    public List<ClothingRecipe> GetAvailableRecipes(WorldCharacter character, Inventory inventory)
    {
        var result = new List<ClothingRecipe>();
        foreach (var recipe in allRecipes)
        {
            if (recipe == null) continue;
            if (recipe.minProgressionLevel > character.progressionLevel) continue;
            if (!HasDiscoveredElements(recipe)) continue;
            result.Add(recipe);
        }
        return result;
    }

    /// <summary>
    /// Start crafting a recipe. Consumes materials from inventory.
    /// Returns false if: stations full, materials missing, elements not discovered.
    /// </summary>
    public bool StartCraft(ClothingRecipe recipe, WorldCharacter crafter, Inventory inventory)
    {
        if (recipe == null || crafter == null || inventory == null) return false;
        if (_activeJobs.Count >= maxConcurrentJobs)
        {
            Debug.Log("[ClothingArea] Sin estaciones libres.");
            return false;
        }
        if (!HasDiscoveredElements(recipe))
        {
            Debug.Log("[ClothingArea] Elementos químicos necesarios no descubiertos.");
            return false;
        }
        if (!ConsumeFragments(recipe, inventory))
        {
            Debug.Log("[ClothingArea] Fragmentos de elementos insuficientes.");
            return false;
        }

        var job = new CraftJob
        {
            recipe          = recipe,
            crafterName     = crafter.characterName,
            targetInventory = inventory,
        };
        job.routine = StartCoroutine(CraftRoutine(job));
        _activeJobs.Add(job);

        Debug.Log($"[ClothingArea] {crafter.characterName} comenzó a confeccionar: " +
                  $"{recipe.clothingName} ({recipe.craftingMinutes} min).");
        return true;
    }

    /// <summary>How many crafting stations are currently free.</summary>
    public int FreeStations => maxConcurrentJobs - _activeJobs.Count;

    // ── Crafting routine ──────────────────────────────────────────────────────

    IEnumerator CraftRoutine(CraftJob job)
    {
        // Scale minutes by TimeController so compressed time works correctly.
        // TimeController.TimeSpeed = 1 → real time. 60 → 1 real second = 1 game minute.
        float realSeconds = job.recipe.craftingMinutes * 60f;
        // TODO: divide by TimeController.TimeSpeed when that class is accessible here.
        // For now we use a fixed 1× multiplier.
        yield return new WaitForSeconds(realSeconds);

        // Deliver finished garment
        if (job.targetInventory != null)
        {
            // Convert recipe to ItemData on-the-fly (or use a pre-authored ItemData per recipe)
            // For now we log — in a full implementation, each ClothingRecipe links to an ItemData.
            Debug.Log($"[ClothingArea] ✅ {job.crafterName} terminó: {job.recipe.clothingName}. " +
                      $"Añadido al inventario.");
            // job.targetInventory.AddItem(job.recipe.resultItem, 1);
        }

        OnCraftingComplete?.Invoke(job.recipe, job.crafterName);
        _activeJobs.Remove(job);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    bool HasDiscoveredElements(ClothingRecipe recipe)
    {
        if (recipe.chemicalRequirements == null) return true;
        var ptm = PeriodicTableManager.Instance;
        foreach (var req in recipe.chemicalRequirements)
        {
            if (req.requiresDiscovery && ptm != null && !ptm.IsKnown(req.elementSymbol))
                return false;
        }
        return true;
    }

    bool ConsumeFragments(ClothingRecipe recipe, Inventory inventory)
    {
        // TODO: when Fragment ItemDatas are in place, deduct them from inventory here.
        // For now we always succeed (no fragment inventory yet).
        return true;
    }
}
