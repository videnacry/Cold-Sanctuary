using UnityEngine;

// Zorro ártico (Vulpes lagopus): caza, carroña y bayas; madriguera en montículos; crían los dos padres. Ficha: docs/reglas-animales.md (Fichas).
public class ArcticFoxBehavior : Animal
{
    protected override string SpeciesArchetype => "ArcticFox";

    void Start() => Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0.2f; }   // pequeño: evita la pelea
}
