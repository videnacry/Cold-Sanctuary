using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// (2026-11-11) ¿QUIÉN LE QUITA EL CAMINO A UN ANIMAL? Ronda 55: «se queda sin camino hacia … (12 m): camino False … destino a 0,0 m»
/// justo después de cada SetPath/SetDestination de un paseo, 200 «[Fauna] atascado» en 2 días. Todas las conductas del animal que
/// le paran o le cambian el destino pasan por aquí (<see cref="Reset"/>, <see cref="Dest"/>): se apunta la última por animal (la dice
/// el aviso de «se queda sin camino») y, la PRIMERA vez que cada sitio del código lo hace con un viaje de deseo en marcha, se escribe
/// su pila una vez («[Fauna] NavTrace»).
/// </summary>
public static class NavTrace
{
    static readonly Dictionary<Animal, (string why, float at, Vector3 to)> _last = new Dictionary<Animal, (string, float, Vector3)>();
    static readonly HashSet<string> _stackLogged = new HashSet<string>();

    // (2026-11-14, ronda 108) UN CAMINO NUEVO NO EN CADA FOTOGRAMA. Con los fotogramas lentos de las pruebas (2–4 s de juego) un agente tarda 1–2
    // fotogramas en echar a andar tras un camino nuevo; quien le ponía uno cada segundo (perseguir, pelear, el hábito, ir hacia la pareja, la
    // huida, el pasto) le ponía uno por fotograma y no andaba nunca (renos #8 y #67). Regla común a todas las conductas:
    // - si ya lleva (o calcula) un camino que acaba a < KeepWithin m del sitio nuevo, se deja;
    // - si se le puso uno hace < RepathSeconds s o < RepathFrames fotogramas, también (solo para quien lo pide sin parar: Dest).
    public const float KeepWithin = 3f, RepathSeconds = 1f;
    public const int RepathFrames = 3;
    public static int KeptPaths;
    static readonly Dictionary<NavMeshAgent, (float t, int f)> _pathed = new Dictionary<NavMeshAgent, (float, int)>();
    /// <summary>¿Se le deja el camino que lleva en vez de ponerle uno hacia <paramref name="to"/>? <paramref name="rateLimit"/>: también si se le
    /// puso uno hace muy poco.</summary>
    public static bool KeepPath(NavMeshAgent nav, Vector3 to, bool rateLimit)
    {
        if (nav == null || !nav.isOnNavMesh || !(nav.hasPath || nav.pathPending)) return false;
        Vector3 d = nav.destination - to; d.y = 0f;
        if (d.magnitude < KeepWithin) { KeptPaths++; return true; }
        if (rateLimit && _pathed.TryGetValue(nav, out var l) && (Time.time - l.t < RepathSeconds || Time.frameCount - l.f < RepathFrames)) { KeptPaths++; return true; }
        return false;
    }
    /// <summary>Apunta que a este agente se le acaba de poner un camino.</summary>
    public static void Pathed(NavMeshAgent nav) { if (nav != null) _pathed[nav] = (Time.time, Time.frameCount); }

    /// <summary>ResetPath con su porqué.</summary>
    public static void Reset(Animal a, string why)
    {
        NavMeshAgent nav = a != null ? a.nav : null;
        if (nav == null || !nav.enabled || !nav.isOnNavMesh) return;
        Note(a, why, a.transform.position);
        nav.ResetPath();
    }

    /// <summary>SetDestination con su porqué.</summary>
    public static bool Dest(Animal a, NavMeshAgent nav, Vector3 to, string why)
    {
        if (nav == null || !nav.enabled || !nav.isOnNavMesh) return false;
        if (KeepPath(nav, to, true)) return true;   // (ronda 108) mismo sitio, o camino recién puesto: se deja
        Pathed(nav);
        if (a != null) Note(a, why, to);
        return nav.SetDestination(to);
    }

    static void Note(Animal a, string why, Vector3 to)
    {
        if (a == null) return;
        _last[a] = (why, Time.time, to);
        if (DesireCatalog.LiveTrip(a, out string key) && !why.StartsWith("deseo") && _stackLogged.Add(why))
            Debug.Log($"[Fauna] NavTrace: «{a.name}» lleva un viaje «{key}» y «{why}» le cambia el camino (hacia ({to.x:0.0},{to.y:0.0},{to.z:0.0})). Pila: {System.Environment.StackTrace.Replace(System.Environment.NewLine, " / ")}");
    }

    /// <summary>Solo lo apunta (un SetPath u otro cambio que hace quien llama).</summary>
    public static void Mark(Animal a, string why, Vector3 to) => Note(a, why, to);

    /// <summary>La última conducta que le paró o le cambió el destino (para los avisos).</summary>
    public static string Last(Animal a) => a != null && _last.TryGetValue(a, out var l) ? $"último cambio: «{l.why}» hace {Time.time - l.at:0.0} s hacia ({l.to.x:0.0},{l.to.y:0.0},{l.to.z:0.0})" : "sin cambios registrados";
}
