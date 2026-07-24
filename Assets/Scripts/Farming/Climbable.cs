using UnityEngine;

/// <summary>
/// Marca un objeto por el que se puede TREPAR (árbol, pared). El `PlayerClimber` del jugador permite
/// subir por él (docs/creature-stats.md §Pools derivados → "Trepar").
/// </summary>
[RequireComponent(typeof(Collider))]
public class Climbable : MonoBehaviour
{
    [Tooltip("Y del mundo hasta la que llega este trepable (su copa/tope). El jugador nunca sube por " +
             "encima de aquí, aunque su alcance por fuerza/peso diera para más.")]
    public float topY = 8f;
}
