using System.Collections.Generic;
using UnityEngine;

/// <summary>Icebergs: cifras y umbrales (docs/sitio-nuclear.md §14). Tamaños de tabular real: en el santuario 1 la quilla solo se dibuja hasta el fondo somero, pero su masa cuenta entera (§20). Remolcar sale bien el 85–95 % de las veces (Grand Banks).</summary>
public static class IcebergRule
{
    /// <summary>Los sitios que se vigilan: los tres casquetes artificiales.</summary>
    public enum Site { Nuclear, Launch, Energy }
    public static readonly Site[] Sites = { Site.Nuclear, Site.Launch, Site.Energy };

    /// <summary>Deriva (m/s): 0,2 m/s son 30 m por día de juego.</summary>
    public const float Speed = 0.2f;
    /// <summary>Radios desde el centro de un sitio (m): PELIGRO, su casquete y medio iceberg con margen; ALERTA, 110 m antes (unos 4 días de deriva para remolcarlo).</summary>
    public const float DangerMargin = 105f, AlertMargin = 110f;
    /// <summary>Lo que aguanta el escudo (toneladas): un trozo. Terra Nova desconecta a partir de 100 000 t.</summary>
    public const float ShieldTonnes = 100000f;
    public const float TowSuccess = 0.9f, IceDensity = 900f;
    /// <summary>Hasta dónde pueden llegar las quillas actuales (m): lo que cubre el escudo; y dónde va el conector, por debajo: el muñón fijo queda fuera de su alcance.</summary>
    public const float KeelBand = 400f, ConnectorDepth = 420f;
    public const float MinFromBuildings = 250f;
    static readonly Vector3 LaunchCenter = new Vector3(-560f, 0f, 0f);   // (LaunchSiteCenter, en el constructor; la prueba comprueba que coincide)

    public static Vector3 Center(Site s) => s == Site.Nuclear ? NuclearSiteRule.Center : s == Site.Launch ? LaunchCenter : EnergyRule.Center;
    static float CapHalf(Site s) => s == Site.Nuclear ? 85f : s == Site.Launch ? 55f : EnergyRule.CapHalf;
    public static float DangerRadius(Site s) => CapHalf(s) + DangerMargin;
    public static float AlertRadius(Site s) => DangerRadius(s) + AlertMargin;

    public static float Tonnes(Vector3 size) => size.x * size.y * size.z * IceDensity / 1000f;
    /// <summary>¿Sale bien el remolque del día? Fijo para cada día, para poder repetir la partida.</summary>
    public static bool TowWorks(int day) => unchecked((uint)day * 2654435761u >> 20) % 100u < (uint)(TowSuccess * 100f);
    public static float FlatDistance(Vector3 p, Site s) { p.y = 0f; Vector3 c = Center(s); c.y = 0f; return Vector3.Distance(p, c); }
}

/// <summary>
/// UN ICEBERG (Berón 2026-11-05; docs/sitio-nuclear.md §14): deriva despacio entre dos puntos, o está varado en un banco. Barato: unas pocas cajas, sin luces propias, y solo
/// se mueve una vez por fotograma. Alrededor, como en los estudios reales, aves, kril, peces y animales de verdad (<see cref="IcebergVisitor"/>). Si se acerca a un casquete: ALERTA → misión de remolque de ese sitio; PELIGRO → en el
/// nuclear se desconecta la línea (<see cref="SiteLine.Disconnected"/>); en la lanzadera, vaciar los propulsores; en el parque, parar molinos y turbina.
/// </summary>
public class Iceberg : MonoBehaviour
{
    public static readonly List<Iceberg> All = new List<Iceberg>();
    public const string TowMission = "nuc_remolque", LaunchTow = "lanz_remolque", EnergyTow = "ene_remolque";

    [Tooltip("Varado en un banco: no deriva.")] public bool grounded;
    [Tooltip("Extremos de la deriva (mundo). Empieza yendo hacia driftB.")] public Vector3 driftA, driftB;
    [Tooltip("Tamaño total (m): ancho, de arriba abajo, largo.")] public Vector3 size;
    [Tooltip("El kril que lo sigue.")] public Swarm krill;
    [Tooltip("Los peces que lo siguen (docs/vida-en-icebergs.md).")] public Swarm fish;
    [Tooltip("El pie de hielo a ras de agua: lo único a lo que se puede subir.")] public Transform ledge;

    /// <summary>Dónde nada quien vive aquí: un corro alrededor, fuera del hielo (la quilla no tiene colisión).</summary>
    public float SwimRadius => Mathf.Max(size.x, size.z) * 0.75f + 10f;
    /// <summary>El punto del agua frente a la repisa: por ahí se sube.</summary>
    public Vector3 Approach => ledge != null ? ledge.position + (ledge.position - transform.position).normalized * 12f + Vector3.down : transform.position;
    /// <summary>Dónde se posa un ave: arriba, en el escalón del hielo. Se mueve con él.</summary>
    public Vector3 PerchSpot(float seed) => transform.TransformPoint(new Vector3(-size.x * 0.3f + Mathf.Repeat(seed * 7f, size.x * 0.25f), size.y * 0.1f + 0.1f, size.z * 0.3f));

    /// <summary>Un sitio en la repisa, propio de cada uno.</summary>
    public Vector3 HaulSpot(float seed)
    {
        if (ledge == null) return transform.position;
        float a = seed * Mathf.Deg2Rad;
        return ledge.position + new Vector3(Mathf.Cos(a) * size.x * 0.22f, 0.6f, Mathf.Sin(a) * 3f);
    }

    public int Tows { get; private set; }
    public int FailedTows { get; private set; }
    Vector3 _to;
    bool _started;
    float _nextKrill;

    public float Tonnes => IcebergRule.Tonnes(size);
    public float DistanceTo(IcebergRule.Site s) => IcebergRule.FlatDistance(transform.position, s);
    public float DistanceToSite => DistanceTo(IcebergRule.Site.Nuclear);
    /// <summary>¿Viene hacia ese sitio y ya está en alerta?</summary>
    public bool ThreatTo(IcebergRule.Site s) => !grounded && IcebergRule.FlatDistance(_to, s) < DistanceTo(s) && DistanceTo(s) < IcebergRule.AlertRadius(s);
    public bool DangerTo(IcebergRule.Site s) => !grounded && DistanceTo(s) < IcebergRule.DangerRadius(s);

    void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void Update()
    {
        if (grounded) return;
        if (!_started) { _started = true; if (_to == Vector3.zero) _to = driftB; }
        Vector3 p = Vector3.MoveTowards(transform.position, _to, IcebergRule.Speed * Time.deltaTime);
        transform.position = p;
        if ((p - _to).sqrMagnitude < 0.04f) _to = _to == driftB ? driftA : driftB;
        if (krill != null && Time.time > _nextKrill)
        {
            _nextKrill = Time.time + 20f;
            krill.GoTo(p + Vector3.down * 4f + transform.right * (size.x * 0.5f + 12f));
            if (fish != null) fish.GoTo(p + Vector3.down * 2f - transform.right * (size.x * 0.5f + 12f));
        }
        SiteLine line = SiteLine.Instance;
        if (line != null) line.Disconnected = Danger(IcebergRule.Site.Nuclear);
    }

    /// <summary>La prueba lo pone en un punto, yendo hacia <paramref name="to"/>.</summary>
    public void Place(Vector3 at, Vector3 to) { transform.position = at; _to = to; _started = true; }

    /// <summary>EL REMOLQUE: lo devuelve al extremo de su deriva más lejano del sitio. Puede fallar: entonces mañana, otra vez, y más cerca.</summary>
    public bool Tow(string who, bool works)
    {
        Tows++;
        if (!works) { FailedTows++; Debug.LogWarning($"[Iceberg] {who} no consigue desviarlo: se rompe el cabo. Sigue viniendo."); return false; }
        IcebergRule.Site near = IcebergRule.Site.Nuclear;
        foreach (IcebergRule.Site s in IcebergRule.Sites) if (DistanceTo(s) < DistanceTo(near)) near = s;
        _to = IcebergRule.FlatDistance(driftA, near) > IcebergRule.FlatDistance(driftB, near) ? driftA : driftB;
        Debug.Log($"[Iceberg] {who} lo remolca: {Tonnes / 1e6f:0.0} Mt desviados, a {DistanceTo(near):0} m de {near}.");
        return true;
    }

    public static Iceberg Threatening(IcebergRule.Site s) { foreach (Iceberg b in All) if (b != null && b.ThreatTo(s)) return b; return null; }
    public static bool Danger(IcebergRule.Site s) { foreach (Iceberg b in All) if (b != null && b.DangerTo(s)) return true; return false; }

    /// <summary>El efecto de la misión: remolca el que amenaza a ese sitio.</summary>
    public static void TowThreat(IcebergRule.Site s, string who)
    {
        Iceberg b = Threatening(s);
        if (b == null) return;
        int day = TimeController.timeController != null ? TimeController.timeController.Day : 0;
        b.Tow(who, IcebergRule.TowWorks(day));
    }
}
