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
/// ⚠ Al cargarse additive en runtime junto al mundo base puede querer un OFFSET de posición (el contenido está autorado
///   en coords base); pendiente si se solapa con otra escena cargada a la vez.
/// </summary>
public static class MicrocosmosSceneBuilder
{
    public const string SceneName = "Microcosmos_Scene1_Ambrosio";
    const string ScenePath = "Assets/Scenes/" + SceneName + ".unity";

    [MenuItem("Tools/Cold Sanctuary/Build Microcosmos Scene1 (Ambrosio)")]
    public static void BuildScene1()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        Scene prev  = EditorSceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);   // los GameObjects nuevos caen en esta escena

        // Luz direccional (el alba).
        GameObject lightGO = new GameObject("Sun");
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.95f, 0.85f);   // luz cálida de amanecer

        // Contenido del microcosmos Nivel 1 (reutiliza los builders existentes; ahora viven en SU escena).
        GameObject root = new GameObject("Microcosmos_Scene1_AUTO");
        SampleSceneBuilder.BuildMicrocosmosSandbox(root.transform);   // cueva/pulgón-guía/familia caída (el tableau del alba)
        SampleSceneBuilder.BuildNivel1Sandbox(root.transform);        // mapa abierto: hormigas + depredadores + hechizos de Kushal

        BakeMicroNavMesh();

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

    static void AddToBuildSettings(string path)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Exists(s => s.path == path)) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
