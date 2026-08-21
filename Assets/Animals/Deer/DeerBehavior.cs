using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeerBehavior : Herbivore
{
    protected override string SpeciesArchetype => "Deer";

    // Escala medida contra el mesh crudo (ver AnimalPrefabGenerator > Measure Raw Animal Sizes):
    // altura cruda 5.489m (usa Stag.fbx) -> objetivo realista de altura de hombro adulto ~1.2m.

    // Herbívoro: huye ante amenazas, no lucha.

    void Start() => Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0f; t.canHitAndRun = false; }
}
