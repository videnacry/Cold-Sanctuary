using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CULTIVO DEL HUERTO hecho de bloques: flora MODIFICADA del santuario (crece en el hielo y luce tonos claros, a juego
/// con el ambiente) para los ingredientes del restaurante: arroz, lenteja, banana, quinoa, tomate, espinaca, maíz, piña.
/// Tiene «mordiscos» (salud): un animal hambriento que llega a ella le quita uno y la planta encoge; si nadie la come,
/// rebrota poco a poco. Lo usa la misión «Alimentar a los visitantes» (FeedVisitorMission) para saber qué proteger.
/// </summary>
public class GardenCrop : MonoBehaviour
{
    public enum Kind { Arroz, Lenteja, Banana, Quinoa, Tomate, Espinaca, Maiz, Pina }

    public Kind kind;
    [Min(1)] public int maxBites = 3;
    public int bites;                       // mordiscos recibidos
    [Tooltip("Segundos para rebrotar un mordisco.")]
    public float regrowSeconds = 40f;

    public static readonly List<GardenCrop> All = new List<GardenCrop>();

    Vector3 _fullScale;
    float _regrow;

    public bool Eaten => bites >= maxBites;
    public string CropName => kind == Kind.Maiz ? "maíz" : kind == Kind.Pina ? "piña" : kind.ToString().ToLowerInvariant();

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

    /// <summary>El cultivo sano más cercano a <paramref name="p"/> dentro de <paramref name="radius"/>.</summary>
    public static GardenCrop Nearest(Vector3 p, float radius)
    {
        GardenCrop best = null; float bestSq = radius * radius;
        foreach (GardenCrop c in All)
        {
            if (c == null || c.Eaten) continue;
            float sq = (c.transform.position - p).sqrMagnitude;
            if (sq < bestSq) { bestSq = sq; best = c; }
        }
        return best;
    }

    // ── Construcción por bloques ─────────────────────────────────────────────────────────────────

    static readonly Dictionary<Color, Material> _mats = new Dictionary<Color, Material>();

    static Material Mat(Color c)
    {
        if (_mats.TryGetValue(c, out Material m) && m != null) return m;
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        m = new Material(sh) { color = c, name = $"Cultivo_{ColorUtility.ToHtmlStringRGB(c)}" };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        _mats[c] = m;
        return m;
    }

    // Paleta clara (flora modificada del hielo): verdes menta/salvia y frutos pastel.
    static readonly Color Leaf = new Color(0.66f, 0.84f, 0.70f), LeafPale = new Color(0.78f, 0.90f, 0.80f),
        Stalk = new Color(0.80f, 0.86f, 0.62f), Gold = new Color(0.95f, 0.88f, 0.60f), Pod = new Color(0.74f, 0.88f, 0.60f),
        Banana = new Color(0.98f, 0.93f, 0.58f), Trunk = new Color(0.72f, 0.78f, 0.66f), QuinoaHead = new Color(0.96f, 0.80f, 0.84f),
        Tomato = new Color(0.98f, 0.62f, 0.60f), Corn = new Color(0.99f, 0.90f, 0.55f), Husk = new Color(0.84f, 0.90f, 0.70f),
        Pine = new Color(0.98f, 0.84f, 0.52f), Paddy = new Color(0.62f, 0.80f, 0.88f);

    /// <summary>Crea la planta de <paramref name="kind"/> en <paramref name="pos"/> (suelo). <paramref name="scale"/> 1 = tamaño real.</summary>
    public static GardenCrop Build(Transform parent, Kind kind, Vector3 pos, int seed, float scale = 1f)
    {
        var rnd = new System.Random(seed);
        float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);

        GameObject root = new GameObject($"Cultivo_{kind}");
        root.transform.SetParent(parent, true);
        root.transform.position = pos;
        root.transform.rotation = Quaternion.Euler(0f, R(0f, 360f), 0f);
        root.transform.localScale = Vector3.one * scale;
        Transform t = root.transform;

        switch (kind)
        {
            case Kind.Arroz:   // arrozal: lámina de agua clara y matas de tallos finos con espigas doradas
                B(t, new Vector3(0f, 0.02f, 0f), new Vector3(1.4f, 0.04f, 1.4f), 0f, Paddy);
                for (int i = 0; i < 9; i++)
                {
                    Vector3 p = new Vector3((i % 3 - 1) * 0.42f + R(-0.08f, 0.08f), 0f, (i / 3 - 1) * 0.42f + R(-0.08f, 0.08f));
                    float h = R(0.7f, 1.0f);
                    B(t, p + Vector3.up * h * 0.5f, new Vector3(0.05f, h, 0.05f), R(-8f, 8f), Stalk);
                    B(t, p + new Vector3(0.06f, h - 0.05f, 0f), new Vector3(0.07f, 0.22f, 0.07f), 25f, Gold);
                }
                break;
            case Kind.Lenteja: // mata baja y tupida con vainas claras
                for (int i = 0; i < 6; i++)
                    B(t, new Vector3(R(-0.35f, 0.35f), R(0.15f, 0.4f), R(-0.35f, 0.35f)), Vector3.one * R(0.25f, 0.4f), R(0f, 45f), i % 2 == 0 ? Leaf : LeafPale);
                for (int i = 0; i < 7; i++)
                    B(t, new Vector3(R(-0.4f, 0.4f), R(0.3f, 0.55f), R(-0.4f, 0.4f)), new Vector3(0.16f, 0.06f, 0.06f), R(0f, 180f), Pod);
                break;
            case Kind.Banana:  // pseudotallo, grandes hojas arqueadas y racimo amarillo pálido
                B(t, new Vector3(0f, 1.1f, 0f), new Vector3(0.3f, 2.2f, 0.3f), 0f, Trunk);
                for (int i = 0; i < 6; i++)
                {
                    Quaternion q = Quaternion.Euler(-30f, i * 60f + R(-10f, 10f), 0f);
                    GameObject leaf = B(t, new Vector3(0f, 2.3f, 0f) + q * Vector3.forward * 0.75f, new Vector3(0.45f, 0.04f, 1.5f), 0f, i % 2 == 0 ? Leaf : LeafPale);
                    leaf.transform.localRotation = q;
                }
                for (int i = 0; i < 5; i++)
                    B(t, new Vector3(0.28f, 1.6f - i * 0.12f, 0.05f * (i % 2)), new Vector3(0.1f, 0.1f, 0.32f), 0f, Banana);
                break;
            case Kind.Quinoa:  // tallo alto con penacho rosado
                for (int i = 0; i < 3; i++)
                {
                    Vector3 p = new Vector3(R(-0.3f, 0.3f), 0f, R(-0.3f, 0.3f));
                    float h = R(1.1f, 1.5f);
                    B(t, p + Vector3.up * h * 0.5f, new Vector3(0.08f, h, 0.08f), 0f, Stalk);
                    B(t, p + Vector3.up * (h * 0.5f), new Vector3(0.35f, 0.05f, 0.18f), R(0f, 90f), Leaf);
                    B(t, p + Vector3.up * (h + 0.15f), new Vector3(0.22f, 0.4f, 0.22f), R(0f, 45f), QuinoaHead);
                }
                break;
            case Kind.Tomate:  // tutor, mata y tomates pastel
                B(t, new Vector3(0f, 0.7f, 0f), new Vector3(0.05f, 1.4f, 0.05f), 0f, Trunk);
                for (int i = 0; i < 6; i++)
                    B(t, new Vector3(R(-0.3f, 0.3f), R(0.3f, 1.1f), R(-0.3f, 0.3f)), Vector3.one * R(0.25f, 0.35f), R(0f, 45f), i % 2 == 0 ? Leaf : LeafPale);
                for (int i = 0; i < 5; i++)
                    B(t, new Vector3(R(-0.32f, 0.32f), R(0.35f, 1.0f), R(-0.32f, 0.32f)), Vector3.one * 0.16f, R(0f, 45f), Tomato);
                break;
            case Kind.Espinaca: // roseta baja de hojas anchas
                for (int i = 0; i < 7; i++)
                {
                    Quaternion q = Quaternion.Euler(-25f, i * 51f, 0f);
                    GameObject leaf = B(t, new Vector3(0f, 0.12f, 0f) + q * Vector3.forward * 0.22f, new Vector3(0.22f, 0.03f, 0.42f), 0f, i % 2 == 0 ? Leaf : LeafPale);
                    leaf.transform.localRotation = q;
                }
                break;
            case Kind.Maiz:    // caña alta, hojas en cinta y mazorca
                B(t, new Vector3(0f, 1.0f, 0f), new Vector3(0.1f, 2.0f, 0.1f), 0f, Stalk);
                for (int i = 0; i < 5; i++)
                {
                    Quaternion q = Quaternion.Euler(-35f, i * 137f, 0f);
                    GameObject leaf = B(t, new Vector3(0f, 0.5f + i * 0.3f, 0f) + q * Vector3.forward * 0.35f, new Vector3(0.12f, 0.03f, 0.75f), 0f, Leaf);
                    leaf.transform.localRotation = q;
                }
                B(t, new Vector3(0.13f, 1.2f, 0f), new Vector3(0.14f, 0.38f, 0.14f), -15f, Husk);
                B(t, new Vector3(0.15f, 1.32f, 0f), new Vector3(0.1f, 0.22f, 0.1f), -15f, Corn);
                break;
            case Kind.Pina:    // roseta de hojas en punta y la fruta encima
                for (int i = 0; i < 8; i++)
                {
                    Quaternion q = Quaternion.Euler(-45f, i * 45f, 0f);
                    GameObject leaf = B(t, new Vector3(0f, 0.25f, 0f) + q * Vector3.forward * 0.3f, new Vector3(0.1f, 0.03f, 0.65f), 0f, LeafPale);
                    leaf.transform.localRotation = q;
                }
                B(t, new Vector3(0f, 0.55f, 0f), new Vector3(0.3f, 0.42f, 0.3f), 45f, Pine);
                B(t, new Vector3(0f, 0.86f, 0f), new Vector3(0.14f, 0.22f, 0.14f), 0f, Leaf);
                break;
        }

        GardenCrop crop = root.AddComponent<GardenCrop>();
        crop.kind = kind;
        crop._fullScale = root.transform.localScale;
        return crop;
    }

    static GameObject B(Transform parent, Vector3 localPos, Vector3 size, float yaw, Color c)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = "Bloque";
        Collider col = g.GetComponent<Collider>();
        if (Application.isPlaying) Destroy(col); else DestroyImmediate(col);   // decorativo: no bloquea el paso
        g.transform.SetParent(parent, false);
        g.transform.localPosition = localPos;
        g.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = Mat(c);
        return g;
    }
}
