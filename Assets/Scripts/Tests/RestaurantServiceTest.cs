using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TEST EN VIVO DEL SERVICIO DEL RESTAURANTE (Berón 2026-10-29: «probar en vivo el restaurante con los trabajadores acordados
/// —cocineros, limpieza y obradores— y los turistas que piden su comida y la reciben en su mesa»). En la escena construida:
///  1. pone de turno la BRIGADA ACORDADA (docs/restaurant-plan.md §5: 3 cocineros de línea, 1 en el pase que sube los platos,
///     2 obradores —masas; lácteos y fermentos— y 2 de limpieza —cocina y lavado; comedor—) con un plan de la JORNADA
///     (<see cref="WorkdayDirector.today"/>: misiones coc_servicio, coc_atender, coc_obrador, coc_limpieza y coc_comedor), y el
///     <see cref="RestaurantService"/> la toma de <see cref="WorkdayDirector.DoingNow"/> como en el juego;
///  2. llegan <see cref="tourists"/> turistas al pie de la torre y el servicio corre solo: mesa, comanda, cocina, pase, mesa,
///     comer, irse, limpiar; los obradores elaboran por mayor mientras;
///  3. mide CADA FOTOGRAMA a todos los que se mueven: saltos (teletransportes) y tramos a pie (vueltas), y comprueba mesa, plato,
///     tiempos, raciones individuales, equipo de cada plato, limpieza y obradores.
/// Se añade solo al orquestador cuando se piden las pruebas.
/// </summary>
public class RestaurantServiceTest : MonoBehaviour, ITestUnit
{
    public int Group => 45;   // tras la historia del origen de la cocina (44)
    public bool ParallelSafe => false;
    public float speed = 2f;   // a ×3, con el editor a pocos FPS (Δt tope 0,33 s) un paso normal ya pasaba de 1,5 m
    public int tourists = 5;
    [Tooltip("Tope (s de juego) desde que pide hasta que tiene el plato en su mesa.")]
    public float serveBound = 300f;
    [Tooltip("Tope (s de juego) de todo el servicio.")]
    public float serviceBound = 900f;
    public float maxJump = 1.5f, circling = 1.6f, circlingSlack = 3f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();   // solo está activo si se pidieron las pruebas
        if (tr != null && tr.GetComponentInChildren<RestaurantServiceTest>() == null) tr.gameObject.AddComponent<RestaurantServiceTest>();
    }

    // La brigada acordada: (misión de la jornada, quién). Goluis, el jefe, en la línea; Gohageneis, en el pase.
    static readonly string[] Missions = { "coc_servicio", "coc_atender", "coc_servicio", "coc_servicio", "coc_obrador", "coc_obrador", "coc_limpieza", "coc_comedor" };
    static readonly string[] People = { "Goluis", "Gohageneis", "Panterilia", "Irosene", "Lizmibet", "JefeConstruccion", "Sadhguru", "Andrea" };

    class Track
    {
        public string name; public Vector3 prev; public bool prevRiding, seg; public int segLeg;
        public Vector3 segStart, segEnd; public float walked, still, maxJump; public string jumpInfo;
        public readonly List<string> circles = new List<string>(); public readonly List<string> stalls = new List<string>();
        public float worst = 0f; public string worstInfo = "";
    }

    readonly Dictionary<GameObject, Track> _tracks = new Dictionary<GameObject, Track>();
    readonly List<(Behaviour b, bool was)> _frozen = new List<(Behaviour, bool)>();
    readonly List<GameObject> _spawned = new List<GameObject>();

    public IEnumerator Run()
    {
        TestProbe.Begin("Servicio del restaurante en vivo: brigada acordada y turistas servidos en su mesa");
        WorkdayDirector wd = WorkdayDirector.Instance;
        KitchenLarder larder = null;
        foreach (KitchenLarder l in FindObjectsByType<KitchenLarder>(FindObjectsSortMode.None)) if (l.transform.parent != null && l.transform.parent.GetComponent<RestaurantKitchen>() != null) larder = l;
        RestaurantService svc = RestaurantService.Instance;
        if (svc == null && larder != null) svc = larder.gameObject.AddComponent<RestaurantService>();
        TimeController tc = TimeController.timeController;
        if (wd == null || larder == null || svc == null || tc == null)
        { TestProbe.Check("hay jornada, despensa y servicio del restaurante en la escena", false, $"jornada {wd != null} · despensa {larder != null} · servicio {svc != null}"); TestProbe.End(); yield break; }
        if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld) { MobWorldLoader.Instance.ExitMobWorld(); yield return Until(() => !MobWorldLoader.Instance.IsInMobWorld && !MobWorldLoader.Instance.IsBusy, 20f); }
        RestaurantService.AllowDuringTests = true;
        svc.ResetService();
        yield return null;
        svc.RefreshCrewNow();
        TestProbe.Check("el restaurante tiene pase, comedor en el anillo con mesas para todos y alfombras de llegada",
            svc.Pass != null && svc.Tables.Count >= tourists && svc.ArrivalPad != null && svc.RingPad != null,
            $"{svc.Tables.Count} mesas · llegada «{svc.ArrivalPad?.displayName}» · comedor «{svc.RingPad?.displayName}»");
        if (svc.Pass == null || svc.Tables.Count < tourists || svc.ArrivalPad == null) { RestaurantService.AllowDuringTests = false; TestProbe.End(); yield break; }

        // ── 1. El TURNO de la brigada acordada, por la jornada ──
        WorkRoster.DayPlan oldToday = wd.today; int oldPlanned = wd.plannedDay;
        int oldDay = tc.Day; float oldHour = tc.Hour, oldScale = Time.timeScale;
        tc.SetClock(oldDay, 12f);   // 12:00, segundo bloque de trabajo
        var plan = new WorkRoster.DayPlan { day = tc.Day };
        var crewGos = new List<GameObject>();
        for (int i = 0; i < Missions.Length; i++)
        {
            GameObject g = WorkdayDirector.Body(People[i]);
            string who = People[i];
            if (g == null || g.GetComponent<Anima>() == null)   // falta esa persona: la Magnate se divide (como en la jornada)
            {
                who = $"Magnate (brigada {i + 1})";
                g = WorkdayDirector.SpawnMagnateBody(who); g.name = who; _spawned.Add(g);
            }
            crewGos.Add(g);
            Freeze(g);
            WorkRoster.Mission m = null;
            foreach (WorkRoster.Mission c in AreaWork.Catalog) if (c.id == Missions[i]) { m = c.Copy(); break; }
            m.hours = 8f;   // el turno cubre toda la prueba
            plan.work.Add(new WorkRoster.Assignment { worker = new WorkRoster.Worker(who), mission = m, start = 12f });
        }
        wd.today = plan; wd.plannedDay = tc.Day;
        svc.RefreshCrewNow();
        int cooks = 0, waiters = 0, obr = 0, cleanK = 0, cleanD = 0; var fams = new HashSet<string>(); var puestos = new HashSet<string>();
        foreach (RestaurantService.Actor a in svc.Crew)
            switch (a.role)
            {
                case RestaurantService.Role.Cocinero: cooks++; if (a.puesto != null) puestos.Add(a.puesto.name); break;
                case RestaurantService.Role.Camarero: waiters++; break;
                case RestaurantService.Role.Obrador: obr++; fams.Add(a.family); break;
                case RestaurantService.Role.LimpiezaCocina: cleanK++; break;
                default: cleanD++; break;
            }
        TestProbe.Check("la jornada pone de turno la brigada acordada (3 cocineros en su puesto, 1 en el pase, 2 obradores, 2 de limpieza)",
            cooks == 3 && puestos.Count == 3 && waiters == 1 && obr == 2 && fams.Count == 2 && cleanK == 1 && cleanD == 1,
            string.Join(" · ", CrewText(svc)));
        // Al arrancar la partida, la Magnate EVALÚA a cada uno y lo pone en su área inicial (SanctuaryDirector.AssessArrival,
        // un teletransporte de hasta 200 m): se espera a que acabe con la brigada antes de empezar la jornada.
        yield return Until(() => crewGos.TrueForAll(g => g == null || !g.TryGetComponent(out WorldCharacter wc) || !wc.isNewArrival), 30f);
        SanctuaryDirector sd = SanctuaryDirector.Instance;
        System.Action<WorldCharacter, SanctuaryArea> replaced = (wc, area) =>
        {
            foreach (RestaurantService.Actor a in svc.Crew)
                if (a.go == wc.gameObject)
                {
                    WorkdayDirector.Teleport(a.go.transform, PathMover.Of(a.go.transform).OnGround(svc.PostOf(a)));
                    if (_tracks.TryGetValue(a.go, out Track tr)) { tr.prev = a.go.transform.position; if (tr.seg) Close(tr); }
                    Debug.Log($"[Restaurante] La evaluación de la Magnate movió a «{a.go.name}» a {area.displayName}: vuelve a su puesto (arranque, no cuenta como salto).");
                }
        };
        if (sd != null) sd.OnCharacterPlaced += replaced;
        // Arranque de la jornada: cada uno aparece en su sitio (como WorkdayDirector.PlaceAtStart).
        // Quien venga volando en alfombra (de su área de antes) baja primero: el vuelo lo lleva cada fotograma y pisaría el arranque.
        yield return Until(() => crewGos.TrueForAll(g => g == null || !FlyingCarpet.Riding(g)), 40f);
        var posts = new Dictionary<GameObject, Vector3>();
        foreach (RestaurantService.Actor a in svc.Crew)
        {
            Vector3 post = svc.PostOf(a);   // en el punto andable de su puesto (el que buscará al andar)
            if (UnityEngine.AI.NavMesh.SamplePosition(post, out UnityEngine.AI.NavMeshHit nh, 1.2f, UnityEngine.AI.NavMesh.AllAreas) && Mathf.Abs(nh.position.y - post.y) < 0.8f) post = nh.position;
            Vector3 p = PathMover.Of(a.go.transform).OnGround(post); posts[a.go] = p; WorkdayDirector.Teleport(a.go.transform, p);
        }
        WorkdayDirector.LogTeleports = true;
        for (int k = 0; k < 3; k++) yield return null;
        foreach (var kv in posts)   // ¿se quedó cada uno en su sitio? (si algo lo devuelve a otra parte, que se vea quién)
            if (kv.Key != null && (kv.Key.transform.position - kv.Value).magnitude > 1f)
                Debug.Log($"[Restaurante] «{kv.Key.name}» no se queda en su puesto: está en {kv.Key.transform.position}, puesto {kv.Value}.");

        // ── 2. La despensa: la carta del día con su mise en place, y género para los obradores ──
        var carta = new List<Gastronomy.Dish>(); var eqs = new HashSet<string>();
        foreach (Gastronomy.Dish d in Gastronomy.Menu) if (carta.Count < tourists && eqs.Add(d.equipment)) carta.Add(d);
        svc.carta.Clear();
        foreach (Gastronomy.Dish d in carta) { svc.carta.Add(d.name); foreach (string c in d.components) if (larder.Have(c) < 3f) larder.Add(c, 3f - larder.Have(c)); }
        foreach (var (item, n) in new[] { ("trigo", 4f), ("trigo duro", 2f), ("agua", 10f), ("sal", 6f), ("levadura", 3f), ("leche (sin animales)", 4f), ("cuajo microbiano", 3f), ("cultivos lácticos", 3f), ("soja", 3f), ("rhizopus", 2f) })
            if (larder.Have(item) < n) larder.Add(item, n - larder.Have(item));
        int servedBefore = larder.dishesServed;

        // ── 3. El servicio: llegan los turistas y todo corre solo; se mide cada fotograma ──
        Time.timeScale = speed; WorkdayDirector.LogTeleports = true;
        float t0 = Time.time, end = t0 + serviceBound, nextSpawn = t0 + 1f;
        int spawned = 0;
        foreach (GameObject g in crewGos) StartTrack(g);
        while (Time.time < end)
        {
            if (spawned < tourists && Time.time >= nextSpawn)
            {
                RestaurantService.Diner d = svc.SpawnTourist();
                if (d != null) { spawned++; StartTrack(d.go); }
                nextSpawn = Time.time + 4f;
            }
            Sample(); Frames(svc);
            if (spawned == tourists && Done(svc)) break;
            yield return null;
        }
        Time.timeScale = oldScale; WorkdayDirector.LogTeleports = false;
        foreach (Track tr in _tracks.Values) if (tr.seg) Close(tr);
        float took = Time.time - t0;

        // ── 4. Comprobaciones ──
        var diners = new List<RestaurantService.Diner>(svc.Diners);
        var orders = new List<RestaurantService.Order>(svc.Orders);
        string Name(RestaurantService.Role r) { foreach (RestaurantService.Actor a in svc.Crew) if (a.role == r) return a.go.name; return "?"; }
        var cookNames = new HashSet<string>(); foreach (RestaurantService.Actor a in svc.Crew) if (a.role == RestaurantService.Role.Cocinero) cookNames.Add(a.go.name);
        string waiter = Name(RestaurantService.Role.Camarero), diningCleaner = Name(RestaurantService.Role.LimpiezaSala), kitchenCleaner = Name(RestaurantService.Role.LimpiezaCocina);

        TestProbe.Check($"llegan {tourists} turistas al pie de la torre", spawned == tourists && diners.Count == tourists, $"{spawned} en {took:0} s de juego");
        var tables = new HashSet<string>(); bool seated = true; var seatTxt = new List<string>();
        foreach (RestaurantService.Diner d in diners)
        {
            seated &= d.seatedAt > 0f && d.table != null && tables.Add(d.table.Name);
            seatTxt.Add($"T{d.number}→{(d.table != null ? d.table.Name : "sin mesa")}{(d.seatedAt > 0f ? $" ({d.seatedAt - d.spawnedAt:0} s)" : " (no se sentó)")}");
        }
        TestProbe.Check("cada turista se sienta en una mesa DISTINTA del comedor", seated && tables.Count == tourists, string.Join(" · ", seatTxt));

        bool served = true; var servTxt = new List<string>();
        foreach (RestaurantService.Diner d in diners)
        {
            RestaurantService.Order o = d.order;
            bool ok = o != null && o.servedAt > 0f && o.table == d.table && o.servedTable == d.table?.Name && o.plateOffset >= 0f && o.plateOffset < 0.3f
                      && o.servedAt - o.orderedAt <= serveBound && (o.state == RestaurantService.OrderState.Eaten);
            served &= ok;
            servTxt.Add(o == null ? $"T{d.number}: sin comanda" : $"T{d.number} pidió «{o.dish.name}» → {(o.servedAt > 0f ? $"servido en {o.servedTable} a {o.servedAt - o.orderedAt:0} s" : $"no servido ({o.state}{(o.why != null ? ": " + o.why : "")})")}");
        }
        TestProbe.Check($"cada turista recibe EN SU MESA el plato que pidió en ≤ {serveBound:0} s y se lo come", served, string.Join(" · ", servTxt));

        bool gone = true; foreach (RestaurantService.Diner d in diners) gone &= d.state == RestaurantService.DinerState.Gone && d.leftAt > 0f && d.order != null && d.order.state == RestaurantService.OrderState.Eaten;
        TestProbe.Check("cada turista termina, se levanta y se va por las alfombras", gone, string.Join(" · ", diners.ConvertAll(d => $"T{d.number}: {d.state}")));

        bool single = orders.Count == tourists; var singleTxt = new List<string>();
        foreach (RestaurantService.Order o in orders)
        {
            bool ok = o.recipe != null && o.recipe.servings == 1 && !o.recipe.IsBulk && o.taken.Count == o.dish.components.Length;
            foreach (string c in o.dish.components) ok &= o.taken.TryGetValue(c, out float t) && Mathf.Approximately(t, o.recipe != null ? o.recipe.Amount(c) : -1f);
            single &= ok;
            if (!ok) singleTxt.Add($"#{o.number} {o.dish.name}: {o.taken.Count}/{o.dish.components.Length} componentes, ×{o.recipe?.servings}");
        }
        int servedNow = larder.dishesServed - servedBefore;
        TestProbe.Check("cada plato sale de la despensa en ración INDIVIDUAL (−1 ración de cada componente, receta ×1)", single && servedNow == orders.Count,
            singleTxt.Count > 0 ? string.Join(" · ", singleTxt) : $"{orders.Count} comandas · despensa: {servedNow} raciones emplatadas · p. ej. {(orders.Count > 0 ? Taken(orders[0]) : "")}");

        bool stations = orders.Count > 0; var stTxt = new List<string>(); var usedPuestos = new HashSet<string>();
        foreach (RestaurantService.Order o in orders)
        {
            bool ok = o.cookName != null && cookNames.Contains(o.cookName) && o.station != null && o.stationName == "Equipo_" + o.dish.equipment
                      && o.station.parent != null && o.station.parent.name == o.puesto && o.stationDistance >= 0f && o.stationDistance < 1.8f;
            stations &= ok;
            if (o.puesto != null) usedPuestos.Add(o.puesto);
            stTxt.Add($"#{o.number} {o.dish.name}: {o.cookName} en {o.puesto}/{o.stationName} a {o.stationDistance:0.0} m");
        }
        TestProbe.Check("cada plato lo cocina un cocinero de la brigada en el equipo de ESE plato de su propio puesto", stations, string.Join(" · ", stTxt));

        bool carried = orders.Count > 0; foreach (RestaurantService.Order o in orders) carried &= o.waiterName == waiter;
        TestProbe.Check("el del pase sube cada plato a la mesa del turista", carried, $"camarero {waiter}");

        bool cleaned = true; var clTxt = new List<string>();
        foreach (RestaurantService.Diner d in diners)
        {
            RestaurantService.Table t = d.table; if (t == null) { cleaned = false; continue; }
            bool ok = t.cleanedAt > t.leftAt && t.leftAt > 0f && t.cleanedBy == diningCleaner && !t.dirty && t.leftover == null;
            cleaned &= ok;
            clTxt.Add($"{t.Name}: {(ok ? $"limpia por {t.cleanedBy} a {t.cleanedAt - t.leftAt:0} s" : "sin limpiar")}");
        }
        TestProbe.Check("la limpieza del comedor recoge y limpia cada mesa que dejan los turistas", cleaned, string.Join(" · ", clTxt));

        bool scrubbed = usedPuestos.Count > 0;
        foreach (string p in usedPuestos) { bool any = false; foreach (RestaurantService.CleanLog c in svc.Cleans) any |= !c.table && c.what == p && c.who == kitchenCleaner; scrubbed &= any; }
        TestProbe.Check("la limpieza de cocina friega cada puesto en el que se cocinó", scrubbed, $"{kitchenCleaner}: {string.Join(", ", usedPuestos)}");

        bool baked = true; var obTxt = new List<string>();
        foreach (RestaurantService.Actor a in svc.Crew)
        {
            if (a.role != RestaurantService.Role.Obrador) continue;
            var mine = new List<string>(); bool bulk = false;
            foreach (RestaurantService.PrepLog p in svc.Preps) if (p.who == a.go.name) { mine.Add($"{p.prep} ×{p.portions} en {p.room}"); bulk |= p.portions > 1 && p.workshop; }
            baked &= bulk;
            obTxt.Add($"{a.go.name} ({a.family}): {(mine.Count > 0 ? string.Join(", ", mine) : "nada")}");
        }
        TestProbe.Check("los obradores elaboran POR MAYOR en sus obradores durante el servicio", baked, string.Join(" · ", obTxt));

        float worstJump = 0f; string jumpTxt = "";
        foreach (Track tr in _tracks.Values) if (tr.maxJump > worstJump) { worstJump = tr.maxJump; jumpTxt = $"{tr.name}: {tr.jumpInfo}"; }
        TestProbe.Check($"nadie se teletransporta (≤ {maxJump} m entre fotogramas, sin contar el vuelo en alfombra)", worstJump <= maxJump, $"peor {worstJump:0.00} m · {jumpTxt}");

        var circ = new List<string>(); float worstRatio = 0f; string worstTxt = "";
        foreach (Track tr in _tracks.Values) { circ.AddRange(tr.circles); if (tr.worst > worstRatio) { worstRatio = tr.worst; worstTxt = $"{tr.name}: {tr.worstInfo}"; } }
        TestProbe.Check($"nadie da vueltas: cada tramo a pie ≤ {circling} × la recta + {circlingSlack} m", circ.Count == 0,
            circ.Count > 0 ? string.Join(" · ", circ.GetRange(0, Mathf.Min(6, circ.Count))) : $"el tramo más largo respecto a su recta: {worstTxt}");

        int stuck = 0; var stTxt2 = new List<string>();
        foreach (var kv in _tracks) { ServiceActor tag = kv.Key != null ? kv.Key.GetComponent<ServiceActor>() : null; if (tag != null && tag.stuck > 0) { stuck += tag.stuck; stTxt2.Add($"{kv.Value.name} ×{tag.stuck}"); } stTxt2.AddRange(kv.Value.stalls); }
        TestProbe.Check("nadie se queda atascado (ningún tramo pasa de su tope ni se queda quieto andando > 20 s)", stuck == 0 && stTxt2.Count == 0, stTxt2.Count > 0 ? string.Join(" · ", stTxt2) : $"{_tracks.Count} personas seguidas");

        // ── 5. Dejarlo todo como estaba ──
        if (sd != null) sd.OnCharacterPlaced -= replaced;
        wd.today = oldToday; wd.plannedDay = oldPlanned;
        svc.ResetService(); svc.carta.Clear(); RestaurantService.AllowDuringTests = false;
        tc.SetClock(oldDay, oldHour);
        foreach (var (b, was) in _frozen) if (b != null) b.enabled = was;
        _frozen.Clear();
        foreach (GameObject g in _spawned) if (g != null) Destroy(g);
        _spawned.Clear(); _tracks.Clear();
        TestProbe.End();
    }

    static string Taken(RestaurantService.Order o)
    {
        var l = new List<string>(); foreach (var kv in o.taken) l.Add($"{kv.Key} −{kv.Value:0.##}");
        return $"#{o.number} {o.dish.name}: {string.Join(", ", l)}";
    }

    static List<string> CrewText(RestaurantService svc)
    {
        var l = new List<string>();
        foreach (RestaurantService.Actor a in svc.Crew) l.Add($"{a.go.name}={a.role}{(a.puesto != null ? $"@{a.puesto.name}" : a.family != null ? $"({a.family})" : "")}");
        return l;
    }

    // Terminado: todos los turistas se fueron, ninguna comanda en marcha, las mesas limpias, los puestos fregados y cada
    // obrador con al menos una tanda.
    static bool Done(RestaurantService svc)
    {
        foreach (RestaurantService.Diner d in svc.Diners) if (d.state != RestaurantService.DinerState.Gone) return false;
        foreach (RestaurantService.Order o in svc.Orders) if (o.state != RestaurantService.OrderState.Eaten && o.state != RestaurantService.OrderState.Cancelled) return false;
        foreach (RestaurantService.Table t in svc.Tables) if (t.dirty) return false;
        foreach (RestaurantService.Actor a in svc.Crew)
        {
            if (a.role == RestaurantService.Role.LimpiezaCocina && a.cleaning != null) return false;
            if (a.role != RestaurantService.Role.Obrador) continue;
            bool any = false; foreach (RestaurantService.PrepLog p in svc.Preps) any |= p.who == a.go.name;
            if (!any) return false;
        }
        foreach (RestaurantService.Order o in svc.Orders)
        {
            bool scrubbed = false; foreach (RestaurantService.CleanLog c in svc.Cleans) scrubbed |= !c.table && c.what == o.puesto;
            if (o.puesto != null && !scrubbed) return false;
        }
        return true;
    }

    // Que nada más lo mueva mientras trabaja: merodear, las necesidades del cuerpo (va a comer AQUÍ luego), el jefe de cocina…
    void Freeze(GameObject g)
    {
        foreach (Behaviour b in g.GetComponents<Behaviour>())
            if (b is IdleBehavior || b is NeedsDriver || b is KitchenBoss || b is Linduarda || b is CarnivoreKeeper || b is NewcomerInitiator || b is UnityEngine.AI.NavMeshAgent)
            { _frozen.Add((b, b.enabled)); b.enabled = false; }
        if (g.TryGetComponent(out ActionQueue q)) { q.ClearOwnForTest(); q.ClearPlayer(); }
        if (g.TryGetComponent(out Anima a) && a.asleep) { a.asleep = false; DailyPoses.Stand().ApplyFinal(a); }
    }

    void StartTrack(GameObject g)
    {
        if (g == null || _tracks.ContainsKey(g)) return;
        _tracks[g] = new Track { name = g.name, prev = g.transform.position, prevRiding = FlyingCarpet.Riding(g) };
    }

    // Cada fotograma: salto desde el anterior (sin el vuelo) y el tramo a pie en curso.
    void Sample()
    {
        foreach (var kv in _tracks)
        {
            GameObject g = kv.Key; Track tr = kv.Value;
            if (g == null) { if (tr.seg) Close(tr); continue; }
            Vector3 p = g.transform.position;
            bool riding = FlyingCarpet.Riding(g);
            ServiceActor tag = g.GetComponent<ServiceActor>();
            bool moving = tag != null && tag.moving;
            int leg = tag != null ? tag.leg : 0;
            if (!riding && !tr.prevRiding)
            {
                float j = (p - tr.prev).magnitude;
                if (j > maxJump) Debug.Log($"[Restaurante] SALTO de «{g.name}»: {j:0.00} m de ({tr.prev.x:0.00},{tr.prev.y:0.00},{tr.prev.z:0.00}) a ({p.x:0.00},{p.y:0.00},{p.z:0.00}) haciendo «{tag?.doing}» (andando {moving}, tramo {leg}).");
                if (j > tr.maxJump) { tr.maxJump = j; tr.jumpInfo = $"{j:0.00} m en ({p.x:0.0},{p.y:0.0},{p.z:0.0}) haciendo «{tag?.doing}»"; }
            }
            float step = Flat(p - tr.prev);
            if (tr.seg)
            {
                if (leg == tr.segLeg && !riding && !tr.prevRiding) { tr.walked += step; tr.segEnd = p; }
                if (leg != tr.segLeg || !moving || riding) Close(tr);
            }
            if (!tr.seg && moving && !riding && !tr.prevRiding) { tr.seg = true; tr.segLeg = leg; tr.segStart = tr.prev; tr.segEnd = p; tr.walked = step; }
            // Quieto mientras anda (sin alfombra): atascado contra algo.
            if (moving && !riding && step < 0.003f)
            {
                tr.still += Time.deltaTime;
                if (tr.still > 20f) { tr.stalls.Add($"{tr.name} quieto 20 s yendo a «{tag?.doing}» en ({p.x:0.0},{p.y:0.0},{p.z:0.0})"); tr.still = -1e6f; }
            }
            else if (tr.still > 0f || !moving) tr.still = 0f;
            tr.prev = p; tr.prevRiding = riding;
        }
    }

    void Close(Track tr)
    {
        tr.seg = false;
        float straight = Flat(tr.segEnd - tr.segStart);
        if (tr.walked < 0.5f) return;
        // LO MÍNIMO QUE HAY QUE ANDAR: la recta, o el camino más corto del NavMesh si los muebles obligan a rodear (cada puesto
        // de la cocina es una U de 7 × 6,5 m con la entrada delante: de un fregadero al del puesto de al lado hay 9 m en recta
        // pero ~19 m saliendo por una entrada y entrando por la otra). Dar vueltas = andar mucho más que ESO.
        float least = straight; string via = "";
        var path = new UnityEngine.AI.NavMeshPath();
        if (UnityEngine.AI.NavMesh.SamplePosition(tr.segStart, out UnityEngine.AI.NavMeshHit a, 1.5f, UnityEngine.AI.NavMesh.AllAreas)
            && UnityEngine.AI.NavMesh.SamplePosition(tr.segEnd, out UnityEngine.AI.NavMeshHit b, 1.5f, UnityEngine.AI.NavMesh.AllAreas)
            && UnityEngine.AI.NavMesh.CalculatePath(a.position, b.position, UnityEngine.AI.NavMesh.AllAreas, path) && path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
        {
            float len = 0f; for (int i = 1; i < path.corners.Length; i++) len += Flat(path.corners[i] - path.corners[i - 1]);
            if (len > least) { least = len; via = $" ({len:0.0} m por el camino más corto del NavMesh)"; }
        }
        float ratio = tr.walked / (circling * least + circlingSlack);
        string info = $"tramo {tr.segLeg}: {tr.walked:0.0} m andados para {straight:0.0} m en recta{via}";
        if (ratio > tr.worst) { tr.worst = ratio; tr.worstInfo = info; }
        if (ratio > 1f) tr.circles.Add($"{tr.name} {info}");
    }

    static float Flat(Vector3 v) { v.y = 0f; return v.magnitude; }

    // ── FOTOGRAMAS (Temp/restaurante_*.png en el proyecto): una cámara propia, para no tocar la del jugador ──
    readonly HashSet<string> _shot = new HashSet<string>();
    public readonly List<string> frames = new List<string>();

    void Frames(RestaurantService svc)
    {
        foreach (RestaurantService.Diner d in svc.Diners)
            if (d.state == RestaurantService.DinerState.Seated && !d.settling && d.go != null) { Shot("1_turista_sentado_pide", d.go.transform, d.go.transform.position); break; }
        foreach (RestaurantService.Order o in svc.Orders)
        {
            if (o.state == RestaurantService.OrderState.Cooking && o.stationDistance >= 0f && o.cook != null && o.cook.go != null) Shot("2_cocinero_en_el_equipo_del_plato", o.cook.go.transform, o.station.position);
            if (o.state == RestaurantService.OrderState.Ready && o.plate != null) Shot("3_plato_en_el_pase", svc.Pass, o.plate.position);
            if (o.state == RestaurantService.OrderState.Served && o.plate != null && o.diner?.go != null) Shot("4_plato_servido_en_su_mesa", o.diner.go.transform, o.plate.position);
        }
        foreach (RestaurantService.Actor a in svc.Crew)
        {
            if (a.go == null || a.tag == null || a.tag.moving || a.until < 0f) continue;
            if (a.role == RestaurantService.Role.Obrador && a.prep != null) Shot($"5_obrador_{(a.family == RestaurantService.Masas ? "masas" : "lacteos")}", a.go.transform, a.station != null ? a.station.position : a.go.transform.position);
            if (a.role == RestaurantService.Role.LimpiezaSala && a.table != null) Shot("6_limpia_la_mesa", a.go.transform, a.table.mesa.position);
            if (a.role == RestaurantService.Role.LimpiezaCocina && a.cleaning != null) Shot("7_friega_el_puesto", a.go.transform, a.go.transform.position);
        }
    }

    void Shot(string tag, Transform who, Vector3 focus)
    {
        if (!_shot.Add(tag)) return;
        var go = new GameObject("CamaraPrueba");
        Camera cam = go.AddComponent<Camera>();
        Vector3 back = who != null ? -who.forward : Vector3.back; back.y = 0f; back = back.sqrMagnitude > 0.01f ? back.normalized : Vector3.back;
        Vector3 side = Vector3.Cross(Vector3.up, back);
        cam.transform.position = focus + back * 3.2f + side * 1.6f + Vector3.up * 2.4f;
        cam.transform.LookAt(focus + Vector3.up * 0.6f);
        cam.fieldOfView = 55f; cam.nearClipPlane = 0.1f;
        var rt = new RenderTexture(1280, 720, 24);
        cam.targetTexture = rt; cam.Render();
        RenderTexture prev = RenderTexture.active; RenderTexture.active = rt;
        var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply();
        RenderTexture.active = prev; cam.targetTexture = null;
        string path = $"Temp/restaurante_{tag}.png";
        try { System.IO.File.WriteAllBytes(path, tex.EncodeToPNG()); frames.Add(path); Debug.Log($"[Restaurante] fotograma: {path}"); }
        catch (System.Exception e) { Debug.Log($"[Restaurante] no se pudo guardar {path}: {e.Message}"); }
        Destroy(tex); Destroy(rt); Destroy(go);
    }

    static IEnumerator Until(System.Func<bool> cond, float seconds)
    {
        float end = Time.time + seconds;
        while (!cond() && Time.time < end) yield return null;
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pruebas/Servicio del restaurante (sola)")]
    static void Solo()
    {
        if (!Application.isPlaying) { Debug.Log("[Solo] no está en Play"); return; }
        var go = new GameObject("SoloServicioRestaurante"); DontDestroyOnLoad(go);
        RestaurantServiceTest t = go.AddComponent<RestaurantServiceTest>();
        t.StartCoroutine(Wrap(t));
    }

    static IEnumerator Wrap(RestaurantServiceTest t)
    {
        TestProbe.ResetTotals();
        yield return t.StartCoroutine(t.Run());
        TestProbe.Total();
        Debug.Log("[Solo] listo");
    }
#endif
}
