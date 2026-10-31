using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// MAR HASTA EL HORIZONTE (Berón 2026-10-31: «desde lo alto del árbol se ven los bordes de la escena»). Lo que se veía
/// era el FIN del plano del mar (1200 m) y, detrás, la mitad de abajo del cielo (marrón): el mar es transparente y debajo
/// no había nada, y más allá del plano de recorte de la cámara (1000 m) tampoco. Con la niebla (exp², 0,008) todo lo que
/// está a más de ~300 m ya es color niebla, así que no hace falta un mar de 20 km: basta con que NUNCA se vea el vacío.
/// Solo visual, sin colliders (salvo el límite), un objeto por cosa:
///  · SUPERFICIE: el plano del mar de la escena («Sea_Placeholder») sigue a la cámara (rejilla de 10 m) y llega hasta casi
///    el recorte. El volumen nadable (Sea_SwimZone, WaterZone) y el fondo físico NO se mueven: siguen siendo 1200 m.
///  · FONDO PROFUNDO: un cuenco opaco azul oscuro bajo el agua (plano a −16 m alrededor de la cámara, que sube hasta
///    rozar la superficie en el borde): lo que se ve por los canales es mar hondo y no el cielo.
///  · BANDA DEL HORIZONTE: un cilindro sin luz, del color de la niebla, del agua al horizonte y que se desvanece 3° por
///    encima: tapa lo que queda entre el fin del mar y el horizonte y funde el mar con el cielo. Como toma la niebla de
///    la escena, bajo el agua se tiñe sola.
///  · TÉMPANOS e ICEBERGS lejanos (una malla, sin colliders) al otro lado del límite, que el nadador ve al acercarse.
///  · LÍMITE: cuatro paredes invisibles en el borde del agua nadable (±600 m), capa «Ignore Raycast», para que nadie
///    salga nadando al vacío.
/// Si la cámara está lejos del santuario (microcosmos y galerías, a ≥ 5 km, que se cargan encima en aditivo) todo se
/// apaga y el mar vuelve a su sitio.
/// </summary>
public class MarHorizonte : MonoBehaviour
{
    /// <summary>Mitad del lado del agua nadable (Sea_SwimZone es de 1200 m).</summary>
    public const float PlayHalf = 600f;
    /// <summary>Más lejos que esto del origen la cámara está en un microcosmos o una galería: el mar infinito se apaga.</summary>
    public const float NearWorld = 2500f;
    const float FadeDeg = 3f, Grid = 10f, Depth = 16f, FlatFraction = 0.25f;

    /// <summary>Para comparar antes/después (fotos y coste): false lo apaga todo y deja el mar como estaba.</summary>
    public static bool Mostrar = true;
    public static MarHorizonte Instance { get; private set; }

    Transform _sea; Vector3 _seaPos, _seaScale; float _seaY;
    Transform _floor, _band, _bergs;
    Renderer _floorR, _bandR, _bergsR;
    Mesh _bandMesh; Material _bandMat;
    float _bandCamY = float.NaN, _bandRadius = -1f;
    bool _shown;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Hook() { SceneManager.sceneLoaded -= OnLoaded; SceneManager.sceneLoaded += OnLoaded; }

    static void OnLoaded(Scene s, LoadSceneMode mode)
    {
        if (s.name != "SampleScene" || Instance != null) return;
        var go = new GameObject("MarHorizonte_AUTO");
        SceneManager.MoveGameObjectToScene(go, s);
        go.AddComponent<MarHorizonte>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        GameObject sea = GameObject.Find("Sea_Placeholder");
        if (sea == null || sea.scene != gameObject.scene) { Debug.LogWarning("[MarHorizonte] no hay Sea_Placeholder en SampleScene: sin mar infinito."); enabled = false; return; }
        _sea = sea.transform; _seaPos = _sea.position; _seaScale = _sea.localScale; _seaY = _seaPos.y;

        _floor = Part("Mar_FondoProfundo", BowlMesh(_seaY - Depth, _seaY - 0.5f), Lit(new Color(0.04f, 0.14f, 0.20f), 0f), out _floorR);
        _bandMesh = new Mesh { name = "Mar_Horizonte" }; _bandMesh.MarkDynamic();
        _bandMat = BandMaterial();
        _band = Part("Mar_Horizonte", _bandMesh, _bandMat, out _bandR);
        _bergs = Part("Mar_Tempanos_lejanos", BergsMesh(_seaY), Lit(new Color(0.86f, 0.92f, 0.97f), 0.35f), out _bergsR);
        BuildLimit();
        _shown = true;   // empieza apagado hasta que una cámara lo vea
        Show(false);
    }

    void OnEnable() { RenderPipelineManager.beginCameraRendering += BeforeCamera; }
    void OnDisable() { RenderPipelineManager.beginCameraRendering -= BeforeCamera; Show(false); }
    void OnDestroy() { if (Instance == this) Instance = null; }

    Transform Part(string name, Mesh mesh, Material mat, out Renderer r)
    {
        var go = new GameObject(name); go.transform.SetParent(transform, false);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer mr = go.AddComponent<MeshRenderer>(); mr.sharedMaterial = mat;
        mr.shadowCastingMode = ShadowCastingMode.Off; mr.receiveShadows = false;
        mr.lightProbeUsage = LightProbeUsage.Off; mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
        mr.allowOcclusionWhenDynamic = false;
        r = mr; return go.transform;
    }

    // Cada cámara (la del juego, la de la vista de escena, las de las fotos) ve su propio horizonte: se coloca justo antes
    // de que esa cámara recorte y dibuje.
    void BeforeCamera(ScriptableRenderContext ctx, Camera cam)
    {
        if (_sea == null || cam.cameraType == CameraType.Preview || cam.cameraType == CameraType.Reflection) return;
        Vector3 c = cam.transform.position;
        bool on = Mostrar && new Vector2(c.x, c.z).magnitude < NearWorld;
        Show(on);
        if (!on) return;

        float R = Mathf.Clamp(cam.farClipPlane * 0.85f, 250f, 4000f);   // dentro del recorte (que mide en profundidad, no en radio)
        Vector3 p = _sea.parent != null ? _sea.parent.lossyScale : Vector3.one;
        _sea.position = new Vector3(Mathf.Round(c.x / Grid) * Grid, _seaY, Mathf.Round(c.z / Grid) * Grid);
        float s = Mathf.Max(R * 2f / 10f, _seaScale.x);                // el Plane mide 10 m
        _sea.localScale = new Vector3(s / Mathf.Max(p.x, 1e-4f), _seaScale.y, s / Mathf.Max(p.z, 1e-4f));

        _floor.position = new Vector3(c.x, 0f, c.z); _floor.localScale = new Vector3(R, 1f, R);
        _band.position = new Vector3(c.x, 0f, c.z); _band.localScale = Vector3.one;
        float rb = R * 0.98f;
        if (Mathf.Abs(c.y - _bandCamY) > 0.25f || !Mathf.Approximately(rb, _bandRadius)) { BandVerts(rb, c.y); _bandCamY = c.y; _bandRadius = rb; }
        Color fog = RenderSettings.fogColor; fog.a = 1f;
        _bandMat.SetColor("_BaseColor", fog);
    }

    void Show(bool on)
    {
        if (_floorR == null || on == _shown) return;
        _shown = on;
        _floorR.enabled = _bandR.enabled = _bergsR.enabled = on;
        if (!on && _sea != null) { _sea.position = _seaPos; _sea.localScale = _seaScale; }
    }

    // ── Mallas ───────────────────────────────────────────────────────────────────────────────────────────────

    const int Seg = 64;

    /// <summary>Cuenco de radio 1 (se escala por el radio): llano a <paramref name="deep"/> hasta el 25 % y sube hasta
    /// <paramref name="rim"/> en el borde.</summary>
    static Mesh BowlMesh(float deep, float rim)
    {
        float[] rs = { 0f, FlatFraction, 0.45f, 0.65f, 0.85f, 1f };
        var v = new List<Vector3>(); var t = new List<int>();
        for (int i = 0; i < rs.Length; i++)
        {
            float k = Mathf.InverseLerp(FlatFraction, 1f, rs[i]);
            float y = Mathf.Lerp(deep, rim, k * k);
            for (int j = 0; j <= Seg; j++) { float a = j * Mathf.PI * 2f / Seg; v.Add(new Vector3(Mathf.Cos(a) * rs[i], y, Mathf.Sin(a) * rs[i])); }
        }
        for (int i = 0; i < rs.Length - 1; i++)
            for (int j = 0; j < Seg; j++)
            {
                int a = i * (Seg + 1) + j, b = a + 1, c = a + Seg + 1, d = c + 1;
                t.Add(a); t.Add(b); t.Add(c); t.Add(b); t.Add(d); t.Add(c);   // cara hacia arriba
            }
        var m = new Mesh { name = "Mar_FondoProfundo" }; m.SetVertices(v); m.SetTriangles(t, 0);
        m.RecalculateNormals(); m.bounds = new Bounds(new Vector3(0f, (deep + rim) * 0.5f, 0f), new Vector3(2f, Mathf.Abs(rim - deep) + 1f, 2f));
        return m;
    }

    /// <summary>La banda: tres anillos (bajo el agua, a la altura de la cámara = el horizonte, y 3° por encima), mirando
    /// hacia dentro. La textura pone la opacidad: entera hasta el horizonte y luego se desvanece.</summary>
    void BandVerts(float R, float camY)
    {
        float y0 = _seaY - 1f, y1 = Mathf.Max(camY, y0 + 0.1f), y2 = y1 + R * Mathf.Tan(FadeDeg * Mathf.Deg2Rad);
        float[] ys = { y0, y1, y2 }, vs = { 0f, 0.5f, 1f };
        var v = new Vector3[3 * (Seg + 1)]; var uv = new Vector2[v.Length];
        for (int i = 0; i < 3; i++)
            for (int j = 0; j <= Seg; j++)
            {
                float a = j * Mathf.PI * 2f / Seg;
                v[i * (Seg + 1) + j] = new Vector3(Mathf.Cos(a) * R, ys[i], Mathf.Sin(a) * R);
                uv[i * (Seg + 1) + j] = new Vector2((float)j / Seg, vs[i]);
            }
        _bandMesh.vertices = v; _bandMesh.uv = uv;
        if (_bandMesh.GetIndexCount(0) == 0)
        {
            var t = new int[2 * Seg * 6]; int n = 0;
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < Seg; j++)
                {
                    int a = i * (Seg + 1) + j, b = a + 1, c = a + Seg + 1, d = c + 1;
                    t[n++] = a; t[n++] = b; t[n++] = c; t[n++] = b; t[n++] = d; t[n++] = c;
                }
            _bandMesh.triangles = t;
        }
        _bandMesh.RecalculateBounds();
    }

    /// <summary>Icebergs y témpanos fuera del límite (600–900 m del centro), deterministas, en una sola malla de caras planas.</summary>
    static Mesh BergsMesh(float sea)
    {
        var rnd = new System.Random(1979);
        float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
        var v = new List<Vector3>(); var t = new List<int>();
        void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d) { int i = v.Count; v.Add(a); v.Add(b); v.Add(c); v.Add(d); t.Add(i); t.Add(i + 2); t.Add(i + 1); t.Add(i); t.Add(i + 3); t.Add(i + 2); }
        void Block(Vector3 o, float r, float h, float top, int sides, float bottom)
        {
            var lo = new Vector3[sides]; var hi = new Vector3[sides]; float rot = R(0f, 6.28f);
            for (int i = 0; i < sides; i++)
            {
                float a = rot + i * Mathf.PI * 2f / sides, k = R(0.75f, 1.15f);
                Vector3 d = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                lo[i] = o + d * r * k * 1.08f + Vector3.up * bottom;
                hi[i] = o + d * r * k * top * R(0.8f, 1.1f) + Vector3.up * (h * R(0.75f, 1f));
            }
            for (int i = 0; i < sides; i++) { int j = (i + 1) % sides; Quad(lo[i], lo[j], hi[j], hi[i]); }
            int c0 = v.Count; v.Add(o + Vector3.up * h);
            for (int i = 0; i < sides; i++) v.Add(hi[i]);
            for (int i = 0; i < sides; i++) { t.Add(c0); t.Add(c0 + 1 + (i + 1) % sides); t.Add(c0 + 1 + i); }
        }
        for (int i = 0; i < 16; i++)       // icebergs: 6–34 m sobre el agua
        {
            float a = (i + R(-0.3f, 0.3f)) * Mathf.PI * 2f / 16f, d = R(680f, 900f);
            Block(new Vector3(Mathf.Cos(a) * d, sea, Mathf.Sin(a) * d), R(14f, 38f), R(6f, 34f), R(0.4f, 0.7f), 7, -5f);
        }
        for (int i = 0; i < 48; i++)       // banquisa suelta: placas bajas justo fuera del límite
        {
            float a = R(0f, Mathf.PI * 2f), d = R(615f, 760f);
            Block(new Vector3(Mathf.Cos(a) * d, sea, Mathf.Sin(a) * d), R(5f, 18f), 0.45f, R(0.9f, 1f), 6, -0.8f);
        }
        // Caras planas: cada triángulo con sus vértices (normales duras, aspecto de hielo tallado).
        var fv = new Vector3[t.Count]; var ft = new int[t.Count];
        for (int i = 0; i < t.Count; i++) { fv[i] = v[t[i]]; ft[i] = i; }
        var m = new Mesh { name = "Mar_Tempanos_lejanos", indexFormat = fv.Length > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
        m.vertices = fv; m.triangles = ft; m.RecalculateNormals(); m.RecalculateBounds();
        return m;
    }

    // ── Materiales ───────────────────────────────────────────────────────────────────────────────────────────

    static Material Lit(Color c, float smooth)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(sh) { name = "Mar_" + c };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        m.color = c;
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
        return m;
    }

    static Material BandMaterial()
    {
        var tex = new Texture2D(4, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "Mar_Horizonte_alfa" };
        var px = new Color32[4 * 64];
        for (int y = 0; y < 64; y++)
        {
            float v = y / 63f, a = v <= 0.5f ? 1f : 1f - Mathf.SmoothStep(0f, 1f, (v - 0.5f) / 0.5f);
            for (int x = 0; x < 4; x++) px[y * 4 + x] = new Color32(255, 255, 255, (byte)(a * 255f));
        }
        tex.SetPixels32(px); tex.Apply(false, true);
        Shader sh = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Transparent");
        var m = new Material(sh) { name = "Mar_Horizonte" };
        if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
        m.mainTexture = tex;
        if (m.HasProperty("_Surface"))
        {
            m.SetFloat("_Surface", 1f); m.SetFloat("_Blend", 0f);
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_SrcBlendAlpha", (float)BlendMode.One); m.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f); m.SetFloat("_Cull", 0f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.SetOverrideTag("RenderType", "Transparent");
        }
        // Lo primero de lo transparente: nada transparente más cercano (agua, cristal, el tronco de acrílico) queda tapado.
        m.renderQueue = (int)RenderQueue.GeometryLast + 1;
        return m;
    }

    // ── Límite ───────────────────────────────────────────────────────────────────────────────────────────────

    void BuildLimit()
    {
        var root = new GameObject("Limite_del_mar"); root.transform.SetParent(transform, false);
        root.layer = 2;   // Ignore Raycast: frena cuerpos (CharacterController, rigidbodies) pero no estorba a rayos ni a la cámara
        const float th = 4f, lo = -20f, hi = 500f;
        float h = hi - lo, cy = (hi + lo) * 0.5f, L = PlayHalf * 2f + th * 2f;
        (Vector3 c, Vector3 s)[] w =
        {
            (new Vector3(0f, cy,  PlayHalf + th * 0.5f), new Vector3(L, h, th)),
            (new Vector3(0f, cy, -PlayHalf - th * 0.5f), new Vector3(L, h, th)),
            (new Vector3( PlayHalf + th * 0.5f, cy, 0f), new Vector3(th, h, L)),
            (new Vector3(-PlayHalf - th * 0.5f, cy, 0f), new Vector3(th, h, L)),
        };
        foreach (var (c, s) in w) { BoxCollider b = root.AddComponent<BoxCollider>(); b.center = c; b.size = s; }
    }

    /// <summary>¿El punto está dentro del límite del mar? (lo usan las pruebas)</summary>
    public static bool InsideLimit(Vector3 p) => Mathf.Abs(p.x) < PlayHalf && Mathf.Abs(p.z) < PlayHalf;
}
