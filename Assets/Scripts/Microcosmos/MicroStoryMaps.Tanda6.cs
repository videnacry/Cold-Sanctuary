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

    // ══ SILS MARIA, agosto de 1881 (Nietzsche y el eterno retorno): sobre el Aula ══════════════════════════════════════════════════════════════
    // El pueblo de la Alta Engadina entre el lago de Sils (oeste) y el de Silvaplana (noreste), a la boca del valle de Fex (sur). Nietzsche se alojaba en
    // la casa Durisch (hoy Nietzsche-Haus) y caminaba cada día por la orilla de Silvaplana: junto al «poderoso bloque de forma piramidal» cerca de Surlej
    // le vino el pensamiento del eterno retorno (Ecce Homo). La península de Chastè con sus pinos cembros, los pastos de cervuno del Fex con gencianas,
    // el rododendro en las laderas del Corvatsch y el Lagrev, el edelweiss arriba; líquenes de mapa sobre el gneis.
    static Map MapSilsMaria() => new Map
    {
        display = "Sils Maria", place = "Sils Maria, Alta Engadina: el pueblo, Chastè, el valle de Fex y la roca de Surlej, agosto de 1881", status = Status.Hecho,
        source = "Nietzsche, Ecce Homo («Así habló Zaratustra» §1: la roca piramidal junto a Surlej, agosto de 1881); Siegfriedkarte 1:50 000, hoja 518 " +
                 "(1878); Stiftung Nietzsche-Haus in Sils Maria; Info Flora (la flora alpina de la Engadina); el plano del pueblo",
        floor = Natural(
            new Piece("mesa", Como, "pupitres y mesas: los nevados (el Corvatsch y el Lagrev)", "mesa"),
            new Piece("pared", Como, "la pizarra y los paneles: las paredes de roca del valle de la Engadina", "pared")),
        flora = new[]
        {
            // Los pastos de cervuno del valle de Fex y junto al pueblo, con gencianas.
            new Plant("Cervuno", 80f, -600f, 10, 120f), new Plant("Cervuno", 250f, -300f, 8, 80f), new Plant("Genciana", 100f, -750f, 6, 80f),
            // El rododendro de las laderas del Corvatsch y del Lagrev.
            new Plant("Rododendro", 600f, -600f, 10, 120f), new Plant("Rododendro", -750f, -450f, 8, 120f),
            // Los pinos cembros de la península de Chastè y de la ladera.
            new Plant("Pino cembro", -420f, 420f, 8, 80f), new Plant("Pino cembro", -600f, -300f, 6, 100f),
            // Arriba, el edelweiss y el gneis del Corvatsch.
            new Plant("Edelweiss", 760f, -850f, 4, 80f), new Plant("Gneis", 760f, -850f, 4, 80f),
            // La roca de Surlej (la del eterno retorno) y las rocas del Lagrev, con líquenes de mapa.
            new Plant("Gneis", 600f, 500f, 4, 30f), new Plant("Liquen de mapa", 600f, 500f, 4, 40f), new Plant("Liquen de mapa", -950f, -560f, 4, 80f),
        },
        fauna = new[]
        {
            new Animal("Grasshopper", "Saltamontes alpino sin alas (Podisma pedestris), en los pastos del Fex", 80f, -600f, 4, 2f, 120f),
            new Animal("SnowBee", "Abejorro alpino (Bombus alpinus), en el rododendro del Corvatsch", 600f, -600f, 3, 1.6f, 120f),
            new Animal("Ant", "Hormiga de los montes (Formica lugubris), con su hormiguero de agujas bajo los cembros de Chastè", -420f, 420f, 3, 0.7f, 80f),
            new Animal("Spider", "Araña lobo (Pardosa), al sol sobre la roca de Surlej", 600f, 500f, 2, 0.7f, 40f),
            new Animal("Ladybug", "Mariquita (Coccinella), en los pastos junto al pueblo", 250f, -300f, 2, 0.6f, 80f),
            new Animal("Scorpionfly", "Mosca escorpión de la nieve (Boreus), quieta en el musgo del Lagrev hasta el invierno", -750f, -450f, 2, 0.5f, 100f),
        },
        buildings = new[]
        {
            new Building("Casa Durisch (la habitación de Nietzsche)", -125f, 125f), new Building("Roca de Surlej", 600f, 500f),
            new Building("Península de Chastè", -420f, 420f), new Building("Entrada del valle de Fex", 90f, -820f), new Building("Pensión Edelweiss", 160f, 65f),
        },
        todo = new[] { "en vivo: que la roca de Surlej quede en la orilla de Silvaplana y no dentro del lago" },
    };

    // ══ CHERNÓBIL, principios de mayo de 1986 (Ananenko, Bespalov y Baranov): sobre la Central Nuclear ══════════════════════════════════════════
    // La central con el bloque 4 roto, el canal de toma y el estanque de refrigeración al sureste; el río Prípiat al norte; la ciudad de Prípiat al
    // noroeste (a la mitad de su distancia real) con la avenida de Lenin, la plaza del Palacio de Cultura Energetik, la piscina Lazurny y el parque de
    // atracciones con su noria, evacuada el 27 de abril. Entre la central y la ciudad, el pinar que en semanas será el «Bosque Rojo» (en mayo, aún
    // verde). El pinar de Polesia sobre arena fluvioglacial: pinos, musgo, hormigueros de hormiga roja; dientes de león en los céspedes de la ciudad.
    static Map MapChernobil() => new Map
    {
        display = "Chernóbil", place = "La central de Chernóbil, el estanque de refrigeración y Prípiat, principios de mayo de 1986", status = Status.Hecho,
        source = "Leatherbarrow, Chernobyl 01:23:40 (2016: la central, Prípiat y el vaciado de la piscina de burbujeo); Plokhy, Chernobyl: History of a " +
                 "Tragedy (2018); IAEA, Chernobyl's Legacy (2006: el Bosque Rojo y el pinar); el mapa de Prípiat de 1986; el plano del pueblo",
        floor = Natural(
            new Piece("lavabo", Como, "los lavabos: el estanque de refrigeración de la central", "lavabo"),
            new Piece("banera", Como, "la bañera: la piscina de burbujeo bajo el reactor, la que Ananenko, Bespalov y Baranov vaciaron", "banera")),
        flora = new[]
        {
            // El pinar del oeste (el futuro Bosque Rojo) y el del sur, con su musgo.
            new Plant("Pino silvestre", -600f, -150f, 12, 150f), new Plant("Musgo de pinar", -600f, -150f, 8, 120f),
            new Plant("Pino silvestre", -200f, -600f, 10, 150f), new Plant("Musgo de pinar", -200f, -600f, 6, 120f),
            // Los abedules del parque de Prípiat, con hoja nueva, y los dientes de león de sus céspedes.
            new Plant("Abedul", -900f, 950f, 8, 100f), new Plant("Diente de león", -700f, 700f, 8, 100f),
            // La hierba de arenal en el aluvión del Prípiat.
            new Plant("Hierba de arenal", 200f, 500f, 8, 80f),
            // La arena de las obras de los bloques 5 y 6.
            new Plant("Arena fluvioglacial", 300f, 150f, 6, 80f),
        },
        fauna = new[]
        {
            new Animal("Ant", "Hormiga roja de bosque (Formica rufa), con su hormiguero en el pinar del oeste", -600f, -150f, 4, 0.8f, 150f),
            new Animal("Ant", "Hormiga roja de bosque, en el pinar del sur", -200f, -600f, 3, 0.8f, 150f),
            new Animal("Antlion", "Hormiga león (Myrmeleon formicarius), en la arena del pinar", -200f, -600f, 2, 1.2f, 120f),
            new Animal("Ladybug", "Mariquita de 7 puntos, en los dientes de león de Prípiat", -700f, 700f, 3, 0.7f, 100f),
            new Animal("SnowBee", "Abejorro (Bombus terrestris), en los abedules del parque", -900f, 950f, 3, 1.6f, 100f),
            new Animal("Spider", "Araña lobo (Pardosa), en la arena de las obras", 300f, 150f, 2, 0.8f, 80f),
        },
        buildings = new[]
        {
            new Building("Bloque 4 de la central", 40f, 30f), new Building("Estanque de refrigeración", 520f, -470f), new Building("El pinar (el futuro Bosque Rojo)", -600f, -150f),
            new Building("Palacio de Cultura Energetik", -760f, 700f), new Building("Piscina Lazurny", -850f, 650f), new Building("Noria del parque de atracciones", -800f, 950f),
            new Building("Estación de Yániv", -200f, 600f),
        },
        todo = new[] { "en vivo: que la hierba de arenal quede en la orilla del Prípiat y no en el río; los pinos, todavía verdes (en mayo aún no eran rojos)" },
    };
}
