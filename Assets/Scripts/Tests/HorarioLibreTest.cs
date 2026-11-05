using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PRUEBA EN VIVO DEL HORARIO LIBRE (docs/horario-libre.md; Berón 2026-11-03). En cuatro partes:
///  1. LÓGICA (sin escena): con el catálogo del santuario y 16 personas, una semana entera: todas las franjas llenas, la capacidad
///     respetada y cada actividad repartida por igual (Δ ≤ 1); con poca capacidad (40 personas) sigue llena y Δ ≤ 3.
///  2. LA FAMILIA Y LOS AMIGOS DE BERÓN (2026-11-04: Nesvitor, Amykhuz, Karen, Alicita, Ducristin, Josedrilo, Maite, Shalva y
///     Adaguchi): en la escena con cuerpo de cubos y rótulo, en la jornada (WorkdayDirector.staff, reciben misiones) y con su casa
///     del árbol (Amykhuz y Karen, la misma). Y los FAMOSOS (Sadhguru y los seis del mesocosmos), que se fueron a sus microcosmos,
///     ya no están ni en el santuario ni en la jornada.
///  3. EL SANTUARIO EN VIVO: el horario de la semana para todo el personal (cada hora libre llena, capacidad, uniformidad medida) y
///     dos franjas de verdad (16–18 h, reloj ×<see cref="speed"/>): cada uno anda a su sitio y hace su actividad.
///  4. UN MICROCOSMOS (cargado en aditivo): lo mismo con sus insectos-humanos y sus edificios.
/// Todas las esperas, acotadas. Se añade solo al orquestador cuando se piden las pruebas.
/// </summary>
public class HorarioLibreTest : MonoBehaviour, ITestUnit
{
    public int Group => 65;   // tras las reglas de edificación (63): cambia el reloj y mueve al personal (como la jornada, 50)
    public bool ParallelSafe => false;
    public float speed = 6f;
    public bool shots = true;
    static readonly string[] MicroScenes = { "Microcosmos_Buda", "Microcosmos_Patanjali", "Microcosmos_Eratostenes", "Microcosmos_BruceLee" };
    /// <summary>La familia y los amigos de Berón (2026-11-04, de sus fotos).</summary>
    public static readonly string[] Family = { "Nesvitor", "Amykhuz", "Karen", "Alicita", "Ducristin", "Josedrilo", "Maite", "Shalva", "Adaguchi", "Gallarle", "Julian", "Fausto", "David", "Adonis", "Yulo", "Lale", "Ivelyna", "Luciana", "Malena", "Consol", "Fernando" };
    /// <summary>Los famosos que salieron del santuario a sus microcosmos (2026-11-04).</summary>
    public static readonly string[] Famous = { "Sadhguru", "ElonMusk", "Goku", "BillGates", "WarrenBuffett", "MacKenzieScott", "IbrahimTraore" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<HorarioLibreTest>() == null) tr.gameObject.AddComponent<HorarioLibreTest>();
    }

    static void Log(string s) => Debug.Log("[Horario] " + s);

    public IEnumerator Run()
    {
        TestProbe.Begin("Horario libre: la semana de ocio y cuidado, repartida por igual, andando a cada sitio (santuario y microcosmos)");
        Logic();
        float oldScale = Time.timeScale;
        try { Family_(); } catch (System.Exception ex) { TestProbe.Check("la familia y los amigos de Berón", false, ex.Message); }
        yield return Sanctuary();
        yield return Micro();
        LeisureDirector.ForceRun = false;
        if (LeisureDirector.Instance != null) LeisureDirector.Instance.ReleaseAll();
        HorarioPanel.Show = false; HorarioPanel.FocusScene = "";
        Time.timeScale = oldScale;
        TestProbe.End();
    }

    // ── 1. Lógica ────────────────────────────────────────────────────────────────────────────────
    void Logic()
    {
        var r = new LeisureRoster();
        foreach (var d in LeisureDirector.SanctuaryActivities) r.activities.Add(new LeisureRoster.Activity(d.id, d.title, d.kind, d.where[0], d.cap, d.pose));
        for (int i = 0; i < 16; i++) r.Add("P" + i);
        LeisureRoster.WeekPlan w0 = r.Plan(0), w1 = r.Plan(1);
        int s0 = LeisureRoster.Spread(w0, out int pp0, out string worst0), s1 = LeisureRoster.Spread(w1, out _, out _);
        int slots = LeisureRoster.DaysPerWeek * LeisureRoster.SlotHours.Length;
        TestProbe.Check("lógica: cada persona tiene una actividad en cada hora libre de la semana (35 franjas)", w0.unfilled.Count == 0 && w0.entries.Count == 16 * slots, $"{w0.entries.Count}/{16 * slots}, sin llenar {w0.unfilled.Count}");
        TestProbe.Check("lógica: ninguna actividad pasa de sus plazas", LeisureRoster.OverCapacity(w0).Count == 0 && LeisureRoster.OverCapacity(w1).Count == 0, string.Join("; ", LeisureRoster.OverCapacity(w0)));
        TestProbe.Check("lógica: cada actividad repartida por igual entre las 16 (Δ ≤ 1 por semana)", s0 <= 1 && s1 <= 1, $"semana 0: Δ={s0} ({worst0}), por persona Δ={pp0}; semana 1: Δ={s1}");
        TestProbe.Check("lógica: el orden del catálogo rota una posición por franja", r.Order(1)[0] == r.activities[r.activities.Count - 1] && r.Order(0)[0] == r.activities[0]);
        var tight = new LeisureRoster();
        foreach (var d in LeisureDirector.SanctuaryActivities) tight.activities.Add(new LeisureRoster.Activity(d.id, d.title, d.kind, d.where[0], d.cap, d.pose));
        for (int i = 0; i < 40; i++) tight.Add("Q" + i);
        LeisureRoster.WeekPlan t = tight.Plan(0);
        int st = LeisureRoster.Spread(t, out int ppt, out string wt);
        TestProbe.Check("lógica: con 40 personas (plazas justas) sigue todo lleno y repartido (Δ ≤ 3)", t.unfilled.Count == 0 && LeisureRoster.OverCapacity(t).Count == 0 && st <= 3, $"Δ={st} ({wt}), por persona Δ={ppt}");
        Log($"lógica: 16 personas Δ={s0}/{s1}; 40 personas Δ={st}");
    }

    // ── 2. La familia y los amigos de Berón; los famosos, fuera ─────────────────────────────────────
    void Family_()
    {
        WorkdayDirector wd = WorkdayDirector.Instance != null ? WorkdayDirector.Instance : FindAnyObjectByType<WorkdayDirector>();
        string missing = "", bodyless = "", unlabeled = "", homeless = "";
        foreach (string id in Family)
        {
            GameObject g = WorkdayDirector.Body(id);
            if (g == null) { missing += id + " "; continue; }
            if (g.transform.Find(CubeHumanoid.BodyName) == null) bodyless += id + " ";
            WorldLabel wl = g.GetComponent<WorldLabel>();
            if (wl == null || !wl.text.StartsWith(id)) unlabeled += id + " ";
            if (!g.TryGetComponent(out Anima a) || Bedroom.Of(a) == null) homeless += id + " ";
        }
        TestProbe.Check("la familia y los amigos de Berón están en el santuario", missing.Length == 0, missing.Length > 0 ? "faltan: " + missing : string.Join(", ", Family));
        TestProbe.Check("los de la familia y amigos tienen cuerpo de cubos", bodyless.Length == 0, bodyless);
        TestProbe.Check("los de la familia y amigos llevan su nombre en el rótulo", unlabeled.Length == 0, unlabeled);
        TestProbe.Check("los de la familia y amigos tienen su casa del árbol (dormitorio)", homeless.Length == 0, homeless);
        GameObject ama = WorkdayDirector.Body("Amykhuz"), kar = WorkdayDirector.Body("Karen");
        Bedroom bA = ama != null && ama.TryGetComponent(out Anima aa) ? Bedroom.Of(aa) : null, bK = kar != null && kar.TryGetComponent(out Anima ak) ? Bedroom.Of(ak) : null;
        TestProbe.Check("Amykhuz y Karen (pareja) comparten casa", bA != null && bA == bK, $"{(bA != null ? bA.name : "—")} · {(bK != null ? bK.name : "—")}");
        TestProbe.Check("los de la familia y amigos están en la jornada (personal del santuario)", wd != null && Family.All(s => wd.staff.Contains(s)));
        TestProbe.Check("ningún famoso (Sadhguru ni los seis del mesocosmos) sigue en la jornada", wd != null && !Famous.Any(s => wd.staff.Contains(s)), wd != null ? string.Join(", ", Famous.Where(s => wd.staff.Contains(s))) : "sin jornada");
        string still = string.Join(" ", Famous.Where(s => { GameObject g = WorkdayDirector.Body(s); return g != null && g.scene == SceneManager.GetActiveScene(); }));
        TestProbe.Check("ningún famoso sigue en el santuario (se fueron a sus microcosmos)", still.Length == 0, still);
        if (wd != null)
        {
            TimeController tc = TimeController.timeController;
            WorkRoster.DayPlan p = wd.today ?? wd.PlanDay(tc.Day);
            var got = Family.Where(s => wd.roster.Find(s) != null && p.work.Any(a => a.worker.name == s)).ToList();
            TestProbe.Check("los de la familia y amigos reciben misiones en el reparto de la jornada", got.Count == Family.Length, string.Join(" · ", Family.Select(s => $"{s}: {string.Join(", ", p.work.Where(a => a.worker.name == s).Select(a => a.mission.title))}")));
        }
        if (shots) foreach (string id in Family) { GameObject g = WorkdayDirector.Body(id); if (g != null) Portrait(g.transform, id); }
    }

    // ── 3. El santuario en vivo ──────────────────────────────────────────────────────────────────
    IEnumerator Sanctuary()
    {
        LeisureDirector ld = LeisureDirector.Instance;
        TimeController tc = TimeController.timeController;
        if (!TestProbe.Check("hay director del horario libre", ld != null)) yield break;
        // Sale del microcosmos si una prueba anterior lo dejó dentro (Ambrosio): el santuario, a la vista (y su cámara, allí).
        if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld)
        {
            MobWorldLoader.Instance.ExitMobWorld();
            float until = Time.realtimeSinceStartup + 30f;
            while ((MobWorldLoader.Instance.IsInMobWorld || MobWorldLoader.Instance.IsBusy) && Time.realtimeSinceStartup < until) yield return null;
        }
        WorkdayDirector wd = WorkdayDirector.Instance;
        // La tarde empieza descansada (como la jornada, grupo 50): sin hambre, sueño ni baño que se adelanten a todo.
        Rest(wd != null ? wd.staff : new string[0]);
        int day = tc.Day + 1;
        tc.SetClock(day, 15.9f);
        if (wd != null) wd.PlanDay(day);   // el día de hoy (el horario mira quién hay en el personal)
        ld.Scan(true);
        GameObject any = WorkdayDirector.Body("Linduarda") ?? WorkdayDirector.Body("Goluis");
        LeisureDirector.Group gr = any != null ? ld.GroupOf(any) : ld.groups.FirstOrDefault(g => !g.micro);
        if (!TestProbe.Check("el santuario tiene su horario libre de la semana", gr != null && gr.plan != null)) yield break;
        CheckPlan(gr, "santuario", 15);
        TestProbe.Check("santuario: incluye a la familia y los amigos de Berón, y a ningún famoso", Family.All(s => gr.plan.people.Any(p => p.name == s)) && !Famous.Any(s => gr.plan.people.Any(p => p.name == s)));
        TestProbe.Check("santuario: catálogo de ≥ 6 actividades de ocio y cuidado con sitio", gr.catalog.Count >= 6, string.Join(", ", gr.catalog.Select(a => $"{a.title} → {a.venue}")));
        // (2026-11-04, Berón vio bailar en el piso de ballet sin terminar) Ningún sitio del ocio ni del entrenamiento es un piso EN OBRAS:
        // se usa el de reserva (la calistenia o el gimnasio) hasta que se termine.
        bool Unfinished(Transform t) { ConstructionProject cp = t != null ? t.GetComponentInParent<ConstructionProject>() : null; return cp != null && !cp.Finished; }
        var inWorks = gr.catalog.Where(a => gr.venues.TryGetValue(a, out var v) && v.anchor != null && Unfinished(v.anchor)).Select(a => $"{a.title} → {a.venue}").ToList();
        TestProbe.Check("santuario: ninguna actividad libre en un piso en obras (baile en la calistenia o el gimnasio hasta que el ballet se termine)", inWorks.Count == 0, inWorks.Count > 0 ? string.Join(", ", inWorks) : string.Join(", ", gr.catalog.Select(a => a.venue)));
        if (wd != null)
        {
            var areaObjs = FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None);
            var badTraining = wd.roster.trainingAreas.Where(n => areaObjs.Any(x => x.displayName == n && Unfinished(x.transform))).ToList();
            TestProbe.Check("jornada: ningún entrenamiento en un piso en obras", badTraining.Count == 0, badTraining.Count > 0 ? string.Join(", ", badTraining) : string.Join(", ", wd.roster.trainingAreas));
        }
        // (2026-11-04) LOS MAESTROS DE BAILE (Panterilia y Gohageneis, un día cada uno): guían el baile de las 20 h si están en el horario
        // (en la Sala de Ballet o, mientras está en obras, en la de reserva), y dan el entrenamiento de la Sala de Ballet si está abierta.
        string danceLead = Professions.LeadOn("baile", day);
        var dance = gr.plan.entries.Where(x => x.day == day && x.slot == LeisureRoster.LeadSlot && x.activity.id == "baile").ToList();
        bool leadFree = gr.plan.people.Any(p => p.name == danceLead) && !gr.plan.entries.Any(x => x.day == day && x.slot == LeisureRoster.LeadSlot && x.activity.together && x.person.name == danceLead);
        TestProbe.Check($"el baile de las 20 h lo guía su maestro del día ({danceLead})", !leadFree || dance.Any(x => x.lead && x.person.name == danceLead), string.Join(", ", dance.Select(x => x.person.name + (x.lead ? " (guía)" : ""))));
        if (wd != null && wd.today != null && wd.roster.trainingAreas.Contains("Sala de Ballet"))
        {
            string bl = Professions.LeadOn("Sala de Ballet", wd.today.day);
            WorkRoster.Worker bw = wd.roster.Find(bl);
            TestProbe.Check($"jornada: {bl} da el entrenamiento de la Sala de Ballet (terminada)", bw != null && wd.today.training.TryGetValue(bw, out string ta) && ta == "Sala de Ballet");
        }
        foreach (string l in ld.Report(gr)) Log(l);

        // Dos franjas de verdad: 16–17 y 17–18 h.
        int rec0 = LeisureDirector.Records.Count;
        LeisureDirector.ForceRun = true;
        Time.timeScale = speed;
        HorarioPanel.Show = true;
        float perHour = tc.SecondsPerGameDay / 24f;
        float end = Time.realtimeSinceStartup + Mathf.Max(420f, perHour * 2.3f / speed * 4f);   // (acotado; con la escena cargada los fotogramas son lentos y el reloj avanza menos que el tope)
        bool shot1 = false, labels = false;
        while (tc.Hour < 18.02f && tc.Day == day && Time.realtimeSinceStartup < end)
        {
            if (!labels && tc.Hour > 16.5f) labels = gr.bodies.Any(b => b != null && b.TryGetComponent(out WorldLabel wl) && wl.text.Contains("Libre:"));
            if (shots && !shot1 && tc.Hour > 16.75f) { shot1 = true; HorarioPanel.FocusScene = gr.name; StartCoroutine(Shoot(ld, gr, "panel_santuario_1645")); }
            yield return null;
        }
        if (shots) { HorarioPanel.FocusScene = gr.name; yield return Shoot(ld, gr, "panel_santuario_1800"); }
        Log($"santuario: el reloj llegó a las {tc.Hour:00.00} h del día {tc.Day} (hasta las 18 h)");
        ld.Tick();   // cierra la franja de las 17 h
        var recs = LeisureDirector.Records.Skip(rec0).Concat(ld.Snapshot()).Where(x => !x.micro && x.day == day && x.slot <= 1).ToList();   // (y las que siguen en marcha si el reloj no llegó)
        Summarize(recs, "santuario", 0.7f);
        TestProbe.Check("santuario: el rótulo dice la actividad («Libre: …»)", labels);
        LeisureDirector.ForceRun = false; ld.ReleaseAll(gr);
        Time.timeScale = 1f;
    }

    // ── 4. Un microcosmos ───────────────────────────────────────────────────────────────────────
    IEnumerator Micro()
    {
        LeisureDirector ld = LeisureDirector.Instance;
        TimeController tc = TimeController.timeController;
        if (ld == null) yield break;
        string sceneName = MicroScenes.FirstOrDefault(Application.CanStreamedLevelBeLoaded);
        if (!TestProbe.Check("hay un microcosmos de eras civilizadas en Build Settings", sceneName != null)) yield break;
        Scene s = SceneManager.GetSceneByName(sceneName);
        bool loadedHere = false;
        if (!s.isLoaded)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            float until = Time.realtimeSinceStartup + 60f;
            while (op != null && !op.isDone && Time.realtimeSinceStartup < until) yield return null;
            s = SceneManager.GetSceneByName(sceneName);
            if (!TestProbe.Check($"{sceneName} se carga", s.isLoaded)) yield break;
            loadedHere = true;
        }
        yield return null; yield return null;
        int day = tc.Day + 1;
        tc.SetClock(day, 15.95f);
        ld.Scan(true);
        LeisureDirector.Group gr = ld.GroupOf(s);
        if (TestProbe.Check($"{sceneName}: su horario libre de la semana (insectos-humanos)", gr != null && gr.plan != null && gr.plan.people.Count >= 3, gr != null ? $"{gr.plan?.people.Count} personas" : "sin grupo"))
        {
            CheckPlan(gr, sceneName, 3);
            foreach (string l in ld.Report(gr).Take(12)) Log(l);
            // Que nada de lo anterior los tenga cansados o hambrientos.
            foreach (GameObject b in gr.bodies) { if (b == null) continue; if (b.TryGetComponent(out BodyMetabolism m)) { m.hunger = 0.1f; m.toilet = 0.05f; m.dirt = 0.1f; } if (b.TryGetComponent(out Anima a)) { a.sleepiness = 0.05f; } }
            int rec0 = LeisureDirector.Records.Count;
            LeisureDirector.ForceRun = true;
            Time.timeScale = speed;
            float perHour = tc.SecondsPerGameDay / 24f;
            float end = Time.realtimeSinceStartup + Mathf.Max(360f, perHour * 1.2f / speed * 4f);   // (acotado: con el microcosmos cargado —miles de plantas— el reloj avanza más despacio que el tiempo real; 2026-11-04 se cortaba antes de las 16 h)
            bool shot = false;
            while (tc.Hour < 17.02f && tc.Day == day && Time.realtimeSinceStartup < end)
            {
                if (shots && !shot && tc.Hour > 16.7f) { shot = true; HorarioPanel.Show = true; HorarioPanel.FocusScene = gr.name; StartCoroutine(Shoot(ld, gr, "panel_" + sceneName)); }
                yield return null;
            }
            float reached = tc.Hour; bool started = reached >= 16f || tc.Day > day;
            Log($"{sceneName}: el reloj llegó a las {reached:00.00} h del día {tc.Day} (hasta las 17 h)");
            TestProbe.Check($"{sceneName}: el reloj llega a la franja libre de las 16 h", started, $"{reached:00.00} h");
            ld.Tick();
            var recs = LeisureDirector.Records.Skip(rec0).Concat(ld.Snapshot()).Where(x => x.micro && x.group == gr.name && x.day == day && x.slot == 0).ToList();   // (con las que siguen en marcha)
            Summarize(recs, sceneName, 0.7f);
            LeisureDirector.ForceRun = false; ld.ReleaseAll(gr);
            Time.timeScale = 1f;
        }
        if (loadedHere)
        {
            AsyncOperation un = SceneManager.UnloadSceneAsync(s);
            float until = Time.realtimeSinceStartup + 30f;
            while (un != null && !un.isDone && Time.realtimeSinceStartup < until) yield return null;
        }
    }

    // ── Comprobaciones comunes ───────────────────────────────────────────────────────────────────
    void CheckPlan(LeisureDirector.Group gr, string tag, int minPeople)
    {
        LeisureRoster.WeekPlan p = gr.plan;
        int slots = LeisureRoster.DaysPerWeek * LeisureRoster.SlotHours.Length;
        int missing = 0; string miss = "";
        foreach (LeisureRoster.Person q in p.people)
            for (int d = p.firstDay; d < p.firstDay + LeisureRoster.DaysPerWeek; d++)
                for (int sl = 0; sl < LeisureRoster.SlotHours.Length; sl++)
                    if (p.At(q, d, sl) == null) { missing++; if (miss.Length < 200) miss += $"{q.name} d{d} {LeisureRoster.SlotHours[sl]:00}h; "; }
        TestProbe.Check($"{tag}: todas las horas libres de la semana llenas ({p.people.Count} personas × {slots} franjas)", p.people.Count >= minPeople && missing == 0, missing > 0 ? $"{missing} vacías: {miss}" : $"{p.entries.Count} franjas");
        var over = LeisureRoster.OverCapacity(p);
        TestProbe.Check($"{tag}: ninguna actividad pasa de sus plazas", over.Count == 0, string.Join("; ", over.Take(5)));
        int sp = LeisureRoster.Spread(p, out int pp, out string worst);
        TestProbe.Check($"{tag}: reparto uniforme de cada actividad entre los personajes (Δ ≤ 2)", sp <= 2, $"Δ={sp} ({worst}) · por persona Δ={pp} · {p.people.Count} personas, {p.activities.Count} actividades");
        Log($"{tag}: uniformidad medida Δ={sp} ({worst}), por persona Δ={pp}");
    }

    void Summarize(List<LeisureDirector.Record> recs, string tag, float need)
    {
        int n = recs.Count;
        var free = recs.Where(r => r.blocked < r.since * 0.5f).ToList();   // los que no pasaron la franja con una necesidad (comer, el baño…)
        int arrived = free.Count(r => r.arrived), walked = free.Count(r => r.arrived && !r.fallback), performed = free.Count(r => r.arrived && r.performed > 3f);
        int jumps = recs.Sum(r => r.jumps);
        string late = string.Join("; ", free.Where(r => !r.arrived).Take(8).Select(r => $"{r.who} → {r.activity} ({r.venue}) {r.walked:0} m andados"));
        string blocked = string.Join("; ", recs.Where(r => r.blocked >= r.since * 0.5f).Take(6).Select(r => $"{r.who}: {r.blockedBy ?? "necesidad"}"));
        TestProbe.Check($"{tag}: hay franjas libres en marcha", n > 0, $"{n} franjas");
        if (n == 0) return;
        TestProbe.Check($"{tag}: cada uno llega a su actividad (≥ {need:P0} de los que no atendían una necesidad)", free.Count > 0 && arrived >= need * free.Count, $"{arrived}/{free.Count} llegan" + (late.Length > 0 ? $"; sin llegar: {late}" : "") + (blocked.Length > 0 ? $"; con necesidades: {blocked}" : ""));
        TestProbe.Check($"{tag}: llegan ANDANDO (o en alfombra), no puestos allí", arrived == 0 || walked >= 0.8f * arrived, $"{walked}/{arrived} andando · {free.Where(r => r.arrived).Sum(r => r.walked):0} m andados, {free.Sum(r => r.flown):0} m en alfombra");
        TestProbe.Check($"{tag}: y la hacen (postura o gesto en el sitio)", arrived == 0 || performed >= 0.9f * arrived, $"{performed}/{arrived} · media {free.Where(r => r.arrived).Select(r => r.performed).DefaultIfEmpty(0f).Average():0} s haciéndola");
        TestProbe.Check($"{tag}: sin saltos ni teletransportes", jumps == 0, $"{jumps}: {string.Join(", ", recs.Where(r => r.jumps > 0).Select(r => r.who))}");
        foreach (var g in recs.GroupBy(r => r.activity)) Log($"{tag} · {g.Key}: {string.Join(", ", g.Select(r => $"{r.who}{(r.arrived ? "✓" : "…")}"))}");
    }

    void Rest(IEnumerable<string> staff)
    {
        foreach (string who in staff)
        {
            GameObject g = WorkdayDirector.Body(who);
            if (g == null || !g.TryGetComponent(out Anima a) || a.death) continue;
            if (g.TryGetComponent(out BodyMetabolism b)) { b.hunger = 0.1f; b.toilet = 0.05f; b.dirt = 0.1f; }
            a.sleepiness = 0.05f; a.aware = false;
            bool sleeping = g.TryGetComponent(out ActionQueue q) && q.Own.Any(s => s is SleepSpell);
            if (a.asleep && !sleeping) { a.asleep = false; DailyPoses.Stand().ApplyFinal(a); }
        }
    }

    // ── Fotos ────────────────────────────────────────────────────────────────────────────────────
    const string Dir = "Temp/Horario";

    // UNA CAPTURA DE LA ESCENA DEL GRUPO con el panel encima (2026-11-04: la del santuario salía con el microcosmos de Ambrosio de fondo
    // —la cámara seguía allí— y la de Kapilavastu con el horario del santuario). Una cámara propia, por encima de la principal, mira a
    // alguien del grupo (mejor uno que ya hace su actividad); el panel enseña ese grupo (HorarioPanel.FocusScene).
    IEnumerator Shoot(LeisureDirector ld, LeisureDirector.Group gr, string tag)
    {
        GameObject who = null;
        foreach (var kv in ld.Runs) if (kv.Key != null && kv.Value.group == gr && kv.Value.Arrived) { who = kv.Key; break; }
        if (who == null) who = gr.bodies.FirstOrDefault(b => b != null);
        if (who == null) { Capture(tag); yield break; }
        var go = new GameObject("FotoHorarioPanel_TEMP");
        Camera cam = go.AddComponent<Camera>();
        cam.depth = 100f; cam.fieldOfView = 50f; cam.nearClipPlane = 0.1f;
        Transform t = who.transform;
        Vector3 at = t.position + Vector3.up * 0.8f;
        cam.transform.position = at - t.forward * 7f + t.right * 3f + Vector3.up * 3.5f;
        cam.transform.LookAt(at);
        yield return null;
        Capture(tag);
        yield return null; yield return null;
        Destroy(go);
    }

    static void Capture(string tag)
    {
        System.IO.Directory.CreateDirectory(Dir);
        string path = $"{Dir}/{tag}.png";
        ScreenCapture.CaptureScreenshot(path);
        Log($"captura: {path}");
    }

    /// <summary>Un retrato de frente (cámara propia, a la altura del pecho) en Temp/Horario/&lt;id&gt;.png.</summary>
    public static void Portrait(Transform t, string id)
    {
        System.IO.Directory.CreateDirectory(Dir);
        var go = new GameObject("FotoHorario_TEMP");
        try
        {
            Camera cam = go.AddComponent<Camera>();
            cam.fieldOfView = 32f; cam.nearClipPlane = 0.1f;
            Vector3 chest = t.position + Vector3.up * 0.4f;
            cam.transform.position = chest + t.forward * 3.4f + Vector3.up * 0.25f + t.right * 0.6f;
            cam.transform.LookAt(chest);
            var rt = new RenderTexture(640, 760, 24); cam.targetTexture = rt; cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(640, 760, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 640, 760), 0, 0); tex.Apply();
            RenderTexture.active = null;
            System.IO.File.WriteAllBytes($"{Dir}/{id}.png", tex.EncodeToPNG());
            Object.Destroy(tex); cam.targetTexture = null; rt.Release();
            Log($"retrato: {Dir}/{id}.png");
        }
        finally { Object.Destroy(go); }
    }
}
