using System.Collections.Generic;

/// <summary>
/// FAUNA DE CADA ERA (Berón, 2026-10-24: «investiga la flora y fauna de cada era/lugar de los microcosmos y crea los
/// insectos-humanos para cada una»). Los artrópodos y demás invertebrados que faltaban en los biomas de
/// docs/microcosmos-biomas.md, con su forma real, su ciclo de vida y su dimorfismo sexual (fuentes en
/// docs/microcosmos-fauna-eras.md):
/// - ARÁCNIDOS: escorpión (cola de 5 segmentos con aguijón, pinzas, peines), solífugo (quelíceros enormes, pedipalpos
///   como patas con ventosa; el macho con flagelo), ácaro de terciopelo (larva de 6 patas → ninfa → adulto de 8).
/// - CRUSTÁCEOS: cochinilla (7 placas, la cría «manca» nace con 6 pares de patas; la hembra con marsupio), cangrejo
///   fantasma (pinzas desiguales, ojos en pedúnculo, corre de lado; larva zoea → megalopa → cangrejo).
/// - MOLUSCOS: caracol (concha en espiral con bandas; el labio solo en el adulto) y babosa (manto con pneumostoma).
/// - ANÉLIDOS: lombriz (anillos; el CLITELO solo en el adulto).
/// - INSECTOS: cigarra (ninfa excavadora años bajo tierra; el macho con timbales), luciérnaga (linterna: 2 segmentos
///   en el macho, 1 en la hembra; la larva también brilla), tijereta (fórceps curvo del macho, recto de la hembra),
///   cucaracha oriental (el macho con alas a 3/4 del abdomen, la hembra con muñones), pulga de nieve Boreus (pico largo;
///   el macho con alas en cerdas, la hembra con oviscapto en sable).
/// Los moluscos y la lombriz son HERMAFRODITAS: el mismo cuerpo para los dos «sexos» del juego.
/// </summary>
public static partial class InsectMorphology
{
    /// <summary>Las piezas propias de cada grupo (todas en fracción de L). CubeInsect (CubeInsect.Fauna.cs) las dibuja.</summary>
    public class FaunaTraits
    {
        // Dónde van las patas y la cintura del centauro (0 = como siempre): fracción del cuerpo desde la cabeza.
        public float legFrom, legTo, waist;
        // Alas: largo respecto al abdomen (1,05 = como siempre), transparentes con manchas (cigarra), en cerdas (Boreus macho), muñones.
        public float wingLen = 1.05f;
        public bool clearWings, wingSpots, bristleWings, wingPads;
        // ARÁCNIDOS Y CRUSTÁCEOS
        public bool crustacean;                  // sin quelíceros ni pedipalpos de araña
        public float chelae, chelaWidth = 0.04f, chelaAsym = 1f;   // pinzas (escorpión, cangrejo): largo, grosor y la derecha ×asym
        public float tailLen, tailWidth = 0.05f; // metasoma (5 segmentos) + telson con aguijón
        public float pectines;                   // peines ventrales del escorpión (más largos en el macho)
        public float bigChelicerae;              // solífugo: quelíceros como tenazas, hasta 1/3 del cuerpo
        public float palpLegs;                   // solífugo: pedipalpos como patas largas con ventosa (son sus brazos)
        public bool flagellum;                   // solífugo macho: el flagelo sobre los quelíceros
        public float eyeStalks;                  // cangrejo: ojos en pedúnculo (alto)
        public int curl;                         // defensa pasiva: 1 hacerse bola (cochinilla) · 2 meterse en la concha · 3 encogerse
        public float apron;                      // cangrejo: el abdomen plegado bajo el caparazón (ancho: la hembra ancho, el macho estrecho)
        public float uropods;                    // cochinilla: los urópodos de la cola
        public bool broodPouch;                  // cochinilla hembra: el marsupio (bolsa de cría)
        // MOLUSCOS Y ANÉLIDOS
        public float shell; public int whorls = 4; public bool shellLip, shellBands = true;   // caracol
        public float tentacles;                  // tentáculos con ojos (y los dos cortos de abajo)
        public bool mantle;                      // babosa: el manto con el pneumostoma (el orificio para respirar)
        public bool hermaphrodite;               // caracol, babosa, lombriz
        // INSECTOS
        public int lantern;                      // luciérnaga: segmentos luminosos (macho 2, hembra 1)
        public float forceps; public bool forcepsCurved;   // tijereta
        public float cerci;                      // cucaracha: cercos cortos
        public bool tymbal;                      // cigarra macho: los timbales y opérculos
        public float rostrum;                    // Boreus: el pico (rostro) hacia abajo
        public bool digger;                      // ninfa de cigarra: patas delanteras excavadoras
        public bool bigEyes;                     // ojos compuestos salientes a los lados (cigarra)
        public UnityEngine.Color accent = UnityEngine.Color.clear;   // 2.º color (el pronoto rosado de la luciérnaga, el cuerpo del caracol)
        // CICLO DE VIDA: la forma de cada fase (nombre de otra ficha), si no es la genérica.
        public string larva, nymph, juvenile;
        public bool larvaLegs, zoea;             // larva con patitas (luciérnaga, Boreus) · larva zoea del cangrejo (en el mar)
        public FaunaTraits Clone() => (FaunaTraits)MemberwiseClone();
    }

    static Seg S(string n, float l, float w, float h = -1f) => new Seg(n, l, w, h);
    static float[] I(float f, float t, float ta) => new[] { f, t, ta };

    static Seg[] Rings(bool clitellum)
    {
        // Lombriz: prostomio, 4 anillos, el CLITELO (segmentos 32–37 de ~150: a un cuarto del cuerpo) y la cola aplanada.
        var r = new List<Seg> { S("Prostomio", 0.04f, 0.05f, 0.045f) };
        for (int i = 1; i <= 4; i++) r.Add(S("Anillo_" + i, 0.05f, 0.06f, 0.055f));
        r.Add(clitellum ? S("Clitelo", 0.1f, 0.075f, 0.07f) : S("Anillo_5", 0.1f, 0.06f, 0.055f));
        for (int i = 6; i <= 18; i++) { float t = (i - 6) / 12f; r.Add(S("Anillo_" + i, 0.05f, UnityEngine.Mathf.Lerp(0.06f, 0.07f, t), UnityEngine.Mathf.Lerp(0.055f, 0.035f, t))); }
        return r.ToArray();
    }

    static Seg[] Tergites(float w)
    {
        // Escorpión: el prosoma (escudo con los ojos) y los 7 terguitos del mesosoma, más anchos en medio.
        var r = new List<Seg> { S("Prosoma", 0.14f, 0.14f, 0.07f) };
        for (int i = 1; i <= 7; i++) r.Add(S("Terguito_" + i, 0.043f, w * (0.82f + 0.18f * UnityEngine.Mathf.Sin(i / 8f * UnityEngine.Mathf.PI)), 0.07f));
        return r.ToArray();
    }

    static Seg[] Pereion(int plates)
    {
        // Cochinilla: cabeza, 7 placas del pereion (la manca, la 7.ª aún diminuta), 2 del pleon y el telson.
        float[] w = { 0.55f, 0.62f, 0.65f, 0.66f, 0.65f, 0.6f, 0.55f };
        var r = new List<Seg> { S("Cabeza", 0.1f, 0.3f, 0.08f) };
        for (int i = 0; i < 7; i++) r.Add(i < plates ? S("Pereion_" + (i + 1), 0.095f, w[i], 0.1f) : S("Pereion_" + (i + 1), 0.03f, w[i] * 0.8f, 0.07f));
        r.Add(S("Pleon_1", 0.05f, 0.4f, 0.07f)); r.Add(S("Pleon_2", 0.05f, 0.32f, 0.06f)); r.Add(S("Telson", 0.04f, 0.2f, 0.05f));
        return r.ToArray();
    }

    static readonly Dictionary<string, Spec> _fauna = new Dictionary<string, Spec>
    {
        // ── ARÁCNIDOS ──────────────────────────────────────────────────────────────────────────────────────────
        // Escorpión (Leiurus quinquestriatus 30–77 mm; Heterometrus, Mesobuthus): 4 pares de patas en el prosoma, pinzas
        // (pedipalpos) que son sus brazos, la cola (metasoma de 5 segmentos + telson con aguijón) curvada sobre el dorso.
        // Vivíparo: las crías nacen blancas y viajan sobre el dorso de la madre hasta la 1.ª muda.
        ["Scorpion"] = new Spec { name = "Scorpion", juvenileScale = 0.45f,  spider = true, body = Tergites(0.17f), armPairs = 0, antenna = 0f, bodyHeight = 0.06f, legWidth = 0.02f,
            legs = new[] { Sp(0.28f), Sp(0.32f), Sp(0.36f), Sp(0.40f) },
            fx = new FaunaTraits { chelae = 0.34f, chelaWidth = 0.035f, tailLen = 0.5f, tailWidth = 0.05f, pectines = 0.045f } },
        // Solífugo (Galeodes arabs 40–60 mm): quelíceros enormes (hasta 1/3 del cuerpo) y pedipalpos largos como patas,
        // con ventosa en la punta (con ellos sujeta y trepa: son sus brazos); anda con los 3 pares de atrás y tantea con el 1.º.
        ["CamelSpider"] = new Spec { name = "CamelSpider", juvenileScale = 0.4f,  spider = true, armPairs = 0, antenna = 0f, bodyHeight = 0.12f, legWidth = 0.025f,
            body = new[] { S("Propeltidio", 0.22f, 0.2f, 0.14f), S("Abdomen", 0.5f, 0.2f, 0.14f) },
            legs = new[] { Sp(0.55f), Sp(0.65f), Sp(0.75f), Sp(0.9f) },
            fx = new FaunaTraits { bigChelicerae = 0.25f, palpLegs = 0.75f } },
        // Ácaro de terciopelo rojo (Trombidium, 4–6 mm, «bir bahuti»): un solo cuerpo velludo (gnatosoma + idiosoma),
        // patas cortas y gruesas; el 1.er par tantea como antenas (son sus brazos al erguirse).
        ["VelvetMite"] = new Spec { name = "VelvetMite", larvaScale = 0.15f, nymphScale = 0.6f,  spider = true, antenna = 0f, bodyHeight = 0.05f, legWidth = 0.05f,
            body = new[] { S("Gnatosoma", 0.1f, 0.12f, 0.08f), S("Propodosoma", 0.3f, 0.6f, 0.4f), S("Histerosoma", 0.6f, 0.72f, 0.45f) },
            legs = new[] { Sp(0.55f), Sp(0.4f), Sp(0.4f), Sp(0.5f) },
            fx = new FaunaTraits { legFrom = 0.12f, legTo = 0.45f, waist = 0.4f, larva = "VelvetMiteLarva" } },
        // Larva del ácaro: 3 pares de patas, redonda; parásita de saltamontes y langostas (se les agarra a las alas).
        ["VelvetMiteLarva"] = new Spec { name = "VelvetMiteLarva", spider = true, antenna = 0f, bodyHeight = 0.05f, legWidth = 0.05f,
            body = new[] { S("Gnatosoma", 0.12f, 0.14f, 0.1f), S("Idiosoma", 0.88f, 0.8f, 0.55f) },
            legs = new[] { Sp(0.5f), Sp(0.42f), Sp(0.48f) },
            fx = new FaunaTraits { legFrom = 0.14f, legTo = 0.5f, waist = 0.45f } },

        // ── CRUSTÁCEOS ─────────────────────────────────────────────────────────────────────────────────────────
        // Cochinilla de humedad (Porcellio scaber 10–17 mm; Armadillidium vulgare se hace bola): 7 placas, 7 pares de patas,
        // antenas acodadas y urópodos. Crustáceo terrestre: la hembra lleva los huevos en un marsupio bajo el vientre.
        ["Woodlouse"] = new Spec { name = "Woodlouse", juvenileScale = 0.15f,  body = Pereion(7), antenna = 0.45f, elbowedAntenna = true, bodyHeight = 0.04f, legWidth = 0.02f,
            legs = new[] { I(0.1f, 0.1f, 0.06f), I(0.1f, 0.1f, 0.06f), I(0.1f, 0.1f, 0.06f), I(0.1f, 0.1f, 0.06f), I(0.1f, 0.11f, 0.06f), I(0.11f, 0.11f, 0.06f), I(0.11f, 0.12f, 0.06f) },
            fx = new FaunaTraits { legFrom = 0.12f, legTo = 0.78f, uropods = 0.1f, curl = 1, juvenile = "WoodlouseManca" } },
        // La MANCA (cría): sale del marsupio con 6 pares de patas y la 7.ª placa a medio hacer; el 7.º par aparece tras la 1.ª muda.
        ["WoodlouseManca"] = new Spec { name = "WoodlouseManca", body = Pereion(6), antenna = 0.4f, elbowedAntenna = true, bodyHeight = 0.04f, legWidth = 0.02f,
            legs = new[] { I(0.1f, 0.1f, 0.06f), I(0.1f, 0.1f, 0.06f), I(0.1f, 0.1f, 0.06f), I(0.1f, 0.1f, 0.06f), I(0.1f, 0.11f, 0.06f), I(0.11f, 0.11f, 0.06f) },
            fx = new FaunaTraits { legFrom = 0.13f, legTo = 0.75f, uropods = 0.08f, curl = 1 } },
        // Cangrejo fantasma (Ocypode quadrata, caparazón ~5 cm): caparazón casi cuadrado, ojos en pedúnculos largos (ven 360°),
        // pinzas DESIGUALES (son sus brazos) y 4 pares de patas largas; corre de lado a >1 m/s (eso lo hace la conducta). La hembra, con el abdomen plegado ancho.
        ["GhostCrab"] = new Spec { name = "GhostCrab", larvaScale = 0.03f, nymphScale = 0.1f,  spider = true, armPairs = 0, antenna = 0.05f, bodyHeight = 0.22f, legWidth = 0.05f,
            body = new[] { S("Caparazon", 0.8f, 0.9f, 0.32f) },
            legs = new[] { Sp(0.9f), Sp(1.0f), Sp(1.05f), Sp(0.9f) },
            fx = new FaunaTraits { crustacean = true, chelae = 0.55f, chelaWidth = 0.12f, chelaAsym = 1.35f, eyeStalks = 0.3f, apron = 0.45f, waist = 0.5f,
                zoea = true, nymph = "Megalopa" } },
        // MEGALOPA: la última larva, ya con pinzas y patas pero con el abdomen extendido detrás (nadadora); llega a la playa.
        ["Megalopa"] = new Spec { name = "Megalopa", spider = true, armPairs = 0, antenna = 0.1f, bodyHeight = 0.12f, legWidth = 0.035f,
            body = new[] { S("Caparazon", 0.45f, 0.42f, 0.25f), S("Abdomen", 0.5f, 0.2f, 0.1f) },
            legs = new[] { Sp(0.45f), Sp(0.5f), Sp(0.5f), Sp(0.4f) },
            fx = new FaunaTraits { crustacean = true, chelae = 0.3f, chelaWidth = 0.06f, eyeStalks = 0.12f, waist = 0.3f } },

        // ── MOLUSCOS ───────────────────────────────────────────────────────────────────────────────────────────
        // Caracol (Helix lucorum, concha 35–60 mm; Cepaea nemoralis 18–25 mm, con 5 bandas y labio pardo): el pie, la
        // cabeza con 2 tentáculos largos con ojos y 2 cortos, y la concha en espiral; el LABIO del peristoma solo en el adulto.
        ["Snail"] = new Spec { name = "Snail", juvenileScale = 0.4f,  antenna = 0f, bodyHeight = 0f, legs = new float[0][],
            body = new[] { S("Cabeza", 0.2f, 0.2f, 0.14f), S("Pie", 0.8f, 0.26f, 0.1f) },
            fx = new FaunaTraits { shell = 0.7f, whorls = 4, shellLip = true, tentacles = 0.22f, curl = 2, hermaphrodite = true, accent = new UnityEngine.Color(0.55f, 0.48f, 0.4f), juvenile = "SnailJuvenile" } },
        // Caracol joven: concha de 2 vueltas, más pequeña, sin labio (lo forma al madurar, a los 1–3 años).
        ["SnailJuvenile"] = new Spec { name = "SnailJuvenile", antenna = 0f, bodyHeight = 0f, legs = new float[0][],
            body = new[] { S("Cabeza", 0.22f, 0.22f, 0.15f), S("Pie", 0.78f, 0.26f, 0.1f) },
            fx = new FaunaTraits { shell = 0.5f, whorls = 2, shellLip = false, tentacles = 0.2f, curl = 2, hermaphrodite = true, accent = new UnityEngine.Color(0.6f, 0.52f, 0.44f) } },
        // Babosa negra (Arion ater, 10–15 cm): la concha reducida a granitos dentro del MANTO (con el pneumostoma a la
        // derecha) y el pie rugoso; la cría nace naranja, pasa a pajiza y se vuelve negra.
        ["Slug"] = new Spec { name = "Slug", juvenileScale = 0.3f,  antenna = 0f, bodyHeight = 0f, legs = new float[0][],
            body = new[] { S("Cabeza", 0.12f, 0.16f, 0.12f), S("Manto", 0.3f, 0.2f, 0.16f), S("Pie", 0.58f, 0.18f, 0.13f) },
            fx = new FaunaTraits { mantle = true, tentacles = 0.14f, curl = 3, hermaphrodite = true } },

        // ── ANÉLIDOS ───────────────────────────────────────────────────────────────────────────────────────────
        // Lombriz de tierra (Lumbricus terrestris, 9–30 cm): ~150 anillos, el CLITELO (la «silla» que hace el capullo)
        // solo en el adulto; la cola aplanada. Hermafrodita; vive 4–8 años.
        ["Earthworm"] = new Spec { name = "Earthworm", juvenileScale = 0.4f,  antenna = 0f, bodyHeight = 0f, legs = new float[0][], body = Rings(true),
            fx = new FaunaTraits { waist = 0.3f, curl = 3, hermaphrodite = true, juvenile = "EarthwormJuvenile" } },
        ["EarthwormJuvenile"] = new Spec { name = "EarthwormJuvenile", antenna = 0f, bodyHeight = 0f, legs = new float[0][], body = Rings(false),
            fx = new FaunaTraits { waist = 0.3f, curl = 3, hermaphrodite = true } },

        // ── INSECTOS ───────────────────────────────────────────────────────────────────────────────────────────
        // Cigarra del fresno (Cicada orni, 25 mm, 70 mm de envergadura): cabeza ancha con los ojos a los lados, tórax alto,
        // alas transparentes con manchas negras en tejado, más largas que el cuerpo; chupa savia con el pico.
        ["Cicada"] = new Spec { name = "Cicada", nymphScale = 0.9f,  antenna = 0.06f, stylet = true, wings = true, bodyHeight = 0.1f, legWidth = 0.03f,
            body = new[] { S("Cabeza", 0.14f, 0.36f, 0.16f), S("Torax", 0.3f, 0.34f, 0.26f), S("Abdomen", 0.56f, 0.28f, 0.22f) },
            legs = new[] { I(0.14f, 0.12f, 0.06f), I(0.12f, 0.12f, 0.06f), I(0.14f, 0.16f, 0.07f) },
            fx = new FaunaTraits { wingLen = 1.45f, clearWings = true, wingSpots = true, bigEyes = true, nymph = "CicadaNymph" } },
        // NINFA de cigarra: años bajo tierra chupando savia de las raíces; robusta, con las patas delanteras EXCAVADORAS y
        // los esbozos de las alas; la última sube a un tronco, se abre por la espalda y sale el adulto (deja la exuvia).
        ["CicadaNymph"] = new Spec { name = "CicadaNymph", antenna = 0.08f, stylet = true, wings = true, bodyHeight = 0.06f, legWidth = 0.035f,
            body = new[] { S("Cabeza", 0.16f, 0.3f, 0.16f), S("Torax", 0.34f, 0.32f, 0.3f), S("Abdomen", 0.5f, 0.3f, 0.26f) },
            legs = new[] { I(0.16f, 0.12f, 0.05f), I(0.12f, 0.12f, 0.06f), I(0.14f, 0.15f, 0.07f) },
            fx = new FaunaTraits { wingLen = 0.25f, digger = true, bigEyes = true } },
        // Luciérnaga (Luciola, 8–12 mm): pronoto en escudo (rosado) que tapa la cabeza, élitros blandos pardos y la
        // LINTERNA bajo la punta del abdomen. La larva, depredadora de caracoles, también brilla.
        ["Firefly"] = new Spec { name = "Firefly", larvaScale = 1.4f, pupaScale = 0.9f,  elytra = true, antenna = 0.3f, bodyHeight = 0.08f, legWidth = 0.02f,
            body = new[] { S("Cabeza", 0.08f, 0.14f, 0.1f), S("Pronoto", 0.2f, 0.38f, 0.08f), S("Elitros", 0.72f, 0.4f, 0.16f) },
            legs = new[] { I(0.14f, 0.14f, 0.1f), I(0.14f, 0.14f, 0.1f), I(0.15f, 0.15f, 0.1f) },
            fx = new FaunaTraits { lantern = 1, larvaLegs = true, accent = new UnityEngine.Color(0.92f, 0.55f, 0.45f) } },
        // Tijereta (Forficula auricularia, 12–15 mm): plana, alargada, con las tegminas cortas y los FÓRCEPS (cercos):
        // en la hembra rectos y finos (~3 mm); en el macho anchos en la base, curvos y dentados. La madre cuida los huevos.
        ["Earwig"] = new Spec { name = "Earwig", nymphScale = 0.75f,  antenna = 0.45f, wings = true, bodyHeight = 0.06f, legWidth = 0.02f,
            body = new[] { S("Cabeza", 0.13f, 0.13f, 0.08f), S("Torax", 0.2f, 0.14f, 0.08f), S("Abdomen", 0.52f, 0.16f, 0.08f) },
            legs = new[] { I(0.14f, 0.12f, 0.08f), I(0.14f, 0.12f, 0.08f), I(0.15f, 0.13f, 0.08f) },
            fx = new FaunaTraits { wingLen = 0.22f, forceps = 0.2f } },
        // Cucaracha oriental (Blatta orientalis, 18–29 mm): plana, con el pronoto en escudo, antenas largas y patas
        // espinosas; la HEMBRA (la ficha) con muñones de ala y más ancha; el macho con alas hasta 3/4 del abdomen.
        ["Cockroach"] = new Spec { name = "Cockroach", nymphScale = 0.85f,  antenna = 1.0f, wings = true, bodyHeight = 0.07f, legWidth = 0.025f,
            body = new[] { S("Cabeza", 0.1f, 0.16f, 0.1f), S("Pronoto", 0.2f, 0.45f, 0.08f), S("Abdomen", 0.7f, 0.5f, 0.1f) },
            legs = new[] { I(0.22f, 0.25f, 0.2f), I(0.26f, 0.3f, 0.24f), I(0.3f, 0.36f, 0.28f) },
            fx = new FaunaTraits { legFrom = 0.12f, legTo = 0.5f, wingLen = 0.15f, cerci = 0.08f } },
        // Pulga de nieve (Boreus hyemalis, 3–5 mm; un mecóptero, no una pulga): pico largo hacia abajo, abdomen
        // abultado, patas largas para saltar; activa SOBRE la nieve de octubre a marzo. La larva vive en el musgo.
        ["Scorpionfly"] = new Spec { name = "Scorpionfly", larvaScale = 1.1f, pupaScale = 0.9f,  antenna = 0.5f, bodyHeight = 0.14f, legWidth = 0.02f,
            body = new[] { S("Cabeza", 0.14f, 0.14f, 0.16f), S("Torax", 0.26f, 0.16f, 0.16f), S("Abdomen", 0.55f, 0.22f, 0.24f) },
            legs = new[] { I(0.3f, 0.3f, 0.15f), I(0.35f, 0.35f, 0.18f), I(0.45f, 0.45f, 0.22f) },
            fx = new FaunaTraits { rostrum = 0.22f, larvaLegs = true } },
    };

    /// <summary>
    /// DIMORFISMO de las especies de cada era (lo llama ForSex). Los hermafroditas (caracol, babosa, lombriz) no cambian.
    /// </summary>
    static void FaunaForSex(Spec c, bool female, System.Action<string, float, float> scale)
    {
        if (c.fx == null) return;
        c.fx = c.fx.Clone();
        if (c.legs != null) c.legs = (float[][])c.legs.Clone();
        switch (c.name)
        {
            // Escorpión: el macho, mesosoma más estrecho, cola más larga y peines mayores; la hembra, más ancha (preñada).
            case "Scorpion":
                for (int i = 1; i <= 7; i++) scale("Terguito_" + i, 1f, female ? 1.08f : 0.88f);
                if (!female) { c.fx.tailLen *= 1.12f; c.fx.pectines *= 1.35f; }
                break;
            // Solífugo: el macho, con flagelo en los quelíceros, patas más largas y abdomen más fino.
            case "CamelSpider":
                if (!female) { c.fx.flagellum = true; scale("Abdomen", 0.9f, 0.85f); for (int p = 0; p < c.legs.Length; p++) { var l = (float[])c.legs[p].Clone(); for (int k = 0; k < l.Length; k++) l[k] *= 1.1f; c.legs[p] = l; } }
                break;
            case "VelvetMite": if (female) { scale("Propodosoma", 1.1f, 1.1f); scale("Histerosoma", 1.2f, 1.15f); } break;   // la hembra, mayor
            case "Woodlouse": if (female) c.fx.broodPouch = true; break;   // el marsupio (hembra grávida)
            // Cangrejo fantasma: el macho algo mayor, con el abdomen plegado en «T» estrecha; la hembra, ancho (lleva los huevos).
            case "GhostCrab": if (female) c.fx.apron = 0.7f; else { c.fx.apron = 0.25f; scale("Caparazon", 1.06f, 1.06f); c.fx.chelae *= 1.1f; } break;
            // Cigarra: el macho canta con los timbales (y tiene el abdomen hueco); la hembra lleva el oviscapto.
            case "Cicada": if (female) c.ovipositor = 0.1f; else c.fx.tymbal = true; break;
            // Luciérnaga (Luciola): la linterna ocupa 2 segmentos en el macho y 1 en la hembra, algo mayor y más ancha.
            case "Firefly": if (female) { c.fx.lantern = 1; scale("Elitros", 1.04f, 1.06f); } else c.fx.lantern = 2; break;
            // Tijereta: fórceps del macho anchos, curvos y más largos; los de la hembra rectos y finos.
            case "Earwig": if (!female) { c.fx.forceps = 0.3f; c.fx.forcepsCurved = true; } break;
            // Cucaracha: el macho más estrecho y con alas hasta 3/4 del abdomen; la hembra, con muñones.
            case "Cockroach": if (!female) { c.fx.wingLen = 0.75f; scale("Abdomen", 1f, 0.85f); } break;
            // Boreus: el macho con las alas en CERDAS curvas (con ellas sujeta a la hembra); la hembra con muñones y el
            // oviscapto en sable.
            case "Scorpionfly": if (female) { c.fx.wingPads = true; c.ovipositor = 0.25f; } else c.fx.bristleWings = true; break;
        }
    }

    /// <summary>La ficha de la FASE si la especie tiene una propia (la ninfa de cigarra, la manca, la megalopa…), en su sexo.</summary>
    public static Spec StageSpec(Spec s, Metamorphosis.Phase phase, bool female)
    {
        if (s == null || s.fx == null) return null;
        string n = phase == Metamorphosis.Phase.Larva ? s.fx.larva : phase == Metamorphosis.Phase.Nymph ? s.fx.nymph : phase == Metamorphosis.Phase.Juvenile ? s.fx.juvenile : null;
        if (n == null || n == s.name) return null;
        return Named(n);
    }

    // TAMAÑO DE LAS FASES (InsectMorphology.PhaseScale), reales aunque salgan diminutas: la zoea del cangrejo mide ~1 mm
    // frente a los ~5 cm del caparazón adulto (×0,03) y la megalopa ~4,5 mm (×0,1); la manca de cochinilla ~2 mm (×0,15);
    // la larva del ácaro ~0,5 mm (×0,15); la larva madura de luciérnaga es MAYOR que el adulto (×1,4); la ninfa de último
    // estadio de cigarra casi como el adulto (×0,9). Los «juveniles» de desarrollo directo, a media vida (escorpión ×0,45). [A]

    /// <summary>El nombre de la fase en esta especie («zoea», «manca», «ninfa excavadora»…) o null (el de siempre).</summary>
    public static string StageLabel(string species, Metamorphosis.Phase phase)
    {
        switch (For(species).name)
        {
            case "GhostCrab": return phase == Metamorphosis.Phase.Larva ? "zoea (en el mar)" : phase == Metamorphosis.Phase.Nymph ? "megalopa" : null;
            case "VelvetMite": return phase == Metamorphosis.Phase.Larva ? "larva (6 patas, parásita)" : null;
            case "Woodlouse": return phase == Metamorphosis.Phase.Juvenile ? "manca (6 pares de patas)" : null;
            case "Snail": return phase == Metamorphosis.Phase.Juvenile ? "joven (sin labio)" : null;
            case "Earthworm": return phase == Metamorphosis.Phase.Juvenile ? "joven (sin clitelo)" : null;
            case "Scorpion": return phase == Metamorphosis.Phase.Juvenile ? "cría (va sobre la madre)" : null;
            case "Cicada": return phase == Metamorphosis.Phase.Nymph ? "ninfa (años bajo tierra)" : null;
            case "Firefly": return phase == Metamorphosis.Phase.Larva ? "larva luminosa" : null;
            default: return phase == Metamorphosis.Phase.Juvenile ? "joven" : null;
        }
    }

    /// <summary>¿Hermafrodita (sin macho ni hembra visibles)? Caracol, babosa, lombriz.</summary>
    public static bool Hermaphrodite(string species) { Spec s = For(species); return s != null && s.fx != null && s.fx.hermaphrodite; }
}
