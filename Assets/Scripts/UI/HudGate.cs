using UnityEngine;

/// <summary>
/// Decide cuándo se dibujan los HUD OnGUI de los SANDBOX de prueba. Antes todos pintaban a la vez desde el primer
/// frame (unos 10 paneles superpuestos en la escena principal) y no se podía jugar. Ahora:
///   · cada panel de sandbox sale solo si el jugador está CERCA de su objeto (<see cref="Near"/>), y
///   · <b>F1</b> alterna el "modo prueba", que los muestra todos como antes.
/// Los HUD de juego (recursos, estado del jugador, pensamientos, nombres) no pasan por aquí.
/// </summary>
public static class HudGate
{
    public const KeyCode ToggleKey = KeyCode.F1;

    /// <summary>Modo prueba: true = todos los paneles de sandbox visibles, como antes.</summary>
    public static bool ShowAll;

    static Transform _player;

    /// <summary>¿Dibujar un panel de sandbox anclado en <paramref name="anchor"/>?</summary>
    public static bool Near(Vector3 anchor, float radius = 4f)
    {
        if (ShowAll) return true;
        Transform p = Player();
        if (p == null) return false;
        Vector3 d = p.position - anchor; d.y = 0f;
        return d.sqrMagnitude <= radius * radius;
    }

    public static bool Near(Component c, float radius = 4f) => c != null && Near(c.transform.position, radius);

    static readonly string[] Arrows = { "↑", "↗", "→", "↘", "↓", "↙", "←", "↖" };

    /// <summary>Flecha (relativa a hacia dónde mira la cámara) de <paramref name="from"/> a <paramref name="to"/>,
    /// para que los HUD de misión digan hacia dónde ir y no solo a cuántos metros.</summary>
    public static string Arrow(Vector3 from, Vector3 to)
    {
        Vector3 d = to - from; d.y = 0f;
        Vector3 fwd = Camera.main != null ? Camera.main.transform.forward : Vector3.forward; fwd.y = 0f;
        float ang = Vector3.SignedAngle(fwd, d, Vector3.up);
        return Arrows[Mathf.RoundToInt(((ang + 360f) % 360f) / 45f) % 8];
    }

    static Transform Player()
    {
        if (_player != null) return _player;
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        _player = go != null ? go.transform : null;
        return _player;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        ShowAll = false;
        _player = null;
        GameObject go = new GameObject("HudGate");
        Object.DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideInHierarchy;
        go.AddComponent<HudGateInput>();
    }

    class HudGateInput : MonoBehaviour
    {
        void Update()
        {
            if (!Input.GetKeyDown(ToggleKey)) return;
            ShowAll = !ShowAll;
            Debug.Log($"[HUD] Modo prueba {(ShowAll ? "ON: todos los paneles de sandbox visibles" : "OFF: solo los de cerca")} (F1).");
        }

        void OnGUI()
        {
            GUI.depth = -1000;   // encima de los tintes de ScreenEffects (los párpados tapaban la posición)
            GUI.Label(new Rect(Screen.width - 190f, Screen.height - 24f, 185f, 22f),
                      ShowAll ? "F1: ocultar paneles de prueba" : "F1: ver paneles de prueba");
            // En modo prueba, la posición del jugador: los sandbox se describen por coordenadas en los docs.
            Transform p = Player();
            if (ShowAll && p != null)
                GUI.Label(new Rect(Screen.width - 190f, Screen.height - 44f, 185f, 22f),
                          $"pos ({p.position.x:0}, {p.position.y:0.0}, {p.position.z:0})");
        }
    }
}
