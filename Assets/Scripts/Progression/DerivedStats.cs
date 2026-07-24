using UnityEngine;

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

    /// <summary>Copia las aptitudes de un companion (para derivar SUS pools).</summary>
    public static Aptitudes From(CompanionBase c) => new Aptitudes
    {
        agility = c.agility, perception = c.perception, strength = c.strength, bodyMass = c.bodyMass,
        adaptability = c.adaptability, composure = c.composure, endurance = c.endurance,
        reasoning = c.reasoning, memory = c.memory, creativity = c.creativity,
        sociability = c.sociability, discipline = c.discipline
    };
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
    static float LevelFactor(int level, float per) => 1f + per * Mathf.Max(0, level - 1);

    /// <summary>Vida = resistencia + fuerza + masa (aguante a golpes).</summary>
    public static float MaxHealth(Aptitudes a, int level) =>
        Mathf.Max(10f, (40f * C(a.endurance) + 35f * C(a.strength) + 25f * C(a.bodyMass))
                       * LevelFactor(level, 0.15f));

    /// <summary>Energía = resistencia + agilidad, penalizada por el peso (asanas, correr, trepar).</summary>
    public static float MaxEnergy(Aptitudes a, int level) =>
        Mathf.Max(10f, (60f * C(a.endurance) + 50f * C(a.agility) - 10f * C(a.bodyMass))
                       * LevelFactor(level, 0.10f));

    /// <summary>Maná = razonamiento + memoria (hechizos, magia intelectual).</summary>
    public static float MaxMana(Aptitudes a, int level) =>
        Mathf.Max(0f, (30f * C(a.reasoning) + 20f * C(a.memory))
                      * LevelFactor(level, 0.12f));

    /// <summary>Defensa pasiva (se resta al daño recibido) = masa + fuerza + temple.</summary>
    public static float PassiveDefense(Aptitudes a) =>
        5f * C(a.bodyMass) + 5f * C(a.strength) + 5f * C(a.composure);

    /// <summary>Poder de hechizo = creatividad + razonamiento (× nivel). Multiplicador base ~1.0.</summary>
    public static float SpellPower(Aptitudes a, int level) =>
        (0.6f * C(a.creativity) + 0.4f * C(a.reasoning)) * LevelFactor(level, 0.10f);
}
