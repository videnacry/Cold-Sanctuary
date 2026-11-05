using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;

/// <summary>
/// SONDA DE RENDIMIENTO (consola: <c>rendimiento</c>): mide unos segundos de juego y escribe en el log ([Rendimiento]) el
/// FPS, el tiempo del bucle por fases (scripts Update/LateUpdate/FixedUpdate, físicas, GUI, render), los contadores de
/// dibujado (batches, SetPass, triángulos), cuántos objetos/renderers/etiquetas hay y, sobre todo, QUÉ SCRIPTS cuestan
/// más (los marcadores «Clase.Update()» del profiler, ordenados por milisegundos por frame).
/// </summary>
public class PerfProbe : MonoBehaviour
{
    public static PerfProbe Instance { get; private set; }
    void OnEnable() { Instance = this; }

    public void Run(int frames = 240) => StartCoroutine(Measure(frames));

    /// <summary>MEDIDA A/B (docs/rendimiento.md): mide con los lotes estáticos, sin ellos y otra vez con ellos, en el mismo sitio. La
    /// diferencia entre la 1.ª y la 2.ª es lo que dan los lotes; entre la 1.ª y la 3.ª, el ruido de la medida.</summary>
    public void RunAB(int frames = 120) => StartCoroutine(MeasureAB(frames));

    IEnumerator MeasureAB(int frames)
    {
        Debug.Log($"[Rendimiento] A/B: con lotes ({LoteEstatico.LastReport})");
        LoteEstatico.SetLive(true);
        yield return Measure(frames);
        Debug.Log("[Rendimiento] A/B: sin lotes");
        LoteEstatico.SetLive(false);
        yield return null; yield return null;
        yield return Measure(frames);
        Debug.Log("[Rendimiento] A/B: con lotes otra vez");
        LoteEstatico.SetLive(true);
        yield return null; yield return null;
        yield return Measure(frames);
        Debug.Log("[Rendimiento] A/B: fin");
    }

    static readonly string[] Phases =
    {
        "PlayerLoop", "BehaviourUpdate", "LateBehaviourUpdate", "FixedBehaviourUpdate", "Physics.Simulate", "Physics.Processing",
        "CoroutinesDelayedCalls", "GUIUtility.ProcessEvent", "UGUI.Rendering.UpdateBatches", "Camera.Render", "Gfx.WaitForPresentOnGfxThread",
        "NavMeshManager", "AI.NavMeshManager", "Physics.SyncTransforms", "Director.ProcessFrame",
        // El reparto del dibujado: hilo principal contra hilo de render, recorte, sombras y el prepaso del SSAO (dibuja la escena otra vez).
        "Main Thread", "Render Thread", "CullScriptable", "MainLightShadow", "AdditionalLightsShadow", "DepthNormalPrepass", "DepthPrepass",
        "DrawOpaqueObjects", "DrawTransparentObjects", "SSAO", "Copy Color", "Copy Depth",
    };

    IEnumerator Measure(int frames)
    {
        var recorders = new List<(string name, ProfilerRecorder r)>();
        foreach (string p in Phases) recorders.Add((p, ProfilerRecorder.StartNew(ProfilerCategory.Internal, p, frames)));
        var render = new List<(string, ProfilerRecorder)>
        {
            ("Batches", ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count", frames)),
            ("SetPass", ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count", frames)),
            ("Triángulos", ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count", frames)),
            ("Draw calls", ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count", frames)),
        };
        // Todos los marcadores de scripts (Clase.Update(), Clase.LateUpdate(), Clase.OnGUI(), corutinas…).
        var handles = new List<ProfilerRecorderHandle>();
        ProfilerRecorderHandle.GetAvailable(handles);
        var scripts = new List<(string, ProfilerRecorder)>();
        foreach (var h in handles)
        {
            var d = ProfilerRecorderHandle.GetDescription(h);
            string n = d.Name;
            if (n.EndsWith(".Update()") || n.EndsWith(".LateUpdate()") || n.EndsWith(".FixedUpdate()") || n.EndsWith(".OnGUI()") || n.StartsWith("Coroutine:") || n.EndsWith(".MoveNext"))
                scripts.Add((n, ProfilerRecorder.StartNew(d.Category, n, frames)));
        }
        float t0 = Time.realtimeSinceStartup;
        for (int i = 0; i < frames; i++) yield return null;
        float secs = Time.realtimeSinceStartup - t0;

        Debug.Log($"[Rendimiento] {frames} frames en {secs:0.0} s → {frames / secs:0.0} FPS ({secs / frames * 1000f:0.0} ms/frame).");
        foreach (var (n, r) in recorders) if (r.Valid && r.Count > 0) Debug.Log($"[Rendimiento] fase {n}: {Avg(r) / 1e6:0.00} ms/frame");
        foreach (var (n, r) in render) if (r.Valid && r.Count > 0) Debug.Log($"[Rendimiento] dibujado {n}: {AvgRaw(r):0}");
        var top = scripts.Where(s => s.Item2.Valid && s.Item2.Count > 0).Select(s => (s.Item1, ms: Avg(s.Item2) / 1e6)).OrderByDescending(s => s.ms).Take(25).ToList();
        foreach (var s in top) Debug.Log($"[Rendimiento] script {s.Item1}: {s.ms:0.000} ms/frame");

        int gos = FindObjectsByType<Transform>(FindObjectsSortMode.None).Length;
        int rends = FindObjectsByType<Renderer>(FindObjectsSortMode.None).Length;
        int cols = FindObjectsByType<Collider>(FindObjectsSortMode.None).Length;
        int labels = FindObjectsByType<WorldLabel>(FindObjectsSortMode.None).Length;
        int animas = FindObjectsByType<Anima>(FindObjectsSortMode.None).Length;
        var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        Debug.Log($"[Rendimiento] escena: {gos} objetos, {rends} renderers, {cols} colliders, {labels} etiquetas, {animas} animas, {behaviours.Length} componentes de script.");
        foreach (var g in behaviours.GroupBy(b => b.GetType().Name).OrderByDescending(g => g.Count()).Take(12))
            Debug.Log($"[Rendimiento] componente {g.Key}: {g.Count()}");
        LogRenderers();

        foreach (var (_, r) in recorders) r.Dispose();
        foreach (var (_, r) in render) r.Dispose();
        foreach (var (_, r) in scripts) r.Dispose();
    }

    // Qué BUILDER pone los renderers: los que se dibujan de verdad (encendidos y sin forceRenderingOff), por rama de la jerarquía
    // (cada rama de LevelDressing_AUTO es un builder) y por nombre de pieza; más lo que lleva el lote estático.
    static void LogRenderers()
    {
        Renderer[] rs = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        var branch = new Dictionary<string, int>();
        var leaf = new Dictionary<string, int>();
        var cache = new Dictionary<Transform, string>();
        var mats = new HashSet<Material>();
        int drawn = 0, shadow = 0;
        foreach (Renderer r in rs)
        {
            if (!r.enabled || r.forceRenderingOff) continue;
            drawn++;
            if (r.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off) shadow++;
            if (r.sharedMaterial != null) mats.Add(r.sharedMaterial);
            string b = Branch(r.transform.parent != null ? r.transform.parent : r.transform, cache);
            branch.TryGetValue(b, out int n); branch[b] = n + 1;
            leaf.TryGetValue(r.name, out n); leaf[r.name] = n + 1;
        }
        Debug.Log($"[Rendimiento] renderers que se dibujan: {drawn} de {rs.Length}; con sombra {shadow}; materiales {mats.Count}; lotes {LoteEstatico.Groups} con {LoteEstatico.Pieces} piezas; devueltas {LoteEstatico.Ejected}; lotes de detalle apagados {LoteEstatico.HiddenDetail}; cuerpos lejanos sin dibujar {LoteEstatico.HiddenBodies}");
        foreach (var kv in branch.OrderByDescending(k => k.Value).Take(30)) Debug.Log($"[Rendimiento] builder {kv.Key}: {kv.Value}");
        foreach (var kv in leaf.OrderByDescending(k => k.Value).Take(20)) Debug.Log($"[Rendimiento] pieza {kv.Key}: {kv.Value}");
    }

    // Los tres primeros niveles del camino (raíz/builder/grupo).
    static string Branch(Transform t, Dictionary<Transform, string> cache)
    {
        if (cache.TryGetValue(t, out string s)) return s;
        var path = new List<string>();
        for (Transform p = t; p != null; p = p.parent) path.Insert(0, p.name);
        s = string.Join("/", path.Take(3));
        cache[t] = s;
        return s;
    }

    static double Avg(ProfilerRecorder r)
    {
        var samples = new List<ProfilerRecorderSample>(r.Capacity);
        r.CopyTo(samples);
        if (samples.Count == 0) return 0;
        double s = 0; foreach (var x in samples) s += x.Value;
        return s / samples.Count;
    }
    static double AvgRaw(ProfilerRecorder r) => Avg(r);
}
