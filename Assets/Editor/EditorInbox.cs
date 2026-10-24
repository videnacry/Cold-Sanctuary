using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// ENTRADA DIRECTA DEL EDITOR (pruebas en vivo sin ratón ni teclado): cada línea escrita en
/// `&lt;proyecto&gt;/Temp/editor-inbox.txt` se ejecuta en el editor y deja `[EditorInbox] …` en el log.
/// Comandos: `play`, `stop`, `refresh`, `menu &lt;ruta del menú&gt;` (p. ej. `menu Tools/Cold Sanctuary/Verify (compilar + blockout + guardar)`),
/// `open &lt;ruta de escena&gt;`. Complementa a `Temp/spell-inbox.txt` (hechizos en Play) — ver SpellConsole.
/// </summary>
[InitializeOnLoad]
static class EditorInbox
{
    static double _next;
    static string Path => System.IO.Path.Combine(Application.dataPath, "..", "Temp", "editor-inbox.txt");

    static EditorInbox() { EditorApplication.update += Poll; }

    static void Poll()
    {
        if (EditorApplication.timeSinceStartup < _next) return;
        _next = EditorApplication.timeSinceStartup + 0.5;
        if (!File.Exists(Path)) return;
        string[] lines;
        try { lines = File.ReadAllLines(Path); File.Delete(Path); }
        catch (IOException) { return; }

        foreach (string raw in lines)
        {
            string line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#")) continue;
            Debug.Log($"[EditorInbox] {line}");
            try { Run(line); }
            catch (System.Exception e) { Debug.LogError($"[EditorInbox] «{line}» falló: {e.Message}"); }
        }
    }

    static void Run(string line)
    {
        int sp = line.IndexOf(' ');
        string cmd = (sp < 0 ? line : line.Substring(0, sp)).ToLowerInvariant();
        string arg = sp < 0 ? "" : line.Substring(sp + 1).Trim();
        switch (cmd)
        {
            case "play":    EditorApplication.isPlaying = true; break;
            case "test":    PlayWithTests(); break;   // Play con las pruebas (sin esto, un Play es solo para jugar)
            case "stop":    EditorApplication.isPlaying = false; break;
            case "refresh": AssetDatabase.Refresh(); break;
            case "menu":    if (!EditorApplication.ExecuteMenuItem(arg)) Debug.LogWarning($"[EditorInbox] menú no encontrado: {arg}"); break;
            case "open":    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(arg); break;
            case "save":    UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes(); Debug.Log("[EditorInbox] escenas abiertas guardadas."); break;
            default:        Debug.LogWarning($"[EditorInbox] comando desconocido: {cmd}"); break;
        }
    }

    /// <summary>Play CON las pruebas automáticas (TestRunner solo corre si existe su archivo de petición).</summary>
    [MenuItem("Tools/Cold Sanctuary/Jugar con las pruebas")]
    public static void PlayWithTests()
    {
        System.IO.File.WriteAllText(ColdSanctuary.TestRunner.RequestFile, "");
        EditorApplication.isPlaying = true;
    }
}
