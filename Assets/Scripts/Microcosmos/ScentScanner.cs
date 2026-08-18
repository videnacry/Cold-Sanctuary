using UnityEngine;

/// <summary>
/// ESCÁNER DE OLOR — detecta <see cref="ScentEmitter"/> cercanos y genera un impulso
/// de ATRACCIÓN hacia la fuente más intensa.
///
/// Colócalo en los depredadores/carroñeros del Microcosmos de la Cocina.
/// Cuando la comida (ScentEmitter activos) desaparece, los depredadores pierden
/// el rastro y la misión puede terminar.
///
/// El impulso resultante es aditivo vía <see cref="ImpulseController"/>, igual que
/// el impulso de huida de <see cref="ThreatScanner"/>.
/// </summary>
[RequireComponent(typeof(ImpulseController))]
public class ScentScanner : MonoBehaviour
{
    [Header("Detección")]
    [Tooltip("Radio de olfato (m). Debe solaparse con el radius de los ScentEmitter objetivo.")]
    [Min(0.1f)] public float scanRadius = 12f;

    [Tooltip("Umbral mínimo de olor para generar atracción (filtra fondos débiles).")]
    [Min(0f)] public float scentThreshold = 0.3f;

    [Tooltip("Magnitud máxima del impulso de atracción.")]
    [Min(0f)] public float maxApproachMagnitude = 5f;

    [Tooltip("Frecuencia de escaneo (s). Menor = más reactivo pero más costoso.")]
    [Min(0.05f)] public float scanRate = 0.5f;

    [Tooltip("Decaimiento del impulso cuando el olor desaparece (por segundo).")]
    [Min(0f)] public float decayRate = 1.5f;

    // ── Estado público ────────────────────────────────────────────────────────

    /// <summary>Intensidad de olor percibida en el último escaneo.</summary>
    public float PerceivedScent { get; private set; }

    // ── Estado privado ────────────────────────────────────────────────────────

    ImpulseController _ctrl;
    float             _next;

    const string SCENT_TAG = "scent_approach";

    // ── Ciclo ─────────────────────────────────────────────────────────────────

    void Awake()
    {
        _ctrl = GetComponent<ImpulseController>();
    }

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + scanRate;

        _ctrl.RemoveByTag(SCENT_TAG);

        float   bestScent  = 0f;
        Vector3 bestDir    = Vector3.zero;

        var cols = Physics.OverlapSphere(transform.position, scanRadius);
        foreach (var col in cols)
        {
            ScentEmitter emitter = col.GetComponent<ScentEmitter>();
            if (emitter == null) continue;

            float scent = emitter.ScentAt(transform.position);
            if (scent > bestScent)
            {
                bestScent = scent;
                // Dirección HACIA la fuente (opuesto a la huida de ThreatScanner)
                bestDir = emitter.transform.position - transform.position;
            }
        }

        PerceivedScent = bestScent;

        if (bestScent > scentThreshold && bestDir.sqrMagnitude > 0.001f)
        {
            float mag = Mathf.Clamp(bestScent / (scentThreshold + 0.01f), 0f, maxApproachMagnitude);
            _ctrl.AddImpulse(new MovementImpulse(SCENT_TAG, bestDir, mag, decayRate));
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.9f, 0.8f, 0.1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, scanRadius);
        if (PerceivedScent > 0f)
        {
            Gizmos.color = new Color(1f, 0.9f, 0f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, Mathf.Clamp(PerceivedScent * 0.2f, 0.05f, 1.5f));
        }
    }
}
