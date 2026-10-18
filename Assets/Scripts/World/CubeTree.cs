using UnityEngine;

/// <summary>
/// ÁRBOL CARNÍVORO DE HIELO hecho de BLOQUES (estilo Minecraft, sin modelo 3D) — docs/ice-sanctuary-ecology.md §2:
/// tronco alto que se estrecha, varios PISOS de ramas largas que suben y se abren, y en la punta de cada rama una
/// PLATAFORMA plana (con collider) donde pueden posarse animales y anidar las abejas de las nieves; algunas llevan nido.
/// El árbol sigue siendo el mismo depredador (<see cref="IceCarnivorousTree"/>: señuelo de olor + sopor + crece al
/// absorber), con radios a la escala del árbol. Determinista por semilla: cada árbol sale distinto pero reproducible.
/// </summary>
public static class CubeTree
{
    static Material _bark, _branch, _leaf, _platform, _nest;

    static Material Mat(ref Material cache, Color c)
    {
        if (cache != null) return cache;
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        cache = new Material(sh) { color = c };
        if (cache.HasProperty("_BaseColor")) cache.SetColor("_BaseColor", c);
        return cache;
    }

    public static GameObject Build(Transform parent, string name, Vector3 groundPos, int seed, float height = 16f, bool carnivorous = true)
    {
        var rnd = new System.Random(seed);
        float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);

        Material bark = Mat(ref _bark, new Color(0.55f, 0.72f, 0.80f));       // hielo opaco azulado
        Material branchMat = Mat(ref _branch, new Color(0.70f, 0.86f, 0.92f));
        Material leaf = Mat(ref _leaf, new Color(0.45f, 0.75f, 0.85f));       // "follaje" de escarcha
        Material platformMat = Mat(ref _platform, new Color(0.85f, 0.94f, 0.98f));
        Material nestMat = Mat(ref _nest, new Color(0.90f, 0.78f, 0.35f));    // nidos de miel de las abejas

        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = groundPos;

        // EDAD por altura: brote (<6 m) un piso y sin posaderos; joven (<12 m) dos pisos; adulto tres; viejo (≥20 m) con
        // más raíces y una rama seca. El grosor sigue a la altura.
        float girth = Mathf.Clamp(height / 16f, 0.3f, height > 60f ? 2.1f : 1.35f);   // el gigante de 83 m, más grueso
        int tiers = height < 6f ? 1 : height < 12f ? 2 : 3;
        bool old = height >= 20f;
        // TRONCO: bloques apilados que se estrechan, con un leve giro (más orgánico que un cilindro).
        int trunkBlocks = Mathf.Max(4, Mathf.RoundToInt(height / 2f));
        float block = height / trunkBlocks;
        for (int i = 0; i < trunkBlocks; i++)
        {
            float w = Mathf.Lerp(2.2f, 0.9f, i / (float)(trunkBlocks - 1)) * girth;
            Block(root.transform, "Tronco", new Vector3(0f, block * (i + 0.5f), 0f), new Vector3(w, block * 1.02f, w),
                  Quaternion.Euler(0f, i * 11f, 0f), bark, true);
        }
        // Raíces: cuatro contrafuertes en la base.
        for (int k = 0, roots = old ? 7 : 4; k < roots; k++)
        {
            float a = k * 360f / roots + R(-15f, 15f);
            Vector3 d = Quaternion.Euler(0f, a, 0f) * Vector3.forward;
            Block(root.transform, "Raiz", d * 1.6f * girth + Vector3.up * 0.4f * girth, new Vector3(0.7f, 0.8f, 2.2f) * girth,
                  Quaternion.LookRotation(d) * Quaternion.Euler(18f, 0f, 0f), bark, true);
        }

        // RAMAS: 3 pisos; cada rama es una cadena de bloques que sube y se abre, con plataforma en la punta.
        int nests = 0;
        for (int tier = 0; tier < tiers; tier++)
        {
            float tf = tiers > 1 ? tier / (float)(tiers - 1) : 0.5f;
            float y = height * Mathf.Lerp(0.38f, 0.82f, tf);
            int count = height < 6f ? 3 : 5 - tier;
            float reach = Mathf.Lerp(7f, 3.5f, tf) * (height / 16f);
            float offset = R(0f, 360f);
            for (int b = 0; b < count; b++)
            {
                float yaw = offset + b * 360f / count + R(-12f, 12f);
                float pitch = R(18f, 35f);
                Vector3 p = new Vector3(0f, y + R(-0.6f, 0.6f), 0f);
                int segs = 3;
                float segLen = reach / segs;
                float thick = Mathf.Lerp(0.7f, 0.45f, tf) * Mathf.Max(0.5f, girth);
                for (int s = 0; s < segs; s++)
                {
                    Vector3 dir = Quaternion.Euler(-pitch + s * 9f, yaw, 0f) * Vector3.forward;   // sube y se va aplanando
                    Block(root.transform, "Rama", p + dir * segLen * 0.5f, new Vector3(thick, thick, segLen * 1.05f),
                          Quaternion.LookRotation(dir), branchMat, true);
                    p += dir * segLen;
                    thick *= 0.8f;
                }
                if (height < 9f) { Block(root.transform, "Escarcha", p + Vector3.up * 0.3f, Vector3.one * R(0.5f, 0.8f), Quaternion.Euler(0f, R(0f, 90f), 0f), leaf, false); continue; }
                // Plataforma de la punta (posadero) + mata de escarcha encima; a veces un nido.
                GameObject plat = Block(root.transform, "Rama_Plataforma", p + Vector3.up * 0.1f, new Vector3(1.8f, 0.2f, 1.8f),
                                        Quaternion.Euler(0f, yaw, 0f), platformMat, true);
                Block(root.transform, "Escarcha", p + Vector3.up * 0.75f, Vector3.one * R(0.9f, 1.4f),
                      Quaternion.Euler(R(0f, 30f), R(0f, 90f), 0f), leaf, false);
                if (height >= 12f && (b + tier) % 2 == 0)
                {
                    Honeycomb(plat.transform.parent, p + new Vector3(0.5f, -0.05f, 0.2f), nestMat, (b * 7 + tier) * 13);
                    nests++;
                }
            }
        }
        if (old)   // rama seca y quebrada: el árbol viejo se nota
        {
            Vector3 dd = Quaternion.Euler(-5f, R(0f, 360f), 0f) * Vector3.forward;
            Block(root.transform, "Rama_Seca", new Vector3(0f, height * 0.25f, 0f) + dd * 1.8f, new Vector3(0.5f, 0.5f, 3.2f), Quaternion.LookRotation(dd) * Quaternion.Euler(25f, 0f, 0f), bark, true);
        }
        // Copa: dos bloques de escarcha en lo alto.
        Block(root.transform, "Copa", new Vector3(0f, height + 0.8f * girth, 0f), new Vector3(2.6f, 1.6f, 2.6f) * Mathf.Max(0.45f, girth), Quaternion.Euler(0f, 20f, 0f), leaf, false);
        Block(root.transform, "Copa", new Vector3(0f, height + 2.0f * girth, 0f), new Vector3(1.6f, 1.2f, 1.6f) * Mathf.Max(0.45f, girth), Quaternion.Euler(0f, 55f, 0f), leaf, false);

        if (height < 6f || !carnivorous) return root;   // un brote aún no caza (y el árbol de la guardia no caza nunca)
        // El árbol depredador, con radios a su escala (señuelo amplio, sopor bajo la copa).
        IceCarnivorousTree t = root.AddComponent<IceCarnivorousTree>();
        t.lureRadius = 10f + height;
        t.killRadius = 3f + height * 0.15f;
        t.growthPerKill = 0.05f; t.maxScale = 1.5f;   // crece con cada presa, pero un árbol de 16 m no puede triplicarse
        return root;
    }


    // PANALES de las abejas de las nieves (20 cm): una colonia silvestre cuelga de la rama varias láminas de cera
    // paralelas (panales) separadas por un "paso de abeja"; cada lámina tiene celdillas hexagonales a ambos lados (aquí,
    // en filas alternas desplazadas medio hueco), unas OPERCULADAS (tapadas de cera clara, con miel) y otras abiertas
    // (oscuras, con cría o néctar). Arriba, la cera que las pega a la rama; abajo, la piquera por donde entran.
    static Material _wax, _honey, _cellDark, _capped;
    static void Honeycomb(Transform parent, Vector3 localTop, Material nestMat, int seed)
    {
        var rnd = new System.Random(seed);
        Material wax = Mat(ref _wax, new Color(0.93f, 0.8f, 0.42f));
        Material honey = Mat(ref _honey, new Color(0.85f, 0.55f, 0.12f));
        Material dark = Mat(ref _cellDark, new Color(0.45f, 0.3f, 0.12f));
        Material capped = Mat(ref _capped, new Color(0.98f, 0.9f, 0.62f));
        GameObject root = new GameObject("Nido_Abejas");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localTop;
        root.transform.localRotation = Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f);
        Block(root.transform, "Cera_de_union", new Vector3(0f, 0.04f, 0f), new Vector3(0.95f, 0.08f, 0.75f), Quaternion.identity, wax, false);
        const int combs = 4; const float gap = 0.18f, w = 0.85f, h = 0.75f, cell = 0.07f;
        for (int c = 0; c < combs; c++)
        {
            float x = (c - (combs - 1) * 0.5f) * gap;
            float hh = h * (c == 0 || c == combs - 1 ? 0.8f : 1f);   // los de fuera, algo más cortos (forma de lengua)
            Transform comb = Block(root.transform, "Panal", new Vector3(x, -hh * 0.5f, 0f), new Vector3(0.05f, hh, w), Quaternion.identity, honey, false).transform;
            Block(root.transform, "Borde_panal", new Vector3(x, -hh + 0.02f, 0f), new Vector3(0.06f, 0.05f, w * 0.7f), Quaternion.identity, wax, false);
            if (c != 0 && c != combs - 1) continue;   // celdillas visibles solo en las caras exteriores
            float side = c == 0 ? -1f : 1f;
            int rows = Mathf.FloorToInt((hh - 0.08f) / (cell * 0.87f)), cols = Mathf.FloorToInt((w - 0.08f) / cell);
            for (int rr = 0; rr < rows; rr++)
                for (int k = 0; k < cols; k++)
                {
                    float z = -w * 0.5f + 0.05f + k * cell + (rr % 2) * cell * 0.5f;
                    if (z > w * 0.5f - 0.04f) continue;
                    float y = -0.06f - rr * cell * 0.87f;
                    bool cap = rnd.NextDouble() < 0.45 || y > -0.25f;   // la miel se guarda arriba: operculada
                    Block(root.transform, "Celdilla", new Vector3(x + side * 0.027f, y, z), new Vector3(0.012f, cell * 0.78f, cell * 0.78f), Quaternion.Euler(30f, 0f, 0f), cap ? capped : dark, false);
                }
        }
        Block(root.transform, "Piquera", new Vector3(0f, -h - 0.02f, 0f), new Vector3(0.3f, 0.06f, 0.18f), Quaternion.identity, dark, false);
        root.AddComponent<Hive>();   // la colmena: estados de las celdillas (cría, néctar, miel, polen) que trabajan las abejas
    }
    static GameObject Block(Transform parent, string name, Vector3 localPos, Vector3 size, Quaternion rot, Material m, bool solid)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        g.transform.SetParent(parent, false);
        g.transform.localPosition = localPos;
        g.transform.localRotation = rot;
        g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = m;
        if (!solid) Object.DestroyImmediate(g.GetComponent<Collider>());
        return g;
    }
}
