using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL ÁRBOL DE LOS CAMPOS (Berón 2026-10-29; docs/edificio-arbol.md, medidas en <see cref="TreeSpec"/>): los campos de cultivo
/// en un solo edificio con forma de árbol, en el lóbulo Sur:Milpa.
/// - RAÍZ: columna de agua de Ø 20 m (pared de acrílico de 1 m, transparente como el tanque de virtualización: se ve nadar a
///   la gente de piso en piso), de 0 a 101 m, sostenida por 16 nervios de acero (CFST) con zunchos de canto cada 6 m. Una PUERTA por
///   nivel con su campo magnético (el agua no sale); el hueco de cada puerta es una campana de aire.
/// - TRONCO: 8 pisos de cultivo (octógonos de ~51 m) en voladizo desde la raíz, girados 45° y con 3° de inclinación
///   alterna; la canaleta del borde bajo de cada uno riega el de abajo (cascada).
/// - RAMAS: 6 tubos de agua a 35° desde 66 m, de 40 m, con una campana y un anillo de nidos (RingDeck) en la punta: frutales
///   enanos, flores para las abejas de las nieves y nidos.
/// - COPA: plataforma octogonal plana de 22 m con las alfombras (de edificio y del santuario). Abajo, en el hielo, las del
///   suelo. Entre pisos, sin alfombras: por la raíz, nadando.
/// Se construye SIN POSAR (pisos sin girar ni inclinar: los muebles, las alfombras y las plazas reservadas se calculan
/// alineados) y <see cref="PoseTree"/> lo deja en su forma al final de <see cref="PlaceCarpetAreas"/>.
/// </summary>
public static partial class SampleSceneBuilder
{
    static Transform _treeRoot;
    static bool _treePosed;
    static readonly List<TreeFloor> _treeFloors = new List<TreeFloor>();

    static readonly Color TreeSteel = new Color(0.36f, 0.39f, 0.44f), TreeCopper = new Color(0.72f, 0.45f, 0.22f),
        TreeGlow = new Color(0.55f, 0.85f, 1f, 0.35f), TreeDeck = new Color(0.48f, 0.4f, 0.33f), TreeSlab = new Color(0.66f, 0.6f, 0.52f);

    /// <summary>El agua de la raíz: el mismo tono que el del tanque, más clara (20 m de agua: si no, no se vería nadar a nadie).</summary>
    static Color RootWater => new Color(VirtualizationTank.WaterColor.r, VirtualizationTank.WaterColor.g, VirtualizationTank.WaterColor.b, 0.2f);

    /// <summary>Levanta el árbol con el área del Huerto (<paramref name="garden"/>) en su piso 5. Devuelve la raíz.</summary>
    static Transform BuildTree(Transform parent, SanctuaryArea garden)
    {
        _treeFloors.Clear(); _treePosed = false;
        Transform tree = new GameObject(TreeSpec.RootName).transform;
        tree.SetParent(parent, false); tree.position = TreeSpec.Site;
        ArbolCampos ac = tree.gameObject.AddComponent<ArbolCampos>();
        tree.gameObject.AddComponent<TreeRotator>();
        Rigidbody rb = tree.gameObject.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false;   // gira: sus colliders, de un cuerpo cinemático
        _treeRoot = tree;

        // ── HUECOS DE LA PARED: una puerta por nivel (suelo y 8 pisos) y la boca de cada rama ──
        var gaps = new List<(float ang, float half, float y0, float y1)>();
        float doorHalf = Mathf.Asin(TreeSpec.DoorWidth * 0.5f / TreeSpec.OuterRadius) * Mathf.Rad2Deg;
        for (int k = 0; k <= 8; k++) gaps.Add((TreeSpec.DoorAngle(k), doorHalf, TreeSpec.DoorBottom(k), TreeSpec.DoorBottom(k) + TreeSpec.DoorHeight));
        for (int b = 0; b < 6; b++) gaps.Add((TreeSpec.BranchYaw[b], 12.5f, TreeSpec.BranchBaseY - 2.5f, TreeSpec.BranchBaseY + 2.5f));

        Root(tree, ac, gaps);
        // ── NIVELES: el suelo y los 8 pisos ──
        ac.levels.Clear();
        ac.levels.Add(Level(TreeLevel.Kind.Ground, 0, "el suelo", null));
        for (int k = 1; k <= 8; k++)
        {
            Transform f = TreeFloorAt(tree, k, garden);
            ac.levels.Add(Level(TreeLevel.Kind.Floor, k, $"el piso {k} ({GardenPlot.Title(TreeSpec.Guilds[k - 1])})", f));
        }
        for (int k = 0; k <= 8; k++) ac.levels[k].field = DoorFrame(tree, TreeSpec.DoorAngle(k), TreeSpec.DoorBottom(k), k == 0 ? "Puerta_suelo" : $"Puerta_piso_{k}");
        for (int b = 0; b < 6; b++) ac.levels.Add(Branch(tree, b));
        ac.top = Top(tree);
        ac.levels.Add(new TreeLevel { name = "la copa", kind = TreeLevel.Kind.Top, index = 9, deck = ac.top, outside = new Vector3(0f, TreeSpec.RootHeight + 0.3f, 0f) });

        // Alfombras del SUELO, en el hielo (no giran): a la copa. (Las de la copa giran con el árbol.)
        Vector3 padAt = TreeSpec.Site + TreeSpec.Dir(45f) * 22f + Vector3.up * 0.01f;
        // (Su propio padre, sin colliders: si colgara de «Areas_Placed», CarpetPad.OnDeck la tomaría por la pista de CUALQUIER piso
        // sin pista propia y mandaría a la gente al pie del árbol.)
        Transform padRoot = new GameObject("Arbol_alfombras_suelo").transform; padRoot.SetParent(parent, false);
        CarpetPad.BuildZone(padRoot, padAt, Quaternion.LookRotation(TreeSpec.Site - new Vector3(padAt.x, 0f, padAt.z)), CarpetPad.Kind.Edificio, TreeSpec.Building, 0, $"{TreeSpec.Building}#0", "Suelo (hielo)", 2);
        // …y las del SANTUARIO, al lado (el mismo suelo): desde las otras torres se llega volando AL PIE del árbol, que es por
        // donde se entra a los pisos (antes había que bajar a la copa y volar otra vez al suelo: dos vuelos para llegar al pie).
        Vector3 padS = TreeSpec.Site + TreeSpec.Dir(69f) * 22f + Vector3.up * 0.01f;
        CarpetPad.BuildZone(padRoot, padS, Quaternion.LookRotation(TreeSpec.Site - new Vector3(padS.x, 0f, padS.z)), CarpetPad.Kind.Santuario, TreeSpec.Building, 0, $"{TreeSpec.Building}#0",
            $"{TreeSpec.Building} (al pie: a los pisos, nadando por la raíz)", 2);

        // LA RAÍZ AGUANTA (StructuralLimits.RootCheck): 8 pisos, 6 ramas, la copa, su pared y el agua.
        StructuralLimits.RootReport rep = StructuralLimits.RootCheck(8, 6, TreeSpec.WaterRadius, 100f, TreeSpec.WallThickness, TreeSpec.Ribs, 51f, TreeSpec.HoopSpacing, TreeSpec.HoopArea);
        if (rep.Fail) Debug.LogError($"[Estructura] La raíz del Árbol de los Campos no aguanta: {rep}. Refuérzala (más nervios o más pared).");
        else if (rep.Warn) Debug.LogWarning($"[Estructura] La raíz del Árbol de los Campos va justa: {rep}.");
        else Debug.Log($"[Estructura] Raíz del Árbol de los Campos (Ø {2f * TreeSpec.OuterRadius:0} m, {TreeSpec.Ribs} nervios): {rep}.");
        Debug.Log($"[Árbol] «{TreeSpec.Building}» en ({TreeSpec.Site.x:0}, {TreeSpec.Site.z:0}): raíz de {TreeSpec.RootHeight:0} m, 8 pisos ({string.Join(" · ", System.Array.ConvertAll(TreeSpec.Guilds, g => GardenPlot.Title(g)))}), 6 ramas con anillos, copa con alfombras.");
        return tree;
    }

    static TreeLevel Level(TreeLevel.Kind kind, int k, string name, Transform deck)
    {
        float ang = TreeSpec.DoorAngle(k), y = TreeSpec.DoorBottom(k);
        return new TreeLevel
        {
            name = name, kind = kind, index = k, deck = deck, doorAngle = ang,
            outside = TreeSpec.OnFloorAtDoor(k, TreeSpec.OuterRadius + 2.5f),
            door = TreeSpec.Dir(ang) * (TreeSpec.WaterRadius + 0.5f) + Vector3.up * y,
            inside = TreeSpec.Dir(ang) * (TreeSpec.WaterRadius - 1.5f) + Vector3.up * (y + 0.2f),
        };
    }

    // ── LA RAÍZ ───────────────────────────────────────────────────────────────────────────────────────────────

    static void Root(Transform tree, ArbolCampos ac, List<(float ang, float half, float y0, float y1)> gaps)
    {
        float rIn = TreeSpec.WaterRadius, rOut = TreeSpec.OuterRadius, H = TreeSpec.RootHeight;
        // PARED de acrílico transparente (el cristal del tanque de virtualización), con colliders: se entra solo por las puertas.
        GameObject wall = new GameObject("Pared_raiz");
        wall.transform.SetParent(tree, false);
        Mesh wm = TubeMesh("Pared_raiz", rIn, rOut, 0f, H, 72, gaps);
        wall.AddComponent<MeshFilter>().sharedMesh = wm;
        wall.AddComponent<MeshRenderer>().sharedMaterial = KitchenProps.Mat(VirtualizationTank.GlassColor);
        wall.AddComponent<MeshCollider>().sharedMesh = wm;
        ac.wall = wall.transform;

        // AGUA (se ve) y su VOLUMEN nadable (WaterZone: la superficie es lo más alto del volumen).
        float wy0 = TreeSpec.WaterBottom, wy1 = TreeSpec.WaterTop;
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        water.name = "Agua_raiz"; water.transform.SetParent(tree, false);
        water.transform.localPosition = Vector3.up * ((wy0 + wy1) * 0.5f);
        water.transform.localScale = new Vector3(2f * rIn, (wy1 - wy0) * 0.5f, 2f * rIn);
        Object.DestroyImmediate(water.GetComponent<Collider>());
        water.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(RootWater);
        ac.water = water.transform;
        WaterVolume(tree, "Agua_raiz_volumen", water);

        // TAPA de cristal arriba (101 m: entre el agua y la tapa queda una campana de aire de 1,4 m) y fondo.
        Transform cap = KitchenProps.C(tree, Vector3.up * (H + 0.05f), new Vector3(2f * rOut, 0.1f, 2f * rOut), VirtualizationTank.GlassColor, 0f, "Tapa_raiz");
        KitchenProps.C(tree, Vector3.up * 0.02f, new Vector3(2f * rIn, 0.04f, 2f * rIn), new Color(0.3f, 0.42f, 0.48f), 0f, "Fondo_raiz");
        KitchenProps.C(tree, Vector3.down * 0.4f, new Vector3(2f * rOut + 3f, 0.8f, 2f * rOut + 3f), new Color(0.72f, 0.71f, 0.68f), 0f, "Encepado_raiz");   // el cimiento (bajo el hielo)
        _ = cap;

        // EXOESQUELETO: 16 nervios de acero (CFST Ø 0,8 m) y zunchos de canto cada 6 m (los que sujetan el empuje del agua con el acrílico; StructuralLimits.RootCheck), una sola
        // malla; no cruzan los huecos de las puertas ni las bocas de las ramas (ni la losa de los pisos).
        var boxes = new List<(Vector3 c, Vector3 s, Quaternion q)>();
        bool InGap(float ang, float y0, float y1, float pad)
        {
            foreach (var g in gaps) if (Mathf.Abs(Mathf.DeltaAngle(ang, g.ang)) < g.half + pad && y1 > g.y0 - 0.1f && y0 < g.y1 + 0.1f) return true;
            return false;
        }
        bool AtFloor(float y) { foreach (float fy in TreeSpec.FloorY) if (y > fy - 1.4f && y < fy + 0.9f) return true; return false; }
        var hoopY = new List<float>();
        for (float y = 3f; y < H - 0.5f; y += TreeSpec.HoopSpacing) hoopY.Add(AtFloor(y) ? y + 2.4f : y);   // cada 6 m (si cae en una losa, justo encima)
        hoopY.Add(H - 0.3f);
        for (int i = 0; i < TreeSpec.Ribs; i++)
        {
            float ang = 11.25f + i * 22.5f; Vector3 d = TreeSpec.Dir(ang);
            for (float y = 0f; y < H; y += 4f)
            {
                float y1 = Mathf.Min(H, y + 4f);
                if (InGap(ang, y, y1, 2f)) continue;
                boxes.Add((d * (rOut + 0.3f) + Vector3.up * ((y + y1) * 0.5f), new Vector3(0.55f, y1 - y, 0.6f), Quaternion.Euler(0f, ang, 0f)));
            }
        }
        foreach (float y in hoopY)
            for (int s = 0; s < 36; s++)
            {
                float ang = s * 10f + 5f;
                if (InGap(ang, y - 0.3f, y + 0.3f, 6f)) continue;
                { float rh = rOut + TreeSpec.HoopDepth * 0.5f;   // de canto: delgado a la vista, hondo hacia fuera
                  boxes.Add((TreeSpec.Dir(ang) * rh + Vector3.up * y, new Vector3(2f * rh * Mathf.Tan(5f * Mathf.Deg2Rad) + 0.05f, TreeSpec.HoopHeight, TreeSpec.HoopDepth), Quaternion.Euler(0f, ang, 0f))); }
            }
        GameObject exo = new GameObject("Exoesqueleto");
        exo.transform.SetParent(tree, false);
        exo.AddComponent<MeshFilter>().sharedMesh = BoxesMesh("Exoesqueleto", boxes);
        exo.AddComponent<MeshRenderer>().sharedMaterial = KitchenProps.Mat(TreeSteel);
        Label(KitchenProps.C(tree, TreeSpec.Dir(0f) * (rOut + 0.8f) + Vector3.up * 3.4f, new Vector3(0.1f, 0.1f, 0.1f), TreeSteel, 0f, "Cartel_raiz"),
              "Raíz del Árbol de los Campos: entra por la puerta y nada a tu piso", 0.4f);
    }

    /// <summary>Volumen nadable (WaterZone) con la forma de la malla de <paramref name="shape"/> (convexa, trigger).</summary>
    static void WaterVolume(Transform parent, string name, GameObject shape)
    {
        GameObject v = new GameObject(name);
        v.transform.SetParent(parent, false);
        v.transform.localPosition = shape.transform.localPosition; v.transform.localRotation = shape.transform.localRotation; v.transform.localScale = shape.transform.localScale;
        MeshCollider mc = v.AddComponent<MeshCollider>();
        mc.sharedMesh = shape.GetComponent<MeshFilter>().sharedMesh; mc.convex = true; mc.isTrigger = true;
        v.AddComponent<WaterZone>();
    }

    /// <summary>Marco de una puerta de la raíz (bobinas de cobre a los lados y arriba, como el tanque) y su campo magnético en
    /// la cara de dentro: lo cruzan los cuerpos, no el agua. Devuelve el campo.</summary>
    static Transform DoorFrame(Transform tree, float ang, float y, string name)
    {
        Transform d = new GameObject(name).transform; d.SetParent(tree, false);
        d.localPosition = TreeSpec.Dir(ang) * TreeSpec.WaterRadius + Vector3.up * y; d.localRotation = Quaternion.Euler(0f, ang, 0f);
        float w = 2f * TreeSpec.WaterRadius * Mathf.Sin(5f * Mathf.Deg2Rad), h = TreeSpec.DoorHeight;   // el hueco de la pared: 2 celdas de 5°
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(d, new Vector3(s * (w * 0.5f + 0.06f), h * 0.5f, 0.5f), new Vector3(0.12f, h, 1f), TreeCopper, 0f, "Bobina");
        KitchenProps.C(d, new Vector3(0f, h + 0.06f, 0.5f), new Vector3(w + 0.24f, 0.12f, 1f), TreeCopper, 0f, "Bobina");
        Transform field = KitchenProps.C(d, new Vector3(0f, h * 0.5f, 0.02f), new Vector3(w, h, 0.02f), TreeGlow, 0f, "Campo_magnetico");
        field.gameObject.AddComponent<CampoMagnetico>();
        return field;
    }

    // ── LOS PISOS ─────────────────────────────────────────────────────────────────────────────────────────────

    static Transform TreeFloorAt(Transform tree, int k, SanctuaryArea garden)
    {
        GardenPlot.Guild guild = TreeSpec.Guilds[k - 1];
        Transform f = new GameObject($"Piso_arbol_{k}_{guild}").transform;
        f.SetParent(tree, false); f.localPosition = Vector3.up * TreeSpec.FloorY[k - 1];   // SIN POSAR (PoseTree lo gira e inclina)
        TreeFloor tf = f.gameObject.AddComponent<TreeFloor>();
        tf.index = k; tf.tilt = TreeSpec.TiltOf(k); tf.yaw = TreeSpec.YawOf(k);
        float half = TreeSpec.FloorHalf, X = half + TreeSpec.FloorEdge, c = 2f * TreeSpec.FloorEdge;
        // LOSA: octógono (como las de las torres) con el hueco de la raíz; en voladizo desde ella: ménsulas y tornapuntas.
        Transform s = new GameObject("Estructura_Elevada").transform; s.SetParent(f, false);
        GameObject slab = new GameObject("Losa_arbol"); slab.transform.SetParent(s, false);
        Mesh sm = SlabMesh($"Losa_arbol_{k}", X, c, TreeSpec.OuterRadius, 0.28f);
        slab.AddComponent<MeshFilter>().sharedMesh = sm;
        slab.AddComponent<MeshRenderer>().sharedMaterial = KitchenProps.Mat(TreeSlab);
        slab.AddComponent<MeshCollider>().sharedMesh = sm;
        for (int i = 0; i < 8; i++)
        {
            float ang = 22.5f + i * 45f; Vector3 dir = TreeSpec.Dir(ang); Quaternion q = Quaternion.Euler(0f, ang, 0f);
            float r0 = TreeSpec.OuterRadius, r1 = 23.5f;
            KitchenProps.C(s, dir * ((r0 + r1) * 0.5f) + Vector3.down * 0.58f, new Vector3(0.35f, 0.6f, r1 - r0), TreeSteel, q, "Mensula");
            Vector3 a = dir * (r0 + 0.2f) + Vector3.down * 4f, b = dir * 17f + Vector3.down * 0.9f;
            KitchenProps.C(s, (a + b) * 0.5f, new Vector3(0.3f, 0.3f, (b - a).magnitude), TreeSteel, Quaternion.LookRotation(b - a), "Tornapunta");
        }
        // CANALETA en el borde que quedará BAJO (la inclinación +3° baja el lado +Z): recoge el agua y la vierte al de abajo.
        float zLow = tf.tilt > 0f ? X - 0.3f : -(X - 0.3f);
        KitchenProps.C(s, new Vector3(0f, 0.06f, zLow), new Vector3(2f * (X - c), 0.25f, 0.4f), TreeSteel, 0f, "Canaleta");
        _platforms[f] = new Vector2(2f * half, 2f * half);   // sus plazas reservadas (regla 8) sobre el rectángulo de 46 × 46

        // EL CULTIVO del piso: una franja de 9 m en el lado +Z, el de su puerta (se sale de la raíz al bancal: sin rodear la raíz).
        if (guild == GardenPlot.Guild.Hortalizas && garden != null)
        {
            garden.transform.SetParent(f, false);
            garden.transform.localPosition = new Vector3(0f, 0f, 18f);
            garden.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);   // su lado abierto (+Z del área) mira a la raíz
            GameObject go = MakePlot(garden.transform, guild, garden.transform.TransformPoint(new Vector3(-6f, 0.1f, 0f)), new Vector2(6f, 28f), TreeSpec.Seeds[k - 1], false);
            go.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);   // (sus filas, a lo largo de la franja: caben todas las del gremio)
        }
        else
        {
            GameObject go = MakePlot(f, guild, f.TransformPoint(new Vector3(0f, 0.06f, 18f)), new Vector2(7f, 38f), TreeSpec.Seeds[k - 1], false);
            go.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);   // (las filas, a lo largo de la franja: caben todas las del gremio)
            if (k == 8) go.GetComponent<GardenPlot>().speciesOffset = 5;   // el segundo bosque comestible: los otros árboles del dosel
            Transform soil = KitchenProps.C(go.transform, new Vector3(0f, -0.03f, 0f), new Vector3(9f, 0.06f, 40f),
                guild == GardenPlot.Guild.Arrozal ? new Color(0.62f, 0.78f, 0.84f) : new Color(0.70f, 0.64f, 0.58f), 0f, "Suelo_Huerto");
            _ = soil;
        }
        _treeFloors.Add(tf);
        return f;
    }

    // ── LAS RAMAS ─────────────────────────────────────────────────────────────────────────────────────────────

    static TreeLevel Branch(Transform tree, int b)
    {
        Vector3 S = TreeSpec.BranchStart(b), E = TreeSpec.BranchEnd(b), dir = TreeSpec.BranchDir(b), h = TreeSpec.Dir(TreeSpec.BranchYaw[b]);
        float L = TreeSpec.BranchLength + 1.2f;   // empieza 1,2 m dentro: la boca entra en el agua de la raíz
        Vector3 S0 = S - dir * 1.2f;
        Quaternion q = Quaternion.FromToRotation(Vector3.up, dir);
        // TUBO de agua (acrílico, como la raíz) y su agua.
        Transform tube = new GameObject($"Rama_{b + 1}").transform; tube.SetParent(tree, false);
        tube.localPosition = S0; tube.localRotation = q;
        Mesh tm = TubeMesh($"Rama_{b + 1}", TreeSpec.BranchWater, TreeSpec.BranchRadius, 0f, L, 24, null);
        tube.gameObject.AddComponent<MeshFilter>().sharedMesh = tm;
        tube.gameObject.AddComponent<MeshRenderer>().sharedMaterial = KitchenProps.Mat(VirtualizationTank.GlassColor);
        tube.gameObject.AddComponent<MeshCollider>().sharedMesh = tm;
        GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        w.name = "Agua_rama"; w.transform.SetParent(tube, false);
        w.transform.localPosition = Vector3.up * (L * 0.5f); w.transform.localScale = new Vector3(2f * TreeSpec.BranchWater, L * 0.5f, 2f * TreeSpec.BranchWater);
        Object.DestroyImmediate(w.GetComponent<Collider>());
        w.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(RootWater);
        GameObject vol = new GameObject("Agua_rama_volumen"); vol.transform.SetParent(tube, false);
        CapsuleCollider cc = vol.AddComponent<CapsuleCollider>(); cc.isTrigger = true; cc.direction = 1;
        cc.radius = TreeSpec.BranchWater; cc.height = L + 2f * TreeSpec.BranchWater; cc.center = Vector3.up * (L * 0.5f);
        vol.AddComponent<WaterZone>();
        // Nervio de acero bajo el tubo (el que carga) y el COLLAR que cierra la boca en la pared de la raíz.
        Vector3 under = Vector3.ProjectOnPlane(Vector3.down, dir).normalized;
        KitchenProps.C(tree, S + dir * (TreeSpec.BranchLength * 0.5f) + under * (TreeSpec.BranchRadius + 0.25f), new Vector3(0.35f, 0.45f, TreeSpec.BranchLength),
            TreeSteel, Quaternion.LookRotation(dir, -under), "Nervio_rama");
        Transform collar = new GameObject($"Collar_rama_{b + 1}").transform; collar.SetParent(tree, false);
        collar.localPosition = h * (TreeSpec.WaterRadius + 0.5f) + Vector3.up * TreeSpec.BranchBaseY; collar.localRotation = Quaternion.LookRotation(h);
        float gw = 2f * TreeSpec.OuterRadius * Mathf.Sin(15f * Mathf.Deg2Rad) + 0.4f, gh = 5.2f, tw = 2f * TreeSpec.BranchRadius, th = tw / Mathf.Cos(TreeSpec.BranchAngle * Mathf.Deg2Rad);
        for (int sx = -1; sx <= 1; sx += 2)
        {
            Solid(collar, new Vector3(sx * (tw * 0.5f + (gw - tw) * 0.25f), 0f, 0f), new Vector3((gw - tw) * 0.5f, gh, 1f), TreeSteel, "Collar");
            Solid(collar, new Vector3(0f, sx * (th * 0.5f + (gh - th) * 0.25f), 0f), new Vector3(tw, (gh - th) * 0.5f, 1f), TreeSteel, "Collar");
        }

        // CAMPANA de la punta (cilindro de agua vertical que asoma sobre el anillo) con su puerta hacia fuera.
        float yD = TreeSpec.RingDeckY(b), yb0 = E.y - 1.8f, yb1 = yD + 3f;
        Transform bulb = new GameObject($"Campana_rama_{b + 1}").transform; bulb.SetParent(tree, false);
        bulb.localPosition = new Vector3(E.x, 0f, E.z);
        float outAng = TreeSpec.BranchYaw[b];
        var bg = new List<(float ang, float half, float y0, float y1)> { (outAng, 23f, yD, yD + 2.4f), (outAng + 180f, 45f, yb0, E.y + 2f) };
        Mesh bm = TubeMesh($"Campana_rama_{b + 1}", TreeSpec.BulbInner, TreeSpec.BulbRadius, yb0, yb1, 24, bg);
        bulb.gameObject.AddComponent<MeshFilter>().sharedMesh = bm;
        bulb.gameObject.AddComponent<MeshRenderer>().sharedMaterial = KitchenProps.Mat(VirtualizationTank.GlassColor);
        bulb.gameObject.AddComponent<MeshCollider>().sharedMesh = bm;
        Solid(bulb, Vector3.up * (yb0 - 0.05f), new Vector3(2f * TreeSpec.BulbRadius, 0.1f, 2f * TreeSpec.BulbRadius), VirtualizationTank.GlassColor, "Fondo_campana");
        Solid(bulb, Vector3.up * (yb1 + 0.05f), new Vector3(2f * TreeSpec.BulbRadius, 0.1f, 2f * TreeSpec.BulbRadius), VirtualizationTank.GlassColor, "Tapa_campana");
        GameObject bw = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bw.name = "Agua_campana"; bw.transform.SetParent(bulb, false);
        float bwTop = yD + 2.6f;
        bw.transform.localPosition = Vector3.up * ((yb0 + bwTop) * 0.5f); bw.transform.localScale = new Vector3(2f * TreeSpec.BulbInner, (bwTop - yb0) * 0.5f, 2f * TreeSpec.BulbInner);
        Object.DestroyImmediate(bw.GetComponent<Collider>());
        bw.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(RootWater);
        WaterVolume(bulb, "Agua_campana_volumen", bw);
        Transform bdoor = new GameObject("Puerta_anillo").transform; bdoor.SetParent(bulb, false);
        bdoor.localPosition = h * TreeSpec.BulbInner + Vector3.up * yD; bdoor.localRotation = Quaternion.LookRotation(h);
        float dw = 2f * TreeSpec.BulbInner * Mathf.Sin(23f * Mathf.Deg2Rad);
        for (int sx = -1; sx <= 1; sx += 2) KitchenProps.C(bdoor, new Vector3(sx * (dw * 0.5f + 0.05f), 1.2f, 0.1f), new Vector3(0.1f, 2.4f, 0.25f), TreeCopper, 0f, "Bobina");
        Transform field = KitchenProps.C(bdoor, new Vector3(0f, 1.2f, 0.01f), new Vector3(dw, 2.4f, 0.02f), TreeGlow, 0f, "Campo_magnetico");
        field.gameObject.AddComponent<CampoMagnetico>();

        // ANILLO DE NIDOS (RingDeck, como los del hospital y el restaurante) alrededor de la campana, con su centro relleno.
        Transform ring = RingDeck(tree, $"Nido_anillo_{b + 1}", new Vector2(TreeSpec.RingHub, TreeSpec.RingHub), 0f, TreeSpec.RingWidth, 0.2f);
        ring.localPosition = new Vector3(E.x, yD, E.z); ring.localRotation = Quaternion.Euler(0f, outAng, 0f);
        float inner = TreeSpec.RingHub * 0.5f - TreeSpec.RingWidth * 0.2f, bR = TreeSpec.BulbRadius + 0.05f;   // 2,7 m: el hueco del anillo
        for (int sx = -1; sx <= 1; sx += 2)
        {
            Solid(ring, new Vector3(sx * (bR + inner) * 0.5f, -0.075f, 0f), new Vector3(inner - bR, 0.15f, 2f * inner), TreeDeck, "Losa_centro");
            Solid(ring, new Vector3(0f, -0.075f, sx * (bR + inner) * 0.5f), new Vector3(2f * bR, 0.15f, inner - bR), TreeDeck, "Losa_centro");
        }
        NestRing(ring, inner);
        // Tornapuntas de la campana a las esquinas del anillo (lo que lo sostiene).
        for (int i = 0; i < 4; i++)
        {
            Vector3 corner = ring.localPosition + ring.localRotation * new Vector3((i & 1) == 0 ? -5.5f : 5.5f, -0.3f, (i & 2) == 0 ? -5.5f : 5.5f);
            Vector3 foot = new Vector3(E.x, yb0 + 0.3f, E.z);
            KitchenProps.C(tree, (corner + foot) * 0.5f, new Vector3(0.22f, 0.22f, (corner - foot).magnitude), TreeSteel, Quaternion.LookRotation(corner - foot), "Tornapunta_anillo");
        }

        Vector3 B = new Vector3(E.x, 0f, E.z);
        float ax = TreeSpec.OuterRadius - 2f;   // la boca, dentro del agua de la raíz (a 9 m del eje)
        Vector3 Sin = S + dir * ((ax - TreeSpec.OuterRadius) / Mathf.Cos(TreeSpec.BranchAngle * Mathf.Deg2Rad));
        return new TreeLevel
        {
            name = $"el anillo de nidos {b + 1}", kind = TreeLevel.Kind.Ring, index = b + 1, deck = ring, doorAngle = outAng, field = field,
            outside = B + h * 3.9f + Vector3.up * yD,
            door = B + h * (TreeSpec.BulbRadius + 0.1f) + Vector3.up * yD,
            inside = B + h * 0.8f + Vector3.up * (yD + 0.2f),
            via = new[] { Sin + Vector3.down * 0.6f, E + Vector3.down * 0.6f, B + Vector3.up * (yD - 0.2f) },   // (pies: el pivote va ~1 m más arriba, por el eje del tubo)
        };
    }

    // Frutales enanos en las esquinas, flores para las abejas de las nieves y nidos de aves en el lado de dentro (el de la
    // puerta, hacia fuera, queda libre).
    static void NestRing(Transform ring, float inner)
    {
        float m = inner + TreeSpec.RingWidth * 0.5f;   // la línea central del anillo
        Color trunk = new Color(0.42f, 0.32f, 0.24f);
        Color[] crowns = { new Color(0.45f, 0.62f, 0.38f), new Color(0.52f, 0.66f, 0.34f) };
        Color[] fruit = { new Color(0.92f, 0.45f, 0.2f), new Color(0.85f, 0.2f, 0.25f), new Color(0.95f, 0.8f, 0.25f) };
        int n = 0;
        for (int sx = -1; sx <= 1; sx += 2)
            for (int sz = -1; sz <= 1; sz += 2)
            {
                Transform t = new GameObject("Frutal_enano").transform; t.SetParent(ring, false); t.localPosition = new Vector3(sx * m, 0f, sz * m);
                KitchenProps.C(t, new Vector3(0f, 0.25f, 0f), new Vector3(1.1f, 0.5f, 1.1f), new Color(0.55f, 0.42f, 0.32f), 0f, "Maceta");
                KitchenProps.C(t, new Vector3(0f, 1.1f, 0f), new Vector3(0.18f, 1.2f, 0.18f), trunk, 0f, "Tronco");
                KitchenProps.C(t, new Vector3(0f, 2.1f, 0f), new Vector3(1.6f, 1.1f, 1.6f), crowns[n % 2], 45f, "Copa");
                for (int f = 0; f < 4; f++) KitchenProps.C(t, new Vector3(Mathf.Sin(f * 1.6f) * 0.7f, 1.8f + (f % 2) * 0.4f, Mathf.Cos(f * 1.6f) * 0.7f), Vector3.one * 0.16f, fruit[(n + f) % 3], 0f, "Fruta");
                n++;
            }
        Color[] petals = { new Color(0.95f, 0.85f, 0.3f), new Color(0.75f, 0.55f, 0.9f), new Color(0.98f, 0.98f, 0.98f), new Color(0.95f, 0.5f, 0.6f) };
        for (int sx = -1; sx <= 1; sx += 2)
        {
            Transform p = new GameObject("Flores_abejas").transform; p.SetParent(ring, false); p.localPosition = new Vector3(sx * m, 0f, 0f);
            KitchenProps.C(p, new Vector3(0f, 0.2f, 0f), new Vector3(1f, 0.4f, 3.2f), new Color(0.55f, 0.42f, 0.32f), 0f, "Jardinera");
            for (int i = 0; i < 10; i++) KitchenProps.C(p, new Vector3((i % 2 - 0.5f) * 0.5f, 0.5f + (i % 3) * 0.05f, -1.4f + i * 0.31f), Vector3.one * 0.18f, petals[i % 4], 0f, "Flor");
        }
        for (int i = -1; i <= 1; i++)
        {
            Transform nest = new GameObject("Nido_aves").transform; nest.SetParent(ring, false); nest.localPosition = new Vector3(i * 2.2f, 0f, -m);
            KitchenProps.C(nest, new Vector3(0f, 0.9f, 0f), new Vector3(0.12f, 1.8f, 0.12f), trunk, 0f, "Poste");
            KitchenProps.C(nest, new Vector3(0f, 1.9f, 0f), new Vector3(0.6f, 0.22f, 0.6f), new Color(0.5f, 0.38f, 0.22f), 45f, "Nido");
            KitchenProps.C(nest, new Vector3(0f, 2.02f, 0f), new Vector3(0.36f, 0.08f, 0.36f), new Color(0.3f, 0.22f, 0.14f), 0f, "Hueco");
        }
    }

    // ── LA COPA ──────────────────────────────────────────────────────────────────────────────────────────────

    static Transform Top(Transform tree)
    {
        float H = TreeSpec.RootHeight, X = TreeSpec.TopWidth * 0.5f, c = 4f;
        Transform top = new GameObject("Copa_arbol").transform; top.SetParent(tree, false); top.localPosition = Vector3.up * (H + 0.4f);
        Solid(top, new Vector3(0f, -0.15f, 0f), new Vector3(2f * X, 0.3f, 2f * (X - c)), TreeDeck, "Losa_copa");
        Solid(top, new Vector3(0f, -0.15f, 0f), new Vector3(2f * (X - c), 0.3f, 2f * X), TreeDeck, "Losa_copa");
        for (int sx = -1; sx <= 1; sx += 2)
            for (int sz = -1; sz <= 1; sz += 2)
                Solid(top, new Vector3(sx * (X - c), -0.15f, sz * (X - c)), new Vector3(c * 1.4142f, 0.3f, c * 1.4142f), TreeDeck, "Losa_copa").localRotation = Quaternion.Euler(0f, 45f, 0f);
        string surf = $"{TreeSpec.Building}#copa";
        CarpetPad.BuildZone(top, top.TransformPoint(new Vector3(0f, 0.03f, 2.9f)), top.rotation, CarpetPad.Kind.Edificio, TreeSpec.Building, 9, surf, "Copa (alfombras)", 2);
        CarpetPad.BuildZone(top, top.TransformPoint(new Vector3(0f, 0.03f, -2.9f)), top.rotation, CarpetPad.Kind.Santuario, TreeSpec.Building, 9, surf,
            $"{TreeSpec.Building}: arrozal, bambusal, milpa, chacra, huerto, rotación, bosque comestible y nidos", 2);
        return top;
    }

    /// <summary>POSA el árbol (al final de <see cref="PlaceCarpetAreas"/>, con todo ya medido sin girar): cada piso 45° más que el
    /// de abajo e inclinado ±3°; la cascada de cada canaleta al piso de abajo y las terrazas planas del arrozal.</summary>
    static void PoseTree()
    {
        if (_treeRoot == null || _treePosed) return;
        _treePosed = true;
        Transform tree = _treeRoot;
        foreach (TreeFloor tf in _treeFloors) if (tf != null) tf.transform.localRotation = TreeSpec.PoseOf(tf.index);
        foreach (TreeFloor tf in _treeFloors)
        {
            if (tf == null) continue;
            int k = tf.index;
            float X = TreeSpec.FloorHalf + TreeSpec.FloorEdge, zLow = tf.tilt > 0f ? X - 0.3f : -(X - 0.3f);
            if (k == 1)
            {
                // ARROZAL EN TERRAZAS PLANAS: el bancal, nivelado sobre su terraza (el agua del arroz no corre).
                GardenPlot gp = tf.GetComponentInChildren<GardenPlot>();
                if (gp != null)
                {
                    gp.transform.localPosition = new Vector3(0f, 0.32f, 18f);
                    gp.transform.localRotation = Quaternion.Inverse(Quaternion.Euler(tf.tilt, 0f, 0f)) * Quaternion.Euler(0f, 90f, 0f);
                    Solid(gp.transform, new Vector3(0f, -0.45f, 0f), new Vector3(9.4f, 0.9f, 40f), new Color(0.52f, 0.44f, 0.36f), "Terraza_arrozal");
                }
                continue;   // (el agua que sobra del arrozal vuelve a la raíz)
            }
            // CASCADA: de la canaleta del borde bajo al piso de abajo (riego por gravedad), vertical.
            Vector3 g = new Vector3(0f, TreeSpec.FloorY[k - 1], 0f) + TreeSpec.PoseOf(k) * new Vector3(0f, -0.1f, zLow);
            Vector3 n = TreeSpec.PoseOf(k - 1) * Vector3.up;
            float below = TreeSpec.FloorY[k - 2] - (n.x * g.x + n.z * g.z) / n.y;
            float L = g.y - below;
            if (L < 0.5f) continue;
            Transform cas = KitchenProps.C(tf.transform, Vector3.zero, new Vector3(4f, L, 0.08f), new Color(0.55f, 0.75f, 0.88f, 0.35f), 0f, "Cascada");
            cas.position = tree.TransformPoint(g + Vector3.down * (L * 0.5f));
            cas.rotation = tree.rotation * Quaternion.Euler(0f, TreeSpec.YawOf(k), 0f);
        }
        Debug.Log($"[Árbol] Posado: {_treeFloors.Count} pisos girados 45° uno sobre otro e inclinados ±{TreeSpec.Tilt:0}° (acordeón), cascadas de riego y arrozal en terrazas.");
    }

    // ── MALLAS ───────────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>TUBO de pared gruesa (eje Y local, de <paramref name="y0"/> a <paramref name="y1"/>) en celdas de
    /// 360/<paramref name="segs"/>° con HUECOS (puertas, bocas): una celda falta si su centro cae dentro de un hueco. Caras de
    /// fuera, de dentro y los cantos de los huecos y los extremos: sirve para verse y para chocar (MeshCollider).</summary>
    static Mesh TubeMesh(string name, float rIn, float rOut, float y0, float y1, int segs, List<(float ang, float half, float y0, float y1)> gaps)
    {
        var ys = new List<float> { y0, y1 };
        if (gaps != null) foreach (var g in gaps) { if (g.y0 > y0 && g.y0 < y1) ys.Add(g.y0); if (g.y1 > y0 && g.y1 < y1) ys.Add(g.y1); }
        for (float y = y0 + 8f; y < y1; y += 8f) ys.Add(y);   // (celdas no muy altas)
        ys.Sort(); for (int i = ys.Count - 1; i > 0; i--) if (ys[i] - ys[i - 1] < 0.01f) ys.RemoveAt(i);
        int rows = ys.Count - 1;
        float step = 360f / segs;
        bool[,] solid = new bool[segs, rows];
        for (int i = 0; i < segs; i++)
            for (int j = 0; j < rows; j++)
            {
                float a = (i + 0.5f) * step, ym = (ys[j] + ys[j + 1]) * 0.5f; bool s = true;
                if (gaps != null) foreach (var g in gaps) if (Mathf.Abs(Mathf.DeltaAngle(a, g.ang)) <= g.half && ym > g.y0 && ym < g.y1) { s = false; break; }
                solid[i, j] = s;
            }
        var v = new List<Vector3>(); var t = new List<int>();
        void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d) { int o = v.Count; v.Add(a); v.Add(b); v.Add(c); v.Add(d); t.Add(o); t.Add(o + 2); t.Add(o + 1); t.Add(o); t.Add(o + 3); t.Add(o + 2); }   // (al revés: las caras miran hacia fuera de la pared)
        Vector3 P(float ang, float r, float y) => TreeSpec.Dir(ang) * r + Vector3.up * y;
        for (int i = 0; i < segs; i++)
        {
            float a0 = i * step, a1 = (i + 1) * step;
            for (int j = 0; j < rows; j++)
            {
                if (!solid[i, j]) continue;
                float ya = ys[j], yb = ys[j + 1];
                // (brújula: el ángulo crece en el sentido de las agujas visto desde arriba; Unity es de mano izquierda)
                Quad(P(a0, rOut, ya), P(a0, rOut, yb), P(a1, rOut, yb), P(a1, rOut, ya));   // fuera
                Quad(P(a1, rIn, ya), P(a1, rIn, yb), P(a0, rIn, yb), P(a0, rIn, ya));       // dentro
                if (j == 0 || !solid[i, j - 1]) Quad(P(a0, rIn, ya), P(a0, rOut, ya), P(a1, rOut, ya), P(a1, rIn, ya));          // abajo
                if (j == rows - 1 || !solid[i, j + 1]) Quad(P(a1, rIn, yb), P(a1, rOut, yb), P(a0, rOut, yb), P(a0, rIn, yb));   // arriba
                if (!solid[(i + segs - 1) % segs, j]) Quad(P(a0, rIn, ya), P(a0, rIn, yb), P(a0, rOut, yb), P(a0, rOut, ya));    // canto del hueco
                if (!solid[(i + 1) % segs, j]) Quad(P(a1, rOut, ya), P(a1, rOut, yb), P(a1, rIn, yb), P(a1, rIn, ya));
            }
        }
        return Finish(name, v, t);
    }

    /// <summary>LOSA de un piso: octógono (rectángulo + <c>X</c>, chaflanes <paramref name="c"/>) con un agujero redondo de
    /// <paramref name="hole"/> m para la raíz; cara de arriba a y = 0, de <paramref name="thick"/> m.</summary>
    static Mesh SlabMesh(string name, float X, float c, float hole, float thick)
    {
        var corners = new List<Vector3>   // en el sentido de la brújula, empezando en +Z
        {
            new Vector3(X - c, 0f, X), new Vector3(X, 0f, X - c), new Vector3(X, 0f, -(X - c)), new Vector3(X - c, 0f, -X),
            new Vector3(-(X - c), 0f, -X), new Vector3(-X, 0f, -(X - c)), new Vector3(-X, 0f, X - c), new Vector3(-(X - c), 0f, X),
        };
        var outer = new List<Vector3>();
        for (int e = 0; e < 8; e++) for (int s = 0; s < 8; s++) outer.Add(Vector3.Lerp(corners[e], corners[(e + 1) % 8], s / 8f));
        // (empezar en el ángulo más bajo para que los pares exterior–interior vayan en orden)
        int n = outer.Count;
        var v = new List<Vector3>(); var t = new List<int>();
        void Quad(Vector3 a, Vector3 b, Vector3 cc, Vector3 d) { int o = v.Count; v.Add(a); v.Add(b); v.Add(cc); v.Add(d); t.Add(o); t.Add(o + 1); t.Add(o + 2); t.Add(o); t.Add(o + 2); t.Add(o + 3); }
        Vector3 In(Vector3 p) => new Vector3(p.x, 0f, p.z).normalized * hole;
        Vector3 down = Vector3.down * thick;
        for (int i = 0; i < n; i++)
        {
            Vector3 o0 = outer[i], o1 = outer[(i + 1) % n], i0 = In(o0), i1 = In(o1);
            Quad(i0, o0, o1, i1);                              // arriba
            Quad(i1 + down, o1 + down, o0 + down, i0 + down);  // abajo
            Quad(o0, o0 + down, o1 + down, o1);                // canto de fuera
            Quad(i1, i1 + down, i0 + down, i0);                // canto del hueco
        }
        return Finish(name, v, t);
    }

    /// <summary>Muchas cajas (centro, tamaño, giro) en una sola malla (los nervios y zunchos: miles de piezas, un solo dibujo).</summary>
    static Mesh BoxesMesh(string name, List<(Vector3 c, Vector3 s, Quaternion q)> boxes)
    {
        var v = new List<Vector3>(); var t = new List<int>();
        int[] faces = { 0, 2, 3, 1, 4, 5, 7, 6, 0, 1, 5, 4, 2, 6, 7, 3, 0, 4, 6, 2, 1, 3, 7, 5 };
        foreach (var (c, s, q) in boxes)
        {
            var p = new Vector3[8];
            for (int i = 0; i < 8; i++) p[i] = c + q * Vector3.Scale(s * 0.5f, new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f));
            for (int f = 0; f < 6; f++)
            {
                int o = v.Count;
                for (int k = 0; k < 4; k++) v.Add(p[faces[f * 4 + k]]);
                t.Add(o); t.Add(o + 1); t.Add(o + 2); t.Add(o); t.Add(o + 2); t.Add(o + 3);
            }
        }
        return Finish(name, v, t);
    }

    static Mesh Finish(string name, List<Vector3> v, List<int> t)
    {
        Mesh m = new Mesh { name = name };
        if (v.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m.vertices = v.ToArray(); m.triangles = t.ToArray();
        m.RecalculateNormals(); m.RecalculateBounds();
        return m;
    }
}
