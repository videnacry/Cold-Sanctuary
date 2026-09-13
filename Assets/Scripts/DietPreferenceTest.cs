using System.Collections;
using UnityEngine;

/// <summary>
/// Test de la PREFERENCIA DE DIETA (docs/microcosmos-sakshi-origin.md §5, testing-checklist §43), por `TestProbe`:
/// función pura `DietPreference.For(predador, presa)` — determinista, sin escena. La mariquita prefiere el pulgón; un
/// depredador de hormigas prefiere HORMIGA a GUSANO (por eso va a Sakshi y no a Kushal); lo no listado es neutro (1).
/// </summary>
public class DietPreferenceTest : MonoBehaviour, ITestUnit
{
    public int Group => 10;
    public bool ParallelSafe => true;

    public IEnumerator Run()
    {
        TestProbe.Begin("Preferencia de dieta (apetencia por especie)");
        yield return null;

        TestProbe.Greater("la mariquita prefiere el pulgon a la propia (Aphid>1)", DietPreference.For("Ladybug", "Aphid"), 1f);
        TestProbe.Check("un depredador de hormigas PREFIERE hormiga a gusano",
            DietPreference.For("Depredador", "Ant") > DietPreference.For("Depredador", "Gusano"));
        TestProbe.Check("la hormiga le apetece (>1) y el gusano no (<1)",
            DietPreference.For("Depredador", "Ant") > 1f && DietPreference.For("Depredador", "Gusano") < 1f);
        TestProbe.Near("lo no listado es NEUTRO (1)", DietPreference.For("Ladybug", "Whale"), 1f, 0.001f);
        TestProbe.Near("predador/presa vacios → neutro (1)", DietPreference.For(null, "Ant"), 1f, 0.001f);

        TestProbe.End();
    }
}
