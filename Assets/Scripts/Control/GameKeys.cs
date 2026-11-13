using System.Linq;
using UnityEngine;

/// <summary>
/// TECLADO DEL JUEGO: el mismo que <see cref="Input"/>, pero MUDO mientras el lanzador de hechizos está abierto
/// (<see cref="SpellConsole.Capturing"/>). Antes, al escribir «walkTo…» en el lanzador, cada letra también movía a
/// Kushal (W/E/S/D/F), giraba la cámara (J/L/I/K) o abría menús. Todo el juego lee las teclas por aquí; el lanzador
/// sigue leyendo <see cref="Input"/> directamente.
/// </summary>
public static class GameKeys
{
    /// <summary>Un menú propio (p. ej. el de la máquina de virtualización) se queda con el teclado.</summary>
    public static bool MenuOpen;
    /// <summary>Hay un diálogo abierto (opciones, confirmación, menú de misiones): secuestra Intro y oculta el lanzador.</summary>
    /// <summary>Qué diálogo está abierto (para los registros); vacío si ninguno.</summary>
    public static string OpenDialogs => string.Join(", ", new[] { MenuOpen ? "menú" : null, ChoiceDialog.IsOpen ? "diálogo de opciones" : null, ConfirmationPanel.IsVisible ? "panel de confirmación" : null, MissionSelectMenu.Instance != null && MissionSelectMenu.Instance.IsOpen ? "menú de misiones" : null }.Where(x => x != null));
    /// <summary>Cierra todos los diálogos (las pruebas, si uno se quedó abierto).</summary>
    public static void CloseDialogs() { MenuOpen = false; if (ChoiceDialog.IsOpen) ChoiceDialog.Close(); if (ConfirmationPanel.IsVisible) ConfirmationPanel.Instance.Hide(); if (MissionSelectMenu.Instance != null) MissionSelectMenu.Instance.CloseSilently(); }
    public static bool DialogOpen => MenuOpen || ChoiceDialog.IsOpen || ConfirmationPanel.IsVisible || (MissionSelectMenu.Instance != null && MissionSelectMenu.Instance.IsOpen);
    static bool Muted => SpellConsole.Capturing || MenuOpen;   // los diálogos de opciones NO bloquean el teclado (solo ocultan el lanzador): uno se puede alejar y se cierran
    public static bool GetKey(KeyCode k) => !Muted && (Input.GetKey(k) || VirtualHeld(k));
    public static bool GetKey(string k) => !Muted && Input.GetKey(k);
    public static bool GetKeyDown(KeyCode k) => !Muted && (Input.GetKeyDown(k) || VirtualDown(k));
    public static bool GetKeyDown(string k) => !Muted && Input.GetKeyDown(k);
    public static bool GetKeyUp(KeyCode k) => !Muted && (Input.GetKeyUp(k) || VirtualUp(k));
    public static bool GetKeyUp(string k) => !Muted && Input.GetKeyUp(k);
    public static float GetAxis(string a) => Muted ? 0f : Move(a) ?? Look(a) ?? Input.GetAxis(a);
    public static float GetAxisRaw(string a) => Muted ? 0f : Move(a) ?? Look(a) ?? Input.GetAxisRaw(a);

    // El personaje se mueve SOLO con WASD: las FLECHAS quedan para navegar los diálogos (alfombras, máquinas…) y el
    // lanzador, sin que el personaje se mueva a la vez. (2026-11-13, web ligera) Más el joystick táctil (TouchControls).
    static float? Move(string a)
    {
        if (a == "Horizontal") return Mathf.Clamp((Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f) + VirtualMove.x, -1f, 1f);
        if (a == "Vertical") return Mathf.Clamp((Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f) + VirtualMove.y, -1f, 1f);
        return null;
    }

    // ── LOS CONTROLES TÁCTILES (2026-11-13, web ligera: TouchControls) ─────────────────────────────────────────────────────────────
    // El joystick da VirtualMove; arrastrar en la mitad derecha da VirtualLook (en unidades del ratón) y mantiene el botón de mirar;
    // los botones en pantalla PULSAN teclas (E, F, espacio…) un fotograma, o las mantienen. Sin pantalla táctil, todo queda a cero.
    /// <summary>El joystick táctil (−1..1 en cada eje).</summary>
    public static Vector2 VirtualMove;
    /// <summary>El arrastre de mirar de este fotograma (unidades de «Mouse X/Y»); null si no se arrastra.</summary>
    public static Vector2? VirtualLook;
    static readonly System.Collections.Generic.HashSet<KeyCode> _vHeld = new System.Collections.Generic.HashSet<KeyCode>();
    static readonly System.Collections.Generic.Dictionary<KeyCode, int> _vDown = new System.Collections.Generic.Dictionary<KeyCode, int>(), _vUp = new System.Collections.Generic.Dictionary<KeyCode, int>();
    /// <summary>Mantiene (o suelta) una tecla virtual. Al pulsar, también cuenta como GetKeyDown este fotograma; al soltar, GetKeyUp.</summary>
    public static void SetVirtual(KeyCode k, bool held)
    {
        if (held && _vHeld.Add(k)) _vDown[k] = Time.frameCount;
        else if (!held && _vHeld.Remove(k)) _vUp[k] = Time.frameCount;
    }
    /// <summary>Un toque: la tecla cuenta como pulsada (GetKeyDown) solo este fotograma.</summary>
    public static void TapVirtual(KeyCode k) => _vDown[k] = Time.frameCount;
    static bool VirtualHeld(KeyCode k) => _vHeld.Count > 0 && _vHeld.Contains(k);
    static bool VirtualDown(KeyCode k) => _vDown.Count > 0 && _vDown.TryGetValue(k, out int f) && f == Time.frameCount;
    static bool VirtualUp(KeyCode k) => _vUp.Count > 0 && _vUp.TryGetValue(k, out int f) && f == Time.frameCount;
    static float? Look(string a)
    {
        if (!VirtualLook.HasValue) return null;
        if (a == "Mouse X") return VirtualLook.Value.x;
        if (a == "Mouse Y") return VirtualLook.Value.y;
        return null;
    }
    public static string inputString => Muted ? "" : Input.inputString;
}
