using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base for any NPC that can evaluate the player's asana attempts and give feedback.
/// Reads List<PositionEvaluation> from PaletteResult.payload.
///
/// Subclass and override the feedback methods to express the NPC's personality:
///   - The maestra: patient, specific, encouraging even when wrong
///   - Goluis: brusque, comments all errors at once, adds pressure
///
/// Wire to Palette.OnFormulaEvaluated from the system that opens the asana palette.
/// </summary>
public abstract class TeacherNPC : MonoBehaviour
{
    [Header("Teacher")]
    public string teacherName;

    [Header("References")]
    [Tooltip("The player's IBody — obtained from the Player GameObject at runtime.")]
    public PlayerStats playerStats;

    protected virtual void Start()
    {
        if (playerStats == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerStats = player.GetComponent<PlayerStats>();
        }
    }

    // ── Public API — call this from the system that owns the Palette ──────────

    /// <summary>
    /// Evaluate the result of an asana attempt and deliver feedback.
    /// Wire to Palette.OnFormulaEvaluated.
    /// </summary>
    public void EvaluateResult(PaletteResult result)
    {
        if (!result.success)
        {
            if (result.outcomeId == "no_match")   { OnWrongFormula();  return; }
            if (result.outcomeId == "impossible")
            {
                var evals = result.payload as List<PositionEvaluation>;
                OnImpossiblePositions(evals ?? new List<PositionEvaluation>());
                return;
            }
            if (result.outcomeId == "errors")
            {
                var evals = result.payload as List<PositionEvaluation>;
                OnShortcutErrors(evals ?? new List<PositionEvaluation>());
                return;
            }
        }

        // Success — check per-position quality
        var successEvals = result.payload as List<PositionEvaluation>;
        if (successEvals != null)
            OnSuccess(successEvals);
    }

    // ── Hooks — override per personality ─────────────────────────────────────

    /// <summary>The selected body parts don't form any known asana.</summary>
    protected abstract void OnWrongFormula();

    /// <summary>Formula matched but one or more positions are physically impossible right now.</summary>
    protected abstract void OnImpossiblePositions(List<PositionEvaluation> impossible);

    /// <summary>Shortcut mode: specific positions had random errors (low mastery).</summary>
    protected abstract void OnShortcutErrors(List<PositionEvaluation> errors);

    /// <summary>
    /// Asana started successfully. evals contains the quality of each position —
    /// some may be DirectionOkLowStat, which the teacher can comment on.
    /// </summary>
    protected abstract void OnSuccess(List<PositionEvaluation> evals);

    // ── Shared helpers ────────────────────────────────────────────────────────

    protected string PartName(BodyPart part)
    {
        switch (part)
        {
            case BodyPart.Elbows:    return "codos";
            case BodyPart.Hands:     return "manos";
            case BodyPart.Knees:     return "rodillas";
            case BodyPart.Feet:      return "pies";
            case BodyPart.Hips:      return "cadera";
            case BodyPart.Back:      return "espalda";
            case BodyPart.Shoulders: return "hombros";
            case BodyPart.Head:      return "cabeza";
            default: return part.ToString().ToLower();
        }
    }

    protected string DimName(BodyStatDimension dim)
    {
        switch (dim)
        {
            case BodyStatDimension.Flexibility: return "flexibilidad";
            case BodyStatDimension.Strength:    return "fuerza";
            case BodyStatDimension.Stability:   return "estabilidad";
            default: return dim.ToString().ToLower();
        }
    }

    /// <summary>Log a dialogue line — replace with your dialogue/subtitle system.</summary>
    protected void Say(string line)
    {
        Debug.Log($"[{teacherName}] {line}");
        // TODO: DialogueManager.Instance.Say(teacherName, line);
    }
}
