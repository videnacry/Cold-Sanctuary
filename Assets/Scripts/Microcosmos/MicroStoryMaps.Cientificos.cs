using UnityEngine;

/// <summary>LOS MAPAS A MANO DE LOS CIENTÍFICOS (2026-11-12; petición 75, regla M9; docs/microcosmos-mapas-a-mano.md §Tanda 4). Mismo marco que el plano del
/// pueblo (<c>CivicBuilder.TownPlans</c>). En las CIUDADES no hay flora ni bichos salvajes (MicroBiome): solo lo plantado y los animales del pueblo
/// (los caballos, pulgones de tiro; los gatos, cochinillas; las ratas, cucarachas), cada uno donde estaba.</summary>
public static partial class MicroStoryMaps
{
    // ══ BERNA, mayo de 1905 (Einstein y el año milagroso): sobre la Planta Atómica ══════════════════════════════════════════════════════════
    // El casco viejo en la península del Aar: la Kramgasse con sus soportales y sus fuentes, la Zytglogge al oeste de ella (el escenario) y la casa
    // de los Einstein en el n.º 49; la Oficina de patentes cerca de la estación; la terraza del Palacio Federal (1902) y la Kleine Schanze al suroeste;
    // la Münsterplattform con sus castaños en flor; el Aar alrededor, con los puentes de Nydegg, Kirchenfeld y Kornhaus; el Gurten al sur.
    static Map MapBerna() => new Map
    {
        display = "Berna", place = "Berna, el casco viejo en la península del Aar, mayo de 1905", status = Status.Hecho,
        source = "Stadtplan der Stadt Bern 1905 (el plano del pueblo); Einstein-Haus Bern (Kramgasse 49, piso de 1903–1905); Stadtgrün Bern, «Münsterplattform» " +
                 "y «Kleine Schanze» (castaños, tilos, césped y setos); Bundeshaus (Palacio Federal, 1902)",
        floor = Natural(
            new Piece("banera", Como, "la piscina del reactor: el meandro del Aar", "banera"),
            new Piece("mesa", Como, "bancos y consolas: el Gurten al sur y la meseta del Kirchenfeld", "mesa"),
            new Piece("lavabo", Como, "los lavabos: las fuentes de la ciudad vieja (la del Zähringer, la del Ogro, la de la Justicia)", "lavabo")),
        flora = new[]
        {
            // La Münsterplattform: castaños de Indias en flor, su césped y sus setos.
            new Plant("Castaño de Indias", 420f, -140f, 8, 40f), new Plant("Césped de la Kleine Schanze", 420f, -140f, 10, 40f), new Plant("Seto de boj", 420f, -140f, 4, 30f),
            // La Kleine Schanze, al suroeste: castaños, césped y setos; y los tilos de la terraza del Palacio Federal.
            new Plant("Castaño de Indias", -560f, -140f, 6, 50f), new Plant("Césped de la Kleine Schanze", -560f, -140f, 20, 60f), new Plant("Seto de boj", -560f, -140f, 6, 50f),
            new Plant("Tilo", -320f, -170f, 4, 30f),
            // Los tilos de las plazas (Bärenplatz y Bundesplatz) y los geranios de las jardineras de la Kramgasse y la Gerechtigkeitsgasse.
            new Plant("Tilo", -220f, -60f, 4, 20f),
            new Plant("Geranios de las jardineras", 200f, 8f, 5, 15f), new Plant("Geranios de las jardineras", 450f, 8f, 5, 15f), new Plant("Geranios de las jardineras", 650f, 8f, 5, 15f),
            // El adoquín de arenisca de la Münstergasse.
            new Plant("Adoquín de arenisca", 300f, -70f, 4, 30f),
        },
        fauna = new[]
        {
            new Animal("Aphid", "Caballo del carro de reparto (en el microcosmos, un pulgón de tiro), en la Kramgasse", 350f, 0f, 3, 2.4f, 40f),
            new Animal("Aphid", "Caballo de carro en el mercado de la Bärenplatz", -250f, 40f, 3, 2.4f, 30f),
            new Animal("Aphid", "Caballo de coche de punto ante la estación", -700f, 40f, 3, 2.4f, 40f),
            new Animal("Woodlouse", "Gato de los soportales (en el microcosmos, una cochinilla hormiguera)", 200f, 70f, 2, 0.5f, 20f),
            new Animal("Woodlouse", "Gato de la Münstergasse", 450f, -60f, 2, 0.5f, 20f),
        },
        buildings = new[]
        {
            new Building("Casa de Einstein (Kramgasse 49)", 150f, 15f), new Building("Oficina de patentes (Genfergasse)", -650f, 200f), new Building("Plaza (Bärenplatz)", -220f, -60f),
            new Building("Estación", -700f, 40f), new Building("Münsterplattform", 420f, -140f), new Building("Kleine Schanze", -560f, -140f), new Building("Puente de Nydegg", 800f, 0f),
        },
        todo = new[] { "en vivo: que los castaños caigan en la plataforma y no en la Münstergasse; que los caballos queden en la calle y no bajo los soportales" },
    };
}
