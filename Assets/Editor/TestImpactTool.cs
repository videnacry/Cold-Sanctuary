using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// HERRAMIENTAS DE LAS PRUEBAS RÁPIDAS (2026-11-11, scratchpad analisis-pruebas.md):
///  · Grupos afectados: de «git diff --name-only A..B» a los grupos de prueba que tocan los archivos cambiados (TestImpact), escritos en
///    Temp/runner-groups.txt (lo que lee el arnés de grupos del probador). Si una clase centro toca más de la mitad de los grupos, lo dice y
///    recomienda la batería entera. Desde la línea de órdenes: -executeMethod TestImpactTool.FromCommandLine -impactRange A..B
///  · Rehacer la lista rápida: de Logs/test-results.tsv (una batería entera con el TestRunner nuevo) a Scripts/TestPresetsData.cs.
///  · Jugar con las pruebas (rápida): la lista rápida, de una vez.
/// </summary>
public static class TestImpactTool
{
    public const string GroupsFile = "Temp/runner-groups.txt";
    public const string RapidaFile = "Assets/Scripts/TestPresetsData.cs";
    public const float RapidaMaxSeconds = 60f;

    [MenuItem("Tools/Cold Sanctuary/Pruebas/Grupos afectados (desde edificios)")]
    public static void FromMenu() => Run("origin/claude/edificios...HEAD");

    /// <summary>-executeMethod TestImpactTool.FromCommandLine -impactRange A..B</summary>
    public static void FromCommandLine()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        int i = System.Array.IndexOf(args, "-impactRange");
        Run(i >= 0 && i + 1 < args.Length ? args[i + 1] : "origin/claude/edificios...HEAD");
    }

    public static TestImpact.Result Run(string range)
    {
        string root = Path.GetDirectoryName(Application.dataPath);
        List<string> changed = Git(root, $"diff --name-only {range}");
        var tests = new List<TestImpact.TestFile>();
        foreach (string f in Directory.GetFiles(Path.Combine(root, "Assets", "Scripts"), "*.cs", SearchOption.AllDirectories))
        {
            string src = File.ReadAllText(f); int g = TestImpact.TestGroup(src);
            if (g >= 0) tests.Add(new TestImpact.TestFile { cls = Path.GetFileNameWithoutExtension(f), group = g, text = src });
        }
        var classes = new List<string>(); var changedTests = new List<string>(); var other = new List<string>();
        foreach (string p in changed)
        {
            if (!p.EndsWith(".cs")) { if (p.StartsWith("Assets/") && !p.EndsWith(".meta")) other.Add(p); continue; }
            string full = Path.Combine(root, p);
            string src = File.Exists(full) ? File.ReadAllText(full) : "";
            if (TestImpact.TestGroup(src) >= 0) { changedTests.Add(Path.GetFileNameWithoutExtension(p)); continue; }
            List<string> decl = TestImpact.ClassesIn(src);
            if (decl.Count == 0) decl.Add(Path.GetFileNameWithoutExtension(p));   // (borrado: por su nombre)
            classes.AddRange(decl);
        }
        TestImpact.Result r = TestImpact.Groups(classes, changedTests, tests);
        Directory.CreateDirectory(Path.Combine(root, "Temp"));
        File.WriteAllText(Path.Combine(root, GroupsFile), string.Join(" ", r.groups));
        Debug.Log($"[Pruebas] {range}: {changed.Count} archivos cambiados → {r.groups.Count} de {r.totalGroups} grupos: {string.Join(" ", r.groups)} (en {GroupsFile})." +
                  (r.unmapped.Count > 0 ? $" Sin prueba que los cite: {string.Join(", ", r.unmapped.Take(20))}." : "") +
                  (other.Count > 0 ? $" Otros archivos (escenas, prefabs…; mejor la batería entera si son de la escena): {string.Join(", ", other.Take(10))}." : ""));
        if (r.RecommendFull) Debug.LogWarning($"[Pruebas] Cambia un CENTRO ({string.Join("; ", r.hubs)}): toca más de la mitad de los grupos. Mejor la batería entera.");
        return r;
    }

    static List<string> Git(string root, string args)
    {
        var l = new List<string>();
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("git", args) { WorkingDirectory = root, RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
            using (var p = System.Diagnostics.Process.Start(psi))
            {
                string o = p.StandardOutput.ReadToEnd(); p.WaitForExit(30000);
                foreach (string line in o.Split('\n')) { string t = line.Trim(); if (t.Length > 0) l.Add(t); }
            }
        }
        catch (System.Exception e) { Debug.LogWarning($"[Pruebas] git {args}: {e.Message}"); }
        return l;
    }

    [MenuItem("Tools/Cold Sanctuary/Pruebas/Rehacer la lista rápida")]
    public static void RefreshRapidaMenu() => RefreshRapida(TestResults.Path);

    /// <summary>De un archivo de resultados (una batería entera) a la lista rápida: las unidades de menos de 60 s que acabaron (sin TIMEOUT).</summary>
    public static int RefreshRapida(string resultsPath)
    {
        List<TestResults.Row> rows = TestResults.Read(resultsPath);
        if (rows.Count == 0) { Debug.LogWarning($"[Pruebas] {resultsPath} no tiene filas: corre antes la batería entera."); return 0; }
        var ok = rows.Where(r => !r.unit.StartsWith("paralelo") && r.status != "TIMEOUT" && r.seconds < RapidaMaxSeconds).OrderBy(r => r.unit).ToList();
        float total = ok.Sum(r => r.seconds);
        var lines = new List<string>
        {
            "using System.Collections.Generic;", "",
            "/// <summary>",
            "/// LA LISTA RÁPIDA, COMO DATOS EN C# (el repositorio solo guarda .cs y documentos). Una unidad por entrada: clase, grupo y segundos en la",
            "/// última batería completa. La reescribe «Tools/Cold Sanctuary/Pruebas/Rehacer la lista rápida» (TestImpactTool.RefreshRapida).",
            "/// </summary>",
            "public static class TestPresetsData", "{",
            $"    // Prueba rápida: unidades de menos de {RapidaMaxSeconds:0} s en la última batería completa ({resultsPath}, {System.DateTime.Now:yyyy-MM-dd HH:mm}): {ok.Count} unidades, {total:0} s.",
            "    public static readonly (string unit, int group, float seconds)[] Rapida =", "    {"
        };
        lines.AddRange(ok.Select(r => $"        (\"{r.unit}\", {r.group}, {r.seconds.ToString("0", System.Globalization.CultureInfo.InvariantCulture)}f),"));
        lines.Add("    };"); lines.Add("}");
        File.WriteAllLines(RapidaFile, lines);
        AssetDatabase.ImportAsset(RapidaFile);
        Debug.Log($"[Pruebas] Lista rápida rehecha: {ok.Count} unidades, {total:0} s (de {rows.Count} filas).");
        return ok.Count;
    }

    [MenuItem("Tools/Cold Sanctuary/Pruebas/Jugar con las pruebas (rápida)")]
    public static void PlayRapida()
    {
        File.WriteAllText(ColdSanctuary.TestRunner.PresetFile, TestPresets.Rapida);
        File.WriteAllText(ColdSanctuary.TestRunner.RequestFile, "");
        EditorApplication.isPlaying = true;
    }
}
