using UnityEngine;

/// <summary>
/// La MISIÓN de producción de un área (docs/kitchen-simulation.md §3b): una **receta = pasos ordenados**
/// sobre estaciones; seguir la secuencia produce un platillo/producto; repetir hasta la **cuota** cumple
/// la misión (= **sustento del santuario**). Recibe los pasos vía <see cref="Submit"/> (los emite el
/// <see cref="VirtualPointer"/> al confirmar sobre una <see cref="StationPart"/>). Motor genérico: sirve a
/// cocina, huerto, forja… cambiando la receta.
/// </summary>
public class ProductionOrder : VirtualTask
{
    [Header("Receta (pasos en ORDEN; mismo índice en los tres arrays)")]
    public string[] stepStation;
    public string[] stepAction;
    [Tooltip("Descripción por paso (opcional, para el feedback de consola).")]
    public string[] stepLabel;

    [Header("Producción")]
    public string productName = "plato";
    [Tooltip("Contenedor que se rellena al completar la receta (opcional).")]
    public FoodContainer output;
    [Min(1)] public int quota = 3;   // cuántos productos pide la misión (sustento)

    int _idx, _produced;
    bool _done;

    public int Produced => _produced;
    public bool Done => _done;

    /// <summary>Un paso realizado por el jugador. Avanza si es el siguiente esperado; produce al completar.</summary>
    public override void Submit(string stationId, string actionId)
    {
        if (_done || stepStation == null || stepStation.Length == 0) return;
        if (stepAction == null || stepAction.Length != stepStation.Length) return;   // receta mal formada
        if (_idx >= stepStation.Length) _idx = 0;

        if (stationId == stepStation[_idx] && actionId == stepAction[_idx])
        {
            string lbl = (stepLabel != null && _idx < stepLabel.Length && !string.IsNullOrEmpty(stepLabel[_idx]))
                ? stepLabel[_idx] : $"{stationId}/{actionId}";
            Debug.Log($"[Producción] Paso {_idx + 1}/{stepStation.Length}: {lbl}. ✓");
            _idx++;

            if (_idx >= stepStation.Length)   // receta completa → un producto
            {
                _idx = 0;
                _produced++;
                if (output != null) output.Deposit(1);
                Debug.Log($"[Producción] ¡{productName} listo! ({_produced}/{quota} para la misión).");
                if (_produced >= quota)
                {
                    _done = true;
                    Debug.Log($"[Producción] MISIÓN CUMPLIDA: {quota} {productName} (sustento del santuario).");
                }
            }
        }
        else
        {
            Debug.Log($"[Producción] Paso incorrecto ({stationId}/{actionId}); esperaba " +
                      $"{stepStation[_idx]}/{stepAction[_idx]} (paso {_idx + 1}).");
        }
    }
}
