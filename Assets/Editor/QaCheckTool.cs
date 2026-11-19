using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// EL COMPROBADOR DE QA EN EL EDITOR (2026-11-13, docs/qa-propuesta.md §6): «Tools/Cold Sanctuary/QA/Comprobar» corre <see cref="QaCheck"/>
/// sobre el repositorio (el de Temp/qa-root.txt, que escribe tools/sync-to-unity.sh; o COLD_SANCTUARY_REPO; o el propio proyecto), lo
/// escribe en la consola y en Logs/qa-check.txt. Desde la línea de órdenes:
///   Unity -batchmode -projectPath … -executeMethod QaCheckTool.FromCommandLine [-qaRoot RUTA]   (sale con 0 si cuadra, 1 si no, 2 sin docs/qa)
/// Sin Unity, el qa-check.sh compila Assets/Scripts/QaCheck.cs con el dotnet del Editor y llama a <see cref="QaCheck.Cli"/>.
/// </summary>
public static class QaCheckTool
{
    public const string ReportFile = "Logs/qa-check.txt";

    [MenuItem("Tools/Cold Sanctuary/QA/Comprobar")]
    public static void FromMenu() => Check(null);

    /// <summary>-executeMethod QaCheckTool.FromCommandLine [-qaRoot RUTA]</summary>
    public static void FromCommandLine()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        int i = System.Array.IndexOf(args, "-qaRoot");
        int code = Check(i >= 0 && i + 1 < args.Length ? args[i + 1] : null);
        if (Application.isBatchMode) EditorApplication.Exit(code);
    }

    /// <summary>Corre el comprobador. Devuelve 0 si cuadra, 1 si hay problemas, 2 si no encuentra docs/qa.</summary>
    public static int Check(string root)
    {
        string project = Path.GetDirectoryName(Application.dataPath);
        root = root ?? QaCheck.FindRoot(project);
        if (root == null || !Directory.Exists(Path.Combine(root, "docs", "qa")))
        {
            Debug.LogWarning($"[QA] No encuentro docs/qa: ni en {QaCheck.RootFile} del proyecto, ni en COLD_SANCTUARY_REPO, ni en {project}. Sincroniza con tools/sync-to-unity.sh.");
            return 2;
        }
        QaCheck.Report rep = QaCheck.Run(QaCheck.Load(root));
        string text = rep.Text();
        try { Directory.CreateDirectory(Path.Combine(project, "Logs")); File.WriteAllText(Path.Combine(project, ReportFile), $"{root}\n{text}"); } catch (System.Exception e) { Debug.LogWarning($"[QA] No se pudo escribir {ReportFile}: {e.Message}"); }
        if (rep.Problems.Count == 0) Debug.Log(text); else Debug.LogWarning(text);   // como TestProbe: un problema de QA no es un error de compilación
        return rep.Problems.Count == 0 ? 0 : 1;
    }
}
