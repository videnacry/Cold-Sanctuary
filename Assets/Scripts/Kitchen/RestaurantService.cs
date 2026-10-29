using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SERVICIO EN VIVO DEL RESTAURANTE (docs/restaurant-plan.md §5, Berón 2026-10-29: «el restaurante con los trabajadores
/// acordados: cocineros, limpieza y obradores, y los turistas que piden su comida y la reciben en su mesa»). La BRIGADA es
/// quien la jornada (<see cref="WorkdayDirector"/>) tiene ahora en una misión de la Cocina, con su papel según la misión:
///  · coc_servicio → COCINERO en su puesto completo: saca UNA ración de la despensa (<see cref="KitchenLarder.Serve"/>), la
///    cocina en el equipo del plato de SU puesto (wok, paellero, horno de pizza…), emplata en su encimera y la deja en el pase;
///  · coc_atender / coc_sala → CAMARERO (el del pase): recoge los platos del pase y los sube en alfombra a la MESA del turista;
///  · coc_obrador / prep_* → OBRADOR: elabora POR MAYOR en su obrador (masas: panadería y pasta; lácteos y fermentos) lo que la
///    despensa necesita (<see cref="KitchenLarder.StartPrep"/>);
///  · coc_comedor → LIMPIEZA DEL COMEDOR: recoge y limpia cada mesa que deja un turista;
///  · coc_limpieza → LIMPIEZA DE LA COCINA: friega el puesto de cada cocinero tras cada plato.
/// Los TURISTAS (copias de cuerpos, como en la virtualización) llegan al pie de la torre, suben en alfombra al comedor del
/// anillo, se sientan a una mesa libre, PIDEN un plato de la carta (una ración), lo reciben en su mesa, comen y se van.
/// Todos andan con <see cref="PathMover"/> (NavMesh y alfombras); cada espera tiene su tope. Las pruebas leen su estado.
/// </summary>
public class RestaurantService : MonoBehaviour
{
    public static RestaurantService Instance { get; private set; }

    public enum Role { Cocinero, Camarero, Obrador, LimpiezaCocina, LimpiezaSala }
    public enum OrderState { Pending, Cooking, Ready, Carrying, Served, Eaten, Cancelled }
    public enum DinerState { Arriving, Seated, Eating, Leaving, Gone }

    [Header("Tiempos (s)")]
    public float takeSeconds = 2f, cookSeconds = 6f, plateSeconds = 2f, eatSeconds = 10f, cleanSeconds = 3f, prepSeconds = 6f;
    [Tooltip("Tope de cada tramo a pie (incluidas las alfombras): si no llega, se da por atascado y sigue.")]
    public float legTimeout = 150f;
    [Tooltip("Lo que espera un turista sentado a que le llegue el plato antes de irse.")]
    public float waitTimeout = 420f;
    public float staffSpeed = 2.2f, dinerSpeed = 1.6f;
    public int maxPlates = 3;
    [Tooltip("La CARTA DEL DÍA (nombres de Gastronomy.Menu). Vacía: toda la carta.")]
    public List<string> carta = new List<string>();

    // ── Lo que ven las pruebas ──
    public class Table
    {
        public Transform mesa; public Transform[] chairs;
        public Diner occupant; public Actor cleaner; public Transform leftover;
        public bool dirty; public int uses, lastChair; public float leftAt = -1f, cleanedAt = -1f; public string cleanedBy;
        public string Name => mesa != null ? mesa.name : "?";
    }

    public class Order
    {
        public int number; public Diner diner; public Table table; public Gastronomy.Dish dish; public CookingRecipe recipe;
        public OrderState state; public Actor cook, waiter; public Transform plate, station;
        public float orderedAt = -1f, readyAt = -1f, servedAt = -1f, eatenAt = -1f, stationDistance = -1f, plateOffset = -1f;
        public string cookName, waiterName, puesto, stationName, servedTable, why;
        public readonly Dictionary<string, float> taken = new Dictionary<string, float>();   // lo que salió de la despensa
    }

    public class Diner
    {
        public int number; public GameObject go; public ServiceActor tag; public Mover mv;
        public DinerState state; public Table table; public int chair; public Order order;
        public float spawnedAt, seatedAt = -1f, leftAt = -1f, goneAt = -1f, until;
    }

    public class Actor
    {
        public GameObject go; public Role role; public string mission; public int index; public ServiceActor tag; public Mover mv;
        public Transform puesto, nevera, encimera, fregadero;   // cocinero: su puesto completo
        public string family;                                   // obrador: «masas» o «lácteos y fermentos»
        public Order order; public readonly List<Order> carry = new List<Order>();
        public Table table; public Transform cleaning, held, station;
        public Gastronomy.Prep prep;
        public int step; public float until = -1f, restUntil;
    }

    public class PrepLog { public string who, prep, equipment, station, room; public int portions; public bool workshop; public float at; }
    public class CleanLog { public string who, what; public bool table; public float at; }

    readonly List<Actor> _crew = new List<Actor>();
    readonly HashSet<GameObject> _crewSet = new HashSet<GameObject>();
    readonly List<Order> _orders = new List<Order>();
    readonly List<Diner> _diners = new List<Diner>();
    readonly List<Table> _tables = new List<Table>();
    readonly List<PrepLog> _preps = new List<PrepLog>();
    readonly List<CleanLog> _cleans = new List<CleanLog>();
    readonly List<Transform> _dirtyPuestos = new List<Transform>();
    readonly Transform[] _puestos = new Transform[8];

    public IReadOnlyList<Actor> Crew => _crew;
    public IReadOnlyList<Order> Orders => _orders;
    public IReadOnlyList<Diner> Diners => _diners;
    public IReadOnlyList<Table> Tables => _tables;
    public IReadOnlyList<PrepLog> Preps => _preps;
    public IReadOnlyList<CleanLog> Cleans => _cleans;
    public CarpetPad ArrivalPad => _arrivalPad;
    public CarpetPad RingPad => _ringPad;
    public Transform Pass => _pass;

    KitchenLarder _larder;
    Transform _area, _pass, _caja, _lavado;
    CarpetPad _ringPad, _arrivalPad, _floorPad;
    float _locateAt, _refreshAt;
    int _orderN, _dinerN;

    static TimeController TC => TimeController.timeController;

    // En el santuario: el servicio vive en la sala de la cocina (la de la despensa). Se añade solo al cargar la escena.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        if (Instance != null) return;
        foreach (KitchenLarder l in FindObjectsByType<KitchenLarder>(FindObjectsSortMode.None))
            if (l.transform.parent != null && l.transform.parent.GetComponent<RestaurantKitchen>() != null) { l.gameObject.AddComponent<RestaurantService>(); return; }
    }

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }
    void OnDestroy() { ResetService(); }

    /// <summary>¿Lleva el servicio a <paramref name="g"/> ahora? (la jornada no lo manda a un punto del área)</summary>
    public static bool Drives(GameObject g) => Instance != null && g != null && (!ColdSanctuary.TestRunner.Running || AllowDuringTests) && Instance._crewSet.Contains(g) && !Busy(g);

    /// <summary>El papel en el restaurante de una misión de la Cocina (null si no es del servicio).</summary>
    public static Role? RoleOf(string missionId)
    {
        if (string.IsNullOrEmpty(missionId)) return null;
        if (missionId == "coc_servicio") return Role.Cocinero;
        if (missionId == "coc_atender" || missionId == "coc_sala") return Role.Camarero;
        if (missionId == "coc_obrador" || missionId.StartsWith("prep_")) return Role.Obrador;
        if (missionId == "coc_limpieza") return Role.LimpiezaCocina;
        if (missionId == "coc_comedor") return Role.LimpiezaSala;
        return null;
    }

    static string RoleName(Role r) => r switch
    {
        Role.Cocinero => "Cocinero", Role.Camarero => "Camarero (pase y sala)", Role.Obrador => "Obrador",
        Role.LimpiezaCocina => "Limpieza de cocina y lavado", _ => "Limpieza del comedor",
    };

    // Necesidades del cuerpo (comer, el baño, dormir) van antes: mientras, no lo mueve el servicio.
    static bool Busy(GameObject g)
    {
        if (g.TryGetComponent(out ActionQueue q) && q.Own.Count > 0) return true;
        if (AreaInitiator.Leading(g) || (g.TryGetComponent(out NewcomerInitiator ni) && ni.Busy)) return true;   // inicia a un recién llegado: lo lleva él
        return g.TryGetComponent(out Anima a) && (a.asleep || a.death);
    }

    /// <summary>Durante las pruebas el servicio está quieto (usan a los personajes), salvo la suya.</summary>
    public static bool AllowDuringTests;

    // ── Dónde está todo ─────────────────────────────────────────────────────────────────────────────────────────────

    bool Locate()
    {
        if (_pass != null && _area != null) return true;
        if (Time.time < _locateAt) return false;
        _locateAt = Time.time + 3f;
        _larder = GetComponent<KitchenLarder>();
        if (_larder == null) _larder = KitchenLarder.Instance;
        GameObject ka = GameObject.Find("Kitchen_Area");
        if (ka == null) return false;
        _area = ka.transform;
        _pass = VirtualInPlace.FindDeep(transform, "Barra de servicio");
        if (_pass == null) return false;
        _caja = VirtualInPlace.FindDeep(_area, "Caja_registradora");
        _lavado = VirtualInPlace.FindDeep(transform, "Lavado");
        for (int i = 0; i < _puestos.Length; i++) _puestos[i] = transform.Find($"Puesto_{i + 1}");
        _tables.Clear();
        Transform dining = VirtualInPlace.FindDeep(_area, "Comedor_Gourmet");
        if (dining != null)
            foreach (Transform t in dining.GetComponentsInChildren<Transform>())
            {
                if (!t.name.StartsWith("Mesa_")) continue;
                var chairs = new List<Transform>();
                foreach (Transform c in t) if (c.name == "Asiento") chairs.Add(c);
                if (chairs.Count > 0) _tables.Add(new Table { mesa = t, chairs = chairs.ToArray() });
            }
        _ringPad = null; _arrivalPad = null;
        foreach (CarpetPad p in CarpetPad.All) if (p != null && p.surface.Contains("#anillo") && p.transform.IsChildOf(_area)) { _ringPad = p; break; }
        if (_ringPad != null) foreach (CarpetPad p in CarpetPad.All) if (p != null && p.building == _ringPad.building && p.surface == _ringPad.building + "#0") { _arrivalPad = p; break; }
        _floorPad = CarpetPad.For(_pass.position + Vector3.up * 0.5f);
        if (_arrivalPad == null) _arrivalPad = _floorPad != null ? _floorPad : _ringPad;
        Debug.Log($"[Restaurante] Servicio en vivo: {_tables.Count} mesas en el anillo, pase «{_pass.name}», llegada de turistas por «{(_arrivalPad != null ? _arrivalPad.displayName : "?")}» y comedor por «{(_ringPad != null ? _ringPad.displayName : "?")}».");
        return true;
    }

    // ── Bucle ───────────────────────────────────────────────────────────────────────────────────────────────────────

    void Update()
    {
        if (ColdSanctuary.TestRunner.Running && !AllowDuringTests) return;
        if (!Locate()) return;
        if (Time.time >= _refreshAt) { _refreshAt = Time.time + 1f; RefreshCrew(); }
        for (int i = 0; i < _crew.Count; i++)
        {
            Actor a = _crew[i];
            if (a.go == null || Busy(a.go)) continue;
            switch (a.role)
            {
                case Role.Cocinero: TickCook(a); break;
                case Role.Camarero: TickWaiter(a); break;
                case Role.Obrador: TickObrador(a); break;
                case Role.LimpiezaCocina: TickCleanKitchen(a); break;
                default: TickCleanDining(a); break;
            }
        }
        for (int i = 0; i < _diners.Count; i++) TickDiner(_diners[i]);
    }

    /// <summary>Relee ya la brigada de la jornada (las pruebas lo llaman tras poner el turno).</summary>
    public void RefreshCrewNow() { if (Locate()) RefreshCrew(); }

    // LA BRIGADA = quien la jornada tiene ahora en una misión de la Cocina con papel en el servicio.
    void RefreshCrew()
    {
        var want = new List<(GameObject g, string mission)>();
        WorkdayDirector wd = WorkdayDirector.Instance;
        if (wd != null && wd.today != null && TC != null)
        {
            float h = TC.Hour;
            var seen = new HashSet<string>();
            foreach (WorkRoster.Assignment a in wd.today.work)
            {
                if (a.mission.area != "Cocina" || RoleOf(a.mission.id) == null || !seen.Add(a.mission.id)) continue;
                foreach (GameObject g in wd.DoingNow(a.mission.id, h))
                    if (!want.Exists(x => x.g == g)) want.Add((g, a.mission.id));
            }
        }
        for (int i = _crew.Count - 1; i >= 0; i--)
        {
            Actor c = _crew[i];
            if (c.go == null || !want.Exists(x => x.g == c.go && x.mission == c.mission)) Release(c);
        }
        foreach (var (g, m) in want) if (!_crewSet.Contains(g)) Hire(g, m);
    }

    void Hire(GameObject g, string mission)
    {
        Role r = RoleOf(mission).Value;
        var a = new Actor { go = g, role = r, mission = mission, tag = g.GetOrAdd<ServiceActor>() };
        a.tag.role = RoleName(r); a.tag.moving = false;
        a.mv = new Mover(g.transform, a.tag);
        a.index = _crew.FindAll(x => x.role == r).Count;
        if (r == Role.Cocinero)
        {
            for (int i = 0; i < _puestos.Length; i++)
                if (_puestos[i] != null && !_crew.Exists(x => x.puesto == _puestos[i])) { a.puesto = _puestos[i]; break; }
            if (a.puesto != null)
            {
                a.nevera = Child(a.puesto, "Nevera"); a.encimera = Child(a.puesto, "Encimera"); a.fregadero = Child(a.puesto, "Fregadero");
            }
        }
        if (r == Role.Obrador) a.family = _crew.Exists(x => x.role == Role.Obrador && x.family == Masas) ? Dairy : Masas;
        _crew.Add(a); _crewSet.Add(g);
        Doing(a, "entra de turno");
        Debug.Log($"[Restaurante] «{g.name}» entra de {RoleName(r)}{(a.puesto != null ? $" en el {a.puesto.name}" : a.family != null ? $" ({a.family})" : "")} («{mission}»).");
    }

    void Release(Actor a)
    {
        if (a.order != null && a.order.state == OrderState.Cooking) { a.order.state = OrderState.Pending; a.order.cook = null; }
        foreach (Order o in a.carry) if (o != null && o.state == OrderState.Carrying) { o.state = OrderState.Ready; o.waiter = null; if (o.plate != null) PlaceOnPass(o); }
        a.carry.Clear();
        if (a.table != null) a.table.cleaner = null;
        if (a.cleaning != null && !_dirtyPuestos.Contains(a.cleaning)) _dirtyPuestos.Add(a.cleaning);
        DropHeld(a);
        if (a.tag != null) { a.tag.moving = false; a.tag.doing = ""; }
        RestoreLabel(a.go);
        _crew.Remove(a); _crewSet.Remove(a.go);
        if (a.go != null) Debug.Log($"[Restaurante] «{a.go.name}» deja el turno de {RoleName(a.role)}.");
    }

    static Transform Child(Transform p, string prefix) { foreach (Transform c in p) if (c.name.StartsWith(prefix)) return c; return null; }

    /// <summary>Dónde empieza el turno cada uno (la jornada coloca a todos en su área al arrancar).</summary>
    public Vector3 PostOf(Actor a)
    {
        switch (a.role)
        {
            case Role.Cocinero: return a.encimera != null ? Front(a.encimera, 0.85f) : PassPoint(a);
            case Role.Camarero: return PassPoint(a);
            case Role.Obrador: return ObradorHome(a);
            case Role.LimpiezaCocina: return _lavado != null ? Front(_lavado, 1.1f) : PassPoint(a);
            default: return DiningIdle(a);
        }
    }

    // ── Cocineros ───────────────────────────────────────────────────────────────────────────────────────────────────

    void TickCook(Actor a)
    {
        if (a.order == null || a.order.state != OrderState.Cooking || a.order.cook != a)
        {
            a.order = null;
            Order next = _orders.Find(x => x.state == OrderState.Pending);
            if (next == null || a.puesto == null) { DropHeld(a); Doing(a, "libre en su puesto"); Idle(a, PostOf(a)); return; }
            next.state = OrderState.Cooking; next.cook = a; next.cookName = a.go.name; next.puesto = a.puesto.name;
            next.recipe = RecipeOf(next.dish);
            a.order = next; a.step = next.taken.Count > 0 ? 1 : 0; a.until = -1f;
            Debug.Log($"[Restaurante] «{a.go.name}» toma la comanda #{next.number} ({next.dish.name}, una ración) en el {a.puesto.name}.");
        }
        Order o = a.order;
        switch (a.step)
        {
            case 0:   // 1. de la nevera de su puesto: UNA ración de cada componente
                Doing(a, $"#{o.number} {o.dish.name}: saca los ingredientes");
                if (!Reach(a, a.nevera != null ? Front(a.nevera, 0.9f) : PostOf(a), a.nevera)) return;
                if (!Act(a, takeSeconds)) return;
                if (!TakeFromLarder(o)) { Cancel(o, "falta género en la despensa"); a.order = null; return; }
                Hold(a, KitchenProps.Ingredient(null, o.dish.components[0], Vector3.zero));
                StepLog(a, o, 0); a.step = 1; return;
            case 1:   // 2. al equipo del plato, en SU puesto
                if (o.station == null) { o.station = StationFor(a, o.dish.equipment); o.stationName = o.station != null ? o.station.name : "?"; }
                Doing(a, $"#{o.number} {o.dish.name}: cocina en {o.dish.equipment}");
                Vector3 at = o.station != null ? Front(o.station, 0.85f) : PostOf(a);
                if (!Reach(a, at, o.station)) return;
                if (o.stationDistance < 0f && o.station != null) o.stationDistance = Flat(a.go.transform.position - o.station.position);
                if (!Act(a, cookSeconds)) return;
                DropHeld(a); StepLog(a, o, 1); a.step = 2; return;
            case 2:   // 3. emplata en su encimera
                Doing(a, $"#{o.number} {o.dish.name}: emplata");
                if (!Reach(a, a.encimera != null ? Front(a.encimera, 0.85f) : PostOf(a), a.encimera)) return;
                if (!Act(a, plateSeconds)) return;
                o.plate = KitchenProps.Dish(null, o.dish.name, Vector3.zero);
                o.plate.name = $"Plato_{o.number}";
                WorldLabel wl = o.plate.gameObject.AddComponent<WorldLabel>(); wl.text = $"#{o.number} {o.dish.name}"; wl.height = 0.35f;
                Hold(a, o.plate, plate: true);
                StepLog(a, o, 2); a.step = 3; return;
            default:  // 4. al pase
                Doing(a, $"#{o.number} {o.dish.name}: lo deja en el pase");
                if (!Reach(a, PassPoint(a), _pass)) return;
                a.held = null;
                PlaceOnPass(o);
                o.state = OrderState.Ready; o.readyAt = Time.time;
                StepLog(a, o, 3);
                if (!_dirtyPuestos.Contains(a.puesto)) _dirtyPuestos.Add(a.puesto);
                Debug.Log($"[Restaurante] Pedido #{o.number} ({o.dish.name}) listo en el pase: lo cocinó «{a.go.name}» en {o.stationName} del {a.puesto.name}.");
                a.order = null; return;
        }
    }

    /// <summary>La receta del servicio: individual (una ración, para el turista), con los pasos de verdad.</summary>
    public static CookingRecipe RecipeOf(Gastronomy.Dish d) => new CookingRecipe(d.name,
        $"toma de la nevera y la despensa una ración de {string.Join(", ", d.components)}",
        $"cocina en el {d.equipment}", "emplata en la encimera", "deja el plato en el pase") { servings = 1 };

    void StepLog(Actor a, Order o, int i)
    {
        if (o.recipe != null && i < o.recipe.steps.Length) Debug.Log($"[Cocina] «{a.go.name}» {o.dish.name}: {o.recipe.steps[i]}");
    }

    bool TakeFromLarder(Order o)
    {
        if (o.taken.Count > 0) return true;
        if (_larder == null) return false;
        var before = new Dictionary<string, float>();
        foreach (string c in o.dish.components) before[c] = _larder.Have(c);
        if (!_larder.Serve(o.dish.name)) return false;
        foreach (string c in o.dish.components) o.taken[c] = before[c] - _larder.Have(c);
        return true;
    }

    // El equipo del plato en el puesto del cocinero (todos los puestos tienen la carta completa); si no, el más cercano.
    Transform StationFor(Actor a, string equipment)
    {
        string n = "Equipo_" + equipment;
        Transform t = a.puesto != null ? a.puesto.Find(n) : null;
        return t != null ? t : Nearest(transform, n, a.go.transform.position, null);
    }

    void PlaceOnPass(Order o)
    {
        int slot = 0; foreach (Order x in _orders) if (x != o && x.state == OrderState.Ready && x.plate != null) slot++;
        o.plate.SetParent(_pass, true);
        o.plate.SetPositionAndRotation(_pass.position + _pass.right * (-2.0f + (slot % 8) * 0.55f) + Vector3.up * (KitchenProps.CounterTop + 0.02f), _pass.rotation);
    }

    // ── Camarero ────────────────────────────────────────────────────────────────────────────────────────────────────

    void TickWaiter(Actor a)
    {
        a.carry.RemoveAll(o => o == null || o.state != OrderState.Carrying);
        if (a.carry.Count == 0)
        {
            if (!_orders.Exists(o => o.state == OrderState.Ready && o.waiter == null)) { Doing(a, "espera platos en el pase"); Idle(a, PassPoint(a)); return; }
            Doing(a, "recoge los platos del pase");
            if (!Reach(a, PassPoint(a), _pass)) return;
            foreach (Order o in _orders)
            {
                if (o.state != OrderState.Ready || o.waiter != null || a.carry.Count >= maxPlates) continue;
                o.state = OrderState.Carrying; o.waiter = a; o.waiterName = a.go.name;
                a.carry.Add(o);
            }
            StackPlates(a);
            Debug.Log($"[Restaurante] «{a.go.name}» recoge {a.carry.Count} plato(s) del pase: {string.Join(", ", a.carry.ConvertAll(o => $"#{o.number} → {o.table.Name}"))}.");
            return;
        }
        Order c = a.carry[0];
        Doing(a, $"lleva #{c.number} {c.dish.name} a la {c.table.Name}");
        if (!Reach(a, ChairFront(c.table, c.diner.chair), c.table.mesa)) return;
        Vector3 place = PlacePoint(c.table, c.diner.chair);
        c.plate.SetParent(null, true);
        c.plate.SetPositionAndRotation(place, c.table.mesa.rotation);
        c.state = OrderState.Served; c.servedAt = Time.time;
        c.plateOffset = Flat(c.plate.position - place);
        c.servedTable = NearestTable(c.plate.position)?.Name;
        a.carry.RemoveAt(0);
        StackPlates(a);
        Debug.Log($"[Restaurante] «{a.go.name}» sirve el #{c.number} ({c.dish.name}) al turista {c.diner.number} en la {c.table.Name}.");
    }

    void StackPlates(Actor a)
    {
        for (int i = 0; i < a.carry.Count; i++)
        {
            Transform p = a.carry[i].plate; if (p == null) continue;
            p.SetParent(a.go.transform, false);
            p.localPosition = new Vector3((i - (a.carry.Count - 1) * 0.5f) * 0.32f, 0.05f, 0.45f);
            p.localRotation = Quaternion.identity;
        }
    }

    // ── Obradores ───────────────────────────────────────────────────────────────────────────────────────────────────

    public const string Masas = "masas (panadería y pasta)", Dairy = "lácteos y fermentos";
    static readonly HashSet<string> MasasEq = new HashSet<string> { "molino de piedra", "cámara de fermentación", "horno mixto", "amasadora espiral", "laminadora de pasta", "extrusora y secadero" };
    static readonly HashSet<string> DairyEq = new HashSet<string> { "yogurtera (incubadora)", "cuba de quesería", "cava de maduración", "prensa de tofu", "incubadora de tempeh", "barricas de fermentación" };

    /// <summary>De qué obrador es una elaboración (null: es de la línea, no de un obrador).</summary>
    public static string FamilyOf(Gastronomy.Prep p)
    {
        if (MasasEq.Contains(p.equipment)) return Masas;
        if (DairyEq.Contains(p.equipment) || p.name.StartsWith("leche") || p.name.StartsWith("nata")) return Dairy;
        return null;
    }

    void TickObrador(Actor a)
    {
        if (a.prep == null)
        {
            if (Time.time >= a.restUntil) { a.prep = ChoosePrep(a); a.restUntil = Time.time + 3f; }
            if (a.prep == null) { Doing(a, "revisa el obrador"); Idle(a, ObradorHome(a)); return; }
            a.station = PrepStation(a.prep, a.go.transform.position);
            a.until = -1f;
            Debug.Log($"[Restaurante] «{a.go.name}» ({a.family}) se pone con «{a.prep.name}» en {(a.station != null ? $"{a.station.parent.name}/{a.station.name}" : "?")}.");
        }
        Doing(a, $"elabora {a.prep.name} por mayor ({a.prep.portions} raciones)");
        if (!Reach(a, a.station != null ? Front(a.station, 0.85f) : ObradorHome(a), a.station)) return;
        if (!Act(a, prepSeconds)) return;
        Gastronomy.Prep p = a.prep;
        a.prep = null; a.restUntil = Time.time + 2f;
        if (_larder == null || !InputsFree(p) || !_larder.StartPrep(p.name, a.go.name)) { Debug.Log($"[Restaurante] «{a.go.name}»: ya no hay con qué hacer «{p.name}»."); return; }
        bool workshop = a.station != null && InFranja(a.station);
        _preps.Add(new PrepLog { who = a.go.name, prep = p.name, equipment = p.equipment, portions = p.portions, at = Time.time, workshop = workshop,
                                 station = a.station != null ? a.station.name : "?", room = a.station != null && a.station.parent != null ? a.station.parent.name : "?" });
    }

    Gastronomy.Prep ChoosePrep(Actor a)
    {
        if (_larder == null) return null;
        var cands = new List<Gastronomy.Prep>(_larder.Needed(40));   // primero lo que la carta necesita
        foreach (Gastronomy.Prep p in Gastronomy.Preps) if (!cands.Contains(p)) cands.Add(p);   // y si no, adelantar para mañana
        foreach (Gastronomy.Prep p in cands)
        {
            if (FamilyOf(p) != a.family || _larder.Have(p.name) >= 6f) continue;
            if (_larder.running.Exists(b => b.prep == p) || _crew.Exists(x => x != a && x.prep == p)) continue;
            if (!InputsFree(p) || PrepStation(p, a.go.transform.position) == null) continue;
            return p;
        }
        return null;
    }

    // Lo que no está apartado para una comanda que aún no ha salido de la despensa.
    bool InputsFree(Gastronomy.Prep p)
    {
        foreach (string inp in p.inputs) if (_larder.Have(inp) - Reserved(inp) < 1f) return false;
        return true;
    }

    float Reserved(string item)
    {
        float n = 0f;
        foreach (Order o in _orders)
            if ((o.state == OrderState.Pending || o.state == OrderState.Cooking) && o.taken.Count == 0 && System.Array.IndexOf(o.dish.components, item) >= 0) n++;
        return n;
    }

    // El equipo de la elaboración en los obradores de los bordes (el pan, en el horno de pisos); si no hay, el de un puesto.
    Transform PrepStation(Gastronomy.Prep p, Vector3 near)
    {
        string n = p.equipment == "horno mixto" ? "Horno de pisos" : "Equipo_" + p.equipment;
        Transform best = null; float bd = float.MaxValue;
        foreach (Transform f in transform)
        {
            if (!f.name.StartsWith("Franja_")) continue;
            Transform t = Nearest(f, n, near, null);
            if (t != null && (t.position - near).sqrMagnitude < bd) { bd = (t.position - near).sqrMagnitude; best = t; }
        }
        return best != null ? best : Nearest(transform, "Equipo_" + p.equipment, near, null);
    }

    bool InFranja(Transform t) { for (Transform x = t; x != null && x != transform; x = x.parent) if (x.name.StartsWith("Franja_")) return true; return false; }

    Vector3 ObradorHome(Actor a)
    {
        Transform room = VirtualInPlace.FindDeep(transform, a.family == Masas ? "Panaderia_1" : "Queseria_1");
        return room != null ? Front(room, 1.4f) + (room.right * (a.index % 2 == 0 ? -0.8f : 0.8f)) : PassPoint(a);
    }

    // ── Limpieza ────────────────────────────────────────────────────────────────────────────────────────────────────

    void TickCleanDining(Actor a)
    {
        if (a.table == null)
        {
            a.table = _tables.Find(t => t.dirty && t.occupant == null && t.cleaner == null);
            if (a.table != null) { a.table.cleaner = a; a.until = -1f; }
        }
        if (a.table == null) { Doing(a, "repasa el comedor"); Idle(a, DiningIdle(a)); return; }
        Table tb = a.table;
        Doing(a, $"recoge y limpia la {tb.Name}");
        if (!Reach(a, ChairFront(tb, tb.lastChair), tb.mesa)) return;
        if (!Act(a, cleanSeconds)) return;
        if (tb.leftover != null) Destroy(tb.leftover.gameObject);
        tb.leftover = null; tb.dirty = false; tb.cleaner = null; tb.cleanedAt = Time.time; tb.cleanedBy = a.go.name;
        _cleans.Add(new CleanLog { who = a.go.name, what = tb.Name, table = true, at = Time.time });
        Debug.Log($"[Restaurante] «{a.go.name}» recoge el plato y limpia la {tb.Name}.");
        a.table = null;
    }

    void TickCleanKitchen(Actor a)
    {
        if (a.cleaning == null && _dirtyPuestos.Count > 0) { a.cleaning = _dirtyPuestos[0]; _dirtyPuestos.RemoveAt(0); a.until = -1f; }
        if (a.cleaning == null) { Doing(a, "lava en la línea de lavado"); Idle(a, PostOf(a)); return; }
        Transform sink = Child(a.cleaning, "Fregadero");
        Doing(a, $"friega el {a.cleaning.name}");
        if (!Reach(a, sink != null ? Front(sink, 0.85f) : a.cleaning.position, sink)) return;
        if (!Act(a, cleanSeconds)) return;
        _cleans.Add(new CleanLog { who = a.go.name, what = a.cleaning.name, table = false, at = Time.time });
        Debug.Log($"[Restaurante] «{a.go.name}» friega y deja limpio el {a.cleaning.name}.");
        a.cleaning = null;
    }

    Vector3 DiningIdle(Actor a)
    {
        if (_caja != null) return Front(_caja, 1.2f) + _caja.right * (1.2f + a.index * 1.2f);
        return _ringPad != null ? _ringPad.transform.position + _ringPad.transform.forward * 3f : PassPoint(a);
    }

    // ── Turistas ────────────────────────────────────────────────────────────────────────────────────────────────────

    static readonly string[] TouristBodies = { "Panterilia", "Irosene", "Lizmibet", "JefeConstruccion", "Sadhguru", "Goluis", "Andrea" };

    /// <summary>Llega un turista al pie de la torre del restaurante (una copia de cuerpo, como en la virtualización).</summary>
    public Diner SpawnTourist()
    {
        if (!Locate() || _arrivalPad == null) return null;
        int n = ++_dinerN;
        GameObject g = WorkdayDirector.SpawnBodyCopy(TouristBodies[(n - 1) % TouristBodies.Length], $"Turista_{n}");
        Transform pt = _arrivalPad.transform;
        Vector3 at = pt.position + pt.forward * 3.2f + pt.right * ((n % 5) - 2) * 1.1f;   // junto a la pista de llegada
        g.transform.position = PathMover.Of(g.transform).OnGround(at);
        var d = new Diner { number = n, go = g, tag = g.AddComponent<ServiceActor>(), state = DinerState.Arriving, spawnedAt = Time.time };
        d.tag.role = "Turista"; d.mv = new Mover(g.transform, d.tag);
        _diners.Add(d);
        DinerDoing(d, "llega al restaurante");
        Debug.Log($"[Restaurante] Llega el turista {n} («{g.name}») a «{_arrivalPad.displayName}».");
        return d;
    }

    void TickDiner(Diner d)
    {
        if (d.state == DinerState.Gone) return;
        if (d.go == null) { Gone(d); return; }
        switch (d.state)
        {
            case DinerState.Arriving:
                if (d.table == null)
                {
                    d.table = ClaimTable(d);
                    if (d.table == null) { DinerDoing(d, "espera una mesa libre"); d.tag.moving = false; return; }
                    d.chair = ChairFor(d.table);
                    Debug.Log($"[Restaurante] El turista {d.number} va a la {d.table.Name}.");
                }
                DinerDoing(d, $"va a la {d.table.Name}");
                if (!d.mv.Go(ChairFront(d.table, d.chair), dinerSpeed, legTimeout, d.table.Name)) return;
                Sit(d);
                d.seatedAt = Time.time; d.state = DinerState.Seated; d.table.uses++;
                PlaceOrder(d);
                return;
            case DinerState.Seated:
                if (d.order == null || d.order.state == OrderState.Cancelled) { Leave(d, "se va sin comer"); return; }
                if (d.order.state == OrderState.Served)
                {
                    d.state = DinerState.Eating; d.until = Time.time + eatSeconds;
                    DinerDoing(d, $"come {d.order.dish.name}");
                    Debug.Log($"[Restaurante] El turista {d.number} recibe en su mesa su {d.order.dish.name} y empieza a comer.");
                    return;
                }
                if (Time.time - d.order.orderedAt > waitTimeout) { Cancel(d.order, "el turista se cansó de esperar"); Leave(d, "se cansa de esperar y se va"); }
                return;
            case DinerState.Eating:
                Transform food = d.order.plate != null ? d.order.plate.Find("Comida") : null;
                if (food != null) food.localScale = Vector3.Lerp(new Vector3(0.14f, 0.025f, 0.12f), new Vector3(0.03f, 0.01f, 0.03f), 1f - Mathf.Clamp01((d.until - Time.time) / eatSeconds));
                if (Time.time < d.until) return;
                d.order.state = OrderState.Eaten; d.order.eatenAt = Time.time;
                d.table.leftover = d.order.plate; d.order.plate = null;
                Leave(d, "termina y se va");
                return;
            case DinerState.Leaving:
                DinerDoing(d, "se va del restaurante");
                if (!d.mv.Go(_arrivalPad.transform.position, dinerSpeed, legTimeout, "la salida")) return;
                Debug.Log($"[Restaurante] El turista {d.number} se va del restaurante.");
                Destroy(d.go); Gone(d);
                return;
        }
    }

    void Gone(Diner d) { d.state = DinerState.Gone; d.goneAt = Time.time; if (d.tag != null) d.tag.moving = false; if (d.table != null && d.table.occupant == d) d.table.occupant = null; }

    void Leave(Diner d, string why)
    {
        StandUp(d);
        if (d.table != null)
        {
            if (d.table.occupant == d) d.table.occupant = null;
            d.table.lastChair = d.chair; d.table.leftAt = Time.time;
            d.table.dirty = d.table.dirty || d.table.leftover != null || d.order != null;
        }
        d.leftAt = Time.time; d.state = DinerState.Leaving;
        Debug.Log($"[Restaurante] El turista {d.number} {why}{(d.table != null ? $" (deja la {d.table.Name} para limpiar)" : "")}.");
    }

    Table ClaimTable(Diner d)
    {
        Table best = null; float bd = float.MaxValue;
        Vector3 from = _ringPad != null ? _ringPad.transform.position : d.go.transform.position;
        foreach (Table t in _tables)
        {
            if (t.occupant != null || t.dirty || t.cleaner != null) continue;
            float s = (t.mesa.position - from).sqrMagnitude;
            if (s < bd) { bd = s; best = t; }
        }
        if (best != null) best.occupant = d;
        return best;
    }

    int ChairFor(Table t)
    {
        Vector3 from = _ringPad != null ? _ringPad.transform.position : t.mesa.position;
        int best = 0; float bd = float.MaxValue;
        for (int i = 0; i < t.chairs.Length; i++) { float s = (ChairFront(t, i) - from).sqrMagnitude; if (s < bd) { bd = s; best = i; } }
        return best;
    }

    void PlaceOrder(Diner d)
    {
        Gastronomy.Dish dish = PickDish(d.number);
        if (dish == null) { Debug.Log($"[Restaurante] El turista {d.number}: no queda nada de la carta en la despensa."); return; }
        var o = new Order { number = ++_orderN, diner = d, table = d.table, dish = dish, orderedAt = Time.time, state = OrderState.Pending };
        _orders.Add(o); d.order = o;
        DinerDoing(d, $"ha pedido {dish.name} · espera");
        Debug.Log($"[Restaurante] El turista {d.number}, sentado en la {d.table.Name}, pide «{dish.name}» ({dish.origin}), una ración: la comanda #{o.number} llega al riel del pase.");
    }

    Gastronomy.Dish PickDish(int n)
    {
        // Cada turista elige a su gusto (rota por la carta), y entre lo que hay, lo que menos se ha pedido hoy.
        Gastronomy.Dish[] menu = Gastronomy.Menu;
        Gastronomy.Dish best = null; int fewest = int.MaxValue;
        for (int k = 0; k < menu.Length; k++)
        {
            Gastronomy.Dish d = menu[(n * 7 + k) % menu.Length];
            if (carta.Count > 0 && !carta.Contains(d.name)) continue;
            if (!Servable(d)) continue;
            int asked = _orders.FindAll(o => o.dish == d).Count;
            if (asked < fewest) { fewest = asked; best = d; }
        }
        return best;
    }

    bool Servable(Gastronomy.Dish d)
    {
        if (_larder == null) return false;
        foreach (string c in d.components) if (_larder.Have(c) - Reserved(c) < 1f) return false;
        return true;
    }

    void Cancel(Order o, string why)
    {
        if (o == null || o.state == OrderState.Cancelled) return;
        o.state = OrderState.Cancelled; o.why = why;
        if (o.cook != null && o.cook.order == o) { o.cook.order = null; DropHeld(o.cook); }
        if (o.waiter != null) o.waiter.carry.Remove(o);
        if (o.plate != null) Destroy(o.plate.gameObject);
        Debug.Log($"[Restaurante] Comanda #{o.number} anulada: {why}.");
    }

    // Sentado en la silla: la pelvis sobre el asiento (a 0,45 m) mirando a la mesa (como en la virtualización del servicio).
    void Sit(Diner d)
    {
        Transform body = d.go.transform.Find(CubeHumanoid.BodyName);
        Transform chair = d.table.chairs[d.chair];
        Vector3 look = d.table.mesa.position - chair.position; look.y = 0f;
        float off = body != null ? d.go.transform.position.y - body.position.y : 1f, s = body != null ? body.lossyScale.y : 1f;
        if (body != null) ClonePose.Apply(body, DailyPoses.Sit());
        float seatTop = chair.position.y + chair.lossyScale.y * 0.5f;
        Vector3 feet = new Vector3(chair.position.x, seatTop - 0.38f * s, chair.position.z);
        d.go.transform.SetPositionAndRotation(feet + Vector3.up * off, look.sqrMagnitude > 1e-4f ? Quaternion.LookRotation(look) : d.go.transform.rotation);
        DinerDoing(d, "se sienta");
    }

    void StandUp(Diner d)
    {
        if (d.go == null || d.table == null || d.seatedAt < 0f) return;
        Transform body = d.go.transform.Find(CubeHumanoid.BodyName);
        if (body != null) ClonePose.Rest(body);
        d.go.transform.position = PathMover.Of(d.go.transform).OnGround(ChairFront(d.table, d.chair));
    }

    // ── Sitios ──────────────────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>Delante de un equipo o mueble (su −Z local es el frente: puertas, mandos, la encimera), a ras de suelo.</summary>
    public static Vector3 Front(Transform t, float dist) { Vector3 p = t.position - t.forward * dist; p.y = t.position.y; return p; }

    /// <summary>Por fuera de la silla <paramref name="chair"/>: donde se pone quien se sienta, sirve o limpia ese cubierto.</summary>
    public static Vector3 ChairFront(Table t, int chair)
    {
        Transform c = t.chairs[Mathf.Clamp(chair, 0, t.chairs.Length - 1)];
        Vector3 o = c.position - t.mesa.position; o.y = 0f; o = o.sqrMagnitude > 1e-4f ? o.normalized : t.mesa.forward;
        Vector3 p = c.position + o * 0.55f; p.y = t.mesa.position.y;
        return p;
    }

    /// <summary>Donde va el plato servido: en el mantel, delante del cubierto de esa silla.</summary>
    public static Vector3 PlacePoint(Table t, int chair)
    {
        Transform c = t.chairs[Mathf.Clamp(chair, 0, t.chairs.Length - 1)];
        Vector3 o = c.position - t.mesa.position; o.y = 0f; o = o.sqrMagnitude > 1e-4f ? o.normalized : t.mesa.forward;
        return t.mesa.position + o * 0.42f + Vector3.up * 0.81f;
    }

    public Table NearestTable(Vector3 p)
    {
        Table best = null; float bd = float.MaxValue;
        foreach (Table t in _tables) { float s = (t.mesa.position - p).sqrMagnitude; if (s < bd) { bd = s; best = t; } }
        return best;
    }

    // El lado del pase de cada uno (fijo: el de su puesto, o el de las alfombras de la planta para el camarero), repartidos.
    Vector3 PassPoint(Actor a)
    {
        Vector3 from = a.puesto != null ? a.puesto.position : _floorPad != null ? _floorPad.transform.position : _pass.position + _pass.forward;
        float side = Vector3.Dot(from - _pass.position, _pass.forward) >= 0f ? 1f : -1f;
        float along = Mathf.Clamp((a.role == Role.Cocinero ? a.index - 1 : 0) * 0.9f, -2f, 2f);
        Vector3 p = _pass.position + _pass.forward * side * 1.0f + _pass.right * along; p.y = _pass.position.y;
        return p;
    }

    static Transform Nearest(Transform root, string name, Vector3 near, Transform within)
    {
        Transform best = null; float bd = float.MaxValue;
        foreach (Transform t in root.GetComponentsInChildren<Transform>())
        {
            if (t.name != name || (within != null && !t.IsChildOf(within))) continue;
            float s = (t.position - near).sqrMagnitude;
            if (s < bd) { bd = s; best = t; }
        }
        return best;
    }

    static float Flat(Vector3 v) { v.y = 0f; return v.magnitude; }

    // ── Andar, actuar, llevar ───────────────────────────────────────────────────────────────────────────────────────

    bool Reach(Actor a, Vector3 at, Transform face)
    {
        if (!a.mv.Go(at, staffSpeed, legTimeout, face != null ? face.name : "su sitio")) { a.until = -1f; return false; }
        if (face != null) Face(a.go.transform, face.position);
        return true;
    }

    void Idle(Actor a, Vector3 at) { a.until = -1f; a.mv.Go(at, staffSpeed * 0.8f, legTimeout, "su sitio"); }

    bool Act(Actor a, float seconds)
    {
        if (a.until < 0f) a.until = Time.time + seconds;
        Gesture(a);
        if (Time.time < a.until) return false;
        a.until = -1f;
        return true;
    }

    static void Face(Transform t, Vector3 p) { Vector3 d = p - t.position; d.y = 0f; if (d.sqrMagnitude > 1e-4f) t.rotation = Quaternion.LookRotation(d); }

    // Brazos trabajando (remover, amasar, fregar) en quien tiene esqueleto de anima.
    static void Gesture(Actor a)
    {
        CreatureRig rig = a.go.GetComponent<CreatureRig>(); if (rig == null) return;
        float t = Time.time * 6f;
        Transform sr = rig.Get(BodyPart.ShoulderRight), fr = rig.Get(BodyPart.ForearmRight), sl = rig.Get(BodyPart.ShoulderLeft);
        if (sr != null) { rig.LockPose(BodyPart.ShoulderRight, 0.2f); sr.localRotation = Quaternion.Euler(-55f + 12f * Mathf.Sin(t), 0f, 12f * Mathf.Cos(t)); }
        if (fr != null) { rig.LockPose(BodyPart.ForearmRight, 0.2f); fr.localRotation = Quaternion.Euler(-30f, 0f, 0f); }
        if (sl != null) { rig.LockPose(BodyPart.ShoulderLeft, 0.2f); sl.localRotation = Quaternion.Euler(-25f, 0f, 0f); }
    }

    void Hold(Actor a, Transform item, bool plate = false)
    {
        DropHeld(a);
        a.held = item;
        item.SetParent(a.go.transform, false);
        item.localPosition = plate ? new Vector3(0f, 0.05f, 0.45f) : new Vector3(0.25f, 0.05f, 0.4f);
        item.localRotation = Quaternion.identity;
    }

    static void DropHeld(Actor a)
    {
        if (a.held == null) return;
        bool isPlate = a.order != null && a.held == a.order.plate;
        if (!isPlate) Destroy(a.held.gameObject);
        a.held = null;
    }

    // ── Etiquetas ───────────────────────────────────────────────────────────────────────────────────────────────────

    readonly Dictionary<GameObject, string> _baseLabel = new Dictionary<GameObject, string>();

    void Doing(Actor a, string what)
    {
        if (a.tag != null) a.tag.doing = what;
        Label(a.go, $"{RoleName(a.role)}: {what}");
    }

    void DinerDoing(Diner d, string what) { if (d.tag != null) d.tag.doing = what; Label(d.go, what); }

    void Label(GameObject g, string what)
    {
        if (g == null || !g.TryGetComponent(out WorldLabel wl)) return;
        if (!_baseLabel.TryGetValue(g, out string b))
        {
            b = wl.text; int cut = b.IndexOf("\n<"); if (cut >= 0) b = b.Substring(0, cut);   // sin el «<lo que hace>» de la jornada
            _baseLabel[g] = b;
        }
        wl.text = $"{b}\n<{what}>";
    }

    void RestoreLabel(GameObject g)
    {
        if (g != null && _baseLabel.TryGetValue(g, out string b) && g.TryGetComponent(out WorldLabel wl)) wl.text = b;
        if (g != null) _baseLabel.Remove(g);
    }

    /// <summary>Vacía el servicio: se van los turistas, se recogen los platos y la brigada queda libre (las pruebas, al acabar).</summary>
    public void ResetService()
    {
        foreach (Diner d in _diners) if (d.go != null) Destroy(d.go);
        foreach (Order o in _orders) if (o.plate != null) Destroy(o.plate.gameObject);
        foreach (Table t in _tables) { if (t.leftover != null) Destroy(t.leftover.gameObject); t.leftover = null; t.dirty = false; t.occupant = null; t.cleaner = null; t.uses = 0; t.leftAt = t.cleanedAt = -1f; t.cleanedBy = null; }
        for (int i = _crew.Count - 1; i >= 0; i--) { _crew[i].order = null; _crew[i].carry.Clear(); Release(_crew[i]); }
        _diners.Clear(); _orders.Clear(); _preps.Clear(); _cleans.Clear(); _dirtyPuestos.Clear();
    }

    /// <summary>
    /// ANDAR A UN DESTINO por el NavMesh y las alfombras (<see cref="PathMover"/>), contando cada destino nuevo como un TRAMO
    /// (<see cref="ServiceActor.leg"/>). Llega cuando está a menos de 0,4 m en planta y en el mismo piso; si un tramo pasa de
    /// su tope, se da por atascado (<see cref="ServiceActor.stuck"/>) y sigue con lo siguiente.
    /// </summary>
    public class Mover
    {
        readonly Transform _t; readonly ServiceActor _tag;
        Vector3 _goal = new Vector3(float.NaN, 0f, 0f);
        float _since; bool _gaveUp;
        public Mover(Transform t, ServiceActor tag) { _t = t; _tag = tag; }

        public bool Go(Vector3 goal, float speed, float timeout, string what)
        {
            if (_t == null) return true;
            PathMover pm = PathMover.Of(_t);
            if (float.IsNaN(_goal.x) || (goal - _goal).sqrMagnitude > 0.25f) { _goal = goal; _since = Time.time; _gaveUp = false; _tag.leg++; pm.Restart(); }
            if (_gaveUp) { _tag.moving = false; return true; }
            float left = pm.StepToward(goal, speed);
            bool riding = FlyingCarpet.Riding(_t.gameObject);
            bool level = Mathf.Abs(_t.position.y - pm.PivotHeight - goal.y) < 1.3f;
            if (!riding && level && left < 0.4f) { _tag.moving = false; return true; }
            _tag.moving = true;
            if (Time.time - _since > timeout)
            {
                _gaveUp = true; _tag.stuck++; _tag.moving = false;
                Debug.Log($"[Restaurante] «{_t.name}» no llega a {what} en {timeout:0} s (se queda a {left:0.0} m, Δy {(_t.position.y - pm.PivotHeight - goal.y):0.0} m): sigue con lo suyo.");
                return true;
            }
            return false;
        }
    }
}
