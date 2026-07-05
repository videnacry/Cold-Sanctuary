using System;
using UnityEngine;

/// <summary>
/// Tracks the player's coin balance. Singleton.
///
/// Coins are earned from:
///   - Mission completion (MissionTracker → Earn)
///   - Selling excess fragments/items at a VendorNPC
///
/// Coins are spent at vendors to buy armor and tools.
/// </summary>
public class CoinWallet : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────

    public static CoinWallet Instance { get; private set; }

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired whenever the balance changes. (newBalance, delta)</summary>
    public event Action<int, int> OnBalanceChanged;

    // ── State ─────────────────────────────────────────────────────────────────

    [Header("Starting balance")]
    [SerializeField] int _startingCoins = 0;

    int _balance;

    public int Balance => _balance;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _balance = _startingCoins;
    }

    // ── API ───────────────────────────────────────────────────────────────────

    /// <summary>Add coins (mission reward, item sale).</summary>
    public void Earn(int amount)
    {
        if (amount <= 0) return;
        _balance += amount;
        OnBalanceChanged?.Invoke(_balance, amount);
        Debug.Log($"[CoinWallet] +{amount} monedas → Total: {_balance}");
    }

    /// <summary>
    /// Spend coins (vendor purchase). Returns false if insufficient balance.
    /// </summary>
    public bool Spend(int amount)
    {
        if (amount > _balance)
        {
            Debug.Log($"[CoinWallet] Sin fondos. Necesita {amount}, tiene {_balance}.");
            return false;
        }
        _balance -= amount;
        OnBalanceChanged?.Invoke(_balance, -amount);
        Debug.Log($"[CoinWallet] -{amount} monedas → Total: {_balance}");
        return true;
    }

    /// <summary>True if the player can afford the given amount.</summary>
    public bool CanAfford(int amount) => _balance >= amount;
}
