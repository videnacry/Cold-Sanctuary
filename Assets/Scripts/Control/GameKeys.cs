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
    static bool Muted => SpellConsole.Capturing || MenuOpen;
    public static bool GetKey(KeyCode k) => !Muted && Input.GetKey(k);
    public static bool GetKey(string k) => !Muted && Input.GetKey(k);
    public static bool GetKeyDown(KeyCode k) => !Muted && Input.GetKeyDown(k);
    public static bool GetKeyDown(string k) => !Muted && Input.GetKeyDown(k);
    public static bool GetKeyUp(KeyCode k) => !Muted && Input.GetKeyUp(k);
    public static bool GetKeyUp(string k) => !Muted && Input.GetKeyUp(k);
    public static float GetAxis(string a) => Muted ? 0f : Input.GetAxis(a);
    public static float GetAxisRaw(string a) => Muted ? 0f : Input.GetAxisRaw(a);
    public static string inputString => Muted ? "" : Input.inputString;
}
