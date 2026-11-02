using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Volumen de un medio (normalmente agua). Al entrar/salir, fija el `currentMedium` de cualquier
/// Anima que lo atraviese, alimentando el multiplicador de medio (ver docs/creature-stats.md
/// §Modificadores de medio). Es el equivalente generalizado del WaterZone del jugador, pero para
/// todas las criaturas. Requiere un Collider marcado como trigger.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MediumZone : MonoBehaviour
{
    [Tooltip("Medio que representa este volumen.")]
    public Medium medium = Medium.Water;

    static readonly List<MediumZone> _all = new List<MediumZone>();
    Collider _col;

    void OnEnable() { _col = GetComponent<Collider>(); _all.Add(this); }
    void OnDisable() { _all.Remove(this); }

    /// <summary>¿Están estos pies DENTRO del agua (más de 10 cm bajo la superficie de una zona de agua)? El trigger de todo el
    /// mar llega a ras del hielo y un cuerpo que anda por el borde de la placa lo roza: eso no es estar en el agua.</summary>
    public static bool Submerged(Vector3 feet)
    {
        foreach (MediumZone z in _all)
        {
            if (z == null || z.medium != Medium.Water || z._col == null) continue;
            Bounds b = z._col.bounds;
            if (feet.x >= b.min.x && feet.x <= b.max.x && feet.z >= b.min.z && feet.z <= b.max.z && feet.y >= b.min.y && feet.y < b.max.y - 0.1f) return true;
        }
        return false;
    }

    void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        Anima le = other.GetComponent<Anima>();
        if (le != null) le.currentMedium = medium;
    }

    void OnTriggerExit(Collider other)
    {
        Anima le = other.GetComponent<Anima>();
        if (le != null) le.currentMedium = Medium.Land;   // al salir se asume tierra (limitación: zonas solapadas)
    }
}
