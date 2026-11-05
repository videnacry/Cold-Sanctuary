using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// QUÉ SCRIPTS CUESTAN, CON NOMBRE (docs/rendimiento.md). Los ProfilerRecorder de <see cref="PerfProbe"/> no ven los marcadores por
/// script sin el perfilador en marcha. Esto lo enciende, graba unos frames en Play y lee los mismos datos que la ventana Profiler:
/// el coste de cada «Clase.Update()» bajo BehaviourUpdate y los demás bucles, y lo más caro del hilo principal por tiempo propio.
/// Todo va al log con [Rendimiento] y acaba en «perfil: fin». Espera acotada: <see cref="MaxSeconds"/>.
/// </summary>
public static class RendimientoPerfil
{
    const string Menu = "Tools/Cold Sanctuary/Rendimiento/";
    const int Frames = 40;
    const double MaxSeconds = 180;
    const int Rows = 40;
    static readonly HashSet<string> Loops = new HashSet<string>
    {
        "BehaviourUpdate", "LateBehaviourUpdate", "FixedBehaviourUpdate", "PreLateUpdate.ScriptRunBehaviourLateUpdate",
        "Update.ScriptRunBehaviourUpdate", "FixedUpdate.ScriptRunBehaviourFixedUpdate", "CoroutinesDelayedCalls",
    };
    /// <summary>De estas marcas se dice quién las llama (no tienen nombre de script).</summary>
    static readonly string[] Watched = { "GetComponentNullErrorMessage", "AssetDatabase.GetAssetPathToGUIDV2", "FindObjectsOfType", "Debug.Log", "Logging", "Physics.OverlapSphere", "Physics.RaycastNonAlloc" };
    static readonly Dictionary<string, long> Callers = new Dictionary<string, long>();
    /// <summary>Hasta esta profundidad se reparte el hilo principal, con su tiempo total: física, animación, recorte, render, GC…</summary>
    const int TreeDepth = 4;
    const double TreeMinMs = 0.4;
    static readonly Dictionary<string, Row> Tree = new Dictionary<string, Row>();
    /// <summary>Marcas que se suman estén donde estén.</summary>
    static readonly string[] Totals =
    {
        "UpdateRendererBoundingVolumes", "TransformChangeSystem", "Physics.Processing", "Physics.Simulate", "PxScene.simulate", "Physics.SyncColliderTransform", "Physics.SyncRigidbodyTransform",
        "Physics.SendTriggerEvents", "Physics.SyncTransforms", "GC.Collect", "GC.Alloc", "Animators.Update", "Director.ProcessFrame", "CullScriptable", "Culling", "SRPBatcher.Flush",
        "NavMeshManager", "AI.NavMeshManager", "UGUI.Rendering.UpdateBatches", "GUI.Repaint", "Gfx.WaitForPresentOnGfxThread", "Gfx.WaitForRenderThread", "WaitForTargetFPS", "Profiler.FlushMemoryCounters",
    };
    static readonly Dictionary<string, Row> Named = new Dictionary<string, Row>();
    static double _until;
    static int _first;

    [MenuItem(Menu + "Perfilar scripts (40 frames, con nombres)")]
    public static void Start()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[Rendimiento] perfil: hay que estar en Play."); return; }
        ProfilerDriver.ClearAllFrames();
        ProfilerDriver.profileEditor = false;
        ProfilerDriver.deepProfiling = false;
        ProfilerDriver.enabled = true;
        _first = -1;
        _until = EditorApplication.timeSinceStartup + MaxSeconds;
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        Debug.Log($"[Rendimiento] perfil: grabando {Frames} frames.");
    }

    static void Tick()
    {
        if (_first < 0 && ProfilerDriver.lastFrameIndex >= 0) _first = ProfilerDriver.lastFrameIndex + 2;   // (los primeros salen a medias)
        bool enough = _first >= 0 && ProfilerDriver.lastFrameIndex >= _first + Frames;
        bool late = EditorApplication.timeSinceStartup > _until || !Application.isPlaying;
        if (!enough && !late) return;
        EditorApplication.update -= Tick;
        ProfilerDriver.enabled = false;
        if (late && !enough) Debug.LogWarning($"[Rendimiento] perfil: se acabó el tiempo con {ProfilerDriver.lastFrameIndex - _first} frames.");
        try { Report(); }
        catch (System.Exception e) { Debug.LogError($"[Rendimiento] perfil: {e}"); }
        ProfilerDriver.ClearAllFrames();
        Debug.Log("[Rendimiento] perfil: fin");
    }

    sealed class Row { public double total, self; public long calls; public double gc; }

    static void Report()
    {
        var byScript = new Dictionary<string, Row>();
        var bySelf = new Dictionary<string, Row>();
        Callers.Clear();
        Tree.Clear(); Named.Clear();
        int from = Mathf.Max(_first, ProfilerDriver.firstFrameIndex), to = ProfilerDriver.lastFrameIndex, frames = 0;
        for (int f = from; f <= to; f++)
        {
            using (HierarchyFrameDataView v = ProfilerDriver.GetHierarchyFrameDataView(f, 0, HierarchyFrameDataView.ViewModes.MergeSamplesWithTheSameName, HierarchyFrameDataView.columnTotalTime, false))
            {
                if (v == null || !v.valid) continue;
                frames++;
                Walk(v, v.GetRootItemID(), "", "", 0, false, byScript, bySelf);
            }
        }
        if (frames == 0) { Debug.LogWarning("[Rendimiento] perfil: ningún frame válido."); return; }
        double sum = byScript.Values.Sum(r => r.total) / frames;
        Debug.Log($"[Rendimiento] perfil: {frames} frames; scripts {sum:0.0} ms por frame en {byScript.Count} tipos.");
        foreach (var kv in byScript.OrderByDescending(k => k.Value.total).Take(Rows))
            Debug.Log($"[Rendimiento] coste {kv.Key}: {kv.Value.total / frames:0.00} ms/frame, {kv.Value.calls / (float)frames:0} llamadas, {kv.Value.gc / frames / 1024.0:0} KB");
        foreach (var kv in bySelf.OrderByDescending(k => k.Value.self).Take(Rows))
            Debug.Log($"[Rendimiento] propio {kv.Key}: {kv.Value.self / frames:0.00} ms/frame, {kv.Value.calls / (float)frames:0} llamadas");
        foreach (var kv in Tree.OrderBy(k => k.Key, System.StringComparer.Ordinal))
            if (kv.Value.total / frames >= TreeMinMs) Debug.Log($"[Rendimiento] hilo {kv.Key}: {kv.Value.total / frames:0.00} ms/frame, {kv.Value.calls / (float)frames:0.0} llamadas");
        foreach (var kv in Named.OrderByDescending(k => k.Value.total))
            Debug.Log($"[Rendimiento] total {kv.Key}: {kv.Value.total / frames:0.00} ms/frame, {kv.Value.calls / (float)frames:0.0} llamadas");
        foreach (var kv in Callers.OrderByDescending(k => k.Value).Take(Rows)) Debug.Log($"[Rendimiento] llamada {kv.Key}: {kv.Value / (float)frames:0.0} por frame");
    }

    static void Walk(HierarchyFrameDataView v, int id, string parent, string path, int depth, bool underLoop, Dictionary<string, Row> byScript, Dictionary<string, Row> bySelf)
    {
        var kids = new List<int>();
        v.GetItemChildren(id, kids);
        foreach (int k in kids)
        {
            string n = v.GetItemName(k);
            double total = v.GetItemColumnDataAsFloat(k, HierarchyFrameDataView.columnTotalTime);
            double self = v.GetItemColumnDataAsFloat(k, HierarchyFrameDataView.columnSelfTime);
            long calls = (long)v.GetItemColumnDataAsFloat(k, HierarchyFrameDataView.columnCalls);
            if (underLoop) Add(byScript, n).Sum(total, 0, calls, v.GetItemColumnDataAsFloat(k, HierarchyFrameDataView.columnGcMemory));
            Add(bySelf, n).Sum(0, self, calls, 0);
            foreach (string w in Watched)
                if (n.StartsWith(w)) { string key = $"{w} ← {parent}"; Callers.TryGetValue(key, out long c); Callers[key] = c + calls; }
            string here = path.Length == 0 ? n : path + " > " + n;
            if (depth < TreeDepth) Add(Tree, here).Sum(total, 0, calls, 0);
            if (System.Array.IndexOf(Totals, n) >= 0) Add(Named, n).Sum(total, 0, calls, 0);
            Walk(v, k, n, here, depth + 1, Loops.Contains(n), byScript, bySelf);
        }
    }

    static Row Add(Dictionary<string, Row> d, string n)
    {
        if (!d.TryGetValue(n, out Row r)) d[n] = r = new Row();
        return r;
    }

    static void Sum(this Row r, double total, double self, long calls, double gc) { r.total += total; r.self += self; r.calls += calls; r.gc += gc; }

    // ── Build de medida: FPS fuera del editor (tools/build-perf.sh) ─────────────────────────────────────────────────────

    /// <summary>Solo SampleScene: construir las 300 escenas no cabe en la memoria del PC. De desarrollo, nombra scripts en el Profiler;
    /// sin desarrollo, da el FPS que vería quien juega.</summary>
    static void BuildPerf(bool dev)
    {
        var o = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/SampleScene.unity" },
            locationPathName = $"Builds/Perf{(dev ? "Dev" : "")}/ColdSanctuary.exe",
            target = BuildTarget.StandaloneWindows64,
            options = dev ? BuildOptions.Development : BuildOptions.None,
        };
        BuildReport r = BuildPipeline.BuildPlayer(o);
        Debug.Log($"[Rendimiento] build: {r.summary.result}, {r.summary.totalSize / (1024 * 1024)} MB, {r.summary.totalTime.TotalMinutes:0.0} min, en {o.locationPathName}; {r.summary.totalErrors} errores.");
    }

    [MenuItem(Menu + "Build de medida (final)")]
    public static void BuildFinal() => BuildPerf(false);

    [MenuItem(Menu + "Build de medida (desarrollo)")]
    public static void BuildDevelopment() => BuildPerf(true);
}
