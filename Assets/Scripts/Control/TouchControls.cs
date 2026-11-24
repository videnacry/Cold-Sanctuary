using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CONTROLES TÁCTILES: LOS JOY-CON DE LA SWITCH SOBRE LA PANTALLA, COMO EN EL SANTUARIO 1 DE CODESHIP. Rehechos desde cero el 2026-11-20
/// (Berón: «los mandos de switch están muy pequeños y se deben ocultar cuando se muestran modales… mirarlo en vivo y de allí sacar
/// información para su recreación»): medidos en vivo en CodeShip (Playwright, iPhone 13) y copiados de src/ui/action-pad.css y style.css.
/// Todo el juego lee el teclado por <see cref="GameKeys"/>, así que el mando solo le da valores virtuales; el teclado sigue igual.
///  · DOS JOYSTICKS de 120 px fijos abajo, a 22 px de cada lado (MOVERSE / MIRAR): se cogen tocándolos; el pomo de 50 px va hasta el borde.
///    Andar → <see cref="GameKeys.VirtualMove"/>; mirar → <see cref="GameKeys.VirtualLook"/> con la curva de CodeShip (v·|v|). No se mira
///    arrastrando la pantalla (reglas.md §5.7).
///  · LOS 16 BOTONES en dos grupos de 148 px: la cruceta, L/ZL, «−» y captura a la izquierda; el rombo A/B/X/Y, R/ZR, «+» e inicio a la
///    derecha. En horizontal cada grupo va al lado de su joystick; en vertical, encima. Redondos de 50 px, gatillos de 64×44 en píldora y
///    los pequeños de 44; oscuros y translúcidos (0,62), su marca en blanco (17 px) y lo que hacen debajo (9 px); el puesto o pulsado, en
///    naranja. Qué pulsa cada uno: <see cref="JoyConMap"/>.
///  · Todo en píxeles CSS de CodeShip pasados a esta pantalla por <see cref="CsUi.Dp"/>: ocupa la misma parte de la pantalla que allí.
///  · CON UN MODAL ABIERTO (<see cref="CsUi.ModalOpen"/>: el Grimorio, Misiones, Tareas, Áreas, la pausa, un reto, un diálogo…) EL MANDO SE
///    OCULTA y suelta lo que tuviera pulsado; vuelve al cerrarlo. El toque que pulsó un botón del mando no llega como clic a la ventana que
///    abre (en CodeShip, swallowGhostClick).
/// Solo se crea en la web ligera con pantalla táctil (<see cref="WebLigera"/>). Diseño y lista de cambios: docs/sistemas/mandos-tactiles.md §6.
/// </summary>
[DefaultExecutionOrder(-1000)]   // antes que el jugador y las misiones: lo que pulsa este fotograma lo leen este mismo fotograma
public class TouchControls : MonoBehaviour
{
    [Tooltip("Joystick de la cámara a tope: unidades de «Mouse X/Y» por segundo (CodeShip: 2 rad/s de giro y 1,4 de cabeceo).")]
    public float stickYawSpeed = 57f, stickPitchSpeed = 40f;
    const KeyCode LookButton = KeyCode.Mouse1;   // PlayerController.cameraLookButton por defecto

    // ── LA MEDIDA DE CODESHIP (px CSS, action-pad.css y style.css) ────────────────────────────────────────────────────────────────────
    const float Side = 148f, Btn = 50f, Step = 49f, ShoulderW = 64f, ShoulderH = 44f, Small = 44f;
    const float StickSize = 120f, StickInset = 22f, StickBottom = 26f, KnobSize = 50f;
    public const float IdleAlpha = 0.62f;

    int _moveFinger = -1, _lookFinger = -1;
    Vector2 _moveNow, _lookNow;   // IMGUI (origen arriba-izquierda)
    readonly Dictionary<int, JoyConMap.Pad> _fingerOnButton = new Dictionary<int, JoyConMap.Pad>();   // dedo → botón mantenido
    readonly HashSet<int> _padFingers = new HashSet<int>();     // dedos que empezaron en el mando: su clic no es para las ventanas
    readonly HashSet<JoyConMap.Pad> _toggled = new HashSet<JoyConMap.Pad>();
    readonly Dictionary<JoyConMap.Pad, float> _flash = new Dictionary<JoyConMap.Pad, float>();         // el hundido de un toque
    readonly Dictionary<JoyConMap.Pad, KeyCode> _heldKey = new Dictionary<JoyConMap.Pad, KeyCode>();   // la tecla que se mantiene (por si cambia el contexto)
    Layout _lay;
    int _layW, _layH, _endedOnPadFrame = -1;
    bool _hidden;

    /// <summary>(2026-11-16, prueba de la web: WebPrueba) Un empuje del joystick de andar sin dedo; null = el del dedo.</summary>
    public static Vector2? TestMove;
    /// <summary>¿Se ve ahora el mando? (false con un modal abierto).</summary>
    public static bool Showing { get; private set; }
    /// <summary>¿Hay un dedo que empezó en el mando? Su clic simulado no es para ninguna ventana (<see cref="CsUi"/> lo descarta).</summary>
    public static bool PadTouch { get; private set; }

    public static TouchControls Ensure()
    {
        TouchControls t = FindAnyObjectByType<TouchControls>();
        if (t != null) return t;
        var go = new GameObject("TouchControls");
        DontDestroyOnLoad(go);
        return go.AddComponent<TouchControls>();
    }

    // ── LA COLOCACIÓN (en píxeles de pantalla; IMGUI: origen arriba-izquierda). Lógica pura: la prueba la comprueba en varias pantallas. ──
    public struct Layout
    {
        /// <summary>Píxeles de pantalla por píxel CSS de CodeShip.</summary>
        public float dp;
        /// <summary>El lado de un botón del rombo (50 px CSS) y el radio de un joystick (60 px CSS).</summary>
        public float unit, stickRadius;
        public Vector2 moveCenter, lookCenter;
        /// <summary>El sitio de cada botón (cuadrado para los redondos y la cruceta; 64×44 los gatillos).</summary>
        public Dictionary<JoyConMap.Pad, Rect> rects;
    }

    public static Layout Compute(float w, float h)
    {
        float dp = CsUi.DpFor(w, h);
        float W = w / dp, H = h / dp;        // la pantalla en px CSS (≥ 390×664 o 750×342: lo que cabe en CodeShip cabe aquí)
        bool portrait = h > w;
        var L = new Layout { dp = dp, unit = Btn * dp, stickRadius = StickSize * 0.5f * dp, rects = new Dictionary<JoyConMap.Pad, Rect>() };
        float sy = H - StickBottom - StickSize * 0.5f;
        L.moveCenter = new Vector2(StickInset + StickSize * 0.5f, sy) * dp;
        L.lookCenter = new Vector2(W - StickInset - StickSize * 0.5f, sy) * dp;

        // .sp-side: 148×148; horizontal: bottom 20, left/right 154; vertical: bottom 160, left/right 14.
        float boxY = H - (portrait ? 160f : 20f) - Side;
        float lx = portrait ? 14f : 154f, rx = W - (portrait ? 14f : 154f) - Side;
        Rect C(float x, float y, float cw, float ch) => new Rect(x * dp, y * dp, cw * dp, ch * dp);
        // El rombo y la cruceta: arriba, derecha, abajo, izquierda.
        L.rects[JoyConMap.Pad.Up] = C(lx + Step, boxY, Btn, Btn);
        L.rects[JoyConMap.Pad.Right] = C(lx + Side - Btn, boxY + Step, Btn, Btn);
        L.rects[JoyConMap.Pad.Down] = C(lx + Step, boxY + Side - Btn, Btn, Btn);
        L.rects[JoyConMap.Pad.Left] = C(lx, boxY + Step, Btn, Btn);
        L.rects[JoyConMap.Pad.X] = C(rx + Step, boxY, Btn, Btn);
        L.rects[JoyConMap.Pad.A] = C(rx + Side - Btn, boxY + Step, Btn, Btn);
        L.rects[JoyConMap.Pad.B] = C(rx + Step, boxY + Side - Btn, Btn, Btn);
        L.rects[JoyConMap.Pad.Y] = C(rx, boxY + Step, Btn, Btn);
        // Los gatillos: L/R a −50, ZL/ZR a −100 del grupo; en horizontal, 140 px hacia fuera (encima del joystick); en vertical, en su borde.
        float slx = portrait ? lx : lx - 140f, srx = portrait ? rx + Side - ShoulderW : rx + Side + 140f - ShoulderW;
        L.rects[JoyConMap.Pad.L] = C(slx, boxY - 50f, ShoulderW, ShoulderH);
        L.rects[JoyConMap.Pad.ZL] = C(slx, boxY - 100f, ShoulderW, ShoulderH);
        L.rects[JoyConMap.Pad.R] = C(srx, boxY - 50f, ShoulderW, ShoulderH);
        L.rects[JoyConMap.Pad.ZR] = C(srx, boxY - 100f, ShoulderW, ShoulderH);
        // «−» y captura, «+» e inicio (44 px): en horizontal, en las esquinas de dentro (arriba −4, abajo −4, 50 px hacia el centro);
        // en vertical, encima del grupo, por dentro (−50 y −100).
        if (portrait)
        {
            L.rects[JoyConMap.Pad.Minus] = C(lx + Side - Small, boxY - 50f, Small, Small);
            L.rects[JoyConMap.Pad.Capture] = C(lx + Side - Small, boxY - 100f, Small, Small);
            L.rects[JoyConMap.Pad.Plus] = C(rx, boxY - 50f, Small, Small);
            L.rects[JoyConMap.Pad.Home] = C(rx, boxY - 100f, Small, Small);
        }
        else
        {
            L.rects[JoyConMap.Pad.Minus] = C(lx + Side + 50f - Small, boxY - 4f, Small, Small);
            L.rects[JoyConMap.Pad.Capture] = C(lx + Side + 50f - Small, boxY + Side + 4f - Small, Small, Small);
            L.rects[JoyConMap.Pad.Plus] = C(rx - 50f, boxY - 4f, Small, Small);
            L.rects[JoyConMap.Pad.Home] = C(rx - 50f, boxY + Side + 4f - Small, Small, Small);
        }
        return L;
    }

    static Vector2 Gui(Vector2 screen) => new Vector2(screen.x, Screen.height - screen.y);   // pantalla (abajo-izq.) → IMGUI (arriba-izq.)

    void Update()
    {
        if (_lay.rects == null || _layW != Screen.width || _layH != Screen.height) { _lay = Compute(Screen.width, Screen.height); _layW = Screen.width; _layH = Screen.height; }   // solo al girar la pantalla
        bool hide = CsUi.ModalOpen;
        if (hide && !_hidden)   // al abrirse un modal: nada se queda pulsado ni andando
        {
            // (revisión PB-169) Los dedos de los joysticks dejan de ser del mando: si no, el modal que se abre solo (un diálogo al llegar
            // andando) no recibiría toques hasta levantarlos. Solo los de los botones siguen: son los del toque fantasma.
            if (_moveFinger >= 0) _padFingers.Remove(_moveFinger);
            if (_lookFinger >= 0) _padFingers.Remove(_lookFinger);
            ReleaseAll();
        }
        _hidden = hide;
        Showing = !hide;

        float R = _lay.stickRadius;
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            Vector2 g = Gui(t.position);
            bool ended = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
            if (ended && _padFingers.Remove(t.fingerId)) _endedOnPadFrame = Time.frameCount;   // su MouseUp llega este fotograma
            if (t.phase == TouchPhase.Began && !hide)
            {
                if (HitButton(g, out JoyConMap.Pad p)) { Press(p, true); _fingerOnButton[t.fingerId] = p; _padFingers.Add(t.fingerId); continue; }
                if (_lookFinger < 0 && (g - _lay.lookCenter).magnitude <= R) { _lookFinger = t.fingerId; _lookNow = g; _padFingers.Add(t.fingerId); continue; }
                if (_moveFinger < 0 && (g - _lay.moveCenter).magnitude <= R) { _moveFinger = t.fingerId; _moveNow = g; _padFingers.Add(t.fingerId); continue; }
            }
            if (_fingerOnButton.TryGetValue(t.fingerId, out JoyConMap.Pad held)) { if (ended) { Press(held, false); _fingerOnButton.Remove(t.fingerId); } }
            else if (t.fingerId == _moveFinger) { if (ended) _moveFinger = -1; else _moveNow = g; }
            else if (t.fingerId == _lookFinger) { if (ended) _lookFinger = -1; else _lookNow = g; }
        }
        if (Input.touchCount == 0) { ReleaseAll(); _padFingers.Clear(); }   // un dedo perdido no deja nada pulsado
        PadTouch = _padFingers.Count > 0 || _endedOnPadFrame == Time.frameCount;

        // EL JOYSTICK DE LA CÁMARA: la curva de CodeShip (v·|v|: lo pequeño, fino; a tope, rápido) a velocidad constante.
        Vector2 ls = LookStick;
        bool looking = _lookFinger >= 0;
        GameKeys.VirtualLook = looking ? new Vector2(Curve(ls.x) * stickYawSpeed, Curve(ls.y) * stickPitchSpeed) * Time.unscaledDeltaTime : (Vector2?)null;
        GameKeys.SetVirtual(LookButton, looking);
        GameKeys.VirtualMove = TestMove ?? MoveStick;   // (la prueba de la web empuja el joystick sin dedo)
    }

    static float Curve(float v) => Mathf.Sign(v) * v * v;

    /// <summary>El joystick de andar (−1..1); arriba en la pantalla = adelante.</summary>
    Vector2 MoveStick => Stick(_moveFinger, _moveNow, _lay.moveCenter);
    /// <summary>El joystick de la cámara (−1..1): derecha = girar a la derecha, arriba = mirar arriba.</summary>
    Vector2 LookStick => Stick(_lookFinger, _lookNow, _lay.lookCenter);
    Vector2 Stick(int finger, Vector2 at, Vector2 center)
    {
        if (finger < 0) return Vector2.zero;
        Vector2 d = (at - center) / _lay.stickRadius;
        if (d.sqrMagnitude > 1f) d.Normalize();
        return new Vector2(d.x, -d.y);   // IMGUI: y hacia abajo
    }

    bool HitButton(Vector2 g, out JoyConMap.Pad pad)
    {
        pad = default;
        if (_lay.rects == null) return false;
        foreach (KeyValuePair<JoyConMap.Pad, Rect> kv in _lay.rects)
            if (kv.Value.Contains(g)) { pad = kv.Key; return true; }
        return false;
    }

    /// <summary>Pulsa (o suelta) un botón del mando: toque, mantener o interruptor, según <see cref="JoyConMap"/>. Público para las pruebas.</summary>
    public void Press(JoyConMap.Pad p, bool down)
    {
        JoyConMap.Button b = JoyConMap.Get(p);
        if (down) _flash[p] = Time.unscaledTime;
        switch (b.mode)
        {
            case JoyConMap.Mode.Toggle:
                if (!down) return;
                bool on = _toggled.Add(p);
                if (!on) _toggled.Remove(p);
                GameKeys.SetVirtual(b.Key, on);
                break;
            case JoyConMap.Mode.Hold:
                if (down) { KeyCode press = b.Key; _heldKey[p] = press; GameKeys.SetVirtual(press, true); }
                else if (_heldKey.TryGetValue(p, out KeyCode release)) { GameKeys.SetVirtual(release, false); _heldKey.Remove(p); }
                break;
            default:
                if (down) GameKeys.TapVirtual(b.Key);
                break;
        }
    }

    public bool IsOn(JoyConMap.Pad p) => _toggled.Contains(p) || _heldKey.ContainsKey(p);

    /// <summary>Suelta lo mantenido y los joysticks (al abrirse un modal o al perder los dedos). Correr, si estaba puesto, sigue puesto.</summary>
    void ReleaseAll()
    {
        foreach (KeyValuePair<JoyConMap.Pad, KeyCode> kv in _heldKey) GameKeys.SetVirtual(kv.Value, false);
        _heldKey.Clear();
        _fingerOnButton.Clear();
        _moveFinger = _lookFinger = -1;
        GameKeys.VirtualMove = Vector2.zero; GameKeys.VirtualLook = null;
        GameKeys.SetVirtual(LookButton, false);
    }

    void OnDisable()
    {
        ReleaseAll();
        foreach (JoyConMap.Pad p in _toggled) GameKeys.SetVirtual(JoyConMap.Get(p).Key, false);
        _toggled.Clear(); _padFingers.Clear();
        Showing = PadTouch = false;
    }

    // ── EL DIBUJO (action-pad.css y style.css: .ap-btn, .vstick) ──────────────────────────────────────────────────────────────────────
    static int _texFor = -1;
    static Texture2D _disc, _discOn, _square, _squareOn, _pill, _pillOn, _small, _smallOn, _capture, _captureOn, _ring, _knob;
    GUIStyle _face, _faceOn, _label, _labelOn, _stickLabel;
    int _styleFor = -1;

    static readonly Color Rim = new Color32(0xff, 0xff, 0xff, 0x40), RimOn = new Color32(0xff, 0xd2, 0xa0, 0xff);
    static Texture2D Btn2(int w, int h, float radius, bool on, float rimPx)
    {
        // radial-gradient(circle at 40% 35%, #4a4f57cc, #1c1f24cc 70%), borde 2 px #ffffff40; puesto: #ffd2a0 → #ff9a2e, borde #ffd2a0.
        Color a = on ? (Color)new Color32(0xff, 0xd2, 0xa0, 0xff) : (Color)new Color32(0x4a, 0x4f, 0x57, 0xcc), b = on ? CsTheme.Orange : (Color)new Color32(0x1c, 0x1f, 0x24, 0xcc);
        return CsTheme.Shape(w, h, radius, CsTheme.Radial(0.4f, 0.35f, a, b, 0.7f, (float)w / h), on ? RimOn : Rim, rimPx);
    }

    static void Textures(float dp)
    {
        int key = Mathf.RoundToInt(dp * 100f);
        if (_disc != null && key == _texFor) return;
        _texFor = key;
        foreach (Texture2D old in new[] { _disc, _discOn, _square, _squareOn, _pill, _pillOn, _small, _smallOn, _capture, _captureOn, _ring, _knob })
            if (old != null) Object.Destroy(old);   // al girar la pantalla: las de la medida anterior no se quedan en memoria
        int P(float css) => Mathf.Max(8, Mathf.RoundToInt(css * dp));
        int b = P(Btn), s = P(Small), sw = P(ShoulderW), sh = P(ShoulderH);
        _disc = Btn2(b, b, b * 0.5f, false, 2f * dp); _discOn = Btn2(b, b, b * 0.5f, true, 2f * dp);
        _square = Btn2(b, b, 12f * dp, false, 2f * dp); _squareOn = Btn2(b, b, 12f * dp, true, 2f * dp);                 // la cruceta: border-radius 12px
        _pill = Btn2(sw, sh, sh * 0.5f, false, 2f * dp); _pillOn = Btn2(sw, sh, sh * 0.5f, true, 2f * dp);               // los gatillos: border-radius 22px
        _small = Btn2(s, s, s * 0.5f, false, 2f * dp); _smallOn = Btn2(s, s, s * 0.5f, true, 2f * dp);
        _capture = Btn2(s, s, 8f * dp, false, 2f * dp); _captureOn = Btn2(s, s, 8f * dp, true, 2f * dp);                  // captura: border-radius 8px
        // .vstick: radial-gradient(circle, #ffffff10 0 44%, transparent 45%), #1c1f2433; borde 2 px #ffffff38 (y opacidad 0,72 al dibujar).
        int st = P(StickSize);
        _ring = CsTheme.Shape(st, st, st * 0.5f, (u, v) =>
        {
            float d = Mathf.Sqrt((u - 0.5f) * (u - 0.5f) + (v - 0.5f) * (v - 0.5f)) / 0.7071f;
            return d <= 0.445f ? new Color32(0x2b, 0x2e, 0x33, 0x43) : new Color32(0x1c, 0x1f, 0x24, 0x33);
        }, new Color32(0xff, 0xff, 0xff, 0x38), 2f * dp);
        // .knob: radial-gradient(circle at 42% 38%, #5a5f67, #262a30 72%), borde 3 px #121519 y un aro interior de 4 px #3b4048aa.
        int k = P(KnobSize);
        System.Func<float, float, Color> grad = CsTheme.Radial(0.42f, 0.38f, new Color32(0x5a, 0x5f, 0x67, 0xff), new Color32(0x26, 0x2a, 0x30, 0xff), 0.72f);
        float ringIn = 1f - 2f * (3f + 4f) / KnobSize, ringOut = 1f - 2f * 3f / KnobSize;
        _knob = CsTheme.Shape(k, k, k * 0.5f, (u, v) =>
        {
            float d = 2f * Mathf.Sqrt((u - 0.5f) * (u - 0.5f) + (v - 0.5f) * (v - 0.5f));
            Color c = grad(u, v);
            return d >= ringIn && d < ringOut ? Color.Lerp(c, new Color32(0x3b, 0x40, 0x48, 0xff), 0.67f) : c;
        }, new Color32(0x12, 0x15, 0x19, 0xff), 3f * dp);
    }

    void OnGUI()
    {
        GUI.depth = -1500;
        // El toque que empezó en el mando no es un clic para las ventanas de debajo (ni para la que acaba de abrir): CodeShip swallowGhostClick.
        Event e = Event.current;
        if ((e.type == EventType.MouseDown || e.type == EventType.MouseUp || e.type == EventType.MouseDrag) && PadTouch) { e.Use(); return; }
        if (_hidden || _lay.rects == null || e.type != EventType.Repaint) return;
        float dp = _lay.dp;
        Textures(dp);
        int fs = Mathf.RoundToInt(17f * dp);
        if (_face == null || _styleFor != fs)
        {
            _face = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = fs, fontStyle = FontStyle.Bold, clipping = TextClipping.Overflow, wordWrap = false, padding = new RectOffset(0, 0, 0, 0), richText = false };
            _face.normal.textColor = Color.white;
            _faceOn = new GUIStyle(_face); _faceOn.normal.textColor = CsTheme.Ink;
            _label = new GUIStyle(_face) { fontSize = Mathf.Max(8, Mathf.RoundToInt(9f * dp)), fontStyle = FontStyle.Normal };
            _label.normal.textColor = new Color(1f, 1f, 1f, 0.9f);
            _labelOn = new GUIStyle(_label); _labelOn.normal.textColor = CsTheme.Ink;
            _stickLabel = new GUIStyle(_label) { fontSize = Mathf.Max(8, Mathf.RoundToInt(11f * dp)) };
            _stickLabel.normal.textColor = new Color(1f, 1f, 1f, 0.67f);
            _styleFor = fs;
        }
        Color was = GUI.color;

        // Los joysticks (.vstick 0,72; tocado 0,95), su nombre debajo, y el pomo donde está el dedo.
        DrawStick(_lay.moveCenter, MoveStick, _moveFinger >= 0, "MOVERSE", dp);
        DrawStick(_lay.lookCenter, LookStick, _lookFinger >= 0, "MIRAR", dp);

        foreach (KeyValuePair<JoyConMap.Pad, Rect> kv in _lay.rects)
        {
            JoyConMap.Pad p = kv.Key;
            JoyConMap.Button b = JoyConMap.Get(p);
            bool hit = _flash.TryGetValue(p, out float at) && Time.unscaledTime - at < 0.16f;
            bool on = _toggled.Contains(p);   // naranja: lo que se queda puesto (Correr); pulsar o mantener solo lo hunde
            Rect r = kv.Value;
            if (hit || _heldKey.ContainsKey(p)) { float k = r.width * 0.04f; r = new Rect(r.x + k, r.y + r.height * 0.04f, r.width - 2f * k, r.height * 0.92f); }   // se hunde (scale 0.92)
            bool pill = p == JoyConMap.Pad.L || p == JoyConMap.Pad.ZL || p == JoyConMap.Pad.R || p == JoyConMap.Pad.ZR;
            bool dpad = p == JoyConMap.Pad.Up || p == JoyConMap.Pad.Down || p == JoyConMap.Pad.Left || p == JoyConMap.Pad.Right;
            bool small = p == JoyConMap.Pad.Plus || p == JoyConMap.Pad.Minus || p == JoyConMap.Pad.Home || p == JoyConMap.Pad.Capture;
            Texture2D tex = pill ? (on ? _pillOn : _pill) : dpad ? (on ? _squareOn : _square) : p == JoyConMap.Pad.Capture ? (on ? _captureOn : _capture) : small ? (on ? _smallOn : _small) : (on ? _discOn : _disc);
            GUI.color = new Color(1f, 1f, 1f, on || hit ? 0.95f : IdleAlpha);
            GUI.DrawTexture(r, tex, ScaleMode.StretchToFill);
            // La marca (17 px; la cruceta 14, los gatillos 13, los pequeños 15) y debajo lo que hace (9 px), centradas en la columna.
            float faceCss = dpad ? 14f : pill ? 13f : small ? 15f : 17f;
            _face.fontSize = _faceOn.fontSize = Mathf.RoundToInt(faceCss * dp);
            string label = b.label ?? "";
            float faceH = faceCss * dp, labelH = string.IsNullOrEmpty(label) ? 0f : 11f * dp, top = r.y + (r.height - faceH - labelH) * 0.5f;
            GUI.Label(new Rect(r.x, top, r.width, faceH), JoyConMap.Face(p), on ? _faceOn : _face);
            if (labelH > 0f) GUI.Label(new Rect(r.x - r.width, top + faceH, r.width * 3f, labelH), label, on ? _labelOn : _label);
        }
        GUI.color = was;
    }

    void DrawStick(Vector2 c, Vector2 v, bool active, string name, float dp)
    {
        float r = _lay.stickRadius, k = KnobSize * 0.5f * dp, travel = r;
        GUI.color = new Color(1f, 1f, 1f, active ? 0.95f : 0.72f);
        GUI.DrawTexture(new Rect(c.x - r, c.y - r, 2f * r, 2f * r), _ring, ScaleMode.StretchToFill);
        Vector2 at = c + new Vector2(v.x, -v.y) * travel;
        GUI.DrawTexture(new Rect(at.x - k, at.y - k, 2f * k, 2f * k), _knob, ScaleMode.StretchToFill);
        GUI.Label(new Rect(c.x - r, c.y + r + 2f * dp, 2f * r, 16f * dp), name, _stickLabel);   // span: bottom −18 px
    }
}
