using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class Carnivore : Animal
{
    /// <summary>
    /// Tabla de presas priorizada de la especie (preferencia, dificultad, rango).
    /// Ver docs/behavior-system.md (Componente A).
    /// </summary>
    public abstract Diet Diet { get; set; }

    // Config del Forager (etapa 3): un carnívoro caza PRESA según su Diet. (Un omnívoro marcaría además eatsGrass.)
    protected override void ConfigureForager(Forager f) { f.eatsPrey = true; f.diet = Diet; }

    // Cazar (elegir presa + perseguir + comer + llevar sobras a las crías) vive en Forager.Hunt (etapa 3). Se delega.
    public override IEnumerator Feed() => Forage.Hunt(this);
}