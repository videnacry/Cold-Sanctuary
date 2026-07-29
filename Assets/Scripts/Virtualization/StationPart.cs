using UnityEngine;

/// <summary>
/// Una PARTE manipulable de una estación de virtualización (docs/kitchen-simulation.md §3b): la puerta del
/// mesón, la sartén, la nevera, un huevo, el fogón… Es puro dato + un collider para que el
/// <see cref="VirtualPointer"/> la pueda apuntar. Al confirmar sobre ella, el puntero emite el paso
/// <c>(stationId, actionId)</c> a las <see cref="ProductionOrder"/> activas. Generalizable a TODAS las
/// áreas: cambian los ids y las recetas, no el motor.
/// </summary>
[RequireComponent(typeof(Collider))]
public class StationPart : MonoBehaviour
{
    [Tooltip("A qué estación pertenece (Meson, Nevera, Cocina, Parcela…).")]
    public string stationId = "Estacion";
    [Tooltip("Qué acción representa (AbrirPuerta, TomarSarten, EncenderFuego…).")]
    public string actionId = "accion";
    [Tooltip("Texto de feedback (qué hace este paso).")]
    public string label = "";

    Vector3 _baseScale;
    bool _cached;

    /// <summary>Resalta la parte cuando el puntero la apunta (feedback de feel). MVP: un pequeño bump de escala.</summary>
    public void SetHighlighted(bool on)
    {
        if (!_cached) { _baseScale = transform.localScale; _cached = true; }
        transform.localScale = on ? _baseScale * 1.25f : _baseScale;
    }
}

