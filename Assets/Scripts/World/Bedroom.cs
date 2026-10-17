using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DORMITORIO de un habitante (arboleda residencial: cabañas en los árboles o sobre columnas). Cuando su cuerpo pide
/// dormir (NeedsDriver), el habitante va primero a SU cama y duerme allí. El dueño se reconoce por su nombre.
/// </summary>
public class Bedroom : MonoBehaviour
{
    public string owner;
    public Transform bed;

    public static readonly List<Bedroom> All = new List<Bedroom>();
    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    /// <summary>El dormitorio de <paramref name="a"/> (por su nombre), o null si no tiene.</summary>
    public static Bedroom Of(Anima a)
    {
        if (a == null) return null;
        string n = a.name.ToLowerInvariant();
        foreach (Bedroom b in All)
            if (b != null && !string.IsNullOrEmpty(b.owner) && n.Contains(b.owner.ToLowerInvariant())) return b;
        return null;
    }
}
