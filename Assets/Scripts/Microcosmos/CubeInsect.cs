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

    // ── HORMIGA HUMANIZADA (los humanos del microcosmos): BÍPEDA, erguida, con CARA (ojos, cejas, boca) ─────────────
    readonly List<(Transform t, int side, bool arm)> _limbs = new List<(Transform, int, bool)>();

    /// <summary>
    /// Viste a <paramref name="host"/> como HORMIGA HUMANIZADA de <paramref name="height"/> m: de pie sobre las patas
    /// traseras (piernas), con dos brazos y dos bracitos (las patas del medio), tórax erguido, el gáster detrás como una
    /// cola y una cabeza con cara (ojos grandes, cejas, boca) y antenas. <paramref name="bulk"/> la hace más ancha
    /// (Nasatya, la «almohada gigante»); <paramref name="aphid"/> = pulgón (cuerpo de pera, sin gáster de hormiga).
    /// </summary>
    public static CubeInsect DressHuman(GameObject host, Color body, float height, float bulk = 1f, bool aphid = false)
    {
        foreach (MeshRenderer mr in host.GetComponents<MeshRenderer>()) mr.enabled = false;
        Transform old = host.transform.Find("Insecto"); if (old != null) DestroyNow(old.gameObject);
        var root = new GameObject("Insecto").transform;
        root.SetParent(host.transform, false);
        Vector3 ls = host.transform.lossyScale;
        root.localScale = new Vector3(1f / Mathf.Max(0.001f, ls.x), 1f / Mathf.Max(0.001f, ls.y), 1f / Mathf.Max(0.001f, ls.z)) * height;
        MeshFilter mf = host.GetComponent<MeshFilter>();
        bool capsule = mf != null && mf.sharedMesh != null && mf.sharedMesh.name.Contains("Capsule");
        root.localPosition = capsule ? Vector3.down * 1f : Vector3.zero;   // los pies en el suelo
        var ci = host.GetComponent<CubeInsect>() ?? host.AddComponent<CubeInsect>();
        ci._legs.Clear(); ci._limbs.Clear();
        Color dark = body * 0.55f; dark.a = 1f;
        float w = bulk;
        // Piernas (patas traseras), de cadera a pie.
        for (int s = -1; s <= 1; s += 2)
        {
            var hip = new GameObject(s < 0 ? "Pierna_I" : "Pierna_D").transform; hip.SetParent(root, false); hip.localPosition = new Vector3(s * 0.07f * w, 0.46f, 0f);
            Part(hip, "Muslo", new Vector3(0f, -0.11f, 0f), new Vector3(0.06f * w, 0.24f, 0.06f), dark);
            Part(hip, "Tibia", new Vector3(0f, -0.34f, 0.01f), new Vector3(0.045f, 0.24f, 0.045f), dark);
            Part(hip, "Pie", new Vector3(0f, -0.455f, 0.04f), new Vector3(0.06f, 0.025f, 0.12f), dark);
            ci._limbs.Add((hip, s, false));
        }
        if (aphid) Part(root, "Cuerpo", new Vector3(0f, 0.6f, -0.02f), new Vector3(0.36f * w, 0.36f, 0.3f * w), body);   // pera blanda
        else
        {
            Part(root, "Gaster", new Vector3(0f, 0.5f, -0.16f), new Vector3(0.2f * w, 0.18f, 0.26f), body, 25f);   // el abdomen, detrás, como una cola
            Part(root, "Peciolo", new Vector3(0f, 0.5f, -0.04f), new Vector3(0.05f, 0.06f, 0.06f), dark);
            Part(root, "Torax", new Vector3(0f, 0.66f, 0f), new Vector3(0.16f * w, 0.26f, 0.12f * w), body);
        }
        // Brazos (patas delanteras) y bracitos (las del medio).
        for (int s = -1; s <= 1; s += 2)
        {
            var sh = new GameObject(s < 0 ? "Brazo_I" : "Brazo_D").transform; sh.SetParent(root, false); sh.localPosition = new Vector3(s * 0.11f * w, 0.76f, 0f);
            Part(sh, "Brazo", new Vector3(0f, -0.1f, 0f), new Vector3(0.04f, 0.2f, 0.04f), dark);
            Part(sh, "Antebrazo", new Vector3(0f, -0.27f, 0.02f), new Vector3(0.035f, 0.17f, 0.035f), dark);
            Part(sh, "Mano", new Vector3(0f, -0.37f, 0.03f), new Vector3(0.05f, 0.04f, 0.04f), dark);
            ci._limbs.Add((sh, s, true));
            Part(root, "Bracito", new Vector3(s * 0.1f * w, 0.62f, 0.02f), new Vector3(0.025f, 0.025f, 0.12f), dark, 20f, s * 30f);
        }
        // Cabeza con CARA: ojos grandes (con pupila y brillo), cejas, boca y mandíbulas pequeñas; antenas.
        Transform head = Part(root, "Cabeza", new Vector3(0f, 0.9f, 0.01f), new Vector3(0.2f, 0.18f, 0.17f), body);
        for (int s = -1; s <= 1; s += 2)
        {
            Part(root, "Ojo", new Vector3(s * 0.05f, 0.92f, 0.09f), new Vector3(0.055f, 0.06f, 0.012f), Color.white);
            Part(root, "Pupila", new Vector3(s * 0.05f, 0.915f, 0.097f), new Vector3(0.028f, 0.035f, 0.01f), new Color(0.08f, 0.06f, 0.05f));
            Part(root, "Brillo", new Vector3(s * 0.045f, 0.925f, 0.103f), new Vector3(0.01f, 0.01f, 0.006f), Color.white);
            Part(root, "Ceja", new Vector3(s * 0.05f, 0.965f, 0.09f), new Vector3(0.06f, 0.012f, 0.012f), dark, 0f, 0f);
            Part(root, "Mandibula", new Vector3(s * 0.035f, 0.83f, 0.08f), new Vector3(0.02f, 0.02f, 0.05f), dark, 0f, -s * 30f);
            Part(root, "Antena", new Vector3(s * 0.05f, 1.04f, 0.04f), new Vector3(0.015f, 0.16f, 0.015f), dark, -20f, 0f).localRotation = Quaternion.Euler(-20f, 0f, -s * 25f);
        }
        Part(root, "Boca", new Vector3(0f, 0.855f, 0.087f), new Vector3(0.06f, 0.012f, 0.01f), new Color(0.25f, 0.1f, 0.1f));
        return ci;
    }

    void Awake()
    {
        Transform root = transform.Find("Insecto");
        if (root != null && _legs.Count == 0 && _limbs.Count == 0)
            foreach (Transform t in root)
            {
                if (t.name.StartsWith("Pata_")) _legs.Add((t, t.name.EndsWith("I") ? -1 : 1, t.name[5] - '0'));
                else if (t.name.StartsWith("Pierna_")) _limbs.Add((t, t.name.EndsWith("I") ? -1 : 1, false));
                else if (t.name.StartsWith("Brazo_")) _limbs.Add((t, t.name.EndsWith("I") ? -1 : 1, true));
            }
        _last = transform.position;
    }

    // Insectos: trípode alterno. Hormigas humanizadas: piernas y brazos alternos, como al andar una persona.
    void Update()
    {
        Vector3 v = (transform.position - _last) / Mathf.Max(Time.deltaTime, 1e-4f); v.y = 0f;
        _last = transform.position;
        float speed = v.magnitude;
        _phase += speed * Time.deltaTime * 4f;
        float amp = Mathf.Clamp01(speed / 1.5f) * 25f;
        foreach (var (t, side, pair) in _legs)
        {
            if (t == null) continue;
            int group = (pair % 2 == 0) == (side < 0) ? 0 : 1;
            t.localRotation = Quaternion.Euler(0f, side * (60f + (pair - 1) * 25f) + Mathf.Sin(_phase + group * Mathf.PI) * amp, 0f);
        }
        foreach (var (t, side, arm) in _limbs)
        {
            if (t == null) continue;
            float s = Mathf.Sin(_phase + (side < 0 ? 0f : Mathf.PI)) * amp * (arm ? -0.8f : 1f);
            t.localRotation = Quaternion.Euler(s, 0f, 0f);
        }
    }
}
