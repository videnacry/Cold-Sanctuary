using UnityEngine;

/// <summary>LOS MAPAS A MANO DE LA TANDA 3 (V): San Pablo, Madrid, Gettysburg y la Feria.</summary>
public static partial class MicroStoryMaps
{
    // ══ SAN PABLO, junio de 1952 (Guevara y Granado): sobre el Hospital ═════════════════════════════════════════════════════════
    // El Amazonas (el agua sale de los lavabos y bañeras del hospital) entre dos orillas de selva; las camas son lomas de selva, las máquinas
    // del hospital no existen en la selva: rocas; la colonia en la orilla sur y los médicos en la norte.
    static Map MapSanPablo() => new Map
    {
        display = "San Pablo", place = "Colonia de San Pablo, Loreto, Perú, junio de 1952", status = Status.Hecho,
        source = "Guevara 1993; Granado 1978; International Leprosy Association, History of Leprosy; OpenStreetMap, San Pablo de Loreto; Brack Egg 1999",
        floor = Natural(
            new Piece("alfombra", Apilar, "las alfombras, apiladas: el alto de la colonia, que no se inunda"),
            new Piece("silla", Como, "sin montañas en la Amazonía: las sillas son lupunas enormes (macetas: bosque)", "planta"),
            new Piece("cama", Como, "las camas: lomas de selva", "planta")),
        flora = new[]
        {
            new Plant("Lupuna", 860f, -880f, 8, 200f), new Plant("Lupuna", -980f, -800f, 8, 200f), new Plant("Lupuna", 800f, 960f, 6, 200f),
            new Plant("Cetico", 700f, -560f, 12, 160f), new Plant("Cetico", -700f, -420f, 10, 140f), new Plant("Cetico", -500f, 900f, 8, 120f),
            new Plant("Capirona", 980f, -600f, 8, 140f), new Plant("Capirona", 1000f, 900f, 7, 130f),
            new Plant("Caña brava", -520f, 300f, 14, 90f), new Plant("Caña brava", 460f, 320f, 14, 90f), new Plant("Caña brava", -220f, 680f, 12, 80f),
            new Plant("Heliconia", 240f, -440f, 8, 70f), new Plant("Heliconia", -480f, -140f, 7, 60f), new Plant("Helecho", 900f, -900f, 12, 150f),
            new Plant("Aguaje", -340f, 300f, 5, 40f), new Plant("Aguaje", 480f, 260f, 4, 40f), new Plant("Aguaje", 1100f, -320f, 8, 90f),
            new Plant("Plátano", 420f, -200f, 12, 90f), new Plant("Yuca", 380f, -140f, 12, 90f), new Plant("Hierba de la cancha", 300f, -340f, 12, 50f),
        },
        fauna = new[]
        {
            new Animal("Ant", "Hormiga cortadora (Atta cephalotes)", 700f, -560f, 6, 1.6f, 120f),
            new Animal("Termite", "Comején (Nasutitermes)", -700f, -420f, 4, 1.2f, 80f),
            new Animal("Cicada", "Cigarra de la selva (Quesada gigas)", 860f, -880f, 3, 1.8f, 120f),
            new Animal("Mantis", "Mantis de la selva (Stagmatoptera)", 460f, 320f, 2, 2f, 60f),
            new Animal("Spider", "Araña de la selva (Nephila)", -520f, 300f, 2, 1.8f, 50f),
            new Animal("Ladybug", "Gallina de la colonia (en el microcosmos, una mariquita parda)", 90f, -150f, 4, 0.5f, 60f),
            new Animal("Woodlouse", "Perro de la colonia (en el microcosmos, una cochinilla hormiguera)", 40f, -20f, 3, 0.7f, 90f),
        },
        buildings = new[] { new Building("Hospital de la colonia", -180f, 110f), new Building("Casa del personal", -70f, 870f), new Building("Embarcadero", 120f, 240f), new Building("Chacra de la colonia", 420f, -200f), new Building("Maloca yagua", 1160f, -60f) },
        todo = new[] { "en vivo: que el Amazonas se lea como río ancho y que nada caiga en él" },
    };

    // ══ MADRID, 1 de junio de 2018: sobre la Sala de asambleas ═══════════════════════════════════════════
    // Ciudad: los plátanos del Paseo del Prado, los tilos de las calles, el Retiro al este. Los pupitres, las manzanas altas del centro.
    static Map MapMadrid() => new Map
    {
        display = "Madrid 2018", place = "El Barrio de las Cortes, Madrid, 1 de junio de 2018", status = Status.Hecho,
        source = "Ayuntamiento de Madrid (arbolado urbano); OpenStreetMap, Madrid",
        floor = new[]
        {
            new Piece("alfombra", Quitar, "las alfombras no hacen falta en el centro de Madrid: se retiran (siguen contadas)"),
            new Piece("silla", Como, "las sillas: las torres de Plaza de España, a lo lejos", "botella"),
            new Piece("mesa", Como, "las mesas: las mesetas de la sierra de Guadarrama, al fondo", "mesa"),
        },
        flora = new[]
        {
            new Plant("Plátano de sombra", 490f, 200f, 8, 180f), new Plant("Plátano de sombra", 550f, -300f, 12, 260f), new Plant("Plátano de sombra", -300f, 335f, 5, 120f),
            new Plant("Tilo", -80f, -30f, 3, 40f), new Plant("Tilo", 160f, -130f, 4, 70f), new Plant("Tilo", -340f, -70f, 4, 70f),
            new Plant("Castaño de Indias", 1100f, -240f, 14, 240f), new Plant("Almendro", 860f, -500f, 6, 60f), new Plant("Almendro", 1100f, -420f, 5, 60f),
            new Plant("Césped del Retiro", 1100f, -240f, 26, 250f), new Plant("Césped del Retiro", 520f, -300f, 14, 90f), new Plant("Rosa de la Rosaleda", 1200f, -500f, 10, 60f), new Plant("Bordillo", -760f, 30f, 6, 40f),
        },
        fauna = new[]
        {
            new Animal("SnowBee", "Abeja del Retiro (Apis mellifera)", 1200f, -500f, 4, 1.2f, 70f),
            new Animal("Ant", "Hormiga negra (Lasius niger)", 1100f, -240f, 4, 0.8f, 100f),
            new Animal("Woodlouse", "Perro de paseo (en el microcosmos, una cochinilla hormiguera)", 520f, -100f, 4, 0.7f, 160f),
            new Animal("Woodlouse", "Gato de la calle (en el microcosmos, una cochinilla hormiguera)", -340f, -240f, 2, 0.5f, 70f),
            new Animal("Ladybug", "Mariquita del parque", 860f, -500f, 3, 0.8f, 60f),
        },
        buildings = new[] { new Building("Palacio de las Cortes", 40f, 70f), new Building("Palacio de la Zarzuela (acercado)", -900f, 780f), new Building("Museo del Prado", 660f, -120f), new Building("Biblioteca Nacional", 300f, 700f), new Building("Plaza (Puerta del Sol)", -760f, 30f) },
        todo = new[] { "en vivo: los leones van delante del claro y la Zarzuela debe leerse apartada, en su monte" },
    };

    // ══ GETTYSBURG, 19 de noviembre de 1863 (Lincoln): sobre la Sala de asambleas ═══════════════════════
    // Otoño tardío: robles desnudos, rastrojos, manzanos ya cosechados, rocas de diabasa en Culp's Hill y en los Round Tops. El pupitre del profesor,
    // Cemetery Hill; las sillas, lomas; los libros, las rocas de Devil's Den.
    static Map MapGettysburg() => new Map
    {
        display = "Gettysburg", place = "Gettysburg, Pensilvania, 19 de noviembre de 1863", status = Status.Hecho,
        source = "National Park Service, Gettysburg National Military Park; plano de Warren (1868); Wills 1992",
        floor = Natural(
            new Piece("alfombra", Apilar, "las alfombras, apiladas: Cemetery Hill, bajo el claro"),
            new Piece("silla", Como, "las sillas: las lomas de Seminary Ridge y Cemetery Ridge, sin nieve aún", "mesa"),
            new Piece("libro", Como, "los libros: las rocas apiladas de Devil's Den", "libro")),
        flora = new[]
        {
            new Plant("Roble blanco", 620f, -60f, 14, 150f), new Plant("Roble blanco", -660f, 760f, 12, 140f), new Plant("Roble blanco", -440f, -1200f, 8, 120f),
            new Plant("Nogal negro", 680f, 300f, 7, 90f), new Plant("Nogal negro", 700f, -600f, 6, 80f),
            new Plant("Rastrojo de trigo", -700f, -640f, 30, 180f), new Plant("Rastrojo de trigo", -1100f, 900f, 18, 150f), new Plant("Hierba de noviembre", 60f, 60f, 20, 110f),
            new Plant("Manzano del condado de Adams", 1000f, -760f, 14, 150f), new Plant("Manzano del condado de Adams", -300f, -420f, 5, 50f),
            new Plant("Arce rojo", 20f, 620f, 5, 90f), new Plant("Arce rojo", 200f, 820f, 4, 70f), new Plant("Césped del patio", 60f, 720f, 6, 30f),
            new Plant("Diabasa", 620f, -100f, 8, 100f), new Plant("Diabasa", -440f, -1100f, 8, 110f),
        },
        fauna = new[]
        {
            new Animal("Cricket", "Grillo de otoño (Gryllus pennsylvanicus)", -700f, -640f, 4, 1.2f, 120f),
            new Animal("Spider", "Araña del campo", 620f, -60f, 2, 1.2f, 80f),
            new Animal("Ant", "Hormiga carpintera (Camponotus pennsylvanicus)", -660f, 760f, 3, 1f, 70f),
            new Animal("Aphid", "Caballo del carro (en el microcosmos, un pulgón de tiro)", 40f, 300f, 4, 2f, 120f),
            new Animal("Woodlouse", "Perro del pueblo (en el microcosmos, una cochinilla hormiguera)", 0f, 720f, 3, 0.7f, 100f),
        },
        buildings = new[] { new Building("Cementerio Nacional de los Soldados", 60f, -20f), new Building("Puerta del Evergreen", -40f, 120f), new Building("Casa de David Wills", 60f, 760f), new Building("Seminario Luterano", -560f, 840f), new Building("Estación de Gettysburg", 60f, 960f), new Building("Granja de Basil Biggs", -300f, -520f) },
        todo = new[] { "en vivo: el pueblo queda al norte del claro: que se vea desde el estrado, como en las fotos de la ceremonia" },
    };

    // ══ LA FERIA DE LAS HAZAÑAS (inventada): sobre la Sala de asambleas ═════════════════════════════════
    // Un jardín de países. El mapa es de invención, a mano igualmente: cada árbol junto a su pabellón.
    static Map MapFeria() => new Map
    {
        display = "Feria de las Hazañas", place = "Una feria inventada, hoy", status = Status.Hecho,
        source = "Lugar inventado (como la aldea de Goku): cada planta, del país de su pabellón",
        floor = new[]
        {
            new Piece("alfombra", Apilar, "las alfombras, apiladas: el mirador de la Feria"),
            new Piece("silla", Como, "las sillas: los palcos del anfiteatro", "mesa"),
        },
        flora = new[]
        {
            new Plant("Árbol de la lluvia", -40f, 560f, 3, 60f), new Plant("Árbol de la lluvia", 80f, 460f), new Plant("Orquídea de Singapur", 60f, 560f, 6, 30f),
            new Plant("Roble blanco", -460f, 380f, 3, 40f), new Plant("Roble blanco", -680f, 110f, 3, 40f),
            new Plant("Corteza amarilla", -500f, -370f, 4, 40f), new Plant("Acacia del Kalahari", 510f, -360f, 4, 40f),
            new Plant("Protea", 680f, 60f, 7, 40f), new Plant("Protea", 620f, 120f, 5, 30f),
            new Plant("Césped de la Feria", 0f, 0f, 16, 90f), new Plant("Césped de la Feria", -900f, -400f, 22, 200f), new Plant("Adoquín", 40f, -130f, 5, 40f),
        },
        fauna = new[]
        {
            new Animal("Ant", "Hormiga tejedora (Oecophylla smaragdina)", -40f, 560f, 5, 1f, 50f),
            new Animal("SnowBee", "Abeja del jardín", -900f, -400f, 4, 1.2f, 90f),
            new Animal("Ladybug", "Mariquita del jardín", 620f, 90f, 3, 0.8f, 40f),
            new Animal("Woodlouse", "Perro de la Feria (en el microcosmos, una cochinilla hormiguera)", 40f, -180f, 3, 0.7f, 90f),
        },
        buildings = new[] { new Building("Pabellón de Singapur", 0f, 500f), new Building("Pabellón de Mount Vernon", -400f, 330f), new Building("Pabellón de Sudáfrica", 620f, 60f), new Building("Pabellón de Botsuana", 460f, -320f), new Building("Plaza de las Hazañas", 40f, -130f) },
        todo = new[] { "en vivo: cada pabellón a la vista desde la plaza" },
    };
}
