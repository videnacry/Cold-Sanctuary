using UnityEngine;

/// <summary>
/// The yoga room as the EXIT of a mob world (docs mob-world-architecture §4): an interactable that
/// takes the player back to the normal world. Entry happens via the VirtualizationMachine / loto;
/// this is the way out from inside the mob world.
///
/// Minimal first version: reuses MeditationSession.EndMission() (fade to black → restore size →
/// fade in). Place it on the yoga building inside the mob world.
/// </summary>
[RequireComponent(typeof(Collider))]
public class YogaPortal : MonoBehaviour, IInteractable
{
    public string InteractLabel => "Salir por la sala de yoga";

    public bool CanInteract =>
        MeditationSession.Instance.IsInMission && !MeditationSession.Instance.IsBusy;

    public void Interact()
    {
        if (CanInteract) MeditationSession.Instance.EndMission();
    }

    void Start()
    {
        var col = GetComponent<Collider>();
        if (col != null && !col.isTrigger) col.isTrigger = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.75f, 0.65f, 0.85f, 0.35f);
        var col = GetComponent<Collider>();
        if (col != null) Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
}
