using UnityEngine;

/// <summary>LOS MAPAS A MANO DE LA TANDA 3 (IV): Langley, Viena y Leiden.</summary>
public static partial class MicroStoryMaps
{
    // ══ HAMPTON Y LANGLEY, febrero de 1962: sobre Informática ═══════════════════════════════════════════════════════════════════
    // Invierno suave: pinos y cedros, marismas pardas del río Back, jardines con acebos y camelias. Las máquinas del clúster, los edificios de
    // Langley; las mesas, las mesetas de la costa; las sillas, no nevados: en la costa de Virginia no hay montañas: las dunas de la bahía.
    static Map MapLangley() => new Map
    {
        display = "Langley", place = "Langley y Hampton, Virginia, febrero de 1962", status = Status.Hecho,
        source = "NASA, Langley Historic Sites; Shetterly 2016; OpenStreetMap, Hampton; Weakley et al. 2012, Flora of Virginia",
        floor = new[]
        {
            new Piece("alfombra", Apilar, "las alfombras, apiladas: las dunas de la punta de Old Point Comfort"),
            new Piece("silla", Como, "sin montañas en la costa: las sillas son dunas", "cojin"),
            new Piece("maquina", Como, "las máquinas del clúster: los edificios y túneles de Langley", "maquina"),
        },
        flora = new[]
        {
            new Plant("Pino taeda", -500f, 300f, 12, 120f), new Plant("Pino taeda", 700f, 240f, 10, 110f), new Plant("Pino taeda", -900f, -900f, 12, 130f),
            new Plant("Cedro rojo", -300f, 480f, 6, 60f), new Plant("Cedro rojo", 260f, -300f, 6, 60f),
            new Plant("Espartina", 200f, 730f, 30, 170f), new Plant("Espartina", -800f, 760f, 18, 120f), new Plant("Espartina", 860f, -900f, 16, 100f),
            new Plant("Acebo americano", 120f, -620f), new Plant("Acebo americano", -160f, -580f), new Plant("Acebo americano", -950f, -200f), new Plant("Acebo americano", 400f, -780f),
            new Plant("Camelia", 70f, -760f, 4, 20f), new Plant("Camelia", -240f, -700f, 4, 20f), new Plant("Camelia", -1030f, -290f, 3, 15f),
            new Plant("Césped de invierno", 60f, 40f, 20, 110f), new Plant("Césped de invierno", 600f, -620f, 14, 90f), new Plant("Bordillo", 480f, -500f, 6, 60f),
        },
        fauna = new[]
        {
            new Animal("Spider", "Araña casera", 60f, 100f, 3, 1.2f, 50f),
            new Animal("Ladybug", "Mariquita de invierno", 120f, -620f, 3, 1f, 40f),
            new Animal("Ant", "Hormiga casera (Tapinoma sessile)", 180f, -100f, 3, 0.9f, 30f),
            new Animal("Woodlouse", "Perro de la casa (en el microcosmos, una cochinilla hormiguera)", 110f, -700f, 3, 0.7f, 70f),
            new Animal("Woodlouse", "Gato de la casa (en el microcosmos, una cochinilla hormiguera)", -1000f, -240f, 2, 0.5f, 50f),
        },
        buildings = new[] { new Building("Sección de cálculo", 60f, 100f), new Building("Sala del ordenador IBM", -150f, 120f), new Building("Túnel supersónico", 220f, 180f), new Building("Hampton High School", 330f, -640f), new Building("Hampton Institute", 860f, -520f), new Building("Casa de Katherine Johnson", 110f, -700f) },
        todo = new[] { "en vivo: los edificios de la NASA estaban repartidos entre el este y el oeste de Langley: aquí, juntos alrededor del claro" },
    };

    // ══ VIENA, 7 de mayo de 1824 (Beethoven): sobre la Sala de Ballet (pendiente de su sala de conciertos) ═════════════════════
    // Una ciudad amurallada entre prados: castaños en flor en el Prater, tilos en el glacis, sauces en el canal. El suelo de la sala de ballet es
    // el glacis; los espejos de la sala no son glaciares, sino la fachada de la catedral; las barras, las murallas.
    static Map MapViena() => new Map
    {
        display = "Viena 1824", place = "Viena, dentro y fuera de las murallas, 7 de mayo de 1824", status = Status.Hecho,
        source = "Plano de Vasquez (hacia 1830); Wien Geschichte Wiki; Fischer 2008, Exkursionsflora von Österreich",
        floor = Natural(
            new Piece("alfombra", Apilar, "las alfombras, apiladas: los montes de Kahlenberg, al norte"),
            new Piece("espejo", Como, "los espejos del ballet: la fachada de piedra de San Esteban y las de las casas", "pared"),
            new Piece("silla", Como, "las sillas: las colinas de los Bosques de Viena, sin nieve en mayo", "mesa")),
        flora = new[]
        {
            new Plant("Castaño de Indias", 1250f, 760f, 16, 220f), new Plant("Castaño de Indias", 1000f, 500f, 6, 70f), new Plant("Castaño de Indias", 80f, -300f, 6, 180f),
            new Plant("Tilo", -300f, -260f, 7, 90f), new Plant("Tilo", 420f, -260f, 7, 90f), new Plant("Tilo", -560f, 300f, 5, 60f),
            new Plant("Lila", 960f, -500f, 3, 15f), new Plant("Lila", -220f, -520f, 4, 30f), new Plant("Lila", 620f, -420f, 4, 30f),
            new Plant("Sauce del Danubio", 300f, 960f, 12, 150f), new Plant("Álamo", 900f, 620f, 10, 110f), new Plant("Álamo", -880f, 1000f, 8, 110f),
            new Plant("Prado del glacis", 80f, -300f, 26, 240f), new Plant("Diente de león", -400f, -280f, 10, 100f),
            new Plant("Vid de los suburbios", -1000f, -1000f, 24, 180f), new Plant("Empedrado", 120f, 420f, 6, 60f),
        },
        fauna = new[]
        {
            new Animal("DungBeetle", "Escarabajo de mayo (Melolontha melolontha)", 1250f, 760f, 3, 1.6f, 120f),
            new Animal("SnowBee", "Abeja de los castaños (Apis mellifera)", 1000f, 500f, 4, 1.2f, 70f),
            new Animal("Ant", "Hormiga negra (Lasius niger)", 80f, -300f, 4, 0.8f, 100f),
            new Animal("Aphid", "Caballo del coche (en el microcosmos, un pulgón de tiro)", 100f, 200f, 3, 2f, 120f),
            new Animal("Woodlouse", "Perro de la ciudad (en el microcosmos, una cochinilla hormiguera)", 200f, 400f, 3, 0.7f, 100f),
        },
        buildings = new[] { new Building("Teatro de la Puerta de Carintia", 60f, 90f), new Building("Casa de la Ungargasse", 950f, -480f), new Building("Catedral de San Esteban", 180f, 460f), new Building("Gesellschaft der Musikfreunde", -200f, 300f), new Building("Taller de Streicher", -200f, -560f) },
        todo = new[] { "en vivo: que la Ungargasse quede fuera de las murallas, cruzando el glacis" },
    };

    // ══ LEIDEN, 8 de junio de 1637 (Descartes): sobre el Aula ══════════════════════════════════════════════════════════════════
    // Una ciudad de canales en un llano: olmos y tilos en los muelles, sauces y carrizos en los fosos, prados al sur. Todo es llano: los muebles
    // del Aula no pueden ser montañas: las mesas son mesetas muy bajas de pólder; las sillas, los molinos y las torres de las iglesias.
    static Map MapLeiden() => new Map
    {
        display = "Leiden", place = "Leiden, Provincias Unidas, 8 de junio de 1637", status = Status.Hecho,
        source = "Plano de Blaeu (1649); Erfgoed Leiden; Lunsingh Scheurleer et al. 1975, Het Rapenburg; Weeda et al. 1985",
        floor = Natural(
            new Piece("alfombra", Apilar, "las alfombras, apiladas: el Burcht, la mota de la ciudad"),
            new Piece("silla", Como, "en Holanda no hay nevados: las sillas son las torres de las iglesias y de los molinos", "columna"),
            new Piece("libro", Como, "los libros de la biblioteca son libros: la ciudad de la imprenta", "libro")),
        flora = new[]
        {
            new Plant("Olmo holandés", -130f, -320f, 7, 100f), new Plant("Olmo holandés", -130f, 180f, 5, 60f), new Plant("Olmo holandés", 30f, -300f, 6, 90f), new Plant("Olmo holandés", 500f, 370f, 5, 90f),
            new Plant("Tilo", 300f, 190f, 5, 60f), new Plant("Tilo", 620f, 130f, 4, 50f),
            new Plant("Sauce podado", -880f, 200f, 10, 120f), new Plant("Sauce podado", 1100f, 40f, 9, 110f),
            new Plant("Carrizo", -860f, 500f, 18, 90f), new Plant("Carrizo", 1120f, 320f, 16, 90f), new Plant("Carrizo", 200f, -620f, 14, 100f),
            new Plant("Lirio amarillo", -80f, -380f, 6, 30f), new Plant("Lirio amarillo", -860f, 300f, 6, 40f),
            new Plant("Prado del pólder", -1000f, -880f, 28, 240f), new Plant("Rosa de jardín", -210f, -170f, 6, 40f), new Plant("Rosa de jardín", -260f, 200f, 3, 15f),
            new Plant("Adoquín", 300f, 180f, 5, 90f),
        },
        fauna = new[]
        {
            new Animal("SnowBee", "Abejorro (Bombus terrestris)", -210f, -170f, 4, 1.4f, 60f),
            new Animal("Ladybug", "Mariquita de siete puntos", 500f, 370f, 3, 1f, 60f),
            new Animal("Ant", "Hormiga negra (Lasius niger)", -130f, -320f, 4, 0.8f, 60f),
            new Animal("Aphid", "Caballo de la barca de sirga (en el microcosmos, un pulgón de tiro)", -560f, 400f, 2, 2f, 40f),
            new Animal("Aphid", "Vaca del pólder (en el microcosmos, un pulgón)", -1000f, -880f, 4, 2f, 160f),
            new Animal("Woodlouse", "Gato de la imprenta (en el microcosmos, una cochinilla hormiguera)", 100f, 40f, 3, 0.5f, 60f),
        },
        buildings = new[] { new Building("Imprenta de Jan Maire", 100f, 40f), new Building("Academiegebouw", -120f, -40f), new Building("Hortus botanicus", -240f, -190f), new Building("Pieterskerk", 280f, -140f), new Building("Plaza del mercado", 500f, 240f), new Building("Sala de esgrima", -320f, -320f) },
        todo = new[] { "en vivo: que el Rapenburg cruce el claro por el oeste y que los muebles del Aula no parezcan montañas en un llano" },
    };
}
