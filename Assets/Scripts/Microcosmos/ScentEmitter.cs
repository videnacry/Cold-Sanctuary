using UnityEngine;

/// <summary>
/// EMISOR DE OLOR — cualquier objeto (comida, cadáver, nido) puede convertirse en
/// fuente de atracción olfativa para depredadores cercanos.
///
/// Simula la quimiorecepción de insectos: las hormigas perciben el mundo casi
/// exclusivamente por olfato a través de sus antenas. Un ítem de comida sin recoger
/// emite compuestos volátiles que los depredadores y carroñeros pueden detectar.
///
/// La mecánica de misión de la Cocina (era pre-fuego):
///   1. Los restos de comida esparcidos emiten olor (<see cref="ScentEmitter"/>).
///   2. Los depredadores con <see cref="ScentScanner"/> se acercan al olor.
///   3. Kushal (vía <see cref="PullSpell"/>) y las hormigas llevan la comida a la cueva.
///   4. Cuando todos los <see cref="ScentEmitter"/> se desactivan (comida recogida),
///      los depredadores pierden el rastro y la misión termina.
///
/// Simétrico a <see cref="ThreatEmitter"/>; <see cref="ScentScanner"/> lo consume
/// igual que <see cref="ThreatScanner"/> consume <see cref="ThreatEmitter"/>.
/// </summary>
public class ScentEmitter : MonoBehaviour
{
    [Tooltip("Intensidad del olor en la fuente. Los depredadores con ScentScanner la leen para calcular su impulso de atracción.")]
    [Min(0f)] public float scentStrength = 4f;

    [Tooltip("Radio de difusión del olor (m). El ScentScanner debe tener scanRadius >= este valor.")]
    [Min(0.1f)] public float radius = 15f;

    [Tooltip("Caída de la intensidad con la distancia (0 = constante; 1 = lineal hasta 0 en el borde).")]
    [Range(0f, 1f)] public float falloff = 0.6f;

    [Tooltip("Si true, el olor se reduce linealmente hasta 0 en <decayTime> segundos (simula dispersión o recogida).")]
    public bool decayOverTime = false;

    [Tooltip("Tiempo (s) hasta que el olor llega a 0 cuando decayOverTime = true.")]
    [Min(1f)] public float decayTime = 60f;

    // ── Estado ────────────────────────────────────────────────────────────────

    float _elapsed;
    bool  _active = true;

    /// <summary>true mientras el emisor sigue activo (olor > 0).</summary>
    public bool IsActive => _active;

    // ── API ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Intensidad de olor percibida desde <paramref name="position"/>.
    /// Devuelve 0 si está fuera del radio o el emisor está inactivo.
    /// </summary>
    public float ScentAt(Vector3 position)
    {
        if (!_active) return 0f;

        float dist = Vector3.Distance(transform.position, position);
        if (dist > radius) return 0f;

        float t        = dist / radius;                          // 0 en centro, 1 en borde
        float strength = scentStrength * CurrentIntensity();
        return strength * Mathf.Lerp(1f, 1f - t, falloff);
    }

    /// <summary>
    /// Desactiva manualmente el emisor (cuando la comida es recogida).
    /// </summary>
    public void Deactivate()
    {
        _active = false;
    }

    // ── Ciclo ─────────────────────────────────────────────────────────────────

    void Update()
    {
        if (!decayOverTime || !_active) return;

        _elapsed += Time.deltaTime;
        if (_elapsed >= decayTime)
            _active = false;
    }

    float CurrentIntensity()
    {
        if (!decayOverTime) return 1f;
        return Mathf.Clamp01(1f - _elapsed / decayTime);
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        if (!_active) return;
        float intensity = Application.isPlaying ? CurrentIntensity() : 1f;
        Gizmos.color = new Color(0.9f, 0.8f, 0.1f, 0.15f * intensity);
        Gizmos.DrawSphere(transform.position, radius);
        Gizmos.color = new Color(0.9f, 0.8f, 0.1f, 0.6f * intensity);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
