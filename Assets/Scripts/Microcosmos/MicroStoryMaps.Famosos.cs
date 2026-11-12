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
}
