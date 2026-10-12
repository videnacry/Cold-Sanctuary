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
            GUI.Label(new Rect(Screen.width - 190f, Screen.height - 24f, 185f, 22f),
                      ShowAll ? "F1: ocultar paneles de prueba" : "F1: ver paneles de prueba");
        }
    }
}
