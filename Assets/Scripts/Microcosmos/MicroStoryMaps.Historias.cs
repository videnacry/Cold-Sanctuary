using UnityEngine;

/// <summary>
/// LOS MAPAS A MANO DE LA TANDA 3 (docs/microcosmos-historias-3.md; regla M9): todos HECHOS. Metros desde la «Historia», x al este y z al
/// norte: el mismo marco que su plano (Assets/Editor/Historias/). Los nombres de planta empiezan como en su bioma; al construir, el
/// registro lo dice si no («[Mapa] … no está en su bioma»). En un mapa hecho no se siembra nada al azar: los animales del pueblo (perros,
/// burros…) también van aquí. Cada mapa, un método (el orden de inicialización entre archivos de una clase parcial no está garantizado).
/// </summary>
public static partial class MicroStoryMaps
{
    static MicroFloorElements.Action Quitar => MicroFloorElements.Action.Quitar;
    static MicroFloorElements.Action Apilar => MicroFloorElements.Action.Apilar;
    static MicroFloorElements.Action Como => MicroFloorElements.Action.Como;

    /// <summary>LOS MAPAS DE LA TANDA 3, en el orden de la tabla de pisos.</summary>
    public static readonly Map[] Historias3 =
    {
        MapChimbacalle(), MapLagunillas(), MapCajamarca(), MapOnondaga(), MapAlejandriaCleopatra(),
    };

    // ══ CHIMBACALLE, 25 de junio de 1908 (Eloy Alfaro): sobre Mecánica (pendiente de su estación) ══════════════════════════════
    // Quito al norte tras el Panecillo, el Machángara al este, el Pichincha al oeste; junio: el verano seco de la sierra. Eucaliptos
    // recién plantados, capulíes en los patios, pencos en los bordes de los caminos, sigses en la quebrada y paja en el páramo.
    static Map MapChimbacalle() => new Map
    {
        display = "Chimbacalle", place = "Chimbacalle, al sur de Quito, 25 de junio de 1908", status = Status.Hecho,
        source = "Clark 1998, The Redemptive Work; Ayala Mora 1994; planos históricos de Quito (Atlas histórico de Quito, Instituto Geográfico Militar)",
        floor = new[]
        {
            new Piece("alfombra", Apilar, "las alfombras del taller, apiladas: el Panecillo, la loma que separa Chimbacalle del centro de Quito"),
            new Piece("hornilla", Como, "en este valle no hay volcanes a mano: los fogones son las mesetas del altiplano por donde sube la vía", "mesa"),
            new Piece("silla", Como, "las sillas, nevados: el Pichincha, el Cotopaxi y el Antisana en el horizonte de Quito", "silla"),
            new Piece("caja", Como, "las cajas de herramientas: fábricas, los talleres del ferrocarril", "caja"),
        },
        flora = new[]
        {
            new Plant("Eucalipto", -260f, 120f, 8, 60f), new Plant("Eucalipto", 300f, -300f, 7, 70f), new Plant("Eucalipto", -500f, -350f, 6, 60f),
            new Plant("Capulí", 220f, 90f), new Plant("Capulí", -180f, -180f), new Plant("Capulí", 400f, 160f), new Plant("Capulí", -320f, -220f),
            new Plant("Penco", -60f, -110f, 8, 90f), new Plant("Penco", 130f, -420f, 8, 80f),
            new Plant("Sigse", 560f, 240f, 12, 80f), new Plant("Sigse", 620f, -180f, 10, 70f),
            new Plant("Chilca", 520f, 40f, 10, 70f), new Plant("Paja de páramo", -950f, 420f, 30, 170f), new Plant("Paja de páramo", -1050f, -200f, 24, 150f),
            new Plant("Retama", -130f, 620f, 10, 60f), new Plant("Andesita", -880f, 300f, 6, 80f), new Plant("Andesita", -130f, 700f, 5, 50f),
        },
        fauna = new[]
        {
            new Animal("SnowBee", "Abejorro del páramo (Bombus funebris)", -950f, 420f, 4, 2f, 60f),
            new Animal("Ant", "Hormiga de los eucaliptos", -260f, 120f, 5, 1.2f, 40f),
            new Animal("Grasshopper", "Saltamontes de la paja (Trimerotropis)", -1050f, -200f, 4, 2f, 50f),
            new Animal("Spider", "Araña de la quebrada", 560f, 240f, 2, 1.6f, 40f),
            new Animal("Aphid", "Burro de carga (en el microcosmos, un pulgón de carga)", 120f, -150f, 3, 1.6f, 60f),
            new Animal("Woodlouse", "Perro del barrio (en el microcosmos, una cochinilla hormiguera)", -80f, -220f, 3, 0.7f, 60f),
            new Animal("Ladybug", "Gallina del patio (en el microcosmos, una mariquita parda)", 240f, 110f, 3, 0.5f, 30f),
        },
        buildings = new[] { new Building("Estación de Chimbacalle", 40f, 90f), new Building("Talleres del ferrocarril", -160f, 90f), new Building("Escuela Normal", 200f, 300f), new Building("Parada del ferrocarril", -420f, -480f), new Building("Plaza del barrio", 120f, -120f), new Building("Mercado", 340f, -240f) },
        todo = new[] { "en vivo: mirar que la vía cae entera dentro del barrio y que ningún eucalipto cae sobre ella" },
    };
}
