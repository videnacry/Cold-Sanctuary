using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// TODAS LAS MÁQUINAS DEL GIMNASIO SE PUEDEN USAR (Berón 2026-10-29: «hay que probar en vivo que los humanos usan bien todas
/// las máquinas»). En la escena construida y en Play, 2–3 personas del personal de estaturas distintas (la más alta, la más
/// baja y una intermedia) recorren TODAS las <see cref="GymStation"/> (cada tipo, cada copia y cada «Sitio_» de los aparatos
/// de varios puestos) por el MISMO camino que la jornada (<see cref="WorkdayDirector.GymSession"/>): reservan la máquina,
/// andan con <see cref="PathMover"/> hasta su punto de llegada (en alfombra entre pisos), se colocan con
/// <see cref="WorkdayDirector.GymPlace"/>, hacen la repetición A → B al ritmo de <see cref="GymPoses.RepHalf"/> con
/// <see cref="WorkdayDirector.GymRep"/> y la sueltan. En cada máquina mide, al final de cada mitad (la postura ya llegada):
///  · llegó ANDANDO en un tiempo acotado y sin saltos (ningún fotograma recorre más de lo que se anda, salvo en alfombra);
///  · la reservó y la soltó (<see cref="GymStation.Claim"/> / <see cref="GymStation.Release"/>);
///  · la PELVIS a la altura de la postura (puesto + rootHeight × escala del cuerpo) ±0,15 m en A y en B;
///  · los PIES: ninguno más de 3 cm por debajo de lo que pisa (el suelo o la pieza que tiene debajo) y, si la postura se
///    apoya en ellos, los que apoyan a ≤ 5 cm de su superficie;
///  · las MANOS a ≤ 0,15 m de un agarre de la máquina (piezas «Agarre», «Asa», «Barra», «Peldano», «Anilla»… cerca del
///    puesto) cuando la postura agarra algo;
///  · sin ATRAVESAR la máquina: puntos a lo largo de cada segmento del cuerpo (no las manos) dentro de una pieza maciza;
///  · en los aparatos de varios puestos, varias personas a la vez en sitios vecinos sin solaparse entre ellas.
/// Un Check por máquina (con el tipo y los números de lo que falla) y el resumen «todas las máquinas usables: N/N».
/// Se añade solo al orquestador cuando se piden las pruebas, en su propio grupo, tras todos los demás. También se corre
/// sola con «Tools/Cold Sanctuary/Pruebas/Gimnasio: todas las máquinas (sola)».
/// </summary>
public class GymUsabilityTest : MonoBehaviour, ITestUnit
{
    public int Group => 50;   // tras todo lo demás (las iniciaciones y los microcosmos llegan hasta el 44)
    public bool ParallelSafe => false;
    [Tooltip("Velocidad del tiempo durante la prueba (el paso de la cinta, 0,35 s, aún se mide bien a ×3).")]
    public float speed = 3f;
    [Tooltip("Cuántas personas de estaturas distintas (2–3).")]
    public int people = 3;
    [Tooltip("Tope (s de juego) para llegar andando a una máquina del mismo piso; entre pisos (alfombra) se dobla.")]
    public float walkLimit = 60f;
    [Tooltip("Tope total (s reales) de la prueba.")]
    public float realLimit = 1800f;
    [Tooltip("Si no vacío: solo los tipos de esta lista (nombres de GymMachineType separados por comas).")]
    public string onlyTypes = "";

    const float PelvisTol = 0.15f, FootBelow = 0.03f, FootFloat = 0.05f, HandTol = 0.15f, Inside = 0.01f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();   // solo está activo si se pidieron las pruebas
        if (tr != null && tr.GetComponentInChildren<GymUsabilityTest>() == null) tr.gameObject.AddComponent<GymUsabilityTest>();
    }

    // ── Personas ────────────────────────────────────────────────────────────────────────────────

    class Person
    {
        public GameObject g; public Anima a; public CreatureRig rig; public Transform body; public float height;
        public Vector3 pos; public Quaternion rot;
        public IdleBehavior idle; public bool idleWas; public ActionQueue q; public bool qWas;
        public UnityEngine.AI.NavMeshAgent agent; public bool agentWas;
        public float maxJump; public string jumpAt;
        public bool done;
        public string Name => g.name;
    }

    /// <summary>Lo que falla en una máquina (con la persona y los números).</summary>
    readonly Dictionary<GymStation, List<string>> _fail = new Dictionary<GymStation, List<string>>();
    readonly Dictionary<GymStation, int> _used = new Dictionary<GymStation, int>();
    readonly Dictionary<GymStation, Equipment> _eq = new Dictionary<GymStation, Equipment>();
    float _walkSpeed = 2.6f;

    void Fail(GymStation st, string what)
    {
        if (!_fail.TryGetValue(st, out List<string> l)) _fail[st] = l = new List<string>();
        l.Add(what);
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Gimnasio: todas las máquinas usables por humanos (andando, posturas A/B, pies, manos, sin atravesar)");
        float realEnd = Time.realtimeSinceStartup + realLimit;
        HashSet<string> only = new HashSet<string>(onlyTypes.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0));
        List<GymStation> sts = GymStation.All.Where(s => s != null && s.isActiveAndEnabled && (only.Count == 0 || only.Contains(s.type.ToString()))).ToList();
        var missing = new List<string>();
        foreach (GymMachineType t in System.Enum.GetValues(typeof(GymMachineType)))
            if ((only.Count == 0 || only.Contains(t.ToString())) && !sts.Any(s => s.type == t)) missing.Add(t.ToString());
        TestProbe.Check("hay al menos una estación de cada tipo de máquina", missing.Count == 0, missing.Count == 0 ? $"{sts.Count} estaciones" : "faltan: " + string.Join(", ", missing));
        List<Person> ps = PickPeople();
        TestProbe.Check($"hay personas del personal con cuerpo de cubos para probar ({people} de estaturas distintas)", ps.Count >= Mathf.Min(2, people),
            string.Join(", ", ps.Select(p => $"{p.Name} {p.height:0.00} m")));
        if (sts.Count == 0 || ps.Count == 0) { TestProbe.End(); yield break; }

        WorkdayDirector wd = WorkdayDirector.Instance;
        _walkSpeed = wd != null ? wd.walkSpeed : 2.6f;
        bool wdWas = wd != null && wd.enabled; if (wd != null) wd.enabled = false;   // (sola, fuera del orquestador, la jornada también los movería)
        float oldScale = Time.timeScale; Time.timeScale = speed;
        foreach (Person p in ps) Hold(p);

        // ── 1. Cada persona, TODAS las máquinas (en paralelo, empezando por sitios distintos del recorrido) ──
        List<GymStation> tour = Tour(sts);
        for (int i = 0; i < ps.Count; i++)
        {
            int off = i * tour.Count / ps.Count;
            var mine = new List<GymStation>(); for (int k = 0; k < tour.Count; k++) mine.Add(tour[(off + k) % tour.Count]);
            StartCoroutine(Circuit(ps[i], mine, realEnd));
        }
        while (ps.Any(p => !p.done) && Time.realtimeSinceStartup < realEnd) yield return null;
        if (ps.Any(p => !p.done)) TestProbe.Check("el recorrido acaba dentro del tope de tiempo", false, $"{realLimit:0} s reales");
        foreach (Person p in ps) TestProbe.Check($"«{p.Name}» anda sin saltos entre máquinas (ningún fotograma más de lo que se anda)", p.maxJump <= 0f,
            p.maxJump > 0f ? $"salto de {p.maxJump:0.00} m {p.jumpAt}" : "");

        // ── 2. Varias personas a la vez en los aparatos de varios puestos ──
        if (ps.Count >= 2) yield return MultiUser(sts, ps, realEnd);

        // ── Veredicto por máquina y resumen ──
        int okN = 0;
        foreach (GymStation st in tour)
        {
            bool ok = !_fail.TryGetValue(st, out List<string> l) || l.Count == 0;
            int used = _used.TryGetValue(st, out int u) ? u : 0;
            if (used < ps.Count) { ok = false; Fail(st, $"solo la usaron {used}/{ps.Count}"); l = _fail[st]; }
            if (ok) okN++;
            string detail = ok ? $"{used} personas" : string.Join(" | ", l.Take(8)) + (l.Count > 8 ? $" | (+{l.Count - 8})" : "");
            TestProbe.Check($"usable: {GymStation.Label(st.type)} [{st.type}] #{st.copy + 1} ({st.name})", ok, detail);
        }
        TestProbe.Check($"todas las máquinas usables: {okN}/{tour.Count}", okN == tour.Count);

        foreach (Person p in ps) Restore(p);
        Time.timeScale = oldScale;
        if (wd != null) wd.enabled = wdWas;
        TestProbe.End();
    }

    List<Person> PickPeople()
    {
        string[] names = WorkdayDirector.Instance != null ? WorkdayDirector.Instance.staff
            : new[] { "Linduarda", "Lizmibet", "Goluis", "Panterilia", "Gohageneis", "Irosene", "JefeConstruccion", "Sadhguru", "Andrea" };
        var all = new List<Person>();
        foreach (string n in names)
        {
            GameObject g = WorkdayDirector.Body(n);
            if (g == null || g.CompareTag("Player") || g.GetComponent<Magnate>() != null) continue;
            if (g.TryGetComponent(out Linduarda lin) && lin.enabled) continue;   // su conducta propia la mueve (la enfermería)
            if (g.TryGetComponent(out NewcomerInitiator ni) && ni.InProgress) continue;
            Anima a = g.GetComponent<Anima>(); CreatureRig rig = g.GetComponent<CreatureRig>();
            Transform body = g.transform.Find(CubeHumanoid.BodyName);
            if (a == null || rig == null || body == null || rig.Get(BodyPart.Hips) == null) continue;
            all.Add(new Person { g = g, a = a, rig = rig, body = body, height = body.lossyScale.y * 1.9f });
        }
        all.Sort((x, y) => y.height.CompareTo(x.height));
        var pick = new List<Person>();
        if (all.Count == 0) return pick;
        pick.Add(all[0]);                                              // la más alta
        if (people >= 2 && all.Count >= 2) pick.Add(all[all.Count - 1]);   // la más baja
        if (people >= 3 && all.Count >= 3) pick.Add(all[all.Count / 2]);  // una intermedia
        return pick;
    }

    void Hold(Person p)
    {
        p.pos = p.g.transform.position; p.rot = p.g.transform.rotation;
        p.idle = p.g.GetComponent<IdleBehavior>(); p.idleWas = p.idle != null && p.idle.enabled; if (p.idle != null) p.idle.enabled = false;
        p.q = p.g.GetComponent<ActionQueue>(); p.qWas = p.q != null && p.q.enabled; if (p.q != null) { p.q.ClearOwn(); p.q.enabled = false; }
        p.agent = p.g.GetComponent<UnityEngine.AI.NavMeshAgent>(); p.agentWas = p.agent != null && p.agent.enabled; if (p.agent != null) p.agent.enabled = false;
        p.a.asleep = false;
        PathMover.Of(p.g.transform).Restart();
    }

    void Restore(Person p)
    {
        if (p.g == null) return;
        GymStation.Release(p.Name);
        DailyPoses.Stand().ApplyFinal(p.a);
        p.g.transform.SetPositionAndRotation(p.pos, p.rot);
        if (p.agent != null) { p.agent.enabled = p.agentWas; if (p.agent.enabled && p.agent.isOnNavMesh) p.agent.Warp(p.pos); }
        if (p.q != null) p.q.enabled = p.qWas;
        if (p.idle != null) p.idle.enabled = p.idleWas;
    }

    /// <summary>Recorrido: piso a piso (de abajo arriba) y, dentro de cada piso, siempre a la máquina más cercana.</summary>
    static List<GymStation> Tour(List<GymStation> sts)
    {
        var tour = new List<GymStation>();
        foreach (var floor in sts.GroupBy(s => Mathf.RoundToInt(s.ApproachPoint.y / 3f)).OrderBy(gr => gr.Key))
        {
            var left = floor.ToList();
            Vector3 at = left.OrderBy(s => s.ApproachPoint.x + s.ApproachPoint.z).First().ApproachPoint;
            while (left.Count > 0)
            {
                GymStation n = left.OrderBy(s => (s.ApproachPoint - at).sqrMagnitude).First();
                tour.Add(n); left.Remove(n); at = n.ApproachPoint;
            }
        }
        return tour;
    }

    // ── 1. El recorrido de una persona ─────────────────────────────────────────────────────────────

    IEnumerator Circuit(Person p, List<GymStation> list, float realEnd)
    {
        var queue = new Queue<GymStation>(list);
        var deferred = new Dictionary<GymStation, int>();
        while (queue.Count > 0 && Time.realtimeSinceStartup < realEnd)
        {
            GymStation st = queue.Dequeue();
            if (st == null) continue;
            // Como la jornada: la reserva ANTES de ir. Si otro la usa, más tarde.
            if (!st.Claim(p.Name))
            {
                deferred.TryGetValue(st, out int n);
                if (n < 4) { deferred[st] = n + 1; queue.Enqueue(st); yield return null; continue; }
                Fail(st, $"{p.Name}: siempre ocupada por {st.Occupant}");
                continue;
            }
            var u = new WorkdayDirector.GymUse { st = st, since = Time.time };
            yield return Walk(p, st, u);
            yield return Use(p, st, u, true);
            _used[st] = (_used.TryGetValue(st, out int c) ? c : 0) + 1;
        }
        p.done = true;
    }

    /// <summary>Anda hasta la máquina como la jornada (PathMover hacia el punto de llegada; alfombras entre pisos).</summary>
    IEnumerator Walk(Person p, GymStation st, WorkdayDirector.GymUse u)
    {
        Transform tr = p.g.transform;
        bool otherFloor = Mathf.Abs(WorkdayDirector.GymApproach(st).y - tr.position.y) > 1.5f;
        float limit = walkLimit * (otherFloor ? 3f : 1f), t0 = Time.time;
        Vector3 last = tr.position; bool rode = false;
        PathMover pm = PathMover.Of(tr);
        while (!WorkdayDirector.GymArrived(p.g, st) && Time.time - t0 < limit)
        {
            pm.StepToward(WorkdayDirector.GymApproach(st), _walkSpeed);
            yield return null;
            bool riding = FlyingCarpet.Riding(p.g);
            float jump = Vector3.Distance(tr.position, last), lim = Mathf.Max(0.6f, _walkSpeed * 3f * Time.deltaTime);
            if (!riding && !rode && jump > lim && jump > p.maxJump) { p.maxJump = jump; p.jumpAt = $"yendo a {st.name}"; }
            rode = riding; last = tr.position;
        }
        if (!WorkdayDirector.GymArrived(p.g, st))
        {
            Vector3 d = WorkdayDirector.GymApproach(st) - tr.position;
            Fail(st, $"{p.Name}: no llega andando en {limit:0} s (a {new Vector2(d.x, d.z).magnitude:0.0} m, Δy {d.y:0.0})");
            WorkdayDirector.Teleport(tr, WorkdayDirector.GymApproach(st));   // para probar igualmente las posturas (y no arrastrar el fallo)
            PathMover.Of(tr).Restart();
        }
    }

    /// <summary>Se sienta, hace A → B como la jornada (midiendo al final de cada mitad) y se levanta.</summary>
    IEnumerator Use(Person p, GymStation st, WorkdayDirector.GymUse u, bool measure)
    {
        WorkdayDirector.GymSit(u);
        if (st.Occupant != p.Name) Fail(st, $"{p.Name}: la reserva no es suya ({st.Occupant ?? "libre"})");
        float half = GymPoses.RepHalf(st.type), end = Time.time + half * 2f + 2f;
        var hands = new float[2][]; int flips = 0; bool seatedA = false;
        while (flips < 2 && Time.time < end)
        {
            if (measure && Time.time - u.phaseAt >= half)   // a punto de cambiar de fase: la postura de esta mitad ya llegó
            {
                hands[u.b ? 1 : 0] = Measure(p, st, u.b, u.spell);
                if (!u.b) seatedA = GymStation.Seated(p.g);
            }
            if (WorkdayDirector.GymRep(p.g, p.a, u)) flips++;
            yield return null;
        }
        if (measure)
        {
            if (flips < 2) Fail(st, $"{p.Name}: no completa la repetición A → B ({flips}/2 cambios)");
            if (!seatedA) Fail(st, $"{p.Name}: en A no cuenta como sentado en la máquina (GroundGuard lo movería)");
            CheckHands(p, st, hands);
        }
        WorkdayDirector.GymStandUp(p.g, u);
        GymStation.Release(p.Name);
        if (!st.Free || GymStation.Of(p.Name) != null) Fail(st, $"{p.Name}: no queda libre al soltarla ({st.Occupant})");
        PathMover.Of(p.g.transform).Restart();
    }

    // ── Mediciones ───────────────────────────────────────────────────────────────────────────────

    /// <summary>Mide pelvis, pies y penetración en la fase <paramref name="b"/>; devuelve la distancia de cada mano al agarre más cercano.</summary>
    float[] Measure(Person p, GymStation st, bool b, AsanaPoseSpell sp)
    {
        string ph = b ? "B" : "A";
        Equipment eq = EquipmentOf(st);
        float s = p.body.lossyScale.y;
        // PELVIS
        Transform hips = p.rig.Get(BodyPart.Hips);
        float want = st.SeatPoint(b ? 1f : 0f).y + (sp.rootHeight >= 0f ? sp.rootHeight : 0.95f) * s;
        float err = hips.position.y - want;
        if (Mathf.Abs(err) > PelvisTol) Fail(st, $"{p.Name} {ph}: pelvis a {hips.position.y - st.SeatPoint(b ? 1f : 0f).y:0.00} m del puesto (esperada {want - st.SeatPoint(b ? 1f : 0f).y:0.00}, Δ {err:+0.00;-0.00})");
        // PIES
        int need = Feet(st.type, b);
        var gaps = new List<(float gap, string side, string on)>();
        foreach (BodyPart f in new[] { BodyPart.FootLeft, BodyPart.FootRight })
        {
            Transform seg = Segment(p.rig, f); if (seg == null) continue;
            float bottom = LowestY(seg);
            float top = SurfaceBelow(new Vector3(seg.position.x, bottom + 0.3f, seg.position.z), eq, p.g.transform, out string on);
            if (float.IsNegativeInfinity(top)) { gaps.Add((99f, Side(f), "nada")); continue; }
            if (top - bottom > FootBelow) Fail(st, $"{p.Name} {ph}: pie {Side(f)} {(top - bottom) * 100f:0} cm bajo «{on}»");
            gaps.Add((bottom - top, Side(f), on));
        }
        gaps.Sort((x, y) => x.gap.CompareTo(y.gap));
        for (int i = 0; i < need && i < gaps.Count; i++)
            if (gaps[i].gap > FootFloat) Fail(st, $"{p.Name} {ph}: pie {gaps[i].side} flota {gaps[i].gap * 100f:0} cm sobre «{gaps[i].on}» (apoya {need})");
        // ATRAVESAR la máquina (no las manos: agarran)
        float worst = 0f; string where = "";
        foreach (CubeJoint j in p.rig.Get(BodyPart.Hips).GetComponentsInChildren<CubeJoint>())
        {
            if (!BodySegment(j.part) || j.part == BodyPart.HandLeft || j.part == BodyPart.HandRight || j.segment == null) continue;
            foreach (Vector3 pt in Samples(j.segment))
                foreach (Transform piece in eq.solid)
                {
                    float d = Depth(pt, piece);
                    if (d > worst) { worst = d; where = $"{j.part} dentro de «{piece.name}»"; }
                }
        }
        if (worst > Inside) Fail(st, $"{p.Name} {ph}: atraviesa la máquina {worst * 100f:0} cm ({where})");
        // MANOS
        var hd = new float[2];
        int hi = 0;
        foreach (BodyPart h in new[] { BodyPart.HandLeft, BodyPart.HandRight })
        {
            Transform seg = Segment(p.rig, h);
            float best = 99f;
            if (seg != null) foreach (Transform g in eq.grip) best = Mathf.Min(best, Dist(seg.position, g));
            hd[hi++] = best;
        }
        return hd;
    }

    void CheckHands(Person p, GymStation st, float[][] hands)
    {
        int ha = Hands(st.type, false), hb = Hands(st.type, true);
        bool Ok(float[] h, int n) { if (h == null) return false; float[] s = h.OrderBy(x => x).ToArray(); return n <= 0 || s[n - 1] <= HandTol; }
        string Txt(float[] h) => h == null ? "sin medir" : $"izq {h[0]:0.00} m, der {h[1]:0.00} m";
        if (ha == -2)   // máquinas de palancas: el agarre (fijo) está en una de las dos fases
        {
            if (!Ok(hands[0], 2) && !Ok(hands[1], 2)) Fail(st, $"{p.Name}: manos lejos del agarre en A ({Txt(hands[0])}) y en B ({Txt(hands[1])})");
            return;
        }
        if (ha > 0 && !Ok(hands[0], ha)) Fail(st, $"{p.Name} A: manos lejos del agarre ({Txt(hands[0])}, agarran {ha})");
        if (hb > 0 && !Ok(hands[1], hb)) Fail(st, $"{p.Name} B: manos lejos del agarre ({Txt(hands[1])}, agarran {hb})");
    }

    static string Side(BodyPart f) => f == BodyPart.FootLeft || f == BodyPart.HandLeft ? "izq." : "der.";

    static Transform Segment(CreatureRig rig, BodyPart part)
    {
        Transform b = rig.Get(part); if (b == null) return null;
        CubeJoint j = b.GetComponent<CubeJoint>();
        return j != null ? j.segment : null;
    }

    static bool BodySegment(BodyPart p)
    {
        switch (p)
        {
            case BodyPart.Hips: case BodyPart.Back: case BodyPart.Chest: case BodyPart.Neck: case BodyPart.Head:
            case BodyPart.ShoulderLeft: case BodyPart.ShoulderRight: case BodyPart.ForearmLeft: case BodyPart.ForearmRight:
            case BodyPart.HandLeft: case BodyPart.HandRight: case BodyPart.ThighLeft: case BodyPart.ThighRight:
            case BodyPart.ShinLeft: case BodyPart.ShinRight: case BodyPart.FootLeft: case BodyPart.FootRight: return true;
            default: return false;
        }
    }

    static float LowestY(Transform cube)
    {
        float y = float.MaxValue;
        for (int c = 0; c < 8; c++) y = Mathf.Min(y, cube.TransformPoint(new Vector3((c & 1) == 0 ? -0.5f : 0.5f, (c & 2) == 0 ? -0.5f : 0.5f, (c & 4) == 0 ? -0.5f : 0.5f)).y);
        return y;
    }

    /// <summary>Puntos a lo largo del eje largo de un segmento (cubo unidad escalado): centro y ±0,2/±0,4 de su largo.</summary>
    static IEnumerable<Vector3> Samples(Transform seg)
    {
        Vector3 ls = seg.lossyScale; Vector3 ax = Vector3.right; float len = Mathf.Abs(ls.x);
        if (Mathf.Abs(ls.y) > len) { ax = Vector3.up; len = Mathf.Abs(ls.y); }
        if (Mathf.Abs(ls.z) > len) { ax = Vector3.forward; }
        foreach (float f in new[] { -0.4f, -0.2f, 0f, 0.2f, 0.4f }) yield return seg.TransformPoint(ax * f);
    }

    /// <summary>Cuánto está <paramref name="p"/> DENTRO de la pieza (cubo unidad escalado), en metros; ≤ 0 = fuera.</summary>
    static float Depth(Vector3 p, Transform box)
    {
        Vector3 l = box.InverseTransformPoint(p), s = box.lossyScale;
        return Mathf.Min((0.5f - Mathf.Abs(l.x)) * Mathf.Abs(s.x), Mathf.Min((0.5f - Mathf.Abs(l.y)) * Mathf.Abs(s.y), (0.5f - Mathf.Abs(l.z)) * Mathf.Abs(s.z)));
    }

    /// <summary>Distancia de <paramref name="p"/> a la pieza (0 si está dentro).</summary>
    static float Dist(Vector3 p, Transform box)
    {
        Vector3 l = box.InverseTransformPoint(p);
        Vector3 c = new Vector3(Mathf.Clamp(l.x, -0.5f, 0.5f), Mathf.Clamp(l.y, -0.5f, 0.5f), Mathf.Clamp(l.z, -0.5f, 0.5f));
        return Vector3.Distance(p, box.TransformPoint(c));
    }

    /// <summary>Lo alto de lo que hay justo debajo de <paramref name="from"/> (piezas de la máquina, el suelo del piso o,
    /// si no, el collider que no sea de una máquina). −∞ si nada.</summary>
    static float SurfaceBelow(Vector3 from, Equipment eq, Transform self, out string on)
    {
        float best = float.NegativeInfinity; on = "nada";
        foreach (Transform piece in eq.surface)
        {
            Vector3 o = piece.InverseTransformPoint(from), d = piece.InverseTransformVector(Vector3.down);
            float tmin = float.NegativeInfinity, tmax = float.PositiveInfinity;
            bool miss = false;
            for (int i = 0; i < 3 && !miss; i++)
            {
                if (Mathf.Abs(d[i]) < 1e-6f) { if (Mathf.Abs(o[i]) > 0.5f) miss = true; continue; }
                float t1 = (-0.5f - o[i]) / d[i], t2 = (0.5f - o[i]) / d[i];
                tmin = Mathf.Max(tmin, Mathf.Min(t1, t2)); tmax = Mathf.Min(tmax, Mathf.Max(t1, t2));
            }
            if (miss || tmax < Mathf.Max(0f, tmin)) continue;
            float y = from.y - Mathf.Max(0f, tmin);   // (si ya está dentro, la superficie está al menos a la altura de partida)
            if (y > best) { best = y; on = piece.name; }
        }
        if (float.IsNegativeInfinity(best))
            foreach (RaycastHit h in Physics.RaycastAll(from, Vector3.down, 10f, ~0, QueryTriggerInteraction.Ignore))
                if (!h.transform.IsChildOf(self) && h.collider.GetComponentInParent<GymStation>() == null && h.collider.GetComponentInParent<Anima>() == null && h.point.y > best)
                { best = h.point.y; on = h.collider.name; }
        return best;
    }

    // ── El equipo de cada estación ─────────────────────────────────────────────────────────────

    class Equipment { public List<Transform> solid = new List<Transform>(), grip = new List<Transform>(), surface = new List<Transform>(); }

    static readonly string[] GripNames = { "Agarre", "Asa", "Asidero", "Barra", "Mango", "Tirador", "Manillar", "Pasamanos", "Anilla", "Peldano",
                                          "Poste_empuje", "Cuerda", "Maneta", "Tubo", "Barandilla" };
    static readonly string[] Ignored = { "Cable", "Cinta_anilla", "Cadena", "Marca", "Diana", "Tejido", "Dibujo", "Flecos", "Zona", "Borde" };

    static bool IsGrip(GymMachineType t, string n)
    {
        if (n == "Brazo" && t != GymMachineType.SillaRomana) return true;   // brazos de la elíptica y del assault bike (en la silla romana son soportes)
        if (t == GymMachineType.MuroSubida && n.StartsWith("Muro")) return true;     // el borde del muro
        if (t == GymMachineType.KongVault && (n.StartsWith("Cajon") || n.StartsWith("Tapa"))) return true;   // las manos sobre el cajón
        foreach (string g in GripNames) if (n.StartsWith(g)) return true;
        return false;
    }

    Equipment EquipmentOf(GymStation st)
    {
        if (_eq.TryGetValue(st, out Equipment e)) return e;
        e = new Equipment();
        Transform root = st.name.StartsWith("Sitio_") && st.transform.parent != null ? st.transform.parent : st.transform;
        Vector3 c = (st.SeatPoint(0f) + st.SeatPoint(1f)) * 0.5f + Vector3.up * 0.9f;
        float reach = 2.2f + st.shiftB.magnitude;
        foreach (MeshFilter mf in root.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.sharedMesh == null || !mf.sharedMesh.name.StartsWith("Cube")) continue;
            Renderer r = mf.GetComponent<Renderer>(); if (r == null || !r.enabled) continue;
            Transform t = mf.transform;
            if (t.GetComponentInParent<Anima>() != null || t.GetComponentInParent<FlyingCarpet>() != null) continue;
            if (Ignored.Any(n => t.name.StartsWith(n))) continue;
            if (Dist(c, t) > reach) continue;
            e.surface.Add(t);
            if (IsGrip(st.type, t.name)) e.grip.Add(t); else e.solid.Add(t);
        }
        // El suelo del piso (caucho, losetas, césped): lo que pisan.
        for (Transform a = root; a != null; a = a.parent)
        {
            bool floor = false;
            foreach (Transform ch in a) if (ch.name.StartsWith("Suelo_caucho")) { floor = true; break; }
            if (!floor) continue;
            foreach (Transform ch in a)
                if ((ch.name.StartsWith("Suelo") || ch.name.StartsWith("Loseta") || ch.name.StartsWith("Pista")) && !e.surface.Contains(ch)) e.surface.Add(ch);
            break;
        }
        _eq[st] = e;
        return e;
    }

    /// <summary>Manos que AGARRAN algo de la máquina en cada fase: 2, 1, 0; −2 = las dos, en A o en B (palancas que se
    /// mueven: la pieza del agarre está fija en una de las dos posiciones).</summary>
    static int Hands(GymMachineType t, bool b)
    {
        switch (t)
        {
            case GymMachineType.PressPecho: case GymMachineType.PressHombros: case GymMachineType.Remo: case GymMachineType.BicepsTriceps:
            case GymMachineType.PressBancaPlano: case GymMachineType.PressBancaInclinado: case GymMachineType.Eliptica: case GymMachineType.AssaultBike:
                return -2;
            case GymMachineType.Jalon: case GymMachineType.SentadillaJaula: case GymMachineType.SentadillaSmith: case GymMachineType.PesoMuerto:
            case GymMachineType.Cargada: case GymMachineType.RemoErgometro: case GymMachineType.SkiErg:
                return b ? 0 : 2;
            case GymMachineType.Prensa: case GymMachineType.ExtensionCuadriceps: case GymMachineType.CurlFemoral:
            case GymMachineType.BiciVertical: case GymMachineType.BiciReclinada: case GymMachineType.Escaladora: case GymMachineType.SillaRomana:
            case GymMachineType.Dominadas: case GymMachineType.Fondos: case GymMachineType.FondosAnillas: case GymMachineType.RemoAustraliano:
            case GymMachineType.FrontLever: case GymMachineType.ElevacionPiernas: case GymMachineType.LSit: case GymMachineType.BanderaHumana:
            case GymMachineType.MuroSubida: case GymMachineType.Lache: case GymMachineType.Kipping: case GymMachineType.Trineo: case GymMachineType.Cuerda:
                return 2;
            case GymMachineType.MonkeyBars: return 1;
            case GymMachineType.KongVault: return b ? 2 : 0;
            default: return 0;   // cinta, esterillas, banco declinado, hiperextensiones, saco, barandilla, saltos, wall ball, swing, GHD, hip thrust, gemelos, remo con mancuerna
        }
    }

    /// <summary>Pies que APOYAN en algo en cada fase (los que deben estar a ≤ 5 cm de su superficie).</summary>
    static int Feet(GymMachineType t, bool b)
    {
        switch (t)
        {
            case GymMachineType.PressPecho: case GymMachineType.PressHombros: case GymMachineType.Jalon: case GymMachineType.Remo:
            case GymMachineType.HipThrust: case GymMachineType.Gemelos: case GymMachineType.BicepsTriceps:
            case GymMachineType.SentadillaJaula: case GymMachineType.PressBancaPlano: case GymMachineType.PressBancaInclinado:
            case GymMachineType.SentadillaSmith: case GymMachineType.PesoMuerto: case GymMachineType.Crunch: case GymMachineType.EstiramientoIsquios:
            case GymMachineType.SacoBoxeo: case GymMachineType.RemoAustraliano: case GymMachineType.SaltoPrecision: case GymMachineType.Cargada:
            case GymMachineType.SkiErg: case GymMachineType.WallBall: case GymMachineType.SaltoCajon: case GymMachineType.Swing:
                return 2;
            case GymMachineType.RemoMancuerna: case GymMachineType.Cinta: case GymMachineType.Escaladora: case GymMachineType.Trineo: return 1;
            case GymMachineType.KongVault: return b ? 0 : 1;
            case GymMachineType.Barandilla: return b ? 2 : 1;
            default: return 0;   // colgados, sentados con las piernas libres, pedales (fijos), tumbados con los pies enganchados
        }
    }

    // ── 2. Varias personas a la vez ──────────────────────────────────────────────────────────────

    IEnumerator MultiUser(List<GymStation> sts, List<Person> ps, float realEnd)
    {
        var groups = sts.Where(s => s.name.StartsWith("Sitio_") && s.transform.parent != null).GroupBy(s => s.transform.parent).Where(gr => gr.Count() >= 2);
        int batches = 0, bad = 0;
        foreach (var gr in groups)
        {
            var list = gr.OrderBy(s => s.transform.position.x).ThenBy(s => s.transform.position.z).ToList();
            for (int i = 0; i < list.Count; i += ps.Count)
            {
                if (Time.realtimeSinceStartup > realEnd) yield break;
                int from = Mathf.Min(i, Mathf.Max(0, list.Count - ps.Count));
                var batch = list.Skip(from).Take(ps.Count).ToList();
                if (batch.Count < 2) continue;
                batches++;
                var uses = new WorkdayDirector.GymUse[batch.Count];
                for (int k = 0; k < batch.Count; k++)
                {
                    batch[k].Claim(ps[k].Name);
                    uses[k] = new WorkdayDirector.GymUse { st = batch[k], since = Time.time };
                    StartCoroutine(Walk(ps[k], batch[k], uses[k]));
                }
                float until = Time.time + walkLimit * 3f + 5f;
                while (Time.time < until && Enumerable.Range(0, batch.Count).Any(k => !WorkdayDirector.GymArrived(ps[k].g, batch[k]))) yield return null;
                yield return null;   // (los Walk acaban este fotograma)
                for (int k = 0; k < batch.Count; k++) WorkdayDirector.GymSit(uses[k]);
                float half = batch.Max(s => GymPoses.RepHalf(s.type)), end = Time.time + half * 2f + 0.2f;
                float worst = 0f; string where = "";
                while (Time.time < end)
                {
                    for (int k = 0; k < batch.Count; k++) WorkdayDirector.GymRep(ps[k].g, ps[k].a, uses[k]);
                    for (int x = 0; x < batch.Count; x++)
                        for (int y = 0; y < batch.Count; y++)
                        {
                            if (x == y) continue;
                            float d = Overlap(ps[x], ps[y], out string w);
                            if (d > worst) { worst = d; where = $"{ps[x].Name} ({batch[x].name}) ↔ {ps[y].Name} ({batch[y].name}): {w}"; }
                        }
                    yield return null;
                }
                for (int k = 0; k < batch.Count; k++) { WorkdayDirector.GymStandUp(ps[k].g, uses[k]); GymStation.Release(ps[k].Name); }
                if (worst > Inside)
                {
                    bad++;
                    foreach (GymStation s in batch) Fail(s, $"a la vez con {batch.Count - 1} más se solapan {worst * 100f:0} cm: {where}");
                }
            }
        }
        TestProbe.Check($"aparatos de varios puestos: varias personas a la vez sin solaparse ({batches - bad}/{batches} tandas)", bad == 0);
    }

    /// <summary>Cuánto se mete <paramref name="x"/> en el cuerpo de <paramref name="y"/> (m; ≤ 0 = nada).</summary>
    static float Overlap(Person x, Person y, out string where)
    {
        float worst = 0f; where = "";
        var ys = y.rig.Get(BodyPart.Hips).GetComponentsInChildren<CubeJoint>().Where(j => BodySegment(j.part) && j.segment != null).ToList();
        foreach (CubeJoint j in x.rig.Get(BodyPart.Hips).GetComponentsInChildren<CubeJoint>())
        {
            if (!BodySegment(j.part) || j.segment == null) continue;
            foreach (Vector3 pt in Samples(j.segment))
                foreach (CubeJoint k in ys)
                {
                    float d = Depth(pt, k.segment);
                    if (d > worst) { worst = d; where = $"{j.part} en {k.part}"; }
                }
        }
        return worst;
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Cold Sanctuary/Pruebas/Gimnasio: todas las máquinas (sola)")]
    static void Solo()
    {
        if (!Application.isPlaying) { Debug.Log("[Solo] no está en Play"); return; }
        var go = new GameObject("SoloGimnasio"); DontDestroyOnLoad(go);
        GymUsabilityTest t = go.AddComponent<GymUsabilityTest>();
        if (System.IO.File.Exists("Temp/gym_tipos.txt")) t.onlyTypes = System.IO.File.ReadAllText("Temp/gym_tipos.txt").Replace('\n', ',').Replace('\r', ',');
        t.StartCoroutine(Wrap(t));
    }

    static IEnumerator Wrap(GymUsabilityTest t)
    {
        TestProbe.ResetTotals();
        yield return t.StartCoroutine(t.Run());
        TestProbe.Total();
        Debug.Log("[Solo] listo");
    }
#endif
}
