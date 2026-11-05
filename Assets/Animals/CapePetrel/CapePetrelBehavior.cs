using UnityEngine;

// Petrel damero (Daption capense). Vuela con SnowPetrelLife, sin NavMesh. Ficha: docs/fauna-fria-santuarios.md.
public class CapePetrelBehavior : Animal
{
    protected override string SpeciesArchetype => "CapePetrel";

    void Start() => Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0.1f; }
}
