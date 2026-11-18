using UnityEngine;

/// <summary>
/// LA CUENTA DEL VIAJE AUTOMÁTICO, PURA (sin escena; <see cref="CodeShipLogicTest"/>): a qué punto se va, cuándo se ha llegado y cuánto
/// se espera como mucho (ninguna espera sin tope).
/// </summary>
public static class TravelMath
{
    public delegate bool Sampler(Vector3 p, float maxDistance, out Vector3 hit);

    /// <summary>El tope de cualquier viaje, en segundos.</summary>
    public const float MaxSeconds = 240f;
    /// <summary>Diferencia de altura máxima para «he llegado» (otro piso no vale; como <see cref="PlayerTaskRunner.ReachUp"/>).</summary>
    public const float MaxRise = 2.5f;

    /// <summary>
    /// El punto al que se va para llegar a <paramref name="center"/> desde <paramref name="from"/>:
    ///  1) el punto andable (NavMesh) más cercano al objetivo, si queda a menos de <paramref name="search"/> m (el centro de una mesa o una
    ///     consola está dentro de su caja: el NavMesh da su borde);
    ///  2) si no, el borde de su caja <paramref name="solid"/> del lado de quien va, más <paramref name="margin"/>;
    ///  3) si no, el propio objetivo.
    /// </summary>
    public static Vector3 ResolveGoal(Vector3 center, Vector3 from, Sampler nav, Bounds? solid, float margin = 0.6f, float search = 3f)
    {
        if (nav != null && nav(center, search, out Vector3 hit) && Flat(hit - center) <= search) return hit;
        if (solid.HasValue)
        {
            Bounds b = solid.Value;
            var p = new Vector3(Mathf.Clamp(from.x, b.min.x, b.max.x), center.y, Mathf.Clamp(from.z, b.min.z, b.max.z));
            Vector3 dir = from - center; dir.y = 0f;
            if (dir.sqrMagnitude < 1e-6f) dir = Vector3.forward;
            return p + dir.normalized * margin;
        }
        return center;
    }

    /// <summary>¿Ha llegado? Cerca del punto (0,8 m) o del objetivo (<paramref name="arrive"/>), en planta y en el mismo piso.</summary>
    public static bool Arrived(Vector3 player, Vector3 goal, Vector3 center, float arrive)
    {
        if (Mathf.Abs(player.y - goal.y) > MaxRise && Mathf.Abs(player.y - center.y) > MaxRise) return false;
        return Flat(player - goal) <= 0.8f || Flat(player - center) <= arrive;
    }

    /// <summary>Segundos como mucho para un viaje de <paramref name="distance"/> m a <paramref name="speed"/> m/s: el doble y medio de lo
    /// que costaría en línea recta (los rodeos, las alfombras) más 10 s, entre 15 s y <see cref="MaxSeconds"/>.</summary>
    public static float Timeout(float distance, float speed) => Mathf.Clamp(distance / Mathf.Max(0.1f, speed) * 2.5f + 10f, 15f, MaxSeconds);

    public static float Flat(Vector3 d) { d.y = 0f; return d.magnitude; }
}

/// <summary>
/// EL VIAJE AUTOMÁTICO «IR» (CodeShip reglas.md §3; docs/design/viaje-a-una-tarea.md): lleva al jugador andando hasta el sitio de una tarea o
/// un área, por el NavMesh de la escena y las alfombras (<see cref="PathMover"/>, lo mismo que usan la jornada y el restaurante).
///  · Por defecto ANDANDO; el teletransporte es una opción del jugador (<see cref="Teleport"/>, en los ajustes de Tareas).
///  · Durante el viaje sale «Parar» y la cámara sigue libre; tocar el joystick o WASD también lo para (manda el jugador).
///  · Con TOPE: si en <see cref="StuckSeconds"/> s no avanza, o pasa <see cref="TravelMath.Timeout"/>, se para y ofrece teletransportar.
///  · Al llegar, si «al llegar, hacer» está puesto (<see cref="ActOnArrive"/>), hace la acción de la tarea (pulsar E, abrir el reto…).
/// </summary>
[DefaultExecutionOrder(-900)]   // después de TouchControls y antes que la historia: la E que pulsa al llegar la lee la historia en el mismo fotograma
public class AutoTravel : MonoBehaviour
{
    static KeyCode? _tap; static int _tapFrame;
    /// <summary>Pulsa <paramref name="k"/> (como el botón E de la pantalla) al principio del próximo Update, antes que la historia y las
    /// tareas lo lean: desde el menú (que corre en OnGUI, tras los Update) un toque directo se perdía.</summary>
    public static void Tap(KeyCode k) { _tap = k; _tapFrame = Time.frameCount; }

    public enum Result { None, Arrived, Cancelled, Stuck, Timeout, NoPlayer, Teleported }

    public const float StuckSeconds = 8f;
    public static AutoTravel Instance { get; private set; }

    /// <summary>Ajuste: teletransporte en vez de andar (CodeShip: nunca por defecto).</summary>
    public static bool Teleport { get => PlayerPrefs.GetInt("cs.viaje.teleport", 0) == 1; set => PlayerPrefs.SetInt("cs.viaje.teleport", value ? 1 : 0); }
    /// <summary>Ajuste: al llegar, hacer la acción de la tarea (por defecto, sí).</summary>
    public static bool ActOnArrive { get => PlayerPrefs.GetInt("cs.viaje.actuar", 1) == 1; set => PlayerPrefs.SetInt("cs.viaje.actuar", value ? 1 : 0); }

    public bool Traveling { get; private set; }
    public string Label { get; private set; } = "";
    public Result Last { get; private set; }
    public string LastMessage { get; private set; } = "";
    /// <summary>Viajes que llegaron (pruebas y onboarding).</summary>
    public static int Arrivals;

    Transform _target, _player;
    Vector3 _goal;
    float _arrive, _deadline, _lastProgressAt, _toastUntil;
    Vector3 _lastProgressPos;
    System.Action _onArrive;
    Transform _failedTarget; float _failedArrive; string _failedLabel; System.Action _failedOnArrive;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() { Ensure(); }

    public static AutoTravel Ensure()
    {
        if (Instance != null) return Instance;
        var go = new GameObject("AutoTravel_AUTO");
        DontDestroyOnLoad(go);
        return go.AddComponent<AutoTravel>();
    }

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }

    static Transform FindPlayer() { GameObject p = GameObject.FindGameObjectWithTag("Player"); return p != null ? p.transform : null; }

    static float Speed(Transform p) => 2.6f * Mathf.Clamp(PersonLife.HeightOf(p) / 1.75f, 0.3f, 2f);

    /// <summary>Echa a andar hacia <paramref name="target"/>. <paramref name="arrive"/>: a cuántos metros vale; <paramref name="onArrive"/>:
    /// lo que se hace al llegar (si el ajuste lo permite). false si no hay jugador o destino.</summary>
    public bool Go(Transform target, string label, float arrive = 2f, System.Action onArrive = null)
    {
        _player = FindPlayer();
        if (_player == null || target == null) { Finish(Result.NoPlayer, "No hay jugador o destino en esta escena."); return false; }
        _target = target; Label = label ?? target.name; _arrive = Mathf.Max(0.8f, arrive); _onArrive = onArrive;
        _goal = GoalFor(target, _player.position);
        if (Teleport) { TeleportNow(); return true; }
        Traveling = true; _toastUntil = 0f;
        float d = TravelMath.Flat(_goal - _player.position);
        _deadline = Time.time + TravelMath.Timeout(d, Speed(_player));
        _lastProgressAt = Time.time; _lastProgressPos = _player.position;
        PathMover.Of(_player).Restart();
        Debug.Log($"[Viaje] Ir a «{Label}» ({d:0} m, tope {_deadline - Time.time:0} s).");
        return true;
    }

    /// <summary>El punto al que se va para llegar a <paramref name="t"/> (NavMesh junto a él, el borde de su caja o él mismo).</summary>
    public static Vector3 GoalFor(Transform t, Vector3 from)
    {
        Bounds? solid = null;
        foreach (Collider c in t.GetComponentsInChildren<Collider>())
        {
            if (c.isTrigger || !c.enabled) continue;
            if (solid == null) solid = c.bounds; else { Bounds b = solid.Value; b.Encapsulate(c.bounds); solid = b; }
        }
        if (solid.HasValue && Mathf.Max(solid.Value.size.x, solid.Value.size.z) > 12f) solid = null;   // un suelo o un edificio entero no es «su caja»
        return TravelMath.ResolveGoal(t.position, from, NavSample, solid);
    }

    static bool NavSample(Vector3 p, float max, out Vector3 hit)
    {
        bool ok = UnityEngine.AI.NavMesh.SamplePosition(p, out UnityEngine.AI.NavMeshHit h, max, UnityEngine.AI.NavMesh.AllAreas);
        hit = ok ? h.position : p;
        return ok;
    }

    public void Cancel(string why = "Viaje parado.")
    {
        if (!Traveling) return;
        Finish(Result.Cancelled, why);
    }

    /// <summary>Pone al jugador en el sitio (la opción del jugador, o tras un atasco). true = lo puso.</summary>
    public bool TeleportNow()
    {
        Transform p = _player != null ? _player : FindPlayer();
        Transform t = _target != null ? _target : _failedTarget;
        if (p == null || t == null) return false;
        if (_target == null) { _target = _failedTarget; _arrive = _failedArrive; Label = _failedLabel; _onArrive = _failedOnArrive; }
        Vector3 goal = GoalFor(t, p.position);
        CharacterController cc = p.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        p.position = PathMover.Of(p).OnGround(goal);
        if (cc != null) cc.enabled = true;
        Physics.SyncTransforms();
        Debug.Log($"[Viaje] Teletransporte a «{Label}».");
        Arrive(Result.Teleported);
        return true;
    }

    void Update()
    {
        if (_tap.HasValue && Time.frameCount > _tapFrame) { GameKeys.TapVirtual(_tap.Value); _tap = null; }
        if (!Traveling) return;
        if (_player == null || _target == null || !_target.gameObject.activeInHierarchy) { Finish(Result.Cancelled, "El destino ya no está."); return; }
        // Manda el jugador: tocar el joystick o WASD para el viaje.
        if (Mathf.Abs(GameKeys.GetAxisRaw("Horizontal")) + Mathf.Abs(GameKeys.GetAxisRaw("Vertical")) > 0.2f) { Finish(Result.Cancelled, "Viaje parado: te mueves tú."); return; }
        if (CodeChallengeSheet.IsOpen) return;   // con un reto abierto no se anda
        Vector3 pos = _player.position;
        if (TravelMath.Arrived(pos, _goal, _target.position, _arrive)) { Arrive(Result.Arrived); return; }
        if (Time.time > _deadline) { Fail(Result.Timeout, $"No llegué a «{Label}» a tiempo (quedan {TravelMath.Flat(_target.position - pos):0} m)."); return; }
        PathMover pm = PathMover.Of(_player);
        bool riding = pm.Trace.Contains("alfombras");   // esperando o volando en alfombra: eso es avanzar
        if (riding || (pos - _lastProgressPos).sqrMagnitude > 0.09f) { _lastProgressPos = pos; _lastProgressAt = Time.time; }
        else if (Time.time - _lastProgressAt > StuckSeconds) { Fail(Result.Stuck, $"Me he atascado camino de «{Label}» (a {TravelMath.Flat(_target.position - pos):0} m)."); return; }
        // El destino se mueve (una persona): se recalcula el punto si se alejó.
        if (TravelMath.Flat(_target.position - _goal) > _arrive + 1.5f) _goal = GoalFor(_target, pos);
        pm.StepToward(_goal, Speed(_player));
    }

    void Arrive(Result r)
    {
        System.Action act = _onArrive;
        Arrivals++;
        Finish(r, $"Has llegado a «{Label}».");
        if (act != null && ActOnArrive) act();
    }

    void Fail(Result r, string msg)
    {
        _failedTarget = _target; _failedArrive = _arrive; _failedLabel = Label; _failedOnArrive = _onArrive;
        Finish(r, msg);
    }

    void Finish(Result r, string msg)
    {
        Traveling = false; Last = r; LastMessage = msg;
        if (r != Result.Stuck && r != Result.Timeout) _failedTarget = null;
        _target = null; _onArrive = null;
        _toastUntil = Time.unscaledTime + (r == Result.Stuck || r == Result.Timeout ? 12f : 3f);
        Debug.Log($"[Viaje] {msg}");
    }

    // ── La barra del viaje y el aviso: arriba en el centro (no tapa los mandos de abajo ni el recuadro de la historia) ──────────────
    GUIStyle _st;
    public static float BarTop => WebLigera.Active ? Mathf.Max(44f, Mathf.Min(Screen.width, Screen.height) * 0.13f) * 0.55f + 16f : 96f;
    static float Unit => WebLigera.Active ? Mathf.Max(44f, Mathf.Min(Screen.width, Screen.height) * 0.07f) : 30f;

    void OnGUI()
    {
        if (ColdSanctuary.TestRunner.Running || CodeChallengeSheet.IsOpen) return;
        if (!Traveling && Time.unscaledTime > _toastUntil) return;
        if (_st == null) _st = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true, fontSize = WebLigera.Active ? 15 : 13, alignment = TextAnchor.MiddleLeft };
        float u = Unit, w = Mathf.Min(520f, Screen.width - 16f);
        var r = new Rect((Screen.width - w) * 0.5f, BarTop, w, u + 12f);
        GUI.Box(r, ""); GUI.Box(r, "");
        if (Traveling)
        {
            float d = _player != null && _target != null ? TravelMath.Flat(_target.position - _player.position) : 0f;
            GUI.Label(new Rect(r.x + 10f, r.y + 6f, w - u * 3.2f - 20f, u), $"➤ Yendo a <b>{Label}</b> · {TaskEntry.DistanceText(d)}", _st);
            if (GUI.Button(new Rect(r.xMax - u * 3f - 6f, r.y + 6f, u * 3f, u), "■ Parar")) Cancel();
            return;
        }
        bool failed = (Last == Result.Stuck || Last == Result.Timeout) && _failedTarget != null;
        GUI.Label(new Rect(r.x + 10f, r.y + 6f, w - (failed ? u * 6.4f : u * 1.2f) - 20f, u), (failed ? "✗ " : "✓ ") + LastMessage, _st);
        if (failed && GUI.Button(new Rect(r.xMax - u * 6.2f - 6f, r.y + 6f, u * 5f, u), "Teletransportar")) { TeleportNow(); return; }
        if (GUI.Button(new Rect(r.xMax - u - 6f, r.y + 6f, u, u), "✕")) { _toastUntil = 0f; _failedTarget = null; }
    }
}
