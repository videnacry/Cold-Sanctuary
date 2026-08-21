using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class BearBehaviour : Animal
{
    protected override string SpeciesArchetype => "Bear";

    // Family creation default values

    // ThreatResponse: solitario pero pesado y agresivo → lucha si tiene ventaja de masa.

    // El umbral 50 de 'difficulty' está por calibrar — ver decisiones abiertas en behavior-system.md.

    void Start()
    {
        base.Init();
    }

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0.6f; t.canHitAndRun = false; }
}
