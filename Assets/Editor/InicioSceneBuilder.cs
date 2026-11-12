using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ESCENA INICIO (Berón 2026-10-30: «una escena Inicio que solo tenga esa pantalla y cargue una u otra escena según lo
/// elegido»). Assets/Scenes/Inicio.unity: una cámara, una luz y la <see cref="StartScreen"/>. Va la PRIMERA de Build Settings
/// (índice 0) y se añaden las escenas de Assets/Scenes que faltaran, para que todas se puedan elegir. Los demás constructores
/// solo añaden al final de la lista, así que Inicio no se mueve; aun así, al recompilar y cada vez que cambia la lista se
/// comprueba que siga la primera. Con Play desde SampleScene (las pruebas, los menús) nada cambia: el editor juega la escena abierta.
/// </summary>
[InitializeOnLoad]
public static class InicioSceneBuilder
{
    const string ScenePath = "Assets/Scenes/Inicio.unity";
    // Escenas que ya no genera nadie y que la Sala de Eras saca de la lista a propósito (los archivos se quedan).
    static readonly string[] Retired = { "Assets/Scenes/Microcosmos_Enfermeria.unity", "Assets/Scenes/Microcosmos_Veterinaria.unity" };

    static InicioSceneBuilder()
    {
        EditorApplication.delayCall += () => KeepFirst(false);
        EditorBuildSettings.sceneListChanged += () => EditorApplication.delayCall += () => KeepFirst(false);
    }

    [MenuItem("Tools/Cold Sanctuary/Build Inicio (pantalla de inicio y lista de escenas)")]
    public static void Build()
    {
        MicrocosmosAreaSceneBuilder.DropUntitled();
        Scene prev = EditorSceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);

        var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.07f, 0.09f, 0.13f);
        camGo.AddComponent<AudioListener>();
        var sun = new GameObject("Sol").AddComponent<Light>();
        sun.type = LightType.Directional; sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        new GameObject("Inicio").AddComponent<StartScreen>();

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorSceneManager.CloseScene(scene, removeScene: true);
        if (prev.IsValid()) SceneManager.SetActiveScene(prev);
        KeepFirst(true);
        Debug.Log($"[Inicio] {ScenePath} lista: primera de Build Settings ({EditorBuildSettings.scenes.Length} escenas).");
    }

    /// <summary>Inicio en el índice 0; con <paramref name="addMissing"/>, además las escenas de Assets/Scenes que no estén.</summary>
    static void KeepFirst(bool addMissing)
    {
        if (!System.IO.File.Exists(ScenePath)) return;
        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool first = list.Count > 0 && list[0].path == ScenePath && list[0].enabled;
        int added = 0;
        if (addMissing)
            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" }))
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                if (p == ScenePath || System.Array.IndexOf(Retired, p) >= 0 || list.Exists(s => s.path == p)) continue;
                list.Add(new EditorBuildSettingsScene(p, true));
                added++;
                Debug.Log($"[Inicio] añadida a Build Settings: {p}");
            }
        if (first && added == 0) return;
        list.RemoveAll(s => s.path == ScenePath);
        list.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = list.ToArray();
        AssetDatabase.SaveAssets();   // que la lista quede escrita en ProjectSettings ya
        if (!first) Debug.Log("[Inicio] Inicio vuelve a ser la primera escena de Build Settings.");
    }
}
