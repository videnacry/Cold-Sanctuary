using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL GRIMORIO (2026-11-20, Berón: «para los hechizos vamos a hacer una prueba del sistema móvil: agregamos el botón de grimorio, debe
/// mostrar una modal con todos los hechizos existentes y se lanzan con los retos de código»). Es la forma de lanzar hechizos sin teclado:
/// el botón R del mando (o F8 en el teclado) abre una ventana con TODOS los hechizos del lanzador (<see cref="SpellConsole.Catalog"/>),
/// los de prueba aparte; al tocar uno se abre su RETO DE CÓDIGO en la hoja de CodeShip (<see cref="CodeChallengeSheet"/>) y, al acertarlo,
/// se lanza (<see cref="SpellConsole.CastFromUi"/>) con sus parámetros por defecto y el destino que sugiere el interés del poseído
/// (<see cref="SpellConsole.LineFor"/>). Los que el poseído no ha aprendido se ven, apagados. El reto lo fabrica <see cref="SpellChallenge"/>:
/// ordenar las piezas de la llamada si tiene parámetros, o elegir la línea buena si no. Tema: <see cref="CsTheme"/>. docs/mandos-tactiles.md §5.
/// </summary>
public class Grimorio : MonoBehaviour
{
    public const KeyCode ToggleKey = KeyCode.F8;   // libre: F1–F7 son el HUD y las ventanas, F10 la pausa
    static Grimorio _i;
    public static bool IsOpen => _i != null && _i._open;
    /// <summary>(pruebas) Sustituye al lanzador: recibe la línea y devuelve la respuesta. null = el lanzador de verdad.</summary>
    public static Func<string, string> CastHook;
    /// <summary>La última línea lanzada desde el Grimorio y lo que contestó (pruebas y HUD).</summary>
    public static string LastLine, LastAnswer;

    bool _open, _menuWasOpen;
    int _openedFrame;
    Vector2 _scroll;
    GUIStyle _title, _row, _rowOff, _small, _head;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() => Ensure();

    public static Grimorio Ensure()
    {
        if (_i != null) return _i;
        _i = FindAnyObjectByType<Grimorio>();
        if (_i != null) return _i;
        var go = new GameObject("Grimorio_AUTO");
        DontDestroyOnLoad(go);
        return _i = go.AddComponent<Grimorio>();
    }

    public static void Toggle() { if (IsOpen) Close(); else Open(); }

    public static void Open()
    {
        Grimorio g = Ensure();
        if (g._open) return;
        g._open = true; g._openedFrame = Time.frameCount; g._scroll = Vector2.zero;
        g._menuWasOpen = GameKeys.MenuOpen;
        GameKeys.MenuOpen = true;   // el teclado y el mando del juego, mudos mientras se elige (los toques llegan a la ventana)
    }

    public static void Close()
    {
        if (_i == null || !_i._open) return;
        _i._open = false;
        GameKeys.MenuOpen = _i._menuWasOpen;
    }

    void Update()
    {
        if (ColdSanctuary.TestRunner.Running && CastHook == null) return;   // (las pruebas lo abren a mano)
        if (_open)
        {
            if (Time.frameCount != _openedFrame && (GameKeys.UiKeyDown(ToggleKey) || GameKeys.UiKeyDown(KeyCode.Escape))) Close();
        }
        else if (GameKeys.GetKeyDown(ToggleKey) && !CodeChallengeSheet.IsOpen) Open();
    }

    /// <summary>Elige un hechizo: cierra la ventana y abre su reto; al acertarlo, se lanza.</summary>
    public static bool Choose(string spell)
    {
        if (!SpellConsole.PlayerKnows(spell)) return false;
        Close();
        string line = SpellConsole.LineFor(spell);
        CodeExercise ex = SpellChallenge.For(spell, line);
        CodeChallengeSheet.Open(ex, a =>
        {
            if (!ex.IsCorrect(a)) return false;
            LastLine = line;
            LastAnswer = CastHook != null ? CastHook(line) : SpellConsole.CastFromUi(line);
            ex.explain = $"Lanzado: <b>{line}</b>. {LastAnswer}";   // la hoja lo enseña al acertar
            return true;
        });
        return true;
    }

    void OnGUI()
    {
        if (!_open) return;
        CsTheme.Use();
        GUI.depth = -1400;
        float s = Mathf.Min(Screen.width, Screen.height);
        int f = Mathf.Clamp(Mathf.RoundToInt(s * 0.028f), 13, 24);
        if (_row == null || _row.fontSize != f)
        {
            _title = new GUIStyle(GUI.skin.label) { fontSize = f + 6, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
            _title.normal.textColor = CsTheme.Cyan;
            _small = new GUIStyle(GUI.skin.label) { fontSize = f - 2, wordWrap = true, richText = true };
            _head = new GUIStyle(_small) { fontStyle = FontStyle.Bold };
            _head.normal.textColor = CsTheme.OrangeLight;
            _row = new GUIStyle(GUI.skin.button) { fontSize = f, richText = true, alignment = TextAnchor.MiddleLeft, wordWrap = false, padding = new RectOffset(14, 10, 6, 6) };
            _rowOff = new GUIStyle(_row);
            _rowOff.normal.textColor = _rowOff.hover.textColor = new Color(CsTheme.Text.r, CsTheme.Text.g, CsTheme.Text.b, 0.4f);
        }
        // La ventana: centrada, 90 % en el móvil en vertical, más estrecha en horizontal; deja libre la franja de abajo (los Joy-Con).
        float w = Mathf.Min(Screen.width * 0.9f, Mathf.Max(420f, Screen.width * 0.5f));
        float h = Mathf.Min(Screen.height * 0.72f, Screen.height - 24f);
        var win = new Rect((Screen.width - w) * 0.5f, Mathf.Max(12f, (Screen.height - h) * 0.3f), w, h);
        GUI.Box(win, "");
        float pad = 14f, y = win.y + pad, rowH = f * 2.4f;
        GUI.Label(new Rect(win.x + pad, y, w - 2f * pad - 110f, f * 2f), "✦ Grimorio", _title);
        if (GUI.Button(new Rect(win.xMax - pad - 100f, y, 100f, f * 2f), "Cerrar")) { Close(); return; }
        y += f * 2.2f;
        GUI.Label(new Rect(win.x + pad, y, w - 2f * pad, f * 2.6f), SpellConsole.HasPossessed
            ? "Toca un hechizo y resuelve su <b>reto de código</b> para lanzarlo. Los apagados aún no los has aprendido."
            : "<color=#ffb357>No manejas a nadie ahora mismo: los hechizos no se pueden lanzar.</color>", _small);
        y += f * 2.8f;

        var game = new List<(string name, string template)>();
        var admin = new List<(string name, string template)>();
        foreach (var e in SpellConsole.Catalog) (SpellConsole.AdminSpells.Contains(e.name) ? admin : game).Add(e);
        float listH = (game.Count + admin.Count) * (rowH + 6f) + 2f * f * 2f;
        var view = new Rect(win.x + pad, y, w - 2f * pad, win.yMax - pad - y);
        _scroll = GUI.BeginScrollView(view, _scroll, new Rect(0f, 0f, view.width - 20f, listH));
        float ly = 0f;
        Section("Hechizos", game, ref ly, view.width - 20f, rowH, f);
        Section("De prueba (administrador)", admin, ref ly, view.width - 20f, rowH, f);
        GUI.EndScrollView();
    }

    void Section(string title, List<(string name, string template)> list, ref float y, float w, float rowH, int f)
    {
        if (list.Count == 0) return;
        GUI.Label(new Rect(0f, y, w, f * 1.8f), title.ToUpperInvariant(), _head);
        y += f * 2f;
        bool can = SpellConsole.HasPossessed;
        foreach (var e in list)
        {
            bool known = SpellConsole.PlayerKnows(e.name);
            string text = $"<b>{e.name}</b>   <size={f - 3}>{e.template}</size>" + (known ? "" : $"   <size={f - 3}>(no aprendido)</size>");
            bool was = GUI.enabled;
            GUI.enabled = was && known && can;
            if (GUI.Button(new Rect(0f, y, w, rowH), text, known ? _row : _rowOff)) Choose(e.name);
            GUI.enabled = was;
            y += rowH + 6f;
        }
    }
}

/// <summary>
/// EL RETO DE CÓDIGO DE UN HECHIZO (Grimorio, docs/mandos-tactiles.md §5). Lógica pura: de la línea que se va a lanzar fabrica un
/// <see cref="CodeExercise"/>. Con parámetros: ORDENAR las tres piezas de la llamada (nombre y paréntesis, el objeto de parámetros, el
/// cierre) con una pieza trampa. Sin parámetros: ELEGIR la línea bien escrita entre cuatro. Cada pieza tiene su cara en pseudocódigo y en JS.
/// </summary>
public static class SpellChallenge
{
    public static CodeExercise For(string spell, string line)
    {
        int p = line.IndexOf('(');
        string args = p >= 0 ? line.Substring(p + 1, Mathf.Max(0, line.LastIndexOf(')') - p - 1)).Trim() : "";
        if (args.Length > 0)
        {
            return new CodeExercise
            {
                id = "hechizo-" + spell, kind = CodeExercise.Kind.Order, concept = 1, skill = CodeSkill.Completar,
                idea = $"Un hechizo es una llamada: su nombre, sus parámetros entre llaves y el cierre.",
                question = $"Ordena las piezas para lanzar «{spell}».",
                blocks = new[]
                {
                    new CodeBlock { id = "nombre", pseudo = $"lanzar «{spell}» con…", js = spell + "(" },
                    new CodeBlock { id = "parametros", pseudo = "estos parámetros: " + Pseudo(args), js = args, indent = 1 },
                    new CodeBlock { id = "cierre", pseudo = "fin del hechizo", js = ")" },
                    new CodeBlock { id = "trampa", pseudo = "fin con corchete (mal cerrado)", js = "]" },
                },
                solution = new[] { "nombre", "parametros", "cierre" },
                source = "SpellConsole (el lanzador de hechizos de Cold Sanctuary)",
            };
        }
        string good = spell + "()";
        var opts = new List<string> { good, spell + "[]", spell + "(", "()" + spell };
        int rot = Mathf.Abs(spell.GetHashCode() % opts.Count);   // dónde cae la buena (fijo por hechizo dentro de una sesión)
        var rotated = new string[opts.Count];
        for (int i = 0; i < opts.Count; i++) rotated[(i + rot) % opts.Count] = opts[i];
        return new CodeExercise
        {
            id = "hechizo-" + spell, kind = CodeExercise.Kind.Choice, concept = 1, skill = CodeSkill.Reparar,
            idea = "Un hechizo sin parámetros se lanza igual: su nombre y los paréntesis vacíos.",
            question = $"¿Qué línea lanza «{spell}»?",
            blocks = new[] { new CodeBlock { id = "orden", pseudo = $"lanzar «{spell}» sin parámetros", js = "// " + spell + " sin parámetros" } },
            options = rotated, correct = rot,
            source = "SpellConsole (el lanzador de hechizos de Cold Sanctuary)",
        };
    }

    /// <summary>«{destiny:"Marilia", amount:20}» → «destiny = Marilia, amount = 20».</summary>
    static string Pseudo(string args)
    {
        string a = args.Trim().TrimStart('{').TrimEnd('}');
        var parts = new List<string>();
        foreach (string kv in a.Split(','))
        {
            int c = kv.IndexOf(':');
            parts.Add(c > 0 ? $"{kv.Substring(0, c).Trim()} = {kv.Substring(c + 1).Trim().Trim('"')}" : kv.Trim());
        }
        return string.Join(", ", parts);
    }

    /// <summary>La respuesta buena (pruebas).</summary>
    public static CodeAnswer Answer(CodeExercise ex) => ex.kind == CodeExercise.Kind.Order ? CodeAnswer.Order(ex.solution) : CodeAnswer.Choice(ex.correct);
}
