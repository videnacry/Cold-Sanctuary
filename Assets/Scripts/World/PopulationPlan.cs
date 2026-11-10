using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// EQUILIBRIO DE ESPECIES (docs/ice-sanctuary-ecology.md; docs/reglas-animales.md §Fauna por humanos): es un SANTUARIO de animales, no una ciudad, así
/// que la fauna se dimensiona con los HUMANOS que viven en él. (Berón 2026-11-09) El total de animales de un santuario = K × sus humanos
/// (<see cref="AnimalsPerHuman"/>), repartido por la pirámide trófica (<see cref="LevelShare"/>: muchas más presas y herbívoros que depredadores) y,
/// dentro de cada nivel, por el peso de cada especie (<see cref="PerHuman"/>). Cada especie tiene su mínimo viable (2; el zorro ártico 4). La
/// reproducción no pasa de lo que le toca a cada especie ni del total. Sustituye a la escala fija de 11 (FaunaScale).
/// </summary>
public static class PopulationPlan
{
    /// <summary>EL PESO de cada especie DENTRO DE SU NIVEL trófico (antes: individuos por humano). La cadena alimenticia (<see cref="FoodChain"/>) puede
    /// subir el de una presa para que sus depredadores coman.</summary>
    public static readonly Dictionary<string, float> PerHuman = new Dictionary<string, float>
    {
        { "Bear", 0.3f }, { "PolarBear", 0.3f }, { "Penguin", 1.2f }, { "Seal", 0.6f }, { "Whale", 0.2f }, { "Orca", 0.1f },
        { "Fox", 0.2f }, { "Wolf", 0.3f },
        // Territorio polar (docs/reglas-animales.md): en la isla, más herbívoros que depredadores.
        { "SnowPetrel", 0.55f }, { "ArcticTern", 0.2f }, { "WanderingAlbatross", 0.2f }, { "CapePetrel", 0.2f }, { "PolarSkua", 0.2f }, { "Walrus", 0.3f }, { "EmperorPenguin", 0.8f }, { "Reindeer", 0.6f }, { "MuskOx", 0.3f }, { "ArcticHare", 0.5f }, { "ArcticFox", 0.2f },
    };

    /// <summary>Quién vive en el santuario 1 y en cuántos grupos (lo reparte el constructor). Una especie nueva entra aquí y la cadena alimenticia
    /// (<see cref="FoodChain"/>) la evalúa sola: nada más hace falta para que nadie pase hambre.</summary>
    public static readonly Dictionary<string, int> Groups = new Dictionary<string, int>
    {
        { "Bear", 1 }, { "Seal", 3 }, { "Whale", 2 }, { "Penguin", 2 }, { "Orca", 1 },
        { "SnowPetrel", 1 }, { "Walrus", 2 }, { "EmperorPenguin", 2 }, { "Reindeer", 2 }, { "MuskOx", 1 }, { "ArcticHare", 2 }, { "ArcticFox", 1 }, { "Wolf", 1 },
    };

    /// <summary>Personas en juego (comidas, verdura, prendas). La jornada puede subirlo. Ya no escala la fauna: la escala con <see cref="HumansIn"/>.</summary>
    public static int Humans = 11;

    /// <summary>(Berón 2026-11-09, rendimiento) INSECTOS EN EL SANTUARIO 1: por ahora no (abejas y arañas de las nieves, mariquitas, sus
    /// colmenas y enjambres de abejas). Un solo interruptor para que vuelvan (<see cref="InsectPurge"/>). Los peces, el kril y las medusas
    /// (también Swarm) no son insectos y alimentan a la fauna marina: se quedan.</summary>
    public static bool InsectsInSanctuary1 = false;

    /// <summary>(2026-11-08) LAS PERSONAS QUE VIVEN EN EL SANTUARIO según su lista de personal (<see cref="WorkdayDirector.Staff"/>), más la Magnate y el
    /// recién llegado: 32. No depende de quién está ya iniciado: el O₂ (<see cref="FotobioFacade"/>) y la verdura (<see cref="VerticalFarm"/>) se dimensionan
    /// para todos, al construir y en juego.</summary>
    public static int PlannedPeople => WorkdayDirector.Staff.Length + 2;
    /// <summary>Lo mayor de la plantilla en juego (<see cref="Humans"/>) y la planeada.</summary>
    public static int People => Mathf.Max(Humans, PlannedPeople);

    // ── LA FAUNA POR HUMANOS (Berón 2026-11-09) ─────────────────────────────────────────────────────────────────────────────────────
    /// <summary>K: ANIMALES POR HUMANO. El total de animales de un santuario = K × los humanos que viven en él. Por defecto 3; Berón puede subirlo a 4 o 5.
    /// Memoria: con 3 y 27 humanos, 81 animales en el santuario 1 (hoy viven ~86; docs/hielo-y-respiraderos.md).</summary>
    public static int AnimalsPerHuman = 3;

    /// <summary>Los niveles tróficos: presas y herbívoros, mesodepredadores y depredadores ápice.</summary>
    public enum Trophic { Prey, Meso, Apex }

    /// <summary>LA PIRÁMIDE, en número de individuos: 70 % presas y herbívoros, 22 % mesodepredadores, 8 % ápice (~9 presas por depredador ápice). Sale de la
    /// eficiencia de ~10 % entre niveles (Lindeman 1942; Pauly y Christensen 1995) y de la densidad de los carnívoros (Carbone y Gittleman 2002: 10 000 kg de
    /// presa sostienen ~90 kg de una especie carnívora), pasada a individuos con presas más pequeñas que sus cazadores. Cuentas: docs/reglas-animales.md.</summary>
    public static readonly Dictionary<Trophic, float> LevelShare = new Dictionary<Trophic, float> { { Trophic.Prey, 0.70f }, { Trophic.Meso, 0.22f }, { Trophic.Apex, 0.08f } };

    /// <summary>El nivel de cada especie (lo que no está aquí cuenta como mesodepredador).</summary>
    public static readonly Dictionary<string, Trophic> Level = new Dictionary<string, Trophic>
    {
        { "Penguin", Trophic.Prey }, { "EmperorPenguin", Trophic.Prey }, { "SnowPetrel", Trophic.Prey }, { "Reindeer", Trophic.Prey }, { "MuskOx", Trophic.Prey }, { "ArcticHare", Trophic.Prey },
        { "CapePetrel", Trophic.Prey }, { "ArcticTern", Trophic.Prey }, { "WanderingAlbatross", Trophic.Prey },
        { "Seal", Trophic.Meso }, { "Walrus", Trophic.Meso }, { "Whale", Trophic.Meso }, { "ArcticFox", Trophic.Meso }, { "Fox", Trophic.Meso }, { "PolarSkua", Trophic.Meso },
        { "Bear", Trophic.Apex }, { "PolarBear", Trophic.Apex }, { "Wolf", Trophic.Apex }, { "Orca", Trophic.Apex },
    };
    public static Trophic LevelOf(string species) => species != null && Level.TryGetValue(species, out Trophic t) ? t : Trophic.Meso;

    /// <summary>EL GRUPO MÍNIMO VIABLE de las especies que su parte deja demasiado cortas: el zorro ártico, 4 (Berón 2026-11-09). Las demás, 2 (una pareja).</summary>
    public static readonly Dictionary<string, int> MinViable = new Dictionary<string, int> { { "ArcticFox", 4 } };
    public static int MinOf(string species) => species != null && MinViable.TryGetValue(species, out int m) ? m : 2;

    /// <summary>El total de animales para <paramref name="humans"/> humanos: K × humanos.</summary>
    public static int TotalFor(int humans) => Mathf.Max(0, AnimalsPerHuman * humans);

    /// <summary>LOS HUMANOS QUE VIVEN en un santuario: los de la jornada, por su casa de hoy; los demás de la lista (y sin jornada, todos), por su casa de inicio
    /// o su poder (<see cref="SanctuaryPlacement"/>).</summary>
    // (los humanos se cuentan una vez por reparto de la jornada: la cuenta pasa por el poder de cada uno)
    static readonly Dictionary<int, int> _humans = new Dictionary<int, int>(); static int _humansKey = int.MinValue;
    public static int HumansIn(int sanctuary)
    {
        WorkdayDirector wd0 = WorkdayDirector.Instance;
        int key = (wd0 != null && wd0.today != null ? wd0.plannedDay * 64 + wd0.roster.workers.Count : -1) * 8 + AnimalsPerHuman;
        if (key != _humansKey) { _humans.Clear(); _humansKey = key; }
        if (!_humans.TryGetValue(sanctuary, out int cached)) _humans[sanctuary] = cached = CountHumans(sanctuary);
        return cached;
    }

    static int CountHumans(int sanctuary)
    {
        WorkdayDirector wd = WorkdayDirector.Instance;
        var bosses = SanctuaryPower.Bosses();
        int n = 0;
        foreach (string name in WorkdayDirector.Staff)
        {
            WorkRoster.Worker w = wd != null && wd.today != null ? wd.roster.Find(name) : null;
            int pos = w != null ? w.home : SanctuaryPlacement.Next(false, 0, false, SanctuaryPlacement.StartingHome.TryGetValue(name, out int st) ? st : 0, SanctuaryPower.OfHuman(null, name), bosses);
            if (SanctuaryPlacement.Home.At(pos).sanctuary == sanctuary) n++;
        }
        return n;
    }

    /// <summary>La parte de cada especie (suman 1): la de su nivel, repartida dentro del nivel por su peso (<see cref="PerHuman"/>). Si falta un nivel, los
    /// demás se reparten su parte.</summary>
    public static Dictionary<string, float> Shares(IEnumerable<string> species)
    {
        var list = new List<string>(species);
        var weightOf = new Dictionary<Trophic, float>();
        foreach (string s in list) { Trophic t = LevelOf(s); weightOf[t] = (weightOf.TryGetValue(t, out float x) ? x : 0f) + Weight(s); }
        float levels = 0f; foreach (Trophic t in weightOf.Keys) levels += LevelShare[t];
        var r = new Dictionary<string, float>();
        foreach (string s in list) { Trophic t = LevelOf(s); r[s] = weightOf[t] > 0f && levels > 0f ? LevelShare[t] / levels * Weight(s) / weightOf[t] : 0f; }
        return r;
    }
    static float Weight(string s) => PerHuman.TryGetValue(s, out float w) ? Mathf.Max(0.01f, w) : 0.2f;

    /// <summary>REPARTE <paramref name="total"/> animales entre las especies: cada una su parte, con su mínimo viable, y la suma exacta. Si el total no da para
    /// los mínimos de todas, salen las más raras de los depredadores (primero las del ápice), sin pasarse nunca del total; <paramref name="dropped"/> dice cuáles.</summary>
    public static Dictionary<string, int> Allocate(int total, IEnumerable<string> species, out List<string> dropped)
    {
        var list = new List<string>(species);
        dropped = new List<string>();
        var shares = Shares(list);
        var predators = list.Where(s => LevelOf(s) != Trophic.Prey).OrderBy(s => LevelOf(s) == Trophic.Apex ? 0 : 1).ThenBy(s => shares[s]).ThenBy(s => s, System.StringComparer.Ordinal).ToList();
        while (list.Sum(MinOf) > total && predators.Count > 0) { string gone = predators[0]; predators.RemoveAt(0); list.Remove(gone); dropped.Add(gone); }
        var r = new Dictionary<string, int>();
        foreach (string s in dropped) r[s] = 0;
        if (list.Count == 0 || list.Sum(MinOf) > total) { foreach (string s in list) r[s] = 0; return r; }
        shares = Shares(list);
        foreach (string s in list) r[s] = Mathf.Max(MinOf(s), Mathf.FloorToInt(shares[s] * total));
        for (int guard = 0; guard < 1000 && list.Sum(s => r[s]) > total; guard++)
        {
            string s = list.Where(x => r[x] > MinOf(x)).OrderByDescending(x => r[x] - shares[x] * total).FirstOrDefault();
            if (s == null) break;
            r[s]--;
        }
        for (int guard = 0; guard < 1000 && list.Sum(s => r[s]) < total; guard++)
            r[list.OrderByDescending(x => shares[x] * total - r[x]).ThenBy(x => x, System.StringComparer.Ordinal).First()]++;
        return r;
    }

    /// <summary>El reparto del santuario 1 (la fauna de esta escena): K × sus humanos entre las especies de <see cref="Groups"/>. Los santuarios 2 y 3 tienen su
    /// total (<see cref="TotalFor"/> × <see cref="HumansIn"/>), pero hoy sus escenas no tienen fauna propia que repartir.</summary>
    static Dictionary<string, int> _land; static string _landKey;
    public static Dictionary<string, int> LandAllocation()
    {
        int total = TotalFor(HumansIn(SanctuaryPlacement.Land));
        string key = total + "|" + string.Join(",", Groups.Keys.Select(s => $"{s}:{Weight(s):0.###}"));   // (cambia si cambian los humanos, K, las especies o sus pesos)
        if (_land == null || key != _landKey) { _land = Allocate(total, Groups.Keys, out _); _landKey = key; }
        return _land;
    }

    /// <summary>Cuántos individuos le tocan a la especie (pasa antes por la cadena alimenticia, que ajusta los pesos de las presas).</summary>
    public static int Target(string species) { FoodChain.Evaluate(); return Raw(species); }

    /// <summary><see cref="Target"/> sin pasar por la cadena (la usa la propia cadena): su parte de K × los humanos del santuario 1. Una especie que no vive
    /// en él (fuera de <see cref="Groups"/>) no tiene plan aquí: 0.</summary>
    public static int Raw(string species) => species != null && Groups.ContainsKey(species) && LandAllocation().TryGetValue(species, out int n) ? n : 0;

    /// <summary>Los que comen de verdad: el plan más los que viven en los icebergs (fijos: no crían; <see cref="IcebergVisitor"/>).</summary>
    public static int Census(string species) => Raw(species) + (species == "Seal" ? IcebergVisitor.Seals : species == "Penguin" ? IcebergVisitor.Penguins : 0);

    /// <summary>Techo de la reproducción: lo que le toca con K (antes, 1,5× el plan viejo); 0 = sin plan para esa especie. Y el de todos, <see cref="TotalCap"/>.</summary>
    public static int Cap(string species) => Target(species);

    /// <summary>El techo de todos los animales del santuario 1: K × sus humanos.</summary>
    public static int TotalCap => TotalFor(HumansIn(SanctuaryPlacement.Land));

    /// <summary>Reparte <see cref="Target"/> entre <paramref name="groups"/> grupos (los primeros, uno más si no es exacto).</summary>
    public static int[] Split(string species, int groups)
    {
        int t = Target(species); groups = Mathf.Max(1, groups);
        var r = new int[groups];
        for (int i = 0; i < groups; i++) r[i] = t / groups + (i < t % groups ? 1 : 0);
        return r;
    }
}
