using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UN HOGAR (docs/ciclo-de-vida.md §Humanos; docs/microcosmos-reglas-edificacion.md regla 6; Berón 2026-11-02: «los pueblos de
/// historia de los microcosmos deben ser reconstruidos para que haya hogares para los demás personajes, además de los lugares de
/// trabajo y los servicios básicos, para que cada personaje realice su ciclo de vida correctamente»).
/// La casa de una FAMILIA (una persona sola, una pareja, una pareja con hijos): sus camas (una por persona), el retrete, el
/// lavabo, la mesa con sus sillas y la DESPENSA (<see cref="FoodContainer"/>) de la que se come en casa. La misma pieza para los
/// humanos y para los insectos-humanos: cuando el cuerpo pide (<see cref="NeedsDriver"/>), quien tiene hogar come, duerme, va al
/// baño y se lava EN SU CASA (<see cref="HomeNeedSpell"/>). Los hijos que nacen entran en la familia (<see cref="Join"/>).
/// Va en un hijo «Hogar_…» del edificio (un edificio puede alojar varias familias: los barracones).
/// </summary>
public class Household : MonoBehaviour
{
    public string family = "";
    public List<GameObject> members = new List<GameObject>();
    public List<Transform> beds = new List<Transform>();
    public Transform toilet, washbasin, table;
    public List<Transform> seats = new List<Transform>();
    public FoodContainer pantry;
    [Tooltip("Despensa inagotable (escenas de ciclo de vida): nunca baja de 10 raciones.")]
    public bool bottomless;
    [Tooltip("Raciones por persona que entran en la despensa cada día de juego (lo que se compra, se cosecha o se trae del mercado).")]
    public int rationsPerPersonDay = 3;

    public static readonly List<Household> All = new List<Household>();
    void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    void OnDisable() => All.Remove(this);

    int _stockedDay = int.MinValue;

    void Start() => Restock();

    void Update()
    {
        if (Time.frameCount % 30 != 0) return;
        Restock();
    }

    void Restock()
    {
        if (pantry == null) return;
        if (bottomless) { if (pantry.rations < 10) pantry.rations = 20; return; }
        int day = TimeController.timeController != null ? TimeController.timeController.Day : 0;
        if (day == _stockedDay) return;
        _stockedDay = day;
        int want = Mathf.Max(1, Living) * rationsPerPersonDay;
        if (pantry.rations < want) pantry.rations = want;
    }

    /// <summary>Cuántos de la familia siguen vivos.</summary>
    public int Living
    {
        get
        {
            int n = 0;
            foreach (GameObject m in members) if (m != null && !(m.TryGetComponent(out Anima a) && a.death)) n++;
            return n;
        }
    }

    /// <summary>El hogar de <paramref name="g"/> (el que apunta su <see cref="PersonLife"/>, o el que lo cuenta entre los suyos).</summary>
    public static Household Of(GameObject g)
    {
        if (g == null) return null;
        if (g.TryGetComponent(out PersonLife pl) && pl.home != null) return pl.home;
        foreach (Household h in All) if (h != null && h.members.Contains(g)) return h;
        return null;
    }

    /// <summary>Entra en la familia (un recién nacido, una pareja que se casa).</summary>
    public void Join(GameObject g)
    {
        if (g == null) return;
        if (!members.Contains(g)) members.Add(g);
        if (g.TryGetComponent(out PersonLife pl)) pl.home = this;
    }

    /// <summary>Sale de la familia (muere).</summary>
    public void Leave(GameObject g) { if (g != null) members.Remove(g); }

    /// <summary>La cama de <paramref name="g"/>: la de su puesto en la familia; si hay más personas que camas (un bebé), comparte la
    /// de su madre (la última).</summary>
    public Transform BedOf(GameObject g)
    {
        if (beds.Count == 0) return null;
        int i = members.IndexOf(g);
        if (i < 0) i = 0;
        if (i < beds.Count) return beds[i];
        if (g != null && g.TryGetComponent(out PersonLife pl) && pl.mother != null)
        {
            int m = members.IndexOf(pl.mother.gameObject);
            if (m >= 0 && m < beds.Count) return beds[m];
        }
        return beds[beds.Count - 1];
    }

    /// <summary>La silla de <paramref name="g"/> a la mesa (cada uno la suya; si hay más que sillas, se repiten).</summary>
    public Transform SeatOf(GameObject g)
    {
        if (seats.Count == 0) return table;
        int i = Mathf.Max(0, members.IndexOf(g));
        return seats[i % seats.Count];
    }

    /// <summary>Lo que le falta a este hogar para que su familia haga su ciclo de vida (vacío = completo): una cama por persona,
    /// retrete, lavabo, mesa con sillas y despensa.</summary>
    public List<string> Problems()
    {
        var l = new List<string>();
        string who = string.IsNullOrEmpty(family) ? name : family;
        int people = 0, sleepers = 0;
        foreach (GameObject m in members)
        {
            if (m == null || (m.TryGetComponent(out Anima a) && a.death)) continue;
            people++;
            if (!(m.TryGetComponent(out PersonLife pl) && pl.Stage == LifeStage.child && pl.mother != null)) sleepers++;   // un niño puede dormir con su madre
        }
        if (people == 0) l.Add($"{who}: hogar sin nadie");
        int beds0 = 0; foreach (Transform b in beds) if (b != null) beds0++;
        if (beds0 == 0) l.Add($"{who}: sin cama");
        else if (beds0 < sleepers) l.Add($"{who}: {beds0} cama(s) para {sleepers} personas");
        if (toilet == null) l.Add($"{who}: sin retrete");
        if (washbasin == null) l.Add($"{who}: sin lavabo");
        if (table == null) l.Add($"{who}: sin mesa");
        if (seats.Count == 0) l.Add($"{who}: sin sillas");
        if (pantry == null) l.Add($"{who}: sin despensa");
        return l;
    }

    /// <summary>
    /// HOGARES DE UN PUEBLO (regla 6 de edificación; lo miran las pruebas 62 y 63): cada persona (<see cref="PersonLife"/>; en el
    /// microcosmos, cada insecto-humano con <see cref="FolkLineage"/>) bajo <paramref name="scope"/> tiene un hogar completo, y su
    /// familia cabe en él. Devuelve los problemas (vacío = bien).
    /// </summary>
    public static List<string> Check(Transform scope)
    {
        var bad = new List<string>();
        if (scope == null) return bad;
        var seen = new HashSet<Household>();
        var people = new HashSet<GameObject>();
        foreach (PersonLife p in scope.GetComponentsInChildren<PersonLife>(true)) people.Add(p.gameObject);
        foreach (FolkLineage f in scope.GetComponentsInChildren<FolkLineage>(true)) people.Add(f.gameObject);
        foreach (GameObject g in people)
        {
            if (g.TryGetComponent(out Anima a) && a.death) continue;
            Household h = null;
            if (g.TryGetComponent(out PersonLife pl)) h = pl.home;
            if (h == null) foreach (Household x in scope.GetComponentsInChildren<Household>(true)) if (x.members.Contains(g)) { h = x; break; }
            if (h == null) { bad.Add($"«{g.name}» sin hogar"); continue; }
            if (seen.Add(h)) bad.AddRange(h.Problems());
        }
        return bad;
    }
}

/// <summary>
/// UNA NECESIDAD ATENDIDA EN CASA (la vida diaria del hogar, <see cref="Household"/>): va andando a su mueble —la mesa, la cama, el
/// retrete, el lavabo— y lo usa: COMER (se sienta, toma una ración de la despensa, ~20 min), DORMIR (se tumba en su cama hasta
/// descansar; una amenaza lo despierta), IR AL BAÑO (~5 min; se lava las manos) y LAVARSE (~10 min). Lo encola
/// <see cref="NeedsDriver"/> en el carril propio. El mismo hechizo para un humano y para un insecto-humano: solo cambia la postura
/// (el cuerpo de cubos se sienta o se tumba con las asanas de <see cref="DailyPoses"/>; el insecto se tumba de lado entero).
/// Ninguna espera sin tope: si en 45 s reales no llega andando, se le pone junto al mueble (y se cuenta en <see cref="Fallbacks"/>).
/// </summary>
public class HomeNeedSpell : QueuedSpell
{
    public enum Need { Comer, Dormir, Bano, Lavarse }
    public Need need;
    public Household home;
    public float eatMinutes = 20f, toiletMinutes = 5f, washMinutes = 10f, maxSleepHours = 14f;
    const float WalkTimeout = 45f;

    /// <summary>Cuántas veces alguien no llegó andando a su mueble de casa y se le puso allí (registro, pruebas).</summary>
    public static int Fallbacks;
    /// <summary>Quien se quedó sin comida en casa lo vuelve a intentar pasado un rato (no en bucle).</summary>
    public static readonly Dictionary<Anima, float> RetryEat = new Dictionary<Anima, float>();

    int _phase;          // 0 ir · 1 usarlo · 2 levantarse
    float _since, _until, _sleepStart;
    Transform _spot;

    public override string Label => need switch
    {
        Need.Comer => _phase == 0 ? "ir a comer a casa" : "comer en casa",
        Need.Dormir => _phase == 0 ? "ir a su cama" : "dormir en casa",
        Need.Bano => "ir al baño (en casa)",
        _ => _phase == 0 ? "ir al lavabo" : "lavarse",
    };

    static float MinuteSecs => TimeController.timeController != null ? TimeController.timeController.TimeSpeedMinuteSecs : 1f;

    public override bool Tick(Anima self)
    {
        if (self == null || home == null) return true;
        BodyMetabolism body = self.GetComponent<BodyMetabolism>();
        Transform t = self.transform;
        float h = PersonLife.HeightOf(t);
        switch (_phase)
        {
            case 0:
            {
                if (_spot == null)
                {
                    _spot = need switch { Need.Comer => home.SeatOf(self.gameObject), Need.Dormir => home.BedOf(self.gameObject), Need.Bano => home.toilet, _ => home.washbasin };
                    if (_spot == null) { Debug.Log($"[Hogar] «{self.name}»: su casa no tiene {need}."); return true; }
                    if (need == Need.Comer && (home.pantry == null || !home.pantry.HasFood))
                    { Debug.Log($"[Hogar] «{self.name}» tiene hambre y la despensa está vacía."); RetryEat[self] = Time.time + 20f; return true; }
                    _since = Time.time;
                }
                Vector3 to = UsePoint(_spot, t.position, h);
                bool there = MicroMove.Step(t, to, 1.4f * h / 1.75f, 0.45f * h);
                if (!there && Time.time - _since > WalkTimeout)
                {
                    HomeMove.Place(t, to); HomeMove.Release(t); there = true; Fallbacks++;
                    if (Fallbacks <= 20) Debug.LogWarning($"[Hogar] «{self.name}» no llega andando en {WalkTimeout:0} s a «{_spot.name}» de su casa: se le pone allí.");
                }
                if (!there) return false;
                Begin(self, body, h);
                _phase = 1;
                return false;
            }
            case 1:
            {
                if (need == Need.Dormir)
                {
                    bool rested = self.sleepiness < 0.1f, woken = self.aware;
                    bool tooLong = (PersonLife.NowDays - _sleepStart) * 24f > maxSleepHours;
                    if (!rested && !woken && !tooLong) { HomeMove.KeepLying(self, _spot); return false; }
                    self.asleep = false;
                    Debug.Log($"[Hogar] «{self.name}» se despierta {(woken ? "(amenaza)" : rested ? "descansado" : "(tope de horas)")}.");
                }
                else
                {
                    if (need == Need.Comer) HomeMove.KeepSitting(self);
                    if (Time.time < _until) return false;
                    Finish(self, body);
                }
                HomeMove.StandUp(self, UsePoint(_spot, t.position, h));
                _phase = 2;
                return true;
            }
            default: return true;
        }
    }

    void Begin(Anima self, BodyMetabolism body, float h)
    {
        Transform t = self.transform;
        switch (need)
        {
            case Need.Comer:
                HomeMove.Sit(self, _spot, home.table);
                _until = Time.time + eatMinutes * MinuteSecs;
                Debug.Log($"[Hogar] «{self.name}» se sienta a comer en casa.");
                break;
            case Need.Dormir:
                self.asleep = true; _sleepStart = PersonLife.NowDays;
                HomeMove.Lie(self, _spot);
                self.GetComponentInChildren<CubeFace>()?.Show(CubeFace.Expression.Asleep, 3600f);
                PersonLife.Report(self.gameObject, "dormir");
                Debug.Log($"[Hogar] «{self.name}» se acuesta en su cama ({_spot.name}).");
                break;
            case Need.Bano:
                HomeMove.Face(t, _spot.position);
                _until = Time.time + toiletMinutes * MinuteSecs;
                Debug.Log($"[Hogar] «{self.name}» va al baño ({_spot.name}).");
                break;
            default:
                HomeMove.Face(t, _spot.position);
                _until = Time.time + washMinutes * MinuteSecs;
                Debug.Log($"[Hogar] «{self.name}» se lava ({_spot.name}).");
                break;
        }
    }

    void Finish(Anima self, BodyMetabolism body)
    {
        switch (need)
        {
            case Need.Comer:
                if (home.pantry != null && home.pantry.Take())
                {
                    if (body != null) { body.Feed(0.75f); body.toilet = Mathf.Clamp01(body.toilet + 0.25f); }
                    PersonLife.Report(self.gameObject, "comer");
                    Debug.Log($"[Hogar] «{self.name}» come en casa ({home.pantry.dishName}).");
                }
                else { RetryEat[self] = Time.time + 20f; Debug.Log($"[Hogar] «{self.name}»: no quedaba nada en la despensa."); }
                break;
            case Need.Bano:
                if (body != null) { body.toilet = 0f; body.dirt = Mathf.Max(0f, body.dirt - 0.2f); }   // y se lava las manos
                PersonLife.Report(self.gameObject, "bano");
                break;
            case Need.Lavarse:
                if (body != null) body.dirt = 0f;
                PersonLife.Report(self.gameObject, "lavarse");
                break;
        }
    }

    /// <summary>El sitio de uso de un mueble: delante de su cara frontal (−z de su caja), a un tercio de la estatura; si no tiene
    /// caja, a su lado, hacia quien llega.</summary>
    public static Vector3 UsePoint(Transform piece, Vector3 from, float height)
    {
        float gap = height / 3f;
        BoxCollider bc = piece.GetComponent<BoxCollider>();
        Vector3 p;
        if (bc == null)
        {
            Vector3 d = from - piece.position; d.y = 0f;
            p = piece.position + (d.sqrMagnitude > 1e-4f ? d.normalized : -piece.forward) * gap;
        }
        else
        {
            float front = bc.center.z - bc.size.z * 0.5f;
            p = piece.TransformPoint(new Vector3(bc.center.x, 0f, front - gap / Mathf.Max(1e-3f, Mathf.Abs(piece.lossyScale.z))));
        }
        p.y = piece.position.y;
        return p;
    }
}

/// <summary>Las posturas y los movimientos del hogar, para un cuerpo de cubos humano (<see cref="CreatureRig"/>) y para un
/// insecto-humano (<see cref="CubeInsect"/>). Mientras está sentado o tumbado no lo mueve su NavMeshAgent.</summary>
public static class HomeMove
{
    /// <summary>Quieto (sin agente ni controlador) mientras está sentado o tumbado.</summary>
    public static void Hold(Transform t)
    {
        var ag = t.GetComponent<UnityEngine.AI.NavMeshAgent>(); if (ag != null) ag.enabled = false;
        var cc = t.GetComponent<CharacterController>(); if (cc != null) cc.enabled = false;
    }

    /// <summary>Vuelve a andar (el agente se pega al NavMesh donde esté).</summary>
    public static void Release(Transform t)
    {
        var cc = t.GetComponent<CharacterController>(); if (cc != null) cc.enabled = true;
        var ag = t.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (ag != null && !ag.enabled) { ag.enabled = true; if (ag.isOnNavMesh) ag.Warp(t.position); }
    }

    public static void Place(Transform t, Vector3 p) { Hold(t); t.position = p; }

    public static void Face(Transform t, Vector3 p) { Vector3 d = p - t.position; d.y = 0f; if (d.sqrMagnitude > 1e-4f) t.rotation = Quaternion.LookRotation(d); }

    static Transform Insect(Anima a) => a.GetComponent<CubeInsect>() != null ? a.transform.Find("Insecto") : null;

    /// <summary>Se sienta en <paramref name="seat"/> mirando a la mesa.</summary>
    public static void Sit(Anima a, Transform seat, Transform table)
    {
        Transform t = a.transform;
        Place(t, new Vector3(seat.position.x, seat.parent != null ? seat.parent.position.y : seat.position.y, seat.position.z));
        if (table != null) Face(t, table.position);
        if (a.GetComponent<CreatureRig>() != null) DailyPoses.Sit().ApplyFinal(a);
    }

    public static void KeepSitting(Anima a) { if (a.GetComponent<CreatureRig>() != null) DailyPoses.Sit().ApplyFinal(a); }

    /// <summary>Se tumba en la cama <paramref name="bed"/>, con la cabeza en la almohada (+z de la cama).</summary>
    public static void Lie(Anima a, Transform bed)
    {
        Transform t = a.transform;
        float top = bed.position.y + 0.5f;
        foreach (Renderer r in bed.GetComponentsInChildren<Renderer>()) top = Mathf.Max(top, r.bounds.max.y);
        Hold(t);
        float h = PersonLife.HeightOf(t);
        if (a.GetComponent<CreatureRig>() != null)
        {
            // La pose Lie pone la cabeza hacia −z del cuerpo y la almohada está en +z de la cama: se gira 180° (como SleepSpell).
            t.position = new Vector3(bed.position.x, top - 0.02f, bed.position.z);
            t.rotation = bed.rotation * Quaternion.Euler(0f, 180f, 0f);
            DailyPoses.Lie().ApplyFinal(a);
        }
        else
        {
            string rest = RestOf(bed);
            t.rotation = bed.rotation;
            Transform ins = Insect(a);
            CubeInsect ci = a.GetComponent<CubeInsect>();
            if (rest == MicroBody.Vientre)
            {   // DE VIENTRE (el insecto-humano: centauro, cúpula, araña, ciempiés; regla 7): el torso baja, las patas se recogen y
                // el cuerpo entero descansa a lo largo de su lecho, la cabeza hacia la cabecera (+z) y el pecho en su apoyo.
                if (ins != null) ins.localRotation = Quaternion.identity;
                if (ci != null) ci.PoseNow(false);
                t.position = new Vector3(bed.position.x, LieHeight(bed, top), bed.position.z);
            }
            else if (rest == MicroBody.Percha)
            {   // AGARRADO A LA PERCHA (abeja, luciérnaga, cigarra) o COLGADO DE SU BARRA (mantis): de pie bajo la barra, quieto.
                if (ins != null) ins.localRotation = Quaternion.identity;
                t.position = new Vector3(bed.position.x, bed.position.y + 0.08f, bed.position.z);
            }
            else
            {   // De espaldas (una cama humana): el cuerpo (que va de los pies, en el pivote, hacia arriba) se tumba hacia +z.
                t.position = new Vector3(bed.position.x, top, bed.position.z) - bed.forward * (h * 0.5f);
                if (ins != null) ins.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }
    }

    public static void KeepLying(Anima a, Transform bed = null)
    {
        if (a.GetComponent<CreatureRig>() != null) { DailyPoses.Lie().ApplyFinal(a); return; }
        string rest = RestOf(bed);
        Transform ins = Insect(a); if (ins == null) return;
        if (rest == MicroBody.Vientre || rest == MicroBody.Percha) ins.localRotation = Quaternion.identity;
        else ins.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    // La altura donde se apoya el cuerpo tumbado de vientre: la del colchón (bajo el apoyo del pecho) o la red de la hamaca;
    // no la de lo más alto de la cama (los postes del dosel, la cabecera).
    static float LieHeight(Transform bed, float top)
    {
        foreach (string n in new[] { "Apoyo_del_pecho", "Red_de_seda" })
        {
            Transform p = bed.Find(n); Renderer r = p != null ? p.GetComponent<Renderer>() : null;
            if (r != null) return n == "Red_de_seda" ? r.bounds.max.y : r.bounds.min.y;
        }
        return top - 0.1f;
    }

    /// <summary>Cómo se tumba quien duerme en <paramref name="bed"/> (MicroPiece.rest): de espaldas si no lo dice.</summary>
    public static string RestOf(Transform bed) => bed != null && bed.TryGetComponent(out MicroPiece mp) && !string.IsNullOrEmpty(mp.rest) ? mp.rest : MicroBody.Espalda;

    /// <summary>De pie junto al mueble, y a andar otra vez.</summary>
    public static void StandUp(Anima a, Vector3 at)
    {
        Transform t = a.transform;
        Transform ins = Insect(a); if (ins != null) ins.localRotation = Quaternion.identity;
        CubeInsect ci = a.GetComponent<CubeInsect>(); if (ci != null && ci.folk) ci.PoseNow(true);   // erguido otra vez (tras dormir de vientre)
        if (a.GetComponent<CreatureRig>() != null) DailyPoses.Stand().ApplyFinal(a);
        t.position = at;
        Vector3 e = t.eulerAngles; t.rotation = Quaternion.Euler(0f, e.y, 0f);
        Release(t);
    }
}
