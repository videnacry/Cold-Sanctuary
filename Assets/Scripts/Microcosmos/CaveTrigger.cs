using UnityEngine;

/// <summary>
/// Zona "cueva" — detecta cuando un ítem de comida entra en su trigger y
/// notifica a <see cref="KitchenFireMission"/> para registrarlo como recogido.
///
/// Uso: añadir a un GameObject con Collider (Is Trigger = true) que delimite
/// la entrada de la cueva. Arrastrar el objeto de misión a <see cref="mission"/>.
///
/// El ítem se identifica por tener un <see cref="ScentEmitter"/> activo.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CaveTrigger : MonoBehaviour
{
    [Tooltip("Referencia a la misión que gestiona el conteo.")]
    public KitchenFireMission mission;

    void OnTriggerEnter(Collider other)
    {
        if (mission == null || mission.IsComplete) return;

        ScentEmitter scent = other.GetComponent<ScentEmitter>();
        if (scent == null || !scent.IsActive) return;

        mission.RegisterCollected(other.gameObject, deactivateObject: true);
    }
}
