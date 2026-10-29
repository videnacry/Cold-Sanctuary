using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EDIFICIOS DE INVESTIGACIÓN Y DE PRODUCCIÓN (Berón 2026-10-23/24; docs/laboratorios-investigacion.md y
/// docs/edificios-investigacion.md): un edificio por materia y la central de producción, en el casquete Sur, junto a la
/// planta atómica (que es el edificio de FÍSICA: reactor, ciclotrón y, arriba, la sala de experimentos con su anillo).
///  · QUÍMICA: abajo, la síntesis (4 campanas con su línea Schlenk, 4 rotavapores, caja de guantes, armarios separados por
///    riesgo, duchas); arriba, el análisis (RMN de 400 MHz en su sala, 4 HPLC, GC-MS, IR/UV-Vis, DRX, hornos, balanzas).
///  · BIOLOGÍA (BSL-2): abajo, el cultivo (4 cabinas de seguridad, incubadores, microscopios, biorreactores, −80 °C,
///    cámara fría, autoclave a la salida); arriba, la molecular en sentido único (pre-PCR → extracción → post-PCR).
///  · CENTRAL NUCLEAR de producción: un microrreactor tipo eVinci (5 MWe y calor para la calefacción de distrito), con su
///    límite del sitio pintado (sin valla: no hay barandillas), sala de control para 4 operadores, conversión de potencia, subestación, diésel de respaldo y losa de
///    contenedores (el núcleo gastado vuelve a fábrica).
/// Cada equipo es un BLOQUE («Bloque_…») con 2 m libres a su alrededor (FurnitureSpacing lo comprueba); lo que va junto
/// en la realidad (la campana con su mesa, las cámaras frías) es un solo bloque. Lo que hace una persona está ×4, para
/// que cuatro puedan hacer lo mismo a la vez; los equipos grandes y caros, una vez.
/// </summary>
public static partial class SampleSceneBuilder
{
    // Química y Biología son pisos de la torre de CIENCIAS (CienciasSize, docs/plano-edificios.md); la central, aparte.
    public static readonly Vector2 NuclearSize = new Vector2(46f, 30f);
    static readonly Vector3 NuclearCenter = new Vector3(60f, 0f, -162f);

    static readonly Color LabWhite = new Color(0.9f, 0.92f, 0.93f), LabSteel = new Color(0.68f, 0.71f, 0.74f), LabDark = new Color(0.22f, 0.24f, 0.27f),
        LabGlass = new Color(0.65f, 0.82f, 0.9f, 0.4f), LabBench = new Color(0.25f, 0.28f, 0.3f), LabYellow = new Color(0.95f, 0.8f, 0.15f),
        LabGreen = new Color(0.2f, 0.65f, 0.35f), LabBlue = new Color(0.25f, 0.45f, 0.8f), LabRed = new Color(0.8f, 0.25f, 0.2f);

    /// <summary>Un bloque de la distribución: nombre, huella (a lo largo × fondo) y cómo se construye en su raíz.</summary>
    struct LabBlock
    {
        public string name; public float w, d; public System.Action<Transform> build;
        public LabBlock(string n, float w, float d, System.Action<Transform> b) { name = n; this.w = w; this.d = d; build = b; }
    }

    static List<LabBlock> Times(int n, string name, float w, float d, System.Action<Transform> b)
    {
        var l = new List<LabBlock>();
        for (int i = 1; i <= n; i++) l.Add(new LabBlock(n > 1 ? $"{name} {i}" : name, w, d, b));
        return l;
    }

    /// <summary>Coloca los bloques en filas sobre una planta de <paramref name="size"/>: 2 m entre bloques y 2 m al borde,
    /// fuera de las <paramref name="keepOut"/> (el área de alfombras, la máquina de virtualización). Devuelve los que no cupieron.</summary>
    static List<LabBlock> PackFloor(Transform floor, Vector2 size, List<LabBlock> blocks, List<Rect> keepOut)
    {
        const float gap = 2.1f, margin = 2f;   // 2 m libres y un poco más: la caja real de cada bloque sobresale unos cm de su huella
        var left = new List<LabBlock>();
        float x0 = -size.x * 0.5f + margin, x1 = size.x * 0.5f - margin, z = -size.y * 0.5f + margin, zMax = size.y * 0.5f - margin;
        float x = x0, rowD = 0f;
        foreach (LabBlock b in blocks)
        {
            bool placed = false;
            for (int guard = 0; guard < 60 && !placed; guard++)
            {
                if (x + b.w > x1) { x = x0; z += rowD + gap; rowD = 0f; }
                if (z + b.d > zMax) break;
                var r = new Rect(x, z, b.w, b.d);
                Rect hit = default; bool blocked = false;
                foreach (Rect k in keepOut) if (k.Overlaps(new Rect(r.x - gap * 0.5f, r.y - gap * 0.5f, r.width + gap, r.height + gap))) { hit = k; blocked = true; break; }
                if (blocked) { x = Mathf.Max(x + 0.5f, hit.xMax + gap * 0.5f); continue; }
                Transform t = KitchenProps.Root(floor, "Bloque_" + b.name, new Vector3(x + b.w * 0.5f, 0f, z + b.d * 0.5f), 0f);
                b.build(t);
                KitchenProps.Fit(t, t.Find("Pared_sala") == null);   // en las salas se entra: su caja es solo de alcance
                WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = b.name; wl.height = 2.4f;
                x += b.w + gap; rowD = Mathf.Max(rowD, b.d); placed = true;
            }
            if (!placed) left.Add(b);
        }
        return left;
    }

    /// <summary>Bloques a lo largo de la línea central de un anillo, con 2 m entre ellos (fuera de las esquinas y del hueco
    /// de su área de alfombras, en el centro de +Z, y su simétrico en −Z). Devuelve los que no cupieron.</summary>
    static List<LabBlock> PackRing(Transform ring, Vector2 floor, List<LabBlock> blocks)
    {
        var left = new List<LabBlock>();
        float len = RingLength(floor), s = 2f;
        foreach (LabBlock b in blocks)
        {
            bool placed = false;
            while (s < len && !placed)
            {
                var (p0, yaw, corner) = RingPoint(floor, s + b.w * 0.5f);
                var (pa, _, ca) = RingPoint(floor, s); var (pb, _, cb) = RingPoint(floor, s + b.w);
                bool stair = Mathf.Abs(p0.x) < b.w * 0.5f + 3f && Mathf.Abs(p0.z) > floor.y * 0.5f - 3f;   // el hueco del área de alfombras del anillo (RingCarpets, centro de +Z; y −Z)
                if (corner || ca || cb || stair || Vector3.Distance(pa, pb) < b.w * 0.9f) { s += 1f; continue; }
                Transform t = KitchenProps.Root(ring, "Bloque_" + b.name, p0, yaw);
                b.build(t);
                KitchenProps.Fit(t, t.Find("Pared_sala") == null);   // en las salas se entra: su caja es solo de alcance
                WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = b.name; wl.height = 2.4f;
                s += b.w + 2.1f; placed = true;
            }
            if (!placed) left.Add(b);
        }
        return left;
    }

    // Zonas que no se amueblan en una planta: el hueco de su ÁREA DE ALFOMBRAS (+Z, en <paramref name="x"/>; PlaceCarpetAreas
    // la pone donde no hay muebles) y la máquina de virtualización (esquina +X +Z). Ya no hay escaleras (docs/alfombras-plan.md).
    static List<Rect> CarpetKeepOut(Vector2 size, float x = 0f) =>
        new List<Rect> { new Rect(x - 4.2f, size.y * 0.5f - 5f, 8.4f, 5f), new Rect(size.x * 0.5f - 4f, size.y * 0.5f - 4f, 4f, 4f) };

    static void LogLeft(string where, List<LabBlock> left)
    {
        if (left.Count == 0) return;
        Debug.LogWarning($"[Investigación] {where}: no caben {left.Count} bloques ({string.Join(", ", left.ConvertAll(b => b.name))}).");
    }

    // ── Piezas de laboratorio (todas de cubos, con lo que las hace reconocibles) ─────────────────────────────────────

    static void Bench(Transform t, float w, float d, Color top)
    {
        KitchenProps.C(t, new Vector3(0f, 0.45f, 0f), new Vector3(w, 0.9f, d), LabWhite, 0f, "Mueble");
        KitchenProps.C(t, new Vector3(0f, 0.92f, 0f), new Vector3(w + 0.02f, 0.04f, d + 0.02f), top, 0f, "Encimera");
    }

    static void FumeHood(Transform t)   // campana de gases sobre su mueble, con guillotina, conducto y línea Schlenk (y su mesa auxiliar): centrada en su huella de 3 m (antes asomaba 0,6 m por +X)
    {
        Bench(KitchenProps.Root(t, "Mueble", new Vector3(-0.6f, 0f, 0f)), 1.8f, 0.9f, LabBench);
        KitchenProps.C(t, new Vector3(-0.6f, 1.6f, 0.1f), new Vector3(1.8f, 1.3f, 0.7f), LabWhite, 0f, "Campana");
        KitchenProps.C(t, new Vector3(-0.6f, 1.4f, -0.26f), new Vector3(1.6f, 0.75f, 0.02f), LabGlass, 0f, "Guillotina");
        KitchenProps.C(t, new Vector3(-0.6f, 2.6f, 0.2f), new Vector3(0.35f, 0.7f, 0.35f), LabSteel, 0f, "Conducto");
        KitchenProps.C(t, new Vector3(-0.6f, 1.95f, -0.1f), new Vector3(1.5f, 0.06f, 0.06f), LabGlass, 0f, "Linea_Schlenk");
        for (int i = 0; i < 4; i++) KitchenProps.C(t, new Vector3(-1.2f + i * 0.4f, 1.85f, -0.1f), new Vector3(0.04f, 0.14f, 0.04f), LabSteel, 0f, "Llave");
        Bench(KitchenProps.Root(t, "Mesa", new Vector3(0.95f, 0f, 0f)), 1.2f, 0.9f, LabBench);
    }

    static void Instrument(Transform t, float w, float d, float h, Color body, string label)   // instrumento sobre su mesa, con pantalla
    {
        Bench(t, w, d, LabBench);
        KitchenProps.C(t, new Vector3(-w * 0.15f, 0.94f + h * 0.5f, 0f), new Vector3(w * 0.55f, h, d * 0.8f), body, 0f, label);
        KitchenProps.C(t, new Vector3(w * 0.33f, 1.2f, -d * 0.1f), new Vector3(0.45f, 0.3f, 0.04f), LabDark, 0f, "Pantalla");
    }

    static void Cabinet(Transform t, float w, Color c, string what)   // armario de seguridad (inflamables, ácidos, bases)
    {
        KitchenProps.C(t, new Vector3(0f, 0.9f, 0f), new Vector3(w, 1.8f, 0.5f), c, 0f, what);
        KitchenProps.C(t, new Vector3(0f, 1.4f, -0.26f), new Vector3(w * 0.5f, 0.2f, 0.01f), Color.white, 0f, "Pictograma");
    }

    static void SafetyShower(Transform t)
    {
        KitchenProps.C(t, new Vector3(0f, 1.15f, 0f), new Vector3(0.08f, 2.3f, 0.08f), LabGreen, 0f, "Columna");
        KitchenProps.C(t, new Vector3(0f, 2.3f, -0.3f), new Vector3(0.35f, 0.08f, 0.35f), LabGreen, 0f, "Ducha");
        KitchenProps.C(t, new Vector3(0f, 1.0f, -0.25f), new Vector3(0.3f, 0.12f, 0.2f), LabGreen, 0f, "Lavaojos");
        KitchenProps.C(t, new Vector3(0f, 0.01f, -0.3f), new Vector3(0.9f, 0.02f, 0.9f), LabYellow, 0f, "Zona_marcada");
    }

    static void Room(Transform t, float w, float d, Color wall, string label)   // sala o cámara: paredes con su puerta abierta (se entra)
    {
        const float h = 2.4f, th = 0.15f, door = 1.2f;
        Solid(t, new Vector3(0f, h * 0.5f, d * 0.5f), new Vector3(w, h, th), wall, "Pared_sala");
        Solid(t, new Vector3(-w * 0.5f, h * 0.5f, 0f), new Vector3(th, h, d), wall, "Pared_sala");
        Solid(t, new Vector3(w * 0.5f, h * 0.5f, 0f), new Vector3(th, h, d), wall, "Pared_sala");
        float side = (w - door) * 0.5f;
        Solid(t, new Vector3(-(door + side) * 0.5f, h * 0.5f, -d * 0.5f), new Vector3(side, h, th), wall, "Pared_sala");
        Solid(t, new Vector3((door + side) * 0.5f, h * 0.5f, -d * 0.5f), new Vector3(side, h, th), wall, "Pared_sala");
    }

    static void Tall(Transform t, float w, float d, float h, Color c, string label, Color display)   // armario alto con pantalla
    {
        KitchenProps.C(t, new Vector3(0f, h * 0.5f, 0f), new Vector3(w, h, d), c, 0f, label);
        KitchenProps.C(t, new Vector3(w * 0.25f, h - 0.2f, -d * 0.5f - 0.01f), new Vector3(0.18f, 0.08f, 0.01f), display, 0f, "Display");
    }

    static void BioSafetyCabinet(Transform t)   // cabina de seguridad biológica clase II sobre su soporte
    {
        KitchenProps.C(t, new Vector3(0f, 0.35f, 0f), new Vector3(1.9f, 0.7f, 0.8f), LabSteel, 0f, "Soporte");
        KitchenProps.C(t, new Vector3(0f, 1.45f, 0f), new Vector3(1.9f, 1.5f, 0.8f), LabWhite, 0f, "Cabina");
        KitchenProps.C(t, new Vector3(0f, 1.35f, -0.41f), new Vector3(1.6f, 0.7f, 0.02f), LabGlass, 0f, "Frontal");
        KitchenProps.C(t, new Vector3(0f, 2.1f, -0.41f), new Vector3(1.6f, 0.2f, 0.02f), LabDark, 0f, "Panel_flujo");
    }

    static void Microscope(Transform t, bool fluo)
    {
        Bench(t, 1.5f, 0.8f, LabBench);
        KitchenProps.C(t, new Vector3(0f, 1.05f, 0.1f), new Vector3(0.35f, 0.2f, 0.4f), fluo ? LabDark : LabWhite, 0f, "Base_microscopio");
        KitchenProps.C(t, new Vector3(0f, 1.35f, 0.18f), new Vector3(0.1f, 0.45f, 0.1f), LabWhite, 0f, "Brazo");
        KitchenProps.C(t, new Vector3(0f, 1.5f, -0.02f), new Vector3(0.08f, 0.08f, 0.3f), LabDark, 30f, "Oculares");
        if (fluo) KitchenProps.C(t, new Vector3(0.45f, 1.1f, 0.1f), new Vector3(0.3f, 0.3f, 0.3f), LabBlue, 0f, "Lampara_fluorescencia");
    }

    static void Vessel(Transform t, float r, float h, Color c, string label)   // depósito vertical (biorreactor, dewar, criostato)
    {
        for (int k = 0; k < 3; k++) KitchenProps.C(t, new Vector3(0f, h * 0.5f, 0f), new Vector3(r * 2f, h, r * 2f * 0.7f), c, k * 30f, label);
        KitchenProps.C(t, new Vector3(0f, h + 0.05f, 0f), new Vector3(r * 1.4f, 0.1f, r * 1.4f), LabSteel, 45f, "Tapa");
    }

    static void OpticalTable(Transform t, float w, float d)   // mesa óptica antivibración con su láser y espejos
    {
        for (int i = 0; i < 4; i++) KitchenProps.C(t, new Vector3((i % 2 - 0.5f) * (w - 0.3f), 0.4f, (i / 2 - 0.5f) * (d - 0.3f)), new Vector3(0.25f, 0.8f, 0.25f), LabDark, 0f, "Pata_neumatica");
        KitchenProps.C(t, new Vector3(0f, 0.85f, 0f), new Vector3(w, 0.1f, d), new Color(0.35f, 0.37f, 0.4f), 0f, "Tablero_roscado");
        KitchenProps.C(t, new Vector3(-w * 0.4f, 0.98f, 0f), new Vector3(0.35f, 0.15f, 0.12f), LabRed, 0f, "Laser");
        for (int i = 0; i < 3; i++) KitchenProps.C(t, new Vector3(-w * 0.1f + i * w * 0.18f, 1.0f, (i % 2 - 0.5f) * d * 0.4f), new Vector3(0.05f, 0.15f, 0.05f), LabSteel, 45f, "Espejo");
    }

    // ── QUÍMICA ──────────────────────────────────────────────────────────────────────────────────────────────────

    static SanctuaryArea BuildChemistry(Transform areasGroup)
    {
        SanctuaryArea a = BuildAreaBox(areasGroup, "Chemistry_Area", "Química", SanctuaryAreaType.Chemistry, 3, CienciasCenter, CienciasSize, new Color(0.78f, 0.85f, 0.9f));
        Vector2 cs = TowerFloor(a.transform);   // el siguiente piso de la torre de Ciencias (sobre la sala de experimentos)
        // Planta baja: SÍNTESIS (lo húmedo y peligroso, junto a las duchas y los armarios).
        var wet = new List<LabBlock>();
        wet.AddRange(Times(4, "Campana de gases", 3.0f, 0.9f, FumeHood));
        wet.AddRange(Times(4, "Rotavapor", 1.5f, 0.8f, t => { Bench(t, 1.5f, 0.8f, LabBench); KitchenProps.C(t, new Vector3(-0.2f, 1.15f, 0f), new Vector3(0.3f, 0.3f, 0.3f), LabGlass, 45f, "Matraz"); KitchenProps.C(t, new Vector3(0.25f, 1.4f, 0f), new Vector3(0.08f, 0.6f, 0.08f), LabGlass, -30f, "Refrigerante"); KitchenProps.C(t, new Vector3(0.55f, 1.05f, 0f), new Vector3(0.3f, 0.2f, 0.35f), LabDark, 0f, "Bomba_vacio"); }));
        wet.Add(new LabBlock("Caja de guantes", 2.5f, 1.15f, t => { KitchenProps.C(t, new Vector3(0f, 0.4f, 0f), new Vector3(2.5f, 0.8f, 1f), LabSteel, 0f, "Soporte"); KitchenProps.C(t, new Vector3(0f, 1.3f, 0f), new Vector3(2.5f, 1f, 1f), LabSteel, 0f, "Camara_inerte"); KitchenProps.C(t, new Vector3(0f, 1.3f, -0.51f), new Vector3(2f, 0.7f, 0.02f), LabGlass, 0f, "Ventana"); for (int i = 0; i < 2; i++) KitchenProps.C(t, new Vector3(-0.45f + i * 0.9f, 1.15f, -0.53f), new Vector3(0.22f, 0.22f, 0.04f), LabDark, 45f, "Guante"); }));
        wet.Add(new LabBlock("Armarios de inflamables", 2.2f, 0.5f, t => { Cabinet(KitchenProps.Root(t, "A", new Vector3(-0.55f, 0f, 0f)), 1.1f, LabYellow, "Inflamables"); Cabinet(KitchenProps.Root(t, "B", new Vector3(0.55f, 0f, 0f)), 1.1f, LabYellow, "Inflamables"); }));
        wet.Add(new LabBlock("Armario de ácidos", 1.1f, 0.5f, t => Cabinet(t, 1.1f, new Color(0.95f, 0.95f, 0.95f), "Acidos")));
        wet.Add(new LabBlock("Armario de bases", 1.1f, 0.5f, t => Cabinet(t, 1.1f, LabBlue, "Bases")));
        wet.AddRange(Times(2, "Ducha de seguridad", 1.0f, 1.3f, SafetyShower));
        wet.Add(new LabBlock("Residuos químicos", 1.2f, 0.6f, t => { for (int i = 0; i < 3; i++) KitchenProps.C(t, new Vector3(-0.4f + i * 0.4f, 0.35f, 0f), new Vector3(0.32f, 0.7f, 0.4f), i == 0 ? LabRed : i == 1 ? LabYellow : LabBlue, 0f, "Bidon"); }));
        wet.AddRange(Times(2, "Puesto de cuaderno", 1.6f, 1.0f, t => { Bench(t, 1.6f, 1f, new Color(0.55f, 0.45f, 0.35f)); KitchenProps.C(t, new Vector3(0f, 1.15f, 0.25f), new Vector3(0.6f, 0.4f, 0.04f), LabDark, 0f, "Monitor"); }));
        Transform g = new GameObject("Planta_sintesis").transform; g.SetParent(a.transform, false); g.localPosition = Vector3.up * 0.1f;
        LogLeft("Química (síntesis)", PackFloor(g, cs, wet, CarpetKeepOut(cs)));
        // Piso de arriba: ANÁLISIS (los instrumentos, lejos de los disolventes).
        Transform up = InnerFloor(a.transform, "Planta_analisis", a.transform);
        Vector2 us = FloorSize(up, cs);
        Solid(up, new Vector3(0f, 0.05f, 0f), new Vector3(us.x, 0.1f, us.y), new Color(0.8f, 0.84f, 0.86f), "Suelo_analisis");
        var dry = new List<LabBlock>
        {
            new LabBlock("Sala de RMN 400 MHz", 4.0f, 3.0f, t => { Room(t, 4f, 3f, new Color(0.85f, 0.88f, 0.92f), "Sala_RMN"); Vessel(KitchenProps.Root(t, "Iman", new Vector3(0f, 0f, 0.3f)), 0.45f, 1.6f, LabSteel, "Iman_superconductor"); KitchenProps.C(t, new Vector3(0f, 0.02f, -1.2f), new Vector3(3.6f, 0.02f, 0.08f), LabYellow, 0f, "Linea_5_gauss"); }),
            new LabBlock("RMN de sobremesa", 1.2f, 0.75f, t => Instrument(t, 1.2f, 0.75f, 0.4f, LabWhite, "RMN_sobremesa")),
            new LabBlock("GC-MS", 1.5f, 0.75f, t => Instrument(t, 1.5f, 0.75f, 0.55f, LabWhite, "Cromatografo_gases")),
        };
        dry.AddRange(Times(4, "HPLC", 1.2f, 0.75f, t => Instrument(t, 1.2f, 0.75f, 0.7f, new Color(0.85f, 0.85f, 0.82f), "Columna_HPLC")));
        dry.Add(new LabBlock("IR y UV-Vis", 2.0f, 0.75f, t => Instrument(t, 2.0f, 0.75f, 0.35f, LabWhite, "Espectrometros")));
        dry.Add(new LabBlock("Difractómetro de rayos X", 1.5f, 1.3f, t => Tall(t, 1.5f, 1.3f, 1.9f, new Color(0.88f, 0.88f, 0.86f), "DRX", LabYellow)));
        dry.Add(new LabBlock("Hornos y muflas", 2.0f, 0.75f, t => { Bench(t, 2f, 0.75f, LabBench); for (int i = 0; i < 2; i++) KitchenProps.C(t, new Vector3(-0.5f + i * 1f, 1.2f, 0f), new Vector3(0.6f, 0.5f, 0.55f), LabDark, 0f, "Mufla"); }));
        dry.Add(new LabBlock("Cuarto de balanzas", 2.5f, 2.0f, t => { Room(t, 2.5f, 2f, new Color(0.9f, 0.9f, 0.88f), "Cuarto_balanzas"); Bench(KitchenProps.Root(t, "Mesa_antivibracion", new Vector3(0f, 0f, 0.55f)), 2.2f, 0.6f, LabDark); for (int i = 0; i < 4; i++) KitchenProps.C(t, new Vector3(-0.8f + i * 0.53f, 1.05f, 0.55f), new Vector3(0.3f, 0.22f, 0.3f), LabGlass, 0f, "Balanza"); }));
        dry.AddRange(Times(2, "Puesto de análisis", 1.6f, 1.0f, t => { Bench(t, 1.6f, 1f, new Color(0.55f, 0.45f, 0.35f)); KitchenProps.C(t, new Vector3(0f, 1.15f, 0.25f), new Vector3(0.6f, 0.4f, 0.04f), LabDark, 0f, "Monitor"); }));
        LogLeft("Química (análisis)", PackFloor(up, us, dry, CarpetKeepOut(us, -us.x * 0.3f)));
        return a;
    }

    // ── BIOLOGÍA ─────────────────────────────────────────────────────────────────────────────────────────────────

    static SanctuaryArea BuildBiology(Transform areasGroup)
    {
        SanctuaryArea a = BuildAreaBox(areasGroup, "Biology_Area", "Biología", SanctuaryAreaType.Biology, 3, CienciasCenter, CienciasSize, new Color(0.78f, 0.9f, 0.78f));
        Vector2 bs = TowerFloor(a.transform);   // el siguiente piso de la torre de Ciencias (sobre el análisis de Química)
        // Planta baja: CULTIVO. El biorreactor piloto es I+D (líneas celulares y medios sin suero) para el laboratorio de
        // PRODUCCIÓN de carne cultivada (CulturedMeatLab), que es quien da de comer a los carnívoros; aquí no se produce.
        // De lo limpio a lo sucio: cabinas e incubadores al fondo; autoclave y lavado junto a la salida).
        var cult = new List<LabBlock>();
        cult.AddRange(Times(4, "Cabina de seguridad biológica", 2.0f, 0.85f, BioSafetyCabinet));
        cult.AddRange(Times(4, "Incubador de CO2", 0.8f, 0.85f, t => { Tall(KitchenProps.Root(t, "Abajo", Vector3.zero), 0.8f, 0.85f, 0.9f, LabWhite, "Incubador", LabGreen); Tall(KitchenProps.Root(t, "Arriba", new Vector3(0f, 0.92f, 0f)), 0.8f, 0.85f, 0.9f, LabWhite, "Incubador", LabGreen); }));
        cult.AddRange(Times(2, "Microscopio invertido", 1.5f, 0.8f, t => Microscope(t, false)));
        cult.AddRange(Times(2, "Microscopio de fluorescencia", 1.5f, 0.9f, t => Microscope(t, true)));
        cult.AddRange(Times(4, "Centrífuga", 0.9f, 0.8f, t => { KitchenProps.C(t, new Vector3(0f, 0.45f, 0f), new Vector3(0.9f, 0.9f, 0.8f), LabWhite, 0f, "Centrifuga"); KitchenProps.C(t, new Vector3(0f, 0.91f, 0f), new Vector3(0.6f, 0.03f, 0.6f), LabDark, 0f, "Tapa"); }));
        cult.Add(new LabBlock("Biorreactor piloto (50 L)", 1.8f, 1.2f, t => { Vessel(KitchenProps.Root(t, "Vaso", new Vector3(-0.4f, 0f, 0f)), 0.45f, 1.5f, LabSteel, "Biorreactor"); Tall(KitchenProps.Root(t, "Control", new Vector3(0.55f, 0f, 0f)), 0.6f, 0.6f, 1.5f, LabWhite, "Controlador", LabGreen); }));
        cult.AddRange(Times(2, "Biorreactores de sobremesa", 1.8f, 0.75f, t => { Bench(t, 1.8f, 0.75f, LabBench); for (int i = 0; i < 3; i++) Vessel(KitchenProps.Root(t, "Vaso", new Vector3(-0.55f + i * 0.55f, 0.94f, 0f)), 0.12f, 0.35f, LabGlass, "Vaso_sobremesa"); }));
        cult.AddRange(Times(2, "Congelador de −80 °C", 1.0f, 1.0f, t => Tall(t, 1.0f, 1.0f, 2.0f, LabWhite, "Ultracongelador", LabBlue)));
        cult.Add(new LabBlock("Cámara fría (4 °C)", 2.5f, 2.5f, t => Room(t, 2.5f, 2.5f, new Color(0.8f, 0.88f, 0.95f), "Camara_fria")));
        cult.Add(new LabBlock("Autoclave y fregadero", 2.5f, 0.9f, t => { Bench(t, 2.5f, 0.9f, LabSteel); KitchenProps.C(t, new Vector3(-0.6f, 1.4f, 0f), new Vector3(0.9f, 0.9f, 0.8f), LabSteel, 0f, "Autoclave"); KitchenProps.C(t, new Vector3(0.7f, 0.88f, 0f), new Vector3(0.6f, 0.1f, 0.5f), LabDark, 0f, "Pila"); }));
        cult.Add(new LabBlock("Lavamanos y lavaojos", 1.0f, 1.3f, SafetyShower));
        Transform g = new GameObject("Planta_cultivo").transform; g.SetParent(a.transform, false); g.localPosition = Vector3.up * 0.1f;
        LogLeft("Biología (cultivo)", PackFloor(g, bs, cult, CarpetKeepOut(bs)));
        // Piso de arriba: MOLECULAR, en sentido único (pre-PCR limpio → extracción → post-PCR y secuenciación).
        Transform up = InnerFloor(a.transform, "Planta_molecular", a.transform);
        Vector2 us = FloorSize(up, bs);
        Solid(up, new Vector3(0f, 0.05f, 0f), new Vector3(us.x, 0.1f, us.y), new Color(0.82f, 0.88f, 0.82f), "Suelo_molecular");
        var mol = new List<LabBlock>();
        mol.AddRange(Times(4, "Estación pre-PCR", 1.8f, 0.75f, t => { Bench(t, 1.8f, 0.75f, LabWhite); KitchenProps.C(t, new Vector3(0f, 1.3f, 0f), new Vector3(1.6f, 0.7f, 0.6f), LabGlass, 0f, "Campana_PCR"); }));
        mol.AddRange(Times(4, "Mesa de extracción de ADN", 1.8f, 0.75f, t => { Bench(t, 1.8f, 0.75f, LabBench); for (int i = 0; i < 2; i++) KitchenProps.C(t, new Vector3(-0.5f + i * 1f, 1.05f, 0f), new Vector3(0.35f, 0.2f, 0.35f), LabWhite, 0f, "Microcentrifuga"); }));
        mol.AddRange(Times(2, "Termocicladores", 1.8f, 0.75f, t => { Bench(t, 1.8f, 0.75f, LabBench); for (int i = 0; i < 2; i++) KitchenProps.C(t, new Vector3(-0.45f + i * 0.9f, 1.08f, 0f), new Vector3(0.45f, 0.25f, 0.5f), LabWhite, 0f, "Termociclador"); }));
        mol.Add(new LabBlock("Secuenciadores (MiSeq y MinION)", 1.8f, 0.75f, t => Instrument(t, 1.8f, 0.75f, 0.6f, LabWhite, "Secuenciador")));
        mol.Add(new LabBlock("Dewar de nitrógeno líquido", 0.6f, 0.6f, t => Vessel(t, 0.3f, 1.2f, LabSteel, "Dewar")));
        mol.Add(new LabBlock("Lavamanos y lavaojos (arriba)", 1.0f, 1.3f, SafetyShower));
        mol.AddRange(Times(2, "Nevera y congelador de −20 °C", 0.7f, 0.75f, t => Tall(t, 0.7f, 0.75f, 1.8f, LabWhite, "Nevera_reactivos", LabBlue)));
        mol.AddRange(Times(4, "Puesto de análisis de datos", 1.6f, 1.0f, t => { Bench(t, 1.6f, 1f, new Color(0.55f, 0.45f, 0.35f)); KitchenProps.C(t, new Vector3(0f, 1.15f, 0.25f), new Vector3(0.6f, 0.4f, 0.04f), LabDark, 0f, "Monitor"); }));
        LogLeft("Biología (molecular)", PackFloor(up, us, mol, CarpetKeepOut(us, -us.x * 0.3f)));
        return a;
    }

    // ── FÍSICA: la sala de experimentos, tercer piso de la planta atómica (y de la torre de Ciencias), con su anillo ─────────────────────────────

    static void BuildPhysicsHall(Transform plant)
    {
        Transform cyc = plant.Find("Planta_ciclotron");
        if (cyc == null) return;
        // El tercer piso de la torre de Ciencias, encima del ciclotrón.
        Transform hall = InnerFloor(plant, "Sala_experimentos_fisica", cyc);
        Vector2 hs = FloorSize(hall, CienciasSize);
        Solid(hall, new Vector3(0f, 0.05f, 0f), new Vector3(hs.x, 0.1f, hs.y), new Color(0.3f, 0.32f, 0.36f), "Suelo_oscuro");   // zona oscura para la óptica
        var inside = new List<LabBlock>();
        inside.AddRange(Times(2, "Interferómetro de Michelson", 2.44f, 1.22f, t => OpticalTable(t, 2.44f, 1.22f)));
        inside.AddRange(Times(2, "Gota de aceite de Millikan", 1.5f, 0.75f, t => { Bench(t, 1.5f, 0.75f, LabBench); KitchenProps.C(t, new Vector3(-0.2f, 1.1f, 0f), new Vector3(0.3f, 0.3f, 0.3f), LabSteel, 0f, "Condensador"); KitchenProps.C(t, new Vector3(0.35f, 1.2f, 0f), new Vector3(0.08f, 0.08f, 0.35f), LabDark, 90f, "Microscopio_medida"); }));
        inside.AddRange(Times(2, "Espectroscopio de red", 1.5f, 0.75f, t => { Bench(t, 1.5f, 0.75f, LabBench); KitchenProps.C(t, new Vector3(0f, 1.0f, 0f), new Vector3(0.4f, 0.06f, 0.4f), LabSteel, 0f, "Plataforma_goniometro"); KitchenProps.C(t, new Vector3(-0.35f, 1.1f, 0f), new Vector3(0.4f, 0.08f, 0.08f), LabDark, 0f, "Colimador"); KitchenProps.C(t, new Vector3(0.3f, 1.1f, 0.15f), new Vector3(0.4f, 0.08f, 0.08f), LabDark, 30f, "Anteojo"); KitchenProps.C(t, new Vector3(-0.65f, 1.05f, 0f), new Vector3(0.15f, 0.2f, 0.15f), new Color(1f, 0.6f, 0.9f), 0f, "Lampara_espectral"); }));
        inside.Add(new LabBlock("Electrónica y escritorios", 3.0f, 1.5f, t => { Bench(t, 3f, 1.5f, new Color(0.55f, 0.45f, 0.35f)); Tall(KitchenProps.Root(t, "Rack", new Vector3(1.0f, 0.94f, 0f)), 0.6f, 0.6f, 1f, LabDark, "Rack", LabGreen); }));
        var hallKeep = CarpetKeepOut(hs, -hs.x * 0.3f);
        LogLeft("Física (sala)", PackFloor(hall, hs, inside, hallKeep));
        // El ANILLO (10 m, 20 % dentro): la doble rendija de fotón único, Chladni, helio superfluido, Cavendish y, en una
        // esquina, el péndulo de Foucault con su torre (15 m de hilo, foso de 3 m).
        Transform ring = RingDeck(hall, "Anillo_experimentos", hs);
        // Sin escaleras (Berón 2026-10-28): al anillo se sube en alfombra.
        RingCarpets(ring, hall, hs, plant.GetComponent<SanctuaryArea>() is SanctuaryArea psa ? psa.displayName : "Planta Atómica", "Anillo de experimentos");
        var onRing = new List<LabBlock>
        {
            new LabBlock("Doble rendija de fotón único", 4.5f, 0.8f, t => { Bench(t, 4.5f, 0.8f, LabDark); KitchenProps.C(t, new Vector3(-2f, 1.05f, 0f), new Vector3(0.3f, 0.15f, 0.15f), LabRed, 0f, "Laser_atenuado"); KitchenProps.C(t, new Vector3(0f, 1.1f, 0f), new Vector3(0.03f, 0.25f, 0.2f), LabSteel, 0f, "Rendijas"); KitchenProps.C(t, new Vector3(2f, 1.1f, 0f), new Vector3(0.25f, 0.25f, 0.25f), LabDark, 0f, "Contador_fotones"); KitchenProps.C(t, new Vector3(0f, 1.25f, 0f), new Vector3(4.4f, 0.5f, 0.7f), new Color(0.1f, 0.1f, 0.12f, 0.5f), 0f, "Tubo_oscuro"); }),
            new LabBlock("Placas de Chladni", 1.5f, 0.75f, t => { Bench(t, 1.5f, 0.75f, LabBench); KitchenProps.C(t, new Vector3(0f, 1.1f, 0f), new Vector3(0.5f, 0.02f, 0.5f), LabSteel, 0f, "Placa"); KitchenProps.C(t, new Vector3(0f, 1.0f, 0f), new Vector3(0.12f, 0.15f, 0.12f), LabDark, 0f, "Excitador"); KitchenProps.C(t, new Vector3(0.55f, 1.05f, 0f), new Vector3(0.3f, 0.2f, 0.3f), LabDark, 0f, "Generador_funciones"); }),
            new LabBlock("Helio superfluido", 2.5f, 1.5f, t => { Vessel(KitchenProps.Root(t, "Criostato", new Vector3(-0.6f, 0f, 0f)), 0.4f, 1.8f, LabSteel, "Criostato_vidrio"); Tall(KitchenProps.Root(t, "Bomba", new Vector3(0.7f, 0f, 0f)), 0.8f, 0.7f, 1.0f, LabWhite, "Bomba_vacio", LabGreen); }),
            new LabBlock("Balanza de torsión de Cavendish", 4.0f, 1.0f, t => { KitchenProps.C(t, new Vector3(-1.4f, 0.4f, 0f), new Vector3(1.0f, 0.8f, 1f), new Color(0.6f, 0.6f, 0.58f), 0f, "Pilar_anclado"); KitchenProps.C(t, new Vector3(-1.4f, 1.2f, 0f), new Vector3(0.5f, 0.8f, 0.5f), LabGlass, 0f, "Caja_torsion"); for (int s = -1; s <= 1; s += 2) KitchenProps.C(t, new Vector3(-1.4f + s * 0.35f, 1.0f, 0f), new Vector3(0.18f, 0.18f, 0.18f), LabDark, 0f, "Esfera_plomo"); KitchenProps.C(t, new Vector3(1.6f, 0.8f, 0f), new Vector3(0.1f, 1.6f, 0.6f), Color.white, 0f, "Escala_laser"); }),
            new LabBlock("Electrónica del anillo", 1.0f, 1.0f, t => Tall(t, 1f, 1f, 1.8f, LabDark, "Rack", LabGreen)),
        };
        LogLeft("Física (anillo)", PackRing(ring, hs, onRing));
        // Péndulo de Foucault en la esquina del anillo más alejada de su área de alfombras: torre de 17 m, hilo de 15 m.
        float cx = hs.x * 0.5f - 2f + 5f, cz = hs.y * 0.5f - 2f + 5f;
        Transform fp = KitchenProps.Root(ring, "Bloque_Péndulo de Foucault", new Vector3(cx, 0f, -cz));
        KitchenProps.C(fp, new Vector3(0f, 0.02f, 0f), new Vector3(3f, 0.04f, 3f), new Color(0.75f, 0.7f, 0.55f), 0f, "Foso_rosa_de_los_vientos");
        for (int i = 0; i < 4; i++) Solid(fp, new Vector3((i % 2 - 0.5f) * 3.2f, 8.5f, (i / 2 - 0.5f) * 3.2f), new Vector3(0.25f, 17f, 0.25f), LabSteel, "Pilar_torre");
        Solid(fp, new Vector3(0f, 17f, 0f), new Vector3(3.6f, 0.3f, 3.6f), LabSteel, "Coronacion");
        KitchenProps.C(fp, new Vector3(0f, 9.4f, 0f), new Vector3(0.02f, 15f, 0.02f), LabDark, 0f, "Hilo");
        KitchenProps.C(fp, new Vector3(0f, 1.7f, 0f), new Vector3(0.4f, 0.4f, 0.4f), new Color(0.78f, 0.62f, 0.3f), 45f, "Bola_de_laton");
        WorldLabel pl = fp.gameObject.AddComponent<WorldLabel>(); pl.text = "Péndulo de Foucault (15 m)"; pl.height = 3f;
    }

    // ── CENTRAL NUCLEAR DE PRODUCCIÓN (microrreactor) ─────────────────────────────────────────────────────────────

    static SanctuaryArea BuildNuclearPlant(Transform areasGroup)
    {
        SanctuaryArea a = BuildAreaBox(areasGroup, "NuclearPower_Area", "Central Nuclear", SanctuaryAreaType.NuclearPower, 4, NuclearCenter, NuclearSize, new Color(0.75f, 0.78f, 0.8f));
        Elevate(a.transform, NuclearSize, ElevatedHeight);
        Transform g = new GameObject("Planta_central").transform; g.SetParent(a.transform, false); g.localPosition = Vector3.up * 0.1f;
        Color concrete = new Color(0.7f, 0.7f, 0.68f);
        var blocks = new List<LabBlock>
        {
            new LabBlock("Puesto de acceso", 6f, 3f, t => { Room(t, 4f, 3f, concrete, "Garita"); KitchenProps.C(t, new Vector3(1.75f, 1f, -1.6f), new Vector3(2.5f, 0.1f, 0.1f), LabYellow, 0f, "Barrera"); }),
            new LabBlock("Sala de control", 10f, 8f, t => { Room(t, 10f, 8f, concrete, "Sala_control"); for (int i = 0; i < 4; i++) { KitchenProps.C(t, new Vector3(-3.6f + i * 2.4f, 0.45f, 0.5f), new Vector3(1.6f, 0.9f, 0.8f), LabDark, 0f, "Consola_operador"); KitchenProps.C(t, new Vector3(-3.6f + i * 2.4f, 1.15f, 0.75f), new Vector3(1.2f, 0.5f, 0.05f), LabBlue, 0f, "Pantallas"); } KitchenProps.C(t, new Vector3(0f, 1.5f, 3.75f), new Vector3(8f, 1.2f, 0.1f), LabBlue, 0f, "Panel_mimico"); }),
            new LabBlock("Microrreactor (5 MWe)", 12f, 4.5f, t => { KitchenProps.C(t, new Vector3(0f, 0.3f, 0f), new Vector3(12f, 0.6f, 4.5f), concrete, 0f, "Losa_blindada"); KitchenProps.C(t, new Vector3(0f, 2.1f, 0f), new Vector3(10f, 3f, 3.2f), new Color(0.55f, 0.6f, 0.65f), 0f, "Modulo_reactor"); KitchenProps.C(t, new Vector3(-5.2f, 2.1f, 0f), new Vector3(0.6f, 3.4f, 3.4f), LabYellow, 0f, "Tapa_trebol"); KitchenProps.C(t, new Vector3(0f, 3.8f, 0f), new Vector3(10f, 0.4f, 0.6f), LabSteel, 0f, "Tubos_calor"); }),
            new LabBlock("Conversión de potencia", 12f, 3.5f, t => { KitchenProps.C(t, new Vector3(0f, 1.6f, 0f), new Vector3(12f, 3.2f, 3.5f), LabSteel, 0f, "Contenedor_turbina"); KitchenProps.C(t, new Vector3(4f, 3.6f, 0f), new Vector3(2.5f, 0.8f, 2.5f), LabDark, 0f, "Aerorrefrigerador"); }),
            new LabBlock("Intercambiador de la calefacción de distrito", 8f, 6f, t => { for (int i = 0; i < 2; i++) Vessel(KitchenProps.Root(t, "Deposito", new Vector3(-2f + i * 4f, 0f, 0f)), 1.2f, 3.5f, new Color(0.8f, 0.5f, 0.3f), "Intercambiador"); KitchenProps.C(t, new Vector3(0f, 2.5f, 0f), new Vector3(7f, 0.3f, 0.3f), new Color(0.8f, 0.3f, 0.25f), 0f, "Tuberia_agua_caliente"); }),
            new LabBlock("Subestación", 6f, 4f, t => { for (int i = 0; i < 2; i++) KitchenProps.C(t, new Vector3(-1.5f + i * 3f, 1.2f, 0f), new Vector3(1.8f, 2.4f, 1.8f), new Color(0.4f, 0.45f, 0.4f), 0f, "Transformador"); KitchenProps.C(t, new Vector3(0f, 1.0f, 2f), new Vector3(6f, 2f, 0.05f), LabSteel, 0f, "Valla_subestacion"); }),
            new LabBlock("Diésel de respaldo", 6f, 3f, t => { KitchenProps.C(t, new Vector3(0f, 1.3f, 0f), new Vector3(6f, 2.6f, 3f), new Color(0.3f, 0.5f, 0.35f), 0f, "Grupo_electrogeno"); KitchenProps.C(t, new Vector3(2.4f, 3f, 0f), new Vector3(0.3f, 1f, 0.3f), LabDark, 0f, "Escape"); }),
            new LabBlock("Losa de contenedores del núcleo", 15f, 6f, t => { KitchenProps.C(t, new Vector3(0f, 0.15f, 0f), new Vector3(15f, 0.3f, 6f), concrete, 0f, "Losa"); for (int i = 0; i < 2; i++) KitchenProps.C(t, new Vector3(-4f + i * 8f, 1.6f, 0f), new Vector3(6f, 2.6f, 2.6f), new Color(0.5f, 0.52f, 0.55f), 0f, "Contenedor_transporte"); }),
            new LabBlock("Almacén", 6f, 4f, t => Room(t, 6f, 4f, concrete, "Almacen")),
        };
        LogLeft("Central nuclear", PackFloor(g, NuclearSize, blocks, CarpetKeepOut(NuclearSize)));
        // El límite del sitio (la zona de exclusión del juego) va PINTADO en el suelo, no con valla: en el santuario no hay
        // barandillas en las plataformas (Berón 2026-10-22). El control de acceso es el puesto de la entrada.
        Transform line = new GameObject("Limite_del_sitio").transform; line.SetParent(g, false);
        for (int s = -1; s <= 1; s += 2)
        {
            KitchenProps.C(line, new Vector3(0f, 0.01f, s * (NuclearSize.y * 0.5f - 0.6f)), new Vector3(NuclearSize.x - 1.2f, 0.02f, 0.15f), LabYellow, 0f, "Linea_limite");
            KitchenProps.C(line, new Vector3(s * (NuclearSize.x * 0.5f - 0.6f), 0.01f, 0f), new Vector3(0.15f, 0.02f, NuclearSize.y - 1.2f), LabYellow, 0f, "Linea_limite");
        }
        WorldLabel wl = a.gameObject.AddComponent<WorldLabel>(); wl.text = "Central nuclear (microrreactor de 5 MWe + calefacción de distrito)"; wl.height = 8f;
        return a;
    }
}
