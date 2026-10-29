using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// SOTOBOSQUE POLAR (docs/ice-sanctuary-ecology.md): solo crece al pie de los árboles carnívoros (su sombra y la materia
/// que dejan sus presas hacen de suelo), no por todo el hielo. Son especies REALES del Ártico, modificadas para verse
/// más, cada una con su forma reconocible hecha de bloques:
///   · Amapola ártica: roseta de hojas, tallos finos y flor en copa de 5 pétalos amarillo pálido.
///   · Saxífraga púrpura: cojín bajo cubierto de estrellitas lila.
///   · Silene acaulis (musgo campion): cúpula verde con puntos rosa.
///   · Dryas (aven de montaña): alfombra de hojitas y flores blancas de 8 pétalos con centro amarillo.
///   · Algodón ártico: mata de hojas y tallos con borla blanca de algodón.
///   · Sauce ártico: ramas leñosas rastreras con hojas ovaladas y amentos erguidos.
///   · Camarina: arbusto bajo de hojas aciculares con bayas.
///   · Liquen de reno: matas ramificadas gris verdoso.
/// Se genera al arrancar (determinista) y se fusiona en pocas mallas por trozo y color. Las manchas de sauce, camarina y
/// liquen además son <see cref="Affordance"/> (comida que se gasta y rebrota).
/// </summary>
public class PolarUnderstory : MonoBehaviour
{
    public int seed = 4242;
    public float chunkSize = 100f;
    public string groundName = "Casquete";
    [Tooltip("Plantas por metro de altura del árbol (un brote tiene pocas a su pie; un árbol viejo, muchas).")]
    public float plantsPerTreeMeter = 1.6f;

    enum C { Leaf, LeafDark, Stem, Wood, PetalYellow, PetalLilac, PetalPink, PetalWhite, Center, Cotton, Catkin, Berry, Lichen, Cushion }
    static readonly Color[] Palette =
    {
        new Color(0.62f, 0.76f, 0.62f), new Color(0.46f, 0.60f, 0.50f), new Color(0.64f, 0.74f, 0.56f), new Color(0.62f, 0.52f, 0.46f),
        new Color(0.99f, 0.93f, 0.60f), new Color(0.84f, 0.72f, 0.95f), new Color(0.98f, 0.74f, 0.84f), new Color(0.98f, 0.98f, 0.96f),
        new Color(0.98f, 0.80f, 0.35f), new Color(1.00f, 1.00f, 1.00f), new Color(0.95f, 0.82f, 0.86f), new Color(0.70f, 0.74f, 0.92f),
        new Color(0.80f, 0.85f, 0.78f), new Color(0.56f, 0.72f, 0.56f),
    };

    public int Plants { get; private set; }

    Mesh _cube;
    System.Random _rnd;
    Dictionary<int, List<CombineInstance>> _lists;
    int _chunk, _chunksPerSide;
    const float Half = 260f;

    float R(float a, float b) => a + (float)_rnd.NextDouble() * (b - a);

    void Start() { Generate(); }

    public void Generate()
    {
        _rnd = new System.Random(seed);
        GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _cube = tmp.GetComponent<MeshFilter>().sharedMesh;
        Destroy(tmp);
        _lists = new Dictionary<int, List<CombineInstance>>();
        _chunksPerSide = Mathf.CeilToInt(2f * Half / chunkSize);

        int cubes0 = 0;
        foreach (IceCarnivorousTree tree in FindObjectsByType<IceCarnivorousTree>(FindObjectsSortMode.None))
            AroundTree(tree.transform.position, TreeHeight(tree.transform));
        // Los brotes no llevan IceCarnivorousTree: también tienen su pequeño corro.
        foreach (Transform t in transform.parent != null ? transform.parent.GetComponentsInChildren<Transform>() : new Transform[0])
            if (t.name.StartsWith("ArbolCarnivoroHielo_Brote")) AroundTree(t.position, 4f);

        Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        foreach (var kv in _lists)
        {
            int chunk = kv.Key / 32, mat = kv.Key % 32;
            Material m = new Material(sh) { color = Palette[mat], name = $"Sotobosque_{(C)mat}" };
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", Palette[mat]);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.15f);
            Mesh mesh = new Mesh { indexFormat = IndexFormat.UInt32, name = $"Sotobosque_{chunk}_{(C)mat}" };
            mesh.CombineMeshes(kv.Value.ToArray(), true, true);
            mesh.RecalculateBounds();
            cubes0 += kv.Value.Count;
            GameObject g = new GameObject($"Sotobosque_{chunk}_{(C)mat}");
            g.transform.SetParent(transform, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer mr = g.AddComponent<MeshRenderer>();
            mr.sharedMaterial = m;
            mr.shadowCastingMode = ShadowCastingMode.Off;
        }
        Debug.Log($"[Flora] Sotobosque al pie de los árboles: {Plants} plantas árticas ({cubes0} bloques) — amapola, saxífraga, silene, dryas, algodón ártico, sauce ártico, camarina y liquen.");
    }

    static float TreeHeight(Transform t)
    {
        float top = t.position.y;
        foreach (Renderer r in t.GetComponentsInChildren<Renderer>()) top = Mathf.Max(top, r.bounds.max.y);
        return Mathf.Max(3f, top - t.position.y);
    }

    static readonly Collider[] _near = new Collider[16];
    /// <summary>¿No hay raíces ni tronco a menos de <paramref name="r"/> m? (donde come un animal grande cabe su cuerpo)</summary>
    static bool ClearOfTree(Vector3 p, float r)
    {
        int n = Physics.OverlapSphereNonAlloc(p + Vector3.up * 0.5f, r, _near, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++) if (_near[i] != null && (_near[i].name.StartsWith("Raiz") || _near[i].name.StartsWith("Tronco"))) return false;
        return true;
    }

    void AroundTree(Vector3 c, float height)
    {
        int n = Mathf.RoundToInt(height * plantsPerTreeMeter);
        float rMin = 1.5f + height * 0.08f, rMax = 4f + height * 0.45f;
        for (int i = 0; i < n; i++)
        {
            float a = R(0f, Mathf.PI * 2f), d = R(rMin, rMax);
            Vector3 p = c + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * d;
            if (!Physics.Raycast(p + Vector3.up * 6f, Vector3.down, out RaycastHit hit, 10f, ~0, QueryTriggerInteraction.Ignore)) continue;
            if (hit.collider.name != groundName || hit.point.y < -0.1f) continue;
            p.y = hit.point.y;
            _chunk = Mathf.Clamp(Mathf.FloorToInt((p.x + Half) / chunkSize), 0, _chunksPerSide - 1) * _chunksPerSide
                   + Mathf.Clamp(Mathf.FloorToInt((p.z + Half) / chunkSize), 0, _chunksPerSide - 1);
            Quaternion yaw = Quaternion.Euler(0f, R(0f, 360f), 0f);
            // Un pequeño grupo de la misma especie (así crecen: en manchas).
            int species = _rnd.Next(8), group = 1 + _rnd.Next(3);
            // Las que algo COME son interactuables (docs/natural-interactions.md): sauce ártico (liebre), camarina (bayas:
            // oso en verano) y liquen de reno (reno). Un punto por mancha; se gastan y rebrotan.
            AffordanceKind food = species == 5 ? AffordanceKind.Willow : species == 6 ? AffordanceKind.Berry : AffordanceKind.Lichen;
            // (solo las manchas con sitio alrededor: un oso que come camarina junto a una raíz del árbol se metía en la
            // «Raiz» — JornadaTest 2026-10-29; las raíces no tallan el NavMesh. La planta sigue ahí, de adorno.)
            if (species >= 5 && ClearOfTree(p, 2.5f))
            {
                var spot = new GameObject($"Planta_{food}");
                spot.transform.SetParent(transform, false); spot.transform.position = p;
                Affordance.Put(spot, food, group, group, 0.0005f);
            }
            for (int k = 0; k < group; k++)
            {
                Vector3 q = p + yaw * new Vector3(R(-0.7f, 0.7f), 0f, R(-0.7f, 0.7f));
                Quaternion y2 = Quaternion.Euler(0f, R(0f, 360f), 0f);
                switch (species)
                {
                    case 0: Poppy(q, y2); break;
                    case 1: Saxifrage(q, y2); break;
                    case 2: Campion(q, y2); break;
                    case 3: Dryas(q, y2); break;
                    case 4: Cottongrass(q, y2); break;
                    case 5: Willow(q, y2); break;
                    case 6: Crowberry(q, y2); break;
                    default: Lichen(q, y2); break;
                }
                Plants++;
            }
        }
    }

    // ── Especies ─────────────────────────────────────────────────────────────────────────────────

    void Poppy(Vector3 p, Quaternion yaw)
    {
        for (int i = 0; i < 5; i++)   // roseta basal
        {
            Quaternion r = yaw * Quaternion.Euler(-15f, i * 72f, 0f);
            Add(C.Leaf, p + r * Vector3.forward * 0.07f + Vector3.up * 0.02f, r, new Vector3(0.04f, 0.01f, 0.12f));
        }
        for (int s = 0, ns = 1 + _rnd.Next(3); s < ns; s++)
        {
            float h = R(0.18f, 0.32f), lean = R(-12f, 12f);
            Quaternion st = yaw * Quaternion.Euler(lean, s * 120f, 0f);
            Vector3 top = p + st * Vector3.up * h;
            Add(C.Stem, p + st * Vector3.up * h * 0.5f, st, new Vector3(0.012f, h, 0.012f));
            for (int k = 0; k < 5; k++)   // copa de 5 pétalos abiertos hacia arriba
            {
                Quaternion pr = st * Quaternion.Euler(0f, k * 72f, 0f) * Quaternion.Euler(-40f, 0f, 0f);
                Add(C.PetalYellow, top + pr * Vector3.forward * 0.035f, pr, new Vector3(0.045f, 0.008f, 0.06f));
            }
            Add(C.Center, top + Vector3.up * 0.01f, st, new Vector3(0.025f, 0.02f, 0.025f));
        }
    }

    void Saxifrage(Vector3 p, Quaternion yaw)
    {
        float w = R(0.25f, 0.45f);
        Add(C.Cushion, p + Vector3.up * 0.03f, yaw, new Vector3(w, 0.06f, w * 0.8f));
        Add(C.Cushion, p + Vector3.up * 0.07f, yaw * Quaternion.Euler(0f, 30f, 0f), new Vector3(w * 0.7f, 0.05f, w * 0.6f));
        for (int i = 0; i < 9; i++)   // estrellitas lila sobre el cojín
        {
            Vector3 o = yaw * new Vector3(R(-w * 0.4f, w * 0.4f), 0f, R(-w * 0.35f, w * 0.35f));
            Quaternion r = Quaternion.Euler(0f, R(0f, 90f), 0f);
            Add(C.PetalLilac, p + o + Vector3.up * 0.1f, r, new Vector3(0.05f, 0.012f, 0.015f));
            Add(C.PetalLilac, p + o + Vector3.up * 0.1f, r * Quaternion.Euler(0f, 90f, 0f), new Vector3(0.05f, 0.012f, 0.015f));
        }
    }

    void Campion(Vector3 p, Quaternion yaw)
    {
        float w = R(0.2f, 0.35f);
        Add(C.LeafDark, p + Vector3.up * 0.04f, yaw, new Vector3(w, 0.08f, w));
        Add(C.LeafDark, p + Vector3.up * 0.09f, yaw * Quaternion.Euler(0f, 45f, 0f), new Vector3(w * 0.75f, 0.05f, w * 0.75f));
        Add(C.LeafDark, p + Vector3.up * 0.12f, yaw, new Vector3(w * 0.45f, 0.03f, w * 0.45f));
        for (int i = 0; i < 10; i++)
        {
            float a = R(0f, Mathf.PI * 2f), d = R(0f, w * 0.45f);
            Vector3 o = new Vector3(Mathf.Cos(a) * d, 0.08f + (w * 0.45f - d) * 0.25f + 0.04f, Mathf.Sin(a) * d);
            Add(C.PetalPink, p + o, yaw, Vector3.one * 0.03f);
        }
    }

    void Dryas(Vector3 p, Quaternion yaw)
    {
        for (int i = 0; i < 10; i++)   // alfombra de hojitas
            Add(C.LeafDark, p + yaw * new Vector3(R(-0.25f, 0.25f), 0.015f, R(-0.25f, 0.25f)), yaw * Quaternion.Euler(0f, R(0f, 180f), 0f), new Vector3(0.04f, 0.015f, 0.07f));
        for (int f = 0, nf = 1 + _rnd.Next(2); f < nf; f++)
        {
            Vector3 b = p + yaw * new Vector3(R(-0.15f, 0.15f), 0f, R(-0.15f, 0.15f));
            float h = R(0.1f, 0.16f);
            Add(C.Stem, b + Vector3.up * h * 0.5f, yaw, new Vector3(0.01f, h, 0.01f));
            for (int k = 0; k < 8; k++)
            {
                Quaternion pr = yaw * Quaternion.Euler(0f, k * 45f, 0f) * Quaternion.Euler(-10f, 0f, 0f);
                Add(C.PetalWhite, b + Vector3.up * h + pr * Vector3.forward * 0.03f, pr, new Vector3(0.022f, 0.006f, 0.045f));
            }
            Add(C.Center, b + Vector3.up * (h + 0.008f), yaw, new Vector3(0.025f, 0.015f, 0.025f));
        }
    }

    void Cottongrass(Vector3 p, Quaternion yaw)
    {
        for (int i = 0; i < 6; i++)   // hojas finas
        {
            float h = R(0.12f, 0.25f);
            Quaternion t = yaw * Quaternion.Euler(R(-20f, 20f), i * 60f, R(-20f, 20f));
            Add(C.Leaf, p + t * Vector3.up * h * 0.5f, t, new Vector3(0.015f, h, 0.008f));
        }
        for (int s = 0, ns = 2 + _rnd.Next(3); s < ns; s++)   // tallos con borla de algodón
        {
            float h = R(0.3f, 0.45f);
            Quaternion t = yaw * Quaternion.Euler(R(-8f, 8f), s * 97f, R(-8f, 8f));
            Vector3 top = p + t * Vector3.up * h;
            Add(C.Stem, p + t * Vector3.up * h * 0.5f, t, new Vector3(0.01f, h, 0.01f));
            Add(C.Cotton, top + Vector3.up * 0.03f, t, new Vector3(0.06f, 0.07f, 0.06f));
            Add(C.Cotton, top + Vector3.up * 0.06f, t * Quaternion.Euler(0f, 45f, 0f), new Vector3(0.045f, 0.05f, 0.045f));
        }
    }

    void Willow(Vector3 p, Quaternion yaw)
    {
        for (int b = 0; b < 3; b++)   // ramas leñosas rastreras
        {
            Quaternion r = yaw * Quaternion.Euler(-5f, b * 120f + R(-20f, 20f), 0f);
            float len = R(0.35f, 0.6f);
            Add(C.Wood, p + r * Vector3.forward * len * 0.5f + Vector3.up * 0.03f, r, new Vector3(0.03f, 0.03f, len));
            for (int l = 0; l < 4; l++)   // hojas ovaladas a lo largo
            {
                Vector3 at = p + r * Vector3.forward * (len * (0.25f + l * 0.22f)) + Vector3.up * 0.06f;
                Add(C.Leaf, at, r * Quaternion.Euler(-20f, l % 2 == 0 ? 40f : -40f, 0f), new Vector3(0.05f, 0.01f, 0.08f));
            }
            Vector3 tip = p + r * Vector3.forward * len;   // amento erguido en la punta
            Add(C.Catkin, tip + Vector3.up * 0.1f, r, new Vector3(0.035f, 0.12f, 0.035f));
        }
    }

    void Crowberry(Vector3 p, Quaternion yaw)
    {
        float h = R(0.18f, 0.3f);
        for (int i = 0; i < 6; i++)   // tallitos con agujas
        {
            Quaternion t = yaw * Quaternion.Euler(R(-30f, 30f), i * 60f, 0f);
            Add(C.Wood, p + t * Vector3.up * h * 0.5f, t, new Vector3(0.02f, h, 0.02f));
            for (int k = 0; k < 3; k++)
                Add(C.LeafDark, p + t * Vector3.up * h * (0.4f + k * 0.25f), t * Quaternion.Euler(0f, k * 60f, 90f), new Vector3(0.012f, 0.06f, 0.012f));
        }
        for (int i = 0; i < 6; i++)
            Add(C.Berry, p + yaw * new Vector3(R(-0.12f, 0.12f), R(0.05f, h), R(-0.12f, 0.12f)), yaw, Vector3.one * 0.035f);
    }

    void Lichen(Vector3 p, Quaternion yaw)
    {
        for (int i = 0; i < 7; i++)   // matas ramificadas
        {
            Vector3 b = p + yaw * new Vector3(R(-0.2f, 0.2f), 0f, R(-0.2f, 0.2f));
            float h = R(0.05f, 0.12f);
            Quaternion t = yaw * Quaternion.Euler(R(-15f, 15f), R(0f, 180f), R(-15f, 15f));
            Add(C.Lichen, b + t * Vector3.up * h * 0.5f, t, new Vector3(0.03f, h, 0.03f));
            Add(C.Lichen, b + t * Vector3.up * h + t * new Vector3(0.02f, 0.015f, 0f), t * Quaternion.Euler(0f, 0f, 40f), new Vector3(0.018f, 0.05f, 0.018f));
            Add(C.Lichen, b + t * Vector3.up * h + t * new Vector3(-0.02f, 0.015f, 0f), t * Quaternion.Euler(0f, 0f, -40f), new Vector3(0.018f, 0.05f, 0.018f));
        }
    }

    void Add(C c, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        int key = _chunk * 32 + (int)c;
        if (!_lists.TryGetValue(key, out var l)) _lists[key] = l = new List<CombineInstance>();
        l.Add(new CombineInstance { mesh = _cube, transform = Matrix4x4.TRS(pos, rot, scale) });
    }
}
