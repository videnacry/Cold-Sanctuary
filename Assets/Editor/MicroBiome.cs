using UnityEngine;

/// <summary>
/// FLORA Y FAUNA NATIVAS de cada microcosmos (Berón, 2026-10-23: «cada microcosmos se debe generar con la flora y fauna
/// nativa de la era/región»). AreaMicrocosmos.Nature lee el bioma del microcosmos por su nombre: qué plantas crecen (con su
/// nombre real, su color y su porte), en qué proporción, y qué artrópodos viven allí en la ESTACIÓN de la historia
/// (en invierno casi todos están aletargados: solo salen los activos). Cada especie real se dibuja con el cuerpo de cubos
/// de su grupo (InsectMorphology: una hormiga tejedora es una «Ant» naranja; un abejorro, una «SnowBee» a rayas).
/// Datos y fuentes: docs/microcosmos-biomas.md. Sin bioma propio (las áreas del santuario de hoy), el genérico de siempre.
/// Escala: 1 cm real de planta ≈ 0,25 m en el microcosmos (la brizna de hierba de 5 cm ≈ 12 m); los bichos, ≈ 0,3 m por mm.
/// </summary>
public static class MicroBiome
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
        public Bug(string sp, int n, float mm, Color tint, string name) { species = sp; count = n; this.mm = mm; this.tint = tint; this.name = name; }
        public float Length => Mathf.Clamp(mm * 0.3f, 0.8f, 12f);
    }

    public class Biome
    {
        public string title;     // región y era, para el cartel y el log
        public bool winter;      // casi todo aletargado: menos fauna, escarcha o nieve en el suelo
        public Color ground = new Color(0.45f, 0.36f, 0.25f);   // la tierra de alrededor
        public Plant[] plants;
        public Bug[] bugs;
    }

    static Color C(float r, float g, float b) => new Color(r, g, b);
    static readonly Color Stone = C(0.55f, 0.52f, 0.48f), Sand = C(0.82f, 0.72f, 0.5f), DungC = C(0.35f, 0.25f, 0.15f), Snow = C(0.94f, 0.96f, 1f);

    /// <summary>El bioma del microcosmos llamado <paramref name="display"/> (o null: el genérico).</summary>
    public static Biome For(string display)
    {
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
                new Bug("Aphid", 6, 2, C(0.55f, 0.75f, 0.35f), "Pulgón del arroz") } };

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
                new Bug("DungBeetle", 3, 25, C(0.12f, 0.1f, 0.08f), "Escarabajo pelotero") } };

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
                new Bug("Ant", 3, 4, C(0.15f, 0.12f, 0.1f), "Hormiga negra (Lasius niger), al sol") } };

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
                new Bug("Ladybug", 4, 7, C(0.85f, 0.12f, 0.1f), "Mariquita de 7 puntos (Coccinella septempunctata)") } };

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
                new Bug("Ant", 3, 7, C(0.18f, 0.12f, 0.1f), "Hormiga segadora (Messor), en un día templado") } };

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
                new Bug("DungBeetle", 4, 12, C(0.08f, 0.08f, 0.1f), "Carábido de invierno (Nebria brevicollis)") } };

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
                new Bug("Ladybug", 3, 6, C(0.95f, 0.45f, 0.15f), "Mariquita de 9 puntos (Coccinella novemnotata), invernando") } };

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
                new Bug("SnowBee", 4, 13, C(0.75f, 0.55f, 0.2f), "Abeja melífera griega (Apis mellifera cecropia)") } };

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
                new Bug("SnowBee", 3, 12, C(0.9f, 0.7f, 0.25f), "Abeja egipcia (Apis mellifera lamarckii)") } };

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
                new Bug("SnowBee", 4, 18, C(0.2f, 0.18f, 0.12f), "Abejorro (Bombus terrestris)") } };

            // ── Ambrosio y la era del fuego: taiga prehistórica (verano corto) ──
            case "Hospital": case "Cocina": return new Biome { title = "Taiga prehistórica, antes del fuego (verano corto)", ground = C(0.3f, 0.26f, 0.18f), plants = new[] {
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

            default: return null;
        }
    }
}
