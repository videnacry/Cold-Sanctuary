using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// LA JORNADA EN EL JUEGO (docs/jornada-y-rotacion.md). Cada día a las 8:00 reparte el trabajo con <see cref="WorkRoster"/>
/// entre los miembros del santuario y lo hace cumplir hasta las 16:00:
///   8–11 trabajo · 11–12 comida (en el restaurante) · 12–14 trabajo · 14–16 entrenamiento (yoga, gimnasio…) · LIBRE.
/// Durante esas 8 h los personajes van al área de su misión y se quedan allí (bloqueados); después merodean a su aire.
/// Si una misión es NUEVA para alguien, la Magnate aparece a su lado y lo inicia (la mímica: ella lanza los hechizos y el
/// otro los repite). Si faltan personas para las obligatorias, la Magnate se DIVIDE: sus clones van a esos puestos y
/// desaparecen al terminar. Las obras avanzan con sus misiones (material + trabajo) y, terminadas, abren sus
/// entrenamientos. Consola: <c>jornada</c> (plan y equilibrio).
/// </summary>
public class WorkdayDirector : MonoBehaviour
{
    public static WorkdayDirector Instance { get; private set; }

    [Tooltip("Personal del santuario en su orden FIJO (el de la rotación). El jugador entra cuando ya es miembro.")]
    public string[] staff = { "Player", "Linduarda", "Lizmibet", "Goluis", "Panterilia", "Gohageneis", "Irosene", "JefeConstruccion", "Sadhguru", "Andrea" };
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
    /// <summary>El GameObject de un miembro del personal (en escena algunos se llaman «Goluis_Post»…). Cacheado: buscar por nombre cada frame era lo más caro de la jornada.</summary>
    public static GameObject Body(string n)
    {
        if (_bodies.TryGetValue(n, out GameObject g) && g != null) return g;
        g = GameObject.Find(n) ?? GameObject.Find(n + "_Post");
        _bodies[n] = g;
        return g;
    }

    void SyncStaff()
    {
        foreach (string n in staff)
        {
            GameObject g = Body(n);
            if (g == null || roster.Find(n) != null) continue;
            if (!Magnate.IsMember(g.GetComponent<Anima>())) continue;   // aún sin iniciación: no trabaja (está con Linduarda)
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
        today = roster.Plan(day, AreaWork.Today(day, ConstructionProject.All));
        plannedDay = day;
        foreach (WorkRoster.Worker c in today.clones) _clones[c] = SpawnMagnateBody(c.name);
        Debug.Log($"[Jornada] Día {day}: {today.work.Count} misiones para {roster.workers.Count} personas · orden de áreas: {string.Join(" → ", today.areaOrder)}" +
                  (today.clones.Count > 0 ? $" · la Magnate se divide en {today.clones.Count}" : "") + (today.backlog.Count > 0 ? $" · {today.backlog.Count} pendientes para mañana" : ""));
        foreach (string s in WorkRoster.Balance(today, roster.workers.Count)) Debug.Log("[Jornada] " + s);
        return today;
    }

    void Update()
    {
        if (TC == null || ColdSanctuary.TestRunner.Running) return;   // las pruebas usan a los personajes
        float h = TC.Hour; int day = TC.Day;
        if (plannedDay != day && h >= WorkRoster.StartHour && h < WorkRoster.StartHour + 8f) PlanDay(day);
        if (today == null || plannedDay != day) return;
        Tick(h);
    }

    /// <summary>Avanza la jornada a la hora <paramref name="h"/>: empieza y termina misiones, mueve a cada uno a su sitio.</summary>
    public void Tick(float h)
    {
        foreach (WorkRoster.Assignment a in today.work)
        {
            float end = a.start + a.mission.hours;
            if (h >= a.start && !_started.Contains(a)) { _started.Add(a); Begin(a); }
            if (h >= end && !_done.Contains(a)) { _done.Add(a); Finish(a); }
        }
        // Clones que ya no tienen trabajo pendiente: desaparecen.
        foreach (var kv in _clones.ToList())
            if (kv.Value != null && today.work.Where(x => x.worker == kv.Key).All(x => _done.Contains(x))) { Destroy(kv.Value); _clones.Remove(kv.Key); Debug.Log($"[Jornada] {kv.Key.name} termina y se disuelve."); }
        for (int i = _initiators.Count - 1; i >= 0; i--) if (_initiators[i].go == null || h >= _initiators[i].until) { if (_initiators[i].go != null) Destroy(_initiators[i].go); _initiators.RemoveAt(i); }

        foreach (WorkRoster.Worker w in roster.workers)
        {
            GameObject g = Body(w.name);
            if (g == null || g.CompareTag("Player")) continue;
            string where = Where(w, h, out string doing);
            Drive(g, where, doing, Current(w, h)?.mission.id);
        }
        foreach (var kv in _clones)
            if (kv.Value != null) { string where = Where(kv.Key, h, out string doing); Drive(kv.Value, where, doing, null, alwaysMove: true); }
        HoldPlayer(h);
    }

    // EL JUGADOR TAMBIÉN ESTÁ BLOQUEADO durante la jornada: si sale del área de lo que le toca (trabajo, comida o
    // entrenamiento), vuelve a ella con un aviso. La Guardia patrulla por todo el santuario: ahí no se le retiene.
    string _holdMsg; float _holdMsgUntil;
    GameObject _player;
    public int playerHeldBack { get; private set; }
    readonly Dictionary<string, float> _areaRadius = new Dictionary<string, float>();

    void HoldPlayer(float h)
    {
        WorkRoster.Worker me = roster.Find("Player"); if (me == null) return;
        if (_player == null) _player = GameObject.FindGameObjectWithTag("Player");
        GameObject p = _player; if (p == null) return;
        if (MobWorldLoader.Instance != null && (MobWorldLoader.Instance.IsInMobWorld || MobWorldLoader.Instance.IsBusy)) return;   // dentro del microcosmos
        string area = Where(me, h, out string doing);
        if (area == null || area == AreaWork.Guard) return;
        Vector3 c = AreaPoint(area, "centro"); if (c == Vector3.zero) return;
        float r = AreaRadius(area);
        Vector3 d = p.transform.position - c; d.y = 0f;
        if (d.magnitude <= r) return;
        CharacterController cc = p.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        p.transform.position = AreaPoint(area, "Player");
        if (cc != null) cc.enabled = true;
        playerHeldBack++;
        _holdMsg = $"Jornada: ahora te toca «{doing}» en {area}. Hasta las 16:00 no hay tiempo libre."; _holdMsgUntil = Time.time + 4f;
        Debug.Log("[Jornada] " + _holdMsg);
    }

    float AreaRadius(string area)
    {
        if (_areaRadius.TryGetValue(area, out float r)) return r;
        r = 14f;
        foreach (SanctuaryArea a in FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None))
            if (a.displayName == area)
            {
                Renderer[] rs = a.GetComponentsInChildren<Renderer>(); if (rs.Length == 0) break;
                Bounds b = rs[0].bounds; foreach (Renderer x in rs) b.Encapsulate(x.bounds);
                r = new Vector2(b.extents.x, b.extents.z).magnitude + 2f; break;
            }
        _areaRadius[area] = r;
        return r;
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
        if (h >= s + 6f) { doing = today.training.TryGetValue(w, out string t) ? $"Entrenamiento: {t}" : "Entrenamiento"; return today.training.TryGetValue(w, out string ta) ? ta : "Sala de Yoga"; }
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

    /// <summary>¿Hay algún recién llegado sin iniciar? (Linduarda lo atiende antes que nada: es la médica que lo recibe)</summary>
    static float _newcomerCheck = -1f; static bool _newcomerCached;
    static bool NewcomerWaiting()
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
    void Drive(GameObject g, string area, string doing, string missionId, bool alwaysMove = false)
    {
        IdleBehavior idle = g.GetComponent<IdleBehavior>();
        Linduarda lin = g.GetComponent<Linduarda>();
        CarnivoreKeeper keeper = g.GetComponent<CarnivoreKeeper>();
        bool guardDuty = missionId == "gua_carnivoros";
        if (guardDuty && keeper == null && g.GetComponent<Anima>() != null) keeper = g.AddComponent<CarnivoreKeeper>();
        if (keeper != null) keeper.enabled = guardDuty;
        if (lin != null) lin.enabled = area == null || area == "Enfermería" || NewcomerWaiting();
        if (lin != null && lin.enabled && NewcomerWaiting() && idle != null) idle.enabled = false;   // con un recién llegado, Linduarda no merodea: lo atiende
        if (area == null)
        {
            if (g.name == "Sadhguru") { if (idle != null) idle.enabled = false; SadhguruMeditates(g); SetStatus(g, "Meditando en la sala de yoga"); return; }   // su tiempo libre: meditar
            if (idle != null && !idle.enabled) idle.enabled = true;   // libre
            SetStatus(g, null);
            return;
        }
        SetStatus(g, doing);
        if ((guardDuty && keeper != null) || (lin != null && lin.enabled)) return;   // su conducta propia hace ESTA misión
        if (YogaClass(g, missionId)) { if (idle != null) idle.enabled = false; return; }
        if (idle != null && idle.enabled) idle.enabled = false;
        Vector3 to = AreaPoint(area, g.name);
        if ((g.transform.position - to).sqrMagnitude > 4f)
        {
            if (alwaysMove && g.GetComponent<Anima>() == null) g.transform.position = Vector3.MoveTowards(g.transform.position, to, walkSpeed * 3f * Time.deltaTime);
            else PathMover.Of(g.transform).StepToward(to, walkSpeed);
        }
    }

    // ── SALA DE YOGA ─────────────────────────────────────────────────────────────────────────────
    // La CLASE (misión de grupo): el guía hace la secuencia en la plataforma central, el demostrador la hace entre los
    // alumnos y los correctores van de esterilla en esterilla. En su tiempo libre, SADHGURU medita en la plataforma,
    // inmóvil como una estatua más.
    static Transform _yogaRoom;
    static Vector3 YogaPoint(Vector3 local)
    {
        if (_yogaRoom == null) { GameObject y = GameObject.Find("YogaRoom_Area"); _yogaRoom = y != null ? y.transform : null; }
        return _yogaRoom != null ? _yogaRoom.TransformPoint(local) : Vector3.zero;
    }

    /// <summary>Si la misión es de la clase de yoga, coloca al personaje en su papel; true = ya lo maneja.</summary>
    bool YogaClass(GameObject g, string missionId)
    {
        if (missionId == null || !missionId.StartsWith("yog_") || missionId == "yog_sala" || missionId == "yog_aseos") return false;
        Vector3 to = missionId == "yog_guia" ? YogaPoint(new Vector3(0f, 0.6f, 0f))
                   : missionId == "yog_demo" ? YogaPoint(new Vector3(0f, 0.2f, -3.4f * 0.82f))
                   : YogaPoint(new Vector3(Mathf.Cos(Time.time * 0.05f + (missionId.EndsWith("2") ? 3f : 0f)) * 4.4f, 0.2f, Mathf.Sin(Time.time * 0.05f + (missionId.EndsWith("2") ? 3f : 0f)) * 3.6f));   // los correctores rondan entre los anillos
        if ((g.transform.position - to).sqrMagnitude > 1f) { PathMover.Of(g.transform).StepToward(to, walkSpeed); return true; }
        if (missionId == "yog_guia" || missionId == "yog_demo")
        {
            Vector3 face = YogaPoint(Vector3.zero) - g.transform.position; face.y = 0f;
            if (missionId == "yog_demo" && face.sqrMagnitude > 0.01f) g.transform.rotation = Quaternion.LookRotation(face);
            ActionQueue q = g.GetComponent<ActionQueue>();
            if (q != null && q.Own.Count == 0) foreach (QueuedSpell s in SunSalutation.Sequence()) q.EnqueueOwn(s);   // la secuencia, a la perfección
        }
        return true;
    }

    void SadhguruMeditates(GameObject g)
    {
        Anima me = g.GetComponent<Anima>(); ActionQueue mq = g.GetComponent<ActionQueue>();
        if (me == null || me.asleep || (mq != null && mq.Own.Count > 0)) return;   // comer, el baño y dormir van antes
        Vector3 to = YogaPoint(new Vector3(0f, 0.6f, 0f));
        if ((g.transform.position - to).sqrMagnitude > 1f) { PathMover.Of(g.transform).StepToward(to, walkSpeed); return; }
        Anima a = g.GetComponent<Anima>();
        if (a != null) DailyPoses.Meditate().ApplyFinal(a);
        g.GetComponentInChildren<CubeFace>()?.Show(CubeFace.Expression.Asleep, 2f);   // ojos cerrados
    }

    static readonly Dictionary<GameObject, string> _baseLabel = new Dictionary<GameObject, string>();
    static void SetStatus(GameObject g, string doing)
    {
        WorldLabel wl = g.GetComponent<WorldLabel>(); if (wl == null) return;
        if (!_baseLabel.TryGetValue(g, out string b)) _baseLabel[g] = b = wl.text;
        wl.text = doing == null ? b : $"{b}\n<{doing}>";
    }

    /// <summary>Un punto del área (repartidos por persona para que no se amontonen).</summary>
    static readonly Dictionary<string, Transform> _areas = new Dictionary<string, Transform>();
    /// <summary>El área por su nombre visible (cacheada).</summary>
    public static Transform AreaTransform(string area)
    {
        if (area == null) return null;
        if (_areas.TryGetValue(area, out Transform t) && t != null) return t;
        foreach (SanctuaryArea a in FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None)) _areas[a.displayName] = a.transform;
        return _areas.TryGetValue(area, out t) ? t : null;
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

    void Finish(WorkRoster.Assignment a)
    {
        Debug.Log($"[Jornada] {a.worker.name} termina «{a.mission.title}».");
        Apply(a.mission, a.worker.name);
    }

    /// <summary>Efecto de completar una misión en el mundo (las obras avanzan; el resto, de momento, se registra).</summary>
    public static void Apply(WorkRoster.Mission m, string who)
    {
        if (m.id.StartsWith("obra_material_") || m.id.StartsWith("obra_trabajo_"))
        {
            string title = m.id.Substring(m.id.StartsWith("obra_material_") ? 14 : 13);
            foreach (ConstructionProject p in ConstructionProject.All)
                if (p.title == title) { if (m.id.StartsWith("obra_material_")) p.DeliverMaterial(who); else p.WorkDay(who); }
            return;
        }
        MaterialStore store = MaterialStore.Instance;
        KitchenLarder larder = KitchenLarder.Instance;
        CulturedMeatLab lab = CulturedMeatLab.All.Count > 0 ? CulturedMeatLab.All[0] : null;
        switch (m.id)
        {
            case "mat_guadua": store?.HarvestGuadua(who); return;
            case "mat_bahareque": store?.MakeBahareque(who); return;
            case "mat_micelio": store?.GrowMycelium(who); return;
            case "mat_cuerda": store?.TwistRope(who); return;
            case "lab_iniciar": lab?.OrderStart(who); return;
            case "lab_cosechar": lab?.OrderHarvest(who); return;
            case "coc_servicio": larder?.ServeService(12, who); return;
        }
        if (m.id.StartsWith("prep_") && larder != null) larder.StartPrep(m.id.Substring(5), who);
    }

    // Un cuerpo de la Magnate (copia de su cuerpo de cubos) sin su conducta: para clones de trabajo e iniciaciones.
    public static GameObject SpawnMagnateBody(string label)
    {
        GameObject mag = GameObject.Find("Magnate");
        GameObject g = new GameObject(label);
        if (mag != null)
        {
            Transform body = mag.transform.Find(CubeHumanoid.BodyName);
            if (body != null) { Transform b = Instantiate(body.gameObject, g.transform, false).transform; b.localPosition = body.localPosition; b.localScale = body.localScale; }
            g.transform.position = mag.transform.position + Vector3.right * 1.5f;
        }
        WorldLabel wl = g.AddComponent<WorldLabel>(); wl.text = label; wl.height = 2.1f;
        return g;
    }

    /// <summary>El jugador hace una misión del área desde la máquina (VIRTUALIZACIÓN): la registra (ya la conoce) y aplica su efecto.</summary>
    public void PlayerVirtualizes(WorkRoster.Mission m)
    {
        WorkRoster.Worker w = roster.Find("Player") ?? roster.Add("Player");
        bool first = !w.Knows(m.id);
        w.count[m.id] = w.Count(m.id) + 1; w.lastDay[m.id] = TC != null ? TC.Day : 0;
        if (first) { initiations++; Debug.Log($"[Iniciación] La Magnate inicia a Kushal en «{m.title}» dentro de la virtualización."); }
        Debug.Log($"[Virtualización] Kushal practica «{m.title}» ({m.area}).");
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
    void OnGUI()
    {
        if (Time.time < _holdMsgUntil && !string.IsNullOrEmpty(_holdMsg)) GUI.Box(new Rect(Screen.width * 0.5f - 300, Screen.height - 120, 600, 40), _holdMsg);
        if (today == null || TC == null) return;
        WorkRoster.Worker me = roster.Find("Player"); if (me == null) return;
        float h = TC.Hour;
        string now = Where(me, h, out string doing) != null ? doing : (h >= WorkRoster.StartHour + 8f ? "Tiempo libre" : "Antes de la jornada");
        var lines = new List<string> { $"Jornada · día {today.day} · {Mathf.Floor(h):00}:{(h % 1f) * 60f:00}", $"Ahora: {now}" };
        foreach (var a in today.Of(me)) lines.Add($"{a.start:00}:00  {a.mission.title}");
        lines.Add("11:00  Comida (restaurante)");
        if (today.training.TryGetValue(me, out string tr)) lines.Add($"14:00  Entrenamiento: {tr}");
        lines.Add("16:00  Libre");
        GUI.Box(new Rect(Screen.width - 330, 10, 320, 22 + lines.Count * 18), "");
        for (int i = 0; i < lines.Count; i++) GUI.Label(new Rect(Screen.width - 322, 14 + i * 18, 310, 20), lines[i]);
    }
}
