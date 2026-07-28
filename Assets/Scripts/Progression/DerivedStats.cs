using UnityEngine;

/// <summary>Identifica una de las 12 aptitudes (para recompensas/ganancias por misión).</summary>
public enum AptitudeKind
{
    Agility, Perception, Strength, BodyMass, Adaptability, Composure,
    Endurance, Reasoning, Memory, Creativity, Sociability, Discipline
}

/// <summary>
/// Aptitudes de un personaje (1.0 = media). Es el INPUT de <see cref="DerivedStats"/>. Coincide con el
/// set de `CompanionBase` (docs/creature-stats.md). Struct serializable para poder tunearla en el
/// Inspector; ojo: una struct SIN inicializar es todo 0, por eso usa <see cref="Default"/> como valor
/// inicial.
/// </summary>
[System.Serializable]
public struct Aptitudes
{
    public float agility, perception, strength, bodyMass, adaptability, composure,
                 endurance, reasoning, memory, creativity, sociability, discipline;

    /// <summary>Todas a 1.0 (media). Valor inicial recomendado.</summary>
    public static Aptitudes Default => new Aptitudes
    {
        agility = 1f, perception = 1f, strength = 1f, bodyMass = 1f, adaptability = 1f, composure = 1f,
        endurance = 1f, reasoning = 1f, memory = 1f, creativity = 1f, sociability = 1f, discipline = 1f
    };

    /// <summary>Copia las aptitudes de cualquier ser vivo (animal/companion/jugador) vía IAptitudes.</summary>
    public static Aptitudes From(IAptitudes a) => new Aptitudes
    {
        agility = a.Agility, perception = a.Perception, strength = a.Strength, bodyMass = a.BodyMass,
        adaptability = a.Adaptability, composure = a.Composure, endurance = a.Endurance,
        reasoning = a.Reasoning, memory = a.Memory, creativity = a.Creativity,
        sociability = a.Sociability, discipline = a.Discipline
    };

    /// <summary>Suma <paramref name="amt"/> a TODAS las aptitudes (crecimiento íntegro del alma).</summary>
    public void AddAll(float amt)
    {
        agility += amt; perception += amt; strength += amt; bodyMass += amt;
        adaptability += amt; composure += amt; endurance += amt; reasoning += amt;
        memory += amt; creativity += amt; sociability += amt; discipline += amt;
    }

    /// <summary>Suma <paramref name="amt"/> a la aptitud indicada (muta este struct).</summary>
    public void Add(AptitudeKind k, float amt)
    {
        switch (k)
        {
            case AptitudeKind.Agility:      agility      += amt; break;
            case AptitudeKind.Perception:   perception   += amt; break;
            case AptitudeKind.Strength:     strength     += amt; break;
            case AptitudeKind.BodyMass:     bodyMass     += amt; break;
            case AptitudeKind.Adaptability: adaptability += amt; break;
            case AptitudeKind.Composure:    composure    += amt; break;
            case AptitudeKind.Endurance:    endurance    += amt; break;
            case AptitudeKind.Reasoning:    reasoning    += amt; break;
            case AptitudeKind.Memory:       memory       += amt; break;
            case AptitudeKind.Creativity:   creativity   += amt; break;
            case AptitudeKind.Sociability:  sociability  += amt; break;
            case AptitudeKind.Discipline:   discipline   += amt; break;
        }
    }

    /// <summary>Lee el valor de la aptitud indicada (para gatear pensamientos/acciones por aptitud).</summary>
    public float Get(AptitudeKind k)
    {
        switch (k)
        {
            case AptitudeKind.Agility:      return agility;
            case AptitudeKind.Perception:   return perception;
            case AptitudeKind.Strength:     return strength;
            case AptitudeKind.BodyMass:     return bodyMass;
            case AptitudeKind.Adaptability: return adaptability;
            case AptitudeKind.Composure:    return composure;
            case AptitudeKind.Endurance:    return endurance;
            case AptitudeKind.Reasoning:    return reasoning;
            case AptitudeKind.Memory:       return memory;
            case AptitudeKind.Creativity:   return creativity;
            case AptitudeKind.Sociability:  return sociability;
            case AptitudeKind.Discipline:   return discipline;
            default:                        return 0f;
        }
    }
}

/// <summary>
/// Deriva los pools de acción/combate desde las aptitudes (docs/creature-stats.md §Pools derivados).
///
/// Funciones PURAS (sin estado ni escena) → fáciles de probar y reutilizar por <see cref="CharacterLevel"/>
/// y el futuro `NPCBase`. Coeficientes ajustables. A aptitudes 1.0 (media) y nivel 1 da ~100 vida,
/// ~100 energía y ~50 maná.
/// </summary>
public static class DerivedStats
{
    static float C(float v) => Mathf.Max(0f, v);                        // aptitud no-negativa

    // Factor de nivel: `soulLevels` = SUMA de niveles ganados en TODAS las margas (0 al empezar).
    // Cada nivel de CUALQUIER marga sube los puntos del alma → "cada marga es otro multiplicador".
    static float LevelFactor(int soulLevels, float per) => 1f + per * Mathf.Max(0, soulLevels);

    /// <summary>Vida = resistencia + fuerza + masa (aguante a golpes).</summary>
    public static float MaxHealth(Aptitudes a, int soulLevels) =>
        Mathf.Max(10f, (40f * C(a.endurance) + 35f * C(a.strength) + 25f * C(a.bodyMass))
                       * LevelFactor(soulLevels, 0.15f));

    /// <summary>Energía = resistencia + agilidad, penalizada por el peso (asanas, correr, trepar).</summary>
    public static float MaxEnergy(Aptitudes a, int soulLevels) =>
        Mathf.Max(10f, (60f * C(a.endurance) + 50f * C(a.agility) - 10f * C(a.bodyMass))
                       * LevelFactor(soulLevels, 0.10f));

    /// <summary>Maná = razonamiento + memoria (hechizos, magia intelectual).</summary>
    public static float MaxMana(Aptitudes a, int soulLevels) =>
        Mathf.Max(0f, (30f * C(a.reasoning) + 20f * C(a.memory))
                      * LevelFactor(soulLevels, 0.12f));

    /// <summary>Defensa pasiva (se resta al daño recibido) = masa + fuerza + temple.</summary>
    public static float PassiveDefense(Aptitudes a) =>
        5f * C(a.bodyMass) + 5f * C(a.strength) + 5f * C(a.composure);

    /// <summary>Poder de hechizo = creatividad + razonamiento (× nivel de alma). Base ~1.0.</summary>
    public static float SpellPower(Aptitudes a, int soulLevels) =>
        (0.6f * C(a.creativity) + 0.4f * C(a.reasoning)) * LevelFactor(soulLevels, 0.10f);
}
