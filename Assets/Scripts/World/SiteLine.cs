using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Velocidades y tiempos de la línea (docs/sitio-nuclear.md §8). Son valores del juego, sostenidos en precedentes reales.</summary>
public static class SiteLineRule
{
    /// <summary>LA LÍNEA DE SÚPER VELOCIDAD (docs §9): 20 m/s, justo bajo el récord de ascensores de pasajeros (20,5 m/s, Shanghái); 1,5 m/s², el tope de confort de los
    /// ascensores rápidos (≤ 1–1,5 m/s²). Un ascensor de obra de mina va a ~19–20 m/s; ninguno de pasajeros bajo el mar.</summary>
    public const float CarSpeed = 20f, CarAccel = 1.5f;
    /// <summary>EL LÍMITE DE BERÓN: de andén a galería, esclusas incluidas, menos de un minuto.</summary>
    public const float MaxOneWay = 60f;
    /// <summary>Puertas estancas a cada lado del viaje (s): el carro es estanco y va a presión de cabina, no hay que igualar nada (valor del juego).</summary>
    public const float LockSeconds = 8f;
    /// <summary>El ascensor vertical con el que se compara: las mismas cifras, sin los ~10° de inclinación.</summary>
    public const float HoistSpeed = CarSpeed, HoistAccel = CarAccel;
    /// <summary>La salida de escape: jaula lenta, a baterías, en su propio pozo.</summary>
    public const float EscapeSpeed = 6f, EscapeAccel = 0.5f, WalkSpeed = 1.4f;
    /// <summary>Tiempo de inspección abajo (s reales) y la misión que lo contiene (h de juego).</summary>
    public const float InspectSeconds = 90f, MissionHours = 2f;

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
    public static float HoistOneWay(float drop) => Run(drop, HoistSpeed, HoistAccel) + 2f * LockSeconds;
    public static float RoundTrip(float len) => 2f * OneWay(len) + InspectSeconds;
    public static float EscapeSeconds(float drop, float walk) => walk / WalkSpeed + Run(drop, EscapeSpeed, EscapeAccel);
}

/// <summary>
/// LA LÍNEA DE LAS CAVERNAS (docs/sitio-nuclear.md §8–§9): un tubo sellado de súper velocidad, del borde del casquete a la galería entre las dos cavernas (por el pilar de roca),
/// y una SALIDA DE ESCAPE en su propio pozo, sin pasar por el del reactor. A la estación de arriba se llega en alfombra (es una pista más de la red del
/// edificio). Quien va dentro cuenta como «en alfombra» (<see cref="FlyingCarpet.Carry"/>): lo demás del juego ya sabe no tocarlo. Un viaje con la zona en
/// emergencia o la roca en alarma vuelve atrás; con la línea cerrada, se sale por el pozo de escape. Todas las esperas son finitas.
/// </summary>
public class SiteLine : MonoBehaviour
{
    public const string AreaName = "Línea de las cavernas", MissionId = "nuc_bajada";
    const float BoardRadius = 9f;

    public static SiteLine Instance { get; private set; }

    [Tooltip("Donde para el carro abajo, en la galería.")] public Transform bottom;
    [Tooltip("Pie y cabeza del pozo de escape.")] public Transform escapeBottom, escapeTop;
    [Tooltip("El carro y la jaula de escape.")] public Transform car, cage;

    /// <summary>Línea fuera de servicio (humo en el tubo, avería): solo queda el pozo de escape.</summary>
    public bool OutOfService;
    /// <summary>Desconectada porque viene un iceberg (<see cref="Iceberg"/>): la compuerta del lecho, cerrada.</summary>
    public bool Disconnected;
    /// <summary>El pozo de escape, también fuera de servicio: solo queda el refugio.</summary>
    public bool EscapeOut;
    public bool LineClosed => OutOfService || Disconnected;
    [Tooltip("La compuerta estanca del lecho.")] public Transform seabedGate;
    public bool Busy { get; private set; }
    public int Trips { get; private set; }
    public int Escapes { get; private set; }
    /// <summary>Cómo acabó el último viaje: «línea», «vuelta» (se volvió atrás) o «escape», y lo que tardó.</summary>
    public string LastExit { get; private set; } = "";
    public float LastSeconds { get; private set; }
    /// <summary>¿Hay alguien esperando en el refugio?</summary>
    public bool Refuged { get; private set; }

    readonly HashSet<string> _rode = new HashSet<string>();
    Vector3 _cageHome;

    public float Length => bottom != null ? Vector3.Distance(transform.position, bottom.position) : 0f;
    public float Drop => bottom != null ? transform.position.y - bottom.position.y : 0f;
    public float AngleDeg { get { Vector3 d = bottom.position - transform.position, f = d; f.y = 0f; return Vector3.Angle(d, f); } }
    public float EscapeDrop => escapeTop != null && escapeBottom != null ? escapeTop.position.y - escapeBottom.position.y : 0f;
    public float EscapeWalk => escapeBottom != null && bottom != null ? Vector3.Distance(bottom.position, escapeBottom.position) : 0f;

    /// <summary>¿Se puede bajar ahora? No si se evacúa, si la roca está en alarma, si ya va alguien o si la línea está cerrada.</summary>
    public bool CanRide => !Busy && !LineClosed && !NuclearSite.Evacuating && (CavernRock.Instance == null || !CavernRock.Instance.Closed);
    bool Recall => NuclearSite.Evacuating || Disconnected || (CavernRock.Instance != null && CavernRock.Instance.Closed);

    void Awake() { if (cage != null) _cageHome = cage.position; }
    void OnEnable() { Instance = this; WorkdayDirector.RegisterPlace(AreaName, transform); }
    void OnDisable() { if (Instance == this) Instance = null; }

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

    /// <summary>Baja a <paramref name="who"/>, lo deja <paramref name="dwell"/> s en la galería y lo sube.</summary>
    public bool Ride(GameObject who, float dwell)
    {
        if (who == null || !CanRide) return false;
        StartCoroutine(Trip(who, dwell));
        return true;
    }

    // Al jugador en la estación: bajar y volver. (En las cavernas no hay suelo por el que andar: el viaje es de ida y vuelta.)
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || FlyingCarpet.Riding(other.gameObject)) return;
        GameObject p = other.gameObject;
        if (!CanRide) { ChoiceDialog.Show($"{AreaName}: cerrada", new[] { "Entendido" }, _ => { }, null, transform, 4f); return; }
        ChoiceDialog.Show($"{AreaName}: {SiteLineRule.OneWay(Length):0} s hasta la galería", new[] { "Bajar a las cavernas y volver", "Quedarse" },
            i => { if (i == 0) Ride(p, 20f); }, null, transform, 4f);
    }

    // Quien va dentro y cómo devolverlo: lo mismo que apaga y enciende la alfombra.
    sealed class Rider
    {
        public GameObject who; public Anima a; public UnityEngine.AI.NavMeshAgent agent; public CharacterController cc; public PlayerController pc;
        public bool agentWas, ccWas, player; public float above;
    }

    IEnumerator Trip(GameObject who, float dwell)
    {
        Busy = true; Trips++;
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

        Vector3 up = transform.position, dn = bottom.position;
        float len = Length, s = 0f, t = 0f, run = SiteLineRule.Run(len, SiteLineRule.CarSpeed, SiteLineRule.CarAccel);
        yield return Wait(r, SiteLineRule.LockSeconds, up, true);
        bool back = Recall;
        // BAJA. Si en el camino se declara la emergencia o la roca entra en alarma, frena y vuelve desde donde esté.
        while (!back && t < run)
        {
            if (Recall) { back = true; break; }
            t += Time.deltaTime;
            s = SiteLineRule.Pos(len, SiteLineRule.CarSpeed, SiteLineRule.CarAccel, t);
            Stand(r, Vector3.Lerp(up, dn, s / len));
            yield return null;
        }
        if (!back)
        {
            s = len;
            yield return Wait(r, SiteLineRule.LockSeconds, dn, true);
            yield return Wait(r, dwell, dn, true);
            if (LineClosed)
            {
                if (EscapeOut) yield return InRefuge(r);
                if (LineClosed) { yield return EscapeUp(r, dn, t0); yield break; }
            }
            yield return Wait(r, SiteLineRule.LockSeconds, dn, false);
        }
        // SUBE, desde donde esté.
        run = SiteLineRule.Run(s, SiteLineRule.CarSpeed, SiteLineRule.CarAccel);
        for (float u = 0f; u < run; u += Time.deltaTime)
        {
            float d = s - SiteLineRule.Pos(s, SiteLineRule.CarSpeed, SiteLineRule.CarAccel, u);
            Stand(r, Vector3.Lerp(up, dn, d / len));
            yield return null;
        }
        yield return Wait(r, SiteLineRule.LockSeconds, up, false);
        Stand(r, up);
        Finish(r, up, back ? "vuelta" : "línea", t0);
    }

    // AL REFUGIO: la línea y el pozo, cerrados. Espera a que se abra uno, hasta que se acabe el aire (finito: las horas del refugio, en tiempo de juego).
    IEnumerator InRefuge(Rider r)
    {
        Refuge f = Refuge.Instance;
        Vector3 at = f != null ? f.transform.position : bottom.position;
        if (r.who != null) r.who.transform.position = at + Vector3.up * (0.12f + r.above);
        if (f != null) f.Enter();
        float secondsPerHour = TimeController.timeController != null ? TimeController.timeController.SecondsPerGameDay / 24f : 150f, left = RefugeRule.Hours * secondsPerHour;
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

    // POR EL POZO DE ESCAPE: a pie hasta su pie y en la jaula hasta el casquete; otro pozo, otro aire.
    IEnumerator EscapeUp(Rider r, Vector3 dn, float t0)
    {
        Escapes++;
        Vector3 foot = escapeBottom.position, head = escapeTop.position;
        float walk = Vector3.Distance(dn, foot) / SiteLineRule.WalkSpeed, w = 0f;
        while (w < walk)
        {
            w += Time.deltaTime;
            if (r.who != null) r.who.transform.position = Vector3.Lerp(dn, foot, w / walk) + Vector3.up * (0.12f + r.above);
            yield return null;
        }
        float rise = head.y - foot.y, T = SiteLineRule.Run(rise, SiteLineRule.EscapeSpeed, SiteLineRule.EscapeAccel);
        for (float e = 0f; e < T; e += Time.deltaTime)
        {
            Vector3 p = Vector3.Lerp(foot, head, SiteLineRule.Pos(rise, SiteLineRule.EscapeSpeed, SiteLineRule.EscapeAccel, e) / rise);
            if (cage != null) cage.position = p;
            if (r.who != null) r.who.transform.position = p + Vector3.up * (0.12f + r.above);
            yield return null;
        }
        if (cage != null) cage.position = _cageHome;
        if (car != null) car.position = transform.position;   // (el carro vuelve a su sitio cuando se repare; aquí, para no dejarlo a medias)
        Finish(r, head, "escape", t0);
    }

    IEnumerator Wait(Rider r, float seconds, Vector3 p, bool canAbort)
    {
        float w = 0f;
        while (w < seconds && !(canAbort && Recall)) { w += Time.deltaTime; Stand(r, p); yield return null; }
    }

    void Stand(Rider r, Vector3 p)
    {
        if (car != null) car.position = p;
        if (r.who == null) return;
        r.who.transform.position = p + Vector3.up * (0.12f + r.above);
        if (r.a != null) DailyPoses.Stand().ApplyFinal(r.a);
    }

    void Finish(Rider r, Vector3 at, string exit, float t0)
    {
        LastExit = exit; LastSeconds = Time.time - t0; Busy = false;
        FlyingCarpet.Carry(r.who, false);
        if (r.who == null) return;
        if (r.a != null) DailyPoses.Stand().ApplyFinal(r.a);
        r.who.transform.position = at + Vector3.up * (0.02f + r.above);
        if (r.cc != null) r.cc.enabled = r.ccWas;
        if (r.agent != null) { r.agent.enabled = r.agentWas; if (r.agent.enabled && r.agent.isOnNavMesh) r.agent.Warp(r.who.transform.position); }
        if (r.pc != null) r.pc.HoldBody(false);
        if (r.player && CameraManager.Instance != null) CameraManager.Instance.SwitchTo(CameraManager.Instance.preferredMode, forced: true);
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
