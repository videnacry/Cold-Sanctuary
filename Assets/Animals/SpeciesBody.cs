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

        Mind mind = a.GetComponent<Mind>();     // pensamientos base de la especie (si tiene Mente): piensa como su especie
        if (mind != null) mind.SeedThoughts(Archetypes.BaseThoughtsOf(species));
    }
}
