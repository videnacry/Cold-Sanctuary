using UnityEngine;

/// <summary>
/// EL ÁREA DE MÚSICA (Berón 2026-11-05: «si no existe área de música créala»; docs/piso-musica.md): el piso «Sala de grabación y escenario de
/// conciertos», encima del Aula en la torre del Entrenamiento (un árbol más de la hélice: <see cref="TowerFloor"/>). Al −X, el ESCENARIO con sus
/// instrumentos y las filas de butacas para TODOS los miembros (regla J16: el ocio «para todos»); al +X, la CABINA de voz de cristal (la pared de
/// cristal es la única permitida, en una sala que debe aislarse: S6), la mesa de mezclas, el banco del técnico y el rincón de ensayo.
/// El umbral de las puertas (x = 0) queda libre (F5) y hay 2 m de paso entre muebles; lo que va junto es un solo mueble.
/// </summary>
public static partial class SampleSceneBuilder
{
    public const string MusicArea = "Music_Area";
    /// <summary>Butacas por fila y filas del piso: 50 plazas. Si los miembros pasan de ahí, <see cref="AreaCapacity"/> lo señala y hace falta otro piso.</summary>
    public const int MusicRowSeats = 10, MusicRows = 5;
    static readonly Color StageBlack = new Color(0.16f, 0.15f, 0.16f), StageWood = new Color(0.58f, 0.43f, 0.3f), SeatRed = new Color(0.5f, 0.14f, 0.16f), CableBlack = new Color(0.1f, 0.1f, 0.11f);

    // Un tambor o caja de altavoz: piezas compartidas del escenario y del rincón de ensayo.
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

    /// <summary>EL PISO DE MÚSICA: el siguiente piso de la torre del Entrenamiento (encima del Aula).</summary>
    static SanctuaryArea BuildMusic(Transform areasGroup)
    {
        SanctuaryArea a = BuildAreaBox(areasGroup, MusicArea, "Música", SanctuaryAreaType.Music, 1, EntrenamientoCenter, GymSize, new Color(0.62f, 0.5f, 0.72f));
        Vector2 s = TowerFloor(a.transform);
        Transform g = new GameObject("Planta_musica").transform; g.SetParent(a.transform, false); g.localPosition = Vector3.up * 0.1f;
        Solid(g, new Vector3(0f, -0.05f, 0f), new Vector3(s.x - 0.2f, 0.1f, s.y - 0.2f), new Color(0.42f, 0.34f, 0.3f), "Suelo_de_madera");
        float hx = s.x * 0.5f, hz = s.y * 0.5f;

        // EL ESCENARIO (−X), con todo su equipo sobre la tarima: un solo mueble. Lejos del umbral de la puerta del tronco.
        Transform stage = KitchenProps.Root(g, "Escenario de conciertos", new Vector3(-10f, 0f, -hz + 3.4f));
        KitchenProps.C(stage, new Vector3(0f, 0.3f, 0f), new Vector3(10.5f, 0.6f, 4.8f), StageBlack, 0f, "Tarima");
        KitchenProps.C(stage, new Vector3(0f, 0.61f, 0f), new Vector3(10.3f, 0.02f, 4.6f), StageWood, 0f, "Tablas_de_la_tarima");
        Vector3 up = Vector3.up * 0.62f;
        DrumKit(stage, up + new Vector3(0f, 0f, 1.5f));
        UprightPiano(stage, up + new Vector3(-3.6f, 0f, 1.2f));
        GuitarAmp(stage, up + new Vector3(3.2f, 0f, 1.5f));
        for (int k = -1; k <= 1; k++) MicStandAt(stage, up + new Vector3(k * 2.4f, 0f, -1.4f));
        for (int k = -1; k <= 1; k += 2) Speaker(stage, up + new Vector3(k * 4.7f, 0f, 1.8f), "Altavoz_de_escenario");
        KitchenProps.Fit(stage);
        AreaSeat.Add(stage.gameObject, "tocar en concierto", AreaSeat.Category.Trabajo, 6);
        WorldLabel swl = stage.gameObject.AddComponent<WorldLabel>(); swl.text = "Escenario de conciertos"; swl.height = 3.4f;

        // LAS BUTACAS: filas de diez, un solo mueble cada una, con 2 m de paso (regla J16: caben todos los miembros a la vez).
        for (int r = 0; r < MusicRows; r++)
        {
            Transform row = KitchenProps.Root(g, $"Fila de sillas {r + 1} ({MusicRowSeats} plazas)", new Vector3(-8f, 0f, -3.6f + r * 2.8f));
            KitchenProps.C(row, new Vector3(0f, 0.22f, 0f), new Vector3(MusicRowSeats * 0.7f, 0.44f, 0.5f), SeatRed, 0f, "Silla_asiento");
            KitchenProps.C(row, new Vector3(0f, 0.65f, 0.27f), new Vector3(MusicRowSeats * 0.7f, 0.5f, 0.06f), SeatRed, 0f, "Silla_respaldo");
            KitchenProps.Fit(row);
            AreaSeat.Add(row.gameObject, "concierto", AreaSeat.Category.Ocio, MusicRowSeats);
        }

        // LA CABINA DE VOZ: la única pared, de cristal, porque debe aislarse (S6). Dos micrófonos dentro; el técnico la ve desde la mesa.
        Vector3 boothAt = new Vector3(11f, 0f, -hz + 5f);
        Transform booth = KitchenProps.Root(g, "Micrófonos de la cabina", boothAt);
        for (int k = -1; k <= 1; k += 2) MicStandAt(booth, new Vector3(k * 1.2f, 0f, 0f));
        KitchenProps.Fit(booth);
        AreaSeat.Add(booth.gameObject, "grabar", AreaSeat.Category.Trabajo, 2);
        GlassWall.Box(g, "Cabina_de_voz", boothAt, new Vector2(9f, 6f), 0f, 2);

        // LA MESA DE MEZCLAS, frente a la cabina: dos puestos.
        Transform desk = KitchenProps.Root(g, "Mesa de mezclas y monitores", new Vector3(11f, 0f, 0f));
        Bench(desk, 3.6f, 1f, new Color(0.3f, 0.3f, 0.33f));
        for (int k = 0; k < 12; k++) KitchenProps.C(desk, new Vector3(-1.5f + k * 0.27f, 0.97f, -0.1f), new Vector3(0.1f, 0.03f, 0.3f), k % 3 == 0 ? new Color(0.9f, 0.7f, 0.25f) : LabDark, 0f, "Fader");
        for (int k = -1; k <= 1; k += 2) KitchenProps.C(desk, new Vector3(k * 1.1f, 1.25f, 0.3f), new Vector3(0.3f, 0.45f, 0.3f), StageBlack, 0f, "Altavoz_monitor");
        KitchenProps.C(desk, new Vector3(0f, 1.25f, 0.3f), new Vector3(0.7f, 0.42f, 0.03f), LabDark, 0f, "Pantalla");
        KitchenProps.Fit(desk);
        AreaSeat.Add(desk.gameObject, "mezcla", AreaSeat.Category.Trabajo, 2);
        WorldLabel dwl = desk.gameObject.AddComponent<WorldLabel>(); dwl.text = "Mesa de mezclas"; dwl.height = 2f;

        // EL RINCÓN DE ENSAYO (todo un mueble) y el BANCO DEL TÉCNICO (cuerdas, cables, válvulas).
        Transform rehearsal = KitchenProps.Root(g, "Rincón de ensayo", new Vector3(11.5f, 0f, 5.6f));
        DrumKit(rehearsal, new Vector3(-2f, 0f, 0.2f));
        UprightPiano(rehearsal, new Vector3(1.4f, 0f, 0.3f));
        GuitarAmp(rehearsal, new Vector3(3.6f, 0f, -0.2f));
        KitchenProps.Fit(rehearsal);
        AreaSeat.Add(rehearsal.gameObject, "ensayar", AreaSeat.Category.Trabajo, 4);
        WorldLabel rwl = rehearsal.gameObject.AddComponent<WorldLabel>(); rwl.text = "Rincón de ensayo"; rwl.height = 2.2f;

        Transform repair = KitchenProps.Root(g, "Banco del técnico de sonido", new Vector3(5.5f, 0f, hz - 2.2f));
        Bench(repair, 3f, 0.9f, LabSteel);
        for (int k = 0; k < 4; k++) KitchenProps.C(repair, new Vector3(-1.1f + k * 0.7f, 1f, 0f), new Vector3(0.25f, 0.1f, 0.25f), CableBlack, 0f, "Rollo_de_cable");
        KitchenProps.Fit(repair);
        AreaSeat.Add(repair.gameObject, "sonido", AreaSeat.Category.Trabajo, 2);
        WorldLabel twl = repair.gameObject.AddComponent<WorldLabel>(); twl.text = "Banco del técnico"; twl.height = 2f;

        Aseos(a.transform, new Vector3(-hx + 2.1f, 0.1f, hz - 0.9f), 180f, 1);
        WorldLabel wl = a.gameObject.AddComponent<WorldLabel>(); wl.text = "Música: sala de grabación y escenario de conciertos"; wl.height = 4f;
        Debug.Log($"[Música] escenario, {MusicRows * MusicRowSeats} butacas para {WorkdayDirector.Staff.Length} miembros, cabina de cristal, mesa de mezclas y rincón de ensayo.");
        return a;
    }
}
