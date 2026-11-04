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

    [Tooltip("(2026-11-04, pijamadas) Las PLAZAS para dormir de la casa: las del piso de abajo de la cama-litera (una por plaza, a lo ancho) y después las del de arriba. Cada una, un punto 0,5 m por debajo de la cara de arriba de su colchón y girado como su cama.")]
    public Transform[] spots;
    [Tooltip("Dónde se juntan los de una pijamada antes de dormir (puntos de la casa, delante de la cama).")]
    public Transform[] gather;
    [Tooltip("(2026-11-04, «las literas tienen resbaladera») Lo alto y el pie del TOBOGÁN de la cama-litera: por él se baja del piso de arriba (se sube trepando por el poste).")]
    public Transform slideTop, slideBottom;

    /// <summary>¿Es <paramref name="spot"/> una plaza del piso de arriba de la cama-litera?</summary>
    public bool Upper(int spot) => spot >= sleeps && spots != null && spot < spots.Length;

    /// <summary>Cuántos duermen en la casa (los dos pisos de la cama-litera: 8).</summary>
    public int Capacity => spots != null && spots.Length > 0 ? spots.Length : sleeps;

    /// <summary>El dormitorio de <paramref name="a"/>: el de la pijamada de esta noche si está invitado (<see cref="Sleepover"/>); si
    /// no, el suyo (por su nombre, como dueño o como pareja), o null si no tiene.</summary>
    public static Bedroom Of(Anima a)
    {
        if (a == null) return null;
        Bedroom guest = Sleepover.RoomOf(a);
        if (guest != null) return guest;
        return Own(a);
    }

    /// <summary>Su propio dormitorio (sin contar pijamadas).</summary>
    public static Bedroom Own(Anima a)
    {
        if (a == null) return null;
        string n = a.name.ToLowerInvariant();
        bool Is(string who) => !string.IsNullOrEmpty(who) && n.Contains(who.ToLowerInvariant());
        foreach (Bedroom b in All)
            if (b != null && (Is(b.owner) || Is(b.partner))) return b;
        return null;
    }

    /// <summary>¿Vive aquí (dueño o pareja)?</summary>
    public bool Lives(Anima a)
    {
        if (a == null) return false;
        string n = a.name.ToLowerInvariant();
        return (!string.IsNullOrEmpty(owner) && n.Contains(owner.ToLowerInvariant())) || (!string.IsNullOrEmpty(partner) && n.Contains(partner.ToLowerInvariant()));
    }

    /// <summary>La plaza de <paramref name="a"/> en la cama: el dueño en la del medio y su pareja a su lado (en una cama de 1, la
    /// única). Índice de 0 a <see cref="sleeps"/> − 1.</summary>
    public int LaneOf(Anima a)
    {
        if (sleeps <= 1) return 0;
        bool isPartner = a != null && !string.IsNullOrEmpty(partner) && a.name.ToLowerInvariant().Contains(partner.ToLowerInvariant());
        return Mathf.Clamp((sleeps - 1) / 2 + (isPartner ? 1 : 0), 0, sleeps - 1);
    }

    /// <summary>La plaza de <paramref name="a"/> esta noche: la de la pijamada si es invitado; si no, la suya de la cama.</summary>
    public int SpotIndex(Anima a) => Sleepover.SpotIn(a, this, out int i) ? i : LaneOf(a);

    /// <summary>La plaza (su punto) de <paramref name="a"/>: a donde anda para acostarse.</summary>
    public Transform SpotOf(Anima a)
    {
        int i = SpotIndex(a);
        return spots != null && i >= 0 && i < spots.Length && spots[i] != null ? spots[i] : bed;
    }

    /// <summary>Dónde se tumba <paramref name="a"/>: su plaza (la cara de arriba de su colchón queda 0,5 m más arriba).</summary>
    public Vector3 LieSpot(Anima a)
    {
        if (spots != null && spots.Length > 0) { Transform s = SpotOf(a); if (s != null) return s.position; }
        if (bed == null) return transform.position;
        float off = (LaneOf(a) - (sleeps - 1) * 0.5f) * laneWidth;
        return bed.position + bed.right * off;
    }

    /// <summary>Cómo queda girado al tumbarse en su plaza (como su cama).</summary>
    public Quaternion LieRotation(Anima a) { Transform s = SpotOf(a); return s != null ? s.rotation : bed != null ? bed.rotation : transform.rotation; }

    /// <summary>El punto <paramref name="i"/> donde se juntan los de una pijamada (de pie o sentados, en el pasillo).</summary>
    public Vector3 GatherSpot(int i) => gather != null && gather.Length > 0 && gather[((i % gather.Length) + gather.Length) % gather.Length] != null
        ? gather[((i % gather.Length) + gather.Length) % gather.Length].position : FootSpot;

    /// <summary>Al pie de la cama, de pie (para esperar junto a ella sin pisar el colchón).</summary>
    public Vector3 FootSpot => bed != null ? bed.position - bed.forward * (bedLength * 0.5f + 0.7f) : transform.position;
}
