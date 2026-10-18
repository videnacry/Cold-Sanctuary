using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CULTIVO de bloques: flora MODIFICADA del santuario (crece en el hielo, tonos claros) que da los ingredientes
/// vegetales de TODOS los platillos del restaurante (docs/huertos-y-cultivos.md). Cada especie usa una FORMA real
/// (cereal, legumbre, roseta, bulbo, raíz, rizoma, mata con fruto, cactus, frutal, palmera, enredadera, hongo, alga…)
/// con sus colores y su tamaño. Las piezas se FUSIONAN en una malla por color: una planta = 2–4 renderers, no 30 cubos.
/// Tiene «mordiscos»: un visitante hambriento que llega le quita uno (encoge) y rebrota con el tiempo.
/// </summary>
public class GardenCrop : MonoBehaviour
{
    public enum Kind
    {
        // cereales y pseudocereales
        Arroz, Avena, Teff, Maiz, Quinoa, Amaranto,
        // legumbres
        Lenteja, Garbanzo, Frijol, Caupi, Soja, Mani, Tarwi,
        // hojas y aromáticas
        Espinaca, Col, Cilantro, Albahaca, Huacatay, Comino,
        // bulbos, raíces y rizomas
        Cebolla, Ajo, Cebollin, Zanahoria, Curcuma, Jengibre,
        // hortalizas de fruto
        Tomate, Chile, Pimiento, Berenjena, Calabaza, Sandia, Nopal,
        // tropicales herbáceas
        Banana, Pina,
        // frutales y árboles de especia
        Limon, Lima, Naranja, Mango, Guanabana, Almendro, Olivo, Cacao, Canela, Laurel,
        // palmeras, enredadera, fibra, hongo y alga
        Coco, Datil, Pimienta, Lino, Shiitake, Alga,
        // ampliación para la carta del restaurante (docs/cocina-profesional.md)
        Trigo, MaizMorado, Haba, Arveja, Melloco, Papa, Perejil, Romero, Azafran, Achiote, Sesamo, Wasabi, Pepino,
        Frutilla, Hierbaluisa, Cedron, Cana, Naranjilla, Mortino, Babaco, Clavo, Cafe, Aguacate,
    }

    public Kind kind;
    [Min(1)] public int maxBites = 3;
    public int bites;
    [Tooltip("Segundos para rebrotar un mordisco.")]
    public float regrowSeconds = 40f;

    public static readonly List<GardenCrop> All = new List<GardenCrop>();

    Vector3 _fullScale;
    float _regrow;

    public bool Eaten => bites >= maxBites;
    public string CropName => Names.TryGetValue(kind, out string n) ? n : kind.ToString().ToLowerInvariant();

    static readonly Dictionary<Kind, string> Names = new Dictionary<Kind, string>
    {
        { Kind.Maiz, "maíz" }, { Kind.Caupi, "caupí" }, { Kind.Mani, "maní" }, { Kind.Cebollin, "cebollín" }, { Kind.Curcuma, "cúrcuma" },
        { Kind.Sandia, "sandía" }, { Kind.Pina, "piña" }, { Kind.Limon, "limón" }, { Kind.Guanabana, "guanábana" }, { Kind.Datil, "dátil" },
        { Kind.Lenteja, "lenteja" }, { Kind.Frijol, "frijol" },
        { Kind.MaizMorado, "maíz morado" }, { Kind.Azafran, "azafrán" }, { Kind.Sesamo, "sésamo" }, { Kind.Cana, "caña de azúcar" },
        { Kind.Mortino, "mortiño" }, { Kind.Cedron, "cedrón" }, { Kind.Cafe, "café" },
    };

    void Awake() { _fullScale = transform.localScale; }
    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void Update()
    {
        if (bites <= 0) return;
        _regrow += Time.deltaTime;
        if (_regrow >= regrowSeconds) { _regrow = 0f; bites--; ApplyScale(); }
    }

    /// <summary>Un animal le da un mordisco. Devuelve false si ya no queda planta.</summary>
    public bool Bite(string who)
    {
        if (Eaten) return false;
        bites++; _regrow = 0f;
        ApplyScale();
        Debug.Log($"[Huerto] {who} muerde la planta de {CropName} ({bites}/{maxBites}).{(Eaten ? " ¡Se la ha comido entera!" : "")}");
        return true;
    }

    void ApplyScale()
    {
        if (_fullScale == Vector3.zero) _fullScale = transform.localScale;
        float k = Mathf.Lerp(1f, 0.25f, bites / (float)maxBites);
        transform.localScale = new Vector3(_fullScale.x, _fullScale.y * k, _fullScale.z);
    }

    /// <summary>El cultivo sano más cercano a <paramref name="p"/> dentro de <paramref name="radius"/> (sin árboles: un
    /// visitante no se come un mango entero).</summary>
    public static GardenCrop Nearest(Vector3 p, float radius)
    {
        GardenCrop best = null; float bestSq = radius * radius;
        foreach (GardenCrop c in All)
        {
            if (c == null || c.Eaten || IsTree(c.kind) || c.kind == Kind.Alga) continue;
            float sq = (c.transform.position - p).sqrMagnitude;
            if (sq < bestSq) { bestSq = sq; best = c; }
        }
        return best;
    }

    /// <summary>Cuánto le apetece este cultivo a una especie (DietPreference por nombre de cultivo; 1 = neutro).</summary>
    public static float Appetite(string species, GardenCrop c) => c == null || !DietPreference.Has(species, c.CropName) ? 0f : DietPreference.For(species, c.CropName);   // solo quien la tiene en su dieta

    /// <summary>El cultivo que más le APETECE a <paramref name="species"/> cerca (apetencia ÷ distancia), o null. Desdeña
    /// los que apenas le gustan (aromáticas, aliáceas) y los árboles y algas.</summary>
    public static GardenCrop BestFor(string species, Vector3 p, float radius)
    {
        GardenCrop best = null; float bestScore = 0f;
        foreach (GardenCrop c in All)
        {
            if (c == null || c.Eaten || IsTree(c.kind) || c.kind == Kind.Alga) continue;
            float d = Vector3.Distance(c.transform.position, p);
            if (d > radius) continue;
            float a = Appetite(species, c);
            if (a < 0.5f) continue;
            float score = a / Mathf.Max(1f, d);
            if (score > bestScore) { bestScore = score; best = c; }
        }
        return best;
    }

    public static bool IsTree(Kind k) => (k >= Kind.Limon && k <= Kind.Datil) || k == Kind.Babaco || k == Kind.Clavo || k == Kind.Cafe || k == Kind.Aguacate;

    // ── Construcción por bloques fusionados ─────────────────────────────────────────────────────

    static readonly Dictionary<Color, Material> _mats = new Dictionary<Color, Material>();
    static Mesh _cube;

    static Material Mat(Color c)
    {
        if (_mats.TryGetValue(c, out Material m) && m != null) return m;
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        m = new Material(sh) { color = c, name = $"Cultivo_{ColorUtility.ToHtmlStringRGB(c)}" };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.15f);
        _mats[c] = m;
        return m;
    }

    // Paleta clara (flora modificada del hielo).
    static Color Hex(string h) { ColorUtility.TryParseHtmlString("#" + h, out Color c); return c; }
    static readonly Color Leaf = Hex("A9D6B2"), LeafPale = Hex("C8E6CC"), LeafDeep = Hex("8FBF9C"), Stalk = Hex("CCDB9E"),
        Gold = Hex("F2E09A"), Wood = Hex("B9A391"), Bark = Hex("A8B4A6"), Soil = Hex("9E8C80"), White = Hex("F7F5EF");

    Dictionary<Color, List<CombineInstance>> _parts;

    void P(Vector3 pos, Vector3 size, Quaternion rot, Color c)
    {
        if (!_parts.TryGetValue(c, out var l)) _parts[c] = l = new List<CombineInstance>();
        l.Add(new CombineInstance { mesh = _cube, transform = Matrix4x4.TRS(pos, rot, size) });
    }
    void P(Vector3 pos, Vector3 size, Color c) => P(pos, size, Quaternion.identity, c);
    void P(Vector3 pos, Vector3 size, float yaw, Color c) => P(pos, size, Quaternion.Euler(0f, yaw, 0f), c);

    System.Random _rnd;
    float R(float a, float b) => a + (float)_rnd.NextDouble() * (b - a);

    /// <summary>Crea la planta en <paramref name="pos"/> (suelo). <paramref name="scale"/> 1 = tamaño real.</summary>
    public static GardenCrop Build(Transform parent, Kind kind, Vector3 pos, int seed, float scale = 1f)
    {
        if (_cube == null)
        {
            GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _cube = tmp.GetComponent<MeshFilter>().sharedMesh;
            if (Application.isPlaying) Destroy(tmp); else DestroyImmediate(tmp);
        }
        GameObject root = new GameObject($"Cultivo_{kind}");
        root.transform.SetParent(parent, true);
        root.transform.position = pos;
        var rnd = new System.Random(seed);
        root.transform.rotation = Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f);
        root.transform.localScale = Vector3.one * scale;

        GardenCrop crop = root.AddComponent<GardenCrop>();
        crop.kind = kind;
        crop._rnd = rnd;
        crop._parts = new Dictionary<Color, List<CombineInstance>>();
        crop.Shape(kind);
        crop.Bake();
        crop._fullScale = root.transform.localScale;
        if (IsTree(kind)) crop.maxBites = 12;
        return crop;
    }

    void Bake()
    {
        foreach (var kv in _parts)
        {
            Mesh m = new Mesh { name = $"{name}_{ColorUtility.ToHtmlStringRGB(kv.Key)}" };
            if (kv.Value.Count * 24 > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.CombineMeshes(kv.Value.ToArray(), true, true);
            m.RecalculateBounds();
            GameObject g = new GameObject("Malla");
            g.transform.SetParent(transform, false);
            g.AddComponent<MeshFilter>().sharedMesh = m;
            MeshRenderer mr = g.AddComponent<MeshRenderer>();
            mr.sharedMaterial = Mat(kv.Key);
            mr.shadowCastingMode = IsTree(kind) ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        _parts = null;
    }

    void Shape(Kind k)
    {
        switch (k)
        {
            case Kind.Arroz:     Paddy(); Cereal(9, 0.7f, 1.0f, Gold, true); break;
            case Kind.Avena:     Cereal(10, 0.8f, 1.1f, Hex("EDE3B5"), true); break;
            case Kind.Teff:      Cereal(14, 0.5f, 0.8f, Hex("E8CFB0"), true); break;
            case Kind.Maiz:      Maize(); break;
            case Kind.Quinoa:    Plume(3, 1.1f, 1.5f, Hex("F6CBD6")); break;
            case Kind.Amaranto:  Plume(3, 1.2f, 1.7f, Hex("E9A9C4")); break;

            case Kind.Lenteja:   LegumeBush(0.35f, 0.45f, Hex("BFE09A"), Hex("E6E0F5")); break;
            case Kind.Garbanzo:  LegumeBush(0.45f, 0.5f, Hex("D9E3A6"), Hex("F5E7F0")); break;
            case Kind.Frijol:    PoleVine(1.8f, Hex("B8D99A"), Hex("F2D4E0")); break;
            case Kind.Caupi:     LegumeBush(0.5f, 0.6f, Hex("C9E3A3"), Hex("E8DAF5")); break;
            case Kind.Soja:      LegumeBush(0.6f, 0.55f, Hex("BEDFA0"), Hex("EAD9F2")); break;
            case Kind.Mani:      LegumeBush(0.25f, 0.5f, Hex("C6DDA4"), Hex("F9E79A")); break;
            case Kind.Tarwi:     Lupin(); break;

            case Kind.Espinaca:  Rosette(7, 0.22f, 0.42f, 0.05f, Leaf, LeafPale); break;
            case Kind.Col:       Cabbage(); break;
            case Kind.Cilantro:  Herb(0.35f, Hex("B6DDB0"), White, true); break;
            case Kind.Albahaca:  Herb(0.4f, Hex("9ED3A6"), White, false); break;
            case Kind.Huacatay:  Herb(0.9f, Hex("B2D4A0"), Hex("F3EBC2"), false); break;
            case Kind.Comino:    Herb(0.35f, Hex("C3DCB2"), Hex("F7EFF5"), true); break;

            case Kind.Cebolla:   Allium(Hex("F2E3EC"), 0.12f, 0.55f); break;
            case Kind.Ajo:       Allium(White, 0.09f, 0.5f); break;
            case Kind.Cebollin:  Allium(LeafPale, 0f, 0.45f); break;
            case Kind.Zanahoria: Root(Hex("F7C39A")); break;
            case Kind.Curcuma:   Rhizome(1.0f, Hex("F6D27A")); break;
            case Kind.Jengibre:  Rhizome(0.9f, Hex("EBD8B5")); break;

            case Kind.Tomate:    FruitBush(1.3f, Hex("F7A8A2"), 0.15f, true); break;
            case Kind.Chile:     FruitBush(0.6f, Hex("F49B8F"), 0.06f, false, true); break;
            case Kind.Pimiento:  FruitBush(0.7f, Hex("F9D38A"), 0.13f, false); break;
            case Kind.Berenjena: FruitBush(0.8f, Hex("C9B3E8"), 0.16f, false, true); break;
            case Kind.Calabaza:  GroundVine(Hex("F8D29B"), 0.45f); break;
            case Kind.Sandia:    GroundVine(Hex("BFE3B8"), 0.55f); break;
            case Kind.Nopal:     Cactus(); break;

            case Kind.Banana:    Banana(); break;
            case Kind.Pina:      Pineapple(); break;

            case Kind.Limon:     FruitTree(3.0f, 2.4f, LeafDeep, Hex("FAF0A0"), 0.12f); break;
            case Kind.Lima:      FruitTree(2.8f, 2.2f, LeafDeep, Hex("CDEBA6"), 0.1f); break;
            case Kind.Naranja:   FruitTree(3.5f, 2.6f, LeafDeep, Hex("FBCB8E"), 0.14f); break;
            case Kind.Mango:     FruitTree(5.5f, 3.6f, Leaf, Hex("FAD691"), 0.16f); break;
            case Kind.Guanabana: FruitTree(4.0f, 2.6f, LeafDeep, Hex("CDE6B5"), 0.26f); break;
            case Kind.Almendro:  FruitTree(4.0f, 3.0f, LeafPale, Hex("F7E4EC"), 0.07f); break;   // en flor
            case Kind.Olivo:     FruitTree(3.5f, 3.0f, Hex("C5D3C2"), Hex("B7C49A"), 0.06f); break;
            case Kind.Cacao:     Cacao(); break;
            case Kind.Canela:    FruitTree(5.0f, 2.4f, Hex("B4D2A4"), Hex("E8C9AE"), 0f); break;
            case Kind.Laurel:    FruitTree(4.0f, 2.2f, LeafDeep, Hex("E9EAD0"), 0f); break;
            case Kind.Coco:      Palm(7f, Hex("C9B79A"), 0.22f); break;
            case Kind.Datil:     Palm(6f, Hex("E8C38E"), 0.05f); break;

            case Kind.Pimienta:  PoleVine(2.4f, LeafDeep, Hex("F2C9A0")); break;
            case Kind.Lino:      Flax(); break;
            case Kind.Shiitake:  MushroomLog(); break;
            case Kind.Alga:      Seaweed(); break;

            case Kind.Trigo:     Cereal(12, 0.9f, 1.1f, Hex("F0D98C"), true); break;
            case Kind.MaizMorado: Maize(); break;
            case Kind.Haba:      LegumeBush(0.7f, 0.5f, Hex("B5D9A0"), Hex("F2F2F2")); break;
            case Kind.Arveja:    PoleVine(1.3f, Hex("BFE3A8"), Hex("F3E2F0")); break;
            case Kind.Melloco:   Rosette(6, 0.18f, 0.3f, 0.04f, LeafDeep, Hex("F6E58A")); break;
            case Kind.Papa:      LegumeBush(0.6f, 0.6f, Leaf, Hex("EAD9F2")); break;
            case Kind.Perejil:   Herb(0.3f, Hex("A3D6A0"), LeafPale, true); break;
            case Kind.Romero:    Herb(0.7f, Hex("A9C4B0"), Hex("D9D4F2"), false); break;
            case Kind.Azafran:   Allium(Hex("D8C6EE"), 0.05f, 0.25f); break;
            case Kind.Achiote:   FruitBush(1.6f, Hex("F2A08C"), 0.1f, false); break;
            case Kind.Sesamo:    Plume(4, 0.8f, 1.2f, Hex("F4EBD8")); break;
            case Kind.Wasabi:    Rosette(5, 0.25f, 0.3f, 0.05f, Leaf, LeafPale); break;
            case Kind.Pepino:    GroundVine(Hex("C2E3A8"), 0.25f); break;
            case Kind.Frutilla:  Rosette(6, 0.16f, 0.2f, 0.03f, Leaf, Hex("F7A3A8")); break;
            case Kind.Hierbaluisa: Herb(1.0f, Hex("B8DCA6"), Hex("F0EAF7"), false); break;
            case Kind.Cedron:    Herb(1.1f, Hex("B0D3A0"), White, false); break;
            case Kind.Cana:      Cereal(6, 1.8f, 2.6f, Hex("D6E3A8"), false); break;
            case Kind.Naranjilla: FruitBush(1.4f, Hex("F8C98A"), 0.12f, false); break;
            case Kind.Mortino:   FruitBush(0.6f, Hex("B9B3E8"), 0.04f, false, true); break;
            case Kind.Babaco:    FruitTree(2.2f, 1.2f, Leaf, Hex("F4EBA0"), 0.2f); break;
            case Kind.Clavo:     FruitTree(4.5f, 2.2f, LeafDeep, Hex("F2C2B0"), 0.03f); break;
            case Kind.Cafe:      FruitTree(2.4f, 1.8f, LeafDeep, Hex("F2A3A0"), 0.05f); break;
            case Kind.Aguacate:  FruitTree(5.0f, 3.4f, LeafDeep, Hex("B9D49A"), 0.14f); break;
        }
    }

    // ── Formas ──────────────────────────────────────────────────────────────────────────────────

    void Paddy() { P(new Vector3(0f, 0.02f, 0f), new Vector3(1.4f, 0.04f, 1.4f), Hex("A9D3E6")); }

    void Cereal(int n, float hMin, float hMax, Color head, bool nod)
    {
        for (int i = 0; i < n; i++)
        {
            Vector3 b = new Vector3(R(-0.45f, 0.45f), 0f, R(-0.45f, 0.45f));
            float h = R(hMin, hMax);
            Quaternion t = Quaternion.Euler(R(-6f, 6f), R(0f, 360f), R(-6f, 6f));
            P(b + t * Vector3.up * h * 0.5f, new Vector3(0.04f, h, 0.04f), t, Stalk);
            P(b + t * Vector3.up * h * 0.35f + t * Vector3.right * 0.08f, new Vector3(0.18f, 0.015f, 0.04f), t * Quaternion.Euler(0f, 0f, -25f), LeafPale);
            Quaternion hq = t * Quaternion.Euler(nod ? 35f : 0f, 0f, 0f);
            P(b + t * Vector3.up * h + hq * Vector3.up * 0.1f, new Vector3(0.06f, 0.22f, 0.05f), hq, head);
        }
    }

    void Maize()
    {
        P(new Vector3(0f, 1.0f, 0f), new Vector3(0.1f, 2.0f, 0.1f), Stalk);
        for (int i = 0; i < 6; i++)
        {
            Quaternion q = Quaternion.Euler(0f, i * 137f, 0f) * Quaternion.Euler(-35f, 0f, 0f);
            P(new Vector3(0f, 0.45f + i * 0.28f, 0f) + q * Vector3.forward * 0.38f, new Vector3(0.12f, 0.02f, 0.78f), q, Leaf);
        }
        P(new Vector3(0.13f, 1.15f, 0f), new Vector3(0.14f, 0.38f, 0.14f), Quaternion.Euler(0f, 0f, -15f), Hex("D6E4B0"));
        P(new Vector3(0.16f, 1.3f, 0f), new Vector3(0.1f, 0.2f, 0.1f), Quaternion.Euler(0f, 0f, -15f), Hex("FBE59A"));
        for (int i = 0; i < 4; i++)   // penacho
            P(new Vector3(0f, 2.1f, 0f), new Vector3(0.02f, 0.3f, 0.02f), Quaternion.Euler(30f, i * 90f, 0f), Gold);
    }

    void Plume(int n, float hMin, float hMax, Color plume)
    {
        for (int i = 0; i < n; i++)
        {
            Vector3 b = new Vector3(R(-0.3f, 0.3f), 0f, R(-0.3f, 0.3f));
            float h = R(hMin, hMax);
            P(b + Vector3.up * h * 0.5f, new Vector3(0.07f, h, 0.07f), Stalk);
            for (int l = 0; l < 3; l++)
                P(b + Vector3.up * (h * (0.3f + l * 0.18f)), new Vector3(0.3f, 0.02f, 0.16f), R(0f, 180f), Leaf);
            P(b + Vector3.up * (h + 0.18f), new Vector3(0.22f, 0.42f, 0.22f), R(0f, 45f), plume);
            P(b + Vector3.up * (h + 0.42f), new Vector3(0.12f, 0.15f, 0.12f), R(0f, 45f), plume);
        }
    }

    void LegumeBush(float h, float w, Color pod, Color flower)
    {
        for (int i = 0; i < 8; i++)   // follaje de hojuelas
            P(new Vector3(R(-w, w) * 0.6f, R(0.08f, h), R(-w, w) * 0.6f), new Vector3(0.12f, 0.02f, 0.08f), R(0f, 180f), i % 2 == 0 ? Leaf : LeafPale);
        for (int i = 0; i < 5; i++)
            P(new Vector3(R(-w, w) * 0.5f, h * 0.5f, R(-w, w) * 0.5f), new Vector3(0.02f, h, 0.02f), Quaternion.Euler(R(-15f, 15f), 0f, R(-15f, 15f)), Stalk);
        for (int i = 0; i < 7; i++)   // vainas
            P(new Vector3(R(-w, w) * 0.55f, R(h * 0.3f, h * 0.9f), R(-w, w) * 0.55f), new Vector3(0.14f, 0.035f, 0.04f), Quaternion.Euler(0f, R(0f, 180f), R(-40f, 40f)), pod);
        for (int i = 0; i < 4; i++)   // flores
            P(new Vector3(R(-w, w) * 0.5f, h + 0.03f, R(-w, w) * 0.5f), Vector3.one * 0.04f, flower);
    }

    void Lupin()
    {
        LegumeBush(0.6f, 0.5f, Hex("D2E3B3"), LeafPale);
        for (int s = 0; s < 3; s++)   // espigas de flores azul pálido
        {
            Vector3 b = new Vector3(R(-0.2f, 0.2f), 0.6f, R(-0.2f, 0.2f));
            for (int k = 0; k < 6; k++) P(b + Vector3.up * (k * 0.06f), new Vector3(0.09f - k * 0.01f, 0.04f, 0.09f - k * 0.01f), Hex("C7D3F5"));
        }
    }

    void PoleVine(float h, Color pod, Color flower)
    {
        P(new Vector3(0f, h * 0.5f, 0f), new Vector3(0.04f, h, 0.04f), Wood);   // tutor
        for (int i = 0; i < 14; i++)
        {
            float y = R(0.1f, h);
            Vector3 o = new Vector3(Mathf.Cos(y * 6f) * 0.12f, y, Mathf.Sin(y * 6f) * 0.12f);
            P(o, new Vector3(0.14f, 0.015f, 0.12f), R(0f, 180f), i % 2 == 0 ? Leaf : LeafPale);
            if (i % 3 == 0) P(o + Vector3.down * 0.08f, new Vector3(0.03f, 0.16f, 0.03f), pod);
            if (i % 5 == 0) P(o + Vector3.up * 0.04f, Vector3.one * 0.035f, flower);
        }
    }

    void Rosette(int n, float w, float len, float h, Color a, Color b)
    {
        for (int i = 0; i < n; i++)
        {
            Quaternion q = Quaternion.Euler(0f, i * 360f / n, 0f) * Quaternion.Euler(-25f, 0f, 0f);
            P(new Vector3(0f, h + 0.06f, 0f) + q * Vector3.forward * len * 0.5f, new Vector3(w, 0.025f, len), q, i % 2 == 0 ? a : b);
        }
        P(new Vector3(0f, h + 0.05f, 0f), new Vector3(0.08f, 0.1f, 0.08f), LeafPale);
    }

    void Cabbage()
    {
        Rosette(8, 0.35f, 0.5f, 0.02f, LeafDeep, Leaf);
        P(new Vector3(0f, 0.2f, 0f), new Vector3(0.36f, 0.32f, 0.36f), 0f, Hex("D8EDD5"));
        P(new Vector3(0f, 0.22f, 0f), new Vector3(0.3f, 0.3f, 0.4f), 45f, Hex("CDE8CB"));
    }

    void Herb(float h, Color leaf, Color flower, bool lacy)
    {
        for (int s = 0; s < 6; s++)
        {
            Quaternion t = Quaternion.Euler(R(-15f, 15f), s * 60f, R(-15f, 15f));
            float hh = h * R(0.7f, 1f);
            P(t * Vector3.up * hh * 0.5f, new Vector3(0.02f, hh, 0.02f), t, Stalk);
            for (int l = 0; l < 3; l++)
                P(t * Vector3.up * hh * (0.35f + l * 0.22f), lacy ? new Vector3(0.1f, 0.01f, 0.1f) : new Vector3(0.09f, 0.03f, 0.13f), t * Quaternion.Euler(0f, l * 70f, 0f), leaf);
            if (lacy) P(t * Vector3.up * (hh + 0.02f), new Vector3(0.09f, 0.02f, 0.09f), t, flower);   // umbela
            else if (s % 2 == 0) P(t * Vector3.up * (hh + 0.05f), new Vector3(0.04f, 0.1f, 0.04f), t, flower);
        }
    }

    void Allium(Color bulb, float bulbSize, float h)
    {
        if (bulbSize > 0f) P(new Vector3(0f, bulbSize * 0.4f, 0f), Vector3.one * bulbSize, 45f, bulb);
        for (int i = 0; i < 6; i++)
        {
            Quaternion t = Quaternion.Euler(R(-10f, 10f), i * 60f, R(-10f, 10f));
            P(t * Vector3.up * h * 0.5f + Vector3.up * bulbSize * 0.5f, new Vector3(0.025f, h, 0.025f), t, LeafPale);
        }
    }

    void Root(Color shoulder)
    {
        P(new Vector3(0f, 0.03f, 0f), new Vector3(0.07f, 0.06f, 0.07f), shoulder);   // asoma el cuello de la raíz
        for (int i = 0; i < 7; i++)   // hojas plumosas
        {
            Quaternion t = Quaternion.Euler(R(-25f, 25f), i * 51f, 0f);
            P(t * Vector3.up * 0.18f + Vector3.up * 0.05f, new Vector3(0.015f, 0.32f, 0.015f), t, Stalk);
            for (int k = 0; k < 3; k++)
                P(t * Vector3.up * (0.15f + k * 0.07f) + Vector3.up * 0.05f, new Vector3(0.08f, 0.008f, 0.03f), t * Quaternion.Euler(0f, k * 60f, 0f), LeafPale);
        }
    }

    void Rhizome(float h, Color root)
    {
        P(new Vector3(0.08f, 0.03f, 0f), new Vector3(0.18f, 0.06f, 0.08f), 20f, root);
        for (int i = 0; i < 6; i++)
        {
            Quaternion t = Quaternion.Euler(R(-12f, 12f), i * 60f, 0f);
            P(t * Vector3.up * h * 0.35f, new Vector3(0.03f, h * 0.7f, 0.03f), t, Stalk);
            P(t * Vector3.up * h * 0.75f + t * Vector3.forward * 0.12f, new Vector3(0.1f, 0.02f, 0.36f), t * Quaternion.Euler(-50f, 0f, 0f), i % 2 == 0 ? Leaf : LeafPale);
        }
    }

    void FruitBush(float h, Color fruit, float fruitSize, bool staked, bool hanging = false)
    {
        if (staked) P(new Vector3(0f, h * 0.5f, 0f), new Vector3(0.04f, h, 0.04f), Wood);
        for (int i = 0; i < 4; i++)
            P(new Vector3(R(-0.1f, 0.1f), h * 0.4f, R(-0.1f, 0.1f)), new Vector3(0.025f, h * 0.8f, 0.025f), Quaternion.Euler(R(-20f, 20f), 0f, R(-20f, 20f)), Stalk);
        for (int i = 0; i < 9; i++)
            P(new Vector3(R(-0.3f, 0.3f), R(h * 0.3f, h), R(-0.3f, 0.3f)), new Vector3(0.2f, 0.12f, 0.2f) * R(0.8f, 1.3f), R(0f, 90f), i % 2 == 0 ? Leaf : LeafPale);
        for (int i = 0; i < 6; i++)
        {
            Vector3 at = new Vector3(R(-0.3f, 0.3f), R(h * 0.25f, h * 0.85f), R(-0.3f, 0.3f));
            Vector3 size = hanging ? new Vector3(fruitSize * 0.6f, fruitSize * 1.6f, fruitSize * 0.6f) : Vector3.one * fruitSize;
            P(at, size, R(0f, 45f), fruit);
        }
    }

    void GroundVine(Color fruit, float fruitSize)
    {
        for (int b = 0; b < 4; b++)
        {
            Quaternion r = Quaternion.Euler(0f, b * 90f + R(-20f, 20f), 0f);
            P(r * Vector3.forward * 0.6f + Vector3.up * 0.03f, new Vector3(0.03f, 0.03f, 1.2f), r, Stalk);
            for (int l = 0; l < 4; l++)
                P(r * Vector3.forward * (0.2f + l * 0.3f) + Vector3.up * 0.12f, new Vector3(0.3f, 0.02f, 0.28f), r * Quaternion.Euler(R(-15f, 15f), R(0f, 90f), 0f), l % 2 == 0 ? Leaf : LeafPale);
        }
        P(new Vector3(0.35f, fruitSize * 0.45f, 0.2f), new Vector3(fruitSize * 1.2f, fruitSize * 0.9f, fruitSize), 20f, fruit);
        P(new Vector3(-0.4f, fruitSize * 0.4f, -0.35f), new Vector3(fruitSize, fruitSize * 0.8f, fruitSize * 1.1f), 70f, fruit);
    }

    void Cactus()
    {
        Color pad = Hex("B5D8B9");
        P(new Vector3(0f, 0.3f, 0f), new Vector3(0.35f, 0.5f, 0.07f), 0f, pad);
        P(new Vector3(0.2f, 0.72f, 0f), new Vector3(0.3f, 0.42f, 0.06f), Quaternion.Euler(0f, 25f, -20f), pad);
        P(new Vector3(-0.18f, 0.7f, 0.05f), new Vector3(0.28f, 0.38f, 0.06f), Quaternion.Euler(0f, -30f, 18f), pad);
        P(new Vector3(0.05f, 1.05f, 0f), new Vector3(0.24f, 0.32f, 0.05f), Quaternion.Euler(0f, 60f, 5f), pad);
        for (int i = 0; i < 6; i++)   // tunas
            P(new Vector3(R(-0.3f, 0.3f), R(0.9f, 1.25f), R(-0.05f, 0.05f)), new Vector3(0.07f, 0.1f, 0.07f), Hex("F5B5C8"));
    }

    void Banana()
    {
        P(new Vector3(0f, 1.1f, 0f), new Vector3(0.3f, 2.2f, 0.3f), Hex("C4D1B4"));
        for (int i = 0; i < 7; i++)
        {
            Quaternion q = Quaternion.Euler(0f, i * 51f, 0f) * Quaternion.Euler(-30f, 0f, 0f);
            P(new Vector3(0f, 2.3f, 0f) + q * Vector3.forward * 0.8f, new Vector3(0.5f, 0.04f, 1.6f), q, i % 2 == 0 ? Leaf : LeafPale);
        }
        for (int i = 0; i < 6; i++)
            P(new Vector3(0.28f, 1.7f - i * 0.11f, 0.05f * (i % 2)), new Vector3(0.1f, 0.1f, 0.32f), Quaternion.Euler(0f, 0f, 20f), Hex("FAEE98"));
        P(new Vector3(0.3f, 1.05f, 0f), new Vector3(0.12f, 0.2f, 0.12f), Hex("E6B7D2"));   // bellota (flor)
    }

    void Pineapple()
    {
        for (int i = 0; i < 10; i++)
        {
            Quaternion q = Quaternion.Euler(0f, i * 36f, 0f) * Quaternion.Euler(-45f, 0f, 0f);
            P(new Vector3(0f, 0.25f, 0f) + q * Vector3.forward * 0.3f, new Vector3(0.1f, 0.03f, 0.65f), q, LeafPale);
        }
        P(new Vector3(0f, 0.58f, 0f), new Vector3(0.3f, 0.44f, 0.3f), 45f, Hex("FAD889"));
        P(new Vector3(0f, 0.58f, 0f), new Vector3(0.32f, 0.38f, 0.26f), 0f, Hex("F3CC7A"));
        for (int i = 0; i < 5; i++)
            P(new Vector3(0f, 0.9f, 0f), new Vector3(0.05f, 0.25f, 0.05f), Quaternion.Euler(25f, i * 72f, 0f), Leaf);
    }

    void FruitTree(float h, float crown, Color canopy, Color fruit, float fruitSize)
    {
        float trunkH = h * 0.45f;
        P(new Vector3(0f, trunkH * 0.5f, 0f), new Vector3(0.22f, trunkH, 0.22f) * Mathf.Max(1f, h / 4f), Bark);
        for (int b = 0; b < 4; b++)   // ramas principales
        {
            Quaternion q = Quaternion.Euler(0f, b * 90f + R(-20f, 20f), 0f) * Quaternion.Euler(-45f, 0f, 0f);
            P(new Vector3(0f, trunkH, 0f) + q * Vector3.forward * crown * 0.25f, new Vector3(0.1f, 0.1f, crown * 0.5f), q, Bark);
        }
        for (int i = 0; i < 9; i++)   // copa en bloques
        {
            Vector3 o = new Vector3(R(-crown, crown) * 0.42f, trunkH + R(0.3f, h - trunkH), R(-crown, crown) * 0.42f);
            P(o, Vector3.one * R(crown * 0.35f, crown * 0.55f), R(0f, 90f), i % 3 == 0 ? LeafPale : canopy);
        }
        if (fruitSize > 0f)
            for (int i = 0; i < 12; i++)
            {
                float a = R(0f, Mathf.PI * 2f);
                Vector3 o = new Vector3(Mathf.Cos(a) * crown * 0.5f, trunkH + R(0.2f, h - trunkH - 0.2f), Mathf.Sin(a) * crown * 0.5f);
                P(o, Vector3.one * fruitSize, R(0f, 45f), fruit);
            }
    }

    void Cacao()
    {
        FruitTree(4.5f, 2.6f, LeafDeep, Hex("F2D1A6"), 0f);
        for (int i = 0; i < 7; i++)   // mazorcas pegadas al tronco (cauliflora)
        {
            float a = i * 51f * Mathf.Deg2Rad;
            P(new Vector3(Mathf.Cos(a) * 0.2f, 0.5f + i * 0.2f, Mathf.Sin(a) * 0.2f), new Vector3(0.12f, 0.24f, 0.12f), i * 51f, Hex("F2C59B"));
        }
    }

    void Palm(float h, Color fruit, float fruitSize)
    {
        for (int i = 0; i < 8; i++)   // tronco anillado y algo curvo
            P(new Vector3(i * 0.04f, h / 8f * (i + 0.5f), 0f), new Vector3(0.32f - i * 0.015f, h / 8f * 1.02f, 0.32f - i * 0.015f), i * 15f, i % 2 == 0 ? Hex("CDBDA6") : Hex("BFAE96"));
        Vector3 top = new Vector3(0.32f, h, 0f);
        for (int i = 0; i < 9; i++)
        {
            Quaternion q = Quaternion.Euler(0f, i * 40f, 0f) * Quaternion.Euler(-20f, 0f, 0f);
            P(top + q * Vector3.forward * 1.2f + Vector3.down * 0.25f, new Vector3(0.4f, 0.03f, 2.4f), q * Quaternion.Euler(15f, 0f, 0f), i % 2 == 0 ? Leaf : LeafPale);
        }
        for (int i = 0; i < 6; i++)
            P(top + new Vector3(R(-0.25f, 0.25f), -0.3f - R(0f, 0.2f), R(-0.25f, 0.25f)), Vector3.one * Mathf.Max(0.06f, fruitSize), R(0f, 45f), fruit);
    }

    void Flax()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 b = new Vector3(R(-0.3f, 0.3f), 0f, R(-0.3f, 0.3f));
            float h = R(0.6f, 0.85f);
            P(b + Vector3.up * h * 0.5f, new Vector3(0.015f, h, 0.015f), Stalk);
            for (int k = 0; k < 5; k++)
                P(b + Vector3.up * h, new Vector3(0.025f, 0.006f, 0.05f), Quaternion.Euler(-15f, k * 72f, 0f), Hex("BCD3F7"));
        }
    }

    void MushroomLog()
    {
        P(new Vector3(0f, 0.15f, 0f), new Vector3(1.4f, 0.28f, 0.28f), Quaternion.Euler(0f, 0f, 0f), Wood);
        P(new Vector3(0f, 0.15f, 0.3f), new Vector3(1.3f, 0.26f, 0.26f), Quaternion.Euler(0f, 8f, 0f), Wood);
        for (int i = 0; i < 9; i++)
        {
            Vector3 at = new Vector3(R(-0.6f, 0.6f), 0.3f, R(-0.05f, 0.35f));
            P(at, new Vector3(0.04f, 0.06f, 0.04f), White);
            P(at + Vector3.up * 0.04f, new Vector3(0.14f, 0.04f, 0.14f), R(0f, 45f), Hex("D9BFA6"));
        }
    }

    void Seaweed()
    {
        for (int i = 0; i < 6; i++)
        {
            Vector3 b = new Vector3(R(-0.4f, 0.4f), -1.2f, R(-0.4f, 0.4f));
            float h = R(0.8f, 1.4f);
            for (int s = 0; s < 4; s++)   // cinta ondulada que llega a la superficie
                P(b + new Vector3(Mathf.Sin(s + i) * 0.06f, h * (s + 0.5f) / 4f, 0f), new Vector3(0.12f, h / 4f, 0.02f), Quaternion.Euler(0f, R(0f, 180f), R(-10f, 10f)), Hex("A9C9A0"));
        }
    }
}
