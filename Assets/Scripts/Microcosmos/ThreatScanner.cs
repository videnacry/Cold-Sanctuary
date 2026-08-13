using UnityEngine;

/// <summary>
/// Escáner de AMENAZAS — detecta dos tipos de peligro y los combina en
/// <see cref="PerceivedDanger"/> (leído por <see cref="PackAwareness"/> para
/// decidir si ayudar a compañeros):
///
///   1. Anima depredadores cercanos: diff = PredatorPower(otro) − PredatorPower(yo)
///   2. ThreatEmitter ambientales: cualquier objeto marcado como zona peligrosa
///      (suelo de selva, veneno, etc.) contribuye con su ThreatAt(myPos).
///
/// La suma es el PELIGRO GENERAL percibido. El peligro ESPECÍFICO (el que importa
/// para decidir huir) incluye el diferencial de poder propio:
///   peligroEspecifico = perceivedDanger − PredatorPower(yo)
///
/// Añade impulsos de HUIDA al <see cref="ImpulseController"/>:
///   magnitud = clamp(perceivedDanger / fearThreshold, 0, maxFleeMagnitude)
///
/// Con decaimiento: el miedo se atenúa cuando el peligro desaparece.
/// </summary>
[RequireComponent(typeof(ImpulseController))]
public class ThreatScanner : MonoBehaviour
{
    [Header("Detección")]
    [Tooltip("Radio de detección (Anima depredadores + ThreatEmitter).")]
    [Min(0.1f)] public float scanRadius = 8f;

    [Tooltip("Peligro mínimo percibido para generar impulso de huida.")]
    [Min(0f)] public float fearThreshold = 0.3f;

    [Tooltip("Magnitud máxima del impulso de huida.")]
    [Min(0f)] public float maxFleeMagnitude = 6f;

    [Tooltip("Tasa de decaimiento del impulso de huida (por segundo) cuando no hay amenaza.")]
    [Min(0f)] public float decayRate = 2f;

    [Tooltip("Frecuencia de escaneo (s).")]
    [Min(0.1f)] public float scanRate = 0.4f;

    // ── Estado público ──────────────────────────────────────────────────────

    /// <summary>
    /// PELIGRO GENERAL percibido este tick (suma de amenazas Anima + ThreatEmitter).
    /// Leído por <see cref="PackAwareness"/> para la lógica de ayuda al compañero.
    /// </summary>
    public float PerceivedDanger { get; private set; }

    // ── Estado privado ──────────────────────────────────────────────────────

    ImpulseController _ctrl;
    Anima             _self;
    float             _next;

    const string TAG_PREFIX = "flee_";

    // ── Ciclo ──────────────────────────────────────────────────────────────

    void Awake()
    {
        _ctrl = GetComponent<ImpulseController>();
        _self = GetComponent<Anima>();
    }

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + scanRate;

        _ctrl.RemoveByTag(TAG_PREFIX + "scan");

        float myPower    = Predation.PredatorPower(_self);
        float totalDanger = 0f;
        Vector3 fleeDir  = Vector3.zero;
        float maxDiff    = 0f;

        var cols = Physics.OverlapSphere(transform.position, scanRadius);
        foreach (var col in cols)
        {
            // ── Fuente 1: Anima depredadores ───────────────────────────────
            Anima other = col.GetComponent<Anima>();
            if (other != null && other != _self)
            {
                float otherPower = Predation.PredatorPower(other);
                float diff = otherPower - myPower;
                if (diff > fearThreshold)
                {
                    totalDanger += diff;
                    Vector3 away = transform.position - other.transform.position;
                    if (diff > maxDiff) { maxDiff = diff; fleeDir = away; }
                }
                continue;
            }

            // ── Fuente 2: ThreatEmitter ambientales ────────────────────────
            ThreatEmitter emitter = col.GetComponent<ThreatEmitter>();
            if (emitter != null)
            {
                float contrib = emitter.ThreatAt(transform.position);
                if (contrib > 0f)
                {
                    totalDanger += contrib;
                    // Huir del centro del emitter (ej. huir del centro de la selva).
                    Vector3 away = transform.position - emitter.transform.position;
                    if (contrib > maxDiff) { maxDiff = contrib; fleeDir = away; }
                }
            }
        }

        PerceivedDanger = totalDanger;

        if (totalDanger > fearThreshold && fleeDir.sqrMagnitude > 0.001f)
        {
            float mag = Mathf.Clamp(totalDanger / (fearThreshold + 0.01f), 0f, maxFleeMagnitude);
            _ctrl.AddImpulse(new MovementImpulse(TAG_PREFIX + "scan", fleeDir, mag, decayRate));
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.9f, 0.2f, 0.2f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, scanRadius);
        if (PerceivedDanger > 0f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Mathf.Clamp(PerceivedDanger * 0.3f, 0.1f, 2f));
        }
    }
}
