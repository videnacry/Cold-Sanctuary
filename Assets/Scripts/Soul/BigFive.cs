using UnityEngine;

/// <summary>Perfil de PERSONALIDAD Big Five / OCEAN (docs/apremios-guardian-observacion.md §8) — el modelo más usado en
/// psicología. NO son stats nuevos: se **DERIVAN** de las aptitudes que ya tiene el ser (mapeo formalizado), para dar una
/// personalidad legible (0..1 por rasgo).</summary>
public struct BigFiveProfile
{
    public float openness;          // Apertura   ≈ creatividad
    public float conscientiousness; // Responsab. ≈ disciplina
    public float extraversion;      // Extraversión ≈ sociabilidad
    public float agreeableness;     // Amabilidad ≈ afabilidad
    public float neuroticism;       // Neuroticismo ≈ inverso del temple (composure)
}

/// <summary>Deriva el Big Five de un ser desde sus aptitudes (apremios-guardian-observacion.md §8). Solo-lectura.</summary>
public static class BigFive
{
    public static BigFiveProfile Of(Anima a)
    {
        if (a == null) return default;
        return new BigFiveProfile
        {
            openness          = N(a.creativity),
            conscientiousness = N(a.discipline),
            extraversion      = N(a.sociability),
            agreeableness     = N(a.afabilidad),
            neuroticism       = Mathf.Clamp01((2f - a.composure) * 0.5f),   // más temple → menos neuroticismo
        };
    }

    // Aptitud (base 1, rango ~0..2) → rasgo 0..1.
    static float N(float aptitude) => Mathf.Clamp01(aptitude * 0.5f);
}
