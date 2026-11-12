using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL RELIEVE DE LOS PUEBLOS, CON ASPECTO REAL (docs/microcosmos-pueblos-mapas.md §Relieve; Berón 2026-11-03: «¿un cono gris? ¿es
/// una montaña? entonces debería tener una apariencia realista y lo mismo con lagos, colinas, acantilados, etc.»). Mallas
/// LOW-POLY facetadas (cada triángulo con su normal: el estilo del juego), hechas a mano en el editor:
/// - MONTES Y COLINAS (<see cref="Peak"/>): una malla radial (anillos × radios) cuya altura es un perfil de montaña (pie ancho,
///   ladera, cumbre) con RUIDO (Perlin en dos octavas: crestas, collados, barrancos); el color por altura y pendiente en submallas:
///   pradera abajo, roca donde la ladera pasa de ~35°, canchal arriba y NIEVE por encima de la línea de nieve si el lugar la tiene
///   (los Alpes, Pikes Peak, el Rainier). Collider de malla: se sube andando por las laderas suaves.
/// - LAGOS (<see cref="Lake"/>): una orilla irregular (radio con ruido) con su agua y una franja de orilla (arena o hierba) un poco
///   más alta alrededor; el agua, un material liso y algo brillante.
/// - RÍOS (<see cref="RiverReach"/>): cada tramo, una cinta de agua de ancho variable con sus dos riberas (tierra), en trozos de
///   ≤ 60 m (los solares y la flora esquivan cada trozo).
/// - ACANTILADOS (<see cref="Cliff"/>): una pared de roca vertical facetada a lo largo de una línea, con su cornisa irregular.
/// Las alturas son las reales del lugar (desnivel sobre el pueblo), recortadas a 300 m (los muros del piso no pasan de 400 m).
/// </summary>
internal static partial class CivicBuilder
{
    static readonly Color Grass = C(0.42f, 0.55f, 0.3f), DryGrass = C(0.62f, 0.6f, 0.38f), RockC = C(0.5f, 0.47f, 0.43f), Scree = C(0.62f, 0.6f, 0.56f),
        SnowC = C(0.95f, 0.96f, 0.98f), WaterC = C(0.24f, 0.45f, 0.58f), BankC = C(0.55f, 0.47f, 0.34f), SandC = C(0.82f, 0.74f, 0.55f);

    static Material TerrainMat(string name, Color c, float smooth = 0.1f)
    {
        Material m = AreaMicrocosmos.Mat("Relieve_" + name, c);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
        return m;
    }

    // Una malla facetada: cada triángulo con sus 3 vértices propios (normales planas), repartido en submallas por material.
    sealed class Faceted
    {
        readonly List<Vector3> v = new List<Vector3>();
        readonly List<int>[] subs;
        public Faceted(int materials) { subs = new List<int>[materials]; for (int i = 0; i < materials; i++) subs[i] = new List<int>(); }
        public void Tri(Vector3 a, Vector3 b, Vector3 c, int sub)
        {
            if (Vector3.Cross(b - a, c - a).y < 0f) { Vector3 t = b; b = c; c = t; }   // hacia arriba (las paredes verticales: como vengan)
            int i = v.Count; v.Add(a); v.Add(b); v.Add(c); subs[sub].Add(i); subs[sub].Add(i + 1); subs[sub].Add(i + 2);
        }
        public void TriRaw(Vector3 a, Vector3 b, Vector3 c, int sub) { int i = v.Count; v.Add(a); v.Add(b); v.Add(c); subs[sub].Add(i); subs[sub].Add(i + 1); subs[sub].Add(i + 2); }
        public Mesh Build(string name)
        {
            var m = new Mesh { name = name };
            if (v.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.SetVertices(v); m.subMeshCount = subs.Length;
            for (int i = 0; i < subs.Length; i++) m.SetTriangles(subs[i], i);
            m.RecalculateNormals(); m.RecalculateBounds();
            return m;
        }
    }

    static GameObject MeshObject(Transform parent, string name, Mesh mesh, Material[] mats, bool collider)
    {
        var g = new GameObject(name); g.transform.SetParent(parent, false);
        g.AddComponent<MeshFilter>().sharedMesh = mesh;
        g.AddComponent<MeshRenderer>().sharedMaterials = mats;
        if (collider) g.AddComponent<MeshCollider>().sharedMesh = mesh;
        return g;
    }

    /// <summary>Un MONTE o una COLINA de radio <paramref name="r"/> y <paramref name="h"/> de desnivel, en <paramref name="c"/> (locales de
    /// <paramref name="parent"/>, el pie a ras de 0). <paramref name="snowLine"/>: altura (sobre el pie) desde la que hay nieve; ≤ 0, sin
    /// nieve.</summary>
    internal static GameObject Peak(Transform parent, string name, Vector2 c, float r, float h, float snowLine, int seed, bool dry = false)
    {
        h = Mathf.Min(h, 300f);
        int rings = Mathf.Clamp(Mathf.RoundToInt(r / 25f), 6, 14), spokes = Mathf.Clamp(Mathf.RoundToInt(r / 12f), 18, 40);
        float ox = (seed % 997) * 0.37f, oz = (seed % 431) * 0.53f;
        // Altura en (anillo, radio): perfil de montaña (cúpula que se abre en ladera) × ruido de crestas, y el pie a 0.
        var pts = new Vector3[rings + 1, spokes];
        for (int i = 0; i <= rings; i++)
            for (int k = 0; k < spokes; k++)
            {
                float t = i / (float)rings, a = k * Mathf.PI * 2f / spokes;
                float jag = 1f + 0.12f * (Mathf.PerlinNoise(ox + Mathf.Cos(a) * 2f, oz + Mathf.Sin(a) * 2f) - 0.5f) * 2f;   // el pie no es un círculo
                float rr = r * t * jag;
                Vector2 p = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * rr;
                float prof = Mathf.Pow(Mathf.Max(0f, 1f - t * t), 1.6f);
                float n1 = Mathf.PerlinNoise(ox + p.x / (r * 0.45f), oz + p.y / (r * 0.45f)), n2 = Mathf.PerlinNoise(ox * 2f + p.x / (r * 0.18f), oz * 2f + p.y / (r * 0.18f));
                float ridge = 1f - Mathf.Abs(n1 * 2f - 1f);   // crestas
                float y = h * prof * (0.7f + 0.45f * ridge + 0.2f * (n2 - 0.5f));
                if (i == rings) y = -0.5f;
                if (i == 0) y = h * (0.95f + 0.1f * n1);
                pts[i, k] = new Vector3(c.x + p.x, y, c.y + p.y);
            }
        // 0 pradera · 1 roca · 2 canchal · 3 nieve
        var f = new Faceted(4);
        int Sub(Vector3 a, Vector3 b, Vector3 d)
        {
            Vector3 n = Vector3.Cross(b - a, d - a).normalized; float slope = Vector3.Angle(n.y < 0f ? -n : n, Vector3.up);
            float y = (a.y + b.y + d.y) / 3f;
            if (snowLine > 0f && y > snowLine && slope < 50f) return 3;
            if (slope > 34f) return 1;
            if (y > h * 0.72f) return 2;
            return 0;
        }
        for (int i = 0; i < rings; i++)
            for (int k = 0; k < spokes; k++)
            {
                int k2 = (k + 1) % spokes;
                Vector3 a = pts[i, k], b = pts[i, k2], d = pts[i + 1, k], e = pts[i + 1, k2];
                if (i == 0) { f.Tri(a, d, e, Sub(a, d, e)); continue; }   // la cumbre: un abanico
                f.Tri(a, d, e, Sub(a, d, e)); f.Tri(a, e, b, Sub(a, e, b));
            }
        var mats = new[] { TerrainMat(dry ? "pradera_seca" : "pradera", dry ? DryGrass : Grass), TerrainMat("roca", RockC), TerrainMat("canchal", Scree), TerrainMat("nieve", SnowC, 0.35f) };
        return MeshObject(parent, name, f.Build(name), mats, true);
    }

    /// <summary>Un LAGO de semiejes <paramref name="rx"/> × <paramref name="rz"/> en <paramref name="c"/>: orilla irregular, agua y una franja
    /// de orilla alrededor (arena o hierba). Sin colisión (el agua no se pisa; la flora y los solares la esquivan).</summary>
    internal static GameObject Lake(Transform parent, string name, Vector2 c, float rx, float rz, int seed, bool sandy = false)
    {
        int n = Mathf.Clamp(Mathf.RoundToInt((rx + rz) / 6f), 16, 48);
        float o = (seed % 577) * 0.41f;
        var f = new Faceted(2);
        Vector3 Pt(float a, float s, float y)
        {
            float jag = 0.82f + 0.36f * Mathf.PerlinNoise(o + Mathf.Cos(a) * 1.7f, o + Mathf.Sin(a) * 1.7f);
            return new Vector3(c.x + Mathf.Cos(a) * rx * jag * s, y, c.y + Mathf.Sin(a) * rz * jag * s);
        }
        Vector3 mid = new Vector3(c.x, 0.03f, c.y);
        for (int k = 0; k < n; k++)
        {
            float a0 = k * Mathf.PI * 2f / n, a1 = (k + 1) * Mathf.PI * 2f / n;
            f.Tri(mid, Pt(a0, 1f, 0.03f), Pt(a1, 1f, 0.03f), 0);   // el agua
            Vector3 i0 = Pt(a0, 1f, 0.05f), i1 = Pt(a1, 1f, 0.05f), e0 = Pt(a0, 1.12f, 0.12f), e1 = Pt(a1, 1.12f, 0.12f);
            f.Tri(i0, e0, e1, 1); f.Tri(i0, e1, i1, 1);              // la orilla
        }
        var mats = new[] { TerrainMat("agua", WaterC, 0.85f), TerrainMat(sandy ? "arena" : "orilla", sandy ? SandC : Grass) };
        return MeshObject(parent, name, f.Build(name), mats, false);
    }

    /// <summary>Un TRAMO DE RÍO de <paramref name="a"/> a <paramref name="b"/> (locales), de ancho <paramref name="w"/>: cinta de agua de
    /// ancho variable con sus dos riberas de tierra. Sin colisión.</summary>
    internal static GameObject RiverReach(Transform parent, string name, Vector2 a, Vector2 b, float w, int seed)
    {
        Vector2 d = b - a; float len = d.magnitude; if (len < 0.5f) return null;
        d /= len; Vector2 nrm = new Vector2(-d.y, d.x);
        int steps = Mathf.Max(2, Mathf.CeilToInt(len / 8f));
        float o = (seed % 389) * 0.29f, bank = Mathf.Clamp(w * 0.18f, 1.5f, 12f);
        var f = new Faceted(2);
        Vector3 P(Vector2 q, float y) => new Vector3(q.x, y, q.y);
        for (int s = 0; s < steps; s++)
        {
            float t0 = s / (float)steps, t1 = (s + 1) / (float)steps;
            float w0 = w * (0.85f + 0.3f * Mathf.PerlinNoise(o + (a.x + d.x * t0 * len) * 0.02f, o + (a.y + d.y * t0 * len) * 0.02f));
            float w1 = w * (0.85f + 0.3f * Mathf.PerlinNoise(o + (a.x + d.x * t1 * len) * 0.02f, o + (a.y + d.y * t1 * len) * 0.02f));
            Vector2 c0 = a + d * t0 * len, c1 = a + d * t1 * len;
            Vector2 l0 = c0 + nrm * w0 * 0.5f, r0 = c0 - nrm * w0 * 0.5f, l1 = c1 + nrm * w1 * 0.5f, r1 = c1 - nrm * w1 * 0.5f;
            f.Tri(P(l0, 0.03f), P(r0, 0.03f), P(r1, 0.03f), 0); f.Tri(P(l0, 0.03f), P(r1, 0.03f), P(l1, 0.03f), 0);
            Vector2 L0 = l0 + nrm * bank, L1 = l1 + nrm * bank, R0 = r0 - nrm * bank, R1 = r1 - nrm * bank;
            f.Tri(P(l0, 0.04f), P(L0, 0.25f), P(L1, 0.25f), 1); f.Tri(P(l0, 0.04f), P(L1, 0.25f), P(l1, 0.04f), 1);
            f.Tri(P(r0, 0.04f), P(R1, 0.25f), P(R0, 0.25f), 1); f.Tri(P(r0, 0.04f), P(r1, 0.04f), P(R1, 0.25f), 1);
        }
        var mats = new[] { TerrainMat("agua", WaterC, 0.85f), TerrainMat("ribera", BankC) };
        return MeshObject(parent, name, f.Build(name), mats, false);
    }

    /// <summary>Un ACANTILADO a lo largo de <paramref name="pts"/> (locales), de <paramref name="h"/> m: pared de roca vertical facetada con
    /// su cornisa irregular (la cara mira a la izquierda de la línea). Collider de malla.</summary>
    internal static GameObject Cliff(Transform parent, string name, List<Vector2> pts, float h, int seed)
    {
        var f = new Faceted(2);
        float o = (seed % 211) * 0.61f;
        for (int i = 0; i + 1 < pts.Count; i++)
        {
            Vector2 a = pts[i], b = pts[i + 1]; float len = (b - a).magnitude; if (len < 1f) continue;
            Vector2 d = (b - a) / len, nrm = new Vector2(-d.y, d.x);
            int steps = Mathf.Max(2, Mathf.CeilToInt(len / 6f));
            for (int s = 0; s < steps; s++)
            {
                float t0 = s / (float)steps, t1 = (s + 1) / (float)steps;
                Vector2 p0 = a + d * t0 * len, p1 = a + d * t1 * len;
                float n0 = Mathf.PerlinNoise(o + p0.x * 0.05f, o + p0.y * 0.05f), n1 = Mathf.PerlinNoise(o + p1.x * 0.05f, o + p1.y * 0.05f);
                float h0 = h * (0.8f + 0.4f * n0), h1 = h * (0.8f + 0.4f * n1), j0 = (n0 - 0.5f) * 3f, j1 = (n1 - 0.5f) * 3f;
                Vector3 b0 = new Vector3(p0.x + nrm.x * j0, 0f, p0.y + nrm.y * j0), b1 = new Vector3(p1.x + nrm.x * j1, 0f, p1.y + nrm.y * j1);
                Vector3 m0 = b0 + Vector3.up * h0 * 0.5f + new Vector3(nrm.x, 0f, nrm.y) * (n1 - 0.5f) * 2f, m1 = b1 + Vector3.up * h1 * 0.5f;
                Vector3 t0v = b0 + Vector3.up * h0 - new Vector3(nrm.x, 0f, nrm.y) * 1.5f, t1v = b1 + Vector3.up * h1 - new Vector3(nrm.x, 0f, nrm.y) * 1.5f;
                f.TriRaw(b0, m0, b1, 0); f.TriRaw(m0, m1, b1, 0); f.TriRaw(m0, t0v, m1, 0); f.TriRaw(t0v, t1v, m1, 0);   // la cara
                Vector3 k0 = t0v - new Vector3(nrm.x, 0f, nrm.y) * 6f, k1 = t1v - new Vector3(nrm.x, 0f, nrm.y) * 6f;
                f.Tri(t0v, k0, k1, 1); f.Tri(t0v, k1, t1v, 1);                                                           // la cornisa (hierba)
            }
        }
        var mats = new[] { TerrainMat("roca_acantilado", C(0.55f, 0.5f, 0.44f)), TerrainMat("pradera", Grass) };
        GameObject g = MeshObject(parent, name, f.Build(name), mats, true);
        // (la cara la ven los dos lados: doble cara con un segundo material no hace falta; las normales planas la sombrean)
        return g;
    }
}
