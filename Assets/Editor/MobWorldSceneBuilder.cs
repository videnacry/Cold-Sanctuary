using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Genera la ESCENA propia del mundo mob (docs mob-world-architecture §13). Escenas-desde-código:
/// la escena no se versiona (como SampleScene) pero se regenera con esta herramienta.
///
/// Construye la ciudad-insecto = Mesopotamia del amanecer a un ORIGEN LEJANO (no solapa el mundo
/// base, que sigue cargado en aditivo) con: suelo, luz, MobSpawnPoint, 3 chozas + anclas y el
/// YogaPortal de salida. La añade a Build Settings para poder cargarla por nombre en runtime.
///
/// Uso: Tools → Cold Sanctuary → Build MobWorld Mesopotamia. Luego pon "MobWorld_Mesopotamia" en el
/// campo mobWorldSceneName de la VirtualizationMachine de la cocina.
/// </summary>
public static class MobWorldSceneBuilder
{
    public const string SceneName = "MobWorld_Mesopotamia";
    const string ScenePath = "Assets/Scenes/" + SceneName + ".unity";

    // Origen lejano para que el mundo mob no solape el mundo base (ambos cargados en aditivo).
    static readonly Vector3 O = new Vector3(5000f, 0f, 5000f);

    [MenuItem("Tools/Cold Sanctuary/Build MobWorld Mesopotamia")]
    public static void BuildMesopotamia()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        Scene prev  = EditorSceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene); // los GameObjects nuevos caen en esta escena

        // Luz direccional.
        var lightGO = new GameObject("Sun");
        lightGO.transform.position = O + Vector3.up * 10f;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;

        // Suelo (~40x40) con collider para el CharacterController del jugador.
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position   = O;
        ground.transform.localScale = new Vector3(4f, 1f, 4f);
        ground.GetComponent<Renderer>().sharedMaterial = MakeMat("MobGround", new Color(0.80f, 0.72f, 0.55f));

        // Spawn del jugador (junto al portal, mirando hacia la ciudad).
        var spawnGO = new GameObject("MobSpawnPoint");
        spawnGO.transform.SetPositionAndRotation(O + new Vector3(0f, 1f, -6f), Quaternion.LookRotation(Vector3.forward));
        spawnGO.AddComponent<MobSpawnPoint>();

        // Chozas de adobe + anclas (estructura representativa de la era).
        Color mud = new Color(0.72f, 0.55f, 0.36f);
        AddResidentWithHut("Guardián del Fuego", "El Hogar",   O + new Vector3(0f, 1f, 0f),  mud, new Color(0.95f, 0.50f, 0.20f));
        AddResidentWithHut("El Tallador",        "La Fragua",  O + new Vector3(4f, 1f, 2f),  mud, new Color(0.60f, 0.60f, 0.65f));
        AddResidentWithHut("La Recolectora",     "El Granero", O + new Vector3(-4f, 1f, 2f), mud, new Color(0.50f, 0.70f, 0.35f));

        // Yoga-portal de salida.
        var portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portal.name = "YogaPortal";
        portal.transform.position   = O + new Vector3(0f, 1f, -5f);
        portal.transform.localScale = new Vector3(1.5f, 2f, 0.3f);
        portal.GetComponent<Collider>().isTrigger = true;
        portal.GetComponent<Renderer>().sharedMaterial = MakeMat("MobYogaPortal", new Color(0.75f, 0.65f, 0.85f));
        portal.AddComponent<YogaPortal>();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings(ScenePath);
        EditorSceneManager.CloseScene(scene, removeScene: true);
        if (prev.IsValid()) SceneManager.SetActiveScene(prev);

        Debug.Log($"[MobWorldSceneBuilder] Escena guardada en {ScenePath} y añadida a Build Settings. " +
                  $"Pon '{SceneName}' en 'mobWorldSceneName' de la VirtualizationMachine de la cocina.");
    }

    static void AddResidentWithHut(string residentName, string role, Vector3 pos, Color hutColor, Color bodyColor)
    {
        var hut = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hut.name = $"Hut_{residentName}";
        hut.transform.position   = pos;
        hut.transform.localScale = new Vector3(2.2f, 2f, 2.2f);
        hut.GetComponent<Renderer>().sharedMaterial = MakeMat($"Hut_{residentName}", hutColor);

        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = residentName;
        go.transform.position = pos + new Vector3(0f, 0f, 1.6f); // frente a la choza
        go.GetComponent<Renderer>().sharedMaterial = MakeMat(residentName, bodyColor);
        go.GetComponent<Collider>().isTrigger = true;
        var res = go.AddComponent<MobResident>();
        res.residentName = residentName;
        res.role = role;
    }

    static Material MakeMat(string name, Color c)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        return new Material(shader) { name = name, color = c };
    }

    static void AddToBuildSettings(string path)
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Exists(s => s.path == path)) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
