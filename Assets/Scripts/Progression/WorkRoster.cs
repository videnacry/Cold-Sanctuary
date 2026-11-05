using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ROTACIÓN DIARIA DEL TRABAJO (docs/jornada-y-rotacion.md). Cada persona trabaja 8 h al día: 3 h de trabajo, 1 h de
/// comida, 2 h de trabajo y 2 h de entrenamiento; después, tiempo libre. Las 5 h de trabajo se reparten así:
///  1. Cada día hay una lista de MISIONES POR ÁREA (las obligatorias de cada día primero, y antes que todas, las que
///     quedaron pendientes ayer).
///  2. Se recorre el ORDEN DE ÁREAS: de cada área se toma una misión y se le da a la PRIMERA persona (en el orden fijo
///     del personal) a la que aún le quepa en sus 5 h; luego la siguiente área, y así en vueltas hasta llenar a todos.
///  3. De las misiones del área, a esa persona se le da primero una que NUNCA haya hecho; si no, la que MENOS veces haya
///     hecho (y a igualdad, la que hace más días que no hace). Así todos acaban haciendo todas las misiones.
///  4. El orden de áreas ROTA cada día una posición (la última pasa a ser la primera); el del personal no cambia, así la
///     primera persona empieza cada día por un área distinta.
///  5. Si sobran misiones OBLIGATORIAS (las áreas deben estar cubiertas todos los días), la Magnate se DIVIDE: cada clon
///     toma 5 h de huecos y desaparece al terminar. Las demás que sobren pasan a PENDIENTES y son las primeras mañana.
/// DESDE EL 2026-11-05, con <see cref="initiationModel"/> (lo enciende <c>WorkdayDirector</c>): el modelo jornada/tarea. Entrenamiento 8–9 y 14–15; 5 h de tareas (9–11, 12–14, 15–16).
/// La INICIACIÓN es una tarea más: cada día se hace la lista de quién no sabe qué y se reparte PRIMERO, empezando por quien menos sabe: un recién llegado, toda su primera jornada;
/// luego, como mucho la mitad de sus horas; luego, lo que le falte. Después, las del santuario, también de quien menos sabe a quien más. Hoy inicia la Magnate; con
/// <see cref="instructorsTeach"/>, quien sepa la tarea y sepa iniciar, y ella si no hay nadie. Antes de asignar se miran los requisitos (<see cref="MissionRequirements"/>).
/// Datos puros (sin escena): lo usa <c>WorkdayDirector</c> en el juego y lo prueban los tests.
/// </summary>
public class WorkRoster
{
    public const float WorkHours = 5f, FirstBlock = 3f, LunchHours = 1f, SecondBlock = 2f, TrainingHours = 2f;
    public const float StartHour = 8f;   // 8–9 entrenamiento · 9–10 lección o trabajo · 10–11 trabajo · 11–12 comida · 12–14 trabajo · 14–16 entrenamiento · libre
    public const float MorningTraining = 1f, AfternoonTraining = 1f, LessonHours = 1f;
    /// <summary>Una iniciación es a la vez para hasta estos; en cada hora, como mucho estas clases (la Magnate se divide). Quien no cabe trabaja y aprende otro día.</summary>
    public const int LessonGroupSize = 6, MaxLessonGroups = 6;
    /// <summary>Tras su primer día, las horas que puede pasar iniciándose: la mitad de las 5 h, por lo bajo.</summary>
    public const float InitiationHalf = 2f;
    /// <summary>Saber iniciar a otros también se aprende, y lo aprenden todos (regla de oro), cuando ya saben unas cuantas tareas.</summary>
    public const string InstructSkill = "instruir";
    public const int MinSkillsToInstruct = 3;
    /// <summary>Las iniciaciones llevan estos prefijos: así ningún sistema las toma por la tarea de verdad (la clase de yoga, la brigada, las rondas).</summary>
    public const string LessonPrefix = "aprender_", InstructorPrefix = "instruir_";
    /// <summary>La misión de verdad detrás de una iniciación (o la misma, si no lo es).</summary>
    public static string LessonBase(string id) => id == null ? null : id.StartsWith(LessonPrefix) ? id.Substring(LessonPrefix.Length) : id.StartsWith(InstructorPrefix) ? id.Substring(InstructorPrefix.Length) : id;

    public class Mission
    {
        public string id, area, title;
        public string group;     // misiones que se hacen JUNTAS a la vez (la clase de yoga: guía, demostrador y correctores)
        public float hours;
        public bool mandatory;   // debe cubrirse cada día (iniciación de turistas, vigilar a los recién llegados…)
        // ESPACIO DE LA TAREA (lo que mide la Magnate): además de su área, otras áreas por las que la tarea hace ir
        // (llevar la cosecha a la cocina, reparar en otra área), la obra en la que se trabaja, o TODAS las áreas
        // (reparar maquinaria allí donde esté).
        public string[] also = new string[0];
        public string site;      // título de la obra (ConstructionProject) donde se trabaja
        public bool roams;
        public bool lead;        // DIRIGE (guiar la clase, demostrar, operar, iniciar a otros): no se le da a un recién llegado
        public string owner;     // PUESTO (regla de oro, J30): se le ofrece primero a esta persona (el entrenador y sus planes); sin ella, va por rotación, no se hace
        public Mission(string area, string id, string title, float hours, bool mandatory = false) { this.area = area; this.id = id; this.title = title; this.hours = hours; this.mandatory = mandatory; }
        public Mission Copy() => new Mission(area, id, title, hours, mandatory) { group = group, also = also, site = site, roams = roams, lead = lead, owner = owner };
        public override string ToString() => $"{area}: {title} ({hours:0.#} h)";
    }

    public class Worker
    {
        public string name;
        public bool clone;
        public readonly Dictionary<string, int> count = new Dictionary<string, int>();      // veces que ha hecho cada misión
        public readonly Dictionary<string, int> lastDay = new Dictionary<string, int>();    // último día que la hizo
        public int joined = int.MinValue;   // primer día que entró en la rotación
        /// <summary>Lo que le ha enseñado la Magnate (claves de <see cref="MissionRequirements.KnowledgeKey"/>).</summary>
        public readonly HashSet<string> taught = new HashSet<string>();
        public Worker(string name, bool clone = false) { this.name = name; this.clone = clone; }
        public int Count(string id) => count.TryGetValue(id, out int c) ? c : 0;
        public int Last(string id) => lastDay.TryGetValue(id, out int d) ? d : int.MinValue;
        public bool Knows(string id) => Count(id) > 0;
    }

    public class Assignment
    {
        public Worker worker; public Mission mission;
        public float start;          // hora del día en que empieza
        public bool firstTime;       // nunca la había hecho → la Magnate lo inicia
        public bool lesson;          // es una lección, no el trabajo: no tiene efecto en el santuario
        public override string ToString() => $"{start:00.0}h {worker.name} → {mission}{(firstTime ? " [iniciación]" : "")}";
    }

    /// <summary>Una clase de las 9:00: la Magnate (o una de sus divisiones) enseña una tarea a un grupo, en el área de esa tarea.</summary>
    public class Lesson
    {
        public string teacher; public Mission mission, real; public List<Worker> learners = new List<Worker>();
        public Worker instructor;   // null: la Magnate
        public float start;
        public override string ToString() => $"{teacher} enseña «{mission.title}» a {string.Join(", ", learners.Select(w => w.name))}";
    }

    public class DayPlan
    {
        public int day;
        public List<string> areaOrder = new List<string>();
        public List<Assignment> work = new List<Assignment>();
        public Dictionary<Worker, string> training = new Dictionary<Worker, string>();
        public List<Mission> backlog = new List<Mission>();          // pasan a mañana (por falta de personal)
        public List<Worker> clones = new List<Worker>();             // la Magnate dividida para las obligatorias
        public float hoursDemanded, hoursStaff;
        public Dictionary<Worker, string> morning = new Dictionary<Worker, string>();   // entrenamiento de las 8:00
        public List<Lesson> lessons = new List<Lesson>();            // la lista de iniciaciones del día, ya repartida
        public List<(Mission mission, string why)> blocked = new List<(Mission, string)>();   // sin sus requisitos: hoy no
        public List<Mission> unqualified = new List<Mission>();      // hoy nadie sabe: mañana vuelven solas al catálogo
        public int waitingLesson;                                    // les quedaba algo por aprender y no cupieron en ninguna clase
        public IEnumerable<Assignment> Of(Worker w) => work.Where(a => a.worker == w).OrderBy(a => a.start);
    }

    public readonly List<string> areas = new List<string>();          // orden base de áreas (día 0)
    public readonly List<Worker> workers = new List<Worker>();         // orden fijo del personal
    public readonly List<string> trainingAreas = new List<string>();   // entrenamientos abiertos (yoga, gimnasio…)
    public List<Mission> pending = new List<Mission>();                // pendientes de días anteriores

    /// <summary>(2026-11-05) La mañana nueva y los requisitos. Apagado, el reparto puro de siempre (el que usan las pruebas de la rotación).</summary>
    public bool initiationModel;
    /// <summary>Apagado (hoy): inicia siempre la Magnate. Encendido: quien sepa la tarea y sepa iniciar; ella, si no hay nadie libre. Cambiar esto es todo el paso.</summary>
    public bool instructorsTeach;
    /// <summary>¿Puede iniciar a otros en esta tarea? La Magnate, siempre (por eso no sale aquí): ella es el último recurso.</summary>
    public bool CanInitiate(Worker w, Mission m) => !w.clone && w.taught.Contains(InstructSkill) && Learned(w, m);
    /// <summary>Lo que le cabe de iniciación hoy: su primer día (entró después del primer reparto), toda la jornada; luego, la mitad.</summary>
    public float InitiationCap(Worker w, int day) => w.joined == day && day > _firstDay ? WorkHours : InitiationHalf;
    /// <summary>Requisitos del mundo: el motivo por el que no se puede hacer hoy, o null.</summary>
    public System.Func<Mission, string> worldCheck;

    /// <summary>¿Sabe? Lo hizo ya, se lo enseñó la Magnate o es de su profesión. Un clon es la Magnate: lo sabe todo.</summary>
    public bool Learned(Worker w, Mission m)
    {
        if (w.clone || w.Knows(m.id)) return true;
        if (w.taught.Contains(MissionRequirements.KnowledgeKey(m.id))) return true;
        return useProfessions && Professions.Practises(w.name, m.area, m.id);
    }

    /// <summary>¿Puede hacerla hoy? Solo las tareas con riesgo piden saber; las demás se aprenden haciéndolas (regla de oro J30).</summary>
    public bool Qualified(Worker w, Mission m) => !initiationModel || !MissionRequirements.NeedsLesson(m.id) || Learned(w, m);

    public Worker Add(string name) { var w = new Worker(name); workers.Add(w); return w; }
    public Worker Find(string name) => workers.FirstOrDefault(w => w.name == name);

    // RECIÉN LLEGADO (Berón 2026-10-29): sus primeras jornadas no dirige nada (ni guiar la clase de yoga desde la plataforma,
    // ni demostrar, ni operar, ni iniciar a otros); le tocan papeles de participante o de apoyo. Es quien entró en la rotación
    // después del primer reparto, durante sus NewcomerDays primeros días.
    public const int NewcomerDays = 3;
    int _firstDay = int.MinValue;
    public bool IsNewcomer(Worker w, int day) => w != null && !w.clone && _firstDay != int.MinValue && w.joined > _firstDay && day - w.joined < NewcomerDays;

    /// <summary>Orden de áreas del día: rota una posición por día (la última pasa a ser la primera).</summary>
    public List<string> AreaOrder(int day)
    {
        int n = areas.Count; var l = new List<string>(n);
        if (n == 0) return l;
        int shift = ((day % n) + n) % n;
        for (int i = 0; i < n; i++) l.Add(areas[((i - shift) % n + n) % n]);
        return l;
    }

    /// <summary>Planifica el día <paramref name="day"/> con las misiones del día (por área). Registra quién hace qué.</summary>
    public DayPlan Plan(int day, IEnumerable<Mission> todays)
    {
        var plan = new DayPlan { day = day, areaOrder = AreaOrder(day) };
        if (_firstDay == int.MinValue) _firstDay = day;
        foreach (Worker w in workers) if (w.joined == int.MinValue) w.joined = day;
        // Pools por área: pendientes de ayer → obligatorias → resto.
        var pools = new Dictionary<string, List<Mission>>();
        foreach (string a in areas) pools[a] = new List<Mission>();
        foreach (Mission m in pending.Concat(todays.Where(x => x.mandatory)).Concat(todays.Where(x => !x.mandatory)))
        {
            if (!pools.TryGetValue(m.area, out var p)) { pools[m.area] = p = new List<Mission>(); if (!areas.Contains(m.area)) { areas.Add(m.area); plan.areaOrder.Add(m.area); } }
            p.Add(m);
        }
        pending = new List<Mission>();
        // REQUISITOS DEL MUNDO: lo que hoy no se puede hacer no se reparte. Si venía de ayer, sigue pendiente; la del catálogo vuelve sola mañana.
        if (worldCheck != null)
            foreach (var pool in pools.Values)
                foreach (Mission m in pool.ToList())
                {
                    string why = worldCheck(m);
                    if (why == null) continue;
                    pool.Remove(m); plan.blocked.Add((m, why));
                }
        plan.hoursDemanded = pools.Values.Sum(p => p.Sum(m => m.hours));
        plan.hoursStaff = workers.Count * WorkHours;

        var used = workers.ToDictionary(w => w, w => 0f);
        List<Worker> order = workers;
        if (initiationModel) order = PlanInitiations(plan, pools, used);   // PRIMERO las iniciaciones; lo aprendido ya se puede hacer hoy, a partir de esa hora
        AssignOwned(plan, pools, used);    // las de puesto, primero a su dueño (si no, la rotación le llenaba la jornada); sin él, por rotación
        AssignGroups(plan, pools, used);   // primero las de grupo: todas a la misma hora, cada una a una persona distinta
        ProfessionRound(plan, pools, used);   // (2026-11-04) luego, a cada profesional una misión de cada área de su profesión
        RunRounds(plan, pools, order, used);

        // Obligatorias sin cubrir: la Magnate se divide (clones de 5 h) hasta cubrirlas todas.
        int cloneN = 0;
        while (pools.Values.Any(p => p.Any(m => m.mandatory)))
        {
            var c = new Worker($"Magnate (clon {++cloneN})", clone: true);
            plan.clones.Add(c); used[c] = 0f;
            var mandOnly = pools.ToDictionary(kv => kv.Key, kv => kv.Value.Where(m => m.mandatory).ToList());
            RunRounds(plan, mandOnly, new List<Worker> { c }, used);
            if (used[c] <= 0f) { plan.clones.Remove(c); break; }   // ninguna cabe en una jornada: queda pendiente
            foreach (var kv in pools) kv.Value.RemoveAll(m => m.mandatory && !mandOnly[kv.Key].Contains(m));
            if (cloneN > 50) break;   // salvaguarda
        }
        // El resto pasa a mañana (las primeras en asignarse). Las que nadie sabe hacer aún no se acumulan: mañana vuelven con el catálogo y entre tanto hay lecciones.
        foreach (string a in plan.areaOrder) if (pools.TryGetValue(a, out var p))
                foreach (Mission m in p)
                    if (initiationModel && !m.mandatory && MissionRequirements.NeedsLesson(m.id) && !workers.Any(w => Learned(w, m))) plan.unqualified.Add(m); else plan.backlog.Add(m);
        pending = plan.backlog.Select(m => m).ToList();

        // EL ENTRENAMIENTO DE LAS 8:00: rota con otra cuenta, para no repetir el de las 14 h.
        if (initiationModel && trainingAreas.Count > 0)
            for (int i = 0; i < workers.Count; i++) plan.morning[workers[i]] = trainingAreas[(i + day + 1) % trainingAreas.Count];
        // ENTRENAMIENTO (de tarde): rota también entre los abiertos (persona i, día d → área (i + d) mod n).
        if (trainingAreas.Count > 0)
            for (int i = 0; i < workers.Count; i++) plan.training[workers[i]] = trainingAreas[(i + day) % trainingAreas.Count];
        // (2026-11-04) LOS MAESTROS de una clase de entrenamiento (el baile: Panterilia y Gohageneis, un día cada uno) dan la suya si su
        // sala está abierta (terminada); si no, entrenan como los demás.
        if (useProfessions)
            foreach (var kv in Professions.ClassLeads)
            {
                if (!trainingAreas.Contains(kv.Key)) continue;
                Worker t = Find(Professions.LeadOn(kv.Key, day));
                if (t != null && !t.clone) plan.training[t] = kv.Key;
            }
        return plan;
    }


    // LAS INICIACIONES (Berón 2026-11-05): la lista del día sale de los personajes y de lo que no saben de lo que hay hoy. Se reparte antes que nada y de quien menos sabe a quien
    // más, llenando sus horas de iniciación desde la primera de la jornada; así queda claro si serán todas, la mitad o las que le falten. Por clases: quien puede, entra en una que ya
    // exista. Devuelve el orden del personal para repartir después las del santuario. Saber iniciar es una tarea más de la lista.
    List<Worker> PlanInitiations(DayPlan plan, Dictionary<string, List<Mission>> pools, Dictionary<Worker, float> used)
    {
        var options = pools.Values.SelectMany(p => p)
            .Where(m => string.IsNullOrEmpty(m.owner) && !FruitGardener.IsRound(m.id))
            .GroupBy(m => MissionRequirements.KnowledgeKey(m.id)).Select(g => g.First()).ToList();
        var skill = new Mission("Aula", InstructSkill, "Saber iniciar a otros", LessonHours);
        var skills = workers.ToDictionary(w => w, w => options.Count(m => Learned(w, m)));
        options.Add(skill);
        var order = workers.OrderBy(w => skills[w]).ToList();   // (estable: a igualdad, el orden fijo del personal)
        bool Wants(Worker w, Mission m) => !Learned(w, m) && (m != skill || skills[w] >= MinSkillsToInstruct);
        var scores = new Dictionary<Mission, int>();   // (se calcula una vez por hora: Practises parte cadenas)
        int Compute(Mission m) => workers.Count(w => !w.clone && Wants(w, m)) + (m.mandatory ? 100 : 0) + (MissionRequirements.NeedsLesson(m.id) ? 200 : 0) + (workers.Any(w => Learned(w, m)) || m == skill ? 0 : 400);
        int Score(Mission m) => scores[m];
        var done = workers.ToDictionary(w => w, w => 0f);
        var waiting = new HashSet<Worker>();
        for (int hour = 0; hour < WorkHours; hour++)
        {
            float clock = -1f;
            Place(hour, LessonHours, out clock);
            foreach (Mission m in options) scores[m] = Compute(m);
            var classes = new List<Lesson>();
            foreach (Worker w in order)
            {
                if (w.clone || System.Math.Abs(used[w] - hour) > 1e-3f || done[w] + LessonHours > InitiationCap(w, plan.day) + 1e-3f) continue;
                var mine = options.Where(m => Wants(w, m)).ToList();
                if (mine.Count == 0) continue;
                Lesson c = classes.Where(x => x.learners.Count < LessonGroupSize && mine.Contains(x.real)).OrderByDescending(x => Score(x.real)).FirstOrDefault();
                if (c == null)
                {
                    if (classes.Count >= MaxLessonGroups) { waiting.Add(w); continue; }
                    Mission best = mine.OrderByDescending(Score).First();
                    Mission shown = best.Copy();
                    shown.id = LessonPrefix + best.id; shown.title = "Iniciación: " + best.title; shown.hours = LessonHours; shown.group = null; shown.mandatory = false; shown.lead = false; shown.owner = null;
                    c = new Lesson { mission = shown, real = best, start = clock };
                    classes.Add(c);
                }
                c.learners.Add(w);
                w.taught.Add(MissionRequirements.KnowledgeKey(c.real.id));
                used[w] = hour + LessonHours; done[w] += LessonHours;
                plan.work.Add(new Assignment { worker = w, mission = c.mission, start = clock, lesson = true });
            }
            // QUIÉN INICIA: hoy, la Magnate; con instructorsTeach, quien sepa y esté libre a esa hora, y ella si no hay nadie.
            int magnates = 0;
            foreach (Lesson c in classes)
            {
                Worker t = !instructorsTeach ? null : workers.FirstOrDefault(w => System.Math.Abs(used[w] - hour) < 1e-3f && CanInitiate(w, c.real) && !c.learners.Contains(w));
                if (t != null)
                {
                    c.instructor = t; c.teacher = t.name; used[t] = hour + LessonHours;
                    Mission inst = c.mission.Copy(); inst.id = InstructorPrefix + c.real.id; inst.title = $"Iniciar a {c.learners.Count} en: {c.real.title}";
                    plan.work.Add(new Assignment { worker = t, mission = inst, start = clock, lesson = true });
                }
                else c.teacher = magnates++ == 0 ? "Magnate (iniciación)" : $"Magnate (iniciación {magnates})";
            }
            plan.lessons.AddRange(classes);
        }
        plan.waitingLesson = waiting.Count;
        return order;
    }

    // MISIONES DE PUESTO (Mission.owner): se le dan primero a su dueño, como un extra. REGLA DE ORO (Berón 2026-11-05, J30): todos saben hacer de
    // todo; si el dueño no está o no le cabe, la misión sigue en su área y la hace la rotación normal (antes, ese día no se hacía).
    void AssignOwned(DayPlan plan, Dictionary<string, List<Mission>> pools, Dictionary<Worker, float> used)
    {
        foreach (var pool in pools.Values)
            foreach (Mission m in pool.Where(x => !string.IsNullOrEmpty(x.owner)).ToList())
            {
                Worker w = Find(m.owner);
                if (w == null || Place(used[w], m.hours, out _) < 0f) continue;
                pool.Remove(m);
                Assign(plan, w, m, used);
            }
    }

    /// <summary>(2026-11-04) Usar las PROFESIONES del personal (<see cref="Professions"/>): cada uno, una misión al día de cada área de su
    /// profesión antes de las vueltas, y el profesional dirige la misión de grupo de su área si está libre. Sin ellas, rotación pura.</summary>
    public bool useProfessions = true;

    // LA RONDA DE LAS PROFESIONES: a cada uno (en el orden fijo del personal), una misión de cada área de su profesión: la que menos ha
    // hecho y que quepa. Lo demás sigue en las vueltas por áreas (todos aprenden todo, regla J2).
    void ProfessionRound(DayPlan plan, Dictionary<string, List<Mission>> pools, Dictionary<Worker, float> used)
    {
        if (!useProfessions) return;
        foreach (Worker w in workers)
        {
            if (w.clone) continue;
            foreach (string area in Professions.AreasOf(w.name).Distinct())
            {
                if (!pools.TryGetValue(area, out var pool) || pool.Count == 0) continue;
                var mine = pool.Where(m => Professions.Practises(w.name, area, m.id) && (!IsNewcomer(w, plan.day) || !m.lead) && string.IsNullOrEmpty(m.group)).ToList();   // (solo las de su profesión; las de grupo van en AssignGroups)
                Mission pick = Choose(w, mine.Where(m => Qualified(w, m)).ToList(), used[w]);
                if (pick == null) continue;
                pool.Remove(pick);
                Assign(plan, w, pick, used);
            }
        }
    }

    // MISIONES DE GRUPO: se hacen a la vez (p. ej. la clase de yoga necesita mínimo 4: guía en la plataforma, demostrador
    // y dos correctores). Se asignan al principio de la jornada (8:00), cada una a una persona distinta que rota cada día;
    // dentro del grupo, a cada uno el papel que menos ha hecho.
    void AssignGroups(DayPlan plan, Dictionary<string, List<Mission>> pools, Dictionary<Worker, float> used)
    {
        var groups = new Dictionary<string, List<Mission>>();
        foreach (var pool in pools.Values)
            foreach (Mission m in pool) if (!string.IsNullOrEmpty(m.group)) { if (!groups.TryGetValue(m.group, out var g)) groups[m.group] = g = new List<Mission>(); g.Add(m); }
        int offset = plan.day * 3;
        foreach (var kv in groups)
        {
            var roles = kv.Value;
            if (workers.Count < roles.Count) continue;   // sin gente suficiente: quedan para los clones
            // LA HORA DEL GRUPO: todos a la vez, en la primera franja con gente libre (con las iniciaciones repartidas ya, puede no ser la primera del día).
            var crew = new List<Worker>(); Worker pro = null; int support = roles.Count(m => !m.lead);
            bool ok = false; float free = 0f;
            foreach (float f in initiationModel ? new[] { 0f, 2f, 4f } : new[] { 0f })
            {
                if (roles.Any(m => Place(f, m.hours, out _) != f)) continue;
                crew.Clear(); free = f;
                // (2026-11-04) EL PROFESIONAL del área (el maestro de yoga en la clase), si está libre y no es recién llegado, entra y la dirige.
                pro = useProfessions ? workers.FirstOrDefault(w => !w.clone && used[w] <= f + 1e-3f && !IsNewcomer(w, plan.day) && Professions.Practises(w.name, roles[0].area, roles[0].id)) : null;
                if (pro != null) crew.Add(pro);   // (si hay papel que dirige, lo dirige; en una obra, entra el jefe de obra nueva)
                int newcomers = 0;
                for (int i = 0; crew.Count < roles.Count && i < workers.Count; i++)
                {
                    Worker w = workers[(offset + i) % workers.Count];
                    if (used[w] > f + 1e-3f || crew.Contains(w)) continue;
                    if (IsNewcomer(w, plan.day)) { if (newcomers >= support) continue; newcomers++; }   // al recién llegado, solo un papel de apoyo
                    crew.Add(w);
                }
                if (crew.Count >= roles.Count) { ok = true; break; }
            }
            if (!ok) continue;
            offset += roles.Count;
            foreach (Worker w in crew) used[w] = free;   // (empiezan todos a la vez)
            var pending = new List<Mission>(roles);
            // Primero los recién llegados (eligen entre los papeles de apoyo); luego el resto, entre lo que queda.
            foreach (Worker w in crew.OrderBy(x => IsNewcomer(x, plan.day) ? 0 : 1).ToList())
            {
                bool fresh = IsNewcomer(w, plan.day);
                Mission best = null;
                if (w == pro) best = pending.FirstOrDefault(m => m.lead);   // el profesional, el papel que dirige
                if (best == null) foreach (Mission m in pending) if ((!fresh || !m.lead) && (best == null || w.Count(m.id) < w.Count(best.id))) best = m;
                if (best == null) continue;
                pending.Remove(best);
                pools[best.area].Remove(best);
                Assign(plan, w, best, used);
            }
        }
    }
    // Vueltas por las áreas: de cada área, una misión a la primera persona a la que le quepa.
    void RunRounds(DayPlan plan, Dictionary<string, List<Mission>> pools, List<Worker> staff, Dictionary<Worker, float> used)
    {
        bool progress = true;
        while (progress)
        {
            progress = false;
            foreach (string area in plan.areaOrder)
            {
                if (!pools.TryGetValue(area, out var pool) || pool.Count == 0) continue;
                foreach (Worker w in staff)
                {
                    Mission pick = Choose(w, pool.Where(m => (!IsNewcomer(w, plan.day) || !m.lead) && Qualified(w, m)).ToList(), used[w]);
                    if (pick == null) continue;
                    pool.Remove(pick);
                    Assign(plan, w, pick, used);
                    progress = true;
                    break;
                }
            }
        }
    }

    // Nunca hecha → la menos hecha → la que hace más días que no hace (y que quepa en lo que le queda de jornada).
    Mission Choose(Worker w, List<Mission> pool, float used)
    {
        Mission best = null;
        foreach (Mission m in pool)
        {
            if (Place(used, m.hours, out _) < 0f) continue;
            if (w.name == "Player" && FruitGardener.IsRound(m.id)) continue;   // (2026-11-05) la ronda de frutales la hace la conducta FruitGardener, que no mueve al jugador: con Kushal, ese día se quedaba sin hacer
            if (best == null) { best = m; continue; }
            int cm = w.Count(m.id), cb = w.Count(best.id);
            if (cm < cb || (cm == cb && w.Last(m.id) < w.Last(best.id))) best = m;
        }
        return best;
    }

    void Assign(DayPlan plan, Worker w, Mission m, Dictionary<Worker, float> used)
    {
        // Lo que no cabe en lo que queda de su franja empieza en la siguiente (no se parte en la comida ni en el entrenamiento).
        float u = Place(used[w], m.hours, out float start);
        var a = new Assignment { worker = w, mission = m, start = start, firstTime = initiationModel ? !Learned(w, m) : !w.Knows(m.id) };
        plan.work.Add(a);
        used[w] = u + m.hours;
        w.count[m.id] = w.Count(m.id) + 1;
        w.lastDay[m.id] = plan.day;
    }

    // Las FRANJAS de trabajo: (desde, hasta) en horas de jornada y la hora de reloj en que empiezan.
    static readonly (float from, float to, float clock)[] OldBlocks = { (0f, FirstBlock, StartHour), (FirstBlock, WorkHours, StartHour + FirstBlock + LunchHours) };
    static readonly (float from, float to, float clock)[] NewBlocks = { (0f, 2f, 9f), (2f, 4f, 12f), (4f, 5f, 15f) };

    /// <summary>Dónde cabe una misión de <paramref name="h"/> horas a quien lleva <paramref name="used"/>: las horas de jornada en que empieza (−1 si no cabe) y su hora de reloj.</summary>
    public float Place(float used, float h, out float clock)
    {
        foreach (var b in initiationModel ? NewBlocks : OldBlocks)
        {
            float u = System.Math.Max(used, b.from);
            if (u + h <= b.to + 1e-4f) { clock = b.clock + u - b.from; return u; }
        }
        clock = -1f;
        return -1f;
    }

    /// <summary>Las horas de jornada que han pasado a la hora de reloj <paramref name="clock"/> (para quien se incorpora a medias).</summary>
    public float UsedAt(float clock)
    {
        float u = 0f;
        foreach (var b in initiationModel ? NewBlocks : OldBlocks) u += System.Math.Max(0f, System.Math.Min(b.to - b.from, clock - b.clock));
        return u;
    }

    /// <summary>¿Cabe una misión de <paramref name="h"/> horas a quien ya lleva <paramref name="used"/>? (en el reparto puro: sin partirla en la comida)</summary>
    public static bool Fits(float used, float h)
    {
        if (used + h <= FirstBlock + 1e-4f) return true;
        return System.Math.Max(used, FirstBlock) + h <= WorkHours + 1e-4f;
    }

    /// <summary>EQUILIBRIO: ¿faltan personas para todas las misiones, o faltan misiones en algún área?</summary>
    public static List<string> Balance(DayPlan p, int workersCount)
    {
        var l = new List<string>();
        if (p.clones.Count > 0) l.Add($"Faltan personas: la Magnate se dividió en {p.clones.Count} clon(es) para cubrir las misiones obligatorias.");
        if (p.backlog.Count > 0) l.Add($"Faltan personas: {p.backlog.Count} misión(es) ({p.backlog.Sum(m => m.hours):0.#} h) pasan a mañana.");
        float idle = workersCount * WorkHours - p.work.Where(a => !a.worker.clone).Sum(a => a.mission.hours);
        if (idle > 0.5f && p.backlog.Count == 0) l.Add($"Faltan misiones: {idle:0.#} h de trabajo sin asignar; conviene crear misiones en las áreas con menos carga.");
        var perArea = p.work.GroupBy(a => a.mission.area).ToDictionary(g => g.Key, g => g.Sum(a => a.mission.hours));
        foreach (string a in p.areaOrder) if (!perArea.ContainsKey(a)) l.Add($"El área «{a}» no tiene misiones hoy.");
        if (p.blocked.Count > 0) l.Add($"Sin sus requisitos, hoy no se hacen {p.blocked.Count}: {string.Join("; ", p.blocked.Take(4).Select(b => $"{b.mission.id} ({b.why})"))}.");
        if (p.unqualified.Count > 0) l.Add($"Nadie sabe hacer aún {p.unqualified.Count} misión(es): {string.Join(", ", p.unqualified.Take(6).Select(m => m.id))}.");
        if (p.waitingLesson > 0) l.Add($"{p.waitingLesson} persona(s) sin sitio en las iniciaciones de hoy: trabajan y aprenden otro día.");
        if (l.Count == 0) l.Add("Equilibrado: todas las misiones cubiertas por el personal, sin clones ni pendientes.");
        return l;
    }
}
