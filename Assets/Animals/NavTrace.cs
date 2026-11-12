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
