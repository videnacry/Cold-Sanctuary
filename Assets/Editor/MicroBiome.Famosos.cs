using UnityEngine;

/// <summary>
/// FLORA Y FAUNA DE LOS PUEBLOS DE LOS FAMOSOS (docs/microcosmos-famosos.md). Como en los científicos: en el campo, la del lugar y la
/// estación; en las ciudades (Hawthorne, Albuquerque, Omaha, Uagadugú), solo lo plantado y los animales del pueblo con el cuerpo de su
/// análogo (docs/microcosmos-fauna-eras.md), ningún bicho salvaje suelto.
/// </summary>
public static partial class MicroBiome
{
    static Biome FamousBiome(string display)
    {
        switch (display)
        {
            // ── Velliangiri (Ghats occidentales, Tamil Nadu), finales de junio: el monzón del suroeste deja lluvias suaves al pie ──
            //    del monte (el grueso cae al otro lado de los Ghats). Matorral seco caducifolio, bambú, cocotales y platanares de la
            //    aldea. Hormiga tejedora (Oecophylla smaragdina), abeja gigante (Apis dorsata), cigarra, luciérnaga, mantis.
            //    (Tamil Nadu Forest Department, «Nilgiri Biosphere Reserve»; Kumar 2012, Ants of the Western Ghats.)
            case "Velliangiri": return new Biome { title = "Velliangiri, Ghats occidentales (junio de 1999)", ground = C(0.6f, 0.38f, 0.26f), plants = new[] {
                new Plant(Kind.Shrub, 8, "Bambú (Bambusa bambos)", 900, C(0.4f, 0.6f, 0.3f), C(0.6f, 0.65f, 0.35f)),
                new Plant(Kind.Shrub, 6, "Teca (Tectona grandis)", 1500, C(0.3f, 0.45f, 0.25f), C(0.55f, 0.45f, 0.3f)),
                new Plant(Kind.Shrub, 5, "Lantana (Lantana camara)", 150, C(0.3f, 0.48f, 0.25f), C(0.95f, 0.55f, 0.2f)),
                new Plant(Kind.Grass, 14, "Hierba del monte (Themeda triandra)", 80, C(0.6f, 0.62f, 0.35f), C(0.7f, 0.5f, 0.3f)),
                new Plant(Kind.Flower, 4, "Flor de la gloria (Gloriosa superba)", 120, C(0.3f, 0.5f, 0.25f), C(0.9f, 0.3f, 0.15f)),
                new Plant(Kind.Stone, 10, "Granito de los Ghats", 0, C(0.6f, 0.58f, 0.55f), Stone) },
              town = new[] {
                new Plant(Kind.Shrub, 6, "Cocotero (Cocos nucifera), plantado", 1800, C(0.3f, 0.5f, 0.25f), C(0.55f, 0.4f, 0.2f)),
                new Plant(Kind.Shrub, 4, "Banano del huerto (Musa)", 400, C(0.3f, 0.55f, 0.25f), C(0.9f, 0.85f, 0.3f)),
                new Plant(Kind.Shrub, 3, "Nim (Azadirachta indica), plantado a la sombra", 1200, C(0.25f, 0.45f, 0.22f), C(0.85f, 0.85f, 0.6f)),
                new Plant(Kind.Flower, 4, "Jazmín (Jasminum sambac)", 100, C(0.25f, 0.42f, 0.22f), C(0.97f, 0.97f, 0.95f)),
                new Plant(Kind.Grass, 10, "Hierba del patio", 15, C(0.45f, 0.6f, 0.3f), C(0.5f, 0.55f, 0.3f)) },
              bugs = new[] {
                new Bug("Ant", 6, 9, C(0.75f, 0.45f, 0.15f), "Hormiga tejedora (Oecophylla smaragdina)"),
                new Bug("SnowBee", 4, 18, C(0.8f, 0.6f, 0.2f), "Abeja gigante (Apis dorsata)"),
                new Bug("Cicada", 3, 35, C(0.3f, 0.35f, 0.25f), "Cigarra de los Ghats (Platypleura)"),
                new Bug("Firefly", 3, 12, C(0.25f, 0.22f, 0.2f), "Luciérnaga"),
                new Bug("Mantis", 2, 70, C(0.4f, 0.65f, 0.3f), "Mantis (Hierodula)") },
              townFauna = new[] {
                new Pet("Aphid", 2, 2.4f, C(0.92f, 0.9f, 0.85f), "Vaca de la aldea (en el microcosmos, un pulgón)"),
                new Pet("Woodlouse", 2, 0.7f, C(0.75f, 0.6f, 0.45f), "Perro de la aldea (en el microcosmos, una cochinilla hormiguera)") } };
            // ── Hawthorne (Los Ángeles), finales de septiembre de 2008: ciudad, sin naturaleza salvaje. Las palmeras de abanico de las
            //    avenidas, jacarandas y ficus de calle, céspedes y buganvillas (City of Hawthorne; LA County street trees). Perros y gatos.
            case "Hawthorne": return new Biome { title = "Hawthorne (2008): ciudad, sin naturaleza salvaje", ground = C(0.62f, 0.6f, 0.56f), plants = new[] {
                new Plant(Kind.Shrub, 6, "Palmera de abanico (Washingtonia robusta), plantada", 2000, C(0.35f, 0.5f, 0.28f), C(0.55f, 0.45f, 0.35f)),
                new Plant(Kind.Grass, 8, "Césped (Cynodon dactylon), segado", 5, C(0.4f, 0.62f, 0.28f), C(0.5f, 0.6f, 0.3f)),
                new Plant(Kind.Stone, 12, "Bordillo de hormigón", 0, C(0.75f, 0.74f, 0.7f), Stone) },
              town = new[] {
                new Plant(Kind.Shrub, 6, "Palmera de abanico (Washingtonia robusta), plantada", 2000, C(0.35f, 0.5f, 0.28f), C(0.55f, 0.45f, 0.35f)),
                new Plant(Kind.Shrub, 3, "Jacaranda (Jacaranda mimosifolia), plantada", 1200, C(0.3f, 0.5f, 0.3f), C(0.6f, 0.45f, 0.85f)),
                new Plant(Kind.Grass, 14, "Césped de la casa (Cynodon dactylon), segado", 5, C(0.4f, 0.62f, 0.28f), C(0.5f, 0.6f, 0.3f)),
                new Plant(Kind.Flower, 4, "Buganvilla (Bougainvillea)", 200, C(0.28f, 0.45f, 0.25f), C(0.85f, 0.2f, 0.55f)) },
              townFauna = new[] {
                new Pet("Woodlouse", 3, 0.7f, C(0.8f, 0.72f, 0.6f), "Perro de la casa (en el microcosmos, una cochinilla hormiguera)"),
                new Pet("Woodlouse", 2, 0.5f, C(0.85f, 0.83f, 0.78f), "Gato de la casa (en el microcosmos, una cochinilla hormiguera)") },
              bugs = new Bug[0] };
            default: return null;
        }
    }
}
