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
            // ── Aldea de la montaña (homenaje a Goku; ficción): monte subtropical del sur de China en primavera. Bambú, pino de
            //    Masson, camelia del té, azalea, helecho y musgo en la piedra húmeda; cigarra, luciérnaga, mantis, hormiga y saltamontes.
            //    En la aldea: los frutales de la China tradicional, el bambú y el té plantados; gallinas y el perro de la casa.
            case "Aldea de la montaña": return new Biome { title = "Aldea de la montaña (homenaje), monte subtropical en primavera", ground = C(0.42f, 0.36f, 0.26f), plants = new[] {
                new Plant(Kind.Shrub, 8, "Bambú (Phyllostachys edulis)", 1200, C(0.35f, 0.58f, 0.3f), C(0.55f, 0.6f, 0.3f)),
                new Plant(Kind.Shrub, 6, "Pino de Masson (Pinus massoniana)", 1800, C(0.2f, 0.38f, 0.25f), C(0.45f, 0.35f, 0.25f)),
                new Plant(Kind.Flower, 5, "Azalea (Rhododendron simsii)", 150, C(0.25f, 0.42f, 0.22f), C(0.9f, 0.25f, 0.4f)),
                new Plant(Kind.Fern, 8, "Helecho del monte (Dicranopteris)", 80, C(0.3f, 0.55f, 0.28f), C(0.35f, 0.3f, 0.2f)),
                new Plant(Kind.Moss, 6, "Musgo de la piedra húmeda", 3, C(0.3f, 0.5f, 0.22f), C(0.45f, 0.6f, 0.25f)),
                new Plant(Kind.Stone, 12, "Granito del monte", 0, C(0.62f, 0.62f, 0.6f), Stone) },
              town = new[] {
                new Plant(Kind.Shrub, 5, "Bambú del patio (Phyllostachys), plantado", 800, C(0.35f, 0.58f, 0.3f), C(0.55f, 0.6f, 0.3f)),
                new Plant(Kind.Shrub, 6, "Camelia del té (Camellia sinensis), plantada", 120, C(0.2f, 0.4f, 0.2f), C(0.95f, 0.95f, 0.85f)),
                new Plant(Kind.Grass, 10, "Hierba del sendero", 15, C(0.45f, 0.6f, 0.3f), C(0.5f, 0.55f, 0.3f)) },
              bugs = new[] {
                new Bug("Cicada", 3, 40, C(0.3f, 0.35f, 0.25f), "Cigarra (Cryptotympana atrata)"),
                new Bug("Firefly", 4, 12, C(0.25f, 0.22f, 0.2f), "Luciérnaga del arrozal"),
                new Bug("Mantis", 2, 80, C(0.4f, 0.65f, 0.3f), "Mantis china (Tenodera sinensis)"),
                new Bug("Ant", 6, 8, C(0.35f, 0.22f, 0.15f), "Hormiga del monte (Camponotus japonicus)"),
                new Bug("Grasshopper", 4, 30, C(0.45f, 0.6f, 0.3f), "Saltamontes del bancal (Oxya chinensis)") },
              townFauna = new[] {
                new Pet("Ladybug", 3, 0.5f, C(0.75f, 0.45f, 0.2f), "Gallina de la aldea (en el microcosmos, una mariquita parda)"),
                new Pet("Woodlouse", 2, 0.7f, C(0.7f, 0.55f, 0.35f), "Perro de la aldea (en el microcosmos, una cochinilla hormiguera)") } };
            // ── Albuquerque (1975), primavera: ciudad, sin naturaleza salvaje. Olmos siberianos y álamos de calle (Populus
            //    deltoides wislizenii, el cottonwood del Río Grande), enebros, chamisa y céspedes; perros y gatos. En las afueras, la
            //    estepa del alto desierto (grama azul, chamisa, cholla). (City of Albuquerque, «Street Tree Guide»; Dick-Peddie 1993.)
            case "Albuquerque": return new Biome { title = "Albuquerque (1975): ciudad, sin naturaleza salvaje", ground = C(0.72f, 0.62f, 0.5f), plants = new[] {
                new Plant(Kind.Grass, 12, "Grama azul (Bouteloua gracilis)", 25, C(0.5f, 0.6f, 0.4f), C(0.55f, 0.45f, 0.35f)),
                new Plant(Kind.Shrub, 8, "Chamisa (Ericameria nauseosa)", 120, C(0.5f, 0.58f, 0.45f), C(0.9f, 0.8f, 0.2f)),
                new Plant(Kind.Shrub, 4, "Cholla (Cylindropuntia imbricata)", 150, C(0.4f, 0.52f, 0.35f), C(0.7f, 0.2f, 0.5f)),
                new Plant(Kind.Stone, 10, "Grava del alto desierto", 0, C(0.7f, 0.6f, 0.5f), Stone) },
              town = new[] {
                new Plant(Kind.Shrub, 5, "Álamo del Río Grande (Populus deltoides wislizenii), plantado", 1800, C(0.35f, 0.55f, 0.3f), C(0.6f, 0.55f, 0.45f)),
                new Plant(Kind.Shrub, 4, "Olmo siberiano (Ulmus pumila), de calle", 1500, C(0.3f, 0.5f, 0.28f), C(0.5f, 0.42f, 0.32f)),
                new Plant(Kind.Shrub, 3, "Enebro del jardín (Juniperus scopulorum)", 400, C(0.25f, 0.4f, 0.32f), C(0.4f, 0.45f, 0.6f)),
                new Plant(Kind.Grass, 10, "Césped de la casa, segado", 5, C(0.42f, 0.6f, 0.3f), C(0.5f, 0.58f, 0.3f)) },
              townFauna = new[] {
                new Pet("Woodlouse", 3, 0.7f, C(0.8f, 0.72f, 0.6f), "Perro de la casa (en el microcosmos, una cochinilla hormiguera)"),
                new Pet("Woodlouse", 2, 0.5f, C(0.85f, 0.83f, 0.78f), "Gato de la casa (en el microcosmos, una cochinilla hormiguera)") },
              bugs = new Bug[0] };
            // ── Omaha (1983), finales de agosto: ciudad, sin naturaleza salvaje. Árboles de calle (fresno verde, arce plateado, olmo,
            //    roble rojo), céspedes y parterres; perros y gatos. Las afueras, pradera de hierba alta (grama, andropogon).
            //    (City of Omaha, «Street Trees»; Nebraska Game and Parks, tallgrass prairie.)
            case "Omaha": return new Biome { title = "Omaha (agosto de 1983): ciudad, sin naturaleza salvaje", ground = C(0.45f, 0.4f, 0.3f), plants = new[] {
                new Plant(Kind.Grass, 14, "Andropogon (Andropogon gerardii), pradera de hierba alta", 150, C(0.45f, 0.55f, 0.3f), C(0.6f, 0.35f, 0.3f)),
                new Plant(Kind.Flower, 4, "Equinácea (Echinacea purpurea)", 90, C(0.3f, 0.48f, 0.25f), C(0.75f, 0.4f, 0.65f)),
                new Plant(Kind.Stone, 8, "Bordillo de hormigón", 0, C(0.75f, 0.74f, 0.7f), Stone) },
              town = new[] {
                new Plant(Kind.Shrub, 5, "Fresno verde (Fraxinus pennsylvanica), de calle", 1800, C(0.3f, 0.5f, 0.28f), C(0.5f, 0.42f, 0.32f)),
                new Plant(Kind.Shrub, 4, "Arce plateado (Acer saccharinum), de calle", 2000, C(0.35f, 0.52f, 0.32f), C(0.75f, 0.78f, 0.75f)),
                new Plant(Kind.Shrub, 3, "Roble rojo (Quercus rubra), plantado", 2000, C(0.25f, 0.45f, 0.22f), C(0.5f, 0.35f, 0.25f)),
                new Plant(Kind.Grass, 14, "Césped de la casa, segado", 5, C(0.4f, 0.62f, 0.28f), C(0.5f, 0.6f, 0.3f)),
                new Plant(Kind.Flower, 4, "Petunias del parterre", 30, C(0.3f, 0.48f, 0.25f), C(0.8f, 0.3f, 0.6f)) },
              townFauna = new[] {
                new Pet("Woodlouse", 3, 0.7f, C(0.8f, 0.72f, 0.6f), "Perro de la casa (en el microcosmos, una cochinilla hormiguera)"),
                new Pet("Woodlouse", 2, 0.5f, C(0.85f, 0.83f, 0.78f), "Gato de la casa (en el microcosmos, una cochinilla hormiguera)") },
              bugs = new Bug[0] };
            // ── Prairie View (condado de Waller, Texas), mediados de diciembre: la sabana de robles del este de Texas (post oak
            //    savanna) y los pinares de pino taeda; invierno suave, pocos bichos. En el campus, encinas de Virginia, árboles de
            //    Júpiter (crape myrtle) y céspedes; perros. (Texas Parks and Wildlife, «Post Oak Savannah».)
            case "Prairie View": return new Biome { title = "Prairie View, sabana de robles del este de Texas (diciembre de 2020)", ground = C(0.5f, 0.4f, 0.3f), plants = new[] {
                new Plant(Kind.Shrub, 6, "Roble post (Quercus stellata)", 1500, C(0.35f, 0.42f, 0.25f), C(0.5f, 0.4f, 0.3f)),
                new Plant(Kind.Shrub, 5, "Pino taeda (Pinus taeda)", 2200, C(0.22f, 0.38f, 0.25f), C(0.45f, 0.35f, 0.25f)),
                new Plant(Kind.Shrub, 4, "Yaupon (Ilex vomitoria)", 300, C(0.25f, 0.4f, 0.22f), C(0.8f, 0.15f, 0.12f)),
                new Plant(Kind.Grass, 14, "Pasto del condado (Paspalum notatum)", 30, C(0.55f, 0.55f, 0.35f), C(0.6f, 0.55f, 0.4f)) },
              town = new[] {
                new Plant(Kind.Shrub, 5, "Encina de Virginia (Quercus virginiana), plantada en el campus", 1500, C(0.22f, 0.4f, 0.22f), C(0.45f, 0.38f, 0.3f)),
                new Plant(Kind.Shrub, 4, "Árbol de Júpiter (Lagerstroemia indica), plantado", 400, C(0.35f, 0.4f, 0.3f), C(0.85f, 0.4f, 0.6f)),
                new Plant(Kind.Grass, 14, "Césped del campus, segado", 5, C(0.45f, 0.58f, 0.3f), C(0.5f, 0.55f, 0.3f)) },
              bugs = new[] {
                new Bug("Ant", 4, 4, C(0.6f, 0.25f, 0.12f), "Hormiga roja de fuego (Solenopsis invicta)"),
                new Bug("Spider", 2, 10, C(0.4f, 0.32f, 0.25f), "Araña lobo (Hogna)") },
              townFauna = new[] {
                new Pet("Woodlouse", 3, 0.7f, C(0.8f, 0.72f, 0.6f), "Perro del pueblo (en el microcosmos, una cochinilla hormiguera)") } };
            default: return null;
        }
    }
}
