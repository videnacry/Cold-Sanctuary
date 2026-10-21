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
    public static bool DialogOpen => MenuOpen || ChoiceDialog.IsOpen || ConfirmationPanel.IsVisible || (MissionSelectMenu.Instance != null && MissionSelectMenu.Instance.IsOpen);
    static bool Muted => SpellConsole.Capturing || MenuOpen;   // los diálogos de opciones NO bloquean el teclado (solo ocultan el lanzador): uno se puede alejar y se cierran
    public static bool GetKey(KeyCode k) => !Muted && Input.GetKey(k);
    public static bool GetKey(string k) => !Muted && Input.GetKey(k);
    public static bool GetKeyDown(KeyCode k) => !Muted && Input.GetKeyDown(k);
    public static bool GetKeyDown(string k) => !Muted && Input.GetKeyDown(k);
    public static bool GetKeyUp(KeyCode k) => !Muted && Input.GetKeyUp(k);
    public static bool GetKeyUp(string k) => !Muted && Input.GetKeyUp(k);
    public static float GetAxis(string a) => Muted ? 0f : Move(a) ?? Input.GetAxis(a);
    public static float GetAxisRaw(string a) => Muted ? 0f : Move(a) ?? Input.GetAxisRaw(a);

    // El personaje se mueve SOLO con WASD: las FLECHAS quedan para navegar los diálogos (alfombras, máquinas…) y el
    // lanzador, sin que el personaje se mueva a la vez.
    static float? Move(string a)
    {
        if (a == "Horizontal") return (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        if (a == "Vertical") return (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
        return null;
    }
    public static string inputString => Muted ? "" : Input.inputString;
}
