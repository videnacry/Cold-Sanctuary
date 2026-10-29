using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GymFit;

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
    public float realLimit = 3000f;
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
        public CarnivoreKeeper keeper; public bool keeperWas;
        public float maxJump; public string jumpAt;
        public bool done;
        public string Name => g.name;
    }

    /// <summary>Lo que falla en una máquina (con la persona y los números).</summary>
    readonly Dictionary<GymStation, List<string>> _fail = new Dictionary<GymStation, List<string>>();
    readonly Dictionary<GymStation, int> _used = new Dictionary<GymStation, int>();
    float _walkSpeed = 2.6f;

    List<string> _sink;   // (mientras se mide una mitad: sus fallos esperan a que acabe)
    void Fail(GymStation st, string what)
    {
        if (_sink != null) { _sink.Add(what); return; }
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
        // Que el personal ya sea del santuario (al hacerse miembro, SanctuaryDirector lo coloca de golpe en su área): tope 60 s.
        float mEnd = Time.realtimeSinceStartup + 120f; int members = 0;
        while (Time.realtimeSinceStartup < mEnd)
        {
            members = 0;
            foreach (Anima a0 in FindObjectsByType<Anima>(FindObjectsSortMode.None)) if (a0 != null && !a0.CompareTag("Player") && Magnate.IsMember(a0) && a0.GetComponent<CreatureRig>() != null) members++;
            if (members >= people + 2 && !Arriving()) break;
            yield return new WaitForSecondsRealtime(1f);
        }
        yield return new WaitForSecondsRealtime(1f);
        List<Person> ps = PickPeople();
        TestProbe.Check($"hay personas del personal con cuerpo de cubos para probar ({people} de estaturas distintas)", ps.Count >= Mathf.Min(2, people),
            string.Join(", ", ps.Select(p => $"{p.Name} {p.height:0.00} m")));
        if (sts.Count == 0 || ps.Count == 0) { TestProbe.End(); yield break; }

        WorkdayDirector wd = WorkdayDirector.Instance;
        _walkSpeed = wd != null ? wd.walkSpeed : 2.6f;
        WorkdayDirector.LogTeleports = true;
        bool wdWas = wd != null && wd.enabled; if (wd != null) wd.enabled = false;   // (sola, fuera del orquestador, la jornada también los movería)
        float oldScale = Time.timeScale, oldMax = Time.maximumDeltaTime; Time.timeScale = speed;
        Time.maximumDeltaTime = 0.04f;   // cada fotograma avanza poco: la cinta (0,35 s por mitad) se ve en varios fotogramas aunque vayan lentos
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
        Time.timeScale = oldScale; Time.maximumDeltaTime = oldMax;
        WorkdayDirector.LogTeleports = false;
        if (wd != null) wd.enabled = wdWas;
        TestProbe.End();
    }

    /// <summary>¿Queda alguien del personal por colocar en su área de llegada? (SanctuaryDirector lo teletransporta allí.)</summary>
    public static bool Arriving()
    {
        foreach (WorldCharacter w in FindObjectsByType<WorldCharacter>(FindObjectsSortMode.None))
            if (w != null && w.isNewArrival && !w.CompareTag("Player")) return true;
        return false;
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
            if (RestaurantService.Drives(g) || Orchestra.Busy(g)) continue;   // los lleva el servicio del restaurante o una orquesta
            Anima a = g.GetComponent<Anima>(); CreatureRig rig = g.GetComponent<CreatureRig>();
            Transform body = g.transform.Find(CubeHumanoid.BodyName);
            if (a == null || rig == null || body == null || rig.Get(BodyPart.Hips) == null) continue;
            if (!Magnate.IsMember(a)) continue;   // quien aún no es del santuario: al serlo, SanctuaryDirector lo coloca de golpe en su área
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
        p.keeper = p.g.GetComponent<CarnivoreKeeper>(); p.keeperWas = p.keeper != null && p.keeper.enabled; if (p.keeper != null) p.keeper.enabled = false;   // (el guardia lleva raciones al comedero de los carnívoros)
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
        if (p.keeper != null) p.keeper.enabled = p.keeperWas;
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
            float w0 = Time.time, r0 = Time.realtimeSinceStartup;
            int before = _fail.TryGetValue(st, out List<string> fl) ? fl.Count : 0;
            yield return Walk(p, st, u);
            float walked = Time.time - w0;
            yield return Use(p, st, u, true);
            _used[st] = (_used.TryGetValue(st, out int c) ? c : 0) + 1;
            int after = _fail.TryGetValue(st, out fl) ? fl.Count : 0;
            Debug.Log($"[GymTest] {p.Name} · {st.name} [{st.type}] ({list.Count - queue.Count}/{list.Count}) andando {walked:0.0} s · {Time.realtimeSinceStartup - r0:0.0} s reales · {(after > before ? string.Join(" | ", fl.Skip(before)) : "OK")}");
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
            if (!riding && !rode && jump > lim && jump > p.maxJump) { p.maxJump = jump; p.jumpAt = $"yendo a {st.name}: de ({last.x:0.0},{last.y:0.0},{last.z:0.0}) a ({tr.position.x:0.0},{tr.position.y:0.0},{tr.position.z:0.0})"; }
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
        var hands = new float[2][]; int flips = 0, frames = 0; bool seatedA = false; float travel = 0f, seatOff = 0f;
        // Se mide JUSTO TRAS colocar el cuerpo (como en la jornada), en el último fotograma de cada mitad con la postura ya
        // llegada (≥ 75 % de la mitad); al cambiar de fase se da por buena la última medida de la mitad que acaba.
        List<string> pendFails = null; float[] pendHands = null; bool pendB = false, pendSeated = false; float pendOff = 0f;
        Vector3 placed = p.g.transform.position; float foreign = 0f;
        while (flips < 2 && (Time.time < end || frames < 90))   // (fotogramas: con el tiempo acelerado un fotograma lento salta segundos)
        {
            if (frames > 0) foreign = Mathf.Max(foreign, Vector3.Distance(p.g.transform.position, placed));   // ¿lo movió otro entre fotogramas?
            if (measure && pendFails == null && Time.time - u.phaseAt >= half)   // va a cambiar de fase sin haberla medido (fotogramas lentos): la del fotograma anterior
            {
                _sink = new List<string>(); pendHands = Measure(p, st, u.b, u.spell); pendB = u.b; pendFails = _sink; _sink = null;
                pendSeated = GymStation.Seated(p.g); Vector3 sd0 = p.g.transform.position - st.SeatPoint(); sd0.y = 0f; pendOff = sd0.magnitude;
            }
            bool flip = WorkdayDirector.GymRep(p.g, p.a, u);
            placed = p.g.transform.position;
            if (flip)
            {
                flips++;
                if (pendFails != null) { foreach (string s2 in pendFails) Fail(st, s2); hands[pendB ? 1 : 0] = pendHands; if (!pendB) { seatedA = pendSeated; seatOff = pendOff; } }
                pendFails = null;
            }
            else if (measure && pendFails == null && Time.time - u.phaseAt >= 0.62f * half)   // una vez por mitad, ya llegada la postura (60 %)
            {
                _sink = new List<string>();
                pendHands = Measure(p, st, u.b, u.spell); pendB = u.b;
                pendFails = _sink; _sink = null;
                Photo(p, st, u.b);
                pendSeated = GymStation.Seated(p.g); Vector3 sd = p.g.transform.position - st.SeatPoint(); sd.y = 0f; pendOff = sd.magnitude;
            }
            travel = Mathf.Max(travel, GymFit.LastTravel(st));
            frames++;
            yield return null;
        }
        if (measure && foreign > 0.05f) Fail(st, $"{p.Name}: otro sistema lo mueve mientras usa la máquina ({foreign:0.00} m entre fotogramas)");
        if (measure && travel > 1.5f) Fail(st, $"{p.Name}: el agarre móvil tiene que desplazarse {travel:0.00} m para llegar a sus manos (recorrido > 1,5 m)");
        if (measure)
        {
            if (flips < 2) Fail(st, $"{p.Name}: no completa la repetición A → B ({flips}/2 cambios)");
            if (!seatedA) Fail(st, $"{p.Name}: en A no cuenta como sentado en la máquina (a {seatOff:0.00} m del puesto; GroundGuard lo movería; ocupa {GymStation.Of(p.Name)?.name ?? "nada"})");
            CheckHands(p, st, hands);
        }
        WorkdayDirector.GymStandUp(p.g, u);
        GymStation.Release(p.Name);
        if (!st.Free || GymStation.Of(p.Name) != null) Fail(st, $"{p.Name}: no queda libre al soltarla ({st.Occupant})");
        PathMover.Of(p.g.transform).Restart();
    }

    // ── Fotos (opcional: Temp/gym_fotos.txt con tipos, uno por línea) ─────────────────────────────────

    readonly HashSet<string> _shot = new HashSet<string>();
    HashSet<string> _photoTypes;

    void Photo(Person p, GymStation st, bool b)
    {
        if (_photoTypes == null)
        {
            _photoTypes = new HashSet<string>();
            if (System.IO.File.Exists("Temp/gym_fotos.txt"))
                foreach (string l in System.IO.File.ReadAllLines("Temp/gym_fotos.txt")) if (l.Trim().Length > 0) _photoTypes.Add(l.Trim());
        }
        string key = $"{st.type}_{st.copy}_{p.Name}_{(b ? "B" : "A")}";
        if (!_photoTypes.Contains(st.type.ToString()) || st.copy != 0 || !_shot.Add(key)) return;
        System.IO.Directory.CreateDirectory("Temp/PoseGallery");
        Vector3 c = p.rig.Get(BodyPart.Hips).position, side = st.Facing * Vector3.right, fwd = st.Facing * Vector3.forward;
        var go = new GameObject("CamGym"); var cam = go.AddComponent<Camera>(); cam.fieldOfView = 55f;
        go.transform.position = c + side * 2.4f + fwd * 0.8f + Vector3.up * 0.5f; go.transform.LookAt(c);
        var rt = new RenderTexture(960, 720, 24); cam.targetTexture = rt; cam.Render();
        RenderTexture.active = rt; var tex = new Texture2D(960, 720, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 960, 720), 0, 0); tex.Apply(); RenderTexture.active = null;
        System.IO.File.WriteAllBytes($"Temp/PoseGallery/Gym_{key}.png", tex.EncodeToPNG());
        Destroy(tex); Destroy(go); rt.Release();
    }

    // ── Mediciones ───────────────────────────────────────────────────────────────────────────────

    /// <summary>Mide pelvis, pies y penetración en la fase <paramref name="b"/>; devuelve la distancia de cada mano al agarre más cercano.</summary>
    float[] Measure(Person p, GymStation st, bool b, AsanaPoseSpell sp)
    {
        string ph = b ? "B" : "A";
        Equipment eq = GymFit.Of(st);
        float s = p.body.lossyScale.y;
        // PELVIS
        Transform hips = p.rig.Get(BodyPart.Hips);
        float want = st.SeatPoint(b ? 1f : 0f).y + (sp.rootHeight >= 0f ? sp.rootHeight : 0.95f) * s + GymFit.LastDy(st);   // (más lo que el cuerpo se ajustó a la máquina: GymFit)
        float err = hips.position.y - want;
        Vector3 off = p.g.transform.position - st.SeatPoint(b ? 1f : 0f); off.y = 0f;
        // (las posturas apoyadas en el suelo del cuerpo —las 10 de placas— no usan rootHeight: la pelvis sale de apoyar el cuerpo)
        if (!sp.ground && Mathf.Abs(err) > PelvisTol) Fail(st, $"{p.Name} {ph}: pelvis a {hips.position.y - st.SeatPoint(b ? 1f : 0f).y:0.00} m del puesto (esperada {want - st.SeatPoint(b ? 1f : 0f).y:0.00}, Δ {err:+0.00;-0.00}; pelvis local {hips.localPosition.y:0.00}, ajuste {GymFit.LastDy(st):+0.00;-0.00}, a {off.magnitude:0.00} m del puesto)");
        // PIES
        int need = Feet(st.type, b);
        var gaps = new List<(float gap, string side, string on)>();
        foreach (BodyPart f in new[] { BodyPart.FootLeft, BodyPart.FootRight })
        {
            Transform seg = Segment(p.rig, f); if (seg == null) continue;
            float gap = FootGap(seg, eq, p.g.transform, out string on);
            if (float.IsInfinity(gap)) { gaps.Add((99f, Side(f), "nada")); continue; }
            if (-gap > FootBelow) Fail(st, $"{p.Name} {ph}: pie {Side(f)} {-gap * 100f:0} cm bajo «{on}» (planta a {LowestY(seg) - st.SeatPoint(b ? 1f : 0f).y:0.000} m del puesto, ajuste {GymFit.LastDy(st):0.000}, f{Time.frameCount})");
            gaps.Add((gap, Side(f), on));
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
                    float d = Depth(pt, piece) - (Soft(piece.name) ? 0.02f : 0f);   // (los acolchados ceden 2 cm)
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

    static readonly string[] SoftNames = { "Almohadilla", "Respaldo", "Asiento", "Banco", "Rodillo", "Sillin", "Apoy", "Esterilla", "Saco" };
    static bool Soft(string n) => SoftNames.Any(s => n.StartsWith(s));

    static string Side(BodyPart f) => f == BodyPart.FootLeft || f == BodyPart.HandLeft ? "izq." : "der.";


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


    /// <summary>Puntos a lo largo del eje largo de un segmento (cubo unidad escalado): centro y ±0,2/±0,4 de su largo.</summary>
    static IEnumerable<Vector3> Samples(Transform seg)
    {
        Vector3 ls = seg.lossyScale; Vector3 ax = Vector3.right; float len = Mathf.Abs(ls.x);
        if (Mathf.Abs(ls.y) > len) { ax = Vector3.up; len = Mathf.Abs(ls.y); }
        if (Mathf.Abs(ls.z) > len) { ax = Vector3.forward; }
        foreach (float f in new[] { -0.4f, -0.2f, 0f, 0.2f, 0.4f }) yield return seg.TransformPoint(ax * f);
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
        GymFit.Debug_ = System.IO.File.Exists("Temp/gym_debug.txt");
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
