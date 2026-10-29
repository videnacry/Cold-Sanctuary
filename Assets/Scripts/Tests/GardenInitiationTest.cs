using System.Collections;
using UnityEngine;

/// <summary>
/// TEST DE LA INICIACIÓN DEL HUERTO (Berón 2026-10-29): tras la cocina, la Magnate lleva al recién llegado al huerto y allí
/// el trabajador de turno lo inicia en PREPARAR EL SUELO del bancal (docs/garden-simulation.md §8: abono del compostero,
/// esparcirlo, la azada, los surcos, rastrillar), con la misma ORQUESTA que las demás iniciaciones: el trabajador va hasta
/// 2 m, cada uno anda solo a su sitio en el borde del bancal, el trabajador demuestra cada paso y el jugador lo lanza
/// (`step`; uno se lo deja pasar y lo hacen juntos al vencer el tiempo); después, los dos al tanque del huerto y su
/// microcosmos (La Sembradora). Comprueba: el suelo queda preparado, pasos del jugador y ayudados, queda formado en el
/// huerto, entra al microcosmos del huerto y la orquesta (sin solaparse, sin saltos, piernas, sin vueltas en círculo).
/// Se añade solo al orquestador cuando se piden las pruebas.
/// </summary>
public class GardenInitiationTest : MonoBehaviour, ITestUnit
{
    public int Group => 43;   // tras la cocina (41) y la sala de yoga (42)
    public bool ParallelSafe => false;
    public float speed = 6f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();   // solo está activo si se pidieron las pruebas
        if (tr != null && tr.GetComponentInChildren<GardenInitiationTest>() == null) tr.gameObject.AddComponent<GardenInitiationTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Iniciación del huerto: preparar el suelo → tanque → microcosmos del huerto");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        SanctuaryOnboarding ob = player != null ? player.GetComponent<SanctuaryOnboarding>() : null;
        Anima pa = player != null ? player.GetComponent<Anima>() : null;
        GameObject area = GameObject.Find("Garden_Area");
        if (player == null || ob == null || pa == null || area == null) { TestProbe.Check("hay jugador recién llegado y huerto", false); TestProbe.End(); yield break; }

        Magnate[] mags = FindObjectsByType<Magnate>(FindObjectsSortMode.None);
        KitchenBoss[] bosses = FindObjectsByType<KitchenBoss>(FindObjectsSortMode.None);
        foreach (Magnate m in mags) m.enabled = false;
        foreach (KitchenBoss b in bosses) b.enabled = false;
        if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld) { MobWorldLoader.Instance.ExitMobWorld(); yield return Until(() => !MobWorldLoader.Instance.IsInMobWorld && !MobWorldLoader.Instance.IsBusy, 20f); }
        foreach (AreaInitiator x in FindObjectsByType<AreaInitiator>(FindObjectsSortMode.None)) Destroy(x);
        foreach (NewcomerInitiator x in FindObjectsByType<NewcomerInitiator>(FindObjectsSortMode.None)) x.ResetState();   // (jugadas sueltas: el examen de llegada del arranque no se cruza)
        ob.apt = true;
        if (player.TryGetComponent(out HealthRecord hrec)) { hrec.analyzed = true; hrec.vaccinated = true; }   // (ya pasó el examen de llegada: la puerta de la enfermería no lo retiene)
        yield return null;

        ob.apt = ob.presented = ob.initiated = ob.kitchenTrained = true; ob.gardenTrained = false;
        AreaInitiator.AllowDuringTests = true;
        float oldScale = Time.timeScale; Time.timeScale = speed;
        // La Magnate lo deja en el huerto (a unos metros del centro del área).
        Vector3 start = area.transform.position;
        foreach (Vector3 off in new[] { area.transform.right * 4f, -area.transform.right * 4f, area.transform.forward * 4f, -area.transform.forward * 4f, Vector3.zero })   // en su piso, no en el hielo de abajo
            if (UnityEngine.AI.NavMesh.SamplePosition(area.transform.position + off, out UnityEngine.AI.NavMeshHit h, 1.5f, UnityEngine.AI.NavMesh.AllAreas) && Mathf.Abs(h.position.y - area.transform.position.y) < 1.5f) { start = h.position; break; }
        WorkdayDirector.Teleport(player.transform, PathMover.Of(player.transform).OnGround(start));
        AreaInitiator ai = AreaInitiator.Begin(AreaInitiator.Kind.Garden, ob, mags.Length > 0 ? mags[0].GetComponent<Anima>() : null);
        TestProbe.Check("un trabajador del huerto empieza la iniciación", ai != null, ai != null ? ai.Who : "nadie");
        if (ai == null) { Restore(oldScale, mags, bosses); TestProbe.End(); yield break; }

        yield return Until(() => ai == null || (ai.Now != AreaInitiator.Phase.None && ai.Now != AreaInitiator.Phase.Approach), 400f);   // (None: aún no arrancó)
        float startGap = ai != null ? Flat(ai.transform.position - player.transform.position) : -1f;
        TestProbe.Check("el trabajador llega a 2 m del recién llegado antes de empezar", ai != null && startGap <= 2.3f, $"{startGap:0.0} m");
        Orchestra.ResetStats();
        var watch = new ExperienciaJugadorTest.OrchestraWatch(player.transform, ai != null ? ai.transform : player.transform);

        // El jugador emulado: cuando se le pide un paso, lo lanza (como escribir `step`); el segundo se lo deja pasar.
        int asked = 0; bool skipped = false;
        float limit = Time.time + 600f;
        while (ai != null && AreaInitiator.Owns(ob) && ai.Now != AreaInitiator.Phase.ToTank && Time.time < limit)
        {
            watch.Sample(true);
            if (ai.WaitingForStudent)
            {
                asked++;
                if (!skipped && asked == 2) { skipped = true; yield return Until(() => ai == null || !ai.WaitingForStudent, ai.stepTimeout + 5f); }
                else AreaInitiator.StudentStep(pa);
                for (int k = 0; k < 3; k++) yield return null;
            }
            yield return null;
        }
        { float end = Time.time + 150f; while (AreaInitiator.Owns(ob) && Time.time < end) { watch.Sample(true); yield return null; } }
        AreaInitiator.Report r = AreaInitiator.Last;
        GardenPlot plot = null;
        foreach (GardenPlot gp in GardenPlot.All) if (gp != null && r != null && gp.name == r.plot) plot = gp;
        TestProbe.Check("el suelo del bancal queda preparado (abonado, labrado, con surcos)", r != null && r.soilPrepared && plot != null && plot.SoilPrepared && plot.transform.Find("Surcos") != null, r != null ? $"«{r.plot}» {(plot != null ? plot.SoilProgress : 0f):P0}" : "sin informe");
        TestProbe.Check("el jugador lanza los pasos que le piden y el de turno ayuda si no llega", r != null && r.stepsByStudent > 0 && r.stepsHelped >= 1 && r.stepsByStudent + r.stepsHelped == AreaInitiator.SoilSteps.Length, r != null ? $"{r.stepsByStudent} del jugador, {r.stepsHelped} ayudados de {AreaInitiator.SoilSteps.Length}" : "");
        TestProbe.Check("queda formado en el huerto (y aprende «preparar-suelo»)", ob.gardenTrained && player.GetComponent<Grimoire>() != null && player.GetComponent<Grimoire>().Knows("preparar-suelo"));
        ExperienciaJugadorTest.CheckOrchestra("huerto", watch);

        // ── Al tanque y al microcosmos del huerto ──
        if (!Application.CanStreamedLevelBeLoaded(AreaInitiator.GardenMicrocosmos)) TestProbe.Check("la escena del microcosmos del huerto está en Build Settings", false, AreaInitiator.GardenMicrocosmos);
        else
        {
            yield return Until(() => MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld && !MobWorldLoader.Instance.IsBusy, 90f);
            string sc = MobWorldLoader.HasInstance ? MobWorldLoader.Instance.ActiveScene : null;
            TestProbe.Check("el de turno lo mete en el tanque y aparece en el microcosmos del huerto", sc == AreaInitiator.GardenMicrocosmos, sc ?? "fuera");
            if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld) { MobWorldLoader.Instance.ExitMobWorld(); yield return Until(() => !MobWorldLoader.Instance.IsInMobWorld && !MobWorldLoader.Instance.IsBusy, 30f); }
        }
        Restore(oldScale, mags, bosses);
        TestProbe.End();
    }

    static void Restore(float scale, Magnate[] mags, KitchenBoss[] bosses)
    {
        Time.timeScale = scale; AreaInitiator.AllowDuringTests = false;
        foreach (Magnate m in mags) if (m != null) m.enabled = true;
        foreach (KitchenBoss b in bosses) if (b != null) b.enabled = true;
    }

    static float Flat(Vector3 v) { v.y = 0f; return v.magnitude; }

    static IEnumerator Until(System.Func<bool> cond, float seconds)
    {
        float end = Time.time + seconds;
        while (!cond() && Time.time < end) yield return null;
    }
}
