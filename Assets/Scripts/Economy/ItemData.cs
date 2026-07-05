using UnityEngine;

/// <summary>
/// Defines an equippable or consumable item in the sanctuary economy.
/// Create via Assets > Create > Cold Sanctuary > Item.
///
/// Item types:
///   Tool   — increases PlayerCombat.toolDamageMultiplier
///   Armor  — increases PlayerCombat.armorDamageReduction
///   Consumable — instant stat restore on use
///   Fragment — element fragment (sellable; typically auto-generated, not authored)
/// </summary>
[CreateAssetMenu(menuName = "Cold Sanctuary/Item", fileName = "NewItem")]
public class ItemData : ScriptableObject
{
    // ── Identity ──────────────────────────────────────────────────────────────

    [Header("Identity")]
    public string itemName;

    [TextArea(1, 3)]
    public string description;

    public ItemType itemType;

    public Sprite icon;

    // ── Economy ───────────────────────────────────────────────────────────────

    [Header("Economy")]
    [Tooltip("Base sell price at a vendor.")]
    public int sellPrice = 5;

    [Tooltip("Base buy price at a vendor. 0 = not for sale.")]
    public int buyPrice = 10;

    [Tooltip("The sanctuary area this item originates from. " +
             "Selling at a different area's workshop gives a rarity bonus. " +
             "Leave as the first enum value if the item has no specific origin zone.")]
    public SanctuaryAreaType sourceArea;

    // ── Tool stats ────────────────────────────────────────────────────────────

    [Header("Tool / Armor Stats")]
    [Tooltip("Tool: multiplier applied to PlayerCombat.toolDamageMultiplier. 1 = no change.")]
    public float damageMultiplier = 1f;

    [Tooltip("Armor: reduction applied to PlayerCombat.armorDamageReduction (0–1).")]
    [Range(0f, 0.9f)]
    public float damageReduction = 0f;

    // ── Consumable stats ──────────────────────────────────────────────────────

    [Header("Consumable Stats (ignored for Tool/Armor)")]
    public MindChannel restoreChannel;
    public float       restoreAmount;

    // ── Fragment ──────────────────────────────────────────────────────────────

    [Header("Fragment (auto-filled by IngredientMob drops)")]
    public string elementSymbol;
}

public enum ItemType
{
    Tool,
    Armor,
    Consumable,
    Fragment,
}
