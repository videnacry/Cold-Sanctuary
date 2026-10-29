using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

/// <summary>
/// EL ÁRBOL GIRA (docs/edificio-arbol.md; Berón 2026-10-29): toda la copa, el tronco y las ramas giran sobre la raíz en
/// sentido CONTRARIO al sol (en el santuario el sol da la vuelta al horizonte cada día), por defecto UNA VUELTA CADA 6 HORAS
/// del juego (un día del juego = 1 hora real → 15 minutos reales por vuelta). Así la sombra de las ramas y los anillos no
/// cae siempre sobre los mismos cultivos, y el viento y la nieve se reparten. Hechizo <c>arbol({vueltaHoras:N})</c>
/// (0 = quieto).
/// - Quien está DE PIE en un piso, un anillo o la copa gira con él (se le aplica el mismo giro: no resbala).
/// - El NavMesh del árbol (su propio NavMeshSurface, horneado en sus coordenadas) se vuelve a colocar con el árbol cada
///   <see cref="navRefresh"/> s.
/// </summary>
[DefaultExecutionOrder(-40)]
public class TreeRotator : MonoBehaviour
{
    public static TreeRotator Instance { get; private set; }
    [Tooltip("Horas del juego por vuelta (0 = quieto).")]
    public float hoursPerTurn = 6f;
    [Tooltip("Cada cuánto (s) se recoloca el NavMesh del árbol.")]
    public float navRefresh = 0.25f;

    /// <summary>Grados girados desde que empezó (con signo).</summary>
    public float TotalDegrees { get; private set; }
    public int NavRefreshes { get; private set; }
    public int Carried { get; private set; }
    /// <summary>+1 o −1: el sentido del giro (contrario al del sol en el cielo).</summary>
    public float Direction { get; private set; } = 1f;

    NavMeshSurface _surface;
    float _navAt, _scanAt, _dirAt;
    bool _warned;
    Quaternion _navRot;
    readonly List<Transform> _bodies = new List<Transform>();
    readonly Collider[] _overlap = new Collider[2048];

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }
    void Start() { _surface = GetComponent<NavMeshSurface>(); _navRot = transform.rotation; }

    /// <summary>Cambia la velocidad (hechizo «arbol»): horas del juego por vuelta; 0 lo para.</summary>
    public static string Set(float hours)
    {
        if (Instance == null) return "No hay Árbol de los Campos en esta escena.";
        Instance.hoursPerTurn = Mathf.Max(0f, hours);
        return hours <= 0f ? "El Árbol de los Campos se queda quieto." : $"El Árbol de los Campos da una vuelta cada {hours:0.##} h del juego, al revés que el sol.";
    }

    /// <summary>Grados por segundo (del juego, Time.deltaTime) a la velocidad actual.</summary>
    public float DegreesPerSecond
    {
        get
        {
            if (hoursPerTurn <= 0f) return 0f;
            float secPerHour = 60f * Mathf.Max(0.001f, TimeController.timeController.TimeSpeedMinuteSecs);
            return Direction * 360f / (hoursPerTurn * secPerHour);
        }
    }

    // El sol en el cielo del santuario: su acimut (brújula, como el giro en Y de Unity visto desde arriba) avanza en un
    // sentido; el árbol, en el otro.
    void SunDirection()
    {
        TimeController tc = TimeController.timeController;
        float doy = SolarPosition.SanctuaryDayOfYear(tc), h = tc.Hour;
        float a0 = SolarPosition.SunFor(SolarPosition.SanctuaryLat, doy, h).azimuth, a1 = SolarPosition.SunFor(SolarPosition.SanctuaryLat, doy, h + 0.25f).azimuth;
        float d = Mathf.DeltaAngle(a0, a1);
        if (Mathf.Abs(d) > 0.01f) Direction = -Mathf.Sign(d);
    }

    void Update()
    {
        if (Time.time >= _dirAt) { _dirAt = Time.time + 30f; SunDirection(); }
        float dps = DegreesPerSecond;
        if (Mathf.Abs(dps) > 1e-6f && Time.deltaTime > 0f)
        {
            float deg = dps * Time.deltaTime;
            Quaternion dq = Quaternion.AngleAxis(deg, Vector3.up);
            Vector3 pivot = transform.position;
            transform.rotation = dq * transform.rotation;
            TotalDegrees += deg;
            Physics.SyncTransforms();
            Carry(dq, pivot);
        }
        if (_surface != null && Time.time >= _navAt)
        {
            _navAt = Time.time + navRefresh;
            if (Quaternion.Angle(_navRot, transform.rotation) > 0.05f)
            {
                _surface.RemoveData(); _surface.AddData();
                _navRot = transform.rotation; NavRefreshes++;
            }
        }
    }

    // Los cuerpos de pie sobre el árbol giran con él: cada 0,5 s se buscan los cuerpos cerca (jugador, personajes,
    // animales); cada frame, si lo que tienen bajo los pies es del árbol, se les aplica el giro.
    void Carry(Quaternion dq, Vector3 pivot)
    {
        if (Time.time >= _scanAt)
        {
            _scanAt = Time.time + 0.5f;
            _bodies.Clear();
            int n = Physics.OverlapCapsuleNonAlloc(pivot + Vector3.down * 2f, pivot + Vector3.up * (TreeSpec.RootHeight + 6f), 56f, _overlap, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                Collider c = _overlap[i];
                if (c == null || c.transform.IsChildOf(transform)) continue;
                Transform body = c.GetComponent<CharacterController>() != null ? c.transform
                    : c.GetComponentInParent<Anima>() is Anima a ? a.transform
                    : c.GetComponentInParent<TreeRider>() is TreeRider tr ? tr.transform : null;
                if (body != null && !_bodies.Contains(body)) _bodies.Add(body);
            }
            if (n >= _overlap.Length && !_warned) { _warned = true; Debug.LogWarning("[Árbol] Demasiados colliders cerca del árbol: puede que alguien no gire con él."); }
            GameObject pl = GameObject.FindGameObjectWithTag("Player");   // el jugador, siempre
            if (pl != null && !_bodies.Contains(pl.transform)) _bodies.Add(pl.transform);
        }
        foreach (Transform b in _bodies)
        {
            if (b == null || TreeTrip.Busy(b.gameObject) || FlyingCarpet.Riding(b.gameObject) || SwimBody.Swimming(b.gameObject)) continue;
            if (!OnTree(b)) continue;
            Vector3 np = pivot + dq * (b.position - pivot), delta = np - b.position;
            CharacterController cc = b.GetComponent<CharacterController>();
            if (cc != null && cc.enabled) cc.Move(delta); else b.position = np;
            b.rotation = dq * b.rotation;
            Carried++;
        }
    }

    bool OnTree(Transform b)
    {
        Vector3 o = b.position + Vector3.up * 0.3f;
        float best = float.MaxValue; Transform hitT = null;
        foreach (RaycastHit h in Physics.RaycastAll(o, Vector3.down, 2.8f, ~0, QueryTriggerInteraction.Ignore))
            if (!h.transform.IsChildOf(b) && h.distance < best) { best = h.distance; hitT = h.transform; }
        return hitT != null && hitT.IsChildOf(transform);
    }
}

/// <summary>Marca para que algo sin Anima ni CharacterController (un cuerpo de prueba, un objeto) gire con el árbol si está
/// encima de él.</summary>
public class TreeRider : MonoBehaviour { }
