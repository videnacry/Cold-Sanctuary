using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Marks a flat grazing area. Herbivore.Feed() finds the nearest one and walks there before
/// eating — land herbivores (Bunny, Deer) only; marine ones (Whale) filter-feed in open water
/// and never look for one, see Herbivore.GrazesOnLand / WhaleBehavior.
/// </summary>
public class GrassPatch : MonoBehaviour
{
    public static readonly List<GrassPatch> All = new List<GrassPatch>();

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    public static GrassPatch Nearest(Vector3 position)
    {
        GrassPatch nearest = null;
        float bestDistSqr = float.MaxValue;
        foreach (GrassPatch patch in All)
        {
            float distSqr = (patch.transform.position - position).sqrMagnitude;
            if (distSqr < bestDistSqr)
            {
                bestDistSqr = distSqr;
                nearest = patch;
            }
        }
        return nearest;
    }
}
