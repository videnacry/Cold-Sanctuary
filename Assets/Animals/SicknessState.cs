using UnityEngine;

/// <summary>
/// ENFERMEDAD como **hechizo-estado** (docs/environmental-navigation.md §4.2). Un ser enfermo: (1) tiene `Anima.sickness`
/// &gt; 0 → `Predation` le baja poder Y defensa (**presa fácil**); (2) deposita el canal `Sickness` en la `TraceField`
/// (los demás lo **evitan**, el depredador prioriza al **débil**); (3) **se recupera** solo con el tiempo — o antes por
/// la **cura de la enfermería** (`Heal`). Abre el área de Enfermería. Auto-añadido en `Animal.Init`.
///
/// Balance-safe: `sickness` arranca en 0 (sano → sin efecto). El **onset espontáneo es raro** (`spontaneousChancePerMinute`)
/// y modesto, y siempre se recupera → no diezma la fauna. Se enferma sobre todo por infección/trigger externo (`MakeSick`).
/// </summary>
public class SicknessState : MonoBehaviour
{
    [Tooltip("Probabilidad de enfermar espontáneamente por MINUTO de juego (baja; 0 = nunca espontáneo).")]
    [Range(0f, 1f)] public float spontaneousChancePerMinute = 0.01f;
    [Tooltip("Gravedad al enfermar espontáneamente (0..1).")]
    [Range(0f, 1f)] public float onsetSeverity = 0.5f;
    [Tooltip("Recuperación por segundo (se cura solo; la enfermería acelera con Heal).")]
    [Min(0f)] public float recoverPerSecond = 0.01f;
    [Tooltip("Intensidad del rastro Sickness mientras dura.")]
    [Min(0f)] public float scentStrength = 6f;
    [Min(0.1f)] public float depositInterval = 1f;

    Anima _anima;
    float _nextDeposit, _nextRoll;

    public bool IsSick => _anima != null && _anima.sickness > 0.01f;

    void Awake() { _anima = GetComponent<Anima>(); }

    void Update()
    {
        if (_anima == null || _anima.death) return;

        // Onset espontáneo: una tirada baja por minuto de juego (solo si está sano).
        if (Time.time >= _nextRoll)
        {
            _nextRoll = Time.time + 60f;
            if (_anima.sickness <= 0f && Random.value < spontaneousChancePerMinute) MakeSick(onsetSeverity);
        }

        if (_anima.sickness > 0f)
        {
            _anima.sickness = Mathf.Max(0f, _anima.sickness - recoverPerSecond * Time.deltaTime);   // se recupera solo
            if (Time.time >= _nextDeposit)
            {
                _nextDeposit = Time.time + depositInterval;
                TraceField.Leave(transform.position, TraceChannel.Sickness, scentStrength);
            }
        }
    }

    /// <summary>Enferma al ser (gravedad 0..1; toma la mayor). Trigger externo/infección/test.</summary>
    public void MakeSick(float severity)
    {
        if (_anima != null) _anima.sickness = Mathf.Clamp01(Mathf.Max(_anima.sickness, severity));
    }

    /// <summary>CURA (la usa la enfermería): reduce la gravedad.</summary>
    public void Heal(float amount)
    {
        if (_anima != null) _anima.sickness = Mathf.Max(0f, _anima.sickness - Mathf.Abs(amount));
    }
}
