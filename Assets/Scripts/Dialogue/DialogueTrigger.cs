using UnityEngine;

/// <summary>
/// Attach to any collider (set to IsTrigger) to play a DialogueSequence
/// when Kushal enters.
///
/// Common uses:
///   - The Magnate's evaluation chamber entrance
///   - A new area reveal (first time the player steps into a zone)
///   - An environmental discovery (a strange machine, a locked door)
///
/// The trigger respects DialogueSequence.playOnce — if the sequence has
/// already played, entering again does nothing.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DialogueTrigger : MonoBehaviour
{
    [Tooltip("The sequence to play when the player enters this trigger.")]
    public DialogueSequence sequence;

    [Tooltip("Only trigger on objects with this tag. Default: Player.")]
    public string playerTag = "Player";

    void Start()
    {
        // Ensure collider is set as trigger
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[DialogueTrigger] Collider en {gameObject.name} " +
                             "no estaba en modo Trigger — forzado a Trigger.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (sequence == null) return;

        DialogueManager.Instance?.Play(sequence);
    }
}
