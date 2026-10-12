using UnityEngine;

/// <summary>
/// ESTANTE a abastecer (misiones de INVENTARIO, docs/sanctuary1 §4): tiene un nº `filled` de `target`. Abastecerlo es un
/// HECHIZO (<see cref="StockSpell"/>) con rango, aprendible y enseñable (como la limpieza). Sin tope duro en el mundo
/// (la producción es infinita); `target` es el nivel que la misión quiere mantener. Ponlo en un GameObject-estante.
/// </summary>
public class StockShelf : MonoBehaviour
{
    public string label = "estante";
    [Min(0)] public int filled = 0;
    [Min(1)] public int target = 6;

    public bool Needs => filled < target;

    /// <summary>Abastece una unidad; devuelve true si cabía (quedaba por llenar).</summary>
    public bool StockOne()
    {
        if (filled >= target) return false;
        filled++;
        Debug.Log($"[Inventario] «{name}»: +1 ({filled}/{target}).");
        return true;
    }
}
