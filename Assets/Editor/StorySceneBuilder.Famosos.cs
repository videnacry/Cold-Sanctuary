using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// MICROCOSMOS DE LOS FAMOSOS (docs/microcosmos-famosos.md; Berón 2026-11-04: «los personajes famosos, elon musk, sadhguru, y demás
/// sácalos del mesocosmos y crea sus microcosmos respectivos»). Cada figura vive UNA historia de su vida pública, bien documentada,
/// respetuosa e inspiradora, en el PUEBLO REAL donde ocurrió (CivicBuilder.Famosos: su plano, su estilo de época y lugar), sobre el
/// piso del santuario que mejor la cuenta (MicroFloors). Como los científicos: insectos-humanos (su insecto por etología, su ropa de
/// referencia por los colores de su versión humana), sitios de la misión en el claro y la HistoryCard con fuentes al terminar.
/// Las frases son de juego, en la voz de cada uno; los hechos y las fechas, de las fuentes del doc.
/// </summary>
public static partial class StorySceneBuilder
{
    internal static readonly string[] FamousScenes =
    {
        "Microcosmos_Sadhguru", "Microcosmos_ElonMusk", "Microcosmos_Goku", "Microcosmos_BillGates", "Microcosmos_WarrenBuffett", "Microcosmos_MacKenzieScott",
    };

    /// <summary>Genera SOLO los microcosmos de los famosos, uno tras otro (con la escena principal abierta). Con poca memoria, mejor
    /// uno a uno con su propio menú.</summary>
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de famosos (todos, uno tras otro)")]
    public static void BuildFamous() { foreach (string sc in FamousScenes) { try { BuildOne(sc); } catch (System.Exception e) { Debug.LogWarning($"[Historias] {sc}: {e.Message}"); } } }

    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de famosos/Sadhguru (Velliangiri, 1999)")]
    public static void BuildSadhguru() => BuildOne("Microcosmos_Sadhguru");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de famosos/Elon Musk (Hawthorne, 2008)")]
    public static void BuildElonMusk() => BuildOne("Microcosmos_ElonMusk");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de famosos/Goku (aldea de la montaña, homenaje)")]
    public static void BuildGoku() => BuildOne("Microcosmos_Goku");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de famosos/Bill Gates (Albuquerque, 1975)")]
    public static void BuildBillGates() => BuildOne("Microcosmos_BillGates");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de famosos/Warren Buffett (Omaha, 1983)")]
    public static void BuildWarrenBuffett() => BuildOne("Microcosmos_WarrenBuffett");
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de famosos/MacKenzie Scott (Prairie View, 2020)")]
    public static void BuildMacKenzieScott() => BuildOne("Microcosmos_MacKenzieScott");

    // ══ SADHGURU · Isha Yoga Center, al pie de los montes Velliangiri (Coimbatore), 24 de junio de 1999 · Sala de Yoga ══════════════

    static int Sadhguru(Transform t)
    {
        Material granite = M("Granito_negro", 0.16f, 0.16f, 0.17f), brick = M("Ladrillo_de_barro", 0.66f, 0.42f, 0.3f), mud = M("Mortero_de_barro", 0.55f, 0.42f, 0.3f);
        Material stone = M("Piedra_clara", 0.85f, 0.82f, 0.74f), copper = M("Cobre", 0.75f, 0.45f, 0.25f), water = M("Agua_Noyyal", 0.3f, 0.5f, 0.6f), cloth = M("Tela_blanca", 0.95f, 0.94f, 0.9f);

        // 1 · EL DHYANALINGA: el linga de granito bajo su cúpula de ladrillo y barro (sin acero ni hormigón). La cúpula es la historia:
        //     se queda abierta (sin muros: regla 2), solo su anillo de arranque y los arcos.
        Transform linga = Spot(t, "Dhyanalinga", new Vector3(0f, 0f, 15f), "El Dhyanalinga (consagrado el 24 de junio de 1999)", 4.5f);
        Prop(linga, "Avudaiyar (base de granito)", new Vector3(0f, 0.3f, 0f), new Vector3(2.6f, 0.6f, 2.6f), granite);
        Prop(linga, "Linga de granito", new Vector3(0f, 2f, 0f), new Vector3(1.1f, 2.8f, 1.1f), granite);
        Prop(linga, "Anillo de cobre", new Vector3(0f, 1.2f, 0f), new Vector3(1.2f, 0.12f, 1.2f), copper, false);
        for (int k = 0; k < 8; k++)
        {
            float a = k * Mathf.PI / 4f;
            Prop(linga, "Arranque de la cúpula (ladrillo y barro)", new Vector3(Mathf.Cos(a) * 7f, 0.4f, Mathf.Sin(a) * 7f), new Vector3(1.6f, 0.8f, 0.6f), brick);
        }
        for (int k = 0; k < 6; k++) Prop(linga, "Cueva de meditación (nicho)", new Vector3(-5f + k * 2f, 0.15f, -5.5f), new Vector3(1.2f, 0.3f, 1.2f), stone, false);

        // 2 · EL SARVA DHARMA STAMBHA: la columna de la entrada, con los símbolos de muchas religiones (el templo es para todos).
        Transform pillar = Spot(t, "Sarva Dharma Stambha", new Vector3(-12f, 0f, 2f), "Sarva Dharma Stambha (la columna de todas las religiones)", 6f);
        Prop(pillar, "Columna de granito", new Vector3(0f, 2.5f, 0f), new Vector3(0.8f, 5f, 0.8f), stone);
        for (int k = 0; k < 4; k++) Prop(pillar, "Símbolo tallado", new Vector3(0f, 1f + k * 1f, -0.42f), new Vector3(0.45f, 0.45f, 0.04f), granite, false);

        // 3 · LA OBRA: ladrillos de barro cocido para terminar el anillo de la cúpula (los voluntarios).
        var bricks = Items(t, brick, new Vector3(0.5f, 0.25f, 0.25f), ("Ladrillo de barro", new Vector3(10f, 0.12f, -8f)), ("Ladrillo de barro", new Vector3(13f, 0.12f, -6f)), ("Ladrillo de barro", new Vector3(16f, 0.12f, -9f)));
        Prop(t, "Montón de mortero de barro", new Vector3(13f, 0.2f, -10f), new Vector3(1.6f, 0.4f, 1.2f), mud, false);

        // 4 · EL ASHRAM: la cocina (comida para todos, sentados en el suelo), el huerto y el arroyo.
        Transform kitchen = Spot(t, "Cocina del ashram", new Vector3(14f, 0f, 6f), "La cocina del ashram (comer en silencio, en el suelo)", 3f);
        Prop(kitchen, "Fogón de gas con olla de acero", new Vector3(0f, 0.45f, 0f), new Vector3(1.6f, 0.9f, 0.8f), M("Acero", 0.7f, 0.72f, 0.75f));
        Prop(kitchen, "Hojas de plátano (platos)", new Vector3(0f, 0.02f, -2f), new Vector3(2.4f, 0.04f, 0.6f), M("Hoja_platano", 0.3f, 0.55f, 0.25f), false);
        Transform garden = Spot(t, "Huerto del ashram", new Vector3(17f, 0f, 16f), "El huerto (cocos, plátanos y verduras)", 2.5f);
        Prop(garden, "Bancal", new Vector3(0f, 0.15f, 0f), new Vector3(4f, 0.3f, 2.2f), M("Tierra_roja", 0.55f, 0.32f, 0.22f));
        Transform stream = Spot(t, "Arroyo de la Noyyal", new Vector3(-17f, 0f, 14f), "El arroyo que baja de Velliangiri (la Noyyal)", 2.5f);
        Prop(stream, "Agua del arroyo", new Vector3(0f, 0.02f, 0f), new Vector3(3f, 0.04f, 14f), water, false);
        var chores = new List<Transform> { kitchen, garden, stream };

        // 5 · EL MIRADOR: los siete montes de Velliangiri, el «Kailash del sur».
        Transform view = Spot(t, "Mirador de Velliangiri", new Vector3(-16f, 0f, -6f), "Mirador: los siete montes de Velliangiri", 3f);
        Prop(view, "Piedra para sentarse", new Vector3(0f, 0.25f, 0f), new Vector3(1.6f, 0.5f, 1f), stone);
        Prop(view, "Nandi (el toro que espera, de cara al linga)", new Vector3(3f, 0.6f, 4f), new Vector3(0.9f, 1.2f, 1.8f), granite);

        // La gente: insectos-humanos. Sadhguru, una CIGARRA (años quieta bajo tierra y una muda: la transformación interior; su canto),
        // con los colores de su versión humana: túnica y turbante blancos, barba.
        Color white = new Color(0.95f, 0.94f, 0.9f), ochre = new Color(0.85f, 0.6f, 0.3f), sage = new Color(0.5f, 0.72f, 0.62f);
        GameObject sg = Ancient(t, "Sadhguru (Jaggi Vasudev)", new Vector3(2.5f, 0f, 8f), false, white, beard: true, h: 1.72f, species: "Cicada");
        CubeInsect.WearHat(sg, CubeInsect.Hat.Turban);
        GameObject vol = Ancient(t, "Voluntaria del ashram (Sakshi)", new Vector3(-10f, 0f, 0f), true, sage);
        GameObject mason = Ancient(t, "Albañil voluntario (Atlas)", new Vector3(11f, 0f, -11f), false, ochre); Routine(mason, linga);
        GameObject cook = Ancient(t, "Cocinera del ashram (Ruth)", new Vector3(14f, 0f, 3f), true, white); Routine(cook, kitchen, garden);
        GameObject villager = Ancient(t, "Vecino de Ikkarai Boluvampatti (Momo)", new Vector3(-14f, 0f, -9f), false, new Color(0.6f, 0.5f, 0.38f)); Routine(villager, view, stream);
        int n = 5;

        Mission(t, "Sadhguru en Velliangiri: el Dhyanalinga (1999)", StoryMission.Marga.Yoga, 60f,
            new[]
            {
                Talk("Habla con Sadhguru junto al Dhyanalinga", sg.transform, "Sadhguru", "Llevamos tres años preparando este espacio. No es para rezar ni para pedir nada: es para sentarse en silencio. Quien entre, de la fe que sea o de ninguna, puede quedarse quieto aquí."),
                Reach("Ve a la columna de la entrada, el Sarva Dharma Stambha", pillar, "Voluntaria del ashram", "Lleva los símbolos de muchas religiones. Es la puerta: aquí nadie pregunta de dónde vienes."),
                Collect("La obra: lleva tres ladrillos de barro al anillo de la cúpula", bricks, linga, "Albañil voluntario", "Ladrillo y mortero de barro, sin acero ni hormigón. La cúpula se sostiene sola por su forma."),
                Visit("Seva: ayuda en la cocina, en el huerto y en el arroyo", chores, "Cocinera del ashram", "Aquí el trabajo también es práctica: lo que haces con atención te cambia tanto como lo que haces sentado."),
                Hold("Siéntate bajo la cúpula, en silencio", linga, 12f, "Sadhguru", "No hagas nada. Ni siquiera intentes meditar. Solo quédate."),
                Talk("Habla con el vecino de Ikkarai Boluvampatti", villager.transform, "Vecino", "Antes aquí solo había matorral y elefantes que bajaban del monte. Ahora vienen de muy lejos; los del pueblo traemos verdura y nos quedamos a comer."),
                Hold("En el mirador, mira los siete montes de Velliangiri", view, 8f, "Voluntaria del ashram", "Los peregrinos suben los siete montes descalzos. Dicen que es el Kailash del sur."),
                Talk("Vuelve con Sadhguru", sg.transform, "Sadhguru", "El templo ya está hecho. Ahora lo importante es lo que cada uno haga consigo mismo."),
            },
            "El Isha Yoga Center, al pie de los montes Velliangiri, a unos 40 km de Coimbatore (Tamil Nadu, India), el 24 de junio de 1999, sobre la Sala de Yoga del santuario a escala de hormiga. Ese día termina la consagración del Dhyanalinga, un espacio de meditación abierto a todos. Recórrelo con Sadhguru y los voluntarios del ashram. E para hablar, recoger y ayudar; para meditar, quédate quieto.",
            new[]
            {
                "Jaggi Vasudev, conocido como Sadhguru, nació en Mysuru (Karnataka) en 1957. Cuenta que en 1982, sentado en una roca de la colina de Chamundi, tuvo una experiencia que cambió su vida; desde entonces enseña yoga y meditación. En 1992 fundó la Fundación Isha.",
                "En 1994 la fundación compró tierra al pie de los montes Velliangiri, en los Ghats occidentales, cerca de la aldea de Ikkarai Boluvampatti, y allí se levantó el Isha Yoga Center. Los Velliangiri son un lugar de peregrinación (la subida de los siete montes) y en ellos nace el río Noyyal.",
                "El Dhyanalinga es un linga de granito bajo una cúpula de ladrillo y mortero de barro, construida sin acero ni hormigón. Según la fundación, su consagración (prāṇa pratiṣṭhā) duró tres años y terminó el 24 de junio de 1999 (algunas fuentes dan el 23); se abrió al público el 23 de noviembre de 1999. No es un templo de culto: no hay ritos ni ofrendas, solo silencio, y está abierto a personas de cualquier fe o de ninguna; en la entrada, el Sarva Dharma Stambha muestra símbolos de varias religiones.",
                "Las dimensiones exactas de la cúpula y del linga, y lo que ocurre durante una consagración, se dan aquí como los cuenta la fundación. Isha y su fundador también han recibido críticas (por ejemplo, sobre permisos de construcción en la falda del monte); esta historia se limita a 1999.",
            },
            new[]
            {
                "Isha Foundation, «Dhyanalinga – His Only Obsession» (isha.sadhguru.org/en/blog/article/dhyanalinga-his-only-obsession).",
                "Isha Foundation, «Dhyanalinga Consecration Anniversary» (isha.sadhguru.org/events/annual-events/dhyanalinga-consecration-anniversary).",
                "Covai Post (24-6-2019), «20th anniversary of Isha Dhyanalinga celebrated» (covaipost.com).",
                "Sadhguru (2016). Inner Engineering: A Yogi's Guide to Joy. Spiegel & Grau (la colina de Chamundi, 1982).",
            },
            "Sadhguru", "Lo que buscas no está en este lugar. Este lugar solo te ayuda a dejar de buscar fuera.");
        return n;
    }

    // ══ ELON MUSK · la fábrica de SpaceX en Hawthorne (California), 28 de septiembre de 2008 · Mecánica ═══════════════════════════
    //    El cuarto lanzamiento del Falcon 1 (desde Omelek, atolón de Kwajalein) se vivió en la fábrica: el equipo de Hawthorne lo
    //    siguió en las pantallas y Musk les habló al llegar a órbita.

    static int ElonMusk(Transform t)
    {
        Material white = M("Fuselaje_blanco", 0.94f, 0.94f, 0.95f), steel = M("Acero", 0.7f, 0.72f, 0.75f), dark = M("Pantalla", 0.08f, 0.1f, 0.14f);
        Material copper = M("Cobre_tobera", 0.72f, 0.42f, 0.25f), blue = M("Gráfica_azul", 0.25f, 0.55f, 0.85f), epoxy = M("Suelo_epoxi", 0.82f, 0.83f, 0.85f);

        // 1 · LA FÁBRICA: la primera etapa del Falcon 1 tumbada en su cuna (21 m de cohete entero; aquí la etapa) y un Merlin 1C.
        Transform floor = Spot(t, "Nave de montaje", new Vector3(-8f, 0f, 14f), "La nave de montaje: el Falcon 1 del vuelo 4", 3.5f);
        Prop(floor, "Suelo de epoxi", new Vector3(0f, 0.02f, 0f), new Vector3(16f, 0.04f, 8f), epoxy, false);
        Prop(floor, "Primera etapa del Falcon 1 (en su cuna)", new Vector3(0f, 1.2f, 0f), new Vector3(12f, 1.7f, 1.7f), white);
        for (int k = 0; k < 3; k++) Prop(floor, "Cuna", new Vector3(-5f + k * 5f, 0.35f, 0f), new Vector3(0.6f, 0.7f, 2.2f), steel);
        Prop(floor, "Tobera del Merlin 1C", new Vector3(6.6f, 1.2f, 0f), new Vector3(1.2f, 1.2f, 1.2f), copper, false);

        // 2 · LA CONSOLA DEL VUELO 3 (2 de agosto de 2008): la gráfica del empuje que quedó tras el apagado.
        Transform data = Spot(t, "Consola de datos del vuelo 3", new Vector3(12f, 0f, 12f), "Los datos del vuelo 3 (2 de agosto de 2008)", 3f);
        Prop(data, "Mesa", new Vector3(0f, 0.4f, 0f), new Vector3(2.4f, 0.8f, 1f), steel);
        Prop(data, "Pantalla", new Vector3(0f, 1.3f, 0.4f), new Vector3(2f, 1f, 0.08f), dark, false);
        Prop(data, "Curva del empuje residual", new Vector3(0f, 1.3f, 0.35f), new Vector3(1.6f, 0.06f, 0.02f), blue, false);

        // 3 · LAS TRES COSAS QUE MIRAR antes del vuelo 4.
        var fixes = Items(t, steel, new Vector3(0.7f, 0.9f, 0.7f),
            ("Motor Merlin 1C (empuje residual tras el apagado)", new Vector3(14f, 0.45f, -2f)),
            ("Interetapa (la separación)", new Vector3(17f, 0.45f, 3f)),
            ("Secuencia de vuelo (la espera antes de separar)", new Vector3(11f, 0.45f, 3f)));

        // 4 · EL CONTROL DE MISIÓN: las pantallas con la imagen de Omelek (Kwajalein).
        Transform control = Spot(t, "Control de misión", new Vector3(-14f, 0f, -4f), "Control de misión: la señal de Omelek", 3.5f);
        for (int k = 0; k < 3; k++) Prop(control, "Pantalla grande", new Vector3(-3f + k * 3f, 2.2f, 2f), new Vector3(2.6f, 1.6f, 0.1f), dark, false);
        Prop(control, "Fila de consolas", new Vector3(0f, 0.4f, -1f), new Vector3(8f, 0.8f, 1f), steel);

        // La gente: insectos-humanos. Musk, un SALTAMONTES (el salto es un lanzamiento: guarda energía en las patas y la suelta de
        // golpe), con la camiseta negra de su versión humana.
        Color black = new Color(0.08f, 0.08f, 0.09f), polo = new Color(0.2f, 0.25f, 0.35f);
        GameObject musk = Who(t, "Elon Musk", new Vector3(-4f, 0f, 9f), false, 1.88f, "Grasshopper"); CubeInsect.WearShirt(musk, black);
        GameObject prop = Who(t, "Ingeniera de propulsión (Medea)", new Vector3(10f, 0f, 9f), true); CubeInsect.WearShirt(prop, polo);
        GameObject launch = Who(t, "Director de lanzamiento (Héspero)", new Vector3(-12f, 0f, -8f), false); CubeInsect.WearShirt(launch, polo);
        GameObject tech = Who(t, "Técnico de la nave (Atlas)", new Vector3(-12f, 0f, 12f), false); CubeInsect.WearShirt(tech, new Color(0.3f, 0.32f, 0.36f)); Routine(tech, floor, data);
        GameObject welder = Who(t, "Soldadora de tanques (Ruth)", new Vector3(-2f, 0f, 18f), true); CubeInsect.WearShirt(welder, new Color(0.45f, 0.4f, 0.32f)); Routine(welder, floor);
        int n = 5;

        Mission(t, "Elon Musk y el Falcon 1: a la cuarta, órbita (2008)", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("Agosto de 2008: habla con Elon Musk junto al cohete", musk.transform, "Elon Musk", "Tres lanzamientos, tres fallos. Pero tenemos piezas para un cuarto cohete y un equipo que no se ha rendido. Vamos a entender qué pasó y a intentarlo otra vez."),
                Hold("Mira con el equipo los datos del vuelo 3", data, 10f, "Ingeniera de propulsión", "Al apagarse, el motor nuevo siguió empujando un poco más de lo previsto, y la primera etapa alcanzó a la segunda después de separarse. El fallo no está en una pieza rota: está en el tiempo."),
                Visit("Revisa las tres cosas: el motor, la interetapa y la secuencia", fixes, "Ingeniera de propulsión", "Basta con esperar más entre el apagado y la separación. Un cambio pequeño en la secuencia."),
                Talk("Habla con el técnico de la nave", tech.transform, "Técnico", "Siete semanas entre el vuelo 3 y el 4. Hemos dormido poco, pero el cohete ya está en la isla."),
                Reach("28 de septiembre, 16:15: ve al control de misión", control, "Director de lanzamiento", "Omelek, en el atolón de Kwajalein, en medio del Pacífico. Desde aquí lo vemos todo por las pantallas."),
                Hold("La cuenta atrás y el ascenso: quédate mirando las pantallas", control, 12f, "Director de lanzamiento", "Despegue… separación de etapas limpia… encendido de la segunda etapa… apagado. ¡Estamos en órbita!"),
                Talk("Habla con la soldadora de tanques", welder.transform, "Soldadora", "Hay gente llorando por toda la nave. Yo soldé ese tanque."),
                Talk("Musk habla a todo el equipo", musk.transform, "Elon Musk", "Mucha gente pensaba que no podíamos hacerlo. Lo habéis hecho vosotros, todos. A la cuarta va la vencida."),
            },
            "La fábrica de SpaceX en Hawthorne (California), entre agosto y el 28 de septiembre de 2008, sobre el taller de Mecánica del santuario a escala de hormiga (pendiente de su piso: una fábrica de cohetes). Tras tres fallos, la empresa tiene un último Falcon 1. Ayuda al equipo a entender el fallo del vuelo 3 y vive con ellos el cuarto lanzamiento. E para hablar y revisar; para mirar, quédate quieto.",
            new[]
            {
                "Elon Musk (Pretoria, 1971) fundó SpaceX en 2002 para abaratar el acceso al espacio. Su primer cohete, el Falcon 1, era pequeño (unos 21 m) y usaba un motor propio, el Merlin. Se lanzaba desde la isla de Omelek, en el atolón de Kwajalein (Islas Marshall).",
                "Los tres primeros vuelos fallaron: en 2006 un incendio por una fuga apagó el motor poco después del despegue; en 2007 la segunda etapa se apagó antes de tiempo; y el 2 de agosto de 2008 el empuje residual del motor tras el apagado hizo que la primera etapa chocara con la segunda después de separarse. La solución fue alargar la espera entre el apagado y la separación.",
                "El 28 de septiembre de 2008 (23:15 UTC; las 16:15 en California), siete semanas después, el cuarto Falcon 1 llegó a órbita con una carga de prueba de 165 kg (RatSat): fue el primer cohete de combustible líquido desarrollado con dinero privado en alcanzar la órbita terrestre. En la sede de Hawthorne, unos 500 empleados lo siguieron por las pantallas; Musk les dio las gracias y dijo que era «uno de los mejores días de mi vida».",
                "En diciembre de 2008 la NASA contrató a SpaceX para llevar carga a la Estación Espacial. Musk es también una figura pública muy discutida por otros asuntos; esta historia se limita a 2008 y al trabajo de su equipo.",
            },
            new[]
            {
                "Space.com (29-9-2008), «SpaceX Successfully Launches Falcon 1 Rocket Into Orbit».",
                "Spaceflight Now, «Falcon 1 flight 4» (spaceflightnow.com/falcon/004).",
                "Aviation Week (2008), «Falcon 1 Makes Orbit On Fourth Attempt».",
                "Koenigsmann, H. et al. (2009). «Falcon 1 Launch Vehicle: Demo Flights, Status, Manifest and Upgrade Path». SmallSat Conference (digitalcommons.usu.edu/smallsat/2009).",
            },
            "Elon Musk", "Que falle no es lo grave. Lo grave sería dejar de intentarlo.");
        return n;
    }

    // ══ GOKU · una aldea de montaña (HOMENAJE ORIGINAL, ficción) · Dojo ═══════════════════════════════════════════════════════════
    //    Goku es un personaje de ficción (Dragon Ball, Akira Toriyama). Esto NO copia ninguna escena, diálogo, lugar ni técnica de la
    //    obra: es una historia nueva de entrenamiento en una aldea de montaña, al modo de las escuelas de artes marciales de los montes
    //    de China (Song, Wudang), donde el entrenamiento empieza por las tareas del pueblo.

    static int Goku(Transform t)
    {
        Material wood = M("Madera_de_pino", 0.6f, 0.45f, 0.3f), stone = M("Piedra_del_monte", 0.6f, 0.6f, 0.58f), water = M("Agua_del_torrente", 0.35f, 0.6f, 0.7f);
        Material bucket = M("Cubo_de_madera", 0.55f, 0.4f, 0.25f), mud = M("Barro_del_bancal", 0.45f, 0.36f, 0.25f), log = M("Leña", 0.5f, 0.35f, 0.22f);

        // 1 · EL PATIO: postes de madera para golpear (hoy no se usan) y el aljibe de piedra.
        Transform yard = Spot(t, "Patio de entrenamiento", new Vector3(0f, 0f, 14f), "El patio de entrenamiento de la aldea", 3f);
        for (int k = 0; k < 3; k++) Prop(yard, "Poste de madera", new Vector3(-4f + k * 4f, 0.9f, 3f), new Vector3(0.35f, 1.8f, 0.35f), wood);
        Transform cistern = Spot(t, "Aljibe de la aldea", new Vector3(6f, 0f, 8f), "El aljibe de la aldea (vacío al amanecer)", 2.4f);
        Prop(cistern, "Aljibe de piedra", new Vector3(0f, 0.45f, 0f), new Vector3(1.8f, 0.9f, 1.8f), stone);

        // 2 · EL TORRENTE: tres cubos para subir el agua.
        Transform river = Spot(t, "Torrente", new Vector3(-17f, 0f, 6f), "El torrente que baja de la cascada", 2.4f);
        Prop(river, "Agua del torrente", new Vector3(0f, 0.02f, 0f), new Vector3(3f, 0.04f, 18f), water, false);
        var buckets = Items(t, bucket, new Vector3(0.45f, 0.5f, 0.45f), ("Cubo de agua", new Vector3(-15f, 0.25f, 0f)), ("Cubo de agua", new Vector3(-15f, 0.25f, 3f)), ("Cubo de agua", new Vector3(-15f, 0.25f, 6f)));

        // 3 · LAS TAREAS: la leñera, el bancal de arroz y la cerca del huerto.
        var chores = Items(t, log, new Vector3(1.2f, 0.6f, 0.8f),
            ("Leña sin apilar", new Vector3(14f, 0.3f, 2f)), ("Bancal sin escardar", new Vector3(16f, 0.3f, 10f)), ("Cerca caída del huerto", new Vector3(12f, 0.3f, 16f)));
        Prop(t, "Bancal de arroz", new Vector3(17f, 0.05f, 13f), new Vector3(5f, 0.1f, 4f), mud, false);

        // 4 · LA CASCADA y 5 · LA CUMBRE.
        Transform falls = Spot(t, "Poza de la cascada", new Vector3(-14f, 0f, 18f), "La cascada: respirar bajo el agua fría", 4f);
        Prop(falls, "Roca de la cascada", new Vector3(0f, 1.5f, 2f), new Vector3(3f, 3f, 1.2f), stone);
        Prop(falls, "Chorro de agua", new Vector3(0f, 1.5f, 1.3f), new Vector3(1f, 3f, 0.2f), water, false);
        Prop(falls, "Poza", new Vector3(0f, 0.02f, 0f), new Vector3(4f, 0.04f, 3f), water, false);
        Transform summit = Spot(t, "Cumbre", new Vector3(16f, 0f, -6f), "La cumbre: el sol sale sobre el valle", 3f);
        Prop(summit, "Roca plana de la cumbre", new Vector3(0f, 0.3f, 0f), new Vector3(3f, 0.6f, 2.4f), stone);

        // La gente: insectos-humanos. Goku, un ESCARABAJO PELOTERO (el insecto más fuerte por su peso, Onthophagus taurus, que
        // entrena empujando cada día su carga), con el naranja y el azul de su versión humana.
        Color orange = new Color(0.95f, 0.47f, 0.12f), grey = new Color(0.55f, 0.55f, 0.52f), blue = new Color(0.12f, 0.25f, 0.62f);
        GameObject goku = Who(t, "Goku", new Vector3(2f, 0f, 9f), false, 1.75f, "DungBeetle"); CubeInsect.WearShirt(goku, orange);
        GameObject master = Who(t, "Maestra de la montaña (Shanidar)", new Vector3(-2f, 0f, 16f), true, 1.6f); CubeInsect.WearShirt(master, grey);
        GameObject rival = Who(t, "Compañero de entrenamiento (Momo)", new Vector3(4f, 0f, 17f), false, 1.7f); CubeInsect.WearShirt(rival, blue); Routine(rival, yard, falls);
        GameObject farmer = Who(t, "Campesina del arrozal (Ruth)", new Vector3(15f, 0f, 8f), true); CubeInsect.WearShirt(farmer, new Color(0.45f, 0.55f, 0.6f)); Routine(farmer, chores[1], chores[2]);
        GameObject cutter = Who(t, "Leñador de la aldea (Atlas)", new Vector3(12f, 0f, 0f), false); CubeInsect.WearShirt(cutter, new Color(0.5f, 0.4f, 0.3f)); Routine(cutter, chores[0], river);
        int n = 5;

        Mission(t, "Goku en la aldea de la montaña (homenaje)", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("Al amanecer, habla con la maestra de la montaña", master.transform, "Maestra", "Hoy no vas a golpear ningún poste. Hoy vas a entrenar como entrenaban los viejos: sirviendo a la aldea. El aljibe está vacío."),
                Collect("Sube tres cubos de agua del torrente al aljibe", buckets, cistern, "Goku", "¡Uf! Pesan más que cualquier rival. Y ahora toda la aldea tiene agua para el día."),
                Visit("Ayuda en la aldea: apila la leña, escarda el bancal y levanta la cerca", chores, "Campesina del arrozal", "Con la misma fuerza con que golpeas, ayudas. Eso también es kung fu: trabajo hecho con esfuerzo."),
                Hold("Bajo la cascada: respira despacio con el agua fría encima", falls, 10f, "Maestra", "El cuerpo quiere salir corriendo. No lo dejes mandar: respira largo y tranquilo."),
                Talk("Habla con tu compañero de entrenamiento", rival.transform, "Compañero", "Ayer me ganaste y hoy no me has dicho nada. Gracias. Mañana te enseño cómo me esquivaste, que yo también aprendo."),
                Reach("Sube a la cumbre antes de que salga el sol del todo", summit, "Goku", "¡Desde aquí se ve todo el valle! Mira: el humo de las cocinas, el arrozal, el torrente."),
                Hold("En la cumbre: haz la forma despacio, con el sol", summit, 10f, "Maestra", "Despacio se aprende lo que deprisa se olvida."),
                Talk("Vuelve con la maestra", master.transform, "Maestra", "Ser fuerte no es ganar. Es poder ayudar cuando alguien lo necesita. Eso ya lo sabías; hoy lo has entrenado."),
            },
            "Una aldea en un valle de montaña, al amanecer, sobre el Dojo del santuario a escala de hormiga. Un HOMENAJE ORIGINAL a Goku, el personaje de ficción de Dragon Ball: aquí no hay combates ni técnicas de la obra, sino un día de entrenamiento en el que la fuerza se usa para servir al pueblo. E para hablar, recoger y ayudar; para respirar, quédate quieto.",
            new[]
            {
                "Goku es el protagonista de Dragon Ball, el manga de Akira Toriyama (1955–2024) publicado en la revista Weekly Shōnen Jump entre 1984 y 1995 y después llevado al anime. El personaje se inspira en Sun Wukong, el Rey Mono de la novela china «Viaje al Oeste» (siglo XVI, atribuida a Wu Cheng'en).",
                "Este microcosmos no recrea ninguna escena, diálogo, lugar ni técnica de la obra: es una historia nueva, en homenaje, sobre la idea que Goku encarna para muchos lectores: entrenar cada día, disfrutar del esfuerzo y usar la fuerza para proteger a los demás.",
                "La aldea se inspira en las escuelas de artes marciales de las montañas de China: el monasterio de Shaolin, en el monte Song (Henan, fundado hacia el 495), y los templos de los montes Wudang (Hubei), Patrimonio de la Humanidad desde 1994. En las historias de esas escuelas, el aprendiz empieza por las tareas: acarrear agua, cortar leña, cuidar el huerto.",
                "Como no es una historia real, su fecha («un amanecer de primavera») y su lugar son de ficción; su estilo de casas y muebles es el de una aldea de montaña de la China tradicional.",
            },
            new[]
            {
                "Toriyama, A. (1984–1995). Dragon Ball. Shūeisha (Weekly Shōnen Jump).",
                "Wu Cheng'en (atrib.), Viaje al Oeste (trad. A. C. Yu, The Journey to the West, University of Chicago Press, 2012).",
                "Shahar, M. (2008). The Shaolin Monastery: History, Religion, and the Chinese Martial Arts. University of Hawai'i Press.",
                "UNESCO, «Ancient Building Complex in the Wudang Mountains» (whc.unesco.org/en/list/705).",
            },
            "Maestra", "Mañana el aljibe volverá a estar vacío. Y tú volverás a llenarlo.");
        return n;
    }

    // ══ BILL GATES · Albuquerque (Nuevo México), primavera de 1975 · Mecánica ═════════════════════════════════════════════════════
    //    El BASIC del Altair 8800 funciona en MITS a la primera; Gates y Allen fundan Micro-Soft (4 de abril de 1975).

    static int BillGates(Transform t)
    {
        Material blue = M("Panel_azul_Altair", 0.2f, 0.32f, 0.55f), grey = M("Gris_metal", 0.62f, 0.63f, 0.65f), led = M("Led_rojo", 0.95f, 0.15f, 0.1f);
        Material paper = M("Cinta_de_papel", 0.95f, 0.9f, 0.7f), wood = M("Formica_madera", 0.6f, 0.45f, 0.3f), cream = M("Teletipo_crema", 0.86f, 0.82f, 0.7f);

        // 1 · EL ALTAIR 8800 con su teletipo y su lector de cinta perforada (MITS, en un pequeño centro comercial).
        Transform altair = Spot(t, "Altair 8800", new Vector3(0f, 0f, 14f), "El Altair 8800: el BASIC carga desde una cinta de papel", 3f);
        Prop(altair, "Mesa de trabajo", new Vector3(0f, 0.4f, 0f), new Vector3(3.2f, 0.8f, 1.2f), wood);
        Prop(altair, "Altair 8800", new Vector3(0f, 1f, 0f), new Vector3(1.2f, 0.4f, 0.9f), blue);
        for (int k = 0; k < 8; k++) Prop(altair, "Luz del panel", new Vector3(-0.5f + k * 0.14f, 1.1f, -0.46f), new Vector3(0.05f, 0.05f, 0.02f), led, false);
        var parts = new List<Transform>();
        Transform tty = Spot(t, "Teletipo", new Vector3(-6f, 0f, 14f), "Teletipo (escribe y lee)", 2.4f);
        Prop(tty, "Teletipo Modelo 33", new Vector3(0f, 0.55f, 0f), new Vector3(1f, 1.1f, 0.8f), cream); parts.Add(tty);
        Transform reader = Spot(t, "Lector de cinta perforada", new Vector3(6f, 0f, 14f), "Lector de cinta perforada", 2.4f);
        Prop(reader, "Lector", new Vector3(0f, 0.5f, 0f), new Vector3(0.6f, 1f, 0.5f), grey); parts.Add(reader);
        parts.Add(altair);

        // 2 · LAS COPIAS DEL BASIC para enviar a los aficionados, y la mesa de envíos de MITS.
        var tapes = Items(t, paper, new Vector3(0.35f, 0.35f, 0.12f), ("Rollo de cinta del BASIC", new Vector3(12f, 0.2f, 4f)), ("Rollo de cinta del BASIC", new Vector3(14f, 0.2f, 6f)), ("Rollo de cinta del BASIC", new Vector3(16f, 0.2f, 4f)));
        Transform ship = Spot(t, "Mesa de envíos de MITS", new Vector3(-14f, 0f, 4f), "Envíos: kits del Altair y cintas del BASIC", 2.4f);
        Prop(ship, "Mesa de envíos", new Vector3(0f, 0.45f, 0f), new Vector3(2.4f, 0.9f, 1f), wood);

        // 3 · EL MOTEL: la mesa donde se escribe código a mano, de noche.
        Transform motel = Spot(t, "Mesa del motel", new Vector3(-15f, 0f, -6f), "La habitación del motel: código a lápiz", 2.4f);
        Prop(motel, "Mesa pequeña", new Vector3(0f, 0.38f, 0f), new Vector3(1.2f, 0.76f, 0.7f), wood);
        Prop(motel, "Listado de código", new Vector3(0f, 0.78f, 0f), new Vector3(0.6f, 0.02f, 0.4f), paper, false);

        // La gente: insectos-humanos. Gates, una HORMIGA (la colonia resuelve problemas repartiéndolos en pasos sencillos: los
        // algoritmos de colonia de hormigas, Dorigo 1992), con el jersey azul de su versión humana.
        Color sweater = new Color(0.22f, 0.32f, 0.55f), brown = new Color(0.45f, 0.35f, 0.25f);
        GameObject gates = Who(t, "Bill Gates", new Vector3(3f, 0f, 8f), false, 1.77f, "Ant"); CubeInsect.WearShirt(gates, sweater);
        GameObject allen = Who(t, "Paul Allen", new Vector3(-3f, 0f, 10f), false, 1.85f, "Spider"); CubeInsect.WearShirt(allen, brown);
        GameObject roberts = Who(t, "Ed Roberts (MITS)", new Vector3(8f, 0f, 10f), false, 1.9f, "DungBeetle"); CubeInsect.Wear1940s(roberts, new Color(0.35f, 0.38f, 0.42f), CubeInsect.Hat.None);
        GameObject clerk = Who(t, "Empleada de envíos de MITS (Ruth)", new Vector3(-12f, 0f, 2f), true); CubeInsect.WearShirt(clerk, new Color(0.7f, 0.5f, 0.35f)); Routine(clerk, ship, altair);
        int n = 4;

        Mission(t, "Bill Gates y Paul Allen: el BASIC del Altair (1975)", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("En MITS, habla con Paul Allen", allen.transform, "Paul Allen", "Volé hasta aquí con el BASIC en una cinta de papel. En el avión me di cuenta de que no habíamos escrito el programita que lee la cinta… y lo escribí a mano antes de aterrizar."),
                Visit("Mira las tres piezas: el teletipo, el lector de cinta y el Altair", parts, "Ed Roberts", "El Altair no tiene pantalla ni teclado: solo luces e interruptores. Con el teletipo y la cinta, se le puede hablar."),
                Hold("Quédate mirando mientras la cinta carga", altair, 10f, "Paul Allen", "El teletipo escribe «MEMORY SIZE?». ¡Funciona! Nunca lo habíamos probado en un Altair de verdad: solo en un simulador que escribí en Harvard."),
                Talk("Habla con Ed Roberts", roberts.transform, "Ed Roberts", "Lo vendemos con el Altair. Si los aficionados pueden programar en BASIC, cualquiera podrá usar un ordenador."),
                Talk("Habla con Bill Gates", gates.transform, "Bill Gates", "Paul se queda en MITS y yo vengo cuando puedo. Montamos una empresa para hacer programas para estos micro-ordenadores: Micro-Soft."),
                Collect("Lleva las tres cintas del BASIC a la mesa de envíos", tapes, ship, "Empleada de envíos", "Cada kit sale con su cinta. Los clubes de aficionados se las pasan unos a otros."),
                Hold("De noche, en el motel: repasa el código a lápiz", motel, 8f, "Bill Gates", "Cada byte cuenta: el Altair tiene muy poca memoria. Hay que decir lo mismo con menos."),
                Talk("Vuelve con Bill Gates", gates.transform, "Bill Gates", "Un día habrá un ordenador en cada mesa y en cada casa. Hoy es una caja azul con luces."),
            },
            "Albuquerque (Nuevo México), en la primavera de 1975, sobre el taller de Mecánica del santuario a escala de hormiga (pendiente de su piso: una sala de informática). En MITS, la pequeña empresa que fabrica el Altair 8800, el BASIC de Bill Gates y Paul Allen funciona por primera vez. Ayúdales a cargarlo, a enviarlo y a empezar su empresa. E para hablar, mirar y recoger; para esperar, quédate quieto.",
            new[]
            {
                "Bill Gates (Seattle, 1955) y Paul Allen (Seattle, 1953–2018) se conocieron en el instituto Lakeside, donde aprendieron a programar. En enero de 1975 la revista Popular Electronics sacó en portada el Altair 8800 de MITS, una empresa de Albuquerque: uno de los primeros ordenadores personales, vendido en kit.",
                "Gates y Allen escribieron un intérprete de BASIC para el Altair en unas ocho semanas, en Harvard, sin tener ninguno: Allen programó un simulador del procesador Intel 8080 en un ordenador grande de la universidad (y Monte Davidoff escribió las rutinas de cálculo con decimales). Allen voló a Albuquerque con el programa en una cinta perforada; según su relato, escribió el cargador de la cinta en el avión. En MITS funcionó a la primera.",
                "Allen entró en MITS como director de software el 1 de marzo de 1975 y la sociedad Micro-Soft se fundó en Albuquerque el 4 de abril de 1975 (el nombre fue idea de Allen). El 22 de julio firmaron la licencia del BASIC con MITS. Microsoft se quedó en Albuquerque hasta 1979, cuando se mudó a Bellevue (Washington).",
                "Las fechas exactas de la demostración (entre febrero y marzo) y algunos detalles cambian según quién lo cuente. Años después, Gates y su entonces esposa crearon una fundación que financia, entre otras cosas, vacunas para los países más pobres.",
            },
            new[]
            {
                "Allen, P. (2011). Idea Man: A Memoir by the Cofounder of Microsoft. Portfolio.",
                "Microsoft, «History of Microsoft: 1975» (learn.microsoft.com/en-us/shows/history/history-of-microsoft-1975).",
                "Computer History Museum, «Altair BASIC Interpreter Source Tape» (computerhistory.org/revolution/artifact/312/1142).",
                "Freiberger, P. y Swaine, M. (2000). Fire in the Valley: The Making of the Personal Computer, 2.ª ed. McGraw-Hill.",
            },
            "Bill Gates", "Empezamos con una cinta de papel y muchas ganas de aprender. Lo demás vino después.");
        return n;
    }

    // ══ WARREN BUFFETT · Omaha (Nebraska), 30 de agosto de 1983 · Estudio Textil ═════════════════════════════════════════════════
    //    El día de su 53 cumpleaños, Buffett compra la Nebraska Furniture Mart a Rose Blumkin con un contrato de una página, un apretón
    //    de manos y sin auditoría. (La tienda empezó vendiendo alfombras y telas: de ahí el Estudio Textil.)

    static int WarrenBuffett(Transform t)
    {
        Material wood = M("Nogal", 0.45f, 0.32f, 0.22f), rug = M("Alfombra_roja", 0.65f, 0.2f, 0.18f), rug2 = M("Alfombra_azul", 0.25f, 0.35f, 0.6f);
        Material sofa = M("Tapizado", 0.55f, 0.48f, 0.38f), white = M("Esmalte_blanco", 0.92f, 0.92f, 0.9f), paper = M("Papel", 0.95f, 0.93f, 0.86f);

        // 1 · EL DESPACHO de Berkshire Hathaway (Kiewit Plaza): una mesa y los informes anuales.
        Transform office = Spot(t, "Despacho de Buffett", new Vector3(-14f, 0f, 10f), "El despacho de Berkshire Hathaway (Kiewit Plaza)", 2.8f);
        Prop(office, "Mesa del despacho", new Vector3(0f, 0.4f, 0f), new Vector3(2f, 0.8f, 1f), wood);
        Prop(office, "Pila de informes anuales", new Vector3(0.5f, 0.9f, 0f), new Vector3(0.5f, 0.2f, 0.4f), paper, false);

        // 2 · LA TIENDA: alfombras, muebles y electrodomésticos, y la mesa de la señora B.
        Transform store = Spot(t, "Nebraska Furniture Mart", new Vector3(4f, 0f, 14f), "Nebraska Furniture Mart (72nd Street)", 3.5f);
        Transform carpets = Spot(t, "Sección de alfombras", new Vector3(-2f, 0f, 18f), "Alfombras (por donde empezó en 1937)", 2.4f);
        Prop(carpets, "Rollo de alfombra", new Vector3(-1f, 0.35f, 0f), new Vector3(3f, 0.7f, 0.7f), rug);
        Prop(carpets, "Rollo de alfombra", new Vector3(1.5f, 0.35f, 1f), new Vector3(3f, 0.7f, 0.7f), rug2);
        Transform furniture = Spot(t, "Sección de muebles", new Vector3(6f, 0f, 19f), "Muebles", 2.4f);
        Prop(furniture, "Sofá", new Vector3(0f, 0.4f, 0f), new Vector3(2.2f, 0.8f, 0.9f), sofa);
        Transform appliances = Spot(t, "Sección de electrodomésticos", new Vector3(12f, 0f, 16f), "Electrodomésticos", 2.4f);
        Prop(appliances, "Nevera", new Vector3(0f, 0.9f, 0f), new Vector3(0.8f, 1.8f, 0.7f), white);
        Prop(appliances, "Lavadora", new Vector3(1.2f, 0.45f, 0f), new Vector3(0.7f, 0.9f, 0.7f), white);
        var sections = new List<Transform> { carpets, furniture, appliances };
        Transform desk = Spot(t, "Mesa de la señora B", new Vector3(8f, 0f, 8f), "La mesa de Rose Blumkin", 2.4f);
        Prop(desk, "Mesa", new Vector3(0f, 0.38f, 0f), new Vector3(1.4f, 0.76f, 0.8f), wood);
        var contract = Items(t, paper, new Vector3(0.3f, 0.02f, 0.4f), ("Contrato de una página", new Vector3(-12f, 0.85f, 10f)));

        // La gente: insectos-humanos. Buffett, una HORMIGA SEGADORA (guarda grano en los años buenos para los malos; paciencia), con
        // el traje gris de su versión humana; la señora B, una ABEJA OBRERA (trabajó en la tienda hasta pasados los 100 años).
        GameObject buffett = Modern(t, "Warren Buffett", new Vector3(-11f, 0f, 8f), false, new Color(0.42f, 0.43f, 0.46f), CubeInsect.Hat.None, 1.78f, "Ant");
        GameObject mrsB = Modern(t, "Rose Blumkin (la señora B)", new Vector3(8f, 0f, 6f), true, new Color(0.3f, 0.3f, 0.35f), CubeInsect.Hat.None, 1.47f, "SnowBee");
        GameObject louie = Modern(t, "Louie Blumkin", new Vector3(10f, 0f, 11f), false, new Color(0.35f, 0.33f, 0.3f), CubeInsect.Hat.None, 1.75f, "Cricket");
        GameObject seller = Modern(t, "Vendedora de alfombras (Medea)", new Vector3(-3f, 0f, 15f), true, new Color(0.55f, 0.25f, 0.25f)); Routine(seller, carpets, furniture);
        GameObject buyer = Modern(t, "Cliente de Omaha (Momo)", new Vector3(2f, 0f, 10f), false, new Color(0.4f, 0.5f, 0.35f)); Routine(buyer, appliances, furniture, carpets);
        int n = 5;

        Mission(t, "Warren Buffett y la señora B: un apretón de manos (1983)", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("En su despacho, habla con Warren Buffett", buffett.transform, "Warren Buffett", "Hoy cumplo 53 años y quiero hacerme un regalo: comprar la mejor tienda de Omaha. Pero solo si la señora B quiere venderla."),
                Reach("Ve a la Nebraska Furniture Mart", store, "Cliente de Omaha", "Aquí compra media ciudad. Lo tienen todo, y más barato que nadie."),
                Talk("Habla con Rose Blumkin, la señora B", mrsB.transform, "Rose Blumkin", "Llegué de Rusia sin saber inglés y sin ir a la escuela. En 1937 abrí esto con 500 dólares en un sótano. Mi regla: vende barato y di la verdad."),
                Visit("Recorre la tienda: alfombras, muebles y electrodomésticos", sections, "Vendedora de alfombras", "La señora B se sabe el precio de cada alfombra de memoria, y lo que nos costó."),
                Talk("Habla con Louie, su hijo", louie.transform, "Louie Blumkin", "Mi madre no sabe leer un balance, pero sabe exactamente lo que vale cada cosa. Y nunca miente a un cliente."),
                Collect("Lleva el contrato de una página a la mesa de la señora B", contract, desk, "Warren Buffett", "No necesito auditores ni abogados. Conozco esta tienda como cliente desde hace años, y me fío de ella."),
                Hold("En la mesa: el apretón de manos", desk, 8f, "Rose Blumkin", "Trato hecho. Usted me da su palabra y yo le doy la mía. Y yo sigo viniendo a trabajar."),
                Talk("Vuelve con Warren Buffett", buffett.transform, "Warren Buffett", "Prefiero un trato sencillo con alguien honrado que uno perfecto con alguien de quien no me fío."),
            },
            "Omaha (Nebraska, Estados Unidos), el 30 de agosto de 1983, el día en que Warren Buffett cumple 53 años, sobre el Estudio Textil del santuario a escala de hormiga (pendiente de su piso: un mercado y oficina de comercio). Buffett quiere comprar la tienda de muebles de Rose Blumkin, una inmigrante que la levantó de la nada. Acompáñale: la compra se hará con una página y un apretón de manos. E para hablar, recorrer y recoger; para el trato, quédate quieto.",
            new[]
            {
                "Warren Buffett (Omaha, 1930) dirige desde 1965 Berkshire Hathaway, una empresa que compra otras empresas y las deja en manos de quienes las llevan. Vive en Omaha, en la casa de Farnam Street que compró en 1958.",
                "Rose Blumkin (1893–1998), «la señora B», nació cerca de Minsk, llegó a Estados Unidos en 1917 tras cruzar Siberia, China y Japón, y nunca fue a la escuela. En 1937 abrió la Nebraska Furniture Mart en el sótano de la tienda de su marido, en Omaha, con 500 dólares; su lema era «vende barato y di la verdad».",
                "El 30 de agosto de 1983, el día de su cumpleaños, Buffett le llevó una propuesta de compra de poco más de una página. La cerraron con un apretón de manos y sin auditoría: no revisó el inventario ni las cuentas. Las cifras varían según la fuente (unos 55 millones de dólares por el 80–90 % de la tienda; la familia se quedó el resto). Buffett lo contó en su carta a los accionistas de 1983.",
                "La señora B siguió trabajando en la tienda, en un carrito eléctrico, hasta pasados los 100 años. En 2006 Buffett se comprometió a dar casi toda su fortuna a la filantropía, y en 2010 lanzó con Bill y Melinda Gates el Giving Pledge.",
            },
            new[]
            {
                "Buffett, W. (1984). Carta a los accionistas de Berkshire Hathaway de 1983 (berkshirehathaway.com/letters/1983.html).",
                "The New York Times (agosto de 1998), obituario de Rose Blumkin («Rose Blumkin, Retail Queen, Dies at 104»).",
                "Lowenstein, R. (1995). Buffett: The Making of an American Capitalist. Random House.",
                "Benzinga (2018), «This Day In Market History: Warren Buffett Buys Nebraska Furniture Mart».",
            },
            "Warren Buffett", "La confianza no sale en ningún balance, y es lo que más vale.");
        return n;
    }

    // ══ MACKENZIE SCOTT · Prairie View A&M University (Texas), 15 de diciembre de 2020 · Química ════════════════════════════════
    //    Su donación sin condiciones de 50 millones de dólares, la mayor de la historia de la universidad. Ella no estuvo en el campus:
    //    su mesa de escritura (donde escribió el ensayo que anunció las donaciones) está en el claro, como un recuerdo.

    static int MacKenzieScott(Transform t)
    {
        Material wood = M("Madera_clara", 0.7f, 0.58f, 0.42f), paper = M("Papel", 0.95f, 0.93f, 0.86f), maroon = M("Granate_PVAMU", 0.45f, 0.12f, 0.2f);
        Material gold = M("Oro_PVAMU", 0.85f, 0.7f, 0.3f), screen = M("Pantalla", 0.12f, 0.14f, 0.18f), books = M("Lomos_de_libros", 0.4f, 0.3f, 0.45f);

        // 1 · LA MESA DE ESCRITURA (Seattle): el portátil con el ensayo «384 maneras de ayudar».
        Transform writing = Spot(t, "Mesa de escritura de Scott", new Vector3(-15f, 0f, 10f), "La mesa donde escribe MacKenzie Scott (Seattle)", 2.6f);
        Prop(writing, "Mesa", new Vector3(0f, 0.38f, 0f), new Vector3(1.6f, 0.76f, 0.8f), wood);
        Prop(writing, "Portátil (el ensayo)", new Vector3(0f, 0.82f, 0f), new Vector3(0.5f, 0.05f, 0.35f), screen, false);

        // 2 · LA PRESIDENCIA de la universidad y la oficina de ayudas.
        Transform president = Spot(t, "Despacho de la presidenta", new Vector3(2f, 0f, 16f), "Despacho de la presidenta de Prairie View A&M", 2.8f);
        Prop(president, "Mesa de la presidenta", new Vector3(0f, 0.4f, 0f), new Vector3(2f, 0.8f, 1f), wood);
        Prop(president, "Bandera granate y oro", new Vector3(1.4f, 1.2f, 0.6f), new Vector3(0.6f, 0.9f, 0.04f), maroon, false);
        Transform aid = Spot(t, "Oficina de ayudas a estudiantes", new Vector3(14f, 0f, 6f), "Oficina de ayudas: las deudas que impedían graduarse", 2.8f);
        Prop(aid, "Mostrador", new Vector3(0f, 0.5f, 0f), new Vector3(2.4f, 1f, 0.8f), wood);
        var letters = Items(t, paper, new Vector3(0.3f, 0.02f, 0.22f), ("Carta de ayuda de emergencia", new Vector3(4f, 0.85f, 16f)), ("Carta de ayuda de emergencia", new Vector3(4.5f, 0.85f, 15.6f)), ("Carta de ayuda de emergencia", new Vector3(3.5f, 0.85f, 15.6f)));

        // 3 · LA BIBLIOTECA.
        Transform library = Spot(t, "Biblioteca", new Vector3(-12f, 0f, -6f), "La biblioteca John B. Coleman", 3f);
        for (int k = 0; k < 3; k++) Prop(library, "Estantería", new Vector3(-3f + k * 3f, 1f, 2f), new Vector3(2f, 2f, 0.5f), books);
        Prop(library, "Mesa de estudio", new Vector3(0f, 0.38f, -1f), new Vector3(3f, 0.76f, 1f), wood);
        Prop(library, "Banderín de la graduación", new Vector3(0f, 1.6f, 2.3f), new Vector3(1.2f, 0.5f, 0.04f), gold, false);

        // La gente: insectos-humanos. Scott, una ABEJA (da sin pedir nada a cambio: poliniza las flores que visita y sigue su vuelo),
        // con la blusa azul claro de su versión humana; Ruth Simmons, la presidenta, y los estudiantes.
        Color blouse = new Color(0.62f, 0.75f, 0.88f), suit = new Color(0.3f, 0.25f, 0.35f);
        GameObject scott = Modern(t, "MacKenzie Scott", new Vector3(-13f, 0f, 7f), true, blouse, CubeInsect.Hat.None, 1.7f, "SnowBee");
        GameObject simmons = Modern(t, "Ruth Simmons", new Vector3(5f, 0f, 13f), true, suit, CubeInsect.Hat.None, 1.68f, "Firefly");
        var students = new List<Transform>();
        foreach (var (name, pos, female) in new[] { ("Estudiante de ingeniería (Sakshi)", new Vector3(10f, 0f, -4f), true), ("Estudiante de enfermería (Medea)", new Vector3(16f, 0f, -2f), true), ("Estudiante de agronomía (Momo)", new Vector3(13f, 0f, -9f), false) })
        { GameObject s = Modern(t, name, pos, female, new Color(0.45f, 0.12f, 0.2f)); Routine(s, library, aid); students.Add(s.transform); }
        GameObject clerk = Modern(t, "Encargado de la oficina de ayudas (Héspero)", new Vector3(14f, 0f, 3f), false, new Color(0.35f, 0.38f, 0.42f));
        int n = 6;

        Mission(t, "MacKenzie Scott y Prairie View: dar sin condiciones (2020)", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("En su mesa de escritura, habla con MacKenzie Scott", scott.transform, "MacKenzie Scott", "Este año la pandemia ha hecho más pobres a los que ya lo tenían difícil. Con un pequeño equipo hemos buscado a quienes ya hacen el trabajo, para darles el dinero sin pedirles nada a cambio. Ellos saben mejor que yo en qué usarlo."),
                Hold("Quédate mientras escribe el ensayo que lo anuncia", writing, 8f, "MacKenzie Scott", "No quiero que esto trate de mí. Voy a escribir sobre ellos: 384 organizaciones, y por qué las elegimos."),
                Reach("15 de diciembre: ve al despacho de la presidenta de Prairie View A&M", president, "Encargado de la oficina de ayudas", "La presidenta acaba de colgar el teléfono. No dice nada: solo sonríe."),
                Talk("Habla con Ruth Simmons, la presidenta", simmons.transform, "Ruth Simmons", "Cincuenta millones de dólares, sin condiciones: la mayor donación en los casi 145 años de esta universidad. Y lo primero que haremos es ayudar a los que están a punto de terminar."),
                Visit("Habla con tres estudiantes que no podían pagar su último curso", students, "Estudiante de enfermería", "Me faltaba un semestre y debía la matrícula. Pensaba dejarlo y ponerme a trabajar."),
                Collect("Lleva las tres cartas de ayuda a la oficina de ayudas", letters, aid, "Encargado de la oficina de ayudas", "Diez millones para ayudas de emergencia a los de tercero y cuarto. Estas tres deudas quedan pagadas."),
                Hold("En la biblioteca: estudia con ellos un rato", library, 8f, "Estudiante de ingeniería", "Ahora solo tengo que preocuparme de aprobar."),
                Talk("Vuelve con Ruth Simmons", simmons.transform, "Ruth Simmons", "El resto va al fondo de la universidad, para que dure. Esto no es solo dinero: es que alguien de fuera nos haya mirado y haya confiado en nosotros."),
            },
            "Prairie View A&M University (Texas), el 15 de diciembre de 2020, en plena pandemia, sobre el laboratorio de Química del santuario a escala de hormiga (pendiente de su piso: un aula y biblioteca universitaria). La escritora MacKenzie Scott dona, sin condiciones, la mayor cantidad que esta universidad ha recibido nunca. Mira cómo llega la ayuda a los estudiantes. E para hablar, recorrer y recoger; para escribir y estudiar, quédate quieto.",
            new[]
            {
                "MacKenzie Scott (San Francisco, 1970) es novelista: estudió en Princeton, donde Toni Morrison fue su profesora, y publicó «The Testing of Luther Albright» (2005) y «Traps» (2013). En 2019 firmó el Giving Pledge, el compromiso de dar la mayor parte de su fortuna.",
                "En 2020 dio de forma muy distinta a la habitual: sin convocatorias ni informes, con un equipo de asesores que buscó organizaciones que ya hacían el trabajo, y donaciones sin condiciones (cada organización decide cómo usarlas). Lo anunció en dos ensayos: el 28 de julio («116 Organizations Driving Change», unos 1700 millones de dólares) y el 15 de diciembre («384 Ways to Help», unos 4200 millones para 384 organizaciones: bancos de alimentos, ayudas de emergencia, universidades históricamente negras, como Prairie View).",
                "Prairie View A&M, fundada en 1876, es la segunda universidad pública más antigua de Texas y una universidad históricamente negra (HBCU). Sus 50 millones fueron la mayor donación de su historia y aumentaron su fondo en torno a un 40 %. Su presidenta, Ruth Simmons, destinó 10 millones a ayudas de emergencia para estudiantes de tercero y cuarto con deudas de matrícula, y la mayor parte del resto al fondo.",
                "Scott no estuvo en el campus: en este microcosmos su mesa de escritura está en el claro como recuerdo de dónde salió la ayuda.",
            },
            new[]
            {
                "Scott, M. (15-12-2020). «384 Ways to Help». Medium (mackenzie-scott.medium.com).",
                "Prairie View A&M University (15-12-2020), «Donor announces landmark $50 million gift to Prairie View A&M University» (pvamu.edu).",
                "The Texas Tribune (15-12-2020), «Prairie View A&M receives $50 million donation from MacKenzie Scott».",
                "The Chronicle of Philanthropy (2020), «Gifts From MacKenzie Scott Put Overlooked Colleges and Universities in the Spotlight».",
            },
            "Estudiante de ingeniería", "Me gradúo en mayo. Y algún día ayudaré yo a otro.");
        return n;
    }
}
