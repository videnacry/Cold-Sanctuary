using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SITIO DE GUARIDA (la guarida de nieve del oso, la caseta del malamute): donde una hembra preñada de una especie con
/// guarida de maternidad (<see cref="LifeHistory.den"/>) va a parir y criar sus primeras semanas (<see cref="MaternityDen"/>).
/// Sin ninguna cerca, la hembra cava/hace la suya donde esté (la osa polar excava su guarida en un ventisquero).
/// </summary>
public class Den : MonoBehaviour
{
    public static readonly List<Den> All = new List<Den>();
    [Tooltip("Especie que la usa (vacío = cualquiera).")]
    public string species = "";
    [System.NonSerialized] public Animal occupant;

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    /// <summary>El punto de dentro (el centro del suelo de la guarida).</summary>
    public Vector3 Inside => transform.position;

    public static Den Nearest(Animal a, float radius)
    {
        Den best = null; float bd = radius;
        foreach (Den d in All)
        {
            if (d == null || (d.occupant != null && d.occupant != a && !d.occupant.death)) continue;
            if (!string.IsNullOrEmpty(d.species) && d.species != a.LifeSpecies) continue;
            float dd = Vector3.Distance(d.transform.position, a.transform.position);
            if (dd < bd) { bd = dd; best = d; }
        }
        return best;
    }
}
