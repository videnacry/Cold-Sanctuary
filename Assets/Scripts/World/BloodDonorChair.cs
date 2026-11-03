using System.Collections.Generic;
using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase.)
/// <summary>EL SILLÓN DE DONANTE del banco de sangre (F): quien se sienta dona una unidad (si han pasado 56 días).</summary>
public class BloodDonorChair : MonoBehaviour, IInteractable
{
    public string InteractLabel => "Donar sangre (una unidad, 450 mL)";
    public bool CanInteract => BloodBank.Instance != null;
    public void Interact()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null && BloodBank.Instance != null) BloodBank.Instance.Donate(p);
    }
}
