using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

/// <summary>
/// PAISAJE POLAR del santuario de hielo (docs/ice-sanctuary-ecology.md):
///  · CASQUETES: el suelo deja de ser una losa única. Es banquisa: placas de hielo separadas por CANALES de agua
///    (grietas por donde asoma el mar), con PUENTES de hielo cada cierto tramo para cruzar a pie; al este, mar abierto
///    con témpanos sueltos. Las áreas del santuario, los nidos, los seres y el hub quedan siempre sobre hielo firme.
///  · BOSQUE con EDADES: arboledas de un árbol madre (adulto o viejo) con jóvenes y brotes alrededor, más árboles
///    sueltos, esparcidos por todo el hielo y lejos de las áreas para no cortar el paso.
/// Determinista (semillas fijas): cada regeneración da el mismo mapa.
/// </summary>
public static class PolarLandscapeBuilder
{
    public const float Half = 250f;          // el hielo cubre [-250, 250]
    const float Cell = 2.5f;                 // resolución de la banquisa
    const float IceTop = 0f, IceThickness = 1.5f;
    public const float WaterLevel = -0.35f;  // el agua asoma 35 cm bajo el borde del hielo (se puede salir nadando)

    // Canales (grietas) que cruzan el hielo; ancho ~6 m, con puentes cada BridgeEvery m.
    static readonly Vector2[][] Leads =
    {
        new[] { new Vector2(-250f, -200f), new Vector2(-170f, -140f), new Vector2(-120f, -60f), new Vector2(-180f, 20f), new Vector2(-250f, 60f) },
        new[] { new Vector2(-250f, 150f), new Vector2(-150f, 175f), new Vector2(-60f, 205f), new Vector2(20f, 250f) },
        new[] { new Vector2(-60f, -250f), new Vector2(-45f, -180f), new Vector2(30f, -140f), new Vector2(110f, -165f), new Vector2(165f, -110f) },
        new[] { new Vector2(165f, 70f), new Vector2(125f, 150f), new Vector2(60f, 185f), new Vector2(-5f, 160f), new Vector2(-40f, 250f) },
        new[] { new Vector2(-120f, -60f), new Vector2(-65f, -55f), new Vector2(-55f, 45f), new Vector2(-100f, 100f), new Vector2(-150f, 175f) },
        new[] { new Vector2(30f, -140f), new Vector2(60f, -100f), new Vector2(150f, -60f) },
    };
    const float LeadHalfWidth = 3f, BridgeEvery = 45f, BridgeWidth = 5f;

    // Témpanos del mar abierto (este): centro + radio.
    static readonly Vector3[] Floes =
    {
        new Vector3(195f, 0f, 70f) , new Vector3(215f, 0f, -70f), new Vector3(185f, 0f, -125f),
        new Vector3(230f, 0f, 125f), new Vector3(200f, 0f, 180f), new Vector3(175f, 0f, -185f), new Vector3(235f, 0f, 15f),
    };
    static readonly float[] FloeRadius = { 17f, 20f, 13f, 15f, 19f, 14f, 9f };

    /// <summary>Zonas que deben ser hielo firme (x, z, radio) — áreas, hub, nidos, seres. Se rellena antes de BuildFloes.</summary>
    public static readonly List<Vector3> Protect = new List<Vector3>();

    static float Noise(float x, float z) => Mathf.PerlinNoise(x * 0.035f + 13.1f, z * 0.035f + 7.7f);

    static float CoastX(float z) => 165f + 6f * Mathf.Sin(z * 0.05f) + 4f * Mathf.Sin(z * 0.13f + 1f);

    /// <summary>¿Hay hielo en (x, z)? (antes de forzar las zonas protegidas).</summary>
    public static bool NaturalIce(float x, float z)
    {
        if (Mathf.Abs(x) > Half || Mathf.Abs(z) > Half) return false;
        if (x > CoastX(z))
        {
            for (int i = 0; i < Floes.Length; i++)
            {
                float dx = x - Floes[i].x, dz = z - Floes[i].z;
                float a = Mathf.Atan2(dz, dx);
                float r = FloeRadius[i] * (1f + 0.18f * Mathf.Sin(a * 5f + i) + 0.08f * Mathf.Sin(a * 11f + 2f * i));
                if (dx * dx + dz * dz < r * r) return true;
            }
            return false;
        }
        Vector2 p = new Vector2(x, z);
        foreach (Vector2[] lead in Leads)
        {
            float s = 0f;
            for (int k = 0; k < lead.Length - 1; k++)
            {
                Vector2 a = lead[k], b = lead[k + 1];
                Vector2 ab = b - a; float len = ab.magnitude;
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / (len * len));
                float d = Vector2.Distance(p, a + ab * t);
                float hw = LeadHalfWidth * (0.7f + 0.8f * Noise(x, z));
                if (d < hw)
                {
                    float along = s + t * len;
                    if (along % BridgeEvery < BridgeWidth && along > 10f) return true;   // puente de hielo
                    return false;
                }
                s += len;
            }
        }
        return true;
    }

    /// <summary>Construye la banquisa (bloques de hielo fusionados en rectángulos) como hijos de <paramref name="ground"/>.</summary>
    public static int BuildFloes(Transform ground, Material ice)
    {
        int n = Mathf.RoundToInt(2f * Half / Cell);
        bool[,] land = new bool[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                float x = -Half + (i + 0.5f) * Cell, z = -Half + (j + 0.5f) * Cell;
                land[i, j] = NaturalIce(x, z);
            }
        // Hielo firme bajo todo lo importante.
        foreach (Vector3 pr in Protect)
        {
            int i0 = Mathf.Max(0, Mathf.FloorToInt((pr.x - pr.z - (-Half)) / Cell)), i1 = Mathf.Min(n - 1, Mathf.CeilToInt((pr.x + pr.z + Half) / Cell));
            int j0 = Mathf.Max(0, Mathf.FloorToInt((pr.y - pr.z + Half) / Cell)), j1 = Mathf.Min(n - 1, Mathf.CeilToInt((pr.y + pr.z + Half) / Cell));
            for (int i = i0; i <= i1; i++)
                for (int j = j0; j <= j1; j++)
                {
                    float x = -Half + (i + 0.5f) * Cell, z = -Half + (j + 0.5f) * Cell;
                    if ((x - pr.x) * (x - pr.x) + (z - pr.y) * (z - pr.y) <= pr.z * pr.z) land[i, j] = true;
                }
        }

        // Rectángulos voraces: corre en X por fila y extiende en Z mientras la fila siguiente tenga el mismo tramo.
        GameObject root = new GameObject("Banquisa_Casquetes");
        root.transform.SetParent(ground, false);
        bool[,] used = new bool[n, n];
        int boxes = 0;
        for (int j = 0; j < n; j++)
            for (int i = 0; i < n; i++)
            {
                if (!land[i, j] || used[i, j]) continue;
                int i1 = i; while (i1 + 1 < n && land[i1 + 1, j] && !used[i1 + 1, j]) i1++;
                int j1 = j;
                while (j1 + 1 < n)
                {
                    bool ok = true;
                    for (int k = i; k <= i1 && ok; k++) ok = land[k, j1 + 1] && !used[k, j1 + 1];
                    if (!ok) break;
                    j1++;
                }
                for (int a = i; a <= i1; a++) for (int b = j; b <= j1; b++) used[a, b] = true;
                float x0 = -Half + i * Cell, x1 = -Half + (i1 + 1) * Cell, z0 = -Half + j * Cell, z1 = -Half + (j1 + 1) * Cell;
                GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                g.name = "Casquete";
                g.transform.SetParent(root.transform, false);
                g.transform.position = new Vector3((x0 + x1) * 0.5f, IceTop - IceThickness * 0.5f, (z0 + z1) * 0.5f);
                g.transform.localScale = new Vector3(x1 - x0, IceThickness, z1 - z0);
                g.GetComponent<Renderer>().sharedMaterial = ice;
                g.isStatic = true;
                boxes++;
            }
        return boxes;
    }

    /// <summary>Que la navegación ignore este objeto (agua, fondo, flora decorativa): si no, se andaría sobre el mar.</summary>
    public static void IgnoreForNav(GameObject g)
    {
        NavMeshModifier m = g.GetComponent<NavMeshModifier>() ?? g.AddComponent<NavMeshModifier>();
        m.ignoreFromBuild = true;
    }

    // ── Bosque con edades ───────────────────────────────────────────────────────────────────────

    public enum TreeAge { Brote, Joven, Adulto, Viejo }

    public static float HeightFor(TreeAge age, System.Random rnd)
    {
        float r = (float)rnd.NextDouble();
        switch (age)
        {
            case TreeAge.Brote: return Mathf.Lerp(2.5f, 4.5f, r);
            case TreeAge.Joven: return Mathf.Lerp(7f, 10f, r);
            case TreeAge.Adulto: return Mathf.Lerp(14f, 18f, r);
            default: return Mathf.Lerp(20f, 24f, r);
        }
    }

    /// <summary>
    /// Arboledas esparcidas: un árbol madre (adulto o viejo) con jóvenes y brotes alrededor (su descendencia), y árboles
    /// sueltos de cualquier edad. Evita <paramref name="avoid"/> (x, z, radio) y el mar. Devuelve (posición, edad).
    /// </summary>
    public static List<(Vector3 pos, TreeAge age, float height)> PlanForest(List<Vector3> avoid, int seed, int groves, int loose)
    {
        var rnd = new System.Random(seed);
        float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
        var trees = new List<(Vector3, TreeAge, float)>();

        bool Free(Vector3 p, float minSpacing)
        {
            if (Mathf.Abs(p.x) > Half - 8f || Mathf.Abs(p.z) > Half - 8f || p.x > CoastX(p.z) - 12f) return false;
            if (!NaturalIce(p.x, p.z) || !NaturalIce(p.x + 3f, p.z) || !NaturalIce(p.x - 3f, p.z) || !NaturalIce(p.x, p.z + 3f) || !NaturalIce(p.x, p.z - 3f)) return false;
            foreach (Vector3 a in avoid) if ((p.x - a.x) * (p.x - a.x) + (p.z - a.y) * (p.z - a.y) < a.z * a.z) return false;
            foreach (var t in trees) if (Vector3.Distance(p, t.Item1) < minSpacing) return false;
            return true;
        }

        for (int g = 0, tries = 0; g < groves && tries < 2000; tries++)
        {
            Vector3 c = new Vector3(R(-Half + 20f, 150f), 0f, R(-Half + 20f, Half - 20f));
            if (!Free(c, 45f)) continue;
            TreeAge motherAge = rnd.NextDouble() < 0.35 ? TreeAge.Viejo : TreeAge.Adulto;
            trees.Add((c, motherAge, HeightFor(motherAge, rnd)));
            int kids = 2 + rnd.Next(3);
            for (int k = 0, kt = 0; k < kids && kt < 40; kt++)
            {
                TreeAge age = rnd.NextDouble() < 0.5 ? TreeAge.Brote : TreeAge.Joven;
                float ang = R(0f, Mathf.PI * 2f), d = R(10f, 18f);
                Vector3 p = c + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * d;
                if (!Free(p, age == TreeAge.Brote ? 5f : 8f)) continue;
                trees.Add((p, age, HeightFor(age, rnd)));
                k++;
            }
            g++;
        }
        for (int l = 0, tries = 0; l < loose && tries < 2000; tries++)
        {
            Vector3 p = new Vector3(R(-Half + 15f, 150f), 0f, R(-Half + 15f, Half - 15f));
            if (!Free(p, 22f)) continue;
            double r = rnd.NextDouble();
            TreeAge age = r < 0.3 ? TreeAge.Brote : r < 0.6 ? TreeAge.Joven : r < 0.9 ? TreeAge.Adulto : TreeAge.Viejo;
            trees.Add((p, age, HeightFor(age, rnd)));
            l++;
        }
        return trees;
    }
}
