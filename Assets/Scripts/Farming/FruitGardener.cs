using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// EL JARDINERO DE LOS FRUTALES (Berón 2026-10-30, docs/jardines-frutales.md): la misión diaria «Ronda de frutales» de la
/// jornada (<see cref="AreaWork"/>; la reparte la rotación como las demás). Quien la tiene va al punto de cuidados del
/// primer techo de su ronda (prepara el abono y mira el cabezal del goteo: filtro, válvula y programador), recorre las macetas
/// en orden, revisa cada una (<see cref="FruitPlant.Care"/>: el gotero, hojas y envés, abona y trata la plaga; el AGUA la pone
/// el goteo, Berón 2026-10-31) el tiempo de su ficha (<see cref="FruitCare.Ficha.minutos"/>, en minutos de juego) y, al acabar
/// un techo, vuela en alfombra al siguiente. Con el tiempo que le queda de su misión (3 h) hace las TAREAS DE LA SEMANA de sus
/// plantas: polinizar a pincel las que lo necesitan y podar y anotar el crecimiento (<see cref="FruitCare.Polinizar"/>,
/// <see cref="FruitCare.PodaDias"/>), mientras quepan.
///
/// ¿UNA PERSONA O VARIAS? <see cref="Rounds"/> mide la ronda entera (revisión de cada planta + andar entre macetas + vuelos
/// + preparar en cada punto) en horas de juego y la parte en rondas de ≤ <see cref="MaxRoundHours"/> h (el bloque de la
/// mañana), por techos seguidos del camino: si todo cabe en una, la hace una persona; si no, la reparte entre varias.
/// Todas las esperas tienen tope (si no llega a una maceta en <see cref="ReachTimeout"/> s, la salta y lo dice).
/// </summary>
public class FruitGardener : MonoBehaviour
{
    public const string MissionPrefix = "jar_frutales";
    /// <summary>Lo más larga que puede ser una ronda (h de juego): el bloque de la mañana (las misiones no se parten en la comida).</summary>
    public const float MaxRoundHours = 3f;
    /// <summary>Holgura (h de juego) que se guarda al repartir el tiempo de las tareas de la semana. Era 0,15 h; medido en Unity
    /// (2026-10-31) la ronda real va siempre algo por DEBAJO de su estimación (2,25/2,36, 2,81/2,88 y 2,86/2,88 h), y con rondas
    /// de 2,9 h esos 0,15 h se comían todo lo que sobra de las 3 h: ninguna polinización ni poda en todo el día.</summary>
    public const float ExtraMargin = 0.05f;
    /// <summary>Lo que se apunta a llenar cada ronda (h): deja margen bajo las 3 h de la misión.</summary>
    public const float RoundTarget = 2.9f;
    /// <summary>Segundos reales estimados de un vuelo de techo a techo (llamar la alfombra, subir, volar a ~26 m/s, posarse).</summary>
    public const float FlightSeconds = 45f;
    /// <summary>Minutos de juego para preparar en el punto de cuidados: el abono y medir pH y EC en la sala principal; en los
    /// demás, mirar el filtro, la válvula y el programador del goteo.</summary>
    public const float PrepMainMinutes = 5f, PrepMinutes = 2f;
    public const float ReachTimeout = 150f;

    public float walkSpeed = 2.6f;

    // ── Rondas (lógica pura sobre las plantas de la escena) ──────────────────────────────────────

    public class Leg { public string building; public List<FruitPlant> plants = new List<FruitPlant>(); public FruitCareStation station; public float hours; }
    public class Round { public string id, title; public List<Leg> legs = new List<Leg>(); public float hours; public IEnumerable<FruitPlant> Plants => legs.SelectMany(l => l.plants); }

    static List<Round> _rounds; static int _roundsFor = -1;

    static float SecondsPerGameHour => TimeController.timeController != null ? Mathf.Max(1f, TimeController.timeController.SecondsPerGameDay / 24f) : 150f;
    public static float RealSecondsForGameMinutes(float min) => min / 60f * SecondsPerGameHour;

    /// <summary>Horas de juego de una revisión en orden de <paramref name="plants"/> (sin vuelo): su ficha + andar.</summary>
    public static float LegHours(List<FruitPlant> plants, bool mainRoom, float walk = 2.6f)
    {
        float min = mainRoom ? PrepMainMinutes : PrepMinutes, walkS = 0f;
        Vector3 at = plants.Count > 0 ? plants[0].transform.position : Vector3.zero;
        foreach (FruitPlant p in plants)
        {
            min += p.Ficha != null ? p.Ficha.minutos : 2f;
            walkS += Vector3.Distance(at, p.transform.position) / walk; at = p.transform.position;
        }
        return min / 60f + walkS / SecondsPerGameHour;
    }

    /// <summary>Las rondas del día (recalculadas si cambian las plantas): techos en el orden del camino más corto desde la
    /// sala principal, recorridos maceta a maceta y partidos en rondas equilibradas de ≤ <see cref="RoundTarget"/> h.</summary>
    public static List<Round> Rounds()
    {
        FruitPlant.All.RemoveAll(p => p == null);
        if (_rounds != null && _roundsFor == FruitPlant.All.Count) return _rounds;
        _roundsFor = FruitPlant.All.Count;
        _rounds = new List<Round>();
        var byRoof = FruitPlant.All.GroupBy(p => p.building).ToDictionary(g => g.Key, g => g.OrderBy(p => p.order).ToList());
        if (byRoof.Count == 0) return _rounds;
        var stations = Object.FindObjectsByType<FruitCareStation>(FindObjectsSortMode.None);
        FruitCareStation StationOf(string b) => stations.FirstOrDefault(s => s.building == b);
        Vector3 Center(string b) { Vector3 c = Vector3.zero; foreach (FruitPlant p in byRoof[b]) c += p.transform.position; return c / byRoof[b].Count; }
        // Camino: desde el techo de la sala principal, siempre al techo más cercano.
        string start = stations.FirstOrDefault(s => s.mainRoom && byRoof.ContainsKey(s.building))?.building ?? byRoof.Keys.OrderBy(k => k).First();
        var order = new List<string> { start };
        while (order.Count < byRoof.Count)
        {
            Vector3 here = Center(order[order.Count - 1]);
            order.Add(byRoof.Keys.Where(k => !order.Contains(k)).OrderBy(k => Vector3.Distance(here, Center(k))).First());
        }
        // LLENADO SEGUIDO (medido en Unity, 2026-10-31: partir por techos enteros daba 7 rondas de 1,3–2,1 h): se recorre el
        // camino maceta a maceta y se cierra la ronda cuando la siguiente ya no cabe en su parte del total (rondas
        // equilibradas de ≤ RoundTarget h); al cambiar de techo, cuenta el vuelo y preparar en su punto de cuidados.
        float flightH = FlightSeconds / SecondsPerGameHour, total = 0f;
        foreach (string b in order) { FruitCareStation st = StationOf(b); total += LegHours(byRoof[b], st != null && st.mainRoom) + flightH; }
        int nr = Mathf.Max(1, Mathf.CeilToInt(total / RoundTarget));
        // Cada corte añade un vuelo y una preparación: se sube la meta de 0,1 en 0,1 h hasta que salen las rondas previstas.
        for (float target = Mathf.Min(RoundTarget, total / nr + 0.15f); ; target += 0.1f)
        {
            _rounds.Clear();
            Round cur = null; Leg leg = null;
            foreach (string b in order)
            {
                FruitCareStation st = StationOf(b);
                leg = null;
                Vector3 at = Vector3.zero;
                foreach (FruitPlant p in byRoof[b])
                {
                    float walkH = leg != null ? Vector3.Distance(at, p.transform.position) / 2.6f / SecondsPerGameHour : 0f;
                    float plantH = (p.Ficha != null ? p.Ficha.minutos : 2f) / 60f + walkH;
                    float open = leg == null ? flightH + (st != null && st.mainRoom ? PrepMainMinutes : PrepMinutes) / 60f : 0f;
                    if (cur == null || (cur.Plants.Any() && cur.hours + open + plantH > target)) { cur = new Round(); _rounds.Add(cur); leg = null; open = flightH + (st != null && st.mainRoom ? PrepMainMinutes : PrepMinutes) / 60f; plantH -= walkH; }
                    if (leg == null) { leg = new Leg { building = b, station = st }; cur.legs.Add(leg); cur.hours += open; }
                    leg.plants.Add(p); leg.hours += plantH; cur.hours += plantH; at = p.transform.position;
                }
            }
            if (_rounds.Count <= nr || target >= MaxRoundHours - 0.05f) break;
        }
        for (int i = 0; i < _rounds.Count; i++)
        {
            Round r = _rounds[i];
            r.id = $"{MissionPrefix}_{i + 1}";
            string roofs = string.Join(", ", r.legs.Select(l => l.building).Distinct());
            r.title = $"Ronda de frutales {i + 1}/{_rounds.Count}: revisar, abonar, polinizar y podar las macetas (con goteo) del techo de {roofs} ({r.Plants.Count()} plantas)";
        }
        Debug.Log($"[Frutales] {FruitPlant.All.Count} macetas en {byRoof.Count} techos · ronda completa {_rounds.Sum(r => r.hours):0.0} h de juego → {_rounds.Count} " +
                  $"{(_rounds.Count == 1 ? "persona" : "personas")} al día (≤ {MaxRoundHours:0} h cada una): " + string.Join(" · ", _rounds.Select(r => $"{r.id} {r.hours:0.0} h [{string.Join("+", r.legs.Select(l => $"{l.building} {l.plants.Count}"))}]")));
        return _rounds;
    }

    public static Round RoundFor(string missionId) => missionId == null ? null : Rounds().FirstOrDefault(r => r.id == missionId);
    public static bool IsRound(string missionId) => missionId != null && missionId.StartsWith(MissionPrefix);

    /// <summary>Las misiones de la jornada: una por ronda, obligatorias (las plantas en maceta no aguantan un día sin
    /// revisar), que recorren varios edificios (<c>roams</c>), de 3 h: la revisión y, con lo que sobra, las tareas de la semana.</summary>
    public static List<WorkRoster.Mission> Missions()
    {
        var l = new List<WorkRoster.Mission>();
        foreach (Round r in Rounds())
            l.Add(new WorkRoster.Mission("Huerto", r.id, r.title, MaxRoundHours, true) { roams = true });   // 3 h: lo que sobra de la revisión, para polinizar y podar
        return l;
    }

    /// <summary>Dónde empieza la ronda (el punto de cuidados de su primer techo), para colocar a quien la tiene al arrancar.</summary>
    public static Vector3? StartPoint(string missionId)
    {
        Round r = RoundFor(missionId);
        if (r == null || r.legs.Count == 0) return null;
        Leg l = r.legs[0];
        return l.station != null ? l.station.StandPoint : l.plants.Count > 0 ? l.plants[0].StandPoint(l.plants[0].transform.position + Vector3.forward) : (Vector3?)null;
    }

    // ── La ronda en el juego ─────────────────────────────────────────────────────────────────────

    enum Phase { ToStation, Prep, ToPlant, Caring, ToExtra, Extra, Done }
    Phase _phase = Phase.Done;
    Round _round;
    int _leg, _plant;
    float _until, _targetSince, _startedAt;
    Transform _can;

    public string MissionId { get; private set; }
    public bool Finished => _phase == Phase.Done && _round != null;
    public int Cared { get; private set; }
    public int Skipped { get; private set; }
    public int Pollinated { get; private set; }
    public int Pruned { get; private set; }
    public float RoundSeconds { get; private set; }
    public readonly List<string> roofsDone = new List<string>();
    public string Doing { get; private set; }

    /// <summary>Le da la ronda <paramref name="missionId"/> (si ya la tiene, sigue donde iba).</summary>
    public void Assign(string missionId)
    {
        if (MissionId == missionId && _round != null) return;
        MissionId = missionId; _round = RoundFor(missionId);
        _leg = 0; _plant = 0; Cared = 0; Skipped = 0; Pollinated = 0; Pruned = 0; _extras.Clear(); roofsDone.Clear(); RoundSeconds = 0f;
        _startedAt = Time.time;
        if (_round == null || _round.legs.Count == 0) { _phase = Phase.Done; Doing = "Sin frutales que revisar"; return; }
        Begin(Phase.ToStation);
        Debug.Log($"[Frutales] «{name}» empieza «{_round.title}» (estimado {_round.hours:0.0} h de juego).");
    }

    void Begin(Phase p) { _phase = p; _targetSince = Time.time; PathMover.Of(this).Restart(); }

    void OnEnable() { if (TryGetComponent(out IdleBehavior idle)) idle.enabled = false; }
    void OnDisable() { Hold(false); }

    Leg CurLeg => _round != null && _leg < _round.legs.Count ? _round.legs[_leg] : null;

    void Update()
    {
        if (_round == null || _phase == Phase.Done) return;
        // Las NECESIDADES van antes (baño, comer, dormir), como el guardia.
        if (TryGetComponent(out ActionQueue q) && q.Own.Count > 0 && (q.Own[0] is ToiletSpell || q.Own[0] is DineSpell || q.Own[0] is SleepSpell || q.Own[0] is EatSpell)) return;
        Leg leg = CurLeg;
        if (leg == null) { Finish(); return; }
        switch (_phase)
        {
            case Phase.ToStation:
                if (leg.station == null) { Begin(Phase.ToPlant); return; }
                Doing = $"Va al punto de cuidados del techo de {leg.building}";
                if (Reach(leg.station.StandPoint)) { _until = Time.time + RealSecondsForGameMinutes(leg.station.mainRoom && _leg == 0 ? PrepMainMinutes : PrepMinutes); _phase = Phase.Prep; }
                break;
            case Phase.Prep:
                Doing = leg.station != null && leg.station.mainRoom ? "Prepara el abono, mide pH y EC y mira el cabezal del goteo en la sala de cuidados" : $"Mira el filtro, la válvula y el programador del goteo del techo de {leg.building}";
                if (Time.time >= _until) Begin(Phase.ToPlant);
                break;
            case Phase.ToPlant:
            {
                FruitPlant p = _plant < leg.plants.Count ? leg.plants[_plant] : null;
                if (p == null) { NextPlant(false); return; }
                Doing = $"Va al {p.Name} ({_plant + 1}/{leg.plants.Count}, techo de {leg.building})";
                if (Reach(p.StandPoint(transform.position)))
                {
                    Vector3 look = p.transform.position - transform.position; look.y = 0f;
                    if (look.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(look);
                    _until = Time.time + RealSecondsForGameMinutes(p.Ficha != null ? p.Ficha.minutos : 2f);
                    _phase = Phase.Caring; Hold(true);
                }
                break;
            }
            case Phase.Caring:
            {
                FruitPlant p = _plant < leg.plants.Count ? leg.plants[_plant] : null;
                if (p == null) { NextPlant(false); return; }
                Doing = $"Revisa el {p.Name}: el gotero, hojas y envés, abono y plaga";
                if (Time.time >= _until)
                {
                    var (b, a) = p.Care(name);
                    Cared++;
                    if (Cared <= 3 || Cared % 10 == 0) Debug.Log($"[Frutales] «{name}» cuida el {p.Name} del techo de {leg.building}: bienestar {b * 100f:0}% → {a * 100f:0}%.");
                    NextPlant(true);
                }
                break;
            }
            case Phase.ToExtra:
            {
                if (_extras.Count == 0) { EndLeg(); return; }
                FruitPlant p = _extras[0];
                if (p == null) { _extras.RemoveAt(0); return; }
                Doing = $"Va al {p.Name} a {ExtraWhat(p)} (techo de {leg.building})";
                if (Reach(p.StandPoint(transform.position)))
                {
                    Vector3 look = p.transform.position - transform.position; look.y = 0f;
                    if (look.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(look);
                    _until = Time.time + RealSecondsForGameMinutes(ExtraMinutes(p)); _phase = Phase.Extra; Hold(true);
                }
                break;
            }
            case Phase.Extra:
            {
                FruitPlant p = _extras.Count > 0 ? _extras[0] : null;
                if (p == null) { Begin(Phase.ToExtra); return; }
                Doing = $"{char.ToUpper(ExtraWhat(p)[0])}{ExtraWhat(p).Substring(1)} el {p.Name}";
                if (Time.time >= _until)
                {
                    int day = TimeController.timeController != null ? TimeController.timeController.Day : 0;
                    if (p.NeedsPollination(day)) { p.Pollinate(name); Pollinated++; }
                    if (p.NeedsPruning(day)) { p.Prune(name); Pruned++; }
                    _extras.RemoveAt(0); Hold(false);
                    NextExtra();
                }
                break;
            }
        }
    }

    // ── LAS TAREAS DE LA SEMANA (el tiempo que libera el goteo) ──
    // Al acabar la revisión de un techo, las plantas de ese tramo a las que hoy les toca polinizar o podar, mientras quepan en
    // su parte del tiempo que sobra de la misión (3 h menos lo que falta de revisión y vuelos, repartido por macetas).
    readonly List<FruitPlant> _extras = new List<FruitPlant>();
    float _extraUntil;
    static string ExtraWhat(FruitPlant p)
    {
        int day = TimeController.timeController != null ? TimeController.timeController.Day : 0;
        bool po = p.NeedsPollination(day), pr = p.NeedsPruning(day);
        return po && pr ? "polinizar y podar" : po ? "polinizar a pincel" : "podar y anotar el crecimiento de";
    }
    static float ExtraMinutes(FruitPlant p)
    {
        int day = TimeController.timeController != null ? TimeController.timeController.Day : 0;
        return (p.NeedsPollination(day) ? FruitCare.MinPolinizar : 0f) + (p.NeedsPruning(day) ? FruitCare.MinPodar : 0f);
    }

    void StartExtras(Leg leg)
    {
        _extras.Clear();
        int day = TimeController.timeController != null ? TimeController.timeController.Day : 0;
        foreach (FruitPlant p in leg.plants) if (p != null && (p.NeedsPollination(day) || p.NeedsPruning(day))) _extras.Add(p);
        float flightH = FlightSeconds / SecondsPerGameHour, elapsed = (Time.time - _startedAt) / SecondsPerGameHour, rest = 0f; int restPlants = 0;
        for (int i = _leg + 1; i < _round.legs.Count; i++) { rest += _round.legs[i].hours + (_round.legs[i].building != _round.legs[i - 1].building ? flightH : 0f); restPlants += _round.legs[i].plants.Count; }
        float spare = Mathf.Max(0f, MaxRoundHours - ExtraMargin - elapsed - rest);
        float share = spare * leg.plants.Count / Mathf.Max(1, leg.plants.Count + restPlants);
        _extraUntil = Time.time + share * SecondsPerGameHour;
        NextExtra();
    }

    // La siguiente tarea si cabe (andar hasta ella + su tiempo); si no, se deja para otro día y sigue la ronda.
    void NextExtra()
    {
        if (_extras.Count > 0)
        {
            FruitPlant p = _extras[0];
            float cost = p == null ? 0f : Vector3.Distance(transform.position, p.transform.position) / walkSpeed + RealSecondsForGameMinutes(ExtraMinutes(p));
            if (p != null && Time.time + cost <= _extraUntil) { Begin(Phase.ToExtra); return; }
        }
        _extras.Clear();
        EndLeg();
    }

    // Anda (o vuela) hacia el punto; true al llegar. Con tope: si no llega, lo dice y lo da por imposible.
    bool _timedOut;
    bool Reach(Vector3 to)
    {
        _timedOut = false;
        float d = PathMover.Of(this).StepToward(to, walkSpeed);
        Vector3 flat = to - transform.position; flat.y = 0f;
        if (flat.magnitude < 1.2f && Mathf.Abs(to.y - transform.position.y) < 2.5f) return true;
        if (FlyingCarpet.Riding(gameObject)) { _targetSince = Time.time; return false; }   // volando no cuenta
        if (Time.time - _targetSince > ReachTimeout)
        {
            _timedOut = true;
            Debug.LogWarning($"[Frutales] «{name}» no llega a ({to.x:0},{to.y:0},{to.z:0}) en {ReachTimeout:0} s (queda {d:0.0} m): lo salta.");
            if (_phase == Phase.ToStation) { Begin(Phase.ToPlant); return false; }
            if (_phase == Phase.ToExtra) { if (_extras.Count > 0) _extras.RemoveAt(0); NextExtra(); return false; }
            Skipped++; NextPlant(false);
        }
        return false;
    }

    void NextPlant(bool cared)
    {
        Hold(false);
        Leg leg = CurLeg;
        _plant++;
        if (leg != null && _plant >= leg.plants.Count) { StartExtras(leg); return; }   // revisado el tramo: sus tareas de la semana
        Begin(Phase.ToPlant);
    }

    // Fin de un tramo (revisión y tareas): al siguiente, en su techo o volando al de al lado.
    void EndLeg()
    {
        Hold(false);
        Leg leg = CurLeg;
        if (leg != null && !roofsDone.Contains(leg.building)) roofsDone.Add(leg.building);
        _leg++; _plant = 0;
        if (CurLeg == null) { Finish(); return; }
        Begin(leg != null && CurLeg.building == leg.building ? Phase.ToPlant : Phase.ToStation);
    }

    void Finish()
    {
        Hold(false);
        _phase = Phase.Done;
        RoundSeconds = Time.time - _startedAt;
        float h = RoundSeconds / SecondsPerGameHour;
        Doing = $"Ronda terminada: {Cared} frutales revisadas, {Pollinated} polinizadas y {Pruned} podadas en {h:0.0} h";
        Debug.Log($"[Frutales] «{name}» termina «{_round.title}»: {Cared} cuidadas, {Skipped} saltadas, {Pollinated} polinizadas, {Pruned} podadas; techos {string.Join(", ", roofsDone)}; {RoundSeconds:0} s de juego = {h:0.00} h de juego (estimado {_round.hours:0.00} h).");
    }

    // La regadera en la mano mientras revisa una maceta.
    void Hold(bool on)
    {
        if (on && _can == null)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(g.GetComponent<Collider>());
            g.name = "Regadera"; _can = g.transform; _can.SetParent(transform, false);
            _can.localPosition = new Vector3(0.35f, 0.1f, 0.45f); _can.localScale = new Vector3(0.22f, 0.25f, 0.35f);
            g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(new Color(0.35f, 0.62f, 0.55f));
        }
        else if (!on && _can != null) { Destroy(_can.gameObject); _can = null; }
    }
}
