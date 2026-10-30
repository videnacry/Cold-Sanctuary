using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lo elegido en INICIO para la próxima escena (Berón 2026-10-30: «entrar con Kushal iniciado, saltando todas las
/// iniciaciones», «elegir la escena desde un menú»). Estático: sobrevive a la carga de escena; lo aplica <see cref="SceneLauncher"/>.
/// </summary>
public static class LaunchOptions
{
    public const string InicioPath = "Assets/Scenes/Inicio.unity";
    public const string SanctuaryPath = "Assets/Scenes/SampleScene.unity";
    /// <summary>Anfitriona ligera de los microcosmos y virtuales (se cargan en aditivo encima, como desde la Sala de Eras).</summary>
    public const string HostPath = "Assets/Scenes/Sala_de_Eras.unity";

    /// <summary>Kushal entra ya miembro del santuario (el hechizo admin «miembro» al cargar).</summary>
    public static bool KushalMember;
    /// <summary>Microcosmos o virtual que se carga en aditivo sobre la escena elegida (MobWorldLoader); null si ninguno.</summary>
    public static string MicroScene;

    public static bool InicioInBuild => SceneUtility.GetBuildIndexByScenePath(InicioPath) >= 0;
}

/// <summary>
/// CAMBIO DE ESCENA EN UNA MISMA PARTIDA: carga lo elegido en Inicio y vuelve a Inicio con F10 o el botón de pausa, sin salir
/// del Play. Antes de cada carga deja la partida como recién abierta: tiempo a 1, cursor libre, fuera los objetos persistentes
/// del juego (DontDestroyOnLoad) y, tras cargar, vuelve a correr los arranques <c>[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]</c>
/// (Unity solo los corre en la primera escena del Play: HudGate, la jornada, la voz de Kushal, el restaurante…).
/// Se instala solo en cualquier escena si Inicio está en Build Settings; con Play desde SampleScene todo sigue igual.
/// </summary>
public class SceneLauncher : MonoBehaviour
{
    public const KeyCode PauseKey = KeyCode.F10;   // libre: F1 es HudGate y Escape lo usan los diálogos y el cursor

    static SceneLauncher _inst;
    static List<MethodInfo> _boots;
    bool _reboot, _paused;
    float _oldScale = 1f;
    string _loading;
    float _progress;
    // La última carga, para «Reiniciar esta escena».
    string _lastPath, _lastMicro;
    bool _lastMember;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install() { if (LaunchOptions.InicioInBuild) Ensure(); }

    public static SceneLauncher Ensure()
    {
        if (_inst != null) return _inst;
        var go = new GameObject("SceneLauncher");
        DontDestroyOnLoad(go);
        _inst = go.AddComponent<SceneLauncher>();
        _inst._lastPath = SceneManager.GetActiveScene().path;
        return _inst;
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    // ── API ──────────────────────────────────────────────────────────────────────

    /// <summary>Carga <paramref name="scenePath"/> (sola) y, si se da, el microcosmos <paramref name="micro"/> encima.</summary>
    public static void Load(string scenePath, bool kushalMember, string micro = null)
    {
        SceneLauncher l = Ensure();
        if (l._loading != null) return;
        LaunchOptions.KushalMember = kushalMember;
        LaunchOptions.MicroScene = micro;
        if (scenePath != LaunchOptions.InicioPath) { l._lastPath = scenePath; l._lastMicro = micro; l._lastMember = kushalMember; }
        l.StartCoroutine(l.Go(scenePath));
    }

    public static void BackToInicio() => Load(LaunchOptions.InicioPath, false);

    /// <summary>Repite la última carga (la escena de ahora, desde cero).</summary>
    public static void Restart()
    {
        SceneLauncher l = Ensure();
        Load(string.IsNullOrEmpty(l._lastPath) ? SceneManager.GetActiveScene().path : l._lastPath, l._lastMember, l._lastMicro);
    }

    /// <summary>Destruye los objetos persistentes del juego (los que llevan algún script nuestro), menos este. Los de Unity y
    /// los paquetes se quedan. Los singletons se recrean solos (Ensure/Bootstrap) o con los arranques que se vuelven a correr.</summary>
    public static void CleanPersistent()
    {
        var probe = new GameObject("SceneLauncher_probe");
        DontDestroyOnLoad(probe);
        Assembly ours = typeof(SceneLauncher).Assembly;
        int n = 0;
        foreach (GameObject g in probe.scene.GetRootGameObjects())
        {
            if (g == probe || (_inst != null && g == _inst.gameObject)) continue;
            foreach (MonoBehaviour mb in g.GetComponentsInChildren<MonoBehaviour>(true))
                if (mb != null && mb.GetType().Assembly == ours) { Destroy(g); n++; break; }
        }
        Destroy(probe);
        if (n > 0) Debug.Log($"[Inicio] {n} objetos persistentes del juego fuera antes de cambiar de escena.");
    }

    /// <summary>La partida como recién abierta: tiempo normal, cursor libre, teclado del juego sin menús, sin admin.</summary>
    static void ResetGlobals()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        AudioListener.pause = false;
        GameKeys.MenuOpen = false;
        SpellConsole.Admin = false;
    }

    // ── Carga ────────────────────────────────────────────────────────────────────

    IEnumerator Go(string path)
    {
        if (_paused) SetPaused(false);
        _loading = System.IO.Path.GetFileNameWithoutExtension(path);
        _progress = 0f;
        yield return null;   // un frame con «Cargando…» en pantalla
        ResetGlobals();
        CleanPersistent();
        _reboot = true;
        Debug.Log($"[Inicio] Cargando {path}" + (LaunchOptions.KushalMember ? " (Kushal iniciado)" : "") + (LaunchOptions.MicroScene != null ? $" + {LaunchOptions.MicroScene}" : "") + ".");
        AsyncOperation op = SceneManager.LoadSceneAsync(path, LoadSceneMode.Single);
        if (op == null) { Debug.LogError($"[Inicio] No se pudo cargar {path}: ¿está en Build Settings?"); _reboot = false; _loading = null; yield break; }
        float until = Time.realtimeSinceStartup + 300f;
        while (!op.isDone && Time.realtimeSinceStartup < until) { _progress = op.progress; yield return null; }
        if (!op.isDone) Debug.LogError($"[Inicio] {path} no terminó de cargar en 300 s.");
        _loading = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single || !_reboot) return;
        _reboot = false;
        if (scene.path == LaunchOptions.InicioPath) return;   // Inicio no lleva juego: la pantalla limpia lo persistente
        RerunBoots();
        if (LaunchOptions.KushalMember)
        {
            // Antes del primer Update: ningún iniciador lo coge; un frame después, el resto de «miembro» (cuerpo y cámara).
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null && p.TryGetComponent(out SanctuaryOnboarding ob)) { ob.introMissionsDone = ob.introMissionsRequired; ob.apt = ob.presented = ob.initiated = ob.kitchenTrained = ob.gardenTrained = true; }
            StartCoroutine(MemberLater());
        }
        if (!string.IsNullOrEmpty(LaunchOptions.MicroScene)) StartCoroutine(EnterMicro(LaunchOptions.MicroScene));
    }

    /// <summary>Los arranques AfterSceneLoad del juego, como si la escena fuera la primera del Play.</summary>
    static void RerunBoots()
    {
        if (_boots == null)
        {
            _boots = new List<MethodInfo>();
            foreach (System.Type t in typeof(SceneLauncher).Assembly.GetTypes())
            {
                if (t == typeof(SceneLauncher) || t.ContainsGenericParameters) continue;
                foreach (MethodInfo m in t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    var a = m.GetCustomAttribute<RuntimeInitializeOnLoadMethodAttribute>();
                    if (a != null && a.loadType == RuntimeInitializeLoadType.AfterSceneLoad && m.GetParameters().Length == 0) _boots.Add(m);
                }
            }
        }
        foreach (MethodInfo m in _boots)
        {
            try { m.Invoke(null, null); }
            catch (System.Exception e) { Debug.LogError($"[Inicio] arranque {m.DeclaringType.Name}.{m.Name} falló: {e.InnerException ?? e}"); }
        }
    }

    IEnumerator MemberLater()
    {
        yield return null;
        yield return null;
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        string r = SpellConsole.MakeMember(p);
        Debug.Log("[Inicio] " + (r ?? (p == null ? "Kushal iniciado: no hay jugador en esta escena." : $"Kushal iniciado: «{p.name}» no tiene onboarding (ya es del santuario).")));
    }

    IEnumerator EnterMicro(string micro)
    {
        // Que el jugador de la anfitriona exista y haya hecho su Start (máx. ~10 s).
        float until = Time.realtimeSinceStartup + 10f;
        int frames = 0;
        while ((frames++ < 3 || GameObject.FindGameObjectWithTag("Player") == null) && Time.realtimeSinceStartup < until) yield return null;
        if (GameObject.FindGameObjectWithTag("Player") == null) Debug.LogWarning($"[Inicio] {micro}: la anfitriona no tiene jugador; se carga igual.");
        MobWorldLoader.Instance.EnterMobWorld(micro);
        LaunchOptions.MicroScene = null;
    }

    // ── Pausa (F10 o el botón) ──────────────────────────────────────────────────

    bool InInicio => SceneManager.GetActiveScene().path == LaunchOptions.InicioPath;

    void SetPaused(bool on)
    {
        if (on == _paused) return;
        _paused = on;
        if (on)
        {
            _oldScale = Time.timeScale; Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            GameKeys.MenuOpen = true;   // el teclado del juego, mudo mientras
        }
        else { Time.timeScale = _oldScale; GameKeys.MenuOpen = false; }
    }

    void Update()
    {
        if (_loading != null || InInicio) return;
        if (Input.GetKeyDown(PauseKey) && !SpellConsole.Capturing) SetPaused(!_paused);
    }

    GUIStyle _big;

    void OnGUI()
    {
        GUI.depth = -2000;
        if (_loading != null)
        {
            GUI.color = Color.black; GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = Color.white;
            if (_big == null) _big = new GUIStyle(GUI.skin.label) { fontSize = 26, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(0, Screen.height * 0.5f - 30f, Screen.width, 60f), $"Cargando {StartScreen.Pretty(_loading)}…  {_progress * 100f:0}%", _big);
            return;
        }
        if (InInicio || ColdSanctuary.TestRunner.Running) return;
        if (!_paused)
        {
            if (GUI.Button(new Rect(Screen.width - 190f, Screen.height - 72f, 185f, 24f), "F10: pausa · Inicio")) SetPaused(true);
            return;
        }
        GUI.color = new Color(0f, 0f, 0f, 0.55f); GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = Color.white;
        var r = new Rect(Screen.width * 0.5f - 160f, Screen.height * 0.5f - 120f, 320f, 240f);
        GUI.Box(r, "Pausa — " + StartScreen.Pretty(SceneManager.GetActiveScene().name));
        if (GUI.Button(new Rect(r.x + 20f, r.y + 40f, 280f, 40f), "Seguir (F10)")) SetPaused(false);
        if (GUI.Button(new Rect(r.x + 20f, r.y + 90f, 280f, 40f), "Reiniciar esta escena")) Restart();
        if (GUI.Button(new Rect(r.x + 20f, r.y + 140f, 280f, 40f), "Volver a Inicio")) BackToInicio();
        GUI.Label(new Rect(r.x + 20f, r.y + 192f, 280f, 40f), "El tiempo está parado mientras tanto.");
    }
}
