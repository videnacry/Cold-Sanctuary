using UnityEngine;

// Pingüino emperador (Aptenodytes forsteri): cría en el hielo fijo, sin nido; el macho incuba el huevo sobre las patas ayunando (LifeHistory, Egg). Ficha: docs/reglas-animales.md (Fichas).
public class EmperorPenguinBehavior : Animal
{
    protected override string SpeciesArchetype => "EmperorPenguin";

    void Start() => Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0f; }   // no lucha: huye
}
