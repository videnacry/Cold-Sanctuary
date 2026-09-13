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

    static void Build()
    {
        _catalog = new Dictionary<string, Dictionary<string, float>>
        {
            // Insectos del Microcosmos (cadena real): preferencias entre sus presas.
            { "Ladybug", new Dictionary<string, float> { { "Aphid", 3.0f }, { "Cricket", 0.6f } } },  // especialista en pulgón
            { "Spider",  new Dictionary<string, float> { { "Aphid", 2.0f }, { "Cricket", 2.0f }, { "Ant", 1.0f }, { "Ladybug", 0.6f } } }, // élitros duros = menos
            { "Ant",     new Dictionary<string, float> { { "Aphid", 1.5f } } },                        // mirmecofilia (cuida/ordeña pulgón)
            { "Cricket", new Dictionary<string, float>() },                                            // omnívoro: neutro

            // Depredadores de HORMIGAS (p.ej. el escarabajo del anillo / el depredador débil del nivel de Sakshi):
            // prefieren HORMIGA a GUSANO → van a por Sakshi (hormiga) antes que a por Kushal (gusano).
            { "Escarabajo", new Dictionary<string, float> { { "Ant", 3.0f }, { "Gusano", 0.3f }, { "Worm", 0.3f } } },
            { "Depredador", new Dictionary<string, float> { { "Ant", 3.0f }, { "Gusano", 0.3f }, { "Worm", 0.3f } } },
        };
    }
}
