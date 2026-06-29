using UnityEngine;

/// <summary>
/// Entidad que puede recoger comida del suelo y depositarla en otra posición.
/// Implementado por Animal y PlayerTarget. Ver docs/behavior-system.md (Fase 5).
/// </summary>
public interface ICarrier
{
    FoodItem CarriedFood { get; }

    /// <summary> Recoge la comida del suelo. Devuelve false si ya carga algo o la comida está consumida. </summary>
    bool PickUp(FoodItem food);

    /// <summary> Deposita la comida en la posición indicada, asigna droppedBy y devuelve el item. </summary>
    FoodItem Drop(Vector3 position);
}
