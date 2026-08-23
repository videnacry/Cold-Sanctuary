using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfBehavior : Animal
{
    protected override string SpeciesArchetype => "Wolf";

    // Escala medida contra el mesh crudo (ver AnimalPrefabGenerator > Measure Raw Animal Sizes):
    // altura cruda 2.984m -> objetivo realista de altura de hombro adulto ~0.8m.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0.7f; }
}
