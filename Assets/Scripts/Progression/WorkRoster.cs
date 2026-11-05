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
/// DESDE EL 2026-11-05, con <see cref="dailyLearning"/> (lo enciende <c>WorkdayDirector</c>): la 1.ª hora (8–9) es entrenamiento para todos y la 2.ª (9–10), la LECCIÓN: a quien
/// le quedan tareas por aprender, la Magnate le enseña una en persona, en grupos; a quien no, trabaja. Antes de asignar se miran los requisitos (<see cref="MissionRequirements"/>).
/// Las 5 h siguen siendo el tope: incluyen esas horas, así que quedan 4 h de trabajo (3 con lección).
/// Datos puros (sin escena): lo usa <c>WorkdayDirector</c> en el juego y lo prueban los tests.
/// </summary>
public class WorkRoster
{
    public const float WorkHours = 5f, FirstBlock = 3f, LunchHours = 1f, SecondBlock = 2f, TrainingHours = 2f;
    public const float StartHour = 8f;   // 8–9 entrenamiento · 9–10 lección o trabajo · 10–11 trabajo · 11–12 comida · 12–14 trabajo · 14–16 entrenamiento · libre
    public const float MorningTraining = 1f, LessonHours = 1f;
    /// <summary>Una Magnate enseña a la vez a estos, y como mucho se divide en estas clases: lo que dura su hora. Quien no cabe trabaja y aprende otro día.</summary>
    public const int LessonGroupSize = 6, MaxLessonGroups = 6;
    /// <summary>Las misiones de una lección llevan este prefijo: así ningún sistema las toma por la de verdad (la clase de yoga, la brigada, las rondas).</summary>
    public const string LessonPrefix = "aprender_";
    /// <summary>La misión de verdad detrás de una lección (o la misma, si no lo es).</summary>
    public static string LessonBase(string id) => id != null && id.StartsWith(LessonPrefix) ? id.Substring(LessonPrefix.Length) : id;

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
        public string teacher; public Mission mission; public List<Worker> learners = new List<Worker>();
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
        public List<Lesson> lessons = new List<Lesson>();            // las clases de las 9:00
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
    public bool dailyLearning;
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
    public bool Qualified(Worker w, Mission m) => !dailyLearning || !MissionRequirements.NeedsLesson(m.id) || Learned(w, m);

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
        plan.hoursStaff = workers.Count * (WorkHours - (dailyLearning ? MorningTraining : 0f));

        var used = workers.ToDictionary(w => w, w => dailyLearning ? MorningTraining : 0f);
        if (dailyLearning) PlanLessons(plan, pools, used);   // antes que nada: lo que aprenden a las 9 lo pueden hacer ya hoy
        AssignOwned(plan, pools, used);    // las de puesto, primero a su dueño (si no, la rotación le llenaba la jornada); sin él, por rotación
        AssignGroups(plan, pools, used);   // primero las de grupo: todas a la misma hora, cada una a una persona distinta
        ProfessionRound(plan, pools, used);   // (2026-11-04) luego, a cada profesional una misión de cada área de su profesión
        RunRounds(plan, pools, workers, used);

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
                    if (dailyLearning && !m.mandatory && MissionRequirements.NeedsLesson(m.id) && !workers.Any(w => Learned(w, m))) plan.unqualified.Add(m); else plan.backlog.Add(m);
        pending = plan.backlog.Select(m => m).ToList();

        // EL ENTRENAMIENTO DE LAS 8:00: rota con otra cuenta, para no repetir el de las 14 h.
        if (dailyLearning && trainingAreas.Count > 0)
            for (int i = 0; i < workers.Count; i++) plan.morning[workers[i]] = trainingAreas[(i + day + 1) % trainingAreas.Count];
        // ENTRENAMIENTO (2 h): rota también entre los abiertos (persona i, día d → área (i + d) mod n).
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


    // LA HORA DE LA LECCIÓN (Berón 2026-11-05): a quien le queda algo por aprender de lo que hay hoy, la Magnate le enseña una tarea. Va por grupos: primero lo que hoy nadie
    // sabe, luego lo obligatorio, luego lo que más gente tiene pendiente. Una clase son hasta LessonGroupSize; si hacen falta varias a la vez, se divide (MaxLessonGroups).
    void PlanLessons(DayPlan plan, Dictionary<string, List<Mission>> pools, Dictionary<Worker, float> used)
    {
        var options = pools.Values.SelectMany(p => p)
            .Where(m => !FruitGardener.IsRound(m.id))   // (J30: las de puesto también se enseñan; sin su dueño las hace la rotación)
            .GroupBy(m => MissionRequirements.KnowledgeKey(m.id)).Select(g => g.First()).ToList();
        var left = workers.Where(w => !w.clone && options.Any(m => !Learned(w, m))).ToList();
        while (left.Count > 0 && plan.lessons.Count < MaxLessonGroups)
        {
            Mission best = null; int bestScore = 0;
            foreach (Mission m in options)
            {
                int n = left.Count(w => !Learned(w, m));
                if (n == 0) continue;
                int score = n + (m.mandatory ? 100 : 0) + (MissionRequirements.NeedsLesson(m.id) ? 200 : 0) + (workers.Any(w => Learned(w, m)) ? 0 : 400);
                if (score > bestScore) { bestScore = score; best = m; }
            }
            if (best == null) break;
            var group = left.Where(w => !Learned(w, best)).Take(LessonGroupSize).ToList();
            var lesson = new Lesson { teacher = plan.lessons.Count == 0 ? "Magnate (lección)" : $"Magnate (lección {plan.lessons.Count + 1})", learners = group };
            Mission shown = best.Copy();
            shown.id = LessonPrefix + best.id; shown.title = "Aprender con la Magnate: " + best.title; shown.hours = LessonHours; shown.group = null; shown.mandatory = false; shown.lead = false; shown.owner = null;
            lesson.mission = shown;
            plan.lessons.Add(lesson);
            string key = MissionRequirements.KnowledgeKey(best.id);
            foreach (Worker w in group)
            {
                w.taught.Add(key);
                used[w] = MorningTraining + LessonHours;
                plan.work.Add(new Assignment { worker = w, mission = shown, start = StartHour + MorningTraining, lesson = true });
                left.Remove(w);
            }
        }
        plan.waitingLesson = left.Count;
    }
    // MISIONES DE PUESTO (Mission.owner): se le dan primero a su dueño, como un extra. REGLA DE ORO (Berón 2026-11-05, J30): todos saben hacer de
    // todo; si el dueño no está o no le cabe, la misión sigue en su área y la hace la rotación normal (antes, ese día no se hacía).
    void AssignOwned(DayPlan plan, Dictionary<string, List<Mission>> pools, Dictionary<Worker, float> used)
    {
        foreach (var pool in pools.Values)
            foreach (Mission m in pool.Where(x => !string.IsNullOrEmpty(x.owner)).ToList())
            {
                Worker w = Find(m.owner);
                if (w == null || !Fits(used[w], m.hours)) continue;
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
            var crew = new List<Worker>();
            // (2026-11-04) EL PROFESIONAL del área (el maestro de yoga en la clase), si está libre y no es recién llegado, entra y la dirige.
            float free = dailyLearning ? MorningTraining + LessonHours : 0f;   // (hasta las 10:00, para que quepan los que vienen de su lección y empiecen todos a la vez)
            Worker pro = useProfessions ? workers.FirstOrDefault(w => !w.clone && used[w] <= free && !IsNewcomer(w, plan.day) && Professions.Practises(w.name, roles[0].area, roles[0].id)) : null;
            if (pro != null) crew.Add(pro);   // (si hay papel que dirige, lo dirige; en una obra, entra el jefe de obra nueva)
            int support = roles.Count(m => !m.lead), newcomers = 0;
            for (int i = 0; crew.Count < roles.Count && i < workers.Count; i++)
            {
                Worker w = workers[(offset + i) % workers.Count];
                if (used[w] > free || crew.Contains(w)) continue;
                if (IsNewcomer(w, plan.day)) { if (newcomers >= support) continue; newcomers++; }   // al recién llegado, solo un papel de apoyo
                crew.Add(w);
            }
            if (crew.Count < roles.Count) continue;
            offset += roles.Count;
            foreach (Worker w in crew) used[w] = free;
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
    static Mission Choose(Worker w, List<Mission> pool, float used)
    {
        Mission best = null;
        foreach (Mission m in pool)
        {
            if (!Fits(used, m.hours)) continue;
            if (w.name == "Player" && FruitGardener.IsRound(m.id)) continue;   // (2026-11-05) la ronda de frutales la hace la conducta FruitGardener, que no mueve al jugador: con Kushal, ese día se quedaba sin hacer
            if (best == null) { best = m; continue; }
            int cm = w.Count(m.id), cb = w.Count(best.id);
            if (cm < cb || (cm == cb && w.Last(m.id) < w.Last(best.id))) best = m;
        }
        return best;
    }

    void Assign(DayPlan plan, Worker w, Mission m, Dictionary<Worker, float> used)
    {
        float u = used[w];
        // Bloque 1: 8–11; comida 11–12; bloque 2: 12–14. Lo que no cabe en lo que queda del bloque 1 empieza en el 2.
        if (u + m.hours > FirstBlock + 1e-4f && u < FirstBlock) u = FirstBlock;
        float start = u < FirstBlock ? StartHour + u : StartHour + LunchHours + u;
        var a = new Assignment { worker = w, mission = m, start = start, firstTime = dailyLearning ? !Learned(w, m) : !w.Knows(m.id) };
        plan.work.Add(a);
        used[w] = u + m.hours;
        w.count[m.id] = w.Count(m.id) + 1;
        w.lastDay[m.id] = plan.day;
    }

    /// <summary>¿Cabe una misión de <paramref name="h"/> horas a quien ya lleva <paramref name="used"/>? (sin partirla en la comida)</summary>
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
        float idle = workersCount * WorkHours - p.work.Where(a => !a.worker.clone).Sum(a => a.mission.hours) - p.morning.Count * MorningTraining;
        if (idle > 0.5f && p.backlog.Count == 0) l.Add($"Faltan misiones: {idle:0.#} h de trabajo sin asignar; conviene crear misiones en las áreas con menos carga.");
        var perArea = p.work.GroupBy(a => a.mission.area).ToDictionary(g => g.Key, g => g.Sum(a => a.mission.hours));
        foreach (string a in p.areaOrder) if (!perArea.ContainsKey(a)) l.Add($"El área «{a}» no tiene misiones hoy.");
        if (p.blocked.Count > 0) l.Add($"Sin sus requisitos, hoy no se hacen {p.blocked.Count}: {string.Join("; ", p.blocked.Take(4).Select(b => $"{b.mission.id} ({b.why})"))}.");
        if (p.unqualified.Count > 0) l.Add($"Nadie sabe hacer aún {p.unqualified.Count} misión(es): {string.Join(", ", p.unqualified.Take(6).Select(m => m.id))}.");
        if (p.waitingLesson > 0) l.Add($"{p.waitingLesson} persona(s) sin sitio en las clases de hoy: trabajan a las 9 y aprenden otro día.");
        if (l.Count == 0) l.Add("Equilibrado: todas las misiones cubiertas por el personal, sin clones ni pendientes.");
        return l;
    }
}
