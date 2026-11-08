using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LA MAQUINARIA DE PRODUCCIÓN (Berón 2026-11-05; manual, regla P19; docs/produccion-maquinaria.md): cada área produce lo que se le pide, y la cifra sale
/// de las máquinas que hay en la escena, no de un número suelto.
/// </summary>
public class ProduccionTest : MonoBehaviour, ITestUnit
{
    public int Group => 103;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<ProduccionTest>() == null) tr.gameObject.AddComponent<ProduccionTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Maquinaria de producción: cada área da lo que se le pide");
        if (FindAnyObjectByType<Territorio>() == null) { Debug.Log("[TEST] SKIP · Producción — esta escena no es el santuario"); TestProbe.End(); yield break; }
        foreach (ProductionAudit.Result r in ProductionAudit.Audit())
            TestProbe.Check($"{r.area}: {r.product}", r.Ok, r.ToString());
        TestProbe.Check("ninguna área de producción sin revisar", ProductionAudit.Unlisted().Count == 0, string.Join(", ", ProductionAudit.Unlisted()));
        // (2026-11-08) EL HUERTO PARA TODOS: la verdura de las personas de la lista (32: 25,6 kg/día), de los bancales del Árbol y los estantes LED del
        // Huerto vertical de la torre de Producción; los estantes salen de la demanda (regla P19), no de un número.
        ProductionAudit.Result veg = ProductionAudit.Check(ProductionAudit.Lines.First(l => l.area == "Huerto"));
        float want = PopulationPlan.PlannedPeople * VerticalFarm.VegKgPerPersonDay, fields = VerticalFarm.FieldsKgDay();
        TestProbe.Check($"Huerto: pide la verdura de todas las personas de la lista ({PopulationPlan.PlannedPeople} × {VerticalFarm.VegKgPerPersonDay} = {want:0.0} kg/día) y la da",
            veg.demand >= want - 0.01f && veg.capacity >= veg.demand, $"{veg}; bancales {fields:0.0} kg/día + {VerticalFarm.RacksInScene()} estantes × {VerticalFarm.RackKgDay:0.00} = {VerticalFarm.CapacityKgDay():0.0}");
        int needRacks = VerticalFarm.RacksFor(veg.demand, fields);
        TestProbe.Check("los estantes LED del Huerto vertical salen de la demanda (lo que no dan los bancales, con su reserva)", VerticalFarm.RacksInScene() >= needRacks && needRacks > 0,
            $"{VerticalFarm.RacksInScene()} estantes; la demanda pide {needRacks} ({VerticalFarm.RackM2:0.0} m² de bandeja, {VerticalFarm.YieldKgM2Year} kg/m²/año: Pennisi et al. 2025)");
        VerticalFarm farm = FindAnyObjectByType<VerticalFarm>();
        ArbolCampos prodTree = farm != null ? farm.GetComponentInParent<ArbolCampos>() : null;
        SanctuaryArea farmArea = farm != null ? farm.GetComponent<SanctuaryArea>() : null;
        TestProbe.Check("el Huerto vertical es un piso de la torre de Producción", farmArea != null && farmArea.displayName == VerticalFarm.AreaName && prodTree != null && prodTree.building == "Producción",
            farm == null ? "no hay Huerto vertical" : $"«{farm.name}» en «{(prodTree != null ? prodTree.building : "ningún árbol")}»");
        if (farm != null)
        {
            string[] kit = { "Bloque_Vivero", "Bloque_Lavado", "Bloque_Cámara", "Bloque_Dosificador", "Bloque_Banco de herramientas", "Aseos" };
            var missing = kit.Where(k => !farm.GetComponentsInChildren<Transform>(true).Any(t => t.name.StartsWith(k))).ToList();
            TestProbe.Check("con su vivero, su lavado, su cámara, su dosificador, sus herramientas y sus aseos", missing.Count == 0, missing.Count == 0 ? "todo" : "faltan " + string.Join(", ", missing));
            FloorEnergy fe = farm.GetComponent<FloorEnergy>();
            TestProbe.Check("declara su energía (LED, clima y bombas)", fe != null && fe.KW >= farm.LedKW - 0.01f && farm.LedKW > 0f, fe != null ? $"{fe.KW:0.0} kW (LED {farm.LedKW:0.0})" : "sin FloorEnergy");
        }
        var huv = AreaWork.Catalog.Where(m => m.area == VerticalFarm.AreaName).ToList();
        TestProbe.Check("sus tareas diarias están en el catálogo y ninguna pasa de 1 h", huv.Count >= 3 && huv.All(m => m.hours <= 1f) && huv.Any(m => m.mandatory), string.Join(" · ", huv.Select(m => $"{m.id} {m.hours} h")));
        CulturedMeatLab lab = CulturedMeatLab.All.FirstOrDefault();
        TestProbe.Check("hay laboratorio", lab != null);
        if (lab != null)
        {
            ProductionAudit.Result meat = ProductionAudit.Check(ProductionAudit.Lines[0]);
            TestProbe.Near("el lote lo fija su maquinaria, no un número", lab.batchGrams / 1000f, meat.capacity, 0.5f);
            TestProbe.Near("la cadena alimenticia cuenta con lo que da de verdad", FoodChain.LabKgPerDay, meat.capacity, 0.5f);
        }
        // Una línea con una máquina de menos deja de cumplir: el auditor mira las máquinas.
        var fake = new ProductionAudit.Line { area = "Prueba", product = "nada", unit = "kg", demand = () => 1f, stages = new[] { new ProductionAudit.Stage("no existe", "Maquina_que_no_existe_", 10f) } };
        TestProbe.Check("sin máquinas no hay producción", !ProductionAudit.Check(fake).Ok);
        yield return null;
        TestProbe.End();
    }
}
