using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Velocidades y tiempos de la línea (docs/sitio-nuclear.md §9 y §20). Son valores del juego, sostenidos en precedentes reales.</summary>
public static class SiteLineRule
{
    /// <summary>LA LÍNEA DE SÚPER VELOCIDAD: 20 m/s, justo bajo el récord de ascensores de pasajeros (20,5 m/s, Shanghái); 1,5 m/s², el tope de confort de los
    /// ascensores rápidos (≤ 1–1,5 m/s²). Un pozo de mina hondo va a ~19–20 m/s; ninguno de pasajeros bajo el mar.</summary>
    public const float CarSpeed = 20f, CarAccel = 1.5f;
    /// <summary>EL LÍMITE DE BERÓN: de andén a galería, esclusas incluidas, menos de un minuto. La carga de la escena va dentro.</summary>
    public const float MaxOneWay = 60f;
    /// <summary>Puertas estancas a cada lado del viaje (s): el carro es estanco y va a presión de cabina, no hay que igualar nada (valor del juego).</summary>
    public const float LockSeconds = 8f;
    /// <summary>La salida de escape: jaula lenta, a baterías, en su propio pozo; y lo que se anda hasta su pie.</summary>
    public const float EscapeSpeed = 6f, EscapeAccel = 0.5f, EscapeWalkM = 13f, WalkSpeed = 1.4f;
    /// <summary>Tiempo de inspección abajo (s reales) y la misión que lo contiene (h de juego).</summary>
    public const float InspectSeconds = 90f, MissionHours = 2f;
    /// <summary>DESCONEXIÓN (docs §17): en emergencia, 90 s (lo que pide API a una desconexión de riser); volver a conectar, las 4 h de una maniobra planificada (600 s de juego).</summary>
    public const float DisconnectSeconds = 90f, ReconnectSeconds = 600f, SwingDeg = 1.5f;
    /// <summary>Lo que se ve del tubo en el santuario 1 (m): del casquete al fondo somero. Lo demás está en el Santuario Marino.</summary>
    public const float VisibleDrop = 14f;
    /// <summary>Tope (s) para que acabe de cargar la escena al llegar. Si no carga, el carro vuelve: nadie se queda en el vacío.</summary>
    public const float LoadLimit = 20f;
    /// <summary>Tope (s reales) de la estancia del jugador abajo, sin avisar de que sube.</summary>
    public const float PlayerStayLimit = 900f;

    /// <summary>Del andén a la galería (m): el mar hondo, la roca y la altura hasta el suelo de las salas.</summary>
    public static float Drop => -(NuclearSiteRule.CavernCenterY - NuclearSiteRule.CavernHeight * 0.5f + 0.6f);

    /// <summary>Tiempo (s) de recorrer <paramref name="len"/> m arrancando y frenando.</summary>
    public static float Run(float len, float v, float a) => len >= v * v / a ? len / v + v / a : 2f * Mathf.Sqrt(len / a);

    /// <summary>Metros recorridos a los <paramref name="t"/> s de ese viaje.</summary>
    public static float Pos(float len, float v, float a, float t)
    {
        float T = Run(len, v, a), ta = Mathf.Min(v / a, T * 0.5f), top = a * ta;
        t = Mathf.Clamp(t, 0f, T);
        if (t < ta) return 0.5f * a * t * t;
        if (t > T - ta) { float r = T - t; return len - 0.5f * a * r * r; }
        return 0.5f * a * ta * ta + top * (t - ta);
    }

    public static float OneWay(float len) => Run(len, CarSpeed, CarAccel) + 2f * LockSeconds;
    public static float RoundTrip(float len) => 2f * OneWay(len) + InspectSeconds;
    public static float EscapeSeconds(float drop, float walk) => walk / WalkSpeed + Run(drop, EscapeSpeed, EscapeAccel);
}

/// <summary>
/// LA LÍNEA DE LAS CAVERNAS (docs/sitio-nuclear.md §20): vive en el santuario 1, donde están su estación y los primeros metros del tubo. Lo hondo es otra escena, Santuario_Marino,
/// y solo se carga para quien va en persona (Kushal), aditiva, durante el viaje: el santuario sigue vivo arriba y la memoria solo sube mientras él está abajo.
/// Los demás viajan SIN CARGAR NADA: el carro baja lo que se ve, el cuerpo se oculta y el viaje corre en <see cref="Depth"/>, con el mismo tiempo, las mismas emergencias y la misma salida.
/// Quien va dentro cuenta como «en alfombra» (<see cref="FlyingCarpet.Carry"/>). A la estación se llega en alfombra. Todas las esperas son finitas.
/// </summary>
public class SiteLine : MonoBehaviour
{
    public const string AreaName = "Línea de las cavernas", MissionId = "nuc_bajada", MarineScene = "Santuario_Marino";
    const float BoardRadius = 9f;

    public static SiteLine Instance { get; private set; }

    [Tooltip("La cabeza del pozo de escape, en el casquete.")] public Transform escapeTop;
    [Tooltip("El carro.")] public Transform car;
    [Tooltip("El tramo de tubo que suelta: del casquete hacia abajo.")] public Transform upperTube;

    /// <summary>Línea fuera de servicio (humo en el tubo, avería): solo queda el pozo de escape.</summary>
    public bool OutOfService;
    /// <summary>Desconectada porque viene un iceberg (<see cref="Iceberg"/>): la compuerta del lecho, cerrada.</summary>
    public bool Disconnected;
    /// <summary>El pozo de escape, también fuera de servicio: solo queda el refugio.</summary>
    public bool EscapeOut;
    public bool LineClosed => OutOfService || Disconnected || Separation > 0.001f;
    /// <summary>0 = conectado; 1 = suelto y apartado.</summary>
    public float Separation { get; private set; }
    public void SetSeparation(float s) { Separation = Mathf.Clamp01(s); ApplySwing(); }
    public bool Busy { get; private set; }
    public int Trips { get; private set; }
    public int Escapes { get; private set; }
    /// <summary>Cómo acabó el último viaje: «línea», «vuelta» (se volvió atrás) o «escape», y lo que tardó.</summary>
    public string LastExit { get; private set; } = "";
    public float LastSeconds { get; private set; }
    /// <summary>¿Hay alguien esperando en el refugio?</summary>
    public bool Refuged { get; private set; }
    /// <summary>A qué hondura va el carro (m bajo el andén), se vea o no.</summary>
    public float Depth { get; private set; }
    /// <summary>¿Va alguien por la parte que no se ve (o abajo)?</summary>
    public bool RiderBelow => Busy && Depth > SiteLineRule.VisibleDrop;

    readonly HashSet<string> _rode = new HashSet<string>();
    bool _inTube, _wantUp, _ownLoad;
    Quaternion _upperHome = Quaternion.identity;

    public float Length => SiteLineRule.Drop;
    public float EscapeDrop => SiteLineRule.Drop;
    public float EscapeWalk => SiteLineRule.EscapeWalkM;

    /// <summary>¿Se puede bajar ahora? No si se evacúa, si la roca está en alarma, si ya va alguien o si la línea está cerrada.</summary>
    public bool CanRide => !Busy && !LineClosed && !NuclearSite.Evacuating && (CavernRock.Instance == null || !CavernRock.Instance.Closed);
    bool Recall => NuclearSite.Evacuating || Disconnected || (CavernRock.Instance != null && CavernRock.Instance.Closed);

    void Awake() { if (upperTube != null) _upperHome = upperTube.localRotation; }
    void OnEnable() { Instance = this; WorkdayDirector.RegisterPlace(AreaName, transform); }
    void OnDisable() { if (Instance == this) Instance = null; }

    // LA DESCONEXIÓN: suelta en cuanto el carro no está en el tramo que se aparta; vuelve despacio, con el peligro ya pasado.
    void Update()
    {
        if (upperTube == null) return;
        float s = Separation;
        if (Disconnected) { if (!_inTube) s = Mathf.MoveTowards(s, 1f, Time.deltaTime / SiteLineRule.DisconnectSeconds); }
        else s = Mathf.MoveTowards(s, 0f, Time.deltaTime / SiteLineRule.ReconnectSeconds);
        if (s != Separation) { Separation = s; ApplySwing(); }
    }

    void ApplySwing() { if (upperTube != null) upperTube.localRotation = _upperHome * Quaternion.Euler(SiteLineRule.SwingDeg * Separation, 0f, 0f); }

    /// <summary>La jornada: quien tiene la bajada y ya está en la estación, sube (una vez al día). true = va dentro.</summary>
    public static bool Board(GameObject g, string mission = MissionId)
    {
        SiteLine l = Instance;
        if (l == null || g == null || FlyingCarpet.Riding(g)) return false;
        if ((g.transform.position - l.transform.position).sqrMagnitude > BoardRadius * BoardRadius) return false;
        string key = $"{g.name}#{mission}#{(TimeController.timeController != null ? TimeController.timeController.Day : 0)}";
        if (l._rode.Contains(key) || !l.Ride(g, SiteLineRule.InspectSeconds)) return false;
        l._rode.Add(key);
        return true;
    }

    /// <summary>Baja a <paramref name="who"/>, lo deja <paramref name="dwell"/> s abajo y lo sube. Solo el jugador carga la escena de abajo.</summary>
    public bool Ride(GameObject who, float dwell)
    {
        if (who == null || !CanRide) return false;
        StartCoroutine(Trip(who, dwell));
        return true;
    }

    /// <summary>Kushal, abajo, en el andén: sube cuando quiera.</summary>
    public void AskUp(GameObject p)
    {
        if (!Busy || p == null) return;
        ChoiceDialog.Show($"{AreaName}: subir", new[] { "Volver a la superficie", "Quedarse" }, i => { if (i == 0) _wantUp = true; }, null, p.transform, 6f);
    }

    // Al jugador en la estación: bajar.
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || FlyingCarpet.Riding(other.gameObject)) return;
        GameObject p = other.gameObject;
        if (!CanRide) { ChoiceDialog.Show($"{AreaName}: cerrada", new[] { "Entendido" }, _ => { }, null, transform, 4f); return; }
        ChoiceDialog.Show($"{AreaName}: {SiteLineRule.OneWay(Length):0} s hasta la galería", new[] { "Bajar a las cavernas", "Quedarse" },
            i => { if (i == 0) Ride(p, SiteLineRule.PlayerStayLimit); }, null, transform, 4f);
    }

    // Quien va dentro y cómo devolverlo: lo mismo que apaga y enciende la alfombra.
    sealed class Rider
    {
        public GameObject who; public Anima a; public UnityEngine.AI.NavMeshAgent agent; public CharacterController cc; public PlayerController pc;
        public bool agentWas, ccWas, player, inMarine, hidden; public float above;
        public readonly List<Renderer> off = new List<Renderer>();
    }

    IEnumerator Trip(GameObject who, float dwell)
    {
        Busy = true; Trips++; _wantUp = false;
        float t0 = Time.time;
        var r = new Rider
        {
            who = who, above = RootAbove(who), player = who.CompareTag("Player"), a = who.GetComponent<Anima>(), agent = who.GetComponent<UnityEngine.AI.NavMeshAgent>(),
            cc = who.GetComponent<CharacterController>(), pc = who.GetComponent<PlayerController>(),
        };
        r.agentWas = r.agent != null && r.agent.enabled; r.ccWas = r.cc != null && r.cc.enabled;
        if (r.agent != null) r.agent.enabled = false;
        if (r.cc != null) r.cc.enabled = false;
        if (r.pc != null) r.pc.HoldBody(true);
        if (r.player && CameraManager.Instance != null) CameraManager.Instance.SwitchTo(CameraMode.ThirdPerson, forced: true);
        FlyingCarpet.Carry(who, true);
        if (r.player) LoadMarine();   // ya: la carga va dentro de los 60 s

        float len = Length, s = 0f, t = 0f, run = SiteLineRule.Run(len, SiteLineRule.CarSpeed, SiteLineRule.CarAccel);
        yield return Wait(r, SiteLineRule.LockSeconds, true);
        bool back = Recall;
        _inTube = !back;
        // BAJA. Si en el camino se declara la emergencia o la roca entra en alarma, frena y vuelve desde donde esté.
        while (!back && t < run)
        {
            if (Recall) { back = true; break; }
            t += Time.deltaTime;
            s = SiteLineRule.Pos(len, SiteLineRule.CarSpeed, SiteLineRule.CarAccel, t);
            Stand(r, s);
            yield return null;
        }
        if (!back)
        {
            s = len; _inTube = false;
            Stand(r, s);
            if (r.player) yield return Arrive(r);
            if (r.player && !r.inMarine) back = true;   // la escena no cargó: vuelve
            else
            {
                yield return Wait(r, SiteLineRule.LockSeconds, true);
                if (r.inMarine) { float w = 0f; while (w < SiteLineRule.PlayerStayLimit && !_wantUp && !Recall && !LineClosed) { w += Time.unscaledDeltaTime; yield return null; } }
                else yield return Wait(r, dwell, true);
                if (LineClosed)
                {
                    if (EscapeOut) yield return InRefuge(r);
                    if (LineClosed) { yield return EscapeUp(r, t0); yield break; }
                }
                yield return Wait(r, SiteLineRule.LockSeconds, false);
                if (r.inMarine) yield return LeaveMarine(r);
            }
        }
        // SUBE, desde donde esté.
        _inTube = true;
        run = SiteLineRule.Run(s, SiteLineRule.CarSpeed, SiteLineRule.CarAccel);
        for (float u = 0f; u < run; u += Time.deltaTime)
        {
            Stand(r, s - SiteLineRule.Pos(s, SiteLineRule.CarSpeed, SiteLineRule.CarAccel, u));
            yield return null;
        }
        _inTube = false;
        Stand(r, 0f);
        yield return Wait(r, SiteLineRule.LockSeconds, false);
        Finish(r, transform.position, back ? "vuelta" : "línea", t0);
    }

    // ── EN PERSONA: la escena de abajo ─────────────────────────────────────────────────────────────────────────────

    void LoadMarine()
    {
        if (SceneManager.GetSceneByName(MarineScene).isLoaded) return;
        if (!Application.CanStreamedLevelBeLoaded(MarineScene)) { Debug.LogWarning($"[Línea] Falta {MarineScene} en Build Settings: Tools/Cold Sanctuary/Build Santuario Marino."); return; }
        _ownLoad = true;
        SceneManager.LoadSceneAsync(MarineScene, LoadSceneMode.Additive);
    }

    void UnloadMarine()
    {
        if (!_ownLoad) return;
        _ownLoad = false;
        if (SceneManager.GetSceneByName(MarineScene).isLoaded) SceneManager.UnloadSceneAsync(MarineScene);
    }

    IEnumerator Arrive(Rider r)
    {
        float w = 0f;
        while (MarineBase.Instance == null && w < SiteLineRule.LoadLimit) { w += Time.unscaledDeltaTime; yield return null; }
        MarineBase b = MarineBase.Instance;
        if (b == null) { Debug.LogWarning("[Línea] La escena de abajo no ha cargado a tiempo: el carro vuelve."); yield break; }
        r.inMarine = true;
        if (r.who == null) yield break;
        r.who.transform.position = b.arrival.position + Vector3.up * (0.1f + r.above);
        Physics.SyncTransforms();
        b.Enter();
        if (r.cc != null) r.cc.enabled = r.ccWas;
        if (r.pc != null) r.pc.HoldBody(false);
        ScreenFader.Get().FadeFromBlack();
    }

    IEnumerator LeaveMarine(Rider r)
    {
        bool black = false;
        ScreenFader.Get().FadeToBlack(() => black = true);
        float w = 0f;
        while (!black && w < 5f) { w += Time.unscaledDeltaTime; yield return null; }
        if (r.cc != null) r.cc.enabled = false;
        if (r.pc != null) r.pc.HoldBody(true);
        if (MarineBase.Instance != null) MarineBase.Instance.Leave();
        r.inMarine = false;
    }

    // AL REFUGIO: la línea y el pozo, cerrados. Espera a que se abra uno, hasta que se acabe el aire (finito: las horas del refugio, en tiempo de juego).
    IEnumerator InRefuge(Rider r)
    {
        Refuge f = Refuge.Instance;
        if (r.inMarine && r.who != null && MarineBase.Instance != null) r.who.transform.position = MarineBase.Instance.refugeSpot.position + Vector3.up * (0.1f + r.above);
        if (f != null) f.Enter();
        float secondsPerHour = TimeController.timeController != null ? TimeController.timeController.SecondsPerGameDay / 24f : 150f, left = (f != null ? f.HoursLeft : RefugeRule.StoredHours) * secondsPerHour;
        Refuged = true;
        while (LineClosed && EscapeOut && left > 0f)
        {
            left -= Time.deltaTime;
            if (f != null) f.Use(Time.deltaTime / secondsPerHour);
            yield return null;
        }
        Refuged = false;
        if (left <= 0f) EscapeOut = false;   // (acabado el aire, el rescate: nada se queda esperando para siempre)
        if (f != null) f.Leave();
    }

    // POR EL POZO DE ESCAPE: a pie hasta su pie y en la jaula hasta el casquete; otro pozo, otro aire. No se ve: es su tiempo.
    IEnumerator EscapeUp(Rider r, float t0)
    {
        Escapes++;
        if (r.inMarine) yield return LeaveMarine(r);
        float T = SiteLineRule.EscapeSeconds(EscapeDrop, EscapeWalk), rise = EscapeDrop;
        for (float e = 0f; e < T; e += Time.deltaTime)
        {
            Depth = rise * (1f - e / T);
            yield return null;
        }
        Depth = 0f;
        if (car != null) car.position = transform.position;
        Finish(r, escapeTop.position, "escape", t0);
    }

    IEnumerator Wait(Rider r, float seconds, bool canAbort)
    {
        float w = 0f;
        while (w < seconds && !(canAbort && Recall)) { w += Time.deltaTime; Stand(r, Depth); yield return null; }
    }

    // El carro va por lo que se ve; por debajo, el cuerpo se oculta y el viaje sigue en Depth.
    void Stand(Rider r, float depth)
    {
        Depth = depth;
        Vector3 p = transform.position + Vector3.down * Mathf.Min(depth, SiteLineRule.VisibleDrop);
        if (car != null) car.position = p;
        if (r.who == null || r.inMarine) return;
        Hide(r, depth > SiteLineRule.VisibleDrop);
        r.who.transform.position = p + Vector3.up * (0.12f + r.above);
        if (r.a != null) DailyPoses.Stand().ApplyFinal(r.a);
    }

    void Hide(Rider r, bool on)
    {
        if (r.hidden == on) return;
        r.hidden = on;
        if (r.player) { if (on) ScreenFader.Get().FadeToBlack(); return; }
        if (on) { r.off.Clear(); foreach (Renderer x in r.who.GetComponentsInChildren<Renderer>()) if (x.enabled) { x.enabled = false; r.off.Add(x); } }
        else { foreach (Renderer x in r.off) if (x != null) x.enabled = true; r.off.Clear(); }
    }

    void Finish(Rider r, Vector3 at, string exit, float t0)
    {
        LastExit = exit; LastSeconds = Time.time - t0; Busy = false; Depth = 0f; _inTube = false;
        FlyingCarpet.Carry(r.who, false);
        if (r.player) UnloadMarine();
        if (r.who == null) return;
        Hide(r, false);
        if (r.a != null) DailyPoses.Stand().ApplyFinal(r.a);
        r.who.transform.position = at + Vector3.up * (0.02f + r.above);
        if (r.cc != null) r.cc.enabled = r.ccWas;
        if (r.agent != null) { r.agent.enabled = r.agentWas; if (r.agent.enabled && r.agent.isOnNavMesh) r.agent.Warp(r.who.transform.position); }
        if (r.pc != null) r.pc.HoldBody(false);
        if (r.player)
        {
            ScreenFader.Get().FadeFromBlack();
            if (CameraManager.Instance != null) CameraManager.Instance.SwitchTo(CameraManager.Instance.preferredMode, forced: true);
        }
        Debug.Log($"[Línea] «{r.who.name}» sale por «{exit}» tras {LastSeconds:0} s.");
    }

    // Lo que va el origen de un cuerpo sobre sus pies (como en la alfombra).
    static float RootAbove(GameObject who)
    {
        float best = float.MaxValue;
        foreach (RaycastHit h in Physics.RaycastAll(who.transform.position + Vector3.up * 0.3f, Vector3.down, 2.5f, ~0, QueryTriggerInteraction.Ignore))
            if (!h.transform.IsChildOf(who.transform) && h.point.y <= who.transform.position.y + 0.05f) best = Mathf.Min(best, who.transform.position.y - h.point.y);
        return best == float.MaxValue ? 0f : Mathf.Clamp(best, 0f, 1.6f);
    }
}
