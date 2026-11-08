using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// LA JORNADA EN EL JUEGO (docs/jornada-y-rotacion.md). Cada día a las 8:00 reparte el trabajo con <see cref="WorkRoster"/>
/// entre los miembros del santuario y lo hace cumplir hasta las 16:00:
///   8–9 entrenamiento · 9–11 tareas · 11–12 comida (en el restaurante) · 12–14 tareas · 14–15 entrenamiento (yoga, gimnasio…) · 15–16 tareas · LIBRE.
/// Durante esas 8 h los personajes van al área de su misión y se quedan allí (bloqueados); después merodean a su aire.
/// Si una misión es NUEVA para alguien, la Magnate aparece a su lado y lo inicia (la mímica: ella lanza los hechizos y el
/// otro los repite). Si faltan personas para las obligatorias, la Magnate se DIVIDE: sus clones van a esos puestos y
/// desaparecen al terminar. Las obras avanzan con sus misiones (material + trabajo) y, terminadas, abren sus
/// entrenamientos. Consola: <c>jornada</c> (plan y equilibrio).
/// </summary>
public class WorkdayDirector : MonoBehaviour
{
    public static WorkdayDirector Instance { get; private set; }

    public static readonly string[] Staff = { "Player", "Linduarda", "Lizmibet", "Goluis", "Panterilia", "Gohageneis", "Irosene", "JefeConstruccion", "Andrea",
                               "Nesvitor", "Amykhuz", "Karen", "Alicita", "Ducristin", "Josedrilo", "Maite", "Shalva", "Adaguchi",   // familia y amigos de Berón (docs/diseno-personajes.md)
                               "Gallarle", "Julian", "Fausto",   // (2026-11-04) parejas: de Ducristin (Gallarle), de Alicita (Julian) y de Irosene (Fausto); pendientes de foto
                               "David", "Adonis", "Yulo", "Lale", "Ivelyna", "Luciana", "Malena",   // (2026-11-04) seis nuevos y Malena (2026-11-05): informática, biotecnología, pediatría y piloto
                               "Consol", "Fernando" };   // (2026-11-05) Consol, maestra de Ashtanga, y Fernando, paneles solares (docs/personajes/)
    /// <summary>(2026-11-04) La lista del personal, también sin escena: la usan la regla de capacidad (AreaCapacity) y el
    /// constructor del Aula (cuántas plazas y cuántos pisos).</summary>
    [Tooltip("Personal del santuario en su orden FIJO (el de la rotación). El jugador entra cuando ya es miembro.")]
    public string[] staff = (string[])Staff.Clone();
    // (2026-11-04, Berón: «los personajes famosos … sácalos del mesocosmos y crea sus microcosmos respectivos») Sadhguru y los seis
    // del mesocosmos (Elon Musk, Goku, Bill Gates, Warren Buffett, MacKenzie Scott, Ibrahim Traoré) ya no son personal del santuario:
    // viven en sus microcosmos. Sus papeles los llevan otros (docs/personajes-mesocosmos.md §Reparto de papeles).
    public float walkSpeed = 2.6f;

    public readonly WorkRoster roster = new WorkRoster();
    public WorkRoster.DayPlan today;
    public int plannedDay = int.MinValue;
    readonly HashSet<WorkRoster.Assignment> _done = new HashSet<WorkRoster.Assignment>();
    readonly HashSet<WorkRoster.Assignment> _started = new HashSet<WorkRoster.Assignment>();
    readonly Dictionary<WorkRoster.Worker, GameObject> _clones = new Dictionary<WorkRoster.Worker, GameObject>();
    readonly List<(GameObject go, float until)> _initiators = new List<(GameObject, float)>();
    public int initiations { get; private set; }

    static TimeController TC => TimeController.timeController;

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }

    void Start()
    {
        if (KitchenLarder.Instance != null) KitchenLarder.Instance.autoPlan = false;   // la cocina produce por misiones desde el primer día
        foreach (SanctuaryArea a in FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None))
            if (a.GetComponent<ConstructionProject>() == null && !IsTraining(a.areaType) && !roster.areas.Contains(a.displayName)) roster.areas.Add(a.displayName);
        roster.areas.Sort(System.StringComparer.Ordinal);   // orden base estable (día 0)
        if (!roster.areas.Contains(AreaWork.Guard)) roster.areas.Add(AreaWork.Guard);
        if (AreaTransform(AreaWork.GuardRoom) != null && !roster.areas.Contains(AreaWork.GuardRoom)) roster.areas.Add(AreaWork.GuardRoom);
        // LO PRIMERO al empezar: la jornada y cada uno ya en su área (sin pasar unos segundos en su sitio antiguo).
        if (TC != null && !ColdSanctuary.TestRunner.Running && FindAnyObjectByType<ColdSanctuary.TestRunner>() == null) { float h = TC.Hour; if (h >= WorkRoster.StartHour && h < WorkRoster.StartHour + 8f) { PlanDay(TC.Day); Tick(h); } }
    }

    static bool IsTraining(SanctuaryAreaType t) => t == SanctuaryAreaType.Gym || t == SanctuaryAreaType.Ballet || t == SanctuaryAreaType.MartialArts || t == SanctuaryAreaType.Calisthenics;

    /// <summary>Entrenamientos abiertos: yoga, gimnasio y las obras de entrenamiento ya terminadas.</summary>
    public List<string> OpenTraining()
    {
        var l = new List<string>();
        foreach (SanctuaryArea a in FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None))
        {
            bool training = a.areaType == SanctuaryAreaType.YogaRoom || IsTraining(a.areaType);
            ConstructionProject cp = a.GetComponent<ConstructionProject>();
            if (training && (cp == null || cp.Finished)) l.Add(a.displayName);
        }
        l.Sort(System.StringComparer.Ordinal);
        return l;
    }

    /// <summary>El GameObject de un miembro del personal (en escena algunos se llaman «Goluis_Post»…).</summary>
    static readonly Dictionary<string, GameObject> _bodies = new Dictionary<string, GameObject>();
    static readonly Dictionary<string, float> _missing = new Dictionary<string, float>();
    /// <summary>Ids de antes de un cambio de nombre (id nuevo → id viejo), para las escenas sin reconstruir.</summary>
    public static readonly System.Collections.Generic.Dictionary<string, string> LegacyIds = new System.Collections.Generic.Dictionary<string, string> { { "Amykhuz", "Amahuz" }, { "Lale", "Ale" }, { "Yulo", "Julito" } };

    /// <summary>El GameObject de un miembro del personal (en escena algunos se llaman «Goluis_Post»…). Cacheado: buscar por nombre cada frame era lo más caro de la jornada.</summary>
    public static GameObject Body(string n)
    {
        if (_bodies.TryGetValue(n, out GameObject g) && g != null) return g;
        // Quien no está en la escena no se vuelve a buscar en un rato: GameObject.Find recorre todos los objetos.
        if (!ColdSanctuary.TestRunner.Running && _missing.TryGetValue(n, out float retry) && Time.unscaledTime < retry) return null;
        g = GameObject.Find(n) ?? GameObject.Find(n + "_Post");
        // (2026-11-04) NOMBRES ANTIGUOS: las escenas sin reconstruir guardan el id de antes (Amahuz → Amykhuz).
        if (g == null && LegacyIds.TryGetValue(n, out string old)) g = GameObject.Find(old) ?? GameObject.Find(old + "_Post");
        _bodies[n] = g;
        if (g == null) _missing[n] = Time.unscaledTime + 3f; else _missing.Remove(n);
        return g;
    }

    void SyncStaff()
    {
        foreach (string n in staff)
        {
            GameObject g = Body(n);
            if (g == null || roster.Find(n) != null) continue;
            if (!Magnate.IsMember(g.GetComponent<Anima>())) continue;   // aún sin iniciación: no trabaja (está con Linduarda)
            if (g.CompareTag("Player") && !PlayerWorkday.Ready(g)) continue;   // [Jornada de Kushal] solo tras TODA la iniciación (huerto y su microcosmos)
            roster.Add(n);
        }
    }

    /// <summary>Planifica el día <paramref name="day"/> (lo llama el reloj a las 8:00; los tests, directamente).</summary>
    public WorkRoster.DayPlan PlanDay(int day)
    {
        SyncStaff();
        if (KitchenLarder.Instance != null) KitchenLarder.Instance.autoPlan = false;   // en el santuario cada elaboración es una misión de alguien
        PopulationPlan.Humans = Mathf.Max(PopulationPlan.Humans, roster.workers.Count + 2);   // el personal + la Magnate + el recién llegado: la fauna se dimensiona con ellos
        roster.trainingAreas.Clear(); roster.trainingAreas.AddRange(OpenTraining());
        foreach (GameObject c in _clones.Values) if (c != null) Destroy(c);
        _clones.Clear(); _done.Clear(); _started.Clear();
        _lunched.Clear(); _lunchStocked = false; _heading.Clear();   // [Jornada] la comida de hoy
        ReleaseGym(); _coachDay = int.MinValue;   // las sesiones de gimnasio de ayer
        roster.initiationModel = true; roster.worldCheck = MissionRequirements.Blocked;   // la mañana nueva y los requisitos (docs/jornada-y-rotacion.md)
        List<WorkRoster.Mission> todays = AreaWork.Today(day, ConstructionProject.All);
        todays.AddRange(GymProgramming.Missions());   // la del entrenador (papel fijo: revisar el registro y ajustar los planes)
        today = roster.Plan(day, todays);
        _orders.Clear(); _cloneOrders.Clear();   // (las órdenes guardadas eran del plan de ayer)
        int gymDays = GymProgramming.ApplyGymDays(today, roster.trainingAreas, roster.workers);   // los días de gimnasio de cada plan
        if (gymDays > 0) Debug.Log($"[Gimnasio] Día {day}: {gymDays} cambios en el entrenamiento de las 14 h por los días de cada plan; al gimnasio: {string.Join(", ", today.training.Where(kv => kv.Value == GymArea).Select(kv => kv.Key.name))}.");
        plannedDay = day;
        foreach (WorkRoster.Worker c in today.clones) _clones[c] = SpawnMagnateBody(c.name);
        _taughtToday.Clear();
        foreach (WorkRoster.Lesson l in today.lessons) Debug.Log($"[Iniciación] {l}.");
        Debug.Log($"[Jornada] Día {day}: {today.work.Count} misiones para {roster.workers.Count} personas · orden de áreas: {string.Join(" → ", today.areaOrder)}" +
                  (today.clones.Count > 0 ? $" · la Magnate se divide en {today.clones.Count}" : "") + (today.backlog.Count > 0 ? $" · {today.backlog.Count} pendientes para mañana" : ""));
        foreach (string s in WorkRoster.Balance(today, roster.workers.Count)) Debug.Log("[Jornada] " + s);
        // (el reloj saltó la franja del desayuno —las pruebas lo ponen a las 7:57—: desayunan ahora, antes de la primera misión)
        if (_breakfastDay != day && TC != null && TC.Day == day && TC.Hour >= BreakfastHour && TC.Hour < WorkRoster.StartHour + 0.5f) Breakfast(day);
        return today;
    }

    // ── EL DESAYUNO (2026-11-02): de 7:15 a 7:50, en casa, antes de la jornada. Sin él, el hambre de la noche llegaba a las 8 y
    // el «pedir en la barra» (o el que quedó a medias de la tarde) se comía la primera misión: Lizmibet y Andrea, la hora de la
    // clase de yoga entera en el restaurante y la clase con 0 alumnos (JornadaTest día 2). Desayunados, el hambre vuelve hacia
    // las 11, la hora de comer; en turno solo interrumpe el hambre de verdad (NeedsDriver.starvingAbove), el baño y el sueño.
    public const float BreakfastHour = 7.25f, BreakfastEnd = 7.84f;
    int _breakfastDay = int.MinValue;

    /// <summary>El personal desayuna: sacia el hambre y deja la comida que tuviera a medias (ya ha comido).</summary>
    public void Breakfast(int day)
    {
        _breakfastDay = day;
        int n = 0;
        foreach (string who in staff)
        {
            GameObject g = Body(who);
            if (g == null || !g.TryGetComponent(out Anima a) || a.death) continue;
            if (g.TryGetComponent(out ActionQueue q))
            {
                foreach (QueuedSpell s in q.Own) if (s is DineSpell d) d.Abandon(a);
                if (q.RemoveOwnWhere(s => s is DineSpell || s is EatSpell) > 0 && !a.asleep) DailyPoses.Stand().ApplyFinal(a);
            }
            if (g.TryGetComponent(out BodyMetabolism b)) b.hunger = Mathf.Min(b.hunger, 0.05f);
            n++;
        }
        Debug.Log($"[Jornada] Día {day} · desayuno: {n} personas desayunan antes de la jornada.");
    }

    void Update()
    {
        if (TC == null || ColdSanctuary.TestRunner.Running) return;   // las pruebas usan a los personajes
        float h = TC.Hour; int day = TC.Day;
        if (_breakfastDay != day && h >= BreakfastHour && h < BreakfastEnd) Breakfast(day);
        if (plannedDay != day && h >= PlanFrom && h < WorkRoster.StartHour + 8f) PlanDay(day);
        if (today == null || plannedDay != day) return;
        Tick(h, true);
    }

    /// <summary>Hora a la que se reparte el día: al acabar el desayuno (7:50), para que cada uno salga de casa hacia su primera
    /// misión y llegue a las 8 (de un árbol a otro se tarda casi una hora de juego).</summary>
    public const float PlanFrom = BreakfastEnd;

    /// <summary>Avanza la jornada a la hora <paramref name="h"/>: empieza y termina misiones, mueve a cada uno a su sitio.</summary>
    /// <summary>Buscar las misiones de una persona es lo caro (LINQ sobre todo el día): en juego se rehace para 1/<see cref="DriveSlices"/> del
    /// personal por frame y el resto usa la orden guardada. <b>Conducirla (<see cref="Drive"/>) es en CADA frame</b>: anda, entrena y actúa con
    /// el Time.deltaTime de un frame, así que conducir 1 de cada 8 frames la hacía andar a 1/8 de la velocidad (la jornada entera llegaba tarde:
    /// nadie comía a su hora ni entrenaba, JornadaTest 32/15). Empezar y acabar misiones, también en cada frame.</summary>
    public const int DriveSlices = 8;
    public void Tick(float h) => Tick(h, false);
    /// <summary>El mismo paso que da el juego en Update, con el reparto: lo usa JornadaTest para probar la jornada tal como corre de verdad.</summary>
    public void TickLive(float h) => Tick(h, true);

    /// <summary>La orden guardada de una persona (dónde, qué y qué misión). <c>early</c>: es de antes de las 8.</summary>
    struct Order { public string where, doing, mid; public bool early; }
    readonly Dictionary<WorkRoster.Worker, Order> _orders = new Dictionary<WorkRoster.Worker, Order>(), _cloneOrders = new Dictionary<WorkRoster.Worker, Order>();

    void Tick(float h, bool sliced)
    {
        _hour = h;
        int slot = Time.frameCount % DriveSlices, turn = 0;
        bool Mine() => !sliced || turn++ % DriveSlices == slot;
        if (h < WorkRoster.StartHour)   // (7:50–8:00) desayunados: cada uno sale hacia su primera misión si está en otro árbol
        {
            // (sin colocarlos de golpe a las 8: ya van andando. PlaceAtStart es para cuando el juego EMPIEZA en plena jornada; con
            // el reparto a las 7:50 los teletransportaba 140–170 m a las 8:01 delante de todos — JornadaTest, batería completa)
            _placed = true;
            foreach (WorkRoster.Worker w in roster.workers)
            {
                bool fresh = Mine();
                GameObject g = Body(w.name);
                if (g == null || g.CompareTag("Player") || OnOwnDuty(g)) continue;
                if (fresh || !_orders.TryGetValue(w, out Order o) || !o.early)
                {
                    WorkRoster.Assignment first = HeadingEarly(w, g, h);
                    _orders[w] = o = new Order { where = first?.mission.area, doing = first?.mission.title, early = true };
                }
                if (o.where != null) Drive(g, o.where, o.doing, null);
            }
            return;
        }
        foreach (WorkRoster.Assignment a in today.work)
        {
            float end = a.start + a.mission.hours;
            if (h >= a.start && !_started.Contains(a)) { _started.Add(a); Begin(a); }
            if (h >= end && !_done.Contains(a)) { _done.Add(a); Finish(a); }
        }
        // Clones que ya no tienen trabajo pendiente: desaparecen.
        if (!sliced || slot == 0) foreach (var kv in _clones.ToList())
            if (kv.Value != null && today.work.Where(x => x.worker == kv.Key).All(x => _done.Contains(x))) { Destroy(kv.Value); _clones.Remove(kv.Key); Debug.Log($"[Jornada] {kv.Key.name} termina y se disuelve."); }
        for (int i = _initiators.Count - 1; i >= 0; i--) if (_initiators[i].go == null || h >= _initiators[i].until) { if (_initiators[i].go != null) Destroy(_initiators[i].go); _initiators.RemoveAt(i); }

        if (!_lunchStocked && h >= WorkRoster.StartHour + WorkRoster.FirstBlock) { _lunchStocked = true; StockLunch(); }   // [Jornada] la comida de las 11:00
        if (!_placed) PlaceAtStart(h);
        UpdateInitiator(h);
        foreach (WorkRoster.Worker w in roster.workers)
        {
            bool fresh = Mine();
            GameObject g = Body(w.name);
            if (g == null || g.CompareTag("Player") || OnOwnDuty(g)) continue;
            if (fresh || !_orders.TryGetValue(w, out Order o) || o.early)
            {
                string where = Where(w, h, out string doing);
                string mid = Current(w, h)?.mission.id;
                WorkRoster.Assignment early = HeadingEarly(w, g, h);
                if (early != null) { where = early.mission.area; doing = early.mission.title; mid = null; }   // ya va hacia lo siguiente (otra torre)
                _orders[w] = o = new Order { where = where, doing = doing, mid = mid };
            }
            Drive(g, o.where, o.doing, o.mid);
        }
        foreach (var kv in _clones)
        {
            if (kv.Value == null || OnOwnDuty(kv.Value)) continue;
            if (!sliced || slot == 1 || !_cloneOrders.TryGetValue(kv.Key, out Order o))
            {
                string where = Where(kv.Key, h, out string doing);
                string cm = Current(kv.Key, h)?.mission.id;
                _cloneOrders[kv.Key] = o = new Order { where = where, doing = doing, mid = FruitGardener.IsRound(cm) ? cm : null };
            }
            Drive(kv.Value, o.where, o.doing, o.mid, alwaysMove: true);
        }
        HoldPlayer(h);
    }

    // ARRANQUE: lo primero es la asignación de itinerarios; con ella cada uno aparece YA en el área de su primer turno
    // (como si el reloj empezara justo al comenzar la jornada). El jugador, recién llegado, sigue en la enfermería.
    bool _placed;
    void PlaceAtStart(float h)
    {
        _placed = true;
        int n = 0;
        foreach (WorkRoster.Worker w in roster.workers)
        {
            GameObject g = Body(w.name);
            if (g == null || g.CompareTag("Player")) continue;
            string area = Where(w, h, out _);
            if (area == null) continue;
            Vector3 p = FruitGardener.StartPoint(Current(w, h)?.mission.id) ?? WorkPoint(area, g.name); if (p == Vector3.zero) continue;   // la ronda de frutales empieza en su techo
            Teleport(g.transform, p); n++;
        }
        foreach (var kv in _clones) if (kv.Value != null) { string area = Where(kv.Key, h, out _); Vector3 p = area != null ? WorkPoint(area, kv.Key.name) : Vector3.zero; if (p != Vector3.zero) Teleport(kv.Value.transform, p); }
        Debug.Log($"[Jornada] Arranque: itinerarios asignados y {n} personajes colocados en el área de su primer turno.");
    }

    /// <summary>¿Está <paramref name="g"/> (Kushal o alguien del personal) en su jornada AHORA (8–16 h del día repartido)? Nadie
    /// la interrumpe: la Magnate ya no le da clases de upa-yoga a mitad de turno (JornadaTest 2026-10-29).</summary>
    public bool OnShift(GameObject g)
    {
        if (g == null || today == null || TC == null || plannedDay != TC.Day) return false;
        float h = TC.Hour;
        if (h < WorkRoster.StartHour || h >= WorkRoster.StartHour + 8f) return false;
        return roster.Find(g.CompareTag("Player") ? "Player" : g.name.Replace("_Post", "")) != null;
    }

    /// <summary>Diagnóstico de las pruebas: registra quién teletransporta a quién (con la pila).</summary>
    public static bool LogTeleports;
    public static void Teleport(Transform t, Vector3 p)
    {
        if (LogTeleports && t != null && (t.position - p).sqrMagnitude > 4f) Debug.Log($"[Teleport] «{t.name}» {(t.position - p).magnitude:0.0} m: {System.Environment.StackTrace.Replace(System.Environment.NewLine, " / ")}");
        UnityEngine.AI.NavMeshAgent ag = t.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (ag != null && ag.enabled && ag.isOnNavMesh) { ag.Warp(p); return; }
        CharacterController cc = t.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        t.position = p;
        if (cc != null) cc.enabled = true;
    }

    /// <summary>Quien hace ahora la misión <paramref name="missionId"/> (cuerpos de personas y clones de la Magnate).</summary>
    public List<GameObject> DoingNow(string missionId, float h)
    {
        var l = new List<GameObject>();
        if (today == null) return l;
        foreach (WorkRoster.Assignment a in today.work)
        {
            if (a.mission.id != missionId || h < a.start || h >= a.start + a.mission.hours) continue;
            GameObject g = a.worker.clone ? (_clones.TryGetValue(a.worker, out GameObject c) ? c : null) : Body(a.worker.name);
            if (g != null && !g.CompareTag("Player")) l.Add(g);
        }
        return l;
    }

    /// <summary>Quién trabaja en <paramref name="area"/>: primero los que están de turno allí AHORA, luego los que tienen
    /// misión allí hoy (los iniciadores de área: el de la sala de yoga, el de la cocina).</summary>
    public List<GameObject> InArea(string area)
    {
        var l = new List<GameObject>();
        if (today == null) return l;
        float h = TC != null ? TC.Hour : _hour;
        for (int pass = 0; pass < 2; pass++)
            foreach (WorkRoster.Assignment a in today.work)
            {
                if (a.mission.area != area) continue;
                bool now = h >= a.start && h < a.start + a.mission.hours;
                if (now != (pass == 0)) continue;
                GameObject g = a.worker.clone ? (_clones.TryGetValue(a.worker, out GameObject c) ? c : null) : Body(a.worker.name);
                if (g != null && !g.CompareTag("Player") && !l.Contains(g)) l.Add(g);
            }
        return l;
    }

    // INICIADOR: quien tenga ahora la misión de iniciación lanza «iniciar» sobre los recién llegados.
    void UpdateInitiator(float h)
    {
        if (NewcomerInitiator.Current != null && NewcomerInitiator.Current.InProgress) return;   // quien la empezó, la termina
        List<GameObject> who = DoingNow("enf_iniciacion", h);
        GameObject g = who.Count > 0 ? who[0] : null;
        // Si a esta hora nadie la tiene, la hace quien la tenga HOY (el hechizo compuesto lo conoce quien está de turno).
        if (g == null && today != null)
            foreach (WorkRoster.Assignment a in today.work)
                if (a.mission.id == "enf_iniciacion") { g = a.worker.clone ? (_clones.TryGetValue(a.worker, out GameObject c) ? c : null) : Body(a.worker.name); if (g != null && !g.CompareTag("Player")) break; g = null; }
        NewcomerInitiator.Current = g != null ? g.GetOrAdd<NewcomerInitiator>() : null;
    }

    // Personajes que su misión mueve por sí misma ahora (el iniciador con un recién llegado) o que la Magnate está
    // devolviendo a su sitio.
    static bool OnOwnDuty(GameObject g)
    {
        if (HospitalSubmarine.Engaged(g)) return true;   // va en el submarino-ambulancia o en el Leviatán (docs/submarinos-medicos.md)
        NewcomerInitiator ni = g.GetComponent<NewcomerInitiator>();
        if (ni != null && ni.Busy) return true;
        if (AreaInitiator.Leading(g)) return true;   // inicia a un recién llegado en su área (yoga, cocina): lo lleva él
        return MagnateWarden.Instance.Handling(g);
    }

    // NADIE ABANDONA SU PUESTO durante la jornada: si el jugador sale del área de lo que le toca (trabajo, comida o
    // entrenamiento), la Magnate se teletransporta delante de él y lo devuelve (MagnateWarden). No hay paredes: puede
    // intentarlo. La Guardia patrulla por todo el santuario: ahí no se le retiene.
    GameObject _player;
    public int playerHeldBack { get; private set; }

    void HoldPlayer(float h)
    {
        WorkRoster.Worker me = roster.Find("Player"); if (me == null) return;
        if (_player == null) _player = GameObject.FindGameObjectWithTag("Player");
        GameObject p = _player; if (p == null) return;
        if (NuclearSite.Evacuating) { _transitSince = -1f; return; }   // se está evacuando: nadie lo devuelve a un puesto del sitio
        if (AereoLink.PlayerThere) { _transitSince = -1f; return; }   // en el Santuario Aéreo: tiene su andén para volver
        if (MagnateWarden.Blocked(p)) { _transitSince = -1f; return; }   // fuera de su alcance: en alfombra, en el microcosmos…
        if (!MagnateWarden.GuardPosts) { _transitSince = -1f; return; }   // casilla de Inicio quitada: explora libre
        if (!InfirmaryGate.Cleared(p)) return;   // recién llegado: hasta terminar la iniciación su sitio es el hospital (InfirmaryGate)
        string area = Where(me, h, out string doing);
        WorkRoster.Mission postMission = Current(me, h)?.mission;
        WorkRoster.Assignment early = PlayerWorkday.HeadingEarly(h);   // ya hizo lo suyo y va a la siguiente: ese es su puesto
        if (early != null) { area = early.mission.area; doing = early.mission.title; postMission = early.mission; }
        if (area != _postArea)   // [Jornada de Kushal] puesto nuevo: el reloj del trayecto empieza, con lo que tarda el viaje (al Sitio Nuclear pasa de 120 s y la Magnate lo traía a medio camino)
        {
            _postArea = area; _postSince = Time.time;
            _postCommute = area != null ? Mathf.Max(PlayerWorkday.Commute, ArbolCampos.TripSeconds(p.transform.position, WorkPoint(area, "Player")) * 1.3f) : PlayerWorkday.Commute;
        }
        if (area == null || area == AreaWork.Guard) return;
        if (AreaTransform(area) == null || MagnateWarden.Instance.Handling(p)) return;
        if (Time.time - _postSince < _postCommute) return;   // [Jornada de Kushal] tiene un rato para llegar (andando / en alfombra)
        Vector3 pos = p.transform.position;
        if (InWorkspace(postMission, area, pos)) { _transitSince = -1f; return; }
        // Va a comer: la barra y las mesas están fuera del área («Comida» en Cocina), y la Magnate lo traía a la Cocina a media comida.
        if (doing == "Comida" && p.TryGetComponent(out ActionQueue pq) && pq.Own.Any(s => s is DineSpell)) { _transitSince = -1f; return; }
        // DE CAMINO (en un puente, en una pista, en alfombra, en una escalera): se le deja pasar un rato, que las tareas
        // de varias áreas obligan a cruzar; quedarse ahí no es trabajar.
        if (InTransit(p))
        {
            if (_transitSince < 0f) _transitSince = Time.time;
            if (Time.time - _transitSince < MaxTransit) return;
        }
        _transitSince = -1f;
        playerHeldBack++;
        MagnateWarden.Bring(p, AreaPoint(area, "Player"), $"Ahora te toca «{doing}» en {area}. Hasta las 16:00 nadie deja su puesto.");
    }

    float _transitSince = -1f;
    const float MaxTransit = 90f;
    string _postArea; float _postSince = -999f, _postCommute = PlayerWorkday.Commute;   // [Jornada de Kushal] su puesto actual y desde cuándo (PlayerWorkday.Commute)
    /// <summary>Desde cuándo (Time.time) el puesto de Kushal es el actual: el rato para llegar (PlayerWorkday.Commute) cuenta desde aquí.</summary>
    public float PlayerPostSince => _postSince;

    /// <summary>¿Está <paramref name="pos"/> en el espacio que su tarea necesita? El EDIFICIO de su área (todas las que se
    /// llaman así, con entreplantas, pisos de arriba aunque sean otra área, y sus pistas de alfombras), las áreas por las que la tarea hace ir, la obra en la que se trabaja, los
    /// huertos si la tarea es del Huerto, o cualquier área si la tarea recorre todas (reparar maquinaria).</summary>
    public static bool InWorkspace(WorkRoster.Mission m, string area, Vector3 pos)
    {
        if (InAreaNamed(area, pos)) return true;
        if (area == EnergyRule.Area && EnergySite.OnCap(pos)) return true;   // (los paneles, las torres y el almacén están en el casquete)
        if (m == null) return false;
        foreach (string x in m.also) if (InAreaNamed(x, pos)) return true;
        if (m.roams && SceneCache.All<SanctuaryArea>(2f).Any(a => a != null && a.InBuilding(pos))) return true;
        if (m.site != null)
            foreach (ConstructionProject cp in ConstructionProject.All)
                if (cp != null && cp.title == m.site && cp.Area != null && cp.Area.Contains(pos, 3f)) return true;
        return false;
    }

    // Todas las áreas con ese nombre (y, para el Huerto, sus parcelas y el bambusal, que están en el suelo).
    static bool InAreaNamed(string name, Vector3 pos)
    {
        if (AereoRule.IsArea(name)) { if (AereoRule.InSector(pos) || AereoLink.PlayerThere) return true; AereoLink l = AereoLink.Instance; if (l != null && (pos - l.transform.position).sqrMagnitude < 400f) return true; }
        // La sala de guardia es un sitio, no un área: nadie contaba como dentro aunque estuviera en ella (Gohageneis «parado fuera» ×123; a Kushal lo traería la Magnate).
        if (name == AreaWork.GuardRoom) { Transform gr = AreaTransform(name); if (gr == null) return false; Vector3 gl = gr.InverseTransformPoint(pos); return Mathf.Abs(gl.x) < 24f && Mathf.Abs(gl.z) < 24f && gl.y > -2f && gl.y < 7f; }
        if (name == SiteLine.AreaName) return SiteLine.Instance != null && (pos - SiteLine.Instance.transform.position).sqrMagnitude < 225f;   // la estación no es un área: a 15 m del andén
        foreach (SanctuaryArea a in SceneCache.All<SanctuaryArea>(2f))
            if (a != null && a.displayName == name && a.InBuilding(pos)) return true;
        if (name == "Huerto")
            foreach (GardenPlot g in SceneCache.All<GardenPlot>(2f))
            {
                if (g == null) continue;
                Vector3 l = g.transform.InverseTransformPoint(pos);
                if (Mathf.Abs(l.x) < g.size.x * 0.5f + 2f && Mathf.Abs(l.z) < g.size.y * 0.5f + 2f && l.y > -2f && l.y < 8f) return true;
            }
        return false;
    }

    // En un área de alfombras (esperando una) o volando.
    static bool InTransit(GameObject p)
    {
        if (FlyingCarpet.Riding(p)) return true;
        if (!Physics.Raycast(p.transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 4f, ~0, QueryTriggerInteraction.Ignore)) return false;
        for (Transform t = hit.transform; t != null; t = t.parent)
            if (t.name.StartsWith("Alfombras_")) return true;
        return false;
    }

    /// <summary>Dónde debe estar <paramref name="w"/> a la hora <paramref name="h"/> (área) y qué hace; null = libre.</summary>
    public string Where(WorkRoster.Worker w, float h, out string doing)
    {
        doing = null;
        if (today == null) return null;
        float s = WorkRoster.StartHour;
        if (h < s || h >= s + 8f) return null;   // fuera de la jornada: libre
        foreach (WorkRoster.Assignment a in today.work)
            if (a.worker == w && h >= a.start && h < a.start + a.mission.hours) { doing = a.mission.title; return a.mission.area; }
        if (h >= s + WorkRoster.FirstBlock && h < s + WorkRoster.FirstBlock + WorkRoster.LunchHours) { doing = "Comida"; return "Cocina"; }
        if (h < s + WorkRoster.MorningTraining && today.morning.TryGetValue(w, out string mt)) { doing = $"Entrenamiento de la mañana: {mt}"; return mt; }
        if (h >= s + 6f && (h < s + 6f + WorkRoster.AfternoonTraining || !roster.initiationModel)) { doing = today.training.TryGetValue(w, out string t) ? $"Entrenamiento: {t}" : "Entrenamiento"; return today.training.TryGetValue(w, out string ta) ? ta : "Sala de Yoga"; }
        doing = "Sin tarea asignada (disponible)";
        return null;
    }

    /// <summary>La misión que <paramref name="w"/> está haciendo a la hora <paramref name="h"/> (null si ninguna).</summary>
    public WorkRoster.Assignment Current(WorkRoster.Worker w, float h)
    {
        if (today == null) return null;
        foreach (WorkRoster.Assignment a in today.work) if (a.worker == w && h >= a.start && h < a.start + a.mission.hours) return a;
        return null;
    }

    // SALIR ANTES HACIA LA SIGUIENTE MISIÓN (2026-11-02), como Kushal (PlayerWorkday.HeadingEarly): de un árbol a otro se tarda
    // 35–95 s de una hora de 148 s, y quien salía a la hora en punto llegaba con la misión siguiente a medias (JornadaTest día 2).
    // Desde una misión sale a MITAD del viaje (las dos pierden lo mismo); ya comido, o en un hueco sin misión, con el viaje
    // entero (eso no es trabajo de nadie). Lo siguiente es una misión de hoy en otra área (no la comida ni el entrenamiento, que
    // empiezan a su hora). Una vez en camino no se da la vuelta (el viaje que queda se acorta al andar).
    readonly Dictionary<WorkRoster.Worker, WorkRoster.Assignment> _heading = new Dictionary<WorkRoster.Worker, WorkRoster.Assignment>();
    public WorkRoster.Assignment HeadingEarly(WorkRoster.Worker w, GameObject g, float h)
    {
        if (today == null || TC == null || g == null || w == null) return null;
        float s = WorkRoster.StartHour, l0 = s + WorkRoster.FirstBlock, l1 = l0 + WorkRoster.LunchHours;
        if (h < BreakfastEnd || h >= s + 7f) return null;   // (desde que acaba el desayuno: sale de casa hacia la primera misión)
        if (_heading.TryGetValue(w, out WorkRoster.Assignment latched))
        {
            if (latched != null && h < latched.start && (today.work.Contains(latched) || latched == LunchOf(w) || latched == TrainingOf(w))) return latched;
            _heading.Remove(w);
        }
        WorkRoster.Assignment cur = Current(w, h);
        float end, share;
        if (cur != null) { end = cur.start + cur.mission.hours; share = 0.5f; }
        else if (h >= l0 && h < l1)
        {
            if (!_lunched.Contains(g)) return null;   // aún no ha comido
            ActionQueue q = g.GetComponent<ActionQueue>();
            if (q != null && q.Own.Any(x => x is DineSpell)) return null;
            end = l1; share = 1f;
        }
        else
        {
            end = float.MaxValue;
            foreach (WorkRoster.Assignment a in today.work) if (a.worker == w && a.start > h && a.start < end) end = a.start;
            if (h < l0 && l0 <= end) end = l0;   // lo siguiente es la comida
            share = 1f;
        }
        WorkRoster.Assignment next = null;
        foreach (WorkRoster.Assignment a in today.work) if (a.worker == w && Mathf.Abs(a.start - end) < 0.01f) { next = a; break; }
        // LA COMIDA también es un sitio al que se va: de otra torre se llegaba a las 11:45 y se comía hasta pasadas las 12 (la
        // misión de las 12 se perdía: Lizmibet «Del ADN a la secuencia» 0 %, grupo 50). Sale hacia el restaurante como hacia una
        // misión (a mitad del viaje desde la de las 10); allí pide al dar las 11 (Where → «Comida»; la barra se llena entonces).
        if (next == null && Mathf.Abs(end - l0) < 0.01f && !_lunched.Contains(g)) next = LunchOf(w);
        if (next == null && Mathf.Abs(end - (s + 6f)) < 0.01f) next = TrainingOf(w);   // (es 1 h: saliendo a las 14 se iba media en el viaje)
        if (next == null || next.mission.area == AreaWork.Guard || (cur != null && next.mission.area == cur.mission.area)) return null;
        Vector3 to = WorkPoint(next.mission.area, g.name);
        if (to == Vector3.zero) return null;
        float perHour = TC.SecondsPerGameDay / 24f;
        float lead = (ArbolCampos.TripSeconds(g.transform.position, to) + (AereoRule.IsArea(next.mission.area) ? AereoRule.TripSeconds(next.mission.area) : 0f)) * 1.15f / Mathf.Max(1f, perHour) * share;
        if (end - h > lead) return null;
        _heading[w] = next;
        Debug.Log($"[Jornada] {h:00.0} h · {w.name} sale ya hacia «{next.mission.title}» ({next.mission.area}, a las {next.start:00.0} h): el viaje es de {lead / share * perHour:0} s.");
        return next;
    }

    // La comida de cada uno como destino (HeadingEarly). El título NO es «Comida»: de camino y antes de las 11 no pide en la barra
    // (OwnNeeds pide al llegar solo con «Comida», que es lo que da Where de 11 a 12, con la barra ya llena).
    readonly Dictionary<WorkRoster.Worker, WorkRoster.Assignment> _lunchOf = new Dictionary<WorkRoster.Worker, WorkRoster.Assignment>();
    WorkRoster.Assignment LunchOf(WorkRoster.Worker w)
    {
        if (!_lunchOf.TryGetValue(w, out WorkRoster.Assignment a))
            _lunchOf[w] = a = new WorkRoster.Assignment { worker = w, mission = new WorkRoster.Mission(PlayerWorkday.LunchArea, "comida", "De camino a la comida", WorkRoster.LunchHours), start = WorkRoster.StartHour + WorkRoster.FirstBlock };
        return a;
    }

    // El entrenamiento de las 14 como destino, como la comida. El título es el de Where: Drive lo trata como entrenamiento al llegar.
    readonly Dictionary<WorkRoster.Worker, WorkRoster.Assignment> _trainingOf = new Dictionary<WorkRoster.Worker, WorkRoster.Assignment>();
    WorkRoster.Assignment TrainingOf(WorkRoster.Worker w)
    {
        if (today == null || !today.training.TryGetValue(w, out string area)) return null;
        if (!_trainingOf.TryGetValue(w, out WorkRoster.Assignment a) || a.mission.area != area)
            _trainingOf[w] = a = new WorkRoster.Assignment { worker = w, mission = new WorkRoster.Mission(area, "entrenamiento", $"Entrenamiento: {area}", WorkRoster.AfternoonTraining), start = WorkRoster.StartHour + 6f };
        return a;
    }

    /// <summary>¿Hay algún recién llegado sin iniciar? (Linduarda lo atiende antes que nada: es la médica que lo recibe)</summary>
    static float _newcomerCheck = -1f; static bool _newcomerCached;
    public static bool NewcomerWaiting()
    {
        if (Time.time < _newcomerCheck) return _newcomerCached;   // (una vez por segundo: antes era una búsqueda por personaje y frame)
        _newcomerCheck = Time.time + 1f; _newcomerCached = false;
        foreach (SanctuaryOnboarding o in FindObjectsByType<SanctuaryOnboarding>(FindObjectsSortMode.None)) if (o != null && !o.apt) { _newcomerCached = true; break; }
        return _newcomerCached;
        return false;
    }

    // TODOS SIGUEN SU MISIÓN: las conductas propias (Linduarda en la enfermería, el guardia que lleva raciones) solo
    // funcionan cuando la misión de ese momento es la suya; si no, se apagan y el personaje va al área que le toca.
    // El que tenga la misión «Patrulla y raciones a los carnívoros» recibe la conducta de guardia, sea quien sea.
    public const string EvacuationDoing = "Evacuación del sitio nuclear: control de contaminación";

    /// <summary>EVACÚA la zona de exclusión del sitio nuclear: el personal va al hospital en alfombra y a Kushal lo saca la Magnate. Devuelve
    /// cuántos siguen dentro. La jornada solo mueve a la gente en horario de trabajo: por eso <see cref="NuclearSite"/> lo repite mientras dura.</summary>
    public int EvacuateSite()
    {
        int left = 0;
        foreach (WorkRoster.Worker w in roster.workers)
        {
            GameObject g = Body(w.name);
            if (g == null || !NuclearSiteRule.InZone(g.transform.position)) continue;
            left++;
            if (g.CompareTag("Player")) { MagnateWarden.Bring(g, AreaPoint(AreaWork.Hospital, "Player"), "Emergencia en el sitio nuclear: te saco de aquí. Vamos al hospital, al control de contaminación."); continue; }
            if (!OnOwnDuty(g)) Drive(g, AreaWork.Hospital, EvacuationDoing, null, true);
        }
        foreach (var kv in _clones)
            if (kv.Value != null && NuclearSiteRule.InZone(kv.Value.transform.position)) { left++; if (!OnOwnDuty(kv.Value)) Drive(kv.Value, AreaWork.Hospital, EvacuationDoing, null, true); }
        return left;
    }

    void Drive(GameObject g, string area, string doing, string missionId, bool alwaysMove = false)
    {
        // EVACUACIÓN DEL SITIO NUCLEAR (docs/sitio-nuclear.md): nadie va a sus pisos, y quien estaba allí, al hospital.
        // Quien está en la zona se va, sea cual sea su misión: la jornada lo pide cada fotograma y la evacuación solo cada 2 s (ronda 12: a Linduarda, de obra, la dejaba allí).
        bool evacuating = NuclearSite.Evacuating && (NuclearSite.IsSiteArea(area) || NuclearSiteRule.InZone(g.transform.position));
        if (evacuating) { area = AreaWork.Hospital; doing = EvacuationDoing; missionId = null; alwaysMove = true; }
        AereoLink.Note(g, area);   // SANTUARIO AÉREO (docs/sitio-nuclear.md §23): quien trabaja allí vuela desde el muelle y su misión corre por el reloj
        if (AereoRule.IsArea(area) && (AereoLink.IsAway(g) || AereoLink.Board(g))) return;
        if ((missionId == SiteLine.MissionId || missionId == Refuge.MissionId || HunterCell.Rides(missionId)) && SiteLine.Board(g, missionId)) return;   // la bajada: ya en la estación, sube al carro (docs/sitio-nuclear.md §8)
        NewcomerInitiator ini = g.GetComponent<NewcomerInitiator>();
        if (ini != null && ini.InProgress) return;   // está iniciando a un recién llegado: lo lleva ella (si no, la jornada la mandaba en alfombra a otro piso)
        Dress(g, area, doing, missionId);   // (2026-11-04) el uniforme de su tarea al llegar; su ropa al acabar
        bool afternoon = _hour >= WorkRoster.StartHour + 6f && _hour < WorkRoster.StartHour + 6f + WorkRoster.AfternoonTraining;   // (las clases del gimnasio y de yoga son de las 14 h; a las 8 se entrena en el sitio, una hora)
        bool gymTime = afternoon && !alwaysMove && area == GymArea && missionId == null && doing != null && doing.StartsWith("Entrenamiento");
        if (!gymTime) LeaveGym(g);   // deja libre su máquina del gimnasio (si tenía)
        bool yogaTime = afternoon && !alwaysMove && area == YogaArea && missionId == null && doing != null && doing.StartsWith("Entrenamiento");
        if (!yogaTime && missionId != "yog_guia") LeaveYogaClass(g);   // deja la clase de yoga (si estaba en ella)
        if (missionId != "yog_guia" && missionId != "yog_demo") DropClassSequence(g);   // la secuencia de la clase, solo mientras la clase es su misión
        IdleBehavior idle = g.GetComponent<IdleBehavior>();
        Linduarda lin = g.GetComponent<Linduarda>();
        CarnivoreKeeper keeper = g.GetComponent<CarnivoreKeeper>();
        bool guardDuty = missionId == "gua_carnivoros";
        if (guardDuty && keeper == null && g.GetComponent<Anima>() != null) keeper = g.AddComponent<CarnivoreKeeper>();
        if (keeper != null) keeper.enabled = guardDuty;
        // EL ALIMENTADOR (docs/alimentador.md): quien tiene la misión sigue la lista compartida, sea quien sea.
        bool feederDuty = missionId != null && missionId.StartsWith("gua_alimentador");
        PredatorFeeder predatorFeeder = g.GetComponent<PredatorFeeder>();
        if (feederDuty && predatorFeeder == null && g.GetComponent<Anima>() != null) predatorFeeder = g.AddComponent<PredatorFeeder>();
        if (predatorFeeder != null) predatorFeeder.enabled = feederDuty;
        if (feederDuty && predatorFeeder != null && idle != null) idle.enabled = false;
        // LA RONDA DE FRUTALES (docs/jardines-frutales.md): quien la tiene recorre los techos con su conducta propia.
        bool gardenDuty = FruitGardener.IsRound(missionId);
        FruitGardener gardener = g.GetComponent<FruitGardener>();
        if (gardenDuty && gardener == null) gardener = g.AddComponent<FruitGardener>();
        if (gardener != null) { gardener.enabled = gardenDuty; if (gardenDuty) gardener.Assign(missionId); }
        if (area != AreaWork.Hospital) HospitalStation.Release(g.name);   // su puesto médico queda libre para otro
        // LOS ANIMALES INGRESADOS: quien está de turno en el hospital les lleva la comida (el lobo de la perrera no comía nunca).
        VetCare vet = g.GetComponent<VetCare>();
        bool vetDuty = area == AreaWork.Hospital && missionId != null && g.GetComponent<Anima>() != null && g.GetComponent<Linduarda>() == null;
        if (vetDuty && vet == null) vet = g.AddComponent<VetCare>();
        if (vet != null) vet.enabled = vetDuty;
        if (vet != null && vet.Busy) { if (idle != null) idle.enabled = false; SetStatus(g, vet.Doing ?? doing); return; }
        bool linAway = lin != null && area == AreaWork.Hospital && !NewcomerWaiting() && !InWorkspace(null, AreaWork.Hospital, g.transform.position);   // (su conducta solo sabe del hospital: desde otra torre se quedaba quieta allí, toda la hora)
        if (lin != null) lin.enabled = !evacuating && !linAway && (area == null || area == AreaWork.Hospital || NewcomerWaiting());   // (con su conducta encendida, Drive no la mueve: en la evacuación tiene que irse como los demás)
        if (lin != null && lin.enabled && NewcomerWaiting() && idle != null) idle.enabled = false;   // con un recién llegado, Linduarda no merodea: lo atiende
        if (RestaurantService.Drives(g)) { if (idle != null) idle.enabled = false; return; }   // la brigada del restaurante: la mueve el servicio (puesto, pase, obrador, limpieza)
        if (area == null)
        {
            // EL HORARIO LIBRE (docs/horario-libre.md): en sus horas libres lo lleva a su actividad de ocio o cuidado de la semana.
            if (LeisureDirector.Drives(g)) { if (idle != null) idle.enabled = false; if (lin != null && !NewcomerWaiting()) lin.enabled = false; return; }
            // El sitio nuclear no es para el tiempo libre: quien acaba allí su turno, se va.
            if (NuclearSiteRule.InZone(g.transform.position)) { Drive(g, GymArea, "Sale del sitio nuclear al acabar su turno", null, true); return; }
            if (g.name == YogaTeacher) { if (idle != null) idle.enabled = false; TeacherMeditates(g); SetStatus(g, "Meditando en la sala de yoga"); return; }   // su tiempo libre: meditar
            // HUECO DENTRO DEL TURNO (p. ej. la obra acaba a las 10 y lo siguiente es a las 12): no es tiempo libre. Antes esta
            // rama le encendía el merodeo ANTES de llegar a OwnNeeds (que es donde se quita el merodeo en turno), y JefeConstruccion
            // y Gohageneis merodeaban por la obra atravesando la «Columna», la «Grua_pluma» y la «Miniexcavadora» (JornadaTest
            // 2026-10-29). En el hueco se queda donde está (sus necesidades siguen yendo por su carril propio).
            if (_hour >= WorkRoster.StartHour && _hour < WorkRoster.StartHour + 8f && g.GetComponent<Anima>() != null)
            {
                if (idle != null) idle.enabled = false;
                ActionQueue gq = g.GetComponent<ActionQueue>();
                if (gq != null && gq.Player.Count > 0 && gq.Player[0] is WanderSpell) gq.RemovePlayerAt(0);
                SetStatus(g, lin != null && lin.enabled ? lin.Doing : null);
                return;
            }
            if (idle != null && !idle.enabled) idle.enabled = true;   // libre
            SetStatus(g, lin != null && lin.enabled ? lin.Doing : null);   // (la doctora: lo que esté haciendo)
            return;
        }
        SetStatus(g, lin != null && lin.enabled && lin.Doing != null ? lin.Doing : doing);
        if ((feederDuty && predatorFeeder != null) || (guardDuty && keeper != null) || (lin != null && lin.enabled)) { LeaveGym(g); return; }   // su conducta propia hace ESTA misión
        if (gardenDuty && gardener != null) { if (idle != null) idle.enabled = false; SetStatus(g, gardener.Doing ?? doing); return; }   // la ronda la lleva FruitGardener (y sus necesidades van antes, dentro de ella)
        // LAS NECESIDADES VAN ANTES QUE LA CLASE DE YOGA (2026-11-02): con «ir al baño» o «pedir en la barra» en marcha en su carril
        // propio, la clase (su papel de guía, demostrador o corrector, o la esterilla del entrenamiento de las 14 h) lo seguía
        // llevando a la sala A LA VEZ, cada fotograma: un paso hacia los aseos y otro igual hacia la sala, y no se movía. Lizmibet
        // y Goluis en el piso de Construcción (camino de la clase de las 8, con los aseos al otro lado), y Goluis ×46 «pedir en
        // la barra» junto a su esterilla de la Sala de Yoga (JornadaTest). Deja la clase; al acabar, vuelve a ella.
        bool needing = NeedsDriver.Attending(g.GetComponent<ActionQueue>());
        if (needing) LeaveYogaClass(g);
        if (!needing && YogaClassRole(g, missionId)) { if (idle != null) idle.enabled = false; return; }
        if (gymTime && GymSession(g, doing)) { if (idle != null) idle.enabled = false; return; }
        if (!needing && yogaTime && YogaTraining(g)) { if (idle != null) idle.enabled = false; return; }
        if (idle != null && idle.enabled) idle.enabled = false;
        if (OwnNeeds(g, area, doing)) return;   // [Jornada] comer, el baño y dormir van antes; a mediodía, a comer
        Vector3 to = EnergySite.FieldPoint(WorkRoster.LessonBase(missionId), g.name) ?? WorkPoint(area, g.name);   // (los paneles y las torres se revisan en el casquete, no en el piso de control)
        if (!AtWork(g, area, to))
        {
            if (g.GetComponent<Anima>() != null && CarpetPad.Route(g, ref to)) return;   // otro grupo de plataformas: en alfombra voladora
            if (alwaysMove && g.GetComponent<Anima>() == null) g.transform.position = Vector3.MoveTowards(g.transform.position, to, walkSpeed * 3f * Time.deltaTime);
            else PathMover.Of(g.transform).StepToward(to, walkSpeed);
        }
        else Perform(g, area, doing, missionId);   // (2026-11-04) YA EN SU SITIO: hace la tarea, no se queda de pie
    }

    // ── HACER LA TAREA (docs/actividades-y-uniformes.md §3; Berón 2026-11-04: «parece que solo van al piso que les toca y no hacen
    // nada»). En su puesto, cada uno hace la actuación de su misión (Performances: pasos con postura, herramienta en la mano y su
    // efecto); la cola larga, el bucle genérico de su clase. Mientras la jornada lo tenga aquí se llama cada fotograma: si se va (a
    // comer, al baño, a otra misión), el que actúa se para solo (ActivityPerformer.KeepAlive).
    static void Perform(GameObject g, string area, string doing, string missionId)
    {
        if (g == null || g.GetComponent<Anima>() == null) return;   // (los clones de la Magnate no tienen esqueleto que mover)
        ActivityPerformer.Performance p = missionId != null ? Performances.ForMission(WorkRoster.LessonBase(missionId))
            : doing != null && doing.StartsWith("Entrenamiento") ? Performances.ForTraining(area) : null;
        if (p == null) return;
        Vector3 face = AreaTransform(area) != null ? AreaTransform(area).position : g.transform.position + g.transform.forward;
        if (area == AreaWork.Hospital) { HospitalStation post = HospitalStation.PostFor(g.name); if (post != null) face = post.transform.position; }
        ActivityPerformer ap = ActivityPerformer.Keep(g, p, face);
        if (ap != null && ap.Doing != null) SetStatus(g, $"{doing} · {ap.Doing}");
    }

    // ── EL UNIFORME DE LA TAREA (docs/actividades-y-uniformes.md §4): al llegar a su tarea se lo pone; al acabar (libre, la comida, otra
    // tarea sin uniforme) vuelve a su ropa. Se mira una vez por segundo por persona.
    readonly Dictionary<GameObject, float> _dressAt = new Dictionary<GameObject, float>();
    void Dress(GameObject g, string area, string doing, string missionId)
    {
        if (g == null || g.GetComponent<Anima>() == null) return;
        if (_dressAt.TryGetValue(g, out float at) && Time.time < at) return;
        _dressAt[g] = Time.time + 1f;
        Uniforms.Kind k = Uniforms.ForWork(WorkRoster.LessonBase(missionId), area, doing);
        if (k == Uniforms.Kind.None) { UniformWearer.Want(g, k); return; }
        Vector3 pos = g.transform.position;
        bool there = area == AreaWork.Guard || FruitGardener.IsRound(missionId) || EnergySite.IsField(WorkRoster.LessonBase(missionId)) || InAreaNamed(area, pos)
                     || (WorkPoint(area, g.name) - pos).sqrMagnitude < 36f;
        if (there) UniformWearer.Want(g, k);
    }

    /// <summary>(Pruebas) Lo mismo que hace la jornada con quien está en su puesto: el uniforme de su tarea y la actuación.</summary>
    public static void PerformForTest(GameObject g, string area, string doing, string missionId, bool dress = true)
    {
        if (dress) UniformWearer.Want(g, Uniforms.ForWork(missionId, area, doing));   // (lo que hace Dress al llegar)
        Perform(g, area, doing, missionId);
    }

    // [Jornada] Las NECESIDADES van antes que el puesto: con «ir al baño», «a comer» o «a la cama» en su carril propio la jornada
    // no tira de él a la vez (lo hacía dar vueltas en círculo y no llegar nunca: JornadaTest 2026-10-29). A mediodía, ya en la
    // cocina, se sienta a comer (DineSpell) una vez al día. En turno no se merodea.
    readonly HashSet<GameObject> _lunched = new HashSet<GameObject>();
    readonly Dictionary<GameObject, (QueuedSpell s, float t)> _needSince = new Dictionary<GameObject, (QueuedSpell, float)>();
    const float MaxNeed = 240f;   // s de juego: una necesidad que no acaba (no llega a la barra, a los aseos) no lo saca de su puesto para siempre
    bool _lunchStocked;
    bool OwnNeeds(GameObject g, string area, string doing)
    {
        ActionQueue q = g.GetComponent<ActionQueue>(); if (q == null) return false;
        if (q.Player.Count > 0 && q.Player[0] is WanderSpell) q.RemovePlayerAt(0);
        if (q.Own.Count > 0)
        {
            QueuedSpell s = q.Own[0];
            if (s is ToiletSpell || s is DineSpell || s is SleepSpell || s is EatSpell || s is CarpetWalkSpell)
            {
                if (!_needSince.TryGetValue(g, out var ns) || ns.s != s) _needSince[g] = ns = (s, Time.time);
                if (s is DineSpell ds && ds.Eating) _needSince[g] = (s, Time.time);   // ya con el plato: come hasta acabar (antes se le cortaba sentado —Irosene «deja comer Yogur tras 240 s»— y volvía a pedir)
                else if (!(s is SleepSpell) && Time.time - ns.t > MaxNeed) { q.ClearOwn(); Debug.Log($"[Jornada] «{g.name}» deja «{s.Label}» tras {MaxNeed:0} s sin acabar: vuelve a su puesto."); return false; }
                SetStatus(g, s.Label); return true;
            }
        }
        if (doing == "Comida" && !_lunched.Contains(g) && g.GetComponent<Anima>() != null && InWorkspace(null, area, g.transform.position))
        {
            _lunched.Add(g);
            BodyMetabolism body = g.GetComponent<BodyMetabolism>();
            if (body != null && body.hunger < 0.25f) return false;   // ya comió (el hambre de verdad, NeedsDriver): no come dos veces y llega a la misión de las 12
            q.EnqueueOwn(new DineSpell()); return true;
        }
        return false;
    }

    // [Jornada] LA COMIDA DE LAS 11:00: el servicio del restaurante (misión obligatoria de cada día) deja la carta en la barra con
    // raciones para todo el personal. Sin esto la barra estaba vacía a mediodía y nadie comía («no hay comida en ningún sitio»).
    void StockLunch()
    {
        RestaurantKitchen rk = FindAnyObjectByType<RestaurantKitchen>();
        if (rk == null || rk.dishes == null || rk.dishes.Length == 0 || today == null || !today.work.Any(a => a.mission.id == "coc_servicio")) return;
        int per = Mathf.CeilToInt((roster.workers.Count + today.clones.Count + 2) / (float)rk.dishes.Length);
        int added = 0;
        foreach (RestaurantKitchen.DishStock d in rk.dishes)
        {
            if (d.recipe == null) continue;
            int need = per - d.Have;
            if (need > 0) { rk.Deliver(d.recipe.dishName, null, need); added += need; }
        }
        Debug.Log($"[Jornada] 11:00 · el servicio deja la comida en la barra: +{added} raciones ({per} de cada uno de {rk.dishes.Length} platillos).");
    }

    // ── PISOS DE YOGA (docs/yoga-pisos.md) ─────────────────────────────────────────────────────
    // Dos pisos (Hatha y Ashtanga) con dos salas cada uno (occidental y tradicional; YogaHall); la de cada día la da
    // YogaHall.VersionOn (tradicional los pares, occidental los impares). La CLASE DE HATHA (misión de grupo «clase_yoga»): el
    // guía hace la secuencia en la tarima de su sala, el demostrador la hace entre los alumnos y los correctores van de
    // esterilla en esterilla. El ENTRENAMIENTO de las 14 h es la CLASE DE ASHTANGA, en el piso de Ashtanga: la da su maestra (Consol) y, si no está, el primero que
    // llega. En su tiempo libre, el MAESTRO DE YOGA ('YogaTeacher': Josedrilo, el profesional de yoga; antes Andrea y Sadhguru) medita en la tarima de la sala de Hatha tradicional, junto al linga.
    const string YogaArea = "Sala de Yoga";
    /// <summary>LOS DOS MAESTROS JEFES DE YOGA (Berón 2026-11-05, docs/reglas-personajes.md): <see cref="AshtangaTeacher"/>, CONSOL, experta en
    /// Ashtanga, dirige ese estilo; <see cref="YogaTeacher"/>, JOSEDRILO, dirige el Hatha y medita junto al linga en su tiempo libre.
    /// Ya no hay segunda maestra: Panterilia, que lo fue sin base documentada, se queda con el baile y las finanzas. Historia: el maestro fue
    /// primero Sadhguru (se fue a su microcosmos), luego Andrea (pasó a entrenador del gimnasio) y luego JOSEDRILO, el profesional de
    /// yoga (Berón 2026-11-04: «el profesional de yoga es Josedrilo»).</summary>
    public const string YogaTeacher = "Josedrilo", AshtangaTeacher = "Consol";
    /// <summary>La sesión de cada clase: Hatha (la guiada, misión «yog_guia») y Ashtanga (el entrenamiento de las 14 h).</summary>
    public const string HathaSession = "Saludo al Sol", AshtangaSession = "Ashtanga Primaria";

    /// <summary>La maestra de Ashtanga, si hoy da la clase de las 14 h y está libre para abrirla (despierta, sin necesidades en marcha y sin
    /// otra clase); si no, null, y la abre el primero que llega.</summary>
    static Anima AshtangaHead()
    {
        WorkdayDirector wd = Instance;
        WorkRoster.Worker w = wd != null ? wd.roster.Find(AshtangaTeacher) : null;
        if (w == null || wd.today == null || !wd.today.training.TryGetValue(w, out string tr) || tr != YogaArea) return null;
        GameObject g = Body(AshtangaTeacher);
        Anima a = g != null ? g.GetComponent<Anima>() : null;
        if (a == null || a.asleep || global::YogaClass.ClassOf(a) != null || NeedsDriver.Attending(g.GetComponent<ActionQueue>())) return null;
        return a;
    }

    /// <summary>Entrenamiento de yoga: a la CLASE DE ASHTANGA de hoy (<see cref="global::YogaClass"/>, en su sala del piso de
    /// Ashtanga). Si no hay ninguna, la da el primero que llega; los demás van cada uno a su esterilla (en la tradicional, antes
    /// al kund) y repiten los pasos del maestro. false = sin esterilla libre.</summary>
    static bool YogaTraining(GameObject g)
    {
        Anima a = g.GetComponent<Anima>(); ActionQueue q = g.GetComponent<ActionQueue>();
        if (a == null || a.asleep) return false;
        global::YogaClass mine = global::YogaClass.ClassOf(a);
        if (q != null && q.Own.Count > 0 && mine == null) return false;   // comer, el baño y dormir van antes
        if (mine != null) return true;
        // (regla J16) TODOS A LA VEZ: si la clase de la sala del día está llena, la siguiente sala del estilo (la otra versión, luego
        // los demás pisos de Ashtanga) abre otra clase con el que llega; así caben todos los miembros (TrainingCentres).
        foreach (YogaHall h in YogaHall.Ordered(YogaHall.Style.Ashtanga, YogaHall.VersionOn(YogaHall.Today)))
        {
            global::YogaClass c = global::YogaClass.On(h);
            if (c == null) { c = global::YogaClass.Begin(AshtangaHead() ?? a, AshtangaSession, h); if (c != null && (c.teacher == a || c.Join(a))) return true; continue; }
            if (c.Join(a)) return true;
        }
        return false;
    }

    static void LeaveYogaClass(GameObject g)
    {
        if (g == null) return;
        Anima a = g.GetComponent<Anima>();
        global::YogaClass c = global::YogaClass.ClassOf(a);
        if (c != null) c.Leave(a);
    }

    /// <summary>Un punto de la sala de la clase de Hatha de hoy (local a la sala: su tarima en el origen).</summary>
    static Vector3 YogaPoint(Vector3 local) => HallPoint(YogaHall.For(HathaSession, YogaHall.Today), local);

    static Vector3 HallPoint(YogaHall h, Vector3 local)
    {
        if (h != null) return h.transform.TransformPoint(local);
        Transform room = global::YogaClass.Room;
        return room != null ? room.TransformPoint(local) : Vector3.zero;
    }

    // La secuencia que la CLASE pone en la cola propia del guía o del demostrador: al acabar esa misión se le QUITA. Antes se
    // quedaba y Linduarda (de 8 a 9 demostradora) seguía en asanas a las 10 en vez de ir a Biología, y Panterilia, en el
    // quirófano (JornadaTest 2026-10-29). Solo está en la clase quien tiene la clase por misión (o entrena yoga a las 14).
    static readonly Dictionary<GameObject, HashSet<QueuedSpell>> _classSeq = new Dictionary<GameObject, HashSet<QueuedSpell>>();
    static void DropClassSequence(GameObject g)
    {
        if (g == null || !_classSeq.TryGetValue(g, out HashSet<QueuedSpell> seq)) return;
        _classSeq.Remove(g);
        if (!g.TryGetComponent(out ActionQueue q) || q.RemoveOwnWhere(s => seq.Contains(s)) == 0) return;
        Anima a = g.GetComponent<Anima>(); if (a != null) DailyPoses.Stand().ApplyFinal(a);
        Debug.Log($"[Jornada] «{g.name}» deja la secuencia de la clase de yoga: su misión ahora es otra.");
    }

    /// <summary>Si la misión es de la clase de yoga, coloca al personaje en su papel; true = ya lo maneja.</summary>
    bool YogaClassRole(GameObject g, string missionId)
    {
        if (missionId == null || !missionId.StartsWith("yog_") || missionId == "yog_sala" || missionId == "yog_aseos") return false;
        if (missionId == "yog_guia")   // el guía da la CLASE DE HATHA: sube a la tarima de la sala de hoy y los alumnos le siguen desde sus esterillas
        {
            Anima ga = g.GetComponent<Anima>();
            YogaClass c = ga != null ? YogaClass.Begin(ga, HathaSession, YogaHall.For(HathaSession, YogaHall.Today)) : null;
            if (c != null && c.teacher == ga) return true;
        }
        Vector3 to = missionId == "yog_guia" ? YogaPoint(new Vector3(0f, 0.6f, 0f))
                   : missionId == "yog_demo" ? YogaPoint(new Vector3(0f, 0.2f, -3.4f * 0.82f))
                   : YogaPoint(new Vector3(Mathf.Cos(Time.time * 0.05f + (missionId.EndsWith("2") ? 3f : 0f)) * 4.4f, 0.2f, Mathf.Sin(Time.time * 0.05f + (missionId.EndsWith("2") ? 3f : 0f)) * 3.6f));   // los correctores rondan entre las esterillas
        if ((g.transform.position - to).sqrMagnitude > 1f) { PathMover.Of(g.transform).StepToward(to, walkSpeed); return true; }
        if (missionId == "yog_guia" || missionId == "yog_demo")
        {
            Vector3 face = YogaPoint(Vector3.zero) - g.transform.position; face.y = 0f;
            if (missionId == "yog_demo" && face.sqrMagnitude > 0.01f) g.transform.rotation = Quaternion.LookRotation(face);
            ActionQueue q = g.GetComponent<ActionQueue>();
            if (q != null && q.Own.Count == 0) foreach (QueuedSpell s in SunSalutation.Sequence()) { if (!_classSeq.TryGetValue(g, out var seq)) _classSeq[g] = seq = new HashSet<QueuedSpell>(); seq.Add(s); q.EnqueueOwn(s); }   // la secuencia, a la perfección
        }
        return true;
    }

    void TeacherMeditates(GameObject g)
    {
        Anima me = g.GetComponent<Anima>(); ActionQueue mq = g.GetComponent<ActionQueue>();
        if (me == null || me.asleep || (mq != null && mq.Own.Count > 0)) return;   // comer, el baño y dormir van antes
        // En la tarima de la sala de Hatha TRADICIONAL (junto al linga), si no hay clase en ella; si la hay, en la occidental.
        YogaHall h = YogaHall.Find(YogaHall.Style.Hatha, YogaHall.Version.Tradicional);
        if (h == null || (YogaClass.On(h) != null && YogaClass.On(h).teacher != me)) h = YogaHall.Find(YogaHall.Style.Hatha, YogaHall.Version.Occidental) ?? h;
        Vector3 to = HallPoint(h, new Vector3(0f, 0.6f, 0f));
        if ((g.transform.position - to).sqrMagnitude > 1f) { PathMover.Of(g.transform).StepToward(to, walkSpeed); return; }
        Anima a = g.GetComponent<Anima>();
        if (a != null) DailyPoses.Meditate().ApplyFinal(a);
        g.GetComponentInChildren<CubeFace>()?.Show(CubeFace.Expression.Asleep, 2f);   // ojos cerrados
    }

    // ── GIMNASIO (docs/gimnasio-maquinas.md, §13 y §15) ────────────────────────────────────────
    // El entrenamiento en el gimnasio (14–16 h) ya no es un circuito igual para todos: cada uno sigue la RUTINA DE HOY de su plan
    // (GymProgramming: reparto, series × repeticiones, carga, descanso y RPE) con una GymTrainingSession: calentamiento (la clase
    // del entrenador o cardio suave), los ejercicios en orden —máquina libre, si no una alternativa, si no espera o lo deja para el
    // final—, las series con su descanso y la vuelta a la calma. El ENTRENADOR (GymProgramming.Trainer) da la clase de
    // calentamiento, hace rondas corrigiendo y de spotter (GymCoach) y, los días de su plan, la última hora hace su rutina.
    const string GymArea = GymProgramming.GymArea;
    const float GymClassMin = 10f, GymEndMin = 55f, GymCoachRoundsEnd = 35f;   // min de juego desde las 14:00 (una hora desde el 2026-11-05: la otra pasó a las 8:00)

    /// <summary>Una persona en una máquina: la estación, si ya se sentó, la fase (A/B) y su postura. Público para que
    /// <see cref="GymUsabilityTest"/> use EXACTAMENTE el mismo camino que la jornada.</summary>
    public class GymUse { public GymStation st; public bool seated, b; public float since, phaseAt; public AsanaPoseSpell spell; }
    readonly Dictionary<GameObject, GymTrainingSession> _sessions = new Dictionary<GameObject, GymTrainingSession>();
    GymCoach _coach; int _coachDay = int.MinValue;
    /// <summary>Las sesiones de gimnasio de hoy (el panel de la rutina).</summary>
    public IReadOnlyDictionary<GameObject, GymTrainingSession> GymSessions => _sessions;
    public GymCoach Coach => _coach;
    /// <summary>¿Está <paramref name="g"/> ahora en una máquina del gimnasio (sentado en su puesto, haciendo la repetición)?</summary>
    public bool OnGymMachine(GameObject g)
    {
        if (g == null) return false;
        GymTrainingSession s = _sessions.TryGetValue(g, out GymTrainingSession x) ? x : _coach != null && _coach.g == g ? _coach.own : null;
        return s != null && s.use != null && s.use.seated;
    }
    /// <summary>Todos se levantan de las máquinas del gimnasio (y las dejan en su sitio): al terminar una prueba de la jornada a
    /// las 16:00 alguien podía quedarse sentado con el agarre desplazado a su medida, y la prueba del gimnasio lo encontraba así.</summary>
    public void ReleaseGym()
    {
        foreach (GymTrainingSession s in _sessions.Values) s.Stop("se acaba el entrenamiento");
        _sessions.Clear();
        if (_coach != null) _coach.Stop("se acaba el entrenamiento");
        _coach = null;
    }
    float _hour;

    /// <summary>Punto de la clase de calentamiento del entrenador (la zona libre del gimnasio) y hacia dónde mira.</summary>
    public static Vector3 GymClassPoint() => WorkPoint(GymArea, "Clase de calentamiento");

    /// <summary>El entrenador de hoy (una vez al día): si el entrenador es del personal y hoy le toca el gimnasio.</summary>
    void EnsureCoach(float min, float ms)
    {
        if (_coachDay == today.day) return;
        WorkRoster.Worker tw = roster.Find(GymProgramming.Trainer);
        GameObject g = tw != null && today.training.TryGetValue(tw, out string t) && t == GymArea ? Body(GymProgramming.Trainer) : null;
        if (g == null || g.CompareTag("Player")) return;
        _coachDay = today.day;
        GymProgramming.DayRoutine own = GymProgramming.For(g.name, today.day)?.Trains(today.day) == true ? GymProgramming.Prescribe(g.name, today.day) : null;
        float classSec = Mathf.Max(0f, GymClassMin - min) * ms;
        float roundsSec = own != null ? Mathf.Max(0f, GymCoachRoundsEnd - Mathf.Max(min, GymClassMin)) * ms : 999999f;
        Transform at = AreaTransform(GymArea);
        _coach = new GymCoach(g, GymClassPoint(), at != null ? Quaternion.Euler(0f, at.eulerAngles.y, 0f) : Quaternion.identity, classSec, roundsSec, own, (GymEndMin - Mathf.Max(min, GymCoachRoundsEnd)) * ms) { walkSpeed = walkSpeed };
    }

    /// <summary>Si toca gimnasio, conduce a <paramref name="g"/> por su rutina de hoy; true = ya lo maneja. false = a la zona
    /// libre (antes del bloque, ya terminó su rutina o sin plan): sigue el punto del área de siempre.</summary>
    bool GymSession(GameObject g, string doing)
    {
        Anima a = g.GetComponent<Anima>();
        ActionQueue q = g.GetComponent<ActionQueue>();
        if (a == null || a.asleep || (q != null && q.Own.Count > 0)) { PauseGym(g); return false; }   // comer, el baño y dormir van antes
        if (today == null) return false;
        float min = (_hour - (WorkRoster.StartHour + 6f)) * 60f;
        float ms = TC != null && TC.TimeSpeedMinuteSecs > 0.01f ? TC.TimeSpeedMinuteSecs : 2.5f;
        if (min < 0f) return false;
        if (min >= GymEndMin) { LeaveGym(g); return false; }
        // EL ENTRENADOR: la clase, las rondas y (si su plan lo pide hoy) su rutina. Se crea con el primero que llega al bloque (si
        // no, los que van antes que él en la lista del personal empezaban a calentar solos sin ver la clase).
        EnsureCoach(min, ms);
        if (g.name == GymProgramming.Trainer)
        {
            if (_coach == null || _coachDay != today.day) return false;
            if (!_coach.Tick(_sessions.Values)) { LeaveGym(g); return false; }
            SetStatus(g, $"{doing} · {_coach.Status}");   // («Entrenamiento: Gimnasio · Entrenador: …»: JornadaTest no le mira la postura en el gimnasio)
            return true;
        }
        if (!_sessions.TryGetValue(g, out GymTrainingSession s))
        {
            GymProgramming.DayRoutine r = GymProgramming.Prescribe(g.name, today.day);
            if (r == null) return false;   // sin plan (no es humano del santuario)
            _sessions[g] = s = new GymTrainingSession(g, r, (GymEndMin - min) * ms) { walkSpeed = walkSpeed };
        }
        if (s.Done) return false;   // ya terminó: a la zona libre
        GymCoach coach = _coach != null && _coachDay == today.day ? _coach : null;
        if (!s.Tick(coach)) return false;
        SetStatus(g, $"{doing} · {s.Status()}");
        return true;
    }

    /// <summary>A dónde anda para usar <paramref name="st"/>: el pasillo junto a la máquina (a la altura del pivote, como AreaPoint).</summary>
    public static Vector3 GymApproach(GymStation st) => st.ApproachPoint + Vector3.up;

    /// <summary>¿Ya llegó andando junto a la máquina (mismo piso, a menos de 0,8 m del punto de llegada)?</summary>
    public static bool GymArrived(GameObject g, GymStation st)
    {
        Vector3 d = GymApproach(st) - g.transform.position;
        bool sameFloor = Mathf.Abs(d.y) < 1.5f; d.y = 0f;
        return sameFloor && d.sqrMagnitude <= 0.8f * 0.8f;
    }

    /// <summary>Se sienta en la máquina: empieza por la postura A.</summary>
    public static void GymSit(GymUse u) { u.seated = true; u.b = false; u.phaseAt = Time.time; u.spell = u.st.PoseA(); }

    /// <summary>Un fotograma de la repetición: alterna A ↔ B al ritmo real del equipo, coloca el cuerpo en el puesto y
    /// aplica la postura. true = en este fotograma cambió de fase.</summary>
    public static bool GymRep(GameObject g, Anima a, GymUse u) => GymRep(g, a, u, GymPoses.RepHalf(u.st.type));   // el ritmo real de cada equipo (cinta 0,35 s, bici 0,4 s, press…)

    /// <summary>Lo mismo con el ritmo <paramref name="half"/> (s por mitad): la rutina lo ajusta para que quepa en el bloque.</summary>
    public static bool GymRep(GameObject g, Anima a, GymUse u, float half)
    {
        GymStation want = u.st;
        bool flip = Time.time - u.phaseAt >= half;
        if (flip) { u.b = !u.b; u.phaseAt = Time.time; u.spell = u.b ? want.PoseB() : want.PoseA(); }
        float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((Time.time - u.phaseAt) / (half * 0.6f)));
        GymPlace(g, want.SeatPoint(u.b ? k : 1f - k), want.Facing);   // (el puesto se desplaza entre A y B en las que tienen shiftB)
        if (a != null) u.spell.Tick(a);
        GymFit.Fit(want, g, u.b);   // a la medida de quien la usa: pies en lo que pisa, manos en el agarre
        return flip;
    }

    /// <summary>DESCANSO ENTRE SERIES: sigue en la máquina, quieto en la postura A (a su medida).</summary>
    public static void GymHold(GameObject g, Anima a, GymUse u)
    {
        if (u == null || u.st == null) return;
        if (u.b || u.spell == null) { u.b = false; u.spell = u.st.PoseA(); }
        GymPlace(g, u.st.SeatPoint(0f), u.st.Facing);
        if (a != null) u.spell.Tick(a);
        GymFit.Fit(u.st, g, false);
    }

    /// <summary>Se levanta de la máquina: de pie, en el pasillo (la reserva la suelta <see cref="GymStation.Release"/>).</summary>
    public static void GymStandUp(GameObject g, GymUse u)
    {
        if (u != null) GymFit.Reset(u.st);   // el asiento, las palancas y la barra vuelven a su sitio
        if (g == null || u == null || !u.seated || u.st == null) return;
        Anima a = g.GetComponent<Anima>();
        if (a != null) DailyPoses.Stand().ApplyFinal(a);
        // (2026-11-04) De pie en el pasillo con el pivote a la altura con la que ANDA (PathMover.OnGround: su suelo y su altura de
        // pivote). Con la de los pies del cuerpo de cubos, el primer paso lo recolocaba de golpe a otra altura (GymUsabilityTest ronda 6).
        g.transform.position = PathMover.Of(g.transform).OnGround(u.st.ApproachPoint);
        PathMover.Of(g.transform).Restart();
    }

    /// <summary>Coloca los PIES (la base del cuerpo de cubos) de <paramref name="g"/> en <paramref name="feet"/>, mirando a <paramref name="facing"/>.</summary>
    public static void GymPlace(GameObject g, Vector3 feet, Quaternion facing)
    {
        Transform body = g.transform.Find(CubeHumanoid.BodyName);
        float off = body != null ? g.transform.position.y - body.position.y : 1f;
        g.transform.SetPositionAndRotation(new Vector3(feet.x, feet.y + off, feet.z), facing);
    }

    /// <summary>Se levanta de la máquina (de pie, en el pasillo) y la deja libre.</summary>
    void LeaveGym(GameObject g)
    {
        if (g == null) return;
        if (_sessions.TryGetValue(g, out GymTrainingSession s) && !s.Done) s.Stop("se acaba el entrenamiento");
        if (_coach != null && _coach.g == g && _coach.phase != GymCoach.Phase.Done) _coach.Stop("se acaba el entrenamiento");
    }

    /// <summary>Lo deja a medias por una necesidad (comer, el baño, dormir): suelta la máquina; al volver sigue la rutina.</summary>
    void PauseGym(GameObject g)
    {
        if (g == null) return;
        if (_sessions.TryGetValue(g, out GymTrainingSession s)) s.Pause();
        if (_coach != null && _coach.g == g && _coach.own != null) _coach.own.Pause();
    }

    static readonly Dictionary<GameObject, string> _baseLabel = new Dictionary<GameObject, string>();
    public static void SetStatus(GameObject g, string doing)
    {
        WorldLabel wl = g.GetComponent<WorldLabel>(); if (wl == null) return;
        if (!_baseLabel.TryGetValue(g, out string b)) _baseLabel[g] = b = wl.text;
        wl.text = doing == null ? b : $"{b}\n<{doing}>";
    }

    /// <summary>Un punto del área (repartidos por persona para que no se amontonen).</summary>
    static readonly Dictionary<string, Transform> _areas = new Dictionary<string, Transform>();
    /// <summary>El área por su nombre visible (cacheada).</summary>
    /// <summary>Un sitio de trabajo que no es un área de la escena (la estación de la línea de las cavernas): la jornada lo trata como a un área.</summary>
    public static void RegisterPlace(string name, Transform t) { _areas[name] = t; }

    static float _areasRetry;
    public static Transform AreaTransform(string area)
    {
        if (area == null) return null;
        if (_areas.TryGetValue(area, out Transform t) && t != null) return t;
        // Un área que no está en esta escena no se busca en cada llamada (se pide por persona y por frame).
        if (Time.unscaledTime < _areasRetry && !ColdSanctuary.TestRunner.Running) return null;
        _areasRetry = Time.unscaledTime + 3f;
        foreach (SanctuaryArea a in FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None)) _areas[a.displayName] = a.transform;
        return _areas.TryGetValue(area, out t) ? t : null;
    }

    /// <summary>Dónde trabaja <paramref name="who"/> en <paramref name="area"/>: en el HOSPITAL, en uno de sus 8 puestos médicos
    /// (docs/hospital.md); en el resto (o si los puestos están llenos), un punto del área.</summary>
    static Vector3 WorkPoint(string area, string who)
    {
        if (area == AreaWork.Hospital) { HospitalStation post = HospitalStation.PostFor(who); if (post != null) return Clear(area, who, post.StandPoint, 0.12f); }   // (radio justo: pegado a la mesa es su sitio; dentro de ella, no)
        Vector3 p = AreaPoint(area, who);
        // Al NavMesh (como RestaurantService): la rejilla de puestos alrededor del centro del área caía a veces DENTRO de un
        // mueble (el «Bloque_Intercambiador» de la central) y se le iba a buscar atravesándolo (JornadaTest 2026-10-29).
        if (p != Vector3.zero && UnityEngine.AI.NavMesh.SamplePosition(p + Vector3.down, out UnityEngine.AI.NavMeshHit h, 2.5f, UnityEngine.AI.NavMesh.AllAreas) && Mathf.Abs(h.position.y - (p.y - 1f)) < 1f)
            p = h.position + Vector3.up;
        return Clear(area, who, p);
    }

    // Un puesto de la rejilla dentro de un mueble (29 alumnos en una clase del Hospital: «Quirofano 2», «Bloque_Microrreactor»): al sitio libre más cercano. Se recuerda: se pide por persona y frame.
    static readonly Dictionary<(string, string), (Vector3 raw, Vector3 free)> _clear = new Dictionary<(string, string), (Vector3, Vector3)>();
    static bool Solid(Vector3 p, float radius)
    {
        foreach (Collider c in Physics.OverlapCapsule(p - Vector3.up * 0.4f, p + Vector3.up * 0.7f, radius, ~0, QueryTriggerInteraction.Ignore))
            if (c.GetComponentInParent<Anima>() == null) return true;
        return false;
    }
    // «Ya está» a 2 m vale en un área abierta, no en un puesto del hospital: el puesto está a 1,1–1,45 m del centro de su mesa,
    // y quien llegaba por el otro lado de la mesa se paraba encima y empezaba a trabajar allí. En un puesto hay que llegar a él;
    // si el camino no alcanza el punto (acotado: 6 s a menos de 2 m), trabaja donde llegó y lo dice, en vez de andar para siempre.
    static readonly Dictionary<string, float> _postWalk = new Dictionary<string, float>();
    static int _postWalkLogs;
    static bool AtWork(GameObject g, string area, Vector3 to)
    {
        Vector3 d = g.transform.position - to;
        if (d.sqrMagnitude > 4f) { _postWalk.Remove(g.name); return false; }
        // Fuera de un puesto, a 2 m vale, salvo si está DENTRO de un mueble: el alumno de una lección del hospital veterinario sin puesto
        // (los 8 son del personal) llegaba a la rejilla por el otro lado de una mesa y se quedaba en ella (Goluis, «Quirofano veterinario 3»).
        bool post = area == AreaWork.Hospital && HospitalStation.PostFor(g.name) != null;
        if (!post && !Solid(g.transform.position, 0.12f)) { _postWalk.Remove(g.name); return true; }
        d.y = 0f;
        if (d.sqrMagnitude <= 0.16f) return true;
        if (!_postWalk.TryGetValue(g.name, out float since)) { _postWalk[g.name] = Time.time; return false; }
        if (Time.time - since < 6f) return false;
        if (_postWalkLogs < 20 && Time.time - since < 6.5f) { _postWalkLogs++; Debug.Log($"[Jornada] «{g.name}» no alcanza su puesto {to}: se queda a {d.magnitude:0.0} m, en {g.transform.position}."); }
        return true;
    }

    static Vector3 Clear(string area, string who, Vector3 p, float radius = 0.35f)
    {
        if (p == Vector3.zero) return p;
        var key = (area, who);
        if (_clear.TryGetValue(key, out var c) && (c.raw - p).sqrMagnitude < 0.04f) return c.free;
        Vector3 free = p;
        if (Solid(p, radius))
            for (int ring = 1; ring <= 3 && free == p; ring++)
                for (int k = 0; k < 8; k++)
                {
                    Vector3 q = p + Quaternion.Euler(0f, k * 45f, 0f) * Vector3.forward * (ring * 1.5f);
                    if (!Solid(q, radius) && UnityEngine.AI.NavMesh.SamplePosition(q + Vector3.down, out UnityEngine.AI.NavMeshHit nh, 0.8f, UnityEngine.AI.NavMesh.AllAreas)) { free = nh.position + Vector3.up; break; }
                }
        _clear[key] = (p, free);
        return free;
    }

    public static Vector3 AreaPoint(string area, string who)
    {
        Transform t = null;
        if (area == AreaWork.Guard) { GameObject gt = Body("Guardia_AUTO"); t = gt != null ? gt.transform : null; }
        else t = AreaTransform(area);
        if (t == null) return Vector3.zero;
        int hsh = Mathf.Abs((who ?? "").GetHashCode());
        Vector3 off = new Vector3((hsh % 5 - 2) * 1.2f, 1f, ((hsh / 5) % 3 - 1) * 1.2f + 2f);
        return t.TransformPoint(off);
    }

    void Begin(WorkRoster.Assignment a)
    {
        Debug.Log($"[Jornada] {a.start:00.0} h · {a.worker.name} empieza «{a.mission.title}» ({a.mission.area}).");
        // OBRA VISIBLE (docs/construction-simulation.md): el turno de «Trabajar la obra» va colocando las piezas del día.
        if (a.mission.id != null && a.mission.id.StartsWith("obra_trabajo_") && a.mission.site != null)
            foreach (ConstructionProject cp in ConstructionProject.All)
                if (cp != null && cp.title == a.mission.site) cp.BeginShift(a.worker.name, Body(a.worker.name), a.start, a.mission.hours);
        if (a.lesson) { TeachLesson(a); return; }
        if (a.firstTime && !a.worker.clone)
        {
            // INICIACIÓN: la Magnate (un clon suyo) aparece junto a quien hace la misión por primera vez y lo inicia.
            initiations++;
            GameObject g = Body(a.worker.name);
            GameObject ini = SpawnMagnateBody("Magnate (iniciación)");
            if (ini != null && g != null) ini.transform.position = g.transform.position + g.transform.right * 1.2f;
            if (ini != null) _initiators.Add((ini, a.start + 0.5f));
            Debug.Log($"[Iniciación] La Magnate inicia a {a.worker.name} en «{a.mission.title}»: ella lanza los hechizos y {a.worker.name} los repite.");
        }
    }

    // LA INICIACIÓN: un cuerpo de la Magnate por clase, en el área de la tarea, durante la hora. Solo si ningún compañero puede dar la clase (roster.instructorsTeach): ella es el último recurso, máxima autoridad de todas las áreas.
    readonly HashSet<WorkRoster.Lesson> _taughtToday = new HashSet<WorkRoster.Lesson>();
    public int LessonsGiven { get; private set; }
    void TeachLesson(WorkRoster.Assignment a)
    {
        WorkRoster.Lesson l = today.lessons.FirstOrDefault(x => x.mission == a.mission);
        if (l == null || !_taughtToday.Add(l)) return;
        LessonsGiven++;
        if (l.instructor != null) { Debug.Log($"[Iniciación] {a.start:00.0} h · {l}."); return; }   // (la da un compañero: no hace falta un cuerpo de la Magnate)
        GameObject m = SpawnMagnateBody(l.teacher);
        Vector3 at = EnergySite.FieldPoint(WorkRoster.LessonBase(l.mission.id), l.teacher) ?? WorkPoint(l.mission.area, l.teacher);
        if (m != null && at != Vector3.zero) m.transform.position = at;
        if (m != null) _initiators.Add((m, a.start + WorkRoster.LessonHours));
        Debug.Log($"[Iniciación] {a.start:00.0} h · {l}.");
    }

    void Finish(WorkRoster.Assignment a)
    {
        Debug.Log($"[Jornada] {a.worker.name} termina «{a.mission.title}».");
        if (a.lesson) return;   // una lección no cambia nada en el santuario: el efecto es del trabajo de verdad
        // (2026-11-04) LA DE KUSHAL NO LA HACE NADIE POR ÉL: su efecto lo aplica su tarea con E al hacer el último paso
        // (PlayerTaskRunner) o la máquina de virtualización; si la ignora, caduca sin efecto (PlayerWorkday).
        if (!a.worker.clone && a.worker.name == "Player") return;
        Apply(a.mission, a.worker.name);
        if (a.mission.id == GymProgramming.PlanMissionId && today != null)   // el entrenador deja los planes revisados
            foreach (string l in GymProgramming.Review(a.worker.name, today.day, roster.workers.Where(w => !w.clone).Select(w => w.name))) Debug.Log($"[Entrenador] {a.worker.name} {l}");
    }

    /// <summary>Efecto de completar una misión en el mundo: lo trae su función en el REGISTRO (<see cref="AreaFunctions.Run"/>: las obras,
    /// el material, el laboratorio, el servicio de la carta y las elaboraciones); el resto, de momento, se registra.</summary>
    public static void Apply(WorkRoster.Mission m, string who)
    {
        AreaFunctions.Run(m.id, who);
    }

    // Un cuerpo de la Magnate (copia de su cuerpo de cubos) sin su conducta: para clones de trabajo e iniciaciones.
    public static GameObject SpawnMagnateBody(string label) => SpawnBodyCopy("Magnate", label);

    /// <summary>Una copia del cuerpo de cubos de <paramref name="source"/> (sin su conducta), de pie: clones, turistas virtuales…</summary>
    public static GameObject SpawnBodyCopy(string source, string label)
    {
        // Dentro de una virtualización el santuario está apagado (GameObject.Find no ve lo inactivo) y la persona puede
        // llamarse «<nombre>_Post»: sin esto, el turista virtual salía como una etiqueta sin cuerpo.
        GameObject src = Body(source);
        if (src == null)
            foreach (Anima a in Object.FindObjectsByType<Anima>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (a.name == source || a.name == source + "_Post") { src = a.gameObject; break; }
        GameObject g = new GameObject(label);
        if (src != null)
        {
            Transform body = src.transform.Find(CubeHumanoid.BodyName);
            if (body != null)
            {
                Transform b = Instantiate(body.gameObject, g.transform, false).transform; b.name = body.name; b.localPosition = body.localPosition; b.localScale = body.localScale;
                // La copia sale DE PIE: si el original estaba en una asana (yoga, enseñando), la copia no se queda congelada en ella.
                ClonePose.Rest(b);
            }
            g.transform.position = src.transform.position + Vector3.right * 1.5f;
        }
        WorldLabel wl = g.AddComponent<WorldLabel>(); wl.text = label; wl.height = 2.1f;
        return g;
    }

    /// <summary>El jugador hace una misión del área desde la máquina (VIRTUALIZACIÓN): la registra (ya la conoce) y aplica su efecto.</summary>
    public void PlayerVirtualizes(WorkRoster.Mission m)
    {
        // [Jornada de Kushal] a la rotación lo mete PlayerWorkday al terminar la iniciación (si no, entraba aquí a medias)
        WorkRoster.Worker w = roster.Find("Player") ?? new WorkRoster.Worker("Player");
        PlayerWorkday.Virtualized(m);   // [Jornada de Kushal] cuenta como hecha si es de su jornada
        bool first = !w.Knows(m.id);
        w.count[m.id] = w.Count(m.id) + 1; w.lastDay[m.id] = TC != null ? TC.Day : 0;
        if (first) { initiations++; Debug.Log($"[Iniciación] La Magnate inicia a Kushal en «{m.title}» dentro de la virtualización."); }
        Debug.Log($"[Virtualización] Kushal practica «{m.title}» ({m.area}).");
        Progress.MarkMission(m.id);
        Apply(m, "Kushal");
    }

    /// <summary>Texto del plan (consola <c>jornada</c>).</summary>
    public List<string> Report()
    {
        var l = new List<string>();
        if (today == null) { l.Add("Aún no hay jornada planificada (empieza a las 8:00)."); return l; }
        l.Add($"Día {today.day} · áreas: {string.Join(" → ", today.areaOrder)}");
        foreach (WorkRoster.Worker w in roster.workers.Concat(today.clones))
        {
            var mine = today.Of(w).Select(a => $"{a.start:00}h {a.mission.title}{(a.firstTime ? " (nueva)" : "")}");
            string tr = today.training.TryGetValue(w, out string t) ? $" · 14h entrenamiento: {t}" : "";
            l.Add($"{w.name}: {string.Join(" · ", mine)}{tr}");
        }
        l.AddRange(WorkRoster.Balance(today, roster.workers.Count));
        return l;
    }

    // ── HUD del jugador: su itinerario de hoy ──
    /// <summary>El itinerario de hoy del jugador (null si no tiene jornada): lo que sale en el recuadro de arriba a la derecha y en la
    /// ventana «Misiones y jornada» (<see cref="HudModals"/>), de la misma fuente. <paramref name="marks"/>: cada misión con ✓ o ☐.</summary>
    public List<string> Itinerary(bool marks = false)
    {
        if (today == null || TC == null) return null;
        WorkRoster.Worker me = roster.Find("Player"); if (me == null) return null;
        float h = TC.Hour;
        string now = Where(me, h, out string doing) != null ? doing : (h >= WorkRoster.StartHour + 8f ? "Tiempo libre" : "Antes de la jornada");
        var lines = new List<string> { $"Jornada · día {today.day} · {Mathf.Floor(h):00}:{(h % 1f) * 60f:00}", $"Ahora: {now}" };
        foreach (var a in today.Of(me)) lines.Add($"{(marks ? (PlayerWorkday.Done(a) ? "✓ " : "☐ ") : "")}{a.start:00}:00  {a.mission.title}");
        lines.Add("11:00  Comida (restaurante)");
        if (today.training.TryGetValue(me, out string tr)) lines.Add($"14:00  Entrenamiento: {tr}");
        lines.Add("16:00  Libre");
        return lines;
    }

    void OnGUI()
    {
        List<string> lines = Itinerary();
        if (lines == null) return;
        GUI.Box(new Rect(Screen.width - 330, 10, 320, 22 + lines.Count * 18), "");
        for (int i = 0; i < lines.Count; i++) GUI.Label(new Rect(Screen.width - 322, 14 + i * 18, 310, 20), lines[i]);
    }
}
