using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DEL SITIO NUCLEAR (Berón 2026-11-05; docs/sitio-nuclear.md):
/// 1. SITIO: en su casquete artificial, a ≥ <see cref="NuclearSiteRule.MinDistance"/> m de todo edificio-árbol y de las islas, a sotavento y
///    lejos de la lanzadera.
/// 2. PISOS: los dos reactores en el nivel más bajo; encima, el ciclotrón y la sala de experimentos; arriba, el Centro de emergencias y el garaje.
/// 3. BAJO EL MAR: el TRIGA en su cajón de doble casco apoyado en el lecho; el microrreactor y los residuos en cavernas con 60 m de roca encima, con
///    su bucle de termosifón al mar y esclusas desde el tronco.
/// 4. NADA SE PIERDE: el reactor, la consola, el ciclotrón, el equipo, las misiones de sus áreas y sus microcosmos.
/// 5. AGUA Y PLAN: el agua por gravedad cubre las 72 h de referencia; la emergencia para el reactor, abre las válvulas y enciende las balizas;
///    el simulacro no para nada; vigilancia, chimenea filtrada, sumidero y zona de exclusión están.
/// 6. TRANSPORTE: alfombras (suelo y techo), máscaras, fondo abierto y el submarino de obra hasta su garaje.
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
        TestProbe.Begin("Sitio nuclear: lejos, a sotavento, reactores abajo, contención, agua por gravedad, plan de emergencia y acceso");
        NuclearSite ns = FindAnyObjectByType<NuclearSite>();
        ArbolCampos tree = ArbolCampos.All.FirstOrDefault(a => a != null && a.building == NuclearSiteRule.Building);
        if (ns == null || tree == null) { TestProbe.Check("hay un sitio nuclear (su árbol y su casquete)", false); TestProbe.End(); yield break; }
        Transform site = ns.transform;
        float y0 = tree.transform.position.y;

        // ── 1. SITIO ──
        var others = ArbolCampos.All.Where(a => a != null && a != tree).ToList();
        ArbolCampos near = others.OrderBy(a => Flat(a.transform.position, tree.transform.position)).First();
        float nearest = Flat(near.transform.position, tree.transform.position), nearestIce = 9999f;
        GameObject floes = GameObject.Find("Banquisa_Casquetes");
        if (floes != null)
            foreach (Transform b in floes.transform)
            {
                if (b.name != "Casquete") continue;
                Vector3 p = b.position, s = b.localScale, c = tree.transform.position;
                float dx = Mathf.Max(0f, Mathf.Abs(c.x - p.x) - s.x * 0.5f), dz = Mathf.Max(0f, Mathf.Abs(c.z - p.z) - s.z * 0.5f);
                nearestIce = Mathf.Min(nearestIce, Mathf.Sqrt(dx * dx + dz * dz));
            }
        TestProbe.Check($"en su casquete artificial, a ≥ {NuclearSiteRule.MinDistance:0} m de todo árbol y de las islas", Named(site, "Casquete_artificial") != null && nearest >= NuclearSiteRule.MinDistance && nearestIce >= NuclearSiteRule.MinDistance,
            $"a {nearest:0} m de {near.building}; al hielo, {nearestIce:0} m");
        TestProbe.Check("a sotavento del santuario", NuclearSiteRule.DownwindCosine(tree.transform.position) >= 0.7f, $"coseno {NuclearSiteRule.DownwindCosine(tree.transform.position):0.00}");
        ArbolCampos launch = others.FirstOrDefault(a => a.building == LaunchHeightRule.Building);
        TestProbe.Check("lejos de la lanzadera (fuera de su distancia de seguridad)", launch == null || Flat(launch.transform.position, tree.transform.position) >= LaunchHeightRule.MinDistance, launch != null ? $"{Flat(launch.transform.position, tree.transform.position):0} m" : "sin lanzadera");

        // ── 2. PISOS ──
        Transform central = GameObject.Find("NuclearPower_Area")?.transform, plant = GameObject.Find("AtomicPlant_Area")?.transform;
        Transform cyc = plant != null ? plant.Find("Planta_ciclotron") : null, hall = plant != null ? plant.Find("Sala_experimentos_fisica") : null;
        Transform em = Named(tree.transform, "Centro_emergencias"), gar = Named(tree.transform, "Garaje_nuclear");
        bool all = central != null && plant != null && cyc != null && hall != null && em != null && gar != null;
        TestProbe.Check("los pisos del sitio: Central, Planta, ciclotrón, sala de experimentos, Centro de emergencias y garaje", all, $"central {central != null}, planta {plant != null}, ciclotrón {cyc != null}, sala {hall != null}, emergencias {em != null}, garaje {gar != null}");
        if (all)
        {
            float yc = central.position.y - y0, yp = plant.position.y - y0, ym = Mathf.Min(cyc.position.y, hall.position.y) - y0, ye = em.position.y - y0;
            TestProbe.Check("los dos reactores abajo, en el primer nivel", Mathf.Abs(yc - NuclearSiteRule.FirstFloorY) < 0.2f && Mathf.Abs(yp - NuclearSiteRule.FirstFloorY) < 0.2f, $"Central a {yc:0} m, Planta a {yp:0} m");
            TestProbe.Check("las salas de mando y lo demás, por encima de los reactores", ym > yp + 5f && ye > ym + 5f, $"ciclotrón y sala desde {ym:0} m, emergencias a {ye:0} m");
        }
        var floorLabels = tree.GetComponentsInChildren<SanctuaryArea>(true).Select(a => a.displayName).Distinct().ToList();
        TestProbe.Check("sus áreas en el árbol del sitio", floorLabels.Contains(NuclearSite.PlantArea) && floorLabels.Contains(NuclearSite.PowerArea), string.Join(", ", floorLabels));

        // ── 3. CONTENCIÓN ──
        Transform cj = Named(site, "Cajon_TRIGA"), cm = Named(site, "Caverna_microrreactor"), core = plant != null ? Named(plant, "Nucleo_UZrH") : null, mod = cm != null ? Named(cm, "Modulo_reactor") : null;
        if (cj == null || cm == null || core == null || mod == null) TestProbe.Check("el cajón del TRIGA y la caverna del microrreactor", false, $"cajón {cj != null}, caverna {cm != null}, núcleo {core != null}, módulo {mod != null}");
        else
        {
            float capBottom = NuclearSiteRule.CaissonTopY;
            bool Inside(Transform box, Vector3 p, Vector3 half) { Vector3 d = p - box.position; return Mathf.Abs(d.x) < half.x && Mathf.Abs(d.z) < half.z && Mathf.Abs(d.y) < half.y; }
            TestProbe.Check("el núcleo del TRIGA, bajo el nivel del mar y dentro de su cajón", core.position.y < y0 + capBottom && Inside(cj, core.position, new Vector3(7f, 8f, 7f)), $"núcleo a {core.position.y - y0:0.0} m");
            float rock = NuclearSiteRule.SeabedY - (cm.position.y + NuclearSiteRule.CavernHeight * 0.5f - y0);
            TestProbe.Check($"el microrreactor, en su caverna, con ≥ {NuclearSiteRule.RockCover:0} m de roca encima", rock >= NuclearSiteRule.RockCover - 0.1f && Inside(cm, mod.position, new Vector3(10f, 5f, 6f)), $"{rock:0} m de roca; módulo a {mod.position.y - y0:0.0} m");
            var cjn = cj.GetComponentsInChildren<Transform>(true).Select(t => t.name).ToList();
            TestProbe.Check("el cajón del TRIGA: doble casco, sobre su zapata en el lecho y sin colisiones", cjn.Count(n => n == "Casco_exterior") == 6 && cjn.Count(n => n == "Casco_interior") == 6 && cjn.Contains("Zapata_lecho") && cj.GetComponentsInChildren<Collider>(true).Length == 0 && Mathf.Abs(NuclearSiteRule.CaissonTopY - NuclearSiteRule.CaissonHeight - NuclearSiteRule.SeabedY) < 0.5f);
            string[] rockParts = { "Lecho_marino", "Caverna_residuos", "Galeria_cavernas", "Bucle_termosifon", "Intercambiador_lecho", "Sumidero_caverna" };
            var noRock = rockParts.Where(p => Named(site, p) == null).ToList();
            TestProbe.Check("el lecho a la vista, la caverna de residuos, el bucle de termosifón al mar y el sumidero", noRock.Count == 0, string.Join(", ", noRock));
            Transform loop = Named(site, "Bucle_termosifon");
            TestProbe.Check("el reactor queda más abajo que su sumidero de calor: el agua sube sola", loop != null && mod.position.y < Named(site, "Intercambiador_lecho").position.y);
            int locks = site.GetComponentsInChildren<Transform>(true).Count(t => t.name == "Esclusa_tronco");
            TestProbe.Check("se llega al cajón y al pozo de la caverna desde el tronco (esclusas)", locks == 2, $"{locks} esclusas");
            TestProbe.Check("los cajones no estorban el paso del submarino (fuera del hueco del tronco)", Flat(cj.position, tree.transform.position) > tree.bottomHole + 7f );
        }

        // ── 4. NADA SE PIERDE ──
        ResearchReactor rr = ResearchReactor.Instance;
        TestProbe.Check("el reactor, la consola, el ciclotrón y el equipo siguen en la Planta", rr != null && plant != null && rr.transform.IsChildOf(plant) && plant.GetComponentInChildren<ReactorConsole>(true) != null && plant.GetComponentInChildren<Cyclotron>(true) != null && plant.GetComponentsInChildren<PlantEquipment>(true).Length >= 5);
        string[] ids = { "pla_operar", "pla_isotopos", "pla_ronda", "pla_ciclotron", "fis_experimentos", "nuc_turno" };
        var lost = ids.Where(i => AreaFunctions.Get(i) == null).ToList();
        TestProbe.Check("sus misiones siguen en el registro", lost.Count == 0, string.Join(", ", lost));
        var missions = AreaWork.Today(7, ConstructionProject.All);
        TestProbe.Check("la jornada del sitio: vigilancia, revisión de lo pasivo y el simulacro semanal", new[] { "nuc_vigilancia", "nuc_pasivos", "nuc_simulacro" }.All(i => missions.Any(m => m.id == i)));
        TestProbe.Check("sus microcosmos siguen apuntando a sus áreas", MicroFloors.Table.Any(m => m.platform == "AtomicPlant_Area") && MicroFloors.Table.Any(m => m.platform == "NuclearPower_Area"));

        // ── 5. AGUA Y PLAN ──
        float hours = NuclearSiteRule.PassiveHours(ns.gravityWaterM3);
        TestProbe.Check($"el agua del tronco cubre las {NuclearSiteRule.ReferenceHours:0} h de referencia, por gravedad", ns.gravityWaterM3 > 1000f && hours >= NuclearSiteRule.ReferenceHours, $"{ns.gravityWaterM3:0} m³, {hours:0} h (cota)");
        string[] parts = { "Linea_pasiva_tronco", "Intercambiador_intermedio", "Toma_agua_mar", "Pozo_microrreactor", "Sonda_agua_mar", "Chimenea_filtrada", "Filtros_HEPA_carbon", "Boya_exclusion", "Boya_linea_submarina" };
        var miss = parts.Where(p => Named(site, p) == null).ToList();
        TestProbe.Check("líneas pasivas, sumidero con circuito intermedio, chimenea filtrada, zona de exclusión y línea al santuario", miss.Count == 0, miss.Count == 0 ? $"{parts.Length} piezas" : "faltan: " + string.Join(", ", miss));
        TestProbe.Check("8 estaciones de vigilancia y 2 líneas de válvulas", ns.monitors.Count == 8 && ns.valves.Count == 2 && ns.beacons.Count == 8 && ns.seaProbes.Count == 4, $"{ns.monitors.Count} estaciones, {ns.valves.Count} válvulas, {ns.seaProbes.Count} sondas");
        int d0 = ns.Drills;
        bool wasScrammed = rr == null || rr.scrammed;
        ns.Drill("prueba");
        TestProbe.Check("el simulacro prueba el plan sin parar nada y lo deja como estaba", ns.Drills == d0 + 1 && ns.State == NuclearSite.Alert.Normal && !ns.ValvesOpen && (rr == null || rr.scrammed == wasScrammed));
        // EVACUACIÓN: alguien del personal en la Central; la emergencia lo tiene que sacar de la zona, sin que nadie más lo mueva.
        WorkdayDirector wd = WorkdayDirector.Instance;
        GameObject evac = wd != null ? wd.roster.workers.Select(w => WorkdayDirector.Body(w.name)).FirstOrDefault(g => g != null && !g.CompareTag("Player") && !RestaurantService.Drives(g) && g.GetComponent<Anima>() != null) : null;
        Vector3 back = evac != null ? evac.transform.position : Vector3.zero;
        if (evac != null && central != null) WorkdayDirector.Teleport(evac.transform, WorkdayDirector.AreaPoint(NuclearSite.PowerArea, evac.name));
        ns.DeclareEmergency("prueba");
        TestProbe.Check("EMERGENCIA: reactor parado, válvulas abiertas y balizas encendidas, sin nadie", ns.State == NuclearSite.Alert.Emergencia && ns.ValvesOpen && (rr == null || rr.scrammed) && ns.beacons.All(b => b.activeSelf));
        if (evac == null) TestProbe.Check("hay alguien del personal para evacuar", false);
        else
        {
            bool started = ns.EvacueesLeft > 0;
            float oldTs = Time.timeScale; Time.timeScale = 4f;
            float te = Time.time, re = Time.realtimeSinceStartup;
            while (NuclearSiteRule.InZone(evac.transform.position) && Time.time - te < 300f && Time.realtimeSinceStartup - re < 120f) yield return null;
            Time.timeScale = oldTs;
            TestProbe.Check("EVACUACIÓN: la emergencia saca de la zona a quien estaba en la Central", started && !NuclearSiteRule.InZone(evac.transform.position), $"«{evac.name}»: {ns.EvacueesLeft} en la zona al último recuento; {Time.time - te:0} s de juego; a {Flat(evac.transform.position, NuclearSiteRule.Center):0} m del sitio");
        }
        ns.ClearEmergency();
        if (evac != null) WorkdayDirector.Teleport(evac.transform, back);
        // TIEMPO LIBRE: el ocio no abre ningún área del sitio.
        TestProbe.Check("el tiempo libre no va al sitio nuclear", LeisureDirector.OpenArea(NuclearSite.PlantArea, tree.gameObject.scene) == null && LeisureDirector.OpenArea(NuclearSite.PowerArea, tree.gameObject.scene) == null);
        TestProbe.Check("fin de la emergencia: todo vuelve a normal", ns.State == NuclearSite.Alert.Normal && !ns.ValvesOpen && ns.beacons.All(b => !b.activeSelf));

        // ── 6. TRANSPORTE ──
        var pads = FindObjectsByType<CarpetPad>(FindObjectsSortMode.None).Where(p => p != null && p.building == NuclearSiteRule.Building).ToList();
        TestProbe.Check("se llega en alfombra (suelo del casquete y techo; sin escaleras)", pads.Count >= 2, $"{pads.Count} zonas");
        TestProbe.Check("alfombra del santuario en su techo", FindObjectsByType<CarpetPad>(FindObjectsSortMode.None).Any(p => p != null && p.kind == CarpetPad.Kind.Santuario && Flat(p.transform.position, tree.transform.position) < 40f));
        TestProbe.Check("fondo abierto y máscaras, como los demás árboles", tree.bottomHole > 0f && tree.GetComponentsInChildren<MaskStation>(true).Length > 0, $"hueco {tree.bottomHole:0.0} m, {tree.GetComponentsInChildren<MaskStation>(true).Length} puestos de máscaras");
        ConstructionSubmarine sub = ConstructionSubmarine.Instance;
        TreeLevel garage = tree.levels.FirstOrDefault(l => l.deck != null && l.deck.name == "Garaje_nuclear");
        if (sub == null || garage == null) TestProbe.Check("el submarino de obra llega a su garaje", false, $"submarino {sub != null}, garaje {garage != null}");
        else
        {
            float old = Time.timeScale; Time.timeScale = 4f;
            { float tw = Time.time, rw = Time.realtimeSinceStartup; while (sub.Now != ConstructionSubmarine.State.Parked && Time.time - tw < 900f && Time.realtimeSinceStartup - rw < 300f) yield return null; }
            int dl = sub.Deliveries;
            bool sent = sub.SendTo(tree, garage, "material del sitio");
            float t0 = Time.time, rr0 = Time.realtimeSinceStartup;
            while (sent && sub.Deliveries == dl && Time.time - t0 < 900f && Time.realtimeSinceStartup - rr0 < 300f) yield return null;
            TreeLevel at = tree.LevelAt(sub.transform.position + Vector3.up * 0.5f);
            TestProbe.Check("el submarino de obra llega por la vía marina (fondo abierto) a su garaje y aparca", sent && sub.Deliveries > dl && at == garage, $"en {Time.time - t0:0} s de juego; en {at?.name ?? "ningún nivel"}");
            float t1 = Time.time, r1 = Time.realtimeSinceStartup;
            while (sub.Now != ConstructionSubmarine.State.Parked && Time.time - t1 < 900f && Time.realtimeSinceStartup - r1 < 300f) yield return null;
            Time.timeScale = old;
        }

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
