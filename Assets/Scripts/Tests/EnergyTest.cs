using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DEL PARQUE ENERGÉTICO (Berón 2026-11-05; docs/energia.md):
/// 1. SITIO: casquete y árbol bajo, a ≥ 300 m de todo; turbinas a ≥ 200 m de las islas y fuera del alcance del hielo.
/// 2. DEMANDA: todo piso la declara; los paneles salen en proporción; el validador cumple.
/// 3. SOL: cero en la noche polar, ~900 kWh/kWp al año, la suciedad resta y limpiar lo devuelve.
/// 4. PISO NUEVO: pide mesas, sale <c>sol_instalacion</c>, se montan y vuelve a cumplir.
/// 5. FAUNA: un ave para la turbina y rearma; pala negra, cámaras y radar; un animal en el agua para la mareal; punta ≤ 5 m/s.
/// 6. ALMACÉN: sin reactor, las baterías sostienen lo esencial y se apaga lo demás; lo que sobra va a las baterías y al silo.
/// 7. JORNADA: las misiones tienen pasos con E, actuación, uniforme y sitio; averías y reparación; Fernando es su profesional.
/// 8. AVES: ni cría ni cura junto al rotor (refugio a ≥ 400 m), ronda, cura y suelta; figuras y dragón solo a demanda, con habituación, y la turbina para igual.
/// </summary>
public class EnergyTest : MonoBehaviour, ITestUnit
{
    public int Group => 101;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<EnergyTest>() == null) tr.gameObject.AddComponent<EnergyTest>();
    }

    static float Flat(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));

    public IEnumerator Run()
    {
        TestProbe.Begin("Parque Energético: cada piso con sus paneles, oferta ≥ demanda, sol polar, fauna a salvo, almacén y jornada");
        yield return AereoLink.LoadForTest();   // (lo que se ve del parque está en el Santuario Aéreo; la lógica, en el santuario 1)
        EnergySite es = FindAnyObjectByType<EnergySite>();
        ArbolCampos tree = ArbolCampos.All.FirstOrDefault(a => a != null && a.building == EnergyRule.Building);
        if (es == null || tree == null) { TestProbe.Check("hay un Parque Energético (su árbol y su casquete): Build Santuario Aéreo", false); TestProbe.End(); yield break; }
        float soil0 = es.soil, bat0 = es.batteryMWh;
        int inst0 = es.installed;

        // ── 1. SITIO ──
        var others = ArbolCampos.All.Where(a => a != null && a != tree).ToList();
        float nearest = others.Min(a => Flat(a.transform.position, tree.transform.position));
        TestProbe.Greater("a ≥ 300 m del edificio-árbol más cercano", nearest, EnergyRule.MinDistance - 0.5f);
        TestProbe.Check("tiene su casquete artificial", tree.transform.parent != null && tree.transform.parent.GetComponentsInChildren<Transform>(true).Any(t => t.name == "Casquete_artificial"));
        TestProbe.Check("árbol bajo: primer piso a 12 m", Mathf.Abs(tree.GetComponentsInChildren<TreeFloor>(true).Min(f => f.transform.position.y) - EnergyRule.FirstFloorY) < 1f);
        GameObject floes = GameObject.Find("Banquisa_Casquetes");
        float toIce = 9999f;
        if (floes != null)
            foreach (Transform b in floes.transform)
                if (b.name == "Casquete")
                    foreach (WindTurbineUnit u in es.turbines) toIce = Mathf.Min(toIce, Flat(u.transform.position, b.position) - Mathf.Max(b.localScale.x, b.localScale.z) * 0.5f);
        TestProbe.Greater("las turbinas, a ≥ 200 m de las islas", toIce, EnergyRule.IslandBuffer - 1f);
        TestProbe.Check("tres turbinas fuera del alcance del hielo sobre el árbol", es.turbines.Count == EnergyRule.Turbines && es.turbines.All(u => Flat(u.transform.position, tree.transform.position) >= EnergyRule.IceThrowRadius));
        TestProbe.Check("las turbinas cuentan como elementos elevados (T13)", es.turbines.All(u => u.GetComponent<ElevatedElement>() != null));

        // ── 2. DEMANDA Y REGLA ──
        EnergyRule.DeclareMissing();
        var areas = FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None).Where(a => a.gameObject.scene == es.gameObject.scene).ToArray();
        TestProbe.Check("todo piso declara su demanda", areas.All(a => a.GetComponent<FloorEnergy>() != null && a.GetComponent<FloorEnergy>().KW >= 0f));
        TestProbe.Check("también los pisos sin área: residencia, sala de guardia y laboratorio", new[] { "Residencia, plaza", AreaWork.GuardRoom, "Laboratorio de contención", "Aire de la celda del árbol cazador" }.All(n => FloorEnergy.All.Any(f => f.floor == n && f.KW > 0f)) && FloorEnergy.All.Count(f => f.floor.StartsWith("Residencia, Planta")) >= 5 && FloorEnergy.All.Any(f => f.floor.StartsWith("Aire") && f.essential));
        FloorEnergy info = areas.FirstOrDefault(a => a.areaType == SanctuaryAreaType.Informatics)?.GetComponent<FloorEnergy>();
        TestProbe.Check("Informática suma sus 95 kW", info != null && info.fixedKW >= EnergyRule.InformaticsKW);
        TestProbe.Check("los paneles van en proporción a la energía", EnergyRule.TablesFor(200f) >= 2 * EnergyRule.TablesFor(100f) - 1 && EnergyRule.TablesFor(0f) == 0);
        EnergyRule.Report rep = es.Validate("prueba");
        Debug.Log($"[Energía] Balance: {rep}");
        TestProbe.Check($"la regla cumple: {rep}", rep.Ok);
        TestProbe.Greater("hay al menos el reactor para la punta", EnergyRule.ReactorKW, rep.peakKW);
        TestProbe.Check("quedan plazas para pisos futuros", es.tables.Count > es.installed);
        TestProbe.Check("las mesas de más esperan apagadas", es.tables.Skip(es.installed).All(t => !t.gameObject.activeSelf) && es.tables.Take(es.installed).All(t => t.gameObject.activeSelf));

        // ── 3. EL SOL ──
        TestProbe.Check("en la noche polar el sol da cero", EnergyRule.SolarAt(172f, 12f) == 0f && EnergyRule.SolarAt(172f, 0f) == 0f);
        TestProbe.Check("en verano da sol a medianoche", EnergyRule.SolarAt(355f, 0.5f) > 0f);
        TestProbe.Near("el año rinde ~900 kWh/kWp", EnergyRule.YearYield(), EnergyRule.AnnualYield, 150f);
        float[] y = EnergyRule.MonthlyYield();
        TestProbe.Check("de mayo a julio, nada; diciembre es de lo mejor", y[4] + y[5] + y[6] < 1f && y[11] > 100f);
        es.reactorForcedOff = false;
        es.soil = 0f; es.Step(0f, 355f, 12f, 0f); float clean = es.SolarKW;
        es.soil = EnergyRule.MaxSoil; es.Step(0f, 355f, 12f, 0f); float dirty = es.SolarKW;
        TestProbe.Check("la escarcha resta producción", clean > 0f && dirty < clean * 0.75f, $"{clean:0} → {dirty:0} kW");
        TestProbe.Check("sol_mantenimiento limpia", AreaFunctions.Run("sol_mantenimiento", "Prueba") && es.soil == 0f);
        es.Step(0f, 172f, 12f, 0f);
        TestProbe.Check("en la noche polar el campo no da nada y el reactor cubre", es.SolarKW == 0f && es.ReactorKW + es.WindKW + es.TidalKW >= es.DemandKW - 1f);

        // ── 4. UN PISO NUEVO ──
        GameObject fake = new GameObject("Piso_de_prueba_energia");
        FloorEnergy fe = fake.AddComponent<FloorEnergy>(); fe.floor = "Prueba"; fe.areaM2 = 1024f; fe.wattsPerM2 = 45f;
        int pending = es.Pending;
        TestProbe.Check("un piso nuevo pide mesas", pending > 0, $"{pending}");
        TestProbe.Check("sale la misión de instalar", EnergySite.Missions(0).Any(m => m.id == "sol_instalacion"));
        TestProbe.Check("sale su sitio en el casquete", EnergySite.FieldPoint("sol_instalacion", "Prueba").HasValue);
        bool notYet = !es.Report().Ok;
        TestProbe.Check("sin paneles, la regla lo dice", notYet);
        for (int i = 0; i < 20 && es.Pending > 0; i++) AreaFunctions.Run("sol_instalacion", "Prueba");
        TestProbe.Check("instaladas sus mesas, cumple", es.Report().Ok && es.tables[es.installed - 1].gameObject.activeSelf);
        DestroyImmediate(fake);
        for (int i = inst0; i < es.installed; i++) es.tables[i].gameObject.SetActive(false);
        es.installed = inst0;

        // ── 5. LA FAUNA ──
        WindTurbineUnit w = es.turbines[0];
        TestProbe.Check("cada torre lleva su pala negra y su detector", es.turbines.All(u => u.blackBlade != null && u.blackBlade.name == "Pala_negra" && u.detector != null));
        Vector3 outward = w.transform.position - EnergyRule.Center; outward.y = 0f;
        GameObject bird = new GameObject("Ave_de_prueba"); bird.transform.position = w.transform.position + outward.normalized * 100f + Vector3.up * 40f;   // (por fuera del casquete: a 100 m hacia dentro también paraba a la torre de al lado, a 208 m)
        EnergySite.TestBirds.Add(bird.transform);
        es.ScanNow();
        TestProbe.Check("un ave a ≤ 200 m para la turbina", w.Stopped);
        TestProbe.Near("parada no entrega nada", w.OutputKW(10f), 0f, 0.01f);
        w.Tick(10f);
        TestProbe.Near("parada, el rotor va a 0", w.targetRpm, 0f, 0.01f);
        bird.transform.position = w.transform.position + new Vector3(600f, 40f, 0f);
        es.ScanNow();
        TestProbe.Check("un ave lejos no para las demás", es.turbines.Skip(1).All(u => !u.Stopped));
        w.ClearForTest(); es.ScanNow();
        TestProbe.Check("lejos, no para", !w.Stopped);
        w.Hold("prueba");
        float t0 = Time.time;
        while (w.Stopped && Time.time - t0 < EnergyRule.ResumeSeconds + 4f) yield return new WaitForSeconds(1f);
        TestProbe.Check("sin aves rearma sola", !w.Stopped, $"{Time.time - t0:0} s");
        EnergySite.TestBirds.Clear(); Destroy(bird);

        TidalTurbineUnit m = es.tidal[0];
        TestProbe.Check("la mareal lleva sonar y anillo", es.tidal.Count == EnergyRule.TidalUnits && es.tidal.All(u => u.sonar != null && u.sonar.parent.GetComponentsInChildren<Transform>(true).Any(t => t.name == "Anillo_mareal")));
        GameObject seal = new GameObject("Foca_de_prueba"); seal.transform.position = m.transform.position + new Vector3(30f, 0f, 0f);
        EnergySite.TestSwimmers.Add(seal.transform);
        es.ScanNow();
        TestProbe.Check("un animal a ≤ 50 m para la mareal", m.Stopped);
        EnergySite.TestSwimmers.Clear(); Destroy(seal); m.ClearForTest();
        TestProbe.Check("la punta no pasa de 5 m/s ni con 2 m/s de corriente", EnergyRule.TipSpeed(2f) <= EnergyRule.MaxTipSpeed && EnergyRule.MaxTipSpeed < 5.6f);
        TestProbe.Check("la mareal es un piloto: ≤ 2 % de la demanda", EnergyRule.TidalPowerKW(EnergyRule.PeakCurrent) * EnergyRule.TidalUnits < 30f);

        // ── 6. EL ALMACÉN ──
        es.batteryMWh = EnergyRule.BatteryMWh * 0.5f;
        es.reactorForcedOff = true;
        float before = es.batteryMWh;
        es.Step(0f, 172f, 12f, 1f);
        TestProbe.Check("sin reactor se apaga lo no esencial", es.ReactorKW == 0f && es.ShedKW > 0f);
        float gen = es.WindKW + es.TidalKW, need = EnergyRule.EssentialKW();
        if (gen < need) TestProbe.Check("sin reactor ni viento bastante, tiran las baterías", es.batteryMWh < before);
        TestProbe.Greater("lo esencial aguanta ≥ 72 h", EnergyRule.BatteryMWh * EnergyRule.BatteryEfficiency * 1000f / Mathf.Max(1f, need), EnergyRule.EssentialHours);
        es.reactorForcedOff = false;
        es.batteryMWh = 0f;
        es.Step(0f, 355f, 12f, 1f);
        TestProbe.Check("con el reactor, nada se apaga", es.ShedKW < 0.01f);
        TestProbe.Check("hay 6 contenedores y un silo", es.storage != null && es.storage.GetComponentsInChildren<Transform>(true).Count(t => t.name.StartsWith("Bateria_LFP")) == EnergyRule.BatteryContainers && es.storage.Find("Silo_arena") != null);

        // ── 7. LA JORNADA ──
        es.Fail(0);
        TestProbe.Check("una avería enciende su baliza y da misión", es.Faults == 1 && es.tables[0].Find("Aviso_averia").gameObject.activeSelf && EnergySite.Missions(0).Any(x => x.id == "sol_reparacion"));
        es.Step(0f, 355f, 12f, 0f); float broken = es.SolarKW;
        TestProbe.Check("sol_reparacion la repone", AreaFunctions.Run("sol_reparacion", "Prueba") && es.Faults == 0 && !es.tables[0].Find("Aviso_averia").gameObject.activeSelf);
        es.Step(0f, 355f, 12f, 0f);
        TestProbe.Check("la mesa vuelve a producir", es.SolarKW > broken);
        string[] ids = { "sol_mantenimiento", "sol_instalacion", "sol_reparacion", "ene_control", "ene_eolica", "ene_mareal", "ene_almacen" };
        var authored = PlayerTasks.AuthoredIds.ToList(); var own = Performances.OwnMissions.ToList();
        TestProbe.Check("todas con pasos con E", ids.All(authored.Contains));
        TestProbe.Check("todas con su actuación", ids.All(own.Contains));
        TestProbe.Check("todas con su uniforme", ids.All(i => Uniforms.ForWork(i, EnergyRule.Area, "x") == Uniforms.Kind.Energia));
        var seen = Enumerable.Range(0, 3).SelectMany(d => EnergySite.Missions(d)).Select(x => x.id).ToList();
        TestProbe.Check("cada tres días salen las revisiones", new[] { "sol_mantenimiento", "ene_control", "ene_eolica", "ene_mareal", "ene_almacen" }.All(seen.Contains));
        foreach (string st in new[] { "panel", "aerogenerador", "detector", "pontona", "sonar", "bateria", "silo", "consola", "banco_taller" })
            TestProbe.Check($"el puesto «{st}» está en el parque", PlayerTasks.Resolve(EnergyRule.Area, st, 0, null, null).found);
        TestProbe.Check("las de sitio van al casquete", new[] { "sol_mantenimiento", "ene_eolica", "ene_mareal", "ene_almacen" }.All(i => { Vector3? p = EnergySite.FieldPoint(i, "Prueba"); return p.HasValue && EnergySite.OnCap(p.Value); }));
        TestProbe.Check("Fernando es su profesional", Professions.Practises("Fernando", EnergyRule.Area, "sol_mantenimiento"));

        // ── 8. LAS AVES ──
        BirdRehabPoint rp = FindAnyObjectByType<BirdRehabPoint>();
        TestProbe.Check("hay refugio de aves, en el Hospital", rp != null && rp.GetComponentInParent<SanctuaryArea>() != null && rp.GetComponentInParent<SanctuaryArea>().displayName == AreaWork.Hospital);
        if (rp == null) { yield return AereoLink.UnloadForTest(); TestProbe.End(); yield break; }
        TestProbe.Greater("el refugio, lejos de todas las turbinas", es.turbines.Min(u => Vector3.Distance(u.transform.position, rp.transform.position)), EnergyRule.RehabMinDistance);
        TestProbe.Check("en el parque no hay cría ni refugio", es.GetComponentInChildren<BirdRehabPoint>(true) == null);
        rp.patients.Clear();
        int rel0 = rp.Released;
        es.injuredWaiting = 1; es.carcassesWaiting = 1; int car0 = es.Carcasses;
        TestProbe.Check("la ronda sale cada día", EnergySite.Missions(1).Any(x => x.id == "ene_ronda_aves"));
        TestProbe.Check("la ronda lleva al ave al refugio y retira los restos", AreaFunctions.Run("ene_ronda_aves", "Prueba") && rp.patients.Count == 1 && es.injuredWaiting == 0 && es.Carcasses == car0 + 1);
        TestProbe.Check("el ave se ve en su caja", rp.birds[0].activeSelf);
        TestProbe.Check("sale la misión del Hospital", EnergySite.Missions(1).Any(x => x.id == "hos_aves" && x.area == AreaWork.Hospital));
        for (int i = 0; i < EnergyRule.RehabDays; i++) AreaFunctions.Run("hos_aves", "Prueba");
        TestProbe.Check("curada, se suelta", rp.Released == rel0 + 1 && rp.patients.Count == 0 && !rp.birds[0].activeSelf);
        foreach (string st in new[] { "caja_aves", "hangar" }) TestProbe.Check($"el puesto «{st}» está en el parque", PlayerTasks.Resolve(EnergyRule.Area, st, 0, null, null).found);
        TestProbe.Check("el puesto del refugio está en el Hospital", PlayerTasks.Resolve(AreaWork.Hospital, "refugio", 0, null, null).found);
        string[] ids8 = { "ene_ronda_aves", "ene_disuasion", "hos_aves" };
        var authored8 = PlayerTasks.AuthoredIds.ToList(); var own8 = Performances.OwnMissions.ToList();
        TestProbe.Check("con pasos con E y actuación", ids8.All(authored8.Contains) && ids8.All(own8.Contains));
        TestProbe.Check("hos_aves, de veterinaria; las otras, de energía", Uniforms.ForWork("hos_aves", AreaWork.Hospital, "x") == Uniforms.Kind.Veterinaria && Uniforms.ForWork("ene_ronda_aves", EnergyRule.Area, "x") == Uniforms.Kind.Energia);

        TestProbe.Check("las figuras esperan ocultas", es.figures.Count > 0 && es.figureSpots.Count >= es.figures.Count);
        TestProbe.Check("el dragón tiene sus drones y el cartel de adorno", es.dragon != null && es.dragon.drones.Count > 0);
        foreach (WindTurbineUnit u in es.turbines) u.ClearForTest();
        es.habituation = 0.9f;
        float wait0 = Time.time;
        while (Time.time - wait0 < 61f) yield return new WaitForSeconds(1f);   // (la disuasión espera 60 s entre salidas)
        GameObject bird2 = new GameObject("Ave_de_prueba_2"); bird2.transform.position = es.turbines[1].transform.position + new Vector3(50f, 40f, 0f);
        EnergySite.TestBirds.Add(bird2.transform);
        int sc0 = es.Scares;
        es.ScanNow();
        TestProbe.Check("habituadas: la turbina para igual, sin figuras", es.turbines[1].Stopped && es.Scares == sc0 && !es.figures[0].gameObject.activeSelf);
        TestProbe.Check("sale la misión de moverlas", EnergySite.Missions(0).Any(x => x.id == "ene_disuasion"));
        Vector3 was = es.figures[0].position;
        AreaFunctions.Run("ene_disuasion", "Prueba");
        TestProbe.Check("moverlas baja la habituación y cambia el sitio", es.habituation < 0.9f && es.figures[0].position != was);
        es.habituation = 0f; foreach (WindTurbineUnit u in es.turbines) u.ClearForTest();
        wait0 = Time.time;
        while (Time.time - wait0 < 61f) yield return new WaitForSeconds(1f);
        foreach (WindTurbineUnit u in es.turbines) u.ClearForTest();   // (el barrido de cada segundo la paró durante la espera: sin esto no «salta» el detector)
        es.ScanNow();
        TestProbe.Check("sin habituación salen las figuras y sube", es.Scares == sc0 + 1 && es.figures[0].gameObject.activeSelf && es.habituation > 0f);
        EnergySite.TestBirds.Clear(); Destroy(bird2);
        foreach (WindTurbineUnit u in es.turbines) u.ClearForTest();
        es.habituation = 0f;

        es.soil = soil0; es.batteryMWh = bat0;
        yield return AereoLink.UnloadForTest();
        TestProbe.End();
    }
}
