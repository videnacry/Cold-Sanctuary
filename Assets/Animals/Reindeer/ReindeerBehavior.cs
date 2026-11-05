using UnityEngine;

// Reno (Rangifer tarandus): manada de la tundra; liquen en invierno, sauce y juncia en verano. Ficha: docs/reglas-animales.md (Fichas).
public class ReindeerBehavior : Animal
{
    protected override string SpeciesArchetype => "Reindeer";

    void Start() => Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0.1f; }   // huye; solo embiste acorralado
}
