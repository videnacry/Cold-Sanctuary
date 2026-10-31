using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA FACHADA DE FOTOBIORREACTORES DEL ÁRBOL DE LOS CAMPOS (Berón 2026-10-31; docs/fotobiorreactores.md, medidas en
/// <see cref="FotobioSpec"/>): espirulina (Arthrospira, la de MELiSSA) en tubos de vidrio y paneles planos, fuera del agua del
/// tronco (dentro, el agua se pondría verde y taparía a los nadadores).
/// - TRONCO: arpas de 28 tubos horizontales (Ø 7 cm) en módulos de 3 × 4 m con sus colectores, a 1,4 m de la pared de acrílico,
///   en los vanos 0 y 2 de cada cara (lejos del carril de nado, las puertas y las bocas de las ramas), de 37,6 a 56 m.
///   Detrás de cada arpa, regletas LED (la noche polar); una bomba abajo y un desgasificador arriba de cada columna.
/// - RAMAS: una quilla de 6 paneles planos colgada bajo el tramo largo de cada rama (no tapa los lados del tubo).
/// - CUADRO de control al pie del tronco (cara a, lejos de la puerta del suelo) con la etiqueta del cultivo.
/// Cuelga del árbol (gira con él). Se llama después de <c>BuildTree</c>; lee la geometría de <see cref="TreeSpec.Helix"/>.
/// </summary>
public static partial class SampleSceneBuilder
{
    static readonly Color FotobioCulture = new Color(0.10f, 0.40f, 0.32f), FotobioFrame = new Color(0.78f, 0.80f, 0.82f),
        FotobioLedOff = new Color(0.22f, 0.22f, 0.25f), FotobioPump = new Color(0.25f, 0.45f, 0.62f);

    /// <summary>Monta la fachada en el árbol <paramref name="tree"/> (la raíz «Arbol_de_los_Campos»). Sin árbol, avisa y no hace nada.</summary>
    static void BuildFotobio(Transform tree)
    {
        if (tree == null) { Debug.LogWarning("[Fotobio] No hay Árbol de los Campos: sin fachada de fotobiorreactores."); return; }
        HelixTower h = TreeSpec.Helix;
        Transform root = new GameObject(FotobioSpec.RootName).transform;
        root.SetParent(tree, false);

        var tubes = new List<(Vector3 a, Vector3 b, float r)>();
        var culture = new List<(Vector3 c, Vector3 s, Quaternion q)>();
        var frame = new List<(Vector3 c, Vector3 s, Quaternion q)>();
        var leds = new List<(Vector3 c, Vector3 s, Quaternion q)>();
        var pumps = new List<(Vector3 c, Vector3 s, Quaternion q)>();
        float W = FotobioSpec.ModuleW, H = FotobioSpec.ModuleH, M = FotobioSpec.ManifoldH, z = FotobioSpec.PlaneZ(h), ribFace = h.OuterHalf + 0.6f;
        int modules = 0, panels = 0, branches = 0;

        // ── TRONCO: 4 caras × 2 vanos × 4 módulos ──
        for (int s = 0; s < 4; s++)
        {
            Quaternion q = HelixTower.SideRot(s);
            Vector3 P(float x, float y, float zz) => q * new Vector3(x, y, zz);
            void Box(List<(Vector3, Vector3, Quaternion)> l, float x, float y, float zz, Vector3 size) => l.Add((P(x, y, zz), size, q));
            foreach (Rect r in FotobioSpec.TrunkModules(h))
            {
                float xc = r.center.x, y0 = r.yMin;
                Box(frame, xc, y0 + M * 0.5f, z, new Vector3(W + 0.2f, M, 0.16f));            // colector de abajo
                Box(frame, xc, y0 + M + H + M * 0.5f, z, new Vector3(W + 0.2f, M, 0.16f));    // colector de arriba
                for (int i = 0; i < FotobioSpec.TubesPerModule; i++)
                {
                    float y = y0 + M + (i + 0.5f) * FotobioSpec.TubePitch;
                    tubes.Add((P(xc - W * 0.5f, y, z), P(xc + W * 0.5f, y, z), FotobioSpec.TubeD * 0.5f));
                }
                foreach (float sx in new[] { -1f, 1f })
                {
                    float px = xc + sx * (W * 0.5f + 0.06f);
                    Box(frame, px, y0 + r.height * 0.5f, z, new Vector3(0.08f, r.height, 0.1f));   // montante
                    foreach (float by in new[] { y0 + 0.5f, y0 + r.height - 0.5f })                   // ménsulas al nervio
                        Box(frame, px, by, (ribFace + z) * 0.5f, new Vector3(0.1f, 0.1f, z - ribFace));
                }
                for (int k = 0; k < 4; k++) Box(leds, xc, y0 + M + 0.5f + k, z - 0.22f, new Vector3(W, 0.05f, 0.04f));   // regletas LED detrás del arpa
                modules++;
            }
            foreach (int bay in FotobioSpec.TrunkBays)
            {
                float xc = FotobioSpec.BayCenter(h, bay);
                Box(pumps, xc, FotobioSpec.TrunkBottom - 0.4f, z, new Vector3(0.9f, 0.6f, 0.5f));                  // bomba (impulsa la columna)
                Box(pumps, xc + 1.0f, FotobioSpec.TrunkTop + 0.7f, z, new Vector3(0.35f, 1.2f, 0.35f));            // desgasificador (saca el O₂)
                Box(frame, xc + W * 0.5f + 0.2f, (FotobioSpec.TrunkBottom + FotobioSpec.TrunkTop) * 0.5f, z, new Vector3(0.1f, FotobioSpec.TrunkTop - FotobioSpec.TrunkBottom, 0.1f));   // tubería de subida
            }
        }

        // ── RAMAS: una quilla de paneles planos bajo el tramo largo de cada rama ──
        for (int k = 1; k <= TreeSpec.Platforms; k++)
        {
            var (lv, side, tip) = TreeSpec.Platform(k);
            if (!tip) continue;
            Quaternion q = HelixTower.SideRot(side);
            Vector3 P(float x, float y, float zz) => q * new Vector3(x, y, zz);
            var kl = FotobioSpec.Keel(h, lv);
            float ym = (kl.y0 + kl.y1) * 0.5f, chordBottom = h.LevelY(lv) - 0.3f - h.tubeWall - 0.45f - 0.45f;
            for (int p = 0; p < FotobioSpec.KeelPanels; p++)
            {
                float x0 = kl.x0 + p * (FotobioSpec.KeelPanelW + FotobioSpec.KeelGap), xc = x0 + FotobioSpec.KeelPanelW * 0.5f;
                culture.Add((P(xc, ym, kl.z), new Vector3(FotobioSpec.KeelPanelW, FotobioSpec.KeelPanelH, FotobioSpec.KeelThick), q));
                frame.Add((P(xc, kl.y1 + 0.04f, kl.z), new Vector3(FotobioSpec.KeelPanelW, 0.08f, 0.1f), q));   // perfil de arriba
                frame.Add((P(xc, kl.y0 - 0.04f, kl.z), new Vector3(FotobioSpec.KeelPanelW, 0.08f, 0.1f), q));   // y de abajo
                foreach (float sx in new[] { -0.9f, 0.9f })
                    frame.Add((P(xc + sx, (kl.y1 + chordBottom) * 0.5f, kl.z), new Vector3(0.08f, chordBottom - kl.y1, 0.08f), q));   // colgador
                frame.Add((P(xc, chordBottom - 0.06f, kl.z), new Vector3(0.12f, 0.12f, 2f * h.tubeHalf), q));   // travesaño bajo los cordones
                foreach (float sz in new[] { -0.28f, 0.28f }) leds.Add((P(xc, ym, kl.z + sz), new Vector3(FotobioSpec.KeelPanelW, 0.04f, 0.04f), q));   // regletas LED a los dos lados
                panels++;
            }
            pumps.Add((P(kl.x0 - 0.6f, kl.y1 - 0.4f, kl.z), new Vector3(0.6f, 0.6f, 0.5f), q));   // su bomba
            branches++;
        }

        // ── MALLAS (una por material) ──
        var cv = new List<Vector3>(); var ct = new List<int>();
        foreach (var (a, b, r) in tubes) FotobioTube(cv, ct, a, b, r);
        FotobioBoxes(cv, ct, culture);
        FotobioPiece(root, "Fotobio_cultivo", cv, ct, FotobioCulture);
        var fv = new List<Vector3>(); var ft = new List<int>(); FotobioBoxes(fv, ft, frame); FotobioPiece(root, "Fotobio_marco", fv, ft, FotobioFrame);
        var lv2 = new List<Vector3>(); var lt = new List<int>(); FotobioBoxes(lv2, lt, leds); FotobioPiece(root, "Fotobio_LED", lv2, lt, FotobioLedOff);
        var pv = new List<Vector3>(); var pt = new List<int>(); FotobioBoxes(pv, pt, pumps); FotobioPiece(root, "Fotobio_bombas", pv, pt, FotobioPump);

        // ── CUADRO DE CONTROL al pie (cara a, entre los nervios de −7,3 y −3,7 m: lejos de la puerta del suelo, en x = 5) ──
        Quaternion q0 = HelixTower.SideRot(TreeSpec.GroundSide);
        Transform panel = KitchenProps.C(root, HelixTower.FromSide(TreeSpec.GroundSide, new Vector3(-5.5f, 1.6f, h.OuterHalf + 0.2f)), new Vector3(1.2f, 1.6f, 0.4f), FotobioPump, q0, "Cuadro_fotobio");
        Label(panel, FotobioSpec.Title, 1.3f);

        FotobioFacade fb = root.gameObject.AddComponent<FotobioFacade>();
        fb.trunkModules = modules; fb.keelPanels = panels;
        fb.areaM2 = modules * FotobioSpec.ModuleArea + panels * FotobioSpec.KeelPanelArea;
        fb.liters = modules * FotobioSpec.ModuleArea * FotobioSpec.TubeLitersPerM2 + panels * FotobioSpec.KeelPanelArea * FotobioSpec.KeelThick * 1000f;

        // ── EL TRONCO AGUANTA LA FACHADA ──
        StructuralLimits.RootReport rep = FotobioSpec.Structure(h, out float mn, out float km);
        string what = $"fachada {mn:0.00} MN ({FotobioSpec.TrunkTonnes:0} t en el tronco, {FotobioSpec.KeelTonnes:0.0} t por quilla, {km:0.0} MN·m en cada rama) → {rep}";
        if (rep.Fail) Debug.LogError($"[Estructura] El tronco no aguanta la fachada de fotobiorreactores: {what}.");
        else if (rep.Warn) Debug.LogWarning($"[Estructura] Con la fachada de fotobiorreactores el tronco va justo: {what}.");
        else Debug.Log($"[Estructura] Tronco con la fachada de fotobiorreactores: {what}.");
        float o2 = FotobioSpec.O2KgDay(fb.areaM2);
        Debug.Log($"[Fotobio] Fachada: {modules} módulos de tubos en el tronco ({FotobioSpec.TrunkBottom:0.0}–{FotobioSpec.TrunkTop:0} m) y {panels} paneles en {branches} ramas: " +
                  $"{fb.areaM2:0} m², {fb.liters / 1000f:0.0} m³ de cultivo; a pleno rendimiento {FotobioSpec.BiomassKgDay(fb.areaM2):0.0} kg de espirulina y {o2:0.0} kg de O₂ al día " +
                  $"({o2 / FotobioSpec.O2PerPerson:0.0} personas); LED {FotobioSpec.LedKW(fb.areaM2):0} kW ({FotobioSpec.LedPPFD(fb.areaM2):0} µmol/m²/s) en la noche polar.");
    }

    static void FotobioPiece(Transform root, string name, List<Vector3> v, List<int> t, Color c)
    {
        if (v.Count == 0) return;
        Mesh m = new Mesh { name = name };
        if (v.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m.vertices = v.ToArray(); m.triangles = t.ToArray();
        m.RecalculateNormals(); m.RecalculateBounds();
        GameObject g = new GameObject(name);
        g.transform.SetParent(root, false);
        g.AddComponent<MeshFilter>().sharedMesh = m;
        MeshRenderer mr = g.AddComponent<MeshRenderer>();
        mr.sharedMaterial = KitchenProps.Mat(c);
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;   // cientos de tubos: sin sombra
    }

    /// <summary>Un tubo (prisma de 8 caras, sin tapas) de <paramref name="a"/> a <paramref name="b"/>.</summary>
    static void FotobioTube(List<Vector3> v, List<int> t, Vector3 a, Vector3 b, float r)
    {
        Vector3 d = (b - a).normalized, u = Vector3.Cross(d, Mathf.Abs(d.y) < 0.9f ? Vector3.up : Vector3.right).normalized, w = Vector3.Cross(d, u);
        const int n = 8;
        for (int i = 0; i < n; i++)
        {
            float a0 = i * Mathf.PI * 2f / n, a1 = (i + 1) * Mathf.PI * 2f / n;
            Vector3 o0 = (u * Mathf.Cos(a0) + w * Mathf.Sin(a0)) * r, o1 = (u * Mathf.Cos(a1) + w * Mathf.Sin(a1)) * r;
            int k = v.Count;
            v.Add(a + o0); v.Add(b + o0); v.Add(b + o1); v.Add(a + o1);
            // hacia fuera: el giro de (b − a) × (o1 − o0) apunta como o0 + o1
            if (Vector3.Dot(Vector3.Cross(b - a, o1 - o0), o0 + o1) > 0f) { t.Add(k); t.Add(k + 1); t.Add(k + 2); t.Add(k); t.Add(k + 2); t.Add(k + 3); }
            else { t.Add(k); t.Add(k + 2); t.Add(k + 1); t.Add(k); t.Add(k + 3); t.Add(k + 2); }
        }
    }

    /// <summary>Muchas cajas (centro, tamaño, giro) en una malla.</summary>
    static void FotobioBoxes(List<Vector3> v, List<int> t, List<(Vector3 c, Vector3 s, Quaternion q)> boxes)
    {
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
    }
}
