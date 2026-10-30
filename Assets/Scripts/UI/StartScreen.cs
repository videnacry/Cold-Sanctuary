using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PANTALLA DE INICIO (escena Inicio, la primera del build; Berón 2026-10-30): no carga nada pesado hasta elegir.
/// Entrada normal (SampleScene como siempre), entrada con Kushal iniciado (miembro, sin examen ni iniciaciones) y la LISTA
/// de todas las escenas de Build Settings por grupos (Santuario, Microcosmos, Pruebas, Virtual, Otros) para entrar directo.
/// Los microcosmos y virtuales se cargan en aditivo sobre una anfitriona (la Sala de Eras, o el Santuario si se pide), como
/// desde el tanque. En el juego, F10 o el botón de pausa vuelven aquí (<see cref="SceneLauncher"/>).
/// </summary>
public class StartScreen : MonoBehaviour
{
    static readonly string[] Groups = { "Santuario", "Microcosmos", "Pruebas", "Virtual", "Otros" };

    class Entry { public string path, name, label, group; }

    readonly List<Entry> _entries = new List<Entry>();
    readonly Dictionary<string, bool> _open = new Dictionary<string, bool>();
    bool _kushal, _fromSanctuary;
    Vector2 _scroll;
    GUIStyle _title, _sub, _head, _btn, _big, _note;

    void Start()
    {
        SceneLauncher.Ensure();
        SceneLauncher.CleanPersistent();   // los arranques de Unity del primer Play también corren aquí: fuera
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        _kushal = PlayerPrefs.GetInt("Inicio.Kushal", 1) == 1;
        _fromSanctuary = PlayerPrefs.GetInt("Inicio.DesdeSantuario", 0) == 1;
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
    static bool Additive(Entry e) => e.group == "Microcosmos" || e.group == "Virtual";

    void Pick(Entry e)
    {
        PlayerPrefs.SetString("Inicio.Ultima", e.path); PlayerPrefs.Save();
        if (Additive(e)) SceneLauncher.Load(_fromSanctuary ? LaunchOptions.SanctuaryPath : LaunchOptions.HostPath, _fromSanctuary && _kushal, e.name);
        else SceneLauncher.Load(e.path, _kushal && e.group != "Otros");
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
        _title.normal.textColor = new Color(0.85f, 0.93f, 1f);
    }

    void OnGUI()
    {
        Styles();
        float sc = Mathf.Max(1f, Screen.height / 900f);
        GUI.matrix = Matrix4x4.Scale(new Vector3(sc, sc, 1f));
        float W = Screen.width / sc, H = Screen.height / sc;

        // Columna izquierda: las dos entradas y las opciones.
        float lx = 40f, lw = Mathf.Min(420f, W * 0.4f);
        GUI.Label(new Rect(lx, 30f, lw + 200f, 56f), "Cold Sanctuary", _title);
        GUI.Label(new Rect(lx, 86f, lw, 44f), "Inicio: elige cómo entrar. Nada se carga hasta elegir.", _sub);

        if (GUI.Button(new Rect(lx, 150f, lw, 60f), "Entrada normal", _big)) SceneLauncher.Load(LaunchOptions.SanctuaryPath, false);
        GUI.Label(new Rect(lx, 212f, lw, 22f), "El Santuario como siempre: examen, iniciación y misiones.", _note);
        if (GUI.Button(new Rect(lx, 250f, lw, 60f), "Entrada con Kushal iniciado", _big)) SceneLauncher.Load(LaunchOptions.SanctuaryPath, true);
        GUI.Label(new Rect(lx, 312f, lw, 40f), "Kushal ya es miembro: sin examen ni iniciaciones, libre para explorar.", _note);

        GUI.Label(new Rect(lx, 370f, lw, 24f), "Opciones de la lista", _sub);
        bool k = GUI.Toggle(new Rect(lx, 398f, lw, 24f), _kushal, " Kushal iniciado (Santuario, pruebas, microcosmos)");
        bool fs = GUI.Toggle(new Rect(lx, 426f, lw, 24f), _fromSanctuary, " Microcosmos y virtuales desde el Santuario (más lento)");
        if (k != _kushal) { _kushal = k; PlayerPrefs.SetInt("Inicio.Kushal", k ? 1 : 0); }
        if (fs != _fromSanctuary) { _fromSanctuary = fs; PlayerPrefs.SetInt("Inicio.DesdeSantuario", fs ? 1 : 0); }
        GUI.Label(new Rect(lx, 456f, lw, 60f), _fromSanctuary
            ? "Los microcosmos se abren encima del Santuario, como desde el tanque; su portal de salida devuelve a Kushal al Santuario."
            : "Los microcosmos se abren encima de la Sala de Eras (ligera); su portal de salida devuelve a la sala.", _note);

        string last = PlayerPrefs.GetString("Inicio.Ultima", "");
        Entry le = last.Length > 0 ? _entries.Find(e => e.path == last) : null;
        if (le != null && GUI.Button(new Rect(lx, 524f, lw, 34f), $"Repetir la última: {le.label}", _btn)) Pick(le);

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
                string extra = e.group == "Otros" ? "" : Additive(e) ? (_fromSanctuary ? "  · sobre el Santuario" : "  · sobre la Sala de Eras") : (_kushal ? "  · Kushal iniciado" : "");
                if (GUI.Button(new Rect(16f, y, inner - 16f, rowH - 3f), e.label + extra, _btn)) Pick(e);
                y += rowH;
            }
        }
        GUI.EndScrollView();
        GUI.matrix = Matrix4x4.identity;
    }

    int Count(string g) { int c = 0; foreach (Entry e in _entries) if (e.group == g) c++; return c; }
}
