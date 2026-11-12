using UnityEngine;

// Charrán ártico (Sterna paradisaea). Vuela con SnowPetrelLife, sin NavMesh. Ficha: docs/fauna-fria-santuarios.md.
public class ArcticTernBehavior : Animal
{
    protected override string SpeciesArchetype => "ArcticTern";

    void Start() => Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0.1f; }
}
