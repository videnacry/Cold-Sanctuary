using System;
using UnityEngine;

/// <summary>
/// LOS MANDOS DE LA SWITCH, COMO DATOS (2026-11-19, Berón: «en CodeShip se ha pasado a recrear los mandos de la Nintendo Switch sobre la
/// pantalla para los controles, ¿se podría implementar así para mobile aquí?»). Los 16 botones de un par de Joy-Con, como en CodeShip
/// (src/ui/action-pad.ts): a la izquierda la cruceta junto al joystick de andar, L y ZL encima, «−» y captura; a la derecha el rombo
/// A/B/X/Y junto al joystick de la cámara, R y ZR encima, «+» e inicio. Cada botón PULSA una tecla del juego por <see cref="GameKeys"/>
/// (el teclado sigue igual): el mando no tiene lógica propia. Lógica pura (sin escena): la usa <see cref="TouchControls"/> y la prueba
/// <c>JoyConTest</c>. Asignación: la de CodeShip (A Saltar, B Correr, + Misiones, − Áreas, captura Vista, inicio Menú) y el resto con
/// las teclas que ya usa el juego (docs/sistemas/mandos-tactiles.md).
/// </summary>
public static class JoyConMap
{
    public enum Pad { A, B, X, Y, R, ZR, Plus, Home, Up, Down, Left, Right, L, ZL, Minus, Capture }
    /// <summary>Toque (cuenta un fotograma), mantener (mientras el dedo está) o interruptor (se queda puesto, como Correr).</summary>
    public enum Mode { Tap, Hold, Toggle }

    public struct Button
    {
        public Pad pad;
        public string label;
        public KeyCode key;
        public Mode mode;
        /// <summary>Si la tecla depende de lo que haya en pantalla (p. ej. «+»: Tareas en un microcosmos, Misiones en el santuario).</summary>
        public Func<KeyCode> resolve;
        public KeyCode Key => resolve != null ? resolve() : key;
    }

    /// <summary>Lo que se lee en la cara de cada botón (las marcas del Joy-Con; glifos del juego WGL4, que lleva la fuente por defecto también en la web).</summary>
    public static string Face(Pad p)
    {
        switch (p)
        {
            case Pad.Plus: return "+";
            case Pad.Minus: return "−";
            case Pad.Home: return "⌂";
            case Pad.Capture: return "●";
            case Pad.Up: return "▲";
            case Pad.Down: return "▼";
            case Pad.Left: return "◄";
            case Pad.Right: return "►";
            default: return p.ToString();
        }
    }

    public static readonly Pad[] LeftSide = { Pad.Up, Pad.Down, Pad.Left, Pad.Right, Pad.L, Pad.ZL, Pad.Minus, Pad.Capture };
    public static readonly Pad[] RightSide = { Pad.X, Pad.A, Pad.B, Pad.Y, Pad.R, Pad.ZR, Pad.Plus, Pad.Home };

    /// <summary>La asignación (decisión de Berón 2026-11-19, opción «como CodeShip y el resto a mi criterio»).</summary>
    public static readonly Button[] Default =
    {
        // El rombo de la derecha: lo que se hace con el cuerpo.
        B(Pad.A, "Saltar", KeyCode.Space, Mode.Hold),          // PlayerController.jumpKey; mantenido, trepa (PlayerClimber.climbKey)
        B(Pad.B, "Correr", KeyCode.LeftShift, Mode.Toggle),    // PlayerController.sprintKey (se queda puesto, como en CodeShip)
        B(Pad.X, "Usar", KeyCode.F, Mode.Tap),                 // InteractionController.interactKey, VirtualPointer.confirmKey
        B(Pad.Y, "Hablar", KeyCode.E, Mode.Tap),               // hablar, atender, el paso de cocina (CookingStation.stepKey)
        // Los gatillos.
        B(Pad.R, "Grimorio", Grimorio.ToggleKey, Mode.Tap),   // (2026-11-20) los hechizos sin teclado: la ventana del Grimorio (F8)
        B(Pad.ZR, "Intro", KeyCode.Return, Mode.Tap),          // seguir una tarjeta, elegir en un diálogo
        B(Pad.L, "Bajar", KeyCode.LeftControl, Mode.Hold),     // PlayerController.swimDownKey, SurfaceWalker.descendKey
        B(Pad.ZL, "Cerrar", KeyCode.Escape, Mode.Tap),         // cerrar diálogos, ventanas y el reto de código
        // La cruceta: las flechas de los diálogos (alfombras, máquinas, opciones).
        B(Pad.Up, "", KeyCode.UpArrow, Mode.Tap),
        B(Pad.Down, "", KeyCode.DownArrow, Mode.Tap),
        B(Pad.Left, "", KeyCode.LeftArrow, Mode.Tap),
        B(Pad.Right, "", KeyCode.RightArrow, Mode.Tap),
        // Los generales (CodeShip, Berón 2026-11-19: «los botones de config general pueden pasar a los nuevos mandos»).
        new Button { pad = Pad.Plus, label = "Misiones", key = HudModals.MisionesKey, mode = Mode.Tap, resolve = PlusKey },
        B(Pad.Minus, "Áreas", HudModals.AreasKey, Mode.Tap),
        B(Pad.Capture, "Vista", KeyCode.V, Mode.Tap),          // PlayerController.perspectiveToggleKey (1.ª / 3.ª persona)
        new Button { pad = Pad.Home, label = "Menú", key = SceneLauncher.PauseKey, mode = Mode.Tap, resolve = HomeKey },
    };

    static Button B(Pad p, string label, KeyCode k, Mode m) => new Button { pad = p, label = label, key = k, mode = m };

    /// <summary>«+»: en un microcosmos con tareas, la ventana Tareas (F6, la de CodeShip); si no, Misiones y jornada (F4).</summary>
    static KeyCode PlusKey() => HudModals.Available(HudModals.Modal.Tareas) ? HudModals.TareasKey : HudModals.MisionesKey;
    /// <summary>Inicio: el menú árbol de CodeShip (F5) donde lo hay; si no, la pausa (F10, que lleva a Inicio).</summary>
    static KeyCode HomeKey() => MicroRootMenu.Available ? MicroRootMenu.ToggleKey : SceneLauncher.PauseKey;

    public static Button Get(Pad p)
    {
        foreach (Button b in Default) if (b.pad == p) return b;
        return default;
    }
}
