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
///
/// **Migrado a `SpellBase`** (docs/stats-as-truth §hechizos): usa `CastMode.Repeat` (mantener `spellKey` =
/// disparo múltiple; cada llama que no impacta sube el **forcejeo**) y la **carga** (mantener `spellKey`+
/// `channelKey`/Shift = **channeling** → al soltar Shift dispara una llama más potente). El bonus de poder
/// (forcejeo físico + channeling mental) **multiplica la intensidad** → más gramos/energía por llama.
/// </summary>
public class FireSpell : SpellBase
{
    public Anima caster;   // (usa `spellKey` de SpellBase para el input; None = solo por Cast()/driver)

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

    float _powerMult = 1f;   // multiplicador de intensidad por el bonus (forcejeo+channeling); 1 = sin bonus.
    float _fireTimer;        // temporizador del disparo múltiple (Repeat).

    /// <summary>Energía que libera/exige el fuego (J) = potencia × tiempo × bonus (forcejeo/channeling).</summary>
    public float EnergyReleased => Mathf.Max(0f, powerWatts) * _powerMult * Mathf.Max(0f, seconds);
    /// <summary>Masa de combustible a quemar (g) en modo químico.</summary>
    public float FuelGrams => fuelEnergyDensity > 0f ? EnergyReleased / (fuelEnergyDensity / 1000f) : 0f;
    /// <summary>Materia aniquilada (g) en modo masa-energía (minúscula).</summary>
    public float AnnihilatedGrams => EnergyReleased / C2_JOULES_PER_GRAM;
    /// <summary>Energía a pagar del pool: en masa-energía es TODA; en químico solo la ignición.</summary>
    public float EnergyCost => massEnergyMode ? EnergyReleased : EnergyReleased * activationFraction;

    void Awake()
    {
        if (caster == null) caster = GetComponent<Anima>();
        castMode = CastMode.Repeat;
    }

    // Input propio (opt-in con `spellKey`): solo tecla = disparo múltiple (forcejeo por llama que no impacta);
    // tecla + channelKey (Shift) = CANALIZAR (carga el bonus mental, no dispara); al soltar Shift con la tecla
    // aún pulsada → dispara una llama cargada. El bonus multiplica la intensidad (más gramos/energía).
    void Update()
    {
        if (spellKey == KeyCode.None) return;
        float dt = Time.deltaTime;
        bool key = Input.GetKey(spellKey);
        bool channeling = key && Input.GetKey(channelKey);
        TickChanneling(channeling, dt);

        if (channeling) return;                                             // cargando: acumula, no dispara
        if (Input.GetKeyUp(channelKey) && key && RawChannel > 0f) { Cast(); return; }   // soltó Shift → llama cargada

        if (key)                                                           // solo tecla → disparo múltiple
        {
            _fireTimer -= dt;
            if (_fireTimer <= 0f)
            {
                Cast();
                ReportResult(false);   // sin detección de impacto aún → cada llama "empuja" el forcejeo (placeholder)
                _fireTimer = repeatCooldown;
            }
        }
        else _fireTimer = 0f;
    }

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

    /// <summary>Lanza el fuego si hay reservas (materia + energía). El bonus (forcejeo+channeling) multiplica la
    /// intensidad. Devuelve si salió.</summary>
    public bool Cast()
    {
        _powerMult = 1f + Mathf.Max(0f, BonusPower(caster));   // el bonus agranda la llama (más gramos/energía)
        MagicReserves mr = caster != null ? caster.GetComponent<MagicReserves>() : GetComponent<MagicReserves>();
        List<ElementCost> cost = BuildCost();
        float eCost = EnergyCost;
        if (mr != null && !mr.Pay(cost, eCost))
        {
            _powerMult = 1f;
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
        _powerMult = 1f;
        return true;
    }

    // ── API SpellBase (para IA/targeting) ──────────────────────────────────────
    public override bool CanCast(Anima c, ITarget target) => true;
    public override void Cast(Anima c, ITarget target) { if (c != null) caster = c; Cast(); }
}
