using UnityEngine;

/// <summary>
/// EL PORTAL DE VUELTA AL MESOCOSMOS, como un AGUJERO NEGRO en el centro de una pequeña GALAXIA (Berón 2026-10-27): un
/// núcleo negro con su anillo de luz (el disco de acreción) y dos brazos en espiral de estrellas que giran despacio. Todo de
/// primitivas (sin texturas); el trigger del portal (<see cref="WorldExitPortal"/>) sigue siendo la caja de siempre.
/// </summary>
public class GalaxyPortal : MonoBehaviour
{
    public float spin = 18f;   // grados por segundo del disco
    Transform _disk, _arms;

    /// <summary>Viste el portal <paramref name="portal"/> (su caja se oculta) con el agujero negro y la galaxia.</summary>
    public static GalaxyPortal Dress(Transform portal, float radius = 2.4f)
    {
        Renderer box = portal.GetComponent<Renderer>(); if (box != null) box.enabled = false;
        var root = new GameObject("Galaxia").transform; root.SetParent(portal, false);
        // La caja del portal puede estar escalada: la galaxia va en su propio tamaño.
        Vector3 s = portal.lossyScale; root.localScale = new Vector3(1f / Mathf.Max(0.01f, s.x), 1f / Mathf.Max(0.01f, s.y), 1f / Mathf.Max(0.01f, s.z));
        root.localPosition = Vector3.up * 0.3f / Mathf.Max(0.01f, s.y);
        var g = portal.gameObject.AddComponent<GalaxyPortal>();

        Material black = Unlit(new Color(0.01f, 0.01f, 0.02f)), glow = Unlit(new Color(1f, 0.78f, 0.45f)), hot = Unlit(new Color(1f, 0.95f, 0.85f));
        Material blue = Unlit(new Color(0.55f, 0.7f, 1f)), violet = Unlit(new Color(0.75f, 0.55f, 0.95f));
        Part(root, PrimitiveType.Sphere, Vector3.zero, Vector3.one * radius * 0.42f, black, "Horizonte");
        // El disco de acreción: un anillo inclinado de bloques dorados (más blancos por dentro).
        g._disk = new GameObject("Disco").transform; g._disk.SetParent(root, false); g._disk.localRotation = Quaternion.Euler(72f, 0f, 0f);
        for (int i = 0; i < 48; i++)
        {
            float a = i * Mathf.PI * 2f / 48f;
            for (int k = 0; k < 2; k++)
            {
                float r = radius * (0.32f + k * 0.1f);
                Part(g._disk, PrimitiveType.Cube, new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r), new Vector3(0.16f, 0.03f, 0.16f) * radius * 0.5f, k == 0 ? hot : glow, "Plasma").localRotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 0f);
            }
        }
        // Dos brazos en espiral de estrellas (azules y violetas), en el plano del portal, mirando al jugador.
        g._arms = new GameObject("Brazos").transform; g._arms.SetParent(root, false); g._arms.localRotation = Quaternion.Euler(90f, 0f, 0f);
        var rnd = new System.Random(7);
        for (int arm = 0; arm < 2; arm++)
            for (int i = 0; i < 60; i++)
            {
                float t = i / 60f, a = arm * Mathf.PI + t * Mathf.PI * 2.6f, r = radius * (0.5f + t * 0.9f);
                float jitter = (float)(rnd.NextDouble() - 0.5) * 0.18f * radius;
                Vector3 p = new Vector3(Mathf.Cos(a) * r + jitter, (float)(rnd.NextDouble() - 0.5) * 0.08f, Mathf.Sin(a) * r + jitter);
                Part(g._arms, PrimitiveType.Cube, p, Vector3.one * radius * (0.05f + 0.04f * (1f - t)), i % 3 == 0 ? violet : blue, "Estrella");
            }
        return g;
    }

    void Update()
    {
        if (_disk != null) _disk.Rotate(Vector3.up, spin * Time.deltaTime, Space.Self);
        if (_arms != null) _arms.Rotate(Vector3.up, spin * 0.25f * Time.deltaTime, Space.Self);
    }

    static Transform Part(Transform parent, PrimitiveType type, Vector3 local, Vector3 size, Material m, string name)
    {
        GameObject g = GameObject.CreatePrimitive(type); g.name = name;
        Collider c = g.GetComponent<Collider>(); if (Application.isPlaying) Destroy(c); else DestroyImmediate(c);
        g.transform.SetParent(parent, false); g.transform.localPosition = local; g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = m;
        return g.transform;
    }

    static Material Unlit(Color c)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
        var m = new Material(sh) { color = c };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        return m;
    }
}
