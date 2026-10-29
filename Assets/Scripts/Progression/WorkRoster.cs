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
/// Datos puros (sin escena): lo usa <c>WorkdayDirector</c> en el juego y lo prueban los tests.
/// </summary>
public class WorkRoster
{
    public const float WorkHours = 5f, FirstBlock = 3f, LunchHours = 1f, SecondBlock = 2f, TrainingHours = 2f;
    public const float StartHour = 8f;   // 8:00 → 11:00 trabajo · 11–12 comida · 12–14 trabajo · 14–16 entrenamiento · libre

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
        public Mission(string area, string id, string title, float hours, bool mandatory = false) { this.area = area; this.id = id; this.title = title; this.hours = hours; this.mandatory = mandatory; }
        public Mission Copy() => new Mission(area, id, title, hours, mandatory) { group = group, also = also, site = site, roams = roams, lead = lead };
        public override string ToString() => $"{area}: {title} ({hours:0.#} h)";
    }

    public class Worker
    {
        public string name;
        public bool clone;
        public readonly Dictionary<string, int> count = new Dictionary<string, int>();      // veces que ha hecho cada misión
        public readonly Dictionary<string, int> lastDay = new Dictionary<string, int>();    // último día que la hizo
        public int joined = int.MinValue;   // primer día que entró en la rotación
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
        public override string ToString() => $"{start:00.0}h {worker.name} → {mission}{(firstTime ? " [iniciación]" : "")}";
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
        public IEnumerable<Assignment> Of(Worker w) => work.Where(a => a.worker == w).OrderBy(a => a.start);
    }

    public readonly List<string> areas = new List<string>();          // orden base de áreas (día 0)
    public readonly List<Worker> workers = new List<Worker>();         // orden fijo del personal
    public readonly List<string> trainingAreas = new List<string>();   // entrenamientos abiertos (yoga, gimnasio…)
    public List<Mission> pending = new List<Mission>();                // pendientes de días anteriores

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
        plan.hoursDemanded = pools.Values.Sum(p => p.Sum(m => m.hours));
        plan.hoursStaff = workers.Count * WorkHours;

        var used = workers.ToDictionary(w => w, w => 0f);
        AssignGroups(plan, pools, used);   // primero las de grupo: todas a la misma hora, cada una a una persona distinta
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
        // El resto pasa a mañana (las primeras en asignarse).
        foreach (string a in plan.areaOrder) if (pools.TryGetValue(a, out var p)) plan.backlog.AddRange(p);
        pending = plan.backlog.Select(m => m).ToList();

        // ENTRENAMIENTO (2 h): rota también entre los abiertos (persona i, día d → área (i + d) mod n).
        if (trainingAreas.Count > 0)
            for (int i = 0; i < workers.Count; i++) plan.training[workers[i]] = trainingAreas[(i + day) % trainingAreas.Count];
        return plan;
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
            int support = roles.Count(m => !m.lead), newcomers = 0;
            for (int i = 0; crew.Count < roles.Count && i < workers.Count; i++)
            {
                Worker w = workers[(offset + i) % workers.Count];
                if (used[w] > 0f || crew.Contains(w)) continue;
                if (IsNewcomer(w, plan.day)) { if (newcomers >= support) continue; newcomers++; }   // al recién llegado, solo un papel de apoyo
                crew.Add(w);
            }
            if (crew.Count < roles.Count) continue;
            offset += roles.Count;
            var pending = new List<Mission>(roles);
            // Primero los recién llegados (eligen entre los papeles de apoyo); luego el resto, entre lo que queda.
            foreach (Worker w in crew.OrderBy(x => IsNewcomer(x, plan.day) ? 0 : 1).ToList())
            {
                bool fresh = IsNewcomer(w, plan.day);
                Mission best = null;
                foreach (Mission m in pending) if ((!fresh || !m.lead) && (best == null || w.Count(m.id) < w.Count(best.id))) best = m;
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
                    Mission pick = Choose(w, IsNewcomer(w, plan.day) ? pool.Where(m => !m.lead).ToList() : pool, used[w]);
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
        var a = new Assignment { worker = w, mission = m, start = start, firstTime = !w.Knows(m.id) };
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
        float idle = workersCount * WorkHours - p.work.Where(a => !a.worker.clone).Sum(a => a.mission.hours);
        if (idle > 0.5f && p.backlog.Count == 0) l.Add($"Faltan misiones: {idle:0.#} h de trabajo sin asignar; conviene crear misiones en las áreas con menos carga.");
        var perArea = p.work.GroupBy(a => a.mission.area).ToDictionary(g => g.Key, g => g.Sum(a => a.mission.hours));
        foreach (string a in p.areaOrder) if (!perArea.ContainsKey(a)) l.Add($"El área «{a}» no tiene misiones hoy.");
        if (l.Count == 0) l.Add("Equilibrado: todas las misiones cubiertas por el personal, sin clones ni pendientes.");
        return l;
    }
}
