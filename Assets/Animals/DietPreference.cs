using System.Collections.Generic;

/// <summary>
/// PREFERENCIA DE DIETA (docs/microcosmos-sakshi-origin.md §5, apremios §8) — el "sabor": cuánto APETECE una especie
/// de presa a un depredador. Hoy `Forager` solo tenía flags gruesos (`eatsPrey`/`eatsGrass`/`eatsFish`), que dicen QUÉ
/// TIPO come pero no CUÁL prefiere entre varias presas. La depredación por stats elige por *facilidad + distancia*
/// (`Forager.SelectPrey`); esto añade el eje de **apetencia por especie** (forrajeo óptimo real: especialistas vs
/// generalistas). Peso 1 = neutro; &gt;1 preferida; &lt;1 desdeñada. Desconocido → 1 (neutro).
///
/// Ejemplos: la mariquita es **especialista en pulgón**; la araña desdeña los élitros duros de la mariquita; un
/// depredador de hormigas **prefiere hormiga a gusano** (por eso, en el nivel de Sakshi, va a por ella y no a por Kushal).
/// </summary>
public static class DietPreference
{
    static Dictionary<string, Dictionary<string, float>> _catalog;

    /// <summary>Apetencia del <paramref name="predator"/> por la presa <paramref name="prey"/> (1 = neutro).</summary>
    public static float For(string predator, string prey)
    {
        if (string.IsNullOrEmpty(predator) || string.IsNullOrEmpty(prey)) return 1f;
        if (_catalog == null) Build();
        return _catalog.TryGetValue(predator, out Dictionary<string, float> m) && m.TryGetValue(prey, out float w) ? w : 1f;
    }

    /// <summary>¿Tiene esta especie una preferencia DECLARADA por esa presa/planta? (sin entrada = no la considera comida)</summary>
    public static bool Has(string predator, string prey)
    {
        if (string.IsNullOrEmpty(predator) || string.IsNullOrEmpty(prey)) return false;
        if (_catalog == null) Build();
        return _catalog.TryGetValue(predator, out Dictionary<string, float> m) && m.ContainsKey(prey);
    }

    static void Build()
    {
        _catalog = new Dictionary<string, Dictionary<string, float>>
        {
            // Insectos del Microcosmos (cadena real): preferencias entre sus presas.
            // Mariquita: especialista en pulgón (también cochinillas y ácaros); un grillo le queda grande → casi nunca.
            { "Ladybug", new Dictionary<string, float> { { "Aphid", 3.0f }, { "Cricket", 0.05f } } },
            { "Spider",  new Dictionary<string, float> { { "Aphid", 2.0f }, { "Cricket", 2.0f }, { "Ant", 1.0f }, { "Ladybug", 0.6f }, { "SnowBee", 2.5f },
                                                         { "Cockroach", 1.5f }, { "Woodlouse", 0.5f }, { "Firefly", 0.1f }, { "VelvetMite", 0.1f } } }, // élitros duros = menos; la araña de las nieves caza abejas de las nieves; las arañas rechazan luciérnagas
            // Mirmecofilia: la hormiga CUIDA y ordeña al pulgón (melaza, HoneydewProducer), no lo caza. Un peso alto aquí la
            // hacía preferirlo como PRESA en Forager.SelectPrey; casi nulo = solo si no queda nada más.
            { "Ant",     new Dictionary<string, float> { { "Aphid", 0.05f } } },
            { "Cricket", new Dictionary<string, float>() },                                            // omnívoro: neutro

            // Especies de las eras (docs/era-species-behavior.md). La luciérnaga (lucibufaginas) y el ácaro de terciopelo
            // (aposemático) saben mal: casi nadie los quiere; la cochinilla y el caracol acorazados, poco.
            { "Scorpion",    new Dictionary<string, float> { { "Cricket", 2f }, { "Cockroach", 2f }, { "Termite", 2f }, { "Grasshopper", 1.5f },
                                                             { "Earwig", 1.2f }, { "Woodlouse", 0.6f }, { "Scorpion", 0.4f }, { "Firefly", 0.1f }, { "VelvetMite", 0.1f } } },   // insectívoro; canibalismo ocasional (ADW)
            { "CamelSpider", new Dictionary<string, float> { { "Termite", 2.5f }, { "Cricket", 2f }, { "Grasshopper", 2f }, { "Cockroach", 1.5f },
                                                             { "Scorpion", 1f }, { "Firefly", 0.1f }, { "VelvetMite", 0.1f } } },   // termitas y lo que pille (Punzo 1998)
            { "GhostCrab",   new Dictionary<string, float> { { "Cricket", 1f }, { "Cockroach", 1f }, { "Earwig", 1f }, { "Woodlouse", 0.8f }, { "Ant", 0.5f } } },
            { "Firefly",     new Dictionary<string, float> { { "Snail", 3f }, { "Slug", 3f }, { "Earthworm", 1.5f } } },   // larva malacófaga
            { "VelvetMite",  new Dictionary<string, float> { { "Aphid", 2.5f }, { "Ant", 0.3f } } },
            { "Earwig",      new Dictionary<string, float> { { "Aphid", 3f }, { "Ladybug", 0.2f } } },   // gran depredadora de pulgones

            // Depredadores de HORMIGAS (p.ej. el escarabajo del anillo / el depredador débil del nivel de Sakshi):
            // prefieren HORMIGA a GUSANO → van a por Sakshi (hormiga) antes que a por Kushal (gusano).
            { "Escarabajo", new Dictionary<string, float> { { "Ant", 3.0f }, { "Gusano", 0.3f }, { "Worm", 0.3f } } },
            { "Depredador", new Dictionary<string, float> { { "Ant", 3.0f }, { "Gusano", 0.3f }, { "Worm", 0.3f } } },

            // HERBÍVOROS del santuario y los CULTIVOS del huerto (GardenCrop.CropName). La liebre ártica (Bunny) adora las hojas
            // tiernas y la zanahoria y desdeña las aromáticas y aliáceas (por eso proteger con albahaca/cebolla funciona);
            // el reno (Deer) busca grano y forraje.
            { "Bunny", new Dictionary<string, float> { { "zanahoria", 3f }, { "col", 2.5f }, { "espinaca", 2.5f }, { "lenteja", 2f }, { "garbanzo", 1.5f },
                                                       { "cilantro", 1.2f }, { "maní", 1.5f }, { "soja", 2f }, { "albahaca", 0.4f }, { "cebolla", 0.2f },
                                                       { "ajo", 0.1f }, { "cebollín", 0.3f }, { "chile", 0.1f }, { "huacatay", 0.3f } } },
            { "Deer",  new Dictionary<string, float> { { "avena", 2.5f }, { "maíz", 2.5f }, { "arroz", 1.5f }, { "quinoa", 1.5f }, { "teff", 1.5f },
                                                       { "col", 1.5f }, { "lenteja", 1.5f }, { "calabaza", 1.2f }, { "tomate", 0.5f }, { "cebolla", 0.2f },
                                                       { "ajo", 0.1f }, { "chile", 0.1f } } },
        };
    }
}
