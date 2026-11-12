using UnityEngine;

/// <summary>LOS MAPAS A MANO DE LA TANDA 5 (2026-11-12; petición 75, regla M9; docs/microcosmos-mapas-a-mano.md §Tanda 4).</summary>
public static partial class MicroStoryMaps
{
    // ══ ESCÚTARI, invierno de 1854–55 (Florence Nightingale): sobre la Enfermería ══════════════════════════════════════════════════════════════
    // El cuartel de Selimiye (el Hospital de los Barracones: un cuadrilátero de ~260 × 220 m con torres en las esquinas) en lo alto de la ribera
    // asiática del Bósforo (al oeste); al norte, el camino de Üsküdar; al este, el gran cementerio musulmán de Karacaahmet, bosque de cipreses y
    // lápidas; al sur, hacia Haydarpaşa, el cementerio militar británico donde se enterraba a los muertos del hospital. Invierno: la hiedra y los
    // musgos en los muros, el ciclamen bajo los cipreses; mariquitas y caracoles dormidos, cucarachas y arañas dentro del cuartel.
    static Map MapEscutari() => new Map
    {
        display = "Escútari", place = "Escútari (Üsküdar), el cuartel de Selimiye sobre el Bósforo, invierno de 1854–55", status = Status.Hecho,
        source = "Royal Collection Trust, «The Military Hospital, Scutari» (Simpson, 1855); Plan d'Üsküdar (1850); Commonwealth War Graves Commission, " +
                 "Haidar Pasha Cemetery; Nightingale, Notes on Matters Affecting the Health … of the British Army (1858); el plano del pueblo",
        floor = Natural(
            new Piece("cama", Como, "las camas de la sala: las alas del cuartel de Selimiye, con sus filas de catres", "cama"),
            new Piece("lavabo", Como, "los lavabos: el Bósforo y las fuentes del cuartel (el agua que Nightingale hizo limpiar)", "lavabo")),
        flora = new[]
        {
            // Los muros del cuartel: hiedra y musgo en las alas y las torres.
            new Plant("Hiedra rastrera", -130f, 60f, 8, 40f), new Plant("Hiedra rastrera", 130f, -60f, 8, 40f),
            new Plant("Musgo de pared", -130f, -110f, 5, 20f), new Plant("Musgo de pared", 130f, 110f, 5, 20f),
            // Los caminos: el de Üsküdar y la calle de Selimiye, con sus orillas de cebada ratonera y malvas.
            new Plant("Cebada ratonera", 0f, 500f, 8, 60f), new Plant("Malva", -200f, 300f, 6, 60f),
            // La ribera del Bósforo, con caléndulas silvestres al sol de invierno.
            new Plant("Caléndula silvestre", -400f, -300f, 6, 60f),
            // El cementerio británico (sur, hacia Haydarpaşa) y el de Karacaahmet (este): cipreses, ciclámenes y lápidas.
            new Plant("Plántula de ciprés", 100f, -700f, 6, 60f), new Plant("Ciclamen de invierno", 100f, -700f, 6, 50f), new Plant("Lápida y piedra", 100f, -700f, 6, 40f),
            new Plant("Plántula de ciprés", 650f, -150f, 10, 80f), new Plant("Ciclamen de invierno", 650f, -150f, 6, 60f), new Plant("Lápida y piedra", 650f, -150f, 6, 60f),
            // La ladera de Çamlıca.
            new Plant("Malva", 900f, 600f, 6, 80f),
        },
        fauna = new[]
        {
            new Animal("Cockroach", "Cucaracha oriental (Blatta orientalis), en el ala oeste del cuartel", -130f, 0f, 3, 2.4f, 30f),
            new Animal("Cockroach", "Cucaracha oriental, en el ala sur", 0f, -110f, 3, 2.4f, 30f),
            new Animal("Cockroach", "Cucaracha oriental, en el ala este", 130f, 0f, 3, 2.4f, 30f),
            new Animal("Spider", "Araña de sótano (Pholcus phalangioides), en la torre de Nightingale", -130f, 110f, 2, 0.8f, 20f),
            new Animal("Snail", "Caracol turco (Helix lucorum), sellado con su epifragma en la hiedra", -130f, 60f, 2, 4.5f, 30f),
            new Animal("Woodlouse", "Cochinilla de bola (Armadillidium vulgare), bajo las lápidas", 100f, -700f, 3, 1.4f, 40f),
            new Animal("Ladybug", "Mariquitas de 7 puntos invernando en los cipreses de Karacaahmet", 650f, -150f, 4, 0.7f, 60f),
            new Animal("Ant", "Hormiga segadora (Messor), en un día templado de la ribera", -400f, -300f, 2, 0.7f, 50f),
        },
        buildings = new[]
        {
            new Building("Cuartel de Selimiye (Hospital de los Barracones)", 0f, 0f), new Building("Torre de Florence Nightingale", -130f, 110f),
            new Building("Mezquita de Selimiye", 250f, 200f), new Building("Embarcadero del Bósforo", -440f, 0f),
            new Building("Cementerio militar británico (Haidar Pasha)", 100f, -700f), new Building("Cementerio de Karacaahmet", 650f, -150f),
        },
        todo = new[] { "en vivo: que las cucarachas queden pegadas a las alas del cuartel (no en el patio) y los cipreses en los dos cementerios" },
    };

    // ══ EGINA, la Grecia heroica (Éaco y los mirmidones): sobre la Construcción ═══════════════════════════════════════════════════════════════════
    // La isla del golfo Sarónico a final de primavera: al oeste, el puerto y la colina de Kolona (el asentamiento de la Edad del Bronce y el santuario
    // de Apolo); al sureste, el monte Oros con el santuario de Zeus Helenio (el Zeus al que Éaco rezó); en medio, los olivares y los campos de cebada ya
    // secos. El mito (Ovidio, Met. VII; Estrabón VIII.6.16): Zeus convirtió en hombres a las hormigas que subían por su árbol; Estrabón lo explica por
    // los eginetas, que cavaban la tierra pobre como hormigas. La garriga: tomillo, pimpinela espinosa, jara, salvia, gamones, lentisco sobre la caliza.
    static Map MapEgina() => new Map
    {
        display = "Egina", place = "Egina, el puerto, Kolona y el monte Oros, final de la primavera (la Grecia heroica)", status = Status.Hecho,
        source = "Pausanias, Descripción de Grecia II.29–30 (Kolona, el Eaceo y Zeus Helenio en el Oros); Ovidio, Metamorfosis VII.614–660 (los mirmidones); " +
                 "Estrabón, Geografía VIII.6.16; Walter y Felten, Alt-Ägina III.1 (1981: el asentamiento de Kolona); el plano del pueblo",
        floor = Natural(
            new Piece("escalera", Como, "las escaleras: las terrazas de piedra seca de las laderas del Oros", "escalera"),
            new Piece("caja", Como, "las cajas de herramientas: las casas de piedra apiñadas de Kolona", "caja")),
        flora = new[]
        {
            // Kolona: el tomillo de las abejas sobre la caliza de la colina.
            new Plant("Tomillo de Creta", -520f, 120f, 8, 80f), new Plant("Caliza", -520f, 120f, 4, 40f),
            // La costa del Sarónico: lentisco.
            new Plant("Lentisco", -650f, -300f, 8, 80f),
            // Los olivares (con anémonas al pie y la tierra roja) y los campos de cebada ya agostados.
            new Plant("Anémona", 300f, 250f, 8, 100f), new Plant("Tierra roja", 300f, 250f, 4, 80f), new Plant("Hierba seca", 250f, -250f, 10, 100f),
            new Plant("Gamón", -300f, 350f, 6, 80f),
            // El monte Oros: jara al pie, pimpinela espinosa y salvia en la ladera, caliza en la cima.
            new Plant("Jara de Creta", 450f, -450f, 8, 100f), new Plant("Pimpinela espinosa", 650f, -650f, 10, 120f), new Plant("Salvia griega", 750f, -750f, 8, 100f),
            new Plant("Caliza", 800f, -800f, 6, 80f),
        },
        fauna = new[]
        {
            new Animal("Ant", "Hormiga segadora (Messor), la del mito: su fila sube por el camino del Oros", 300f, -270f, 6, 0.8f, 80f),
            new Animal("Ant", "Hormiga segadora, recogiendo el grano en los campos de cebada", 250f, -250f, 4, 0.8f, 80f),
            new Animal("SnowBee", "Abeja melífera griega (Apis mellifera cecropia), en el tomillo de Kolona", -520f, 120f, 3, 1.3f, 60f),
            new Animal("Cicada", "Cigarra del fresno (Cicada orni), cantando en los olivos", 300f, 250f, 3, 2.5f, 100f),
            new Animal("Grasshopper", "Langosta egipcia (Anacridium aegyptium), en los olivares", 350f, 300f, 2, 5f, 80f),
            new Animal("Mantis", "Mantis religiosa, en la salvia del Oros", 650f, -650f, 2, 6f, 100f),
            new Animal("DungBeetle", "Escarabajo pelotero (Scarabaeus), en el camino del puerto", -300f, 50f, 2, 2.5f, 60f),
            new Animal("Antlion", "Hormiga león (Myrmeleon), en la arena de la costa", -650f, -300f, 2, 1.2f, 60f),
            new Animal("Scorpion", "Escorpión griego (Mesobuthus gibbosus), bajo las piedras de la cima", 800f, -800f, 2, 6f, 80f),
        },
        buildings = new[]
        {
            new Building("Santuario de Apolo en Kolona", -520f, 120f), new Building("Puerto de Egina", -600f, 0f),
            new Building("Santuario de Zeus Helenio", 800f, -800f), new Building("Palacio de Éaco", -100f, 300f),
            new Building("Talleres de los alfareros", -300f, 0f),
        },
        todo = new[] { "en vivo: que la fila de hormigas del camino del Oros se vea subiendo hacia el santuario" },
    };
}
