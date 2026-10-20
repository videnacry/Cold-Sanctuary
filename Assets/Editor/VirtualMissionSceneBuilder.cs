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

        EditorSceneManager.SaveScene(scene, path);
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(sc => sc.path == path)) { scenes.Add(new EditorBuildSettingsScene(path, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
        EditorSceneManager.CloseScene(scene, removeScene: true);
        if (prev.IsValid()) SceneManager.SetActiveScene(prev);
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
