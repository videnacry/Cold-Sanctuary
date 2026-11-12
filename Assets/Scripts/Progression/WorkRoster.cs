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
/// luego, como mucho la mitad de sus horas; luego, lo que le falte. Después, las del santuario, también de quien menos sabe a quien más. Inicia quien sepa la tarea y sepa iniciar
/// (<see cref="instructorsTeach"/>); la Magnate, si no hay nadie libre. Antes de asignar se miran los requisitos (<see cref="MissionRequirements"/>).
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
        /// <summary>(Berón 2026-11-08) OPCIONAL: si ningún humano tiene tiempo, ese día no se hace; nunca lleva un clon ni pasa a mañana. Sale de
        /// <see cref="OptionalIds"/>; una obligatoria nunca lo es.</summary>
        public bool optional;
        /// <summary>(2026-11-09, ronda 33) DIVISIBLE: se puede hacer a trozos de 1 h o de media (las rondas de frutales, la patrulla, el alimentador, el vínculo
        /// con una cría), para llenar los huecos de 0,5–1 h que dejan las franjas de 2, 2 y 1 h. Sale de <see cref="IsDivisible"/>.</summary>
        public bool divisible;
        /// <summary>Si es un TROZO de una tarea partida: aplica el efecto (lo que la tarea hace en el mundo) solo el último en acabar.</summary>
        public bool piece, effectPiece = true;
        public Mission(string area, string id, string title, float hours, bool mandatory = false) { this.area = area; this.id = id; this.title = title; this.hours = hours; this.mandatory = mandatory; optional = !mandatory && IsOptional(id); divisible = IsDivisible(id); }
        public Mission Copy() => new Mission(area, id, title, hours, mandatory) { group = group, also = also, site = site, roams = roams, lead = lead, owner = owner, optional = optional, divisible = divisible, piece = piece, effectPiece = effectPiece };
        public override string ToString() => $"{area}: {title} ({hours:0.#} h)";
    }

    public class Worker
    {
        public string name;
        public bool clone;
        public readonly Dictionary<string, int> count = new Dictionary<string, int>();      // veces que ha hecho cada misión
        public readonly Dictionary<string, int> lastDay = new Dictionary<string, int>();    // último día que la hizo
        public int joined = int.MinValue;   // primer día que entró en la rotación
        /// <summary>(2026-11-08) Su CASA como posición (<see cref="SanctuaryPlacement.Home.Position"/>): 0 = santuario 1 nivel 1. Solo cuenta con <see cref="sanctuaryModel"/>.</summary>
        public int home;
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
        // (2026-11-08) EL REPARTO POR SANTUARIOS (sanctuaryModel): dónde pasa cada uno el día y lo que pide cada sitio.
        public Dictionary<Worker, SanctuaryPlacement.Home> place = new Dictionary<Worker, SanctuaryPlacement.Home>();
        public Dictionary<SanctuaryPlacement.Home, float> hoursAt = new Dictionary<SanctuaryPlacement.Home, float>();   // horas de tareas de cada sitio
        public Dictionary<SanctuaryPlacement.Home, float> share = new Dictionary<SanctuaryPlacement.Home, float>();     // la parte igual de cada humano del sitio (≤ 5 h)
        public Dictionary<string, float> hoursByArea = new Dictionary<string, float>();   // horas de tareas de cada área ENTERA (AreaGroup), de su lista de hoy
        public Dictionary<string, int> quota = new Dictionary<string, int>();             // personas que pide cada área: ⌈horas / 5⌉, al menos 1
        public List<string> cloneAreas = new List<string>();           // el área (la primera de sus tareas) de cada clon
        public int emptyAreaClones;                                    // de ellos, los de áreas que se quedaban sin nadie
        public List<Worker> secondSplit = new List<Worker>();          // sobraban tras cubrir las horas: segundo reparto
        public List<Mission> skipped = new List<Mission>();            // opcionales que hoy no se hacen (nadie tenía tiempo): no pasan a mañana
        public List<string> cloneReasons = new List<string>();        // (2026-11-09) por qué salió cada clon: «clon para «área»: motivo …»
        public List<Mission> impossible = new List<Mission>();         // (solo registro) tareas que nadie, ni un clon, puede hacer hoy: debe quedar vacía
        public IEnumerable<Assignment> Of(Worker w) => work.Where(a => a.worker == w).OrderBy(a => a.start);
    }

    public readonly List<string> areas = new List<string>();          // orden base de áreas (día 0)
    public readonly List<Worker> workers = new List<Worker>();         // orden fijo del personal
    public readonly List<string> trainingAreas = new List<string>();   // entrenamientos abiertos (yoga, gimnasio…)
    public List<Mission> pending = new List<Mission>();                // pendientes de días anteriores

    /// <summary>(2026-11-05) La mañana nueva y los requisitos. Apagado, el reparto puro de siempre (el que usan las pruebas de la rotación).</summary>
    public bool initiationModel;
    /// <summary>(Berón 2026-11-08) EL REPARTO POR SANTUARIOS (lo enciende <c>WorkdayDirector</c>; docs/jornada-y-rotacion.md §Reparto por santuarios):
    ///  1. Cada uno pasa el día en UN sitio (santuario y nivel) al que puede ir (<see cref="SanctuaryPlacement.CanAccess"/>). Primero se cubren las horas
    ///     de cada sitio, empezando por el que menos gente alcanza; los que sobran van, en un segundo reparto, al sitio con más horas por persona.
    ///  2. El orden del personal es una PERMUTACIÓN sembrada con el día (<see cref="DayOrder"/>): el reparto cambia cada día.
    ///  3. PARTES IGUALES (Berón 2026-11-08, corregido): las horas de tareas del sitio se reparten por igual entre sus humanos (cada tarea, a quien menos
    ///     lleva). Si la parte pasa de 5 h, cada humano hace 5 h y el resto lo hacen clones de la Magnate; si no, todos la misma parte, menor.
    ///     Tolerancia: entre quien más y quien menos lleva del sitio, como mucho la tarea más larga del sitio (las tareas no se parten).
    ///  4. NADA PASA A MAÑANA: lo que no cabe en los humanos lo hacen clones (de 5 h), obligatorio o no. Un área que nadie de su sitio puede hacer
    ///     recibe primero una iniciación (J30) y, si aun así se queda sin nadie, su clon. Sin tareas, nadie. Las clases sin maestro siguen siendo suyas.
    /// Apagado: el reparto puro de siempre (orden fijo, el primero al que le cabe, 5 h, clones para las obligatorias, pendientes para mañana).</summary>
    public bool sanctuaryModel;
    /// <summary>El sitio de cada área (por defecto, la tabla de <see cref="SanctuaryPlacement.PlaceOfArea"/>).</summary>
    public System.Func<string, SanctuaryPlacement.Home> placeOf = SanctuaryPlacement.PlaceOfArea;
    /// <summary>EL ÁREA ENTERA (Berón 2026-11-08): un área de varios pisos o salas cuenta como UNA para el reparto (horas, personas que pide y «al
    /// menos 1»). Las misiones ya usan un solo nombre por área («Hospital», «Sala de Yoga»); esto junta además los nombres de sus pisos o salas
    /// («Sala de Yoga · Hatha tradicional», «Hospital veterinario…») si alguno llega como área propia.</summary>
    /// <summary>LAS OPCIONALES (Berón 2026-11-08; docs/tareas-por-area.md): investigación, formación, cultura, cuentas y avisos, y lo que puede
    /// esperar. Nunca la seguridad, el cuidado de seres vivos (pacientes, crías, animales, plantas), la comida del día ni las obras.</summary>
    public static readonly HashSet<string> OptionalIds = new HashSet<string>
    {
        "coc_cuentas", "coc_avisos", "asa_mediar", "mec_vehiculos", "tex_tejer", "gym_mantener", "sol_instalacion", "lanz_fabrica",
        "qui_sintesis", "bio_pcr", "fis_experimentos", "aul_estudio", "aul_letras", "aul_lenguas", "son_grabar", "son_ensayo",
    };
    public static bool IsOptional(string id) => id != null && OptionalIds.Contains(id);

    /// <summary>LAS DIVISIBLES (Berón 2026-11-09, ronda 33): las rondas de frutales (sus tramos ya son de ≤ 1 h: FruitGardener.TreeStretches), la patrulla,
    /// el alimentador y el vínculo con una cría. El quirófano y la veterinaria, no: van enteros.</summary>
    public static readonly HashSet<string> DivisibleIds = new HashSet<string> { "gua_carnivoros", "gua_alimentador", "gua_alimentador_2", "cri_vinculo", "huv_cosecha" };
    public static bool IsDivisible(string id) => id != null && (DivisibleIds.Contains(id) || FruitGardener.IsRound(id) || id.StartsWith("prep_"));   // (ronda 35: la cosecha del huerto vertical y las elaboraciones de la cocina también)
    /// <summary>El trozo más pequeño de una tarea divisible (h).</summary>
    public const float MinPiece = 0.5f;

    public static string AreaGroup(string area)
    {
        if (area == null) return null;
        if (area.StartsWith("Sala de Yoga")) return "Sala de Yoga";
        if (area.StartsWith(AreaWork.Hospital)) return AreaWork.Hospital;
        return area;
    }
    readonly Dictionary<Worker, SanctuaryPlacement.Home> _place = new Dictionary<Worker, SanctuaryPlacement.Home>();

    /// <summary>¿Puede <paramref name="w"/> hacer hoy <paramref name="m"/>? Con el reparto por santuarios, solo lo de su sitio del día (un clon, todo).</summary>
    public bool Allowed(Worker w, Mission m) => !sanctuaryModel || w == null || w.clone || !_place.TryGetValue(w, out var p) || p.Equals(placeOf(m.area));

    /// <summary>EL ORDEN DEL PERSONAL DEL DÍA (Berón 2026-11-08): una permutación sembrada con el día; dos días seguidos nunca dan el mismo orden
    /// (con 2 o 3 personas, el orden rota). Kushal va siempre el primero: el jugador tiene su jornada entera (sus 5 h, PlayerWorkday); el orden de
    /// los demás es el que cambia (INFERIDO). Apagado el reparto por santuarios, el orden fijo de siempre.</summary>
    public List<Worker> DayOrder(int day)
    {
        if (!sanctuaryModel) return new List<Worker>(workers);
        var others = workers.Where(w => w.name != "Player").ToList();
        List<Worker> today;
        int n = others.Count;
        if (n <= 3) { today = new List<Worker>(n); int k = n == 0 ? 0 : ((day % n) + n) % n; for (int i = 0; i < n; i++) today.Add(others[(i + k) % n]); }
        else
        {
            today = Shuffle(others, day);
            if (today.SequenceEqual(Shuffle(others, day - 1))) { Worker first = today[0]; today.RemoveAt(0); today.Add(first); }   // (1 de n!: rota uno)
        }
        Worker me = workers.FirstOrDefault(w => w.name == "Player");
        if (me != null) today.Insert(0, me);
        return today;
    }

    static List<Worker> Shuffle(List<Worker> from, int day)
    {
        var l = new List<Worker>(from);
        uint x = (uint)day * 2654435761u ^ 0x9E3779B9u; if (x == 0u) x = 1u;
        for (int i = l.Count - 1; i > 0; i--)
        {
            x ^= x << 13; x ^= x >> 17; x ^= x << 5;   // xorshift32: el mismo en cualquier máquina
            int j = (int)(x % (uint)(i + 1));
            (l[i], l[j]) = (l[j], l[i]);
        }
        return l;
    }
    /// <summary>Encendido (Berón 2026-11-05, diseño final): inicia quien sepa la tarea y sepa iniciar; la Magnate, si no hay nadie libre. Apagado: siempre ella.</summary>
    public bool instructorsTeach = true;
    /// <summary>¿Puede iniciar a otros en esta tarea? La Magnate, siempre (por eso no sale aquí): ella es el último recurso.</summary>
    public bool CanInitiate(Worker w, Mission m) => !w.clone && w.taught.Contains(InstructSkill) && Learned(w, m);
    /// <summary>Lo que le cabe de iniciación hoy: su primer día (entró después del primer reparto), toda la jornada; luego, la mitad.</summary>
    public float InitiationCap(Worker w, int day) => (w.joined == day && day > _firstDay) || RemoteLearner(w) ? WorkHours : InitiationHalf;
    /// <summary>(ronda 35) Quien pasa el día en el Marino o el Aéreo se inicia en todo lo de su sitio el primer día que haga falta, la jornada entera si hace
    /// falta: allí solo están ellos, y si no lo saben todo, lo de su sitio lo hacen clones «por no saber».</summary>
    bool RemoteLearner(Worker w) => sanctuaryModel && _place.TryGetValue(w, out var p) && p.sanctuary != SanctuaryPlacement.Land;
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
        // (2026-11-08) UNA TAREA DE CADA DÍA que quedó pendiente y hoy vuelve en la lista es la MISMA tarea: se cuenta una vez (antes se sumaban las dos,
        // y con cada día sin hacer las horas del área crecían: el Huerto pedía 61 h con las rondas de frutales repetidas). La de ayer va primero.
        todays = todays.ToList();
        var again = new HashSet<string>(pending.Select(m => m.id));
        var fresh = todays.Where(x => !again.Contains(x.id)).ToList();
        foreach (Mission m in pending.Concat(fresh.Where(x => x.mandatory)).Concat(fresh.Where(x => !x.mandatory)))
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

        // (Berón 2026-11-08) LAS OPCIONALES aparte: primero se reparten por igual las obligadas; después, las opcionales a quien aún tenga tiempo.
        var optional = new Dictionary<string, List<Mission>>();
        if (sanctuaryModel)
            foreach (var kv in pools) { optional[kv.Key] = kv.Value.Where(m => m.optional).ToList(); kv.Value.RemoveAll(m => m.optional); }
        var used = workers.ToDictionary(w => w, w => 0f);
        _place.Clear();
        _staff = DayOrder(day);   // (2026-11-08) el orden del personal de HOY: con el reparto por santuarios, una permutación del día
        if (sanctuaryModel) SplitDay(plan, pools);   // cada uno, a su sitio del día
        List<Worker> order = _staff;
        if (initiationModel) order = PlanInitiations(plan, pools, used);   // PRIMERO las iniciaciones; lo aprendido ya se puede hacer hoy, a partir de esa hora
        AssignOwned(plan, pools, used);    // las de puesto, primero a su dueño (si no, la rotación le llenaba la jornada); sin él, por rotación
        AssignGroups(plan, pools, used);   // primero las de grupo: todas a la misma hora, cada una a una persona distinta
        ProfessionRound(plan, pools, used);   // (2026-11-04) luego, a cada profesional una misión de cada área de su profesión
        RunRounds(plan, pools, order, used);

        if (sanctuaryModel)
        {
            CloneForRest(plan, pools, used);   // lo que no cabe en los humanos: clones (primero las áreas sin nadie), y nada a mañana
            // LAS OPCIONALES, a los humanos que aún tienen tiempo en sus 5 h (la que menos lleva primero); nunca a un clon. Las que no caben, hoy no se hacen.
            RunRounds(plan, optional, _staff.Where(w => !w.clone).ToList(), used);
            foreach (var kv in optional) { plan.skipped.AddRange(kv.Value); kv.Value.Clear(); }
        }
        // Obligatorias sin cubrir: la Magnate se divide (clones de 5 h) hasta cubrirlas todas.
        int cloneN = 0;
        while (!sanctuaryModel && pools.Values.Any(p => p.Any(m => m.mandatory)))
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
        foreach (var kv in _place) plan.place[kv.Key] = kv.Value;
        if (sanctuaryModel)
            foreach (var kv in plan.hoursAt)
            {
                var mine = workers.Where(w => _place.TryGetValue(w, out var p) && p.Equals(kv.Key)).ToList();
                plan.share[kv.Key] = mine.Count == 0 ? 0f : System.Math.Min(WorkHours, kv.Value / mine.Count);
            }

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


    List<Worker> _staff = new List<Worker>();

    // EL REPARTO POR SITIOS (Berón 2026-11-08). Las horas de cada sitio (santuario y nivel) se cubren primero empezando por el que menos gente
    // alcanza (con la gente que menos sitios alcanza, para no gastar en la tierra a quien también puede ir al Aéreo); cada sitio pide la suma de lo que pide cada
    // área suya entera (⌈sus horas / 5⌉, al menos 1). Todo sale de los datos de hoy (personal y lista de tareas): nada está fijado a mano.
    // Los que sobran (segundo reparto) van, uno a uno, al sitio con más horas por persona de los que alcanzan. Kushal, siempre en el santuario 1
    // (es el jugador: está en esta escena). Quien no alcanza ningún sitio con tareas, al santuario 1 nivel 1.
    void SplitDay(DayPlan plan, Dictionary<string, List<Mission>> pools)
    {
        // LAS HORAS DE CADA ÁREA (Berón 2026-11-08): las de su propia lista de hoy (catálogo + obras, cocina, sitio…), contando el área ENTERA
        // (el hospital y el yoga son un área, tengan los pisos que tengan: AreaGroup). Cada área pide ⌈horas / 5⌉ personas, y al menos 1.
        foreach (var kv in pools)
        {
            float h = kv.Value.Sum(m => m.hours); if (h <= 0f) continue;
            string g = AreaGroup(kv.Key);
            plan.hoursByArea[g] = (plan.hoursByArea.TryGetValue(g, out float ha) ? ha : 0f) + h;
            var at = placeOf(kv.Key);
            plan.hoursAt[at] = (plan.hoursAt.TryGetValue(at, out float o) ? o : 0f) + h;
        }
        var needAt = plan.hoursAt.Keys.ToDictionary(p => p, p => 0);
        foreach (var kv in plan.hoursByArea)
        {
            int q = System.Math.Max(1, (int)System.Math.Ceiling(kv.Value / WorkHours - 1e-4f));
            plan.quota[kv.Key] = q;
            needAt[placeOf(kv.Key)] += q;
        }
        var count = plan.hoursAt.Keys.ToDictionary(p => p, p => 0);
        void Put(Worker w, SanctuaryPlacement.Home p) { _place[w] = p; if (count.ContainsKey(p)) count[p]++; }
        Worker me = _staff.FirstOrDefault(w => w.name == "Player");
        if (me != null) Put(me, SanctuaryPlacement.LandFirst);
        int Reach(SanctuaryPlacement.Home p) => _staff.Count(w => SanctuaryPlacement.CanAccess(w.home, p));
        foreach (var p in plan.hoursAt.Keys.OrderBy(Reach).ThenByDescending(p => p.Position).ToList())
        {
            int need = needAt[p];
            foreach (Worker w in _staff.Where(w => !_place.ContainsKey(w) && SanctuaryPlacement.CanAccess(w.home, p)).OrderBy(w => w.home).ToList())   // (estable: a igualdad, el orden del día)
            {
                if (count[p] >= need) break;
                Put(w, p);
            }
        }
        foreach (Worker w in _staff)
        {
            if (_place.ContainsKey(w)) continue;
            SanctuaryPlacement.Home best = SanctuaryPlacement.LandFirst; float load = -1f;
            foreach (var p in plan.hoursAt.Keys)
            {
                if (!SanctuaryPlacement.CanAccess(w.home, p)) continue;
                float l = plan.hoursAt[p] / (count[p] + 1);   // las horas que le tocarían a cada uno si va
                if (l > load + 1e-4f) { load = l; best = p; }
            }
            Put(w, best);
            plan.secondSplit.Add(w);
        }
    }

    // NADA PASA A MAÑANA (Berón 2026-11-08, corregido): lo que no cupo en los humanos lo hacen clones de la Magnate, de 5 h, obligatorio o no.
    //  1. Primero, un clon por cada área ENTERA (AreaGroup) con tareas y sin ningún humano hoy (nadie de su sitio, o nadie pudo iniciarse en ella).
    //  2. Después, clones para lo que quede, sitio a sitio (cada uno con las tareas de un sitio), hasta acabar. Un área sin tareas no pide a nadie.
    // Acotado: como mucho 200 clones; lo que ni un clon puede hacer (una tarea más larga que cualquier franja) queda solo en el registro (impossible).
    /// <summary>(2026-11-09) POR QUÉ ningún humano hace <paramref name="m"/>: nadie de su sitio, nadie sabe hacerla, todos al tope de 5 h, o los que
    /// tienen horas no tienen un hueco de su duración en sus franjas (se dice quiénes y cuánto les queda).</summary>
    string Why(DayPlan plan, Mission m)
    {
        var at = placeOf(m.area);
        var humans = workers.Where(w => !w.clone && _place.TryGetValue(w, out var p) && p.Equals(at)).ToList();
        if (humans.Count == 0) return $"nadie vive en {at} o llega a él ({m.id})";
        var can = humans.Where(w => Qualified(w, m) && !(IsNewcomer(w, plan.day) && m.lead)).ToList();
        if (can.Count == 0) return $"nadie de {at} sabe hacer {m.id} (sin iniciar)";
        float Worked(Worker w) => plan.work.Where(a => a.worker == w).Sum(a => a.mission.hours);
        var free = can.Where(w => Worked(w) < WorkHours - 1e-3f).ToList();
        if (free.Count == 0) return $"todos los de {at} que saben {m.id} están al tope de {WorkHours:0} h";
        return $"sin hueco de {m.hours:0.#} h para {m.id} en las franjas de quien tiene horas ({string.Join(", ", free.Take(4).Select(w => $"{w.name} {WorkHours - Worked(w):0.#} h"))}{(free.Count > 4 ? $" y {free.Count - 4} más" : "")})";
    }

    void CloneForRest(DayPlan plan, Dictionary<string, List<Mission>> pools, Dictionary<Worker, float> used)
    {
        Worker NewClone() { var c = new Worker($"Magnate (clon {plan.clones.Count + 1})", clone: true); used[c] = 0f; return c; }
        foreach (var grp in plan.areaOrder.Where(a => pools.TryGetValue(a, out var p) && p.Count > 0).GroupBy(AreaGroup).ToList())
        {
            var members = new HashSet<string>(grp);
            if (plan.work.Any(a => !a.lesson && !a.worker.clone && members.Contains(a.mission.area))) continue;   // ya tiene a alguien
            string why = Why(plan, pools[grp.First()].First());
            Worker c = NewClone();
            RunRounds(plan, members.ToDictionary(a => a, a => pools[a]), new List<Worker> { c }, used);
            if (used[c] <= 0f) { used.Remove(c); continue; }
            plan.clones.Add(c); plan.cloneAreas.Add(grp.Key); plan.emptyAreaClones++;
            plan.cloneReasons.Add($"clon para «{grp.Key}»: motivo {why}");
        }
        for (int k = 0; k < 200; k++)
        {
            string area = plan.areaOrder.FirstOrDefault(a => pools.TryGetValue(a, out var p) && p.Count > 0);
            if (area == null) return;
            var at = placeOf(area);
            var mine = pools.Where(kv => kv.Value.Count > 0 && placeOf(kv.Key).Equals(at)).ToDictionary(kv => kv.Key, kv => kv.Value);
            string why2 = Why(plan, pools[area].First());
            Worker c = NewClone();
            RunRounds(plan, mine, new List<Worker> { c }, used);
            if (used[c] <= 0f) { used.Remove(c); break; }
            plan.clones.Add(c); plan.cloneAreas.Add(area);
            plan.cloneReasons.Add($"clon para «{area}»: motivo {why2}");
        }
        foreach (var kv in pools) { plan.impossible.AddRange(kv.Value); kv.Value.Clear(); }   // (solo registro: debe quedar vacío)
    }

    // LAS INICIACIONES (Berón 2026-11-05): la lista del día sale de los personajes y de lo que no saben de lo que hay hoy. Se reparte antes que nada y de quien menos sabe a quien
    // más, llenando sus horas de iniciación desde la primera de la jornada; así queda claro si serán todas, la mitad o las que le falten. Por clases: quien puede, entra en una que ya
    // exista. Devuelve el orden del personal para repartir después las del santuario. Saber iniciar es una tarea más de la lista.
    List<Worker> PlanInitiations(DayPlan plan, Dictionary<string, List<Mission>> pools, Dictionary<Worker, float> used)
    {
        _busy.Clear();
        var options = pools.Values.SelectMany(p => p)
            .Where(m => string.IsNullOrEmpty(m.owner) && !FruitGardener.IsRound(m.id))
            .GroupBy(m => MissionRequirements.KnowledgeKey(m.id)).Select(g => g.First()).ToList();
        var skill = new Mission("Aula", InstructSkill, "Saber iniciar a otros", LessonHours);
        var skills = workers.ToDictionary(w => w, w => options.Count(m => Learned(w, m)));
        options.Add(skill);
        var order = _staff.OrderBy(w => skills[w]).ToList();   // (estable: a igualdad, el orden del personal del día)
        // (ronda 52) Con el reparto por santuarios, solo se inicia en lo que tiene riesgo (MissionRequirements.NeedsLesson: J31, lo demás se aprende haciéndolo) y en
        // saber iniciar. Antes cada uno pasaba 2 h al día aprendiendo a hacer leche o a barrer, y esas horas las cubrían 9 clones en la cocina.
        bool Wants(Worker w, Mission m) => !Learned(w, m) && (m != skill || skills[w] >= MinSkillsToInstruct) && (!sanctuaryModel || m == skill || MissionRequirements.NeedsLesson(m.id));
        var scores = new Dictionary<Mission, int>();   // (se calcula una vez por hora: Practises parte cadenas)
        // (2026-11-08) UN ÁREA QUE HOY NADIE PUEDE HACER (todas sus tareas piden saber y nadie de su sitio sabe ninguna) va PRIMERO: alguien se inicia
        // en ella y la hace después; la Magnate la cubre solo si nadie pudo iniciarse ese día. Regla de oro: todos aprenden todo.
        var emptyAreas = new HashSet<string>();
        void FindEmpty()
        {
            emptyAreas.Clear();
            foreach (var kv in pools)
                if (kv.Value.Count > 0 && !kv.Value.Any(m => workers.Any(w => !w.clone && Qualified(w, m) && Allowed(w, m)))) emptyAreas.Add(AreaGroup(kv.Key));
        }
        int Compute(Mission m) => workers.Count(w => !w.clone && Wants(w, m)) + (m.mandatory ? 100 : 0) + (MissionRequirements.NeedsLesson(m.id) ? 200 : 0) + (workers.Any(w => Learned(w, m)) || m == skill ? 0 : 400)
            + (m != skill && emptyAreas.Contains(AreaGroup(m.area)) ? 800 : 0);
        int Score(Mission m) => scores[m];
        var done = workers.ToDictionary(w => w, w => 0f);
        var waiting = new HashSet<Worker>();
        for (int hour = 0; hour < WorkHours; hour++)
        {
            float clock = -1f;
            Place(hour, LessonHours, out clock);
            FindEmpty();
            foreach (Mission m in options) scores[m] = Compute(m);
            var classes = new List<Lesson>();
            bool OpenHere(Mission m) => emptyAreas.Contains(AreaGroup(m.area)) && !classes.Any(x => AreaGroup(x.real.area) == AreaGroup(m.area));
            foreach (Worker w in order)
            {
                if (w.clone || used[w] > hour + 1e-3f || Busy(w, hour) || done[w] + LessonHours > InitiationCap(w, plan.day) + 1e-3f) continue;
                var mine = options.Where(m => Wants(w, m) && (m == skill || Allowed(w, m)) && !FarAfterLunch(w, m.area, clock)).ToList();
                if (mine.Count == 0) continue;
                // (un área vacía sin clase todavía esta hora abre la suya antes de que nadie se sume a otra: cada área vacía, su clase)
                // (ronda 33: Irosene y Lizmibet no se iniciaban en la planta nuclear porque las 6 clases de la hora ya las llenaban los de la tierra) Un área vacía
                // abre su clase aunque ya haya 6: la Magnate se divide para darla, y así nadie la cubre con un clon por no saber.
                Mission empty = mine.Where(OpenHere).OrderByDescending(Score).FirstOrDefault();
                Lesson c = empty != null ? null : classes.Where(x => x.learners.Count < LessonGroupSize && mine.Contains(x.real)).OrderByDescending(x => Score(x.real)).FirstOrDefault();
                if (c == null)
                {
                    if (classes.Count >= MaxLessonGroups && empty == null) { waiting.Add(w); continue; }
                    Mission best = empty ?? mine.OrderByDescending(Score).First();
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
            // QUIÉN INICIA: quien sepa y esté libre a ESA hora (no hace falta que su jornada vaya por ahí: la hora queda reservada y las tareas se colocan alrededor,
            // con PlaceFor), el que menos clases lleve hoy; ella, si no hay nadie. Kushal no: el jugador no tiene cómo dar la clase, y sin él no habría nadie delante.
            int magnates = 0;
            foreach (Lesson c in classes)
            {
                Worker t = !instructorsTeach ? null : workers.Where(w => w.name != "Player" && used[w] <= hour + 1e-3f && !Busy(w, hour) && CanInitiate(w, c.real) && Allowed(w, c.real) && !c.learners.Contains(w)).OrderBy(w => _busy.TryGetValue(w, out var b) ? b.Count : 0).FirstOrDefault();
                if (t != null)
                {
                    c.instructor = t; c.teacher = t.name;
                    if (!_busy.TryGetValue(t, out var mine)) _busy[t] = mine = new HashSet<int>();
                    mine.Add(hour);
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
                if (w == null || !Allowed(w, m) || PlaceFor(w, used[w], m.hours, out _) < 0f) continue;
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
        foreach (Worker w in _staff)   // (el orden del personal del día)
        {
            if (w.clone) continue;
            foreach (string area in Professions.AreasOf(w.name).Distinct())
            {
                if (!pools.TryGetValue(area, out var pool) || pool.Count == 0) continue;
                var mine = pool.Where(m => Professions.Practises(w.name, area, m.id) && (!IsNewcomer(w, plan.day) || !m.lead) && string.IsNullOrEmpty(m.group) && Allowed(w, m)).ToList();   // (solo las de su profesión; las de grupo van en AssignGroups)
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
                float span = roles.Max(m => m.hours);
                pro = useProfessions ? workers.FirstOrDefault(w => !w.clone && used[w] <= f + 1e-3f && PlaceFor(w, f, span, out _) == f && !IsNewcomer(w, plan.day) && Allowed(w, roles[0]) && Professions.Practises(w.name, roles[0].area, roles[0].id)) : null;
                if (pro != null) crew.Add(pro);   // (si hay papel que dirige, lo dirige; en una obra, entra el jefe de obra nueva)
                int newcomers = 0;
                for (int i = 0; crew.Count < roles.Count && i < workers.Count; i++)
                {
                    Worker w = _staff[(offset + i) % _staff.Count];
                    if (used[w] > f + 1e-3f || PlaceFor(w, f, span, out _) != f || crew.Contains(w) || !Allowed(w, roles[0])) continue;
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
    // Vueltas por las áreas: de cada área, una misión a la primera persona a la que le quepa (el reparto puro).
    void RunRounds(DayPlan plan, Dictionary<string, List<Mission>> pools, List<Worker> staff, Dictionary<Worker, float> used)
    {
        if (sanctuaryModel) { SharedRounds(plan, pools, staff, used); return; }
        bool progress = true;
        while (progress)
        {
            progress = false;
            foreach (string area in plan.areaOrder)
            {
                if (!pools.TryGetValue(area, out var pool) || pool.Count == 0) continue;
                foreach (Worker w in staff)
                {
                    Mission pick = Choose(w, pool.Where(m => (!IsNewcomer(w, plan.day) || !m.lead) && Qualified(w, m) && Allowed(w, m)).ToList(), used[w]);
                    if (pick == null) continue;
                    pool.Remove(pick);
                    Assign(plan, w, pick, used);
                    progress = true;
                    break;
                }
            }
        }
    }

    // PARTES IGUALES (2026-11-08) Y SIN HUECOS (2026-11-09, ronda 31 en vivo). Cada vez, la persona que MENOS horas lleva hoy (a igualdad, el orden del
    // día) coge, de TODAS las áreas de su sitio, la tarea que mejor encaja: la que empieza justo donde acaba lo suyo (sin dejar un hueco que ya nadie
    // usaría, porque la jornada se llena hacia delante), y a igualdad la más larga (las franjas son de 2, 2 y 1 h), la que menos ha hecho, la que hace
    // más días que no hace y el orden de áreas del día. Antes se iba área por área: las de 1 h ocupaban las franjas de 2 h, las de 2 h (rondas de
    // frutales, servicio, guardias, obras) ya no cabían en nadie y salían 21 clones con humanos ociosos («Faltan misiones: 14 h»).
    void SharedRounds(DayPlan plan, Dictionary<string, List<Mission>> pools, List<Worker> staff, Dictionary<Worker, float> used)
    {
        // (ronda 35) EN LOS HUECOS DE VERDAD: cada uno tiene sus tramos libres (sus franjas menos lo que ya hace: clases, grupos, puestos y las horas en que
        // inicia). Antes se colocaba siempre detrás de lo último (used), y los huecos que dejaban un grupo a las 12 o una clase se perdían: humanos con 1–2 h
        // libres y clones «sin hueco de 0,5 h».
        var worked = staff.ToDictionary(w => w, w => 0f);
        foreach (Assignment a in plan.work) if (worked.ContainsKey(a.worker)) worked[a.worker] += a.mission.hours;
        var free = staff.ToDictionary(w => w, w => FreeIntervals(plan, w));
        var areaIdx = new Dictionary<string, int>();
        for (int i = 0; i < plan.areaOrder.Count; i++) areaIdx[plan.areaOrder[i]] = i;
        var full = new HashSet<Worker>();
        for (int guard = 0; guard < 5000; guard++)
        {
            Worker pickW = null; Mission pickM = null; string pickArea = null; float pickH = 0f, pickAt = 0f;
            foreach (Worker w in staff.Where(x => !full.Contains(x)).OrderBy(x => worked[x]))
            {
                Mission best = null; string bestArea = null; float bestH = 0f, bestAt = 0f; bool bestExact = false;
                foreach (var kv in pools)
                    foreach (Mission m in kv.Value)
                    {
                        if ((IsNewcomer(w, plan.day) && m.lead) || !Qualified(w, m) || !Allowed(w, m)) continue;
                        if (w.name == "Player" && FruitGardener.IsRound(m.id)) continue;   // (la ronda de frutales la lleva FruitGardener, que no mueve al jugador)
                        // El mejor tramo para ella: uno que llene justa (entera o, si es divisible, un trozo); si no, el más largo que quepa.
                        float h = -1f, at = -1f; bool exact = false;
                        foreach (var t in free[w])
                        {
                            float len = t.to - t.from;
                            float hh = m.hours <= len + 1e-4f ? m.hours : m.divisible ? (float)System.Math.Floor(System.Math.Min(len, m.hours - MinPiece) / MinPiece + 1e-4f) * MinPiece : -1f;
                            if (hh < MinPiece - 1e-4f && hh < m.hours - 1e-4f) continue;
                            if (hh <= 0f) continue;
                            if (FarAfterLunch(w, m.area, Clock(t.from))) continue;
                            bool ex = System.Math.Abs(hh - len) < 1e-4f;
                            bool better = h < 0f || (ex && !exact) || (ex == exact && hh > h + 1e-4f);
                            if (better) { h = hh; at = t.from; exact = ex; }
                        }
                        if (h <= 0f) continue;
                        if (best != null)
                        {
                            if (!exact && bestExact) continue;
                            if (exact == bestExact)
                            {
                                if (h < bestH - 1e-4f) continue;
                                if (System.Math.Abs(h - bestH) < 1e-4f)
                                {
                                    // (a igualdad, primero la que no se puede partir: tiene menos sitios donde caber; la divisible llena lo que quede)
                                    if (m.divisible && !best.divisible) continue;
                                    if (m.divisible == best.divisible) {
                                    int cm = w.Count(m.id), cb = w.Count(best.id);
                                    if (cm > cb) continue;
                                    if (cm == cb)
                                    {
                                        int lm = w.Last(m.id), lb = w.Last(best.id);
                                        if (lm > lb) continue;
                                        if (lm == lb && (areaIdx.TryGetValue(kv.Key, out int ia) ? ia : 999) >= (areaIdx.TryGetValue(bestArea, out int ib) ? ib : 999)) continue;
                                    }
                                    }
                                }
                            }
                        }
                        best = m; bestArea = kv.Key; bestH = h; bestAt = at; bestExact = exact;
                    }
                if (best == null) { full.Add(w); continue; }
                pickW = w; pickM = best; pickArea = bestArea; pickH = bestH; pickAt = bestAt;
                break;
            }
            if (pickW == null) return;
            Mission done = pickM;
            if (pickH < pickM.hours - 1e-4f)
            {
                // UN TROZO: se le da una copia con sus horas; lo que queda sigue en la lista (y es el que aplica el efecto al acabar)
                done = pickM.Copy(); done.hours = pickH; done.piece = true; done.effectPiece = false;
                pickM.hours -= pickH; pickM.piece = true;
            }
            else pools[pickArea].Remove(pickM);
            AssignAt(plan, pickW, done, pickAt, used);
            worked[pickW] += done.hours;
            free[pickW] = FreeIntervals(plan, pickW);
        }
    }

    /// <summary>Los tramos libres de <paramref name="w"/> hoy, en horas de jornada: sus franjas menos lo que ya tiene (tareas, clases, grupos) y las horas en
    /// que inicia a otros. Solo los de media hora o más.</summary>
    List<(float from, float to)> FreeIntervals(DayPlan plan, Worker w)
    {
        var taken = new List<(float from, float to)>();
        foreach (Assignment a in plan.work) if (a.worker == w) { float s = Jornada(a.start); if (s >= 0f) taken.Add((s, s + a.mission.hours)); }
        if (_busy.TryGetValue(w, out var b)) foreach (int k in b) taken.Add((k, k + 1f));
        taken.Sort((x, y) => x.from.CompareTo(y.from));
        var l = new List<(float from, float to)>();
        foreach (var blk in initiationModel ? NewBlocks : OldBlocks)
        {
            float cur = blk.from;
            foreach (var t in taken)
            {
                if (t.to <= cur + 1e-4f || t.from >= blk.to - 1e-4f) continue;
                if (t.from > cur + 1e-4f) l.Add((cur, System.Math.Min(t.from, blk.to)));
                cur = System.Math.Max(cur, t.to);
            }
            if (blk.to > cur + 1e-4f) l.Add((cur, blk.to));
        }
        l.RemoveAll(t => t.to - t.from < MinPiece - 1e-4f);
        return l;
    }

    /// <summary>La hora de reloj de unas horas de jornada, y al revés (-1 si cae fuera de las franjas).</summary>
    float Clock(float u) { foreach (var b in initiationModel ? NewBlocks : OldBlocks) if (u >= b.from - 1e-4f && u < b.to - 1e-4f) return b.clock + u - b.from; return -1f; }
    float Jornada(float clock) { foreach (var b in initiationModel ? NewBlocks : OldBlocks) if (clock >= b.clock - 1e-4f && clock < b.clock + (b.to - b.from) - 1e-4f) return b.from + clock - b.clock; return -1f; }

    /// <summary>Le da <paramref name="m"/> a <paramref name="w"/> a las <paramref name="u"/> horas de jornada (en un tramo suyo libre).</summary>
    void AssignAt(DayPlan plan, Worker w, Mission m, float u, Dictionary<Worker, float> used)
    {
        var a = new Assignment { worker = w, mission = m, start = Clock(u), firstTime = initiationModel ? !Learned(w, m) : !w.Knows(m.id) };
        plan.work.Add(a);
        used[w] = System.Math.Max(used[w], u + m.hours);
        w.count[m.id] = w.Count(m.id) + 1;
        w.lastDay[m.id] = plan.day;
    }

    /// <summary>Dónde acaba la franja en la que empieza una tarea a las <paramref name="u"/> horas de jornada.</summary>
    float BlockEnd(float u)
    {
        foreach (var b in initiationModel ? NewBlocks : OldBlocks) if (u < b.to - 1e-4f) return b.to;
        return WorkHours;
    }

    // Nunca hecha → la menos hecha → la que hace más días que no hace (y que quepa en lo que le queda de jornada).
    /// <summary>A Kushal no se le da a las 12 una misión de un sitio lejano: sale de comer a esa hora y el viaje (140 s de una hora de 150) se la come entera. Regla J31.</summary>
    static bool FarAfterLunch(Worker w, string area, float clock) => w.name == "Player" && System.Math.Abs(clock - (StartHour + FirstBlock + LunchHours)) < 0.01f && (NuclearSite.IsSiteArea(area) || AereoRule.IsArea(area) || area == EnergyRule.Area || (FarFromLunch != null && FarFromLunch(area)));
    /// <summary>(2026-11-11) ¿Queda lejos del restaurante? Lo pone la jornada en juego (por el viaje estimado): de la comida en Servicios a la
    /// sala de yoga de Entrenamiento son 97 m de tronco, una alfombra y 172 m más, y Kushal llegaba a mitad de la misión de las 12 (47 %).</summary>
    public static System.Func<string, bool> FarFromLunch;

    Mission Choose(Worker w, List<Mission> pool, float used)
    {
        Mission best = null;
        foreach (Mission m in pool)
        {
            if (PlaceFor(w, used, m.hours, out float at) < 0f || FarAfterLunch(w, m.area, at)) continue;
            if (w.name == "Player" && FruitGardener.IsRound(m.id)) continue;   // (2026-11-05) la ronda de frutales la hace la conducta FruitGardener, que no mueve al jugador: con Kushal, ese día se quedaba sin hacer
            if (best == null) { best = m; continue; }
            int cm = w.Count(m.id), cb = w.Count(best.id);
            // (2026-11-08) Con el reparto por santuarios, a igualdad, la MÁS LARGA primero: las franjas son de 2, 2 y 1 h, y con la de 1 h delante
            // no cabían dos de 2 h (salía un clon con horas de sobra). Una persona encadena así todas las tareas de su área si le caben.
            int lm = w.Last(m.id), lb = w.Last(best.id);
            if (cm < cb || (cm == cb && lm < lb) || (sanctuaryModel && cm == cb && lm == lb && m.hours > best.hours + 1e-4f)) best = m;
        }
        return best;
    }

    void Assign(DayPlan plan, Worker w, Mission m, Dictionary<Worker, float> used)
    {
        // Lo que no cabe en lo que queda de su franja empieza en la siguiente (no se parte en la comida ni en el entrenamiento).
        float u = PlaceFor(w, used[w], m.hours, out float start);
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

    // Las horas en que alguien ya da una clase: sus tareas se colocan alrededor.
    readonly Dictionary<Worker, HashSet<int>> _busy = new Dictionary<Worker, HashSet<int>>();
    bool Busy(Worker w, int hour) => _busy.TryGetValue(w, out var b) && b.Contains(hour);

    /// <summary><see cref="Place"/> para una persona: salta las horas en que da una clase.</summary>
    public float PlaceFor(Worker w, float used, float h, out float clock)
    {
        float from = used;
        for (int i = 0; i < 6; i++)
        {
            float u = Place(from, h, out clock);
            if (u < 0f) return -1f;
            int hit = -1;
            for (int k = (int)u; k < u + h - 1e-3f; k++) if (Busy(w, k)) hit = k;
            if (hit < 0) return u;
            from = hit + 1f;
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
        if (p.clones.Count > 0 && p.cloneAreas.Count > 0) l.Add($"Faltan personas: la Magnate se dividió en {p.clones.Count} clon(es) ({p.emptyAreaClones} para áreas sin nadie, {p.clones.Count - p.emptyAreaClones} para lo que no cabía en los humanos): {string.Join(", ", p.cloneAreas.Distinct())}.");
        else if (p.clones.Count > 0) l.Add($"Faltan personas: la Magnate se dividió en {p.clones.Count} clon(es) para cubrir las misiones obligatorias.");
        if (p.backlog.Count > 0) l.Add($"Faltan personas: {p.backlog.Count} misión(es) ({p.backlog.Sum(m => m.hours):0.#} h) pasan a mañana.");
        float idle = workersCount * WorkHours - p.work.Where(a => !a.worker.clone).Sum(a => a.mission.hours);
        if (idle > 0.5f && p.backlog.Count == 0) l.Add($"Faltan misiones: {idle:0.#} h de trabajo sin asignar; conviene crear misiones en las áreas con menos carga.");
        var perArea = p.work.GroupBy(a => a.mission.area).ToDictionary(g => g.Key, g => g.Sum(a => a.mission.hours));
        foreach (string a in p.areaOrder) if (!perArea.ContainsKey(a)) l.Add($"El área «{a}» no tiene misiones hoy.");
        if (p.blocked.Count > 0) l.Add($"Sin sus requisitos, hoy no se hacen {p.blocked.Count}: {string.Join("; ", p.blocked.Take(4).Select(b => $"{b.mission.id} ({b.why})"))}.");
        if (p.unqualified.Count > 0) l.Add($"Nadie sabe hacer aún {p.unqualified.Count} misión(es): {string.Join(", ", p.unqualified.Take(6).Select(m => m.id))}.");
        if (p.waitingLesson > 0) l.Add($"{p.waitingLesson} persona(s) sin sitio en las iniciaciones de hoy: trabajan y aprenden otro día.");
        if (l.Count == 0) l.Add("Equilibrado: todas las misiones cubiertas por el personal, sin clones ni pendientes.");
        foreach (var kv in p.hoursAt.OrderBy(k => k.Key.Position))
        {
            int n = p.place.Count(x => x.Value.Equals(kv.Key));
            l.Add($"{kv.Key}: {kv.Value:0.#} h de tareas para {n} persona(s){(n > 0 ? $" ({(p.share.TryGetValue(kv.Key, out float sh) ? sh : 0f):0.#} h cada una; el resto, clones)" : " (todo, clones)")}.");
        }
        if (p.secondSplit.Count > 0) l.Add($"Segundo reparto: {p.secondSplit.Count} persona(s) que sobraban, al sitio con más horas por persona.");
        if (p.impossible.Count > 0) l.Add($"IMPOSIBLE hoy (ni un clon puede): {string.Join(", ", p.impossible.Take(6).Select(m => $"{m.id} {m.hours:0.#} h"))}.");
        if (p.skipped.Count > 0) l.Add($"Opcionales que hoy no se hacen (nadie tenía tiempo; no pasan a mañana): {string.Join(", ", p.skipped.Take(8).Select(m => m.id))}.");
        return l;
    }
}
