using System.Collections;
using UnityEngine;

/// <summary>
/// TEST DE LA INICIACIÓN DE LA COCINA (Berón 2026-10-28): tras el río, el trabajador de turno de la cocina obliga al recién
/// llegado a cocinar con él, POR MAYOR, el platillo con MENOS stock; al acabar lo lleva al tanque y entra al primer
/// microcosmos de la cocina. Comprueba: (1) se elige el platillo con menos stock; (2) por mayor las cantidades son mayores
/// que en una ración individual (y el preset individual no cambia); (3) el stock sube con todas las raciones de la tanda;
/// (4) el jugador acaba en el microcosmos de la cocina y queda formado en cocina. El jugador se emula como en
/// <see cref="ExperienciaJugadorTest"/>: lanza el paso que se le pide (hechizo `step` = <see cref="CookingStation.AdvanceStep"/>).
/// Se añade solo al orquestador cuando se piden las pruebas (no hace falta tocar el constructor de la escena).
/// </summary>
public class KitchenInitiationTest : MonoBehaviour, ITestUnit
{
    public int Group => 41;   // tras la experiencia del jugador (40): el jugador ya está iniciado en la enfermería
    public bool ParallelSafe => false;
    public float speed = 6f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        // (AfterSceneLoad va tras los Awake y antes de los Start: el TestRunner aún no ha reunido sus unidades.)
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();   // solo está activo si se pidieron las pruebas
        if (tr != null && tr.GetComponentInChildren<KitchenInitiationTest>() == null) tr.gameObject.AddComponent<KitchenInitiationTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Iniciación de la cocina: platillo con menos stock, por mayor → tanque → microcosmos");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        SanctuaryOnboarding ob = player != null ? player.GetComponent<SanctuaryOnboarding>() : null;
        RestaurantKitchen rk = FindAnyObjectByType<RestaurantKitchen>();
        if (player == null || ob == null || rk == null || rk.dishes == null || rk.dishes.Length < 2)
        { TestProbe.Check("hay jugador recién llegado y restaurante con platillos", false); TestProbe.End(); yield break; }

        // La Magnate y el jefe de cocina, quietos: el test hace él la entrega (como la Magnate al volver del río).
        Magnate[] mags = FindObjectsByType<Magnate>(FindObjectsSortMode.None);
        KitchenBoss[] bosses = FindObjectsByType<KitchenBoss>(FindObjectsSortMode.None);
        foreach (Magnate m in mags) m.enabled = false;
        foreach (KitchenBoss b in bosses) b.enabled = false;
        if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld) { MobWorldLoader.Instance.ExitMobWorld(); yield return Until(() => !MobWorldLoader.Instance.IsInMobWorld && !MobWorldLoader.Instance.IsBusy, 20f); }

        // ── 1. El platillo con MENOS stock ──
        int[] saved = new int[rk.dishes.Length];
        for (int i = 0; i < rk.dishes.Length; i++) { saved[i] = rk.dishes[i].Have; if (rk.dishes[i].container != null) rk.dishes[i].container.rations = 30; }   // (lejos: los cocineros de la cocina no lo alcanzan durante el test)
        int low = rk.dishes.Length / 2;
        if (rk.dishes[low].container != null) rk.dishes[low].container.rations = 0;
        RestaurantKitchen.DishStock lowest = rk.LowestStock();
        string want = rk.dishes[low].recipe != null ? rk.dishes[low].recipe.dishName : "?";
        TestProbe.Check("el restaurante señala el platillo con menos stock", lowest == rk.dishes[low], lowest?.recipe?.dishName ?? "nada");

        // ── 2. Por mayor vs individual ──
        int n = AreaInitiator.StaffServings();
        CookingRecipe single = CookingRecipe.HuevosRevueltos(), bulk = single.ForServings(n);
        string ing = single.Ingredients().Count > 0 ? single.Ingredients()[0] : "huevos";
        TestProbe.Check("por mayor salen varias raciones de una tanda; individual, una", bulk.servings == n && n > 1 && single.servings == 1 && bulk.IsBulk && !single.IsBulk, $"×{n}");
        TestProbe.Check("por mayor la cantidad de cada ingrediente es mayor que la individual", bulk.Amount(ing) > single.Amount(ing), $"{ing}: {single.Amount(ing):0.##} → {bulk.Amount(ing):0.##}");
        TestProbe.Check("la receta individual (turistas) no cambia al hacer la de por mayor", CookingRecipe.HuevosRevueltos().servings == 1 && single.steps.Length == bulk.steps.Length);

        // ── 3. La iniciación: el de turno, la tanda y el stock ──
        foreach (AreaInitiator x in FindObjectsByType<AreaInitiator>(FindObjectsSortMode.None)) Destroy(x);
        foreach (NewcomerInitiator x in FindObjectsByType<NewcomerInitiator>(FindObjectsSortMode.None)) x.ResetState();   // (jugadas sueltas: el examen de llegada del arranque no se cruza)
        ob.apt = true;
        if (player.TryGetComponent(out HealthRecord hrec)) { hrec.analyzed = true; hrec.vaccinated = true; }   // (ya pasó el examen de llegada: la puerta de la enfermería no lo retiene)
        yield return null;
        ob.apt = ob.presented = ob.initiated = true; ob.kitchenTrained = false;
        AreaInitiator.AllowDuringTests = true;
        float oldScale = Time.timeScale; Time.timeScale = speed;
        GameObject spot = GameObject.Find("CocinaSpot");
        if (spot != null) WorkdayDirector.Teleport(player.transform, spot.transform.position + Vector3.up * 0.5f);
        int before = rk.dishes[low].Have;
        AreaInitiator ai = AreaInitiator.Begin(AreaInitiator.Kind.Kitchen, ob, mags.Length > 0 ? mags[0].GetComponent<Anima>() : null);
        TestProbe.Check("un trabajador de la cocina empieza la iniciación", ai != null, ai != null ? ai.Who : "nadie");
        if (ai == null) { Restore(oldScale, mags, bosses); TestProbe.End(); yield break; }

        // El jugador emulado: cuando se le pide un paso, lo lanza (como escribir `step`). Uno se lo deja pasar (lo hace el de turno).
        // Hasta 2 m andando; desde ahí, la ORQUESTA (cada uno a su sitio): se mira que no se pisen, no salten ni den vueltas.
        yield return Until(() => ai == null || (ai.Now != AreaInitiator.Phase.None && ai.Now != AreaInitiator.Phase.Approach), 400f);   // (None: aún no arrancó)
        Orchestra.ResetStats();
        var watch = new ExperienciaJugadorTest.OrchestraWatch(player.transform, ai != null ? ai.transform : player.transform);
        int cast = 0; bool skipped = false;
        float limit = Time.time + 400f;
        while (AreaInitiator.Owns(ob) && ai != null && ai.Now != AreaInitiator.Phase.ToTank && Time.time < limit)
        {
            watch.Sample(true);
            if (ai.WaitingForStudent)
            {
                CookingStation cs = player.GetComponent<CookingStation>();
                if (!skipped && cast == 1) { skipped = true; yield return Until(() => ai == null || !ai.WaitingForStudent || (cs != null && cs.StepIndex >= 2), ai.stepTimeout + 5f); }
                else if (cs != null) { cs.AdvanceStep(); cast++; }
                for (int k = 0; k < 3; k++) yield return null;
            }
            yield return null;
        }
        { float end = Time.time + 120f; while (AreaInitiator.Owns(ob) && Time.time < end) { watch.Sample(true); yield return null; } }
        AreaInitiator.Report r = AreaInitiator.Last;
        TestProbe.Check("cocinan el platillo con menos stock", r != null && r.dish == want, r != null ? r.dish : "sin informe");
        TestProbe.Check("el jugador lanza los pasos que le piden y el de turno ayuda si no llega", r != null && r.stepsByStudent > 0 && r.stepsHelped >= 1 && r.cooked, r != null ? $"{r.stepsByStudent} del jugador, {r.stepsHelped} ayudados" : "");
        int after = rk.dishes[low].Have;
        TestProbe.Check("el stock sube con TODAS las raciones de la tanda por mayor", after >= before + n, $"{before} → {after} (×{n})");
        TestProbe.Check("queda formado en cocina", ob.kitchenTrained);
        ExperienciaJugadorTest.CheckOrchestra("cocina", watch);

        // ── 4. Al tanque y al primer microcosmos de la cocina ──
        bool loadable = Application.CanStreamedLevelBeLoaded(AreaInitiator.KitchenMicrocosmos);
        if (!loadable) TestProbe.Check("la escena del microcosmos de la cocina está en Build Settings (Build Microcosmos por área)", false, AreaInitiator.KitchenMicrocosmos);
        else
        {
            yield return Until(() => MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld && !MobWorldLoader.Instance.IsBusy, 90f);
            string sc = MobWorldLoader.HasInstance ? MobWorldLoader.Instance.ActiveScene : null;
            TestProbe.Check("el de turno lo mete en el tanque y aparece en el microcosmos de la cocina", sc == AreaInitiator.KitchenMicrocosmos, sc ?? "fuera");
            TestProbe.Check("es la era del fuego: Nasatya y la Tirana de las Feromonas están", GameObject.Find("Nasatya") != null && GameObject.Find("La Tirana de las Feromonas") != null);
            if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld) { MobWorldLoader.Instance.ExitMobWorld(); yield return Until(() => !MobWorldLoader.Instance.IsInMobWorld && !MobWorldLoader.Instance.IsBusy, 30f); }
        }

        for (int i = 0; i < rk.dishes.Length; i++) if (rk.dishes[i].container != null) rk.dishes[i].container.rations = saved[i];
        Restore(oldScale, mags, bosses);
        TestProbe.End();
    }

    static void Restore(float scale, Magnate[] mags, KitchenBoss[] bosses)
    {
        Time.timeScale = scale; AreaInitiator.AllowDuringTests = false;
        foreach (Magnate m in mags) if (m != null) m.enabled = true;
        foreach (KitchenBoss b in bosses) if (b != null) b.enabled = true;
    }

    static IEnumerator Until(System.Func<bool> cond, float seconds)
    {
        float end = Time.time + seconds;
        while (!cond() && Time.time < end) yield return null;
    }
}
