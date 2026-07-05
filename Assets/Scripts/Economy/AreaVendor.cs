using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles buying and selling at a specific sanctuary area or at the market.
///
/// Pricing rules:
///   Workshop vendor (isMarket = false):
///     - Only accepts items whose sourceArea matches this vendor's area
///       OR items from any other zone (cross-zone items) — but prices differ.
///     - Selling item from OWN area      → sellPrice × ownAreaMultiplier (base, e.g. 1.0×)
///     - Selling item from OTHER area    → sellPrice × crossZoneBonus    (premium, e.g. 1.6×)
///     - Buying item available here      → buyPrice  × workshopBuyMultiplier (standard)
///
///   Market vendor (isMarket = true):
///     - Accepts ALL items regardless of source.
///     - Selling any item               → sellPrice × marketSellMultiplier  (discounted, e.g. 0.7×)
///     - Buying any item                → buyPrice  × marketBuyMultiplier   (premium,    e.g. 1.4×)
///
/// The price difference creates natural trade routes:
///   NPC in UnderwaterGarden collects P fragments → brings them to Kitchen
///   → sells at 1.6× instead of 0.7× at the market.
/// </summary>
public class AreaVendor : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Vendor Identity")]
    public string vendorName = "Vendedor";

    [Tooltip("The area this vendor belongs to. Ignored when isMarket = true.")]
    public SanctuaryAreaType homeArea;

    [Tooltip("True for the market hub — accepts all items, different price multipliers.")]
    public bool isMarket;

    [Header("Workshop Price Multipliers (ignored when isMarket = true)")]
    [Tooltip("Sell multiplier for items originating from this area (base rate).")]
    public float ownAreaSellMultiplier  = 1.0f;

    [Tooltip("Sell multiplier for items from other zones (rarity premium).")]
    public float crossZoneSellBonus     = 1.6f;

    [Tooltip("Buy multiplier applied to buyPrice for items sold here.")]
    public float workshopBuyMultiplier  = 1.0f;

    [Header("Market Price Multipliers (used when isMarket = true)")]
    [Tooltip("Market takes a commission: sell price is discounted.")]
    public float marketSellMultiplier   = 0.70f;

    [Tooltip("Market charges a convenience premium on purchases.")]
    public float marketBuyMultiplier    = 1.40f;

    [Header("Stock (items available for purchase)")]
    [Tooltip("Items the vendor has in stock. Each entry is (ItemData, quantity).")]
    public VendorStockEntry[] stock;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired when the player or an NPC sells an item here.</summary>
    public event Action<ItemData, int, int> OnItemSold;   // (item, qty, coinsEarned)

    /// <summary>Fired when the player or an NPC buys an item here.</summary>
    public event Action<ItemData, int, int> OnItemBought; // (item, qty, coinsPaid)

    // ── Price queries ─────────────────────────────────────────────────────────

    /// <summary>
    /// How many coins selling <qty> of <item> earns at this vendor.
    /// </summary>
    public int SellValue(ItemData item, int qty = 1)
    {
        float multiplier = GetSellMultiplier(item);
        return Mathf.RoundToInt(item.sellPrice * multiplier * qty);
    }

    /// <summary>
    /// How many coins buying <qty> of <item> costs at this vendor.
    /// </summary>
    public int BuyPrice(ItemData item, int qty = 1)
    {
        float multiplier = isMarket ? marketBuyMultiplier : workshopBuyMultiplier;
        return Mathf.RoundToInt(item.buyPrice * multiplier * qty);
    }

    float GetSellMultiplier(ItemData item)
    {
        if (isMarket) return marketSellMultiplier;
        return item.sourceArea == homeArea ? ownAreaSellMultiplier : crossZoneSellBonus;
    }

    // ── Sell ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sell items from an Inventory to this vendor.
    /// Coins go to the provided CoinWallet (player or NPC).
    /// Returns false if the item is not in the inventory.
    /// </summary>
    public bool Sell(ItemData item, int qty, Inventory inventory, CoinWallet wallet)
    {
        if (!inventory.RemoveItem(item, qty)) return false;

        int earned = SellValue(item, qty);
        wallet.Earn(earned);

        OnItemSold?.Invoke(item, qty, earned);
        Debug.Log($"[{vendorName}] {qty}× {item.itemName} vendido por {earned} monedas " +
                  $"(×{GetSellMultiplier(item):F1} — " +
                  (isMarket ? "mercadillo" :
                   item.sourceArea == homeArea ? "zona propia" : "zona cruzada") + ").");
        return true;
    }

    /// <summary>
    /// NPC version — sells from NPCEconomy mini-inventory.
    /// Coins go to NPCEconomy wallet directly.
    /// </summary>
    public bool SellFromNPC(string itemName, int qty, NPCEconomy npc)
    {
        // NPCEconomy uses string keys, not ItemData references.
        // Coin calculation uses a flat 5-coin base (NPCEconomy default) × multiplier.
        // In a full implementation, look up ItemData by name from a registry.
        float multiplier = isMarket ? marketSellMultiplier : crossZoneSellBonus;
        int earned = Mathf.RoundToInt(5 * multiplier * qty);
        npc.ReceiveMissionReward(earned); // re-use this to add coins to NPC wallet

        OnItemSold?.Invoke(null, qty, earned);
        Debug.Log($"[{vendorName}] NPC {npc.GetComponent<WorldCharacter>()?.characterName} " +
                  $"vendió {qty}× {itemName} por {earned} monedas.");
        return true;
    }

    // ── Buy ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Buy an item from this vendor's stock into an Inventory.
    /// Returns false if out of stock or insufficient coins.
    /// </summary>
    public bool Buy(ItemData item, int qty, Inventory inventory, CoinWallet wallet)
    {
        // Check stock
        var entry = FindStockEntry(item);
        if (entry == null || entry.quantity < qty)
        {
            Debug.Log($"[{vendorName}] Sin stock de {item.itemName}.");
            return false;
        }

        int cost = BuyPrice(item, qty);
        if (!wallet.CanAfford(cost))
        {
            Debug.Log($"[{vendorName}] Fondos insuficientes. Coste: {cost}, balance: {wallet.Balance}.");
            return false;
        }

        wallet.Spend(cost);
        inventory.AddItem(item, qty);
        entry.quantity -= qty;

        OnItemBought?.Invoke(item, qty, cost);
        Debug.Log($"[{vendorName}] Comprado {qty}× {item.itemName} por {cost} monedas.");
        return true;
    }

    // ── Stock helpers ─────────────────────────────────────────────────────────

    VendorStockEntry FindStockEntry(ItemData item)
    {
        if (stock == null) return null;
        foreach (var e in stock)
            if (e.item == item) return e;
        return null;
    }

    /// <summary>Items currently in stock (item, quantity).</summary>
    public IEnumerable<VendorStockEntry> Stock => stock ?? System.Array.Empty<VendorStockEntry>();
}

// ── Supporting type ───────────────────────────────────────────────────────────

[System.Serializable]
public class VendorStockEntry
{
    public ItemData item;
    [Min(0)] public int quantity = 10;
}
