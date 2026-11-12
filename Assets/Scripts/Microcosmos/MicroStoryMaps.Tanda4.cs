using UnityEngine;

/// <summary>LOS MAPAS A MANO DE LA TANDA 4 (2026-11-12; petición 75, regla M9; docs/microcosmos-mapas-a-mano.md §Tanda 4): las historias que el jugador ve
/// primero, con su lugar real. Mismo marco que el plano del pueblo (<c>CivicBuilder.TownPlans</c>): metros desde la «Historia», x al este, z al norte.
/// Nada se siembra sobre el escenario de la historia (≤ 30 m del origen): ahí están sus personas y sus sitios.</summary>
public static partial class MicroStoryMaps
{
    // ══ KAPILAVASTU (Tilaurakot), s. V a. C., otoño tras el monzón (Siddhārtha y los cuatro encuentros): sobre el Hospital ══════════════════════════
    // La ciudadela amurallada (~500 × 450 m) de ladrillo cocido con foso y cuatro puertas (las del este y el oeste, excavadas), calles en cuadrícula
    // y estanques revestidos de ladrillo, en la orilla este del Banganga; fuera, los arrozales del Terai y el estanque Sagar. Al sur, a unos 6 km
    // (aquí, al borde sur del mapa), Kudan: la arboleda de banianos del Nigrodhārāma, donde la tradición sitúa el reencuentro con su padre. Al este, a
    // unos 25 km (aquí, al borde este), el bosque de sal de Lumbini, donde nació. Octubre–noviembre: el arroz granado, la caña kans en flor blanca en
    // las orillas, la tierra húmeda (termitas y su seta, el ácaro rojo de las lluvias, luciérnagas sobre los arrozales).
    static Map MapKapilavastu() => new Map
    {
        display = "Kapilavastu", place = "Kapilavastu (Tilaurakot, Terai del Nepal), otoño tras el monzón, s. V a. C.", status = Status.Hecho,
        source = "Coningham et al., proyecto Tilaurakot-Kapilavastu (Univ. de Durham, 2014–2018: ciudadela de ~500 × 400 m, cuadrícula, murallas y foso); " +
                 "Lumbini Development Trust (Kudan, el Nigrodhārāma, ~3 km al sur de Taulihawa); Wikipedia, «Tilaurakot» (orilla este del Banganga); " +
                 "flora y fauna del Terai: bosque de sal (Shorea robusta) y pastizales de kans (Saccharum spontaneum), FAO/DNPWC Nepal",
        floor = Natural(
            new Piece("alfombra", Apilar, "las alfombras, apiladas: el montículo de la ciudadela (Tilaurakot se alza sobre la llanura)"),
            new Piece("silla", Como, "en el Terai no nieva: las sillas son las colinas Siwalik (Churia), al norte", "mesa"),
            new Piece("cama", Como, "las camas: los diques de los arrozales, bancales bajos de barro", "cama"),
            new Piece("banera", Como, "bañeras y piletas: los estanques revestidos de ladrillo de la ciudad", "banera"),
            new Piece("lavabo", Como, "los lavabos: el Banganga y sus brazos, al oeste", "lavabo")),
        flora = new[]
        {
            // Los arrozales granados, al este y al oeste (los del plano).
            new Plant("Arroz", 520f, 0f, 40, 180f), new Plant("Arroz", -680f, 300f, 30, 120f),
            // El Banganga: caña kans en flor en las orillas, arena y cantos en el cauce de la seca que empieza.
            new Plant("Caña kans", -470f, 450f, 12, 60f), new Plant("Caña kans", -430f, 0f, 12, 60f), new Plant("Caña kans", -470f, -450f, 12, 60f),
            new Plant("Arena del río", -440f, -150f, 4, 25f), new Plant("Arena del río", -460f, 250f, 4, 25f), new Plant("Piedra", -450f, -300f, 3, 20f),
            // Dentro de las murallas: la grama de las calles, la tulsi de los patios y un pipal junto al estanque de ladrillo; pipales en las puertas.
            new Plant("Grama durva", 50f, -110f, 6, 25f), new Plant("Grama durva", -190f, 190f, 8, 40f),
            new Plant("Albahaca sagrada", -125f, 140f, 4, 15f), new Plant("Albahaca sagrada", 125f, 60f, 4, 15f), new Plant("Albahaca sagrada", -60f, -160f, 3, 12f),
            new Plant("Pipal", 155f, -110f), new Plant("Pipal", 290f, 20f), new Plant("Pipal", -290f, -20f),
            // Fuera: los mangos de la puerta norte, la kusha en la orilla del Sagar y las boñigas del ganado junto a las puertas.
            new Plant("Mango", 60f, 330f, 8, 50f), new Plant("Hierba kusha", 420f, 340f, 8, 30f),
            new Plant("Boñiga", 0f, -280f, 3, 15f), new Plant("Boñiga", 0f, 280f, 3, 15f),
            // Al sur, Kudan: la arboleda de banianos del Nigrodhārāma, con su kusha.
            new Plant("Baniano", 0f, -900f, 6, 80f), new Plant("Hierba kusha", 0f, -900f, 14, 90f),
            // Al este, hacia Lumbini: el bosque de sal, sus plántulas, los termiteros y su seta; y una franja de sal al norte de la ciudadela.
            new Plant("Árbol de sal", 950f, -300f, 14, 140f), new Plant("Plántula de sal", 950f, -300f, 8, 160f), new Plant("Seta de termitero", 900f, -260f, 6, 60f),
            new Plant("Árbol de sal", -200f, 520f, 6, 80f), new Plant("Plántula de sal", -200f, 520f, 4, 80f),
        },
        fauna = new[]
        {
            new Animal("Grasshopper", "Saltamontes del arroz (Oxya hyla)", 520f, 0f, 4, 2.4f, 150f),
            new Animal("Aphid", "Pulgón del arroz", -680f, 300f, 4, 1.2f, 100f),
            new Animal("Ladybug", "Mariquita de bandas (Coccinella transversalis), cazando pulgones", 520f, 120f, 3, 1.4f, 80f),
            new Animal("Firefly", "Luciérnaga (Luciola), al anochecer sobre los arrozales", 560f, -80f, 4, 1f, 120f),
            new Animal("Termite", "Termita de montículo (Odontotermes obesus), en el bosque de sal", 900f, -260f, 4, 1.2f, 50f),
            new Animal("DungBeetle", "Pelotero gigante (Heliocopris bucephalus), en la boñiga de la puerta sur", 0f, -290f, 3, 2f, 20f),
            new Animal("Antlion", "Hormiga león (Myrmeleon), en la arena del Banganga", -440f, -150f, 3, 1.4f, 20f),
            new Animal("Mantis", "Mantis asiática (Hierodula), en la caña kans", -470f, 450f, 2, 2.4f, 50f),
            new Animal("VelvetMite", "Ácaro de terciopelo rojo, «bir bahuti» (Trombidium), tras las lluvias en Kudan", 0f, -900f, 3, 0.8f, 60f),
        },
        buildings = new[]
        {
            new Building("Puerta del este", 250f, 0f), new Building("Puerta del oeste", -250f, 0f), new Building("Puerta del norte", 0f, 225f), new Building("Puerta del sur", 0f, -225f),
            new Building("Plaza", 50f, -65f), new Building("Estanque de ladrillo", 130f, -130f), new Building("Sagar (estanque de fuera)", 360f, 300f), new Building("Arrozales del este", 520f, 0f),
        },
        todo = new[] { "en vivo: que la caña kans y la arena caigan en la orilla del Banganga y no en su cauce; que Kudan y el bosque de sal entren en el mapa (borde sur y este)" },
    };
}
