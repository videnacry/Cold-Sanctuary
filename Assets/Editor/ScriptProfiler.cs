using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// PERFIL DE SCRIPTS (menú Tools/Cold Sanctuary/Perfil de scripts, en Play): graba ~120 frames con el profiler y escribe
/// en el log ([Perfil]) los marcadores que más tiempo propio consumen (Clase.Update(), Find…, Raycast, GUI…), para saber
/// QUÉ hay que optimizar en vez de adivinarlo.
/// </summary>
public static class ScriptProfiler
{
    static int _startFrame, _frames;

    [MenuItem("Tools/Cold Sanctuary/Perfil de scripts (en Play)")]
    public static void Run()
    {
        if (!EditorApplication.isPlaying) { Debug.Log("[Perfil] Hay que estar en Play."); return; }
        ProfilerDriver.ClearAllFrames();
        ProfilerDriver.enabled = true;
        _startFrame = -1; _frames = 0;
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        Debug.Log("[Perfil] Grabando 120 frames…");
    }

    static void Tick()
    {
        int last = ProfilerDriver.lastFrameIndex;
        if (_startFrame < 0) { _startFrame = last; return; }
        if (last - _startFrame < 120) return;
        EditorApplication.update -= Tick;
        ProfilerDriver.enabled = false;
        Report(Mathf.Max(ProfilerDriver.firstFrameIndex, _startFrame), last);
    }

    static void Report(int first, int last)
    {
        var self = new Dictionary<string, double>();
        var total = new Dictionary<string, double>();
        var calls = new Dictionary<string, int>();
        int frames = 0;
        for (int f = first; f <= last; f++)
        {
            using (var view = ProfilerDriver.GetHierarchyFrameDataView(f, 0, HierarchyFrameDataView.ViewModes.MergeSamplesWithTheSameName, HierarchyFrameDataView.columnSelfTime, false))
            {
                if (view == null || !view.valid) continue;
                frames++;
                var stack = new Stack<int>(); stack.Push(view.GetRootItemID());
                var kids = new List<int>();
                while (stack.Count > 0)
                {
                    int id = stack.Pop();
                    string name = view.GetItemName(id);
                    double s = view.GetItemColumnDataAsDouble(id, HierarchyFrameDataView.columnSelfTime);
                    double t = view.GetItemColumnDataAsDouble(id, HierarchyFrameDataView.columnTotalTime);
                    int c = (int)view.GetItemColumnDataAsDouble(id, HierarchyFrameDataView.columnCalls);
                    if (!string.IsNullOrEmpty(name))
                    {
                        self[name] = (self.TryGetValue(name, out double a) ? a : 0) + s;
                        total[name] = (total.TryGetValue(name, out double b) ? b : 0) + t;
                        calls[name] = (calls.TryGetValue(name, out int k) ? k : 0) + c;
                    }
                    kids.Clear(); view.GetItemChildren(id, kids);
                    foreach (int k2 in kids) stack.Push(k2);
                }
            }
        }
        if (frames == 0) { Debug.Log("[Perfil] Sin datos del profiler."); return; }
        Debug.Log($"[Perfil] {frames} frames analizados.");
        foreach (var kv in self.OrderByDescending(kv => kv.Value).Take(30))
            Debug.Log($"[Perfil] propio {kv.Value / frames:0.00} ms/frame · total {total[kv.Key] / frames:0.00} · {calls[kv.Key] / frames:0} llamadas/frame · {kv.Key}");
        foreach (var kv in total.Where(kv => kv.Key.EndsWith("()")).OrderByDescending(kv => kv.Value).Take(25))
            Debug.Log($"[Perfil] script total {kv.Value / frames:0.00} ms/frame · {calls[kv.Key] / frames:0} llamadas/frame · {kv.Key}");
    }
}
