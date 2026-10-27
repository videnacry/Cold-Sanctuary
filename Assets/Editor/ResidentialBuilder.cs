using UnityEngine;

/// <summary>
/// ARBOLEDA RESIDENCIAL (docs/alfombras-plan.md): los dormitorios de los humanos son CASAS DE ÁRBOL, dos por árbol a
/// distinta altura, sin escaleras ni puentes. En el centro, a 12 m, la plataforma central con las alfombras a las
/// habitaciones, las del santuario y las que bajan al hielo. Cada casa tiene su alfombra (también esterilla de yoga), que
/// va a la plataforma central. Dentro: cama, mesilla, armario y el nombre de quien vive allí.
/// </summary>
public static class ResidentialBuilder
{
    public const float DeckY = 12f;   // como las demás áreas humanas: a 12 m, el bosque no queda en sombra
    static readonly Color Wood = new Color(0.62f, 0.52f, 0.42f), WoodDark = new Color(0.48f, 0.4f, 0.33f), Roof = new Color(0.55f, 0.66f, 0.75f);

    public static readonly string[] Owners =
    {
        "Player", "Magnate", "Linduarda", "Goluis", "Panterilia", "Gohageneis", "Irosene", "Lizmibet", "JefeConstruccion", "Sadhguru", "Andrea",   // solo personajes del juego
    };
    static readonly string[] Shown =
    {
        "Kushal", "la Magnate", "Linduarda", "Goluis", "Panterilia", "Gohageneis", "Irosene", "Lizmibet", "el jefe de construcción", "Sadhguru", "Andrea",
    };

    public const string Name = "Residencia";
    const float CentralSize = 18f, TreeRing = 21f, HouseOut = 5.2f;

    // Dónde va cada casa de árbol (local) y hacia dónde mira su entrada: dos por árbol, a distinta altura y a cada lado.
    static readonly System.Collections.Generic.List<(Vector3 pos, Vector3 door)> _houses = new System.Collections.Generic.List<(Vector3, Vector3)>();

    public static Transform Build(Transform parent, Vector3 center)
    {
        Transform root = new GameObject("ArboledaResidencial").transform;
        root.SetParent(parent, true);
        root.position = center;

        // CASAS DE ÁRBOL (Berón 2026-10-27, docs/alfombras-plan.md): sin escaleras ni puentes. Seis árboles en anillo y en
        // cada uno DOS casas a distinta altura, a ambos lados del tronco; cada casa tiene SU alfombra (y esterilla de yoga),
        // que solo va a la PLATAFORMA CENTRAL. Allí están las alfombras a las habitaciones y las del santuario.
        _houses.Clear();
        var trees = new Vector3[6];
        for (int k = 0; k < 6; k++)
        {
            float a = (30f + 60f * k) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            trees[k] = dir * TreeRing;
            Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;
            float low = DeckY - 3f + (k % 3) * 1.5f;   // de 9 a 12 m; la otra, 7 m más arriba
            _houses.Add((trees[k] + side * HouseOut + Vector3.up * low, -dir));
            _houses.Add((trees[k] - side * HouseOut + Vector3.up * (low + 7f), -dir));
        }
        for (int k = 0; k < 6; k++)
            CubeTree.Build(root, $"ArbolResidencial_{k + 1}", center + trees[k], 7100 + k * 13, 26f, carnivorous: false, blocked: p => Built(root.InverseTransformPoint(p)));

        // Plataforma central (18 × 18) sobre columnas, con sus tres áreas de alfombras.
        Plank(root, new Vector3(0f, DeckY, 0f), new Vector3(CentralSize, 0.3f, CentralSize), "Plataforma central");
        float hc = CentralSize * 0.5f - 0.4f;
        for (int i = 0; i < 4; i++) Solid(root, new Vector3(i % 2 == 0 ? -hc : hc, DeckY * 0.5f, i < 2 ? -hc : hc), new Vector3(0.5f, DeckY, 0.5f), Wood, "Columna");
        Vector3 top = Vector3.up * (DeckY + 0.15f);
        string central = $"{Name}#1";
        CarpetPad.BuildZone(root, root.TransformPoint(top + new Vector3(0f, 0f, 5.6f)), root.rotation, CarpetPad.Kind.Residencia, Name, 1, central, "Plataforma central", 4, 10.4f, 4.4f);
        CarpetPad.BuildZone(root, root.TransformPoint(top), root.rotation, CarpetPad.Kind.Santuario, Name, 1, central, Name, 2);
        CarpetPad.BuildZone(root, root.TransformPoint(top + new Vector3(0f, 0f, -5.6f)), root.rotation, CarpetPad.Kind.Edificio, Name, 1, central, "Plataforma central", 2);
        CarpetPad.BuildZone(root, root.TransformPoint(new Vector3(0f, 0.08f, -CentralSize * 0.5f - 4f)), root.rotation, CarpetPad.Kind.Edificio, Name, 0, $"{Name}#0", "Suelo (hielo)", 2);

        for (int i = 0; i < _houses.Count; i++)
        {
            var (pos, door) = _houses[i];
            Transform cab = Cabin(root, pos, door, i, $"Casa del árbol {i + 1}");
            Vector3 trunk = trees[i / 2] + Vector3.up * (pos.y - 0.35f);
            Beam(root, pos + Vector3.up * -0.35f, trunk);   // viga al tronco que la sostiene
            Beam(root, pos + Vector3.up * -0.35f + Vector3.Cross(Vector3.up, door).normalized * 2.5f, trunk + Vector3.down * 2.5f);   // y su tornapuntas
        }
        SoftGround.Plant(root, center + Vector3.down * 0.1f, new Vector2(64f, 64f), 77);   // colchón de musgo bajo toda la arboleda
        WorldLabel wl = root.gameObject.AddComponent<WorldLabel>(); wl.text = "Arboleda residencial (casas de árbol)"; wl.height = DeckY + 5f;
        return root;
    }

    // ZONA CONSTRUIDA (coordenadas locales de la arboleda): la plataforma central y las casas de árbol, con altura libre
    // para estar de pie. Las ramas de los árboles la esquivan.
    static bool Built(Vector3 q)
    {
        Vector2 h = new Vector2(q.x, q.z);
        if (Mathf.Abs(q.x) < CentralSize * 0.5f + 1f && Mathf.Abs(q.z) < CentralSize * 0.5f + 1f && q.y > DeckY - 1.5f && q.y < DeckY + 4f) return true;   // plataforma central
        foreach (var (pos, _) in _houses)
            if ((h - new Vector2(pos.x, pos.z)).magnitude < 5.2f && q.y > pos.y - 1.5f && q.y < pos.y + 4f) return true;   // casas (7 × 6 m y margen)
        return false;
    }

    // Una habitación al ESTILO DE LOS TEMPLOS GRIEGOS (7 × 6 m) pero ABIERTA: solo el suelo y el estilóbato (escalón),
    // sin paredes, columnas ni tejado (se quitaron para ganar visibilidad). Se entra por cualquier lado sin que la vista se
    // tape. Dentro: cama, mesilla con lámpara, armario y estante; el BAÑO en una esquina, tras un biombo (intimidad).
    static Transform Cabin(Transform root, Vector3 local, Vector3 door, int ownerIdx, string name)
    {
        Transform t = new GameObject(name).transform;
        t.SetParent(root, false);
        t.localPosition = local;
        t.localRotation = Quaternion.LookRotation(-door);   // su -Z mira hacia dentro de la arboleda
        Color marbleDark = new Color(0.82f, 0.8f, 0.76f);
        float W = 7f, D = 6f;
        Plank(t, Vector3.zero, new Vector3(W, 0.3f, D), "Suelo");
        Solid(t, new Vector3(0f, 0.2f, 0f), new Vector3(W - 0.4f, 0.1f, D - 0.4f), marbleDark, "Estilobato");
        // Cama, mesilla y armario (sin baño: se usan los aseos comunes; sin lámpara ni estante: no hay techo ni paredes).
        Transform bed = KitchenProps.Root(t, "Cama", new Vector3(-1.8f, 0.25f, 1.2f));
        KitchenProps.C(bed, new Vector3(0f, 0.2f, 0f), new Vector3(1.5f, 0.3f, 2.2f), WoodDark, 0f, "Armazon");   // colchón de 2,10 × 1,40 (docs/medidas-muebles.md)
        KitchenProps.C(bed, new Vector3(0f, 0.42f, 0f), new Vector3(1.4f, 0.16f, 2.1f), new Color(0.95f, 0.95f, 0.93f), 0f, "Colchon");
        KitchenProps.C(bed, new Vector3(0f, 0.52f, 0.2f), new Vector3(1.44f, 0.06f, 1.3f), Blanket(ownerIdx), 0f, "Manta");
        KitchenProps.C(bed, new Vector3(0f, 0.55f, 0.85f), new Vector3(0.9f, 0.1f, 0.3f), Color.white, 0f, "Almohada");
        KitchenProps.C(t, new Vector3(-0.6f, 0.55f, 2.2f), new Vector3(0.45f, 0.55f, 0.4f), Wood, 0f, "Mesilla");
        KitchenProps.C(t, new Vector3(0.9f, 1.25f, 2.45f), new Vector3(1.4f, 1.9f, 0.55f), Wood, 0f, "Armario");
        // SU ALFOMBRA, que es también su ESTERILLA DE YOGA: lleva a la plataforma central (y se hace yoga encima).
        string who = ownerIdx < Shown.Length ? $"Habitación de {Shown[ownerIdx]}" : $"Habitación libre {ownerIdx + 1}";
        CarpetPad.BuildZone(t, t.TransformPoint(new Vector3(1.75f, 0.27f, -0.9f)), t.rotation, CarpetPad.Kind.Habitacion, Name, 2, $"{Name}#hab{ownerIdx}", who, 1, 3f, 3.6f);
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
