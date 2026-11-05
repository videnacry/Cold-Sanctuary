using System.Collections.Generic;
using UnityEngine;

/// <summary>Icebergs: cifras y umbrales (docs/sitio-nuclear.md §14). Los tabulares del Ross calan lo que mide el hielo. Remolcar sale bien el 85–95 % de las veces (Grand Banks).</summary>
public static class IcebergRule
{
    /// <summary>Deriva (m/s): 0,2 m/s son 30 m por día de juego.</summary>
    public const float Speed = 0.2f;
    /// <summary>Radios desde el centro del sitio (m): ALERTA, misión de remolque; PELIGRO, se desconecta la línea (el casquete y medio iceberg).</summary>
    public const float AlertRadius = 300f, DangerRadius = 190f;
    /// <summary>Lo que aguanta el escudo (toneladas): un trozo. Terra Nova desconecta a partir de 100 000 t.</summary>
    public const float ShieldTonnes = 100000f;
    public const float TowSuccess = 0.9f, IceDensity = 900f;
    /// <summary>Hasta dónde pueden llegar las quillas actuales (m): lo que se cubre con el escudo.</summary>
    public const float KeelBand = 400f;
    public const float MinFromBuildings = 250f;

    public static float Tonnes(Vector3 size) => size.x * size.y * size.z * IceDensity / 1000f;
    /// <summary>¿Sale bien el remolque del día? Fijo para cada día, para poder repetir la partida.</summary>
    public static bool TowWorks(int day) => unchecked((uint)day * 2654435761u >> 20) % 100u < (uint)(TowSuccess * 100f);
    public static float FlatDistance(Vector3 p) { p.y = 0f; Vector3 c = NuclearSiteRule.Center; c.y = 0f; return Vector3.Distance(p, c); }
}

/// <summary>
/// UN ICEBERG (Berón 2026-11-05; docs/sitio-nuclear.md §14): deriva despacio entre dos puntos, o está varado en un banco. Barato: unas pocas cajas, sin luces propias, y solo
/// se mueve una vez por fotograma. Alrededor, como en los estudios reales, aves, focas y kril. Si se acerca al sitio: ALERTA → misión de remolque (<c>nuc_remolque</c>);
/// PELIGRO → se desconecta la línea (<see cref="SiteLine.Disconnected"/>) hasta que se aleja.
/// </summary>
public class Iceberg : MonoBehaviour
{
    public static readonly List<Iceberg> All = new List<Iceberg>();
    public const string TowMission = "nuc_remolque";

    [Tooltip("Varado en un banco: no deriva.")] public bool grounded;
    [Tooltip("Extremos de la deriva (mundo). Empieza yendo hacia driftB.")] public Vector3 driftA, driftB;
    [Tooltip("Tamaño total (m): ancho, de arriba abajo, largo.")] public Vector3 size;
    [Tooltip("Las aves que vuelan alrededor: giran juntas.")] public Transform birds;
    [Tooltip("El kril que lo sigue.")] public Swarm krill;

    public int Tows { get; private set; }
    public int FailedTows { get; private set; }
    Vector3 _to;
    bool _started;
    float _nextKrill;

    public float Tonnes => IcebergRule.Tonnes(size);
    public float DistanceToSite => IcebergRule.FlatDistance(transform.position);
    bool Heading => !grounded && IcebergRule.FlatDistance(_to) < DistanceToSite;
    /// <summary>¿Viene hacia el sitio y ya está en alerta?</summary>
    public bool Threat => !grounded && Heading && DistanceToSite < IcebergRule.AlertRadius;
    public bool Danger => !grounded && DistanceToSite < IcebergRule.DangerRadius;

    void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void Update()
    {
        if (birds != null) birds.Rotate(0f, 12f * Time.deltaTime, 0f);
        if (grounded) return;
        if (!_started) { _started = true; if (_to == Vector3.zero) _to = driftB; }
        Vector3 p = Vector3.MoveTowards(transform.position, _to, IcebergRule.Speed * Time.deltaTime);
        transform.position = p;
        if ((p - _to).sqrMagnitude < 0.04f) _to = _to == driftB ? driftA : driftB;
        if (krill != null && Time.time > _nextKrill) { _nextKrill = Time.time + 20f; krill.GoTo(p + Vector3.down * 4f + transform.right * (size.x * 0.5f + 12f)); }
        SiteLine line = SiteLine.Instance;
        if (line != null) line.Disconnected = AnyDanger;
    }

    /// <summary>La prueba lo pone en un punto, yendo hacia <paramref name="to"/>.</summary>
    public void Place(Vector3 at, Vector3 to) { transform.position = at; _to = to; _started = true; }

    /// <summary>EL REMOLQUE (<c>nuc_remolque</c>): lo desvía de vuelta. Puede fallar: entonces mañana, otra vez, y más cerca.</summary>
    public bool Tow(string who, bool works)
    {
        Tows++;
        if (!works) { FailedTows++; Debug.LogWarning($"[Iceberg] {who} no consigue desviarlo: se rompe el cabo. Sigue a {DistanceToSite:0} m."); return false; }
        _to = IcebergRule.FlatDistance(driftA) > IcebergRule.FlatDistance(driftB) ? driftA : driftB;
        Debug.Log($"[Iceberg] {who} lo remolca: {Tonnes / 1e6f:0.0} Mt desviados, a {DistanceToSite:0} m del sitio.");
        return true;
    }

    public static Iceberg Threatening { get { foreach (Iceberg b in All) if (b != null && b.Threat) return b; return null; } }
    public static bool AnyDanger { get { foreach (Iceberg b in All) if (b != null && b.Danger) return true; return false; } }

    /// <summary>El efecto de la misión: remolca el que viene.</summary>
    public static void TowThreat(string who)
    {
        Iceberg b = Threatening;
        if (b == null) return;
        int day = TimeController.timeController != null ? TimeController.timeController.Day : 0;
        b.Tow(who, IcebergRule.TowWorks(day));
    }
}
