using System.Collections.Generic;
using UnityEngine;

/// <summary>Presets documentados de fuego (docs/magic-metabolism-progression.md §13).</summary>
public enum FireTier
{
    Spark,          // chispa / encender: ~800 W · 1 s = 800 J → ~0,018 g de combustible.
    Flamethrower,   // lanzallamas / bola: ~1 MW · 1 s = 1 MJ → ~22 g.
    DragonBreath    // aliento de dragón: ~100 MW · 1 s = 100 MJ → ~2,2 kg (INVIABLE por química; a S4 va por masa-energía).
}

/// <summary>
/// HECHIZO DE FUEGO con **coste físico REAL** (docs/magic-metabolism-progression.md §13/§15). El fuego es
/// **combustión**: quemar combustible (hidrocarburo/grasa, ~45 MJ/kg) libera la energía de la llama. El coste =
/// **potencia × tiempo** → energía (J) → **masa de combustible** (g). Se paga de `MagicReserves`:
///   • **Modo químico** (T1/T2): cuesta el **combustible en elementos** (C+H; el O₂ del aire es gratis) + una
///     pequeña **ignición** en julios. Chispa/lanzallamas se pagan comiendo. El aliento de dragón (2,2 kg/s) es
///     **inviable** así → por eso el juego sube de nivel.
///   • **Modo masa-energía** (T4, S4): en vez de quemar kilos, **aniquila** una masa minúscula (E=mc²): paga la
///     energía entera del **pool de energía** + ~µg de materia. Así un dragón/mago avanzado sostiene alientos.
/// Opt-in; se cablea en `Anima`s reales. Sin `MagicReserves` es gratis (prototipado).
/// </summary>
public class FireSpell : MonoBehaviour
{
    public Anima caster;

    [Header("Parámetros físicos del fuego")]
    [Tooltip("Potencia de la llama (W). Chispa ~800; lanzallamas ~1e6; aliento de dragón ~1e8.")]
    public float powerWatts = 1_000_000f;
    [Tooltip("Duración del disparo (s). El coste = potencia × tiempo.")]
    public float seconds = 1f;
    [Tooltip("Densidad energética del combustible que se quema mágicamente (J/kg). Hidrocarburos/grasa ~45 MJ/kg.")]
    public float fuelEnergyDensity = 45_000_000f;
    [Range(0f, 1f)]
    [Tooltip("Modo químico: fracción de la energía del fuego que se paga como IGNICIÓN del pool de energía (el resto la aporta el combustible).")]
    public float activationFraction = 0.02f;

    [Header("Nivel de mago")]
    [Tooltip("S4 / T4: en vez de quemar combustible, ANIQUILA materia (E=mc²) → paga energía pura + µg de masa.")]
    public bool massEnergyMode = false;

    // c² en J/g (E=mc²): 1 g de materia = 8,99e13 J = ~90 TJ.
    const float C2_JOULES_PER_GRAM = 8.98755e13f;

    /// <summary>Energía que libera/exige el fuego (J) = potencia × tiempo.</summary>
    public float EnergyReleased => Mathf.Max(0f, powerWatts) * Mathf.Max(0f, seconds);
    /// <summary>Masa de combustible a quemar (g) en modo químico.</summary>
    public float FuelGrams => fuelEnergyDensity > 0f ? EnergyReleased / (fuelEnergyDensity / 1000f) : 0f;
    /// <summary>Materia aniquilada (g) en modo masa-energía (minúscula).</summary>
    public float AnnihilatedGrams => EnergyReleased / C2_JOULES_PER_GRAM;
    /// <summary>Energía a pagar del pool: en masa-energía es TODA; en químico solo la ignición.</summary>
    public float EnergyCost => massEnergyMode ? EnergyReleased : EnergyReleased * activationFraction;

    void Awake() { if (caster == null) caster = GetComponent<Anima>(); }

    /// <summary>Coloca los parámetros de un preset documentado (§13).</summary>
    public void SetTier(FireTier tier)
    {
        switch (tier)
        {
            case FireTier.Spark:        powerWatts = 800f;         seconds = 1f; massEnergyMode = false; break;
            case FireTier.Flamethrower: powerWatts = 1_000_000f;   seconds = 1f; massEnergyMode = false; break;
            case FireTier.DragonBreath: powerWatts = 100_000_000f; seconds = 1f; massEnergyMode = true;  break;
        }
    }

    /// <summary>Combustible → elementos. Químico: hidrocarburo CH₂ ≈ 85,7% C + 14,3% H (el O₂ del aire, gratis).
    /// Masa-energía: la masa aniquilada (da igual el elemento; es materia).</summary>
    List<ElementCost> BuildCost()
    {
        if (massEnergyMode)
            return new List<ElementCost> { new ElementCost { symbol = "C", amount = AnnihilatedGrams } };
        float f = FuelGrams;
        return new List<ElementCost>
        {
            new ElementCost { symbol = "C", amount = f * 0.857f },
            new ElementCost { symbol = "H", amount = f * 0.143f },
        };
    }

    /// <summary>Lanza el fuego si hay reservas (materia + energía). Devuelve si salió.</summary>
    public bool Cast()
    {
        MagicReserves mr = caster != null ? caster.GetComponent<MagicReserves>() : GetComponent<MagicReserves>();
        List<ElementCost> cost = BuildCost();
        float eCost = EnergyCost;
        if (mr != null && !mr.Pay(cost, eCost))
        {
            Debug.Log($"[Fuego] «{name}» sin reservas: exige " +
                      (massEnergyMode ? $"{AnnihilatedGrams:0.###e0} g (aniquilar) + {eCost:0} J" :
                                        $"{FuelGrams:0.###} g de combustible (C+H) + {eCost:0} J de ignición") + ".");
            return false;
        }
        // Fuego = aura destructiva → más temido (Predation lo lee vía Anima.magicAura).
        if (caster != null) caster.GetComponent<MagicAura>()?.RegisterDestructiveUse(Mathf.Log10(EnergyReleased + 1f));

        Debug.Log($"[Fuego] «{name}» ¡llamarada! {powerWatts:0} W × {seconds:0.##}s = {EnergyReleased:0} J → " +
                  (massEnergyMode ? $"aniquila {AnnihilatedGrams:0.###e0} g + {eCost:0} J del pool (E=mc²)"
                                  : $"quema {FuelGrams:0.###} g de combustible (C+H) + {eCost:0} J de ignición") + ".");
        return true;
    }
}
