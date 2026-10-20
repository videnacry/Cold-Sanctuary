using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ESCENAS DE VIRTUALIZACIÓN, UNA POR MISIÓN (VirtualMissions). Cada misión de trabajo tiene su escena «Virtual_&lt;clave&gt;»:
/// suelo con el color del área, sus PASOS como puestos numerados en arco delante del punto de aparición (VirtualMissionStation),
/// el que lleva la misión (VirtualMissionRunner) y el portal de salida. La de iniciación de la enfermería trae además al
/// TURISTA, que sigue al jugador, y un tanque de virtualización como último paso. Se cargan en aditivo con MobWorldLoader
/// desde la máquina de cada área (opción 1 · Virtualización).
/// </summary>
public static class VirtualMissionSceneBuilder
{
    [MenuItem("Tools/Cold Sanctuary/Build escenas de virtualización (una por misión)")]
    public static void BuildAll()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        var seen = new HashSet<string>();
        int i = 0;
        foreach (var (key, area, title) in VirtualMissions.All())
        {
            if (!seen.Add(VirtualMissions.Key(key))) continue;   // las clases de yoga de corrector comparten pasos, no escena
            Build(VirtualMissions.Key(key), area, title, new Vector3(9000f + i * 150f, 0f, 9000f));
            i++;
        }
        Debug.Log($"[Virtualización] {i} escenas de misión generadas. Listo — escenas de virtualización.");
    }

    static void Build(string key, string area, string title, Vector3 o)
    {
        string sceneName = "Virtual_" + key, path = $"Assets/Scenes/{sceneName}.unity";
        Color tint = VirtualMissions.Tint(area);
        Scene prev = EditorSceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);

        var sun = new GameObject("Sun");
        sun.transform.SetPositionAndRotation(o + Vector3.up * 10f, Quaternion.Euler(50f, -30f, 0f));
        sun.AddComponent<Light>().type = LightType.Directional;

        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground"; ground.transform.position = o; ground.transform.localScale = new Vector3(3f, 1f, 3f);
        ground.GetComponent<Renderer>().sharedMaterial = Mat($"Virtual_{area}_Suelo", Color.Lerp(tint, Color.white, 0.45f));

        var spawn = new GameObject("MobSpawnPoint");
        spawn.transform.SetPositionAndRotation(o + new Vector3(0f, 1f, -8f), Quaternion.LookRotation(Vector3.forward));
        spawn.AddComponent<MobSpawnPoint>();

        var portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portal.name = "WorldExitPortal";
        portal.transform.position = o + new Vector3(0f, 1f, -11f);
        portal.transform.localScale = new Vector3(1.5f, 2f, 0.3f);
        portal.GetComponent<Collider>().isTrigger = true;
        portal.GetComponent<Renderer>().sharedMaterial = Mat("MobWorldExitPortal", new Color(0.75f, 0.65f, 0.85f));
        portal.AddComponent<WorldExitPortal>().label = "Salir de la virtualización";

        var root = new GameObject($"Virtualizacion_{key}");
        root.transform.position = o;
        var runner = root.AddComponent<VirtualMissionRunner>();
        runner.key = key; runner.title = title;
        WorldLabel tl = root.AddComponent<WorldLabel>(); tl.text = $"{area} · {title}"; tl.height = 4f;

        // SERVICIO DEL RESTAURANTE: escena propia de juego (cocinero, pedidos por número, turistas que comen y se van).
        if (key == "coc_servicio")
        {
            Object.DestroyImmediate(runner);
            BuildRestaurantService(root.transform, o);
            Save(scene, path, prev);
            return;
        }

        // Los pasos en arco (de izquierda a derecha) delante del jugador.
        string[] steps = VirtualMissions.Steps(key);
        var stations = new List<VirtualMissionStation>();
        Material stMat = Mat($"Virtual_{area}_Puesto", tint), tankMat = Mat("Virtual_Tanque", new Color(0.3f, 0.65f, 0.9f, 1f));
        for (int s = 0; s < steps.Length; s++)
        {
            float a = Mathf.Lerp(-60f, 60f, steps.Length > 1 ? s / (float)(steps.Length - 1) : 0.5f) * Mathf.Deg2Rad;
            Vector3 p = o + new Vector3(Mathf.Sin(a) * 7f, 0f, -1f + Mathf.Cos(a) * 7f);
            bool tank = key == "enf_iniciacion" && s == steps.Length - 1;   // la primera virtualización del turista: un tanque
            var st = GameObject.CreatePrimitive(PrimitiveType.Cube);
            st.name = $"Paso_{s + 1}";
            st.transform.SetParent(root.transform, true);
            st.transform.position = p + Vector3.up * (tank ? 1.2f : 0.5f);
            st.transform.localScale = tank ? new Vector3(1.8f, 2.4f, 1.8f) : new Vector3(1.2f, 1f, 1.2f);
            st.GetComponent<Renderer>().sharedMaterial = tank ? tankMat : stMat;
            var vs = st.AddComponent<VirtualMissionStation>();
            vs.runner = runner; vs.step = steps[s];
            WorldLabel wl = st.AddComponent<WorldLabel>(); wl.text = $"{s + 1} · {steps[s]}"; wl.height = tank ? 1.6f : 1.1f;
            stations.Add(vs);
        }
        runner.stations = stations.ToArray();

        // INICIACIÓN: el turista recién llegado, que te sigue de paso en paso.
        if (key == "enf_iniciacion")
        {
            var tourist = new GameObject("Turista");
            tourist.transform.SetParent(root.transform, true);
            tourist.transform.position = o + new Vector3(1.5f, 0f, -7f);
            Material shirt = Mat("Virtual_Turista_Camisa", new Color(0.95f, 0.6f, 0.3f)), skin = Mat("Virtual_Turista_Piel", new Color(0.92f, 0.76f, 0.62f));
            Part(tourist.transform, new Vector3(0f, 0.45f, 0f), new Vector3(0.35f, 0.9f, 0.3f), Mat("Virtual_Turista_Pantalon", new Color(0.3f, 0.35f, 0.5f)));
            Part(tourist.transform, new Vector3(0f, 1.25f, 0f), new Vector3(0.5f, 0.7f, 0.3f), shirt);
            Part(tourist.transform, new Vector3(0f, 1.8f, 0f), new Vector3(0.3f, 0.3f, 0.3f), skin);
            tourist.AddComponent<VirtualFollower>();
            WorldLabel twl = tourist.AddComponent<WorldLabel>(); twl.text = "Turista"; twl.height = 2.2f;
        }

        Save(scene, path, prev);
    }

    static void Save(Scene scene, string path, Scene prev)
    {
        EditorSceneManager.SaveScene(scene, path);
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(sc => sc.path == path)) { scenes.Add(new EditorBuildSettingsScene(path, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
        EditorSceneManager.CloseScene(scene, removeScene: true);
        if (prev.IsValid()) SceneManager.SetActiveScene(prev);
    }

    // RESTAURANTE VIRTUAL: la cocina delante del jugador (fogón y pase con campana), la cola de turistas junto al pase, seis
    // mesas con su silla detrás y la entrada/salida a un lado.
    static void BuildRestaurantService(Transform root, Vector3 o)
    {
        Material steel = Mat("Virtual_Acero", new Color(0.72f, 0.74f, 0.76f)), wood = Mat("Virtual_Madera", new Color(0.55f, 0.4f, 0.28f));
        Material fire = Mat("Virtual_Fuego", new Color(1f, 0.45f, 0.15f)), floor = Mat("Virtual_Comedor", new Color(0.85f, 0.8f, 0.72f));
        Block(root, "Suelo_comedor", o + new Vector3(0f, 0.01f, 5f), new Vector3(22f, 0.02f, 16f), floor, false);

        // Fogón (F: cocinar el pedido más antiguo).
        Transform stove = Block(root, "Fogon", o + new Vector3(-3f, 0.5f, -3.5f), new Vector3(2.4f, 1f, 1f), steel, true);
        Block(stove, "Llama", Vector3.zero, Vector3.zero, fire, false).localPosition = new Vector3(0f, 0.55f, 0f);
        stove.Find("Llama").localScale = new Vector3(0.4f, 0.1f, 0.4f);
        Transform pass = Block(root, "Pase", o + new Vector3(1.5f, 0.5f, -2f), new Vector3(4f, 1f, 0.8f), steel, true);
        Transform bell = Block(root, "Campana", o + new Vector3(3.9f, 0.6f, -2f), new Vector3(0.5f, 1.2f, 0.5f), Mat("Virtual_Laton", new Color(0.85f, 0.7f, 0.3f)), true);
        Transform passMark = new GameObject("Pase_frente").transform; passMark.SetParent(root, true);
        passMark.SetPositionAndRotation(o + new Vector3(1.5f, 0f, -2f), Quaternion.LookRotation(Vector3.forward));

        var game = root.gameObject.AddComponent<RestaurantServiceGame>();
        game.pass = passMark;
        game.entrance = Marker(root, "Entrada", o + new Vector3(10f, 0f, 2f));
        game.exit = Marker(root, "Salida", o + new Vector3(10f, 0f, 9f));
        game.waiting = Marker(root, "Cola", o + new Vector3(2f, 0f, 0.6f));
        game.shirts = new[] { Mat("Virtual_Turista_Camisa", new Color(0.95f, 0.6f, 0.3f)), Mat("Virtual_Turista_Camisa2", new Color(0.3f, 0.6f, 0.9f)), Mat("Virtual_Turista_Camisa3", new Color(0.55f, 0.8f, 0.45f)), Mat("Virtual_Turista_Camisa4", new Color(0.85f, 0.45f, 0.7f)) };
        game.skin = Mat("Virtual_Turista_Piel", new Color(0.92f, 0.76f, 0.62f));
        game.plateMat = Mat("Virtual_Plato", Color.white);
        game.foodMat = Mat("Virtual_Comida", new Color(0.8f, 0.45f, 0.25f));

        foreach (var (t, isBell, label) in new[] { (stove, false, "Fogón (F: cocinar)"), (bell, true, "Campana (F: llamar por número)") })
        {
            var st = t.gameObject.AddComponent<RestaurantServiceStation>(); st.game = game; st.bell = isBell;
            WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = label; wl.height = 1.2f;
        }
        WorldLabel pl = pass.gameObject.AddComponent<WorldLabel>(); pl.text = "Pase"; pl.height = 1f;

        // Seis mesas (2 filas × 3), cada una con su silla (el asiento es el punto donde se sienta el turista).
        var seats = new List<Transform>();
        for (int r = 0; r < 2; r++)
            for (int c = 0; c < 3; c++)
            {
                Vector3 tp = o + new Vector3(-5f + c * 5f, 0f, 5f + r * 4.5f);
                Block(root, $"Mesa_{seats.Count + 1}", tp + new Vector3(0f, 0.38f, 0f), new Vector3(1.2f, 0.06f, 1.2f), wood, true);
                Block(root, "Pata", tp + new Vector3(0f, 0.18f, 0f), new Vector3(0.12f, 0.36f, 0.12f), wood, false);
                Block(root, "Silla", tp + new Vector3(0f, 0.22f, -0.95f), new Vector3(0.5f, 0.44f, 0.5f), wood, false);
                Transform seat = new GameObject($"Asiento_{seats.Count + 1}").transform; seat.SetParent(root, true);
                seat.SetPositionAndRotation(tp + new Vector3(0f, 0f, -0.95f), Quaternion.LookRotation(Vector3.forward));
                seats.Add(seat);
            }
        game.seats = seats.ToArray();
        Debug.Log("[Virtualización] Restaurante virtual: fogón, pase con campana, 6 mesas y turistas con número de pedido.");
    }

    static Transform Block(Transform parent, string name, Vector3 pos, Vector3 size, Material m, bool solid)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name; g.transform.SetParent(parent, true);
        g.transform.position = pos; g.transform.localScale = size;
        if (!solid) Object.DestroyImmediate(g.GetComponent<Collider>());
        g.GetComponent<Renderer>().sharedMaterial = m;
        return g.transform;
    }

    static Transform Marker(Transform parent, string name, Vector3 pos)
    {
        Transform t = new GameObject(name).transform; t.SetParent(parent, true); t.position = pos; return t;
    }

    static void Part(Transform parent, Vector3 local, Vector3 size, Material m)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = local; g.transform.localScale = size;
        Object.DestroyImmediate(g.GetComponent<Collider>());
        g.GetComponent<Renderer>().sharedMaterial = m;
    }

    static readonly Dictionary<string, Material> _mats = new Dictionary<string, Material>();
    static Material Mat(string name, Color c)
    {
        if (_mats.TryGetValue(name, out Material cached) && cached != null) return cached;
        Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(sh) { name = name, color = c };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        return _mats[name] = m;
    }
}
