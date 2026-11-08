using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA PLANTA NUCLEAR BAJO EL MAR (Berón 2026-11-08; docs/sitio-nuclear.md §25): la Central Nuclear y la Planta Atómica, con todos sus pisos y su maquinaria, en el
/// SANTUARIO MARINO, junto a las cavernas del microrreactor y de los residuos, al nivel de su galería (se va andando desde el andén de abajo, sin escaleras).
/// Los mismos constructores de siempre (<see cref="BuildNuclearPlant"/>, <see cref="BuildAtomicPlant"/>, <see cref="BuildPhysicsHall"/>): con
/// <see cref="_marineFloors"/> encendido, cada piso que pedían a la torre (<see cref="TowerFloor"/>, <see cref="InnerFloor"/>) va a su sala, en dos filas de tres,
/// con <see cref="MarineHallGap"/> m de paso entre salas. Cada sala es una caverna con su revestimiento, a la cota de la galería.
/// </summary>
public static partial class SampleSceneBuilder
{
    /// <summary>¿Los pisos van a las salas de abajo en vez de a una torre? (solo mientras se construye la planta del Santuario Marino)</summary>
    static bool _marineFloors;
    static float _marineFloorY, _marineStartX, _marineCursorX, _marineRowZ;
    static int _marineInRow;
    static readonly List<(Transform floor, Rect plan)> _marineHalls = new List<(Transform, Rect)>();
    /// <summary>Paso entre una sala y la siguiente (m): las puertas de las áreas (abiertas a +Z) dan a él. Más de 2 personas y un carro.</summary>
    const float MarineHallGap = 8f;
    const int MarineHallsPerRow = 3;

    /// <summary>El siguiente piso de la planta, en su sala: de oeste a este en la fila, y la fila siguiente al norte.</summary>
    static Vector2 MarineFloor(Transform floor)
    {
        Vector2 s = NuclearSize;
        float x = _marineCursorX + s.x * 0.5f;
        floor.SetPositionAndRotation(new Vector3(x, _marineFloorY, _marineRowZ), Quaternion.identity);
        _platforms[floor] = s;
        _marineHalls.Add((floor, new Rect(x - s.x * 0.5f, _marineRowZ - s.y * 0.5f, s.x, s.y)));
        _marineCursorX += s.x + MarineHallGap;
        if (++_marineInRow == MarineHallsPerRow) { _marineInRow = 0; _marineCursorX = _marineStartX; _marineRowZ += s.y + MarineHallGap; }
        _towerTop = floor; _towerSize = s;
        return s;
    }

    /// <summary>Monta la planta junto a las cavernas, desde <paramref name="xStart"/> hacia el este, a la cota <paramref name="floorY"/> (la del suelo de la galería).
    /// Devuelve su planta (para el suelo y los límites de la escena).</summary>
    static Rect BuildMarinePlant(Transform root, Vector3 at, float floorY, float xStart)
    {
        _marineFloors = true; _marineHalls.Clear();
        _marineFloorY = floorY; _marineStartX = _marineCursorX = xStart; _marineRowZ = at.z; _marineInRow = 0;
        _towerHelix = null; _towerTop = null; _towerSize = NuclearSize; _towerCenter = new Vector3(xStart, floorY, at.z);
        Rect zone = new Rect(xStart, at.z, 0f, 0f);
        SanctuaryArea nuc = null, plant = null;
        try
        {
            // Fila 1: la Central (control, conversión, calefacción, subestación…), su Centro de emergencias y la Planta Atómica (el TRIGA en su piscina).
            nuc = BuildNuclearPlant(root);
            Transform em = InnerFloor(nuc.transform, NuclearSiteRule.EmergencyName, nuc.transform);
            FurnishEmergencyCenter(em, NuclearSize);
            plant = BuildAtomicPlant(root);
            // Fila 2: el ciclotrón (lo pone BuildAtomicPlant) y la sala de experimentos de física.
            BuildPhysicsHall(plant.transform);
        }
        finally { _marineFloors = false; _towerTop = null; }

        // Cada sala, una caverna (como las del microrreactor y los residuos: 10 m de alto, el techo a la misma cota, 60 m de roca encima).
        float cy = floorY - 0.6f + NuclearSiteRule.CavernHeight * 0.5f;
        bool first = true;
        foreach (var (floor, plan) in _marineHalls)
        {
            if (floor == null) continue;
            BuildCavern(root, $"Caverna_{floor.name}", new Vector3(plan.center.x, cy, plan.center.y), new Vector3(plan.width + 2f, NuclearSiteRule.CavernHeight, plan.height + 2f),
                $"Sala bajo el mar: {(floor.GetComponent<SanctuaryArea>() is SanctuaryArea a ? a.displayName : floor.name.Replace('_', ' '))}");
            zone = first ? plan : Rect.MinMaxRect(Mathf.Min(zone.xMin, plan.xMin), Mathf.Min(zone.yMin, plan.yMin), Mathf.Max(zone.xMax, plan.xMax), Mathf.Max(zone.yMax, plan.yMax));
            first = false;
        }
        // El péndulo de Foucault (17 m) no cabe en 10 m: sube por su pozo, perforado en la roca encima de la sala de física.
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == "Bloque_Péndulo de Foucault")
            {
                KitchenProps.C(root, new Vector3(t.position.x, floorY + 13f, t.position.z), new Vector3(4.4f, 8f, 4.4f), new Color(0.55f, 0.55f, 0.53f), 0f, "Pozo_pendulo");
                Label(t, "Péndulo de Foucault: su torre sube por un pozo perforado en la roca", 4f);
            }
        // T16: LAS COLUMNAS DE ROCA de cada sala (luz libre ≤ 14 m), a ≥ 2 m de todo mueble; donde un equipo fijo no deja sitio, una viga de transferencia.
        int cols = 0, beams = 0;
        foreach (var (floor, plan) in _marineHalls) if (floor != null) HallSupports(root, floor, plan, floorY, ref cols, ref beams);
        Debug.Log($"[Marino] T16 en las salas de la planta: {cols} columnas de roca y {beams} vigas de transferencia (donde el equipo fijo no deja 2 m alrededor de la columna).");
        // Las máquinas de virtualización de sus áreas (las de siempre: Newton, Curie y Einstein; Chernóbil), ya con las columnas puestas.
        WireMeditationArea(root, "AtomicPlant_Area", NuclearSite.PlantArea, new Color(0.8f, 0.82f, 0.78f), MissionKind.Channel, PhysicsSceneBuilder.SceneName);
        WireMeditationArea(root, "NuclearPower_Area", NuclearSite.PowerArea, new Color(0.75f, 0.78f, 0.8f), MissionKind.Channel, null, virtualOnly: true);
        // Las POSICIONES RESERVADAS de sus microcosmos (regla 8; antes las ponía el Blockout del santuario 1).
        PlaceReservedSlotsOn(new[] { nuc != null ? nuc.transform : null, plant != null ? plant.transform : null }, root);
        // Luz de las salas de la planta.
        Transform lamp = new GameObject("Luz_planta").transform; lamp.SetParent(root, false); lamp.position = new Vector3(zone.center.x, floorY + 8f, zone.center.y);
        Light l = lamp.gameObject.AddComponent<Light>(); l.type = LightType.Point; l.range = Mathf.Max(zone.width, zone.height); l.intensity = 2f;
        Debug.Log($"[Marino] Planta nuclear bajo el mar: {_marineHalls.Count} salas de {NuclearSize.x:0} × {NuclearSize.y:0} m, de x {zone.xMin:0} a {zone.xMax:0} y de z {zone.yMin:0} a {zone.yMax:0}, a {floorY:0} m (la galería).");
        return zone;
    }

    // ── T16 EN LAS SALAS DE LA PLANTA: columnas de roca con la luz libre ≤ 14 m ──
    /// <summary>Cuánto se corre la Planta Atómica (el TRIGA y su equipo, una distribución compacta de 22 × 16 m) al este de su sala: así la columna oeste
    /// queda a ≥ 2 m de todo. La este cae en medio del equipo: va su viga de transferencia.</summary>
    const float MarineAtomicShiftX = 9f;
    /// <summary>Cuánto se corre el ciclotrón al sur de su sala: fuera de la banda de columnas (z = 0).</summary>
    const float MarineCyclotronShiftZ = -6f;

    /// <summary>Las columnas de una sala de <paramref name="size"/> (marco de la sala): las menos que dejan cada luz libre (de columna a columna o a la pared)
    /// en ≤ <see cref="CavernRule.MaxFreeSpan"/>, repartidas por igual. 46 × 30 m: dos, en (±8,3; 0).</summary>
    static List<Vector2> MarineColumns(Vector2 size)
    {
        float cs = CavernRule.ColumnSize, max = CavernRule.MaxFreeSpan;
        int nx = Mathf.Max(0, Mathf.CeilToInt((size.x - max) / (max + cs) - 1e-4f)), nz = Mathf.Max(0, Mathf.CeilToInt((size.y - max) / (max + cs) - 1e-4f));
        float sx = (size.x - nx * cs) / (nx + 1), sz = (size.y - nz * cs) / (nz + 1);
        var l = new List<Vector2>();
        for (int i = 0; i < nx; i++)
            for (int j = 0; j < Mathf.Max(1, nz); j++)
                l.Add(new Vector2(-size.x * 0.5f + sx * (i + 1) + cs * (i + 0.5f), nz == 0 ? 0f : -size.y * 0.5f + sz * (j + 1) + cs * (j + 0.5f)));
        return l;
    }

    /// <summary>Lo que no se amuebla en una sala de abajo (lo usa <see cref="CarpetKeepOut"/>): cada columna con 1 m alrededor (PackFloor deja otro metro: 2 m) y
    /// la esquina del tanque de virtualización. Sin alfombras: abajo no hay.</summary>
    static List<Rect> MarineKeepOut(Vector2 size)
    {
        float h = CavernRule.ColumnSize * 0.5f + 1f;
        var l = new List<Rect>();
        foreach (Vector2 c in MarineColumns(size)) l.Add(new Rect(c.x - h, c.y - h, 2f * h, 2f * h));
        l.Add(new Rect(size.x * 0.5f - 4f, size.y * 0.5f - 4f, 4f, 4f));
        return l;
    }

    /// <summary>Las columnas de una sala; la que no tenga 2 m libres alrededor, viga de transferencia en su línea (de pared a pared, bajo el techo).</summary>
    static void HallSupports(Transform root, Transform floor, Rect plan, float floorY, ref int cols, ref int beams)
    {
        float cs = CavernRule.ColumnSize, top = floorY - 0.6f + NuclearSiteRule.CavernHeight - 0.6f;
        Color stone = new Color(0.45f, 0.44f, 0.42f), steel = TreeSteel, dark = new Color(0.2f, 0.22f, 0.25f);
        Transform cav = root.Find($"Caverna_{floor.name}");
        Transform parent = cav != null ? cav : root;
        foreach (Vector2 c in MarineColumns(plan.size))
        {
            Vector3 w = new Vector3(plan.center.x + c.x, floorY, plan.center.y + c.y);
            string blocker = ColumnBlocker(root, w, cs * 0.5f + 2f, floorY);
            if (blocker == null)
            {
                Transform col = Solid(parent, parent.InverseTransformPoint(w + Vector3.up * (top - floorY) * 0.5f), new Vector3(cs, top - floorY, cs), stone, "Columna_roca");
                KitchenProps.C(parent, parent.InverseTransformPoint(w + new Vector3(0f, 1f, -cs * 0.5f - 0.1f)), new Vector3(0.5f, 0.5f, 0.2f), dark, 0f, "Celula_columna");
                cols++;
            }
            else
            {
                Transform beam = KitchenProps.C(parent, parent.InverseTransformPoint(new Vector3(w.x, top - 0.6f, plan.center.y)), new Vector3(2.4f, 1.2f, plan.height + 2f), steel, 0f, "Viga_transferencia");   // (2,4 m de ala: la luz a cada lado queda en ≤ 14 m)
                Label(beam, $"Viga de transferencia: aquí iría una columna de roca, pero «{blocker}» no deja 2 m alrededor (T16)", -1.5f);
                Debug.Log($"[Marino] T16: en «{floor.name}» la columna de ({c.x:0.0}, {c.y:0.0}) chocaría con «{blocker}»: viga de transferencia de {plan.height + 2f:0} m en su lugar.");
                beams++;
            }
        }
    }

    // Lo primero que se alza a menos de <paramref name="half"/> (en planta) del centro de una columna, entre el suelo y 3 m; null si nada.
    static string ColumnBlocker(Transform root, Vector3 c, float half, float floorY)
    {
        foreach (Renderer r in root.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled || r.GetComponent<TMPro.TMP_Text>() != null) continue;
            string n = r.name;
            if (n.StartsWith("Revestimiento_") || n.StartsWith("Forro_") || n == "Roca" || n.StartsWith("Lecho") || n.StartsWith("Suelo") || n == "Mat" || n.StartsWith("Linea_")
                || n == "Columna_roca" || n == "Celula_columna" || n == "Viga_transferencia" || n == "Agua" || n.StartsWith("Fondo")) continue;
            Bounds b = r.bounds;
            if (b.max.y < floorY + 0.15f || b.min.y > floorY + 3f) continue;
            if (b.size.y < 0.3f && Mathf.Max(b.size.x, b.size.z) > 3f) continue;   // (suelos y losas)
            if (b.max.x > c.x - half && b.min.x < c.x + half && b.max.z > c.z - half && b.min.z < c.z + half) return r.transform.parent != null ? $"{r.transform.parent.name}/{n}" : n;
        }
        return null;
    }

    /// <summary>EL CENTRO DE EMERGENCIAS de la planta: vigilancia radiológica, viento, plan de evacuación y el panel de parada remota.</summary>
    static void FurnishEmergencyCenter(Transform em, Vector2 size)
    {
        float S = size.x;
        Color desk = new Color(0.32f, 0.34f, 0.38f), screen = new Color(0.12f, 0.35f, 0.45f), alarm = new Color(0.8f, 0.2f, 0.15f);
        Solid(em, new Vector3(0f, 0.05f, 0f), new Vector3(size.x, 0.1f, size.y), new Color(0.3f, 0.32f, 0.35f), "Suelo_emergencias");
        for (int i = 0; i < 4; i++)
        {
            Transform p = new GameObject("Puesto_emergencias").transform; p.SetParent(em, false); p.localPosition = new Vector3(-9f + i * 6f, 0.1f, 6f);   // (a 3,5 m de la banda de columnas, T16)
            KitchenProps.C(p, new Vector3(0f, 0.38f, 0f), new Vector3(2.2f, 0.76f, 0.9f), desk, 0f, "Mesa_control");
            KitchenProps.C(p, new Vector3(0f, 1.05f, 0.25f), new Vector3(1.8f, 0.6f, 0.06f), screen, 0f, "Pantallas");
            AreaSeat.Add(p.gameObject, "emergencias", AreaSeat.Category.Trabajo);
        }
        Transform panel = KitchenProps.Root(em, "Panel_parada_remota", new Vector3(0f, 0.1f, 11f));
        KitchenProps.C(panel, new Vector3(0f, 1.2f, 0f), new Vector3(6f, 2.4f, 0.4f), desk, 0f, "Panel");
        KitchenProps.C(panel, new Vector3(0f, 1.6f, -0.25f), new Vector3(0.5f, 0.5f, 0.1f), alarm, 0f, "Pulsador_parada");
        Label(panel, "Panel de parada remota: para el reactor aunque no se pueda entrar en la sala de control", 3f);
        KitchenProps.C(em, new Vector3(-S * 0.5f + 6f, 2.5f, 12f), new Vector3(8f, 4f, 0.3f), screen, 0f, "Plano_emergencia");
        Label(KitchenProps.C(em, new Vector3(0f, 4.5f, 6f), Vector3.one * 0.1f, screen, 0f, "Cartel_emergencias"), "Centro de emergencias: vigilancia radiológica, viento y plan de evacuación", 1f);
    }
}
