using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IPaletteEvaluator for the asana system.
/// Receives body-position elements from the Palette (Formula mode) and checks them
/// against unlocked Asanas. Returns per-position quality in PaletteResult.payload
/// so TeacherNPC and the UI can show targeted feedback.
///
/// Quality evaluation:
///   gap = 0             → Correct          (full benefit, slow stat growth)
///   0 < gap ≤ 0.7       → DirectionOkLowStat (reduced benefit, discomfort, faster stat growth)
///   gap > 0.7           → Impossible        (no benefit, position visualized as unreachable)
///
/// Shortcut mode (ShortcutUnlocked): skips formula selection, rolls random errors per
/// RollErrors(). Errors are returned as DirectionOkLowStat with no specific gap data.
/// </summary>
public class AsanaEvaluator : IPaletteEvaluator
{
    readonly AsanaQueue _queue;
    readonly Asana[]    _unlocked;

    // Training delta applied per successful / struggled position
    const float TrainDeltaCorrect    = 0.0005f;
    const float TrainDeltaStruggling = 0.0015f; // faster growth under productive discomfort

    public AsanaEvaluator(AsanaQueue queue, Asana[] unlocked)
    {
        _queue    = queue;
        _unlocked = unlocked;
    }

    public PaletteResult Evaluate(List<PaletteElementData> selection, GameObject context)
    {
        var selected = BuildSelectedParts(selection);
        IBody body   = context != null ? context.GetComponent<IBody>() : null;

        foreach (Asana asana in _unlocked)
        {
            if (!asana.isUnlocked) continue;
            if (!IsMatch(asana, selected)) continue;

            // ── Shortcut mode ────────────────────────────────────────────────
            if (asana.ShortcutUnlocked)
            {
                List<BodyPosition> errors = asana.RollErrors();
                if (errors.Count > 0)
                {
                    var shortcutEvals = BuildShortcutEvals(errors);
                    return new PaletteResult
                    {
                        success   = false,
                        outcomeId = "errors",
                        magnitude = 0f,
                        payload   = shortcutEvals,
                    };
                }
            }

            // ── Formula mode — evaluate per-position quality ─────────────────
            var evals = EvaluatePositions(asana, body);

            bool anyImpossible = evals.Exists(e => e.quality == PositionQuality.Impossible);
            if (anyImpossible)
            {
                return new PaletteResult
                {
                    success   = false,
                    outcomeId = "impossible",
                    magnitude = 0f,
                    payload   = evals,
                };
            }

            // Train body parts based on quality, then start the asana
            if (body != null)
                ApplyTraining(evals, body);

            _queue.StartActive(asana);

            // Magnitude reduced proportionally to how many positions are struggling
            float magnitude = ComputeMagnitude(asana, evals);

            return new PaletteResult
            {
                success   = true,
                outcomeId = asana.name,
                magnitude = magnitude,
                payload   = evals,
            };
        }

        return new PaletteResult { success = false, outcomeId = "no_match" };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static HashSet<BodyPart> BuildSelectedParts(List<PaletteElementData> selection)
    {
        var set = new HashSet<BodyPart>();
        foreach (PaletteElementData el in selection)
            if (el.payload is BodyPosition bp)
                set.Add(bp.bodyPart);
        return set;
    }

    static bool IsMatch(Asana asana, HashSet<BodyPart> selected)
    {
        foreach (BodyPosition pos in asana.requiredPositions)
            if (!selected.Contains(pos.bodyPart)) return false;
        return true;
    }

    static List<PositionEvaluation> EvaluatePositions(Asana asana, IBody body)
    {
        var evals = new List<PositionEvaluation>(asana.requiredPositions.Length);
        foreach (BodyPosition pos in asana.requiredPositions)
        {
            BodyPartStats stats = body != null
                ? body.GetBodyPartStats(pos.bodyPart)
                : new BodyPartStats { flexibility = 1f, strength = 1f, stability = 1f };
            evals.Add(pos.Evaluate(stats));
        }
        return evals;
    }

    static List<PositionEvaluation> BuildShortcutEvals(List<BodyPosition> errors)
    {
        var evals = new List<PositionEvaluation>(errors.Count);
        foreach (BodyPosition err in errors)
            evals.Add(new PositionEvaluation
            {
                part        = err.bodyPart,
                quality     = PositionQuality.DirectionOkLowStat,
                statGap     = 0f,   // no specific gap data in shortcut mode
                limitingDim = BodyStatDimension.Flexibility,
            });
        return evals;
    }

    static void ApplyTraining(List<PositionEvaluation> evals, IBody body)
    {
        foreach (PositionEvaluation e in evals)
        {
            float delta = e.quality == PositionQuality.Correct
                ? TrainDeltaCorrect
                : TrainDeltaStruggling;
            body.TrainBodyPart(e.part, e.limitingDim, delta);
        }
    }

    static float ComputeMagnitude(Asana asana, List<PositionEvaluation> evals)
    {
        float penalty = 0f;
        foreach (PositionEvaluation e in evals)
            if (e.quality == PositionQuality.DirectionOkLowStat)
                penalty += e.statGap * 0.5f;

        float raw = asana.containerCurrent - penalty;
        return Mathf.Max(raw, asana.containerCurrent * 0.25f); // floor at 25%
    }
}
