using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The player's inventory. Singleton.
///
/// Holds items (tools, armor, consumables, element fragments).
/// Equipping a tool/armor pushes its stats onto PlayerCombat.
/// Selling an item gives coins via CoinWallet.
///
/// Fragments collected beyond a mission's requirement stay here and can be sold.
/// </summary>
public class Inventory : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────

    public static Inventory Instance { get; private set; }

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired when an item is added or its quantity changes.</summary>
    public event Action<ItemData, int> OnItemAdded;

    /// <summary>Fired when an item is removed.</summary>
    public event Action<ItemData, int> OnItemRemoved;

    /// <summary>Fired when a tool or armor is equipped.</summary>
    public event Action<ItemData> OnItemEquipped;

    // ── State ─────────────────────────────────────────────────────────────────

    // ItemData → quantity
    readonly Dictionary<ItemData, int> _items = new Dictionary<ItemData, int>();

    ItemData _equippedTool;
    ItemData _equippedArmor;

    public ItemData EquippedTool  => _equippedTool;
    public ItemData EquippedArmor => _equippedArmor;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Add / Remove ──────────────────────────────────────────────────────────

    /// <summary>Add an item (or increase quantity).</summary>
    public void AddItem(ItemData item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return;

        if (_items.ContainsKey(item))
            _items[item] += quantity;
        else
            _items[item] = quantity;

        OnItemAdded?.Invoke(item, _items[item]);
        Debug.Log($"[Inventory] +{quantity}× {item.itemName}. Total: {_items[item]}");
    }

    /// <summary>
    /// Remove quantity of an item. Returns false if insufficient stock.
    /// </summary>
    public bool RemoveItem(ItemData item, int quantity = 1)
    {
        if (item == null || !_items.ContainsKey(item)) return false;
        if (_items[item] < quantity) return false;

        _items[item] -= quantity;
        if (_items[item] == 0) _items.Remove(item);

        OnItemRemoved?.Invoke(item, quantity);
        return true;
    }

    /// <summary>Current quantity of an item (0 if not in inventory).</summary>
    public int GetQuantity(ItemData item)
        => item != null && _items.ContainsKey(item) ? _items[item] : 0;

    /// <summary>Snapshot of all items for UI display.</summary>
    public IReadOnlyDictionary<ItemData, int> AllItems => _items;

    // ── Equip ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Equip a tool or armor from the inventory.
    /// Pushes stats onto the PlayerCombat component on the Player.
    /// </summary>
    public bool Equip(ItemData item)
    {
        if (item == null || GetQuantity(item) < 1) return false;

        var combat = FindFirstObjectByType<PlayerCombat>();

        switch (item.itemType)
        {
            case ItemType.Tool:
                _equippedTool = item;
                if (combat != null)
                    combat.toolDamageMultiplier = item.damageMultiplier;
                break;

            case ItemType.Armor:
                _equippedArmor = item;
                if (combat != null)
                    combat.armorDamageReduction = item.damageReduction;
                break;

            default:
                Debug.Log($"[Inventory] {item.itemName} no es equipable.");
                return false;
        }

        OnItemEquipped?.Invoke(item);
        Debug.Log($"[Inventory] Equipado: {item.itemName}.");
        return true;
    }

    // ── Sell ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sell one unit of an item. Coins go to CoinWallet.
    /// Returns false if item not in inventory.
    /// </summary>
    public bool SellItem(ItemData item, int quantity = 1)
    {
        if (!RemoveItem(item, quantity)) return false;

        int total = item.sellPrice * quantity;
        CoinWallet.Instance?.Earn(total);

        Debug.Log($"[Inventory] Vendido {quantity}× {item.itemName} → +{total} monedas.");
        return true;
    }

    // ── Use (consumable) ──────────────────────────────────────────────────────

    /// <summary>Use a consumable item. Applies stat restore and removes one from inventory.</summary>
    public bool UseConsumable(ItemData item)
    {
        if (item == null || item.itemType != ItemType.Consumable) return false;
        if (!RemoveItem(item, 1)) return false;

        var stats = FindFirstObjectByType<PlayerStats>();
        if (stats != null && item.restoreAmount > 0)
            stats.RestoreMind(item.restoreAmount, item.restoreChannel);

        Debug.Log($"[Inventory] Usado: {item.itemName} → +{item.restoreAmount} {item.restoreChannel}.");
        return true;
    }
}
