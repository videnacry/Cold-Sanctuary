using UnityEngine;

/// <summary>
/// Base de una TAREA de virtualización que recibe pasos del <see cref="VirtualPointer"/> (docs
/// kitchen-simulation.md §3b). La implementan <see cref="ProductionOrder"/> (receta ordenada → producir) y
/// <see cref="StockingTask"/> (abastecer/ordenar). El puntero emite <c>(stationId, actionId)</c> a todas
/// las tareas activas; cada una decide si le concierne.
/// </summary>
public abstract class VirtualTask : MonoBehaviour
{
    public abstract void Submit(string stationId, string actionId);
}
