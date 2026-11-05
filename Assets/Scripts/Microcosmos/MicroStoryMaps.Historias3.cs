using UnityEngine;

/// <summary>LOS MAPAS A MANO DE LA TANDA 3 (III): Onondaga y Alejandría.</summary>
public static partial class MicroStoryMaps
{
    // ══ ONONDAGA, hacia 1450 (la Gran Ley de la Paz): sobre la Sala de asambleas ════════════════════════
    // (Los pupitres de abajo son del Aula, donde iba antes: en la Sala de asambleas esas órdenes no encuentran pieza y no hacen nada.) El lago al noroeste, los campos de las tres hermanas al sureste, arces y olmos en las lomas, eneas en la orilla, el gran pino blanco
    // sobre el claro. Los pupitres son la meseta caliza; las sillas, no nevados: colinas (en Onondaga no hay cumbres blancas a final de verano).
    static Map MapOnondaga() => new Map
    {
        display = "Onondaga", place = "El lago Onondaga (hoy Nueva York), finales de verano, hacia 1450", status = Status.Hecho,
        source = "Bradley 1987; Snow 1994; Engelbrecht 2003; el lago y el arroyo, de OpenStreetMap; la aldea se recrea por las excavadas de la región",
        floor = Natural(
            new Piece("alfombra", Apilar, "las alfombras, apiladas: las colinas del este"),
            new Piece("silla", Como, "las sillas: colinas calizas de Onondaga, sin nieve en verano", "mesa"),
            new Piece("pared", Como, "las pizarras: acantilados de caliza de Onondaga", "pared"),
            new Piece("libro", Como, "los libros: los estratos de caliza que dan nombre a la formación Onondaga", "libro")),
        flora = new[]
        {
            new Plant("Pino blanco", -40f, 300f, 6, 70f), new Plant("Pino blanco", -700f, 80f, 10, 130f), new Plant("Pino blanco", 700f, 500f, 8, 110f),
            new Plant("Arce de azúcar", -620f, -300f, 16, 160f), new Plant("Arce de azúcar", 1100f, -500f, 12, 180f),
            new Plant("Olmo americano", 620f, 120f, 7, 70f), new Plant("Olmo americano", -400f, -520f, 7, 80f),
            new Plant("Enea", -560f, 330f, 16, 70f), new Plant("Enea", -640f, 480f, 12, 70f),
            new Plant("Vara de oro", 450f, -320f, 10, 70f), new Plant("Vara de oro", -220f, -420f, 8, 60f), new Plant("Helecho", -500f, -420f, 12, 90f),
            new Plant("Maíz", 340f, -680f, 34, 190f), new Plant("Maíz", 560f, 760f, 18, 130f), new Plant("Calabaza", 340f, -560f, 16, 150f),
            new Plant("Nogal", 190f, 250f), new Plant("Nogal", -300f, -60f), new Plant("Nogal", 260f, 60f), new Plant("Nogal", -140f, 340f),
            new Plant("Caliza de Onondaga", 1200f, -620f, 8, 100f),
        },
        fauna = new[]
        {
            new Animal("Cicada", "Cigarra de agosto (Neotibicen)", -620f, -300f, 3, 1.6f, 80f),
            new Animal("Firefly", "Luciérnaga (Photinus pyralis)", 340f, -520f, 4, 1f, 90f),
            new Animal("SnowBee", "Abejorro oriental (Bombus impatiens)", 450f, -320f, 4, 1.6f, 70f),
            new Animal("Ant", "Hormiga carpintera (Camponotus pennsylvanicus)", -700f, 80f, 5, 1.2f, 70f),
            new Animal("Grasshopper", "Saltamontes (Melanoplus)", 560f, 760f, 3, 1.8f, 90f),
            new Animal("Spider", "Araña de la orilla", -560f, 330f, 2, 1.4f, 40f),
            new Animal("Woodlouse", "Perro de la aldea (en el microcosmos, una cochinilla hormiguera)", 40f, 60f, 3, 0.7f, 60f),
        },
        buildings = new[] { new Building("Casa larga del consejo", -60f, 210f), new Building("Claro del consejo", 0f, 120f), new Building("Casa larga de Jigonsaseh", -210f, -60f), new Building("Campos de las tres hermanas", 340f, -520f), new Building("Campo de tewaarathon", 500f, -130f) },
        todo = new[] { "en vivo: que el lago quede al noroeste del claro y se vea desde el Árbol de la Paz" },
    };

    // ══ ALEJANDRÍA, verano del 42 a. C. (Cleopatra): sobre la Cocina ═══════════════════════════════════════════════════════════
    // Ciudad de piedra clara entre el mar y el lago: palmeras en los patios, jardines en el Paneo, vides y papiros en el Mareotis. Los fregaderos
    // son el Gran Puerto y el lago; las sillas, los pedestales del Faro y del Paneo; los fogones no son volcanes (no hay en el Delta).
    static Map MapAlejandriaCleopatra() => new Map
    {
        display = "Alejandría de Cleopatra", place = "Alejandría, verano del 42 a. C.", status = Status.Hecho,
        source = "McKenzie 2007, The Architecture of Alexandria and Egypt; Fraser 1972; Mahmoud-Bey 1872 (plano); Täckholm y Drar, Flora of Egypt",
        floor = Natural(
            new Piece("alfombra", Apilar, "las alfombras, apiladas: la colina del Paneo"),
            new Piece("hornilla", Como, "en el Delta no hay volcanes: los fogones son las islas y los promontorios de caliza (Faro, Lochias)", "mesa"),
            new Piece("silla", Como, "las sillas: rocas del cabo Lochias, sin nieve", "roca"),
            new Piece("lavabo", Como, "los fregaderos: el Gran Puerto y el lago Mareotis", "lavabo")),
        flora = new[]
        {
            new Plant("Palmera datilera", -300f, 340f, 6, 50f), new Plant("Palmera datilera", 420f, 330f, 6, 50f), new Plant("Palmera datilera", -700f, -1150f, 24, 240f), new Plant("Palmera datilera", -820f, -640f, 7, 60f),
            new Plant("Sicómoro", 140f, -130f), new Plant("Sicómoro", 320f, -240f), new Plant("Sicómoro", -480f, -300f), new Plant("Sicómoro", 620f, 100f),
            new Plant("Olivo", 720f, -460f, 12, 100f), new Plant("Olivo", 340f, -440f, 6, 40f),
            new Plant("Vid de Mareotis", -300f, -1000f, 16, 150f), new Plant("Vid de Mareotis", 620f, -1000f, 12, 120f),
            new Plant("Papiro", 200f, -1100f, 18, 160f), new Plant("Papiro", 800f, -1100f, 12, 100f),
            new Plant("Tamarisco", -900f, 480f, 8, 70f), new Plant("Tamarisco", 900f, 480f, 7, 70f), new Plant("Salicornia", -1100f, 620f, 14, 90f),
            new Plant("Arena de la costa", -500f, 660f, 12, 120f), new Plant("Arena de la costa", 700f, 660f, 10, 110f), new Plant("Caliza oolítica", 1000f, 560f, 6, 60f),
            new Plant("Loto azul", 720f, -400f, 5, 30f),
        },
        fauna = new[]
        {
            new Animal("DungBeetle", "Escarabajo sagrado (Scarabaeus sacer)", 720f, -460f, 3, 1.6f, 70f),
            new Animal("Mantis", "Mantis (Sphodromantis viridis)", -300f, -1000f, 2, 2f, 60f),
            new Animal("Cicada", "Cigarra de los olivos", 340f, -440f, 3, 1.4f, 40f),
            new Animal("SnowBee", "Abeja egipcia (Apis mellifera lamarckii)", 700f, -500f, 4, 1.2f, 60f),
            new Animal("Woodlouse", "Gato del barrio (en el microcosmos, una cochinilla hormiguera)", 60f, -170f, 3, 0.5f, 90f),
            new Animal("Aphid", "Asno de carga (en el microcosmos, un pulgón de carga)", -300f, 420f, 3, 1.6f, 90f),
        },
        buildings = new[] { new Building("Palacio de Lochias", 60f, 100f), new Building("Mouseion", 330f, 220f), new Building("Graneros reales", -260f, 400f), new Building("Serapeo", -800f, -520f), new Building("Ágora", 240f, -170f), new Building("Gimnasio", 180f, -180f) },
        todo = new[] { "en vivo: que el Faro (isla al norte, fuera del barrio) se lea desde el muelle; si no cabe, una torre en el borde" },
    };
}
