using UnityEngine;

// Págalo polar (Stercorarius maccormicki). Vuela con SnowPetrelLife, sin NavMesh. Ficha: docs/fauna-fria-santuarios.md.
public class PolarSkuaBehavior : Animal
{
    protected override string SpeciesArchetype => "PolarSkua";

    void Start() => Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0.1f; }
}
