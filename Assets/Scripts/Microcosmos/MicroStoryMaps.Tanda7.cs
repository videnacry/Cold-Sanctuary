using UnityEngine;

/// <summary>LOS MAPAS A MANO DE LA TANDA 7 (2026-11-12; petición 75, regla M9; docs/microcosmos-mapas-a-mano.md §Tanda 6).</summary>
public static partial class MicroStoryMaps
{
    // ══ PATANJALI, invierno seco del s. II a. C.: sobre la Sala de Yoga ════════════════════════════════════════════════════════════════════════
    // Un āśrama en la llanura del Ganges en tiempos de los Śuṅga (cuando vivió el Patañjali gramático, autor del Mahābhāṣya, a quien la tradición
    // identifica con el de los Yoga Sūtra): las chozas bajo un pipal, el ghat que baja al río (oeste), la aldea con su pozo y sus campos de cebada y
    // mostaza. Invierno seco: la cebada madurando, la hierba kusha (el asiento de la práctica), la grama durva, el aak en los baldíos, el tulsi del patio.
    static Map MapPatanjali() => new Map
    {
        display = "Patanjali", place = "Un āśrama a orillas del Ganges, con su aldea y sus campos, invierno seco del s. II a. C.", status = Status.Hecho,
        source = "Bryant, The Yoga Sūtras of Patañjali (2009: la fecha y la identidad de Patañjali); Patañjali, Mahābhāṣya (la época Śuṅga); Thapar, " +
                 "Early India (2002: la aldea del Ganges, la cebada y la mostaza); Bhagavad Gītā VI.11 (el asiento de hierba kusha); el plano del pueblo",
        floor = Natural(
            new Piece("alfombra", Como, "las esterillas: los campos de cebada y mostaza de la aldea", "planta"),
            new Piece("espejo", Como, "el espejo: el Ganges, al oeste", "banera")),
        flora = new[]
        {
            // El āśrama: el pipal, la hierba kusha y el tulsi del patio.
            new Plant("Pipal", -50f, 200f, 2, 40f), new Plant("Hierba kusha", -150f, 250f, 10, 100f), new Plant("Tulsi", 120f, -100f, 4, 40f),
            // La orilla y el ghat: la grama durva, la juncia, la piedra de los escalones y el limo seco.
            new Plant("Grama durva", -150f, -250f, 8, 60f), new Plant("Juncia", -180f, 400f, 6, 60f), new Plant("Piedra", -170f, 30f, 4, 30f),
            new Plant("Limo seco", -150f, -600f, 6, 80f),
            // Los campos: la cebada madurando y la kusha de las lindes de la mostaza.
            new Plant("Cebada madurando", 520f, -320f, 12, 150f), new Plant("Hierba kusha", 520f, 360f, 8, 120f),
            // El baldío con aak y la calle de la aldea con sus boñigas.
            new Plant("Algodoncillo", 300f, -400f, 6, 100f), new Plant("Boñiga de vaca", 300f, 150f, 4, 60f),
        },
        fauna = new[]
        {
            new Animal("Termite", "Termita (Odontotermes obesus), con su termitero en el baldío", 300f, -400f, 4, 0.5f, 100f),
            new Animal("Cricket", "Grillo de dos manchas, en la cebada", 520f, -320f, 3, 2.5f, 150f),
            new Animal("SnowBee", "Abeja enana (Apis florea), en la mostaza en flor", 520f, 360f, 3, 0.8f, 120f),
            new Animal("Ladybug", "Mariquita de 7 puntos, comiendo pulgones en la mostaza", 520f, 360f, 2, 0.7f, 120f),
            new Animal("Aphid", "Pulgón de la mostaza (Lipaphis erysimi)", 520f, 360f, 4, 0.2f, 120f),
            new Animal("DungBeetle", "Escarabajo pelotero, en la calle de la aldea", 300f, 150f, 2, 2.5f, 60f),
            new Animal("Scorpion", "Escorpión gigante de bosque (Heterometrus), de noche en la kusha", -150f, 250f, 2, 9f, 100f),
        },
        buildings = new[]
        {
            new Building("Chozas del āśrama", 0f, 0f), new Building("Ghat del Ganges", -150f, 0f), new Building("Aldea y su pozo", 260f, 125f),
            new Building("Campos de cebada", 520f, -320f), new Building("Campos de mostaza", 520f, 360f),
        },
        todo = new[] { "en vivo: que la grama y la piedra del ghat queden en la orilla y no en el Ganges" },
    };

    // ══ LA TREGUA DE NAVIDAD, 25 de diciembre de 1914 (Saint-Yvon, Ploegsteert): sobre el Gimnasio ═════════════════════════════════════════════
    // Las dos líneas de trincheras a 50–100 m, la británica al sur y la sajona al norte, y entre ellas la tierra de nadie: pasto escarchado y barro
    // helado tras la helada de Nochebuena. Detrás de la británica, el bosque de Ploegsteert («Plugstreet Wood») y los puestos de socorro; detrás de la
    // alemana, el Douve y la colina de Messines. Los alemanes pusieron árboles de Navidad con velas en el parapeto; en la tierra de nadie se cantó,
    // se enterró a los muertos y se jugó al fútbol. Invierno: arañas de sábana activas en el pasto, carábidos, lombrices en el barro, babosas.
    static Map MapTregua1914() => new Map
    {
        display = "Tregua 1914", place = "Saint-Yvon (Ploegsteert, Flandes): las trincheras y la tierra de nadie, 25 de diciembre de 1914", status = Status.Hecho,
        source = "Brown y Seaton, Christmas Truce (1984: Saint-Yvon y los árboles en el parapeto); Bairnsfather, Bullets & Billets (1916); Imperial War Museums, " +
                 "«The Christmas Truce»; Commonwealth War Graves Commission, Lancashire Cottage Cemetery; el plano del pueblo",
        floor = Natural(
            new Piece("alfombra", Como, "las esterillas: la tierra de nadie, pasto escarchado y barro helado", "suelo"),
            new Piece("espejo", Como, "los espejos: el Douve y los cráteres con agua helada", "banera")),
        flora = new[]
        {
            // La tierra de nadie: pasto escarchado, escarcha y, en las trincheras, barro helado.
            new Plant("Ballico de pasto", 400f, 10f, 8, 25f), new Plant("Ballico de pasto", -450f, 10f, 8, 25f), new Plant("Escarcha", 200f, 10f, 6, 25f),
            new Plant("Barro helado", -600f, -40f, 6, 40f), new Plant("Barro helado", 600f, 60f, 6, 40f),
            // El campo de remolachas de detrás de la línea británica: romazas y terrones.
            new Plant("Romaza", 400f, -350f, 6, 100f), new Plant("Terrón", 400f, -350f, 4, 100f),
            // La trinchera de comunicación y el bosque de Ploegsteert: ortigas secas, pamplina y musgo.
            new Plant("Ortiga muerta", -260f, -300f, 6, 80f), new Plant("Musgo", -450f, -380f, 8, 150f), new Plant("Pamplina", -300f, -220f, 6, 60f),
            // La orilla del Douve y, hacia Messines, las amapolas en roseta.
            new Plant("Ranúnculo rastrero", 0f, 330f, 6, 40f), new Plant("Amapola en roseta", 300f, 700f, 6, 150f),
        },
        fauna = new[]
        {
            new Animal("Spider", "Araña de sábana (Erigone atra), activa en el pasto helado de la tierra de nadie", 400f, 10f, 3, 0.3f, 25f),
            new Animal("Spider", "Araña de sábana, en el pasto del lado oeste", -450f, 10f, 3, 0.3f, 25f),
            new Animal("DungBeetle", "Carábido de invierno (Nebria brevicollis), en la hojarasca de Ploegsteert", -450f, -380f, 3, 1.2f, 150f),
            new Animal("Earthworm", "Lombriz de tierra, en el campo de remolachas", 400f, -350f, 3, 15f, 100f),
            new Animal("Woodlouse", "Cochinilla (Porcellio scaber), en las tablas de la trinchera británica", -600f, -40f, 3, 1.4f, 40f),
            new Animal("Slug", "Babosa negra (Arion ater), en el bosque", -500f, -300f, 2, 12f, 120f),
        },
        buildings = new[]
        {
            new Building("Trinchera británica", 0f, -45f), new Building("Trinchera alemana (sajona)", 0f, 55f), new Building("Árboles de Navidad en el parapeto alemán", 350f, 70f),
            new Building("Campo del partido de fútbol", 150f, 10f), new Building("Bosque de Ploegsteert", -450f, -380f), new Building("Puesto de socorro (Lancashire Cottage)", -300f, -300f),
            new Building("Colina de Messines", 300f, 900f),
        },
        todo = new[] { "en vivo: que el pasto de la tierra de nadie quede entre las dos trincheras y nada caiga dentro de ellas salvo el barro" },
    };

    // ══ BRUCE LEE, de 1957 (Kowloon) a 1964 (Seattle): sobre el Dojo ═══════════════════════════════════════════════════════════════════════════
    // Dos barrios en un piso. Al oeste, Kowloon en los años 50: Nathan Road con sus banianos (la casa de la familia Lee, en el 218), Temple Street y
    // Shanghai Street, la escuela de Wing Chun de Yip Man en Lee Tat Street (Yau Ma Tei), el puerto Victoria al sur y, al norte, la ladera de la Roca del
    // León con su monte bajo subtropical (cogón, helechos, mirto de rosa, melastoma) en septiembre. Al este, Seattle: el restaurante de Ruby Chow en
    // Broadway y Jefferson, donde Bruce trabajó al llegar (1959), y la Universidad de Washington junto al lago Union.
    static Map MapBruceLee() => new Map
    {
        display = "Bruce Lee", place = "Kowloon (Yau Ma Tei y Nathan Road, 1957) y Seattle (Broadway y la Universidad de Washington, 1959–64)", status = Status.Hecho,
        source = "Polly, Bruce Lee: A Life (2018: el 218 de Nathan Road, la escuela de Yip Man, Ruby Chow y la Universidad de Washington); Hong Kong Map 1957 " +
                 "(Crown Lands and Survey Office); Hong Kong Herbarium (la flora de las laderas y los árboles de calle); Kroll's Map of Seattle (1960); el plano del pueblo",
        floor = Natural(
            new Piece("alfombra", Como, "los tatamis: las calles en cuadrícula de Kowloon y de Seattle", "suelo"),
            new Piece("espejo", Como, "los espejos: el puerto Victoria y el lago Union", "banera")),
        flora = new[]
        {
            // Kowloon: los banianos de Nathan Road, la bauhinia y el césped del parque, los setos de ixora de la plaza, las buganvillas.
            new Plant("Baniano de calle", -300f, 250f, 8, 150f), new Plant("Baniano de calle", -280f, -250f, 8, 150f), new Plant("Árbol de orquídeas", -430f, 120f, 6, 80f),
            new Plant("Césped de grama", -430f, 120f, 6, 80f), new Plant("Seto de ixora", -220f, 110f, 6, 80f), new Plant("Buganvilla", -520f, 80f, 4, 80f),
            // La arena de la orilla del puerto.
            new Plant("Arena de la bahía", -400f, -480f, 4, 30f),
            // La ladera de la Roca del León: cogón, helechos, mirto de rosa, melastoma, setas de termitero y granito.
            new Plant("Cogón", -400f, 800f, 10, 150f), new Plant("Helecho dicotómico", -250f, 750f, 8, 150f), new Plant("Mirto de rosa", -550f, 750f, 6, 150f),
            new Plant("Melastoma", -400f, 650f, 6, 120f), new Plant("Seta", -300f, 700f, 4, 100f), new Plant("Granito", -400f, 900f, 4, 100f),
            // Seattle: el césped de la Universidad de Washington y de los barrios.
            new Plant("Césped de grama", 520f, 300f, 8, 150f), new Plant("Césped de grama", 300f, 300f, 6, 120f),
        },
        fauna = new[]
        {
            new Animal("Termite", "Termita de Formosa, con las setas de su termitero en la ladera", -300f, 700f, 4, 0.6f, 100f),
            new Animal("Grasshopper", "Saltamontes del arroz (Oxya chinensis), en el cogón", -400f, 800f, 3, 3f, 150f),
            new Animal("Mantis", "Mantis (Hierodula patellifera), en el mirto de rosa", -550f, 750f, 2, 8f, 150f),
            new Animal("Spider", "Araña de seda dorada (Nephila pilipes), entre los helechos", -250f, 750f, 2, 4f, 150f),
            new Animal("Firefly", "Luciérnaga (Luciola), en la melastoma", -400f, 650f, 3, 1f, 120f),
            new Animal("Ladybug", "Mariquita zigzag, en los setos de ixora de Kowloon", -220f, 110f, 2, 0.5f, 80f),
        },
        buildings = new[]
        {
            new Building("Escuela de Yip Man (Lee Tat Street)", -300f, 50f), new Building("Casa de la familia Lee (218 Nathan Road)", -300f, -300f),
            new Building("Puerto Victoria", -400f, -640f), new Building("Restaurante de Ruby Chow", 300f, -100f), new Building("Universidad de Washington", 520f, 300f),
            new Building("Roca del León", -400f, 980f),
        },
        todo = new[] { "en vivo: que la ladera quede al norte de Kowloon y la arena en la orilla del puerto, no en el agua" },
    };

    // ══ IRENA SENDLER, otoño de 1942 (Varsovia): sobre la Enfermería ═══════════════════════════════════════════════════════════════════════════
    // El gueto al oeste (en el claro de la historia), cerrado por su muro; Leszno lo cruza de este a oeste, con el tribunal cuyas dos entradas (una
    // dentro y otra fuera del muro) sirvieron para sacar niños. Fuera, al este, Marszałkowska, el Jardín Sajón y el Vístula; al sur, Aleje
    // Jerozolimskie y la estación central; en el borde suroeste, Ochota, donde los nombres de los niños se enterraron en frascos bajo un manzano.
    // Otoño–invierno: hierbas ruderales en las grietas y los escombros (del asedio de 1939), escarcha; arañas y cucarachas en los sótanos.
    static Map MapIrenaSendler() => new Map
    {
        display = "Irena Sendler", place = "Varsovia: el gueto, Leszno, el Jardín Sajón y el Vístula, otoño de 1942", status = Status.Hecho,
        source = "Engelking y Leociak, The Warsaw Ghetto: A Guide to the Perished City (2009: el muro, Leszno y el tribunal); Mieszkowska, Irena Sendler: " +
                 "Mother of the Children of the Holocaust (2011: las rutas y los frascos bajo el manzano de la calle Lekarska); USHMM, «Warsaw»; Plan Warszawy 1939",
        floor = Natural(
            new Piece("cama", Como, "las camas: las manzanas de casas de vecinos de Varsovia", "cama"),
            new Piece("lavabo", Como, "los lavabos: el Vístula y las alcantarillas por las que salieron niños", "lavabo")),
        flora = new[]
        {
            // Los escombros del asedio de 1939 dentro del muro, con ortigas y cenizos secos.
            new Plant("Escombro", -500f, 400f, 8, 150f), new Plant("Escombro", -400f, -200f, 6, 120f), new Plant("Ortiga seca", -550f, -300f, 6, 120f),
            new Plant("Cenizo seco", -400f, -200f, 6, 120f),
            // Las grietas del empedrado de Leszno, con musgo plateado y llantén.
            new Plant("Musgo plateado", -300f, 160f, 8, 60f), new Plant("Musgo plateado", 400f, 160f, 6, 60f), new Plant("Llantén mayor", 200f, 160f, 8, 60f),
            // El Jardín Sajón: poa y dientes de león, y la escarcha de la mañana en la plaza.
            new Plant("Poa anual", 420f, 380f, 10, 120f), new Plant("Diente de león", 480f, 420f, 6, 100f), new Plant("Escarcha", 500f, -200f, 6, 100f),
        },
        fauna = new[]
        {
            new Animal("Spider", "Araña doméstica (Eratigena atrica), en los sótanos", -300f, 300f, 3, 1.4f, 100f),
            new Animal("Cockroach", "Cucaracha oriental, en los sótanos", -400f, -200f, 3, 2.4f, 120f),
            new Animal("Woodlouse", "Cochinilla de humedad (Porcellio scaber), bajo el escombro", -500f, 400f, 3, 1.4f, 150f),
            new Animal("Ladybug", "Mariquitas de 7 puntos invernando en grupo, en el muro del Jardín Sajón", 420f, 380f, 4, 0.7f, 100f),
            new Animal("Ant", "Hormiga negra (Lasius niger), al sol del mediodía", 480f, 420f, 2, 0.4f, 100f),
        },
        buildings = new[]
        {
            new Building("Muro del gueto", -200f, 250f), new Building("Tribunal de Leszno (dos entradas)", -100f, 160f), new Building("Jardín Sajón", 420f, 380f),
            new Building("Estación central", 560f, -640f), new Building("El manzano de la calle Lekarska (Ochota)", -750f, -750f), new Building("Vístula", 900f, 0f),
        },
        todo = new[] { "en vivo: que lo de dentro del muro quede al oeste de Marszałkowska y el Jardín Sajón al este" },
    };

    // ══ LE CHAMBON-SUR-LIGNON, otoño de 1942: sobre la Cocina ══════════════════════════════════════════════════════════════════════════════════
    // El pueblo del plateau del Vivarais-Lignon (~1000 m) que, con el pastor André Trocmé y Magda, escondió y alimentó a miles de refugiados: el templo
    // protestante y el presbiterio, la calle mayor, la estación del tren del Vivarais por la que llegaban, la Escuela Nueva Cevenol, las granjas. Al
    // sur, el Lignon y sus gargantas de granito; al noroeste, el monte Lizieux con brezo, arándano y piorno; alrededor, los pinares con su musgo, sus
    // matamoscas y sus boletos, y los prados de siega. Otoño: el helecho águila rojizo.
    static Map MapLeChambon() => new Map
    {
        display = "Le Chambon-sur-Lignon", place = "Le Chambon-sur-Lignon y el plateau del Vivarais-Lignon (Alto Loira), otoño de 1942", status = Status.Hecho,
        source = "Hallie, Lest Innocent Blood Be Shed (1979); Moorehead, Village of Secrets (2014); Yad Vashem, Justos entre las Naciones: Le Chambon-sur-Lignon " +
                 "(1990); Carte de l'État-major; ONF (los pinares del plateau); el plano del pueblo",
        floor = Natural(
            new Piece("mesa", Como, "las encimeras: el plateau del Vivarais-Lignon", "mesa"),
            new Piece("lavabo", Como, "los fregaderos: el Lignon y sus gargantas", "lavabo")),
        flora = new[]
        {
            // Los pinares, con musgo de Schreber, matamoscas y boletos.
            new Plant("Pino silvestre", -600f, 400f, 10, 150f), new Plant("Pino silvestre", 600f, 400f, 8, 150f), new Plant("Musgo de Schreber", -600f, 400f, 8, 120f),
            new Plant("Matamoscas", -650f, 350f, 4, 100f), new Plant("Boleto", 550f, 450f, 4, 100f),
            // El monte Lizieux: arándano, brezo y piorno.
            new Plant("Arándano", -900f, 600f, 8, 150f), new Plant("Brezo", -750f, 550f, 8, 150f), new Plant("Piorno", -950f, 700f, 6, 150f),
            // El helecho águila rojizo en las lindes y los prados de siega de las granjas.
            new Plant("Helecho águila", -400f, 300f, 8, 100f), new Plant("Prado de siega", -400f, -250f, 8, 80f), new Plant("Prado de siega", 500f, 100f, 8, 120f),
            // El granito de las gargantas del Lignon.
            new Plant("Granito", 400f, -420f, 4, 60f),
        },
        fauna = new[]
        {
            new Animal("Ant", "Hormiga roja de bosque (Formica rufa), con su hormiguero de agujas en el pinar", -600f, 400f, 4, 0.7f, 150f),
            new Animal("DungBeetle", "Geotrupo de bosque (Anoplotrupes stercorosus), en el pinar del este", 550f, 450f, 3, 1.6f, 120f),
            new Animal("Grasshopper", "Saltamontes de prado, en el prado de siega", 500f, 100f, 3, 1.8f, 120f),
            new Animal("Spider", "Araña de jardín (Araneus diadematus), en el helecho", -400f, 300f, 2, 1.2f, 100f),
            new Animal("SnowBee", "Abejorro, en el último brezo en flor", -750f, 550f, 2, 1.8f, 150f),
            new Animal("Ladybug", "Mariquita de 7 puntos, en el prado de la granja", -400f, -250f, 2, 0.7f, 80f),
            new Animal("Earthworm", "Lombriz de tierra, en el prado", 500f, 100f, 2, 15f, 120f),
        },
        buildings = new[]
        {
            new Building("Templo protestante", -260f, 70f), new Building("Presbiterio de los Trocmé", -200f, 0f), new Building("Estación del tren del Vivarais", 300f, -210f),
            new Building("Escuela Nueva Cevenol", 420f, 250f), new Building("Granjas del plateau", -400f, -250f), new Building("Monte Lizieux", -950f, 650f),
        },
        todo = new[] { "en vivo: que el prado de las granjas quede al norte del Lignon y no en el río" },
    };

    // ══ MESOPOTAMIA: URUK, primavera de ~3500 a. C. (la ciudad-insecto del amanecer de Mesopotamia) ═════════════════════════════════════════════
    // La primera ciudad: el recinto del Eanna y, al oeste, la terraza de Anu con el Templo Blanco; los canales que cruzan la ciudad (el del Eanna y el
    // del sur) y el viejo curso del Éufrates al oeste con sus cañaverales y tarays; fuera, al este, los campos de cebada en sazón y, más allá, el borde
    // seco con espina de camello. Palmerales en las orillas, regaliz en los barbechos; asnos de carga y los rebaños de ovejas del templo.
    static Map MapMesopotamia() => new Map
    {
        display = "Mesopotamia", place = "Uruk (Warka, Irak): el Eanna, la terraza de Anu, los canales y el Éufrates, primavera de ~3500 a. C.", status = Status.Hecho,
        source = "Nissen, The Early History of the Ancient Near East (1988); Crüsemann et al. (eds.), Uruk: First City of the Ancient World (2019: el Eanna, el " +
                 "Templo Blanco y los canales); Postgate, Early Mesopotamia (1992: cebada, palmeras y rebaños del templo); Pournelle, Marshland of Cities (2003); el plano del pueblo",
        floor = Natural(
            new Piece("escalera", Como, "las escaleras: la terraza escalonada de Anu, el zigurat temprano", "escalera"),
            new Piece("lavabo", Como, "los lavabos: el Éufrates y los canales de riego", "lavabo")),
        flora = new[]
        {
            // Los campos de cebada en sazón del este.
            new Plant("Cebada en sazón", 620f, 420f, 12, 150f), new Plant("Cebada en sazón", 620f, -460f, 10, 150f),
            // Los palmerales del canal del Eanna y de la orilla del Éufrates.
            new Plant("Palmera datilera", 300f, 180f, 8, 80f), new Plant("Palmera datilera", -550f, -400f, 10, 100f),
            // Los cañaverales y tarays del Éufrates, y su limo.
            new Plant("Caña", -600f, 500f, 10, 80f), new Plant("Taray", -600f, -700f, 6, 100f), new Plant("Limo del Éufrates", -600f, 200f, 6, 60f),
            // El regaliz de los barbechos, la espina de camello del borde seco y el adobe de la terraza de Anu.
            new Plant("Regaliz silvestre", 500f, -150f, 6, 100f), new Plant("Espina de camello", 900f, 0f, 8, 150f), new Plant("Adobe", -260f, 80f, 4, 40f),
        },
        fauna = new[]
        {
            new Animal("Ant", "Hormiga segadora (Messor), recogiendo el grano de la cebada", 620f, 420f, 4, 0.8f, 150f),
            new Animal("Grasshopper", "Langosta del desierto, en los campos del sur", 620f, -460f, 3, 5.5f, 150f),
            new Animal("DungBeetle", "Escarabajo pelotero, tras los asnos de la calle del Eanna", 100f, -200f, 2, 2.5f, 50f),
            new Animal("Scorpion", "Escorpión de cola gruesa, de noche en el borde seco", 900f, 0f, 2, 9f, 150f),
            new Animal("Mantis", "Mantis, en los tarays del Éufrates", -600f, -700f, 2, 6f, 100f),
            new Animal("Aphid", "Asno de carga (en el microcosmos, un pulgón de carga), en la calle del Eanna", 300f, -100f, 3, 1.6f, 80f),
            new Animal("Aphid", "Oveja del rebaño del templo (en el microcosmos, un pulgón), en el rastrojo", 700f, -300f, 3, 1.2f, 120f),
        },
        buildings = new[]
        {
            new Building("Templo Blanco sobre la terraza de Anu", -260f, 80f), new Building("Recinto del Eanna", 100f, 0f), new Building("Canal del Eanna", 0f, 75f),
            new Building("Embarcadero del Éufrates", -680f, 0f), new Building("Talleres de los alfareros", 150f, -200f), new Building("Campos de cebada", 620f, 420f),
        },
        todo = new[] { "en vivo: que los cañaverales queden en la orilla del Éufrates; ojo: el MobWorld de Mesopotamia aún no siembra con AreaMicrocosmos.Nature (ver doc)" },
    };
}
