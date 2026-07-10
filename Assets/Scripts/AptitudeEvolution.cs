using UnityEngine;

/// <summary>
/// Evolución lenta de una aptitud por uso/desuso, dentro de una banda alrededor de su valor base.
/// El uso la empuja hacia arriba; el desuso la deja decaer. Modela el cambio por tarea/tiempo
/// (trabajo físico → fuerza, estudio → percepción/razón, variedad → adaptabilidad, sedentarismo → ↓).
/// Ver docs/creature-stats.md §Evolución de aptitudes.
/// </summary>
public static class AptitudeEvolution
{
    /// <param name="current">valor actual de la aptitud</param>
    /// <param name="baseValue">valor base (origen) de la aptitud</param>
    /// <param name="useSignal">0 = sin uso; 1 = uso pleno en este tick</param>
    /// <param name="dt">segundos transcurridos desde el último paso</param>
    /// <returns>el nuevo valor, acotado a [baseValue*minFactor, baseValue*maxFactor]</returns>
    public static float Step(float current, float baseValue, float useSignal, float dt,
                             float gainRate = 0.01f, float decayRate = 0.003f,
                             float minFactor = 0.7f, float maxFactor = 1.6f)
    {
        useSignal = Mathf.Clamp01(useSignal);
        float delta = (useSignal * gainRate - (1f - useSignal) * decayRate) * dt * baseValue;
        return Mathf.Clamp(current + delta, baseValue * minFactor, baseValue * maxFactor);
    }
}
