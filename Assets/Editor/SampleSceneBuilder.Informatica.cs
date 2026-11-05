using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LOS DOS PISOS NUEVOS (Berón 2026-11-04; docs/piso-informatica.md y docs/piso-aula.md), en el piso libre de su torre-árbol:
/// - INFORMÁTICA (torre de Ciencias, encima de Biología): el CLÚSTER DE IA LOCAL del santuario: dos filas de racks de GPU con
///   refrigeración líquida directa al chip en circuito cerrado (CDU en cada fila), red y almacenamiento, el intercambiador que da
///   el calor a la red de distrito (fotobiorreactores, frutales), el de agua de mar (el foco frío), las bombas, el depósito de
///   reposición (agua del tronco de la torre), la acometida (central nuclear y paneles), dos SAI con sus baterías de litio, la
///   extinción por gas inerte, los puestos de monitorización y los bancos de mantenimiento. El modelo vive en <see cref="AiCluster"/>.
/// - AULA (torre del Entrenamiento, encima del Dojo): un AULA de clases (24 plazas frente a la tarima y la pizarra) y una SALA DE
///   ESTUDIO con biblioteca (24 plazas en mesas de seis, estanterías y dos puestos del catálogo): estudio y entrenamiento sirven a
///   TODOS los miembros a la vez (regla de capacidad, <see cref="AreaCapacity"/>).
/// Con 2 m de paso entre muebles (los que van juntos, juntos: pupitres de una fila, la mesa y la pizarra, las estanterías).
/// </summary>
public static partial class SampleSceneBuilder
{
    public const string InformaticsArea = "Informatics_Area", ClassroomArea = "Classroom_Area";
    static readonly Color RackBlack = new Color(0.12f, 0.13f, 0.15f), HoseBlue = new Color(0.2f, 0.45f, 0.85f), HoseRed = new Color(0.85f, 0.25f, 0.2f), LedGreen = new Color(0.3f, 0.95f, 0.45f);

    // ══ INFORMÁTICA ═════════════════════════════════════════════════════════════════════════════════════════════════════════════

    /// <summary>Un rack de 42U: armario negro, puerta de malla, servidores con sus luces y los colectores del agua (azul, fría;
    /// roja, caliente) por detrás.</summary>
    static void Rack(Transform t, Vector3 at, string label, bool gpu)
    {
        Transform r = KitchenProps.Root(t, label, at);
        KitchenProps.C(r, new Vector3(0f, 1.1f, 0f), new Vector3(0.6f, 2.2f, 1.2f), RackBlack, 0f, "Armario_rack");
        KitchenProps.C(r, new Vector3(0f, 1.1f, -0.61f), new Vector3(0.56f, 2.1f, 0.02f), LabDark, 0f, "Puerta_de_malla");
        int n = gpu ? 4 : 8;
        for (int i = 0; i < n; i++)
        {
            float y = 0.25f + i * (gpu ? 0.45f : 0.24f);
            KitchenProps.C(r, new Vector3(0f, y, -0.6f), new Vector3(0.5f, gpu ? 0.36f : 0.08f, 0.02f), new Color(0.3f, 0.32f, 0.36f), 0f, gpu ? "Servidor_8_GPU" : "Bandeja");
            KitchenProps.C(r, new Vector3(0.2f, y, -0.62f), new Vector3(0.04f, 0.03f, 0.01f), LedGreen, 0f, "Led");
        }
        if (!gpu) return;
        KitchenProps.C(r, new Vector3(-0.22f, 1.1f, 0.62f), new Vector3(0.06f, 2f, 0.06f), HoseBlue, 0f, "Colector_frio");
        KitchenProps.C(r, new Vector3(0.22f, 1.1f, 0.62f), new Vector3(0.06f, 2f, 0.06f), HoseRed, 0f, "Colector_caliente");
    }

    static void GpuRow(Transform t, string row)
    {
        for (int i = 0; i < 4; i++) Rack(t, new Vector3(-1.6f + i * 0.65f, 0f, 0f), $"Rack_GPU_{row}{i + 1}", true);
        // La CDU (unidad de distribución de refrigerante): bombas, intercambiador de placas interno y filtros; separa el agua del
        // rack (limpia, con glicol) del circuito de la planta.
        Transform cdu = KitchenProps.Root(t, "CDU", new Vector3(1.4f, 0f, 0f));
        KitchenProps.C(cdu, new Vector3(0f, 1.1f, 0f), new Vector3(1.1f, 2.2f, 1.2f), new Color(0.8f, 0.82f, 0.85f), 0f, "CDU");
        KitchenProps.C(cdu, new Vector3(0.2f, 1.6f, -0.61f), new Vector3(0.35f, 0.25f, 0.02f), LabDark, 0f, "Pantalla_CDU");
        KitchenProps.C(cdu, new Vector3(-0.2f, 0.5f, -0.61f), new Vector3(0.3f, 0.3f, 0.02f), LabBlue, 0f, "Filtro");
        KitchenProps.C(t, new Vector3(-0.2f, 2.35f, 0.4f), new Vector3(3.6f, 0.08f, 0.08f), HoseBlue, 0f, "Tuberia_fria");
        KitchenProps.C(t, new Vector3(-0.2f, 2.45f, 0.5f), new Vector3(3.6f, 0.08f, 0.08f), HoseRed, 0f, "Tuberia_caliente");
    }

    static void HeatExchanger(Transform t, Color hot, Color cold, string label)   // intercambiador de placas con sus cuatro bocas
    {
        KitchenProps.C(t, new Vector3(0f, 0.15f, 0f), new Vector3(2.2f, 0.3f, 1.2f), LabSteel, 0f, "Bancada");
        KitchenProps.C(t, new Vector3(-0.3f, 1.0f, 0f), new Vector3(1.2f, 1.4f, 0.7f), new Color(0.75f, 0.78f, 0.8f), 0f, label);
        for (int i = 0; i < 6; i++) KitchenProps.C(t, new Vector3(-0.85f + i * 0.22f, 1.0f, -0.37f), new Vector3(0.04f, 1.3f, 0.02f), LabSteel, 0f, "Placa");
        KitchenProps.C(t, new Vector3(0.65f, 1.4f, -0.2f), new Vector3(0.7f, 0.12f, 0.12f), hot, 0f, "Boca_caliente");
        KitchenProps.C(t, new Vector3(0.65f, 0.6f, -0.2f), new Vector3(0.7f, 0.12f, 0.12f), cold, 0f, "Boca_fria");
        KitchenProps.C(t, new Vector3(0.65f, 1.4f, 0.2f), new Vector3(0.7f, 0.12f, 0.12f), hot * 0.8f, 0f, "Boca_caliente_2");
        KitchenProps.C(t, new Vector3(0.65f, 0.6f, 0.2f), new Vector3(0.7f, 0.12f, 0.12f), cold * 0.8f, 0f, "Boca_fria_2");
    }

    static void MonitorDesk(Transform t, int screens)
    {
        Bench(t, 1.6f, 1f, new Color(0.35f, 0.37f, 0.4f));
        for (int i = 0; i < screens; i++) KitchenProps.C(t, new Vector3(-0.5f + i * 0.5f, 1.25f, 0.3f), new Vector3(0.45f, 0.3f, 0.03f), LabDark, 0f, "Pantalla");
        KitchenProps.C(t, new Vector3(0f, 0.95f, -0.15f), new Vector3(0.45f, 0.02f, 0.15f), LabDark, 0f, "Teclado");
        KitchenProps.C(t, new Vector3(0f, 0.25f, -0.85f), new Vector3(0.5f, 0.5f, 0.5f), new Color(0.25f, 0.3f, 0.4f), 0f, "Silla");
    }

    /// <summary>EL PISO DE INFORMÁTICA: el siguiente piso de la torre de Ciencias (el libre, encima de Biología).</summary>
    static SanctuaryArea BuildInformatics(Transform areasGroup)
    {
        SanctuaryArea a = BuildAreaBox(areasGroup, InformaticsArea, "Informática", SanctuaryAreaType.Informatics, 3, CienciasCenter, CienciasSize, new Color(0.55f, 0.65f, 0.8f));
        Vector2 s = TowerFloor(a.transform);
        Transform g = new GameObject("Planta_informatica").transform; g.SetParent(a.transform, false); g.localPosition = Vector3.up * 0.1f;
        Solid(g, new Vector3(0f, -0.05f, 0f), new Vector3(s.x - 0.2f, 0.1f, s.y - 0.2f), new Color(0.78f, 0.8f, 0.83f), "Suelo_tecnico");
        var blocks = new List<LabBlock>
        {
            new LabBlock("Fila de racks de GPU A (4 racks + CDU)", 4.0f, 1.4f, t => GpuRow(t, "A")),
            new LabBlock("Fila de racks de GPU B (4 racks + CDU)", 4.0f, 1.4f, t => GpuRow(t, "B")),
            new LabBlock("Rack de red (InfiniBand 400G y Ethernet)", 0.8f, 1.4f, t => Rack(t, Vector3.zero, "Rack_red", false)),
            new LabBlock("Racks de almacenamiento (NVMe y copias)", 1.6f, 1.4f, t => { Rack(t, new Vector3(-0.35f, 0f, 0f), "Rack_almacenamiento_1", false); Rack(t, new Vector3(0.35f, 0f, 0f), "Rack_almacenamiento_2", false); }),
            new LabBlock("Intercambiador: calor a la red de distrito", 2.4f, 1.4f, t => HeatExchanger(t, HoseRed, new Color(0.95f, 0.6f, 0.2f), "Intercambiador_distrito")),
            new LabBlock("Intercambiador de agua de mar (foco frío)", 2.4f, 1.4f, t => HeatExchanger(t, HoseRed, new Color(0.15f, 0.35f, 0.55f), "Intercambiador_mar")),
            new LabBlock("Bombas del circuito (2 + 1 de reserva)", 2.0f, 1.0f, t => { for (int i = 0; i < 3; i++) { KitchenProps.C(t, new Vector3(-0.65f + i * 0.65f, 0.3f, 0f), new Vector3(0.45f, 0.6f, 0.7f), i == 2 ? LabSteel : LabBlue, 0f, i == 2 ? "Bomba_reserva" : "Bomba"); KitchenProps.C(t, new Vector3(-0.65f + i * 0.65f, 0.75f, 0f), new Vector3(0.3f, 0.3f, 0.3f), LabDark, 0f, "Motor"); } }),
            new LabBlock("Depósito de reposición (agua del tronco)", 1.4f, 1.4f, t => { Vessel(t, 0.55f, 1.6f, new Color(0.7f, 0.8f, 0.88f), "Deposito_reposicion"); KitchenProps.C(t, new Vector3(0f, 1.0f, -0.6f), new Vector3(0.12f, 0.6f, 0.02f), LabBlue, 0f, "Nivel"); }),
            new LabBlock("Cuadro de la acometida (central nuclear y paneles)", 2.4f, 0.8f, t => { KitchenProps.C(t, new Vector3(0f, 1.0f, 0f), new Vector3(2.4f, 2.0f, 0.6f), new Color(0.85f, 0.85f, 0.8f), 0f, "Cuadro_general"); KitchenProps.C(t, new Vector3(-0.6f, 1.5f, -0.31f), new Vector3(0.5f, 0.3f, 0.02f), LabYellow, 0f, "Rotulo_alta_tension"); KitchenProps.C(t, new Vector3(0.6f, 1.3f, -0.31f), new Vector3(0.4f, 0.25f, 0.02f), LabDark, 0f, "Analizador_de_red"); AreaSeat.Add(t.gameObject, "energía", AreaSeat.Category.Trabajo); }),
        };
        blocks.AddRange(Times(2, "SAI", 1.2f, 0.9f, t => { KitchenProps.C(t, new Vector3(0f, 0.95f, 0f), new Vector3(1.2f, 1.9f, 0.85f), new Color(0.25f, 0.27f, 0.3f), 0f, "SAI"); KitchenProps.C(t, new Vector3(0.3f, 1.5f, -0.44f), new Vector3(0.3f, 0.2f, 0.02f), LedGreen, 0f, "Pantalla_SAI"); AreaSeat.Add(t.gameObject, "energía", AreaSeat.Category.Trabajo); }));
        blocks.AddRange(Times(3, "Armario de baterías de litio", 0.8f, 0.8f, t => Tall(t, 0.8f, 0.75f, 1.9f, new Color(0.3f, 0.32f, 0.36f), "Baterias_litio", LedGreen)));
        blocks.Add(new LabBlock("Extinción por gas inerte", 1.6f, 0.6f, t => { for (int i = 0; i < 4; i++) KitchenProps.C(t, new Vector3(-0.6f + i * 0.4f, 0.8f, 0f), new Vector3(0.3f, 1.6f, 0.3f), LabRed, 45f, "Botella_gas_inerte"); }));
        // (2026-11-04, ronda 5) Huella de 2,3 m de fondo: la silla queda 0,85 m detrás de la mesa (con 1,0 m asomaba 0,6 m y el
        // depósito de reposición quedaba a 1,53 m del puesto 1, grupo 28).
        blocks.AddRange(Times(4, "Puesto de monitorización", 1.6f, 2.3f, t => { MonitorDesk(t, 3); AreaSeat.Add(t.gameObject, "sysadmin", AreaSeat.Category.Trabajo); }));
        blocks.AddRange(Times(2, "Banco de mantenimiento de la refrigeración", 2.0f, 0.9f, t => { Bench(t, 2f, 0.9f, LabBench); KitchenProps.C(t, new Vector3(-0.5f, 1.05f, 0f), new Vector3(0.4f, 0.2f, 0.3f), HoseBlue, 0f, "Manguera_y_racores"); KitchenProps.C(t, new Vector3(0.5f, 1.05f, 0f), new Vector3(0.35f, 0.15f, 0.35f), LabBlue, 0f, "Filtros_nuevos"); AreaSeat.Add(t.gameObject, "refrigeración", AreaSeat.Category.Trabajo); }));
        blocks.Add(new LabBlock("Armario de repuestos (GPU, ventiladores, mangueras)", 1.2f, 0.6f, t => Cabinet(t, 1.2f, new Color(0.4f, 0.42f, 0.46f), "Repuestos")));
        LogLeft("Informática", PackFloor(g, s, blocks, CarpetKeepOut(s)));

        // El MODELO del clúster y su pantalla de estado sobre el primer puesto de monitorización.
        AiCluster ai = a.gameObject.AddComponent<AiCluster>();
        ai.racks = 2; ai.serversPerRack = 4; ai.gridKW = 5000f;
        Transform desk = g.Find("Bloque_Puesto de monitorización 1");
        if (desk != null)
        {
            Transform scr = new GameObject("Pantalla_estado").transform; scr.SetParent(desk, false); scr.localPosition = new Vector3(0f, 0.6f, 0f);
            ai.monitor = scr.gameObject.AddComponent<WorldLabel>(); ai.monitor.height = 3.2f; ai.monitor.text = "Clúster de IA";
        }
        Aseos(a.transform, new Vector3(-s.x * 0.5f + 0.2f + 1.9f, 0.1f, s.y * 0.5f - 0.9f), 180f, 1);
        WorldLabel wl = a.gameObject.AddComponent<WorldLabel>(); wl.text = "Informática: el clúster de IA del santuario (refrigeración líquida, calor a la red de distrito)"; wl.height = 5f;
        return a;
    }

    // ══ AULA ════════════════════════════════════════════════════════════════════════════════════════════════════════════════════

    /// <summary>Una fila de <paramref name="n"/> pupitres con su silla detrás (van juntos: un solo mueble).</summary>
    static Transform DeskRow(Transform parent, string name, Vector3 at, int n, AreaSeat.Category cat, string activity)
    {
        Transform r = KitchenProps.Root(parent, name, at);
        Color wood = new Color(0.62f, 0.48f, 0.32f), seat = new Color(0.25f, 0.35f, 0.55f);
        float w = 1.2f, x0 = -(n - 1) * w * 0.5f;
        for (int i = 0; i < n; i++)
        {
            float x = x0 + i * w;
            KitchenProps.C(r, new Vector3(x, 0.74f, 0.3f), new Vector3(1.1f, 0.04f, 0.6f), wood, 0f, "Tablero_pupitre");
            KitchenProps.C(r, new Vector3(x, 0.36f, 0.55f), new Vector3(1.0f, 0.72f, 0.05f), wood * 0.85f, 0f, "Faldon");
            KitchenProps.C(r, new Vector3(x, 0.45f, -0.3f), new Vector3(0.45f, 0.05f, 0.45f), seat, 0f, "Asiento");
            KitchenProps.C(r, new Vector3(x, 0.22f, -0.3f), new Vector3(0.4f, 0.44f, 0.4f), LabSteel, 0f, "Patas");
            KitchenProps.C(r, new Vector3(x, 0.72f, -0.52f), new Vector3(0.45f, 0.5f, 0.04f), seat, 0f, "Respaldo");
        }
        KitchenProps.Fit(r);
        AreaSeat.Add(r.gameObject, activity, cat, n);
        WorldLabel wl = r.gameObject.AddComponent<WorldLabel>(); wl.text = name; wl.height = 1.6f;
        return r;
    }

    /// <summary>Una mesa de estudio con <paramref name="perSide"/> sillas a cada lado (un solo mueble).</summary>
    static Transform StudyTable(Transform parent, string name, Vector3 at, int perSide)
    {
        Transform r = KitchenProps.Root(parent, name, at);
        Color wood = new Color(0.55f, 0.4f, 0.28f), seat = new Color(0.35f, 0.45f, 0.3f);
        float len = perSide * 1.0f;
        KitchenProps.C(r, new Vector3(0f, 0.74f, 0f), new Vector3(len, 0.05f, 1.0f), wood, 0f, "Tablero_mesa");
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(r, new Vector3(s * (len * 0.5f - 0.1f), 0.36f, 0f), new Vector3(0.08f, 0.72f, 0.9f), wood * 0.8f, 0f, "Pata");
        for (int i = 0; i < perSide; i++)
            for (int s = -1; s <= 1; s += 2)
            {
                float x = -len * 0.5f + 0.5f + i;
                KitchenProps.C(r, new Vector3(x, 0.45f, s * 0.8f), new Vector3(0.45f, 0.05f, 0.45f), seat, 0f, "Asiento");
                KitchenProps.C(r, new Vector3(x, 0.72f, s * 1.02f), new Vector3(0.45f, 0.5f, 0.04f), seat, 0f, "Respaldo");
                KitchenProps.C(r, new Vector3(x, 0.77f, s * 0.25f), new Vector3(0.3f, 0.02f, 0.22f), new Color(0.95f, 0.93f, 0.85f), 0f, "Libro_abierto");
                KitchenProps.C(r, new Vector3(x + 0.3f, 0.9f, s * 0.35f), new Vector3(0.12f, 0.25f, 0.12f), new Color(1f, 0.92f, 0.7f), 0f, "Lampara_de_estudio");
            }
        KitchenProps.Fit(r);
        AreaSeat.Add(r.gameObject, "estudio", AreaSeat.Category.Estudio, perSide * 2);
        WorldLabel wl = r.gameObject.AddComponent<WorldLabel>(); wl.text = name; wl.height = 1.6f;
        return r;
    }

    /// <summary>Plazas de cada piso del Aula (4 filas de 6 pupitres y 4 mesas de estudio de 6).</summary>
    public const int AulaSeatsPerFloor = 24;

    /// <summary>Cuántos pisos de Aula hacen falta para que quepan TODOS los miembros a la vez (la lista del personal,
    /// <see cref="WorkdayDirector.Staff"/>; Berón 2026-11-04: si un piso no basta, otro piso, como el segundo de yoga).</summary>
    public static int AulaFloors() => Mathf.Max(1, Mathf.CeilToInt(WorkdayDirector.Staff.Length / (float)AulaSeatsPerFloor));

    /// <summary>EL AULA: el siguiente piso de la torre del Entrenamiento (el libre, encima del Dojo) y, si los miembros no caben en
    /// uno, los pisos siguientes («Planta_aula_2»…, plantas propias del Aula). En cada piso, aula de clases al −X y sala de estudio
    /// con biblioteca al +X. Las plazas salen de la lista del personal: en total ≥ miembros en clase y ≥ miembros en estudio.</summary>
    static SanctuaryArea BuildClassroom(Transform areasGroup)
    {
        SanctuaryArea a = BuildAreaBox(areasGroup, ClassroomArea, "Aula", SanctuaryAreaType.Classroom, 1, EntrenamientoCenter, GymSize, new Color(0.85f, 0.8f, 0.65f));
        Vector2 s = TowerFloor(a.transform);
        int members = WorkdayDirector.Staff.Length, floors = AulaFloors(), left = members;
        for (int k = 0; k < floors; k++)
        {
            Transform floor = a.transform;
            if (k > 0) { floor = new GameObject($"Planta_aula_{k + 1}").transform; floor.SetParent(a.transform, false); s = TowerFloor(floor); }
            int seats = Mathf.Clamp(left, 1, AulaSeatsPerFloor);
            if (k == floors - 1 && floors == 1) seats = AulaSeatsPerFloor;   // un solo piso: completo (sobra sitio para invitados)
            FurnishAulaFloor(floor, s, seats, k + 1);
            left -= seats;
        }
        Debug.Log($"[Aula] {members} miembros (lista del personal): {floors} piso(s) de Aula, {AulaSeatsPerFloor} plazas por piso en clase y en estudio.");
        WorldLabel wl = a.gameObject.AddComponent<WorldLabel>(); wl.text = "Aula: clases y sala de estudio (caben todos)"; wl.height = 4f;
        return a;
    }

    /// <summary>Un piso del Aula con <paramref name="seats"/> plazas en el aula y otras tantas en la sala de estudio (en múltiplos de 6).</summary>
    static void FurnishAulaFloor(Transform floor, Vector2 s, int seats, int number)
    {
        Transform g = new GameObject("Planta_aula").transform; g.SetParent(floor, false); g.localPosition = Vector3.up * 0.1f;
        Solid(g, new Vector3(0f, -0.05f, 0f), new Vector3(s.x - 0.2f, 0.1f, s.y - 0.2f), new Color(0.72f, 0.6f, 0.45f), "Suelo_de_madera");
        float hx = s.x * 0.5f, hz = s.y * 0.5f;
        int groups = Mathf.Clamp(Mathf.CeilToInt(seats / 6f), 1, 4);
        // AULA DE CLASES (−X): la tarima del profesor con su mesa, la pizarra y la pantalla del proyector (van juntas) y las filas de
        // seis pupitres mirando hacia ella, con 2 m de paso entre filas.
        Transform aula = new GameObject("Aula_de_clases").transform; aula.SetParent(g, false);
        float ax = -hx * 0.47f;   // centro del aula (−8,5 m en un piso de 36 m)
        Transform front = KitchenProps.Root(aula, "Tarima del profesor (mesa, pizarra y pantalla)", new Vector3(ax, 0f, -hz + 2.3f));
        KitchenProps.C(front, new Vector3(0f, 0.1f, 0f), new Vector3(6f, 0.2f, 2.6f), new Color(0.55f, 0.42f, 0.3f), 0f, "Tarima");
        KitchenProps.C(front, new Vector3(0f, 0.95f, 0.4f), new Vector3(2f, 0.05f, 0.8f), new Color(0.5f, 0.38f, 0.26f), 0f, "Mesa_del_profesor");
        KitchenProps.C(front, new Vector3(0f, 0.55f, 0.4f), new Vector3(1.9f, 0.7f, 0.05f), new Color(0.45f, 0.34f, 0.24f), 0f, "Faldon_mesa");
        KitchenProps.C(front, new Vector3(-1.6f, 1.6f, -1.15f), new Vector3(2.8f, 1.2f, 0.06f), new Color(0.15f, 0.3f, 0.22f), 0f, "Pizarra");
        KitchenProps.C(front, new Vector3(1.6f, 1.8f, -1.15f), new Vector3(2.6f, 1.5f, 0.04f), new Color(0.95f, 0.95f, 0.95f), 0f, "Pantalla_proyector");
        // (2026-11-05, regla S14) Herramientas de saber del mundo sobre la mesa: ábaco chino, astrolabio, quipu y esfera griega.
        KitchenProps.C(front, new Vector3(-0.7f, 1.02f, 0.4f), new Vector3(0.3f, 0.06f, 0.18f), new Color(0.45f, 0.25f, 0.15f), 0f, "Abaco_China");
        KitchenProps.C(front, new Vector3(-0.25f, 1.03f, 0.4f), new Vector3(0.18f, 0.03f, 0.18f), new Color(0.75f, 0.6f, 0.3f), 0f, "Astrolabio_Al_Andalus");
        KitchenProps.C(front, new Vector3(0.2f, 1.02f, 0.4f), new Vector3(0.3f, 0.02f, 0.14f), new Color(0.86f, 0.8f, 0.65f), 0f, "Quipu_Andes");
        KitchenProps.C(front, new Vector3(0.7f, 1.09f, 0.4f), new Vector3(0.18f, 0.18f, 0.18f), new Color(0.6f, 0.55f, 0.4f), 0f, "Esfera_Grecia");
        KitchenProps.Fit(front);
        AreaSeat.Add(front.gameObject, "dar clase", AreaSeat.Category.Trabajo, 2);   // profesor y ayudante
        WorldLabel fl = front.gameObject.AddComponent<WorldLabel>(); fl.text = "Tarima del profesor"; fl.height = 2.8f;
        float z0 = -hz + 6.2f;   // 2 m libres tras la tarima (sus sillas sobresalen 0,54 m)
        for (int r = 0; r < groups; r++) DeskRow(aula, $"Fila de pupitres {r + 1} (6 plazas)", new Vector3(ax, 0f, z0 + r * 3.4f), 6, AreaSeat.Category.Estudio, "clase");
        // SALA DE ESTUDIO Y BIBLIOTECA (+X): mesas de seis, las estanterías contra la pared (un solo mueble) y dos puestos del catálogo
        // y de ordenador (con el clúster de IA del santuario).
        Transform lib = new GameObject("Sala_de_estudio").transform; lib.SetParent(g, false);
        float lx = hx * 0.25f, lx2 = hx * 0.58f;   // 4,5 y 10,5 m
        Vector3[] tables = { new Vector3(lx, 0f, -hz + 5f), new Vector3(lx2, 0f, -hz + 5f), new Vector3(lx, 0f, -hz + 11f), new Vector3(lx2, 0f, -hz + 11f) };
        for (int t = 0; t < groups; t++) StudyTable(lib, $"Mesa de estudio {t + 1} (6 plazas)", tables[t], 3);
        Transform shelves = KitchenProps.Root(lib, "Estanterías de la biblioteca", new Vector3(hx - 2.2f, 0f, -hz + 8.5f));
        Color[] spines = { new Color(0.55f, 0.2f, 0.2f), new Color(0.2f, 0.35f, 0.55f), new Color(0.3f, 0.45f, 0.25f), new Color(0.6f, 0.5f, 0.25f) };
        for (int k = 0; k < 6; k++)
        {
            float z = -6f + k * 2.4f;
            KitchenProps.C(shelves, new Vector3(0f, 1.1f, z), new Vector3(0.45f, 2.2f, 2.3f), new Color(0.5f, 0.38f, 0.26f), 0f, "Estanteria");
            for (int b = 0; b < 4; b++) KitchenProps.C(shelves, new Vector3(-0.2f, 0.35f + b * 0.5f, z), new Vector3(0.06f, 0.3f, 2.1f), spines[(k + b) % 4], 0f, "Lomos_de_libros");
        }
        KitchenProps.Fit(shelves);
        WorldLabel sl = shelves.gameObject.AddComponent<WorldLabel>(); sl.text = number > 1 ? $"Biblioteca (piso {number})" : "Biblioteca"; sl.height = 2.8f;
        for (int k = 0; k < 2; k++)
        {
            Transform pc = KitchenProps.Root(lib, $"Puesto del catálogo y de ordenador {k + 1}", new Vector3(k == 0 ? lx : lx2, 0f, -hz + 16.5f));
            MonitorDesk(pc, 1);
            KitchenProps.Fit(pc);
            AreaSeat.Add(pc.gameObject, "catálogo", AreaSeat.Category.Trabajo);
            WorldLabel pl = pc.gameObject.AddComponent<WorldLabel>(); pl.text = pc.name; pl.height = 1.8f;
        }
        Aseos(floor, new Vector3(-hx + 2.1f, 0.1f, hz - 0.9f), 180f, 1);
    }
}
