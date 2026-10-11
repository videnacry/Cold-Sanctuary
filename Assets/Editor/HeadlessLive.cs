using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Prueba EN VIVO automatizada: abre la escena, monta el blockout, **entra en Play**, deja correr el
/// mundo N segundos y sale, todo desde la línea de comandos. Sirve para recorrer el checklist de
/// `docs/testing-checklist.md` sin depender del bridge MCP (que no conecta de forma fiable).
///
/// Uso (modo GUI, NO -batchmode — y nunca los dos a la vez):
/// <code>
/// Unity.exe -projectPath &lt;proyecto&gt; -logFile &lt;log&gt; -executeMethod HeadlessLive.RunLive \
///           -liveSeconds 180 -liveReport &lt;ruta-del-informe.log&gt;
/// </code>
///
/// DOS LECCIONES DE UN INTENTO FALLIDO (2026-10-11), por las que esto está escrito así:
///  1. **No usar `EditorApplication.delayCall`** para retomar tras el cambio a Play: no volvió a
///     disparar después del domain reload, así que el `Tick` nunca se registró, la fase se quedó en
///     "entering" y —al no correr el `Tick`— tampoco corrió su watchdog: el Editor se quedó colgado
///     indefinidamente. Aquí la suscripción a `EditorApplication.update` se hace **directamente en el
///     constructor estático**, que sí corre en cada domain reload.
///  2. **No fiarse del `-logFile` de Unity**: buffea y puede pasar minutos sin volcar a disco, así que
///     desde fuera no se distingue "trabajando" de "colgado". Este script escribe **su propio informe**
///     (`-liveReport`) con `flush` en cada línea, capturando además TODA la consola
///     (`Application.logMessageReceivedThreaded`) → ahí salen los `[TEST]`/`[ECO]` garantizados.
///
/// El informe lleva marcas `[LIVE]`; los tests del proyecto salen con `[TEST]` (`TestRunner_AUTO`) y la
/// misión-observación con `[ECO]`. El proceso se cierra solo al acabar, o por **watchdog** si algo se
/// atasca (nunca se queda colgado para siempre).
/// </summary>
[InitializeOnLoad]
public static class HeadlessLive
{
    const string PhaseKey    = "ColdSanctuary_HeadlessLive_Phase";
    const string DeadlineKey = "ColdSanctuary_HeadlessLive_Deadline";
    const string SecondsKey  = "ColdSanctuary_HeadlessLive_Seconds";
    const string ReportKey   = "ColdSanctuary_HeadlessLive_Report";
    // El auto-build de SampleSceneBuilder se dispara en cada domain reload si la escena activa es
    // "SampleScene" y este flag está a false. Aquí el build se controla explícitamente.
    const string BuilderAutoRanKey = "ColdSanctuary_SampleSceneBuilder_AutoRan";

    const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
    // Margen para que Play arranque (compilar + domain reload + primer frame).
    const double EnterPlaySeconds = 300;
    // Margen para que salir de Play y cerrar se complete.
    const double LeaveSeconds = 120;

    static bool _hooked;
    static DateTime _lastBeat = DateTime.MinValue;

    static HeadlessLive()
    {
        // Suscripción DIRECTA (sin delayCall): el ctor estático corre en cada domain reload, también en
        // el de entrar/salir de Play, así que el Tick siempre queda vivo y su watchdog siempre vigila.
        if (EditorPrefs.GetString(PhaseKey, "") == "") return;
        Hook();
        Report($"[LIVE] (reload) fase='{EditorPrefs.GetString(PhaseKey, "")}' isPlaying={EditorApplication.isPlaying}");
    }

    static void Hook()
    {
        if (_hooked) return;
        _hooked = true;
        EditorApplication.update += Tick;
        Application.logMessageReceivedThreaded -= OnLog;
        Application.logMessageReceivedThreaded += OnLog;
    }

    static void Unhook()
    {
        EditorApplication.update -= Tick;
        Application.logMessageReceivedThreaded -= OnLog;
        _hooked = false;
    }

    /// <summary>
    /// Deja el Editor LISTO para que alguien pulse Play: abre la escena, monta el blockout, lo guarda y
    /// **arma la captura de consola** al informe — pero NO entra en Play ni cierra Unity.
    ///
    /// Existe porque automatizar el Play desde la línea de comandos no funciona en este entorno: con el
    /// Editor en segundo plano el **player loop corre UN frame y se detiene** (sale un único `[ECO]`, el
    /// `WaitForSeconds` del `TestRunner` no vence, y ni `Application.runInBackground=true` ni traer la
    /// ventana al frente lo arreglan; el proceso queda respondiendo con la CPU plana). Con una pulsación
    /// real de Play el bucle corre normal y los `[TEST]` salen solos al informe.
    /// </summary>
    [MenuItem("Tools/Cold Sanctuary/Preparar prueba en vivo (montar y esperar Play)")]
    public static void PrepareOnly()
    {
        string report = ReadStringArg("-liveReport", Path.Combine(Path.GetTempPath(), "cold-sanctuary-live.log"));
        EditorPrefs.SetString(ReportKey, report);
        EditorPrefs.SetString(PhaseKey, "armed");   // mantiene la captura viva entre domain reloads
        EditorPrefs.SetBool(BuilderAutoRanKey, true);
        try { File.WriteAllText(report, ""); } catch { }
        Hook();

        Report("[LIVE] PREPARAR — los scripts COMPILAN. Se monta el blockout y se espera una pulsación de Play.");
        try
        {
            EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
            Report($"[LIVE] escena abierta: {SampleScenePath}");
            SampleSceneBuilder.Build();
            Report("[LIVE] SampleSceneBuilder.Build() completado.");
            Inventory();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Report("[LIVE] escena guardada.");
        }
        catch (Exception e) { Report($"[LIVE] FALLO preparando: {e}"); return; }

        Report("[LIVE] LISTO. Pulsa Play: los [TEST] del TestRunner_AUTO y los [ECO] se escribirán aquí solos.");
    }

    [MenuItem("Tools/Cold Sanctuary/Prueba EN VIVO (blockout + Play + salir)")]
    public static void RunLive()
    {
        int seconds = ReadIntArg("-liveSeconds", 180);
        string report = ReadStringArg("-liveReport", Path.Combine(Path.GetTempPath(), "cold-sanctuary-live.log"));

        EditorPrefs.SetString(ReportKey, report);
        EditorPrefs.SetInt(SecondsKey, seconds);
        EditorPrefs.SetString(PhaseKey, "building");
        EditorPrefs.SetBool(BuilderAutoRanKey, true);
        try { File.WriteAllText(report, ""); } catch { /* el informe es best-effort */ }
        Hook();

        Report($"[LIVE] inicio — los scripts COMPILAN (si no, este método no se invocaría). Play previsto: {seconds}s.");

        try
        {
            EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
            Report($"[LIVE] escena abierta: {SampleScenePath}");
        }
        catch (Exception e) { Report($"[LIVE] FALLO al abrir {SampleScenePath}: {e}"); Finish(1); return; }

        try
        {
            SampleSceneBuilder.Build();
            Report("[LIVE] SampleSceneBuilder.Build() completado.");
        }
        catch (Exception e) { Report($"[LIVE] FALLO en SampleSceneBuilder.Build(): {e}"); Finish(1); return; }

        Inventory();

        try { EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene()); Report("[LIVE] escena guardada."); }
        catch (Exception e) { Report($"[LIVE] aviso: no se pudo guardar la escena: {e.Message}"); }

        // IMPRESCINDIBLE para una corrida sin nadie delante: el Editor que lanzamos desde la línea de
        // comandos NUNCA tiene el foco, y con "Run In Background" desactivado Unity **pausa el Play**
        // en cuanto pierde el foco. Síntoma exacto (visto dos veces el 2026-10-11): corre UN frame
        // (sale un único [ECO]), el `WaitForSeconds(1.6f)` del TestRunner no vence jamás, el
        // `EditorApplication.update` deja de disparar, y el proceso queda RESPONDIENDO pero con la CPU
        // plana. Parece un cuelgue y no lo es: es el Play en pausa por falta de foco.
        // OJO: ponerlo solo en memoria NO basta. `EnterPlaymode()` dispara un domain reload que
        // **recarga ProjectSettings del disco**, y el cambio sin guardar se pierde (visto: el informe
        // decía "se activa" y `ProjectSettings.asset` seguía con `runInBackground: 0`, con el Play
        // igual de congelado). Hay que PERSISTIRLO con SaveAssets antes del reload.
        if (!PlayerSettings.runInBackground)
        {
            PlayerSettings.runInBackground = true;
            AssetDatabase.SaveAssets();
            Report("[LIVE] 'Run In Background' estaba DESACTIVADO — se activa y se GUARDA (sin él, el Play se pausa al no tener foco).");
        }

        SetDeadline(EnterPlaySeconds);
        EditorPrefs.SetString(PhaseKey, "entering");
        Report("[LIVE] entrando en Play...");
        EditorApplication.EnterPlaymode();
    }

    static void Tick()
    {
        string phase = EditorPrefs.GetString(PhaseKey, "");
        if (phase == "") { Unhook(); return; }

        bool playing = EditorApplication.isPlaying;
        DateTime now = DateTime.UtcNow;
        bool expired = now > GetDeadline();

        switch (phase)
        {
            case "entering":
                if (playing)
                {
                    int seconds = EditorPrefs.GetInt(SecondsKey, 180);
                    SetDeadline(seconds);
                    EditorPrefs.SetString(PhaseKey, "playing");

                    // DIAGNÓSTICO del tiempo de juego. Una corrida del 2026-10-11 entró en Play pero
                    // NI el TestRunner ni el [ECO] progresaron: el `WaitForSeconds(1.6f)` del runner no
                    // vencía nunca y el [ECO] logueó exactamente UNA vez. Las dos cosas dependen del
                    // tiempo ESCALADO (`Time.time`/`WaitForSeconds`), así que si está congelado
                    // (timeScale 0 o Play en pausa) no corre ningún test. Se mide y se deja en el informe.
                    // `Application.runInBackground` es el equivalente de RUNTIME y se puede poner ya
                    // dentro del Play: surte efecto en el acto, sin depender de que el PlayerSetting
                    // sobreviviera al domain reload. Es el cinturón que complementa al tirante.
                    Application.runInBackground = true;
                    Report($"[LIVE] tiempo de juego: timeScale={Time.timeScale} isPaused={EditorApplication.isPaused} " +
                           $"runInBackground={Application.runInBackground} (PlayerSetting={PlayerSettings.runInBackground}) frames={Time.frameCount}");
                    if (EditorApplication.isPaused)
                    {
                        EditorApplication.isPaused = false;
                        Report("[LIVE] AVISO: Play estaba EN PAUSA — se reanuda para que los tests puedan correr.");
                    }
                    if (Time.timeScale <= 0.0001f)
                    {
                        Time.timeScale = 1f;
                        Report("[LIVE] AVISO: timeScale era 0 (juego congelado) — se fuerza a 1. Esto es un HALLAZGO: " +
                               "al dar Play el mundo arranca detenido y ningún test basado en tiempo puede correr.");
                    }

                    Report($"[LIVE] EN PLAY. Dejando correr el mundo {seconds}s...");
                }
                else if (expired)
                {
                    Report("[LIVE] FALLO (watchdog): Play no arrancó dentro del margen.");
                    Finish(1);
                }
                break;

            case "playing":
                // Latido cada ~30 s de reloj real: deja en el informe si el tiempo de JUEGO avanza
                // (Time.time) o está clavado. Sin esto no se distingue "el mundo corre pero los tests
                // no logean" de "el mundo está detenido".
                if (playing && (DateTime.UtcNow - _lastBeat).TotalSeconds >= 30)
                {
                    _lastBeat = DateTime.UtcNow;
                    Report($"[LIVE] latido: Time.time={Time.time:F1}s timeScale={Time.timeScale} frames={Time.frameCount}");
                }
                if (!playing)
                {
                    Report("[LIVE] aviso: se salió de Play antes de tiempo.");
                    EditorPrefs.SetString(PhaseKey, "leaving");
                    SetDeadline(LeaveSeconds);
                }
                else if (expired)
                {
                    Report("[LIVE] tiempo cumplido — saliendo de Play.");
                    EditorPrefs.SetString(PhaseKey, "leaving");
                    SetDeadline(LeaveSeconds);
                    EditorApplication.ExitPlaymode();
                }
                break;

            case "leaving":
                if (!playing) { Report("[LIVE] Play terminado. FIN."); Finish(0); }
                else if (expired) { Report("[LIVE] FALLO (watchdog): no se pudo salir de Play."); Finish(1); }
                break;
        }
    }

    static void Inventory()
    {
        try
        {
            GameObject root = GameObject.Find("LevelDressing_AUTO");
            if (root == null) { Report("[LIVE] FALLO: no existe 'LevelDressing_AUTO' tras Build()."); return; }
            Report($"[LIVE] LevelDressing_AUTO con {root.GetComponentsInChildren<Transform>(true).Length - 1} descendientes.");
            foreach (string n in new[] { "TraceField_AUTO", "Clock_AUTO", "TestRunner_AUTO", "EcoObservation_AUTO" })
                Report($"[LIVE]   {(GameObject.Find(n) != null ? "OK   " : "FALTA")} {n}");
            Report($"[LIVE]   {(GameObject.FindGameObjectWithTag("Player") != null ? "OK   " : "FALTA")} tag 'Player'");
        }
        catch (Exception e) { Report($"[LIVE] FALLO en el inventario: {e}"); }
    }

    // ── Informe propio, con flush por línea (el -logFile de Unity buffea) ───────────────────────────
    static void OnLog(string message, string stackTrace, LogType type)
    {
        // Las propias líneas [LIVE] las escribe ya `Report`; si se dejaran pasar aquí (su Debug.Log
        // vuelve por este callback) saldrían DUPLICADAS en el informe.
        if (message.Contains("[LIVE]")) return;
        // Solo lo que sirve para el checklist + cualquier error: así el informe queda legible.
        bool interesting = type == LogType.Error || type == LogType.Exception || type == LogType.Assert
                           || message.Contains("[TEST]") || message.Contains("[ECO]");
        if (!interesting) return;
        string line = type == LogType.Log ? message : $"{type}: {message}";
        Write(line);
        if (type == LogType.Exception || type == LogType.Assert) Write(stackTrace);
    }

    static void Report(string line) { Debug.Log(line); Write(line); }   // Debug.Log → también al Editor.log

    static void Write(string line)
    {
        string path = EditorPrefs.GetString(ReportKey, "");
        if (string.IsNullOrEmpty(path)) return;
        try
        {
            // append + flush en cada línea: desde fuera siempre se ve el estado real.
            using var w = new StreamWriter(path, append: true);
            w.WriteLine($"{DateTime.Now:HH:mm:ss} {line}");
            w.Flush();
        }
        catch { /* nunca romper la corrida por el informe */ }
    }

    static void SetDeadline(double seconds)
        => EditorPrefs.SetString(DeadlineKey, DateTime.UtcNow.AddSeconds(seconds).ToString("o"));

    static DateTime GetDeadline()
        => DateTime.TryParse(EditorPrefs.GetString(DeadlineKey, ""), null,
               System.Globalization.DateTimeStyles.RoundtripKind, out DateTime d) ? d : DateTime.UtcNow;

    static void Finish(int code)
    {
        Report($"[LIVE] RESULTADO: {(code == 0 ? "corrida completada" : "corrida con fallos")} (exit {code}).");
        Unhook();
        EditorPrefs.DeleteKey(PhaseKey);
        EditorPrefs.DeleteKey(DeadlineKey);
        EditorPrefs.DeleteKey(SecondsKey);
        EditorPrefs.DeleteKey(ReportKey);
        EditorApplication.Exit(code);
    }

    static int ReadIntArg(string name, int fallback)
        => int.TryParse(ReadStringArg(name, null), out int v) && v > 0 ? v : fallback;

    static string ReadStringArg(string name, string fallback)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == name) return args[i + 1];
        return fallback;
    }
}
