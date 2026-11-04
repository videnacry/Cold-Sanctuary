using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DORMITORIO de un habitante (arboleda residencial: cabañas en los árboles o sobre columnas). Cuando su cuerpo pide
/// dormir (NeedsDriver), el habitante va primero a SU cama y duerme allí. El dueño se reconoce por su nombre; una pareja
/// comparte casa y cama (manual M18; 2026-11-04: Amahuz y Karen).
/// </summary>
public class Bedroom : MonoBehaviour
{
    public string owner;
    [Tooltip("Su pareja, si comparten casa. Vacío si vive solo.")]
    public string partner;
    public Transform bed;

    public static readonly List<Bedroom> All = new List<Bedroom>();
    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    /// <summary>El dormitorio de <paramref name="a"/> (por su nombre, como dueño o como pareja), o null si no tiene.</summary>
    public static Bedroom Of(Anima a)
    {
        if (a == null) return null;
        string n = a.name.ToLowerInvariant();
        bool Is(string who) => !string.IsNullOrEmpty(who) && n.Contains(who.ToLowerInvariant());
        foreach (Bedroom b in All)
            if (b != null && (Is(b.owner) || Is(b.partner))) return b;
        return null;
    }
}
