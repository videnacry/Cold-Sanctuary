using UnityEngine;

/// <summary>
/// UN PUESTO DE UN ÁREA (Berón 2026-11-04, regla de capacidad; docs/manual-de-diseno.md §9 C-capacidad): marca un mueble o una
/// máquina donde <see cref="seats"/> personas hacen la misma <see cref="activity"/> a la vez (un pupitre de seis plazas, un
/// puesto de monitorización, un banco de mantenimiento). <see cref="AreaCapacity"/> los cuenta por área y por actividad.
/// </summary>
public class AreaSeat : MonoBehaviour
{
    /// <summary>Trabajo: basta con que varias personas puedan hacer la tarea a la vez. Entrenamiento y estudio: caben TODOS los
    /// miembros del santuario a la vez.</summary>
    public enum Category { Trabajo, Entrenamiento, Estudio, Ocio }

    [Tooltip("La actividad del puesto (p. ej. «sysadmin», «refrigeración», «clase», «estudio»).")]
    public string activity = "";
    public Category category = Category.Trabajo;
    [Min(1)] public int seats = 1;

    public static AreaSeat Add(GameObject g, string activity, Category category, int seats = 1)
    {
        AreaSeat s = g.AddComponent<AreaSeat>();
        s.activity = activity; s.category = category; s.seats = Mathf.Max(1, seats);
        return s;
    }
}
