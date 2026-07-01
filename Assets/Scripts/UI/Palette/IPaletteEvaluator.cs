using System.Collections.Generic;
using UnityEngine;

public struct PaletteResult
{
    public bool   success;
    public string outcomeId;  // id del asana, hechizo o preferencia activada
    public float  magnitude;  // intensidad — útil para preferencias de animal (0–1)
}

// Evalúa una selección acumulada de ingredientes y devuelve un resultado.
// Cada sistema implementa su propia versión:
//   AsanaEvaluator        — comprueba si la selección de posiciones forma un asana válido
//   EnchantmentEvaluator  — comprueba si la combinación de elementos forma un hechizo
//   EnrichmentEvaluator   — consulta las preferencias ocultas del animal
public interface IPaletteEvaluator
{
    PaletteResult Evaluate(List<PaletteElementData> selection, GameObject context);
}
