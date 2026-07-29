using UnityEngine;

/// <summary>
/// Lado RECEPTOR de una petición entre personajes (docs/anima-architecture.md §11.7). Solo tiene que
/// contestar sí/no. En este MVP decide con una probabilidad tuneable; más adelante leerá el vínculo
/// (bond), los humores y las inclinaciones del ser (si lo pedido va muy en contra, dirá que no). Sin este
/// componente, un ser acepta por defecto.
/// </summary>
public class HelpResponder : MonoBehaviour
{
    [Range(0f, 1f)]
    [Tooltip("Probabilidad de aceptar una petición (MVP). Futuro: función de bond/humores/inclinaciones.")]
    public float acceptChance = 1f;

    /// <summary>¿Aceptaría este ser la petición de <paramref name="from"/>? (sí/no).</summary>
    public bool WouldAccept(AnimaController from)
    {
        bool yes = Random.value <= acceptChance;
        Debug.Log($"[Petición] «{name}» responde {(yes ? "SÍ" : "NO")} a «{(from != null ? from.name : "?")}».");
        return yes;
    }
}
