using UnityEngine;

/// <summary>
/// FLORA Y FAUNA NATIVAS de cada microcosmos (Berón, 2026-10-23: «cada microcosmos se debe generar con la flora y fauna
/// nativa de la era/región»). AreaMicrocosmos.Nature lee el bioma del microcosmos por su nombre: qué plantas crecen (con su
/// nombre real, su color y su porte), en qué proporción, y qué artrópodos viven allí en la ESTACIÓN de la historia
/// (en invierno casi todos están aletargados: solo salen los activos). Cada especie real se dibuja con el cuerpo de cubos
/// de su grupo (InsectMorphology: una hormiga tejedora es una «Ant» naranja; un abejorro, una «SnowBee» a rayas).
/// Datos y fuentes: docs/microcosmos-biomas.md. Sin bioma propio (las áreas del santuario de hoy), el genérico de siempre.
/// Las especies sin cuerpo propio hasta 2026-10-24 (escorpión, solífugo, ácaro, cochinilla, cangrejo, caracol, babosa,
/// lombriz, cigarra, luciérnaga, tijereta, cucaracha) ya tienen su ficha (InsectMorphology.Fauna.cs): aquí van con su
/// nombre real; ver docs/microcosmos-fauna-eras.md.
/// Escala: 1 cm real de planta ≈ 0,25 m en el microcosmos (la brizna de hierba de 5 cm ≈ 12 m); los bichos, ≈ 0,3 m por mm.
/// </summary>
public static partial class MicroBiome
{
    public enum Kind { Grass, Moss, Fungus, Fern, Flower, Shrub, Stone, Sand, Dung, Snow }

    public struct Plant
    {
        public Kind kind; public float weight; public string name; public float cm; public Color main, accent;
        public Plant(Kind k, float w, string n, float cm, Color main, Color accent) { kind = k; weight = w; name = n; this.cm = cm; this.main = main; this.accent = accent; }
    }

    public struct Bug
    {
        public string species; public int count; public float mm; public Color tint; public string name;
        public string dormant;   // aletargado en la estación de la historia (por qué): está, pero quieto (Dormant)
        public Bug(string sp, int n, float mm, Color tint, string name, string dormant = null) { species = sp; count = n; this.mm = mm; this.tint = tint; this.name = name; this.dormant = dormant; }
        public float Length => Mathf.Clamp(mm * 0.3f, 0.8f, 12f);
    }

    /// <summary>Un ANIMAL DEL PUEBLO (los caballos de los carros, los gatos de las casas…) en un mundo de insectos: va con el cuerpo
    /// de su ANÁLOGO entre los artrópodos (el canon: las vacas son pulgones, Berkeley) y a la talla del animal real
    /// (<see cref="length"/>, en m, sin la escala de hormiga: el pueblo es a escala humana). Ver docs/microcosmos-fauna-eras.md.</summary>
    public struct Pet
    {
        public string species; public int count; public float length; public Color tint; public string name;
        public Pet(string sp, int n, float length, Color tint, string name) { species = sp; count = n; this.length = length; this.tint = tint; this.name = name; }
    }

    public class Biome
    {
        public string title;     // región y era, para el cartel y el log
        public bool winter;      // casi todo aletargado: menos fauna, escarcha o nieve en el suelo
        public Color ground = new Color(0.45f, 0.36f, 0.25f);   // la tierra de alrededor
        public Plant[] plants;
        public Bug[] bugs;
        /// <summary>Lo PLANTADO en el pueblo (árboles de calle, setos, céspedes, parterres) en las eras avanzadas (regla 5): crece
        /// junto a las calles y entre los solares, a escala de pueblo (AreaMicrocosmos.Nature, modo pueblo). Sin lista, la del
        /// lugar (<see cref="plants"/>). <see cref="plants"/> sigue siendo lo del lugar: en las afueras, lejos del pueblo.</summary>
        public Plant[] town;
        /// <summary>Los ANIMALES DEL PUEBLO (por las calles): ver <see cref="Pet"/>.</summary>
        public Pet[] townFauna;
    }

    static Color C(float r, float g, float b) => new Color(r, g, b);
    static readonly Color Stone = C(0.55f, 0.52f, 0.48f), Sand = C(0.82f, 0.72f, 0.5f), DungC = C(0.35f, 0.25f, 0.15f), Snow = C(0.94f, 0.96f, 1f);

    /// <summary>El bioma del microcosmos llamado <paramref name="display"/> (o null: el genérico).</summary>
    public static Biome For(string display)
    {
        Biome famous = FamousBiome(display);   // los famosos (MicroBiome.Famosos.cs, docs/microcosmos-famosos.md)
        if (famous != null) return famous;
        Biome music = MusicBiome(display);   // la música (MicroBiome.Musica.cs, docs/microcosmos-musica.md)
        if (music != null) return music;
        switch (display)
        {
            // ── Kapilavastu / Lumbini (Terai del Nepal), s. V a. C., tras el monzón (oct–nov) ──
            case "Kapilavastu": return new Biome { title = "Terai del Nepal, s. V a. C. (otoño, tras el monzón)", ground = C(0.5f, 0.4f, 0.28f), plants = new[] {
                new Plant(Kind.Grass, 22, "Arroz (Oryza sativa)", 100, C(0.55f, 0.68f, 0.25f), C(0.85f, 0.72f, 0.35f)),
                new Plant(Kind.Grass, 14, "Hierba kusha (Desmostachya bipinnata)", 90, C(0.55f, 0.62f, 0.5f), C(0.7f, 0.68f, 0.55f)),
                new Plant(Kind.Grass, 12, "Grama durva (Cynodon dactylon)", 20, C(0.4f, 0.65f, 0.25f), C(0.5f, 0.6f, 0.3f)),
                new Plant(Kind.Flower, 8, "Caña kans en flor (Saccharum spontaneum)", 150, C(0.5f, 0.62f, 0.35f), C(0.97f, 0.97f, 0.95f)),
                new Plant(Kind.Shrub, 7, "Albahaca sagrada, tulsi (Ocimum tenuiflorum)", 45, C(0.35f, 0.5f, 0.25f), C(0.55f, 0.3f, 0.55f)),
                new Plant(Kind.Fern, 6, "Plántula de sal (Shorea robusta)", 40, C(0.3f, 0.55f, 0.25f), C(0.35f, 0.3f, 0.2f)),
                new Plant(Kind.Fungus, 8, "Seta de termitero (Termitomyces)", 15, C(0.9f, 0.86f, 0.75f), C(0.8f, 0.7f, 0.55f)),
                new Plant(Kind.Stone, 10, "Piedra", 0, Stone, Stone), new Plant(Kind.Sand, 4, "Arena del río", 0, Sand, Sand), new Plant(Kind.Dung, 3, "Boñiga de vaca", 0, DungC, DungC) },
              bugs = new[] {
                new Bug("Termite", 10, 5, C(0.92f, 0.86f, 0.72f), "Termita de montículo (Odontotermes obesus)"),
                new Bug("Grasshopper", 5, 25, C(0.45f, 0.65f, 0.25f), "Saltamontes del arroz (Oxya hyla)"),
                new Bug("Ladybug", 5, 5, C(0.95f, 0.5f, 0.1f), "Mariquita de bandas (Coccinella transversalis)"),
                new Bug("DungBeetle", 4, 48, C(0.12f, 0.1f, 0.08f), "Pelotero gigante (Heliocopris bucephalus)"),
                new Bug("Mantis", 3, 70, C(0.4f, 0.7f, 0.3f), "Mantis asiática (Hierodula)"),
                new Bug("Antlion", 4, 12, C(0.6f, 0.52f, 0.4f), "Hormiga león (Myrmeleon)"),
                new Bug("Aphid", 6, 2, C(0.55f, 0.75f, 0.35f), "Pulgón del arroz"),
                new Bug("Firefly", 6, 10, C(0.3f, 0.22f, 0.15f), "Luciérnaga (Luciola), destellos al anochecer sobre los arrozales"),
                new Bug("VelvetMite", 3, 5, C(0.85f, 0.08f, 0.06f), "Ácaro de terciopelo rojo, «bir bahuti» (Trombidium), tras las lluvias") } };

            // ── Llanura del Ganges, s. II a. C., invierno seco – primavera (feb–mar) ──
            case "Patanjali": return new Biome { title = "Llanura del Ganges, s. II a. C. (invierno seco)", ground = C(0.7f, 0.6f, 0.45f), plants = new[] {
                new Plant(Kind.Grass, 18, "Hierba kusha (Desmostachya bipinnata), el asiento de las ásanas", 90, C(0.55f, 0.62f, 0.5f), C(0.7f, 0.68f, 0.55f)),
                new Plant(Kind.Grass, 16, "Cebada madurando (Hordeum vulgare)", 80, C(0.65f, 0.7f, 0.35f), C(0.88f, 0.8f, 0.5f)),
                new Plant(Kind.Grass, 10, "Grama durva (Cynodon dactylon)", 20, C(0.4f, 0.65f, 0.25f), C(0.5f, 0.6f, 0.3f)),
                new Plant(Kind.Shrub, 8, "Algodoncillo, aak (Calotropis procera)", 150, C(0.6f, 0.68f, 0.62f), C(0.75f, 0.6f, 0.85f)),
                new Plant(Kind.Shrub, 6, "Tulsi (Ocimum tenuiflorum)", 45, C(0.35f, 0.5f, 0.25f), C(0.55f, 0.3f, 0.55f)),
                new Plant(Kind.Flower, 6, "Juncia (Cyperus rotundus)", 25, C(0.4f, 0.55f, 0.25f), C(0.55f, 0.3f, 0.2f)),
                new Plant(Kind.Stone, 12, "Piedra", 0, Stone, Stone), new Plant(Kind.Sand, 6, "Limo seco", 0, C(0.78f, 0.7f, 0.55f), Sand), new Plant(Kind.Dung, 3, "Boñiga de vaca", 0, DungC, DungC) },
              bugs = new[] {
                new Bug("Termite", 10, 5, C(0.92f, 0.86f, 0.72f), "Termita (Odontotermes obesus)"),
                new Bug("Cricket", 5, 25, C(0.1f, 0.08f, 0.07f), "Grillo de dos manchas (Gryllus bimaculatus)"),
                new Bug("SnowBee", 4, 8, C(0.85f, 0.45f, 0.2f), "Abeja enana (Apis florea)"),
                new Bug("Ladybug", 5, 7, C(0.85f, 0.12f, 0.1f), "Mariquita de 7 puntos (Coccinella septempunctata)"),
                new Bug("Aphid", 8, 2, C(0.6f, 0.68f, 0.55f), "Pulgón de la mostaza (Lipaphis erysimi)"),
                new Bug("DungBeetle", 3, 25, C(0.12f, 0.1f, 0.08f), "Escarabajo pelotero"),
                new Bug("Scorpion", 2, 90, C(0.08f, 0.08f, 0.1f), "Escorpión gigante de bosque (Heterometrus), de noche") } };

            // ── Varsovia, otoño–invierno de 1942: escombros, grietas y hierbas ruderales ──
            case "Irena Sendler": return new Biome { title = "Varsovia, 1942 (otoño–invierno)", winter = true, ground = C(0.48f, 0.45f, 0.42f), plants = new[] {
                new Plant(Kind.Flower, 10, "Llantén mayor (Plantago major)", 20, C(0.25f, 0.42f, 0.2f), C(0.45f, 0.4f, 0.3f)),
                new Plant(Kind.Grass, 16, "Poa anual (Poa annua)", 12, C(0.55f, 0.68f, 0.4f), C(0.6f, 0.65f, 0.45f)),
                new Plant(Kind.Moss, 14, "Musgo plateado de las grietas (Bryum argenteum)", 1, C(0.6f, 0.68f, 0.58f), C(0.75f, 0.8f, 0.72f)),
                new Plant(Kind.Flower, 6, "Diente de león (Taraxacum officinale)", 15, C(0.35f, 0.5f, 0.25f), C(0.98f, 0.85f, 0.15f)),
                new Plant(Kind.Fern, 7, "Ortiga seca (Urtica dioica)", 90, C(0.45f, 0.38f, 0.25f), C(0.4f, 0.35f, 0.25f)),
                new Plant(Kind.Shrub, 6, "Cenizo seco (Chenopodium album)", 60, C(0.55f, 0.48f, 0.35f), C(0.6f, 0.55f, 0.45f)),
                new Plant(Kind.Stone, 24, "Escombro", 0, C(0.6f, 0.45f, 0.4f), Stone), new Plant(Kind.Snow, 10, "Escarcha", 0, Snow, Snow) },
              bugs = new[] {
                new Bug("Spider", 4, 14, C(0.45f, 0.32f, 0.22f), "Araña doméstica (Eratigena atrica)"),
                new Bug("Ladybug", 6, 7, C(0.85f, 0.12f, 0.1f), "Mariquitas de 7 puntos invernando en grupo"),
                new Bug("Ant", 3, 4, C(0.15f, 0.12f, 0.1f), "Hormiga negra (Lasius niger), al sol"),
                new Bug("Woodlouse", 4, 14, C(0.42f, 0.43f, 0.45f), "Cochinilla de humedad (Porcellio scaber), bajo el escombro"),
                new Bug("Cockroach", 3, 24, C(0.22f, 0.12f, 0.07f), "Cucaracha oriental (Blatta orientalis), de los sótanos") } };

            // ── Le Chambon-sur-Lignon (meseta del Vivarais-Lignon, ~1000 m), otoño de 1942 ──
            case "Le Chambon-sur-Lignon": return new Biome { title = "Meseta del Vivarais-Lignon, 1942 (otoño)", ground = C(0.36f, 0.3f, 0.22f), plants = new[] {
                new Plant(Kind.Shrub, 14, "Arándano (Vaccinium myrtillus)", 30, C(0.6f, 0.3f, 0.2f), C(0.2f, 0.2f, 0.4f)),
                new Plant(Kind.Shrub, 12, "Brezo (Calluna vulgaris)", 35, C(0.3f, 0.35f, 0.22f), C(0.7f, 0.45f, 0.7f)),
                new Plant(Kind.Shrub, 6, "Piorno (Cytisus oromediterraneus)", 70, C(0.2f, 0.35f, 0.18f), C(0.25f, 0.4f, 0.2f)),
                new Plant(Kind.Fern, 10, "Helecho águila (Pteridium aquilinum), rojizo", 90, C(0.65f, 0.4f, 0.2f), C(0.45f, 0.3f, 0.15f)),
                new Plant(Kind.Moss, 12, "Musgo de Schreber (Pleurozium schreberi)", 10, C(0.55f, 0.62f, 0.25f), C(0.7f, 0.72f, 0.3f)),
                new Plant(Kind.Fungus, 6, "Matamoscas (Amanita muscaria)", 15, C(0.95f, 0.93f, 0.88f), C(0.85f, 0.15f, 0.1f)),
                new Plant(Kind.Fungus, 5, "Boleto (Boletus edulis)", 14, C(0.92f, 0.88f, 0.78f), C(0.5f, 0.32f, 0.18f)),
                new Plant(Kind.Grass, 12, "Prado de siega", 20, C(0.5f, 0.6f, 0.3f), C(0.7f, 0.65f, 0.4f)),
                new Plant(Kind.Stone, 12, "Granito", 0, C(0.62f, 0.6f, 0.58f), Stone) },
              bugs = new[] {
                new Bug("Ant", 8, 7, C(0.7f, 0.25f, 0.12f), "Hormiga roja de bosque (Formica rufa)"),
                new Bug("DungBeetle", 4, 16, C(0.12f, 0.15f, 0.35f), "Geotrupo de bosque (Anoplotrupes stercorosus)"),
                new Bug("Grasshopper", 4, 18, C(0.5f, 0.55f, 0.3f), "Saltamontes de prado (Pseudochorthippus parallelus)"),
                new Bug("Spider", 4, 12, C(0.65f, 0.42f, 0.2f), "Araña de jardín (Araneus diadematus)"),
                new Bug("SnowBee", 3, 18, C(0.2f, 0.18f, 0.12f), "Abejorro (Bombus terrestris)"),
                new Bug("Ladybug", 4, 7, C(0.85f, 0.12f, 0.1f), "Mariquita de 7 puntos (Coccinella septempunctata)"),
                new Bug("Earthworm", 4, 150, C(0.75f, 0.42f, 0.42f), "Lombriz de tierra (Lumbricus terrestris)") } };

            // ── Escútari (Üsküdar), invierno de 1854–55: el cementerio de Karacaahmet junto al cuartel ──
            case "Escútari": return new Biome { title = "Escútari, Estambul, invierno de 1854–55", winter = true, ground = C(0.42f, 0.36f, 0.28f), plants = new[] {
                new Plant(Kind.Moss, 14, "Hiedra rastrera (Hedera helix)", 10, C(0.15f, 0.32f, 0.15f), C(0.2f, 0.4f, 0.2f)),
                new Plant(Kind.Flower, 8, "Ciclamen de invierno (Cyclamen coum)", 8, C(0.2f, 0.3f, 0.2f), C(0.85f, 0.2f, 0.55f)),
                new Plant(Kind.Flower, 8, "Malva (Malva sylvestris)", 50, C(0.35f, 0.5f, 0.3f), C(0.7f, 0.5f, 0.8f)),
                new Plant(Kind.Flower, 6, "Caléndula silvestre (Calendula arvensis)", 20, C(0.4f, 0.55f, 0.3f), C(0.98f, 0.6f, 0.1f)),
                new Plant(Kind.Grass, 14, "Cebada ratonera (Hordeum murinum)", 25, C(0.5f, 0.65f, 0.35f), C(0.7f, 0.7f, 0.45f)),
                new Plant(Kind.Fern, 6, "Plántula de ciprés (Cupressus sempervirens)", 25, C(0.15f, 0.3f, 0.18f), C(0.2f, 0.25f, 0.15f)),
                new Plant(Kind.Moss, 8, "Musgo de pared (Tortula muralis)", 1, C(0.4f, 0.55f, 0.3f), C(0.65f, 0.7f, 0.6f)),
                new Plant(Kind.Stone, 16, "Lápida y piedra", 0, C(0.75f, 0.74f, 0.7f), Stone) },
              bugs = new[] {
                new Bug("Spider", 4, 8, C(0.8f, 0.72f, 0.6f), "Araña de sótano (Pholcus phalangioides)"),
                new Bug("Ladybug", 5, 7, C(0.85f, 0.12f, 0.1f), "Mariquitas de 7 puntos invernando"),
                new Bug("Ant", 3, 7, C(0.18f, 0.12f, 0.1f), "Hormiga segadora (Messor), en un día templado"),
                new Bug("Cockroach", 4, 24, C(0.22f, 0.12f, 0.07f), "Cucaracha oriental (Blatta orientalis), en el cuartel"),
                new Bug("Woodlouse", 3, 14, C(0.5f, 0.5f, 0.52f), "Cochinilla de bola (Armadillidium vulgare)"),
                new Bug("Snail", 2, 45, C(0.85f, 0.78f, 0.62f), "Caracol turco (Helix lucorum)", "inverna sellado con su epifragma") } };

            // ── Flandes (saliente de Ypres), Nochebuena de 1914: helada dura sobre el barro ──
            case "Tregua 1914": return new Biome { title = "Flandes, diciembre de 1914 (helada)", winter = true, ground = C(0.33f, 0.28f, 0.22f), plants = new[] {
                new Plant(Kind.Grass, 18, "Ballico de pasto (Lolium perenne) escarchado", 20, C(0.45f, 0.55f, 0.35f), C(0.85f, 0.88f, 0.9f)),
                new Plant(Kind.Flower, 8, "Pamplina (Stellaria media), en flor en invierno", 10, C(0.4f, 0.65f, 0.3f), C(0.97f, 0.97f, 0.97f)),
                new Plant(Kind.Flower, 6, "Ranúnculo rastrero (Ranunculus repens), roseta", 15, C(0.25f, 0.42f, 0.2f), C(0.3f, 0.45f, 0.25f)),
                new Plant(Kind.Shrub, 6, "Romaza (Rumex obtusifolius)", 40, C(0.35f, 0.3f, 0.18f), C(0.55f, 0.3f, 0.15f)),
                new Plant(Kind.Fern, 6, "Ortiga muerta (Urtica dioica)", 70, C(0.45f, 0.38f, 0.25f), C(0.4f, 0.35f, 0.25f)),
                new Plant(Kind.Flower, 4, "Amapola en roseta (Papaver rhoeas): florece en mayo", 4, C(0.45f, 0.55f, 0.45f), C(0.5f, 0.6f, 0.5f)),
                new Plant(Kind.Moss, 8, "Musgo (Brachythecium rutabulum)", 3, C(0.55f, 0.6f, 0.25f), C(0.7f, 0.72f, 0.3f)),
                new Plant(Kind.Sand, 14, "Barro helado", 0, C(0.35f, 0.3f, 0.25f), C(0.4f, 0.35f, 0.3f)),
                new Plant(Kind.Snow, 14, "Escarcha", 0, Snow, Snow), new Plant(Kind.Stone, 8, "Terrón", 0, C(0.42f, 0.35f, 0.28f), Stone) },
              bugs = new[] {
                new Bug("Spider", 6, 3, C(0.08f, 0.08f, 0.08f), "Araña de sábana (Erigone atra), activa en invierno"),
                new Bug("DungBeetle", 4, 12, C(0.08f, 0.08f, 0.1f), "Carábido de invierno (Nebria brevicollis)"),
                new Bug("Earthworm", 4, 150, C(0.72f, 0.42f, 0.42f), "Lombriz de tierra (Lumbricus terrestris), en el barro"),
                new Bug("Woodlouse", 3, 14, C(0.42f, 0.43f, 0.45f), "Cochinilla (Porcellio scaber)"),
                new Bug("Slug", 2, 120, C(0.08f, 0.07f, 0.07f), "Babosa negra (Arion ater)") } };

            // ── Kill Devil Hills, 17 de diciembre de 1903: la duna de los Wright ──
            case "Dayton": return new Biome { title = "Kill Devil Hills, diciembre de 1903 (duna)", winter = true, ground = C(0.86f, 0.78f, 0.6f), plants = new[] {
                new Plant(Kind.Grass, 16, "Barrón americano (Ammophila breviligulata)", 80, C(0.75f, 0.68f, 0.45f), C(0.82f, 0.75f, 0.5f)),
                new Plant(Kind.Grass, 10, "Avena de mar (Uniola paniculata)", 140, C(0.72f, 0.65f, 0.42f), C(0.88f, 0.78f, 0.45f)),
                new Plant(Kind.Shrub, 8, "Brezo de playa (Hudsonia tomentosa)", 15, C(0.6f, 0.65f, 0.58f), C(0.7f, 0.72f, 0.65f)),
                new Plant(Kind.Grass, 6, "Heno de marisma (Spartina patens)", 50, C(0.7f, 0.62f, 0.4f), C(0.75f, 0.65f, 0.45f)),
                new Plant(Kind.Shrub, 5, "Nopal oriental (Opuntia humifusa)", 20, C(0.35f, 0.55f, 0.3f), C(0.4f, 0.6f, 0.35f)),
                new Plant(Kind.Shrub, 5, "Mirto de cera (Morella cerifera)", 150, C(0.4f, 0.48f, 0.3f), C(0.7f, 0.72f, 0.75f)),
                new Plant(Kind.Sand, 40, "Arena de duna", 0, C(0.88f, 0.8f, 0.62f), Sand), new Plant(Kind.Stone, 4, "Concha", 0, C(0.9f, 0.88f, 0.82f), Stone) },
              bugs = new[] {
                new Bug("Antlion", 5, 12, C(0.8f, 0.74f, 0.6f), "Hormiga león (Myrmeleon immaculatus), aletargada en su embudo"),
                new Bug("Spider", 3, 13, C(0.85f, 0.82f, 0.72f), "Araña lobo de playa (Arctosa littoralis)"),
                new Bug("Ladybug", 3, 6, C(0.95f, 0.45f, 0.15f), "Mariquita de 9 puntos (Coccinella novemnotata), invernando"),
                new Bug("GhostCrab", 3, 45, C(0.82f, 0.76f, 0.6f), "Cangrejo fantasma (Ocypode quadrata)", "hiberna en su madriguera de octubre a abril") } };

            // ── Egina, Grecia arcaica (mito de los mirmidones), final de primavera ──
            case "Egina": return new Biome { title = "Egina, Grecia arcaica (final de primavera)", ground = C(0.66f, 0.45f, 0.32f), plants = new[] {
                new Plant(Kind.Shrub, 14, "Tomillo de Creta (Thymbra capitata)", 35, C(0.5f, 0.55f, 0.45f), C(0.75f, 0.45f, 0.7f)),
                new Plant(Kind.Shrub, 10, "Pimpinela espinosa (Sarcopoterium spinosum)", 45, C(0.55f, 0.6f, 0.5f), C(0.75f, 0.25f, 0.2f)),
                new Plant(Kind.Flower, 8, "Jara de Creta (Cistus creticus)", 70, C(0.5f, 0.58f, 0.45f), C(0.92f, 0.55f, 0.75f)),
                new Plant(Kind.Shrub, 7, "Salvia griega (Salvia fruticosa)", 70, C(0.6f, 0.65f, 0.58f), C(0.7f, 0.6f, 0.85f)),
                new Plant(Kind.Flower, 6, "Gamón (Asphodelus ramosus)", 90, C(0.4f, 0.55f, 0.3f), C(0.97f, 0.95f, 0.92f)),
                new Plant(Kind.Shrub, 6, "Lentisco (Pistacia lentiscus)", 100, C(0.18f, 0.32f, 0.18f), C(0.6f, 0.2f, 0.2f)),
                new Plant(Kind.Flower, 5, "Anémona (Anemone coronaria)", 20, C(0.35f, 0.5f, 0.3f), C(0.85f, 0.1f, 0.15f)),
                new Plant(Kind.Grass, 8, "Hierba seca", 25, C(0.75f, 0.7f, 0.45f), C(0.82f, 0.75f, 0.5f)),
                new Plant(Kind.Stone, 22, "Caliza", 0, C(0.85f, 0.82f, 0.75f), Stone), new Plant(Kind.Sand, 4, "Tierra roja", 0, C(0.7f, 0.45f, 0.3f), Sand) },
              bugs = new[] {
                new Bug("Ant", 12, 8, C(0.15f, 0.1f, 0.08f), "Hormiga segadora (Messor), la del mito"),
                new Bug("Mantis", 3, 60, C(0.45f, 0.6f, 0.3f), "Mantis religiosa (Mantis religiosa)"),
                new Bug("Grasshopper", 4, 50, C(0.55f, 0.48f, 0.38f), "Langosta egipcia (Anacridium aegyptium)"),
                new Bug("DungBeetle", 3, 25, C(0.08f, 0.08f, 0.08f), "Escarabajo pelotero (Scarabaeus)"),
                new Bug("Antlion", 4, 12, C(0.62f, 0.55f, 0.45f), "Hormiga león (Myrmeleon)"),
                new Bug("SnowBee", 4, 13, C(0.75f, 0.55f, 0.2f), "Abeja melífera griega (Apis mellifera cecropia)"),
                new Bug("Cicada", 4, 25, C(0.5f, 0.45f, 0.38f), "Cigarra del fresno (Cicada orni), cantando en los pinos"),
                new Bug("Scorpion", 2, 60, C(0.72f, 0.6f, 0.35f), "Escorpión griego (Mesobuthus gibbosus), de noche") } };

            // ── Alejandría y Siene (Asuán), solsticio de verano de ~240 a. C. ──
            case "Alejandría": return new Biome { title = "Egipto, ~240 a. C. (solsticio de verano)", ground = C(0.88f, 0.76f, 0.52f), plants = new[] {
                new Plant(Kind.Grass, 12, "Rastrojo de trigo almidonero (Triticum dicoccum)", 20, C(0.85f, 0.75f, 0.45f), C(0.9f, 0.8f, 0.5f)),
                new Plant(Kind.Grass, 10, "Hierba halfa (Desmostachya bipinnata)", 90, C(0.55f, 0.62f, 0.5f), C(0.7f, 0.68f, 0.55f)),
                new Plant(Kind.Grass, 8, "Grama (Cynodon dactylon)", 20, C(0.45f, 0.62f, 0.28f), C(0.5f, 0.6f, 0.3f)),
                new Plant(Kind.Shrub, 8, "Espina de camello (Alhagi maurorum)", 60, C(0.55f, 0.62f, 0.5f), C(0.85f, 0.5f, 0.6f)),
                new Plant(Kind.Fern, 5, "Papiro (Cyperus papyrus)", 300, C(0.35f, 0.6f, 0.25f), C(0.5f, 0.7f, 0.3f)),
                new Plant(Kind.Shrub, 5, "Algodón de Sodoma (Calotropis procera)", 150, C(0.6f, 0.68f, 0.62f), C(0.75f, 0.6f, 0.85f)),
                new Plant(Kind.Shrub, 4, "Taray del Nilo (Tamarix nilotica)", 200, C(0.55f, 0.6f, 0.55f), C(0.85f, 0.7f, 0.8f)),
                new Plant(Kind.Sand, 26, "Arena del desierto", 0, C(0.9f, 0.78f, 0.55f), Sand), new Plant(Kind.Stone, 12, "Granito de Siene", 0, C(0.7f, 0.5f, 0.45f), Stone),
                new Plant(Kind.Dung, 4, "Boñiga de asno", 0, DungC, DungC) },
              bugs = new[] {
                new Bug("DungBeetle", 6, 30, C(0.06f, 0.06f, 0.06f), "Escarabajo sagrado (Scarabaeus sacer)"),
                new Bug("Ant", 8, 10, C(0.6f, 0.2f, 0.1f), "Hormiga del desierto (Cataglyphis bicolor)"),
                new Bug("Grasshopper", 3, 65, C(0.6f, 0.5f, 0.35f), "Langosta del desierto (Schistocerca gregaria)"),
                new Bug("Mantis", 2, 75, C(0.45f, 0.68f, 0.3f), "Mantis africana (Sphodromantis viridis)"),
                new Bug("Termite", 8, 5, C(0.95f, 0.9f, 0.78f), "Termita de arena (Psammotermes hybostoma)"),
                new Bug("Antlion", 5, 12, C(0.85f, 0.75f, 0.55f), "Hormiga león (Myrmeleon)"),
                new Bug("SnowBee", 3, 12, C(0.9f, 0.7f, 0.25f), "Abeja egipcia (Apis mellifera lamarckii)"),
                new Bug("Scorpion", 3, 58, C(0.85f, 0.75f, 0.4f), "Escorpión amarillo, el acechador de la muerte (Leiurus quinquestriatus)"),
                new Bug("CamelSpider", 2, 50, C(0.78f, 0.64f, 0.42f), "Solífugo (Galeodes arabs)") } };

            // ── Newton, Curie, Einstein: un jardín templado de Europa occidental, final del verano ──
            case "Planta Atómica": return new Biome { title = "Jardín de Europa occidental (Woolsthorpe, París, Berna; final del verano)", ground = C(0.36f, 0.48f, 0.25f), plants = new[] {
                new Plant(Kind.Grass, 22, "Césped de ballico (Lolium perenne)", 15, C(0.35f, 0.6f, 0.25f), C(0.45f, 0.62f, 0.3f)),
                new Plant(Kind.Flower, 10, "Margarita (Bellis perennis)", 6, C(0.3f, 0.5f, 0.25f), C(0.98f, 0.97f, 0.95f)),
                new Plant(Kind.Flower, 10, "Trébol blanco (Trifolium repens)", 10, C(0.3f, 0.55f, 0.25f), C(0.95f, 0.95f, 0.92f)),
                new Plant(Kind.Flower, 7, "Diente de león (Taraxacum officinale)", 20, C(0.35f, 0.5f, 0.25f), C(0.98f, 0.85f, 0.15f)),
                new Plant(Kind.Shrub, 6, "Fresa silvestre (Fragaria vesca)", 12, C(0.3f, 0.5f, 0.25f), C(0.85f, 0.15f, 0.15f)),
                new Plant(Kind.Moss, 8, "Hiedra (Hedera helix)", 10, C(0.15f, 0.32f, 0.15f), C(0.2f, 0.4f, 0.2f)),
                new Plant(Kind.Stone, 10, "Grava del camino", 0, Stone, Stone), new Plant(Kind.Dung, 3, "Manzana caída (Malus domestica)", 0, C(0.75f, 0.2f, 0.15f), C(0.4f, 0.6f, 0.2f)) },
              bugs = new[] {
                new Bug("Ant", 8, 4, C(0.15f, 0.12f, 0.1f), "Hormiga negra de jardín (Lasius niger)"),
                new Bug("Aphid", 8, 2, C(0.12f, 0.12f, 0.12f), "Pulgón negro de las habas (Aphis fabae)"),
                new Bug("Ladybug", 5, 5, C(0.85f, 0.12f, 0.1f), "Mariquita de 2 puntos (Adalia bipunctata)"),
                new Bug("Grasshopper", 4, 20, C(0.5f, 0.42f, 0.3f), "Saltamontes campestre (Chorthippus brunneus)"),
                new Bug("Spider", 3, 12, C(0.65f, 0.42f, 0.2f), "Araña de jardín (Araneus diadematus)"),
                new Bug("SnowBee", 4, 18, C(0.2f, 0.18f, 0.12f), "Abejorro (Bombus terrestris)"),
                new Bug("Earwig", 4, 14, C(0.45f, 0.22f, 0.12f), "Tijereta (Forficula auricularia)"),
                new Bug("Snail", 3, 22, C(0.92f, 0.82f, 0.35f), "Caracol de labio pardo (Cepaea nemoralis)"),
                new Bug("Earthworm", 3, 150, C(0.75f, 0.42f, 0.42f), "Lombriz de tierra (Lumbricus terrestris)") } };

            // ── Ambrosio y la era del fuego: taiga prehistórica (verano corto) ──
            case "Hospital": case "Cocina": case "Ambrosio (nivel 1)": case "Sakshi (el río)": return new Biome { title = "Taiga prehistórica, antes del fuego (verano corto)", ground = C(0.3f, 0.26f, 0.18f), plants = new[] {
                new Plant(Kind.Moss, 20, "Musgo de Schreber (Pleurozium schreberi)", 10, C(0.55f, 0.62f, 0.25f), C(0.7f, 0.72f, 0.3f)),
                new Plant(Kind.Moss, 10, "Liquen de reno (Cladonia rangiferina)", 7, C(0.78f, 0.8f, 0.78f), C(0.85f, 0.86f, 0.84f)),
                new Plant(Kind.Shrub, 10, "Arándano rojo (Vaccinium vitis-idaea)", 20, C(0.15f, 0.35f, 0.18f), C(0.8f, 0.1f, 0.12f)),
                new Plant(Kind.Shrub, 8, "Arándano (Vaccinium myrtillus)", 30, C(0.3f, 0.5f, 0.25f), C(0.2f, 0.2f, 0.4f)),
                new Plant(Kind.Fern, 10, "Cola de caballo de bosque (Equisetum sylvaticum)", 40, C(0.45f, 0.65f, 0.3f), C(0.35f, 0.5f, 0.25f)),
                new Plant(Kind.Fungus, 8, "Matamoscas (Amanita muscaria)", 15, C(0.95f, 0.93f, 0.88f), C(0.85f, 0.15f, 0.1f)),
                new Plant(Kind.Flower, 5, "Linnaea (Linnaea borealis)", 8, C(0.3f, 0.5f, 0.25f), C(0.95f, 0.75f, 0.82f)),
                new Plant(Kind.Grass, 8, "Hierba del claro", 20, C(0.55f, 0.65f, 0.3f), C(0.7f, 0.65f, 0.4f)),
                new Plant(Kind.Stone, 10, "Canto rodado", 0, Stone, Stone), new Plant(Kind.Sand, 3, "Arena de pinar", 0, Sand, Sand), new Plant(Kind.Dung, 3, "Excremento de alce", 0, DungC, DungC) },
              bugs = new[] {
                new Bug("Ant", 8, 7, C(0.65f, 0.22f, 0.12f), "Hormiga de bosque (Formica aquilonia)"),
                new Bug("Aphid", 8, 4, C(0.32f, 0.24f, 0.18f), "Pulgón del abeto (Cinara), que ordeñan las hormigas"),
                new Bug("Ladybug", 4, 8, C(0.85f, 0.2f, 0.12f), "Mariquita de ojos (Anatis ocellata)"),
                new Bug("Spider", 4, 7, C(0.3f, 0.22f, 0.16f), "Araña lobo (Pardosa)"),
                new Bug("SnowBee", 3, 16, C(0.25f, 0.2f, 0.12f), "Abejorro boreal (Bombus)"),
                new Bug("DungBeetle", 4, 6, C(0.2f, 0.15f, 0.1f), "Afodio (Aphodius), en el excremento de alce"),
                new Bug("Antlion", 2, 13, C(0.6f, 0.55f, 0.45f), "Hormiga león (Myrmeleon formicarius), en el arenal soleado") } };

            // ── Palenque (Lakamha'), selva alta perennifolia de Chiapas, finales de agosto (lluvias) ──
            // Flora: ceiba, ramón, cacao, palma de corozo, helechos, bromelias; la milpa (maíz, frijol, calabaza). Fauna: hormiga
            // cortadora (Atta cephalotes), abeja sin aguijón de los mayas (Melipona beecheii, la xunáan kab), cigarra gigante (Quesada
            // gigas), luciérnaga, saltamontes de la milpa y la araña de seda dorada (Trichonephila clavipes). (CONABIO, Reserva de Palenque.)
            case "Palenque": return new Biome { title = "Palenque, selva alta de Chiapas (agosto, lluvias)", ground = C(0.42f, 0.3f, 0.2f), plants = new[] {
                new Plant(Kind.Fern, 14, "Helecho arborescente (Cyathea)", 300, C(0.25f, 0.5f, 0.25f), C(0.35f, 0.3f, 0.2f)),
                new Plant(Kind.Shrub, 10, "Ramón (Brosimum alicastrum)", 1500, C(0.2f, 0.42f, 0.2f), C(0.85f, 0.6f, 0.2f)),
                new Plant(Kind.Shrub, 6, "Ceiba (Ceiba pentandra)", 2000, C(0.25f, 0.45f, 0.22f), C(0.62f, 0.6f, 0.52f)),
                new Plant(Kind.Shrub, 8, "Cacao (Theobroma cacao)", 500, C(0.2f, 0.38f, 0.18f), C(0.85f, 0.5f, 0.15f)),
                new Plant(Kind.Grass, 14, "Maíz de la milpa (Zea mays)", 220, C(0.45f, 0.65f, 0.25f), C(0.95f, 0.8f, 0.3f)),
                new Plant(Kind.Flower, 7, "Bromelia (Aechmea bracteata)", 80, C(0.3f, 0.5f, 0.25f), C(0.85f, 0.2f, 0.3f)),
                new Plant(Kind.Fungus, 5, "Hongo de la selva (Marasmius)", 6, C(0.9f, 0.85f, 0.75f), C(0.75f, 0.35f, 0.25f)),
                new Plant(Kind.Moss, 6, "Musgo de la piedra caliza", 3, C(0.3f, 0.5f, 0.22f), C(0.45f, 0.6f, 0.25f)),
                new Plant(Kind.Stone, 6, "Caliza", 0, Stone, Stone) },
              town = new[] {
                new Plant(Kind.Shrub, 7, "Cacao del huerto (Theobroma cacao)", 500, C(0.2f, 0.38f, 0.18f), C(0.85f, 0.5f, 0.15f)),
                new Plant(Kind.Shrub, 5, "Aguacate (Persea americana)", 900, C(0.2f, 0.36f, 0.18f), C(0.2f, 0.35f, 0.15f)),
                new Plant(Kind.Shrub, 4, "Ceiba de la plaza (Ceiba pentandra)", 2000, C(0.25f, 0.45f, 0.22f), C(0.62f, 0.6f, 0.52f)),
                new Plant(Kind.Grass, 12, "Maíz del solar (Zea mays)", 200, C(0.45f, 0.65f, 0.25f), C(0.95f, 0.8f, 0.3f)),
                new Plant(Kind.Flower, 5, "Calabaza (Cucurbita moschata)", 40, C(0.3f, 0.55f, 0.25f), C(0.95f, 0.65f, 0.15f)),
                new Plant(Kind.Grass, 10, "Césped de grama (Paspalum)", 5, C(0.4f, 0.6f, 0.28f), C(0.5f, 0.6f, 0.3f)) },
              bugs = new[] {
                new Bug("Ant", 10, 12, C(0.6f, 0.3f, 0.15f), "Hormiga cortadora de hojas (Atta cephalotes)"),
                new Bug("SnowBee", 5, 9, C(0.55f, 0.4f, 0.2f), "Abeja sin aguijón de los mayas, xunáan kab (Melipona beecheii)"),
                new Bug("Cicada", 3, 60, C(0.35f, 0.3f, 0.2f), "Cigarra gigante (Quesada gigas)"),
                new Bug("Firefly", 4, 12, C(0.3f, 0.22f, 0.15f), "Luciérnaga (Photinus)"),
                new Bug("Grasshopper", 4, 40, C(0.4f, 0.6f, 0.25f), "Saltamontes de la milpa (Sphenarium)"),
                new Bug("Spider", 2, 40, C(0.85f, 0.7f, 0.2f), "Araña de seda dorada (Trichonephila clavipes)") },
              townFauna = new[] {
                new Pet("Woodlouse", 3, 0.6f, C(0.55f, 0.45f, 0.35f), "Perro del solar (en el microcosmos, una cochinilla)"),
                new Pet("Ladybug", 3, 0.8f, C(0.2f, 0.25f, 0.3f), "Pavo de monte (en el microcosmos, una mariquita oscura)") } };

            // ── Hong Kong, ladera subtropical, mediados de septiembre (Bruce Lee empieza en la escuela de Yip Man) ──
            // Flora y fauna nativas comunes del sur de China y Hong Kong (Hong Kong Herbarium, AFCD; Hong Kong Biodiversity Database).
            case "Bruce Lee": return new Biome { title = "Hong Kong, ladera subtropical (septiembre)", ground = C(0.58f, 0.42f, 0.3f), plants = new[] {
                new Plant(Kind.Grass, 16, "Grama (Cynodon dactylon)", 20, C(0.4f, 0.62f, 0.28f), C(0.5f, 0.6f, 0.3f)),
                new Plant(Kind.Grass, 12, "Cogón (Imperata cylindrica)", 80, C(0.5f, 0.65f, 0.35f), C(0.95f, 0.95f, 0.92f)),
                new Plant(Kind.Fern, 12, "Helecho dicotómico (Dicranopteris pedata)", 100, C(0.35f, 0.55f, 0.25f), C(0.4f, 0.5f, 0.3f)),
                new Plant(Kind.Flower, 8, "Mirto de rosa (Rhodomyrtus tomentosa)", 150, C(0.4f, 0.5f, 0.35f), C(0.92f, 0.55f, 0.7f)),
                new Plant(Kind.Flower, 7, "Melastoma (Melastoma malabathricum)", 150, C(0.35f, 0.48f, 0.3f), C(0.7f, 0.45f, 0.8f)),
                new Plant(Kind.Fungus, 5, "Seta (Termitomyces)", 15, C(0.9f, 0.86f, 0.75f), C(0.8f, 0.7f, 0.55f)),
                new Plant(Kind.Stone, 10, "Granito", 0, Stone, Stone), new Plant(Kind.Sand, 4, "Arena de la bahía", 0, Sand, Sand) },
              // Kowloon (Yau Ma Tei, 1957) y los barrios de Seattle: lo plantado en la ciudad. El baniano de calle, la bauhinia (el
              // árbol de orquídeas de Hong Kong, plantado por toda la colonia desde 1914), setos de ixora, céspedes de grama y
              // buganvillas (Hong Kong Herbarium; LCSD, «Bauhinia blakeana»). La ladera silvestre (arriba) queda para las afueras.
              town = new[] {
                new Plant(Kind.Shrub, 6, "Baniano de calle (Ficus microcarpa), plantado", 1400, C(0.22f, 0.42f, 0.2f), C(0.45f, 0.4f, 0.34f)),
                new Plant(Kind.Flower, 5, "Árbol de orquídeas de Hong Kong (Bauhinia × blakeana), plantado", 800, C(0.3f, 0.48f, 0.25f), C(0.75f, 0.3f, 0.6f)),
                new Plant(Kind.Shrub, 8, "Seto de ixora (Ixora chinensis)", 100, C(0.2f, 0.4f, 0.2f), C(0.9f, 0.35f, 0.2f)),
                new Plant(Kind.Grass, 14, "Césped de grama (Cynodon dactylon), segado", 5, C(0.4f, 0.62f, 0.28f), C(0.5f, 0.6f, 0.3f)),
                new Plant(Kind.Flower, 4, "Buganvilla (Bougainvillea glabra)", 150, C(0.3f, 0.45f, 0.25f), C(0.85f, 0.2f, 0.55f)) },
              bugs = new[] {
                new Bug("Termite", 8, 6, C(0.92f, 0.86f, 0.72f), "Termita de Formosa (Coptotermes formosanus)"),
                new Bug("Grasshopper", 5, 30, C(0.45f, 0.65f, 0.25f), "Saltamontes del arroz (Oxya chinensis)"),
                new Bug("Ladybug", 5, 5, C(0.95f, 0.6f, 0.1f), "Mariquita zigzag (Cheilomenes sexmaculata)"),
                new Bug("Mantis", 3, 80, C(0.4f, 0.7f, 0.3f), "Mantis (Hierodula patellifera)"),
                new Bug("Spider", 3, 40, C(0.85f, 0.75f, 0.2f), "Araña de seda dorada (Nephila pilipes)"),
                new Bug("Firefly", 4, 10, C(0.3f, 0.22f, 0.15f), "Luciérnaga (Luciola)") } };

            // ══ CIENTÍFICOS E HISTÓRICOS (docs/microcosmos-cientificos.md) ══════════════════════════════════════════════
            // En las CIUDADES (París 1898, Berna 1905, Pasadena 1953, Bagdad ~820) no hay flora ni fauna salvaje: solo lo plantado
            // por la gente (árboles de calle, jardines, palmeras) y los animales del pueblo (townFauna: caballos, asnos, gatos…,
            // con el cuerpo de su análogo), ningún bicho suelto. En el campo (Woolsthorpe, Berkeley, Sils
            // Maria, Crotona, el Nilo, la pradera de Colorado, el Orinoco, el pinar de Polesia) sí: la de ese lugar y estación.

            // ── Woolsthorpe (Lincolnshire), verano de 1666: prados de ovejas y el huerto de manzanos ──
            case "Woolsthorpe": return new Biome { title = "Woolsthorpe, Lincolnshire, verano de 1666", ground = C(0.38f, 0.48f, 0.26f), plants = new[] {
                new Plant(Kind.Grass, 20, "Ballico (Lolium perenne)", 30, C(0.4f, 0.6f, 0.28f), C(0.55f, 0.6f, 0.35f)),
                new Plant(Kind.Flower, 10, "Trébol blanco (Trifolium repens)", 15, C(0.3f, 0.55f, 0.25f), C(0.97f, 0.97f, 0.95f)),
                new Plant(Kind.Flower, 6, "Botón de oro (Ranunculus acris)", 50, C(0.35f, 0.55f, 0.25f), C(0.98f, 0.85f, 0.1f)),
                new Plant(Kind.Shrub, 8, "Majuelo del seto (Crataegus monogyna)", 300, C(0.25f, 0.42f, 0.2f), C(0.7f, 0.12f, 0.1f)),
                new Plant(Kind.Shrub, 6, "Manzano (Malus domestica), con manzanas caídas", 400, C(0.3f, 0.48f, 0.22f), C(0.75f, 0.2f, 0.12f)),
                new Plant(Kind.Moss, 6, "Musgo de pared (Tortula muralis)", 2, C(0.45f, 0.55f, 0.25f), C(0.55f, 0.6f, 0.3f)),
                new Plant(Kind.Stone, 8, "Piedra caliza", 0, C(0.82f, 0.78f, 0.65f), Stone), new Plant(Kind.Dung, 4, "Excremento de oveja", 0, DungC, DungC) },
              bugs = new[] {
                new Bug("Ant", 6, 4, C(0.15f, 0.12f, 0.1f), "Hormiga negra (Lasius niger)"),
                new Bug("Aphid", 6, 2, C(0.35f, 0.45f, 0.25f), "Pulgón verde del manzano (Aphis pomi)"),
                new Bug("Earwig", 4, 14, C(0.35f, 0.18f, 0.1f), "Tijereta (Forficula auricularia), dentro de las manzanas caídas"),
                new Bug("Ladybug", 4, 7, C(0.85f, 0.12f, 0.1f), "Mariquita de 7 puntos (Coccinella septempunctata)"),
                new Bug("SnowBee", 3, 18, C(0.15f, 0.12f, 0.08f), "Abejorro común (Bombus terrestris)"),
                new Bug("Grasshopper", 4, 20, C(0.5f, 0.45f, 0.3f), "Saltamontes de campo (Chorthippus brunneus)"),
                new Bug("Spider", 3, 12, C(0.55f, 0.4f, 0.25f), "Araña de jardín (Araneus diadematus)"),
                new Bug("DungBeetle", 3, 7, C(0.15f, 0.12f, 0.1f), "Afodio (Aphodius), en el excremento de oveja") } };

            // ── Berkeley (Gloucestershire), 14 de mayo de 1796: el valle del Severn, prados de vacas lecheras en primavera ──
            //    En el microcosmos, las VACAS son PULGONES (las hormigas ordeñan pulgones: docs/natural-interactions.md).
            case "Berkeley": return new Biome { title = "Berkeley, valle del Severn, primavera de 1796", ground = C(0.36f, 0.46f, 0.24f), plants = new[] {
                new Plant(Kind.Grass, 20, "Cola de zorra (Alopecurus pratensis)", 50, C(0.4f, 0.6f, 0.3f), C(0.6f, 0.6f, 0.45f)),
                new Plant(Kind.Flower, 8, "Prímula (Primula veris)", 20, C(0.35f, 0.55f, 0.25f), C(0.98f, 0.82f, 0.2f)),
                new Plant(Kind.Flower, 8, "Botón de oro (Ranunculus acris)", 50, C(0.35f, 0.55f, 0.25f), C(0.98f, 0.85f, 0.1f)),
                new Plant(Kind.Shrub, 8, "Majuelo en flor (Crataegus monogyna)", 300, C(0.28f, 0.45f, 0.22f), C(0.97f, 0.95f, 0.92f)),
                new Plant(Kind.Fern, 4, "Helecho macho (Dryopteris filix-mas)", 80, C(0.3f, 0.5f, 0.25f), C(0.35f, 0.45f, 0.25f)),
                new Plant(Kind.Moss, 6, "Musgo (Kindbergia praelonga)", 3, C(0.4f, 0.55f, 0.25f), C(0.5f, 0.6f, 0.3f)),
                new Plant(Kind.Stone, 6, "Piedra arenisca", 0, C(0.7f, 0.55f, 0.45f), Stone), new Plant(Kind.Dung, 5, "Boñiga de vaca", 0, DungC, DungC) },
              bugs = new[] {
                new Bug("Ant", 6, 4, C(0.6f, 0.4f, 0.15f), "Hormiga amarilla (Lasius flavus), la que ordeña pulgones de raíz"),
                new Bug("SnowBee", 4, 14, C(0.6f, 0.45f, 0.25f), "Abejorro cardador (Bombus pascuorum)"),
                new Bug("Ladybug", 4, 7, C(0.85f, 0.12f, 0.1f), "Mariquita de 7 puntos (Coccinella septempunctata)"),
                new Bug("DungBeetle", 4, 9, C(0.15f, 0.12f, 0.1f), "Afodio (Aphodius rufipes), en la boñiga"),
                new Bug("Spider", 3, 8, C(0.3f, 0.25f, 0.2f), "Araña lobo (Pardosa amentata)"),
                new Bug("Woodlouse", 3, 12, C(0.42f, 0.43f, 0.45f), "Cochinilla (Oniscus asellus)") } };

            // ── Sils Maria (Alta Engadina, 1800 m), principios de agosto de 1881: prados alpinos, alerces y el lago de Silvaplana ──
            case "Sils Maria": return new Biome { title = "Sils Maria, Alta Engadina (agosto de 1881)", ground = C(0.4f, 0.42f, 0.3f), plants = new[] {
                new Plant(Kind.Grass, 16, "Cervuno (Nardus stricta)", 25, C(0.45f, 0.58f, 0.32f), C(0.55f, 0.55f, 0.4f)),
                new Plant(Kind.Shrub, 12, "Rododendro alpino (Rhododendron ferrugineum)", 80, C(0.2f, 0.35f, 0.2f), C(0.85f, 0.3f, 0.45f)),
                new Plant(Kind.Flower, 8, "Genciana de Koch (Gentiana acaulis)", 8, C(0.3f, 0.45f, 0.25f), C(0.15f, 0.25f, 0.8f)),
                new Plant(Kind.Flower, 6, "Edelweiss (Leontopodium nivale)", 10, C(0.75f, 0.78f, 0.72f), C(0.95f, 0.95f, 0.9f)),
                new Plant(Kind.Fern, 6, "Pino cembro joven (Pinus cembra)", 150, C(0.2f, 0.38f, 0.22f), C(0.3f, 0.3f, 0.25f)),
                new Plant(Kind.Moss, 8, "Liquen de mapa (Rhizocarpon geographicum)", 1, C(0.75f, 0.8f, 0.2f), C(0.15f, 0.15f, 0.15f)),
                new Plant(Kind.Stone, 14, "Gneis y granito", 0, Stone, Stone) },
              bugs = new[] {
                new Bug("Grasshopper", 5, 20, C(0.45f, 0.5f, 0.3f), "Saltamontes alpino sin alas (Podisma pedestris)"),
                new Bug("SnowBee", 4, 16, C(0.15f, 0.12f, 0.08f), "Abejorro alpino (Bombus alpinus)"),
                new Bug("Ant", 6, 7, C(0.55f, 0.2f, 0.12f), "Hormiga de los montes (Formica lugubris)"),
                new Bug("Spider", 3, 7, C(0.3f, 0.25f, 0.2f), "Araña lobo (Pardosa)"),
                new Bug("Ladybug", 3, 6, C(0.85f, 0.2f, 0.12f), "Mariquita (Coccinella)"),
                new Bug("Scorpionfly", 2, 5, C(0.2f, 0.18f, 0.12f), "Mosca escorpión de la nieve (Boreus), quieta en el musgo hasta el invierno", dormant: "en verano la larva espera en el musgo; el adulto sale con la nieve") } };

            // ── Crotona (Calabria), verano, ~530 a. C.: maquis mediterráneo, olivos y el cabo Lacinio ──
            case "Crotona": return new Biome { title = "Crotona, Magna Grecia (verano, ~530 a. C.)", ground = C(0.7f, 0.55f, 0.38f), plants = new[] {
                new Plant(Kind.Shrub, 14, "Olivo (Olea europaea)", 400, C(0.45f, 0.52f, 0.38f), C(0.3f, 0.32f, 0.2f)),
                new Plant(Kind.Shrub, 10, "Lentisco (Pistacia lentiscus)", 150, C(0.25f, 0.4f, 0.22f), C(0.7f, 0.15f, 0.12f)),
                new Plant(Kind.Flower, 8, "Tomillo (Thymus capitatus)", 25, C(0.4f, 0.48f, 0.3f), C(0.75f, 0.5f, 0.75f)),
                new Plant(Kind.Grass, 12, "Esparto seco (Lygeum spartum)", 50, C(0.75f, 0.68f, 0.42f), C(0.8f, 0.72f, 0.5f)),
                new Plant(Kind.Flower, 4, "Haba (Vicia faba): los pitagóricos no la comían", 60, C(0.3f, 0.5f, 0.25f), C(0.95f, 0.95f, 0.95f)),
                new Plant(Kind.Stone, 10, "Arenisca", 0, C(0.8f, 0.7f, 0.5f), Stone), new Plant(Kind.Sand, 6, "Arena del Jónico", 0, Sand, Sand) },
              bugs = new[] {
                new Bug("Cicada", 5, 25, C(0.45f, 0.4f, 0.3f), "Cigarra del fresno (Cicada orni), cantando en los olivos"),
                new Bug("Ant", 6, 8, C(0.2f, 0.12f, 0.1f), "Hormiga segadora (Messor barbarus)"),
                new Bug("SnowBee", 4, 13, C(0.65f, 0.45f, 0.15f), "Abeja de la miel (Apis mellifera ligustica)"),
                new Bug("Mantis", 2, 60, C(0.45f, 0.65f, 0.3f), "Mantis religiosa (Mantis religiosa)"),
                new Bug("Grasshopper", 4, 30, C(0.6f, 0.55f, 0.35f), "Langosta egipcia (Anacridium aegyptium)"),
                new Bug("Scorpion", 2, 40, C(0.2f, 0.15f, 0.1f), "Escorpión (Euscorpius italicus), bajo las piedras") } };

            // ── Menfis y el Nilo, verano (inicio de la crecida), ~450 a. C. (Heródoto, Historias II) ──
            case "Menfis": return new Biome { title = "Menfis, orilla del Nilo (verano, ~450 a. C.)", ground = C(0.42f, 0.34f, 0.25f), plants = new[] {
                new Plant(Kind.Fern, 14, "Papiro (Cyperus papyrus)", 300, C(0.35f, 0.58f, 0.28f), C(0.55f, 0.6f, 0.3f)),
                new Plant(Kind.Shrub, 10, "Palmera datilera (Phoenix dactylifera)", 1500, C(0.35f, 0.5f, 0.25f), C(0.75f, 0.45f, 0.15f)),
                new Plant(Kind.Flower, 8, "Loto azul (Nymphaea caerulea), en las acequias", 10, C(0.25f, 0.45f, 0.3f), C(0.5f, 0.65f, 0.95f)),
                new Plant(Kind.Grass, 12, "Trigo emmer (Triticum dicoccum)", 90, C(0.75f, 0.68f, 0.35f), C(0.82f, 0.72f, 0.4f)),
                new Plant(Kind.Grass, 6, "Caña (Phragmites australis)", 250, C(0.45f, 0.55f, 0.3f), C(0.6f, 0.5f, 0.4f)),
                new Plant(Kind.Sand, 10, "Limo negro del Nilo", 0, C(0.25f, 0.2f, 0.15f), C(0.25f, 0.2f, 0.15f)), new Plant(Kind.Sand, 6, "Arena del desierto", 0, Sand, Sand), new Plant(Kind.Dung, 4, "Excremento de asno", 0, DungC, DungC) },
              bugs = new[] {
                new Bug("DungBeetle", 5, 28, C(0.1f, 0.1f, 0.1f), "Escarabajo sagrado (Scarabaeus sacer)"),
                new Bug("Grasshopper", 5, 55, C(0.75f, 0.65f, 0.3f), "Langosta del desierto (Schistocerca gregaria)"),
                new Bug("Ant", 6, 6, C(0.3f, 0.15f, 0.1f), "Hormiga (Cataglyphis bicolor), la que corre al sol del mediodía"),
                new Bug("Mantis", 2, 50, C(0.75f, 0.65f, 0.45f), "Mantis del desierto (Eremiaphila)"),
                new Bug("Scorpion", 2, 90, C(0.8f, 0.7f, 0.35f), "Escorpión amarillo (Leiurus quinquestriatus), de noche"),
                new Bug("Cricket", 3, 20, C(0.3f, 0.22f, 0.15f), "Grillo (Gryllus bimaculatus)") } };

            // ── Colorado Springs, 3 de julio de 1899: pradera de hierba corta al pie de Pikes Peak, tormenta de la tarde ──
            case "Colorado Springs": return new Biome { title = "Colorado Springs, pradera de hierba corta (julio de 1899)", ground = C(0.6f, 0.48f, 0.35f), plants = new[] {
                new Plant(Kind.Grass, 20, "Grama azul (Bouteloua gracilis)", 25, C(0.5f, 0.6f, 0.4f), C(0.55f, 0.45f, 0.35f)),
                new Plant(Kind.Grass, 10, "Hierba del búfalo (Bouteloua dactyloides)", 12, C(0.5f, 0.6f, 0.38f), C(0.6f, 0.6f, 0.45f)),
                new Plant(Kind.Shrub, 8, "Yuca (Yucca glauca)", 80, C(0.4f, 0.52f, 0.4f), C(0.95f, 0.95f, 0.85f)),
                new Plant(Kind.Shrub, 6, "Nopal de la pradera (Opuntia polyacantha)", 25, C(0.4f, 0.55f, 0.3f), C(0.95f, 0.85f, 0.3f)),
                new Plant(Kind.Flower, 6, "Margarita de la pradera (Ratibida columnifera)", 50, C(0.35f, 0.5f, 0.28f), C(0.8f, 0.25f, 0.1f)),
                new Plant(Kind.Stone, 10, "Arenisca roja", 0, C(0.7f, 0.4f, 0.3f), Stone) },
              bugs = new[] {
                new Bug("Grasshopper", 6, 25, C(0.55f, 0.5f, 0.35f), "Saltamontes migratorio (Melanoplus sanguinipes)"),
                new Bug("Ant", 6, 7, C(0.6f, 0.25f, 0.12f), "Hormiga segadora (Pogonomyrmex occidentalis), con su montículo de grava"),
                new Bug("Ladybug", 4, 6, C(0.9f, 0.35f, 0.1f), "Mariquita convergente (Hippodamia convergens)"),
                new Bug("SnowBee", 3, 18, C(0.85f, 0.65f, 0.2f), "Abejorro (Bombus huntii)"),
                new Bug("Spider", 3, 15, C(0.4f, 0.32f, 0.25f), "Araña lobo (Hogna)"),
                new Bug("DungBeetle", 3, 15, C(0.15f, 0.25f, 0.15f), "Escarabajo pelotero (Phanaeus)") } };

            // ── Angostura del Orinoco, 15 de febrero de 1819: sabana en estación seca, lajas de granito, palmar de moriche ──
            case "Angostura": return new Biome { title = "Angostura del Orinoco (febrero de 1819, estación seca)", ground = C(0.65f, 0.45f, 0.3f), plants = new[] {
                new Plant(Kind.Grass, 16, "Paja de sabana (Trachypogon spicatus)", 80, C(0.75f, 0.68f, 0.4f), C(0.7f, 0.55f, 0.35f)),
                new Plant(Kind.Fern, 8, "Moriche (Mauritia flexuosa), en el morichal", 2000, C(0.3f, 0.5f, 0.25f), C(0.6f, 0.3f, 0.15f)),
                new Plant(Kind.Shrub, 8, "Chaparro (Curatella americana)", 400, C(0.35f, 0.45f, 0.25f), C(0.45f, 0.35f, 0.25f)),
                new Plant(Kind.Flower, 6, "Araguaney en flor (Handroanthus chrysanthus)", 800, C(0.4f, 0.35f, 0.25f), C(0.98f, 0.85f, 0.1f)),
                new Plant(Kind.Flower, 4, "Bromelia (Bromelia chrysantha)", 60, C(0.35f, 0.5f, 0.3f), C(0.9f, 0.3f, 0.2f)),
                new Plant(Kind.Stone, 14, "Laja de granito del escudo guayanés", 0, C(0.5f, 0.45f, 0.42f), Stone), new Plant(Kind.Sand, 6, "Arena del Orinoco", 0, Sand, Sand) },
              bugs = new[] {
                new Bug("Ant", 8, 12, C(0.55f, 0.25f, 0.12f), "Bachaco (Atta sexdens), hormiga cortadora de hojas"),
                new Bug("Termite", 6, 5, C(0.4f, 0.3f, 0.2f), "Comején (Nasutitermes), con su nido en los chaparros"),
                new Bug("Grasshopper", 4, 45, C(0.45f, 0.55f, 0.25f), "Saltamontes (Tropidacris collaris)"),
                new Bug("Mantis", 2, 60, C(0.4f, 0.6f, 0.3f), "Mantis (Stagmatoptera)"),
                new Bug("Firefly", 4, 12, C(0.3f, 0.25f, 0.15f), "Cocuyo (Pyrophorus), de noche"),
                new Bug("Spider", 2, 30, C(0.75f, 0.6f, 0.2f), "Araña de seda dorada (Trichonephila clavipes)") } };

            // ── Prípiat y la central de Chernóbil, principios de mayo de 1986: pinar de Polesia en primavera (el «bosque rojo») ──
            case "Chernóbil": return new Biome { title = "Prípiat, pinar de Polesia (mayo de 1986)", ground = C(0.62f, 0.55f, 0.4f), plants = new[] {
                new Plant(Kind.Fern, 14, "Pino silvestre (Pinus sylvestris)", 1500, C(0.25f, 0.4f, 0.22f), C(0.55f, 0.35f, 0.2f)),
                new Plant(Kind.Shrub, 8, "Abedul (Betula pendula), con hoja nueva", 800, C(0.45f, 0.65f, 0.3f), C(0.92f, 0.92f, 0.9f)),
                new Plant(Kind.Moss, 14, "Musgo de pinar (Pleurozium schreberi)", 10, C(0.55f, 0.62f, 0.25f), C(0.7f, 0.72f, 0.3f)),
                new Plant(Kind.Grass, 10, "Hierba de arenal (Corynephorus canescens)", 25, C(0.6f, 0.65f, 0.55f), C(0.65f, 0.55f, 0.5f)),
                new Plant(Kind.Flower, 4, "Diente de león (Taraxacum officinale)", 15, C(0.35f, 0.5f, 0.25f), C(0.98f, 0.85f, 0.15f)),
                new Plant(Kind.Sand, 12, "Arena fluvioglacial", 0, Sand, Sand) },
              bugs = new[] {
                new Bug("Ant", 8, 8, C(0.6f, 0.25f, 0.12f), "Hormiga roja de bosque (Formica rufa)"),
                new Bug("Ladybug", 4, 7, C(0.85f, 0.12f, 0.1f), "Mariquita de 7 puntos (Coccinella septempunctata)"),
                new Bug("Spider", 3, 8, C(0.3f, 0.25f, 0.2f), "Araña lobo (Pardosa)"),
                new Bug("Antlion", 2, 12, C(0.6f, 0.55f, 0.45f), "Hormiga león (Myrmeleon formicarius), en la arena del pinar"),
                new Bug("SnowBee", 3, 16, C(0.15f, 0.12f, 0.08f), "Abejorro (Bombus terrestris)") } };

            // ── Las CIUDADES: sin flora ni bichos salvajes; solo lo plantado (a escala de pueblo: AreaMicrocosmos.Nature, modo
            //    pueblo) y los ANIMALES DEL PUEBLO (townFauna) con el cuerpo de su análogo (docs/microcosmos-fauna-eras.md):
            //    · bestias de tiro y de carga (caballo, mula, asno, camello) → PULGÓN, como las vacas de Berkeley (las hormigas «crían
            //      ganado» de pulgones), a la talla del animal (caballo 2,4 m, asno 1,6 m, camello 3 m);
            //    · gatos y perros de las casas → COCHINILLA hormiguera (Platyarthrus hoffmannseggii, el huésped blanco que vive en los
            //      hormigueros con permiso de las hormigas: el animal de casa), a 0,5–0,7 m;
            //    · ratas de los sótanos y las alcantarillas → CUCARACHA (la misma plaga urbana), a 0,3 m.
            //    Ninguno usa un cuerpo de la gente (InsectCast.Folk), para no confundirse con ella. Las aves (palomas, gorriones) no
            //    tienen análogo con alas fuera de la gente: no se ponen.
            case "París 1898": return new Biome { title = "París, rue Lhomond (diciembre de 1898): ciudad, sin naturaleza salvaje", winter = true, ground = C(0.5f, 0.48f, 0.45f), plants = new[] {
                new Plant(Kind.Shrub, 6, "Plátano de sombra del bulevar (Platanus × hispanica), plantado", 1500, C(0.5f, 0.45f, 0.35f), C(0.6f, 0.55f, 0.45f)),
                new Plant(Kind.Stone, 20, "Adoquín", 0, C(0.45f, 0.45f, 0.48f), Stone) },
              // Diciembre: los árboles del bulevar sin hoja (ramas pardas), el césped de los squares de Haussmann, setos de boj y
              // hiedra en las verjas (Alphand, «Les Promenades de Paris», 1867–73).
              town = new[] {
                new Plant(Kind.Shrub, 8, "Plátano de sombra del bulevar (Platanus × hispanica), sin hoja en diciembre", 1500, C(0.5f, 0.45f, 0.38f), C(0.62f, 0.57f, 0.48f)),
                new Plant(Kind.Shrub, 4, "Castaño de Indias del square (Aesculus hippocastanum), sin hoja", 1500, C(0.42f, 0.36f, 0.3f), C(0.5f, 0.42f, 0.34f)),
                new Plant(Kind.Grass, 10, "Césped del square, de invierno", 5, C(0.38f, 0.5f, 0.3f), C(0.45f, 0.52f, 0.35f)),
                new Plant(Kind.Shrub, 6, "Seto de boj (Buxus sempervirens)", 80, C(0.2f, 0.35f, 0.18f), C(0.25f, 0.4f, 0.2f)),
                new Plant(Kind.Moss, 4, "Hiedra de la verja (Hedera helix)", 10, C(0.15f, 0.32f, 0.15f), C(0.2f, 0.4f, 0.2f)) },
              // Los ómnibus y los fiacres de caballos (la Compagnie générale des omnibus tenía unos 17 000 caballos hacia 1900), los
              // gatos de los patios y las ratas del alcantarillado de Belgrand.
              townFauna = new[] {
                new Pet("Aphid", 4, 2.4f, C(0.45f, 0.3f, 0.2f), "Caballo de fiacre (en el microcosmos, un pulgón de tiro)"),
                new Pet("Woodlouse", 2, 0.5f, C(0.85f, 0.83f, 0.78f), "Gato del patio (en el microcosmos, una cochinilla hormiguera)"),
                new Pet("Cockroach", 3, 0.3f, C(0.25f, 0.15f, 0.08f), "Rata del alcantarillado (en el microcosmos, una cucaracha)") },
              bugs = new Bug[0] };
            case "Berna": return new Biome { title = "Berna (mayo de 1905): ciudad, sin naturaleza salvaje", ground = C(0.55f, 0.52f, 0.48f), plants = new[] {
                new Plant(Kind.Shrub, 6, "Tilo de la plaza (Tilia platyphyllos), plantado", 1500, C(0.35f, 0.55f, 0.28f), C(0.75f, 0.75f, 0.4f)),
                new Plant(Kind.Flower, 3, "Geranios de los balcones (Pelargonium), plantados", 30, C(0.3f, 0.5f, 0.25f), C(0.85f, 0.15f, 0.15f)),
                new Plant(Kind.Stone, 20, "Adoquín de arenisca", 0, C(0.7f, 0.65f, 0.5f), Stone) },
              // Mayo: los tilos de las plazas, los castaños de Indias en flor de la Münsterplattform y la Kleine Schanze, sus céspedes
              // y setos, y los geranios rojos de las jardineras de la Kramgasse (Stadtgrün Bern, «Münsterplattform»).
              town = new[] {
                new Plant(Kind.Shrub, 6, "Tilo de la plaza (Tilia platyphyllos), plantado", 1500, C(0.35f, 0.55f, 0.28f), C(0.75f, 0.75f, 0.4f)),
                new Plant(Kind.Shrub, 5, "Castaño de Indias en flor de la Münsterplattform (Aesculus hippocastanum)", 1800, C(0.25f, 0.45f, 0.22f), C(0.97f, 0.95f, 0.9f)),
                new Plant(Kind.Grass, 10, "Césped de la Kleine Schanze", 6, C(0.38f, 0.6f, 0.28f), C(0.45f, 0.62f, 0.3f)),
                new Plant(Kind.Shrub, 5, "Seto de boj (Buxus sempervirens)", 80, C(0.2f, 0.35f, 0.18f), C(0.25f, 0.4f, 0.2f)),
                new Plant(Kind.Flower, 6, "Geranios de las jardineras (Pelargonium), plantados", 30, C(0.3f, 0.5f, 0.25f), C(0.85f, 0.15f, 0.15f)) },
              // Los caballos de los carros de reparto (la cerveza, la leche: el tranvía era eléctrico desde 1901, los carros no) y los
              // gatos de las casas de soportales. Los osos del Bärengraben (desde 1857) viven en su foso, fuera del piso.
              townFauna = new[] {
                new Pet("Aphid", 3, 2.4f, C(0.5f, 0.32f, 0.2f), "Caballo del carro de reparto (en el microcosmos, un pulgón de tiro)"),
                new Pet("Woodlouse", 3, 0.5f, C(0.85f, 0.83f, 0.78f), "Gato de la Kramgasse (en el microcosmos, una cochinilla hormiguera)") },
              bugs = new Bug[0] };
            case "Pasadena": return new Biome { title = "Pasadena (1953): ciudad, sin naturaleza salvaje", ground = C(0.6f, 0.58f, 0.52f), plants = new[] {
                new Plant(Kind.Shrub, 6, "Palmera de abanico (Washingtonia robusta), plantada en la avenida", 2000, C(0.35f, 0.5f, 0.28f), C(0.55f, 0.45f, 0.35f)),
                new Plant(Kind.Grass, 8, "Césped del campus (Cynodon dactylon), segado", 5, C(0.4f, 0.62f, 0.28f), C(0.5f, 0.6f, 0.3f)),
                new Plant(Kind.Stone, 12, "Bordillo de hormigón", 0, C(0.75f, 0.74f, 0.7f), Stone) },
              // Las avenidas de palmeras, los cedros del Himalaya de la «Christmas Tree Lane» (1885), las encinas de la costa, los
              // céspedes de las casas y los rosales (la ciudad del Torneo de las Rosas) (City of Pasadena, «Street Trees»).
              town = new[] {
                new Plant(Kind.Shrub, 6, "Palmera de abanico (Washingtonia robusta), plantada en la avenida", 2000, C(0.35f, 0.5f, 0.28f), C(0.55f, 0.45f, 0.35f)),
                new Plant(Kind.Shrub, 3, "Cedro del Himalaya (Cedrus deodara), plantado", 1800, C(0.25f, 0.38f, 0.3f), C(0.3f, 0.35f, 0.3f)),
                new Plant(Kind.Shrub, 3, "Encina de la costa (Quercus agrifolia)", 1200, C(0.22f, 0.38f, 0.2f), C(0.4f, 0.35f, 0.25f)),
                new Plant(Kind.Grass, 14, "Césped de la casa (Cynodon dactylon), segado", 5, C(0.4f, 0.62f, 0.28f), C(0.5f, 0.6f, 0.3f)),
                new Plant(Kind.Flower, 5, "Rosal del jardín (Rosa)", 120, C(0.25f, 0.42f, 0.22f), C(0.85f, 0.15f, 0.25f)),
                new Plant(Kind.Shrub, 4, "Seto de pitósporo (Pittosporum tobira)", 150, C(0.22f, 0.4f, 0.22f), C(0.95f, 0.95f, 0.85f)) },
              // Una ciudad de coches (sin caballos): los perros y los gatos de las casas.
              townFauna = new[] {
                new Pet("Woodlouse", 3, 0.7f, C(0.8f, 0.72f, 0.6f), "Perro de la casa (en el microcosmos, una cochinilla hormiguera)"),
                new Pet("Woodlouse", 2, 0.5f, C(0.85f, 0.83f, 0.78f), "Gato de la casa (en el microcosmos, una cochinilla hormiguera)") },
              bugs = new Bug[0] };
            case "Bagdad": return new Biome { title = "Bagdad, la Ciudad Redonda (~820): ciudad, sin naturaleza salvaje", ground = C(0.78f, 0.68f, 0.5f), plants = new[] {
                new Plant(Kind.Shrub, 8, "Palmera datilera de los jardines (Phoenix dactylifera), plantada", 1500, C(0.35f, 0.5f, 0.25f), C(0.75f, 0.45f, 0.15f)),
                new Plant(Kind.Shrub, 4, "Granado del patio (Punica granatum), plantado", 300, C(0.3f, 0.45f, 0.22f), C(0.8f, 0.15f, 0.12f)),
                new Plant(Kind.Stone, 16, "Ladrillo cocido", 0, C(0.75f, 0.55f, 0.38f), Stone) },
              // Los jardines de los patios abasíes: palmeras, granados, naranjos amargos, rosales de Damasco y setos de arrayán
              // (Lassner 1970; Ruggles 2008, «Islamic Gardens and Landscapes»).
              town = new[] {
                new Plant(Kind.Shrub, 8, "Palmera datilera de los jardines (Phoenix dactylifera), plantada", 1500, C(0.35f, 0.5f, 0.25f), C(0.75f, 0.45f, 0.15f)),
                new Plant(Kind.Shrub, 4, "Granado del patio (Punica granatum), plantado", 300, C(0.3f, 0.45f, 0.22f), C(0.8f, 0.15f, 0.12f)),
                new Plant(Kind.Shrub, 3, "Naranjo amargo (Citrus × aurantium)", 500, C(0.2f, 0.4f, 0.2f), C(0.95f, 0.55f, 0.1f)),
                new Plant(Kind.Flower, 4, "Rosal de Damasco (Rosa × damascena)", 150, C(0.28f, 0.42f, 0.25f), C(0.92f, 0.55f, 0.65f)),
                new Plant(Kind.Shrub, 4, "Seto de arrayán (Myrtus communis)", 120, C(0.2f, 0.35f, 0.2f), C(0.95f, 0.95f, 0.9f)) },
              // Los asnos de los aguadores, los camellos de las caravanas del zoco de al-Karj y los gatos (queridos en el islam: el
              // compañero Abu Hurayra, «el del gatito»).
              townFauna = new[] {
                new Pet("Aphid", 3, 1.6f, C(0.55f, 0.5f, 0.45f), "Asno del aguador (en el microcosmos, un pulgón de carga)"),
                new Pet("Aphid", 2, 3f, C(0.75f, 0.6f, 0.4f), "Camello de la caravana del zoco (en el microcosmos, un pulgón de carga)"),
                new Pet("Woodlouse", 3, 0.5f, C(0.85f, 0.83f, 0.78f), "Gato del patio (en el microcosmos, una cochinilla hormiguera)") },
              bugs = new Bug[0] };

            default: return null;
        }
    }
}
