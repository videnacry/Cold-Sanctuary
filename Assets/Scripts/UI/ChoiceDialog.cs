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
        if (!_open || Time.frameCount == _openedFrame || _options.Length == 0 && !GameKeys.UiKeyDown(KeyCode.Escape)) return;
        if (GameKeys.UiKeyDown(KeyCode.Escape)) { Action c = _onCancel; Close(); c?.Invoke(); return; }
        if (GameKeys.UiKeyDown(KeyCode.DownArrow)) _sel = (_sel + 1) % _options.Length;
        if (GameKeys.UiKeyDown(KeyCode.UpArrow)) _sel = (_sel - 1 + _options.Length) % _options.Length;
        if (GameKeys.UiKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Pick(_sel);
    }

    bool _laid;
    Vector2 _scroll;

    /// <summary>(2026-11-20, PB-169) La hoja de CodeShip (CsUi.SheetFrame) con cada opción en una fila; la marcada por las flechas, con ▸.</summary>
    void OnGUI()
    {
        CsTheme.Use();   // (2026-11-19) el tema de CodeShip para los menús (UI/CsTheme.cs)
        if (!CsUi.Draw(ref _laid, _open)) return;
        GUI.depth = -1300;
        Rect inner = CsUi.SheetFrame();
        float y = inner.y;
        if (CsUi.SheetHead(inner, ref y, "Elige", _title) == 0) { Action c = _onCancel; Close(); c?.Invoke(); return; }
        GUILayout.BeginArea(new Rect(inner.x, y, inner.width, inner.yMax - y));
        _scroll = GUILayout.BeginScrollView(_scroll);
        int picked = -1;
        string[] options = _options;
        for (int i = 0; i < options.Length; i++)
            if (CsUi.SetRow(options[i], null, i == _sel ? "▸" : "", true)) picked = i;
        if (!CsUi.Mobile) GUILayout.Label("↑/↓ elige · Intro o clic confirma · Esc cierra", CsUi.NoteStyle);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
        if (picked >= 0) Pick(picked);   // después de cerrar los grupos del dibujo
    }
}
