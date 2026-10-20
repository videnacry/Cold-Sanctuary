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

    static readonly string[] Phases =
    {
        "PlayerLoop", "BehaviourUpdate", "LateBehaviourUpdate", "FixedBehaviourUpdate", "Physics.Simulate", "Physics.Processing",
        "CoroutinesDelayedCalls", "GUIUtility.ProcessEvent", "UGUI.Rendering.UpdateBatches", "Camera.Render", "Gfx.WaitForPresentOnGfxThread",
        "NavMeshManager", "AI.NavMeshManager", "Physics.SyncTransforms", "Director.ProcessFrame",
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

        foreach (var (_, r) in recorders) r.Dispose();
        foreach (var (_, r) in render) r.Dispose();
        foreach (var (_, r) in scripts) r.Dispose();
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
