using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DEL DESHIERBE CON CUIDADO (Berón 2026-11-05; docs/deshierbe-cuidadoso.md): hay malas hierbas de verdad; se levantan con raíz; las nativas van al
/// vivero y de ahí a la tundra; las no nativas, a la compostera y nunca a la tierra.
/// </summary>
public class DeshierbeTest : MonoBehaviour, ITestUnit
{
    public int Group => 100;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<DeshierbeTest>() == null) tr.gameObject.AddComponent<DeshierbeTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Deshierbe con cuidado: con raíz, las nativas a la tundra y las foráneas a la compostera");
        if (FindAnyObjectByType<Territorio>() == null) { Debug.Log("[TEST] SKIP · Deshierbe — esta escena no es el santuario"); TestProbe.End(); yield break; }
        yield return new WaitForSeconds(1f);
        TestProbe.Check("cada bancal tiene sus malas hierbas", GardenWeed.All.Count > 0, $"{GardenWeed.All.Count}");
        TestProbe.Check("la ficha dice cuáles son nativas (hay de las dos)", WildPlants.All.Any(s => s.native) && WildPlants.All.Any(s => !s.native));
        TestProbe.Check("Poa annua no es nativa", WildPlants.All.Any(s => s.scientific == "Poa annua" && !s.native));

        WeedBench.ResetCounts();
        GardenPlot plot = GardenPlot.All.FirstOrDefault(p => p != null && p.GetComponent<WeedPatch>() != null);
        if (!TestProbe.Check("hay un bancal con su maleza", plot != null)) { TestProbe.End(); yield break; }
        WeedPatch patch = plot.GetComponent<WeedPatch>();

        // Una de cada: se fuerza la especie para que la prueba no dependa del azar.
        patch.Sprout(); patch.Sprout();
        var mine = GardenWeed.All.Where(w => w != null && w.plot == plot).ToList();
        GardenWeed nat = mine[0], alien = mine[1];
        nat.species = WildPlants.All.First(s => s.native && s.survival > 0.8f); alien.species = WildPlants.All.First(s => !s.native);
        TestProbe.Check("la nativa se levanta entera", WeedBench.Lift(nat, "Prueba"));
        TestProbe.Check("…y queda viva en el vivero", WeedBench.Nursery.Count == 1);
        TestProbe.Check("la foránea se levanta", WeedBench.Lift(alien, "Prueba"));
        TestProbe.Check("…y va a la compostera, no al vivero", WeedBench.Composted == 1 && WeedBench.Nursery.Count == 1);
        yield return null;

        int planted = WeedBench.TransplantAll("Prueba");
        TestProbe.Check("la nativa se trasplanta (o consta que no agarró)", planted + WeedBench.Failed == 1, $"{planted} plantadas, {WeedBench.Failed} no agarraron");
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t.name.StartsWith("Trasplante_")))
        {
            bool ground = Physics.Raycast(t.position + Vector3.up * 3f, Vector3.down, out RaycastHit h, 6f, ~0, QueryTriggerInteraction.Ignore);
            TestProbe.Check($"{t.name}: en suelo de tundra y a ras", ground && h.collider.name == Territorio.Land && Mathf.Abs(h.point.y - t.position.y) < 0.1f);
            WildPlants.Species s = WildPlants.All.First(p => t.name.EndsWith(p.key));
            TestProbe.Check($"{t.name}: es nativa", s.native);
        }
        AreaFunctions.AreaFunction f = AreaFunctions.Get("hue_transplante");
        TestProbe.Check("el trasplante es una misión de la rotación, con E y virtual", f != null && f.sanctuary && f.roles.Length == 0 && PlayerTasks.Authored("hue_transplante") && VirtualMissions.Steps("hue_transplante").Length >= 3);
        WeedBench.ResetCounts();
        TestProbe.End();
    }
}
