using UnityEngine;

/// <summary>
/// Defines one clothing item craftable in the ClothingCraftingArea.
/// Create via Assets > Create > Cold Sanctuary > Clothing Recipe.
///
/// Materials come from two tracks:
///   Textile    — gathered from plants/animals (linen, wool, bio-silk from underwater)
///   Chemical   — synthesized from discovered periodic table elements
///               (graphene from C, Kevlar-analog from C+N, aerogel from Si+O)
///
/// The clothing system replaces generic "armor" — items are believable garments
/// with real-world material inspirations, while still providing stat bonuses.
/// Later the team can decide if heavier protection (genuine armor) makes sense.
///
/// Stats a garment can provide:
///   defenseRating        — reduces incoming mob damage (maps to armorDamageReduction)
///   velocityBonus        — added to player velocity stat
///   observationBonus     — added to observationRadius
///   specialEffect        — optional tag for unique abilities (e.g. "bioluminescent")
/// </summary>
[CreateAssetMenu(menuName = "Cold Sanctuary/Clothing Recipe", fileName = "NewClothingRecipe")]
public class ClothingRecipe : ScriptableObject
{
    // ── Identity ──────────────────────────────────────────────────────────────

    [Header("Identity")]
    public string clothingName;

    [TextArea(1, 3)]
    public string description;

    public Sprite icon;

    public ClothingSlot slot;

    public ClothingTrack track;

    // ── Material requirements ─────────────────────────────────────────────────

    [Header("Textile Materials (ClothingTrack.Textile or Hybrid)")]
    [Tooltip("E.g. 'Lino', 'Lana', 'Seda Bio-marina'")]
    public TextileMaterial[] textileRequirements;

    [Header("Chemical Materials (ClothingTrack.Chemical or Hybrid)")]
    [Tooltip("Element symbols required in PeriodicTableManager (player must have discovered them).")]
    public ChemicalMaterial[] chemicalRequirements;

    // ── Crafting ──────────────────────────────────────────────────────────────

    [Header("Crafting")]
    [Tooltip("Minutes of real in-game time the crafting takes.")]
    public float craftingMinutes = 30f;

    [Tooltip("Minimum player progression level to unlock this recipe.")]
    public int minProgressionLevel = 1;

    // ── Stats ─────────────────────────────────────────────────────────────────

    [Header("Stats")]
    [Tooltip("Reduces incoming mob damage (0 = none, 0.5 = 50% reduction). " +
             "Maps to PlayerCombat.armorDamageReduction.")]
    [Range(0f, 0.9f)]
    public float defenseRating;

    [Tooltip("Added to PlayerStats.velocity.")]
    public float velocityBonus;

    [Tooltip("Added to PlayerStats.observationRadius.")]
    public float observationBonus;

    [Tooltip("Optional special effect tag (e.g. 'bioluminescent', 'heat_resistant', 'conductive').")]
    public string specialEffect;

    // ── Economy ───────────────────────────────────────────────────────────────

    [Header("Economy")]
    [Tooltip("Sell value at a vendor.")]
    public int sellPrice = 30;
}

// ── Supporting types ──────────────────────────────────────────────────────────

public enum ClothingSlot
{
    Head,
    Torso,
    Hands,
    Legs,
    Feet,
    Accessory,
}

public enum ClothingTrack
{
    Textile,    // gathered materials only (linen, wool, silk)
    Chemical,   // synthesized from elements (graphene, aerogel, bio-polymer)
    Hybrid,     // combines both tracks (e.g. graphene-reinforced linen)
}

[System.Serializable]
public class TextileMaterial
{
    [Tooltip("Material name (e.g. 'Lino', 'Lana de Oveja', 'Seda Bio-marina').")]
    public string materialName;

    [Tooltip("Quantity required.")]
    public int quantity = 1;
}

[System.Serializable]
public class ChemicalMaterial
{
    [Tooltip("Element symbol (e.g. 'C' for graphene, 'Si' for aerogel).")]
    public string elementSymbol;

    [Tooltip("Number of element fragments required.")]
    public int fragmentsRequired = 5;

    [Tooltip("Element must be discovered before the recipe appears.")]
    public bool requiresDiscovery = true;
}
