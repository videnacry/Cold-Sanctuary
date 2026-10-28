using UnityEngine;

/// <summary>
/// ÁREAS DE ENTRENAMIENTO (docs/jornada-y-rotacion.md): las 2 h diarias de entrenamiento son virtualizaciones de estas
/// áreas. El EDIFICIO DEL GIMNASIO (docs/entrenamiento-areas.md §3) tiene 3 pisos de 36 × 26 m: máquinas de placas y peso
/// libre (piso 0), cardio y zona libre (piso 1), y calistenia, parkour y crossfit (piso 2). Cada equipo deja 2 m libres
/// hasta el de al lado y lleva su <see cref="GymStation"/> con las posturas A/B del doc. La sala de BALLET y el DOJO de
/// artes marciales siguen EN OBRAS: cada uno tiene a pie de obra el material de un día de trabajo, andamio y paredes a
/// medio levantar (<see cref="ConstructionProject"/>), y el equipo de Construcción los va terminando.
/// </summary>
public static partial class SampleSceneBuilder
{
    static readonly Color TRubber = new Color(0.18f, 0.18f, 0.2f), TIron = new Color(0.35f, 0.36f, 0.4f), TChrome = new Color(0.78f, 0.8f, 0.84f),
        TRed = new Color(0.75f, 0.2f, 0.2f), TWood = new Color(0.7f, 0.52f, 0.34f);
    static readonly Color TPad = new Color(0.12f, 0.12f, 0.14f), TDisc = new Color(0.08f, 0.08f, 0.09f), TAcc = new Color(0.85f, 0.65f, 0.15f),
        TTurf = new Color(0.25f, 0.5f, 0.27f), TTile = new Color(0.1f, 0.11f, 0.12f), TMat = new Color(0.3f, 0.45f, 0.4f),
        TBumper = new Color(0.15f, 0.3f, 0.6f), TPlatWood = new Color(0.8f, 0.64f, 0.42f);

    /// <summary>Centro (local del piso, sobre la losa) y giro de la ZONA DE ALFOMBRAS de 7,6 × 4,4 m de cada piso del gimnasio
    /// (docs/entrenamiento-areas.md §3.2–3.4). Los Furnish* la dejan libre, con ≥ 2 m hasta el equipo más cercano. En el
    /// piso 2 va girada 90° (4,4 en X × 7,6 en Z).</summary>
    static Vector3 GymCarpetSpot(int floor, out float yaw)
    {
        yaw = floor == 2 ? 90f : 0f;
        return floor == 0 ? new Vector3(-8f, 0f, -9.8f) : floor == 1 ? new Vector3(-8f, 0f, 9.8f) : new Vector3(-1.2f, 0f, -8.2f);
    }

    /// <summary>Centro de la huella (x, z) de cada tipo de <see cref="GymMachine"/> respecto a su origen: el doc da centros de
    /// huella, así que al colocarlas se resta esto.</summary>
    static readonly Vector2[] GymMachineCenter =
    {
        new Vector2(0f, 0.135f), new Vector2(0f, 0.19f), new Vector2(0f, -0.215f), new Vector2(0f, -0.22f), new Vector2(0f, -0.16f),
        new Vector2(0.2f, 0.075f), new Vector2(-0.18f, 0.095f), new Vector2(0f, -0.15f), new Vector2(0f, -0.21f), new Vector2(0.15f, -0.05f)
    };

    /// <summary>PISO 0 DEL GIMNASIO (docs/entrenamiento-areas.md §3.2), 36 × 26 m: las 30 máquinas de placas (10 tipos × 3) en
    /// 5 filas mirando hacia −Z, el pasillo central (x de 2 a 4,2) y el bloque de PESO LIBRE: 2 estantes de mancuernas, 4 bancos
    /// regulables, 3 jaulas de sentadillas, 2 press de banca planos y 1 inclinado, Smith, plataforma de peso muerto, árboles de
    /// discos y soporte de barras. 2 m libres entre huellas. Sin entreplanta: se cambia de piso en alfombra (la zona de
    /// alfombras, en <see cref="GymCarpetSpot"/>, queda libre).</summary>
    static void FurnishGym(Transform area, Vector2 size)
    {
        Transform r = new GameObject("Gimnasio").transform; r.SetParent(area, false); r.localPosition = new Vector3(0f, 0.1f, 0f);
        KitchenProps.C(r, new Vector3(0f, 0.01f, 0f), new Vector3(size.x - 0.4f, 0.02f, size.y - 0.4f), TRubber, 0f, "Suelo_caucho");
        // MÁQUINAS DE PLACAS (x de −17 a 2, z de −4,5 a 12): 5 filas de 6, dos tipos por fila. El orden alterna tren superior e
        // inferior, como el circuito de gimnasio-maquinas.md §13, y las 10 estaciones están en este piso.
        string[] types = { "Press de pecho", "Press de hombros", "Jalón al pecho (espalda)", "Remo sentado", "Prensa de piernas",
                           "Extensión de cuádriceps", "Curl femoral", "Hip thrust (glúteos)", "Gemelos", "Curl de bíceps y tríceps" };
        float[] rowZ = { 11.3f, 7.9f, 4.05f, -0.1f, -3.8f };
        int[,] rowTypes = { { 0, 1 }, { 2, 3 }, { 4, 8 }, { 5, 6 }, { 7, 9 } };
        float[][] rowX =
        {
            new[] { -16.5f, -13.5f, -10.5f, -7.5f, -4.5f, -1.5f },
            new[] { -16.4f, -13.2f, -10.0f, -6.9f, -3.9f, -0.9f },
            new[] { -16.1f, -12.3f, -8.5f, -5.0f, -1.8f, 1.4f },
            new[] { -16.35f, -13.05f, -9.75f, -6.5f, -3.3f, -0.1f },
            new[] { -16.25f, -12.75f, -9.25f, -6.0f, -3.0f, 0f },
        };
        for (int row = 0; row < 5; row++)
            for (int j = 0; j < 6; j++)
            {
                int t = rowTypes[row, j / 3], c = j % 3;
                Vector2 o = GymMachineCenter[t];
                GymMachine(r, $"{types[t]} {c + 1}", t, c, new Vector3(rowX[row][j] - o.x, 0f, rowZ[row] - o.y), j == 0 ? 1f : -1f);   // la primera columna, junto al borde: se llega por su derecha
            }
        WorldLabel ml = KitchenProps.Root(r, "Zona_maquinas", new Vector3(-7.5f, 0f, 4f)).gameObject.AddComponent<WorldLabel>(); ml.text = "Máquinas de placas"; ml.height = 3.2f;

        // PESO LIBRE (x de 4 a 17): columnas en x = 5,3 · 9,5 · 13,7.
        GDumbbellRack(r, "Estante_mancuernas_1", new Vector3(6f, 0f, 11.6f));
        GDumbbellRack(r, "Estante_mancuernas_2", new Vector3(12.5f, 0f, 11.6f));
        float[] fid = { 4.5f, 7.6f, 10.7f, 13.8f };
        for (int i = 0; i < 4; i++) GymFID(r, $"Banco regulable {i + 1}", i, new Vector3(fid[i], 0f, 7.5f));
        float[] col = { 5.3f, 9.5f, 13.7f };
        for (int i = 0; i < 3; i++) GymRack(r, $"Jaula de sentadillas {i + 1}", i, new Vector3(col[i], 0f, 2.5f));
        GymBench(r, "Press de banca plano 1", 0, new Vector3(5.3f, 0f, -2.5f), false);
        GymBench(r, "Press de banca plano 2", 1, new Vector3(9.5f, 0f, -2.5f), false);
        GymBench(r, "Press de banca inclinado 1", 0, new Vector3(13.7f, 0f, -2.5f), true);
        GymSmith(r, "Máquina Smith 1", 0, new Vector3(6f, 0f, -8f));
        GymPlatform(r, "Plataforma de peso muerto 1", GymMachineType.PesoMuerto, 0, new Vector3(12.5f, 0f, -8f), TDisc);
        GPlateTree(r, new Vector3(17.1f, 0f, 2.5f)); GPlateTree(r, new Vector3(17.1f, 0f, -2.5f));
        GBarHolder(r, new Vector3(17.1f, 0f, -8f), 6);
        WorldLabel fl = KitchenProps.Root(r, "Zona_peso_libre", new Vector3(10.5f, 0f, 0f)).gameObject.AddComponent<WorldLabel>(); fl.text = "Peso libre"; fl.height = 3.2f;

        // FRANJA DELANTERA: fuente de agua y toallas, a los lados de la zona de alfombras (x de −11,8 a −4,2).
        Transform fu = KitchenProps.Root(r, "Fuente_de_agua", new Vector3(-15.4f, 0f, -10.5f));
        KitchenProps.C(fu, new Vector3(0f, 0.5f, 0f), new Vector3(0.5f, 1f, 0.4f), TChrome, 0f, "Fuente");
        KitchenProps.C(fu, new Vector3(0f, 1.02f, -0.05f), new Vector3(0.4f, 0.04f, 0.25f), TIron, 0f, "Pila");
        KitchenProps.Fit(fu); fu.gameObject.AddComponent<WorldLabel>().text = "Fuente de agua";
        Transform tw = KitchenProps.Root(r, "Toallas", new Vector3(0f, 0f, -10.5f));
        KitchenProps.C(tw, new Vector3(0f, 0.6f, 0f), new Vector3(0.8f, 1.2f, 0.4f), TWood, 0f, "Mueble");
        for (int i = 0; i < 4; i++) KitchenProps.C(tw, new Vector3(-0.27f + i * 0.18f, 1.26f, 0f), new Vector3(0.15f, 0.12f, 0.3f), new Color(0.95f, 0.95f, 0.92f), 0f, "Toalla");
        KitchenProps.Fit(tw); tw.gameObject.AddComponent<WorldLabel>().text = "Toallas";
        area.gameObject.AddComponent<WorldLabel>().text = "Gimnasio (abierto)";
    }

    /// <summary>PISO 1 DEL GIMNASIO (docs/entrenamiento-areas.md §3.3): CARDIO mirando al frente (−Z, el paisaje), con 2 m
    /// entre máquinas y 2 m detrás de cada fila (6 cintas, 4 elípticas, 2 escaladoras, 4 bicis, 2 reclinadas y 4 remos), y la
    /// ZONA LIBRE: 12 esterillas (abdominales y estiramientos), 2 sillas romanas, 2 bancos declinados, 2 de hiperextensiones,
    /// 2 sacos de boxeo, la estantería de material y espalderas. La zona de alfombras (<see cref="GymCarpetSpot"/>) queda
    /// libre.</summary>
    static void FurnishGymCardio(Transform floor, Vector2 size)
    {
        Transform r = new GameObject("Gimnasio_cardio").transform; r.SetParent(floor, false); r.localPosition = new Vector3(0f, 0.1f, 0f);
        KitchenProps.C(r, new Vector3(0f, 0.01f, 0f), new Vector3(size.x - 0.4f, 0.02f, size.y - 0.4f), TRubber, 0f, "Suelo_caucho");
        // CARDIO (x de −17 a −1,6): las filas, de delante (−Z) atrás; z = centro de la huella.
        float[] c1 = { -16.55f, -13.65f, -10.75f, -7.85f, -4.95f, -2.05f };
        for (int i = 0; i < 6; i++) GymCardio(r, GymMachineType.Cinta, i, new Vector3(c1[i], 0f, -10.95f));
        float[] c2 = { -16.6f, -13.85f, -11.1f, -8.35f };
        for (int i = 0; i < 4; i++) GymCardio(r, GymMachineType.Eliptica, i, new Vector3(c2[i], 0f, -6.825f));
        GymCardio(r, GymMachineType.Escaladora, 0, new Vector3(-5.575f, 0f, -7.1f));
        GymCardio(r, GymMachineType.Escaladora, 1, new Vector3(-2.775f, 0f, -7.1f));
        float[] c3 = { -16.7f, -14.1f, -11.5f, -8.9f };
        for (int i = 0; i < 4; i++) GymCardio(r, GymMachineType.BiciVertical, i, new Vector3(c3[i], 0f, -3.15f));
        GymCardio(r, GymMachineType.BiciReclinada, 0, new Vector3(-6.25f, 0f, -2.95f));
        GymCardio(r, GymMachineType.BiciReclinada, 1, new Vector3(-3.55f, 0f, -2.95f));
        for (int i = 0; i < 4; i++) GymCardio(r, GymMachineType.RemoErgometro, i, new Vector3(c3[i], 0f, 1.05f));   // copias 0–3 (las 4–5 están en el crossfit)
        WorldLabel cl = KitchenProps.Root(r, "Zona_cardio", new Vector3(-9f, 0f, -5f)).gameObject.AddComponent<WorldLabel>(); cl.text = "Cardio"; cl.height = 3.2f;

        // ZONA LIBRE (x de 4 a 17): 12 esterillas de 1 × 2 m con 2 m entre ellas; la mitad para abdominales y la mitad para estirar.
        float[] mx = { 4.5f, 7.5f, 10.5f, 13.5f }, mz = { 10.5f, 6.5f, 2.5f };
        int crunch = 0, stretch = 0, k = 0;
        foreach (float z in mz)
            foreach (float x in mx)
            {
                bool cr = k++ % 2 == 0;
                GMat(r, $"Esterilla_gimnasio_{k}", cr ? GymMachineType.Crunch : GymMachineType.EstiramientoIsquios, cr ? crunch++ : stretch++, new Vector3(x, 0f, z));
            }
        GymCaptainsChair(r, "Silla romana 1", 0, new Vector3(4.5f, 0f, -2.5f));
        GymCaptainsChair(r, "Silla romana 2", 1, new Vector3(7.5f, 0f, -2.5f));
        GymDeclineBench(r, "Banco declinado 1", 0, new Vector3(10.5f, 0f, -2.8f));
        GymDeclineBench(r, "Banco declinado 2", 1, new Vector3(13.1f, 0f, -2.8f));
        GymHyperext(r, "Hiperextensiones 1", 0, new Vector3(4.5f, 0f, -7f));
        GymHyperext(r, "Hiperextensiones 2", 1, new Vector3(7.5f, 0f, -7f));
        // SACOS DE BOXEO colgados de un pórtico de 2,6 m; cada uno con su zona de golpeo de Ø 2,4 m.
        Transform por = KitchenProps.Root(r, "Portico_sacos", new Vector3(12.5f, 0f, -7f));
        for (int s = -1; s <= 1; s += 2) GBar(por, new Vector3(s * 2.9f, 0f, 0f), new Vector3(s * 2.9f, 2.6f, 0f), 0.1f, TIron, "Poste");
        GBar(por, new Vector3(-2.9f, 2.6f, 0f), new Vector3(2.9f, 2.6f, 0f), 0.1f, TIron, "Viga");
        KitchenProps.Fit(por, false);
        for (int i = 0; i < 2; i++)
        {
            Transform bag = KitchenProps.Root(r, $"Saco_{i + 1}", new Vector3(11f + i * 3f, 0f, -7f));
            KitchenProps.C(bag, new Vector3(0f, 1.2f, 0f), new Vector3(0.4f, 1.1f, 0.4f), TRed, 0f, "Saco_boxeo");
            KitchenProps.C(bag, new Vector3(0f, 2.15f, 0f), new Vector3(0.02f, 0.8f, 0.02f), TIron, 0f, "Cadena");
            KitchenProps.Fit(bag);
            GStation(bag, GymMachineType.SacoBoxeo, i, new Vector3(0f, 0f, -0.85f), 0.90f, default, -1f, 0f);   // delante del saco, mirando hacia +Z
        }
        // Estantería de material: fitballs, rodillos, bandas, ruedas de abdominales y colchonetas.
        Transform sh = KitchenProps.Root(r, "Estanteria_material", new Vector3(6f, 0f, -11.2f));
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(sh, new Vector3(s * 1.18f, 0.9f, 0f), new Vector3(0.04f, 1.8f, 0.6f), TWood, 0f, "Lateral");
        for (int y = 0; y < 3; y++) KitchenProps.C(sh, new Vector3(0f, 0.05f + y * 0.7f, 0f), new Vector3(2.4f, 0.04f, 0.6f), TWood, 0f, "Balda");
        for (int i = 0; i < 3; i++) KitchenProps.C(sh, new Vector3(-0.8f + i * 0.6f, 0.36f, 0f), new Vector3(0.5f, 0.5f, 0.5f), new Color(0.3f + i * 0.2f, 0.5f, 0.8f - i * 0.2f), 45f, "Fitball");
        for (int i = 0; i < 4; i++) KitchenProps.C(sh, new Vector3(-0.9f + i * 0.25f, 0.83f, 0f), new Vector3(0.15f, 0.15f, 0.45f), new Color(0.2f, 0.25f, 0.35f), 0f, "Rodillo");
        for (int i = 0; i < 4; i++) KitchenProps.C(sh, new Vector3(0.3f + i * 0.22f, 0.8f, 0f), new Vector3(0.18f, 0.06f, 0.18f), new Color(0.85f, 0.3f + i * 0.15f, 0.2f), 0f, "Banda");
        for (int i = 0; i < 3; i++) KitchenProps.C(sh, new Vector3(-0.7f + i * 0.35f, 1.53f, 0f), new Vector3(0.06f, 0.18f, 0.18f), TPad, 0f, "Rueda_abdominales");
        for (int i = 0; i < 3; i++) KitchenProps.C(sh, new Vector3(0.5f, 1.47f + i * 0.04f, 0f), new Vector3(0.6f, 0.03f, 0.5f), TMat, 0f, "Colchoneta");
        KitchenProps.Fit(sh); sh.gameObject.AddComponent<WorldLabel>().text = "Material: fitballs, rodillos, bandas";
        GWallBars(r, "Espalderas_estiramiento", new Vector3(12.5f, 0f, -11.5f), 180f, 2, 0);   // miran hacia +Z
        WorldLabel zl = KitchenProps.Root(r, "Zona_libre", new Vector3(10.5f, 0f, 4.5f)).gameObject.AddComponent<WorldLabel>(); zl.text = "Zona libre: abdominales y estiramientos"; zl.height = 2.6f;
        floor.gameObject.AddComponent<WorldLabel>().text = "Gimnasio · cardio y zona libre";
    }

    /// <summary>PISO 2 DEL GIMNASIO (docs/entrenamiento-areas.md §3.4): CALISTENIA (dominadas a 3 alturas, monkey bars,
    /// anillas, paralelas bajas y largas, fondos en V, barras bajas, bandera humana, espalderas), PARKOUR (muros alto y medio,
    /// barra de lâché, cajones, barandillas y bloques de precisión) y CROSSFIT (rig de 10 m con cuerdas y dianas de wall
    /// ball, plataformas, remos, assault bikes, SkiErg, kettlebells, GHD, almacén y pista de trineo). Suelo de caucho, césped
    /// en el trineo y losetas gruesas bajo lo que tiene más de 1,6 m de caída. Ya está construido (no es obra).</summary>
    static void FurnishCalisthenics(Transform area, Vector2 size)
    {
        Transform r = new GameObject("Calistenia").transform; r.SetParent(area, false); r.localPosition = new Vector3(0f, 0.1f, 0f);
        KitchenProps.C(r, new Vector3(0f, 0.01f, 0f), new Vector3(size.x - 0.4f, 0.02f, size.y - 0.4f), TRubber, 0f, "Suelo_caucho_40mm");
        void Tile(Vector3 c, float w, float d) => KitchenProps.C(r, new Vector3(c.x, 0.03f, c.z), new Vector3(w, 0.02f, d), TTile, 0f, "Loseta_caida_80mm");

        // ── CALISTENIA (x de −17 a 1, z de 2,6 a 12) ──
        // Barras de dominadas a 3 alturas: 3 vanos de 1,2 m, con peldaños en los postes.
        Tile(new Vector3(-15.1f, 0f, 10f), 4.2f, 4f);
        Transform pu = KitchenProps.Root(r, "Barras_dominadas", new Vector3(-15.1f, 0f, 10f));
        float[] ph = { 2.0f, 2.3f, 2.5f };
        for (int i = 0; i < 4; i++)
        {
            float x = -1.8f + i * 1.2f;
            GBar(pu, new Vector3(x, 0f, 0f), new Vector3(x, 2.6f, 0f), 0.08f, TIron, "Poste");
            for (int p = 1; p <= 3; p++) KitchenProps.C(pu, new Vector3(x, p * 0.4f, 0f), new Vector3(0.22f, 0.04f, 0.04f), TAcc, 0f, "Peldano");
        }
        for (int i = 0; i < 3; i++)
        {
            float x = -1.2f + i * 1.2f;
            GBar(pu, new Vector3(x - 0.6f, ph[i], 0f), new Vector3(x + 0.6f, ph[i], 0f), 0.04f, TChrome, "Barra");
            GSpot(pu, GymMachineType.Dominadas, i, new Vector3(x, 0f, 0f), 0f, 1.33f + (ph[i] - 2.4f) / 0.974f);   // colgado, manos en la barra: la pelvis sube o baja con ella
        }
        KitchenProps.Fit(pu, false); pu.gameObject.AddComponent<WorldLabel>().text = "Dominadas (2,0 · 2,3 · 2,5 m)";
        // Escalera horizontal (monkey bars): peldaños a 2,4 m cada 0,4 m; se cruza en X.
        Tile(new Vector3(-9.4f, 0f, 10f), 4f, 2.6f);
        Transform mb = KitchenProps.Root(r, "Escalera_horizontal", new Vector3(-9.4f, 0f, 10f));
        for (int s = -1; s <= 1; s += 2)
        {
            GBar(mb, new Vector3(-1.8f, 2.4f, s * 0.4f), new Vector3(1.8f, 2.4f, s * 0.4f), 0.06f, TIron, "Larguero");
            for (int e = -1; e <= 1; e += 2) GBar(mb, new Vector3(e * 1.8f, 0f, s * 0.4f), new Vector3(e * 1.8f, 2.4f, s * 0.4f), 0.08f, TIron, "Poste");
        }
        for (int i = 0; i < 9; i++) GBar(mb, new Vector3(-1.6f + i * 0.4f, 2.4f, -0.4f), new Vector3(-1.6f + i * 0.4f, 2.4f, 0.4f), 0.035f, TChrome, "Peldano");
        for (int p = 1; p <= 3; p++) KitchenProps.C(mb, new Vector3(-1.8f, p * 0.5f, 0f), new Vector3(0.06f, 0.04f, 0.8f), TAcc, 0f, "Escalon");
        GSpot(mb, GymMachineType.MonkeyBars, 0, new Vector3(-1.4f, 0f, 0f), -90f, 1.39f);   // avanza hacia +X
        KitchenProps.Fit(mb, false); mb.gameObject.AddComponent<WorldLabel>().text = "Escalera horizontal";
        // Pórtico de anillas: viga a 3,0 m y 2 pares de anillas a 1,25 m (fondos).
        Tile(new Vector3(-4.1f, 0f, 10f), 3.4f, 4f);
        Transform ri = KitchenProps.Root(r, "Portico_anillas", new Vector3(-4.1f, 0f, 10f));
        for (int s = -1; s <= 1; s += 2) GBar(ri, new Vector3(s * 1.5f, 0f, 0f), new Vector3(s * 1.5f, 3f, 0f), 0.1f, TIron, "Poste");
        GBar(ri, new Vector3(-1.5f, 3f, 0f), new Vector3(1.5f, 3f, 0f), 0.1f, TIron, "Viga");
        for (int pr = 0; pr < 2; pr++)
        {
            float px = pr == 0 ? -0.75f : 0.75f;
            for (int s = -1; s <= 1; s += 2)
            {
                GBar(ri, new Vector3(px + s * 0.25f, 3f, 0f), new Vector3(px + s * 0.25f, 1.34f, 0f), 0.015f, TDisc, "Cinta_anilla");
                KitchenProps.C(ri, new Vector3(px + s * 0.25f, 1.25f, 0f), new Vector3(0.03f, 0.18f, 0.18f), TWood, 0f, "Anilla");
            }
            GSpot(ri, GymMachineType.FondosAnillas, pr, new Vector3(px, 0f, 0f), 0f, 1.33f + 0.05f / 0.974f);
        }
        KitchenProps.Fit(ri, false); ri.gameObject.AddComponent<WorldLabel>().text = "Anillas";
        // Paralelas bajas (parallettes): 4 pares de 0,33 m.
        Transform pl = KitchenProps.Root(r, "Paralelas_bajas", new Vector3(0.2f, 0f, 10f));
        for (int i = 0; i < 4; i++)
        {
            Vector3 c = new Vector3(i % 2 == 0 ? -0.4f : 0.4f, 0f, i < 2 ? -0.5f : 0.5f);
            for (int s = -1; s <= 1; s += 2)
            {
                GBar(pl, c + new Vector3(s * 0.2f, 0.33f, -0.25f), c + new Vector3(s * 0.2f, 0.33f, 0.25f), 0.04f, TWood, "Barra");
                for (int e = -1; e <= 1; e += 2) GBar(pl, c + new Vector3(s * 0.2f, 0f, e * 0.2f), c + new Vector3(s * 0.2f, 0.33f, e * 0.2f), 0.03f, TIron, "Pata");
            }
        }
        GSpot(pl, GymMachineType.LSit, 0, new Vector3(-0.4f, 0f, 0.5f), 0f, 0.42f);
        GSpot(pl, GymMachineType.LSit, 1, new Vector3(0.4f, 0f, 0.5f), 0f, 0.42f);
        KitchenProps.Fit(pl, false); pl.gameObject.AddComponent<WorldLabel>().text = "Paralelas bajas";
        // Paralelas largas a 1,2 m (fondos y desplazamientos): las barras van en X, la persona mira a lo largo de ellas.
        Transform pa = KitchenProps.Root(r, "Paralelas", new Vector3(-16f, 0f, 4.9f));
        for (int s = -1; s <= 1; s += 2)
        {
            GBar(pa, new Vector3(-1f, 1.2f, s * 0.25f), new Vector3(1f, 1.2f, s * 0.25f), 0.045f, TChrome, "Barra");
            for (int e = -1; e <= 1; e += 2) GBar(pa, new Vector3(e * 0.9f, 0f, s * 0.25f), new Vector3(e * 0.9f, 1.2f, s * 0.25f), 0.07f, TIron, "Poste");
        }
        GSpot(pa, GymMachineType.Fondos, 0, Vector3.zero, -90f, 1.39f);
        KitchenProps.Fit(pa, false); pa.gameObject.AddComponent<WorldLabel>().text = "Paralelas";
        // Barras de fondos en V a 1,3 m, abiertas de 0,45 a 0,6 m.
        Transform vd = KitchenProps.Root(r, "Fondos_en_V", new Vector3(-12.6f, 0f, 4.9f));
        for (int s = -1; s <= 1; s += 2)
        {
            GBar(vd, new Vector3(s * 0.3f, 1.3f, -0.3f), new Vector3(s * 0.225f, 1.3f, 0.3f), 0.045f, TChrome, "Barra");
            GBar(vd, new Vector3(s * 0.26f, 0f, 0.25f), new Vector3(s * 0.26f, 1.3f, 0.25f), 0.07f, TIron, "Poste");
        }
        GSpot(vd, GymMachineType.Fondos, 1, Vector3.zero, 0f, 1.39f + 0.1f / 0.974f);
        KitchenProps.Fit(vd, false); vd.gameObject.AddComponent<WorldLabel>().text = "Fondos";
        // Barras bajas: a 1,1 m (remo australiano) y a 1,8 m (front lever agrupado); el cuerpo queda en Z.
        Transform lb = KitchenProps.Root(r, "Barras_bajas", new Vector3(-9f, 0f, 4.9f));
        GBar(lb, new Vector3(-1.2f, 0f, 0f), new Vector3(-1.2f, 1.1f, 0f), 0.08f, TIron, "Poste");
        GBar(lb, new Vector3(0f, 0f, 0f), new Vector3(0f, 1.8f, 0f), 0.08f, TIron, "Poste");
        GBar(lb, new Vector3(1.2f, 0f, 0f), new Vector3(1.2f, 1.8f, 0f), 0.08f, TIron, "Poste");
        GBar(lb, new Vector3(-1.2f, 1.1f, 0f), new Vector3(0f, 1.1f, 0f), 0.04f, TChrome, "Barra_1,1");
        GBar(lb, new Vector3(0f, 1.8f, 0f), new Vector3(1.2f, 1.8f, 0f), 0.04f, TChrome, "Barra_1,8");
        GSpot(lb, GymMachineType.RemoAustraliano, 0, new Vector3(-0.6f, 0f, -0.5f), 0f, 0.30f);   // el pecho bajo la barra; los pies hacia −Z
        GSpot(lb, GymMachineType.FrontLever, 0, new Vector3(0.6f, 0f, 0f), 0f, 0.72f, new Vector3(0f, 0f, 0.52f));   // en B (horizontal) la raíz avanza: los hombros siguen bajo la barra
        KitchenProps.Fit(lb, false); lb.gameObject.AddComponent<WorldLabel>().text = "Barras bajas (1,1 · 1,8 m)";
        // Poste de bandera humana: agarres a 0,9 · 1,3 · 1,7 · 2,1 m; círculo libre de 2 m.
        Transform fp = KitchenProps.Root(r, "Poste_bandera", new Vector3(-3.8f, 0f, 5f));
        KitchenProps.C(fp, new Vector3(0f, 0.01f, 0f), new Vector3(0.4f, 0.02f, 0.4f), TIron, 0f, "Base");
        GBar(fp, Vector3.zero, new Vector3(0f, 2.6f, 0f), 0.06f, TIron, "Poste");
        foreach (float h in new[] { 0.9f, 1.3f, 1.7f, 2.1f }) GBar(fp, new Vector3(0f, h, 0f), new Vector3(-0.14f, h, 0f), 0.035f, TChrome, "Agarre");
        GSpot(fp, GymMachineType.BanderaHumana, 0, new Vector3(-0.75f, 0f, 0f), 0f, 1.50f);   // mirando hacia −Z, el poste queda a su izquierda (+X), a la distancia de los brazos
        KitchenProps.Fit(fp, false); fp.gameObject.AddComponent<WorldLabel>().text = "Bandera humana";
        GWallBars(r, "Espalderas", new Vector3(0.9f, 0f, 4.95f), 90f, 3, 2);   // 3 cuerpos, miran hacia −X
        WorldLabel kl = KitchenProps.Root(r, "Zona_calistenia", new Vector3(-8f, 0f, 7.5f)).gameObject.AddComponent<WorldLabel>(); kl.text = "Calistenia"; kl.height = 3.4f;

        // ── PARKOUR (x de −17 a −5,4, z de −12 a 0,6): recorrido barandillas → bloques → cajones → muro medio → lâché ──
        Tile(new Vector3(-15.5f, 0f, -10.7f), 3f, 1.5f);
        Transform hw = KitchenProps.Root(r, "Muro_alto", new Vector3(-15.5f, 0f, -11.6f));
        KitchenProps.C(hw, new Vector3(0f, 1.2f, 0f), new Vector3(3f, 2.4f, 0.3f), new Color(0.55f, 0.55f, 0.52f), 0f, "Muro_2,4");
        KitchenProps.Fit(hw);
        GSpot(hw, GymMachineType.MuroSubida, 0, new Vector3(0f, 0f, 0.30f), 0f, 1.59f);   // cara +Z, pegado al muro: colgado del borde y en B sube sobre él
        hw.gameObject.AddComponent<WorldLabel>().text = "Muro alto (2,4 m)";
        Transform mw = KitchenProps.Root(r, "Muro_medio", new Vector3(-11f, 0f, -11.6f));
        KitchenProps.C(mw, new Vector3(0f, 0.75f, 0f), new Vector3(2.4f, 1.5f, 0.3f), new Color(0.6f, 0.6f, 0.56f), 0f, "Muro_1,5");
        KitchenProps.Fit(mw); mw.gameObject.AddComponent<WorldLabel>().text = "Muro medio (1,5 m)";
        Tile(new Vector3(-7.6f, 0f, -10.8f), 3.6f, 2.4f);
        Transform la = KitchenProps.Root(r, "Portico_lache", new Vector3(-7.6f, 0f, -10.8f));
        for (int s = -1; s <= 1; s += 2) GBar(la, new Vector3(0f, 0f, s * 1.2f), new Vector3(0f, 2.05f, s * 1.2f), 0.1f, TIron, "Poste");
        GBar(la, new Vector3(0f, 2f, -1.2f), new Vector3(0f, 2f, 1.2f), 0.042f, TChrome, "Barra_lache");
        GSpot(la, GymMachineType.Lache, 0, new Vector3(-0.24f, 0f, 0f), -90f, 0.95f, new Vector3(0f, 0f, 0.65f));   // se balancea en X con las manos en la barra
        KitchenProps.Fit(la, false); la.gameObject.AddComponent<WorldLabel>().text = "Barra de lâché";
        float[] vx = { -11f, -7.8f };
        for (int i = 0; i < 2; i++)
        {
            Transform vb = KitchenProps.Root(r, $"Cajon_salto_{i + 1}", new Vector3(vx[i], 0f, -5.5f));
            KitchenProps.C(vb, new Vector3(0f, 0.5f, 0f), new Vector3(0.8f, 1f, 1.5f), TWood, 0f, "Cajon");
            KitchenProps.C(vb, new Vector3(0f, 1.01f, 0f), new Vector3(0.8f, 0.02f, 1.5f), TRubber, 0f, "Tapa_caucho");
            KitchenProps.Fit(vb);
            GSpot(vb, GymMachineType.KongVault, i, new Vector3(0f, 0f, 1.35f), 0f, 0.80f, new Vector3(0f, 0f, 1.35f));   // batida desde +Z; en B, sobre el cajón
            vb.gameObject.AddComponent<WorldLabel>().text = "Cajón de salto (1,0 m)";
        }
        float[] rx = { -16.4f, -14.9f }, rh = { 0.5f, 1.0f };
        for (int i = 0; i < 2; i++)
        {
            Transform ra = KitchenProps.Root(r, $"Barandilla_{i + 1}", new Vector3(rx[i], 0f, -1.1f));
            GBar(ra, new Vector3(0f, rh[i], -1.5f), new Vector3(0f, rh[i], 1.5f), 0.05f, TChrome, "Tubo");
            for (int p = 0; p < 3; p++) GBar(ra, new Vector3(0f, 0f, -1.4f + p * 1.4f), new Vector3(0f, rh[i] - 0.03f, -1.4f + p * 1.4f), 0.06f, TIron, "Soporte");
            KitchenProps.Fit(ra, false);
            GSpot(ra, GymMachineType.Barandilla, i, new Vector3(0f, 0f, 0f), 0f, 1.46f + (rh[i] - 0.5f) / 0.974f);
            ra.gameObject.AddComponent<WorldLabel>().text = $"Barandilla ({rh[i]:0.0} m)";
        }
        float[] bx = { -12.6f, -11f, -9.6f, -8.4f, -7f, -6f }, bh = { 0.40f, 0.50f, 0.65f, 0.50f, 0.40f, 0.65f };
        Transform bl = KitchenProps.Root(r, "Bloques_precision", new Vector3(0f, 0f, -1f));
        Vector3 BP(int i) => new Vector3(bx[i], 0f, i % 2 == 0 ? 0.3f : -0.3f);   // al tresbolillo
        for (int i = 0; i < 6; i++) KitchenProps.C(bl, BP(i) + new Vector3(0f, bh[i] * 0.5f, 0f), new Vector3(0.5f, bh[i], 0.5f), new Color(0.6f, 0.58f, 0.54f), 0f, "Bloque");
        for (int i = 0, c = 0; i < 6; i += 2, c++)   // un puesto cada dos bloques: salta al siguiente
        {
            Vector3 d = BP(i + 1) - BP(i);
            GSpot(bl, GymMachineType.SaltoPrecision, c, BP(i), Mathf.Atan2(-d.x, -d.z) * Mathf.Rad2Deg, 1.11f + (bh[i] - 0.5f) / 0.974f, new Vector3(0f, bh[i + 1] - bh[i] - 0.15f, d.magnitude));   // alturas del doc: de 0,5 a 0,65 m
        }
        KitchenProps.Fit(bl); bl.gameObject.AddComponent<WorldLabel>().text = "Bloques de precisión";
        WorldLabel pkl = KitchenProps.Root(r, "Zona_parkour", new Vector3(-11f, 0f, -8f)).gameObject.AddComponent<WorldLabel>(); pkl.text = "Parkour"; pkl.height = 3.2f;

        // ── CROSSFIT (x de 3 a 17, z de −12 a 12) ──
        KitchenProps.C(r, new Vector3(4f, 0.03f, -1f), new Vector3(2f, 0.02f, 20f), TTurf, 0f, "Pista_trineo");
        for (int i = 0; i < 2; i++)
        {
            Transform sl = KitchenProps.Root(r, $"Trineo_{i + 1}", new Vector3(3.5f + i, 0f, -9.3f));
            KitchenProps.C(sl, new Vector3(0f, 0.08f, 0f), new Vector3(0.7f, 0.12f, 1f), TIron, 0f, "Patin");
            for (int k2 = 0; k2 < 2; k2++) KitchenProps.C(sl, new Vector3(0f, 0.2f + k2 * 0.05f, 0.15f), new Vector3(0.05f, 0.45f, 0.45f), TBumper, 90f, "Disco");
            for (int s = -1; s <= 1; s += 2) GBar(sl, new Vector3(s * 0.25f, 0.1f, -0.45f), new Vector3(s * 0.25f, 1f, -0.5f), 0.05f, TChrome, "Poste_empuje");
            KitchenProps.Fit(sl);
            GSpot(sl, GymMachineType.Trineo, i, new Vector3(0f, 0f, -1.4f), 180f, 0.80f);   // empuja hacia +Z, manos en los postes
        }
        float[] pz = { -9.8f, -5.4f, -1f };
        for (int i = 0; i < 3; i++) GymPlatform(r, $"Plataforma crossfit {i + 1}", GymMachineType.Cargada, i, new Vector3(8.2f, 0f, pz[i]), TBumper);
        for (int i = 0; i < 2; i++)
        {
            float x = i == 0 ? 11.7f : 14.3f;
            GymCardio(r, GymMachineType.RemoErgometro, 4 + i, new Vector3(x, 0f, -10.8f));
            GymCardio(r, GymMachineType.AssaultBike, i, new Vector3(x, 0f, -6.95f));
            GymCardio(r, GymMachineType.SkiErg, i, new Vector3(x, 0f, -3.65f));
        }
        Transform kb = KitchenProps.Root(r, "Estante_kettlebells", new Vector3(16.9f, 0f, -7f));
        for (int y = 0; y < 2; y++) KitchenProps.C(kb, new Vector3(0f, 0.25f + y * 0.4f, 0f), new Vector3(0.5f, 0.04f, 2f), TIron, 0f, "Balda");
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(kb, new Vector3(0f, 0.4f, s * 0.98f), new Vector3(0.5f, 0.8f, 0.04f), TIron, 0f, "Lateral");
        for (int i = 0; i < 8; i++) { float sz = 0.16f + i * 0.015f; KitchenProps.C(kb, new Vector3(0f, (i < 4 ? 0.27f : 0.67f) + sz * 0.5f, -0.75f + (i % 4) * 0.5f), new Vector3(sz, sz * 1.1f, sz), Color.black, 0f, "Kettlebell"); }
        KitchenProps.Fit(kb); kb.gameObject.AddComponent<WorldLabel>().text = "Kettlebells (8–32 kg)";
        GSpot(r, GymMachineType.Swing, 0, new Vector3(15.7f, 0f, -7.6f), 90f, 0.88f);   // los swings, en x 15–16, mirando hacia −X
        GSpot(r, GymMachineType.Swing, 1, new Vector3(15.7f, 0f, -6.2f), 90f, 0.88f);
        GymRig(r, new Vector3(12.5f, 0f, 6f));
        GymGHD(r, "GHD 1", 0, new Vector3(8.2f, 0f, 3.5f));
        GymGHD(r, "GHD 2", 1, new Vector3(8.2f, 0f, 7.5f));
        // Almacén (x 16–17): bumpers, cajones pliométricos, balones medicinales y barras.
        Transform bu = KitchenProps.Root(r, "Estante_bumpers", new Vector3(16.5f, 0f, 2f));
        KitchenProps.C(bu, new Vector3(0f, 0.04f, 0f), new Vector3(1f, 0.08f, 1.2f), TIron, 0f, "Base");
        for (int i = 0; i < 6; i++) KitchenProps.C(bu, new Vector3(-0.25f + (i % 2) * 0.5f, 0.31f, -0.4f + (i / 2) * 0.4f), new Vector3(0.45f, 0.45f, 0.08f + (i / 2) * 0.03f), i < 2 ? TBumper : i < 4 ? new Color(0.75f, 0.65f, 0.15f) : new Color(0.2f, 0.5f, 0.25f), 0f, "Bumpers");
        KitchenProps.Fit(bu); bu.gameObject.AddComponent<WorldLabel>().text = "Discos (bumpers)";
        Transform pb = KitchenProps.Root(r, "Cajones_pliometricos", new Vector3(16.5f, 0f, 4.5f));
        float py = 0f;
        for (int i = 0; i < 6; i++) { float h = i % 3 == 0 ? 0.51f : i % 3 == 1 ? 0.61f : 0.76f; h *= 0.5f; KitchenProps.C(pb, new Vector3(0f, py + h * 0.5f, 0f), new Vector3(0.8f, h, 0.9f), TWood, i * 7f, "Cajon"); py += h; }   // apilados de lado
        KitchenProps.Fit(pb); pb.gameObject.AddComponent<WorldLabel>().text = "Cajones 51 · 61 · 76 cm";
        Transform mbl = KitchenProps.Root(r, "Balones_medicinales", new Vector3(16.5f, 0f, 7f));
        KitchenProps.C(mbl, new Vector3(0f, 0.3f, 0f), new Vector3(0.6f, 0.04f, 1.5f), TIron, 0f, "Balda");
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(mbl, new Vector3(0f, 0.3f, s * 0.73f), new Vector3(0.6f, 0.6f, 0.04f), TIron, 0f, "Lateral");
        for (int i = 0; i < 4; i++) KitchenProps.C(mbl, new Vector3(0f, 0.47f, -0.5f + i * 0.33f), new Vector3(0.3f, 0.3f, 0.3f), i % 2 == 0 ? new Color(0.2f, 0.2f, 0.25f) : new Color(0.6f, 0.15f, 0.15f), 20f, "Balon");
        KitchenProps.Fit(mbl); mbl.gameObject.AddComponent<WorldLabel>().text = "Balones medicinales (6 y 9 kg)";
        GBarHolder(r, new Vector3(16.5f, 0f, 9.5f), 8);
        // Cajones pliométricos de uso (61 cm) en el suelo abierto (x de 6 a 10,6, z de 9,6 a 12).
        for (int i = 0; i < 2; i++)
        {
            Transform bx2 = KitchenProps.Root(r, $"Cajon_pliometrico_{i + 1}", new Vector3(6.5f + i * 2.5f, 0f, 11.1f));
            KitchenProps.C(bx2, new Vector3(0f, 0.305f, 0f), new Vector3(0.76f, 0.61f, 0.51f), TWood, 0f, "Cajon_61");
            KitchenProps.Fit(bx2);
            GSpot(bx2, GymMachineType.SaltoCajon, i, new Vector3(0f, 0f, -0.65f), 180f, 0.60f, new Vector3(0f, 0f, 0.5f));   // mira hacia +Z y cae encima (la B ya cuenta el cajón)
        }
        WorldLabel cfl = KitchenProps.Root(r, "Zona_crossfit", new Vector3(10f, 0f, 0f)).gameObject.AddComponent<WorldLabel>(); cfl.text = "Crossfit"; cfl.height = 4.2f;
        area.gameObject.AddComponent<WorldLabel>().text = "Calistenia, parkour y crossfit";
    }

    // ── Constructores del edificio de entrenamiento (docs/entrenamiento-areas.md §3.7) ─────────────────────────────────────

    /// <summary>Puesto + <see cref="GymStation"/> en <paramref name="m"/>, como en <see cref="GymMachine"/>: el Puesto va en
    /// <paramref name="seat"/> girado <paramref name="yaw"/> (180 = mira hacia −Z, 0 = hacia +Z).
    /// <paramref name="shiftB"/> va en ejes del Puesto (+Z = hacia donde mira).</summary>
    static GymStation GStation(Transform m, GymMachineType t, int copy, Vector3 seat, float root, Vector3 shiftB = default, float side = -1f, float yaw = 180f)
    {
        Transform pu = new GameObject("Puesto").transform; pu.SetParent(m, false);
        pu.localPosition = seat; pu.localRotation = Quaternion.Euler(0f, yaw, 0f);
        GymStation st = m.gameObject.AddComponent<GymStation>();
        st.type = t; st.copy = copy; st.puesto = pu; st.rootHeight = root; st.shiftB = shiftB; st.approachSide = side;
        return st;
    }

    /// <summary>Sitio de uso dentro de un aparato con varios puestos (barras, rig, espalderas…): hijo vacío «Sitio_…» en
    /// <paramref name="pos"/>, girado <paramref name="yaw"/> (0: la persona mira hacia −Z; 180: +Z; −90: +X; 90: −X), con su
    /// Puesto y su <see cref="GymStation"/> (así el punto de llegada sale de su propio sitio).</summary>
    static GymStation GSpot(Transform parent, GymMachineType t, int copy, Vector3 pos, float yaw, float root, Vector3 shiftB = default, float side = -1f)
    {
        Transform sp = KitchenProps.Root(parent, $"Sitio_{GymStation.Label(t)} {copy + 1}", pos, yaw);
        return GStation(sp, t, copy, Vector3.zero, root, shiftB, side);
    }

    static void GLabel(Transform m, string text, float h = 2.4f) { WorldLabel wl = m.gameObject.AddComponent<WorldLabel>(); wl.text = text; wl.height = h; }

    /// <summary>Barra olímpica de <paramref name="len"/> m en X, centrada en <paramref name="c"/>, con <paramref name="n"/>
    /// discos de Ø <paramref name="d"/> por lado.</summary>
    static void GBarbell(Transform m, Vector3 c, float len = 2.2f, float d = 0.45f, int n = 2, Color? disc = null)
    {
        Color dc = disc ?? TDisc;
        KitchenProps.C(m, c, new Vector3(len - 0.02f, 0.03f, 0.03f), TChrome, 0f, "Barra_olimpica");
        for (int s = -1; s <= 1; s += 2)
        {
            KitchenProps.C(m, c + new Vector3(s * (len * 0.5f - 0.22f), 0f, 0f), new Vector3(0.42f, 0.05f, 0.05f), TChrome, 0f, "Manguito");
            for (int i = 0; i < n; i++) KitchenProps.C(m, c + new Vector3(s * (len * 0.5f - 0.40f + i * 0.06f), 0f, 0f), new Vector3(0.05f, d, d), dc, 0f, "Disco");
        }
    }

    /// <summary>JAULA DE SENTADILLAS (power rack): caja de 1,3 × 1,4 m y 2,3 m de alto, J-hooks con la barra a 1,45 m y topes
    /// de seguridad a 0,75 m. Se usa dentro, mirando hacia −Z.</summary>
    static void GymRack(Transform parent, string name, int copy, Vector3 pos)
    {
        Transform m = KitchenProps.Root(parent, "Maquina_" + name, pos);
        for (int sx = -1; sx <= 1; sx += 2)
        {
            for (int sz = -1; sz <= 1; sz += 2) GBar(m, new Vector3(sx * 0.6f, 0f, sz * 0.65f), new Vector3(sx * 0.6f, 2.3f, sz * 0.65f), 0.08f, TIron, "Poste");
            GBar(m, new Vector3(sx * 0.6f, 0.04f, -0.65f), new Vector3(sx * 0.6f, 0.04f, 0.65f), 0.08f, TIron, "Larguero");
            GBar(m, new Vector3(sx * 0.6f, 2.26f, -0.65f), new Vector3(sx * 0.6f, 2.26f, 0.65f), 0.08f, TIron, "Larguero_alto");
            GBar(m, new Vector3(sx * 0.6f, 0.75f, -0.65f), new Vector3(sx * 0.6f, 0.75f, 0.65f), 0.05f, TAcc, "Tope_seguridad");
            KitchenProps.C(m, new Vector3(sx * 0.6f, 1.40f, -0.71f), new Vector3(0.06f, 0.08f, 0.06f), TAcc, 0f, "J_hook");
        }
        for (int sz = -1; sz <= 1; sz += 2) GBar(m, new Vector3(-0.6f, 2.26f, sz * 0.65f), new Vector3(0.6f, 2.26f, sz * 0.65f), 0.08f, TIron, "Travesano");
        GBar(m, new Vector3(-0.6f, 0.04f, 0.65f), new Vector3(0.6f, 0.04f, 0.65f), 0.08f, TIron, "Travesano");
        GBarbell(m, new Vector3(0f, 1.45f, -0.71f));
        KitchenProps.Fit(m);
        GStation(m, GymMachineType.SentadillaJaula, copy, new Vector3(0f, 0f, -0.74f), 0.95f, new Vector3(0f, 0f, -0.19f));   // bajo la barra de los J-hooks; en B la pelvis retrocede
        GLabel(m, name);
    }

    /// <summary>PRESS DE BANCA olímpico (2,2 × 1,7 m): plano (banco a 0,43 m, tumbado con la cabeza hacia +Z y los ojos bajo
    /// la barra) o inclinado (asiento a 0,45 m y respaldo a 30° de la horizontal, mirando hacia −Z). Soportes detrás (+Z).</summary>
    static void GymBench(Transform parent, string name, int copy, Vector3 pos, bool incline)
    {
        Transform m = KitchenProps.Root(parent, "Maquina_" + name, pos);
        GFrame(m, 0.5f, -0.8f, 0.8f);
        float hook = incline ? 1.40f : 1.10f;
        for (int s = -1; s <= 1; s += 2)
        {
            GBar(m, new Vector3(s * 0.55f, 0f, 0.62f), new Vector3(s * 0.55f, hook, 0.62f), 0.07f, TIron, "Soporte_barra");
            GBar(m, new Vector3(s * 0.25f, 0.04f, 0.62f), new Vector3(s * 0.55f, 0.04f, 0.62f), 0.07f, TIron, "Pie_soporte");
            KitchenProps.C(m, new Vector3(s * 0.55f, hook - 0.06f, 0.56f), new Vector3(0.06f, 0.06f, 0.08f), TAcc, 0f, "Gancho");
        }
        GBarbell(m, new Vector3(0f, hook, 0.56f));
        if (incline)
        {
            GSeat(m, -0.45f, 0.45f, 0.32f, 0.35f, TPad);
            GBack(m, new Vector3(0f, 0.48f, -0.25f), 1.0f, 60f, 0.30f, TPad);
            KitchenProps.Fit(m);
            GStation(m, GymMachineType.PressBancaInclinado, copy, new Vector3(0f, 0f, -0.42f), 0.57f);
        }
        else
        {
            for (int e = -1; e <= 1; e += 2) GBar(m, new Vector3(0f, 0.04f, e * 0.45f - 0.1f), new Vector3(0f, 0.35f, e * 0.45f - 0.1f), 0.07f, TIron, "Pata");
            KitchenProps.C(m, new Vector3(0f, 0.39f, -0.1f), new Vector3(0.30f, 0.08f, 1.2f), TPad, 0f, "Banco");
            KitchenProps.Fit(m);
            GStation(m, GymMachineType.PressBancaPlano, copy, new Vector3(0f, 0f, -0.2f), 0.55f);
        }
        GLabel(m, name);
    }

    /// <summary>MÁQUINA SMITH (2,0 × 1,5 × 2,2 m): barra guiada en dos raíles a 1,45 m; sentadilla con los pies adelantados.</summary>
    static void GymSmith(Transform parent, string name, int copy, Vector3 pos)
    {
        Transform m = KitchenProps.Root(parent, "Maquina_" + name, pos);
        GFrame(m, 1.9f, -0.7f, 0.7f, 0f);
        for (int sx = -1; sx <= 1; sx += 2)
        {
            for (int sz = -1; sz <= 1; sz += 2) GBar(m, new Vector3(sx * 0.95f, 0f, sz * 0.7f), new Vector3(sx * 0.95f, 2.2f, sz * 0.7f), 0.08f, TIron, "Poste");
            GBar(m, new Vector3(sx * 0.95f, 2.16f, -0.7f), new Vector3(sx * 0.95f, 2.16f, 0.7f), 0.08f, TIron, "Larguero_alto");
            GBar(m, new Vector3(sx * 0.5f, 0.06f, 0f), new Vector3(sx * 0.5f, 2.16f, 0f), 0.04f, TChrome, "Guia");
            KitchenProps.C(m, new Vector3(sx * 0.5f, 1.45f, 0f), new Vector3(0.1f, 0.14f, 0.1f), TAcc, 0f, "Carro_guia");
            GBar(m, new Vector3(sx * 0.7f, 0.7f, -0.65f), new Vector3(sx * 0.7f, 0.7f, 0.65f), 0.05f, TAcc, "Tope_seguridad");
        }
        GBar(m, new Vector3(-0.95f, 2.16f, 0f), new Vector3(0.95f, 2.16f, 0f), 0.08f, TIron, "Travesano_guias");
        for (int sz = -1; sz <= 1; sz += 2) GBar(m, new Vector3(-0.95f, 2.16f, sz * 0.7f), new Vector3(0.95f, 2.16f, sz * 0.7f), 0.08f, TIron, "Travesano");
        GBarbell(m, new Vector3(0f, 1.45f, 0f), 1.9f);
        KitchenProps.Fit(m);
        GStation(m, GymMachineType.SentadillaSmith, copy, new Vector3(0f, 0f, -0.06f), 0.95f, new Vector3(0f, 0f, -0.10f));   // la barra, a la espalda
        GLabel(m, name);
    }

    /// <summary>PLATAFORMA de halterofilia de 2,4 × 2,4 m (madera en el centro y caucho donde caen los discos), con la barra en
    /// el suelo (discos de Ø 0,45). Peso muerto en el piso 0 y cargadas en el crossfit.</summary>
    static void GymPlatform(Transform parent, string name, GymMachineType type, int copy, Vector3 pos, Color disc)
    {
        Transform m = KitchenProps.Root(parent, "Maquina_" + name, pos);
        KitchenProps.C(m, new Vector3(0f, 0.025f, 0f), new Vector3(2.4f, 0.05f, 2.4f), TPad, 0f, "Plataforma_caucho");
        KitchenProps.C(m, new Vector3(0f, 0.026f, 0f), new Vector3(1.2f, 0.051f, 2.4f), TPlatWood, 0f, "Plataforma_madera");
        GBarbell(m, new Vector3(0f, 0.05f + 0.225f, -0.15f), 2.2f, 0.45f, 1, disc);
        KitchenProps.Fit(m, false);   // se pisa: no bloquea
        GStation(m, type, copy, new Vector3(0f, 0.05f, 0.24f), 0.64f, type == GymMachineType.PesoMuerto ? new Vector3(0f, 0f, 0.10f) : default);   // en el bloqueo la cadera avanza
        GLabel(m, name);
    }

    /// <summary>BANCO REGULABLE (FID, 0,6 × 1,4 m; respaldo de 0 a 85°, aquí plano) con una mancuerna en el suelo: remo a una
    /// mano, con la rodilla y la mano izquierdas en el banco y la persona en su lado +X.</summary>
    static void GymFID(Transform parent, string name, int copy, Vector3 pos)
    {
        Transform m = KitchenProps.Root(parent, "Maquina_" + name, pos);
        GFrame(m, 0.5f, -0.7f, 0.7f);
        for (int e = -1; e <= 1; e += 2) GBar(m, new Vector3(0f, 0.04f, e * 0.45f), new Vector3(0f, 0.36f, e * 0.45f), 0.07f, TIron, "Pata");
        KitchenProps.C(m, new Vector3(0f, 0.40f, -0.45f), new Vector3(0.28f, 0.07f, 0.40f), TPad, 0f, "Asiento");
        KitchenProps.C(m, new Vector3(0f, 0.40f, 0.22f), new Vector3(0.28f, 0.07f, 0.90f), TPad, 0f, "Respaldo");
        KitchenProps.C(m, new Vector3(0.45f, 0.06f, 0.55f), new Vector3(0.32f, 0.1f, 0.1f), Color.black, 0f, "Mancuerna");
        KitchenProps.Fit(m);
        GStation(m, GymMachineType.RemoMancuerna, copy, new Vector3(0.22f, 0f, 0.05f), 0.95f, default, 1f, 0f);   // mirando hacia +Z: el banco le queda a la izquierda
        GLabel(m, name, 1.6f);
    }

    /// <summary>ESTANTE DE MANCUERNAS (2,3 × 0,7 m): dos baldas inclinadas 15° hacia el frente (−Z), a 0,55 y 0,85 m, con 10
    /// pares cada una (de 2 a 40 kg).</summary>
    static void GDumbbellRack(Transform parent, string name, Vector3 pos)
    {
        Transform m = KitchenProps.Root(parent, name, pos);
        for (int s = -1; s <= 1; s += 2) for (int e = -1; e <= 1; e += 2) GBar(m, new Vector3(s * 1.1f, 0f, e * 0.3f), new Vector3(s * 1.1f, 0.9f, e * 0.3f), 0.06f, TIron, "Pata");
        for (int k = 0; k < 2; k++)
        {
            float y = k == 0 ? 0.55f : 0.85f, z = k == 0 ? -0.15f : 0.15f;
            KitchenProps.C(m, new Vector3(0f, y, z), new Vector3(2.3f, 0.04f, 0.32f), TIron, new Vector3(-15f, 0f, 0f), "Balda");
            for (int i = 0; i < 10; i++)
            {
                float w = 0.07f + (k * 10 + i) * 0.004f;
                for (int p = -1; p <= 1; p += 2) KitchenProps.C(m, new Vector3(-1.035f + i * 0.23f + p * 0.055f, y + 0.02f + w * 0.5f, z), new Vector3(w, w, 0.28f), Color.black, new Vector3(-15f, 0f, 0f), "Mancuerna");
            }
        }
        KitchenProps.Fit(m); GLabel(m, "Mancuernas (2–40 kg)", 1.4f);
    }

    /// <summary>ÁRBOL DE DISCOS (0,6 × 0,6 m).</summary>
    static void GPlateTree(Transform parent, Vector3 pos)
    {
        Transform m = KitchenProps.Root(parent, "Arbol_de_discos", pos);
        KitchenProps.C(m, new Vector3(0f, 0.03f, 0f), new Vector3(0.6f, 0.06f, 0.6f), TIron, 0f, "Base");
        GBar(m, new Vector3(0f, 0.06f, 0f), new Vector3(0f, 1.3f, 0f), 0.07f, TIron, "Poste");
        for (int i = 0; i < 6; i++)
        {
            float y = 0.35f + (i / 2) * 0.33f, s = i % 2 == 0 ? 1f : -1f, d = 0.45f - (i / 2) * 0.1f;
            KitchenProps.C(m, new Vector3(s * 0.12f, y, 0f), new Vector3(0.2f, 0.04f, 0.04f), TChrome, 0f, "Gancho");
            KitchenProps.C(m, new Vector3(s * 0.16f, y, 0f), new Vector3(0.06f, d, d), TDisc, 0f, "Discos");
        }
        KitchenProps.Fit(m);
    }

    /// <summary>SOPORTE DE BARRAS olímpicas en vertical (0,6 × 0,6 m o 0,6 × 1,0).</summary>
    static void GBarHolder(Transform parent, Vector3 pos, int bars)
    {
        Transform m = KitchenProps.Root(parent, "Soporte_barras", pos);
        float d = bars > 6 ? 1f : 0.6f;
        KitchenProps.C(m, new Vector3(0f, 0.05f, 0f), new Vector3(0.6f, 0.1f, d), TIron, 0f, "Base");
        for (int i = 0; i < bars; i++) KitchenProps.C(m, new Vector3(i % 2 == 0 ? -0.12f : 0.12f, 1.1f, -d * 0.5f + 0.1f + (i / 2) * (d - 0.2f) / Mathf.Max(1, (bars - 1) / 2)), new Vector3(0.03f, 2.2f, 0.03f), TChrome, 0f, "Barra");
        KitchenProps.Fit(m); GLabel(m, "Barras olímpicas", 2.4f);
    }

    /// <summary>MÁQUINA DE CARDIO o de acondicionamiento (docs/entrenamiento-areas.md §2.5 y §2.4), con el origen en el centro
    /// de su huella y mirando hacia −Z: cinta, elíptica, escaladora, bici vertical o reclinada, remo, assault bike o SkiErg.</summary>
    static void GymCardio(Transform parent, GymMachineType t, int copy, Vector3 pos)
    {
        string name = $"{GymStation.Label(t)} {copy + 1}";
        Transform m = KitchenProps.Root(parent, "Maquina_" + name, pos);
        Vector3 seat; float root; Vector3 shiftB = Vector3.zero;
        switch (t)
        {
            case GymMachineType.Cinta:   // 0,9 × 2,1: banda a 0,04 m, motor delante, consola y pasamanos
                KitchenProps.C(m, new Vector3(0f, 0.02f, 0.15f), new Vector3(0.56f, 0.04f, 1.7f), TRubber, 0f, "Banda");
                for (int s = -1; s <= 1; s += 2)
                {
                    KitchenProps.C(m, new Vector3(s * 0.37f, 0.04f, 0.12f), new Vector3(0.16f, 0.08f, 1.85f), TIron, 0f, "Larguero");
                    GBar(m, new Vector3(s * 0.38f, 0.1f, -0.85f), new Vector3(s * 0.36f, 1.25f, -0.8f), 0.07f, TIron, "Columna");
                    GBar(m, new Vector3(s * 0.38f, 1.0f, -0.78f), new Vector3(s * 0.38f, 1.0f, -0.25f), 0.04f, TChrome, "Pasamanos");
                }
                KitchenProps.C(m, new Vector3(0f, 0.12f, -0.87f), new Vector3(0.85f, 0.24f, 0.35f), TIron, 0f, "Motor");
                KitchenProps.C(m, new Vector3(0f, 1.32f, -0.8f), new Vector3(0.8f, 0.3f, 0.1f), TIron, new Vector3(-30f, 0f, 0f), "Consola");
                seat = new Vector3(0f, 0f, 0.15f); root = 0.93f; break;
            case GymMachineType.Eliptica:   // 0,75 × 2,15: riel central, volante detrás, pedales a 0,25 m y brazos móviles
                KitchenProps.C(m, new Vector3(0f, 0.06f, 0.05f), new Vector3(0.3f, 0.12f, 1.9f), TIron, 0f, "Riel");
                KitchenProps.C(m, new Vector3(0f, 0.04f, -0.95f), new Vector3(0.7f, 0.08f, 0.15f), TIron, 0f, "Pie");
                KitchenProps.C(m, new Vector3(0f, 0.35f, 0.85f), new Vector3(0.32f, 0.6f, 0.4f), TIron, 0f, "Volante");
                for (int s = -1; s <= 1; s += 2)
                {
                    KitchenProps.C(m, new Vector3(s * 0.13f, 0.25f, 0.05f + s * 0.12f), new Vector3(0.15f, 0.04f, 0.38f), TRubber, 0f, "Pedal");
                    GBar(m, new Vector3(s * 0.25f, 0.3f, -0.55f), new Vector3(s * 0.3f, 1.55f, -0.45f), 0.04f, TChrome, "Brazo");
                }
                GBar(m, new Vector3(0f, 0.1f, -0.95f), new Vector3(0f, 1.4f, -0.85f), 0.09f, TIron, "Columna");
                KitchenProps.C(m, new Vector3(0f, 1.5f, -0.85f), new Vector3(0.4f, 0.25f, 0.08f), TIron, new Vector3(-30f, 0f, 0f), "Consola");
                seat = new Vector3(0f, 0f, 0.05f); root = 1.15f; break;
            case GymMachineType.Escaladora:   // 0,8 × 1,6 × 2,1: escalera giratoria de peldaños de 0,20 m y barandillas
                GFrame(m, 0.75f, -0.8f, 0.8f);
                KitchenProps.C(m, new Vector3(0f, 0.6f, -0.6f), new Vector3(0.7f, 1.2f, 0.35f), TIron, 0f, "Carcasa");
                for (int i = 0; i < 4; i++) KitchenProps.C(m, new Vector3(0f, 0.12f + i * 0.2f, 0.45f - i * 0.22f), new Vector3(0.6f, 0.04f, 0.26f), TRubber, 0f, "Peldano");
                for (int s = -1; s <= 1; s += 2)
                {
                    GBar(m, new Vector3(s * 0.38f, 0.04f, 0.75f), new Vector3(s * 0.38f, 1.2f, 0.45f), 0.06f, TIron, "Poste");
                    GBar(m, new Vector3(s * 0.38f, 1.2f, 0.45f), new Vector3(s * 0.38f, 1.25f, -0.6f), 0.045f, TChrome, "Barandilla");
                }
                GBar(m, new Vector3(0f, 1.2f, -0.6f), new Vector3(0f, 2.0f, -0.7f), 0.08f, TIron, "Columna");
                KitchenProps.C(m, new Vector3(0f, 2.05f, -0.7f), new Vector3(0.45f, 0.25f, 0.08f), TIron, new Vector3(-20f, 0f, 0f), "Consola");
                seat = new Vector3(0f, 0f, 0.2f); root = 1.10f; break;
            case GymMachineType.BiciVertical:   // 0,6 × 1,2: sillín a 0,95 m, volante delante
            case GymMachineType.AssaultBike:    // 0,6 × 1,3: sillín a 0,85 m, ventilador delante y brazos que empujan y tiran
            {
                bool air = t == GymMachineType.AssaultBike;
                float sy = air ? 0.85f : 0.95f;
                KitchenProps.C(m, new Vector3(0f, 0.04f, 0f), new Vector3(0.55f, 0.08f, air ? 1.2f : 1.1f), TIron, 0f, "Base");
                if (air) KitchenProps.C(m, new Vector3(0f, 0.5f, -0.35f), new Vector3(0.25f, 0.62f, 0.62f), TIron, 0f, "Ventilador");
                else KitchenProps.C(m, new Vector3(0f, 0.4f, -0.35f), new Vector3(0.12f, 0.5f, 0.5f), TIron, 0f, "Volante");
                GBar(m, new Vector3(0f, 0.08f, 0.3f), new Vector3(0f, sy - 0.04f, 0.25f), 0.08f, TIron, "Tija");
                KitchenProps.C(m, new Vector3(0f, sy, 0.25f), new Vector3(0.22f, 0.06f, 0.28f), TPad, 0f, "Sillin");
                for (int s = -1; s <= 1; s += 2) KitchenProps.C(m, new Vector3(s * 0.13f, 0.35f, -0.02f), new Vector3(0.1f, 0.03f, 0.12f), TRubber, 0f, "Pedal");
                if (air) for (int s = -1; s <= 1; s += 2) GBar(m, new Vector3(s * 0.2f, 0.5f, -0.2f), new Vector3(s * 0.25f, 1.25f, -0.25f), 0.05f, TChrome, "Brazo");
                else
                {
                    GBar(m, new Vector3(0f, 0.08f, -0.3f), new Vector3(0f, 1.15f, -0.4f), 0.08f, TIron, "Columna");
                    KitchenProps.C(m, new Vector3(0f, 1.15f, -0.42f), new Vector3(0.45f, 0.04f, 0.12f), TChrome, 0f, "Manillar");
                }
                seat = new Vector3(0f, 0f, 0.25f); root = air ? 0.98f : 1.09f; break;
            }
            case GymMachineType.BiciReclinada:   // 0,7 × 1,6: asiento a 0,50 m, respaldo a 25°, pedales delante y asas laterales
                KitchenProps.C(m, new Vector3(0f, 0.04f, 0f), new Vector3(0.6f, 0.08f, 1.5f), TIron, 0f, "Base");
                GSeat(m, 0.3f, 0.5f, 0.45f, 0.4f, TPad);
                GBack(m, new Vector3(0f, 0.54f, 0.48f), 0.6f, 25f, 0.42f, TPad);
                KitchenProps.C(m, new Vector3(0f, 0.35f, -0.6f), new Vector3(0.25f, 0.5f, 0.35f), TIron, 0f, "Volante");
                for (int s = -1; s <= 1; s += 2)
                {
                    KitchenProps.C(m, new Vector3(s * 0.13f, 0.38f, -0.42f), new Vector3(0.1f, 0.12f, 0.03f), TRubber, 0f, "Pedal");
                    KitchenProps.C(m, new Vector3(s * 0.3f, 0.45f, 0.25f), new Vector3(0.04f, 0.04f, 0.2f), TChrome, 0f, "Asa");
                }
                GBar(m, new Vector3(0f, 0.6f, -0.6f), new Vector3(0f, 1.1f, -0.45f), 0.06f, TIron, "Columna");
                KitchenProps.C(m, new Vector3(0f, 1.15f, -0.45f), new Vector3(0.35f, 0.2f, 0.06f), TIron, new Vector3(-30f, 0f, 0f), "Consola");
                seat = new Vector3(0f, 0f, 0.3f); root = 0.62f; break;
            case GymMachineType.RemoErgometro:   // 0,6 × 2,4: riel, volante delante, asiento a 0,36 m que corre 0,6 m hacia atrás
                GBar(m, new Vector3(0f, 0.3f, -0.85f), new Vector3(0f, 0.3f, 1.15f), 0.12f, TChrome, "Riel");
                KitchenProps.C(m, new Vector3(0f, 0.04f, -1f), new Vector3(0.55f, 0.08f, 0.3f), TIron, 0f, "Pie_delantero");
                KitchenProps.C(m, new Vector3(0f, 0.15f, 1.15f), new Vector3(0.45f, 0.3f, 0.08f), TIron, 0f, "Pie_trasero");
                KitchenProps.C(m, new Vector3(0f, 0.45f, -1f), new Vector3(0.2f, 0.5f, 0.4f), TIron, 0f, "Volante");
                KitchenProps.C(m, new Vector3(0f, 0.38f, -0.15f), new Vector3(0.3f, 0.05f, 0.3f), TPad, 0f, "Asiento");
                for (int s = -1; s <= 1; s += 2) KitchenProps.C(m, new Vector3(s * 0.12f, 0.3f, -0.62f), new Vector3(0.12f, 0.04f, 0.3f), TRubber, new Vector3(-45f, 0f, 0f), "Reposapies");
                KitchenProps.C(m, new Vector3(0f, 0.55f, -0.8f), new Vector3(0.5f, 0.04f, 0.04f), TChrome, 0f, "Mango");
                GBar(m, new Vector3(0f, 0.6f, -1.05f), new Vector3(0f, 1f, -0.95f), 0.04f, TIron, "Brazo_monitor");
                KitchenProps.C(m, new Vector3(0f, 1.05f, -0.95f), new Vector3(0.2f, 0.15f, 0.04f), TIron, 0f, "Monitor");
                seat = new Vector3(0f, 0f, -0.36f); root = 0.53f; shiftB = new Vector3(0f, 0f, -0.52f); break;   // el carro va hacia atrás; los pies, en los reposapiés
            default:   // SkiErg con soporte: 0,6 × 1,3 × 2,16, torre delante y dos tiradores arriba
                KitchenProps.C(m, new Vector3(0f, 0.03f, 0f), new Vector3(0.6f, 0.06f, 1.3f), TRubber, 0f, "Plataforma");
                GBar(m, new Vector3(0f, 0.06f, -0.5f), new Vector3(0f, 2.16f, -0.5f), 0.12f, TIron, "Torre");
                KitchenProps.C(m, new Vector3(0f, 1.9f, -0.42f), new Vector3(0.25f, 0.35f, 0.2f), TIron, 0f, "Volante");
                for (int s = -1; s <= 1; s += 2)
                {
                    GBar(m, new Vector3(s * 0.08f, 2f, -0.4f), new Vector3(s * 0.15f, 1.9f, -0.35f), 0.015f, TDisc, "Cuerda");
                    KitchenProps.C(m, new Vector3(s * 0.15f, 1.83f, -0.35f), new Vector3(0.04f, 0.14f, 0.04f), TAcc, 0f, "Tirador");
                }
                seat = new Vector3(0f, 0.06f, 0.25f); root = 0.95f; break;
        }
        KitchenProps.Fit(m);
        GStation(m, t, copy, seat, root, shiftB);
        GLabel(m, name, 2.2f);
    }

    /// <summary>ESTERILLA de 1 × 2 m. Abdominales: tumbado con la cabeza hacia +Z. Isquios: sentado con las piernas hacia −Z.</summary>
    static void GMat(Transform parent, string name, GymMachineType t, int copy, Vector3 pos)
    {
        Transform m = KitchenProps.Root(parent, name, pos);
        KitchenProps.C(m, new Vector3(0f, 0.03f, 0f), new Vector3(1f, 0.02f, 2f), TMat, 0f, "Esterilla");
        GStation(m, t, copy, new Vector3(0f, 0.04f, t == GymMachineType.Crunch ? -0.25f : 0.45f), 0.13f);
    }

    /// <summary>SILLA ROMANA (VKR, 1,0 × 1,0 × 2,2 m): respaldo, apoyabrazos a 1,45 m y escalón; elevación de rodillas.</summary>
    static void GymCaptainsChair(Transform parent, string name, int copy, Vector3 pos)
    {
        Transform m = KitchenProps.Root(parent, "Maquina_" + name, pos);
        for (int s = -1; s <= 1; s += 2)
        {
            GBar(m, new Vector3(s * 0.45f, 0f, 0.4f), new Vector3(s * 0.45f, 2.2f, 0.4f), 0.07f, TIron, "Poste");
            GBar(m, new Vector3(s * 0.45f, 0.04f, -0.45f), new Vector3(s * 0.45f, 0.04f, 0.4f), 0.07f, TIron, "Pie");
            GBar(m, new Vector3(s * 0.45f, 1.4f, 0.4f), new Vector3(s * 0.25f, 1.4f, 0.3f), 0.05f, TIron, "Brazo");
            KitchenProps.C(m, new Vector3(s * 0.25f, 1.45f, 0.05f), new Vector3(0.12f, 0.08f, 0.5f), TPad, 0f, "Apoyabrazos");
            KitchenProps.C(m, new Vector3(s * 0.25f, 1.5f, -0.18f), new Vector3(0.04f, 0.12f, 0.04f), TChrome, 0f, "Agarre");
        }
        GBar(m, new Vector3(-0.45f, 2.2f, 0.4f), new Vector3(0.45f, 2.2f, 0.4f), 0.07f, TIron, "Travesano");
        KitchenProps.C(m, new Vector3(0f, 1.35f, 0.33f), new Vector3(0.35f, 0.7f, 0.08f), TPad, 0f, "Respaldo");
        KitchenProps.C(m, new Vector3(0f, 0.35f, 0.25f), new Vector3(0.4f, 0.04f, 0.2f), TIron, 0f, "Escalon");
        KitchenProps.Fit(m);
        GStation(m, GymMachineType.SillaRomana, copy, new Vector3(0f, 0f, 0.2f), 1.30f);
        GLabel(m, name);
    }

    /// <summary>BANCO DE ABDOMINALES DECLINADO (0,6 × 1,6 m, 30°): el extremo alto (+Z) tiene los rodillos de los pies; se
    /// usa mirando hacia +Z y tumbándose hacia el extremo bajo.</summary>
    static void GymDeclineBench(Transform parent, string name, int copy, Vector3 pos)
    {
        Transform m = KitchenProps.Root(parent, "Maquina_" + name, pos);
        GFrame(m, 0.5f, -0.75f, 0.75f);
        KitchenProps.C(m, new Vector3(0f, 0.62f, -0.15f), new Vector3(0.3f, 0.07f, 1.2f), TPad, new Vector3(-30f, 0f, 0f), "Banco");
        GBar(m, new Vector3(0f, 0.04f, -0.55f), new Vector3(0f, 0.38f, -0.55f), 0.07f, TIron, "Pata");
        GBar(m, new Vector3(0f, 0.04f, 0.3f), new Vector3(0f, 0.85f, 0.3f), 0.07f, TIron, "Pata");
        GBar(m, new Vector3(0f, 0.85f, 0.3f), new Vector3(0f, 0.85f, 0.65f), 0.06f, TIron, "Brazo_rodillos");
        for (int k = 0; k < 2; k++) KitchenProps.C(m, new Vector3(0f, 0.95f - k * 0.18f, 0.62f), new Vector3(0.4f, 0.1f, 0.1f), TPad, 0f, "Rodillo_pies");
        KitchenProps.Fit(m);
        GStation(m, GymMachineType.AbdominalDeclinado, copy, new Vector3(0f, 0.09f, 0.15f), 0.88f, default, -1f, 0f);   // tumbado sobre el banco, rodillas en el extremo alto
        GLabel(m, name);
    }

    /// <summary>BANCO DE HIPEREXTENSIONES 45° (0,8 × 1,3 m): almohadilla de cadera a ~0,95 m delante y rodillo de talones detrás.</summary>
    static void GymHyperext(Transform parent, string name, int copy, Vector3 pos)
    {
        Transform m = KitchenProps.Root(parent, "Maquina_" + name, pos);
        GFrame(m, 0.7f, -0.6f, 0.6f);
        GBar(m, new Vector3(0f, 0.08f, 0.6f), new Vector3(0f, 0.9f, -0.3f), 0.1f, TIron, "Viga_45");
        KitchenProps.C(m, new Vector3(0f, 0.95f, -0.32f), new Vector3(0.45f, 0.12f, 0.25f), TPad, new Vector3(45f, 0f, 0f), "Almohadilla_cadera");
        KitchenProps.C(m, new Vector3(0f, 0.28f, 0.42f), new Vector3(0.45f, 0.04f, 0.35f), TIron, new Vector3(-45f, 0f, 0f), "Plataforma_pies");
        KitchenProps.C(m, new Vector3(0f, 0.45f, 0.52f), new Vector3(0.4f, 0.1f, 0.1f), TPad, 0f, "Rodillo_talones");
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(m, new Vector3(s * 0.3f, 0.9f, -0.1f), new Vector3(0.04f, 0.14f, 0.04f), TChrome, 0f, "Asa");
        KitchenProps.Fit(m);
        GStation(m, GymMachineType.Hiperextension, copy, new Vector3(0f, 0f, -0.1f), 0.95f);
        GLabel(m, name);
    }

    /// <summary>ESPALDERAS: <paramref name="bodies"/> cuerpos de 0,9 × 2,4 m (peldaños cada 0,16 m), girados <paramref name="yaw"/>
    /// (0: se usan desde −Z). En cada cuerpo, un puesto de espaldas a las barras: elevación de piernas colgado del peldaño de
    /// 2,3 m. Las copias empiezan en <paramref name="firstCopy"/>.</summary>
    static void GWallBars(Transform parent, string name, Vector3 pos, float yaw, int bodies, int firstCopy)
    {
        Transform m = KitchenProps.Root(parent, name, pos, yaw);
        float w = bodies * 0.9f;
        for (int i = 0; i <= bodies; i++) KitchenProps.C(m, new Vector3(-w * 0.5f + i * 0.9f, 1.2f, 0f), new Vector3(0.06f, 2.4f, 0.15f), TWood, 0f, "Larguero");
        for (int i = 0; i < bodies; i++)
        {
            float x = -w * 0.5f + 0.45f + i * 0.9f;
            for (int k = 1; k <= 15; k++) KitchenProps.C(m, new Vector3(x, k * 0.16f, 0f), new Vector3(0.84f, 0.035f, 0.035f), TWood, 0f, "Peldano");
            GSpot(m, GymMachineType.ElevacionPiernas, firstCopy + i, new Vector3(x, 0f, -0.15f), 0f, 1.30f);   // espalda contra las barras, colgado del peldaño de 2,24 m
        }
        KitchenProps.Fit(m); GLabel(m, "Espalderas", 2.8f);
    }

    /// <summary>RIG de crossfit (1,8 × 10 m, 3,6 m de alto): 5 vanos de 2 m con barras de dominadas a 2,3 y 2,6 m a los dos
    /// lados (20 sitios), anillas en el vano central, J-hooks en el primero, 2 cuerdas de trepa de 4,57 m colgadas de una viga a
    /// 5,5 m en el último y 3 dianas de wall ball (3,05 y 2,74 m) en la cara +X.</summary>
    static void GymRig(Transform parent, Vector3 pos)
    {
        Transform m = KitchenProps.Root(parent, "Rig", pos);
        for (int j = 0; j <= 5; j++)
        {
            float z = -5f + j * 2f, top = j >= 4 ? 5.6f : 3.6f;   // los postes del último vano suben a la viga de las cuerdas
            for (int s = -1; s <= 1; s += 2) GBar(m, new Vector3(s * 0.9f, 0f, z), new Vector3(s * 0.9f, top, z), 0.1f, TIron, "Poste");
            GBar(m, new Vector3(-0.9f, 3.6f, z), new Vector3(0.9f, 3.6f, z), 0.1f, TIron, "Travesano");
            if (j >= 4) GBar(m, new Vector3(-0.9f, 5.5f, z), new Vector3(0.9f, 5.5f, z), 0.1f, TIron, "Travesano_cuerdas");
        }
        for (int s = -1; s <= 1; s += 2) GBar(m, new Vector3(s * 0.9f, 3.6f, -5f), new Vector3(s * 0.9f, 3.6f, 5f), 0.1f, TIron, "Larguero");
        GBar(m, new Vector3(0f, 5.5f, 3f), new Vector3(0f, 5.5f, 5f), 0.12f, TIron, "Viga_cuerdas");
        int kc = 0;
        for (int j = 0; j < 5; j++)
        {
            float z0 = -5f + j * 2f, h = j % 2 == 0 ? 2.3f : 2.6f;
            for (int s = -1; s <= 1; s += 2)
            {
                GBar(m, new Vector3(s * 0.9f, h, z0), new Vector3(s * 0.9f, h, z0 + 2f), 0.04f, TChrome, "Barra_dominadas");
                if (s < 0) for (int k = 0; k < 2; k++) GSpot(m, GymMachineType.Kipping, kc++, new Vector3(-0.9f, 0f, z0 + 0.5f + k), -90f, 1.35f + (h - 2.4f) / 0.974f);   // de cara al rig
            }
        }
        for (int s = -1; s <= 1; s += 2)
        {
            GBar(m, new Vector3(0f, 3.6f, s * 0.25f), new Vector3(0f, 2.5f, s * 0.25f), 0.015f, TDisc, "Cinta_anilla");
            KitchenProps.C(m, new Vector3(0f, 2.42f, s * 0.25f), new Vector3(0.18f, 0.18f, 0.03f), TWood, 90f, "Anilla");
            KitchenProps.C(m, new Vector3(s * 0.9f, 1.45f, -5.08f), new Vector3(0.08f, 0.08f, 0.08f), TAcc, 0f, "J_hook");
        }
        for (int c = 0; c < 2; c++)
        {
            float z = 3.5f + c;
            GBar(m, new Vector3(0f, 5.5f, z), new Vector3(0f, 0.93f, z), 0.04f, new Color(0.75f, 0.65f, 0.45f), "Cuerda_4,57");
            KitchenProps.C(m, new Vector3(0f, 4.5f, z), new Vector3(0.05f, 0.05f, 0.05f), TRed, 0f, "Marca");
            GSpot(m, GymMachineType.Cuerda, c, new Vector3(0.15f, 0f, z), 90f, 1.3f, new Vector3(0f, 0.6f, 0f));   // la cuerda, delante del pecho; sube 0,6 m por ciclo
        }
        float[] wz = { -3.5f, -1f, 1.5f };   // z 2,5 · 5,0 · 7,5 del piso
        for (int i = 0; i < 3; i++)
        {
            KitchenProps.C(m, new Vector3(0.96f, 3.05f, wz[i]), new Vector3(0.03f, 0.35f, 0.35f), TRed, 0f, "Diana_3,05");
            KitchenProps.C(m, new Vector3(0.96f, 2.74f, wz[i]), new Vector3(0.03f, 0.25f, 0.25f), TAcc, 0f, "Diana_2,74");
            GSpot(m, GymMachineType.WallBall, i, new Vector3(1.5f, 0f, wz[i]), 90f, 0.49f);   // a 0,6 m de la cara, mirando hacia −X
        }
        KitchenProps.Fit(m, false); GLabel(m, "Rig: dominadas, anillas, cuerdas y wall balls", 4.2f);
    }

    /// <summary>GHD (glute-ham developer, 0,8 × 1,8 m): almohadilla de cadera a ~1,0 m y pies enganchados delante (−Z).</summary>
    static void GymGHD(Transform parent, string name, int copy, Vector3 pos)
    {
        Transform m = KitchenProps.Root(parent, "Maquina_" + name, pos);
        GFrame(m, 0.7f, -0.9f, 0.9f);
        GBar(m, new Vector3(0f, 0.04f, 0.15f), new Vector3(0f, 0.9f, 0.15f), 0.08f, TIron, "Columna");
        KitchenProps.C(m, new Vector3(0f, 0.97f, 0.15f), new Vector3(0.45f, 0.14f, 0.4f), TPad, 0f, "Almohadilla_cadera");
        GBar(m, new Vector3(0f, 0.04f, -0.75f), new Vector3(0f, 0.6f, -0.7f), 0.07f, TIron, "Columna_pies");
        KitchenProps.C(m, new Vector3(0f, 0.5f, -0.72f), new Vector3(0.4f, 0.04f, 0.25f), TIron, new Vector3(-60f, 0f, 0f), "Plataforma_pies");
        for (int k = 0; k < 2; k++) KitchenProps.C(m, new Vector3(0f, 0.62f + k * 0.15f, -0.62f - k * 0.08f), new Vector3(0.4f, 0.1f, 0.1f), TPad, 0f, "Rodillo_tobillos");
        KitchenProps.Fit(m);
        GStation(m, GymMachineType.GHD, copy, new Vector3(0f, 0f, 0.15f), 1.21f);
        GLabel(m, name);
    }

    /// <summary>Una máquina de gimnasio COMO LAS DE VERDAD (docs/gimnasio-maquinas.md, «Qué cambiar» de cada una): cada tipo
    /// tiene su bastidor, su asiento o banco, sus palancas, rodillos y almohadillas, y su pila de placas en una torre con
    /// carcasa (el cable va DENTRO) o discos si es de carga libre. Nada cruza el sitio de la cabeza de quien la usa. Lleva un
    /// hijo vacío «Puesto» (dónde va la raíz del usuario y hacia dónde mira: −Z) y su <see cref="GymStation"/>.
    /// Origen en el centro de la base, a ras de suelo; X a la derecha, Y arriba, −Z es el frente.</summary>
    static void GymMachine(Transform parent, string name, int type, int copy, Vector3 pos, float approachSide = -1f)
    {
        Transform m = KitchenProps.Root(parent, "Maquina_" + name, pos);
        Color F = TIron, pad = new Color(0.12f, 0.12f, 0.14f), acc = type % 2 == 0 ? new Color(0.85f, 0.65f, 0.15f) : new Color(0.75f, 0.2f, 0.2f);
        Color disc = new Color(0.08f, 0.08f, 0.09f);
        Vector3 seat; float root; Vector3 shiftB = Vector3.zero;
        switch (type)
        {
            case 0:   // PRESS DE PECHO: respaldo reclinado 10°, palancas que pivotan arriba y detrás, torre baja (1,5 m)
                GFrame(m, 0.9f, -0.45f, 0.72f);
                GSeat(m, 0.05f, 0.48f, 0.45f, 0.42f, pad);
                GBack(m, new Vector3(0f, 0.55f, 0.24f), 0.75f, 10f, 0.42f, pad);
                GStack(m, new Vector3(0f, 0f, 0.55f), 1.5f, 0.40f, 0.25f, 1f);
                GBar(m, new Vector3(-0.38f, 1.45f, 0.47f), new Vector3(0.38f, 1.45f, 0.47f), 0.07f, F, "Eje_palancas");
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 piv = new Vector3(s * 0.35f, 1.45f, 0.30f), grip = new Vector3(s * 0.28f, 1.00f, -0.25f);
                    GBar(m, new Vector3(s * 0.35f, 1.45f, 0.47f), piv, 0.06f, F, "Soporte_pivote");
                    KitchenProps.C(m, piv, new Vector3(0.09f, 0.09f, 0.09f), acc, 0f, "Pivote");
                    GBar(m, piv, grip, 0.06f, F, "Palanca");
                    KitchenProps.C(m, grip, new Vector3(0.04f, 0.18f, 0.04f), TChrome, 0f, "Agarre_neutro");
                    KitchenProps.C(m, grip + new Vector3(-s * 0.08f, 0.09f, 0f), new Vector3(0.16f, 0.035f, 0.035f), TChrome, 0f, "Agarre_horizontal");
                }
                GBar(m, new Vector3(0f, 0.04f, -0.45f), new Vector3(0f, 0.09f, -0.30f), 0.05f, F, "Soporte_reposapies");
                KitchenProps.C(m, new Vector3(0f, 0.10f, -0.30f), new Vector3(0.5f, 0.05f, 0.1f), F, 0f, "Reposapies");
                seat = new Vector3(0f, 0f, 0.05f); root = 0.60f; break;

            case 1:   // PRESS DE HOMBROS: respaldo alto casi vertical (8°), palancas desde arriba y detrás
                GFrame(m, 0.9f, -0.40f, 0.78f);
                GSeat(m, 0.05f, 0.48f, 0.45f, 0.42f, pad);
                GBack(m, new Vector3(0f, 0.55f, 0.25f), 0.95f, 8f, 0.42f, pad);
                GStack(m, new Vector3(0f, 0f, 0.62f), 1.55f, 0.40f, 0.25f, 1f);
                GBar(m, new Vector3(-0.34f, 1.50f, 0.55f), new Vector3(0.34f, 1.50f, 0.55f), 0.07f, F, "Eje_palancas");
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 piv = new Vector3(s * 0.30f, 1.55f, 0.35f), grip = new Vector3(s * 0.25f, 1.18f, -0.10f);
                    GBar(m, new Vector3(s * 0.30f, 1.50f, 0.55f), piv, 0.06f, F, "Soporte_pivote");
                    KitchenProps.C(m, piv, new Vector3(0.09f, 0.09f, 0.09f), acc, 0f, "Pivote");
                    GBar(m, piv, grip, 0.06f, F, "Palanca");
                    KitchenProps.C(m, grip, new Vector3(0.04f, 0.18f, 0.04f), TChrome, 0f, "Agarre_neutro");
                }
                GBar(m, new Vector3(0f, 0.04f, -0.40f), new Vector3(0f, 0.09f, -0.28f), 0.05f, F, "Soporte_reposapies");
                KitchenProps.C(m, new Vector3(0f, 0.10f, -0.28f), new Vector3(0.5f, 0.05f, 0.1f), F, 0f, "Reposapies");
                seat = new Vector3(0f, 0f, 0.05f); root = 0.60f; break;

            case 2:   // JALÓN AL PECHO: torre DELANTE, polea alta, cable visible, barra de 1,2 m, rodillos de muslo, sin respaldo
                GFrame(m, 0.6f, -0.75f, 0.32f);
                GStack(m, new Vector3(0f, 0f, -0.55f), 2.2f, 0.45f, 0.30f, -1f);
                GBar(m, new Vector3(0f, 2.17f, -0.55f), new Vector3(0f, 2.17f, -0.09f), 0.08f, F, "Viga_superior");
                KitchenProps.C(m, new Vector3(0f, 2.10f, -0.12f), new Vector3(0.05f, 0.10f, 0.10f), TChrome, 0f, "Polea");
                GBar(m, new Vector3(0f, 2.05f, -0.10f), new Vector3(0f, 1.82f, -0.08f), 0.012f, F, "Cable");   // delante de la cara y por encima de la cabeza
                KitchenProps.C(m, new Vector3(0f, 1.80f, -0.08f), new Vector3(0.8f, 0.035f, 0.035f), TChrome, 0f, "Barra_jalon");
                for (int s = -1; s <= 1; s += 2)
                    GBar(m, new Vector3(s * 0.4f, 1.80f, -0.08f), new Vector3(s * (0.4f + 0.2f * Mathf.Cos(20f * Mathf.Deg2Rad)), 1.80f - 0.2f * Mathf.Sin(20f * Mathf.Deg2Rad), -0.08f), 0.035f, acc, "Punta_barra");
                GSeat(m, 0.05f, 0.48f, 0.40f, 0.35f, pad);
                GBar(m, new Vector3(0f, 0.04f, -0.32f), new Vector3(0f, 0.66f, -0.18f), 0.06f, F, "Soporte_rodillos");
                for (int s = -1; s <= 1; s += 2) KitchenProps.C(m, new Vector3(s * 0.17f, 0.68f, -0.18f), new Vector3(0.26f, 0.10f, 0.10f), pad, 0f, "Rodillo_muslo");
                seat = new Vector3(0f, 0f, 0.05f); root = 0.60f; break;

            case 3:   // REMO SENTADO: torre delante, almohadilla de pecho, brazos de remo con agarres neutros, reposapiés
                GFrame(m, 0.6f, -0.80f, 0.36f);
                GStack(m, new Vector3(0f, 0f, -0.65f), 1.8f, 0.45f, 0.30f, -1f);
                GBar(m, new Vector3(0f, 1.15f, -0.50f), new Vector3(0f, 1.15f, -0.19f), 0.06f, F, "Soporte_almohadilla");
                KitchenProps.C(m, new Vector3(0f, 1.15f, -0.15f), new Vector3(0.30f, 0.40f, 0.08f), pad, 0f, "Almohadilla_pecho");
                GSeat(m, 0.10f, 0.48f, 0.40f, 0.35f, pad);
                for (int s = -1; s <= 1; s += 2)
                {
                    GBar(m, new Vector3(s * 0.15f, 0.04f, -0.52f), new Vector3(s * 0.15f, 0.16f, -0.44f), 0.04f, F, "Soporte_reposapies");
                    KitchenProps.C(m, new Vector3(s * 0.15f, 0.22f, -0.42f), new Vector3(0.16f, 0.20f, 0.03f), F, new Vector3(-25f, 0f, 0f), "Reposapies");
                    GBar(m, new Vector3(s * 0.20f, 1.60f, -0.48f), new Vector3(s * 0.20f, 1.05f, -0.55f), 0.05f, F, "Brazo_remo");
                    KitchenProps.C(m, new Vector3(s * 0.20f, 1.05f, -0.55f), new Vector3(0.04f, 0.16f, 0.04f), TChrome, 0f, "Agarre_neutro");
                }
                KitchenProps.C(m, new Vector3(0f, 1.60f, -0.48f), new Vector3(0.48f, 0.07f, 0.07f), acc, 0f, "Pivote");
                seat = new Vector3(0f, 0f, 0.10f); root = 0.60f; break;

            case 4:   // PRENSA 45°: de discos (sin torre), raíles a 45°, carro con plataforma perpendicular, respaldo a 50°
            {
                GFrame(m, 1.1f, -1.22f, 0.90f, -1.15f, -0.15f);
                Vector3 d = new Vector3(0f, 1f, -1f).normalized, n = new Vector3(0f, 1f, 1f).normalized;
                Quaternion rot = Quaternion.LookRotation(d, n);
                for (int s = -1; s <= 1; s += 2)
                {
                    GBar(m, new Vector3(s * 0.40f, 0.35f, -0.15f), new Vector3(s * 0.40f, 1.35f, -1.15f), 0.08f, TChrome, "Rail_45");
                    GBar(m, new Vector3(s * 0.40f, 0.04f, -1.15f), new Vector3(s * 0.40f, 1.35f, -1.15f), 0.08f, F, "Poste_rail");
                    GBar(m, new Vector3(s * 0.40f, 0.04f, -0.15f), new Vector3(s * 0.40f, 0.35f, -0.15f), 0.08f, F, "Poste_rail");
                }
                Vector3 c = new Vector3(0f, 0.75f, -0.55f);   // centro de la plataforma, abajo (rodilla a 90°)
                Vector3 sled = c + d * 0.25f + n * 0.06f;
                KitchenProps.C(m, sled, new Vector3(0.9f, 0.06f, 0.5f), F, rot, "Carro");
                KitchenProps.C(m, c, new Vector3(0.75f, 0.55f, 0.05f), acc, rot, "Plataforma_pies");
                for (int s = -1; s <= 1; s += 2)
                {
                    KitchenProps.C(m, sled + new Vector3(s * 0.60f, 0f, 0f), new Vector3(0.30f, 0.05f, 0.05f), TChrome, rot, "Cuerno");
                    for (int i = 0; i < 2; i++) KitchenProps.C(m, sled + new Vector3(s * (0.55f + i * 0.08f), 0f, 0f), new Vector3(0.05f, 0.45f, 0.45f), disc, rot, "Disco");
                    GBar(m, new Vector3(s * 0.32f, 0.04f, 0.25f), new Vector3(s * 0.32f, 0.43f, 0.25f), 0.04f, F, "Poste_asa");
                    KitchenProps.C(m, new Vector3(s * 0.32f, 0.45f, 0.25f), new Vector3(0.04f, 0.04f, 0.18f), TChrome, 0f, "Asa");
                    KitchenProps.C(m, new Vector3(s * 0.34f, 0.55f, 0.12f), new Vector3(0.03f, 0.20f, 0.03f), acc, new Vector3(-30f, 0f, 0f), "Tope_seguridad");
                }
                GBar(m, new Vector3(0f, 0.04f, 0.35f), new Vector3(0f, 0.32f, 0.35f), 0.08f, F, "Columna_asiento");
                KitchenProps.C(m, new Vector3(0f, 0.36f, 0.35f), new Vector3(0.45f, 0.08f, 0.45f), pad, new Vector3(15f, 0f, 0f), "Asiento");
                GBack(m, new Vector3(0f, 0.45f, 0.30f), 0.85f, 50f, 0.45f, pad);
                seat = new Vector3(0f, 0.42f, 0.35f); root = 0.52f; break;
            }

            case 5:   // EXTENSIÓN DE CUÁDRICEPS: pivote alineado con la rodilla, rodillo sobre el tobillo, pila a un lado
                GFrame(m, 0.6f, -0.40f, 0.55f);
                GSeat(m, 0.10f, 0.50f, 0.45f, 0.42f, pad);
                GBack(m, new Vector3(0f, 0.55f, 0.33f), 0.75f, 12f, 0.42f, pad);
                GBar(m, new Vector3(0.32f, 0.04f, -0.15f), new Vector3(0.32f, 0.53f, -0.24f), 0.05f, F, "Soporte_pivote");
                KitchenProps.C(m, new Vector3(0.32f, 0.56f, -0.25f), new Vector3(0.08f, 0.08f, 0.08f), acc, 0f, "Pivote_rodilla");
                GBar(m, new Vector3(0.32f, 0.56f, -0.25f), new Vector3(0.32f, 0.20f, -0.32f), 0.05f, F, "Palanca");
                KitchenProps.C(m, new Vector3(0.26f, 0.20f, -0.32f), new Vector3(0.12f, 0.03f, 0.03f), TChrome, 0f, "Eje_rodillo");
                KitchenProps.C(m, new Vector3(0f, 0.20f, -0.32f), new Vector3(0.40f, 0.12f, 0.12f), pad, 0f, "Rodillo_espinilla");
                for (int s = -1; s <= 1; s += 2) KitchenProps.C(m, new Vector3(s * 0.28f, 0.50f, 0.05f), new Vector3(0.04f, 0.04f, 0.20f), TChrome, 0f, "Asa");
                GStack(m, new Vector3(0.55f, 0f, 0.15f), 1.4f, 0.30f, 0.25f, 1f);
                seat = new Vector3(0f, 0f, 0.15f); root = 0.62f; break;

            case 6:   // CURL FEMORAL TUMBADO: banco en tres tramos con joroba en la cadera, rodillo tras los tobillos
                GFrame(m, 0.6f, -0.85f, 0.60f);
                KitchenProps.C(m, new Vector3(0f, 0.58f, -0.475f), new Vector3(0.32f, 0.08f, 0.75f), pad, 0f, "Banco_pecho");
                KitchenProps.C(m, new Vector3(0f, 0.63f, 0.075f), new Vector3(0.32f, 0.10f, 0.35f), pad, 0f, "Banco_joroba");
                KitchenProps.C(m, new Vector3(0f, 0.62f, 0.375f), new Vector3(0.32f, 0.08f, 0.25f), pad, 0f, "Banco_muslos");
                GBar(m, new Vector3(0f, 0.04f, -0.60f), new Vector3(0f, 0.54f, -0.60f), 0.07f, F, "Pata");
                GBar(m, new Vector3(0f, 0.04f, 0.30f), new Vector3(0f, 0.58f, 0.30f), 0.07f, F, "Pata");
                GBar(m, new Vector3(0.30f, 0.04f, 0.58f), new Vector3(0.30f, 0.67f, 0.58f), 0.05f, F, "Soporte_pivote");
                KitchenProps.C(m, new Vector3(0.30f, 0.70f, 0.58f), new Vector3(0.08f, 0.08f, 0.08f), acc, 0f, "Pivote_rodilla");
                GBar(m, new Vector3(0.30f, 0.70f, 0.58f), new Vector3(0.30f, 0.74f, 0.98f), 0.05f, F, "Palanca");
                KitchenProps.C(m, new Vector3(0.26f, 0.74f, 0.98f), new Vector3(0.12f, 0.03f, 0.03f), TChrome, 0f, "Eje_rodillo");
                KitchenProps.C(m, new Vector3(0f, 0.74f, 0.98f), new Vector3(0.40f, 0.12f, 0.12f), pad, 0f, "Rodillo_tobillo");
                for (int s = -1; s <= 1; s += 2)
                {
                    GBar(m, new Vector3(s * 0.22f, 0.04f, -0.80f), new Vector3(s * 0.22f, 0.43f, -0.80f), 0.04f, F, "Poste_asidero");
                    KitchenProps.C(m, new Vector3(s * 0.22f, 0.45f, -0.80f), new Vector3(0.04f, 0.04f, 0.16f), TChrome, 0f, "Asidero");
                }
                GStack(m, new Vector3(-0.55f, 0f, 0f), 1.5f, 0.30f, 0.25f, 1f);
                seat = new Vector3(0f, 0.62f, 0.10f); root = 0.81f; break;

            case 7:   // HIP THRUST: almohadilla de espalda baja, asiento bajo, plataforma de pies, palanca con almohadilla de cadera y discos
                GFrame(m, 1.0f, -0.80f, 0.50f);
                for (int s = -1; s <= 1; s += 2) GBar(m, new Vector3(s * 0.40f, 0.04f, 0.40f), new Vector3(s * 0.40f, 0.23f, 0.40f), 0.06f, F, "Pata_almohadilla");
                KitchenProps.C(m, new Vector3(0f, 0.33f, 0.40f), new Vector3(1.0f, 0.20f, 0.20f), pad, 0f, "Almohadilla_espalda");
                GBar(m, new Vector3(0f, 0.04f, 0f), new Vector3(0f, 0.10f, 0f), 0.08f, F, "Soporte_asiento");
                KitchenProps.C(m, new Vector3(0f, 0.14f, 0f), new Vector3(0.45f, 0.08f, 0.35f), pad, 0f, "Asiento_bajo");
                KitchenProps.C(m, new Vector3(0f, 0.07f, -0.55f), new Vector3(0.9f, 0.05f, 0.45f), F, new Vector3(10f, 0f, 0f), "Plataforma_pies");
                for (int s = -1; s <= 1; s += 2)
                {
                    GBar(m, new Vector3(s * 0.55f, 0.04f, 0.40f), new Vector3(s * 0.55f, 0.40f, 0.40f), 0.08f, F, "Poste_pivote");
                    GBar(m, new Vector3(s * 0.55f, 0.40f, 0.40f), new Vector3(s * 0.30f, 0.40f, -0.10f), 0.06f, F, "Brazo_palanca");
                    KitchenProps.C(m, new Vector3(s * 0.62f, 0.40f, 0.40f), new Vector3(0.14f, 0.05f, 0.05f), TChrome, 0f, "Cuerno");
                    for (int i = 0; i < 2; i++) KitchenProps.C(m, new Vector3(s * (0.61f + i * 0.05f), 0.40f, 0.40f), new Vector3(0.04f, 0.45f, 0.45f), disc, 0f, "Disco");
                }
                KitchenProps.C(m, new Vector3(0f, 0.40f, -0.10f), new Vector3(0.6f, 0.15f, 0.15f), pad, 0f, "Almohadilla_cadera");
                seat = new Vector3(0f, 0.05f, -0.10f); root = 0.25f; shiftB = new Vector3(0f, 0f, 0.10f); break;   // en B la pelvis avanza 0,10 m (hacia −Z de la máquina)

            case 8:   // GEMELOS SENTADO: asiento corto sin respaldo, almohadilla sobre las rodillas, calzo para las puntas
                GFrame(m, 0.5f, -0.70f, 0.28f);
                GSeat(m, 0.05f, 0.55f, 0.35f, 0.35f, pad);
                KitchenProps.C(m, new Vector3(0f, 0.06f, -0.47f), new Vector3(0.50f, 0.12f, 0.15f), F, 0f, "Calzo_puntas");
                GBar(m, new Vector3(0f, 0.04f, -0.65f), new Vector3(0f, 0.22f, -0.65f), 0.08f, F, "Soporte_pivote");
                KitchenProps.C(m, new Vector3(0f, 0.25f, -0.65f), new Vector3(0.55f, 0.07f, 0.07f), acc, 0f, "Pivote");
                for (int s = -1; s <= 1; s += 2) GBar(m, new Vector3(s * 0.25f, 0.25f, -0.65f), new Vector3(s * 0.25f, 0.72f, -0.32f), 0.05f, F, "Palanca");
                KitchenProps.C(m, new Vector3(0f, 0.72f, -0.32f), new Vector3(0.42f, 0.08f, 0.15f), pad, 0f, "Almohadilla_rodillas");
                KitchenProps.C(m, new Vector3(0.33f, 0.72f, -0.32f), new Vector3(0.16f, 0.05f, 0.05f), TChrome, 0f, "Cuerno");
                KitchenProps.C(m, new Vector3(0.37f, 0.72f, -0.32f), new Vector3(0.04f, 0.45f, 0.45f), disc, 0f, "Disco");
                GBar(m, new Vector3(-0.25f, 0.72f, -0.32f), new Vector3(-0.30f, 0.86f, -0.28f), 0.03f, acc, "Maneta_seguridad");
                seat = new Vector3(0f, 0f, 0.05f); root = 0.67f; break;

            default:   // BÍCEPS Y TRÍCEPS: apoyo de brazos inclinado 40°, levas alineadas con el codo, pila detrás a la derecha
            {
                GFrame(m, 0.6f, -0.65f, 0.55f);
                GSeat(m, 0.10f, 0.52f, 0.42f, 0.40f, pad);
                Vector3 lo = new Vector3(0f, 0.85f, -0.35f), hi = new Vector3(0f, 1.05f, -0.10f);
                GBar(m, new Vector3(0f, 0.04f, -0.32f), new Vector3(0f, 0.90f, -0.25f), 0.07f, F, "Columna_apoyo");
                KitchenProps.C(m, (lo + hi) * 0.5f, new Vector3(0.50f, 0.06f, 0.35f), pad, Quaternion.LookRotation((hi - lo).normalized, Vector3.up), "Apoyo_brazos");
                for (int s = -1; s <= 1; s += 2)
                {
                    GBar(m, new Vector3(s * 0.30f, 0.04f, -0.45f), new Vector3(s * 0.30f, 0.82f, -0.39f), 0.05f, F, "Soporte_leva");
                    KitchenProps.C(m, new Vector3(s * 0.30f, 0.88f, -0.38f), new Vector3(0.04f, 0.16f, 0.16f), acc, new Vector3(45f, 0f, 0f), "Leva");
                    GBar(m, new Vector3(s * 0.30f, 0.88f, -0.38f), new Vector3(s * 0.27f, 0.62f, -0.55f), 0.045f, F, "Palanca");
                }
                KitchenProps.C(m, new Vector3(0f, 0.62f, -0.55f), new Vector3(0.56f, 0.04f, 0.04f), TChrome, 0f, "Barra_agarre");
                GStack(m, new Vector3(0.45f, 0f, 0.40f), 1.4f, 0.30f, 0.25f, 1f);
                seat = new Vector3(0f, 0f, 0.10f); root = 0.64f; break;
            }
        }
        KitchenProps.Fit(m);
        Transform pu = new GameObject("Puesto").transform; pu.SetParent(m, false);
        pu.localPosition = seat; pu.localRotation = Quaternion.Euler(0f, 180f, 0f);   // mira hacia −Z (el frente de la máquina)
        GymStation st = m.gameObject.AddComponent<GymStation>();
        st.type = (GymMachineType)type; st.copy = copy; st.puesto = pu; st.rootHeight = root; st.shiftB = shiftB; st.approachSide = approachSide;
        WorldLabel wl = m.gameObject.AddComponent<WorldLabel>(); wl.text = name; wl.height = 2.4f;
    }

    /// <summary>Barra (cubo alargado) de <paramref name="a"/> a <paramref name="b"/>, de sección <paramref name="t"/>.</summary>
    static Transform GBar(Transform m, Vector3 a, Vector3 b, float t, Color c, string name)
    {
        Vector3 d = b - a; float len = d.magnitude;
        Vector3 dir = len > 1e-4f ? d / len : Vector3.forward;
        Quaternion q = Quaternion.LookRotation(dir, Mathf.Abs(dir.y) > 0.99f ? Vector3.forward : Vector3.up);
        return KitchenProps.C(m, (a + b) * 0.5f, new Vector3(t, t, Mathf.Max(len, t)), c, q, name);
    }

    /// <summary>Bastidor en el suelo: dos largueros de ancho <paramref name="w"/> entre z0 y z1, con travesaños en los extremos
    /// (y en <paramref name="extra"/>).</summary>
    static void GFrame(Transform m, float w, float z0, float z1, params float[] extra)
    {
        for (int s = -1; s <= 1; s += 2) GBar(m, new Vector3(s * w * 0.5f, 0.04f, z0), new Vector3(s * w * 0.5f, 0.04f, z1), 0.08f, TIron, "Larguero");
        GBar(m, new Vector3(-w * 0.5f, 0.04f, z0), new Vector3(w * 0.5f, 0.04f, z0), 0.08f, TIron, "Travesano");
        GBar(m, new Vector3(-w * 0.5f, 0.04f, z1), new Vector3(w * 0.5f, 0.04f, z1), 0.08f, TIron, "Travesano");
        foreach (float z in extra) GBar(m, new Vector3(-w * 0.5f, 0.04f, z), new Vector3(w * 0.5f, 0.04f, z), 0.08f, TIron, "Travesano");
    }

    /// <summary>Asiento con la parte de arriba a <paramref name="top"/> m, sobre su columna.</summary>
    static void GSeat(Transform m, float z, float top, float w, float d, Color pad)
    {
        GBar(m, new Vector3(0f, 0.04f, z), new Vector3(0f, top - 0.08f, z), 0.07f, TIron, "Columna_asiento");
        KitchenProps.C(m, new Vector3(0f, top - 0.04f, z), new Vector3(w, 0.08f, d), pad, 0f, "Asiento");
    }

    /// <summary>Respaldo de <paramref name="len"/> m desde su borde de abajo, reclinado <paramref name="recline"/>° (la parte de
    /// arriba hacia +Z, lejos del frente), con su soporte desde el bastidor.</summary>
    static void GBack(Transform m, Vector3 bottom, float len, float recline, float w, Color pad)
    {
        float r = recline * Mathf.Deg2Rad;
        Vector3 up = new Vector3(0f, Mathf.Cos(r), Mathf.Sin(r));
        Vector3 c = bottom + up * (len * 0.5f);
        KitchenProps.C(m, c, new Vector3(w, len, 0.08f), pad, new Vector3(recline, 0f, 0f), "Respaldo");
        Vector3 back = new Vector3(0f, Mathf.Sin(r), -Mathf.Cos(r)) * -0.06f;   // detrás del cojín
        GBar(m, new Vector3(0f, 0.04f, bottom.z + 0.05f), c + back, 0.06f, TIron, "Soporte_respaldo");
    }

    /// <summary>Torre de placas con CARCASA (guías, tapa y panel) de <paramref name="h"/> m: el cable va dentro. El panel queda
    /// del lado <paramref name="panelSide"/> en Z (+1 detrás, −1 delante), lejos de quien entrena.</summary>
    static void GStack(Transform m, Vector3 at, float h, float w, float d, float panelSide)
    {
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(m, at + new Vector3(s * (w * 0.5f - 0.02f), h * 0.5f, 0f), new Vector3(0.04f, h, d), TIron, 0f, "Carcasa");
        KitchenProps.C(m, at + new Vector3(0f, h - 0.03f, 0f), new Vector3(w, 0.06f, d), TIron, 0f, "Tapa");
        KitchenProps.C(m, at + new Vector3(0f, h * 0.5f, panelSide * (d * 0.5f - 0.01f)), new Vector3(w, h, 0.02f), TIron, 0f, "Panel");
        int plates = Mathf.Clamp(Mathf.FloorToInt((h - 0.6f) / 0.06f), 6, 13);
        for (int i = 0; i < plates; i++) KitchenProps.C(m, at + new Vector3(0f, 0.08f + i * 0.06f, 0f), new Vector3(w - 0.1f, 0.05f, d * 0.6f), Color.black, 0f, "Placa");
        float topPlate = 0.08f + plates * 0.06f;
        KitchenProps.C(m, at + new Vector3(0f, (topPlate + h - 0.06f) * 0.5f, 0f), new Vector3(0.012f, h - 0.06f - topPlate, 0.012f), new Color(0.2f, 0.2f, 0.22f), 0f, "Cable");
    }
    /// <summary>OBRA: paredes a medio levantar, andamio y el material de UN día de trabajo a pie de obra.</summary>
    static void MakeConstructionSite(Transform area, Vector2 size, string title, int days)
    {
        float hx = size.x * 0.5f, hz = size.y * 0.5f;
        Transform scaf = KitchenProps.Root(area, "Andamio_obra", new Vector3(-hx + 1.4f, 0.1f, -hz + 1f));
        for (int i = 0; i < 4; i++) KitchenProps.C(scaf, new Vector3(i % 2 == 0 ? -0.9f : 0.9f, 1.5f, i < 2 ? -0.4f : 0.4f), new Vector3(0.05f, 3f, 0.05f), TChrome, 0f, "Tubo");
        for (int y = 0; y < 2; y++) KitchenProps.C(scaf, new Vector3(0f, 1.4f + y * 1.4f, 0f), new Vector3(1.9f, 0.05f, 0.85f), TWood, 0f, "Plataforma");
        Transform mat = KitchenProps.Root(area, "Material_del_dia", new Vector3(hx - 2f, 0.1f, 1f));
        KitchenProps.C(mat, new Vector3(-0.7f, 0.06f, 0f), new Vector3(1.1f, 0.12f, 1.1f), TWood, 0f, "Pale");
        for (int y = 0; y < 4; y++) for (int i = 0; i < 4; i++)
            KitchenProps.C(mat, new Vector3(-1.08f + i * 0.25f, 0.18f + y * 0.1f, 0f), new Vector3(0.22f, 0.09f, 1f), new Color(0.62f, 0.5f, 0.38f), 0f, "Barro_con_paja");
        KitchenProps.C(mat, new Vector3(0.7f, 0.06f, 0f), new Vector3(1.1f, 0.12f, 1.1f), TWood, 0f, "Pale");
        for (int y = 0; y < 3; y++) KitchenProps.C(mat, new Vector3(0.7f, 0.2f + y * 0.15f, 0f), new Vector3(0.95f, 0.14f, 0.45f), new Color(0.93f, 0.9f, 0.82f), 0f, "Panel_micelio");
        for (int i = 0; i < 5; i++) KitchenProps.C(mat, new Vector3(0f, 0.08f + i * 0.1f, 1.1f), new Vector3(2.8f, 0.09f, 0.09f), new Color(0.78f, 0.74f, 0.42f), 0f, "Guadua");
        WorldLabel ml = mat.gameObject.AddComponent<WorldLabel>(); ml.text = "Material de un día de obra: 4 guaduas, barro con paja, 2 paneles de micelio, cuerda"; ml.height = 1f;
        for (int i = 0; i < 3; i++) KitchenProps.C(area, new Vector3(-1f + i * 0.8f, 0.3f, hz - 0.6f), new Vector3(0.25f, 0.4f, 0.25f), new Color(0.95f, 0.45f, 0.15f), 45f, "Cono");
        ConstructionProject cp = area.gameObject.AddComponent<ConstructionProject>();
        cp.title = title; cp.daysRequired = days;
        WorldLabel wl = area.gameObject.AddComponent<WorldLabel>(); wl.text = $"{title} — EN OBRAS (0/{days} días)"; wl.height = 4.5f;
    }
}
