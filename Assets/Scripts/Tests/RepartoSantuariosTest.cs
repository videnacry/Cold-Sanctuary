using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DEL REPARTO POR SANTUARIOS (grupo 113; Berón 2026-11-08; docs/jornada-y-rotacion.md §Reparto por santuarios). Casi todo es lógica pura, sin escena:
///  1. La casa de cada uno: los nueve de siempre en s1n1, los demás por la tabla de umbrales; se va a lo anterior, nunca a lo siguiente.
///  2. Cada uno pasa el día en un sitio al que llega; las horas de cada área ENTERA (el hospital, el yoga) salen de su lista de hoy.
///  3. Las horas se estiran desde las 16:00 para acabar; Kushal no. La Magnate, solo para áreas sin nadie; lo que sobra, segundo reparto.
///  4. El reparto cambia cada día, y cambia con los datos: más personas o más tareas, otras horas y otros clones.
///  5. Quien pasa el día fuera se apaga y vuelve para la fiesta; el horario libre ve la jornada estirada.
/// </summary>
public class RepartoSantuariosTest : MonoBehaviour, ITestUnit
{
    public int Group => 113;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<RepartoSantuariosTest>() == null) tr.gameObject.AddComponent<RepartoSantuariosTest>();
    }

    static readonly SanctuaryPlacement.Home S1 = new SanctuaryPlacement.Home(1, 1), S2 = new SanctuaryPlacement.Home(2, 1), S3 = new SanctuaryPlacement.Home(3, 1), S1N2 = new SanctuaryPlacement.Home(1, 2);
    static string Energy => EnergyRule.Area;
    static string Control => LaunchSite.ControlArea;
    static string Line => SiteLine.AreaName;

    static WorkRoster Roster(params (string name, int home)[] people)
    {
        var r = new WorkRoster { sanctuaryModel = true, useProfessions = false };
        r.areas.AddRange(new[] { "A", "B" });
        foreach (var p in people) r.Add(p.name).home = p.home;
        return r;
    }

    static List<WorkRoster.Mission> Ms(string area, string prefix, int n, float h = 1f, bool mandatory = false)
    {
        var l = new List<WorkRoster.Mission>();
        for (int i = 0; i < n; i++) l.Add(new WorkRoster.Mission(area, prefix + i, prefix + i, h, mandatory));
        return l;
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Reparto por santuarios: casa por estadísticas, acceso hacia atrás, horas por área, jornada estirada y clones solo donde no hay nadie");

        // ── 1. La casa de cada uno ──
        // SE SUBE POR PODER (Berón 2026-11-08; docs/progresion-santuarios.md): jefe = el animal más poderoso; magia y bonusPacks para superarlo
        var bosses = SanctuaryPower.Bosses();
        float whale = SanctuaryPower.OfSpecies("Whale");
        float faunaMax = SanctuaryPower.FaunaOf[SanctuaryPlacement.Land].Max(SanctuaryPower.OfSpecies);
        TestProbe.Check("el jefe de s1n1 sale de la fauna y sus arquetipos: el más fuerte de ballena, orca y oso polar (aquí, la ballena)",
            bosses.Count >= 3 && bosses[0].who == "Whale" && Mathf.Approximately(bosses[0].power, faunaMax) && whale >= SanctuaryPower.OfSpecies("Orca") && whale >= SanctuaryPower.OfSpecies("Bear"),
            string.Join(" · ", bosses.Select((b, i) => $"{SanctuaryPlacement.Home.At(i)}: {b.who} {b.power:0.00}")));
        TestProbe.Check("con la fauna viva de la escena, manda la viva", Mathf.Approximately(SanctuaryPower.Bosses(("Orca_TEST", 9f))[0].power, 9f));
        bool rising = true; for (int i = 1; i < bosses.Count; i++) rising &= bosses[i].power >= bosses[i - 1].power;
        TestProbe.Check("cada jefe es al menos tan fuerte como el anterior (hay que superar a TODOS sus animales)", rising);
        var probe = new GameObject("Reparto_TEST_poder"); Anima pa0 = probe.AddComponent<Anima>(); pa0.strength = 1.3f; pa0.bodyMass = 0.9f; pa0.armament = 0.2f;
        bool oneFormula = Mathf.Approximately(Predation.PredatorPower(pa0), Predation.Power(pa0.strength, pa0.bodyMass, pa0.armadura, pa0.agility, pa0.armament, pa0.sickness));
        Destroy(probe);
        TestProbe.Check("una sola fórmula para animales y humanos (Predation.Power)", oneFormula);
        TestProbe.Check("la magia es lo que suben las margas (0 sin subir)", Mathf.Approximately(SanctuaryPower.Magic(Aptitudes.Default, 0), 0f) && SanctuaryPower.Magic(Aptitudes.Default, 10) > 0.5f);
        float plain = SanctuaryPower.PlainHuman;
        TestProbe.Check("un humano corriente se queda en s1n1", SanctuaryPower.PositionFor(plain, bosses) == 0 && SanctuaryPlacement.Next(false, 0, false, 0, SanctuaryPower.OfHuman(null, "Fernando"), bosses) == 0);
        TestProbe.Check("el pack solo no basta para la ballena; con magia, sí", plain + SanctuaryPower.OfPack("bonusPack2") < bosses[0].power && plain + SanctuaryPower.OfPack("bonusPack2") + SanctuaryPower.Magic(Aptitudes.Default, 10) > bosses[0].power);
        TestProbe.Check("Irosene lleva bonusPack2", Mathf.Approximately(SanctuaryPower.OfHuman(null, "Irosene"), SanctuaryPower.OfSpecies("Irosene") + SanctuaryPower.OfPack("bonusPack2")));
        int StartOf(string n) => SanctuaryPlacement.Next(false, 0, false, SanctuaryPlacement.StartingHome.TryGetValue(n, out int st) ? st : 0, SanctuaryPower.OfHuman(null, n), bosses);
        TestProbe.Check("de inicio: Irosene y Lizmibet en s2n1, Linduarda en s3n1, Kushal y los demás por su poder (s1n1)",
            StartOf("Irosene") == 1 && StartOf("Lizmibet") == 1 && StartOf("Linduarda") == 2 && StartOf("Player") == 0 && StartOf("Goluis") == 0 && StartOf("Luciana") == 0,
            string.Join(", ", new[] { "Irosene", "Lizmibet", "Linduarda", "Player", "Goluis", "Luciana" }.Select(n => $"{n} {SanctuaryPower.OfHuman(null, n):0.00} → {SanctuaryPlacement.Home.At(StartOf(n))}")));
        TestProbe.Check("quien supera al jefe de su casa sube UN santuario, y una vez por día", SanctuaryPlacement.Next(true, 0, false, 0, bosses[0].power + 0.1f, bosses) == 1 && SanctuaryPlacement.Next(true, 0, true, 0, bosses[0].power + 0.1f, bosses) == 0 && SanctuaryPlacement.Next(true, 0, false, 0, bosses[0].power - 0.1f, bosses) == 0);
        TestProbe.Check("tras el último santuario del nivel, el nivel siguiente; y nunca baja", SanctuaryPlacement.Home.At(SanctuaryPlacement.Next(true, 2, false, 0, bosses[2].power + 0.1f, bosses)).Equals(S1N2) && SanctuaryPlacement.Next(true, 2, false, 0, 0f, bosses) == 2);
        TestProbe.Check("posición y casa: s3n1 es la 2 y la 3 es s1n2 (acabar la vuelta sube el nivel)", S3.Position == 2 && SanctuaryPlacement.Home.At(3).Equals(S1N2) && SanctuaryPlacement.Home.At(2).Equals(S3));
        TestProbe.Check("s1n1 no va a s1n2 ni a s2n1", !SanctuaryPlacement.CanAccess(S1, S1N2) && !SanctuaryPlacement.CanAccess(S1, S2) && SanctuaryPlacement.CanAccess(S1, S1));
        TestProbe.Check("s1n2 va a s1n1, s2n1 y s3n1", SanctuaryPlacement.CanAccess(S1N2, S1) && SanctuaryPlacement.CanAccess(S1N2, S2) && SanctuaryPlacement.CanAccess(S1N2, S3));
        TestProbe.Check("s2n1 va a s1n1 pero no a s3n1", SanctuaryPlacement.CanAccess(S2, S1) && !SanctuaryPlacement.CanAccess(S2, S3));
        TestProbe.Check("el santuario de cada área: el Aéreo, el Marino y la tierra", SanctuaryPlacement.SanctuaryOfArea(Energy) == 3 && SanctuaryPlacement.SanctuaryOfArea(Control) == 3 && SanctuaryPlacement.SanctuaryOfArea(Line) == 2 && SanctuaryPlacement.SanctuaryOfArea("Cocina") == 1 && SanctuaryPlacement.SanctuaryOfArea(AreaWork.Hospital) == 1);
        TestProbe.Check("la planta nuclear (Planta Atómica y Central Nuclear) es del Marino: lejos del santuario y de la vida", SanctuaryPlacement.SanctuaryOfArea(NuclearSite.PlantArea) == 2 && SanctuaryPlacement.SanctuaryOfArea(NuclearSite.PowerArea) == 2);
        WorkRoster nucLand = Roster(("T0", 0), ("T1", 0));
        WorkRoster.DayPlan pnl = nucLand.Plan(0, Ms(NuclearSite.PowerArea, "nuc_t", 2));
        TestProbe.Check("los de la tierra nunca trabajan en la planta nuclear: la cubre un clon", !pnl.work.Any(a => !a.worker.clone) && pnl.clones.Count == 1);

        // ── 2 y 3. Un día con los tres santuarios ──
        WorkRoster ro = Roster(("L1", 0), ("L2", 0), ("L3", 0), ("M", 1), ("Ae", 2));
        var day = new List<WorkRoster.Mission>();
        day.AddRange(Ms("A", "a", 4)); day.AddRange(Ms("B", "b", 4)); day.AddRange(Ms(Energy, "e", 6)); day.AddRange(Ms(Line, "l", 1));
        WorkRoster.DayPlan p = ro.Plan(0, day);
        WorkRoster.Worker ae = ro.Find("Ae"), m = ro.Find("M");
        TestProbe.Check("cada uno a un sitio al que llega: los de s1n1 en la tierra, M en el Marino y Ae en el Aéreo",
            ro.workers.Where(w => w.home == 0).All(w => p.place[w].Equals(S1)) && p.place[m].Equals(S2) && p.place[ae].Equals(S3), string.Join(" · ", p.place.Select(kv => $"{kv.Key.name}: {kv.Value}")));
        TestProbe.Check("nadie va más allá de su casa", ro.workers.All(w => SanctuaryPlacement.CanAccess(w.home, p.place[w])));
        TestProbe.Check("nadie trabaja fuera de su sitio del día", p.work.Where(a => !a.worker.clone).All(a => p.place[a.worker].Equals(SanctuaryPlacement.PlaceOfArea(a.mission.area))), string.Join(" · ", p.work.Select(a => $"{a.worker.name}→{a.mission.area}")));
        TestProbe.Check("las horas de cada área salen de su lista de hoy (Energía 6 h → pide 2 personas; Línea 1 h → 1)", Mathf.Approximately(p.hoursByArea[Energy], 6f) && p.quota[Energy] == 2 && p.quota[Line] == 1 && Mathf.Approximately(p.hoursByArea["A"], 4f));
        // PARTES IGUALES (Berón 2026-11-08, corregido). Tolerancia: entre quien más y quien menos del sitio, como mucho la tarea más larga del sitio.
        float Worked(WorkRoster.DayPlan pl, WorkRoster.Worker w) => pl.Of(w).Sum(a => a.mission.hours);
        bool Even(WorkRoster.DayPlan pl, IEnumerable<WorkRoster.Worker> ws, float longest) { var hs = ws.Select(w => Worked(pl, w)).ToList(); return hs.Count == 0 || hs.Max() - hs.Min() <= longest + 1e-3f; }
        var land = ro.workers.Where(w => w.home == 0).ToList();
        TestProbe.Check("en la tierra, 8 h entre 3: partes iguales (diferencia ≤ la tarea más larga, 1 h) y todas hechas", Even(p, land, 1f) && Mathf.Approximately(land.Sum(w => Worked(p, w)), 8f), string.Join(" · ", land.Select(w => $"{w.name} {Worked(p, w)} h")));
        TestProbe.Check("en el Aéreo, 6 h para 1 persona: ella 5 h y un clon la hora que sobra", Mathf.Approximately(Worked(p, ae), 5f) && p.clones.Count == 1 && p.Of(p.clones[0]).All(a => a.mission.area == Energy) && Mathf.Approximately(p.share[S3], 5f), $"Ae {Worked(p, ae)} h · {p.clones.Count} clones");
        TestProbe.Check("nada pasa a mañana ni queda imposible", p.backlog.Count == 0 && ro.pending.Count == 0 && p.impossible.Count == 0);
        TestProbe.Check("nadie trabaja después de las 16:00 (sin jornada estirada)", p.work.All(a => a.start + a.mission.hours <= WorkRoster.StartHour + 8f + 1e-3f));
        TestProbe.Check("el que sobraba tras cubrir las horas va al segundo reparto", p.secondSplit.Count == 1 && p.secondSplit[0].home == 0, string.Join(", ", p.secondSplit.Select(w => w.name)));
        TestProbe.Check("el informe dice las horas de cada santuario y la parte de cada uno", WorkRoster.Balance(p, ro.workers.Count).Any(s => s.StartsWith("santuario 3 nivel 1") && s.Contains("5 h cada una")));
        WorkRoster four = Roster(("F0", 0), ("F1", 0), ("F2", 0), ("F3", 0));
        WorkRoster.DayPlan p4 = four.Plan(0, Ms("A", "f", 4));
        TestProbe.Check("pocas horas: 4 h entre 4, una cada uno, sin clones", four.workers.All(w => Mathf.Approximately(Worked(p4, w), 1f)) && p4.clones.Count == 0);
        WorkRoster mix = Roster(("X0", 0), ("X1", 0), ("X2", 0));
        var mixed = new List<WorkRoster.Mission>(Ms("A", "m2_", 3, 2f)); mixed.AddRange(Ms("B", "m1_", 4));
        WorkRoster.DayPlan pm = mix.Plan(0, mixed);
        TestProbe.Check("con tareas de 2 h y de 1 h (10 h entre 3): diferencia ≤ 2 h, todo hecho, sin clones", Even(pm, mix.workers, 2f) && Mathf.Approximately(mix.workers.Sum(w => Worked(pm, w)), 10f) && pm.clones.Count == 0, string.Join(" · ", mix.workers.Select(w => $"{w.name} {Worked(pm, w)} h")));

        // Clones solo para las áreas sin nadie
        WorkRoster lone = Roster(("L1", 0), ("L2", 0));
        var far = new List<WorkRoster.Mission>();
        far.AddRange(Ms("A", "a", 4)); far.AddRange(Ms(Energy, "e", 2)); far.AddRange(Ms(Control, "c", 1)); far.AddRange(Ms(Line, "l", 1));
        WorkRoster.DayPlan pf = lone.Plan(0, far);
        TestProbe.Check("un clon por cada área sin nadie (Energía, Control de misión, Línea), y ninguno en la tierra",
            pf.clones.Count == 3 && pf.emptyAreaClones == 3 && new HashSet<string>(pf.cloneAreas).SetEquals(new[] { Energy, Control, Line }), $"{pf.clones.Count}: {string.Join(", ", pf.cloneAreas)}");
        TestProbe.Check("cada clon, en una sola área", pf.clones.All(c => pf.Of(c).Select(a => a.mission.area).Distinct().Count() == 1));
        WorkRoster solo = Roster(("Solo", 0));
        WorkRoster.DayPlan ps = solo.Plan(0, Ms("A", "x", 6, 2f, true));
        WorkRoster.Worker so = solo.Find("Solo");
        TestProbe.Check("12 h obligatorias para 1 persona: ella hasta 5 h y clones el resto, nada a mañana",
            Worked(ps, so) <= WorkRoster.WorkHours + 1e-3f && ps.clones.Count >= 1 && ps.work.Count == 6 && ps.backlog.Count == 0 && ps.impossible.Count == 0 && WorkRoster.Balance(ps, 1).Any(s => s.StartsWith("Faltan personas")), $"{Worked(ps, so)} h, {ps.clones.Count} clones, {ps.backlog.Count} pendientes");
        WorkRoster solo2 = Roster(("Solo", 0));
        WorkRoster.DayPlan ps2 = solo2.Plan(0, Ms("A", "y", 6, 2f));
        TestProbe.Check("las no obligatorias también: clones para lo que no cabe, nada a mañana", ps2.clones.Count >= 1 && ps2.work.Count == 6 && ps2.backlog.Count == 0 && solo2.pending.Count == 0, $"{ps2.clones.Count} clones, {ps2.backlog.Count} pendientes");
        WorkRoster onlyA = Roster(("I1", 0)); WorkRoster.DayPlan pa = onlyA.Plan(0, Ms("A", "a", 2));
        TestProbe.Check("un área sin tareas hoy (la B) no pide a nadie ni tiene clon", !pa.quota.ContainsKey("B") && pa.quota.ContainsKey("A") && pa.clones.Count == 0, string.Join(", ", pa.quota.Keys));
        WorkRoster idle = Roster(("I0", 0)); WorkRoster.DayPlan pi = idle.Plan(0, Ms(Energy, "e", 0));
        TestProbe.Check("sin ninguna tarea, ni cuota ni clones", pi.quota.Count == 0 && pi.clones.Count == 0);

        // Los de s2 y s3 pueden trabajar en la tierra; los de la tierra nunca fuera
        WorkRoster up = Roster(("U0", 2), ("U1", 1));
        WorkRoster.DayPlan pu = up.Plan(0, Ms("A", "a", 6));
        TestProbe.Check("los de s2 y s3 trabajan en la tierra si allí está el trabajo (3 h cada uno)", pu.place.Values.All(x => x.Equals(S1)) && up.workers.All(w => Mathf.Approximately(Worked(pu, w), 3f)) && pu.clones.Count == 0);
        WorkRoster down = Roster(("D0", 0));
        WorkRoster.DayPlan pdn = down.Plan(0, Ms(Energy, "e", 2));
        TestProbe.Check("los de la tierra no van fuera aunque allí haya trabajo: clon", pdn.place[down.Find("D0")].Equals(S1) && pdn.clones.Count == 1 && !pdn.work.Any(a => a.worker.name == "D0"));

        // UN ÁREA QUE NADIE SABE HACER: primero se inicia alguien en ella y la hace; el clon, solo si nadie pudo
        var ini = new WorkRoster { sanctuaryModel = true, initiationModel = true, useProfessions = false };
        ini.areas.AddRange(new[] { "A", "Planta Atómica" });
        ini.trainingAreas.Add("Sala de Yoga");
        for (int i = 0; i < 3; i++) ini.Add("N" + i).home = 1;   // (la Planta es del Marino: solo los de s2 en adelante)
        var risky = new List<WorkRoster.Mission>(Ms("A", "qui_a", 10)); risky.AddRange(Ms("A", "limpiar_a", 3)); risky.Add(new WorkRoster.Mission("Planta Atómica", "pla_operar", "Operar", 1f));
        WorkRoster.DayPlan pin = ini.Plan(0, risky);
        TestProbe.Check("la Planta, que nadie sabe, recibe su iniciación y la hace una persona (sin clon)",
            pin.lessons.Any(l => l.real.area == "Planta Atómica") && pin.work.Any(a => !a.lesson && !a.worker.clone && a.mission.area == "Planta Atómica") && !pin.cloneAreas.Contains("Planta Atómica"),
            $"clases: {string.Join(" · ", pin.lessons.Select(l => l.real.id))} · clones: {string.Join(", ", pin.cloneAreas)}");

        // UNA TAREA DE CADA DÍA QUE QUEDÓ PENDIENTE SE CUENTA UNA VEZ
        // (en el reparto puro, que aún deja pendientes; con santuarios no queda ninguno)
        var dup = new WorkRoster { useProfessions = false }; dup.areas.Add("A"); dup.Add("R0");
        dup.Plan(0, Ms("A", "d", 12));
        int pend = dup.pending.Count;
        WorkRoster.DayPlan pdup = dup.Plan(1, Ms("A", "d", 12));
        TestProbe.Check("lo pendiente de ayer que vuelve hoy no se suma dos veces (12 h, no más)", pend > 0 && Mathf.Approximately(pdup.hoursDemanded, 12f), $"{pend} pendientes · hoy {pdup.hoursDemanded} h");

        WorkRoster kushal = Roster(("Player", 2));
        WorkRoster.DayPlan pk = kushal.Plan(0, Ms("A", "k", 6));
        TestProbe.Check("Kushal, siempre en la tierra: sus 5 h y un clon la que sobra", Mathf.Approximately(Worked(pk, kushal.Find("Player")), 5f) && pk.place[kushal.Find("Player")].Equals(S1) && pk.clones.Count == 1 && pk.backlog.Count == 0);

        // EL ÁREA ENTERA: el yoga y el hospital cuentan como una, tengan los pisos que tengan
        TestProbe.Check("los pisos del yoga y del hospital son su área", WorkRoster.AreaGroup("Sala de Yoga · Hatha tradicional") == "Sala de Yoga" && WorkRoster.AreaGroup("Sala de Yoga") == "Sala de Yoga" && WorkRoster.AreaGroup(AreaWork.Hospital + " veterinario") == AreaWork.Hospital && WorkRoster.AreaGroup("Cocina") == "Cocina");
        WorkRoster nobody = Roster();
        var yoga = new List<WorkRoster.Mission>(Ms("Sala de Yoga", "y", 2)); yoga.AddRange(Ms("Sala de Yoga · Ashtanga occidental", "ya", 2));
        WorkRoster.DayPlan py = nobody.Plan(0, yoga);
        TestProbe.Check("el yoga de varios pisos: sus horas juntas y un solo clon", py.clones.Count == 1 && py.hoursByArea.Count == 1 && Mathf.Approximately(py.hoursByArea["Sala de Yoga"], 4f) && py.quota["Sala de Yoga"] == 1, $"{py.clones.Count} clones · {string.Join(", ", py.hoursByArea.Select(kv => $"{kv.Key} {kv.Value} h"))}");

        // ── 4. Cambia cada día y con los datos ──
        WorkRoster six = Roster(("P0", 2), ("P1", 2), ("P2", 2), ("P3", 2), ("P4", 2), ("P5", 2));
        bool differs = true;
        for (int d = 0; d < 30; d++) differs &= !six.DayOrder(d).SequenceEqual(six.DayOrder(d + 1));
        TestProbe.Check("el orden del personal cambia todos los días (30 días)", differs);
        WorkRoster three = Roster(("Q0", 0), ("Q1", 0), ("Q2", 0));
        TestProbe.Check("también con 3 personas", !three.DayOrder(4).SequenceEqual(three.DayOrder(5)));
        var maps = new HashSet<string>();
        for (int d = 0; d < 7; d++)
        {
            var dm = new List<WorkRoster.Mission>(); dm.AddRange(Ms("A", "a", 4)); dm.AddRange(Ms(Energy, "e", 4)); dm.AddRange(Ms(Line, "l", 4));
            WorkRoster.DayPlan pd = six.Plan(d, dm);
            maps.Add(string.Join(",", six.workers.Select(w => pd.place[w].sanctuary)));
        }
        TestProbe.Check("quién va a cada santuario cambia entre días", maps.Count >= 2, $"{maps.Count} repartos distintos en 7 días");
        var fixedOrder = new WorkRoster { useProfessions = false }; for (int i = 0; i < 6; i++) fixedOrder.Add("F" + i);
        TestProbe.Check("apagado el reparto por santuarios, el orden fijo de siempre", fixedOrder.DayOrder(3).SequenceEqual(fixedOrder.workers) && fixedOrder.DayOrder(4).SequenceEqual(fixedOrder.workers));

        WorkRoster.DayPlan Live(List<(string, int)> people, List<WorkRoster.Mission> ms) => Roster(people.ToArray()).Plan(0, ms);
        var crew = new List<(string, int)> { ("L1", 0) };
        var acts = new List<WorkRoster.Mission>(Ms("A", "a", 2)); acts.AddRange(Ms(Energy, "e", 2));
        WorkRoster.DayPlan v0 = Live(crew, acts);
        acts.AddRange(Ms(Control, "c", 1));
        WorkRoster.DayPlan v1 = Live(crew, acts);
        crew.Add(("Ae", 2));
        WorkRoster.DayPlan v2 = Live(crew, acts);
        TestProbe.Check("una tarea nueva en un área sin nadie sube las horas y los clones", v0.clones.Count == 1 && v1.clones.Count == 2 && v1.hoursAt[S3] > v0.hoursAt[S3] + 0.5f, $"{v0.clones.Count} → {v1.clones.Count} clones · {v0.hoursAt[S3]} → {v1.hoursAt[S3]} h");
        TestProbe.Check("una persona nueva que llega allí quita los clones", v2.clones.Count == 0, $"{v2.clones.Count} clones");

        // ── 5. Fuera de la escena, el horario y las franjas ──
        TestProbe.Check("quien pasa el día fuera se apaga; Kushal y la tierra no; en la fiesta, nadie", RemoteStaff.ShouldHide("M", S2, false) && RemoteStaff.ShouldHide("Ae", S3, false) && !RemoteStaff.ShouldHide("L1", S1, false) && !RemoteStaff.ShouldHide("Player", S3, false) && !RemoteStaff.ShouldHide("Ae", S3, true));
        var bodies = ro.workers.ToDictionary(w => w.name, w => new GameObject("Reparto_TEST_" + w.name));
        bool was = RemoteStaff.InTests; RemoteStaff.InTests = true;
        RemoteStaff.Apply(p, n => bodies.TryGetValue(n, out GameObject g) ? g : null, false);
        bool hid = !bodies["M"].activeSelf && !bodies["Ae"].activeSelf && bodies["L1"].activeSelf && RemoteStaff.Hidden(bodies["Ae"]);
        RemoteStaff.Apply(p, n => bodies.TryGetValue(n, out GameObject g) ? g : null, true);
        bool back = bodies.Values.All(g => g.activeSelf) && !RemoteStaff.Hidden(bodies["Ae"]);
        RemoteStaff.InTests = was;
        foreach (GameObject g in bodies.Values) Destroy(g);
        TestProbe.Check("se apagan los de fuera (no se simulan)", hid);
        TestProbe.Check("y vuelven todos para la fiesta", back);

        TestProbe.Check("el tiempo libre empieza a las 16:00 para todos", LeisureRoster.BlockAt(15.9f) == "jornada" && LeisureRoster.BlockAt(16.1f) == "libre");

        // ── En la escena (si hay jornada): lo que da el reparto de verdad, para el informe ──
        WorkdayDirector wd = FindAnyObjectByType<WorkdayDirector>();
        if (wd != null && wd.today != null && wd.roster.sanctuaryModel)
        {
            WorkRoster.DayPlan t = wd.today;
            foreach (var kv in t.hoursByArea.OrderBy(k => k.Key)) Debug.Log($"[Reparto] {kv.Key} ({SanctuaryPlacement.PlaceOfArea(kv.Key)}): {kv.Value:0.#} h, pide {t.quota[kv.Key]} persona(s).");
            foreach (string s in WorkRoster.Balance(t, wd.roster.workers.Count)) Debug.Log("[Reparto] " + s);
            TestProbe.Check("en el santuario: nadie trabaja fuera de su sitio ni más allá de su casa", t.work.Where(a => !a.worker.clone && t.place.ContainsKey(a.worker)).All(a => t.place[a.worker].Equals(SanctuaryPlacement.PlaceOfArea(a.mission.area)) && SanctuaryPlacement.CanAccess(a.worker.home, t.place[a.worker])));
            TestProbe.Check("en el santuario: cada clon tiene su área apuntada", t.clones.Count == t.cloneAreas.Count, $"{t.clones.Count} clones · {string.Join(", ", t.cloneAreas)}");
            TestProbe.Check("en el santuario: nada pasa a mañana ni queda imposible", t.backlog.Count == 0 && t.impossible.Count == 0 && wd.roster.pending.Count == 0, string.Join(", ", t.impossible.Take(6).Select(x => x.id)));
            TestProbe.Check("en el santuario: nadie pasa de 5 h ni trabaja después de las 16:00", t.work.Where(a => !a.worker.clone).GroupBy(a => a.worker).All(g => g.Sum(a => a.mission.hours) <= WorkRoster.WorkHours + 1e-3f) && t.work.All(a => a.start + a.mission.hours <= WorkRoster.StartHour + 8f + 1e-3f));
            foreach (var kv in t.hoursAt)
            {
                var hs = t.place.Where(x => x.Value.Equals(kv.Key)).Select(x => t.Of(x.Key).Sum(a => a.mission.hours)).ToList();
                if (hs.Count > 0) Debug.Log($"[Reparto] {kv.Key}: {hs.Count} personas, parte {t.share[kv.Key]:0.#} h, de {hs.Min():0.#} a {hs.Max():0.#} h (con iniciaciones, puestos y grupos).");
            }
        }
        yield return null;
        TestProbe.End();
    }
}
