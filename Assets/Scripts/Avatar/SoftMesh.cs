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

    /// <summary>Suaviza el CUERPO de una persona de forma UNIFORME: el tronco (cadera → lumbar → tórax) se ahúsa para que
    /// cada tramo empalme con el siguiente al mismo ancho, formando una silueta continua y simétrica —hombros, cintura
    /// estrecha y caderas (más marcadas en una silueta femenina)— y los miembros son cápsulas que se afinan hacia la
    /// articulación de abajo y se solapan con la de arriba (sin huecos). La CABEZA y la cara se quedan como estaban (de una
    /// pieza, sin «capas»); el pelo lleva cantos suaves.</summary>
    public static void Soften(Transform root, bool feminine = false)
    {
        foreach (MeshFilter mf in root.GetComponentsInChildren<MeshFilter>(true))
        {
            if (mf.sharedMesh == null || !mf.sharedMesh.name.StartsWith("Cube")) continue;   // solo los cubos de primitiva
            string n = mf.gameObject.name;
            bool inHead = false;
            for (Transform t = mf.transform.parent; t != null && t != root; t = t.parent) if (t.name == "Head") { inHead = true; break; }
            if (inHead && n != "Pelo") continue;   // la cara, de una pieza
            (float r, float top, float bot, int seg, float ext)? p = Profile(n, feminine);
            if (p == null) continue;
            var v = p.Value;
            mf.sharedMesh = Rounded(v.r, v.top, v.bot, v.seg, v.ext);
        }
    }

    // Anchos del tronco (m) en cada empalme, de abajo arriba: ingle, cadera alta, cintura, bajo el pecho, hombros.
    // Segmentos: cadera 0,34 de ancho; lumbar 0,36; tórax 0,40 → escala = ancho deseado / ancho del segmento.
    static (float r, float top, float bot, int seg, float ext)? Profile(string n, bool fem)
    {
        if (n.EndsWith("_cubo"))
        {
            string part = n.Substring(0, n.Length - 5);
            float groin = fem ? 0.36f : 0.32f, hipTop = fem ? 0.35f : 0.32f, waist = fem ? 0.27f : 0.31f, underChest = fem ? 0.29f : 0.34f, shoulders = fem ? 0.36f : 0.42f;
            switch (part)
            {
                case "Hips": return (0.14f, hipTop / 0.34f, groin / 0.34f, 5, 1.04f);
                case "Back": return (0.12f, waist / 0.36f, hipTop / 0.36f, 5, 1.04f);
                case "Chest": return (0.14f, shoulders / 0.4f, underChest / 0.4f, 5, 1.04f);
                case "Neck": return (0.4f, 1f, 1f, 4, 1.5f);
                case "ShoulderLeft": case "ShoulderRight": return (0.4f, 1f, 0.86f, 5, 1.1f);   // brazo: se afina al codo
                case "ForearmLeft": case "ForearmRight": return (0.4f, 1f, 0.8f, 5, 1.1f);     // antebrazo: a la muñeca
                case "ThighLeft": case "ThighRight": return (0.4f, fem ? 1.14f : 1f, fem ? 0.74f : 0.78f, 5, 1.08f);   // muslo: grueso arriba (más en la silueta femenina: caderas), rodilla fina
                case "ShinLeft": case "ShinRight": return (0.4f, 1f, 0.74f, 5, 1.08f);         // pantorrilla → tobillo
                case "HandLeft": case "HandRight": return (0.3f, 1f, 0.9f, 4, 1f);
                case "FootLeft": case "FootRight": return (0.3f, 0.9f, 1f, 4, 1f);
                case "ScapulaLeft": case "ScapulaRight": return (0.35f, 1f, 1f, 4, 1.05f);
                default: return null;   // la cabeza, tal cual
            }
        }
        if (n == "Pelo") return (0.25f, 1f, 1f, 3, 1f);
        return null;   // ropa y detalles: como estaban
    }
}
