using System.Collections.Generic;
using UnityEngine;

public struct PaletteResult
{
    public bool               success;
    public string             outcomeId;   // id del asana, hechizo o preferencia activada
    public float              magnitude;   // intensidad — escala efectos y tamaño de estructuras
    public SpatialArrangement spatial;     // si el hechizo materializa bloques (puede ser null)
    // Feedback enriquecido — el tipo concreto depende del evaluador:
    //   AsanaEvaluator → List<PositionEvaluation> con calidad y gap por extremidad
    //   TeacherNPC lee este payload para elegir su línea de diálogo
    public object             payload;
}

// Instrucciones para que MaterializationExecutor materialice los PaletteElements
// de la paleta en un patrón de disposición espacial.
// Los objetos a mover son siempre los propios elementos de la paleta —
// no se referencian aquí; los obtiene MaterializationExecutor de Palette.
public class SpatialArrangement
{
    public ArrangementPattern pattern;    // cómo calcular las posiciones
    public string             patternId; // "stairs" | "barrier" | "platform" | "fallbreaker"
}

// Evalúa una selección acumulada de ingredientes y devuelve un resultado.
// Implementaciones por sistema:
//   AsanaEvaluator       — posiciones corporales → asana válido
//   EnchantmentEvaluator — elementos periódicos  → hechizo
//   EnrichmentEvaluator  — elementos naturales   → preferencia del animal
//   BlockSpellEvaluator  — cualquier fórmula     → SpatialArrangement con patrón
public interface IPaletteEvaluator
{
    PaletteResult Evaluate(List<PaletteElementData> selection, GameObject context);
}
