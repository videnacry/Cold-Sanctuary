using UnityEngine;

/// <summary>
/// Un TICKET de avería (docs/forge-simulation.md §5): una máquina/estructura estropeada en un área, con su
/// **receta de arreglo** (<see cref="ProductionOrder"/>, normalmente con `requiresTools`). Está **abierto**
/// hasta que la receta se completa. El <see cref="ServiceHub"/> los lista para que el jugador sepa qué
/// reparar y dónde.
/// </summary>
public class RepairTicket : MonoBehaviour
{
    [Tooltip("Área donde está la avería (Cocina, Textil, Enfermería…).")]
    public string area = "Cocina";
    [Tooltip("Qué está averiado (nevera, horno, telar, tubería…).")]
    public string what = "nevera";
    [Tooltip("La receta que la arregla (al completarse, el ticket se cierra).")]
    public ProductionOrder repair;

    bool _closed;
    public bool IsOpen => !_closed;

    void Update()
    {
        if (_closed || repair == null) return;
        if (repair.Done)
        {
            _closed = true;
            Debug.Log($"[Ticket] «{what}» de «{area}» reparada — ticket cerrado. Vuelve al taller y devuelve las herramientas.");
        }
    }
}
