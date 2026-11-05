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
            case "Vega Baja": return VegaBajaTown(t);
            case "Montreal": return MontrealTown(t);
            case "Gary": return GaryTown(t);
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
        // VEGA BAJA (2016): el centro con la plaza José Francisco Náter y la PR-2 y la PR-22 de este a oeste, la playa de Puerto Nuevo y el
        // Atlántico al norte, la Laguna Tortuguero al noreste, el río Cibuco al oeste y, al sur, los mogotes de Almirante Sur por la PR-155.
        // Plano ACORTADO al sur y al norte (la costa está a ~4 km del centro) y aproximado; la situación del Econo no está verificada (OpenStreetMap).
        ["Vega Baja"] = @"
mar Océano Atlántico | 2100 130 | 0 735
lago Laguna Tortuguero | 170 70 | 790 520
rio Río Cibuco | 30 | -760 -760, -640 -200, -700 200, -620 660
colina Mogote de Almirante Sur | 150 70 | -240 -600
colina Mogote de la PR-155 | 120 60 | 330 -620
colina Mogote del oeste | 110 55 | -880 -560
campo Pastos del llano del norte | 320 110 | 300 470
calle Carretera PR-2 | 18 | -1000 -60, 1000 -60
calle Expreso PR-22 | 24 | -1000 -300, 1000 -280
calle PR-155 (a Almirante Sur) | 10 | 60 -60, 40 -720
calle Calle Betances | 8 | -420 120, 420 120
calle Calle de la plaza | 8 | -240 -60, -240 300
calle Calle Tulio Otero | 8 | 240 -60, 240 300
calle Carretera a Puerto Nuevo | 10 | 0 -60, 0 640
calle Camino de Almirante Sur | 6 | -440 -480, 480 -480
hito 0 | 90 110
hito 1 | -280 190
hito 2 | -230 -540
hito 3 | -130 -520
hito 4 | 200 -130
hito 5 | 120 -180
hito 6 | 320 170
hito 7 | -420 -130
hito 8 | -60 -380
Plaza | -130 210
Mercado | 380 40
Parque | -520 70
Piscina | 470 230 | actual: playa de Puerto Nuevo (Vega Baja), posición aproximada
Entrenamiento | -330 -170 | actual: parque atlético de Vega Baja, posición aproximada
Guarderia | 160 240
Estudio | -420 230
Bomberos | 560 -160
Policia | -540 -130
Museo | -60 280
Hotel | 150 560
Taller | 640 60
Carpinteria | -680 -30
Monumento | -180 130
relleno | 55 | 0 90",
        // MONTREAL (1969): el Queen Elizabeth Hotel en el bulevar Dorchester (hoy René-Lévesque), sobre la Gare Centrale; Place Ville Marie al
        // norte, Square Dominion y la catedral al oeste, la rue Sainte-Catherine, McGill y la Place des Arts al noreste, el Mont Royal al noroeste
        // y el San Lorenzo al sureste. Plano aproximado y ACORTADO (OpenStreetMap; Ville de Montréal).
        ["Montreal"] = @"
rio Río San Lorenzo | 240 | 1000 -300, 620 -560, 120 -760
monte Mont Royal | 430 130 | -760 560
colina Falda del Mont Royal | 200 40 | -420 620
campo Parque de la Montaña | 300 170 | -620 620
calle Boulevard Dorchester | 24 | -1000 -30, 1000 -30
calle Rue Sainte-Catherine | 14 | -1000 230, 1000 230
calle Rue Sherbrooke | 18 | -1000 470, 1000 470
calle Rue University | 16 | 90 -520, 90 640
calle Rue Peel | 12 | -420 -520, -420 520
calle Rue Saint-Denis | 12 | 800 -420, 800 700
calle Rue Saint-Jacques | 12 | -700 -320, 900 -320
calle Rue Sainte-Marguerite (casas) | 8 | -860 -520, -860 180
hito 0 | 60 40
hito 1 | -90 -80
hito 2 | 220 90
hito 3 | -140 130
hito 4 | 170 370
hito 5 | 620 340
hito 6 | -320 340
hito 7 | -560 390
hito 8 | -640 -210
hito 9 | 330 -250
Plaza | -300 80
Mercado | -660 150
Parque | 480 520
Piscina | -760 -60 | actual: bains publics de Montréal, posición aproximada
Guarderia | 280 -170
Estudio | 470 90
Museo | -290 420
Estacion | -130 -240
Bomberos | 520 -160
Policia | 620 60
Mecanica | -200 -420
Tecnologia | 640 -60
Monumento | -240 -110
relleno | 55 | 0 110",
        // GARY (1966): la cuadrícula de calles con nombres de presidentes, la Avenida 23 y Jackson Street con la casa de la familia, el instituto
        // Theodore Roosevelt en la Avenida 25, Broadway al este, Mr. Lucky's en Grant Street, y al norte el Gran Calumet, la acería de la US Steel y el lago
        // Míchigan. Plano ACORTADO al norte (la acería está a ~3 km) y aproximado (OpenStreetMap; WBEZ 2017).
        ["Gary"] = @"
mar Lago Míchigan | 2100 120 | 0 740
campo Patio de la acería (US Steel, Gary Works) | 1500 90 | 0 610
rio Gran Calumet | 30 | -1000 520, 1000 520
rio Pequeño Calumet | 26 | -1000 -700, 1000 -660
colina Dunas del lago | 180 16 | 840 640
calle Avenida 23 | 10 | -1000 0, 1000 0
calle Avenida 25 | 10 | -1000 -150, 1000 -150
calle Avenida 21 | 10 | -1000 160, 1000 160
calle Avenida 5 (la Lincoln Highway) | 16 | -1000 430, 1000 430
calle Jackson Street | 8 | 0 -520, 0 400
calle Grant Street | 12 | -520 -520, -520 430
calle Broadway | 20 | 620 -640, 620 470
calle Avenida 35 | 12 | -1000 -500, 1000 -500
calle Harrison Street | 8 | 330 -520, 330 400
hito 0 | -60 60
hito 1 | -140 -220
hito 2 | -620 90
hito 3 | 240 390
hito 4 | 700 230
hito 5 | 700 -70
hito 6 | 120 -290
hito 7 | 420 80
hito 8 | -300 280
Plaza | 760 100
Mercado | 540 -230
Parque | -300 -380
Piscina | 170 -420 | actual: piscina del parque Gleason (Gary), posición aproximada
Guarderia | -210 120
Estudio | -700 -240
Estacion | 760 370
Bomberos | 430 -340
Policia | 730 -320
Museo | 840 -100
Mecanica | -760 300
Taller | -320 390
Carpinteria | -860 -60
Monumento | 760 170
relleno | 60 | 0 60",
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

    // ══ VEGA BAJA (2016): el Econo, la iglesia y la casa de Almirante Sur, la escuela donde enseñaba su madre, una barbería, un chinchorro, la
    //    cancha y el hospital; casas de bloque con marquesina, colmados y talleres.
    static int VegaBajaTown(Transform t)
    {
        var s = Town(t, "Vega Baja", new[]
        {
            new Lot("Econo Plaza Vega Baja", "Mercado", "Econo Plaza Vega Baja (supermercado)", 20f, 14f, CaribeBlock,
                S("puesto", -6f, 3f, 2.4f, 1.2f, "Caja 4 con su cinta"), S("puesto", 0f, 3f, 2.4f, 1.2f, "Frutas: guineos, plátanos, recao"), S("estante", 6f, 3f, 1.2f, 2f, "Góndola de arroz y habichuelas"), S("armario", 0f, -4f, 1.6f, 0.6f, "Bolsas y cajas del empaque")),
            new Lot("Iglesia del centro", "Templo", "Iglesia de Vega Baja", 14f, 10f, CaribeBlock, S("altar", 0f, 2f, 2f, 1f, "Altar"), S("banco", 0f, -1f, 2f, 0.6f, "Banco del coro"), S("piano", 3f, -2f, 1.6f, 1f, "Piano del coro")),
            new Lot("Casa de la familia Martínez Ocasio", "Casa", "Casa de la familia, en Almirante Sur", 12f, 10f, CaribeBlock,
                S("cama", -3f, 2f, 1.4f, 2f, "Cama de los padres"), S("cama", 2f, 2f, 1.2f, 2f, "Cama de Benito"), S("mesa_mezclas", 4f, -2f, 1.6f, 0.7f, "Mesa del cuarto: el portátil y el micrófono")),
            new Lot("Escuela de Almirante Sur", "Escuela", "Escuela del barrio Almirante Sur", 16f, 12f, CaribeBlock,
                S("pizarra", 0f, 3.5f, 2.4f, 0.4f, "Pizarra"), S("mesa", 0f, 0f, 2.4f, 1f, "Pupitres"), S("estante", -5f, -3f, 0.8f, 1.6f, "Libros")),
            new Lot("Chinchorro de la PR-155", "Restaurante", "Chinchorro de la PR-155 (mofongo y pinchos)", 14f, 10f, CaribeBlock,
                S("fogon", -3f, 2f, 1.6f, 0.8f, "Caldero y pilón"), S("mesa", 2f, -1f, 2f, 1f, "Mesa con bancos"), S("barra", 3f, 3f, 2f, 0.8f, "Barra del chinchorro")),
            new Lot("Barbería", "Taller", "Barbería del centro (aquí suena todo lo nuevo)", 12f, 10f, CaribeBlock,
                S("banco_taller", 0f, 2f, 2f, 0.9f, "Sillón y tocador"), S("armario", 3f, -3f, 1.2f, 0.6f, "Tijeras y máquinas")),
            new Lot("Estudio de la marquesina", "Centro de tecnología", "Estudio casero de la marquesina (productores del barrio)", 14f, 10f, CaribeBlock,
                S("consola", -3f, 2f, 1.6f, 0.8f, "Ordenador del productor"), S("mesa_mezclas", 3f, 2f, 1.6f, 0.7f, "Interfaz y altavoces"), S("microfono", 0f, -3f, 0.6f, 0.6f, "Micrófono de voz")),
            new Lot("Hospital de Vega Baja", "Hospital", "Hospital de Vega Baja", 16f, 12f, CaribeBlock,
                S("camilla", -3f, 2f, 1f, 2f, "Camilla"), S("cama", 3f, 2f, 1.2f, 2f, "Cama"), S("armario", 0f, -3f, 1.2f, 0.6f, "Medicinas")),
            new Lot("Casa de doña Carmen", "Casa", "Casa de doña Carmen", 12f, 10f, CaribeBlock, S("cama", 0f, 2f, 1.4f, 2f, "Cama")),
        }, new[]
        {
            new Lot("Casa de Vega Baja", "Casa", "Casa de bloque con marquesina", 10f, 9f, CaribeBlock),
            new Lot("Colmado", "Mercado", "Colmado del barrio", 10f, 8f, CaribeBlock, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
            new Lot("Taller de gomas", "Taller", "Gomera y taller de carros", 12f, 10f, CaribeBlock, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco"), S("armario", 0f, 0f, 1.2f, 0.6f, "Herramientas")),
            new Lot("Lechonera", "Restaurante", "Lechonera", 10f, 8f, CaribeBlock, S("fogon", 0f, 0f, 1.6f, 0.8f, "Asador"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa con bancos")),
        }, C(0.55f, 0.54f, 0.52f));
        int n = 0;
        n += W(t, "Cajera del Econo", s[0], "comerciante", s[4], null, null);
        n += W(t, "Empacador del Econo", s[0], "comerciante", s[4], null, null);
        n += W(t, "Pastora del coro", s[1], "", s[4], null, null);
        n += W(t, "Maestra de Almirante Sur", s[3], "sabio", s[4], null, null);
        n += W(t, "Cocinero del chinchorro", s[4], "cocinero", s[4], null, null);
        n += W(t, "Barbero del centro", s[5], "", s[4], null, null);
        n += W(t, "Productor del barrio", s[6], "", s[4], null, null);
        n += W(t, "Enfermera de Vega Baja", s[7], "enfermera", s[4], null, null);
        n += Townsfolk(t, s, 9, "Vecina de Vega Baja", "Vecino de Vega Baja", s[4], null);
        return n;
    }

    // ══ MONTREAL (1969): el Queen Elizabeth, el Beaver Club, el estudio de André Perry, la Y de la rue Drummond, McGill, la Place des Arts, el Hospital
    //    Royal Victoria y dúplex de ladrillo con su escalera (sin subir: en terraza); tiendas de Sainte-Catherine y talleres.
    static int MontrealTown(Transform t)
    {
        var s = Town(t, "Montreal", new[]
        {
            new Lot("Queen Elizabeth Hotel", "Hotel", "Queen Elizabeth Hotel (habitaciones 1738–1744)", 20f, 14f, MontrealBrick,
                S("cama", -5f, 3f, 2f, 2.2f, "Cama de la habitación 1742"), S("cama", 0f, 3f, 1.6f, 2f, "Cama de la 1740"), S("mesa", 5f, 3f, 1.6f, 1f, "Mesa de los teléfonos"), S("microfono", 0f, -4f, 0.6f, 0.6f, "Micrófono de la grabación")),
            new Lot("Gare Centrale", "Estación", "Gare Centrale (bajo el hotel)", 16f, 10f, MontrealBrick, S("anden", 0f, 0f, 8f, 3f, "Andén"), S("puesto", 0f, 0f, 2f, 1f, "Taquilla")),
            new Lot("Estudio de André Perry", "Centro de tecnología", "Estudio de grabación de André Perry", 14f, 10f, MontrealBrick,
                S("consola", -3f, 2f, 1.6f, 0.8f, "Mesa del técnico"), S("mesa_mezclas", 3f, 2f, 1.6f, 0.7f, "Magnetófono de cuatro pistas"), S("microfono", 0f, -3f, 0.6f, 0.6f, "Micrófono"), S("piano", -4f, -3f, 1.6f, 1f, "Piano del estudio")),
            new Lot("Beaver Club", "Restaurante", "Beaver Club (el restaurante del hotel)", 14f, 10f, MontrealBrick,
                S("cocina", -3f, 2f, 2f, 0.8f, "Cocina"), S("mesa", 2f, -1f, 1.6f, 1f, "Mesa con mantel"), S("barra", 3f, 3f, 2f, 0.8f, "Barra")),
            new Lot("Universidad McGill", "Escuela", "Universidad McGill", 18f, 12f, MontrealBrick,
                S("pizarra", 0f, 4f, 2.4f, 0.4f, "Pizarra"), S("mesa", 0f, 0f, 2.4f, 1f, "Mesa de estudio"), S("estante", -6f, -3f, 1.2f, 2f, "Biblioteca")),
            new Lot("Place des Arts", "Sala de baile", "Place des Arts (conciertos y danza)", 18f, 14f, MontrealBrick,
                S("pista", 0f, 0f, 6f, 6f, "Escenario y pista"), S("tarima", 0f, 0f, 6f, 4f, "Tarima de la orquesta"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa de los músicos"), S("piano", 0f, 0f, 1.6f, 1f, "Piano de cola")),
            new Lot("YMCA de la rue Drummond", "Centro de entrenamiento", "YMCA de la rue Drummond", 16f, 14f, MontrealBrick, TrainingKit("gimnasio_moderno")),
            new Lot("Hospital Royal Victoria", "Hospital", "Hospital Royal Victoria", 18f, 12f, MontrealBrick,
                S("camilla", -4f, 2f, 1f, 2f, "Camilla"), S("cama", 0f, 2f, 1.2f, 2f, "Cama"), S("cama", 4f, 2f, 1.2f, 2f, "Cama"), S("armario", 0f, -3f, 1.2f, 0.6f, "Medicinas")),
            new Lot("Dúplex de Sainte-Marguerite", "Casa", "Dúplex de ladrillo de la rue Sainte-Marguerite", 12f, 10f, MontrealBrick, S("cama", -2f, 2f, 1.4f, 2f, "Cama"), S("cama", 3f, 2f, 1.2f, 2f, "Cama")),
            new Lot("Apartamento de la periodista", "Casa", "Apartamento de la periodista", 12f, 10f, MontrealBrick, S("cama", 0f, 2f, 1.4f, 2f, "Cama"), S("estante", 3f, 2f, 0.8f, 1.6f, "Periódicos")),
        }, new[]
        {
            new Lot("Dúplex de Montreal", "Casa", "Dúplex de ladrillo de Montreal", 10f, 9f, MontrealBrick),
            new Lot("Tienda de Sainte-Catherine", "Mercado", "Tienda de la rue Sainte-Catherine", 10f, 8f, MontrealBrick, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
            new Lot("Disquería", "Mercado", "Tienda de discos", 10f, 8f, MontrealBrick, S("puesto", 0f, 0f, 2f, 1f, "Mostrador de discos"), S("estante", 0f, 0f, 0.8f, 1.6f, "Discos")),
            new Lot("Taller de Griffintown", "Taller", "Taller de Griffintown", 12f, 10f, MontrealBrick, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco"), S("armario", 0f, 0f, 1.2f, 0.6f, "Herramientas")),
        }, C(0.5f, 0.49f, 0.48f));
        int n = 0;
        n += W(t, "Recepcionista del hotel", s[0], "", s[3], s[6], s[8]);
        n += W(t, "Camarera de piso", s[0], "", s[3], s[6], s[8]);
        n += W(t, "Jefe de estación", s[1], "", s[3], s[6], null);
        n += W(t, "Ayudante de André Perry", s[2], "", s[3], s[6], null);
        n += W(t, "Cocinero del Beaver Club", s[3], "cocinero", s[3], s[6], null);
        n += W(t, "Profesora de McGill", s[4], "sabio", s[3], s[6], null);
        n += W(t, "Monitor de la YMCA", s[6], "maestro", s[3], s[6], null);
        n += W(t, "Enfermera del Royal Victoria", s[7], "enfermera", s[3], s[6], null);
        n += Townsfolk(t, s, 10, "Vecina de Montreal", "Vecino de Montreal", s[3], s[6]);
        return n;
    }

    // ══ GARY (1966): la casa de Jackson Street, Roosevelt High School, Mr. Lucky's, la puerta de la acería, Steeltown Records, el Sears de Broadway, el
    //    gimnasio de boxeo, el Mercy y el Salón del Reino; casas de tablas de los obreros, tiendas y talleres.
    static int GaryTown(Transform t)
    {
        var s = Town(t, "Gary", new[]
        {
            new Lot("Casa de 2300 Jackson Street", "Casa", "Casa de los Jackson (2300 Jackson Street)", 12f, 10f, SteelFrame,
                S("cama", -4f, 2f, 1.4f, 2f, "Cama de Joe y Katherine"), S("cama", 0f, 2f, 1.2f, 2f, "Literas de los hermanos"), S("cama", 4f, 2f, 1.2f, 2f, "Sofá-cama de las hermanas"), S("amplificador", 0f, -3f, 1f, 0.5f, "Amplificador y guitarra de la sala")),
            new Lot("Theodore Roosevelt High School", "Escuela", "Theodore Roosevelt High School", 20f, 14f, SteelFrame,
                S("pizarra", -5f, 4f, 2.4f, 0.4f, "Pizarra"), S("mesa", 0f, 4f, 2.4f, 1f, "Pupitres"), S("tarima", 0f, -3f, 6f, 4f, "Escenario del salón de actos"), S("estante", 6f, 4f, 1f, 2f, "Trofeos y libros")),
            new Lot("Mr. Lucky's", "Sala de baile", "Mr. Lucky's (sala de fiestas de Grant Street)", 14f, 12f, SteelFrame,
                S("pista", 0f, 0f, 6f, 6f, "Pista"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa con mantel"), S("bateria", 0f, 0f, 1.6f, 1.6f, "Batería del grupo")),
            new Lot("Puerta de la acería", "Taller", "US Steel, Gary Works (la puerta y el taller de grúas)", 20f, 14f, SteelFrame,
                S("banco_taller", -5f, 3f, 2.4f, 1.2f, "Banco de grúas"), S("torno", 0f, 3f, 1.6f, 1f, "Torno"), S("armario", 5f, 3f, 1.6f, 0.8f, "Cascos y fiambreras")),
            new Lot("Steeltown Records", "Centro de tecnología", "Steeltown Records (Gordon Keith)", 14f, 10f, SteelFrame,
                S("consola", -3f, 2f, 1.6f, 0.8f, "Mesa del productor"), S("mesa_mezclas", 3f, 2f, 1.6f, 0.7f, "Magnetófono"), S("microfono", 0f, -3f, 0.6f, 0.6f, "Micrófono"), S("piano", -4f, -3f, 1.6f, 1f, "Piano")),
            new Lot("Sears de Broadway", "Mercado", "Sears de Broadway (Katherine trabaja aquí a tiempo parcial)", 16f, 12f, SteelFrame,
                S("puesto", -4f, 2f, 2.4f, 1.2f, "Mostrador de ropa"), S("puesto", 2f, 2f, 2.4f, 1.2f, "Mostrador del hogar"), S("estante", 0f, -3f, 1.2f, 2f, "Estantes")),
            new Lot("Gimnasio de boxeo", "Centro de entrenamiento", "Gimnasio de boxeo de los obreros (Joe fue boxeador)", 16f, 14f, SteelFrame, TrainingKit("gimnasio_moderno")),
            new Lot("St. Mary Mercy Hospital", "Hospital", "St. Mary Mercy Hospital (aquí nació Michael)", 16f, 12f, SteelFrame,
                S("camilla", -3f, 2f, 1f, 2f, "Camilla"), S("cama", 3f, 2f, 1.2f, 2f, "Cama"), S("cuna", 0f, -3f, 0.8f, 1.3f, "Cuna"), S("armario", 5f, -3f, 1.2f, 0.6f, "Medicinas")),
            new Lot("Salón del Reino", "Templo", "Salón del Reino de los Testigos de Jehová", 12f, 10f, SteelFrame, S("altar", 0f, 2f, 2f, 1f, "Atril"), S("banco", 0f, -1f, 2f, 0.6f, "Banco")),
        }, new[]
        {
            new Lot("Casa de Gary", "Casa", "Casa de tablas de una familia de la acería", 10f, 9f, SteelFrame),
            new Lot("Tienda de la Avenida 25", "Mercado", "Tienda de la esquina", 10f, 8f, SteelFrame, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
            new Lot("Taller de coches", "Taller", "Taller de coches", 12f, 10f, SteelFrame, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco"), S("armario", 0f, 0f, 1.2f, 0.6f, "Recambios")),
            new Lot("Soul food", "Restaurante", "Comedor de soul food", 12f, 9f, SteelFrame, S("fogon", 0f, 0f, 1.6f, 0.8f, "Cocina"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa con bancos")),
        }, C(0.4f, 0.39f, 0.38f));
        // El comedor del pueblo: el primer «Soul food» del relleno.
        AreaFunctionSite food = s.Find(x => x != null && x.building == "Restaurante") ?? s[2];
        int n = 0;
        n += W(t, "Profesor de Roosevelt High School", s[1], "sabio", food, s[6], null);
        n += W(t, "Dueño de Mr. Lucky's", s[2], "", food, s[6], null);
        n += W(t, "Gruista de la acería", s[3], "", food, s[6], null);
        n += W(t, "Fundidor de la acería", s[3], "", food, s[6], null);
        n += W(t, "Productor de Steeltown (Gordon Keith)", s[4], "", food, s[6], null);
        n += W(t, "Dependienta de Sears", s[5], "comerciante", food, s[6], null);
        n += W(t, "Entrenador de boxeo", s[6], "maestro", food, s[6], null);
        n += W(t, "Enfermera del Mercy", s[7], "enfermera", food, s[6], null);
        n += Townsfolk(t, s, 9, "Vecina de Gary", "Vecino de Gary", food, s[6]);
        return n;
    }

    // ══FIN DE LOS PUEBLOS══
}
