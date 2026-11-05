using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LA ROCA Y LA LÍNEA DE LAS CAVERNAS (Berón 2026-11-05; docs/sitio-nuclear.md §7–§8):
/// 1. HOLGURA: medida en la escena, ≥ <see cref="CavernRule.ClearanceM"/> m, también a las columnas; luz libre ≤ <see cref="CavernRule.MaxFreeSpan"/> m y pilar de roca suficiente.
/// 2. INSTRUMENTOS y APOYOS: columnas, bulones, cables; extensómetros, convergencia, geófonos, piezómetros, aforos, tendones del cajón, registrador y panel.
/// 3. LA ALERTA: leer → AVISO → bajada obligatoria → reforzar → normal; ALARMA → nadie baja, refuerzo a distancia; sin reforzar → emergencia.
/// 4. EL MAR Y LA LÍNEA: mar de profundidad real, del casquete a la galería, cruzando el lecho, en menos de un minuto y sin compartir pozo con el reactor; segunda salida aparte.
/// 5. LA RED: la estación es una pista más de las alfombras, cuenta como zona del sitio y sus misiones tienen pasos para el jugador.
/// 6. LA JORNADA: el viaje cabe en la misión; aceleración y velocidad dentro de lo que aguanta una persona; se compara con el ascensor.
/// 7. REFUGIO, HIELO E ICEBERGS: el refugio con sus cuentas y su revisión; la plataforma de 300 m con los troncos abiertos; escudo, compuerta, detección, remolque y desconexión.
/// 8. EL VIAJE, de verdad, con un cuerpo de prueba: ida y vuelta; emergencia a media bajada (vuelve); línea cerrada (sale por el pozo de escape).
/// </summary>
public class NuclearAccessTest : MonoBehaviour, ITestUnit
{
    public int Group => 100;   // (libres al escribirlo, mirando todas las ramas: 77, 85, 100; 101 es de EnergyTest)
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<NuclearAccessTest>() == null) tr.gameObject.AddComponent<NuclearAccessTest>();
    }

    static Transform Named(Transform root, string name) => root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
    static float Flat(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));

    // La holgura de una pieza medida en el mundo: del borde de lo que se dibuja al forro de la caverna, por los lados y por arriba.
    static float Gap(Transform cav, Vector3 size, Transform piece)
    {
        Bounds b = piece.GetComponent<Renderer>().bounds;
        Vector3 inner = (size - Vector3.one * 1.2f) * 0.5f, c = cav.position;
        return Mathf.Min(inner.x - Mathf.Max(b.max.x - c.x, c.x - b.min.x), inner.z - Mathf.Max(b.max.z - c.z, c.z - b.min.z), c.y + inner.y - b.max.y);
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Roca y línea de las cavernas: holgura, instrumentos, alerta, acceso rápido y segunda salida");
        NuclearSite ns = FindAnyObjectByType<NuclearSite>();
        CavernRock rock = CavernRock.Instance;
        SiteLine line = SiteLine.Instance;
        if (ns == null || rock == null || line == null) { TestProbe.Check("hay sitio nuclear, vigilancia de la roca y línea", false, $"sitio {ns != null}, roca {rock != null}, línea {line != null}"); TestProbe.End(); yield break; }
        Transform site = ns.transform;
        Transform cr = Named(site, "Caverna_microrreactor"), cw = Named(site, "Caverna_residuos"), well = Named(site, "Pozo_microrreactor");
        if (cr == null || cw == null || well == null) { TestProbe.Check("las dos cavernas y el pozo", false); TestProbe.End(); yield break; }

        // ── 1. HOLGURA ──
        Vector3 sr = new Vector3(CavernRule.ReactorLength, NuclearSiteRule.CavernHeight, CavernRule.ReactorSpan), sw = new Vector3(CavernRule.WasteLength, NuclearSiteRule.CavernHeight, CavernRule.WasteSpan);
        var gaps = new List<float>();
        foreach (Transform t in cr.GetComponentsInChildren<Transform>(true).Where(x => x.name == "Modulo_reactor" || x.name == "Tapa_trebol" || x.name == "Tubos_calor")) gaps.Add(Gap(cr, sr, t));
        foreach (Transform t in cw.GetComponentsInChildren<Transform>(true).Where(x => x.name == "Contenedor_residuos")) gaps.Add(Gap(cw, sw, t));
        float gap = gaps.Count > 0 ? gaps.Min() : 0f;
        TestProbe.Check($"holgura medida ≥ {CavernRule.ClearanceM:0} m entre el equipo y el forro, en las dos cavernas", gaps.Count == 9 && gap >= CavernRule.ClearanceM, $"{gap:0.0} m en {gaps.Count} piezas");
        TestProbe.Check("la holgura guardada no supera la medida", rock.clearanceM <= gap + 0.1f && rock.clearanceM >= CavernRule.ClearanceM, $"{rock.clearanceM:0.0} m");
        float pillar = Mathf.Abs(cw.position.z - cr.position.z) - (CavernRule.ReactorSpan + CavernRule.WasteSpan) * 0.5f;
        TestProbe.Check("pilar de roca entre salas ≥ la luz libre y ≥ medio ancho de sala", pillar >= CavernRule.MaxFreeSpan && pillar >= CavernRule.WasteSpan * 0.5f && Mathf.Abs(pillar - CavernRule.PillarM) < 0.1f, $"{pillar:0} m");
        // COLUMNAS: la sala es grande porque se apoya; la luz libre es lo que manda, y el equipo guarda su holgura también a ellas.
        var cols = cr.GetComponentsInChildren<Renderer>(true).Concat(cw.GetComponentsInChildren<Renderer>(true)).Where(r => r.name == "Columna_roca").ToList();
        float span = CavernRule.ColumnPitch - CavernRule.ColumnSize;
        TestProbe.Check("salas grandes con columnas: 4 por sala, y ninguna luz libre pasa del máximo", cols.Count == 8 && span <= CavernRule.MaxFreeSpan && Mathf.Max(CavernRule.ReactorLength, CavernRule.WasteLength) * 0.5f - CavernRule.ColumnPitch * 0.5f - CavernRule.ColumnSize * 0.5f <= CavernRule.MaxFreeSpan, $"{cols.Count} columnas, {span:0} m de luz central");
        TestProbe.Check("la sala de 44 × 40 m tiene más planta que las de antes, sin perder holgura", CavernRule.ReactorLength * CavernRule.ReactorSpan >= 5f * 240f);
        float toCol = float.MaxValue;
        foreach (Transform t in cr.GetComponentsInChildren<Transform>(true).Concat(cw.GetComponentsInChildren<Transform>(true)).Where(x => x.name == "Modulo_reactor" || x.name == "Tapa_trebol" || x.name == "Tubos_calor" || x.name == "Contenedor_residuos"))
            foreach (Renderer k in cols.Where(k => k.transform.parent == t.parent))
            {
                Bounds a = t.GetComponent<Renderer>().bounds, b = k.bounds;
                toCol = Mathf.Min(toCol, Mathf.Max(Mathf.Max(a.min.x - b.max.x, b.min.x - a.max.x), Mathf.Max(a.min.z - b.max.z, b.min.z - a.max.z)));
            }
        TestProbe.Check("el equipo guarda la holgura también a las columnas", toCol >= CavernRule.ClearanceM, $"{toCol:0.0} m");
        TestProbe.Check("con el ritmo del SFR la holgura dura > 10 000 años; con el de la sal de WIPP (~100 mm/año), décadas", CavernRule.YearsToClose(gap, CavernRule.BaseRateMmDay * 365f) > 10000f && CavernRule.YearsToClose(gap, 100f) >= 25f,
            $"{CavernRule.YearsToClose(gap, CavernRule.BaseRateMmDay * 365f):0} años; {CavernRule.YearsToClose(gap, 100f):0} años");

        // ── 2. INSTRUMENTOS ──
        TestProbe.Check("instrumentos en las dos cavernas", rock.extensometers.Count == 8 && rock.convergence.Count == 8 && rock.loadCells.Count == 14 && rock.geophones.Count == 4 && rock.piezometers.Count == 4 && rock.flowMeters.Count == 2,
            $"{rock.extensometers.Count} extensómetros, {rock.convergence.Count} dianas, {rock.loadCells.Count} células, {rock.geophones.Count} geófonos, {rock.piezometers.Count} piezómetros, {rock.flowMeters.Count} aforos");
        Transform cj = Named(site, "Cajon_TRIGA");
        int piles = cj != null ? cj.GetComponentsInChildren<Transform>(true).Count(t => t.name == "Tendon_cajon") : 0;
        int bolts = cr.GetComponentsInChildren<Transform>(true).Count(t => t.name == "Bulon_techo"), cables = cr.GetComponentsInChildren<Transform>(true).Count(t => t.name == "Cable_anclaje");
        TestProbe.Check("techo sostenido: bulones y cables en cada sala", bolts >= 40 && cables == 4, $"{bolts} bulones, {cables} cables");
        TestProbe.Check("el cajón cuelga: tendones al lecho, con su célula, e inclinómetros", piles == 4 && rock.caissonGauges.Count == 6, $"{piles} tendones, {rock.caissonGauges.Count} sensores");
        GameObject central = GameObject.Find("NuclearPower_Area");
        TestProbe.Check("el panel de la roca, en la Central, y el registrador abajo", central != null && Named(central.transform, "Panel_roca") != null && Named(site, "Registrador_roca") != null);

        // ── 3. LA ALERTA ──
        if (rock.Now != CavernRock.Level.Normal) rock.Reinforce("prueba");
        int day = TimeController.timeController != null ? TimeController.timeController.Day : 0;
        var normal = NuclearSite.Missions(7);
        TestProbe.Check("en normal se lee a diario, sin bajada ni refuerzo; la bajada es semanal", normal.Any(m => m.id == "nuc_roca") && !normal.Any(m => m.id == "nuc_refuerzo" || m.id == SiteLine.MissionId) && NuclearSite.Missions(3).Any(m => m.id == SiteLine.MissionId && !m.mandatory));
        int reads = rock.Readings;
        TestProbe.Check("la misión de leer anota la lectura", AreaFunctions.Run("nuc_roca", "prueba") && rock.Readings == reads + 1 && rock.LastReader == "prueba");
        rock.Begin(CavernRock.Cause.Cuna);
        TestProbe.Check("una cuña que se mueve da AVISO al leer", rock.Read("prueba", day) == CavernRock.Level.Aviso && rock.MicroEvents > 0, $"{rock.RateMmDay:0.00} mm/día");
        var aviso = NuclearSite.Missions(1);
        TestProbe.Check("con AVISO hay bajada obligatoria, y se puede bajar", aviso.Any(m => m.id == SiteLine.MissionId && m.mandatory) && line.CanRide);
        int reinf = rock.Reinforcements, insp = rock.Inspections;
        AreaFunctions.Run(SiteLine.MissionId, "prueba");
        TestProbe.Check("la bajada inspecciona y refuerza: la roca para", rock.Now == CavernRock.Level.Normal && rock.Inspections == insp + 1 && rock.Reinforcements == reinf + 1 && rock.Episode == CavernRock.Cause.Ninguna);
        rock.Begin(CavernRock.Cause.Agua);
        CavernRock.Level lv = rock.Read("prueba", day + 1);
        TestProbe.Check("si sube el agua, también AVISO", lv == CavernRock.Level.Aviso, $"{rock.InflowLpm:0} l/min");
        for (int i = 2; i < 12 && lv != CavernRock.Level.Alarma; i++) lv = rock.Read("prueba", day + i);
        TestProbe.Check("sin actuar acaba en ALARMA: nadie baja", lv == CavernRock.Level.Alarma && rock.Closed && !line.CanRide && !line.Ride(gameObject, 1f), $"{rock.InflowLpm:0} l/min");
        var alarma = NuclearSite.Missions(1);
        TestProbe.Check("en ALARMA, refuerzo obligatorio a distancia y sin bajada", alarma.Any(m => m.id == "nuc_refuerzo" && m.mandatory && m.area == NuclearSite.PowerArea) && !alarma.Any(m => m.id == SiteLine.MissionId));
        AreaFunctions.Run("nuc_refuerzo", "prueba");
        TestProbe.Check("el refuerzo a distancia la devuelve a normal y lo gastado de la holgura no vuelve", rock.Now == CavernRock.Level.Normal && rock.MarginLeftM <= rock.clearanceM);
        rock.Begin(CavernRock.Cause.Cuna); for (int i = 12; i < 22 && rock.Read("prueba", day + i) != CavernRock.Level.Alarma; i++) { }
        int last = day + 22;
        rock.Read("prueba", last); rock.Read("prueba", last + CavernRule.EscalateDays);
        TestProbe.Check($"{CavernRule.EscalateDays} días de ALARMA sin reforzar declaran la emergencia del sitio", ns.State == NuclearSite.Alert.Emergencia, ns.LastReason);
        ns.ClearEmergency(); rock.Reinforce("prueba");

        // ── 4. LA LÍNEA ──
        Vector3 top = line.transform.position, foot = line.bottom.position;
        Vector3 local = top - NuclearSiteRule.Center;
        TestProbe.Check("arriba, en el casquete; abajo, en la galería entre las cavernas", Mathf.Abs(top.y) < 0.5f && Mathf.Abs(local.x) < 85f && Mathf.Abs(local.z) < 85f && Flat(foot, Named(site, "Galeria_cavernas").position) < 1f, $"de {top.y:0} m a {foot.y:0} m");
        TestProbe.Check("cruza el mar y entra en la roca: cristal arriba, acero hasta el lecho y revestimiento", Named(site, "Tubo_cristal") != null && Named(site, "Tubo_acero") != null && Named(site, "Tubo_revestido") != null && top.y > NuclearSiteRule.SeabedY && foot.y < NuclearSiteRule.SeabedY - NuclearSiteRule.RockCover);
        GameObject bed = GameObject.Find("FondoMarino"), zone = GameObject.Find("Sea_SwimZone");
        BoxCollider bedC = bed != null ? bed.GetComponent<BoxCollider>() : null, zoneC = zone != null ? zone.GetComponent<BoxCollider>() : null;
        TestProbe.Check($"el mar tiene {SeaRule.Depth:0} m, como el mar de Ross: el fondo y el agua nadable llegan abajo", bedC != null && zoneC != null && Mathf.Abs(bedC.bounds.max.y - SeaRule.FloorY) < 0.5f && Mathf.Abs(zoneC.bounds.min.y - SeaRule.FloorY) < 0.5f && Mathf.Abs(zoneC.bounds.max.y - SeaRule.Surface) < 0.5f, zoneC != null ? $"agua de {zoneC.bounds.max.y:0.0} a {zoneC.bounds.min.y:0.0} m" : "");
        Transform limit = GameObject.Find("Limite_del_mar")?.transform;
        TestProbe.Check("el límite del mar llega al lecho: no se pasa nadando por debajo", limit == null || limit.GetComponentsInChildren<Collider>(true).All(c => c.bounds.min.y <= SeaRule.FloorY));
        TestProbe.Check("a la línea le basta el recorrido entero: del casquete a la galería", line.Drop >= SeaRule.Depth + NuclearSiteRule.RockCover, $"{line.Length:0} m a {line.AngleDeg:0}°, {line.Drop:0} m de desnivel");
        float MinToAxis(Vector3 p) { float b = float.MaxValue; for (int i = 0; i <= 40; i++) { Vector3 q = Vector3.Lerp(top, foot, i / 40f); b = Mathf.Min(b, Flat(q, p)); } return b; }
        Transform shaft = Named(site, "Pozo_escape");
        TestProbe.Check("el tubo no pasa por el pozo del reactor", MinToAxis(well.position) >= 3f, $"{MinToAxis(well.position):0.0} m");
        TestProbe.Check("segunda salida: pozo propio, lejos del tubo y del pozo del reactor, hasta el casquete", shaft != null && Flat(shaft.position, well.position) >= 5f && MinToAxis(shaft.position) >= 5f && Mathf.Abs(line.escapeTop.position.y) < 0.5f && Named(site, "Jaula_escape") != null,
            shaft != null ? $"a {Flat(shaft.position, well.position):0.0} m del pozo; a {MinToAxis(shaft.position):0.0} m del tubo" : "");

        // ── 5. LA RED ──
        var pads = FindObjectsByType<CarpetPad>(FindObjectsSortMode.None);
        CarpetPad pad = pads.FirstOrDefault(p => p != null && p.displayName == SiteLine.AreaName);
        CarpetPad ground = pads.FirstOrDefault(p => p != null && p.kind == CarpetPad.Kind.Edificio && p.building == NuclearSiteRule.Building && p != pad && p.floor == 0);
        TestProbe.Check("se llega en alfombra: la estación es una pista de la red del edificio, a mano del andén", pad != null && ground != null && ground.Destinations().Contains(pad) && pad.Destinations().Contains(ground) && Vector3.Distance(pad.transform.position, top) < 9f, pad != null ? $"a {Vector3.Distance(pad.transform.position, top):0.0} m del andén" : "sin pista");
        TestProbe.Check("la pista de la estación no se adueña del suelo del casquete", ground != null && CarpetPad.For(ground.transform.position + Vector3.up * 0.2f) == ground);
        TestProbe.Check("la jornada la trata como un sitio del casquete, y la evacuación también", WorkdayDirector.AreaTransform(SiteLine.AreaName) == line.transform && NuclearSite.IsSiteArea(SiteLine.AreaName) && WorkdayDirector.InWorkspace(null, SiteLine.AreaName, top + Vector3.right));
        string[] ids = { "nuc_vigilancia", "nuc_pasivos", "nuc_simulacro", "nuc_roca", SiteLine.MissionId, "nuc_refuerzo", Refuge.MissionId, Iceberg.TowMission };
        var noSteps = ids.Where(i => !PlayerTasks.Authored(i)).ToList();
        TestProbe.Check("las misiones del sitio tienen pasos para el jugador", noSteps.Count == 0, string.Join(", ", noSteps));

        // ── 6. LA JORNADA ──
        float one = SiteLineRule.OneWay(line.Length), hoist = SiteLineRule.HoistOneWay(line.Drop), esc = SiteLineRule.EscapeSeconds(line.EscapeDrop, line.EscapeWalk);
        TestProbe.Check($"LÍMITE DE BERÓN: un viaje, esclusas incluidas, en menos de {SiteLineRule.MaxOneWay:0} s", one < SiteLineRule.MaxOneWay, $"{one:0.0} s");
        TestProbe.Check("dentro de lo que aguanta una persona: ≤ 20,5 m/s (el récord) y ≤ 1,5 m/s² (el tope de confort)", SiteLineRule.CarSpeed <= 20.5f && SiteLineRule.CarAccel <= 1.5f, $"{SiteLineRule.CarSpeed:0} m/s, {SiteLineRule.CarAccel:0.0} m/s²");
        float mbPerSecond = 1.2f * 9.8f * line.Drop / 100f / one;
        TestProbe.Check("los oídos: repartida en el viaje, la presión cambia menos que en Taipei 101 (1,26 mb/s)", mbPerSecond <= 1.26f, $"{mbPerSecond:0.00} mb/s");
        TestProbe.Check("un ascensor a plomo no gana casi nada", hoist < one && one - hoist < 3f, $"ascensor {hoist:0.0} s, línea {one:0.0} s");
        TestProbe.Check("el pozo de escape saca a la gente en menos de 2 min y medio (no está en el minuto: es otro ascensor, a baterías)", esc < 150f, $"{esc:0} s");
        TimeController tc = TimeController.timeController;
        if (tc != null)
        {
            float mission = SiteLineRule.MissionHours * tc.SecondsPerGameDay / 24f;
            float trip = ArbolCampos.TripSeconds(new Vector3(0f, 60f, 0f), top);
            TestProbe.Check("la bajada cabe en su misión, aun llegando tarde media alfombra", SiteLineRule.RoundTrip(line.Length) + trip * 0.5f <= mission, $"{SiteLineRule.RoundTrip(line.Length):0} s + {trip * 0.5f:0} s de {mission:0} s");
        }

        // ── 7. REFUGIO ──
        Refuge refuge = Refuge.Instance;
        Transform foot2 = line.escapeBottom;
        TestProbe.Check("hay refugio al pie del pozo de escape", refuge != null && foot2 != null && Vector3.Distance(refuge.transform.position, foot2.position) < 8f, refuge != null ? $"a {Vector3.Distance(refuge.transform.position, foot2.position):0.0} m" : "");
        if (refuge != null)
        {
            string[] kit = { "Esclusa_refugio", "Botella_oxigeno", "Deposito_agua", "Depurador_CO2", "Baterias_refugio", "Telefono_fibra", "Radio_traves_roca" };
            var noKit = kit.Where(k => Named(refuge.transform, k) == null).ToList();
            TestProbe.Check("esclusa, oxígeno, agua, depurador, baterías y dos comunicaciones", noKit.Count == 0, string.Join(", ", noKit));
            TestProbe.Check($"las cuentas: {RefugeRule.Capacity} plazas y {RefugeRule.Hours:0} h, más que las 96 h de las minas; suelo y botellas alcanzan", RefugeRule.Hours >= 96f && 8f * 5f >= RefugeRule.Capacity * RefugeRule.FloorM2 && RefugeRule.OxygenM3 <= 8f * 10f, $"{RefugeRule.OxygenM3:0} m³ de O₂, {RefugeRule.WaterLitres:0} l de agua");
            int t0Day = day + 40;
            refuge.Service("prueba", t0Day);
            int serv = refuge.Services;
            TestProbe.Check("sin revisar se va quedando sin estar listo, y entonces se revisa antes de bajar", refuge.Ready(t0Day + 5) && !refuge.Ready(t0Day + 15) && NuclearSite.Missions(t0Day + 17).Any(m => m.id == Refuge.MissionId && m.mandatory) && !NuclearSite.Missions(t0Day + 17).Any(m => m.id == SiteLine.MissionId));
            AreaFunctions.Run(Refuge.MissionId, "prueba");
            TestProbe.Check("la revisión lo deja listo", refuge.Services == serv + 1 && refuge.HoursLeft >= RefugeRule.Hours);
        }

        // ── 8. HIELO E ICEBERGS ──
        GameObject floes = GameObject.Find("Banquisa_Casquetes");
        Transform block = floes != null ? floes.transform.Cast<Transform>().FirstOrDefault(t => t.name == "Casquete") : null;
        TestProbe.Check($"la plataforma de hielo: {SeaRule.ShelfThickness:0} m bajo el suelo y mar debajo", block != null && Mathf.Abs(block.localScale.y - SeaRule.ShelfThickness) < 1f && SeaRule.ShelfThickness < SeaRule.Depth, block != null ? $"{block.localScale.y:0} m" : "sin bloques");
        var trees = ArbolCampos.All.Where(a => a != null && a.bottomHole > 0f && a.building != NuclearSiteRule.Building && a.building != LaunchHeightRule.Building).ToList();
        var blocked = trees.Where(a => Physics.SphereCast(new Vector3(a.transform.position.x, -2f, a.transform.position.z), 0.6f, Vector3.down, out RaycastHit _, SeaRule.ShelfThickness + 5f, ~0, QueryTriggerInteraction.Ignore)).Select(a => a.building).ToList();
        TestProbe.Check("los troncos siguen abiertos al mar a través de toda la plataforma", trees.Count > 0 && blocked.Count == 0, string.Join(", ", blocked));
        ConstructionSubmarine csub = ConstructionSubmarine.Instance;
        TestProbe.Check("el submarino de obra cruza por debajo del hielo", csub == null || csub.seaDepth < -SeaRule.ShelfThickness, csub != null ? $"a {csub.seaDepth:0} m" : "");
        string[] guard = { "Escudo_tubo", "Diente_escudo", "Escudo_escape", "Compuerta_lecho", "Collar_lecho", "Radar_hielo", "Sonar_quillas", "Hoyo_lecho" };
        var noGuard = guard.Where(g => Named(site, g) == null).ToList();
        TestProbe.Check("protección: escudo con dientes, compuerta en el lecho, radar, sonar e intercambiador en su hoyo", noGuard.Count == 0, string.Join(", ", noGuard));
        Transform pit = Named(site, "Intercambiador_lecho");
        TestProbe.Check("el intercambiador queda por debajo del lecho", pit != null && pit.position.y + pit.localScale.y * 0.5f < SeaRule.FloorY);
        Iceberg moving = Iceberg.All.FirstOrDefault(b => b != null && !b.grounded), still = Iceberg.All.FirstOrDefault(b => b != null && b.grounded);
        TestProbe.Check("dos icebergs: uno varado y otro a la deriva, varios millones de toneladas y lejos de todo", moving != null && still != null && moving.Tonnes > 1e6f && moving.DistanceToSite > IcebergRule.AlertRadius, moving != null ? $"{moving.Tonnes / 1e6f:0.0} Mt" : "");
        TestProbe.Check("con kril de verdad, aves y focas", moving != null && moving.krill != null && moving.birds != null && FindObjectsByType<IcebergVisitor>(FindObjectsSortMode.None).Any(v => v.berg == moving));
        TestProbe.Check("cálado como su plataforma", moving != null && Mathf.Abs(moving.size.y - SeaRule.ShelfThickness) < 1f);
        TestProbe.Check("un trozo sí lo para el escudo; un iceberg entero, no", moving != null && moving.Tonnes > IcebergRule.ShieldTonnes);
        if (moving != null)
        {
            Vector3 back = moving.transform.position, c = NuclearSiteRule.Center;
            moving.Place(c + new Vector3(0f, 0f, IcebergRule.AlertRadius - 20f), c);
            TestProbe.Check("ALERTA: un iceberg que viene pide remolque obligatorio", Iceberg.Threatening == moving && NuclearSite.Missions(1).Any(m => m.id == Iceberg.TowMission && m.mandatory));
            bool failed = !moving.Tow("prueba", false);
            TestProbe.Check("el remolque puede fallar, y sigue viniendo", failed && Iceberg.Threatening == moving);
            moving.Place(c + new Vector3(0f, 0f, IcebergRule.DangerRadius - 20f), c);
            yield return null; yield return null;
            TestProbe.Check("PELIGRO: se desconecta la línea", Iceberg.AnyDanger && line.Disconnected && line.LineClosed && !line.CanRide);
            moving.Tow("prueba", true);
            moving.Place(back, moving.driftA);
            yield return null; yield return null;
            TestProbe.Check("remolcado y lejos, la línea vuelve", !line.Disconnected && Iceberg.Threatening == null, moving.name);
        }

        // ── 9. EL VIAJE ──
        GameObject body = new GameObject("Prueba_linea");
        body.transform.position = top + Vector3.up;
        float oldTs = Time.timeScale; Time.timeScale = 4f;
        float low = 999f;
        bool WaitOn() => body != null;
        IEnumerator Until(System.Func<bool> done, float gameMax)
        {
            float t0 = Time.time, r0 = Time.realtimeSinceStartup;
            while (!done() && WaitOn() && Time.time - t0 < gameMax && Time.realtimeSinceStartup - r0 < 90f) { low = Mathf.Min(low, body.transform.position.y); yield return null; }
        }
        // Ida y vuelta.
        int trips = line.Trips;
        bool went = line.Ride(body, 2f), carried = FlyingCarpet.Riding(body);
        yield return Until(() => !line.Busy, 300f);
        TestProbe.Check("ida y vuelta: baja a la galería, sube y lo deja libre, contado como en alfombra", went && carried && !line.Busy && line.Trips == trips + 1 && line.LastExit == "línea" && !FlyingCarpet.Riding(body) && low <= foot.y + 2f && Vector3.Distance(body.transform.position, top) < 3f,
            $"hasta y={low:0.0}; {line.LastExit}; {line.LastSeconds:0} s");
        TestProbe.Near("tarda lo que dice la regla", line.LastSeconds, 2f * one + 2f, 4f);
        // Emergencia a media bajada: vuelve.
        low = 999f;
        line.Ride(body, 2f);
        yield return Until(() => body.transform.position.y < -30f, 120f);
        ns.DeclareEmergency("prueba de la línea");
        bool refused = !line.CanRide;
        yield return Until(() => !line.Busy, 120f);
        TestProbe.Check("emergencia a media bajada: el carro vuelve desde donde esté y nadie más sube", refused && !line.Busy && line.LastExit == "vuelta" && low > foot.y + 5f && Vector3.Distance(body.transform.position, top) < 3f, $"llegó a y={low:0.0}; {line.LastExit}");
        ns.ClearEmergency();
        // Línea cerrada: sale por el pozo de escape.
        int escapes = line.Escapes;
        line.Ride(body, 3f);
        yield return Until(() => body.transform.position.y < foot.y + 2f, 120f);
        line.OutOfService = true;
        bool shut = !line.CanRide;
        yield return Until(() => !line.Busy, 180f);
        TestProbe.Check("con la línea cerrada se sale por el pozo de escape, hasta el casquete", shut && line.LastExit == "escape" && line.Escapes == escapes + 1 && Vector3.Distance(body.transform.position, line.escapeTop.position) < 3f && !FlyingCarpet.Riding(body), $"{line.LastExit}; {line.LastSeconds:0} s");
        line.OutOfService = false;
        // Línea y pozo cerrados: al refugio, y de ahí fuera en cuanto se abre uno.
        if (refuge != null)
        {
            refuge.Service("prueba");
            line.Ride(body, 1f);
            yield return Until(() => body.transform.position.y < foot.y + 2f, 120f);
            line.OutOfService = true; line.EscapeOut = true;
            yield return Until(() => line.Refuged, 60f);
            bool inside = line.Refuged && refuge.Occupants == 1 && Vector3.Distance(body.transform.position, refuge.transform.position) < 3f;
            float h0 = refuge.HoursLeft;
            yield return Until(() => refuge.HoursLeft < h0 - 0.05f, 30f);
            TestProbe.Check("línea y pozo cerrados: espera en el refugio, gastando aire", inside && refuge.HoursLeft < h0, $"quedan {refuge.HoursLeft:0.0} h");
            line.OutOfService = false;
            yield return Until(() => !line.Busy, 120f);
            TestProbe.Check("abierta la línea, sale del refugio", !line.Busy && !line.Refuged && refuge.Occupants == 0 && !FlyingCarpet.Riding(body), line.LastExit);
            line.EscapeOut = false; refuge.Service("prueba");
        }
        Time.timeScale = oldTs;
        Destroy(body);
        TestProbe.End();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pruebas/Roca y línea de las cavernas (sola)")]
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
