using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL MODELO DEL ÁRBOL PARA TODOS LOS EDIFICIOS (Berón 2026-10-30/31; docs/edificio-arbol.md §Migración): el constructor GENÉRICO
/// del edificio en hélice (<see cref="HelixTower"/>). Lo usan el Árbol de los Campos (SampleSceneBuilder.Tree.cs: sus cultivos)
/// y las torres migradas (Servicios, Entrenamiento, Ciencias, Producción: sus áreas).
/// - <see cref="BeginHelix"/>: la raíz (con su <see cref="ArbolCampos"/>: niveles y rutas a nado), en su sitio y su giro.
/// - <see cref="HelixFloor"/>: un PISO pegado al tronco (losa cuadrada en voladizo, ménsulas y tornapuntas) en su lado y su nivel;
///   si es un área, el área entera pasa a ser ese piso (sus muebles, en su marco). Las torres los piden en orden con
///   <see cref="TowerFloor"/>.
/// - <see cref="FinishHelix"/>: con los muebles ya puestos (las torres lo hacen al empezar las alfombras), el TRONCO cuadrado de
///   agua con sus puertas de 3 × 3 m (en cada piso, donde su umbral queda libre; y en el balcón del ANILLO del piso, si lo
///   tiene), las RAMAS con sus PUNTAS y sus anillos a la medida, el TECHO (alfombras y puerta de frente) y las alfombras del SUELO.
/// Entre niveles no hay alfombras: se nada por el tronco (ArbolCampos.Route). Solo el Árbol de los Campos gira.
/// </summary>
public static partial class SampleSceneBuilder
{
    /// <summary>Un edificio en hélice en construcción.</summary>
    class Helix
    {
        public string building, rootName;
        public Vector3 site; public float yaw;
        public HelixTower h; public bool rotates;
        public int groundSide;                      // la cara de la puerta del suelo
        public float padAngle = 45f, padAngle2 = 69f;   // brújula (mundo) de las alfombras del suelo, a 24 m del eje
        public Transform root; public ArbolCampos ac;
        public Transform[] decks; public Vector2[] sizes;
        public float[] doorX, ringDoorX;
        public Transform[] rings;
        public bool pickDoors;                      // torres: la puerta, donde su umbral queda libre de muebles
        public System.Func<int, Transform, string> tipContent;   // llena la punta k; devuelve su título
        public System.Func<int, string> floorTitle;
        public System.Func<int, Transform> spareFloor;            // un piso sin área (la torre tiene menos áreas que pisos)
        public string santuarioLabel;
        public bool finished;
        public Transform lastFloor;
        public int Platforms => 4 * h.levels;
        /// <summary>Alto del tronco: 17 m sobre el último nivel (su punta, su anillo y la puerta del techo).</summary>
        public float RootHeight => h.LevelY(h.levels) + 17f;
        public float WaterTop => RootHeight - 0.5f;
        public Vector3 HatchLocal => new Vector3(0f, RootHeight, -6f);
    }

    static readonly List<Helix> _helixes = new List<Helix>();
    static readonly Dictionary<Transform, (Helix hx, int k)> _helixFloor = new Dictionary<Transform, (Helix, int)>();
    static readonly Dictionary<Transform, int> _floorSeq = new Dictionary<Transform, int>();
    static readonly List<(Transform tip, string building)> _orchardTips = new List<(Transform, string)>();

    static readonly Color TreeSteel = new Color(0.36f, 0.39f, 0.44f), TreeCopper = new Color(0.72f, 0.45f, 0.22f),
        TreeGlow = new Color(0.55f, 0.85f, 1f, 0.35f), TreeDeck = new Color(0.48f, 0.4f, 0.33f), TreeSlab = new Color(0.66f, 0.6f, 0.52f);

    /// <summary>El agua del tronco: el mismo tono que el del tanque, más clara (20 m de agua: si no, no se vería nadar a nadie).</summary>
    static Color RootWater => new Color(VirtualizationTank.WaterColor.r, VirtualizationTank.WaterColor.g, VirtualizationTank.WaterColor.b, 0.2f);

    /// <summary>Medidas de una TORRE migrada: su piso cuadrado (el lado mayor de su área más grande), puntas de 16 m con 10 m de
    /// hueco (las alas de los anillos de los pisos vuelan 8 m) y el primer nivel del Árbol de los Campos (sobre el árbol más
    /// alto que crece en el santuario).</summary>
    public static HelixTower TowerHelix(Vector2 size, int levels, float firstY = 0f, float tipGap = 10f) => new HelixTower
    {
        waterHalf = 10f, wall = 1f, firstY = Mathf.Max(firstY, TreeSpec.Helix.firstY), levelGap = 11f, levels = levels,
        floorSize = Mathf.Max(size.x, size.y), tipSize = 16f, tipGap = tipGap, tipRise = 6f,
    };

    static Helix BeginHelix(Transform parent, string building, string rootName, Vector3 site, float yaw, HelixTower h, bool rotates)
    {
        var hx = new Helix { building = building, rootName = rootName, site = site, yaw = yaw, h = h, rotates = rotates };
        Transform tree = new GameObject(rootName).transform;
        tree.SetParent(parent, false); tree.SetPositionAndRotation(site, Quaternion.Euler(0f, yaw, 0f));
        hx.root = tree;
        hx.ac = tree.gameObject.AddComponent<ArbolCampos>();
        hx.ac.building = building; hx.ac.waterHalf = h.waterHalf; hx.ac.rootHeight = hx.RootHeight; hx.ac.reach = h.Reach;
        if (rotates)
        {
            tree.gameObject.AddComponent<TreeRotator>();
            Rigidbody rb = tree.gameObject.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false;   // gira: sus colliders, de un cuerpo cinemático
        }
        int P = hx.Platforms;
        hx.decks = new Transform[P + 1]; hx.sizes = new Vector2[P + 1]; hx.doorX = new float[P + 1]; hx.ringDoorX = new float[P + 1]; hx.rings = new Transform[P + 1];
        // LA PUERTA DEL SUELO y sus alfombras: hacia el hielo (las torres están en islas; el lado con hielo firme a 24 m).
        if (!rotates)
        {
            float best = -1f;
            for (int a = 0; a < 24; a++)
            {
                float ang = a * 15f, ok = 0f;
                for (float r = 16f; r <= 30f; r += 2f) { Vector3 p = site + TreeSpec.Dir(ang) * r; if (PolarLandscapeBuilder.NaturalIce(p.x, p.z)) ok += 1f; }
                Vector3 p2 = site + TreeSpec.Dir(ang + 24f) * 24f; if (PolarLandscapeBuilder.NaturalIce(p2.x, p2.z)) ok += 2f;
                if (ok > best) { best = ok; hx.padAngle = ang; hx.padAngle2 = ang + 24f; }
            }
            hx.groundSide = Mathf.RoundToInt(Mathf.Repeat(hx.padAngle - yaw, 360f) / 90f) % 4;
        }
        _helixes.Add(hx);
        return hx;
    }

    /// <summary>El PISO k (pegado al tronco) del edificio: <paramref name="deck"/> (un área, una planta propia de un área) o uno
    /// nuevo. Le pone su losa cuadrada en voladizo y lo apunta (sus plazas reservadas, su red de alfombras del edificio).</summary>
    static Transform HelixFloor(Helix hx, int k, Transform deck, Vector2 areaSize, string name = null)
    {
        HelixTower h = hx.h;
        var (lv, side, tip) = TreeSpec.Platform(k);
        if (tip) { Debug.LogError($"[Árbol] {hx.building}: la plataforma {k} es una punta, no un piso."); return deck; }
        if (deck == null) { deck = new GameObject(name ?? $"Piso_{hx.rootName}_{k}").transform; deck.SetParent(hx.root, false); }
        else if (deck.parent == null || deck.parent.GetComponentInParent<SanctuaryArea>() == null) deck.SetParent(hx.root, true);   // (una planta propia sigue colgando de su área)
        deck.SetPositionAndRotation(hx.root.TransformPoint(HelixTower.FromSide(side, h.FloorCenterS) + Vector3.up * h.LevelY(lv)), hx.root.rotation * HelixTower.SideRot(side));
        TreeFloor tf = deck.gameObject.GetOrAdd<TreeFloor>(); tf.index = k; tf.tilt = 0f; tf.yaw = side * 90f;
        HelixSlab(deck, h);
        _platforms[deck] = areaSize;   // (sus muebles y sus plazas reservadas, en la planta del área)
        hx.decks[k] = deck; hx.sizes[k] = areaSize; _helixFloor[deck] = (hx, k); _floorSeq[deck] = k;
        if (hx.lastFloor != null && hx.lastFloor != deck) _upperOf[deck] = hx.lastFloor;   // del mismo edificio (BaseOf: su primer piso)
        hx.lastFloor = deck;
        return deck;
    }

    /// <summary>LOSA cuadrada de un piso, en voladizo desde su cara del tronco: ménsulas y tornapuntas que bajan a la cara.</summary>
    static void HelixSlab(Transform f, HelixTower h)
    {
        float S = h.floorSize, half = S * 0.5f, fx = h.FloorCenterS.x;
        Transform s = new GameObject("Estructura_Elevada").transform; s.SetParent(f, false);
        Solid(s, new Vector3(0f, -0.15f, 0f), new Vector3(S, 0.3f, S), TreeSlab, "Losa_arbol");
        foreach (float xs in new[] { -9f, -3f, 3f, 9f })
            KitchenProps.C(s, new Vector3(xs - fx, -0.6f, -half + 5f), new Vector3(0.35f, 0.6f, 10f), TreeSteel, 0f, "Mensula");
        foreach (var (x0, x1) in new[] { (-9f, -8f), (-3f, 2f), (3f, 10f), (9f, 18f) })
        {
            Vector3 a = new Vector3(x0 - fx, -TreeSpec.StrutDrop, -half + 0.3f), b = new Vector3(x1 - fx, -0.35f, -half + 0.625f * S);
            KitchenProps.C(s, (a + b) * 0.5f, new Vector3(0.45f, 0.45f, (b - a).magnitude), TreeSteel, Quaternion.LookRotation(b - a), "Tornapunta");
        }
    }

    /// <summary>¿<paramref name="t"/> está en un PISO pegado al tronco de un edificio en hélice? (su anillo será en U, abierto al tronco)</summary>
    static bool HelixDeckOf(Transform t, out Helix hx, out int k, out Transform deck)
    {
        for (Transform x = t; x != null; x = x.parent)
            if (_helixFloor.TryGetValue(x, out var v)) { hx = v.hx; k = v.k; deck = x; return true; }
        hx = null; k = 0; deck = null; return false;
    }

    // ── TERMINAR: el tronco, las puertas, las ramas, las puntas, el techo y las alfombras ─────────────────────────

    static void FinishHelixes() { foreach (Helix hx in _helixes) FinishHelix(hx); }

    static void FinishHelix(Helix hx)
    {
        if (hx.finished) return;
        hx.finished = true;
        HelixTower h = hx.h;
        Transform tree = hx.root; ArbolCampos ac = hx.ac;
        int P = hx.Platforms;
        Physics.SyncTransforms();
        // Los pisos que no tienen área: libres (o lo que diga el edificio).
        for (int k = 1; k <= P; k++)
        {
            if (TreeSpec.Platform(k).tip || hx.decks[k] != null) continue;
            Transform d = hx.spareFloor != null ? hx.spareFloor(k) : null;
            if (d == null || hx.decks[k] == null) HelixFloor(hx, k, d, new Vector2(h.floorSize, h.floorSize), $"Piso_libre_{k}");
        }
        // Los ANILLOS de los pisos (RingDeck en U con su balcón contra el tronco) y dónde va cada puerta.
        for (int k = 1; k <= P; k++)
        {
            Transform d = hx.decks[k];
            if (d == null) continue;
            Transform wb = d.Find("Wall_Back"); if (wb != null) Object.DestroyImmediate(wb.gameObject);   // el contorno del lado del tronco: ahí está la puerta
            foreach (Transform t in d.GetComponentsInChildren<Transform>(true))
            {
                if (t.Find("Losa_anillo") == null || t.Find("Balcon_tronco") == null) continue;
                if (HelixDeckOf(t, out _, out _, out Transform own) && own == d) { hx.rings[k] = t; break; }
            }
            hx.doorX[k] = hx.pickDoors ? FreeDoorX(hx, k, d, 0f, null) : h.doorX;
            if (hx.rings[k] != null) hx.ringDoorX[k] = FreeDoorX(hx, k, hx.rings[k], RingHeight(hx, k), hx.rings[k]);
        }

        // ── HUECOS DE CADA CARA (marco del lado: x a lo largo de la cara, y altura) y bandas de losas (los nervios no las cruzan) ──
        var gaps = new List<Rect>[4]; var bands = new List<Vector2>[4];
        for (int s = 0; s < 4; s++) { gaps[s] = new List<Rect>(); bands[s] = new List<Vector2>(); }
        gaps[hx.groundSide].Add(DoorRect(h, h.doorX, 0f));
        for (int k = 1; k <= P; k++)
        {
            var (lv, side, tip) = TreeSpec.Platform(k);
            float Y = h.LevelY(lv);
            if (tip) { gaps[side].Add(new Rect(h.mouthX - h.tubeHalf, Y - 0.3f - h.tubeWall, 2f * h.tubeHalf, h.TubeInnerH + 2f * h.tubeWall)); continue; }
            gaps[side].Add(DoorRect(h, hx.doorX[k], Y)); bands[side].Add(new Vector2(Y - 1.2f, Y + 0.4f));
            if (hx.rings[k] != null) { float ry = Y + RingHeight(hx, k); gaps[side].Add(DoorRect(h, hx.ringDoorX[k], ry)); bands[side].Add(new Vector2(ry - 0.7f, ry + 0.2f)); }
        }
        Trunk(hx, gaps, bands);

        // ── NIVELES: el suelo, los pisos (y sus anillos), las puntas (y sus anillos) y el techo ──
        ac.levels.Clear();
        TreeLevel ground = WallLevel(h, TreeLevel.Kind.Ground, 0, hx.groundSide, 0f, "el suelo", null, 0f, h.doorX);
        ground.field = DoorFrame(tree, HelixTower.FromSide(hx.groundSide, new Vector3(h.doorX, 0f, h.waterHalf)), HelixTower.SideRot(hx.groundSide), h.wall, "Puerta_suelo", ground.outside);
        ac.levels.Add(ground);
        int ring = 0, floors = 0, tips = 0;
        var tipRings = new List<TreeLevel>();
        for (int k = 1; k <= P; k++)
        {
            var (lv, side, tip) = TreeSpec.Platform(k);
            if (tip) { var (fl, rl) = Branch(hx, k, ++ring); ac.levels.Add(fl); ac.levels.Add(rl); tips++; continue; }
            Transform f = hx.decks[k]; floors++;
            string title = hx.floorTitle != null ? hx.floorTitle(k) : FloorTitle(f, k);
            TreeLevel L = WallLevel(h, TreeLevel.Kind.Floor, k, side, h.LevelY(lv), $"el piso {k} ({title})", f, h.floorSize * 0.5f, hx.doorX[k]);
            L.level = lv;
            L.field = DoorFrame(tree, HelixTower.FromSide(side, new Vector3(hx.doorX[k], h.LevelY(lv), h.waterHalf)), HelixTower.SideRot(side), h.wall, $"Puerta_piso_{k}", L.outside);
            ac.levels.Add(L);
            Transform rg = hx.rings[k];
            if (rg == null) continue;
            float ry = h.LevelY(lv) + RingHeight(hx, k);
            TreeLevel R = WallLevel(h, TreeLevel.Kind.Ring, 0, side, ry, $"el anillo del piso {k} ({title})", rg, RingHalf(rg), hx.ringDoorX[k]);
            R.level = lv;
            R.field = DoorFrame(tree, HelixTower.FromSide(side, new Vector3(hx.ringDoorX[k], ry, h.waterHalf)), HelixTower.SideRot(side), h.wall, $"Puerta_anillo_piso_{k}", R.outside);
            tipRings.Add(R);
        }
        foreach (TreeLevel R in tipRings) { R.index = ++ring; ac.levels.Add(R); }   // (los anillos de las puntas son 1…N; luego los de los pisos)
        ac.top = Top(hx, out Transform hatchField, out var roof);
        ac.levels.Add(new TreeLevel
        {
            name = "el techo", kind = TreeLevel.Kind.Top, index = P + 1, deck = ac.top, half = TreeSpec.TopWidth * 0.5f, field = hatchField, doorAngle = 0f,
            outside = roof.outside, door = roof.door, inside = roof.inside,
        });

        // Alfombras del SUELO, en el hielo (no giran): al techo.
        Transform padRoot = new GameObject($"{hx.rootName}_alfombras_suelo").transform; padRoot.SetParent(tree.parent, false);
        Vector3 site = tree.position;
        Vector3 padAt = site + TreeSpec.Dir(hx.padAngle) * 24f + Vector3.up * 0.01f;
        CarpetPad.BuildZone(padRoot, padAt, Quaternion.LookRotation(site - new Vector3(padAt.x, site.y, padAt.z)), CarpetPad.Kind.Edificio, hx.building, 0, $"{hx.building}#0", "Suelo (hielo)", 2);
        Vector3 padS = site + TreeSpec.Dir(hx.padAngle2) * 24f + Vector3.up * 0.01f;
        CarpetPad.BuildZone(padRoot, padS, Quaternion.LookRotation(site - new Vector3(padS.x, site.y, padS.z)), CarpetPad.Kind.Santuario, hx.building, 0, $"{hx.building}#0",
            $"{hx.building} (al pie: a los pisos, nadando por el tronco)", 2);

        if (!hx.rotates) _roofs.Add((ac.top, new Vector2(TreeSpec.TopWidth, TreeSpec.TopWidth), hx.building));   // sus paneles solares, en el techo (un borde libre)

        // EL TRONCO AGUANTA (StructuralLimits.RootCheck).
        StructuralLimits.RootReport rep = StructuralLimits.RootCheck(h, floors, tips, hx.RootHeight, TreeSpec.Ribs, TreeSpec.HoopSpacing, TreeSpec.HoopArea, TreeSpec.StrutDrop);
        if (rep.Fail) Debug.LogError($"[Estructura] El tronco de «{hx.building}» no aguanta: {rep}. Refuérzalo (más nervios, más pared o más tornapuntas).");
        else if (rep.Warn) Debug.LogWarning($"[Estructura] El tronco de «{hx.building}» va justo: {rep}.");
        else Debug.Log($"[Estructura] Tronco de «{hx.building}» ({2f * h.OuterHalf:0} × {2f * h.OuterHalf:0} m, {TreeSpec.Ribs} nervios): {rep}.");
        Debug.Log($"[Árbol] «{hx.building}» en ({site.x:0}, {site.z:0}), giro {hx.yaw:0}°: tronco cuadrado de {hx.RootHeight:0} m, hélice de {h.levels} niveles desde {h.firstY:0} m, " +
                  $"{floors} pisos de {h.floorSize:0} m, {tips} puntas de {h.tipSize:0} m, {ring} anillos, alcance {h.Reach:0.0} m, puerta del suelo en la cara {hx.groundSide}.");
    }

    static float RingHeight(Helix hx, int k) => hx.rings[k] != null ? hx.decks[k].InverseTransformPoint(hx.rings[k].position).y : 0f;

    /// <summary>Medio lado (en su marco) de lo que ocupa un anillo de piso.</summary>
    static float RingHalf(Transform ring)
    {
        float m = 0f;
        foreach (Renderer r in ring.GetComponentsInChildren<Renderer>())
        {
            if (r.name != "Losa_anillo") continue;
            Vector3 c = ring.InverseTransformPoint(r.bounds.center), e = r.bounds.extents;
            m = Mathf.Max(m, Mathf.Abs(c.x) + Mathf.Max(e.x, e.z), Mathf.Abs(c.z) + Mathf.Max(e.x, e.z));
        }
        return m;
    }

    static string FloorTitle(Transform f, int k)
    {
        SanctuaryArea a = f.GetComponent<SanctuaryArea>(), owner = f.parent != null ? f.parent.GetComponentInParent<SanctuaryArea>() : null;
        if (a != null) return a.displayName;
        string nice = f.name.Replace("Planta_", "").Replace('_', ' ');
        return owner != null ? $"{owner.displayName}: {nice}" : nice;
    }

    /// <summary>Dónde va la puerta del piso (o del balcón de su anillo) a lo largo de su cara: donde el umbral (3,4 m de ancho y
    /// 4,4 m hacia dentro del piso) choca con menos muebles y, si empata, lo más cerca del sitio de siempre.</summary>
    static float FreeDoorX(Helix hx, int k, Transform deck, float yOff, Transform ringOnly)
    {
        HelixTower h = hx.h;
        var (lv, side, _) = TreeSpec.Platform(k);
        float Y = h.LevelY(lv) + yOff;
        var boxes = new List<Bounds>();
        foreach (Renderer r in deck.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled || r.GetComponent<TMPro.TMP_Text>() != null) continue;
            if (ringOnly == null && r.transform.IsChildOf(deck) && HelixRingOf(r.transform, deck)) continue;   // (lo del anillo no estorba en el piso)
            string n = r.name;
            if (n == "Mat" || n.StartsWith("Losa") || n.StartsWith("Viga") || n.StartsWith("Suelo") || n == "Zona" || n == "Mensula" || n.StartsWith("Tornapunta")) continue;
            Bounds b = r.bounds;
            if (b.size.y < 0.3f && Mathf.Max(b.size.x, b.size.z) > 2.5f) continue;   // suelos, alfombras de suelo
            boxes.Add(b);
        }
        foreach (Collider col in deck.GetComponentsInChildren<Collider>())   // (los muebles con caja de colisión sin dibujo: la cámara de frío, las líneas…)
        {
            if (!col.enabled || col.isTrigger || col.GetComponentInParent<CarpetPad>() != null) continue;
            if (ringOnly == null && HelixRingOf(col.transform, deck)) continue;
            string n = col.name;
            if (n == "Mat" || n.StartsWith("Losa") || n.StartsWith("Viga") || n.StartsWith("Suelo") || n.StartsWith("Wall_") || n == "Mensula" || n.StartsWith("Tornapunta")) continue;
            Bounds b = col.bounds;
            if (b.size.y < 0.3f && Mathf.Max(b.size.x, b.size.z) > 2.5f) continue;
            boxes.Add(b);
        }
        float best = h.doorX, bestScore = float.MaxValue, lim = h.OuterHalf - 0.6f - h.doorWidth * 0.5f - 0.4f;
        for (float x = -lim; x <= lim + 0.01f; x += 0.5f)
        {
            Vector3 c = hx.root.TransformPoint(HelixTower.FromSide(side, new Vector3(x, Y + 1.3f, h.OuterHalf + 2.3f)));
            Vector3 e = hx.root.rotation * HelixTower.SideRot(side) * new Vector3(3.4f, 2.2f, 4.4f);
            Bounds zone = new Bounds(c, new Vector3(Mathf.Abs(e.x), Mathf.Abs(e.y), Mathf.Abs(e.z)));
            int hits = 0; foreach (Bounds b in boxes) if (b.Intersects(zone)) hits++;
            float score = hits * 100f + Mathf.Abs(x - h.doorX);
            if (score < bestScore) { bestScore = score; best = x; }
        }
        if (bestScore >= 100f) Debug.LogWarning($"[Árbol] «{hx.building}» piso {k}{(ringOnly != null ? " (anillo)" : "")}: ningún umbral del todo libre para su puerta ({Mathf.FloorToInt(bestScore / 100f)} muebles).");
        return best;
    }

    static bool HelixRingOf(Transform t, Transform deck)
    {
        for (Transform x = t; x != null && x != deck; x = x.parent) if (x.Find("Balcon_tronco") != null) return true;
        return false;
    }

    static Rect DoorRect(HelixTower h, float x, float y) => new Rect(x - h.doorWidth * 0.5f, y, h.doorWidth, h.doorHeight);

    /// <summary>Un nivel con su puerta en una cara del tronco (el suelo, los pisos pegados y sus anillos).</summary>
    static TreeLevel WallLevel(HelixTower h, TreeLevel.Kind kind, int k, int side, float y, string name, Transform deck, float half, float doorX) => new TreeLevel
    {
        name = name, kind = kind, index = k, side = side, deck = deck, half = half, doorAngle = side * 90f,
        outside = HelixTower.FromSide(side, new Vector3(doorX, y, h.OuterHalf + 2.5f)),
        door = HelixTower.FromSide(side, new Vector3(doorX, y, h.waterHalf + h.wall * 0.5f)),
        inside = HelixTower.FromSide(side, new Vector3(doorX, y + 0.2f, h.waterHalf - 1.5f)),
    };

    // ── EL TRONCO ─────────────────────────────────────────────────────────────────────────────────────────────

    static void Trunk(Helix hx, List<Rect>[] gaps, List<Vector2>[] bands)
    {
        Transform tree = hx.root; ArbolCampos ac = hx.ac; HelixTower h = hx.h;
        float a = h.waterHalf, o = h.OuterHalf, H = hx.RootHeight;
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
        float wy0 = TreeSpec.WaterBottom, wy1 = hx.WaterTop;
        ac.water = WaterBox(tree, "Agua_raiz", new Vector3(0f, (wy0 + wy1) * 0.5f, 0f), new Vector3(2f * a, wy1 - wy0, 2f * a));
        KitchenProps.C(tree, Vector3.up * 0.02f, new Vector3(2f * a, 0.04f, 2f * a), new Color(0.3f, 0.42f, 0.48f), 0f, "Fondo_raiz");
        KitchenProps.C(tree, Vector3.down * 0.4f, new Vector3(2f * o + 3f, 0.8f, 2f * o + 3f), new Color(0.72f, 0.71f, 0.68f), 0f, "Encepado_raiz");   // el cimiento (bajo el hielo)

        // EXOESQUELETO: 24 nervios de acero (CFST: 4 esquinas y 5 por cara, uno cada ~3,7 m) y zunchos de canto cada 6 m, una sola
        // malla; no cruzan las puertas, las bocas de las ramas ni las losas de los pisos y de los balcones de sus anillos.
        var boxes = new List<(Vector3 c, Vector3 s, Quaternion q)>();
        bool AnyBand(float y0, float y1) { for (int s = 0; s < 4; s++) foreach (Vector2 b in bands[s]) if (y1 > b.x && y0 < b.y) return true; return false; }
        for (int s = 0; s < 4; s++)
        {
            Quaternion q = HelixTower.SideRot(s);
            int side = s;
            bool Blocked(float x0, float x1, float y0, float y1, float pad)
            {
                foreach (Rect g in gaps[side]) if (x1 > g.xMin - pad && x0 < g.xMax + pad && y1 > g.yMin - 0.3f && y0 < g.yMax + 0.3f) return true;
                foreach (Vector2 b in bands[side]) if (y1 > b.x && y0 < b.y) return true;
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
                if (AnyBand(y, y1)) continue;
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
        Label(KitchenProps.C(tree, HelixTower.FromSide(hx.groundSide, new Vector3(h.doorX - 2.4f, 3.4f, o + 0.8f)), new Vector3(0.1f, 0.1f, 0.1f), TreeSteel, 0f, "Cartel_raiz"),
              $"Tronco de {hx.building}: entra por la puerta (o por la del techo) y nada a tu piso", 0.4f);
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

    // ── LAS RAMAS, LAS PUNTAS Y SUS ANILLOS ─────────────────────────────────────────────────────────────────────

    /// <summary>La rama de la plataforma k (una punta): tubo cuadrado de agua desde la cara (recto hacia fuera y luego a la
    /// derecha), la COLUMNA de agua en la esquina de la punta (puertas a la punta y al anillo), la punta (lo que lleva, de
    /// <see cref="Helix.tipContent"/>) y su anillo a la medida, con sus nidos.</summary>
    static (TreeLevel field, TreeLevel ring) Branch(Helix hx, int k, int ringN)
    {
        Transform tree = hx.root; HelixTower h = hx.h;
        var (lv, side, _) = TreeSpec.Platform(k);
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
        // CELOSÍA de acero de la rama (lo que carga la punta): 2 cordones abajo, 2 arriba y diagonales.
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

        // ── COLUMNA DE LA PUNTA (marco del lado): del fondo de la rama al anillo; puertas a la punta (+X) y al anillo (−X) ──
        Transform cg = new GameObject($"Campana_rama_{k}").transform; cg.SetParent(tree, false); cg.localRotation = q;
        float cb = y0 - tw, wt = ringY + h.doorHeight + 0.6f, ctop = wt + 0.4f;
        var cv = new List<Vector3>(); var ct = new List<int>();
        Vector3 cc = new Vector3(cx, 0f, cz);
        for (int f = 0; f < 4; f++)
        {
            var g = new List<Rect>();
            if (f == 3) { g.Add(new Rect(-ti, y0, 2f * ti, hh)); g.Add(new Rect(-h.doorWidth * 0.5f, ringY, h.doorWidth, h.doorHeight)); }   // −X: la rama y el anillo
            if (f == 1) g.Add(new Rect(-h.doorWidth * 0.5f, tipY, h.doorWidth, h.doorHeight));                                                  // +X: la punta
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

        // ── LA PUNTA (cuadrada, con el hueco de la columna en su esquina de dentro) ──
        float T = h.tipSize, E = T * 0.5f;
        Vector3 tc = h.TipCenterS;
        Transform tip = new GameObject($"Campo_punta_{k}").transform;
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
        string title = hx.tipContent != null ? hx.tipContent(k, tip) : "terraza";
        hx.decks[k] = tip; hx.sizes[k] = new Vector2(T, T);

        // ── ANILLO encima de la punta (RingDeck, a la medida: 20 % sobre la punta y el resto volando), con sus nidos ──
        Transform rg = RingDeck(tip, $"Nido_anillo_{ringN}", new Vector2(T, T), h.ringH, h.ringWidth, h.ringInside);
        NestRing(rg, h.RingInnerHalf, h.ringWidth);

        // ── NIVELES (puntos locales del árbol, a ras de los pies) ──
        Vector3 S(Vector3 p) => HelixTower.FromSide(side, p);
        Vector3 mouthIn = new Vector3(mx, Y + 0.2f, a - 1.5f), elbow = new Vector3(mx, Y + 0.2f, cz), colFoot = new Vector3(cx - 0.3f, Y + 0.2f, cz);
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

    // Frutales enanos en las esquinas, flores para las abejas de las nieves y nidos de aves (el anillo, a la medida de la punta).
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
    /// el agua del tronco, con su puerta de 3 × 3 m en la cara +Z (hacia las alfombras).</summary>
    static Transform Top(Helix hx, out Transform hatchField, out (Vector3 outside, Vector3 door, Vector3 inside) doorLevel)
    {
        Transform tree = hx.root; HelixTower h = hx.h;
        float H = hx.RootHeight, X = TreeSpec.TopWidth * 0.5f, ci = TreeSpec.HatchHalf, cw = h.colWall, co = ci + cw;
        Vector3 hc = hx.HatchLocal; float hz = hc.z;
        Transform top = new GameObject("Copa_arbol").transform; top.SetParent(tree, false); top.localPosition = Vector3.up * H;
        void Deck(float x0, float x1, float z0, float z1) => Solid(top, new Vector3((x0 + x1) * 0.5f, -0.15f, (z0 + z1) * 0.5f), new Vector3(x1 - x0, 0.3f, z1 - z0), TreeDeck, "Losa_copa");
        Deck(-X, X, hz + co, X); Deck(-X, X, -X, hz - co); Deck(-X, -co, hz - co, hz + co); Deck(co, X, hz - co, hz + co);
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
        WaterBox(cg, "Agua_techo", new Vector3(0f, (hx.WaterTop - 1.5f + wt) * 0.5f, hz), new Vector3(2f * ci, wt - (hx.WaterTop - 1.5f), 2f * ci));
        Vector3 outside = new Vector3(0f, H, hz + co + 2.5f);
        hatchField = DoorFrame(tree, new Vector3(0f, H, hz + ci), Quaternion.identity, cw, "Puerta_techo", outside);
        doorLevel = (outside, new Vector3(0f, H, hz + ci + cw * 0.5f), new Vector3(0f, H + 0.2f, hz + ci - 1.2f));
        Label(KitchenProps.C(top, new Vector3(co + 0.8f, 0.1f, hz + co + 0.4f), new Vector3(0.1f, 0.1f, 0.1f), TreeSteel, 0f, "Cartel_trampilla"),
              "Puerta del tronco: entra de frente y nada a tu piso", 1.2f);
        string surf = $"{hx.building}#copa";
        int idx = hx.Platforms + 1;
        CarpetPad.BuildZone(top, top.TransformPoint(new Vector3(0f, 0.03f, 3.4f)), top.rotation, CarpetPad.Kind.Edificio, hx.building, idx, surf, "Techo (alfombras)", 2);
        CarpetPad.BuildZone(top, top.TransformPoint(new Vector3(0f, 0.03f, 8.1f)), top.rotation, CarpetPad.Kind.Santuario, hx.building, idx, surf,
            hx.santuarioLabel ?? HelixAreasText(hx), 2);
        return top;
    }

    /// <summary>Lo que hay en el edificio (lo que enseñan las alfombras del santuario): «Servicios: Cocina, Hospital, Cría…».</summary>
    static string HelixAreasText(Helix hx)
    {
        var names = new List<string>();
        for (int k = 1; k <= hx.Platforms; k++)
        {
            Transform d = hx.decks[k];
            if (d == null || TreeSpec.Platform(k).tip) continue;
            SanctuaryArea a = d.GetComponent<SanctuaryArea>() ?? (d.parent != null ? d.parent.GetComponentInParent<SanctuaryArea>() : null);
            string n = a != null ? a.displayName : FloorTitle(d, k);
            if (!names.Contains(n)) names.Add(n);
        }
        return $"{hx.building}: {string.Join(", ", names)}";
    }

    /// <summary>Las partes del árbol que no se andan (agua, campos, exoesqueleto, tubos y columnas de las ramas): fuera del NavMesh.</summary>
    static void HelixNavIgnore(Transform tree)
    {
        foreach (Transform t in tree.GetComponentsInChildren<Transform>(true))
        {
            string n = t.name;
            if (n.StartsWith("Agua") || n == "Campo_magnetico" || n == "Cascada" || n == "Exoesqueleto" || n.StartsWith("Rama_") || n.StartsWith("Campana_rama_")
                || n == "Nervio_rama" || n.StartsWith("Tornapunta") || n == "Tapa_raiz" || n == "Cartel_raiz" || n == "Encepado_raiz" || n == "Fondo_raiz" || n == "Pared_raiz" || n == "Campana_techo")
                t.gameObject.GetOrAdd<Unity.AI.Navigation.NavMeshModifier>().ignoreFromBuild = true;
        }
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

public static partial class SampleSceneBuilder
{
    /// <summary>¿Choca <paramref name="world"/> (con <paramref name="margin"/> m de holgura) con algún edificio-árbol? Se mide con sus
    /// medidas (aún sin construir el tronco ni las puntas): tronco, pisos con un anillo de 8 m, ramas, puntas con su anillo y techo;
    /// el Árbol de los Campos, girando (su círculo). Lo usa el árbol de la guardia para que sus ramas los esquiven.</summary>
    static bool HelixBlockedAt(Vector3 world, float margin)
    {
        foreach (Helix hx in _helixes)
        {
            if (hx.root == null) continue;
            HelixTower h = hx.h;
            Vector3 l = hx.root.InverseTransformPoint(world);
            float o = h.OuterHalf + 0.6f;
            if (l.y > -2f && l.y < hx.RootHeight + 5f + margin && Mathf.Abs(l.x) < o + margin && Mathf.Abs(l.z) < o + margin) return true;   // tronco (y techo)
            if (hx.rotates)
            {
                float r = new Vector2(l.x, l.z).magnitude;
                if (l.y > h.firstY - TreeSpec.StrutDrop - margin && l.y < hx.RootHeight + 5f + margin && r < h.Reach + margin) return true;
                continue;
            }
            for (int k = 1; k <= hx.Platforms; k++)
            {
                var (lv, side, tip) = TreeSpec.Platform(k);
                Vector3 s = Quaternion.Inverse(HelixTower.SideRot(side)) * l;   // al marco del lado
                float Y = h.LevelY(lv);
                bool In(float x0, float x1, float z0, float z1, float y0, float y1) =>
                    s.x > x0 - margin && s.x < x1 + margin && s.z > z0 - margin && s.z < z1 + margin && s.y > y0 - margin && s.y < y1 + margin;
                if (!tip) { if (In(-h.OuterHalf - 8f, h.floorSize - h.OuterHalf + 8f, h.OuterHalf, h.OuterHalf + h.floorSize + 8f, Y - TreeSpec.StrutDrop, Y + 8f)) return true; continue; }
                Vector3 t = h.TipCenterS; float e = h.tipSize * 0.5f + 3.2f;
                if (In(t.x - e, t.x + e, t.z - e, t.z + e, h.TipY(lv) - TreeSpec.StrutDrop, h.RingY(lv) + 4f)) return true;
                if (In(h.mouthX - h.tubeHalf, h.ColumnS.x + h.ColOuter, h.OuterHalf, h.ColumnS.z + h.ColOuter, Y - 1f, Y + 6f)) return true;
            }
        }
        return false;
    }

    /// <summary>Lo más alto de los edificios (el techo del edificio-árbol más alto, con la columna de su puerta).</summary>
    static float TallestBuilding()
    {
        float m = 0f;
        foreach (Helix hx in _helixes) m = Mathf.Max(m, hx.RootHeight + TreeSpec.DoorHeight + 1.6f);
        return m;
    }
}
