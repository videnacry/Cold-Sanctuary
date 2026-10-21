using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VIRTUALIZACIÓN «SERVICIO DEL RESTAURANTE» (escena Virtual_coc_servicio): el jugador es el COCINERO. Llegan turistas,
/// cada uno con su NÚMERO de pedido y un plato de la carta. En el FOGÓN se cocina el pedido más antiguo (F) y el plato
/// sale al PASE; con la CAMPANA se llama por su número (F): ese turista se acerca, recoge su plato, se sienta a una
/// mesa a comer, termina y se va. Con <see cref="target"/> turistas servidos, la misión cuenta como hecha.
/// </summary>
public class RestaurantServiceGame : MonoBehaviour
{
    public Transform entrance, waiting, pass, exit;
    public Transform[] seats = new Transform[0];
    public Material[] shirts = new Material[0];
    public Material skin, plateMat, foodMat;
    public int target = 5;
    public float spawnEvery = 7f, cookSeconds = 3f, eatSeconds = 8f, walkSpeed = 2.6f;
    [Tooltip("Turistas como copias de cuerpos de gente del santuario (en el restaurante real) en vez de figuras simples.")]
    public bool copies;
    public float seatHeight = 0.45f, plateAhead = 0.95f, plateUp = 0.45f;

    enum State { Arriving, Waiting, Cooking, Ready, Called, ToTable, Eating, Leaving, Gone }
    class Order
    {
        public int number; public string dish; public State state; public Transform body, plate; public int seat = -1;
        public float timer;
    }

    readonly List<Order> _orders = new List<Order>();
    int _next = 1, _served, _spawned;
    float _spawnAt;
    bool _done;
    float _exitAt = -1f;
    string _toast; float _toastUntil;

    public int Served => _served;
    public Transform returnPoint;

    void OnDestroy() { foreach (Order o in _orders) { if (o.plate != null) Destroy(o.plate.gameObject); if (o.body != null) Destroy(o.body.gameObject); } }

    /// <summary>
    /// El SERVICIO en el restaurante REAL (la cocina y el comedor del santuario): fogón de un puesto, la barra de servicio
    /// como pase, las sillas del comedor y la entrada de la plataforma; los turistas son copias de gente del santuario.
    /// </summary>
    public static RestaurantServiceGame StartInRestaurant(Transform area, Transform returnPoint, WorkRoster.Mission m)
    {
        Transform pass = VirtualInPlace.FindDeep(area, "Barra de servicio");
        Transform stove = null;
        foreach (Transform t in area.GetComponentsInChildren<Transform>(true))
            if (t.name.Contains("fogón") || t.name.Contains("Fogon") || t.name.StartsWith("Puesto_1")) { stove = t; break; }
        var seats = new List<Transform>();
        Transform dining = VirtualInPlace.FindDeep(area, "Comedor_Gourmet");
        if (dining != null) foreach (Transform t in dining.GetComponentsInChildren<Transform>()) if (t.name == "Asiento" && seats.Count < 12) seats.Add(t);
        if (pass == null || stove == null || seats.Count == 0) { Debug.LogWarning("[Restaurante] No encuentro la cocina/comedor reales: se usa la escena aparte."); return null; }

        var go = new GameObject("Servicio_virtual");
        var g = go.AddComponent<RestaurantServiceGame>();
        g.copies = true; g.returnPoint = returnPoint;
        Transform mat = area.Find("Mat"); float hz = mat != null ? mat.localScale.z * 0.5f : 10f;
        g.entrance = Mark(go.transform, "Entrada", area.TransformPoint(new Vector3(1.5f, 0.15f, hz - 1.5f)));
        g.exit = Mark(go.transform, "Salida", area.TransformPoint(new Vector3(-1.5f, 0.15f, hz - 1.5f)));
        Vector3 pf = pass.position; pf.y = area.position.y + 0.15f;
        Transform passMark = Mark(go.transform, "Pase", pf); passMark.rotation = pass.rotation;
        g.pass = passMark;
        g.waiting = Mark(go.transform, "Cola", pf + pass.forward * 2.2f); g.waiting.rotation = pass.rotation;
        g.seats = seats.ToArray(); g.seatHeight = 0.45f; g.plateAhead = 0.55f; g.plateUp = 0.32f;
        Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        g.plateMat = new Material(sh) { color = Color.white }; g.foodMat = new Material(sh) { color = new Color(0.8f, 0.45f, 0.25f) };
        // El fogón y la campana del pase, en los sitios reales.
        Station(go.transform, g, stove.position, false, "Fogón del puesto (F: cocinar el pedido)");
        Station(go.transform, g, pass.position + Vector3.up * 0.2f, true, "Campana del pase (F: llamar por número)");
        VirtualMissionRunner.Pending = m;
        Debug.Log("[Restaurante] Virtualización del servicio en la cocina y el comedor REALES del santuario.");
        return g;
    }

    static Transform Mark(Transform parent, string name, Vector3 p) { var t = new GameObject(name).transform; t.SetParent(parent, true); t.position = p; return t; }

    static void Station(Transform parent, RestaurantServiceGame g, Vector3 p, bool bell, string label)
    {
        var t = new GameObject(bell ? "Campana_virtual" : "Fogon_virtual").transform; t.SetParent(parent, true); t.position = p;
        var c = t.gameObject.AddComponent<SphereCollider>(); c.isTrigger = true; c.radius = 1.2f;
        var s = t.gameObject.AddComponent<RestaurantServiceStation>(); s.game = g; s.bell = bell;
        WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = label; wl.height = 1.3f;
    }


    void Start() { _spawnAt = Time.time + 1f; }

    void Update()
    {
        if (!_done && Time.time >= _spawnAt && _spawned < target + 2 && Active() < 5) { Spawn(); _spawnAt = Time.time + spawnEvery; }
        for (int i = _orders.Count - 1; i >= 0; i--) Tick(_orders[i]);
        if (_exitAt > 0f && Time.time >= _exitAt)
        {
            _exitAt = -1f;
            if (returnPoint != null)   // en el restaurante real: se «despierta» delante de la máquina
            {
                GameObject pl = GameObject.FindGameObjectWithTag("Player");
                if (pl != null) WorkdayDirector.Teleport(pl.transform, returnPoint.position + Vector3.up * 0.1f);
                Destroy(gameObject);
            }
            else if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld) MobWorldLoader.Instance.ExitMobWorld();
        }
    }

    int Active() { int n = 0; foreach (Order o in _orders) if (o.state != State.Gone) n++; return n; }

    void Spawn()
    {
        var o = new Order { number = _next++, state = State.Arriving };
        Gastronomy.Dish[] menu = Gastronomy.Menu;
        o.dish = menu.Length > 0 ? menu[(o.number * 7) % menu.Length].name : "plato del día";
        o.body = Tourist(o.number);
        o.body.position = entrance.position;
        _orders.Add(o); _spawned++;
        Debug.Log($"[Restaurante] Llega un turista: pedido #{o.number} — {o.dish}.");
    }

    Transform Tourist(int n)
    {
        if (copies)
        {
            string[] who = { "Goluis", "Panterilia", "Gohageneis", "Irosene", "Lizmibet", "JefeConstruccion", "Sadhguru" };
            GameObject c = WorkdayDirector.SpawnBodyCopy(who[n % who.Length], $"Turista_{n}");
            c.transform.SetParent(transform, true);
            return c.transform;
        }
        var root = new GameObject($"Turista_{n}").transform;
        root.SetParent(transform, true);
        Material shirt = shirts.Length > 0 ? shirts[n % shirts.Length] : null;
        Part(root, new Vector3(0f, 0.45f, 0f), new Vector3(0.35f, 0.9f, 0.3f), shirt);
        Part(root, new Vector3(0f, 1.25f, 0f), new Vector3(0.5f, 0.7f, 0.3f), shirt);
        Part(root, new Vector3(0f, 1.8f, 0f), new Vector3(0.3f, 0.3f, 0.3f), skin);
        WorldLabel wl = root.gameObject.AddComponent<WorldLabel>(); wl.height = 2.2f;
        return root;
    }

    static Transform Part(Transform parent, Vector3 local, Vector3 size, Material m)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(g.GetComponent<Collider>());
        g.transform.SetParent(parent, false);
        g.transform.localPosition = local; g.transform.localScale = size;
        if (m != null) g.GetComponent<Renderer>().sharedMaterial = m;
        return g.transform;
    }

    bool Walk(Order o, Vector3 to)
    {
        Vector3 p = o.body.position; to.y = p.y;
        Vector3 d = to - p;
        if (d.sqrMagnitude > 0.01f) o.body.rotation = Quaternion.LookRotation(d);
        o.body.position = Vector3.MoveTowards(p, to, walkSpeed * Time.deltaTime);
        return (o.body.position - to).sqrMagnitude < 0.04f;
    }

    void Tick(Order o)
    {
        WorldLabel wl = o.body != null ? o.body.GetComponent<WorldLabel>() : null;
        switch (o.state)
        {
            case State.Arriving:
                if (Walk(o, WaitingSpot(o))) o.state = State.Waiting;
                break;
            case State.Waiting: case State.Cooking: case State.Ready:
                Walk(o, WaitingSpot(o));
                if (o.state == State.Cooking && Time.time >= o.timer) { o.state = State.Ready; o.plate = Plate(o); Debug.Log($"[Restaurante] Pedido #{o.number} ({o.dish}) listo en el pase."); }
                break;
            case State.Called:
                if (Walk(o, pass.position + pass.forward * 0.9f))
                {
                    o.plate.SetParent(o.body, false); o.plate.localPosition = new Vector3(0f, 1.05f, 0.35f);
                    o.seat = FreeSeat();
                    o.state = State.ToTable;
                }
                break;
            case State.ToTable:
                if (o.seat < 0) { o.seat = FreeSeat(); if (o.seat < 0) break; }
                if (Walk(o, seats[o.seat].position))
                {
                    o.body.rotation = seats[o.seat].rotation;
                    if (copies) { Vector3 sp = seats[o.seat].position; o.body.position = new Vector3(sp.x, sp.y - seatHeight, sp.z); ClonePose.Apply(o.body.Find(CubeHumanoid.BodyName), DailyPoses.Sit()); }
                    else o.body.position = seats[o.seat].position + Vector3.down * 0.4f;   // sentado
                    o.plate.SetParent(null, true); o.plate.position = seats[o.seat].position + seats[o.seat].forward * plateAhead + Vector3.up * plateUp;   // en la mesa, delante
                    o.timer = Time.time + eatSeconds; o.state = State.Eating;
                }
                break;
            case State.Eating:
                if (o.plate != null) { Transform f = o.plate.Find("Comida"); if (f != null) f.localScale = Vector3.Lerp(new Vector3(0.6f, 0.6f, 0.6f), new Vector3(0.1f, 0.2f, 0.1f), 1f - (o.timer - Time.time) / eatSeconds); }
                if (Time.time >= o.timer)
                {
                    if (o.plate != null) Destroy(o.plate.gameObject);
                    if (copies) ClonePose.Rest(o.body.Find(CubeHumanoid.BodyName)); else o.body.position += Vector3.up * 0.4f;
                    o.state = State.Leaving; _served++;
                    Debug.Log($"[Restaurante] El turista #{o.number} termina de comer y se va ({_served}/{target} servidos).");
                    if (_served >= target && !_done) Complete();
                }
                break;
            case State.Leaving:
                if (Walk(o, exit.position)) { Destroy(o.body.gameObject); o.state = State.Gone; }
                break;
        }
        if (wl != null) wl.text = o.state == State.Eating ? $"#{o.number} comiendo" : $"#{o.number} · {o.dish}";
    }

    Vector3 WaitingSpot(Order o)
    {
        int i = 0; foreach (Order x in _orders) { if (x == o) break; if (x.state <= State.Ready) i++; }
        return waiting.position + waiting.right * (i * 1.1f - 2.2f);
    }

    int FreeSeat()
    {
        for (int s = 0; s < seats.Length; s++)
        {
            bool taken = false;
            foreach (Order o in _orders) if (o.seat == s && (o.state == State.ToTable || o.state == State.Eating)) { taken = true; break; }
            if (!taken) return s;
        }
        return -1;
    }

    Transform Plate(Order o)
    {
        var p = new GameObject($"Plato_{o.number}").transform;
        p.SetParent(pass, false);
        int ready = 0; foreach (Order x in _orders) if (x.state == State.Ready && x.plate != null) ready++;
        p.localPosition = new Vector3(-1.2f + ready * 0.6f, 1.03f, 0f);   // sobre el pase
        Transform d = Part(p, Vector3.zero, new Vector3(0.4f, 0.03f, 0.4f), plateMat); d.name = "Plato";
        Transform f = Part(p, new Vector3(0f, 0.06f, 0f), new Vector3(0.22f, 0.08f, 0.22f), foodMat); f.name = "Comida";
        WorldLabel wl = p.gameObject.AddComponent<WorldLabel>(); wl.text = $"#{o.number}"; wl.height = 0.3f;
        return p;
    }

    // ── Lo que hace el jugador ──
    public string CookLabel { get { Order o = Oldest(State.Waiting); return o != null ? $"Cocinar el pedido #{o.number} ({o.dish})" : "No hay pedidos por cocinar"; } }
    public bool CanCook => !_done && Oldest(State.Waiting) != null;
    public void Cook()
    {
        Order o = Oldest(State.Waiting); if (o == null) return;
        o.state = State.Cooking; o.timer = Time.time + cookSeconds;
        Toast($"Cocinando el pedido #{o.number}: {o.dish}…");
        Debug.Log($"[Restaurante] El cocinero prepara el pedido #{o.number} ({o.dish}).");
    }

    public string CallLabel { get { Order o = Oldest(State.Ready); return o != null ? $"Llamar al número {o.number}" : "No hay platos en el pase"; } }
    public bool CanCall => !_done && Oldest(State.Ready) != null;
    public void Call()
    {
        Order o = Oldest(State.Ready); if (o == null) return;
        o.state = State.Called;
        Toast($"«¡Número {o.number}!»");
        Debug.Log($"[Restaurante] El cocinero llama al número {o.number}: el turista viene a por su {o.dish}.");
    }

    Order Oldest(State s) { foreach (Order o in _orders) if (o.state == s) return o; return null; }

    void Complete()
    {
        _done = true;
        WorkRoster.Mission m = VirtualMissionRunner.Pending;
        if (m != null && m.id == "coc_servicio" && WorkdayDirector.Instance != null) WorkdayDirector.Instance.PlayerVirtualizes(m);
        VirtualMissionRunner.Pending = null;
        Toast("¡Servicio completado!");
        Debug.Log($"[Restaurante] Servicio completado: {_served} turistas comieron.");
        _exitAt = Time.time + 4f;
    }

    void Toast(string s) { _toast = s; _toastUntil = Time.time + 2.5f; }

    GUIStyle _st;
    void OnGUI()
    {
        if (Event.current.type != EventType.Repaint) return;
        if (_st == null) _st = new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true };
        var lines = new List<string> { $"<b>Virtualización · Servicio del restaurante</b>  ({_served}/{target} servidos)" };
        foreach (Order o in _orders)
        {
            if (o.state == State.Gone || o.state == State.Leaving) continue;
            string st = o.state switch { State.Arriving => "llegando", State.Waiting => "esperando · cocínalo en el fogón", State.Cooking => "en el fogón…", State.Ready => "<color=#FFD27A>listo en el pase · llámalo con la campana</color>", State.Called => "viene a por su plato", State.ToTable => "va a su mesa", _ => "comiendo" };
            lines.Add($"#{o.number} {o.dish} — {st}");
        }
        if (_done) lines.Add("<color=#8FE08F>¡Servicio completado! Volviendo…</color>");
        GUI.Box(new Rect(12, 80, 470, 12 + lines.Count * 20), "");
        for (int i = 0; i < lines.Count; i++) GUI.Label(new Rect(22, 86 + i * 20, 460, 20), lines[i], _st);
        if (Time.time < _toastUntil) GUI.Box(new Rect(Screen.width * 0.5f - 200, 60, 400, 28), _toast);
    }
}
