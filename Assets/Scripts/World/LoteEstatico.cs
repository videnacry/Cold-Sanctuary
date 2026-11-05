using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

/// <summary>
/// LOTE ESTÁTICO (docs/rendimiento.md; 2026-11-05: SampleScene en Play, 65 545 renderers y 3 FPS, y 51 596 de los 53 128 de la
/// escena guardada son el cubo primitivo). Al cargar una escena, EN PLAY, junta los cubos quietos en mallas por zona y las dibuja
/// en su lugar. La escena no cambia y NO hay que reconstruir nada:
/// - cada pieza sigue siendo su GameObject, con su nombre, su collider y su MeshRenderer ENCENDIDO (enabled = true): solo lleva
///   <see cref="Renderer.forceRenderingOff"/>. Lo que busca piezas por nombre, las pruebas que leen <c>r.enabled</c> y
///   <c>r.bounds</c>, y las copias de piso de los microcosmos (que se hacen en el editor) no notan nada.
/// - LOTES: por celda del mundo (o del «ancla», lo que se mueve entero: el árbol que gira, una alfombra), por material y por
///   tamaño. Lo opaco y liso de URP/Lit comparte además una PALETA (un texel por color), porque la escena tiene 1 134 materiales
///   que casi solo se diferencian en el color: una celda es entonces un dibujado o dos, no uno por color.
/// - DETALLE: las piezas de menos de <see cref="DetailSize"/> m van en lotes propios que se apagan a más de
///   <see cref="DetailDistance"/> m de la cámara (con la niebla del santuario, a esa distancia ya están medio borradas).
/// - AUTOCURACIÓN: una barrida (máx. <see cref="SweepMs"/> ms por frame) mira cada pieza: si alguien la apagó, la movió, le cambió
///   el material o la destruyó, sale del lote y vuelve a dibujarse sola. Una máscara que se coge del perchero o una puerta que
///   nadie previó siguen funcionando; solo tardan un momento.
/// - CUERPOS LEJANOS: animas y grupos de animales a más de <see cref="BodyDistance"/> m dejan de dibujarse, también con
///   forceRenderingOff (no pisa los <c>r.enabled</c> de InsectForm ni de AereoLink).
/// Apagar: <c>PlayerPrefs "ColdSanctuary.Lote" = 0</c> o, en vivo, <see cref="SetLive"/>.
/// </summary>
public class LoteEstatico : MonoBehaviour
{
    public const string PrefKey = "ColdSanctuary.Lote";
    /// <summary>Lado (m) de la celda de los lotes de DETALLE, y alto.</summary>
    public const float DetailCell = 20f, DetailCellY = 6f;
    /// <summary>Lado (m) de la celda de los lotes de ESTRUCTURA, y alto.</summary>
    public const float StructureCell = 48f, StructureCellY = 18f;
    /// <summary>Una pieza de menos de esto (m, su lado mayor en el mundo) es DETALLE.</summary>
    public const float DetailSize = 1.2f;
    /// <summary>Los lotes de detalle se apagan más allá de esto (m a la cámara, desde el borde del lote).</summary>
    public const float DetailDistance = 120f;
    /// <summary>Las animas y los grupos de animales dejan de dibujarse más allá de esto (m).</summary>
    public const float BodyDistance = 130f;
    const float Hysteresis = 8f;
    /// <summary>Tiempo máximo (ms por frame) de la barrida de autocuración.</summary>
    public const float SweepMs = 1f;
    const float MaxPieceSize = 60f;   // lo enorme (hielo, mar) se queda como está: son pocos y no hay nada que ganar
    const int MaxPieceVerts = 4096, RebuildsPerFrame = 3;
    const float TransparentMaxSize = 1.5f;   // un cristal grande se ordena mejor solo; un visor pequeño, no importa
    const int PaletteSize = 256;
    const string LitShader = "Universal Render Pipeline/Lit";

    public static LoteEstatico Instance { get; private set; }
    /// <summary>¿Está permitido en este equipo? (se lee al cargar cada escena).</summary>
    public static bool Allowed => PlayerPrefs.GetInt(PrefKey, 1) != 0;
    /// <summary>Lo que hay ahora: lotes, piezas en lote y piezas devueltas por la autocuración.</summary>
    public static int Groups => Instance != null ? Instance._groups.Count : 0;
    public static int Pieces => Instance != null ? Instance._all.Count : 0;
    public static int Ejected { get; private set; }
    public static int HiddenBodies => Instance != null ? Instance._hiddenBodies : 0;
    public static int HiddenDetail => Instance != null ? Instance._hiddenDetail : 0;
    public static string LastReport { get; private set; } = "";
    /// <summary>¿Dibuja ahora los lotes? (false tras <see cref="SetLive"/>(false)).</summary>
    public static bool Live => Instance != null && Instance._live;

    /// <summary>Tipos cuyo subárbol NO entra: se mueven, se animan o cambian solos. (La autocuración cubre lo demás, pero tarda.)</summary>
    static readonly System.Type[] Blocked =
    {
        typeof(Anima), typeof(Animator), typeof(CreatureRig), typeof(CubeJoint), typeof(Swarm), typeof(Hive), typeof(GardenCrop), typeof(FruitPlant),
        typeof(Phytoplankton), typeof(Rigidbody), typeof(NavMeshAgent), typeof(CharacterController), typeof(ConstructionProject),
        typeof(SlideDoor), typeof(PullDoor), typeof(SoundDoor), typeof(StationPart), typeof(MarineLift), typeof(CampoMagnetico),
        typeof(HospitalSubmarine), typeof(WhaleHospitalSubmarine), typeof(ConstructionSubmarine), typeof(TMP_Text), typeof(TextMesh),
    };
    /// <summary>Tipos que se mueven enteros y rígidos: sus lotes cuelgan de ellos y se mueven con ellos.</summary>
    static readonly System.Type[] Anchors = { typeof(TreeRotator), typeof(FlyingCarpet) };

    sealed class Piece
    {
        public MeshRenderer r; public Transform t; public GameObject go; public Material mat; public Mesh mesh;
        public Matrix4x4 toGroup; public Vector3 localPos; public Quaternion localRot;
        public bool usePalette, orphan; public Vector2 uv; public Group g;
    }

    sealed class Group
    {
        public Transform anchor; public Material mat; public bool detail; public ShadowCastingMode cast; public bool receive; public int layer;
        public LightProbeUsage probes; public ReflectionProbeUsage reflections;
        public string name; public Transform parent;
        public GameObject go; public MeshRenderer r; public Mesh mesh;
        public Vector3 center; public float radius;
        public readonly List<Piece> pieces = new List<Piece>();
        public bool hidden, dirty;
    }

    sealed class MeshData { public Vector3[] v, n; public Vector2[] uv; public int[] tri; }
    sealed class PaintInfo { public bool palette; public Material drawn; public Vector2 uv; public bool small; }
    sealed class Body { public Transform root; public bool hidden; public Renderer[] rs; public int children; }

    readonly List<Group> _groups = new List<Group>();
    readonly List<Group> _detail = new List<Group>();
    readonly List<Piece> _all = new List<Piece>();
    readonly List<Group> _dirty = new List<Group>();
    readonly Dictionary<Mesh, MeshData> _meshes = new Dictionary<Mesh, MeshData>();
    readonly Dictionary<Transform, Body> _bodies = new Dictionary<Transform, Body>();
    readonly List<Transform> _gone = new List<Transform>();
    readonly Stopwatch _sw = new Stopwatch();
    readonly List<Vector3> _v = new List<Vector3>(4096), _n = new List<Vector3>(4096);
    readonly List<Vector2> _uv = new List<Vector2>(4096);
    readonly List<int> _tri = new List<int>(8192);
    int _cursor, _hiddenBodies, _hiddenDetail;
    float _nextCull, _nextBodies, _nextBodyScan;
    bool _live = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (Instance != null) return;
        GameObject g = new GameObject("LoteEstatico_AUTO");
        DontDestroyOnLoad(g);
        Instance = g.AddComponent<LoteEstatico>();
        for (int i = 0; i < SceneManager.sceneCount; i++) Instance.StartCoroutine(Instance.AfterLoad(SceneManager.GetSceneAt(i)));
    }

    void OnEnable() { SceneManager.sceneLoaded += Loaded; }
    void OnDisable() { SceneManager.sceneLoaded -= Loaded; if (Instance == this) Instance = null; }
    void Loaded(Scene s, LoadSceneMode mode) { StartCoroutine(AfterLoad(s)); }

    // Tras los Start de la escena (colocan percheros, encienden luces, montan cubos): espera acotada, en tiempo real.
    IEnumerator AfterLoad(Scene s)
    {
        yield return new WaitForSecondsRealtime(1.5f);
        yield return null; yield return null;
        if (!Allowed || !s.IsValid() || !s.isLoaded) yield break;
        try { Pass(s); }
        catch (System.Exception e) { Debug.LogError($"[Rendimiento] LoteEstatico no pudo juntar «{s.name}»: {e}"); }
    }

    // ── La pasada: clasificar, pintar la paleta, construir ─────────────────────────────────────────────────────────────

    void Pass(Scene scene)
    {
        Stopwatch time = Stopwatch.StartNew();
        var blocked = new HashSet<Transform>();
        var anchors = new HashSet<Transform>();
        foreach (System.Type t in Blocked)
            foreach (Object o in FindObjectsByType(t, FindObjectsInactive.Include, FindObjectsSortMode.None)) if (o is Component c) blocked.Add(c.transform);
        foreach (System.Type t in Anchors)
            foreach (Object o in FindObjectsByType(t, FindObjectsInactive.Include, FindObjectsSortMode.None)) if (o is Component c) anchors.Add(c.transform);
        var memo = new Dictionary<Transform, (bool blocked, Transform anchor)>();

        var palette = new Dictionary<Color32, int>();
        var tex = new Texture2D(PaletteSize, PaletteSize, TextureFormat.RGBA32, false, false)
        { name = $"Lote_paleta_{scene.name}", filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, anisoLevel = 0 };
        var variants = new Dictionary<(int, int, int, Shader), Material>();
        var paints = new Dictionary<Material, PaintInfo>();
        var byKey = new Dictionary<(Transform, Material, int, int, int, bool, ShadowCastingMode, bool, int), Group>();
        var fresh = new List<Group>();
        GameObject container = null;
        int seen = 0;

        foreach (MeshRenderer r in FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (r.gameObject.scene != scene) continue;
            seen++;
            if (!r.enabled || r.forceRenderingOff || r.isPartOfStaticBatch || r.HasPropertyBlock()) continue;
            Material[] ms = r.sharedMaterials;
            if (ms.Length != 1 || ms[0] == null) continue;
            Material m = ms[0];
            if (!r.TryGetComponent(out MeshFilter mf)) continue;
            Mesh mesh = mf.sharedMesh;
            if (mesh == null || !mesh.isReadable || mesh.subMeshCount != 1 || mesh.vertexCount > MaxPieceVerts) continue;
            Transform t = r.transform;
            (bool isBlocked, Transform anchor) info = Lookup(t, blocked, anchors, memo);
            if (info.isBlocked) continue;
            Bounds wb = r.bounds;
            float size = Mathf.Max(wb.size.x, Mathf.Max(wb.size.y, wb.size.z));
            if (size > MaxPieceSize) continue;
            Matrix4x4 toGroup = info.anchor != null ? info.anchor.worldToLocalMatrix * t.localToWorldMatrix : t.localToWorldMatrix;
            if (Mathf.Abs(toGroup.determinant) < 1e-12f) continue;
            if (!paints.TryGetValue(m, out PaintInfo paint)) paints[m] = paint = Paint(m, palette, tex, variants);
            if (paint == null || (paint.small && size > TransparentMaxSize)) continue;

            bool detail = size < DetailSize;
            float cell = detail ? DetailCell : StructureCell, cellY = detail ? DetailCellY : StructureCellY;
            Vector3 c = info.anchor != null ? info.anchor.InverseTransformPoint(wb.center) : wb.center;
            var key = (info.anchor, paint.drawn, Mathf.FloorToInt(c.x / cell), Mathf.FloorToInt(c.y / cellY), Mathf.FloorToInt(c.z / cell), detail, r.shadowCastingMode, r.receiveShadows, r.gameObject.layer);
            if (!byKey.TryGetValue(key, out Group g))
            {
                if (container == null && info.anchor == null)
                {
                    container = new GameObject("Lote_AUTO");
                    SceneManager.MoveGameObjectToScene(container, scene);
                }
                byKey[key] = g = new Group
                {
                    anchor = info.anchor, mat = paint.drawn, detail = detail, cast = r.shadowCastingMode, receive = r.receiveShadows, layer = r.gameObject.layer,
                    probes = r.lightProbeUsage, reflections = r.reflectionProbeUsage,
                    name = $"Lote_{(detail ? "detalle" : "estructura")}_{key.Item3}_{key.Item4}_{key.Item5}_{paint.drawn.name}",
                    parent = info.anchor != null ? info.anchor : null,
                };
                fresh.Add(g);
            }
            g.pieces.Add(new Piece
            {
                r = r, t = t, go = r.gameObject, mat = m, mesh = mesh, toGroup = toGroup, g = g, usePalette = paint.palette, uv = paint.uv,
                localPos = info.anchor != null ? info.anchor.InverseTransformPoint(t.position) : Vector3.zero,
                localRot = info.anchor != null ? Quaternion.Inverse(info.anchor.rotation) * t.rotation : Quaternion.identity,
            });
        }
        tex.Apply(false, false);

        int pieces = 0, kept = 0;
        foreach (Group g in fresh)
        {
            if (g.pieces.Count < 2) { kept += g.pieces.Count; continue; }   // solo: no hay nada que juntar
            if (g.parent == null) g.parent = container.transform;
            Build(g);
            if (g.go == null) { kept += g.pieces.Count; continue; }
            _groups.Add(g);
            if (g.detail) _detail.Add(g);
            foreach (Piece p in g.pieces)
            {
                p.r.forceRenderingOff = _live;
                p.t.hasChanged = false;
                _all.Add(p);
            }
            pieces += g.pieces.Count;
            if (!_live && g.r != null) g.r.enabled = false;
        }
        LastReport = $"«{scene.name}»: {seen} renderers vistos, {pieces} piezas en {_groups.Count} lotes ({paints.Count} materiales → {variants.Count} con paleta de {palette.Count} colores), {time.ElapsedMilliseconds} ms";
        Debug.Log($"[Rendimiento] Lote estático {LastReport}.");
    }

    static (bool, Transform) Lookup(Transform t, HashSet<Transform> blocked, HashSet<Transform> anchors, Dictionary<Transform, (bool, Transform)> memo)
    {
        if (t == null) return (false, null);
        if (memo.TryGetValue(t, out var known)) return known;
        (bool isBlocked, Transform anchor) up = Lookup(t.parent, blocked, anchors, memo);
        (bool, Transform) res = up.isBlocked || blocked.Contains(t) ? (true, null) : anchors.Contains(t) ? (false, t) : up;
        memo[t] = res;
        return res;
    }

    // Cómo se dibuja este material en un lote. null = no entra (necesita tangentes para su mapa de normales).
    static PaintInfo Paint(Material m, Dictionary<Color32, int> palette, Texture2D tex, Dictionary<(int, int, int, Shader), Material> variants)
    {
        if (m.IsKeywordEnabled("_NORMALMAP")) return null;
        bool lit = m.shader != null && m.shader.name == LitShader;
        bool opaque = m.renderQueue < (int)RenderQueue.GeometryLast + 1 && (!m.HasProperty("_Surface") || m.GetFloat("_Surface") < 0.5f);
        if (m.renderQueue >= (int)RenderQueue.Transparent) return new PaintInfo { drawn = m, small = true };   // transparente: su material, solo lo pequeño
        if (!lit || !opaque || m.IsKeywordEnabled("_EMISSION") || m.IsKeywordEnabled("_ALPHATEST_ON") || m.IsKeywordEnabled("_SPECULAR_SETUP") ||
            HasTexture(m, "_BaseMap") || HasTexture(m, "_MetallicGlossMap") || HasTexture(m, "_OcclusionMap"))
            return new PaintInfo { drawn = m };
        Color c = m.GetColor("_BaseColor"); c.a = 1f;
        Color32 c32 = c;
        if (!palette.TryGetValue(c32, out int index))
        {
            if (palette.Count >= PaletteSize * PaletteSize) return new PaintInfo { drawn = m };
            index = palette.Count; palette[c32] = index;
            tex.SetPixel(index % PaletteSize, index / PaletteSize, c);
        }
        var key = (Mathf.RoundToInt(m.GetFloat("_Smoothness") * 20f), Mathf.RoundToInt(m.GetFloat("_Metallic") * 20f), m.HasProperty("_Cull") ? (int)m.GetFloat("_Cull") : 2, m.shader);
        if (!variants.TryGetValue(key, out Material drawn))
        {
            drawn = new Material(m) { name = $"Lote_paleta_{variants.Count}" };
            drawn.SetColor("_BaseColor", Color.white);
            drawn.SetTexture("_BaseMap", tex);
            variants[key] = drawn;
        }
        return new PaintInfo { palette = true, drawn = drawn, uv = new Vector2((index % PaletteSize + 0.5f) / PaletteSize, (index / PaletteSize + 0.5f) / PaletteSize) };
    }

    // (Un material sin textura puede devolver la blanca por defecto.)
    static bool HasTexture(Material m, string prop)
    {
        Texture t = m.GetTexture(prop);
        return t != null && t != Texture2D.whiteTexture && t.name != "UnityWhite";
    }

    MeshData Data(Mesh mesh)
    {
        if (mesh == null) return null;
        if (_meshes.TryGetValue(mesh, out MeshData d)) return d;
        d = new MeshData { v = mesh.vertices, n = mesh.normals, uv = mesh.uv, tri = mesh.triangles };
        if (d.n == null || d.n.Length != d.v.Length) { Mesh copy = Instantiate(mesh); copy.RecalculateNormals(); d.n = copy.normals; Destroy(copy); }
        if (d.uv == null || d.uv.Length != d.v.Length) d.uv = new Vector2[d.v.Length];
        return _meshes[mesh] = d;
    }

    // (Re)hace la malla del lote con sus piezas de ahora, en el espacio de su ancla (o del mundo).
    void Build(Group g)
    {
        _v.Clear(); _n.Clear(); _uv.Clear(); _tri.Clear();
        foreach (Piece p in g.pieces)
        {
            MeshData d = Data(p.mesh);
            if (d == null) continue;
            int b = _v.Count;
            Matrix4x4 m = p.toGroup, nm = m.inverse.transpose;
            for (int i = 0; i < d.v.Length; i++)
            {
                _v.Add(m.MultiplyPoint3x4(d.v[i]));
                _n.Add(nm.MultiplyVector(d.n[i]).normalized);
                _uv.Add(p.usePalette ? p.uv : d.uv[i]);
            }
            bool flip = m.determinant < 0f;   // una escala negativa da la vuelta a los triángulos: el renderer de la pieza lo corregía solo
            for (int i = 0; i + 2 < d.tri.Length; i += 3)
            {
                _tri.Add(b + d.tri[i]);
                _tri.Add(b + d.tri[flip ? i + 2 : i + 1]);
                _tri.Add(b + d.tri[flip ? i + 1 : i + 2]);
            }
        }
        Mesh old = g.mesh;
        g.mesh = null;
        if (_v.Count > 0)
        {
            var mesh = new Mesh { name = g.name, indexFormat = _v.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            mesh.SetVertices(_v); mesh.SetNormals(_n); mesh.SetUVs(0, _uv); mesh.SetTriangles(_tri, 0, true);
            g.center = mesh.bounds.center; g.radius = mesh.bounds.extents.magnitude;
            mesh.UploadMeshData(true);   // la copia de la CPU no hace falta: rehacer un lote parte de sus piezas
            g.mesh = mesh;
        }
        if (g.mesh != null)
        {
            if (g.go == null)
            {
                if (g.parent == null) { if (old != null) Destroy(old); return; }
                g.go = new GameObject(g.name) { layer = g.layer };
                g.go.transform.SetParent(g.parent, false);
                g.go.AddComponent<MeshFilter>();
                g.r = g.go.AddComponent<MeshRenderer>();
                g.r.sharedMaterial = g.mat; g.r.shadowCastingMode = g.cast; g.r.receiveShadows = g.receive;
                g.r.lightProbeUsage = g.probes; g.r.reflectionProbeUsage = g.reflections;
            }
            g.go.GetComponent<MeshFilter>().sharedMesh = g.mesh;
        }
        if (old != null) Destroy(old);
    }

    // ── En marcha: autocuración, detalle lejano, cuerpos lejanos ───────────────────────────────────────────────────────

    void Update()
    {
        if (!_live) return;
        Sweep();
        for (int i = 0; i < RebuildsPerFrame && _dirty.Count > 0; i++)
        {
            Group g = _dirty[_dirty.Count - 1];
            _dirty.RemoveAt(_dirty.Count - 1);
            g.dirty = false;
            Rebuild(g);
        }
        float now = Time.unscaledTime;
        if (now >= _nextCull) { _nextCull = now + 0.25f; CullDetail(); }
        if (now >= _nextBodies) { _nextBodies = now + 0.5f; CullBodies(now); }
    }

    void Sweep()
    {
        if (_all.Count == 0) return;
        long budget = (long)(SweepMs * 0.001f * Stopwatch.Frequency);
        int total = _all.Count, looked = 0;
        _sw.Restart();
        while (_all.Count > 0 && looked < total)
        {
            if (_cursor >= _all.Count) _cursor = 0;
            Piece p = _all[_cursor];
            looked++;
            if (!p.orphan && StillStatic(p)) _cursor++;
            else
            {
                if (!p.orphan) Eject(p);
                _all[_cursor] = _all[_all.Count - 1];
                _all.RemoveAt(_all.Count - 1);
            }
            if ((looked & 31) == 0 && _sw.ElapsedTicks >= budget) break;
        }
    }

    static bool StillStatic(Piece p)
    {
        if (p.r == null) return false;
        if (!p.go.activeInHierarchy) return p.g.go != null && !p.g.go.activeInHierarchy;   // se apagó con su ancla: el lote también
        if (!p.r.enabled || p.r.sharedMaterial != p.mat) return false;
        if (!p.t.hasChanged) return true;
        Transform a = p.g.anchor;
        if (a == null) return false;
        if ((a.InverseTransformPoint(p.t.position) - p.localPos).sqrMagnitude > 1e-4f) return false;
        if (Mathf.Abs(Quaternion.Dot(Quaternion.Inverse(a.rotation) * p.t.rotation, p.localRot)) < 0.9999f) return false;
        p.t.hasChanged = false;   // se movió el ancla, no la pieza
        return true;
    }

    void Eject(Piece p)
    {
        if (p.r != null) p.r.forceRenderingOff = false;
        p.g.pieces.Remove(p);
        Ejected++;
        if (!p.g.dirty) { p.g.dirty = true; _dirty.Add(p.g); }
    }

    void Rebuild(Group g)
    {
        if (g.pieces.Count > 0 && (g.go != null)) { Build(g); if (g.mesh != null) return; }
        foreach (Piece p in g.pieces) { if (p.r != null) p.r.forceRenderingOff = false; p.orphan = true; }   // (la barrida las quita de la lista)
        g.pieces.Clear();
        if (g.go != null) Destroy(g.go);
        if (g.mesh != null) Destroy(g.mesh);
        _groups.Remove(g); _detail.Remove(g);
    }

    void CullDetail()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        Vector3 at = cam.transform.position;
        bool all = Swarm.AnimateAll;
        int hidden = 0;
        for (int i = 0; i < _detail.Count; i++)
        {
            Group g = _detail[i];
            if (g.r == null) continue;
            Vector3 c = g.anchor != null ? g.anchor.TransformPoint(g.center) : g.center;
            float d = (c - at).magnitude - g.radius;
            bool hide = !all && d > DetailDistance + (g.hidden ? -Hysteresis : Hysteresis);
            if (hide != g.hidden) { g.hidden = hide; g.r.enabled = !hide; }
            if (hide) hidden++;
        }
        _hiddenDetail = hidden;
    }

    void CullBodies(float now)
    {
        if (now >= _nextBodyScan)
        {
            _nextBodyScan = now + 5f;
            foreach (Anima a in FindObjectsByType<Anima>(FindObjectsSortMode.None)) Add(a.transform);
            foreach (Swarm s in Swarm.All) if (s != null) Add(s.transform);
        }
        Camera cam = Camera.main;
        if (cam == null) return;
        Vector3 at = cam.transform.position;
        bool all = Swarm.AnimateAll;
        int hidden = 0;
        _gone.Clear();
        foreach (var kv in _bodies)
        {
            Transform t = kv.Key; Body b = kv.Value;
            if (t == null) { _gone.Add(t); continue; }
            float d = (t.position - at).sqrMagnitude, lim = (BodyDistance + (b.hidden ? -Hysteresis : Hysteresis)) * Mathf.Max(1f, t.lossyScale.y);   // un gigante se ve de más lejos
            bool hide = !all && d > lim * lim;
            // De lejos sigue naciendo gente y peces: lo nuevo también se oculta.
            if (hide != b.hidden || (hide && t.childCount != b.children))
            {
                b.hidden = hide; b.children = t.childCount;
                Set(b, hide);
            }
            if (hide) hidden++;
        }
        foreach (Transform t in _gone) _bodies.Remove(t);
        _hiddenBodies = hidden;
    }

    void Add(Transform t) { if (!_bodies.ContainsKey(t)) _bodies[t] = new Body { root = t, children = t.childCount }; }

    static void Set(Body b, bool off)
    {
        if (b.rs != null && !off) foreach (Renderer r in b.rs) if (r != null) r.forceRenderingOff = false;
        b.rs = b.root.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in b.rs) r.forceRenderingOff = off;
        if (!off) b.rs = null;
    }

    /// <summary>Medida A/B en vivo: false enseña las piezas de siempre y apaga lotes y recortes; true lo vuelve a poner.</summary>
    public static void SetLive(bool on)
    {
        if (Instance == null || Instance._live == on) return;
        LoteEstatico l = Instance;
        l._live = on;
        foreach (Piece p in l._all) if (p.r != null) p.r.forceRenderingOff = on;
        foreach (Group g in l._groups) { g.hidden = false; if (g.r != null) g.r.enabled = on; }
        foreach (Body b in l._bodies.Values) if (b.root != null && b.hidden) { b.hidden = false; Set(b, false); }
        l._hiddenBodies = 0; l._hiddenDetail = 0;
    }
}
