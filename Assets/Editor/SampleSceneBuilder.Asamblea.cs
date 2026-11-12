using UnityEngine;

/// <summary>
/// LA SALA DE ASAMBLEAS (Berón 2026-11-05: «crea la sala de asamblea, asegúrate de investigar todo lo referente a diseño (época/lugar) … en el santuario se crean las
/// áreas como mix de lo mejor del mundo»; docs/piso-asambleas.md). El piso que esperaban Angostura, Onondaga, Madrid, Gettysburg y la Feria. En la torre de Servicios,
/// encima de la Carne Cultivada (nivel 4 de la hélice: mismas caras que el nivel 2, así que no cambia la planta de la torre). Mezcla, con el origen en cada pieza:
/// - el CÍRCULO sin cabecera: la casa larga haudenosaunee, la kgotla de Botsuana, el panchayat (nadie preside desde lo alto: filas en cuadrado, mirando al centro);
/// - la PIEDRA DE LA PALABRA: el Lögberg del Althing islandés (quien habla, de pie en el centro; 12 cm con su rampa: sin escalones, S5);
/// - el CONO DE LUZ: el de la cúpula del Reichstag (luz arriba, a la vista de todos);
/// - la TOGUNA de los dogon: techo bajo de listones para mediar sentados (no se discute de pie);
/// - el KLEROTERION y la CLEPSIDRA de Atenas: sortear quién modera y medir el turno de palabra;
/// - la CABINA DE INTÉRPRETES de la ONU (cristal doble, como en Música), el MARCADOR de votos y el ARCHIVO con quipu, cuentas de concha y libro;
/// - los dos RETOÑOS: el roble de Gernika y la higuera de Bengala del panchayat.
/// Caben TODOS los miembros a la vez (J16): <see cref="AssemblySeats"/>; 2 m de paso (S7); el umbral del tronco (franja de 4,6 m) queda libre (F5).
/// </summary>
public static partial class SampleSceneBuilder
{
    public const string AssemblyArea = "Assembly_Area", AssemblyTitle = "Sala de asambleas";
    /// <summary>Plazas por fila de cada anillo: 4 filas de 8 y 4 de 10 (72 plazas).</summary>
    public const int AssemblyInnerSeats = 8, AssemblyOuterSeats = 10;
    public const int AssemblySeats = 4 * AssemblyInnerSeats + 4 * AssemblyOuterSeats;
    /// <summary>Distancia del centro a cada anillo de filas: 3 m de paso entre ellos, y 2 m en las esquinas.</summary>
    public const float AssemblyInnerRing = 6f, AssemblyOuterRing = 9.75f;

    static readonly Color AsmOak = new Color(0.48f, 0.34f, 0.22f), AsmStone = new Color(0.45f, 0.44f, 0.42f), AsmCream = new Color(0.86f, 0.82f, 0.72f), AsmGlass = new Color(0.72f, 0.82f, 0.88f), AsmShell = new Color(0.88f, 0.86f, 0.8f), AsmMaroon = new Color(0.42f, 0.16f, 0.2f);

    /// <summary>Una fila de asientos que mira al centro: <paramref name="yaw"/> gira su respaldo (hacia +z) hacia fuera.</summary>
    static void AssemblyRow(Transform g, string name, Vector3 at, int seats, float yaw)
    {
        Transform row = KitchenProps.Root(g, name, at);
        KitchenProps.C(row, new Vector3(0f, 0.225f, 0f), new Vector3(seats * 0.7f, 0.45f, 0.5f), AsmOak, 0f, "Silla_asiento");
        KitchenProps.C(row, new Vector3(0f, 0.65f, 0.27f), new Vector3(seats * 0.7f, 0.5f, 0.06f), AsmOak, 0f, "Silla_respaldo");
        KitchenProps.Fit(row);
        row.localRotation = Quaternion.Euler(0f, yaw, 0f);
        AreaSeat.Add(row.gameObject, "asamblea", AreaSeat.Category.Ocio, seats);
    }

    static SanctuaryArea BuildAssembly(Transform areasGroup)
    {
        SanctuaryArea a = BuildAreaBox(areasGroup, AssemblyArea, AssemblyTitle, SanctuaryAreaType.Assembly, 1, ServiciosCenter, ServiciosSize, new Color(0.62f, 0.48f, 0.34f));
        Vector2 s = TowerFloor(a.transform);
        FurnishAssembly(a.transform, s);
        WorldLabel wl = a.gameObject.AddComponent<WorldLabel>(); wl.text = "Sala de asambleas: lo mejor de las asambleas del mundo"; wl.height = 4f;
        Debug.Log($"[Asamblea] {AssemblySeats} plazas para {WorkdayDirector.Staff.Length} miembros.");
        return a;
    }

    static void FurnishAssembly(Transform floor, Vector2 s)
    {
        Transform g = new GameObject("Planta_asambleas").transform; g.SetParent(floor, false); g.localPosition = Vector3.up * 0.1f;
        Solid(g, new Vector3(0f, -0.05f, 0f), new Vector3(s.x - 0.2f, 0.1f, s.y - 0.2f), new Color(0.5f, 0.4f, 0.3f), "Suelo_de_madera");
        float hx = s.x * 0.5f, hz = s.y * 0.5f;
        Vector3 c = new Vector3(0f, 0f, 2f);

        // EL CENTRO: la piedra de la palabra, bajo el cono de luz (un solo mueble).
        Transform centre = KitchenProps.Root(g, "Piedra de la palabra (Lögberg) y cono de luz (Reichstag)", c);
        KitchenProps.C(centre, new Vector3(0f, 0.06f, 0f), new Vector3(2.4f, 0.12f, 2.4f), AsmStone, 0f, "Piedra_de_la_palabra");
        KitchenProps.C(centre, new Vector3(0f, 0.04f, -1.9f), new Vector3(1.2f, 0.08f, 1.4f), AsmStone, 0f, "Rampa_de_la_piedra");
        for (int x = -1; x <= 1; x += 2) for (int z = -1; z <= 1; z += 2) KitchenProps.C(centre, new Vector3(x * 3f, 2.2f, z * 3f), new Vector3(0.1f, 4.4f, 0.1f), LabSteel, 0f, "Poste_fino");
        KitchenProps.C(centre, new Vector3(0f, 4.45f, 0f), new Vector3(6.1f, 0.08f, 6.1f), AsmGlass, 0f, "Aro_de_luz");
        for (int k = 0; k < 4; k++) KitchenProps.C(centre, new Vector3(0f, 4.2f - k * 0.3f, 0f), new Vector3(3f - k * 0.7f, 0.5f, 3f - k * 0.7f), AsmGlass, 0f, "Cono_de_espejos");
        KitchenProps.Fit(centre);
        WorldLabel cw = centre.gameObject.AddComponent<WorldLabel>(); cw.text = "Piedra de la palabra"; cw.height = 5f;

        // LOS DOS ANILLOS DE FILAS, en cuadrado: mirando todos al centro; nadie en lo alto, nadie detrás de nadie.
        (Vector3 dir, float yaw)[] sides = { (Vector3.forward, 0f), (Vector3.right, 90f), (Vector3.back, 180f), (Vector3.left, 270f) };
        for (int k = 0; k < 4; k++)
        {
            AssemblyRow(g, $"Fila interior {k + 1} ({AssemblyInnerSeats} plazas)", c + sides[k].dir * AssemblyInnerRing, AssemblyInnerSeats, sides[k].yaw);
            AssemblyRow(g, $"Fila exterior {k + 1} ({AssemblyOuterSeats} plazas)", c + sides[k].dir * AssemblyOuterRing, AssemblyOuterSeats, sides[k].yaw);
        }

        // LA TOGUNA de los dogon (−X, norte): postes y listones bajos: para mediar hay que sentarse.
        Transform toguna = KitchenProps.Root(g, "Toguna de mediación (dogon)", new Vector3(-15.8f, 0f, 10f));
        for (int x = -1; x <= 1; x++) for (int z = -1; z <= 1; z += 2) KitchenProps.C(toguna, new Vector3(x * 2.4f, 0.7f, z * 2f), new Vector3(0.28f, 1.4f, 0.28f), AsmOak, 0f, "Poste_tallado");
        for (int k = 0; k < 9; k++) KitchenProps.C(toguna, new Vector3(0f, 1.46f, -2f + k * 0.5f), new Vector3(5.4f, 0.1f, 0.14f), AsmOak, 0f, "Liston_de_la_toguna");
        for (int z = -1; z <= 1; z += 2) KitchenProps.C(toguna, new Vector3(0f, 0.22f, z * 1.1f), new Vector3(3.6f, 0.44f, 0.5f), AsmOak, 0f, "Silla_banco_bajo");
        KitchenProps.Fit(toguna);
        AreaSeat.Add(toguna.gameObject, "mediar", AreaSeat.Category.Trabajo, 6);
        WorldLabel tw = toguna.gameObject.AddComponent<WorldLabel>(); tw.text = "Toguna de mediación"; tw.height = 2.2f;

        // LA MESA DE LA COMISIÓN (−X, centro): donde se prepara el orden del día.
        Transform table = KitchenProps.Root(g, "Mesa de la comisión", new Vector3(-15.8f, 0f, 0f));
        Bench(table, 3.4f, 1.4f, AsmOak);
        KitchenProps.C(table, new Vector3(-0.9f, 0.95f, 0f), new Vector3(0.6f, 0.04f, 0.4f), AsmCream, 0f, "Papeles_del_orden_del_dia");
        KitchenProps.Fit(table);
        AreaSeat.Add(table.gameObject, "orden del día", AreaSeat.Category.Trabajo, 4);
        WorldLabel mw = table.gameObject.AddComponent<WorldLabel>(); mw.text = "Mesa de la comisión"; mw.height = 2f;

        // EL ARCHIVO (−X, sur): el acta en cuerdas, en conchas y en papel.
        Transform archive = KitchenProps.Root(g, "Archivo de actas", new Vector3(-15.8f, 0f, -8.5f));
        Bench(archive, 3.6f, 0.8f, AsmOak);
        for (int k = 0; k < 7; k++) KitchenProps.C(archive, new Vector3(-1.4f + k * 0.2f, 1.25f, 0.3f), new Vector3(0.03f, 0.7f, 0.03f), AsmCream, 0f, "Quipu_cuerda");
        KitchenProps.C(archive, new Vector3(0.2f, 1.2f, 0.3f), new Vector3(0.9f, 0.3f, 0.04f), AsmShell, 0f, "Cinturon_de_cuentas_de_concha");
        KitchenProps.C(archive, new Vector3(1.2f, 0.98f, 0f), new Vector3(0.7f, 0.1f, 0.5f), AsmMaroon, 0f, "Libro_de_actas");
        KitchenProps.Fit(archive);
        AreaSeat.Add(archive.gameObject, "acta", AreaSeat.Category.Trabajo, 2);
        WorldLabel aw = archive.gameObject.AddComponent<WorldLabel>(); aw.text = "Archivo de actas"; aw.height = 2.2f;

        // EL MARCADOR DE VOTOS (+X, norte).
        Transform board = KitchenProps.Root(g, "Marcador de votos", new Vector3(16f, 0f, 10f));
        KitchenProps.C(board, new Vector3(0f, 1.9f, 0f), new Vector3(4f, 2.2f, 0.16f), LabDark, 0f, "Pantalla");
        for (int x = -1; x <= 1; x += 2) KitchenProps.C(board, new Vector3(x * 1.8f, 0.4f, 0f), new Vector3(0.14f, 0.8f, 0.14f), LabSteel, 0f, "Pie_del_marcador");
        KitchenProps.Fit(board);
        WorldLabel bw = board.gameObject.AddComponent<WorldLabel>(); bw.text = "Marcador de votos"; bw.height = 3.4f;

        // LA CABINA DE INTÉRPRETES (+X, centro), de doble cristal.
        Vector3 booth = new Vector3(16f, 0f, 0f);
        Transform desk = KitchenProps.Root(g, "Cabina de intérpretes (ONU)", booth);
        Bench(desk, 2.6f, 0.8f, AsmOak);
        for (int k = -1; k <= 1; k += 2) KitchenProps.C(desk, new Vector3(k * 0.7f, 1f, 0f), new Vector3(0.1f, 0.2f, 0.1f), LabDark, 0f, "Microfono");
        KitchenProps.Fit(desk);
        AreaSeat.Add(desk.gameObject, "interpretar", AreaSeat.Category.Trabajo, 2);
        WorldLabel iw = desk.gameObject.AddComponent<WorldLabel>(); iw.text = "Cabina de intérpretes"; iw.height = 2f;
        SoundRoom(g, "Sonica_interpretes", booth, new Vector2(7.6f, 6.4f), 3);   // (2 m libres de la mesa a la piel de dentro, también a lo ancho)

        // EL KLEROTERION Y LA CLEPSIDRA de Atenas (+X, sur): sortear quién modera y medir los turnos.
        Transform lots = KitchenProps.Root(g, "Kleroterion y clepsidra (Atenas)", new Vector3(16f, 0f, -8.5f));
        Bench(lots, 3.4f, 1f, AsmStone);
        KitchenProps.C(lots, new Vector3(-0.8f, 1.5f, 0f), new Vector3(1.2f, 1.2f, 0.14f), AsmCream, 0f, "Kleroterion");
        for (int r = 0; r < 4; r++) for (int k = 0; k < 6; k++) KitchenProps.C(lots, new Vector3(-1.25f + k * 0.18f, 1.2f + r * 0.2f, -0.08f), new Vector3(0.1f, 0.03f, 0.02f), AsmStone, 0f, "Ranura");
        KitchenProps.C(lots, new Vector3(0.9f, 1.25f, 0f), new Vector3(0.5f, 0.5f, 0.5f), new Color(0.6f, 0.36f, 0.22f), 0f, "Clepsidra_jarra_alta");
        KitchenProps.C(lots, new Vector3(0.9f, 0.98f, 0.5f), new Vector3(0.4f, 0.3f, 0.3f), new Color(0.6f, 0.36f, 0.22f), 0f, "Clepsidra_jarra_baja");
        KitchenProps.Fit(lots);
        AreaSeat.Add(lots.gameObject, "sortear", AreaSeat.Category.Trabajo, 2);
        WorldLabel lw = lots.gameObject.AddComponent<WorldLabel>(); lw.text = "Kleroterion y clepsidra"; lw.height = 2.4f;

        // LOS DOS RETOÑOS (norte): el roble de Gernika y la higuera de Bengala (bajo las que se reúnen vascos e indios).
        (string name, Color leaf, float x)[] trees = { ("Retoño del roble de Gernika", new Color(0.3f, 0.48f, 0.26f), -5f), ("Retoño de higuera de Bengala (panchayat)", new Color(0.22f, 0.44f, 0.24f), 5f) };
        foreach (var (name, leaf, x) in trees)
        {
            Transform tree = KitchenProps.Root(g, name, new Vector3(x, 0f, 16.2f));
            KitchenProps.C(tree, new Vector3(0f, 0.3f, 0f), new Vector3(1.6f, 0.6f, 1.6f), new Color(0.45f, 0.3f, 0.22f), 0f, "Maceta");
            KitchenProps.C(tree, new Vector3(0f, 1.3f, 0f), new Vector3(0.22f, 1.6f, 0.22f), AsmOak, 0f, "Tronco");
            KitchenProps.C(tree, new Vector3(0f, 2.5f, 0f), new Vector3(1.8f, 1.4f, 1.8f), leaf, 0f, "Copa");
            KitchenProps.Fit(tree);
            WorldLabel w = tree.gameObject.AddComponent<WorldLabel>(); w.text = name; w.height = 3.4f;
        }
        Aseos(floor, new Vector3(-hx + 2.1f, 0.1f, hz - 0.9f), 180f, 1);
    }
}
