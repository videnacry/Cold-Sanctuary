using System.Collections.Generic;
using UnityEngine;
using El = MicroFloorElements;

/// <summary>
/// LA FORMA DE MICROCOSMOS DE UN PISO, VESTIDA (docs/microcosmos-suelo-y-elementos.md §Elementos): cada pieza de la copia gigante del
/// piso (×100) se queda EN SU SITIO (el paso meso ↔ micro es uno a uno: un cambio del piso se rastrea) y se REINTERPRETA por el
/// catálogo (<see cref="MicroFloorElements"/>) en terreno o en edificio, con detalle: la mesa, una montaña (contrafuertes de roca en
/// las patas, peñascos en el borde del altiplano); la silla, un nevado; el lavabo, un lago (con el grifo abierto, cascada y río); la
/// cajonera, un edificio de la era; la caja, una cueva o una fábrica; el zapato, una cueva o un mercado; las hornillas, volcanes; el
/// foco, el sol; la lámpara, un poste de luz con sus faroles; la alfombra, un desnivel con rampas (o un cúmulo si la historia las
/// apila); la estatua, un coloso de insecto-humano.
/// Todo va en «Forma_microcosmos» (hermano de la copia gigante: el piso no se rompe) con un <see cref="MicroElement"/> por pieza (la
/// prueba de sincronía los mira) y la geometría COMBINADA por material en pocas mallas (un piso de cientos de piezas sigue siendo una
/// docena de renderers). Sin colliders salvo el terreno que se pisa (las laderas y los contrafuertes): no estorba al NavMesh ni a la flora.
/// </summary>
internal static class MicroFloorDress
{
    // Las submallas (materiales) de la geometría combinada.
    enum M { Roca, Nieve, Agua, Lava, Vidrio, VidrioOscuro, Fachada, Tejado, Metal, Hierba, Arena, Hielo, Luz, Tierra, Toldo, Humo }
    static readonly Color[] Colors =
    {
        new Color(0.5f, 0.47f, 0.43f), new Color(0.95f, 0.96f, 0.98f), new Color(0.24f, 0.45f, 0.58f), new Color(1f, 0.42f, 0.1f),
        new Color(1f, 0.88f, 0.55f), new Color(0.2f, 0.26f, 0.32f), new Color(0.8f, 0.76f, 0.68f), new Color(0.55f, 0.28f, 0.2f),
        new Color(0.45f, 0.47f, 0.5f), new Color(0.38f, 0.55f, 0.28f), new Color(0.84f, 0.75f, 0.55f), new Color(0.78f, 0.9f, 0.97f),
        new Color(1f, 0.95f, 0.75f), new Color(0.45f, 0.35f, 0.24f), new Color(0.8f, 0.25f, 0.2f), new Color(0.55f, 0.55f, 0.55f),
    };
    static readonly bool[] Emissive = { false, false, false, true, true, false, false, false, false, false, false, false, true, false, false, false };

    // ── Geometría combinada ────────────────────────────────────────────────────────────────────────────────────────────
    sealed class Mesher
    {
        readonly Transform parent; readonly Material[] mats; readonly string name; readonly bool solid;
        readonly List<Vector3> v = new List<Vector3>(); readonly List<int>[] subs; int chunks;
        public Mesher(Transform parent, string name, Material[] mats, bool solid) { this.parent = parent; this.name = name; this.mats = mats; this.solid = solid; subs = new List<int>[mats.Length]; for (int i = 0; i < subs.Length; i++) subs[i] = new List<int>(); }
        void Room(int add) { if (v.Count + add > 60000) Flush(); }
        public void Box(Vector3 c, Vector3 size, Quaternion q, M m)
        {
            Room(24);
            Vector3 h = size * 0.5f;
            Vector3[] n = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
            foreach (Vector3 f in n)
            {
                Vector3 u = Mathf.Abs(f.y) > 0.5f ? Vector3.right : Vector3.up, w = Vector3.Cross(f, u);
                Vector3 fc = Vector3.Scale(f, h), uu = Vector3.Scale(u, h), ww = Vector3.Scale(w, h);
                int b = v.Count;
                v.Add(c + q * (fc - uu - ww)); v.Add(c + q * (fc + uu - ww)); v.Add(c + q * (fc + uu + ww)); v.Add(c + q * (fc - uu + ww));
                var s = subs[(int)m]; s.Add(b); s.Add(b + 1); s.Add(b + 2); s.Add(b); s.Add(b + 2); s.Add(b + 3);
            }
        }
        public void Box(Vector3 c, Vector3 size, M m) => Box(c, size, Quaternion.identity, m);
        /// <summary>Un tronco de cono facetado (base en <paramref name="c"/>), con jitter en los radios: laderas, cráteres, chimeneas.</summary>
        public void Frustum(Vector3 c, float r0, float r1, float h, int sides, M m, System.Random rnd = null, float jitter = 0f, M? top = null)
        {
            Room(sides * 12 + 2);
            float J() => rnd == null ? 1f : 1f + ((float)rnd.NextDouble() * 2f - 1f) * jitter;
            var lo = new Vector3[sides]; var hi = new Vector3[sides];
            for (int i = 0; i < sides; i++)
            {
                float a = i * Mathf.PI * 2f / sides; Vector3 d = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                lo[i] = c + d * r0 * J(); hi[i] = c + Vector3.up * h + d * r1 * J();
            }
            var s = subs[(int)m];
            for (int i = 0; i < sides; i++)
            {
                int j = (i + 1) % sides, b = v.Count;
                v.Add(lo[i]); v.Add(hi[i]); v.Add(hi[j]); v.Add(lo[j]);
                s.Add(b); s.Add(b + 1); s.Add(b + 2); s.Add(b); s.Add(b + 2); s.Add(b + 3);
            }
            if (r1 > 0.01f)
            {
                var st = subs[(int)(top ?? m)]; int cIdx = v.Count; v.Add(c + Vector3.up * h);
                for (int i = 0; i < sides; i++) { int j = (i + 1) % sides, b = v.Count; v.Add(hi[i]); v.Add(hi[j]); st.Add(cIdx); st.Add(b + 1); st.Add(b); }
            }
        }
        public void Flush()
        {
            if (v.Count == 0) return;
            var mesh = new Mesh { name = $"{name}_{chunks}" };
            mesh.SetVertices(v); mesh.subMeshCount = subs.Length;
            for (int i = 0; i < subs.Length; i++) mesh.SetTriangles(subs[i], i);
            // Normales planas aproximadas: se separan las caras (cada cara ya trae sus vértices propios en las cajas).
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            var g = new GameObject(mesh.name); g.transform.SetParent(parent, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            g.AddComponent<MeshRenderer>().sharedMaterials = mats;
            if (solid) g.AddComponent<MeshCollider>().sharedMesh = mesh;
            v.Clear(); foreach (var s in subs) s.Clear(); chunks++;
        }
        public int Chunks => chunks;
    }

    static Material[] Mats()
    {
        var l = new Material[Colors.Length];
        for (int i = 0; i < l.Length; i++)
        {
            Material m = AreaMicrocosmos.Mat("MicroForma_" + (M)i, Colors[i]);
            if (Emissive[i] && m.HasProperty("_EmissionColor")) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", Colors[i] * 2.2f); m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive; }
            l[i] = m;
        }
        return l;
    }

    /// <summary>
    /// VISTE la forma de microcosmos del piso <paramref name="giant"/> bajo <paramref name="root"/>: una entrada por cada pieza de
    /// <paramref name="inventory"/> (el inventario de la copia gigante, tomado ANTES de recortarla con el borde: lo recortado se apunta
    /// como «fuera»), con la versión de la era <paramref name="year"/> y los cambios de la historia <paramref name="display"/>. Devuelve
    /// (con fila, sin fila).
    /// </summary>
    internal static (int mapped, int unmapped) Dress(Transform root, Transform giant, List<El.Piece> inventory, string display, int year, string story)
    {
        Transform form = new GameObject(FloorForms.MicroName).transform; form.SetParent(root, false);
        Transform recs = new GameObject("Elementos").transform; recs.SetParent(form, false);
        Material[] mats = Mats();
        var terrain = new Mesher(form, "Terreno_microcosmos", mats, true);     // lo que se pisa (laderas, contrafuertes, rampas)
        var deco = new Mesher(form, "Detalle_microcosmos", mats, false);       // lo que se ve (ventanas, nieve, agua, lava, toldos)
        var rnd = new System.Random(display.GetHashCode());
        int mapped = 0, unmapped = 0, suns = 0;
        bool town = MicroBuildRules.IsTownEra(year);
        var stacked = new List<Bounds>();
        foreach (El.Piece p in inventory)
        {
            El.Element e = p.element ?? El.Other;
            El.Override ov = El.OverrideFor(story ?? display, p.key);
            if (ov != null && ov.action == El.Action.Como && El.ById(ov.element) != null) e = El.ById(ov.element);
            var rec = new GameObject($"{e.id}__{p.key}").AddComponent<MicroElement>();
            rec.transform.SetParent(recs, false);
            rec.key = p.key; rec.element = e.id; rec.micro = e.MicroFor(year); rec.mesoPath = p.path; rec.storyOverride = ov != null ? $"{ov.action}: {ov.why}" : "";
            if (p.element == null) unmapped++; else mapped++;
            if (p.t == null) { rec.outside = true; continue; }   // recortada con el borde del piso
            if (!Bounds(p.t, out Bounds b)) continue;
            rec.transform.position = new Vector3(b.center.x, b.min.y, b.center.z);
            if (ov != null && ov.action == El.Action.Quitar) { foreach (Renderer r in p.t.GetComponentsInChildren<Renderer>(true)) r.enabled = false; foreach (Collider c in p.t.GetComponentsInChildren<Collider>(true)) c.enabled = false; continue; }
            if (ov != null && ov.action == El.Action.Mover) { p.t.position += new Vector3(ov.move.x, 0f, ov.move.y) * AreaMicrocosmos.AreaScale; Bounds(p.t, out b); }
            if (ov != null && ov.action == El.Action.Apilar && e.id == "alfombra") { stacked.Add(b); continue; }
            try { DressOne(e, p.t, b, terrain, deco, rnd, year, town, form, ref suns); }
            catch (System.Exception ex) { Debug.LogWarning($"[Forma microcosmos] {display}: no se pudo vestir «{p.path}» como {e.id} ({ex.Message})."); }
        }
        if (stacked.Count > 0) Mound(stacked, terrain, deco, rnd);
        terrain.Flush(); deco.Flush();
        Debug.Log($"[Forma microcosmos] {display} ({Eras.Format(year)}): {inventory.Count} piezas del piso reinterpretadas en su versión de microcosmos ({mapped} con fila del catálogo, {unmapped} como peñascos; {terrain.Chunks + deco.Chunks} mallas combinadas, {suns} soles){(stacked.Count > 0 ? $"; {stacked.Count} alfombras apiladas en un cúmulo" : "")}.");
        return (mapped, unmapped);
    }

    static bool Bounds(Transform t, out Bounds b)
    {
        b = default; bool any = false;
        foreach (Renderer r in t.GetComponentsInChildren<Renderer>(true)) { if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
        return any;
    }

    static float R(System.Random rnd, float a, float b) => a + (float)rnd.NextDouble() * (b - a);

    static void DressOne(El.Element e, Transform piece, Bounds b, Mesher terrain, Mesher deco, System.Random rnd, int year, bool town, Transform form, ref int suns)
    {
        Vector3 s = b.size, foot = new Vector3(b.center.x, b.min.y, b.center.z);
        float big = Mathf.Max(s.x, s.z), small = Mathf.Min(s.x, s.z);
        switch (e.id)
        {
            case "mesa": Mountain(piece, b, terrain, deco, rnd, false); break;
            case "silla": Mountain(piece, b, terrain, deco, rnd, true); break;
            case "cama":
                for (int k = 0; k < 4; k++) terrain.Frustum(new Vector3(R(rnd, b.min.x + small * 0.2f, b.max.x - small * 0.2f), b.max.y, R(rnd, b.min.z + small * 0.2f, b.max.z - small * 0.2f)), small * R(rnd, 0.15f, 0.25f), small * 0.04f, small * R(rnd, 0.04f, 0.08f), 9, M.Hierba, rnd, 0.15f);
                Outcrop(foot, big, terrain, rnd, 3); break;
            case "cojin":
                for (int k = 0; k < 3; k++) terrain.Frustum(new Vector3(R(rnd, b.min.x, b.max.x), b.max.y - s.y * 0.1f, R(rnd, b.min.z, b.max.z)), small * 0.3f, small * 0.05f, s.y * 0.25f, 8, M.Arena, rnd, 0.2f); break;
            case "lavabo": Lake(piece, b, deco, terrain, rnd); break;
            case "banera": deco.Box(new Vector3(b.center.x, b.min.y + s.y * 0.75f, b.center.z), new Vector3(s.x * 0.85f, 0.4f, s.z * 0.85f), M.Agua); Shore(b, b.min.y + s.y * 0.75f, deco); break;
            case "olla":
                terrain.Frustum(foot, big * 0.62f, big * 0.48f, s.y * 1.02f, 10, M.Roca, rnd, 0.08f, M.Agua); break;
            case "hornilla": Volcanoes(piece, b, terrain, deco, rnd); break;
            case "cajonera": Building(b, deco, rnd, year); break;
            case "botella": Tower(b, deco); break;
            case "maquina": Factory(b, deco, terrain, rnd, true); break;
            case "caja": if (year >= 1750) Factory(b, deco, terrain, rnd, false); else Cave(b, deco, terrain, rnd); break;
            case "zapato": if (town) Market(b, deco, rnd); else Cave(b, deco, terrain, rnd); break;
            case "bombilla": Sun(b, deco, form); suns++; break;
            case "lampara": StreetLights(b, deco, rnd); break;
            case "poste":
                deco.Box(new Vector3(b.center.x, b.max.y + 0.5f, b.center.z), new Vector3(1f, 1f, 1f), M.Lava);
                for (int k = 0; k < 6; k++) deco.Box(new Vector3(b.center.x, b.min.y + s.y * (k + 0.5f) / 6f, b.center.z), new Vector3(small * 1.4f + 0.5f, 0.2f, small * 1.4f + 0.5f), Quaternion.Euler(0f, k * 30f, 0f), M.Metal);
                break;
            case "carro":
                deco.Box(new Vector3(b.center.x, b.max.y + 1.5f, b.center.z), new Vector3(s.x * 1.1f, 0.4f, s.z * 1.1f), M.Tejado);
                for (int k = -1; k <= 1; k += 2) deco.Box(new Vector3(b.center.x + k * s.x * 0.5f, b.min.y + 0.4f, b.center.z), new Vector3(1.2f, 0.8f, s.z), M.Fachada);
                break;
            case "jaula": Battlements(b, deco); break;
            case "planta": Forest(b, deco, rnd); break;
            case "espejo": deco.Box(new Vector3(b.center.x, b.center.y, b.center.z), s * 1.02f + new Vector3(0.5f, 0.5f, 0.5f), M.Hielo); break;
            case "libro": for (int k = 0; k < 6; k++) deco.Box(new Vector3(b.center.x, b.min.y + s.y * (k + 0.5f) / 6f, b.center.z), new Vector3(s.x + 0.3f, s.y / 6f * 0.5f, s.z + 0.3f), k % 2 == 0 ? M.Arena : M.Roca); break;
            case "pared": Cliff(b, terrain, rnd); break;
            case "columna": terrain.Frustum(foot, small * 0.9f + 1f, small * 0.55f, s.y * 0.3f, 7, M.Roca, rnd, 0.15f); break;
            case "escalera": for (int k = 0; k < 4; k++) deco.Box(new Vector3(b.center.x, b.min.y + s.y * (k + 1) / 4f, b.center.z), new Vector3(s.x * (1f - k * 0.18f), 0.3f, s.z * (1f - k * 0.18f)), M.Hierba); break;
            case "alfombra": Ramps(b, town ? deco : terrain, deco); break;   // (en un pueblo, sin collider: la tierra ya sube y baja con la alfombra)
            case "estatua": Colossus(piece, b, form); break;
            case "suelo": break;   // la llanura: la capa de tierra del microcosmos (MicroSoilBuilder)
            default: Outcrop(foot, big, terrain, rnd, 4); break;
        }
    }

    // MONTAÑA o NEVADO: contrafuertes de roca en las patas (lo largo y fino que baja al suelo), peñascos en el borde del altiplano y, en
    // un nevado, nieve en lo más alto (el respaldo) y en las cumbres de los contrafuertes.
    static void Mountain(Transform piece, Bounds b, Mesher terrain, Mesher deco, System.Random rnd, bool snow)
    {
        int legs = 0;
        foreach (Renderer r in piece.GetComponentsInChildren<Renderer>(true))
        {
            Bounds lb = r.bounds; float w = Mathf.Max(lb.size.x, lb.size.z);
            if (lb.size.y < 2.5f * w || lb.min.y > b.min.y + 1f) continue;   // una pata: alta, fina y que llega al suelo
            Vector3 c = new Vector3(lb.center.x, lb.min.y, lb.center.z);
            float h = lb.size.y * R(rnd, 0.35f, 0.5f), r0 = w * 0.5f + Mathf.Min(3f, lb.size.y * 0.1f);   // (pegado a la pata: los solares del pueblo guardan 2 m con el mueble)
            terrain.Frustum(c, r0, w * 0.55f, h, 7, M.Roca, rnd, 0.2f);
            if (snow) deco.Frustum(c + Vector3.up * h * 0.82f, w * 0.62f + 0.3f, w * 0.56f, h * 0.2f, 7, M.Nieve);
            legs++;
            if (legs >= 8) break;
        }
        if (legs == 0) terrain.Frustum(new Vector3(b.center.x, b.min.y, b.center.z), Mathf.Max(b.extents.x, b.extents.z) + b.size.y * 0.15f, Mathf.Min(b.extents.x, b.extents.z) * 0.9f, b.size.y * 0.35f, 9, M.Roca, rnd, 0.12f);
        // Peñascos en el borde del altiplano (el centro queda libre: se puebla).
        Bounds top = b; float yTop = TopOf(piece, b);
        for (int k = 0; k < 6; k++)
        {
            bool alongX = k % 2 == 0; float t = R(rnd, -0.45f, 0.45f), sz = Mathf.Min(top.size.x, top.size.z) * R(rnd, 0.04f, 0.08f);
            Vector3 p = alongX ? new Vector3(top.center.x + t * top.size.x, yTop, k < 3 ? top.min.z + sz : top.max.z - sz) : new Vector3(k < 3 ? top.min.x + sz : top.max.x - sz, yTop, top.center.z + t * top.size.z);
            terrain.Frustum(p, sz, sz * 0.4f, sz * 0.9f, 6, M.Roca, rnd, 0.25f);
        }
        if (snow)
        {   // la cumbre nevada: lo más alto (el respaldo de la silla) con su manto de nieve, y nieve en el borde del asiento
            Renderer highest = null; foreach (Renderer r in piece.GetComponentsInChildren<Renderer>(true)) if (highest == null || r.bounds.max.y > highest.bounds.max.y) highest = r;
            if (highest != null)
            {
                Bounds hb = highest.bounds;
                deco.Box(new Vector3(hb.center.x, hb.max.y + 0.3f, hb.center.z), new Vector3(hb.size.x + 0.6f, 0.8f, hb.size.z + 0.6f), M.Nieve);
                deco.Box(new Vector3(hb.center.x, hb.max.y - hb.size.y * 0.12f, hb.center.z), new Vector3(hb.size.x + 0.3f, hb.size.y * 0.25f, hb.size.z + 0.3f), M.Nieve);
            }
            for (int k = 0; k < 4; k++) deco.Box(new Vector3(k < 2 ? top.center.x : (k == 2 ? top.min.x : top.max.x), yTop + 0.2f, k < 2 ? (k == 0 ? top.min.z : top.max.z) : top.center.z), k < 2 ? new Vector3(top.size.x, 0.4f, 2f) : new Vector3(2f, 0.4f, top.size.z), M.Nieve);
        }
    }

    // La cara de arriba de un mueble: la del renderer más grande en planta (el tablero, el asiento).
    static float TopOf(Transform piece, Bounds b)
    {
        float y = b.max.y, best = 0f;
        foreach (Renderer r in piece.GetComponentsInChildren<Renderer>(true)) { float a = r.bounds.size.x * r.bounds.size.z; if (a > best) { best = a; y = r.bounds.max.y; } }
        return y;
    }

    static void Outcrop(Vector3 foot, float big, Mesher terrain, System.Random rnd, int n)
    {
        for (int k = 0; k < n; k++)
        {
            float a = R(rnd, 0f, Mathf.PI * 2f), d = big * R(rnd, 0.4f, 0.5f), sz = Mathf.Clamp(big * R(rnd, 0.06f, 0.12f), 1f, 4f);
            terrain.Frustum(foot + new Vector3(Mathf.Cos(a) * d, 0f, Mathf.Sin(a) * d), sz, sz * 0.35f, sz * 0.8f, 6, M.Roca, rnd, 0.3f);
        }
    }

    // LAGO en la cuenca (a 2/3 de su alto) con su orilla; si el grifo está abierto (un chorro, agua corriendo), CASCADA desde el
    // grifo y RÍO que baja por un costado y corre por el suelo.
    static void Lake(Transform piece, Bounds b, Mesher deco, Mesher terrain, System.Random rnd)
    {
        float y = b.min.y + b.size.y * 0.7f;
        deco.Box(new Vector3(b.center.x, y, b.center.z), new Vector3(b.size.x * 0.7f, 0.3f, b.size.z * 0.7f), M.Agua);
        Shore(b, y, deco);
        Transform tap = null; bool open = MicroFloorElements.Norm(piece.name).Contains("abiert");
        foreach (Transform c in piece.GetComponentsInChildren<Transform>(true))
        {
            string n = MicroFloorElements.Norm(c.name);
            if (n.Contains("grifo") || n.Contains("cano") || n.Contains("tap")) tap = c;
            if (n.Contains("chorro") || n.Contains("cascada")) open = true;
        }
        if (!open || tap == null) return;
        Vector3 tp = tap.position;
        deco.Box(new Vector3(tp.x, (tp.y + y) * 0.5f, tp.z), new Vector3(1.2f, Mathf.Max(0.5f, tp.y - y), 0.8f), M.Agua);   // la cascada
        // El río: por el costado más cercano al grifo, hasta el suelo, y luego por el suelo.
        Vector3 side = b.size.x < b.size.z ? new Vector3(Mathf.Sign(tp.x - b.center.x), 0f, 0f) : new Vector3(0f, 0f, Mathf.Sign(tp.z - b.center.z));
        if (side.sqrMagnitude < 0.5f) side = Vector3.right;
        Vector3 edge = b.center + Vector3.Scale(side, b.extents);
        deco.Box(new Vector3(edge.x, (y + b.min.y) * 0.5f, edge.z) + side * 0.6f, new Vector3(side.x != 0f ? 1f : 3f, y - b.min.y, side.z != 0f ? 1f : 3f), M.Agua);
        float len = Mathf.Max(b.size.x, b.size.z) * 1.2f;
        for (int k = 0; k < 6; k++)
        {
            Vector3 p = new Vector3(edge.x, b.min.y + 0.12f, edge.z) + side * (len * (k + 0.5f) / 6f) + Vector3.Cross(side, Vector3.up) * Mathf.Sin(k * 1.3f) * 4f;
            deco.Box(p, side.x != 0f ? new Vector3(len / 6f + 1f, 0.15f, 4f) : new Vector3(4f, 0.15f, len / 6f + 1f), M.Agua);
            deco.Box(p + Vector3.Cross(side, Vector3.up) * 2.8f + Vector3.up * 0.1f, side.x != 0f ? new Vector3(len / 6f + 1f, 0.3f, 1.4f) : new Vector3(1.4f, 0.3f, len / 6f + 1f), M.Arena);
        }
    }

    static void Shore(Bounds b, float y, Mesher deco)
    {
        float w = Mathf.Max(0.5f, Mathf.Min(b.size.x, b.size.z) * 0.06f);
        deco.Box(new Vector3(b.center.x, y + 0.1f, b.min.z + b.size.z * 0.15f), new Vector3(b.size.x * 0.72f, 0.3f, w), M.Arena);
        deco.Box(new Vector3(b.center.x, y + 0.1f, b.max.z - b.size.z * 0.15f), new Vector3(b.size.x * 0.72f, 0.3f, w), M.Arena);
        deco.Box(new Vector3(b.min.x + b.size.x * 0.15f, y + 0.1f, b.center.z), new Vector3(w, 0.3f, b.size.z * 0.72f), M.Arena);
        deco.Box(new Vector3(b.max.x - b.size.x * 0.15f, y + 0.1f, b.center.z), new Vector3(w, 0.3f, b.size.z * 0.72f), M.Arena);
    }

    // VOLCANES: uno por fuego (quemador, aro, hornilla, fuego) o, si no se distinguen, cuatro en las esquinas de la placa.
    static void Volcanoes(Transform piece, Bounds b, Mesher terrain, Mesher deco, System.Random rnd)
    {
        var fires = new List<Bounds>();
        foreach (Renderer r in piece.GetComponentsInChildren<Renderer>(true))
        {
            string n = MicroFloorElements.Norm(r.name);
            if (n.Contains("quemador") || n.Contains("aro") || n.Contains("fuego") || n.Contains("hornilla") || n.Contains("brasa") || n.Contains("burner")) fires.Add(r.bounds);
        }
        if (fires.Count == 0)
        {
            float qx = b.size.x * 0.25f, qz = b.size.z * 0.25f, w = Mathf.Min(b.size.x, b.size.z) * 0.3f;
            for (int k = 0; k < 4; k++) fires.Add(new Bounds(new Vector3(b.center.x + (k % 2 == 0 ? -qx : qx), b.max.y, b.center.z + (k < 2 ? -qz : qz)), new Vector3(w, 0.1f, w)));
        }
        foreach (Bounds f in fires)
        {
            float w = Mathf.Max(f.size.x, f.size.z), h = w * R(rnd, 0.45f, 0.7f);
            Vector3 c = new Vector3(f.center.x, f.max.y, f.center.z);
            terrain.Frustum(c, w * 0.6f, w * 0.16f, h, 9, M.Roca, rnd, 0.12f, M.Lava);
            deco.Frustum(c + Vector3.up * h * 0.97f, w * 0.15f, w * 0.12f, h * 0.05f, 9, M.Lava);
            for (int k = 0; k < 3; k++) deco.Box(c + Vector3.up * (h + w * (0.2f + k * 0.25f)), Vector3.one * w * (0.12f + k * 0.06f), Quaternion.Euler(0f, k * 25f, 0f), M.Humo);   // la fumarola
        }
    }

    // EDIFICIO de la era: una planta por cada ~3,2 m con su banda de ventanas en las cuatro fachadas (encendidas o no), pilastras,
    // la puerta y la azotea (depósito y antena desde 1900; cornisa y tejado antes).
    static void Building(Bounds b, Mesher deco, System.Random rnd, int year)
    {
        int floors = Mathf.Clamp(Mathf.FloorToInt(b.size.y / 3.2f), 1, 120);
        float fh = b.size.y / floors;
        M facade = year < 1850 ? M.Fachada : year < 1950 ? M.Tejado : M.Metal;
        for (int f = 0; f < floors; f++)
        {
            float y = b.min.y + (f + 0.55f) * fh;
            M glass = (f * 7 + floors) % 3 == 0 ? M.VidrioOscuro : M.Vidrio;
            float gh = year < 1950 ? fh * 0.4f : fh * 0.6f;
            deco.Box(new Vector3(b.center.x, y, b.min.z - 0.06f), new Vector3(b.size.x * 0.92f, gh, 0.1f), glass);
            deco.Box(new Vector3(b.center.x, y, b.max.z + 0.06f), new Vector3(b.size.x * 0.92f, gh, 0.1f), glass);
            deco.Box(new Vector3(b.min.x - 0.06f, y, b.center.z), new Vector3(0.1f, gh, b.size.z * 0.92f), glass);
            deco.Box(new Vector3(b.max.x + 0.06f, y, b.center.z), new Vector3(0.1f, gh, b.size.z * 0.92f), glass);
        }
        int piers = Mathf.Clamp(Mathf.RoundToInt(b.size.x / 12f), 2, 12);
        for (int k = 0; k <= piers; k++)
        {
            float x = b.min.x + b.size.x * k / piers;
            deco.Box(new Vector3(x, b.center.y, b.min.z - 0.15f), new Vector3(0.5f, b.size.y, 0.3f), facade);
            deco.Box(new Vector3(x, b.center.y, b.max.z + 0.15f), new Vector3(0.5f, b.size.y, 0.3f), facade);
        }
        deco.Box(new Vector3(b.center.x, b.min.y + 1.4f, b.min.z - 0.2f), new Vector3(2.4f, 2.8f, 0.2f), M.VidrioOscuro);   // la puerta
        if (year >= 1900)
        {
            deco.Frustum(new Vector3(b.center.x + b.size.x * 0.25f, b.max.y, b.center.z), 2f, 2f, 4f, 8, M.Metal, null, 0f);   // el depósito de agua
            deco.Box(new Vector3(b.center.x - b.size.x * 0.3f, b.max.y + 6f, b.center.z), new Vector3(0.3f, 12f, 0.3f), M.Metal);   // la antena
        }
        else deco.Box(new Vector3(b.center.x, b.max.y + 0.4f, b.center.z), new Vector3(b.size.x + 1f, 0.8f, b.size.z + 1f), M.Tejado);   // la cornisa
    }

    static void Tower(Bounds b, Mesher deco)
    {
        int bands = Mathf.Clamp(Mathf.FloorToInt(b.size.y / 4f), 1, 60);
        for (int k = 0; k < bands; k++) deco.Frustum(new Vector3(b.center.x, b.min.y + (k + 0.4f) * b.size.y / bands, b.center.z), Mathf.Max(b.extents.x, b.extents.z) * 1.01f, Mathf.Max(b.extents.x, b.extents.z) * 1.01f, 1.2f, 10, k % 4 == 0 ? M.VidrioOscuro : M.Vidrio);
        deco.Frustum(new Vector3(b.center.x, b.max.y, b.center.z), 1.5f, 1.5f, 3f, 8, M.Luz);   // el faro
    }

    // FÁBRICA o CENTRAL: tejado en diente de sierra, dos o tres chimeneas con humo, tuberías por la fachada y el portón.
    static void Factory(Bounds b, Mesher deco, Mesher terrain, System.Random rnd, bool plant)
    {
        int teeth = Mathf.Clamp(Mathf.RoundToInt(b.size.x / 8f), 2, 20);
        for (int k = 0; k < teeth; k++)
            deco.Box(new Vector3(b.min.x + b.size.x * (k + 0.5f) / teeth, b.max.y + 1.5f, b.center.z), new Vector3(b.size.x / teeth * 0.95f, 0.4f, b.size.z * 0.9f), Quaternion.Euler(0f, 0f, 25f), k % 2 == 0 ? M.Tejado : M.Vidrio);
        int stacks = plant ? 3 : 2;
        for (int k = 0; k < stacks; k++)
        {
            Vector3 c = new Vector3(b.min.x + b.size.x * (k + 1f) / (stacks + 1f), b.max.y, b.center.z + b.size.z * 0.25f);
            float r = Mathf.Clamp(Mathf.Min(b.size.x, b.size.z) * 0.05f, 0.8f, 6f), h = Mathf.Clamp(b.size.y * 0.6f, 6f, 120f);
            deco.Frustum(c, r, r * 0.75f, h, 10, M.Tejado);
            for (int s = 0; s < 3; s++) deco.Box(c + Vector3.up * (h + r * (1f + s * 1.4f)), Vector3.one * r * (1.2f + s * 0.5f), Quaternion.Euler(0f, s * 30f, 0f), M.Humo);
        }
        for (int k = 0; k < 3; k++) deco.Box(new Vector3(b.center.x, b.min.y + b.size.y * (0.3f + k * 0.2f), b.min.z - 0.6f), new Vector3(b.size.x * 0.9f, 0.8f, 0.8f), M.Metal);   // las tuberías
        deco.Box(new Vector3(b.center.x, b.min.y + 2.5f, b.min.z - 0.1f), new Vector3(Mathf.Min(8f, b.size.x * 0.3f), 5f, 0.2f), M.VidrioOscuro);   // el portón
    }

    // CUEVA: la boca oscura en la cara más ancha, con su arco de roca y derrubios delante.
    static void Cave(Bounds b, Mesher deco, Mesher terrain, System.Random rnd)
    {
        bool front = b.size.x >= b.size.z;
        float w = (front ? b.size.x : b.size.z) * 0.35f, h = Mathf.Min(b.size.y * 0.55f, w * 0.9f);
        Vector3 c = front ? new Vector3(b.center.x, b.min.y + h * 0.5f, b.min.z - 0.15f) : new Vector3(b.min.x - 0.15f, b.min.y + h * 0.5f, b.center.z);
        deco.Box(c, front ? new Vector3(w, h, 0.3f) : new Vector3(0.3f, h, w), M.VidrioOscuro);
        deco.Box(c + Vector3.up * h * 0.55f, front ? new Vector3(w * 1.2f, h * 0.15f, 0.8f) : new Vector3(0.8f, h * 0.15f, w * 1.2f), M.Roca);
        Vector3 outward = front ? Vector3.back : Vector3.left;
        for (int k = 0; k < 6; k++)
        {
            float sz = w * R(rnd, 0.05f, 0.12f);
            terrain.Frustum(new Vector3(c.x, b.min.y, c.z) + outward * R(rnd, 1f, Mathf.Min(6f, w * 0.4f)) + Vector3.Cross(outward, Vector3.up) * R(rnd, -w * 0.6f, w * 0.6f), sz, sz * 0.3f, sz * 0.8f, 6, M.Roca, rnd, 0.3f);
        }
    }

    // MERCADO: puestos con toldo de colores (a la escala del pueblo: 3 m) alrededor de la pieza, con sus cajas de mercancía.
    static void Market(Bounds b, Mesher deco, System.Random rnd)
    {
        float per = 2f * (b.size.x + b.size.z); int n = Mathf.Clamp(Mathf.RoundToInt(per / 5f), 4, 60);
        for (int k = 0; k < n; k++)
        {
            float t = per * k / n; Vector3 p; Vector3 o;
            if (t < b.size.x) { p = new Vector3(b.min.x + t, b.min.y, b.min.z - 2.5f); o = Vector3.back; }
            else if (t < b.size.x + b.size.z) { p = new Vector3(b.max.x + 2.5f, b.min.y, b.min.z + t - b.size.x); o = Vector3.right; }
            else if (t < 2f * b.size.x + b.size.z) { p = new Vector3(b.max.x - (t - b.size.x - b.size.z), b.min.y, b.max.z + 2.5f); o = Vector3.forward; }
            else { p = new Vector3(b.min.x - 2.5f, b.min.y, b.max.z - (t - 2f * b.size.x - b.size.z)); o = Vector3.left; }
            deco.Box(p + Vector3.up * 0.45f, new Vector3(1.8f, 0.9f, 1.8f), M.Fachada);   // el mostrador
            deco.Box(p + Vector3.up * 2.9f, new Vector3(2.4f, 0.1f, 2.4f), Quaternion.Euler(o.z * 8f, 0f, -o.x * 8f), k % 2 == 0 ? M.Toldo : M.Vidrio);   // el toldo
            for (int s = 0; s < 2; s++) deco.Box(p + new Vector3(s == 0 ? -1.1f : 1.1f, 1.45f, 0f), new Vector3(0.1f, 2.9f, 0.1f), M.Metal);
            deco.Box(p + Vector3.up * 1.05f, new Vector3(0.5f, 0.3f, 0.4f), M.Hierba);   // la mercancía
        }
    }

    // EL SOL: una esfera que brilla (dos conos facetados) y una luz puntual de largo alcance.
    static void Sun(Bounds b, Mesher deco, Transform form)
    {
        float r = Mathf.Max(b.extents.x, Mathf.Max(b.extents.y, b.extents.z)) * 1.1f;
        Vector3 c = b.center;
        deco.Frustum(c, r, r * 0.6f, r * 0.7f, 14, M.Luz); deco.Frustum(c - Vector3.up * r * 0.7f, r * 0.6f, r, r * 0.7f, 14, M.Luz);
        var g = new GameObject("Sol_del_microcosmos"); g.transform.SetParent(form, false); g.transform.position = c;
        Light l = g.AddComponent<Light>(); l.type = LightType.Point; l.range = r * 40f; l.intensity = 2.5f; l.color = new Color(1f, 0.92f, 0.75f); l.shadows = LightShadows.None;
    }

    // POSTE DE LUZ gigante (su farol encendido arriba) y, alrededor de su pie, faroles a escala del pueblo (4 m).
    static void StreetLights(Bounds b, Mesher deco, System.Random rnd)
    {
        float top = b.max.y, w = Mathf.Max(b.size.x, b.size.z);
        deco.Box(new Vector3(b.center.x, top - b.size.y * 0.08f, b.center.z), new Vector3(w * 0.6f, b.size.y * 0.12f, w * 0.6f), M.Luz);
        int n = 10; float rr = w * 0.7f + 4f;
        for (int k = 0; k < n; k++)
        {
            float a = k * Mathf.PI * 2f / n; Vector3 p = new Vector3(b.center.x + Mathf.Cos(a) * rr, b.min.y, b.center.z + Mathf.Sin(a) * rr);
            deco.Box(p + Vector3.up * 2f, new Vector3(0.15f, 4f, 0.15f), M.Metal);
            deco.Box(p + Vector3.up * 4.15f, new Vector3(0.45f, 0.45f, 0.45f), M.Luz);
        }
    }

    static void Battlements(Bounds b, Mesher deco)
    {
        float y = b.max.y + 0.6f; int nx = Mathf.Clamp(Mathf.RoundToInt(b.size.x / 3f), 2, 40), nz = Mathf.Clamp(Mathf.RoundToInt(b.size.z / 3f), 2, 40);
        for (int k = 0; k < nx; k += 2) { float x = b.min.x + b.size.x * (k + 0.5f) / nx; deco.Box(new Vector3(x, y, b.min.z), new Vector3(b.size.x / nx, 1.2f, 0.8f), M.Fachada); deco.Box(new Vector3(x, y, b.max.z), new Vector3(b.size.x / nx, 1.2f, 0.8f), M.Fachada); }
        for (int k = 0; k < nz; k += 2) { float z = b.min.z + b.size.z * (k + 0.5f) / nz; deco.Box(new Vector3(b.min.x, y, z), new Vector3(0.8f, 1.2f, b.size.z / nz), M.Fachada); deco.Box(new Vector3(b.max.x, y, z), new Vector3(0.8f, 1.2f, b.size.z / nz), M.Fachada); }
    }

    static void Forest(Bounds b, Mesher deco, System.Random rnd)
    {
        int n = Mathf.Clamp(Mathf.RoundToInt(b.size.x * b.size.z / 60f), 3, 40);
        for (int k = 0; k < n; k++)
        {
            Vector3 p = new Vector3(R(rnd, b.min.x, b.max.x), b.max.y, R(rnd, b.min.z, b.max.z)); float h = R(rnd, 6f, 15f);
            deco.Box(p + Vector3.up * h * 0.25f, new Vector3(0.4f, h * 0.5f, 0.4f), M.Tejado);
            deco.Frustum(p + Vector3.up * h * 0.4f, h * 0.25f, 0.1f, h * 0.6f, 7, M.Hierba, rnd, 0.15f);
        }
    }

    static void Cliff(Bounds b, Mesher terrain, System.Random rnd)
    {
        bool alongX = b.size.x >= b.size.z; float len = alongX ? b.size.x : b.size.z; int n = Mathf.Clamp(Mathf.RoundToInt(len / 6f), 2, 30);
        for (int k = 0; k < n; k++)
        {
            float t = (k + 0.5f) / n;
            Vector3 p = alongX ? new Vector3(b.min.x + b.size.x * t, b.min.y, b.center.z) : new Vector3(b.center.x, b.min.y, b.min.z + b.size.z * t);
            float sz = Mathf.Clamp(len / n * R(rnd, 0.5f, 0.8f), 1f, 4f);
            terrain.Frustum(p, sz, sz * 0.3f, Mathf.Min(b.size.y * R(rnd, 0.2f, 0.45f), sz * 2.5f), 6, M.Roca, rnd, 0.3f);
        }
    }

    // DESNIVEL: la alfombra es una terraza; en sus dos lados largos, rampas suaves de tierra (se sube andando) y un borde de hierba.
    static void Ramps(Bounds b, Mesher terrain, Mesher deco)
    {
        float h = b.size.y; if (h < 0.15f) return;
        float run = Mathf.Max(1.5f, h / Mathf.Tan(18f * Mathf.Deg2Rad));
        bool longX = b.size.x >= b.size.z;
        for (int s = -1; s <= 1; s += 2)
        {
            float slope = Mathf.Sqrt(run * run + h * h), ang = Mathf.Atan2(h, run) * Mathf.Rad2Deg;
            if (longX)
            {
                Vector3 c = new Vector3(b.center.x, b.min.y + h * 0.5f, s < 0 ? b.min.z - run * 0.5f : b.max.z + run * 0.5f);
                terrain.Box(c, new Vector3(b.size.x * 0.9f, 0.3f, slope), Quaternion.Euler(s < 0 ? -ang : ang, 0f, 0f), M.Tierra);
                deco.Box(new Vector3(b.center.x, b.max.y + 0.05f, s < 0 ? b.min.z + 0.5f : b.max.z - 0.5f), new Vector3(b.size.x * 0.95f, 0.1f, 1f), M.Hierba);
            }
            else
            {
                Vector3 c = new Vector3(s < 0 ? b.min.x - run * 0.5f : b.max.x + run * 0.5f, b.min.y + h * 0.5f, b.center.z);
                terrain.Box(c, new Vector3(slope, 0.3f, b.size.z * 0.9f), Quaternion.Euler(0f, 0f, s < 0 ? ang : -ang), M.Tierra);
                deco.Box(new Vector3(s < 0 ? b.min.x + 0.5f : b.max.x - 0.5f, b.max.y + 0.05f, b.center.z), new Vector3(1f, 0.1f, b.size.z * 0.95f), M.Hierba);
            }
        }
    }

    // CÚMULO de alfombras apiladas (la historia lo pide): una loma de tierra y hierba sobre ellas, con capas de colores en la ladera.
    static void Mound(List<Bounds> carpets, Mesher terrain, Mesher deco, System.Random rnd)
    {
        Bounds all = carpets[0]; foreach (Bounds c in carpets) all.Encapsulate(c);
        float r = Mathf.Min(all.extents.x, all.extents.z), h = Mathf.Max(4f, carpets.Count * Mathf.Max(1.5f, all.size.y));
        Vector3 c0 = new Vector3(all.center.x, all.max.y, all.center.z);
        terrain.Frustum(c0, r, r * 0.25f, h, 12, M.Tierra, rnd, 0.1f, M.Hierba);
        for (int k = 0; k < Mathf.Min(carpets.Count, 6); k++) deco.Frustum(c0 + Vector3.up * h * k / (carpets.Count + 1f), r * (1f - 0.75f * k / (carpets.Count + 1f)) + 0.2f, r * (1f - 0.75f * (k + 0.3f) / (carpets.Count + 1f)) + 0.2f, h * 0.08f, 12, k % 2 == 0 ? M.Toldo : M.Vidrio);
    }

    // EL COLOSO: la estatua humana del piso se apaga (sigue contada: el piso no se rompe) y en su sitio se alza su versión de
    // microcosmos, un insecto-humano de piedra en la misma pose (de pie) y el mismo papel (su nombre), con su pedestal.
    static void Colossus(Transform piece, Bounds b, Transform form)
    {
        foreach (Renderer r in piece.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
        string role = piece.name.Replace("Estatua_", "").Replace("Estatua", "").Replace("_", " ").Trim();
        Transform holder = new GameObject($"Coloso_{role}").transform; holder.SetParent(form, false); holder.position = new Vector3(b.center.x, b.min.y, b.center.z);
        float ped = b.size.y * 0.18f;
        var p = GameObject.CreatePrimitive(PrimitiveType.Cube); p.name = "Pedestal"; p.transform.SetParent(holder, false);
        p.transform.localPosition = Vector3.up * ped * 0.5f; p.transform.localScale = new Vector3(b.size.x * 0.9f, ped, b.size.z * 0.9f);
        p.GetComponent<Renderer>().sharedMaterial = AreaMicrocosmos.Mat("Coloso_pedestal", new Color(0.62f, 0.6f, 0.56f));
        MicroStatue.InsectFigure(holder, Vector3.up * ped, piece.eulerAngles.y, b.size.y - ped, string.IsNullOrEmpty(role) ? "Estatua" : role, new Color(0.78f, 0.74f, 0.66f));
    }
}
