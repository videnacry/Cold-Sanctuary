using UnityEngine;

/// <summary>
/// RESPUESTA AGUDA AL ESTRÉS ("el cuerpo se pasa de sus límites") — ciencia real anclada en la química que ya existe:
/// ante una necesidad/amenaza extrema, el eje simpático-adrenal libera **adrenalina + cortisol** (`Humores`), que
/// **movilizan glucosa** y suben la energía → un **impulso físico transitorio** (más velocidad/fuerza/agilidad, `physicalBoost`)
/// **financiado por la glucosa** (se paga: drena reservas). El mismo estado **estrecha la cognición** (visión de túnel,
/// curva de Yerkes-Dodson): sube <see cref="Anima.mindStatusCalculationFog"/>, que enturbia los cálculos de acción
/// (EvaluateThreat/SenseThreats/Volition/…). Y, si el apremio es muy alto, **el ser toma el mando** subiendo la
/// relevancia de su <see cref="AiBrain"/> por encima de la posesión del jugador (control-por-necesidad).
///
/// Modelo (docs/microcosmos-dungbeetle-level.md §3): FÍSICO monotónico con la activación (Yerkes-Dodson: tareas simples/
/// motoras mejoran con arousal), pero **gateado por combustible**; COGNITIVO degradado más allá del óptimo. Opt-in:
/// añade este componente al ser (necesita `Anima`; usa `Mind.humores` si hay `Mind`, si no cae al `stress` directo).
/// </summary>
[RequireComponent(typeof(Anima))]
public class AcuteStressResponse : MonoBehaviour
{
    [Header("Umbrales (0..1 de activación)")]
    [Tooltip("Activación óptima: por debajo, foco fino; por encima, empieza la niebla cognitiva (Yerkes-Dodson).")]
    [Range(0f, 1f)] public float arousalOptimum = 0.5f;
    [Tooltip("Activación a partir de la cual el CUERPO toma el mando (su relevancia supera la del jugador). ~muy alto.")]
    [Range(0f, 1f)] public float takeoverArousal = 0.85f;

    [Header("Ganancias")]
    [Tooltip("Impulso físico máximo (fracción; 0.5 = hasta +50% efectivo) cuando hay adrenalina Y glucosa.")]
    [Min(0f)] public float maxPhysicalBoost = 0.5f;
    [Tooltip("Relevancia extra de la IA al máximo de activación (se suma a su selfRelevance base para vencer la posesión).")]
    [Min(0f)] public float maxTakeoverRelevance = 6f;
    [Tooltip("Glucosa consumida por segundo a pleno impulso (el subidón se paga).")]
    [Min(0f)] public float glucoseDrainPerSec = 0.05f;

    Anima _anima;
    Mind _mind;
    AiBrain _ai;
    float _baseRelevance;

    void Awake()
    {
        _anima = GetComponent<Anima>();
        _mind = GetComponent<Mind>();
        _ai = GetComponent<AiBrain>();
        if (_ai != null) _baseRelevance = _ai.selfRelevance;
    }

    void Update()
    {
        if (_anima == null) return;
        float dt = Time.deltaTime;

        // ── ACTIVACIÓN (arousal) = el apremio más fuerte: estrés + hambre + amenaza percibida ──────────────
        float hunger = (_anima is Animal an) ? Mathf.Clamp01(an.hungry) : 0f;
        float arousal = Mathf.Clamp01(Mathf.Max(_anima.stress, hunger, _anima.alertness));

        // ── QUÍMICA: estrés agudo → adrenalina + cortisol; el cortisol MOVILIZA glucosa (gluconeogénesis) ──
        float glucose = 0.6f;   // fallback si no hay Mind
        if (_mind != null)
        {
            Humores h = _mind.humores;
            // empuja los humores del estrés hacia la activación (suave, para no pisar a MoodDynamics/Guardián).
            h.adrenalina = Mathf.MoveTowards(h.adrenalina, arousal, 0.6f * dt);
            if (arousal > arousalOptimum) h.cortisol = Mathf.MoveTowards(h.cortisol, arousal, 0.4f * dt);
            glucose = h.glucosa;
        }

        // ── IMPULSO FÍSICO: monotónico con la activación PERO gateado por combustible (adrenalina×glucosa) ──
        float fueled = arousal * Mathf.Clamp01(glucose / 0.3f);         // sin glucosa no hay subidón real
        _anima.physicalBoost = Mathf.MoveTowards(_anima.physicalBoost, maxPhysicalBoost * fueled, 1.5f * dt);
        if (_mind != null && _anima.physicalBoost > 0.01f)
            _mind.humores.Consume(Humor.Glucosa, glucoseDrainPerSec * (_anima.physicalBoost / Mathf.Max(0.01f, maxPhysicalBoost)) * dt);

        // ── NIEBLA COGNITIVA: 0 hasta el óptimo, sube hasta 1 al máximo (visión de túnel) ──────────────────
        _anima.mindStatusCalculationFog = arousal <= arousalOptimum ? 0f
            : Mathf.SmoothStep(0f, 1f, (arousal - arousalOptimum) / Mathf.Max(0.01f, 1f - arousalOptimum));

        // ── CONTROL POR NECESIDAD: la IA se reclama más cuanto más apremio; al pasar el umbral vence a la posesión ──
        if (_ai != null)
        {
            float extra = arousal < takeoverArousal ? Mathf.Lerp(0f, 1f, arousal / Mathf.Max(0.01f, takeoverArousal))
                                                    : Mathf.Lerp(1f, maxTakeoverRelevance, (arousal - takeoverArousal) / Mathf.Max(0.01f, 1f - takeoverArousal));
            _ai.selfRelevance = _baseRelevance + extra;
        }
    }
}
