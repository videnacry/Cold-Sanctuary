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
        // del fuego, dentro de la enfermería vista a escala de insecto (×10: el jugador es una hormiga).
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

        MicroWater.SurroundScene(scene);
        EditorSceneManager.SaveScene(scene, path);
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(s => s.path == path)) { scenes.Add(new EditorBuildSettingsScene(path, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
        EditorSceneManager.CloseScene(scene, removeScene: true);
        if (prev.IsValid()) SceneManager.SetActiveScene(prev);
    }


    static void Finish(Scene scene, string path, Scene prev)
    {
        MicroWater.SurroundScene(scene);
        EditorSceneManager.SaveScene(scene, path);
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(s => s.path == path)) { scenes.Add(new EditorBuildSettingsScene(path, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
        EditorSceneManager.CloseScene(scene, removeScene: true);
        if (prev.IsValid()) SceneManager.SetActiveScene(prev);
    }

    // ── MICROCOSMOS DE LA ENFERMERÍA (docs/microcosmos-level1.md + docs/microcosmos-enfermeria.md) ─────────────────────
    // Sus muebles (camas, camilla, analizador…)
    // son aquí MONTAÑAS y MESETAS; en el claro del centro transcurre la historia del alba: Sakshi, Ambrosio el pulgón, las
    // gemelas, la tribu de Héspero, la cueva… y alrededor, la flora y fauna de antes del fuego (sabana): hormigas león con
    // sus embudos, escarabajos peloteros con su bola, termitas, mantis, grillos, saltamontes, pulgones y mariquitas.
    // ESCALA: el JUGADOR (≈1,8 m) es una HORMIGA. Una hormiga real mide ~5 mm: el microcosmos es el mundo visto a ese
    // tamaño, donde los insectos ocupan el lugar de las personas y los animales. Así, la enfermería se amplía ×10
    // (≈ 260 × 200 m) y sus muebles son montañas; el musgo y los hongos hacen de hierba y arbustos, y las briznas de
    // hierba y los helechos, de árboles.
    public const float InfirmaryScale = 10f;
    const float StoryScale = 2.25f;   // la historia (autorada con hormigas de 0,8 m) a escala: hormiga ≈ 1,8 m = el jugador
    const float AntLength = 1.8f;

    static void BuildInfirmaryMicrocosmos(Vector3 o, string display)
    {
        var root = new GameObject($"Microcosmos_{display}");
        // 1) La historia (los builders de Scene1, autorados en el origen) en su claro, a la escala de la hormiga.
        var story = new GameObject("Historia_Ambrosio").transform; story.SetParent(root.transform, false);
        SampleSceneBuilder.BuildMicrocosmosSandbox(story);
        SampleSceneBuilder.BuildNivel1Sandbox(story);
        story.localScale = Vector3.one * StoryScale;
        int dressed = DressCast(story);

        // 2) La ENFERMERÍA GIGANTE alrededor (copia de su equipamiento ×10, sin conducta), con el claro central libre.
        float clear = 24f * StoryScale;
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
                Vector3 p = t.position; if (Mathf.Abs(p.x) < clear && Mathf.Abs(p.z) < clear) kill.Add(t.gameObject);   // claro de la historia
            }
            foreach (GameObject g in kill) Object.DestroyImmediate(g);
        }
        else Debug.LogWarning("[Microcosmos] No encuentro Infirmary_Area (abre la escena principal): sin enfermería gigante.");

        // 3) FLORA a escala de hormiga: musgo y hongos = hierba y arbustos; briznas y helechos = árboles; granos = rocas.
        var rnd = new System.Random(4242);
        float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
        Vector3 Ring(float rMin, float rMax) { float a = R(0f, Mathf.PI * 2f), r = R(rMin, rMax); return new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r); }
        float r0 = clear + 2f, r1 = 125f;
        var flora = new GameObject("Flora_antes_del_fuego").transform; flora.SetParent(root.transform, false);
        Material grass = Mat("Micro_Hierba", new Color(0.62f, 0.68f, 0.3f)), grassDry = Mat("Micro_Hierba_seca", new Color(0.78f, 0.72f, 0.42f));
        Material fern = Mat("Micro_Helecho", new Color(0.25f, 0.5f, 0.25f)), fernStem = Mat("Micro_Helecho_tallo", new Color(0.3f, 0.4f, 0.2f));
        Material stem = Mat("Micro_Pie_Seta", new Color(0.9f, 0.86f, 0.75f)), cap = Mat("Micro_Sombrero", new Color(0.7f, 0.35f, 0.2f));
        Material moss = Mat("Micro_Musgo", new Color(0.3f, 0.45f, 0.2f)), mossTip = Mat("Micro_Musgo_punta", new Color(0.45f, 0.6f, 0.25f));
        Material rock = Mat("Micro_Grano", new Color(0.55f, 0.52f, 0.48f)), seed = Mat("Micro_Semilla", new Color(0.55f, 0.4f, 0.25f));
        // Musgo (la «hierba» del microcosmos): matas de tallitos de 0,4–1,2 m, también en el claro (sin tapar la historia).
        for (int i = 0; i < 260; i++)
        {
            Vector3 p = Ring(i < 60 ? 8f : r0, r1);
            for (int k = 0; k < 4; k++) { float hh = R(0.4f, 1.2f); Box(flora, "Musgo", p + new Vector3(R(-0.6f, 0.6f), hh * 0.5f, R(-0.6f, 0.6f)), new Vector3(0.18f, hh, 0.18f), Quaternion.Euler(R(-8f, 8f), 0f, R(-8f, 8f)), k % 2 == 0 ? moss : mossTip, false); }
        }
        // Hongos (los «arbustos» y árboles pequeños): pie y sombrero, de 2 a 6 m.
        for (int i = 0; i < 45; i++) { Vector3 p = Ring(r0, r1); float hh = R(2f, 6f); Box(flora, "Hongo_pie", p + Vector3.up * hh * 0.5f, new Vector3(hh * 0.15f, hh, hh * 0.15f), Quaternion.identity, stem, true); Box(flora, "Hongo_sombrero", p + Vector3.up * hh, new Vector3(hh * 0.8f, hh * 0.18f, hh * 0.8f), Quaternion.Euler(0f, R(0f, 90f), 0f), cap, false); }
        // Briznas de hierba = ÁRBOLES: hojas finas de 10–25 m que se arquean.
        for (int i = 0; i < 90; i++)
        {
            Vector3 p = Ring(r0, r1); float hh = R(10f, 25f), lean = R(-18f, 18f), yaw = R(0f, 180f);
            Material m = i % 3 == 0 ? grassDry : grass;
            Box(flora, "Brizna", p + Vector3.up * hh * 0.3f, new Vector3(0.25f, hh * 0.6f, 1.2f), Quaternion.Euler(lean * 0.4f, yaw, 0f), m, true);
            Box(flora, "Brizna_punta", p + Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, hh * 0.78f, Mathf.Sin(lean * Mathf.Deg2Rad) * hh * 0.25f), new Vector3(0.2f, hh * 0.4f, 0.8f), Quaternion.Euler(lean, yaw, 0f), m, false);
        }
        // Helechos = ÁRBOLES grandes: un tallo y frondas radiales de 8–12 m.
        for (int i = 0; i < 25; i++)
        {
            Vector3 p = Ring(r0 + 10f, r1); float hh = R(12f, 22f);
            Box(flora, "Helecho_tallo", p + Vector3.up * hh * 0.5f, new Vector3(0.6f, hh, 0.6f), Quaternion.identity, fernStem, true);
            for (int k = 0; k < 7; k++) Box(flora, "Helecho_fronda", p + Vector3.up * hh + Quaternion.Euler(0f, k * 51f, 0f) * new Vector3(0f, -1.5f, 5f), new Vector3(2.4f, 0.2f, 11f), Quaternion.Euler(-20f, k * 51f, 0f), fern, false);
        }
        // Granos de arena y semillas = rocas y troncos caídos.
        for (int i = 0; i < 70; i++) { Vector3 p = Ring(r0, r1); float s = R(1.5f, 6f); Box(flora, "Grano", p + Vector3.up * s * 0.4f, new Vector3(s, s * 0.8f, s * 1.1f), Quaternion.Euler(R(0f, 25f), R(0f, 90f), R(0f, 15f)), rock, true); }
        for (int i = 0; i < 15; i++) { Vector3 p = Ring(r0, r1); float s = R(3f, 7f); Box(flora, "Semilla", p + Vector3.up * s * 0.3f, new Vector3(s * 0.6f, s * 0.55f, s), Quaternion.Euler(0f, R(0f, 180f), 0f), seed, true); }

        // 4) FAUNA de antes del fuego, a escala de la hormiga (≈ ×360 de su tamaño real, algo comprimida en los grandes):
        // pulgón 1 m, termita 1,6, mariquita 2,4, hormiga león 3, grillo y escarabajo pelotero 5, saltamontes 7, mantis 10.
        var fauna = new GameObject("Fauna_antes_del_fuego").transform; fauna.SetParent(root.transform, false);
        Material sand = Mat("Micro_Arena", new Color(0.82f, 0.72f, 0.5f)), dung = Mat("Micro_Estiercol", new Color(0.35f, 0.25f, 0.15f));
        (string sp, int n, float len)[] list = { ("Antlion", 5, 3f), ("DungBeetle", 5, 5f), ("Termite", 10, 1.6f), ("Mantis", 3, 10f), ("Cricket", 5, 5f), ("Grasshopper", 5, 7f), ("Aphid", 8, 1f), ("Ladybug", 5, 2.4f) };
        int placed = 0;
        foreach (var (sp, n, len) in list)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Animals/{sp}/{sp}.prefab");
            for (int i = 0; i < n; i++)
            {
                Vector3 p = Ring(r0, r1 - 10f);
                if (sp == "Antlion")   // su EMBUDO de arena (8–10 m): escalones que bajan hacia la larva escondida
                    for (int k = 0; k < 5; k++) Box(fauna, "Embudo_arena", p + Vector3.up * (0.02f + k * 0.02f), new Vector3(10f - k * 1.9f, 0.08f, 10f - k * 1.9f), Quaternion.Euler(0f, k * 18f, 0f), sand, false);
                if (sp == "DungBeetle") Box(fauna, "Bola_estiercol", p + new Vector3(4f, 2f, 0f), Vector3.one * 4f, Quaternion.Euler(R(0f, 90f), R(0f, 90f), 0f), dung, true);
                if (prefab == null) continue;
                GameObject a = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                a.transform.SetParent(fauna, false); a.transform.localPosition = p;
                FitLength(a, len);
                placed++;
            }
            if (prefab == null) Debug.Log($"[Microcosmos] Sin prefab {sp}: solo su rastro (embudo/bola).");
        }

        root.transform.position = o;   // todo a su origen lejano

        // Entrada (en el claro) y salida.
        var spawn = new GameObject("MobSpawnPoint"); spawn.transform.SetParent(root.transform, false);
        spawn.transform.localPosition = new Vector3(0f, 1f, -6f * StoryScale); spawn.transform.localRotation = Quaternion.LookRotation(Vector3.forward);
        spawn.AddComponent<MobSpawnPoint>();
        var portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portal.name = "WorldExitPortal"; portal.transform.SetParent(root.transform, false);
        portal.transform.localPosition = new Vector3(0f, 1.5f, -9f * StoryScale); portal.transform.localScale = new Vector3(2f, 3f, 0.3f);
        portal.GetComponent<Collider>().isTrigger = true;
        portal.GetComponent<Renderer>().sharedMaterial = Mat("MobWorldExitPortal", new Color(0.75f, 0.65f, 0.85f));
        portal.AddComponent<WorldExitPortal>().label = "Salir del microcosmos";

        // NavMesh de todo el mundo (el claro, la sabana y la enfermería gigante).
        var navGo = new GameObject("NavMesh_Microcosmos"); navGo.transform.SetParent(root.transform, false);
        var nav = navGo.AddComponent<Unity.AI.Navigation.NavMeshSurface>();
        nav.collectObjects = Unity.AI.Navigation.CollectObjects.Volume;
        nav.center = Vector3.up * 15f; nav.size = new Vector3(280f, 60f, 280f);
        nav.BuildNavMesh();
        Debug.Log($"[Microcosmos] Enfermería ×{InfirmaryScale} (el jugador = una hormiga): historia de Ambrosio ×{StoryScale} con {dressed} insectos articulados, flora a escala y {placed} animales de antes del fuego.");
    }

    // El elenco de la historia (cápsulas con Anima) pasa a ser INSECTOS ARTICULADOS: hormigas, Ambrosio el pulgón…
    static int DressCast(Transform story)
    {
        int n = 0;
        foreach (Anima a in story.GetComponentsInChildren<Anima>(true))
        {
            MeshFilter mf = a.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null || !mf.sharedMesh.name.Contains("Capsule")) continue;
            Renderer r = a.GetComponent<Renderer>();
            Color c = r != null && r.sharedMaterial != null ? r.sharedMaterial.color : new Color(0.5f, 0.4f, 0.3f);
            bool aphid = a.name.Contains("Aphid") || a.name.Contains("Ambrosio");
            CubeInsect.Dress(a.gameObject, aphid ? CubeInsect.Kind.Aphid : CubeInsect.Kind.Ant, c, aphid ? AntLength * 1.4f : AntLength);
            n++;
        }
        return n;
    }

    // Escala un prefab para que mida <len> m de largo (su lado mayor).
    static void FitLength(GameObject a, float len)
    {
        Renderer[] rs = a.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) { a.transform.localScale = Vector3.one * len; return; }
        Bounds b = rs[0].bounds; foreach (Renderer x in rs) b.Encapsulate(x.bounds);
        float size = Mathf.Max(b.size.x, b.size.z, 0.0001f);
        a.transform.localScale *= len / size;
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
