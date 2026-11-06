using System.Collections.Generic;
using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>Una mala hierba de un bancal: una planta que se puede levantar entera. La pone <see cref="WeedPatch"/>.</summary>
public class GardenWeed : MonoBehaviour
{
    public static readonly List<GardenWeed> All = new List<GardenWeed>();
    public WildPlants.Species species;
    public GardenPlot plot;
    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);
}
