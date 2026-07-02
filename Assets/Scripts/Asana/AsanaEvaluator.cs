using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IPaletteEvaluator for the asana system.
/// Receives a list of selected BodyPosition elements from the Palette (Formula mode)
/// and checks them against the player's unlocked Asanas.
///
/// Usage:
///   Palette.Open(new PaletteConfig {
///       mode      = PaletteConfig.Mode.Formula,
///       elements  = bodyPositionElements,
///       evaluator = new AsanaEvaluator(asanaQueue, unlockedAsanas)
///   });
///
/// On match: fires OnAsanaMatched via AsanaQueue.StartActive().
/// On errors (shortcut mode): returns success=false with outcomeId="errors".
/// </summary>
public class AsanaEvaluator : IPaletteEvaluator
{
    readonly AsanaQueue _queue;
    readonly Asana[]    _unlocked;

    public AsanaEvaluator(AsanaQueue queue, Asana[] unlocked)
    {
        _queue    = queue;
        _unlocked = unlocked;
    }

    public PaletteResult Evaluate(List<PaletteElementData> selection, UnityEngine.GameObject context)
    {
        // Build a set of selected BodyParts from the payload of each element
        var selected = new HashSet<BodyPart>();
        foreach (PaletteElementData el in selection)
        {
            if (el.payload is BodyPosition bp)
                selected.Add(bp.bodyPart);
        }

        foreach (Asana asana in _unlocked)
        {
            if (!asana.isUnlocked) continue;
            if (!IsMatch(asana, selected)) continue;

            // Shortcut mode — check for errors
            if (asana.ShortcutUnlocked)
            {
                List<BodyPosition> errors = asana.RollErrors();
                if (errors.Count > 0)
                {
                    return new PaletteResult
                    {
                        success   = false,
                        outcomeId = "errors",
                        magnitude = 0f
                    };
                }
            }

            // Match found — start the asana
            _queue.StartActive(asana);

            return new PaletteResult
            {
                success   = true,
                outcomeId = asana.name,
                magnitude = asana.containerCurrent
            };
        }

        return new PaletteResult { success = false, outcomeId = "no_match" };
    }

    bool IsMatch(Asana asana, HashSet<BodyPart> selected)
    {
        foreach (BodyPosition pos in asana.requiredPositions)
            if (!selected.Contains(pos.bodyPart)) return false;
        return true;
    }
}
