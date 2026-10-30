using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>Punto de cuidados de un techo de frutales: la SALA de la experta (Servicios: banco de trasplante, depósito de
/// fertirriego con dosificador, armario de abonos, medidor de pH y EC…) o el punto de riego de los demás techos (depósito y
/// carro). El jardinero empieza ahí la ronda de cada techo.</summary>
public class FruitCareStation : MonoBehaviour
{
    public string building;
    public bool mainRoom;
    /// <summary>Dónde se pone a trabajar (delante de la línea de equipos).</summary>
    public Vector3 StandPoint => transform.TransformPoint(new Vector3(0f, 1f, 1.6f));
}

/// <summary>
/// EL JARDINERO DE LOS FRUTALES (Berón 2026-10-30, docs/jardines-frutales.md): la misión diaria «Ronda de frutales» de la
/// jornada (<see cref="AreaWork"/>; la reparte la rotación como las demás). Quien la tiene va al punto de cuidados del
/// primer techo de su ronda (prepara la solución de abono y llena el carro de fertirriego), recorre las macetas en orden,
/// revisa cada una (<see cref="FruitPlant.Care"/>: riega, abona, trata la plaga) el tiempo real de su ficha
/// (<see cref="FruitCare.Ficha.minutos"/>, en minutos de juego) y, al acabar un techo, vuela en alfombra al siguiente.
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
    /// <summary>Segundos reales estimados de un vuelo de techo a techo (llamar la alfombra, subir, volar a ~26 m/s, posarse).</summary>
    public const float FlightSeconds = 45f;
    /// <summary>Minutos de juego para preparar en el punto de cuidados: la solución de abono en la sala principal y llenar el carro en los demás.</summary>
    public const float PrepMainMinutes = 15f, PrepMinutes = 5f;
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
    /// sala principal, cada techo partido si él solo pasa de <see cref="MaxRoundHours"/>, y techos seguidos juntos mientras quepan.</summary>
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
        float flightH = FlightSeconds / SecondsPerGameHour;
        var legs = new List<Leg>();
        foreach (string b in order)
        {
            List<FruitPlant> ps = byRoof[b];
            FruitCareStation st = StationOf(b);
            float h = LegHours(ps, st != null && st.mainRoom);
            int parts = Mathf.Max(1, Mathf.CeilToInt((h + flightH) / MaxRoundHours));
            for (int k = 0; k < parts; k++)
            {
                var sub = ps.Skip(ps.Count * k / parts).Take(ps.Count * (k + 1) / parts - ps.Count * k / parts).ToList();
                legs.Add(new Leg { building = b, plants = sub, station = st, hours = LegHours(sub, st != null && st.mainRoom && k == 0) });
            }
        }
        Round cur = null;
        foreach (Leg l in legs)
        {
            float add = l.hours + flightH;   // (cada tramo: llegar en alfombra a su techo)
            if (cur == null || cur.hours + add > MaxRoundHours) { cur = new Round(); _rounds.Add(cur); }
            cur.legs.Add(l); cur.hours += add;
        }
        for (int i = 0; i < _rounds.Count; i++)
        {
            Round r = _rounds[i];
            r.id = $"{MissionPrefix}_{i + 1}";
            string roofs = string.Join(", ", r.legs.Select(l => l.building).Distinct());
            r.title = $"Ronda de frutales {i + 1}/{_rounds.Count}: revisar, regar y abonar las macetas del techo de {roofs} ({r.Plants.Count()} plantas)";
        }
        Debug.Log($"[Frutales] {FruitPlant.All.Count} macetas en {byRoof.Count} techos · ronda completa {_rounds.Sum(r => r.hours):0.0} h de juego → {_rounds.Count} " +
                  $"{(_rounds.Count == 1 ? "persona" : "personas")} al día (≤ {MaxRoundHours:0} h cada una): " + string.Join(" · ", _rounds.Select(r => $"{r.id} {r.hours:0.0} h [{string.Join("+", r.legs.Select(l => $"{l.building} {l.plants.Count}"))}]")));
        return _rounds;
    }

    public static Round RoundFor(string missionId) => missionId == null ? null : Rounds().FirstOrDefault(r => r.id == missionId);
    public static bool IsRound(string missionId) => missionId != null && missionId.StartsWith(MissionPrefix);

    /// <summary>Las misiones de la jornada: una por ronda, obligatorias (las plantas en maceta no aguantan un día sin
    /// revisar), que recorren varios edificios (<c>roams</c>). Horas redondeadas a la media hora de arriba.</summary>
    public static List<WorkRoster.Mission> Missions()
    {
        var l = new List<WorkRoster.Mission>();
        foreach (Round r in Rounds())
            l.Add(new WorkRoster.Mission("Huerto", r.id, r.title, Mathf.Clamp(Mathf.Ceil(r.hours * 2f) / 2f, 0.5f, MaxRoundHours), true) { roams = true });
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

    enum Phase { ToStation, Prep, ToPlant, Caring, Done }
    Phase _phase = Phase.Done;
    Round _round;
    int _leg, _plant;
    float _until, _targetSince, _startedAt;
    Transform _can;

    public string MissionId { get; private set; }
    public bool Finished => _phase == Phase.Done && _round != null;
    public int Cared { get; private set; }
    public int Skipped { get; private set; }
    public float RoundSeconds { get; private set; }
    public readonly List<string> roofsDone = new List<string>();
    public string Doing { get; private set; }

    /// <summary>Le da la ronda <paramref name="missionId"/> (si ya la tiene, sigue donde iba).</summary>
    public void Assign(string missionId)
    {
        if (MissionId == missionId && _round != null) return;
        MissionId = missionId; _round = RoundFor(missionId);
        _leg = 0; _plant = 0; Cared = 0; Skipped = 0; roofsDone.Clear(); RoundSeconds = 0f;
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
                Doing = leg.station != null && leg.station.mainRoom ? "Prepara la solución de abono y mide pH y EC en la sala de cuidados" : $"Llena el carro de fertirriego en el techo de {leg.building}";
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
                Doing = $"Revisa el {p.Name}: hojas y envés, humedad, riego y abono";
                if (Time.time >= _until)
                {
                    var (b, a) = p.Care(name);
                    Cared++;
                    if (Cared <= 3 || Cared % 10 == 0) Debug.Log($"[Frutales] «{name}» cuida el {p.Name} del techo de {leg.building}: bienestar {b * 100f:0}% → {a * 100f:0}%.");
                    NextPlant(true);
                }
                break;
            }
        }
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
            Skipped++; NextPlant(false);
        }
        return false;
    }

    void NextPlant(bool cared)
    {
        Hold(false);
        Leg leg = CurLeg;
        _plant++;
        if (leg != null && _plant >= leg.plants.Count)
        {
            if (!roofsDone.Contains(leg.building)) roofsDone.Add(leg.building);
            _leg++; _plant = 0;
            if (CurLeg == null) { Finish(); return; }
            Begin(CurLeg.building == leg.building ? Phase.ToPlant : Phase.ToStation);
            return;
        }
        Begin(Phase.ToPlant);
    }

    void Finish()
    {
        Hold(false);
        _phase = Phase.Done;
        RoundSeconds = Time.time - _startedAt;
        float h = RoundSeconds / SecondsPerGameHour;
        Doing = $"Ronda terminada: {Cared} frutales revisadas en {h:0.0} h";
        Debug.Log($"[Frutales] «{name}» termina «{_round.title}»: {Cared} cuidadas, {Skipped} saltadas, techos {string.Join(", ", roofsDone)}; {RoundSeconds:0} s reales = {h:0.00} h de juego (estimado {_round.hours:0.00} h).");
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
