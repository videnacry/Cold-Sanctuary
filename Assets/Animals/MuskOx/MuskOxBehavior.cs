using UnityEngine;

// Buey almizclero (Ovibos moschatus): manada de la tundra; juncia en verano, sauce y abedul enano en invierno. Ficha: docs/reglas-animales.md (Fichas).
public class MuskOxBehavior : Animal
{
    protected override string SpeciesArchetype => "MuskOx";

    void Start() => Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0.6f; }   // defiende al grupo con los cuernos
}
