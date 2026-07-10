using UnityEngine;

/// <summary>
/// Volumen de un medio (normalmente agua). Al entrar/salir, fija el `currentMedium` de cualquier
/// LivingEntity que lo atraviese, alimentando el multiplicador de medio (ver docs/creature-stats.md
/// §Modificadores de medio). Es el equivalente generalizado del WaterZone del jugador, pero para
/// todas las criaturas. Requiere un Collider marcado como trigger.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MediumZone : MonoBehaviour
{
    [Tooltip("Medio que representa este volumen.")]
    public Medium medium = Medium.Water;

    void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        LivingEntity le = other.GetComponent<LivingEntity>();
        if (le != null) le.currentMedium = medium;
    }

    void OnTriggerExit(Collider other)
    {
        LivingEntity le = other.GetComponent<LivingEntity>();
        if (le != null) le.currentMedium = Medium.Land;   // al salir se asume tierra (limitación: zonas solapadas)
    }
}
