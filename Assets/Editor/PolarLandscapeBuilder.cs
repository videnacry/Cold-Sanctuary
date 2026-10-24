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
    public const float Half = 250f;          // el mapa cubre [-250, 250]
    const float Cell = 2.5f;                 // resolución de las islas
    const float IceTop = 0f, IceThickness = 1.5f;
    public const float WaterLevel = -0.25f;  // el agua asoma 25 cm bajo el borde del hielo (se sale nadando: un escalón)

    /// <summary>
    /// SEIS CASQUETES (no más): cada casquete es una masa de hielo continua hecha de varios LÓBULOS (círculos que se
    /// solapan), y entre casquetes hay agua que se cruza NADANDO. Las áreas humanas y la naturaleza comparten casquete
    /// (los árboles crecen junto a las áreas elevadas). Rodean una gran LAGUNA central donde viven los animales marinos.
    /// (casquete, x, z, radio). Los lóbulos de un mismo casquete se solapan; entre casquetes quedan ≥ 8 m de agua.
    /// </summary>
    public static readonly (string name, float x, float z, float r)[] Islands =
    {
        // 1 · NORTE (la aldea): cocina, enfermería (una sola, para personas y animales), arboleda residencial,
        //     casa del árbol de guardia y el bosque comestible.
        ("Norte:Cocina",            5f,   95f, 30f),
        ("Norte:Enfermeria",       55f,   97f, 28f),
        ("Norte:Paso",            -28f,   95f, 22f),
        ("Norte:Residencial",     -60f,   95f, 30f),
        ("Norte:Guardia",         -25f,   65f, 20f),
        ("Norte:Bosque_Comestible",-82f,  52f, 26f),
        // 2 · CENTRO: el hub (personajes, la Magnate, el área del huerto), el arrozal, la colonia de pingüinos y el textil.
        ("Centro:Hub",              5f,  -10f, 42f),
        ("Centro:Arrozal",        -55f,  -15f, 28f),
        ("Centro:Pinguinos",       55f,  -22f, 18f),
        ("Centro:Textil",          52f,  -52f, 18f),
        // 3 · SUR: los huertos-gremio (rotación, milpa, andino).
        ("Sur:Rotacion",           15f,  -95f, 22f),
        ("Sur:Paso",              -15f,  -90f, 20f),
        ("Sur:Milpa",             -45f,  -80f, 24f),
        ("Sur:Andina",            -10f, -118f, 22f),
        // 4 · ESTE: sala de yoga, mecánica y el territorio de los osos.
        ("Este:SalaYoga",         109f,   38f, 24f),
        ("Este:Paso",             114f,   10f, 20f),
        ("Este:Mecanica_Osos",    118f,  -20f, 30f),
        // 5 · BOSQUE OESTE (salvaje): árboles carnívoros de hielo, abejas y arañas de las nieves.
        ("BosqueOeste:Bosque_1", -140f,  -30f, 34f),
        ("BosqueOeste:Bosque_2", -150f,    5f, 26f),
        ("BosqueOeste:Bosque_3", -160f,   45f, 32f),
        // 6 · BOSQUE NORTE (salvaje).
        ("BosqueNorte:Bosque_4",   40f,  175f, 32f),
        ("BosqueNorte:Bosque_5",   75f,  170f, 24f),
        ("BosqueNorte:Bosque_6",  110f,  160f, 30f),
        // 3 · SUR (añadidos al final para no mover los índices): INVESTIGACIÓN Y PRODUCCIÓN (Berón 2026-10-24), junto a la
        //     planta atómica: Química, Biología y la central nuclear.
        ("Sur:Quimica",            52f, -110f, 24f),
        ("Sur:Biologia",           92f, -112f, 26f),
        ("Sur:Central",            60f, -162f, 34f),
    };

    /// <summary>Nombre del casquete al que pertenece el lóbulo <paramref name="i"/> (hay 6).</summary>
    public static string CapOf(int i) => Islands[i].name.Split(':')[0];

    public const int CapCount = 6;

    /// <summary>La laguna central (sin islas): aquí viven ballena, orca, focas y bancos de peces.</summary>
    public static readonly Vector3 Lagoon = new Vector3(60f, 0f, 25f);
    public const float LagoonRadius = 28f;

    /// <summary>Zonas que deben ser hielo firme aunque caigan fuera de una isla (se usan con moderación).</summary>
    public static readonly List<Vector3> Protect = new List<Vector3>();

    static float IslandRadius(int i, float x, float z)
    {
        var isl = Islands[i];
        float a = Mathf.Atan2(z - isl.z, x - isl.x);
        return isl.r * (1f + 0.06f * Mathf.Sin(a * 5f + i) + 0.03f * Mathf.Sin(a * 11f + 2f * i));   // orilla irregular, sin unir casquetes
    }

    /// <summary>¿Hay hielo en (x, z)? = dentro de alguna isla.</summary>
    public static bool NaturalIce(float x, float z)
    {
        if (Mathf.Abs(x) > Half || Mathf.Abs(z) > Half) return false;
        for (int i = 0; i < Islands.Length; i++)
        {
            float dx = x - Islands[i].x, dz = z - Islands[i].z;
            float r = IslandRadius(i, x, z);
            if (dx * dx + dz * dz < r * r) return true;
        }
        return false;
    }

    /// <summary>Isla que contiene (x, z), o -1.</summary>
    public static int IslandAt(float x, float z)
    {
        for (int i = 0; i < Islands.Length; i++)
        {
            float dx = x - Islands[i].x, dz = z - Islands[i].z, r = IslandRadius(i, x, z);
            if (dx * dx + dz * dz < r * r) return i;
        }
        return -1;
    }

    /// <summary>
    /// ENLACES DE NADO para la IA: entre islas vecinas (a menos de <paramref name="maxGap"/> m de agua) un NavMeshLink de
    /// orilla a orilla, así compañeros y hechizos `walkTo` cruzan a nado de una isla a otra (el NavMesh no pisa agua).
    /// </summary>
    public static int BuildSwimLinks(Transform parent, float maxGap = 26f)
    {
        GameObject root = new GameObject("EnlacesDeNado"); root.transform.SetParent(parent, false);
        int n = 0;
        for (int i = 0; i < Islands.Length; i++)
            for (int j = i + 1; j < Islands.Length; j++)
            {
                Vector3 a = new Vector3(Islands[i].x, 0f, Islands[i].z), b = new Vector3(Islands[j].x, 0f, Islands[j].z);
                float d = Vector3.Distance(a, b), gap = d - Islands[i].r - Islands[j].r;
                if (gap > maxGap || gap < -Mathf.Min(Islands[i].r, Islands[j].r)) continue;
                Vector3 dir = (b - a) / d;
                Vector3 pa = a + dir * Islands[i].r * 0.8f, pb = b - dir * Islands[j].r * 0.8f;
                if (gap <= 0f) continue;   // se tocan: el NavMesh ya las une
                GameObject l = new GameObject($"Nado_{Islands[i].name}_{Islands[j].name}");
                l.transform.SetParent(root.transform, false);
                NavMeshLink link = l.AddComponent<NavMeshLink>();
                link.startPoint = pa; link.endPoint = pb;
                link.width = 2f; link.bidirectional = true; link.costModifier = 3f;   // nadar cuesta más que andar
                n++;
            }
        return n;
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
            case TreeAge.Joven: return Mathf.Lerp(9f, 14f, r);
            case TreeAge.Adulto: return Mathf.Lerp(22f, 32f, r);   // los árboles de hielo adultos: 20–40 m
            default: return Mathf.Lerp(32f, 40f, r);
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

        int whyEdge = 0, whyIce = 0, whyAvoid = 0, whyTree = 0;
        bool Free(Vector3 p, float minSpacing)
        {
            if (Mathf.Abs(p.x) > Half - 8f || Mathf.Abs(p.z) > Half - 8f) { whyEdge++; return false; }
            if (!NaturalIce(p.x, p.z) || !NaturalIce(p.x + 3f, p.z) || !NaturalIce(p.x - 3f, p.z) || !NaturalIce(p.x, p.z + 3f) || !NaturalIce(p.x, p.z - 3f)) { whyIce++; return false; }
            foreach (Vector3 a in avoid) if ((p.x - a.x) * (p.x - a.x) + (p.z - a.y) * (p.z - a.y) < a.z * a.z) { whyAvoid++; return false; }
            foreach (var t in trees) if (Vector3.Distance(p, t.Item1) < minSpacing) { whyTree++; return false; }
            return true;
        }

        // Una arboleda por isla de bosque (y, si sobran, más arboledas en esas mismas islas).
        var forestIslands = new List<int>();
        for (int k = 0; k < Islands.Length; k++) if (Islands[k].name.StartsWith("Bosque")) forestIslands.Add(k);   // solo los casquetes salvajes
        int islandTries = 0;
        for (int g = 0, tries = 0; g < groves && tries < 4000; tries++)
        {
            var isl = Islands[forestIslands[g % forestIslands.Count]];
            float jit = g < forestIslands.Count ? 0.35f : 0.65f;   // 2ª arboleda de la isla: hacia la orilla, lejos de la 1ª
            Vector3 c = new Vector3(isl.x + R(-isl.r, isl.r) * jit, 0f, isl.z + R(-isl.r, isl.r) * jit);
            if (!Free(c, 16f))
            {
                if (++islandTries > 80) { Debug.Log($"[IceFlora] la isla {isl.name} no admite otra arboleda (borde {whyEdge}, agua {whyIce}, zona a evitar {whyAvoid}, otro árbol {whyTree})."); whyEdge = whyIce = whyAvoid = whyTree = 0; islandTries = 0; g++; }
                continue;
            }
            islandTries = 0;
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
            Vector3 p = new Vector3(R(-Half + 15f, Half - 15f), 0f, R(-Half + 15f, Half - 15f));
            if (!Free(p, 18f)) continue;
            double r = rnd.NextDouble();
            TreeAge age = r < 0.3 ? TreeAge.Brote : r < 0.6 ? TreeAge.Joven : r < 0.9 ? TreeAge.Adulto : TreeAge.Viejo;
            trees.Add((p, age, HeightFor(age, rnd)));
            l++;
        }
        return trees;
    }
}
