/// <summary>
/// LOS MAPAS A MANO DE LA MÚSICA (docs/microcosmos-musica.md; regla M9). Todos «Hecho»: el piso ordenado para el lugar real, cada rodal de
/// flora y cada bicho en su sitio (metros desde la «Historia», x al este, z al norte; el mismo marco que <c>CivicBuilder.MusicPlans</c>).
/// </summary>
public static partial class MicroStoryMaps
{
    const MicroFloorElements.Action Quitar = MicroFloorElements.Action.Quitar, Apilar = MicroFloorElements.Action.Apilar, Como = MicroFloorElements.Action.Como;

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

        // ══ VEGA BAJA, 2016 (Bad Bunny): sobre Informática (pendiente de su sala de grabación) ═══════════════════════════════════════
        new Map
        {
            display = "Vega Baja", place = "Vega Baja (Puerto Rico), 2016", status = Status.Hecho,
            source = "Reserva Natural Laguna Tortuguero (DRNA); Little y Wadsworth 1964; OpenStreetMap",
            floor = new[]
            {
                new Piece("maquina", Como, "los armarios del clúster son los MOGOTES del karst del norte de Puerto Rico: mesetas de caliza con bosque arriba", "mesa"),
                new Piece("silla", Como, "ni nieve en el Caribe: las sillas son mogotes menores", "mesa"),
                new Piece("alfombra", Quitar, "el llano de la costa no tiene desniveles"),
                new Piece("pared", Como, "el borde del llano sube hacia el karst en terrazas de pasto", "escalera"),
            },
            flora = new[]
            {
                new Plant("Ausubo", -240f, -600f, 10, 90f), new Plant("Ausubo", 330f, -620f, 8, 70f), new Plant("Ausubo", -880f, -560f, 6, 60f),
                new Plant("Palma de sombrero", -120f, -640f, 6, 50f), new Plant("Palma de sombrero", 470f, -600f, 5, 40f),
                new Plant("Helecho del mogote", -260f, -560f, 14, 70f), new Plant("Helecho del mogote", 310f, -570f, 12, 60f),
                new Plant("Enea de la laguna", 700f, 470f, 18, 60f), new Plant("Enea de la laguna", 880f, 470f, 14, 50f),
                new Plant("Yerba del llano", 300f, 470f, 24, 120f), new Plant("Yerba del llano", -500f, 470f, 20, 110f),
                new Plant("Flamboyán", -60f, 150f), new Plant("Flamboyán", -200f, 150f), new Plant("Flamboyán", -60f, 270f), new Plant("Flamboyán", -200f, 270f),
                new Plant("Palma real", 40f, -30f), new Plant("Palma real", 150f, -30f), new Plant("Palma real", 260f, -30f), new Plant("Palma real", -160f, -30f),
                new Plant("Amapola", 120f, 90f, 4, 8f), new Plant("Amapola", -270f, 140f, 4, 8f),
                new Plant("Caliza del mogote", -200f, -640f, 6, 40f),
            },
            fauna = new[]
            {
                new Animal("Cricket", "Grillo de la noche", -240f, -470f, 4, 1.8f, 40f),
                new Animal("Firefly", "Cucubano (Pyrophorus), en los mogotes", 330f, -560f, 4, 1.2f, 40f),
                new Animal("Ant", "Hormiga brava", 300f, 460f, 5, 1.2f, 30f),
                new Animal("Mantis", "Santateresa, en la enea", 760f, 470f, 2, 2.2f, 30f),
            },
            buildings = new[]
            {
                new Building("Econo Plaza Vega Baja", 90f, 110f), new Building("Iglesia del centro", -280f, 190f), new Building("Casa de la familia Martínez Ocasio", -230f, -540f),
                new Building("Escuela de Almirante Sur", -130f, -520f), new Building("Plaza", -130f, 210f), new Building("Hospital de Vega Baja", 320f, 170f),
            },
            todo = new[] { "verificar la situación real del Econo Plaza; en vivo, que la enea quede en la orilla de la laguna" },
        },

        // ══ MONTREAL, 1 de junio de 1969 (John Lennon y Yoko Ono): sobre la Sala de Ballet (pendiente de su piso) ══════════════════════
        new Map
        {
            display = "Montreal", place = "Queen Elizabeth Hotel, Montreal, 1 de junio de 1969", status = Status.Hecho,
            source = "Wikipedia «Bed-in»; Les amis de la montagne; Ville de Montréal (arbres publics); OpenStreetMap",
            floor = new[]
            {
                new Piece("espejo", Como, "la pared de espejo no es un glaciar en junio: son las torres de Place Ville Marie y del centro de Montreal", "cajonera"),
                new Piece("silla", Como, "sin nieve en junio: las sillas y bancos son cerros de la falda del Mont Royal", "mesa"),
                new Piece("mesa", Como, "el piano y las mesas: el Mont Royal, con su meseta de bosque", "mesa"),
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

        // ══ GARY, 1966 (los Jackson): sobre la Sala de Ballet (pendiente de su escenario) ═════════════════════════════════════════════
        new Map
        {
            display = "Gary", place = "Gary (Indiana), 1966", status = Status.Hecho,
            source = "WBEZ 2017; Indiana Dunes (National Park Service); OpenStreetMap",
            floor = new[]
            {
                new Piece("espejo", Como, "la pared de espejo es la ACERÍA de la US Steel a lo largo del lago: altos hornos y chimeneas", "maquina"),
                new Piece("silla", Como, "ni nevados en Indiana: las sillas y bancos son las dunas del lago", "cojin"),
                new Piece("mesa", Como, "el piano y las mesas: las naves de la acería", "maquina"),
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

        // ══FIN DE LOS MAPAS══
    };
}
