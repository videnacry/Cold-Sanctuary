using UnityEngine;

/// <summary>
/// OBSERVACIÓN / ECUANIMIDAD (docs/apremios-guardian-observacion.md §4) — la libertad frente al apremio. NO baja el
/// apremio (el hambre sigue empujando a comer); **amortigua el SUFRIMIENTO** que el Guardián (MoodDynamics) convierte en
/// estrés. Nivel [0,1] que sale de: temple (composure) + razón + disciplina POR ENCIMA de la media, + la habilidad
/// entrenada (<see cref="ObservationSkill"/>, sube mirando) + un impulso pasivo (ojos, <see cref="ObserveSpell"/>).
///
/// Diseño clave (balance-safe): un ser PROMEDIO (aptitudes base 1) da observación 0 → NO cambia el estrés existente;
/// solo los serenos/entrenados/observando reciben el amortiguador. Así es aditivo sobre lo ya tuneado.
/// </summary>
public static class Observation
{
    /// <summary>Cuánto reduce el sufrimiento la observación plena (1). 0.8 = hasta −80% del estrés del apremio.</summary>
    public const float MaxDamping = 0.8f;

    /// <summary>Nivel de observación de un ser [0,1].</summary>
    public static float LevelOf(Anima a)
    {
        if (a == null) return 0f;
        // Base por temple/atención POR ENCIMA de la media (base 1 → 0; alto → >0). No penaliza al promedio.
        float baseLvl = Mathf.Clamp01((a.composure + a.reasoning + a.discipline) / 6f - 0.5f);
        ObservationSkill sk = a.GetComponent<ObservationSkill>();
        float skill = sk != null ? sk.Level : 0f;
        return Mathf.Clamp01(baseLvl + skill);
    }

    /// <summary>Factor por el que se multiplica la carga del Guardián: 1 = sufre todo; &lt;1 = la observación lo amortigua.
    /// Lo usa <c>MoodDynamics</c> (y el test). Un ser promedio → 1 (sin cambio).</summary>
    public static float SufferingFactor(Anima a) => Mathf.Clamp01(1f - MaxDamping * LevelOf(a));
}
