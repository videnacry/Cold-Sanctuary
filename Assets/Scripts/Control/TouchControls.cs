using UnityEngine;

/// <summary>
/// CONTROLES TÁCTILES MÍNIMOS (2026-11-13, web ligera: Berón quiere abrir el juego en el móvil). El proyecto no tenía entrada táctil: todo
/// el juego lee el teclado por <see cref="GameKeys"/>, así que esto solo le da valores virtuales:
///  · un JOYSTICK abajo a la izquierda (el dedo que empieza en el tercio izquierdo) → <see cref="GameKeys.VirtualMove"/> (WASD);
///  · ARRASTRAR en la mitad derecha → mirar (<see cref="GameKeys.VirtualLook"/> y el botón de mirar mantenido);
///  · BOTONES a la derecha: E (hablar, atender), F (usar), Intro (seguir una tarjeta), Saltar (espacio) y Correr (Mayús, se queda puesto).
/// Los toques sobre los menús IMGUI del juego siguen llegando como clics (Unity simula el ratón con los toques).
/// Solo se crea en la web ligera con pantalla táctil (<see cref="WebLigera"/>); en el editor y en escritorio no existe.
/// </summary>
[DefaultExecutionOrder(-1000)]   // antes que el jugador y las misiones: lo que pulsa este fotograma lo leen este mismo fotograma
public class TouchControls : MonoBehaviour
{
    public float lookSensitivity = 0.12f;   // píxeles de arrastre → unidades de «Mouse X/Y» (el ratón da ~0,1 por píxel)
    const KeyCode LookButton = KeyCode.Mouse1;   // PlayerController.cameraLookButton por defecto

    int _moveFinger = -1, _lookFinger = -1;
    Vector2 _moveOrigin, _moveNow;
    bool _run;

    struct Btn { public string label; public KeyCode key; public bool hold; public Rect rect; }
    Btn[] _btns;

    float Unit => Mathf.Max(48f, Mathf.Min(Screen.width, Screen.height) * 0.12f);   // el tamaño de un botón: ~1 cm en un móvil
    float StickRadius => Unit * 1.3f;

    public static TouchControls Ensure()
    {
        TouchControls t = FindAnyObjectByType<TouchControls>();
        if (t != null) return t;
        var go = new GameObject("TouchControls");
        DontDestroyOnLoad(go);
        return go.AddComponent<TouchControls>();
    }

    void Layout()
    {
        float u = Unit, m = u * 0.25f, x = Screen.width - u - m, y = Screen.height - u - m;   // coordenadas de pantalla con el origen arriba (IMGUI)
        _btns = new[]
        {
            new Btn { label = "E", key = KeyCode.E, rect = new Rect(x, y - (u + m), u, u) },
            new Btn { label = "F", key = KeyCode.F, rect = new Rect(x - (u + m), y - (u + m) * 0.5f, u, u) },
            new Btn { label = "Saltar", key = KeyCode.Space, hold = true, rect = new Rect(x, y, u, u) },
            new Btn { label = "Intro", key = KeyCode.Return, rect = new Rect(x - (u + m), y + (u + m) * 0.5f - u * 0.5f, u, u * 0.5f) },
            new Btn { label = "Correr", key = KeyCode.LeftShift, rect = new Rect(x, y - 2f * (u + m), u, u * 0.6f) },
        };
    }

    static Vector2 Gui(Vector2 screen) => new Vector2(screen.x, Screen.height - screen.y);   // pantalla (abajo-izq.) → IMGUI (arriba-izq.)

    void Update()
    {
        Layout();
        GameKeys.VirtualLook = null;
        bool lookHeld = false;
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            Vector2 g = Gui(t.position);
            if (t.phase == TouchPhase.Began)
            {
                int b = HitButton(g);
                if (b >= 0) { Press(b, true); continue; }
                if (t.position.x < Screen.width * 0.4f && _moveFinger < 0) { _moveFinger = t.fingerId; _moveOrigin = _moveNow = t.position; continue; }
                if (t.position.x >= Screen.width * 0.4f && _lookFinger < 0) { _lookFinger = t.fingerId; continue; }
            }
            if (t.fingerId == _moveFinger)
            {
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) _moveFinger = -1;
                else _moveNow = t.position;
            }
            else if (t.fingerId == _lookFinger)
            {
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) _lookFinger = -1;
                else { GameKeys.VirtualLook = t.deltaPosition * lookSensitivity; lookHeld = true; }
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                int b = HitButton(g);
                if (b >= 0 && _btns[b].hold) Press(b, false);
            }
        }
        if (Input.touchCount == 0) { _moveFinger = _lookFinger = -1; GameKeys.SetVirtual(KeyCode.Space, false); }
        GameKeys.SetVirtual(LookButton, lookHeld);
        Vector2 d = _moveFinger >= 0 ? (_moveNow - _moveOrigin) / StickRadius : Vector2.zero;
        GameKeys.VirtualMove = d.sqrMagnitude > 1f ? d.normalized : (d.magnitude < 0.15f ? Vector2.zero : d);
    }

    int HitButton(Vector2 g)
    {
        for (int b = 0; b < _btns.Length; b++) if (_btns[b].rect.Contains(g)) return b;
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

    void OnGUI()
    {
        if (_btns == null) return;
        GUI.depth = -1500;
        if (_style == null) _style = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter, fontSize = Mathf.RoundToInt(Unit * 0.28f) };
        Color was = GUI.color;
        // El joystick: su aro donde empezó el dedo (o su sitio de reposo) y el pomo donde está.
        Vector2 o = _moveFinger >= 0 ? Gui(_moveOrigin) : new Vector2(Unit * 1.6f, Screen.height - Unit * 1.6f);
        Vector2 k = _moveFinger >= 0 ? o + new Vector2(GameKeys.VirtualMove.x, -GameKeys.VirtualMove.y) * StickRadius : o;
        GUI.color = new Color(1f, 1f, 1f, 0.25f);
        GUI.Box(new Rect(o.x - StickRadius, o.y - StickRadius, StickRadius * 2f, StickRadius * 2f), "", _style);
        GUI.color = new Color(1f, 1f, 1f, 0.55f);
        GUI.Box(new Rect(k.x - Unit * 0.4f, k.y - Unit * 0.4f, Unit * 0.8f, Unit * 0.8f), "", _style);
        foreach (Btn b in _btns)
        {
            bool on = b.key == KeyCode.LeftShift && _run;
            GUI.color = new Color(1f, 1f, 1f, on ? 0.85f : 0.5f);
            GUI.Box(b.rect, b.label, _style);
        }
        GUI.color = was;
    }
}
