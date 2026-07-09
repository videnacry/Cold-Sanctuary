/// <summary>
/// Marker interface for any object Kushal can interact with.
///
/// Implement on a MonoBehaviour attached to the interactable GameObject.
/// InteractionController detects it via GetComponentInParent, so the component
/// can sit on the object or any of its parents.
///
/// Philosophy — three ways to interact (playable fully with OR without mouse):
///   1. Walk near + press interactKey (default F — left hand, close to WASD)
///   2. Left-click directly on the object (mouse)
///   3. Tab-select the object, then press interactKey (keyboard only)
/// </summary>
public interface IInteractable
{
    /// <summary>Short label shown in the on-screen prompt, e.g. "Entrar a la Cocina".</summary>
    string InteractLabel { get; }

    /// <summary>Whether interaction is possible right now. Gate access here, not in Interact().</summary>
    bool CanInteract { get; }

    /// <summary>Called when the player triggers the interaction.</summary>
    void Interact();
}
