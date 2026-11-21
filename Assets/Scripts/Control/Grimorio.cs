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
/// ordenar las piezas de la llamada si tiene parámetros, o elegir la línea buena si no. Tema: <see cref="CsTheme"/>. docs/sistemas/mandos-tactiles.md §5.
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

    bool _laid;

    void OnGUI()
    {
        if (!CsUi.Draw(ref _laid, _open)) return;
        CsTheme.Use();
        GUI.depth = -1400;
        // (2026-11-20, PB-169) La hoja de CodeShip (CsUi.SheetFrame: abajo en vertical, a la derecha en horizontal; la escena se ve):
        // cabecera en versalitas con su ✕, y cada hechizo una fila con «Lanzar» (o apagada con por qué).
        Rect inner = CsUi.SheetFrame();
        float y = inner.y;
        if (CsUi.SheetHead(inner, ref y, "Grimorio", SpellConsole.HasPossessed
            ? "Toca «Lanzar» y resuelve su reto de código. Los apagados aún no los has aprendido."
            : "<color=#ffb347>No manejas a nadie ahora mismo: los hechizos no se pueden lanzar.</color>") == 0) { Close(); return; }

        var game = new List<(string name, string template)>();
        var admin = new List<(string name, string template)>();
        foreach (var e in SpellConsole.Catalog) (SpellConsole.AdminSpells.Contains(e.name) ? admin : game).Add(e);
        GUILayout.BeginArea(new Rect(inner.x, y, inner.width, inner.yMax - y));
        _scroll = GUILayout.BeginScrollView(_scroll);
        string chosen = Section("Hechizos", game), chosenAdmin = Section("De prueba (administrador)", admin);   // las dos siempre (GUILayout)
        GUILayout.EndScrollView();
        GUILayout.EndArea();
        if ((chosen ?? chosenAdmin) != null) Choose(chosen ?? chosenAdmin);   // después de cerrar los grupos del dibujo
    }

    static string Section(string title, List<(string name, string template)> list)
    {
        if (list.Count == 0) return null;
        CsUi.SectionLabel(title);
        bool can = SpellConsole.HasPossessed;
        string hit = null;
        foreach (var e in list)
        {
            bool known = SpellConsole.PlayerKnows(e.name);
            if (CsUi.Row(e.name, e.template, known ? null : "⊘ Aún no aprendido", "Lanzar", known && can)) hit = e.name;
        }
        return hit;
    }
}

/// <summary>
/// EL RETO DE CÓDIGO DE UN HECHIZO (Grimorio, docs/sistemas/mandos-tactiles.md §5). Lógica pura: de la línea que se va a lanzar fabrica un
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
