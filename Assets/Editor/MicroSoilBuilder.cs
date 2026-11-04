using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA CAPA DE TIERRA DE LOS MICROCOSMOS (docs/microcosmos-suelo-y-elementos.md §Capa de tierra; Berón 2026-11-04: «colocar una capa
/// de superficie que tenga una altura de 10cm de alto o lo necesario para cubrir las superficies de los pisos que se usan … un suelo
/// de tierra, el cual tiene sus irregularidades, cúmulos y piedras»). Sobre cada suelo que usa el microcosmos:
/// - la TIERRA sigue la cara de arriba MÁS ALTA de los suelos del piso gigante en cada punto (el suelo, las alfombras, las losas del
///   hospital) + <see cref="MicroGround.SoilThickness"/>, con RUIDO (Perlin, ±4 cm, nunca por debajo de la mitad del grosor), CÚMULOS
///   (lomas suaves de 15–60 cm en el barrio de la historia, de 0,3–2 m en el resto del piso gigante) y PIEDRAS medio enterradas;
/// - donde está la historia y su pueblo (sus huellas) la tierra es LISA (sin cúmulos ni piedras: no tapan nada), y no se pone sobre el
///   agua de la historia (ríos, lagos);
/// - en las caras de arriba de los muebles gigantes, una PLACA de tierra que se afina hacia el borde;
/// - en MOSAICOS: una malla combinada por mosaico (≤ 64 × 64 celdas; tierra y piedras en dos submallas) con su MeshCollider: pocos
///   renderers aunque el piso mida kilómetros.
/// Luego <see cref="RaiseTown"/> sube a la tierra los edificios y tramos de calle que quedaran por debajo (sobre una alfombra).
/// </summary>
internal static class MicroSoilBuilder
{
    const int TileCells = 64;
    /// <summary>¿Se construye la capa de tierra? (MicroGround.SoilEnabled: apagada por defecto, 2026-11-04.)</summary>
    internal static bool Enabled => MicroGround.SoilEnabled;
    static float T => MicroGround.SoilThickness;

    /// <summary>Los suelos del piso gigante que hacen de «terreno» (los planos bajos apoyados en el suelo: el suelo, las alfombras, las
    /// losas): sus cajas. No las losas en alto (entreplantas) ni los muebles.</summary>
    internal static List<Bounds> GroundLayers(List<Renderer> floors, float groundY)
    {
        var l = new List<Bounds>();
        foreach (Renderer r in floors) if (r != null && r.enabled && r.bounds.min.y < groundY + MicroGround.Probe) l.Add(r.bounds);
        return l;
    }

    static float BaseAt(float x, float z, List<Bounds> layers, float groundY)
    {
        float y = groundY;
        foreach (Bounds b in layers) if (x >= b.min.x && x <= b.max.x && z >= b.min.z && z <= b.max.z && b.max.y > y) y = b.max.y;
        return y;
    }

    struct Mound { public Vector2 c; public float r, h; }

    // El terreno: la base (la cara más alta de los suelos del piso), el ruido, los cúmulos (en un HASH de celdas de 40 m: antes cada
    // vértice recorría los 600 cúmulos) y lo liso y el agua.
    sealed class Field
    {
        public List<Bounds> layers; public float groundY; public List<Bounds> flat, holes;
        public int count; public float ox, oz;
        const float Cell = 40f;
        readonly Dictionary<long, List<Mound>> _hash = new Dictionary<long, List<Mound>>();
        static long Key(int i, int k) => ((long)i << 32) ^ (uint)k;
        public void Add(Mound m)
        {
            int i0 = Mathf.FloorToInt((m.c.x - m.r) / Cell), i1 = Mathf.FloorToInt((m.c.x + m.r) / Cell), k0 = Mathf.FloorToInt((m.c.y - m.r) / Cell), k1 = Mathf.FloorToInt((m.c.y + m.r) / Cell);
            for (int i = i0; i <= i1; i++) for (int k = k0; k <= k1; k++) { long key = Key(i, k); if (!_hash.TryGetValue(key, out var l)) _hash[key] = l = new List<Mound>(); l.Add(m); }
            count++;
        }
        public float Noise(float x, float z) => 0.03f * (Mathf.PerlinNoise(ox + x * 0.13f, oz + z * 0.13f) - 0.5f) * 2f + 0.015f * (Mathf.PerlinNoise(ox + 31f + x * 0.6f, oz + 17f + z * 0.6f) - 0.5f) * 2f;
        public bool Flat(float x, float z) { foreach (Bounds b in flat) if (x > b.min.x && x < b.max.x && z > b.min.z && z < b.max.z) return true; return false; }
        public bool Hole(float x, float z) { foreach (Bounds b in holes) if (x > b.min.x && x < b.max.x && z > b.min.z && z < b.max.z) return true; return false; }
        public float Bump(float x, float z)
        {
            if (!_hash.TryGetValue(Key(Mathf.FloorToInt(x / Cell), Mathf.FloorToInt(z / Cell)), out var l)) return 0f;
            float bump = 0f;
            foreach (Mound m in l)
            {
                float dx = x - m.c.x, dz = z - m.c.y, d2 = dx * dx + dz * dz, r2 = m.r * m.r;
                if (d2 < r2) { float k = 1f - d2 / r2; bump += m.h * k * k; }
            }
            return bump;
        }
        /// <summary>La cara de la tierra que se VE (con ruido y cúmulos).</summary>
        public float Y(float x, float z)
        {
            float y = BaseAt(x, z, layers, groundY) + T;
            if (Flat(x, z)) return y;
            return y + Mathf.Max(Noise(x, z), -T * 0.5f) + Bump(x, z);
        }
        /// <summary>La cara del COLLIDER: lisa (sin ruido ni piedras), por debajo de la que se ve (la mitad del grosor), con los cúmulos
        /// (lomas suaves). Es la que hornea el NavMesh: con el ruido y las piedras en el collider, sus polígonos se multiplicaban.</summary>
        public float Yc(float x, float z)
        {
            float y = BaseAt(x, z, layers, groundY) + T * 0.5f;
            return Flat(x, z) ? y : y + Bump(x, z);
        }
    }

    /// <summary>Una rejilla de alturas (la de los mosaicos que se ven o la de su collider): celdas de sx × sz que caben justas en su zona.</summary>
    sealed class Grid
    {
        public float x0, z0, sx, sz; public int nx, nz; public float[] h; public bool skipFine; public Bounds fine; public Field f;
        public Grid(Bounds zone, float step, bool skipFine, Bounds fine, Field f, System.Func<float, float, float> y)
        {
            x0 = zone.min.x; z0 = zone.min.z; this.skipFine = skipFine; this.fine = fine; this.f = f;
            nx = Mathf.Max(1, Mathf.CeilToInt(zone.size.x / step)); nz = Mathf.Max(1, Mathf.CeilToInt(zone.size.z / step));
            sx = zone.size.x / nx; sz = zone.size.z / nz;
            h = new float[(nx + 1) * (nz + 1)];
            for (int i = 0; i <= nx; i++) for (int k = 0; k <= nz; k++) h[i * (nz + 1) + k] = y(x0 + i * sx, z0 + k * sz);
        }
        public float H(int i, int k) => h[i * (nz + 1) + k];
        public Vector3 V(int i, int k) => new Vector3(x0 + i * sx, H(i, k), z0 + k * sz);
        public bool InFine(float x, float z) => x > fine.min.x && x < fine.max.x && z > fine.min.z && z < fine.max.z;
        /// <summary>¿Lleva tierra la celda (i, k)? No si la tapa la otra zona ni sobre el agua.</summary>
        public bool Cell(int i, int k)
        {
            float mx = x0 + (i + 0.5f) * sx, mz = z0 + (k + 0.5f) * sz;
            return !(skipFine && InFine(mx, mz)) && !f.Hole(mx, mz);
        }
        /// <summary>La altura de la MALLA en (x, z), con su misma triangulación; false si ahí no hay tierra de esta rejilla.</summary>
        public bool Sample(float x, float z, out float y)
        {
            y = float.NaN;
            float u = (x - x0) / sx, w = (z - z0) / sz;
            if (u < 0f || w < 0f || u > nx || w > nz) return false;
            int i = Mathf.Min(nx - 1, Mathf.FloorToInt(u)), k = Mathf.Min(nz - 1, Mathf.FloorToInt(w));
            if (!Cell(i, k)) return false;
            float fu = u - i, fv = w - k, h00 = H(i, k), h11 = H(i + 1, k + 1);
            y = fu >= fv ? h00 + fu * (H(i + 1, k) - h00) + fv * (h11 - H(i + 1, k)) : h00 + fv * (H(i, k + 1) - h00) + fu * (h11 - H(i, k + 1));
            return true;
        }
    }

    /// <summary>
    /// Pone la capa de tierra bajo <paramref name="root"/>: sobre el piso <paramref name="rect"/> (fina en <paramref name="dense"/>, el
    /// barrio de la historia; más abierta en el resto) y en las caras de arriba <paramref name="tops"/>. <paramref name="flat"/>: lo que
    /// ocupa la historia y su pueblo (tierra lisa); <paramref name="holes"/>: el agua de la historia (sin tierra). Devuelve su
    /// <see cref="MicroSoil"/> y, en <paramref name="surface"/>, la altura de la tierra que SE VE en (x, z) (NaN fuera de ella), para
    /// apoyar las plantas encima (MicroGround.Settle).
    /// (2026-11-04, rendimiento) Lo que se ve (ruido, cúmulos, piedras) va en mallas SIN collider; el collider es otra malla más gruesa
    /// y LISA (la mitad del grosor + los cúmulos): con el ruido y las piedras en el collider, el NavMesh de la Cocina (4 × 4 km) no
    /// terminaba de hornearse.
    /// </summary>
    internal static MicroSoil Lay(Transform root, Bounds rect, Bounds dense, List<Renderer> floors, float groundY, List<Bounds> tops,
                                  List<Bounds> flat, List<Bounds> holes, Color soil, int seed, string label, out System.Func<float, float, float> surface)
    {
        Transform g = new GameObject(MicroGround.SoilName).transform; g.SetParent(root, false);
        MicroSoil ms = g.gameObject.AddComponent<MicroSoil>();
        var rnd = new System.Random(seed);
        float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
        var f = new Field { layers = GroundLayers(floors, groundY), groundY = groundY, flat = flat ?? new List<Bounds>(), holes = holes ?? new List<Bounds>(), ox = (seed & 1023) * 0.37f, oz = ((seed >> 10) & 1023) * 0.53f };
        Material earth = AreaMicrocosmos.Mat("Micro_Tierra_capa_" + label, Color.Lerp(soil, new Color(0.36f, 0.27f, 0.18f), 0.35f));
        Material rock = AreaMicrocosmos.Mat("Micro_Tierra_piedra", new Color(0.52f, 0.5f, 0.46f));
        var mats = new[] { earth, rock };

        // Las dos zonas: el barrio (fino) y el resto del piso (abierto). El paso sale del tamaño: ≤ ~240 celdas por lado en el barrio y
        // ≤ ~100 en el resto (un piso de 4 km: celdas de 40 m; sus cúmulos miden 8–20 m de radio y la malla los suaviza igual).
        Bounds inner = new Bounds(rect.center, new Vector3(rect.size.x, 0f, rect.size.z));
        Bounds fine = dense; { Vector3 mn = Vector3.Max(fine.min, inner.min), mx = Vector3.Min(fine.max, inner.max); fine.SetMinMax(new Vector3(mn.x, 0f, mn.z), new Vector3(Mathf.Max(mn.x, mx.x), 0f, Mathf.Max(mn.z, mx.z))); }
        float fineStep = Mathf.Max(2f, Mathf.Max(fine.size.x, fine.size.z) / 240f);
        float coarseStep = Mathf.Clamp(Mathf.Max(inner.size.x, inner.size.z) / 100f, 16f, 40f);
        float fineCol = Mathf.Max(6f, fineStep * 3f), coarseCol = coarseStep;

        void Mounds(Bounds zone, float density, float rMin, float rMax, float hMin, float hMax, int cap, bool skipFine)
        {
            int n = Mathf.Min(cap, Mathf.RoundToInt(zone.size.x * zone.size.z * density));
            for (int i = 0, tries = 0; i < n && tries < n * 4; tries++)
            {
                float x = R(zone.min.x, zone.max.x), z = R(zone.min.z, zone.max.z);
                if (skipFine && x > fine.min.x && x < fine.max.x && z > fine.min.z && z < fine.max.z) continue;
                float r = R(rMin, rMax);
                bool clear = true;
                foreach (Bounds b in f.flat) if (x > b.min.x - r && x < b.max.x + r && z > b.min.z - r && z < b.max.z + r) { clear = false; break; }
                if (!clear) continue;
                f.Add(new Mound { c = new Vector2(x, z), r = r, h = R(hMin, hMax) }); i++;
            }
        }
        Mounds(fine, 1f / 400f, 1.5f, 4f, 0.15f, 0.6f, 300, false);
        Mounds(inner, 1f / 6000f, 8f, 20f, 0.3f, 2f, 300, true);
        ms.mounds = f.count;

        // Lo que SE VE: la malla con ruido, cúmulos y piedras (sin collider), en mosaicos de TileCells × TileCells.
        Grid Visual(Bounds zone, float step, bool skipFine, string name, float stoneDensity, float sMin, float sMax)
        {
            if (zone.size.x < step || zone.size.z < step) return null;
            var gr = new Grid(zone, step, skipFine, fine, f, f.Y);
            for (int tx = 0; tx < gr.nx; tx += TileCells)
                for (int tz = 0; tz < gr.nz; tz += TileCells)
                {
                    int cx = Mathf.Min(TileCells, gr.nx - tx), cz = Mathf.Min(TileCells, gr.nz - tz);
                    var v = new List<Vector3>((cx + 1) * (cz + 1)); var tri = new List<int>(); var stoneTri = new List<int>();
                    for (int i = 0; i <= cx; i++) for (int k = 0; k <= cz; k++) v.Add(gr.V(tx + i, tz + k));
                    int cells = 0;
                    for (int i = 0; i < cx; i++)
                        for (int k = 0; k < cz; k++)
                        {
                            if (!gr.Cell(tx + i, tz + k)) continue;
                            int a = i * (cz + 1) + k, b = a + 1, c = (i + 1) * (cz + 1) + k + 1, e = (i + 1) * (cz + 1) + k;
                            tri.Add(a); tri.Add(b); tri.Add(c); tri.Add(a); tri.Add(c); tri.Add(e);
                            cells++;
                        }
                    if (cells == 0) continue;
                    float x0 = gr.x0 + tx * gr.sx, z0 = gr.z0 + tz * gr.sz, w = cx * gr.sx, d = cz * gr.sz;
                    int ns = Mathf.Min(200, Mathf.RoundToInt(w * d * stoneDensity));
                    for (int s = 0; s < ns; s++)
                    {
                        float x = x0 + R(0f, w), z = z0 + R(0f, d);
                        if (skipFine && gr.InFine(x, z)) continue;
                        if (f.Flat(x, z) || !gr.Sample(x, z, out float sy)) continue;
                        Stone(v, stoneTri, new Vector3(x, sy, z), R(sMin, sMax), R(0f, 360f), R(0.55f, 0.9f));
                        ms.stones++;
                    }
                    Tile(g, $"{name}_{tx / TileCells}_{tz / TileCells}", v, tri, stoneTri, mats, false);
                    ms.tiles++;
                }
            return gr;
        }
        // El COLLIDER: la misma zona, más gruesa y lisa (Yc), sin renderer.
        void Collision(Bounds zone, float step, bool skipFine, string name)
        {
            if (zone.size.x < step || zone.size.z < step) return;
            var gr = new Grid(zone, step, skipFine, fine, f, f.Yc);
            for (int tx = 0; tx < gr.nx; tx += TileCells)
                for (int tz = 0; tz < gr.nz; tz += TileCells)
                {
                    int cx = Mathf.Min(TileCells, gr.nx - tx), cz = Mathf.Min(TileCells, gr.nz - tz);
                    var v = new List<Vector3>((cx + 1) * (cz + 1)); var tri = new List<int>();
                    for (int i = 0; i <= cx; i++) for (int k = 0; k <= cz; k++) v.Add(gr.V(tx + i, tz + k));
                    for (int i = 0; i < cx; i++)
                        for (int k = 0; k < cz; k++)
                        {
                            if (!gr.Cell(tx + i, tz + k)) continue;
                            int a = i * (cz + 1) + k, b = a + 1, c = (i + 1) * (cz + 1) + k + 1, e = (i + 1) * (cz + 1) + k;
                            tri.Add(a); tri.Add(b); tri.Add(c); tri.Add(a); tri.Add(c); tri.Add(e);
                        }
                    if (tri.Count == 0) continue;
                    var m = new Mesh { name = $"{name}_{tx / TileCells}_{tz / TileCells}" };
                    m.SetVertices(v); m.SetTriangles(tri, 0); m.RecalculateBounds();
                    var go = new GameObject(m.name); go.transform.SetParent(g, false);
                    go.AddComponent<MeshCollider>().sharedMesh = m;
                    ms.colliders++;
                }
        }
        Grid gFine = Visual(fine, fineStep, false, "Tierra_barrio", 1f / 150f, 0.08f, 0.5f);
        Grid gCoarse = Visual(inner, coarseStep, true, "Tierra_piso", 1f / 3000f, 0.3f, 1.5f);
        Collision(fine, fineCol, false, "Colision_barrio");
        Collision(inner, coarseCol, true, "Colision_piso");
        surface = (x, z) =>
        {
            if (gFine != null && gFine.Sample(x, z, out float y)) return y;
            if (gCoarse != null && gCoarse.Sample(x, z, out y)) return y;
            return float.NaN;
        };

        // Las PLACAS sobre las caras de arriba de los muebles (se afinan hacia el borde: no asoma un canto de tierra). Pequeñas: su
        // collider es la propia placa, sin las piedras.
        if (tops != null)
            foreach (Bounds top in tops)
            {
                if (top.size.x < 3f || top.size.z < 3f) continue;
                float step = Mathf.Clamp(Mathf.Min(top.size.x, top.size.z) / 12f, 1f, 8f);
                int nx = Mathf.Clamp(Mathf.CeilToInt(top.size.x / step), 2, TileCells), nz = Mathf.Clamp(Mathf.CeilToInt(top.size.z / step), 2, TileCells);
                var v = new List<Vector3>(); var tri = new List<int>(); var stoneTri = new List<int>();
                float y0 = top.max.y, inset = 0.3f;
                for (int i = 0; i <= nx; i++)
                    for (int k = 0; k <= nz; k++)
                    {
                        float x = Mathf.Lerp(top.min.x + inset, top.max.x - inset, i / (float)nx), z = Mathf.Lerp(top.min.z + inset, top.max.z - inset, k / (float)nz);
                        bool edge = i == 0 || k == 0 || i == nx || k == nz;
                        v.Add(new Vector3(x, edge ? y0 + 0.01f : y0 + T + Mathf.Max(f.Noise(x, z), -T * 0.5f), z));
                    }
                for (int i = 0; i < nx; i++)
                    for (int k = 0; k < nz; k++)
                    {
                        int a = i * (nz + 1) + k, b = a + 1, c = (i + 1) * (nz + 1) + k + 1, e = (i + 1) * (nz + 1) + k;
                        tri.Add(a); tri.Add(b); tri.Add(c); tri.Add(a); tri.Add(c); tri.Add(e);
                    }
                int nv = v.Count;
                int ns = Mathf.Min(40, Mathf.RoundToInt(top.size.x * top.size.z / 120f));
                for (int s = 0; s < ns; s++)
                {
                    float x = R(top.min.x + 2f, top.max.x - 2f), z = R(top.min.z + 2f, top.max.z - 2f);
                    Stone(v, stoneTri, new Vector3(x, y0 + T, z), R(0.1f, 0.6f), R(0f, 360f), R(0.55f, 0.9f));
                    ms.stones++;
                }
                GameObject plate = Tile(g, $"Tierra_sobre_mueble_{ms.plates}", v, tri, stoneTri, mats, false);
                var cm = new Mesh { name = plate.name + "_colision" }; cm.SetVertices(v.GetRange(0, nv)); cm.SetTriangles(tri, 0); cm.RecalculateBounds();
                plate.AddComponent<MeshCollider>().sharedMesh = cm;
                ms.plates++;
            }
        Debug.Log($"[Tierra] {label}: capa de tierra de {T * 100f:0} cm en {ms.tiles} mosaicos (paso {fineStep:0.#} m en el barrio, {coarseStep:0} m en el resto), {ms.colliders} colliders lisos (paso {fineCol:0} / {coarseCol:0} m) y {ms.plates} placas sobre muebles; {ms.mounds} cúmulos y {ms.stones} piedras.");
        return ms;
    }

    // Una piedra baja de 8 caras (un octaedro aplastado y girado), medio enterrada en p.
    static void Stone(List<Vector3> v, List<int> tri, Vector3 p, float size, float yaw, float flat)
    {
        Quaternion q = Quaternion.Euler(0f, yaw, 0f);
        float hx = size * 0.5f, hz = size * 0.4f, hy = size * 0.5f * flat;
        Vector3 c = p + Vector3.up * (hy * 0.4f);
        int b = v.Count;
        v.Add(c + q * new Vector3(hx, 0f, 0f)); v.Add(c + q * new Vector3(0f, 0f, hz)); v.Add(c + q * new Vector3(-hx * 0.8f, 0f, 0f)); v.Add(c + q * new Vector3(0f, 0f, -hz * 1.1f));
        v.Add(c + Vector3.up * hy); v.Add(c - Vector3.up * hy);
        int[] faces = { 0, 4, 1, 1, 4, 2, 2, 4, 3, 3, 4, 0, 1, 5, 0, 2, 5, 1, 3, 5, 2, 0, 5, 3 };
        foreach (int i in faces) tri.Add(b + i);
    }

    static GameObject Tile(Transform parent, string name, List<Vector3> v, List<int> earthTri, List<int> stoneTri, Material[] mats, bool collider)
    {
        var m = new Mesh { name = name };
        if (v.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m.SetVertices(v);
        m.subMeshCount = 2;
        m.SetTriangles(earthTri, 0); m.SetTriangles(stoneTri, 1);
        m.RecalculateNormals(); m.RecalculateBounds();
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        go.AddComponent<MeshFilter>().sharedMesh = m;
        var mr = go.AddComponent<MeshRenderer>(); mr.sharedMaterials = mats;
        if (collider) go.AddComponent<MeshCollider>().sharedMesh = m;
        return go;
    }

    /// <summary>
    /// EL PUEBLO SOBRE LA TIERRA: cada edificio (AreaFunctionSite) y cada tramo de calle de <paramref name="story"/> que quede con su
    /// base por debajo de la tierra que tiene debajo (sobre una alfombra, una losa más alta) se sube a ella. Nunca se baja nada.
    /// </summary>
    internal static int RaiseTown(Transform story, List<Renderer> floors, float groundY)
    {
        var layers = GroundLayers(floors, groundY);
        int n = 0;
        var done = new List<Transform>();
        void Raise(Transform t, Bounds b)
        {
            float want = float.MinValue;
            foreach (Vector2 q in new[] { new Vector2(b.center.x, b.center.z), new Vector2(b.min.x, b.min.z), new Vector2(b.max.x, b.min.z), new Vector2(b.min.x, b.max.z), new Vector2(b.max.x, b.max.z) })
                want = Mathf.Max(want, BaseAt(q.x, q.y, layers, groundY) + T);
            float dy = want - b.min.y;
            if (dy > 0.02f) { t.position += Vector3.up * dy; n++; }
        }
        foreach (AreaFunctionSite site in story.GetComponentsInChildren<AreaFunctionSite>(true))
        {
            bool nested = false; foreach (Transform d in done) if (site.transform.IsChildOf(d)) { nested = true; break; }
            if (nested) continue;
            done.Add(site.transform);
            Bounds b = default; bool any = false;
            foreach (Renderer r in site.GetComponentsInChildren<Renderer>(true)) { if (r.GetComponentInParent<CubeInsect>(true) != null) continue; if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
            if (any) Raise(site.transform, b);
        }
        foreach (Renderer r in story.GetComponentsInChildren<Renderer>(true))
        {
            if (r.GetComponentInParent<AreaFunctionSite>(true) != null) continue;
            bool street = false; for (Transform p = r.transform; p != null && p != story; p = p.parent) if (p.name.StartsWith("Calle")) { street = true; break; }
            if (street) Raise(r.transform, r.bounds);
        }
        return n;
    }
}
