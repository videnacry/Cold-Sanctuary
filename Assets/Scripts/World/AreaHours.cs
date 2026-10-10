using UnityEngine;

/// <summary>
/// HORARIO de un área (docs/typed-spells-and-queues.md / world): un área (restaurante, etc.) puede **cerrar de noche**.
/// Reutiliza el reloj que YA existe (`TimeController.Hour`/`IsDay`, 6–18 = día). De noche los demás duermen y la Magnate
/// (que no duerme) ronda el bosque → Kushal no puede hacer misiones de ciertas áreas. Ponlo en el GameObject del área.
/// </summary>
public class AreaHours : MonoBehaviour
{
    [Range(0f, 24f)] public float openHour = 6f;
    [Range(0f, 24f)] public float closeHour = 20f;
    [Tooltip("Si true, ignora las horas y usa directamente IsDay del reloj (día=abierto).")]
    public bool followDayNight = true;

    public bool IsOpen
    {
        get
        {
            var tc = TimeController.timeController;
            if (tc == null) return true;
            if (followDayNight) return tc.IsDay;
            float h = tc.Hour;
            return openHour <= closeHour ? (h >= openHour && h < closeHour)
                                         : (h >= openHour || h < closeHour);   // cruza medianoche
        }
    }
}
