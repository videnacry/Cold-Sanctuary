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
            // ══FIN DE LOS BIOMAS══
            default: return null;
        }
    }
}
