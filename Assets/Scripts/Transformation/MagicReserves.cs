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

    [Header("Reservas por elemento (símbolo de la tabla periódica → cantidad). Agotado → no lanza ese hechizo.")]
    public List<ElementAmount> reserves = new List<ElementAmount>();

    void Awake() { if (anima == null) anima = GetComponent<Anima>(); }

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
}
