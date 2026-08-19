using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Registro de poblaciones vivas POR ESPECIE (docs/anima-dissolving-animal.md, etapa 5). Antes cada clase de especie
/// tenía su `static HashSet&lt;GameObject&gt; population`; ahora está centralizado por nombre de especie. Lo usan
/// `Animal.Population` (alta/baja del ser) y las `Diet` (targeting de presa por especie). Así el "quién existe de
/// cada especie" deja de vivir en la clase y se puede referir por nombre — un paso para disolver las subclases.
/// (Las aves usan su propio `BirdBehavior.population`, que NO es una especie del roster y no se toca.)
/// </summary>
public static class AnimalPopulations
{
    static readonly Dictionary<string, HashSet<GameObject>> _bySpecies = new Dictionary<string, HashSet<GameObject>>();

    /// <summary>La población viva de una especie (se crea vacía la primera vez). Nombre = arquetipo (Wolf, Deer…).</summary>
    public static HashSet<GameObject> Of(string species)
    {
        if (species == null) species = "";
        if (!_bySpecies.TryGetValue(species, out HashSet<GameObject> set))
        {
            set = new HashSet<GameObject>();
            _bySpecies[species] = set;
        }
        return set;
    }
}
