using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// AJUSTES DE RENDIMIENTO DEL PROYECTO (docs/rendimiento.md). Los ajustes de URP no están en git (Assets/Settings y ProjectSettings
/// viven en la carpeta de Unity), así que se aplican con este menú, sin reconstruir nada, y cada uno se puede deshacer.
/// - SSAO SIN PREPASO: el SSAO de PC_Renderer pedía profundidad + normales antes de lo opaco, y eso obliga a un prepaso que dibuja
///   TODA la escena una vez más. Tras lo opaco y solo con profundidad no hay prepaso; a media resolución, además, pesa la cuarta parte.
/// - SOMBRAS: 2 cascadas en vez de 4 (cada una dibuja lo que le cae dentro) y 40 m (de 50).
/// - GPU RESIDENT DRAWER: aparte, para probarlo: los MeshRenderer que quedan (cuerpos, peces) pasan a dibujarse instanciados por
///   la GPU. Exige conservar las variantes BRG, y la primera entrada en Play compila sombreadores.
/// </summary>
public static class RendimientoAjustes
{
    const string RendererPath = "Assets/Settings/PC_Renderer.asset";
    const string Menu = "Tools/Cold Sanctuary/Rendimiento/";

    static UniversalRenderPipelineAsset Pipeline => (QualitySettings.renderPipeline ?? GraphicsSettings.defaultRenderPipeline) as UniversalRenderPipelineAsset;

    static void Ssao(bool afterOpaque, int source, bool downsample)
    {
        var data = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
        ScriptableRendererFeature f = data != null ? data.rendererFeatures.FirstOrDefault(x => x != null && x.GetType().Name == "ScreenSpaceAmbientOcclusion") : null;
        if (f == null) { Debug.LogWarning($"[Rendimiento] no encuentro el SSAO en {RendererPath}."); return; }
        var so = new SerializedObject(f);
        so.FindProperty("m_Settings.AfterOpaque").boolValue = afterOpaque;
        so.FindProperty("m_Settings.Source").enumValueIndex = source;
        so.FindProperty("m_Settings.Downsample").boolValue = downsample;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(f); EditorUtility.SetDirty(data);
    }

    [MenuItem(Menu + "Aplicar ajustes ligeros (SSAO sin prepaso, 2 cascadas, 40 m)")]
    public static void ApplyLight()
    {
        UniversalRenderPipelineAsset p = Pipeline;
        if (p == null) { Debug.LogError("[Rendimiento] el pipeline activo no es URP."); return; }
        Debug.Log($"[Rendimiento] antes: {p.name}, cascadas {p.shadowCascadeCount}, sombra a {p.shadowDistance} m.");
        Ssao(true, 0, true);
        p.shadowCascadeCount = 2;
        p.shadowDistance = 40f;
        EditorUtility.SetDirty(p);
        AssetDatabase.SaveAssets();
        Debug.Log("[Rendimiento] ajustes ligeros aplicados: SSAO tras lo opaco, de profundidad y a media resolución; 2 cascadas; sombra a 40 m.");
    }

    [MenuItem(Menu + "Restaurar ajustes ligeros (como estaban)")]
    public static void RestoreLight()
    {
        UniversalRenderPipelineAsset p = Pipeline;
        if (p == null) return;
        Ssao(false, 1, false);
        p.shadowCascadeCount = 4;
        p.shadowDistance = 50f;
        EditorUtility.SetDirty(p);
        AssetDatabase.SaveAssets();
        Debug.Log("[Rendimiento] ajustes ligeros restaurados: SSAO de profundidad y normales, 4 cascadas, sombra a 50 m.");
    }

    [MenuItem(Menu + "Activar GPU Resident Drawer (probar)")]
    public static void EnableResidentDrawer() => ResidentDrawer(true);

    [MenuItem(Menu + "Desactivar GPU Resident Drawer")]
    public static void DisableResidentDrawer() => ResidentDrawer(false);

    static void ResidentDrawer(bool on)
    {
        UniversalRenderPipelineAsset p = Pipeline;
        if (p == null) return;
        if (on && EditorGraphicsSettings.batchRendererGroupShaderStrippingMode != BatchRendererGroupStrippingMode.KeepAll)
        {
            var so = new SerializedObject(GraphicsSettings.GetGraphicsSettings());   // (la propiedad es de solo lectura)
            so.FindProperty("m_BrgStripping").enumValueFlag = (int)BatchRendererGroupStrippingMode.KeepAll;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        p.gpuResidentDrawerMode = on ? GPUResidentDrawerMode.InstancedDrawing : GPUResidentDrawerMode.Disabled;
        EditorUtility.SetDirty(p);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Rendimiento] GPU Resident Drawer {(on ? "activado" : "desactivado")}: {p.gpuResidentDrawerMode}.");
    }

    [MenuItem(Menu + "Medir A y B (con y sin lotes)")]
    public static void MeasureAB()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Rendimiento] la medida es en Play."); return; }
        (PerfProbe.Instance != null ? PerfProbe.Instance : new GameObject("PerfProbe").AddComponent<PerfProbe>()).RunAB();
    }

    [MenuItem(Menu + "Lotes estáticos: encender o apagar en este equipo")]
    public static void ToggleLote()
    {
        bool on = !LoteEstatico.Allowed;
        PlayerPrefs.SetInt(LoteEstatico.PrefKey, on ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"[Rendimiento] lotes estáticos {(on ? "encendidos" : "apagados")} (desde la próxima escena que se cargue).");
    }
}
