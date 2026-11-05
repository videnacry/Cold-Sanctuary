using UnityEngine;

// Petrel de las nieves (Pagodroma nivea): el ave de los icebergs. Vuela con SnowPetrelLife, sin NavMesh; anida en roca. Ficha: docs/reglas-animales.md (Fichas).
public class SnowPetrelBehavior : Animal
{
    protected override string SpeciesArchetype => "SnowPetrel";

    void Start() => Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0f; }   // huye volando
}
