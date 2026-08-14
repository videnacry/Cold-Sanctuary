using UnityEngine;

/// <summary>
/// Base de una TAREA de virtualización que recibe pasos del <see cref="VirtualPointer"/> (docs
/// kitchen-simulation.md §3b). La implementan <see cref="ProductionOrder"/> (receta ordenada → producir) y
/// <see cref="StockingTask"/> (abastecer/ordenar). El puntero emite <c>(stationId, actionId)</c> a todas
/// las tareas activas; cada una decide si le concierne.
/// </summary>
public abstract class VirtualTask : MonoBehaviour
{
    [Header("Coste físico de trabajar aquí (vía A) — «de pie o haciendo fuerza»")]
    [Tooltip("Quién REALIZA el trabajo. Si se asigna, cada paso le desgasta reservas/fatiga (→ estrés vía MoodDynamics). " +
             "Vacío = sin desgaste corporal (p. ej. mientras el jugador aún no es un Anima).")]
    public Anima worker;
    [Tooltip("Coste por paso correcto. ProductionOrder puede sobrescribirlo por paso (encender fuego cuesta más que abrir una puerta).")]
    public ExertionCost exertionPerStep = new ExertionCost();

    public abstract void Submit(string stationId, string actionId);

    /// <summary>Aplica el desgaste de un paso de trabajo al <see cref="worker"/> (si hay). `over` = coste específico del paso.</summary>
    protected void SpendExertion(ExertionCost over = null)
    {
        if (worker != null) Exertion.Apply(worker, over ?? exertionPerStep);
    }
}
