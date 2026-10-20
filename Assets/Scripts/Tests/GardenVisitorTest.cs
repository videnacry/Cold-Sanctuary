using System.Collections;
using UnityEngine;

/// <summary>
/// VISITANTES DEL HUERTO = ANIMAS REALES: una liebre ártica (Bunny) del santuario con hambre ELIGE SOLA el cultivo que más
/// le apetece (Forager + DietPreference) —zanahoria antes que albahaca o cebolla— y, al darle de comer, se sacia y la
/// misión la cuenta sin destruirla.
/// </summary>
public class GardenVisitorTest : MonoBehaviour, ITestUnit
{
    public int Group => 27;
    public bool ParallelSafe => false;
    [Tooltip("Prefab de la liebre ártica (Bunny) — lo asigna el builder.")]
    public GameObject harePrefab;

    public IEnumerator Run()
    {
        TestProbe.Begin("Visitantes del huerto (animas reales con hambre y dieta de cultivos)");
        yield return null;

        TestProbe.Greater("a la liebre le apetece más la zanahoria que la albahaca", DietPreference.For("Bunny", "zanahoria"), DietPreference.For("Bunny", "albahaca") * 3f);
        TestProbe.Greater("al reno le apetece más la avena que la cebolla", DietPreference.For("Deer", "avena"), DietPreference.For("Deer", "cebolla") * 5f);

        FeedVisitorMission mission = FindFirstObjectByType<FeedVisitorMission>(FindObjectsInactive.Include);
        GameObject prefab = harePrefab;
        if (prefab == null && mission != null) foreach (GameObject p in mission.visitorPrefabs) if (p != null && p.name.Contains("Bunny")) prefab = p;
        if (prefab == null) { Debug.Log("[TEST] SKIP · visitantes — no hay prefab de liebre (Bunny) en la misión del huerto."); TestProbe.End(); yield break; }

        // Un huerto de prueba: albahaca y cebolla cerca, zanahoria algo más lejos → debe ir a la zanahoria (le apetece más).
        Vector3 c = new Vector3(-6f, 0.02f, -22f);
        GameObject beds = new GameObject("HuertoPrueba_TEST");
        GardenCrop basil = GardenCrop.Build(beds.transform, GardenCrop.Kind.Albahaca, c + new Vector3(2f, 0f, 0f), 1);
        GardenCrop onion = GardenCrop.Build(beds.transform, GardenCrop.Kind.Cebolla, c + new Vector3(-2f, 0f, 0f), 2);
        GardenCrop carrot = GardenCrop.Build(beds.transform, GardenCrop.Kind.Zanahoria, c + new Vector3(0f, 0f, 5f), 3);

        GameObject hare = Instantiate(prefab, c + new Vector3(0f, 0f, -3f), Quaternion.identity);
        hare.name = "Liebre_TEST";
        VisitorAnimaMob v = hare.AddComponent<VisitorAnimaMob>();
        yield return null; yield return null; yield return null;
        Animal an = hare.GetComponent<Animal>();
        TestProbe.NotNull("la visitante es un Animal compuesto (anima)", an);
        TestProbe.NotNull("tiene Forager (decide qué comer)", an != null ? an.Forage : null);
        TestProbe.Greater("llega con hambre", an != null ? an.hungry : 0f, 0.9f);
        GameObject choice = an != null && an.Forage != null ? an.Forage.SelectTarget(an) : null;
        TestProbe.Check("por su cuenta elige la ZANAHORIA (la que más le apetece), no la albahaca ni la cebolla",
                        choice != null && choice == carrot.gameObject, choice != null ? choice.name : "nada");

        TestProbe.Check("darle de comer la sacia", v.Feed("el test") && an.hungry < 0f, $"hambre {(an != null ? an.hungry : 0f):0.0}");
        float t0 = Time.time;
        while (!v.IsDissolved && Time.time - t0 < 14f) yield return null;   // se sienta a comer (6 s) y se marcha (4 s)
        TestProbe.Check("la misión la cuenta (resuelta) y la liebre SIGUE en el santuario", v == null || v.IsDissolved ? hare != null : false);

        Destroy(hare); Destroy(beds);
        TestProbe.End();
    }
}
