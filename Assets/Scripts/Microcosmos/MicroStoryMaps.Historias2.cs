using UnityEngine;

/// <summary>LOS MAPAS A MANO DE LA TANDA 3 (II): las historias de pueblos antes de la máquina (Arauco, Cajamarca, Onondaga, Alejandría).</summary>
public static partial class MicroStoryMaps
{
    /// <summary>En las historias anteriores a la máquina, lo que en el piso sería máquina, antena o estación es roca o peñón (no hay fábricas
    /// en 1557): se añade a las órdenes propias del mapa.</summary>
    static Piece[] Natural(params Piece[] own)
    {
        var l = new System.Collections.Generic.List<Piece>(own);
        l.Add(new Piece("maquina", Como, "antes de las máquinas: un afloramiento de roca", "roca"));
        l.Add(new Piece("carro", Como, "sin ruedas en este lugar y época: un peñasco", "roca"));
        l.Add(new Piece("poste", Como, "sin antenas: una aguja de roca", "columna"));
        l.Add(new Piece("lampara", Como, "sin postes de luz: una aguja de roca", "columna"));
        l.Add(new Piece("botella", Como, "sin torres: una aguja de roca", "columna"));
        l.Add(new Piece("cajonera", Como, "sin edificios altos: una loma de roca", "roca"));
        return l.ToArray();
    }

    // ══ UN LOF DE ARAUCO, noviembre de 1557 (Galvarino): sobre el Hospital ═════════════════════════════════════════════════════
    // Bosque templado en primavera: robles, arrayanes y quila en las lomas, canelos junto a los rewe, copihues en los bordes, las chacras
    // al sur, el golfo al oeste y Nahuelbuta al este. Las camas del hospital son las lomas; las sillas no son nevados (no hay nieve en
    // la costa de Arauco en noviembre): son cerros.
    static Map MapLagunillas() => new Map
    {
        display = "Lagunillas", place = "Un lof de las tierras de Arauco, noviembre de 1557", status = Status.Hecho,
        source = "Bengoa 2003; Ercilla, La Araucana; el detalle del lof es una recreación (no hay plano): paisaje de la provincia de Arauco (OpenStreetMap)",
        floor = Natural(
            new Piece("alfombra", Apilar, "las alfombras, apiladas: la loma del canelo"),
            new Piece("silla", Como, "en la costa de Arauco no hay nieve: las sillas son cerros de la cordillera de Nahuelbuta", "mesa"),
            new Piece("cama", Como, "las camas son las lomas boscosas del lof", "cama")),
        flora = new[]
        {
            new Plant("Canelo", -150f, 160f), new Plant("Canelo", -60f, 240f), new Plant("Canelo", -380f, 480f, 8, 60f), new Plant("Canelo", -180f, 90f),
            new Plant("Roble pellín", 900f, -850f, 18, 240f), new Plant("Roble pellín", 520f, 520f, 10, 110f), new Plant("Roble pellín", 1100f, 240f, 10, 160f),
            new Plant("Arrayán", -500f, 620f, 8, 80f), new Plant("Arrayán", -130f, 190f, 5, 30f),
            new Plant("Quila", 640f, -300f, 12, 110f), new Plant("Quila", -620f, -420f, 10, 100f),
            new Plant("Copihue", 500f, 460f, 6, 60f), new Plant("Copihue", 820f, -620f, 8, 90f),
            new Plant("Helecho", -560f, 700f, 14, 90f), new Plant("Helecho", 300f, 40f, 8, 50f),
            new Plant("Maqui", 180f, -300f, 6, 60f), new Plant("Matico", -220f, 60f, 5, 40f), new Plant("Kinwa", 260f, -420f, 22, 130f),
            new Plant("Pasto del claro", -60f, 20f, 18, 90f), new Plant("Piedra de Nahuelbuta", 1050f, 300f, 7, 90f),
        },
        fauna = new[]
        {
            new Animal("DungBeetle", "Ciervo volante (Chiasognathus grantii), en los robles", 900f, -850f, 3, 2.4f, 90f),
            new Animal("SnowBee", "Moscardón nativo (Bombus dahlbomii), en los copihues", 500f, 460f, 4, 1.8f, 60f),
            new Animal("Cicada", "Cigarra chilena (Tettigades)", 520f, 520f, 3, 1.6f, 70f),
            new Animal("Ant", "Hormiga negra (Camponotus morosus)", -380f, 480f, 5, 1.2f, 50f),
            new Animal("Firefly", "Luciérnaga del estero", -300f, 400f, 3, 1f, 60f),
            new Animal("Aphid", "Chilihueque (en el microcosmos, un pulgón de lana)", -240f, -120f, 3, 1.8f, 40f),
            new Animal("Woodlouse", "Perro del lof (en el microcosmos, una cochinilla hormiguera)", 120f, 60f, 2, 0.7f, 50f),
        },
        buildings = new[] { new Building("Ruka de Galvarino", 140f, 120f), new Building("Ruka de la machi", -150f, 120f), new Building("Campo del koyag", -60f, 210f), new Building("Paliwe", -480f, -300f), new Building("Chacra del lof", 260f, -240f) },
        todo = new[] { "en vivo: que el mar del golfo entre por el oeste y no tape el paliwe" },
    };

    // ══ CAJAMARCA, 15 de noviembre de 1532 (Atahualpa): sobre el Estudio Textil (pendiente de su contaduría) ═════════════════════════════
    // El valle al empezar las lluvias, con la siembra: maizales y papas, alisos en las acequias, ichu en los cerros; la plaza, las kallankas y las
    // colcas; Pultumarca al este. Las mesas de corte son la meseta de los cerros; los rollos de tela, las terrazas de cultivo.
    static Map MapCajamarca() => new Map
    {
        display = "Cajamarca", place = "Cajamarca, valle del norte del Perú, 15 de noviembre de 1532", status = Status.Hecho,
        source = "Xerez 1534; Hyslop 1990, Inka Settlement Planning; Weberbauer 1945; OpenStreetMap, Cajamarca",
        floor = Natural(
            new Piece("alfombra", Apilar, "las alfombras, apiladas: el cerro Santa Apolonia, sobre la plaza"),
            new Piece("silla", Como, "las sillas son cerros secos de la sierra norte: no hay nevados a la vista de Cajamarca", "mesa"),
            new Piece("escalera", Como, "las escaleras son andenes: terrazas de cultivo", "escalera"),
            new Piece("lavabo", Como, "los lavabos, las lagunas altas que dan agua al valle", "lavabo")),
        flora = new[]
        {
            new Plant("Maíz", 720f, -820f, 30, 170f), new Plant("Maíz", 620f, -560f, 14, 90f), new Plant("Papa", -760f, 480f, 26, 140f), new Plant("Papa", 280f, -420f, 10, 70f),
            new Plant("Aliso", -300f, -420f, 7, 80f), new Plant("Aliso", 300f, -480f, 7, 80f), new Plant("Aliso", 1000f, 240f, 5, 60f),
            new Plant("Lúcuma", 170f, 200f), new Plant("Lúcuma", -300f, 230f), new Plant("Lúcuma", 340f, 20f), new Plant("Lúcuma", -180f, -280f),
            new Plant("Ichu", -340f, -640f, 18, 150f), new Plant("Ichu", -1100f, 100f, 24, 200f),
            new Plant("Chilca", -500f, -260f, 8, 60f), new Plant("Tuna", 240f, -340f, 8, 60f), new Plant("Tuna", -140f, 440f, 6, 50f), new Plant("Lupino", 860f, 620f, 12, 120f),
            new Plant("Caliza", -340f, -680f, 7, 70f),
        },
        fauna = new[]
        {
            new Animal("Ant", "Hormiga andina (Camponotus)", 720f, -820f, 5, 1.2f, 80f),
            new Animal("SnowBee", "Abejorro andino (Bombus)", 860f, 620f, 4, 1.8f, 90f),
            new Animal("Grasshopper", "Saltamontes del ichu", -1100f, 100f, 4, 2f, 100f),
            new Animal("Spider", "Araña del pajonal", -340f, -640f, 2, 1.6f, 50f),
            new Animal("Aphid", "Llama de carga (en el microcosmos, un pulgón de carga)", 150f, 60f, 4, 1.8f, 70f),
            new Animal("Aphid", "Llama del ganado del Inca (en el microcosmos, un pulgón de lana)", 860f, 620f, 4, 1.8f, 100f),
            new Animal("Woodlouse", "Cuy (en el microcosmos, una cochinilla pequeña)", -60f, -200f, 3, 0.4f, 40f),
        },
        buildings = new[] { new Building("Haukaypata", 30f, -10f), new Building("Casa de los quipus", 40f, 120f), new Building("Colcas", -230f, -160f), new Building("Acllawasi", -360f, 200f), new Building("Pultumarca", 930f, 180f), new Building("Templo del Sol", -480f, -40f) },
        todo = new[] { "en vivo: la plaza es triangular en Xerez; aquí, rectangular: mirar si se puede dar la forma con las calles" },
    };
}
