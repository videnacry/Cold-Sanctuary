using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// LLEVAR A UN SER (paciente o tripulante) dentro de un vehículo: se le quitan, mientras va, las conductas que lo moverían
/// (IA, NavMeshAgent, nado del mar, merodeo) y se cuelga del sitio que le toca (camilla, asiento, pozo médico); al bajar se le
/// devuelven tal cual y se le deja de pie (o tumbado) donde toca, con su agente en el NavMesh. Nada de teletransportes: quien
/// lo lleva lo MUEVE (rampa, camilla, grúa) con <see cref="MoveTowards"/>.
/// </summary>
public class Carried : MonoBehaviour
{
    readonly List<Behaviour> _off = new List<Behaviour>();
    bool _kinematic, _gravity; Rigidbody _rb;
    Transform _oldParent;

    static readonly System.Type[] Keep = { typeof(Anima), typeof(BloodLoss), typeof(WorldLabel), typeof(Carried) };

    public static Carried Take(GameObject g, Transform holder)
    {
        if (g == null) return null;
        Carried c = g.GetComponent<Carried>();
        if (c == null) c = g.AddComponent<Carried>();
        c._oldParent = g.transform.parent;
        c._off.Clear();
        foreach (Behaviour b in g.GetComponents<Behaviour>())
        {
            if (b == null || !b.enabled) continue;
            bool keep = false; foreach (System.Type t in Keep) if (t.IsInstanceOfType(b) && !(b is Animal)) keep = true;
            if (b is Anima && !(b is Animal)) keep = true;   // (las personas: su Anima sigue viva —hambre, enfermedad—; los animales: su IA se para)
            if (keep) continue;
            b.enabled = false; c._off.Add(b);
        }
        c._rb = g.GetComponent<Rigidbody>();
        if (c._rb != null) { c._kinematic = c._rb.isKinematic; c._gravity = c._rb.useGravity; c._rb.isKinematic = true; c._rb.useGravity = false; }
        g.transform.SetParent(holder, true);
        return c;
    }

    /// <summary>Lo baja en <paramref name="at"/> (pies), mirando a <paramref name="facing"/>, y le devuelve sus conductas.</summary>
    public void Release(Vector3 at, Quaternion facing)
    {
        transform.SetParent(_oldParent, true);
        transform.SetPositionAndRotation(at, facing);
        if (_rb != null) { _rb.isKinematic = _kinematic; _rb.useGravity = _gravity; }
        foreach (Behaviour b in _off) if (b != null) b.enabled = true;
        NavMeshAgent ag = GetComponent<NavMeshAgent>();
        if (ag != null && ag.enabled && NavMesh.SamplePosition(at, out NavMeshHit hit, 3f, NavMesh.AllAreas)) ag.Warp(hit.position);
        _off.Clear();
        Destroy(this);
    }

    /// <summary>Un paso de la camilla / la grúa hacia <paramref name="to"/> (mundo). true al llegar.</summary>
    public static bool MoveTowards(Transform t, Vector3 to, float speed)
    {
        t.position = Vector3.MoveTowards(t.position, to, speed * Time.deltaTime);
        return (t.position - to).sqrMagnitude < 0.0025f;
    }
}

/// <summary>
/// EL SUBMARINO-AMBULANCIA ANFIBIO DEL HOSPITAL (Berón 2026-11-03; docs/submarinos-medicos.md): «crear un submarino para el
/// hospital de modo que se pueda trasladar personas y animales… equipado para brindar servicios de emergencia a los animales y
/// personas más grandes terrestres». Va por el hielo con orugas, por el mar sumergido y POR DENTRO DE LOS EDIFICIOS-ÁRBOL: cabe
/// por sus puertas (16 × 5 × 5 m; las puertas, 6 × 6 m) y sube nadando por el tronco hasta la puerta del piso del hospital.
/// - Bodega de 7,5 × 3,8 × 4 m: cabe un oso polar o un elefante asiático tumbado (hasta ~5 t: grúa de 8 t y trineo de carga),
///   camilla de personas, oxígeno (concentrador PSA y 2 bombonas), monitor multiparamétrico, desfibrilador, ventilador, nevera
///   de sangre O− (4 unidades, como los helicópteros médicos que llevan sangre: London's Air Ambulance desde 2012) y fluidos.
/// - LA MISIÓN (<see cref="Dispatch"/>): sube por el tronco a la puerta de urgencias a recoger al EQUIPO (dos del personal del
///   hospital de turno: <see cref="WorkdayDirector.InArea"/>; los veterinarios si es un animal), sale, va al paciente, lo
///   carga por la rampa de popa, lo atiende por el camino (oxígeno, monitor, sangre o fluidos) y vuelve a la puerta del piso que
///   le toca (urgencias o veterinaria), lo descarga en camilla hasta el puesto, el equipo lo atiende (<see cref="HospitalStation.Treat"/>,
///   <see cref="BloodBank.Transfuse"/>) y el submarino vuelve a su sitio, aparcado junto a la puerta del suelo del árbol.
/// - Mientras van, la jornada no mueve al equipo (<see cref="Engaged"/>). Todo con tope de tiempo: si algo no llega, lo dice.
/// </summary>
public class HospitalSubmarine : MonoBehaviour
{
    public const float Length = 16f, Beam = 5f, Height = 5f;
    public static readonly Vector3 Bay = new Vector3(3.8f, 4f, 7.5f);   // ancho, alto, largo de la bodega
    public const int BloodCapacity = 4;
    public float driveSpeed = 8f, swimSpeed = 5f, climbSpeed = 3f, turnSpeed = 60f, stretcherSpeed = 1.4f;
    public string building = "Servicios";
    public Vector3 parkPos; public Quaternion parkRot = Quaternion.identity;
    public int bloodUnits = BloodCapacity, fluidBags = 20;
    public float oxygenLitres = 6800f;

    public enum Phase { Aparcado, ARecogerEquipo, EmbarqueEquipo, AlPaciente, Carga, AlHospital, Descarga, Vuelta }
    public Phase phase = Phase.Aparcado;
    public Anima patient;
    public readonly List<GameObject> crew = new List<GameObject>();
    public string status = "Aparcado";
    public bool delivered; public string deliveredTo; public float missionSeconds, deliveredAfter;
    public int transfusedOnBoard;
    public readonly List<string> log = new List<string>();

    public static readonly List<HospitalSubmarine> All = new List<HospitalSubmarine>();
    static readonly HashSet<GameObject> _engaged = new HashSet<GameObject>();
    public static bool Engaged(GameObject g) => g != null && _engaged.Contains(g);
    void OnEnable() { if (!All.Contains(this)) All.Add(this); if (parkPos == Vector3.zero) { parkPos = transform.position; parkRot = transform.rotation; } }
    void OnDisable() { All.Remove(this); }

    Transform BayT => transform.Find("Bodega") ?? transform;
    Transform Seat(int i) => transform.Find($"Cabina/Asiento_{i + 1}") ?? transform;
    float FloorToCenter => Height * 0.5f + 0.2f;

    public bool FitsDoor(out string why)
    {
        float w = TreeSpec.DoorWidth, h = TreeSpec.DoorHeight;
        why = $"puerta de {w:0.#} × {h:0.#} m, submarino de {Beam:0.#} × {Height:0.#} m";
        return w >= Beam + 0.4f && h >= Height + 0.4f;
    }

    void Say(string s) { status = s; log.Add(s); Debug.Log($"[Submarino] {s}"); }

    public bool Dispatch(Anima p)
    {
        if (phase != Phase.Aparcado || p == null) return false;
        StartCoroutine(Mission(p));
        return true;
    }

    // ── EL HOSPITAL EN SU ÁRBOL ─────────────────────────────────────────────────────────────────────────────

    /// <summary>El árbol del hospital y el nivel (piso) del puesto de <paramref name="service"/> más bajo.</summary>
    public static bool FindFloor(HospitalService service, out ArbolCampos ac, out TreeLevel level, out HospitalStation station)
    {
        ac = null; level = null; station = null;
        foreach (HospitalStation s in HospitalStation.All)
        {
            if (s == null || s.service != service) continue;
            if (station != null && s.transform.position.y >= station.transform.position.y - 0.5f && !(s.doctorPost && !station.doctorPost)) continue;
            station = s;
        }
        if (station == null) return false;
        foreach (ArbolCampos t in ArbolCampos.All)
        {
            if (t == null) continue;
            TreeLevel best = null; float bd = float.MaxValue;
            foreach (TreeLevel l in t.levels)
            {
                if (l.kind != TreeLevel.Kind.Floor || l.deck == null || !station.transform.IsChildOf(l.deck)) continue;
                float dy = Mathf.Abs(t.World(l.door).y - station.transform.position.y);
                if (dy < bd) { bd = dy; best = l; }
            }
            if (best != null) { ac = t; level = best; return true; }
        }
        return false;
    }

    List<GameObject> PickCrew(bool animal)
    {
        var l = new List<GameObject>();
        var pool = new List<GameObject>();
        if (WorkdayDirector.Instance != null) pool.AddRange(WorkdayDirector.Instance.InArea(AreaWork.Hospital));
        foreach (string n in new[] { "Lizmibet", "Linduarda" }) { GameObject g = GameObject.Find(n); if (g != null && !pool.Contains(g)) pool.Add(g); }
        foreach (GameObject g in pool)
        {
            if (g == null || g.CompareTag("Player") || g.GetComponent<Anima>() == null || Engaged(g)) continue;
            if (g.GetComponent<VetCare>() is VetCare vc && vc.Busy) continue;
            l.Add(g);
            if (l.Count == 2) break;
        }
        return l;
    }

    // ── MOVERSE ───────────────────────────────────────────────────────────────────────────────────────────

    static readonly RaycastHit[] _hits = new RaycastHit[12];

    /// <summary>Alto del CENTRO del submarino en (x, z): sobre el hielo, con las orugas; en el mar, sumergido bajo las placas.</summary>
    float TravelY(Vector3 p, float prevY)
    {
        int n = Physics.RaycastNonAlloc(new Vector3(p.x, 8f, p.z), Vector3.down, _hits, 30f, ~0, QueryTriggerInteraction.Ignore);
        float best = float.NegativeInfinity;
        for (int i = 0; i < n; i++)
        {
            Collider c = _hits[i].collider;
            if (c.transform.IsChildOf(transform) || c.GetComponentInParent<Anima>() != null || c.GetComponentInParent<WorldCharacter>() != null) continue;
            if (_hits[i].point.y > 1.5f) return prevY;   // algo encima del hielo (un árbol, un mueble): no lo escala, sigue a su altura
            best = Mathf.Max(best, _hits[i].point.y);
        }
        if (best > -0.6f) return best + FloorToCenter;   // hielo
        return -0.25f - 1.7f - Height * 0.5f;            // mar: el lomo a 1,7 m bajo el agua (por debajo de las placas de hielo, 1,5 m)
    }

    IEnumerator MoveTo(Vector3 to, float speed, bool turn = true, float cap = 0f)
    {
        float t0 = Time.time, d0 = Vector3.Distance(transform.position, to);
        if (cap <= 0f) cap = d0 / Mathf.Max(0.1f, speed) * 3f + 20f;
        while ((transform.position - to).sqrMagnitude > 0.01f)
        {
            Vector3 flat = to - transform.position; flat.y = 0f;
            if (turn && flat.sqrMagnitude > 0.25f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(flat.normalized), turnSpeed * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, to, speed * Time.deltaTime);
            if (Time.time - t0 > cap) { Say($"no llega a ({to.x:0}, {to.y:0}, {to.z:0}) en {cap:0} s: sigue desde donde está"); yield break; }
            yield return null;
        }
    }

    IEnumerator TurnTo(Vector3 dir)
    {
        dir.y = 0f; if (dir.sqrMagnitude < 1e-4f) yield break;
        Quaternion q = Quaternion.LookRotation(dir.normalized);
        float t0 = Time.time;
        while (Quaternion.Angle(transform.rotation, q) > 0.5f && Time.time - t0 < 20f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, turnSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = q;
    }

    /// <summary>Por tierra y mar hasta <paramref name="to"/> (en planta): rodea los troncos de los árboles y sigue el terreno.</summary>
    IEnumerator Travel(Vector3 to, float standOff = 0f)
    {
        var pts = new List<Vector3>();
        Vector3 from = transform.position;
        Vector3 goal = to;
        if (standOff > 0f) { Vector3 d = from - to; d.y = 0f; goal = to + (d.sqrMagnitude > 1e-3f ? d.normalized : Vector3.forward) * standOff; }
        // Rodeo de los troncos (y de su alcance a ras de suelo).
        var legs = new List<Vector3> { from };
        foreach (ArbolCampos ac in ArbolCampos.All)
        {
            if (ac == null) continue;
            Vector3 c = ac.transform.position; float r = ac.waterHalf + 1f + Beam + 2f;
            Vector3 a = legs[legs.Count - 1], ab = goal - a; ab.y = 0f;
            float len = ab.magnitude; if (len < 1f) continue;
            Vector3 u = ab / len, ac2 = c - a; ac2.y = 0f;
            float s = Vector3.Dot(ac2, u);
            if (s < 0f || s > len) continue;
            Vector3 closest = a + u * s; closest.y = 0f; Vector3 cc = new Vector3(c.x, 0f, c.z);
            if ((closest - cc).magnitude >= r) continue;
            Vector3 side = Vector3.Cross(Vector3.up, u); if (Vector3.Dot(closest - cc, side) < 0f) side = -side;
            legs.Add(cc + side * (r + 3f) - u * r); legs.Add(cc + side * (r + 3f) + u * r);
        }
        legs.Add(goal);
        float y = from.y;
        for (int i = 1; i < legs.Count; i++)
        {
            Vector3 a = legs[i - 1], b = legs[i]; float len = Vector3.Distance(new Vector3(a.x, 0f, a.z), new Vector3(b.x, 0f, b.z));
            int steps = Mathf.Max(1, Mathf.CeilToInt(len / 3f));
            for (int k = 1; k <= steps; k++)
            {
                Vector3 p = Vector3.Lerp(a, b, k / (float)steps);
                y = TravelY(p, y); p.y = y;
                pts.Add(p);
            }
        }
        foreach (Vector3 p in pts) { bool water = p.y < -1f; yield return MoveTo(p, water ? swimSpeed : driveSpeed, true, 30f); }
    }

    /// <summary>Del sitio donde esté a la PUERTA de <paramref name="lv"/> por dentro del tronco: hasta la puerta del suelo, dentro
    /// (de frente), por el eje del tronco hasta la altura del piso, gira la popa hacia su puerta y atraca con la rampa en ella.</summary>
    IEnumerator TrunkTo(ArbolCampos ac, TreeLevel lv)
    {
        TreeLevel g = ac.Ground;
        Vector3 axis = ac.transform.position;
        Vector3 gOut = ac.World(g.outside), gDoor = ac.World(g.door);
        Vector3 outDir = gOut - gDoor; outDir.y = 0f; outDir.Normalize();
        float gy = gDoor.y + FloorToCenter;
        Vector3 front = gOut + outDir * (Length * 0.5f + 3f); front.y = gy;
        if (!InTrunk(ac))
        {
            yield return Travel(front);
            yield return MoveTo(front, driveSpeed, true, 30f);
            yield return TurnTo(-outDir);
            Say($"entra por la puerta del suelo de «{ac.building}» ({TreeSpec.DoorWidth:0.#} × {TreeSpec.DoorHeight:0.#} m)");
            yield return MoveTo(new Vector3(axis.x, gy, axis.z), swimSpeed, false, 30f);
        }
        Vector3 door = ac.World(lv.door);
        float ly = door.y + FloorToCenter;
        yield return MoveTo(new Vector3(axis.x, ly, axis.z), climbSpeed, false, 90f);
        Vector3 dd = door - axis; dd.y = 0f; dd.Normalize();
        yield return TurnTo(-dd);
        Vector3 dock = door - dd * (Length * 0.5f + 0.4f); dock.y = ly;
        yield return MoveTo(dock, swimSpeed * 0.5f, false, 30f);
        Say($"atraca con la rampa de popa en la puerta de {lv.name}");
    }

    IEnumerator TrunkOut(ArbolCampos ac)
    {
        TreeLevel g = ac.Ground;
        Vector3 axis = ac.transform.position, gOut = ac.World(g.outside), gDoor = ac.World(g.door);
        Vector3 outDir = gOut - gDoor; outDir.y = 0f; outDir.Normalize();
        float gy = gDoor.y + FloorToCenter;
        yield return MoveTo(new Vector3(axis.x, transform.position.y, axis.z), swimSpeed * 0.5f, false, 30f);
        yield return MoveTo(new Vector3(axis.x, gy, axis.z), climbSpeed, false, 90f);
        yield return TurnTo(outDir);
        Vector3 front = gOut + outDir * (Length * 0.5f + 3f); front.y = gy;
        yield return MoveTo(front, swimSpeed, false, 30f);
        Say($"sale del tronco de «{ac.building}» por la puerta del suelo");
    }

    bool InTrunk(ArbolCampos ac)
    {
        Vector3 d = transform.position - ac.transform.position; d.y = 0f;
        return Mathf.Abs(d.x) < ac.waterHalf + 0.5f && Mathf.Abs(d.z) < ac.waterHalf + 0.5f;
    }

    // ── LA MISIÓN ─────────────────────────────────────────────────────────────────────────────────────────

    IEnumerator Mission(Anima p)
    {
        patient = p; delivered = false; deliveredTo = null; transfusedOnBoard = 0; log.Clear();
        float t0 = Time.time;
        bool animal = p is Animal;
        if (!FitsDoor(out string why)) Say($"AVISO: no cabe por las puertas de los árboles ({why})");
        bool ok = FindFloor(HospitalService.Urgencias, out ArbolCampos ac, out TreeLevel urg, out HospitalStation urgSt);
        TreeLevel dest = urg; HospitalStation destSt = urgSt;
        if (animal && FindFloor(HospitalService.Veterinaria, out ArbolCampos acV, out TreeLevel vet, out HospitalStation vetSt) && acV == ac) { dest = vet; destSt = vetSt; }
        Say($"llamada: «{p.name}» ({(animal ? "animal" : "persona")}) a {Vector3.Distance(transform.position, p.transform.position):0} m; destino {(dest != null ? dest.name : "?")}");

        // 1. EL EQUIPO: sube por el tronco a la puerta de urgencias y embarcan dos del hospital.
        crew.Clear();
        if (ok)
        {
            yield return BoardCrew(ac, urg, animal);
            yield return TrunkOut(ac);
        }
        else Say("AVISO: no encuentro el piso de urgencias en un árbol: sale sin equipo");

        // 2. AL PACIENTE.
        phase = Phase.AlPaciente;
        Vector3 pp = p.transform.position;
        yield return Travel(pp, Length * 0.5f + 4f);
        Vector3 toP = pp - transform.position; toP.y = 0f;
        yield return TurnTo(-toP);   // la popa (la rampa) hacia el paciente

        // 3. LA CARGA: por la rampa de popa, en camilla o en el trineo de la grúa, a la bodega.
        phase = Phase.Carga;
        Carried cp = Carried.Take(p.gameObject, BayT);
        Vector3 ramp = transform.TransformPoint(new Vector3(0f, -Height * 0.5f + 0.3f, -Length * 0.5f - 1.5f));
        float l0 = Time.time;
        while (!Carried.MoveTowards(p.transform, ramp, stretcherSpeed) && Time.time - l0 < 40f) yield return null;
        while (!Carried.MoveTowards(p.transform, BayT.position, stretcherSpeed) && Time.time - l0 < 60f) yield return null;
        Say($"«{p.name}» a bordo ({(animal ? "trineo de la grúa de 8 t" : "camilla")}); oxígeno y monitor");
        Coroutine care = StartCoroutine(OnBoardCare(p));

        // 4. AL HOSPITAL: por el tronco a la puerta de su piso.
        phase = Phase.AlHospital;
        if (ok && dest != null) yield return TrunkTo(ac, dest);
        StopCoroutine(care);

        // 5. LA DESCARGA: baja el equipo, el paciente en camilla hasta su puesto y lo atienden.
        phase = Phase.Descarga;
        Vector3 doorOut = ok && dest != null ? ac.World(dest.outside) : transform.position + transform.forward * -(Length * 0.5f + 2f);
        for (int i = 0; i < crew.Count; i++)
        {
            GameObject g = crew[i];
            Carried c = g.GetComponent<Carried>();
            Vector3 at = doorOut + Vector3.Cross(Vector3.up, (doorOut - transform.position).normalized) * (i == 0 ? 1.2f : -1.2f);
            float c0 = Time.time;
            while (c != null && !Carried.MoveTowards(g.transform, at + Vector3.up * 0.05f, stretcherSpeed * 2f) && Time.time - c0 < 30f) yield return null;
            if (c != null) c.Release(at, Quaternion.LookRotation(Flat(doorOut - transform.position)));
        }
        Vector3 spot = destSt != null ? destSt.transform.TransformPoint(new Vector3(0f, 0f, 1.6f)) : doorOut + Flat(doorOut - transform.position) * 3f;
        float s0 = Time.time;
        Vector3 doorP = ok && dest != null ? ac.World(dest.door) + Vector3.up * 0.05f : doorOut;
        while (!Carried.MoveTowards(p.transform, doorP, stretcherSpeed) && Time.time - s0 < 40f) yield return null;
        while (!Carried.MoveTowards(p.transform, doorOut + Vector3.up * 0.05f, stretcherSpeed) && Time.time - s0 < 60f) yield return null;
        while (!Carried.MoveTowards(p.transform, spot + Vector3.up * 0.05f, stretcherSpeed) && Time.time - s0 < 120f) yield return null;
        cp.Release(spot, Quaternion.LookRotation(Flat(spot - doorOut)));
        delivered = true; deliveredAfter = Time.time - t0;
        SanctuaryArea area = null; foreach (SanctuaryArea a in FindObjectsByType<SanctuaryArea>(FindObjectsSortMode.None)) if (a.areaType == SanctuaryAreaType.Infirmary) area = a;
        deliveredTo = destSt != null ? HospitalStation.Label(destSt.service) : "la puerta";
        Say($"«{p.name}» entregado en {deliveredTo} a los {deliveredAfter:0} s de juego");
        // El equipo lo atiende: estabilizar (o consulta veterinaria) y, si sangra, el banco de sangre.
        Anima doc = crew.Count > 0 ? crew[0].GetComponent<Anima>() : null;
        if (destSt != null) destSt.Treat(doc);
        if (p.GetComponent<BloodLoss>() is BloodLoss bl && bl.lost >= 0.15f && BloodBank.Instance != null) BloodBank.Instance.Transfuse(p, doc != null ? doc.name : "el equipo");
        foreach (GameObject g in crew) _engaged.Remove(g);

        // 6. VUELVE A SU SITIO.
        phase = Phase.Vuelta;
        if (ok) yield return TrunkOut(ac);
        yield return Travel(parkPos);
        yield return MoveTo(parkPos, driveSpeed, true, 30f);
        yield return TurnTo(parkRot * Vector3.forward);
        missionSeconds = Time.time - t0;
        phase = Phase.Aparcado;
        Say($"aparcado de nuevo ({missionSeconds:0} s de juego en total)");
    }

    /// <summary>Atracado en la puerta de urgencias: dos del hospital de turno andan hasta la puerta y suben a sus asientos.</summary>
    IEnumerator BoardCrew(ArbolCampos ac, TreeLevel urg, bool animal)
    {
        phase = Phase.ARecogerEquipo;
        yield return TrunkTo(ac, urg);
        phase = Phase.EmbarqueEquipo;
        var want = PickCrew(animal);
        foreach (GameObject g in want) _engaged.Add(g);
        Vector3 meet = ac.World(urg.outside);
        float w0 = Time.time;
        var arrived = new HashSet<GameObject>();
        while (arrived.Count < want.Count && Time.time - w0 < 120f)
        {
            for (int i = 0; i < want.Count; i++)
            {
                GameObject g = want[i]; if (g == null || arrived.Contains(g)) continue;
                Vector3 spot = meet + Vector3.Cross(Vector3.up, Flat(meet - transform.position)) * (i == 0 ? 1f : -1f);
                PathMover.Of(g.transform).StepToward(spot, 3f);
                Vector3 f = spot - g.transform.position; f.y = 0f;
                if (f.magnitude < 1.5f && Mathf.Abs(spot.y - g.transform.position.y) < 2.5f) arrived.Add(g);
            }
            yield return null;
        }
        for (int i = 0; i < want.Count; i++)
        {
            GameObject g = want[i]; if (g == null) continue;
            if (!arrived.Contains(g)) { Say($"«{g.name}» no llega a la puerta en 120 s: sale sin él"); _engaged.Remove(g); continue; }
            Carried.Take(g, Seat(crew.Count));
            float b0 = Time.time;
            while (!Carried.MoveTowards(g.transform, ac.World(urg.door) + Vector3.up * 0.05f, stretcherSpeed * 2f) && Time.time - b0 < 20f) yield return null;
            while (!Carried.MoveTowards(g.transform, Seat(crew.Count).position, stretcherSpeed * 2f) && Time.time - b0 < 40f) yield return null;
            crew.Add(g);
        }
        Say($"embarcan {crew.Count} del hospital: {string.Join(", ", crew.ConvertAll(x => x.name))}");
    }

    /// <summary>LANZADERA DEL EQUIPO VETERINARIO al Leviatán (el submarino colosal no cabe en los árboles): recoge al equipo en
    /// la puerta de urgencias, va a su pasarela y los sube a sus asientos (<paramref name="seats"/>); luego vuelve a su sitio.</summary>
    public IEnumerator Ferry(Vector3 gangway, Transform[] seats, List<GameObject> handedOver)
    {
        if (phase != Phase.Aparcado) yield break;
        float t0 = Time.time; log.Clear(); crew.Clear();
        if (!FindFloor(HospitalService.Urgencias, out ArbolCampos ac, out TreeLevel urg, out _)) { Say("lanzadera: sin piso de urgencias"); yield break; }
        yield return BoardCrew(ac, urg, true);
        yield return TrunkOut(ac);
        phase = Phase.AlPaciente;
        yield return Travel(gangway, Length * 0.5f + 3f);
        for (int i = 0; i < crew.Count; i++)
        {
            GameObject g = crew[i]; Transform seat = seats != null && i < seats.Length ? seats[i] : null;
            if (seat == null) continue;
            g.transform.SetParent(seat, true);
            float c0 = Time.time;
            while (!Carried.MoveTowards(g.transform, gangway + Vector3.up * 0.1f, stretcherSpeed * 3f) && Time.time - c0 < 30f) yield return null;
            while (!Carried.MoveTowards(g.transform, seat.position, stretcherSpeed * 3f) && Time.time - c0 < 60f) yield return null;
            handedOver.Add(g);
        }
        Say($"lanzadera: deja a {handedOver.Count} en el Leviatán ({Time.time - t0:0} s)");
        crew.Clear();
        phase = Phase.Vuelta;
        yield return Travel(parkPos);
        yield return MoveTo(parkPos, driveSpeed, true, 30f);
        yield return TurnTo(parkRot * Vector3.forward);
        phase = Phase.Aparcado;
    }

    /// <summary>Quien deja la lanzadera ya no es de la jornada hasta que lo suelten (<see cref="Free"/>).</summary>
    public static void Free(GameObject g) { if (g != null) _engaged.Remove(g); }

    static Vector3 Flat(Vector3 v) { v.y = 0f; return v.sqrMagnitude > 1e-4f ? v.normalized : Vector3.forward; }

    /// <summary>LO QUE HACE EL EQUIPO POR EL CAMINO: oxígeno y monitor (baja el estrés, frena el empeoramiento), y si sangra,
    /// sangre O− de la nevera (personas) o fluidos (animales: no hay donante universal) cada pocos segundos.</summary>
    IEnumerator OnBoardCare(Anima p)
    {
        while (p != null)
        {
            p.stress = Mathf.Max(0f, p.stress - 0.05f);
            p.sickness = Mathf.Max(0f, p.sickness - 0.01f);
            oxygenLitres = Mathf.Max(0f, oxygenLitres - 15f);
            BloodLoss bl = p.GetComponent<BloodLoss>();
            if (bl != null && bl.lost >= 0.2f)
            {
                if (!(p is Animal) && bloodUnits > 0)
                {
                    bloodUnits--; transfusedOnBoard++; bl.lost = Mathf.Max(0f, bl.lost - BloodBank.UnitFraction(p)); bl.unitsReceived++; bl.lastProduct = "hematíes O− del submarino";
                    Say($"transfunde 1 unidad de O− a «{p.name}» por el camino (pérdida {bl.lost * 100f:0} %, quedan {bloodUnits})");
                }
                else if (p is Animal && fluidBags > 0) { fluidBags--; bl.lost = Mathf.Max(0f, bl.lost - 0.02f); }
            }
            yield return new WaitForSeconds(4f);
        }
    }
}
