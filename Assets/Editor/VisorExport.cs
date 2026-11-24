using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// VISOR WEB SIN BUILD (PB-168, docs/plataforma/visor-web.md). Vuelca una escena (cubos y primitivas con su color, en coordenadas de mundo) y sus
/// retos de código (historia, puestos y duelo de ideas) a <c>Builds/Visor/&lt;escena&gt;/</c>, para verla en el visor three.js de
/// <c>tools/visor/index.html</c> sin Play ni build WebGL (que tarda 30–60 min). Tarda segundos; abre la escena en modo aditivo y la cierra
/// sin guardar, así que no toca la escena que esté abierta.
///  · <c>escena.jsonl.gz</c>: un objeto por línea (mismo formato que el volcado de SampleScene de 2026-11-19, más <c>m</c>, la matriz de mundo).
///  · <c>retos.json</c>: la historia (pasos y diálogos), los retos de la historia (<see cref="CodeChallenges"/>), las misiones de puesto
///    (<see cref="MissionSites"/> → <see cref="MissionLibrary"/>) con el sitio resuelto, el banco de ideas y los edificios para saltar.
/// Por línea de comandos (con el Editor cerrado): <c>-batchmode -executeMethod VisorExport.ExportMuskBatch -quit</c>.
/// </summary>
public static class VisorExport
{
    const string Root = "Builds/Visor";
    const string MuskScene = "Microcosmos_ElonMusk";

    [MenuItem("Tools/Cold Sanctuary/Web/Visor sin build: exportar Elon Musk (Hawthorne)")]
    public static void ExportMusk() => ExportByName(MuskScene);

    [MenuItem("Tools/Cold Sanctuary/Web/Visor sin build: exportar la escena activa")]
    public static void ExportActive()
    {
        Scene s = SceneManager.GetActiveScene();
        if (!s.IsValid()) { Debug.LogError("[Visor] No hay escena activa."); return; }
        Export(s);
    }

    public static void ExportMuskBatch() => ExportByName(MuskScene);

    public static string ExportByName(string sceneName)
    {
        Scene loaded = SceneManager.GetSceneByName(sceneName);
        if (loaded.IsValid() && loaded.isLoaded) return Export(loaded);
        string path = null;
        foreach (string guid in AssetDatabase.FindAssets("t:Scene " + sceneName))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(p) == sceneName) { path = p; break; }
        }
        if (path == null) { Debug.LogError($"[Visor] No encuentro la escena {sceneName}: genérala antes con su menú de Build Microcosmos."); return null; }
        Scene s = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        try { return Export(s); }
        finally { EditorSceneManager.CloseScene(s, true); }
    }

    public static string Export(Scene s)
    {
        string dir = Path.Combine(Root, s.name);
        Directory.CreateDirectory(dir);
        var all = new List<Transform>();
        foreach (GameObject g in s.GetRootGameObjects()) all.AddRange(g.GetComponentsInChildren<Transform>(true));

        int n = 0, drawn = 0;
        string sceneFile = Path.Combine(dir, "escena.jsonl.gz");
        using (var fs = File.Create(sceneFile))
        using (var gz = new GZipStream(fs, System.IO.Compression.CompressionLevel.Optimal))
        using (var w = new StreamWriter(gz, new UTF8Encoding(false)))
        {
            foreach (Transform t in all)
            {
                if (!t.gameObject.activeInHierarchy) continue;
                string line = ObjectLine(t, out bool draws);
                if (line == null) continue;
                w.Write(line); w.Write('\n');
                n++; if (draws) drawn++;
            }
        }

        string retos = RetosJson(s, all);
        File.WriteAllText(Path.Combine(dir, "retos.json"), retos, new UTF8Encoding(false));
        Debug.Log($"[Visor] {s.name}: {n} objetos ({drawn} visibles) y retos → {Path.GetFullPath(dir)} ({new FileInfo(sceneFile).Length / 1024} KB). " +
                  "Ábrelo con tools/visor/index.html (docs/plataforma/visor-web.md).");
        return dir;
    }

    // ── Un objeto por línea: solo los que se dibujan (MeshRenderer) o tienen algo que el visor usa (seres, sitios, historia).
    static string ObjectLine(Transform t, out bool draws)
    {
        draws = false;
        var r = t.GetComponent<MeshRenderer>();
        var mf = t.GetComponent<MeshFilter>();
        bool anima = t.GetComponent<Anima>() != null;
        bool site = t.GetComponent<AreaFunctionSite>() != null;
        if ((r == null || !r.enabled || mf == null || mf.sharedMesh == null) && !anima && !site) return null;

        var sb = new StringBuilder(256);
        sb.Append("{\"path\":").Append(Q(PathOf(t))).Append(",\"name\":").Append(Q(t.name));
        sb.Append(",\"pos\":").Append(V(t.position)).Append(",\"rot\":").Append(V(t.eulerAngles)).Append(",\"scale\":").Append(V(t.lossyScale));
        if (r != null && r.enabled && mf != null && mf.sharedMesh != null)
        {
            draws = true;
            Matrix4x4 m = t.localToWorldMatrix;
            sb.Append(",\"m\":[");
            for (int row = 0; row < 3; row++) for (int col = 0; col < 4; col++) { if (row + col > 0) sb.Append(','); sb.Append(F(m[row, col])); }
            sb.Append(']');
            Bounds b = r.bounds;
            sb.Append(",\"bounds\":{\"size\":").Append(V(b.size)).Append(",\"center\":").Append(V(b.center)).Append('}');
            Material mat = r.sharedMaterial;
            Color c = mat == null ? Color.magenta : mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.HasProperty("_Color") ? mat.color : Color.white;
            sb.Append(",\"color\":").Append(Q("#" + ColorUtility.ToHtmlStringRGBA(c)));
            if (mat != null && mat.HasProperty("_EmissionColor") && mat.IsKeywordEnabled("_EMISSION"))
                sb.Append(",\"emission\":").Append(Q("#" + ColorUtility.ToHtmlStringRGB(mat.GetColor("_EmissionColor"))));
            string mesh = mf.sharedMesh.name;
            sb.Append(",\"mesh\":").Append(Q(mesh));
            string prim = Primitive(mesh);
            if (prim != null) sb.Append(",\"primitive\":").Append(Q(prim));
        }
        if (anima) sb.Append(",\"anima\":true");
        if (site) sb.Append(",\"building\":").Append(Q(t.GetComponent<AreaFunctionSite>().building));
        sb.Append('}');
        return sb.ToString();
    }

    static string Primitive(string mesh)
    {
        switch (mesh)
        {
            case "Cube": return "cube";
            case "Sphere": return "sphere";
            case "Cylinder": return "cylinder";
            case "Capsule": return "capsule";
            case "Plane": return "plane";
            case "Quad": return "quad";
            default: return null;
        }
    }

    // ── Los retos: historia, retos de la historia, misiones de puesto (con su sitio) y banco de ideas.
    static string RetosJson(Scene s, List<Transform> all)
    {
        var sb = new StringBuilder();
        sb.Append("{\"scene\":").Append(Q(s.name)).Append(",\"exportedAt\":").Append(Q(System.DateTime.Now.ToString("s")));

        StoryMission story = null;
        foreach (Transform t in all) { story = t.GetComponent<StoryMission>(); if (story != null) break; }
        Vector3 center = story != null ? story.transform.position : Vector3.zero;
        if (story != null)
        {
            sb.Append(",\"story\":{\"title\":").Append(Q(story.title)).Append(",\"pos\":").Append(V(center));
            sb.Append(",\"finalSpeaker\":").Append(Q(story.finalSpeaker)).Append(",\"finalLine\":").Append(Q(story.finalLine));
            sb.Append(",\"steps\":[");
            for (int i = 0; i < story.steps.Length; i++)
            {
                StoryMission.StoryStep st = story.steps[i];
                if (i > 0) sb.Append(',');
                sb.Append("{\"goal\":").Append(Q(st.goal)).Append(",\"kind\":").Append(Q(st.kind.ToString()));
                sb.Append(",\"speaker\":").Append(Q(st.speaker)).Append(",\"line\":").Append(Q(st.line)).Append(",\"source\":").Append(Q(st.truthSource));
                sb.Append(",\"targets\":[");
                bool first = true;
                foreach (Transform tg in st.targets)
                {
                    if (tg == null) continue;
                    if (!first) sb.Append(','); first = false;
                    sb.Append("{\"name\":").Append(Q(tg.name)).Append(",\"pos\":").Append(V(tg.position)).Append('}');
                }
                sb.Append("]}");
            }
            sb.Append("]}");
        }

        sb.Append(",\"storyChallenges\":[");
        bool f1 = true;
        foreach (CodeChallenges.Spec sp in CodeChallenges.ForScene(s.name))
        {
            if (!f1) sb.Append(','); f1 = false;
            Transform tg = null;
            if (!string.IsNullOrEmpty(sp.target)) foreach (Transform t in all) if (t.name == sp.target) { tg = t; break; }
            sb.Append("{\"afterGoal\":").Append(Q(sp.afterGoal)).Append(",\"goal\":").Append(Q(sp.goal)).Append(",\"target\":").Append(Q(sp.target));
            sb.Append(",\"speaker\":").Append(Q(sp.speaker)).Append(",\"line\":").Append(Q(sp.line));
            if (tg != null) sb.Append(",\"pos\":").Append(V(tg.position));
            sb.Append(",\"exercise\":").Append(JsonUtility.ToJson(sp.exercise)).Append('}');
        }
        sb.Append(']');

        sb.Append(",\"stations\":[");
        bool f2 = true;
        foreach (MissionSites.Binding b in MissionSites.ForScene(s.name))
        {
            ChoreTemplate tpl = MissionLibrary.Find(b.template);
            if (tpl == null) continue;
            Transform tg = StationMissions.Resolve(all, b, center, out bool fallback);
            if (!f2) sb.Append(','); f2 = false;
            sb.Append("{\"site\":").Append(Q(tg != null ? tg.name : b.fallback));
            if (tg != null) sb.Append(",\"pos\":").Append(V(tg.position));
            sb.Append(",\"supuesto\":").Append(tg == null || fallback ? "true" : "false");
            sb.Append(",\"template\":").Append(JsonUtility.ToJson(tpl)).Append('}');
        }
        sb.Append(']');

        sb.Append(",\"ideas\":[");
        bool f3 = true;
        foreach (CodeExercise e in MissionLibrary.IdeaBank) { if (!f3) sb.Append(','); f3 = false; sb.Append(JsonUtility.ToJson(e)); }
        sb.Append(']');

        sb.Append(",\"buildings\":[");
        bool f4 = true;
        foreach (Transform t in all)
        {
            if (!t.gameObject.activeInHierarchy || !t.TryGetComponent(out AreaFunctionSite a)) continue;
            if (!f4) sb.Append(','); f4 = false;
            sb.Append("{\"name\":").Append(Q(t.name)).Append(",\"building\":").Append(Q(a.building)).Append(",\"pos\":").Append(V(t.position)).Append('}');
        }
        sb.Append("]}");
        return sb.ToString();
    }

    static string PathOf(Transform t)
    {
        var parts = new List<string>();
        for (Transform x = t; x != null; x = x.parent) parts.Add(x.name);
        parts.Reverse();
        return string.Join("/", parts);
    }

    static string F(float v) => (Mathf.Round(v * 1000f) / 1000f).ToString("0.###", CultureInfo.InvariantCulture);
    static string V(Vector3 v) => "[" + F(v.x) + "," + F(v.y) + "," + F(v.z) + "]";

    static string Q(string s)
    {
        if (s == null) return "\"\"";
        var sb = new StringBuilder(s.Length + 2);
        sb.Append('"');
        foreach (char c in s)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': break;
                case '\t': sb.Append("\\t"); break;
                default: if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4")); else sb.Append(c); break;
            }
        }
        return sb.Append('"').ToString();
    }
}
