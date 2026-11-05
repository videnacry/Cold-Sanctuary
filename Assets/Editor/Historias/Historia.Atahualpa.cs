using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CÓMO VIVÍAN LOS INCAS (docs/microcosmos-historias-3.md; Berón 2026-11-05: «lo que me parece más interesante es cómo funcionaban los incas, su estilo de
/// vida y si hay registros de obras que quedaron sin terminar, también la amistosa bienvenida que ofrecieron a los recién llegados»). Cajamarca, 15 de
/// noviembre de 1532, el día ANTES de la captura: el ayni entre vecinos, la mit'a, las colcas, los quipus, los caminos, la comida y la tela; un muro sin
/// terminar; y la llegada de los extranjeros: lo que un anciano recuerda de Tumbes (1528) y la chicha que Atahualpa ofrece a los enviados. Lo que pasó
/// después va en la ficha. Regla W10 del manual: revelar lo bello; lo recreado, marcado en la ficha. Las frases son de juego.
/// </summary>
public static partial class StorySceneBuilder
{
    static int Atahualpa(Transform t)
    {
        Material stone = M("Piedra_de_Cajamarca", 0.62f, 0.56f, 0.46f), adobe = M("Adobe_inca", 0.62f, 0.5f, 0.38f), gold = M("Oro_del_qero", 0.95f, 0.78f, 0.25f), earth = M("Tierra_de_la_chacra", 0.36f, 0.27f, 0.2f);
        Material red = M("Cumbi_rojo", 0.72f, 0.14f, 0.12f), wool = M("Cuerdas_de_quipu", 0.85f, 0.78f, 0.6f), thatch = M("Ichu", 0.75f, 0.65f, 0.38f), water = M("Agua_termal", 0.45f, 0.62f, 0.68f), wood = M("Madera_de_qero", 0.45f, 0.3f, 0.2f), maize = M("Maiz", 0.9f, 0.78f, 0.3f);

        // 1 · EL AYNI: lo que los vecinos hacen unos por otros: el techo de la viuda, la chacra del abuelo y el telar.
        Transform roof = Spot(t, "Casa de la viuda", new Vector3(-13f, 0f, 9f), "Ayni: hoy el ayllu le techa la casa a la viuda", 3f);
        for (int k = 0; k < 2; k++) Prop(roof, "Muro de adobe", new Vector3(-1.8f + k * 3.6f, 0.9f, 0f), new Vector3(0.4f, 1.8f, 3.4f), adobe);
        Prop(roof, "Medio techo de ichu", new Vector3(-0.9f, 2f, 0f), new Vector3(2f, 0.3f, 3.8f), thatch, false);
        Transform plot = Spot(t, "Chacra del abuelo", new Vector3(-18f, 0f, 1f), "Ayni: sembrar la chacra de quien ya no puede", 2.4f);
        Prop(plot, "Surcos", new Vector3(0f, 0.1f, 0f), new Vector3(3.6f, 0.2f, 2.6f), earth, false);
        Prop(plot, "Chaquitaclla (arado de pie)", new Vector3(2.2f, 0.7f, 0f), new Vector3(0.12f, 1.4f, 0.12f), wood);
        Transform loom = Spot(t, "Telar de cumbi", new Vector3(-10f, 0f, -9f), "El telar: la tela vale más que el oro", 2.4f);
        Prop(loom, "Telar", new Vector3(0f, 1f, 0f), new Vector3(2f, 2f, 0.2f), wood);
        Prop(loom, "Cumbi a medio tejer", new Vector3(0f, 0.9f, -0.14f), new Vector3(1.6f, 1.2f, 0.04f), red, false);
        Transform pot = Spot(t, "Olla de la minka", new Vector3(-6f, 0f, 2f), "La olla común: quien trabaja, come", 2f);
        Prop(pot, "Q'oncha (fogón de barro)", new Vector3(0f, 0.3f, 0f), new Vector3(1.2f, 0.6f, 0.8f), adobe);
        Prop(pot, "Olla de mote y charqui", new Vector3(0f, 0.75f, 0f), new Vector3(0.7f, 0.4f, 0.7f), maize, false);
        var ayni = new List<Transform> { roof, plot, loom, pot };

        // 2 · LA OBRA SIN TERMINAR: un muro a medias, sus piedras por llegar y la piedra cansada.
        Transform wall = Spot(t, "Muro sin terminar", new Vector3(0f, 0f, 16f), "Un muro sin terminar: la obra de la mit'a se paró con la guerra", 3.6f);
        for (int k = 0; k < 5; k++) Prop(wall, "Sillar labrado", new Vector3(-4f + k * 2f, 0.6f, 2f), new Vector3(1.9f, 1.2f, 1f), stone);
        for (int k = 0; k < 2; k++) Prop(wall, "Sillar de la segunda hilada", new Vector3(-3f + k * 2f, 1.7f, 2f), new Vector3(1.9f, 1f, 1f), stone);
        Prop(wall, "Rampa de tierra", new Vector3(5.6f, 0.5f, 2f), new Vector3(3f, 1f, 1.4f), earth, false);
        var blocks = Items(t, stone, new Vector3(0.7f, 0.5f, 0.6f), ("Sillar para el muro", new Vector3(7f, 0.25f, 8f)), ("Sillar para el muro", new Vector3(11f, 0.25f, 11f)), ("Sillar para el muro", new Vector3(5f, 0.25f, 11f)));
        Transform tired = Spot(t, "Piedra cansada", new Vector3(13f, 0f, 17f), "La piedra cansada: se quedó en el camino", 2.4f);
        Prop(tired, "Bloque abandonado", new Vector3(0f, 0.8f, 0f), new Vector3(3.2f, 1.6f, 2f), stone);

        // 3 · EL ESTADO: las colcas y los quipus; el camino, el tambo y el chasqui.
        Transform qollqa = Spot(t, "Colca", new Vector3(14f, 0f, 5f), "La colca: maíz, chuño y charqui para los años malos", 2.8f);
        Prop(qollqa, "Colca circular de piedra", new Vector3(0f, 1.1f, 0f), new Vector3(2.4f, 2.2f, 2.4f), stone);
        Prop(qollqa, "Techo de ichu", new Vector3(0f, 2.4f, 0f), new Vector3(2.8f, 0.4f, 2.8f), thatch, false);
        Prop(qollqa, "Quipu de la colca", new Vector3(-2.2f, 1.2f, 0f), new Vector3(0.1f, 1f, 1.8f), wool, false);
        Transform road = Spot(t, "Tambo del camino", new Vector3(13f, 0f, -9f), "El tambo del Qhapaq Ñan: comida y descanso para quien va de paso", 2.8f);
        Prop(road, "Techo de ichu", new Vector3(0f, 2.4f, 1f), new Vector3(5f, 0.3f, 3f), thatch, false);
        for (int k = 0; k < 2; k++) Prop(road, "Horcón", new Vector3(-2f + k * 4f, 1.2f, 1f), new Vector3(0.3f, 2.4f, 0.3f), wood);
        Prop(road, "Empedrado del camino", new Vector3(5f, 0.03f, 1f), new Vector3(6f, 0.06f, 2f), stone, false);

        // 4 · PULTUMARCA: el agua caliente y los vasos de la bienvenida.
        Transform baths = Spot(t, "Pozo de Pultumarca", new Vector3(19f, 0f, 14f), "Pultumarca: Atahualpa recibe a los enviados", 2.8f);
        Prop(baths, "Pozo de piedra", new Vector3(0f, 0.2f, 0f), new Vector3(3f, 0.4f, 3f), stone);
        Prop(baths, "Agua caliente", new Vector3(0f, 0.41f, 0f), new Vector3(2.6f, 0.02f, 2.6f), water, false);
        for (int k = 0; k < 2; k++) Prop(baths, "Vaso de chicha", new Vector3(-0.5f + k, 0.95f, -2.4f), new Vector3(0.2f, 0.3f, 0.2f), gold, false);
        Prop(baths, "Mesa baja", new Vector3(0f, 0.4f, -2.4f), new Vector3(2f, 0.8f, 0.7f), wood);

        // La gente: insectos-humanos. Atahualpa, una TERMITA; el contador, una ARAÑA (anuda sus hilos); la madre del ayllu, una HORMIGA (nada se hace solo).
        Color redC = new Color(0.72f, 0.14f, 0.12f), ochre = new Color(0.75f, 0.55f, 0.25f), brown = new Color(0.42f, 0.32f, 0.24f), black = new Color(0.12f, 0.12f, 0.14f), cream = new Color(0.86f, 0.8f, 0.68f);
        GameObject inca = Ancient(t, "Atahualpa", new Vector3(17f, 0f, 10f), false, redC, false, 1.72f, "Termite");
        Prop(inca.transform, "Mascapaicha (borla roja)", new Vector3(0f, 1.72f, 0.1f), new Vector3(0.3f, 0.08f, 0.1f), red, false);
        GameObject mother = Ancient(t, "Madre del ayllu (Ruth)", new Vector3(3f, 0f, 3f), true, brown, false, 1.55f, "Ant"); Routine(mother, roof, pot);
        GameObject mason = Ancient(t, "Maestro de la mit'a (Atlas)", new Vector3(8f, 0f, 15f), false, ochre, false, 1.66f, "DungBeetle"); Routine(mason, wall, tired);
        GameObject accountant = Ancient(t, "Quipucamayoc (Medea)", new Vector3(11f, 0f, 3f), true, ochre, false, 1.6f, "Spider"); Routine(accountant, qollqa);
        GameObject elder = Ancient(t, "Anciano de Tumbes (Momo)", new Vector3(10f, 0f, -7f), false, cream, false, 1.58f, "Grasshopper"); Routine(elder, road);
        GameObject felipillo = Modern(t, "Felipillo", new Vector3(16f, 0f, 16f), false, cream, CubeInsect.Hat.None, 1.6f, "Cricket");
        GameObject soto = Modern(t, "Hernando de Soto", new Vector3(21f, 0f, 16f), false, black, CubeInsect.Hat.Brimmed, 1.72f, "Mantis");
        int n = 7;

        Mission(t, "Atahualpa y los incas: cómo vivían, y cómo recibieron al extranjero (1532)", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("Habla con la madre del ayllu", mother.transform, "Madre del ayllu", "Aquí nadie tiene dinero y nadie lo echa de menos. Hoy le techamos la casa a la viuda; mañana ella hilará para nosotros. A eso lo llamamos ayni: hoy por ti, mañana por mí. Ven, que faltan manos."),
                Visit("Ayuda en el ayni: el techo, la chacra, el telar y la olla", ayni, "Madre del ayllu", "El abuelo ya no puede con la chaquitaclla: le sembramos nosotros, y él nos cuenta lo que vio de joven. Y al que trabaja se le da de comer: mote, charqui y chicha. Nadie se va con hambre de la minka."),
                Collect("La mit'a: lleva tres sillares al muro", blocks, wall, "Maestro de la mit'a", "Cada familia debe al Inca unos meses de trabajo por turnos: caminos, muros, chacras. No se paga con oro: se paga con tus manos. Y el Estado te da de comer, de beber y ropa mientras dura. Luego vuelves a casa."),
                Reach("Ve a la piedra cansada con el maestro", tired, "Maestro de la mit'a", "Este muro lo paró la guerra entre los hermanos. Y esa piedra no llegó nunca. Dicen que hay otras por el Cusco y por Ollantaytambo: la gente las llama cansadas, y cuentan que hasta lloraron. A mí me gustan: las cosas se empiezan aunque otro tenga que acabarlas."),
                Reach("Ve a la colca con la quipucamayoc", qollqa, "Quipucamayoc", "Cada cuerda, un pueblo; cada nudo, una cantidad. Aquí se guarda lo de los años buenos para los malos, para las viudas, los viejos y los huérfanos. El Tawantinsuyu no tiene letras y no pierde una mazorca."),
                Reach("Baja al tambo del camino y escucha al anciano de Tumbes", road, "Anciano de Tumbes", "Por este camino va todo, y el chasqui corre de posta en posta. Yo los vi llegar, hace cuatro años, en Tumbes: una casa que flotaba. Salimos en balsas con comida y agua, por pura curiosidad y alegría. Les dijimos que bajaran sin miedo. Entonces no sabíamos nada. Dimos lo que se da."),
                Hold("En Pultumarca, mira cómo reciben a los enviados", baths, 10f, "Felipillo", "Es Hernando de Soto, con quince jinetes, y yo traduzco como puedo. El Inca los ha hecho esperar y ahora les ofrece chicha en vasos de oro: aquí, a quien llega se le da de beber. Lo que cada uno piensa de verdad, eso no lo sé traducir."),
                Talk("Habla con Atahualpa", inca.transform, "Atahualpa", "Sé que han quitado cosas en mi tierra, y sé cuántos son. Pero a quien llega cansado se le da de beber: así somos. Mañana iré a verlos. El camino y las colcas seguirán aquí, y el muro lo acabará otro. Eso es lo que quería que vieras."),
            },
            "Cajamarca, en el norte del Perú, el 15 de noviembre de 1532, el día antes de la captura de Atahualpa, sobre el Estudio Textil del santuario a escala de hormiga (pendiente de su piso: un almacén y contaduría). Vive un día de un ayllu inca: el trabajo entre vecinos, la mit'a, las colcas, los quipus y los caminos; mira una obra que quedó sin terminar y conoce cómo fueron recibidos los extranjeros. E para hablar, recoger y ayudar; para mirar, quédate quieto.",
            new[]
            {
                "El Tawantinsuyu (el «país de las cuatro partes») iba del sur de Colombia al centro de Chile, sin moneda, sin mercado grande y sin escritura alfabética. La vida se organizaba por ayllus (grupos de parientes con su tierra). Los vecinos se ayudaban por ayni (ayuda devuelta) y minka (trabajo común con comida y chicha para quien lo hacía). Al Estado no se le pagaba con dinero sino con la mit'a, turnos de trabajo en sus tierras, caminos y obras, durante los que este daba de comer y de beber. Lo guardado en colcas alimentaba a soldados y trabajadores, y según los cronistas también a viudas y huérfanos; las cuentas se llevaban en quipus. Unos 40 000 km de caminos, con tambos y chasquis, unían el país; la tela fina (cumbi) era el regalo más alto. Era también un Estado duro: podía trasladar pueblos enteros, y los mitmaq no elegían.",
                "¿Quedaron obras sin terminar? Sí. La más documentada es el Templo del Sol de Ollantaytambo: quedó inconcluso y por el camino de la cantera de Kachiqhata hay bloques abandonados, las «piedras cansadas». No se sabe qué lo paró: la guerra entre Huáscar y Atahualpa, la conquista o la marcha de Manco Inca (Protzen 1993). El Inca Garcilaso (Comentarios Reales, VII, 29) cuenta de una piedra enorme para Sacsayhuamán que se «cansó» en el camino y no llegó. El muro y la piedra de esta escena son una recreación: no consta ninguno así en Cajamarca.",
                "¿Fue amistosa la bienvenida? En 1528 sí: Cieza de León, que habló con testigos, cuenta que en Tumbes la gente salió asombrada y alegre, llevó comida al barco, y el gobernador les ofreció bajar a por agua y lo que necesitaran «sin temor». En 1532 ya no era lo mismo: Tumbes estaba en ruinas por la guerra civil. Atahualpa envió mensajeros y regalos, habló de amistad y los dejó subir, pero también los hizo vigilar; y parece que quería capturarlos él. Recibió a Soto y a Hernando Pizarro con espera y frialdad, y les ofreció chicha (Titu Cusi; el gesto también podía querer decir que eran sus subordinados). Según Hernando Pizarro, le reclamó lo que habían quitado en su tierra. Y los españoles ya planeaban capturarlo. Hubo, pues, hospitalidad verdadera al principio, y diplomacia con desconfianza por las dos partes al final.",
                "El 16 de noviembre de 1532 los españoles lo capturaron en la plaza por sorpresa y mataron a miles de sus acompañantes. Preso, ofreció llenar de oro una sala hasta donde alcanzara su mano, y lo cumplió; aun así, lo ejecutaron el 26 de julio de 1533. Es recreación de juego lo que no está en las fuentes: el ayllu, la viuda, el anciano, y lo que dicen él y los demás.",
            },
            new[]
            {
                "Cieza de León, P. de (hacia 1554). Crónica del Perú, parte III (Tumbes, 1528), citado en PBS, Conquistadors: The Conquest of the Incas (pbs.org/conquistadors).",
                "Pizarro, H. (1533). Carta a la Audiencia de Santo Domingo (faculty.chass.ncsu.edu/slatta/hi216/documents/pizarro.htm).",
                "Titu Cusi Yupanqui (1570). Instrucción al licenciado Lope García de Castro; Xerez, F. de (1534), Verdadera relación de la conquista del Perú.",
                "Garcilaso de la Vega, Inca (1609). Comentarios Reales, libro VII, cap. 29.",
                "Protzen, J.-P. (1993). Inca Architecture and Construction at Ollantaytambo. Oxford University Press.",
                "D'Altroy, T. (2014). The Incas. Wiley-Blackwell; Murra, J. (1980). The Economic Organization of the Inka State. JAI Press.",
                "Hemming, J. (1970). The Conquest of the Incas. Macmillan; Urton, G. (2003). Signs of the Inka Khipu. University of Texas Press.",
            },
            "Atahualpa", "A quien llega cansado se le da de beber. Lo demás se verá mañana.");
        return n;
    }
}

internal static partial class CivicBuilder
{
    static readonly Style IncaStone = new Style(C(0.62f, 0.56f, 0.46f), C(0.55f, 0.46f, 0.36f), C(0.75f, 0.65f, 0.38f), Roof.Gable, 3.2f);   // piedra labrada e ichu
    static readonly Style IncaAdobe = new Style(C(0.66f, 0.54f, 0.4f), C(0.55f, 0.45f, 0.34f), C(0.75f, 0.65f, 0.38f), Roof.Gable, 3f);    // adobe y ichu

    // CAJAMARCA (1532): la gran plaza triangular de Xerez, las kallankas y la sala del rescate junto a ella, el Qhapaq Ñan de norte a sur, el
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
            new Lot("Casa de los quipus", "Taller de papeles", "Casa de los quipucamayocs (la sala de piedra que luego fue la del rescate)", 16f, 12f, IncaStone,
                S("mesa", -3f, 2f, 2f, 1f, "Mesa de cuentas con yupana"), S("estante", 3f, 2f, 1.6f, 0.6f, "Quipus de los pueblos"), S("armario", 0f, -3f, 1.2f, 0.6f, "Cuentas de la mit'a")),
            new Lot("Kallanka del norte", "Restaurante", "Kallanka (gran sala de los banquetes del Estado)", 18f, 10f, IncaStone,
                S("fogon", -5f, 2f, 1.6f, 0.8f, "Ollas de la chicha y del locro"), S("mesa", 0f, 0f, 2.4f, 1f, "Estera con qeros"), S("barra", 5f, 2f, 2f, 0.8f, "Aríbalos de chicha")),
            new Lot("Kallanka del sur", "Hotel", "Kallanka (alojamiento de los que vienen por la mit'a)", 18f, 10f, IncaStone,
                S("cama", -5f, 2f, 1.2f, 2f, "Estera y manta"), S("cama", 0f, 2f, 1.2f, 2f, "Estera y manta"), S("cama", 5f, 2f, 1.2f, 2f, "Estera y manta")),
            new Lot("Aposento de Atahualpa", "Casa", "Aposento de Atahualpa", 14f, 12f, IncaStone,
                S("cama", -3f, 2f, 1.4f, 2f, "Lecho de Atahualpa"), S("estante", 3f, 2f, 1.2f, 0.6f, "Cumbi y mantas de vicuña"), S("mesa", 0f, -3f, 1.6f, 1f, "Mesa baja con qeros")),
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
            new Lot("Casa de adobe de Cajamarca", "Casa", "Casa del ayllu (adobe e ichu)", 10f, 9f, IncaAdobe),
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
    // Cajamarca (2750 m), noviembre: empiezan las lluvias y la siembra. Valle de maíz y papas, alisos en las acequias, chilcas, tunas y pajonal en los
    // cerros; hormigas, saltamontes, abejorros andinos; llamas y cuyes. (Weberbauer 1945, El mundo vegetal de los Andes peruanos; D'Altroy 2014.)
    static Biome CajamarcaBiome() => new Biome { title = "Valle de Cajamarca, Andes del norte (noviembre de 1532)", ground = C(0.5f, 0.4f, 0.3f), plants = new[] {
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
