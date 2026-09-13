using System.Collections.Generic;
using UnityEngine;

/// <summary>Estado de un elemento respecto a su cantidad IDEAL (docs/consciousness-mechanics.md §UI/estado ideal).</summary>
public enum ElementLevel { Deficiente, Bajo, Ideal, Alto, Exceso }

/// <summary>
/// ESTADO DE ELEMENTOS (docs/consciousness-mechanics.md) — la ÚNICA función que, dado cuánto tiene un ser de cada
/// elemento y su MASA, dice si cada elemento está deficiente/ideal/en exceso, con un COLOR para la UI (verde=estable,
/// naranja=fuera de rango, rojo=crítico) y el valor con unidad (g/mg/µg…).
///
/// Base científica REAL (composición elemental del cuerpo por % de masa): O 65%, C 18.5%, H 9.5%, N 3.3%, Ca 1.6%,
/// P 1.2%, K 0.4%, S 0.25%, Na 0.15%, Cl 0.15%, Mg 0.05% (≈99.9%). El IDEAL de un ser = esa fracción × su masa → se
/// **extrapola a CUALQUIER especie/body** con solo su masa (una hormiga y una ballena comparten las proporciones).
/// (Los RDAs de INGESTA diaria son otra tabla — esto es la COMPOSICIÓN de reservas; ver §activity para el gasto.)
/// </summary>
public static class ElementsStatus
{
    // Fracción de la masa corporal por elemento (los 11 que suman ~99.9%).
    static readonly Dictionary<string, float> IdealFraction = new Dictionary<string, float>
    {
        { "O", 0.650f }, { "C", 0.185f }, { "H", 0.095f }, { "N", 0.033f }, { "Ca", 0.016f },
        { "P", 0.012f }, { "K", 0.004f }, { "S", 0.0025f }, { "Na", 0.0015f }, { "Cl", 0.0015f }, { "Mg", 0.0005f },
    };

    /// <summary>Gramos IDEALES de un elemento para una masa corporal dada (kg). 0 si el elemento no está en la tabla.</summary>
    public static float IdealGrams(string element, float bodyMassKg)
        => (element != null && IdealFraction.TryGetValue(element, out float f)) ? f * bodyMassKg * 1000f : 0f;

    /// <summary>Clasifica una cantidad actual (g) frente a su ideal (g) → nivel. Usa el RATIO actual/ideal.</summary>
    public static ElementLevel Classify(float currentGrams, float idealGrams)
    {
        if (idealGrams <= 0f) return ElementLevel.Ideal;         // sin referencia → neutro
        float r = currentGrams / idealGrams;
        if (r < 0.5f)  return ElementLevel.Deficiente;
        if (r < 0.85f) return ElementLevel.Bajo;
        if (r <= 1.15f) return ElementLevel.Ideal;
        if (r <= 1.6f) return ElementLevel.Alto;
        return ElementLevel.Exceso;
    }

    /// <summary>El color de la UI para un nivel: verde=ideal, naranja=fuera de rango, rojo=crítico.</summary>
    public static Color ColorOf(ElementLevel level)
    {
        switch (level)
        {
            case ElementLevel.Deficiente: return new Color(0.85f, 0.20f, 0.20f);   // rojo
            case ElementLevel.Bajo:       return new Color(0.95f, 0.60f, 0.20f);   // naranja
            case ElementLevel.Ideal:      return new Color(0.30f, 0.80f, 0.40f);   // verde
            case ElementLevel.Alto:       return new Color(0.95f, 0.60f, 0.20f);   // naranja
            default:                      return new Color(0.85f, 0.20f, 0.20f);   // rojo (exceso)
        }
    }

    /// <summary>Formatea una cantidad en gramos con la unidad adecuada (g / mg / µg / ng) para la UI.</summary>
    public static string Format(float grams)
    {
        float a = Mathf.Abs(grams);
        if (a >= 1f)      return $"{grams:0.##} g";
        if (a >= 1e-3f)   return $"{grams * 1e3f:0.##} mg";
        if (a >= 1e-6f)   return $"{grams * 1e6f:0.##} µg";
        return $"{grams * 1e9f:0.##} ng";
    }

    /// <summary>Atajo: nivel + color de un elemento dado lo que el ser tiene (g) y su masa (kg). Lo llama la UI por elemento.</summary>
    public static (ElementLevel level, Color color) Evaluate(string element, float currentGrams, float bodyMassKg)
    {
        ElementLevel lvl = Classify(currentGrams, IdealGrams(element, bodyMassKg));
        return (lvl, ColorOf(lvl));
    }
}
