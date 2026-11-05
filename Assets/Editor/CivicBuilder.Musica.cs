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
            case "Seúl": return SeoulTown(t);
            case "Kingston": return KingstonTown(t);
            case "Belfast": return BelfastTown(t);
            case "Long Island": return LongIslandTown(t);
            case "Grandes aportes de las canciones": return SongsTown(t);
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
mar Océano Atlántico | 2100 130 | 0 573
lago Laguna Tortuguero | 170 70 | 711 406
rio Río Cibuco | 30 | -684 -593, -576 -156, -630 156, -558 515
colina Mogote de Almirante Sur | 150 70 | -216 -468
colina Mogote de la PR-155 | 120 60 | 297 -484
colina Mogote del oeste | 110 55 | -792 -437
campo Pastos del llano del norte | 320 110 | 270 367
calle Carretera PR-2 | 18 | -900 -47, 900 -47
calle Expreso PR-22 | 24 | -900 -234, 900 -218
calle PR-155 (a Almirante Sur) | 10 | 54 -47, 36 -562
calle Calle Betances | 8 | -378 94, 378 94
calle Calle de la plaza | 8 | -216 -47, -216 234
calle Calle Tulio Otero | 8 | 216 -47, 216 234
calle Carretera a Puerto Nuevo | 10 | 0 -47, 0 499
calle Camino de Almirante Sur | 6 | -396 -374, 432 -374
hito 0 | 81 86
hito 1 | -252 148
hito 2 | -207 -421
hito 3 | -117 -406
hito 4 | 180 -101
hito 5 | 108 -140
hito 6 | 288 133
hito 7 | -378 -101
hito 8 | -54 -296
Plaza | -117 164
Mercado | 342 31
Parque | -468 55
Piscina | 423 179 | actual: playa de Puerto Nuevo (Vega Baja), posición aproximada
Entrenamiento | -297 -133 | actual: parque atlético de Vega Baja, posición aproximada
Guarderia | 144 187
Estudio | -378 179
Bomberos | 504 -125
Policia | -486 -101
Museo | -54 218
Hotel | 135 437
Taller | 576 47
Carpinteria | -612 -23
Monumento | -162 101
relleno | 55 | 0 70",
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
        // SEÚL (2016): el barrio de Hapjeong (Mapo-gu), con la agencia YG entre la estación de Hapjeong y el río Han; Yanghwa-ro en diagonal hacia Hongdae,
        // el mercado de Mangwon al oeste, el parque de Mangwon en la orilla y el puente de Yanghwa. Plano aproximado y ACORTADO (OpenStreetMap).
        ["Seúl"] = @"
rio Río Han | 240 | -1000 -500, 0 -600, 1000 -700
campo Parque de Mangwon (orilla del Han) | 700 90 | -300 -410
colina Monte Wausan | 220 40 | 840 560
calle Yanghwa-ro | 24 | -560 -440, -120 -120, 500 260, 900 420
calle Dongmak-ro | 12 | 220 -520, 240 640
calle Wausan-ro | 10 | -500 620, 620 -60
calle Tojeong-ro | 10 | -1000 -300, 300 -340
calle Poeun-ro | 8 | -760 -320, -660 560
calle Calle de los pisos | 6 | -560 110, 40 300
calle Calle de Hongdae | 8 | 330 150, 620 560
hito 0 | -60 -40
hito 1 | -120 60
hito 2 | -230 190
hito 3 | -30 90
hito 4 | 120 -90
hito 5 | -640 180
hito 6 | 240 -220
hito 7 | 480 390
hito 8 | -420 -130
Plaza | -220 -120
Parque | -540 -390
Mercado | 340 20
Guarderia | -330 320
Estudio | 60 390
Museo | 410 -160
Estacion | -190 -260
Bomberos | 590 180
Policia | -500 40
Tecnologia | 150 170
Taller | -760 -130
Monumento | -300 -40
relleno | 55 | 0 100",
        // KINGSTON (1978): de oeste a este, Trench Town y sus calles numeradas, Spanish Town Road, el mercado de Coronation junto al puerto, Studio One en Brentford
        // Road, el Estadio Nacional y Hope Road (el n.º 56, Tuff Gong) al noreste, con las Montañas Azules y el puerto de Kingston al sur. Plano ACORTADO (las
        // distancias reales son el triple) y aproximado (OpenStreetMap; Trench Town Culture Yard; National Library of Jamaica).
        ["Kingston"] = @"
mar Puerto de Kingston | 2100 150 | 0 -730
monte Montañas Azules | 420 280 | 880 640
colina Long Mountain | 200 80 | 900 -160 | seco
campo Solares del oeste de Kingston | 300 120 | -800 -500
calle Marcus Garvey Drive | 14 | -1000 -560, 400 -560
calle Spanish Town Road | 14 | -1000 -420, -240 -470
calle Maxfield Avenue | 8 | -620 -420, -620 300
calle First Street (Trench Town) | 6 | -840 -60, -440 -60
calle Second Street (Trench Town) | 6 | -840 -120, -440 -120
calle Seventh Street (Trench Town) | 6 | -840 -360, -440 -360
calle Slipe Road | 12 | -220 -470, -220 500
calle Brentford Road | 10 | -420 180, -80 220
calle Arthur Wint Drive | 12 | 120 -160, 120 400
calle Mountain View Avenue | 12 | 420 -540, 420 420
calle Hope Road | 14 | -120 600, 900 500
calle Half Way Tree Road | 12 | -340 720, -220 500
hito 0 | 120 60
hito 1 | -640 -40
hito 2 | -700 -110
hito 3 | -300 230
hito 4 | 330 540
hito 5 | -130 -420
hito 6 | -520 -200
hito 7 | -40 -310
hito 8 | 260 -60
Plaza | -130 -130
Parque | 320 300
Piscina | 560 240 | actual: complejo acuático del Estadio Nacional (Kingston), posición aproximada
Guarderia | -540 170
Estudio | 60 310
Museo | -60 -520
Estacion | -300 -530
Bomberos | 300 -440
Policia | -30 170
Taller | -800 -230
Carpinteria | -730 170
Monumento | 40 -170
relleno | 55 | -300 0",
        // BELFAST (1998): el Waterfront Hall (Lanyon Place) junto al río Lagan; el Maysfield Leisure Centre al lado; el City Hall y Donegall Place al oeste; Falls Road y
        // Shankill Road hacia el noroeste, separadas por la línea de paz; Queen's University al sur; los astilleros Harland & Wolff al este del río; Cave Hill al noroeste y Belfast
        // Lough al noreste. Plano ACORTADO a un tercio al oeste y aproximado (OpenStreetMap; Belfast City Council).
        ["Belfast"] = @"
rio Río Lagan | 90 | 180 -760, 150 -260, 120 20, 250 270, 560 700
lago Belfast Lough | 300 110 | 760 620
monte Cave Hill | 280 180 | -880 610
colina Black Mountain | 240 90 | -920 170
campo Parque Ormeau | 240 160 | 620 -520
calle Oxford Street | 14 | 60 -220, -40 240
calle May Street | 14 | -440 -40, 60 -30
calle Donegall Place | 16 | -440 -260, -420 300
calle Royal Avenue | 16 | -420 300, -260 500
calle Falls Road | 10 | -440 40, -1000 -130
calle Shankill Road | 10 | -420 230, -1000 440
calle Línea de paz (Cupar Way) | 6 | -1000 150, -640 150
calle Ormeau Road | 12 | 40 -180, 280 -720
calle Lisburn Road | 12 | -440 -260, -640 -720
calle Queen's Road (astilleros) | 12 | 330 200, 1000 330
calle Newtownards Road | 12 | 210 -40, 1000 -60
hito 0 | -10 60
hito 1 | -160 150
hito 2 | 40 -160
hito 3 | -540 -590
hito 4 | -820 -120
hito 5 | -620 50
hito 6 | -640 290
hito 7 | 620 200
hito 8 | -470 40
Plaza | -320 -100
Mercado | -60 -280
Parque | 560 -480
Piscina | -760 -230 | actual: Falls Leisure Centre (Belfast), posición aproximada
Guarderia | -560 -150
Museo | -470 -470
Estacion | -160 -300
Bomberos | -200 340
Policia | -330 180
Tecnologia | 500 60
Mecanica | 780 -160
Taller | 760 80
Monumento | -420 -170
relleno | 60 | -300 100",
        // LONG ISLAND (1989): Roosevelt, con Nassau Road de norte a sur, y, al norte, Hempstead (South Franklin Street) y la Universidad Adelphi en Garden City; la Southern State Parkway,
        // el lago de Hempstead al oeste y el Nassau Coliseum al noreste. Plano ACORTADO (el campus queda a ~6 km de Roosevelt) y aproximado (OpenStreetMap; Town of Hempstead).
        ["Long Island"] = @"
lago Lago de Hempstead | 90 220 | -880 -120
campo Hempstead Lake State Park | 200 300 | -700 -440
campo Parque Eisenhower | 300 170 | 760 600
calle Nassau Road | 14 | 0 -720, 0 330
calle Peninsula Boulevard | 14 | -120 330, -600 -720
calle Southern State Parkway | 26 | -1000 -180, 1000 -140
calle Front Street (Hempstead) | 12 | -1000 370, 1000 330
calle South Franklin Street | 10 | -120 330, -120 700
calle Stewart Avenue (Garden City) | 10 | -700 620, 1000 620
calle Hempstead Turnpike | 16 | 150 480, 1000 440
calle Calle de las casas | 6 | -460 -440, 460 -440
calle Centennial Avenue | 8 | -340 60, 460 60
calle Babylon Turnpike | 12 | 40 -300, 900 -720
hito 0 | 120 660
hito 1 | 40 680
hito 2 | -190 540
hito 3 | -130 -30
hito 4 | 260 -400
hito 5 | -250 150
hito 6 | 840 380
hito 7 | 120 110
hito 8 | 630 -30
hito 9 | -210 -340
Plaza | 90 240
Mercado | 340 180
Parque | 480 -270
Piscina | 650 -480 | actual: Roosevelt Park (Long Island), posición aproximada
Guarderia | 150 -260
Museo | 450 540
Estacion | -320 270
Bomberos | -350 -240
Policia | 260 240
Mecanica | 580 240
Taller | -640 160
Carpinteria | 740 80
relleno | 55 | 0 -300",
        // LA PLAZA DE LAS CANCIONES (lugar INVENTADO, como la aldea de la montaña): una plaza y su paseo, y a su alrededor seis barrios-rincón con el clima, la flora y el mobiliario de cada
        // canción (Lot.Era). El río del sur y los montes son de atrezo: dan sitio al arrozal y a los Andes.
        ["Grandes aportes de las canciones"] = @"
rio Río de las Voces | 26 | -1000 -640, 0 -560, 1000 -680
monte Andes de la canción | 280 170 | 760 -480
colina Monte de Highlander | 220 60 | -820 560
campo Arrozal de Vercelli | 300 200 | -700 -300
campo Prado de Ascot | 260 160 | -60 590
calle Paseo de las Canciones | 10 | -900 0, 900 0
calle Calle del norte | 8 | 0 0, 0 620
calle Calle del sur | 8 | 0 0, 0 -500
calle Ronda | 8 | -560 -420, 560 -420, 560 420, -560 420, -560 -420
calle Camino de Highlander | 6 | -560 420, -820 500
calle Camino de los Andes | 6 | 560 -320, 700 -400
hito 0 | -560 300
hito 1 | -600 -230
hito 2 | -80 500
hito 3 | 640 300
hito 4 | 650 -60
hito 5 | 610 -370
hito 6 | 70 110
hito 7 | -110 80
hito 8 | 150 -100
hito 9 | -140 -130
hito 10 | 260 180
hito 11 | -260 -280
Plaza | -60 -70
Mercado | 300 -240
Parque | 300 290
Guarderia | 240 -340
Museo | -300 170
Estacion | -860 -70
Bomberos | 460 90
Policia | -420 -60
Taller | 440 -300
Carpinteria | -360 -390
Monumento | 40 -220
relleno | 40 | 0 -120",
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

    // ══ SEÚL (2016): la agencia YG, la sala de ensayo, la residencia de las aprendizas, el comedor, el aula de lenguas, el mercado de Mangwon, el jjimjilbang, una sala
    //    de conciertos de Hongdae y la clínica; pisos de hormigón, cafeterías y talleres.
    static int SeoulTown(Transform t)
    {
        var s = Town(t, "Seúl", new[]
        {
            new Lot("Edificio de la agencia YG", "Centro de tecnología", "Agencia YG (Hapjeong): estudios de grabación", 18f, 12f, SeoulConcrete,
                S("consola", -4f, 2f, 1.6f, 0.8f, "Mesa del productor"), S("mesa_mezclas", 3f, 2f, 1.6f, 0.7f, "Portátil, interfaz y altavoces"), S("microfono", 0f, -3f, 0.6f, 0.6f, "Micrófono de la cabina")),
            new Lot("Sala de ensayo de las aprendizas", "Centro de entrenamiento", "Sala de ensayo y gimnasio de las aprendizas", 16f, 14f, SeoulConcrete, TrainingKit("gimnasio_moderno")),
            new Lot("Residencia de las aprendizas", "Casa", "Residencia de las aprendizas", 14f, 10f, SeoulConcrete,
                S("cama", -5f, 2f, 1f, 2f, "Cama de Jennie"), S("cama", -2f, 2f, 1f, 2f, "Cama de Lisa"), S("cama", 1f, 2f, 1f, 2f, "Cama de Rosé"), S("cama", 4f, 2f, 1f, 2f, "Cama de Jisoo")),
            new Lot("Comedor de la agencia", "Restaurante", "Comedor de la agencia (kimchi-jjigae y arroz)", 14f, 10f, SeoulConcrete,
                S("cocina", -3f, 2f, 2f, 0.8f, "Ollas de arroz y sopa"), S("mesa", 2f, -1f, 2f, 1f, "Mesas"), S("barra", 3f, 3f, 2f, 0.8f, "Barra de servir")),
            new Lot("Aula de lenguas", "Escuela", "Aula de lenguas de la agencia (coreano, japonés e inglés)", 14f, 10f, SeoulConcrete,
                S("pizarra", 0f, 3.5f, 2.4f, 0.4f, "Pizarra de hangul"), S("mesa", 0f, 0f, 2.4f, 1f, "Mesa de estudio"), S("estante", -5f, -3f, 0.8f, 1.6f, "Diccionarios")),
            new Lot("Mercado de Mangwon", "Mercado", "Mercado de Mangwon", 18f, 12f, SeoulConcrete,
                S("puesto", -5f, 2f, 2f, 1f, "Puesto de tteokbokki"), S("puesto", 0f, 2f, 2f, 1f, "Puesto de verduras"), S("puesto", 5f, 2f, 2f, 1f, "Puesto de pescado seco")),
            new Lot("Jjimjilbang", "Baños", "Jjimjilbang de Hapjeong (baños y sauna)", 14f, 12f, SeoulConcrete,
                S("piscina", 0f, 0f, 4f, 3f, "Baño caliente"), S("banco", 0f, 0f, 2f, 0.6f, "Banco de la sauna"), S("armario", 0f, 0f, 1.2f, 0.6f, "Taquillas")),
            new Lot("Sala de conciertos de Hongdae", "Sala de baile", "Sala de conciertos de Hongdae", 16f, 12f, SeoulConcrete,
                S("pista", 0f, 0f, 6f, 6f, "Pista"), S("tarima", 0f, 0f, 6f, 4f, "Escenario"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa del sonido"), S("amplificador", 0f, 0f, 1f, 0.5f, "Amplificador")),
            new Lot("Clínica de Mapo", "Hospital", "Clínica de Mapo (rodillas y tobillos de bailarinas)", 14f, 10f, SeoulConcrete,
                S("camilla", -3f, 2f, 1f, 2f, "Camilla"), S("cama", 3f, 2f, 1.2f, 2f, "Cama"), S("armario", 0f, -3f, 1.2f, 0.6f, "Medicinas")),
        }, new[]
        {
            new Lot("Piso de Mapo", "Casa", "Piso de Mapo", 10f, 9f, SeoulConcrete),
            new Lot("Cafetería", "Restaurante", "Cafetería de Hapjeong", 10f, 8f, SeoulConcrete, S("barra", 0f, 0f, 2f, 0.8f, "Barra"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa")),
            new Lot("Tienda de 24 horas", "Mercado", "Tienda de 24 horas", 10f, 8f, SeoulConcrete, S("puesto", 0f, 0f, 2f, 1f, "Caja"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
            new Lot("Taller de sonido", "Taller", "Taller de reparación de instrumentos", 12f, 10f, SeoulConcrete, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco"), S("armario", 0f, 0f, 1.2f, 0.6f, "Cuerdas y válvulas")),
        }, C(0.5f, 0.5f, 0.5f));
        int n = 0;
        n += W(t, "Productor de YG", s[0], "", s[3], s[1], null);
        n += W(t, "Técnica de sonido de YG", s[0], "", s[3], s[1], null);
        n += W(t, "Coreógrafa de la agencia", s[1], "maestro", s[3], s[1], null);
        n += W(t, "Cocinera de la agencia", s[3], "cocinero", s[3], s[1], null);
        n += W(t, "Profesora de hangul", s[4], "sabio", s[3], s[1], null);
        n += W(t, "Vendedora de Mangwon", s[5], "comerciante", s[3], null, null);
        n += W(t, "Masajista del jjimjilbang", s[6], "", s[3], null, null);
        n += W(t, "Doctora de Mapo", s[8], "medico", s[3], s[1], null);
        n += Townsfolk(t, s, 9, "Vecina de Hapjeong", "Vecino de Hapjeong", s[3], s[1]);
        return n;
    }

    // ══ KINGSTON (1978): el Estadio Nacional, el Government Yard de First Street, el patio de Joe Higgs, Studio One, la casa de Hope Road, Coronation Market, un
    //    cookshop, el Kingston Public Hospital y el campo de fútbol (Marley jugaba a diario); casas de bloque y zinc, talleres y tiendas.
    static int KingstonTown(Transform t)
    {
        var s = Town(t, "Kingston", new[]
        {
            new Lot("Estadio Nacional", "Sala de baile", "Estadio Nacional de Kingston (el One Love Peace Concert)", 22f, 16f, YardBlock,
                S("pista", 0f, 0f, 6f, 6f, "Campo: el público baila"), S("tarima", 0f, 0f, 6f, 4f, "Escenario"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa del sonido"), S("amplificador", 0f, 0f, 1f, 0.5f, "Amplificadores"), S("bateria", 0f, 0f, 1.6f, 1.6f, "Batería")),
            new Lot("Government Yard de First Street", "Casa", "Government Yard de First Street (Trench Town)", 14f, 12f, YardBlock,
                S("cama", -4f, 2f, 1.2f, 2f, "Cama"), S("cama", 0f, 2f, 1.2f, 2f, "Cama"), S("cama", 4f, 2f, 1.2f, 2f, "Cama"), S("lavadero", 0f, -3f, 1.4f, 0.8f, "Pila común del patio")),
            new Lot("Patio de Joe Higgs", "Escuela", "Patio de Joe Higgs (clases de armonía)", 14f, 10f, YardBlock,
                S("pizarra", 0f, 3.5f, 2f, 0.4f, "Tablas con las voces"), S("mesa", 0f, 0f, 2f, 1f, "Mesa de guitarras"), S("estante", -5f, -3f, 0.8f, 1.6f, "Discos")),
            new Lot("Studio One", "Centro de tecnología", "Studio One (Brentford Road)", 14f, 10f, YardBlock,
                S("consola", -3f, 2f, 1.6f, 0.8f, "Mesa de Coxsone Dodd"), S("mesa_mezclas", 3f, 2f, 1.6f, 0.7f, "Magnetófono"), S("microfono", 0f, -3f, 0.6f, 0.6f, "Micrófono"), S("piano", -4f, -3f, 1.6f, 1f, "Piano del estudio")),
            new Lot("Casa de Hope Road", "Casa", "Casa de Bob Marley y sede de Tuff Gong (56 Hope Road)", 14f, 12f, YardBlock,
                S("cama", -3f, 2f, 1.4f, 2f, "Cama"), S("cama", 2f, 2f, 1.2f, 2f, "Cama"), S("mesa_mezclas", 4f, -2f, 1.6f, 0.7f, "Estudio de Tuff Gong")),
            new Lot("Coronation Market", "Mercado", "Coronation Market", 18f, 12f, YardBlock,
                S("puesto", -5f, 2f, 2f, 1f, "Puesto de ackee y árbol del pan"), S("puesto", 0f, 2f, 2f, 1f, "Puesto de ñame y plátanos"), S("puesto", 5f, 2f, 2f, 1f, "Puesto de telas")),
            new Lot("Cookshop de Trench Town", "Restaurante", "Cookshop de Trench Town (comida ital y pescado)", 12f, 10f, YardBlock,
                S("fogon", -3f, 2f, 1.6f, 0.8f, "Coal pot"), S("mesa", 2f, -1f, 2f, 1f, "Mesa con bancos"), S("barra", 3f, 3f, 2f, 0.8f, "Barra de jugos")),
            new Lot("Kingston Public Hospital", "Hospital", "Kingston Public Hospital", 16f, 12f, YardBlock,
                S("camilla", -3f, 2f, 1f, 2f, "Camilla"), S("cama", 3f, 2f, 1.2f, 2f, "Cama"), S("armario", 0f, -3f, 1.2f, 0.6f, "Medicinas")),
            new Lot("Campo de fútbol", "Centro de entrenamiento", "Campo de fútbol y atletismo junto al estadio", 16f, 18f, YardBlock, TrainingKit("estadio")),
        }, new[]
        {
            new Lot("Casa de Kingston", "Casa", "Casa de bloque y zinc", 10f, 9f, YardBlock),
            new Lot("Tienda de patio", "Mercado", "Tienda de la esquina", 10f, 8f, YardBlock, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
            new Lot("Taller de mecánica", "Taller", "Taller de mecánica", 12f, 10f, YardBlock, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco"), S("armario", 0f, 0f, 1.2f, 0.6f, "Recambios")),
            new Lot("Sound system", "Sala de baile", "Sound system del barrio", 12f, 10f, YardBlock, S("pista", 0f, 0f, 4f, 4f, "Patio de baile"), S("mesa", 0f, 0f, 1.6f, 1f, "Tocadiscos"), S("amplificador", 0f, 0f, 1f, 0.5f, "Altavoces")),
        }, C(0.5f, 0.46f, 0.4f));
        int n = 0;
        n += W(t, "Técnico de sonido del estadio", s[0], "", s[6], s[8], null);
        n += W(t, "Músico del patio", s[2], "maestro", s[6], s[8], s[1]);
        n += W(t, "Ingeniero de Studio One", s[3], "", s[6], s[8], s[1]);
        n += W(t, "Vendedora de ñame", s[5], "comerciante", s[6], null, s[1]);
        n += W(t, "Cocinera del cookshop", s[6], "cocinero", s[6], null, null);
        n += W(t, "Enfermera del Public Hospital", s[7], "enfermera", s[6], null, null);
        n += W(t, "Entrenador de fútbol", s[8], "maestro", s[6], s[8], null);
        n += Townsfolk(t, s, 9, "Vecina de Kingston", "Vecino de Kingston", s[6], s[8]);
        return n;
    }

    // ══ BELFAST (1998): el Waterfront Hall, la oficina del Sí, Maysfield, Queen's, el Royal Victoria, una casa de Falls Road y otra de Shankill Road, Harland & Wolff y el Crown
    //    Liquor Saloon; casas adosadas de ladrillo, tiendas y talleres.
    static int BelfastTown(Transform t)
    {
        var s = Town(t, "Belfast", new[]
        {
            new Lot("Waterfront Hall", "Sala de baile", "Waterfront Hall (concierto del Sí, 19 de mayo de 1998)", 22f, 16f, BelfastBrick,
                S("pista", 0f, 0f, 6f, 6f, "Patio de butacas"), S("tarima", 0f, 0f, 6f, 4f, "Escenario"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa del sonido"), S("amplificador", 0f, 0f, 1f, 0.5f, "Amplificadores"), S("bateria", 0f, 0f, 1.6f, 1.6f, "Batería")),
            new Lot("Oficina del Sí", "Taller de papeles", "Oficina de la campaña del Sí", 14f, 10f, BelfastBrick,
                S("mesa", 0f, 2f, 2.4f, 1f, "Mesa de la campaña"), S("armario", 3f, -3f, 1.2f, 0.6f, "Carteles y folletos")),
            new Lot("Maysfield Leisure Centre", "Centro de entrenamiento", "Maysfield Leisure Centre", 16f, 14f, BelfastBrick, TrainingKit("gimnasio_moderno")),
            new Lot("Queen's University", "Escuela", "Queen's University", 20f, 14f, BelfastBrick,
                S("pizarra", 0f, 4f, 2.4f, 0.4f, "Pizarra"), S("mesa", 0f, 0f, 2.4f, 1f, "Mesa de estudio"), S("estante", -6f, -3f, 1.2f, 2f, "Biblioteca")),
            new Lot("Royal Victoria Hospital", "Hospital", "Royal Victoria Hospital", 18f, 12f, BelfastBrick,
                S("camilla", -4f, 2f, 1f, 2f, "Camilla"), S("cama", 0f, 2f, 1.2f, 2f, "Cama"), S("cama", 4f, 2f, 1.2f, 2f, "Cama"), S("armario", 0f, -3f, 1.2f, 0.6f, "Medicinas")),
            new Lot("Casa de Falls Road", "Casa", "Casa de Falls Road", 12f, 10f, BelfastBrick, S("cama", -2f, 2f, 1.4f, 2f, "Cama"), S("cama", 3f, 2f, 1.2f, 2f, "Cama de la hija")),
            new Lot("Casa de Shankill Road", "Casa", "Casa de Shankill Road", 12f, 10f, BelfastBrick, S("cama", -2f, 2f, 1.4f, 2f, "Cama"), S("cama", 3f, 2f, 1.2f, 2f, "Cama del hijo")),
            new Lot("Harland & Wolff", "Taller", "Astilleros Harland & Wolff", 20f, 14f, BelfastBrick,
                S("banco_taller", -5f, 3f, 2.4f, 1.2f, "Banco de soldar"), S("torno", 0f, 3f, 1.6f, 1f, "Torno"), S("armario", 5f, 3f, 1.6f, 0.8f, "Cascos y herramientas")),
            new Lot("Crown Liquor Saloon", "Restaurante", "Crown Liquor Saloon", 14f, 10f, BelfastBrick,
                S("cocina", -3f, 2f, 2f, 0.8f, "Cocina (estofado)"), S("barra", 2f, 2f, 2.4f, 0.8f, "Barra de madera"), S("mesa", 0f, -3f, 1.6f, 1f, "Reservado")),
        }, new[]
        {
            new Lot("Casa de Belfast", "Casa", "Casa adosada de ladrillo", 10f, 9f, BelfastBrick),
            new Lot("Tienda de barrio", "Mercado", "Tienda de barrio", 10f, 8f, BelfastBrick, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
            new Lot("Taller de lino", "Taller", "Taller de lino", 12f, 10f, BelfastBrick, S("telar", 0f, 0f, 1.6f, 1f, "Telar"), S("armario", 0f, 0f, 1.2f, 0.6f, "Hilos")),
            new Lot("Pub de barrio", "Restaurante", "Pub de barrio", 10f, 8f, BelfastBrick, S("barra", 0f, 0f, 2f, 0.8f, "Barra"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa")),
        }, C(0.42f, 0.41f, 0.41f));
        int n = 0;
        n += W(t, "Técnica de sonido del Waterfront", s[0], "", s[8], s[2], null);
        n += W(t, "Voluntaria del Sí", s[1], "", s[8], s[2], s[5]);
        n += W(t, "Voluntario del Sí", s[1], "", s[8], s[2], s[6]);
        n += W(t, "Monitora de Maysfield", s[2], "maestro", s[8], s[2], null);
        n += W(t, "Profesor de Queen's", s[3], "sabio", s[8], s[2], null);
        n += W(t, "Enfermera del Royal Victoria", s[4], "enfermera", s[8], null, s[5]);
        n += W(t, "Soldador de Harland & Wolff", s[7], "", s[8], null, s[6]);
        n += W(t, "Camarero del Crown", s[8], "cocinero", s[8], null, null);
        n += Townsfolk(t, s, 9, "Vecina de Belfast", "Vecino de Belfast", s[8], s[2]);
        return n;
    }

    // ══ LONG ISLAND (1989): la Universidad Adelphi y su radio WBAU, el estudio de South Franklin Street, la casa de los Ridenhour y la de Flavor Flav, la cancha, el Nassau Coliseum, un
    //    diner, el hospital y la iglesia; casas de tablas, tiendas y talleres.
    static int LongIslandTown(Transform t)
    {
        var s = Town(t, "Long Island", new[]
        {
            new Lot("Universidad Adelphi", "Escuela", "Universidad Adelphi (Garden City)", 20f, 14f, SuburbFrame,
                S("pizarra", 0f, 4f, 2.4f, 0.4f, "Pizarra"), S("mesa", 0f, 0f, 2.4f, 1f, "Mesa de dibujo"), S("estante", -6f, -3f, 1.2f, 2f, "Biblioteca")),
            new Lot("WBAU", "Centro de tecnología", "WBAU, la radio de Adelphi", 14f, 10f, SuburbFrame,
                S("consola", -3f, 2f, 1.6f, 0.8f, "Mesa de la emisora"), S("microfono", 3f, 2f, 0.6f, 0.6f, "Micrófono de locución"), S("estante", 0f, -3f, 1.2f, 2f, "Discoteca de la radio")),
            new Lot("510 South Franklin", "Centro de tecnología", "510 South Franklin Street (Hempstead): el estudio del Bomb Squad", 14f, 10f, SuburbFrame,
                S("consola", -3f, 2f, 1.6f, 0.8f, "Sampler"), S("mesa_mezclas", 3f, 2f, 1.6f, 0.7f, "Magnetófono de pistas"), S("microfono", 0f, -3f, 0.6f, 0.6f, "Micrófono de voz"), S("amplificador", -4f, -3f, 1f, 0.5f, "Altavoces")),
            new Lot("Casa de los Ridenhour", "Casa", "Casa de la familia Ridenhour (Roosevelt)", 12f, 10f, SuburbFrame,
                S("cama", -3f, 2f, 1.4f, 2f, "Cama de los padres"), S("cama", 2f, 2f, 1.2f, 2f, "Cama de Carlton"), S("estante", 4f, -2f, 0.8f, 1.6f, "Discos y libros")),
            new Lot("Casa del barrio sur", "Casa", "Casa del barrio sur de Roosevelt", 12f, 10f, SuburbFrame, S("cama", 0f, 2f, 1.4f, 2f, "Cama")),
            new Lot("Cancha de Roosevelt", "Centro de entrenamiento", "Cancha de baloncesto de Roosevelt", 16f, 14f, SuburbFrame, TrainingKit("gimnasio_moderno")),
            new Lot("Nassau Coliseum", "Sala de baile", "Nassau Coliseum (Uniondale)", 20f, 16f, SuburbFrame,
                S("pista", 0f, 0f, 6f, 6f, "Pista"), S("tarima", 0f, 0f, 6f, 4f, "Escenario"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa del sonido"), S("amplificador", 0f, 0f, 1f, 0.5f, "Amplificadores")),
            new Lot("Diner de Nassau Road", "Restaurante", "Diner de Nassau Road", 14f, 10f, SuburbFrame,
                S("cocina", -3f, 2f, 2f, 0.8f, "Plancha del diner"), S("barra", 2f, 2f, 2.4f, 0.8f, "Barra con taburetes"), S("mesa", 0f, -3f, 1.6f, 1f, "Mesa de reservado")),
            new Lot("Hospital del condado", "Hospital", "Hospital del condado de Nassau", 16f, 12f, SuburbFrame,
                S("camilla", -3f, 2f, 1f, 2f, "Camilla"), S("cama", 3f, 2f, 1.2f, 2f, "Cama"), S("armario", 0f, -3f, 1.2f, 0.6f, "Medicinas")),
            new Lot("Iglesia de Roosevelt", "Templo", "Iglesia baptista de Roosevelt", 12f, 10f, SuburbFrame, S("altar", 0f, 2f, 2f, 1f, "Púlpito"), S("banco", 0f, -1f, 2f, 0.6f, "Banco"), S("piano", 3f, -2f, 1.6f, 1f, "Piano del coro")),
        }, new[]
        {
            new Lot("Casa de Roosevelt", "Casa", "Casa de tablas de Roosevelt", 10f, 9f, SuburbFrame),
            new Lot("Tienda de Nassau Road", "Mercado", "Tienda de Nassau Road", 10f, 8f, SuburbFrame, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
            new Lot("Barbería", "Taller", "Barbería", 10f, 8f, SuburbFrame, S("banco_taller", 0f, 0f, 2f, 0.9f, "Sillón y tocador"), S("armario", 0f, 0f, 1.2f, 0.6f, "Tijeras y máquinas")),
            new Lot("Disquería", "Mercado", "Tienda de discos de Hempstead", 10f, 8f, SuburbFrame, S("puesto", 0f, 0f, 2f, 1f, "Mostrador de discos"), S("estante", 0f, 0f, 0.8f, 1.6f, "Discos")),
        }, C(0.45f, 0.45f, 0.46f));
        int n = 0;
        n += W(t, "Profesora de diseño de Adelphi", s[0], "sabio", s[7], s[5], null);
        n += W(t, "Locutora de WBAU", s[1], "", s[7], s[5], s[4]);
        n += W(t, "Ingeniero del Bomb Squad", s[2], "", s[7], s[5], null);
        n += W(t, "Entrenador de la cancha", s[5], "maestro", s[7], s[5], null);
        n += W(t, "Técnica del Coliseum", s[6], "", s[7], null, null);
        n += W(t, "Cocinera del diner", s[7], "cocinero", s[7], null, s[4]);
        n += W(t, "Enfermera del condado", s[8], "enfermera", s[7], null, null);
        n += Townsfolk(t, s, 10, "Vecina de Roosevelt", "Vecino de Roosevelt", s[7], s[5]);
        return n;
    }

    // ══ LA PLAZA DE LAS CANCIONES: seis rincones de época (Lot.Era: su año y su región) y el pueblo inventado que los une (sala de conciertos, escuela de música, comedor, gimnasio, hospital, casas).
    static int SongsTown(Transform t)
    {
        var s = Town(t, "Grandes aportes de las canciones", new[]
        {
            new Lot("Escuela Highlander", "Escuela", "Escuela Highlander (Tennessee, 1960): We Shall Overcome", 14f, 10f, SuburbFrame,
                S("pizarra", 0f, 3.5f, 2f, 0.4f, "Pizarra del taller de líderes"), S("mesa", 0f, 0f, 2f, 1f, "Mesa del taller"), S("estante", -5f, -3f, 0.8f, 1.6f, "Himnarios")).Era(1960, "Norteamerica"),
            new Lot("Arrozal de las mondine", "Huerto", "Arrozal de las mondine (Vercelli, 1906): Bella Ciao", 16f, 12f, SuburbFrame,
                S("bancal", -3f, 0f, 4f, 6f, "Arrozal inundado"), S("pozo", 4f, 2f, 1.2f, 1.2f, "Compuerta del agua")).Era(1906, "EuropaUrbana"),
            new Lot("Tittenhurst Park", "Centro de tecnología", "Tittenhurst Park (Ascot, 1971): Imagine", 14f, 10f, BelfastBrick,
                S("consola", -3f, 2f, 1.6f, 0.8f, "Ascot Sound Studios"), S("mesa_mezclas", 3f, 2f, 1.6f, 0.7f, "Magnetófono"), S("piano", 0f, -3f, 1.6f, 1f, "Piano vertical Steinway")).Era(1971, "IslasBritanicas"),
            new Lot("Comunidad de Ginsberg", "Escuela", "Comunidad de Ginsberg (Sudáfrica, 1977): Biko", 14f, 10f, YardBlock,
                S("pizarra", 0f, 3.5f, 2f, 0.4f, "Pizarra de la Conciencia Negra"), S("mesa", 0f, 0f, 2f, 1f, "Mesa de los boletines"), S("estante", -5f, -3f, 0.8f, 1.6f, "Libros")).Era(1977, "Sudafrica"),
            new Lot("Patio de Trench Town", "Sala de baile", "Patio de Trench Town (Kingston, 1980): Redemption Song", 14f, 10f, YardBlock,
                S("pista", 0f, 0f, 4f, 4f, "Patio"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa de dominó"), S("amplificador", 0f, 0f, 1f, 0.5f, "Guitarra acústica")).Era(1980, "Jamaica"),
            new Lot("Pueblo de los Andes", "Mercado", "Pueblo de los Andes (Perú, 2011): Latinoamérica", 14f, 10f, MountainTimber,
                S("puesto", -3f, 2f, 2f, 1f, "Puesto de tejidos"), S("puesto", 3f, 2f, 2f, 1f, "Puesto de maíz y papa"), S("telar", 0f, -3f, 1.6f, 1f, "Telar de cintura")).Era(2011, "Hispanoamerica"),
            new Lot("Sala de conciertos", "Sala de baile", "Sala de conciertos de la Plaza de las Canciones", 18f, 14f, CaribeBlock,
                S("pista", 0f, 0f, 6f, 6f, "Patio de butacas"), S("tarima", 0f, 0f, 6f, 4f, "Escenario"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa del sonido"), S("piano", 0f, 0f, 1.6f, 1f, "Piano de cola"), S("bateria", 0f, 0f, 1.6f, 1.6f, "Batería")),
            new Lot("Escuela de música", "Escuela", "Escuela de música (historia de las canciones)", 14f, 10f, CaribeBlock,
                S("pizarra", 0f, 3.5f, 2f, 0.4f, "Pizarra pautada"), S("mesa", 0f, 0f, 2f, 1f, "Atriles"), S("estante", -5f, -3f, 0.8f, 1.6f, "Partituras y discos")),
            new Lot("Comedor de la plaza", "Restaurante", "Comedor de la plaza", 14f, 10f, CaribeBlock,
                S("cocina", -3f, 2f, 2f, 0.8f, "Cocina"), S("mesa", 2f, -1f, 2f, 1f, "Mesas largas"), S("barra", 3f, 3f, 2f, 0.8f, "Barra")),
            new Lot("Casa de la cronista", "Casa", "Casa de la cronista", 12f, 10f, CaribeBlock, S("cama", -2f, 2f, 1.4f, 2f, "Cama"), S("estante", 3f, 2f, 0.8f, 1.6f, "Discos y libros")),
            new Lot("Gimnasio de la plaza", "Centro de entrenamiento", "Gimnasio de la plaza (voz, baile y cuerpo)", 16f, 14f, CaribeBlock, TrainingKit("gimnasio_moderno")),
            new Lot("Casa de salud", "Hospital", "Casa de salud de la plaza", 14f, 10f, CaribeBlock,
                S("camilla", -3f, 2f, 1f, 2f, "Camilla"), S("cama", 3f, 2f, 1.2f, 2f, "Cama"), S("armario", 0f, -3f, 1.2f, 0.6f, "Medicinas")),
        }, new[]
        {
            new Lot("Casa de la plaza", "Casa", "Casa de la Plaza de las Canciones", 10f, 9f, CaribeBlock),
            new Lot("Tienda de discos", "Mercado", "Tienda de discos de segunda mano", 10f, 8f, CaribeBlock, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Discos")),
            new Lot("Taller de instrumentos", "Taller", "Taller de instrumentos", 12f, 10f, CaribeBlock, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco del luthier"), S("armario", 0f, 0f, 1.2f, 0.6f, "Maderas y cuerdas")),
        }, C(0.55f, 0.54f, 0.5f));
        int n = 0;
        n += W(t, "Guía de Highlander", s[0], "sabio", s[8], s[10], null);
        n += W(t, "Hortelana del arrozal", s[1], "hortelano", s[8], s[10], null);
        n += W(t, "Técnico de Ascot", s[2], "", s[8], s[10], null);
        n += W(t, "Maestra de Ginsberg", s[3], "sabio", s[8], s[10], null);
        n += W(t, "Vendedor de los Andes", s[5], "comerciante", s[8], s[10], null);
        n += W(t, "Técnica de la sala de conciertos", s[6], "", s[8], s[10], null);
        n += W(t, "Profesora de música", s[7], "sabio", s[8], s[10], s[9]);
        n += W(t, "Cocinero de la plaza", s[8], "cocinero", s[8], s[10], null);
        n += W(t, "Monitora del gimnasio", s[10], "maestro", s[8], s[10], null);
        n += W(t, "Enfermera de la plaza", s[11], "enfermera", s[8], s[10], null);
        n += Townsfolk(t, s, 12, "Vecina de la plaza", "Vecino de la plaza", s[8], s[10]);
        return n;
    }

    // ══FIN DE LOS PUEBLOS══
}
