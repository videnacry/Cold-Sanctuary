using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// EL SANTUARIO MARINO, escena propia (Berón 2026-11-05; docs/sitio-nuclear.md §20; docs/world-topology-and-planes.md §3.1): el lecho a su hondura real, las dos salas con columnas, la galería,
/// el pie del tubo, el pie del pozo de escape y el refugio. Pocas piezas: se carga aditiva solo cuando Kushal baja. Se construye con su propio menú, sin tocar el santuario 1.
/// Las coordenadas son las del sitio, muy por debajo de todo lo demás: al cargarla encima del santuario no choca con nada.
/// </summary>
public static partial class SampleSceneBuilder
{
    public const string MarineScenePath = "Assets/Scenes/" + SiteLine.MarineScene + ".unity";

    [MenuItem("Tools/Cold Sanctuary/Build Santuario Marino (escena del fondo)")]
    public static void BuildMarineScene()
    {
        MicrocosmosAreaSceneBuilder.DropUntitled();
        Scene prev = EditorSceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        GameObject go = new GameObject("Santuario_Marino");
        MarineBase b = go.AddComponent<MarineBase>();
        Transform root = go.transform;
        BuildMarineBase(root, b);
        EditorSceneManager.SaveScene(scene, MarineScenePath);
        var bs = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!bs.Exists(s => s.path == MarineScenePath)) bs.Add(new EditorBuildSettingsScene(MarineScenePath, true));
        EditorBuildSettings.scenes = bs.ToArray();
        AssetDatabase.SaveAssets();
        int pieces = root.GetComponentsInChildren<Transform>(true).Length;
        Debug.Log($"[Marino] {MarineScenePath}: {pieces} objetos; holgura {b.clearanceM:0.0} m.");
        if (prev.IsValid()) SceneManager.SetActiveScene(prev);
        EditorSceneManager.CloseScene(scene, removeScene: true);
    }

    static void BuildMarineBase(Transform root, MarineBase b)
    {
        Vector3 site = NuclearSiteRule.Center;
        Color conc = new Color(0.6f, 0.6f, 0.58f), steel = TreeSteel, pipe = new Color(0.25f, 0.5f, 0.75f), yel = new Color(0.95f, 0.8f, 0.2f), rockC = new Color(0.16f, 0.16f, 0.17f), red = new Color(0.8f, 0.2f, 0.15f);
        Vector3 at = new Vector3(site.x, NuclearSiteRule.CavernCenterY, site.z), wasteAt = at + new Vector3(0f, 0f, CavernRule.WasteOffsetZ);
        float floorY = at.y - NuclearSiteRule.CavernHeight * 0.5f + 0.6f, topY = at.y + NuclearSiteRule.CavernHeight * 0.5f;
        Vector3 hallR = new Vector3(CavernRule.ReactorLength, NuclearSiteRule.CavernHeight, CavernRule.ReactorSpan), hallW = new Vector3(CavernRule.WasteLength, NuclearSiteRule.CavernHeight, CavernRule.WasteSpan);

        // El lecho y una caja oscura alrededor: la roca que no se dibuja.
        KitchenProps.C(root, new Vector3(site.x, SeaRule.MarineFloorY - 0.5f, site.z + 40f), new Vector3(420f, 1f, 420f), new Color(0.2f, 0.2f, 0.2f), 0f, "Lecho_marino");
        Transform box = KitchenProps.Root(root, "Roca_alrededor", new Vector3(site.x, at.y - 14f, site.z + 40f));
        Hull(box, new Vector3(300f, 100f, 300f), 2f, rockC, "Roca");

        // ── LA SALA DEL MICRORREACTOR ──
        Transform cav = BuildCavern(root, "Caverna_microrreactor", at, hallR, "Sala del microrreactor: 60 m de roca encima");
        Color metal = new Color(0.55f, 0.6f, 0.65f);
        KitchenProps.C(cav, new Vector3(0f, -2f, 0f), new Vector3(10f, 3f, 3.2f), metal, 0f, "Modulo_reactor");
        KitchenProps.C(cav, new Vector3(0f, -0.3f, 0f), new Vector3(10f, 0.4f, 0.6f), steel, 0f, "Tubos_calor");
        KitchenProps.C(cav, new Vector3(-5.2f, -2f, 0f), new Vector3(0.6f, 3.4f, 3.4f), yel, 0f, "Tapa_trebol");
        KitchenProps.C(cav, new Vector3(7f, -4.1f, 4f), new Vector3(2f, 0.6f, 2f), pipe, 0f, "Sumidero_caverna");
        KitchenProps.C(root, new Vector3(at.x, (SeaRule.MarineFloorY + topY) * 0.5f, at.z), new Vector3(2.4f, SeaRule.MarineFloorY - topY, 2.4f), conc, 0f, "Pozo_microrreactor");
        // El reactor está MÁS ABAJO que su sumidero: el agua caliente sube sola hasta el mar. El intercambiador, en un hoyo: un iceberg que arase pasa por encima.
        for (int i = -1; i <= 1; i += 2)
            KitchenProps.C(root, new Vector3(at.x + 3f, (SeaRule.MarineFloorY + topY) * 0.5f, at.z + i * 1.2f), new Vector3(0.5f, SeaRule.MarineFloorY - topY, 0.5f), pipe, 0f, "Bucle_termosifon");
        Vector3 pit = new Vector3(at.x + 3f, SeaRule.MarineFloorY - 3f, at.z);
        KitchenProps.C(root, pit, new Vector3(5f, 1.6f, 4f), steel, 0f, "Intercambiador_lecho");
        for (int k = -1; k <= 1; k += 2)
        {
            KitchenProps.C(root, pit + new Vector3(k * 5f, 1.5f, 0f), new Vector3(1f, 7f, 10f), conc, 0f, "Hoyo_lecho");
            KitchenProps.C(root, pit + new Vector3(0f, 1.5f, k * 5f), new Vector3(10f, 7f, 1f), conc, 0f, "Hoyo_lecho");
        }

        // ── LA SALA DE RESIDUOS ──
        Transform waste = BuildCavern(root, "Caverna_residuos", wasteAt, hallW, "Sala de residuos y combustible gastado: se puede sacar todo");
        for (int i = 0; i < 6; i++) KitchenProps.C(waste, new Vector3(-7f + (i % 3) * 7f, -3f, i < 3 ? -2.5f : 2.5f), new Vector3(2f, 2.5f, 2f), new Color(0.72f, 0.5f, 0.3f), 0f, "Contenedor_residuos");

        // ── LA GALERÍA, por el pilar de roca entre las dos salas, el pie del tubo y el del pozo de escape ──
        Vector3 foot = new Vector3(at.x, floorY, at.z + CavernRule.ReactorSpan * 0.5f + CavernRule.PillarM * 0.5f);
        Transform gal = new GameObject("Galeria_cavernas").transform; gal.SetParent(root, false); gal.position = foot + Vector3.up * 1.5f;
        Hull(gal, new Vector3(3f, 3f, CavernRule.PillarM + 2f), 0.3f, conc, "Revestimiento_galeria");
        Transform bot = new GameObject("Estacion_inferior").transform; bot.SetParent(root, false); bot.position = foot;
        Hull(KitchenProps.Root(bot, "Caja_estacion", new Vector3(0f, 2.3f, 0f)), new Vector3(9f, 4.6f, 7f), 0.3f, conc, "Revestimiento_estacion");
        KitchenProps.C(bot, new Vector3(3f, 1f, 2.8f), new Vector3(1.2f, 2f, 0.5f), new Color(0.25f, 0.7f, 0.4f), 0f, "Registrador_roca");
        Label(bot, "Galería: registrador de la roca (fibra hasta el panel) y andén", 3f);
        BoxCollider ride = bot.gameObject.AddComponent<BoxCollider>(); ride.isTrigger = true; ride.center = new Vector3(0f, 1f, 0f); ride.size = new Vector3(3f, 2f, 3f);
        bot.gameObject.AddComponent<MarineLift>();
        b.arrival = bot;

        // El muñón del tubo: fijo, por debajo de donde llegan las quillas. Arriba, el conector; en el lecho, la compuerta.
        float cy = -IcebergRule.ConnectorDepth;
        Transform stump = new GameObject("Tubo_muñon").transform; stump.SetParent(root, false); stump.position = new Vector3(foot.x, (cy + floorY) * 0.5f + 2.3f, foot.z);
        Hull(stump, new Vector3(LineBore, cy - floorY - 4.6f, LineBore), 0.2f, steel, "Munon_tubo");
        KitchenProps.C(root, new Vector3(foot.x, cy, foot.z), new Vector3(LineBore + 2f, 2f, LineBore + 2f), yel, 0f, "Conector_rapido");
        KitchenProps.C(root, new Vector3(foot.x, SeaRule.MarineFloorY, foot.z), new Vector3(LineBore + 0.4f, 0.8f, LineBore + 0.4f), red, 0f, "Compuerta_lecho");
        KitchenProps.C(root, new Vector3(foot.x, SeaRule.MarineFloorY - 6f, foot.z), new Vector3(LineBore + 8f, 12f, LineBore + 8f), conc, 0f, "Collar_lecho");

        Vector3 along = Vector3.right, escapeFoot = foot + along * SiteLineRule.EscapeWalkM;
        KitchenProps.C(root, foot + along * (SiteLineRule.EscapeWalkM * 0.5f) + Vector3.up * 1.2f, new Vector3(SiteLineRule.EscapeWalkM, 2.4f, 2.4f), conc, 0f, "Galeria_escape");
        KitchenProps.C(root, new Vector3(escapeFoot.x, (cy + floorY) * 0.5f, escapeFoot.z), new Vector3(2.8f, cy - floorY, 2.8f), conc, 0f, "Pozo_escape_fondo");
        KitchenProps.C(root, new Vector3(escapeFoot.x, cy, escapeFoot.z), new Vector3(4f, 2f, 4f), yel, 0f, "Conector_escape");   // el punto débil del pozo: si lo arrastran, rompe aquí y no tira de la roca
        KitchenProps.C(root, escapeFoot + Vector3.up * 1.1f, new Vector3(2f, 2.2f, 2f), yel, 0f, "Jaula_escape");
        BuildRefuge(root, escapeFoot + along * 13f, at, b);

        BuildRockMonitoring(b, cav, waste, hallR, hallW);

        // ── EL SUELO: uno solo y sus cuatro límites. Se anda por las salas y la galería; las paredes de hormigón no tienen colisión todavía ──
        float x0 = at.x - 24f, x1 = at.x + 42f, z0 = at.z - 22f, z1 = wasteAt.z + 22f;
        Vector3 mid = new Vector3((x0 + x1) * 0.5f, floorY - 0.25f, (z0 + z1) * 0.5f);
        Solid(root, mid, new Vector3(x1 - x0, 0.5f, z1 - z0), new Color(0.32f, 0.33f, 0.34f), "Suelo_marino");
        void Limit(Vector3 c, Vector3 s) { BoxCollider w = new GameObject("Limite_marino").AddComponent<BoxCollider>(); w.transform.SetParent(root, false); w.transform.position = c; w.size = s; }
        Limit(new Vector3(x0 - 0.5f, floorY + 3f, mid.z), new Vector3(1f, 6f, z1 - z0));
        Limit(new Vector3(x1 + 0.5f, floorY + 3f, mid.z), new Vector3(1f, 6f, z1 - z0));
        Limit(new Vector3(mid.x, floorY + 3f, z0 - 0.5f), new Vector3(x1 - x0, 6f, 1f));
        Limit(new Vector3(mid.x, floorY + 3f, z1 + 0.5f), new Vector3(x1 - x0, 6f, 1f));
        Transform sun = new GameObject("Luz_salas").transform; sun.SetParent(root, false); sun.position = at + Vector3.up * 1f; sun.rotation = Quaternion.Euler(90f, 0f, 0f);
        Light l = sun.gameObject.AddComponent<Light>(); l.type = LightType.Point; l.range = 120f; l.intensity = 2f;
    }


    /// <summary>Los instrumentos de las dos salas, sus apoyos y la holgura MEDIDA (si no llega a la regla, error).</summary>
    static void BuildRockMonitoring(MarineBase b, Transform reactor, Transform waste, Vector3 hallR, Vector3 hallW)
    {
        Color yel = new Color(0.95f, 0.8f, 0.2f), steel = TreeSteel, dark = new Color(0.2f, 0.22f, 0.25f), blue = new Color(0.25f, 0.5f, 0.75f);
        void Instruments(Transform c, Vector3 size)
        {
            float hx = size.x * 0.5f - 1f, hy = size.y * 0.5f, hz = size.z * 0.5f;
            // Extensómetros: varillas a varias profundidades, 3 en el techo y 1 en la pared (el SFR usa de 2 y 6 m; aquí llegan a 8).
            for (int i = 0; i < 3; i++)
            {
                b.extensometers.Add(KitchenProps.C(c, new Vector3(-hx * 0.6f + i * hx * 0.6f, hy + 4f, 0f), new Vector3(0.08f, 8f, 0.08f), yel, 0f, "Extensometro_varilla"));
                KitchenProps.C(c, new Vector3(-hx * 0.6f + i * hx * 0.6f, hy - 0.35f, 0f), new Vector3(0.3f, 0.2f, 0.3f), dark, 0f, "Cabeza_extensometro");
            }
            b.extensometers.Add(KitchenProps.C(c, new Vector3(0f, 0f, hz + 4f), new Vector3(0.08f, 0.08f, 8f), yel, 0f, "Extensometro_varilla"));
            // Puntos de convergencia: dianas enfrentadas; se mide la distancia entre las de cada par.
            for (int i = -1; i <= 1; i += 2)
                for (int s = -1; s <= 1; s += 2)
                    b.convergence.Add(KitchenProps.C(c, new Vector3(i * hx * 0.5f, 1f, s * (hz - 0.75f)), new Vector3(0.25f, 0.25f, 0.05f), new Color(0.9f, 0.25f, 0.2f), 0f, "Punto_convergencia"));
            // Células de carga, geófonos y piezómetros.
            for (int i = -1; i <= 1; i++) b.loadCells.Add(KitchenProps.C(c, new Vector3(i * hx * 0.8f, hy + 1f, 2f), new Vector3(0.1f, 2.6f, 0.1f), steel, 0f, "Celula_carga_bulon"));
            for (int s = -1; s <= 1; s += 2) b.geophones.Add(KitchenProps.C(c, new Vector3(s * (hx + 3f), 2f, 0f), new Vector3(0.4f, 0.4f, 0.4f), dark, 0f, "Geofono_microsismico"));
            for (int s = -1; s <= 1; s += 2) b.piezometers.Add(KitchenProps.C(c, new Vector3(s * hx * 0.5f, hy + 6f, -3f), new Vector3(0.12f, 4f, 0.12f), blue, 0f, "Piezometro"));
            b.flowMeters.Add(KitchenProps.C(c, new Vector3(-hx * 0.8f, -hy + 0.9f, hz - 1.4f), new Vector3(0.8f, 0.6f, 0.6f), blue, 0f, "Aforo_sumidero"));
        }
        Instruments(reactor, hallR);
        Instruments(waste, hallW);
        BuildHallSupport(b, reactor, hallR);
        BuildHallSupport(b, waste, hallW);
        float cr = Clearance(reactor, hallR, "Modulo_reactor", "Tapa_trebol", "Tubos_calor"), cw = Clearance(waste, hallW, "Contenedor_residuos");
        b.clearanceM = Mathf.Min(cr, cw);
        if (b.clearanceM < CavernRule.ClearanceM || Mathf.Abs(b.clearanceM - CavernRule.DesignClearanceM) > 0.1f) Debug.LogError($"[Marino] Holgura de {b.clearanceM:0.0} m (pide ≥ {CavernRule.ClearanceM:0} m; la regla dice {CavernRule.DesignClearanceM:0.0}): reactor {cr:0.0} m, residuos {cw:0.0} m.");
    }

    // Una sala con apoyos: 4 columnas de roca del suelo al techo, cada una con su célula; bulones cada 6 m y cables más largos sobre la luz central.
    static void BuildHallSupport(MarineBase b, Transform c, Vector3 size)
    {
        Color stone = new Color(0.45f, 0.44f, 0.42f), steel = TreeSteel, dark = new Color(0.2f, 0.22f, 0.25f);
        float hy = size.y * 0.5f, inner = size.y - 1.2f, cs = CavernRule.ColumnSize;
        foreach (Vector2 p in CavernRule.Columns)
        {
            Solid(c, new Vector3(p.x, 0f, p.y), new Vector3(cs, inner, cs), stone, "Columna_roca");
            b.loadCells.Add(KitchenProps.C(c, new Vector3(p.x, 1f, p.y - cs * 0.5f - 0.1f), new Vector3(0.5f, 0.5f, 0.2f), dark, 0f, "Celula_columna"));
        }
        for (float x = -size.x * 0.5f + 3f; x < size.x * 0.5f; x += 6f)
            for (float z = -size.z * 0.5f + 2f; z < size.z * 0.5f; z += 6f)
                KitchenProps.C(c, new Vector3(x, hy + 1.5f, z), new Vector3(0.06f, 3.6f, 0.06f), steel, 0f, "Bulon_techo");
        for (int i = -1; i <= 1; i += 2)
            for (int j = -1; j <= 1; j += 2)
                KitchenProps.C(c, new Vector3(i * 3.5f, hy + 5f, j * 3.5f), new Vector3(0.1f, 10f, 0.1f), steel, 0f, "Cable_anclaje");
    }

    // La holgura de verdad: del equipo al forro, por los lados y por arriba (el suelo no cuenta: el equipo apoya en él), y a las columnas.
    static float Clearance(Transform cav, Vector3 size, params string[] pieces)
    {
        Vector3 inner = (size - Vector3.one * 1.2f) * 0.5f;
        float best = float.MaxValue, half = CavernRule.ColumnSize * 0.5f;
        foreach (Transform t in cav.GetComponentsInChildren<Transform>(true).Where(x => pieces.Contains(x.name)))
        {
            Vector3 c = cav.InverseTransformPoint(t.position), h = t.localScale * 0.5f;
            best = Mathf.Min(best, inner.x - Mathf.Abs(c.x) - h.x, inner.z - Mathf.Abs(c.z) - h.z, inner.y - c.y - h.y);
            foreach (Vector2 k in CavernRule.Columns)
                best = Mathf.Min(best, Mathf.Max(Mathf.Abs(c.x - k.x) - h.x - half, Mathf.Abs(c.z - k.y) - h.z - half));
        }
        return best;
    }

    /// <summary>El refugio: una sala estanca más allá del pie del pozo de escape, centrada en el pilar de roca, con su esclusa, aire, agua, comunicaciones y lo que hace falta para un mes.</summary>
    static void BuildRefuge(Transform root, Vector3 at, Vector3 reactorHall, MarineBase b)
    {
        Color wall = new Color(0.82f, 0.45f, 0.15f), steel = TreeSteel, dark = new Color(0.2f, 0.22f, 0.25f), blue = new Color(0.25f, 0.5f, 0.75f), green = new Color(0.25f, 0.7f, 0.4f);
        Vector3 size = new Vector3(20f, 3f, 8f);   // 160 m²: 13 m² por persona, para un mes
        Transform r = new GameObject("Refugio_fondo").transform; r.SetParent(root, false); r.position = at;
        b.refugeSpot = r;
        Hull(KitchenProps.Root(r, "Caja_refugio", new Vector3(0f, 1.5f, 0f)), size, 0.2f, wall, "Pared_refugio");
        KitchenProps.C(r, new Vector3(-size.x * 0.5f - 0.9f, 1.1f, 0f), new Vector3(1.6f, 2.2f, 2f), steel, 0f, "Esclusa_refugio");
        for (int i = 0; i < 8; i++) KitchenProps.C(r, new Vector3(-3.4f + i * 0.45f, 0.75f, 2f), new Vector3(0.28f, 1.5f, 0.28f), green, 0f, "Botella_oxigeno");
        KitchenProps.C(r, new Vector3(1.6f, 0.5f, 2f), new Vector3(1.6f, 1f, 0.7f), blue, 0f, "Deposito_agua");
        KitchenProps.C(r, new Vector3(3.2f, 0.6f, 2f), new Vector3(1f, 1.2f, 0.6f), dark, 0f, "Depurador_CO2");
        KitchenProps.C(r, new Vector3(3.4f, 1.3f, -1.6f), new Vector3(0.5f, 0.9f, 0.5f), dark, 0f, "Baterias_refugio");
        KitchenProps.C(r, new Vector3(-3.6f, 1.4f, -2.3f), new Vector3(0.4f, 0.4f, 0.1f), new Color(0.9f, 0.3f, 0.2f), 0f, "Telefono_fibra");
        KitchenProps.C(r, new Vector3(-2.8f, 1.4f, -2.3f), new Vector3(0.4f, 0.4f, 0.1f), steel, 0f, "Radio_traves_roca");
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(r, new Vector3(0f, 0.45f, s * 0.9f), new Vector3(6.4f, 0.1f, 0.7f), dark, 0f, "Banco_refugio");
        // UN MES: lo que hace agua y aire de la mar, y lo que da de comer. El agua llega sola: a 57 bar el mar ya empuja más que lo que pide la membrana.
        KitchenProps.C(r, new Vector3(5.5f, 1f, 3f), new Vector3(2.4f, 2f, 1.2f), blue, 0f, "Osmosis_inversa");
        KitchenProps.C(r, new Vector3(8.2f, 1f, 3f), new Vector3(2f, 2f, 1.2f), new Color(0.75f, 0.8f, 0.85f), 0f, "Electrolizador");
        KitchenProps.C(r, new Vector3(8.2f, 1f, -3f), new Vector3(2f, 2f, 1.2f), dark, 0f, "Depurador_amina");
        for (int i = -1; i <= 1; i += 2) KitchenProps.C(r, new Vector3(i * 8f, 1.5f, 3.7f), new Vector3(0.25f, 3f, 0.25f), blue, 0f, "Sondeo_mar");
        KitchenProps.C(r, new Vector3(-8f, 1.1f, -2.8f), new Vector3(2.6f, 2.2f, 1.6f), new Color(0.95f, 0.75f, 0.1f), 0f, "Perforadora_sondeos");
        KitchenProps.C(r, new Vector3(-8f, 2.5f, 3.2f), new Vector3(0.9f, 0.6f, 0.9f), new Color(0.8f, 0.2f, 0.15f), 0f, "Preventor_sondeo");
        KitchenProps.C(r, new Vector3(-5.5f, 1.1f, 3f), new Vector3(2.6f, 2.2f, 1f), new Color(0.6f, 0.45f, 0.3f), 0f, "Despensa_refugio");
        for (int i = 0; i < 2; i++) KitchenProps.C(r, new Vector3(-6f + i * 7f, 1.2f, -3.2f), new Vector3(5f, 2.2f, 0.9f), new Color(0.25f, 0.65f, 0.3f), 0f, "Estante_hidroponia");
        for (int i = 0; i < 3; i++) KitchenProps.C(r, new Vector3(-3.5f + i * 3.5f, 1.4f, 3.7f), new Vector3(3f, 2.4f, 0.12f), new Color(0.1f, 0.5f, 0.45f), 0f, "Panel_espirulina");
        KitchenProps.C(r, new Vector3(2.5f, 0.5f, -1.4f), new Vector3(1.2f, 1f, 0.6f), blue, 0f, "Deposito_abono");
        Vector3 reactor = reactorHall - r.position;
        KitchenProps.C(r, reactor * 0.5f + Vector3.up * 2.6f, new Vector3(0.2f, 0.2f, reactor.magnitude), new Color(0.9f, 0.45f, 0.15f), Quaternion.LookRotation(reactor), "Cable_reactor");
        Label(r, $"Refugio del fondo: {RefugeRule.Capacity} plazas, un mes con corriente y {RefugeRule.StoredHours / 24f:0} días con lo guardado", 3.4f);
    }
}
