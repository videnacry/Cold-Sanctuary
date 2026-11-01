using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AGUA PARA BEBER (un bebedero, un charco de deshielo): adonde va un animal que bebe (<see cref="LifeHistory.Water.Drinks"/>)
/// cuando tiene sed (<see cref="Hydration"/>). Sin ninguna al alcance, come nieve.
/// </summary>
public class WaterSource : MonoBehaviour
{
    public static readonly List<WaterSource> All = new List<WaterSource>();
    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    public static WaterSource Nearest(Vector3 p, float radius)
    {
        WaterSource best = null; float bd = radius;
        foreach (WaterSource w in All)
        {
            if (w == null) continue;
            float d = Vector3.Distance(w.transform.position, p);
            if (d < bd) { bd = d; best = w; }
        }
        return best;
    }
}
