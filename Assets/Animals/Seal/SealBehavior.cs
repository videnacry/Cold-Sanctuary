using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SealBehavior : Animal
{
    protected override string SpeciesArchetype => "Seal";

    // Pesca en mar abierto — no hay pasto que buscar (el flag eatsFish lo fija Forager.ConfigureForSpecies).
    // Afinidad de medio → data del arquetipo "Seal" (Archetypes), aplicada por SpeciesBody (etapa 5).

    void Start() => Init();

}
