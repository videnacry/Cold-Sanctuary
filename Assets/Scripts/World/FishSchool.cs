using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Marks a patch of open water where marine herbivores (Whale, Seal) filter-feed/fish.
/// Equivalent to GrassPatch but for the GrazesOnLand == false side of Herbivore.Feed().
/// </summary>
public class FishSchool : MonoBehaviour
{
    public static readonly List<FishSchool> All = new List<FishSchool>();

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    public static FishSchool Nearest(Vector3 position)
    {
        FishSchool nearest = null;
        float bestDistSqr = float.MaxValue;
        foreach (FishSchool school in All)
        {
            float distSqr = (school.transform.position - position).sqrMagnitude;
            if (distSqr < bestDistSqr)
            {
                bestDistSqr = distSqr;
                nearest = school;
            }
        }
        return nearest;
    }
}
