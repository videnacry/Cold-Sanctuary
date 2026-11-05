using UnityEngine;

// Albatros viajero (Diomedea exulans). Vuela con SnowPetrelLife, sin NavMesh. Ficha: docs/fauna-fria-santuarios.md.
public class WanderingAlbatrossBehavior : Animal
{
    protected override string SpeciesArchetype => "WanderingAlbatross";

    void Start() => Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0.1f; }
}
