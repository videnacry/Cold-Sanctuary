using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// LA CADENA ALIMENTICIA (Berón 2026-11-05; docs/reglas-animales.md, regla E15): «evalúa a los animales existentes y su alimentación para
/// equilibrar el número de poblaciones y también puede incrementar el número de plantas o peces o crustáceos […] para que nadie muera de hambre».
///
/// Lee lo que ya hay, sin una tabla aparte de quién come qué: <see cref="PopulationPlan.Groups"/> (quién vive en el santuario),
/// <see cref="Forager"/> y <see cref="Ethogram"/> (qué come cada uno), <see cref="Archetypes.RelationValue"/> (quién teme a quién: sus presas) y
/// <see cref="Reproduction.Calendar"/> (cuánto cría). Cuenta en kg por AÑO DEL SANTUARIO (28 días de juego): un animal come su ración de un día real
/// cada día de juego, y sus presas crían un año real en 28 días. Por eso salen números jugables sin inventar ninguno:
///   · Lo que necesita: <see cref="Intake"/> × 28 × individuos (menos lo que le lleva el alimentador: <see cref="LabShare"/>).
///   · Lo que da una presa: individuos × masa × (crías al año por hembra × ½ hembras × ½ de la masa adulta, porque se come sobre todo a los jóvenes).
///   · Lo que da un banco o una planta: lo que rebrota en ese año (ver <see cref="BankYield"/> y <see cref="PlantYield"/>).
/// Con eso reparte el hambre de cada consumidor entre sus fuentes según lo que ofrece cada una y, si una fuente no da para lo que le piden con
/// <see cref="Margin"/> de holgura, la SUBE: más presas (hasta ×<see cref="MaxRaise"/>), más bancos o más plantas. Vuelve a mirar, porque subir una presa la
/// hace comer más abajo. Se evalúa sola la primera vez que alguien pide una población (<see cref="PopulationPlan.Target"/>), así que una especie
/// nueva en <see cref="PopulationPlan.Groups"/> entra sola; y el Blockout escribe el informe (<c>[Cadena]</c>). La prueba es el grupo 85.
/// </summary>
public static class FoodChain
{
    public const float DaysPerYear = 28f, SecondsPerGameDay = 3600f;
    /// <summary>Una fuente solo puede dar este tanto de su producción: el resto mantiene la población.</summary>
    public const float Margin = 0.8f;
    /// <summary>Tope de subida de una población animal: más sería un santuario lleno de Animal completos (caros).</summary>
    public const float MaxRaise = 2f;
    public const int MaxBanks = 12;
    /// <summary>Un banco de peces y otro de kril por iceberg (Smith et al. 2007): además de los de la laguna, que son los que pide <see cref="Banks"/>.</summary>
    public const int BergBanks = 2;
    /// <summary>Por debajo de esto una especie pasa hambre (una presa muy menor que se agote no cuenta).</summary>
    public const float Fed = 0.9f;
    public const float MaxPlantFactor = 6f;

    /// <summary>Ración diaria real (kg de comida fresca). Fuentes en docs/reglas-animales.md §Cadena alimenticia.</summary>
    public static readonly Dictionary<string, float> Intake = new Dictionary<string, float>
    {
        { "Walrus", 60f },      // ~6 % de la masa (Born & Acquarone)
        { "Seal", 5f },         // foca: 3,4–9 kg (Nilssen; Markussen & Øritsland)
        { "Bear", 6f },         // 43 focas anilladas al año (Pagano et al. 2018: una adulta cada 10–12 días)
        { "Orca", 120f },       // ~3 % de la masa del catálogo (4–6 % Sergeant 1969; 2 % en cautividad)
        { "Whale", 65f },       // ~5 % de la masa del catálogo (Savoca et al. 2021: 5–30 % los días que come)
        { "PolarSkua", 0.25f }, // [A] o calculado por energía (docs/fauna-fria-santuarios.md)
        { "CapePetrel", 0.1f }, // [A] o calculado por energía (docs/fauna-fria-santuarios.md)
        { "WanderingAlbatross", 0.85f }, // [A] o calculado por energía (docs/fauna-fria-santuarios.md)
        { "ArcticTern", 0.03f }, // [A] o calculado por energía (docs/fauna-fria-santuarios.md)
        { "SnowPetrel", 0.06f }, // [A] ~20 % de su masa
        { "Penguin", 0.6f },    // Adelia: 0,55–0,65 kg
        { "EmperorPenguin", 3f }, // 1,8–6,3 kg (Robertson & Newgrain 1996)
        { "Wolf", 3f },         // 2,5–6 kg (Fuller 1989)
        { "ArcticFox", 0.25f }, // 530–624 kcal; la masa es una estimación [A]
        { "Reindeer", 3f },     // 1–3,7 kg de materia seca
        { "MuskOx", 3f },       // 2,6–2,8 kg de materia seca
        { "ArcticHare", 0.3f }, // 133–262 kcal; la masa es una estimación [A]
    };

    /// <summary>Lo que da el laboratorio al día para los depredadores de tierra: un lote por jornada (<see cref="CulturedMeatLab.batchGrams"/>).</summary>
    public const float LabKgPerDay = CulturedMeatLab.MachineryGrams / 1000f;
    /// <summary>Lo que necesitan al día los depredadores de tierra, según el plan (lo que se le pide al laboratorio: regla P19).</summary>
    public static float LandNeedKgPerDay { get { Evaluate(); return _landNeed / DaysPerYear; } }
    /// <summary>Parte de lo que necesitan los depredadores de tierra que cubre el alimentador: lo que da el laboratorio entre lo que necesitan todos.</summary>
    public static float LabShare { get; private set; }

    const string Fish = "peces", Krill = "kril", Clam = "bivalvos", Jelly = "medusas";

    public class Line
    {
        public string species;
        public int individuals;
        /// <summary>kg que necesita al año del santuario (sin lo que le dan).</summary>
        public float needs;
        /// <summary>De lo que necesita, la parte que encuentra (1 = nadie pasa hambre).</summary>
        public float covered;
        public List<string> sources = new List<string>();
    }

    static readonly Dictionary<SwarmKind, int> _banks = new Dictionary<SwarmKind, int> { { SwarmKind.Fish, 2 }, { SwarmKind.Krill, 2 }, { SwarmKind.Clam, 2 } };
    static readonly Dictionary<AffordanceKind, float> _plants = new Dictionary<AffordanceKind, float>();
    static readonly Dictionary<string, float> _base = new Dictionary<string, float>();
    static readonly Dictionary<string, List<string>> _diet = new Dictionary<string, List<string>>();
    static readonly List<string> _changes = new List<string>();
    static readonly HashSet<string> _land = new HashSet<string>();   // los que atiende el alimentador (PredatorRound.Served)
    static bool _done, _busy;
    static float _landNeed;
    static int _speciesSeen;

    /// <summary>Bancos del santuario que pide la cadena (peces, kril, bivalvos). El constructor pone esos.</summary>
    public static int Banks(SwarmKind k) { Evaluate(); return _banks.TryGetValue(k, out int n) ? n : 1; }

    /// <summary>Por cuánto multiplicar lo que da una planta comestible de la tundra (<see cref="PolarUnderstory"/>).</summary>
    public static float PlantFactor(AffordanceKind k) { Evaluate(); return _plants.TryGetValue(k, out float f) ? f : 1f; }

    // Lo que crece un banco en un año del santuario (Swarm.growthPerSecond, 0,05/s) en kg (Swarm.perUnitMass: el kril pesa 10 veces menos).
    static float BankYield(SwarmKind k) => 0.05f * SecondsPerGameDay * DaysPerYear * (k == SwarmKind.Krill ? 0.05f : k == SwarmKind.Clam ? 0.5f : 0.5f);

    // Puntos comestibles planeados en la tundra, y lo que rebrota cada uno al año (2 de existencias, 0,0005/s, ~¼ kg por unidad comida).
    static readonly Dictionary<AffordanceKind, int> PlannedSpots = new Dictionary<AffordanceKind, int>
    {
        { AffordanceKind.Grass, 120 }, { AffordanceKind.Willow, 90 }, { AffordanceKind.Lichen, 70 }, { AffordanceKind.Berry, 40 },
    };
    const float PlantYield = 2f * 0.0005f * SecondsPerGameDay * DaysPerYear * 0.25f;

    static float Crop(string prey)
    {
        var (_, interval) = Reproduction.Calendar(prey);
        return 12f / Mathf.Max(1f, interval) * 0.5f * 0.5f;   // crías/año por hembra × ½ hembras × ½ de la masa adulta
    }

    static float Mass(string sp) => Physiognomy.Of(sp) != null ? Physiognomy.Of(sp).baseMass : 40f;

    static List<string> DietOf(string sp, Forager f)
    {
        var d = new List<string>();
        f.ConfigureForSpecies(sp);
        if (f.eatsFish) d.Add(Fish);
        if (f.eatsKrill) d.Add(Krill);
        if (f.eatsClams) d.Add(Clam);
        if (sp == "Penguin") d.Add(Jelly);
        foreach (EthoAct e in Ethogram.Of(sp))
            if (Ethogram.Eats(sp, e.kind) && !d.Contains(e.kind.ToString())) d.Add(e.kind.ToString());
        if (f.eatsPrey)
            foreach (string prey in PopulationPlan.Groups.Keys)
                if (prey != sp && Archetypes.RelationValue(prey, sp) < 0f) d.Add(prey);
        return d;
    }

    // Lo que ofrece una fuente al año del santuario (0 = no existe en este santuario).
    static float Supply(string s, Dictionary<string, float> real)
    {
        if (real != null && real.TryGetValue(s, out float r)) return r;
        if (s == Fish) return (_banks[SwarmKind.Fish] + BergBanks) * BankYield(SwarmKind.Fish);
        if (s == Krill) return (_banks[SwarmKind.Krill] + BergBanks) * BankYield(SwarmKind.Krill);
        if (s == Clam) return _banks[SwarmKind.Clam] * BankYield(SwarmKind.Clam);
        if (s == Jelly) return 200f;
        if (System.Enum.TryParse(s, out AffordanceKind k) && PlannedSpots.TryGetValue(k, out int n))
            return n * PlantYield * (_plants.TryGetValue(k, out float f) ? f : 1f);
        return PopulationPlan.Groups.ContainsKey(s) ? PopulationPlan.Census(s) * Mass(s) * Crop(s) : 0f;
    }

    static float Need(string sp)
    {
        float kg = Intake.TryGetValue(sp, out float i) ? i : Mass(sp) * 0.05f;   // sin dato: 5 % de su masa [A]
        return PopulationPlan.Census(sp) * kg * DaysPerYear * (_land.Contains(sp) ? 1f - LabShare : 1f);
    }

    // Reparte cada hambre entre sus fuentes según lo que ofrecen. Devuelve lo que se le pide a cada fuente.
    static Dictionary<string, float> Demand(Dictionary<string, float> real)
    {
        var demand = new Dictionary<string, float>();
        foreach (string sp in PopulationPlan.Groups.Keys)
        {
            float total = 0f;
            foreach (string s in _diet[sp]) total += Supply(s, real);
            if (total <= 0f) continue;
            float need = Need(sp);
            foreach (string s in _diet[sp])
            {
                demand.TryGetValue(s, out float d);
                demand[s] = d + need * Supply(s, real) / total;
            }
        }
        return demand;
    }

    /// <summary>Evalúa la cadena y sube lo que haga falta. Solo vuelve a calcular si hay especies nuevas. No crea nada: dice cuánto.</summary>
    public static void Evaluate()
    {
        if (_busy || (_done && _speciesSeen == PopulationPlan.Groups.Count)) return;
        _busy = true;
        try
        {
            _speciesSeen = PopulationPlan.Groups.Count; _done = true; _changes.Clear();
            // Las dietas, de sus tablas. El Forager es de un objeto inactivo: no arranca nada.
            GameObject probe = new GameObject("FoodChain_probe") { hideFlags = HideFlags.HideAndDontSave };
            probe.SetActive(false);
            Forager f = probe.AddComponent<Forager>();
            foreach (string sp in PopulationPlan.Groups.Keys)
            {
                if (!PopulationPlan.PerHuman.ContainsKey(sp)) { Debug.LogWarning($"[Cadena] {sp} vive en el santuario pero no está en PopulationPlan.PerHuman."); PopulationPlan.PerHuman[sp] = 0.2f; }
                if (!_base.ContainsKey(sp)) _base[sp] = PopulationPlan.PerHuman[sp];
                f.eatsFish = f.eatsKrill = f.eatsClams = f.eatsPrey = f.eatsGrass = f.eatsPlants = false;
                _diet[sp] = DietOf(sp, f);
                if (f.eatsPrey && sp != "Orca") _land.Add(sp);
            }
            if (Application.isPlaying) Object.Destroy(probe); else Object.DestroyImmediate(probe);

            for (int pass = 0; pass < 12; pass++)
            {
                // El alimentador: lo que da el laboratorio se reparte entre todos los depredadores de tierra (cambia si suben las poblaciones).
                float land = 0f;
                foreach (string sp in _land) land += PopulationPlan.Raw(sp) * (Intake.TryGetValue(sp, out float i) ? i : Mass(sp) * 0.05f) * DaysPerYear;
                _landNeed = land;
                LabShare = land > 0f ? Mathf.Clamp01(LabKgPerDay * DaysPerYear / land) : 0f;
                // Primero bancos y plantas (no cuestan Animal completos); las presas, solo si con eso no basta.
                bool changed = false;
                Dictionary<string, float> demand = Demand(null);
                foreach (var kv in demand)
                {
                    if (PopulationPlan.Groups.ContainsKey(kv.Key)) continue;
                    float ratio = kv.Value / Mathf.Max(0.01f, Supply(kv.Key, null) * Margin);
                    if (ratio > 1f && Raise(kv.Key, ratio)) changed = true;
                }
                if (changed) continue;
                foreach (var kv in demand)
                {
                    if (!PopulationPlan.Groups.ContainsKey(kv.Key)) continue;
                    float ratio = kv.Value / Mathf.Max(0.01f, Supply(kv.Key, null) * Margin);
                    if (ratio > 1f && Raise(kv.Key, ratio)) changed = true;
                }
                if (!changed) break;
            }
        }
        finally { _busy = false; }
    }

    static bool Raise(string s, float ratio)
    {
        SwarmKind? bank = s == Fish ? SwarmKind.Fish : s == Krill ? SwarmKind.Krill : s == Clam ? SwarmKind.Clam : (SwarmKind?)null;
        if (bank != null)
        {
            int now = _banks[bank.Value], next = Mathf.Min(MaxBanks, Mathf.CeilToInt(now * ratio));
            if (next <= now) return false;
            _banks[bank.Value] = next; Note($"{s}: {now} → {next} bancos");
            return true;
        }
        if (System.Enum.TryParse(s, out AffordanceKind k))
        {
            float now = _plants.TryGetValue(k, out float f) ? f : 1f, next = Mathf.Min(MaxPlantFactor, now * ratio);
            if (next <= now + 0.001f) return false;
            _plants[k] = next; Note($"{k}: plantas ×{next:0.0}");
            return true;
        }
        if (!PopulationPlan.PerHuman.TryGetValue(s, out float per)) return false;
        float max = _base[s] * MaxRaise, up = Mathf.Min(max, per * ratio);
        if (up <= per + 0.001f) return false;
        PopulationPlan.PerHuman[s] = up; Note($"{s}: {_base[s]:0.00} → {up:0.00} por humano");
        return true;
    }

    static void Note(string what) { _changes.RemoveAll(c => c.StartsWith(what.Split(':')[0] + ":")); _changes.Add(what); }

    /// <summary>Cada especie: cuánto necesita y qué parte encuentra. <paramref name="real"/>: lo que hay de verdad en la escena, por fuente, en vez de
    /// lo planeado (la prueba cuenta los bancos y las plantas que existen).</summary>
    public static List<Line> Report(Dictionary<string, float> real = null)
    {
        Evaluate();
        var lines = new List<Line>();
        Dictionary<string, float> demand = Demand(real);
        foreach (string sp in PopulationPlan.Groups.Keys)
        {
            var l = new Line { species = sp, individuals = PopulationPlan.Census(sp), needs = Need(sp) };
            float total = 0f, got = 0f;
            foreach (string s in _diet[sp]) total += Supply(s, real);
            foreach (string s in _diet[sp])
            {
                float sup = Supply(s, real);
                if (sup <= 0f) continue;
                l.sources.Add(s);
                demand.TryGetValue(s, out float d);
                got += sup / total * Mathf.Min(1f, sup / Mathf.Max(0.01f, d));
            }
            l.covered = l.needs <= 0f ? 1f : got;
            lines.Add(l);
        }
        return lines;
    }

    /// <summary>Cuántos bancos de esta clase pide un santuario aparte con esta fauna, sin mezclarlo con el 1.</summary>
    public static int BanksFor(Dictionary<string, int> animals, SwarmKind kind)
    {
        float need = 0f;
        foreach (var (sp, n) in Pairs(animals))
        {
            bool fish = Eats(sp, SwarmKind.Fish), krill = Eats(sp, SwarmKind.Krill);
            if (!(kind == SwarmKind.Fish ? fish : krill)) continue;
            need += n * (Intake.TryGetValue(sp, out float i) ? i : 1f) * DaysPerYear / (fish && krill ? 2f : 1f);
        }
        return need <= 0f ? 0 : Mathf.Max(1, Mathf.CeilToInt(need / (BankYield(kind) * Margin)));
    }

    /// <summary>El balance de un santuario aparte (el aéreo), con los bancos de <see cref="BanksFor"/>: cada especie y lo que cubre.</summary>
    public static List<Line> Isolated(Dictionary<string, int> animals)
    {
        var lines = new List<Line>();
        foreach (var (sp, n) in Pairs(animals))
        {
            var l = new Line { species = sp, individuals = n, needs = n * (Intake.TryGetValue(sp, out float i) ? i : 1f) * DaysPerYear };
            float supply = 0f;
            foreach (SwarmKind k in new[] { SwarmKind.Fish, SwarmKind.Krill })
                if (Eats(sp, k)) { l.sources.Add(k.ToString()); supply += BanksFor(animals, k) * BankYield(k) * Margin; }
            l.covered = l.needs <= 0f ? 1f : Mathf.Min(1f, supply / l.needs);
            lines.Add(l);
        }
        return lines;
    }

    static IEnumerable<(string, int)> Pairs(Dictionary<string, int> d) { foreach (var kv in d) yield return (kv.Key, kv.Value); }

    static bool Eats(string sp, SwarmKind k)
    {
        GameObject probe = new GameObject("FoodChain_probe") { hideFlags = HideFlags.HideAndDontSave };
        probe.SetActive(false);
        Forager f = probe.AddComponent<Forager>(); f.ConfigureForSpecies(sp);
        bool eats = k == SwarmKind.Fish ? f.eatsFish : f.eatsKrill;
        if (Application.isPlaying) Object.Destroy(probe); else Object.DestroyImmediate(probe);
        return eats;
    }

    /// <summary>Lo que escribe el Blockout: una línea por especie y lo que tuvo que subir. Devuelve cuántas especies pasarían hambre.</summary>
    public static int Log()
    {
        List<Line> lines = Report();
        var sb = new StringBuilder("[Cadena] La cadena alimenticia del santuario (kg por año del santuario):\n");
        int hungry = 0;
        foreach (Line l in lines)
        {
            if (l.covered < Fed) hungry++;
            sb.AppendLine($"  {l.species} ×{l.individuals}: necesita {l.needs:0}, encuentra {l.covered:P0} ({string.Join(", ", l.sources)})");
        }
        sb.AppendLine($"  El alimentador cubre {LabShare:P0} de lo que necesitan los depredadores de tierra (laboratorio: {LabKgPerDay:0} kg al día).");
        sb.AppendLine(_changes.Count > 0 ? "  Subió: " + string.Join(" · ", _changes) : "  No hubo que subir nada.");
        if (hungry > 0) Debug.LogWarning(sb + $"  {hungry} especies pasarían hambre: revisar sus fuentes."); else Debug.Log(sb.ToString());
        return hungry;
    }
}
