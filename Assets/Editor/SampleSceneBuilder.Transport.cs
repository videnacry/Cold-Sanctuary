using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TRANSPORTE ENTRE PLATAFORMAS (docs/transporte-alfombras.md):
/// - Las plataformas CERCANAS (p. ej. el restaurante y la enfermería) se unen con PUENTES de 8 m de ancho.
/// - Cada GRUPO de plataformas unidas por puentes tiene UNA pista de aterrizaje de ALFOMBRAS VOLADORAS (como una sala
///   de yoga con esterillas grandes), unida a una de sus áreas por un puente. La arboleda residencial tiene la suya.
/// Así hay menos pistas y menos necesidad de volar: dentro de un grupo se va andando por los puentes.
/// </summary>
public static partial class SampleSceneBuilder
{
    const float BridgeWidth = 8f, PadSize = 14f;

    struct Plat { public Transform t; public string name; public Vector2 c, half; public int group; }

    static readonly Dictionary<Transform, List<(int side, float a, float b)>> _openings = new Dictionary<Transform, List<(int, float, float)>>();

    static void BuildBridgesAndPads(Transform parent, SanctuaryArea[] areas, Vector2[] sizes)
    {
        _openings.Clear(); _landings.Clear();
        var plats = new List<Plat>();
        for (int i = 0; i < areas.Length && i < sizes.Length; i++)
        {
            Vector3 p = areas[i].transform.position;
            plats.Add(new Plat { t = areas[i].transform, name = areas[i].displayName, c = new Vector2(p.x, p.z), half = sizes[i] * 0.5f, group = i });
        }
        var bridgeRects = new List<Rect>();
        Transform root = new GameObject("Transporte_Plataformas").transform; root.SetParent(parent, false);

        // PUENTES de 8 m entre plataformas cercanas (rectos, pueden ir en diagonal; sin cruzar otra área). Se unen primero
        // las más cercanas y solo si aún no estaban conectadas (como un árbol): menos puentes, menos grupos, menos pistas.
        var links = new List<(int i, int j, Vector2 from, Vector2 to, int sa, int sb, float gap)>();
        for (int i = 0; i < plats.Count; i++)
            for (int j = i + 1; j < plats.Count; j++)
                if (Link(plats[i], plats[j], out Vector2 from, out Vector2 to, out int sa, out int sb) && !Crosses(plats, i, j, from, to))
                    links.Add((i, j, from, to, sa, sb, Vector2.Distance(from, to)));
        links.Sort((x, y) => x.gap.CompareTo(y.gap));
        int bridges = 0;
        foreach (var l in links)
        {
            int gi = plats[l.i].group, gj = plats[l.j].group;
            if (gi == gj) continue;
            Bridge(root, new Vector3(l.from.x, ElevatedHeight, l.from.y), new Vector3(l.to.x, ElevatedHeight, l.to.y), BridgeWidth, $"Puente_{plats[l.i].name}_{plats[l.j].name}");
            OpenRail(plats[l.i], l.sa, l.sa < 2 ? l.from.y : l.from.x); OpenRail(plats[l.j], l.sb, l.sb < 2 ? l.to.y : l.to.x);
            bridgeRects.Add(SegRect(l.from, l.to)); bridges++;
            for (int k = 0; k < plats.Count; k++) if (plats[k].group == gj) { var q = plats[k]; q.group = gi; plats[k] = q; }
        }

        // UNA PISTA por grupo. Va ENCIMA del piso más alto de su área mayor, como un piso más de la plataforma multinivel
        // (Berón 2026-10-22: la pista no ocupa terreno aparte ni necesita su puente); las alfombras llegan desde el cielo
        // abierto. Su sitio exacto en ese piso se elige al final (`PlaceStackedPads`), con los muebles ya puestos. Solo si
        // ningún piso del grupo cabe una pista de 14 × 14 m se hace aparte, junto a un área, como antes.
        _padGroups.Clear();
        var taken = new List<Rect>(bridgeRects);
        foreach (Plat p in plats) taken.Add(Inflate(PRect(p), 3f));
        foreach (Rect g in Gardens) taken.Add(g);
        taken.Add(new Rect(-60f - 34f, 95f - 34f, 68f, 68f));   // la arboleda residencial
        var groups = new Dictionary<int, List<Plat>>();
        foreach (Plat p in plats) { if (!groups.ContainsKey(p.group)) groups[p.group] = new List<Plat>(); groups[p.group].Add(p); }
        int pads = 0;
        foreach (var kv in groups)
        {
            var members = kv.Value; members.Sort((a, b) => (b.half.x * b.half.y).CompareTo(a.half.x * a.half.y));
            var allNames = new List<string>(); var allCenters = new List<Vector3>();
            foreach (Plat x in members) { allNames.Add(x.name); allCenters.Add(new Vector3(x.c.x, ElevatedHeight, x.c.y)); }
            bool stacked = false;
            foreach (Plat m in members)
            {
                int ri = _roofs.FindIndex(rf => rf.name == m.name);
                if (ri < 0 || _roofs[ri].size.x < PadSize + 2f || _roofs[ri].size.y < PadSize + 2f) continue;
                _padGroups.Add((ri, string.Join(" + ", allNames), allCenters.ToArray()));
                stacked = true; pads++;
                break;
            }
            if (stacked) continue;
            bool placed = false;
            foreach (Plat m in members)
            {
                foreach (int side in new[] { 1, 0, 3 })   // -X, +X, -Z (por +Z sube la escalera)
                {
                    Vector2 dir = SideDir(side);
                    float halfAlong = side < 2 ? m.half.x : m.half.y;
                    Vector2 c = m.c + dir * (halfAlong + 6f + PadSize * 0.5f);
                    Rect pr = new Rect(c.x - PadSize * 0.5f - 3f, c.y - PadSize * 0.5f - 3f, PadSize + 6f, PadSize + 6f);
                    bool clash = false; foreach (Rect t in taken) if (t.Overlaps(pr) && !(t.center == Inflate(PRect(m), 3f).center)) { clash = true; break; }
                    if (clash) continue;
                    var names = new List<string>(); var centers = new List<Vector3>();
                    foreach (Plat x in members) { names.Add(x.name); centers.Add(new Vector3(x.c.x, ElevatedHeight, x.c.y)); }
                    // La pista mira con su −Z al área (por ahí llega el puente); su escalera queda al otro lado.
                    Transform pad = Pad(root, new Vector3(c.x, 0f, c.y), Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.y)), string.Join(" + ", names), centers.ToArray());
                    Vector2 edgeA = m.c + dir * halfAlong, edgeB = c - dir * PadSize * 0.5f;
                    Bridge(root, new Vector3(edgeA.x, ElevatedHeight, edgeA.y), new Vector3(edgeB.x, ElevatedHeight, edgeB.y), BridgeWidth, $"Puente_pista_{m.name}");
                    float along = side < 2 ? m.c.y : m.c.x;
                    OpenRail(m, side, along);
                    OpenRailPad(pad, 3, 0f, BridgeWidth);
                    taken.Add(pr); taken.Add(SegRect(edgeA, edgeB)); pads++; placed = true;
                    break;
                }
                if (placed) break;
            }
            if (!placed) Debug.LogWarning($"[Transporte] Sin sitio para la pista de {members[0].name}.");
        }

        // ARBOLEDA RESIDENCIAL: su pista, hacia fuera de la cabaña de 0° (entre dos árboles; las de arriba van encima), con
        // un puente de 4 m desde su lado exterior (r 15).
        Vector3 rc = new Vector3(-60f, 0f, 95f);
        Vector3 rd = new Vector3(Mathf.Cos(ResidentialBuilder.PadAngle * Mathf.Deg2Rad), 0f, Mathf.Sin(ResidentialBuilder.PadAngle * Mathf.Deg2Rad));
        Vector3 padC = rc + rd * (15f + 5f + PadSize * 0.5f);
        Transform rpad = Pad(root, padC, Quaternion.LookRotation(rd), "Arboleda residencial (habitaciones)", new[] { rc + Vector3.up * ElevatedHeight });
        Bridge(root, rc + rd * 15f + Vector3.up * ElevatedHeight, rc + rd * 20f + Vector3.up * ElevatedHeight, 4f, "Puente_pista_residencial");
        OpenRailPad(rpad, 3, 0f, 4f);
        pads++;
        Debug.Log($"[Transporte] {bridges} puentes de 8 m (por cercanía, de {links.Count} posibles) y {pads} pistas de alfombras voladoras (una por grupo).");
    }

    // Pistas que van ENCIMA de un piso (índice en `_roofs`, nombre del grupo, centros de sus áreas).
    static readonly List<(int roof, string label, Vector3[] members)> _padGroups = new List<(int, string, Vector3[])>();

    /// <summary>Coloca cada pista de alfombras como PISO SUPERIOR (a 10 m) del piso más alto de su área: busca, en una
    /// rejilla de 2 m, el sitio donde sus columnas del borde y el pie de su escalera no caen sobre muebles. Después la
    /// pista pasa a ser ese "piso más alto" (sus paneles solares van en su borde).</summary>
    static void PlaceStackedPads()
    {
        ResolveInnerUpperFloors();   // pisos altos montados dentro de un área (la planta veterinaria del hospital)
        Transform root = GameObject.Find("Transporte_Plataformas")?.transform;
        foreach (var (ri, label, members) in _padGroups)
        {
            var (top, size, name) = _roofs[ri];
            if (top == null) continue;
            float hx = size.x * 0.5f - PadSize * 0.5f - 0.5f, hz = size.y * 0.5f - PadSize * 0.5f - 0.5f, floorY = top.position.y;
            var blockers = new List<Bounds>();
            foreach (Renderer rr in top.GetComponentsInChildren<Renderer>())
                if (rr.enabled && rr.bounds.min.y > floorY + 0.3f && rr.bounds.min.y < floorY + UpperFloorGap) blockers.Add(rr.bounds);
            _openings.TryGetValue(top, out var gaps);
            int Hits(Vector3 c)
            {
                int n = 0; float h = PadSize * 0.5f;
                var pts = new List<Vector3> { new Vector3(-h, 0f, 0f), new Vector3(h, 0f, 0f), new Vector3(0f, 0f, h + 1f) };   // columnas de los lados y pie de la escalera
                for (int i = 0; i < 4; i++) { float x = -h + i * (2f * h / 3f); pts.Add(new Vector3(x, 0f, -h)); pts.Add(new Vector3(x, 0f, h)); }   // columnas de delante y detrás
                foreach (Vector3 q in pts)
                {
                    Vector3 lp = c + q;   // ¿cae en la llegada de un puente? (4,5 m hacia dentro del hueco)
                    if (gaps != null && gaps.Exists(o =>
                    {
                        float depth = o.side == 0 ? size.x * 0.5f - lp.x : o.side == 1 ? lp.x + size.x * 0.5f : o.side == 2 ? size.y * 0.5f - lp.z : lp.z + size.y * 0.5f;
                        float along = o.side < 2 ? lp.z : lp.x;
                        return depth < 4.5f && along > o.a - 0.6f && along < o.b + 0.6f;
                    })) { n++; continue; }
                    Vector3 w = top.TransformPoint(lp) + Vector3.up * 1.5f;
                    Bounds b = new Bounds(w, new Vector3(1.4f, 2.4f, 1.4f));
                    foreach (Bounds bl in blockers) if (bl.Intersects(b)) { n++; break; }
                }
                return n;
            }
            Vector3 best = Vector3.zero; int bestN = int.MaxValue;
            for (float z = -hz; z <= hz + 0.01f && bestN > 0; z += 2f)
                for (float x = -hx; x <= hx + 0.01f && bestN > 0; x += 2f)
                {
                    int n = Hits(new Vector3(x, 0f, z));
                    if (n < bestN) { bestN = n; best = new Vector3(x, 0f, z); }
                }
            Transform pad = Pad(root != null ? root : top, top.TransformPoint(best + Vector3.up * 0.1f), top.rotation, label, members, UpperFloorGap, false);
            _roofs[ri] = (pad, new Vector2(PadSize, PadSize), name);
            if (bestN > 0) Debug.LogWarning($"[Transporte] Pista sobre «{name}»: {bestN} columnas tocan muebles (no hay hueco del todo libre).");
            else Debug.Log($"[Transporte] Pista de alfombras como piso superior de «{name}» ({label}).");
        }
        _padGroups.Clear();
    }

    // Los huertos elevados (MakePlot): no poner pistas encima.
    static readonly Rect[] Gardens =
    {
        new Rect(-45f - 13f, -75f - 13f, 26f, 26f), new Rect(-10f - 12f, -118f - 10f, 24f, 20f), new Rect(-60f - 14f, -15f - 11f, 28f, 22f),
        new Rect(25f - 15f, -82f - 11f, 30f, 22f), new Rect(-78f - 18f, 45f - 16f, 36f, 32f), new Rect(-42f - 11f, 0f - 9f, 22f, 18f),
    };

    static Rect PRect(Plat p) => new Rect(p.c.x - p.half.x, p.c.y - p.half.y, p.half.x * 2f, p.half.y * 2f);
    static Rect Inflate(Rect r, float m) => new Rect(r.x - m, r.y - m, r.width + 2f * m, r.height + 2f * m);
    static Vector2 SideDir(int side) => side == 0 ? Vector2.right : side == 1 ? Vector2.left : side == 2 ? Vector2.up : Vector2.down;

    const float MaxLink = 32f;

    // Puente entre los lados que se miran: el lado de A según hacia dónde está B (eje dominante) y el opuesto de B; el
    // punto de cada lado, lo más cerca posible del otro, sin pegarse a las esquinas (cabe el hueco de 8 m). Evita la
    // escalera vertical (centro del lado +Z).
    static bool Link(Plat A, Plat B, out Vector2 from, out Vector2 to, out int sa, out int sb)
    {
        Vector2 d = B.c - A.c;
        bool x = Mathf.Abs(d.x) / (A.half.x + B.half.x) > Mathf.Abs(d.y) / (A.half.y + B.half.y);
        sa = x ? (d.x > 0 ? 0 : 1) : (d.y > 0 ? 2 : 3); sb = x ? (sa == 0 ? 1 : 0) : (sa == 2 ? 3 : 2);
        from = SidePoint(A, sa, x ? B.c.y : B.c.x);
        to = SidePoint(B, sb, x ? from.y : from.x);
        from = SidePoint(A, sa, x ? to.y : to.x);
        float gap = Vector2.Distance(from, to);
        return gap > 0.5f && gap <= MaxLink && (x ? (to.x - from.x) * d.x > 0f : (to.y - from.y) * d.y > 0f);
    }

    static Vector2 SidePoint(Plat p, int side, float alongWorld)
    {
        float half = side < 2 ? p.half.y : p.half.x, margin = BridgeWidth * 0.5f + 0.5f;
        float center = side < 2 ? p.c.y : p.c.x;
        float u = half > margin ? Mathf.Clamp(alongWorld - center, -half + margin, half - margin) : 0f;
        if (side == 2 && Mathf.Abs(u) < BridgeWidth * 0.5f + 1f && half > BridgeWidth + 1.5f) u = u >= 0f ? BridgeWidth * 0.5f + 1f : -(BridgeWidth * 0.5f + 1f);   // no encima de la escalera
        return side == 0 ? new Vector2(p.c.x + p.half.x, center + u) : side == 1 ? new Vector2(p.c.x - p.half.x, center + u)
             : side == 2 ? new Vector2(center + u, p.c.y + p.half.y) : new Vector2(center + u, p.c.y - p.half.y);
    }

    static bool Crosses(List<Plat> plats, int i, int j, Vector2 a, Vector2 b)
    {
        for (int k = 0; k < plats.Count; k++)
        {
            if (k == i || k == j) continue;
            Rect r = Inflate(PRect(plats[k]), BridgeWidth * 0.5f + 1f);
            for (int s = 1; s < 20; s++) if (r.Contains(Vector2.Lerp(a, b, s / 20f))) return true;
        }
        return false;
    }

    static Rect SegRect(Vector2 a, Vector2 b)
    {
        float w = BridgeWidth * 0.5f;
        return new Rect(Mathf.Min(a.x, b.x) - w, Mathf.Min(a.y, b.y) - w, Mathf.Abs(b.x - a.x) + 2f * w, Mathf.Abs(b.y - a.y) + 2f * w);
    }

    // Tablero, barandillas (con barrera invisible de 4 m) y pilares.
    static void Bridge(Transform root, Vector3 a, Vector3 b, float width, string name)
    {
        Color wood = new Color(0.62f, 0.52f, 0.42f), woodDark = new Color(0.48f, 0.4f, 0.33f);
        Vector3 d = b - a; d.y = 0f; float len = d.magnitude; if (len < 0.1f) return;
        Transform t = new GameObject(name).transform; t.SetParent(root, false);
        t.position = (a + b) * 0.5f; t.rotation = Quaternion.LookRotation(d);
        Solid(t, new Vector3(0f, -0.15f, 0f), new Vector3(width, 0.3f, len + 0.6f), woodDark, "Tablero");
        for (int s = -1; s <= 1; s += 2)
        {
            Solid(t, new Vector3(s * width * 0.5f, 1f, 0f), new Vector3(0.08f, 0.08f, len), wood, "Barandilla");
            Transform bar = Solid(t, new Vector3(s * width * 0.5f, 2f, 0f), new Vector3(0.06f, 4f, len), wood, "Barandilla_Barrera");
            bar.GetComponent<Renderer>().enabled = false;
            int posts = Mathf.Max(2, Mathf.RoundToInt(len / 1.5f));
            for (int i = 0; i <= posts; i++) KitchenProps.C(t, new Vector3(s * width * 0.5f, 0.5f, -len * 0.5f + len * i / posts), new Vector3(0.07f, 1f, 0.07f), woodDark, 0f, "Balaustre");
            for (float z = -len * 0.5f + 2f; z < len * 0.5f - 1f; z += 6f)
                Solid(t, new Vector3(s * (width * 0.5f - 0.3f), -a.y * 0.5f - 0.15f, z), new Vector3(0.4f, a.y, 0.4f), wood, "Pilar");
        }
    }

    // LLEGADAS DE LOS PUENTES: el paso de 8 m (y 4 m hacia dentro) debe quedar libre de muebles (camas, mesas…), si no
    // el puente desemboca contra ellos. Se apuntan al construir y se despejan al final, con todo ya amueblado.
    static readonly List<(Transform area, Vector2 half, int side, float center)> _landings = new List<(Transform, Vector2, int, float)>();

    static void ClearBridgeLandings()
    {
        int moved = 0;
        foreach (var l in _landings)
        {
            if (l.area == null) continue;
            Vector3 inward = l.side == 0 ? Vector3.left : l.side == 1 ? Vector3.right : l.side == 2 ? Vector3.back : Vector3.forward;   // local, hacia dentro
            foreach (Transform c in l.area)
            {
                if (c.name == "Estructura_Elevada" || c.name == "Mat" || c.childCount == 0 || c.GetComponent<VirtualizationMachine>() != null) continue;
                foreach (Transform item in c)
                {
                    if (item.name.StartsWith("Suelo") || item.GetComponentInChildren<Renderer>() == null || !InLanding(l, item, out float depth)) continue;
                    // Se APARTA hacia dentro (no se borra): el paso del puente queda libre y no se pierde nada.
                    item.position += l.area.TransformDirection(inward) * (4.3f - depth);
                    moved++;
                }
            }
        }
        Debug.Log($"[Transporte] Llegadas de los puentes despejadas: {moved} muebles apartados del paso.");
        _landings.Clear();
    }

    static bool InLanding((Transform area, Vector2 half, int side, float center) l, Transform t, out float depth)
    {
        Vector3 p = l.area.InverseTransformPoint(t.position);
        depth = l.side == 0 ? l.half.x - p.x : l.side == 1 ? p.x + l.half.x : l.side == 2 ? l.half.y - p.z : p.z + l.half.y;
        float along = l.side < 2 ? p.z : p.x;
        return depth < 4f && Mathf.Abs(along - l.center) < BridgeWidth * 0.5f + 0.6f;
    }


    // Abre un hueco de 8 m en la barandilla del lado <side> de un área (donde llega un puente) y apunta su llegada.
    static void OpenRail(Plat p, int side, float alongWorld)
    {
        float centerLocal = side < 2 ? alongWorld - p.c.y : alongWorld - p.c.x;
        OpenRailIn(p.t, p.half, side, centerLocal, BridgeWidth);
        _landings.Add((p.t, p.half, side, centerLocal));
    }

    static void OpenRailPad(Transform pad, int side, float center, float width) => OpenRailIn(pad, new Vector2(PadSize, PadSize) * 0.5f, side, center, width);

    static void OpenRailIn(Transform area, Vector2 half, int side, float center, float width)
    {
        Transform s = area.Find("Estructura_Elevada"); if (s == null) return;
        if (!_openings.TryGetValue(area, out var list)) _openings[area] = list = new List<(int, float, float)>();
        list.Add((side, center - width * 0.5f, center + width * 0.5f));
        // Borra la barandilla de ese lado y la rehace con los huecos (puentes y, en +Z, la escalera).
        float edge = side < 2 ? half.x : half.y; float sign = side % 2 == 0 ? 1f : -1f;
        var kill = new List<GameObject>();
        foreach (Transform c in s)
        {
            if (c.name != "Barandilla" && c.name != "Barandilla_Barrera" && c.name != "Balaustre") continue;
            float perp = side < 2 ? c.localPosition.x : c.localPosition.z;
            if (Mathf.Abs(perp - sign * edge) < 0.2f) kill.Add(c.gameObject);
        }
        foreach (GameObject g in kill) Object.DestroyImmediate(g);
        float halfAlong = side < 2 ? half.y : half.x;
        var gaps = new List<(float a, float b)>();
        if (side == 2) gaps.Add((-0.6f, 0.6f));   // la escalera vertical
        foreach (var o in list) if (o.side == side) gaps.Add((o.a, o.b));
        gaps.Sort((x, y) => x.a.CompareTo(y.a));
        float cur = -halfAlong;
        Color wood = new Color(0.62f, 0.52f, 0.42f), woodDark = new Color(0.48f, 0.4f, 0.33f);
        void Seg(float a, float b)
        {
            if (b - a < 0.2f) return;
            Vector3 P(float u) => side < 2 ? new Vector3(sign * edge, 0f, u) : new Vector3(u, 0f, sign * edge);
            Vector3 from = P(a), to = P(b), mid = (from + to) * 0.5f, d = to - from;
            Transform bar = Solid(s, mid + Vector3.up * 2f, new Vector3(Mathf.Abs(d.x) + 0.06f, 4f, Mathf.Abs(d.z) + 0.06f), wood, "Barandilla_Barrera");   // solo la barrera invisible
            bar.GetComponent<Renderer>().enabled = false;
        }
        foreach (var g in gaps) { Seg(cur, Mathf.Clamp(g.a, -halfAlong, halfAlong)); cur = Mathf.Max(cur, Mathf.Clamp(g.b, -halfAlong, halfAlong)); }
        Seg(cur, halfAlong);
    }

    // PISTA: plataforma elevada (como las áreas) con ALFOMBRAS VOLADORAS como esterillas grandes.
    static Transform Pad(Transform root, Vector3 ground, Quaternion rot, string label, Vector3[] members, float height = ElevatedHeight, bool groundBelow = true)
    {
        Transform t = new GameObject($"Pista_Alfombras_{label}").transform; t.SetParent(root, false);
        t.position = ground; t.rotation = rot;
        Elevate(t, new Vector2(PadSize, PadSize), height, groundBelow);
        Solid(t, new Vector3(0f, -0.05f, 0f), new Vector3(PadSize, 0.14f, PadSize), new Color(0.86f, 0.82f, 0.74f), "Suelo_pista");   // SÓLIDO (antes sin collider: se caía uno al subir a la alfombra)
        CarpetPad pad = t.gameObject.AddComponent<CarpetPad>();
        pad.displayName = label; pad.members = members;
        var slots = new List<Vector3>();
        for (int r = 0; r < 2; r++) for (int c = 0; c < 4; c++) slots.Add(new Vector3(-4.5f + c * 3f, 0.08f, -2.6f + r * 5f));
        pad.slots = slots.ToArray();
        Color[] colors = { new Color(0.75f, 0.2f, 0.25f), new Color(0.2f, 0.35f, 0.7f), new Color(0.85f, 0.6f, 0.15f), new Color(0.3f, 0.6f, 0.4f), new Color(0.55f, 0.3f, 0.65f), new Color(0.15f, 0.55f, 0.6f) };
        for (int i = 0; i < 6; i++)   // 6 alfombras posadas; quedan 2 huecos para las que lleguen
        {
            Transform c = new GameObject($"Alfombra_{i + 1}").transform; c.SetParent(t, false);
            c.localPosition = slots[i];
            BoxCollider bc = c.gameObject.AddComponent<BoxCollider>(); bc.isTrigger = true; bc.center = new Vector3(0f, 0.6f, 0f); bc.size = new Vector3(2.3f, 1.2f, 3.4f);
            KitchenProps.C(c, Vector3.zero, new Vector3(2.2f, 0.05f, 3.2f), colors[i % colors.Length], 0f, "Tejido");
            KitchenProps.C(c, new Vector3(0f, 0.01f, 0f), new Vector3(1.6f, 0.05f, 2.6f), new Color(0.95f, 0.85f, 0.45f), 0f, "Dibujo");
            for (int e = -1; e <= 1; e += 2) KitchenProps.C(c, new Vector3(0f, 0.005f, e * 1.68f), new Vector3(2.1f, 0.02f, 0.16f), Color.white, 0f, "Flecos");
            FlyingCarpet fc = c.gameObject.AddComponent<FlyingCarpet>(); fc.pad = pad;
        }
        WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = $"Pista de alfombras · {label}"; wl.height = 3f;
        return t;
    }
}
