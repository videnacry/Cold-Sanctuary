using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FRUTALES EN LOS TECHOS DE ALFOMBRAS (Berón 2026-10-30, docs/jardines-frutales.md). Los techos de las torres (el «piso de
/// alfombras» encima del último piso, del tamaño de la planta) solo llevaban dos áreas de alfombras y una fila de paneles:
/// el resto se llena con FRUTALES EN MACETA, UNA DE CADA ESPECIE (<see cref="FruitCare.All"/>) y otra vuelta, y otra…
/// hasta que no cabe ninguna más:
/// - cada maceta es un MUEBLE (raíz con BoxCollider de la maceta, sin renderer) que guarda los 2 m de paso
///   (<see cref="FurnitureSpacing.Standard"/>) con todo, medidos sobre la caja ENTERA (maceta + copa), como la prueba;
/// - a ≥ 2 m de las áreas de alfombras (ahí se posan y se sube) y de los paneles; dentro del techo (0,4 m del borde);
/// - primero las grandes (árboles, palmeras) y luego las pequeñas, así en cada vuelta caben todas;
/// - los techos no tienen plazas reservadas de microcosmos (<see cref="PlatformSlots"/> solo en pisos con área), así que
///   no compiten con ninguna.
/// Antes de las plantas, el PUNTO DE CUIDADOS de cada techo (una línea de equipos contra un borde): en Servicios la SALA
/// de la experta (banco de trasplante, depósito de fertirriego con dosificador, armario de abonos, mesa de diagnóstico con
/// pH/EC, herramientas de poda, compostador, carro de cosecha, mochila pulverizadora, caja de abejorros para polinizar y
/// escalera); en los demás, el depósito, el carro y el armario.
/// </summary>
public static partial class SampleSceneBuilder
{
    const float OrchardEdge = 0.4f, OrchardStep = 0.5f, OrchardPadClear = 2f;

    static void PlaceRooftopOrchards()
    {
        Physics.SyncTransforms();
        Renderer[] scene = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        int total = 0, roofs = 0;
        foreach (var (roof, size, bname) in _roofs)
        {
            if (roof == null || !roof.name.StartsWith("Techo_alfombras_")) continue;
            total += PlantRoof(roof, size, bname, scene);
            roofs++;
        }
        Debug.Log($"[Frutales] {total} frutales en maceta en {roofs} techos de alfombras (una de cada una de las {FruitCare.All.Length} especies por vuelta, hasta llenar).");
    }

    // Caja en planta (XZ) del mundo: la misma medida que FurnitureSpacing.
    static float OrchardGap(Bounds a, Bounds b)
    {
        float dx = Mathf.Max(0f, Mathf.Max(a.min.x - b.max.x, b.min.x - a.max.x));
        float dz = Mathf.Max(0f, Mathf.Max(a.min.z - b.max.z, b.min.z - a.max.z));
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    static bool OrchardBox(Transform t, out Bounds b)
    {
        b = default; bool any = false;
        foreach (Renderer r in t.GetComponentsInChildren<Renderer>()) { if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
        return any;
    }

    // ¿La caja cae dentro del techo (en su marco), a OrchardEdge del borde?
    static bool InsideRoof(Transform roof, Vector2 size, Bounds b)
    {
        float hx = size.x * 0.5f - OrchardEdge, hz = size.y * 0.5f - OrchardEdge;
        for (int i = 0; i < 4; i++)
        {
            Vector3 c = new Vector3((i & 1) == 0 ? b.min.x : b.max.x, roof.position.y, (i & 2) == 0 ? b.min.z : b.max.z);
            Vector3 l = roof.InverseTransformPoint(c);
            if (Mathf.Abs(l.x) > hx || Mathf.Abs(l.z) > hz) return false;
        }
        return true;
    }

    // Holgura hasta el borde útil del techo (en su marco), para el empaquetado.
    static float EdgeSlack(Transform roof, Vector2 size, Bounds b)
    {
        float hx = size.x * 0.5f - OrchardEdge, hz = size.y * 0.5f - OrchardEdge, s = float.MaxValue;
        for (int i = 0; i < 4; i++)
        {
            Vector3 l = roof.InverseTransformPoint(new Vector3((i & 1) == 0 ? b.min.x : b.max.x, roof.position.y, (i & 2) == 0 ? b.min.z : b.max.z));
            s = Mathf.Min(s, Mathf.Min(hx - Mathf.Abs(l.x), hz - Mathf.Abs(l.z)));
        }
        return s;
    }

    static int PlantRoof(Transform roof, Vector2 size, string bname, Renderer[] scene)
    {
        float fy = roof.position.y;
        // Lo que ya hay en el techo (y lo que asome de otros sitios a esa altura): paneles, postes, lo que sea.
        var blockers = new List<(Bounds b, float need)>();
        Bounds foot = new Bounds(roof.position + Vector3.up * 2.5f, new Vector3(size.x + size.y, 5f, size.x + size.y));
        foreach (Renderer r in scene)
        {
            if (r == null || !r.enabled || r.name == "Suelo_techo") continue;
            Bounds b = r.bounds;
            if (!b.Intersects(foot) || b.max.y < fy + 0.05f || b.min.y > fy + 6f) continue;
            if (r.GetComponentInParent<CarpetPad>() != null) continue;   // las alfombras: por su zona, abajo
            if (b.size.y < 0.3f && Mathf.Max(b.size.x, b.size.z) > 3f && b.max.y < fy + 0.3f) continue;   // suelos y losas
            blockers.Add((b, FurnitureSpacing.Standard));
        }
        foreach (CarpetPad p in roof.GetComponentsInChildren<CarpetPad>())
        {
            BoxCollider bc = p.GetComponent<BoxCollider>(); if (bc == null) continue;
            Vector3 e = p.transform.rotation * bc.size;
            blockers.Add((new Bounds(p.transform.TransformPoint(bc.center), new Vector3(Mathf.Abs(e.x), Mathf.Abs(e.y), Mathf.Abs(e.z))), OrchardPadClear));
        }

        bool main = bname == TorreServicios;
        Transform station = PlaceCareStation(roof, size, bname, main, blockers);
        if (station != null)
        {
            foreach (Transform it in station) if (OrchardBox(it, out Bounds ib)) blockers.Add((ib, FurnitureSpacing.Standard));
            Transform mat = station.Find("Suelo_sala_cuidados");
            if (mat != null && mat.TryGetComponent(out Renderer mr)) blockers.Add((mr.bounds, 0.5f));
        }
        else Debug.LogWarning($"[Frutales] Techo de «{bname}»: no cabe su punto de cuidados.");

        // PODA DE CONTENCIÓN: si en la primera vuelta no caben todas las especies (techos pequeños), la experta poda más
        // corto los árboles, las palmeras, la banana y la sandía (copa más pequeña) y se vuelve a plantar.
        Transform group = null; int n = 0, rounds = 0; var missing = new List<string>(); float shrink = 1f;
        for (int attempt = 0; attempt < 4; attempt++)
        {
            shrink = 1f - 0.12f * attempt;
            if (group != null) Object.DestroyImmediate(group.gameObject);
            group = new GameObject("Frutales_" + bname).transform;
            group.SetParent(roof, false);
            (n, rounds) = PlantAll(roof, size, bname, fy, group, blockers, shrink, missing);
            if (missing.Count == 0) break;
        }
        // EL ORDEN DE LA RONDA: en zigzag por franjas de 4 m, empezando por el lado del punto de cuidados (plantadas en el
        // orden del empaquetado, saltaban de punta a punta del techo: ~1,3 km andando en Servicios).
        var all = new List<FruitPlant>(group.GetComponentsInChildren<FruitPlant>());
        float sz = station != null && roof.InverseTransformPoint(station.position).z > 0f ? -1f : 1f;   // desde el lado del punto
        int Band(FruitPlant p) => Mathf.FloorToInt((sz * roof.InverseTransformPoint(p.transform.position).z + size.y * 0.5f) / 4f);
        all.Sort((a, b) =>
        {
            int ba = Band(a), bb = Band(b);
            if (ba != bb) return ba.CompareTo(bb);
            float xa = roof.InverseTransformPoint(a.transform.position).x, xb = roof.InverseTransformPoint(b.transform.position).x;
            return (ba % 2 == 0 ? 1 : -1) * xa.CompareTo(xb);
        });
        for (int i = 0; i < all.Count; i++) all[i].order = i;
        Debug.Log($"[Frutales] Techo de «{bname}» ({size.x:0}×{size.y:0} m): {n} macetas en {rounds - 1} vueltas" + (shrink < 1f ? $", copas podadas al {shrink * 100f:0} %" : "") +
                  (missing.Count > 0 ? $"; en la primera vuelta NO cupieron: {string.Join(", ", missing)}" : "; las " + FruitCare.All.Length + " especies") + ".");
        if (missing.Count > 0) Debug.LogWarning($"[Frutales] Techo de «{bname}»: faltan especies ({string.Join(", ", missing)}): amplía el techo o aparta algo.");
        return n;
    }

    static (int n, int rounds) PlantAll(Transform roof, Vector2 size, string bname, float fy, Transform group, List<(Bounds b, float need)> blockers, float shrink, List<string> missing)
    {
        missing.Clear();
        var placed = new List<Bounds>();
        var fichas = new List<FruitCare.Ficha>(FruitCare.All);   // ya van de las grandes a las pequeñas
        int n = 0, rounds = 0;
        float hx = size.x * 0.5f, hz = size.y * 0.5f;
        while (n < 400)
        {
            int before = n;
            foreach (FruitCare.Ficha f in fichas)
            {
                Transform plant = BuildPotted(group, f, n, bname, fy, f.forma == FruitCare.Forma.Arbol || f.forma == FruitCare.Forma.Palmera || f.kind == GardenCrop.Kind.Banana || f.kind == GardenCrop.Kind.Sandia ? shrink : 1f);
                if (!OrchardBox(plant, out Bounds b0)) { Object.DestroyImmediate(plant.gameObject); continue; }
                Vector3 p0 = plant.position;
                // EL HUECO MÁS AJUSTADO (empaquetado): de los sitios donde cabe con sus 2 m, el que menos holgura deja con
                // lo de al lado (otra maceta, un equipo, las alfombras o el borde); así no quedan huecos sueltos inservibles.
                bool ok = false; float bestSlack = float.MaxValue; Vector3 bestW = p0; Bounds bestB = b0;
                for (float z = -hz + OrchardEdge; z <= hz - OrchardEdge; z += OrchardStep)
                    for (float x = -hx + OrchardEdge; x <= hx - OrchardEdge; x += OrchardStep)
                    {
                        Vector3 w = roof.TransformPoint(new Vector3(x, 0f, z)); w.y = p0.y;
                        Bounds b = b0; b.center += w - p0;
                        if (!InsideRoof(roof, size, b)) continue;
                        float slack = EdgeSlack(roof, size, b);
                        foreach (Bounds q in placed) { float s = OrchardGap(q, b) - FurnitureSpacing.Standard; if (s < slack) slack = s; if (slack < 0f) break; }
                        if (slack >= 0f) foreach (var (q, need) in blockers) { float s = OrchardGap(q, b) - need; if (s < slack) slack = s; if (slack < 0f) break; }
                        if (slack < 0f || slack >= bestSlack - 0.001f) continue;
                        bestSlack = slack; bestW = w; bestB = b; ok = true;
                    }
                if (ok) { plant.position = bestW; placed.Add(bestB); }
                if (!ok)
                {
                    Object.DestroyImmediate(plant.gameObject);
                    if (rounds == 0) missing.Add(f.nombre);
                    continue;
                }
                plant.GetComponent<FruitPlant>().order = n;
                n++;
            }
            rounds++;
            if (n == before) break;
        }
        return (n, rounds);
    }

    // UNA MACETA con su frutal (variedad enana, a la escala de su ficha) y su estado de cuidado.
    static Transform BuildPotted(Transform group, FruitCare.Ficha f, int n, string bname, float fy, float shrink = 1f)
    {
        Transform root = new GameObject($"Frutal_{f.kind}_{n + 1}").transform;
        root.SetParent(group, false);
        root.position = new Vector3(group.position.x, fy, group.position.z);
        Vector3 m = f.maceta;
        Color clay = new Color(0.71f, 0.5f, 0.38f), rim = new Color(0.6f, 0.42f, 0.32f), soil = new Color(0.3f, 0.22f, 0.16f);
        KitchenProps.C(root, new Vector3(0f, m.y * 0.5f, 0f), new Vector3(m.x, m.y, m.z), clay, 0f, "Maceta");
        KitchenProps.C(root, new Vector3(0f, m.y - 0.03f, 0f), new Vector3(m.x + 0.06f, 0.06f, m.z + 0.06f), rim, 0f, "Borde");
        KitchenProps.C(root, new Vector3(0f, m.y + 0.01f, 0f), new Vector3(m.x - 0.1f, 0.02f, m.z - 0.1f), soil, 0f, "Sustrato");
        BoxCollider bc = root.gameObject.AddComponent<BoxCollider>();   // un MUEBLE (FurnitureSpacing): caja de la maceta, sin renderer en la raíz
        bc.center = new Vector3(0f, m.y * 0.5f, 0f); bc.size = new Vector3(m.x + 0.06f, m.y, m.z + 0.06f);
        GardenCrop.Build(root, f.kind, root.TransformPoint(new Vector3(0f, m.y + 0.02f, 0f)), 4100 + n * 7 + (int)f.kind, f.escala * shrink);
        FruitPlant fp = root.gameObject.AddComponent<FruitPlant>();
        fp.kind = f.kind; fp.building = bname;
        var rnd = new System.Random(n * 31 + (int)f.kind);   // estados de partida distintos (unas con más sed que otras)
        fp.water = 0.35f + 0.4f * (float)rnd.NextDouble(); fp.nutrients = 0.35f + 0.35f * (float)rnd.NextDouble();
        fp.pests = 0.1f + 0.3f * (float)rnd.NextDouble(); fp.health = 0.55f + 0.25f * (float)rnd.NextDouble();
        WorldLabel wl = root.gameObject.AddComponent<WorldLabel>(); wl.text = f.nombre; wl.height = 1.6f;
        return root;
    }

    // EL PUNTO DE CUIDADOS: una línea de equipos (FurnitureLine: van juntos) contra un borde libre, mirando hacia dentro.
    static Transform PlaceCareStation(Transform roof, Vector2 size, string bname, bool main, List<(Bounds b, float need)> blockers)
    {
        Transform line = new GameObject(main ? $"Sala_cuidados_frutales_{bname}" : $"Punto_cuidados_frutales_{bname}").transform;
        line.SetParent(roof, false);
        line.gameObject.AddComponent<FurnitureLine>();
        FruitCareStation st = line.gameObject.AddComponent<FruitCareStation>();
        st.building = bname; st.mainRoom = main;
        float len = BuildCareEquipment(line, main);
        float depth = main ? 3.6f : 2.6f;   // la línea y su zona de trabajo delante
        if (main)
        {
            Transform mat = Solid(line, new Vector3(0f, 0.02f, 1.45f), new Vector3(len + 0.6f, 0.04f, 2.2f), new Color(0.74f, 0.79f, 0.7f), "Suelo_sala_cuidados");
            Object.DestroyImmediate(mat.GetComponent<Collider>());
        }
        WorldLabel wl = line.gameObject.AddComponent<WorldLabel>();
        wl.text = main ? "Sala de cuidados de los frutales · experta: Irosene" : "Punto de cuidados de los frutales";
        wl.height = 2.6f;
        // Bordes candidatos (en el marco del techo): centro de la línea y su giro (su +Z, hacia dentro).
        float hx = size.x * 0.5f, hz = size.y * 0.5f, back = 0.5f + 0.45f;
        var cands = new List<(Vector3 p, float yaw)>();
        foreach (float s in new[] { 0f, -0.3f, 0.3f, -0.6f, 0.6f })
        {
            float rx = Mathf.Max(0f, hx - len * 0.5f - 1f), rz = Mathf.Max(0f, hz - len * 0.5f - 1f);
            cands.Add((new Vector3(s * rx, 0f, hz - back), 180f));
            cands.Add((new Vector3(s * rx, 0f, -hz + back), 0f));
            cands.Add((new Vector3(hx - back, 0f, s * rz), -90f));
            cands.Add((new Vector3(-hx + back, 0f, s * rz), 90f));
        }
        Physics.SyncTransforms();
        foreach (var (p, yaw) in cands)
        {
            line.localPosition = p; line.localRotation = Quaternion.Euler(0f, yaw, 0f);
            Bounds zone = new Bounds(line.TransformPoint(new Vector3(0f, 1f, depth * 0.5f - 0.45f)), Vector3.zero);
            zone.Encapsulate(line.TransformPoint(new Vector3(-len * 0.5f, 0f, -0.45f))); zone.Encapsulate(line.TransformPoint(new Vector3(len * 0.5f, 2f, depth - 0.45f)));
            if (!InsideRoof(roof, size, zone)) continue;
            bool free = true;
            foreach (var (q, need) in blockers) if (OrchardGap(q, zone) < need) { free = false; break; }
            if (free) return line;
        }
        Object.DestroyImmediate(line.gameObject);
        return null;
    }

    // Los equipos, en fila a lo largo de la X local (de frente: +Z). Devuelve el largo de la línea.
    static float BuildCareEquipment(Transform line, bool main)
    {
        var items = new List<(string name, float w, System.Action<Transform> build)>();
        Color steel = KitchenProps.Steel, wood = KitchenProps.Wood, white = KitchenProps.White, green = new Color(0.45f, 0.62f, 0.4f),
              blue = new Color(0.55f, 0.72f, 0.86f), clay = new Color(0.71f, 0.5f, 0.38f), dark = KitchenProps.DarkSteel;
        if (main)
        {
            items.Add(("Banco_trasplante", 1.8f, t =>
            {
                KitchenProps.C(t, new Vector3(0f, 0.88f, 0f), new Vector3(1.8f, 0.06f, 0.8f), wood);
                foreach (float x in new[] { -0.82f, 0.82f }) foreach (float z in new[] { -0.32f, 0.32f }) KitchenProps.C(t, new Vector3(x, 0.43f, z), new Vector3(0.06f, 0.86f, 0.06f), KitchenProps.WoodDark);
                KitchenProps.C(t, new Vector3(0f, 1.0f, -0.35f), new Vector3(1.8f, 0.2f, 0.06f), wood, 0f, "Tope");
                for (int i = 0; i < 4; i++) KitchenProps.C(t, new Vector3(-0.6f + i * 0.35f, 0.99f, 0.1f), new Vector3(0.22f, 0.2f, 0.22f), clay, 0f, "Maceta_pequena");
                KitchenProps.C(t, new Vector3(0.6f, 0.4f, 0f), new Vector3(0.5f, 0.35f, 0.5f), new Color(0.32f, 0.24f, 0.18f), 0f, "Saco_sustrato");
            }));
            items.Add(("Deposito_fertirriego", 1.2f, t =>
            {
                KitchenProps.C(t, new Vector3(0f, 0.7f, 0f), new Vector3(1.1f, 1.4f, 1.0f), white, 0f, "Deposito_1000L");
                KitchenProps.C(t, new Vector3(0f, 1.45f, 0f), new Vector3(0.4f, 0.1f, 0.4f), dark, 0f, "Tapa");
                KitchenProps.C(t, new Vector3(0.4f, 0.4f, 0.55f), new Vector3(0.25f, 0.4f, 0.15f), blue, 0f, "Dosificador");
                KitchenProps.C(t, new Vector3(0.4f, 0.12f, 0.6f), new Vector3(0.06f, 0.06f, 0.3f), green, 0f, "Manguera");
            }));
            items.Add(("Armario_abonos", 1.2f, t =>
            {
                KitchenProps.C(t, new Vector3(0f, 0.95f, 0f), new Vector3(1.2f, 1.9f, 0.6f), new Color(0.82f, 0.84f, 0.78f));
                KitchenProps.C(t, new Vector3(-0.3f, 0.95f, 0.31f), new Vector3(0.56f, 1.8f, 0.02f), new Color(0.72f, 0.76f, 0.68f), 0f, "Puerta");
                KitchenProps.C(t, new Vector3(0.3f, 0.95f, 0.31f), new Vector3(0.56f, 1.8f, 0.02f), new Color(0.72f, 0.76f, 0.68f), 0f, "Puerta");
                KitchenProps.C(t, new Vector3(0f, 1.6f, 0.33f), new Vector3(0.5f, 0.12f, 0.01f), white, 0f, "Rotulo_NPK_quelatos_correctores");
            }));
            items.Add(("Mesa_diagnostico", 1.3f, t =>
            {
                KitchenProps.C(t, new Vector3(0f, 0.86f, 0f), new Vector3(1.3f, 0.05f, 0.7f), white);
                foreach (float x in new[] { -0.6f, 0.6f }) KitchenProps.C(t, new Vector3(x, 0.42f, 0f), new Vector3(0.05f, 0.84f, 0.6f), steel);
                KitchenProps.C(t, new Vector3(-0.35f, 0.95f, 0f), new Vector3(0.18f, 0.12f, 0.1f), dark, 0f, "Medidor_pH_EC");
                KitchenProps.C(t, new Vector3(0f, 0.93f, 0.1f), new Vector3(0.12f, 0.08f, 0.12f), steel, 0f, "Lupa");
                KitchenProps.C(t, new Vector3(0.35f, 0.95f, -0.1f), new Vector3(0.25f, 0.15f, 0.02f), new Color(0.95f, 0.9f, 0.2f), 0f, "Trampas_cromaticas");
                KitchenProps.C(t, new Vector3(0.4f, 0.9f, 0.18f), new Vector3(0.3f, 0.02f, 0.22f), white, 0f, "Cuaderno_registro");
            }));
            items.Add(("Estante_herramientas", 1.3f, t =>
            {
                KitchenProps.C(t, new Vector3(0f, 0.9f, -0.15f), new Vector3(1.3f, 1.8f, 0.05f), KitchenProps.WoodDark);
                KitchenProps.C(t, new Vector3(-0.4f, 1.2f, -0.08f), new Vector3(0.06f, 0.25f, 0.05f), new Color(0.8f, 0.2f, 0.2f), 0f, "Tijeras_poda");
                KitchenProps.C(t, new Vector3(0f, 1.1f, -0.08f), new Vector3(0.06f, 0.6f, 0.05f), steel, 0f, "Serrucho_poda");
                KitchenProps.C(t, new Vector3(0.4f, 1.0f, -0.08f), new Vector3(0.06f, 0.8f, 0.05f), new Color(0.8f, 0.2f, 0.2f), 0f, "Tijeras_dos_manos");
                KitchenProps.C(t, new Vector3(0f, 0.45f, 0.05f), new Vector3(0.9f, 0.4f, 0.35f), green, 0f, "Regaderas");
            }));
            items.Add(("Compostador", 1.0f, t =>
            {
                KitchenProps.C(t, new Vector3(0f, 0.5f, 0f), new Vector3(1.0f, 1.0f, 0.9f), new Color(0.3f, 0.36f, 0.3f));
                KitchenProps.C(t, new Vector3(0f, 1.03f, 0f), new Vector3(1.02f, 0.06f, 0.92f), dark, 0f, "Tapa");
            }));
            items.Add(("Caja_abejorros", 0.7f, t =>   // polinización (fresa, sandía, mortiño, almendro, aguacate…): EDEN ISS no tenía y fracasaron las fresas
            {
                KitchenProps.C(t, new Vector3(0f, 0.45f, 0f), new Vector3(0.6f, 0.9f, 0.5f), wood, 0f, "Soporte");
                KitchenProps.C(t, new Vector3(0f, 1.05f, 0f), new Vector3(0.45f, 0.3f, 0.35f), new Color(0.95f, 0.8f, 0.3f), 0f, "Colonia_abejorros");
                KitchenProps.C(t, new Vector3(0.15f, 0.95f, 0.18f), new Vector3(0.06f, 0.04f, 0.02f), KitchenProps.Black, 0f, "Piquera");
            }));
            items.Add(("Pulverizador_mochila", 0.6f, t =>
            {
                KitchenProps.C(t, new Vector3(0f, 0.3f, 0f), new Vector3(0.4f, 0.6f, 0.3f), new Color(0.85f, 0.5f, 0.2f));
                KitchenProps.C(t, new Vector3(0.15f, 0.65f, 0f), new Vector3(0.04f, 0.7f, 0.04f), dark, 0f, "Lanza");
            }));
            items.Add(("Carro_cosecha", 1.0f, t =>
            {
                KitchenProps.C(t, new Vector3(0f, 0.5f, 0f), new Vector3(1.0f, 0.06f, 0.6f), steel);
                foreach (float x in new[] { -0.45f, 0.45f }) KitchenProps.C(t, new Vector3(x, 0.15f, 0f), new Vector3(0.1f, 0.3f, 0.5f), dark, 0f, "Rueda");
                KitchenProps.C(t, new Vector3(-0.22f, 0.66f, 0f), new Vector3(0.45f, 0.26f, 0.5f), green, 0f, "Caja_cosecha");
                KitchenProps.C(t, new Vector3(0.24f, 0.66f, 0f), new Vector3(0.45f, 0.26f, 0.5f), green, 0f, "Caja_cosecha");
                KitchenProps.C(t, new Vector3(0f, 0.9f, -0.3f), new Vector3(0.8f, 0.04f, 0.04f), dark, 0f, "Asa");
            }));
            items.Add(("Escalera_tijera", 0.7f, t =>
            {
                KitchenProps.C(t, new Vector3(0f, 0.9f, -0.15f), new Vector3(0.55f, 1.8f, 0.05f), steel, new Vector3(-10f, 0f, 0f), "Tramo");
                KitchenProps.C(t, new Vector3(0f, 0.9f, 0.15f), new Vector3(0.55f, 1.8f, 0.05f), steel, new Vector3(10f, 0f, 0f), "Tramo");
            }));
        }
        else
        {
            items.Add(("Deposito_fertirriego", 1.0f, t =>
            {
                KitchenProps.C(t, new Vector3(0f, 0.6f, 0f), new Vector3(0.9f, 1.2f, 0.9f), white, 0f, "Deposito_500L");
                KitchenProps.C(t, new Vector3(0.3f, 0.35f, 0.5f), new Vector3(0.22f, 0.35f, 0.12f), blue, 0f, "Dosificador");
            }));
            items.Add(("Carro_fertirriego", 1.0f, t =>
            {
                KitchenProps.C(t, new Vector3(0f, 0.45f, 0f), new Vector3(0.9f, 0.5f, 0.55f), green, 0f, "Tanque_carro");
                foreach (float x in new[] { -0.4f, 0.4f }) KitchenProps.C(t, new Vector3(x, 0.12f, 0f), new Vector3(0.1f, 0.24f, 0.45f), dark, 0f, "Rueda");
                KitchenProps.C(t, new Vector3(0f, 0.75f, 0.2f), new Vector3(0.5f, 0.1f, 0.1f), dark, 0f, "Enrollador_manguera");
            }));
            items.Add(("Armario_herramientas", 1.0f, t =>
            {
                KitchenProps.C(t, new Vector3(0f, 0.9f, 0f), new Vector3(1.0f, 1.8f, 0.55f), new Color(0.82f, 0.84f, 0.78f));
                KitchenProps.C(t, new Vector3(0f, 0.9f, 0.29f), new Vector3(0.9f, 1.7f, 0.02f), new Color(0.72f, 0.76f, 0.68f), 0f, "Puerta");
            }));
        }
        const float gap = 0.3f;
        float len = -gap; foreach (var it in items) len += it.w + gap;
        float x0 = -len * 0.5f;
        foreach (var (name, w, build) in items)
        {
            Transform t = KitchenProps.Root(line, name, new Vector3(x0 + w * 0.5f, 0f, 0f));
            build(t);
            KitchenProps.Fit(t);
            x0 += w + gap;
        }
        return len;
    }
}
