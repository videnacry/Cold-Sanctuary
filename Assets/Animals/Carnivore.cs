using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class Carnivore : Animal
{
    // Config del Forager: un carnívoro caza PRESA. Quién es presa sale de STATS por proximidad (Forager.SelectPrey +
    // Predation), ya no de una tabla Diet (retirada). Un pescador (Oso/Zorro) marca además eatsFish en su override.
    protected override void ConfigureForager(Forager f) { f.eatsPrey = true; }

    // Cazar (elegir presa + perseguir + comer + llevar sobras a las crías) vive en Forager.Hunt (etapa 3). Se delega.
    public override IEnumerator Feed() => Forage.Hunt(this);
}