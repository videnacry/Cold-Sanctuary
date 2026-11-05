using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LA ROCA Y LA LÍNEA DE LAS CAVERNAS (Berón 2026-11-05; docs/sitio-nuclear.md §7–§8):
/// 1. HOLGURA: medida en la escena, ≥ <see cref="CavernRule.ClearanceM"/> m, y un pilar de roca entre las cavernas no menor que la luz mayor.
/// 2. INSTRUMENTOS: extensómetros, convergencia, bulones, geófonos, piezómetros, aforos, asiento del cajón, pilotes, registrador y panel.
/// 3. LA ALERTA: leer → AVISO → bajada obligatoria → reforzar → normal; ALARMA → nadie baja, refuerzo a distancia; sin reforzar → emergencia.
/// 4. LA LÍNEA: del casquete a la galería, cruzando el lecho, con pendiente válida y sin compartir pozo con el reactor; segunda salida aparte.
/// 5. LA RED: la estación es una pista más de las alfombras, cuenta como zona del sitio y sus misiones tienen pasos para el jugador.
/// 6. LA JORNADA: el viaje cabe en la misión, y se compara con el ascensor.
/// 7. EL VIAJE, de verdad, con un cuerpo de prueba: ida y vuelta; emergencia a media bajada (vuelve); línea cerrada (sale por el pozo de escape).
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
        TestProbe.Check("la holgura guardada coincide con la medida", Mathf.Abs(rock.clearanceM - gap) < 0.1f, $"{rock.clearanceM:0.0} m");
        float pillar = Mathf.Abs(cw.position.z - cr.position.z) - (CavernRule.ReactorSpan + CavernRule.WasteSpan) * 0.5f;
        TestProbe.Check("pilar de roca entre cavernas ≥ la luz mayor", pillar >= CavernRule.WasteSpan && Mathf.Abs(pillar - CavernRule.PillarM) < 0.1f, $"{pillar:0} m");
        TestProbe.Check("con el ritmo del SFR la holgura dura > 10 000 años; con el de la sal de WIPP (~100 mm/año), décadas", CavernRule.YearsToClose(gap, CavernRule.BaseRateMmDay * 365f) > 10000f && CavernRule.YearsToClose(gap, 100f) >= 25f,
            $"{CavernRule.YearsToClose(gap, CavernRule.BaseRateMmDay * 365f):0} años; {CavernRule.YearsToClose(gap, 100f):0} años");

        // ── 2. INSTRUMENTOS ──
        TestProbe.Check("instrumentos en las dos cavernas", rock.extensometers.Count == 8 && rock.convergence.Count == 8 && rock.loadCells.Count == 6 && rock.geophones.Count == 4 && rock.piezometers.Count == 4 && rock.flowMeters.Count == 2,
            $"{rock.extensometers.Count} extensómetros, {rock.convergence.Count} dianas, {rock.loadCells.Count} células, {rock.geophones.Count} geófonos, {rock.piezometers.Count} piezómetros, {rock.flowMeters.Count} aforos");
        Transform cj = Named(site, "Cajon_TRIGA");
        int piles = cj != null ? cj.GetComponentsInChildren<Transform>(true).Count(t => t.name == "Pilote_roca") : 0;
        TestProbe.Check("el cajón, sobre el sedimento: pilotes a la roca, hitos de asiento e inclinómetros", piles == 4 && rock.caissonGauges.Count == 6, $"{piles} pilotes, {rock.caissonGauges.Count} sensores");
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
        TestProbe.Check("cruza el mar y entra en la roca: cristal y revestimiento", Named(site, "Tubo_cristal") != null && Named(site, "Tubo_revestido") != null && top.y > NuclearSiteRule.SeabedY && foot.y < NuclearSiteRule.SeabedY - NuclearSiteRule.RockCover);
        TestProbe.Check($"pendiente de funicular (≤ {SiteLineRule.MaxGradient:P0})", line.Gradient <= SiteLineRule.MaxGradient, $"{line.Length:0} m al {line.Gradient:P0}");
        float MinToAxis(Vector3 p) { float b = float.MaxValue; for (int i = 0; i <= 40; i++) { Vector3 q = Vector3.Lerp(top, foot, i / 40f); if (q.y < p.y + 90f) b = Mathf.Min(b, Flat(q, p)); } return b; }
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
        string[] ids = { "nuc_vigilancia", "nuc_pasivos", "nuc_simulacro", "nuc_roca", SiteLine.MissionId, "nuc_refuerzo" };
        var noSteps = ids.Where(i => !PlayerTasks.Authored(i)).ToList();
        TestProbe.Check("las misiones del sitio tienen pasos para el jugador", noSteps.Count == 0, string.Join(", ", noSteps));

        // ── 6. LA JORNADA ──
        float one = SiteLineRule.OneWay(line.Length), hoist = SiteLineRule.HoistOneWay(line.Drop), esc = SiteLineRule.EscapeSeconds(line.EscapeDrop, line.EscapeWalk);
        TestProbe.Check("un viaje, en menos de un minuto y medio; las esclusas tardan más que el carro", one <= 90f && 2f * SiteLineRule.LockSeconds > SiteLineRule.Run(line.Length, SiteLineRule.CarSpeed, SiteLineRule.CarAccel), $"{one:0} s");
        TestProbe.Check("el ascensor no gana casi nada: las esclusas son las mismas", hoist < one && one - hoist < 30f, $"ascensor {hoist:0} s, línea {one:0} s");
        TestProbe.Check("el pozo de escape saca a la gente en menos de 2 min", esc < 120f, $"{esc:0} s");
        TimeController tc = TimeController.timeController;
        if (tc != null)
        {
            float mission = SiteLineRule.MissionHours * tc.SecondsPerGameDay / 24f;
            float trip = ArbolCampos.TripSeconds(new Vector3(0f, 60f, 0f), top);
            TestProbe.Check("la bajada cabe en su misión, aun llegando tarde media alfombra", SiteLineRule.RoundTrip(line.Length) + trip * 0.5f <= mission, $"{SiteLineRule.RoundTrip(line.Length):0} s + {trip * 0.5f:0} s de {mission:0} s");
        }

        // ── 7. EL VIAJE ──
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
