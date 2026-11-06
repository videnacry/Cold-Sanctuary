using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LA CADENA ALIMENTICIA (Berón 2026-11-05; docs/reglas-animales.md, regla E15):
/// 1. PLAN: toda especie del santuario tiene dieta y nadie pasa hambre con lo planeado.
/// 2. ESCENA: con lo que hay de verdad (bancos y plantas contados), tampoco.
/// 3. ESPECIE NUEVA: añadir un depredador a <see cref="PopulationPlan.Groups"/> hace subir a sus presas sin tocar nada más.
/// 4. ESPECIES REALES: todo banco de peces, crustáceos, bivalvos y medusas es de una especie con nombre científico; hay de los dos polos.
/// </summary>
public class CadenaAlimenticiaTest : MonoBehaviour, ITestUnit
{
    public int Group => 85;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<CadenaAlimenticiaTest>() == null) tr.gameObject.AddComponent<CadenaAlimenticiaTest>();
    }

    // Lo que ofrece de verdad cada fuente de la escena, en las claves de FoodChain.
    static Dictionary<string, float> Counted()
    {
        const float year = FoodChain.DaysPerYear * FoodChain.SecondsPerGameDay;
        var real = new Dictionary<string, float> { { "peces", 0f }, { "kril", 0f }, { "bivalvos", 0f }, { "medusas", 0f } };
        foreach (Swarm s in Swarm.All)
        {
            if (s == null || s.kind == SwarmKind.Bee) continue;
            string k = s.kind == SwarmKind.Fish ? "peces" : s.kind == SwarmKind.Krill ? "kril" : s.kind == SwarmKind.Clam ? "bivalvos" : "medusas";
            real[k] += s.growthPerSecond * year * s.perUnitMass;
        }
        foreach (AffordanceKind k in new[] { AffordanceKind.Grass, AffordanceKind.Willow, AffordanceKind.Lichen, AffordanceKind.Berry })
            real[k.ToString()] = Affordance.Of(k).Where(a => a != null).Sum(a => a.maxStock * a.regrowPerSecond * year * 0.25f);
        return real;
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Cadena alimenticia: dietas, nadie pasa hambre, sube presas y especies reales");
        if (FindAnyObjectByType<Territorio>() == null) { Debug.Log("[TEST] SKIP · Cadena alimenticia — esta escena no es el santuario"); TestProbe.End(); yield break; }

        // ── 1. PLAN ──
        List<FoodChain.Line> plan = FoodChain.Report();
        foreach (FoodChain.Line l in plan)
        {
            TestProbe.Check($"{l.species}: tiene de qué comer", l.sources.Count > 0, string.Join(", ", l.sources));
            TestProbe.Check($"{l.species}: nadie pasa hambre con lo planeado", l.covered >= FoodChain.Fed, $"encuentra {l.covered:P0} de {l.needs:0} kg");
        }
        TestProbe.Check("más herbívoros que lobos en la isla", PopulationPlan.Target("Reindeer") > PopulationPlan.Target("Wolf") && PopulationPlan.Target("ArcticHare") > PopulationPlan.Target("ArcticFox"));

        // ── 2. ESCENA ──
        foreach (FoodChain.Line l in FoodChain.Report(Counted()))
            TestProbe.Check($"{l.species}: con lo que hay en la escena tampoco", l.covered >= FoodChain.Fed, $"encuentra {l.covered:P0}");
        TestProbe.Check("los bancos de la escena son los que pide la cadena", Swarm.All.Count(s => s != null && s.kind == SwarmKind.Fish) >= FoodChain.Banks(SwarmKind.Fish) && Swarm.All.Count(s => s != null && s.kind == SwarmKind.Clam) == FoodChain.Banks(SwarmKind.Clam));

        // ── 3. ESPECIE NUEVA (hipotética, y se deja todo como estaba) ──
        float reindeer = PopulationPlan.PerHuman["Reindeer"], hare = PopulationPlan.PerHuman["ArcticHare"], musk = PopulationPlan.PerHuman["MuskOx"], wolves = PopulationPlan.PerHuman["Wolf"];
        PopulationPlan.PerHuman["Wolf"] = wolves * 20f;   // (×2 ya no: el laboratorio, a medida del techo de depredadores (P19), les cubre esa comida y las presas no tienen por qué subir)
        PopulationPlan.Groups["PruebaDepredador"] = 1;   // desconocida: sin relaciones, sale sin dieta y no estorba
        PopulationPlan.PerHuman["PruebaDepredador"] = 0.2f;
        FoodChain.Evaluate();
        bool up = PopulationPlan.PerHuman["Reindeer"] > reindeer || PopulationPlan.PerHuman["ArcticHare"] > hare || PopulationPlan.PerHuman["MuskOx"] > musk;
        TestProbe.Check("con más depredadores suben sus presas, sola", up, $"reno {reindeer:0.00} → {PopulationPlan.PerHuman["Reindeer"]:0.00}, laboratorio {FoodChain.LabShare:P0}");
        PopulationPlan.Groups.Remove("PruebaDepredador"); PopulationPlan.PerHuman.Remove("PruebaDepredador");
        PopulationPlan.PerHuman["Wolf"] = wolves; PopulationPlan.PerHuman["Reindeer"] = reindeer; PopulationPlan.PerHuman["ArcticHare"] = hare; PopulationPlan.PerHuman["MuskOx"] = musk;
        FoodChain.Evaluate();

        // ── 4. ESPECIES REALES ──
        var swarms = Swarm.All.Where(s => s != null && s.kind != SwarmKind.Bee).ToList();
        TestProbe.Check("todo banco lleva su especie con nombre científico", swarms.All(s => s.speciesSet && SwarmModels.Label(s.species).Contains("(") || s.species == SwarmModels.Species.Diplulmaris || s.species == SwarmModels.Species.Desmonema), $"{swarms.Count} bancos");
        var used = swarms.Select(s => s.species).Distinct().ToList();
        foreach (SwarmModels.Species sp in new[] { SwarmModels.Species.SilverFish, SwarmModels.Species.ArcticCod, SwarmModels.Species.Krill, SwarmModels.Species.IceKrill, SwarmModels.Species.Cyanea, SwarmModels.Species.Desmonema, SwarmModels.Species.Clam })
            TestProbe.Check($"hay {SwarmModels.Label(sp)}", used.Contains(sp));
        yield return null;
        TestProbe.End();
    }
}
