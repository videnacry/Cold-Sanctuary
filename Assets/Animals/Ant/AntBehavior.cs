using UnityEngine;

public class AntBehavior : Animal
{
    protected override string SpeciesArchetype => "Ant";

    // Hormiga obrera: pequeña, ágil, fuerte para su tamaño, exoesqueleto de quitina.
    // Mandíbulas = armamento corto; colonia = factor de manada máximo.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.6f;   // defiende colonia activamente
        armadura = 0.35f;          // quitina: notable para su tamaño
        armament = 0.25f;          // mandíbulas + ácido fórmico
    }
}
