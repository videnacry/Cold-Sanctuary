using UnityEngine;

/// <summary>
/// Las HERRAMIENTAS del jugador para reparar (docs/forge-simulation.md §5). Gate global simple: sin
/// herramientas no se puede reparar; se **toman/devuelven** en un <see cref="ServiceHub"/> (Mecánica o
/// Construcción). Obliga al bucle: tomar → ir al área → reparar → volver y devolver.
/// </summary>
public static class Toolbox
{
    public static bool HasTools { get; private set; }

    public static void Take()
    {
        HasTools = true;
        Debug.Log("[Herramientas] Tomadas — ya puedes reparar. Ve al área del ticket.");
    }

    public static void Return()
    {
        HasTools = false;
        Debug.Log("[Herramientas] Devueltas al taller.");
    }
}
