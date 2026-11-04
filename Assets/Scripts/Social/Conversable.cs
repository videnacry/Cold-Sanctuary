using UnityEngine;

/// <summary>
/// MARCA DE CHARLA (docs/conversaciones.md): pone a un <see cref="Anima"/> en las charlas entre personas aunque no tenga
/// <see cref="PersonLife"/> ni sea del personal de la jornada (un personaje de una historia, un visitante), o lo saca de ellas
/// (<see cref="optOut"/>: alguien que la historia necesita callado). Sin esta marca entran solas todas las personas: los que
/// llevan <see cref="PersonLife"/> (humanos e insectos-humanos) y el personal del santuario (<see cref="WorkdayDirector.staff"/>).
/// </summary>
[DisallowMultipleComponent]
public class Conversable : MonoBehaviour
{
    [Tooltip("No charla nunca (la historia lo necesita callado).")]
    public bool optOut;
    [Tooltip("Su oficio o papel, para los temas de trabajo si no lo dice su jornada (vacío: el de su jornada).")]
    public string role = "";
}
