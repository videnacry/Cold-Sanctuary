/// <summary>
/// LOS MAPAS A MANO DE LA MÚSICA (docs/microcosmos-musica.md; regla M9). Todos «Hecho»: el piso ordenado para el lugar real, cada rodal de
/// flora y cada bicho en su sitio (metros desde la «Historia», x al este, z al norte; el mismo marco que <c>CivicBuilder.MusicPlans</c>).
/// </summary>
public static partial class MicroStoryMaps
{
    const MicroFloorElements.Action Quitar = MicroFloorElements.Action.Quitar, Apilar = MicroFloorElements.Action.Apilar, Como = MicroFloorElements.Action.Como;

    public static readonly Map[] Music =
    {
        // ══ LA PLAYA (Barranquilla), 2009 (Shakira): sobre el Aula ══════════════════════════════════════════════════════════════════
        new Map
        {
            display = "Barranquilla", place = "Corregimiento de La Playa, Barranquilla, 2009", status = Status.Hecho,
            source = "Fundación Pies Descalzos, Historia; Humboldt 2014 (bosque seco); OpenStreetMap",
            floor = new[]
            {
                new Piece("silla", Como, "ni nieve en el Caribe: las sillas son cerros de las lomas de Puerto Colombia", "mesa"),
                new Piece("libro", Como, "los libros del Aula son las casas de bloque de colores: el pueblo sube por las estanterías", "cajonera"),
                new Piece("pared", Como, "la pizarra y la pantalla no son acantilados: la barranca baja de la orilla de la Ciénaga", "escalera"),
                new Piece("alfombra", Quitar, "el suelo del corregimiento es llano: arena y cemento"),
            },
            flora = new[]
            {
                new Plant("Mangle rojo", -300f, 370f, 12, 60f), new Plant("Mangle rojo", 220f, 375f, 10, 50f), new Plant("Mangle salado", -40f, 365f, 10, 50f),
                new Plant("Pasto de la orilla", -520f, 380f, 18, 60f), new Plant("Pasto de la orilla", 480f, 385f, 14, 50f),
                new Plant("Trupillo", -700f, -470f, 12, 80f), new Plant("Trupillo", -800f, 110f, 8, 60f), new Plant("Trupillo", 760f, 120f, 8, 60f),
                new Plant("Cardón", -760f, -620f, 6, 50f), new Plant("Cardón", 790f, -420f, 5, 40f),
                new Plant("Almendro de playa", -80f, -105f), new Plant("Almendro de playa", 80f, -105f), new Plant("Almendro de playa", -230f, 35f), new Plant("Almendro de playa", 230f, 35f),
                new Plant("Roble morado", -60f, -235f), new Plant("Roble morado", 60f, -235f), new Plant("Roble morado", -60f, -330f), new Plant("Roble morado", 60f, -330f),
                new Plant("Matarratón", -340f, 80f, 6, 18f), new Plant("Matarratón", 400f, 80f, 6, 18f),
                new Plant("Trinitaria", 150f, 80f, 4, 8f), new Plant("Trinitaria", -150f, 80f, 4, 8f),
                new Plant("Arena de la Ciénaga", -160f, 400f, 6, 40f), new Plant("Arena de la Ciénaga", 300f, 405f, 5, 30f),
            },
            fauna = new[]
            {
                new Animal("Ant", "Hormiga arriera (Atta colombica), hacia los trupillos", -690f, -440f, 6, 1.6f, 25f),
                new Animal("Firefly", "Cocuyo del manglar", -280f, 360f, 4, 1.2f, 40f),
                new Animal("Grasshopper", "Saltamontes de la seca", 760f, 100f, 4, 2.4f, 40f),
                new Animal("Cricket", "Grillo del solar", 620f, -520f, 3, 1.8f, 40f),
            },
            buildings = new[]
            {
                new Building("Colegio Pies Descalzos de La Playa", 0f, 130f), new Building("Comedor escolar", 140f, 170f), new Building("Centro comunitario", -140f, 160f),
                new Building("Cancha del colegio", 200f, -70f), new Building("Puesto de salud de La Playa", 330f, -100f), new Building("Plaza", -60f, -280f), new Building("Mercado", 170f, -290f),
            },
            todo = new[] { "en vivo: mirar que el manglar quede en la orilla de la Ciénaga y no sobre una calle" },
        },

        // ══FIN DE LOS MAPAS══
    };
}
