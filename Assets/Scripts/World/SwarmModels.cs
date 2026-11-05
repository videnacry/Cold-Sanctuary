using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LOS INDIVIDUOS DE LOS GRUPOS (Berón 2026-11-03: «los peces ahora mismo parecen plataformas, deberían tener la apariencia de
/// peces»; docs/grupos-de-animales.md). Modelos low-poly hechos por código, con mallas y materiales COMPARTIDOS (una malla por
/// especie y pieza, para todos los individuos): así un grupo de 40 peces cuesta 80 renderers baratos y ninguna malla nueva.
/// Medidas reales de las especies del mar de Ross (el santuario está a −77,85°, el de McMurdo):
///   · PEZ PLATEADO ANTÁRTICO (<i>Pleuragramma antarctica</i>): el pez pelágico que forma bancos en la plataforma antártica;
///     15–25 cm, alargado, lomo gris azulado y vientre plateado, aleta caudal ahorquillada.
///   · KRIL ANTÁRTICO (<i>Euphausia superba</i>): 5–6 cm, translúcido rojizo, ojos negros grandes, abdomen curvado y patas.
///   · MEDUSAS (Scyphozoa) que viven en McMurdo: <i>Diplulmaris antarctica</i> (umbrela de 10–18 cm, anaranjada) y
///     <i>Desmonema glaciale</i> (la grande: umbrela de hasta ~1 m, pardo rojiza, tentáculos de varios metros).
///   · ABEJA DE LAS NIEVES (la del santuario, ~20 cm): cabeza, tórax, abdomen a bandas y dos pares de alas.
/// Para que se vean desde la cámara del juego, los peces y el kril van algo mayores que en la vida real (×1,4 y ×2): se
/// apunta en <see cref="DisplayScale"/>.
/// </summary>
public static class SwarmModels
{
    public enum Species { SilverFish, Krill, Diplulmaris, Desmonema, SnowBee, Clam }

    /// <summary>Escala de visualización sobre la talla real (la talla real va en la malla).</summary>
    public static float DisplayScale(Species s) => s == Species.SilverFish ? 1.4f : s == Species.Krill ? 2f : 1f;

    public static string Label(Species s) => s switch
    {
        Species.SilverFish => "Pez plateado antártico (Pleuragramma antarctica)",
        Species.Krill => "Kril antártico (Euphausia superba)",
        Species.Diplulmaris => "Medusa Diplulmaris antarctica",
        Species.Desmonema => "Medusa Desmonema glaciale",
        Species.Clam => "Almeja de Groenlandia (Serripes groenlandicus)",
        _ => "Abeja de las nieves",
    };

    static readonly Dictionary<string, Mesh> _meshes = new Dictionary<string, Mesh>();

    /// <summary>Un individuo nuevo (sin collider), con sus piezas como hijos: «Cuerpo», «Aletas»/«Patas», «Alas»…</summary>
    public static GameObject Make(Species s)
    {
        GameObject g = new GameObject(s == Species.SnowBee ? "Abeja" : s == Species.Krill ? "Kril" : s == Species.SilverFish ? "Pez" : "Medusa");
        switch (s)
        {
            case Species.SilverFish:
                Part(g.transform, "Cuerpo", Get("pez_cuerpo", FishBody), new Color(0.72f, 0.78f, 0.84f));
                Part(g.transform, "Lomo", Get("pez_lomo", FishBack), new Color(0.28f, 0.36f, 0.48f));
                Part(g.transform, "Aletas", Get("pez_aletas", FishFins), new Color(0.45f, 0.52f, 0.6f));
                Part(g.transform, "Ojos", Get("pez_ojos", FishEyes), new Color(0.05f, 0.05f, 0.06f));
                break;
            case Species.Krill:
                Part(g.transform, "Cuerpo", Get("kril_cuerpo", KrillBody), new Color(0.95f, 0.42f, 0.32f, 0.82f));
                Part(g.transform, "Patas", Get("kril_patas", KrillLegs), new Color(0.98f, 0.55f, 0.45f, 0.7f));
                Part(g.transform, "Ojos", Get("kril_ojos", KrillEyes), new Color(0.04f, 0.04f, 0.04f));
                break;
            case Species.Clam:   // dos valvas (la campana de la medusa, aplastada), de ~10 cm
                Part(g.transform, "Valva", Get("almeja_valva", () => JellyBell(0.06f, 0.025f)), new Color(0.72f, 0.64f, 0.52f));
                Part(g.transform, "Valva_baja", Get("almeja_valva", () => JellyBell(0.06f, 0.025f)), new Color(0.6f, 0.52f, 0.42f)).localScale = new Vector3(1f, -1f, 1f);
                break;
            case Species.Diplulmaris:
                Part(g.transform, "Umbrela", Get("dip_umbrela", () => JellyBell(0.07f, 0.045f)), new Color(0.98f, 0.66f, 0.3f, 0.55f));
                Part(g.transform, "Brazos", Get("dip_brazos", () => JellyArms(0.03f, 0.16f, 4)), new Color(1f, 0.8f, 0.55f, 0.6f));
                Part(g.transform, "Tentaculos", Get("dip_tent", () => JellyTentacles(0.066f, 0.25f, 16)), new Color(1f, 0.85f, 0.7f, 0.45f));
                break;
            case Species.Desmonema:
                Part(g.transform, "Umbrela", Get("des_umbrela", () => JellyBell(0.45f, 0.25f)), new Color(0.62f, 0.28f, 0.2f, 0.55f));
                Part(g.transform, "Brazos", Get("des_brazos", () => JellyArms(0.15f, 1.6f, 4)), new Color(0.78f, 0.45f, 0.35f, 0.6f));
                Part(g.transform, "Tentaculos", Get("des_tent", () => JellyTentacles(0.42f, 3.2f, 24)), new Color(0.85f, 0.6f, 0.5f, 0.4f));
                break;
            default:
                Part(g.transform, "Cuerpo", Get("abeja_cuerpo", BeeBody), new Color(0.72f, 0.72f, 0.69f));
                Part(g.transform, "Bandas", Get("abeja_bandas", BeeBands), new Color(0.4f, 0.38f, 0.35f));
                Transform wl = Part(g.transform, "Ala_izq", Get("abeja_ala", BeeWing), new Color(0.62f, 0.66f, 0.7f, 0.3f));
                Transform wr = Part(g.transform, "Ala_der", Get("abeja_ala", BeeWing), new Color(0.62f, 0.66f, 0.7f, 0.3f));
                wl.localPosition = new Vector3(-0.015f, 0.035f, 0.01f); wr.localPosition = new Vector3(0.015f, 0.035f, 0.01f);
                wr.localScale = new Vector3(-1f, 1f, 1f);
                break;
        }
        g.transform.localScale = Vector3.one * DisplayScale(s);
        return g;
    }

    static Transform Part(Transform parent, string name, Mesh m, Color c)
    {
        GameObject p = new GameObject(name);
        p.transform.SetParent(parent, false);
        p.AddComponent<MeshFilter>().sharedMesh = m;
        MeshRenderer r = p.AddComponent<MeshRenderer>();
        r.sharedMaterial = Matte(c);   // (2026-11-04: mate, sin brillo ni emisión: las abejas deslumbraban como luces)
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;   // pequeños y muchos: sin sombra
        r.receiveShadows = false;
        return p.transform;
    }

    // ── MATERIAL MATE (Berón 2026-11-04: «las abejas brillan tanto que deslumbran») ──
    static readonly Dictionary<Color, Material> _matte = new Dictionary<Color, Material>();
    /// <summary>Lo más claro que puede ser un individuo (luminancia): con el sol del hielo y el posprocesado, un cuerpo casi blanco y
    /// liso se ve como una luz.</summary>
    public const float MaxLuminance = 0.55f;
    /// <summary>LUZ PROPIA SUAVE (ronda 7, 2026-11-05): sin ella, a contraluz las abejas salían casi negras y las medusas, negras sobre
    /// el agua oscura. Es el suelo de brillo, en fracción del color (los translúcidos, más: el alfa la reduce); muy por debajo del
    /// umbral del posprocesado, así que no deslumbra.</summary>
    public const float SoftGlow = 0.3f, SoftGlowTranslucent = 0.8f;

    /// <summary>Un material MATE para <paramref name="c"/>: sin brillo especular ni reflejos (también los translúcidos: alas,
    /// medusas, kril), con una luz propia suave para no verse negro a contraluz y, si es muy claro, oscurecido hasta
    /// <see cref="MaxLuminance"/>.</summary>
    public static Material Matte(Color c)
    {
        if (_matte.TryGetValue(c, out Material m) && m != null) return m;
        Color t = c;
        float lum = 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
        if (lum > MaxLuminance) { float k = MaxLuminance / lum; t = new Color(c.r * k, c.g * k, c.b * k, c.a); }
        m = new Material(KitchenProps.Mat(t)) { name = "Grupo_mate_" + ColorUtility.ToHtmlStringRGBA(t) };
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", t.a < 0.99f ? 0.2f : 0.15f);   // (translúcidos sin cristal: alas, umbrela, tentáculos, kril)
        if (m.HasProperty("_SpecularHighlights")) m.SetFloat("_SpecularHighlights", 0f);
        m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        if (m.HasProperty("_EnvironmentReflections")) m.SetFloat("_EnvironmentReflections", 0f);
        m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
        if (m.HasProperty("_EmissionColor"))
        {
            float glow = t.a < 0.99f ? SoftGlowTranslucent : SoftGlow;
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", new Color(t.r * glow, t.g * glow, t.b * glow, 1f));
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;   // no ilumina nada: solo que no se vea negro
        }
        return _matte[c] = m;
    }

    /// <summary>Pasa a MATE todos los renderers de un cuerpo (las abejas de la colmena: sus prefabs eran casi blancos y lisos).</summary>
    public static void Dim(GameObject who)
    {
        foreach (Renderer r in who.GetComponentsInChildren<Renderer>(true))
        {
            if (r is ParticleSystemRenderer || r is LineRenderer || r.sharedMaterial == null) continue;
            if (r.GetComponent<TMPro.TMP_Text>() != null) continue;
            Color c = r.sharedMaterial.HasProperty("_BaseColor") ? r.sharedMaterial.GetColor("_BaseColor") : r.sharedMaterial.color;
            r.sharedMaterial = Matte(c);
        }
    }

    static Mesh Get(string key, System.Func<B> build)
    {
        if (_meshes.TryGetValue(key, out Mesh m) && m != null) return m;
        m = build().ToMesh(key);
        _meshes[key] = m;
        return m;
    }

    // ── Constructor de mallas ───────────────────────────────────────────────────────────────────

    class B
    {
        public readonly List<Vector3> v = new List<Vector3>();
        public readonly List<int> t = new List<int>();
        public void Tri(Vector3 a, Vector3 b, Vector3 c, bool both = false)
        {
            int i = v.Count; v.Add(a); v.Add(b); v.Add(c); t.Add(i); t.Add(i + 1); t.Add(i + 2);
            if (both) { t.Add(i); t.Add(i + 2); t.Add(i + 1); }
        }
        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, bool both = false) { Tri(a, b, c, both); Tri(a, c, d, both); }
        /// <summary>Un cuerpo a lo largo de +Z: secciones elípticas (z, semiancho, semialto, desplazamiento en y).</summary>
        public void Loft((float z, float w, float h, float y)[] s, int sides)
        {
            for (int k = 0; k + 1 < s.Length; k++)
                for (int i = 0; i < sides; i++)
                {
                    float a0 = i * Mathf.PI * 2f / sides, a1 = (i + 1) * Mathf.PI * 2f / sides;
                    Vector3 P(int kk, float a) => new Vector3(Mathf.Cos(a) * s[kk].w, s[kk].y + Mathf.Sin(a) * s[kk].h, s[kk].z);
                    Quad(P(k, a0), P(k, a1), P(k + 1, a1), P(k + 1, a0));
                }
        }
        public void Ellipsoid(Vector3 c, Vector3 r, int lat = 5, int lon = 8)
        {
            for (int i = 0; i < lat; i++)
                for (int j = 0; j < lon; j++)
                {
                    Vector3 P(int ii, int jj)
                    {
                        float th = Mathf.PI * ii / lat, ph = Mathf.PI * 2f * jj / lon;
                        return c + new Vector3(Mathf.Sin(th) * Mathf.Cos(ph) * r.x, Mathf.Cos(th) * r.y, Mathf.Sin(th) * Mathf.Sin(ph) * r.z);
                    }
                    Quad(P(i, j), P(i, j + 1), P(i + 1, j + 1), P(i + 1, j));
                }
        }
        public void Box(Vector3 c, Vector3 s)
        {
            Vector3 h = s * 0.5f;
            Vector3 V(int x, int y, int z) => c + new Vector3(x * h.x, y * h.y, z * h.z);
            Quad(V(-1, -1, 1), V(1, -1, 1), V(1, 1, 1), V(-1, 1, 1));
            Quad(V(1, -1, -1), V(-1, -1, -1), V(-1, 1, -1), V(1, 1, -1));
            Quad(V(-1, 1, -1), V(-1, 1, 1), V(1, 1, 1), V(1, 1, -1));
            Quad(V(-1, -1, 1), V(-1, -1, -1), V(1, -1, -1), V(1, -1, 1));
            Quad(V(1, -1, 1), V(1, -1, -1), V(1, 1, -1), V(1, 1, 1));
            Quad(V(-1, -1, -1), V(-1, -1, 1), V(-1, 1, 1), V(-1, 1, -1));
        }
        public Mesh ToMesh(string name)
        {
            Mesh m = new Mesh { name = "Grupo_" + name };
            m.SetVertices(v); m.SetTriangles(t, 0);
            m.RecalculateNormals(); m.RecalculateBounds();
            return m;
        }
    }

    // ── Pez plateado antártico: 20 cm, alargado (alto ≈ 1/6 del largo), vientre plateado y lomo oscuro ──
    const float FL = 0.2f;
    static readonly (float z, float w, float h, float y)[] FishSections =
    {
        (-0.5f * FL, 0.004f, 0.006f, 0f), (-0.36f * FL, 0.012f, 0.02f, 0f), (-0.18f * FL, 0.016f, 0.03f, 0.002f), (0.02f * FL, 0.017f, 0.032f, 0.003f),
        (0.22f * FL, 0.015f, 0.027f, 0.002f), (0.38f * FL, 0.011f, 0.019f, 0.001f), (0.5f * FL, 0.003f, 0.006f, 0f),
    };
    static B FishBody() { B b = new B(); b.Loft(FishSections, 8); return b; }
    static B FishBack()
    {
        // El lomo: la mitad de arriba algo más grande (cubre el cuerpo plateado por encima).
        B b = new B();
        var s = FishSections;
        for (int k = 0; k + 1 < s.Length; k++)
            for (int i = 0; i < 4; i++)
            {
                float a0 = Mathf.PI * (0.15f + 0.7f * i / 4f), a1 = Mathf.PI * (0.15f + 0.7f * (i + 1) / 4f);
                Vector3 P(int kk, float a) => new Vector3(Mathf.Cos(a) * s[kk].w * 1.04f, s[kk].y + Mathf.Sin(a) * s[kk].h * 1.04f, s[kk].z);
                b.Quad(P(k, a0), P(k, a1), P(k + 1, a1), P(k + 1, a0));
            }
        return b;
    }
    static B FishFins()
    {
        B b = new B();
        float tz = -0.5f * FL;
        // Caudal ahorquillada (dos lóbulos), dorsal larga y baja, pectorales.
        b.Tri(new Vector3(0f, 0f, tz + 0.01f), new Vector3(0f, 0.032f, tz - 0.045f), new Vector3(0f, 0.008f, tz - 0.03f), true);
        b.Tri(new Vector3(0f, 0f, tz + 0.01f), new Vector3(0f, -0.008f, tz - 0.03f), new Vector3(0f, -0.032f, tz - 0.045f), true);
        b.Tri(new Vector3(0f, 0.03f, 0.05f * FL), new Vector3(0f, 0.045f, -0.12f * FL), new Vector3(0f, 0.025f, -0.3f * FL), true);
        b.Tri(new Vector3(0f, -0.025f, -0.05f * FL), new Vector3(0f, -0.038f, -0.2f * FL), new Vector3(0f, -0.018f, -0.35f * FL), true);
        for (int s = -1; s <= 1; s += 2)
            b.Tri(new Vector3(s * 0.016f, -0.005f, 0.24f * FL), new Vector3(s * 0.04f, -0.012f, 0.12f * FL), new Vector3(s * 0.016f, -0.012f, 0.16f * FL), true);
        return b;
    }
    static B FishEyes()
    {
        B b = new B();
        for (int s = -1; s <= 1; s += 2) b.Ellipsoid(new Vector3(s * 0.012f, 0.006f, 0.38f * FL), new Vector3(0.004f, 0.005f, 0.005f), 3, 6);
        return b;
    }

    // ── Kril antártico: 6 cm, el abdomen curvado hacia abajo, ojos grandes y negros, toracópodos en cesta ──
    static B KrillBody()
    {
        B b = new B();
        b.Loft(new (float, float, float, float)[]
        {
            (-0.03f, 0.002f, 0.002f, -0.006f), (-0.024f, 0.0035f, 0.004f, -0.004f), (-0.014f, 0.005f, 0.0055f, -0.001f), (-0.004f, 0.0055f, 0.006f, 0f),
            (0.008f, 0.0065f, 0.0072f, 0.001f), (0.02f, 0.006f, 0.0065f, 0.001f), (0.028f, 0.003f, 0.003f, 0f),
        }, 6);
        // Telson y urópodos (la «cola» en abanico).
        b.Tri(new Vector3(0f, -0.006f, -0.03f), new Vector3(-0.006f, -0.009f, -0.038f), new Vector3(0.006f, -0.009f, -0.038f), true);
        // Antenas.
        for (int s = -1; s <= 1; s += 2) b.Tri(new Vector3(s * 0.002f, 0.002f, 0.028f), new Vector3(s * 0.012f, 0.006f, 0.06f), new Vector3(s * 0.003f, 0.003f, 0.03f), true);
        return b;
    }
    static B KrillLegs()
    {
        B b = new B();
        for (int i = 0; i < 6; i++)
        {
            float z = 0.015f - i * 0.0045f;
            for (int s = -1; s <= 1; s += 2) b.Tri(new Vector3(s * 0.002f, -0.005f, z), new Vector3(s * 0.006f, -0.016f, z - 0.002f), new Vector3(s * 0.002f, -0.005f, z - 0.002f), true);
        }
        return b;
    }
    static B KrillEyes()
    {
        B b = new B();
        for (int s = -1; s <= 1; s += 2) b.Ellipsoid(new Vector3(s * 0.0045f, 0.002f, 0.026f), new Vector3(0.0028f, 0.0028f, 0.0028f), 3, 6);
        return b;
    }

    // ── Medusas: umbrela (cúpula), brazos orales (cintas) y tentáculos del borde ──
    static B JellyBell(float r, float h)
    {
        B b = new B();
        const int lat = 5, lon = 14;
        for (int i = 0; i < lat; i++)
            for (int j = 0; j < lon; j++)
            {
                Vector3 P(int ii, int jj)
                {
                    float th = Mathf.PI * 0.5f * ii / lat, ph = Mathf.PI * 2f * jj / lon;
                    float lobe = 1f + (ii == lat ? 0.06f * Mathf.Cos(ph * 8f) : 0f);   // el borde festoneado (lóbulos)
                    return new Vector3(Mathf.Sin(th) * Mathf.Cos(ph) * r * lobe, Mathf.Cos(th) * h, Mathf.Sin(th) * Mathf.Sin(ph) * r * lobe);
                }
                b.Quad(P(i, j), P(i, j + 1), P(i + 1, j + 1), P(i + 1, j), true);
            }
        return b;
    }
    static B JellyArms(float w, float len, int n)
    {
        B b = new B();
        for (int k = 0; k < n; k++)
        {
            float a = k * Mathf.PI * 2f / n;
            Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)), side = new Vector3(-dir.z, 0f, dir.x);
            const int seg = 6;
            for (int i = 0; i < seg; i++)
            {
                float y0 = -len * i / seg, y1 = -len * (i + 1) / seg;
                Vector3 c0 = dir * (w * 0.4f + 0.15f * w * Mathf.Sin(i * 1.3f)) + Vector3.up * y0, c1 = dir * (w * 0.4f + 0.15f * w * Mathf.Sin((i + 1) * 1.3f)) + Vector3.up * y1;
                float ww0 = w * (1f - 0.12f * i), ww1 = w * (1f - 0.12f * (i + 1));
                b.Quad(c0 - side * ww0 * 0.5f, c0 + side * ww0 * 0.5f, c1 + side * ww1 * 0.5f, c1 - side * ww1 * 0.5f, true);
            }
        }
        return b;
    }
    static B JellyTentacles(float r, float len, int n)
    {
        B b = new B();
        float w = Mathf.Max(0.002f, r * 0.04f);
        for (int k = 0; k < n; k++)
        {
            float a = (k + 0.5f) * Mathf.PI * 2f / n;
            Vector3 root = new Vector3(Mathf.Cos(a) * r, -0.01f, Mathf.Sin(a) * r), side = new Vector3(-Mathf.Sin(a), 0f, Mathf.Cos(a)) * w;
            float l = len * (0.7f + 0.3f * Mathf.Abs(Mathf.Sin(k * 2.1f)));
            const int seg = 5;
            for (int i = 0; i < seg; i++)
            {
                Vector3 p0 = root + new Vector3(0.1f * r * Mathf.Sin(i * 0.9f + k), -l * i / seg, 0f), p1 = root + new Vector3(0.1f * r * Mathf.Sin((i + 1) * 0.9f + k), -l * (i + 1) / seg, 0f);
                b.Quad(p0 - side, p0 + side, p1 + side, p1 - side, true);
            }
        }
        return b;
    }

    // ── Abeja de las nieves (~20 cm): cabeza, tórax y abdomen; bandas oscuras; alas ──
    static B BeeBody()
    {
        B b = new B();
        b.Ellipsoid(new Vector3(0f, 0f, 0.065f), new Vector3(0.022f, 0.022f, 0.02f), 4, 8);     // cabeza
        b.Ellipsoid(new Vector3(0f, 0.005f, 0.025f), new Vector3(0.03f, 0.03f, 0.032f), 5, 8);  // tórax (peludo)
        b.Ellipsoid(new Vector3(0f, -0.004f, -0.045f), new Vector3(0.03f, 0.028f, 0.055f), 5, 10);   // abdomen
        return b;
    }
    static B BeeBands()
    {
        B b = new B();
        foreach (float z in new[] { -0.02f, -0.045f, -0.07f })
            b.Loft(new (float, float, float, float)[] { (z - 0.006f, 0.031f, 0.029f, -0.004f), (z + 0.006f, 0.031f, 0.029f, -0.004f) }, 10);
        for (int s = -1; s <= 1; s += 2) b.Ellipsoid(new Vector3(s * 0.016f, 0.006f, 0.075f), new Vector3(0.008f, 0.012f, 0.008f), 3, 6);   // ojos compuestos
        for (int s = -1; s <= 1; s += 2) b.Tri(new Vector3(s * 0.006f, 0.015f, 0.08f), new Vector3(s * 0.02f, 0.045f, 0.11f), new Vector3(s * 0.008f, 0.017f, 0.084f), true);   // antenas
        b.Tri(new Vector3(0f, -0.006f, -0.1f), new Vector3(-0.003f, -0.004f, -0.09f), new Vector3(0.003f, -0.004f, -0.09f), true);   // aguijón
        return b;
    }
    static B BeeWing()
    {
        B b = new B();
        b.Quad(new Vector3(0f, 0f, 0.012f), new Vector3(-0.09f, 0.006f, 0.0f), new Vector3(-0.085f, 0.004f, -0.03f), new Vector3(0f, 0f, -0.012f), true);
        b.Quad(new Vector3(0f, 0f, -0.01f), new Vector3(-0.06f, 0.004f, -0.035f), new Vector3(-0.05f, 0.003f, -0.05f), new Vector3(0f, 0f, -0.025f), true);
        return b;
    }
}
