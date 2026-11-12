using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// SOMBRAS BARATAS (Berón 2026-11-09, rendimiento). Un solo interruptor, <see cref="Cheap"/> (por defecto, encendido):
///  · la luz principal (el sol) sigue haciendo sombra, hasta <see cref="MaxDistance"/> m y con 1 cascada;
///  · ninguna otra luz hace sombra;
///  · no hacen sombra las piezas pequeñas (menos de <see cref="SmallSize"/> m: detalles de muebles, utensilios, el pelo y las piezas de un
///    animal); de cada animal, solo su pieza más grande.
/// El ajuste del URP se hace sobre una COPIA del asset (en el editor, cambiar el asset en juego lo guardaría en el proyecto).
/// </summary>
public static class ShadowQuality
{
    public static bool Cheap = true;
    public const float MaxDistance = 60f, SmallSize = 0.4f;
    public const int Cascades = 1;
    public static int RenderersOff { get; private set; }
    public static int LightsOff { get; private set; }
    static bool _pipelineDone;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnLoad()
    {
        SceneManager.sceneLoaded -= Loaded;
        SceneManager.sceneLoaded += Loaded;
        for (int i = 0; i < SceneManager.sceneCount; i++) Apply(SceneManager.GetSceneAt(i));
    }

    static void Loaded(Scene s, LoadSceneMode m) => Apply(s);

    public static void Apply(Scene s)
    {
        if (!Cheap || !s.IsValid() || !s.isLoaded) return;
        Pipeline();
        Light sun = RenderSettings.sun;
        int lights = 0, rends = 0;
        foreach (GameObject root in s.GetRootGameObjects())
        {
            foreach (Light l in root.GetComponentsInChildren<Light>(true))
            {
                if (l == null || l.shadows == LightShadows.None) continue;
                bool main = l == sun || (sun == null && l.type == LightType.Directional);
                if (!main) { l.shadows = LightShadows.None; lights++; }
            }
            foreach (Animal a in root.GetComponentsInChildren<Animal>(true))
            {
                Renderer biggest = null; float bv = -1f;
                foreach (Renderer r in a.GetComponentsInChildren<Renderer>(true)) { float v = r.bounds.size.sqrMagnitude; if (v > bv) { bv = v; biggest = r; } }
                foreach (Renderer r in a.GetComponentsInChildren<Renderer>(true))
                    if (r != biggest && r.shadowCastingMode != ShadowCastingMode.Off) { r.shadowCastingMode = ShadowCastingMode.Off; rends++; }
            }
            foreach (MeshRenderer r in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (r.shadowCastingMode == ShadowCastingMode.Off) continue;
                Vector3 e = r.bounds.size;
                if (Mathf.Max(e.x, e.y, e.z) < SmallSize) { r.shadowCastingMode = ShadowCastingMode.Off; rends++; }
            }
        }
        LightsOff += lights; RenderersOff += rends;
        if (lights + rends > 0) Debug.Log($"[Rendimiento] Sombras en {s.name}: sol hasta {MaxDistance:0} m y {Cascades} cascada; {lights} luces sin sombra; {rends} piezas pequeñas o de animal sin sombra.");
    }

    static void Pipeline()
    {
        if (_pipelineDone) return;
        _pipelineDone = true;
        if (!(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset src)) return;
        UniversalRenderPipelineAsset copy = Object.Instantiate(src);
        copy.name = src.name + " (sombras baratas)";
        copy.shadowDistance = Mathf.Min(src.shadowDistance, MaxDistance);
        copy.shadowCascadeCount = Cascades;
        QualitySettings.renderPipeline = copy;
    }
}
