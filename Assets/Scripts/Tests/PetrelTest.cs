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
        // (el acantilado de la isla o el peñasco de un casquete silvestre: ese no tiene collider, lo marca su posadero)
        bool OnRock(SnowPetrelNest n) => Physics.OverlapSphere(n.transform.position, 6f).Any(c => c.name == "Acantilado")
            || (n.transform.parent != null && n.transform.parent.GetComponentsInChildren<Transform>(true).Any(t => t.name == "Posadero_roca" && (t.position - n.transform.position).sqrMagnitude < 64f));
        var off = nests.Where(n => !OnRock(n)).ToList();
        TestProbe.Check("todos en la roca, ninguno en hielo", off.Count == 0, off.Count > 0 ? $"{off.Count} fuera, el primero en {off[0].transform.position}" : "");
        TestProbe.Check("sin nidos en los icebergs", !nests.Any(n => n.GetComponentInParent<Iceberg>() != null));
        TestProbe.Check("cada uno tiene su nido", birds.All(b => b.Nest != null));

        // Vuela sin NavMesh.
        SnowPetrelLife p = birds.FirstOrDefault(b => !b.Breeding) ?? birds[0];   // (el que incuba o corteja va a su nido, no a descansar)
        Vector3 a0 = p.transform.position;
        yield return new WaitForSeconds(2f);
        TestProbe.Check("vuela: se mueve y sin agente de NavMesh", p != null && (!p.TryGetComponent(out UnityEngine.AI.NavMeshAgent nav) || !nav.enabled));
        TestProbe.Check("se separa del suelo", birds.Any(b => b.transform.position.y > 2f));

        // Se posa en un iceberg: se le deja a tiro. Espera acotada.
        Iceberg berg = Iceberg.All.First(b => b != null);
        p.transform.position = berg.PerchSpot(0f) + Vector3.up * 6f;
        if (p.TryGetComponent(out Animal an)) an.hungry = -0.5f;
        p.RestNow(berg);
        float t0 = Time.realtimeSinceStartup;
        while (!p.Perched && Time.realtimeSinceStartup - t0 < 25f) { if (an != null) an.hungry = -0.5f; yield return null; }
        TestProbe.Check("se posa en el iceberg", p.Perched, p.OnBerg != null ? p.OnBerg.name : "sin iceberg");
        // Posado ENCIMA, no dentro: la jornada mira con una esfera de 0,12 m y lo veía «dentro de Iceberg_deriva» a y = 30.
        // Lo más bajo que queda MIENTRAS está posado (a 1 m de su sitio ya cuenta como posado y baja hasta él). Antes se medía a los 1,5 s, sin
        // sujetarle el hambre: si para entonces había vuelto a volar (sube en vertical hasta pasar el iceberg) salían «6,74 m sobre la caja».
        float clear = float.MaxValue; int perchedFrames = 0; float w0 = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - w0 < 1.5f)
        {
            if (an != null) an.hungry = -0.5f;
            if (p.Perched) { perchedFrames++; clear = Mathf.Min(clear, p.transform.position.y - (berg.transform.position.y + berg.size.y * 0.1f)); }
            yield return null;
        }
        TestProbe.Check("posado sobre el hielo, no dentro de él", perchedFrames > 0 && clear > 0.12f, perchedFrames > 0 ? $"lo más bajo, {clear:0.00} m sobre la caja del iceberg ({perchedFrames} fotogramas posado)" : "no se quedó posado");
        TestProbe.End();
    }
}
