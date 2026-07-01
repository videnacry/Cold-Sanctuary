using System.Collections.Generic;
using UnityEngine;

// Añadir al mismo GameObject que Palette.
// Cuando la paleta evalúa una fórmula con SpatialArrangement, este componente
// toma los PaletteElements disponibles (en estado UI, no bloqueados) y los
// materializa en las posiciones calculadas por el patrón.
// RecallAll() devuelve todos los bloques físicos al menú.
[RequireComponent(typeof(Palette))]
public class MaterializationExecutor : MonoBehaviour
{
    Palette palette;

    void Awake()
    {
        palette = GetComponent<Palette>();
        palette.OnFormulaEvaluated += OnResult;
    }

    void OnResult(PaletteResult result)
    {
        if (!result.success || result.spatial == null) return;

        List<PaletteElement> available = palette.GetMaterializableElements();
        if (available.Count == 0) return;

        Vector3[] positions = result.spatial.pattern.Compute(
            available.Count,
            palette.user.transform,
            result.magnitude
        );

        for (int i = 0; i < available.Count && i < positions.Length; i++)
            available[i].Materialize(positions[i]);
    }

    public void RecallAll()
    {
        foreach (PaletteElement el in palette.GetAllElements())
            if (el.state == ElementState.Physical)
                el.Dematerialize();
    }
}
