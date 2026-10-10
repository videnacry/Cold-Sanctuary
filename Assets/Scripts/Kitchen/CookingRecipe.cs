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

    public static CookingRecipe YogurConFruta() => new CookingRecipe("Yogur con fruta",
        "toma yogur/kéfir y semillas de lino", "pulveriza las semillas de lino", "lava 7 frutas",
        "prepara la sandía y el coco (corte especial)", "corta las demás frutas",
        "mezcla el yogur con la fruta", "espolvorea el lino y emplata");

    public static CookingRecipe BowlQuinoa() => new CookingRecipe("Bowl de quinoa",
        "toma quinoa, lentejas, piña y mango", "enjuaga y cuece la quinoa", "cuece las lentejas",
        "corta la piña y el mango", "mezcla quinoa y lentejas en el bowl", "añade la fruta y aliña", "emplata");

    public static CookingRecipe Menestra() => new CookingRecipe("Menestra",
        "toma arroz/quinoa y lenteja/garbanzo/frijol", "sofríe y prepara el caldo/salsa",
        "cuece las legumbres en el caldo", "cuece el arroz (o quinoa)", "mezcla legumbres y arroz",
        "rectifica el sazón", "emplata");

    /// <summary>Devuelve el preset cuyo nombre coincide (ignora may/min); si no, null.</summary>
    public static CookingRecipe Preset(string dishName)
    {
        if (string.IsNullOrEmpty(dishName)) return null;
        string n = dishName.Trim().ToLowerInvariant();
        if (n.Contains("huevo")) return HuevosRevueltos();
        if (n.Contains("ensalada")) return Ensalada();
        if (n.Contains("avena")) return Avena();
        if (n.Contains("yogur") || n.Contains("kéfir") || n.Contains("kefir")) return YogurConFruta();
        if (n.Contains("bowl") || n.Contains("quinoa")) return BowlQuinoa();
        if (n.Contains("menestra") || n.Contains("lenteja")) return Menestra();
        return null;
    }
}
