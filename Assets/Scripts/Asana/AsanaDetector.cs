using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Attach to the Player. Listens for BodyPosition button presses,
/// compares against unlocked asanas, and fires OnAsanaMatched when complete.
/// </summary>
public class AsanaDetector : MonoBehaviour
{
    [Header("Unlocked Asanas (assign in Inspector)")]
    public Asana[] unlockedAsanas;

    [Header("Events")]
    public UnityEvent<Asana> OnAsanaMatched;   // fires when a full match is found
    public UnityEvent<List<BodyPosition>> OnErrorsDetected; // fires during shortcut with errors

    // Currently selected positions this attempt
    private HashSet<BodyPart> _selected = new HashSet<BodyPart>();

    /// <summary>Called by BodyPositionButton when the player taps a position.</summary>
    public void SelectPosition(BodyPart part)
    {
        _selected.Add(part);
        CheckForMatch();
    }

    public void ClearSelection() => _selected.Clear();

    private void CheckForMatch()
    {
        foreach (Asana asana in unlockedAsanas)
        {
            if (!asana.isUnlocked) continue;
            if (IsMatch(asana))
            {
                if (asana.ShortcutUnlocked)
                {
                    List<BodyPosition> errors = asana.RollErrors();
                    if (errors.Count > 0)
                    {
                        OnErrorsDetected?.Invoke(errors);
                        return;
                    }
                }
                OnAsanaMatched?.Invoke(asana);
                ClearSelection();
                return;
            }
        }
    }

    private bool IsMatch(Asana asana)
    {
        foreach (BodyPosition pos in asana.requiredPositions)
            if (!_selected.Contains(pos.bodyPart)) return false;
        return true;
    }
}
