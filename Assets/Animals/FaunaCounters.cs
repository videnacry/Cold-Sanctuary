using UnityEngine;

/// <summary>
/// (2026-11-13, ronda 82) LOS CONTADORES DE LA FAUNA, EN UNA LÍNEA CADA MINUTO. Eran campos estáticos que nadie imprimía (StaleRepaths, BadPaths,
/// HomeLegs… salían «0 líneas» para el probador). Una línea «[Fauna] contadores: …» por minuto de juego real, solo si alguno cambió.
/// </summary>
public class FaunaCounters : MonoBehaviour
{
    const float Every = 60f;
    float _next;
    string _last;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (FindAnyObjectByType<FaunaCounters>() != null) return;
        var g = new GameObject("FaunaCounters") { hideFlags = HideFlags.HideInHierarchy };
        DontDestroyOnLoad(g);
        g.AddComponent<FaunaCounters>();
    }

    /// <summary>La línea de contadores (también para las pruebas).</summary>
    public static string Line() =>
        $"[Fauna] contadores: StaleRepaths={DesireCatalog.StaleRepaths} NavReach.BadPaths={NavReach.BadPaths} HomeLegs={DesireCatalog.HomeLegs} " +
        $"EdgeNudges={DesireCatalog.EdgeNudges} FrozenRestarts={DesireCatalog.FrozenRestarts} FrozenTrips={DesireCatalog.FrozenTrips} " +
        $"BadPaths(viajes)={DesireCatalog.BadPaths} NoPathTrips={DesireCatalog.NoPathTrips} IdleRefused={ActionPrep.IdleRefused} " +
        $"PenWalker={PenWalker.Started}/{PenWalker.Arrived}/{PenWalker.Gave} (empezados/llegados/sin sitio)";

    void Update()
    {
        if (Time.unscaledTime < _next) return;
        _next = Time.unscaledTime + Every;
        string l = Line();
        if (l == _last) return;
        _last = l;
        Debug.Log(l);
    }
}
