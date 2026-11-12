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
}
