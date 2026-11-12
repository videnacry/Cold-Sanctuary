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
}
