using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MOBILIARIO, UTENSILIOS E INGREDIENTES DE COCINA hechos con UNIONES DE CUBOS (docs/kitchen-simulation.md): cada pieza
/// se reconoce por su forma —la sartén tiene mango, la olla asas y tapa, el cuchillo hoja y cacha, la nevera puerta con
/// tirador y baldas, el fogón quemadores y mandos, la estantería baldas llenas de frascos y sacos— en vez de un cubo
/// liso con una etiqueta. A tamaño real (encimera a 0,9 m). Cada objeto raíz lleva un BoxCollider ajustado (los
/// hechizos `use/take/putIn` miden el alcance contra su superficie) y su nombre de siempre («Nevera», «Sarten 1»…).
/// </summary>
public static class KitchenProps
{
    public const float CounterTop = 0.9f;

    static readonly Dictionary<Color, Material> _mats = new Dictionary<Color, Material>();
    public static Material Mat(Color c)
    {
        if (_mats.TryGetValue(c, out Material m) && m != null) return m;
        Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        m = new Material(sh) { color = c, name = $"Cocina_{ColorUtility.ToHtmlStringRGB(c)}" };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.35f);
        if (c.a < 0.99f && m.HasProperty("_Surface"))   // TRANSPARENTE (cristal, agua de una pecera): URP Lit en modo alfa
        {
            m.SetFloat("_Surface", 1f); m.SetFloat("_Blend", 0f);
            m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f); m.SetFloat("_Smoothness", 0.92f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.SetOverrideTag("RenderType", "Transparent");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        return _mats[c] = m;
    }

    static Color H(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out Color c); return c; }
    public static readonly Color Steel = H("B9BEC6"), DarkSteel = H("5E636B"), Wood = H("B58E6A"), WoodDark = H("8A6A4E"),
        White = H("F3F3EF"), Black = H("2B2B2E"), Ceramic = H("EDEAE2"), Glass = H("CFE6EE"), Counter = H("DCD6CB"), Tile = H("E4E8EA");

    // ── Base ────────────────────────────────────────────────────────────────────────────────────

    /// <summary>Raíz con nombre (el que usan los hechizos) en <paramref name="localPos"/> (base apoyada).</summary>
    public static Transform Root(Transform parent, string name, Vector3 localPos, float yaw = 0f)
    {
        Transform t = new GameObject(name).transform;
        t.SetParent(parent, false);
        t.localPosition = localPos;
        t.localRotation = Quaternion.Euler(0f, yaw, 0f);
        return t;
    }

    public static Transform C(Transform parent, Vector3 pos, Vector3 size, Color c, float yaw = 0f, string name = "cubo")
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        Collider col = g.GetComponent<Collider>();
        if (Application.isPlaying) Object.Destroy(col); else Object.DestroyImmediate(col);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = Mat(c);
        return g.transform;
    }

    /// <summary>BoxCollider en la raíz que envuelve todas sus piezas (alcance y bloqueo físico).</summary>
    public static void Fit(Transform root, bool solid = true)
    {
        Renderer[] rs = root.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return;
        Bounds b = rs[0].bounds; foreach (Renderer r in rs) b.Encapsulate(r.bounds);
        if (!root.TryGetComponent(out BoxCollider bc)) bc = root.gameObject.AddComponent<BoxCollider>();   // (?? no vale con componentes de Unity: el «null» falso del editor)
        Vector3 ls = root.lossyScale;
        bc.center = root.InverseTransformPoint(b.center);
        Vector3 size = Quaternion.Inverse(root.rotation) * b.size;
        bc.size = new Vector3(Mathf.Abs(size.x) / Mathf.Max(ls.x, 1e-4f), Mathf.Abs(size.y) / Mathf.Max(ls.y, 1e-4f), Mathf.Abs(size.z) / Mathf.Max(ls.z, 1e-4f));
        bc.isTrigger = !solid;
    }

    // ── Mobiliario ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Tramo de encimera con armarios bajos (puertas y tiradores) y zócalo.</summary>
    public static Transform CounterRun(Transform parent, string name, Vector3 pos, float length, float yaw = 0f, float depth = 0.65f)
    {
        Transform t = Root(parent, name, pos, yaw);
        C(t, new Vector3(0f, 0.05f, 0.03f), new Vector3(length, 0.1f, depth - 0.06f), DarkSteel, 0f, "Zocalo");
        C(t, new Vector3(0f, 0.47f, 0f), new Vector3(length, 0.74f, depth), WoodDark, 0f, "Armarios");
        int doors = Mathf.Max(1, Mathf.RoundToInt(length / 0.6f));
        for (int i = 0; i < doors; i++)
        {
            float x = -length * 0.5f + (i + 0.5f) * length / doors;
            C(t, new Vector3(x, 0.47f, -depth * 0.5f - 0.01f), new Vector3(length / doors - 0.03f, 0.68f, 0.02f), Wood, 0f, "Puerta");
            C(t, new Vector3(x + length / doors * 0.35f, 0.72f, -depth * 0.5f - 0.03f), new Vector3(0.02f, 0.12f, 0.02f), Steel, 0f, "Tirador");
        }
        C(t, new Vector3(0f, CounterTop - 0.02f, 0f), new Vector3(length + 0.02f, 0.04f, depth + 0.04f), Counter, 0f, "Tablero");
        Fit(t);
        return t;
    }

    /// <summary>Nevera alta de dos puertas con tiradores, rejilla y baldas interiores visibles por el lateral.</summary>
    public static Transform Fridge(Transform parent, string name, Vector3 pos, float yaw = 0f)
    {
        Transform t = Root(parent, name, pos, yaw);
        C(t, new Vector3(0f, 0.95f, 0f), new Vector3(0.8f, 1.9f, 0.7f), H("E9EEF2"), 0f, "Cuerpo");
        C(t, new Vector3(0f, 1.42f, -0.36f), new Vector3(0.78f, 0.92f, 0.03f), White, 0f, "Puerta_Congelador");
        C(t, new Vector3(0f, 0.5f, -0.36f), new Vector3(0.78f, 0.88f, 0.03f), White, 0f, "Puerta_Nevera");
        C(t, new Vector3(0.3f, 1.2f, -0.39f), new Vector3(0.03f, 0.35f, 0.03f), Steel, 0f, "Tirador");
        C(t, new Vector3(0.3f, 0.75f, -0.39f), new Vector3(0.03f, 0.35f, 0.03f), Steel, 0f, "Tirador");
        C(t, new Vector3(0f, 0.02f, -0.33f), new Vector3(0.7f, 0.04f, 0.04f), DarkSteel, 0f, "Rejilla");
        Fit(t);
        return t;
    }

    /// <summary>COCINA DE INDUCCIÓN (aquí no hay gas: todo es solar) de 4 zonas sobre vitrocerámica negra, con panel táctil,
    /// y horno eléctrico debajo (puerta con ventana). Las zonas encendidas brillan en ámbar tenue.</summary>
    public static Transform Stove(Transform parent, string name, Vector3 pos, float yaw = 0f)
    {
        Transform t = Root(parent, name, pos, yaw);
        C(t, new Vector3(0f, 0.45f, 0f), new Vector3(0.76f, 0.9f, 0.65f), Steel, 0f, "Cuerpo");
        C(t, new Vector3(0f, 0.42f, -0.33f), new Vector3(0.62f, 0.5f, 0.02f), DarkSteel, 0f, "Horno");
        C(t, new Vector3(0f, 0.45f, -0.345f), new Vector3(0.42f, 0.25f, 0.01f), H("3A3F47"), 0f, "Ventana");
        C(t, new Vector3(0f, 0.72f, -0.35f), new Vector3(0.6f, 0.03f, 0.03f), Steel, 0f, "Asa_Horno");
        C(t, new Vector3(0f, 0.905f, 0f), new Vector3(0.74f, 0.012f, 0.62f), H("16181C"), 0f, "Vitroceramica");
        for (int i = 0; i < 4; i++)
        {
            Vector3 b = new Vector3(i % 2 == 0 ? -0.18f : 0.18f, 0.912f, i < 2 ? -0.13f : 0.15f);
            float r = i == 0 ? 0.11f : 0.08f;   // una zona grande y tres normales
            for (int s = 0; s < 4; s++)   // el aro serigrafiado de la zona (4 trazos)
                C(t, b + Quaternion.Euler(0f, s * 90f, 0f) * new Vector3(0f, 0f, r), new Vector3(r * 1.4f, 0.002f, 0.006f), H("5A5F66"), s * 90f, "Zona");
            C(t, b + Vector3.up * 0.001f, new Vector3(r * 0.9f, 0.002f, r * 0.9f), H("3A2A22"), 45f, "Bobina");
        }
        C(t, new Vector3(0f, 0.912f, -0.27f), new Vector3(0.5f, 0.003f, 0.05f), H("2A2D33"), 0f, "Panel_Tactil");
        for (int k = 0; k < 5; k++) C(t, new Vector3(-0.2f + k * 0.1f, 0.914f, -0.27f), new Vector3(0.02f, 0.002f, 0.02f), H("E8A050"), 0f, "Tecla");
        Fit(t);
        return t;
    }

    /// <summary>Fregadero: pila hundida en la encimera, grifo de cuello de cisne y escurridor.</summary>
    public static Transform Sink(Transform parent, string name, Vector3 pos, float yaw = 0f)
    {
        Transform t = CounterRun(parent, name, pos, 1.2f, yaw);
        C(t, new Vector3(-0.15f, CounterTop - 0.06f, 0f), new Vector3(0.55f, 0.1f, 0.42f), Steel, 0f, "Pila");
        C(t, new Vector3(-0.15f, CounterTop - 0.1f, 0f), new Vector3(0.48f, 0.02f, 0.36f), DarkSteel, 0f, "Fondo");
        C(t, new Vector3(-0.15f, CounterTop + 0.15f, 0.22f), new Vector3(0.03f, 0.3f, 0.03f), Steel, 0f, "Grifo");
        C(t, new Vector3(-0.15f, CounterTop + 0.3f, 0.14f), new Vector3(0.03f, 0.03f, 0.18f), Steel, 0f, "Caño");
        for (int i = 0; i < 4; i++) C(t, new Vector3(0.35f, CounterTop + 0.01f, -0.15f + i * 0.1f), new Vector3(0.3f, 0.015f, 0.02f), Steel, 0f, "Escurridor");
        return t;
    }

    /// <summary>Estantería de 4 baldas llena de frascos, sacos, cajas de fruta y botellas (la despensa).</summary>
    public static Transform Shelving(Transform parent, string name, Vector3 pos, float yaw, int seed)
    {
        var rnd = new System.Random(seed);
        Transform t = Root(parent, name, pos, yaw);
        float w = 1.6f, d = 0.45f;
        for (int s = -1; s <= 1; s += 2) C(t, new Vector3(w * 0.5f * s, 0.95f, 0f), new Vector3(0.04f, 1.9f, d), Wood, 0f, "Lateral");
        C(t, new Vector3(0f, 0.95f, d * 0.5f), new Vector3(w, 1.9f, 0.02f), WoodDark, 0f, "Fondo");
        for (int lv = 0; lv < 4; lv++)
        {
            float y = 0.12f + lv * 0.48f;
            C(t, new Vector3(0f, y, 0f), new Vector3(w, 0.03f, d), Wood, 0f, "Balda");
            float x = -w * 0.5f + 0.12f;
            while (x < w * 0.5f - 0.12f)
            {
                int k = rnd.Next(5);
                Vector3 p = new Vector3(x, y + 0.015f, -0.02f);
                Color fill = PantryColors[rnd.Next(PantryColors.Length)];
                switch (k)
                {
                    case 0: Jar(t, p, fill); x += 0.16f; break;
                    case 1: Sack(t, p, fill); x += 0.26f; break;
                    case 2: Bottle(t, p, fill); x += 0.12f; break;
                    case 3: Crate(t, p, fill); x += 0.34f; break;
                    default: Jar(t, p, fill, 0.1f, 0.14f); x += 0.13f; break;
                }
            }
        }
        Fit(t);
        return t;
    }

    static readonly Color[] PantryColors =
    {
        H("F2E3B0"), H("D9B98A"), H("C88B5A"), H("E8D9A6"), H("9CC27A"), H("E2A35A"), H("F0CF6E"), H("B5653F"), H("EEEBDF"), H("D57A6A"),
    };

    static void Jar(Transform t, Vector3 p, Color fill, float w = 0.12f, float h = 0.2f)
    {
        C(t, p + Vector3.up * h * 0.45f, new Vector3(w, h * 0.9f, w), Glass, 0f, "Frasco");
        C(t, p + Vector3.up * h * 0.35f, new Vector3(w * 0.85f, h * 0.65f, w * 0.85f), fill, 0f, "Contenido");
        C(t, p + Vector3.up * h * 0.95f, new Vector3(w * 1.05f, h * 0.12f, w * 1.05f), WoodDark, 0f, "Tapa");
    }

    static void Sack(Transform t, Vector3 p, Color fill)
    {
        Color burlap = H("C9B48C");
        C(t, p + Vector3.up * 0.13f, new Vector3(0.22f, 0.26f, 0.2f), burlap, 0f, "Saco");
        C(t, p + Vector3.up * 0.28f, new Vector3(0.15f, 0.06f, 0.14f), burlap * 0.9f + new Color(0, 0, 0, 1), 0f, "Atado");
        C(t, p + new Vector3(0f, 0.15f, -0.101f), new Vector3(0.12f, 0.1f, 0.005f), fill, 0f, "Etiqueta");
    }

    static void Bottle(Transform t, Vector3 p, Color fill)
    {
        C(t, p + Vector3.up * 0.11f, new Vector3(0.08f, 0.22f, 0.08f), Glass, 0f, "Botella");
        C(t, p + Vector3.up * 0.08f, new Vector3(0.065f, 0.15f, 0.065f), fill, 0f, "Liquido");
        C(t, p + Vector3.up * 0.26f, new Vector3(0.03f, 0.07f, 0.03f), Glass, 0f, "Cuello");
        C(t, p + Vector3.up * 0.3f, new Vector3(0.035f, 0.02f, 0.035f), Black, 0f, "Tapon");
    }

    static void Crate(Transform t, Vector3 p, Color fruit)
    {
        C(t, p + Vector3.up * 0.06f, new Vector3(0.3f, 0.12f, 0.25f), Wood, 0f, "Caja");
        for (int i = 0; i < 6; i++)
            C(t, p + new Vector3(-0.09f + (i % 3) * 0.09f, 0.14f, -0.05f + (i / 3) * 0.1f), Vector3.one * 0.075f, fruit, i * 20f, "Fruta");
    }

    // ── Utensilios ──────────────────────────────────────────────────────────────────────────────

    /// <summary>Sartén: base, borde, fondo oscuro y mango largo.</summary>
    public static Transform Pan(Transform parent, string name, Vector3 pos, float yaw = 0f)
    {
        Transform t = Root(parent, name, pos, yaw);
        C(t, new Vector3(0f, 0.01f, 0f), new Vector3(0.26f, 0.02f, 0.26f), DarkSteel, 0f, "Base");
        C(t, new Vector3(0f, 0.02f, 0f), new Vector3(0.26f, 0.02f, 0.26f), DarkSteel, 45f, "Base");
        for (int s = 0; s < 4; s++)
            C(t, Quaternion.Euler(0f, s * 90f, 0f) * new Vector3(0f, 0.03f, 0.13f), new Vector3(0.2f, 0.04f, 0.015f), DarkSteel, s * 90f, "Borde");
        C(t, new Vector3(0f, 0.021f, 0f), new Vector3(0.21f, 0.003f, 0.21f), Black, 0f, "Antiadherente");
        C(t, new Vector3(0f, 0.035f, -0.25f), new Vector3(0.035f, 0.025f, 0.24f), Black, 0f, "Mango");
        Fit(t, false);
        return t;
    }

    /// <summary>Olla: cuerpo, borde, dos asas y tapa con pomo.</summary>
    public static Transform Pot(Transform parent, string name, Vector3 pos, float yaw = 0f)
    {
        Transform t = Root(parent, name, pos, yaw);
        C(t, new Vector3(0f, 0.07f, 0f), new Vector3(0.22f, 0.14f, 0.22f), Steel, 0f, "Cuerpo");
        C(t, new Vector3(0f, 0.07f, 0f), new Vector3(0.22f, 0.14f, 0.22f), Steel, 45f, "Cuerpo");
        for (int s = -1; s <= 1; s += 2) C(t, new Vector3(0.14f * s, 0.12f, 0f), new Vector3(0.05f, 0.02f, 0.06f), Black, 0f, "Asa");
        C(t, new Vector3(0f, 0.15f, 0f), new Vector3(0.21f, 0.015f, 0.21f), H("D3D7DD"), 45f, "Tapa");
        C(t, new Vector3(0f, 0.17f, 0f), new Vector3(0.04f, 0.025f, 0.04f), Black, 0f, "Pomo");
        Fit(t, false);
        return t;
    }

    /// <summary>Bol de cerámica (fondo estrecho, boca ancha).</summary>
    public static Transform Bowl(Transform parent, string name, Vector3 pos)
    {
        Transform t = Root(parent, name, pos);
        C(t, new Vector3(0f, 0.015f, 0f), new Vector3(0.1f, 0.03f, 0.1f), Ceramic, 0f, "Pie");
        C(t, new Vector3(0f, 0.05f, 0f), new Vector3(0.16f, 0.05f, 0.16f), Ceramic, 0f, "Cuenco");
        C(t, new Vector3(0f, 0.05f, 0f), new Vector3(0.16f, 0.05f, 0.16f), Ceramic, 45f, "Cuenco");
        C(t, new Vector3(0f, 0.076f, 0f), new Vector3(0.13f, 0.004f, 0.13f), H("D8D3C8"), 0f, "Interior");
        Fit(t, false);
        return t;
    }

    public static Transform Plate(Transform parent, string name, Vector3 pos)
    {
        Transform t = Root(parent, name, pos);
        C(t, new Vector3(0f, 0.008f, 0f), new Vector3(0.24f, 0.016f, 0.24f), White, 0f, "Plato");
        C(t, new Vector3(0f, 0.008f, 0f), new Vector3(0.24f, 0.016f, 0.24f), White, 45f, "Plato");
        C(t, new Vector3(0f, 0.017f, 0f), new Vector3(0.15f, 0.002f, 0.15f), Ceramic, 22f, "Centro");
        Fit(t, false);
        return t;
    }

    /// <summary>Tabla de cortar con cuchillo (hoja + cacha) encima.</summary>
    public static Transform BoardAndKnife(Transform parent, string name, Vector3 pos, float yaw = 0f)
    {
        Transform t = Root(parent, name, pos, yaw);
        C(t, new Vector3(0f, 0.012f, 0f), new Vector3(0.4f, 0.024f, 0.28f), H("D9B98A"), 0f, "Tabla");
        C(t, new Vector3(0.16f, 0.012f, 0f), new Vector3(0.025f, 0.01f, 0.06f), WoodDark, 0f, "Agujero");
        C(t, new Vector3(-0.05f, 0.03f, -0.06f), new Vector3(0.18f, 0.004f, 0.035f), Steel, 10f, "Hoja");
        C(t, new Vector3(-0.19f, 0.032f, -0.085f), new Vector3(0.1f, 0.018f, 0.022f), Black, 10f, "Cacha");
        Fit(t, false);
        return t;
    }

    /// <summary>Bote de utensilios: espátula, cucharón, batidor y cuchara de madera.</summary>
    public static Transform UtensilCrock(Transform parent, string name, Vector3 pos)
    {
        Transform t = Root(parent, name, pos);
        C(t, new Vector3(0f, 0.07f, 0f), new Vector3(0.11f, 0.14f, 0.11f), H("C9D6D9"), 0f, "Bote");
        C(t, new Vector3(-0.02f, 0.22f, 0f), new Vector3(0.015f, 0.22f, 0.01f), Black, 0f, "Espatula");
        C(t, new Vector3(-0.02f, 0.33f, 0f), new Vector3(0.06f, 0.07f, 0.008f), Black, 0f, "Pala");
        C(t, new Vector3(0.025f, 0.24f, 0.01f), new Vector3(0.012f, 0.26f, 0.012f), Steel, 0f, "Cucharon");
        C(t, new Vector3(0.025f, 0.37f, 0.03f), new Vector3(0.06f, 0.03f, 0.06f), Steel, 0f, "Cazo");
        C(t, new Vector3(0f, 0.22f, -0.03f), new Vector3(0.012f, 0.2f, 0.012f), Steel, 0f, "Batidor");
        for (int i = 0; i < 3; i++) C(t, new Vector3(0f, 0.34f, -0.03f), new Vector3(0.05f, 0.09f, 0.004f), Steel, i * 60f, "Varillas");
        C(t, new Vector3(0.01f, 0.25f, -0.01f), new Vector3(0.014f, 0.28f, 0.014f), Wood, 15f, "Cuchara_Madera");
        return t;
    }

    // ── Ingredientes (lo que se coge y se mueve) ────────────────────────────────────────────────

    /// <summary>Modelo pequeño de un ingrediente para llevar en la mano o poner en un recipiente.</summary>
    public static Transform Ingredient(Transform parent, string what, Vector3 pos)
    {
        string w = (what ?? "").ToLowerInvariant();
        Transform t = Root(parent, "Ingrediente_" + what, pos);
        if (w.Contains("huevo"))
        {
            if (w.Contains("batido") || w.Contains("revuelt"))
            { C(t, new Vector3(0f, 0.015f, 0f), new Vector3(0.14f, 0.025f, 0.12f), H("F7DD72"), 20f, "Revuelto"); C(t, new Vector3(0.03f, 0.03f, 0.02f), new Vector3(0.05f, 0.02f, 0.05f), H("FBE89A"), 50f, "Grumo"); }
            else for (int i = 0; i < 3; i++)
                { Vector3 o = new Vector3(-0.05f + i * 0.05f, 0f, 0f); C(t, o + Vector3.up * 0.028f, new Vector3(0.04f, 0.055f, 0.04f), H("F4EEDF"), 0f, "Huevo"); C(t, o + Vector3.up * 0.028f, new Vector3(0.045f, 0.035f, 0.045f), H("F4EEDF"), 45f, "Huevo"); }
        }
        else if (w.Contains("tomate") || w.Contains("fruta")) { C(t, new Vector3(0f, 0.04f, 0f), new Vector3(0.08f, 0.07f, 0.08f), H("E8665A"), 0f, "Fruto"); C(t, new Vector3(0f, 0.08f, 0f), new Vector3(0.04f, 0.01f, 0.04f), H("6FA65A"), 45f, "Tallo"); }
        else if (w.Contains("verdura") || w.Contains("espinaca") || w.Contains("col")) { for (int i = 0; i < 4; i++) C(t, new Vector3(0f, 0.03f + i * 0.005f, 0f), new Vector3(0.14f, 0.012f, 0.08f), i % 2 == 0 ? H("7FB86A") : H("A6D38E"), i * 45f, "Hoja"); }
        else if (w.Contains("avena") || w.Contains("arroz") || w.Contains("quinoa") || w.Contains("legumbre") || w.Contains("lenteja")) Jar(t, Vector3.zero, w.Contains("legumbre") || w.Contains("lenteja") ? H("B5653F") : H("F2E3B0"), 0.1f, 0.16f);
        else if (w.Contains("yogur") || w.Contains("leche")) { C(t, new Vector3(0f, 0.07f, 0f), new Vector3(0.08f, 0.14f, 0.08f), White, 0f, "Envase"); C(t, new Vector3(0f, 0.15f, 0f), new Vector3(0.08f, 0.02f, 0.08f), H("8FB8DE"), 0f, "Tapa"); }
        else if (w.Contains("banana")) { for (int i = 0; i < 3; i++) C(t, new Vector3(-0.02f + i * 0.02f, 0.02f, 0f), new Vector3(0.025f, 0.025f, 0.16f), H("F6DE6E"), -10f + i * 10f, "Banana"); }
        else C(t, new Vector3(0f, 0.04f, 0f), new Vector3(0.08f, 0.08f, 0.08f), H("D9C79A"), 0f, "Ingrediente");
        return t;
    }

    /// <summary>Plato servido (para la barra de servicio y lo que lleva el cocinero al emplatar).</summary>
    public static Transform Dish(Transform parent, string dishName, Vector3 pos)
    {
        Transform t = Plate(parent, "Platillo_" + dishName, pos);
        BoxCollider bc = t.GetComponent<BoxCollider>();
        if (Application.isPlaying) Object.Destroy(bc); else Object.DestroyImmediate(bc);
        string d = dishName.ToLowerInvariant();
        Color a = d.Contains("huevo") ? H("F7DD72") : d.Contains("ensalada") ? H("8CC474") : d.Contains("avena") ? H("E6D3A8")
                : d.Contains("yogur") ? H("F4F1EA") : d.Contains("quinoa") ? H("E9D9A8") : H("C98E5E");
        Color b = d.Contains("ensalada") ? H("E8665A") : d.Contains("yogur") || d.Contains("avena") ? H("E07A8A") : H("7FB86A");
        C(t, new Vector3(0f, 0.03f, 0f), new Vector3(0.14f, 0.025f, 0.12f), a, 15f, "Comida");
        C(t, new Vector3(0.03f, 0.045f, 0.02f), new Vector3(0.05f, 0.02f, 0.05f), b, 40f, "Guarnicion");
        return t;
    }
}
