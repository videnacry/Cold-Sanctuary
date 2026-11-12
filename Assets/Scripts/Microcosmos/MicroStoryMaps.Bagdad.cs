using UnityEngine;

/// <summary>EL MAPA A MANO DE BAGDAD (2026-11-12; petición 75, regla M9; docs/microcosmos-mapas-a-mano.md §Tanda 4).</summary>
public static partial class MicroStoryMaps
{
    // ══ BAGDAD, hacia 820 (al-Juarismi en la Casa de la Sabiduría): sobre el Aula ══════════════════════════════════════════════════════════════
    // La Ciudad Redonda de al-Mansur (762) con sus cuatro puertas (Jorasán, Basora, Kufa y Siria) y, en el centro, el palacio de la Puerta de Oro y la gran
    // mezquita; entre las murallas, las casas con patio y sus jardines (granados, naranjos amargos, rosales de Damasco, setos de arrayán). Al sur, el
    // barrio de al-Karj con su zoco entre los canales Sarat e Isa, bordeados de palmerales; al este, el Tigris con el puente de barcas y los huertos de
    // palmeras de su orilla. Una ciudad: solo lo plantado y sus animales (los asnos de los aguadores, los camellos de las caravanas, los gatos).
    static Map MapBagdad() => new Map
    {
        display = "Bagdad", place = "Bagdad, la Ciudad Redonda y el barrio de al-Karj, hacia 820", status = Status.Hecho,
        source = "Lassner, The Topography of Baghdad in the Early Middle Ages (1970: la Ciudad Redonda, sus puertas y al-Karj); Le Strange, Baghdad during the Abbasid " +
                 "Caliphate (1900: los canales Sarat e Isa y el puente de barcas); al-Yaʿqubi, Kitab al-Buldan (s. IX); Ruggles, Islamic Gardens and Landscapes (2008: " +
                 "los jardines de patio abasíes); el plano del pueblo",
        floor = Natural(
            new Piece("mesa", Como, "pupitres y mesas: los tells (montículos de las ciudades antiguas) de la llanura mesopotámica", "mesa"),
            new Piece("pared", Como, "la pizarra y los paneles: la muralla de la Ciudad Redonda", "pared")),
        flora = new[]
        {
            // Los palmerales de los canales Sarat e Isa y de la orilla del Tigris.
            new Plant("Palmera datilera", -600f, -150f, 12, 80f), new Plant("Palmera datilera", 300f, -150f, 10, 80f), new Plant("Palmera datilera", -400f, -460f, 12, 80f),
            new Plant("Palmera datilera", 650f, -400f, 10, 80f), new Plant("Palmera datilera", 680f, 700f, 10, 80f),
            // Los jardines de los patios entre las murallas de la Ciudad Redonda.
            new Plant("Granado del patio", -500f, 600f, 6, 40f), new Plant("Granado del patio", -100f, 250f, 6, 40f), new Plant("Naranjo amargo", -100f, 600f, 6, 40f),
            new Plant("Rosal de Damasco", -150f, 600f, 6, 40f), new Plant("Rosal de Damasco", -450f, 300f, 6, 40f), new Plant("Seto de arrayán", -300f, 650f, 6, 40f),
            // El ladrillo cocido de la gran plaza del centro.
            new Plant("Ladrillo cocido", -300f, 450f, 4, 30f),
        },
        fauna = new[]
        {
            new Animal("Aphid", "Asno del aguador (en el microcosmos, un pulgón de carga), en el canal Sarat", -200f, -150f, 3, 1.6f, 50f),
            new Animal("Aphid", "Asno del aguador, en el canal Isa", 100f, -460f, 2, 1.6f, 50f),
            new Animal("Aphid", "Camello de la caravana del zoco de al-Karj (en el microcosmos, un pulgón de carga)", -150f, -300f, 3, 3f, 40f),
            new Animal("Woodlouse", "Gato del patio (en el microcosmos, una cochinilla hormiguera)", -400f, 600f, 2, 0.5f, 40f),
            new Animal("Woodlouse", "Gato del patio", 0f, 600f, 2, 0.5f, 40f),
        },
        buildings = new[]
        {
            new Building("Palacio de la Puerta de Oro y gran mezquita", -300f, 450f), new Building("Puerta de Jorasán", 190f, 800f), new Building("Puerta de Basora", 190f, 100f),
            new Building("Puerta de Kufa", -790f, 100f), new Building("Puerta de Siria", -790f, 800f), new Building("Puente de barcas del Tigris", 700f, 300f),
            new Building("Zoco de al-Karj", -150f, -300f),
        },
        todo = new[] { "en vivo: que los palmerales caigan en las orillas de los canales y no en el agua; que los patios queden entre las dos murallas" },
    };
}
