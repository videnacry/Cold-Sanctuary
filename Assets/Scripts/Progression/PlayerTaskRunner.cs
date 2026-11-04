using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL QUE HACE LAS TAREAS DEL JUGADOR CON «E» (docs/tareas-del-jugador.md; el catálogo de pasos y sitios es
/// <see cref="PlayerTasks"/>). Generaliza el modelo de Ambrosio (<see cref="AmbrosioStory.Act"/>) a toda la jornada y a las
/// iniciaciones de área:
///   · MARCADORES en el mundo: un poste de luz con su rótulo «E — acción» en el sitio de cada paso (el actual, grande y
///     ámbar; los siguientes, pequeños y azules); los hechos desaparecen.
///   · AVISO «E — acción» en pantalla cuando el jugador está en el sitio (≤ <see cref="Reach"/> m), y la PISTA DE RUTA
///     (distancia, hacia dónde y si hay que subir o bajar) cuando está lejos.
///   · Pulsar E en el sitio HACE EL PASO: el cuerpo hace el gesto de la clase de la función <see cref="ActionSeconds"/> s
///     (quieto, como al empujar a Shanidar), se le da el OBJETO del paso (en la mano y en <see cref="Inventory"/>), se
///     aplica el efecto del paso y se pasa al siguiente sitio.
///   · PROGRESO en el panel (✓ hechos, ▶ el actual, · los que faltan) y CIERRE: al acabar el último paso se aplica el
///     efecto de la misión en el santuario (<see cref="AreaFunctions.Run"/>), se marca en el progreso y se avisa a quien la
///     pidió (<see cref="PlayerWorkday"/>: la misión cuenta como hecha).
///   · PASOS EN VIVO (<see cref="Live"/>): una iniciación de área (cocina, huerto, yoga) pide cada fotograma el paso que
///     espera del jugador; la E en su sitio lo lanza (lo mismo que escribir `step` o el asana).
/// Las esperas tienen tope: el gesto dura <see cref="ActionSeconds"/> s y un paso en vivo caduca si nadie lo renueva.
/// </summary>
public class PlayerTaskRunner : MonoBehaviour
{
    public const string MarkerPrefix = "TareaMarcador";
    /// <summary>Distancia (en planta) a la que la E hace el paso.</summary>
    public const float Reach = 3f;
    /// <summary>Diferencia de altura máxima con el sitio (otro piso no vale).</summary>
    public const float ReachUp = 2.5f;
    /// <summary>Segundos que dura el gesto de cada paso.</summary>
    public const float ActionSeconds = 2.5f;
    const string ItemSlot = "TareaObjeto";

    public static PlayerTaskRunner Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() { Ensure(); }

    public static PlayerTaskRunner Ensure()
    {
        if (Instance != null) return Instance;
        var go = new GameObject("PlayerTaskRunner_AUTO");
        DontDestroyOnLoad(go);
        return go.AddComponent<PlayerTaskRunner>();
    }

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }

    /// <summary>Una tarea en marcha: sus pasos, sus sitios y marcadores, y lo que lleva hecho.</summary>
    public class Task
    {
        public PlayerTasks.Plan plan;
        public PlayerTasks.Spot[] spots;
        public readonly List<GameObject> markers = new List<GameObject>();
        public readonly List<string> given = new List<string>();
        public int index;
        public bool acting, completed, lapsed, effectRan;
        public float actUntil;
        public object owner;
        public System.Action<Task> onDone;
        public int Count => plan.steps.Count;
        public bool Finished => index >= Count;
        public PlayerTasks.Step Current => Finished ? null : plan.steps[index];
        public PlayerTasks.Spot CurrentSpot => Finished ? default : spots[index];
        public GameObject CurrentMarker => Finished || index >= markers.Count ? null : markers[index];
    }

    /// <summary>La tarea de la jornada en marcha (null si ninguna).</summary>
    public Task Active { get; private set; }
    /// <summary>Lo que el jugador ha recibido en sus tareas (objeto → cuántas veces).</summary>
    public readonly Dictionary<string, int> Inventory = new Dictionary<string, int>();
    /// <summary>Las misiones que terminó con sus pasos (ids, en orden).</summary>
    public readonly List<string> Completed = new List<string>();
    /// <summary>La última respuesta de la E (para el log y las pruebas).</summary>
    public string LastResult { get; private set; }
    public int LivePresses { get; private set; }

    GameObject _player;
    bool _holding, _frameB;
    float _nextFrame;
    AreaFunctions.Kind _actKind;
    float _liveActUntil;

    GameObject Player()
    {
        if (_player == null) _player = GameObject.FindGameObjectWithTag("Player");
        return _player;
    }

    // ── EMPEZAR, ABANDONAR ─────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>Empieza la tarea <paramref name="plan"/> para <paramref name="owner"/> (la jornada): pone los marcadores y
    /// la pista hacia el primer sitio. Si había otra, se deja (caduca).</summary>
    public Task Begin(PlayerTasks.Plan plan, object owner, System.Action<Task> onDone)
    {
        if (plan == null || plan.steps.Count == 0) return null;
        if (Active != null) Lapse(Active.owner, $"empieza «{plan.title}»");
        var t = new Task { plan = plan, owner = owner, onDone = onDone };
        GameObject p = Player();
        t.spots = Spots(plan, p != null ? p.transform : null);
        t.markers.AddRange(BuildMarkers(plan, t.spots));
        Restyle(t);
        Active = t;
        PlayerTasks.Spot s0 = t.spots[0];
        Debug.Log($"[Tareas] Kushal empieza «{plan.title}» ({plan.area}): {plan.steps.Count} pasos con E. Primero: «{plan.steps[0].action}» en {s0.label}{(s0.found ? "" : " (punto del área)")}.");
        LessonPrompt.Show($"Tu tarea: «{plan.title}» en {plan.area}. Sigue los marcadores y pulsa E en cada uno ({plan.steps.Count} pasos). Primero: {plan.steps[0].action}.", null, 10f);
        return t;
    }

    /// <summary>La tarea de <paramref name="owner"/> caduca (se acabó su hora sin hacerla, o la sustituye otra): fuera los
    /// marcadores; no se aplica su efecto.</summary>
    public void Lapse(object owner, string why)
    {
        Task t = Active;
        if (t == null || (owner != null && t.owner != owner)) return;
        StopActing(t, false);
        t.lapsed = true;
        Clear(t);
        Active = null;
        Debug.Log($"[Tareas] «{t.plan.title}» caduca ({why}): {t.index}/{t.Count} pasos hechos.");
    }

    /// <summary>¿Es <paramref name="owner"/> el dueño de la tarea en marcha?</summary>
    public bool Owns(object owner) => Active != null && Active.owner == owner;

    /// <summary>Los sitios de cada paso (el n-ésimo mueble del puesto para el paso n: los pasos se reparten por el área).</summary>
    public static PlayerTasks.Spot[] Spots(PlayerTasks.Plan plan, Transform near)
    {
        var s = new PlayerTasks.Spot[plan.steps.Count];
        var seen = new Dictionary<string, int>();
        for (int i = 0; i < s.Length; i++)
        {
            PlayerTasks.Step st = plan.steps[i];
            string area = st.area ?? plan.area;
            string key = area + "|" + st.station;
            seen.TryGetValue(key, out int n); seen[key] = n + 1;
            s[i] = PlayerTasks.Resolve(area, st.station, n, plan.site, near);
        }
        return s;
    }

    /// <summary>Un marcador por paso, con su rótulo «E — acción» (público para la prueba: todas las misiones tienen los suyos).</summary>
    public static List<GameObject> BuildMarkers(PlayerTasks.Plan plan, PlayerTasks.Spot[] spots)
    {
        var l = new List<GameObject>();
        for (int i = 0; i < plan.steps.Count; i++) l.Add(Marker($"{MarkerPrefix}_{plan.id}_{i + 1}", spots[i].pos, plan.steps[i].action));
        return l;
    }

    static GameObject Marker(string name, Vector3 at, string action)
    {
        var root = new GameObject(name);
        root.transform.position = at;
        Piece(root.transform, PrimitiveType.Cylinder, "Poste", new Vector3(0f, 1.6f, 0f), new Vector3(0.18f, 1.6f, 0.18f), Quaternion.identity);
        Piece(root.transform, PrimitiveType.Cylinder, "Aro", new Vector3(0f, 0.03f, 0f), new Vector3(1.6f, 0.02f, 1.6f), Quaternion.identity);
        Piece(root.transform, PrimitiveType.Cube, "Rombo", new Vector3(0f, 3.6f, 0f), new Vector3(0.45f, 0.45f, 0.45f), Quaternion.Euler(45f, 0f, 45f));
        WorldLabel wl = root.AddComponent<WorldLabel>(); wl.text = $"E — {action}"; wl.height = 4.1f;
        return root;
    }

    static void Piece(Transform parent, PrimitiveType type, string name, Vector3 pos, Vector3 scale, Quaternion rot)
    {
        GameObject g = GameObject.CreatePrimitive(type);
        Collider c = g.GetComponent<Collider>(); if (c != null) Destroy(c);
        g.name = name; g.transform.SetParent(parent, false); g.transform.localPosition = pos; g.transform.localScale = scale; g.transform.localRotation = rot;
    }

    // El actual, grande y ámbar; los siguientes, pequeños y azules.
    static void Restyle(Task t)
    {
        for (int i = 0; i < t.markers.Count; i++)
        {
            GameObject m = t.markers[i]; if (m == null) continue;
            bool cur = i == t.index;
            m.transform.localScale = Vector3.one * (cur ? 1f : 0.55f);
            Material mat = KitchenProps.Mat(cur ? new Color(1f, 0.78f, 0.2f) : new Color(0.45f, 0.6f, 0.9f));
            foreach (Renderer r in m.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
            WorldLabel wl = m.GetComponent<WorldLabel>(); if (wl != null) wl.text = cur ? $"E — {t.plan.steps[i].action}" : $"E — {t.plan.steps[i].action} ({i + 1}/{t.Count})";
        }
    }

    static void Clear(Task t) { foreach (GameObject m in t.markers) if (m != null) Destroy(m); t.markers.Clear(); }

    // ── LA TECLA E ─────────────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>¿Está el jugador en el sitio del paso actual?</summary>
    public bool NearCurrent => Active != null && !Active.Finished && Near(Player(), Active.CurrentSpot.pos);

    static bool Near(GameObject p, Vector3 spot)
    {
        if (p == null) return false;
        Vector3 d = p.transform.position - spot;
        if (Mathf.Abs(d.y) > ReachUp + 1.2f) return false;   // (la posición del jugador va ~1 m sobre sus pies)
        d.y = 0f;
        return d.magnitude <= Reach;
    }

    /// <summary>LA E: lo que toca según dónde está el jugador (un paso en vivo de una iniciación, o el de su tarea).</summary>
    public string Press() => LastResult = DoPress(false);

    /// <summary>El piloto automático de las pruebas (PlayerWorkday.autopilot), ya en su puesto: hace el paso sin mirar la
    /// distancia al sitio. Es el propio jugador (simulado), no la IA de los demás.</summary>
    public string PressAnywhere() => LastResult = DoPress(true);

    string DoPress(bool anywhere)
    {
        GameObject p = Player(); if (p == null) return "sin jugador";
        LiveStep lv = LiveNow();
        if (lv != null && (anywhere || Near(p, lv.spot)))
        {
            if (!lv.press()) return $"«{lv.action}»: ahora no";
            LivePresses++;
            if (lv.gesture.HasValue) { _actKind = lv.gesture.Value; _liveActUntil = Time.time + 1.5f; }
            Debug.Log($"[Tareas] (E) {lv.title}: {lv.action} (paso {lv.idx + 1}/{lv.total}).");
            _live = null;   // el dueño pondrá el siguiente
            return $"haces «{lv.action}»";
        }
        Task t = Active;
        if (t == null || t.Finished) return lv != null ? $"acércate al sitio: {lv.action}" : "nada que hacer aquí";
        if (t.acting) return $"ya estás con «{t.Current.action}»";
        if (!anywhere && !Near(p, t.CurrentSpot.pos)) return $"acércate al marcador: {t.Current.action} ({t.CurrentSpot.label})";
        StartStep(t, p);
        return $"haces «{t.Current.action}»";
    }

    void StartStep(Task t, GameObject p)
    {
        PlayerTasks.Step s = t.Current;
        t.acting = true; t.actUntil = Time.time + ActionSeconds; _actKind = s.gesture;
        PlayerController pc = p.GetComponent<PlayerController>();
        if (pc != null && !_holding) { pc.HoldBody(true); _holding = true; }   // quieto mientras lo hace (como al empujar a Shanidar)
        Give(p, s.item);
        Debug.Log($"[Tareas] (E) «{t.plan.title}» paso {t.index + 1}/{t.Count}: {s.action}{(string.IsNullOrEmpty(s.item) ? "" : $" (te dan: {s.item})")}.");
    }

    // El objeto del paso, en la mano derecha (las herramientas de cubos conocidas, con su forma; el resto, un bloque de color).
    static readonly HashSet<string> KnownTools = new HashSet<string> { "Tablet", "Cepillo", "Termometro", "Pulsioximetro", "Pera", "Manguito", "Estetoscopio", "Hisopo", "Electrodo", "Electrodos", "Jeringa", "Credencial", "Pala", "Azada", "Rastrillo" };
    static void Give(GameObject p, string item)
    {
        if (string.IsNullOrEmpty(item)) return;
        GameObject slot = ArrivalExam.Hold(p, item, BodyPart.HandRight, ItemSlot);
        if (slot == null)
        {
            Transform old = p.transform.Find(ItemSlot); if (old != null) Destroy(old.gameObject);
            slot = new GameObject(ItemSlot); slot.transform.SetParent(p.transform, false); slot.transform.localPosition = new Vector3(0.3f, 1.1f, 0.45f);
        }
        if (!KnownTools.Contains(item))
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Collider c = g.GetComponent<Collider>(); if (c != null) Destroy(c);
            g.name = item; g.transform.SetParent(slot.transform, false); g.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
            int h = Mathf.Abs(item.GetHashCode());
            g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(Color.HSVToRGB((h % 360) / 360f, 0.55f, 0.85f));
        }
    }

    /// <summary>El objeto que lleva en la mano por su tarea (null si ninguno).</summary>
    public GameObject HeldItem()
    {
        GameObject p = Player(); if (p == null) return null;
        foreach (Transform t in p.GetComponentsInChildren<Transform>(true)) if (t.name == ItemSlot) return t.gameObject;
        return null;
    }

    void FinishStep(Task t)
    {
        PlayerTasks.Step s = t.Current;
        StopActing(t, true);
        GameObject p = Player();
        if (!string.IsNullOrEmpty(s.item)) { Inventory.TryGetValue(s.item, out int n); Inventory[s.item] = n + 1; t.given.Add(s.item); }
        try { s.effect?.Invoke(p); } catch (System.Exception e) { Debug.LogWarning($"[Tareas] efecto del paso «{s.action}»: {e.Message}"); }
        if (t.index < t.markers.Count && t.markers[t.index] != null) Destroy(t.markers[t.index]);
        t.index++;
        if (t.Finished) { Complete(t); return; }
        Restyle(t);
        LessonPrompt.Show($"Hecho: {s.action}. Ahora ({t.index + 1}/{t.Count}): {t.Current.action} — sigue el marcador.", null, 6f);
    }

    void StopActing(Task t, bool keepItem)
    {
        t.acting = false;
        GameObject p = Player();
        if (_holding) { PlayerController pc = p != null ? p.GetComponent<PlayerController>() : null; if (pc != null) pc.HoldBody(false); _holding = false; }
        Anima a = p != null ? p.GetComponent<Anima>() : null;
        if (a != null) DailyPoses.Stand().ApplyFinal(a);
        if (!keepItem && p != null) ArrivalExam.Hold(p, null, BodyPart.HandRight, ItemSlot);
    }

    // CIERRE: el efecto de la misión en el santuario, el progreso y el aviso a quien la pidió.
    void Complete(Task t)
    {
        t.completed = true;
        GameObject p = Player();
        if (p != null) ArrivalExam.Hold(p, null, BodyPart.HandRight, ItemSlot);
        try { t.effectRan = AreaFunctions.Run(t.plan.id, "Kushal"); } catch (System.Exception e) { Debug.LogWarning($"[Tareas] efecto de «{t.plan.id}»: {e.Message}"); }
        Progress.MarkMission(t.plan.id);
        Completed.Add(t.plan.id);
        Clear(t);
        if (Active == t) Active = null;
        Debug.Log($"[Tareas] Kushal termina «{t.plan.title}» con sus {t.Count} pasos{(t.effectRan ? " (efecto en el santuario aplicado)" : "")}; recibió: {(t.given.Count > 0 ? string.Join(", ", t.given) : "nada")}.");
        LessonPrompt.Show($"¡Tarea hecha! «{t.plan.title}» ({t.Count}/{t.Count} pasos).", null, 7f);
        KushalVoice.Reply(KushalVoice.Ctx.Thanks);
        try { t.onDone?.Invoke(t); } catch (System.Exception e) { Debug.LogWarning($"[Tareas] al cerrar «{t.plan.id}»: {e.Message}"); }
    }

    // ── PASOS EN VIVO (las iniciaciones de área) ───────────────────────────────────────────────────────────────────
    class LiveStep { public object owner; public string title, action; public Vector3 spot; public int idx, total, frame; public System.Func<bool> press; public AreaFunctions.Kind? gesture; }
    LiveStep _live;
    GameObject _liveMarker;

    /// <summary>Una iniciación pide al jugador un paso ahora (llamar CADA fotograma mientras lo espera; si deja de llamarse,
    /// caduca solo). La E en <paramref name="spot"/> llama a <paramref name="press"/> (true = hecho).</summary>
    public static void Live(object owner, string title, string action, Vector3 spot, int idx, int total, System.Func<bool> press, AreaFunctions.Kind? gesture = null)
    {
        PlayerTaskRunner r = Ensure();
        if (r._live == null || r._live.owner != owner || r._live.idx != idx) r._live = new LiveStep { owner = owner };
        LiveStep l = r._live;
        l.title = title; l.action = action; l.spot = spot; l.idx = idx; l.total = total; l.press = press; l.gesture = gesture; l.frame = Time.frameCount;
    }

    LiveStep LiveNow() => _live != null && Time.frameCount - _live.frame <= 5 ? _live : null;
    /// <summary>¿Hay una iniciación esperando un paso del jugador? (y dónde)</summary>
    public bool LiveActive => LiveNow() != null;
    public Vector3 LiveSpot => LiveNow() != null ? _live.spot : Vector3.zero;
    public string LiveAction => LiveNow()?.action;
    public int LiveIdx => LiveNow() != null ? _live.idx : -1;
    public GameObject LiveMarker => LiveNow() != null ? _liveMarker : null;
    public bool NearLive => LiveNow() != null && Near(Player(), _live.spot);

    // ── CADA FOTOGRAMA ─────────────────────────────────────────────────────────────────────────────────────────────
    void Update()
    {
        LiveStep lv = LiveNow();
        if (lv != null)
        {
            if (_liveMarker == null) _liveMarker = Marker($"{MarkerPrefix}_iniciacion", lv.spot, lv.action);
            _liveMarker.transform.position = lv.spot;
            WorldLabel wl = _liveMarker.GetComponent<WorldLabel>(); if (wl != null) wl.text = $"E — {lv.action}";
            Material mat = KitchenProps.Mat(new Color(1f, 0.78f, 0.2f));
            foreach (Renderer r in _liveMarker.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
        }
        else if (_liveMarker != null) { Destroy(_liveMarker); _liveMarker = null; }

        if ((lv != null || Active != null) && !GameKeys.DialogOpen && GameKeys.GetKeyDown(KeyCode.E)) Debug.Log($"[Tareas] (E) {Press()}");

        // El rombo de los marcadores gira (se ven de lejos).
        if (Active != null) { GameObject m = Active.CurrentMarker; Transform rb = m != null ? m.transform.Find("Rombo") : null; if (rb != null) rb.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World); }

        GameObject p = Player();
        Anima a = p != null ? p.GetComponent<Anima>() : null;
        bool acting = (Active != null && Active.acting) || Time.time < _liveActUntil;
        if (acting && a != null && Time.time >= _nextFrame)
        {
            _nextFrame = Time.time + 0.6f; _frameB = !_frameB;
            PlayerTasks.Gesture(_actKind, _frameB).ApplyFinal(a);
        }
        if (Active != null && Active.acting && Time.time >= Active.actUntil) FinishStep(Active);
    }

    // ── HUD: el progreso, la pista de ruta y el aviso «E — acción» ─────────────────────────────────────────────────
    GUIStyle _st, _big;
    void OnGUI()
    {
        GameObject p = Player();
        LiveStep lv = LiveNow();
        Task t = Active;
        if (p == null || (lv == null && t == null)) return;
        if (_st == null) { _st = new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true, wordWrap = true }; _big = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true }; }

        string title; Vector3 spot; string action; string label; var lines = new List<string>();
        if (lv != null) { title = lv.title; spot = lv.spot; action = lv.action; label = "tu sitio"; lines.Add($"▶ {lv.idx + 1}/{lv.total}. {lv.action}"); }
        else
        {
            title = $"{t.plan.title} · {t.plan.area}"; spot = t.CurrentSpot.pos; action = t.Current?.action; label = t.CurrentSpot.label;
            for (int i = 0; i < t.Count; i++) lines.Add($"{(i < t.index ? "✓" : i == t.index ? "▶" : "·")} {i + 1}. {t.plan.steps[i].action}");
        }
        bool near = Near(p, spot);
        string route = near ? "Estás en el sitio." : Route(p.transform.position, spot, label);
        float h = 46f + lines.Count * 18f + 22f;
        var box = new Rect(10f, Screen.height * 0.32f, 400f, h);
        GUI.Box(box, GUIContent.none);
        GUI.Label(new Rect(box.x + 8f, box.y + 4f, box.width - 16f, 36f), $"<b>Tarea:</b> {title}", _st);
        for (int i = 0; i < lines.Count; i++) GUI.Label(new Rect(box.x + 8f, box.y + 40f + i * 18f, box.width - 16f, 20f), lines[i], _st);
        GUI.Label(new Rect(box.x + 8f, box.y + 42f + lines.Count * 18f, box.width - 16f, 20f), route, _st);

        if (t != null && t.acting)
        {
            float k = 1f - Mathf.Clamp01((t.actUntil - Time.time) / ActionSeconds);
            var r = new Rect(Screen.width * 0.5f - 200f, Screen.height * 0.6f, 400f, 44f);
            GUI.Box(r, GUIContent.none);
            GUI.Label(new Rect(r.x, r.y + 2f, r.width, 24f), $"{t.Current.action}…", _st);
            GUI.Box(new Rect(r.x + 8f, r.y + 28f, (r.width - 16f) * k, 10f), GUIContent.none);
        }
        else if (near && !string.IsNullOrEmpty(action))
        {
            var r = new Rect(Screen.width * 0.5f - 220f, Screen.height * 0.6f, 440f, 40f);
            GUI.Box(r, GUIContent.none);
            GUI.Label(r, $"E — {action}", _big);
        }
    }

    /// <summary>La pista de ruta: distancia, hacia dónde (respecto a la cámara) y si hay que subir o bajar.</summary>
    public static string Route(Vector3 from, Vector3 to, string label)
    {
        Vector3 d = to - from; float dy = d.y + 1f; d.y = 0f;
        Camera cam = Camera.main;
        Vector3 fw = cam != null ? cam.transform.forward : Vector3.forward; fw.y = 0f; if (fw.sqrMagnitude < 1e-4f) fw = Vector3.forward;
        float ang = Vector3.SignedAngle(fw.normalized, d.sqrMagnitude > 1e-4f ? d.normalized : fw.normalized, Vector3.up);
        string dir = ang > -22.5f && ang <= 22.5f ? "↑ de frente" : ang > 22.5f && ang <= 67.5f ? "↗ delante a la derecha" : ang > 67.5f && ang <= 112.5f ? "→ a la derecha"
                   : ang > 112.5f && ang <= 157.5f ? "↘ detrás a la derecha" : ang < -22.5f && ang >= -67.5f ? "↖ delante a la izquierda" : ang < -67.5f && ang >= -112.5f ? "← a la izquierda"
                   : ang < -112.5f && ang >= -157.5f ? "↙ detrás a la izquierda" : "↓ detrás";
        string up = dy > 2.5f ? $" · sube {dy:0} m (otro piso: alfombra o nadando)" : dy < -2.5f ? $" · baja {-dy:0} m" : "";
        return $"→ {label}: {d.magnitude:0} m, {dir}{up}";
    }
}
