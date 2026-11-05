using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LA VIDA EN LOS ICEBERGS (Berón 2026-11-05; docs/vida-en-icebergs.md): animales de verdad, no figuras; con sitio al que subir, comida cerca, motivo para quedarse
/// y un descanso que va con el iceberg; y contados en la cadena alimenticia.
/// </summary>
public class IcebergVidaTest : MonoBehaviour, ITestUnit
{
    public int Group => 108;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<IcebergVidaTest>() == null) tr.gameObject.AddComponent<IcebergVidaTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Vida en los icebergs: animales reales con su motivo");
        if (Iceberg.All.Count == 0) { Debug.Log("[TEST] SKIP · Icebergs — esta escena no los tiene"); TestProbe.End(); yield break; }
        yield return new WaitForSeconds(1f);
        var visitors = FindObjectsByType<IcebergVisitor>(FindObjectsSortMode.None);
        foreach (Iceberg b in Iceberg.All)
        {
            if (b == null) continue;
            var here = visitors.Where(v => v.berg == b).Select(v => v.GetComponent<Animal>()).Where(a => a != null).ToList();
            TestProbe.Check($"{b.name}: tiene focas y pingüinos de verdad", here.Any(a => a.SpeciesName == "Seal") && here.Any(a => a.SpeciesName == "Penguin"), $"{here.Count}");
            TestProbe.Check($"{b.name}: ya no tiene figuras", !b.GetComponentsInChildren<Transform>().Any(t => t.name.StartsWith("Foca_iceberg") || t.name.StartsWith("Petrel_nival")));
            TestProbe.Check($"{b.name}: tiene repisa a ras de agua y descansadero", b.ledge != null && b.ledge.GetComponentInChildren<Affordance>() != null && b.ledge.GetComponentInChildren<Collider>().bounds.max.y < 1f);
            TestProbe.Check($"{b.name}: tiene peces y kril cerca", b.fish != null && b.krill != null);
            TestProbe.Check($"{b.name}: no crían aquí y nadan con MarineLife", here.All(a => a.GetComponent<MarineLife>() != null));
        }
        // El motivo, por especie y estación.
        TestProbe.Check("muda: la foca se queda más en el iceberg que se mueve", IcebergVisitor.Stay("Seal", false, 7).why == IcebergVisitor.Reason.Muda);
        TestProbe.Check("crías: en el varado, en primavera", IcebergVisitor.Stay("Seal", true, 1).why == IcebergVisitor.Reason.CuidadoCrias);
        TestProbe.Check("descanso, el resto del año, y el pingüino solo descansa", IcebergVisitor.Stay("Seal", true, 15).why == IcebergVisitor.Reason.Descanso && IcebergVisitor.Stay("Penguin", true, 1).why == IcebergVisitor.Reason.Descanso);
        // Cadena alimenticia.
        var seal = FoodChain.Report().FirstOrDefault(l => l.species == "Seal");
        TestProbe.Check("la cadena cuenta a los del iceberg", seal != null && seal.individuals == PopulationPlan.Raw("Seal") + IcebergVisitor.Seals);
        TestProbe.End();
    }
}
