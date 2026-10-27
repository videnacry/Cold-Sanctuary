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
            if (SampleSceneBuilder.MicroSceneName(a.key) == MicrocosmosSceneBuilder.SceneName0) continue;   // el de la Sala de Yoga (el río) lo hace BuildScene0
            Vector3 o = new Vector3(6000f + i * 400f, 0f, 6000f);   // no solapa el mundo base ni a Mesopotamia (5000)
            Build(SampleSceneBuilder.MicroSceneName(a.key), a.display, a.tint, a.kind, o, a.areaGo);
            i++;
        }
        Debug.Log($"[Microcosmos] {i} escenas de microcosmos por área generadas.");
    }

    // La fecha y la hora del microcosmos de cada área (su época está en Eras.OfMicrocosmos).
    static readonly Dictionary<string, (string date, float hour)> AreaDates = new Dictionary<string, (string, float)>
    {
        ["Cocina"] = ("otoño, la era del fuego (las reencarnaciones)", 19f),         // la brasa de la tribu, al anochecer
        ["Huerto"] = ("primavera, los primeros cultivos", 9f),
        [AreaWork.Hospital] = ("verano corto de la taiga, antes del fuego", 5.5f),   // el alba de Ambrosio
        ["Estudio Textil"] = ("otoño, Paleolítico", 15f),
        ["Mecánica"] = ("verano, Paleolítico", 11f),
    };

    static void Build(string sceneName, string display, Color tint, SampleSceneBuilder.MissionKind kind, Vector3 o, string areaGo)
    {
        string path = $"Assets/Scenes/{sceneName}.unity";
        DropUntitled();
        Scene prev = EditorSceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);

        // La PLANTILLA: la fecha y la hora de cada área (su época, AreaDates) y la plataforma (el área) a la que pertenece.
        var (date, hour) = AreaDates.TryGetValue(display, out var dh) ? dh : ("la época de la tribu", 10f);
        var spec = new MicrocosmSpec(sceneName, display, date, hour, areaGo) { tint = tint, kind = kind };
        var sun = new GameObject("Sun");
        sun.transform.position = o + Vector3.up * 10f;
        Light l = sun.AddComponent<Light>(); l.type = LightType.Directional;
        MicrocosmTemplate.Sun(l, hour, display);

        // ENFERMERÍA: su microcosmos es la historia de AMBROSIO (su muerte camino de la cueva), en un mundo extenso de antes
        // del fuego, dentro de la enfermería vista a escala de insecto (×10: el jugador es una hormiga).
        // TODAS las áreas: el área gigante, poblada por insectos a escala humana y MIRMIDONES (hormigas humanas), con su historia.
        AreaMicrocosmos.Build(areaGo, display, kind, tint, o, null, spec); MicroWater.SurroundScene(scene); Finish(scene, path, prev); return;
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


    /// <summary>Una escena de microcosmos con HISTORIA PROPIA (compasión, mitos…) sobre un área del santuario ×10, según la
    /// PLANTILLA (<see cref="MicrocosmTemplate"/>): su fecha, su hora (el sol) y la plataforma a la que pertenece.</summary>
    internal static void BuildStoryScene(MicrocosmSpec s, Vector3 o)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        string path = $"Assets/Scenes/{s.scene}.unity";
        DropUntitled();
        Scene prev = EditorSceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            var sun = new GameObject("Sun");
            sun.transform.position = o + Vector3.up * 10f;
            Light l = sun.AddComponent<Light>(); l.type = LightType.Directional;
            MicrocosmTemplate.Sun(l, s.hour, s.display);
            AreaMicrocosmos.Build(s.platform, s.display, s.kind, s.tint, o, s.story, s);
        }
        catch
        {
            // Si la historia falla a medias, la escena nueva (sin título y sin guardar) se cierra: si se quedara abierta,
            // el siguiente NewScene aditivo lanzaría «Cannot create a new scene additively with an untitled scene unsaved».
            EditorSceneManager.CloseScene(scene, removeScene: true);
            if (prev.IsValid()) SceneManager.SetActiveScene(prev);
            throw;
        }
        Finish(scene, path, prev);
    }

    /// <summary>Cierra las escenas SIN TÍTULO que haya dejado abiertas un constructor que falló (con ellas Unity no deja
    /// crear otra escena aditiva). Si es la única escena cargada, reabre la principal.</summary>
    internal static void DropUntitled()
    {
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (!string.IsNullOrEmpty(s.path)) continue;
            if (SceneManager.sceneCount > 1) { Debug.LogWarning("[Microcosmos] Cierro una escena sin título que quedó abierta."); EditorSceneManager.CloseScene(s, removeScene: true); }
            else if (System.IO.File.Exists("Assets/Scenes/SampleScene.unity")) EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);
        }
        if (!SceneManager.GetActiveScene().IsValid() || string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            for (int i = 0; i < SceneManager.sceneCount; i++) if (!string.IsNullOrEmpty(SceneManager.GetSceneAt(i).path)) { SceneManager.SetActiveScene(SceneManager.GetSceneAt(i)); break; }
    }

    internal static void Finish(Scene scene, string path, Scene prev)
    {
        MicroWater.SurroundScene(scene);
        EditorSceneManager.SaveScene(scene, path);
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(s => s.path == path)) { scenes.Add(new EditorBuildSettingsScene(path, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
        EditorSceneManager.CloseScene(scene, removeScene: true);
        if (prev.IsValid()) SceneManager.SetActiveScene(prev);
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
