using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EQUILIBRIO DE ESPECIES (docs/ice-sanctuary-ecology.md): es un SANTUARIO de animales, no una ciudad, así que la fauna
/// se dimensiona en proporción a los HUMANOS que viven en él (la base). Por cada humano: 0,3 osos, 1,2 pingüinos, 0,6
/// focas, 0,2 ballenas y 0,1 orcas (y 0,2 zorros/lobos donde los haya). Así carnívoros y presas quedan equilibrados y,
/// si llega más gente, el santuario puede acoger más animales. La reproducción respeta un techo de 1,5× ese objetivo.
/// </summary>
public static class PopulationPlan
{
    public static readonly Dictionary<string, float> PerHuman = new Dictionary<string, float>
    {
        { "Bear", 0.3f }, { "PolarBear", 0.3f }, { "Penguin", 1.2f }, { "Seal", 0.6f }, { "Whale", 0.2f }, { "Orca", 0.1f },
        { "Fox", 0.2f }, { "Wolf", 0.3f },
        // Territorio polar (docs/reglas-animales.md): en la isla, más herbívoros que depredadores.
        { "SnowPetrel", 0.55f }, { "ArcticTern", 0.2f }, { "WanderingAlbatross", 0.2f }, { "CapePetrel", 0.2f }, { "PolarSkua", 0.2f }, { "Walrus", 0.3f }, { "EmperorPenguin", 0.8f }, { "Reindeer", 0.6f }, { "MuskOx", 0.3f }, { "ArcticHare", 0.5f }, { "ArcticFox", 0.2f },
    };

    /// <summary>Quién vive en el santuario y en cuántos grupos (lo reparte el constructor). Una especie nueva entra aquí y la cadena alimenticia
    /// (<see cref="FoodChain"/>) la evalúa sola: nada más hace falta para que nadie pase hambre.</summary>
    public static readonly Dictionary<string, int> Groups = new Dictionary<string, int>
    {
        { "Bear", 1 }, { "Seal", 3 }, { "Whale", 2 }, { "Penguin", 2 }, { "Orca", 1 },
        { "SnowPetrel", 1 }, { "Walrus", 2 }, { "EmperorPenguin", 2 }, { "Reindeer", 2 }, { "MuskOx", 1 }, { "ArcticHare", 2 }, { "ArcticFox", 1 }, { "Wolf", 1 },
    };

    /// <summary>Humanos para escalar la fauna al construir: 11 (el equipo real son 30 o más; ver el comentario siguiente). La jornada puede subirlo en juego.</summary>
    // Escala de la fauna, no el censo de personas: atada a 11 para no triplicar animales al crecer el equipo (límite de memoria del PC).
    public static int Humans = 11;

    /// <summary>LA ESCALA DE LA FAUNA, fija: la fauna y su cadena alimenticia no siguen a la plantilla en Play (el techo de reproducción habría subido a 1,5 × 32, con 15 osos,
    /// y los bancos y plantas son los de la construcción). <see cref="Humans"/> cuenta personas (comidas, verdura, prendas). docs/hielo-y-respiraderos.md.</summary>
    public const int FaunaScale = 11;

    /// <summary>(2026-11-08) LAS PERSONAS QUE VIVEN EN EL SANTUARIO según su lista de personal (<see cref="WorkdayDirector.Staff"/>), más la Magnate y el
    /// recién llegado: 32. No depende de quién está ya iniciado: el O₂ (<see cref="FotobioFacade"/>) y la verdura (<see cref="VerticalFarm"/>) se dimensionan
    /// para todos, al construir y en juego.</summary>
    public static int PlannedPeople => WorkdayDirector.Staff.Length + 2;
    /// <summary>Lo mayor de la plantilla en juego (<see cref="Humans"/>) y la planeada.</summary>
    public static int People => Mathf.Max(Humans, PlannedPeople);

    /// <summary>Cuántos individuos le tocan a la especie (mínimo 2, para que pueda haber pareja).</summary>
    public static int Target(string species) { FoodChain.Evaluate(); return Raw(species); }

    /// <summary>(Berón 2026-11-09) EL GRUPO MÍNIMO VIABLE de las especies que su proporción deja demasiado cortas: el zorro ártico, 4 (con 11 × 0,2 salían 2).
    /// Las demás, el mínimo de 2 de siempre (una pareja).</summary>
    public static readonly Dictionary<string, int> MinViable = new Dictionary<string, int> { { "ArcticFox", 4 } };

    /// <summary><see cref="Target"/> sin pasar por la cadena (la usa la propia cadena).</summary>
    public static int Raw(string species) => PerHuman.TryGetValue(species, out float r) ? Mathf.Max(MinViable.TryGetValue(species, out int min) ? min : 2, Mathf.RoundToInt(FaunaScale * r)) : 0;

    /// <summary>Los que comen de verdad: el plan más los que viven en los icebergs (fijos: no crían; <see cref="IcebergVisitor"/>).</summary>
    public static int Census(string species) => Raw(species) + (species == "Seal" ? IcebergVisitor.Seals : species == "Penguin" ? IcebergVisitor.Penguins : 0);

    /// <summary>Techo para la reproducción (1,5× el objetivo); 0 = sin plan para esa especie.</summary>
    public static int Cap(string species) { int t = Target(species); return t > 0 ? Mathf.CeilToInt(t * 1.5f) : 0; }

    /// <summary>Reparte <see cref="Target"/> entre <paramref name="groups"/> grupos (los primeros, uno más si no es exacto).</summary>
    public static int[] Split(string species, int groups)
    {
        int t = Target(species); groups = Mathf.Max(1, groups);
        var r = new int[groups];
        for (int i = 0; i < groups; i++) r[i] = t / groups + (i < t % groups ? 1 : 0);
        return r;
    }
}
