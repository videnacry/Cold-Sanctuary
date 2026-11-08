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
        TestProbe.Check("los nueve de siempre son santuario 1 nivel 1", SanctuaryPlacement.Originals.Length == 9 && new[] { "Player", "Linduarda", "Lizmibet", "Goluis", "Panterilia", "Gohageneis", "Irosene", "JefeConstruccion", "Andrea" }.All(SanctuaryPlacement.IsOriginal));
        TestProbe.Check("la tabla de umbrales: 3 → s1n1, 6 → s2n1, 9 → s3n1, 12 → s1n2", SanctuaryPlacement.PositionFor(3) == 0 && SanctuaryPlacement.PositionFor(6) == 1 && SanctuaryPlacement.PositionFor(9) == 2 && SanctuaryPlacement.PositionFor(12) == 3 && SanctuaryPlacement.PositionFor(5) == 0);
        bool rising = true; for (int s = 0; s < 30; s++) rising &= SanctuaryPlacement.PositionFor(s + 1) >= SanctuaryPlacement.PositionFor(s);
        TestProbe.Check("más puntuación nunca baja de sitio", rising);
        TestProbe.Check("posición y casa: s3n1 es la 2 y la 3 es s1n2 (acabar la vuelta sube el nivel)", S3.Position == 2 && SanctuaryPlacement.Home.At(3).Equals(S1N2) && SanctuaryPlacement.Home.At(2).Equals(S3));
        TestProbe.Check("s1n1 no va a s1n2 ni a s2n1", !SanctuaryPlacement.CanAccess(S1, S1N2) && !SanctuaryPlacement.CanAccess(S1, S2) && SanctuaryPlacement.CanAccess(S1, S1));
        TestProbe.Check("s1n2 va a s1n1, s2n1 y s3n1", SanctuaryPlacement.CanAccess(S1N2, S1) && SanctuaryPlacement.CanAccess(S1N2, S2) && SanctuaryPlacement.CanAccess(S1N2, S3));
        TestProbe.Check("s2n1 va a s1n1 pero no a s3n1", SanctuaryPlacement.CanAccess(S2, S1) && !SanctuaryPlacement.CanAccess(S2, S3));
        TestProbe.Check("el santuario de cada área: el Aéreo, el Marino y la tierra", SanctuaryPlacement.SanctuaryOfArea(Energy) == 3 && SanctuaryPlacement.SanctuaryOfArea(Control) == 3 && SanctuaryPlacement.SanctuaryOfArea(Line) == 2 && SanctuaryPlacement.SanctuaryOfArea("Cocina") == 1 && SanctuaryPlacement.SanctuaryOfArea(AreaWork.Hospital) == 1);
        TestProbe.Check("sin margas en el cuerpo, la tabla de inicio (Fernando al Aéreo)", SanctuaryPlacement.PositionFor(SanctuaryPlacement.SoulSum(null, "Fernando")) == 2 && SanctuaryPlacement.PositionFor(SanctuaryPlacement.SoulSum(null, "Nadie_TEST")) == SanctuaryPlacement.PositionFor(SanctuaryPlacement.DefaultStartingScore));

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
        TestProbe.Check("Ae estira su jornada 1 h y acaba todo el Aéreo", Mathf.Approximately(p.Overtime(ae), 1f) && p.Of(ae).Sum(a => a.mission.hours) >= 6f - 1e-3f && !p.backlog.Any(x => x.area == Energy), $"+{p.Overtime(ae)} h, {p.Of(ae).Count()} tareas");
        TestProbe.Check("lo estirado va después de las 16:00 y la jornada acaba más tarde", p.Of(ae).Any(a => a.start >= 16f - 1e-3f) && Mathf.Approximately(p.EndOf(ae), 17f), string.Join(" · ", p.Of(ae).Select(a => $"{a.start:0.#}")));
        TestProbe.Check("sin áreas vacías, la Magnate no se divide", p.clones.Count == 0 && p.backlog.Count == 0, $"{p.clones.Count} clones, {p.backlog.Count} pendientes");
        TestProbe.Check("el que sobraba tras cubrir las horas va al segundo reparto", p.secondSplit.Count == 1 && p.secondSplit[0].home == 0, string.Join(", ", p.secondSplit.Select(w => w.name)));
        TestProbe.Check("el informe dice las horas de cada santuario y quién se estira", WorkRoster.Balance(p, ro.workers.Count).Any(s => s.StartsWith("santuario 3 nivel 1")) && WorkRoster.Balance(p, ro.workers.Count).Any(s => s.StartsWith("Jornada estirada")));

        // Clones solo para las áreas sin nadie
        WorkRoster lone = Roster(("L1", 0), ("L2", 0));
        var far = new List<WorkRoster.Mission>();
        far.AddRange(Ms("A", "a", 4)); far.AddRange(Ms(Energy, "e", 2)); far.AddRange(Ms(Control, "c", 1)); far.AddRange(Ms(Line, "l", 1));
        WorkRoster.DayPlan pf = lone.Plan(0, far);
        TestProbe.Check("un clon por cada área sin nadie (Energía, Control de misión, Línea), y ninguno en la tierra",
            pf.clones.Count == 3 && new HashSet<string>(pf.cloneAreas).SetEquals(new[] { Energy, Control, Line }), $"{pf.clones.Count}: {string.Join(", ", pf.cloneAreas)}");
        TestProbe.Check("cada clon, en una sola área", pf.clones.All(c => pf.Of(c).Select(a => a.mission.area).Distinct().Count() == 1));
        WorkRoster solo = Roster(("Solo", 0));
        WorkRoster.DayPlan ps = solo.Plan(0, Ms("A", "x", 6, 2f, true));
        WorkRoster.Worker so = solo.Find("Solo");
        TestProbe.Check("las obligatorias que no caben ni estirando hasta la cena: un clon para ellas, ninguna a mañana",
            ps.clones.Count == 1 && ps.mandatoryClones == 1 && ps.backlog.Count == 0 && ps.Overtime(so) <= WorkRoster.MaxOvertime + 1e-3f && ps.Overtime(so) > 0f && WorkRoster.Balance(ps, 1).Any(s => s.StartsWith("Faltan personas")), $"+{ps.Overtime(so)} h, {ps.clones.Count} clones, {ps.backlog.Count} pendientes");
        WorkRoster solo2 = Roster(("Solo", 0));
        WorkRoster.DayPlan ps2 = solo2.Plan(0, Ms("A", "y", 6, 2f));
        TestProbe.Check("las no obligatorias que no caben: se estira hasta la cena y el resto pasa a mañana, sin clones", ps2.clones.Count == 0 && ps2.backlog.Count == 3, $"{ps2.clones.Count} clones, {ps2.backlog.Count} pendientes");
        WorkRoster onlyA = Roster(("I1", 0)); WorkRoster.DayPlan pa = onlyA.Plan(0, Ms("A", "a", 2));
        TestProbe.Check("un área sin tareas hoy (la B) no pide a nadie ni tiene clon", !pa.quota.ContainsKey("B") && pa.quota.ContainsKey("A") && pa.clones.Count == 0, string.Join(", ", pa.quota.Keys));
        WorkRoster idle = Roster(("I0", 0)); WorkRoster.DayPlan pi = idle.Plan(0, Ms(Energy, "e", 0));
        TestProbe.Check("sin ninguna tarea, ni cuota ni clones", pi.quota.Count == 0 && pi.clones.Count == 0);

        // ACABAR UN SANTUARIO: 30 días vividos en cada uno hasta el suyo, y sube de nivel
        var lived = new Dictionary<int, int>();
        SanctuaryPlacement.Home h = S2;
        for (int i = 0; i < SanctuaryPlacement.DaysToFinish; i++) h = SanctuaryPlacement.AfterDay(lived, h, S2);
        bool notYet = h.Equals(S2);
        for (int i = 0; i < SanctuaryPlacement.DaysToFinish - 1; i++) h = SanctuaryPlacement.AfterDay(lived, h, S1);
        bool stillNot = h.Equals(S2);
        h = SanctuaryPlacement.AfterDay(lived, h, S1);
        TestProbe.Check($"s2n1 sube a s2n2 al acabar s1 y s2 ({SanctuaryPlacement.DaysToFinish} días cada uno), no antes", notYet && stillNot && h.Equals(new SanctuaryPlacement.Home(2, 2)) && lived.Count == 0, h.ToString());
        var l1 = new Dictionary<int, int>(); SanctuaryPlacement.Home k1 = S1;
        for (int i = 0; i < SanctuaryPlacement.DaysToFinish; i++) k1 = SanctuaryPlacement.AfterDay(l1, k1, i % 2 == 0 ? S1 : S1N2);
        TestProbe.Check("los días en otro nivel no cuentan; con 30 en s1, s1n1 pasa a s1n2", k1.Equals(S1) && SanctuaryPlacement.Home.At(SanctuaryPlacement.AfterDay(new Dictionary<int, int> { { 1, SanctuaryPlacement.DaysToFinish - 1 } }, S1, S1).Position).Equals(S1N2));
        TestProbe.Check("en las pruebas no se cuenta ni se guarda", SanctuaryPlacement.LiveDay("Reparto_TEST", 5, 0, S1) == 0 && SanctuaryPlacement.DaysIn("Reparto_TEST", 1) == 0);
        WorkRoster kushal = Roster(("Player", 2));
        WorkRoster.DayPlan pk = kushal.Plan(0, Ms("A", "k", 6));
        TestProbe.Check("Kushal no se estira y siempre está en la tierra", pk.Overtime(kushal.Find("Player")) == 0f && pk.place[kushal.Find("Player")].Equals(S1) && pk.backlog.Count == 1);

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

        float c;
        var old = new WorkRoster();
        TestProbe.Check("reparto puro: lo estirado es una franja nueva a las 16:00", old.Place(5f, 1f, out c, 1f) == 5f && Mathf.Approximately(c, 16f) && old.Place(5f, 1f, out c) < 0f && Mathf.Approximately(old.UsedAt(16.5f, 1f), 5.5f));
        var nw = new WorkRoster { initiationModel = true };
        TestProbe.Check("modelo nuevo: la franja de las 15 sigue después de las 16:00", nw.Place(4f, 2f, out c, 1f) == 4f && Mathf.Approximately(c, 15f) && nw.Place(4f, 2f, out c) < 0f);
        TestProbe.Check("el horario libre ve la jornada estirada", LeisureRoster.BlockAt(16.5f, 17f) == "jornada" && LeisureRoster.BlockAt(16.5f) == "libre" && LeisureRoster.BlockAt(17.5f, 17f) == "libre");

        // ── En la escena (si hay jornada): lo que da el reparto de verdad, para el informe ──
        WorkdayDirector wd = FindAnyObjectByType<WorkdayDirector>();
        if (wd != null && wd.today != null && wd.roster.sanctuaryModel)
        {
            WorkRoster.DayPlan t = wd.today;
            foreach (var kv in t.hoursByArea.OrderBy(k => k.Key)) Debug.Log($"[Reparto] {kv.Key} ({SanctuaryPlacement.PlaceOfArea(kv.Key)}): {kv.Value:0.#} h, pide {t.quota[kv.Key]} persona(s).");
            foreach (string s in WorkRoster.Balance(t, wd.roster.workers.Count)) Debug.Log("[Reparto] " + s);
            TestProbe.Check("en el santuario: nadie trabaja fuera de su sitio ni más allá de su casa", t.work.Where(a => !a.worker.clone && t.place.ContainsKey(a.worker)).All(a => t.place[a.worker].Equals(SanctuaryPlacement.PlaceOfArea(a.mission.area)) && SanctuaryPlacement.CanAccess(a.worker.home, t.place[a.worker])));
            TestProbe.Check("en el santuario: los clones son solo de áreas sin nadie", t.clones.Count == t.cloneAreas.Count, $"{t.clones.Count} clones · {string.Join(", ", t.cloneAreas)}");
        }
        yield return null;
        TestProbe.End();
    }
}
