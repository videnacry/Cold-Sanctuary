using UnityEngine;

// Liebre ártica (Lepus arcticus): ramonea sauce ártico. Ficha: docs/reglas-animales.md (Fichas).
public class ArcticHareBehavior : Animal
{
    protected override string SpeciesArchetype => "ArcticHare";

    void Start() => Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0f; }   // huye
}
