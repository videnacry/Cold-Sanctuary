using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LA FAUNA FRÍA DE LOS OTROS SANTUARIOS (Berón 2026-11-05; docs/fauna-fria-santuarios.md): catálogos completos, cuerpo de cada ave, modelo de cada grupo, presupuesto y
/// comida. Las escenas se comprueban al montarlas: esto no necesita que estén cargadas.
/// </summary>
public class FaunaFriaTest : MonoBehaviour, ITestUnit
{
    public int Group => 110;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<FaunaFriaTest>() == null) tr.gameObject.AddComponent<FaunaFriaTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Fauna fría: marina y aérea, con sus tablas y su comida");
        foreach (string sp in ColdBirds.AirBudget.Keys)
        {
            TestProbe.Check($"{sp}: en todas sus tablas", SpeciesProfile.Of(sp) != SpeciesProfile.Default && LifeHistory.Has(sp) && SpeciesScaleFix.Has(sp) && PopulationPlan.PerHuman.ContainsKey(sp) && ActionsPrep.Of(sp) != null
                && Reproduction.Calendar(sp).gestationMonths != Reproduction.Calendar("no-existe").gestationMonths && StageProfile.Of(sp) != StageProfile.Of("no-existe") && FoodChain.Intake.ContainsKey(sp));
            TestProbe.Check($"{sp}: ave voladora, no insecto", CubeCreature.SpecFor(sp).plan == CubeCreature.Plan.FlyingBird && CubeBody.PlanFor(sp) == CubeBody.Plan.Bird);
            TestProbe.Check($"{sp}: vuela sola", ColdBirds.Is(sp));
        }
        TestProbe.Check("presupuesto bajo: pocas aves", ColdBirds.AirBudget.Values.Sum() <= 12, $"{ColdBirds.AirBudget.Values.Sum()}");
        foreach (FoodChain.Line l in FoodChain.Isolated(ColdBirds.AirBudget))
            TestProbe.Check($"{l.species}: con sus bancos no pasa hambre", l.sources.Count > 0 && l.covered >= FoodChain.Fed, $"{l.covered:P0} de {l.needs:0} kg");
        foreach (SwarmModels.Species s in new[] { SwarmModels.Species.Icefish, SwarmModels.Species.Toothfish, SwarmModels.Species.ColossalSquid, SwarmModels.Species.SeaSpider, SwarmModels.Species.YetiCrab })
        {
            GameObject g = SwarmModels.Make(s);
            TestProbe.Check($"{SwarmModels.Label(s)}: tiene modelo y nombre real", g != null && SwarmModels.Label(s).Contains("("));
            if (g != null) Destroy(g);
        }
        yield return null;
        TestProbe.End();
    }
}
