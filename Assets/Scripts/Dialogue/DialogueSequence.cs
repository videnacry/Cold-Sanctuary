using UnityEngine;

/// <summary>
/// A ScriptableObject asset that represents a complete dialogue event —
/// a series of lines spoken in order, with no branching.
///
/// Create via: Right-click → Create → Cold Sanctuary → Dialogue → Sequence
///
/// Design intent:
///   Each sequence is a narrative beat. The Magnate's first evaluation of Kushal,
///   Goluis's pressure speech before a double shift, a monster's alien greeting —
///   each is one DialogueSequence asset.
///
/// Trigger sequences via DialogueManager.Instance.Play(sequence).
/// The manager respects the playOnce flag across runtime (not yet persisted to disk —
/// hook into a save system when that exists).
/// </summary>
[CreateAssetMenu(menuName = "Cold Sanctuary/Dialogue/Sequence", fileName = "DLG_NewSequence")]
public class DialogueSequence : ScriptableObject
{
    [Tooltip("Unique string key. Used by DialogueManager to track played sequences. " +
             "Example: 'magnate_first_evaluation', 'goluis_pressure_01'.")]
    public string sequenceId;

    [Tooltip("If true, this sequence will only play once per session. " +
             "Re-entering the trigger will be ignored. " +
             "(Tip: set to false for ambient lines that repeat on cooldown.)")]
    public bool playOnce = true;

    [Tooltip("Ordered list of lines. They play top to bottom.")]
    public DialogueLine[] lines;
}
