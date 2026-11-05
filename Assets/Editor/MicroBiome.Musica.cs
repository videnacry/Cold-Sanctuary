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
            // ══FIN DE LOS BIOMAS══
            default: return null;
        }
    }
}
