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
        // (Berón 2026-11-09, fauna por humanos) El total es K × humanos y cada nivel trófico tiene su parte: con más depredadores, DENTRO de las presas sube
        // la parte de la cazada y baja la de otras, y el total no cambia. Sin el laboratorio (LabOverride = 0), los lobos cazan todo lo que comen: si no,
        // el alimentador les cubre la comida (P19) y no hay por qué subir nada.
        float reindeer = PopulationPlan.PerHuman["Reindeer"], hare = PopulationPlan.PerHuman["ArcticHare"], musk = PopulationPlan.PerHuman["MuskOx"], wolves = PopulationPlan.PerHuman["Wolf"];
        var prey = PopulationPlan.Groups.Keys.Where(s => PopulationPlan.LevelOf(s) == PopulationPlan.Trophic.Prey).ToList();
        var share0 = PopulationPlan.Shares(PopulationPlan.Groups.Keys);
        PopulationPlan.PerHuman["Wolf"] = wolves * 20f;
        PopulationPlan.Groups["PruebaDepredador"] = 1;   // desconocida: sin relaciones, sale sin dieta y no estorba (y obliga a evaluar la cadena otra vez)
        PopulationPlan.PerHuman["PruebaDepredador"] = 0.2f;
        FoodChain.LabOverride = 0f;
        FoodChain.Evaluate();
        var share1 = PopulationPlan.Shares(PopulationPlan.Groups.Keys);
        var hunted = new[] { "Reindeer", "ArcticHare", "MuskOx" };
        bool up = hunted.Any(s => share1[s] > share0[s] + 1e-4f);
        bool otherDown = prey.Any(s => share1[s] < share0[s] - 1e-4f);
        int total = PopulationPlan.TotalCap, alloc = PopulationPlan.Allocate(total, PopulationPlan.Groups.Keys, out _).Values.Sum();
        TestProbe.Check("con más depredadores sube la parte de su presa dentro de las presas, baja la de otras y el total sigue en K × humanos",
            up && otherDown && alloc == total,
            $"reno {share0["Reindeer"]:P1} → {share1["Reindeer"]:P1}, liebre {share0["ArcticHare"]:P1} → {share1["ArcticHare"]:P1}, buey {share0["MuskOx"]:P1} → {share1["MuskOx"]:P1} · total {alloc}/{total} · laboratorio {FoodChain.LabShare:P0}");
        FoodChain.LabOverride = null;
        PopulationPlan.Groups.Remove("PruebaDepredador"); PopulationPlan.PerHuman.Remove("PruebaDepredador");
        PopulationPlan.PerHuman["Wolf"] = wolves; PopulationPlan.PerHuman["Reindeer"] = reindeer; PopulationPlan.PerHuman["ArcticHare"] = hare; PopulationPlan.PerHuman["MuskOx"] = musk;
        FoodChain.Evaluate();

        // ── 4. ESPECIES REALES ──
        var swarms = Swarm.All.Where(s => s != null && s.kind != SwarmKind.Bee).ToList();
        TestProbe.Check("todo banco lleva su especie con nombre científico", swarms.All(s => s.speciesSet && SwarmModels.Label(s.species).Contains("(") || s.species == SwarmModels.Species.Diplulmaris || s.species == SwarmModels.Species.Desmonema), $"{swarms.Count} bancos");
        var used = swarms.Select(s => s.species).Distinct().ToList();
        foreach (SwarmModels.Species sp in new[] { SwarmModels.Species.SilverFish, SwarmModels.Species.ArcticCod, SwarmModels.Species.Krill, SwarmModels.Species.IceKrill, SwarmModels.Species.Cyanea, SwarmModels.Species.Desmonema, SwarmModels.Species.Clam })
            TestProbe.Check($"hay {SwarmModels.Label(sp)}", used.Contains(sp));
        Caza();
        yield return null;
        TestProbe.End();
    }

    // LA CAZA RESPETA LA CADENA (2026-11-08): en una batería (≈1,5 días de juego) caían 3 de las 6 liebres y los 2 zorros, y lobos y osos se cazaban entre sí.
    // La cadena da por año del santuario lo que crían (crías al año por hembra × ½ hembras × margen); se caza eso, y nunca por debajo de una pareja.
    void Caza()
    {
        TestProbe.Check("cuota de caza: 2 liebres por año del santuario (6 × 1 cría × ½ × 0,8)", HuntQuota.PerYear("ArcticHare") == 2, $"{HuntQuota.PerYear("ArcticHare")}");
        TestProbe.Check("cuota de caza: 1 buey almizclero (3 × 12/15 × ½ × 0,8, al menos 1)", HuntQuota.PerYear("MuskOx") == 1, $"{HuntQuota.PerYear("MuskOx")}");
        TestProbe.Check("con la cuota del año cumplida no se caza más de esa especie", HuntQuota.Allows("ArcticHare", 6, 1) && !HuntQuota.Allows("ArcticHare", 6, 2));
        TestProbe.Check("nunca por debajo de una pareja: con 2 vivos no se caza", HuntQuota.Allows("ArcticHare", 3, 0) && !HuntQuota.Allows("ArcticHare", 2, 0));
        TestProbe.Check("una especie fuera del plan (escenarios de prueba) no tiene cuota", HuntQuota.Allows("Bunny", 1, 99));
        TestProbe.Check("lobo y oso no se comen entre sí", DietPreference.For("Wolf", "Bear") == 0f && DietPreference.For("Bear", "Wolf") == 0f);
        TestProbe.Check("ni lobo ni oso tienen al zorro ártico por comida", DietPreference.For("Wolf", "ArcticFox") == 0f && DietPreference.For("Bear", "ArcticFox") == 0f);
        TestProbe.Check("el lobo sigue cazando buey, reno y liebre (Mech 2007)", DietPreference.For("Wolf", "MuskOx") > 0f && DietPreference.For("Wolf", "Reindeer") > 0f && DietPreference.For("Wolf", "ArcticHare") > 0f);
    }
}
