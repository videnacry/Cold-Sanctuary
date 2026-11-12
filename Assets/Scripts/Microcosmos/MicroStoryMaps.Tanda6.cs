using UnityEngine;

/// <summary>LOS MAPAS A MANO DE LA TANDA 6 (2026-11-12; petición 75, regla M9; docs/microcosmos-mapas-a-mano.md §Tanda 4).</summary>
public static partial class MicroStoryMaps
{
    // ══ MENFIS, verano de ~450 a. C. (Heródoto en Egipto): sobre el Aula ═══════════════════════════════════════════════════════════════════════
    // La «Muralla Blanca» (Ineb-hedj): el recinto de Ptah en el centro con su lago sagrado y el coloso de Ramsés II; al este, el Nilo con el puerto de
    // Peru-nefer y su canal, bordeados de papiro y caña; al norte y al sur, los campos que la crecida empieza a cubrir (el loto azul vuelve a las
    // acequias); al oeste, el rastrojo del trigo emmer y, en el borde, la meseta desértica de Saqqara. Heródoto (II.14) vio que aquí la tierra «la da
    // el río»: el limo negro. Los asnos de la avenida procesional y sus escarabajos sagrados; en el desierto, la hormiga que corre al mediodía.
    static Map MapMenfis() => new Map
    {
        display = "Menfis", place = "Menfis, el recinto de Ptah, el Nilo y el borde de Saqqara, al empezar la crecida (~450 a. C.)", status = Status.Hecho,
        source = "Heródoto, Historias II.14, II.99–101 y II.153 (Menfis, el templo de Ptah y el patio del buey Apis); Jeffreys, The Survey of Memphis " +
                 "(Egypt Exploration Society, desde 1981); Bonneau, La crue du Nil (1964); el plano del pueblo",
        floor = Natural(
            new Piece("mesa", Como, "pupitres y mesas: los koms (montículos de la ciudad vieja) sobre la llanura de la crecida", "mesa"),
            new Piece("pared", Como, "la pizarra y los paneles: la Muralla Blanca (Ineb-hedj) que dio nombre a Menfis", "pared")),
        flora = new[]
        {
            // La orilla del Nilo y el canal del puerto: papiro y caña.
            new Plant("Papiro", 280f, 300f, 10, 80f), new Plant("Papiro", 280f, -400f, 10, 80f), new Plant("Caña", 300f, -100f, 6, 40f),
            // Los campos de la crecida: limo negro y loto azul en las acequias que se llenan.
            new Plant("Loto azul", 220f, 520f, 8, 120f), new Plant("Limo negro", 220f, 520f, 4, 100f),
            new Plant("Loto azul", 220f, -560f, 8, 120f), new Plant("Limo negro", 220f, -560f, 4, 100f),
            // El lago sagrado de Ptah.
            new Plant("Loto azul", 90f, -140f, 4, 30f),
            // Los palmerales junto al templo y los campos.
            new Plant("Palmera datilera", -300f, -300f, 8, 100f), new Plant("Palmera datilera", -200f, 500f, 6, 80f),
            // El rastrojo del emmer, ya segado en verano.
            new Plant("Trigo emmer", -480f, 420f, 10, 120f),
            // La avenida procesional (los asnos) y la meseta de Saqqara.
            new Plant("Excremento de asno", -300f, -40f, 4, 60f), new Plant("Arena del desierto", -850f, 0f, 8, 120f),
        },
        fauna = new[]
        {
            new Animal("DungBeetle", "Escarabajo sagrado (Scarabaeus sacer), rodando su bola en la avenida procesional", -300f, -40f, 3, 2.8f, 60f),
            new Animal("DungBeetle", "Escarabajo sagrado, en el borde del desierto", -750f, 100f, 2, 2.8f, 100f),
            new Animal("Grasshopper", "Langosta del desierto (Schistocerca gregaria), en el rastrojo del emmer", -480f, 420f, 3, 5.5f, 120f),
            new Animal("Cricket", "Grillo (Gryllus bimaculatus), en los campos de la crecida", 220f, -560f, 3, 2f, 100f),
            new Animal("Ant", "Hormiga (Cataglyphis bicolor), corriendo al sol del mediodía en Saqqara", -850f, 0f, 4, 0.6f, 120f),
            new Animal("Mantis", "Mantis del desierto (Eremiaphila), en la meseta", -800f, -200f, 2, 5f, 100f),
            new Animal("Scorpion", "Escorpión amarillo (Leiurus quinquestriatus), de noche en la meseta", -850f, 250f, 2, 9f, 100f),
        },
        buildings = new[]
        {
            new Building("Templo de Ptah", -100f, -60f), new Building("Coloso de Ramsés II", -20f, 160f), new Building("Patio del buey Apis", -150f, -250f),
            new Building("Puerto de Peru-nefer", 330f, -50f), new Building("Necrópolis de Saqqara y pirámide escalonada", -900f, 0f),
        },
        todo = new[] { "en vivo: que el papiro quede en la orilla oeste del Nilo y no en el agua; el loto, en las acequias de los campos" },
    };
}
