using UnityEngine;

/// <summary>
/// EL ÁREA DE MÚSICA (Berón 2026-11-05: «si no existe área de música créala» y «tendría que haber más de un escenario y estudio de grabación … para que varios
/// grupos puedan practicar simultáneamente … paredes transparentes para bloquear sonido»; docs/piso-musica.md). En la torre del Entrenamiento, encima del Aula:
/// - el PISO DE CONCIERTOS: el escenario grande con 50 butacas y, tras su mampara, el segundo escenario con 16;
/// - los PISOS DE ESTUDIOS (⌈miembros / <see cref="MusicRehearsalSeats"/>⌉): cada uno, 2 estudios de grabación (control + sala) y 4 salas de ensayo.
/// Cada sala que debe aislarse es una <see cref="SoundRoom"/>: doble cristal con cámara y tapa de cristal (<see cref="MusicAcoustics"/>). Todos pueden ensayar
/// a la vez (regla S13 = J16). El umbral de cada puerta del tronco (7,4 × 4,4 m) queda libre (F5) y hay 2 m de paso, dentro y fuera de cada sala.
/// </summary>
public static partial class SampleSceneBuilder
{
    public const string MusicArea = "Music_Area";
    /// <summary>Butacas por fila y filas del escenario grande: 50 plazas.</summary>
    public const int MusicRowSeats = 10, MusicRows = 5;
    /// <summary>Personas por banda en una sala de ensayo, y plazas de ensayo de cada piso de estudios (4 salas).</summary>
    public const int MusicBandSize = 5, MusicRehearsalSeats = 4 * MusicBandSize;
    /// <summary>Medida exterior de una sala de un piso de estudios (x, z), con su doble cristal: su banda + 2 m a cada lado.</summary>
    public static readonly Vector2 MusicCell = new Vector2(8.4f, 6.2f);

    /// <summary>Pisos de estudios: los que hagan falta para que todos los miembros ensayen a la vez (regla J16).</summary>
    public static int MusicStudioFloors() => Mathf.Max(1, Mathf.CeilToInt(WorkdayDirector.Staff.Length / (float)MusicRehearsalSeats));
    /// <summary>Pisos del área: el de conciertos y los de estudios.</summary>
    public static int MusicFloors() => 1 + MusicStudioFloors();

    static readonly Color StageBlack = new Color(0.16f, 0.15f, 0.16f), StageWood = new Color(0.58f, 0.43f, 0.3f), SeatRed = new Color(0.5f, 0.14f, 0.16f), CableBlack = new Color(0.1f, 0.1f, 0.11f);

    /// <summary>UNA SALA QUE AÍSLA EL SONIDO: dos cajas de cristal, una dentro de otra, con su cámara de aire, y una tapa de cristal (por arriba, una sala abierta
    /// no aísla nada: docs/piso-musica.md §Acústica). <paramref name="outer"/> es la medida de fuera.</summary>
    static void SoundRoom(Transform g, string name, Vector3 c, Vector2 outer, int doorSide)
    {
        float inset = MusicAcoustics.Cavity + GlassWall.Thick;
        Vector2 inner = outer - Vector2.one * (2f * inset);
        Leaf(GlassWall.Box(g, name + "_fuera", c, outer, 0f, doorSide), outer, doorSide, -1f);
        Leaf(GlassWall.Box(g, name + "_dentro", c, inner, 0f, doorSide), inner, doorSide, 1f);
        Transform lid = KitchenProps.C(g, new Vector3(c.x, GlassWall.Height + 0.03f, c.z), new Vector3(outer.x, 0.06f, outer.y), GlassWall.GlassTint, 0f, "Tapa_cristal_" + name);
        lid.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    // La hoja corredera del hueco de una piel (SoundDoor), dentro de la cámara de aire: <paramref name="toCavity"/> la aparta del plano del paño hacia la cámara.
    static void Leaf(Transform room, Vector2 size, int side, float toCavity)
    {
        bool alongX = side == 0 || side == 2;
        float sign = side == 0 || side == 1 ? 1f : -1f, half = (alongX ? size.y : size.x) * 0.5f - GlassWall.Thick * 0.5f, offset = half * sign + toCavity * sign * 0.06f;
        Vector3 at = (alongX ? new Vector3(0f, 0f, offset) : new Vector3(offset, 0f, 0f)) + Vector3.up * (GlassWall.DoorHeight * 0.5f);
        Vector3 scale = alongX ? new Vector3(GlassWall.DoorWidth, GlassWall.DoorHeight, 0.04f) : new Vector3(0.04f, GlassWall.DoorHeight, GlassWall.DoorWidth);
        Transform leaf = KitchenProps.C(room, at, scale, GlassWall.GlassTint, 0f, "Hoja_sonica");
        leaf.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        leaf.gameObject.AddComponent<SoundDoor>().slide = (alongX ? Vector3.right : Vector3.forward) * GlassWall.DoorWidth;
    }

    static void Speaker(Transform t, Vector3 at, string label)
    {
        KitchenProps.C(t, at + new Vector3(0f, 0.6f, 0f), new Vector3(0.7f, 1.2f, 0.6f), StageBlack, 0f, label);
        KitchenProps.C(t, at + new Vector3(0f, 0.8f, -0.31f), new Vector3(0.4f, 0.4f, 0.02f), LabDark, 0f, "Altavoz_grave");
        KitchenProps.C(t, at + new Vector3(0f, 0.3f, -0.31f), new Vector3(0.25f, 0.25f, 0.02f), LabDark, 0f, "Altavoz_agudos");
    }
    static void DrumKit(Transform t, Vector3 at)
    {
        KitchenProps.C(t, at + new Vector3(0f, 0.3f, 0.2f), new Vector3(0.6f, 0.6f, 0.45f), new Color(0.45f, 0.18f, 0.16f), 0f, "Tambor_bombo");
        for (int k = -1; k <= 1; k += 2)
        {
            KitchenProps.C(t, at + new Vector3(k * 0.3f, 0.7f, 0.1f), new Vector3(0.32f, 0.2f, 0.32f), new Color(0.45f, 0.18f, 0.16f), 0f, "Tambor");
            KitchenProps.C(t, at + new Vector3(k * 0.75f, 0.6f, 0f), new Vector3(0.04f, 1.2f, 0.04f), LabSteel, 0f, "Soporte_de_platillo");
            KitchenProps.C(t, at + new Vector3(k * 0.75f, 1.2f, 0f), new Vector3(0.45f, 0.02f, 0.45f), new Color(0.78f, 0.62f, 0.3f), 0f, "Platillo");
        }
        KitchenProps.C(t, at + new Vector3(0f, 0.25f, -0.6f), new Vector3(0.36f, 0.5f, 0.36f), StageBlack, 0f, "Taburete");
    }
    static void UprightPiano(Transform t, Vector3 at)
    {
        KitchenProps.C(t, at + new Vector3(0f, 0.65f, 0.15f), new Vector3(1.5f, 1.3f, 0.5f), StageBlack, 0f, "Piano");
        KitchenProps.C(t, at + new Vector3(0f, 0.75f, -0.2f), new Vector3(1.4f, 0.06f, 0.25f), new Color(0.92f, 0.92f, 0.88f), 0f, "Teclado");
        KitchenProps.C(t, at + new Vector3(0f, 0.25f, -0.65f), new Vector3(0.8f, 0.5f, 0.35f), StageWood, 0f, "Banqueta_del_piano");
    }
    static void MicStandAt(Transform t, Vector3 at)
    {
        KitchenProps.C(t, at + new Vector3(0f, 0.02f, 0f), new Vector3(0.3f, 0.04f, 0.3f), StageBlack, 0f, "Base_de_micro");
        KitchenProps.C(t, at + new Vector3(0f, 0.8f, 0f), new Vector3(0.03f, 1.56f, 0.03f), LabSteel, 0f, "Pie_de_micro");
        KitchenProps.C(t, at + new Vector3(0f, 1.62f, 0f), new Vector3(0.08f, 0.16f, 0.08f), StageBlack, 0f, "Microfono");
    }
    static void GuitarAmp(Transform t, Vector3 at)
    {
        KitchenProps.C(t, at + new Vector3(0f, 0.4f, 0f), new Vector3(0.8f, 0.8f, 0.4f), StageBlack, 0f, "Amplificador");
        KitchenProps.C(t, at + new Vector3(0.7f, 0.55f, 0f), new Vector3(0.36f, 1.1f, 0.09f), StageWood, 0f, "Guitarra_en_su_soporte");
    }

    // Un escenario (un solo mueble): tarima, equipo, altavoces y su RAMPA (0,6 m en 2,4 m: 14°, sin escalones: S5). El grande lleva batería, piano, amplificador y 3
    // micrófonos, con la rampa a −X; el pequeño, amplificador y 2 micrófonos, con la rampa hacia el público. La tarima y la rampa se pisan: la caja del mueble no bloquea.
    static Transform BuildStage(Transform g, string name, Vector3 at, Vector2 size, bool large)
    {
        Transform stage = KitchenProps.Root(g, name, at);
        KitchenProps.C(stage, new Vector3(0f, 0.3f, 0f), new Vector3(size.x, 0.6f, size.y), StageBlack, 0f, "Tarima").gameObject.AddComponent<BoxCollider>();
        float slope = Mathf.Atan2(0.6f, 2.4f) * Mathf.Rad2Deg, rampLen = Mathf.Sqrt(2.4f * 2.4f + 0.6f * 0.6f);
        Transform ramp = large
            ? KitchenProps.C(stage, new Vector3(-size.x * 0.5f - 1.2f, 0.3f, 0f), new Vector3(rampLen, 0.1f, 1.6f), StageWood, new Vector3(0f, 0f, slope), "Rampa_del_escenario")
            : KitchenProps.C(stage, new Vector3(size.x * 0.5f - 1f, 0.3f, size.y * 0.5f + 1.2f), new Vector3(1.6f, 0.1f, rampLen), StageWood, new Vector3(slope, 0f, 0f), "Rampa_del_escenario");
        ramp.gameObject.AddComponent<BoxCollider>();
        KitchenProps.C(stage, new Vector3(0f, 0.61f, 0f), new Vector3(size.x - 0.2f, 0.02f, size.y - 0.2f), StageWood, 0f, "Tablas_de_la_tarima");
        Vector3 up = Vector3.up * 0.62f;
        float back = -size.y * 0.5f + 0.9f, front = size.y * 0.5f - 0.6f;   // el público se sienta hacia +Z
        if (large) { DrumKit(stage, up + new Vector3(0f, 0f, back)); UprightPiano(stage, up + new Vector3(-3.6f, 0f, back)); }
        GuitarAmp(stage, up + new Vector3(large ? 3.2f : 0f, 0f, back));
        if (large) for (int k = -1; k <= 1; k++) MicStandAt(stage, up + new Vector3(k * 2.4f, 0f, front));
        else for (int k = -1; k <= 1; k += 2) MicStandAt(stage, up + new Vector3(k * 1.2f, 0f, front));
        for (int k = -1; k <= 1; k += 2) Speaker(stage, up + new Vector3(k * (size.x * 0.5f - 0.5f), 0f, back - 0.3f), "Altavoz_de_escenario");
        KitchenProps.Fit(stage, false);
        AreaSeat.Add(stage.gameObject, "tocar en concierto", AreaSeat.Category.Trabajo, large ? 6 : 4);
        WorldLabel wl = stage.gameObject.AddComponent<WorldLabel>(); wl.text = name; wl.height = 3.4f;
        return stage;
    }

    static void SeatRow(Transform g, string name, Vector3 at, int seats)
    {
        Transform row = KitchenProps.Root(g, name, at);
        KitchenProps.C(row, new Vector3(0f, 0.225f, 0f), new Vector3(seats * 0.7f, 0.45f, 0.5f), SeatRed, 0f, "Silla_asiento");
        KitchenProps.C(row, new Vector3(0f, 0.65f, 0.27f), new Vector3(seats * 0.7f, 0.5f, 0.06f), SeatRed, 0f, "Silla_respaldo");
        KitchenProps.Fit(row);
        AreaSeat.Add(row.gameObject, "concierto", AreaSeat.Category.Ocio, seats);
    }

    /// <summary>EL ÁREA DE MÚSICA: el piso de conciertos y, encima, sus pisos de estudios (plantas propias, como el Aula).</summary>
    static SanctuaryArea BuildMusic(Transform areasGroup)
    {
        SanctuaryArea a = BuildAreaBox(areasGroup, MusicArea, "Música", SanctuaryAreaType.Music, 1, EntrenamientoCenter, GymSize, new Color(0.62f, 0.5f, 0.72f));
        Vector2 s = TowerFloor(a.transform);
        FurnishConcertFloor(a.transform, s);
        int floors = MusicStudioFloors();
        for (int k = 0; k < floors; k++)
        {
            Transform f = new GameObject($"Planta_musica_{k + 2}").transform; f.SetParent(a.transform, false);
            FurnishStudioFloor(f, TowerFloor(f), k);
        }
        WorldLabel wl = a.gameObject.AddComponent<WorldLabel>(); wl.text = "Música: escenarios, estudios y salas de ensayo"; wl.height = 4f;
        Debug.Log($"[Música] {WorkdayDirector.Staff.Length} miembros: 2 escenarios, {MusicRows * MusicRowSeats + 16} butacas y {floors} piso(s) de estudios: {2 * floors} estudios y {4 * floors} salas de ensayo ({floors * MusicRehearsalSeats} plazas).");
        return a;
    }

    // EL PISO DE CONCIERTOS. Al −X, el escenario grande y sus butacas; al +X, tras su mampara, el pequeño. Entre los dos queda libre el umbral de la puerta (x ≈ 0).
    static void FurnishConcertFloor(Transform floor, Vector2 s)
    {
        Transform g = new GameObject("Planta_musica").transform; g.SetParent(floor, false); g.localPosition = Vector3.up * 0.1f;
        Solid(g, new Vector3(0f, -0.05f, 0f), new Vector3(s.x - 0.2f, 0.1f, s.y - 0.2f), new Color(0.42f, 0.34f, 0.3f), "Suelo_de_madera");
        float hx = s.x * 0.5f, hz = s.y * 0.5f;
        BuildStage(g, "Escenario de conciertos", new Vector3(-10f, 0f, -hz + 3.4f), new Vector2(10.5f, 4.8f), true);
        for (int r = 0; r < MusicRows; r++) SeatRow(g, $"Fila de sillas {r + 1} ({MusicRowSeats} plazas)", new Vector3(-8f, 0f, -3.6f + r * 2.8f), MusicRowSeats);
        // EL SEGUNDO ESCENARIO, en su sala de doble cristal: dos conciertos o dos ensayos generales a la vez, sin pisarse.
        Vector3 hall = new Vector3(10.5f, 0f, -2.2f);
        BuildStage(g, "Segundo escenario", new Vector3(hall.x, 0f, -7.9f), new Vector2(6f, 3f), false);
        for (int r = 0; r < 2; r++) SeatRow(g, $"Fila de sillas pequeña {r + 1} (8 plazas)", new Vector3(hall.x, 0f, -1.2f + r * 2.8f), 8);
        SoundRoom(g, "Sonica_escenario_2", hall, new Vector2(13f, 20.4f), 3);
        // EL BANCO DEL TÉCNICO (cuerdas, cables, válvulas).
        Transform repair = KitchenProps.Root(g, "Banco del técnico de sonido", new Vector3(5.5f, 0f, hz - 2.2f));
        Bench(repair, 3f, 0.9f, LabSteel);
        for (int k = 0; k < 4; k++) KitchenProps.C(repair, new Vector3(-1.1f + k * 0.7f, 1f, 0f), new Vector3(0.25f, 0.1f, 0.25f), CableBlack, 0f, "Rollo_de_cable");
        // (2026-11-05, regla S14) Instrumentos del mundo sobre el banco: sitar, charango, kora y shamisen.
        string[] world = { "Instrumento_sitar_India", "Instrumento_charango_Andes", "Instrumento_kora_Mali", "Instrumento_shamisen_Japon" };
        for (int k = 0; k < 4; k++) KitchenProps.C(repair, new Vector3(-1.1f + k * 0.7f, 1.04f, 0.28f), new Vector3(0.5f, 0.08f, 0.14f), StageWood, 0f, world[k]);
        KitchenProps.Fit(repair);
        AreaSeat.Add(repair.gameObject, "sonido", AreaSeat.Category.Trabajo, 2);
        WorldLabel twl = repair.gameObject.AddComponent<WorldLabel>(); twl.text = "Banco del técnico"; twl.height = 2f;
        Aseos(floor, new Vector3(-hx + 2.1f, 0.1f, hz - 0.9f), 180f, 1);
    }

    // UN PISO DE ESTUDIOS: la franja del tronco (4,6 m), libre para la puerta; dos filas de cuatro salas y un pasillo de 3 m entre ellas, y los aseos al fondo.
    // Fila del tronco: 4 salas de ensayo. Fila del fondo: 2 estudios, cada uno con su control y su sala de grabación, cristal con cristal.
    static void FurnishStudioFloor(Transform floor, Vector2 s, int index)
    {
        Transform g = new GameObject("Planta_estudios").transform; g.SetParent(floor, false); g.localPosition = Vector3.up * 0.1f;
        Solid(g, new Vector3(0f, -0.05f, 0f), new Vector3(s.x - 0.2f, 0.1f, s.y - 0.2f), new Color(0.42f, 0.34f, 0.3f), "Suelo_de_madera");
        float hx = s.x * 0.5f, hz = s.y * 0.5f, zSouth = -hz + 7.7f, zNorth = -hz + 16.9f;
        float X(int col) => -13.35f + col * 8.9f;
        for (int c = 0; c < 4; c++)
        {
            int n = index * 4 + c + 1;
            Vector3 at = new Vector3(X(c), 0f, zSouth);
            Transform band = KitchenProps.Root(g, $"Banda de ensayo {n}", at);
            DrumKit(band, new Vector3(-0.8f, 0f, 0.18f));
            GuitarAmp(band, new Vector3(0.9f, 0f, 0.18f));
            KitchenProps.Fit(band);
            AreaSeat.Add(band.gameObject, "ensayar", AreaSeat.Category.Entrenamiento, MusicBandSize);
            WorldLabel wl = band.gameObject.AddComponent<WorldLabel>(); wl.text = $"Sala de ensayo {n}"; wl.height = 2.4f;
            SoundRoom(g, $"Sonica_ensayo_{n}", at, MusicCell, 0);
        }
        for (int st = 0; st < 2; st++)
        {
            int n = index * 2 + st + 1;
            Vector3 control = new Vector3(X(st * 2), 0f, zNorth), take = new Vector3(X(st * 2 + 1), 0f, zNorth);
            Transform desk = KitchenProps.Root(g, $"Mesa de mezclas {n}", control);
            Bench(desk, 3.4f, 1f, new Color(0.3f, 0.3f, 0.33f));
            for (int k = 0; k < 12; k++) KitchenProps.C(desk, new Vector3(-1.45f + k * 0.26f, 0.97f, -0.1f), new Vector3(0.1f, 0.03f, 0.3f), k % 3 == 0 ? new Color(0.9f, 0.7f, 0.25f) : LabDark, 0f, "Fader");
            for (int k = -1; k <= 1; k += 2) KitchenProps.C(desk, new Vector3(k * 1.1f, 1.25f, 0.3f), new Vector3(0.3f, 0.45f, 0.3f), StageBlack, 0f, "Altavoz_monitor");
            KitchenProps.C(desk, new Vector3(0f, 1.25f, 0.3f), new Vector3(0.7f, 0.42f, 0.03f), LabDark, 0f, "Pantalla");
            KitchenProps.Fit(desk);
            AreaSeat.Add(desk.gameObject, "mezcla", AreaSeat.Category.Trabajo, 2);
            WorldLabel dw = desk.gameObject.AddComponent<WorldLabel>(); dw.text = $"Estudio {n}: control"; dw.height = 2f;
            SoundRoom(g, $"Sonica_control_{n}", control, MusicCell, 2);

            Transform booth = KitchenProps.Root(g, $"Cabina de grabación {n}", take);
            UprightPiano(booth, new Vector3(-0.9f, 0f, 0.2f));
            MicStandAt(booth, new Vector3(0.6f, 0f, -0.1f)); MicStandAt(booth, new Vector3(1.5f, 0f, -0.1f));
            KitchenProps.Fit(booth);
            AreaSeat.Add(booth.gameObject, "grabar", AreaSeat.Category.Trabajo, 4);
            WorldLabel bw = booth.gameObject.AddComponent<WorldLabel>(); bw.text = $"Estudio {n}: sala de grabación"; bw.height = 2.2f;
            SoundRoom(g, $"Sonica_grabacion_{n}", take, MusicCell, 2);
        }
        Aseos(floor, new Vector3(-hx + 2.1f, 0.1f, hz - 0.9f), 180f, 1);
    }
}
