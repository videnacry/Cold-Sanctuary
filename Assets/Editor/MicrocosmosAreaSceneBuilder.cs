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

    static Material Mat(string name, Color c)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(sh) { name = name, color = c };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        return m;
    }
}
