using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LA MAQUINARIA DE PRODUCCIÓN (Berón 2026-11-05; manual, regla P8; docs/produccion-maquinaria.md): cada área produce lo que se le pide, y la cifra sale
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
