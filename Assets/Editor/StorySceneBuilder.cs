using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// MICROCOSMOS DE HISTORIAS (docs/microcosmos-historias.md): la vida de grandes figuras recreada sobre el área del
/// santuario que mejor la cuenta, ×10, cada una en SU escena y con entrada por la máquina de virtualización del área. El
/// elenco de la enfermería renace con su nombre en cada historia. La misión es una <see cref="StoryMission"/> (pasos con E)
/// y al terminar la <see cref="HistoryCard"/> cuenta lo que pasó de verdad, con fuentes.
/// - Sala de Yoga: Patanjali y los ocho miembros del yoga.
/// - Gimnasio: la tregua de Navidad de 1914 (la guerra; el partido en tierra de nadie).
/// - Enfermería: Florence Nightingale en Escútari (1854–55) · Siddhārtha y los cuatro encuentros.
/// - Mecánica: los hermanos Wright, de la bicicleta al avión (1892–1903).
/// - Planta Atómica: Eratóstenes mide la Tierra (~240 a. C.), primer tramo de la escalera de la física.
/// </summary>
public static class StorySceneBuilder
{
    internal static readonly (string areaGo, string display, string scene, Color tint, System.Func<Transform, int> story)[] Stories =
    {
        ("YogaRoom_Area",        "Patanjali",   "Microcosmos_Patanjali",   new Color(0.75f, 0.65f, 0.85f), new System.Func<Transform, int>(Patanjali)),
        ("Gym_Area",             "Tregua 1914", "Microcosmos_Tregua1914",  new Color(0.55f, 0.6f, 0.7f), new System.Func<Transform, int>(ChristmasTruce)),
        ("Infirmary_Area",       "Escútari",    "Microcosmos_Nightingale", new Color(0.85f, 0.95f, 0.9f), new System.Func<Transform, int>(Nightingale)),
        ("Infirmary_Area",       "Kapilavastu", "Microcosmos_Buda",        new Color(0.85f, 0.95f, 0.9f), new System.Func<Transform, int>(Buddha)),
        ("VehicleWorkshop_Area", "Dayton",      "Microcosmos_Wright",      new Color(0.55f, 0.55f, 0.6f), new System.Func<Transform, int>(Wright)),
        ("AtomicPlant_Area",     "Alejandría",  "Microcosmos_Eratostenes", new Color(0.8f, 0.75f, 0.55f), new System.Func<Transform, int>(Eratosthenes)),
    };

    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos de historias")]
    public static void BuildAll()
    {
        int i = 0;
        foreach (var s in Stories)
        {
            try { MicrocosmosAreaSceneBuilder.BuildStoryScene(s.areaGo, s.display, s.scene, s.tint, new Vector3(6000f + i * 900f, 0f, 12500f), s.story); }
            catch (System.Exception e) { Debug.LogWarning($"[Historias] No se pudo generar {s.scene}: {e.Message}"); }
            i++;
        }
        Debug.Log($"[Historias] {i} microcosmos de historias generados.");
    }

    // ══ PATANJALI · Sala de Yoga ═══════════════════════════════════════════════════════════════════════════════════

    static int Patanjali(Transform t)
    {
        Color white = new Color(0.95f, 0.93f, 0.86f), ochre = new Color(0.85f, 0.6f, 0.3f), skin = new Color(0.42f, 0.28f, 0.2f);
        Material bark = AreaMicrocosmos.Mat("Hist_Corteza", new Color(0.36f, 0.27f, 0.2f)), leaf = AreaMicrocosmos.Mat("Hist_Hoja", new Color(0.25f, 0.45f, 0.22f));
        Material water = AreaMicrocosmos.Mat("Hist_Agua", new Color(0.3f, 0.5f, 0.7f)), earth = AreaMicrocosmos.Mat("Hist_Tierra_batida", new Color(0.62f, 0.5f, 0.36f));

        Transform tree = Tree(t, "Árbol del āśram", new Vector3(0f, 0f, 14f), bark, leaf, 7f);
        Transform mat = Spot(t, "Esterilla", new Vector3(0f, 0f, 9f), "Esterilla (āsana)", 1.2f);
        AreaMicrocosmos.Box(mat, "Esterilla", new Vector3(0f, 0.03f, 0f), new Vector3(1f, 0.06f, 2f), AreaMicrocosmos.Mat("Hist_Esterilla", new Color(0.6f, 0.45f, 0.25f)), false);
        Transform river = Spot(t, "Orilla del río", new Vector3(-18f, 0f, 8f), "Orilla del río (prāṇāyāma)", 2f);
        AreaMicrocosmos.Box(river, "Río", new Vector3(-3f, 0.02f, 0f), new Vector3(4f, 0.04f, 30f), water, false);
        Transform lamp = Spot(t, "Lámpara", new Vector3(12f, 0f, 16f), "Lámpara de aceite (dhāraṇā)", 1.6f);
        AreaMicrocosmos.Box(lamp, "Peana", new Vector3(0f, 0.4f, 0f), new Vector3(0.6f, 0.8f, 0.6f), earth, true);
        AreaMicrocosmos.Box(lamp, "Llama", new Vector3(0f, 0.95f, 0f), new Vector3(0.12f, 0.25f, 0.12f), AreaMicrocosmos.Mat("Hist_Llama", new Color(1f, 0.7f, 0.25f)), false);
        Transform cave = Spot(t, "Cueva silenciosa", new Vector3(20f, 0f, 2f), "Cueva silenciosa (pratyāhāra)", 4f);
        for (int k = 0; k < 3; k++) AreaMicrocosmos.Box(cave, "Roca", new Vector3(0f, 1.2f + k * 0.9f, 2.5f), new Vector3(6f - k * 1.5f, 1.8f, 4f - k), AreaMicrocosmos.Mat("Hist_Roca", new Color(0.5f, 0.47f, 0.42f)), true);
        Transform market = Spot(t, "Mercado", new Vector3(12f, 0f, -4f), "Mercado (ruido)", 3f);
        for (int k = 0; k < 3; k++) AreaMicrocosmos.Box(market, "Puesto", new Vector3(-3f + k * 3f, 0.5f, 0f), new Vector3(2f, 1f, 1.2f), earth, true);
        Transform basket = Spot(t, "Cesta de Ruth", new Vector3(-10f, 0f, -2f), "Cesta de Ruth", 1.5f);
        AreaMicrocosmos.Box(basket, "Cesta", new Vector3(0f, 0.3f, 0f), new Vector3(0.9f, 0.6f, 0.9f), AreaMicrocosmos.Mat("Hist_Mimbre", new Color(0.7f, 0.55f, 0.3f)), true);
        Transform mangoTree = Tree(t, "Mango", new Vector3(-12f, 0f, -8f), bark, leaf, 5f);
        Material mango = AreaMicrocosmos.Mat("Hist_Mango", new Color(0.95f, 0.65f, 0.15f));
        var mangos = new List<Transform>();
        for (int k = 0; k < 3; k++) mangos.Add(AreaMicrocosmos.Box(t, "Mango", new Vector3(6f + k * 2.5f, 0.25f, -9f + k), Vector3.one * 0.4f, mango, false));
        var leaves = new List<Transform>();
        for (int k = 0; k < 5; k++) { Transform l = AreaMicrocosmos.Box(t, "Hojarasca", new Vector3(-6f + k * 3f, 0.05f, 4f + (k % 2) * 3f), new Vector3(1.2f, 0.08f, 0.8f), leaf, false); Label(l, "Hojarasca", 0.6f); leaves.Add(l); }

        GameObject pat = Person(t, "Patanjali", new Vector3(1.5f, 0f, 12.5f), skin, 1.8f);
        CubeInsect.WearGreek(pat, white, wreath: false, beard: true);
        int n = 1;
        foreach (var (name, pos, robe) in new[] { ("Sakshi", new Vector3(-2f, 0f, 10f), ochre), ("Héspero", new Vector3(3f, 0f, 9f), white), ("Atlas", new Vector3(-4f, 0f, 7f), ochre), ("Medea", new Vector3(5f, 0f, 6f), white), ("Shanidar", new Vector3(-1f, 0f, 16f), white) })
        {
            GameObject s = Person(t, name, pos, skin, name == "Shanidar" ? 1.6f : 1.8f);
            CubeInsect.WearGreek(s, robe, wreath: false, beard: name == "Shanidar");
            Routine(s, river, tree, lamp, mat); n++;
        }
        GameObject ruth = Person(t, "Ruth", new Vector3(-9f, 0f, -3f), skin, 1.8f); CubeInsect.WearGreek(ruth, new Color(0.7f, 0.3f, 0.35f), false, false); CubeInsect.WearHat(ruth, CubeInsect.Hat.Turban); Routine(ruth, basket, mangoTree, market); n++;
        GameObject momo = Person(t, "Momo", new Vector3(9f, 0f, -6f), skin, 1.7f); CubeInsect.WearGreek(momo, new Color(0.5f, 0.6f, 0.4f), false, false); Routine(momo, market, cave, basket); n++;
        GameObject amb = Person(t, "Ambrosio", new Vector3(14f, 0f, -2f), skin, 1.8f, 1.3f); CubeInsect.WearGreek(amb, white, false, false); Routine(amb, market, river); n++;

        Mission(t, "Patanjali y los ocho miembros del yoga", StoryMission.Marga.Yoga, 60f,
            new[]
            {
                Talk("Escucha a Patanjali bajo el árbol", pat.transform, "Patanjali", "Yogaś citta-vṛtti-nirodhaḥ: el yoga es aquietar los movimientos de la mente (I.2). Se aprende en ocho pasos."),
                Collect("1 · Yama (no robar): devuelve los mangos que Momo cogió a la cesta de Ruth", mangos, basket, "Ruth", "Gracias. Momo, la próxima vez pídelos: te los habría dado."),
                Visit("2 · Niyama (limpieza): barre la hojarasca del patio", leaves, "Patanjali", "Śauca: limpio por fuera, para empezar a limpiar por dentro."),
                Hold("3 · Āsana: siéntate en la esterilla, firme y cómodo", mat, 12f, "Patanjali", "Sthira-sukham āsanam: la postura es firme y cómoda (II.46). Para mí, āsana es el asiento de meditar."),
                Hold("4 · Prāṇāyāma: respira despacio en la orilla del río", river, 10f, "Sakshi", "Al alargar la respiración, la mente también se calma (II.49–51)."),
                Reach("5 · Pratyāhāra: cruza el mercado sin dejar que el ruido te lleve, hasta la cueva", cave, "Momo", "¡Eh, eh! ¿No me miras? ... Vale, vale, ya veo que vas a lo tuyo."),
                Hold("6 · Dhāraṇā: fija la atención en la llama de la lámpara", lamp, 10f, "Patanjali", "Dhāraṇā es atar la mente a un solo lugar (III.1)."),
                Hold("7–8 · Dhyāna y samādhi: medita bajo el árbol con Patanjali", tree, 15f, "Patanjali", "Cuando la atención fluye sin cortes es dhyāna (III.2); cuando solo queda el objeto, samādhi (III.3)."),
            },
            "Un āśram de la India antigua, a escala de hormiga. Patanjali enseña a sus alumnos (el elenco de la enfermería, renacido) los ocho miembros del yoga. Sigue el camino con ellos: cada paso se practica en un sitio del āśram. E para hablar y recoger; en los pasos de práctica, quédate quieto.",
            new[]
            {
                "Los «Yoga Sūtras» atribuidos a Patanjali son 195–196 aforismos en cuatro capítulos (samādhi, sādhana, vibhūti, kaivalya). De su autor no se sabe casi nada: muchos estudiosos sitúan el texto hacia los siglos IV–V d. C., otros siglos antes. La fecha se discute.",
                "La definición central es la del sūtra I.2: el yoga es aquietar las fluctuaciones de la mente. Los ocho miembros (aṣṭāṅga, II.29) son yama (conducta con los demás: no dañar, no mentir, no robar…), niyama (con uno mismo: limpieza, contento…), āsana, prāṇāyāma, pratyāhāra (retirar los sentidos), dhāraṇā (concentración), dhyāna (meditación) y samādhi (absorción).",
                "En los Sūtras, āsana es sobre todo el asiento estable para meditar (II.46). La mayoría de las posturas que hoy se practican vienen del haṭha yoga medieval y de la gimnasia moderna, siglos después.",
                "La leyenda (no historia): Ādiśeṣa, la serpiente sobre la que descansa Viṣṇu, quiso enseñar a los humanos y cayó, pequeña, en las manos juntas (añjali) de una mujer, Gonikā, que rezaba junto a un río: «pata» (caído) + «añjali» = Patañjali.",
            },
            new[]
            {
                "Bryant, E. F. (2009). The Yoga Sūtras of Patañjali. North Point Press.",
                "Mallinson, J. y Singleton, M. (2017). Roots of Yoga. Penguin Classics.",
                "Iyengar, B. K. S., «The Legend of Sage Patanjali» (bksiyengar.com).",
            },
            "Patanjali", "Lo que has hecho hoy ya no es mío: es tuyo.");
        return n;
    }

    // ══ LA TREGUA DE NAVIDAD (1914) · Gimnasio ═════════════════════════════════════════════════════════════════════
    // La guerra elegida: la Primera Guerra Mundial, contada desde el momento en que los soldados dejaron de dispararse.
    // Va en el GIMNASIO porque su imagen más conocida es el partido de fútbol en tierra de nadie.

    static int ChristmasTruce(Transform t)
    {
        Material mud = AreaMicrocosmos.Mat("Hist_Barro", new Color(0.33f, 0.27f, 0.2f)), sand = AreaMicrocosmos.Mat("Hist_Sacos", new Color(0.62f, 0.55f, 0.4f));
        Material wire = AreaMicrocosmos.Mat("Hist_Alambre", new Color(0.3f, 0.3f, 0.3f)), pine = AreaMicrocosmos.Mat("Hist_Abeto", new Color(0.15f, 0.35f, 0.2f));
        Color khaki = new Color(0.55f, 0.5f, 0.32f), grey = new Color(0.45f, 0.48f, 0.45f), skin = new Color(0.4f, 0.27f, 0.2f);

        Transform british = Trench(t, "Trinchera británica", new Vector3(0f, 0f, -6f), mud, sand);
        Transform german = Trench(t, "Trinchera alemana", new Vector3(0f, 0f, 24f), mud, sand);
        for (int k = 0; k < 4; k++) { Transform tr = AreaMicrocosmos.Box(german, "Árbol de Navidad", new Vector3(-12f + k * 8f, 1.9f, -1.6f), new Vector3(0.6f, 1.2f, 0.6f), pine, false); AreaMicrocosmos.Box(tr, "Vela", new Vector3(0f, 0.6f, 0f), new Vector3(0.2f, 0.2f, 0.2f), AreaMicrocosmos.Mat("Hist_Vela", new Color(1f, 0.85f, 0.4f)), false); }
        Transform parapet = Spot(t, "Parapeto", new Vector3(0f, 0f, -4f), "Parapeto: escucha", 2f);
        Transform middle = Spot(t, "Tierra de nadie", new Vector3(0f, 0f, 9f), "Tierra de nadie", 1.5f);
        for (int k = 0; k < 6; k++) AreaMicrocosmos.Box(t, "Alambre de espino", new Vector3(-15f + k * 6f, 0.5f, 2f + (k % 2) * 13f), new Vector3(4f, 1f, 0.15f), wire, true);
        for (int k = 0; k < 3; k++) AreaMicrocosmos.Box(t, "Cráter", new Vector3(-10f + k * 10f, -0.05f, 6f + k * 2f), new Vector3(3f, 0.1f, 3f), mud, false);
        var fallen = new List<Transform>();
        for (int k = 0; k < 4; k++) { Transform f = AreaMicrocosmos.Box(t, "Caído", new Vector3(-9f + k * 6f, 0.2f, 12f - (k % 2) * 4f), new Vector3(0.6f, 0.4f, 1.8f), AreaMicrocosmos.Mat("Hist_Manta", new Color(0.4f, 0.38f, 0.3f)), false); Label(f, "Caído", 1f); fallen.Add(f); }
        Material ration = AreaMicrocosmos.Mat("Hist_Racion", new Color(0.7f, 0.25f, 0.2f));
        var gifts = new List<Transform>();
        foreach (string g in new[] { "Chocolate", "Carne en lata", "Tabaco" }) { Transform r = AreaMicrocosmos.Box(british, g, new Vector3(-3f + gifts.Count * 2f, 0.3f, 1.2f), new Vector3(0.4f, 0.25f, 0.3f), ration, false); Label(r, g, 0.6f); gifts.Add(r); }
        Transform ball = AreaMicrocosmos.Box(t, "Balón", new Vector3(6f, 0.25f, 2f), Vector3.one * 0.45f, AreaMicrocosmos.Mat("Hist_Cuero", new Color(0.55f, 0.35f, 0.2f)), false); Label(ball, "Balón", 0.8f);
        Transform goal = Spot(t, "Portería", new Vector3(-6f, 0f, 9f), "Portería (dos gorras)", 1f);
        foreach (float x in new[] { -1.5f, 1.5f }) AreaMicrocosmos.Box(goal, "Gorra", new Vector3(x, 0.05f, 0f), new Vector3(0.4f, 0.1f, 0.4f), AreaMicrocosmos.Mat("Hist_Gorra", khaki), false);

        int n = 0;
        GameObject Soldier(string name, Vector3 pos, bool brit, float h = 1.8f)
        {
            GameObject g = Person(t, name, pos, skin, h);
            CubeInsect.Wear1940s(g, brit ? khaki : grey, brit ? CubeInsect.Hat.Cap : CubeInsect.Hat.Spike);
            n++; return g;
        }
        GameObject atlas = Soldier("Atlas", new Vector3(3f, 0f, -7f), true);
        GameObject hespero = Soldier("Héspero", new Vector3(-8f, 0f, -8f), true);
        GameObject momo = Soldier("Momo", new Vector3(8f, 0f, -7f), true, 1.7f);
        GameObject ambrosio = Soldier("Ambrosio", new Vector3(-3f, 0f, -8f), true);
        GameObject medea = Soldier("Medea", new Vector3(0f, 0f, 21f), false);
        GameObject ruth = Soldier("Ruth", new Vector3(-6f, 0f, 22f), false);
        GameObject sakshi = Soldier("Sakshi", new Vector3(7f, 0f, 22f), false);
        GameObject shanidar = Soldier("Shanidar", new Vector3(12f, 0f, 23f), false, 1.6f);
        foreach (GameObject g in new[] { hespero, momo, ambrosio }) Routine(g, british, parapet);
        foreach (GameObject g in new[] { ruth, sakshi, shanidar }) Routine(g, german);

        Mission(t, "La tregua de Navidad, Flandes, 1914", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Hold("Nochebuena: escucha desde el parapeto lo que llega de la otra trinchera", parapet, 8f, "Medea", "Stille Nacht, heilige Nacht…"),
                Talk("Habla con el sargento Atlas", atlas.transform, "Atlas", "Están cantando. Y han puesto árboles con velas en su parapeto. ¿Les respondemos con otro villancico?"),
                Reach("Sal a la tierra de nadie desarmado, con las manos a la vista", middle, "Medea", "Frohe Weihnachten! No disparamos si no disparáis."),
                Collect("Lleva a Medea lo que tenéis para regalar", gifts, medea.transform, "Medea", "Danke. Tomad: puros y un botón de mi uniforme, de recuerdo."),
                Visit("Enterrad juntos a los caídos que llevan días en la tierra de nadie", fallen, "Shanidar", "Eran hijos de alguien, de los dos lados."),
                Collect("Alguien ha traído un balón: llévalo a la portería de gorras", new List<Transform> { ball }, goal, "Momo", "¡Gol! … Bueno, ellos nos han metido más. Da igual."),
                Talk("Héspero trae una orden del mando", hespero.transform, "Héspero", "El alto mando lo prohíbe: mañana volvemos a disparar."),
            },
            "Flandes (Bélgica), Nochebuena de 1914, sobre el gimnasio del santuario a escala de hormiga. Entre las dos trincheras hay alambre, cráteres y caídos. Desde la trinchera alemana llega un villancico. Vive la tregua con el elenco renacido: británicos (gorra) y alemanes (casco de pincho). E para hablar y recoger.",
            new[]
            {
                "La Primera Guerra Mundial (1914–1918) mató a unos 9 millones de soldados y a millones de civiles. En el frente occidental, en diciembre de 1914, los ejércitos llevaban meses metidos en trincheras a veces a menos de 100 m unas de otras.",
                "En Nochebuena y Navidad de 1914, en muchos tramos del frente, sobre todo en Flandes, cerca de Ypres, los soldados dejaron de dispararse sin que nadie lo ordenara. Se calcula que participaron unos 100.000 soldados. Los alemanes pusieron árboles con velas en sus parapetos y cantaron; los británicos respondieron.",
                "Salieron a la tierra de nadie, se dieron la mano, intercambiaron comida, tabaco, botones y gorras, y enterraron juntos a sus muertos, en algunos sitios con una ceremonia común. Hay cartas que cuentan partidos de fútbol improvisados; el teniente sajón Johannes Niemann escribió que su lado ganó 3–2.",
                "Los mandos de ambos ejércitos lo prohibieron. En 1915 hubo pocos intentos y después ninguno: la guerra duró casi cuatro años más. La tregua demostró que el enemigo también era una persona.",
            },
            new[]
            {
                "Imperial War Museums, «The real story of the Christmas truce» (iwm.org.uk).",
                "Weintraub, S. (2001). Silent Night: The Story of the World War I Christmas Truce. Free Press.",
                "Brown, M. y Seaton, S. (1984). Christmas Truce. Leo Cooper.",
                "Football and the First World War, «The Story of the Christmas Truce 1914» (footballandthefirstworldwar.org).",
            },
            "Atlas", "Mañana me tocará apuntar a ese hombre. Hoy he jugado al fútbol con él.");
        return n;
    }

    static Transform Trench(Transform t, string name, Vector3 pos, Material mud, Material sand)
    {
        Transform tr = Spot(t, name, pos, name, 2.4f);
        AreaMicrocosmos.Box(tr, "Fondo", new Vector3(0f, 0.02f, 0f), new Vector3(34f, 0.04f, 3f), mud, false);
        float face = name.Contains("alemana") ? -2f : 2f;   // los sacos hacia la tierra de nadie
        AreaMicrocosmos.Box(tr, "Sacos terreros", new Vector3(0f, 0.6f, face), new Vector3(34f, 1.2f, 1f), sand, true);
        AreaMicrocosmos.Box(tr, "Talud", new Vector3(0f, 0.4f, -face), new Vector3(34f, 0.8f, 1f), mud, true);
        return tr;
    }

    // ══ FLORENCE NIGHTINGALE · Enfermería ══════════════════════════════════════════════════════════════════════════

    static int Nightingale(Transform t)
    {
        Material bed = AreaMicrocosmos.Mat("Hist_Catre", new Color(0.5f, 0.38f, 0.25f)), sheet = AreaMicrocosmos.Mat("Hist_Sabana", new Color(0.95f, 0.95f, 0.92f));
        Material dirt = AreaMicrocosmos.Mat("Hist_Suciedad", new Color(0.3f, 0.25f, 0.18f)), shut = AreaMicrocosmos.Mat("Hist_Postigo", new Color(0.35f, 0.25f, 0.18f)), wall = AreaMicrocosmos.Mat("Hist_Muro", new Color(0.85f, 0.8f, 0.68f));
        Color skin = new Color(0.42f, 0.28f, 0.2f), red = new Color(0.65f, 0.15f, 0.15f), dress = new Color(0.25f, 0.22f, 0.24f);

        Transform ward = Spot(t, "Sala de los heridos", new Vector3(0f, 0f, 10f), "Sala de los heridos (cuartel de Selimiye)", 3.5f);
        AreaMicrocosmos.Box(ward, "Muro", new Vector3(-12f, 1.6f, 0f), new Vector3(0.4f, 3.2f, 22f), wall, true);
        AreaMicrocosmos.Box(ward, "Muro", new Vector3(12f, 1.6f, 0f), new Vector3(0.4f, 3.2f, 22f), wall, true);
        var windows = new List<Transform>();
        for (int k = 0; k < 4; k++) { Transform w = AreaMicrocosmos.Box(ward, "Ventana cerrada", new Vector3(k < 2 ? -11.7f : 11.7f, 1.8f, -6f + (k % 2) * 12f), new Vector3(0.2f, 1.4f, 1.6f), shut, false); Label(w, "Ventana cerrada", 1f); windows.Add(w); }
        var patients = new List<Transform>();
        var dirtSpots = new List<Transform>();
        for (int k = 0; k < 6; k++)
        {
            float x = k < 3 ? -8f : 8f, z = -6f + (k % 3) * 6f;
            AreaMicrocosmos.Box(ward, "Catre", new Vector3(x, 0.3f, z), new Vector3(1.2f, 0.6f, 2.2f), bed, true);
            GameObject p = AreaMicrocosmos.Ant(ward, $"Herido {k + 1}", new Vector3(x, 0.6f, z), new Color(0.4f, 0.26f, 0.18f), 1.7f, 1f);
            Object.DestroyImmediate(p.GetComponent<UnityEngine.AI.NavMeshAgent>());
            p.transform.localRotation = Quaternion.Euler(0f, 0f, 80f); CubeInsect.Wear1940s(p, red, CubeInsect.Hat.None);
            patients.Add(p.transform);
            Transform d = AreaMicrocosmos.Box(ward, "Suciedad", new Vector3(x * 0.4f, 0.03f, z + 1.5f), new Vector3(1.4f, 0.05f, 1.2f), dirt, false); Label(d, "Suciedad", 0.6f); dirtSpots.Add(d);
        }
        Transform laundry = Spot(t, "Lavandería", new Vector3(-18f, 0f, -4f), "Lavandería", 2.5f);
        AreaMicrocosmos.Box(laundry, "Tina", new Vector3(0f, 0.5f, 0f), new Vector3(2f, 1f, 1.4f), AreaMicrocosmos.Mat("Hist_Tina", new Color(0.55f, 0.45f, 0.35f)), true);
        var sheets = new List<Transform>();
        for (int k = 0; k < 4; k++) sheets.Add(AreaMicrocosmos.Box(laundry, "Sábana limpia", new Vector3(-1.5f + k, 1.2f, 1.5f), new Vector3(0.8f, 0.15f, 0.6f), sheet, false));
        Transform kitchen = Spot(t, "Cocina de Soyer", new Vector3(18f, 0f, -4f), "Cocina de Soyer", 2.5f);
        AreaMicrocosmos.Box(kitchen, "Fogón", new Vector3(0f, 0.5f, 0f), new Vector3(2f, 1f, 1.2f), AreaMicrocosmos.Mat("Hist_Hierro", new Color(0.2f, 0.2f, 0.22f)), true);
        var broth = new List<Transform>();
        for (int k = 0; k < 3; k++) broth.Add(AreaMicrocosmos.Box(kitchen, "Caldo", new Vector3(-1f + k, 1.15f, 0f), new Vector3(0.35f, 0.3f, 0.35f), AreaMicrocosmos.Mat("Hist_Caldo", new Color(0.8f, 0.6f, 0.3f)), false));
        Transform desk = Spot(t, "Mesa de los datos", new Vector3(-6f, 0f, -8f), "Mesa de los datos", 2f);
        AreaMicrocosmos.Box(desk, "Mesa", new Vector3(0f, 0.45f, 0f), new Vector3(1.6f, 0.9f, 0.9f), bed, true);
        Transform rose = AreaMicrocosmos.Box(desk, "Diagrama de rosa", new Vector3(0f, 1.6f, 0.5f), new Vector3(1.2f, 1.2f, 0.05f), AreaMicrocosmos.Mat("Hist_Rosa", new Color(0.4f, 0.55f, 0.75f)), false);

        GameObject florence = Person(t, "Florence Nightingale", new Vector3(2f, 0f, -6f), skin, 1.75f);
        CubeInsect.Wear1940s(florence, dress, CubeInsect.Hat.Bonnet);
        int n = 1 + patients.Count;
        GameObject sutherland = Person(t, "John Sutherland", new Vector3(-12f, 0f, -9f), skin, 1.8f);
        CubeInsect.Wear1940s(sutherland, new Color(0.15f, 0.15f, 0.18f), CubeInsect.Hat.Brimmed); n++;
        foreach (var (name, pos) in new[] { ("Sakshi", new Vector3(-4f, 0f, 3f)), ("Ruth", new Vector3(4f, 0f, 3f)), ("Medea", new Vector3(0f, 0f, 17f)) })
        {
            GameObject nurse = Person(t, name, pos, skin, 1.75f); CubeInsect.Wear1940s(nurse, dress, CubeInsect.Hat.Bonnet);
            Routine(nurse, ward, laundry, kitchen); n++;
        }
        GameObject soyer = Person(t, "Alexis Soyer", new Vector3(16f, 0f, -6f), skin, 1.8f); CubeInsect.Wear1940s(soyer, Color.white, CubeInsect.Hat.Cap); Routine(soyer, kitchen); n++;

        StoryMission.StoryStep data = Talk("En la mesa de los datos, dibuja con Florence el diagrama de rosa", desk, "Florence", "Mira: casi todas las muertes eran evitables. Con este dibujo convenceremos al gobierno.");
        data.reveal = new[] { rose.gameObject };
        Mission(t, "Florence Nightingale en Escútari, 1854–1855", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("Preséntate a Florence Nightingale", florence.transform, "Florence", "Aquí mueren más soldados por la suciedad y las fiebres que por las heridas. Empecemos por limpiar."),
                Visit("Friega la suciedad del suelo de la sala", dirtSpots, "Sakshi", "Hacía semanas que nadie fregaba este suelo."),
                Visit("Abre las ventanas: que entre aire limpio", windows, "Florence", "La ventilación es lo primero que necesita un enfermo."),
                Collect("Trae sábanas limpias de la lavandería a la sala", sheets, ward, "Ruth", "Ropa limpia en cada cama."),
                Collect("Trae el caldo de la cocina de Soyer a la sala", broth, ward, "Alexis Soyer", "Comida caliente y bien hecha también cura."),
                Visit("Ronda de noche con la lámpara: atiende a cada herido", patients, "Herido 3", "Es la dama de la lámpara… Pasa cada noche."),
                Talk("Habla con John Sutherland, de la Comisión Sanitaria", sutherland.transform, "Sutherland", "Hemos limpiado las cloacas que pasaban bajo el hospital y el agua sucia. Ahora sí bajarán las muertes."),
                data,
            },
            "El hospital del cuartel de Selimiye, en Escútari (hoy Üsküdar, Estambul), durante la guerra de Crimea, sobre la enfermería del santuario a escala de hormiga. Ayuda a Florence Nightingale y a sus enfermeras (el elenco renacido) a cambiar el hospital: limpieza, aire, ropa, comida, cuidado de noche y datos. E para hablar, recoger y atender.",
            new[]
            {
                "Florence Nightingale llegó a Escútari el 4 de noviembre de 1854 con 38 enfermeras. En el hospital del cuartel morían muchos más soldados por tifus, cólera y disentería que por sus heridas.",
                "Organizó la limpieza, la lavandería, la comida (con el cocinero Alexis Soyer, que reformó las cocinas en 1855) y la ropa para los enfermos. Hacía la ronda de noche con una lámpara: el periódico The Times la llamó «la dama de la lámpara».",
                "En marzo de 1855 llegó la Comisión Sanitaria (John Sutherland y Robert Rawlinson), que limpió las cloacas y mejoró la ventilación y el agua. Entre febrero y junio de 1855 la mortalidad del hospital bajó de alrededor del 42 % a alrededor del 2 %. La propia Nightingale concluyó después que la causa principal había sido la falta de higiene.",
                "De vuelta en Inglaterra usó la estadística para convencer al gobierno: su diagrama de área polar («de rosa», 1858) mostraba que la mayoría de muertes eran evitables. Fue la primera mujer miembro de la Royal Statistical Society (1858), escribió «Notes on Nursing» (1859) y fundó en 1860 la escuela de enfermería del hospital St Thomas, en Londres.",
            },
            new[]
            {
                "National Army Museum, «Florence Nightingale: The Lady with the Lamp» (nam.ac.uk).",
                "Nightingale, F. (1858). Notes on Matters Affecting the Health, Efficiency and Hospital Administration of the British Army.",
                "McDonald, L. (ed.) (2010). Florence Nightingale: The Crimean War. Collected Works, vol. 14. Wilfrid Laurier Univ. Press.",
                "Karimi, H. y Masoudi Alavi, N. (2015). Florence Nightingale: The Mother of Nursing. Nurs Midwifery Stud. (PMC).",
            },
            "Florence", "Cuidar también es medir: lo que no se cuenta, no se arregla.");
        return n;
    }

    // ══ SIDDHĀRTHA Y LOS CUATRO ENCUENTROS · Enfermería → árbol de la meditación ═══════════════════════════════════

    static int Buddha(Transform t)
    {
        Material wall = AreaMicrocosmos.Mat("Hist_Palacio", new Color(0.9f, 0.82f, 0.65f)), bark = AreaMicrocosmos.Mat("Hist_Corteza", new Color(0.36f, 0.27f, 0.2f));
        Material leaf = AreaMicrocosmos.Mat("Hist_Hoja", new Color(0.25f, 0.45f, 0.22f)), water = AreaMicrocosmos.Mat("Hist_Agua", new Color(0.3f, 0.5f, 0.7f));
        Color skin = new Color(0.42f, 0.28f, 0.2f), royal = new Color(0.85f, 0.75f, 0.35f), saffron = new Color(0.9f, 0.55f, 0.15f), plain = new Color(0.85f, 0.8f, 0.7f);

        Transform palace = Spot(t, "Palacio de Kapilavastu", new Vector3(0f, 0f, -8f), "Palacio de Kapilavastu", 4f);
        AreaMicrocosmos.Box(palace, "Muralla", new Vector3(-7f, 1.5f, 3f), new Vector3(10f, 3f, 0.6f), wall, true);
        AreaMicrocosmos.Box(palace, "Muralla", new Vector3(7f, 1.5f, 3f), new Vector3(10f, 3f, 0.6f), wall, true);
        Transform gate = Spot(t, "Puerta del palacio", new Vector3(0f, 0f, -5f), "Puerta", 3.2f);
        Transform forest = Tree(t, "Bosque de los ascetas", new Vector3(16f, 0f, 18f), bark, leaf, 6f);
        Transform river = Spot(t, "Río Nairañjanā", new Vector3(4f, 0f, 22f), "Río Nairañjanā", 1.5f);
        AreaMicrocosmos.Box(river, "Río", new Vector3(0f, 0.02f, 2f), new Vector3(30f, 0.04f, 3f), water, false);
        Transform bodhi = Tree(t, "Árbol de la meditación (pipal)", new Vector3(-12f, 0f, 22f), bark, leaf, 8f);
        Transform bowl = AreaMicrocosmos.Box(t, "Cuenco de agua", new Vector3(-3f, 0.2f, 2f), new Vector3(0.4f, 0.25f, 0.4f), AreaMicrocosmos.Mat("Hist_Barro_cocido", new Color(0.7f, 0.4f, 0.25f)), false); Label(bowl, "Cuenco de agua", 0.6f);
        Transform rice = AreaMicrocosmos.Box(t, "Arroz con leche", new Vector3(6f, 0.2f, 16f), new Vector3(0.45f, 0.25f, 0.45f), AreaMicrocosmos.Mat("Hist_Arroz", new Color(0.95f, 0.92f, 0.8f)), false); Label(rice, "Arroz con leche de Sujātā", 0.6f);

        int n = 0;
        GameObject P(string name, Vector3 pos, Color robe, bool beard = false, float h = 1.8f, CubeInsect.Hat hat = CubeInsect.Hat.None)
        {
            GameObject g = Person(t, name, pos, skin, h); CubeInsect.WearGreek(g, robe, false, beard); if (hat != CubeInsect.Hat.None) CubeInsect.WearHat(g, hat); n++; return g;
        }
        GameObject prince = P("Siddhārtha", new Vector3(2f, 0f, -9f), royal, hat: CubeInsect.Hat.Turban);
        GameObject channa = P("Channa (el cochero)", new Vector3(-2f, 0f, -9f), plain, hat: CubeInsect.Hat.Turban);
        GameObject elder = P("Shanidar", new Vector3(-8f, 0f, 2f), plain, beard: true, h: 1.55f);
        elder.transform.localRotation = Quaternion.Euler(18f, 90f, 0f);   // encorvado
        GameObject sick = P("Ambrosio", new Vector3(-4f, 0f, 3f), plain);
        sick.transform.localRotation = Quaternion.Euler(0f, 0f, 75f);
        Object.DestroyImmediate(sick.GetComponent<UnityEngine.AI.NavMeshAgent>());
        Transform funeral = Spot(t, "Cortejo fúnebre", new Vector3(6f, 0f, 6f), "Cortejo fúnebre", 2.6f);
        AreaMicrocosmos.Box(funeral, "Andas", new Vector3(0f, 0.8f, 0f), new Vector3(0.8f, 0.2f, 2f), AreaMicrocosmos.Mat("Hist_Andas", new Color(0.95f, 0.95f, 0.92f)), false);
        foreach (var (name, x) in new[] { ("Ruth", -1.2f), ("Medea", 1.2f) }) { GameObject m = P(name, new Vector3(6f + x, 0f, 4.8f), plain); Routine(m, funeral); }
        GameObject ascetic = P("Héspero (asceta)", new Vector3(12f, 0f, 10f), saffron, beard: true);
        GameObject forestPrince = P("Siddhārtha (asceta)", new Vector3(14f, 0f, 16f), saffron);
        forestPrince.SetActive(false);
        GameObject sujata = P("Sakshi (Sujātā)", new Vector3(6f, 0f, 14f), new Color(0.75f, 0.3f, 0.4f));
        Routine(sujata, river, forest);
        GameObject atlas = P("Atlas", new Vector3(-6f, 0f, -6f), royal, hat: CubeInsect.Hat.Turban); Routine(atlas, palace, gate);
        GameObject momo = P("Momo", new Vector3(10f, 0f, -2f), plain, h: 1.7f); Routine(momo, gate, funeral, river);

        StoryMission.StoryStep leave = Reach("Sal con Siddhārtha y su cochero por la puerta del palacio", gate, "Channa", "Su padre no quería que viera nada triste. Hoy lo verá.");
        StoryMission.StoryStep renounce = Talk("4 · Un asceta tranquilo: habla con Héspero", ascetic.transform, "Siddhārtha", "Él ha visto lo mismo que yo y busca una salida. Me voy con los ascetas.");
        renounce.hide = new[] { prince };
        renounce.reveal = new[] { forestPrince };
        Mission(t, "Siddhārtha y los cuatro encuentros", StoryMission.Marga.Yoga, 60f,
            new[]
            {
                Talk("Habla con el príncipe Siddhārtha en su palacio", prince.transform, "Siddhārtha", "Tengo de todo aquí dentro, pero nunca he visto qué hay fuera."),
                leave,
                Talk("1 · Un anciano: habla con Shanidar", elder.transform, "Channa", "Es la vejez, príncipe. Nos llega a todos."),
                Collect("2 · Un enfermo: lleva el cuenco de agua a Ambrosio", new List<Transform> { bowl }, sick.transform, "Ambrosio", "Gracias… la fiebre no me deja ni levantarme."),
                Reach("3 · Un muerto: acompaña al cortejo fúnebre", funeral, "Channa", "Es la muerte. Tampoco nadie se libra."),
                renounce,
                Hold("En el bosque, Siddhārtha ayuna hasta casi morir: quédate con él", forest, 10f, "Siddhārtha", "Así no: torturar el cuerpo no aquieta la mente."),
                Collect("Lleva a Siddhārtha el arroz con leche que ofrece Sujātā", new List<Transform> { rice }, forestPrince.transform, "Siddhārtha", "Ni el lujo del palacio ni el hambre del bosque: un camino del medio."),
                Hold("Medita bajo el árbol de la meditación", bodhi, 15f, "Siddhārtha", "He visto cómo nace el sufrimiento y cómo cesa."),
            },
            "El norte de la India antigua, sobre la enfermería del santuario a escala de hormiga: lo que se ve en una enfermería —vejez, enfermedad, muerte— es lo que cambió a Siddhārtha. Acompaña al príncipe fuera del palacio, a los cuatro encuentros, al bosque de los ascetas y al árbol donde medita. E para hablar y recoger; en la meditación, quédate quieto.",
            new[]
            {
                "Siddhārtha Gautama nació en Lumbinī (hoy Nepal), del clan de los Śākya, cuya capital era Kapilavastu. Vivió en el norte de la India hacia los siglos VI–V a. C.; las fechas exactas se discuten.",
                "Los cuatro encuentros (un anciano, un enfermo, un muerto y un asceta) se cuentan en biografías posteriores, como el «Buddhacarita» de Aśvaghoṣa (s. I–II d. C.); los textos más antiguos los narran del buda mítico Vipassī (Mahāpadāna Sutta, DN 14). Es la tradición, no un relato comprobado.",
                "Según los textos antiguos (Majjhima Nikāya 36), practicó durante años un ascetismo extremo y casi muere de hambre. Al aceptar comida —la tradición dice que un arroz con leche de una joven, Sujātā— eligió el «camino del medio» entre el lujo y la mortificación (Saṃyutta Nikāya 56.11).",
                "Meditó bajo una higuera pipal (Ficus religiosa) en Bodh Gayā hasta lo que la tradición llama el despertar; desde entonces se le llama Buda, «el que ha despertado». Su primera enseñanza, en Sarnath, fueron las cuatro nobles verdades: el sufrimiento, su origen, su cese y el camino que lleva a él.",
            },
            new[]
            {
                "Aśvaghoṣa, Buddhacarita, cantos III–XIV (trad. P. Olivelle, 2008, Clay Sanskrit Library).",
                "Majjhima Nikāya 36, Mahāsaccaka Sutta; Saṃyutta Nikāya 56.11 (trad. Bhikkhu Bodhi, Wisdom Publications).",
                "Gethin, R. (1998). The Foundations of Buddhism. Oxford University Press.",
            },
            "Siddhārtha", "No creas algo porque yo lo diga: compruébalo tú.");
        return n;
    }

    // ══ LOS HERMANOS WRIGHT · Mecánica ═════════════════════════════════════════════════════════════════════════════

    static int Wright(Transform t)
    {
        Material wood = AreaMicrocosmos.Mat("Hist_Madera", new Color(0.55f, 0.4f, 0.25f)), metal = AreaMicrocosmos.Mat("Hist_Metal", new Color(0.4f, 0.4f, 0.42f));
        Material canvas = AreaMicrocosmos.Mat("Hist_Lona", new Color(0.92f, 0.88f, 0.78f)), dune = AreaMicrocosmos.Mat("Hist_Duna", new Color(0.85f, 0.78f, 0.6f));
        Color skin = new Color(0.42f, 0.28f, 0.2f), suit = new Color(0.25f, 0.24f, 0.26f), apron = new Color(0.6f, 0.55f, 0.45f);

        Transform shop = Spot(t, "Wright Cycle Company", new Vector3(0f, 0f, 0f), "Wright Cycle Company (Dayton, Ohio)", 3.5f);
        AreaMicrocosmos.Box(shop, "Banco", new Vector3(0f, 0.45f, 4f), new Vector3(6f, 0.9f, 1.2f), wood, true);
        var bikes = new List<Transform>();
        foreach (var (label, x) in new[] { ("Bici: cojinete que chirría (engrasar)", -6f), ("Bici: cadena floja (tensar)", 0f), ("Bici: rueda pinchada (parchear)", 6f) })
        {
            Transform b = new GameObject("Bicicleta").transform; b.SetParent(shop, false); b.localPosition = new Vector3(x, 0f, -2f);
            foreach (float z in new[] { -0.6f, 0.6f }) AreaMicrocosmos.Box(b, "Rueda", new Vector3(0f, 0.4f, z), new Vector3(0.08f, 0.8f, 0.8f), metal, false);
            AreaMicrocosmos.Box(b, "Cuadro", new Vector3(0f, 0.65f, 0f), new Vector3(0.08f, 0.08f, 1.2f), metal, false);
            AreaMicrocosmos.Box(b, "Sillín", new Vector3(0f, 0.95f, -0.3f), new Vector3(0.2f, 0.06f, 0.3f), wood, false);
            Label(b, label, 1.4f); bikes.Add(b);
        }
        Transform box = AreaMicrocosmos.Box(shop, "Caja de cámara", new Vector3(2f, 1.05f, 4f), new Vector3(0.6f, 0.2f, 0.3f), AreaMicrocosmos.Mat("Hist_Carton", new Color(0.75f, 0.6f, 0.4f)), false);
        Label(box, "Caja de una cámara de bici", 0.5f);
        Transform tunnel = Spot(t, "Túnel de viento", new Vector3(-14f, 0f, 8f), "Túnel de viento (1901)", 2.4f);
        AreaMicrocosmos.Box(tunnel, "Túnel", new Vector3(0f, 0.8f, 0f), new Vector3(0.8f, 0.8f, 5f), wood, true);
        var profiles = new List<Transform>();
        for (int k = 0; k < 4; k++) { Transform p = AreaMicrocosmos.Box(tunnel, "Perfil de ala", new Vector3(1.6f, 0.6f, -1.5f + k), new Vector3(0.5f, 0.05f, 0.3f), metal, false); Label(p, $"Perfil de ala {k + 1}", 0.5f); profiles.Add(p); }
        Transform bench = Spot(t, "Banco del Flyer", new Vector3(12f, 0f, 8f), "Banco del Flyer", 2f);
        var parts = new List<Transform>();
        foreach (var (name, pos, size) in new[] { ("Ala superior", new Vector3(-12f, 0.2f, 14f), new Vector3(3f, 0.1f, 0.8f)), ("Ala inferior", new Vector3(-6f, 0.2f, 16f), new Vector3(3f, 0.1f, 0.8f)), ("Hélices", new Vector3(4f, 0.2f, 14f), new Vector3(1.2f, 0.1f, 0.2f)), ("Motor de Charlie Taylor", new Vector3(-2f, 0.3f, 4f), new Vector3(0.6f, 0.5f, 0.6f)) })
        { Transform p = AreaMicrocosmos.Box(t, name, pos, size, name.StartsWith("Ala") ? canvas : metal, false); Label(p, name, 0.6f); parts.Add(p); }
        Transform hill = Spot(t, "Kill Devil Hills", new Vector3(0f, 0f, 22f), "Kill Devil Hills (Kitty Hawk, 17-12-1903)", 3f);
        AreaMicrocosmos.Box(hill, "Duna", new Vector3(0f, 0.4f, 3f), new Vector3(20f, 0.8f, 6f), dune, true);
        AreaMicrocosmos.Box(hill, "Raíl de lanzamiento", new Vector3(0f, 0.05f, -1f), new Vector3(0.2f, 0.1f, 18f), wood, false);
        // El Flyer (12,3 m de envergadura en la realidad; aquí, a escala de hormiga): en el raíl al montarlo, en el aire al volar.
        GameObject Flyer(string name, Vector3 at)
        {
            GameObject f = new GameObject(name); f.transform.SetParent(hill, false); f.transform.localPosition = at;
            foreach (float y in new[] { 0f, 0.9f }) AreaMicrocosmos.Box(f.transform, "Ala", new Vector3(0f, y, 0f), new Vector3(6f, 0.06f, 1f), canvas, false);
            AreaMicrocosmos.Box(f.transform, "Timón", new Vector3(0f, 0.45f, -2.2f), new Vector3(0.05f, 0.9f, 0.5f), canvas, false);
            AreaMicrocosmos.Box(f.transform, "Elevador", new Vector3(0f, 0.3f, 2f), new Vector3(2f, 0.05f, 0.5f), canvas, false);
            return f;
        }
        GameObject onRail = Flyer("Wright Flyer (en el raíl)", new Vector3(0f, 0.6f, -6f)), flying = Flyer("Wright Flyer (volando)", new Vector3(0f, 3f, 4f));

        int n = 0;
        GameObject P(string name, Vector3 pos, Color coat, CubeInsect.Hat hat, float h = 1.8f) { GameObject g = Person(t, name, pos, skin, h); CubeInsect.Wear1940s(g, coat, hat); n++; return g; }
        GameObject wilbur = P("Wilbur Wright", new Vector3(-2f, 0f, 2f), suit, CubeInsect.Hat.Cap);
        GameObject orville = P("Orville Wright", new Vector3(3f, 0f, 3f), suit, CubeInsect.Hat.Brimmed);
        GameObject taylor = P("Charlie Taylor (Atlas)", new Vector3(-3f, 0f, 5f), apron, CubeInsect.Hat.Cap); Routine(taylor, shop, bench);
        foreach (var (name, pos) in new[] { ("Ruth", new Vector3(-8f, 0f, -6f)), ("Momo", new Vector3(8f, 0f, -6f)), ("Medea", new Vector3(10f, 0f, -2f)), ("Ambrosio", new Vector3(-10f, 0f, -2f)) })
        { GameObject c = P(name, pos, new Color(0.5f, 0.35f, 0.3f), CubeInsect.Hat.Brimmed); Routine(c, shop, tunnel, hill); }
        GameObject sakshi = P("Sakshi", new Vector3(6f, 0f, 20f), new Color(0.35f, 0.4f, 0.5f), CubeInsect.Hat.None); Routine(sakshi, hill);

        StoryMission.StoryStep assemble = Collect("Lleva las piezas del Flyer a su banco", parts, bench, "Charlie Taylor", "Un motor de aluminio de 12 caballos, hecho en seis semanas. Ligero como pedían.");
        assemble.reveal = new[] { onRail };
        StoryMission.StoryStep flight = Hold("Mira el primer vuelo junto al raíl", hill, 12f, "Wilbur", "¡Doce segundos en el aire! Es poco… y lo cambia todo.");
        flight.hide = new[] { onRail };
        flight.reveal = new[] { flying };
        Mission(t, "Los hermanos Wright: de la bicicleta al avión", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("Habla con Wilbur en el taller de bicicletas", wilbur.transform, "Wilbur", "Arreglamos bicis para vivir. Con lo que sacamos, intentamos volar."),
                Visit("Repara las tres bicis del pueblo", bikes, "Orville", "Grasa contra el rozamiento, cadena tensa para transmitir la fuerza, aire en la rueda: máquinas simples."),
                Talk("Mira cómo Wilbur retuerce la caja de una cámara de bici", wilbur.transform, "Wilbur", "Si retuerzo las alas igual que esta caja, el avión se inclinará como un pájaro… o como una bici al girar."),
                Visit("Prueba en el túnel de viento los perfiles de ala", profiles, "Orville", "Las tablas de los demás estaban mal. Medimos nosotros: unos 200 perfiles."),
                assemble,
                Reach("Ve a las dunas de Kill Devil Hills", hill, "Orville", "Viento fuerte y arena blanda: buen sitio para caer."),
                flight,
            },
            "Dayton (Ohio), 1892–1903, sobre la mecánica del santuario a escala de hormiga. Wilbur y Orville Wright reparan bicicletas y, con lo que aprenden, construyen el primer avión. Repara bicis, ayuda en el túnel de viento, monta el Flyer y míralo volar. E para hablar, reparar y recoger.",
            new[]
            {
                "La bicicleta es mecánica a la vista: Karl Drais presentó en 1817 su «Laufmaschine», sin pedales (la idea de que nació porque murieron muchos caballos tras el «año sin verano» de 1816 es una hipótesis del historiador H.-E. Lessing); John Kemp Starley hizo en 1885 la «Rover», con cadena; John Boyd Dunlop, en 1888, el neumático de aire.",
                "Los hermanos Wilbur y Orville Wright abrieron una tienda de bicicletas en Dayton en 1892 y desde 1896 fabricaron las suyas. El taller les pagaba los experimentos y les enseñó lo que hacía falta: estructuras ligeras, cadenas y transmisión, equilibrio.",
                "Wilbur ideó en 1899 el «alabeo» de las alas al retorcer una caja de cartón de una cámara de bicicleta. En 1901 construyeron un túnel de viento y midieron unos 200 perfiles de ala, porque los datos publicados eran erróneos. Su mecánico Charlie Taylor les construyó un motor de aluminio de unos 12 caballos.",
                "El 17 de diciembre de 1903, en Kill Devil Hills (cerca de Kitty Hawk, Carolina del Norte), Orville voló 12 segundos y unos 36 m: el primer vuelo controlado y sostenido de un avión a motor. Ese día volaron cuatro veces; el último vuelo, de Wilbur, duró 59 segundos y recorrió unos 260 m.",
            },
            new[]
            {
                "Smithsonian National Air and Space Museum, «The Wright Brothers & The Invention of the Aerial Age» (airandspace.si.edu).",
                "National Park Service, Wright Brothers National Memorial (nps.gov/wrbr).",
                "Library of Congress, The Wilbur and Orville Wright Papers.",
                "Lessing, H.-E. (2003). Automobilität – Karl Drais und die unglaublichen Anfänge. Maxime.",
            },
            "Orville", "Nadie nos dio permiso para volar: lo aprendimos arreglando bicis.");
        return n;
    }

    // ══ ERATÓSTENES MIDE LA TIERRA · Planta Atómica (primer tramo de la escalera de la física) ══════════════════════

    static int Eratosthenes(Transform t)
    {
        Material stone = AreaMicrocosmos.Mat("Hist_Piedra", new Color(0.88f, 0.84f, 0.74f)), shadow = AreaMicrocosmos.Mat("Hist_Sombra", new Color(0.15f, 0.13f, 0.12f));
        Material papyrus = AreaMicrocosmos.Mat("Hist_Papiro", new Color(0.9f, 0.82f, 0.6f)), water = AreaMicrocosmos.Mat("Hist_Agua", new Color(0.3f, 0.5f, 0.7f)), sun = AreaMicrocosmos.Mat("Hist_Sol", new Color(1f, 0.9f, 0.5f));
        Color skin = new Color(0.42f, 0.28f, 0.2f), white = new Color(0.95f, 0.93f, 0.88f);

        Transform library = Spot(t, "Biblioteca de Alejandría", new Vector3(0f, 0f, -4f), "Biblioteca de Alejandría", 4f);
        for (int k = 0; k < 3; k++) AreaMicrocosmos.Box(library, "Estante de rollos", new Vector3(-4f + k * 4f, 1f, 3f), new Vector3(3f, 2f, 0.6f), papyrus, true);
        AreaMicrocosmos.Box(library, "Mesa", new Vector3(0f, 0.45f, 0f), new Vector3(1.6f, 0.9f, 0.9f), stone, true);
        // Siena (Asuán) al sur, Alejandría al norte: 5000 estadios entre las dos. Aquí, ~40 m.
        Transform well = Spot(t, "Pozo de Siena", new Vector3(-16f, 0f, 22f), "Pozo de Siena (Asuán)", 2.4f);
        AreaMicrocosmos.Box(well, "Brocal", new Vector3(0f, 0.5f, 0f), new Vector3(1.8f, 1f, 1.8f), stone, true);
        AreaMicrocosmos.Box(well, "Reflejo del Sol", new Vector3(0f, 0.95f, 0f), new Vector3(1.2f, 0.05f, 1.2f), sun, false);
        AreaMicrocosmos.Box(well, "Palo sin sombra", new Vector3(2f, 1f, 0f), new Vector3(0.1f, 2f, 0.1f), stone, true);
        Transform gnomon = Spot(t, "Gnomon de Alejandría", new Vector3(14f, 0f, 2f), "Gnomon de Alejandría", 3.4f);
        AreaMicrocosmos.Box(gnomon, "Gnomon", new Vector3(0f, 1.5f, 0f), new Vector3(0.12f, 3f, 0.12f), stone, true);
        // La sombra de un palo de 3 m con el Sol a 7,2° del cenit mide 3 · tan 7,2° ≈ 0,38 m, hacia el norte.
        AreaMicrocosmos.Box(gnomon, "Sombra (7,2°)", new Vector3(0f, 0.02f, 0.19f), new Vector3(0.12f, 0.02f, 0.38f), shadow, false);
        AreaMicrocosmos.Box(gnomon, "Escafe", new Vector3(-2f, 0.4f, 0f), new Vector3(1f, 0.8f, 1f), stone, true);
        for (int k = 0; k < 8; k++) AreaMicrocosmos.Box(t, "Mojón del camino", new Vector3(-16f + k * 4f, 0.3f, 22f - k * 2.5f), new Vector3(0.3f, 0.6f, 0.3f), stone, true);

        int n = 0;
        GameObject P(string name, Vector3 pos, Color robe, bool beard = false, float h = 1.8f) { GameObject g = Person(t, name, pos, skin, h); CubeInsect.WearGreek(g, robe, false, beard); n++; return g; }
        GameObject era = P("Eratóstenes", new Vector3(1.5f, 0f, -5f), white, beard: true);
        GameObject bematist = P("Atlas (bematista)", new Vector3(-12f, 0f, 18f), new Color(0.75f, 0.6f, 0.4f));
        Routine(bematist, well, gnomon);
        foreach (var (name, pos) in new[] { ("Sakshi", new Vector3(-4f, 0f, -2f)), ("Héspero", new Vector3(4f, 0f, -1f)), ("Medea", new Vector3(10f, 0f, 4f)), ("Momo", new Vector3(-14f, 0f, 20f)), ("Ruth", new Vector3(-18f, 0f, 24f)) })
        { GameObject g = P(name, pos, new Color(0.7f, 0.65f, 0.8f)); Routine(g, library, gnomon, well); }

        Mission(t, "Eratóstenes mide la Tierra, ~240 a. C.", StoryMission.Marga.Stats, 60f,
            new[]
            {
                Talk("Habla con Eratóstenes en la biblioteca", era.transform, "Eratóstenes", "Dicen que en Siena, al mediodía del día más largo, el Sol ilumina el fondo de los pozos. Aquí, en Alejandría, los palos sí dan sombra. ¿Por qué?"),
                Reach("Viaja al sur, al pozo de Siena", well, "Momo", "¡Mira dentro! El Sol se ve en el agua del fondo."),
                Hold("Mediodía del solsticio en Siena: mira el palo, sin sombra", well, 8f, "Eratóstenes", "El Sol está justo encima de Siena: sus rayos caen rectos."),
                Talk("Pregunta al bematista Atlas cuánto hay hasta Alejandría", bematist.transform, "Atlas", "Lo he medido a pasos: 5000 estadios."),
                Reach("Vuelve al norte, al gnomon de Alejandría", gnomon, "Medea", "Hoy es el mismo día, y este palo sí da sombra."),
                Hold("Mide el ángulo de la sombra del gnomon", gnomon, 8f, "Eratóstenes", "La sombra marca 7,2°: una cincuentava parte del círculo."),
                Talk("Haz la cuenta con Eratóstenes", era.transform, "Eratóstenes", "Si 5000 estadios son 1/50 de la vuelta, la Tierra entera mide 50 × 5000 = 250 000 estadios."),
            },
            "Alejandría y Siena (Egipto), hacia el 240 a. C., sobre la planta atómica del santuario a escala de hormiga: aquí empieza la escalera de la física. Eratóstenes, bibliotecario de Alejandría, quiere saber cuánto mide la Tierra con un palo, un pozo y la sombra. Viaja con él entre las dos ciudades. E para hablar; para medir, quédate quieto.",
            new[]
            {
                "Eratóstenes de Cirene (c. 276–194 a. C.) dirigió la Biblioteca de Alejandría. Su medida la conocemos sobre todo por Cleomedes («Caelestia» I, 7), que la contó siglos después.",
                "Supo que en Siena (hoy Asuán), al mediodía del solsticio de verano, el Sol iluminaba el fondo de los pozos: estaba en el cenit. El mismo día, en Alejandría, un gnomon daba una sombra que marcaba 1/50 de círculo (7,2°). Como el Sol está tan lejos que sus rayos llegan casi paralelos, ese ángulo es también el que separa las dos ciudades medido desde el centro de la Tierra.",
                "Con 5000 estadios entre Siena y Alejandría, la circunferencia sale 50 × 5000 = 250 000 estadios (luego usó 252 000). No sabemos con seguridad cuánto medía su estadio: según el valor, el resultado queda entre unos 39 000 y 46 000 km. La circunferencia real es de unos 40 000 km.",
                "Es geometría y observación: dos sombras, una distancia y una regla de tres. La misma manera de pensar llevó, siglos después, a Galileo, a Newton y al átomo.",
            },
            new[]
            {
                "Cleomedes, Caelestia I, 7 (trad. A. C. Bowen y R. B. Todd, 2004, Univ. of California Press).",
                "Wikipedia, «Earth's circumference» (historia de la medida de Eratóstenes).",
                "Observatoire de Paris, An Etymological Dictionary of Astronomy and Astrophysics, «Eratosthenes' method».",
            },
            "Eratóstenes", "No hace falta dar la vuelta al mundo para medirlo: basta con mirar bien una sombra.");
        return n;
    }

    // ══ Piezas comunes ═════════════════════════════════════════════════════════════════════════════════════════════

    static void Mission(Transform t, string title, StoryMission.Marga marga, float xp, StoryMission.StoryStep[] steps, string intro, string[] history, string[] sources, string finalSpeaker, string finalLine)
    {
        var card = t.gameObject.AddComponent<HistoryCard>();
        card.title = title; card.intro = intro; card.history = history; card.sources = sources;
        var m = t.gameObject.AddComponent<StoryMission>();
        m.title = title; m.marga = marga; m.xp = xp; m.steps = steps;
        m.finalSpeaker = finalSpeaker; m.finalLine = finalLine;
    }

    static StoryMission.StoryStep Make(string goal, StoryMission.Kind kind, Transform[] targets, string speaker, string line) =>
        new StoryMission.StoryStep { goal = goal, kind = kind, targets = targets, speaker = speaker, line = line };
    static StoryMission.StoryStep Talk(string goal, Transform who, string speaker, string line) => Make(goal, StoryMission.Kind.Talk, new[] { who }, speaker, line);
    static StoryMission.StoryStep Reach(string goal, Transform where, string speaker, string line) => Make(goal, StoryMission.Kind.Reach, new[] { where }, speaker, line);
    static StoryMission.StoryStep Visit(string goal, List<Transform> each, string speaker, string line) => Make(goal, StoryMission.Kind.VisitAll, each.ToArray(), speaker, line);
    static StoryMission.StoryStep Hold(string goal, Transform where, float seconds, string speaker, string line)
    { var s = Make(goal, StoryMission.Kind.Hold, new[] { where }, speaker, line); s.seconds = seconds; return s; }
    static StoryMission.StoryStep Collect(string goal, List<Transform> items, Transform to, string speaker, string line)
    { var s = Make(goal, StoryMission.Kind.Collect, items.ToArray(), speaker, line); s.deliverTo = to; return s; }

    static GameObject Person(Transform t, string name, Vector3 pos, Color c, float height, float bulk = 1f) => AreaMicrocosmos.Ant(t, name, pos, c, height, bulk);

    // La jornada del mirmidón: va de un puesto a otro de la historia (así el pueblo no está quieto).
    static void Routine(GameObject g, params Transform[] stops) { var r = g.AddComponent<MicroRoutine>(); r.stops = stops; }

    static Transform Tree(Transform t, string name, Vector3 pos, Material bark, Material leaf, float h)
    {
        Transform s = Spot(t, name, pos, name, h + 2.5f);
        AreaMicrocosmos.Box(s, "Tronco", new Vector3(0f, h * 0.5f, 0f), new Vector3(0.9f, h, 0.9f), bark, true);
        for (int k = 0; k < 5; k++) AreaMicrocosmos.Box(s, "Copa", new Vector3(Mathf.Cos(k * 1.26f) * 1.8f, h + (k % 2) * 0.8f, Mathf.Sin(k * 1.26f) * 1.8f), new Vector3(3f, 1.8f, 3f), leaf, false);
        return s;
    }

    static Transform Spot(Transform parent, string name, Vector3 local, string label = null, float h = 3f)
    {
        Transform s = new GameObject(name).transform; s.SetParent(parent, false); s.localPosition = local;
        if (label != null) Label(s, label, h);
        return s;
    }

    static void Label(Transform t, string text, float height)
    {
        WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = text; wl.height = height;
    }
}
