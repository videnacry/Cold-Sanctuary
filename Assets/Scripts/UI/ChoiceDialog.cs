using System;
using UnityEngine;

/// <summary>
/// DIÁLOGO DE OPCIONES (máquina de virtualización, alfombras voladoras…): se abre al tocar el objeto y se responde con
/// ↑/↓ + Intro o con clic; Esc lo cierra. Mientras está abierto SECUESTRA Intro: el lanzador de hechizos se oculta y el
/// teclado del juego queda mudo (GameKeys.DialogOpen). Uno a la vez.
/// </summary>
public class ChoiceDialog : MonoBehaviour
{
    static ChoiceDialog _i;
    public static bool IsOpen => _i != null && _i._open;
    /// <summary>Frame en que se cerró (el Intro que lo cerró no debe abrir el lanzador).</summary>
    public static int ClosedFrame { get; private set; } = -10;

    bool _open;
    string _title;
    string[] _options;
    Action<int> _onPick;
    Action _onCancel;
    int _sel, _openedFrame;
    Transform _anchor, _player; float _radius;
    const int Visible = 10;

    static ChoiceDialog Get()
    {
        if (_i != null) return _i;
        var go = new GameObject("ChoiceDialog_AUTO");
        DontDestroyOnLoad(go);
        return _i = go.AddComponent<ChoiceDialog>();
    }

    /// <summary>Abre el diálogo. Con <paramref name="anchor"/>, se cierra solo si el jugador se aleja más de <paramref name="radius"/> m.</summary>
    public static void Show(string title, string[] options, Action<int> onPick, Action onCancel = null, Transform anchor = null, float radius = 4.5f)
    {
        ChoiceDialog d = Get();
        d._title = title; d._options = options ?? new string[0]; d._onPick = onPick; d._onCancel = onCancel;
        d._anchor = anchor; d._radius = radius;
        d._sel = 0; d._open = true; d._openedFrame = Time.frameCount;
    }

    public static void Close()
    {
        if (_i == null || !_i._open) return;
        _i._open = false; ClosedFrame = Time.frameCount;
    }

    /// <summary>Elige la opción <paramref name="i"/> (también para pruebas).</summary>
    public static void Pick(int i)
    {
        if (!IsOpen || i < 0 || i >= _i._options.Length) return;
        Action<int> cb = _i._onPick;
        Close();
        cb?.Invoke(i);
    }

    public static string[] Options => IsOpen ? _i._options : new string[0];
    /// <summary>A qué objeto va atado el diálogo abierto (null si no hay o no va atado).</summary>
    public static Transform Anchor => IsOpen ? _i._anchor : null;

    void Update()
    {
        if (_open && _anchor != null)   // alejarse lo cierra (no bloquea nada: solo oculta el lanzador mientras está)
        {
            if (_player == null) { GameObject p = GameObject.FindGameObjectWithTag("Player"); _player = p != null ? p.transform : null; }
            if (_player != null) { Vector3 d = _player.position - _anchor.position; d.y = 0f; if (d.magnitude > _radius) { Close(); return; } }
        }
        if (!_open || Time.frameCount == _openedFrame || _options.Length == 0 && !Input.GetKeyDown(KeyCode.Escape)) return;
        if (Input.GetKeyDown(KeyCode.Escape)) { Action c = _onCancel; Close(); c?.Invoke(); return; }
        if (Input.GetKeyDown(KeyCode.DownArrow)) _sel = (_sel + 1) % _options.Length;
        if (Input.GetKeyDown(KeyCode.UpArrow)) _sel = (_sel - 1 + _options.Length) % _options.Length;
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Pick(_sel);
    }

    GUIStyle _btn, _btnOn, _head;
    void OnGUI()
    {
        if (!_open) return;
        if (_btn == null)
        {
            _btn = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, fontSize = 14, richText = true };
            _btnOn = new GUIStyle(_btn) { fontStyle = FontStyle.Bold };
            _btnOn.normal.textColor = _btnOn.hover.textColor = new Color(1f, 0.85f, 0.45f);
            _head = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, richText = true };
        }
        int first = Mathf.Clamp(_sel - Visible / 2, 0, Mathf.Max(0, _options.Length - Visible));
        int shown = Mathf.Min(Visible, _options.Length);
        float w = Mathf.Min(680f, Screen.width - 40f), rowH = 30f, h = 70f + shown * (rowH + 4f);
        float x = (Screen.width - w) * 0.5f, y = (Screen.height - h) * 0.5f;
        GUI.Box(new Rect(x, y, w, h), "");
        GUI.Label(new Rect(x, y + 8f, w, 24f), _title, _head);
        for (int k = 0; k < shown; k++)
        {
            int i = first + k;
            Rect r = new Rect(x + 16f, y + 40f + k * (rowH + 4f), w - 32f, rowH);
            if (GUI.Button(r, (i == _sel ? "➤ " : "   ") + _options[i], i == _sel ? _btnOn : _btn)) { Pick(i); return; }
        }
        GUI.Label(new Rect(x, y + h - 26f, w, 20f), "<size=11>↑/↓ elige · Intro o clic confirma · Esc cierra</size>", _head);
    }
}
