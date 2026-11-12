using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL COHETE, CON FORMA DE COHETE (ronda 7, 2026-11-05; docs/lanzadera-de-cohetes.md): modelo low-poly de 18 m de la clase Electron
/// con sus etapas: nueve toberas, faldón, aletas, primera etapa, interetapa, segunda etapa y cofia ojival con su punta. Son mallas
/// de revolución de caras planas (<see cref="Lathe"/>), no cubos. El origen es la base de las toberas.
/// </summary>
public static class RocketModel
{
    public const float Height = 18f, Radius = 0.6f;
    static readonly Color White = new Color(0.86f, 0.86f, 0.84f), Black = new Color(0.14f, 0.14f, 0.16f), Red = new Color(0.72f, 0.16f, 0.12f),
        Metal = new Color(0.5f, 0.5f, 0.54f), Dark = new Color(0.07f, 0.07f, 0.08f);

    /// <summary>Monta el cohete bajo <paramref name="parent"/> (la raíz debe ser «Cohete»: la mira <c>LaunchSiteTest</c>).</summary>
    public static void Build(Transform rk)
    {
        float r = Radius;
        // TOBERAS: una central y ocho alrededor (la clase Electron lleva 9 motores).
        Mesh bell = Lathe("Cohete_tobera", new[] { (0f, 0.12f), (0.2f, 0f), (0.2f, 0.06f), (0.09f, 0.6f), (0.13f, 0.72f), (0f, 0.72f) }, 10);
        Part(rk, "Tobera", bell, Metal, Vector3.zero, Quaternion.identity);
        for (int i = 0; i < 8; i++)
        {
            float a = i * 45f * Mathf.Deg2Rad;
            Part(rk, "Tobera", bell, Metal, new Vector3(Mathf.Cos(a) * 0.4f, 0f, Mathf.Sin(a) * 0.4f), Quaternion.identity);
        }
        // CUERPO: faldón que se ensancha hacia la base, primera etapa con franjas, interetapa, segunda etapa, cofia.
        Part(rk, "Faldon", Lathe("Cohete_faldon", new[] { (0f, 0.6f), (r + 0.05f, 0.6f), (r, 1.1f), (0f, 1.1f) }, 16), Black, Vector3.zero, Quaternion.identity);
        Part(rk, "Primera_etapa", Lathe("Cohete_etapa1", new[] { (0f, 1.1f), (r, 1.1f), (r, 12f), (0f, 12f) }, 16), White, Vector3.zero, Quaternion.identity);
        foreach (float y in new[] { 1.6f, 4.5f, 8.5f })
            Part(rk, "Franja", Lathe("Cohete_franja", new[] { (0f, 0f), (r + 0.015f, 0f), (r + 0.015f, 0.5f), (0f, 0.5f) }, 16), Black, new Vector3(0f, y, 0f), Quaternion.identity);
        Part(rk, "Interetapa", Lathe("Cohete_interetapa", new[] { (0f, 12f), (r + 0.02f, 12f), (r - 0.06f, 12.6f), (0f, 12.6f) }, 16), Black, Vector3.zero, Quaternion.identity);
        Part(rk, "Segunda_etapa", Lathe("Cohete_etapa2", new[] { (0f, 12.6f), (r - 0.06f, 12.6f), (r - 0.06f, 14.4f), (0f, 14.4f) }, 16), White, Vector3.zero, Quaternion.identity);
        Part(rk, "Franja", Lathe("Cohete_franja2", new[] { (0f, 14.1f), (r - 0.045f, 14.1f), (r - 0.045f, 14.4f), (0f, 14.4f) }, 16), Red, Vector3.zero, Quaternion.identity);
        Part(rk, "Cofia", Lathe("Cohete_cofia", new[] { (0f, 14.4f), (r, 14.4f), (r, 15.2f), (r * 0.9f, 15.9f), (r * 0.68f, 16.6f), (r * 0.38f, 17.2f), (r * 0.16f, 17.6f), (0f, 17.6f) }, 16), White, Vector3.zero, Quaternion.identity);
        Part(rk, "Punta_cofia", Lathe("Cohete_punta", new[] { (0f, 17.6f), (r * 0.16f, 17.6f), (0f, Height) }, 16), Red, Vector3.zero, Quaternion.identity);
        // ALETAS: cuatro, en flecha, al pie de la primera etapa.
        Mesh fin = Fin("Cohete_aleta", r - 0.03f, 1.15f, 0.1f);
        for (int i = 0; i < 4; i++)
            Part(rk, "Aleta", fin, Red, Vector3.zero, Quaternion.Euler(0f, 45f + i * 90f, 0f));
    }

    static void Part(Transform parent, string name, Mesh m, Color c, Vector3 pos, Quaternion rot)
    {
        GameObject g = new GameObject(name);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos; g.transform.localRotation = rot;
        g.AddComponent<MeshFilter>().sharedMesh = m;
        g.AddComponent<MeshRenderer>().sharedMaterial = KitchenProps.Mat(c);
    }

    /// <summary>Malla de revolución de caras planas. El perfil (radio, altura) va del eje de abajo, por fuera, al eje de arriba.</summary>
    public static Mesh Lathe(string name, (float r, float y)[] profile, int sides)
    {
        var v = new List<Vector3>(); var t = new List<int>();
        for (int k = 0; k + 1 < profile.Length; k++)
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * Mathf.PI * 2f / sides, a1 = (i + 1) * Mathf.PI * 2f / sides;
                Vector3 P(float r, float y, float a) => new Vector3(r * Mathf.Cos(a), y, r * Mathf.Sin(a));
                Vector3 p0 = P(profile[k].r, profile[k].y, a0), p1 = P(profile[k].r, profile[k].y, a1),
                    q0 = P(profile[k + 1].r, profile[k + 1].y, a0), q1 = P(profile[k + 1].r, profile[k + 1].y, a1);
                if (profile[k + 1].r > 0.0001f || profile[k].r > 0.0001f)
                {
                    if (profile[k].r > 0.0001f) Tri(v, t, p0, q0, p1);
                    if (profile[k + 1].r > 0.0001f) Tri(v, t, p1, q0, q1);
                }
            }
        return Finish(name, v, t);
    }

    /// <summary>Aleta en flecha: raíz pegada al cuerpo (en <paramref name="root"/> del eje), envergadura <paramref name="span"/> y
    /// grosor <paramref name="thick"/>, en el plano XY.</summary>
    static Mesh Fin(string name, float root, float span, float thick)
    {
        var v = new List<Vector3>(); var t = new List<int>();
        Vector2[] o = { new Vector2(root, 1.1f), new Vector2(root + span, 0.15f), new Vector2(root + span, 1.05f), new Vector2(root, 3.4f) };
        float h = thick * 0.5f;
        Vector3 F(int i) => new Vector3(o[i].x, o[i].y, -h);
        Vector3 B(int i) => new Vector3(o[i].x, o[i].y, h);
        // Las dos caras grandes y los cuatro cantos.
        Tri(v, t, F(0), F(3), F(1)); Tri(v, t, F(1), F(3), F(2));
        Tri(v, t, B(0), B(1), B(3)); Tri(v, t, B(1), B(2), B(3));
        for (int i = 0; i < 4; i++)
        {
            int j = (i + 1) % 4;
            Tri(v, t, F(i), F(j), B(i)); Tri(v, t, F(j), B(j), B(i));
        }
        return Finish(name, v, t);
    }

    static void Tri(List<Vector3> v, List<int> t, Vector3 a, Vector3 b, Vector3 c)
    {
        t.Add(v.Count); v.Add(a); t.Add(v.Count); v.Add(b); t.Add(v.Count); v.Add(c);   // vértices propios: caras planas
    }

    static Mesh Finish(string name, List<Vector3> v, List<int> t)
    {
        var m = new Mesh { name = name };
        m.SetVertices(v); m.SetTriangles(t, 0);
        m.RecalculateNormals(); m.RecalculateBounds();
        return m;
    }
}
