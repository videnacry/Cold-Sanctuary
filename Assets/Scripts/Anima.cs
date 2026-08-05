using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shared base for all living beings — animals, NPCs, and eventually the player.
///
/// Holds the drives every living thing has:
///   stress       → environmental pressure it feels
///   trauma       → history of harm that slows bonding
///   fatReserves  → metabolic storage
///   temperature  → body heat
///   bonds        → relational ties to other beings
///
/// Declares abstract hooks so each subclass responds to those drives differently:
///   Animal.RespondToHunger()    → hunts or grazes
///   NPCBase.RespondToHunger()   → navigates to the kitchen
///   PlayerEntity (future)       → fires a UI event
/// </summary>
public abstract class Anima : MonoBehaviour, IAptitudes
{
    // ── Drives ───────────────────────────────────────────────────────────────────

    [HideInInspector] public float stress;               // 0–1; ambient anxiety
    [HideInInspector] public float trauma;               // 0–100; slows bond growth, fades over time
    [HideInInspector] public float fatReserves;          // metabolic storage
    [HideInInspector] public float temperature = 38f;    // body temperature in °C
    [HideInInspector] public bool  death;
    [HideInInspector] public bool  asleep;
    [HideInInspector] public bool  aware;                // true while actively responding to a threat

    // Drives de mente/físico UNIVERSALES (migración 2026-07-28): eran de PlayerStats/WorldCharacter y se
    // consolidan aquí (hogar único). Baratos (floats); la mente compleja sigue siendo un pilar opcional.
    [HideInInspector] public float satisfaction;         // 0–1
    [HideInInspector] public float mentalFatigue;        // 0–1
    [HideInInspector] public float sleepiness;           // 0–1
    [HideInInspector] public float observationRadius = 3f;
    [HideInInspector] public float velocity = 1f;
    [HideInInspector] public float physicalResistance = 1f;

    // Aptitudes (1.0 = media real de la especie/arquetipo; escalan con tareas/origen — ver docs/creature-stats.md)
    // Hogar UNIVERSAL de las 12 aptitudes (docs §Progresión): todo Anima las tiene. Los animales
    // usan activamente agility/perception (evolucionan); el resto quedan latentes por ahora (default 1).
    [HideInInspector] public float agility      = 1f;
    [HideInInspector] public float perception   = 1f;
    [HideInInspector] public float strength     = 1f;
    [HideInInspector] public float bodyMass     = 1f;
    [HideInInspector] public float adaptability = 1f;
    [HideInInspector] public float composure    = 1f;
    [HideInInspector] public float endurance    = 1f;
    [HideInInspector] public float reasoning    = 1f;
    [HideInInspector] public float memory       = 1f;
    [HideInInspector] public float creativity   = 1f;
    [HideInInspector] public float sociability  = 1f;
    [HideInInspector] public float discipline   = 1f;
    // Extendidas para deep-sim emocional (docs/emotion-model.md §6); aún NO en IAptitudes:
    [HideInInspector] public float afabilidad   = 1f;   // agreeableness: calidez/cooperación vs frialdad
    [HideInInspector] public float sensibilidad = 1f;   // reactividad EMOCIONAL (cuánto oscilan los humores).
                                                        // OJO: distinta de Animal.sensibility (rango de detección de amenazas).

    // IAptitudes — acceso uniforme (getters virtuales: Animal puede sobreescribir p.ej. Strength/BodyMass
    // desde Physiognomy más adelante).
    public virtual float Agility      => agility;
    public virtual float Perception   => perception;
    public virtual float Strength     => strength;
    public virtual float BodyMass     => bodyMass;
    public virtual float Adaptability => adaptability;
    public virtual float Composure    => composure;
    public virtual float Endurance    => endurance;
    public virtual float Reasoning    => reasoning;
    public virtual float Memory       => memory;
    public virtual float Creativity   => creativity;
    public virtual float Sociability  => sociability;
    public virtual float Discipline   => discipline;

    // Medio actual + afinidad por medio (tierra/agua/aire). El rendimiento físico se multiplica
    // por la afinidad del medio en que está la criatura. Ver docs/creature-stats.md §Modificadores de medio.
    [HideInInspector] public Medium currentMedium = Medium.Land;
    public virtual float LandAffinity  => 1f;
    public virtual float WaterAffinity => 0.4f;   // por defecto: un animal terrestre nada torpemente
    public virtual float AirAffinity   => 0f;
    public float MediumFactor => currentMedium == Medium.Water ? WaterAffinity
                               : currentMedium == Medium.Air   ? AirAffinity
                               :                                 LandAffinity;
    /// <summary>Agilidad efectiva en el medio actual (base × afinidad del medio).</summary>
    public float EffectiveAgility => agility * MediumFactor;

    public List<Bond> bonds = new List<Bond>();

    // ── Drive response hooks ─────────────────────────────────────────────────────

    /// <summary>Called by the being's internal cycle when hunger needs addressing.</summary>
    protected abstract void RespondToHunger();

    /// <summary>
    /// How threatening is this source to this being?
    /// Returns 0 (safe) to 1+ (existential threat).
    /// Used to decide whether to flee, fight, or ignore.
    /// </summary>
    protected abstract float EvaluateThreat(GameObject source);

    /// <summary>
    /// Called when a threat exceeds this being's tolerance threshold.
    /// Public so external systems (e.g. a predator) can trigger it directly.
    /// </summary>
    public abstract void RespondToThreat(GameObject threat);

    /// <summary>Override to react when this being's life stage advances.</summary>
    protected virtual void OnLifeStageChanged(char prev, char next) { }

    // ── Species parameters (override per species/archetype) ──────────────────────

    public virtual float HarmVsBond          => 0.5f;
    public virtual float BondGrowthRate      => 1f;
    public virtual float MaxFatReserves      => 20f;
    public virtual float FatAccumulationRate => 0.5f;

    // ── Bond system ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Current life stage as a char constant from LifeStage.
    /// Used to compute bond growth multipliers. Override in subclasses.
    /// </summary>
    protected virtual char LifeStageChar => LifeStage.adult;

    /// <summary>
    /// Real bond growth rate: base × life-stage multiplier × (1 − trauma).
    /// Cubs bond 3× faster; trauma slows bonding proportionally.
    /// </summary>
    public float EffectiveBondGrowthRate
    {
        get
        {
            char stage = LifeStageChar;
            float lifeMultiplier = stage == LifeStage.child ? 3f
                                 : stage == LifeStage.teen  ? 1.5f
                                 : 0.5f;
            return BondGrowthRate * lifeMultiplier * (1f - trauma / 100f);
        }
    }

    public Bond GetBond(ITarget target)
    {
        foreach (Bond b in bonds)
            if (b.target == target) return b;
        return null;
    }

    public void GrowBond(ITarget target, BondType type, float amount)
    {
        Bond b = GetBond(target);
        if (b == null) { b = new Bond(target, type); bonds.Add(b); }
        b.value = Mathf.Clamp(b.value + amount * EffectiveBondGrowthRate, 0f, 100f);
    }

    /// <summary>False if the bond with this target is strong enough to block harm.</summary>
    public bool CanHarm(ITarget target)
    {
        if (target == null) return true;
        Bond b = GetBond(target);
        if (b == null) return true;
        return b.value < HarmVsBond * 100f;
    }
}
