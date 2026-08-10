using System.Collections.Generic;
using UnityEngine;

/// <summary>Coste de un hechizo en elementos (docs/magic-metabolism-progression.md §1).</summary>
[System.Serializable]
public class ElementCost
{
    public string symbol = "C";
    public float amount = 1f;
}

/// <summary>
/// RESERVAS DE MAGIA por elemento (docs/magic-metabolism-progression.md §1): el mago **sostiene mágicamente**
/// un stock de cada elemento — **lo que el jugador ve en la tabla periódica, con su conteo**. Cada **hechizo es
/// una reacción química** con un **coste** (`ElementCost`s); al **agotar** un elemento, no se pueden lanzar
/// hechizos que dependan de él. Se rellena procesando materia (comer/absorber → `Add`). Opt-in.
/// Reutiliza `ElementAmount` (symbol, amount) de `Constitution`.
/// </summary>
public class MagicReserves : MonoBehaviour
{
    public Anima anima;

    [Tooltip("Las crea el PRIMER HECHIZO (Grimoire → Unlock). Hasta entonces, el exceso de comida va a grasa (no a magia).")]
    public bool unlocked = false;
    [Tooltip("Tope por elemento — BASE. El tope real se ESCALA con los stats (ver EffectiveCapPerElement). "
        + "La escala por santuario es solo referencia: como todo, depende de los stats.")]
    public float capPerElement = 100f;

    [Header("Reservas por elemento (símbolo de la tabla periódica → cantidad). Agotado → no lanza ese hechizo.")]
    public List<ElementAmount> reserves = new List<ElementAmount>();

    [Header("Reserva de ENERGÍA (julios). La libera DESCOMPONER materia (compuesto→átomo→núcleo→quarks): "
        + "cada nivel suelta MUCHÍSima más (química→nuclear→masa-energía). Paga los hechizos T3-T5 y los 'bosones'.")]
    [Tooltip("Julios almacenados. Se rellena en las cocinas al descomponer/desintegrar (StoreEnergy).")]
    public float energy = 0f;
    [Tooltip("Tope de energía — BASE. El tope real se ESCALA con los stats (ver EffectiveEnergyCap).")]
    public float energyCap = 1e6f;

    [Tooltip("Símbolos que siembra el primer hechizo (pools vacías, con tope). Los básicos de la vida.")]
    public List<string> seedElements = new List<string> { "H", "C", "N", "O" };

    void Awake() { if (anima == null) anima = GetComponent<Anima>(); }

    // Niveles de alma (opcional): multiplican los topes junto con las aptitudes. 0 si no hay CharacterLevel.
    int SoulLevels { get { CharacterLevel cl = GetComponent<CharacterLevel>(); return cl != null ? cl.SoulLevels : 0; } }

    /// <summary>Tope REAL por elemento = base × factor de STATS (stats-as-truth). La materia que puedes sostener
    /// escala con el "aguante" (resistencia/fuerza/masa, vía `DerivedStats.MaxHealth`); por eso un mago avanzado
    /// almacena para crear una casa y uno novato apenas nada. Sin `anima` → el valor base.</summary>
    public float EffectiveCapPerElement
    {
        get
        {
            if (anima == null) return capPerElement;
            float health = DerivedStats.MaxHealth(Aptitudes.From(anima), SoulLevels);
            return capPerElement * Mathf.Max(0.1f, health / 100f);   // 100 = MaxHealth con aptitudes base
        }
    }

    /// <summary>Tope REAL de energía = base × factor de STATS. La energía que puedes sostener escala con el
    /// **maná** (razón/memoria, vía `DerivedStats.MaxMana`). Sin `anima` → el valor base.</summary>
    public float EffectiveEnergyCap
    {
        get
        {
            if (anima == null) return energyCap;
            float mana = DerivedStats.MaxMana(Aptitudes.From(anima), SoulLevels);
            return energyCap * Mathf.Max(0.1f, mana / 50f);          // 50 = MaxMana con aptitudes base
        }
    }

    /// <summary>El PRIMER HECHIZO: crea las pools de magia (vacías, con tope) y la reserva de energía.
    /// A partir de aquí el exceso de comer/descomponer las llena. Idempotente.</summary>
    public void Unlock()
    {
        if (unlocked) return;
        unlocked = true;
        foreach (string s in seedElements)
            if (!string.IsNullOrEmpty(s) && !Has(s))
                reserves.Add(new ElementAmount { symbol = s, amount = 0f });
        Debug.Log($"[Reservas] «{(anima != null ? anima.name : name)}» despierta las pools de magia " +
                  $"(tope {capPerElement}/elemento; energía tope {energyCap:0} J): {string.Join(", ", seedElements)}.");
    }

    bool Has(string symbol)
    {
        foreach (ElementAmount r in reserves) if (r != null && r.symbol == symbol) return true;
        return false;
    }

    /// <summary>Absorbe energía de descomponer materia (julios), hasta el tope; devuelve el sobrante que no cupo.</summary>
    public float StoreEnergy(float joules)
    {
        if (joules <= 0f) return 0f;
        float put = Mathf.Min(joules, Mathf.Max(0f, EffectiveEnergyCap - energy));
        energy += put;
        return joules - put;
    }

    /// <summary>Paga un coste en energía (julios) si hay reserva (todo o nada). Para hechizos nuclear/masa-energía.</summary>
    public bool PayEnergy(float joules)
    {
        if (joules <= 0f) return true;
        if (energy < joules) return false;
        energy -= joules;
        return true;
    }

    /// <summary>Guarda hasta el tope; devuelve el sobrante (lo que no cupo → grasa). El exceso de comer entra aquí.</summary>
    public float Store(string symbol, float amount)
    {
        if (amount <= 0f) return 0f;
        float cap = EffectiveCapPerElement;
        foreach (ElementAmount r in reserves)
            if (r != null && r.symbol == symbol)
            {
                float put = Mathf.Min(amount, Mathf.Max(0f, cap - r.amount));
                r.amount += put;
                return amount - put;
            }
        float put2 = Mathf.Min(amount, cap);
        reserves.Add(new ElementAmount { symbol = symbol, amount = put2 });
        return amount - put2;
    }

    public float Get(string symbol)
    {
        foreach (ElementAmount r in reserves)
            if (r != null && r.symbol == symbol) return r.amount;
        return 0f;
    }

    /// <summary>Suma (o resta, con delta negativo) al stock de un elemento; nunca baja de 0.</summary>
    public void Add(string symbol, float delta)
    {
        foreach (ElementAmount r in reserves)
            if (r != null && r.symbol == symbol) { r.amount = Mathf.Max(0f, r.amount + delta); return; }
        if (delta > 0f) reserves.Add(new ElementAmount { symbol = symbol, amount = delta });
    }

    /// <summary>¿Hay reservas para pagar todos los costes del hechizo?</summary>
    public bool CanCast(IEnumerable<ElementCost> costs)
    {
        if (costs == null) return true;
        foreach (ElementCost c in costs)
            if (c != null && Get(c.symbol) < c.amount) return false;
        return true;
    }

    /// <summary>Paga el coste del hechizo si hay reservas (todo o nada). Devuelve si se pudo lanzar.</summary>
    public bool Pay(IEnumerable<ElementCost> costs)
    {
        if (!CanCast(costs)) return false;
        if (costs != null)
            foreach (ElementCost c in costs)
                if (c != null) Add(c.symbol, -c.amount);
        return true;
    }

    /// <summary>Paga MATERIA (elementos) **y** ENERGÍA (julios) juntos, todo o nada. Todo hechizo cuesta
    /// ambos: los elementos que consume la reacción + la energía de activación/canalización (y los hechizos
    /// grandes —nuclear/masa-energía— pesan sobre todo en energía). Devuelve si se pudo lanzar.</summary>
    public bool Pay(IEnumerable<ElementCost> costs, float energyJoules)
    {
        if (!CanCast(costs)) return false;
        if (energyJoules > 0f && energy < energyJoules) return false;
        Pay(costs);
        if (energyJoules > 0f) energy -= energyJoules;
        return true;
    }
}
