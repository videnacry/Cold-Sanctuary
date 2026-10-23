using UnityEngine;

/// <summary>
/// ARBOLEDA RESIDENCIAL (docs/construcciones-elevadas.md): los dormitorios de los humanos, todos ELEVADOS a 12 m. En el
/// centro, una plataforma con escalera vertical; alrededor, seis árboles grandes (no carnívoros). Seis cabañas se APOYAN EN DOS
/// ÁRBOLES a la vez (vigas a cada tronco) y, ENCIMA de cada una, a 5 m, otra cabaña sobre cuatro columnas, con su escalera
/// vertical desde la pasarela (§Pisos: antes eran seis cabañas más afuera con seis pasarelas más). Cada cabaña es el
/// dormitorio de una persona: cama, mesilla, armario y su nombre.
/// </summary>
public static class ResidentialBuilder
{
    public const float DeckY = 12f;   // como las demás áreas humanas: a 12 m, el bosque no queda en sombra
    public const float StoryGap = 6f; // de una cabaña a la de encima (+1 m, Berón 2026-10-23)
    public const float PadAngle = 0f; // hacia dónde sale la pista de alfombras (desde la cabaña de 0°)
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

        // Plataforma central (8 × 8) y su ESCALERA VERTICAL por el lado sur (por ahí se llega desde la entrada).
        Plank(root, new Vector3(0f, DeckY, 0f), new Vector3(8f, 0.3f, 8f), "Plataforma central");
        for (int i = 0; i < 4; i++) Solid(root, new Vector3(i % 2 == 0 ? -3.6f : 3.6f, DeckY * 0.5f, i < 2 ? -3.6f : 3.6f), new Vector3(0.5f, DeckY, 0.5f), Wood, "Columna");
        Ladder.Build(root, new Vector3(0f, 0f, -4f - Ladder.Gap), 0f, DeckY, Wood, WoodDark);   // pegada al borde: sin hueco

        // Árboles de apoyo en anillo (r 13, a 30°+60°k; el de 270° desplazado para no tapar la escalera de la entrada).
        var trees = new Vector3[6];
        for (int k = 0; k < 6; k++)
        {
            float deg = 30f + 60f * k; if (k == 4) deg = 287f;   // el de 270° se aparta: ahí baja la escalera de la entrada
            float a = deg * Mathf.Deg2Rad;
            trees[k] = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 15f;
            CubeTree.Build(root, $"ArbolResidencial_{k + 1}", center + trees[k], 7100 + k * 13, 22f, carnivorous: false, blocked: p => Built(root.InverseTransformPoint(p)));   // (CubeTree ya escala ×1,3: las cabañas están a 12 m)
        }

        int owner = 0;
        for (int k = 0; k < 6; k++)
        {
            // Cabaña ENTRE DOS ÁRBOLES (a 60°k, en la cuerda entre los troncos k-1 y k), con vigas a ambos troncos.
            float a = 60f * k * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            Vector3 c = dir * 12f;
            Transform cab = Cabin(root, c, -dir, owner++, $"Cabaña entre árboles {k + 1}");
            Vector3 t1 = trees[(k + 5) % 6], t2 = trees[k];
            Beam(root, c + Vector3.up * (DeckY - 0.35f), t1 + Vector3.up * (DeckY - 0.35f));
            Beam(root, c + Vector3.up * (DeckY - 0.35f), t2 + Vector3.up * (DeckY - 0.35f));
            Bridge(root, dir * 4f, dir * 9f);

            // Cabaña de ARRIBA, 5 m sobre la de abajo, en cuatro columnas que se apoyan en sus esquinas; se sube por una
            // escalera vertical desde la pasarela, a un lado de la puerta de abajo.
            Transform cab2 = Cabin(root, c + Vector3.up * StoryGap, -dir, owner++, $"Cabaña de arriba {k + 1}");
            for (int i = 0; i < 4; i++) Solid(cab2, new Vector3(i % 2 == 0 ? -3.2f : 3.2f, -StoryGap * 0.5f, i < 2 ? -2.7f : 2.7f), new Vector3(0.3f, StoryGap, 0.3f), Wood, "Columna");
            Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;
            Ladder.Build(root, dir * (12f - 3f - Ladder.Gap) + side * 1.4f + Vector3.up * DeckY, Quaternion.LookRotation(dir).eulerAngles.y, StoryGap, Wood, WoodDark);
        }
        SoftGround.Plant(root, center + Vector3.down * 0.1f, new Vector2(40f, 40f), 77);   // colchón de musgo bajo toda la arboleda
        WorldLabel wl = root.gameObject.AddComponent<WorldLabel>(); wl.text = "Arboleda residencial"; wl.height = DeckY + 5f;
        return root;
    }

    // ZONA CONSTRUIDA (coordenadas locales de la arboleda): plataforma central, cabañas entre árboles (r 12) y sobre
    // columnas (r 22), y los puentes que las unen, con altura libre para andar. Las ramas de los árboles la esquivan.
    static bool Built(Vector3 q)
    {
        if (Mathf.Abs(q.x) < 1.8f && q.z < -3f && q.z > -7f && q.y < DeckY + 3f) return true;   // la escalera vertical al borde (toda su altura)
        { Vector2 d3 = new Vector2(Mathf.Cos(PadAngle * Mathf.Deg2Rad), Mathf.Sin(PadAngle * Mathf.Deg2Rad)); Vector2 hq = new Vector2(q.x, q.z); float al = Vector2.Dot(hq, d3); if (al > 14f && al < 37f && Mathf.Abs(d3.x * hq.y - d3.y * hq.x) < 9f && q.y > DeckY - 3f) return true; }   // la pista de alfombras y su puente
        if (q.y < DeckY - 1.5f || q.y > DeckY + StoryGap + 5f) return false;   // dos pisos de cabañas
        Vector2 h = new Vector2(q.x, q.z);
        if (h.magnitude < 6.5f) return true;                                     // plataforma central
        for (int k = 0; k < 6; k++)
        {
            float a = 60f * k * Mathf.Deg2Rad;
            Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            if ((h - d * 12f).magnitude < 5.6f) return true;   // cabañas (7 × 6 m y margen), las dos plantas
            float along = Vector2.Dot(h, d);
            if (q.y < DeckY + 5f && along > 3f && along < 10f && Mathf.Abs(d.x * h.y - d.y * h.x) < 2.8f) return true;   // pasarelas (4 m de ancho)
        }
        return false;
    }

    // Una habitación al ESTILO DE LOS TEMPLOS GRIEGOS (7 × 6 m) pero ABIERTA: solo el suelo y el estilóbato (escalón),
    // sin paredes, columnas ni tejado (se quitaron para ganar visibilidad). Se entra por cualquier lado sin que la vista se
    // tape. Dentro: cama, mesilla con lámpara, armario y estante; el BAÑO en una esquina, tras un biombo (intimidad).
    static Transform Cabin(Transform root, Vector3 local, Vector3 door, int ownerIdx, string name)
    {
        Transform t = new GameObject(name).transform;
        t.SetParent(root, false);
        t.localPosition = local + Vector3.up * DeckY;
        t.localRotation = Quaternion.LookRotation(-door);   // su -Z mira hacia dentro de la arboleda
        Color marbleDark = new Color(0.82f, 0.8f, 0.76f);
        float W = 7f, D = 6f;
        Plank(t, Vector3.zero, new Vector3(W, 0.3f, D), "Suelo");
        Solid(t, new Vector3(0f, 0.2f, 0f), new Vector3(W - 0.4f, 0.1f, D - 0.4f), marbleDark, "Estilobato");
        // Cama, mesilla y armario (sin baño: se usan los aseos comunes; sin lámpara ni estante: no hay techo ni paredes).
        Transform bed = KitchenProps.Root(t, "Cama", new Vector3(-1.8f, 0.25f, 1.2f));
        KitchenProps.C(bed, new Vector3(0f, 0.2f, 0f), new Vector3(1.4f, 0.3f, 2.1f), WoodDark, 0f, "Armazon");
        KitchenProps.C(bed, new Vector3(0f, 0.42f, 0f), new Vector3(1.3f, 0.16f, 2f), new Color(0.95f, 0.95f, 0.93f), 0f, "Colchon");
        KitchenProps.C(bed, new Vector3(0f, 0.52f, 0.2f), new Vector3(1.34f, 0.06f, 1.3f), Blanket(ownerIdx), 0f, "Manta");
        KitchenProps.C(bed, new Vector3(0f, 0.55f, 0.8f), new Vector3(0.9f, 0.1f, 0.3f), Color.white, 0f, "Almohada");
        KitchenProps.C(t, new Vector3(-0.6f, 0.55f, 2.2f), new Vector3(0.45f, 0.55f, 0.4f), Wood, 0f, "Mesilla");
        KitchenProps.C(t, new Vector3(0.9f, 1.25f, 2.45f), new Vector3(1.4f, 1.9f, 0.55f), Wood, 0f, "Armario");
        Bedroom room = t.gameObject.AddComponent<Bedroom>();
        room.owner = ownerIdx < Owners.Length ? Owners[ownerIdx] : null;   // las demás: para quien llegue al santuario
        room.bed = bed;
        WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = ownerIdx < Shown.Length ? $"Habitación de {Shown[ownerIdx]}" : "Habitación libre (para recién llegados)"; wl.height = 2.5f;
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
        Plank(t, Vector3.zero, new Vector3(4f, 0.12f, len), "Tablones");   // puentes AMPLIOS (4 m): se cruzan sin agobio
        for (int s = -1; s <= 1; s += 2)
        {
            Solid(t, new Vector3(s * 2f, 0.9f, 0f), new Vector3(0.05f, 0.05f, len), new Color(0.75f, 0.7f, 0.6f), "Cuerda");
            Transform bar = Solid(t, new Vector3(s * 2f, 2f, 0f), new Vector3(0.05f, 4f, len), Wood, "Barrera");   // invisible, 4 m (ni saltando): no se salta
            bar.GetComponent<Renderer>().enabled = false;
            for (float z = -len * 0.5f; z <= len * 0.5f; z += 1.2f) KitchenProps.C(t, new Vector3(s * 2f, 0.45f, z), new Vector3(0.04f, 0.9f, 0.04f), WoodDark, 0f, "Poste");
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
