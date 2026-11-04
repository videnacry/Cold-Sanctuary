using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DORMITORIO de un habitante (arboleda residencial: cabañas en los árboles o sobre columnas). Cuando su cuerpo pide
/// dormir (NeedsDriver), el habitante va primero a SU cama y duerme allí. El dueño se reconoce por su nombre; una pareja
/// comparte casa y cama (manual M18; 2026-11-04: Amahuz y Karen).
/// (2026-11-04, Berón: «las camas de las residencias del santuario deben ser capaces de dar espacio a 4 personas») La cama tiene
/// <see cref="sleeps"/> PLAZAS de <see cref="laneWidth"/> una junto a otra: cada uno se tumba en la suya (<see cref="LieSpot"/>),
/// el dueño y su pareja en las dos del medio.
/// </summary>
public class Bedroom : MonoBehaviour
{
    public string owner;
    [Tooltip("Su pareja, si comparten casa. Vacío si vive solo.")]
    public string partner;
    public Transform bed;
    [Tooltip("Plazas de la cama, una junto a otra a lo ancho (eje X de la cama).")]
    [Min(1)] public int sleeps = 1;
    [Tooltip("Ancho de cada plaza (m).")]
    public float laneWidth = 0.9f;
    [Tooltip("Largo del colchón (m): el pie de la cama queda a la mitad, hacia −Z de la cama.")]
    public float bedLength = 2.1f;

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

    /// <summary>La plaza de <paramref name="a"/> en la cama: el dueño en la del medio y su pareja a su lado (en una cama de 1, la
    /// única). Índice de 0 a <see cref="sleeps"/> − 1.</summary>
    public int LaneOf(Anima a)
    {
        if (sleeps <= 1) return 0;
        bool isPartner = a != null && !string.IsNullOrEmpty(partner) && a.name.ToLowerInvariant().Contains(partner.ToLowerInvariant());
        return Mathf.Clamp((sleeps - 1) / 2 + (isPartner ? 1 : 0), 0, sleeps - 1);
    }

    /// <summary>Dónde se tumba <paramref name="a"/>: la raíz de la cama desplazada a su plaza (el colchón queda 0,5 m más arriba).</summary>
    public Vector3 LieSpot(Anima a)
    {
        if (bed == null) return transform.position;
        float off = (LaneOf(a) - (sleeps - 1) * 0.5f) * laneWidth;
        return bed.position + bed.right * off;
    }

    /// <summary>Al pie de la cama, de pie (para esperar junto a ella sin pisar el colchón).</summary>
    public Vector3 FootSpot => bed != null ? bed.position - bed.forward * (bedLength * 0.5f + 0.7f) : transform.position;
}
