using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PRUEBA DE LOS DOS SANTUARIOS Y LA LÍNEA (Berón 2026-11-05; docs/sitio-nuclear.md §7–§20):
/// 1. SANTUARIO 1: mar somero, placas finas, falda de roca corta, sin tubos hondos, submarino de obra a −12 y nado limitado por el fondo.
/// 2. LÓGICA SIN ESCENA: con el Santuario Marino descargado, la alerta de la roca, el refugio y las misiones funcionan, y un cuerpo baja y sube sin cargar nada.
/// 3. LÍNEA Y RED: estación, alfombras, jornada, el minuto, emergencia, escape, refugio y desconexión.
/// 4. ICEBERGS: detección, remolque y qué hace cada casquete.
/// 5. EL SANTUARIO MARINO: se carga aditivo, mide lo que dice la regla y se descarga dejando el santuario 1 igual.
/// </summary>
public class NuclearAccessTest : MonoBehaviour, ITestUnit
{
    public int Group => 100;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<NuclearAccessTest>() == null) tr.gameObject.AddComponent<NuclearAccessTest>();
    }

    static Transform Named(Transform root, string name) => root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
    static float Flat(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));

    // La holgura de una pieza medida en el mundo: del borde de lo que se dibuja al forro de la sala, por los lados y por arriba.
    static float Gap(Transform cav, Vector3 size, Transform piece)
    {
        Bounds b = piece.GetComponent<Renderer>().bounds;
        Vector3 inner = (size - Vector3.one * 1.2f) * 0.5f, c = cav.position;
        return Mathf.Min(inner.x - Mathf.Max(b.max.x - c.x, c.x - b.min.x), inner.z - Mathf.Max(b.max.z - c.z, c.z - b.min.z), c.y + inner.y - b.max.y);
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Dos santuarios: mar somero, lógica sin escena, línea, refugio, icebergs y Santuario Marino");
        NuclearSite ns = FindAnyObjectByType<NuclearSite>();
        CavernRock rock = CavernRock.Instance;
        SiteLine line = SiteLine.Instance;
        Refuge refuge = Refuge.Instance;
        if (ns == null || rock == null || line == null || refuge == null) { TestProbe.Check("hay sitio nuclear, roca, refugio y línea en el santuario 1", false, $"sitio {ns != null}, roca {rock != null}, refugio {refuge != null}, línea {line != null}"); TestProbe.End(); yield break; }
        Transform site = ns.transform;
        int day = TimeController.timeController != null ? TimeController.timeController.Day : 0;

        // ── 1. EL SANTUARIO 1 ──
        GameObject bed = GameObject.Find("FondoMarino"), zone = GameObject.Find("Sea_SwimZone");
        BoxCollider bedC = bed != null ? bed.GetComponent<BoxCollider>() : null, zoneC = zone != null ? zone.GetComponent<BoxCollider>() : null;
        TestProbe.Check($"mar somero de {SeaRule.Depth:0} m: el fondo y el agua nadable", bedC != null && zoneC != null && Mathf.Abs(bedC.bounds.max.y - SeaRule.FloorY) < 0.5f && Mathf.Abs(zoneC.bounds.min.y - SeaRule.FloorY) < 0.5f, zoneC != null ? $"agua hasta {zoneC.bounds.min.y:0.0} m" : "");
        Transform limit = GameObject.Find("Limite_del_mar")?.transform;
        TestProbe.Check("nadar no pasa de ahí: el fondo es sólido y el límite del mar llega a él", bedC != null && !bedC.isTrigger && (limit == null || limit.GetComponentsInChildren<Collider>(true).All(c => c.bounds.min.y <= SeaRule.FloorY)));
        GameObject floes = GameObject.Find("Banquisa_Casquetes");
        Transform block = floes != null ? floes.transform.Cast<Transform>().FirstOrDefault(t => t.name == "Casquete") : null;
        TestProbe.Check($"placas de hielo finas ({SeaRule.PlateThickness} m)", block != null && Mathf.Abs(block.localScale.y - SeaRule.PlateThickness) < 0.1f, block != null ? $"{block.localScale.y:0.0} m" : "");
        Transform skirt = floes != null ? floes.transform.Find("Zocalo_rocoso") : null;
        TestProbe.Check("la roca de la isla es una falda corta, por encima del submarino", skirt == null || skirt.GetComponent<Renderer>().bounds.min.y > -10f);
        ConstructionSubmarine csub = ConstructionSubmarine.Instance;
        TestProbe.Check("el submarino de obra vuelve a −12 m", csub == null || Mathf.Abs(csub.seaDepth + 12f) < 0.1f);
        TestProbe.Check("sin nada hondo en esta escena", !site.GetComponentsInChildren<Transform>(true).Any(t => t.position.y < SeaRule.FloorY - 13f));
        TestProbe.Check("el sitio, a ≥ 300 m y más cerca que antes", NuclearSiteRule.MinDistance >= 300f && NuclearSiteRule.Center.magnitude < 600f, $"{NuclearSiteRule.Center}");

        // ── 2. LÓGICA SIN ESCENA ──
        bool noMarine = MarineBase.Instance == null;
        TestProbe.Check("arranca sin la escena de abajo cargada", noMarine);
        if (rock.Now != CavernRock.Level.Normal) rock.Reinforce("prueba");
        var normal = NuclearSite.Missions(7);
        TestProbe.Check("en normal se lee a diario, sin bajada ni refuerzo; la bajada es semanal", normal.Any(m => m.id == "nuc_roca") && !normal.Any(m => m.id == "nuc_refuerzo" || m.id == SiteLine.MissionId) && NuclearSite.Missions(3).Any(m => m.id == SiteLine.MissionId && !m.mandatory));
        int reads = rock.Readings;
        TestProbe.Check("la misión de leer anota la lectura", AreaFunctions.Run("nuc_roca", "prueba") && rock.Readings == reads + 1 && rock.LastReader == "prueba");
        rock.Begin(CavernRock.Cause.Cuna);
        TestProbe.Check("una cuña que se mueve da AVISO al leer", rock.Read("prueba", day) == CavernRock.Level.Aviso && rock.MicroEvents > 0, $"{rock.RateMmDay:0.00} mm/día");
        TestProbe.Check("con AVISO hay bajada obligatoria, y se puede bajar", NuclearSite.Missions(1).Any(m => m.id == SiteLine.MissionId && m.mandatory) && line.CanRide);
        int reinf = rock.Reinforcements, insp = rock.Inspections;
        AreaFunctions.Run(SiteLine.MissionId, "prueba");
        TestProbe.Check("la bajada inspecciona y refuerza: la roca para", rock.Now == CavernRock.Level.Normal && rock.Inspections == insp + 1 && rock.Reinforcements == reinf + 1);
        rock.Begin(CavernRock.Cause.Agua);
        CavernRock.Level lv = rock.Read("prueba", day + 1);
        for (int i = 2; i < 12 && lv != CavernRock.Level.Alarma; i++) lv = rock.Read("prueba", day + i);
        TestProbe.Check("sin actuar acaba en ALARMA: nadie baja", lv == CavernRock.Level.Alarma && rock.Closed && !line.CanRide && !line.Ride(gameObject, 1f), $"{rock.InflowLpm:0} l/min");
        var alarma = NuclearSite.Missions(1);
        TestProbe.Check("en ALARMA, refuerzo obligatorio a distancia y sin bajada", alarma.Any(m => m.id == "nuc_refuerzo" && m.mandatory && m.area == NuclearSite.PowerArea) && !alarma.Any(m => m.id == SiteLine.MissionId));
        AreaFunctions.Run("nuc_refuerzo", "prueba");
        TestProbe.Check("el refuerzo a distancia la devuelve a normal", rock.Now == CavernRock.Level.Normal);
        rock.Begin(CavernRock.Cause.Cuna); for (int i = 12; i < 22 && rock.Read("prueba", day + i) != CavernRock.Level.Alarma; i++) { }
        int last = day + 22;
        rock.Read("prueba", last); rock.Read("prueba", last + CavernRule.EscalateDays);
        TestProbe.Check($"{CavernRule.EscalateDays} días de ALARMA sin reforzar declaran la emergencia del sitio", ns.State == NuclearSite.Alert.Emergencia, ns.LastReason);
        ns.ClearEmergency(); rock.Reinforce("prueba");

        TestProbe.Check($"el refugio: {RefugeRule.Capacity} plazas, un mes con máquinas y una semana con lo guardado", RefugeRule.Hours >= 720f && RefugeRule.StoredHours >= 96f && 20f * 8f >= RefugeRule.Capacity * RefugeRule.FloorM2 && RefugeRule.OxygenM3 <= 8f * 10f, $"{RefugeRule.OxygenM3:0} m³ de O₂ y {RefugeRule.FoodKg:0} kg de comida guardados");
        TestProbe.Check("un mes consume poco: menos de 10 kW", RefugeRule.KwFull < 10f, $"{RefugeRule.KwhPersonDay:0.0} kWh por persona y día; {RefugeRule.KwFull:0.0} kW");
        bool feed = refuge.reactorFeed; refuge.reactorFeed = false;
        TestProbe.Check("sin corriente del reactor, una semana", Mathf.Abs(refuge.MaxHours - RefugeRule.StoredHours) < 1f && !refuge.MakesOwn);
        refuge.reactorFeed = feed;
        int t0Day = day + 40;
        refuge.Service("prueba", t0Day);
        int serv = refuge.Services;
        TestProbe.Check("sin revisar deja de estar listo, y se revisa antes de bajar", refuge.Ready(t0Day + 5) && !refuge.Ready(t0Day + 15) && NuclearSite.Missions(t0Day + 17).Any(m => m.id == Refuge.MissionId && m.mandatory) && !NuclearSite.Missions(t0Day + 17).Any(m => m.id == SiteLine.MissionId));
        AreaFunctions.Run(Refuge.MissionId, "prueba");
        TestProbe.Check("la revisión lo deja listo", refuge.Services == serv + 1 && refuge.HoursLeft >= RefugeRule.Hours);

        // ── 3. LÍNEA Y RED ──
        Vector3 top = line.transform.position, local = top - NuclearSiteRule.Center;
        TestProbe.Check("la estación, en el casquete; el tubo que se ve, hasta el fondo somero", Mathf.Abs(top.y) < 0.5f && Mathf.Abs(local.x) < 85f && Mathf.Abs(local.z) < 85f && Named(site, "Tubo_cristal") != null && Named(site, "Boca_linea") != null && Named(site, "Escudo_tubo") != null);
        Transform shaft = Named(site, "Pozo_escape");
        TestProbe.Check("segunda salida, lejos del tubo", shaft != null && Flat(shaft.position, top) >= 20f && Mathf.Abs(line.escapeTop.position.y) < 0.5f);
        var pads = FindObjectsByType<CarpetPad>(FindObjectsSortMode.None);
        CarpetPad pad = pads.FirstOrDefault(p => p != null && p.displayName == SiteLine.AreaName);
        CarpetPad ground = pads.FirstOrDefault(p => p != null && p.kind == CarpetPad.Kind.Edificio && p.building == NuclearSiteRule.Building && p != pad && p.floor == 0);
        TestProbe.Check("se llega en alfombra: una pista de la red del edificio, a mano del andén", pad != null && ground != null && ground.Destinations().Contains(pad) && Vector3.Distance(pad.transform.position, top) < 9f);
        TestProbe.Check("la pista de la estación no se adueña del suelo del casquete", ground != null && CarpetPad.For(ground.transform.position + Vector3.up * 0.2f) == ground);
        TestProbe.Check("la jornada la trata como un sitio, y la evacuación también", WorkdayDirector.AreaTransform(SiteLine.AreaName) == line.transform && NuclearSite.IsSiteArea(SiteLine.AreaName) && WorkdayDirector.InWorkspace(null, SiteLine.AreaName, top + Vector3.right));
        string[] ids = { "nuc_vigilancia", "nuc_pasivos", "nuc_simulacro", "nuc_roca", SiteLine.MissionId, "nuc_refuerzo", Refuge.MissionId, Iceberg.TowMission, Iceberg.LaunchTow, Iceberg.EnergyTow, "lanz_vaciado", "ene_parada" };
        var noSteps = ids.Where(i => !PlayerTasks.Authored(i)).ToList();
        TestProbe.Check("las misiones tienen pasos para el jugador", noSteps.Count == 0, string.Join(", ", noSteps));

        float one = SiteLineRule.OneWay(line.Length), esc = SiteLineRule.EscapeSeconds(line.EscapeDrop, line.EscapeWalk);
        TestProbe.Check($"LÍMITE DE BERÓN: un viaje, con carga y esclusas, en menos de {SiteLineRule.MaxOneWay:0} s", one < SiteLineRule.MaxOneWay, $"{one:0.0} s");
        TestProbe.Check("dentro de lo que aguanta una persona: ≤ 20,5 m/s y ≤ 1,5 m/s²", SiteLineRule.CarSpeed <= 20.5f && SiteLineRule.CarAccel <= 1.5f);
        float mbPerSecond = 1.2f * 9.8f * line.Length / 100f / one;
        TestProbe.Check("los oídos: menos que Taipei 101 (1,26 mb/s)", mbPerSecond <= 1.26f, $"{mbPerSecond:0.00} mb/s");
        TestProbe.Check("el pozo de escape: menos de 2 min y medio", esc < 150f, $"{esc:0} s");
        TimeController tc = TimeController.timeController;
        if (tc != null)
        {
            float mission = SiteLineRule.MissionHours * tc.SecondsPerGameDay / 24f, trip = ArbolCampos.TripSeconds(new Vector3(0f, 60f, 0f), top);
            TestProbe.Check("la bajada cabe en su misión, aun llegando tarde media alfombra", SiteLineRule.RoundTrip(line.Length) + trip * 0.5f <= mission, $"{SiteLineRule.RoundTrip(line.Length):0} s + {trip * 0.5f:0} s de {mission:0} s");
        }

        // EL VIAJE DE UN TRABAJADOR, SIN CARGAR LA ESCENA: un cuerpo de prueba.
        GameObject body = new GameObject("Prueba_linea");
        MeshRenderer probe = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshRenderer>(); Destroy(probe.GetComponent<Collider>()); probe.transform.SetParent(body.transform, false);
        body.transform.position = top + Vector3.up;
        float oldTs = Time.timeScale; Time.timeScale = 4f;
        float deepest = 0f; bool hid = false;
        IEnumerator Until(System.Func<bool> done, float gameMax)
        {
            float t0 = Time.time, r0 = Time.realtimeSinceStartup;
            while (!done() && Time.time - t0 < gameMax && Time.realtimeSinceStartup - r0 < 90f) { deepest = Mathf.Max(deepest, line.Depth); hid |= !probe.enabled; yield return null; }
        }
        int trips = line.Trips;
        bool went = line.Ride(body, 2f), carried = FlyingCarpet.Riding(body);
        yield return Until(() => !line.Busy, 300f);
        TestProbe.Check("ida y vuelta sin cargar nada: baja hasta el fondo, oculto, y sube", went && carried && !line.Busy && line.Trips == trips + 1 && line.LastExit == "línea" && !FlyingCarpet.Riding(body) && deepest >= line.Length - 1f && hid && probe.enabled && MarineBase.Instance == null && Vector3.Distance(body.transform.position, top) < 3f, $"hasta {deepest:0} m; {line.LastSeconds:0} s");
        TestProbe.Near("tarda lo que dice la regla", line.LastSeconds, 2f * one + 2f, 4f);
        deepest = 0f;
        line.Ride(body, 2f);
        yield return Until(() => line.Depth > 100f, 120f);
        ns.DeclareEmergency("prueba de la línea");
        bool refused = !line.CanRide;
        yield return Until(() => !line.Busy, 120f);
        TestProbe.Check("emergencia a media bajada: vuelve desde donde esté", refused && !line.Busy && line.LastExit == "vuelta" && deepest < line.Length - 5f && Vector3.Distance(body.transform.position, top) < 3f, $"llegó a {deepest:0} m");
        ns.ClearEmergency();
        int escapes = line.Escapes;
        line.Ride(body, 3f);
        yield return Until(() => line.Depth > line.Length - 2f, 120f);
        line.OutOfService = true;
        yield return Until(() => !line.Busy, 180f);
        TestProbe.Check("con la línea cerrada se sale por el pozo de escape", line.LastExit == "escape" && line.Escapes == escapes + 1 && Vector3.Distance(body.transform.position, line.escapeTop.position) < 3f && !FlyingCarpet.Riding(body), $"{line.LastSeconds:0} s");
        line.OutOfService = false;
        refuge.Service("prueba");
        line.Ride(body, 1f);
        yield return Until(() => line.Depth > line.Length - 2f, 120f);
        line.OutOfService = true; line.EscapeOut = true;
        yield return Until(() => line.Refuged, 60f);
        bool inside = line.Refuged && refuge.Occupants == 1;
        float h0 = refuge.HoursLeft;
        yield return Until(() => refuge.HoursLeft < h0 - 0.05f, 30f);
        TestProbe.Check("línea y pozo cerrados: espera en el refugio, gastando aire", inside && refuge.HoursLeft < h0, $"quedan {refuge.HoursLeft:0.0} h");
        line.OutOfService = false;
        yield return Until(() => !line.Busy, 120f);
        TestProbe.Check("abierta la línea, sale del refugio", !line.Busy && !line.Refuged && refuge.Occupants == 0, line.LastExit);
        line.EscapeOut = false; refuge.Service("prueba");
        Time.timeScale = oldTs;
        Destroy(body);

        // ── 4. ICEBERGS ──
        Iceberg moving = Iceberg.All.FirstOrDefault(b => b != null && !b.grounded), still = Iceberg.All.FirstOrDefault(b => b != null && b.grounded);
        TestProbe.Check("dos icebergs, millones de toneladas y fuera de la alerta de los tres casquetes", moving != null && still != null && moving.Tonnes > 1e6f && IcebergRule.Sites.All(s => moving.DistanceTo(s) > IcebergRule.AlertRadius(s) && still.DistanceTo(s) > IcebergRule.DangerRadius(s)), moving != null ? $"{moving.Tonnes / 1e6f:0.0} Mt" : "");
        TestProbe.Check("el varado sigue en (−500, −500), en el santuario 1", still != null && Flat(still.transform.position, new Vector3(-500f, 0f, -500f)) < 1f);
        TestProbe.Check("con kril de verdad, aves y focas", moving != null && moving.krill != null && FindObjectsByType<IcebergVisitor>(FindObjectsSortMode.None).Any(v => v.berg == moving));
        TestProbe.Check("un trozo sí lo para el escudo; un iceberg entero, no", moving != null && moving.Tonnes > IcebergRule.ShieldTonnes);
        ArbolCampos launchTree = ArbolCampos.All.FirstOrDefault(a => a != null && a.building == LaunchHeightRule.Building), energyTree = ArbolCampos.All.FirstOrDefault(a => a != null && a.building == EnergyRule.Building);
        TestProbe.Check("los tres casquetes vigilados están donde dice la regla", (launchTree == null || Flat(launchTree.transform.position, IcebergRule.Center(IcebergRule.Site.Launch)) < 1f) && (energyTree == null || Flat(energyTree.transform.position, IcebergRule.Center(IcebergRule.Site.Energy)) < 1f));
        int radars = FindObjectsByType<Transform>(FindObjectsSortMode.None).Count(t => t.name == "Radar_hielo"), sonars = FindObjectsByType<Transform>(FindObjectsSortMode.None).Count(t => t.name == "Sonar_quillas");
        TestProbe.Check("radar y sonar en cada casquete artificial", radars >= 2 && radars == sonars, $"{radars} radares");
        if (moving != null)
        {
            Vector3 back = moving.transform.position, c = NuclearSiteRule.Center, lc = IcebergRule.Center(IcebergRule.Site.Launch);
            IcebergRule.Site N = IcebergRule.Site.Nuclear, L = IcebergRule.Site.Launch;
            moving.Place(lc + new Vector3(0f, 0f, IcebergRule.AlertRadius(L) - 20f), lc);
            TestProbe.Check("LA LANZADERA TAMBIÉN: remolque obligatorio", Iceberg.Threatening(L) == moving && LaunchSite.Missions().Any(m => m.id == Iceberg.LaunchTow && m.mandatory));
            moving.Place(lc + new Vector3(0f, 0f, IcebergRule.DangerRadius(L) - 20f), lc);
            TestProbe.Check("si llega a la lanzadera, se vacían los propulsores, y la línea nuclear no se toca", LaunchSite.Missions().Any(m => m.id == "lanz_vaciado" && m.mandatory) && !Iceberg.Danger(N));
            moving.Place(c + new Vector3(0f, 0f, IcebergRule.AlertRadius(N) - 20f), c);
            TestProbe.Check("ALERTA en el sitio nuclear: remolque obligatorio", Iceberg.Threatening(N) == moving && NuclearSite.Missions(1).Any(m => m.id == Iceberg.TowMission && m.mandatory));
            bool failed = !moving.Tow("prueba", false);
            TestProbe.Check("el remolque puede fallar, y sigue viniendo", failed && Iceberg.Threatening(N) == moving);
            moving.Place(c + new Vector3(0f, 0f, IcebergRule.DangerRadius(N) - 20f), c);
            yield return null; yield return null;
            TestProbe.Check("PELIGRO: se desconecta la línea", Iceberg.Danger(N) && line.Disconnected && line.LineClosed && !line.CanRide);
            float rs0 = Time.realtimeSinceStartup, sep0 = line.Separation;
            Time.timeScale = 8f;
            while (line.Separation < 0.2f && Time.realtimeSinceStartup - rs0 < 20f) yield return null;
            Time.timeScale = oldTs;
            TestProbe.Check("DESCONEXIÓN: suelta el tramo de arriba", line.upperTube != null && line.Separation > sep0, $"{line.Separation:0.00}");
            TestProbe.Check("reconectar tarda 4 h de juego; soltar, 90 s", Mathf.Abs(SiteLineRule.ReconnectSeconds / (tc != null ? tc.SecondsPerGameDay / 24f : 150f) - 4f) < 0.5f && SiteLineRule.DisconnectSeconds <= 90f);
            line.SetSeparation(0f);
            moving.Tow("prueba", true);
            moving.Place(back, moving.driftA);
            yield return null; yield return null;
            TestProbe.Check("remolcado y lejos, la línea vuelve", !line.Disconnected && !line.LineClosed && Iceberg.Threatening(N) == null);
        }

        // ── 5. EL SANTUARIO MARINO ──
        if (!Application.CanStreamedLevelBeLoaded(SiteLine.MarineScene)) { TestProbe.Check("el Santuario Marino está construido y en Build Settings", false, "Tools/Cold Sanctuary/Build Santuario Marino"); TestProbe.End(); yield break; }
        float mem0 = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f), rl = Time.realtimeSinceStartup;
        AsyncOperation op = SceneManager.LoadSceneAsync(SiteLine.MarineScene, LoadSceneMode.Additive);
        while (op != null && !op.isDone && Time.realtimeSinceStartup - rl < 30f) yield return null;
        float loadS = Time.realtimeSinceStartup - rl, mem1 = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
        MarineBase mb = MarineBase.Instance;
        TestProbe.Check("carga dentro del viaje (≤ el tiempo del carro)", mb != null && loadS <= SiteLineRule.Run(line.Length, SiteLineRule.CarSpeed, SiteLineRule.CarAccel), $"{loadS:0.0} s; {mem1 - mem0:0} MB");
        if (mb != null)
        {
            Transform m = mb.transform, cr = Named(m, "Caverna_microrreactor"), cw = Named(m, "Caverna_residuos");
            TestProbe.Check("el lecho hondo, las salas y el reactor, a su cota", Named(m, "Lecho_marino") != null && cr != null && cw != null && Mathf.Abs(cr.position.y - NuclearSiteRule.CavernCenterY) < 0.5f && Mathf.Abs(Flat(cr.position, NuclearSiteRule.Center)) < 1f, cr != null ? $"sala a {cr.position.y:0} m" : "");
            float cover = SeaRule.MarineFloorY - (cr.position.y + NuclearSiteRule.CavernHeight * 0.5f);
            TestProbe.Check($"≥ {NuclearSiteRule.RockCover:0} m de roca encima", cover >= NuclearSiteRule.RockCover - 0.1f, $"{cover:0} m");
            Vector3 sr = new Vector3(CavernRule.ReactorLength, NuclearSiteRule.CavernHeight, CavernRule.ReactorSpan), sw = new Vector3(CavernRule.WasteLength, NuclearSiteRule.CavernHeight, CavernRule.WasteSpan);
            var pieces = cr.GetComponentsInChildren<Transform>(true).Where(x => x.name == "Modulo_reactor" || x.name == "Tapa_trebol" || x.name == "Tubos_calor").Concat(cw.GetComponentsInChildren<Transform>(true).Where(x => x.name == "Contenedor_residuos")).ToList();
            var gaps = pieces.Select(t => Gap(t.IsChildOf(cr) ? cr : cw, t.IsChildOf(cr) ? sr : sw, t)).ToList();
            var cols = cr.GetComponentsInChildren<Renderer>(true).Concat(cw.GetComponentsInChildren<Renderer>(true)).Where(r => r.name == "Columna_roca").ToList();
            float toCol = float.MaxValue;
            foreach (Transform t in pieces) foreach (Renderer k in cols.Where(k => k.transform.parent == t.parent))
                { Bounds a = t.GetComponent<Renderer>().bounds, b = k.bounds; toCol = Mathf.Min(toCol, Mathf.Max(Mathf.Max(a.min.x - b.max.x, b.min.x - a.max.x), Mathf.Max(a.min.z - b.max.z, b.min.z - a.max.z))); }
            TestProbe.Check($"holgura medida ≥ {CavernRule.ClearanceM:0} m, al forro y a las columnas, y la regla coincide", pieces.Count == 9 && gaps.Min() >= CavernRule.ClearanceM && toCol >= CavernRule.ClearanceM && Mathf.Abs(mb.clearanceM - CavernRule.DesignClearanceM) < 0.1f && Mathf.Abs(rock.clearanceM - mb.clearanceM) < 0.1f, $"{gaps.Min():0.0} m; a columnas {toCol:0.0} m");
            float pillar = Mathf.Abs(cw.position.z - cr.position.z) - (CavernRule.ReactorSpan + CavernRule.WasteSpan) * 0.5f;
            TestProbe.Check("salas con 8 columnas, luz libre ≤ 14 m y pilar suficiente", cols.Count == 8 && CavernRule.ColumnPitch - CavernRule.ColumnSize <= CavernRule.MaxFreeSpan && pillar >= CavernRule.MaxFreeSpan && pillar >= CavernRule.WasteSpan * 0.5f, $"pilar {pillar:0} m");
            TestProbe.Check("instrumentos", mb.extensometers.Count == 8 && mb.convergence.Count == 8 && mb.loadCells.Count == 14 && mb.geophones.Count == 4 && mb.piezometers.Count == 4 && mb.flowMeters.Count == 2);
            string[] parts = { "Bucle_termosifon", "Intercambiador_lecho", "Hoyo_lecho", "Registrador_roca", "Munon_tubo", "Conector_rapido", "Compuerta_lecho", "Collar_lecho", "Pozo_escape_fondo", "Conector_escape", "Suelo_marino" };
            var none = parts.Where(p => Named(m, p) == null).ToList();
            TestProbe.Check("calor, pie del tubo y del pozo, y suelo para andar", none.Count == 0, string.Join(", ", none));
            Transform heat = Named(m, "Intercambiador_lecho"), mod = Named(cr, "Modulo_reactor");
            TestProbe.Check("el reactor, más abajo que su sumidero, y este en un hoyo", heat != null && mod != null && mod.position.y < heat.position.y && heat.position.y + heat.localScale.y * 0.5f < SeaRule.MarineFloorY);
            Transform rf = Named(m, "Refugio_fondo");
            string[] kit = { "Esclusa_refugio", "Botella_oxigeno", "Deposito_agua", "Depurador_CO2", "Baterias_refugio", "Telefono_fibra", "Radio_traves_roca", "Osmosis_inversa", "Electrolizador", "Depurador_amina", "Sondeo_mar", "Perforadora_sondeos", "Preventor_sondeo", "Despensa_refugio", "Estante_hidroponia", "Panel_espirulina", "Deposito_abono", "Cable_reactor" };
            var noKit = rf != null ? kit.Where(k => Named(rf, k) == null).ToList() : kit.ToList();
            TestProbe.Check("el refugio, entero y centrado en el pilar", rf != null && noKit.Count == 0 && Mathf.Abs(rf.position.z - Named(m, "Estacion_inferior").position.z) < 0.5f, string.Join(", ", noKit));
            TestProbe.Check("pocas piezas: la escena no pesa", m.GetComponentsInChildren<Transform>(true).Length < 2000, $"{m.GetComponentsInChildren<Transform>(true).Length} objetos");
            TestProbe.Check("no toca nada de arriba: el santuario 1 sigue igual", ns != null && SiteLine.Instance == line && SceneManager.GetActiveScene() == gameObject.scene);
            AsyncOperation un = SceneManager.UnloadSceneAsync(SiteLine.MarineScene);
            float ru = Time.realtimeSinceStartup;
            while (un != null && !un.isDone && Time.realtimeSinceStartup - ru < 30f) yield return null;
            TestProbe.Check("se descarga limpia", MarineBase.Instance == null);
        }
        TestProbe.End();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pruebas/Dos santuarios y línea (sola)")]
    static void Solo()
    {
        if (!Application.isPlaying) { Debug.Log("[Solo] no está en Play"); return; }
        var go = new GameObject("SoloLinea"); DontDestroyOnLoad(go);
        NuclearAccessTest t = go.AddComponent<NuclearAccessTest>();
        t.StartCoroutine(Wrap(t));
    }

    static IEnumerator Wrap(NuclearAccessTest t)
    {
        yield return t.Run();
        TestProbe.Total();
    }
#endif
}
