using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DEL SITIO NUCLEAR (Berón 2026-11-05; docs/sitio-nuclear.md). Desde el 2026-11-08 (§25) la Central Nuclear y la Planta Atómica están enteras en el
/// Santuario Marino; en el santuario 1 queda su ESTACIÓN (lo detallado de arriba y de abajo lo prueba el grupo 114, NuclearSubmarinaTest):
/// 1. SITIO: la estación, en su casquete artificial, a ≥ <see cref="NuclearSiteRule.MinDistance"/> m de todo edificio-árbol y de las islas, a sotavento y
///    lejos de la lanzadera.
/// 2. ABAJO: ni el árbol del sitio ni sus áreas en el santuario 1; nada hondo aquí.
/// 4. NADA SE PIERDE: las misiones de sus áreas, la jornada del sitio y sus microcosmos.
/// 5. AGUA Y PLAN: el agua pasiva (el mar) cubre las 72 h de referencia; la emergencia para el reactor, abre las válvulas y enciende las balizas;
///    el simulacro no para nada; vigilancia, chimenea filtrada, zona de exclusión, radar y sonar están; la evacuación saca a quien trabajaba allí.
/// 6. TRANSPORTE: en alfombra a la estación (red del santuario) y de ahí por la Línea.
/// 7. REGLA T13: la chimenea cuenta como elemento elevado y la lanzadera sigue cumpliendo.
/// </summary>
public class NuclearSiteTest : MonoBehaviour, ITestUnit
{
    public int Group => 94;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<NuclearSiteTest>() == null) tr.gameObject.AddComponent<NuclearSiteTest>();
    }

    static Transform Named(Transform root, string name) => root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
    static float Flat(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));

    public IEnumerator Run()
    {
        TestProbe.Begin("Sitio nuclear: lejos, a sotavento, la planta bajo el mar, agua pasiva, plan de emergencia y acceso por la estación");
        NuclearSite ns = FindAnyObjectByType<NuclearSite>();
        if (ns == null) { TestProbe.Check("hay un sitio nuclear (su estación y su lógica)", false); TestProbe.End(); yield break; }
        Transform site = ns.transform;
        Transform station = Named(site, NuclearSiteRule.StationName);
        Vector3 c = station != null ? station.position : NuclearSiteRule.Center;

        // ── 1. SITIO ──
        var trees = ArbolCampos.All.Where(a => a != null && a.gameObject.scene == ns.gameObject.scene).ToList();
        ArbolCampos near = trees.OrderBy(a => Flat(a.transform.position, c)).FirstOrDefault();
        float nearest = near != null ? Flat(near.transform.position, c) : 9999f, nearestIce = 9999f;
        GameObject floes = GameObject.Find("Banquisa_Casquetes");
        if (floes != null)
            foreach (Transform b in floes.transform)
            {
                if (b.name != "Casquete" && !b.name.StartsWith("Tierra")) continue;
                Vector3 p = b.position, s = b.localScale;
                float dx = Mathf.Max(0f, Mathf.Abs(c.x - p.x) - s.x * 0.5f), dz = Mathf.Max(0f, Mathf.Abs(c.z - p.z) - s.z * 0.5f);
                nearestIce = Mathf.Min(nearestIce, Mathf.Sqrt(dx * dx + dz * dz));
            }
        TestProbe.Check($"la estación, en su casquete artificial, a ≥ {NuclearSiteRule.MinDistance:0} m de todo árbol y de las islas", station != null && Named(site, "Casquete_artificial") != null && nearest >= NuclearSiteRule.MinDistance && nearestIce >= NuclearSiteRule.MinDistance,
            $"a {nearest:0} m de {(near != null ? near.building : "—")}; al hielo, {nearestIce:0} m");
        TestProbe.Check("a sotavento del santuario", NuclearSiteRule.DownwindCosine(c) >= 0.7f, $"coseno {NuclearSiteRule.DownwindCosine(c):0.00}");
        ArbolCampos launch = ArbolCampos.All.FirstOrDefault(a => a != null && a.building == LaunchHeightRule.Building);
        TestProbe.Check("lejos de la lanzadera (fuera de su distancia de seguridad)", launch == null || Flat(launch.transform.position, c) >= LaunchHeightRule.MinDistance, launch != null ? $"{Flat(launch.transform.position, c):0} m" : "sin lanzadera (está en el Santuario Aéreo)");

        // ── 2. LA PLANTA, ABAJO (docs §25; lo detallado, grupo 114) ──
        TestProbe.Check("ni el árbol del sitio ni la Central ni la Planta en el santuario 1: están en el Santuario Marino",
            !trees.Any(a => a.building == NuclearSiteRule.Building) && !FindObjectsByType<SanctuaryArea>(FindObjectsInactive.Include, FindObjectsSortMode.None).Any(a => a.gameObject.scene == ns.gameObject.scene && NuclearSite.Underwater(a.displayName)));
        int capTendons = site.GetComponentsInChildren<Transform>(true).Count(t => t.name == "Tendon_casquete");
        TestProbe.Check("el casquete de la estación, atado al fondo somero", capTendons == 4, $"{capTendons} patas");
        TestProbe.Check("sin cajones ni salas en esta escena", Named(site, "Cajon_TRIGA") == null && Named(site, "Caverna_microrreactor") == null);

        // ── 4. NADA SE PIERDE ──
        string[] ids = { "pla_operar", "pla_isotopos", "pla_ronda", "pla_ciclotron", "fis_experimentos", "nuc_turno" };
        var lost = ids.Where(i => AreaFunctions.Get(i) == null).ToList();
        TestProbe.Check("sus misiones siguen en el registro", lost.Count == 0, string.Join(", ", lost));
        var missions = AreaWork.Today(7, ConstructionProject.All);
        TestProbe.Check("la jornada del sitio: vigilancia, revisión de lo pasivo y el simulacro semanal", new[] { "nuc_vigilancia", "nuc_pasivos", "nuc_simulacro" }.All(i => missions.Any(m => m.id == i)));
        TestProbe.Check("sus microcosmos siguen apuntando a sus áreas", MicroFloors.Table.Any(m => m.platform == "AtomicPlant_Area") && MicroFloors.Table.Any(m => m.platform == "NuclearPower_Area"));

        // ── 5. AGUA Y PLAN ──
        float hours = NuclearSiteRule.PassiveHours(ns.gravityWaterM3);
        TestProbe.Check($"el agua pasiva (el mar, por la toma de cada sala) cubre las {NuclearSiteRule.ReferenceHours:0} h de referencia", ns.gravityWaterM3 > 1000f && hours >= NuclearSiteRule.ReferenceHours, $"{ns.gravityWaterM3:0} m³, {hours:0} h (cota)");
        string[] parts = { "Valvula_pasiva", "Sonda_agua_mar", "Chimenea_filtrada", "Filtros_HEPA_carbon", "Boya_exclusion", "Boya_linea_submarina", "Radar_hielo", "Sonar_quillas" };
        var miss = parts.Where(p => Named(site, p) == null).ToList();
        TestProbe.Check("válvulas de la toma pasiva, sondas, chimenea filtrada de la ventilación, zona de exclusión, línea al santuario, radar y sonar", miss.Count == 0, miss.Count == 0 ? $"{parts.Length} piezas" : "faltan: " + string.Join(", ", miss));
        TestProbe.Check("8 estaciones de vigilancia y 2 válvulas", ns.monitors.Count == 8 && ns.valves.Count == 2 && ns.beacons.Count == 8 && ns.seaProbes.Count == 4, $"{ns.monitors.Count} estaciones, {ns.valves.Count} válvulas, {ns.seaProbes.Count} sondas");
        ResearchReactor rr = ResearchReactor.Instance;   // (abajo: solo existe con el Santuario Marino cargado)
        int d0 = ns.Drills;
        bool wasScrammed = rr == null || rr.scrammed;
        ns.Drill("prueba");
        TestProbe.Check("el simulacro prueba el plan sin parar nada y lo deja como estaba", ns.Drills == d0 + 1 && ns.State == NuclearSite.Alert.Normal && !ns.ValvesOpen && (rr == null || rr.scrammed == wasScrammed));
        // EVACUACIÓN: alguien del personal en la estación (desde donde se trabaja la planta); la emergencia lo tiene que sacar de la zona.
        WorkdayDirector wd = WorkdayDirector.Instance;
        GameObject evac = wd != null ? wd.roster.workers.Select(w => WorkdayDirector.Body(w.name)).FirstOrDefault(g => g != null && !g.CompareTag("Player") && !RestaurantService.Drives(g) && g.GetComponent<Anima>() != null) : null;
        Vector3 back = evac != null ? evac.transform.position : Vector3.zero;
        if (evac != null) WorkdayDirector.Teleport(evac.transform, WorkdayDirector.AreaPoint(NuclearSite.PowerArea, evac.name));
        ns.DeclareEmergency("prueba");
        TestProbe.Check("EMERGENCIA: reactor parado, válvulas abiertas y balizas encendidas, sin nadie", ns.State == NuclearSite.Alert.Emergencia && ns.ValvesOpen && (rr == null || rr.scrammed) && ns.beacons.All(b => b.activeSelf));
        if (evac == null) TestProbe.Check("hay alguien del personal para evacuar", false);
        else
        {
            bool started = ns.EvacueesLeft > 0;
            float oldTs = Time.timeScale; Time.timeScale = 4f;
            float te = Time.time, re = Time.realtimeSinceStartup;
            while ((NuclearSiteRule.InZone(evac.transform.position) || SiteLine.IsAway(evac)) && Time.time - te < 300f && Time.realtimeSinceStartup - re < 300f) yield return null;
            Time.timeScale = oldTs;
            TestProbe.Check("EVACUACIÓN: la emergencia saca de la zona a quien trabajaba en la Central", started && !NuclearSiteRule.InZone(evac.transform.position), $"«{evac.name}»: {ns.EvacueesLeft} en la zona al último recuento; {Time.time - te:0} s de juego; a {Flat(evac.transform.position, NuclearSiteRule.Center):0} m del sitio");
        }
        ns.ClearEmergency();
        if (evac != null) WorkdayDirector.Teleport(evac.transform, back);
        TestProbe.Check("el tiempo libre no va al sitio nuclear", LeisureDirector.OpenArea(NuclearSite.PlantArea, ns.gameObject.scene) == null && LeisureDirector.OpenArea(NuclearSite.PowerArea, ns.gameObject.scene) == null);
        TestProbe.Check("fin de la emergencia: todo vuelve a normal", ns.State == NuclearSite.Alert.Normal && !ns.ValvesOpen && ns.beacons.All(b => !b.activeSelf));

        // ── 6. TRANSPORTE: en alfombra a la estación (la red del santuario) y de ahí por la Línea ──
        CarpetPad pad = FindObjectsByType<CarpetPad>(FindObjectsSortMode.None).FirstOrDefault(p => p != null && p.surface == $"{NuclearSiteRule.Building}#linea");
        TestProbe.Check("se llega en alfombra a la estación (red del santuario; sin escaleras)", pad != null && pad.kind == CarpetPad.Kind.Santuario && SiteLine.Instance != null && Vector3.Distance(pad.transform.position, SiteLine.Instance.transform.position) < 12f);

        // ── 7. REGLA T13 ──
        ElevatedElement stack = site.GetComponentInChildren<ElevatedElement>(true);
        LaunchSite ls = LaunchSite.Instance;
        TestProbe.Check("la chimenea cuenta como elemento elevado y la lanzadera sigue por encima de todo", stack != null && (ls == null || ls.PadY >= stack.Top + LaunchHeightRule.Margin - 0.01f), stack != null && ls != null ? $"chimenea {stack.Top:0} m, plataforma {ls.PadY:0} m" : "");
        TestProbe.End();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pruebas/Sitio nuclear (sola)")]
    static void Solo()
    {
        if (!Application.isPlaying) { Debug.Log("[Solo] no está en Play"); return; }
        var go = new GameObject("SoloNuclear"); DontDestroyOnLoad(go);
        NuclearSiteTest t = go.AddComponent<NuclearSiteTest>();
        t.StartCoroutine(Wrap(t));
    }

    static IEnumerator Wrap(NuclearSiteTest t)
    {
        yield return t.Run();
        TestProbe.Total();
    }
#endif
}
