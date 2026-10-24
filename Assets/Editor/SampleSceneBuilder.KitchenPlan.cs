using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// COCINA DEL RESTAURANTE EN TODA LA PLANTA (docs/restaurant-plan.md §3–6, Berón 2026-10-23):
///  · En el centro, el PASE (barra de servicio, mesa de emplatado, encimera de ingredientes, despensa y estantería).
///  · Alrededor, en ANILLO, OCHO PUESTOS COMPLETOS (uno por persona del equipo: si la cocina se satura, los de preparación y
///    limpieza también cocinan). Cada puesto es una U con todo lo que pide la carta (Gastronomy.Equipment), así los ocho
///    pueden hacer el mismo plato a la vez: encimera, inducción y fregadero al fondo; wok, paellero y fogón a un lado;
///    marmitas, vaporera, trompo y abatidor al otro; delante, la barra fría (sushi y montaje) mirando al pase.
///  · Por fuera, junto a los bordes y por el flujo real: recepción, almacén seco y cámaras junto al puente; preparación y
///    lavado (sucio → lavavajillas → limpio) en el lado opuesto; y los OBRADORES DUPLICADOS (dos puestos por tarea) en
///    los otros dos lados: panadería y pasta, y quesería (con su cava y salmuera) y fermentos.
/// Entre bloques quedan 2 m (los equipos que van juntos, como las cámaras frías o la mesa y su máquina, van pegados).
/// </summary>
public static partial class SampleSceneBuilder
{
    // Puestos: centro (en la sala) y orientación; su lado abierto (−Z local) mira al pase.
    static readonly (Vector3 pos, float yaw)[] KitchenStations =
    {
        (new Vector3(-9f, 0f, 9.6f), 0f), (new Vector3(0f, 0f, 9.6f), 0f), (new Vector3(9f, 0f, 9.6f), 0f),
        (new Vector3(12f, 0f, 0f), 90f),
        (new Vector3(9f, 0f, -9.6f), 180f), (new Vector3(0f, 0f, -9.6f), 180f), (new Vector3(-9f, 0f, -9.6f), 180f),
        (new Vector3(-12f, 0f, 0f), -90f),
    };

    /// <summary>Lado de la plataforma de la cocina por el que llega su puente (0 +X, 1 −X, 2 +Z, 3 −Z, en el área) y
    /// dónde (a lo largo del lado). −1 si no hay puente.</summary>
    static int KitchenBridge(Transform area, out float along)
    {
        along = 0f;
        foreach (var l in _landings) if (l.area == area) { along = l.center; return l.side; }
        // Ya despejadas (o construidas en otro orden): se busca el puente que toca la plataforma.
        Transform mat = area.Find("Mat"); Vector3 half = mat != null ? mat.localScale * 0.5f : new Vector3(20f, 0f, 19f);
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            if (!t.name.StartsWith("Puente_") || t.name.Contains("pista")) continue;
            Renderer r = t.GetComponentInChildren<Renderer>(); if (r == null) continue;
            Bounds b = r.bounds; foreach (Renderer rr in t.GetComponentsInChildren<Renderer>()) b.Encapsulate(rr.bounds);
            for (int s = 0; s < 4; s++)
            {
                Vector3 edge = s == 0 ? new Vector3(half.x, 0f, 0f) : s == 1 ? new Vector3(-half.x, 0f, 0f) : s == 2 ? new Vector3(0f, 0f, half.z) : new Vector3(0f, 0f, -half.z);
                Vector3 c = area.InverseTransformPoint(b.center);
                bool facing = s < 2 ? Mathf.Sign(c.x) == Mathf.Sign(edge.x) && Mathf.Abs(c.z) < half.z : Mathf.Sign(c.z) == Mathf.Sign(edge.z) && Mathf.Abs(c.x) < half.x;
                Vector3 near = area.InverseTransformPoint(b.ClosestPoint(area.TransformPoint(edge + (s < 2 ? new Vector3(0f, 0f, c.z) : new Vector3(c.x, 0f, 0f)))));
                if (facing && (s < 2 ? Mathf.Abs(Mathf.Abs(near.x) - half.x) : Mathf.Abs(Mathf.Abs(near.z) - half.z)) < 1.5f) { along = s < 2 ? c.z : c.x; return s; }
            }
        }
        return -1;
    }

    static Vector3 SideOut(int s) => s == 0 ? Vector3.right : s == 1 ? Vector3.left : s == 2 ? Vector3.forward : Vector3.back;

    /// <summary>Planta de la cocina: suelo, pase central, 8 puestos completos en anillo y la franja de cada borde.</summary>
    static void KitchenFloorPlan(Transform room, Transform area, out Transform island, out Transform pass, out Transform desp, out Transform est2)
    {
        Transform mat = area != null ? area.Find("Mat") : null;
        Vector2 half = mat != null ? new Vector2(mat.localScale.x, mat.localScale.z) * 0.5f : new Vector2(20f, 19f);

        // Suelo de baldosa en TODA la planta (la cocina la ocupa entera; el comedor va arriba, en el anillo).
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Suelo_Cocina"; floor.transform.SetParent(room, false);
        floor.transform.localPosition = new Vector3(0f, 0.005f, 0f); floor.transform.localScale = new Vector3(2f * half.x - 0.4f, 0.04f, 2f * half.y - 0.4f);
        floor.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Cocina_Baldosa_MAT", new Color(0.9f, 0.91f, 0.9f));
        for (float x = -half.x + 2f; x < half.x - 1f; x += 2f) Joint(room, new Vector3(x, 0.027f, 0f), new Vector3(0.02f, 0.01f, 2f * half.y - 0.4f));
        for (float z = -half.y + 2f; z < half.y - 1f; z += 2f) Joint(room, new Vector3(0f, 0.027f, z), new Vector3(2f * half.x - 0.4f, 0.01f, 0.02f));

        // ── PASE CENTRAL ── la barra de servicio (de aquí suben los platos al comedor), la mesa de emplatado y la encimera de
        // ingredientes, con la despensa y la estantería compartidas a los lados.
        pass = KitchenProps.CounterRun(room, "Barra de servicio", new Vector3(0f, 0f, 0f), 5f, 180f);
        pass.gameObject.AddComponent<WorldLabel>().text = "Pase (barra de servicio)";
        island = KitchenProps.CounterRun(room, "Mesa de emplatado", new Vector3(0f, 0f, 2.4f), 4.2f, 0f, 1.2f);
        island.gameObject.AddComponent<WorldLabel>().text = "Mesa de emplatado";
        for (int s = 0; s < 3; s++)
            for (int p = 0; p < 4; p++) KitchenProps.Plate(island, "Plato", new Vector3(-1.6f + s * 1.6f, KitchenProps.CounterTop + p * 0.018f, 0.3f));
        KitchenProps.CounterRun(room, "Encimera ingredientes", new Vector3(0f, 0f, -2.4f), 5.5f, 180f);
        string[] items = { "Huevos", "Verduras", "Avena", "Fruta", "Yogur", "Legumbres" };
        for (int i = 0; i < items.Length; i++)
        {
            Transform holder = KitchenProps.Root(room, items[i], new Vector3(-2.1f + i * 0.85f, KitchenProps.CounterTop, -2.4f));
            KitchenProps.Ingredient(holder, items[i], Vector3.zero);
            KitchenProps.Fit(holder, false);
            WorldLabel wl = holder.gameObject.AddComponent<WorldLabel>(); wl.text = items[i]; wl.height = 0.35f;
        }
        desp = KitchenProps.Shelving(room, "Despensa", new Vector3(-5.6f, 0f, 0f), 90f, 12);
        desp.gameObject.AddComponent<StockShelf>().label = "Despensa"; desp.gameObject.AddComponent<WorldLabel>().text = "Despensa";
        est2 = KitchenProps.Shelving(room, "Estanteria", new Vector3(5.6f, 0f, 0f), -90f, 13);

        // ── OCHO PUESTOS COMPLETOS EN ANILLO ──
        for (int i = 0; i < KitchenStations.Length; i++) CompleteStation(room, i + 1, KitchenStations[i].pos, KitchenStations[i].yaw);

        // ── LAS CUATRO FRANJAS DE LOS BORDES ── (en el marco del ÁREA: el puente y la escalera vertical se miden ahí)
        float along = 0f;
        int bridge = area != null ? KitchenBridge(area.transform, out along) : -1;
        if (bridge < 0) { bridge = 3; along = 0f; Debug.LogWarning("[Cocina] no encuentro el puente de la plataforma: la recepción va al lado −Z."); }
        KitchenBridgeSide = bridge;
        int opposite = bridge ^ 1;
        int[] others = bridge < 2 ? new[] { 2, 3 } : new[] { 0, 1 };
        var placed = new Dictionary<int, int>();
        placed[bridge] = Strip(room, area, half, bridge, along, StoresBlocks());
        placed[opposite] = Strip(room, area, half, opposite, along, PrepBlocks());
        placed[others[0]] = Strip(room, area, half, others[0], along, DoughBlocks());
        placed[others[1]] = Strip(room, area, half, others[1], along, DairyBlocks());
        Debug.Log($"[Cocina] planta entera: pase central, {KitchenStations.Length} puestos completos en anillo; recepción y cámaras junto al puente (lado {bridge}), " +
                  $"preparación y lavado enfrente, obradores de masas y de lácteos y fermentos a los lados ({placed[bridge] + placed[opposite] + placed[others[0]] + placed[others[1]]} bloques).");
    }

    static void Joint(Transform room, Vector3 pos, Vector3 size)
    {
        GameObject j = GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(j.GetComponent<Collider>());
        j.name = "Junta"; j.transform.SetParent(room, false);
        j.transform.localPosition = pos; j.transform.localScale = size;
        j.GetComponent<Renderer>().sharedMaterial = MakeMaterial("Cocina_Junta_MAT", new Color(0.78f, 0.8f, 0.8f));
    }

    /// <summary>Un puesto COMPLETO (≈ 7 × 6,5 m): todo el equipo de la carta en U, para que su cocinero haga cualquier plato.</summary>
    static void CompleteStation(Transform room, int n, Vector3 pos, float yaw)
    {
        string N = n.ToString();
        Transform st = KitchenProps.Root(room, $"Puesto_{N}", pos, yaw);
        WorldLabel sl = st.gameObject.AddComponent<WorldLabel>(); sl.text = $"Puesto {N} (completo)"; sl.height = 2.4f;
        // Fondo: encimera, inducción y fregadero (los de siempre, para los hechizos), y los dos hornos.
        Transform counter = KitchenProps.CounterRun(st, n == 1 ? "Encimera cocina" : $"Encimera {N}", new Vector3(-2.0f, 0f, 2.1f), 1.25f);
        Transform stove = KitchenProps.Stove(st, n == 1 ? "Plancha" : $"Plancha {N}", new Vector3(-0.9f, 0f, 2.1f));
        KitchenProps.Sink(st, n == 1 ? "Fregadero" : $"Fregadero {N}", new Vector3(0.1f, 0f, 2.1f));
        Transform pan = KitchenProps.Pan(st, $"Sarten {N}", new Vector3(-1.08f, KitchenProps.CounterTop + 0.02f, 1.97f), 0f);
        Transform pot = KitchenProps.Pot(st, $"Olla {N}", new Vector3(-0.72f, KitchenProps.CounterTop + 0.02f, 2.25f));
        pan.SetParent(stove, true); pot.SetParent(stove, true);
        Transform board = KitchenProps.BoardAndKnife(st, $"Tabla {N}", new Vector3(-2.15f, KitchenProps.CounterTop, 2.05f));
        Transform bowl = KitchenProps.Bowl(st, $"Bol {N}", new Vector3(-1.7f, KitchenProps.CounterTop, 2.0f));
        KitchenProps.UtensilCrock(st, $"Utensilios {N}", new Vector3(-1.6f, KitchenProps.CounterTop, 2.3f));
        for (int p = 0; p < 4; p++) KitchenProps.Plate(st, "Plato", new Vector3(-2.45f, KitchenProps.CounterTop + p * 0.018f, 2.25f));
        board.SetParent(counter, true); bowl.SetParent(counter, true);
        foreach (Transform t in new[] { stove, pan, pot, board, bowl }) if (t.GetComponent<WorldLabel>() == null) { WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = t.name; wl.height = 0.5f; }
        Eq(st, "horno mixto", new Vector3(1.25f, 0f, 2.1f), 0f);
        Eq(st, "horno de pizza (eléctrico)", new Vector3(2.45f, 0f, 2.0f), 0f);
        // Lado izquierdo: el fuego (wok, paellero, fogón) y la nevera del puesto.
        Eq(st, "wok de inducción", new Vector3(-3.1f, 0f, 0.9f), -90f);
        Eq(st, "paellero de inducción", new Vector3(-3.1f, 0f, -0.25f), -90f);
        Eq(st, "fogón de inducción", new Vector3(-3.1f, 0f, -1.4f), -90f);
        Transform fridge = KitchenProps.Fridge(st, n == 1 ? "Nevera" : $"Nevera {N}", new Vector3(-3.1f, 0f, -2.5f), -90f);
        fridge.localScale = new Vector3(1.2f, 1.15f, 1.1f);
        WorldLabel fl = fridge.gameObject.AddComponent<WorldLabel>(); fl.text = $"Nevera · puesto {N}"; fl.height = 2.4f;
        // Lado derecho: marmitas, vaporera, trompo y abatidor.
        Eq(st, "marmita", new Vector3(3.1f, 0f, 0.9f), 90f);
        Eq(st, "marmita a presión", new Vector3(3.1f, 0f, -0.25f), 90f);
        Eq(st, "vaporera", new Vector3(3.1f, 0f, -1.4f), 90f);
        Eq(st, "trompo vertical (inducción)", new Vector3(3.1f, 0f, -2.5f), 90f);
        Eq(st, "abatidor", new Vector3(3.1f, 0f, -3.6f), 90f);
        // Delante, de cara al pase: la barra fría (sushi y montaje). La entrada al puesto queda a su derecha (2 m).
        Eq(st, "barra de sushi", new Vector3(-0.6f, 0f, -3.3f), 180f);
    }

    // ── FRANJAS DE LOS BORDES ────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>Un bloque de una franja: su ancho (a lo largo del borde) y cómo se construye (x = su centro).</summary>
    delegate void BlockBuilder(Transform strip, float x, float z);


    /// <summary>Coloca los bloques de un lado, en fila junto al borde, con 2 m entre ellos; salta la llegada del puente y la
    /// escalera vertical (centro del lado +Z). En los lados ±Z, si no caben, abre una segunda fila. Devuelve cuántos puso.</summary>
    static int Strip(Transform room, Transform area, Vector2 half, int side, float bridgeAlong, List<(string name, float w, float d, BlockBuilder build)> blocks)
    {
        bool zSide = side >= 2;
        float depth = zSide ? 6f : 5f, edge = zSide ? half.y : half.x;
        float L = zSide ? 2f * (half.x - 6f) : 2f * (half.y - 2.5f);   // los ±Z dejan las esquinas a los ±X
        Transform strip = new GameObject($"Franja_{(side == 0 ? "E" : side == 1 ? "O" : side == 2 ? "N" : "S")}").transform;
        strip.SetParent(room, false);
        Vector3 outDir = SideOut(side);
        Vector3 centerA = outDir * (edge - depth * 0.5f) + Vector3.up * 0.1f;
        if (area != null) { strip.position = area.TransformPoint(centerA); strip.rotation = area.rotation * Quaternion.LookRotation(outDir); }
        else { strip.localPosition = -centerA; strip.localRotation = Quaternion.LookRotation(-outDir); }
        // A lo largo de la franja (su X local) en coordenadas del área: +X local = derecha de mirar hacia fuera.
        Vector3 right = Vector3.Cross(Vector3.up, outDir);
        var skip = new List<(float a, float b)>();
        if (KitchenBridgeSide == side) { float c = Vector3.Dot(side < 2 ? new Vector3(0f, 0f, bridgeAlong) : new Vector3(bridgeAlong, 0f, 0f), right); skip.Add((c - 5f, c + 5f)); }
        if (side == 2) { float c = 0f; skip.Add((c - 2.5f, c + 2.5f)); }   // la escalera vertical sube al centro del lado +Z
        float back = depth * 0.5f - 0.35f, rowZ = back;
        float x = -L * 0.5f; int n = 0, row = 0;
        foreach (var bl in blocks)
        {
            for (int guard = 0; guard < 4; guard++)
            {
                foreach (var (a, b) in skip) if (x < b && x + bl.w > a) x = b;
                if (x + bl.w <= L * 0.5f) break;
                if (!zSide || row > 0) { x = float.MaxValue; break; }
                row++; rowZ = back - 2.9f; x = -L * 0.5f;   // segunda fila (los lados ±Z tienen 6 m de fondo)
            }
            if (x == float.MaxValue) { Debug.LogWarning($"[Cocina] «{bl.name}» no cabe en la franja {strip.name}."); continue; }
            Transform b2 = KitchenProps.Root(strip, bl.name, new Vector3(x + bl.w * 0.5f, 0f, rowZ - bl.d * 0.5f));
            bl.build(b2, 0f, 0f);
            WorldLabel wl = b2.gameObject.AddComponent<WorldLabel>(); wl.text = bl.name.Replace('_', ' '); wl.height = 2.9f;
            x += bl.w + 2f; n++;
        }
        return n;
    }

    static int KitchenBridgeSide = -1;

    // Una mesa de trabajo de acero.
    static Transform Bench(Transform p, string name, float x, float len, float d = 0.7f) => KitchenProps.CounterRun(p, name, new Vector3(x, 0f, 0f), len, 0f, d);

    // Cámara frigorífica (un cuarto: paredes, puerta y su termómetro).
    static void WalkIn(Transform p, string name, float x, float w, float d, string temp, Color tint)
    {
        Transform t = KitchenProps.Root(p, name, new Vector3(x, 0f, 0f));
        KitchenProps.C(t, new Vector3(0f, 1.2f, 0f), new Vector3(w, 2.4f, d), new Color(0.88f, 0.9f, 0.92f), 0f, "Paneles");
        KitchenProps.C(t, new Vector3(0f, 1.05f, -d * 0.5f - 0.02f), new Vector3(0.95f, 2.0f, 0.04f), tint, 0f, "Puerta");
        KitchenProps.C(t, new Vector3(0.35f, 1.05f, -d * 0.5f - 0.06f), new Vector3(0.04f, 0.4f, 0.04f), KitchenProps.Steel, 0f, "Tirador");
        KitchenProps.C(t, new Vector3(-0.75f, 1.9f, -d * 0.5f - 0.03f), new Vector3(0.3f, 0.12f, 0.02f), new Color(0.3f, 0.8f, 0.5f), 0f, "Termometro");
        KitchenProps.Fit(t);
        WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = $"{name.Replace('_', ' ')} ({temp})"; wl.height = 2.7f;
    }

    // Recepción, almacén seco y cámaras: junto al puente (la mercancía entra por él).
    static List<(string, float, float, BlockBuilder)> StoresBlocks() => new List<(string, float, float, BlockBuilder)>
    {
        ("Recepcion_de_mercancias", 2.8f, 0.8f, (t, x, z) =>
        {
            KitchenProps.C(t, new Vector3(-0.9f, 0.05f, 0f), new Vector3(0.9f, 0.1f, 0.8f), KitchenProps.Steel, 0f, "Bascula");
            KitchenProps.C(t, new Vector3(-0.9f, 0.6f, 0.35f), new Vector3(0.1f, 1.1f, 0.1f), KitchenProps.Steel, 0f, "Columna_bascula");
            KitchenProps.C(t, new Vector3(-0.9f, 1.2f, 0.33f), new Vector3(0.35f, 0.25f, 0.05f), new Color(0.2f, 0.25f, 0.3f), 0f, "Visor");
            Bench(t, "Mesa de control", 0.55f, 1.6f);
        }),
        ("Almacen_seco", 3.6f, 0.5f, (t, x, z) =>
        {
            KitchenProps.Shelving(t, "Estanteria seco 1", new Vector3(-0.9f, 0f, 0f), 0f, 21);
            KitchenProps.Shelving(t, "Estanteria seco 2", new Vector3(0.9f, 0f, 0f), 0f, 22);
        }),
        ("Camaras_frias", 7.6f, 2.4f, (t, x, z) =>   // van pegadas: comparten paneles y el pasillo de carga
        {
            WalkIn(t, "Camara_de_refrigeracion", -2.6f, 2.6f, 2.4f, "2 °C", new Color(0.55f, 0.75f, 0.95f));
            WalkIn(t, "Camara_de_lacteos", 0f, 2.6f, 2.4f, "4 °C", new Color(0.75f, 0.85f, 0.95f));
            WalkIn(t, "Congelador", 2.5f, 2.4f, 2.4f, "−18 °C", new Color(0.4f, 0.55f, 0.85f));
        }),
    };

    // Preparación, lavado y el obrador «antiguo» (molienda, aceite y panela).
    static List<(string, float, float, BlockBuilder)> PrepBlocks() => new List<(string, float, float, BlockBuilder)>
    {
        ("Preparacion_verdura", 3.4f, 0.7f, (t, x, z) => { Bench(t, "Mesa verdura", -0.6f, 2.0f); KitchenProps.Sink(t, "Fregadero verdura", new Vector3(1.1f, 0f, 0f)); }),
        ("Preparacion_carne_y_pescado", 3.4f, 0.7f, (t, x, z) => { Bench(t, "Mesa carne y pescado", -0.6f, 2.0f); KitchenProps.Sink(t, "Fregadero carne", new Vector3(1.1f, 0f, 0f)); }),
        ("Lavado", 2.8f, 0.7f, (t, x, z) =>   // sucio → lavavajillas de capota → limpio (en línea, como en la realidad)
        {
            Bench(t, "Mesa sucia", -0.95f, 1.1f);
            KitchenProps.C(t, new Vector3(0f, 0.75f, 0f), new Vector3(0.75f, 1.5f, 0.75f), KitchenProps.Steel, 0f, "Lavavajillas_capota");
            KitchenProps.C(t, new Vector3(0f, 1.6f, 0f), new Vector3(0.7f, 0.2f, 0.7f), new Color(0.6f, 0.62f, 0.65f), 0f, "Capota");
            Bench(t, "Mesa limpia", 1.0f, 0.7f);
        }),
        ("Molienda", 4.4f, 1.0f, (t, x, z) =>
        {
            Eq(t, "molino de piedra", new Vector3(-1.7f, 0f, 0f), 0f);
            Eq(t, "nixtamalizadora y molino", new Vector3(-0.4f, 0f, 0f), 0f);
            Eq(t, "arrocera", new Vector3(0.75f, 0f, 0f), 0f);
            Eq(t, "comal", new Vector3(1.75f, 0f, 0f), 0f);
        }),
        ("Aceite_y_panela", 2.8f, 0.9f, (t, x, z) => { Eq(t, "trapiche y paila", new Vector3(-0.7f, 0f, 0f), 0f); Eq(t, "almazara", new Vector3(0.75f, 0f, 0f), 0f); }),
    };

    // Obrador de masas: dos puestos de panadería y dos de pasta, con el horno de pisos y la fermentadora compartidos.
    static List<(string, float, float, BlockBuilder)> DoughBlocks()
    {
        var l = new List<(string, float, float, BlockBuilder)>();
        for (int k = 1; k <= 2; k++)
        {
            int kk = k;
            l.Add(($"Panaderia_{kk}", 3.6f, 0.7f, (t, x, z) =>
            {
                Eq(t, "amasadora espiral", new Vector3(-1.4f, 0f, 0f), 0f);
                Bench(t, $"Mesa de panadería {kk}", 0.1f, 1.8f);
                Eq(t, "batidora planetaria", new Vector3(1.4f, 0f, 0f), 0f);
            }));
        }
        l.Add(("Horno_de_pisos_y_fermentadora", 3.0f, 1.3f, (t, x, z) =>
        {
            Transform h = KitchenProps.Root(t, "Horno de pisos", new Vector3(-0.55f, 0f, 0f));
            KitchenProps.C(h, new Vector3(0f, 0.35f, 0f), new Vector3(1.8f, 0.7f, 1.3f), KitchenProps.Steel, 0f, "Soporte");
            for (int i = 0; i < 3; i++)
            {
                KitchenProps.C(h, new Vector3(0f, 0.95f + i * 0.45f, 0f), new Vector3(1.8f, 0.42f, 1.3f), new Color(0.3f, 0.3f, 0.33f), 0f, "Piso");
                KitchenProps.C(h, new Vector3(0f, 0.95f + i * 0.45f, -0.66f), new Vector3(1.4f, 0.28f, 0.02f), new Color(1f, 0.6f, 0.25f, 0.6f), 0f, "Boca");
            }
            KitchenProps.Fit(h);
            Eq(t, "cámara de fermentación", new Vector3(1.05f, 0f, 0f), 0f);
        }));
        for (int k = 1; k <= 2; k++)
        {
            int kk = k;
            l.Add(($"Pasta_fresca_{kk}", 3.2f, 0.7f, (t, x, z) =>
            {
                Bench(t, $"Mesa de pasta {kk}", -0.8f, 1.8f);
                Eq(t, "laminadora de pasta", new Vector3(0.95f, 0f, 0f), 0f);
            }));
        }
        l.Add(("Extrusora", 1.6f, 0.6f, (t, x, z) => Eq(t, "extrusora y secadero", Vector3.zero, 0f)));
        return l;
    }

    // Obrador de lácteos y fermentos: dos queserías con su cava y salmuera, y dos puestos de fermentos.
    static List<(string, float, float, BlockBuilder)> DairyBlocks()
    {
        var l = new List<(string, float, float, BlockBuilder)>();
        for (int k = 1; k <= 2; k++)
        {
            int kk = k;
            l.Add(($"Queseria_{kk}", 3.2f, 0.8f, (t, x, z) =>
            {
                Eq(t, "cuba de quesería", new Vector3(-0.8f, 0f, 0f), 0f);
                Bench(t, $"Mesa de moldeo {kk}", 0.85f, 1.4f);
            }));
        }
        l.Add(("Cava_y_salmuera", 2.8f, 0.8f, (t, x, z) =>
        {
            Eq(t, "cava de maduración", new Vector3(-0.6f, 0f, 0f), 0f);
            KitchenProps.C(t, new Vector3(0.85f, 0.35f, 0f), new Vector3(0.9f, 0.7f, 0.7f), KitchenProps.Steel, 0f, "Tina_de_salmuera");
            KitchenProps.C(t, new Vector3(0.85f, 0.68f, 0f), new Vector3(0.8f, 0.02f, 0.6f), new Color(0.8f, 0.9f, 0.95f, 0.6f), 0f, "Salmuera");
        }));
        for (int k = 1; k <= 2; k++)
        {
            int kk = k;
            l.Add(($"Fermentos_{kk}", 2.2f, 0.8f, (t, x, z) =>
            {
                Eq(t, "yogurtera (incubadora)", new Vector3(-0.55f, 0f, 0f), 0f);
                Eq(t, "incubadora de tempeh", new Vector3(0.65f, 0f, 0f), 0f);
            }));
        }
        l.Add(("Barricas_y_tofu", 3.0f, 0.8f, (t, x, z) =>
        {
            Eq(t, "barricas de fermentación", new Vector3(-0.6f, 0f, 0f), 0f);
            Eq(t, "prensa de tofu", new Vector3(1.0f, 0f, 0f), 0f);
        }));
        return l;
    }
}
