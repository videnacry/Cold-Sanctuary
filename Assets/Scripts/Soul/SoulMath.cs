using UnityEngine;

/// <summary>Modo de CONVERSIÓN de stats (docs/soul-relations-reincarnation.md §1):
/// <b>Literal (B)</b> conserva la FORMA exacta del ser, redimensionada al presupuesto del cuerpo nuevo (identidad
/// marcada — para reencarnaciones). <b>Relative (A)</b> modula esa forma por la CAPACIDAD del cuerpo nuevo
/// (producto por-stat, renormalizado).</summary>
public enum ConversionMode { Relative, Literal }

/// <summary>
/// Matemática del alma (docs/soul-relations-reincarnation.md §1). El blend y la conversión son la MISMA idea:
/// mezclar **distribuciones** (la *forma* de dónde están los puntos) a un **presupuesto** común. Así un arquetipo
/// al 1% empuja la forma un 1% (no es despreciable) y una transformación conserva la identidad en el cuerpo nuevo.
/// </summary>
public static class SoulMath
{
    public static readonly AptitudeKind[] Physical =
    {
        AptitudeKind.Agility, AptitudeKind.Perception, AptitudeKind.Strength,
        AptitudeKind.BodyMass, AptitudeKind.Adaptability, AptitudeKind.Endurance,
    };
    public static readonly AptitudeKind[] Mental =
    {
        AptitudeKind.Composure, AptitudeKind.Reasoning, AptitudeKind.Memory,
        AptitudeKind.Creativity, AptitudeKind.Sociability, AptitudeKind.Discipline,
    };
    public static readonly AptitudeKind[] All =
    {
        AptitudeKind.Agility, AptitudeKind.Perception, AptitudeKind.Strength, AptitudeKind.BodyMass,
        AptitudeKind.Adaptability, AptitudeKind.Composure, AptitudeKind.Endurance, AptitudeKind.Reasoning,
        AptitudeKind.Memory, AptitudeKind.Creativity, AptitudeKind.Sociability, AptitudeKind.Discipline,
    };

    /// <summary>Suma (presupuesto) de las aptitudes indicadas.</summary>
    public static float Budget(Aptitudes a, AptitudeKind[] kinds)
    {
        float s = 0f;
        foreach (AptitudeKind k in kinds) s += Mathf.Max(0f, a.Get(k));
        return s;
    }

    /// <summary>Multiplica todas las aptitudes por un escalar (magnitud/poder).</summary>
    public static Aptitudes Scale(Aptitudes a, float f)
    {
        Aptitudes r = new Aptitudes();
        foreach (AptitudeKind k in All) r.Add(k, a.Get(k) * f);
        return r;
    }

    /// <summary>Reescala la FORMA de `src` (sobre `kinds`) para que su presupuesto sea `targetBudget`.</summary>
    public static Aptitudes RescaleShape(Aptitudes src, AptitudeKind[] kinds, float targetBudget)
    {
        Aptitudes r = new Aptitudes();
        float b = Budget(src, kinds);
        if (b <= 0f || targetBudget <= 0f) return r;
        float f = targetBudget / b;
        foreach (AptitudeKind k in kinds) r.Add(k, Mathf.Max(0f, src.Get(k)) * f);
        return r;
    }

    /// <summary>CONVERSIÓN de un ser (`current`) a una base nueva (`newBase`), sobre las 12 aptitudes.
    /// Literal (B): forma exacta al presupuesto del cuerpo nuevo. Relative (A): forma × capacidad del cuerpo
    /// nuevo (producto por-stat), renormalizada al mismo presupuesto.</summary>
    public static Aptitudes Remap(Aptitudes current, Aptitudes newBase, ConversionMode mode)
    {
        float target = Budget(newBase, All);
        float curB = Budget(current, All);
        if (curB <= 0f || target <= 0f) return newBase;

        if (mode == ConversionMode.Literal)
            return RescaleShape(current, All, target);

        // Relative: raw[k] = (current[k]/curB) · newBase[k]; luego renormalizar a `target`.
        Aptitudes raw = new Aptitudes();
        foreach (AptitudeKind k in All)
            raw.Add(k, (Mathf.Max(0f, current.Get(k)) / curB) * Mathf.Max(0f, newBase.Get(k)));
        return RescaleShape(raw, All, target);
    }
}
