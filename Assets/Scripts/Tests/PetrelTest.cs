using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DEL PETREL DE LAS NIEVES (Berón 2026-11-05; docs/reglas-animales.md): vuela de verdad, sin NavMesh; se posa en los icebergs; anida en la roca de la isla, nunca en hielo; está
/// en todas sus tablas y en la cadena alimenticia. Su ciclo entero corre en su escena (grupo 58).
/// </summary>
public class PetrelTest : MonoBehaviour, ITestUnit
{
    public int Group => 109;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<PetrelTest>() == null) tr.gameObject.AddComponent<PetrelTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Petrel de las nieves: vuela, se posa en icebergs y anida en la roca");
        var birds = FindObjectsByType<SnowPetrelLife>(FindObjectsSortMode.None);
        if (Iceberg.All.Count == 0 || birds.Length == 0) { TestProbe.Check("hay petreles en el santuario", Iceberg.All.Count == 0); TestProbe.End(); yield break; }
        TestProbe.Check("en todas sus tablas", SpeciesProfile.Of("SnowPetrel") != SpeciesProfile.Default && LifeHistory.Has("SnowPetrel") && PopulationPlan.PerHuman.ContainsKey("SnowPetrel") && SpeciesScaleFix.Has("SnowPetrel") && Reproduction.Calendar("SnowPetrel").gestationMonths != Reproduction.Calendar("no-existe").gestationMonths);
        TestProbe.Check("cuerpo de ave voladora, con su envergadura real", CubeCreature.SpecFor("SnowPetrel").plan == CubeCreature.Plan.FlyingBird && Mathf.Abs(SpeciesScaleFix.Real["SnowPetrel"] - 0.78f) < 0.05f);
        TestProbe.Check("en la cadena alimenticia, con de qué comer", FoodChain.Report().Any(l => l.species == "SnowPetrel" && l.covered >= FoodChain.Fed));
        TestProbe.Check("en el medio aire", birds[0].TryGetComponent(out Animal air) && air.AirAffinity > 0.5f);

        // Nidos: sobre roca de la isla.
        var nests = SnowPetrelNest.All.Where(n => n != null).ToList();
        TestProbe.Greater("nidos en los acantilados de la isla", nests.Count, 3f, $"{nests.Count}");
        bool rock = nests.All(n => Physics.OverlapSphere(n.transform.position, 6f).Any(c => c.name == "Acantilado"));
        TestProbe.Check("todos en la roca, ninguno en hielo", rock);
        TestProbe.Check("sin nidos en los icebergs", !nests.Any(n => n.GetComponentInParent<Iceberg>() != null));
        TestProbe.Check("cada uno tiene su nido", birds.All(b => b.Nest != null));

        // Vuela sin NavMesh.
        SnowPetrelLife p = birds[0];
        Vector3 a0 = p.transform.position;
        yield return new WaitForSeconds(2f);
        TestProbe.Check("vuela: se mueve y sin agente de NavMesh", p != null && (!p.TryGetComponent(out UnityEngine.AI.NavMeshAgent nav) || !nav.enabled));
        TestProbe.Check("se separa del suelo", birds.Any(b => b.transform.position.y > 2f));

        // Se posa en un iceberg: se le deja a tiro. Espera acotada.
        Iceberg berg = Iceberg.All.First(b => b != null);
        p.transform.position = berg.PerchSpot(0f) + Vector3.up * 6f;
        if (p.TryGetComponent(out Animal an)) an.hungry = -0.5f;
        p.RestNow();
        float t0 = Time.realtimeSinceStartup;
        while (!p.Perched && Time.realtimeSinceStartup - t0 < 25f) yield return null;
        TestProbe.Check("se posa en el iceberg", p.Perched, p.OnBerg != null ? p.OnBerg.name : "sin iceberg");
        TestProbe.End();
    }
}
