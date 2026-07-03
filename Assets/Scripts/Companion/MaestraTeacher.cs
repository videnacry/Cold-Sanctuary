using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Personality: calm, precise, names every part that needs work.
/// On success with struggling positions: encourages the effort, names what to develop.
/// On wrong formula: gentle redirection, no blame.
/// </summary>
public class MaestraTeacher : TeacherNPC
{
    protected override void OnWrongFormula()
    {
        Say("Esa combinación no la reconozco. Prueba concentrarte en las posiciones que ya conoces.");
    }

    protected override void OnImpossiblePositions(List<PositionEvaluation> impossible)
    {
        if (impossible.Count == 1)
        {
            var e = impossible[0];
            Say($"Tu {PartName(e.part)} aún no llega. No hay apuro — primero trabaja la {DimName(e.limitingDim)}.");
        }
        else
        {
            var parts = string.Join(" y ", impossible.ConvertAll(e => PartName(e.part)));
            Say($"Hay demasiada resistencia en {parts} por ahora. Prueba una postura más sencilla.");
        }
    }

    protected override void OnShortcutErrors(List<PositionEvaluation> errors)
    {
        var parts = string.Join(", ", errors.ConvertAll(e => PartName(e.part)));
        Say($"Bien, casi. Tu {parts} perdió precisión — falta un poco más de práctica sostenida.");
    }

    protected override void OnSuccess(List<PositionEvaluation> evals)
    {
        var struggling = evals.FindAll(e => e.quality == PositionQuality.DirectionOkLowStat);

        if (struggling.Count == 0)
        {
            Say("Perfecto. Mantén la respiración.");
            return;
        }

        if (struggling.Count == 1)
        {
            var e = struggling[0];
            Say($"Lo estás logrando. La {PartName(e.part)} te pide un poco más de {DimName(e.limitingDim)} — con el tiempo vendrá.");
        }
        else
        {
            var parts = string.Join(" y ", struggling.ConvertAll(e => PartName(e.part)));
            Say($"Buen intento. {parts} están forzando — eso es parte del proceso.");
        }
    }
}
