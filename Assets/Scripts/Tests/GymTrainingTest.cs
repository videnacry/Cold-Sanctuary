using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Prescription = GymProgramming.Prescription;

/// <summary>
/// ENTRENAMIENTOS REALES EN EL GIMNASIO (Berón 2026-11-04: «hay que actualizar a los demás humanos para que realicen
/// entrenamientos reales (series, repeticiones… rutinas donde se varían músculos por días) … y esto también serviría para el
/// trabajo de entrenador del gym»; docs/gimnasio-maquinas.md §15). Comprueba:
///  1. PLANES: cada humano del personal tiene plan (reparto, días, objetivo), con variedad entre personas; los humanos-insecto
///     no (en sus eras no hay gimnasio); cada día de su plan tiene rutina; los días del plan mandan en el reparto de las 14 h.
///  2. EL ENTRENADOR en la jornada: la misión «gym_planes» es solo suya (sin él no la hace nadie ni queda pendiente) y al
///     terminarla deja los planes revisados.
///  3. PROGRESIÓN: tras una semana simulada sube la carga o las repeticiones de cada ejercicio (y el volumen-carga), salvo el de
///     series sin acabar; la cuarta semana es de descarga (menos series y carga).
///  4. EN VIVO (escena con gimnasio, en Play): 3 personas siguen su rutina de hoy recortada a 3 ejercicios, con el entrenador
///     dando la clase de calentamiento y las rondas. Cada una hace ≥ 3 ejercicios EN ORDEN, cada uno con sus series 1…n de las
///     repeticiones prescritas y un descanso entre series; NUNCA dos personas en la misma máquina (y la del primer ejercicio
///     de una de ellas, toda ocupada a propósito al empezar: la cambia por otra, espera o la deja para el final); el
///     entrenador da la clase (alumnos y repeticiones) y corrige o hace de spotter al menos una vez.
/// Grupo 87 (libre: 64, 67–69, 74–76 y 88–92 están cogidos). Sola: «Tools/Cold Sanctuary/Pruebas/Gimnasio: entrenamientos reales (sola)».
/// </summary>
public class GymTrainingTest : MonoBehaviour, ITestUnit
{
    public int Group => 87;
    public bool ParallelSafe => false;
    public float speed = 3f;
    [Tooltip("Tiempo de juego (s) que se da a cada rutina en vivo (el ritmo de las repeticiones se ajusta a él).")]
    public float budget = 150f;
    [Tooltip("Tope real (s) de la parte en vivo.")]
    public float realLimit = 300f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<GymTrainingTest>() == null) tr.gameObject.AddComponent<GymTrainingTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Gimnasio: planes reales, rutina en vivo, entrenador y progresión");
        Plans();
        TrainerMission();
        Progression();
        MicroPlans();
        yield return StartCoroutine(Live());
        yield return StartCoroutine(MicroLive());
        TestProbe.End();
    }

    // ── 1. Planes ───────────────────────────────────────────────────────────────────────────────
    static void Plans()
    {
        GymProgramming.ResetAll();
        string[] staff = WorkdayDirector.Staff;
        var plans = staff.Select(n => GymProgramming.For(n, 0)).ToList();
        TestProbe.Check($"cada humano del personal tiene plan de gimnasio ({staff.Length})", plans.All(p => p != null),
            string.Join(", ", staff.Where(n => GymProgramming.For(n) == null)));
        if (plans.Any(p => p == null)) return;
        foreach (var p in plans) Debug.Log("[GymTrainingTest] plan " + p);
        TestProbe.Check("variedad: ≥ 3 repartos y ≥ 3 objetivos distintos", plans.Select(p => p.split).Distinct().Count() >= 3 && plans.Select(p => p.goal).Distinct().Count() >= 3,
            $"repartos {string.Join("/", plans.Select(p => p.split).Distinct())} · objetivos {string.Join("/", plans.Select(p => p.goal).Distinct())}");
        TestProbe.Check("cada plan entrena 2–6 días y cada día de su plan tiene rutina (≥ 4 ejercicios)",
            plans.All(p => p.days.Length >= 2 && p.days.Length <= 6 && Enumerable.Range(0, 14).Where(p.Trains).All(d => GymProgramming.Prescribe(p.who, d).Main.Count() >= 4)),
            string.Join(", ", plans.Where(p => p.days.Length < 2 || p.days.Length > 6).Select(p => p.who)));
        TestProbe.Check("los días sin gimnasio en su plan, recuperación activa", plans.All(p => Enumerable.Range(0, 7).Where(d => !p.Trains(d)).All(d => GymProgramming.Prescribe(p.who, d).recovery)));
        TestProbe.Check("los humanos-insecto no usan los planes del gimnasio del santuario (tienen los de su época: HistoricTraining)", GymProgramming.For("Obrera de Hormiga") == null && GymProgramming.For("Myrmidon") == null);
        TestProbe.Check($"el entrenador ({GymProgramming.Trainer}) parte con las cargas más altas (el de más fuerza) y Goluis sigue con un plan de fuerza", plans.OrderByDescending(p => p.strength).First().who == GymProgramming.Trainer && GymProgramming.For("Goluis").goal == TrainingGoal.Fuerza);
        GymProgramming.DayRoutine r = GymProgramming.Prescribe("Amahuz", 0);
        TestProbe.Check("la rutina empieza calentando y acaba estirando; series × reps, carga, descanso y RPE en cada ejercicio",
            r.items.First().part == "calentamiento" && r.items.Last().part == "vuelta a la calma" && r.Main.All(x => x.sets > 0 && x.reps > 0 && x.restSec > 0f && x.rpe > 0f),
            string.Join(" · ", r.items.Select(x => x.ToString())));

        // Los días del plan en el reparto de las 14 h.
        var roster = new WorkRoster(); roster.areas.Add("Cocina");
        foreach (string n in new[] { "Amahuz", "Lizmibet", GymProgramming.Trainer, "Karen" }) roster.Add(n);
        var open = new List<string> { GymProgramming.GymArea, "Sala de Yoga" };
        roster.trainingAreas.AddRange(open);
        bool ok = true; string why = "";
        for (int d = 0; d < 7; d++)
        {
            WorkRoster.DayPlan dp = roster.Plan(d, new List<WorkRoster.Mission>());
            GymProgramming.ApplyGymDays(dp, open, roster.workers);
            foreach (WorkRoster.Worker w in roster.workers)
            {
                bool gym = dp.training.TryGetValue(w, out string t) && t == GymProgramming.GymArea;
                bool want = w.name == GymProgramming.Trainer || GymProgramming.For(w.name).Trains(d);
                if (gym != want) { ok = false; why += $"{w.name} día {d}: {t} "; }
            }
        }
        TestProbe.Check("en el reparto de las 14 h van al gimnasio los días de su plan (y el entrenador, siempre)", ok, why);
    }

    // ── 2. La misión del entrenador ─────────────────────────────────────────────────────────────
    static void TrainerMission()
    {
        List<WorkRoster.Mission> Day()
        {
            var l = new List<WorkRoster.Mission>();
            for (int i = 0; i < 12; i++) l.Add(new WorkRoster.Mission("Cocina", "test_coc_" + i, "Cocinar " + i, 1f));
            l.AddRange(GymProgramming.Missions());
            return l;
        }
        var r = new WorkRoster(); r.areas.Add("Cocina"); r.areas.Add(GymProgramming.GymArea);
        foreach (string n in new[] { "Amahuz", "Karen", GymProgramming.Trainer }) r.Add(n);
        bool always = true;
        for (int d = 0; d < 5; d++)
        {
            WorkRoster.DayPlan p = r.Plan(d, Day());
            var a = p.work.Where(x => x.mission.id == GymProgramming.PlanMissionId).ToList();
            if (a.Count != 1 || a[0].worker.name != GymProgramming.Trainer) always = false;
        }
        TestProbe.Check($"la misión «{GymProgramming.PlanMissionId}» la hace cada día el entrenador ({GymProgramming.Trainer}) y nadie más", always);
        var r2 = new WorkRoster(); r2.areas.Add("Cocina"); r2.areas.Add(GymProgramming.GymArea);
        foreach (string n in new[] { "Amahuz", "Karen" }) r2.Add(n);
        WorkRoster.DayPlan p2 = r2.Plan(0, Day());
        TestProbe.Check("sin el entrenador no la hace otro ni queda pendiente", !p2.work.Any(x => x.mission.id == GymProgramming.PlanMissionId) && !p2.backlog.Any(x => x.id == GymProgramming.PlanMissionId)
            , string.Join(", ", p2.work.Where(x => x.mission.id == GymProgramming.PlanMissionId).Select(x => x.worker.name)));
        GymProgramming.ResetAll();
        List<string> lines = GymProgramming.Review(GymProgramming.Trainer, 0, WorkdayDirector.Staff);
        foreach (string l in lines.Take(6)) Debug.Log("[GymTrainingTest] revisión: " + l);
        TestProbe.Check("al terminarla, el entrenador escribe los planes y los deja revisados por él", lines.Count >= WorkdayDirector.Staff.Length && GymProgramming.AllPlans.All(p => p.adjustedBy == GymProgramming.Trainer), $"{lines.Count} líneas");
    }

    // ── 3. Progresión ───────────────────────────────────────────────────────────────────────────
    static void Progression()
    {
        GymProgramming.ResetAll();
        bool ok = true; string why = "";
        foreach (string who in WorkdayDirector.Staff)
        {
            GymProgramming.TrainingPlan p = GymProgramming.For(who, 0);
            var before = p.progress.ToDictionary(kv => kv.Key, kv => (kv.Value.load, kv.Value.reps));
            float t0 = GymProgramming.WeeklyTonnage(p);
            GymMachineType missed = p.progress.Keys.First();
            p.missed.Add(missed);
            GymProgramming.Prescribe(who, 7);   // pasa una semana (SyncWeek aplica la progresión)
            foreach (var kv in p.progress)
            {
                var b = before[kv.Key];
                bool up = kv.Value.load > b.load + 1e-3f || kv.Value.reps > b.reps;
                if (kv.Key == missed ? up : !up) { ok = false; why += $"{who}/{GymStation.Label(kv.Key)} {b.load}×{b.reps}→{kv.Value.load}×{kv.Value.reps} · "; }
            }
            if (GymProgramming.WeeklyTonnage(p) <= t0) { ok = false; why += $"{who}: volumen {t0:0}→{GymProgramming.WeeklyTonnage(p):0} · "; }
        }
        TestProbe.Check("tras una semana simulada sube la carga o las repeticiones de cada ejercicio (salvo el de series sin acabar)", ok, why);
        GymProgramming.ResetAll();
        GymProgramming.TrainingPlan g = GymProgramming.For("Goluis", 0);
        int d3 = Enumerable.Range(21, 7).First(g.Trains), d2 = Enumerable.Range(14, 7).First(g.Trains);
        GymProgramming.DayRoutine w2 = GymProgramming.Prescribe("Goluis", d2);
        Prescription a2 = w2.Main.First(x => x.load > 0f);
        float load2 = g.progress[a2.station].load; int sets2 = g.progress[a2.station].sets;
        GymProgramming.DayRoutine w3 = GymProgramming.Prescribe("Goluis", d3);
        Prescription a3 = w3.Main.First(x => x.load > 0f);
        TestProbe.Check("la 4.ª semana es de descarga: la mitad de series y menos carga", w3.deload && !w2.deload && a3.sets < g.progress[a3.station].sets && a3.load < g.progress[a3.station].load,
            $"{a3.Name}: {a3.sets}×{a3.reps} a {a3.load} (plan {g.progress[a3.station].sets} series a {g.progress[a3.station].load}; semana 3: {sets2} a {load2})");
        GymProgramming.ResetAll();
    }

    // ── 4. En vivo ──────────────────────────────────────────────────────────────────────────────
    class Held
    {
        public GameObject g; public Anima a; public Vector3 pos; public Quaternion rot;
        public IdleBehavior idle; public bool idleWas; public ActionQueue q; public bool qWas;
        public UnityEngine.AI.NavMeshAgent agent; public bool agentWas;
        public CarnivoreKeeper keeper; public bool keeperWas;
    }

    static Held Hold(GameObject g)
    {
        var w = new Held { g = g, a = g.GetComponent<Anima>(), pos = g.transform.position, rot = g.transform.rotation };
        w.idle = g.GetComponent<IdleBehavior>(); w.idleWas = w.idle != null && w.idle.enabled; if (w.idle != null) w.idle.enabled = false;
        w.q = g.GetComponent<ActionQueue>(); w.qWas = w.q != null && w.q.enabled; if (w.q != null) { w.q.ClearOwn(); w.q.enabled = false; }
        w.agent = g.GetComponent<UnityEngine.AI.NavMeshAgent>(); w.agentWas = w.agent != null && w.agent.enabled; if (w.agent != null) w.agent.enabled = false;
        w.keeper = g.GetComponent<CarnivoreKeeper>(); w.keeperWas = w.keeper != null && w.keeper.enabled; if (w.keeper != null) w.keeper.enabled = false;
        if (w.a != null) w.a.asleep = false;
        PathMover.Of(g.transform).Restart();
        return w;
    }

    static void Restore(Held w)
    {
        if (w.g == null) return;
        if (w.a != null) DailyPoses.Stand().ApplyFinal(w.a);
        w.g.transform.SetPositionAndRotation(w.pos, w.rot);
        if (w.agent != null) { w.agent.enabled = w.agentWas; if (w.agent.enabled && w.agent.isOnNavMesh) w.agent.Warp(w.pos); }
        if (w.q != null) w.q.enabled = w.qWas;
        if (w.idle != null) w.idle.enabled = w.idleWas;
        if (w.keeper != null) w.keeper.enabled = w.keeperWas;
    }

    /// <summary>La rutina de hoy de <paramref name="who"/> (el primer día de su plan) recortada a 3 ejercicios, con su
    /// calentamiento y su vuelta a la calma.</summary>
    static GymProgramming.DayRoutine Short(string who)
    {
        GymProgramming.TrainingPlan p = GymProgramming.For(who, 0);
        int d = Enumerable.Range(0, 7).First(p.Trains);
        GymProgramming.DayRoutine r = GymProgramming.Prescribe(who, d);
        var main = r.Main.Take(3).ToList();
        r.items = new List<Prescription> { r.items.First() }.Concat(main).Concat(new[] { r.items.Last() }).ToList();
        return r;
    }

    IEnumerator Live()
    {
        if (GymStation.All.Count == 0) { TestProbe.Check("hay máquinas del gimnasio en la escena (para la parte en vivo)", false, "0 GymStation: construye la escena del santuario"); yield break; }
        float mEnd = Time.realtimeSinceStartup + 120f;
        while (Time.realtimeSinceStartup < mEnd && GymUsabilityTest.Arriving()) yield return new WaitForSecondsRealtime(1f);
        WorkdayDirector wd = WorkdayDirector.Instance;
        if (wd != null) wd.ReleaseGym();
        // «El personaje con más fuerza es Andrea» (Berón 2026-11-04): el entrenador tiene la mayor fuerza del personal en escena.
        var strong = WorkdayDirector.Staff.Select(n => WorkdayDirector.Body(n)).Where(g => g != null && !g.CompareTag("Player")).Select(g => g.GetComponent<Anima>()).Where(a => a != null).ToList();
        Anima strongest = strong.OrderByDescending(a => a.strength).FirstOrDefault();
        TestProbe.Check($"el entrenador ({GymProgramming.Trainer}) es el de más fuerza del personal", strongest != null && strongest.name.StartsWith(GymProgramming.Trainer),
            string.Join(", ", strong.OrderByDescending(a => a.strength).Take(4).Select(a => $"{a.name} {a.strength:0.00}")));
        var people = new List<GameObject>();
        GameObject trainer = null;
        foreach (string n in WorkdayDirector.Staff)
        {
            GameObject g = WorkdayDirector.Body(n);
            if (g == null || g.CompareTag("Player") || g.GetComponent<CreatureRig>() == null) continue;
            Anima a0 = g.GetComponent<Anima>(); if (a0 == null || a0.death) continue;
            if (g.TryGetComponent(out Linduarda lin) && lin.enabled) continue;
            if (g.TryGetComponent(out NewcomerInitiator ni) && ni.InProgress) continue;
            if (RestaurantService.Drives(g) || Orchestra.Busy(g)) continue;
            if (n == GymProgramming.Trainer) trainer = g; else if (people.Count < 3) people.Add(g);
        }
        if (trainer == null && people.Count > 3) { trainer = people[3]; }
        TestProbe.Check("hay 3 personas y el entrenador para la parte en vivo", people.Count >= 3 && trainer != null, $"{string.Join(", ", people.Select(p => p.name))} · entrenador {(trainer != null ? trainer.name : "—")}");
        if (people.Count < 1) yield break;

        bool wdWas = wd != null && wd.enabled; if (wd != null) wd.enabled = false;
        float oldScale = Time.timeScale; Time.timeScale = speed;
        GymProgramming.ResetAll();
        var held = people.Concat(trainer != null ? new[] { trainer } : new GameObject[0]).Select(Hold).ToList();
        var sessions = people.Select(g => new GymTrainingSession(g, Short(g.name), budget)).ToList();

        // La máquina del primer ejercicio de la primera persona: TODAS sus copias ocupadas al empezar.
        GymMachineType busyType = sessions[0].routine.Main.First().station;
        var dummies = new List<string>();
        foreach (GymStation s in GymStation.All.Where(s => s != null && s.type == busyType)) { string d = "Ocupado_" + dummies.Count; if (s.Claim(d)) dummies.Add(d); }
        float dummyUntil = Time.time + 12f;

        Vector3 cp = WorkdayDirector.GymClassPoint();
        Transform at = WorkdayDirector.AreaTransform(GymProgramming.GymArea);
        GymCoach coach = trainer != null ? new GymCoach(trainer, cp, at != null ? Quaternion.Euler(0f, at.eulerAngles.y, 0f) : Quaternion.identity, 45f, 99999f, null, 0f) : null;

        int doubles = 0; string doubleAt = ""; bool dummyUsed = false;
        float tEnd = Time.time + budget * 2.5f + 60f, rEnd = Time.realtimeSinceStartup + realLimit;
        while (Time.time < tEnd && Time.realtimeSinceStartup < rEnd && sessions.Any(s => !s.Done))
        {
            if (dummies.Count > 0 && Time.time >= dummyUntil) { foreach (string d in dummies) GymStation.Release(d); dummies.Clear(); }
            coach?.Tick(sessions);
            foreach (GymTrainingSession s in sessions) s.Tick(coach);
            // Nadie comparte máquina: cada sentado en la suya, reservada a su nombre, y distinta de la de los demás.
            var seen = new Dictionary<GymStation, string>();
            foreach (GymTrainingSession s in sessions)
            {
                if (s.use == null || !s.use.seated) continue;
                if (dummies.Any(d => s.use.st.Occupant == d)) dummyUsed = true;
                if (seen.TryGetValue(s.use.st, out string other) || s.use.st.Occupant != s.Who) { doubles++; doubleAt = $"{s.use.st.name}: {s.Who} y {other ?? s.use.st.Occupant}"; }
                seen[s.use.st] = s.Who;
            }
            yield return null;
        }
        foreach (string d in dummies) GymStation.Release(d);

        TestProbe.Check("nunca dos personas en la misma máquina", doubles == 0 && !dummyUsed, doubles > 0 ? $"{doubles} fotogramas · {doubleAt}" : dummyUsed ? "se sentó en una máquina ocupada" : "");
        GymTrainingSession first = sessions[0];
        TestProbe.Check($"con todas las copias de {GymStation.Label(busyType)} ocupadas, la cambia por otra, espera o la deja para el final",
            first.events.Any(e => e.kind == GymTrainingSession.Kind.Swap || e.kind == GymTrainingSession.Kind.Wait || e.kind == GymTrainingSession.Kind.Defer),
            string.Join(", ", first.events.Where(e => e.kind != GymTrainingSession.Kind.Set && e.kind != GymTrainingSession.Kind.Rest).Take(8)));

        foreach (GymTrainingSession s in sessions)
        {
            var mains = s.queue.Where(x => !x.timed).ToList();
            var done = new List<string>(); string bad = "";
            foreach (Prescription x in mains)
            {
                var evs = s.events.Where(e => e.exercise == x.Name && (e.kind == GymTrainingSession.Kind.Set || e.kind == GymTrainingSession.Kind.Rest)).ToList();
                int n = s.TotalSets(x);
                var sets = evs.Where(e => e.kind == GymTrainingSession.Kind.Set).ToList();
                bool order = sets.Count == n && sets.Select(e => e.set).SequenceEqual(Enumerable.Range(1, n)) && sets.All(e => e.reps == x.reps);
                bool alternate = evs.Count == 2 * n - 1 && evs.Select((e, i) => (e.kind == GymTrainingSession.Kind.Set) == (i % 2 == 0)).All(b => b);
                if (order && alternate) done.Add(x.Name);
                else bad += $"{x.Name}: series {string.Join(",", sets.Select(e => e.set + "×" + e.reps))} de {n}×{x.reps}, {evs.Count(e => e.kind == GymTrainingSession.Kind.Rest)} descansos · ";
            }
            // El orden de los ejercicios: el de la cola (con lo que dejó para el final ya al final).
            var firstSet = mains.Select(x => s.events.FindIndex(e => e.kind == GymTrainingSession.Kind.Set && e.exercise == x.Name)).ToList();
            bool inOrder = firstSet.Where(i => i >= 0).SequenceEqual(firstSet.Where(i => i >= 0).OrderBy(i => i));
            TestProbe.Check($"{s.Who}: ≥ 3 ejercicios en orden, cada uno con sus series 1…n de las reps prescritas y descanso entre series",
                done.Count >= 3 && inOrder, $"{done.Count} bien ({string.Join(" → ", done)}) {bad}{(s.Done ? "" : $"· sin terminar: {s.Status()}")}");
        }
        if (coach != null)
        {
            TestProbe.Check($"el entrenador ({coach.g.name}) da la clase de calentamiento", coach.attended >= 1 && coach.classReps >= 2, $"{coach.attended} alumnos, {coach.classReps} repeticiones");
            TestProbe.Check("el entrenador da la clase en el gimnasio, no donde estaba (ronda 7: Andrea, parada en Cría)", coach.classAtPoint, $"a {Vector3.Distance(trainer.transform.position, cp):0.0} m del punto de la clase al acabar");
            TestProbe.Check($"el entrenador corrige o hace de spotter en las máquinas", coach.corrections + coach.spots >= 1, $"{coach.corrections} correcciones, {coach.spots} de spotter · {string.Join(" | ", coach.notes.Take(5))}");
            coach.Stop("fin de la prueba");
            // SIN NADIE A QUIEN CORREGIR Y LEJOS DEL GIMNASIO (lo que pasó en la ronda 7): vuelve andando; no se queda parado donde está.
            Held th = held.First(h => h.g == trainer);
            if (Vector3.Distance(th.pos, cp) > 5f)
            {
                WorkdayDirector.Teleport(trainer.transform, th.pos); PathMover.Of(trainer.transform).Restart();
                var idle = new GymCoach(trainer, cp, Quaternion.identity, 0f, 99999f, null, 0f);
                float d0 = Vector3.Distance(trainer.transform.position, cp), idleEnd = Time.time + 120f, idleReal = Time.realtimeSinceStartup + 60f;
                var none = new List<GymTrainingSession>();
                while (Time.time < idleEnd && Time.realtimeSinceStartup < idleReal && Vector3.Distance(trainer.transform.position, cp) > 2.5f) { idle.Tick(none); yield return null; }
                float d1 = Vector3.Distance(trainer.transform.position, cp);
                TestProbe.Check("sin nadie a quien corregir, el entrenador vuelve andando al gimnasio", d1 < d0 - 3f || d1 <= 2.5f, $"de {d0:0.0} m a {d1:0.0} m");
            }
        }
        foreach (GymTrainingSession s in sessions) s.Stop("fin de la prueba");
        foreach (Held h in held) Restore(h);
        Time.timeScale = oldScale;
        if (wd != null) wd.enabled = wdWas;
        GymProgramming.ResetAll();
    }

    // ── 5. Microcosmos: entrenar como en su época (docs/gimnasio-maquinas.md §16) ─────────────────────
    /// <summary>Los microcosmos con fecha (Eras.OfMicrocosmos) y las clases estables de los pueblos (MicroStyles.TrainingKind).</summary>
    static readonly string[] Micros = { "Mesopotamia", "Kapilavastu", "Patanjali", "Egina", "Alejandría", "Escútari", "Dayton", "Tregua 1914", "Bruce Lee",
        "Irena Sendler", "Le Chambon-sur-Lignon", "Crotona", "Menfis", "Bagdad", "Woolsthorpe", "Berkeley", "Angostura", "Sils Maria", "París 1898",
        "Colorado Springs", "Berna", "Pasadena", "Chernóbil", "Palenque", "Velliangiri", "Hawthorne", "Aldea de la montaña", "Albuquerque", "Omaha",
        "Prairie View", "Uagadugú", "Planta Atómica", "Huerto", "Cocina", "Sala de Yoga",
        "Chimbacalle", "Lagunillas", "Cajamarca", "Onondaga", "Alejandría de Cleopatra", "Langley", "Viena 1824", "Leiden", "San Pablo", "Madrid 2018", "Gettysburg", "Feria de las Hazañas" };
    static readonly string[] Kinds = { "juegos_heroicos", "gimnasio_griego", "palestra_romana", "patio_lucha_egipcio", "patio_lucha", "akhara", "furusiyya", "zurkhaneh",
        "tekke_guresh", "juego_pelota", "wuguan", "dojo", "sala_armas", "sala_esgrima", "prado_juegos", "garrote_coleo", "turnplatz_schwingen", "turnhalle", "hebert",
        "futbol_tregua", "club_deportivo", "estadio", "gimnasio_moderno", "palin", "warachikuy", "tewaarathon", "pelota_nacional" };

    static void MicroPlans()
    {
        HistoricTraining.ResetPlans();
        var noTrad = Kinds.Where(k => HistoricTraining.For(k) == null).ToList();
        TestProbe.Check($"cada clase de lugar de entrenar de los pueblos tiene su tradición ({Kinds.Length})", noTrad.Count == 0, string.Join(", ", noTrad));
        int towns = 0; string bad = ""; var seen = new List<string>();
        foreach (string m in Micros)
        {
            HistoricTraining.Tradition t = HistoricTraining.ForMicrocosmos(m, out int year, out string kind);
            if (kind == null) { if (t != null) bad += $"{m}: tradición sin clase · "; continue; }   // la prehistoria: no hay lugar de entrenar
            if (t == null) { bad += $"{m} ({kind}, {year}): sin tradición · "; continue; }
            towns++; seen.Add($"{m}→{t.place}");
            var sessions = HistoricTraining.Sessions(t, year, kind);
            if (sessions.Count == 0) { bad += $"{m}: ninguna sesión de ≥ 3 ejercicios de época en {year} · "; continue; }
            foreach (var s in sessions) foreach (var d in s.drills) if (!d.In(year)) bad += $"{m}: {d.name} no existía en {year} · ";
            for (int i = 0; i < 3; i++)
            {
                HistoricTraining.Plan p = HistoricTraining.PlanFor($"{m} insecto {i}", t, year, kind, i == 0 ? "soldado" : "");
                if (p.days.Length < 3) bad += $"{p.who}: {p.days.Length} días · ";
                int d0 = Enumerable.Range(0, 7).First(p.Trains);
                HistoricTraining.Routine r = HistoricTraining.Prescribe(p, d0);
                if (r.Main.Count() < 3 || r.Main.Any(x => x.sets < 1 || x.reps < 1 || x.restSec <= 0f)) bad += $"{p.who}: rutina «{r.name}» con {r.Main.Count()} ejercicios · ";
                if (r.items.First().part != "calentamiento" || r.items.Last().part != "vuelta a la calma") bad += $"{p.who}: sin calentar o sin estirar · ";
                var before = p.progress.ToDictionary(kv => kv.Key, kv => (kv.Value.load, kv.Value.reps));
                HistoricTraining.Prescribe(p, d0 + 7);   // una semana después
                foreach (var kv in p.progress) if (!(kv.Value.load > before[kv.Key].load || kv.Value.reps > before[kv.Key].reps)) bad += $"{p.who}: {kv.Key} no progresa · ";
            }
        }
        Debug.Log("[GymTrainingTest] microcosmos: " + string.Join(", ", seen));
        TestProbe.Check($"cada pueblo con lugar de entrenar tiene tradición, sesiones de época (≥ 3 ejercicios que existían ese año), planes de 3–4 días y progresión ({towns} pueblos)",
            bad.Length == 0 && towns >= 20, bad.Length > 300 ? bad.Substring(0, 300) + "…" : bad);
        TestProbe.Check("Crotona entrena en el gimnasio griego (con el ternero de Milón) y Velliangiri en el akhāṛā con el kalari",
            HistoricTraining.ForMicrocosmos("Crotona", out int yc, out string kc)?.id == "griego" && HistoricTraining.Sessions(HistoricTraining.For(kc), yc, kc).Any(s => s.drills.Any(d => d.id == "ternero"))
            && HistoricTraining.ForMicrocosmos("Velliangiri", out int yv, out string kvv)?.id == "akhara" && HistoricTraining.Sessions(HistoricTraining.For(kvv), yv, kvv).Any(s => s.name == "Kalari"));
        HistoricTraining.ResetPlans();
    }

    /// <summary>EN VIVO, sin cargar un microcosmos: un edificio de entrenamiento griego de prueba (foso, halteres de un sitio, saco,
    /// pista y armero) con un maestro y 3 insectos-humanos (cuerpos vacíos); el sitio de los halteres empieza ocupado.</summary>
    IEnumerator MicroLive()
    {
        var root = new GameObject("PruebaPalestra"); root.transform.position = new Vector3(5000f, 0f, 5000f);
        AreaFunctionSite site = root.AddComponent<AreaFunctionSite>();
        site.building = "Centro de entrenamiento"; site.training = "gimnasio_griego"; site.region = "Egeo"; site.activity = AreaFunctionSite.Entrenamiento;
        void St(string key, Vector3 at, Vector3 size)
        {
            var go = new GameObject(key); go.transform.SetParent(root.transform, false); go.transform.localPosition = at;
            BoxCollider bc = go.AddComponent<BoxCollider>(); bc.size = size; bc.center = new Vector3(0f, size.y * 0.5f, 0f);
            go.AddComponent<AreaFunctionStation>().station = key;
        }
        St("foso_lucha", new Vector3(0f, 0f, 0f), new Vector3(4f, 0.05f, 4f));
        St("halteres", new Vector3(5f, 0f, 0f), new Vector3(1.2f, 0.5f, 0.5f));
        St("saco", new Vector3(5f, 0f, 3f), new Vector3(1.3f, 1.5f, 0.6f));
        St("pista_carreras", new Vector3(-5f, 0f, 0f), new Vector3(2.4f, 0.05f, 12f));
        St("armero", new Vector3(5f, 0f, -3f), new Vector3(1.6f, 1.5f, 0.4f));
        var cls = new HistoricClass(site, HistoricTraining.For("gimnasio_griego"), -240, "gimnasio_griego", 0);
        var people = new List<GameObject>();
        for (int i = 0; i < 4; i++) { var g = new GameObject($"Atleta de prueba {i}"); g.transform.position = root.transform.position + new Vector3(-3f + i * 2f, 0f, -6f); people.Add(g); }
        float oldScale = Time.timeScale; Time.timeScale = speed;
        cls.coach = new HistoricCoach(people[0], cls, 8f);
        var Dr = new System.Func<string, int, HistoricTraining.Prescription>((id, n) => new HistoricTraining.Prescription { drill = HistoricTraining.D(id), sets = 2, reps = n, load = HistoricTraining.D(id).baseLoad, restSec = 90f, rpe = 8f });
        var sessions = new List<HistoricSession>();
        foreach (GameObject g in people.Skip(1))
        {
            var r = new HistoricTraining.Routine { who = g.name, name = "Palestra de prueba" };
            r.items.Add(new HistoricTraining.Prescription { drill = HistoricTraining.D("carrera_suave"), minutes = 5f, part = "calentamiento" });
            r.items.Add(Dr("halteres", 5)); r.items.Add(Dr("lucha_pale", 4)); r.items.Add(Dr("korykos", 6));
            r.items.Add(new HistoricTraining.Prescription { drill = HistoricTraining.D("estirar"), minutes = 5f, part = "vuelta a la calma" });
            cls.PlanOf(g.name, "");
            var s = new HistoricSession(g, cls, r, 60f); cls.sessions[g] = s; sessions.Add(s);
        }
        // El sitio de los halteres (uno solo), ocupado al empezar: el primero espera o lo deja para el final; los otros dos, igual.
        var dummyGo = new GameObject("Ocupante de prueba");
        var dummy = new HistoricSession(dummyGo, cls, new HistoricTraining.Routine { who = dummyGo.name, name = "—" }, 1f);
        int hal = Enumerable.Range(0, cls.SpotCount).FirstOrDefault(i => cls.SpotKey(i) == "halteres");
        cls.Block(hal, dummy); float blockUntil = Time.time + 10f;
        float waitWas = HistoricSession.MaxWait; HistoricSession.MaxWait = 40f;   // (tres comparten un solo sitio de halteres: que esperen su turno)
        int doubles = 0; float tEnd = Time.time + 400f, rEnd = Time.realtimeSinceStartup + realLimit;
        while (Time.time < tEnd && Time.realtimeSinceStartup < rEnd && sessions.Any(s => !s.Done))
        {
            if (dummy != null && Time.time >= blockUntil) { cls.Unblock(hal); dummy = null; }
            cls.coach.Tick(cls.sessions.Values);
            foreach (HistoricSession s in sessions) s.Tick();
            var at = sessions.Where(s => s.spot >= 0 && (s.phase == HistoricSession.Phase.Set || s.phase == HistoricSession.Phase.Rest)).Select(s => s.spot).ToList();
            if (at.Count != at.Distinct().Count()) doubles++;
            if (dummy != null && sessions.Any(s => s.spot == hal)) doubles++;
            yield return null;
        }
        TestProbe.Check("pueblo: nunca dos en el mismo sitio de entrenar (ni en el ocupado)", doubles == 0, $"{doubles} fotogramas");
        TestProbe.Check("pueblo: con el sitio de los halteres ocupado, esperan o lo dejan para el final",
            sessions.Any(s => s.events.Any(e => e.kind == HistoricSession.Kind.Wait || e.kind == HistoricSession.Kind.Defer)));
        foreach (HistoricSession s in sessions)
        {
            var mains = s.queue.Where(x => !x.Timed).ToList(); int ok = 0; string bad = "";
            foreach (var x in mains)
            {
                var evs = s.events.Where(e => e.drill == x.drill.name && (e.kind == HistoricSession.Kind.Set || e.kind == HistoricSession.Kind.Rest)).ToList();
                var sets = evs.Where(e => e.kind == HistoricSession.Kind.Set).ToList();
                bool good = sets.Select(e => e.set).SequenceEqual(Enumerable.Range(1, x.sets)) && sets.All(e => e.reps == x.reps)
                            && evs.Count == 2 * x.sets - 1 && evs.Select((e, i) => (e.kind == HistoricSession.Kind.Set) == (i % 2 == 0)).All(b => b);
                if (good) ok++; else bad += $"{x.drill.name}: {string.Join(",", sets.Select(e => e.set + "×" + e.reps))} · ";
            }
            TestProbe.Check($"pueblo: {s.Who} hace los 3 ejercicios de época con sus series, reps y descanso", ok >= 3, $"{ok}/3 {bad}{(s.Done ? "" : s.Status())}");
        }
        HistoricCoach c = cls.coach;
        TestProbe.Check($"pueblo: el maestro ({cls.tradition.trainer}) da la clase de calentamiento y corrige", c.attended >= 1 && c.classReps >= 2 && c.corrections >= 1,
            $"{c.attended} alumnos, {c.classReps} repeticiones, {c.corrections} correcciones · {string.Join(" | ", c.notes.Take(4))}");
        TestProbe.Check("pueblo: el maestro da la clase en su edificio, no donde estaba", c.classAtSite);
        Time.timeScale = oldScale; HistoricSession.MaxWait = waitWas;
        foreach (GameObject g in people) Object.Destroy(g);
        Object.Destroy(dummyGo); Object.Destroy(root);
        HistoricTraining.ResetPlans();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pruebas/Gimnasio: entrenamientos reales (sola)")]
    static void Solo()
    {
        if (!Application.isPlaying) { Debug.Log("[Solo] no está en Play"); return; }
        var go = new GameObject("SoloEntrenamientos"); DontDestroyOnLoad(go);
        GymTrainingTest t = go.AddComponent<GymTrainingTest>();
        t.StartCoroutine(Wrap(t));
    }

    static IEnumerator Wrap(GymTrainingTest t)
    {
        TestProbe.ResetTotals();
        yield return t.StartCoroutine(t.Run());
        TestProbe.Total();
        Debug.Log("[Solo] listo");
    }
#endif
}
