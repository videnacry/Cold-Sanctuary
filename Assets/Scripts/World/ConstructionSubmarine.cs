using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL SUBMARINO DE OBRA (Berón 2026-11-03; docs/submarino-de-obra.md): un vehículo ANFIBIO —anda por tierra sobre orugas y nada
/// con hélices— que lleva la maquinaria pesada y el material de obra por la VÍA MARINA de los edificios-árbol (sin suelo: se baja
/// por el tronco al mar y se sube por el de otro árbol). Vive en el GARAJE (un piso de la torre de Producción); cuando una obra
/// (<see cref="ConstructionProject"/>) no tiene el material del día, carga el kit en el almacén y la maquinaria, y lo lleva:
///   garaje → (orugas) a la puerta del piso → al agua del tronco → baja por el tronco y sale por el fondo → (mar, bajo el hielo) →
///   sube por el tronco del árbol de la obra → sale por la puerta del piso → (orugas) a su sitio en el piso → descarga
///   (<see cref="ConstructionProject.DeliverMaterial"/>) → vuelve al garaje.
/// ESQUIVA A LOS ANIMALES: los grupos de peces, kril y medusas y la fauna marina; frena y se aparta (de lado y en altura) para no
/// tocarlos, y apunta lo más cerca que pasó (<see cref="MinClearance"/>).
/// Medidas (<see cref="Length"/> × <see cref="Beam"/> × <see cref="Height"/>): las de <see cref="TreeSpec"/> (el vehículo de diseño
/// de las puertas): 14 × 5 × 5 m con su carga, bodega de 10 × 3,4 × 3,4 m (una excavadora de 20 t o un contenedor de 20 pies).
/// </summary>
public class ConstructionSubmarine : MonoBehaviour
{
    public static readonly List<ConstructionSubmarine> All = new List<ConstructionSubmarine>();
    public static ConstructionSubmarine Instance => All.Count > 0 ? All[0] : null;

    public float Length => TreeSpec.VehicleLength;
    public float Beam => TreeSpec.VehicleBeam;
    public float Height => TreeSpec.VehicleHeight;
    [Tooltip("Velocidad nadando (m/s) y por tierra (m/s).")]
    public float swimSpeed = 4f, driveSpeed = 2.5f;
    [Tooltip("Profundidad (y) a la que cruza el mar bajo el hielo.")]
    public float seaDepth = -12f;
    public const string Who = "Submarino de obra";

    [Tooltip("Dónde aparca en el garaje (lo pone el constructor; si no, donde empieza).")]
    public Transform home;

    public enum State { Parked, Loading, Driving, Swimming, Unloading, Returning }
    public State Now { get; private set; } = State.Parked;
    public string Doing { get; private set; } = "Aparcado en el garaje";
    public ConstructionProject Target { get; private set; }
    public int Deliveries { get; private set; }
    /// <summary>Lo más cerca (m, de casco a cuerpo o a grupo) que pasó de un animal en todos sus trayectos.</summary>
    public float MinClearance { get; private set; } = float.PositiveInfinity;
    public int Avoidances { get; private set; }
    public float DrivenMeters { get; private set; }
    public float SwamMeters { get; private set; }
    public readonly List<string> doorsUsed = new List<string>();

    readonly List<(System.Func<Vector3> at, bool water, string label)> _route = new List<(System.Func<Vector3>, bool, string)>();
    int _wp; float _wait; bool _loaded; float _legSince;
    Transform _cargo;

    /// <summary>El centro del casco (el pivote va a ras de las orugas).</summary>
    public Vector3 HullCenter => transform.position + transform.up * Height * 0.5f;
    /// <summary>Distancia (m) de <paramref name="p"/> a la superficie del casco (un cilindro de proa a popa).</summary>
    public float HullDistance(Vector3 p)
    {
        Vector3 c = HullCenter, f = transform.forward, a = c - f * Length * 0.5f, ab = f * Length;
        float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude);
        return Vector3.Distance(p, a + ab * t) - Mathf.Max(Beam, Height) * 0.5f;
    }
    /// <summary>El submarino en marcha con el casco a menos de <paramref name="range"/> m de <paramref name="p"/> (null si ninguno).</summary>
    public static ConstructionSubmarine Nearest(Vector3 p, float range)
    {
        ConstructionSubmarine best = null; float bd = range;
        foreach (ConstructionSubmarine s in All)
        {
            if (s == null || (s.Now != State.Driving && s.Now != State.Returning)) continue;
            float d = s.HullDistance(p);
            if (d < bd) { bd = d; best = s; }
        }
        return best;
    }

    void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    void OnDisable() => All.Remove(this);

    void Start()
    {
        if (transform.Find("Casco") == null) BuildModel();
        if (home == null) { home = new GameObject("Aparcamiento_submarino").transform; home.SetParent(transform.parent, false); home.SetPositionAndRotation(transform.position, transform.rotation); }
    }

    // ── El modelo: casco de cubos con proa redondeada, cabina de cristal, bodega con su carga, orugas, hélices y faros ──
    void BuildModel()
    {
        Color hull = new Color(0.95f, 0.72f, 0.12f), dark = new Color(0.18f, 0.19f, 0.21f), glass = new Color(0.6f, 0.85f, 1f, 0.45f), steel = new Color(0.55f, 0.58f, 0.62f);
        float L = Length, B = Beam, H = Height;
        Transform C(string n, Vector3 p, Vector3 s, Color c, float yaw = 0f) => KitchenProps.C(transform, p, s, c, yaw, n);
        C("Casco", new Vector3(0f, H * 0.55f, 0f), new Vector3(B * 0.82f, H * 0.62f, L * 0.78f), hull);
        C("Proa", new Vector3(0f, H * 0.55f, L * 0.43f), new Vector3(B * 0.62f, H * 0.5f, L * 0.1f), hull);
        C("Popa", new Vector3(0f, H * 0.55f, -L * 0.43f), new Vector3(B * 0.6f, H * 0.45f, L * 0.1f), hull);
        C("Cabina", new Vector3(0f, H * 0.93f, L * 0.28f), new Vector3(B * 0.5f, H * 0.18f, L * 0.18f), glass);
        C("Bodega_puerta", new Vector3(0f, H * 0.87f, -L * 0.05f), new Vector3(B * 0.7f, H * 0.04f, L * 0.5f), dark);
        for (int s = -1; s <= 1; s += 2)
        {
            C("Oruga", new Vector3(s * B * 0.43f, H * 0.12f, 0f), new Vector3(B * 0.16f, H * 0.24f, L * 0.8f), dark);
            C("Helice", new Vector3(s * B * 0.25f, H * 0.5f, -L * 0.5f), new Vector3(B * 0.24f, B * 0.24f, 0.15f), steel, 45f);
            C("Faro", new Vector3(s * B * 0.22f, H * 0.62f, L * 0.49f), new Vector3(0.35f, 0.35f, 0.1f), new Color(1f, 0.95f, 0.7f));
            C("Brazo_grua", new Vector3(s * B * 0.3f, H * 1.0f, -L * 0.32f), new Vector3(0.25f, 0.25f, L * 0.3f), steel);
        }
        C("Rotulo_obra", new Vector3(0f, H * 0.62f, -L * 0.01f + 0.01f), new Vector3(B * 0.83f, 0.6f, 2.5f), new Color(0.1f, 0.1f, 0.12f));
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        WorldLabel wl = gameObject.GetComponent<WorldLabel>() ?? gameObject.AddComponent<WorldLabel>(); wl.text = Who; wl.height = H + 1.5f;
    }

    // La carga: una excavadora de 20 t (CAT 320: 9,5 × 3 × 3 m para el transporte) y los palés del día.
    void ShowCargo(bool on)
    {
        if (!on) { if (_cargo != null) Destroy(_cargo.gameObject); _cargo = null; return; }
        if (_cargo != null) return;
        _cargo = new GameObject("Carga").transform; _cargo.SetParent(transform, false);
        Color yel = new Color(0.98f, 0.8f, 0.1f), blk = new Color(0.15f, 0.15f, 0.15f), wood = new Color(0.72f, 0.56f, 0.36f);
        KitchenProps.C(_cargo, new Vector3(0f, Height * 0.95f, -0.8f), new Vector3(2.6f, 0.9f, 4.2f), yel, 0f, "Excavadora_chasis");
        KitchenProps.C(_cargo, new Vector3(0f, Height * 0.95f + 0.9f, -1.6f), new Vector3(1.6f, 1.2f, 1.8f), yel, 0f, "Excavadora_cabina");
        KitchenProps.C(_cargo, new Vector3(0f, Height * 0.95f + 0.7f, 2.2f), new Vector3(0.4f, 0.4f, 3.6f), yel, 0f, "Excavadora_brazo");
        KitchenProps.C(_cargo, new Vector3(0f, Height * 0.95f + 0.2f, 4.1f), new Vector3(1f, 0.8f, 0.8f), blk, 0f, "Excavadora_cazo");
        for (int i = 0; i < 3; i++) KitchenProps.C(_cargo, new Vector3(-1.2f + i * 1.2f, Height * 0.95f, -4.2f), new Vector3(1.1f, 1f, 1.1f), wood, 0f, "Pale_material");
    }

    void Update()
    {
        if (Now == State.Parked || Now == State.Loading || Now == State.Unloading)
        {
            if (Now == State.Parked && Time.time > _nextLook) { _nextLook = Time.time + 2f; ConstructionProject p = NextJob(); if (p != null) Send(p); }
            if ((Now == State.Loading || Now == State.Unloading) && Time.time >= _wait) Advance();
            return;
        }
        Step();
    }
    float _nextLook;

    ConstructionProject NextJob()
    {
        foreach (ConstructionProject p in ConstructionProject.All)
            if (p != null && !p.Finished && !p.materialOnSite && ArbolCampos.At(p.transform.position + Vector3.up * 0.5f, out _, out _, false) != null) return p;
        return null;
    }

    /// <summary>Lleva el material y la maquinaria a la obra <paramref name="p"/> (si está aparcado). false si no hay camino.</summary>
    public bool Send(ConstructionProject p)
    {
        if (p == null || (Now != State.Parked)) return false;
        ArbolCampos from = ArbolCampos.At(transform.position + Vector3.up * 0.5f, out TreeLevel la, out _, false);
        ArbolCampos to = ArbolCampos.At(p.transform.position + Vector3.up * 0.5f, out TreeLevel lb, out _, false);
        if (from == null || to == null || la == null || lb == null) { Debug.LogWarning($"[Submarino] no encuentro el camino del garaje a «{p.title}»."); return false; }
        Target = p;
        _route.Clear(); doorsUsed.Clear();
        Vector3 park = ParkPoint(to, lb);
        AddTrip(from, la, to, lb, () => park);
        _wp = 0; _legSince = Time.time;
        Now = State.Loading; _wait = Time.time + 2f;
        Doing = $"Carga la maquinaria y el material del día para «{p.title}» en el garaje";
        Debug.Log($"[Submarino] Va a llevar maquinaria y material a «{p.title}» ({to.building}, {lb.name}): {_route.Count} tramos.");
        return true;
    }

    /// <summary>Dónde aparca en un piso: frente a su puerta, metido en el piso (a 9 m del umbral), sobre la losa.</summary>
    static Vector3 ParkPoint(ArbolCampos t, TreeLevel lv)
    {
        Vector3 o = t.World(lv.outside), d = t.transform.TransformDirection(TreeSpec.Dir(lv.doorAngle));
        return o + d * 7f;
    }

    // El trayecto: por tierra a la puerta → agua del tronco → (si es otro árbol: baja, sale por el fondo, mar, sube por el otro) →
    // la puerta de llegada → por tierra a su sitio.
    void AddTrip(ArbolCampos a, TreeLevel la, ArbolCampos b, TreeLevel lb, System.Func<Vector3> end)
    {
        float lift = 0.05f;
        _route.Add((() => a.World(la.outside) + Vector3.up * lift, false, $"a la puerta de {la.name}"));
        _route.Add((() => a.World(la.door) + Vector3.up * lift, false, $"puerta de {la.name}"));
        _route.Add((() => a.World(la.inside + Vector3.up * (Height * 0.5f + 0.5f)) - Vector3.up * Height * 0.5f, true, $"dentro del tronco de {a.building}"));
        if (a != b)
        {
            _route.Add((() => a.transform.position + Vector3.up * 3f, true, $"baja por el tronco de {a.building}"));
            _route.Add((() => a.transform.position + Vector3.up * seaDepth, true, $"sale por el fondo de {a.building} al mar"));
            _route.Add((() => b.transform.position + Vector3.up * seaDepth, true, $"cruza el mar bajo el hielo hasta {b.building}"));
            _route.Add((() => b.transform.position + Vector3.up * 3f, true, $"entra por el fondo de {b.building}"));
        }
        _route.Add((() => b.World(new Vector3(0f, lb.inside.y, 0f) + (lb.inside - new Vector3(0f, lb.inside.y, 0f)) * 0.3f), true, $"sube por el tronco de {b.building}"));
        _route.Add((() => b.World(lb.inside + Vector3.up * 0.5f), true, $"frente a la puerta de {lb.name}"));
        _route.Add((() => b.World(lb.door) + Vector3.up * lift, false, $"puerta de {lb.name}"));
        _route.Add((() => b.World(lb.outside) + Vector3.up * lift, false, $"sale a {lb.name}"));
        _route.Add((end, false, "a su sitio en el piso"));
    }

    void Advance()
    {
        if (Now == State.Loading) { _loaded = true; ShowCargo(true); Now = State.Driving; _legSince = Time.time; return; }
        if (Now == State.Unloading)
        {
            // Descarga: el material del día a la obra y la maquinaria al piso. Vuelve al garaje.
            if (Target != null) { Target.DeliverMaterial(Who); if (Target.materialOnSite) Deliveries++; }
            Debug.Log($"[Submarino] Descarga la excavadora y los palés en «{Target?.title}» (material en obra: {Target?.materialOnSite}); vuelve al garaje.");
            _loaded = false; ShowCargo(false);
            ArbolCampos from = ArbolCampos.At(transform.position + Vector3.up, out TreeLevel la, out _, false);
            ArbolCampos to = ArbolCampos.At(home.position + Vector3.up, out TreeLevel lb, out _, false);
            _route.Clear(); _wp = 0;
            if (from != null && la != null && to != null && lb != null) AddTrip(from, la, to, lb, () => home.position);
            Now = _route.Count > 0 ? State.Returning : State.Parked; _legSince = Time.time;
        }
    }

    void Step()
    {
        if (_wp >= _route.Count)
        {
            if (Now == State.Returning) { Now = State.Parked; Doing = "Aparcado en el garaje"; transform.rotation = home.rotation; Target = null; Debug.Log("[Submarino] Aparcado en el garaje."); return; }
            Now = State.Unloading; _wait = Time.time + 3f; Doing = $"Descarga en «{Target?.title}»"; return;
        }
        var (atF, water, label) = _route[_wp];
        Vector3 goal = atF();
        bool wet = water || WaterZone.Underwater(transform.position + Vector3.up * Height * 0.5f);
        Doing = $"{(Now == State.Returning ? "Vuelve al garaje" : "Lleva maquinaria y material")}: {label}";
        Vector3 pos = transform.position;
        Vector3 d = goal - pos;
        float speed = wet ? swimSpeed : driveSpeed;
        // ESQUIVAR A LOS ANIMALES (solo en el agua: en tierra van por su NavMesh y no hay grupos).
        Vector3 avoid = Vector3.zero; float slow = 1f;
        if (wet) Avoid(pos, d, ref avoid, ref slow);
        Vector3 dir = d.magnitude > 1e-3f ? d.normalized : Vector3.zero;
        Vector3 move = (dir + avoid).normalized * speed * slow * Time.deltaTime;
        if (move.magnitude > d.magnitude && avoid == Vector3.zero) move = d;
        transform.position = pos + move;
        if (wet) SwamMeters += move.magnitude; else DrivenMeters += move.magnitude;
        // Orientación: por tierra, plano y hacia donde va; nadando, la proa hacia donde va (de morro al subir o bajar).
        Vector3 face = move.sqrMagnitude > 1e-6f ? move : transform.forward;
        if (!wet) face.y = 0f;
        if (face.sqrMagnitude > 1e-6f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(face.normalized), 1f - Mathf.Exp(-3f * Time.deltaTime));
        bool exact = !wet || label.StartsWith("puerta") || label.StartsWith("frente") || label.StartsWith("dentro") || label.StartsWith("sale");   // (en el agua abierta basta pasar cerca: si un grupo está en el punto, lo rodea)
        if ((goal - transform.position).magnitude < (exact ? 0.8f : 3f))
        {
            if (label.StartsWith("puerta")) doorsUsed.Add(label);
            _wp++; _legSince = Time.time;
        }
        else if (Time.time - _legSince > 240f)
        {
            Debug.LogWarning($"[Submarino] Atascado yendo {label}: sigue al siguiente tramo.");
            _wp++; _legSince = Time.time;
        }
    }

    // Grupos (peces, kril, medusas) y animales marinos cerca: se aparta hacia el lado libre y frena. Apunta la holgura mínima.
    void Avoid(Vector3 pos, Vector3 d, ref Vector3 avoid, ref float slow)
    {
        Vector3 c = pos + Vector3.up * Height * 0.5f;
        float hullR = Mathf.Max(Beam, Height) * 0.5f, look = Length * 0.5f + 8f;
        foreach (Swarm s in Swarm.All)
        {
            if (s == null || s.kind == SwarmKind.Bee || s.Visible == 0) continue;
            Consider(c, d, s.transform.position, s.schoolSpread + 0.5f, hullR, look, ref avoid, ref slow);
        }
        foreach (GameObject g in Animal.wholePopulation)
        {
            if (g == null || g.GetComponent<MarineLife>() == null) continue;
            Consider(c, d, g.transform.position, 1.5f, hullR, look, ref avoid, ref slow);
        }
    }

    void Consider(Vector3 c, Vector3 d, Vector3 other, float otherR, float hullR, float look, ref Vector3 avoid, ref float slow)
    {
        // Distancia del animal al EJE del casco (un segmento de proa a popa).
        Vector3 fwd = transform.forward, a = c - fwd * Length * 0.5f, b = c + fwd * Length * 0.5f, ab = b - a;
        float tt = Mathf.Clamp01(Vector3.Dot(other - a, ab) / ab.sqrMagnitude);
        float gap = Vector3.Distance(other, a + ab * tt) - hullR - otherR;
        MinClearance = Mathf.Min(MinClearance, gap);
        Vector3 to = other - c; float dist = to.magnitude;
        if (dist > look + otherR) return;
        float ahead = d.sqrMagnitude > 1e-4f ? Vector3.Dot(to, d.normalized) : 0f;
        if (ahead < -Length * 0.5f || gap > 6f) return;
        // Apartarse en perpendicular a la marcha, hacia el lado contrario al animal (y algo en altura).
        Vector3 lat = d.sqrMagnitude > 1e-4f ? Vector3.ProjectOnPlane(-to, d.normalized) : -to;
        if (lat.sqrMagnitude < 1e-4f) lat = Vector3.Cross(d.normalized, Vector3.up);
        float k = Mathf.Clamp01((6f - gap) / 6f);
        avoid += lat.normalized * (1.5f + 2f * k) * k;
        slow = Mathf.Min(slow, Mathf.Lerp(1f, 0.25f, k));
        if (k > 0.3f && Time.time > _avoidLog) { _avoidLog = Time.time + 3f; Avoidances++; Debug.Log($"[Submarino] Esquiva un grupo de animales (holgura {gap:0.0} m)."); }
    }
    float _avoidLog;
}
