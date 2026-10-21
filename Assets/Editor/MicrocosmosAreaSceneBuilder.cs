using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ESCENAS DE MICROCOSMOS POR ÁREA (Sala de Yoga, Huerto, Enfermería, Veterinaria, Textil, Mecánica). Antes estas
/// misiones se jugaban DENTRO del mesocosmos agrandando su decorado (RealityShiftController); ahora cada área tiene su
/// propia escena — como Mesopotamia — y se entra con el hechizo de virtualización (la máquina del área o magia), que la
/// carga en aditivo con MobWorldLoader. Cada escena: suelo, luz, punto de aparición, tablero de misiones del área
/// (MobWorldMissionBoard: menú → misión → vuelve al menú) y el portal de salida. Origen lejano distinto por área.
/// </summary>
public static class MicrocosmosAreaSceneBuilder
{
    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos por área")]
    public static void BuildAll()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        int i = 0;
        foreach (var a in SampleSceneBuilder.MicroAreas)
        {
            Vector3 o = new Vector3(6000f + i * 400f, 0f, 6000f);   // no solapa el mundo base ni a Mesopotamia (5000)
            Build(SampleSceneBuilder.MicroSceneName(a.key), a.display, a.tint, a.kind, o);
            i++;
        }
        Debug.Log($"[Microcosmos] {i} escenas de microcosmos por área generadas.");
    }

    static void Build(string sceneName, string display, Color tint, SampleSceneBuilder.MissionKind kind, Vector3 o)
    {
        string path = $"Assets/Scenes/{sceneName}.unity";
        Scene prev = EditorSceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);

        var sun = new GameObject("Sun");
        sun.transform.SetPositionAndRotation(o + Vector3.up * 10f, Quaternion.Euler(50f, -30f, 0f));
        sun.AddComponent<Light>().type = LightType.Directional;

        // ENFERMERÍA: su microcosmos es la historia de AMBROSIO (su muerte camino de la cueva), en un mundo extenso de antes
        // del fuego, dentro de la enfermería vista a escala de insecto (×5: ocupa ~el 30 % del santuario).
        if (kind == SampleSceneBuilder.MissionKind.Heal) { BuildInfirmaryMicrocosmos(o, display); Finish(scene, path, prev); return; }
        // Suelo del microcosmos (40×40) con el color del área, y "decorado gigante" (lo que en el mundo real es pequeño).
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground"; ground.transform.position = o; ground.transform.localScale = new Vector3(4f, 1f, 4f);
        ground.GetComponent<Renderer>().sharedMaterial = Mat($"{sceneName}_Ground", Color.Lerp(tint, Color.white, 0.4f));
        var rnd = new System.Random(sceneName.GetHashCode());
        for (int k = 0; k < 6; k++)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = "Decorado_Gigante";
            float ang = k * 60f + (float)rnd.NextDouble() * 20f;
            Vector3 d = Quaternion.Euler(0f, ang, 0f) * Vector3.forward;
            float h = 3f + (float)rnd.NextDouble() * 5f;
            g.transform.position = o + d * (12f + (float)rnd.NextDouble() * 4f) + Vector3.up * h * 0.5f;
            g.transform.localScale = new Vector3(3f + (float)rnd.NextDouble() * 3f, h, 3f + (float)rnd.NextDouble() * 3f);
            g.GetComponent<Renderer>().sharedMaterial = Mat($"{sceneName}_Decor", tint * 0.8f + new Color(0, 0, 0, 1));
        }

        var spawn = new GameObject("MobSpawnPoint");
        spawn.transform.SetPositionAndRotation(o + new Vector3(0f, 1f, -6f), Quaternion.LookRotation(Vector3.forward));
        spawn.AddComponent<MobSpawnPoint>();

        var portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portal.name = "WorldExitPortal";
        portal.transform.position = o + new Vector3(0f, 1f, -9f);
        portal.transform.localScale = new Vector3(1.5f, 2f, 0.3f);
        portal.GetComponent<Collider>().isTrigger = true;
        portal.GetComponent<Renderer>().sharedMaterial = Mat("MobWorldExitPortal", new Color(0.75f, 0.65f, 0.85f));
        portal.AddComponent<WorldExitPortal>();

        // Misiones del área (las mismas que antes se jugaban en el sitio) en modo escena: al completarlas limpian aquí.
        var root = new GameObject($"Microcosmos_{display}");
        root.transform.position = o;
        MobMission[] missions = SampleSceneBuilder.BuildAreaMissions(root.transform, root.transform, display, tint, kind);
        foreach (MobMission m in missions)
        {
            if (m == null) continue;
            m.transform.position = o + new Vector3(0f, 0f, 3f);
            foreach (MeditationMissionBase b in m.GetComponents<MeditationMissionBase>()) b.endMode = MissionEndMode.Standalone;
        }
        // HUERTO: un campo de cultivos GIGANTES (aquí eres pequeño) que los visitantes hambrientos quieren morder.
        if (kind == SampleSceneBuilder.MissionKind.GardenChannel)
        {
            GardenCrop.Kind[] kinds = { GardenCrop.Kind.Tomate, GardenCrop.Kind.Maiz, GardenCrop.Kind.Quinoa, GardenCrop.Kind.Col, GardenCrop.Kind.Lenteja, GardenCrop.Kind.Pimiento, GardenCrop.Kind.Banana, GardenCrop.Kind.Pina, GardenCrop.Kind.Arroz, GardenCrop.Kind.Berenjena };
            for (int k = 0; k < 10; k++)
            {
                float ang = k * 36f * Mathf.Deg2Rad;
                Vector3 p = o + new Vector3(Mathf.Cos(ang) * 7f, 0f, 4f + Mathf.Sin(ang) * 5f);
                GardenCrop.Build(root.transform, kinds[k % kinds.Length], p, 900 + k, 3f);
            }
        }
        var board = root.AddComponent<MobWorldMissionBoard>();
        board.areaName = display;
        board.missions = missions;

        // NavMesh propio del microcosmos (los animales visitantes son animas reales con NavMeshAgent). Por VOLUMEN: en el editor
        // la escena del mesocosmos también está abierta y no debe entrar en este horneado.
        var nav = ground.AddComponent<Unity.AI.Navigation.NavMeshSurface>();
        nav.collectObjects = Unity.AI.Navigation.CollectObjects.Volume;
        nav.center = Vector3.up * 5f; nav.size = new Vector3(44f, 20f, 44f);
        nav.BuildNavMesh();

        EditorSceneManager.SaveScene(scene, path);
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(s => s.path == path)) { scenes.Add(new EditorBuildSettingsScene(path, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
        EditorSceneManager.CloseScene(scene, removeScene: true);
        if (prev.IsValid()) SceneManager.SetActiveScene(prev);
    }


    static void Finish(Scene scene, string path, Scene prev)
    {
        EditorSceneManager.SaveScene(scene, path);
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(s => s.path == path)) { scenes.Add(new EditorBuildSettingsScene(path, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
        EditorSceneManager.CloseScene(scene, removeScene: true);
        if (prev.IsValid()) SceneManager.SetActiveScene(prev);
    }

    // ── MICROCOSMOS DE LA ENFERMERÍA (docs/microcosmos-level1.md + docs/microcosmos-enfermeria.md) ─────────────────────
    // El santuario mide ~200 × 215 m (≈ 43 000 m²); su 30 % son ≈ 13 000 m². La enfermería ocupa 26 × 20 = 520 m², así que
    // para llenar ese 30 % se amplía ×5 (×25 en superficie): ≈ 130 × 100 m. Sus muebles (camas, camilla, analizador…)
    // son aquí MONTAÑAS y MESETAS; en el claro del centro transcurre la historia del alba: Sakshi, Ambrosio el pulgón, las
    // gemelas, la tribu de Héspero, la cueva… y alrededor, la flora y fauna de antes del fuego (sabana): hormigas león con
    // sus embudos, escarabajos peloteros con su bola, termitas, mantis, grillos, saltamontes, pulgones y mariquitas.
    public const float InfirmaryScale = 5f;

    static void BuildInfirmaryMicrocosmos(Vector3 o, string display)
    {
        var root = new GameObject($"Microcosmos_{display}");
        // 1) La historia (los builders de Scene1, autorados en el origen; luego todo el root se lleva a o).
        SampleSceneBuilder.BuildMicrocosmosSandbox(root.transform);
        SampleSceneBuilder.BuildNivel1Sandbox(root.transform);

        // 2) La ENFERMERÍA GIGANTE alrededor (copia de su equipamiento ×5, sin conducta), con el claro central libre.
        GameObject inf = GameObject.Find("Infirmary_Area");
        Transform equip = inf != null ? inf.transform.Find("Equipamiento") : null;
        if (equip != null)
        {
            GameObject giant = Object.Instantiate(equip.gameObject);
            giant.name = "Enfermeria_gigante";
            foreach (MonoBehaviour mb in giant.GetComponentsInChildren<MonoBehaviour>(true)) Object.DestroyImmediate(mb);
            giant.transform.SetParent(root.transform, false);
            giant.transform.localPosition = Vector3.zero; giant.transform.localRotation = Quaternion.identity;
            giant.transform.localScale = Vector3.one * InfirmaryScale;
            var kill = new List<GameObject>();
            foreach (Transform t in giant.transform)
            {
                if (t.name.StartsWith("Suelo")) { t.localPosition = new Vector3(t.localPosition.x, -0.006f, t.localPosition.z); continue; }   // el suelo, justo bajo el del bosque
                Vector3 p = t.position; if (Mathf.Abs(p.x) < 24f && Mathf.Abs(p.z) < 24f) kill.Add(t.gameObject);   // claro de la historia
            }
            foreach (GameObject g in kill) Object.DestroyImmediate(g);
        }
        else Debug.LogWarning("[Microcosmos] No encuentro Infirmary_Area (abre la escena principal): sin enfermería gigante.");

        // 3) Flora de antes del fuego: hierbas altas, helechos, setas, musgo, piedras y acacias enanas.
        var rnd = new System.Random(4242);
        float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
        Vector3 Ring(float rMin, float rMax) { float a = R(0f, Mathf.PI * 2f), r = R(rMin, rMax); return new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r); }
        var flora = new GameObject("Flora_antes_del_fuego").transform; flora.SetParent(root.transform, false);
        Material grass = Mat("Micro_Hierba", new Color(0.62f, 0.68f, 0.3f)), fern = Mat("Micro_Helecho", new Color(0.25f, 0.5f, 0.25f));
        Material stem = Mat("Micro_Pie_Seta", new Color(0.9f, 0.86f, 0.75f)), cap = Mat("Micro_Sombrero", new Color(0.7f, 0.35f, 0.2f));
        Material moss = Mat("Micro_Musgo", new Color(0.3f, 0.45f, 0.2f)), rock = Mat("Micro_Piedra", new Color(0.55f, 0.52f, 0.48f));
        Material bark = Mat("Micro_Acacia", new Color(0.4f, 0.3f, 0.2f)), crown = Mat("Micro_Copa", new Color(0.45f, 0.55f, 0.25f));
        for (int i = 0; i < 140; i++) { Vector3 p = Ring(22f, 70f); float h = R(2f, 6f); Box(flora, "Hierba", p + Vector3.up * h * 0.5f, new Vector3(0.15f, h, 0.5f), Quaternion.Euler(R(-12f, 12f), R(0f, 180f), R(-12f, 12f)), grass, false); }
        for (int i = 0; i < 30; i++) { Vector3 p = Ring(22f, 70f); for (int k = 0; k < 5; k++) Box(flora, "Helecho", p + Vector3.up * 1.2f, new Vector3(0.6f, 0.08f, 3.2f), Quaternion.Euler(-25f, k * 72f, 0f), fern, false); }
        for (int i = 0; i < 20; i++) { Vector3 p = Ring(22f, 68f); float h = R(1f, 2.5f); Box(flora, "Seta_pie", p + Vector3.up * h * 0.5f, new Vector3(0.4f, h, 0.4f), Quaternion.identity, stem, true); Box(flora, "Seta_sombrero", p + Vector3.up * h, new Vector3(1.8f, 0.5f, 1.8f), Quaternion.Euler(0f, R(0f, 90f), 0f), cap, true); }
        for (int i = 0; i < 40; i++) { Vector3 p = Ring(20f, 72f); Box(flora, "Musgo", p + Vector3.up * 0.15f, new Vector3(R(2f, 5f), 0.3f, R(2f, 5f)), Quaternion.Euler(0f, R(0f, 90f), 0f), moss, false); }
        for (int i = 0; i < 25; i++) { Vector3 p = Ring(24f, 72f); float s = R(1f, 3.5f); Box(flora, "Piedra", p + Vector3.up * s * 0.4f, new Vector3(s, s * 0.8f, s * 1.1f), Quaternion.Euler(R(0f, 20f), R(0f, 90f), 0f), rock, true); }
        for (int i = 0; i < 8; i++) { Vector3 p = Ring(30f, 70f); Box(flora, "Acacia_tronco", p + Vector3.up * 4f, new Vector3(0.8f, 8f, 0.8f), Quaternion.identity, bark, true); Box(flora, "Acacia_copa", p + Vector3.up * 8.5f, new Vector3(9f, 1.2f, 9f), Quaternion.Euler(0f, R(0f, 90f), 0f), crown, false); }

        // 4) Fauna de antes del fuego (prefabs reales del santuario; si falta alguno, se omite).
        var fauna = new GameObject("Fauna_antes_del_fuego").transform; fauna.SetParent(root.transform, false);
        Material sand = Mat("Micro_Arena", new Color(0.82f, 0.72f, 0.5f)), dung = Mat("Micro_Estiercol", new Color(0.35f, 0.25f, 0.15f));
        (string sp, int n, float rMin, float rMax)[] list = { ("Antlion", 5, 26f, 60f), ("DungBeetle", 5, 26f, 66f), ("Termite", 8, 24f, 60f), ("Mantis", 3, 30f, 66f), ("Cricket", 5, 22f, 66f), ("Grasshopper", 5, 22f, 66f), ("Aphid", 6, 22f, 50f), ("Ladybug", 4, 22f, 60f) };
        int placed = 0;
        foreach (var (sp, n, rMin, rMax) in list)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Animals/{sp}/{sp}.prefab");
            for (int i = 0; i < n; i++)
            {
                Vector3 p = Ring(rMin, rMax);
                if (sp == "Antlion")   // su EMBUDO de arena: escalones que bajan (la trampa de la hormiga león)
                    for (int k = 0; k < 4; k++) Box(fauna, "Embudo_arena", p + Vector3.up * (0.02f + k * 0.01f), new Vector3(5f - k * 1.1f, 0.05f, 5f - k * 1.1f), Quaternion.Euler(0f, k * 22f, 0f), sand, false);
                if (sp == "DungBeetle") Box(fauna, "Bola_estiercol", p + new Vector3(0.9f, 0.45f, 0f), Vector3.one * 0.9f, Quaternion.Euler(R(0f, 90f), R(0f, 90f), 0f), dung, true);
                if (prefab == null) continue;
                GameObject a = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                a.transform.SetParent(fauna, false); a.transform.localPosition = p + Vector3.up * 0.4f;
                placed++;
            }
            if (prefab == null) Debug.Log($"[Microcosmos] Sin prefab {sp}: solo su rastro (embudo/bola).");
        }

        root.transform.position = o;   // todo a su origen lejano

        // Entrada (cerca del claro) y salida.
        var spawn = new GameObject("MobSpawnPoint"); spawn.transform.SetParent(root.transform, false);
        spawn.transform.localPosition = new Vector3(0f, 1f, -6f); spawn.transform.localRotation = Quaternion.LookRotation(Vector3.forward);
        spawn.AddComponent<MobSpawnPoint>();
        var portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portal.name = "WorldExitPortal"; portal.transform.SetParent(root.transform, false);
        portal.transform.localPosition = new Vector3(0f, 1f, -9f); portal.transform.localScale = new Vector3(1.5f, 2f, 0.3f);
        portal.GetComponent<Collider>().isTrigger = true;
        portal.GetComponent<Renderer>().sharedMaterial = Mat("MobWorldExitPortal", new Color(0.75f, 0.65f, 0.85f));
        portal.AddComponent<WorldExitPortal>();

        // NavMesh de todo el mundo (el claro, la sabana y la enfermería gigante).
        var navGo = new GameObject("NavMesh_Microcosmos"); navGo.transform.SetParent(root.transform, false);
        var nav = navGo.AddComponent<Unity.AI.Navigation.NavMeshSurface>();
        nav.collectObjects = Unity.AI.Navigation.CollectObjects.Volume;
        nav.center = Vector3.up * 8f; nav.size = new Vector3(150f, 40f, 150f);
        nav.BuildNavMesh();
        Debug.Log($"[Microcosmos] Enfermería ×{InfirmaryScale}: la historia de Ambrosio en el claro, flora y {placed} animales de antes del fuego.");
    }

    static Transform Box(Transform parent, string name, Vector3 local, Vector3 size, Quaternion rot, Material m, bool solid)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; g.transform.SetParent(parent, false);
        g.transform.localPosition = local; g.transform.localRotation = rot; g.transform.localScale = size;
        if (!solid) Object.DestroyImmediate(g.GetComponent<Collider>());
        g.GetComponent<Renderer>().sharedMaterial = m;
        return g.transform;
    }
    static Material Mat(string name, Color c)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(sh) { name = name, color = c };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        return m;
    }
}
