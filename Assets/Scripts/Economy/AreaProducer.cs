using UnityEngine;

/// <summary>
/// Hace que un área del santuario produzca un recurso (docs/world-topology-and-planes.md §4):
/// un **aporte pasivo lineal** (por minuto de juego) más un **bonus por cada personaje asignado** al
/// área ("como mandar un peón a la mina de oro en Warcraft").
///
/// Tickea en TIEMPO DE JUEGO (vía TimeController), así que la escala Mesocosmos-lento / Macrocosmos-
/// rápido sale sola al cambiar la velocidad de tiempo (TimeController.SetTimeSpeed): más velocidad =
/// menos segundos reales por minuto de juego = economía más rápida.
///
/// Colócalo en el GameObject del área (idealmente el mismo que lleva SanctuaryArea).
/// </summary>
public class AreaProducer : MonoBehaviour
{
    [Header("Producción")]
    public SanctuaryResource resource = SanctuaryResource.Food;

    [Tooltip("Aporte pasivo, en unidades por MINUTO de juego (independiente de la velocidad de tiempo).")]
    [Min(0f)] public float perGameMinute = 10f;

    [Tooltip("Bonus por CADA personaje asignado, en unidades por minuto de juego.")]
    [Min(0f)] public float perWorkerBonus = 15f;

    [Header("Asignación")]
    [Tooltip("Personajes actualmente asignados a esta área. Ajústalo desde el juego (o desde SanctuaryDirector).")]
    [Min(0)] public int assignedWorkers = 0;

    /// <summary>Unidades por minuto de juego que produce ahora mismo (pasivo + asignados).</summary>
    public float CurrentRatePerGameMinute => perGameMinute + perWorkerBonus * Mathf.Max(0, assignedWorkers);

    public void AssignWorker()   => assignedWorkers++;
    public void UnassignWorker() => assignedWorkers = Mathf.Max(0, assignedWorkers - 1);

    void Update()
    {
        float rate = CurrentRatePerGameMinute;
        if (rate <= 0f) return;

        // Segundos reales por minuto de juego (60 a velocidad 1, menos a más velocidad).
        float secsPerGameMinute = TimeController.timeController.TimeSpeedMinuteSecs;
        if (secsPerGameMinute <= 0f) return;

        float delta = rate * (Time.deltaTime / secsPerGameMinute);
        SanctuaryResources.Instance.Add(resource, delta);
    }
}
