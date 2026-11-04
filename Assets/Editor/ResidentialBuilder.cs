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
        "Player", "Magnate", "Linduarda", "Goluis", "Panterilia", "Gohageneis", "Irosene", "Lizmibet", "JefeConstruccion", "Andrea",   // solo personajes del juego
        // (2026-11-04) La familia y los amigos de Berón, en las casas que dejaron Sadhguru y los seis del mesocosmos (se fueron a sus
        // microcosmos). Amahuz y Karen, pareja, comparten casa (Partners). Con ellos, las 18 casas están ocupadas.
        "Nesvitor", "Amahuz", "Alicita", "Ducristin", "Josedrilo", "Maite", "Shalva", "Adaguchi",
    };
    static readonly string[] Shown =
    {
        "Kushal", "la Magnate", "Linduarda", "Goluis", "Panterilia", "Gohageneis", "Irosene", "Lizmibet", "el jefe de construcción", "Andrea",
        "Nesvitor", "Amahuz y Karen", "Alicita", "Ducristin", "Josedrilo", "Maite", "Shalva", "Adaguchi",
    };
    /// <summary>La pareja con la que comparte casa cada dueño (manual M18), si la tiene.</summary>
    static readonly System.Collections.Generic.Dictionary<string, string> Partners = new System.Collections.Generic.Dictionary<string, string> { { "Amahuz", "Karen" } };

    public const string Name = "Residencia";
    // (2026-11-04, Berón: «las camas de las residencias del santuario deben ser capaces de dar espacio a 4 personas») CAMA DE 4: cuatro
    // plazas de 90 cm (el ancho de una cama individual europea, 90 × 200) una junto a otra → colchón de 3,60 × 2,10 m (como las
    // «camas familiares» que juntan cuatro individuales; la Alaskan king, la mayor de serie, mide 2,74 × 2,74). Para que quepa con
    // los 2 m de paso hasta la alfombra-esterilla y el armario, la casa pasa de 7 × 6 a 9 × 7 m y se aleja 1 m más del tronco.
    public const float BedWidth = 3.6f, BedLength = 2.1f, LaneWidth = 0.9f;
    public const int BedSleeps = 4;   // plazas por piso de la cama-litera (dos pisos: 8 por casa)
    /// <summary>Cara de arriba del colchón del piso de arriba de la cama-litera, sobre la raíz de la cama (la del de abajo, a 0,5 m):
    /// 1,1 m entre colchones (unos 0,8 m libres para sentarse abajo).</summary>
    public const float BedUpperTop = 1.6f;
    public const float CabinW = 9f, CabinD = 7f;
    const float CentralSize = 18f, TreeRing = 21f, HouseOut = 1.7f + CabinW * 0.5f;   // (1,7 m del eje del tronco al borde de la casa, como antes)

    // Dónde va cada casa de árbol (local) y hacia dónde mira su entrada: dos por árbol, a distinta altura y a cada lado.
    static readonly System.Collections.Generic.List<(Vector3 pos, Vector3 door)> _houses = new System.Collections.Generic.List<(Vector3, Vector3)>();
    static readonly System.Collections.Generic.List<Vector3> _terraces = new System.Collections.Generic.List<Vector3>();

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
        // (2026-11-03) UNA TERCERA CASA por árbol, 14 m sobre la de abajo y del mismo lado (18 casas; desde el 2026-11-04, de la familia y los amigos de Berón).
        for (int k = 0; k < 6; k++)
        {
            float a = (30f + 60f * k) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)), side = Vector3.Cross(Vector3.up, dir).normalized;
            _houses.Add((trees[k] + side * HouseOut + Vector3.up * (DeckY - 3f + (k % 3) * 1.5f + 14f), -dir));
        }
        // (2026-11-03) Las TERRAZAS FRUTALES de cada casa (7 m más allá de su centro, del lado contrario al tronco): las ramas las esquivan.
        _terraces.Clear();
        for (int i = 0; i < _houses.Count; i++)
        {
            Vector3 trunk = trees[i < 12 ? i / 2 : i - 12], away = _houses[i].pos - trunk; away.y = 0f;
            _terraces.Add(_houses[i].pos + away.normalized * (CabinW * 0.5f + 3.5f));   // (el centro de la terraza de 7 m, pegada a la casa)
        }
        RingSize(Owners.Length + Partners.Count);   // (antes que los árboles: sus ramas esquivan el ring)
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
            Transform cab = Cabin(root, pos, door, i, $"Casa del árbol {i + 1}", i < 12 && i % 2 == 1 ? -1f : 1f);
            Vector3 trunk = trees[i < 12 ? i / 2 : i - 12] + Vector3.up * (pos.y - 0.35f);
            Beam(root, pos + Vector3.up * -0.35f, trunk);   // viga al tronco que la sostiene
            Beam(root, pos + Vector3.up * -0.35f + Vector3.Cross(Vector3.up, door).normalized * 2.5f, trunk + Vector3.down * 2.5f);   // y su tornapuntas
        }
        BuildPillowRing(root);   // (2026-11-04) el ring de almohadas sobre la plataforma central (docs/guerra-de-almohadas.md)
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
        if (Mathf.Abs(q.x) < _ringX * 0.5f + 1f && Mathf.Abs(q.z) < _ringZ * 0.5f + 1f && q.y > RingY - 1.5f && q.y < RingY + 4f) return true;   // el ring de almohadas
        foreach (var (pos, _) in _houses)
            if ((h - new Vector2(pos.x, pos.z)).magnitude < 6.2f && q.y > pos.y - 1.5f && q.y < pos.y + 4f) return true;   // casas (9 × 7 m: media diagonal 5,7, y margen)
        foreach (Vector3 tp in _terraces)
            if ((h - new Vector2(tp.x, tp.z)).magnitude < 5.2f && q.y > tp.y - 1.5f && q.y < tp.y + 6f) return true;   // terrazas frutales (y la copa del frutal)
        return false;
    }

    // Una habitación al ESTILO DE LOS TEMPLOS GRIEGOS (9 × 7 m desde el 2026-11-04; antes 7 × 6) pero ABIERTA: solo el suelo y el
    // estilóbato (escalón), sin paredes, columnas ni tejado (se quitaron para ganar visibilidad). Se entra por cualquier lado sin que
    // la vista se tape. Dentro: la CAMA DE 4, su mesilla y el armario; la alfombra-esterilla, a ≥ 2 m de la cama y del armario.
    static Transform Cabin(Transform root, Vector3 local, Vector3 door, int ownerIdx, string name, float terraceSide = 1f)
    {
        Transform t = new GameObject(name).transform;
        t.SetParent(root, false);
        t.localPosition = local;
        t.localRotation = Quaternion.LookRotation(-door);   // su -Z mira hacia dentro de la arboleda
        Color marbleDark = new Color(0.82f, 0.8f, 0.76f);
        float W = CabinW, D = CabinD;
        Plank(t, Vector3.zero, new Vector3(W, 0.3f, D), "Suelo");
        Solid(t, new Vector3(0f, 0.2f, 0f), new Vector3(W - 0.4f, 0.1f, D - 0.4f), marbleDark, "Estilobato");
        // Cama, mesilla y armario (sin baño: se usan los aseos comunes; sin lámpara ni estante: no hay techo ni paredes).
        // LA CAMA-LITERA DE 8 (Berón 2026-11-04: «las camas de las residencias … para 4 personas»; y luego «la cama de 4 se convierte en
        // litera, de modo que se mantiene solo una cama por residencia»; docs/pijamadas.md). UNA sola pieza por casa, de dos pisos, cada
        // uno un colchón de 4 plazas de 3,60 × 2,10 (armazón 3,70 × 2,30, de x −4,45 a −0,75 y de z 1,15 a 3,45): cabecero alto en +z,
        // una almohada y una plaza de 90 cm por persona en cada piso (Bedroom.LieSpot). Los que viven allí duermen abajo. SIN ESCALERA
        // (regla de plataformas 3): al piso de arriba se sube TREPANDO por el poste del pie (Climbable, como a los árboles); barandilla
        // por los lados abiertos de arriba.
        Transform bed = KitchenProps.Root(t, "Cama", new Vector3(-2.6f, 0.25f, 2.3f));
        float upperTop = BedUpperTop;   // cara de arriba del colchón de arriba (sobre la raíz de la cama; el de abajo, a 0,5)
        KitchenProps.C(bed, new Vector3(0f, 0.2f, 0f), new Vector3(BedWidth + 0.1f, 0.3f, BedLength + 0.2f), WoodDark, 0f, "Armazon");   // colchón de 3,60 × 2,10 (docs/medidas-muebles.md)
        KitchenProps.C(bed, new Vector3(0f, 0.42f, 0f), new Vector3(BedWidth, 0.16f, BedLength), new Color(0.95f, 0.95f, 0.93f), 0f, "Colchon");
        KitchenProps.C(bed, new Vector3(0f, 0.52f, -0.15f), new Vector3(BedWidth + 0.04f, 0.06f, BedLength * 0.62f), Blanket(ownerIdx), 0f, "Manta");
        KitchenProps.C(bed, new Vector3(0f, upperTop - 0.22f, 0f), new Vector3(BedWidth + 0.1f, 0.12f, BedLength + 0.2f), WoodDark, 0f, "Armazon_arriba");
        KitchenProps.C(bed, new Vector3(0f, upperTop - 0.08f, 0f), new Vector3(BedWidth, 0.16f, BedLength), new Color(0.95f, 0.95f, 0.93f), 0f, "Colchon_arriba");
        KitchenProps.C(bed, new Vector3(0f, upperTop + 0.02f, -0.15f), new Vector3(BedWidth + 0.04f, 0.06f, BedLength * 0.62f), Blanket(ownerIdx + 1), 0f, "Manta_arriba");
        KitchenProps.C(bed, new Vector3(0f, (upperTop + 0.5f) * 0.5f, BedLength * 0.5f + 0.06f), new Vector3(BedWidth + 0.1f, upperTop + 0.5f, 0.1f), WoodDark, 0f, "Cabecero");
        for (int k = 0; k < BedSleeps; k++)
        {
            float x = (k - (BedSleeps - 1) * 0.5f) * LaneWidth;
            KitchenProps.C(bed, new Vector3(x, 0.55f, BedLength * 0.5f - 0.3f), new Vector3(0.7f, 0.1f, 0.35f), Color.white, 0f, "Almohada");
            KitchenProps.C(bed, new Vector3(x, upperTop + 0.05f, BedLength * 0.5f - 0.3f), new Vector3(0.7f, 0.1f, 0.35f), Color.white, 0f, "Almohada");
        }
        for (int sx = -1; sx <= 1; sx += 2)
            for (int sz = -1; sz <= 1; sz += 2)
            {
                Transform post = KitchenProps.C(bed, new Vector3(sx * (BedWidth * 0.5f + 0.05f), (upperTop + 0.5f) * 0.5f, sz * (BedLength * 0.5f + 0.1f)), new Vector3(0.1f, upperTop + 0.5f, 0.1f), WoodDark, 0f, "Poste");
                if (sx > 0 && sz < 0)   // el poste para trepar: el del pie, del lado del pasillo
                {
                    if (post.GetComponent<Collider>() == null) post.gameObject.AddComponent<BoxCollider>();
                    Climbable cl = post.gameObject.AddComponent<Climbable>(); cl.topY = bed.position.y + upperTop;
                }
            }
        KitchenProps.C(bed, new Vector3(-(BedWidth * 0.5f + 0.05f), upperTop + 0.25f, 0f), new Vector3(0.05f, 0.05f, BedLength), WoodDark, 0f, "Barandilla");   // lados abiertos de arriba
        KitchenProps.C(bed, new Vector3(0.225f, upperTop + 0.25f, -(BedLength * 0.5f + 0.1f)), new Vector3(2.25f, 0.05f, 0.05f), WoodDark, 0f, "Barandilla");   // al pie: libres el poste (+x) y la boca del tobogán (−x)
        // EL TOBOGÁN (Berón 2026-11-04: «las literas tienen resbaladera»): por él se BAJA del piso de arriba (se sube trepando por el poste).
        // Sale del pie de la cama por el lado −x (x −4,3…−3,7 de la casa) y baja en recta 2,6 m hacia −z hasta el suelo (z ≈ −1,45), con
        // ~32° de pendiente, sus bordes y una pista de frenado plana de 0,6 m. Es parte de la cama (va con ella) y no lleva collider: no
        // estorba al paso. Queda a 5 m de la alfombra-esterilla.
        float slideX = -BedWidth * 0.5f + 0.4f, slideZ0 = -(BedLength * 0.5f + 0.1f), run = 2.6f, drop = upperTop + 0.05f - 0.1f;
        Transform slide = new GameObject("Tobogan").transform; slide.SetParent(bed, false);
        float len = Mathf.Sqrt(run * run + drop * drop), ang = Mathf.Atan2(drop, run) * Mathf.Rad2Deg;
        Transform board = KitchenProps.C(slide, new Vector3(slideX, 0.1f + drop * 0.5f, slideZ0 - run * 0.5f), new Vector3(0.6f, 0.04f, len), new Color(0.95f, 0.55f, 0.2f), 0f, "Rampa_tobogan");
        board.localRotation = Quaternion.Euler(-ang, 0f, 0f);
        for (int s = -1; s <= 1; s += 2)
        {
            Transform edge = KitchenProps.C(slide, new Vector3(slideX + s * 0.31f, 0.22f + drop * 0.5f, slideZ0 - run * 0.5f), new Vector3(0.04f, 0.2f, len), new Color(0.98f, 0.8f, 0.3f), 0f, "Borde_tobogan");
            edge.localRotation = Quaternion.Euler(-ang, 0f, 0f);
        }
        KitchenProps.C(slide, new Vector3(slideX, 0.08f, slideZ0 - run - 0.3f), new Vector3(0.6f, 0.04f, 0.6f), new Color(0.95f, 0.55f, 0.2f), 0f, "Frenada_tobogan");
        foreach (Collider c in slide.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
        Transform slideTop = new GameObject("Tobogan_arriba").transform; slideTop.SetParent(slide, false); slideTop.localPosition = new Vector3(slideX, upperTop + 0.05f, slideZ0);
        Transform slideBottom = new GameObject("Tobogan_abajo").transform; slideBottom.SetParent(slide, false); slideBottom.localPosition = new Vector3(slideX, 0.1f, slideZ0 - run - 0.45f);
        KitchenProps.C(t, new Vector3(-0.4f, 0.55f, 3.2f), new Vector3(0.45f, 0.55f, 0.4f), Wood, 0f, "Mesilla");   // pegada a la cama (van juntas)
        KitchenProps.C(t, new Vector3(2.6f, 1.25f, 3.2f), new Vector3(1.4f, 1.9f, 0.55f), Wood, 0f, "Armario");     // a 2,1 m de la mesilla
        // LAS PLAZAS PARA DORMIR (Bedroom.spots): las 4 de abajo y las 4 de arriba; cada una 0,5 m por debajo de la cara de arriba de su
        // colchón (como la raíz de la cama: SleepSpell tumba a cada uno 0,5 m más arriba) y girada como la cama.
        var spots = new System.Collections.Generic.List<Transform>();
        for (int tier = 0; tier < 2; tier++)
            for (int k = 0; k < BedSleeps; k++)
            {
                Transform sp = new GameObject(tier == 0 ? $"Plaza_{k + 1}" : $"Plaza_arriba_{k + 1}").transform; sp.SetParent(bed, false);
                sp.localPosition = new Vector3((k - (BedSleeps - 1) * 0.5f) * LaneWidth, tier == 0 ? 0f : upperTop - 0.5f, 0f); spots.Add(sp);
            }
        // Dónde se juntan los de una pijamada: delante de la cama, en el sitio que dejó la litera aparte (x −4,5…−0,85, z −3,5…0,9),
        // a ≥ 2 m de la alfombra-esterilla (x ≥ 1,15).
        var gather = new System.Collections.Generic.List<Transform>();
        Vector3[] gp = { new Vector3(-3.0f, 0.25f, 0.4f), new Vector3(-2.2f, 0.25f, 0.4f), new Vector3(-1.4f, 0.25f, 0.4f), new Vector3(-0.6f, 0.25f, 0.4f),   // (a ≥ 0,7 m del tobogán, x ≤ −3,7)
                         new Vector3(-3.0f, 0.25f, -1.2f), new Vector3(-2.2f, 0.25f, -1.2f), new Vector3(-1.4f, 0.25f, -1.2f), new Vector3(-0.6f, 0.25f, -1.2f) };
        for (int k = 0; k < gp.Length; k++) { Transform gt = new GameObject($"Pijamada_{k + 1}").transform; gt.SetParent(t, false); gt.localPosition = gp[k]; gather.Add(gt); }
        // SU ALFOMBRA, que es también su ESTERILLA DE YOGA: lleva a la plataforma central (y se hace yoga encima). Zona de 3 × 3,6 m
        // de x 1,15 a 4,15 y de z −3,45 a 0,15: a 2,15 m de la cama y a 2,8 m del armario.
        string who = ownerIdx < Shown.Length ? $"Habitación de {Shown[ownerIdx]}" : $"Habitación libre {ownerIdx + 1}";
        CarpetPad.BuildZone(t, t.TransformPoint(new Vector3(2.65f, 0.27f, -1.65f)), t.rotation, CarpetPad.Kind.Habitacion, Name, 2, $"{Name}#hab{ownerIdx}", who, 1, 3f, 3.6f);
        // (2026-11-03) SU TERRAZA FRUTAL (Berón: «los frutales en residencias que te pedí era para las residencias del santuario»; docs/
        // jardines-frutales.md §Residencia): una plataforma de 7 × 7 m (7 × 6 hasta el 2026-11-04) pegada a la habitación por el lado contrario al tronco, con un
        // frutal en maceta en su centro (a ≥ 2 m de la cama, el armario y la alfombra). Es una maceta del jardinero (FruitPlant,
        // edificio «Residencia»): entra en su ronda.
        Plank(t, new Vector3(terraceSide * (W * 0.5f + 3.5f), 0f, 0f), new Vector3(7f, 0.3f, D), "Terraza_frutal");
        FruitCare.Ficha fruit = FruitCare.All[FruitCare.All.Length - 1 - ownerIdx % Mathf.Max(1, FruitCare.All.Length / 2)];   // las de porte pequeño, una distinta en cada casa
        Transform pot = SampleSceneBuilder.PottedFruit(t, fruit, 900 + ownerIdx, Name, t.position.y + 0.15f);
        // 5 m fuera de la habitación (1,5 m más allá del centro de la terraza): con la copa del frutal, a 2 m de la alfombra-esterilla
        // (en el centro quedaba a 0,83 m; FruitOrchardTest, 2026-11-03).
        pot.position = t.TransformPoint(new Vector3(terraceSide * (W * 0.5f + 5f), 0.15f, 0f));
        Bedroom room = t.gameObject.AddComponent<Bedroom>();
        room.owner = ownerIdx < Owners.Length ? Owners[ownerIdx] : null;   // las demás: para quien llegue al santuario
        room.partner = room.owner != null && Partners.TryGetValue(room.owner, out string mate) ? mate : null;
        room.bed = bed; room.sleeps = BedSleeps; room.laneWidth = LaneWidth; room.bedLength = BedLength;   // cama de 4 (2026-11-04)
        room.spots = spots.ToArray(); room.gather = gather.ToArray();   // pijamadas: 4 abajo y 4 arriba en la cama-litera (2026-11-04)
        room.slideTop = slideTop; room.slideBottom = slideBottom;   // el tobogán para bajar del piso de arriba
        WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = ownerIdx < Shown.Length ? $"Habitación de {Shown[ownerIdx]}" : "Habitación libre (para recién llegados)"; wl.height = 2.5f;
        return t;
    }

    // ── EL RING DE ALMOHADAS (2026-11-04; docs/guerra-de-almohadas.md) ──────────────────────────────────────────────────────
    // Berón: «se deben crear guerras de almohadas en un ring que se debe crear en la residencia, el ring debe ser creado con la regla
    // para que tenga las dimensiones necesarias para que se enfrenten todos los residentes del santuario y debe estar lleno de
    // almohadas y la base es una cama colosal». Una plataforma 8 m por encima de la central (sobre sus propias columnas), con la
    // CAMA COLOSAL: colchón de n × n celdas de 2,5 m (PillowRing: n = ⌈√residentes⌉), borde blando de 0,6 m con una entrada de 3 m
    // al pie, cabecero con almohadones gigantes y dos almohadas sueltas por luchador. 2 m de paso alrededor y hasta su pista de
    // alfombras (red «Residencia» de edificio: vuela a la plataforma central y al suelo).

    /// <summary>Altura del ring: 8 m sobre la plataforma central (queda dentro del anillo de casas: ver <see cref="RingSize"/>).</summary>
    public const float RingY = DeckY + 8f;
    const float HeadZone = 1.6f, ZoneW = 7.6f, ZoneD = 4.4f, EdgeMargin = 0.8f, FootGap = 3f;
    static int _ringCells = 1, _ringFor;
    static float _ringX, _ringZ, _bedX, _bedZ;

    /// <summary>Las medidas del ring para <paramref name="fighters"/> residentes (la regla: PillowRing.CellsFor). Avisa si ya no cabe
    /// dentro del anillo de casas (su borde interior está a 17,5 m del centro): entonces hay que subirlo o llevarlo a otro sitio.</summary>
    static void RingSize(int fighters)
    {
        _ringFor = fighters; _ringCells = PillowRing.CellsFor(fighters);
        float S = _ringCells * PillowRing.CellSize;
        _bedX = S + 2f * PillowRing.Bumper; _bedZ = S + PillowRing.Bumper + HeadZone;
        _ringX = Mathf.Max(_bedX, ZoneW) + 2f * PillowRing.Walkway;
        _ringZ = PillowRing.Walkway + _bedZ + PillowRing.Walkway + ZoneD + EdgeMargin;
        float corner = new Vector2(_ringX * 0.5f, _ringZ * 0.5f).magnitude, inner = TreeRing - CabinD * 0.5f - 1f;
        if (corner > inner) Debug.LogError($"[Residencia] el ring de almohadas para {fighters} residentes ({_ringX:0.0} × {_ringZ:0.0} m) ya no cabe dentro del anillo de casas (esquina a {corner:0.0} m, máximo {inner:0.0}).");
        else Debug.Log($"[Residencia] ring de almohadas para {fighters} residentes: colchón de {_ringCells} × {_ringCells} celdas ({S:0.0} m de lado, caben {_ringCells * _ringCells}), plataforma de {_ringX:0.0} × {_ringZ:0.0} m a {RingY:0} m.");
    }

    static void BuildPillowRing(Transform root)
    {
        Transform ring = new GameObject("RingAlmohadas").transform;
        ring.SetParent(root, false);
        ring.localPosition = new Vector3(0f, RingY, 0f);
        float S = _ringCells * PillowRing.CellSize, B = PillowRing.Bumper;
        Color mattressC = new Color(0.97f, 0.95f, 0.91f), soft = new Color(0.93f, 0.76f, 0.83f), quilt = new Color(0.88f, 0.84f, 0.8f);
        Plank(ring, Vector3.zero, new Vector3(_ringX, 0.3f, _ringZ), "Plataforma_ring");   // cara de arriba a +0,15
        for (int i = 0; i < 4; i++)
            Solid(ring, new Vector3((i % 2 == 0 ? -1f : 1f) * (_ringX * 0.5f - 0.4f), -RingY * 0.5f, (i < 2 ? -1f : 1f) * (_ringZ * 0.5f - 0.4f)), new Vector3(0.6f, RingY, 0.6f), Wood, "Columna_ring");
        // La CAMA COLOSAL: el cabecero en +z, el pie hacia la pista de alfombras (−z).
        float headTop = _ringZ * 0.5f - PillowRing.Walkway, bedCz = headTop - _bedZ * 0.5f, matCz = headTop - HeadZone - S * 0.5f;
        Transform mat = Solid(ring, new Vector3(0f, 0.25f, matCz), new Vector3(S, 0.2f, S), mattressC, "Colchon_colosal");   // cara de arriba a +0,35: 0,2 m sobre la plataforma
        Solid(ring, new Vector3(0f, 0.25f, headTop - HeadZone * 0.5f), new Vector3(S, 0.2f, HeadZone), mattressC, "Colchon_cabecero");
        // (2026-11-04, grupo 89) El colchón es BLANDO: sin collider se anda sobre la plataforma y uno se hunde 0,2 m en él; con collider,
        // su escalón de 0,2 m frenaba a algunos al entrar. (PillowRing.Spot sigue dando la cara de arriba; CarpetPad.For ve la plataforma.)
        Object.DestroyImmediate(mat.GetComponent<Collider>());
        foreach (Transform hm in ring) if (hm.name == "Colchon_cabecero") Object.DestroyImmediate(hm.GetComponent<Collider>());
        for (int k = 1; k < _ringCells; k++)   // el acolchado marca las celdas
        {
            float o = (k - _ringCells * 0.5f) * PillowRing.CellSize;
            Deco(ring, new Vector3(o, 0.353f, matCz), new Vector3(0.05f, 0.006f, S), quilt, "Acolchado");
            Deco(ring, new Vector3(0f, 0.353f, matCz + o), new Vector3(S, 0.006f, 0.05f), quilt, "Acolchado");
        }
        // Borde blando: a los lados entero; al pie, con la ENTRADA de 3 m en el centro (sin escalones: el colchón sube 0,2 m). Es blando:
        // sin collider, se pasa por encima (quien va a una celda del lado no tiene que rodear hasta la entrada).
        for (int s = -1; s <= 1; s += 2)
            Deco(ring, new Vector3(s * (S * 0.5f + B * 0.5f), 0.375f, bedCz), new Vector3(B, 0.45f, _bedZ), soft, "Borde_blando");
        float footLen = (_bedX - FootGap) * 0.5f, footZ = matCz - S * 0.5f - B * 0.5f;
        for (int s = -1; s <= 1; s += 2)
            Deco(ring, new Vector3(s * (FootGap * 0.5f + footLen * 0.5f), 0.375f, footZ), new Vector3(footLen, 0.45f, B), soft, "Borde_blando_pie");
        Solid(ring, new Vector3(0f, 1.0f, headTop - 0.2f), new Vector3(_bedX, 1.7f, 0.4f), WoodDark, "Cabecero_colosal");
        for (int k = 0; k < _ringCells; k++)   // almohadones gigantes contra el cabecero
            Deco(ring, new Vector3((k - (_ringCells - 1) * 0.5f) * PillowRing.CellSize, 0.62f, headTop - 0.4f - (HeadZone - 0.5f) * 0.5f), new Vector3(PillowRing.CellSize * 0.85f, 0.55f, HeadZone - 0.5f), Color.white, "Almohadon_gigante");
        // LLENO DE ALMOHADAS: dos sueltas por celda (sin collider: se cogen, no se tropieza con ellas).
        Transform loose = new GameObject("Almohadas").transform; loose.SetParent(ring, false);
        Color[] pastel = { new Color(0.98f, 0.86f, 0.9f), new Color(0.86f, 0.92f, 0.99f), new Color(0.99f, 0.95f, 0.8f), new Color(0.88f, 0.97f, 0.88f), new Color(0.93f, 0.88f, 0.99f) };
        var rnd = new System.Random(4242);
        int cells = _ringCells * _ringCells;
        for (int c = 0; c < cells; c++)
            for (int p = 0; p < PillowRing.PillowsPerFighter; p++)
            {
                float cx = (c % _ringCells - (_ringCells - 1) * 0.5f) * PillowRing.CellSize, cz = (c / _ringCells - (_ringCells - 1) * 0.5f) * PillowRing.CellSize;
                Vector3 at = new Vector3(cx + (p == 0 ? -0.65f : 0.65f), 0.43f, matCz + cz + (p == 0 ? 0.55f : -0.55f));
                Transform pil = Deco(loose, at, new Vector3(0.6f, 0.15f, 0.4f), pastel[(c + p) % pastel.Length], $"Almohada_{c * PillowRing.PillowsPerFighter + p + 1}");
                pil.localRotation = Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f);
            }
        PillowRing pr = ring.gameObject.AddComponent<PillowRing>();
        pr.cells = _ringCells; pr.builtFor = _ringFor; pr.mattress = mat; pr.pillows = loose;
        // Su PISTA DE ALFOMBRAS (red de edificio de la Residencia: vuela a la plataforma central y al suelo), a 2 m del pie de la cama.
        float zc = -_ringZ * 0.5f + EdgeMargin + ZoneD * 0.5f;
        CarpetPad.BuildZone(ring, ring.TransformPoint(new Vector3(0f, 0.15f, zc)), ring.rotation, CarpetPad.Kind.Edificio, Name, 2, $"{Name}#ring", "Ring de almohadas", 2, ZoneW, ZoneD);
        WorldLabel wl = ring.gameObject.AddComponent<WorldLabel>();
        wl.text = $"Ring de almohadas (cama colosal): {_ringCells} × {_ringCells} celdas, caben {cells}"; wl.height = 3f;
    }

    // Una pieza sin collider (decoración y almohadas sueltas).
    static Transform Deco(Transform parent, Vector3 local, Vector3 size, Color c, string name)
    {
        Transform t = Solid(parent, local, size, c, name);
        Object.DestroyImmediate(t.GetComponent<Collider>());
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
