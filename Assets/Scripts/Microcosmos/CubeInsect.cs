using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// INSECTO ARTICULADO DE CUBOS (microcosmos): cabeza, tórax y abdomen, seis patas de dos segmentos, antenas y, según la
/// especie, mandíbulas (hormiga), sifones (pulgón) o élitros con puntos (mariquita). Sustituye a las cápsulas: el cuerpo
/// físico (collider, Anima, NavMeshAgent) sigue siendo el del anfitrión; esto es la FORMA visible, a la escala del
/// microcosmos (la hormiga ≈ el jugador). Al moverse, las patas se balancean en trípode alterno (como los insectos reales).
/// </summary>
public class CubeInsect : MonoBehaviour
{
    public enum Kind { Ant, Aphid, Beetle, Spider }

    readonly List<(Transform t, int side, int pair)> _legs = new List<(Transform, int, int)>();
    Vector3 _last; float _phase;

    /// <summary>Viste a <paramref name="host"/> como insecto de <paramref name="length"/> m (oculta su malla primitiva).</summary>
    public static CubeInsect Dress(GameObject host, Kind kind, Color body, float length)
    {
        foreach (MeshRenderer mr in host.GetComponents<MeshRenderer>()) mr.enabled = false;
        Transform old = host.transform.Find("Insecto"); if (old != null) DestroyNow(old.gameObject);
        var root = new GameObject("Insecto").transform;
        root.SetParent(host.transform, false);
        // Contrarresta la escala del anfitrión: el insecto mide `length` en el mundo.
        Vector3 ls = host.transform.lossyScale;
        root.localScale = new Vector3(1f / Mathf.Max(0.001f, ls.x), 1f / Mathf.Max(0.001f, ls.y), 1f / Mathf.Max(0.001f, ls.z)) * length;
        root.localPosition = Vector3.down * 1f;   // apoyado en el suelo (la cápsula va de −1 a +1 en local)
        var ci = host.GetComponent<CubeInsect>() ?? host.AddComponent<CubeInsect>();
        ci._legs.Clear();
        Color dark = body * 0.6f; dark.a = 1f;
        float h = kind == Kind.Aphid ? 0.22f : 0.18f;   // altura del cuerpo sobre el suelo (en «largos»)
        switch (kind)
        {
            case Kind.Ant:
                Part(root, "Cabeza", new Vector3(0f, h + 0.06f, 0.36f), new Vector3(0.18f, 0.16f, 0.18f), body);
                Part(root, "Torax", new Vector3(0f, h + 0.04f, 0.14f), new Vector3(0.12f, 0.12f, 0.24f), body);
                Part(root, "Peciolo", new Vector3(0f, h + 0.02f, -0.02f), new Vector3(0.06f, 0.08f, 0.08f), dark);
                Part(root, "Abdomen", new Vector3(0f, h + 0.06f, -0.22f), new Vector3(0.22f, 0.2f, 0.32f), body);
                for (int s = -1; s <= 1; s += 2) Part(root, "Mandibula", new Vector3(s * 0.05f, h + 0.02f, 0.47f), new Vector3(0.03f, 0.03f, 0.08f), dark, 0f, s * 25f);
                break;
            case Kind.Aphid:   // pulgón: cuerpo en pera, blando, con sus dos sifones atrás
                Part(root, "Cabeza", new Vector3(0f, h + 0.02f, 0.34f), new Vector3(0.16f, 0.14f, 0.14f), body);
                Part(root, "Cuerpo", new Vector3(0f, h + 0.1f, 0f), new Vector3(0.42f, 0.36f, 0.56f), body);
                for (int s = -1; s <= 1; s += 2) Part(root, "Sifon", new Vector3(s * 0.12f, h + 0.24f, -0.26f), new Vector3(0.04f, 0.16f, 0.04f), dark, -30f, 0f);
                break;
            case Kind.Beetle:   // escarabajo / mariquita: élitros abombados
                Part(root, "Cabeza", new Vector3(0f, h + 0.02f, 0.36f), new Vector3(0.2f, 0.12f, 0.12f), dark);
                Part(root, "Pronoto", new Vector3(0f, h + 0.06f, 0.24f), new Vector3(0.34f, 0.14f, 0.14f), dark);
                Part(root, "Elitros", new Vector3(0f, h + 0.12f, -0.06f), new Vector3(0.5f, 0.24f, 0.5f), body);
                break;
            case Kind.Spider:
                Part(root, "Cefalotorax", new Vector3(0f, h + 0.06f, 0.16f), new Vector3(0.24f, 0.16f, 0.26f), body);
                Part(root, "Abdomen", new Vector3(0f, h + 0.12f, -0.2f), new Vector3(0.36f, 0.3f, 0.4f), body);
                break;
        }
        if (kind != Kind.Spider)
            for (int s = -1; s <= 1; s += 2) Part(root, "Antena", new Vector3(s * 0.06f, h + 0.2f, 0.5f), new Vector3(0.02f, 0.02f, 0.26f), dark, -35f, s * 25f);
        // Patas: 6 (araña 8), de dos segmentos, abiertas hacia los lados.
        int pairs = kind == Kind.Spider ? 4 : 3;
        for (int p = 0; p < pairs; p++)
            for (int s = -1; s <= 1; s += 2)
            {
                float z = 0.2f - p * (0.36f / (pairs - 1));
                var hip = new GameObject($"Pata_{p}_{(s < 0 ? "I" : "D")}").transform;
                hip.SetParent(root, false); hip.localPosition = new Vector3(s * 0.08f, h + 0.02f, z);
                hip.localRotation = Quaternion.Euler(0f, s * (60f + (p - 1) * 25f), 0f);
                Part(hip, "Muslo", new Vector3(0f, 0.04f, 0.12f), new Vector3(0.03f, 0.03f, 0.26f), dark, -18f, 0f);
                Part(hip, "Tibia", new Vector3(0f, -0.06f, 0.27f), new Vector3(0.025f, 0.025f, 0.24f), dark, 45f, 0f);
                ci._legs.Add((hip, s, p));
            }
        return ci;
    }

    static Transform Part(Transform parent, string name, Vector3 pos, Vector3 size, Color c, float pitch = 0f, float yaw = 0f)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        DestroyNow(g.GetComponent<Collider>());
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos; g.transform.localRotation = Quaternion.Euler(pitch, yaw, 0f); g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(c);
        return g.transform;
    }

    static void DestroyNow(Object o) { if (o == null) return; if (Application.isPlaying) Destroy(o); else DestroyImmediate(o); }

    void Awake()
    {
        if (_legs.Count == 0)
        {
            Transform root = transform.Find("Insecto");
            if (root != null)
                foreach (Transform t in root)
                    if (t.name.StartsWith("Pata_")) _legs.Add((t, t.name.EndsWith("I") ? -1 : 1, t.name[5] - '0'));
        }
        _last = transform.position;
    }

    // Trípode alterno: patas 0 y 2 de un lado con la 1 del otro; balanceo proporcional a la velocidad.
    void Update()
    {
        Vector3 v = (transform.position - _last) / Mathf.Max(Time.deltaTime, 1e-4f); v.y = 0f;
        _last = transform.position;
        float speed = v.magnitude;
        _phase += speed * Time.deltaTime * 6f;
        float amp = Mathf.Clamp01(speed / 1.5f) * 22f;
        foreach (var (t, side, pair) in _legs)
        {
            if (t == null) continue;
            int group = (pair % 2 == 0) == (side < 0) ? 0 : 1;
            float swing = Mathf.Sin(_phase + group * Mathf.PI) * amp;
            t.localRotation = Quaternion.Euler(0f, side * (60f + (pair - 1) * 25f) + swing, 0f);
        }
    }
}
