using UnityEngine;

/// <summary>
/// ARBOLEDA RESIDENCIAL (docs/construcciones-elevadas.md): los dormitorios de los humanos, todos ELEVADOS a 7 m. En el
/// centro, una plataforma con escalera; alrededor, seis árboles grandes (no carnívoros). Seis cabañas se APOYAN EN DOS
/// ÁRBOLES a la vez (vigas a cada tronco) y otras seis, más afuera, se sostienen sobre COLUMNAS. Pasarelas colgantes
/// las unen. Cada cabaña es el dormitorio de una persona: cama, mesilla con lámpara, estante, ventana, puerta y su nombre.
/// </summary>
public static class ResidentialBuilder
{
    public const float DeckY = 7f;
    static readonly Color Wood = new Color(0.62f, 0.52f, 0.42f), WoodDark = new Color(0.48f, 0.4f, 0.33f), Roof = new Color(0.55f, 0.66f, 0.75f);

    public static readonly string[] Owners =
    {
        "Player", "Magnate", "Linduarda", "Goluis", "Panterilia", "Gohageneis", "Irosene", "Lizmibet", "JefeConstruccion", "Sadhguru", "Andrea",   // solo personajes del juego
    };
    static readonly string[] Shown =
    {
        "Kushal", "la Magnate", "Linduarda", "Goluis", "Panterilia", "Gohageneis", "Irosene", "Lizmibet", "el jefe de construcción", "Sadhguru", "Andrea",
    };

    public static Transform Build(Transform parent, Vector3 center)
    {
        Transform root = new GameObject("ArboledaResidencial").transform;
        root.SetParent(parent, true);
        root.position = center;

        // Plataforma central y su escalera (rampa para andar + peldaños).
        Plank(root, new Vector3(0f, DeckY, 0f), new Vector3(6f, 0.3f, 6f), "Plataforma central");
        for (int i = 0; i < 4; i++) Solid(root, new Vector3(i % 2 == 0 ? -2.6f : 2.6f, DeckY * 0.5f, i < 2 ? -2.6f : 2.6f), new Vector3(0.4f, DeckY, 0.4f), Wood, "Columna");
        Stairs(root, new Vector3(0f, 0f, -3f), DeckY);

        // Árboles de apoyo en anillo (r 12, a 30°+60°k).
        var trees = new Vector3[6];
        for (int k = 0; k < 6; k++)
        {
            float a = (30f + 60f * k) * Mathf.Deg2Rad;
            trees[k] = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 13f;
            CubeTree.Build(root, $"ArbolResidencial_{k + 1}", center + trees[k], 7100 + k * 13, 22f, carnivorous: false);
        }

        int owner = 0;
        for (int k = 0; k < 6; k++)
        {
            // Cabaña ENTRE DOS ÁRBOLES (a 60°k, en la cuerda entre los troncos k-1 y k), con vigas a ambos troncos.
            float a = 60f * k * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            Vector3 c = dir * 11.2f;
            Transform cab = Cabin(root, c, -dir, owner++, $"Cabaña entre árboles {k + 1}");
            Vector3 t1 = trees[(k + 5) % 6], t2 = trees[k];
            Beam(root, c + Vector3.up * (DeckY - 0.35f), t1 + Vector3.up * (DeckY - 0.35f));
            Beam(root, c + Vector3.up * (DeckY - 0.35f), t2 + Vector3.up * (DeckY - 0.35f));
            Bridge(root, dir * 3f, dir * 9f);

            // Cabaña SOBRE COLUMNAS, detrás (más afuera, en la misma dirección), unida por una pasarela a la puerta trasera.
            Vector3 dir2 = dir;
            Vector3 c2 = dir2 * 20f;
            Transform cab2 = Cabin(root, c2, -dir2, owner++, $"Cabaña sobre columnas {k + 1}");
            for (int i = 0; i < 4; i++) Solid(cab2, new Vector3(i % 2 == 0 ? -1.9f : 1.9f, -DeckY * 0.5f, i < 2 ? -1.7f : 1.7f), new Vector3(0.3f, DeckY, 0.3f), Wood, "Columna");
            Bridge(root, c + dir * 2.1f, c2 - dir * 2.1f);
        }
        SoftGround.Plant(root, center + Vector3.down * 0.1f, new Vector2(48f, 48f), 77);   // colchón de musgo bajo toda la arboleda
        WorldLabel wl = root.gameObject.AddComponent<WorldLabel>(); wl.text = "Arboleda residencial"; wl.height = DeckY + 5f;
        return root;
    }

    // Una cabaña-dormitorio (4,4 × 4 m) con la puerta mirando a `door`.
    static Transform Cabin(Transform root, Vector3 local, Vector3 door, int ownerIdx, string name)
    {
        Transform t = new GameObject(name).transform;
        t.SetParent(root, false);
        t.localPosition = local + Vector3.up * DeckY;
        t.localRotation = Quaternion.LookRotation(-door);   // su -Z mira a la puerta (hacia dentro de la arboleda)
        Plank(t, Vector3.zero, new Vector3(4.4f, 0.25f, 4f), "Suelo");
        float h = 2.5f;
        // Fondo: con puerta trasera (las cabañas entre árboles dan paso a la cabaña sobre columnas de detrás).
        Solid(t, new Vector3(-1.45f, h * 0.5f, 2f), new Vector3(1.5f, h, 0.12f), Wood, "Pared");
        Solid(t, new Vector3(1.45f, h * 0.5f, 2f), new Vector3(1.5f, h, 0.12f), Wood, "Pared");
        Solid(t, new Vector3(0f, h - 0.3f, 2f), new Vector3(1.4f, 0.6f, 0.12f), Wood, "Dintel");
        Solid(t, new Vector3(-2.2f, h * 0.5f, 0f), new Vector3(0.12f, h, 4f), Wood, "Pared");
        Solid(t, new Vector3(2.2f, h * 0.5f, 0f), new Vector3(0.12f, h, 4f), Wood, "Pared");
        Solid(t, new Vector3(-1.45f, h * 0.5f, -2f), new Vector3(1.5f, h, 0.12f), Wood, "Pared");             // frente con hueco de puerta
        Solid(t, new Vector3(1.45f, h * 0.5f, -2f), new Vector3(1.5f, h, 0.12f), Wood, "Pared");
        Solid(t, new Vector3(0f, h - 0.3f, -2f), new Vector3(1.4f, 0.6f, 0.12f), Wood, "Dintel");
        KitchenProps.C(t, new Vector3(2.21f, 1.5f, 0.3f), new Vector3(0.02f, 0.7f, 1f), new Color(0.75f, 0.88f, 0.95f), 0f, "Ventana");
        for (int k = 0; k < 3; k++)
            KitchenProps.C(t, new Vector3(0f, h + 0.15f + k * 0.28f, 0f), new Vector3(5f - k * 1.6f, 0.28f, 4.6f - k * 1.4f), k % 2 == 0 ? Roof : WoodDark, 0f, "Tejado");
        // Cama, mesilla con lámpara y estante.
        Transform bed = KitchenProps.Root(t, "Cama", new Vector3(-1.1f, 0.12f, 0.9f));
        KitchenProps.C(bed, new Vector3(0f, 0.2f, 0f), new Vector3(1f, 0.3f, 2f), WoodDark, 0f, "Armazon");
        KitchenProps.C(bed, new Vector3(0f, 0.42f, 0f), new Vector3(0.92f, 0.16f, 1.9f), new Color(0.95f, 0.95f, 0.93f), 0f, "Colchon");
        KitchenProps.C(bed, new Vector3(0f, 0.52f, 0.2f), new Vector3(0.94f, 0.06f, 1.3f), Blanket(ownerIdx), 0f, "Manta");
        KitchenProps.C(bed, new Vector3(0f, 0.55f, 0.75f), new Vector3(0.6f, 0.1f, 0.3f), Color.white, 0f, "Almohada");
        KitchenProps.C(t, new Vector3(0.2f, 0.4f, 1.6f), new Vector3(0.45f, 0.55f, 0.4f), Wood, 0f, "Mesilla");
        KitchenProps.C(t, new Vector3(0.2f, 0.8f, 1.6f), new Vector3(0.16f, 0.25f, 0.16f), new Color(1f, 0.88f, 0.55f), 0f, "Lampara");
        KitchenProps.C(t, new Vector3(1.6f, 1.2f, 1.75f), new Vector3(1f, 0.05f, 0.35f), WoodDark, 0f, "Estante");
        KitchenProps.C(t, new Vector3(1.4f, 1.33f, 1.75f), new Vector3(0.25f, 0.2f, 0.25f), new Color(0.8f, 0.6f, 0.5f), 0f, "Libros");
        // BAÑO propio en la esquina: tabique, inodoro seco de compostaje y lavabo.
        Solid(t, new Vector3(1.15f, h * 0.5f, -0.8f), new Vector3(0.08f, h, 2.3f), Wood, "Tabique_bano");
        KitchenProps.C(t, new Vector3(1.75f, 0.22f, -1.5f), new Vector3(0.42f, 0.44f, 0.5f), Wood, 0f, "Inodoro_seco");
        KitchenProps.C(t, new Vector3(1.75f, 0.45f, -1.5f), new Vector3(0.38f, 0.03f, 0.42f), Color.white, 0f, "Asiento");
        KitchenProps.C(t, new Vector3(1.85f, 0.8f, -0.35f), new Vector3(0.45f, 0.12f, 0.35f), Color.white, 0f, "Lavabo");
        Bedroom room = t.gameObject.AddComponent<Bedroom>();
        room.owner = ownerIdx < Owners.Length ? Owners[ownerIdx] : null;   // las demás: para quien llegue al santuario
        room.bed = bed;
        WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = ownerIdx < Shown.Length ? $"Habitación de {Shown[ownerIdx]}" : "Habitación libre (para recién llegados)"; wl.height = h + 1.2f;
        return t;
    }

    static Color Blanket(int i)
    {
        Color[] c = { new Color(0.62f, 0.8f, 0.92f), new Color(0.82f, 0.72f, 0.92f), new Color(0.95f, 0.82f, 0.72f), new Color(0.72f, 0.88f, 0.75f) };
        return c[i % c.Length];
    }

    static void Beam(Transform root, Vector3 a, Vector3 b)
    {
        Vector3 d = b - a;
        Transform t = KitchenProps.C(root, (a + b) * 0.5f, new Vector3(0.25f, 0.25f, d.magnitude), WoodDark, 0f, "Viga");
        t.localRotation = Quaternion.LookRotation(d);
    }

    // Pasarela colgante (con collider para andar) entre dos puntos a la altura de las plataformas.
    static void Bridge(Transform root, Vector3 a, Vector3 b)
    {
        a.y = b.y = DeckY;
        Vector3 d = b - a; float len = d.magnitude;
        Transform t = new GameObject("Pasarela").transform; t.SetParent(root, false);
        t.localPosition = (a + b) * 0.5f; t.localRotation = Quaternion.LookRotation(d);
        Plank(t, Vector3.zero, new Vector3(1.4f, 0.12f, len), "Tablones");
        for (int s = -1; s <= 1; s += 2)
        {
            Solid(t, new Vector3(s * 0.7f, 0.9f, 0f), new Vector3(0.05f, 0.05f, len), new Color(0.75f, 0.7f, 0.6f), "Cuerda");
            Transform bar = Solid(t, new Vector3(s * 0.7f, 2f, 0f), new Vector3(0.05f, 4f, len), Wood, "Barrera");   // invisible, 4 m (ni saltando): no se salta
            bar.GetComponent<Renderer>().enabled = false;
            for (float z = -len * 0.5f; z <= len * 0.5f; z += 1.2f) KitchenProps.C(t, new Vector3(s * 0.7f, 0.45f, z), new Vector3(0.04f, 0.9f, 0.04f), WoodDark, 0f, "Poste");
        }
    }

    static void Stairs(Transform root, Vector3 top, float h)
    {
        float rise = 0.25f, tread = 0.35f; int n = Mathf.CeilToInt(h / rise); float run = n * tread;
        Transform st = new GameObject("Escalera").transform; st.SetParent(root, false); st.localPosition = new Vector3(top.x, 0f, top.z);
        float len = Mathf.Sqrt(run * run + h * h), ang = Mathf.Atan2(h, run) * Mathf.Rad2Deg;
        Transform ramp = Solid(st, new Vector3(0f, h * 0.5f - 0.05f, -run * 0.5f), new Vector3(2f, 0.1f, len), Wood, "Rampa");
        ramp.localRotation = Quaternion.Euler(-ang, 0f, 0f);
        ramp.GetComponent<Renderer>().enabled = false;
        for (int i = 0; i < n; i++)
            KitchenProps.C(st, new Vector3(0f, (i + 0.5f) * rise, -run + (i + 0.5f) * tread), new Vector3(2f, rise, tread * 1.02f), i % 2 == 0 ? Wood : WoodDark, 0f, "Peldaño");
        for (int sx = -1; sx <= 1; sx += 2)
        {
            Transform hr = Solid(st, new Vector3(sx * 1.1f, h * 0.5f + 1f, -run * 0.5f), new Vector3(0.06f, 0.06f, len), Wood, "Pasamanos");
            hr.localRotation = Quaternion.Euler(-ang, 0f, 0f);
            Transform wall = Solid(st, new Vector3(sx * 1.15f, (h + 3f) * 0.5f, -run * 0.5f), new Vector3(0.08f, h + 3f, run), Wood, "Escalera_Barrera");   // pared VERTICAL del suelo a 3 m sobre la plataforma
            wall.GetComponent<Renderer>().enabled = false;   // nadie salta de lado por la escalera
        }
    }

    static void Plank(Transform parent, Vector3 local, Vector3 size, string name) => Solid(parent, local, size, Wood, name);

    static Transform Solid(Transform parent, Vector3 local, Vector3 size, Color c, string name)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; g.transform.SetParent(parent, false);
        g.transform.localPosition = local; g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(c);
        return g.transform;
    }
}
