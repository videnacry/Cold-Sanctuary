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
    static readonly Style FactorySteel = new Style(C(0.85f, 0.86f, 0.88f), C(0.82f, 0.83f, 0.85f), C(0.6f, 0.62f, 0.65f), Roof.None, 6f);  // la nave de SpaceX: chapa blanca y suelo de epoxi
    static readonly Style MountainTimber = new Style(C(0.72f, 0.66f, 0.56f), C(0.55f, 0.5f, 0.44f), C(0.35f, 0.36f, 0.38f), Roof.Gable, 3.2f);  // aldea de montaña: piedra, pino y teja gris
    static readonly Style TamilTile = new Style(C(0.92f, 0.88f, 0.78f), C(0.6f, 0.45f, 0.32f), C(0.7f, 0.32f, 0.22f), Roof.Gable, 3.2f);  // aldea tamil: encalado y teja de Mangalore
    static readonly Style PuebloStucco = new Style(C(0.82f, 0.68f, 0.52f), C(0.62f, 0.55f, 0.48f), C(0.55f, 0.4f, 0.3f), Roof.Flat, 3.2f);   // Albuquerque: estuco color adobe (Pueblo Revival), tejado plano
    static readonly Style OmahaBrick = new Style(C(0.65f, 0.35f, 0.28f), C(0.55f, 0.45f, 0.35f), C(0.35f, 0.33f, 0.32f), Roof.Gable, 3.4f);   // Omaha: ladrillo rojo y tablas, suelo de madera
    static readonly Style CampusBrick = new Style(C(0.72f, 0.5f, 0.38f), C(0.75f, 0.72f, 0.66f), C(0.45f, 0.12f, 0.2f), Roof.Flat, 3.6f);   // Prairie View: ladrillo del campus, granate y oro
    static readonly Style SahelBanco = new Style(C(0.72f, 0.5f, 0.34f), C(0.62f, 0.45f, 0.32f), C(0.6f, 0.62f, 0.62f), Roof.Flat, 3f);       // Uagadugú: banco (adobe) enlucido y chapa, patio de tierra

    /// <summary>El pueblo de cada famoso (0 si <paramref name="display"/> no es uno de ellos).</summary>
    static int FamousTown(Transform t, string display)
    {
        switch (display)
        {
            case "Velliangiri": return VelliangiriTown(t);
            case "Hawthorne": return HawthorneTown(t);
            case "Aldea de la montaña": return MountainVillageTown(t);
            case "Albuquerque": return AlbuquerqueTown(t);
            case "Omaha": return OmahaTown(t);
            case "Prairie View": return PrairieViewTown(t);
            case "Uagadugú": return OuagadougouTown(t);
            default: return HistoTown(t, display);   // la tanda 3 (Assets/Editor/Historias/), o 0
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
        // HAWTHORNE (2008): la sede de SpaceX en la antigua nave de fuselajes de Northrop (Rocket Road, junto a Crenshaw Boulevard y
        // la calle 120), el aeropuerto municipal de Hawthorne al oeste de Crenshaw, la autopista 105 al norte, El Segundo Boulevard al
        // sur y la cuadrícula de casas de estuco. Plano aproximado (OpenStreetMap; City of Hawthorne).
        ["Hawthorne"] = @"
calle Crenshaw Boulevard | 30 | -80 -1200, -80 1200
calle West 120th Street | 20 | -1400 460, 1400 460
calle Autopista 105 (Glenn Anderson Freeway) | 40 | -1400 920, 1400 920
calle Rocket Road | 12 | -80 120, 420 120
calle Jack Northrop Avenue | 14 | -80 -320, 1000 -320
calle El Segundo Boulevard | 22 | -1400 -1000, 1400 -1000
calle Prairie Avenue | 20 | -1150 -1200, -1150 1200
calle Calle de las casas norte | 8 | 300 600, 1300 600
calle Calle de las casas sur | 8 | 300 -650, 1300 -650
calle Calle de las casas oeste | 8 | -1100 -650, -300 -650
campo Pista del aeropuerto municipal | 900 80 | -620 150
hito 0 | 60 80
hito 1 | -60 -60
hito 2 | 180 -40
hito 3 | 160 180
hito 4 | 520 600
Plaza | 640 -360
Mercado | 380 -460
Parque | 900 300
Piscina | 1050 -500
Entrenamiento | 700 160
Bomberos | 300 -780
Policia | -500 -750
Estudio | -700 -560
Guarderia | 800 -780
Museo | -500 400
Hotel | 300 780
Mecanica | -300 -300
Tecnologia | 420 -200
Estacion | -900 600
relleno | 60 | 600 -600",
        // ALDEA DE LA MONTAÑA (homenaje a Goku; FICCIÓN): un valle con su torrente que baja de una cascada al norte, el monte al
        // noroeste, bancales de arroz y de té en las laderas, el camino de la cumbre y la calle de la aldea. Inventado (no es un lugar
        // real ni de la obra), al modo de las aldeas de los montes Song y Wudang.
        ["Aldea de la montaña"] = @"
rio Torrente de la cascada | 16 | -260 1000, -180 400, -60 0, 120 -500, 300 -1000
lago Poza de la cascada | 40 30 | -260 980
monte El monte del maestro | 700 1300 | -1300 900 | nieve
colina Colina de los pinos | 200 70 | 700 600
colina Cumbre del amanecer | 160 90 | 650 -450
acantilado Paredón de la cascada | 30 | -420 1050, -260 1100, -100 1050
campo Bancales de arroz | 300 200 | 380 -150
campo Bancales de té | 260 180 | -500 -300
calle Calle de la aldea | 6 | -300 60, 400 60
calle Camino de la cascada | 4 | -40 120, -200 900
calle Camino de la cumbre | 4 | 120 40, 600 -400
calle Callejón del templo | 4 | 150 60, 150 320
hito 0 | 0 150
hito 1 | -110 140
hito 2 | 110 140
hito 3 | -60 -50
hito 4 | 150 300
hito 5 | 330 -60
hito 6 | -220 40
Plaza | 40 -80
Mercado | 260 120
Pileta | -150 -100
Parque | 520 300
Estudio | -300 180
Guarderia | 300 -260
Hotel | -400 -80
Carpinteria | -350 300
Obra | 430 220
relleno | 30 | 0 60",
        // ALBUQUERQUE (1975): el noreste de la ciudad, junto a la Central Avenue (la Ruta 66): MITS en un pequeño centro comercial,
        // San Mateo y Carlisle Boulevard de norte a sur, Lomas al norte, la sierra de Sandía al este y el valle del Río Grande al
        // oeste. Plano aproximado: la dirección exacta de MITS y del motel no está verificada (OpenStreetMap; Allen 2011).
        ["Albuquerque"] = @"
calle Central Avenue (Ruta 66) | 24 | -1400 -220, 1400 -220
calle Lomas Boulevard | 18 | -1400 330, 1400 330
calle San Mateo Boulevard | 20 | 320 -1200, 320 1200
calle Carlisle Boulevard | 16 | -520 -1200, -520 1200
calle Calle de las casas norte | 8 | -500 650, 1200 650
calle Calle de las casas sur | 8 | -500 -650, 1200 -650
calle Calle del centro comercial | 10 | -80 -220, -80 120
monte Sierra de Sandía | 800 1600 | 1800 600 | seco
rio Río Grande | 70 | -1550 -1300, -1480 0, -1560 1300
campo Huertos del valle (chile y albaricoques) | 220 300 | -1250 -500
hito 0 | 0 80
hito 1 | 120 120
hito 2 | 220 -260
hito 3 | -150 -260
hito 4 | 420 520
hito 5 | -260 80
Plaza | 650 -380
Mercado | 820 -260
Parque | -350 560
Piscina | 900 450
Entrenamiento | -700 -420
Bomberos | 600 140
Policia | -300 -480
Estudio | 750 -800
Guarderia | -700 750
Museo | -900 -100
Mecanica | 480 -170
Tecnologia | 150 330
Estacion | -1100 -250
relleno | 60 | 500 -600",
        // OMAHA (1983): la Nebraska Furniture Mart en la calle 72 (al sur de Dodge Street, donde está desde 1970), Dodge Street de este
        // a oeste, Farnam Street (la casa de Buffett y, más al este, Kiewit Plaza), Memorial Park y el campus de la Universidad de
        // Nebraska en Omaha; el río Misuri queda a ~10 km al este (fuera). Plano ACORTADO: las distancias al este se han reducido a
        // un tercio para que quepan la casa y el despacho (OpenStreetMap; Nebraska Furniture Mart; Lowenstein 1995).
        ["Omaha"] = @"
calle Calle 72 (72nd Street) | 20 | 0 -1200, 0 1200
calle Dodge Street | 24 | -1400 800, 1400 800
calle Farnam Street | 12 | -600 560, 1400 560
calle Pacific Street | 16 | -1400 -400, 1400 -400
calle Calle 60 | 12 | 700 -1200, 700 1200
calle Calle 84 | 14 | -900 -1200, -900 1200
calle Calle de las casas | 8 | 200 300, 1300 300
calle Calle de las casas sur | 8 | -800 -800, 600 -800
colina Loma de Memorial Park | 200 25 | 1000 1000
campo Prados del oeste | 300 300 | -1200 -900
hito 0 | 60 80
hito 1 | 1250 600
hito 2 | 950 520
hito 3 | 500 -150
hito 4 | 180 -120
hito 5 | -300 250
Plaza | 450 850
Mercado | -400 -300
Parque | 1000 950
Piscina | -500 -650
Entrenamiento | -600 600
Bomberos | 300 650
Policia | -250 -500
Estudio | 800 1100
Guarderia | -700 300
Museo | 1200 -200
Mecanica | 250 -450
Tecnologia | -350 950
Hotel | -150 850
relleno | 70 | 300 -600",
        // PRAIRIE VIEW (2020): el campus de Prairie View A&M en el condado de Waller (Texas), sobre la antigua finca de Alta Vista: la
        // avenida de la universidad, la administración, la biblioteca John B. Coleman, las aulas de ciencias, las residencias y la
        // granja experimental; la autopista 290 al sur y el pueblo de Prairie View; pinares y robledales al norte. Plano aproximado
        // (OpenStreetMap; PVAMU, «Campus Map»).
        ["Prairie View"] = @"
calle University Drive | 14 | -1200 0, 1200 0
calle Avenida del campus | 10 | 0 -400, 0 600
calle Calle de las residencias | 8 | -500 300, 500 300
calle Autopista 290 | 30 | -1400 -950, 1400 -800
calle FM 1098 | 12 | 900 -950, 900 900
calle Calle del pueblo | 8 | 300 -600, 1200 -600
campo Granja experimental | 300 220 | -800 450
campo Pastos del condado de Waller | 400 300 | 1000 700
colina Loma de los pinos | 260 20 | -400 900
hito 0 | 60 120
hito 1 | -140 140
hito 2 | 160 220
hito 3 | -160 -120
hito 4 | 120 380
hito 5 | 180 -140
hito 6 | -700 420
Plaza | 0 -200
Mercado | 600 -500
Parque | -300 -350
Piscina | 350 150
Entrenamiento | -450 120
Bomberos | 700 -700
Policia | 450 -250
Guarderia | 500 -700
Museo | -250 450
Investigacion | 300 420
Tecnologia | -350 300
Hotel | 800 -400
relleno | 50 | 700 -600",
        // UAGADUGÚ (2024): el centro (el gran mercado Rood Woko, la plaza de la Nación, la estación del ferrocarril de 1954), la avenida
        // Kwame Nkrumah, y al norte los embalses (barrages n.º 1, 2 y 3 de Tanghin) con sus huertos y el parque urbano Bangr-Weoogo.
        // Plano ACORTADO al norte (los barrages están a ~2 km del centro) y aproximado (OpenStreetMap; Ville de Ouagadougou).
        ["Uagadugú"] = @"
lago Barrage n.º 2 de Tanghin | 260 90 | -300 950
lago Barrage n.º 3 de Tanghin | 200 80 | 250 1050
campo Huertos del barrage | 500 120 | -50 780
campo Parque urbano Bangr-Weoogo | 380 260 | 750 850
campo Campos de la periferia | 400 300 | -1100 -900
calle Avenida Kwame Nkrumah | 24 | -900 -700, 800 500
calle Bulevar de Tanghin | 16 | 0 0, 0 700
calle Avenida de la Nación | 18 | -1300 -100, 1300 -100
calle Calle del mercado | 10 | -500 -400, 300 -400
calle Calle de los patios norte | 8 | -900 300, 900 300
calle Calle de los patios sur | 8 | -900 -800, 900 -800
calle Vía del ferrocarril | 8 | -1400 -560, 1400 -500
hito 0 | 80 100
hito 1 | -120 640
hito 2 | -250 -330
hito 3 | 240 200
hito 4 | 140 -260
hito 5 | -200 250
hito 6 | 350 -150
Plaza | -60 -160
Mercado | -500 -250
Parque | 700 700
Estacion | -200 -560
Estudio | 450 350
Guarderia | -450 380
Museo | 600 -300
Piscina | 750 -150
Hotel | 300 -380
Bomberos | -650 -450
Taller | -700 100
Carpinteria | 650 100
Obra | -800 650
Entrenamiento | 300 520 | actual: Stade du 4-Août (Uagadugú); posición aproximada
relleno | 70 | 0 -650",
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

    // ══ HAWTHORNE (2008): la sede de SpaceX (la nave de montaje, el control de misión, el banco del Merlin, la cafetería), un piso de
    //    ingenieros y la ciudad de casas de estuco con su taller, su tienda y su taquería.
    static int HawthorneTown(Transform t)
    {
        var s = Town(t, "Hawthorne", new[]
        {
            new Lot("Nave de montaje de SpaceX", "Taller", "Nave de montaje de SpaceX (Falcon 1)", 20f, 14f, FactorySteel,
                S("banco_taller", -5f, 3f, 2.4f, 1f, "Etapa del Falcon 1 en su cuna"), S("torno", 0f, 3f, 1.6f, 1f, "Torno de control numérico"), S("banco_taller", 5f, 3f, 2.4f, 1f, "Banco de soldar tanques"), S("armario", 0f, -4f, 1.6f, 0.6f, "Herramientas y llaves dinamométricas")),
            new Lot("Control de misión", "Centro de tecnología", "Control de misión de Hawthorne", 16f, 12f, FactorySteel,
                S("consola", -3f, 2f, 2f, 0.8f, "Consola de lanzamiento"), S("consola", 3f, 2f, 2f, 0.8f, "Consola de telemetría"), S("pizarra", 0f, -3f, 2.4f, 0.4f, "Pizarra de la secuencia de vuelo")),
            new Lot("Banco de pruebas del Merlin", "Taller", "Taller de motores Merlin", 14f, 10f, FactorySteel,
                S("banco_taller", 0f, 2f, 2.4f, 1f, "Motor Merlin 1C en su soporte"), S("armario", 3f, -3f, 1.2f, 0.6f, "Inyectores y válvulas")),
            new Lot("Cafetería de la fábrica", "Restaurante", "Cafetería de la fábrica", 14f, 10f, FactorySteel,
                S("cocina", -3f, 2f, 2f, 0.8f, "Cocina de la cafetería"), S("mesa", 2f, -1f, 2f, 1f, "Mesas largas"), S("barra", 3f, 3f, 2f, 0.8f, "Barra de café")),
            new Lot("Apartamentos de ingenieros", "Casa", "Apartamento de ingenieros jóvenes", 14f, 10f, Stucco,
                S("cama", -4f, 2f, 1.2f, 2f, "Cama"), S("cama", 0f, 2f, 1.2f, 2f, "Cama"), S("cama", 4f, 2f, 1.2f, 2f, "Cama")),
        }, new[]
        {
            new Lot("Casa de Hawthorne", "Casa", "Casa de estuco de Hawthorne", 10f, 9f, Stucco),
            new Lot("Taller de coches", "Taller", "Taller de coches de Crenshaw", 12f, 10f, Stucco, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco del mecánico"), S("armario", 0f, 0f, 1.2f, 0.6f, "Recambios")),
            new Lot("Tienda de la esquina", "Mercado", "Tienda de la esquina", 10f, 8f, Stucco, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
            new Lot("Taquería", "Restaurante", "Taquería de Hawthorne", 10f, 8f, Stucco, S("fogon", 0f, 0f, 1.6f, 0.8f, "Plancha de los tacos"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa con bancos")),
        }, C(0.45f, 0.45f, 0.46f));
        int n = 0;
        n += W(t, "Ingeniero de aviónica", s[1], "sabio", s[3], null, s[4]);
        n += W(t, "Técnica de integración", s[0], "", s[3], null, s[4]);
        n += W(t, "Técnico de motores", s[2], "", s[3], null, s[4]);
        n += W(t, "Cocinera de la cafetería", s[3], "cocinero", s[3], null, null);
        n += Townsfolk(t, s, 5, "Vecina de Hawthorne", "Vecino de Hawthorne", s[3], null);
        return n;
    }

    // ══ ALDEA DE LA MONTAÑA (homenaje a Goku; ficción): el patio de entrenamiento, la casa de la maestra, la cabaña de Goku, la cocina
    //    de la aldea, el templo de la montaña, los bancales de arroz y la leñera; casas de piedra y pino, huertos de té, un molino.
    static int MountainVillageTown(Transform t)
    {
        var s = Town(t, "Aldea de la montaña", new[]
        {
            new Lot("Patio de entrenamiento", "Centro de entrenamiento", "Patio de entrenamiento de la aldea", 16f, 12f, MountainTimber,
                S("muneco", -4f, 2f, 1f, 1f, "Poste de madera"), S("muneco", 0f, 2f, 1f, 1f, "Poste de madera"), S("armario", 5f, -3f, 1.4f, 0.6f, "Bastones y cubos")),
            new Lot("Casa de la maestra", "Casa", "Casa de la maestra de la montaña", 12f, 10f, MountainTimber,
                S("cama", -2f, 2f, 1.2f, 2f, "Cama de la maestra"), S("estante", 3f, 2f, 0.8f, 1.6f, "Rollos y tazas de té")),
            new Lot("Cabaña de Goku", "Casa", "Cabaña de Goku", 10f, 9f, MountainTimber,
                S("cama", -2f, 2f, 1.2f, 2f, "Cama de Goku")),
            new Lot("Cocina de la aldea", "Cocina", "Cocina de la aldea (arroz y verduras del huerto)", 14f, 10f, MountainTimber,
                S("fogon", -3f, 2f, 1.6f, 0.8f, "Wok sobre el fogón de leña"), S("mesa", 2f, -1f, 2f, 1f, "Mesa larga con banquetas"), S("estante", 4f, 3f, 0.8f, 1.6f, "Tarros de encurtidos")),
            new Lot("Templo de la montaña", "Templo", "Templo de la montaña", 12f, 10f, MountainTimber,
                S("altar", 0f, 2f, 2f, 1f, "Altar con incienso"), S("banco", 0f, -1f, 2f, 0.6f, "Banco")),
            new Lot("Bancales de arroz", "Huerto", "Bancales de arroz de la aldea", 14f, 12f, MountainTimber,
                S("bancal", -2f, 0f, 4f, 6f, "Bancal de arroz"), S("pozo", 4f, 2f, 1.2f, 1.2f, "Acequia")),
            new Lot("Leñera y taller", "Taller", "Leñera y taller de la aldea", 12f, 10f, MountainTimber,
                S("banco_taller", 0f, 2f, 2f, 0.9f, "Tajo de cortar leña"), S("armario", 3f, -3f, 1.2f, 0.6f, "Hachas y sierras")),
        }, new[]
        {
            new Lot("Casa de la aldea", "Casa", "Casa de piedra y pino de la aldea", 10f, 9f, MountainTimber),
            new Lot("Huerto de té", "Huerto", "Huerto de té de la ladera", 12f, 10f, MountainTimber, S("bancal", 0f, 0f, 4f, 6f, "Matas de té"), S("pozo", 0f, 0f, 1f, 1f, "Pozo")),
            new Lot("Molino de agua", "Granero", "Molino de agua del torrente", 10f, 8f, MountainTimber, S("granero", 0f, 0f, 2f, 2f, "Muela del molino"), S("estante", 0f, 0f, 0.8f, 1.6f, "Sacos de harina")),
            new Lot("Tienda de la aldea", "Mercado", "Tienda de la aldea", 10f, 8f, MountainTimber, S("puesto", 0f, 0f, 2f, 1f, "Mostrador (té, sal, aceite)"), S("estante", 0f, 0f, 0.8f, 1.6f, "Tarros")),
        }, C(0.55f, 0.5f, 0.44f));
        int n = 0;
        n += W(t, "Aprendiz de la maestra", s[0], "alumno", s[3], s[0], null);
        n += W(t, "Cocinera de la aldea", s[3], "cocinero", s[3], s[0], null);
        n += W(t, "Guardián del templo", s[4], "", s[3], s[0], null);
        n += W(t, "Hortelano del arrozal", s[5], "hortelano", s[3], s[0], null);
        n += W(t, "Carpintera de la aldea", s[6], "", s[3], s[0], null);
        n += Townsfolk(t, s, 7, "Vecina de la aldea", "Vecino de la aldea", s[3], s[0]);
        return n;
    }

    // ══ ALBUQUERQUE (1975): MITS (el Altair 8800), la primera oficina de Micro-Soft, el motel, la cafetería de la Ruta 66, el piso de
    //    Gates y Allen y la tienda de electrónica; casas de estuco color adobe, talleres de la Ruta 66 y huertos del valle.
    static int AlbuquerqueTown(Transform t)
    {
        var s = Town(t, "Albuquerque", new[]
        {
            new Lot("MITS", "Taller", "MITS (Micro Instrumentation and Telemetry Systems): el Altair 8800", 16f, 12f, PuebloStucco,
                S("banco_taller", -4f, 2f, 2.4f, 1f, "Mesa de montaje del Altair 8800"), S("consola", 2f, 2f, 1.6f, 0.8f, "Altair 8800 con su teletipo"), S("estante", 5f, -3f, 0.8f, 2f, "Kits del Altair en sus cajas")),
            new Lot("Oficina de Micro-Soft", "Centro de tecnología", "La primera oficina de Micro-Soft", 14f, 10f, PuebloStucco,
                S("consola", -3f, 2f, 1.6f, 0.8f, "Terminal"), S("mesa", 2f, 2f, 2f, 1f, "Mesa con listados de código"), S("estante", 4f, -3f, 0.8f, 1.6f, "Cintas perforadas y manuales")),
            new Lot("Motel", "Hotel", "Motel de la Ruta 66", 14f, 10f, PuebloStucco,
                S("cama", -3f, 2f, 1.4f, 2f, "Cama del motel"), S("cama", 2f, 2f, 1.4f, 2f, "Cama del motel"), S("mesa", 4f, -3f, 1.2f, 0.7f, "Mesita de escribir")),
            new Lot("Cafetería de la Ruta 66", "Restaurante", "Cafetería de la Ruta 66 (un diner)", 14f, 10f, PuebloStucco,
                S("cocina", -3f, 2f, 2f, 0.8f, "Plancha del diner"), S("barra", 2f, 2f, 2.4f, 0.8f, "Barra con taburetes"), S("mesa", 0f, -3f, 1.6f, 1f, "Mesa de reservado")),
            new Lot("Piso de Gates y Allen", "Casa", "Piso de Bill Gates y Paul Allen", 12f, 10f, PuebloStucco,
                S("cama", -2f, 2f, 1.2f, 2f, "Cama de Bill"), S("cama", 2f, 2f, 1.2f, 2f, "Cama de Paul")),
            new Lot("Tienda de electrónica", "Mercado", "Tienda de electrónica (componentes y kits)", 12f, 10f, PuebloStucco,
                S("puesto", 0f, 2f, 2f, 1f, "Mostrador de componentes"), S("estante", -3f, -3f, 0.8f, 1.6f, "Cajones de resistencias y chips")),
        }, new[]
        {
            new Lot("Casa de Albuquerque", "Casa", "Casa de estuco de Albuquerque", 10f, 9f, PuebloStucco),
            new Lot("Taller de la Ruta 66", "Taller", "Taller mecánico de la Ruta 66", 12f, 10f, PuebloStucco, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco del mecánico"), S("armario", 0f, 0f, 1.2f, 0.6f, "Recambios")),
            new Lot("Tienda del barrio", "Mercado", "Tienda del barrio", 10f, 8f, PuebloStucco, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
            new Lot("Huerto del valle", "Huerto", "Huerto de chile y albaricoques", 12f, 10f, PuebloStucco, S("bancal", 0f, 0f, 4f, 6f, "Matas de chile"), S("pozo", 0f, 0f, 1f, 1f, "Acequia")),
        }, C(0.5f, 0.48f, 0.46f));
        int n = 0;
        n += W(t, "Técnico de MITS", s[0], "", s[3], null, null);
        n += W(t, "Montadora de kits de MITS", s[0], "", s[3], null, null);
        n += W(t, "Programador de Micro-Soft", s[1], "sabio", s[3], null, s[4]);
        n += W(t, "Recepcionista del motel", s[2], "", s[3], null, null);
        n += W(t, "Cocinero del diner", s[3], "cocinero", s[3], null, null);
        n += W(t, "Dependienta de la tienda de electrónica", s[5], "comerciante", s[3], null, null);
        n += Townsfolk(t, s, 6, "Vecina de Albuquerque", "Vecino de Albuquerque", s[3], null);
        return n;
    }

    // ══ OMAHA (1983): la Nebraska Furniture Mart, el despacho de Berkshire Hathaway (Kiewit Plaza), la casa de Buffett en Farnam
    //    Street, Gorat's (el asador al que va Buffett), el almacén y taller de tapicería de la tienda y la casa de los Blumkin; casas
    //    de ladrillo y tablas, tiendas de Dodge Street, talleres e iglesias.
    static int OmahaTown(Transform t)
    {
        var s = Town(t, "Omaha", new[]
        {
            new Lot("Nebraska Furniture Mart", "Mercado", "Nebraska Furniture Mart (la tienda de la señora B)", 20f, 14f, OmahaBrick,
                S("puesto", -6f, 3f, 2.4f, 1.2f, "Rollos de alfombra"), S("puesto", 0f, 3f, 2.4f, 1.2f, "Sofás y sillones"), S("estante", 6f, 3f, 1.2f, 2f, "Neveras y lavadoras"), S("mesa", 0f, -4f, 1.6f, 1f, "Mesa de la señora B")),
            new Lot("Despacho de Berkshire Hathaway", "Taller de papeles", "Despacho de Berkshire Hathaway (Kiewit Plaza)", 14f, 10f, OmahaBrick,
                S("mesa", -2f, 2f, 2f, 1f, "Mesa de Buffett (informes anuales)"), S("estante", 3f, 2f, 0.8f, 2f, "Informes de empresas"), S("armario", 0f, -3f, 1.2f, 0.6f, "Archivo")),
            new Lot("Casa de Buffett en Farnam Street", "Casa", "Casa de Warren Buffett (Farnam Street, desde 1958)", 12f, 10f, OmahaBrick,
                S("cama", -2f, 2f, 1.6f, 2f, "Cama"), S("estante", 3f, 2f, 0.8f, 1.6f, "Libros y periódicos")),
            new Lot("Gorat's Steak House", "Restaurante", "Gorat's (el asador de Omaha)", 14f, 10f, OmahaBrick,
                S("cocina", -3f, 2f, 2f, 0.8f, "Parrilla"), S("mesa", 2f, -1f, 1.6f, 1f, "Mesa con mantel"), S("mesa", -2f, -1f, 1.6f, 1f, "Mesa con mantel")),
            new Lot("Almacén de la tienda", "Taller", "Almacén y taller de tapicería de la Nebraska Furniture Mart", 16f, 12f, OmahaBrick,
                S("banco_taller", -3f, 2f, 2.4f, 1f, "Banco del tapicero"), S("estante", 3f, 2f, 1.2f, 2f, "Rollos de tela y de alfombra"), S("armario", 0f, -3f, 1.2f, 0.6f, "Herramientas")),
            new Lot("Casa de los Blumkin", "Casa", "Casa de Rose Blumkin", 12f, 10f, OmahaBrick,
                S("cama", -2f, 2f, 1.4f, 2f, "Cama de la señora B")),
        }, new[]
        {
            new Lot("Casa de Omaha", "Casa", "Casa de ladrillo y tablas de Omaha", 10f, 9f, OmahaBrick),
            new Lot("Tienda de Dodge Street", "Mercado", "Tienda de Dodge Street", 10f, 8f, OmahaBrick, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
            new Lot("Taller del barrio", "Taller", "Taller del barrio", 12f, 10f, OmahaBrick, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco"), S("armario", 0f, 0f, 1.2f, 0.6f, "Herramientas")),
            new Lot("Iglesia del barrio", "Templo", "Iglesia del barrio", 12f, 10f, OmahaBrick, S("altar", 0f, 0f, 2f, 1f, "Altar"), S("banco", 0f, 0f, 2f, 0.6f, "Banco")),
        }, C(0.42f, 0.42f, 0.42f));
        int n = 0;
        n += W(t, "Vendedor de alfombras", s[0], "comerciante", s[3], null, null);
        n += W(t, "Vendedora de muebles", s[0], "comerciante", s[3], null, null);
        n += W(t, "Secretaria de Berkshire", s[1], "", s[3], null, null);
        n += W(t, "Camarero de Gorat's", s[3], "cocinero", s[3], null, null);
        n += W(t, "Tapicero de la tienda", s[4], "", s[3], null, null);
        n += Townsfolk(t, s, 6, "Vecina de Omaha", "Vecino de Omaha", s[3], null);
        return n;
    }

    // ══ PRAIRIE VIEW (2020): el campus de Prairie View A&M (la presidencia, la biblioteca, las aulas de ciencias, el comedor, una
    //    residencia, la oficina de ayudas y la granja experimental) y el pueblo (casas, iglesia, tienda, taller).
    static int PrairieViewTown(Transform t)
    {
        var s = Town(t, "Prairie View", new[]
        {
            new Lot("Edificio de administración", "Taller de papeles", "Presidencia de Prairie View A&M", 14f, 10f, CampusBrick,
                S("mesa", -2f, 2f, 2f, 1f, "Mesa de la presidenta"), S("estante", 3f, 2f, 0.8f, 2f, "Historia de la universidad (desde 1876)"), S("mesa", 0f, -3f, 2.4f, 1f, "Mesa de reuniones")),
            new Lot("Biblioteca John B. Coleman", "Escuela", "Biblioteca John B. Coleman", 18f, 12f, CampusBrick,
                S("estante", -5f, 3f, 2f, 0.6f, "Estanterías"), S("estante", 0f, 3f, 2f, 0.6f, "Estanterías"), S("mesa", -2f, -2f, 2.4f, 1f, "Mesa de estudio"), S("mesa", 3f, -2f, 2.4f, 1f, "Mesa de estudio")),
            new Lot("Aulas de ciencias", "Laboratorio", "Aulas y laboratorios de ciencias e ingeniería", 16f, 12f, CampusBrick,
                S("mesa_laboratorio", -3f, 2f, 2.4f, 1f, "Mesa de laboratorio"), S("mesa_laboratorio", 3f, 2f, 2.4f, 1f, "Mesa de laboratorio"), S("pizarra", 0f, -4f, 2.4f, 0.4f, "Pizarra")),
            new Lot("Comedor del campus", "Restaurante", "Comedor del campus", 16f, 12f, CampusBrick,
                S("cocina", -4f, 3f, 2.4f, 0.8f, "Cocina del comedor"), S("mesa", 0f, -1f, 2.4f, 1f, "Mesas largas"), S("barra", 4f, 3f, 2.4f, 0.8f, "Barra de servir")),
            new Lot("Residencia de estudiantes", "Casa", "Residencia de estudiantes", 16f, 12f, CampusBrick,
                S("cama", -5f, 3f, 1f, 2f, "Cama"), S("cama", -2f, 3f, 1f, 2f, "Cama"), S("cama", 1f, 3f, 1f, 2f, "Cama"), S("cama", 4f, 3f, 1f, 2f, "Cama")),
            new Lot("Oficina de ayudas a estudiantes", "Taller de papeles", "Oficina de ayudas a estudiantes", 12f, 10f, CampusBrick,
                S("mesa", 0f, 2f, 2.4f, 1f, "Mostrador de atención"), S("armario", 3f, -3f, 1.2f, 0.6f, "Expedientes")),
            new Lot("Granja experimental", "Huerto", "Granja experimental de la universidad", 16f, 12f, CampusBrick,
                S("bancal", -3f, 0f, 4f, 6f, "Bancal de ensayo"), S("pozo", 4f, 2f, 1.2f, 1.2f, "Riego")),
        }, new[]
        {
            new Lot("Casa de Prairie View", "Casa", "Casa de madera de Prairie View", 10f, 9f, OmahaBrick),
            new Lot("Iglesia de Prairie View", "Templo", "Iglesia del pueblo", 12f, 10f, OmahaBrick, S("altar", 0f, 0f, 2f, 1f, "Púlpito"), S("banco", 0f, 0f, 2f, 0.6f, "Banco")),
            new Lot("Tienda del pueblo", "Mercado", "Tienda del pueblo", 10f, 8f, OmahaBrick, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
            new Lot("Taller de Prairie View", "Taller", "Taller mecánico del pueblo", 12f, 10f, OmahaBrick, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco"), S("armario", 0f, 0f, 1.2f, 0.6f, "Recambios")),
        }, C(0.46f, 0.45f, 0.44f));
        int n = 0;
        n += W(t, "Bibliotecaria de la Coleman", s[1], "sabio", s[3], null, null);
        n += W(t, "Profesora de química", s[2], "sabio", s[3], null, null);
        n += W(t, "Estudiante de cuarto", s[2], "alumno", s[3], s[1], s[4]);
        n += W(t, "Estudiante de tercero", s[1], "alumno", s[3], s[1], s[4]);
        n += W(t, "Cocinero del comedor", s[3], "cocinero", s[3], null, null);
        n += W(t, "Técnica de la granja experimental", s[6], "hortelano", s[3], null, null);
        n += Townsfolk(t, s, 7, "Vecina de Prairie View", "Vecino de Prairie View", s[3], null);
        return n;
    }

    // ══ UAGADUGÚ (2024): la explanada de la entrega (el parque de maquinaria), los huertos del barrage, el mercado Rood Woko, un
    //    centro de formación agrícola, un maquis, la cour familiale de la horticultora y el molino de cereales; patios familiares,
    //    talleres de soldadura, mercados y huertos de barrio. Nada militar.
    static int OuagadougouTown(Transform t)
    {
        var s = Town(t, "Uagadugú", new[]
        {
            new Lot("Parque de maquinaria agrícola", "Taller", "Explanada de la entrega: tractores, motocultores y motobombas", 20f, 14f, SahelBanco,
                S("banco_taller", -5f, 3f, 2.4f, 1.2f, "Tractor"), S("banco_taller", 0f, 3f, 2.4f, 1.2f, "Motocultor"), S("armario", 5f, 3f, 1.6f, 0.8f, "Motobombas y repuestos"), S("mesa", 0f, -4f, 2f, 1f, "Mesa de la brigada de mecanización")),
            new Lot("Huertos del barrage", "Huerto", "Huertos del barrage de Tanghin (tomate, cebolla, col)", 16f, 12f, SahelBanco,
                S("bancal", -4f, 0f, 4f, 6f, "Bancal de tomates"), S("bancal", 2f, 0f, 4f, 6f, "Bancal de cebollas"), S("pozo", 6f, 3f, 1.2f, 1.2f, "Pozo y regaderas")),
            new Lot("Mercado Rood Woko", "Mercado", "Gran mercado Rood Woko", 18f, 12f, SahelBanco,
                S("puesto", -5f, 2f, 2f, 1f, "Puesto de tomates y cebollas"), S("puesto", 0f, 2f, 2f, 1f, "Puesto de mijo, maíz y sorgo"), S("puesto", 5f, 2f, 2f, 1f, "Puesto de telas (Faso Dan Fani)")),
            new Lot("Centro de formación agrícola", "Escuela", "Centro de formación agrícola", 14f, 10f, SahelBanco,
                S("pizarra", 0f, 3.5f, 2.4f, 0.4f, "Pizarra: cómo cavar el zaï"), S("mesa", 0f, 0f, 2.4f, 1f, "Mesa con semillas de muestra"), S("estante", -5f, -3f, 0.8f, 1.6f, "Fichas de cultivos")),
            new Lot("Maquis del barrio", "Restaurante", "Maquis del barrio (tô y salsa de gombo)", 14f, 10f, SahelBanco,
                S("fogon", -3f, 2f, 1.6f, 0.8f, "Olla de tô y salsa de gombo"), S("mesa", 2f, -1f, 2f, 1f, "Mesa con bancos"), S("barra", 3f, 3f, 2f, 0.8f, "Barra de zumo de bissap")),
            new Lot("Cour familiale de la horticultora", "Casa", "Cour familiale de la horticultora", 14f, 12f, SahelBanco,
                S("cama", -4f, 2f, 1.2f, 2f, "Cama"), S("cama", 0f, 2f, 1.2f, 2f, "Cama"), S("cama", 4f, 2f, 1.2f, 2f, "Cama de los niños")),
            new Lot("Molino de cereales", "Granero", "Molino de cereales del barrio", 12f, 10f, SahelBanco,
                S("granero", 0f, 2f, 2f, 2f, "Molino de mijo y maíz"), S("estante", 3f, -3f, 0.8f, 1.6f, "Sacos de harina")),
        }, new[]
        {
            new Lot("Cour familiale de Uagadugú", "Casa", "Cour familiale (patio de varias casas)", 12f, 10f, SahelBanco),
            new Lot("Taller de soldadura", "Taller", "Taller de soldadura (carros y aperos)", 12f, 10f, SahelBanco, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco de soldar"), S("armario", 0f, 0f, 1.2f, 0.6f, "Hierros y herramientas")),
            new Lot("Mercado de barrio", "Mercado", "Mercado de barrio", 10f, 8f, SahelBanco, S("puesto", 0f, 0f, 2f, 1f, "Puesto de verduras"), S("puesto", 0f, 0f, 2f, 1f, "Puesto de cereales")),
            new Lot("Huerto de barrio", "Huerto", "Huerto de barrio", 12f, 10f, SahelBanco, S("bancal", 0f, 0f, 4f, 6f, "Bancal"), S("pozo", 0f, 0f, 1f, 1f, "Pozo")),
        }, C(0.6f, 0.4f, 0.28f));
        int n = 0;
        n += W(t, "Mecánica de la brigada", s[0], "", s[4], null, null);
        n += W(t, "Horticultor del barrage", s[1], "hortelano", s[4], null, null);
        n += W(t, "Vendedora del mercado", s[2], "comerciante", s[4], null, null);
        n += W(t, "Formador agrícola", s[3], "sabio", s[4], null, null);
        n += W(t, "Cocinera del maquis", s[4], "cocinero", s[4], null, null);
        n += W(t, "Molinero del barrio", s[6], "", s[4], null, null);
        n += C2(t, "Hortelana de Tanghin", "Tejedor de Tanghin", s[5], s[4], s[1]);
        n += Townsfolk(t, s, 7, "Vecina de Uagadugú", "Vecino de Uagadugú", s[4], null);
        return n;
    }
}
