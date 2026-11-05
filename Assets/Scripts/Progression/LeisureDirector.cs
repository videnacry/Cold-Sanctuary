using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// EL HORARIO DE ACTIVIDADES LIBRE EN EL JUEGO (docs/horario-libre.md). Una vez por semana reparte con <see cref="LeisureRoster"/> las
/// horas libres de cada personaje de cada ESCENA (el santuario —el mesocosmos— y cada microcosmos cargado) y en esas horas lleva a
/// cada uno a su actividad: anda (en alfombra si es otro piso) hasta el sitio, la hace (postura o gesto) y lo pone en su rótulo.
///  · SANTUARIO: el personal de la jornada (<see cref="WorkdayDirector.staff"/>, sin Kushal: el jugador hace lo que quiere) en las
///    áreas abiertas del santuario (gimnasio, sala de yoga, huerto, cría, estudio textil, restaurante, arboleda, su casa…).
///  · MICROCOSMOS: los insectos-humanos con jornada (<see cref="MicroJornada"/>) en los edificios de su pueblo (dojo, sala de yoga,
///    templo, escuela, mercado, restaurante, huerto) y en su casa. Los edificios con ETIQUETA de actividad (ocio, descanso,
///    recuperación; <see cref="LeisureTags"/>) entran también, con su capacidad.
/// Las necesidades del cuerpo (comer, el baño, dormir), las coreografías, la Magnate y el servicio del restaurante van antes:
/// mientras, el horario espera. Las pruebas no lo ven salvo que lo pidan (<see cref="ForceRun"/>), como la jornada.
/// Consola: <c>horario</c> (panel <see cref="HorarioPanel"/>).
/// </summary>
public class LeisureDirector : MonoBehaviour
{
    public static LeisureDirector Instance { get; private set; }
    /// <summary>Las pruebas lo encienden a propósito (con el orquestador en marcha el horario no mueve a nadie).</summary>
    public static bool ForceRun;
    public static bool Active => Instance != null && Instance.isActiveAndEnabled && (!ColdSanctuary.TestRunner.Running || ForceRun);

    public float walkSpeed = 2.6f, microSpeed = 2.4f;
    [Tooltip("Segundos sin llegar andando a su sitio en el microcosmos tras los que se le pone allí (como MicroJornada).")]
    public float microFallback = 60f;

    static TimeController TC => TimeController.timeController;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("HorarioLibre_AUTO");
        DontDestroyOnLoad(go);
        go.AddComponent<LeisureDirector>();
        go.AddComponent<HorarioPanel>();
    }

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }

    // ── Sitios ───────────────────────────────────────────────────────────────────────────────────

    /// <summary>Un sitio donde se hace una actividad: un área del santuario, la arboleda, la casa de cada uno o los edificios de
    /// una plantilla en el microcosmos (con sus puestos).</summary>
    public class Venue
    {
        public string title;
        public Transform anchor;                 // área del santuario / arboleda
        public bool home;                        // la casa de cada uno (su dormitorio o su casa del pueblo)
        public readonly List<AreaFunctionSite> sites = new List<AreaFunctionSite>();   // microcosmos
        public AreaFunctions.AreaFunction fn;    // la función del registro que se hace (microcosmos)
        public AreaFunctions.Kind kind = AreaFunctions.Kind.Rest;
        public int index;                        // para repartir los sitios de varias actividades en la misma área
        public bool deck;                        // la plataforma central de la arboleda (puntos sobre ella)
        public readonly List<YogaHall> halls = new List<YogaHall>();   // (regla J16) la sala de yoga en todos los pisos de su estilo
        public int capacity;
    }

    public class Group
    {
        public Scene scene; public string name; public bool micro;
        public readonly List<LeisureRoster.Activity> catalog = new List<LeisureRoster.Activity>();
        public readonly Dictionary<LeisureRoster.Activity, Venue> venues = new Dictionary<LeisureRoster.Activity, Venue>();
        public readonly List<GameObject> bodies = new List<GameObject>();
        public readonly Dictionary<GameObject, LeisureRoster.Person> personOf = new Dictionary<GameObject, LeisureRoster.Person>();
        public LeisureRoster.WeekPlan plan;
        public readonly List<LeisureRoster.WeekPlan> history = new List<LeisureRoster.WeekPlan>();
        public string signature = "";
        public int spread, spreadPerPerson; public string worst = "";
        public GameObject BodyOf(LeisureRoster.Person p) { foreach (var kv in personOf) if (kv.Value == p) return kv.Key; return null; }
    }

    public readonly List<Group> groups = new List<Group>();
    float _scanAt;

    /// <summary>El grupo (escena) de <paramref name="g"/>.</summary>
    public Group GroupOf(GameObject g) { foreach (Group gr in groups) if (gr.personOf.ContainsKey(g)) return gr; return null; }
    /// <summary>El grupo de una escena (el santuario: el de la escena activa con jornada).</summary>
    public Group GroupOf(Scene s) => groups.FirstOrDefault(g => g.scene == s);

    // ── Participantes ────────────────────────────────────────────────────────────────────────────

    static readonly string[] DefaultStaff = { "Linduarda", "Lizmibet", "Goluis", "Panterilia", "Gohageneis", "Irosene", "JefeConstruccion", "Andrea",
                                              "Nesvitor", "Amykhuz", "Karen", "Alicita", "Ducristin", "Josedrilo", "Maite", "Shalva", "Adaguchi", "Gallarle", "Julian", "Fausto", "David", "Adonis", "Yulo", "Lale", "Ivelyna", "Luciana", "Malena", "Consol", "Fernando" };

    /// <summary>Vuelve a mirar quién hay en cada escena (cada 2 s, o ya si <paramref name="now"/>) y reparte la semana si cambió
    /// la gente o la semana.</summary>
    public void Scan(bool now = false)
    {
        if (!now && Time.unscaledTime < _scanAt) return;
        _scanAt = Time.unscaledTime + 2f;
        groups.RemoveAll(g => !g.scene.IsValid() || !g.scene.isLoaded);
        int week = LeisureRoster.WeekOf(TC.Day);
        var found = new Dictionary<Scene, (List<GameObject> bodies, bool micro)>();
        // SANTUARIO: el personal de la jornada (sin el jugador).
        string[] staff = WorkdayDirector.Instance != null ? WorkdayDirector.Instance.staff : DefaultStaff;
        foreach (string n in staff)
        {
            if (n == "Player") continue;
            GameObject g = WorkdayDirector.Body(n);
            if (g == null || !g.activeInHierarchy || g.CompareTag("Player") || !g.TryGetComponent(out Anima a) || a.death) continue;
            if (!found.TryGetValue(g.scene, out var l)) found[g.scene] = l = (new List<GameObject>(), false);
            l.bodies.Add(g);
        }
        // MICROCOSMOS: los insectos-humanos con jornada (no los niños, que van con su madre).
        foreach (MicroJornada mj in FindObjectsByType<MicroJornada>(FindObjectsSortMode.None))
        {
            if (mj == null || mj.follow != null || !mj.isActiveAndEnabled) continue;
            if (mj.TryGetComponent(out Anima a) && a.death) continue;
            if (!found.TryGetValue(mj.gameObject.scene, out var l)) found[mj.gameObject.scene] = l = (new List<GameObject>(), true);
            if (!l.micro && l.bodies.Count > 0) continue;   // (una escena del santuario con figuras de jornada micro: manda el personal)
            l.bodies.Add(mj.gameObject);
        }
        foreach (var kv in found)
        {
            List<GameObject> bodies = kv.Value.micro ? kv.Value.bodies.OrderBy(b => b.name, System.StringComparer.Ordinal).ToList() : kv.Value.bodies;
            string sig = string.Join("|", bodies.Select(b => b.GetEntityId().ToString()));
            Group gr = GroupOf(kv.Key);
            if (gr == null) { gr = new Group { scene = kv.Key, name = kv.Key.name, micro = kv.Value.micro }; groups.Add(gr); }
            if (gr.plan != null && gr.plan.week == week && gr.signature == sig) continue;
            gr.signature = sig;
            Build(gr, bodies, week);
        }
    }

    /// <summary>Reparte la semana <paramref name="week"/> del grupo (las cuentas de las semanas anteriores siguen: la rotación continúa).</summary>
    public void Build(Group gr, List<GameObject> bodies, int week)
    {
        ReleaseAll(gr);
        gr.bodies.Clear(); gr.bodies.AddRange(bodies);
        gr.catalog.Clear(); gr.venues.Clear();
        if (gr.micro) MicroCatalog(gr); else SanctuaryCatalog(gr);
        var roster = new LeisureRoster();
        roster.activities.AddRange(gr.catalog);
        gr.personOf.Clear();
        foreach (GameObject b in bodies) gr.personOf[b] = roster.Add(Display(b));
        // Las cuentas de las semanas anteriores (por nombre e id de actividad).
        gr.history.RemoveAll(h => h.week >= week);
        foreach (LeisureRoster.WeekPlan h in gr.history)
            foreach (LeisureRoster.Entry e in h.entries)
            {
                LeisureRoster.Person p = roster.Find(e.person.name); if (p == null) continue;
                p.count[e.activity.id] = p.Count(e.activity.id) + 1;
                int g = LeisureRoster.GlobalSlot(e.day, e.slot); if (g > p.Last(e.activity.id)) p.last[e.activity.id] = g;
            }
        gr.plan = roster.Plan(week);
        gr.history.Add(gr.plan);
        while (gr.history.Count > 6) gr.history.RemoveAt(0);
        gr.spread = LeisureRoster.Spread(gr.plan, out gr.spreadPerPerson, out gr.worst);
        Debug.Log($"[Horario] Semana {week} (días {gr.plan.firstDay}–{gr.plan.firstDay + LeisureRoster.DaysPerWeek - 1}) · {(gr.micro ? "microcosmos" : "santuario")} «{gr.name}»: " +
                  $"{bodies.Count} personas, {gr.catalog.Count} actividades ({string.Join(", ", gr.catalog.Select(a => a.title))}), {gr.plan.entries.Count} franjas" +
                  $" · sin llenar {gr.plan.unfilled.Count} · uniformidad Δ={gr.spread} ({gr.worst}) · por persona Δ={gr.spreadPerPerson} · sobre la capacidad: {LeisureRoster.OverCapacity(gr.plan).Count}");
    }

    static string Display(GameObject g) => g.name.Replace("_Post", "");

    // ── Catálogos ────────────────────────────────────────────────────────────────────────────────

    /// <summary>OCIO Y CUIDADO EN EL SANTUARIO: actividad → área (la primera abierta de la lista), plazas y cómo se hace.</summary>
    public static readonly (string id, string title, LeisureRoster.Kind kind, string[] where, int cap, string pose)[] SanctuaryActivities =
    {
        ("pesas", "Pesas y saco en el gimnasio", LeisureRoster.Kind.Ocio, new[] { "Gimnasio" }, 6, "ejercicio"),
        // LOS PISOS DE YOGA (docs/yoga-pisos.md): «Sala de Yoga · <sala>» es una sala de un piso de yoga (YogaHall); cada plaza, una
        // de sus esterillas. Hatha y meditación junto al linga (la tradicional) y Ashtanga autoguiado al estilo Mysore (la occidental).
        ("yoga", "Hatha libre y meditación junto al linga", LeisureRoster.Kind.Recuperacion, new[] { YogaVenue + "Hatha tradicional", "Sala de Yoga" }, 8, "meditar"),
        ("ashtanga", "Ashtanga autoguiado (estilo Mysore)", LeisureRoster.Kind.Recuperacion, new[] { YogaVenue + "Ashtanga occidental", "Sala de Yoga" }, 8, "ejercicio"),
        ("huerto", "Pasear y cuidar el huerto", LeisureRoster.Kind.Ocio, new[] { "Huerto" }, 6, "pie"),
        ("lectura", "Leer en la plataforma de la arboleda", LeisureRoster.Kind.Ocio, new[] { "Residencia" }, 6, "sentado"),
        ("crias", "Jugar con las crías", LeisureRoster.Kind.Cuidado, new[] { "Cría" }, 4, "pie"),
        ("textil", "Manualidades en el estudio textil", LeisureRoster.Kind.Ocio, new[] { "Estudio Textil" }, 4, "sentado"),
        ("tertulia", "Tertulia en el restaurante", LeisureRoster.Kind.Ocio, new[] { "Cocina" }, 8, "sentado"),
        ("asamblea", "Ir a la asamblea de la semana: hablar y escuchar", LeisureRoster.Kind.Ocio, new[] { "Sala de asambleas", "Cocina" }, 8, "sentado"),   // (2026-11-05) docs/piso-asambleas.md
        ("concierto", "Ir al concierto o tocar en el escenario", LeisureRoster.Kind.Ocio, new[] { "Música", "Cocina" }, 8, "sentado"),   // (2026-11-05) docs/piso-musica.md
        ("baile", "Bailar", LeisureRoster.Kind.Ocio, new[] { "Sala de Ballet", "Calistenia", "Gimnasio" }, 6, "baile"),
        ("artes", "Artes marciales con Gohageneis y Luciana",   // (2026-11-04: antes con Goku, que se fue a su microcosmos; 2026-11-05: y Luciana)
          LeisureRoster.Kind.Ocio, new[] { "Dojo (artes marciales)", "Gimnasio" }, 6, "ejercicio"),
        ("descanso", "Descansar en casa", LeisureRoster.Kind.Descanso, new[] { "casa" }, 0, "tumbado"),
    };

    /// <summary>REGLA J16: actividades de ocio que dan cabida a TODOS los miembros a la vez (su capacidad es la de los miembros). Fuera
    /// quedan «Jugar con las crías» (pocas personas por camada, para no agobiarlas) y «Manualidades» (una herramienta por persona: los
    /// telares y la rueca del estudio textil), que van por turnos.</summary>
    public static readonly HashSet<string> AllMembers = new HashSet<string> { "pesas", "yoga", "ashtanga", "huerto", "lectura", "tertulia", "baile", "artes", "concierto", "asamblea" };

    /// <summary>Prefijo de un sitio que es una SALA de los pisos de yoga («Sala de Yoga · Hatha tradicional»…).</summary>
    public const string YogaVenue = "Sala de Yoga · ";

    void SanctuaryCatalog(Group gr)
    {
        int k = 0;
        foreach (var d in SanctuaryActivities)
        {
            Venue v = null;
            foreach (string w in d.where)
            {
                if (w == "casa") { v = new Venue { title = "su casa del árbol", home = true }; break; }
                if (w.StartsWith(YogaVenue))
                {
                    // (regla J16) La sala de ese nombre en TODOS los pisos del estilo: cada plaza, una esterilla de cualquiera de ellas.
                    // Primero la sala nombrada (en sus pisos) y después la otra versión del estilo: 8 esterillas por sala no llegan.
                    YogaHall named = null;
                    foreach (YogaHall h in YogaHall.All) if (h != null && h.gameObject.scene == gr.scene && YogaVenue + h.Label == w && (named == null || h.floorNo < named.floorNo)) named = h;
                    if (named != null)
                    {
                        v = new Venue { title = w, anchor = named.transform };
                        foreach (YogaHall h in YogaHall.Ordered(named.style, named.version)) if (h.gameObject.scene == gr.scene) v.halls.Add(h);
                        break;
                    }
                    continue;
                }
                if (w == "Residencia")
                {
                    GameObject r = GameObject.Find("ArboledaResidencial");
                    if (r != null && r.scene == gr.scene) { v = new Venue { title = "la arboleda residencial", anchor = r.transform, deck = true }; break; }
                    continue;
                }
                SanctuaryArea a = OpenArea(w, gr.scene);
                if (a != null) { v = new Venue { title = a.displayName, anchor = a.transform }; break; }
            }
            if (v == null) continue;   // no hay sitio abierto: la actividad no entra esta semana
            // REGLA J16: los centros de ocio dan cabida a TODOS los miembros a la vez (los que salen de la lista del personal). Las que no
            // (crías y textil) son pequeñas por su naturaleza y van por turnos (docs/centros-para-todos.md).
            int cap = AllMembers.Contains(d.id) ? Mathf.Max(d.cap, AreaCapacity.Members()) : d.cap;
            v.capacity = cap; v.index = k++;
            var act = new LeisureRoster.Activity(d.id, d.title, d.kind, v.title, cap, d.pose);
            gr.catalog.Add(act); gr.venues[act] = v;
        }
        LeisureTags.AddVenues(gr);
    }

    /// <summary>Un área del santuario con ese nombre, abierta (si es una obra, terminada).</summary>
    public static SanctuaryArea OpenArea(string name, Scene scene)
    {
        foreach (SanctuaryArea a in FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None))
        {
            if (a == null || a.displayName != name || a.gameObject.scene != scene) continue;
            if (NuclearSiteRule.InZone(a.transform.position)) continue;   // el sitio nuclear no es para el tiempo libre (docs/sitio-nuclear.md)
            ConstructionProject cp = a.GetComponent<ConstructionProject>();
            if (cp != null && !cp.Finished) continue;
            return a;
        }
        return null;
    }

    /// <summary>OCIO Y CUIDADO EN EL MICROCOSMOS: actividad → plantilla de edificio y función del registro (sus puestos).</summary>
    public static readonly (string id, string title, LeisureRoster.Kind kind, string building, string fn, string pose)[] MicroActivities =
    {
        ("dojo", "Practicar en el dojo", LeisureRoster.Kind.Ocio, "Dojo", "dojo_combate", "ejercicio"),
        ("yoga", "Yoga y meditación", LeisureRoster.Kind.Recuperacion, "Sala de Yoga", "yog_meditar", "meditar"),
        ("templo", "Meditar o rezar en el templo", LeisureRoster.Kind.Recuperacion, "Templo", "tem_rezar", "meditar"),
        ("huerto", "Pasear por el huerto", LeisureRoster.Kind.Ocio, "Huerto", "hue_cosecha", "pie"),
        ("escuela", "Leer y estudiar en la escuela", LeisureRoster.Kind.Ocio, "Escuela", "esc_estudiar", "sentado"),
        ("mercado", "Pasear por el mercado", LeisureRoster.Kind.Ocio, "Mercado", "mer_comprar", "pie"),
        ("tertulia", "Tertulia en el restaurante", LeisureRoster.Kind.Ocio, "Restaurante", "res_comer", "sentado"),
        ("descanso", "Descansar en casa", LeisureRoster.Kind.Descanso, "Casa", "cas_descansar", "sentado"),
        ("cuidar", "Cuidar de la familia en casa", LeisureRoster.Kind.Cuidado, "Casa", "cas_cuidar", "pie"),
    };

    void MicroCatalog(Group gr)
    {
        var sites = new List<AreaFunctionSite>();
        foreach (GameObject root in gr.scene.GetRootGameObjects()) sites.AddRange(root.GetComponentsInChildren<AreaFunctionSite>(true));
        // ETIQUETAS DE ACTIVIDAD (docs/microcosmos-actividades.md): con los edificios etiquetados, el tiempo libre va a los de ocio,
        // recuperación y descanso (no a los de trabajo: la escuela, el mercado o el huerto de un pueblo etiquetado son trabajo).
        bool tagged = sites.Any(s => !string.IsNullOrEmpty(s.activity));
        int k = 0;
        var covered = new HashSet<string>();
        foreach (var d in MicroActivities)
        {
            var v = new Venue { title = d.building, fn = AreaFunctions.Get(d.fn), home = d.building == "Casa" };
            v.kind = v.fn != null ? v.fn.kind : AreaFunctions.Kind.Rest;
            if (!v.home)
            {
                foreach (AreaFunctionSite s in sites)
                    if (s.building == d.building && s.Stations.Length > 0 && (!tagged || (s.activity != AreaFunctionSite.Trabajo && !string.IsNullOrEmpty(s.activity)))) v.sites.Add(s);
                if (v.sites.Count == 0) continue;
                v.capacity = Capacity(v.sites, v.fn);
                v.title = v.sites.Count == 1 && v.sites[0].title.Length > 0 ? v.sites[0].title : d.building;
                covered.Add(d.building);
            }
            else if (!gr.bodies.Any(b => b.TryGetComponent(out MicroJornada mj) && mj.home != null)) continue;
            v.index = k++;
            var act = new LeisureRoster.Activity(d.id, d.title, d.kind, v.title, v.home ? 0 : v.capacity, d.pose);
            gr.catalog.Add(act); gr.venues[act] = v;
        }
        // Los demás edificios etiquetados de ocio o recuperación (parque, plaza, museo, sala de baile, baños, mirador, centro de
        // entrenamiento, fuente…): una actividad por plantilla, con su interacción, su capacidad y la función de «todos».
        foreach (var byB in sites.Where(s => (s.activity == AreaFunctionSite.Ocio || s.activity == AreaFunctionSite.Entrenamiento || s.activity == AreaFunctionSite.Recuperacion) && s.building != "Hospital" && !covered.Contains(s.building) && s.Stations.Length > 0).GroupBy(s => s.building))
        {
            AreaFunctionSite first = byB.First();
            var fs = first.Functions();
            AreaFunctions.AreaFunction fn = fs.FirstOrDefault(f => f.roles.Length == 0 || System.Array.IndexOf(f.roles, "todos") >= 0) ?? fs.FirstOrDefault();
            var v = new Venue { title = byB.Count() == 1 && first.title.Length > 0 ? first.title : byB.Key, fn = fn, kind = fn != null ? fn.kind : AreaFunctions.Kind.Rest, index = k++ };
            v.sites.AddRange(byB);
            v.capacity = Capacity(v.sites, fn);
            string what = !string.IsNullOrEmpty(first.interaction) ? first.interaction : AreaFunctionSite.InteractionOf(byB.Key);
            string title = $"{byB.Key}: {what}";
            var kind = first.activity == AreaFunctionSite.Recuperacion ? LeisureRoster.Kind.Recuperacion : LeisureRoster.Kind.Ocio;
            string pose = fn != null && (fn.kind == AreaFunctions.Kind.Practice || fn.kind == AreaFunctions.Kind.Teach) ? "ejercicio" : fn != null && fn.kind == AreaFunctions.Kind.Meditate ? "meditar" : "pie";
            var act = new LeisureRoster.Activity("tag_" + byB.Key, title, kind, v.title, v.capacity, pose);
            gr.catalog.Add(act); gr.venues[act] = v;
        }
        LeisureTags.AddVenues(gr, sites);
    }

    // Plazas: la capacidad de su etiqueta (si la tiene); si no, sus puestos que sirven para la función.
    static int Capacity(List<AreaFunctionSite> sites, AreaFunctions.AreaFunction fn)
    {
        int tag = sites.Sum(s => Mathf.Max(0, s.capacity));
        if (tag > 0) return tag;
        return Mathf.Max(1, sites.Sum(s => s.Stations.Count(st => fn == null || fn.stations.Length == 0 || System.Array.IndexOf(fn.stations, st.station) >= 0)));
    }

    // ── Conducir ─────────────────────────────────────────────────────────────────────────────────

    /// <summary>Lo que está haciendo alguien en su hora libre (estado de la franja actual).</summary>
    public class Run
    {
        public Group group; public LeisureRoster.Entry entry; public Venue venue;
        public Vector3 to; public AreaFunctionStation st;
        public float since, arrivedAt = -1f, performed, blockedFor, walked, flown, poseAt;
        public bool reachable;   // (microcosmos) su punto tiene camino andando por el NavMesh
        public float speed = 2.6f, allowed = 60f;   // a qué paso va y cuánto tiempo tiene para llegar andando (microcosmos)
        public Vector3 last; public int jumps; public bool fallback, posed, b, onBed;
        public bool idleWas, linWas, touchedIdle, touchedLin;
        public string blockedBy;
        public bool Arrived => arrivedAt >= 0f;
    }

    /// <summary>Un registro por franja y persona (para las pruebas y el panel): anduvo, llegó, cuánto la hizo.</summary>
    public struct Record
    {
        public string who, group, activity, venue; public int day, slot; public bool micro, arrived, fallback;
        public float walked, flown, performed, blocked, since; public int jumps; public string blockedBy;
    }
    public static readonly List<Record> Records = new List<Record>();

    readonly Dictionary<GameObject, Run> _runs = new Dictionary<GameObject, Run>();
    public Run RunOf(GameObject g) => g != null && _runs.TryGetValue(g, out Run r) ? r : null;
    /// <summary>Todos los que ahora tienen franja libre en marcha.</summary>
    public IEnumerable<KeyValuePair<GameObject, Run>> Runs => _runs;

    /// <summary>¿Lleva el horario libre a <paramref name="g"/> ahora? (la jornada y la de MicroJornada no lo mueven entonces)</summary>
    public static bool Drives(GameObject g)
    {
        if (!Active || g == null) return false;
        Run r = Instance.RunOf(g);
        return r != null && r.blockedBy == null;
    }

    /// <summary>¿Está tumbado en su cama con el horario libre (Descansar en casa)? Como quien duerme, no cuenta como «dentro de un mueble».</summary>
    public static bool OnBed(GameObject g) { Run r = Instance != null ? Instance.RunOf(g) : null; return r != null && r.onBed; }

    /// <summary>Qué hace ahora en su tiempo libre (o null).</summary>
    public static string StatusOf(GameObject g)
    {
        Run r = Instance != null ? Instance.RunOf(g) : null;
        if (r == null) return null;
        return r.Arrived ? $"Libre: {r.entry.activity.title}" : $"Libre: va a «{r.entry.activity.title}» ({r.venue.title})";
    }

    void Update()
    {
        if (!Active) { if (_runs.Count > 0) foreach (Group gr in groups) ReleaseAll(gr); return; }
        Scan();
        Tick();
    }

    /// <summary>Un fotograma del horario: empieza, conduce y termina las franjas de cada uno.</summary>
    public void Tick()
    {
        float h = TC.Hour; int day = TC.Day;
        int slot = LeisureRoster.SlotAt(h);
        foreach (Group gr in groups)
        {
            if (gr.plan == null) continue;
            bool inWeek = day >= gr.plan.firstDay && day < gr.plan.firstDay + LeisureRoster.DaysPerWeek;
            foreach (GameObject g in gr.bodies)
            {
                if (g == null) continue;
                LeisureRoster.Entry e = slot >= 0 && inWeek && gr.personOf.TryGetValue(g, out var p) ? gr.plan.At(p, day, slot) : null;
                // (2026-11-04, ronda 6 del grupo 89: 11/15 llegaban al ring; los de las torres lejanas —Producción, la guardia— no
                // llegaban en la hora) A una actividad de TODOS A LA VEZ se sale con tiempo: TogetherLead antes de su franja.
                if (e == null && inWeek && gr.personOf.TryGetValue(g, out var pe))
                {
                    int next = NextSlotWithin(h, TogetherLead);
                    LeisureRoster.Entry early = next >= 0 ? gr.plan.At(pe, day, next) : null;
                    if (early != null && early.activity.together) e = early;
                }
                _runs.TryGetValue(g, out Run run);
                if (run != null && run.entry != e) { Release(g, run); run = null; }
                if (e == null) continue;
                if (run == null) _runs[g] = run = Begin(gr, g, e);
                Drive(g, run);
            }
        }
    }

    /// <summary>Lo que se sale antes hacia una actividad de todos a la vez (horas de juego): 20 min, lo que tarda el vuelo más largo
    /// (de la torre de Producción o de Ciencias a la arboleda y al ring, con sus tres alfombras).</summary>
    public const float TogetherLead = 0.42f;   // (0,33 hasta 2026-11-05: Producción, ya en la isla de tierra, vuela unos 90 m más)

    /// <summary>La franja que empieza dentro de las próximas <paramref name="lead"/> horas (−1 si ninguna).</summary>
    public static int NextSlotWithin(float h, float lead)
    {
        for (int i = 0; i < LeisureRoster.SlotHours.Length; i++)
            if (h < LeisureRoster.SlotHours[i] && LeisureRoster.SlotHours[i] - h <= lead) return i;
        return -1;
    }

    Run Begin(Group gr, GameObject g, LeisureRoster.Entry e)
    {
        var r = new Run { group = gr, entry = e, venue = gr.venues.TryGetValue(e.activity, out Venue v) ? v : null, since = Time.time, last = g.transform.position };
        if (r.venue != null) r.to = Seat(gr, r, g);
        TravelPlan(gr, r, g);
        return r;
    }

    // A QUÉ PASO Y CON CUÁNTO TIEMPO (2026-11-04, HorarioLibreTest en Kapilavastu): en los pueblos ×100 los sitios están a cientos de
    // metros (el mirador en la meseta de un mueble, la plaza al otro lado del barrio) y a paso de jornada (2,4 m/s) no se llegaba
    // en los 60 s de MicroJornada: a todos se les ponía allí. Ahora, si está lejos, va a PASO LIGERO (hasta 6 m/s, para llegar en
    // ~75 s) y el tiempo para llegar crece con la distancia (de 60 a 120 s de juego; la franja dura 150 s).
    void TravelPlan(Group gr, Run r, GameObject g)
    {
        if (!gr.micro) { r.speed = walkSpeed; return; }
        float baseSpeed = g.TryGetComponent(out MicroJornada mj) ? mj.speed : microSpeed;
        Reachable(r, g, mj);
        Vector3 d = r.to - g.transform.position; d.y = 0f;
        float dist = d.magnitude;
        r.speed = Mathf.Clamp(dist / 75f, baseSpeed, Mathf.Max(baseSpeed, 6f));
        r.allowed = Mathf.Clamp(dist / r.speed * 1.4f + 15f, microFallback, 120f);
    }

    // UN SITIO AL QUE SE PUEDA LLEGAR ANDANDO (2026-11-04, ronda 3 en Kapilavastu: 0/5 llegaban andando; el agente se quedaba en el
    // final de un camino PARCIAL —el puesto está pegado a su mueble o en la meseta de un mueble gigante— y a los 60–120 s se le ponía
    // allí). Se busca, entre los puestos de los edificios de la actividad, uno con camino COMPLETO por el NavMesh (con sus trepadas) y
    // se va a su punto del NavMesh; si no hay ninguno, al final del camino parcial si queda junto al edificio (≤ 15 m).
    void Reachable(Run r, GameObject g, MicroJornada mj)
    {
        UnityEngine.AI.NavMeshAgent ag = g.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (ag == null || !ag.enabled || !ag.isOnNavMesh || r.venue == null) return;
        var cands = new List<(AreaFunctionStation st, Vector3 p)>();
        if (r.st != null) cands.Add((r.st, r.to));
        var sites = r.venue.home ? (mj != null && mj.home != null ? new List<AreaFunctionSite> { mj.home } : new List<AreaFunctionSite>()) : r.venue.sites;
        foreach (AreaFunctionSite s in sites)
            foreach (AreaFunctionStation st in s.Stations)
            {
                if (cands.Count >= 14) break;
                if (st == r.st || (st.occupant != null && st.occupant != (Object)g)) continue;
                if (r.venue.fn != null && r.venue.fn.stations.Length > 0 && System.Array.IndexOf(r.venue.fn.stations, st.station) < 0) continue;
                cands.Add((st, MicroJornada.WorkSpotOf(st, g.transform.position)));
            }
        var path = new UnityEngine.AI.NavMeshPath();
        Vector3? partialEnd = null; AreaFunctionStation partialSt = null;
        foreach (var (st, p) in cands)
        {
            if (!UnityEngine.AI.NavMesh.SamplePosition(p, out UnityEngine.AI.NavMeshHit hit, 4f, ag.areaMask)) continue;
            if (!ag.CalculatePath(hit.position, path)) continue;
            if (path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete) { Take(r, g, st, hit.position); r.reachable = true; return; }
            if (partialEnd == null && path.status == UnityEngine.AI.NavMeshPathStatus.PathPartial && path.corners.Length > 0)
            {
                Vector3 end = path.corners[path.corners.Length - 1];
                Vector3 site = st != null ? st.transform.position : p; Vector3 dd = end - site; dd.y = 0f;
                if (dd.magnitude <= 15f) { partialEnd = end; partialSt = st; }
            }
        }
        if (partialEnd.HasValue) { Take(r, g, partialSt, partialEnd.Value); r.reachable = true; }
    }

    static void Take(Run r, GameObject g, AreaFunctionStation st, Vector3 p)
    {
        if (st != null && st != r.st)
        {
            if (r.st != null && r.st.occupant == (Object)g) r.st.occupant = null;
            r.st = st; if (st.occupant == null) st.occupant = g;
        }
        r.to = p;
    }

    // Dónde se pone a hacerla: un sitio repartido por plazas (no se amontonan), sobre el NavMesh.
    Vector3 Seat(Group gr, Run r, GameObject g)
    {
        Venue v = r.venue;
        int seat = 0;
        foreach (var kv in _runs) if (kv.Value != r && kv.Value.venue == v && kv.Value.entry != null && kv.Value.entry.day == r.entry.day && kv.Value.entry.slot == r.entry.slot) seat++;
        if (gr.micro)
        {
            AreaFunctionSite site = null;
            if (v.home) site = g.TryGetComponent(out MicroJornada mj) ? mj.home : null;
            else
                foreach (AreaFunctionSite s in v.sites) { AreaFunctionStation free = s.StationFor(v.fn, g); if (free != null && (free.occupant == null || free.occupant == g)) { site = s; break; } }
            if (site == null && v.sites.Count > 0) site = v.sites[seat % v.sites.Count];
            if (site == null) return g.transform.position;
            r.st = site.StationFor(v.fn, g);
            if (r.st == null) { AreaFunctionStation[] all = site.Stations; if (all.Length > 0) r.st = all[Mathf.Abs(g.name.GetHashCode()) % all.Length]; }
            if (r.st == null) return site.transform.position;
            if (r.st.occupant == null) r.st.occupant = g;
            return MicroJornada.WorkSpotOf(r.st, g.transform.position);
        }
        if (v.home)
        {
            Bedroom room = g.TryGetComponent(out Anima a) ? Bedroom.Of(a) : null;
            if (room != null && room.bed != null) return OnMesh(room.FootSpot + Vector3.up);   // al pie de su cama (la de 4 es ancha: al lado caía encima)
            GameObject res = GameObject.Find("ArboledaResidencial");   // sin dormitorio: a la arboleda
            if (res == null) return g.transform.position;
            return OnMesh(res.transform.TransformPoint(new Vector3(-6f + (seat % 3) * 1.6f, ResidentialBuilder_DeckY + 1.15f, -1.6f + (seat / 3) * 1.6f)));
        }
        if (v.anchor == null) return g.transform.position;
        if (v.deck)   // la plataforma central (18 × 18): en las dos bandas de los lados (|x| 6 y 7,6 m), lejos de las pistas de alfombras del centro;
        {             // (regla J16) 4 por fila y 10 filas de 1,6 m: caben 40 (antes, 2 por fila: con todos los miembros se salían del borde)
            int row = (seat / 4) % 10;
            float x = (seat % 2 == 0 ? 6f : 7.6f) * (seat % 4 < 2 ? 1f : -1f);
            return OnMesh(v.anchor.TransformPoint(new Vector3(x, ResidentialBuilder_DeckY + 1.15f, -7.2f + row * 1.6f)));
        }
        // EL RING DE ALMOHADAS (2026-11-04): cada plaza, una celda del colchón de la cama colosal (PillowRing.Spot).
        if (v.anchor.TryGetComponent(out PillowRing ring)) return ring.Spot(seat) + Vector3.up;
        // LA PIJAMADA (2026-11-04): en la casa del anfitrión, cada plaza un punto delante de la cama-litera.
        if (!v.home && v.anchor.TryGetComponent(out Bedroom hostRoom)) return hostRoom.GatherSpot(seat) + Vector3.up;
        // Una SALA de los pisos de yoga: cada plaza, una de sus esterillas (de cara a la tarima).
        if (v.halls.Count > 0 || v.anchor.TryGetComponent(out YogaHall _))
        {
            List<CarpetPad> mats = new List<CarpetPad>();   // (regla J16: las esterillas de la sala en todos los pisos del estilo)
            if (v.halls.Count > 0) foreach (YogaHall hh in v.halls) mats.AddRange(hh.Mats()); else mats.AddRange(v.anchor.GetComponent<YogaHall>().Mats());
            if (mats.Count > 0) return OnMesh(mats[seat % mats.Count].transform.position + Vector3.up);
        }
        // (2026-11-04) EL MAESTRO de la clase (el baile: Panterilia o Gohageneis) se pone DELANTE de su grupo, a 1,5 m de la primera fila.
        if (r.entry != null && r.entry.lead) return OnMesh(v.anchor.TransformPoint(new Vector3(((v.index % 3) - 1) * 5f, 1f, 0f - (v.index / 3 % 2) * 4f)));
        // Un área: cada actividad en su zona (−5, 0, +5 m) y cada plaza en una rejilla de 1,6 m.
        float zx = ((v.index % 3) - 1) * 5f;
        Vector3 local = new Vector3(zx + ((seat % 3) - 1) * 1.6f, 1f, 1.5f + (seat / 3) * 1.6f - (v.index / 3 % 2) * 4f);
        return OnMesh(v.anchor.TransformPoint(local));
    }

    const float ResidentialBuilder_DeckY = 12f;   // la plataforma central de la arboleda (ResidentialBuilder.DeckY, en el Editor)

    static Vector3 OnMesh(Vector3 p)
    {
        if (UnityEngine.AI.NavMesh.SamplePosition(p + Vector3.down, out UnityEngine.AI.NavMeshHit hit, 3f, UnityEngine.AI.NavMesh.AllAreas) && Mathf.Abs(hit.position.y - (p.y - 1f)) < 1.5f)
            return hit.position + Vector3.up;
        return p;
    }

    /// <summary>Por qué ahora no se le puede llevar (null = se puede): el cuerpo, una coreografía, la Magnate, el servicio…</summary>
    public static string Blocked(GameObject g)
    {
        g.TryGetComponent(out Anima a);   // (los insectos-humanos del microcosmos pueden no tenerla)
        if (a != null && a.death) return "sin cuerpo";
        if (a != null && a.asleep) return "duerme";
        ActionQueue q = g.GetComponent<ActionQueue>();
        if (NeedsDriver.Attending(q)) return q.Own[0].Label;
        if (q != null && q.Own.Count > 0 && q.Own[0] is CarpetWalkSpell) return q.Own[0].Label;
        if (Orchestra.Busy(g)) return "en una coreografía";
        if (MagnateWarden.Instance != null && MagnateWarden.Instance.Handling(g)) return "con la Magnate";
        if (RestaurantService.Drives(g)) return "servicio del restaurante";
        if (WorkdayDirector.Instance != null && WorkdayDirector.Instance.OnShift(g)) return "en su jornada";
        if (g.TryGetComponent(out MicroJornada mj) && mj.hold) return "la historia lo necesita";
        if (g.GetComponent<Linduarda>() != null && WorkdayDirector.NewcomerWaiting()) return "atiende a un recién llegado";
        return null;
    }

    void Drive(GameObject g, Run r)
    {
        // Lo que anduvo (y los saltos: más de lo que da de sí un paso en ese fotograma, sin ir en alfombra). Solo cuenta lo que pasa
        // mientras lo lleva el horario: si una necesidad o la historia lo mueve (a su cama, al baño…), eso no es un salto del horario.
        Vector3 pos = g.transform.position;
        float step = (pos - r.last).magnitude;
        bool riding = FlyingCarpet.Riding(g);
        string why = r.venue == null ? null : Blocked(g);
        if (why == null && r.blockedBy == null)
        {
            if (riding) r.flown += step;
            else if (step > Mathf.Max(4f, r.speed * 3f * Time.deltaTime + 1.5f)) r.jumps++;   // (con el reloj acelerado un paso de un fotograma lento es largo)
            else r.walked += step;
        }
        r.last = pos;

        if (r.venue == null) return;
        if (why != null)
        {
            if (r.blockedBy == null && r.posed) Unpose(g, r);
            r.blockedBy = why; r.blockedFor += Time.deltaTime;
            return;
        }
        r.blockedBy = null;
        Suspend(g, r);
        Anima a = g.GetComponent<Anima>();
        if (!r.Arrived)
        {
            SetStatus(g, r, StatusOf(g));
            Vector3 d = r.to - pos; float dy = Mathf.Abs(d.y); d.y = 0f;
            bool there = d.magnitude <= (r.group.micro ? 1.6f : 1.7f) && dy < 2.2f;
            // EL RING (grupo 89): quien ya pisa el colchón a menos de una celda y media de la suya, ha llegado (se pega almohadazos desde ahí).
            if (!there && r.venue.anchor != null && r.venue.anchor.TryGetComponent(out PillowRing pring) && pring.OnMattress(pos, 0f) && d.magnitude <= PillowRing.CellSize * 1.5f) there = true;
            if (!there && r.group.micro && d.magnitude <= 4f && g.TryGetComponent(out UnityEngine.AI.NavMeshAgent mag) && mag.enabled && mag.isOnNavMesh && !mag.pathPending && mag.hasPath && mag.remainingDistance <= 0.8f) there = true;   // (el agente ya está al final de su camino, junto al puesto)
            if (!there)
            {
                if (r.group.micro)
                {
                    if (!MicroMove.Step(g.transform, r.to, r.speed, 1.6f) && Time.time - r.since - r.blockedFor > r.allowed)
                    {
                        // (con NavMeshAgent hay que llevarlo con Warp: mover el transform lo dejaba volver a su sitio en el fotograma
                        // siguiente, y contaba decenas de «saltos» de ida y vuelta)
                        WorkdayDirector.Teleport(g.transform, r.to); r.fallback = true; r.last = g.transform.position;
                        Debug.LogWarning($"[Horario] «{g.name}» no llega andando en {r.allowed:0} s a «{r.venue.title}»: se le pone allí.");
                    }
                    return;
                }
                Vector3 to = r.to;
                if (riding || CarpetPad.Route(g, ref to)) return;   // a otro piso: en alfombra voladora
                PathMover.Of(g.transform).StepToward(to, walkSpeed);
                return;
            }
            r.arrivedAt = Time.time;
            Debug.Log($"[Horario] {TC.Hour:00.0} h · «{g.name}» llega a «{r.entry.activity.title}» ({r.venue.title}) tras {r.walked:0} m andando{(r.flown > 1f ? $" y {r.flown:0} m en alfombra" : "")}.");
        }
        // HACERLA: su postura o su gesto, mirando al centro del sitio.
        r.performed += Time.deltaTime;
        SetStatus(g, r, StatusOf(g));
        // LA GUERRA DE ALMOHADAS (docs/guerra-de-almohadas.md): coge una almohada y da almohadazos a los de al lado, sin salir de su celda.
        if (r.entry.activity.pose == "almohadas") { if (a != null) PillowFight.Tick(g, a, r.venue.anchor != null ? r.venue.anchor.GetComponent<PillowRing>() : null); r.posed = true; return; }
        // LA PIJAMADA (docs/pijamadas.md): al llegar a la casa del anfitrión se apunta a dormir allí esta noche (Sleepover: su plaza en la
        // cama-litera, abajo o arriba); luego, sentado charlando con los demás.
        if (r.entry.activity.pose == Sleepover.ActivityId && a != null && !r.posed && r.venue.anchor != null && r.venue.anchor.TryGetComponent(out Bedroom host)) Sleepover.Join(a, host, r.entry.day);
        Vector3 c = r.st != null ? r.st.transform.position : r.venue.anchor != null ? r.venue.anchor.position : r.to;
        Vector3 look = c - g.transform.position; look.y = 0f;
        string pose = r.entry.activity.pose;
        if (pose == "baile") g.transform.Rotate(0f, 120f * Time.deltaTime, 0f);
        else if (look.sqrMagnitude > 0.04f && !r.onBed) g.transform.rotation = Quaternion.Slerp(g.transform.rotation, Quaternion.LookRotation(look), Time.deltaTime * 4f);
        // (2026-11-04) HACERLA DE VERDAD (docs/actividades-y-uniformes.md §3): leer con su libro, bordar, charlar con su taza, regar,
        // jugar con las crías…; el insecto-humano, la actuación de su actividad o el bucle de la clase del edificio (con lo que tenga en
        // las manos y su efecto). Las que tienen su propio sistema (yoga, baile, pesas, almohadas, pijamada, descansar) siguen igual.
        ActivityPerformer.Performance perf = Performances.ForLeisure(r.entry.activity.id);
        CubeInsect ci = g.GetComponent<CubeInsect>();
        if (ci != null)
        {
            ActivityPerformer.Keep(g, perf ?? Performances.ForKind(r.venue.kind == AreaFunctions.Kind.Rest && pose == "pie" ? AreaFunctions.Kind.Care : r.venue.kind), c);
            r.posed = true; return;
        }
        if (a == null) return;
        if (perf != null && !r.onBed) { ActivityPerformer.Keep(g, perf, c); r.posed = true; return; }
        if (!r.posed)
        {
            r.posed = true; r.poseAt = Time.time;
            if (pose == "tumbado") LieOnBed(g, a, r);
            else Pose(pose, false).ApplyFinal(a);
        }
        else if ((pose == "ejercicio" || pose == "baile") && Time.time - r.poseAt > (pose == "baile" ? 0.8f : 1.4f))
        { r.poseAt = Time.time; r.b = !r.b; Pose(pose, r.b).ApplyFinal(a); }
    }

    static AsanaPoseSpell Pose(string pose, bool b)
    {
        switch (pose)
        {
            case "sentado": return DailyPoses.Sit();
            case "pijamada": return DailyPoses.Sit();   // charlando en la casa del anfitrión, antes de dormir
            case "meditar": return DailyPoses.Meditate();
            case "tumbado": return DailyPoses.Lie();
            case "ejercicio": return b ? DailyPoses.Sit() : DailyPoses.Stand();   // sentadillas
            case "baile": return b ? DailyPoses.Meditate() : DailyPoses.Stand();
            default: return DailyPoses.Stand();
        }
    }

    // DESCANSAR EN CASA: tumbado encima de su cama (como al dormir, SleepSpell, pero despierto).
    void LieOnBed(GameObject g, Anima a, Run r)
    {
        Bedroom room = Bedroom.Of(a);
        if (room == null || room.bed == null || Vector3.Distance(room.bed.position, g.transform.position) > 3.5f) { DailyPoses.Sit().ApplyFinal(a); return; }
        CharacterController cc = g.GetComponent<CharacterController>(); if (cc != null) cc.enabled = false;
        g.transform.position = room.LieSpot(a) + Vector3.up * 0.5f;   // en su plaza de la cama de 4
        g.transform.rotation = room.LieRotation(a) * Quaternion.Euler(0f, 180f, 0f);
        if (cc != null) cc.enabled = true;
        r.onBed = true; r.last = g.transform.position;
        DailyPoses.Lie().ApplyFinal(a);
    }

    void Unpose(GameObject g, Run r)
    {
        r.posed = false;
        PillowFight.Stop(g);   // deja la almohada en el ring (si la llevaba)
        if (g.TryGetComponent(out Anima a) && !a.asleep) DailyPoses.Stand().ApplyFinal(a);
        if (r.onBed) { r.onBed = false; WorkdayDirector.Teleport(g.transform, r.to); r.last = r.to; }
    }

    // Mientras lo lleva el horario no merodea (IdleBehavior) ni hace la conducta propia de Linduarda; al acabar, se le devuelven.
    void Suspend(GameObject g, Run r)
    {
        IdleBehavior idle = g.GetComponent<IdleBehavior>();
        if (idle != null && idle.enabled) { if (!r.touchedIdle) { r.touchedIdle = true; r.idleWas = true; } idle.enabled = false; }
        Linduarda lin = g.GetComponent<Linduarda>();
        if (lin != null && lin.enabled) { if (!r.touchedLin) { r.touchedLin = true; r.linWas = true; } lin.enabled = false; }
        ActionQueue q = g.GetComponent<ActionQueue>();
        if (q != null && q.Player.Count > 0 && q.Player[0] is WanderSpell) q.RemovePlayerAt(0);
    }

    void Release(GameObject g, Run r)
    {
        _runs.Remove(g);
        if (g == null || r == null) return;
        if (r.posed) Unpose(g, r);
        if (r.touchedIdle && g.TryGetComponent(out IdleBehavior idle)) idle.enabled = r.idleWas;
        if (r.touchedLin && g.TryGetComponent(out Linduarda lin)) lin.enabled = r.linWas;
        if (r.st != null && r.st.occupant == (Object)g) r.st.occupant = null;
        SetStatus(g, r, null);
        Records.Add(ToRecord(g, r));
        if (Records.Count > 4000) Records.RemoveRange(0, 1000);
    }

    /// <summary>El registro de una franja (en marcha o terminada).</summary>
    static Record ToRecord(GameObject g, Run r) => new Record
    {
        who = g.name, group = r.group.name, micro = r.group.micro, activity = r.entry.activity.title, venue = r.venue != null ? r.venue.title : "—",
        day = r.entry.day, slot = r.entry.slot, arrived = r.Arrived, fallback = r.fallback, walked = r.walked, flown = r.flown,
        performed = r.performed, blocked = r.blockedFor, since = Time.time - r.since, jumps = r.jumps, blockedBy = r.blockedBy
    };

    /// <summary>Los registros de las franjas que AÚN están en marcha (sin terminarlas): las pruebas miden también lo que va por la mitad.</summary>
    public List<Record> Snapshot() { var l = new List<Record>(); foreach (var kv in _runs) if (kv.Key != null) l.Add(ToRecord(kv.Key, kv.Value)); return l; }

    /// <summary>Termina todas las franjas en marcha del grupo (o de todos): cada uno, de pie y con su merodeo de vuelta.</summary>
    public void ReleaseAll(Group gr = null)
    {
        foreach (var kv in _runs.ToList()) if (gr == null || kv.Value.group == gr) Release(kv.Key, kv.Value);
    }

    // El rótulo: en el santuario, como la jornada (<…> bajo el nombre); en el microcosmos, «Nombre · verbo».
    static void SetStatus(GameObject g, Run r, string doing)
    {
        if (!r.group.micro) { WorkdayDirector.SetStatus(g, doing); return; }
        if (!g.TryGetComponent(out WorldLabel wl)) return;
        string baseName = g.TryGetComponent(out MicroJornada mj) ? mj.DisplayName : g.name;
        wl.text = doing == null ? baseName : $"{baseName} · {(r.Arrived ? AreaFunctions.Verb(r.venue.kind) + ": " + r.entry.activity.title : doing)}";
    }

    // ── Para el panel y la consola ───────────────────────────────────────────────────────────────

    /// <summary>Texto del horario de hoy del grupo (consola <c>horario</c>).</summary>
    public List<string> Report(Group gr)
    {
        var l = new List<string>();
        if (gr == null || gr.plan == null) { l.Add("Aún no hay horario libre en esta escena."); return l; }
        int day = TC.Day;
        l.Add($"Horario libre · {gr.name} · semana {gr.plan.week} · día {day} · uniformidad Δ={gr.spread} ({gr.worst}) · por persona Δ={gr.spreadPerPerson}");
        foreach (LeisureRoster.Person p in gr.plan.people)
        {
            var cells = new List<string>();
            for (int s = 0; s < LeisureRoster.SlotHours.Length; s++) { var e = gr.plan.At(p, day, s); cells.Add($"{LeisureRoster.SlotHours[s]:00}h {(e != null ? e.activity.title : "—")}"); }
            l.Add($"{p.name}: {string.Join(" · ", cells)}");
        }
        return l;
    }
}

/// <summary>
/// ETIQUETAS DE ACTIVIDAD de los edificios (docs/horario-libre.md §Sitios): lo que un edificio ofrece en el tiempo libre (ocio,
/// descanso, recuperación), con su capacidad. Un edificio etiquetado entra en el catálogo de su escena como una actividad más.
/// Los pueblos del microcosmos ponen la suya al construirse; mientras no la tengan, mandan las plantillas de
/// <see cref="LeisureDirector.MicroActivities"/>.
/// </summary>
public static class LeisureTags
{
    /// <summary>Proveedores de sitios etiquetados: (grupo, edificios de la escena) → actividades con su sitio. Otros sistemas se
    /// apuntan aquí sin que el horario dependa de ellos.</summary>
    public static readonly List<System.Action<LeisureDirector.Group, List<AreaFunctionSite>>> Providers = new List<System.Action<LeisureDirector.Group, List<AreaFunctionSite>>>();

    public static void AddVenues(LeisureDirector.Group gr, List<AreaFunctionSite> sites = null)
    {
        foreach (var p in Providers)
        {
            try { p(gr, sites ?? new List<AreaFunctionSite>()); }
            catch (System.Exception ex) { Debug.LogWarning($"[Horario] un proveedor de etiquetas falló: {ex.Message}"); }
        }
    }
}
