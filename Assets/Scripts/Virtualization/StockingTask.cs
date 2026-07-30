using UnityEngine;

/// <summary>
/// Tarea de ABASTECER / ORDENAR (docs/kitchen-simulation.md §2, docs/forge-simulation.md §1): coges ítems de
/// las **cajas** (dejadas en la puerta) y los colocas en su **despensa/estante correcto** → el jugador
/// **aprende dónde va cada cosa** antes de producir. Es el segundo escalón del onboarding (tras limpiar,
/// antes de cocinar/reparar) y sirve a cualquier área.
///
/// Convención de `StationPart`: las de las cajas usan <c>stationId = pickStation</c> y <c>actionId = ítem</c>
/// (lo que coges); las de los estantes usan <c>stationId = slotStation</c> y <c>actionId = ítem que aceptan</c>.
/// </summary>
public class StockingTask : VirtualTask
{
    public string pickStation = "Caja";
    public string slotStation = "Despensa";
    [Min(1)] public int total = 3;
    public string areaLabel = "despensa";

    string _held;
    int _placed;
    bool _done;

    public override void Submit(string stationId, string actionId)
    {
        if (_done) return;

        if (stationId == pickStation)
        {
            _held = actionId;
            Debug.Log($"[Abastecer] Coges «{actionId}» de la caja.");
        }
        else if (stationId == slotStation)
        {
            if (_held == null) { Debug.Log("[Abastecer] No sostienes nada — coge algo de la caja primero."); return; }
            if (_held == actionId)
            {
                _placed++;
                Debug.Log($"[Abastecer] «{_held}» guardado en su sitio ({_placed}/{total}).");
                _held = null;
                if (_placed >= total) { _done = true; Debug.Log($"[Abastecer] Todo en su sitio — {areaLabel} abastecida."); }
            }
            else
            {
                Debug.Log($"[Abastecer] Ahí no va: sostienes «{_held}», ese estante es de «{actionId}».");
            }
        }
    }
}
