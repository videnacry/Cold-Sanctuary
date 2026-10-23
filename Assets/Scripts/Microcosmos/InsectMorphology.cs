using System.Collections.Generic;

/// <summary>
/// MORFOLOGÍA de los artrópodos del microcosmos, tal cual docs/insect-morphology.md: cada segmento con su largo y ancho
/// como fracción del largo del cuerpo L, las patas (5 segmentos en insectos, 7 en arañas) y sus rasgos (antena acodada,
/// mandíbulas, alas, patas raptoras o saltadoras…). CubeInsect construye el cuerpo de cubos a partir de estos datos.
/// </summary>
public static class InsectMorphology
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
        public Spec MemberwiseCloneSpec() => (Spec)MemberwiseClone();
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

    // Pata de araña: fémur / patela / tibia / metatarso / tarso = 0,28 / 0,10 / 0,24 / 0,24 / 0,10 de su largo total.
    static float[] Sp(float total) => new[] { 0.28f * total, 0.10f * total, 0.24f * total, 0.24f * total, 0.10f * total };

    /// <summary>La ficha de una especie (por nombre de prefab/especie; tolera «SnowSpider», «Mariquita», etc.).</summary>
    public static Spec Named(string n) => _all.TryGetValue(n, out Spec s) ? s : null;

    /// <summary>Copia de la ficha SIN alas ni élitros (las NINFAS de los hemimetábolos).</summary>
    public static Spec Wingless(Spec s) { var c = (Spec)s.MemberwiseCloneSpec(); c.wings = false; c.beeWings = false; return c; }

    public static Spec For(string species)
    {
        if (string.IsNullOrEmpty(species)) return _all["Ant"];
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
