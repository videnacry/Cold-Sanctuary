using UnityEngine;

/// <summary>
/// A single line of dialogue — one thing one character says.
///
/// Designed for a linear, no-choice narrative. The story unfolds; the player listens.
/// Characters like the Magnate get dramatic screen effects and slow type speed
/// to feel genuinely imposing.
/// </summary>
[System.Serializable]
public class DialogueLine
{
    // ── Speaker ───────────────────────────────────────────────────────────────

    [Tooltip("Name shown in the dialogue panel header.")]
    public string speakerName = "???";

    [Tooltip("Portrait sprite shown beside the text. Optional — blank if null.")]
    public Sprite portrait;

    // ── Text ──────────────────────────────────────────────────────────────────

    [TextArea(2, 8)]
    public string text;

    // ── Pacing ────────────────────────────────────────────────────────────────

    [Tooltip("Characters revealed per second during typewriter animation. " +
             "Lower = more dramatic. Typical: 40. Magnate: 20–25.")]
    [Min(1f)] public float typeSpeed = 40f;

    [Tooltip("Seconds of silence after the line fully appears, before waiting " +
             "for the player to advance. Gives weight to important lines.")]
    [Min(0f)] public float pauseAfter = 0.4f;

    // ── Presentation ──────────────────────────────────────────────────────────

    [Tooltip("Optional screen effect when this line begins.")]
    public DialogueScreenEffect screenEffect = DialogueScreenEffect.None;

    [Tooltip("Override the panel side for this line. " +
             "Left = speaker on left (default). Right = used for Kushal's inner voice.")]
    public DialoguePanelSide side = DialoguePanelSide.Left;
}

/// <summary>Screen-level effect triggered at the start of a dialogue line.</summary>
public enum DialogueScreenEffect
{
    None,

    /// <summary>A brief white flash — used for the Magnate's most commanding lines.</summary>
    Flash,

    /// <summary>A low-frequency camera shake — tension, impact.</summary>
    Shake,

    /// <summary>Screen fades darker before the line begins — ominous, revelatory.</summary>
    Darken,
}

/// <summary>
/// Which side of the panel the portrait appears on.
/// Cosmetic only — does not affect logic.
/// </summary>
public enum DialoguePanelSide { Left, Right }
