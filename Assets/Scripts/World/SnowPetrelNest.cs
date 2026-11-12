using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>Un nido de petrel de las nieves: una repisa de roca de la isla (grietas y repisas sin nieve). Lo pone el constructor; lo reparten dos por nido.</summary>
public class SnowPetrelNest : MonoBehaviour
{
    public static readonly List<SnowPetrelNest> All = new List<SnowPetrelNest>();
    public readonly List<Animal> pair = new List<Animal>();
    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    public static SnowPetrelNest Claim(Animal a)
    {
        SnowPetrelNest best = null; float bd = float.MaxValue;
        foreach (SnowPetrelNest n in All)
        {
            if (n == null) continue;
            n.pair.RemoveAll(x => x == null || x.death);
            if (n.pair.Count >= 2) continue;
            float d = (n.transform.position - a.transform.position).sqrMagnitude;
            if (d < bd) { bd = d; best = n; }
        }
        if (best != null) best.pair.Add(a);
        return best;
    }
}
