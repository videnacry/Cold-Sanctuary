using UnityEngine;

/// <summary>
/// Un IMPULSO DE DIRECCIÓN — vector normalizado + magnitud que contribuye a la decisión
/// de movimiento de un ser (docs/stats-as-truth.md §impulsos).
///
/// El <see cref="ImpulseController"/> suma todos los impulsos activos y deriva el destino
/// del <c>NavMeshAgent</c>. Cualquier hechizo, drive o evento puede añadir, quitar o
/// modular impulsos — sin necesidad de un estado discreto (Flee/Wander/Hunt).
///
/// Ejemplos de tag:
///   "home"        — atracción de baja magnitud hacia el nido; siempre presente.
///   "flee_wolf"   — huida del lobo; magnitud proporcional al nivel de amenaza.
///   "pull_spell"  — arrastre por hechizo "Jalar" de Kushal.
///   "hypnosis"    — un hechizo que borra todos los impulsos salvo el suyo.
///
/// La <see cref="decayRate"/> es el factor por segundo aplicado a la magnitud (0 = permanente).
/// </summary>
[System.Serializable]
public struct MovementImpulse
{
    /// <summary>Identificador del impulso (para añadir/quitar por categoría).</summary>
    public string tag;

    /// <summary>Dirección deseada (normalizada; se normaliza al añadir).</summary>
    public Vector3 direction;

    /// <summary>Fuerza del impulso. Magnitud alta → predomina sobre impulsos débiles.</summary>
    public float magnitude;

    /// <summary>
    /// Tasa de decaimiento por segundo (0 = permanente; 1 = pierde toda la magnitud en 1 s).
    /// Permite que el miedo se atenúe gradualmente cuando el depredador se va.
    /// </summary>
    public float decayRate;

    /// <summary>Constructor directo.</summary>
    public MovementImpulse(string tag, Vector3 direction, float magnitude, float decayRate = 0f)
    {
        this.tag       = tag;
        this.direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.zero;
        this.magnitude = magnitude;
        this.decayRate = decayRate;
    }

    /// <summary>Vector ponderado: dirección × magnitud.</summary>
    public Vector3 Weighted => direction * magnitude;
}
