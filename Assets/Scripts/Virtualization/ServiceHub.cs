using UnityEngine;

/// <summary>
/// Tablero de servicio de un taller (Mecánica o Construcción; docs/forge-simulation.md §5). Al empezar,
/// **lista los tickets abiertos** (avisos de avería por áreas). Gestiona **TOMAR/DEVOLVER herramientas**:
/// las <see cref="StationPart"/> del banco usan <c>stationId = toolStation</c> con
/// <c>actionId = "Tomar" | "Devolver"</c>. Es el punto de partida del bucle de reparación por dispatch:
/// tomar herramientas → ir al área del ticket → reparar → volver y devolver.
/// </summary>
public class ServiceHub : VirtualTask
{
    public string hubName = "Mecánica";
    public string toolStation = "Herramientas";

    void Start()
    {
        RepairTicket[] tickets = FindObjectsOfType<RepairTicket>();
        int open = 0;
        foreach (RepairTicket t in tickets) if (t.IsOpen) open++;
        Debug.Log($"[Servicio] «{hubName}»: {open} ticket(s) de avería. Toma herramientas y ve al área a reparar.");
        foreach (RepairTicket t in tickets) if (t.IsOpen) Debug.Log($"[Servicio]  · {t.what} en {t.area}");
    }

    public override void Submit(string stationId, string actionId)
    {
        if (stationId != toolStation) return;
        if (actionId == "Tomar") Toolbox.Take();
        else if (actionId == "Devolver") Toolbox.Return();
    }
}
