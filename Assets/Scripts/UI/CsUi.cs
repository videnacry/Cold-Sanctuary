using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LAS PIEZAS DE LA INTERFAZ DE CODESHIP, A SU MEDIDA (2026-11-20, Berón: «los mandos de switch están muy pequeños y se deben ocultar cuando
/// se muestran modales… mirarlo en vivo [el santuario 1 de CodeShip] y de allí sacar información para su recreación»). Medido en vivo en el
/// santuario 1 de CodeShip (rama integracion, Playwright en un iPhone 13: 390×664 en vertical y 750×342 en horizontal) y copiado de su CSS
/// (src/ui/action-pad.css, style.css, map-window.css, nav-map.css, settings.css, task-board.css). docs/sistemas/mandos-tactiles.md §6.
///
/// · LA MEDIDA: CodeShip mide en píxeles CSS de un iPhone 13. <see cref="Dp"/> pasa esos píxeles a los de esta pantalla de modo que el mando
///   y las ventanas ocupan LA MISMA PARTE de la pantalla que en CodeShip (la pantalla virtual mide al menos 390×664 en vertical o 750×342 en
///   horizontal, así que lo que cabe allí cabe aquí). En el escritorio, 1 (como CodeShip en un monitor).
/// · LAS VENTANAS: <see cref="SheetFrame"/> (la hoja de los retos y de los ajustes: abajo en vertical, a la derecha en horizontal, deja ver la
///   escena) y <see cref="WindowFrame"/> (Misiones, Tareas, Áreas: a pantalla completa). Sus filas, chips y botones: <see cref="Row"/>,
///   <see cref="Go"/>, <see cref="SetRow"/>…
/// · LOS MODALES: toda ventana que se dibuja con este kit se apunta sola (<see cref="MarkModal"/>); <see cref="ModalOpen"/> también ve los
///   diálogos que ya llevaban su propio indicador. Con un modal abierto, el mando se oculta (<see cref="TouchControls"/>), como en CodeShip
///   (action-pad.css: «Hidden wherever reglas.md hides the controls»).
/// </summary>
public static class CsUi
{
    // ── LA MEDIDA ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    /// <summary>La pantalla de referencia de CodeShip (docs/design/reglas.md §5.5: «iPhone 13, 390×664 visibles»; en horizontal, 750×342).</summary>
    public const float PortraitW = 390f, PortraitH = 664f, LandscapeW = 750f, LandscapeH = 342f;

    /// <summary>Píxeles de pantalla por píxel CSS de CodeShip en una pantalla de <paramref name="w"/>×<paramref name="h"/> (lógica pura).</summary>
    public static float DpFor(float w, float h)
    {
        bool portrait = h > w;
        return Mathf.Max(0.75f, Mathf.Min(w / (portrait ? PortraitW : LandscapeW), h / (portrait ? PortraitH : LandscapeH)));
    }

    /// <summary>¿Pantalla táctil de la web ligera? Ahí todo va a la medida de CodeShip en el móvil; en el escritorio, a 1 px por px.</summary>
    public static bool Mobile => WebLigera.Active && WebLigera.Touch;
    public static float Dp => Mobile ? DpFor(Screen.width, Screen.height) : 1f;
    public static float Px(float css) => css * Dp;
    public static int Fs(float css) => Mathf.Max(9, Mathf.RoundToInt(css * Dp));
    /// <summary>CodeShip pasa a horizontal con <c>min-aspect-ratio: 1/1</c>.</summary>
    public static bool Landscape => Screen.width >= Screen.height;

    // ── LOS MODALES ───────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    static int _modalFrame = -10;
    /// <summary>Lo llama cada ventana modal al dibujarse (las del kit, solas). Vale este fotograma y el siguiente.</summary>
    public static void MarkModal() => _modalFrame = Time.frameCount;

    /// <summary>¿Hay alguna ventana o diálogo encima del juego? Con uno, el mando se oculta.</summary>
    public static bool ModalOpen => Time.frameCount - _modalFrame <= 1 || OpenModals.Count > 0;

    /// <summary>Los que llevan su propio indicador (aunque no se dibujen con el kit). Para la prueba y el registro.</summary>
    public static List<string> OpenModals
    {
        get
        {
            _open.Clear();
            if (GameKeys.DialogOpen) _open.Add(GameKeys.OpenDialogs);
            if (HudModals.Current != HudModals.Modal.None) _open.Add(HudModals.Current.ToString());
            if (Grimorio.IsOpen) _open.Add("Grimorio");
            if (SpellConsole.Capturing) _open.Add("lanzador");
            if (ProgressPanel.Open) _open.Add("progreso");
            if (MicroRootMenu.Open) _open.Add("menú");   // el desplegable ☰ crece hacia abajo y caería sobre los botones
            if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) _open.Add("diálogo");
            return _open;
        }
    }
    static readonly List<string> _open = new List<string>();

    /// <summary>
    /// ¿Se dibuja la ventana en este evento? Con GUILayout, una ventana que se abre a mitad de fotograma (un clic) no tiene su pasada de
    /// Layout y Unity se queja: solo se dibuja si estaba abierta en el último Layout. <paramref name="laid"/> es el recuerdo de cada ventana.
    /// </summary>
    public static bool Draw(ref bool laid, bool open)
    {
        if (Event.current.type == EventType.Layout) laid = open;
        return open && laid;
    }

    /// <summary>El toque que pulsó un botón del mando (y abrió esta ventana) no es un clic para ella (CodeShip: swallowGhostClick).</summary>
    static void SwallowPadTouch()
    {
        EventType t = Event.current.type;
        if (TouchControls.PadTouch && (t == EventType.MouseDown || t == EventType.MouseUp || t == EventType.MouseDrag)) Event.current.Use();
    }

    // ── LA PALETA (el CSS de CodeShip) ─────────────────────────────────────────────────────────────────────────────────────────────────
    static Color Hex(uint rgba) => new Color32((byte)(rgba >> 24), (byte)(rgba >> 16), (byte)(rgba >> 8), (byte)rgba);
    public static readonly Color SheetBg = Hex(0x05060cf2);        // .duel.sheet, .settings-sheet
    public static readonly Color Lilac = Hex(0xb9a8ffff);          // .duel-title, .set-title
    public static readonly Color SheetText = Hex(0xe8e4ffff);
    public static readonly Color Where = Hex(0xbfefffff);          // .set-where
    public static readonly Color Muted = Hex(0xa9b8c8ff);          // .set-row small
    public static readonly Color Note = Hex(0x8a96a8ff);           // .set-note
    public static readonly Color RowText = Hex(0xe6dfd3ff);        // .mw-pick
    public static readonly Color Amber = Hex(0xffb347ff);          // .mw-lock, .set-row em.off
    public static readonly Color Yellow = Hex(0xffd27fff);         // .board .chore em, ventana abierta en el menú

    // ── LAS PIEZAS (texturas y estilos, rehechos si cambia la medida al girar la pantalla) ────────────────────────────────────────────
    static int _builtFor = -1;
    static Texture2D _sheet, _window, _row, _go, _goOff, _tool, _pill, _setRow, _chip, _chipOn, _primary;
    public static GUIStyle SheetTitle, SheetWhere, Body, Section, NoteStyle, RowTitle, RowDesc, RowSmall, RowBox, GoStyle, GoOffStyle,
        ToolStyle, PillStyle, XStyle, SetRowStyle, SetValueOn, SetValueOff, ChipStyle, ChipOnStyle, PrimaryStyle, WindowTitle,
        WindowTitleLeft, WindowX, SetRowText;

    /// <summary>Prepara las piezas para esta pantalla. Dentro de <c>OnGUI</c> (los estilos parten de <c>GUI.skin</c>).</summary>
    public static void Prepare()
    {
        int key = Mathf.RoundToInt(Dp * 100f);
        if (key == _builtFor && SheetTitle != null) return;
        _builtFor = key;
        // Al girar la pantalla: las piezas de la medida anterior no se quedan en memoria.
        foreach (Texture2D old in new[] { _sheet, _window, _row, _go, _goOff, _tool, _pill, _setRow, _chip, _chipOn, _primary })
            if (old != null) Object.Destroy(old);
        _bgs.Clear();
        int R(float css) => Mathf.Max(2, Mathf.RoundToInt(css * Dp));
        float line = Mathf.Max(1f, Mathf.Round(Dp));
        Texture2D Box(Color fill, Color rim, float radiusCss)
        {
            int r = R(radiusCss), n = r * 2 + 2;
            return CsTheme.Shape(n, n, r, (u, v) => fill, rim, line);
        }
        _sheet = Box(SheetBg, Color.clear, 18f);
        // .nav-map: radial-gradient(ellipse at 50% 40%, #0a1a26f2, #02060af7 75%)
        _window = CsTheme.Shape(64, 64, 0f, CsTheme.Radial(0.5f, 0.4f, Hex(0x0a1a26f2), Hex(0x02060af7), 0.75f), Color.clear, 0f);
        _row = Box(Hex(0x0b1c24ff), Hex(0xffffff22), 10f);                 // .mw-row
        _go = Box(CsTheme.Cyan, Color.clear, 10f);                          // .mw-go, .tb-go
        _goOff = Box(Hex(0x23343cff), Hex(0x9fb3bd88), 10f);                // .mw-row.locked .mw-go
        _tool = Box(Hex(0x0b2a36cc), Hex(0x7fdcff66), 10f);                 // .nav-tools button
        _pill = Box(Hex(0xffffff12), Hex(0xb9a8ff55), 22f);                 // .duel-tools button, .set-tools button
        _setRow = Box(Hex(0xffffff0d), Hex(0xb9a8ff33), 12f);               // .set-row
        _chip = Box(Hex(0x13243aff), Hex(0x2c4a6aff), 22f);                 // .board-head .chip
        _chipOn = Box(CsTheme.Cyan, CsTheme.Cyan, 22f);
        _primary = Box(Hex(0x58d6ffff), Hex(0x58d6ffff), 10f);             // .card-actions .primary

        GUIStyle Label(float size, Color c, bool bold = false, bool wrap = true)
        {
            var s = new GUIStyle(GUI.skin.label) { fontSize = Fs(size), richText = true, wordWrap = wrap, fontStyle = bold ? FontStyle.Bold : FontStyle.Normal, padding = new RectOffset(0, 0, 0, 0), margin = new RectOffset(0, 0, R(2f), R(2f)) };
            s.normal.textColor = s.hover.textColor = s.active.textColor = s.focused.textColor = c;
            return s;
        }
        GUIStyle Button(Texture2D bg, Texture2D press, float size, Color c, bool bold, float radiusCss, float padX)
        {
            int b = R(radiusCss);
            var s = new GUIStyle(GUI.skin.button) { fontSize = Fs(size), richText = true, wordWrap = false, alignment = TextAnchor.MiddleCenter, fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                border = new RectOffset(b, b, b, b), padding = new RectOffset(R(padX), R(padX), R(6f), R(6f)), margin = new RectOffset(0, 0, 0, 0) };
            foreach (GUIStyleState st in new[] { s.normal, s.hover, s.focused, s.onNormal, s.onHover, s.onFocused }) { st.background = bg; st.textColor = c; }
            s.active.background = s.onActive.background = press; s.active.textColor = s.onActive.textColor = c;
            return s;
        }
        SheetTitle = Label(13f, Lilac, false, false);
        SheetWhere = Label(12f, Where);
        WindowTitle = Label(18f, Color.white, true, false);                 // .mw-title
        Body = Label(14f, SheetText);
        Section = Label(12f, CsTheme.Cyan, true, false);
        NoteStyle = Label(11f, Note);
        RowTitle = Label(14f, Color.white, true);
        RowDesc = Label(12f, new Color(RowText.r, RowText.g, RowText.b, 0.85f));
        RowSmall = Label(11f, new Color(RowText.r, RowText.g, RowText.b, 0.7f));
        RowBox = new GUIStyle(GUI.skin.box) { border = new RectOffset(R(10f), R(10f), R(10f), R(10f)), padding = new RectOffset(R(10f), R(4f), R(4f), R(4f)), margin = new RectOffset(0, 0, 0, R(6f)) };
        RowBox.normal.background = _row;
        GoStyle = Button(_go, _tool, 13f, Hex(0x001018ff), true, 10f, 10f);
        GoOffStyle = Button(_goOff, _goOff, 13f, Hex(0x9fb3bdff), true, 10f, 10f);
        ToolStyle = Button(_tool, _go, 13f, CsTheme.Text, false, 10f, 10f);
        PillStyle = Button(_pill, _setRow, 12f, Hex(0xd9d2ffff), false, 22f, 12f);
        XStyle = new GUIStyle(PillStyle) { fontSize = Fs(20f), padding = new RectOffset(0, 0, 0, 0) };
        SetRowStyle = Button(_setRow, _pill, 14f, SheetText, false, 12f, 12f);
        SetRowStyle.alignment = TextAnchor.MiddleLeft; SetRowStyle.wordWrap = true;
        SetValueOn = Label(14f, CsTheme.Green, true, false); SetValueOn.alignment = TextAnchor.MiddleRight;
        SetValueOff = new GUIStyle(SetValueOn); SetValueOff.normal.textColor = Amber;
        ChipStyle = Button(_chip, _chipOn, 12f, Hex(0xcfe8f5ff), false, 22f, 10f);
        ChipOnStyle = Button(_chipOn, _chipOn, 12f, Hex(0x06202bff), false, 22f, 10f);
        PrimaryStyle = Button(_primary, _go, 15f, Hex(0x001018ff), true, 10f, 10f);
        WindowTitleLeft = new GUIStyle(WindowTitle) { alignment = TextAnchor.MiddleLeft };
        WindowX = new GUIStyle(ToolStyle) { fontSize = Fs(20f) };
        SetRowText = new GUIStyle(SetRowStyle); SetRowText.normal.background = null;
    }

    /// <summary>El alto mínimo de lo que se toca: 44 px CSS (reglas.md §5.5, Apple HIG).</summary>
    public static float Touch => Px(44f);

    // ── LA HOJA (retos, ajustes, Grimorio, opciones): .duel.sheet / .settings-sheet ─────────────────────────────────────────────────
    /// <summary>
    /// Dónde va la hoja en una pantalla de <paramref name="w"/>×<paramref name="h"/> px con medida <paramref name="dp"/> (lógica pura):
    /// en vertical, abajo desde el 38 % del alto a todo lo ancho (.settings-sheet; la de un reto, <paramref name="challenge"/>, desde el 30 %
    /// en un móvil bajo, ≤ 760 px CSS: .duel.sheet); en horizontal, a la derecha, todo el alto y min(480 px, 46 %) de ancho.
    /// </summary>
    public static Rect SheetRectFor(float w, float h, float dp, bool challenge = false)
    {
        if (w >= h) { float sw = Mathf.Min(480f * dp, w * 0.46f); return new Rect(w - sw, 0f, sw, h); }
        float top = h * (challenge && h / dp <= 760f ? 0.30f : 0.38f);
        return new Rect(0f, top, w, h - top);
    }

    /// <summary>Lo de dentro de la hoja: 16 px de margen; en vertical, el contenido no pasa de 560 px de ancho (como CodeShip).</summary>
    public static Rect SheetInner(Rect sheet)
    {
        float pad = Px(16f), side = Landscape ? pad : Mathf.Max(pad, (sheet.width - Px(560f)) * 0.5f);
        return new Rect(sheet.x + side, sheet.y + pad, sheet.width - 2f * side, sheet.height - 2f * pad);
    }

    /// <summary>Dibuja el fondo de la hoja, la apunta como modal y devuelve su interior.</summary>
    public static Rect SheetFrame(bool challenge = false)
    {
        Prepare();
        MarkModal();
        SwallowPadTouch();
        Rect s = SheetRectFor(Screen.width, Screen.height, Dp, challenge);
        float r = Px(18f);
        // Las esquinas redondas solo hacia la escena: la caja se alarga por fuera del borde de la pantalla.
        Rect bg = Landscape ? new Rect(s.x, s.y - r, s.width + r, s.height + 2f * r) : new Rect(s.x - r, s.y, s.width + 2f * r, s.height + r);
        GUI.Box(bg, GUIContent.none, Bg(_sheet, 18f));
        return SheetInner(s);
    }

    /// <summary>
    /// La cabecera de la hoja (.duel-top / .set-top): el título en versalitas lila, debajo dónde, y a la derecha sus botones en píldora y la ✕.
    /// Avanza <paramref name="y"/>. Devuelve el índice del botón tocado (<paramref name="tools"/>; la ✕ es <c>tools.Length</c>) o −1.
    /// </summary>
    public static int SheetHead(Rect inner, ref float y, string title, string where, params string[] tools)
    {
        float t = Touch, gap = Px(8f), x = inner.xMax;
        int hit = -1;
        if (GUI.Button(new Rect(x - t, y, t, t), "×", XStyle)) hit = tools.Length;
        x -= t + gap;
        for (int i = tools.Length - 1; i >= 0; i--)
        {
            float w = Mathf.Max(t, PillStyle.CalcSize(new GUIContent(tools[i])).x);
            if (GUI.Button(new Rect(x - w, y, w, t), tools[i], PillStyle)) hit = i;
            x -= w + gap;
        }
        float tw = x - inner.x;
        GUI.Label(new Rect(inner.x, y + Px(2f), tw, Px(18f)), (title ?? "").ToUpperInvariant(), SheetTitle);
        float wh = string.IsNullOrEmpty(where) ? 0f : SheetWhere.CalcHeight(new GUIContent(where), tw);
        if (wh > 0f) GUI.Label(new Rect(inner.x, y + Px(20f), tw, wh), where, SheetWhere);
        y += Mathf.Max(t, Px(20f) + wh) + Px(8f);
        return hit;
    }

    // ── LA VENTANA A PANTALLA COMPLETA (Misiones, Tareas, Áreas): .nav-map + .map-window ──────────────────────────────────────────────
    public static Rect WindowFrame()
    {
        Prepare();
        MarkModal();
        SwallowPadTouch();
        var r = new Rect(0f, 0f, Screen.width, Screen.height);
        GUI.DrawTexture(r, _window, ScaleMode.StretchToFill);
        return r;
    }

    /// <summary>
    /// La cabecera de la ventana (.nav-head): el título a la izquierda y los botones cuadrados de 44 px a la derecha (el último, la ✕).
    /// Devuelve el índice del botón tocado (la ✕ es <c>tools.Length</c>) o −1, y el rectángulo de lo que queda debajo.
    /// </summary>
    public static int WindowHead(Rect r, string title, out Rect body, params string[] tools)
    {
        float t = Touch, gap = Px(6f), pad = Px(12f), y = r.y + Px(10f), x = r.xMax - pad;
        int hit = -1;
        if (GUI.Button(new Rect(x - t, y, t, t), "×", WindowX)) hit = tools.Length;
        x -= t + gap;
        for (int i = tools.Length - 1; i >= 0; i--)
        {
            float w = Mathf.Max(t, ToolStyle.CalcSize(new GUIContent(tools[i])).x);
            if (GUI.Button(new Rect(x - w, y, w, t), tools[i], ToolStyle)) hit = i;
            x -= w + gap;
        }
        GUI.Label(new Rect(r.x + Px(14f), y, x - r.x - Px(14f), t), title, WindowTitleLeft);
        // La lista: 14 px de margen; en una pantalla ancha, centrada a 640 px como mucho para que se lea.
        float top = y + t + Px(10f), side = Mathf.Max(Px(14f), (r.width - Px(640f)) * 0.5f);
        body = new Rect(r.x + side, top, r.width - 2f * side, r.yMax - top - Px(14f));
        return hit;
    }

    // ── LAS FILAS (para GUILayout, dentro de una vista con scroll) ────────────────────────────────────────────────────────────────────
    /// <summary>Una fila de lista (.mw-row): título, descripción y detalle a la izquierda, su botón (.mw-go) a la derecha. true = tocado.</summary>
    public static bool Row(string title, string desc, string detail, string go, bool enabled)
    {
        GUILayout.BeginHorizontal(RowBox);
        GUILayout.BeginVertical();
        GUILayout.Label(title, RowTitle);
        if (!string.IsNullOrEmpty(desc)) GUILayout.Label(desc, RowDesc);
        if (!string.IsNullOrEmpty(detail)) GUILayout.Label(detail, RowSmall);
        GUILayout.EndVertical();
        bool hit = false;
        if (!string.IsNullOrEmpty(go))
        {
            GUILayout.BeginVertical(GUILayout.Width(Px(60f)));
            GUILayout.FlexibleSpace();
            bool was = GUI.enabled;
            GUI.enabled = was && enabled;
            hit = GUILayout.Button(go, enabled ? GoStyle : GoOffStyle, GUILayout.MinWidth(Px(52f)), GUILayout.MinHeight(Touch));
            GUI.enabled = was;
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
        return hit && enabled;
    }

    /// <summary>Una fila de ajustes (.set-row): título y explicación a la izquierda, su valor a la derecha (verde puesto, ámbar quitado). true = tocada.</summary>
    public static bool SetRow(string title, string small, string value, bool on)
    {
        string text = $"{title}" + (string.IsNullOrEmpty(small) ? "" : $"\n<size={Fs(11f)}><color=#a9b8c8>{small}</color></size>");
        Rect r = GUILayoutUtility.GetRect(new GUIContent(text), SetRowStyle, GUILayout.MinHeight(Px(52f)), GUILayout.ExpandWidth(true));
        float vw = string.IsNullOrEmpty(value) ? 0f : SetValueOn.CalcSize(new GUIContent(value)).x + Px(12f);
        bool hit = GUI.Button(r, GUIContent.none, SetRowStyle);
        SetRowText.padding.right = Mathf.RoundToInt(vw + Px(12f));   // el texto no se mete bajo el valor
        GUI.Label(r, text, SetRowText);
        if (vw > 0f) GUI.Label(new Rect(r.xMax - vw - Px(12f), r.y, vw, r.height), value, on ? SetValueOn : SetValueOff);
        GUILayout.Space(Px(8f));
        return hit;
    }

    /// <summary>Un chip de filtro (.chip): redondo, cian si está puesto. true = tocado.</summary>
    public static bool Chip(string label, bool on) => GUILayout.Button(label, on ? ChipOnStyle : ChipStyle, GUILayout.MinHeight(Touch), GUILayout.ExpandWidth(false));

    /// <summary>Un encabezado de grupo dentro de una lista.</summary>
    public static void SectionLabel(string text) { GUILayout.Space(Px(6f)); GUILayout.Label(text.ToUpperInvariant(), Section); }

    static readonly Dictionary<Texture2D, GUIStyle> _bgs = new Dictionary<Texture2D, GUIStyle>();
    static GUIStyle Bg(Texture2D t, float radiusCss)
    {
        if (_bgs.TryGetValue(t, out GUIStyle s) && s.normal.background == t) return s;
        int b = Mathf.Max(2, Mathf.RoundToInt(radiusCss * Dp));
        s = new GUIStyle { border = new RectOffset(b, b, b, b) };
        s.normal.background = t;
        return _bgs[t] = s;
    }
}
