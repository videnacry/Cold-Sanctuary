using System.Collections.Generic;

/// <summary>
/// MORFOLOGÍA de los artrópodos del microcosmos, tal cual docs/insect-morphology.md: cada segmento con su largo y ancho
/// como fracción del largo del cuerpo L, las patas (5 segmentos en insectos, 7 en arañas) y sus rasgos (antena acodada,
/// mandíbulas, alas, patas raptoras o saltadoras…). CubeInsect construye el cuerpo de cubos a partir de estos datos.
/// </summary>
public static partial class InsectMorphology
{
    /// <summary>Un segmento del eje del cuerpo, de delante (cabeza) hacia atrás.</summary>
    public struct Seg { public string name; public float len, wid, hei; public Seg(string n, float l, float w, float h = -1f) { name = n; len = l; wid = w; hei = h < 0f ? w : h; } }

    public class Spec
    {
        public string name;
        public Seg[] body;                       // cabeza → … → abdomen
        public float[][] legs;                   // por par: largos (fémur, tibia, tarso) o, en arañas, (fémur, patela, tibia, metatarso, tarso)
        public float legWidth = 0.03f, coxa = 0.05f, troch = 0.03f;
        public bool spider;                      // 8 patas de 7 segmentos; quelíceros y pedipalpos
        public float antenna = 0.3f;             // largo total de cada antena (0 = sin antenas)
        public bool elbowedAntenna;              // escapo + codo + funículo (hormigas, abejas)
        public float mandible;                   // largo de cada mandíbula (0 = sin)
        public float mandibleOpen = 10f;         // apertura en reposo (°)
        public bool raptorial, jumper, elytra, beeWings, wings, siphons, stylet, backward;
        public float bodyHeight = 0.12f;         // altura del vientre sobre el suelo (en L)
        public int armPairs = 1;                 // pares delanteros que son BRAZOS al erguirse (mantis: 2 → anda a dos patas, con 4 brazos)
        // TAMAÑO DE CADA FASE respecto al adulto (largo del cuerpo; la larva madura, la pupa y la ninfa de último estadio).
        public float larvaScale = 0.8f, pupaScale = 0.9f, nymphScale = 0.6f, juvenileScale = 0.5f;   // (juvenil: los de desarrollo directo)
        // DIMORFISMO SEXUAL (ForSex): cuerno del macho, oviscapto de la hembra, bulbos de los pedipalpos del macho de araña.
        public bool horn, palpBulbs;
        public float ovipositor;                 // largo del oviscapto (en L; 0 = sin)
        public FaunaTraits fx;                   // piezas de la fauna de cada era (escorpión, cangrejo, caracol…; InsectMorphology.Fauna.cs)
        public Spec MemberwiseCloneSpec() => (Spec)MemberwiseClone();
    }

    /// <summary>
    /// La ficha de la especie para su SEXO (Berón, 2026-10-23). Rasgos de manual, exagerados lo justo para verse en cubos:
    /// - Hormiga: la obrera (hembra) es la ficha; el MACHO es alado (dos pares), de cabeza pequeña y mandíbulas cortas.
    /// - Pulgón: la hembra áptera (partenogénica) es la ficha; el macho, alado y de abdomen menor (sale en otoño).
    /// - Pelotero: el macho lleva CUERNO en la cabeza (Onthophagus). Mariquita: casi iguales (la hembra algo mayor).
    /// - Saltamontes: la hembra, ~20 % mayor y con las valvas del oviscapto; el macho, menor.
    /// - Grillo: la hembra lleva un OVISCAPTO largo como una aguja; el macho canta (tiene el aparato en las alas).
    /// - Mantis: la hembra mucho más robusta (abdomen ancho); el macho esbelto, de antenas más largas.
    /// - Termita: las obreras son machos y hembras iguales (sin dimorfismo visible); solo la reina engorda (fisogastria).
    /// - Abeja: la obrera (hembra) es la ficha; el ZÁNGANO (macho), más robusto y de cabeza mayor.
    /// - Araña lobo: la hembra, de abdomen grande; el macho, menor y con BULBOS en los pedipalpos (órganos copuladores).
    /// </summary>
    public static Spec ForSex(Spec s, bool female)
    {
        if (s == null) return null;
        var c = s.MemberwiseCloneSpec();
        c.body = (Seg[])s.body.Clone();
        void Scale(string seg, float len, float wid) { for (int i = 0; i < c.body.Length; i++) if (c.body[i].name == seg) { c.body[i].len *= len; c.body[i].wid *= wid; c.body[i].hei *= wid; } }
        switch (s.name)
        {
            case "Ant": if (!female) { c.beeWings = true; c.mandible *= 0.5f; Scale("Cabeza", 0.75f, 0.75f); Scale("Gaster", 0.9f, 0.85f); } break;
            case "Aphid": if (!female) { c.beeWings = true; Scale("Abdomen", 0.7f, 0.75f); } break;
            case "DungBeetle": if (!female) c.horn = true; break;
            case "Ladybug": if (female) Scale("Elitros", 1.05f, 1.05f); break;
            case "Grasshopper": if (female) { Scale("Abdomen", 1.2f, 1.15f); c.ovipositor = 0.06f; } else Scale("Abdomen", 0.9f, 0.9f); break;
            case "Cricket": if (female) c.ovipositor = 0.5f; break;
            case "Mantis": if (female) Scale("Abdomen", 1.1f, 1.4f); else { Scale("Abdomen", 0.9f, 0.75f); c.antenna *= 1.4f; } break;
            case "SnowBee": if (!female) { Scale("Cabeza", 1.15f, 1.15f); Scale("Abdomen", 0.95f, 1.1f); } break;
            case "Spider": if (female) Scale("Abdomen", 1.2f, 1.2f); else { Scale("Abdomen", 0.8f, 0.8f); c.palpBulbs = true; } break;
            default: FaunaForSex(c, female, Scale); break;   // las especies de cada era (InsectMorphology.Fauna.cs)
        }
        return c;
    }

    static readonly Dictionary<string, Spec> _all = new Dictionary<string, Spec>
    {
        ["Ant"] = new Spec { name = "Ant", body = new[] { new Seg("Cabeza", 0.22f, 0.18f, 0.16f), new Seg("Torax", 0.30f, 0.12f), new Seg("Peciolo", 0.06f, 0.06f), new Seg("Gaster", 0.36f, 0.22f, 0.2f) },
            legs = new[] { new[] { 0.22f, 0.20f, 0.18f }, new[] { 0.24f, 0.22f, 0.18f }, new[] { 0.30f, 0.28f, 0.22f } }, antenna = 0.44f, elbowedAntenna = true, mandible = 0.08f },
        ["Aphid"] = new Spec { name = "Aphid", body = new[] { new Seg("Cabeza", 0.14f, 0.16f), new Seg("Torax", 0.24f, 0.3f, 0.28f), new Seg("Abdomen", 0.62f, 0.55f, 0.42f) },
            legs = new[] { new[] { 0.22f, 0.30f, 0.06f }, new[] { 0.22f, 0.30f, 0.06f }, new[] { 0.22f, 0.30f, 0.06f } }, legWidth = 0.02f, antenna = 0.7f, siphons = true, stylet = true, bodyHeight = 0.14f },
        ["Ladybug"] = new Spec { name = "Ladybug", body = new[] { new Seg("Cabeza", 0.15f, 0.30f, 0.2f), new Seg("Pronoto", 0.18f, 0.55f, 0.3f), new Seg("Elitros", 0.70f, 0.90f, 0.45f) },
            legs = new[] { new[] { 0.18f, 0.16f, 0.10f }, new[] { 0.18f, 0.16f, 0.10f }, new[] { 0.18f, 0.16f, 0.10f } }, antenna = 0.15f, elytra = true, bodyHeight = 0.06f },
        ["DungBeetle"] = new Spec { name = "DungBeetle", body = new[] { new Seg("Cabeza", 0.18f, 0.45f, 0.18f), new Seg("Pronoto", 0.28f, 0.60f, 0.35f), new Seg("Elitros", 0.54f, 0.58f, 0.4f) },
            legs = new[] { new[] { 0.20f, 0.24f, 0.04f }, new[] { 0.22f, 0.22f, 0.10f }, new[] { 0.30f, 0.35f, 0.12f } }, legWidth = 0.045f, antenna = 0.12f, elytra = true, bodyHeight = 0.12f },
        ["Grasshopper"] = new Spec { name = "Grasshopper", body = new[] { new Seg("Cabeza", 0.15f, 0.15f, 0.17f), new Seg("Torax", 0.25f, 0.16f, 0.18f), new Seg("Abdomen", 0.60f, 0.13f) },
            legs = new[] { new[] { 0.15f, 0.14f, 0.08f }, new[] { 0.15f, 0.14f, 0.08f }, new[] { 0.50f, 0.48f, 0.15f } }, antenna = 0.35f, jumper = true, wings = true, bodyHeight = 0.12f },
        ["Cricket"] = new Spec { name = "Cricket", body = new[] { new Seg("Cabeza", 0.16f, 0.16f), new Seg("Torax", 0.24f, 0.18f), new Seg("Abdomen", 0.60f, 0.18f) },
            legs = new[] { new[] { 0.15f, 0.14f, 0.08f }, new[] { 0.15f, 0.14f, 0.08f }, new[] { 0.65f, 0.70f, 0.15f } }, antenna = 1.2f, jumper = true, wings = true, bodyHeight = 0.12f },
        ["Mantis"] = new Spec { name = "Mantis", body = new[] { new Seg("Cabeza", 0.08f, 0.14f, 0.1f), new Seg("Protorax", 0.30f, 0.06f), new Seg("Abdomen", 0.62f, 0.12f) },
            legs = new[] { new[] { 0.22f, 0.12f, 0.04f }, new[] { 0.22f, 0.22f, 0.10f }, new[] { 0.22f, 0.22f, 0.10f } }, legWidth = 0.02f, antenna = 0.3f, raptorial = true, wings = true, bodyHeight = 0.16f, armPairs = 2 },
        ["Antlion"] = new Spec { name = "Antlion", body = new[] { new Seg("Cabeza", 0.14f, 0.18f, 0.08f), new Seg("Protorax", 0.08f, 0.08f), new Seg("Abdomen", 0.62f, 0.48f, 0.3f) },
            legs = new[] { new[] { 0.12f, 0.10f, 0.04f }, new[] { 0.12f, 0.10f, 0.04f }, new[] { 0.12f, 0.10f, 0.04f } }, legWidth = 0.025f, antenna = 0f, mandible = 0.30f, mandibleOpen = 60f, backward = true, bodyHeight = 0.04f },
        ["Termite"] = new Spec { name = "Termite", body = new[] { new Seg("Cabeza", 0.20f, 0.18f), new Seg("Torax", 0.22f, 0.14f), new Seg("Abdomen", 0.58f, 0.22f) },
            legs = new[] { new[] { 0.18f, 0.18f, 0.08f }, new[] { 0.18f, 0.18f, 0.08f }, new[] { 0.18f, 0.18f, 0.08f } }, legWidth = 0.025f, antenna = 0.35f, mandible = 0.05f },
        ["SnowBee"] = new Spec { name = "SnowBee", body = new[] { new Seg("Cabeza", 0.18f, 0.24f), new Seg("Torax", 0.32f, 0.38f), new Seg("Abdomen", 0.50f, 0.40f) },
            legs = new[] { new[] { 0.22f, 0.25f, 0.20f }, new[] { 0.22f, 0.25f, 0.20f }, new[] { 0.22f, 0.25f, 0.20f } }, legWidth = 0.04f, antenna = 0.30f, elbowedAntenna = true, beeWings = true },
        // Hormiga león ADULTA (alada, como un caballito del diablo): abdomen largo y fino, alas plegadas en tejado, antenas en maza.
        ["AntlionAdult"] = new Spec { name = "AntlionAdult", body = new[] { new Seg("Cabeza", 0.07f, 0.09f), new Seg("Torax", 0.13f, 0.09f), new Seg("Abdomen", 0.80f, 0.05f) },
            legs = new[] { new[] { 0.10f, 0.10f, 0.05f }, new[] { 0.10f, 0.10f, 0.05f }, new[] { 0.12f, 0.12f, 0.05f } }, legWidth = 0.015f, antenna = 0.18f, wings = true, bodyHeight = 0.08f },
        ["Spider"] = new Spec { name = "Spider", spider = true, body = new[] { new Seg("Cefalotorax", 0.45f, 0.38f, 0.3f), new Seg("Abdomen", 0.55f, 0.45f, 0.4f) },
            legs = new[] { Sp(1.4f), Sp(1.2f), Sp(1.0f), Sp(1.3f) }, antenna = 0f, bodyHeight = 0.18f },
    };

    // TAMAÑO DE LAS FASES respecto al adulto (Berón 2026-10-24: «las ninfas, pupas y larvas deben tener tamaños relativos a
    // los adultos»). Larva madura, pupa y ninfa de último estadio; [A] = aproximación de manual:
    // - mariquita: la larva de 4.º estadio mide 10–11 mm y el adulto 6–8 (la larva ES mayor); pelotero: la larva en C ~1,2×;
    // - hormiga: la larva madura ≈ la obrera; abeja: la larva sellada ≈ 0,8×; hormiga león: la larva 12–15 mm frente a
    //   ~35 mm del adulto alado, y el capullo ~10 mm; ninfas de último estadio ~0,6–0,8× del adulto. [A]
    static InsectMorphology()
    {
        void S(string n, float larva, float pupa, float nymph) { if (_all.TryGetValue(n, out Spec s)) { s.larvaScale = larva; s.pupaScale = pupa; s.nymphScale = nymph; } }
        S("Ant", 0.9f, 1f, 0.6f); S("Ladybug", 1.4f, 1f, 0.6f); S("DungBeetle", 1.2f, 1f, 0.6f); S("SnowBee", 0.8f, 1f, 0.6f);
        S("Antlion", 0.4f, 0.3f, 0.6f);
        S("Grasshopper", 0.8f, 0.9f, 0.65f); S("Cricket", 0.8f, 0.9f, 0.65f); S("Mantis", 0.8f, 0.9f, 0.6f); S("Aphid", 0.8f, 0.9f, 0.7f); S("Termite", 0.8f, 0.9f, 0.8f);
    }

    /// <summary>Largo de la fase <paramref name="phase"/> respecto al adulto (1 = adulto).</summary>
    public static float PhaseScale(string species, Metamorphosis.Phase phase)
    {
        Spec s = For(species);
        return phase == Metamorphosis.Phase.Larva ? s.larvaScale : phase == Metamorphosis.Phase.Pupa ? s.pupaScale : phase == Metamorphosis.Phase.Nymph ? s.nymphScale : phase == Metamorphosis.Phase.Juvenile ? s.juvenileScale : 1f;
    }

    // Pata de araña: fémur / patela / tibia / metatarso / tarso = 0,28 / 0,10 / 0,24 / 0,24 / 0,10 de su largo total.
    static float[] Sp(float total) => new[] { 0.28f * total, 0.10f * total, 0.24f * total, 0.24f * total, 0.10f * total };

    /// <summary>La ficha de una especie (por nombre de prefab/especie; tolera «SnowSpider», «Mariquita», etc.).</summary>
    public static Spec Named(string n) => n == null ? null : _all.TryGetValue(n, out Spec s) ? s : _fauna.TryGetValue(n, out s) ? s : null;

    /// <summary>Copia de la ficha SIN alas ni élitros (las NINFAS de los hemimetábolos).</summary>
    public static Spec Wingless(Spec s) { var c = (Spec)s.MemberwiseCloneSpec(); c.wings = false; c.beeWings = false; return c; }

    public static Spec For(string species)
    {
        if (string.IsNullOrEmpty(species)) return _all["Ant"];
        if (_fauna.TryGetValue(species, out Spec era)) return era;   // nombre exacto primero: «CamelSpider» no es una araña, «Scorpionfly» no es un escorpión
        string s = species.ToLowerInvariant();
        if (s.Contains("spider") || s.Contains("ara")) return _all["Spider"];
        if (s.Contains("antlion") || s.Contains("león") || s.Contains("leon")) return _all["Antlion"];
        if (s.Contains("aphid") || s.Contains("pulg") || s.Contains("ambrosio")) return _all["Aphid"];
        if (s.Contains("ladybug") || s.Contains("mariquita")) return _all["Ladybug"];
        if (s.Contains("dung") || s.Contains("beetle") || s.Contains("pelotero") || s.Contains("escarabajo")) return _all["DungBeetle"];
        if (s.Contains("grasshopper") || s.Contains("saltamontes")) return _all["Grasshopper"];
        if (s.Contains("cricket") || s.Contains("grillo")) return _all["Cricket"];
        if (s.Contains("mantis")) return _all["Mantis"];
        if (s.Contains("termite") || s.Contains("termita")) return _all["Termite"];
        if (s.Contains("bee") || s.Contains("abeja")) return _all["SnowBee"];
        return _all["Ant"];
    }
}
