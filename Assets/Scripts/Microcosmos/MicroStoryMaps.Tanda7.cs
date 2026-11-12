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
}
