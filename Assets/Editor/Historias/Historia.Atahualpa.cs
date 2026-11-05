using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ATAHUALPA EN CAJAMARCA (docs/microcosmos-historias-3.md). Cajamarca, abril de 1533: Atahualpa, Sapa Inca preso, ha prometido llenar una sala
/// de oro y dos de plata a cambio de su libertad. La historia no va de la emboscada del 16 de noviembre (no se juega) sino del Estado inca
/// en marcha: quipus, chasquis, colcas, cumbi. El fin de Atahualpa se cuenta en la ficha, en una línea. Las frases son de juego; los hechos,
/// de los cronistas y de Hemming (1970).
/// </summary>
public static partial class StorySceneBuilder
{
    static int Atahualpa(Transform t)
    {
        Material stone = M("Piedra_de_Cajamarca", 0.62f, 0.56f, 0.46f), adobe = M("Adobe_inca", 0.62f, 0.5f, 0.38f), gold = M("Oro_del_rescate", 0.95f, 0.78f, 0.25f), silver = M("Plata_del_rescate", 0.8f, 0.82f, 0.85f);
        Material red = M("Cumbi_rojo", 0.72f, 0.14f, 0.12f), wool = M("Cuerdas_de_quipu", 0.85f, 0.78f, 0.6f), thatch = M("Ichu", 0.75f, 0.65f, 0.38f), water = M("Agua_termal", 0.45f, 0.62f, 0.68f), wood = M("Madera_de_qero", 0.45f, 0.3f, 0.2f);

        // 1 · LA SALA DEL RESCATE: muros de piedra hasta la cintura (abierta: sin techo) y la línea que Atahualpa marcó con la mano.
        Transform room = Spot(t, "Sala del rescate", new Vector3(0f, 0f, 16f), "La sala del rescate (la línea, a la altura del brazo de Atahualpa)", 4f);
        Prop(room, "Muro norte", new Vector3(0f, 0.5f, 3f), new Vector3(9f, 1f, 0.5f), stone);
        Prop(room, "Muro este", new Vector3(4.4f, 0.5f, 0f), new Vector3(0.5f, 1f, 6f), stone);
        Prop(room, "Muro oeste", new Vector3(-4.4f, 0.5f, 0f), new Vector3(0.5f, 1f, 6f), stone);
        Prop(room, "Línea roja del rescate", new Vector3(0f, 2.3f, 3.25f), new Vector3(9f, 0.08f, 0.04f), red, false);
        for (int k = 0; k < 6; k++) Prop(room, "Vajilla de oro", new Vector3(-3f + k * 1.2f, 0.25f, 1.6f), new Vector3(0.8f, 0.5f, 0.8f), gold, false);

        // 2 · LOS QUIPUS: la contabilidad del rescate.
        Transform quipu = Spot(t, "Mesa del quipucamayoc", new Vector3(-15f, 0f, 10f), "El quipu: cada cuerda, un pueblo; cada nudo, una cantidad", 2.8f);
        Prop(quipu, "Cuerda principal", new Vector3(0f, 1.6f, 0f), new Vector3(3f, 0.08f, 0.08f), wool, false);
        for (int k = 0; k < 9; k++) Prop(quipu, "Cuerda colgante", new Vector3(-1.4f + k * 0.35f, 1.1f, 0f), new Vector3(0.04f, 0.9f, 0.04f), k % 3 == 0 ? red : wool, false);
        Prop(quipu, "Estante del quipu", new Vector3(0f, 0.8f, 0.3f), new Vector3(3.2f, 1.6f, 0.2f), wood);

        // 3 · LOS TRES ENVÍOS que llegan por el Qhapaq Ñan.
        Transform road = Spot(t, "Tambo del camino", new Vector3(16f, 0f, 8f), "El tambo: aquí llegan las llamas del Qhapaq Ñan", 2.8f);
        Prop(road, "Techo de ichu", new Vector3(0f, 2.4f, 1f), new Vector3(5f, 0.3f, 3f), thatch, false);
        for (int k = 0; k < 2; k++) Prop(road, "Horcón", new Vector3(-2f + k * 4f, 1.2f, 1f), new Vector3(0.3f, 2.4f, 0.3f), wood);
        var loads = Items(t, gold, new Vector3(0.6f, 0.5f, 0.6f), ("Carga de plata de Pachacámac", new Vector3(21f, 0.25f, 3f)), ("Carga de oro de Cusco", new Vector3(19f, 0.25f, -3f)), ("Carga de oro de Jauja", new Vector3(14f, 0.25f, -8f)));

        // 4 · EL ESTADO QUE SIGUE: las colcas, el cumbi y el chasqui.
        Transform qollqa = Spot(t, "Colca", new Vector3(-17f, 0f, -3f), "La colca: maíz, chuño y charqui para los años malos", 2.8f);
        Prop(qollqa, "Colca circular de piedra", new Vector3(0f, 1.1f, 0f), new Vector3(2.4f, 2.2f, 2.4f), stone);
        Prop(qollqa, "Techo de ichu", new Vector3(0f, 2.4f, 0f), new Vector3(2.8f, 0.4f, 2.8f), thatch, false);
        Transform cumbi = Spot(t, "Telar de las acllas", new Vector3(-10f, 0f, -9f), "El telar de cumbi: la tela vale más que el oro", 2.4f);
        Prop(cumbi, "Telar", new Vector3(0f, 1f, 0f), new Vector3(2f, 2f, 0.2f), wood);
        Prop(cumbi, "Cumbi a medio tejer", new Vector3(0f, 0.9f, -0.14f), new Vector3(1.6f, 1.2f, 0.04f), red, false);
        Transform relay = Spot(t, "Posta del chasqui", new Vector3(12f, 0f, -11f), "La posta del chasqui: Cusco está a pocos días de carrera", 2.2f);
        Prop(relay, "Choza de la posta", new Vector3(0f, 0.9f, 1f), new Vector3(1.8f, 1.8f, 1.8f), adobe);
        var state = new List<Transform> { qollqa, cumbi, relay };

        // 5 · LOS BAÑOS.
        Transform baths = Spot(t, "Pozo de Pultumarca", new Vector3(14f, 0f, 16f), "El agua termal de Pultumarca", 2.6f);
        Prop(baths, "Pozo de piedra", new Vector3(0f, 0.2f, 0f), new Vector3(3f, 0.4f, 3f), stone);
        Prop(baths, "Agua caliente", new Vector3(0f, 0.41f, 0f), new Vector3(2.6f, 0.02f, 2.6f), water, false);

        // La gente: insectos-humanos. Atahualpa, una TERMITA (levanta una ciudad de barro sin rueda ni hierro, y todo se reparte desde el
        // centro), con su unku de cumbi y la mascapaicha; el contador, una ARAÑA (anuda sus hilos); el chasqui, un SALTAMONTES.
        Color redC = new Color(0.72f, 0.14f, 0.12f), ochre = new Color(0.75f, 0.55f, 0.25f), brown = new Color(0.42f, 0.32f, 0.24f), black = new Color(0.12f, 0.12f, 0.14f), cream = new Color(0.86f, 0.8f, 0.68f);
        GameObject inca = Ancient(t, "Atahualpa", new Vector3(3f, 0f, 9f), false, redC, false, 1.72f, "Termite");
        Prop(inca.transform, "Mascapaicha (borla roja)", new Vector3(0f, 1.72f, 0.1f), new Vector3(0.3f, 0.08f, 0.1f), red, false);
        GameObject accountant = Ancient(t, "Quipucamayoc (Atlas)", new Vector3(-13f, 0f, 7f), false, ochre, false, 1.62f, "Spider"); Routine(accountant, quipu, room);
        GameObject aclla = Ancient(t, "Aclla tejedora (Ruth)", new Vector3(-8f, 0f, -11f), true, cream, false, 1.55f, "SnowBee"); Routine(aclla, cumbi);
        GameObject chasqui = Ancient(t, "Chasqui (Momo)", new Vector3(14f, 0f, -8f), false, brown, false, 1.64f, "Grasshopper"); Routine(chasqui, relay, road);
        GameObject felipillo = Modern(t, "Felipillo", new Vector3(5f, 0f, 12f), false, cream, CubeInsect.Hat.None, 1.6f, "Cricket");
        GameObject soto = Modern(t, "Hernando de Soto", new Vector3(8f, 0f, 3f), false, black, CubeInsect.Hat.Brimmed, 1.72f, "Mantis");
        int n = 6;

        Mission(t, "Atahualpa en Cajamarca: el rescate (1533)", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("Habla con Atahualpa, preso en la sala del rescate", inca.transform, "Atahualpa", "Me tienen aquí, pero el Tawantinsuyu no se ha parado. Dije que llenaría esta sala de oro hasta donde llega mi mano, y dos de plata. Mira cómo se mueve un Estado sin una sola letra escrita."),
                Reach("Ve con el quipucamayoc a ver el quipu del rescate", quipu, "Quipucamayoc", "Cada cuerda es un pueblo; cada nudo, una cantidad. Lo que entra, lo que queda en las colcas, los que deben su mit'a: aquí está todo, y yo sé leerlo. Los extranjeros creen que son adornos."),
                Collect("Lleva tres cargas del tambo a la sala", loads, room, "Quipucamayoc", "Tres cargas más. Las traen hombres y llamas por caminos que cruzan montañas. Oro y plata no son dinero aquí: son para los templos y para vestir al Sol. Para los extranjeros son lo único."),
                Talk("Habla con Felipillo, el intérprete", felipillo.transform, "Felipillo", "Aprendí su lengua hace poco, y mal. De lo que yo diga depende lo que entienda cada uno. Muchos piensan que traduzco lo que me conviene. Es un puente muy estrecho para dos mundos."),
                Visit("Mira lo que sigue funcionando: la colca, el telar y la posta", state, "Aclla tejedora", "Este cumbi lleva meses de trabajo. Una tela así la da el Inca a quien quiere honrar: vale más que el oro. Y en la colca hay comida para años malos: en este país nadie debía pasar hambre."),
                Talk("Habla con Hernando de Soto", soto.transform, "Hernando de Soto", "Le enseñé el juego del ajedrez y aprende deprisa, dicen que lo juega bien. Y aún así hay quien pide su muerte. Yo no quiero estar aquí cuando lo decidan."),
                Hold("Junto al agua de Pultumarca, oye lo que cuenta el chasqui", baths, 8f, "Chasqui", "Vengo de relevar el mensaje: de posta en posta, un recado cruza el país en días. Antes de la guerra entre hermanos, Atahualpa se bañaba aquí, en estas aguas calientes."),
                Talk("Vuelve con Atahualpa", inca.transform, "Atahualpa", "He cumplido. La sala se llena. Mira la línea, cuenta los nudos. Yo he hablado como habla un Inca: lo que digo, se hace. Veremos si ellos saben hacer lo mismo."),
            },
            "Cajamarca, en el norte del Perú, en abril de 1533, sobre el Estudio Textil del santuario a escala de hormiga (pendiente de su piso: un almacén y contaduría). Atahualpa, Sapa Inca, está preso y ha prometido un rescate enorme. Recorre el Estado inca, que sigue funcionando sin escritura ni moneda. E para hablar y recoger; para escuchar, quédate quieto.",
            new[]
            {
                "Atahualpa (hacia 1500–1533) era hijo del Inca Huayna Cápac. A la muerte de su padre, hacia 1527, el Tawantinsuyu (el «país de las cuatro partes») se partió en una guerra entre él y su hermano Huáscar, que acababa de ganar cuando los españoles de Francisco Pizarro llegaron a Cajamarca en noviembre de 1532.",
                "El imperio inca iba del sur de Colombia al centro de Chile, con unos 40 000 km de caminos, postas de chasquis, almacenes (colcas) y turnos de trabajo (mit'a), sin moneda ni escritura alfabética: las cuentas se guardaban en quipus, cuerdas anudadas. La tela fina (cumbi), tejida por las acllas, era el regalo más alto.",
                "El 16 de noviembre de 1532, en la plaza de Cajamarca, los españoles capturaron a Atahualpa por sorpresa y mataron a miles de sus acompañantes. Preso, ofreció llenar de oro una sala hasta donde alcanzara su mano, y dos de plata. Durante meses llegaron las cargas: unos 6000 kg de oro y 12 000 de plata, según las cuentas españolas. Felipillo fue su intérprete, y los cronistas cuentan que el Inca aprendió ajedrez con sus captores.",
                "Pese al rescate, fue condenado y ejecutado el 26 de julio de 1533. Es la sala que hoy se visita en Cajamarca, aunque se discute si era esa la del rescate o su prisión (Cuarto del Rescate). Esta historia se limita a aquellos meses y a mostrar lo que era el Estado inca; lo que se dice aquí en su voz es de juego.",
            },
            new[]
            {
                "Hemming, J. (1970). The Conquest of the Incas. Macmillan.",
                "Xerez, F. de (1534). Verdadera relación de la conquista del Perú.",
                "Betanzos, J. de (1551). Suma y narración de los Incas.",
                "D'Altroy, T. (2014). The Incas. Wiley-Blackwell.",
                "Urton, G. (2003). Signs of the Inka Khipu. University of Texas Press.",
                "Julien, C. (1988). «How Inca decimal administration worked». Ethnohistory; y Boletín de Arqueología PUCP (2023), excavaciones junto al Cuarto del Rescate.",
            },
            "Atahualpa", "Yo cuento con nudos. Tú, mira qué queda cuando se acaba el oro.");
        return n;
    }
}

internal static partial class CivicBuilder
{
    static readonly Style IncaStone = new Style(C(0.62f, 0.56f, 0.46f), C(0.55f, 0.46f, 0.36f), C(0.75f, 0.65f, 0.38f), Roof.Gable, 3.2f);   // piedra labrada e ichu
    static readonly Style IncaAdobe = new Style(C(0.66f, 0.54f, 0.4f), C(0.55f, 0.45f, 0.34f), C(0.75f, 0.65f, 0.38f), Roof.Gable, 3f);    // adobe y ichu

    // CAJAMARCA (1533): la gran plaza triangular de Xerez, las kallankas y la sala del rescate junto a ella, el Qhapaq Ñan de norte a sur, el
    // río San Lucas y el cerro Santa Apolonia al suroeste; las aguas de Pultumarca (Baños del Inca) a 6 km al este, ACERCADAS. Plano aproximado
    // (Xerez 1534; Hyslop 1990, Inka Settlement Planning; OpenStreetMap, Cajamarca).
    const string PlanCajamarca = @"
calle Qhapaq Ñan al norte | 8 | 0 60, -120 1300
calle Qhapaq Ñan al sur | 8 | 0 -220, 160 -1300
calle Calle de las kallankas | 6 | -320 60, 320 60
calle Camino a Pultumarca | 6 | 60 0, 1000 180
calle Calle de los artesanos | 5 | -200 -200, 300 -240
calle Calle de las acllas | 5 | -360 -60, -360 340
rio Río San Lucas | 12 | -1300 -520, -300 -420, 500 -520, 1300 -440
colina Cerro Santa Apolonia | 220 110 | -340 -640
monte Cerros de Cajamarca | 800 800 | -1900 300 | seco
campo Maizales del valle | 460 220 | 720 -820
campo Chacras de papas | 300 200 | -760 480
campo Praderas del ganado del Inca (llamas) | 340 240 | 860 620
hito 0 | 40 120
hito 1 | -220 110
hito 2 | 210 110
hito 3 | 0 -60
hito 4 | -360 200
hito 5 | -230 -160
hito 6 | 430 -60
hito 7 | -480 -40
Plaza | 30 -10
Pileta | 140 -120
Parque | 560 300
Mercado | 330 -240
Estudio | -130 300
Investigacion | -300 -640
Piscina | 930 180
Estacion | -120 900
Entrenamiento | 180 -560 | actual: pampa de la Colina de Santa Apolonia; posición aproximada
Sanacion | -80 -300
Baile | 300 240
Taller | 120 -300
Carpinteria | 430 -300
Obra | -600 150
Hotel | 90 800
relleno | 46 | 0 -200";

    static int CajamarcaTown(Transform t)
    {
        var s = Town(t, "Cajamarca", new[]
        {
            new Lot("Sala del rescate", "Taller de papeles", "Sala del rescate (la línea de Atahualpa)", 16f, 12f, IncaStone,
                S("mesa", -3f, 2f, 2f, 1f, "Mesa de pesar"), S("estante", 3f, 2f, 1.6f, 0.6f, "Vajilla y planchas de oro"), S("armario", 0f, -3f, 1.2f, 0.6f, "Quipus del rescate")),
            new Lot("Kallanka del norte", "Restaurante", "Kallanka (gran sala de los banquetes del Estado)", 18f, 10f, IncaStone,
                S("fogon", -5f, 2f, 1.6f, 0.8f, "Ollas de la chicha y del locro"), S("mesa", 0f, 0f, 2.4f, 1f, "Estera con qeros"), S("barra", 5f, 2f, 2f, 0.8f, "Aríbalos de chicha")),
            new Lot("Kallanka del sur", "Hotel", "Kallanka (alojamiento de los que vienen por la mit'a)", 18f, 10f, IncaStone,
                S("cama", -5f, 2f, 1.2f, 2f, "Estera y manta"), S("cama", 0f, 2f, 1.2f, 2f, "Estera y manta"), S("cama", 5f, 2f, 1.2f, 2f, "Estera y manta")),
            new Lot("Aposento de Atahualpa", "Casa", "Aposento de Atahualpa", 14f, 12f, IncaStone,
                S("cama", -3f, 2f, 1.4f, 2f, "Lecho de Atahualpa"), S("estante", 3f, 2f, 1.2f, 0.6f, "Cumbi y mantas de vicuña"), S("mesa", 0f, -3f, 1.6f, 1f, "Tablero de ajedrez")),
            new Lot("Acllawasi de Cajamarca", "Taller", "Acllawasi (las acllas: cumbi y chicha)", 16f, 12f, IncaAdobe,
                S("telar", -4f, 2f, 2f, 1f, "Telar de cumbi"), S("telar", 0f, 2f, 2f, 1f, "Telar de cumbi"), S("telar", 4f, 2f, 2f, 1f, "Telar de cumbi"), S("estante", 0f, -3f, 1.2f, 0.6f, "Lana teñida")),
            new Lot("Colcas de Cajamarca", "Granero", "Colcas del Estado (maíz, chuño y charqui)", 16f, 12f, IncaStone,
                S("granero", -3f, 2f, 2f, 2f, "Colca de maíz"), S("granero", 3f, 2f, 2f, 2f, "Colca de chuño"), S("armario", 0f, -3f, 1.2f, 0.6f, "Quipu de la colca")),
            new Lot("Chacra del Sol", "Huerto", "Chacra del Sol y del Inca (maíz y papas)", 16f, 12f, IncaAdobe,
                S("bancal", -4f, 0f, 4f, 6f, "Maíz en terrazas"), S("bancal", 2f, 0f, 4f, 6f, "Papas y oca"), S("pozo", 6f, 3f, 1.2f, 1.2f, "Canal de riego")),
            new Lot("Templo del Sol", "Templo", "Templo del Sol de Cajamarca", 14f, 10f, IncaStone,
                S("altar", 0f, 2f, 2f, 1f, "Altar del Sol"), S("banco", 0f, -2f, 2f, 0.6f, "Banco de piedra")),
        }, new[]
        {
            new Lot("Casa de adobe de Cajamarca", "Casa", "Casa de adobe e ichu", 10f, 9f, IncaAdobe),
            new Lot("Taller de orfebres", "Taller", "Taller de orfebres", 10f, 8f, IncaAdobe, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco del orfebre"), S("horno", 0f, 0f, 1.4f, 1.4f, "Huayra (horno de viento)")),
            new Lot("Alfarería de Cajamarca", "Alfarería", "Alfarería (aríbalos y platos)", 10f, 8f, IncaAdobe, S("torno_alfarero", 0f, 0f, 1f, 1f, "Moldear la arcilla a mano")),
            new Lot("Chacra familiar", "Huerto", "Chacra del ayllu", 12f, 10f, IncaAdobe, S("bancal", 0f, 0f, 4f, 6f, "Papas y quinua"), S("pozo", 0f, 0f, 1f, 1f, "Canal")),
        }, C(0.5f, 0.42f, 0.34f));
        int n = 0;
        n += W(t, "Contadora de la sala", s[0], "sabio", s[1], null, s[2]);
        n += W(t, "Cocinera de la kallanka", s[1], "cocinero", s[1], null, s[2]);
        n += W(t, "Tejedora del acllawasi", s[4], "", s[1], null, s[2]);
        n += W(t, "Guardián de las colcas", s[5], "", s[1], null, null);
        n += W(t, "Campesino de la mit'a", s[6], "hortelano", s[1], null, null);
        n += W(t, "Sacerdotisa del Sol", s[7], "", s[1], null, null);
        n += Townsfolk(t, s, 8, "Vecina de Cajamarca", "Vecino de Cajamarca", s[1], s[7]);
        return n;
    }
}

public static partial class MicroBiome
{
    // Cajamarca (2750 m), abril: el final de las lluvias. Valle de maíz y papas, alisos en las acequias, chilcas, tunas y pajonal en los
    // cerros; hormigas, saltamontes, abejorros andinos; llamas y cuyes. (Weberbauer 1945, El mundo vegetal de los Andes peruanos; D'Altroy 2014.)
    static Biome CajamarcaBiome() => new Biome { title = "Valle de Cajamarca, Andes del norte (abril de 1533)", ground = C(0.5f, 0.4f, 0.3f), plants = new[] {
            new Plant(Kind.Grass, 12, "Ichu (Jarava ichu)", 70, C(0.72f, 0.65f, 0.4f), C(0.78f, 0.7f, 0.45f)),
            new Plant(Kind.Shrub, 6, "Aliso (Alnus acuminata), en las acequias", 1500, C(0.3f, 0.48f, 0.28f), C(0.45f, 0.38f, 0.3f)),
            new Plant(Kind.Shrub, 5, "Chilca (Baccharis latifolia)", 250, C(0.3f, 0.48f, 0.28f), C(0.85f, 0.85f, 0.7f)),
            new Plant(Kind.Shrub, 4, "Tuna (Opuntia ficus-indica)", 200, C(0.4f, 0.58f, 0.4f), C(0.8f, 0.3f, 0.3f)),
            new Plant(Kind.Flower, 4, "Lupino (Lupinus mutabilis), el tarwi", 120, C(0.35f, 0.5f, 0.3f), C(0.45f, 0.35f, 0.8f)),
            new Plant(Kind.Stone, 8, "Caliza de Santa Apolonia", 0, C(0.62f, 0.58f, 0.5f), Stone) },
          town = new[] {
            new Plant(Kind.Shrub, 4, "Lúcuma (Pouteria lucuma), del valle", 800, C(0.2f, 0.4f, 0.2f), C(0.85f, 0.6f, 0.15f)),
            new Plant(Kind.Grass, 12, "Maíz (Zea mays), de la chacra", 200, C(0.4f, 0.6f, 0.3f), C(0.9f, 0.8f, 0.3f)),
            new Plant(Kind.Grass, 10, "Papa (Solanum tuberosum), de la chacra", 60, C(0.35f, 0.55f, 0.3f), C(0.8f, 0.75f, 0.9f)) },
          bugs = new[] {
            new Bug("Ant", 5, 8, C(0.3f, 0.2f, 0.15f), "Hormiga andina (Camponotus)"),
            new Bug("SnowBee", 3, 22, C(0.2f, 0.18f, 0.15f), "Abejorro andino (Bombus)"),
            new Bug("Grasshopper", 3, 30, C(0.55f, 0.55f, 0.35f), "Saltamontes del ichu"),
            new Bug("Spider", 2, 12, C(0.4f, 0.3f, 0.25f), "Araña del pajonal") },
          townFauna = new[] {
            new Pet("Aphid", 4, 1.8f, C(0.85f, 0.78f, 0.65f), "Llama de carga (en el microcosmos, un pulgón de carga)"),
            new Pet("Woodlouse", 3, 0.4f, C(0.6f, 0.45f, 0.35f), "Cuy (en el microcosmos, una cochinilla pequeña)") } };
}
