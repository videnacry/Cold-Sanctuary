using UnityEngine;

/// <summary>
/// MARCA de quien se mueve en el SERVICIO DEL RESTAURANTE (<see cref="RestaurantService"/>): la brigada (cocineros, camarero,
/// obradores, limpieza) y los turistas. Dice qué hace y lleva la cuenta de sus TRAMOS a pie (cada destino nuevo es un tramo)
/// y de las veces que se quedó atascado, para que las pruebas midan sus caminos sin adivinar sus destinos.
/// </summary>
public class ServiceActor : MonoBehaviour
{
    public string role = "";
    public string doing = "";
    [Tooltip("Tramo actual: sube cada vez que el servicio le da un destino nuevo.")]
    public int leg;
    [Tooltip("Está andando hacia su destino (no actuando, ni sentado, ni esperando).")]
    public bool moving;
    [Tooltip("Veces que no llegó a su destino en el tiempo de un tramo.")]
    public int stuck;
}
