using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA DE LA MAÑANA NUEVA Y LOS REQUISITOS (Berón 2026-11-05; docs/jornada-y-rotacion.md):
/// 1. 8–9 entrenamiento para todos; 9–10 lección solo si le queda algo por aprender; quien lo sabe todo trabaja a las 9.
/// 2. La Magnate enseña por grupos y se divide, con tope: quien no cabe trabaja.
/// 3. Lo que aprende a las 9 lo puede hacer el mismo día; una tarea con riesgo no va a quien no la sabe; la obligatoria la cubre la Magnate.
/// 4. Sin sus requisitos no se reparte y queda el motivo. Nadie pasa de 5 h, ninguna misión cruza la comida.
/// 5. Todos acaban sabiéndolo todo (regla de oro) y entonces desaparece la lección.
/// </summary>
public class JornadaSaberTest : MonoBehaviour, ITestUnit
{
    public int Group => 102;
    public bool ParallelSafe => false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<JornadaSaberTest>() == null) tr.gameObject.AddComponent<JornadaSaberTest>();
    }

    static WorkRoster New(int people)
    {
        var r = new WorkRoster { dailyLearning = true, useProfessions = false };
        r.areas.AddRange(new[] { "A", "B" });
        r.trainingAreas.Add("Sala de Yoga");
        for (int i = 0; i < people; i++) r.Add("P" + i);
        return r;
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("La mañana nueva: entrenamiento, lección con la Magnate y requisitos de cada misión");
        List<WorkRoster.Mission> Day() => new List<WorkRoster.Mission>
        {
            new WorkRoster.Mission("A", "limpiar", "Limpiar", 1f),
            new WorkRoster.Mission("A", "qui_sintesis", "Síntesis", 1f),
            new WorkRoster.Mission("B", "barrer", "Barrer", 1f),
            new WorkRoster.Mission("B", "pla_operar", "Operar el reactor", 1f, true),
        };

        // ── 1 y 3. La mañana ──
        WorkRoster ro = New(3);
        WorkRoster.DayPlan p = ro.Plan(0, Day());
        WorkRoster.Worker w0 = ro.workers[0];
        TestProbe.Check("todos entrenan de 8 a 9", ro.workers.All(w => p.morning.ContainsKey(w)) && !p.work.Any(a => a.start < 9f && !a.worker.clone));
        TestProbe.Check("a las 9 aprenden con la Magnate", p.lessons.Count >= 1 && p.work.Any(a => a.lesson && a.start == 9f && a.mission.id.StartsWith(WorkRoster.LessonPrefix)), string.Join(" · ", p.lessons));
        TestProbe.Check("primero lo que nadie sabe, con riesgo y obligatorio", p.lessons[0].mission.id == WorkRoster.LessonPrefix + "pla_operar", p.lessons[0].mission.id);
        TestProbe.Check("lo aprendido a las 9 se puede hacer hoy", p.work.Any(a => !a.lesson && a.mission.id == "pla_operar" && !a.worker.clone));
        TestProbe.Check("la lección dura 1 h", p.lessons.All(l => l.mission.hours == WorkRoster.LessonHours));
        TestProbe.Check("nadie pasa de sus 5 h con el entrenamiento", ro.workers.All(w => WorkRoster.MorningTraining + p.Of(w).Sum(a => a.mission.hours) <= WorkRoster.WorkHours + 1e-3f));
        TestProbe.Check("ninguna misión cruza la comida", p.work.All(a => a.start + a.mission.hours <= 11f + 1e-3f || a.start >= 12f));

        // ── Una tarea con riesgo no va a quien no la sabe ──
        WorkRoster ro2 = New(2);
        ro2.workers[0].taught.Add("pla_operar");
        ro2.workers[1].taught.Add("limpiar");
        ro2.workers[1].taught.Add("barrer");
        var solo = new List<WorkRoster.Mission> { new WorkRoster.Mission("B", "pla_operar", "Operar", 1f) };
        WorkRoster.Mission reactor = solo[0];
        TestProbe.Check("el que no la sabe no puede", !ro2.Qualified(ro2.workers[1], reactor));
        TestProbe.Check("el que la sabe, sí", ro2.Qualified(ro2.workers[0], reactor));
        TestProbe.Check("una tarea sin riesgo se aprende haciéndola", ro2.Qualified(ro2.workers[1], new WorkRoster.Mission("A", "limpiar_aseos", "Limpiar", 1f)));
        WorkRoster off = New(1); off.dailyLearning = false;
        TestProbe.Check("apagado, reparte como siempre", off.Qualified(off.workers[0], reactor));

        // ── 2. Grupos: cada clase, hasta 6; con tope ──
        WorkRoster big = New(WorkRoster.LessonGroupSize * WorkRoster.MaxLessonGroups + 4);
        var many = new List<WorkRoster.Mission>();
        for (int i = 0; i < 12; i++) many.Add(new WorkRoster.Mission("A", "qui_t" + i, "Tarea " + i, 1f));
        WorkRoster.DayPlan bp = big.Plan(0, many);
        TestProbe.Check("cada clase cabe en una Magnate", bp.lessons.All(l => l.learners.Count <= WorkRoster.LessonGroupSize));
        TestProbe.Check("se divide, con tope", bp.lessons.Count == WorkRoster.MaxLessonGroups && bp.lessons.Select(l => l.teacher).Distinct().Count() == bp.lessons.Count);
        TestProbe.Check("los que no caben quedan contados y trabajan", bp.waitingLesson == 4);
        TestProbe.Check("nadie en dos clases", bp.lessons.SelectMany(l => l.learners).Distinct().Count() == bp.lessons.Sum(l => l.learners.Count));

        // ── 4. Requisitos del mundo ──
        WorkRoster ro3 = New(2);
        ro3.worldCheck = m => m.id == "barrer" ? "EstadoDelArea: en obras" : null;
        WorkRoster.DayPlan p3 = ro3.Plan(0, Day());
        TestProbe.Check("sin requisitos no se reparte y queda el motivo", !p3.work.Any(a => a.mission.id == "barrer") && p3.blocked.Any(b => b.mission.id == "barrer" && b.why.Contains("obras")));
        TestProbe.Check("ni se enseña ese día", !p3.lessons.Any(l => l.mission.id.EndsWith("barrer")));
        TestProbe.Check("el balance lo cuenta", WorkRoster.Balance(p3, 2).Any(s => s.Contains("requisitos")));
        TestProbe.Check("obra: una misión de obra no pide que el área esté terminada", !MissionRequirements.Of(new WorkRoster.Mission("Construcción", "obra_trabajo_x", "Obra", 2f)).Any(r => r.kind == MissionRequirements.Kind.EstadoDelArea));
        TestProbe.Check("los reactores piden que no haya emergencia", MissionRequirements.Of(new WorkRoster.Mission("Planta Atómica", "pla_operar", "x", 1f)).Any(r => r.kind == MissionRequirements.Kind.Seguridad));
        NuclearSite ns = FindAnyObjectByType<NuclearSite>();
        if (ns != null)
        {
            ns.DeclareEmergency("prueba");
            TestProbe.Check("en emergencia no se opera el reactor", MissionRequirements.Blocked(new WorkRoster.Mission("Planta Atómica", "pla_operar", "x", 1f)) != null);
            TestProbe.Check("pero se vigila", MissionRequirements.Blocked(new WorkRoster.Mission("Central Nuclear", "nuc_vigilancia", "x", 1f)) == null);
            ns.ClearEmergency();
        }

        // ── 5. Regla de oro ──
        WorkRoster ro5 = New(3);
        WorkRoster.DayPlan last = null;
        for (int d = 0; d < 12; d++) last = ro5.Plan(d, Day());
        TestProbe.Check("acaban sabiéndolo todo", ro5.workers.All(w => Day().All(m => ro5.Learned(w, m))));
        TestProbe.Check("sin nada que aprender no hay lección y a las 9 se trabaja", last.lessons.Count == 0 && last.work.Any(a => a.start == 9f && !a.lesson));
        TestProbe.Check("todos pueden hacerlo todo", ro5.workers.All(w => Day().All(m => ro5.Qualified(w, m))));
        TestProbe.Check("la familia de misiones se aprende una vez", MissionRequirements.KnowledgeKey("obra_trabajo_Ballet") == MissionRequirements.KnowledgeKey("obra_trabajo_Dojo"));

        // ── En la escena ──
        WorkdayDirector wd = FindAnyObjectByType<WorkdayDirector>();
        TestProbe.Check("a las 8 hay qué hacer en el gimnasio y en yoga", Performances.ForTraining("Gimnasio") != null && Performances.ForTraining("Sala de Yoga") != null);
        TestProbe.Check("la lección usa la actuación y el uniforme de la tarea", WorkRoster.LessonBase(WorkRoster.LessonPrefix + "hue_riego") == "hue_riego" && PlayerTasks.For(WorkRoster.LessonPrefix + "hue_riego", "Huerto", "x").steps.Count > 0);
        TestProbe.Check("las rondas de frutales caben en la tarde", FruitGardener.MaxRoundHours <= WorkRoster.SecondBlock);
        if (wd != null && wd.today != null)
        {
            WorkRoster.DayPlan t = wd.PlanDay(wd.plannedDay + 1);
            Debug.Log($"[Jornada] Prueba: {t.work.Count(a => !a.lesson)} misiones, {t.lessons.Count} clases, {t.clones.Count} clones, {t.blocked.Count} sin requisitos, {t.unqualified.Count} sin nadie que sepa, {t.backlog.Count} pendientes.");
            TestProbe.Check("en el santuario hay entrenamiento y lecciones", t.morning.Count > 0 && t.lessons.Count > 0, string.Join(" · ", t.lessons));
            TestProbe.Check("las clases cubren a los que aprenden", t.lessons.Count <= WorkRoster.MaxLessonGroups);
        }
        TestProbe.End();
        yield break;
    }
}
