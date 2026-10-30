using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PRUEBA EN VIVO DE LA JORNADA DE KUSHAL (Berón 2026-10-29: «tras el microcosmos de la iniciación del huerto, a Kushal se
/// le da su jornada laboral. Prueba esa jornada en vivo; al terminar el jugador es libre de moverse por el mapa; recorre el
/// mapa mirando qué hacen los demás humanos y animales, y prueba la SEGUNDA jornada»). En tres partes, con el reloj
/// acelerado (×<see cref="speed"/>):
///  1. DÍA 1: con la iniciación completa (huerto incluido) se le DA la jornada (PlayerWorkday); a las 8:00 el reparto lo
///     cuenta. Kushal va solo a cada puesto (el piloto, como la jornada lleva a los demás), come a las 11:00 y entrena a las
///     14:00. A media mañana se ESCAPA a otra torre: la Magnate tiene que devolverlo. Se comprueba que hace sus misiones (en
///     su puesto ≥ 50 % del tiempo), come, entrena, que nadie lo trae o lo mueve sin motivo y qué hace el personal.
///  2. TIEMPO LIBRE: a las 16:00 nadie lo mueve. Recorre andando y en alfombra varias torres y mira qué hacen los humanos y
///     los animales en cada una.
///  3. DÍA 2: nuevo reparto (el orden de áreas rota), Kushal recibe misiones otra vez (alguna nueva) y las completa.
/// Todo el rato un VIGÍA anota anomalías del personal y de la fauna: atascados > 20 s, parados fuera de su puesto en turno,
/// dando vueltas, dentro de un mueble, en el agua sin ser acuáticos, cayendo, volcados y saltos/teletransportes sin motivo.
/// Líneas «[Jornada] …» en el log para quien lo lea. Se añade solo al orquestador cuando se piden las pruebas.
/// </summary>
public class JornadaTest : MonoBehaviour, ITestUnit
{
    public int Group => 50;   // tras las iniciaciones (40–43) y los microcosmos: cambia el reloj y deja a Kushal trabajando
    public bool ParallelSafe => false;
    [Tooltip("Aceleración del tiempo (1 = ritmo real: un día = 1 h).")]
    public float speed = 6f;
    [Tooltip("Torres que recorre en su tiempo libre.")]
    public int towers = 4;
    [Tooltip("Capturas de pantalla en Temp/jornada_*.png en los momentos clave.")]
    public bool shots = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();   // solo está activo si se pidieron las pruebas
        if (tr != null && tr.GetComponentInChildren<JornadaTest>() == null) tr.gameObject.AddComponent<JornadaTest>();
    }

    WorkdayDirector _wd;
    TimeController _tc;
    PlayerWorkday _pw;
    GameObject _player;
    Watch _watch;
    float SecPerHour => _tc.SecondsPerGameDay / 24f;

    public IEnumerator Run()
    {
        TestProbe.Begin("Jornada de Kushal: día 1 → tiempo libre por las torres → día 2");
        _wd = WorkdayDirector.Instance != null ? WorkdayDirector.Instance : FindAnyObjectByType<WorkdayDirector>();
        _tc = TimeController.timeController;
        _player = GameObject.FindGameObjectWithTag("Player");
        SanctuaryOnboarding ob = _player != null ? _player.GetComponent<SanctuaryOnboarding>() : null;
        if (_wd == null || _tc == null || _player == null || ob == null) { TestProbe.Check("hay jornada, reloj y jugador recién llegado", false, $"jornada {_wd != null}, jugador {_player != null}, onboarding {ob != null}"); TestProbe.End(); yield break; }
        _pw = PlayerWorkday.Instance != null ? PlayerWorkday.Instance : new GameObject("PlayerWorkday_AUTO").AddComponent<PlayerWorkday>();

        // ── Kushal sale del microcosmos del huerto con la iniciación completa ──
        if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld) { MobWorldLoader.Instance.ExitMobWorld(); yield return Until(() => !MobWorldLoader.Instance.IsInMobWorld && !MobWorldLoader.Instance.IsBusy, 30f); }
        foreach (AreaInitiator x in FindObjectsByType<AreaInitiator>(FindObjectsSortMode.None)) Destroy(x);
        foreach (NewcomerInitiator ni in FindObjectsByType<NewcomerInitiator>(FindObjectsSortMode.None)) if (ni.InProgress) ni.ResetState();
        HealthRecord hr = _player.GetComponent<HealthRecord>();
        if (hr != null) { hr.analyzed = true; hr.vaccinated = true; }
        ob.introMissionsDone = ob.introMissionsRequired;
        ob.apt = ob.presented = ob.initiated = ob.kitchenTrained = true;
        ob.gardenTrained = false;   // aún en el huerto
        foreach (Magnate mg in FindObjectsByType<Magnate>(FindObjectsSortMode.None)) mg.StopGuiding();   // (otra prueba la dejó acompañándolo a medias)
        if (_player.TryGetComponent(out PlayerController pc0)) pc0.ReleaseBody();
        WorkRoster.Worker old = _wd.roster.Find("Player");
        if (old != null) _wd.roster.workers.Remove(old);   // (otra prueba pudo meterlo): que se le DÉ la jornada aquí
        GameObject garden = GameObject.Find("Garden_Area");
        if (garden != null) Place(_player, WorkdayDirector.AreaPoint("Huerto", "Player"));

        // ── LA MAÑANA EMPIEZA DESCANSADA: en la batería completa las pruebas anteriores (iniciaciones, microcosmos, el
        // servicio del restaurante) dejan correr el reloj y saltar horas sin que nadie coma, duerma ni vaya al baño; el
        // personal llegaba al día 1 con hambre, sueño y ganas de ir al baño desde las 8:00 (todos «pidiendo en la barra»
        // vacía y dormidos a las 11:00). La jornada empieza como empieza un día: con el desayuno hecho y la noche dormida.
        RestDay();

        float oldScale = Time.timeScale;
        int d1 = _tc.Day + 1;
        _tc.SetClock(d1, 7.5f);
        yield return new WaitForSeconds(2f);
        TestProbe.Check("sin terminar el huerto, Kushal aún no está en la rotación", _wd.roster.Find("Player") == null && !PlayerWorkday.Ready(_player));

        int given = 0; System.Action<GameObject> onGiven = g => given++;
        PlayerWorkday.WorkdayGiven += onGiven;
        ob.gardenTrained = true;   // el de turno del huerto lo forma y vuelve de su microcosmos
        PlayerWorkday.NotifyOnboardingComplete();
        yield return Until(() => _wd.roster.Find("Player") != null, 5f);
        PlayerWorkday.WorkdayGiven -= onGiven;
        TestProbe.Check("al terminar la iniciación del huerto se le DA la jornada (entra en la rotación)", _wd.roster.Find("Player") != null && given == 1, $"{given} aviso(s) · {_wd.roster.workers.Count} personas en la rotación");

        // ── DÍA 1 ──
        _watch = new Watch(this);
        Time.timeScale = speed;
        _pw.autopilot = true;
        _tc.SetClock(d1, 7.95f);
        yield return Until(() => { DriveDirector(); return _wd.plannedDay == d1; }, 30f);
        WorkRoster.DayPlan p1 = _wd.today;
        TestProbe.Check("a las 8:00 hay reparto del día 1", p1 != null && _wd.plannedDay == d1, p1 != null ? $"{p1.work.Count} misiones, áreas: {string.Join(" → ", p1.areaOrder)}" : "sin plan");
        if (p1 == null) { Restore(oldScale); TestProbe.End(); yield break; }
        WorkRoster.Worker me = _wd.roster.Find("Player");
        var mine1 = p1.Of(me).ToList();
        TestProbe.Check("Kushal recibe misiones en las áreas (sus 5 h)", mine1.Count > 0 && Mathf.Abs(mine1.Sum(a => a.mission.hours) - WorkRoster.WorkHours) < 1.01f, string.Join(" · ", mine1.Select(a => $"{a.start:00.0} h {a.mission.title} ({a.mission.area})")));
        TestProbe.Check("y un entrenamiento (14:00)", p1.training.ContainsKey(me), p1.training.TryGetValue(me, out string tr1) ? tr1 : "ninguno");
        Log($"día {d1}: plan de Kushal: {string.Join(" · ", mine1.Select(a => $"{a.start:00.0} h «{a.mission.title}» ({a.mission.area})"))} · entrenamiento {tr1}");
        foreach (string line in _wd.Report()) Log("  " + line);

        int returned0 = MagnateWarden.Returned;
        yield return Day(d1, stray: true);
        int strayReturns = _strayReturned ? 1 : 0;
        CheckDay(d1, "día 1", returned0, strayReturns);

        // ── TIEMPO LIBRE ──
        yield return FreeTime();

        // ── DÍA 2 ──
        int d2 = d1 + 1;
        _pw.autopilot = true;
        RestDay();   // el reloj salta la NOCHE (de las 17 h a las 8 h): lo que hace la noche —dormir, y el desayuno— lo hace esto
        _watch.Rebase();
        _tc.SetClock(d2, 7.95f);
        yield return Until(() => { DriveDirector(); _watch.Frame(); return _wd.plannedDay == d2; }, 30f);
        WorkRoster.DayPlan p2 = _wd.today;
        TestProbe.Check("a las 8:00 del día 2 hay un reparto NUEVO", p2 != null && p2 != p1 && _wd.plannedDay == d2, p2 != null ? $"{p2.work.Count} misiones" : "sin plan");
        if (p2 == null || p2 == p1) { Restore(oldScale); TestProbe.End(); yield break; }
        TestProbe.Check("el orden de áreas rota (otra área empieza el día)", p2.areaOrder.Count > 0 && p1.areaOrder.Count > 0 && p2.areaOrder[0] != p1.areaOrder[0], $"{p1.areaOrder.FirstOrDefault()} → {p2.areaOrder.FirstOrDefault()}");
        var mine2 = p2.Of(me).ToList();
        var ids1 = new HashSet<string>(mine1.Select(a => a.mission.id));
        TestProbe.Check("Kushal vuelve a tener misiones el día 2", mine2.Count > 0, string.Join(" · ", mine2.Select(a => $"{a.start:00.0} h {a.mission.title} ({a.mission.area}){(a.firstTime ? " (nueva)" : "")}")));
        TestProbe.Check("la rotación le da alguna misión distinta de las de ayer", mine2.Any(a => !ids1.Contains(a.mission.id)), $"ayer: {string.Join(", ", ids1)} · hoy: {string.Join(", ", mine2.Select(a => a.mission.id))}");
        returned0 = MagnateWarden.Returned;
        yield return Day(d2, stray: false);
        CheckDay(d2, "día 2", returned0, 0);

        // ── Resumen ──
        _watch.Final();
        Restore(oldScale);
        TestProbe.End();
    }

    // ── Un día de trabajo (8:00 → 16:00), con el vigía y, si toca, la prueba de escaparse ──────────────────────────────
    bool _strayReturned, _strayDone;
    IEnumerator Day(int day, bool stray)
    {
        _strayDone = !stray; _strayReturned = false;
        _watch.BeginDay(day);
        float limit = Time.time + (16.05f - Mathf.Min(_tc.Hour, 16f)) * SecPerHour + 120f;
        var shotsAt = new List<float> { 8.6f, 11.4f, 14.5f };
        float lastSnap = -1f;
        while (Time.time < limit && !(_tc.Day == day && _tc.Hour >= 16.02f) && _tc.Day <= day)
        {
            DriveDirector();
            _watch.Frame();
            float h = _tc.Hour;
            if (h >= 10.85f && h < 11f) { BodyMetabolism b = _player.GetComponent<BodyMetabolism>(); if (b != null && b.hunger < 0.6f) b.hunger = 0.6f; }   // a mediodía tiene hambre
            if (shots && shotsAt.Count > 0 && h >= shotsAt[0]) { Shot($"d{day}_{shotsAt[0]:00.0}h"); shotsAt.RemoveAt(0); }
            if (Mathf.Floor(h) != lastSnap && h >= 9f) { lastSnap = Mathf.Floor(h); _watch.Snapshot($"{h:00.00} h (turno)"); }
            if (!_strayDone && h >= 9.25f && h < 10.9f && StrayReady(out string why))
            {
                _strayDone = true;
                yield return Stray(why);
            }
            yield return null;
        }
        _watch.EndDay();
        if (!_strayDone) TestProbe.Check("prueba de escaparse: hubo un momento en su puesto para intentarlo", false, "no estuvo en su puesto entre las 9:15 y las 11:00");
    }

    void CheckDay(int day, string tag, int returnedBefore, int expectedReturns)
    {
        var recs = _pw.Records.ToList();
        TestProbe.Check($"{tag}: PlayerWorkday sigue la jornada de Kushal", _pw.Day == day && recs.Count > 0, _pw.Summary());
        string per = string.Join(" · ", recs.Select(r => $"«{r.a.mission.title}» {r.Share:P0}{(r.done ? " ✓" : " ✗")}"));
        TestProbe.Check($"{tag}: Kushal completa TODAS sus misiones (≥ 50 % del tiempo en su puesto)", recs.Count > 0 && recs.All(r => r.done), per);
        TestProbe.Check($"{tag}: come en el restaurante (11–12 h)", _pw.AteToday);
        TestProbe.Check($"{tag}: hace su entrenamiento (14–16 h)", _pw.TrainingDone, $"{_pw.TrainingArea} {_pw.TrainingShare:P0}");
        TestProbe.Check($"{tag}: a las 16:00 su jornada se cierra (libre)", _pw.DayClosed);
        int extra = MagnateWarden.Returned - returnedBefore - expectedReturns;
        TestProbe.Check($"{tag}: la Magnate no tuvo que traerlo (aparte de la escapada): llega solo a cada puesto", extra <= 0, $"{MagnateWarden.Returned - returnedBefore} vez/veces en total, {expectedReturns} esperada(s)");
        _watch.CheckShift(tag);
    }

    // ── ESCAPADA: en su puesto a media mañana, se va a otra torre; la Magnate lo tiene que devolver ──
    bool StrayReady(out string why)
    {
        why = null;
        WorkRoster.Worker me = _wd.roster.Find("Player"); if (me == null) return false;
        float h = _tc.Hour;
        WorkRoster.Assignment cur = _wd.Current(me, h);
        if (cur == null || cur.mission.area == AreaWork.Guard || FlyingCarpet.Riding(_player) || MagnateWarden.Instance.Handling(_player)) return false;
        // Ya pasó el rato para llegar: el que da la Magnate (WorkdayDirector), que cuenta desde que ESTE es su puesto (si salió antes
        // hacia él —PlayerWorkday.HeadingEarly—, desde entonces; antes se contaba desde la hora de la misión).
        if (Time.time - _wd.PlayerPostSince < PlayerWorkday.Commute + 15f) return false;
        if (!PlayerWorkday.AtPost(cur.mission, cur.mission.area, _player.transform.position)) return false;
        why = $"«{cur.mission.title}» ({cur.mission.area})";
        return true;
    }

    IEnumerator Stray(string post)
    {
        WorkRoster.Worker me = _wd.roster.Find("Player");
        WorkRoster.Assignment cur = _wd.Current(me, _tc.Hour);
        SanctuaryArea far = null; float best = -1f;
        foreach (SanctuaryArea a in FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None))
        {
            if (a == null || string.IsNullOrEmpty(a.displayName)) continue;
            Vector3 pt = WorkdayDirector.AreaPoint(a.displayName, "Player");
            if (pt == Vector3.zero || WorkdayDirector.InWorkspace(cur.mission, cur.mission.area, pt)) continue;
            float d = Vector3.Distance(pt, _player.transform.position);
            if (d > best) { best = d; far = a; }
        }
        if (far == null) { TestProbe.Check("prueba de escaparse: hay otra torre adonde irse", false); yield break; }
        _pw.autopilot = false;
        int before = MagnateWarden.Returned;
        Log($"{_tc.Hour:00.00} h · Kushal se escapa de {post} a «{far.displayName}» ({best:0} m).");
        Place(_player, WorkdayDirector.AreaPoint(far.displayName, "Player"));
        _watch.IgnorePlayerJump();
        float t0 = Time.time;
        yield return Until(() => { DriveDirector(); _watch.Frame(); return MagnateWarden.Returned > before; }, 40f);
        float took = Time.time - t0;
        yield return Until(() => { DriveDirector(); _watch.Frame(); return !MagnateWarden.Instance.Handling(_player); }, 15f);
        bool back = PlayerWorkday.AtPost(cur.mission, cur.mission.area, _player.transform.position);
        _strayReturned = MagnateWarden.Returned > before;
        TestProbe.Check("en turno, si se escapa la Magnate aparece y lo devuelve a su puesto", _strayReturned && back, $"devuelto en {took:0.0} s de juego; ahora {(back ? "en su puesto" : "FUERA de su puesto")} ({Where(_player.transform.position)})");
        if (shots) Shot($"magnate_devuelve");
        _pw.autopilot = true;
    }

    // ── TIEMPO LIBRE: nadie lo mueve; recorre torres y mira qué hacen los demás ─────────────────────────────────────────
    IEnumerator FreeTime()
    {
        _pw.autopilot = false;
        _watch.MarkPhase();
        _watch.Rebase();
        int returned0 = MagnateWarden.Returned;
        Vector3 at = _player.transform.position;
        yield return Until(() => { DriveDirector(); _watch.Frame(); return false; }, 15f);
        float drift = Flat(_player.transform.position - at);
        TestProbe.Check("a las 16:00 es libre: nada lo mueve (ni la jornada ni la Magnate)", drift < 1f && !MagnateWarden.Instance.Handling(_player), $"se movió {drift:0.0} m solo");
        _watch.Snapshot($"{_tc.Hour:00.00} h (libre)");

        // Torres: un área por edificio (de alfombras), de la más lejana a la más cercana a cada paso.
        var byBuilding = new Dictionary<string, SanctuaryArea>();
        foreach (SanctuaryArea a in FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None))
        {
            if (a == null || string.IsNullOrEmpty(a.displayName) || WorkdayDirector.AreaPoint(a.displayName, "Player") == Vector3.zero) continue;
            CarpetPad pad = CarpetPad.For(a.transform.position);
            string key = pad != null && !string.IsNullOrEmpty(pad.building) ? pad.building : a.displayName;
            if (!byBuilding.TryGetValue(key, out SanctuaryArea cur) || a.transform.position.y < cur.transform.position.y) byBuilding[key] = a;
        }
        string here = Building(_player.transform.position);
        var left = byBuilding.Where(kv => kv.Key != here).ToList();
        var route = new List<KeyValuePair<string, SanctuaryArea>>();
        Vector3 from = _player.transform.position;
        while (route.Count < towers && left.Count > 0)
        {
            var next = left.OrderByDescending(kv => Vector3.Distance(kv.Value.transform.position, from)).First();
            route.Add(next); left.Remove(next); from = next.Value.transform.position;
        }
        Log($"tiempo libre: recorre {route.Count} torres: {string.Join(" → ", route.Select(kv => $"{kv.Key} ({kv.Value.displayName})"))}");
        PlayerController pc = _player.GetComponent<PlayerController>();
        if (pc != null) pc.HoldBody(true);
        int reached = 0;
        foreach (var kv in route)
        {
            SanctuaryArea target = kv.Value;
            Vector3 goal = WorkdayDirector.AreaPoint(target.displayName, "Player");
            float t0 = Time.time, limit = t0 + 300f, lastMove = Time.time; Vector3 lastPos = _player.transform.position;
            bool ok = false, stuck = false;
            while (Time.time < limit)
            {
                DriveDirector(); _watch.Frame();
                Vector3 pos = _player.transform.position;
                if (target.InBuilding(pos) && Flat(goal - pos) < 6f) { ok = true; break; }
                if (!FlyingCarpet.Riding(_player))
                {
                    Vector3 to = goal;
                    if (!CarpetPad.Route(_player, ref to)) PathMover.Of(_player.transform).StepToward(to, 2.6f);
                }
                if (Vector3.Distance(pos, lastPos) > 0.5f || FlyingCarpet.Riding(_player)) { lastPos = pos; lastMove = Time.time; }
                else if (Time.time - lastMove > 30f) { stuck = true; break; }
                yield return null;
            }
            if (ok) reached++;
            Log($"{_tc.Hour:00.00} h · Kushal {(ok ? "llega a" : stuck ? "se ATASCA camino de" : "NO llega a")} la torre {kv.Key} («{target.displayName}») en {Time.time - t0:0} s{(ok ? "" : $"; está en {Where(_player.transform.position)}")}.");
            if (!ok) _watch.Add("Kushal", stuck ? "atascado (tiempo libre)" : "no llega", $"camino de {kv.Key} ({target.displayName}), en {Where(_player.transform.position)}", true);
            _watch.Tower(kv.Key, target);
            if (shots) Shot($"torre_{Safe(kv.Key)}");
        }
        if (pc != null) pc.HoldBody(false);
        TestProbe.Check("en su tiempo libre recorre las torres (andando y en alfombra)", route.Count > 0 && reached >= Mathf.Min(3, route.Count), $"{reached}/{route.Count} torres");
        TestProbe.Check("en su tiempo libre la Magnate no lo trae de vuelta", MagnateWarden.Returned == returned0, $"{MagnateWarden.Returned - returned0} vez/veces");
        _watch.Snapshot($"{_tc.Hour:00.00} h (libre, tras el recorrido)");
        _watch.CheckFree();
    }

    // ── El reloj de la jornada: con el orquestador en marcha WorkdayDirector no se mueve solo (Update), lo mueve la prueba ──
    void DriveDirector()
    {
        if (!ColdSanctuary.TestRunner.Running) return;   // (fuera del orquestador lo hace su Update)
        float h = _tc.Hour; int d = _tc.Day;
        if (_wd.plannedDay != d && h >= WorkRoster.StartHour && h < WorkRoster.StartHour + 8f) _wd.PlanDay(d);
        if (_wd.today != null && _wd.plannedDay == d) _wd.Tick(h);
    }

    /// <summary>Cada persona del personal (y Kushal) despierta, de pie, sin hambre, sin sueño y sin ganas de ir al baño: el
    /// estado de una mañana, sea cual sea la prueba que corrió antes (o la noche que se salta el reloj).</summary>
    void RestDay()
    {
        int n = 0;
        foreach (string who in _wd.staff)
        {
            GameObject g = WorkdayDirector.Body(who);
            if (g == null || !g.TryGetComponent(out Anima a) || a.death) continue;
            if (g.TryGetComponent(out BodyMetabolism b)) { b.hunger = 0.1f; b.toilet = 0.05f; }
            a.sleepiness = 0.05f; a.aware = false;
            // (lo que tenga a medias en su cola lo termina solo: dormido en su cama se despierta al ver el sueño a 0 y se
            // levanta; en la cabina del aseo, sale; quitárselo de golpe lo dejaba tumbado en la cama o dentro de la cabina)
            bool sleeping = g.TryGetComponent(out ActionQueue q) && q.Own.Any(s => s is SleepSpell);
            if (a.asleep && !sleeping) { a.asleep = false; DailyPoses.Stand().ApplyFinal(a); g.GetComponentInChildren<CubeFace>()?.Show(CubeFace.Expression.Neutral, 0.1f); }
            n++;
        }
        Log($"mañana: {n} personas descansadas y desayunadas (las pruebas anteriores dejan el reloj correr sin comer ni dormir).");
    }

    void Restore(float scale)
    {
        _wd?.ReleaseGym();   // nadie se queda sentado en una máquina con el agarre a su medida
        foreach (VetCare v in FindObjectsByType<VetCare>(FindObjectsSortMode.None)) v.enabled = false;   // el turno del hospital acabó: nadie sigue yendo a la perrera en las pruebas siguientes
        Time.timeScale = scale;
        if (_pw != null) _pw.autopilot = false;
    }

    static void Place(GameObject g, Vector3 p)
    {
        if (p == Vector3.zero) return;
        WorkdayDirector.Teleport(g.transform, p);
    }

    static void Log(string s) => Debug.Log("[Jornada] " + s);

    static void Shot(string tag)
    {
        string path = $"Temp/jornada_{Safe(tag)}.png";
        ScreenCapture.CaptureScreenshot(path);
        Log($"captura: {path}");
    }

    static string Safe(string s) { var sb = new System.Text.StringBuilder(); foreach (char c in s) sb.Append(char.IsLetterOrDigit(c) ? c : '_'); return sb.ToString(); }

    static float Flat(Vector3 v) { v.y = 0f; return v.magnitude; }

    /// <summary>En qué área (y torre) está un punto.</summary>
    public static string Where(Vector3 p)
    {
        foreach (SanctuaryArea a in SceneCache.All<SanctuaryArea>(2f)) if (a != null && a.Contains(p, 1f)) return a.displayName;
        foreach (SanctuaryArea a in SceneCache.All<SanctuaryArea>(2f)) if (a != null && a.InBuilding(p)) return $"edificio de {a.displayName}";
        return $"fuera de las áreas ({p.x:0},{p.y:0},{p.z:0})";
    }

    static string Building(Vector3 p) { CarpetPad pad = CarpetPad.For(p); return pad != null ? pad.building : ""; }

    static IEnumerator Until(System.Func<bool> ok, float maxGameSeconds)
    {
        float end = Time.time + maxGameSeconds;
        while (!ok() && Time.time < end) yield return null;
    }

    // ══════════════════════════════════════════════════════════════════════════════════════════════════════════════
    // EL VIGÍA: qué hace cada persona y cada animal, y sus anomalías.
    // ══════════════════════════════════════════════════════════════════════════════════════════════════════════════
    class Track
    {
        public GameObject g; public string name; public bool human, player;
        public Anima anima; public Animal animal; public Renderer[] rends;
        public Vector3 lastFrame; public bool lastRiding; public int ignoreFrames;
        public readonly List<(float t, Vector3 p, bool riding)> hist = new List<(float, Vector3, bool)>();
        public int inside, water, tilted; public string insideOf;
        public float offSince = -1f, rodeAt = -99f, movedAt = -99f;   // fuera de su puesto desde; última vez en alfombra; último teletransporte de la prueba
        public string activity = "";
        public string headLabel; public float headSince;   // lo que hace (cabeza de su cola) y desde cuándo
    }

    class Anomaly { public string key, who, kind, detail, firstAt; public int count; public bool human; }

    class Watch
    {
        readonly JornadaTest _t;
        readonly List<Track> _tracks = new List<Track>();
        readonly Dictionary<string, Anomaly> _anom = new Dictionary<string, Anomaly>();
        Dictionary<string, int> _base = new Dictionary<string, int>();   // cuántas había al empezar la fase (día 1, libre, día 2)
        int Since(Anomaly a) => a.count - (_base.TryGetValue(a.key, out int b) ? b : 0);
        public void MarkPhase() { _base = _anom.ToDictionary(kv => kv.Key, kv => kv.Value.count); }
        float _nextSample;
        // Turno: presencia de cada persona en su puesto (misiones, comida y entrenamiento) y quién comió.
        readonly Dictionary<WorkRoster.Assignment, Vector2> _shift = new Dictionary<WorkRoster.Assignment, Vector2>();   // x = presente, y = observado (muestras)
        readonly Dictionary<string, Vector2> _training = new Dictionary<string, Vector2>();
        readonly HashSet<string> _ate = new HashSet<string>();
        int _shiftSamples, _shiftPlausible;
        public int Count => _anom.Values.Sum(a => a.count);

        public Watch(JornadaTest t)
        {
            _t = t;
            foreach (string n in t._wd.staff.Concat(new[] { "Magnate" }))
            {
                GameObject g = WorkdayDirector.Body(n);
                if (g == null || _tracks.Any(x => x.g == g)) continue;
                _tracks.Add(New(g, true, g.CompareTag("Player") ? "Kushal" : n));
            }
            int animals = 0;
            foreach (Animal a in FindObjectsByType<Animal>(FindObjectsSortMode.None))
            {
                if (a == null || a.death) continue;
                if (++animals > 200) break;
                _tracks.Add(New(a.gameObject, false, $"{a.SpeciesName} «{a.name}» #{animals}"));   // (muchos se llaman igual: «Penguin(Clone)»)
            }
            Log($"vigía: {_tracks.Count(x => x.human)} personas y {_tracks.Count(x => !x.human)} animales.");
        }

        static Track New(GameObject g, bool human, string name)
        {
            var tr = new Track { g = g, name = name, human = human, player = g.CompareTag("Player"), anima = g.GetComponent<Anima>(), animal = g.GetComponent<Animal>(), lastFrame = g.transform.position };
            tr.rends = g.GetComponentsInChildren<Renderer>().Where(r => !(r is ParticleSystemRenderer) && !(r is LineRenderer)).ToArray();
            return tr;
        }

        public void Rebase() { foreach (Track tr in _tracks) if (tr.g != null) { tr.lastFrame = tr.g.transform.position; tr.hist.Clear(); tr.ignoreFrames = 3; } }
        public void IgnorePlayerJump() { foreach (Track tr in _tracks) if (tr.player) { tr.ignoreFrames = 4; tr.movedAt = Time.time; } }

        public void Add(string who, string kind, string detail, bool human)
        {
            string key = who + "|" + kind;
            if (!_anom.TryGetValue(key, out Anomaly a))
            {
                _anom[key] = a = new Anomaly { key = key, who = who, kind = kind, detail = detail, firstAt = $"día {_t._tc.Day} {_t._tc.Hour:00.00} h", human = human };
            }
            if (Since(a) == 0) { a.detail = detail; a.firstAt = $"día {_t._tc.Day} {_t._tc.Hour:00.00} h"; Debug.LogWarning($"[Jornada] ANOMALÍA · {who}: {kind} — {detail} ({a.firstAt})"); }   // la primera de cada fase
            a.count++;
        }

        // Cada fotograma: saltos y teletransportes de las personas (andar no da para más de unos metros por fotograma).
        public void Frame()
        {
            foreach (Track tr in _tracks)
            {
                if (!tr.human || tr.g == null) continue;
                if (tr.g.GetComponent<Magnate>() != null) continue;   // la Magnate SÍ se teletransporta (aparece delante de quien deja su puesto)
                Vector3 p = tr.g.transform.position;
                bool riding = FlyingCarpet.Riding(tr.g) || TreeTrip.Busy(tr.g);   // (alfombra o nadando por la raíz del Árbol de los Campos: lo lleva el viaje)
                if (riding) tr.rodeAt = Time.time;
                float d = Vector3.Distance(p, tr.lastFrame), allowed = Mathf.Max(2.5f, 8f * Time.deltaTime);
                if (tr.ignoreFrames > 0) tr.ignoreFrames--;
                else if (d > allowed && Time.time - tr.rodeAt > 1f && !MagnateWarden.Instance.Handling(tr.g))   // (subir y bajar de la alfombra la coloca: 1 s de margen)
                    Add(tr.name, "salto/teletransporte sin motivo", $"{d:0.0} m en un fotograma: de {Where(tr.lastFrame)} a {Where(p)}", true);
                tr.lastFrame = p; tr.lastRiding = riding;
            }
            if (Time.time >= _nextSample) { _nextSample = Time.time + 1f; Sample(); }
        }

        void Sample()
        {
            float now = Time.time, h = _t._tc.Hour;
            bool shift = _t._wd.today != null && _t._wd.plannedDay == _t._tc.Day && h >= WorkRoster.StartHour && h < WorkRoster.StartHour + 8f;
            foreach (Track tr in _tracks)
            {
                if (tr.g == null || (tr.anima != null && tr.anima.death)) continue;
                Vector3 p = tr.g.transform.position;
                bool riding = FlyingCarpet.Riding(tr.g) || TreeTrip.Busy(tr.g) || MagnateWarden.Instance.Handling(tr.g);   // (la Magnate lo trae: no es andar)
                tr.hist.Add((now, p, riding));
                while (tr.hist.Count > 0 && now - tr.hist[0].t > 25f) tr.hist.RemoveAt(0);
                if (tr.human) SampleHuman(tr, p, riding, h, shift); else SampleAnimal(tr, p, riding);
            }
        }

        // Ventana de los últimos 20 s: recorrido y desplazamiento neto (null si aún no hay 20 s o voló en alfombra).
        static bool Window(Track tr, out float path, out float net)
        {
            path = net = 0f;
            if (tr.hist.Count < 2 || tr.hist[tr.hist.Count - 1].t - tr.hist[0].t < 19f) return false;
            int i0 = tr.hist.FindIndex(x => tr.hist[tr.hist.Count - 1].t - x.t <= 20.5f);
            if (i0 < 0) return false;
            for (int i = i0; i < tr.hist.Count; i++) { if (tr.hist[i].riding) return false; if (i > i0) path += Vector3.Distance(tr.hist[i].p, tr.hist[i - 1].p); }
            net = Flat(tr.hist[tr.hist.Count - 1].p - tr.hist[i0].p);
            return true;
        }

        static Bounds BodyBounds(Track tr)
        {
            Bounds b = new Bounds(tr.g.transform.position, Vector3.zero); bool any = false;
            foreach (Renderer r in tr.rends) { if (r == null || !r.enabled) continue; if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
            return b;
        }

        static string InsideOf(Vector3 c, float r, Transform self)
        {
            foreach (Collider col in Physics.OverlapSphere(c, r, ~0, QueryTriggerInteraction.Ignore))
            {
                if (col == null || col.transform.IsChildOf(self) || col is TerrainCollider || col is CharacterController) continue;
                if (col.GetComponentInParent<Anima>() != null) continue;
                if (col.attachedRigidbody != null && !col.attachedRigidbody.isKinematic) continue;   // cosas sueltas (comida, herramientas)
                return col.name;
            }
            return null;
        }

        static string Head(GameObject g)
        {
            ActionQueue q = g.GetComponent<ActionQueue>();
            if (q == null) return null;
            if (q.Own.Count > 0) return q.Own[0].Label;
            if (q.Player.Count > 0) return q.Player[0].Label;
            return null;
        }

        static bool Walking(string label) => label != null && (label.StartsWith("ir a") || label.StartsWith("pedir") || label.StartsWith("buscar"));

        void SampleHuman(Track tr, Vector3 p, bool riding, float h, bool shift)
        {
            float now = Time.time;
            bool magnate = tr.g.GetComponent<Magnate>() != null;   // ella SÍ se teletransporta: no cuenta como vueltas ni caídas
            WorkdayDirector wd = _t._wd;
            WorkRoster.Worker w = wd.roster.Find(tr.player ? "Player" : tr.g.name.Replace("_Post", ""));
            string doing = null, area = shift && w != null ? wd.Where(w, h, out doing) : null;
            WorkRoster.Assignment cur = shift && w != null ? wd.Current(w, h) : null;
            bool handled = MagnateWarden.Instance.Handling(tr.g);
            string head = Head(tr.g);
            bool needs = head != null && (head.StartsWith("comer") || head.StartsWith("pedir") || head.StartsWith("buscar mesa") || head.StartsWith("levantarse") || head.Contains("en la barra") || head.Contains("baño") || head.Contains("aseo") || head.Contains("dormir") || head.Contains("cama"));
            bool asleep = tr.anima != null && tr.anima.asleep;
            bool atPost = area != null && PlayerWorkday.AtPost(cur?.mission, area, p);
            bool full = Window(tr, out float path, out float net);
            bool moving = !full || path > 1f;
            string status = StatusOf(tr.g);
            // LO QUE HACE (su cola; si no, su cartel; si no, la conducta propia de la doctora o del veterinario): «atascado» y
            // «da vueltas» cuentan solo si hace LO MISMO toda la ventana (ir a algo, hacerlo y volver no es dar vueltas).
            string doingNow = head ?? status ?? tr.g.GetComponent<Linduarda>()?.Doing ?? (tr.g.TryGetComponent(out VetCare vc) && vc.enabled ? vc.Doing : null);
            if (doingNow != tr.headLabel) { tr.headLabel = doingNow; tr.headSince = now; }
            tr.activity = asleep ? "durmiendo" : riding ? "en alfombra" : handled ? "con la Magnate" : head ?? (status ?? (moving ? "andando" : "quieto"));

            if (shift && area != null && w != null)
            {
                _shiftSamples++;
                bool plausible = atPost || riding || handled || asleep || needs;
                if (plausible) _shiftPlausible++;
                if (cur != null && !tr.player) { _shift.TryGetValue(cur, out Vector2 v); v.y++; if (atPost) v.x++; _shift[cur] = v; }
                if (doing != null && doing.StartsWith("Entrenamiento")) { _training.TryGetValue(tr.name, out Vector2 v); v.y++; if (atPost) v.x++; _training[tr.name] = v; }
                if (plausible) tr.offSince = -1f; else if (tr.offSince < 0f) tr.offSince = now;
                if (full && !plausible && path < 0.5f && now - tr.offSince >= 20f)   // 20 s FUERA y quieto (no al cambiar de puesto)
                    Add(tr.name, "parado fuera de su puesto en turno", $"le toca «{doing}» en {area}; está en {Where(p)} sin moverse 20 s ({tr.activity})", true);
            }
            else tr.offSince = -1f;
            if (head != null && head.StartsWith("comer") && h >= 10.9f && h < 12.3f) _ate.Add(tr.name);

            if (full && !riding && !handled && !asleep && !magnate)
            {
                if (Walking(head) && path < 0.5f && now - tr.headSince >= 19f) Add(tr.name, "atascado > 20 s", $"«{head}» sin avanzar en {Where(p)}", true);
                bool rounds = (cur != null && cur.mission.id.StartsWith("yog_corrige")) || (status != null && (status.Contains("Gimnasio") || status.Contains("Entrenamiento")));
                if (!rounds && head != "merodear" && path > 12f && net < 2.5f && now - tr.headSince >= 19f)   // (merodear es ir y venir al azar a propósito; ir a algo, hacerlo y volver —la barra, el aseo— cambia lo que hace: eso no es dar vueltas)
                    Add(tr.name, "da vueltas en círculo", $"{path:0} m en 20 s para {net:0.0} m netos en {Where(p)} ({tr.activity})", true);
            }
            if (!magnate) CheckBody(tr, p, riding, asleep || needs || (status != null && status.Contains("Gimnasio")) || wd.OnGymMachine(tr.g), true);
        }

        void SampleAnimal(Track tr, Vector3 p, bool riding)
        {
            Animal a = tr.animal; if (a == null) return;
            bool habit = a.TryGetComponent(out Habit hb) && hb.Active;
            bool marine = a.TryGetComponent(out MarineLife ml);
            bool inHabitat = marine && ml.InHabitat;
            string desire = a.TryGetComponent(out Volition vo) ? vo.LastDesire : "";
            bool resting = a.asleep || a.busy || habit || inHabitat || desire == "rest" || desire == "brood" || desire == "nurse" || desire == "tend" || desire == "habit" || desire == "";
            bool full = Window(tr, out float path, out float net);
            tr.activity = a.asleep ? "durmiendo" : a.busy ? "comiendo/cazando" : habit ? $"hábito: {hb.Doing}" : inHabitat ? "en su hábitat" : full && path > 1f ? $"moviéndose ({desire})" : $"quieto ({desire})";
            bool flier = a.AirAffinity >= 0.5f;
            if (full && !resting && path < 0.5f) Add(tr.name, "atascado > 20 s", $"quiere «{desire}» y no se mueve en {Where(p)}", false);
            if (full && !flier && !marine && !a.busy && path > 15f && net < 2f) Add(tr.name, "da vueltas en círculo", $"{path:0} m en 20 s para {net:0.0} m netos ({tr.activity})", false);
            CheckBody(tr, p, riding, a.asleep || a.busy, false);
        }

        // Dentro de un mueble, en el agua sin ser acuático, cayendo y volcado (con 3 muestras seguidas, para no contar un roce).
        void CheckBody(Track tr, Vector3 p, bool riding, bool lyingOk, bool human)
        {
            Bounds b = BodyBounds(tr);
            float size = Mathf.Max(b.size.x, b.size.y, b.size.z);
            float r = Mathf.Clamp(Mathf.Min(b.extents.x, b.extents.y, b.extents.z) * 0.5f, 0.02f, 0.12f);
            string inside = riding ? null : InsideOf(b.center, r, tr.g.transform);
            if (inside != null && !lyingOk) { tr.inside++; tr.insideOf = inside; } else tr.inside = 0;
            if (tr.inside == 3) Add(tr.name, "dentro de un objeto", $"«{tr.insideOf}» en {Where(p)} ({tr.activity})", human);

            bool aquatic = !human && (tr.animal != null && (tr.animal.WaterAffinity >= 0.5f || tr.animal.GetComponent<MarineLife>() != null));
            if (!aquatic && !riding && b.min.y < -0.35f) tr.water++; else tr.water = 0;
            if (tr.water == 3) Add(tr.name, "en el agua sin ser acuático", $"pies a y = {b.min.y:0.00} en {Where(p)}", human);

            if (tr.hist.Count >= 2 && !riding && Time.time - tr.movedAt > 3f && !MagnateWarden.Instance.Handling(tr.g))
            {
                Vector3 prev = tr.hist[tr.hist.Count - 2].p;
                float drop = prev.y - p.y;
                bool flier = tr.animal != null && (tr.animal.AirAffinity >= 0.5f || size < 0.3f);   // (insectos: vuelan o trepan)
                bool teleport = Flat(p - prev) > 10f;   // eso es un salto (ya contado), no una caída
                if (drop > 4f && !flier && !teleport && !tr.hist[tr.hist.Count - 2].riding) Add(tr.name, "cae", $"{drop:0.0} m en 1 s, ahora en {Where(p)}", human);
            }
            if (p.y < -15f) Add(tr.name, "bajo el mundo", $"y = {p.y:0}", human);

            bool tiltCheck = human || (size > 0.3f && tr.animal != null && tr.animal.AirAffinity < 0.5f);
            float tilt = Vector3.Angle(tr.g.transform.up, Vector3.up);
            if (tiltCheck && !lyingOk && !riding && tilt > (human ? 45f : 60f) && !(tr.anima != null && tr.anima.asleep)) tr.tilted++; else tr.tilted = 0;
            if (tr.tilted == 3) Add(tr.name, "volcado / tumbado mal", $"{tilt:0}° respecto a la vertical en {Where(p)} ({tr.activity})", human);
        }

        static string StatusOf(GameObject g)
        {
            WorldLabel wl = g.GetComponent<WorldLabel>();
            if (wl == null || wl.text == null) return null;
            int i = wl.text.IndexOf('<'); int j = wl.text.LastIndexOf('>');
            return i >= 0 && j > i ? wl.text.Substring(i + 1, j - i - 1) : null;
        }

        // ── Informes ──
        public void Snapshot(string when)
        {
            var lines = new List<string>();
            foreach (Track tr in _tracks)
            {
                if (!tr.human || tr.g == null) continue;
                WorkRoster.Worker w = _t._wd.roster.Find(tr.player ? "Player" : tr.g.name.Replace("_Post", ""));
                string area = w != null ? _t._wd.Where(w, _t._tc.Hour, out string doing) : null;
                string task = area != null ? $"«{_t._wd.Current(w, _t._tc.Hour)?.mission.title ?? "—"}» ({area})" : "libre";
                bool atPost = area != null && PlayerWorkday.AtPost(_t._wd.Current(w, _t._tc.Hour)?.mission, area, tr.g.transform.position);
                lines.Add($"{tr.name}: {task}{(area != null ? (atPost ? " · en su puesto" : " · FUERA de su puesto") : "")} · {tr.activity} · en {Where(tr.g.transform.position)}");
            }
            Log($"── {when}: el personal ──");
            foreach (string l in lines) Log("  " + l);
            Log("  fauna: " + AnimalSummary(null, 0f));
        }

        string AnimalSummary(Vector3? near, float radius)
        {
            var by = new SortedDictionary<string, Dictionary<string, int>>();
            foreach (Track tr in _tracks)
            {
                if (tr.human || tr.animal == null || tr.g == null || tr.animal.death) continue;
                if (near.HasValue && Vector3.Distance(near.Value, tr.g.transform.position) > radius) continue;
                string sp = tr.animal.SpeciesName ?? "?";
                if (!by.TryGetValue(sp, out var d)) by[sp] = d = new Dictionary<string, int>();
                string act = tr.activity.Split(' ')[0].TrimEnd(':');
                d[act] = d.TryGetValue(act, out int c) ? c + 1 : 1;
            }
            if (by.Count == 0) return "ninguna cerca";
            return string.Join(" · ", by.Select(kv => $"{kv.Key} {kv.Value.Values.Sum()} ({string.Join(", ", kv.Value.Select(x => $"{x.Key} {x.Value}"))})"));
        }

        public void Tower(string building, SanctuaryArea a)
        {
            var who = _tracks.Where(tr => tr.human && tr.g != null && !tr.player && a.InBuilding(tr.g.transform.position)).Select(tr => $"{tr.name} ({tr.activity})").ToList();
            Log($"  en la torre {building}: {(who.Count > 0 ? string.Join(", ", who) : "nadie del personal")} · fauna a 40 m: {AnimalSummary(_t._player.transform.position, 40f)}");
        }

        public void BeginDay(int day) { _shift.Clear(); _training.Clear(); _ate.Clear(); _shiftSamples = _shiftPlausible = 0; MarkPhase(); Rebase(); }
        public void EndDay() { }

        public void CheckShift(string tag)
        {
            int total = 0, ok = 0; var bad = new List<string>();
            foreach (var kv in _shift)
            {
                if (kv.Value.y < 3f) continue;
                total++;
                float share = kv.Value.x / kv.Value.y;
                if (share >= 0.5f) ok++; else if (bad.Count < 12) bad.Add($"{kv.Key.worker.name} «{kv.Key.mission.title}» {share:P0}");
            }
            TestProbe.Check($"{tag}: el personal hace sus misiones en su puesto (≥ 80 % de las misiones con ≥ 50 % del tiempo allí)", total > 0 && ok >= 0.8f * total, $"{ok}/{total}" + (bad.Count > 0 ? "; fuera: " + string.Join(", ", bad) : ""));
            float pl = _shiftSamples > 0 ? _shiftPlausible / (float)_shiftSamples : 0f;
            TestProbe.Check($"{tag}: en turno cada uno está en su puesto o haciendo algo razonable (comer, baño, dormir, en camino) ≥ 75 %", pl >= 0.75f, $"{pl:P0} de {_shiftSamples} muestras");
            int trainers = _training.Count(kv => kv.Value.y >= 3f), trainedOk = _training.Count(kv => kv.Value.y >= 3f && kv.Value.x / kv.Value.y >= 0.5f);
            TestProbe.Check($"{tag}: entrenan en su sala (14–16 h)", trainers > 0 && trainedOk >= 0.7f * trainers, $"{trainedOk}/{trainers}: " + string.Join(", ", _training.Select(kv => $"{kv.Key} {(kv.Value.y > 0 ? kv.Value.x / kv.Value.y : 0f):P0}")));
            int staff = _tracks.Count(tr => tr.human && !tr.player && tr.g != null && _t._wd.roster.Find(tr.g.name.Replace("_Post", "")) != null);
            TestProbe.Check($"{tag}: a mediodía come en el restaurante al menos la mitad del personal", _ate.Count(n => n != "Kushal") >= staff / 2, $"{_ate.Count(n => n != "Kushal")}/{staff}: {string.Join(", ", _ate)}");
            HumanChecks(tag);
        }

        public void CheckFree() => HumanChecks("tiempo libre");

        void HumanChecks(string tag)
        {
            string Of(params string[] kinds) => string.Join("; ", _anom.Values.Where(a => a.human && kinds.Contains(a.kind) && Since(a) > 0).Select(a => $"{a.who} ×{Since(a)} ({a.detail}, {a.firstAt})").Take(10));
            int Num(params string[] kinds) => _anom.Values.Where(a => a.human && kinds.Contains(a.kind)).Sum(a => Since(a));
            TestProbe.Check($"{tag}: ninguna persona atascada > 20 s ni parada fuera de su puesto", Num("atascado > 20 s", "parado fuera de su puesto en turno", "atascado (tiempo libre)") == 0, Of("atascado > 20 s", "parado fuera de su puesto en turno", "atascado (tiempo libre)"));
            TestProbe.Check($"{tag}: nadie salta ni se teletransporta sin motivo", Num("salto/teletransporte sin motivo") == 0, Of("salto/teletransporte sin motivo"));
            TestProbe.Check($"{tag}: nadie da vueltas en círculo", Num("da vueltas en círculo") == 0, Of("da vueltas en círculo"));
            TestProbe.Check($"{tag}: nadie dentro de muebles, en el agua, cayendo ni volcado", Num("dentro de un objeto", "en el agua sin ser acuático", "cae", "bajo el mundo", "volcado / tumbado mal") == 0, Of("dentro de un objeto", "en el agua sin ser acuático", "cae", "bajo el mundo", "volcado / tumbado mal"));
        }

        public void Final()
        {
            var animals = _anom.Values.Where(a => !a.human).ToList();
            string Grave(Anomaly a) => a.kind == "dentro de un objeto" || a.kind == "en el agua sin ser acuático" || a.kind == "cae" || a.kind == "bajo el mundo" ? "grave" : "leve";
            int grave = animals.Where(a => Grave(a) == "grave").Sum(a => a.count);
            TestProbe.Check("fauna: ningún animal dentro de muebles, en el agua sin ser acuático ni cayendo", grave == 0, string.Join("; ", animals.Where(a => Grave(a) == "grave").Select(a => $"{a.who}: {a.kind} ({a.detail})").Take(10)));
            TestProbe.Check("fauna: ningún animal atascado > 20 s, dando vueltas o volcado", animals.Where(a => Grave(a) == "leve").Sum(a => a.count) == 0, string.Join("; ", animals.Where(a => Grave(a) == "leve").GroupBy(a => a.kind).Select(g => $"{g.Key}: {g.Count()} ({string.Join(", ", g.Select(a => a.who).Take(6))})")));
            Log($"══ RESUMEN DEL VIGÍA: {_anom.Count} anomalías distintas ({_anom.Values.Where(a => a.human).Sum(a => a.count)} muestras en personas, {animals.Sum(a => a.count)} en animales) ══");
            foreach (Anomaly a in _anom.Values.OrderBy(a => a.human ? 0 : 1).ThenByDescending(a => a.count).Take(60))
                Log($"  {(a.human ? "persona" : "animal")} · {a.who}: {a.kind} ×{a.count} — {a.detail} (desde {a.firstAt})");
        }
    }
}
