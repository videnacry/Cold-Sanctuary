using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// LA JORNADA DE KUSHAL (Berón 2026-10-29: «tras el microcosmos de la iniciación del huerto, a Kushal se le DA SU
/// JORNADA LABORAL»). Mientras es recién llegado la jornada no lo toca (análisis, vacuna, Ambrosio, sala de yoga, cocina,
/// huerto). Cuando termina TODA la iniciación —el huerto y su microcosmos incluidos (<see cref="Ready"/>)— entra en la
/// rotación del santuario como uno más del personal (<see cref="WorkdayDirector"/>):
///   · antes de las 8:00: el reparto de las 8:00 ya lo cuenta;
///   · a mitad del día: se le dan misiones para lo que queda de sus 5 h (en las áreas, sin partirlas en la comida), la
///     comida y su entrenamiento (incorporación);
///   · después de las 16:00: es libre; su primera jornada es mañana.
/// Se le enseña en pantalla (LessonPrompt; el HUD de la jornada y el checklist de progreso lo listan) y la Magnate lo trata
/// como al resto: si deja su puesto en el turno lo devuelve (WorkdayDirector.HoldPlayer), con un rato para ir andando o en
/// alfombra a cada puesto nuevo (<see cref="Commute"/>). A las 16:00 es libre: nada lo mueve y la Magnate no lo trae.
///
/// Cuenta lo que HACE (no basta con que pase la hora): cada misión está hecha si estuvo en su puesto al menos la mitad
/// de su tiempo, o si la practicó en la máquina de virtualización; la comida, si comió en el restaurante; el
/// entrenamiento, si estuvo en su sala. <see cref="autopilot"/> lo lleva solo a cada puesto (como la jornada a los demás):
/// lo usa la prueba en vivo (JornadaTest).
/// </summary>
public class PlayerWorkday : MonoBehaviour
{
    public static PlayerWorkday Instance { get; private set; }

    /// <summary>Segundos (de juego) que tiene para llegar a un puesto NUEVO antes de que la Magnate lo traiga: cruzar de una
    /// torre a otra en alfombra lleva su rato.</summary>
    public const float Commute = 120f;

    /// <summary>Se le dio la jornada (el jugador). Lo lanza al terminar la iniciación.</summary>
    public static event System.Action<GameObject> WorkdayGiven;

    [Tooltip("Lo lleva solo a cada puesto, a comer y a entrenar (como la jornada a los demás). Para las pruebas en vivo.")]
    public bool autopilot;
    public float walkSpeed = 2.6f;

    /// <summary>Una misión suya de hoy y lo que hizo en ella.</summary>
    public class Record
    {
        public WorkRoster.Assignment a;
        public float present, observed;   // segundos de juego en su puesto / de la misión que vio pasar
        public bool started, closed, done, virtualized;
        public float Share => observed > 0f ? present / observed : 0f;
    }

    readonly List<Record> _records = new List<Record>();
    WorkRoster.DayPlan _plan;
    GameObject _player;
    float _nextCheck;
    bool _held, _dineQueued, _dayClosed, _lunchSeen;
    int _dineTries;
    float _lunchHunger = -1f, _trainPresent, _trainObserved;
    DineSpell _eating;
    QueuedSpell _need; float _needSince;

    public IReadOnlyList<Record> Records => _records;
    public int Day => _plan != null ? _plan.day : int.MinValue;
    public bool AteToday { get; private set; }
    public bool TrainingDone { get; private set; }
    public float TrainingShare => _trainObserved > 0f ? _trainPresent / _trainObserved : 0f;
    public string TrainingArea { get; private set; }
    public bool DayClosed => _dayClosed;
    public int GivenDay { get; private set; } = int.MinValue;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("PlayerWorkday_AUTO");
        DontDestroyOnLoad(go);
        go.AddComponent<PlayerWorkday>();
    }

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; Hold(false); }

    // ── ¿Ya es del todo del santuario? ─────────────────────────────────────────────────────────────────────────────

    /// <summary>¿Completó toda la iniciación? Apto, presentado, iniciado en el microcosmos, formado en cocina y en el huerto.</summary>
    public static bool OnboardingComplete(SanctuaryOnboarding o) => o == null || (o.apt && o.presented && o.initiated && o.kitchenTrained && o.gardenTrained);

    /// <summary>¿Se le puede dar ya la jornada? Iniciación completa y de vuelta del microcosmos del huerto (no sigue con el de
    /// turno ni dentro del tanque).</summary>
    public static bool Ready(GameObject player)
    {
        if (player == null) return false;
        SanctuaryOnboarding o = player.GetComponent<SanctuaryOnboarding>();
        if (!OnboardingComplete(o)) return false;
        if (o != null && AreaInitiator.Owns(o)) return false;
        if (MobWorldLoader.HasInstance && (MobWorldLoader.Instance.IsInMobWorld || MobWorldLoader.Instance.IsBusy)) return false;
        return true;
    }

    /// <summary>Para quien termine la iniciación (el que lo forma en el huerto): darle la jornada ya, sin esperar al sondeo.</summary>
    public static void NotifyOnboardingComplete() { if (Instance != null) Instance._nextCheck = 0f; }

    /// <summary>¿Hizo hoy esta misión? (checklist de progreso)</summary>
    public static bool Done(WorkRoster.Assignment a)
    {
        if (Instance == null || a == null) return false;
        foreach (Record r in Instance._records) if (r.a == a) return r.done;
        return false;
    }

    /// <summary>La practicó en la máquina de virtualización (WorkdayDirector.PlayerVirtualizes): cuenta como hecha.</summary>
    public static void Virtualized(WorkRoster.Mission m)
    {
        if (Instance == null || m == null) return;
        foreach (Record r in Instance._records) if (r.a.mission.id == m.id && !r.closed) { r.virtualized = true; break; }
    }

    // ── Cada fotograma ─────────────────────────────────────────────────────────────────────────────────────────────

    void Update()
    {
        WorkdayDirector wd = WorkdayDirector.Instance;
        TimeController tc = TimeController.timeController;
        if (wd == null || tc == null) return;
        if (_player == null) _player = GameObject.FindGameObjectWithTag("Player");
        if (_player == null) return;
        float h = tc.Hour; int day = tc.Day;

        WorkRoster.Worker me = wd.roster.Find("Player");
        if (me == null)
        {
            if (Time.time < _nextCheck) return;
            _nextCheck = Time.time + 1f;
            if (!Ready(_player)) return;
            me = Give(wd, day, h);   // EL GANCHO: iniciación completa → su jornada
        }
        if (wd.today == null || wd.plannedDay != day) { Hold(false); return; }
        if (_plan != wd.today) NewDay(wd, me);
        Track(wd, me, h);
        if (autopilot) Pilot(wd, me, h); else Hold(false);
    }

    /// <summary>Le da la jornada: entra en la rotación y, si el día ya empezó, se incorpora con lo que queda.</summary>
    WorkRoster.Worker Give(WorkdayDirector wd, int day, float h)
    {
        WorkRoster.Worker me = wd.roster.Add("Player");
        GivenDay = day;
        float s = WorkRoster.StartHour, end = s + WorkRoster.WorkHours + WorkRoster.LunchHours + WorkRoster.TrainingHours;
        string when;
        if (wd.today != null && wd.plannedDay == day && h < end) { int n = LateJoin(wd, me, day, h); when = n > 0 ? $"se incorpora HOY con {n} misión(es)" : "se incorpora HOY (comida y entrenamiento)"; }
        else if (h < s) when = "empieza HOY a las 8:00";
        else when = "es libre hoy; empieza MAÑANA a las 8:00";
        Debug.Log($"[Jornada] Kushal termina su iniciación (huerto y su microcosmos): entra en la rotación del santuario y {when}.");
        LessonPrompt.Show($"Ya eres del santuario. Tu jornada: 5 h de misiones en las áreas, comida a las 11:00 y 2 h de entrenamiento a las 14:00; a las 16:00, libre. Tu jornada {when.Replace("se incorpora", "te incorporas").Replace("es libre", "eres libre")}. (Consola: «jornada»)", null, 14f);
        KushalVoice.Reply(KushalVoice.Ctx.Mission);   // Kushal acepta su jornada (arriba)
        WorkdayGiven?.Invoke(_player);
        return me;
    }

    /// <summary>INCORPORACIÓN a mitad del día: misiones para lo que le queda de sus 5 h (desde la siguiente media hora, sin
    /// partirlas en la comida), primero de áreas distintas y que nadie haga ya hoy; y su entrenamiento. Devuelve cuántas.</summary>
    static int LateJoin(WorkdayDirector wd, WorkRoster.Worker me, int day, float h)
    {
        float s = WorkRoster.StartHour, b1 = WorkRoster.FirstBlock, lunch = WorkRoster.LunchHours;
        float t = Mathf.Ceil(h * 2f - 1e-3f) / 2f;
        float used = t <= s ? 0f : t < s + b1 ? t - s : t < s + b1 + lunch ? b1 : t - s - lunch;
        var taken = new HashSet<string>(wd.today.work.Select(a => a.mission.id));
        var order = wd.today.areaOrder;
        var pool = AreaWork.Today(day, ConstructionProject.All)
            .Where(m => string.IsNullOrEmpty(m.group) && m.site == null && m.id != "enf_iniciacion")
            .OrderBy(m => taken.Contains(m.id) ? 1 : 0)
            .ThenBy(m => { int i = order.IndexOf(m.area); return i < 0 ? 99 : i; })
            .ToList();
        var areas = new HashSet<string>();
        int n = 0;
        while (used < WorkRoster.WorkHours - 1e-3f)
        {
            WorkRoster.Mission pick = pool.FirstOrDefault(m => WorkRoster.Fits(used, m.hours) && !areas.Contains(m.area))
                                   ?? pool.FirstOrDefault(m => WorkRoster.Fits(used, m.hours));
            if (pick == null) break;
            pool.Remove(pick); areas.Add(pick.area);
            float u = used;
            if (u + pick.hours > b1 + 1e-4f && u < b1) u = b1;   // no se parte en la comida (como WorkRoster.Assign)
            float start = u < b1 ? s + u : s + lunch + u;
            wd.today.work.Add(new WorkRoster.Assignment { worker = me, mission = pick, start = start, firstTime = !me.Knows(pick.id) });
            me.count[pick.id] = me.Count(pick.id) + 1; me.lastDay[pick.id] = day;
            used = u + pick.hours; n++;
            Debug.Log($"[Jornada] Incorporación de Kushal: {start:00.0} h «{pick.title}» ({pick.area}).");
        }
        if (wd.roster.trainingAreas.Count > 0)
            wd.today.training[me] = wd.roster.trainingAreas[(wd.roster.workers.IndexOf(me) + day) % wd.roster.trainingAreas.Count];
        return n;
    }

    void NewDay(WorkdayDirector wd, WorkRoster.Worker me)
    {
        _plan = wd.today;
        _records.Clear();
        foreach (WorkRoster.Assignment a in _plan.Of(me)) _records.Add(new Record { a = a });
        AteToday = TrainingDone = _dayClosed = _dineQueued = _lunchSeen = false; _dineTries = 0;
        _lunchHunger = -1f; _trainPresent = _trainObserved = 0f; _eating = null;
        TrainingArea = _plan.training.TryGetValue(me, out string tr) ? tr : null;
        string items = string.Join(" · ", _records.Select(r => $"{r.a.start:00.0} h «{r.a.mission.title}» ({r.a.mission.area}{(r.a.firstTime ? ", nueva" : "")})"));
        Debug.Log($"[Jornada] Día {_plan.day} · itinerario de Kushal: {items} · 11:00 comida · 14:00 entrenamiento: {TrainingArea ?? "—"} · 16:00 libre.");
        if (_records.Count > 0)
            LessonPrompt.Show($"Tu jornada de hoy: {string.Join(" · ", _records.Select(r => $"{r.a.start:00}:00 {r.a.mission.title} ({r.a.mission.area})"))} · 11:00 comida · 14:00 {TrainingArea ?? "entrenamiento"} · 16:00 libre.", null, 12f);
        if (_records.Count > 0) KushalVoice.Reply(KushalVoice.Ctx.Mission);
    }

    // Lo que HACE: tiempo en su puesto de cada misión, la comida y el entrenamiento; a las 16:00, el balance del día.
    void Track(WorkdayDirector wd, WorkRoster.Worker me, float h)
    {
        Vector3 pos = _player.transform.position;
        float dt = Time.deltaTime;
        foreach (Record r in _records)
        {
            WorkRoster.Assignment a = r.a;
            float end = a.start + a.mission.hours;
            if (!r.started && h >= a.start && h < end)
            {
                r.started = true;
                Debug.Log($"[Jornada] {a.start:00.0} h · Kushal: empieza «{a.mission.title}» ({a.mission.area}).");
                LessonPrompt.Show($"Ahora: «{a.mission.title}» en {a.mission.area} (hasta las {end:00}:{(end % 1f) * 60f:00}). Ve a tu puesto: entre torres, en alfombra.", null, 10f);
                KushalVoice.Reply(KushalVoice.Ctx.Mission);
            }
            if (h >= a.start && h < end)
            {
                r.observed += dt;
                if (AtPost(a.mission, a.mission.area, pos)) r.present += dt;
            }
            if (!r.closed && h >= end)
            {
                r.closed = true;
                r.done = r.virtualized || (r.observed > 0f && r.Share >= 0.5f);
                if (r.done) Progress.MarkMission(a.mission.id);
                Debug.Log($"[Jornada] Kushal {(r.done ? "completa" : "NO completa")} «{a.mission.title}» ({a.mission.area}): en su puesto {r.Share:P0} de {r.observed:0} s{(r.virtualized ? ", virtualizada" : "")}.");
            }
        }

        // COMIDA (11–12): comió si el cuerpo se sació en el restaurante (o terminó de comer sentado).
        float s = WorkRoster.StartHour, l0 = s + WorkRoster.FirstBlock, l1 = l0 + WorkRoster.LunchHours;
        BodyMetabolism body = _player.GetComponent<BodyMetabolism>();
        ActionQueue q = _player.GetComponent<ActionQueue>();
        if (h >= l0 - 0.25f && h < l1 + 0.5f && !AteToday)
        {
            if (body != null && h >= l0 && _lunchHunger < 0f) _lunchHunger = body.hunger;
            if (q != null)
            {
                DineSpell now = null;
                foreach (QueuedSpell sp in q.Own) if (sp is DineSpell d) { now = d; break; }
                if (now != null && now.Label.StartsWith("comer ")) _eating = now;
                if (_eating != null && now != _eating) Ate("termina de comer en el restaurante");
            }
            if (!AteToday && body != null && _lunchHunger >= 0f && body.hunger < _lunchHunger - 0.15f) Ate("se sacia a mediodía");
        }

        // ENTRENAMIENTO (14–16): en su sala.
        float t0 = s + WorkRoster.WorkHours + WorkRoster.LunchHours, t1 = t0 + WorkRoster.TrainingHours;
        if (TrainingArea != null && h >= t0 && h < t1)
        {
            _trainObserved += dt;
            if (WorkdayDirector.InWorkspace(null, TrainingArea, pos)) _trainPresent += dt;
        }

        if (!_dayClosed && h >= t1)
        {
            _dayClosed = true;
            TrainingDone = TrainingArea != null && _trainObserved > 0f && TrainingShare >= 0.5f;
            if (TrainingDone) Progress.MarkPractice($"Entrenamiento: {TrainingArea}");
            int done = _records.Count(r => r.done);
            Debug.Log($"[Jornada] 16:00 · fin de la jornada de Kushal (día {_plan.day}): {done}/{_records.Count} misiones, comida {(AteToday ? "sí" : "NO")}, entrenamiento {(TrainingDone ? "sí" : "NO")} ({TrainingArea ?? "—"}, {TrainingShare:P0}). Ahora es LIBRE.");
            LessonPrompt.Show($"Fin de la jornada: {done}/{_records.Count} misiones. Ahora eres libre hasta mañana a las 8:00: ve a donde quieras.", null, 10f);
            Hold(false);
        }
    }

    void Ate(string how)
    {
        if (AteToday) return;
        AteToday = true;
        Debug.Log($"[Jornada] Kushal {how}.");
    }

    /// <summary>¿Está en su puesto? El espacio de la tarea (WorkdayDirector.InWorkspace); la Guardia patrulla todo el santuario.</summary>
    public static bool AtPost(WorkRoster.Mission m, string area, Vector3 pos)
    {
        if (area == AreaWork.Guard) return true;
        return WorkdayDirector.InWorkspace(m, area, pos);
    }


    // ── SALIR ANTES HACIA EL PUESTO SIGUIENTE (2026-10-30) ──────────────────────────────────────────────────────────
    /// <summary>Horas antes del final de una misión en que el piloto puede salir ya hacia la siguiente.</summary>
    public const float EarlyLead = 0.4f;
    /// <summary>Parte de la misión (en tiempo de juego) que tiene que llevar hecha en su puesto para salir antes.</summary>
    public const float EarlyDoneShare = 0.55f;

    /// <summary>
    /// SALE ANTES: si el piloto lleva Kushal (<see cref="autopilot"/>), en los últimos <see cref="EarlyLead"/> h de una misión
    /// que ya tiene hecha (≥ <see cref="EarlyDoneShare"/> de su tiempo en el puesto) y la siguiente empieza justo al acabar
    /// esta en OTRA área, va ya hacia la siguiente: así llega a su hora. Lo pidió el Árbol de los Campos (Berón 2026-10-29:
    /// alfombras solo en la copa y en el suelo; a los pisos, nadando por la raíz): de una torre al huerto del piso 5 se
    /// tarda ~1,5 min de juego (alfombra al pie, andar a la puerta y nadar), más de la mitad de una misión de 1 h a ×6. Es lo
    /// que hace cualquiera que acaba lo suyo: se pone en camino. La Magnate no lo devuelve mientras va (WorkdayDirector).
    /// Devuelve la misión siguiente, o null.
    /// </summary>
    bool _earlyLogged;
    public static WorkRoster.Assignment HeadingEarly(float h)
    {
        PlayerWorkday pw = Instance;
        if (pw == null || !pw.autopilot || pw._plan == null || TimeController.timeController == null) return null;
        Record cur = null;
        foreach (Record r in pw._records) if (h >= r.a.start && h < r.a.start + r.a.mission.hours) { cur = r; break; }
        if (cur == null || cur.virtualized) return null;
        float end = cur.a.start + cur.a.mission.hours;
        if (end - h > EarlyLead) return null;
        Record next = null;
        foreach (Record r in pw._records) if (Mathf.Abs(r.a.start - end) < 0.01f) { next = r; break; }
        if (next == null || next.a.mission.area == cur.a.mission.area || next.a.mission.area == AreaWork.Guard) return null;
        float full = cur.a.mission.hours * TimeController.timeController.SecondsPerGameDay / 24f;
        return cur.present >= EarlyDoneShare * full ? next.a : null;
    }

    // ── PILOTO: lo lleva solo a cada sitio, como la jornada a los demás (PathMover + alfombras entre torres) ─────────────

    void Pilot(WorkdayDirector wd, WorkRoster.Worker me, float h)
    {
        string area = wd.Where(me, h, out string doing);
        if (area == null || MagnateWarden.Instance.Handling(_player) || FlyingCarpet.Riding(_player)) { Hold(false); return; }
        ActionQueue q = _player.GetComponent<ActionQueue>();
        Vector3 pos = _player.transform.position;
        WorkRoster.Mission m = wd.Current(me, h)?.mission;
        WorkRoster.Assignment early = HeadingEarly(h);
        if (early != null)   // ya hizo lo suyo aquí: sale hacia la siguiente misión (llega a su hora)
        {
            if (!_earlyLogged) { _earlyLogged = true; Debug.Log($"[Jornada] {h:00.00} h · Kushal ya hizo «{m?.title}»: sale hacia «{early.mission.title}» ({early.mission.area}) para llegar a su hora."); }
            area = early.mission.area; m = early.mission; doing = early.mission.title;
        }
        else _earlyLogged = false;
        bool lunch = doing == "Comida";
        if (lunch && AtPost(null, area, pos) && !AteToday)
        {
            // Si lo dejó a medias (no llegó a la barra, entre la gente), lo vuelve a intentar: hasta 3 veces en la hora de comer.
            if (_dineQueued && q != null && _dineTries < 3 && !q.Own.Any(s => s is DineSpell)) _dineQueued = false;
            if (!_dineQueued && q != null) { _dineQueued = true; _dineTries++; q.EnqueueOwn(new DineSpell()); Debug.Log($"[Jornada] Kushal va a comer al restaurante{(_dineTries > 1 ? $" (intento {_dineTries})" : "")}."); }
            Hold(false);   // come: lo lleva su cola (pedir, sentarse, comer)
            return;
        }
        if (q != null && q.Own.Count > 0)   // comiendo, al baño…: eso va antes (con tope: si no acaba, vuelve a su puesto)
        {
            if (_need != q.Own[0]) { _need = q.Own[0]; _needSince = Time.time; }
            if (Time.time - _needSince > 240f && !(_need is SleepSpell)) { Debug.Log($"[Jornada] Kushal deja «{_need.Label}» sin acabar: vuelve a su puesto."); q.ClearOwn(); _need = null; }
            Hold(false); return;
        }
        Vector3 target = WorkdayDirector.AreaPoint(area, "Player");
        if (target == Vector3.zero) { Hold(false); return; }
        Vector3 d = target - pos; d.y = 0f;
        if (AtPost(m, area, pos) && d.magnitude < 4f) { Hold(false); return; }
        Hold(true);
        Vector3 to = target;
        if (CarpetPad.Route(_player, ref to)) return;   // otro piso u otra torre: en alfombra
        PathMover.Of(_player.transform).StepToward(to, walkSpeed);
    }

    // Mientras lo lleva, el cuerpo no obedece al teclado (la cámara sigue libre), como en la orquesta.
    void Hold(bool on)
    {
        if (_held == on) return;
        PlayerController pc = _player != null ? _player.GetComponent<PlayerController>() : null;
        if (pc != null) pc.HoldBody(on);
        _held = on;
    }

    /// <summary>Resumen del día para la consola y las pruebas.</summary>
    public string Summary() =>
        $"día {Day}: {_records.Count(r => r.done)}/{_records.Count} misiones (" + string.Join(", ", _records.Select(r => $"«{r.a.mission.title}» {r.Share:P0}{(r.done ? " ✓" : "")}")) +
        $"), comida {(AteToday ? "sí" : "no")}, entrenamiento {(TrainingDone ? "sí" : "no")} ({TrainingArea ?? "—"} {TrainingShare:P0})";
}
