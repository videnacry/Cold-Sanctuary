using UnityEngine;

/// <summary>
/// CONTROLES TÁCTILES (2026-11-13, web ligera: Berón juega en el móvil). El proyecto no tenía entrada táctil: todo el juego lee el teclado
/// por <see cref="GameKeys"/>, así que esto solo le da valores virtuales:
///  · JOYSTICK DE ANDAR abajo a la izquierda (el dedo que empieza en el 40 % izquierdo; el aro aparece donde se apoya) → <see cref="GameKeys.VirtualMove"/> (WASD);
///  · JOYSTICK DE LA CÁMARA abajo a la derecha (fijo; Berón 2026-11-13: «un joystick para la cámara») → <see cref="GameKeys.VirtualLook"/> a
///    velocidad constante según cuánto se empuja, y el botón de mirar mantenido. Tiene PRIORIDAD sobre arrastrar;
///  · ARRASTRAR en el resto de la mitad derecha sigue mirando (opción), si el joystick de la cámara no está en uso;
///  · BOTONES en ARCO sobre el joystick de la cámara: E (hablar, atender), F (usar), Saltar (espacio) y Correr (Mayús, se queda puesto);
///    el que tapara el joystick de andar (pantalla estrecha, en vertical) se oculta. «Intro» (seguir una tarjeta), pequeño, arriba en el centro.
/// Todo a la medida del pulgar: un botón, ~13 % del lado corto de la pantalla; vale en horizontal y en vertical (se recoloca cada fotograma).
/// Los toques sobre los menús IMGUI del juego siguen llegando como clics (Unity simula el ratón con los toques).
/// Solo se crea en la web ligera con pantalla táctil (<see cref="WebLigera"/>).
/// </summary>
[DefaultExecutionOrder(-1000)]   // antes que el jugador y las misiones: lo que pulsa este fotograma lo leen este mismo fotograma
public class TouchControls : MonoBehaviour
{
    [Tooltip("Arrastrar para mirar: píxeles → unidades de «Mouse X/Y» (el ratón da ~0,1 por píxel).")]
    public float dragSensitivity = 0.12f;
    [Tooltip("Joystick de la cámara a tope: unidades de «Mouse X/Y» por segundo (×2 de sensibilidad del jugador ≈ 100 °/s de giro).")]
    public float stickYawSpeed = 50f, stickPitchSpeed = 30f;
    public bool dragToLook = true;
    const KeyCode LookButton = KeyCode.Mouse1;   // PlayerController.cameraLookButton por defecto
    const float DeadZone = 0.12f;

    int _moveFinger = -1, _lookFinger = -1, _dragFinger = -1;
    Vector2 _moveOrigin, _moveNow;   // pantalla (origen abajo-izquierda)
    Vector2 _lookNow;                // IMGUI (origen arriba-izquierda)
    bool _run;

    struct Btn { public string label; public KeyCode key; public bool hold, visible; public Rect rect; }
    Btn[] _btns;

    // ── La medida (en píxeles; IMGUI: origen arriba-izquierda) ──────────────────────────────────────────────────────────────────
    float Short => Mathf.Min(Screen.width, Screen.height);
    float Unit => Mathf.Max(44f, Short * 0.13f);   // un botón: ~13 % del lado corto (el pulgar)
    float Margin => Unit * 0.3f;
    float StickRadius => Unit * 1.1f;
    /// <summary>El centro del joystick de andar en reposo y el del de la cámara (fijo), en coordenadas IMGUI.</summary>
    Vector2 MoveRest => new Vector2(Margin + StickRadius, Screen.height - Margin - StickRadius);
    Vector2 LookCenter => new Vector2(Screen.width - Margin - StickRadius, Screen.height - Margin - StickRadius);

    public static TouchControls Ensure()
    {
        TouchControls t = FindAnyObjectByType<TouchControls>();
        if (t != null) return t;
        var go = new GameObject("TouchControls");
        DontDestroyOnLoad(go);
        return go.AddComponent<TouchControls>();
    }

    static readonly (string label, KeyCode key, bool hold, float deg)[] ArcBtns =
    {
        ("E", KeyCode.E, false, 90f), ("F", KeyCode.F, false, 120f), ("Saltar", KeyCode.Space, true, 150f), ("Correr", KeyCode.LeftShift, false, 180f),
    };
    const int RunIndex = 3;

    void Layout()
    {
        float u = Unit, arc = StickRadius + u * 0.85f;   // el arco: sus centros, a esta distancia del centro del joystick de la cámara
        Vector2 c = LookCenter;
        // La zona del joystick de andar (su aro en reposo, con holgura): el botón que la pise se oculta.
        float z = 2f * (StickRadius + Margin) + u * 0.5f;
        Rect moveZone = new Rect(0f, Screen.height - z, z, z);
        if (_btns == null) _btns = new Btn[ArcBtns.Length + 1];
        // De arriba (90°) hacia la izquierda (180°): E arriba del todo, Correr a la izquierda.
        for (int i = 0; i < ArcBtns.Length; i++)
        {
            float a = ArcBtns[i].deg * Mathf.Deg2Rad;
            Vector2 p = c + new Vector2(Mathf.Cos(a), -Mathf.Sin(a)) * arc;   // (IMGUI: y hacia abajo)
            var r = new Rect(p.x - u * 0.5f, p.y - u * 0.5f, u, u);
            bool vis = i == 0 || (!r.Overlaps(moveZone) && r.yMin >= 0f && r.xMin >= 0f);   // E siempre: va encima del joystick
            _btns[i] = new Btn { label = ArcBtns[i].label, key = ArcBtns[i].key, hold = ArcBtns[i].hold, visible = vis, rect = r };
        }
        // Intro (seguir una tarjeta de historia), pequeño, arriba en el centro (arriba a la izquierda, la pila de ventanas; a la derecha, el menú).
        _btns[ArcBtns.Length] = new Btn { label = "Intro", key = KeyCode.Return, visible = true, rect = new Rect(Screen.width * 0.5f - u * 0.6f, 8f, u * 1.2f, u * 0.55f) };
        if (!_btns[RunIndex].visible && _run) { _run = false; GameKeys.SetVirtual(KeyCode.LeftShift, false); }   // correr oculto: no se queda puesto
    }

    static Vector2 Gui(Vector2 screen) => new Vector2(screen.x, Screen.height - screen.y);   // pantalla (abajo-izq.) → IMGUI (arriba-izq.)

    void Update()
    {
        Layout();
        Vector2? look = null;
        bool lookHeld = false;
        Vector2 lc = LookCenter;
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            Vector2 g = Gui(t.position);
            bool ended = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
            if (t.phase == TouchPhase.Began)
            {
                int b = HitButton(g);
                if (b >= 0) { Press(b, true); continue; }
                if (_lookFinger < 0 && (g - lc).magnitude <= StickRadius * 1.25f) { _lookFinger = t.fingerId; _lookNow = g; continue; }
                if (_moveFinger < 0 && t.position.x < Screen.width * 0.4f) { _moveFinger = t.fingerId; _moveOrigin = _moveNow = t.position; continue; }
                if (_dragFinger < 0 && dragToLook && t.position.x >= Screen.width * 0.4f) { _dragFinger = t.fingerId; continue; }
            }
            if (t.fingerId == _moveFinger) { if (ended) _moveFinger = -1; else _moveNow = t.position; }
            else if (t.fingerId == _lookFinger) { if (ended) _lookFinger = -1; else _lookNow = g; }
            else if (t.fingerId == _dragFinger)
            {
                if (ended) _dragFinger = -1;
                else if (_lookFinger < 0) { look = t.deltaPosition * dragSensitivity; lookHeld = true; }   // el joystick de la cámara manda
            }
            else if (ended)
            {
                int b = HitButton(g);
                if (b >= 0 && _btns[b].hold) Press(b, false);
            }
        }
        if (Input.touchCount == 0) { _moveFinger = _lookFinger = _dragFinger = -1; GameKeys.SetVirtual(KeyCode.Space, false); }

        // EL JOYSTICK DE LA CÁMARA: cuánto se empuja (0..1, con zona muerta) → giro a velocidad constante.
        Vector2 ls = LookStick;
        if (_lookFinger >= 0) { lookHeld = true; look = ls == Vector2.zero ? Vector2.zero : new Vector2(ls.x * stickYawSpeed, ls.y * stickPitchSpeed) * Time.unscaledDeltaTime; }
        GameKeys.VirtualLook = look;
        GameKeys.SetVirtual(LookButton, lookHeld);
        GameKeys.VirtualMove = MoveStick;
    }

    /// <summary>El joystick de andar (−1..1, con zona muerta); arriba en la pantalla = adelante.</summary>
    Vector2 MoveStick => Deflect(_moveFinger >= 0 ? (_moveNow - _moveOrigin) / StickRadius : Vector2.zero);
    /// <summary>El joystick de la cámara (−1..1): derecha = girar a la derecha, arriba = mirar arriba.</summary>
    Vector2 LookStick
    {
        get
        {
            if (_lookFinger < 0) return Vector2.zero;
            Vector2 d = (_lookNow - LookCenter) / StickRadius;
            return Deflect(new Vector2(d.x, -d.y));   // IMGUI: y hacia abajo
        }
    }
    static Vector2 Deflect(Vector2 d) => d.magnitude < DeadZone ? Vector2.zero : d.sqrMagnitude > 1f ? d.normalized : d;

    int HitButton(Vector2 g)
    {
        if (_btns == null) return -1;
        for (int b = 0; b < _btns.Length; b++) if (_btns[b].visible && _btns[b].rect.Contains(g)) return b;
        return -1;
    }

    void Press(int b, bool down)
    {
        Btn btn = _btns[b];
        if (btn.key == KeyCode.LeftShift) { if (down) { _run = !_run; GameKeys.SetVirtual(KeyCode.LeftShift, _run); } return; }   // correr: se queda puesto
        if (btn.hold) GameKeys.SetVirtual(btn.key, down);
        else if (down) GameKeys.TapVirtual(btn.key);
    }

    void OnDisable()
    {
        GameKeys.VirtualMove = Vector2.zero; GameKeys.VirtualLook = null;
        foreach (KeyCode k in new[] { KeyCode.Space, KeyCode.LeftShift, LookButton }) GameKeys.SetVirtual(k, false);
    }

    GUIStyle _style;
    int _styleFor;

    void OnGUI()
    {
        if (_btns == null) return;
        GUI.depth = -1500;
        int fs = Mathf.RoundToInt(Unit * 0.26f);
        if (_style == null || _styleFor != fs) { _style = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter, fontSize = fs }; _styleFor = fs; }
        Color was = GUI.color;
        float R = StickRadius, knob = Unit * 0.42f;
        // Joystick de andar: su aro donde empezó el dedo (o en reposo) y el pomo donde está.
        Vector2 mo = _moveFinger >= 0 ? Gui(_moveOrigin) : MoveRest;
        Vector2 mv = MoveStick;
        Stick(mo, mo + new Vector2(mv.x, -mv.y) * R, R, knob, "");
        // Joystick de la cámara: fijo.
        Vector2 lc = LookCenter, lk = LookStick;
        Stick(lc, lc + new Vector2(lk.x, -lk.y) * R, R, knob, "cámara");
        foreach (Btn b in _btns)
        {
            if (!b.visible) continue;
            bool on = b.key == KeyCode.LeftShift && _run;
            GUI.color = new Color(1f, 1f, 1f, on ? 0.85f : 0.5f);
            GUI.Box(b.rect, b.label, _style);
        }
        GUI.color = was;
    }

    void Stick(Vector2 center, Vector2 knobAt, float r, float k, string label)
    {
        GUI.color = new Color(1f, 1f, 1f, 0.22f);
        GUI.Box(new Rect(center.x - r, center.y - r, r * 2f, r * 2f), label, _style);
        GUI.color = new Color(1f, 1f, 1f, 0.55f);
        GUI.Box(new Rect(knobAt.x - k, knobAt.y - k, k * 2f, k * 2f), "", _style);
    }
}
