using UnityEngine;

public class GrasshopperBehavior : Animal
{
    protected override string SpeciesArchetype => "Grasshopper";

    // Saltamontes/langosta: herbívoro de pasto, saltador/volador ágil. En fase GREGARIA forma enjambres
    // (representa "masas/migración"). Presa base de la sabana. Sin defensa activa: salta y huye.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.0f;   // no pelea: salta lejos
        armadura = 0.10f;          // cutícula ligera
        armament = 0.0f;           // sin arma
    }
}
