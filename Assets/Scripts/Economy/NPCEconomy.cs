using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gives an NPC their own wallet and mini-inventory so they can participate in
/// the sanctuary's economy independently of the player.
///
/// NPCs:
///   - Receive a share of mission rewards via ReceiveMissionReward().
///   - Sell excess items (items above maxKeepQuantity) to earn more coins.
///   - Buy clothing from ClothingCraftingArea when they have enough coins.
///   - Sell their old clothing slot before equipping the new one.
///
/// This creates a living economy: while the player is away, NPCs are gearing up,
/// selling, and buying — the world doesn't stand still.
///
/// Attach alongside WorldCharacter.
/// </summary>
[RequireComponent(typeof(WorldCharacter))]
public class NPCEconomy : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Economy Behaviour")]
    [Tooltip("How often (game seconds) this NPC checks if they want to buy/sell.")]
    public float economyTickInterval = 60f;

    [Tooltip("Max quantity of any single item the NPC keeps before selling the excess.")]
    public int maxKeepQuantity = 3;

    [Tooltip("Minimum coins the NPC keeps in reserve before spending on clothing.")]
    public int spendingThreshold = 50;

    [Header("Clothing Preferences")]
    [Tooltip("Clothing recipes the NPC wants to buy when they can afford it. " +
             "Ordered by priority (index 0 = highest priority).")]
    public ClothingRecipe[] wantedClothing;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired when the NPC buys a clothing item.</summary>
    public event Action<NPCEconomy, ClothingRecipe> OnClothingPurchased;

    /// <summary>Fired when the NPC sells an item.</summary>
    public event Action<NPCEconomy, string, int> OnItemSold; // (itemName, coinsEarned)

    // ── State ─────────────────────────────────────────────────────────────────

    WorldCharacter _character;

    int _coins;
    public int Coins => _coins;

    // Mini-inventory: itemName → quantity (simplified — no ItemData dependency)
    readonly Dictionary<string, int> _items = new Dictionary<string, int>();

    // Equipped clothing per slot
    readonly Dictionary<ClothingSlot, ClothingRecipe> _equipped
        = new Dictionary<ClothingSlot, ClothingRecipe>();

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake() => _character = GetComponent<WorldCharacter>();

    void Start() => StartCoroutine(EconomyLoop());

    // ── Mission reward ────────────────────────────────────────────────────────

    /// <summary>
    /// Called by MissionTracker (or the game system) when a group mission completes.
    /// coinShare = total coins / number of participants.
    /// </summary>
    public void ReceiveMissionReward(int coinShare, string itemName = null, int itemQty = 0)
    {
        _coins += coinShare;
        if (!string.IsNullOrEmpty(itemName) && itemQty > 0)
            AddItem(itemName, itemQty);

        Debug.Log($"[{_character.characterName}] Recibió recompensa: " +
                  $"+{coinShare} monedas" +
                  (itemName != null ? $" + {itemQty}× {itemName}" : "") +
                  $". Total: {_coins} monedas.");
    }

    // ── Item management ───────────────────────────────────────────────────────

    public void AddItem(string itemName, int qty)
    {
        if (!_items.ContainsKey(itemName)) _items[itemName] = 0;
        _items[itemName] += qty;
    }

    // ── Economy loop ──────────────────────────────────────────────────────────

    IEnumerator EconomyLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(economyTickInterval);
            SellExcessItems();
            TryBuyClothing();
        }
    }

    void SellExcessItems()
    {
        var toSell = new List<(string name, int qty)>();
        foreach (var kv in _items)
        {
            int excess = kv.Value - maxKeepQuantity;
            if (excess > 0) toSell.Add((kv.Key, excess));
        }

        foreach (var (name, qty) in toSell)
        {
            // Simple sell price: 5 coins per fragment (could look up ItemData later)
            int earned = qty * 5;
            _items[name] -= qty;
            _coins += earned;

            OnItemSold?.Invoke(this, name, earned);
            Debug.Log($"[{_character.characterName}] Vendió {qty}× {name} → +{earned} monedas.");
        }
    }

    void TryBuyClothing()
    {
        if (_coins < spendingThreshold) return;
        if (wantedClothing == null || wantedClothing.Length == 0) return;

        foreach (var recipe in wantedClothing)
        {
            if (recipe == null) continue;

            // Already wearing something in this slot?
            if (_equipped.TryGetValue(recipe.slot, out var current))
            {
                if (current == recipe) continue;       // already wearing it
                if (current.defenseRating >= recipe.defenseRating
                    && current.velocityBonus >= recipe.velocityBonus)
                    continue;                          // current is already better

                // Sell the old item before buying the new one
                int sellGain = current.sellPrice;
                _coins += sellGain;
                Debug.Log($"[{_character.characterName}] Vendió '{current.clothingName}' " +
                          $"→ +{sellGain} monedas.");
            }

            if (_coins >= recipe.sellPrice * 2) // buy at ~2× sell price
            {
                int buyCost = recipe.sellPrice * 2;
                _coins -= buyCost;
                _equipped[recipe.slot] = recipe;

                OnClothingPurchased?.Invoke(this, recipe);
                Debug.Log($"[{_character.characterName}] Compró '{recipe.clothingName}' " +
                          $"(-{buyCost} monedas). Restante: {_coins}.");
                break; // one purchase per tick
            }
        }
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public ClothingRecipe GetEquipped(ClothingSlot slot)
        => _equipped.TryGetValue(slot, out var r) ? r : null;

    public IReadOnlyDictionary<string, int> AllItems => _items;
}
