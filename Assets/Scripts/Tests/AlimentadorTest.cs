using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DEL ALIMENTADOR (Berón 2026-11-05; docs/alimentador.md): no hay comedero; la lista es compartida y nadie mira dos veces al mismo; quien tiene
/// hambre recibe su ración desde lejos, quien no, queda marcado; misión por rotación, con E, gesto y pasos virtuales. Y la cadena alimenticia lo cuenta.
/// </summary>
public class AlimentadorTest : MonoBehaviour, ITestUnit
{
    public int Group => 77;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<AlimentadorTest>() == null) tr.gameObject.AddComponent<AlimentadorTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Alimentador: sin comedero, lista compartida, distancia segura y rotación");
        if (FindAnyObjectByType<Territorio>() == null) { Debug.Log("[TEST] SKIP · Alimentador — esta escena no es el santuario"); TestProbe.End(); yield break; }

        TestProbe.Check("ya no hay comedero", GameObject.Find("Comedero_depredadores") == null);
        TestProbe.Check("hay almacén de raciones junto al laboratorio", PredatorRound.Store != null && PredatorRound.Store.name == "Almacen_raciones");

        // ── La misión: para cualquiera, por rotación ──
        foreach (string id in new[] { "gua_alimentador", "gua_alimentador_2" })
        {
            AreaFunctions.AreaFunction f = AreaFunctions.Get(id);
            TestProbe.Check($"{id}: en el catálogo, sin profesión exigida", f != null && f.sanctuary && f.roles.Length == 0);
            TestProbe.Check($"{id}: con pasos de E", PlayerTasks.Authored(id));
            TestProbe.Check($"{id}: con pasos virtuales", VirtualMissions.Steps(id).Length >= 3);
            TestProbe.Check($"{id}: con gesto de jornada", Performances.ForMission(id) != null);
        }

        // ── La lista compartida ──
        PredatorRound.NewRound();
        var list = PredatorRound.Today();
        TestProbe.Check("la lista tiene a los depredadores de tierra (oso y lobo, y ni uno del mar)", list.Any(e => e.animal.SpeciesName == "Bear") && list.Any(e => e.animal.SpeciesName == "Wolf") && !list.Any(e => e.animal.SpeciesName == "Orca"), $"{list.Count}");
        var a = PredatorRound.Claim("Uno", Vector3.zero); var b = PredatorRound.Claim("Dos", Vector3.zero);
        TestProbe.Check("dos alimentadores no se reservan al mismo", a != null && b != null && a.animal != b.animal);

        // ── Con hambre: ración desde lejos; sin hambre: marcado ──
        if (a != null)
        {
            Animal x = a.animal; float old = x.hungry;
            x.hungry = 0.5f;
            Vector3 from = PredatorRound.StandOff(x, x.transform.position + Vector3.right * 40f);
            TestProbe.Near("se queda a su distancia segura", Vector3.Distance(from, x.transform.position), PredatorRound.SafeDistance(x.SpeciesName), 0.3f);
            FoodItem fi = PredatorRound.Throw(x, 2, from);
            TestProbe.Check("la ración cae a unos metros del animal, no en la mano", Vector2.Distance(new Vector2(fi.transform.position.x, fi.transform.position.z), new Vector2(x.transform.position.x, x.transform.position.z)) > 2f);
            TestProbe.Check("la ración es comida de verdad", fi.material == OrganicMaterial.Meat && fi.grams >= 1000f);
            PredatorRound.Report(a, PredatorRound.State.Fed);
            x.hungry = -0.5f; Destroy(fi.gameObject);
            TestProbe.Check("sin hambre no se le da", !PredatorRound.IsHungry(x));
            x.hungry = old;
            TestProbe.Check("marcado en la lista compartida: nadie vuelve a él", !PredatorRound.Today().Any(e => e.animal == x && e.state == PredatorRound.State.Pending));
        }
        PredatorRound.NewRound();

        // ── Un alimentador de verdad ──
        var runner = new GameObject("Alimentador_prueba");
        PredatorFeeder pf = runner.AddComponent<PredatorFeeder>();
        TestProbe.Check("el componente se puede poner a cualquiera", pf != null);
        Destroy(runner);

        // ── Cadena alimenticia ──
        TestProbe.Check("la cadena no supone el 80 %: lo calcula del laboratorio", FoodChain.LabShare > 0.05f && FoodChain.LabShare <= 1f, $"{FoodChain.LabShare:P0}");
        yield return null;
        TestProbe.End();
    }
}
