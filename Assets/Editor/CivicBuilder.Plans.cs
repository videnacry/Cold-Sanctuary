using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// LOS PUEBLOS DESDE SU MAPA REAL (docs/microcosmos-pueblos-mapas.md; Berón 2026-11-03: «sería ideal acceder a mapas de esos
/// pueblos, de modo que se amplía su estructura y así entran actividades del santuario … y estructuras naturales como ríos, lagos,
/// montañas, etc., con su estilo de la época-lugar y adaptación para humanos-insectos y humanos»).
/// Cada historia de era avanzada tiene su PLANO (<see cref="PlanText"/>): las calles de su trazado real, su río, su lago, sus
/// colinas y montes, sus campos y su muralla, y DÓNDE estaban sus edificios (los de la historia y los del pueblo), en metros reales
/// con el claro de la historia en (0, 0), x = este, z = norte. Los insectos-humanos miden como una persona, así que el plano va a
/// escala 1:1 en su barrio (AreaMicrocosmos.TownScale: el piso × 60, de 1,4 a 2,8 km); si no cabe, se encoge entero (y se dice).
/// Lo que queda fuera del barrio (montes lejanos, el mar) se recorta.
/// Pasos (los llama <see cref="Civilize"/>):
///   1. <see cref="BuildPlan"/>: el PAISAJE («Paisaje de …»: río, lago, colinas, monte, campos, muralla, mar) y las CALLES del plano
///      («Calles de … (plano)»), antes que nada: los solares (<see cref="Lots"/>) los esquivan.
///   2. los edificios de la historia, en su sitio del plano (<see cref="PlanPoint"/>);
///   3. <see cref="PlanCivic"/>: los EDIFICIOS DEL PUEBLO que pidió Berón (plaza, mercado, escuela, estación, comisaría, bomberos,
///      museo, guardería, baños, hotel, carpintería, obra, cuartel, centro de tecnología…), con su nombre y su estilo de época y lugar
///      (<see cref="MicroStyles.CivicName"/>: solo los que existían) y su interacción para humanos e insectos-humanos;
///   4. <see cref="PlanFill"/>: las casas y talleres del pueblo A LO LARGO DE SUS CALLES (de frente a la calle), hasta el tope.
/// </summary>
internal static partial class CivicBuilder
{
    internal const string PlanLandscape = "Paisaje de ", PlanStreets = "(plano)", OutpostPrefix = "Mirador";
    /// <summary>Tope de edificios de un pueblo (rendimiento): historia + pueblo + relleno.</summary>
    const int PlanMaxSites = 150;

    enum FK { Calle, Rio, Lago, Colina, Monte, Campo, Muralla, Mar, Acantilado }
    sealed class Feat { public FK k; public string name; public List<Vector2> p = new List<Vector2>(); public float a, b; public bool snow, dry; }
    sealed class TownPlan
    {
        public string town;
        public readonly List<Feat> feats = new List<Feat>();
        public readonly Dictionary<int, Vector2> hitos = new Dictionary<int, Vector2>();
        public readonly List<(MicroStyles.Civic c, Vector2 at)> civic = new List<(MicroStyles.Civic, Vector2)>();
        public int fill = 60; public Vector2 fillNear;
        public float scale = 1f;
    }

    static TownPlan _plan; static bool _civicDone, _fillDone; static Style _planStyle;
    // Un pueblo de dos barrios (Bruce Lee: Kowloon al oeste, Seattle al este): la región y el estilo de cada punto del plano.
    static System.Func<Vector2, (string region, Style st)> _quarterAt;

    // ══ LOS PLANOS (fuentes en docs/microcosmos-pueblos-mapas.md) ═══════════════════════════════════════════════════════
    // Formato, una línea por elemento («|» separa campos; los puntos «x z» separados por comas; metros reales):
    //   calle Nombre | ancho | x z, x z…      rio / canal Nombre | ancho | x z, x z…     muralla Nombre | – | x z, x z…
    //   lago Nombre | rx rz | x z             colina / monte Nombre | radio alto | x z    campo / mar Nombre | ancho fondo | x z
    //   hito i | x z  (el edificio i de la historia)     Tipo | x z  (un edificio del pueblo: MicroStyles.Civic)
    //   relleno | n | x z  (n casas y talleres a lo largo de las calles, empezando cerca de x z)
    static readonly Dictionary<string, string> PlanText = new Dictionary<string, string>
    {
        // CROTONA (~530 a. C.): retícula de plateiai E-O junto al mar Jonio (este), el río Esaro al norte, la acrópolis sobre el
        // cabo del puerto; el templo de Hera Lacinia en el cabo Colonna (12 km al SE: en el borde). (Spadea 1997; Wikipedia «Crotone»)
        ["Crotona"] = @"
calle Plateia mayor | 10 | -520 0, 460 0
calle Plateia norte | 8 | -520 130, 460 130
calle Plateia sur | 8 | -520 -130, 460 -130
calle Stenopos 1 | 6 | -300 -300, -300 300
calle Stenopos 2 | 6 | -150 -300, -150 300
calle Stenopos 3 | 6 | 150 -300, 150 300
calle Stenopos 4 | 6 | 300 -300, 300 300
calle Camino del cabo Lacinio | 6 | 300 -130, 560 -460
rio Esaro | 25 | -760 380, -200 330, 300 300, 560 280
mar Mar Jonio | 500 2400 | 820 0
colina Acrópolis del puerto | 140 25 | 360 120
acantilado Costa del cabo | 18 | 600 -520, 610 -250, 600 -60
monte La Sila | 600 1600 | -1250 0 | nieve
hito 0 | 40 60
hito 1 | -150 -80
hito 2 | 520 -440
hito 3 | 90 -60
hito 4 | -60 70
Plaza | 0 -75
Mercado | -120 45
Pileta | 60 115
Parque | -360 200
Piscina | 200 -160
Entrenamiento | -260 -190
Estacion | 470 60
Hotel | 240 60
Museo | 470 -330
Investigacion | 150 200
Policia | -40 -160
Milicia | -320 0
Monumento | 40 -115
Carpinteria | -220 120
Obra | 300 -240
Baile | -180 250
Guarderia | 120 -210
Tecnologia | -100 180
Estudio | 220 120
relleno | 70 | 0 0",
        // MENFIS (~450 a. C.): el Nilo al este (el puerto Peru-nefer), el recinto de Ptah en el centro con su lago sagrado, el
        // coloso de Ramsés II, los campos de la crecida y la meseta de Saqqara al oeste. (Jeffreys, The Survey of Memphis, EES;
        // Heródoto II.99–101)
        ["Menfis"] = @"
rio Nilo | 260 | 430 -1000, 400 0, 450 1000
canal Canal del puerto | 30 | 280 -50, 430 -50
lago Lago sagrado de Ptah | 45 25 | 90 -140
colina Meseta de Saqqara | 450 40 | -900 0 | seco
campo Campos de la crecida (norte) | 220 300 | 220 520
campo Campos de la crecida (sur) | 220 300 | 220 -560
campo Campos de trigo emmer | 260 200 | -480 420
calle Avenida procesional | 10 | -520 -40, 300 -40
calle Calle del templo | 8 | -150 -400, -150 400
calle Calle del puerto | 8 | 150 -400, 150 400
calle Calle norte | 8 | -400 200, 300 200
calle Calle sur | 8 | -400 -250, 300 -250
hito 0 | -70 70
hito 1 | 310 -60
hito 2 | 200 380
hito 3 | -100 -70
hito 4 | 70 -80
Plaza | 0 115
Mercado | 220 100
Pileta | -190 30
Parque | -170 160
Piscina | 130 -230
Estacion | 330 130
Museo | -260 100
Investigacion | -260 -90
Policia | 100 180
Milicia | -350 -260
Monumento | -20 160
Carpinteria | 150 -330
Obra | -300 260
Baile | -80 -200
Guarderia | 40 -200
Taller | 250 -150
Entrenamiento | -220 -320
relleno | 70 | 0 0",
        // BAGDAD (~820): la Ciudad Redonda de al-Mansur (762; ~2,6 km de diámetro: aquí su muralla, a escala) con sus cuatro
        // avenidas a las puertas de Kufa, Basora, Jorasán y Siria; el Tigris al este con el puente de barcas; al sur, al-Karj, el
        // barrio del zoco, entre los canales Sarat e Isa. (Lassner 1970, The Topography of Baghdad; Le Strange 1900)
        ["Bagdad"] = @"
rio Tigris | 220 | 760 1000, 700 0, 780 -1000
canal Canal Sarat | 15 | -950 -150, 650 -150
canal Canal Isa | 20 | -950 -460, 650 -460
muralla Muralla de la Ciudad Redonda | 0 | -300 1000, 0 920, 250 750, 330 450, 250 150, 0 -20, -300 -100, -600 -20, -850 150, -930 450, -850 750, -600 920, -300 1000
calle Avenida de la puerta de Jorasán | 12 | -300 450, 190 800
calle Avenida de la puerta de Basora | 12 | -300 450, 190 100
calle Avenida de la puerta de Kufa | 12 | -300 450, -790 100
calle Avenida de la puerta de Siria | 12 | -300 450, -790 800
calle Puente de barcas | 10 | 420 300, 950 300
calle Calle mayor de al-Karj | 10 | -650 -60, 650 -60
calle Calle de los papeleros | 8 | -650 -300, 650 -300
calle Calle del cadí | 8 | -400 -420, -400 100
calle Calle del zoco | 8 | -150 -420, -150 100
calle Calle del bimaristán | 8 | 150 -420, 150 100
calle Calle del puerto | 8 | 420 -420, 420 300
hito 0 | -200 140
hito 1 | 120 -110
hito 2 | -120 -120
hito 3 | 300 -230
hito 4 | 80 90
hito 5 | -360 -210
Plaza | -300 450
Pileta | 40 -100
Parque | 520 160
Piscina | -40 170
Estacion | -650 -40
Mercado | 220 40
Investigacion | 500 620
Policia | -150 320
Milicia | -520 260
Hotel | 260 -30
Tecnologia | -250 -60
Carpinteria | 350 60
Obra | -460 -320
Baile | 180 170
Guarderia | -50 -260
Estudio | -360 40
Entrenamiento | -620 600
Taller | 430 -90
relleno | 80 | 0 0",
        // WOOLSTHORPE (1666): la casa solariega, el granero y el huerto de manzanos; el río Witham al este y, al otro lado,
        // Colsterworth con su iglesia y la Great North Road; los campos abiertos en franjas. (National Trust; OS 1:2500 de 1887)
        ["Woolsthorpe"] = @"
rio Witham | 14 | 420 900, 400 0, 440 -900
calle Water Lane (a Colsterworth) | 6 | 0 -25, 880 -45
calle Camino de Woolsthorpe | 5 | -60 -320, -60 320
calle Great North Road | 9 | 960 -1000, 950 1000
calle Calle de Colsterworth | 6 | 700 120, 950 120
campo Campo abierto (franjas) | 300 200 | -420 300
campo Campo abierto (franjas, sur) | 300 200 | -420 -300
campo Prado del Witham | 150 220 | 220 360
campo Campo de cebada | 150 160 | 220 -360
colina Loma de Woolsthorpe | 420 18 | -760 0
hito 0 | -50 45
hito 1 | -95 -45
hito 2 | -210 160
hito 3 | 880 70
hito 4 | 920 -90
Plaza | 820 -20
Pileta | 760 70
Mercado | 860 160
Estudio | 780 170
Policia | 900 230
Estacion | 990 200
Carpinteria | 700 -160
Obra | 650 110
Guarderia | 700 260
Mecanica | 750 -260
Investigacion | -160 -120
relleno | 30 | 700 0",
        // BERKELEY (1796): la calle mayor, Canonbury y Salter Street; la iglesia de Santa María con The Chantry junto al
        // cementerio; el castillo al sureste; el Little Avon y el estuario del Severn al oeste. (Jenner Museum; VCH Glos. vol. 5)
        ["Berkeley"] = @"
calle High Street | 8 | 60 -160, 60 360
calle Canonbury Street | 7 | 60 120, 420 120
calle Salter Street | 7 | -220 -40, 60 -40
calle Marybrook Street | 7 | 150 -100, 150 -320
calle Camino de la granja | 5 | -320 200, 60 200
rio Little Avon | 15 | -520 -620, 100 -360, 620 -300
mar Estuario del Severn | 700 2600 | -1300 0
colina Loma del castillo | 120 10 | 160 -190
campo Prados de pasto | 220 180 | -350 -350
hito 0 | -55 45
hito 1 | -300 210
hito 2 | 120 220
hito 3 | 40 -75
hito 4 | 90 165
Milicia | 160 -200
Plaza | 80 300
Mercado | 120 335
Pileta | -60 140
Parque | -210 -150
Estudio | 210 100
Policia | 190 260
Bomberos | 20 265
Estacion | 60 430
Hotel | 230 30
Carpinteria | -150 300
Obra | 300 380
Guarderia | -120 250
Mecanica | 270 200
Investigacion | -130 50
relleno | 50 | 60 100",
        // ANGOSTURA (1819): la cuadrícula colonial en la orilla sur del Orinoco (aquí, su estrechura), la Plaza Mayor con la
        // catedral y la Casa del Congreso, el Paseo junto al río, la Piedra del Medio y el cerro El Zamuro con su fortín.
        // (Plano de Angostura, 1819, Archivo General de Indias; Lynch 2006)
        ["Angostura"] = @"
rio Orinoco | 600 | -1300 560, 1300 560
colina Piedra del Medio | 25 8 | 100 560
colina Cerro El Zamuro | 150 40 | 260 -500
acantilado Barranca del Orinoco | 12 | -650 240, 0 235, 650 240
calle Paseo Orinoco | 12 | -720 95, 720 95
calle Calle Venezuela | 8 | -720 -90, 720 -90
calle Calle Bolívar | 8 | -720 -180, 720 -180
calle Calle Igualdad | 8 | -270 -320, -270 95
calle Calle Libertad | 8 | -180 -320, -180 95
calle Calle Amor Patrio | 8 | 90 -320, 90 95
calle Calle Constitución | 8 | 180 -320, 180 95
calle Calle Dalla Costa | 8 | 270 -320, 270 95
hito 0 | 50 50
hito 1 | -125 40
hito 2 | -160 -130
hito 3 | -60 210
hito 4 | 60 -55
hito 5 | 230 -150
Plaza | 0 -70
Monumento | 40 -120
Mercado | 190 60
Pileta | -60 -90
Parque | 360 150
Estacion | 300 210
Policia | -90 80
Milicia | 260 -380
Hotel | -210 60
Carpinteria | -310 -100
Obra | 320 -60
Baile | 120 -190
Guarderia | -120 -230
Taller | -260 20
Mecanica | 380 0
Estudio | 150 -60
relleno | 60 | 0 0",
        // SILS MARIA (1881): el pueblo entre el lago de Sils (oeste) y el de Silvaplana (noreste), a la boca del valle de Fex
        // (sur); la península de Chastè; el piz Corvatsch y el piz Lagrev. (Siegfriedkarte 1:50 000, hoja 518, 1878)
        ["Sils Maria"] = @"
lago Lago de Sils | 700 330 | -950 520
lago Lago de Silvaplana | 480 280 | 950 720
rio Fedacla (arroyo de Fex) | 6 | 120 -900, 60 -100, 0 320
monte Piz Corvatsch | 600 1650 | 760 -850 | nieve
monte Piz Lagrev | 520 1360 | -950 -560 | nieve
colina Península de Chastè | 120 30 | -420 420
calle Via da Marias | 7 | -320 0, 320 0
calle Camino de Maloja | 6 | -320 0, -950 160
calle Camino de Silvaplana | 6 | 320 0, 820 420
calle Camino del valle de Fex | 5 | 0 0, 90 -820
hito 0 | 45 40
hito 1 | -45 55
hito 2 | 75 -45
hito 3 | -210 300
hito 4 | -85 -55
Hotel | 160 65
Plaza | -60 115
Pileta | 35 -95
Estudio | -125 125
Estacion | 260 90
Bomberos | 115 -125
Piscina | -460 250
Policia | -150 -65
Carpinteria | 200 -100
Taller | -200 -150
relleno | 30 | 0 0",
        // PARÍS (1898): el Barrio Latino. La rue Lhomond (la ESPCI y su cobertizo), rue Mouffetard, rue d'Ulm, la Montagne
        // Sainte-Geneviève con el Panteón, el boulevard Saint-Michel, Port-Royal, el Jardín del Luxemburgo, el Jardin des Plantes,
        // el Sena al norte y la rue de la Glacière (la casa de los Curie) al suroeste. (Plan de Paris, Alphand 1889; Quinn 1995)
        ["París 1898"] = @"
rio Sena | 160 | -1400 1000, 0 1060, 1400 860
colina Montagne Sainte-Geneviève | 300 25 | -250 260
calle Boulevard Saint-Michel | 30 | -520 -800, -520 1000
calle Rue Saint-Jacques | 12 | -320 -800, -320 1000
calle Rue d'Ulm | 10 | -150 -200, -150 300
calle Rue Lhomond | 8 | -30 -300, -30 250
calle Rue Mouffetard | 8 | 200 -420, 200 400
calle Rue Claude-Bernard | 12 | -600 -160, 400 -160
calle Boulevard de Port-Royal | 30 | -900 -380, 600 -380
calle Rue Soufflot | 15 | -520 300, -200 300
calle Rue Monge | 15 | 300 600, 150 -200
calle Rue de la Glacière | 10 | -800 -900, -800 -380
hito 0 | 45 30
hito 1 | -770 -650
hito 2 | -700 900
hito 3 | 190 95
hito 4 | 150 -60
Plaza | -250 320
Monumento | -300 380
Estacion | 900 700
Parque | -860 350
Museo | 650 450
Investigacion | -420 620
Estudio | -150 380
Policia | 100 250
Bomberos | -450 -420
Mercado | 260 -150
Piscina | 300 900
Hotel | -460 150
Tecnologia | 220 -520
Guarderia | -100 -260
Baile | -620 -440
Entrenamiento | -300 -260
Carpinteria | 360 -300
Obra | 460 100
Mecanica | 500 -260
Milicia | 330 260
Pileta | 60 -125
Edificio | -220 -110
relleno | 90 | 0 0",
        // COLORADO SPRINGS (1899): la estación de Tesla en Knob Hill (este) y el centro de la ciudad de Palmer al oeste, con sus
        // avenidas de 30 m (Pikes Peak Avenue, Cascade, Tejon), el Monument Creek y el ferrocarril, y el Pikes Peak al fondo.
        // (Sanborn Fire Insurance Map, Colorado Springs 1897; Tesla 1978)
        ["Colorado Springs"] = @"
calle Pikes Peak Avenue | 30 | -1100 0, 100 0
calle Colorado Avenue | 25 | -1100 -150, -300 -150
calle Kiowa Street | 20 | -1100 150, -300 150
calle Cascade Avenue | 30 | -800 -420, -800 420
calle Tejon Street | 20 | -650 -420, -650 420
calle Nevada Avenue | 25 | -500 -420, -500 420
calle Camino de Knob Hill | 10 | 0 0, 0 -220
calle Vía del Denver & Rio Grande | 6 | -900 -900, -900 900
rio Monument Creek | 12 | -980 900, -950 0, -1000 -900
monte Pikes Peak | 700 2460 | -1450 -300 | nieve seco
hito 0 | 35 45
hito 1 | -450 -210
hito 2 | -620 115
hito 3 | -880 65
hito 4 | 80 -65
Plaza | -660 210
Policia | -560 -60
Bomberos | -700 -125
Estudio | -620 500
Mercado | -540 60
Pileta | -700 40
Hotel | -790 -60
Tecnologia | -600 -265
Mecanica | -400 150
Carpinteria | -350 -100
Obra | -300 250
Baile | -520 -175
Guarderia | -480 245
Milicia | -250 -300
Entrenamiento | -720 300
Piscina | -850 -255
relleno | 70 | -600 0",
        // BERNA (1905): el casco viejo en la península del Aar: la Kramgasse (la casa de los Einstein, n.º 49) y la Torre del Reloj,
        // Gerechtigkeitsgasse hasta el puente de Nydegg, Marktgasse y Spitalgasse hasta la estación; las calles paralelas
        // (Rathausgasse, Münstergasse…), los puentes de Kirchenfeld (1883) y Kornhaus (1898), el Gurten al sur. (Stadtplan Bern 1905)
        ["Berna"] = @"
rio Aar | 70 | -750 -260, 0 -190, 600 -170, 820 0, 600 190, 0 210, -320 260, -500 900
calle Kramgasse y Gerechtigkeitsgasse | 9 | 0 0, 700 0
calle Marktgasse y Spitalgasse | 9 | 0 0, -650 10
calle Rathausgasse y Postgasse | 7 | 0 70, 650 70
calle Münstergasse y Junkerngasse | 7 | 0 -70, 650 -70
calle Zeughausgasse | 7 | -350 65, 0 70
calle Amthausgasse | 7 | -350 -60, 0 -70
calle Puente de Nydegg | 10 | 700 0, 900 0
calle Puente de Kirchenfeld | 12 | 150 -70, 150 -520
calle Puente del Kornhaus | 12 | -50 70, -100 520
calle Bahnhofplatz y Bubenbergplatz | 20 | -650 10, -720 -150
colina Gurten | 500 340 | 0 -1150
colina Rosengarten | 150 25 | 820 320
hito 0 | 120 18
hito 1 | -600 80
hito 2 | -500 60
hito 3 | 80 -18
hito 4 | 40 22
Plaza | -220 -60
Estacion | -700 40
Museo | 150 -560
Investigacion | -650 200
Policia | 100 120
Bomberos | -300 85
Parque | -450 -120
Pileta | 450 -12
Mercado | -250 40
Hotel | -150 -95
Tecnologia | -400 -40
Guarderia | 250 120
Baile | -150 110
Entrenamiento | -500 -200
Piscina | -300 -230
Carpinteria | 250 -150
Mecanica | -650 300
Milicia | -200 600
Obra | 300 -500
Estudio | 350 120
Monumento | 300 -100
Edificio | -450 -300
relleno | 80 | 0 0",
        // PASADENA (1953): el campus de Caltech (el laboratorio limpio), la cuadrícula de California, Del Mar y Colorado
        // Boulevard, Lake Avenue, el Arroyo Seco al oeste y los montes de San Gabriel al norte. (USGS Pasadena 1:24 000, 1953)
        ["Pasadena"] = @"
calle California Boulevard | 20 | -900 150, 900 150
calle Del Mar Boulevard | 18 | -900 450, 900 450
calle Colorado Boulevard | 30 | -1000 900, 1000 900
calle San Pasqual Street | 12 | -600 -300, 800 -300
calle Lake Avenue | 25 | 400 -400, 400 1100
calle Hill Avenue | 18 | 0 -400, 0 1100
calle Wilson Avenue | 15 | -300 -400, -300 1100
calle Arroyo Parkway | 20 | -650 -400, -650 1100
calle Allen Avenue | 15 | 650 -400, 650 1100
rio Arroyo Seco | 30 | -1000 1300, -1050 0, -1100 -900
monte Montes de San Gabriel | 900 1480 | 0 1600 | seco
hito 0 | 45 45
hito 1 | -90 60
hito 2 | 300 860
hito 3 | 100 925
hito 4 | -300 300
Plaza | -520 1000
Parque | -650 700
Museo | -400 800
Investigacion | -850 1100
Estacion | -680 880
Policia | -460 1060
Bomberos | -200 1000
Mercado | 460 700
Piscina | -950 1050
Hotel | 500 -300
Tecnologia | 150 -100
Guarderia | 300 300
Baile | -150 700
Entrenamiento | -200 -150
Carpinteria | 500 450
Mecanica | 350 1000
Obra | 600 200
Milicia | 750 800
Edificio | 200 550
relleno | 80 | 0 400",
        // PRÍPIAT Y LA CENTRAL (1986): el bloque 4 (claro), el canal y el estanque de refrigeración, el río Prípiat y la ciudad
        // (a la mitad de su distancia real, 3 km, para que quepa en el piso): la avenida de Lenin, la plaza central con el Palacio
        // de Cultura Energetik, el hotel Polissia, la piscina Lazurny, el parque de atracciones y la estación de Yániv.
        // (Mapa de Prípiat, 1986, ChNPP; Leatherbarrow 2016)
        ["Chernóbil"] = @"
rio Prípiat | 160 | -1400 1400, -300 1000, 600 500, 1100 -200
lago Estanque de refrigeración | 450 180 | 520 -470
canal Canal de toma | 30 | 300 -160, 90 -60
calle Carretera de la central | 15 | 0 0, -300 200
calle Avenida de Lenin | 25 | -300 200, -800 800
calle Calle Kurchátov | 20 | -1100 600, -500 600
calle Calle de la Amistad de los Pueblos | 20 | -1000 450, -1000 1000
calle Calle de la central | 12 | -200 -100, 300 -100
hito 0 | 40 30
hito 1 | -150 120
hito 2 | -720 760
hito 3 | 100 120
hito 4 | 150 -60
hito 5 | -800 870
Plaza | -760 700
Monumento | -700 640
Piscina | -850 650
Baile | -740 820
Estacion | -200 600
Mercado | -650 760
Estudio | -900 700
Guarderia | -950 800
Museo | -650 850
Investigacion | 300 100
Tecnologia | -60 150
Policia | -600 600
Entrenamiento | -550 900
Parque | -800 950
Carpinteria | -400 400
Obra | 250 80
Mecanica | -300 350
Pileta | -780 720
Edificio | -950 600
relleno | 60 | -700 700",
        // ESCÚTARI (1854): el cuartel de Selimiye (un cuadrilátero de ~260 × 220 m con torres en las esquinas: su planta, a ras de
        // suelo) sobre el Bósforo (oeste), la mezquita de Selimiye y su meydan, el camino a Üsküdar (norte) y la colina de
        // Çamlıca. (Plan d'Üsküdar, 1850; Royal Collection Trust, «The Military Hospital, Scutari»)
        ["Escútari"] = @"
mar Bósforo | 600 2600 | -760 0
muralla Planta del cuartel de Selimiye | 0 | -130 -110, 130 -110, 130 110, -130 110, -130 -110
calle Camino de la orilla | 10 | -340 -900, -340 900
calle Camino de Üsküdar | 10 | 0 110, 0 900
calle Calle de Selimiye | 8 | -340 300, 400 300
calle Calle de la mezquita | 8 | 250 110, 250 800
colina Çamlıca | 450 230 | 950 650
acantilado Ribera del Bósforo | 15 | -440 -600, -445 0, -440 600
Plaza | 120 250
Pileta | 60 200
Parque | 320 450
Piscina | 180 340
Estacion | -300 600
Mercado | 80 700
Policia | 150 160
Bomberos | 260 200
Hotel | 50 800
Estudio | 200 390
Carpinteria | 320 600
Obra | 420 320
Guarderia | 360 700
Taller | 0 500
Mecanica | 460 650
Entrenamiento | 520 500
relleno | 50 | 100 450",
        // DAYTON (1903): West Third Street (la Wright Cycle Company, n.º 1127) y Hawthorn Street (la casa, n.º 7); el Great Miami
        // y el Wolf Creek, el centro al este del río. (Sanborn Map, Dayton 1897; NPS Dayton Aviation Heritage)
        ["Dayton"] = @"
calle West Third Street | 18 | -800 0, 1100 0
calle West Fourth Street | 12 | -800 -120, 900 -120
calle West Second Street | 12 | -800 120, 900 120
calle Hawthorn Street | 10 | 60 -300, 60 50
calle Williams Street | 10 | -150 -300, -150 300
calle South Broadway | 12 | 400 -300, 400 300
rio Great Miami | 120 | 1300 900, 1000 200, 1100 -900
rio Wolf Creek | 15 | -800 700, 1000 260
Plaza | 700 60
Estacion | 800 -200
Policia | 300 125
Bomberos | 200 -125
Parque | -300 200
Estudio | 500 200
Mercado | 900 50
Hotel | 600 -50
Tecnologia | 400 -200
Mecanica | -200 125
Carpinteria | -300 -100
Obra | 150 250
Guarderia | 250 -250
Baile | -400 100
Entrenamiento | 650 150
Museo | 750 250
Milicia | 500 -300
Pileta | -50 125
relleno | 60 | 0 0",
        // KAPILAVASTU (s. V a. C.): Tilaurakot, ciudad amurallada de ~500 × 450 m con puertas a los puntos cardinales (las del este
        // y el oeste, excavadas), calles en cuadrícula, estanques revestidos de ladrillo; el río Banganga al oeste y los arrozales.
        // (Durham Univ., proyecto Tilaurakot-Kapilavastu, Coningham et al. 2017)
        ["Kapilavastu"] = @"
muralla Muralla sur (puerta del sur) | 0 | -250 -225, -12 -225
muralla Muralla sur | 0 | 12 -225, 250 -225
muralla Muralla este (puerta del este) | 0 | 250 -225, 250 -12
muralla Muralla este | 0 | 250 12, 250 225
muralla Muralla norte (puerta del norte) | 0 | 250 225, 12 225
muralla Muralla norte | 0 | -12 225, -250 225
muralla Muralla oeste (puerta del oeste) | 0 | -250 225, -250 12
muralla Muralla oeste | 0 | -250 -12, -250 -225
rio Banganga | 40 | -520 900, -430 0, -490 -900
lago Estanque de ladrillo | 30 20 | 130 -130
lago Sagar (estanque de fuera) | 50 30 | 360 300
campo Arrozales del este | 200 400 | 520 0
campo Arrozales del oeste | 150 250 | -680 300
calle Calle norte-sur | 8 | 0 -260, 0 260
calle Calle este-oeste | 8 | -260 0, 260 0
calle Calle de los alfareros | 6 | -125 -225, -125 225
calle Calle de los tejedores | 6 | 125 -225, 125 225
calle Calle del mercado | 6 | -250 110, 250 110
calle Calle del estanque | 6 | -250 -110, 250 -110
Plaza | 50 -65
Pileta | 65 75
Parque | 400 -300
Piscina | 170 160
Estacion | -420 60
Monumento | -65 -85
Estudio | 160 -170
Entrenamiento | -160 160
Baile | 70 165
Hotel | 200 200
Milicia | -180 -160
Carpinteria | -170 -60
Obra | 190 -55
Guarderia | -60 165
Taller | 60 -180
Mecanica | -200 60
relleno | 45 | 0 0",
        // ALEJANDRÍA (~240 a. C.): la retícula de Dinócrates con la Vía Canópica (30 m) y la calle del Soma, el Brucheion (el
        // barrio real con el Museion) junto al Gran Puerto, el Heptastadion a la isla de Faros, el Paneion y el lago Mareotis.
        // (Estrabón XVII.1.6–10; McKenzie 2007, The Architecture of Alexandria and Egypt)
        ["Alejandría"] = @"
calle Vía Canópica | 30 | -1000 -150, 1000 -150
calle Calle del Soma | 15 | -200 -500, -200 300
calle Calle del Museion | 10 | 100 -500, 100 300
calle Calle del teatro | 10 | 400 -500, 400 300
calle Calle oeste | 10 | -500 -500, -500 300
calle Calle del puerto | 10 | -800 100, 800 100
calle Calle sur | 10 | -800 -400, 800 -400
calle Heptastadion | 20 | -500 300, -500 950
mar Gran Puerto | 1700 500 | 0 620
colina Isla de Faros | 140 8 | 300 950
lago Lago Mareotis | 1200 300 | 0 -1050
colina Paneion | 60 20 | -330 -300
Plaza | -150 -220
Monumento | -230 -100
Mercado | 120 -260
Pileta | 60 -70
Parque | -360 -400
Piscina | 260 -60
Estacion | 120 380
Museo | -300 130
Investigacion | 300 160
Estudio | -100 160
Entrenamiento | 420 -220
Hotel | 520 -100
Policia | -60 -330
Milicia | 600 160
Baile | 200 200
Carpinteria | -420 -100
Obra | 520 250
Guarderia | 160 -380
Taller | -620 -250
Tecnologia | 360 40
Mecanica | -660 0
relleno | 70 | 0 0",
        // PATANJALI (s. II a. C.): el āśrama junto al Ganges (oeste, con su ghat), la aldea y sus campos.
        ["Patanjali"] = @"
rio Ganges | 320 | -420 -1000, -380 0, -440 1000
calle Sendero del āśrama | 5 | 0 0, 320 0
calle Calle de la aldea | 6 | 0 150, 420 150
calle Calle del pozo | 6 | 250 -220, 250 320
calle Escalones del ghat | 8 | -60 0, -230 0
campo Campos de cebada | 250 250 | 520 -320
campo Campos de mostaza | 200 200 | 520 360
Plaza | 260 125
Pileta | 180 60
Parque | -120 210
Piscina | 120 -160
Estacion | -190 40
Mercado | 320 -60
Estudio | 60 170
Entrenamiento | 360 230
Baile | 150 260
Hotel | 410 50
Guarderia | 200 -210
Carpinteria | 330 -160
Taller | 430 180
Obra | 100 300
Monumento | -60 -125
relleno | 30 | 250 100",
        // LA TREGUA DE 1914 (Flandes, Saint-Yvon/Ploegsteert): las dos líneas de trincheras a 50–100 m, la tierra de nadie, el
        // bosque de Ploegsteert detrás de la línea británica y el río Douve. (Brown y Seaton 1984, Christmas Truce; IWM)
        ["Tregua 1914"] = @"
calle Trinchera británica | 4 | -700 -40, -350 -30, 0 -45, 350 -30, 700 -40
calle Trinchera alemana | 4 | -700 60, -350 70, 0 55, 350 70, 700 60
calle Trinchera de comunicación británica | 3 | -200 -45, -220 -500
calle Trinchera de comunicación alemana | 3 | 250 60, 270 500
campo Bosque de Ploegsteert | 450 260 | -450 -380
campo Campo de remolachas | 300 200 | 400 -350
rio Douve | 10 | -900 420, 0 380, 900 450
colina Colina de Messines | 500 60 | 300 900
relleno | 20 | -250 -450
calle Camino de Ploegsteert | 6 | -250 -45, -250 -800
calle Calle de Ploegsteert | 6 | -500 -600, 0 -600",
        // PLANTA ATÓMICA (1666 / 1898 / 1905): tres barrios de la escalera de la física: Woolsthorpe al oeste (el Witham), París
        // en el centro (la rue Lhomond) y Berna al este (el Aar y la Kramgasse).
        ["Planta Atómica"] = @"
rio Witham | 14 | -520 -800, -480 800
rio Aar | 60 | 600 -800, 520 0, 640 800
calle Water Lane | 6 | -480 0, -120 0
calle Rue Lhomond | 8 | 0 -300, 0 300
calle Rue Claude-Bernard | 10 | -120 -160, 280 -160
calle Kramgasse | 9 | 120 0, 520 0
campo Campos de Woolsthorpe | 250 300 | -700 0
Plaza | 0 -80
Estudio | -60 160
Mercado | 200 -60
Pileta | 60 120
Estacion | 450 120
Policia | 300 120
Bomberos | 260 -120
Tecnologia | 380 -60
Investigacion | 120 -260
Parque | -300 200
relleno | 40 | 0 0",
        // URUK (~3500 a. C.): el recinto del Eanna, la terraza de Anu con el Templo Blanco al oeste (13 m), los canales que cruzan
        // la ciudad y el viejo curso del Éufrates; campos y palmerales fuera. (Nissen 1988; Crüsemann et al. 2019, Uruk)
        ["Mesopotamia"] = @"
rio Éufrates | 150 | -900 900, -700 0, -850 -900
canal Canal del Eanna | 15 | -700 100, 600 50
canal Canal sur | 10 | -700 -300, 500 -350
colina Terraza de Anu | 60 12 | -260 80
calle Calle del Eanna | 6 | -500 -100, 500 -100
calle Calle de los alfareros | 5 | 150 -400, 150 300
calle Calle del canal | 5 | -150 -400, -150 300
campo Campos de cebada | 300 250 | 620 420
campo Campos del sur | 300 200 | 620 -460
Plaza | 0 -150
Pileta | 100 120
Mercado | 200 -160
Monumento | 60 -80
Estacion | 300 60
Parque | 420 200
Milicia | -300 -250
Entrenamiento | 300 -250
Baile | -100 -230
Carpinteria | 260 200
Taller | -200 250
Obra | 400 -100
Guarderia | 100 250
Mecanica | -350 150
relleno | 40 | 0 0",
        // VARSOVIA (1942): el Vístula al este, Leszno y Marszałkowska, Aleje Jerozolimskie, el Jardín Sajón; los edificios del pueblo
        // lejos del gueto de la historia. (Plan Warszawy 1939)
        ["Irena Sendler"] = @"
rio Vístula | 400 | 900 -1000, 950 0, 900 1000
calle Leszno | 18 | -800 160, 800 160
calle Marszałkowska | 20 | 320 -800, 320 800
calle Aleje Jerozolimskie | 30 | -800 -600, 800 -600
Plaza | 420 -300
Estacion | 560 -640
Parque | 420 380
Policia | -350 420
Bomberos | -450 -350
Mercado | 500 60
Museo | 600 -420
Piscina | -380 -560
Hotel | 300 -500
Tecnologia | 550 300
Guarderia | -300 -420
relleno | 30 | 400 -200",
        // LE CHAMBON-SUR-LIGNON (1942): el pueblo del plateau, el río Lignon, la estación del tren del Vivarais (1902), la carretera
        // de Le Mazet y el monte Lizieux. (Carte de l'État-major; Hallie 1979)
        ["Le Chambon-sur-Lignon"] = @"
rio Lignon | 20 | 900 -800, 0 -300, -900 -500
monte Mont Lizieux | 500 400 | -950 650
acantilado Gargantas del Lignon | 25 | 700 -660, 300 -440, 0 -360
calle Calle mayor | 8 | -500 -80, 500 -80
calle Carretera de Le Mazet | 7 | 500 -80, 900 -400
calle Vía del tren del Vivarais | 5 | -800 -230, 800 -180
Estacion | 300 -210
Plaza | -260 70
Pileta | 260 110
Policia | -320 -150
Bomberos | 360 210
Hotel | 320 260
Mercado | -320 230
Carpinteria | 420 -60
Taller | -420 -60
Mecanica | -460 160
Guarderia | 220 360
Obra | -220 360
relleno | 25 | 300 120",
        // BRUCE LEE (1957–1971): dos barrios. KOWLOON (Yau Ma Tei y Mong Kok, años 50) al oeste: Nathan Road, Temple Street,
        // Shanghai Street, Jordan Road y Waterloo Road, el puerto Victoria al sur y la Roca del León al norte (Hong Kong Map, 1957,
        // Crown Lands and Survey Office). SEATTLE (1959–64) al este: Broadway y Jefferson Street (el restaurante de Ruby Chow),
        // University Way, el lago Union y el monte Rainier al sureste (Kroll's Map of Seattle, 1960).
        ["Bruce Lee"] = @"
calle Nathan Road | 25 | -300 -450, -300 450
calle Temple Street | 8 | -380 -400, -380 300
calle Shanghai Street | 10 | -460 -400, -460 300
calle Jordan Road | 20 | -650 -150, -150 -150
calle Waterloo Road | 20 | -650 200, -150 200
calle Lee Tat Street | 6 | -380 50, -230 50
mar Puerto Victoria | 900 260 | -400 -640
monte Roca del León | 500 495 | -400 980
calle Broadway | 20 | 300 -400, 300 400
calle Jefferson Street | 15 | 150 -100, 650 -100
calle University Way | 15 | 520 100, 520 500
calle Calle 45 Noreste | 15 | 300 300, 700 300
lago Lago Union | 200 110 | 680 160
monte Monte Rainier | 450 4390 | 900 -980 | nieve
Policia | -340 -20
Bomberos | -250 -250
Estacion | -300 -480
Parque | -430 120
Guarderia | -520 80
Hotel | -230 -360
Baile | -520 -250
Carpinteria | -570 0
Obra | -600 320
Edificio | -470 -110
Plaza | -220 110
Mercado | -560 -330
Museo | 650 380
Investigacion | 600 470
Estudio | 380 -220
Mecanica | 420 120
Tecnologia | 560 -60
Milicia | 700 -300
Pileta | 360 220
Piscina | 200 250
relleno | 0 | 0 0",
        // EGINA (la Grecia heroica, ~1250 a. C.): el puerto y la colina de Kolona al oeste (el asentamiento de la Edad del Bronce y el
        // santuario de Apolo), el monte Oros con el santuario de Zeus Helenio al sureste, los olivares. (Walter y Felten 1981; Pausanias II.29)
        ["Egina"] = @"
mar Golfo Sarónico | 500 2400 | -900 0
colina Kolona | 120 15 | -520 120
monte Monte Oros | 500 532 | 800 -800 | seco
calle Camino del puerto | 7 | -560 40, 0 60
calle Camino del Oros | 6 | 0 60, 600 -600
calle Calle de los alfareros | 5 | -300 -150, -300 300
calle Calle del palacio | 6 | -100 -200, -100 300
campo Olivares | 300 200 | 300 250
campo Campos de cebada | 250 200 | 250 -250
hito 0 | -110 120
hito 1 | 560 -540
hito 2 | -560 0
hito 3 | -300 80
Plaza | -60 -90
Pileta | 40 -110
Mercado | -200 -60
Parque | 200 120
Milicia | -150 220
Carpinteria | -250 -200
Obra | 120 -150
Taller | -350 -100
relleno | 40 | -100 60",
        // PALENQUE (683): Lakamha', «el lugar de las grandes aguas». El Palacio y el Templo de las Inscripciones sobre la Gran
        // Plaza, el Otolum que corre al norte (con su acueducto bajo el Palacio), el Grupo de las Cruces sobre su colina al este,
        // el Juego de Pelota y el Grupo Norte, el Baño de la Reina, la sierra al sur y la llanura con las milpas al norte.
        // (Barnhart 2001, The Palenque Mapping Project; Stuart y Stuart 2008)
        ["Palenque"] = @"
rio Otolum | 12 | 80 -560, 60 0, 85 300, 45 700
lago Baño de la Reina | 28 16 | 75 620
colina Colina del Grupo de las Cruces | 90 22 | 230 -95
monte Sierra de Don Juan | 750 600 | 0 -1000
acantilado Escarpe de la sierra | 35 | -420 -380, 0 -400, 420 -370
campo Milpa del llano | 600 260 | 0 920
campo Milpa del oeste | 220 220 | -520 520
calle Gran Plaza | 30 | -150 0, 150 0
calle Sacbé del Grupo Norte | 8 | 20 100, 20 600
calle Camino de las Cruces | 6 | 60 -20, 200 -60
hito 0 | -15 85
hito 1 | -115 35
hito 2 | 60 180
hito 3 | -70 160
hito 4 | -75 105
hito 5 | 215 -45
hito 6 | -160 120
Monumento | 40 -65
Pileta | 95 125
Parque | -300 -210
Piscina | 160 60
Mercado | 210 250
Milicia | -260 160
Baile | -210 -25
Taller | 260 160
Carpinteria | -310 310
Obra | 310 -160
Guarderia | 130 360
relleno | 40 | 0 120",
    };

    /// <summary>El plano de <paramref name="display"/> (null si no tiene o el piso no da para un pueblo: &lt; 500 m).</summary>
    static TownPlan PlanOf(string display, Rect floor)
    {
        if (!PlanText.TryGetValue(display, out string txt) || floor.width < 500f || floor.height < 500f) return null;
        var p = new TownPlan { town = display };
        var ci = CultureInfo.InvariantCulture;
        Vector2 V(string s) { var x = s.Trim().Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries); return new Vector2(float.Parse(x[0], ci), float.Parse(x[1], ci)); }
        foreach (string raw in txt.Split('\n'))
        {
            string line = raw.Trim(); if (line.Length == 0) continue;
            string[] f = line.Split('|');
            for (int i = 0; i < f.Length; i++) f[i] = f[i].Trim();
            int sp = f[0].IndexOf(' ');
            string kind = sp < 0 ? f[0] : f[0].Substring(0, sp), name = sp < 0 ? "" : f[0].Substring(sp + 1);
            try
            {
                switch (kind)
                {
                    case "hito": p.hitos[int.Parse(name, ci)] = V(f[1]); break;
                    case "relleno": p.fill = int.Parse(f[1], ci); p.fillNear = V(f[2]); break;
                    case "calle": case "rio": case "canal": case "muralla": case "acantilado":
                    {
                        var ft = new Feat { k = kind == "calle" ? FK.Calle : kind == "muralla" ? FK.Muralla : kind == "acantilado" ? FK.Acantilado : FK.Rio, name = name, a = kind == "muralla" ? 1.6f : float.Parse(f[1], ci) };
                        foreach (string pt in f[2].Split(',')) ft.p.Add(V(pt));
                        p.feats.Add(ft); break;
                    }
                    case "lago": case "colina": case "monte": case "campo": case "mar":
                    {
                        Vector2 ab = V(f[1]);
                        p.feats.Add(new Feat { k = kind == "lago" ? FK.Lago : kind == "colina" ? FK.Colina : kind == "monte" ? FK.Monte : kind == "campo" ? FK.Campo : FK.Mar, name = name, a = ab.x, b = ab.y, p = { V(f[2]) },
                            snow = f.Length > 3 && f[3].Contains("nieve"), dry = f.Length > 3 && f[3].Contains("seco") });
                        break;
                    }
                    default:
                        if (System.Enum.TryParse(kind, out MicroStyles.Civic c)) p.civic.Add((c, V(f[1])));
                        else Debug.LogWarning($"[Pueblos] {display}: línea del plano sin entender: «{line}»");
                        break;
                }
            }
            catch (System.Exception e) { Debug.LogWarning($"[Pueblos] {display}: línea del plano mal escrita «{line}»: {e.Message}"); }
        }
        // EL TAMAÑO: 1:1 si cabe lo construido (calles, murallas, hitos, edificios) en el barrio con 30 m de margen; si no, el plano
        // entero se encoge lo justo. Ríos, lagos, montes, campos y el mar no cuentan: se recortan en el borde.
        float s = 1f;
        void Fit(Vector2 q)
        {
            if (q.x > 1f) s = Mathf.Min(s, (floor.xMax - 30f) / q.x); else if (q.x < -1f) s = Mathf.Min(s, (floor.xMin + 30f) / q.x);
            if (q.y > 1f) s = Mathf.Min(s, (floor.yMax - 30f) / q.y); else if (q.y < -1f) s = Mathf.Min(s, (floor.yMin + 30f) / q.y);
        }
        foreach (Feat ft in p.feats) if (ft.k == FK.Calle || ft.k == FK.Muralla) foreach (Vector2 q in ft.p) Fit(q);
        foreach (var h in p.hitos.Values) Fit(h);
        foreach (var c in p.civic) Fit(c.at);
        p.scale = Mathf.Clamp(s, 0.15f, 1f);
        return p;
    }

    /// <summary>Un punto del plano en los locales del pueblo (a su escala, dentro del barrio).</summary>
    internal static Vector2 PlanPoint(Vector2 q)
    {
        float s = _plan != null ? _plan.scale : 1f;
        return q * s;
    }

    // ══ 1 · EL PAISAJE Y LAS CALLES ═════════════════════════════════════════════════════════════════════════════════

    /// <summary>Levanta el paisaje y las calles del plano bajo <paramref name="t"/>, ADAPTADOS AL PISO (2026-11-03: el piso no se rompe):
    /// ninguna calle, río, lago, colina o campo atraviesa un mueble del piso que apoye en el suelo (se corta a su lado; bajo el tablero
    /// de una mesa sí se pasa). El relieve, con mallas low-poly realistas (CivicBuilder.Terrain). Devuelve cuántas piezas puso.</summary>
    static int BuildPlan(Transform t, TownPlan p, Rect floor, Color street)
    {
        Transform land = new GameObject(PlanLandscape + p.town).transform; land.SetParent(t, false);
        Transform streets = new GameObject($"Calles de {p.town} {PlanStreets}").transform; streets.SetParent(t, false);
        Material mStreet = AreaMicrocosmos.Mat("Civ_Calle_" + Hex(street), street), field = AreaMicrocosmos.Mat("Civ_Campo", C(0.62f, 0.6f, 0.34f)),
            wall = AreaMicrocosmos.Mat("Civ_Cimiento_muralla", C(0.62f, 0.5f, 0.38f)), sea = TerrainMat("mar", C(0.2f, 0.4f, 0.55f), 0.85f);
        Rect clip = new Rect(floor.xMin + 2f, floor.yMin + 2f, floor.width - 4f, floor.height - 4f);
        // Los muebles del piso que apoyan en el suelo, en los locales de t.
        var furn = new List<Rect>();
        foreach (Bounds fb in AreaMicrocosmos.FloorFurniture(t))
        {
            if (fb.min.y - t.position.y > 5f) continue;
            Vector3 a = t.InverseTransformPoint(fb.min), z = t.InverseTransformPoint(fb.max);
            furn.Add(Rect.MinMaxRect(Mathf.Min(a.x, z.x), Mathf.Min(a.z, z.z), Mathf.Max(a.x, z.x), Mathf.Max(a.z, z.z)));
        }
        bool OnFurniture(Vector2 c, float rad)
        {
            foreach (Rect r in furn) if (c.x > r.xMin - rad && c.x < r.xMax + rad && c.y > r.yMin - rad && c.y < r.yMax + rad) return true;
            return false;
        }
        float s = p.scale; int n = 0, cut = 0, seed = p.town.GetHashCode() & 0x7fffffff;
        foreach (Feat f in p.feats)
        {
            seed += 17;
            switch (f.k)
            {
                case FK.Calle: case FK.Muralla:
                {
                    Transform parent = f.k == FK.Calle ? streets : land;
                    Material m = f.k == FK.Calle ? mStreet : wall;
                    float w = f.k == FK.Muralla ? 1.6f : Mathf.Max(4f, f.a);
                    float y = f.k == FK.Muralla ? 0.25f : 0.02f, h = f.k == FK.Muralla ? 0.5f : 0.04f;
                    string label = (f.k == FK.Calle ? "Calle " : "Cimiento ") + f.name;
                    for (int i = 0; i + 1 < f.p.Count; i++)
                    {
                        Vector2 a = f.p[i] * s, b = f.p[i + 1] * s;
                        if (!ClipSegment(ref a, ref b, clip)) continue;
                        float len = (b - a).magnitude; if (len < 0.5f) continue;
                        int pieces = Mathf.CeilToInt(len / 40f);   // tramos de ≤ 40 m (los solares y la flora esquivan cada uno)
                        float yaw = Mathf.Atan2(b.x - a.x, b.y - a.y) * Mathf.Rad2Deg;
                        for (int k = 0; k < pieces; k++)
                        {
                            Vector2 c = Vector2.Lerp(a, b, (k + 0.5f) / pieces);
                            if (OnFurniture(c, w * 0.5f)) { cut++; continue; }
                            AreaMicrocosmos.Box(parent, label, new Vector3(c.x, y, c.y), new Vector3(w, h, len / pieces + (f.k == FK.Muralla ? 0f : w * 0.3f)), Quaternion.Euler(0f, yaw, 0f), m, f.k == FK.Muralla);
                            n++;
                        }
                    }
                    if (f.k == FK.Muralla) { AddLabel(land, f.name, f.p[0] * s, 2f); n++; }
                    break;
                }
                case FK.Rio:
                {
                    float w = Mathf.Max(6f, f.a * Mathf.Max(s, 0.5f));
                    for (int i = 0; i + 1 < f.p.Count; i++)
                    {
                        Vector2 a = f.p[i] * s, b = f.p[i + 1] * s;
                        if (!ClipSegment(ref a, ref b, clip)) continue;
                        float len = (b - a).magnitude; if (len < 0.5f) continue;
                        int pieces = Mathf.CeilToInt(len / 60f);
                        for (int k = 0; k < pieces; k++)
                        {
                            Vector2 p0 = Vector2.Lerp(a, b, k / (float)pieces), p1 = Vector2.Lerp(a, b, (k + 1) / (float)pieces);
                            if (OnFurniture((p0 + p1) * 0.5f, w * 0.5f)) { cut++; continue; }
                            if (RiverReach(land, "Río " + f.name, p0, p1, w, seed + k) != null) n++;
                        }
                    }
                    AddLabel(land, f.name, f.p[Mathf.Min(1, f.p.Count - 1)] * s, 3f);
                    break;
                }
                case FK.Lago:
                {
                    Vector2 c = f.p[0] * s, half = new Vector2(f.a, f.b) * Mathf.Max(s, 0.5f);
                    if (!clip.Contains(c)) break;
                    half = Vector2.Min(half, new Vector2(Mathf.Min(c.x - clip.xMin, clip.xMax - c.x), Mathf.Min(c.y - clip.yMin, clip.yMax - c.y)) / 1.15f);
                    if (half.x < 6f || half.y < 6f) break;
                    if (OnFurniture(c, Mathf.Min(half.x, half.y) * 0.8f)) { cut++; break; }
                    Lake(land, "Lago " + f.name, c, half.x, half.y, seed);
                    AddLabel(land, f.name, c, 3f); n++; break;
                }
                case FK.Mar: case FK.Campo:
                {
                    Vector2 c = f.p[0] * s; Vector2 half = new Vector2(f.a, f.b) * 0.5f * Mathf.Max(s, 0.5f);
                    Rect r = Rect.MinMaxRect(Mathf.Max(c.x - half.x, clip.xMin), Mathf.Max(c.y - half.y, clip.yMin), Mathf.Min(c.x + half.x, clip.xMax), Mathf.Min(c.y + half.y, clip.yMax));
                    if (r.width < 4f || r.height < 4f) break;
                    // En trozos de ≤ 80 m, sin los que pisan un mueble: el mar y los campos rodean los muebles.
                    int nx = Mathf.CeilToInt(r.width / 80f), nz = Mathf.CeilToInt(r.height / 80f);
                    for (int i = 0; i < nx; i++)
                        for (int k = 0; k < nz; k++)
                        {
                            Rect q = new Rect(r.xMin + i * r.width / nx, r.yMin + k * r.height / nz, r.width / nx, r.height / nz);
                            if (OnFurniture(q.center, Mathf.Max(q.width, q.height) * 0.5f)) { cut++; continue; }
                            if (f.k == FK.Mar) AreaMicrocosmos.Box(land, "Mar " + f.name, new Vector3(q.center.x, 0.025f, q.center.y), new Vector3(q.width, 0.04f, q.height), sea, false);
                            else AreaMicrocosmos.Box(land, "Campo " + f.name, new Vector3(q.center.x, 0.03f, q.center.y), new Vector3(q.width - 1f, 0.04f, q.height - 1f), field, false);
                            n++;
                        }
                    AddLabel(land, f.name, r.center, 3f);
                    break;
                }
                case FK.Colina: case FK.Monte:
                {
                    // Un MONTE o una COLINA de verdad (CivicBuilder.Peak): malla facetada con ruido, pradera, roca, canchal y nieve.
                    Vector2 c = f.p[0] * s; float r = f.a * Mathf.Max(s, 0.5f), h = f.b * Mathf.Max(s, 0.5f);
                    if (!clip.Contains(c)) break;   // un monte lejano fuera del barrio: no se ve desde el pueblo de este piso
                    r = Mathf.Min(r, Mathf.Min(Mathf.Min(c.x - clip.xMin, clip.xMax - c.x), Mathf.Min(c.y - clip.yMin, clip.yMax - c.y)) / 1.15f);
                    if (r < 15f) break;
                    if (OnFurniture(c, r * 0.6f)) { cut++; break; }
                    h = Mathf.Min(h, r * (f.k == FK.Monte ? 0.9f : 0.35f));   // colinas suaves (se suben andando), montes empinados
                    h = Mathf.Min(h, 300f);   // (los muros del piso llegan a 400 m: los montes reales más altos, recortados)
                    Peak(land, (f.k == FK.Monte ? "Monte " : "Colina ") + f.name, c, r, h, f.snow ? h * 0.62f : -1f, seed, f.dry);
                    AddLabel(land, f.name, c, h + 4f);
                    n++; break;
                }
                case FK.Acantilado:
                {
                    var pts = new List<Vector2>();
                    foreach (Vector2 q in f.p) { Vector2 w = q * s; if (clip.Contains(w)) pts.Add(w); }
                    if (pts.Count < 2) break;
                    bool hit = false; foreach (Vector2 q in pts) if (OnFurniture(q, 4f)) hit = true;
                    if (hit) { cut++; break; }
                    Cliff(land, "Acantilado " + f.name, pts, Mathf.Min(f.a, 120f), seed);
                    AddLabel(land, f.name, pts[0], f.a + 3f);
                    n++; break;
                }
            }
        }
        Debug.Log($"[Pueblos] {p.town}: plano a escala {(p.scale >= 0.999f ? "1:1" : $"1:{1f / p.scale:0.0} (encogido para caber en el barrio)")}: {n} piezas de paisaje y calles ({p.feats.Count} elementos del mapa), en un barrio de {floor.width:0} × {floor.height:0} m; {cut} tramos no se pusieron por no atravesar un mueble del piso.");
        return n;
    }

    static WorldLabel AddLabel(Transform parent, string text, Vector2 at, float h)
    {
        var g = new GameObject("Rótulo " + text); g.transform.SetParent(parent, false); g.transform.localPosition = new Vector3(at.x, 0f, at.y);
        var wl = g.AddComponent<WorldLabel>(); wl.text = text; wl.height = h;
        return wl;
    }

    // Recorta el segmento a-b al rectángulo (Liang–Barsky). false si queda fuera.
    static bool ClipSegment(ref Vector2 a, ref Vector2 b, Rect r)
    {
        float t0 = 0f, t1 = 1f; Vector2 d = b - a;
        bool Clip(float p, float q)
        {
            if (Mathf.Abs(p) < 1e-6f) return q >= 0f;
            float u = q / p;
            if (p < 0f) { if (u > t1) return false; if (u > t0) t0 = u; }
            else { if (u < t0) return false; if (u < t1) t1 = u; }
            return true;
        }
        if (!Clip(-d.x, a.x - r.xMin) || !Clip(d.x, r.xMax - a.x) || !Clip(-d.y, a.y - r.yMin) || !Clip(d.y, r.yMax - a.y)) return false;
        Vector2 a0 = a; a = a0 + d * t0; b = a0 + d * t1;
        return true;
    }

    // ══ 2 · LOS EDIFICIOS DEL PUEBLO ════════════════════════════════════════════════════════════════════════════════

    /// <summary>Los EDIFICIOS DEL PUEBLO del plano (los tipos de <see cref="MicroStyles.Civic"/>), cada uno con su nombre de época y
    /// lugar (o ninguno, si no existía), sus puestos interactuables, sus muebles a la medida de humanos e insectos-humanos, sus
    /// etiquetas de actividad y su estilo; en el solar libre más cercano a su sitio del plano. Devuelve cuántos levantó.</summary>
    static int PlanCivic(Transform t, Lots lots, TownPlan p)
    {
        if (_civicDone || p == null) return 0;
        _civicDone = true;
        MicroStyles.Entry e = MicroStyles.For(Year, Region);
        Style st = _planStyle ?? NeedsStyle;
        int n = 0, skipped = 0; var none = new List<string>();
        foreach (var (c, at) in p.civic)
        {
            if (Count(t) >= PlanMaxSites) break;
            string region0 = Region; Style st0 = st;
            if (_quarterAt != null) { var q = _quarterAt(at); Region = q.region; st = q.st; e = MicroStyles.For(Year, Region); }
            string title = MicroStyles.CivicName(c, e, Year);
            if (title == null) { Region = region0; st = st0; e = MicroStyles.For(Year, Region); }
            if (title == null) { none.Add(c.ToString()); continue; }
            AreaFunctionSite site = Civic(t, c, title, st, e);
            if (site == null) { Region = region0; st = st0; e = MicroStyles.For(Year, Region); skipped++; continue; }
            bool ok = PlaceOn(lots, site, PlanPoint(at));
            Region = region0; st = st0; e = MicroStyles.For(Year, Region);
            if (!ok) { Object.DestroyImmediate(site.gameObject); skipped++; continue; }
            n++;
        }
        Debug.Log($"[Pueblos] {p.town}: {n} edificios del pueblo en su sitio del plano ({e.id}, {Eras.Format(Year)}){(skipped > 0 ? $"; {skipped} sin solar" : "")}{(none.Count > 0 ? $"; no existían entonces: {string.Join(", ", none)}" : "")}.");
        return n;
    }

    static int Count(Transform t) => t.GetComponentsInChildren<AreaFunctionSite>(true).Length;

    /// <summary>Un edificio del pueblo del tipo <paramref name="c"/> (abierto, con sus puestos y su interacción).</summary>
    static AreaFunctionSite Civic(Transform t, MicroStyles.Civic c, string title, Style st, MicroStyles.Entry e)
    {
        bool maya = e.id == "maya_clasico";
        string fruit = MicroFurniture.FruitFor(e, title.GetHashCode()).name;
        switch (c)
        {
            case MicroStyles.Civic.Estudio: return Building(t, title, "Escuela", title, Vector3.zero, 0f, new Vector2(14f, 10f), st, S("pizarra", 0, 0, 1.6f, 0.4f, "Pizarra"), S("mesa", 0, 0, 2f, 1f, "Pupitres"), S("estante", 0, 0, 0.8f, 2f, maya ? "Códices de papel amate" : "Libros"));
            case MicroStyles.Civic.Entrenamiento: return Building(t, title, "Centro de entrenamiento", title, Vector3.zero, 0f, new Vector2(16f, 12f), st, maya ? S("pista", 0, 0, 6f, 6f, "Cancha del juego de pelota") : S("tatami", 0, 0, 6f, 5f, "Suelo de entrenar"), S("armario", 0, 0, 1.2f, 0.6f, maya ? "Protectores de cadera (yugos)" : "Equipo"));
            case MicroStyles.Civic.Baile: return Building(t, title, "Sala de baile", title, Vector3.zero, 0f, new Vector2(14f, 12f), st, S("pista", 0, 0, 6f, 6f, "Pista de baile"), S("mesa", 0, 0, 1.6f, 1f, "Mesa de los músicos"));
            case MicroStyles.Civic.Plaza: return Building(t, title, "Plaza", title, Vector3.zero, 0f, new Vector2(24f, 20f), st, S("fuente", 0, 0, 4.4f, 4.4f, MicroStyles.CivicName(MicroStyles.Civic.Pileta, e, Year) ?? "Fuente"), S("estatua", 0, 0, 2.2f, 2.2f, MicroStyles.CivicName(MicroStyles.Civic.Monumento, e, Year) ?? "Piedra de la plaza"), S("banco", 0, 0, 4f, 1.6f, "Banco de la plaza"), S("banco", 0, 0, 4f, 1.6f, "Banco de la plaza"), S("banco", 0, 0, 4f, 1.6f, "Banco de la plaza"));
            case MicroStyles.Civic.Monumento: return Building(t, title, "Plaza", title, Vector3.zero, 0f, new Vector2(10f, 10f), st, S("estatua", 0, 0, 2.2f, 2.2f, title), S("fuente", 0, 0, 4.4f, 4.4f, "Pila"), S("banco", 0, 0, 4f, 1.6f, "Banco"), S("banco", 0, 0, 4f, 1.6f, "Banco"));
            case MicroStyles.Civic.Pileta: return Building(t, title, "Fuente pública", title, Vector3.zero, 0f, new Vector2(10f, 8f), st, S("fuente", 0, 0, 4.4f, 4.4f, title), S("lavadero", 0, 0, 1.6f, 1f, "Lavadero"));
            case MicroStyles.Civic.Parque: return Building(t, title, "Parque", title, Vector3.zero, 0f, new Vector2(22f, 18f), st, S("frutal", 0, 0, 1.6f, 1.6f, fruit), S("frutal", 0, 0, 1.6f, 1.6f, MicroFurniture.FruitFor(e, title.GetHashCode() + 1).name), S("fuente", 0, 0, 4.4f, 4.4f, "Fuente"), S("banco", 0, 0, 4f, 1.6f, "Banco del parque"), S("frutal", 0, 0, 1.6f, 1.6f, MicroFurniture.FruitFor(e, title.GetHashCode() + 2).name));
            case MicroStyles.Civic.Estacion: return Building(t, title, "Estación", title, Vector3.zero, 0f, new Vector2(16f, 10f), st, S("anden", 0, 0, 8f, 3f, "Andén"), S("puesto", 0, 0, 2f, 1f, "Taquilla"));
            case MicroStyles.Civic.Museo: return Building(t, title, "Museo", title, Vector3.zero, 0f, new Vector2(16f, 10f), st, S("vitrina", 0, 0, 1.6f, 0.6f, "Vitrina"), S("vitrina", 0, 0, 1.6f, 0.6f, "Vitrina"), S("estatua", 0, 0, 1.4f, 1.4f, "Pieza mayor de la colección"));
            case MicroStyles.Civic.Investigacion: return Building(t, title, "Laboratorio", title, Vector3.zero, 0f, new Vector2(14f, 10f), st, S("mesa_laboratorio", 0, 0, 2.4f, 1f, maya ? "Mesa de las cuentas del calendario" : "Mesa de experimentos"), S("estante", 0, 0, 0.8f, 2f, "Registros"), S("consola", 0, 0, 1.6f, 0.7f, "Instrumentos"));
            case MicroStyles.Civic.Guarderia: return Building(t, title, "Guardería", title, Vector3.zero, 0f, new Vector2(14f, 10f), st, S("cuna", 0, 0, 0.8f, 1.3f, "Cuna"), S("cuna", 0, 0, 0.8f, 1.3f, "Cuna"), S("esterilla", 0, 0, 2f, 2f, "Estera de juegos"), S("fogon", 0, 0, 1.6f, 0.8f, NeedLabel("fogon", Year)), S("lavabo", 0, 0, 1f, 1f, NeedLabel("lavabo", Year)), S("inodoro", 0, 0, 1f, 1f, NeedLabel("inodoro", Year)));
            case MicroStyles.Civic.Policia: return Building(t, title, "Comisaría", title, Vector3.zero, 0f, new Vector2(14f, 10f), st, S("mesa", 0, 0, 1.6f, 1f, "Mostrador"), S("armero", 0, 0, 1.6f, 0.4f, "Armero"), S("armario", 0, 0, 1.2f, 0.6f, "Registros"));
            case MicroStyles.Civic.Bomberos: return Building(t, title, "Bomberos", title, Vector3.zero, 0f, new Vector2(14f, 12f), st, S("carro", 0, 0, 1.6f, 3f, Year >= 1900 ? "Autobomba" : "Bomba de mano sobre ruedas"), S("fuente", 0, 0, 4.4f, 4.4f, "Pila de agua"), S("armario", 0, 0, 1.2f, 0.6f, "Cubos, hachas y mangueras"));
            case MicroStyles.Civic.Mercado: return Building(t, title, "Mercado", title, Vector3.zero, 0f, new Vector2(18f, 10f), st, S("puesto", 0, 0, 2f, 1f, "Puesto de fruta"), S("puesto", 0, 0, 2f, 1f, "Puesto de grano"), S("puesto", 0, 0, 2f, 1f, "Puesto de telas"));
            case MicroStyles.Civic.Mecanica: return Building(t, title, "Taller", title, Vector3.zero, 0f, new Vector2(14f, 10f), st, S("banco_taller", 0, 0, 2.4f, 0.9f, "Banco"), S("torno", 0, 0, 1.6f, 0.8f, "Torno"), S("carro", 0, 0, 1.6f, 3f, Year >= 1900 ? "Coche en reparación" : "Carro con la rueda rota"));
            case MicroStyles.Civic.Taller: return Building(t, title, "Taller", title, Vector3.zero, 0f, new Vector2(14f, 10f), st, S("banco_taller", 0, 0, 2.4f, 0.9f, maya ? "Banco del lapidario (jade)" : "Banco del artesano"), S("banco_taller", 0, 0, 2.4f, 0.9f, maya ? "Banco del tallador (obsidiana)" : "Banco"));
            case MicroStyles.Civic.Milicia: return Building(t, title, "Cuartel", title, Vector3.zero, 0f, new Vector2(16f, 12f), st, S("armero", 0, 0, 1.6f, 0.4f, "Armero"), S("tatami", 0, 0, 6f, 5f, "Patio de instrucción"), S("cama", 0, 0, 1.2f, 2f, NeedLabel("cama", Year)));
            case MicroStyles.Civic.Piscina: return Building(t, title, "Baños", title, Vector3.zero, 0f, new Vector2(16f, 12f), st, maya || e.id == "islam_abasi" || e.id == "otomano" ? S("temazcal", 0, 0, 2.4f, 2.4f, maya ? "Temazcal" : "Sala caliente") : S("piscina", 0, 0, 6f, 4f, "Pila de baño"), S("lavabo", 0, 0, 1f, 1f, NeedLabel("lavabo", Year)));
            case MicroStyles.Civic.Hotel: return Storeys(t, title, "Hotel", title, st, new[] { new[] { S("barra", 0, 0, 3f, 0.8f, "Recepción"), S("mesa", 0, 0, 1.6f, 1.2f, "Comedor") }, new[] { S("cama", 0, 0, 1.2f, 2f, "Habitación"), S("cama", 0, 0, 1.2f, 2f, "Habitación") }, new[] { S("cama", 0, 0, 1.2f, 2f, "Habitación"), S("cama", 0, 0, 1.2f, 2f, "Habitación") } });
            case MicroStyles.Civic.Carpinteria: return Building(t, title, "Carpintería", title, Vector3.zero, 0f, new Vector2(14f, 10f), st, S("caballete", 0, 0, 2f, 0.6f, "Caballetes y tablón"), S("banco_taller", 0, 0, 2.4f, 0.9f, "Banco del carpintero"));
            case MicroStyles.Civic.Obra: return Building(t, title, "Obra", title, Vector3.zero, 0f, new Vector2(14f, 10f), st, S("material", 0, 0, 2f, 2.4f, maya ? "Piedra caliza, cal y vigas de chicozapote" : "Material de la obra"), S("caballete", 0, 0, 2f, 0.6f, "Caballete de la obra"));
            case MicroStyles.Civic.Tecnologia: return Building(t, title, "Centro de tecnología", title, Vector3.zero, 0f, new Vector2(14f, 10f), st, S("consola", 0, 0, 1.6f, 0.7f, title), S("consola", 0, 0, 1.6f, 0.7f, title), S("mesa", 0, 0, 1.6f, 1f, "Mesa de los registros"));
            case MicroStyles.Civic.Edificio: return Storeys(t, title, "Casa", title, st, new[] { new[] { S("cama", 0, 0, 1.2f, 2f, NeedLabel("cama", Year)), S("cama", 0, 0, 1.2f, 2f, NeedLabel("cama", Year)) }, new[] { S("cama", 0, 0, 1.2f, 2f, NeedLabel("cama", Year)), S("mesa", 0, 0, 1.6f, 1f, NeedLabel("mesa", Year)) }, new[] { S("cama", 0, 0, 1.2f, 2f, NeedLabel("cama", Year)), S("cama", 0, 0, 1.2f, 2f, NeedLabel("cama", Year)) } });
        }
        return null;
    }

    // Lleva el edificio (en el origen) al solar libre más cercano a «near», de frente a la calle si la hay. false si no hay sitio.
    static bool PlaceOn(Lots lots, AreaFunctionSite site, Vector2 near)
    {
        Vector2 size = SiteSize(site.transform);
        Vector3? at = lots.Take(size + new Vector2(2f, 2f), near);
        if (!at.HasValue) return false;
        site.transform.localPosition = at.Value;
        return true;
    }

    // La huella de un edificio (su «Suelo», girado), en planta.
    static Vector2 SiteSize(Transform b)
    {
        Transform f = b.Find("Suelo");
        Vector2 size = f != null ? new Vector2(f.localScale.x, f.localScale.z) : new Vector2(12f, 10f);
        float yaw = b.localEulerAngles.y;
        if (Mathf.Abs(Mathf.DeltaAngle(yaw, 90f)) < 45f || Mathf.Abs(Mathf.DeltaAngle(yaw, 270f)) < 45f) size = new Vector2(size.y, size.x);
        return size;
    }

    // ══ 3 · LAS CASAS A LO LARGO DE LAS CALLES ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// EL RELLENO DEL PUEBLO SOBRE SUS CALLES: hasta <paramref name="max"/> edificios (rotando <paramref name="kinds"/>: casas,
    /// talleres, tiendas de la era) a cada lado de las calles del plano, DE FRENTE A LA CALLE (el lado de uso de sus muebles mira a
    /// ella), empezando por las más cercanas a <see cref="TownPlan.fillNear"/>. Devuelve los sitios.
    /// </summary>
    static List<AreaFunctionSite> PlanFill(Transform t, Lots lots, TownPlan p, Lot[] kinds, int max)
    {
        var made = new List<AreaFunctionSite>();
        if (p == null || kinds == null || kinds.Length == 0 || max <= 0) return made;
        _fillDone = true;
        // Los sitios candidatos: cada 16 m por cada tramo de calle, a los dos lados, con la normal hacia la calle.
        var cand = new List<(Vector2 at, Vector2 toStreet, float half)>();
        float s = p.scale;
        foreach (Feat f in p.feats)
        {
            if (f.k != FK.Calle) continue;
            float w = Mathf.Max(4f, f.a);
            for (int i = 0; i + 1 < f.p.Count; i++)
            {
                Vector2 a = f.p[i] * s, b = f.p[i + 1] * s, d = b - a; float len = d.magnitude;
                if (len < 10f) continue;
                d /= len; Vector2 nrm = new Vector2(-d.y, d.x);
                for (float u = 8f; u < len - 4f; u += 16f)
                    foreach (int side in new[] { 1, -1 }) cand.Add((a + d * u + nrm * side * (w * 0.5f), -nrm * side, w * 0.5f));
            }
        }
        Vector2 from = PlanPoint(p.fillNear);
        cand.Sort((x, y) => (x.at - from).sqrMagnitude.CompareTo((y.at - from).sqrMagnitude));
        int k = 0, tried = 0;
        foreach (var c in cand)
        {
            if (made.Count >= max || Count(t) >= PlanMaxSites) break;
            Lot l = kinds[k % kinds.Length];
            // De frente a la calle: el frente de uso de un edificio es su −z local.
            Vector2 n = c.toStreet; float yaw = Mathf.Abs(n.x) > Mathf.Abs(n.y) ? (n.x > 0f ? 270f : 90f) : (n.y > 0f ? 180f : 0f);
            string name = $"{l.name} {k / kinds.Length + 1}";
            var st = l.stations != null && l.stations.Length > 0 ? l.stations : new[] { S("cama", 0f, 0f, 1f, 2f, NeedLabel("cama", Year)) };
            AreaFunctionSite site = Building(t, name, l.template, l.title, Vector3.zero, yaw, l.size, l.style, st);
            Vector2 size = SiteSize(site.transform) + new Vector2(2f, 2f);
            // Su centro, a una calle de la calle (el paso de 4 m de Lots) más medio fondo.
            float depth = Mathf.Abs(n.x) > Mathf.Abs(n.y) ? size.x : size.y;
            Vector2 at = c.at - n * (MicroBuildRules.Street + 1f + depth * 0.5f);
            tried++;
            if (lots.Free(at, size)) { lots.Mark(new Rect(at - size * 0.5f, size)); site.transform.localPosition = new Vector3(at.x, 0f, at.y); made.Add(site); k++; }
            else Object.DestroyImmediate(site.gameObject);
        }
        Debug.Log($"[Pueblos] {p.town}: {made.Count} casas y talleres a lo largo de sus calles (de {cand.Count} sitios del plano; {tried} probados).");
        return made;
    }

    // ══ LOS EDIFICIOS DE VARIOS PISOS (en terrazas) ═════════════════════════════════════════════════════════════════

    /// <summary>
    /// UN EDIFICIO DE VARIOS PISOS sin techos (regla 2): sus plantas son TERRAZAS escalonadas hacia atrás (la planta k, un bloque
    /// macizo de k × 3,2 m de alto detrás de la anterior), así que ningún piso tapa a otro; se sube por una RAMPA lateral de 20°.
    /// Cada planta lleva sus muebles con el paso de 2 m (<see cref="Layout"/>). Las terrazas y la rampa son «planta» y «rampa»
    /// (MicroPiece que se pisa). Es un sitio del registro de la plantilla <paramref name="template"/>.
    /// </summary>
    static AreaFunctionSite Storeys(Transform t, string name, string template, string title, Style st, (string key, Vector2 at, Vector2 size, string label)[][] floors)
    {
        const float H = 3.2f;
        Transform b = new GameObject(name).transform; b.SetParent(t, false);
        var groups = new List<(Transform g, List<Transform> pieces)>();
        float width = 10f;
        for (int i = 0; i < floors.Length; i++)
        {
            Transform g = new GameObject($"Planta {i + 1}").transform; g.SetParent(b, false);
            var pieces = Furnish(g, i == 0 ? template : "_planta", floors[i]);
            if (i == 0 && template == "Casa") { }   // (la planta baja de una casa ya trae sus necesidades y su frutal)
            groups.Add((g, pieces));
            foreach (Transform pc in pieces) width = Mathf.Max(width, MicroFurniture.LocalBounds(pc).size.x + 2f * MicroBuildRules.Passage);
        }
        var depths = new List<float>(); float total = 0f;
        foreach (var (g, pieces) in groups) { Vector2 used = Layout(g, pieces, new Vector2(width, 6f), 0f); width = Mathf.Max(width, used.x); depths.Add(used.y + 3f); total += used.y + 3f; }   // (+3 m: holgura para cuando los muebles se rehacen a la medida de cada hogar)
        foreach (var (g, pieces) in groups) Layout(g, pieces, new Vector2(width, 6f), 0f);   // todas al mismo ancho
        Material wall = AreaMicrocosmos.Mat("Civ_Muro_" + Hex(st.wall), st.wall), ramp = AreaMicrocosmos.Mat("Civ_Rampa_" + Hex(st.floor), st.floor);
        float z = -total * 0.5f;
        for (int i = 0; i < groups.Count; i++)
        {
            float d = depths[i], zc = z + d * 0.5f;
            groups[i].g.localPosition = new Vector3(0f, i * H, zc);
            if (i > 0)
            {
                Transform tier = new GameObject($"Terraza de la planta {i + 1}").transform; tier.SetParent(b, false); tier.localPosition = new Vector3(0f, 0f, zc);
                AreaMicrocosmos.Box(tier, "Bloque", new Vector3(0f, i * H * 0.5f, 0f), new Vector3(width, i * H, d), wall, true);
                AreaMicrocosmos.Box(tier, "Pretil", new Vector3(0f, i * H + 0.4f, d * 0.5f - 0.1f), new Vector3(width, 0.8f, 0.2f), wall, true);   // baranda baja al fondo (0,8 m: no es muro)
                var mp = tier.gameObject.AddComponent<MicroPiece>(); mp.kind = "planta"; mp.walkable = true;
                // La rampa: por el lado +x, sube de la planta anterior a esta a lo largo de su frente.
                float run = H / Mathf.Tan(20f * Mathf.Deg2Rad);
                Transform rp = new GameObject($"Rampa a la planta {i + 1}").transform; rp.SetParent(b, false);
                float rz = z - run * 0.5f, ry = (i - 0.5f) * H;
                AreaMicrocosmos.Box(rp, "Rampa", new Vector3(width * 0.5f + 0.9f, ry, rz), new Vector3(1.6f, 0.25f, run / Mathf.Cos(20f * Mathf.Deg2Rad)), Quaternion.Euler(-20f, 0f, 0f), ramp, true);
                var rm = rp.gameObject.AddComponent<MicroPiece>(); rm.kind = "rampa"; rm.walkable = true;
            }
            z += d;
        }
        Floor(b, new Vector2(width + 4f, total), st);
        var site = b.gameObject.AddComponent<AreaFunctionSite>(); site.building = template; site.title = title; site.region = Region;
        Tag(site, AreaFunctionSite.InteractionOf(template) + $" (en {groups.Count} plantas en terraza, con rampa)");
        var wl = b.gameObject.AddComponent<WorldLabel>(); wl.text = title; wl.height = groups.Count * H + 2f;
        return site;
    }

    // ══ EL PUEBLO SOBRE LOS MUEBLES DEL PISO (barrio alto) Y LOS MIRADORES ═════════════════════════════════════════════

    /// <summary>
    /// REGLA DE LOS MICROCOSMOS (2026-11-03, Berón: «el piso no debe romperse, las ciudades insecto deben adecuarse al piso … los
    /// insecto-humanos pueden trepar plataformas verticales como las paredes y patas de sillas, mesas, etc., de modo que no tienen
    /// problema con subir a mesas, camas, etc., en caso que parte de los pueblos se cree encima de estructuras elevadas»). Las caras
    /// de arriba de los muebles del piso (mesas, camas, encimeras: mesetas de decenas de metros) son suelo del pueblo:
    /// - DENTRO del barrio, un BARRIO ALTO: en cada una de las más grandes (hasta 6), una casa de la era (con sus necesidades y su
    ///   frutal) y, en la mayor, un mercado; se sube trepando (AreaMicrocosmos pone los NavMeshLink de trepar);
    /// - FUERA del barrio, en las dos más grandes, un MIRADOR de la era (su monumento, un frutal del lugar y bancos).
    /// <paramref name="tops"/>: cajas planas a la altura de cada cara de arriba (espacio de la raíz), ya sin las que tienen algo encima.
    /// Devuelve cuántos edificios levantó encima.
    /// </summary>
    internal static int Outposts(Transform cast, List<Bounds> tops, Bounds district)
    {
        if (tops == null || tops.Count == 0) return 0;
        var sorted = new List<Bounds>(tops);
        sorted.Sort((a, b) => (b.size.x * b.size.z).CompareTo(a.size.x * a.size.z));
        MicroStyles.Entry e = MicroStyles.For(Year, Region);
        Style st = _planStyle ?? NeedsStyle;
        var used = new List<Vector3>();
        int high = 0, look = 0;
        bool Near(Bounds top) { foreach (Vector3 u in used) if (Vector2.Distance(new Vector2(u.x, u.z), new Vector2(top.center.x, top.center.z)) < 40f) return true; return false; }
        bool Fits(Bounds top, Transform b)
        {
            Transform f = b.Find("Suelo");
            Vector2 s = f != null ? new Vector2(f.localScale.x, f.localScale.z) : new Vector2(12f, 10f);
            return top.size.x >= s.x + 2f && top.size.z >= s.y + 2f;
        }
        foreach (Bounds top in sorted)
        {
            bool inside = top.center.x > district.min.x && top.center.x < district.max.x && top.center.z > district.min.z && top.center.z < district.max.z;
            if (inside ? high >= 6 : look >= 2) continue;
            if (top.size.x < 16f || top.size.z < 14f || Near(top)) continue;
            AreaFunctionSite site;
            if (inside)
            {
                string title = high == 0 && MicroStyles.CivicName(MicroStyles.Civic.Mercado, e, Year) != null ? "Mercado del barrio alto" : $"Casa del barrio alto {high + 1}";
                site = title.StartsWith("Mercado")
                    ? Building(cast, title, "Mercado", $"{MicroStyles.CivicName(MicroStyles.Civic.Mercado, e, Year)} (sobre el mueble)", Vector3.zero, 0f, new Vector2(14f, 10f), st,
                               S("puesto", 0, 0, 2f, 1f, "Puesto de fruta"), S("puesto", 0, 0, 2f, 1f, "Puesto de grano"))
                    : Building(cast, title, "Casa", $"Casa del barrio alto (sobre el mueble)", Vector3.zero, 0f, new Vector2(10f, 8f), st, S("cama", 0, 0, 1f, 2f, NeedLabel("cama", Year)), S("cama", 0, 0, 1f, 2f, NeedLabel("cama", Year)));
            }
            else
            {
                string monument = MicroStyles.CivicName(MicroStyles.Civic.Monumento, e, Year) ?? "Piedra del mirador";
                site = Building(cast, $"{OutpostPrefix} de la meseta {look + 1}", "Mirador", $"Mirador sobre el mueble gigante ({e.name})", Vector3.zero, 0f, new Vector2(12f, 10f), st,
                    S("estatua", 0, 0, 1.4f, 1.4f, monument), S("frutal", 0, 0, 1.6f, 1.6f, MicroFurniture.FruitFor(e, look).name));
            }
            if (!Fits(top, site.transform)) { Object.DestroyImmediate(site.gameObject); continue; }
            site.transform.localPosition = cast.InverseTransformPoint(new Vector3(top.center.x, top.max.y, top.center.z));
            used.Add(top.center);
            if (inside) high++; else look++;
        }
        if (high + look > 0) Debug.Log($"[Pueblos] Sobre los muebles del piso: {high} edificios del barrio alto (dentro del pueblo) y {look} miradores (fuera), de {tops.Count} caras de arriba libres; se sube trepando.");
        return high + look;
    }
}
