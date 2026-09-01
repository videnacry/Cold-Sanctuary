using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Identidad de ESPECIE como componente (docs/anima-dissolving-animal.md, etapa 5). Guarda el nombre de especie/
/// arquetipo (Bear, Wolf, Human…) y **aplica** sus stats base (físicas/mentales del catálogo `Archetypes`) + los
/// **pensamientos base** de la especie al `Anima`. Con esto la "especie" deja de ser el tipo de clase
/// (`BearBehaviour`, `WolfBehavior`…) y pasa a ser **data de un componente** — el paso para reconstruir un lobo por
/// composición (`SimpleAnima` + componentes) sin una subclase por especie.
///
/// Respeta `agility`/`perception` (las gobiernan `BaseAgility`/`BasePerception` + evolución), igual que hacía
/// `Animal.ApplySpeciesArchetype`. También da el `SpeciesName` para relaciones/karma.
/// </summary>
public class SpeciesBody : MonoBehaviour
{
    [Tooltip("Nombre de especie/arquetipo (Bear, Wolf, Human…): stats base físicas/mentales + relaciones/karma.")]
    public string species;

    /// <summary>Config escalar de la especie (PackFactor, bond, reservas, IEdible, umbrales post-natales…). La fija
    /// <see cref="Apply"/> desde <see cref="SpeciesProfile.Of"/>; la lee `Animal` en sus virtuals.</summary>
    public SpeciesProfile profile = SpeciesProfile.Default;

    [Header("Bases evolutivas (agility/perception evolucionan hacia aquí; sensibility = base × perception)")]
    public float baseAgility = 1f;
    public float basePerception = 1f;
    public float baseSensibility = 5f;
    [Tooltip("Radio de territorio/hogar de la especie (data; antes override HomeRadius por clase).")]
    public float homeRadius = 100f;

    static Dictionary<string, float> _homeRadii;
    static void BuildHomeRadii() => _homeRadii = new Dictionary<string, float>
    {
        { "Whale", 400f }, { "Bear", 300f }, { "Deer", 250f }, { "Wolf", 200f },
        { "Fox", 150f }, { "Seal", 150f }, { "Malamute", 100f }, { "Bunny", 20f },
        { "Orca", 400f }, { "Penguin", 120f },
    };

    // Bases evolutivas por especie (data; antes eran overrides BaseAgility/BasePerception por clase).
    static Dictionary<string, (float agi, float per)> _baseStats;
    static void BuildBaseStats() => _baseStats = new Dictionary<string, (float, float)>
    {
        { "Fox",  (1.4f, 1.5f) }, { "Bear",     (0.7f, 1.1f) }, { "Whale", (0.6f, 1.0f) }, { "Bunny", (1.5f, 1.6f) },
        { "Wolf", (1.2f, 1.4f) }, { "Malamute", (1.1f, 1.1f) }, { "Deer",  (1.4f, 1.5f) }, { "Seal",  (1.1f, 1.2f) },
        { "Orca", (1.3f, 1.4f) }, { "Penguin",  (0.9f, 1.1f) },
    };

    /// <summary>Escribe en el `Anima` las aptitudes NO-evolutivas del arquetipo (fuerza/masa/mentales) y siembra los
    /// pensamientos base de la especie (si tiene `Mind`). No toca agility/perception (evolutivas).</summary>
    public void Apply(Anima a)
    {
        if (a == null || string.IsNullOrEmpty(species)) return;
        ArchetypeProfile b = Archetypes.BodyOf(species);
        ArchetypeProfile m = Archetypes.MindOf(species);
        a.strength    = b.aptitudes.strength;   a.bodyMass    = b.aptitudes.bodyMass;
        a.endurance   = b.aptitudes.endurance;  a.adaptability = b.aptitudes.adaptability;
        a.composure   = m.aptitudes.composure;  a.reasoning   = m.aptitudes.reasoning;   a.memory     = m.aptitudes.memory;
        a.creativity  = m.aptitudes.creativity; a.sociability = m.aptitudes.sociability;  a.discipline = m.aptitudes.discipline;

        a.landAffinity = b.landAffinity; a.waterAffinity = b.waterAffinity; a.airAffinity = b.airAffinity;   // medio (data del arquetipo)

        profile = SpeciesProfile.Of(species);       // config escalar por especie (la lee Animal)
        if (_homeRadii == null) BuildHomeRadii();
        if (species != null && _homeRadii.TryGetValue(species, out float hr)) homeRadius = hr;   // radio de territorio

        if (_baseStats == null) BuildBaseStats();   // bases evolutivas por especie
        if (species != null && _baseStats.TryGetValue(species, out (float agi, float per) bs)) { baseAgility = bs.agi; basePerception = bs.per; }
        // sensibility NO se fija acá: es propio de `Animal` (rango de detección de amenazas, no de `Anima`
        // genérico) y `Animal.EvolveAptitudes()` ya lo recalcula cada tick desde `BaseSensibility * perception`.
        a.agility = baseAgility; a.perception = basePerception;   // agility/perception evolucionan desde aquí

        Mind mind = a.GetComponent<Mind>();     // pensamientos base de la especie (si tiene Mente): piensa como su especie
        if (mind != null) mind.SeedThoughts(Archetypes.BaseThoughtsOf(species));
    }
}
