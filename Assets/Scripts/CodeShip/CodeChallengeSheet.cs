using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA HOJA DEL RETO DE CÓDIGO (IMGUI; CodeShip: la hoja inferior de los retos, reglas.md §4 y ux-acciones.md §1). Una a la vez.
///  · ORDENAR: los bloques barajados abajo; tocar uno lo añade a «Tu secuencia»; tocar uno de la secuencia lo devuelve. «Comprobar».
///  · ELEGIR: el código arriba y las opciones como botones.
///  · Un fallo se ve con FORMA y color (✗ y rojo); un acierto, ✓ y verde. Repetir un fallo ya dado no cuenta ni cambia nada
///    (<see cref="CodeAttempts"/>). Tras un fallo de ordenar, la pista: cuántos bloques del principio están bien.
///  · «JS» cambia la cara de los bloques (pseudocódigo en español ↔ JavaScript).
///  · Mientras está abierta, el teclado del juego está mudo (<see cref="GameKeys"/>) y la historia no dibuja su recuadro: solo la hoja y
///    la escena. Esc o ✕ la cierran sin resolver (se puede volver con E en el sitio, o desde el menú).
/// Todo toque ≥ 44 px en la web (móvil primero).
/// </summary>
public class CodeChallengeSheet : MonoBehaviour
{
    static CodeChallengeSheet _i;
    public static bool IsOpen => _i != null && _i._ex != null;
    /// <summary>El reto abierto (null si ninguno): pruebas y menú.</summary>
    public static CodeExercise Current => _i != null ? _i._ex : null;

    CodeExercise _ex;
    Func<CodeAnswer, bool> _answer;
    readonly CodeAttempts _attempts = new CodeAttempts();
    readonly List<string> _picked = new List<string>();
    string[] _deck = new string[0];
    bool _js, _solved;
    string _feedback = ""; bool _feedbackBad;
    Vector2 _scroll;
    int _openedFrame;
    // (2026-11-18) MODO TAREA (CodeShip ChoreRun): varios retos en orden, con fallos contados; al acabar (hecha o fallada) avisa a quien la abrió.
    ChoreRun _run;
    string _runTitle = "", _runScenario = "";
    Action<ChoreRun> _onEnd;
    Func<ChoreRun> _retry;
    /// <summary>(2026-11-18, CodeShip §4.2) Se llama al acertar cada reto de la tarea: (tarea, índice del reto, texto del paso) → su animación.</summary>
    Action<ChoreRun, int, string> _onStep;
    bool _ended;
    CodeAttempts Att => _run != null ? _run.Attempts : _attempts;
    /// <summary>La tarea abierta en la hoja (null si es un reto suelto o no hay hoja): pruebas y menú.</summary>
    public static ChoreRun Run => IsOpen ? _i._run : null;

    static CodeChallengeSheet Get()
    {
        if (_i != null) return _i;
        var go = new GameObject("CodeChallengeSheet_AUTO");
        DontDestroyOnLoad(go);
        return _i = go.AddComponent<CodeChallengeSheet>();
    }

    /// <summary>Abre el reto. <paramref name="answer"/> recibe cada respuesta y dice si acertó (quien la abre cumple su paso ahí).</summary>
    public static void Open(CodeExercise ex, Func<CodeAnswer, bool> answer)
    {
        if (ex == null) return;
        CodeChallengeSheet s = Get();
        s.EndRun();
        s._run = null; s._onEnd = null; s._retry = null;
        s.Load(ex);
        s._answer = answer;
        Debug.Log($"[Retos] Abierto «{ex.id}» ({(ex.kind == CodeExercise.Kind.Order ? "ordenar" : "elegir")}).");
    }

    /// <summary>Abre una TAREA: sus retos en orden en la misma hoja. <paramref name="onEnd"/> se llama al cerrarla hecha o fallada (no si se
    /// cierra a medias); <paramref name="retry"/>, si se da, ofrece «Reintentar» tras fallar.</summary>
    public static void OpenRun(ChoreRun run, string title, string scenario, Action<ChoreRun> onEnd, Func<ChoreRun> retry = null, Action<ChoreRun, int, string> onStep = null)
    {
        if (run == null || run.Current == null) return;
        CodeChallengeSheet s = Get();
        s.EndRun();
        s._run = run; s._runTitle = title ?? ""; s._runScenario = scenario ?? ""; s._onEnd = onEnd; s._retry = retry; s._onStep = onStep; s._ended = false;
        s._answer = null;
        s.Load(run.Current);
        Debug.Log($"[Retos] Tarea «{title}»: {run.Steps} retos.");
    }

    void Load(CodeExercise ex)
    {
        _ex = ex;
        _attempts.On(ex);
        _picked.Clear();
        _deck = ex.kind == CodeExercise.Kind.Order ? ex.Shuffled(Time.frameCount) : new string[0];
        _solved = false; _feedback = ""; _scroll = Vector2.zero; _openedFrame = Time.frameCount;
    }

    /// <summary>Si había una tarea hecha o fallada sin avisar, avisa ahora (una sola vez).</summary>
    void EndRun()
    {
        if (_run == null || _ended || _run.Status == ChoreRun.State.Doing) return;
        _ended = true;
        Action<ChoreRun> cb = _onEnd;
        cb?.Invoke(_run);
    }

    public static void Close()
    {
        if (_i == null || _i._ex == null) return;
        _i.EndRun();
        _i._ex = null; _i._answer = null; _i._run = null;
    }

    /// <summary>Da una respuesta (la hoja y las pruebas). true = acertó.</summary>
    public static bool Submit(CodeAnswer a)
    {
        if (!IsOpen) return false;
        CodeChallengeSheet s = _i;
        if (s._run != null) return s.SubmitRun(a);
        if (s._solved) return true;
        if (s._attempts.Repeated(a)) { s._feedback = "✗ Esa respuesta ya la probaste: prueba otra."; s._feedbackBad = true; return false; }
        bool ok = s._ex.IsCorrect(a) && (s._answer == null || s._answer(a));
        s._attempts.Record(a, ok);
        if (ok)
        {
            s._solved = true;
            s._feedback = "✓ ¡Correcto!" + (string.IsNullOrEmpty(s._ex.explain) ? "" : " " + s._ex.explain);
            s._feedbackBad = false;
            return true;
        }
        s._feedbackBad = true;
        if (a.kind == CodeExercise.Kind.Order)
        {
            int good = s._ex.CorrectPrefix(a.order);
            s._feedback = good == 0 ? "✗ No: el primer bloque ya no va ahí." : $"✗ No del todo: {(good == 1 ? "el primer bloque está bien" : $"los {good} primeros están bien")}.";
            // Lo que estaba bien se queda; lo demás vuelve abajo (como en CodeShip, paso a paso).
            s._picked.RemoveRange(good, s._picked.Count - good);
        }
        else s._feedback = "✗ No es esa. Lee otra vez el código.";
        return false;
    }

    bool SubmitRun(CodeAnswer a)
    {
        if (_run.Status != ChoreRun.State.Doing) return _run.Status == ChoreRun.State.Done;
        if (_run.Tried(a)) { _feedback = "✗ Esa respuesta ya la probaste: no cuenta. Prueba otra."; _feedbackBad = true; return false; }
        int good = a.kind == CodeExercise.Kind.Order ? _ex.CorrectPrefix(a.order) : 0;
        int solved = _run.Step; CodeExercise was = _ex;
        bool ok = _run.Submit(a);
        if (ok) _onStep?.Invoke(_run, solved, was.kind == CodeExercise.Kind.Choice && was.correct < was.options.Length ? was.options[was.correct] : "la secuencia, en orden");
        if (ok)
        {
            string why = string.IsNullOrEmpty(_ex.explain) ? "" : " " + _ex.explain;
            if (_run.Status == ChoreRun.State.Done) { _solved = true; _feedback = $"✓ ¡Hecho! {_run.Steps} de {_run.Steps}.{why}"; _feedbackBad = false; return true; }
            Load(_run.Current);   // el siguiente reto, en la misma hoja
            _feedback = $"✓ Bien. Reto {_run.Step + 1} de {_run.Steps}.{why}"; _feedbackBad = false;
            return true;
        }
        _feedbackBad = true;
        if (_run.Status == ChoreRun.State.Failed) { _feedback = $"✗ Demasiados fallos ({_run.Mistakes} de {_run.MaxMistakes}): la tarea no salió."; return false; }
        string left = _run.MistakesLeft == 1 ? "Te queda 1 fallo." : $"Te quedan {_run.MistakesLeft} fallos.";
        if (a.kind == CodeExercise.Kind.Order)
        {
            _feedback = (good == 0 ? "✗ No: el primer bloque ya no va ahí. " : $"✗ No del todo: {(good == 1 ? "el primer bloque está bien" : $"los {good} primeros están bien")}. ") + left;
            _picked.RemoveRange(Mathf.Min(good, _picked.Count), _picked.Count - Mathf.Min(good, _picked.Count));
        }
        else _feedback = "✗ No es esa. " + left;
        return false;
    }

    bool RunOver => _run != null && _run.Status != ChoreRun.State.Doing;

    void Update()
    {
        if (!IsOpen || Time.frameCount == _openedFrame || (_run != null && StepAnimator.Playing)) return;
        if (Input.GetKeyDown(KeyCode.Escape)) { Close(); return; }
        if ((_solved || RunOver) && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))) { Close(); return; }
        if (_ex.kind == CodeExercise.Kind.Choice && !_solved && !RunOver)
            for (int k = 0; k < Mathf.Min(9, _ex.options.Length); k++)
                if (Input.GetKeyDown(KeyCode.Alpha1 + k)) Submit(CodeAnswer.Choice(k));
    }

    // ── El dibujo ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    GUIStyle _txt, _small, _btn, _head, _bad, _good;
    static readonly Color Red = new Color(1f, 0.45f, 0.4f), Green = new Color(0.5f, 0.95f, 0.55f);
    float Row => WebLigera.Active ? Mathf.Max(44f, Mathf.Min(Screen.width, Screen.height) * 0.075f) : 32f;

    void Styles()
    {
        if (_txt != null) return;
        int f = WebLigera.Active ? Mathf.Max(15, Mathf.RoundToInt(Row * 0.36f)) : 14;
        _txt = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true, fontSize = f };
        _small = new GUIStyle(_txt) { fontSize = f - 2 };
        _head = new GUIStyle(_txt) { fontSize = f + 3, fontStyle = FontStyle.Bold };
        _btn = new GUIStyle(GUI.skin.button) { richText = true, alignment = TextAnchor.MiddleLeft, fontSize = f, wordWrap = true, padding = new RectOffset(10, 10, 4, 4) };
        _bad = new GUIStyle(_btn); _bad.normal.textColor = _bad.hover.textColor = Red;
        _good = new GUIStyle(_btn); _good.normal.textColor = _good.hover.textColor = Green;
    }

    string Face(CodeBlock b) => b == null ? "?" : new string(' ', b.indent * 3) + (_js && !string.IsNullOrEmpty(b.js) ? b.js : b.pseudo);

    void OnGUI()
    {
        if (!IsOpen) return;
        Styles();
        GUI.depth = -900;   // encima de los HUD del juego
        float row = Row, pad = 10f;
        if (_run != null && StepAnimator.Playing)   // (CodeShip §4.2) la hoja se recoge: se ve el paso hecho en su sitio
        {
            float bw = Mathf.Min(620f, Screen.width - 16f);
            var bar = new Rect((Screen.width - bw) * 0.5f, Screen.height - row - 20f, bw, row + 12f);
            GUI.Box(bar, ""); GUI.Box(bar, "");
            Color c0 = GUI.contentColor; GUI.contentColor = Green;
            GUI.Label(new Rect(bar.x + 10f, bar.y + 6f, bw - 20f, row), "✓ " + StepAnimator.Caption, _txt);
            GUI.contentColor = c0;
            return;
        }
        float w = Mathf.Min(780f, Screen.width - 16f), h = Mathf.Min(Screen.height * (WebLigera.Active ? 0.86f : 0.8f), 700f);
        var r = new Rect((Screen.width - w) * 0.5f, Screen.height - h - 8f, w, h);   // hoja inferior
        GUI.Box(r, ""); GUI.Box(r, ""); GUI.Box(r, "");   // tres capas: opaca de verdad (CodeShip: las barras casi transparentes no se leían)

        // Cabecera: título, JS, ✕ (a la medida del dedo).
        string kind = _ex.kind == CodeExercise.Kind.Order ? "ordenar" : SkillName(_ex.skill);
        string head = _run == null ? $"<b>Reto de código</b> · {kind}" : $"<b>{_runTitle}</b> · reto {Mathf.Min(_run.Step + 1, _run.Steps)} de {_run.Steps} · {kind}";
        GUI.Label(new Rect(r.x + pad, r.y + 6f, w - 3f * row - 3f * pad, row), head, _head);
        if (_run != null) DrawProgress(new Rect(r.x + pad, r.y + row + 4f, w - 2f * pad, 6f));
        if (GUI.Button(new Rect(r.xMax - 2f * row - 2f * pad - row * 0.6f, r.y + 6f, row * 1.6f, row), _js ? "Pseudo" : "JS")) _js = !_js;
        if (GUI.Button(new Rect(r.xMax - row - pad, r.y + 6f, row, row), "✕")) { Close(); return; }

        var inner = new Rect(r.x + pad, r.y + row + 12f, w - 2f * pad, h - row * 2.4f - 20f);
        GUILayout.BeginArea(inner);
        _scroll = GUILayout.BeginScrollView(_scroll);
        if (_run != null && _run.Step == 0 && _run.Mistakes == 0 && _runScenario.Length > 0) GUILayout.Label($"<color=#B8C0C8>{_runScenario}</color>", _small);
        if (!string.IsNullOrEmpty(_ex.idea)) GUILayout.Label($"<i>{_ex.idea}</i>", _small);
        GUILayout.Label(_ex.question, _txt);
        GUILayout.Space(6f);
        if (_ex.kind == CodeExercise.Kind.Order) DrawOrder(row); else DrawChoice(row);
        if (_feedback.Length > 0)
        {
            GUILayout.Space(6f);
            Color c = GUI.contentColor; GUI.contentColor = _feedbackBad ? Red : Green;
            GUILayout.Label(_feedback, _txt);
            GUI.contentColor = c;
        }
        if (!string.IsNullOrEmpty(_ex.source)) GUILayout.Label($"<size={_small.fontSize - 1}>Fuente: {_ex.source}</size>", _small);
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        // Pie: la acción principal, siempre en el mismo sitio (abajo a la derecha, cerca del pulgar).
        var foot = new Rect(r.x + pad, r.yMax - row - pad, w - 2f * pad, row);
        if (_solved) { if (GUI.Button(new Rect(foot.xMax - row * 3.5f, foot.y, row * 3.5f, row), "✓ Seguir")) Close(); return; }
        if (RunOver)   // fallada: cerrar o reintentar
        {
            if (_retry != null && GUI.Button(new Rect(foot.xMax - row * 7.4f, foot.y, row * 3.6f, row), "Reintentar"))
            {
                Func<ChoreRun> retry = _retry; string t = _runTitle, sc = _runScenario; Action<ChoreRun> cb = _onEnd; Action<ChoreRun, int, string> st = _onStep;
                Close();
                ChoreRun again = retry();
                if (again != null) OpenRun(again, t, sc, cb, retry, st);
                return;
            }
            if (GUI.Button(new Rect(foot.xMax - row * 3.5f, foot.y, row * 3.5f, row), "Cerrar")) Close();
            return;
        }
        string fails = _run != null ? $"Fallos: {_run.Mistakes} de {_run.MaxMistakes}" : $"Fallos: {_attempts.Count}";
        GUI.Label(new Rect(foot.x, foot.y, foot.width - row * 7.5f, row), $"<size={_small.fontSize}>{fails}{(WebLigera.Active ? "" : " · Esc: cerrar")}</size>", _small);
        if (_ex.kind == CodeExercise.Kind.Order)
        {
            if (GUI.Button(new Rect(foot.xMax - row * 7.4f, foot.y, row * 3.2f, row), "Borrar")) { _picked.Clear(); _feedback = ""; }
            GUI.enabled = _picked.Count > 0;
            if (GUI.Button(new Rect(foot.xMax - row * 4f, foot.y, row * 4f, row), "Comprobar")) Submit(CodeAnswer.Order(_picked.ToArray()));
            GUI.enabled = true;
        }
    }

    void DrawOrder(float row)
    {
        GUILayout.Label($"<b>Tu secuencia</b> ({_picked.Count}/{_ex.solution.Length}) <size={_small.fontSize - 1}>· toca un bloque para quitarlo</size>", _txt);
        if (_picked.Count == 0) GUILayout.Label("<color=#9AA0A6>(vacía: toca los bloques de abajo en orden)</color>", _small);
        for (int i = 0; i < _picked.Count; i++)
            if (GUILayout.Button($"{i + 1}. {Face(_ex.Block(_picked[i]))}", _btn, GUILayout.MinHeight(row))) { _picked.RemoveAt(i); _feedback = ""; break; }
        GUILayout.Space(8f);
        GUILayout.Label("<b>Bloques</b>", _txt);
        foreach (string id in _deck)
        {
            if (_picked.Contains(id)) continue;
            if (GUILayout.Button("+ " + Face(_ex.Block(id)), _btn, GUILayout.MinHeight(row))) { _picked.Add(id); _feedback = ""; break; }
        }
    }

    void DrawChoice(float row)
    {
        foreach (CodeBlock b in _ex.blocks) GUILayout.Label($"<color=#BFD7FF>{Face(b)}</color>", _txt);
        GUILayout.Space(8f);
        for (int i = 0; i < _ex.options.Length; i++)
        {
            bool wrong = Att.WrongChoices.Contains(i);
            bool right = _solved && i == _ex.correct;
            string mark = wrong ? "✗ " : right ? "✓ " : $"{i + 1}. ";
            GUI.enabled = !wrong && !_solved && !RunOver;   // una opción fallada queda marcada y no se puede volver a tocar
            if (GUILayout.Button(mark + _ex.options[i], wrong ? _bad : right ? _good : _btn, GUILayout.MinHeight(row))) Submit(CodeAnswer.Choice(i));
            GUI.enabled = true;
        }
    }

    /// <summary>La barra de la tarea: un segmento por reto (verde hecho, ámbar el de ahora, gris lo que falta), opaca y legible (CodeShip).</summary>
    void DrawProgress(Rect r)
    {
        int n = Mathf.Max(1, _run.Steps);
        float gap = 3f, cw = (r.width - gap * (n - 1)) / n;
        Color c = GUI.color;
        for (int i = 0; i < n; i++)
        {
            GUI.color = i < _run.Step ? new Color(0.35f, 0.85f, 0.45f) : i == _run.Step && !RunOver ? new Color(1f, 0.78f, 0.3f) : new Color(0.35f, 0.37f, 0.4f);
            GUI.DrawTexture(new Rect(r.x + i * (cw + gap), r.y, cw, r.height), Texture2D.whiteTexture);
        }
        GUI.color = c;
    }

    static string SkillName(CodeSkill s) => s == CodeSkill.Completar ? "completar" : s == CodeSkill.Predecir ? "predecir" : s == CodeSkill.Reparar ? "reparar" : "especificar";
}
