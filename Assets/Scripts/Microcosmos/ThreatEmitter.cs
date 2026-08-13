using UnityEngine;

/// <summary>
/// EMISOR DE AMENAZA ambiental — cualquier objeto puede convertirse en fuente de peligro.
///
/// A diferencia de un Anima depredador (cuya amenaza viene de sus stats físicos via
/// <see cref="Predation.PredatorPower"/>), un ThreatEmitter representa peligro CONTEXTUAL:
///   • Suelo de selva peligrosa (las hormigas viejas no deberían estar ahí solas).
///   • Zona con veneno, temperatura extrema, terreno inestable, etc.
///   • Un objeto que el diseñador quiere que el ThreatScanner detecte sin que sea un Anima.
///
/// <see cref="ThreatScanner"/> recoge ThreatEmitter igual que Anima: contribuye a
/// <see cref="ThreatScanner.PerceivedDanger"/> del ser que lo detecta.
///
/// La contribución de amenaza cae con la distancia (<see cref="falloff"/>):
///   threat(d) = threatPower × lerp(1, 1-d/radius, falloff)
/// Con falloff=0: amenaza constante dentro del radio.
/// Con falloff=1: lineal, llega a 0 en el borde del radio.
/// </summary>
public class ThreatEmitter : MonoBehaviour
{
    [Tooltip("Potencia de la amenaza en el centro (equivale a PredatorPower de un Anima).")]
    [Min(0f)] public float threatPower = 3f;

    [Tooltip("Radio de influencia (m). El ThreatScanner debe tener scanRadius >= este valor para detectarlo.")]
    [Min(0.1f)] public float radius = 20f;

    [Tooltip("Caída de la amenaza con la distancia (0 = constante; 1 = lineal hasta 0 en el borde).")]
    [Range(0f, 1f)] public float falloff = 0.5f;

    // ── API ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Contribución de amenaza percibida desde <paramref name="position"/>.
    /// Devuelve 0 si está fuera del radio.
    /// </summary>
    public float ThreatAt(Vector3 position)
    {
        float dist = Vector3.Distance(transform.position, position);
        if (dist > radius) return 0f;
        float t = dist / radius;                   // 0 en centro, 1 en borde
        return threatPower * Mathf.Lerp(1f, 1f - t, falloff);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, radius);
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
