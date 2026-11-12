using UnityEngine;

/// <summary>LOS MAPAS A MANO DE LOS CIENTÍFICOS (2026-11-12; petición 75, regla M9; docs/microcosmos-mapas-a-mano.md §Tanda 4). Mismo marco que el plano del
/// pueblo (<c>CivicBuilder.TownPlans</c>). En las CIUDADES no hay flora ni bichos salvajes (MicroBiome): solo lo plantado y los animales del pueblo
/// (los caballos, pulgones de tiro; los gatos, cochinillas; las ratas, cucarachas), cada uno donde estaba.</summary>
public static partial class MicroStoryMaps
{
    // ══ BERNA, mayo de 1905 (Einstein y el año milagroso): sobre la Planta Atómica ══════════════════════════════════════════════════════════
    // El casco viejo en la península del Aar: la Kramgasse con sus soportales y sus fuentes, la Zytglogge al oeste de ella (el escenario) y la casa
    // de los Einstein en el n.º 49; la Oficina de patentes cerca de la estación; la terraza del Palacio Federal (1902) y la Kleine Schanze al suroeste;
    // la Münsterplattform con sus castaños en flor; el Aar alrededor, con los puentes de Nydegg, Kirchenfeld y Kornhaus; el Gurten al sur.
    static Map MapBerna() => new Map
    {
        display = "Berna", place = "Berna, el casco viejo en la península del Aar, mayo de 1905", status = Status.Hecho,
        source = "Stadtplan der Stadt Bern 1905 (el plano del pueblo); Einstein-Haus Bern (Kramgasse 49, piso de 1903–1905); Stadtgrün Bern, «Münsterplattform» " +
                 "y «Kleine Schanze» (castaños, tilos, césped y setos); Bundeshaus (Palacio Federal, 1902)",
        floor = Natural(
            new Piece("banera", Como, "la piscina del reactor: el meandro del Aar", "banera"),
            new Piece("mesa", Como, "bancos y consolas: el Gurten al sur y la meseta del Kirchenfeld", "mesa"),
            new Piece("lavabo", Como, "los lavabos: las fuentes de la ciudad vieja (la del Zähringer, la del Ogro, la de la Justicia)", "lavabo")),
        flora = new[]
        {
            // La Münsterplattform: castaños de Indias en flor, su césped y sus setos.
            new Plant("Castaño de Indias", 420f, -140f, 8, 40f), new Plant("Césped de la Kleine Schanze", 420f, -140f, 10, 40f), new Plant("Seto de boj", 420f, -140f, 4, 30f),
            // La Kleine Schanze, al suroeste: castaños, césped y setos; y los tilos de la terraza del Palacio Federal.
            new Plant("Castaño de Indias", -560f, -140f, 6, 50f), new Plant("Césped de la Kleine Schanze", -560f, -140f, 20, 60f), new Plant("Seto de boj", -560f, -140f, 6, 50f),
            new Plant("Tilo", -320f, -170f, 4, 30f),
            // Los tilos de las plazas (Bärenplatz y Bundesplatz) y los geranios de las jardineras de la Kramgasse y la Gerechtigkeitsgasse.
            new Plant("Tilo", -220f, -60f, 4, 20f),
            new Plant("Geranios de las jardineras", 200f, 8f, 5, 15f), new Plant("Geranios de las jardineras", 450f, 8f, 5, 15f), new Plant("Geranios de las jardineras", 650f, 8f, 5, 15f),
            // El adoquín de arenisca de la Münstergasse.
            new Plant("Adoquín de arenisca", 300f, -70f, 4, 30f),
        },
        fauna = new[]
        {
            new Animal("Aphid", "Caballo del carro de reparto (en el microcosmos, un pulgón de tiro), en la Kramgasse", 350f, 0f, 3, 2.4f, 40f),
            new Animal("Aphid", "Caballo de carro en el mercado de la Bärenplatz", -250f, 40f, 3, 2.4f, 30f),
            new Animal("Aphid", "Caballo de coche de punto ante la estación", -700f, 40f, 3, 2.4f, 40f),
            new Animal("Woodlouse", "Gato de los soportales (en el microcosmos, una cochinilla hormiguera)", 200f, 70f, 2, 0.5f, 20f),
            new Animal("Woodlouse", "Gato de la Münstergasse", 450f, -60f, 2, 0.5f, 20f),
        },
        buildings = new[]
        {
            new Building("Casa de Einstein (Kramgasse 49)", 150f, 15f), new Building("Oficina de patentes (Genfergasse)", -650f, 200f), new Building("Plaza (Bärenplatz)", -220f, -60f),
            new Building("Estación", -700f, 40f), new Building("Münsterplattform", 420f, -140f), new Building("Kleine Schanze", -560f, -140f), new Building("Puente de Nydegg", 800f, 0f),
        },
        todo = new[] { "en vivo: que los castaños caigan en la plataforma y no en la Münstergasse; que los caballos queden en la calle y no bajo los soportales" },
    };

    // ══ PARÍS, diciembre de 1898 (Marie y Pierre Curie aíslan el radio): sobre el Laboratorio de Química ═════════════════════════════════════
    // El cobertizo de la Escuela de Física y Química (rue Lhomond) es el escenario. El Barrio Latino en invierno: los plátanos sin hoja del boulevard
    // Saint-Michel y de Port-Royal; el Jardín del Luxemburgo al oeste y el Jardin des Plantes al este, con césped de invierno, boj y castaños; la hiedra
    // de las verjas del Panteón; el mercado de la rue Mouffetard; ómnibus y fiacres de caballos; las ratas del alcantarillado de Belgrand.
    static Map MapParis1898() => new Map
    {
        display = "París 1898", place = "París, el Barrio Latino alrededor de la rue Lhomond, diciembre de 1898", status = Status.Hecho,
        source = "Plan de Paris (Alphand, 1889; el plano del pueblo); Alphand, Les Promenades de Paris (1867–1873: los plátanos de los bulevares y los squares); " +
                 "Quinn, Marie Curie: A Life (1995: el cobertizo de la rue Lhomond y la casa de la rue de la Glacière, 24); " +
                 "Compagnie générale des omnibus (~17 000 caballos hacia 1900)",
        floor = Natural(
            new Piece("mesa", Como, "las poyatas: la Montagne Sainte-Geneviève", "mesa"),
            new Piece("lavabo", Como, "las pilas: el Sena, al norte", "lavabo"),
            new Piece("silla", Como, "sin montañas en el Barrio Latino: los taburetes son las cúpulas y los tejados del barrio (el Panteón, la Sorbona)", "mesa")),
        flora = new[]
        {
            // Los plátanos sin hoja del boulevard Saint-Michel y del boulevard de Port-Royal.
            new Plant("Plátano de sombra del bulevar", -520f, -500f, 6, 40f), new Plant("Plátano de sombra del bulevar", -520f, 0f, 6, 40f), new Plant("Plátano de sombra del bulevar", -520f, 500f, 6, 40f),
            new Plant("Plátano de sombra del bulevar", -400f, -380f, 6, 40f), new Plant("Plátano de sombra del bulevar", 200f, -380f, 6, 40f),
            // El Jardín del Luxemburgo: castaños sin hoja, césped de invierno y setos de boj.
            new Plant("Castaño de Indias del square", -860f, 350f, 10, 90f), new Plant("Césped del square", -860f, 350f, 20, 110f), new Plant("Seto de boj", -860f, 350f, 8, 90f),
            // El Jardin des Plantes: plátanos, césped y la hiedra de sus verjas.
            new Plant("Plátano de sombra del bulevar", 650f, 450f, 8, 80f), new Plant("Césped del square", 650f, 450f, 12, 80f), new Plant("Hiedra de la verja", 650f, 450f, 6, 90f),
            // La hiedra de las verjas del Panteón y el adoquín de la rue Mouffetard.
            new Plant("Hiedra de la verja", -250f, 320f, 6, 40f), new Plant("Adoquín", 220f, -150f, 4, 30f),
        },
        fauna = new[]
        {
            new Animal("Aphid", "Caballo de ómnibus (en el microcosmos, un pulgón de tiro), en el boulevard Saint-Michel", -520f, 0f, 4, 2.4f, 60f),
            new Animal("Aphid", "Caballo de fiacre, en la plaza del Panteón", -250f, 320f, 2, 2.4f, 30f),
            new Animal("Woodlouse", "Gato de los patios de la rue Mouffetard (en el microcosmos, una cochinilla hormiguera)", 200f, 100f, 2, 0.5f, 30f),
            new Animal("Cockroach", "Rata del alcantarillado (en el microcosmos, una cucaracha), junto al mercado de la Mouffetard", 260f, -150f, 3, 0.3f, 30f),
            new Animal("Cockroach", "Rata de los muelles del Sena", 0f, 960f, 2, 0.3f, 40f),
        },
        buildings = new[]
        {
            new Building("Panteón", -300f, 380f), new Building("Jardín del Luxemburgo", -860f, 350f), new Building("Jardin des Plantes (Muséum)", 650f, 450f),
            new Building("Mercado de la rue Mouffetard", 260f, -150f), new Building("Sorbona", -420f, 620f), new Building("Casa de los Curie (rue de la Glacière)", -770f, -650f),
        },
        todo = new[] { "en vivo: que los plátanos caigan en las aceras de los bulevares y no en la calzada; que el Luxemburgo no tape el boulevard Saint-Michel" },
    };

    // ══ COLORADO SPRINGS, julio de 1899 (Tesla y la alta tensión): sobre la Construcción ═══════════════════════════════════════════════════════
    // La estación experimental de Knob Hill, al este de la ciudad, es el escenario. Alrededor, la pradera de hierba corta en julio (grama azul,
    // hierba del búfalo, yucas en flor, nopales, margaritas de la pradera), con hormigas segadoras y saltamontes; al oeste, la ciudad de Palmer con sus
    // avenidas, el ferrocarril y el Monument Creek con sus álamos; al fondo, las Rocosas y el Pikes Peak, y la arenisca roja del Garden of the Gods.
    static Map MapColoradoSprings() => new Map
    {
        display = "Colorado Springs", place = "Colorado Springs, la pradera de Knob Hill y la ciudad de Palmer, julio de 1899", status = Status.Hecho,
        source = "Tesla, Colorado Springs Notes 1899–1900 (Nolit, 1978: la estación de Knob Hill, al este de la ciudad); Sanborn Fire Insurance Map, Colorado Springs 1897 " +
                 "(el plano del pueblo); la estepa de hierba corta del este de Colorado (USDA-ARS, Central Plains Experimental Range: Bouteloua gracilis y B. dactyloides); " +
                 "bosque de ribera de álamos de las llanuras (Populus deltoides) en los arroyos del Front Range",
        floor = Natural(
            new Piece("alfombra", Apilar, "las alfombras, apiladas: Knob Hill, la loma de la estación"),
            new Piece("mesa", Como, "bancos y mesas de obra: las mesas (buttes) de la pradera", "mesa"),
            new Piece("silla", Como, "las sillas: las cumbres del Front Range, con nieve en el Pikes Peak", "silla")),
        flora = new[]
        {
            // La pradera de hierba corta al este y al norte de la estación.
            new Plant("Grama azul", 300f, 100f, 30, 200f), new Plant("Grama azul", 250f, -300f, 20, 150f), new Plant("Hierba del búfalo", -150f, 200f, 15, 80f),
            new Plant("Yuca", 400f, -100f, 8, 120f), new Plant("Nopal", 200f, 250f, 8, 80f), new Plant("Margarita de la pradera", 300f, -150f, 10, 100f),
            // Los álamos del Monument Creek, al oeste de la ciudad, y la arenisca roja del Garden of the Gods, al noroeste.
            new Plant("Álamo de Virginia", -960f, 450f, 6, 60f), new Plant("Álamo de Virginia", -950f, -300f, 6, 60f), new Plant("Arenisca roja", -1350f, 600f, 6, 100f),
        },
        fauna = new[]
        {
            new Animal("Grasshopper", "Saltamontes migratorio (Melanoplus sanguinipes)", 300f, 100f, 4, 2.4f, 150f),
            new Animal("Ant", "Hormiga segadora (Pogonomyrmex occidentalis), con su montículo de grava", 250f, -300f, 3, 1.2f, 100f),
            new Animal("Ladybug", "Mariquita convergente (Hippodamia convergens)", 300f, -150f, 2, 1.2f, 80f),
            new Animal("SnowBee", "Abejorro (Bombus huntii), en las margaritas", 300f, -150f, 3, 1.8f, 80f),
            new Animal("Spider", "Araña lobo (Hogna)", -150f, 200f, 2, 1.6f, 60f),
            new Animal("DungBeetle", "Escarabajo pelotero (Phanaeus)", 400f, -100f, 2, 1.6f, 80f),
            new Animal("Aphid", "Caballo de los coches de Pikes Peak Avenue (en el microcosmos, un pulgón de tiro)", -600f, 0f, 3, 2.4f, 60f),
        },
        buildings = new[]
        {
            new Building("Plaza (Acacia Park)", -660f, 210f), new Building("Estación del Denver & Rio Grande", -900f, 0f), new Building("Monument Creek", -960f, 0f),
            new Building("Pikes Peak", -1450f, -300f), new Building("Hotel Alta Vista (donde se alojó Tesla)", -790f, -60f),
        },
        todo = new[] { "en vivo: que la pradera caiga fuera del camino de Knob Hill; que los álamos queden en la orilla del Monument Creek y no en la vía" },
    };

    // ══ PASADENA, 1953 (Patterson y la edad de la Tierra): sobre el Laboratorio de Química ══════════════════════════════════════════════════════
    // El laboratorio limpio de Caltech es el escenario. El campus entre California Boulevard, San Pasqual, Hill y Wilson, con los olivos del Olive Walk
    // y las encinas de la costa; al sur y al norte, las calles de casas con su césped, sus rosales y sus setos de pitósporo; al oeste, el Arroyo Seco con su
    // encinar; al norte, Colorado Boulevard y los montes de San Gabriel, y al pie, en Altadena, los cedros del Himalaya de la Christmas Tree Lane (1885).
    // Una ciudad de coches: solo los perros y los gatos de las casas.
    static Map MapPasadena() => new Map
    {
        display = "Pasadena", place = "Pasadena, el campus de Caltech y sus calles, 1953", status = Status.Hecho,
        source = "Caltech Archives, «The Olive Walk» y el plano del campus (California Blvd, San Pasqual, Hill y Wilson); City of Pasadena, «Street Trees»; " +
                 "Christmas Tree Lane (Santa Rosa Avenue, Altadena, cedros plantados en 1885; Registro Nacional de Lugares Históricos); el encinar de Quercus agrifolia " +
                 "del Arroyo Seco (Arroyo Seco Foundation); el plano del pueblo",
        floor = Natural(
            new Piece("mesa", Como, "las poyatas: los montes de San Gabriel, al norte", "mesa"),
            new Piece("lavabo", Como, "las pilas: el Arroyo Seco, al oeste", "lavabo")),
        flora = new[]
        {
            // El campus: los olivos del Olive Walk, las encinas y el césped.
            new Plant("Olivo del Olive Walk", -150f, 60f, 10, 40f), new Plant("Encina de la costa", -200f, -100f, 4, 30f), new Plant("Césped del campus", -100f, -200f, 12, 50f),
            // Las palmeras de California Boulevard.
            new Plant("Palmera de abanico", -500f, 150f, 6, 60f), new Plant("Palmera de abanico", 500f, 150f, 6, 60f),
            // El encinar del Arroyo Seco.
            new Plant("Encina de la costa", -1050f, 300f, 8, 80f), new Plant("Encina de la costa", -1050f, -300f, 8, 80f),
            // Las calles de casas al sur y al norte del campus: césped, rosales y setos de pitósporo.
            new Plant("Césped de la casa", 300f, -350f, 12, 60f), new Plant("Rosal del jardín", 300f, -350f, 6, 50f), new Plant("Seto de pitósporo", 300f, -350f, 4, 50f),
            new Plant("Césped de la casa", -450f, 500f, 12, 60f), new Plant("Rosal del jardín", -450f, 500f, 6, 50f), new Plant("Seto de pitósporo", 600f, 600f, 4, 50f),
            // Al norte, en Altadena: los cedros de la Christmas Tree Lane.
            new Plant("Cedro del Himalaya", 0f, 1450f, 8, 80f),
        },
        fauna = new[]
        {
            new Animal("Woodlouse", "Perro de la casa (en el microcosmos, una cochinilla hormiguera), en las calles al sur del campus", 300f, -350f, 3, 0.7f, 60f),
            new Animal("Woodlouse", "Gato de la casa, en las calles del norte", -450f, 500f, 3, 0.5f, 60f),
            new Animal("Woodlouse", "Perro de la casa, en Lake Avenue", 600f, 600f, 2, 0.7f, 50f),
        },
        buildings = new[]
        {
            new Building("Campus de Caltech", 0f, 0f), new Building("Colorado Boulevard", 0f, 900f), new Building("Arroyo Seco", -1050f, 0f),
            new Building("Montes de San Gabriel", 0f, 1600f), new Building("Christmas Tree Lane (Altadena)", 0f, 1450f),
        },
        todo = new[] { "en vivo: que los olivos queden en el paseo del campus y no sobre el laboratorio; que las palmeras caigan en la acera de California Boulevard" },
    };

    // ══ DAYTON Y KILL DEVIL HILLS, 17 de diciembre de 1903 (los Wright): sobre la Mecánica ═════════════════════════════════════════════════════
    // La historia junta los dos sitios de los Wright: el taller de bicicletas de West Third Street en Dayton (el escenario) y la duna de Kill Devil Hills
    // en los Outer Banks de Carolina del Norte, donde volaron ese día. El mapa sigue el plano de Dayton (las calles, la casa de Hawthorn Street, el
    // Great Miami y el Wolf Creek, con sus árboles sin hoja en diciembre) y pone la duna al sureste, en su dirección (a ~800 km de verdad): el barrón y la
    // avena de mar sobre la arena, el brezo de playa, el mirto de cera y la marisma del lado del estrecho, conchas; el cangrejo fantasma en su madriguera.
    static Map MapDayton() => new Map
    {
        display = "Dayton", place = "Dayton (Ohio) y la duna de Kill Devil Hills (Carolina del Norte), 17 de diciembre de 1903", status = Status.Hecho,
        source = "NPS, Dayton Aviation Heritage (Wright Cycle Company, 1127 West Third Street; la casa del 7 de Hawthorn Street); Sanborn Map, Dayton 1897 (el plano del pueblo); " +
                 "NPS, Wright Brothers National Memorial (Big Kill Devil Hill, la duna de arena y su vegetación de barrón y avena de mar); diario de Orville Wright, 17-12-1903",
        floor = Natural(
            new Piece("alfombra", Apilar, "las alfombras, apiladas: Big Kill Devil Hill, la duna desde la que planeaban"),
            new Piece("mesa", Como, "los bancos del taller: los bancos de arena de los Outer Banks", "mesa"),
            new Piece("lavabo", Como, "los lavabos: el Great Miami y el Wolf Creek", "lavabo")),
        flora = new[]
        {
            // Dayton en diciembre: olmos sin hoja en West Third Street, arces en Hawthorn Street, sicómoros en el Great Miami y en el Wolf Creek.
            new Plant("Olmo americano de la calle", 300f, 30f, 6, 40f), new Plant("Olmo americano de la calle", -400f, 30f, 6, 40f),
            new Plant("Arce plateado de la calle", 60f, -200f, 6, 40f),
            new Plant("Sicómoro del Great Miami", 1050f, 200f, 8, 80f), new Plant("Sicómoro del Great Miami", 1080f, -400f, 8, 80f), new Plant("Sicómoro del Great Miami", -300f, 560f, 6, 60f),
            // Al sureste, la duna de Kill Devil Hills: barrón y avena de mar sobre la arena, brezo, conchas; del lado del estrecho, mirto de cera y la marisma.
            new Plant("Barrón americano", 600f, -900f, 30, 200f), new Plant("Avena de mar", 600f, -900f, 12, 150f), new Plant("Brezo de playa", 600f, -900f, 10, 150f),
            new Plant("Arena de duna", 600f, -900f, 6, 150f), new Plant("Concha", 450f, -1050f, 4, 60f), new Plant("Nopal oriental", 650f, -850f, 6, 80f),
            new Plant("Mirto de cera", 800f, -1000f, 6, 60f), new Plant("Heno de marisma", 800f, -1050f, 8, 80f),
        },
        fauna = new[]
        {
            new Animal("Aphid", "Caballo de carro en West Third Street (en el microcosmos, un pulgón de tiro)", 500f, 0f, 3, 2.4f, 60f),
            new Animal("Aphid", "Caballo de reparto junto a Williams Street", -200f, 0f, 2, 2.4f, 40f),
            new Animal("Woodlouse", "Gato de Hawthorn Street (en el microcosmos, una cochinilla hormiguera)", 60f, -200f, 2, 0.5f, 30f),
            new Animal("Antlion", "Hormiga león (Myrmeleon immaculatus), aletargada en su embudo de la duna", 600f, -900f, 3, 1.2f, 150f),
            new Animal("Spider", "Araña lobo de playa (Arctosa littoralis)", 600f, -900f, 2, 1.4f, 150f),
            new Animal("Ladybug", "Mariquita de 9 puntos (Coccinella novemnotata), invernando en el mirto", 800f, -1000f, 2, 1f, 50f),
            new Animal("GhostCrab", "Cangrejo fantasma (Ocypode quadrata), en su madriguera de invierno", 450f, -1050f, 2, 1.6f, 60f),
        },
        buildings = new[]
        {
            new Building("Wright Cycle Company (1127 West Third Street)", 0f, 0f), new Building("Casa de los Wright (7 Hawthorn Street)", 60f, -250f), new Building("Plaza", 700f, 60f),
            new Building("Estación", 800f, -200f), new Building("Great Miami", 1050f, 0f), new Building("Duna de Kill Devil Hills (hacia el sureste)", 600f, -900f),
        },
        todo = new[] { "en vivo: que la duna quede lejos de las calles de Dayton y que los sicómoros caigan en la orilla del Great Miami, no en el río" },
    };
}
