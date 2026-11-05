using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>El Santuario Aéreo (docs/sitio-nuclear.md §23; docs/world-topology-and-planes.md §3): la lanzadera y el parque energético, en su propia escena y en sus coordenadas de siempre.</summary>
public static class AereoRule
{
    public const string Scene = "Santuario_Aereo", Building = "Santuario Aéreo";
    /// <summary>El vuelo del muelle a la plataforma (s): 30 s a las velocidades de la alfombra.</summary>
    public const float FlightSeconds = 30f;
    /// <summary>LA REGLA DEL CABLE: la energía cruza por un cable submarino, no por magia. Pérdida del 2 % en menos de 1 km (estimación mía).</summary>
    public const float CableEfficiency = 0.98f;
    /// <summary>Lo que piden los pisos de allí arriba (kW): 3 pisos de oficina, a 16 W/m² (estimación con la tabla de <see cref="EnergyRule"/>).</summary>
    public const float FloorsKW = 50f;
    /// <summary>Tope (s reales) para que acabe de cargar la escena. Si no carga, Kushal se queda en el muelle.</summary>
    public const float LoadLimit = 20f;
    /// <summary>Tope (s de juego) de la estancia de un trabajador: un día. Después vuelve, por lo que sea.</summary>
    public const float AwayLimit = 3600f;
    /// <summary>EL ASCENSOR DEL TRONCO (<see cref="AereoLift"/>): 10 m/s y 1,5 m/s² (valores del juego, la mitad de la línea de las cavernas), y sus puertas a cada lado (s).</summary>
    public const float LiftSpeed = 10f, LiftAccel = 1.5f, LiftDoorSeconds = 4f;
    /// <summary>Hasta dónde sube el trabajador de cada área (m), sin cargar la escena: lo más alto de sus pisos (la prueba 110 lo mide en la escena). La regla de altura puede subir la lanzadera: entonces, subir esto.</summary>
    public const float LaunchFloorsRise = 260f, EnergyFloorRise = 34f;
    public static float LiftSeconds(float rise) => SiteLineRule.Run(rise, LiftSpeed, LiftAccel) + 2f * LiftDoorSeconds;
    public static float LiftRise(string area) => area == EnergyRule.Area ? EnergyFloorRise : LaunchFloorsRise;
    /// <summary>El viaje de un trabajador, cada sentido: el vuelo y, si su área está allí, el ascensor.</summary>
    public static float TripSeconds(string area) => FlightSeconds + (IsArea(area) ? LiftSeconds(LiftRise(area)) : 0f);
    /// <summary>¿Esta área está allí? La misma lista arriba y abajo.</summary>
    public static bool IsArea(string area) => area == EnergyRule.Area || area == LaunchSite.ControlArea || area == LaunchSite.FactoryFloor;
    /// <summary>¿Este punto es de allí? (coordenadas de la lanzadera o del parque)</summary>
    public static bool InSector(Vector3 p) { foreach (IcebergRule.Site s in new[] { IcebergRule.Site.Launch, IcebergRule.Site.Energy }) if (IcebergRule.FlatDistance(p, s) < IcebergRule.DangerRadius(s)) return true; return false; }
}

/// <summary>
/// EL MUELLE AÉREO, en el santuario 1 (Berón 2026-11-05: «un vuelo a esas áreas ... también pasaría a una carga de escena»). Se llega en alfombra; es una pista más de la red del santuario.
/// Los trabajadores vuelan SIN CARGAR NADA: su cuerpo se oculta y su misión corre por el reloj, con la misma lógica, hasta que su jornada los lleva a otra parte. Fernando pasa aquí casi todo el día.
/// Kushal va en persona: la escena se carga aditiva y al volver se descarga. Nada de aquí apunta a esa escena: cruza por <see cref="AereoBase.Instance"/>.
/// </summary>
public class AereoLink : MonoBehaviour
{
    const float BoardRadius = 12f;
    public static AereoLink Instance { get; private set; }

    readonly Dictionary<GameObject, string> _wanted = new Dictionary<GameObject, string>();
    readonly HashSet<GameObject> _away = new HashSet<GameObject>();
    /// <summary>¿Cuántos trabajadores hay allí ahora?</summary>
    public int Away => _away.Count;
    public int Flights { get; private set; }
    /// <summary>¿Está Kushal allí? La Magnate no lo devuelve: tiene su salida.</summary>
    public static bool PlayerThere { get; private set; }
    public bool Busy { get; private set; }

    void OnEnable()
    {
        Instance = this;
        WorkdayDirector.RegisterPlace(EnergyRule.Area, transform);
        WorkdayDirector.RegisterPlace(LaunchSite.ControlArea, transform);
        WorkdayDirector.RegisterPlace(LaunchSite.FactoryFloor, transform);
    }
    void OnDisable() { if (Instance == this) Instance = null; }

    /// <summary>La jornada dice adónde va hoy cada uno: si deja de ser allí, vuelve.</summary>
    public static void Note(GameObject g, string area) { if (Instance != null && g != null) Instance._wanted[g] = area; }
    public static bool IsAway(GameObject g) => Instance != null && g != null && Instance._away.Contains(g);

    /// <summary>La jornada: quien tiene trabajo allí y ya está en el muelle, vuela. true = se ha ido.</summary>
    public static bool Board(GameObject g)
    {
        AereoLink l = Instance;
        if (l == null || g == null || FlyingCarpet.Riding(g)) return false;
        if ((g.transform.position - l.transform.position).sqrMagnitude > BoardRadius * BoardRadius) return false;
        l.StartCoroutine(l.Fly(g));
        return true;
    }

    // UN TRABAJADOR: oculto mientras dure su trabajo allí; su misión corre por el reloj de la jornada.
    IEnumerator Fly(GameObject g)
    {
        _away.Add(g); Flights++;
        FlyingCarpet.Carry(g, true);
        var agent = g.GetComponent<UnityEngine.AI.NavMeshAgent>();
        bool agentWas = agent != null && agent.enabled;
        if (agent != null) agent.enabled = false;
        var off = new List<Renderer>();
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>()) if (r.enabled) { r.enabled = false; off.Add(r); }
        float t0 = Time.time, trip = AereoRule.TripSeconds(_wanted.TryGetValue(g, out string there) ? there : null);   // el vuelo y el ascensor, como número: la escena no se carga
        yield return new WaitForSeconds(trip);
        while (g != null && Time.time - t0 < AereoRule.AwayLimit && _wanted.TryGetValue(g, out string area) && AereoRule.IsArea(area)) yield return null;
        yield return new WaitForSeconds(trip);
        foreach (Renderer r in off) if (r != null) r.enabled = true;
        _away.Remove(g);
        FlyingCarpet.Carry(g, false);
        if (g == null) yield break;
        g.transform.position = transform.position + Vector3.up;
        if (agent != null) { agent.enabled = agentWas; if (agent.enabled && agent.isOnNavMesh) agent.Warp(g.transform.position); }
    }

    // KUSHAL, en persona.
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || Busy || FlyingCarpet.Riding(other.gameObject)) return;
        GameObject p = other.gameObject;
        ChoiceDialog.Show($"{AereoRule.Building}: {AereoRule.FlightSeconds:0} s de vuelo", new[] { "Volar a la lanzadera", "Volar al parque energético", "Quedarse" },
            i => { if (i < 2) StartCoroutine(Go(p, i == 0)); }, null, transform, 5f);
    }

    public static IEnumerator Black()
    {
        bool done = false; float w = 0f;
        ScreenFader.Get().FadeToBlack(() => done = true);
        while (!done && w < 5f) { w += Time.unscaledDeltaTime; yield return null; }
    }

    public static void Put(GameObject p, Vector3 at)
    {
        CharacterController cc = p.GetComponent<CharacterController>();
        bool was = cc != null && cc.enabled;
        if (cc != null) cc.enabled = false;
        p.transform.position = at + Vector3.up * 1.1f;
        Physics.SyncTransforms();
        if (cc != null) cc.enabled = was;
    }

    IEnumerator Go(GameObject p, bool launch)
    {
        Busy = true;
        PlayerController pc = p.GetComponent<PlayerController>();
        if (pc != null) pc.HoldBody(true);
        yield return Black();
        AsyncOperation op = null;
        if (!SceneManager.GetSceneByName(AereoRule.Scene).isLoaded)
        {
            if (Application.CanStreamedLevelBeLoaded(AereoRule.Scene)) op = SceneManager.LoadSceneAsync(AereoRule.Scene, LoadSceneMode.Additive);
            else Debug.LogWarning($"[Aéreo] Falta {AereoRule.Scene} en Build Settings: Tools/Cold Sanctuary/Build Santuario Aéreo.");
        }
        float w = 0f;
        while (w < AereoRule.FlightSeconds || (op != null && !op.isDone && w < AereoRule.FlightSeconds + AereoRule.LoadLimit)) { w += Time.unscaledDeltaTime; yield return null; }
        AereoBase b = AereoBase.Instance;
        if (b != null) { Put(p, (launch ? b.launchArrival : b.energyArrival).position); PlayerThere = true; }
        else Debug.LogWarning("[Aéreo] La escena no ha cargado a tiempo: Kushal se queda en el muelle.");
        if (pc != null) pc.HoldBody(false);
        ScreenFader.Get().FadeFromBlack();
        Busy = false;
    }

    /// <summary>Volver al muelle (lo pide el andén de allí).</summary>
    public void Return(GameObject p) { if (!Busy) StartCoroutine(Back(p)); }

    IEnumerator Back(GameObject p)
    {
        Busy = true;
        PlayerController pc = p.GetComponent<PlayerController>();
        if (pc != null) pc.HoldBody(true);
        yield return Black();
        float w = 0f;
        while (w < AereoRule.FlightSeconds) { w += Time.unscaledDeltaTime; yield return null; }
        Put(p, transform.position);
        PlayerThere = false;
        AsyncOperation op = SceneManager.GetSceneByName(AereoRule.Scene).isLoaded ? SceneManager.UnloadSceneAsync(AereoRule.Scene) : null;
        float u = 0f;
        while (op != null && !op.isDone && u < 20f) { u += Time.unscaledDeltaTime; yield return null; }
        if (pc != null) pc.HoldBody(false);
        ScreenFader.Get().FadeFromBlack();
        Busy = false;
    }

    /// <summary>(Pruebas) Carga la escena, sin mover a nadie. Espera acotada.</summary>
    public static IEnumerator LoadForTest()
    {
        if (SceneManager.GetSceneByName(AereoRule.Scene).isLoaded || !Application.CanStreamedLevelBeLoaded(AereoRule.Scene)) yield break;
        AsyncOperation op = SceneManager.LoadSceneAsync(AereoRule.Scene, LoadSceneMode.Additive);
        float r0 = Time.realtimeSinceStartup;
        while (op != null && !op.isDone && Time.realtimeSinceStartup - r0 < 30f) yield return null;
    }

    public static IEnumerator UnloadForTest()
    {
        if (!SceneManager.GetSceneByName(AereoRule.Scene).isLoaded) yield break;
        AsyncOperation op = SceneManager.UnloadSceneAsync(AereoRule.Scene);
        float r0 = Time.realtimeSinceStartup;
        while (op != null && !op.isDone && Time.realtimeSinceStartup - r0 < 30f) yield return null;
    }
}
