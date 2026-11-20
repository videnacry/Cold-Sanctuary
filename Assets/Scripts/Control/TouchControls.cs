using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CONTROLES TÁCTILES: LOS JOY-CON DE LA SWITCH SOBRE LA PANTALLA (2026-11-19, Berón: «en CodeShip se ha pasado a recrear los mandos de la
/// Nintendo Switch sobre la pantalla para los controles, ¿se podría implementar así para mobile aquí?»). Sustituye a la primera versión
/// (2026-11-13, web ligera: dos joysticks y cuatro botones en arco). Todo el juego lee el teclado por <see cref="GameKeys"/>, así que el mando
/// solo le da valores virtuales; el teclado sigue igual:
///  · JOYSTICK DE ANDAR abajo a la izquierda (el dedo que empieza en el 40 % izquierdo, fuera de un botón; el aro aparece donde se apoya) →
///    <see cref="GameKeys.VirtualMove"/> (WASD);
///  · JOYSTICK DE LA CÁMARA abajo a la derecha (fijo) → <see cref="GameKeys.VirtualLook"/>; ARRASTRAR en el resto de la mitad derecha también mira;
///  · los 16 BOTONES de un par de Joy-Con, como en CodeShip (src/ui/action-pad.ts): la cruceta, L/ZL, «−» y captura a la izquierda; el rombo
///    A/B/X/Y, R/ZR, «+» e inicio a la derecha. Qué pulsa cada uno: <see cref="JoyConMap"/>. Redondos, oscuros y translúcidos, con su
///    marca en blanco y lo que hacen debajo; el que está puesto (Correr) o pulsado se enciende en naranja (<see cref="CsTheme"/>).
/// En horizontal, cada grupo de botones va al lado de su joystick; en vertical, encima (se recoloca cada fotograma). Los toques sobre los
/// menús IMGUI siguen llegando como clics (Unity simula el ratón con los toques). Solo se crea en la web ligera con pantalla táctil
/// (<see cref="WebLigera"/>). Diseño y lista de cambios: docs/mandos-tactiles.md.
/// </summary>
[DefaultExecutionOrder(-1000)]   // antes que el jugador y las misiones: lo que pulsa este fotograma lo leen este mismo fotograma
public class TouchControls : MonoBehaviour
{
    [Tooltip("Arrastrar para mirar: píxeles → unidades de «Mouse X/Y» (el ratón da ~0,1 por píxel).")]
    public float dragSensitivity = 0.12f;
    [Tooltip("Joystick de la cámara a tope: unidades de «Mouse X/Y» por segundo (×2 de sensibilidad del jugador ≈ 100 °/s de giro).")]
    public float stickYawSpeed = 50f, stickPitchSpeed = 30f;
    public bool dragToLook = true;
    [Tooltip("Transparencia de los botones en reposo (CodeShip: 0,62).")]
    [Range(0.2f, 1f)] public float idleAlpha = 0.62f;
    const KeyCode LookButton = KeyCode.Mouse1;   // PlayerController.cameraLookButton por defecto
    const float DeadZone = 0.12f;

    int _moveFinger = -1, _lookFinger = -1, _dragFinger = -1;
    Vector2 _moveOrigin, _moveNow;   // pantalla (origen abajo-izquierda)
    Vector2 _lookNow;                // IMGUI (origen arriba-izquierda)
    readonly Dictionary<int, JoyConMap.Pad> _fingerOnButton = new Dictionary<int, JoyConMap.Pad>();   // dedo → botón mantenido
    readonly HashSet<JoyConMap.Pad> _toggled = new HashSet<JoyConMap.Pad>();
    readonly Dictionary<JoyConMap.Pad, float> _flash = new Dictionary<JoyConMap.Pad, float>();         // el destello de un toque
    readonly Dictionary<JoyConMap.Pad, KeyCode> _heldKey = new Dictionary<JoyConMap.Pad, KeyCode>();   // la tecla que se mantiene (por si cambia el contexto)
    Layout _lay;
    int _layW, _layH;

    /// <summary>(2026-11-16, prueba de la web: WebPrueba) Un empuje del joystick de andar sin dedo; null = el del dedo.</summary>
    public static Vector2? TestMove;

    public static TouchControls Ensure()
    {
        TouchControls t = FindAnyObjectByType<TouchControls>();
        if (t != null) return t;
        var go = new GameObject("TouchControls");
        DontDestroyOnLoad(go);
        return go.AddComponent<TouchControls>();
    }

    // ── LA MEDIDA (en píxeles; IMGUI: origen arriba-izquierda). Lógica pura: la prueba la comprueba en varias pantallas. ─────────────
    public struct Layout
    {
        public float unit, stickRadius;
        /// <summary>El centro del joystick de andar en reposo y el del de la cámara (fijo).</summary>
        public Vector2 moveRest, lookCenter;
        /// <summary>El sitio de cada botón (cuadrado para los redondos y la cruceta; 2:1 para los gatillos).</summary>
        public Dictionary<JoyConMap.Pad, Rect> rects;
        /// <summary>La zona del joystick de andar: un dedo que empieza aquí (y no en un botón) lo mueve.</summary>
        public Rect moveZone;
    }

    public static Layout Compute(float w, float h)
    {
        float shortSide = Mathf.Min(w, h);
        bool portrait = h > w;
        // Un botón: ~10 % del lado corto (el pulgar). En horizontal los dos mandos ocupan ~13,3 botones de ancho: en una pantalla 4:3
        // (tableta) se encogen para no pisarse; en vertical ocupan ~8,4.
        float u = Mathf.Max(36f, Mathf.Min(shortSide * 0.105f, w / (portrait ? 9f : 14f)));
        float m = u * 0.3f, R = u * 1.05f, gap = u * 0.35f;
        var L = new Layout { unit = u, stickRadius = R, rects = new Dictionary<JoyConMap.Pad, Rect>() };
        L.moveRest = new Vector2(m + R, h - m - R);
        L.lookCenter = new Vector2(w - m - R, h - m - R);
        L.moveZone = new Rect(0f, 0f, w * 0.4f, h);

        // El centro de cada grupo de 4 (la cruceta, el rombo): al lado del joystick en horizontal, encima en vertical.
        float cluster = u * 1.02f;                                // distancia del centro del grupo a cada botón
        Vector2 leftC, rightC;
        if (!portrait)
        {
            leftC = new Vector2(L.moveRest.x + R + gap + cluster + u * 0.5f, h - m - cluster - u * 0.5f);
            rightC = new Vector2(L.lookCenter.x - R - gap - cluster - u * 0.5f, leftC.y);
        }
        else
        {
            leftC = new Vector2(m + cluster + u * 0.5f, L.moveRest.y - R - gap - cluster - u * 0.5f);
            rightC = new Vector2(w - m - cluster - u * 0.5f, leftC.y);
        }
        Rect Sq(Vector2 at, float size) => new Rect(at.x - size * 0.5f, at.y - size * 0.5f, size, size);
        float b = u * 0.95f;
        L.rects[JoyConMap.Pad.Up] = Sq(leftC + new Vector2(0f, -cluster), b);
        L.rects[JoyConMap.Pad.Down] = Sq(leftC + new Vector2(0f, cluster), b);
        L.rects[JoyConMap.Pad.Left] = Sq(leftC + new Vector2(-cluster, 0f), b);
        L.rects[JoyConMap.Pad.Right] = Sq(leftC + new Vector2(cluster, 0f), b);
        L.rects[JoyConMap.Pad.X] = Sq(rightC + new Vector2(0f, -cluster), b);
        L.rects[JoyConMap.Pad.B] = Sq(rightC + new Vector2(0f, cluster), b);
        L.rects[JoyConMap.Pad.Y] = Sq(rightC + new Vector2(-cluster, 0f), b);
        L.rects[JoyConMap.Pad.A] = Sq(rightC + new Vector2(cluster, 0f), b);

        // Los gatillos (píldoras 2:1): L/R abajo, ZL/ZR encima, sobre el joystick (horizontal) o sobre el grupo (vertical), hacia fuera.
        float ph = u * 0.62f, pw = ph * 2f;
        float topOfLeft = portrait ? leftC.y - cluster - b * 0.5f : L.moveRest.y - R;
        float lx = portrait ? m : L.moveRest.x - pw * 0.5f, rx = portrait ? w - m - pw : L.lookCenter.x - pw * 0.5f;
        float ly = topOfLeft - gap - ph, zy = ly - ph - gap * 0.6f;
        L.rects[JoyConMap.Pad.L] = new Rect(lx, ly, pw, ph);
        L.rects[JoyConMap.Pad.ZL] = new Rect(lx, zy, pw, ph);
        L.rects[JoyConMap.Pad.R] = new Rect(rx, ly, pw, ph);
        L.rects[JoyConMap.Pad.ZR] = new Rect(rx, zy, pw, ph);

        // «−» y captura, «+» e inicio: pequeños, en la esquina de dentro de cada grupo (arriba el «−»/«+», abajo captura/inicio).
        float s = u * 0.68f, inner = cluster + b * 0.5f + gap * 0.5f + s * 0.5f;
        L.rects[JoyConMap.Pad.Minus] = Sq(leftC + new Vector2(inner, -cluster), s);
        L.rects[JoyConMap.Pad.Capture] = Sq(leftC + new Vector2(inner, cluster), s);
        L.rects[JoyConMap.Pad.Plus] = Sq(rightC + new Vector2(-inner, -cluster), s);
        L.rects[JoyConMap.Pad.Home] = Sq(rightC + new Vector2(-inner, cluster), s);
        return L;
    }

    static Vector2 Gui(Vector2 screen) => new Vector2(screen.x, Screen.height - screen.y);   // pantalla (abajo-izq.) → IMGUI (arriba-izq.)

    void Update()
    {
        if (_lay.rects == null || _layW != Screen.width || _layH != Screen.height) { _lay = Compute(Screen.width, Screen.height); _layW = Screen.width; _layH = Screen.height; }   // solo al girar la pantalla
        Vector2? look = null;
        bool lookHeld = false;
        float R = _lay.stickRadius;
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            Vector2 g = Gui(t.position);
            bool ended = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
            if (t.phase == TouchPhase.Began)
            {
                if (HitButton(g, out JoyConMap.Pad p)) { Press(p, true); _fingerOnButton[t.fingerId] = p; continue; }
                if (_lookFinger < 0 && (g - _lay.lookCenter).magnitude <= R * 1.25f) { _lookFinger = t.fingerId; _lookNow = g; continue; }
                if (_moveFinger < 0 && _lay.moveZone.Contains(g)) { _moveFinger = t.fingerId; _moveOrigin = _moveNow = t.position; continue; }
                if (_dragFinger < 0 && dragToLook && !_lay.moveZone.Contains(g)) { _dragFinger = t.fingerId; continue; }
            }
            if (_fingerOnButton.TryGetValue(t.fingerId, out JoyConMap.Pad held)) { if (ended) { Press(held, false); _fingerOnButton.Remove(t.fingerId); } }
            else if (t.fingerId == _moveFinger) { if (ended) _moveFinger = -1; else _moveNow = t.position; }
            else if (t.fingerId == _lookFinger) { if (ended) _lookFinger = -1; else _lookNow = g; }
            else if (t.fingerId == _dragFinger)
            {
                if (ended) _dragFinger = -1;
                else if (_lookFinger < 0) { look = t.deltaPosition * dragSensitivity; lookHeld = true; }   // el joystick de la cámara manda
            }
        }
        if (Input.touchCount == 0)
        {
            _moveFinger = _lookFinger = _dragFinger = -1;
            foreach (JoyConMap.Pad p in new List<JoyConMap.Pad>(_fingerOnButton.Values)) Press(p, false);   // un dedo perdido no deja nada pulsado
            _fingerOnButton.Clear();
        }

        // EL JOYSTICK DE LA CÁMARA: cuánto se empuja (0..1, con zona muerta) → giro a velocidad constante.
        Vector2 ls = LookStick;
        if (_lookFinger >= 0) { lookHeld = true; look = ls == Vector2.zero ? Vector2.zero : new Vector2(ls.x * stickYawSpeed, ls.y * stickPitchSpeed) * Time.unscaledDeltaTime; }
        GameKeys.VirtualLook = look;
        GameKeys.SetVirtual(LookButton, lookHeld);
        GameKeys.VirtualMove = TestMove ?? MoveStick;   // (la prueba de la web empuja el joystick sin dedo)
    }

    /// <summary>El joystick de andar (−1..1, con zona muerta); arriba en la pantalla = adelante.</summary>
    Vector2 MoveStick => Deflect(_moveFinger >= 0 ? (_moveNow - _moveOrigin) / _lay.stickRadius : Vector2.zero);
    /// <summary>El joystick de la cámara (−1..1): derecha = girar a la derecha, arriba = mirar arriba.</summary>
    Vector2 LookStick
    {
        get
        {
            if (_lookFinger < 0) return Vector2.zero;
            Vector2 d = (_lookNow - _lay.lookCenter) / _lay.stickRadius;
            return Deflect(new Vector2(d.x, -d.y));   // IMGUI: y hacia abajo
        }
    }
    static Vector2 Deflect(Vector2 d) => d.magnitude < DeadZone ? Vector2.zero : d.sqrMagnitude > 1f ? d.normalized : d;

    bool HitButton(Vector2 g, out JoyConMap.Pad pad)
    {
        pad = default;
        if (_lay.rects == null) return false;
        foreach (KeyValuePair<JoyConMap.Pad, Rect> kv in _lay.rects)
        {
            // Un poco más de margen que el dibujo: el pulgar no es preciso.
            Rect r = kv.Value; float k = _lay.unit * 0.08f;
            if (new Rect(r.x - k, r.y - k, r.width + 2f * k, r.height + 2f * k).Contains(g)) { pad = kv.Key; return true; }
        }
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

    void OnDisable()
    {
        GameKeys.VirtualMove = Vector2.zero; GameKeys.VirtualLook = null;
        GameKeys.SetVirtual(LookButton, false);
        foreach (KeyValuePair<JoyConMap.Pad, KeyCode> kv in _heldKey) GameKeys.SetVirtual(kv.Value, false);
        foreach (JoyConMap.Pad p in _toggled) GameKeys.SetVirtual(JoyConMap.Get(p).Key, false);
        _heldKey.Clear(); _toggled.Clear(); _fingerOnButton.Clear();
    }

    // ── EL DIBUJO ────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    static Texture2D _disc, _discOn, _square, _squareOn, _pill, _pillOn, _ring, _knob;
    GUIStyle _face, _faceOn, _label, _labelOn;
    int _styleFor;

    static void Textures()
    {
        if (_disc != null) return;
        _disc = CsTheme.Disc(CsTheme.PadFill, CsTheme.PadRim);
        _discOn = CsTheme.Disc(CsTheme.Orange, CsTheme.OrangeLight);
        _square = CsTheme.Rounded(CsTheme.PadFill, CsTheme.PadRim, 14, 3);
        _squareOn = CsTheme.Rounded(CsTheme.Orange, CsTheme.OrangeLight, 14, 3);
        _pill = CsTheme.Pill(CsTheme.PadFill, CsTheme.PadRim);
        _pillOn = CsTheme.Pill(CsTheme.Orange, CsTheme.OrangeLight);
        _ring = CsTheme.Disc(new Color(0.04f, 0.16f, 0.21f, 0.35f), CsTheme.CyanSoft, 128, 3);
        _knob = CsTheme.Disc(new Color(0.5f, 0.86f, 1f, 0.55f), CsTheme.Cyan, 96, 3);
    }

    void OnGUI()
    {
        if (_lay.rects == null) return;
        GUI.depth = -1500;
        Textures();
        float u = _lay.unit;
        int fs = Mathf.RoundToInt(u * 0.34f);
        if (_face == null || _styleFor != fs)
        {
            _face = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = fs, fontStyle = FontStyle.Bold, clipping = TextClipping.Overflow, wordWrap = false };
            _face.normal.textColor = Color.white;
            _faceOn = new GUIStyle(_face); _faceOn.normal.textColor = CsTheme.Ink;
            _label = new GUIStyle(_face) { fontSize = Mathf.Max(9, Mathf.RoundToInt(u * 0.17f)), fontStyle = FontStyle.Normal, alignment = TextAnchor.UpperCenter };
            _labelOn = new GUIStyle(_label); _labelOn.normal.textColor = CsTheme.Ink;
            _styleFor = fs;
        }
        Color was = GUI.color;

        // Los joysticks: el de andar, su aro donde empezó el dedo (o en reposo) y el pomo donde está; el de la cámara, fijo.
        float R = _lay.stickRadius, knob = u * 0.42f;
        Vector2 mo = _moveFinger >= 0 ? Gui(_moveOrigin) : _lay.moveRest, mv = MoveStick;
        Stick(mo, mo + new Vector2(mv.x, -mv.y) * R, R, knob, _moveFinger >= 0);
        Vector2 lc = _lay.lookCenter, lk = LookStick;
        Stick(lc, lc + new Vector2(lk.x, -lk.y) * R, R, knob, _lookFinger >= 0);

        foreach (KeyValuePair<JoyConMap.Pad, Rect> kv in _lay.rects)
        {
            JoyConMap.Pad p = kv.Key;
            JoyConMap.Button b = JoyConMap.Get(p);
            bool flash = _flash.TryGetValue(p, out float at) && Time.unscaledTime - at < 0.16f;
            bool on = IsOn(p) || flash;
            Rect r = kv.Value;
            if (flash) { float k = r.width * 0.04f; r = new Rect(r.x + k, r.y + k, r.width - 2f * k, r.height - 2f * k); }   // se hunde al tocarlo
            bool pill = p == JoyConMap.Pad.L || p == JoyConMap.Pad.ZL || p == JoyConMap.Pad.R || p == JoyConMap.Pad.ZR;
            bool square = p == JoyConMap.Pad.Up || p == JoyConMap.Pad.Down || p == JoyConMap.Pad.Left || p == JoyConMap.Pad.Right || p == JoyConMap.Pad.Capture;
            GUI.color = new Color(1f, 1f, 1f, on ? 0.95f : idleAlpha);
            GUI.DrawTexture(r, pill ? (on ? _pillOn : _pill) : square ? (on ? _squareOn : _square) : (on ? _discOn : _disc), ScaleMode.StretchToFill);
            GUI.color = new Color(1f, 1f, 1f, on ? 1f : Mathf.Min(1f, idleAlpha + 0.3f));
            bool small = p == JoyConMap.Pad.Plus || p == JoyConMap.Pad.Minus || p == JoyConMap.Pad.Home || p == JoyConMap.Pad.Capture;
            string face = JoyConMap.Face(p);
            if (pill || string.IsNullOrEmpty(b.label) || small)
            {
                // Los gatillos llevan la marca y lo que hacen en la misma línea; los pequeños, el nombre fuera, debajo.
                GUI.Label(r, pill && !string.IsNullOrEmpty(b.label) ? $"{face} <size={_label.fontSize}>{b.label}</size>" : face, RichFace(on));
                if (small && !string.IsNullOrEmpty(b.label))
                {
                    bool above = p == JoyConMap.Pad.Minus || p == JoyConMap.Pad.Plus;   // «−»/«+» arriba, captura/inicio abajo: no tapan la cruceta ni el rombo
                    GUI.Label(new Rect(r.x - u * 0.4f, above ? r.y - u * 0.3f - 1f : r.yMax + 1f, r.width + u * 0.8f, u * 0.3f), b.label, _label);
                }
            }
            else
            {
                GUI.Label(new Rect(r.x, r.y - r.height * 0.1f, r.width, r.height), face, on ? _faceOn : _face);
                GUI.Label(new Rect(r.x - u * 0.3f, r.y + r.height * 0.62f, r.width + u * 0.6f, r.height * 0.4f), b.label, on ? _labelOn : _label);
            }
        }
        GUI.color = was;
    }

    GUIStyle _rich, _richOn;
    GUIStyle RichFace(bool on)
    {
        if (_rich == null || _rich.fontSize != _face.fontSize - 2)
        {
            _rich = new GUIStyle(_face) { richText = true, fontSize = _face.fontSize - 2 };
            _richOn = new GUIStyle(_rich); _richOn.normal.textColor = CsTheme.Ink;
        }
        return on ? _richOn : _rich;
    }

    void Stick(Vector2 center, Vector2 knobAt, float r, float k, bool active)
    {
        GUI.color = new Color(1f, 1f, 1f, active ? 0.9f : 0.6f);
        GUI.DrawTexture(new Rect(center.x - r, center.y - r, r * 2f, r * 2f), _ring, ScaleMode.StretchToFill);
        GUI.color = new Color(1f, 1f, 1f, active ? 0.95f : 0.7f);
        GUI.DrawTexture(new Rect(knobAt.x - k, knobAt.y - k, k * 2f, k * 2f), _knob, ScaleMode.StretchToFill);
    }
}
