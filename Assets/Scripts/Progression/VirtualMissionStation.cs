using UnityEngine;

/// <summary>Un paso de la misión: un puesto al que se va y se hace (F).</summary>
[RequireComponent(typeof(Collider))]
public class VirtualMissionStation : MonoBehaviour, IInteractable
{
    public VirtualMissionRunner runner;
    public string step;
    public bool done;

    public string InteractLabel => step;
    public bool CanInteract => runner != null && runner.IsNext(this);
    public void Interact() => runner.Complete(this);
}
