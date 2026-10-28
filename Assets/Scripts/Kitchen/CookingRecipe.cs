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
    [System.NonSerialized] public string id;   // id de catálogo (dish:…) → ingredientes/utensilios y prerequisito 'clean'

    public CookingRecipe() { }
    public CookingRecipe(string name, params string[] steps) { dishName = name; this.steps = steps; }
    CookingRecipe Id(string id) { this.id = id; return this; }

    // ── RACIONES: por mayor (el personal) o individual (turistas) ──────────────────────────────────────────────
    // Berón 2026-10-28: el restaurante prepara de DOS maneras: POR MAYOR para el personal del santuario (para que coman
    // todos: las cantidades de cada ingrediente se multiplican y salen varias raciones de una tanda) e INDIVIDUAL para los
    // turistas que llegan (una ración). Por defecto, individual (1): nada de lo que ya cocinaba cambia.
    [Tooltip("Raciones que salen de UNA tanda: 1 = individual (turistas); N = por mayor (el personal).")]
    [Min(1)] public int servings = 1;
    public bool IsBulk => servings > 1;

    /// <summary>Una COPIA de la receta para <paramref name="n"/> raciones (por mayor si n &gt; 1). El preset no cambia.</summary>
    public CookingRecipe ForServings(int n) =>
        new CookingRecipe(dishName, (string[])steps.Clone()) { id = id, servings = Mathf.Max(1, n) };

    /// <summary>Cantidad de cada ingrediente para UNA ración (las recetas base; las demás cuentan «1 ración» por ingrediente).</summary>
    static readonly System.Collections.Generic.Dictionary<string, (float qty, string unit)> PerServing =
        new System.Collections.Generic.Dictionary<string, (float, string)>
    {
        // Huevos revueltos: 2 huevos por ración, una pizca de sal y de pimienta.
        ["huevos"] = (2f, "u"), ["sal"] = (1f, "g"), ["pimienta"] = (0.5f, "g"),
        // Ensalada (ración de guarnición ~250 g).
        ["tomates"] = (150f, "g"), ["cebollas"] = (40f, "g"), ["limón"] = (0.25f, "u"), ["aceite"] = (10f, "ml"), ["cilantro"] = (5f, "g"),
        // Avena (porridge: 40 g de copos en 240 ml de líquido, mitad leche y mitad bebida de soja).
        ["avena"] = (40f, "g"), ["leche"] = (120f, "ml"), ["soy-milk"] = (120f, "ml"), ["canela"] = (1f, "g"),
        ["chocolate en polvo"] = (5f, "g"), ["banana"] = (0.5f, "u"), ["guanábana"] = (50f, "g"),
    };

    /// <summary>Los ingredientes de la receta: lo que se «toma» en sus pasos («toma tomates, cebollas y limón»).</summary>
    public System.Collections.Generic.List<string> Ingredients()
    {
        var l = new System.Collections.Generic.List<string>();
        foreach (string s in steps)
        {
            string t = s.Trim().ToLowerInvariant();
            if (!t.StartsWith("toma ")) continue;
            foreach (string part in t.Substring(5).Replace(" y ", ",").Split(','))
            {
                string p = part.Trim();
                if (p.StartsWith("los ")) p = p.Substring(4); else if (p.StartsWith("las ")) p = p.Substring(4);
                else if (p.StartsWith("el ")) p = p.Substring(3); else if (p.StartsWith("la ")) p = p.Substring(3);
                if (p.Length > 0 && !l.Contains(p)) l.Add(p);
            }
        }
        return l;
    }

    /// <summary>Cuánto hace falta de <paramref name="ingredient"/> para TODAS las raciones de esta tanda.</summary>
    public float Amount(string ingredient, out string unit)
    {
        string k = (ingredient ?? "").Trim().ToLowerInvariant();
        if (PerServing.TryGetValue(k, out var q)) { unit = q.unit; return q.qty * servings; }
        unit = "ración"; return servings;
    }
    public float Amount(string ingredient) => Amount(ingredient, out _);

    /// <summary>La lista de la compra de esta tanda («huevos 20 u · sal 10 g…»).</summary>
    public string Quantities()
    {
        var parts = new System.Collections.Generic.List<string>();
        foreach (string i in Ingredients()) { float a = Amount(i, out string u); parts.Add($"{i} {a:0.##} {u}"); }
        return string.Join(" · ", parts);
    }

    /// <summary>Ingredientes/utensilios que necesita (del catálogo). Vacío si la receta no está catalogada aún.</summary>
    public System.Collections.Generic.IEnumerable<string> RequiredItems => SpellCatalog.Items(id ?? "");

    /// <summary>Lo que FALTA para poder cocinarla, dados los items disponibles (ingredientes/utensilios presentes).</summary>
    public System.Collections.Generic.List<string> MissingItems(System.Collections.Generic.ICollection<string> available)
    {
        var missing = new System.Collections.Generic.List<string>();
        foreach (string it in RequiredItems) if (available == null || !available.Contains(it)) missing.Add(it);
        return missing;
    }

    // ── Presets (los platillos que pidió el autor) ──────────────────────────────────────────────
    public static CookingRecipe HuevosRevueltos() => new CookingRecipe("Huevos revueltos",
        "ve a la nevera", "toma los huevos", "calienta la plancha", "revuelve los huevos",
        "especia con sal y pimienta", "emplata").Id("dish:huevos-revueltos");

    public static CookingRecipe Ensalada() => new CookingRecipe("Ensalada",
        "toma tomates, cebollas, limón, sal, pimienta, aceite y cilantro", "lava los vegetales",
        "corta los vegetales", "mezcla todo en el bol", "condimenta con sal, pimienta, limón y aceite", "emplata").Id("dish:ensalada");

    public static CookingRecipe Avena() => new CookingRecipe("Avena",
        "toma leche, canela, avena, soy-milk, chocolate en polvo, banana y guanábana",
        "hierve los dos tipos de leche en ollas distintas", "corta las bananas y guanábanas",
        "agrega avena, canela y chocolate a las ollas", "mezcla", "agrega la fruta al final", "emplata").Id("dish:avena");

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

    // ── Platillos "bomba nutricional", uno por cultura (nivel de dificultad siguiente; ~6-8 pasos c/u) ───────
    // Principio: fermentado (probióticos) + legumbre+cereal (proteína completa) + hoja verde + grasa buena + especia.
    public static CookingRecipe DalIndio() => new CookingRecipe("Dal indio",
        "toma lentejas, arroz, espinaca, cúrcuma, jengibre y ajo", "sofríe el tadka (ajo/jengibre/cúrcuma)",
        "cuece las lentejas (dal)", "cuece el arroz", "saltea la espinaca (saag)", "une dal y saag", "sirve con arroz");
    public static CookingRecipe MilpaMexicana() => new CookingRecipe("Milpa mexicana",
        "toma frijol, maíz nixtamalizado, nopal, amaranto y chile", "prepara la masa de maíz", "cuece los frijoles",
        "asa y pica el nopal", "tuesta el amaranto", "haz la salsa de chile", "arma el plato de milpa");
    public static CookingRecipe InjeraEtiope() => new CookingRecipe("Injera con misir wot",
        "toma teff fermentado, lentejas rojas, cebolla y berbere", "vierte y cuece la injera de teff",
        "sofríe la cebolla con berbere", "cuece las lentejas (misir wot)", "liga el guiso", "enrolla con la injera", "emplata");
    public static CookingRecipe BibimbapCoreano() => new CookingRecipe("Bibimbap",
        "toma arroz, kimchi, espinaca, zanahoria, huevo y gochujang", "cuece el arroz", "saltea las verduras por separado",
        "fríe el huevo", "dispón todo sobre el arroz", "añade kimchi y gochujang", "mezcla al servir");
    public static CookingRecipe BowlJapones() => new CookingRecipe("Bowl japonés (natto/miso)",
        "toma arroz, natto, miso, edamame, alga y pescado azul", "cuece el arroz", "disuelve el miso en caldo dashi",
        "remueve el natto", "cuece el edamame", "corta pescado y alga", "monta el bowl");
    public static CookingRecipe MezzeMediterraneo() => new CookingRecipe("Mezze mediterráneo",
        "toma garbanzos, quinoa, verduras, aceite de oliva y yogur", "cuece la quinoa", "haz el hummus de garbanzo",
        "asa las verduras", "bate el yogur con hierbas", "aliña con aceite de oliva", "compón el mezze");
    public static CookingRecipe CurryTailandes() => new CookingRecipe("Curry verde tailandés",
        "toma tofu, pasta de curry verde, leche de coco, berenjena y albahaca", "sofríe la pasta de curry",
        "añade la leche de coco", "cuece tofu y berenjena", "ajusta dulce/salado/ácido", "añade albahaca", "sirve con arroz");
    public static CookingRecipe TagineMarroqui() => new CookingRecipe("Tajín marroquí",
        "toma garbanzos, verduras, dátiles, ras el hanout y almendra", "sofríe la cebolla con ras el hanout",
        "añade verduras y garbanzos", "agrega dátiles y caldo", "guisa a fuego lento en el tajín", "tuesta la almendra", "emplata");
    public static CookingRecipe GadoGadoIndonesio() => new CookingRecipe("Gado-gado",
        "toma tempeh, verduras variadas, huevo, maní y lima", "hierve/vaporiza las verduras", "fríe el tempeh",
        "cuece el huevo", "haz la salsa de maní", "monta las verduras", "napa con salsa de maní");
    public static CookingRecipe FeijoadaBrasil() => new CookingRecipe("Feijoada vegetal",
        "toma frijol negro, verduras, naranja, arroz y col", "cuece el frijol negro", "sofríe las verduras ahumadas",
        "guisa junto al frijol", "cuece el arroz", "saltea la col (couve)", "sirve con gajos de naranja");
    public static CookingRecipe PhoVietnamita() => new CookingRecipe("Pho vegetal",
        "toma fideo de arroz, caldo de especias, tofu, brotes y hierbas", "tuesta jengibre/canela/anís para el caldo",
        "infusiona el caldo", "hidrata el fideo", "escalda tofu y brotes", "monta el bol", "corona con hierbas y lima");
    public static CookingRecipe MercimekTurco() => new CookingRecipe("Mercimek (sopa turca de lenteja)",
        "toma lenteja roja, zanahoria, cebolla, comino y limón", "sofríe cebolla y zanahoria", "añade la lenteja y agua",
        "cuece hasta deshacer", "tritura la sopa", "sazona con comino", "sirve con limón");
    public static CookingRecipe TarwiPeruano() => new CookingRecipe("Guiso andino (tarwi/quinua)",
        "toma tarwi desamargado, quinua, choclo, ají y huacatay", "enjuaga la quinua y cuécela", "cuece el choclo",
        "haz el aderezo de ají", "integra el tarwi", "perfuma con huacatay", "emplata");
    public static CookingRecipe CongeeChino() => new CookingRecipe("Congee con tofu",
        "toma arroz, tofu, jengibre, cebollín, shiitake y soja", "hidrata los shiitake", "cuece el arroz hasta papilla",
        "saltea tofu y jengibre", "integra el shiitake", "sazona con soja", "corona con cebollín");
    public static CookingRecipe MoiMoiNigeriano() => new CookingRecipe("Moi moi (pastel de frijol)",
        "toma frijol caupí, pimiento, cebolla, aceite y especias", "remoja y pela el frijol", "licúa con pimiento y cebolla",
        "sazona la pasta", "vierte en moldes", "cuece al vapor", "desmolda y sirve");
    public static CookingRecipe LentejasEspanolas() => new CookingRecipe("Lentejas a la española",
        "toma lentejas, verduras, pimentón, laurel y aceite", "sofríe el refrito de verduras", "añade pimentón y laurel",
        "incorpora las lentejas y agua", "guisa a fuego lento", "rectifica el sazón", "sirve con un hilo de aceite");

    static CookingRecipe[] _all;
    public static CookingRecipe[] All => _all ?? (_all = new[]
    {
        HuevosRevueltos(), Ensalada(), Avena(), YogurConFruta(), BowlQuinoa(), Menestra(),
        DalIndio(), MilpaMexicana(), InjeraEtiope(), BibimbapCoreano(), BowlJapones(), MezzeMediterraneo(),
        CurryTailandes(), TagineMarroqui(), GadoGadoIndonesio(), FeijoadaBrasil(), PhoVietnamita(),
        MercimekTurco(), TarwiPeruano(), CongeeChino(), MoiMoiNigeriano(), LentejasEspanolas()
    });

    /// <summary>Devuelve el preset cuyo nombre coincide (contiene, ignora may/min); si no, null.</summary>
    public static CookingRecipe Preset(string dishName)
    {
        if (string.IsNullOrEmpty(dishName)) return null;
        string n = dishName.Trim().ToLowerInvariant();
        foreach (CookingRecipe r in All)
            if (r.dishName.ToLowerInvariant().Contains(n) || n.Contains(r.dishName.ToLowerInvariant())) return r;
        return null;
    }
}
