using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL ÁRBOL DE LOS CAMPOS v2 (Berón 2026-10-30; docs/edificio-arbol.md, medidas en <see cref="TreeSpec"/> y el modelo genérico
/// <see cref="HelixTower"/>): los campos de cultivo en un edificio en HÉLICE, en el lóbulo Sur:Milpa.
/// - TRONCO CUADRADO de agua (22 m, pared de acrílico de 1 m, transparente como el tanque de virtualización: se ve nadar a la
///   gente), de 0 a 92 m, con 24 nervios de acero (CFST) y zunchos de canto cada 6 m. Una PUERTA por nivel con su campo
///   magnético (el agua no sale; el hueco es una campana de aire); abajo, la puerta del suelo; arriba, la TRAMPILLA del techo.
/// - HÉLICE de 2 niveles desde 64 m (por encima del árbol más alto del santuario): en cada nivel, 2 PISOS cuadrados de 32 m
///   pegados a dos caras opuestas (en molinete: no se tapan) y, en las otras dos caras, 2 RAMAS (tubos cuadrados de agua) a
///   un CAMPO DE PUNTA cuadrado de 16 m en la esquina libre, con una COLUMNA de agua (puerta al campo y puerta al ANILLO de
///   encima, con nidos, flores y frutales enanos). El nivel siguiente, girado un cuarto de vuelta.
/// - TECHO: plataforma plana de 22 m con las alfombras (de edificio y del santuario) y la trampilla. Abajo, en el hielo, las
///   del suelo. Entre pisos, sin alfombras: por el tronco, nadando. El suelo bajo el árbol queda libre (sin pavimento).
/// Se construye ya en su forma (los pisos son planos y van girados de 90 en 90°: los muebles se miden en el marco del piso).
/// </summary>
public static partial class SampleSceneBuilder
{
    static Transform _treeRoot;
    static bool _treePosed;
    static readonly List<TreeFloor> _treeFloors = new List<TreeFloor>();

    static readonly Color TreeSteel = new Color(0.36f, 0.39f, 0.44f), TreeCopper = new Color(0.72f, 0.45f, 0.22f),
        TreeGlow = new Color(0.55f, 0.85f, 1f, 0.35f), TreeDeck = new Color(0.48f, 0.4f, 0.33f), TreeSlab = new Color(0.66f, 0.6f, 0.52f);

    /// <summary>El agua del tronco: el mismo tono que el del tanque, más clara (20 m de agua: si no, no se vería nadar a nadie).</summary>
    static Color RootWater => new Color(VirtualizationTank.WaterColor.r, VirtualizationTank.WaterColor.g, VirtualizationTank.WaterColor.b, 0.2f);

    /// <summary>Levanta el árbol con el área del Huerto (<paramref name="garden"/>) en su piso 5. Devuelve la raíz.</summary>
    static Transform BuildTree(Transform parent, SanctuaryArea garden)
    {
        HelixTower h = TreeSpec.Helix;
        _treeFloors.Clear(); _treePosed = false;
        Transform tree = new GameObject(TreeSpec.RootName).transform;
        tree.SetParent(parent, false); tree.position = TreeSpec.Site;
        ArbolCampos ac = tree.gameObject.AddComponent<ArbolCampos>();
        tree.gameObject.AddComponent<TreeRotator>();
        Rigidbody rb = tree.gameObject.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false;   // gira: sus colliders, de un cuerpo cinemático
        _treeRoot = tree;

        // ── HUECOS DE CADA CARA (marco del lado: x a lo largo de la cara, y altura): las puertas y las bocas de las ramas ──
        var gaps = new List<Rect>[4];
        for (int s = 0; s < 4; s++) gaps[s] = new List<Rect>();
        gaps[TreeSpec.GroundSide].Add(DoorRect(h, 0f));
        for (int lv = 1; lv <= h.levels; lv++)
            for (int s = 0; s < 4; s++)
                gaps[s].Add(HelixTower.HasFloor(lv, s) ? DoorRect(h, h.LevelY(lv)) : new Rect(h.mouthX - h.tubeHalf, h.LevelY(lv) - 0.3f - h.tubeWall, 2f * h.tubeHalf, h.TubeInnerH + 2f * h.tubeWall));
        Trunk(tree, ac, h, gaps);

        // ── NIVELES: el suelo, las 8 plataformas (pisos y campos de punta, con sus anillos) y el techo ──
        ac.levels.Clear();
        TreeLevel ground = WallLevel(h, TreeLevel.Kind.Ground, 0, TreeSpec.GroundSide, 0f, "el suelo", null, 0f);
        ground.field = DoorFrame(tree, HelixTower.FromSide(TreeSpec.GroundSide, new Vector3(h.doorX, 0f, h.waterHalf)), HelixTower.SideRot(TreeSpec.GroundSide), h.wall, "Puerta_suelo", ground.outside);
        ac.levels.Add(ground);
        int ring = 0;
        for (int k = 1; k <= TreeSpec.Platforms; k++)
        {
            var (lv, side, tip) = TreeSpec.Platform(k);
            if (tip) { var (fl, rl) = Branch(tree, h, k, ++ring); ac.levels.Add(fl); ac.levels.Add(rl); continue; }
            Transform f = TrunkFloor(tree, h, k, garden);
            TreeLevel L = WallLevel(h, TreeLevel.Kind.Floor, k, side, h.LevelY(lv), $"el piso {k} ({GardenPlot.Title(TreeSpec.Guilds[k - 1])})", f, h.floorSize * 0.5f);
            L.level = lv;
            L.field = DoorFrame(tree, HelixTower.FromSide(side, new Vector3(h.doorX, h.LevelY(lv), h.waterHalf)), HelixTower.SideRot(side), h.wall, $"Puerta_piso_{k}", L.outside);
            ac.levels.Add(L);
        }
        ac.top = Top(tree, out Transform hatchField, out var roof);
        ac.levels.Add(new TreeLevel
        {
            name = "el techo", kind = TreeLevel.Kind.Top, index = 9, deck = ac.top, half = TreeSpec.TopWidth * 0.5f, field = hatchField, doorAngle = 0f,
            outside = roof.outside, door = roof.door, inside = roof.inside,
        });

        // Alfombras del SUELO, en el hielo (no giran): al techo. (Las del techo giran con el árbol.)
        Vector3 padAt = TreeSpec.Site + TreeSpec.Dir(45f) * 24f + Vector3.up * 0.01f;
        // (Su propio padre, sin colliders: si colgara de «Areas_Placed», CarpetPad.OnDeck la tomaría por la pista de CUALQUIER piso
        // sin pista propia y mandaría a la gente al pie del árbol.)
        Transform padRoot = new GameObject("Arbol_alfombras_suelo").transform; padRoot.SetParent(parent, false);
        CarpetPad.BuildZone(padRoot, padAt, Quaternion.LookRotation(TreeSpec.Site - new Vector3(padAt.x, 0f, padAt.z)), CarpetPad.Kind.Edificio, TreeSpec.Building, 0, $"{TreeSpec.Building}#0", "Suelo (hielo)", 2);
        Vector3 padS = TreeSpec.Site + TreeSpec.Dir(69f) * 24f + Vector3.up * 0.01f;
        CarpetPad.BuildZone(padRoot, padS, Quaternion.LookRotation(TreeSpec.Site - new Vector3(padS.x, 0f, padS.z)), CarpetPad.Kind.Santuario, TreeSpec.Building, 0, $"{TreeSpec.Building}#0",
            $"{TreeSpec.Building} (al pie: a los pisos, nadando por el tronco)", 2);

        // EL TRONCO AGUANTA (StructuralLimits.RootCheck): 4 pisos, 4 puntas con sus ramas, el techo, su pared y el agua.
        StructuralLimits.RootReport rep = StructuralLimits.RootCheck(h, 4, 4, TreeSpec.RootHeight, TreeSpec.Ribs, TreeSpec.HoopSpacing, TreeSpec.HoopArea, TreeSpec.StrutDrop);
        if (rep.Fail) Debug.LogError($"[Estructura] El tronco del Árbol de los Campos no aguanta: {rep}. Refuérzalo (más nervios, más pared o más tornapuntas).");
        else if (rep.Warn) Debug.LogWarning($"[Estructura] El tronco del Árbol de los Campos va justo: {rep}.");
        else Debug.Log($"[Estructura] Tronco del Árbol de los Campos ({2f * h.OuterHalf:0} × {2f * h.OuterHalf:0} m, {TreeSpec.Ribs} nervios): {rep}.");
        Debug.Log($"[Árbol] «{TreeSpec.Building}» en ({TreeSpec.Site.x:0}, {TreeSpec.Site.z:0}): tronco cuadrado de {TreeSpec.RootHeight:0} m, hélice de {h.levels} niveles desde {h.firstY:0} m " +
                  $"(el árbol más alto del santuario, {TreeSpec.TallestTree:0.#} m), {TreeSpec.Platforms} plataformas ({string.Join(" · ", System.Array.ConvertAll(TreeSpec.Guilds, g => GardenPlot.Title(g)))}), alcance {h.Reach:0.0} m.");
        return tree;
    }

    static Rect DoorRect(HelixTower h, float y) => new Rect(h.doorX - h.doorWidth * 0.5f, y, h.doorWidth, h.doorHeight);

    /// <summary>Un nivel con su puerta en una cara del tronco (el suelo y los pisos pegados).</summary>
    static TreeLevel WallLevel(HelixTower h, TreeLevel.Kind kind, int k, int side, float y, string name, Transform deck, float half) => new TreeLevel
    {
        name = name, kind = kind, index = k, side = side, deck = deck, half = half, doorAngle = side * 90f,
        outside = HelixTower.FromSide(side, new Vector3(h.doorX, y, h.OuterHalf + 2.5f)),
        door = HelixTower.FromSide(side, new Vector3(h.doorX, y, h.waterHalf + h.wall * 0.5f)),
        inside = HelixTower.FromSide(side, new Vector3(h.doorX, y + 0.2f, h.waterHalf - 1.5f)),
    };

    // ── EL TRONCO ─────────────────────────────────────────────────────────────────────────────────────────────

    static void Trunk(Transform tree, ArbolCampos ac, HelixTower h, List<Rect>[] gaps)
    {
        float a = h.waterHalf, o = h.OuterHalf, H = TreeSpec.RootHeight;
        // PARED de acrílico transparente (el cristal del tanque de virtualización), con colliders: se entra solo por las puertas.
        var v = new List<Vector3>(); var t = new List<int>();
        for (int s = 0; s < 4; s++) Panel(v, t, HelixTower.SideRot(s), Vector3.zero, -a, o, 0f, H, a, o, gaps[s]);
        Mesh wm = TreeMesh("Pared_raiz", v, t);
        GameObject wall = new GameObject("Pared_raiz");
        wall.transform.SetParent(tree, false);
        wall.AddComponent<MeshFilter>().sharedMesh = wm;
        wall.AddComponent<MeshRenderer>().sharedMaterial = KitchenProps.Mat(VirtualizationTank.GlassColor);
        wall.AddComponent<MeshCollider>().sharedMesh = wm;
        ac.wall = wall.transform;

        // AGUA (se ve) y su VOLUMEN nadable (WaterZone: la superficie es lo más alto del volumen).
        float wy0 = TreeSpec.WaterBottom, wy1 = TreeSpec.WaterTop;
        ac.water = WaterBox(tree, "Agua_raiz", new Vector3(0f, (wy0 + wy1) * 0.5f, 0f), new Vector3(2f * a, wy1 - wy0, 2f * a));
        KitchenProps.C(tree, Vector3.up * 0.02f, new Vector3(2f * a, 0.04f, 2f * a), new Color(0.3f, 0.42f, 0.48f), 0f, "Fondo_raiz");
        KitchenProps.C(tree, Vector3.down * 0.4f, new Vector3(2f * o + 3f, 0.8f, 2f * o + 3f), new Color(0.72f, 0.71f, 0.68f), 0f, "Encepado_raiz");   // el cimiento (bajo el hielo)

        // EXOESQUELETO: 24 nervios de acero (CFST: 4 esquinas y 5 por cara, uno cada ~3,7 m) y zunchos de canto cada 6 m (los que
        // sujetan el empuje del agua con el acrílico; StructuralLimits.RootCheck), una sola malla; no cruzan las puertas, las
        // bocas de las ramas ni las losas de los pisos.
        var boxes = new List<(Vector3 c, Vector3 s, Quaternion q)>();
        for (int s = 0; s < 4; s++)
        {
            Quaternion q = HelixTower.SideRot(s);
            bool Blocked(float x0, float x1, float y0, float y1, float pad)
            {
                foreach (Rect g in gaps[s]) if (x1 > g.xMin - pad && x0 < g.xMax + pad && y1 > g.yMin - 0.3f && y0 < g.yMax + 0.3f) return true;
                for (int lv = 1; lv <= h.levels; lv++)   // la losa del piso pegado a esta cara (x de −h a −h+S)
                    if (HelixTower.HasFloor(lv, s) && y1 > h.LevelY(lv) - 1.2f && y0 < h.LevelY(lv) + 0.4f) return true;
                return false;
            }
            var ribX = new List<float>();
            for (int i = 1; i <= 5; i++) ribX.Add(-o + i * (2f * o / 6f));
            foreach (float x in ribX)
                for (float y = 0f; y < H; y += 4f)
                {
                    float y1 = Mathf.Min(H, y + 4f);
                    if (Blocked(x - 0.3f, x + 0.3f, y, y1, 0.3f)) continue;
                    boxes.Add((q * new Vector3(x, (y + y1) * 0.5f, o + 0.3f), new Vector3(0.55f, y1 - y, 0.6f), q));
                }
            for (float y = 0f; y < H; y += 4f)   // el nervio de la esquina (+x de esta cara)
            {
                float y1 = Mathf.Min(H, y + 4f);
                bool slab = false;
                for (int lv = 1; lv <= h.levels; lv++) if (y1 > h.LevelY(lv) - 1.2f && y < h.LevelY(lv) + 0.4f) slab = true;
                if (slab) continue;
                boxes.Add((q * new Vector3(o + 0.3f, (y + y1) * 0.5f, o + 0.3f), new Vector3(0.6f, y1 - y, 0.6f), q));
            }
            var hoopY = new List<float>();
            for (float y = 3f; y < H - 0.5f; y += TreeSpec.HoopSpacing) hoopY.Add(y);
            hoopY.Add(H - 0.3f);
            var xs = new List<float> { -o }; xs.AddRange(ribX); xs.Add(o);
            foreach (float y in hoopY)
                for (int i = 0; i + 1 < xs.Count; i++)
                {
                    float x0 = xs[i] + 0.28f, x1 = xs[i + 1] - 0.28f;
                    if (Blocked(x0, x1, y - 0.2f, y + 0.2f, 0.2f)) continue;
                    boxes.Add((q * new Vector3((x0 + x1) * 0.5f, y, o + TreeSpec.HoopDepth * 0.5f), new Vector3(x1 - x0, TreeSpec.HoopHeight, TreeSpec.HoopDepth), q));
                }
        }
        GameObject exo = new GameObject("Exoesqueleto");
        exo.transform.SetParent(tree, false);
        exo.AddComponent<MeshFilter>().sharedMesh = BoxesMesh("Exoesqueleto", boxes);
        exo.AddComponent<MeshRenderer>().sharedMaterial = KitchenProps.Mat(TreeSteel);
        Label(KitchenProps.C(tree, HelixTower.FromSide(TreeSpec.GroundSide, new Vector3(h.doorX - 2.4f, 3.4f, o + 0.8f)), new Vector3(0.1f, 0.1f, 0.1f), TreeSteel, 0f, "Cartel_raiz"),
              "Tronco del Árbol de los Campos: entra por la puerta (o por la trampilla del techo) y nada a tu piso", 0.4f);
    }

    /// <summary>Agua que se ve (caja sin collider) y su volumen nadable (BoxCollider trigger con <see cref="WaterZone"/>).</summary>
    static Transform WaterBox(Transform parent, string name, Vector3 center, Vector3 size)
    {
        GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cube);
        w.name = name; w.transform.SetParent(parent, false);
        w.transform.localPosition = center; w.transform.localScale = size;
        Object.DestroyImmediate(w.GetComponent<Collider>());
        w.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(RootWater);
        GameObject vol = new GameObject(name + "_volumen");
        vol.transform.SetParent(parent, false);
        vol.transform.localPosition = center;
        BoxCollider bc = vol.AddComponent<BoxCollider>(); bc.isTrigger = true; bc.size = size;
        vol.AddComponent<WaterZone>();
        return w.transform;
    }

    /// <summary>Marco de una puerta con campo magnético (bobinas de cobre a los lados y arriba, como el tanque) y su campo en la
    /// cara de dentro: lo cruzan los cuerpos, no el agua. <paramref name="local"/> = el umbral en la cara de DENTRO de la pared
    /// (local de <paramref name="parent"/>), <paramref name="rot"/> mira HACIA FUERA. Lleva su <see cref="TreeDoorExit"/> (quien
    /// nada hacia ella sale de pie en <paramref name="landing"/>, local de <paramref name="parent"/>). Devuelve el campo.</summary>
    static Transform DoorFrame(Transform parent, Vector3 local, Quaternion rot, float wallThick, string name, Vector3 landing)
    {
        Transform d = new GameObject(name).transform; d.SetParent(parent, false);
        d.localPosition = local; d.localRotation = rot;
        float w = TreeSpec.DoorWidth, h = TreeSpec.DoorHeight;
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(d, new Vector3(s * (w * 0.5f + 0.06f), h * 0.5f, wallThick * 0.5f), new Vector3(0.12f, h, wallThick), TreeCopper, 0f, "Bobina");
        KitchenProps.C(d, new Vector3(0f, h + 0.06f, wallThick * 0.5f), new Vector3(w + 0.24f, 0.12f, wallThick), TreeCopper, 0f, "Bobina");
        Transform field = KitchenProps.C(d, new Vector3(0f, h * 0.5f, 0.02f), new Vector3(w, h, 0.02f), TreeGlow, 0f, "Campo_magnetico");
        field.gameObject.AddComponent<CampoMagnetico>();
        Transform land = new GameObject("Salida").transform; land.SetParent(d, false);
        land.position = parent.TransformPoint(landing);
        TreeDoorExit ex = d.gameObject.AddComponent<TreeDoorExit>();
        ex.landing = land; ex.zoneCenter = new Vector3(0f, h * 0.5f, -1.1f); ex.zoneSize = new Vector3(w + 0.8f, h + 2.4f, 3.6f);
        return field;
    }

    // ── LOS PISOS PEGADOS AL TRONCO ───────────────────────────────────────────────────────────────────────────

    static Transform TrunkFloor(Transform tree, HelixTower h, int k, SanctuaryArea garden)
    {
        var (lv, side, _) = TreeSpec.Platform(k);
        GardenPlot.Guild guild = TreeSpec.Guilds[k - 1];
        float S = h.floorSize, half = S * 0.5f, fx = h.FloorCenterS.x;
        Transform f = new GameObject($"Piso_arbol_{k}_{guild}").transform;
        f.SetParent(tree, false);
        f.localPosition = HelixTower.FromSide(side, h.FloorCenterS) + Vector3.up * h.LevelY(lv);
        f.localRotation = HelixTower.SideRot(side);
        TreeFloor tf = f.gameObject.AddComponent<TreeFloor>();
        tf.index = k; tf.tilt = 0f; tf.yaw = side * 90f;
        // LOSA cuadrada, en voladizo desde su cara del tronco: ménsulas y tornapuntas que bajan a la cara.
        Transform s = new GameObject("Estructura_Elevada").transform; s.SetParent(f, false);
        Solid(s, new Vector3(0f, -0.15f, 0f), new Vector3(S, 0.3f, S), TreeSlab, "Losa_arbol");
        foreach (float xs in new[] { -9f, -3f, 3f, 9f })
            KitchenProps.C(s, new Vector3(xs - fx, -0.6f, -half + 5f), new Vector3(0.35f, 0.6f, 10f), TreeSteel, 0f, "Mensula");
        foreach (var (x0, x1) in new[] { (-9f, -8f), (-3f, 2f), (3f, 10f), (9f, 18f) })
        {
            Vector3 a = new Vector3(x0 - fx, -TreeSpec.StrutDrop, -half + 0.3f), b = new Vector3(x1 - fx, -0.35f, -half + 0.625f * S);
            KitchenProps.C(s, (a + b) * 0.5f, new Vector3(0.45f, 0.45f, (b - a).magnitude), TreeSteel, Quaternion.LookRotation(b - a), "Tornapunta");
        }
        _platforms[f] = new Vector2(S, S);   // sus plazas reservadas (regla 8)

        // EL CULTIVO del piso: lejos de la puerta (el borde −Z, pegado al tronco) y de la rama que pasa por encima de esa franja.
        if (guild == GardenPlot.Guild.Hortalizas && garden != null)
        {
            garden.transform.SetParent(f, false);
            garden.transform.localPosition = new Vector3(0f, 0f, -6.5f);
            garden.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);   // su lado abierto (+Z del área) mira a la puerta
            GameObject go = MakePlot(garden.transform, guild, garden.transform.TransformPoint(new Vector3(-4f, 0.1f, 0f)), new Vector2(6f, 22f), TreeSpec.Seeds[k - 1], false);
            go.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);   // (sus filas, a lo largo del área: caben todas las del gremio)
        }
        else
        {
            GameObject go = MakePlot(f, guild, f.TransformPoint(new Vector3(1f, 0.06f, 4f)), new Vector2(22f, 18f), TreeSpec.Seeds[k - 1], false);
            go.transform.localRotation = Quaternion.identity;
            if (k == 6) go.GetComponent<GardenPlot>().speciesOffset = 5;   // el segundo bosque comestible: los otros árboles del dosel
            KitchenProps.C(go.transform, new Vector3(0f, -0.03f, 0f), new Vector3(24f, 0.06f, 20f), new Color(0.70f, 0.64f, 0.58f), 0f, "Suelo_Huerto");
        }
        _treeFloors.Add(tf);
        return f;
    }

    // ── LAS RAMAS, LOS CAMPOS DE PUNTA Y SUS ANILLOS ─────────────────────────────────────────────────────────

    /// <summary>La rama del piso k (un campo de punta): tubo cuadrado de agua desde la cara (recto hacia fuera y luego a la
    /// derecha), la COLUMNA de agua en la esquina del campo (puertas al campo y al anillo), el campo y su anillo.</summary>
    static (TreeLevel field, TreeLevel ring) Branch(Transform tree, HelixTower h, int k, int ringN)
    {
        var (lv, side, _) = TreeSpec.Platform(k);
        GardenPlot.Guild guild = TreeSpec.Guilds[k - 1];
        float Y = h.LevelY(lv), tipY = h.TipY(lv), ringY = h.RingY(lv);
        Quaternion q = HelixTower.SideRot(side);
        float mx = h.mouthX, th = h.tubeHalf, tw = h.tubeWall, ti = th - tw, a = h.waterHalf, o = h.OuterHalf;
        Vector3 col = h.ColumnS; float cx = col.x, cz = col.z, ci = h.colHalf, co = h.ColOuter;
        float y0 = Y - 0.3f, y1 = y0 + h.TubeInnerH, ym = (y0 + y1) * 0.5f, hh = y1 - y0;   // el hueco del tubo (5 m)

        // ── RAMA (marco del lado) ──
        Transform br = new GameObject($"Rama_{k}").transform; br.SetParent(tree, false); br.localRotation = q;
        void Glass(Vector3 c, Vector3 sz, string n) => Solid(br, c, sz, VirtualizationTank.GlassColor, n);
        float zA0 = o, zA1 = cz + th, zR1 = cz - ti, xB0 = mx + ti, xB1 = cx - co;
        Glass(new Vector3(mx, y0 - tw * 0.5f, (zA0 + zA1) * 0.5f), new Vector3(2f * th, tw, zA1 - zA0), "Pared_rama");
        Glass(new Vector3(mx, y1 + tw * 0.5f, (zA0 + zA1) * 0.5f), new Vector3(2f * th, tw, zA1 - zA0), "Pared_rama");
        Glass(new Vector3(mx - th + tw * 0.5f, ym, (zA0 + zA1) * 0.5f), new Vector3(tw, hh + 2f * tw, zA1 - zA0), "Pared_rama");
        Glass(new Vector3(mx + th - tw * 0.5f, ym, (zA0 + zR1) * 0.5f), new Vector3(tw, hh + 2f * tw, zR1 - zA0), "Pared_rama");
        Glass(new Vector3(mx, ym, zA1 - tw * 0.5f), new Vector3(2f * th, hh + 2f * tw, tw), "Pared_rama");
        Glass(new Vector3((xB0 + xB1) * 0.5f, y0 - tw * 0.5f, cz), new Vector3(xB1 - xB0, tw, 2f * th), "Pared_rama");
        Glass(new Vector3((xB0 + xB1) * 0.5f, y1 + tw * 0.5f, cz), new Vector3(xB1 - xB0, tw, 2f * th), "Pared_rama");
        Glass(new Vector3((xB0 + xB1) * 0.5f, ym, cz - th + tw * 0.5f), new Vector3(xB1 - xB0, hh + 2f * tw, tw), "Pared_rama");
        Glass(new Vector3((mx + th + xB1) * 0.5f, ym, cz + th - tw * 0.5f), new Vector3(xB1 - mx - th, hh + 2f * tw, tw), "Pared_rama");
        WaterBox(br, "Agua_rama", new Vector3(mx, ym, (a - 0.5f + zA1 - tw) * 0.5f), new Vector3(2f * ti, hh, zA1 - tw - (a - 0.5f)));
        WaterBox(br, "Agua_rama", new Vector3((mx - ti + cx - ci + 0.3f) * 0.5f, ym, cz), new Vector3(cx - ci + 0.3f - (mx - ti), hh, 2f * ti));
        // CELOSÍA de acero de la rama (lo que carga la punta): 2 cordones abajo, 2 arriba (6,5 m de canto) y diagonales.
        float yb = y0 - tw - 0.45f, yt = y1 + tw + 0.3f;
        void Chord(Vector3 p0, Vector3 p1, float size, string n) => KitchenProps.C(br, (p0 + p1) * 0.5f, new Vector3(size, size, (p1 - p0).magnitude), TreeSteel, Quaternion.LookRotation(p1 - p0), n);
        for (int sgn = -1; sgn <= 1; sgn += 2)
        {
            Chord(new Vector3(mx + sgn * (th - 0.2f), yb, o + 0.2f), new Vector3(mx + sgn * (th - 0.2f), yb, cz), 0.9f, "Nervio_rama");
            Chord(new Vector3(mx, yb, cz + sgn * (th - 0.2f)), new Vector3(xB1, yb, cz + sgn * (th - 0.2f)), 0.9f, "Nervio_rama");
            Chord(new Vector3(mx, yt, cz + sgn * (th - 0.2f)), new Vector3(xB1, yt, cz + sgn * (th - 0.2f)), 0.8f, "Nervio_rama");
            Chord(new Vector3(mx + sgn * (th - 0.2f), yt, o + 0.2f), new Vector3(mx + sgn * (th - 0.2f), yt, cz), 0.8f, "Nervio_rama");
            for (float x = mx + 1f; x < xB1 - 1f; x += 4f)
                Chord(new Vector3(x, yb, cz + sgn * th), new Vector3(Mathf.Min(xB1, x + 4f), yt, cz + sgn * th), 0.25f, "Tornapunta_rama");
        }

        // ── COLUMNA DE LA PUNTA (marco del lado): del fondo de la rama al anillo; puertas al campo (+X) y al anillo (−X) ──
        Transform cg = new GameObject($"Campana_rama_{k}").transform; cg.SetParent(tree, false); cg.localRotation = q;
        float cb = y0 - tw, wt = ringY + h.doorHeight + 0.6f, ctop = wt + 0.4f;
        var cv = new List<Vector3>(); var ct = new List<int>();
        Vector3 cc = new Vector3(cx, 0f, cz);
        for (int f = 0; f < 4; f++)
        {
            var g = new List<Rect>();
            if (f == 3) { g.Add(new Rect(-ti, y0, 2f * ti, hh)); g.Add(new Rect(-h.doorWidth * 0.5f, ringY, h.doorWidth, h.doorHeight)); }   // −X: la rama y el anillo
            if (f == 1) g.Add(new Rect(-h.doorWidth * 0.5f, tipY, h.doorWidth, h.doorHeight));                                                  // +X: el campo
            Panel(cv, ct, HelixTower.SideRot(f), cc, -ci, co, cb, ctop, ci, co, g);
        }
        Mesh cm = TreeMesh($"Campana_rama_{k}", cv, ct);
        cg.gameObject.AddComponent<MeshFilter>().sharedMesh = cm;
        cg.gameObject.AddComponent<MeshRenderer>().sharedMaterial = KitchenProps.Mat(VirtualizationTank.GlassColor);
        cg.gameObject.AddComponent<MeshCollider>().sharedMesh = cm;
        Solid(cg, new Vector3(cx, cb + tw * 0.5f, cz), new Vector3(2f * co, tw, 2f * co), VirtualizationTank.GlassColor, "Fondo_campana");
        Solid(cg, new Vector3(cx, ctop - tw * 0.5f, cz), new Vector3(2f * co, tw, 2f * co), VirtualizationTank.GlassColor, "Tapa_campana");
        WaterBox(cg, "Agua_campana", new Vector3(cx, (y0 + wt) * 0.5f, cz), new Vector3(2f * ci, wt - y0, 2f * ci));
        Vector3 fieldOut = new Vector3(cx + co + 2f, tipY, cz), ringOut = new Vector3(cx - co - 2.2f, ringY, cz);
        Transform fField = DoorFrame(cg, new Vector3(cx + ci, tipY, cz), Quaternion.Euler(0f, 90f, 0f), tw, "Puerta_campo", fieldOut);
        Transform rField = DoorFrame(cg, new Vector3(cx - ci, ringY, cz), Quaternion.Euler(0f, 270f, 0f), tw, "Puerta_anillo", ringOut);

        // ── CAMPO DE PUNTA (cuadrado, con el hueco de la columna en su esquina de dentro) ──
        float T = h.tipSize, E = T * 0.5f;
        Vector3 tc = h.TipCenterS;
        Transform tip = new GameObject($"Campo_punta_{k}_{guild}").transform;
        tip.SetParent(tree, false); tip.localPosition = HelixTower.FromSide(side, tc) + Vector3.up * tipY; tip.localRotation = q;
        TreeFloor tf = tip.gameObject.AddComponent<TreeFloor>(); tf.index = k; tf.tilt = 0f; tf.yaw = side * 90f;
        float hx0 = cx - co - tc.x, hx1 = cx + co - tc.x, hz0 = cz - co - tc.z, hz1 = cz + co - tc.z;
        Transform ts = new GameObject("Estructura_Elevada").transform; ts.SetParent(tip, false);
        void Slab(float x0, float x1, float z0, float z1) => Solid(ts, new Vector3((x0 + x1) * 0.5f, -0.15f, (z0 + z1) * 0.5f), new Vector3(x1 - x0, 0.3f, z1 - z0), TreeSlab, "Losa_punta");
        Slab(hx1, E, -E, E); Slab(-E, hx1, hz1, E); Slab(-E, hx0, -E, hz1); Slab(hx0, hx1, -E, hz0);
        Vector3 colL = new Vector3(cx - tc.x, cb - tipY + 0.4f, cz - tc.z);
        foreach (Vector3 corner in new[] { new Vector3(E - 0.4f, -0.35f, E - 0.4f), new Vector3(E - 0.4f, -0.35f, -E + 0.4f), new Vector3(-E + 0.4f, -0.35f, E - 0.4f) })
            KitchenProps.C(ts, (corner + colL) * 0.5f, new Vector3(0.3f, 0.3f, (corner - colL).magnitude), TreeSteel, Quaternion.LookRotation(corner - colL), "Tornapunta_campo");
        _platforms[tip] = new Vector2(T, T);
        // Bancal de 8,6 × 15 m a lo ancho del campo (lejos de la columna y de su puerta), con las filas a lo largo de los 15 m:
        // caben todas las del gremio (la chacra, 8; la rotación, con el garbanzo, el sésamo y el maní, 6).
        GameObject plot = MakePlot(tip, guild, tip.TransformPoint(new Vector3(0f, 0.06f, 3.3f)), new Vector2(8.6f, 15f), TreeSpec.Seeds[k - 1], false);
        plot.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        KitchenProps.C(plot.transform, new Vector3(0f, -0.03f, 0f), new Vector3(9.2f, 0.06f, 15.6f),
            guild == GardenPlot.Guild.Arrozal ? new Color(0.62f, 0.78f, 0.84f) : new Color(0.70f, 0.64f, 0.58f), 0f, "Suelo_Huerto");
        _treeFloors.Add(tf);

        // ── ANILLO encima del campo (RingDeck, a la medida: 20 % sobre el campo y el resto volando), con sus nidos ──
        Transform rg = RingDeck(tip, $"Nido_anillo_{ringN}", new Vector2(T, T), h.ringH, h.ringWidth, h.ringInside);
        NestRing(rg, h.RingInnerHalf, h.ringWidth);

        // ── NIVELES (puntos locales del árbol, a ras de los pies) ──
        Vector3 S(Vector3 p) => HelixTower.FromSide(side, p);
        Vector3 mouthIn = new Vector3(mx, Y + 0.2f, a - 1.5f), elbow = new Vector3(mx, Y + 0.2f, cz), colFoot = new Vector3(cx - 0.3f, Y + 0.2f, cz);
        string title = GardenPlot.Title(guild);
        var fieldLv = new TreeLevel
        {
            name = $"el piso {k} ({title}, en la punta de su rama)", kind = TreeLevel.Kind.Floor, index = k, tip = true, side = side, level = lv,
            deck = tip, half = E, doorAngle = Mathf.Repeat(side * 90f + 90f, 360f), field = fField,
            outside = S(fieldOut), door = S(new Vector3(cx + ci + tw * 0.5f, tipY, cz)), inside = S(new Vector3(cx + 0.6f, tipY + 0.2f, cz)),
            via = new[] { S(mouthIn), S(elbow), S(colFoot) },
        };
        var ringLv = new TreeLevel
        {
            name = $"el anillo {ringN} (encima del piso {k})", kind = TreeLevel.Kind.Ring, index = ringN, side = side, level = lv,
            deck = rg, half = h.RingOuterHalf, doorAngle = Mathf.Repeat(side * 90f + 270f, 360f), field = rField,
            outside = S(ringOut), door = S(new Vector3(cx - ci - tw * 0.5f, ringY, cz)), inside = S(new Vector3(cx - 0.6f, ringY + 0.2f, cz)),
            via = new[] { S(mouthIn), S(elbow), S(colFoot), S(new Vector3(cx, tipY + 0.2f, cz)) },
        };
        return (fieldLv, ringLv);
    }

    // Frutales enanos en las esquinas, flores para las abejas de las nieves y nidos de aves (el anillo, a la medida del campo).
    static void NestRing(Transform ring, float inner, float width)
    {
        float m = inner + width * 0.5f;   // la línea central del anillo
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
            Transform p = new GameObject("Flores_abejas").transform; p.SetParent(ring, false); p.localPosition = new Vector3(sx * m, 0f, 2f);
            KitchenProps.C(p, new Vector3(0f, 0.2f, 0f), new Vector3(1f, 0.4f, 3.2f), new Color(0.55f, 0.42f, 0.32f), 0f, "Jardinera");
            for (int i = 0; i < 10; i++) KitchenProps.C(p, new Vector3((i % 2 - 0.5f) * 0.5f, 0.5f + (i % 3) * 0.05f, -1.4f + i * 0.31f), Vector3.one * 0.18f, petals[i % 4], 0f, "Flor");
        }
        for (int i = -1; i <= 1; i++)
        {
            Transform nest = new GameObject("Nido_aves").transform; nest.SetParent(ring, false); nest.localPosition = new Vector3(i * 2.2f, 0f, m);
            KitchenProps.C(nest, new Vector3(0f, 0.9f, 0f), new Vector3(0.12f, 1.8f, 0.12f), trunk, 0f, "Poste");
            KitchenProps.C(nest, new Vector3(0f, 1.9f, 0f), new Vector3(0.6f, 0.22f, 0.6f), new Color(0.5f, 0.38f, 0.22f), 45f, "Nido");
            KitchenProps.C(nest, new Vector3(0f, 2.02f, 0f), new Vector3(0.36f, 0.08f, 0.36f), new Color(0.3f, 0.22f, 0.14f), 0f, "Hueco");
        }
    }

    // ── EL TECHO ─────────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>El techo: plataforma cuadrada de 22 m (tan ancha como el tronco) con sus alfombras y la PUERTA DEL TECHO (Berón
    /// 2026-10-31: como la de los anillos, se entra de frente): una columna de agua de 5 × 5 m que asoma sobre la cubierta, sobre
    /// el agua del tronco, con su puerta de 3 × 3 m en la cara +Z (hacia las alfombras). Devuelve el techo; <paramref name="hatchField"/>
    /// = el campo magnético de esa puerta; <paramref name="doorLevel"/> = sus puntos (fuera, puerta, dentro; locales del árbol).</summary>
    static Transform Top(Transform tree, out Transform hatchField, out (Vector3 outside, Vector3 door, Vector3 inside) doorLevel)
    {
        HelixTower h = TreeSpec.Helix;
        float H = TreeSpec.RootHeight, X = TreeSpec.TopWidth * 0.5f, ci = TreeSpec.HatchHalf, cw = h.colWall, co = ci + cw;
        Vector3 hc = TreeSpec.HatchLocal; float hz = hc.z;
        Transform top = new GameObject("Copa_arbol").transform; top.SetParent(tree, false); top.localPosition = Vector3.up * H;
        void Deck(float x0, float x1, float z0, float z1) => Solid(top, new Vector3((x0 + x1) * 0.5f, -0.15f, (z0 + z1) * 0.5f), new Vector3(x1 - x0, 0.3f, z1 - z0), TreeDeck, "Losa_copa");
        Deck(-X, X, hz + co, X); Deck(-X, X, -X, hz - co); Deck(-X, -co, hz - co, hz + co); Deck(co, X, hz - co, hz + co);
        // LA COLUMNA DE LA PUERTA (en el marco del árbol; su agua sigue la del tronco).
        float cb = H - 0.3f, wt = H + h.doorHeight + 0.6f, ctop = wt + 0.4f;
        Transform cg = new GameObject("Campana_techo").transform; cg.SetParent(tree, false);
        var v = new List<Vector3>(); var t = new List<int>();
        Vector3 cc = new Vector3(0f, 0f, hz);
        for (int f = 0; f < 4; f++)
        {
            var g = new List<Rect>();
            if (f == 0) g.Add(new Rect(-h.doorWidth * 0.5f, H, h.doorWidth, h.doorHeight));   // +Z: la puerta
            Panel(v, t, HelixTower.SideRot(f), cc, -ci, co, cb, ctop, ci, co, g);
        }
        Mesh m = TreeMesh("Campana_techo", v, t);
        cg.gameObject.AddComponent<MeshFilter>().sharedMesh = m;
        cg.gameObject.AddComponent<MeshRenderer>().sharedMaterial = KitchenProps.Mat(VirtualizationTank.GlassColor);
        cg.gameObject.AddComponent<MeshCollider>().sharedMesh = m;
        Solid(cg, new Vector3(0f, ctop - cw * 0.5f, hz), new Vector3(2f * co, cw, 2f * co), VirtualizationTank.GlassColor, "Tapa_campana");
        WaterBox(cg, "Agua_techo", new Vector3(0f, (TreeSpec.WaterTop - 1.5f + wt) * 0.5f, hz), new Vector3(2f * ci, wt - (TreeSpec.WaterTop - 1.5f), 2f * ci));
        Vector3 outside = new Vector3(0f, H, hz + co + 2.5f);
        hatchField = DoorFrame(tree, new Vector3(0f, H, hz + ci), Quaternion.identity, cw, "Puerta_techo", outside);
        doorLevel = (outside, new Vector3(0f, H, hz + ci + cw * 0.5f), new Vector3(0f, H + 0.2f, hz + ci - 1.2f));
        Label(KitchenProps.C(top, new Vector3(co + 0.8f, 0.1f, hz + co + 0.4f), new Vector3(0.1f, 0.1f, 0.1f), TreeSteel, 0f, "Cartel_trampilla"),
              "Puerta del tronco: entra de frente y nada a tu piso", 1.2f);
        string surf = $"{TreeSpec.Building}#copa";
        CarpetPad.BuildZone(top, top.TransformPoint(new Vector3(0f, 0.03f, 3.4f)), top.rotation, CarpetPad.Kind.Edificio, TreeSpec.Building, 9, surf, "Techo (alfombras)", 2);
        CarpetPad.BuildZone(top, top.TransformPoint(new Vector3(0f, 0.03f, 8.1f)), top.rotation, CarpetPad.Kind.Santuario, TreeSpec.Building, 9, surf,
            $"{TreeSpec.Building}: bosque comestible, bambusal, arrozal, milpa, huerto, chacra, rotación y nidos", 2);
        return top;
    }

    /// <summary>Al final de <see cref="PlaceCarpetAreas"/>: el árbol v2 ya se construye en su forma (pisos planos, girados de 90 en
    /// 90°); solo queda anotarlo.</summary>
    static void PoseTree()
    {
        if (_treeRoot == null || _treePosed) return;
        _treePosed = true;
        Debug.Log($"[Árbol] Hélice lista: {_treeFloors.Count} plataformas (pisos pegados al tronco y campos de punta), lados alternos de un nivel al siguiente.");
    }

    // ── MALLAS ───────────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>Una PARED plana con HUECOS: en el marco (<paramref name="q"/>, <paramref name="o"/>), x de <paramref name="x0"/> a
    /// <paramref name="x1"/>, y de <paramref name="y0"/> a <paramref name="y1"/>, grosor de z = <paramref name="zIn"/> a
    /// <paramref name="zOut"/>. Celdas cortadas por los bordes de los huecos (y cada 8 m de alto); una celda falta si su centro
    /// cae en un hueco. Caras de fuera, de dentro y los cantos de los huecos y los extremos: sirve para verse y para chocar.</summary>
    static void Panel(List<Vector3> v, List<int> t, Quaternion q, Vector3 o, float x0, float x1, float y0, float y1, float zIn, float zOut, List<Rect> gaps)
    {
        var xs = new List<float> { x0, x1 }; var ys = new List<float> { y0, y1 };
        foreach (Rect g in gaps)
        {
            if (g.xMin > x0 && g.xMin < x1) xs.Add(g.xMin); if (g.xMax > x0 && g.xMax < x1) xs.Add(g.xMax);
            if (g.yMin > y0 && g.yMin < y1) ys.Add(g.yMin); if (g.yMax > y0 && g.yMax < y1) ys.Add(g.yMax);
        }
        for (float y = y0 + 8f; y < y1; y += 8f) ys.Add(y);
        xs.Sort(); ys.Sort();
        for (int i = xs.Count - 1; i > 0; i--) if (xs[i] - xs[i - 1] < 0.01f) xs.RemoveAt(i);
        for (int i = ys.Count - 1; i > 0; i--) if (ys[i] - ys[i - 1] < 0.01f) ys.RemoveAt(i);
        int nx = xs.Count - 1, ny = ys.Count - 1;
        bool[,] solid = new bool[nx, ny];
        for (int i = 0; i < nx; i++)
            for (int j = 0; j < ny; j++)
            {
                Vector2 c = new Vector2((xs[i] + xs[i + 1]) * 0.5f, (ys[j] + ys[j + 1]) * 0.5f); bool s = true;
                foreach (Rect g in gaps) if (g.Contains(c)) { s = false; break; }
                solid[i, j] = s;
            }
        Vector3 P(float x, float y, float z) => o + q * new Vector3(x, y, z);
        void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 n)
        {
            int k = v.Count;
            if (Vector3.Dot(Vector3.Cross(b - a, c - a), q * n) < 0f) { Vector3 tmp = b; b = d; d = tmp; }
            v.Add(a); v.Add(b); v.Add(c); v.Add(d); t.Add(k); t.Add(k + 1); t.Add(k + 2); t.Add(k); t.Add(k + 2); t.Add(k + 3);
        }
        for (int i = 0; i < nx; i++)
            for (int j = 0; j < ny; j++)
            {
                if (!solid[i, j]) continue;
                float xa = xs[i], xb = xs[i + 1], ya = ys[j], yb = ys[j + 1];
                Quad(P(xa, ya, zOut), P(xa, yb, zOut), P(xb, yb, zOut), P(xb, ya, zOut), Vector3.forward);   // fuera
                Quad(P(xa, ya, zIn), P(xa, yb, zIn), P(xb, yb, zIn), P(xb, ya, zIn), Vector3.back);         // dentro
                if (j == 0 || !solid[i, j - 1]) Quad(P(xa, ya, zIn), P(xb, ya, zIn), P(xb, ya, zOut), P(xa, ya, zOut), Vector3.down);
                if (j == ny - 1 || !solid[i, j + 1]) Quad(P(xa, yb, zIn), P(xb, yb, zIn), P(xb, yb, zOut), P(xa, yb, zOut), Vector3.up);
                if (i == 0 || !solid[i - 1, j]) Quad(P(xa, ya, zIn), P(xa, yb, zIn), P(xa, yb, zOut), P(xa, ya, zOut), Vector3.left);
                if (i == nx - 1 || !solid[i + 1, j]) Quad(P(xb, ya, zIn), P(xb, yb, zIn), P(xb, yb, zOut), P(xb, ya, zOut), Vector3.right);
            }
    }

    /// <summary>Muchas cajas (centro, tamaño, giro) en una sola malla (los nervios y zunchos: cientos de piezas, un solo dibujo).</summary>
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
        return TreeMesh(name, v, t);
    }

    static Mesh TreeMesh(string name, List<Vector3> v, List<int> t)
    {
        Mesh m = new Mesh { name = name };
        if (v.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m.vertices = v.ToArray(); m.triangles = t.ToArray();
        m.RecalculateNormals(); m.RecalculateBounds();
        return m;
    }
}
