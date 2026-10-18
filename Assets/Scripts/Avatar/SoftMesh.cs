using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MALLAS SUAVES para los cuerpos de "cubos": en vez del cubo de 24 vértices, una CAJA REDONDEADA subdividida (cubo
/// proyectado hacia una esfera con radio de redondeo `round`) y, opcionalmente, AHUSADA (más estrecha arriba o abajo,
/// como un muslo que se afina hacia la rodilla). Ocupa el mismo cubo unidad (−0,5…0,5), así que se cambia la malla de
/// cada pieza sin tocar su escala, sus articulaciones, BodyShape (grosor), SetStretch ni las posturas.
/// Las mallas se comparten por perfil (caché), así que cien personas no crean cien mallas.
/// </summary>
public static class SoftMesh
{
    static readonly Dictionary<string, Mesh> _cache = new Dictionary<string, Mesh>();

    /// <summary>Caja redondeada: <paramref name="round"/> 0 = cubo, 0,5 = esfera/cápsula. <paramref name="topScale"/> y
    /// <paramref name="bottomScale"/> ahúsan el ancho (x, z) arriba (y = +0,5) y abajo (y = −0,5). <paramref name="seg"/>
    /// subdivisiones por arista de cada cara.</summary>
    public static Mesh Rounded(float round, float topScale = 1f, float bottomScale = 1f, int seg = 4, float yExtend = 1f)
    {
        round = Mathf.Clamp(round, 0f, 0.5f);
        string key = $"{round:0.00}_{topScale:0.00}_{bottomScale:0.00}_{seg}_{yExtend:0.00}";
        if (_cache.TryGetValue(key, out Mesh cached) && cached != null) return cached;

        var verts = new List<Vector3>(); var norms = new List<Vector3>(); var uvs = new List<Vector2>(); var tris = new List<int>();
        Vector3[] faceN = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
        float inner = 0.5f - round;
        foreach (Vector3 n in faceN)
        {
            // Dos ejes tangentes de la cara (orden para que los triángulos miren hacia fuera).
            Vector3 u = Mathf.Abs(n.y) > 0.5f ? Vector3.right : Vector3.up;
            Vector3 v = Vector3.Cross(n, u);
            int start = verts.Count;
            for (int j = 0; j <= seg; j++)
                for (int i = 0; i <= seg; i++)
                {
                    float a = i / (float)seg - 0.5f, b = j / (float)seg - 0.5f;
                    Vector3 c = n * 0.5f + u * a + v * b;   // punto en la superficie del cubo unidad
                    Vector3 core = new Vector3(Mathf.Clamp(c.x, -inner, inner), Mathf.Clamp(c.y, -inner, inner), Mathf.Clamp(c.z, -inner, inner));
                    Vector3 d = c - core;
                    Vector3 nn = d.sqrMagnitude > 1e-8f ? d.normalized : n;
                    Vector3 p = round > 0f ? core + nn * round : c;
                    float t = p.y + 0.5f;   // 0 abajo … 1 arriba
                    float k = Mathf.Lerp(bottomScale, topScale, t);
                    p.x *= k; p.z *= k;
                    p.y *= yExtend;   // se alarga un poco: los segmentos vecinos se solapan y no se ve hueco en la articulación
                    // La normal del ahusado se inclina un poco hacia el lado que se estrecha.
                    Vector3 tn = new Vector3(nn.x / Mathf.Max(0.05f, k), nn.y - (topScale - bottomScale) * 0.5f * new Vector2(nn.x, nn.z).magnitude, nn.z / Mathf.Max(0.05f, k)).normalized;
                    verts.Add(p); norms.Add(tn); uvs.Add(new Vector2(a + 0.5f, b + 0.5f));
                }
            for (int j = 0; j < seg; j++)
                for (int i = 0; i < seg; i++)
                {
                    int i0 = start + j * (seg + 1) + i, i1 = i0 + 1, i2 = i0 + seg + 1, i3 = i2 + 1;
                    tris.Add(i0); tris.Add(i1); tris.Add(i2);   // u × v = n: cara hacia fuera
                    tris.Add(i1); tris.Add(i3); tris.Add(i2);
                }
        }
        var m = new Mesh { name = "Suave_" + key };
        m.SetVertices(verts); m.SetNormals(norms); m.SetUVs(0, uvs); m.SetTriangles(tris, 0);
        m.RecalculateBounds();
        _cache[key] = m;
        return m;
    }

    /// <summary>Cambia la malla de una pieza (si es el cubo de primitiva) por la suave.</summary>
    public static void Apply(GameObject g, float round, float topScale = 1f, float bottomScale = 1f, int seg = 3)
    {
        if (g == null || !g.TryGetComponent(out MeshFilter mf)) return;
        mf.sharedMesh = Rounded(round, topScale, bottomScale, seg);
    }

    /// <summary>Suaviza TODO el cuerpo de una persona: cada segmento con su perfil (miembros como cápsulas que se afinan
    /// hacia la articulación de abajo, torso ensanchado arriba, cabeza ovalada) y los detalles (cara, pelo, ropa) con
    /// cantos redondeados.</summary>
    public static void Soften(Transform root)
    {
        foreach (MeshFilter mf in root.GetComponentsInChildren<MeshFilter>(true))
        {
            if (mf.sharedMesh == null || !mf.sharedMesh.name.StartsWith("Cube")) continue;   // solo los cubos de primitiva
            string n = mf.gameObject.name;
            (float r, float top, float bot, int seg, float ext) p = Profile(n, mf.transform.localScale);
            mf.sharedMesh = Rounded(p.r, p.top, p.bot, p.seg, p.ext);
        }
    }

    static (float, float, float, int, float) Profile(string n, Vector3 s)
    {
        if (n.EndsWith("_cubo"))
        {
            string part = n.Substring(0, n.Length - 5);
            switch (part)
            {
                case "Head": return (0.3f, 1f, 0.96f, 7, 1f);
                case "Neck": return (0.48f, 0.92f, 1f, 5, 1.6f);
                case "Chest": return (0.2f, 1.04f, 0.94f, 6, 1.12f);         // tórax: ancho en los hombros, cintura más estrecha
                case "Back": return (0.18f, 0.94f, 1f, 6, 1.25f);            // lumbar: de la cintura a la cadera
                case "Hips": return (0.22f, 0.98f, 1f, 6, 1.15f);
                case "ShoulderLeft": case "ShoulderRight": return (0.46f, 1f, 0.84f, 6, 1.12f);   // brazo: se afina al codo
                case "ForearmLeft": case "ForearmRight": return (0.46f, 1f, 0.8f, 6, 1.12f);     // antebrazo: se afina a la muñeca
                case "ThighLeft": case "ThighRight": return (0.46f, 1f, 0.78f, 6, 1.1f);         // muslo: grueso arriba, rodilla fina
                case "ShinLeft": case "ShinRight": return (0.46f, 1f, 0.74f, 6, 1.1f);           // pantorrilla → tobillo
                case "HandLeft": case "HandRight": return (0.4f, 1f, 0.85f, 5, 1f);
                case "FootLeft": case "FootRight": return (0.35f, 0.9f, 1f, 5, 1f);
                case "ScapulaLeft": case "ScapulaRight": return (0.4f, 1f, 1f, 5, 1.1f);
                default: return (0.4f, 1f, 1f, 5, 1f);
            }
        }
        switch (n)
        {
            case "Cara_media": return (0.22f, 1f, 0.92f, 6, 1.08f);
            case "Mandibula_forma": return (0.3f, 1f, 0.78f, 6, 1.05f);   // mentón más estrecho
            case "Pomulo": return (0.5f, 1f, 1f, 4, 1f);
            case "Pelo": return (0.25f, 1f, 1f, 3, 1f);
            case "Musculo": case "Grasa": return (0.5f, 1f, 1f, 5, 1f);
        }
        float thin = Mathf.Min(s.x, Mathf.Min(s.y, s.z)), thick = Mathf.Max(s.x, Mathf.Max(s.y, s.z));
        // Piezas muy pequeñas o planas (pestañas, juntas, rayas): canto suave y pocas subdivisiones.
        if (thick < 0.03f || thin / Mathf.Max(1e-4f, thick) < 0.08f) return (0.3f, 1f, 1f, 2, 1f);
        return (0.3f, 1f, 1f, 3, 1f);
    }
}
