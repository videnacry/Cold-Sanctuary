using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class Herbivore : Animal
{
    // Land herbivores (Bunny, Deer) walk to the nearest GrassPatch before eating. Marine ones
    // (Whale, Seal) walk to the nearest FishSchool instead — see WhaleBehavior/SealBehavior.
    protected virtual bool GrazesOnLand => true;

    // Config del Forager (etapa 3): pasto en tierra, banco de peces en el mar.
    protected override void ConfigureForager(Forager f) { if (GrazesOnLand) f.eatsGrass = true; else f.eatsFish = true; }

}