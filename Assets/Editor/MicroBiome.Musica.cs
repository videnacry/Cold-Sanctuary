using UnityEngine;

/// <summary>
/// FLORA Y FAUNA DE LOS PUEBLOS DE LA MÚSICA (docs/microcosmos-musica.md), con su lugar y su estación. En las ciudades (Montreal, Gary,
/// Belfast, Seúl, Long Island) solo lo plantado y los animales del pueblo con el cuerpo de su análogo; en el campo, la del lugar.
/// La colocación a mano de cada planta y bicho está en MicroStoryMaps.Musica (los nombres empiezan igual).
/// </summary>
public static partial class MicroBiome
{
    static Biome MusicBiome(string display)
    {
        switch (display)
        {
            // ── La Playa (Barranquilla), comienzos del año: la seca del Caribe, con la brisa. Bosque seco y manglar de la Ciénaga de Mallorquín
            //    (mangle rojo, mangle salado, trupillo, cardón, uvita de playa); en las calles, almendros, roble morado y trinitarias.
            //    (Humboldt, «Bosque seco tropical en Colombia», 2014; Vía Parque y Ciénaga de Mallorquín, CRA Atlántico.)
            case "Barranquilla": return new Biome { title = "La Playa (Barranquilla): manglar y bosque seco del Caribe (2009)", ground = C(0.76f, 0.68f, 0.52f), plants = new[] {
                new Plant(Kind.Shrub, 6, "Mangle rojo (Rhizophora mangle)", 900, C(0.25f, 0.45f, 0.25f), C(0.45f, 0.3f, 0.2f)),
                new Plant(Kind.Shrub, 5, "Mangle salado (Avicennia germinans)", 700, C(0.32f, 0.45f, 0.3f), C(0.5f, 0.42f, 0.32f)),
                new Plant(Kind.Shrub, 6, "Trupillo (Prosopis juliflora)", 700, C(0.4f, 0.55f, 0.3f), C(0.8f, 0.75f, 0.3f)),
                new Plant(Kind.Shrub, 3, "Cardón (Stenocereus griseus)", 500, C(0.35f, 0.55f, 0.4f), C(0.85f, 0.8f, 0.7f)),
                new Plant(Kind.Grass, 12, "Pasto de la orilla (Sporobolus virginicus)", 30, C(0.6f, 0.62f, 0.35f), C(0.7f, 0.65f, 0.4f)),
                new Plant(Kind.Stone, 6, "Arena de la Ciénaga", 0, C(0.8f, 0.72f, 0.56f), Stone) },
              town = new[] {
                new Plant(Kind.Shrub, 5, "Almendro de playa (Terminalia catappa), plantado", 1200, C(0.25f, 0.5f, 0.25f), C(0.55f, 0.45f, 0.3f)),
                new Plant(Kind.Shrub, 4, "Roble morado (Tabebuia rosea), plantado", 1500, C(0.3f, 0.5f, 0.28f), C(0.85f, 0.55f, 0.75f)),
                new Plant(Kind.Shrub, 4, "Matarratón (Gliricidia sepium), cerca viva", 600, C(0.32f, 0.52f, 0.28f), C(0.85f, 0.65f, 0.75f)),
                new Plant(Kind.Flower, 4, "Trinitaria (Bougainvillea)", 200, C(0.28f, 0.45f, 0.25f), C(0.85f, 0.2f, 0.55f)),
                new Plant(Kind.Grass, 8, "Hierba del solar", 15, C(0.55f, 0.6f, 0.32f), C(0.6f, 0.58f, 0.35f)) },
              bugs = new[] {
                new Bug("Ant", 6, 12, C(0.5f, 0.22f, 0.12f), "Hormiga arriera (Atta colombica)"),
                new Bug("Firefly", 3, 12, C(0.25f, 0.22f, 0.2f), "Cocuyo del manglar"),
                new Bug("Grasshopper", 3, 35, C(0.5f, 0.55f, 0.3f), "Saltamontes de la seca"),
                new Bug("Cricket", 3, 18, C(0.3f, 0.22f, 0.15f), "Grillo del solar") },
              townFauna = new[] {
                new Pet("Ladybug", 3, 0.5f, C(0.75f, 0.45f, 0.2f), "Gallina del patio (en el microcosmos, una mariquita parda)"),
                new Pet("Woodlouse", 2, 0.7f, C(0.8f, 0.72f, 0.6f), "Perro de la calle (en el microcosmos, una cochinilla hormiguera)"),
                new Pet("Aphid", 2, 1.5f, C(0.55f, 0.5f, 0.45f), "Burro de carga (en el microcosmos, un pulgón de carga)") } };
            // ── Vega Baja, final del verano: el llano húmedo de la costa norte y los mogotes del karst (Thrinax morrisii, ausubo, helechos), la
            //    Laguna Tortuguero (enea). En las calles, flamboyanes, palmas reales y almendros; coquíes y gallos. (Reserva Natural Laguna
            //    Tortuguero, DRNA de Puerto Rico; Little y Wadsworth 1964, Common Trees of Puerto Rico.)
            case "Vega Baja": return new Biome { title = "Vega Baja, costa norte de Puerto Rico: llano, mogotes y laguna (2016)", ground = C(0.5f, 0.42f, 0.3f), plants = new[] {
                new Plant(Kind.Shrub, 6, "Ausubo (Manilkara bidentata)", 1800, C(0.22f, 0.42f, 0.24f), C(0.45f, 0.32f, 0.25f)),
                new Plant(Kind.Shrub, 5, "Palma de sombrero (Sabal causiarum)", 900, C(0.3f, 0.5f, 0.28f), C(0.55f, 0.48f, 0.35f)),
                new Plant(Kind.Fern, 8, "Helecho del mogote (Nephrolepis)", 60, C(0.3f, 0.55f, 0.28f), C(0.35f, 0.3f, 0.2f)),
                new Plant(Kind.Grass, 12, "Enea de la laguna (Typha domingensis)", 200, C(0.45f, 0.58f, 0.3f), C(0.5f, 0.35f, 0.2f)),
                new Plant(Kind.Grass, 12, "Yerba del llano (Panicum maximum)", 120, C(0.5f, 0.62f, 0.3f), C(0.6f, 0.55f, 0.35f)),
                new Plant(Kind.Stone, 8, "Caliza del mogote", 0, C(0.8f, 0.78f, 0.7f), Stone) },
              town = new[] {
                new Plant(Kind.Shrub, 5, "Flamboyán (Delonix regia), plantado", 1200, C(0.3f, 0.52f, 0.28f), C(0.9f, 0.3f, 0.15f)),
                new Plant(Kind.Shrub, 4, "Palma real (Roystonea borinquena), plantada", 1800, C(0.3f, 0.5f, 0.28f), C(0.7f, 0.7f, 0.65f)),
                new Plant(Kind.Flower, 4, "Amapola (Hibiscus rosa-sinensis)", 200, C(0.28f, 0.5f, 0.25f), C(0.9f, 0.2f, 0.3f)),
                new Plant(Kind.Grass, 10, "Grama del patio, segada", 5, C(0.42f, 0.62f, 0.28f), C(0.5f, 0.6f, 0.3f)) },
              bugs = new[] {
                new Bug("Ant", 5, 8, C(0.45f, 0.25f, 0.15f), "Hormiga brava"),
                new Bug("Cricket", 4, 20, C(0.3f, 0.22f, 0.15f), "Grillo de la noche"),
                new Bug("Firefly", 3, 12, C(0.25f, 0.22f, 0.2f), "Cucubano (Pyrophorus)"),
                new Bug("Mantis", 2, 60, C(0.4f, 0.65f, 0.3f), "Santateresa") },
              townFauna = new[] {
                new Pet("Ladybug", 3, 0.5f, C(0.75f, 0.45f, 0.2f), "Gallina del patio (en el microcosmos, una mariquita parda)"),
                new Pet("Woodlouse", 3, 0.7f, C(0.8f, 0.72f, 0.6f), "Sato de la calle (en el microcosmos, una cochinilla hormiguera)") } };
            // ── Montreal, 1 de junio: ciudad, primavera tardía. Arces y olmos de calle (el olmo americano aún en pie en 1969), manzanos silvestres
            //    y lilas; en el Mont Royal, robles rojos, arces de azúcar y fresnos. Perros y gatos; palomas fuera. (Les amis de la montagne;
            //    Ville de Montréal, «Arbres publics».)
            case "Montreal": return new Biome { title = "Montreal (junio de 1969): ciudad; Mont Royal, bosque de arces", ground = C(0.48f, 0.44f, 0.36f), plants = new[] {
                new Plant(Kind.Shrub, 7, "Arce de azúcar (Acer saccharum)", 2200, C(0.3f, 0.52f, 0.28f), C(0.5f, 0.42f, 0.32f)),
                new Plant(Kind.Shrub, 5, "Roble rojo (Quercus rubra)", 2200, C(0.25f, 0.45f, 0.22f), C(0.5f, 0.35f, 0.25f)),
                new Plant(Kind.Fern, 6, "Helecho del monte (Dryopteris)", 60, C(0.3f, 0.55f, 0.28f), C(0.35f, 0.3f, 0.2f)),
                new Plant(Kind.Grass, 10, "Hierba de la falda", 40, C(0.42f, 0.6f, 0.3f), C(0.5f, 0.55f, 0.3f)),
                new Plant(Kind.Stone, 8, "Roca del Mont Royal", 0, C(0.5f, 0.5f, 0.5f), Stone) },
              town = new[] {
                new Plant(Kind.Shrub, 6, "Olmo americano (Ulmus americana), de calle", 2200, C(0.3f, 0.5f, 0.28f), C(0.5f, 0.42f, 0.32f)),
                new Plant(Kind.Shrub, 4, "Arce de Noruega (Acer platanoides), de calle", 1600, C(0.28f, 0.5f, 0.26f), C(0.55f, 0.45f, 0.3f)),
                new Plant(Kind.Flower, 4, "Lila (Syringa vulgaris)", 300, C(0.28f, 0.46f, 0.25f), C(0.72f, 0.55f, 0.85f)),
                new Plant(Kind.Grass, 12, "Césped de la plaza, segado", 5, C(0.42f, 0.62f, 0.28f), C(0.5f, 0.6f, 0.3f)) },
              townFauna = new[] {
                new Pet("Woodlouse", 3, 0.7f, C(0.8f, 0.72f, 0.6f), "Perro de la calle (en el microcosmos, una cochinilla hormiguera)"),
                new Pet("Woodlouse", 2, 0.5f, C(0.5f, 0.48f, 0.46f), "Gato de la azotea (en el microcosmos, una cochinilla hormiguera)") },
              bugs = new[] { new Bug("Ant", 4, 7, C(0.25f, 0.2f, 0.18f), "Hormiga del Mont Royal (Formica)"), new Bug("Ladybug", 3, 6, C(0.8f, 0.2f, 0.12f), "Mariquita del arce") } };
            // ── Gary, primavera de 1966: ciudad industrial, sin naturaleza salvaje salvo la orilla. Olmos y arces de calle, ailantos en los solares junto a la
            //    acería; en las dunas del lago Míchigan, barrón y álamos de Virginia (Indiana Dunes, National Park Service). Perros y gatos.
            case "Gary": return new Biome { title = "Gary (1966): ciudad del acero; dunas del lago Míchigan", ground = C(0.42f, 0.39f, 0.35f), plants = new[] {
                new Plant(Kind.Grass, 12, "Barrón de las dunas (Ammophila breviligulata)", 90, C(0.55f, 0.62f, 0.4f), C(0.75f, 0.7f, 0.5f)),
                new Plant(Kind.Shrub, 5, "Álamo de Virginia (Populus deltoides)", 2200, C(0.32f, 0.52f, 0.3f), C(0.6f, 0.55f, 0.45f)),
                new Plant(Kind.Shrub, 4, "Roble negro de la duna (Quercus velutina)", 1800, C(0.25f, 0.44f, 0.22f), C(0.45f, 0.35f, 0.25f)),
                new Plant(Kind.Stone, 8, "Escoria de la acería", 0, C(0.3f, 0.29f, 0.3f), Stone) },
              town = new[] {
                new Plant(Kind.Shrub, 5, "Olmo americano (Ulmus americana), de calle", 2200, C(0.3f, 0.5f, 0.28f), C(0.5f, 0.42f, 0.32f)),
                new Plant(Kind.Shrub, 4, "Arce plateado (Acer saccharinum), de calle", 2000, C(0.35f, 0.52f, 0.32f), C(0.75f, 0.78f, 0.75f)),
                new Plant(Kind.Shrub, 3, "Ailanto (Ailanthus altissima), de los solares", 1500, C(0.35f, 0.5f, 0.28f), C(0.75f, 0.6f, 0.35f)),
                new Plant(Kind.Grass, 12, "Césped del patio, segado", 5, C(0.4f, 0.58f, 0.28f), C(0.5f, 0.56f, 0.3f)) },
              townFauna = new[] {
                new Pet("Woodlouse", 3, 0.7f, C(0.8f, 0.72f, 0.6f), "Perro de la casa (en el microcosmos, una cochinilla hormiguera)"),
                new Pet("Woodlouse", 2, 0.5f, C(0.6f, 0.58f, 0.55f), "Gato del callejón (en el microcosmos, una cochinilla hormiguera)") },
              bugs = new[] { new Bug("Firefly", 4, 12, C(0.25f, 0.22f, 0.2f), "Luciérnaga (Photinus pyralis)"), new Bug("Ant", 4, 6, C(0.25f, 0.2f, 0.18f), "Hormiga de las aceras (Tetramorium)") } };
            // ── Seúl (Mapo-gu), agosto: ciudad en pleno verano húmedo, sin naturaleza salvaje. Ginkgos y plátanos de calle, cerezos Yoshino, pino rojo coreano en el
            //    monte Wausan, carrizo en la orilla del Han. Cigarras por todas partes. (Seoul Metropolitan Government, «Street Trees»; National Institute of Biological Resources.)
            case "Seúl": return new Biome { title = "Seúl (agosto de 2016): ciudad; la orilla del río Han", ground = C(0.5f, 0.48f, 0.42f), plants = new[] {
                new Plant(Kind.Shrub, 6, "Pino rojo coreano (Pinus densiflora)", 1800, C(0.22f, 0.4f, 0.25f), C(0.6f, 0.38f, 0.28f)),
                new Plant(Kind.Grass, 14, "Carrizo del Han (Phragmites australis)", 220, C(0.45f, 0.58f, 0.32f), C(0.6f, 0.5f, 0.35f)),
                new Plant(Kind.Grass, 8, "Hierba de la orilla", 40, C(0.42f, 0.6f, 0.3f), C(0.5f, 0.55f, 0.3f)),
                new Plant(Kind.Stone, 8, "Bloque de la ribera", 0, C(0.62f, 0.62f, 0.6f), Stone) },
              town = new[] {
                new Plant(Kind.Shrub, 6, "Ginkgo (Ginkgo biloba), de calle", 1800, C(0.4f, 0.6f, 0.28f), C(0.5f, 0.42f, 0.32f)),
                new Plant(Kind.Shrub, 5, "Plátano de sombra (Platanus occidentalis), de calle", 2000, C(0.32f, 0.52f, 0.3f), C(0.7f, 0.68f, 0.6f)),
                new Plant(Kind.Shrub, 3, "Cerezo Yoshino (Prunus × yedoensis), plantado", 900, C(0.3f, 0.5f, 0.28f), C(0.5f, 0.35f, 0.3f)),
                new Plant(Kind.Grass, 10, "Césped del parque, segado", 5, C(0.42f, 0.62f, 0.28f), C(0.5f, 0.6f, 0.3f)) },
              townFauna = new[] { new Pet("Woodlouse", 2, 0.5f, C(0.85f, 0.83f, 0.78f), "Perro de piso (en el microcosmos, una cochinilla hormiguera)") },
              bugs = new[] { new Bug("Cicada", 5, 35, C(0.3f, 0.35f, 0.25f), "Cigarra de Seúl (Hyalessa maculaticollis)"), new Bug("Ladybug", 3, 7, C(0.85f, 0.35f, 0.12f), "Mariquita asiática (Harmonia axyridis)") } };
            // ── Kingston, abril: al final de la seca, llanura de Liguanea. Lignum vitae, mahoe azul (el árbol nacional), guaco; en los patios, mangos, cocos y plátanos; cactus y
            //    acacias en las laderas de Long Mountain; pino y helechos en las Montañas Azules. Cabras, gallinas y perros. (Adams 1972, Flowering Plants of Jamaica.)
            case "Kingston": return new Biome { title = "Kingston, llanura de Liguanea (abril de 1978)", ground = C(0.6f, 0.5f, 0.38f), plants = new[] {
                new Plant(Kind.Shrub, 5, "Mahoe azul (Hibiscus elatus)", 1800, C(0.25f, 0.45f, 0.28f), C(0.85f, 0.45f, 0.2f)),
                new Plant(Kind.Shrub, 5, "Lignum vitae (Guaiacum officinale)", 800, C(0.22f, 0.4f, 0.28f), C(0.5f, 0.55f, 0.85f)),
                new Plant(Kind.Shrub, 5, "Acacia de la ladera (Vachellia farnesiana)", 450, C(0.4f, 0.5f, 0.3f), C(0.9f, 0.8f, 0.3f)),
                new Plant(Kind.Shrub, 3, "Cactus de Long Mountain (Stenocereus hystrix)", 500, C(0.35f, 0.55f, 0.4f), C(0.85f, 0.8f, 0.7f)),
                new Plant(Kind.Fern, 8, "Helecho de las Montañas Azules (Cyathea)", 150, C(0.3f, 0.55f, 0.28f), C(0.35f, 0.3f, 0.2f)),
                new Plant(Kind.Grass, 10, "Hierba de guinea (Panicum maximum)", 120, C(0.55f, 0.6f, 0.32f), C(0.65f, 0.58f, 0.35f)),
                new Plant(Kind.Stone, 8, "Grava de la llanura", 0, C(0.65f, 0.58f, 0.48f), Stone) },
              town = new[] {
                new Plant(Kind.Shrub, 5, "Almendro de la India (Terminalia catappa), de sombra", 1200, C(0.25f, 0.5f, 0.25f), C(0.55f, 0.45f, 0.3f)),
                new Plant(Kind.Shrub, 4, "Cocotero (Cocos nucifera), plantado", 1800, C(0.3f, 0.5f, 0.25f), C(0.55f, 0.4f, 0.2f)),
                new Plant(Kind.Flower, 4, "Buganvilla (Bougainvillea)", 200, C(0.28f, 0.45f, 0.25f), C(0.85f, 0.2f, 0.55f)),
                new Plant(Kind.Grass, 8, "Hierba del patio", 15, C(0.55f, 0.6f, 0.32f), C(0.6f, 0.58f, 0.35f)) },
              bugs = new[] {
                new Bug("Cricket", 5, 20, C(0.3f, 0.22f, 0.15f), "Grillo de los patios"),
                new Bug("Firefly", 4, 12, C(0.25f, 0.22f, 0.2f), "Peenie-wallie (luciérnaga, Photinus)"),
                new Bug("Ant", 5, 8, C(0.4f, 0.22f, 0.15f), "Hormiga de Kingston"),
                new Bug("Grasshopper", 3, 35, C(0.5f, 0.55f, 0.3f), "Saltamontes de la seca") },
              townFauna = new[] {
                new Pet("Aphid", 3, 1.2f, C(0.45f, 0.38f, 0.32f), "Cabra de la calle (en el microcosmos, un pulgón)"),
                new Pet("Ladybug", 3, 0.5f, C(0.75f, 0.45f, 0.2f), "Gallina del patio (en el microcosmos, una mariquita parda)"),
                new Pet("Woodlouse", 3, 0.7f, C(0.8f, 0.72f, 0.6f), "Perro del patio (en el microcosmos, una cochinilla hormiguera)") } };
            // ── Belfast, mayo: ciudad húmeda en primavera. Tilos y plátanos de calle, cerezos; aulaga y brezo en Cave Hill y Black Mountain; junco de la orilla del Lagan. Perros y
            //    gatos; pocas mariposas. (Belfast Hills Partnership; Belfast City Council, «Trees».)
            case "Belfast": return new Biome { title = "Belfast (mayo de 1998): ciudad; las colinas de Cave Hill", ground = C(0.42f, 0.42f, 0.36f), plants = new[] {
                new Plant(Kind.Shrub, 7, "Aulaga (Ulex europaeus)", 180, C(0.3f, 0.45f, 0.28f), C(0.95f, 0.8f, 0.2f)),
                new Plant(Kind.Grass, 10, "Brezo de la colina (Calluna vulgaris)", 40, C(0.32f, 0.4f, 0.28f), C(0.65f, 0.4f, 0.6f)),
                new Plant(Kind.Grass, 10, "Junco de la orilla (Juncus effusus)", 80, C(0.4f, 0.55f, 0.32f), C(0.5f, 0.45f, 0.3f)),
                new Plant(Kind.Shrub, 4, "Serbal (Sorbus aucuparia)", 800, C(0.3f, 0.48f, 0.28f), C(0.9f, 0.95f, 0.85f)),
                new Plant(Kind.Stone, 8, "Basalto de Cave Hill", 0, C(0.3f, 0.3f, 0.32f), Stone) },
              town = new[] {
                new Plant(Kind.Shrub, 6, "Tilo (Tilia × europaea), de calle", 1800, C(0.3f, 0.52f, 0.28f), C(0.5f, 0.42f, 0.32f)),
                new Plant(Kind.Shrub, 4, "Plátano de sombra (Platanus × hispanica), de calle", 2000, C(0.32f, 0.52f, 0.3f), C(0.7f, 0.68f, 0.6f)),
                new Plant(Kind.Flower, 4, "Narciso del parterre", 30, C(0.3f, 0.5f, 0.3f), C(0.95f, 0.85f, 0.25f)),
                new Plant(Kind.Grass, 12, "Césped del parque, segado", 5, C(0.42f, 0.62f, 0.3f), C(0.5f, 0.6f, 0.3f)) },
              townFauna = new[] {
                new Pet("Woodlouse", 3, 0.7f, C(0.8f, 0.72f, 0.6f), "Perro de la casa (en el microcosmos, una cochinilla hormiguera)"),
                new Pet("Woodlouse", 2, 0.5f, C(0.5f, 0.48f, 0.46f), "Gato del patio (en el microcosmos, una cochinilla hormiguera)") },
              bugs = new[] { new Bug("Ant", 4, 5, C(0.25f, 0.2f, 0.18f), "Hormiga negra de jardín (Lasius niger)"), new Bug("SnowBee", 3, 16, C(0.75f, 0.6f, 0.25f), "Abejorro de Cave Hill (Bombus)") } };
            // ── Long Island, abril: suburbio de la llanura de Hempstead, primavera temprana. Arces y robles de calle, cornejos en flor; en el parque del lago, robles y pino de brea. Perros y
            //    gatos, ardillas fuera. (NY Department of Environmental Conservation, Hempstead Lake State Park; Long Island Native Plant Initiative.)
            case "Long Island": return new Biome { title = "Long Island (abril de 1989): suburbio; el lago de Hempstead", ground = C(0.46f, 0.42f, 0.34f), plants = new[] {
                new Plant(Kind.Shrub, 6, "Roble blanco (Quercus alba)", 2000, C(0.3f, 0.48f, 0.26f), C(0.5f, 0.42f, 0.32f)),
                new Plant(Kind.Shrub, 5, "Pino de brea (Pinus rigida)", 1600, C(0.22f, 0.4f, 0.26f), C(0.45f, 0.35f, 0.25f)),
                new Plant(Kind.Grass, 12, "Andropogon pequeño (Schizachyrium scoparium)", 80, C(0.5f, 0.55f, 0.35f), C(0.65f, 0.45f, 0.3f)),
                new Plant(Kind.Grass, 8, "Junco del lago", 80, C(0.4f, 0.55f, 0.32f), C(0.5f, 0.45f, 0.3f)),
                new Plant(Kind.Stone, 6, "Grava glaciar", 0, C(0.6f, 0.56f, 0.5f), Stone) },
              town = new[] {
                new Plant(Kind.Shrub, 5, "Arce rojo (Acer rubrum), de calle", 1800, C(0.32f, 0.5f, 0.28f), C(0.75f, 0.25f, 0.2f)),
                new Plant(Kind.Shrub, 4, "Roble de los pantanos (Quercus palustris), de calle", 2000, C(0.3f, 0.48f, 0.26f), C(0.5f, 0.38f, 0.28f)),
                new Plant(Kind.Flower, 4, "Cornejo en flor (Cornus florida)", 500, C(0.3f, 0.48f, 0.28f), C(0.96f, 0.94f, 0.9f)),
                new Plant(Kind.Grass, 12, "Césped de la casa, segado", 5, C(0.42f, 0.6f, 0.3f), C(0.5f, 0.58f, 0.3f)) },
              townFauna = new[] {
                new Pet("Woodlouse", 3, 0.7f, C(0.8f, 0.72f, 0.6f), "Perro de la casa (en el microcosmos, una cochinilla hormiguera)"),
                new Pet("Woodlouse", 2, 0.5f, C(0.5f, 0.48f, 0.46f), "Gato del garaje (en el microcosmos, una cochinilla hormiguera)") },
              bugs = new[] { new Bug("Ant", 4, 5, C(0.25f, 0.2f, 0.18f), "Hormiga de las aceras (Tetramorium)"), new Bug("Woodlouse", 3, 10, C(0.45f, 0.45f, 0.45f), "Cochinilla del jardín") } };
            // ── La Plaza de las Canciones (inventada): la flora de cada rincón en su sitio del mapa, tomada de su lugar real: roble y cornejo de Tennessee, arroz del Po, roble inglés y prado de Ascot, acacia y
            //    aloe del Cabo Oriental, mango y mahoe de Kingston, ichu y queñual de los Andes. (Arrozal: Ente Risi; Andes: Brack Egg 1999.)
            case "Grandes aportes de las canciones": return new Biome { title = "La Plaza de las Canciones (lugar inventado): la flora de cada rincón", ground = C(0.5f, 0.46f, 0.36f), plants = new[] {
                new Plant(Kind.Shrub, 5, "Roble de Tennessee (Quercus alba)", 2000, C(0.3f, 0.48f, 0.26f), C(0.5f, 0.42f, 0.32f)),
                new Plant(Kind.Grass, 10, "Arroz del Po (Oryza sativa)", 90, C(0.5f, 0.68f, 0.32f), C(0.8f, 0.72f, 0.4f)),
                new Plant(Kind.Shrub, 5, "Roble inglés (Quercus robur)", 2000, C(0.28f, 0.46f, 0.24f), C(0.5f, 0.4f, 0.3f)),
                new Plant(Kind.Grass, 10, "Pradera de Ascot (Lolium perenne)", 25, C(0.4f, 0.62f, 0.3f), C(0.5f, 0.6f, 0.3f)),
                new Plant(Kind.Shrub, 5, "Acacia de espinas (Vachellia karroo)", 700, C(0.38f, 0.5f, 0.28f), C(0.92f, 0.8f, 0.25f)),
                new Plant(Kind.Shrub, 3, "Áloe del Cabo (Aloe ferox)", 250, C(0.4f, 0.55f, 0.4f), C(0.9f, 0.35f, 0.15f)),
                new Plant(Kind.Grass, 12, "Ichu de los Andes (Jarava ichu)", 60, C(0.72f, 0.66f, 0.4f), C(0.75f, 0.7f, 0.5f)),
                new Plant(Kind.Shrub, 4, "Queñual (Polylepis racemosa)", 800, C(0.3f, 0.42f, 0.3f), C(0.6f, 0.4f, 0.3f)),
                new Plant(Kind.Stone, 8, "Piedra de los Andes", 0, C(0.55f, 0.53f, 0.5f), Stone) },
              town = new[] {
                new Plant(Kind.Shrub, 5, "Mahoe de Kingston (Hibiscus elatus)", 1800, C(0.25f, 0.45f, 0.28f), C(0.85f, 0.45f, 0.2f)),
                new Plant(Kind.Shrub, 4, "Almendro de la plaza (Terminalia catappa), plantado", 1200, C(0.25f, 0.5f, 0.25f), C(0.55f, 0.45f, 0.3f)),
                new Plant(Kind.Flower, 4, "Flor de la plaza (Bougainvillea)", 200, C(0.28f, 0.45f, 0.25f), C(0.85f, 0.2f, 0.55f)),
                new Plant(Kind.Grass, 12, "Césped de la plaza, segado", 5, C(0.42f, 0.62f, 0.3f), C(0.5f, 0.6f, 0.3f)) },
              townFauna = new[] { new Pet("Woodlouse", 3, 0.7f, C(0.8f, 0.72f, 0.6f), "Perro de la plaza (en el microcosmos, una cochinilla hormiguera)") },
              bugs = new[] { new Bug("Cricket", 4, 18, C(0.3f, 0.22f, 0.15f), "Grillo de la tarde"), new Bug("Cicada", 3, 30, C(0.3f, 0.35f, 0.25f), "Cigarra del paseo"), new Bug("Firefly", 3, 12, C(0.25f, 0.22f, 0.2f), "Luciérnaga del arrozal") } };
            // ══FIN DE LOS BIOMAS══
            default: return null;
        }
    }
}
