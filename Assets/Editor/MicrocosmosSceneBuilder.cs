using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Genera la ESCENA propia del MICROCOSMOS **Scene1 (Ambrosio / el alba)** — el nivel introductorio de la cueva y el
/// pulgón (docs/microcosmos-level1.md, microcosmos-insects.md §13). El microcosmos es su PROPIO plano: sus niveles son
/// escenas separadas, NO objetos dentro de la escena del mesocosmos (Santuario 1). Scene1 (Ambrosio, anterior en la
/// historia) y <see cref="MobWorldSceneBuilder"/> (Mesopotamia, la ciudad-insecto) son escenas **HERMANAS** del mismo
/// microcosmos. Escenas-desde-código: la .unity no se versiona (como SampleScene/Mesopotamia) pero se regenera aquí.
///
/// Reúne el contenido ya existente (`SampleSceneBuilder.BuildMicrocosmosSandbox` = hormiguero/pulgón-guía/familia caída;
/// `BuildNivel1Sandbox` = mapa abierto con hormigas + depredadores + hechizos de Kushal) bajo un root en su propia escena,
/// con luz y NavMesh horneado. Se añade a Build Settings para cargarla por nombre en runtime (additive sobre el jugador).
///
/// Uso: Tools → Cold Sanctuary → Build Microcosmos Scene1 (Ambrosio). También la regenera "Build Sample Scene Blockout".
/// Carga en runtime: `MobWorldLoader.Instance.EnterMobWorld("Microcosmos_Scene1_Ambrosio")` (genérico, no solo mob) →
/// teletransporta al jugador al `MobSpawnPoint`; el `YogaPortal` lo devuelve. Falta cablear QUÉ dispara la entrada (un
/// trigger del prólogo/alba); el resto (offset + spawn + portal) ya está resuelto aquí.
/// </summary>
public static class MicrocosmosSceneBuilder
{
    public const string SceneName = "Microcosmos_Scene1_Ambrosio";
    const string ScenePath = "Assets/Scenes/" + SceneName + ".unity";

    // Origen LEJANO y DISTINTO del de Mesopotamia (5000,0,5000): así, cargada additive sobre el mundo base (o junto a la
    // otra escena hermana), el contenido —autorado en coords base— no solapa a nadie. Todo el root se desplaza aquí.
    static readonly Vector3 O = new Vector3(-5000f, 0f, 5000f);

    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos Scene1 (Ambrosio)")]
    public static void BuildScene1()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        Scene prev  = EditorSceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);   // los GameObjects nuevos caen en esta escena

        // Luz direccional (el alba). Para una luz direccional la posición es irrelevante (solo cuenta la rotación).
        GameObject lightGO = new GameObject("Sun");
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.95f, 0.85f);   // luz cálida de amanecer

        // Contenido del microcosmos Nivel 1 (reutiliza los builders existentes; ahora viven en SU escena). Se construye
        // con el root en el ORIGEN (los builders fijan posiciones en coords base) y luego se DESPLAZA el root entero a O
        // → todo el contenido se mueve coherentemente sin tocar los builders. Las posiciones autoradas quedan como
        // offsets locales respecto de O.
        GameObject root = new GameObject("Microcosmos_Scene1_AUTO");
        SampleSceneBuilder.BuildMicrocosmosSandbox(root.transform);   // cueva/pulgón-guía/familia caída (el tableau del alba)
        SampleSceneBuilder.BuildNivel1Sandbox(root.transform);        // mapa abierto: hormigas + depredadores + hechizos de Kushal
        root.transform.position = O;                                 // desplaza TODO a un origen lejano (no solapa el mundo base)

        // Entrada/salida del jugador (genérico vía MobWorldLoader, como Mesopotamia): teletransporta al MobSpawnPoint al
        // entrar; el YogaPortal devuelve al mundo normal. Ambos parentados al root → caen ya en O.
        GameObject spawn = new GameObject("MobSpawnPoint");
        spawn.transform.SetParent(root.transform);
        spawn.transform.localPosition = new Vector3(0f, 1f, -6f);
        spawn.transform.localRotation = Quaternion.LookRotation(Vector3.forward);
        spawn.AddComponent<MobSpawnPoint>();

        GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portal.name = "YogaPortal";
        portal.transform.SetParent(root.transform);
        portal.transform.localPosition = new Vector3(0f, 1f, -5f);
        portal.transform.localScale = new Vector3(1.5f, 2f, 0.3f);
        portal.GetComponent<Collider>().isTrigger = true;
        portal.GetComponent<Renderer>().sharedMaterial = MakeMat("MicroYogaPortal", new Color(0.75f, 0.65f, 0.85f));
        portal.AddComponent<YogaPortal>();

        BakeMicroNavMesh();   // tras el offset → el NavMesh se hornea en O (donde están de verdad las hormigas)

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings(ScenePath);
        EditorSceneManager.CloseScene(scene, removeScene: true);
        if (prev.IsValid()) SceneManager.SetActiveScene(prev);

        Debug.Log($"[MicrocosmosSceneBuilder] Escena guardada en {ScenePath} y añadida a Build Settings " +
                  $"(hermana de {MobWorldSceneBuilder.SceneName}).");
    }

    // Hornea el NavMesh sobre el suelo del bosque (ForestFloor lo crea BuildNivel1Sandbox), para que las hormigas naveguen.
    static void BakeMicroNavMesh()
    {
        GameObject floor = GameObject.Find("ForestFloor");
        if (floor == null) return;
        NavMeshSurface surface = floor.GetComponent<NavMeshSurface>();
        if (surface == null) surface = floor.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;
        surface.BuildNavMesh();
    }

    static Material MakeMat(string name, Color c)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        return new Material(shader) { name = name, color = c };
    }

    static void AddToBuildSettings(string path)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Exists(s => s.path == path)) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
