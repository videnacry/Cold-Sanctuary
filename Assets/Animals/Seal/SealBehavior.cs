using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SealBehavior : Herbivore
{
    protected override string SpeciesArchetype => "Seal";

    // Pesca en mar abierto — no hay pasto que buscar (ver Herbivore.GrazesOnLand).
    protected override bool GrazesOnLand => false;
    // Afinidad de medio → data del arquetipo "Seal" (Archetypes), aplicada por SpeciesBody (etapa 5).

    void Start() => Init();

}
