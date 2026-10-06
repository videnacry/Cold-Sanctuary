using UnityEngine;

/// <summary>
/// RECETA de un platillo del restaurante (docs/kitchen-simulation.md, spell-genesis §... cocina jugable): un nombre +
/// una **secuencia de pasos** (ingredientes/acciones) que el cocinero recorre. Data pura (el repo solo versiona `.cs`,
/// así que los platillos viven en código como presets, no como assets). No se COME: se **prepara para vender** → el
/// platillo listo se deposita en el stock del restaurante (<see cref="FoodContainer"/>) para que los personajes compren.
/// </summary>
[System.Serializable]
public class CookingRecipe
{
    public string dishName = "platillo";
    public string[] steps = System.Array.Empty<string>();

    public CookingRecipe() { }
    public CookingRecipe(string name, params string[] steps) { dishName = name; this.steps = steps; }

    // ── Presets (los platillos que pidió el autor) ──────────────────────────────────────────────
    public static CookingRecipe HuevosRevueltos() => new CookingRecipe("Huevos revueltos",
        "ve a la nevera", "toma los huevos", "calienta la plancha", "revuelve los huevos",
        "especia con sal y pimienta", "emplata");

    public static CookingRecipe Ensalada() => new CookingRecipe("Ensalada",
        "toma tomates, cebollas, limón, sal, pimienta, aceite y cilantro", "lava los vegetales",
        "corta los vegetales", "mezcla todo en el bol", "condimenta con sal, pimienta, limón y aceite", "emplata");

    public static CookingRecipe Avena() => new CookingRecipe("Avena",
        "toma leche, canela, avena, soy-milk, chocolate en polvo, banana y guanábana",
        "hierve los dos tipos de leche en ollas distintas", "corta las bananas y guanábanas",
        "agrega avena, canela y chocolate a las ollas", "mezcla", "agrega la fruta al final", "emplata");

    /// <summary>Devuelve el preset cuyo nombre coincide (ignora may/min); si no, null.</summary>
    public static CookingRecipe Preset(string dishName)
    {
        if (string.IsNullOrEmpty(dishName)) return null;
        string n = dishName.Trim().ToLowerInvariant();
        if (n.Contains("huevo")) return HuevosRevueltos();
        if (n.Contains("ensalada")) return Ensalada();
        if (n.Contains("avena")) return Avena();
        return null;
    }
}
