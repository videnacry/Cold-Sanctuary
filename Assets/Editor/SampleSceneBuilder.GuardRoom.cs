using UnityEngine;

/// <summary>
/// LA SALA DE GUARDIA (docs/sala-de-guardia.md): el piso libre del nivel 4 de Servicios, a 130 m, el más alto libre, junto a la Sala de asambleas y encima del hospital.
/// No es un <see cref="SanctuaryArea"/>, como el piso de cardio: la jornada la conoce como un sitio (<see cref="GuardRoom"/>). Mezcla, con el origen en cada pieza:
/// - el MURO DE PANTALLAS y las filas de CONSOLAS: el control de misión de Houston;
/// - la MESA DE RADAR: los centros de tráfico marítimo (Hong Kong);
/// - la CARTA DE HIELO: la Patrulla Internacional del Hielo;
/// - el SISMÓGRAFO y el PUESTO DE RESPALDO: la agencia meteorológica de Japón (centros gemelos en Tokio y Osaka);
/// - el MAREÓGRAFO y la BOYA: las boyas DART del sistema de alerta del Pacífico.
/// El umbral del tronco (franja de 4,6 m en −Z) queda libre (F5) y los puntos de trabajo del centro también.
/// </summary>
public static partial class SampleSceneBuilder
{
    static readonly Color GrDesk = new Color(0.3f, 0.32f, 0.36f), GrScreen = new Color(0.1f, 0.24f, 0.28f), GrAlert = new Color(0.62f, 0.14f, 0.12f), GrGlow = new Color(0.3f, 0.85f, 0.6f);

    static void BuildGuardRoom(Transform areasGroup)
    {
        Transform floor = new GameObject(GuardRoom.FloorName).transform; floor.SetParent(areasGroup, false);
        Vector2 s = TowerFloor(floor);
        floor.gameObject.AddComponent<GuardRoom>();
        floor.gameObject.AddComponent<WorldLabel>().text = "Sala de guardia: icebergs, radar, roca, sismos y mar";
        FurnishGuardRoom(floor, s);
    }

    static Transform GrPiece(Transform g, string name, Vector3 at, string label, float labelHeight)
    {
        Transform t = KitchenProps.Root(g, name, at);
        WorldLabel w = t.gameObject.AddComponent<WorldLabel>(); w.text = label; w.height = labelHeight;
        return t;
    }

    static void GrConsoles(Transform g, string name, float z, int seats)
    {
        Transform row = KitchenProps.Root(g, name, new Vector3(0f, 0f, z));
        float L = seats * 3.2f;
        Bench(row, L, 1.2f, GrDesk);
        for (int i = 0; i < seats; i++)
        {
            float x = -L * 0.5f + (i + 0.5f) * 3.2f;
            KitchenProps.C(row, new Vector3(x, 1.25f, 0.3f), new Vector3(1.4f, 0.8f, 0.06f), GrScreen, 0f, "Monitor");
            KitchenProps.C(row, new Vector3(x, 0.92f, -0.15f), new Vector3(0.7f, 0.03f, 0.25f), KitchenProps.DarkSteel, 0f, "Teclado");
        }
        KitchenProps.Fit(row);
        AreaSeat.Add(row.gameObject, "vigilancia", AreaSeat.Category.Trabajo, seats);
    }

    static void FurnishGuardRoom(Transform floor, Vector2 s)
    {
        Transform g = new GameObject("Planta_guardia").transform; g.SetParent(floor, false); g.localPosition = Vector3.up * 0.1f;
        Solid(g, new Vector3(0f, -0.05f, 0f), new Vector3(s.x - 0.2f, 0.1f, s.y - 0.2f), new Color(0.34f, 0.36f, 0.4f), "Suelo_de_sala");
        float hx = s.x * 0.5f, hz = s.y * 0.5f;

        // EL MURO DE PANTALLAS (Houston): siete pantallas sueltas sobre postes, no una pared (S6). Un solo tramo.
        Transform wall = new GameObject("Muro_de_pantallas").transform; wall.SetParent(g, false); wall.gameObject.AddComponent<FurnitureLine>();
        wall.localPosition = new Vector3(0f, 0f, 14.5f);
        Material normal = KitchenProps.Mat(GrScreen), alert = KitchenProps.Mat(GrAlert);
        GuardWatch.Feed[] feeds = GuardWatch.Feeds;
        for (int i = 0; i < feeds.Length; i++)
        {
            Transform sc = KitchenProps.Root(wall, $"Pantalla_{feeds[i]}", new Vector3(-13.2f + i * 4.4f, 0f, 0f));
            KitchenProps.C(sc, new Vector3(0f, 0.9f, 0f), new Vector3(0.14f, 1.8f, 0.14f), KitchenProps.DarkSteel, 0f, "Poste");
            Transform panel = KitchenProps.C(sc, new Vector3(0f, 2.5f, 0f), new Vector3(3.6f, 2f, 0.1f), GrScreen, 0f, "Pantalla");
            KitchenProps.Fit(sc);
            WorldLabel w = sc.gameObject.AddComponent<WorldLabel>(); w.text = GuardWatch.Title(feeds[i]); w.height = 3.8f;
            GuardScreen gs = sc.gameObject.AddComponent<GuardScreen>();
            gs.feed = feeds[i]; gs.panel = panel.GetComponent<Renderer>(); gs.normal = normal; gs.alert = alert;
        }

        // LAS CONSOLAS (las filas del control de misión): miran al muro.
        GrConsoles(g, "Consolas_fila_1", 9.5f, 6);
        GrConsoles(g, "Consolas_fila_2", 5.5f, 6);

        // LA MESA DE RADAR (centros de tráfico marítimo): el barrido gira.
        Transform radar = GrPiece(g, "Mesa de radar (tráfico marítimo)", new Vector3(-14f, 0f, -3f), "Mesa de radar", 1.8f);
        KitchenProps.C(radar, new Vector3(0f, 0.45f, 0f), new Vector3(2.6f, 0.9f, 2.6f), GrDesk, 0f, "Mesa");
        KitchenProps.C(radar, new Vector3(0f, 0.92f, 0f), new Vector3(2.2f, 0.04f, 2.2f), GrScreen, 0f, "Pantalla_radar");
        Transform pivot = KitchenProps.Root(radar, "Barrido", new Vector3(0f, 0.96f, 0f));
        KitchenProps.C(pivot, new Vector3(0.55f, 0f, 0f), new Vector3(1.1f, 0.02f, 0.05f), GrGlow, 0f, "Haz");
        pivot.gameObject.AddComponent<GuardSpin>();
        KitchenProps.Fit(radar);
        AreaSeat.Add(radar.gameObject, "radar", AreaSeat.Category.Trabajo, 2);

        // LA CARTA DE HIELO (Patrulla Internacional del Hielo): el límite del hielo, a mano.
        Transform ice = GrPiece(g, "Carta de hielo (Patrulla Internacional del Hielo)", new Vector3(-7f, 0f, -3f), "Carta de hielo", 1.6f);
        Bench(ice, 3f, 1.6f, GrDesk);
        KitchenProps.C(ice, new Vector3(0f, 0.92f, 0f), new Vector3(2.7f, 0.02f, 1.3f), new Color(0.86f, 0.9f, 0.92f), 0f, "Carta");
        for (int i = 0; i < 5; i++) KitchenProps.C(ice, new Vector3(-1f + i * 0.5f, 0.97f, (i % 2 == 0 ? 0.3f : -0.2f)), new Vector3(0.12f, 0.08f, 0.1f), Color.white, 0f, "Marca_iceberg");
        KitchenProps.Fit(ice);
        AreaSeat.Add(ice.gameObject, "carta de hielo", AreaSeat.Category.Trabajo, 2);

        // EL SISMÓGRAFO (Japón): el tambor gira y la aguja escribe.
        Transform seis = GrPiece(g, "Sismógrafo (agencia meteorológica de Japón)", new Vector3(7f, 0f, -3f), "Sismógrafo", 1.8f);
        Bench(seis, 2.6f, 1f, GrDesk);
        Transform drum = KitchenProps.Root(seis, "Tambor", new Vector3(0f, 1.15f, 0f));
        KitchenProps.C(drum, Vector3.zero, new Vector3(1.6f, 0.4f, 0.4f), new Color(0.92f, 0.9f, 0.84f), 0f, "Papel");
        drum.gameObject.AddComponent<GuardSpin>().degreesPerSecond = new Vector3(20f, 0f, 0f);
        KitchenProps.C(seis, new Vector3(0.2f, 1.42f, 0f), new Vector3(0.03f, 0.18f, 0.03f), KitchenProps.DarkSteel, 0f, "Aguja");
        KitchenProps.Fit(seis);

        // EL MAREÓGRAFO Y LA BOYA (DART): el nivel del mar confirma o cancela el aviso.
        Transform sea = GrPiece(g, "Mareógrafo y boya DART (sistema del Pacífico)", new Vector3(14f, 0f, -3f), "Mareógrafo y boya", 2.8f);
        KitchenProps.C(sea, new Vector3(0f, 1.1f, 0f), new Vector3(0.5f, 2.2f, 0.5f), GrDesk, 0f, "Columna_de_nivel");
        KitchenProps.C(sea, new Vector3(0f, 1f, -0.27f), new Vector3(0.1f, 1.6f, 0.02f), GrGlow, 0f, "Nivel");
        KitchenProps.C(sea, new Vector3(1.3f, 0.35f, 0f), new Vector3(1.2f, 0.7f, 1.2f), new Color(0.9f, 0.7f, 0.15f), 0f, "Maqueta_boya");
        KitchenProps.C(sea, new Vector3(1.3f, 1.05f, 0f), new Vector3(0.1f, 0.7f, 0.1f), KitchenProps.DarkSteel, 0f, "Antena_boya");
        KitchenProps.Fit(sea);

        // LA RADIO, EL TABLERO Y EL PUESTO DE RESPALDO.
        Transform radio = GrPiece(g, "Radio VHF (tráfico marítimo)", new Vector3(-14f, 0f, -10f), "Radio VHF", 2.2f);
        KitchenProps.C(radio, new Vector3(0f, 0.9f, 0f), new Vector3(1.6f, 1.8f, 0.6f), GrDesk, 0f, "Armario_de_radio");
        for (int i = 0; i < 3; i++) KitchenProps.C(radio, new Vector3(0f, 0.5f + i * 0.45f, -0.32f), new Vector3(1.2f, 0.25f, 0.04f), GrScreen, 0f, "Radio");
        KitchenProps.Fit(radio);
        Transform board = GrPiece(g, "Tablero de guardia", new Vector3(-6f, 0f, -10f), "Tablero de guardia", 2.4f);
        KitchenProps.C(board, new Vector3(0f, 1.3f, 0f), new Vector3(2f, 1.4f, 0.1f), new Color(0.86f, 0.78f, 0.62f), 0f, "Tablero");
        KitchenProps.C(board, new Vector3(0f, 0.3f, 0f), new Vector3(0.14f, 0.6f, 0.14f), KitchenProps.DarkSteel, 0f, "Pie");
        KitchenProps.Fit(board);
        board.gameObject.AddComponent<GuardBoard>();
        Transform spare = GrPiece(g, "Puesto de respaldo (centros gemelos de Japón)", new Vector3(14f, 0f, -10f), "Puesto de respaldo", 1.8f);
        Bench(spare, 3.2f, 1.2f, GrDesk);
        KitchenProps.C(spare, new Vector3(0f, 1.25f, 0.3f), new Vector3(1.4f, 0.8f, 0.06f), GrScreen, 0f, "Monitor");
        KitchenProps.Fit(spare);
        AreaSeat.Add(spare.gameObject, "vigilancia", AreaSeat.Category.Trabajo, 2);

        Aseos(floor, new Vector3(-hx + 2.1f, 0.1f, hz - 0.9f), 180f, 1);
    }
}
