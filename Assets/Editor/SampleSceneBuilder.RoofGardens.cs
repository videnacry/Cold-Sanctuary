using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JARDINES FRUTALES EN LA CIMA DE LOS EDIFICIOS-ÁRBOL (Berón 2026-11-03, docs/jardines-frutales.md §Jardines en la cima):
/// «agrega los jardines frutales a las plataformas de alfombras voladoras en la cima de los edificio-árbol… y si sería más
/// conveniente un área de tierra en lugar de macetas». En el techo de cada árbol (la plataforma de alfombras, <c>Copa_arbol</c>),
/// lo que queda libre de las alfombras, de la columna y la puerta del techo, de su umbral y de los paneles se llena con
/// BANCALES DE TIERRA (<see cref="GardenBed"/>: cubierta ajardinada intensiva con lámina antirraíces, drenaje, filtro y 0,8 m de
/// sustrato), uno por GREMIO (frutales que comparten pH, con sus compañeras, flores, insectos útiles y animales), del más largo
/// al más corto, hasta que no cabe ninguno:
/// - cada bancal es UN MUEBLE (raíz con BoxCollider de su caja, sin renderer) y guarda los 2 m con todo (medidos con las copas);
/// - a ≥ 2 m de las áreas de alfombras y del umbral de la puerta del techo;
/// - lo mide en el MARCO del techo (el del Árbol de los Campos gira);
/// - cada techo empieza por un gremio distinto, así entre todos los techos salen todos los gremios.
/// Sus frutales son <see cref="FruitPlant"/> con <see cref="FruitPlant.bed"/>: entran solas en la ronda del jardinero de su
/// edificio (<see cref="FruitGardener.Rounds"/>: el techo es lo más alto, se cuida al llegar en alfombra).
/// </summary>
public static partial class SampleSceneBuilder
{
    const float BedWidth = 1.4f, BedDepth = 0.8f, BedClear = 2f;
    static readonly float[] BedLengths = { 6f, 5f, 4f, 3.2f };

    static void PlaceTreeTopGardens()
    {
        Physics.SyncTransforms();
        Renderer[] scene = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        int beds = 0, plants = 0, roofs = 0, g0 = 0;
        float tonnes = 0f;
        foreach (Helix hx in _helixes)
        {
            Transform top = hx.ac != null ? hx.ac.top : null;
            if (top == null) continue;
            var (b, p, t) = GardenTreeTop(hx, top, scene, g0);
            beds += b; plants += p; tonnes += t; g0 += Mathf.Max(1, b); roofs++;
        }
        Debug.Log($"[Frutales] Jardines en la cima: {beds} bancales de tierra con {plants} frutales en los techos de {roofs} edificios-árbol ({tonnes:0} t con el sustrato saturado).");
    }

    // Caja de un renderer en el marco de 'frame' (como FurnitureSpacing con un piso que gira).
    static Bounds LocalBox(Transform frame, Renderer r) => WrapBox(frame.worldToLocalMatrix * r.transform.localToWorldMatrix, r.localBounds);

    static Bounds WrapBox(Matrix4x4 m, Bounds b)
    {
        Bounds o = new Bounds(m.MultiplyPoint3x4(b.center), Vector3.zero);
        for (int i = 0; i < 8; i++)
            o.Encapsulate(m.MultiplyPoint3x4(b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f))));
        return o;
    }

    static bool LocalBoxOf(Transform frame, Transform t, out Bounds b)
    {
        b = default; bool any = false;
        foreach (Renderer r in t.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled) continue;
            Bounds rb = LocalBox(frame, r);
            if (!any) { b = rb; any = true; } else b.Encapsulate(rb);
        }
        return any;
    }

    /// <summary>Los bancales de un techo. Devuelve (bancales, frutales, toneladas).</summary>
    static (int beds, int plants, float tonnes) GardenTreeTop(Helix hx, Transform top, Renderer[] scene, int guildStart)
    {
        float X = TreeSpec.TopWidth * 0.5f - OrchardEdge;
        // LO QUE YA HAY en el techo (en su marco): la columna y la puerta del techo, el cartel, los paneles, las máscaras…
        var blockers = new List<(Bounds b, float need)>();
        foreach (Renderer r in scene)
        {
            if (r == null || !r.enabled || r.GetComponentInParent<CarpetPad>() != null) continue;
            if (r.name == "Losa_copa" || r.name.StartsWith("Agua")) continue;
            Bounds b = LocalBox(top, r);
            if (b.min.y < -1f) continue;   // lo que sube desde abajo (el tronco, su exoesqueleto, su agua): no está EN el techo
            if (b.max.y < 0.05f || b.min.y > 6f || b.min.x > X + 4f || b.max.x < -X - 4f || b.min.z > X + 4f || b.max.z < -X - 4f) continue;
            if (b.size.y < 0.3f && Mathf.Max(b.size.x, b.size.z) > 3f && b.max.y < 0.3f) continue;   // suelos y losas
            blockers.Add((b, FurnitureSpacing.Standard));
        }
        foreach (CarpetPad p in top.GetComponentsInChildren<CarpetPad>())
        {
            BoxCollider bc = p.GetComponent<BoxCollider>(); if (bc == null) continue;
            blockers.Add((WrapBox(top.worldToLocalMatrix * p.transform.localToWorldMatrix, new Bounds(bc.center, bc.size)), BedClear));
        }
        // EL UMBRAL DE LA PUERTA DEL TECHO: de la puerta a 2 m más allá de su punto de fuera, a lo ancho de la puerta (+1 m).
        TreeLevel lv = hx.ac.Top;
        if (lv != null)
        {
            Vector3 d = top.InverseTransformPoint(hx.root.TransformPoint(lv.door)), o = top.InverseTransformPoint(hx.root.TransformPoint(lv.outside));
            Vector3 dir = o - d; dir.y = 0f; dir = dir.sqrMagnitude > 1e-4f ? dir.normalized : Vector3.forward;
            Bounds zone = new Bounds(d, Vector3.zero);
            Vector3 side = Vector3.Cross(Vector3.up, dir) * (TreeSpec.DoorWidth * 0.5f + 1f);
            foreach (Vector3 c in new[] { d + side, d - side, o + dir * 2f + side, o + dir * 2f - side }) zone.Encapsulate(c);
            zone.Encapsulate(zone.center + Vector3.up * 3f);
            blockers.Add((zone, 0.5f));
        }

        var guilds = GardenBed.Guilds;
        int n = 0, plants = 0; float tonnes = 0f;
        var placed = new List<Bounds>();
        Transform group = new GameObject("Jardin_cima_" + hx.building).transform;
        group.SetParent(top, false);
        var allPlants = new List<FruitPlant>();
        for (int tries = 0; tries < 8; tries++)
        {
            GardenBed.Guild g = guilds[(guildStart + n) % guilds.Length];
            bool ok = false;
            foreach (float L in BedLengths)
            {
                Transform bed = BuildBed(group, g, L, hx.building, n);
                foreach (float yaw in new[] { 0f, 90f })
                {
                    bed.localPosition = Vector3.zero; bed.localRotation = Quaternion.Euler(0f, yaw, 0f);
                    if (!LocalBoxOf(top, bed, out Bounds b0)) continue;
                    float best = float.MaxValue; Vector3 bestP = Vector3.zero; Bounds bestB = b0;
                    for (float z = -X; z <= X; z += OrchardStep)
                        for (float x = -X; x <= X; x += OrchardStep)
                        {
                            Bounds b = b0; b.center += new Vector3(x, 0f, z);
                            if (b.min.x < -X || b.max.x > X || b.min.z < -X || b.max.z > X) continue;
                            float slack = Mathf.Min(X - b.max.x, b.min.x + X, X - b.max.z, b.min.z + X);
                            foreach (Bounds q in placed) { slack = Mathf.Min(slack, OrchardGap(q, b) - BedClear); if (slack < 0f) break; }
                            if (slack >= 0f) foreach (var (q, need) in blockers) { slack = Mathf.Min(slack, OrchardGap(q, b) - need); if (slack < 0f) break; }
                            if (slack < 0f || slack >= best - 0.001f) continue;
                            best = slack; bestP = new Vector3(x, 0f, z); bestB = b;
                        }
                    if (best == float.MaxValue) continue;
                    bed.localPosition = bestP; placed.Add(bestB); ok = true;
                    break;
                }
                if (ok)
                {
                    GardenBed gb = bed.GetComponent<GardenBed>();
                    foreach (FruitPlant p in gb.Plants) allPlants.Add(p);
                    plants += gb.Plants.Count; tonnes += gb.SaturatedTonnes; n++;
                    Debug.Log($"[Frutales] Cima de «{hx.building}»: bancal «{g.nombre}» de {L:0.#} × {BedWidth:0.#} m ({gb.Plants.Count} frutales, {g.ph}).");
                    break;
                }
                Object.DestroyImmediate(bed.gameObject);
            }
            if (!ok) break;
        }
        if (n == 0) { Object.DestroyImmediate(group.gameObject); Debug.LogWarning($"[Frutales] Cima de «{hx.building}»: no cabe ningún bancal (alfombras, puerta, paneles)."); return (0, 0, 0f); }
        // El orden de la ronda: los bancales por cercanía desde la puerta del techo (por donde sale del agua quien llega nadando y
        // por donde pasa quien llega en alfombra).
        Vector3 from = lv != null ? top.InverseTransformPoint(hx.root.TransformPoint(lv.outside)) : Vector3.zero;
        allPlants.Sort((a, b) => Vector3.Distance(top.InverseTransformPoint(a.transform.position), from).CompareTo(Vector3.Distance(top.InverseTransformPoint(b.transform.position), from)));
        for (int i = 0; i < allPlants.Count; i++)
        {
            FruitPlant fp = allPlants[i];
            fp.order = -1000 + i;   // (antes que las puntas: el techo se cuida al llegar)
            int poda = FruitCare.PodaDias(fp.kind);
            if (poda > 0) fp.lastPrunedDay = -1 - i % poda;
            fp.lastPollinatedDay = -1 - i % 2;
        }
        Debug.Log($"[Frutales] Cima de «{hx.building}»: {n} bancales de tierra, {plants} frutales ({tonnes:0.0} t saturados: {tonnes * 9.81f / 1000f:0.00} MN sobre el tronco).");
        return (n, plants, tonnes);
    }

    /// <summary>UN BANCAL de su gremio, a lo largo de su X local (centro en la raíz): marco de 0,8 m con el corte de sus capas en
    /// una punta, la tierra y el acolchado, sus frutales (<see cref="FruitPlant"/>, con su gotero), las compañeras y las flores en
    /// los bordes, el hotel de insectos, la caja de abejorros, la caja nido y el vermicompostador, y su cabezal de goteo.</summary>
    static Transform BuildBed(Transform parent, GardenBed.Guild g, float L, string building, int n)
    {
        float W = BedWidth, H = BedDepth;
        Transform t = KitchenProps.Root(parent, $"Bancal_{g.id}_{n + 1}", Vector3.zero);
        GardenBed gb = t.gameObject.AddComponent<GardenBed>();
        gb.building = building; gb.guild = g.id; gb.size = new Vector2(L, W); gb.depth = H;
        gb.beneficials = g.insectos; gb.animals = g.animales;
        var comp = new List<string>();
        foreach (GardenCrop.Kind k in g.companeras) comp.Add(k.ToString());
        comp.AddRange(g.flores);
        gb.companions = comp.ToArray();
        Color wood = new Color(0.55f, 0.42f, 0.3f), soil = new Color(0.27f, 0.2f, 0.14f), mulch = g.id == "acido" ? new Color(0.45f, 0.3f, 0.22f) : g.id == "mediterraneo" ? new Color(0.7f, 0.68f, 0.62f) : new Color(0.42f, 0.33f, 0.2f);
        // EL MARCO (tablones de 6 cm) y la tierra.
        KitchenProps.C(t, new Vector3(0f, H * 0.5f, W * 0.5f - 0.03f), new Vector3(L, H, 0.06f), wood, 0f, "Tablon");
        KitchenProps.C(t, new Vector3(0f, H * 0.5f, -W * 0.5f + 0.03f), new Vector3(L, H, 0.06f), wood, 0f, "Tablon");
        KitchenProps.C(t, new Vector3(L * 0.5f - 0.03f, H * 0.5f, 0f), new Vector3(0.06f, H, W - 0.12f), wood, 0f, "Tablon");
        KitchenProps.C(t, new Vector3(-L * 0.5f + 0.03f, H * 0.5f, 0f), new Vector3(0.06f, H, W - 0.12f), wood, 0f, "Tablon");
        KitchenProps.C(t, new Vector3(0f, H - 0.04f, 0f), new Vector3(L - 0.12f, 0.06f, W - 0.12f), soil, 0f, "Tierra");
        KitchenProps.C(t, new Vector3(0f, H - 0.005f, 0f), new Vector3(L - 0.14f, 0.02f, W - 0.14f), mulch, 0f, "Acolchado");
        // EL CORTE DE LAS CAPAS (en la punta −X, por fuera): lámina antirraíces, drenaje, filtro y sustrato (cubierta intensiva, FLL).
        Transform cut = KitchenProps.Root(t, "Corte_capas", new Vector3(-L * 0.5f - 0.005f, 0f, 0f));
        KitchenProps.C(cut, new Vector3(0f, 0.01f, 0f), new Vector3(0.012f, 0.02f, W - 0.14f), KitchenProps.Black, 0f, "Lamina_antirraices");
        KitchenProps.C(cut, new Vector3(0f, 0.08f, 0f), new Vector3(0.012f, 0.12f, W - 0.14f), new Color(0.6f, 0.6f, 0.62f), 0f, "Capa_drenaje");
        KitchenProps.C(cut, new Vector3(0f, 0.15f, 0f), new Vector3(0.012f, 0.02f, W - 0.14f), KitchenProps.White, 0f, "Filtro_geotextil");
        KitchenProps.C(cut, new Vector3(0f, 0.47f, 0f), new Vector3(0.012f, 0.62f, W - 0.14f), soil, 0f, "Sustrato_intensivo");
        // LOS FRUTALES: uno cada ~2 m (los que quepan del gremio), en el eje del bancal.
        int fruits = Mathf.Clamp(Mathf.FloorToInt(L / 2f), 1, g.frutales.Length);
        float slot = L / fruits;
        var plants = new List<FruitPlant>();
        for (int i = 0; i < fruits; i++)
        {
            FruitCare.Ficha f = FruitCare.For(g.frutales[i]);
            if (f == null) continue;
            float x = -L * 0.5f + slot * (i + 0.5f);
            Transform root = KitchenProps.Root(t, $"Frutal_{f.kind}_bancal", new Vector3(x, 0f, 0f));
            float s = f.escala * (f.forma == FruitCare.Forma.Arbol || f.forma == FruitCare.Forma.Palmera ? 0.75f : 0.85f);
            GardenCrop.Build(root, f.kind, root.TransformPoint(new Vector3(0f, H, 0f)), 5200 + n * 13 + i * 7 + (int)f.kind, s);
            FruitPlant fp = root.gameObject.AddComponent<FruitPlant>();
            fp.kind = f.kind; fp.building = building; fp.bed = gb; fp.bedSlot = new Vector3(Mathf.Min(slot, 2f), H, W); fp.drip = true;
            var rnd = new System.Random(n * 31 + i * 7 + (int)f.kind);
            fp.water = 0.5f + 0.3f * (float)rnd.NextDouble(); fp.nutrients = 0.45f + 0.3f * (float)rnd.NextDouble();
            fp.pests = 0.05f + 0.2f * (float)rnd.NextDouble(); fp.health = 0.6f + 0.25f * (float)rnd.NextDouble();
            WorldLabel wl = root.gameObject.AddComponent<WorldLabel>(); wl.text = f.nombre; wl.height = 2.2f;
            // Un sobre de liberación de su insecto útil, colgado de la planta (como se sueltan Cryptolaemus o Phytoseiulus).
            KitchenProps.C(root, new Vector3(0.18f, H + 0.6f, 0.1f), new Vector3(0.06f, 0.09f, 0.01f), new Color(0.95f, 0.9f, 0.6f), 0f, "Sobre_insectos_utiles");
            plants.Add(fp);
        }
        // LAS COMPAÑERAS (las que ya existen en el juego): entre los frutales y en las puntas, a un lado del eje.
        for (int i = 0; i < g.companeras.Length; i++)
        {
            float x = -L * 0.5f + (i + 0.5f) * L / g.companeras.Length + slot * 0.25f;
            x = Mathf.Clamp(x, -L * 0.5f + 0.35f, L * 0.5f - 0.35f);
            float z = (i % 2 == 0 ? 1f : -1f) * (W * 0.5f - 0.32f);
            GardenCrop.Build(t, g.companeras[i], t.TransformPoint(new Vector3(x, H, z)), 6100 + n * 11 + i, 0.45f);
        }
        // LAS FLORES DE NÉCTAR: franjas en los dos bordes largos (alyssum, borraja, caléndula, facelia…), colores por especie.
        Color[] petals = { new Color(0.98f, 0.98f, 0.95f), new Color(0.45f, 0.55f, 0.95f), new Color(0.98f, 0.62f, 0.15f), new Color(0.65f, 0.5f, 0.9f), new Color(0.95f, 0.85f, 0.3f) };
        int fl = Mathf.Max(1, g.flores.Length);
        for (int sideZ = -1; sideZ <= 1; sideZ += 2)
            for (float x = -L * 0.5f + 0.2f; x < L * 0.5f - 0.15f; x += 0.28f)
            {
                int k = Mathf.Abs(Mathf.RoundToInt(x * 7f + sideZ * 3f)) % fl;
                KitchenProps.C(t, new Vector3(x, H + 0.1f, sideZ * (W * 0.5f - 0.1f)), new Vector3(0.12f, 0.12f, 0.12f), petals[(k + (int)g.id[0]) % petals.Length], 0f, "Flor_nectar");
            }
        // HOTEL DE INSECTOS (punta +X, sobre el tablón), CAJA DE ABEJORROS, CAJA NIDO y VERMICOMPOSTADOR: dentro de la huella del bancal.
        Transform hotel = KitchenProps.Root(t, "Hotel_insectos", new Vector3(L * 0.5f - 0.25f, H, W * 0.25f));
        KitchenProps.C(hotel, new Vector3(0f, 0.45f, 0f), new Vector3(0.06f, 0.9f, 0.06f), wood, 0f, "Poste");
        KitchenProps.C(hotel, new Vector3(0f, 1.05f, 0f), new Vector3(0.36f, 0.42f, 0.2f), new Color(0.62f, 0.48f, 0.3f), 0f, "Casa");
        for (int i = 0; i < 6; i++) KitchenProps.C(hotel, new Vector3(-0.12f + (i % 3) * 0.12f, 0.95f + (i / 3) * 0.16f, 0.105f), new Vector3(0.08f, 0.08f, 0.01f), new Color(0.3f, 0.22f, 0.14f), 0f, "Tubos_cana");
        KitchenProps.C(hotel, new Vector3(0f, 1.3f, 0f), new Vector3(0.44f, 0.05f, 0.26f), wood, 0f, "Tejadillo");
        Transform bees = KitchenProps.Root(t, "Caja_abejorros", new Vector3(L * 0.5f - 0.25f, H, -W * 0.25f));
        KitchenProps.C(bees, new Vector3(0f, 0.15f, 0f), new Vector3(0.36f, 0.28f, 0.3f), new Color(0.95f, 0.8f, 0.3f), 0f, "Colonia_abejorros");
        KitchenProps.C(bees, new Vector3(0.1f, 0.08f, 0.16f), new Vector3(0.05f, 0.03f, 0.01f), KitchenProps.Black, 0f, "Piquera");
        Transform nest = KitchenProps.Root(t, "Caja_nido_carbonero", new Vector3(-L * 0.5f + 0.25f, H, W * 0.25f));
        KitchenProps.C(nest, new Vector3(0f, 0.8f, 0f), new Vector3(0.05f, 1.6f, 0.05f), wood, 0f, "Poste");
        KitchenProps.C(nest, new Vector3(0f, 1.7f, 0f), new Vector3(0.18f, 0.26f, 0.18f), new Color(0.5f, 0.38f, 0.24f), 0f, "Caja_nido");
        KitchenProps.C(nest, new Vector3(0f, 1.74f, 0.092f), new Vector3(0.05f, 0.05f, 0.005f), KitchenProps.Black, 0f, "Agujero_28mm");
        Transform worms = KitchenProps.Root(t, "Vermicompostador", new Vector3(-L * 0.5f + 0.25f, H, -W * 0.25f));
        KitchenProps.C(worms, new Vector3(0f, 0.12f, 0f), new Vector3(0.34f, 0.24f, 0.34f), new Color(0.32f, 0.36f, 0.3f), 0f, "Cubo_lombrices");
        // EL GOTEO DEL BANCAL: cabezal en la punta −X (filtro, válvula), lateral por encima de la tierra y un gotero por frutal.
        Transform head = KitchenProps.Root(t, "Cabezal_riego", new Vector3(-L * 0.5f + 0.5f, H, 0f));
        KitchenProps.C(head, new Vector3(0f, 0.12f, 0f), new Vector3(0.12f, 0.22f, 0.12f), new Color(0.3f, 0.5f, 0.85f), 0f, "Filtro_anillas");
        KitchenProps.C(head, new Vector3(0.12f, 0.08f, 0f), new Vector3(0.1f, 0.1f, 0.1f), KitchenProps.DarkSteel, 0f, "Electrovalvula");
        Transform net = new GameObject("Riego_goteo_" + building).transform; net.SetParent(t, false);
        float px = -L * 0.5f + 0.5f;
        foreach (FruitPlant p in plants)
        {
            float x = p.transform.localPosition.x;
            KitchenProps.C(net, new Vector3((px + x) * 0.5f, H + 0.02f, 0.25f), new Vector3(Mathf.Abs(x - px) + 0.03f, 0.02f, 0.02f), new Color(0.12f, 0.12f, 0.13f), 0f, "Tuberia_goteo");
            KitchenProps.C(p.transform, new Vector3(0f, H + 0.03f, 0.25f), new Vector3(0.05f, 0.04f, 0.05f), new Color(0.15f, 0.55f, 0.25f), 0f, "Gotero");
            px = x;
        }
        // EL CARTEL del gremio.
        WorldLabel lab = t.gameObject.AddComponent<WorldLabel>();
        lab.text = $"Bancal de tierra · {g.nombre} · {g.ph} · 0,8 m de sustrato · compañeras: {string.Join(", ", comp)} · útiles: {string.Join(", ", g.insectos)}";
        lab.height = 2.9f;
        // UN MUEBLE: su caja de colisión es el marco (las copas no bloquean el paso; sí cuentan para los 2 m, con sus renderers).
        BoxCollider bc = t.gameObject.AddComponent<BoxCollider>();
        bc.center = new Vector3(0f, H * 0.5f, 0f); bc.size = new Vector3(L, H, W);
        return t;
    }
}
