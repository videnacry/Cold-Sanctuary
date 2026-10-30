using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PANTALLA DE INICIO (escena Inicio, la primera del build; Berón 2026-10-30): no carga nada pesado hasta elegir.
/// Una casilla «Kushal iniciado» (miembro, sin examen ni iniciaciones) que vale para TODO: el botón «Entrar al Santuario» y
/// la LISTA de todas las escenas de Build Settings por grupos (Santuario, Microcosmos, Pruebas, Virtual, Otros).
/// Los microcosmos y virtuales se cargan en aditivo sobre el Santuario, como desde el tanque (<see cref="Launch"/>).
/// En el juego, F10 o el botón de pausa vuelven aquí (<see cref="SceneLauncher"/>).
/// </summary>
public class StartScreen : MonoBehaviour
{
    static readonly string[] Groups = { "Santuario", "Microcosmos", "Pruebas", "Virtual", "Otros" };

    class Entry { public string path, name, label, group; }

    readonly List<Entry> _entries = new List<Entry>();
    readonly Dictionary<string, bool> _open = new Dictionary<string, bool>();
    bool _kushal, _guard;
    Vector2 _scroll;
    GUIStyle _title, _sub, _head, _btn, _big, _note, _tog;

    void Start()
    {
        SceneLauncher.Ensure();
        SceneLauncher.CleanPersistent();   // los arranques de Unity del primer Play también corren aquí: fuera
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        _kushal = PlayerPrefs.GetInt("Inicio.Kushal", 1) == 1;
        _guard = PlayerPrefs.GetInt("Inicio.MagnateGuarda", 1) == 1;
        LaunchOptions.MagnateGuards = _guard;
        foreach (string g in Groups) _open[g] = PlayerPrefs.GetInt("Inicio.Abierto." + g, g == "Santuario" || g == "Pruebas" ? 1 : 0) == 1;

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string n = System.IO.Path.GetFileNameWithoutExtension(path);
            string g = GroupOf(path, n);
            if (g == null) continue;
            _entries.Add(new Entry { path = path, name = n, label = Pretty(n), group = g });
        }
        _entries.Sort((a, b) => a.group != b.group ? System.Array.IndexOf(Groups, a.group).CompareTo(System.Array.IndexOf(Groups, b.group)) : a.name == "SampleScene" ? -1 : b.name == "SampleScene" ? 1 : string.Compare(a.label, b.label, System.StringComparison.InvariantCultureIgnoreCase));
        Debug.Log($"[Inicio] Pantalla de inicio: {_entries.Count} escenas en Build Settings.");
    }

    static string GroupOf(string path, string n)
    {
        if (path == LaunchOptions.InicioPath) return null;
        if (path == LaunchOptions.SanctuaryPath) return "Santuario";
        if (path.Contains("/Pruebas/")) return "Pruebas";
        if (n.StartsWith("Microcosmos_") || n.StartsWith("MobWorld_")) return "Microcosmos";
        if (n.StartsWith("Virtual_")) return "Virtual";
        return "Otros";
    }

    /// <summary>¿Se carga en aditivo sobre una anfitriona (no tiene jugador ni cámara propios)?</summary>
    static bool Additive(string group) => group == "Microcosmos" || group == "Virtual";

    /// <summary>
    /// Entra en la escena <paramref name="path"/> de la lista como lo hace el botón. Los microcosmos y virtuales no traen
    /// jugador ni cámara: se cargan en aditivo SIEMPRE sobre el Santuario (SampleScene), como desde el tanque, para que Kushal
    /// —con su cuerpo, su cámara, su HUD y su ficha— esté en todas (Berón 2026-10-30: con la Sala de Eras de anfitriona
    /// solo había el explorador de validación y en Construcción se veía todo celeste). Las de «Otros» (Sala de Eras,
    /// galería) son de validación y traen su propio explorador.
    /// </summary>
    public static void Launch(string path, bool kushalMember)
    {
        string n = System.IO.Path.GetFileNameWithoutExtension(path);
        string g = GroupOf(path, n);
        if (Additive(g)) SceneLauncher.Load(LaunchOptions.SanctuaryPath, kushalMember, n);
        else SceneLauncher.Load(path, kushalMember && g != "Otros");
    }

    /// <summary>Las escenas de la lista (Build Settings sin Inicio), con su grupo.</summary>
    public static List<(string path, string group)> ListScenes()
    {
        var l = new List<(string, string)>();
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string g = GroupOf(path, System.IO.Path.GetFileNameWithoutExtension(path));
            if (g != null) l.Add((path, g));
        }
        return l;
    }

    void Pick(Entry e)
    {
        PlayerPrefs.SetString("Inicio.Ultima", e.path); PlayerPrefs.Save();
        Launch(e.path, _kushal);
    }

    // ── Nombres legibles ─────────────────────────────────────────────────────────

    static readonly Dictionary<string, string> Special = new Dictionary<string, string>
    {
        { "SampleScene", "Santuario" }, { "Inicio", "Inicio" },
        { "Microcosmos_Scene0_SakshiRio", "Sakshi (el río)" }, { "Microcosmos_Scene1_Ambrosio", "Ambrosio (nivel 1)" },
        { "Sala_de_Eras", "Sala de Eras" }, { "Galeria_InsectosHumanos", "Galería de insectos humanos" },
    };
    static readonly Dictionary<string, string> Accents = new Dictionary<string, string>
    {
        { "Atomica", "Atómica" }, { "Mecanica", "Mecánica" }, { "Eratostenes", "Eratóstenes" }, { "Enfermeria", "Enfermería" },
        { "Camara", "Cámara" }, { "Emocion", "Emoción" }, { "Prologo", "Prólogo" }, { "Posesion", "Posesión" },
    };
    static Dictionary<string, string> _virtual;

    /// <summary>El nombre de escena como se lee en el menú: «Microcosmos_PlantaAtomica» → «Planta Atómica».</summary>
    public static string Pretty(string n)
    {
        if (string.IsNullOrEmpty(n)) return n;
        if (Special.TryGetValue(n, out string s)) return s;
        if (n.StartsWith("Virtual_"))
        {
            if (_virtual == null)
            {
                _virtual = new Dictionary<string, string>();
                foreach (var (key, area, title) in VirtualMissions.All()) { string k = VirtualMissions.Key(key); if (!_virtual.ContainsKey(k)) _virtual[k] = $"{area}: {title}"; }
            }
            string vk = n.Substring(8);
            return _virtual.TryGetValue(vk, out string vt) ? vt : Words(vk.Replace('_', ' '));
        }
        foreach (string pre in new[] { "Microcosmos_", "Prueba_", "MobWorld_" }) if (n.StartsWith(pre)) return Words(n.Substring(pre.Length).Replace('_', ' '));
        if (n.StartsWith("Yoga_")) return "Yoga: " + Words(n.Substring(5));
        return Words(n.Replace('_', ' '));
    }

    static string Words(string s)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < s.Length; i++)
        {
            if (i > 0 && char.IsUpper(s[i]) && char.IsLower(s[i - 1])) sb.Append(' ');
            sb.Append(s[i]);
        }
        string[] w = sb.ToString().Split(' ');
        for (int i = 0; i < w.Length; i++) if (Accents.TryGetValue(w[i], out string a)) w[i] = a;
        return string.Join(" ", w);
    }

    // ── Pantalla ─────────────────────────────────────────────────────────────────

    void Styles()
    {
        if (_title != null) return;
        _title = new GUIStyle(GUI.skin.label) { fontSize = 40, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
        _sub = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
        _head = new GUIStyle(GUI.skin.button) { fontSize = 17, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
        _btn = new GUIStyle(GUI.skin.button) { fontSize = 15, alignment = TextAnchor.MiddleLeft };
        _big = new GUIStyle(GUI.skin.button) { fontSize = 20, fontStyle = FontStyle.Bold };
        _note = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true };
        _tog = new GUIStyle(GUI.skin.toggle) { fontSize = 18, fontStyle = FontStyle.Bold };
        _title.normal.textColor = new Color(0.85f, 0.93f, 1f);
    }

    void OnGUI()
    {
        Styles();
        float sc = Mathf.Clamp(Screen.height / 640f, 0.7f, 3f);   // todo cabe también en ventanas bajas (el botón de repetir se cortaba)
        GUI.matrix = Matrix4x4.Scale(new Vector3(sc, sc, 1f));
        float W = Screen.width / sc, H = Screen.height / sc;

        // Columna izquierda: las casillas y la entrada al Santuario.
        float lx = 40f, lw = Mathf.Min(420f, W * 0.4f);
        GUI.Label(new Rect(lx, 30f, lw + 200f, 56f), "Cold Sanctuary", _title);
        GUI.Label(new Rect(lx, 86f, lw, 44f), "Inicio: elige cómo entrar. Nada se carga hasta elegir.", _sub);

        // Una sola casilla de Kushal para el botón y para la lista (antes: dos botones y otra casilla solo para la lista).
        bool k = GUI.Toggle(new Rect(lx, 146f, lw, 26f), _kushal, " Kushal iniciado", _tog);
        if (k != _kushal) { _kushal = k; PlayerPrefs.SetInt("Inicio.Kushal", k ? 1 : 0); }
        GUI.Label(new Rect(lx + 20f, 172f, lw - 20f, 40f), _kushal
            ? "Ya es miembro: sin examen ni iniciaciones, libre para explorar."
            : "Llega como nuevo: examen, iniciación y misiones.", _note);
        bool gd = GUI.Toggle(new Rect(lx, 214f, lw, 26f), _guard, " La Magnate guarda los puestos", _tog);
        if (gd != _guard) { _guard = gd; LaunchOptions.MagnateGuards = gd; PlayerPrefs.SetInt("Inicio.MagnateGuarda", gd ? 1 : 0); }
        GUI.Label(new Rect(lx + 20f, 240f, lw - 20f, 40f), "Si te alejas de tu puesto en horas de trabajo, te devuelve a él.", _note);

        if (GUI.Button(new Rect(lx, 292f, lw, 64f), "Entrar al Santuario", _big)) SceneLauncher.Load(LaunchOptions.SanctuaryPath, _kushal);
        GUI.Label(new Rect(lx, 362f, lw, 72f),
            "O elige a la derecha cualquier escena: las dos casillas valen también para ella. Los microcosmos y virtuales se abren " +
            "encima del Santuario, como desde el tanque; su portal de salida devuelve a Kushal al Santuario.", _note);

        string last = PlayerPrefs.GetString("Inicio.Ultima", "");
        Entry le = last.Length > 0 ? _entries.Find(e => e.path == last) : null;
        if (le != null && GUI.Button(new Rect(lx, 440f, lw, 34f), $"Repetir la última: {le.label}", _btn)) Pick(le);

        GUI.Label(new Rect(lx, H - 54f, lw, 44f), $"En el juego: {SceneLauncher.PauseKey} (o el botón de abajo a la derecha) pausa y vuelve aquí.", _note);

        // Columna derecha: todas las escenas por grupos.
        float rx = lx + lw + 40f, rw = W - rx - 40f, top = 40f, vh = H - top - 30f;
        GUI.Box(new Rect(rx - 8f, top - 8f, rw + 16f, vh + 16f), GUIContent.none);
        float rowH = 30f, y = 0f, inner = rw - 20f;
        float total = 0f;
        foreach (string g in Groups) { int c = Count(g); if (c == 0) continue; total += 36f + (_open[g] ? c * rowH : 0f); }
        _scroll = GUI.BeginScrollView(new Rect(rx, top, rw, vh), _scroll, new Rect(0f, 0f, inner, total));
        foreach (string g in Groups)
        {
            int c = Count(g);
            if (c == 0) continue;
            if (GUI.Button(new Rect(0f, y, inner, 32f), $"{(_open[g] ? "▼" : "►")}  {g}  ({c})", _head)) { _open[g] = !_open[g]; PlayerPrefs.SetInt("Inicio.Abierto." + g, _open[g] ? 1 : 0); }
            y += 36f;
            if (!_open[g]) continue;
            foreach (Entry e in _entries)
            {
                if (e.group != g) continue;
                string extra = e.group == "Otros" ? "  · explorador" : Additive(e.group) ? "  · sobre el Santuario" : "";
                if (GUI.Button(new Rect(16f, y, inner - 16f, rowH - 3f), e.label + extra, _btn)) Pick(e);
                y += rowH;
            }
        }
        GUI.EndScrollView();
        GUI.matrix = Matrix4x4.identity;
    }

    int Count(string g) { int c = 0; foreach (Entry e in _entries) if (e.group == g) c++; return c; }
}
