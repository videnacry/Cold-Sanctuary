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
    /// <summary>La Sala de Eras: ya NO es anfitriona de la lista de Inicio (solo trae el explorador, sin Kushal); los microcosmos van sobre el Santuario.</summary>
    public const string HostPath = "Assets/Scenes/Sala_de_Eras.unity";

    /// <summary>Kushal entra ya miembro del santuario (el hechizo admin «miembro» al cargar).</summary>
    public static bool KushalMember;
    /// <summary>Microcosmos o virtual que se carga en aditivo sobre la escena elegida (MobWorldLoader); null si ninguno.</summary>
    public static string MicroScene;
    /// <summary>La Magnate guarda los puestos (casilla de Inicio): sin ella, nadie es devuelto a su puesto y Kushal explora libre.</summary>
    public static bool MagnateGuards = true;

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

    /// <summary>¿Está cargando una escena (la pantalla negra de «Cargando…»)?</summary>
    public static bool Loading => _inst != null && _inst._loading != null;

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
        MagnateWarden.GuardPosts = LaunchOptions.MagnateGuards;
        // La VELOCIDAD elegida en Inicio (×1 ×2 ×4 ×8): cada escena cargada desde Inicio arranca con ella. Con las pruebas
        // pedidas no: cada prueba fija su propio ritmo y lo restaura.
        if (!ColdSanctuary.TestRunner.Requested) GameSpeed.Apply();
        RerunBoots();
        if (LaunchOptions.KushalMember)
        {
            // Antes del primer Update: ningún iniciador lo coge; un frame después, el resto de «miembro» (cuerpo y cámara).
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null && p.TryGetComponent(out SanctuaryOnboarding ob)) { ob.introMissionsDone = ob.introMissionsRequired; ob.apt = ob.presented = ob.initiated = ob.kitchenTrained = ob.gardenTrained = true; }
            if (p != null && p.TryGetComponent(out HealthRecord hr)) { hr.analyzed = true; hr.vaccinated = true; }
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

    Rect _pauseRect; Vector2 _pauseFor;
    /// <summary>(revisión PB-169) El botón de pausa en el móvil: a la derecha, justo encima del botón más alto del mando (nunca encima de él).</summary>
    public static Rect MobilePauseRect(float w, float h)
    {
        TouchControls.Layout lay = TouchControls.Compute(w, h);
        float top = h;
        foreach (Rect r in lay.rects.Values) top = Mathf.Min(top, r.y);
        float bh = 44f * lay.dp, bw = 150f * lay.dp, margin = 14f * lay.dp;
        return new Rect(w - margin - bw, Mathf.Max(8f * lay.dp, top - 8f * lay.dp - bh), bw, bh);
    }

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
        if (GameKeys.UiKeyDown(PauseKey) && !SpellConsole.Capturing) SetPaused(!_paused);
    }

    GUIStyle _big;
    bool _laid;
    Vector2 _pauseScroll;

    void OnGUI()
    {
        CsTheme.Use();   // (2026-11-19) el tema de CodeShip para los menús (UI/CsTheme.cs)
        GUI.depth = -2000;
        if (_loading != null)
        {
            GUI.color = Color.black; GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = Color.white;
            if (_big == null) _big = new GUIStyle(GUI.skin.label) { fontSize = 26, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(0, Screen.height * 0.5f - 30f, Screen.width, 60f), $"Cargando {StartScreen.Pretty(_loading)}…  {_progress * 100f:0}%", _big);
            return;
        }
        if (InInicio || ColdSanctuary.TestRunner.Running) return;
        bool drawSheet = CsUi.Draw(ref _laid, _paused);
        if (!_paused)
        {
            if (CsUi.ModalOpen) return;   // (PB-169) con una ventana abierta, solo ella y su ✕
            string tag = Time.timeScale > 1.01f ? $"  ({GameSpeed.Label(Time.timeScale)})" : "";
            if (!CsUi.Mobile) { if (GUI.Button(new Rect(Screen.width - 190f, Screen.height - 72f, 185f, 24f), "F10: pausa · Inicio" + tag)) SetPaused(true); return; }
            // (revisión PB-169) En el móvil, abajo a la derecha caería sobre el joystick MIRAR (y, al dibujarse antes, se llevaría su toque).
            // Si el ⌂ del mando ya es la pausa, sobra; si el ⌂ abre el menú ☰ del microcosmos, va encima de los botones de la derecha.
            if (!MicroRootMenu.Available) return;
            if (_pauseFor.x != Screen.width || _pauseFor.y != Screen.height) { _pauseRect = MobilePauseRect(Screen.width, Screen.height); _pauseFor = new Vector2(Screen.width, Screen.height); }
            CsUi.Prepare();
            if (GUI.Button(_pauseRect, "Pausa · Inicio" + tag, CsUi.PillStyle)) SetPaused(true);
            return;
        }
        if (!drawSheet) return;
        // (2026-11-20, PB-169) Los «Ajustes generales» de CodeShip (settings.css): la hoja con su cabecera en versalitas y la ✕, cada
        // opción una fila con su valor a la derecha; se abre con el botón de inicio del mando o F10.
        Rect inner = CsUi.SheetFrame();
        float y = inner.y;
        if (CsUi.SheetHead(inner, ref y, "Ajustes generales", $"{StartScreen.Pretty(SceneManager.GetActiveScene().name)} · pausa (el tiempo está parado)", "Seguir") >= 0) { SetPaused(false); return; }
        GUILayout.BeginArea(new Rect(inner.x, y, inner.width, inner.yMax - y));
        _pauseScroll = GUILayout.BeginScrollView(_pauseScroll);
        int act = 0;
        if (CsUi.SetRow("Reiniciar esta escena", "Vuelve a cargarla desde el principio", "", true)) act = 1;
        if (CsUi.SetRow("Volver a Inicio", "La pantalla de inicio, sin salir del juego", "", true)) act = 2;
        bool gd = MagnateWarden.GuardPosts;
        if (CsUi.SetRow("La Magnate guarda los puestos", "Cubre los puestos que quedan sin nadie", gd ? "Sí" : "No", gd)) { gd = !gd; MagnateWarden.GuardPosts = LaunchOptions.MagnateGuards = gd; PlayerPrefs.SetInt("Inicio.MagnateGuarda", gd ? 1 : 0); }
        CsUi.SectionLabel("Velocidad del tiempo al seguir");
        // VELOCIDAD del tiempo al seguir (GameSpeed): se guarda como la de Inicio.
        Rect sr = GUILayoutUtility.GetRect(inner.width, CsUi.Touch, GUILayout.ExpandWidth(true));
        if (GameSpeed.Buttons(sr, _oldScale, out float sp, CsUi.ChipStyle)) { _oldScale = sp; GameSpeed.Chosen = sp; }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
        if (act == 1) Restart(); else if (act == 2) BackToInicio();
    }
}
