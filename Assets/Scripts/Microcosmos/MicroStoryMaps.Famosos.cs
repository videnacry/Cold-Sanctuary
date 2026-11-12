using UnityEngine;

/// <summary>LOS MAPAS A MANO DE LOS FAMOSOS (2026-11-12; petición 75, regla M9; docs/microcosmos-mapas-a-mano.md §Tanda 4).</summary>
public static partial class MicroStoryMaps
{

    // ══ VELLIANGIRI, 24 de junio de 1999 (Sadhguru y el Dhyanalinga): sobre la Sala de Yoga ═══════════════════════════════════════════════════
    // El Isha Yoga Center al pie de los montes Velliangiri (el «Kailash del sur», al oeste): el Dhyanalinga bajo su cúpula, la sala de práctica y las
    // cabañas; el arroyo que baja hacia la Noyyal; al este, la aldea de Ikkarai Boluvampatti con sus cocotales, platanares, nims y vacas. En la falda,
    // el bosque seco caducifolio de los Ghats occidentales: bambú, teca, lantana, la flor de la gloria; arriba, la hierba del monte sobre el granito.
    static Map MapVelliangiri() => new Map
    {
        display = "Velliangiri", place = "Isha Yoga Center, al pie de los montes Velliangiri (Coimbatore, Tamil Nadu), 24 de junio de 1999", status = Status.Hecho,
        source = "Isha Foundation, «Dhyanalinga» (consagrado el 24-06-1999); Tamil Nadu Forest Department, Boluvampatti Range (bosque seco caducifolio " +
                 "con teca y bambú, Reserva de la Biosfera del Nilgiri); OpenStreetMap, «Isha Yoga Center» e Ikkarai Boluvampatti; el plano del pueblo",
        floor = Natural(
            new Piece("alfombra", Como, "las esterillas: los cocotales y los arrozales del llano de Boluvampatti", "planta"),
            new Piece("cojin", Como, "los cojines: las siete colinas de los montes Velliangiri", "roca")),
        flora = new[]
        {
            // La falda y los montes: bambú, teca, la hierba del monte sobre el granito.
            new Plant("Bambú", -560f, -380f, 10, 120f), new Plant("Teca", -700f, 0f, 8, 150f), new Plant("Hierba del monte", -1000f, 250f, 12, 200f),
            new Plant("Granito de los Ghats", -1100f, 300f, 6, 150f),
            // La lantana de las orillas del camino y la flor de la gloria en el monte bajo.
            new Plant("Lantana", -200f, -200f, 8, 100f), new Plant("Flor de la gloria", -400f, 100f, 4, 100f),
            // La aldea: cocotales, platanares, nims y el patio; el jazmín y el nim del ashram.
            new Plant("Cocotero", 460f, 380f, 12, 150f), new Plant("Banano del huerto", 500f, 300f, 6, 100f), new Plant("Nim", 700f, -220f, 4, 80f),
            new Plant("Hierba del patio", 650f, -300f, 6, 100f), new Plant("Nim", -150f, 200f, 4, 60f), new Plant("Jazmín", 120f, 200f, 4, 40f),
        },
        fauna = new[]
        {
            new Animal("Ant", "Hormiga tejedora (Oecophylla smaragdina), con sus nidos de hojas en la teca", -700f, 0f, 3, 0.9f, 150f),
            new Animal("Ant", "Hormiga tejedora, en los nims de la aldea", 700f, -220f, 2, 0.9f, 80f),
            new Animal("SnowBee", "Abeja gigante (Apis dorsata), con sus panales en las peñas del monte", -1000f, 250f, 2, 1.8f, 200f),
            new Animal("Cicada", "Cigarra de los Ghats (Platypleura), en el bambú de la falda", -560f, -380f, 3, 3.5f, 120f),
            new Animal("Firefly", "Luciérnaga, en el arroyo de la Noyyal", -320f, 260f, 3, 1.2f, 60f),
            new Animal("Mantis", "Mantis (Hierodula), en la lantana", -200f, -200f, 2, 7f, 100f),
            new Animal("Aphid", "Vaca de la aldea (en el microcosmos, un pulgón)", 750f, -300f, 3, 2.4f, 100f),
            new Animal("Woodlouse", "Perro de la aldea (en el microcosmos, una cochinilla hormiguera)", 600f, -150f, 2, 0.7f, 80f),
        },
        buildings = new[]
        {
            new Building("Dhyanalinga", 0f, 140f), new Building("Sala de práctica", -150f, 60f), new Building("Cabañas de los voluntarios", 200f, 90f),
            new Building("Aldea de Ikkarai Boluvampatti", 700f, -220f), new Building("Montes Velliangiri", -1550f, 250f),
        },
        todo = new[] { "en vivo: que los cocotales queden en el llano de la aldea y la hierba del monte en la falda, no en el ashram" },
    };

    // ══ HAWTHORNE, 28 de septiembre de 2008 (Elon Musk y el cuarto Falcon 1): sobre el Control de Misión ═══════════════════════════════════════
    // La sede de SpaceX en la antigua nave de fuselajes de Northrop (Rocket Road, junto a Crenshaw Boulevard): desde su control de misión se siguió el
    // cuarto lanzamiento del Falcon 1, el primero en llegar a órbita (desde Kwajalein). Al oeste de Crenshaw, el aeropuerto municipal; al norte, la
    // autopista 105; alrededor, la cuadrícula de casas de estuco con césped, jacarandas y buganvillas, y las palmeras de abanico de los bulevares.
    // Una ciudad: solo lo plantado y sus animales (perros y gatos de las casas).
    static Map MapHawthorne() => new Map
    {
        display = "Hawthorne", place = "Hawthorne (Los Ángeles): la sede de SpaceX en Rocket Road y sus calles, 28 de septiembre de 2008", status = Status.Hecho,
        source = "SpaceX, «Falcon 1 Flight 4» (28-09-2008, primer cohete de combustible líquido de financiación privada en órbita); Vance, Elon Musk (2015); " +
                 "OpenStreetMap (1 Rocket Road, Crenshaw Boulevard, aeropuerto municipal de Hawthorne); City of Hawthorne, arbolado urbano; el plano del pueblo",
        floor = Natural(
            new Piece("mesa", Como, "las mesas de las consolas: las naves de la fábrica y del aeropuerto", "mesa"),
            new Piece("pared", Como, "las pantallas y los paneles: los muros de sonido de la autopista 105", "pared")),
        flora = new[]
        {
            // Las palmeras de abanico de Crenshaw Boulevard.
            new Plant("Palmera de abanico", -80f, -600f, 10, 150f), new Plant("Palmera de abanico", -80f, 600f, 8, 150f),
            // Las calles de casas: jacarandas, césped y buganvillas.
            new Plant("Jacaranda", 800f, 600f, 8, 150f), new Plant("Jacaranda", 800f, -650f, 8, 150f), new Plant("Césped de la casa", 1000f, 600f, 8, 150f),
            new Plant("Césped de la casa", -700f, -650f, 8, 150f), new Plant("Buganvilla", 600f, -650f, 6, 120f),
            // El césped segado entre las pistas del aeropuerto y los bordillos de Rocket Road.
            new Plant("Césped (Cynodon", -620f, 150f, 6, 200f), new Plant("Bordillo de hormigón", 250f, 120f, 4, 60f),
        },
        fauna = new[]
        {
            new Animal("Woodlouse", "Perro de la casa (en el microcosmos, una cochinilla hormiguera)", 900f, 600f, 3, 0.7f, 120f),
            new Animal("Woodlouse", "Perro de la casa", -700f, -650f, 2, 0.7f, 120f),
            new Animal("Woodlouse", "Gato de la casa (en el microcosmos, una cochinilla hormiguera)", 800f, -650f, 2, 0.5f, 120f),
            new Animal("Woodlouse", "Gato de la casa", 1100f, 300f, 2, 0.5f, 100f),
        },
        buildings = new[]
        {
            new Building("Nave de SpaceX (antigua nave de fuselajes de Northrop)", 150f, 80f), new Building("Control de misión de SpaceX", 60f, 80f),
            new Building("Aeropuerto municipal de Hawthorne", -620f, 150f), new Building("Autopista 105", 0f, 920f), new Building("Casas de estuco", 800f, 600f),
        },
        todo = new[] { "en vivo: que las palmeras queden a los lados de Crenshaw y el césped entre las pistas, no sobre ellas" },
    };

    // ══ ALDEA DE LA MONTAÑA, un amanecer de primavera (Goku; HOMENAJE, FICCIÓN): sobre las Artes Marciales ════════════════════════════════════════
    // No es un lugar real ni de la obra: su plano es inventado (CivicBuilder.Famosos) al modo de las aldeas de los montes Wudang y Song. Lo que sí es
    // real es su naturaleza, la del monte subtropical de China central en primavera: el torrente que baja de una cascada al norte, el monte del maestro
    // al noroeste con pinos de Masson y azaleas, el bambú, los helechos y el musgo de la piedra húmeda; bancales de arroz y de té en las laderas.
    static Map MapAldeaMontana() => new Map
    {
        display = "Aldea de la montaña", place = "La aldea de la montaña (homenaje a Dragon Ball; plano inventado, sin lugar real), un amanecer de primavera", status = Status.Hecho,
        source = "Plano inventado (CivicBuilder.Famosos, homenaje a Toriyama, Dragon Ball); la naturaleza, real: Flora of China (Pinus massoniana, " +
                 "Phyllostachys edulis, Rhododendron simsii, Camellia sinensis); UNESCO, Conjunto de edificios antiguos de los montes Wudang (1994)",
        floor = Natural(
            new Piece("alfombra", Como, "los tatamis: los bancales de arroz y de té de las laderas", "planta"),
            new Piece("espejo", Como, "los espejos: la poza de la cascada", "banera")),
        flora = new[]
        {
            // El monte del maestro y la colina de los pinos: pinos de Masson, azaleas, granito.
            new Plant("Pino de Masson", 700f, 600f, 10, 150f), new Plant("Pino de Masson", -1000f, 700f, 8, 200f), new Plant("Azalea", -600f, 800f, 6, 150f),
            new Plant("Granito del monte", -1100f, 850f, 6, 150f), new Plant("Granito del monte", 650f, -450f, 4, 80f),
            // El bambú y los helechos junto al torrente; el musgo en el paredón de la cascada.
            new Plant("Bambú (Phyllostachys edulis", -400f, 400f, 10, 150f), new Plant("Helecho del monte", -80f, 600f, 8, 80f), new Plant("Musgo de la piedra", -200f, 950f, 6, 60f),
            // Los bancales de té, el bambú de los patios y la hierba del camino de la cumbre.
            new Plant("Camelia del té", -500f, -300f, 12, 150f), new Plant("Bambú del patio", -300f, 180f, 4, 60f), new Plant("Hierba del sendero", 360f, -180f, 6, 100f),
        },
        fauna = new[]
        {
            new Animal("Cicada", "Cigarra (Cryptotympana atrata), en los pinos de la colina", 700f, 600f, 3, 4f, 150f),
            new Animal("Firefly", "Luciérnaga del arrozal, en los bancales de arroz", 380f, -150f, 4, 1.2f, 150f),
            new Animal("Grasshopper", "Saltamontes del bancal (Oxya chinensis)", 380f, -150f, 3, 3f, 150f),
            new Animal("Mantis", "Mantis china (Tenodera sinensis), en el té", -500f, -300f, 2, 8f, 150f),
            new Animal("Ant", "Hormiga del monte (Camponotus japonicus), en la cumbre del amanecer", 650f, -450f, 3, 0.8f, 80f),
            new Animal("Ladybug", "Gallina de la aldea (en el microcosmos, una mariquita parda)", 200f, 250f, 3, 0.5f, 80f),
            new Animal("Woodlouse", "Perro de la aldea (en el microcosmos, una cochinilla hormiguera)", -200f, -150f, 2, 0.7f, 60f),
        },
        buildings = new[]
        {
            new Building("Cabaña del maestro", -300f, 180f), new Building("Templo de la aldea", 150f, 320f), new Building("Cascada y su poza", -260f, 980f),
            new Building("Cumbre del amanecer", 650f, -450f), new Building("Campo de entrenar", 520f, 300f),
        },
        todo = new[] { "en vivo: que el bambú y los helechos queden en la orilla del torrente y no en el agua" },
    };

    // ══ ALBUQUERQUE, primavera de 1975 (Bill Gates, Paul Allen y el Altair): sobre la Informática ══════════════════════════════════════════════
    // El noreste de la ciudad junto a la Central Avenue (la Ruta 66): MITS, que fabricaba el Altair 8800, en un pequeño centro comercial; Micro-Soft
    // nace aquí el 4 de abril. Al este, la sierra de Sandía y el alto desierto (grama azul, chamisa, cholla sobre la grava); al oeste, el valle del Río
    // Grande con su bosque de álamos; en las calles, olmos siberianos, enebros y céspedes regados. Una ciudad: solo lo plantado y sus perros y gatos,
    // y en el borde el desierto (sin bichos salvajes en el bioma).
    static Map MapAlbuquerque() => new Map
    {
        display = "Albuquerque", place = "Albuquerque (Nuevo México): la Central Avenue (Ruta 66) junto a MITS, primavera de 1975", status = Status.Hecho,
        source = "Allen, Idea Man (2011: MITS, el Altair y el motel de la Ruta 66); Popular Electronics, enero de 1975 (el Altair 8800); Microsoft, fundación " +
                 "el 4-04-1975; USDA PLANTS y la Ciudad de Albuquerque (el alto desierto y el bosque del Río Grande); OpenStreetMap; el plano del pueblo",
        floor = Natural(
            new Piece("mesa", Como, "los escritorios: las mesetas del alto desierto al pie de la sierra de Sandía", "mesa"),
            new Piece("pared", Como, "los paneles: la sierra de Sandía", "pared")),
        flora = new[]
        {
            // El alto desierto al este, hacia la sierra de Sandía.
            new Plant("Grama azul", 1200f, 0f, 10, 200f), new Plant("Chamisa", 1300f, 600f, 8, 200f), new Plant("Cholla", 1500f, 200f, 6, 200f),
            new Plant("Grava del alto desierto", 1600f, 500f, 6, 200f),
            // El bosque de álamos del Río Grande, al oeste.
            new Plant("Álamo del Río Grande", -1350f, -200f, 10, 150f),
            // Los olmos siberianos de la Central Avenue y de Lomas; los enebros y céspedes de las casas.
            new Plant("Olmo siberiano", 600f, -220f, 8, 150f), new Plant("Olmo siberiano", -200f, 330f, 6, 150f), new Plant("Enebro del jardín", 700f, 650f, 6, 150f),
            new Plant("Césped de la casa", 700f, -650f, 8, 150f), new Plant("Césped de la casa", -200f, 650f, 8, 150f),
        },
        fauna = new[]
        {
            new Animal("Woodlouse", "Perro de la casa (en el microcosmos, una cochinilla hormiguera)", 800f, 650f, 3, 0.7f, 150f),
            new Animal("Woodlouse", "Perro de la casa", -200f, -650f, 2, 0.7f, 150f),
            new Animal("Woodlouse", "Gato de la casa (en el microcosmos, una cochinilla hormiguera)", 700f, -650f, 2, 0.5f, 150f),
            new Animal("Woodlouse", "Gato de la casa", -300f, 650f, 2, 0.5f, 150f),
        },
        buildings = new[]
        {
            new Building("MITS (el centro comercial de la Central Avenue)", -80f, 0f), new Building("Motel de la Ruta 66", 300f, -260f),
            new Building("Universidad de Nuevo México", -900f, -100f), new Building("Sierra de Sandía", 1800f, 600f), new Building("Bosque del Río Grande", -1350f, 0f),
        },
        todo = new[] { "en vivo: que los álamos queden en la orilla del Río Grande y el desierto al este, más allá de San Mateo" },
    };

    // ══ OMAHA, 30 de agosto de 1983 (Warren Buffett y la señora B): sobre el Taller Textil ══════════════════════════════════════════════════════
    // La Nebraska Furniture Mart de Rose Blumkin en la calle 72, al sur de Dodge Street: Buffett la compra el día de su 53 cumpleaños con un apretón de
    // manos. Al este (acortado), Farnam Street con la casa de Buffett y Kiewit Plaza (la oficina de Berkshire), Memorial Park con sus robles; al oeste,
    // un resto de pradera de hierba alta. Agosto: fresnos y arces de calle, céspedes, petunias. Una ciudad: solo lo plantado y sus perros y gatos.
    static Map MapOmaha() => new Map
    {
        display = "Omaha", place = "Omaha (Nebraska): la Nebraska Furniture Mart en la calle 72, Farnam Street y Memorial Park, 30 de agosto de 1983", status = Status.Hecho,
        source = "Berkshire Hathaway, carta a los accionistas de 1983 (la compra de la Nebraska Furniture Mart); Lowenstein, Buffett (1995); Nebraska Furniture " +
                 "Mart, historia (en la calle 72 desde 1970); OpenStreetMap (Farnam Street, Memorial Park); Nebraska Statewide Arboretum; el plano del pueblo",
        floor = Natural(
            new Piece("alfombra", Como, "las alfombras: el gran almacén de alfombras de la señora B", "suelo"),
            new Piece("mesa", Como, "las mesas de corte: las manzanas de la cuadrícula de Omaha", "mesa")),
        flora = new[]
        {
            // El resto de pradera de hierba alta del oeste.
            new Plant("Andropogon", -1200f, -900f, 10, 200f), new Plant("Equinácea", -1100f, -800f, 6, 200f),
            // Los fresnos verdes de Dodge Street.
            new Plant("Fresno verde", -600f, 800f, 8, 150f), new Plant("Fresno verde", 600f, 800f, 8, 150f),
            // Farnam Street: los arces plateados y las petunias frente a la casa de Buffett.
            new Plant("Arce plateado", 1200f, 560f, 6, 100f), new Plant("Petunias", 1250f, 640f, 4, 60f),
            // Memorial Park y sus robles; los céspedes de las casas.
            new Plant("Roble rojo", 1000f, 1000f, 8, 150f), new Plant("Césped de la casa", 900f, 300f, 8, 150f), new Plant("Césped de la casa", -400f, -800f, 8, 150f),
            // Los bordillos del aparcamiento de la tienda.
            new Plant("Bordillo de hormigón", 100f, -200f, 4, 60f),
        },
        fauna = new[]
        {
            new Animal("Woodlouse", "Perro de la casa (en el microcosmos, una cochinilla hormiguera)", 900f, 300f, 3, 0.7f, 150f),
            new Animal("Woodlouse", "Perro de la casa", -400f, -800f, 2, 0.7f, 150f),
            new Animal("Woodlouse", "Gato de la casa (en el microcosmos, una cochinilla hormiguera)", 1200f, 600f, 2, 0.5f, 100f),
            new Animal("Woodlouse", "Gato de la casa", -200f, -600f, 2, 0.5f, 150f),
        },
        buildings = new[]
        {
            new Building("Nebraska Furniture Mart", 60f, 80f), new Building("Casa de Warren Buffett (Farnam Street)", 1250f, 600f),
            new Building("Kiewit Plaza (la oficina de Berkshire Hathaway)", 950f, 520f), new Building("Memorial Park", 1000f, 1000f),
            new Building("Universidad de Nebraska en Omaha", 800f, 1100f),
        },
        todo = new[] { "en vivo: que los fresnos queden a los lados de Dodge Street y la pradera en el borde suroeste" },
    };

    // ══ PRAIRIE VIEW, 15 de diciembre de 2020 (MacKenzie Scott y Prairie View A&M): sobre el Aula ═══════════════════════════════════════════════
    // El campus de la universidad históricamente negra de Texas, sobre la antigua finca de Alta Vista (condado de Waller): University Drive, la avenida
    // del campus con la administración y la biblioteca John B. Coleman, las residencias y la granja experimental; al sur, la autopista 290 y el pueblo;
    // al norte, la loma de los pinos taeda. La sabana de robles post del este de Texas en diciembre: pastos agostados, yaupon con sus bayas rojas;
    // en el campus, encinas de Virginia y árboles de Júpiter. Las hormigas de fuego y las arañas lobo; los perros del pueblo.
    static Map MapPrairieView() => new Map
    {
        display = "Prairie View", place = "Prairie View A&M (condado de Waller, Texas): el campus, la granja y el pueblo, 15 de diciembre de 2020", status = Status.Hecho,
        source = "Prairie View A&M University, «Campus Map» y el anuncio del donativo de MacKenzie Scott (diciembre de 2020); Scott, «384 Ways to Help» " +
                 "(15-12-2020); Texas Parks and Wildlife, ecorregión de la sabana de robles post; OpenStreetMap; el plano del pueblo",
        floor = Natural(
            new Piece("mesa", Como, "los pupitres: los edificios del campus en su cuadrícula", "mesa"),
            new Piece("pared", Como, "la pizarra y los paneles: el pinar de la loma, al norte", "pared")),
        flora = new[]
        {
            // La sabana de robles post y los pastos del condado de Waller, al noreste.
            new Plant("Roble post", 1000f, 700f, 8, 200f), new Plant("Pasto del condado", 1000f, 700f, 10, 200f),
            // La loma de los pinos taeda, al norte, con yaupon.
            new Plant("Pino taeda", -400f, 900f, 10, 150f), new Plant("Yaupon", -200f, 800f, 6, 120f),
            // La granja experimental.
            new Plant("Pasto del condado", -800f, 450f, 8, 150f),
            // El campus: encinas de Virginia, árboles de Júpiter y el césped de la plaza.
            new Plant("Encina de Virginia", -100f, 300f, 6, 100f), new Plant("Encina de Virginia", 100f, -250f, 6, 100f), new Plant("Árbol de Júpiter", -250f, 450f, 4, 80f),
            new Plant("Césped del campus", 200f, -250f, 8, 100f),
        },
        fauna = new[]
        {
            new Animal("Ant", "Hormiga roja de fuego (Solenopsis invicta), con sus montículos en los pastos", 1000f, 700f, 4, 0.4f, 200f),
            new Animal("Ant", "Hormiga roja de fuego, en la granja experimental", -800f, 450f, 3, 0.4f, 150f),
            new Animal("Spider", "Araña lobo (Hogna), en la hojarasca del pinar", -400f, 900f, 2, 1f, 150f),
            new Animal("Spider", "Araña lobo, en los pastos", 1100f, 600f, 2, 1f, 150f),
            new Animal("Woodlouse", "Perro del pueblo (en el microcosmos, una cochinilla hormiguera)", 700f, -600f, 3, 0.7f, 150f),
        },
        buildings = new[]
        {
            new Building("Administración de Prairie View A&M", 60f, 120f), new Building("Biblioteca John B. Coleman", -140f, 140f),
            new Building("Granja experimental", -800f, 450f), new Building("Pueblo de Prairie View", 700f, -600f), new Building("Autopista 290", 0f, -880f),
        },
        todo = new[] { "en vivo: que los pinos queden en la loma del norte y las encinas a lo largo de la avenida del campus" },
    };
}
