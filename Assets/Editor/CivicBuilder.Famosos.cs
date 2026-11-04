using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LOS PUEBLOS DE LOS FAMOSOS (docs/microcosmos-famosos.md; Berón 2026-11-04). Cada figura en el pueblo real de su historia, sobre su
/// PLANO (<see cref="FamousPlans"/>, mismo formato que <c>PlanText</c>; fuentes en el doc), con su estilo de época y lugar
/// (MicroStyles: india_moderna, sahel_moderno, norteamerica_interior…) y las reglas de siempre: edificios abiertos de
/// <see cref="Building"/> con su etiqueta de actividad, el paso de 2 m, muebles a la medida de insectos-humanos y personas, un hogar
/// para cada personaje (Homes) y su frutal en cada residencia. Solo los muros que la historia necesita (ninguno, en estos siete).
/// </summary>
internal static partial class CivicBuilder
{
    static readonly Style IshaBrick = new Style(C(0.7f, 0.46f, 0.34f), C(0.82f, 0.78f, 0.7f), C(0.6f, 0.35f, 0.25f), Roof.None, 3.4f);     // Isha: ladrillo de barro y suelo de piedra clara
    static readonly Style TamilTile = new Style(C(0.92f, 0.88f, 0.78f), C(0.6f, 0.45f, 0.32f), C(0.7f, 0.32f, 0.22f), Roof.Gable, 3.2f);  // aldea tamil: encalado y teja de Mangalore

    /// <summary>El pueblo de cada famoso (0 si <paramref name="display"/> no es uno de ellos).</summary>
    static int FamousTown(Transform t, string display)
    {
        switch (display)
        {
            case "Velliangiri": return VelliangiriTown(t);
            default: return 0;
        }
    }

    // ══ LOS PLANOS (metros reales, claro de la historia en (0, 0), x = este, z = norte; formato en CivicBuilder.Plans) ══════════
    static readonly Dictionary<string, string> FamousPlans = new Dictionary<string, string>
    {
        // VELLIANGIRI (1999): el Isha Yoga Center al pie de los montes Velliangiri (al oeste, el monte de 1840 m y sus siete cumbres),
        // el arroyo que baja hacia la Noyyal, la aldea de Ikkarai Boluvampatti al este con sus cocotales y arrozales, y la carretera
        // que va a Coimbatore (40 km al este). Plano aproximado (Isha Foundation; OpenStreetMap, «Isha Yoga Center»).
        ["Velliangiri"] = @"
calle Camino del ashram | 8 | 0 -80, 0 320
calle Camino de las cabañas | 5 | -260 90, 260 90
calle Carretera de Coimbatore | 8 | 0 -80, 600 -220, 1300 -320
calle Calle mayor de Ikkarai Boluvampatti | 6 | 450 -520, 760 -90
calle Calle del templo de la aldea | 5 | 520 -260, 900 -330
rio Arroyo de la Noyyal | 14 | -900 320, -320 260, 280 -380, 900 -760
monte Montes Velliangiri | 900 1400 | -1550 250
colina Falda del monte | 160 45 | -560 -380
campo Cocotales y platanares | 320 220 | 460 380
campo Arrozales de la aldea | 300 220 | 820 -620
hito 0 | 0 140
hito 1 | -150 60
hito 2 | 160 40
hito 3 | -110 230
hito 4 | 150 180
hito 5 | 260 -40
hito 6 | 620 -300
Plaza | 700 -220
Mercado | 610 -150
Pileta | 560 -380
Parque | -300 -150
Entrenamiento | 760 -420
Guarderia | 520 -430
Taller | 850 -200
Carpinteria | 420 -300
Obra | -250 220
Hotel | 300 -150
relleno | 40 | 600 -250",
    };

    // ══ VELLIANGIRI (1999): el Isha Yoga Center (el Dhyanalinga bajo su cúpula, la sala de práctica, el comedor, las cabañas de los
    //    voluntarios, el huerto) y la aldea de Ikkarai Boluvampatti (casas encaladas con teja, tienda, herrero, cocotales).
    static int VelliangiriTown(Transform t)
    {
        var s = Town(t, "Velliangiri", new[]
        {
            new Lot("Dhyanalinga", "Templo", "El Dhyanalinga (meditar en silencio, abierto a todos)", 16f, 16f, IshaBrick,
                S("altar", 0f, 2f, 2.4f, 2.4f, "El linga de granito"), S("esterilla", -4f, -3f, 1f, 2f, "Esterilla para sentarse"), S("esterilla", 4f, -3f, 1f, 2f, "Esterilla para sentarse")),
            new Lot("Sala de práctica del ashram", "Sala de Yoga", "Sala de práctica del ashram (hatha yoga y meditación)", 14f, 12f, IshaBrick,
                S("esterilla", -3f, 2f, 1f, 2f, "Esterilla"), S("esterilla", 0f, 2f, 1f, 2f, "Esterilla"), S("esterilla", 3f, 2f, 1f, 2f, "Esterilla"), S("estante", 0f, -4f, 1.6f, 0.5f, "Cojines y mantas")),
            new Lot("Comedor del ashram", "Restaurante", "Comedor del ashram (se come en el suelo, en silencio)", 14f, 10f, IshaBrick,
                S("fogon", -3f, 2f, 1.6f, 0.8f, "Ollas de acero y de barro"), S("mesa", 2f, -1f, 2f, 1f, "Hojas de plátano sobre la estera"), S("lavadero", 4f, 3f, 1.4f, 0.8f, "Pila de lavar los platos")),
            new Lot("Casa de Sadhguru", "Casa", "Casa de Sadhguru en el ashram", 12f, 10f, IshaBrick,
                S("cama", -2f, 2f, 1.2f, 2f, "Catre"), S("estante", 3f, 2f, 0.8f, 1.6f, "Libros")),
            new Lot("Cabañas de los voluntarios", "Casa", "Cabañas de los voluntarios", 14f, 10f, IshaBrick,
                S("cama", -4f, 2f, 1f, 2f, "Catre"), S("cama", 0f, 2f, 1f, 2f, "Catre"), S("cama", 4f, 2f, 1f, 2f, "Catre")),
            new Lot("Huerto del ashram", "Huerto", "Huerto del ashram (verdura para la cocina)", 14f, 12f, IshaBrick,
                S("bancal", -2f, 0f, 4f, 6f, "Bancal de verduras"), S("pozo", 4f, 2f, 1.2f, 1.2f, "Pozo")),
            new Lot("Escuela de la aldea", "Escuela", "Escuela de la aldea (Ikkarai Boluvampatti)", 14f, 10f, TamilTile,
                S("pizarra", 0f, 3.5f, 2f, 0.4f, "Pizarra (tamil e inglés)"), S("mesa", 0f, 0f, 2f, 1f, "Pupitres"), S("estante", -5f, -3f, 0.8f, 1.6f, "Cuadernos")),
        }, new[]
        {
            new Lot("Casa de Ikkarai Boluvampatti", "Casa", "Casa de la aldea (encalada, con teja)", 10f, 9f, TamilTile),
            new Lot("Tienda de la aldea", "Mercado", "Tienda de la aldea (arroz, aceite, té)", 10f, 8f, TamilTile, S("puesto", 0f, 0f, 2f, 1f, "Mostrador con sacos de arroz"), S("estante", 0f, 0f, 0.8f, 1.6f, "Tarros y paquetes")),
            new Lot("Herrería de la aldea", "Taller", "Herrero (azadas y rejas de arado)", 10f, 8f, TamilTile, S("banco_taller", 0f, 0f, 2f, 0.9f, "Yunque"), S("horno", 0f, 0f, 1.4f, 1.4f, "Fragua"), S("armario", 0f, 0f, 1f, 0.6f, "Herramientas")),
            new Lot("Cocotal", "Huerto", "Cocotal y platanar", 12f, 10f, TamilTile, S("bancal", 0f, 0f, 4f, 6f, "Plátanos y verduras"), S("pozo", 0f, 0f, 1f, 1f, "Pozo")),
        }, C(0.62f, 0.45f, 0.34f));
        int n = 0;
        n += W(t, "Instructora de hatha yoga", s[1], "", s[2], s[1], s[4]);
        n += W(t, "Instructor de meditación", s[0], "", s[2], s[1], s[4]);
        n += W(t, "Cocinero del ashram", s[2], "cocinero", s[2], s[1], s[4]);
        n += W(t, "Hortelana del ashram", s[5], "hortelano", s[2], s[1], s[4]);
        n += W(t, "Maestra de la aldea", s[6], "sabio", s[2], null, null);
        n += Townsfolk(t, s, 7, "Vecina de Ikkarai Boluvampatti", "Vecino de Ikkarai Boluvampatti", s[2], s[0]);
        return n;
    }
}
