using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks which periodic table elements the player has discovered.
/// Every system that can reveal an element (IngredientMob, AreaTask, alchemy reactions)
/// calls PeriodicTableManager.Instance.Discover(symbol).
///
/// The 118 elements act as the game's Pokédex — permanent collectibles that unlock
/// enchantments and reveal lore entries.
/// </summary>
public class PeriodicTableManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────

    public static PeriodicTableManager Instance { get; private set; }

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired the first time an element is discovered. symbol = "Na", "C", etc.</summary>
    public event Action<string> OnElementDiscovered;

    /// <summary>Fired on every collection attempt, including already-known elements.</summary>
    public event Action<string, bool> OnElementCollected; // (symbol, isNew)

    // ── Data ──────────────────────────────────────────────────────────────────

    /// <summary>All 118 elements with metadata. Populated in Awake().</summary>
    readonly Dictionary<string, ElementData> _catalog = new Dictionary<string, ElementData>();

    /// <summary>Elements the player has discovered so far.</summary>
    readonly HashSet<string> _discovered = new HashSet<string>();

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildCatalog();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Attempt to discover an element. If it's new, fires OnElementDiscovered.
    /// Returns true if this was the first discovery.
    /// </summary>
    public bool Discover(string symbol)
    {
        if (string.IsNullOrEmpty(symbol)) return false;
        symbol = symbol.Trim();

        bool isNew = _discovered.Add(symbol);
        OnElementCollected?.Invoke(symbol, isNew);

        if (isNew)
        {
            OnElementDiscovered?.Invoke(symbol);
            Debug.Log($"[Tabla Periódica] ¡Nuevo elemento descubierto! {GetDisplayName(symbol)}");
        }

        return isNew;
    }

    /// <summary>True if the player already knows this element.</summary>
    public bool IsKnown(string symbol) => _discovered.Contains(symbol.Trim());

    /// <summary>Number of elements discovered so far.</summary>
    public int DiscoveredCount => _discovered.Count;

    /// <summary>Total elements in the catalog.</summary>
    public int TotalCount => _catalog.Count;

    /// <summary>Returns the ElementData for a symbol, or null if unknown.</summary>
    public ElementData GetData(string symbol)
        => _catalog.TryGetValue(symbol.Trim(), out var data) ? data : null;

    /// <summary>Display name: "Na — Sodio (11)" format.</summary>
    public string GetDisplayName(string symbol)
    {
        var data = GetData(symbol);
        return data != null ? $"{symbol} — {data.spanishName} ({data.atomicNumber})" : symbol;
    }

    /// <summary>All discovered element symbols.</summary>
    public IEnumerable<string> AllDiscovered() => _discovered;

    /// <summary>Every symbol in the catalog, discovered or not.</summary>
    public IEnumerable<string> AllSymbols => _catalog.Keys;

    /// <summary>
    /// Elements in a given group (period table column).
    /// Useful for checking if the player has completed a group to unlock an enchantment.
    /// </summary>
    public List<string> GetGroup(ElementGroup group)
    {
        var result = new List<string>();
        foreach (var kv in _catalog)
            if (kv.Value.group == group) result.Add(kv.Key);
        return result;
    }

    /// <summary>True if the player has discovered every element in a group.</summary>
    public bool HasCompleteGroup(ElementGroup group)
    {
        foreach (var symbol in GetGroup(group))
            if (!IsKnown(symbol)) return false;
        return true;
    }

    // ── Catalog ───────────────────────────────────────────────────────────────

    // Groups used in Cold Sanctuary's enchantment system
    public enum ElementGroup
    {
        AlkaliMetals,
        AlkalineEarths,
        TransitionMetals,
        PostTransitionMetals,
        Metalloids,
        ReactiveNonmetals,
        NobleGases,
        Lanthanides,
        Actinides,
    }

    void BuildCatalog()
    {
        // Format: Add(symbol, atomicNumber, spanishName, group, area hint)
        // Only a curated subset here — add more as areas are designed.
        // Priority: elements discoverable in Tier 1–2 areas first.

        // ── Reactive Nonmetals (most common in kitchen / lab) ─────────────────
        Add("H",  1,  "Hidrógeno",     ElementGroup.ReactiveNonmetals);
        Add("C",  6,  "Carbono",        ElementGroup.ReactiveNonmetals);
        Add("N",  7,  "Nitrógeno",      ElementGroup.ReactiveNonmetals);
        Add("O",  8,  "Oxígeno",        ElementGroup.ReactiveNonmetals);
        Add("F",  9,  "Flúor",          ElementGroup.ReactiveNonmetals);
        Add("P",  15, "Fósforo",        ElementGroup.ReactiveNonmetals);
        Add("S",  16, "Azufre",         ElementGroup.ReactiveNonmetals);
        Add("Cl", 17, "Cloro",          ElementGroup.ReactiveNonmetals);
        Add("Se", 34, "Selenio",        ElementGroup.ReactiveNonmetals);
        Add("Br", 35, "Bromo",          ElementGroup.ReactiveNonmetals);
        Add("I",  53, "Yodo",           ElementGroup.ReactiveNonmetals);

        // ── Alkali Metals (kitchen salt, sea water) ───────────────────────────
        Add("Li", 3,  "Litio",          ElementGroup.AlkaliMetals);
        Add("Na", 11, "Sodio",          ElementGroup.AlkaliMetals);
        Add("K",  19, "Potasio",        ElementGroup.AlkaliMetals);

        // ── Alkaline Earths (cub nutrition, cultured meat) ────────────────────
        Add("Be", 4,  "Berilio",        ElementGroup.AlkalineEarths);
        Add("Mg", 12, "Magnesio",       ElementGroup.AlkalineEarths);
        Add("Ca", 20, "Calcio",         ElementGroup.AlkalineEarths);
        Add("Sr", 38, "Estroncio",      ElementGroup.AlkalineEarths);
        Add("Ba", 56, "Bario",          ElementGroup.AlkalineEarths);

        // ── Noble Gases (fuel lab / electrolysis byproducts) ──────────────────
        Add("He", 2,  "Helio",          ElementGroup.NobleGases);
        Add("Ne", 10, "Neón",           ElementGroup.NobleGases);
        Add("Ar", 18, "Argón",          ElementGroup.NobleGases);
        Add("Kr", 36, "Kriptón",        ElementGroup.NobleGases);
        Add("Xe", 54, "Xenón",          ElementGroup.NobleGases);

        // ── Transition Metals (vehicle workshop, submarine) ───────────────────
        Add("Fe", 26, "Hierro",         ElementGroup.TransitionMetals);
        Add("Cu", 29, "Cobre",          ElementGroup.TransitionMetals);
        Add("Zn", 30, "Zinc",           ElementGroup.TransitionMetals);
        Add("Ag", 47, "Plata",          ElementGroup.TransitionMetals);
        Add("Pt", 78, "Platino",        ElementGroup.TransitionMetals);
        Add("Au", 79, "Oro",            ElementGroup.TransitionMetals);
        Add("Hg", 80, "Mercurio",       ElementGroup.TransitionMetals);

        // ── Post-Transition / Metalloids (alchemy lab, underwater) ────────────
        Add("B",  5,  "Boro",           ElementGroup.Metalloids);
        Add("Si", 14, "Silicio",        ElementGroup.Metalloids);
        Add("As", 33, "Arsénico",       ElementGroup.Metalloids);
        Add("Sb", 51, "Antimonio",      ElementGroup.Metalloids);
        Add("Te", 52, "Telurio",        ElementGroup.Metalloids);
        Add("Al", 13, "Aluminio",       ElementGroup.PostTransitionMetals);
        Add("Sn", 50, "Estaño",         ElementGroup.PostTransitionMetals);
        Add("Pb", 82, "Plomo",          ElementGroup.PostTransitionMetals);

        // ── Lanthanides / Actinides (monster section, rare areas) ─────────────
        Add("La", 57, "Lantano",        ElementGroup.Lanthanides);
        Add("Ce", 58, "Cerio",          ElementGroup.Lanthanides);
        Add("Gd", 64, "Gadolinio",      ElementGroup.Lanthanides);
        Add("U",  92, "Uranio",         ElementGroup.Actinides);
        Add("Pu", 94, "Plutonio",       ElementGroup.Actinides);
        Add("Ra", 88, "Radio",          ElementGroup.AlkalineEarths); // rare, underwater
    }

    void Add(string symbol, int atomicNumber, string spanishName, ElementGroup group)
        => _catalog[symbol] = new ElementData(symbol, atomicNumber, spanishName, group);
}

// ── Supporting types ──────────────────────────────────────────────────────────

[System.Serializable]
public class ElementData
{
    public string symbol;
    public int    atomicNumber;
    public string spanishName;
    public PeriodicTableManager.ElementGroup group;

    public ElementData(string symbol, int atomicNumber, string spanishName,
                       PeriodicTableManager.ElementGroup group)
    {
        this.symbol       = symbol;
        this.atomicNumber = atomicNumber;
        this.spanishName  = spanishName;
        this.group        = group;
    }
}
