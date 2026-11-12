/// <summary>
/// LOS MAPAS A MANO DE LA MÚSICA (docs/microcosmos-musica.md; regla M9). Todos «Hecho»: el piso ordenado para el lugar real, cada rodal de
/// flora y cada bicho en su sitio (metros desde la «Historia», x al este, z al norte; el mismo marco que <c>CivicBuilder.MusicPlans</c>).
/// </summary>
public static partial class MicroStoryMaps
{
    public static readonly Map[] Music =
    {
        // ══ LA PLAYA (Barranquilla), 2009 (Shakira): sobre el Aula ══════════════════════════════════════════════════════════════════
        new Map
        {
            display = "Barranquilla", place = "Corregimiento de La Playa, Barranquilla, 2009", status = Status.Hecho,
            source = "Fundación Pies Descalzos, Historia; Humboldt 2014 (bosque seco); OpenStreetMap",
            floor = new[]
            {
                new Piece("silla", Como, "ni nieve en el Caribe: las sillas son cerros de las lomas de Puerto Colombia", "mesa"),
                new Piece("libro", Como, "los libros del Aula son las casas de bloque de colores: el pueblo sube por las estanterías", "cajonera"),
                new Piece("pared", Como, "la pizarra y la pantalla no son acantilados: la barranca baja de la orilla de la Ciénaga", "escalera"),
                new Piece("alfombra", Quitar, "el suelo del corregimiento es llano: arena y cemento"),
            },
            flora = new[]
            {
                new Plant("Mangle rojo", -300f, 370f, 12, 60f), new Plant("Mangle rojo", 220f, 375f, 10, 50f), new Plant("Mangle salado", -40f, 365f, 10, 50f),
                new Plant("Pasto de la orilla", -520f, 380f, 18, 60f), new Plant("Pasto de la orilla", 480f, 385f, 14, 50f),
                new Plant("Trupillo", -700f, -470f, 12, 80f), new Plant("Trupillo", -800f, 110f, 8, 60f), new Plant("Trupillo", 760f, 120f, 8, 60f),
                new Plant("Cardón", -760f, -620f, 6, 50f), new Plant("Cardón", 790f, -420f, 5, 40f),
                new Plant("Almendro de playa", -80f, -105f), new Plant("Almendro de playa", 80f, -105f), new Plant("Almendro de playa", -230f, 35f), new Plant("Almendro de playa", 230f, 35f),
                new Plant("Roble morado", -60f, -235f), new Plant("Roble morado", 60f, -235f), new Plant("Roble morado", -60f, -330f), new Plant("Roble morado", 60f, -330f),
                new Plant("Matarratón", -340f, 80f, 6, 18f), new Plant("Matarratón", 400f, 80f, 6, 18f),
                new Plant("Trinitaria", 150f, 80f, 4, 8f), new Plant("Trinitaria", -150f, 80f, 4, 8f),
                new Plant("Arena de la Ciénaga", -160f, 400f, 6, 40f), new Plant("Arena de la Ciénaga", 300f, 405f, 5, 30f),
            },
            fauna = new[]
            {
                new Animal("Ant", "Hormiga arriera (Atta colombica), hacia los trupillos", -690f, -440f, 6, 1.6f, 25f),
                new Animal("Firefly", "Cocuyo del manglar", -280f, 360f, 4, 1.2f, 40f),
                new Animal("Grasshopper", "Saltamontes de la seca", 760f, 100f, 4, 2.4f, 40f),
                new Animal("Cricket", "Grillo del solar", 620f, -520f, 3, 1.8f, 40f),
            },
            buildings = new[]
            {
                new Building("Colegio Pies Descalzos de La Playa", 0f, 130f), new Building("Comedor escolar", 140f, 170f), new Building("Centro comunitario", -140f, 160f),
                new Building("Cancha del colegio", 200f, -70f), new Building("Puesto de salud de La Playa", 330f, -100f), new Building("Plaza", -60f, -280f), new Building("Mercado", 170f, -290f),
            },
            todo = new[] { "en vivo: mirar que el manglar quede en la orilla de la Ciénaga y no sobre una calle" },
        },

        // ══ VEGA BAJA, 2016 (Bad Bunny): sobre el piso de Música ═══════════════════════════════════════
        new Map
        {
            display = "Vega Baja", place = "Vega Baja (Puerto Rico), 2016", status = Status.Hecho,
            source = "Reserva Natural Laguna Tortuguero (DRNA); Little y Wadsworth 1964; OpenStreetMap",
            floor = new[]
            {
                new Piece("mesa", Como, "la mesa de mezclas y el banco son los MOGOTES del karst del norte de Puerto Rico: mesetas de caliza con bosque arriba", "mesa"),
                new Piece("silla", Como, "ni nieve en el Caribe: las sillas son mogotes menores", "mesa"),
                new Piece("alfombra", Quitar, "el llano de la costa no tiene desniveles"),
                new Piece("pared", Como, "el borde del llano sube hacia el karst en terrazas de pasto", "escalera"),
            },
            flora = new[]
            {
                new Plant("Ausubo", -216f, -468f, 10, 90f), new Plant("Ausubo", 297f, -484f, 8, 70f), new Plant("Ausubo", -792f, -437f, 6, 60f),
                new Plant("Palma de sombrero", -108f, -499f, 6, 50f), new Plant("Palma de sombrero", 423f, -468f, 5, 40f),
                new Plant("Helecho del mogote", -234f, -437f, 14, 70f), new Plant("Helecho del mogote", 279f, -445f, 12, 60f),
                new Plant("Enea de la laguna", 630f, 367f, 18, 60f), new Plant("Enea de la laguna", 792f, 367f, 14, 50f),
                new Plant("Yerba del llano", 270f, 367f, 24, 120f), new Plant("Yerba del llano", -450f, 367f, 20, 110f),
                new Plant("Flamboyán", -54f, 117f), new Plant("Flamboyán", -180f, 117f), new Plant("Flamboyán", -54f, 211f), new Plant("Flamboyán", -180f, 211f),
                new Plant("Palma real", 36f, -23f), new Plant("Palma real", 135f, -23f), new Plant("Palma real", 234f, -23f), new Plant("Palma real", -144f, -23f),
                new Plant("Amapola", 108f, 70f, 4, 8f), new Plant("Amapola", -243f, 109f, 4, 8f),
                new Plant("Caliza del mogote", -180f, -499f, 6, 40f),
            },
            fauna = new[]
            {
                new Animal("Cricket", "Grillo de la noche", -216f, -367f, 4, 1.8f, 40f),
                new Animal("Firefly", "Cucubano (Pyrophorus), en los mogotes", 297f, -437f, 4, 1.2f, 40f),
                new Animal("Ant", "Hormiga brava", 270f, 359f, 5, 1.2f, 30f),
                new Animal("Mantis", "Santateresa, en la enea", 684f, 367f, 2, 2.2f, 30f),
            },
            buildings = new[]
            {
                new Building("Econo Plaza Vega Baja", 81f, 86f), new Building("Iglesia del centro", -252f, 148f), new Building("Casa de la familia Martínez Ocasio", -207f, -421f),
                new Building("Escuela de Almirante Sur", -117f, -406f), new Building("Plaza", -117f, 164f), new Building("Hospital de Vega Baja", 288f, 133f),
            },
            todo = new[] { "verificar la situación real del Econo Plaza; en vivo, que la enea quede en la orilla de la laguna" },
        },

        // ══ MONTREAL, 1 de junio de 1969 (John Lennon y Yoko Ono): sobre el piso de Música ══════════════════════
        new Map
        {
            display = "Montreal", place = "Queen Elizabeth Hotel, Montreal, 1 de junio de 1969", status = Status.Hecho,
            source = "Wikipedia «Bed-in»; Les amis de la montagne; Ville de Montréal (arbres publics); OpenStreetMap",
            floor = new[]
            {
                new Piece("pared", Como, "los paños de la cabina de cristal: las torres de Place Ville Marie y del centro de Montreal", "cajonera"),
                new Piece("silla", Como, "sin nieve en junio: las sillas y bancos son cerros de la falda del Mont Royal", "mesa"),
                new Piece("mesa", Como, "la mesa de mezclas y el banco: el Mont Royal, con su meseta de bosque", "mesa"),
                new Piece("alfombra", Quitar, "el centro de la ciudad es llano y está asfaltado"),
            },
            flora = new[]
            {
                new Plant("Arce de azúcar", -760f, 560f, 18, 130f), new Plant("Roble rojo", -600f, 620f, 12, 100f), new Plant("Arce de azúcar", -880f, 640f, 10, 80f),
                new Plant("Helecho del monte", -720f, 520f, 14, 90f), new Plant("Hierba de la falda", -470f, 610f, 18, 90f), new Plant("Roca del Mont Royal", -780f, 590f, 6, 60f),
                new Plant("Olmo americano", -260f, 10f), new Plant("Olmo americano", -130f, 10f), new Plant("Olmo americano", 240f, 10f), new Plant("Olmo americano", 370f, 10f),
                new Plant("Olmo americano", -340f, 440f), new Plant("Olmo americano", -180f, 440f), new Plant("Olmo americano", 300f, 440f), new Plant("Olmo americano", 450f, 440f),
                new Plant("Arce de Noruega", -440f, 120f), new Plant("Arce de Noruega", -400f, 300f), new Plant("Arce de Noruega", 110f, 310f), new Plant("Arce de Noruega", 70f, 410f),
                new Plant("Lila", -330f, 60f, 4, 10f), new Plant("Lila", 470f, 500f, 4, 10f),
                new Plant("Césped de la plaza", -300f, 110f, 12, 18f), new Plant("Césped de la plaza", 480f, 530f, 12, 20f),
            },
            fauna = new[]
            {
                new Animal("Ant", "Hormiga del Mont Royal (Formica)", -700f, 520f, 5, 1.2f, 50f),
                new Animal("Ladybug", "Mariquita del arce", -600f, 610f, 3, 1f, 60f),
                new Animal("Cicada", "Cigarra del arce (Neotibicen), todavía bajo tierra en junio: una sola, temprana", -640f, 560f, 1, 2f, 20f),
            },
            buildings = new[]
            {
                new Building("Queen Elizabeth Hotel", 60f, 40f), new Building("Gare Centrale", -90f, -80f), new Building("Estudio de André Perry", 220f, 90f), new Building("Universidad McGill", 170f, 370f),
                new Building("Place des Arts", 620f, 340f), new Building("YMCA de la rue Drummond", -320f, 340f), new Building("Hospital Royal Victoria", -560f, 390f),
            },
            todo = new[] { "verificar la posición del estudio de André Perry en 1969 (no consta en las fuentes de la ficha)" },
        },

        // ══ GARY, 1966 (los Jackson): sobre el piso de Música ═════════════════════════════════════════════
        new Map
        {
            display = "Gary", place = "Gary (Indiana), 1966", status = Status.Hecho,
            source = "WBEZ 2017; Indiana Dunes (National Park Service); OpenStreetMap",
            floor = new[]
            {
                new Piece("pared", Como, "los paños de la cabina de cristal son la ACERÍA de la US Steel a lo largo del lago: altos hornos y chimeneas", "maquina"),
                new Piece("silla", Como, "ni nevados en Indiana: las sillas y bancos son las dunas del lago", "cojin"),
                new Piece("mesa", Como, "la mesa de mezclas y el banco: las naves de la acería", "maquina"),
                new Piece("alfombra", Quitar, "la llanura del lago Míchigan no tiene desniveles"),
            },
            flora = new[]
            {
                new Plant("Barrón de las dunas", 760f, 660f, 20, 90f), new Plant("Barrón de las dunas", 900f, 620f, 14, 70f),
                new Plant("Álamo de Virginia", -840f, 480f, 8, 70f), new Plant("Álamo de Virginia", 840f, 480f, 6, 60f), new Plant("Roble negro de la duna", 800f, 590f, 6, 60f),
                new Plant("Ailanto", -300f, 460f, 8, 80f), new Plant("Ailanto", 160f, 460f, 8, 80f), new Plant("Escoria de la acería", -100f, 500f, 8, 60f), new Plant("Escoria de la acería", 400f, 500f, 6, 50f),
                new Plant("Olmo americano", -60f, 15f), new Plant("Olmo americano", 60f, 15f), new Plant("Olmo americano", -160f, 15f), new Plant("Olmo americano", 170f, 15f),
                new Plant("Olmo americano", 610f, 30f), new Plant("Olmo americano", 610f, -80f), new Plant("Arce plateado", 15f, 90f), new Plant("Arce plateado", 15f, -90f), new Plant("Arce plateado", 15f, -230f), new Plant("Arce plateado", 345f, 60f),
                new Plant("Césped del patio", -300f, -380f, 12, 20f), new Plant("Césped del patio", -210f, 60f, 6, 12f),
            },
            fauna = new[]
            {
                new Animal("Firefly", "Luciérnaga (Photinus pyralis), en los patios", -300f, -380f, 5, 1.2f, 40f),
                new Animal("Ant", "Hormiga de las aceras (Tetramorium)", 60f, 10f, 5, 1f, 30f),
                new Animal("Grasshopper", "Saltamontes de la duna", 780f, 620f, 3, 2f, 40f),
            },
            buildings = new[]
            {
                new Building("Casa de 2300 Jackson Street", -60f, 60f), new Building("Theodore Roosevelt High School", -140f, -220f), new Building("Mr. Lucky's", -620f, 90f), new Building("Puerta de la acería", 240f, 390f),
                new Building("Steeltown Records", 700f, 230f), new Building("St. Mary Mercy Hospital", 420f, 80f),
            },
            todo = new[] { "verificar la distancia real entre Jackson Street y Roosevelt High School en un plano de 1966; Steeltown (dirección no verificada)" },
        },

        // ══ SEÚL, 2016 (BLACKPINK): sobre la Sala de Ballet (representativa) ═══════════════════════════════════════════════════════════
        new Map
        {
            display = "Seúl", place = "Hapjeong-dong (Mapo-gu), Seúl, agosto de 2016", status = Status.Hecho,
            source = "Seoul Metropolitan Government (árboles de calle); National Institute of Biological Resources; OpenStreetMap",
            floor = new[]
            {
                new Piece("espejo", Como, "la pared de espejo son las torres de pisos de Mapo (no un glaciar en agosto)", "cajonera"),
                new Piece("silla", Como, "sin nieve en agosto: las sillas son los cerros del monte Wausan", "mesa"),
                new Piece("alfombra", Quitar, "el barrio está asfaltado"),
            },
            flora = new[]
            {
                new Plant("Carrizo del Han", -600f, -445f, 24, 90f), new Plant("Carrizo del Han", 100f, -470f, 20, 90f), new Plant("Hierba de la orilla", -280f, -400f, 20, 110f),
                new Plant("Bloque de la ribera", -40f, -470f, 6, 60f), new Plant("Pino rojo coreano", 840f, 560f, 14, 110f), new Plant("Pino rojo coreano", 740f, 620f, 8, 70f),
                new Plant("Ginkgo", -180f, -160f), new Plant("Ginkgo", -60f, -70f), new Plant("Ginkgo", 80f, 30f), new Plant("Ginkgo", 210f, 120f), new Plant("Ginkgo", 340f, 200f),
                new Plant("Plátano de sombra", 225f, -120f), new Plant("Plátano de sombra", 225f, -10f), new Plant("Plátano de sombra", 225f, 240f), new Plant("Plátano de sombra", 225f, 350f),
                new Plant("Cerezo Yoshino", -500f, -345f, 6, 40f), new Plant("Cerezo Yoshino", -300f, -345f, 6, 40f),
                new Plant("Césped del parque", -540f, -390f, 14, 30f), new Plant("Césped del parque", -160f, -390f, 10, 30f),
            },
            fauna = new[]
            {
                new Animal("Cicada", "Cigarra de Seúl (Hyalessa maculaticollis), en los árboles de la calle", 80f, 30f, 6, 2f, 80f),
                new Animal("Ladybug", "Mariquita asiática (Harmonia axyridis), en los cerezos", -400f, -345f, 4, 1.2f, 40f),
                new Animal("Grasshopper", "Saltamontes del carrizal", -600f, -445f, 3, 2f, 50f),
            },
            buildings = new[]
            {
                new Building("Edificio de la agencia YG", -60f, -40f), new Building("Sala de ensayo de las aprendizas", -120f, 60f), new Building("Residencia de las aprendizas", -230f, 190f),
                new Building("Mercado de Mangwon", -640f, 180f), new Building("Sala de conciertos de Hongdae", 480f, 390f),
            },
            todo = new[] { "la residencia de las aprendizas no consta: su sitio es representativo" },
        },

        // ══ KINGSTON, 22 de abril de 1978 (Bob Marley y los Wailers): sobre el piso de Música ═════════════════
        new Map
        {
            display = "Kingston", place = "Kingston (Jamaica), 22 de abril de 1978", status = Status.Hecho,
            source = "Adams 1972, Flowering Plants of Jamaica; Trench Town Culture Yard; OpenStreetMap",
            floor = new[]
            {
                new Piece("pared", Como, "los paños de la cabina de cristal: la muralla de las Montañas Azules", "pared"),
                new Piece("silla", Como, "sin nieve: las sillas y bancos son las lomas de Long Mountain", "mesa"),
                new Piece("mesa", Como, "la mesa de mezclas y el banco: las mesetas de las Montañas Azules, con bosque arriba", "mesa"),
                new Piece("alfombra", Quitar, "el suelo de Kingston es la llanura de Liguanea"),
            },
            flora = new[]
            {
                new Plant("Helecho de las Montañas Azules", 840f, 600f, 14, 100f), new Plant("Mahoe azul", 760f, 660f, 10, 90f), new Plant("Mahoe azul", 900f, 560f, 6, 60f),
                new Plant("Acacia de la ladera", 880f, -160f, 12, 100f), new Plant("Cactus de Long Mountain", 860f, -100f, 8, 80f), new Plant("Grava de la llanura", 930f, -210f, 6, 50f),
                new Plant("Hierba de guinea", -800f, -500f, 22, 110f), new Plant("Hierba de guinea", 620f, -360f, 14, 90f),
                new Plant("Lignum vitae", 60f, -30f), new Plant("Lignum vitae", 190f, -30f), new Plant("Lignum vitae", 60f, 150f), new Plant("Lignum vitae", 190f, 150f),
                new Plant("Almendro de la India", -130f, -80f), new Plant("Almendro de la India", -130f, -190f), new Plant("Almendro de la India", -600f, -30f), new Plant("Almendro de la India", -480f, -30f),
                new Plant("Cocotero", 280f, 540f), new Plant("Cocotero", 380f, 540f), new Plant("Cocotero", -10f, -500f), new Plant("Cocotero", 130f, -500f),
                new Plant("Buganvilla", -660f, -20f, 4, 8f), new Plant("Buganvilla", 320f, 500f, 4, 8f),
            },
            fauna = new[]
            {
                new Animal("Cricket", "Grillo de los patios", -640f, -90f, 5, 1.8f, 50f),
                new Animal("Firefly", "Peenie-wallie (luciérnaga, Photinus)", 320f, 300f, 5, 1.2f, 50f),
                new Animal("Ant", "Hormiga de Kingston", -130f, -130f, 5, 1.2f, 30f),
                new Animal("Grasshopper", "Saltamontes de la seca", -800f, -500f, 4, 2.2f, 60f),
            },
            buildings = new[]
            {
                new Building("Estadio Nacional", 120f, 60f), new Building("Government Yard de First Street", -640f, -40f), new Building("Patio de Joe Higgs", -700f, -110f), new Building("Studio One", -300f, 230f),
                new Building("Casa de Hope Road", 330f, 540f), new Building("Coronation Market", -130f, -420f), new Building("Kingston Public Hospital", -40f, -310f),
            },
            todo = new[] { "las distancias están a un tercio: en vivo, mirar que Trench Town y el estadio se lean como barrios distintos" },
        },

        // ══ BELFAST, 19 de mayo de 1998 (U2): sobre el piso de Música ═════════════════════════════════════
        new Map
        {
            display = "Belfast", place = "Waterfront Hall, Belfast, 19 de mayo de 1998", status = Status.Hecho,
            source = "Belfast Hills Partnership; Belfast City Council (árboles); OpenStreetMap",
            floor = new[]
            {
                new Piece("pared", Como, "los paños de la cabina de cristal: el acantilado de basalto de Cave Hill", "pared"),
                new Piece("silla", Como, "sin nieve en mayo: las sillas y bancos son las colinas de Black Mountain", "mesa"),
                new Piece("mesa", Como, "la mesa de mezclas y el banco: los astilleros Harland & Wolff y sus grúas", "maquina"),
                new Piece("alfombra", Quitar, "la orilla del Lagan es llana"),
            },
            flora = new[]
            {
                new Plant("Aulaga", -880f, 560f, 14, 110f), new Plant("Aulaga", -920f, 170f, 10, 90f), new Plant("Brezo de la colina", -800f, 640f, 18, 100f), new Plant("Brezo de la colina", -860f, 240f, 12, 80f),
                new Plant("Serbal", -780f, 540f, 6, 70f), new Plant("Basalto de Cave Hill", -900f, 650f, 8, 60f),
                new Plant("Junco de la orilla", 80f, -360f, 14, 40f), new Plant("Junco de la orilla", 230f, 360f, 14, 40f), new Plant("Junco de la orilla", 80f, 120f, 8, 25f),
                new Plant("Tilo", -300f, -20f), new Plant("Tilo", -180f, -20f), new Plant("Tilo", -60f, -20f), new Plant("Tilo", -400f, -180f), new Plant("Tilo", -400f, 100f),
                new Plant("Plátano de sombra", -500f, -380f), new Plant("Plátano de sombra", -590f, -520f), new Plant("Plátano de sombra", 90f, -280f), new Plant("Plátano de sombra", 170f, -440f),
                new Plant("Narciso del parterre", -320f, -60f, 8, 10f), new Plant("Césped del parque", 560f, -500f, 16, 40f), new Plant("Césped del parque", -320f, -100f, 8, 15f),
            },
            fauna = new[]
            {
                new Animal("Ant", "Hormiga negra de jardín (Lasius niger)", -320f, -100f, 5, 1f, 30f),
                new Animal("SnowBee", "Abejorro de Cave Hill (Bombus)", -880f, 560f, 3, 1.6f, 60f),
                new Animal("Earwig", "Tijereta de los parques", 560f, -500f, 2, 1.2f, 30f),
            },
            buildings = new[]
            {
                new Building("Waterfront Hall", -10f, 60f), new Building("Oficina del Sí", -160f, 150f), new Building("Maysfield Leisure Centre", 40f, -160f), new Building("Queen's University", -540f, -590f),
                new Building("Royal Victoria Hospital", -820f, -120f), new Building("Harland & Wolff", 620f, 200f), new Building("Crown Liquor Saloon", -470f, 40f),
            },
            todo = new[] { "la oficina del Sí no consta: su sitio es representativo. La línea de paz es una calle (sin muro: no hace falta para la historia)" },
        },

        // ══ LONG ISLAND, 1989 (Public Enemy): sobre el piso de Música ═══════════════════════════════
        new Map
        {
            display = "Long Island", place = "Roosevelt y Hempstead (Long Island), primavera de 1989", status = Status.Hecho,
            source = "NY DEC (Hempstead Lake State Park); Long Island Native Plant Initiative; OpenStreetMap",
            floor = new[]
            {
                new Piece("silla", Como, "sin nieve en abril y sin relieve en Long Island: las sillas son los bloques de oficinas de Garden City y Uniondale", "cajonera"),
                new Piece("mesa", Como, "la mesa de mezclas y el banco son el Nassau Coliseum y sus grandes aparcamientos", "maquina"),
                new Piece("pared", Como, "las pizarras y pantallas son las pantallas acústicas de la autopista: terrazas de tierra", "escalera"),
                new Piece("alfombra", Quitar, "la llanura de Hempstead es plana"),
            },
            flora = new[]
            {
                new Plant("Roble blanco", -700f, -440f, 14, 110f), new Plant("Pino de brea", -640f, -560f, 10, 80f), new Plant("Andropogon pequeño", -760f, -300f, 16, 90f),
                new Plant("Junco del lago", -780f, -140f, 12, 40f), new Plant("Junco del lago", -790f, 40f, 8, 30f), new Plant("Grava glaciar", -640f, -320f, 5, 40f),
                new Plant("Roble blanco", 760f, 600f, 10, 90f), new Plant("Andropogon pequeño", 640f, 590f, 10, 70f),
                new Plant("Arce rojo", 20f, 30f), new Plant("Arce rojo", 20f, 140f), new Plant("Arce rojo", 20f, 250f), new Plant("Arce rojo", -20f, -80f), new Plant("Arce rojo", -20f, -210f),
                new Plant("Roble de los pantanos", -100f, 620f), new Plant("Roble de los pantanos", 20f, 590f), new Plant("Roble de los pantanos", 200f, 590f), new Plant("Roble de los pantanos", 350f, 590f),
                new Plant("Cornejo en flor", 90f, 200f, 4, 12f), new Plant("Cornejo en flor", 120f, 640f, 4, 12f),
                new Plant("Césped de la casa", 480f, -270f, 14, 30f),
            },
            fauna = new[]
            {
                new Animal("Ant", "Hormiga de las aceras (Tetramorium)", 90f, 240f, 5, 1f, 30f),
                new Animal("Woodlouse", "Cochinilla del jardín", 480f, -270f, 3, 1f, 30f),
                new Animal("Spider", "Araña de jardín del parque del lago", -700f, -420f, 2, 1.4f, 40f),
            },
            buildings = new[]
            {
                new Building("Universidad Adelphi", 120f, 660f), new Building("WBAU", 40f, 680f), new Building("510 South Franklin", -190f, 540f), new Building("Casa de los Ridenhour", -130f, -30f),
                new Building("Nassau Coliseum", 840f, 380f), new Building("Hospital del condado", 630f, -30f),
            },
            todo = new[] { "verificar en un plano de 1989 el número de South Franklin Street; la casa de los Ridenhour no consta: su sitio es representativo" },
        },

        // ══ LA PLAZA DE LAS CANCIONES (inventada): sobre el piso de Música ═════════════════════════════════════════
        new Map
        {
            display = "Grandes aportes de las canciones", place = "La Plaza de las Canciones (lugar inventado), un 21 de junio", status = Status.Hecho,
            source = "Inventado; flora de cada rincón: la de su lugar real (Tennessee, valle del Po, Ascot, Cabo Oriental, Kingston, Andes)",
            floor = new[]
            {
                new Piece("silla", Como, "las sillas: los montes de cada rincón, sin nieve salvo arriba en los Andes", "mesa"),
                new Piece("libro", Como, "los libros del Aula: estratos de historia, se dejan como acantilados de capas", "libro"),
                new Piece("alfombra", Quitar, "el suelo del pueblo es llano"),
            },
            flora = new[]
            {
                new Plant("Roble de Tennessee", -820f, 560f, 10, 90f), new Plant("Arroz del Po", -700f, -300f, 28, 110f), new Plant("Roble inglés", -160f, 620f, 6, 70f), new Plant("Pradera de Ascot", -60f, 590f, 18, 90f),
                new Plant("Acacia de espinas", 700f, 370f, 8, 70f), new Plant("Áloe del Cabo", 760f, 300f, 6, 50f),
                new Plant("Mahoe de Kingston", 740f, -30f, 5, 50f), new Plant("Ichu de los Andes", 760f, -480f, 22, 110f), new Plant("Queñual", 680f, -540f, 6, 60f), new Plant("Piedra de los Andes", 840f, -420f, 8, 60f),
                new Plant("Almendro de la plaza", -120f, -20f), new Plant("Almendro de la plaza", 120f, -20f), new Plant("Almendro de la plaza", -240f, -20f), new Plant("Almendro de la plaza", 240f, -20f),
                new Plant("Flor de la plaza", -100f, -50f, 4, 8f), new Plant("Flor de la plaza", 100f, -50f, 4, 8f), new Plant("Césped de la plaza", 300f, 290f, 14, 25f),
            },
            fauna = new[]
            {
                new Animal("Firefly", "Luciérnaga del arrozal", -700f, -300f, 4, 1.2f, 60f),
                new Animal("Cricket", "Grillo de la tarde", -60f, 590f, 4, 1.6f, 50f),
                new Animal("Cicada", "Cigarra del paseo", 120f, -20f, 3, 1.8f, 60f),
                new Animal("Grasshopper", "Saltamontes de los Andes", 760f, -480f, 3, 2f, 50f),
            },
            buildings = new[]
            {
                new Building("Escuela Highlander", -560f, 300f), new Building("Arrozal de las mondine", -600f, -230f), new Building("Tittenhurst Park", -80f, 500f), new Building("Comunidad de Ginsberg", 640f, 300f),
                new Building("Patio de Trench Town", 650f, -60f), new Building("Pueblo de los Andes", 610f, -370f), new Building("Sala de conciertos", 70f, 110f),
            },
            todo = new[] { "en vivo: que cada rincón se distinga del pueblo (suelo, flora y mobiliario de su año)" },
        },

        // ══FIN DE LOS MAPAS══
    };
}
