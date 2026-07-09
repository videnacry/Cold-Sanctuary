using UnityEditor;
using UnityEngine;

/// <summary>
/// Utilidades rápidas de medición para diagnosticar problemas de escala en la escena.
/// Uso: seleccionar un GameObject en la Hierarchy y correr el menú.
/// </summary>
public static class SceneDiagnostics
{
    [MenuItem("Tools/Cold Sanctuary/Measure Selected Object Size")]
    public static void MeasureSelected()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.LogWarning("[SceneDiagnostics] No hay nada seleccionado en la Hierarchy.");
            return;
        }

        Bounds? combined = null;
        foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
        {
            if (combined == null) combined = r.bounds;
            else { Bounds b = combined.Value; b.Encapsulate(r.bounds); combined = b; }
        }

        if (!combined.HasValue)
        {
            Debug.LogWarning($"[SceneDiagnostics] {go.name}: no se encontraron Renderers.");
            return;
        }

        Debug.Log($"[SceneDiagnostics] {go.name}: height={combined.Value.size.y:F3}m, " +
                  $"width={combined.Value.size.x:F3}m, depth={combined.Value.size.z:F3}m, " +
                  $"worldScale={go.transform.lossyScale}, center={combined.Value.center}");
    }
}
