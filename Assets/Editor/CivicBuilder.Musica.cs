using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LOS PUEBLOS DE LA MÚSICA (docs/microcosmos-musica.md; Berón 2026-11-05). Cada historia en su pueblo real, sobre su PLANO
/// (<see cref="MusicPlans"/>, formato de <c>PlanText</c>; caben a 1:1 en el barrio de los pisos de 36 × 26 m, ±1000 × ±720 m), con
/// su estilo de época y lugar (MicroStyles: caribe_hispano, jamaica_moderna, corea_moderna, britanico_moderno, norteamerica_norte…), las
/// cuatro categorías (trabajo, ocio, entrenamiento y recuperación), un hogar para cada personaje (Homes) y su frutal en cada residencia.
/// La flora, la fauna y el orden de los muebles del piso: MicroStoryMaps.Musica. Sin muros ni techos.
/// </summary>
internal static partial class CivicBuilder
{
    static readonly Style CaribeBlock = new Style(C(0.92f, 0.86f, 0.72f), C(0.72f, 0.66f, 0.56f), C(0.75f, 0.42f, 0.28f), Roof.Flat, 3f);       // Caribe: bloque encalado de colores y patio de cemento
    static readonly Style SchoolBlock = new Style(C(0.95f, 0.93f, 0.85f), C(0.78f, 0.74f, 0.66f), C(0.25f, 0.5f, 0.6f), Roof.Flat, 3.4f);     // el colegio: blanco con pasillos abiertos
    static readonly Style MontrealBrick = new Style(C(0.6f, 0.36f, 0.28f), C(0.6f, 0.58f, 0.54f), C(0.3f, 0.3f, 0.32f), Roof.Flat, 3.4f);     // Montreal: ladrillo y piedra gris
    static readonly Style SteelFrame = new Style(C(0.55f, 0.42f, 0.34f), C(0.5f, 0.48f, 0.46f), C(0.3f, 0.3f, 0.32f), Roof.Flat, 3.2f);      // Gary: ladrillo oscuro y tablas, hollín de acería
    static readonly Style SeoulConcrete = new Style(C(0.82f, 0.82f, 0.8f), C(0.7f, 0.7f, 0.7f), C(0.3f, 0.32f, 0.35f), Roof.Flat, 3.2f);     // Seúl: hormigón claro y vidrio
    static readonly Style YardBlock = new Style(C(0.86f, 0.8f, 0.62f), C(0.55f, 0.5f, 0.42f), C(0.55f, 0.58f, 0.58f), Roof.Flat, 3f);        // Trench Town: bloque encalado y zinc alrededor del patio
    static readonly Style BelfastBrick = new Style(C(0.58f, 0.3f, 0.24f), C(0.5f, 0.46f, 0.42f), C(0.3f, 0.3f, 0.32f), Roof.Gable, 3.2f);     // Belfast: ladrillo rojo y pizarra
    static readonly Style SuburbFrame = new Style(C(0.85f, 0.82f, 0.74f), C(0.6f, 0.52f, 0.42f), C(0.42f, 0.4f, 0.38f), Roof.Gable, 3f);     // Long Island: casas de tablas y garaje

    /// <summary>El pueblo de cada historia de la música (0 si <paramref name="display"/> no es una de ellas).</summary>
    static int MusicTown(Transform t, string display)
    {
        switch (display)
        {
            case "Barranquilla": return BarranquillaTown(t);
            // ══FIN DE LOS CASOS══
            default: return 0;
        }
    }

    // ══ LOS PLANOS (metros reales, claro de la historia en (0, 0), x = este, z = norte; fuentes en el doc) ══════════════════════════
    static readonly Dictionary<string, string> MusicPlans = new Dictionary<string, string>
    {
        // LA PLAYA (Barranquilla, 2009): el corregimiento entre la Ciénaga de Mallorquín (al norte, y detrás el Caribe) y la vía que va a
        // Barranquilla; el río Magdalena al este, las lomas de Puerto Colombia al suroeste. Plano aproximado y ACORTADO al este (el río
        // queda a varios km; aquí en el borde): los nombres de las calles interiores no están verificados (OpenStreetMap; Fundación Pies Descalzos).
        ["Barranquilla"] = @"
mar Mar Caribe | 2100 150 | 0 730
lago Ciénaga de Mallorquín | 620 110 | -120 500
rio Río Magdalena | 110 | 960 760, 900 300, 980 -200, 1000 -760
colina Lomas de Puerto Colombia | 220 45 | -820 -560 | seco
campo Solares de La Playa | 260 160 | 620 -520
calle Vía a La Playa | 12 | -1000 -220, 0 -160, 1000 -240
calle Calle principal de La Playa | 8 | 0 -700, 0 330
calle Calle del colegio | 6 | -460 60, 460 60
calle Calle de los pescadores | 6 | -320 330, 420 330
calle Calle del arroyo | 6 | -560 -620, -420 -160, -420 330
calle Calle del sur | 6 | -600 -480, 620 -480
calle Vía a Barranquilla | 10 | 620 -720, 620 -160
hito 0 | 0 130
hito 1 | 140 170
hito 2 | -140 160
hito 3 | 200 -70
hito 4 | -170 -80
hito 5 | 310 190
hito 6 | -300 190
hito 7 | 330 -100
hito 8 | 110 280
Plaza | -60 -280
Mercado | 170 -290
Parque | -270 -350
Piscina | 430 -330 | actual: balneario de Puerto Mocho (Barranquilla), posición aproximada
Entrenamiento | 300 -420 | actual: canchas del corregimiento de La Playa, posición aproximada
Guarderia | 120 -420
Estudio | -240 -500
Policia | 470 170
Hotel | -610 -300
Taller | -560 90
Carpinteria | 520 -40
Monumento | -120 -190
relleno | 50 | 0 -250",
        // ══FIN DE LOS PLANOS══
    };

    // ══ LA PLAYA (Barranquilla, 2009): el colegio Pies Descalzos, su comedor, el centro comunitario, la cancha, la sala de danza, el taller de
    //    las familias, el puesto de salud y las casas de los pescadores; casas de bloque de colores, tiendas y talleres.
    static int BarranquillaTown(Transform t)
    {
        var s = Town(t, "Barranquilla", new[]
        {
            new Lot("Colegio Pies Descalzos de La Playa", "Escuela", "Colegio Pies Descalzos de La Playa (2009)", 20f, 14f, SchoolBlock,
                S("pizarra", -5f, 3f, 2.4f, 0.4f, "Pizarra de primero"), S("mesa", 0f, 3f, 2.4f, 1f, "Pupitres"), S("pizarra", 5f, 3f, 2.4f, 0.4f, "Pizarra de quinto"), S("estante", 0f, -4f, 1.6f, 0.6f, "Biblioteca del aula")),
            new Lot("Comedor escolar", "Restaurante", "Comedor escolar (almuerzo para todos)", 16f, 12f, SchoolBlock,
                S("cocina", -4f, 3f, 2.4f, 0.8f, "Ollas del arroz con pescado"), S("mesa", 0f, -1f, 2.4f, 1f, "Mesas largas"), S("barra", 4f, 3f, 2.4f, 0.8f, "Barra de servir")),
            new Lot("Centro comunitario", "Taller de papeles", "Centro comunitario y censo educativo", 14f, 10f, SchoolBlock,
                S("mesa", 0f, 2f, 2.4f, 1f, "Mesa del censo"), S("armario", 3f, -3f, 1.2f, 0.6f, "Fichas de las familias")),
            new Lot("Cancha del colegio", "Centro de entrenamiento", "Cancha del colegio (fútbol y educación física)", 16f, 14f, SchoolBlock, TrainingKit("estadio")),
            new Lot("Sala de danza del colegio", "Sala de baile", "Sala de danza del colegio (cumbia y mapalé)", 14f, 12f, SchoolBlock,
                S("pista", 0f, 0f, 6f, 6f, "Patio de baile"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa de los tambores y las faldas"), S("bateria", 0f, 0f, 1.6f, 1.6f, "Tambores del grupo de danza")),
            new Lot("Taller de las familias", "Taller", "Taller de las familias (costura y oficios para ganarse la vida)", 14f, 10f, CaribeBlock,
                S("banco_taller", -3f, 2f, 2f, 0.9f, "Mesa de corte"), S("telar", 3f, 2f, 1.6f, 1f, "Máquina de coser"), S("armario", 0f, -3f, 1.2f, 0.6f, "Telas e hilos")),
            new Lot("Casa del rector", "Casa", "Casa del rector del colegio", 12f, 10f, CaribeBlock, S("cama", -2f, 2f, 1.4f, 2f, "Cama"), S("estante", 3f, 2f, 0.8f, 1.6f, "Libros")),
            new Lot("Puesto de salud de La Playa", "Hospital", "Puesto de salud de La Playa", 14f, 10f, CaribeBlock,
                S("camilla", -3f, 2f, 1f, 2f, "Camilla"), S("cama", 3f, 2f, 1.2f, 2f, "Cama de observación"), S("armario", 0f, -3f, 1.2f, 0.6f, "Botiquín y vacunas")),
            new Lot("Casa de la vecina", "Casa", "Casa de la vecina de La Playa", 12f, 10f, CaribeBlock, S("cama", -3f, 2f, 1.2f, 2f, "Cama"), S("cama", 2f, 2f, 1.2f, 2f, "Cama de los niños")),
        }, new[]
        {
            new Lot("Casa de La Playa", "Casa", "Casa de bloque de La Playa", 10f, 9f, CaribeBlock),
            new Lot("Tienda de la esquina", "Mercado", "Tienda de la esquina (arepas, queso costeño)", 10f, 8f, CaribeBlock, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
            new Lot("Taller de redes", "Taller", "Taller de redes y lanchas de los pescadores", 12f, 10f, CaribeBlock, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco de remendar redes"), S("armario", 0f, 0f, 1.2f, 0.6f, "Anzuelos y remos")),
            new Lot("Fritanga", "Restaurante", "Fritanga de la Vía a La Playa (arepa de huevo)", 10f, 8f, CaribeBlock, S("fogon", 0f, 0f, 1.6f, 0.8f, "Caldero de la fritanga"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa con bancos")),
        }, C(0.72f, 0.66f, 0.56f));
        int n = 0;
        n += W(t, "Maestra de primero", s[0], "sabio", s[1], s[3], s[6]);
        n += W(t, "Maestro de quinto", s[0], "sabio", s[1], s[3], s[6]);
        n += W(t, "Cocinera del comedor escolar", s[1], "cocinero", s[1], s[3], s[8]);
        n += W(t, "Trabajadora social", s[2], "", s[1], s[3], s[8]);
        n += W(t, "Profesor de educación física", s[3], "maestro", s[1], s[3], null);
        n += W(t, "Costurera del taller", s[5], "", s[1], s[3], null);
        n += W(t, "Enfermera del puesto de salud", s[7], "enfermera", s[1], s[3], null);
        n += Townsfolk(t, s, 9, "Vecina de La Playa", "Vecino de La Playa", s[1], s[3]);
        return n;
    }

    // ══FIN DE LOS PUEBLOS══
}
