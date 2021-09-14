using UnityEngine;
using System.Collections.Generic;
using System;

public class HashSetHolderAnimalPopulation : HashSetHolder
{
    public Animal animal;
    public override HashSet<GameObject> GetHashSetHolded()
    {
        return animal.Population;
    }
}