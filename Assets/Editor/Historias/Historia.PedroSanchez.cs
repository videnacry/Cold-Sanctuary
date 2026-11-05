using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PEDRO SÁNCHEZ Y LA MOCIÓN DE CENSURA DE 2018 (docs/microcosmos-historias-3.md). Una escena CÍVICA y NEUTRAL de un político en activo: Madrid,
/// 31 de mayo a 2 de junio de 2018. La protagonista es la institución: cómo funciona el artículo 113 de la Constitución. A los políticos vivos NO
/// se les ponen palabras en la boca: lo explican una letrada y un ujier inventados; lo único que dicen los reales es de registro público (la
/// presidenta del Congreso lee el recuento y Sánchez, la fórmula legal de la promesa). La ficha no valora su gestión.
/// </summary>
public static partial class StorySceneBuilder
{
    static int PedroSanchez(Transform t)
    {
        Material red = M("Tapiz_del_hemiciclo", 0.55f, 0.1f, 0.12f), wood = M("Nogal_del_Congreso", 0.4f, 0.26f, 0.16f), stone = M("Piedra_de_Colmenar", 0.82f, 0.78f, 0.7f), bronze = M("Bronce_de_los_leones", 0.45f, 0.36f, 0.22f);
        Material paper = M("Boletin_oficial", 0.92f, 0.9f, 0.82f), screen = M("Marcador_de_votos", 0.1f, 0.14f, 0.2f), green = M("Luz_verde", 0.2f, 0.7f, 0.3f), redL = M("Luz_roja", 0.8f, 0.15f, 0.12f);

        // 1 · LA PUERTA DE LOS LEONES: la entrada del Congreso, con su pórtico.
        Transform gate = Spot(t, "Puerta de los Leones", new Vector3(0f, 0f, 2f), "La Puerta de los Leones (Congreso de los Diputados)", 3f);
        for (int k = 0; k < 2; k++) { Prop(gate, "Peana del león", new Vector3(-6f + k * 12f, 0.5f, 0f), new Vector3(1.6f, 1f, 2.4f), stone); Prop(gate, "León de bronce", new Vector3(-6f + k * 12f, 1.5f, 0f), new Vector3(1f, 1f, 2f), bronze); }
        for (int k = 0; k < 6; k++) Prop(gate, "Columna del pórtico", new Vector3(-3f + k * 1.2f, 2.2f, 3f), new Vector3(0.5f, 4.4f, 0.5f), stone);

        // 2 · LA CONSTITUCIÓN: el artículo 113 en un atril.
        Transform article = Spot(t, "Atril de la Constitución", new Vector3(-15f, 0f, 7f), "La Constitución de 1978, artículo 113", 2.4f);
        Prop(article, "Atril", new Vector3(0f, 0.55f, 0f), new Vector3(1f, 1.1f, 0.7f), wood);
        Prop(article, "Constitución abierta", new Vector3(0f, 1.13f, 0f), new Vector3(0.7f, 0.05f, 0.5f), paper, false);

        // 3 · EL ESCRITO DE LA MOCIÓN: lo que necesita para entrar en el registro.
        Transform registry = Spot(t, "Registro del Congreso", new Vector3(15f, 0f, 6f), "El Registro del Congreso", 2.4f);
        Prop(registry, "Mostrador del registro", new Vector3(0f, 0.5f, 0f), new Vector3(3f, 1f, 0.9f), wood);
        var docs = Items(t, paper, new Vector3(0.6f, 0.1f, 0.45f), ("Escrito de la moción", new Vector3(20f, 0.5f, 1f)), ("Firmas de 35 diputados (una décima parte)", new Vector3(18f, 0.5f, -4f)), ("Nombre del candidato a la presidencia", new Vector3(14f, 0.5f, -7f)));

        // 4 · EL HEMICICLO: escaños, tribuna y marcador. Los tres sitios que hay que visitar.
        Transform chamber = Spot(t, "Hemiciclo", new Vector3(0f, 0f, 16f), "El hemiciclo: 350 escaños", 4f);
        for (int r = 0; r < 3; r++) for (int k = 0; k < 7; k++) { float a = Mathf.PI * (0.15f + k * 0.117f); float rad = 5f + r * 1.5f; Prop(chamber, "Fila de escaños", new Vector3(Mathf.Cos(a) * rad, 0.3f + r * 0.25f, Mathf.Sin(a) * rad), new Vector3(1.2f, 0.6f, 0.6f), red); }
        Prop(chamber, "Marcador", new Vector3(0f, 3.4f, 9f), new Vector3(5f, 1.6f, 0.15f), screen, false);
        Prop(chamber, "Barra de votos a favor", new Vector3(-1f, 3.4f, 8.9f), new Vector3(1.8f, 0.4f, 0.02f), green, false);
        Prop(chamber, "Barra de votos en contra", new Vector3(1f, 3.2f, 8.9f), new Vector3(1.7f, 0.4f, 0.02f), redL, false);
        Transform tribune = Spot(t, "Tribuna de oradores", new Vector3(0f, 0f, 12f), "La tribuna de oradores", 2.2f);
        Prop(tribune, "Tribuna", new Vector3(0f, 0.6f, 0f), new Vector3(1.6f, 1.2f, 0.8f), wood);
        Transform press = Spot(t, "Tribuna de prensa", new Vector3(-13f, 0f, -6f), "La tribuna de prensa", 2f);
        Prop(press, "Mesa de prensa", new Vector3(0f, 0.4f, 0f), new Vector3(2.4f, 0.8f, 0.8f), wood);
        Transform visitors = Spot(t, "Tribuna de invitados", new Vector3(13f, 0f, -8f), "La tribuna de invitados: el pleno es público", 2f);
        Prop(visitors, "Banco de invitados", new Vector3(0f, 0.3f, 0f), new Vector3(2.6f, 0.6f, 0.7f), wood);
        var open = new List<Transform> { tribune, press, visitors };

        // 5 · EL PALACIO DE LA ZARZUELA: la mesa de la promesa, con la Constitución y sin símbolos religiosos.
        Transform oath = Spot(t, "Mesa de la promesa", new Vector3(-9f, 0f, -9f), "La Zarzuela, 2 de junio: la promesa del cargo", 2.4f);
        Prop(oath, "Mesa de la promesa", new Vector3(0f, 0.4f, 0f), new Vector3(1.4f, 0.8f, 0.8f), wood);
        Prop(oath, "Ejemplar de la Constitución", new Vector3(0f, 0.84f, 0f), new Vector3(0.45f, 0.06f, 0.32f), paper, false);

        // La gente: insectos-humanos. Todos ABEJAS, tanto el candidato como el presidente saliente (el enjambre decide contando apoyos: Seeley 2010);
        // el insecto es por la institución, no por nadie. Traje oscuro.
        Color navy = new Color(0.12f, 0.15f, 0.25f), grey = new Color(0.32f, 0.33f, 0.36f), wine = new Color(0.4f, 0.14f, 0.2f), black = new Color(0.1f, 0.1f, 0.12f);
        GameObject sanchez = Modern(t, "Pedro Sánchez", new Vector3(3f, 0f, 9f), false, navy, CubeInsect.Hat.None, 1.9f, "SnowBee");
        GameObject rajoy = Modern(t, "Mariano Rajoy", new Vector3(-4f, 0f, 9f), false, grey, CubeInsect.Hat.None, 1.88f, "SnowBee");
        GameObject pastor = Modern(t, "Ana Pastor", new Vector3(0f, 0f, 7f), true, wine, CubeInsect.Hat.None, 1.62f, "SnowBee");
        GameObject lawyer = Modern(t, "Letrada de las Cortes (Ruth)", new Vector3(-12f, 0f, 4f), true, black, CubeInsect.Hat.None, 1.65f, "Ant"); Routine(lawyer, article, chamber);
        GameObject usher = Modern(t, "Ujier del Congreso (Atlas)", new Vector3(6f, 0f, -3f), false, black, CubeInsect.Hat.None, 1.75f, "Ant"); Routine(usher, gate);
        GameObject reporter = Modern(t, "Periodista parlamentaria (Medea)", new Vector3(-11f, 0f, -8f), true, grey, CubeInsect.Hat.None, 1.65f, "Cricket"); Routine(reporter, press);
        int n = 6;

        Mission(t, "El Congreso de los Diputados: la moción de censura de 2018", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("Habla con el ujier en la Puerta de los Leones", usher.transform, "Ujier", "Hoy está esto lleno. Va a pasar algo que no ha pasado nunca en la democracia: puede cambiar el presidente del Gobierno sin elecciones. Es legal, y está escrito. Pregunte a la letrada, y ella se lo explica."),
                Reach("Ve al atril de la Constitución con la letrada", article, "Letrada de las Cortes", "Artículo 113: el Congreso puede echar al Gobierno, pero solo con un candidato alternativo y con mayoría absoluta: 176 de 350. Se llama censura constructiva: no se puede tumbar sin proponer a otro. Viene de la Constitución alemana. Lo pide una décima parte de los diputados: 35."),
                Collect("Lleva al registro lo que exige la Constitución", docs, registry, "Letrada de las Cortes", "El escrito, las firmas y el candidato. Lo presentó el Grupo Socialista el 25 de mayo, un día después de la sentencia del caso Gürtel, y pasados cinco días como mínimo se debate y se vota. Todo tiene un plazo: las reglas son iguales para todos."),
                Visit("Recorre el pleno: la tribuna, la prensa y los invitados", open, "Periodista parlamentaria", "Dos días de debate. Quien presenta la moción y el candidato hablan; contesta el Gobierno, y cada grupo tiene su turno. Todo es público: aquí se escucha lo que luego se discute en la calle. Unos lo llaman necesario; otros, un error. Yo solo apunto."),
                Talk("Habla con Ana Pastor, presidenta del Congreso", pastor.transform, "Ana Pastor", "Votación por llamamiento: cada diputado dice en voz alta sí, no o abstención. Son 350."),
                Hold("Mira cómo se vota, nombre a nombre", chamber, 12f, "Ana Pastor", "Votos emitidos, 350. A favor, 180. En contra, 169. Abstenciones, 1. Queda aprobada la moción de censura. Es la primera que prospera desde 1978."),
                Talk("Mira al presidente saliente y al nuevo presidente", sanchez.transform, "Letrada de las Cortes", "El presidente saliente se acerca y le da la mano al que llega: así termina un gobierno en una democracia. Se ganó con votos de ocho partidos distintos. Le toca ahora el juramento o la promesa."),
                Hold("La Zarzuela, 2 de junio: la promesa del cargo", oath, 8f, "Pedro Sánchez", "Prometo por mi conciencia y honor cumplir fielmente las obligaciones del cargo de presidente del Gobierno con lealtad al Rey, guardar y hacer guardar la Constitución como norma fundamental del Estado."),
            },
            "El Congreso de los Diputados, en Madrid, el 1 de junio de 2018, sobre el Aula del santuario a escala de hormiga (pendiente de su piso: una sala de asambleas). Una escena sobre cómo funciona un parlamento, con hechos de registro público. Acompaña a una letrada y mira cómo se vota. E para hablar y recoger; para mirar, quédate quieto.",
            new[]
            {
                "España es una monarquía parlamentaria: los ciudadanos eligen a los diputados, y el Congreso inviste al presidente del Gobierno. El artículo 113 de la Constitución de 1978 permite retirarle la confianza con una moción de censura «constructiva»: necesita a una décima parte de los diputados para presentarse, un candidato alternativo y la mayoría absoluta para aprobarse. Hasta 2018 se habían presentado tres (1980, 1987 y 2017) y ninguna había salido adelante.",
                "El 24 de mayo de 2018 la Audiencia Nacional dictó sentencia en el caso Gürtel, que afectaba a dirigentes y al partido del Gobierno, el PP. Al día siguiente, el PSOE registró una moción de censura con su secretario general, Pedro Sánchez (Madrid, 1972), como candidato; Sánchez no era entonces diputado. Se debatió el 31 de mayo y el 1 de junio, y se aprobó con 180 votos a favor, 169 en contra y una abstención: la primera que prosperaba. Votaron a favor PSOE, Unidos Podemos, ERC, PDeCAT, PNV, Compromís, EH Bildu y Nueva Canarias.",
                "Al día siguiente, 2 de junio, Sánchez tomó posesión ante el rey Felipe VI en el palacio de la Zarzuela, con la fórmula de la promesa y con la Constitución, sin Biblia ni crucifijo, algo que la Casa del Rey permitía desde 2014 y que nadie había hecho hasta entonces en un acto así. Mariano Rajoy, presidente desde 2011, estuvo presente.",
                "Pedro Sánchez ganó después las elecciones de 2019 y formó en 2020 el primer gobierno de coalición de la democracia, y fue investido de nuevo en 2023. Su gestión y sus pactos son objeto de debate político, sobre el que esta ficha no opina. A fecha de esta ficha (noviembre de 2026) sigue siendo presidente; el Real Decreto 806/2026 disolvió las Cortes y convocó elecciones generales para el 29 de noviembre de 2026. Las frases de la letrada, el ujier y la periodista son de juego; las de Ana Pastor resumen el acta, y la de Sánchez es la fórmula legal.",
            },
            new[]
            {
                "Constitución Española de 1978, arts. 113–114; Reglamento del Congreso, arts. 175–179.",
                "Congreso de los Diputados, documentación de la moción de censura de 2018, XII Legislatura (congreso.es/docu/MocCens/10062018_XIILegMocCen.pdf).",
                "Real Decreto 707/1979, sobre la fórmula de juramento o promesa de cargos públicos (BOE).",
                "CBS News (2-6-2018), «Pedro Sanchez sworn in as Spain's prime minister»; Periódico de Ibiza (2-6-2018), «Pedro Sánchez promete el cargo ante el Rey».",
                "Real Decreto 806/2026, de disolución de las Cortes y de convocatoria de elecciones (BOE, 6-10-2026); Newtral (5-10-2026), calendario electoral.",
            },
            "Letrada de las Cortes", "Aquí no gana quien grita más, sino quien tiene los votos y respeta el reglamento.");
        return n;
    }
}

internal static partial class CivicBuilder
{
    static readonly Style MadridStone = new Style(C(0.82f, 0.78f, 0.7f), C(0.6f, 0.56f, 0.5f), C(0.4f, 0.4f, 0.42f), Roof.Flat, 5f);       // piedra de los edificios públicos
    static readonly Style MadridBrick = new Style(C(0.72f, 0.5f, 0.38f), C(0.65f, 0.6f, 0.54f), C(0.42f, 0.34f, 0.3f), Roof.Flat, 4f);     // ladrillo y enfoscado del centro

    // MADRID (2018): el Congreso en la Carrera de San Jerónimo, la plaza de las Cortes, Neptuno y el paseo del Prado al este, el Retiro, Sol al oeste,
    // Alcalá y la Gran Vía al norte, Atocha al sur. La Zarzuela (15 km al noroeste), ACERCADA para el último paso. (Ayuntamiento de Madrid; OpenStreetMap.)
    const string PlanMadrid = @"
calle Carrera de San Jerónimo | 12 | -800 10, 480 -20
calle Paseo del Prado | 34 | 520 900, 520 -1000
calle Calle de Alcalá | 24 | -800 320, 1400 340
calle Gran Vía | 22 | -1200 760, 320 420
calle Calle de Atocha | 12 | -800 -300, 520 -900
calle Calle de las Huertas | 8 | -600 -160, 520 -300
calle Calle del Prado | 8 | -300 -60, 380 -120
calle Calle de la Montera | 8 | -800 40, -640 700
calle Calle del Barquillo | 8 | 320 340, 200 1100
campo Parque del Retiro | 460 500 | 1100 -240
campo Jardines del Paseo del Prado | 60 400 | 520 -300
lago Estanque del Retiro | 140 60 | 1050 0
hito 0 | 40 70
hito 1 | -900 780
hito 2 | -100 110
hito 3 | 170 90
hito 4 | -340 -240
hito 5 | -420 -360
hito 6 | 660 -120
hito 7 | 300 700
Plaza | -760 30
Pileta | 500 40
Mercado | -300 -500
Parque | 860 -500
Piscina | -760 -480
Entrenamiento | -620 -640 | actual: Polideportivo municipal de La Latina; posición aproximada
Sanacion | 220 -440
Bomberos | -560 480
Policia | -180 -120
Estudio | -120 -640
Guarderia | 80 -760
Hotel | 250 -30
Investigacion | 380 -700
Estacion | 560 -800
Baile | -300 400
Taller | -660 -180
Carpinteria | -860 -300
Mecanica | 120 880
Tecnologia | -320 620
Monumento | 490 110
Obra | 380 1050
Edificio | -120 290
relleno | 60 | -300 -300";

    static int MadridTown(Transform t)
    {
        var s = Town(t, "Madrid 2018", new[]
        {
            new Lot("Palacio de las Cortes", "Taller de papeles", "Palacio de las Cortes (Congreso de los Diputados)", 20f, 14f, MadridStone,
                S("mesa", -5f, 3f, 2.4f, 1f, "Mesa del Congreso"), S("mesa", 3f, 3f, 2.4f, 1f, "Mesa de los letrados"), S("estante", 0f, -4f, 1.6f, 0.6f, "Diarios de Sesiones"), S("armario", 6f, -4f, 1.2f, 0.6f, "Archivo del Congreso")),
            new Lot("Palacio de la Zarzuela", "Taller de papeles", "Palacio de la Zarzuela (acercado)", 18f, 12f, MadridStone,
                S("mesa", 0f, 3f, 2.4f, 1f, "Mesa de las audiencias"), S("estante", 5f, -3f, 1.2f, 0.6f, "Archivo de la Casa del Rey")),
            new Lot("Cafetería del Congreso", "Restaurante", "Cafetería del Congreso", 14f, 10f, MadridBrick,
                S("cocina", -3f, 2f, 2f, 0.8f, "Cocina"), S("mesa", 2f, -1f, 2f, 1f, "Mesas"), S("barra", 3f, 3f, 2f, 0.8f, "Barra del café")),
            new Lot("Sala de prensa", "Centro de tecnología", "Sala de prensa del Congreso", 14f, 10f, MadridBrick,
                S("consola", -3f, 2f, 1.6f, 0.8f, "Puesto de redacción"), S("consola", 3f, 2f, 1.6f, 0.8f, "Cabina de radio"), S("mesa", 0f, -3f, 2f, 1f, "Mesa de ruedas de prensa")),
            new Lot("Piso de las Huertas", "Casa", "Piso de la calle de las Huertas", 12f, 10f, MadridBrick,
                S("cama", -2f, 2f, 1.4f, 2f, "Cama"), S("cama", 3f, 2f, 1.2f, 2f, "Cama"), S("estante", 3f, -3f, 0.8f, 1.6f, "Libros")),
            new Lot("Mercado de Antón Martín", "Mercado", "Mercado de Antón Martín", 16f, 12f, MadridBrick,
                S("puesto", -4f, 2f, 2f, 1f, "Puesto de fruta"), S("puesto", 0f, 2f, 2f, 1f, "Puesto de pescado"), S("puesto", 4f, 2f, 2f, 1f, "Puesto de pan")),
            new Lot("Museo del Prado", "Museo", "Museo del Prado", 20f, 12f, MadridStone,
                S("vitrina", -5f, 2f, 1.6f, 0.6f, "Sala de Velázquez"), S("vitrina", 0f, 2f, 1.6f, 0.6f, "Sala de Goya"), S("estatua", 5f, 2f, 1.4f, 1.4f, "Pieza mayor de la colección")),
            new Lot("Biblioteca Nacional", "Escuela", "Biblioteca Nacional de España", 18f, 12f, MadridStone,
                S("estante", -5f, 3f, 2f, 0.6f, "Depósito de libros"), S("mesa", 0f, -2f, 2.4f, 1f, "Sala de lectura"), S("pizarra", 5f, -3f, 1.6f, 0.4f, "Panel de exposiciones")),
        }, new[]
        {
            new Lot("Casa de Madrid", "Casa", "Edificio de pisos del centro de Madrid", 12f, 10f, MadridBrick),
            new Lot("Bar del barrio", "Restaurante", "Bar del barrio", 10f, 8f, MadridBrick, S("fogon", 0f, 0f, 1.6f, 0.8f, "Plancha"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa con taburetes")),
            new Lot("Tienda del barrio", "Mercado", "Tienda del barrio", 10f, 8f, MadridBrick, S("puesto", 0f, 0f, 2f, 1f, "Mostrador"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
            new Lot("Librería", "Mercado", "Librería de la Cuesta de Moyano", 10f, 8f, MadridBrick, S("puesto", 0f, 0f, 2f, 1f, "Caseta de libros"), S("estante", 0f, 0f, 0.8f, 1.6f, "Estantes")),
        }, C(0.42f, 0.42f, 0.42f));
        int n = 0;
        n += W(t, "Taquígrafa del Congreso", s[0], "", s[2], null, s[4]);
        n += W(t, "Bedel del Congreso", s[0], "", s[2], null, null);
        n += W(t, "Secretario de la Casa del Rey", s[1], "", s[2], null, null);
        n += W(t, "Camarera de la cafetería", s[2], "cocinero", s[2], null, null);
        n += W(t, "Reportero de radio", s[3], "", s[2], null, null);
        n += W(t, "Vendedora del mercado", s[5], "comerciante", s[2], null, null);
        n += W(t, "Guía del Prado", s[6], "sabio", s[2], null, null);
        n += W(t, "Bibliotecaria", s[7], "sabio", s[2], null, null);
        n += Townsfolk(t, s, 8, "Vecina de Madrid", "Vecino de Madrid", s[2], null);
        return n;
    }
}

public static partial class MicroBiome
{
    // Madrid, junio: ciudad, con los árboles de sus paseos (plátano de sombra, tilo, castaño de Indias, olmo) y el Retiro; abejas y hormigas en
    // el parque; perros y gatos en las calles. (Ayuntamiento de Madrid, Arbolado urbano.)
    static Biome MadridBiome() => new Biome { title = "Madrid (junio de 2018): ciudad", ground = C(0.62f, 0.58f, 0.5f), plants = new[] {
            new Plant(Kind.Grass, 12, "Césped del Retiro, segado", 6, C(0.42f, 0.6f, 0.3f), C(0.5f, 0.58f, 0.3f)),
            new Plant(Kind.Flower, 4, "Rosa de la Rosaleda (Rosa)", 100, C(0.28f, 0.45f, 0.25f), C(0.85f, 0.25f, 0.35f)),
            new Plant(Kind.Stone, 8, "Bordillo de granito", 0, C(0.65f, 0.65f, 0.65f), Stone) },
          town = new[] {
            new Plant(Kind.Shrub, 7, "Plátano de sombra (Platanus × hispanica), de paseo", 2200, C(0.3f, 0.5f, 0.28f), C(0.6f, 0.55f, 0.45f)),
            new Plant(Kind.Shrub, 5, "Tilo (Tilia cordata), de calle", 1600, C(0.3f, 0.5f, 0.28f), C(0.75f, 0.7f, 0.4f)),
            new Plant(Kind.Shrub, 4, "Castaño de Indias (Aesculus hippocastanum), del Retiro", 1800, C(0.28f, 0.48f, 0.26f), C(0.5f, 0.35f, 0.22f)),
            new Plant(Kind.Shrub, 3, "Almendro (Prunus dulcis), de los parques", 400, C(0.42f, 0.5f, 0.38f), C(0.7f, 0.75f, 0.5f)) },
          bugs = new[] {
            new Bug("SnowBee", 3, 14, C(0.8f, 0.6f, 0.2f), "Abeja del Retiro (Apis mellifera)"),
            new Bug("Ant", 4, 5, C(0.25f, 0.18f, 0.14f), "Hormiga negra (Lasius niger)") },
          townFauna = new[] {
            new Pet("Woodlouse", 4, 0.7f, C(0.75f, 0.65f, 0.5f), "Perro de paseo (en el microcosmos, una cochinilla hormiguera)"),
            new Pet("Woodlouse", 2, 0.5f, C(0.85f, 0.83f, 0.78f), "Gato de la calle (en el microcosmos, una cochinilla hormiguera)") } };
}
