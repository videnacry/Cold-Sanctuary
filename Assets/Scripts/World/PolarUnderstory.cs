using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// SOTOBOSQUE POLAR del santuario (docs/ice-sanctuary-ecology.md): la flora local del polo existe pero es pequeña y
/// recóndita; aquí está MODIFICADA para mostrarse y dar al santuario aspecto de bosque, ocupando el sitio de la hierba,
/// los arbustos y las flores: matas de hierba de escarcha, arbustos de liquen, alfombras de musgo y flores polares
/// (amapola ártica, saxífraga, silene) en tonos CLAROS que casan con el hielo. Todo de bloques.
/// Se genera al arrancar (determinista por semilla) y se FUSIONA en pocas mallas por trozo y color: decenas de miles de
/// bloques no pueden ser GameObjects sueltos ni guardarse en la escena. Solo crece sobre hielo libre (rayo que cae
/// directamente en un «Casquete»): no sale dentro de las áreas, bajo los árboles ni sobre el agua.
/// </summary>
public class PolarUnderstory : MonoBehaviour
{
    public int seed = 4242;
    public float half = 250f;
    [Tooltip("Separación media entre puntos de siembra (m). Menor = más denso.")]
    public float spacing = 3.2f;
    public float chunkSize = 100f;
    public string groundName = "Casquete";

    static readonly Color[] Palette =
    {
        new Color(0.72f, 0.86f, 0.74f), new Color(0.82f, 0.91f, 0.80f), new Color(0.66f, 0.82f, 0.78f),   // 0-2 hierba
        new Color(0.64f, 0.74f, 0.64f), new Color(0.79f, 0.76f, 0.89f), new Color(0.83f, 0.87f, 0.85f),   // 3-5 arbustos
        new Color(0.98f, 0.79f, 0.86f), new Color(0.87f, 0.79f, 0.97f), new Color(0.98f, 0.94f, 0.66f), new Color(0.97f, 0.97f, 0.95f), // 6-9 pétalos
        new Color(0.98f, 0.84f, 0.45f), new Color(0.62f, 0.78f, 0.60f), new Color(0.74f, 0.84f, 0.68f),  // 10 centro, 11 tallo, 12 musgo
    };

    public int Plants { get; private set; }

    void Start() { Generate(); }

    public void Generate()
    {
        var rnd = new System.Random(seed);
        float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);

        GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh cube = tmp.GetComponent<MeshFilter>().sharedMesh;
        Destroy(tmp);

        Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material[] mats = new Material[Palette.Length];
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i] = new Material(sh) { color = Palette[i], name = $"Sotobosque_{i}" };
            if (mats[i].HasProperty("_BaseColor")) mats[i].SetColor("_BaseColor", Palette[i]);
            if (mats[i].HasProperty("_Smoothness")) mats[i].SetFloat("_Smoothness", 0.1f);
        }

        int chunksPerSide = Mathf.CeilToInt(2f * half / chunkSize);
        var lists = new Dictionary<int, List<CombineInstance>>();
        List<CombineInstance> L(int chunk, int mat)
        {
            int key = chunk * 32 + mat;
            if (!lists.TryGetValue(key, out var l)) lists[key] = l = new List<CombineInstance>();
            return l;
        }

        int cubes = 0;
        for (float x = -half + spacing * 0.5f; x < half; x += spacing)
            for (float z = -half + spacing * 0.5f; z < half; z += spacing)
            {
                Vector3 p = new Vector3(x + R(-1.2f, 1.2f), 0f, z + R(-1.2f, 1.2f));
                // Prados y claros: el ruido decide qué crece en cada zona (y deja calvas de hielo desnudo).
                float meadow = Mathf.PerlinNoise(p.x * 0.02f + 3f, p.z * 0.02f + 9f);
                float bloom = Mathf.PerlinNoise(p.x * 0.05f + 40f, p.z * 0.05f + 1f);
                if (meadow < 0.3f) continue;
                if (!Physics.Raycast(p + Vector3.up * 8f, Vector3.down, out RaycastHit hit, 12f, ~0, QueryTriggerInteraction.Ignore)) continue;
                if (hit.collider.name != groundName || hit.point.y < -0.1f) continue;
                p.y = hit.point.y;
                int chunk = Mathf.Clamp(Mathf.FloorToInt((p.x + half) / chunkSize), 0, chunksPerSide - 1) * chunksPerSide
                          + Mathf.Clamp(Mathf.FloorToInt((p.z + half) / chunkSize), 0, chunksPerSide - 1);
                Quaternion yaw = Quaternion.Euler(0f, R(0f, 360f), 0f);
                double kind = rnd.NextDouble();

                if (bloom > 0.62f && kind < 0.55)          // FLOR polar: tallo + 4 pétalos + centro
                {
                    int petal = 6 + rnd.Next(4);
                    for (int f = 0, nf = 1 + rnd.Next(3); f < nf; f++)
                    {
                        Vector3 q = p + yaw * new Vector3(R(-0.5f, 0.5f), 0f, R(-0.5f, 0.5f));
                        float h = R(0.25f, 0.5f);
                        Add(L(chunk, 11), cube, q + Vector3.up * h * 0.5f, yaw, new Vector3(0.04f, h, 0.04f));
                        for (int k = 0; k < 4; k++)
                        {
                            Quaternion r = yaw * Quaternion.Euler(0f, k * 90f, 0f);
                            Add(L(chunk, petal), cube, q + Vector3.up * h + r * Vector3.forward * 0.07f, r, new Vector3(0.09f, 0.02f, 0.12f));
                        }
                        Add(L(chunk, 10), cube, q + Vector3.up * (h + 0.01f), yaw, new Vector3(0.06f, 0.04f, 0.06f));
                        cubes += 6;
                    }
                }
                else if (kind < 0.62)                       // HIERBA de escarcha: mata de hojas finas
                {
                    int col = rnd.Next(3);
                    for (int b = 0, nb = 3 + rnd.Next(4); b < nb; b++)
                    {
                        float h = R(0.3f, 0.85f);
                        Quaternion tilt = yaw * Quaternion.Euler(R(-14f, 14f), b * 50f, R(-14f, 14f));
                        Add(L(chunk, col), cube, p + yaw * new Vector3(R(-0.25f, 0.25f), 0f, R(-0.25f, 0.25f)) + tilt * Vector3.up * h * 0.5f,
                            tilt, new Vector3(0.06f, h, 0.03f));
                        cubes++;
                    }
                }
                else if (kind < 0.85)                       // ARBUSTO de liquen: racimo de bloques
                {
                    int col = 3 + rnd.Next(3);
                    float size = R(0.5f, 1.2f);
                    for (int b = 0, nb = 3 + rnd.Next(3); b < nb; b++)
                    {
                        Vector3 o = new Vector3(R(-0.5f, 0.5f), R(0.2f, 0.6f), R(-0.5f, 0.5f)) * size;
                        Add(L(chunk, col), cube, p + yaw * o + Vector3.up * size * 0.25f, yaw * Quaternion.Euler(0f, R(0f, 45f), 0f),
                            Vector3.one * size * R(0.45f, 0.7f));
                        cubes++;
                    }
                }
                else                                        // MUSGO: alfombra baja
                {
                    Add(L(chunk, 12), cube, p + Vector3.up * 0.03f, yaw, new Vector3(R(1f, 2.6f), 0.06f, R(1f, 2.4f)));
                    cubes++;
                }
                Plants++;
            }

        foreach (var kv in lists)
        {
            int chunk = kv.Key / 32, mat = kv.Key % 32;
            Mesh m = new Mesh { indexFormat = IndexFormat.UInt32, name = $"Sotobosque_{chunk}_{mat}" };
            m.CombineMeshes(kv.Value.ToArray(), true, true);
            m.RecalculateBounds();
            GameObject g = new GameObject($"Sotobosque_{chunk}_{mat}");
            g.transform.SetParent(transform, false);
            g.AddComponent<MeshFilter>().sharedMesh = m;
            MeshRenderer mr = g.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mats[mat];
            mr.shadowCastingMode = ShadowCastingMode.Off;
        }
        Debug.Log($"[Flora] Sotobosque polar: {Plants} plantas ({cubes} bloques) en {lists.Count} mallas — hierba de escarcha, arbustos de liquen, musgo y flores claras.");
    }

    static void Add(List<CombineInstance> list, Mesh cube, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        list.Add(new CombineInstance { mesh = cube, transform = Matrix4x4.TRS(pos, rot, scale) });
    }
}
