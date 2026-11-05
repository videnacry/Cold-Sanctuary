using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DEL MODELO JORNADA/TAREA (Berón 2026-11-05; docs/jornada-y-rotacion.md):
/// 1. Entrenamiento 8–9 y 14–15; 5 h de tareas en 9–11, 12–14 y 15–16.
/// 2. Las iniciaciones se reparten primero y de quien menos sabe a quien más: un recién llegado, toda su jornada; después, la mitad; luego, lo que falte.
/// 3. Lo aprendido se puede hacer a partir de esa hora; una tarea con riesgo no va a quien no la sabe; la obligatoria la cubre la Magnate.
/// 4. Inicia quien sepa la tarea y sepa iniciar (por defecto), y la Magnate si no hay nadie. Saber iniciar se aprende, y Kushal no da clases.
/// 5. Sin sus requisitos no se reparte. Nadie pasa de 5 h, nada cruza la comida ni el entrenamiento, y los grupos van a la vez.
/// 6. Todos acaban sabiéndolo todo (regla de oro).
/// </summary>
public class JornadaSaberTest : MonoBehaviour, ITestUnit
{
    public int Group => 104;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<JornadaSaberTest>() == null) tr.gameObject.AddComponent<JornadaSaberTest>();
    }

    static WorkRoster New(int people)
    {
        var r = new WorkRoster { initiationModel = true, useProfessions = false };
        r.areas.AddRange(new[] { "A", "B" });
        r.trainingAreas.Add("Sala de Yoga");
        for (int i = 0; i < people; i++) r.Add("P" + i);
        return r;
    }

    // Aprendiendo, no enseñando: dar una clase no cuenta para el tope de iniciaciones de nadie.
    static bool Learning(WorkRoster.Assignment a) => a.lesson && !a.mission.id.StartsWith(WorkRoster.InstructorPrefix);

    static bool InSlot(WorkRoster.Assignment a)
    {
        float s = a.start, e = a.start + a.mission.hours;
        return (s >= 9f && e <= 11f + 1e-3f) || (s >= 12f && e <= 14f + 1e-3f) || (s >= 15f && e <= 16f + 1e-3f);
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Modelo jornada/tarea: iniciaciones como tareas, primero a quien menos sabe, y requisitos de cada misión");
        List<WorkRoster.Mission> Day(int n = 8)
        {
            var l = new List<WorkRoster.Mission>
            {
                new WorkRoster.Mission("B", "pla_operar", "Operar el reactor", 1f, true),
                new WorkRoster.Mission("A", "qui_sintesis", "Síntesis", 1f),
                new WorkRoster.Mission("A", "limpiar", "Limpiar", 1f),
                new WorkRoster.Mission("B", "barrer", "Barrer", 2f),
            };
            for (int i = 0; i < n; i++) l.Add(new WorkRoster.Mission(i % 2 == 0 ? "A" : "B", "bio_t" + i, "Tarea " + i, 1f));
            return l;
        }

        // ── 1 y 2. Una jornada ──
        WorkRoster ro = New(3);
        WorkRoster.Worker sabia = ro.Add("Sabia");
        foreach (var m in Day()) sabia.taught.Add(m.id);
        WorkRoster.DayPlan first = ro.Plan(0, Day());   // (el personal de siempre: el día 0 no es «recién llegado»)
        TestProbe.Check("entrenan a las 8 y a las 14, y nada de trabajo a esas horas", ro.workers.All(w => first.morning.ContainsKey(w) && first.training.ContainsKey(w)) && first.work.All(InSlot), string.Join(" · ", first.work.Where(a => !InSlot(a))));
        TestProbe.Check("nadie pasa de 5 h", ro.workers.All(w => first.Of(w).Sum(a => a.mission.hours) <= WorkRoster.WorkHours + 1e-3f));
        TestProbe.Check("sin ser nuevo, como mucho la mitad en iniciaciones", ro.workers.All(w => first.Of(w).Count(Learning) <= WorkRoster.InitiationHalf + 1e-3f));
        TestProbe.Check("quien lo sabe todo no se inicia en tareas", !first.Of(sabia).Any(a => Learning(a) && !a.mission.id.EndsWith(WorkRoster.InstructSkill)));
        TestProbe.Check("hay lista de iniciaciones", first.lessons.Count > 0);
        TestProbe.Check("cada iniciación va antes que su tarea", first.work.Where(a => !a.lesson && a.mission.id == "qui_sintesis" && a.worker != sabia).All(a => first.Of(a.worker).Any(x => x.lesson && x.mission.id.EndsWith("qui_sintesis") && x.start < a.start)));
        TestProbe.Check("nadie opera lo que no sabe", first.work.Where(a => !a.lesson && a.mission.id == "pla_operar").All(a => a.worker.clone || ro.Learned(a.worker, a.mission)));

        WorkRoster.Worker nuevo = ro.Add("Nuevo");
        WorkRoster.DayPlan day1 = ro.Plan(1, Day());
        TestProbe.Check("el recién llegado, toda su primera jornada en iniciaciones", day1.Of(nuevo).Count(Learning) == 5, $"{day1.Of(nuevo).Count(Learning)}");
        TestProbe.Check("él va primero: el que menos sabe", day1.lessons.First().learners.Contains(nuevo));
        WorkRoster.DayPlan day2 = ro.Plan(2, Day(30));
        int lessons2 = day2.Of(nuevo).Count(Learning);
        TestProbe.Check("el segundo día, como mucho la mitad", lessons2 >= 1 && lessons2 <= WorkRoster.InitiationHalf + 1e-3f, $"{lessons2}");
        TestProbe.Check("y sigue con lo que sabe", day2.Of(nuevo).Any(a => !a.lesson));

        // ── Una tarea con riesgo ──
        WorkRoster.Mission reactor = new WorkRoster.Mission("B", "pla_turno_nuevo", "Operar", 1f);
        WorkRoster ro2 = New(2);
        ro2.workers[0].taught.Add("pla_turno_nuevo");
        TestProbe.Check("el que no la sabe no puede; el que sí, puede", !ro2.Qualified(ro2.workers[1], reactor) && ro2.Qualified(ro2.workers[0], reactor));
        TestProbe.Check("una tarea sin riesgo se aprende haciéndola", ro2.Qualified(ro2.workers[1], new WorkRoster.Mission("A", "limpiar_aseos", "Limpiar", 1f)));
        WorkRoster off = New(1); off.initiationModel = false;
        TestProbe.Check("apagado, reparte como siempre", off.Qualified(off.workers[0], reactor));
        float c;
        TestProbe.Check("apagado, las franjas de siempre", off.Place(0f, 3f, out c) == 0f && c == 8f && off.Place(3f, 2f, out c) == 3f && c == 12f);
        TestProbe.Check("las franjas nuevas: 2 h a las 9, a las 12; 1 h a las 15; 3 h no caben", ro2.Place(0f, 2f, out c) == 0f && c == 9f && ro2.Place(1f, 2f, out c) == 2f && c == 12f && ro2.Place(4f, 1f, out c) == 4f && c == 15f && ro2.Place(0f, 3f, out c) < 0f);

        // ── 4. Quién inicia ──
        WorkRoster ro4 = New(3); ro4.instructorsTeach = false;
        ro4.workers[0].taught.Add("qui_x"); ro4.workers[0].taught.Add(WorkRoster.InstructSkill);
        WorkRoster.DayPlan m0 = ro4.Plan(0, new List<WorkRoster.Mission> { new WorkRoster.Mission("A", "qui_x", "X", 1f) });
        TestProbe.Check("apagado, inicia la Magnate", m0.lessons.Count > 0 && m0.lessons.All(l => l.instructor == null && l.teacher.StartsWith("Magnate")));
        WorkRoster ro4b = New(3);
        ro4b.workers[0].taught.Add("qui_x"); ro4b.workers[0].taught.Add(WorkRoster.InstructSkill);
        WorkRoster.DayPlan m1 = ro4b.Plan(0, new List<WorkRoster.Mission> { new WorkRoster.Mission("A", "qui_x", "X", 1f) });
        TestProbe.Check("por defecto, inicia quien sabe y sabe iniciar", ro4b.instructorsTeach && m1.lessons.Any(l => l.instructor == ro4b.workers[0]) && m1.Of(ro4b.workers[0]).Any(a => a.lesson && a.mission.id.StartsWith(WorkRoster.InstructorPrefix)));
        WorkRoster ro4c = New(2);
        WorkRoster.DayPlan m2 = ro4c.Plan(0, new List<WorkRoster.Mission> { new WorkRoster.Mission("A", "qui_x", "X", 1f) });
        TestProbe.Check("sin nadie que sepa, la Magnate", m2.lessons.All(l => l.instructor == null));
        WorkRoster ro4d = New(2); WorkRoster.Worker yo = ro4d.Add("Player");
        yo.taught.Add("qui_x"); yo.taught.Add(WorkRoster.InstructSkill);
        WorkRoster.DayPlan m3 = ro4d.Plan(0, new List<WorkRoster.Mission> { new WorkRoster.Mission("A", "qui_x", "X", 1f) });
        TestProbe.Check("Kushal no da clases: va la Magnate", m3.lessons.Count > 0 && m3.lessons.All(l => l.instructor == null));

        // ── Clases ──
        WorkRoster big = New(WorkRoster.LessonGroupSize * WorkRoster.MaxLessonGroups + 4);
        var many = new List<WorkRoster.Mission>();
        for (int i = 0; i < 12; i++) many.Add(new WorkRoster.Mission("A", "qui_t" + i, "Tarea " + i, 1f));
        WorkRoster.DayPlan bp = big.Plan(0, many);
        TestProbe.Check("cada clase cabe en su maestro", bp.lessons.All(l => l.learners.Count <= WorkRoster.LessonGroupSize));
        TestProbe.Check("por hora, con tope", bp.lessons.GroupBy(l => l.start).All(g => g.Count() <= WorkRoster.MaxLessonGroups));
        TestProbe.Check("nadie en dos sitios a la vez", big.workers.All(w => bp.Of(w).Select(a => a.start).Distinct().Count() == bp.Of(w).Count()));

        // ── 5. Requisitos y grupos ──
        WorkRoster ro3 = New(2);
        ro3.worldCheck = m => m.id == "limpiar" ? "EstadoDelArea: en obras" : null;
        WorkRoster.DayPlan p3 = ro3.Plan(0, Day());
        TestProbe.Check("sin requisitos no se reparte y queda el motivo", !p3.work.Any(a => a.mission.id.EndsWith("limpiar")) && p3.blocked.Any(b => b.mission.id == "limpiar" && b.why.Contains("obras")));
        TestProbe.Check("el balance lo cuenta", WorkRoster.Balance(p3, 2).Any(s => s.Contains("requisitos")));
        TestProbe.Check("una misión de obra no pide que el área esté terminada", !MissionRequirements.Of(new WorkRoster.Mission("Construcción", "obra_trabajo_x", "Obra", 2f)).Any(r => r.kind == MissionRequirements.Kind.EstadoDelArea));
        NuclearSite ns = FindAnyObjectByType<NuclearSite>();
        if (ns != null)
        {
            ns.DeclareEmergency("prueba");
            TestProbe.Check("en emergencia no se opera el reactor, pero se vigila", MissionRequirements.Blocked(new WorkRoster.Mission("Planta Atómica", "pla_operar", "x", 1f)) != null && MissionRequirements.Blocked(new WorkRoster.Mission("Central Nuclear", "nuc_vigilancia", "x", 1f)) == null);
            ns.ClearEmergency();
        }
        WorkRoster ro6 = New(6);
        var grp = new List<WorkRoster.Mission>();
        for (int i = 0; i < 3; i++) grp.Add(new WorkRoster.Mission("A", "yog_rol" + i, "Rol " + i, 1f, true) { group = "clase" });
        for (int i = 0; i < 6; i++) grp.Add(new WorkRoster.Mission("B", "qui_y" + i, "Y " + i, 1f));
        WorkRoster.DayPlan pg = ro6.Plan(0, grp);
        var roles = pg.work.Where(a => !a.lesson && a.mission.group == "clase").ToList();
        TestProbe.Check("un grupo va entero y a la vez", roles.Count == 3 && roles.Select(a => a.start).Distinct().Count() == 1, string.Join(" · ", roles.Select(a => a.start)));

        // ── 6. Regla de oro ──
        WorkRoster ro5 = New(3);
        WorkRoster.DayPlan last = null;
        for (int d = 0; d < 40; d++) last = ro5.Plan(d, Day());
        TestProbe.Check("acaban sabiéndolo todo, también iniciar", ro5.workers.All(w => Day().All(m => ro5.Learned(w, m)) && w.taught.Contains(WorkRoster.InstructSkill)));
        TestProbe.Check("sin nada que aprender no hay iniciaciones y todo son tareas", last.lessons.Count == 0);
        TestProbe.Check("la familia de misiones se aprende una vez", MissionRequirements.KnowledgeKey("obra_trabajo_Ballet") == MissionRequirements.KnowledgeKey("obra_trabajo_Dojo"));

        // ── En la escena ──
        TestProbe.Check("a las 8 hay qué hacer en el gimnasio y en yoga", Performances.ForTraining("Gimnasio") != null && Performances.ForTraining("Sala de Yoga") != null);
        TestProbe.Check("la iniciación usa los pasos de la tarea", WorkRoster.LessonBase(WorkRoster.LessonPrefix + "hue_riego") == "hue_riego" && WorkRoster.LessonBase(WorkRoster.InstructorPrefix + "hue_riego") == "hue_riego" && PlayerTasks.For(WorkRoster.LessonPrefix + "hue_riego", "Huerto", "x").authored);
        TestProbe.Check("saber iniciar tiene pasos y actuación", PlayerTasks.AuthoredIds.Contains(WorkRoster.InstructSkill) && Performances.OwnMissions.Contains(WorkRoster.InstructSkill));
        TestProbe.Check("las rondas de frutales caben en una franja", FruitGardener.MaxRoundHours <= 2f && FruitGardener.RoundTarget < FruitGardener.MaxRoundHours);
        WorkdayDirector wd = FindAnyObjectByType<WorkdayDirector>();
        if (wd != null && wd.today != null)
        {
            WorkRoster.DayPlan t = wd.PlanDay(wd.plannedDay + 1);
            Debug.Log($"[Jornada] Prueba: {t.work.Count(a => !a.lesson)} tareas, {t.lessons.Count} iniciaciones, {t.clones.Count} clones, {t.blocked.Count} sin requisitos, {t.unqualified.Count} sin nadie que sepa, {t.backlog.Count} pendientes.");
            TestProbe.Check("en el santuario hay entrenamiento e iniciaciones", t.morning.Count > 0 && t.lessons.Count > 0, string.Join(" · ", t.lessons.Take(6)));
            TestProbe.Check("en el santuario, nada fuera de las franjas", t.work.Where(a => !a.worker.clone).All(InSlot), string.Join(" · ", t.work.Where(a => !InSlot(a)).Take(6)));
        }
        TestProbe.End();
        yield break;
    }
}
