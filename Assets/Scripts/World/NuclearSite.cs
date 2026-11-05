using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA REGLA DEL SITIO NUCLEAR (Berón 2026-11-05; docs/sitio-nuclear.md): los dos reactores del santuario van en su propio casquete artificial,
/// lejos y a sotavento, en un árbol BAJO: lo que protege de un reactor es la distancia, la contención y el agua, no la altura.
/// - Distancia: ≥ <see cref="MinDistance"/> m a todo edificio e isla del santuario (lo más lejos que permite el mapa).
/// - Viento: el sitio queda a sotavento de <see cref="PrevailingWindTo"/> (el juego no simula viento: es una decisión de diseño fija).
/// - Altura: los núcleos, BAJO EL MAR (2026-11-05): cada reactor en su cajón sumergido de doble casco, bajo el casquete y por debajo de
///   <see cref="CaissonTopY"/>; arriba, solo lo que gana con ella: el agua por gravedad (el tronco), la chimenea filtrada y las salas de mando.
/// - Agua: el agua del tronco sobre los reactores alcanza, sin bombas ni operador, al menos <see cref="ReferenceHours"/> h.
/// </summary>
public static class NuclearSiteRule
{
    public const string Building = "Sitio Nuclear";
    /// <summary>Centro del casquete: la esquina sureste del mar (el mar llega a ±600 m).</summary>
    public static readonly Vector3 Center = new Vector3(510f, 0f, -510f);
    /// <summary>Distancia mínima (m), de tronco a tronco y a cualquier isla: lo más lejos que permite el mapa. Ninguna norma da esta cifra:
    /// la escala de reactores de investigación va del límite de operaciones (≤ 2 MW) a 800 m (hasta 50 MW).</summary>
    public const float MinDistance = 500f;
    /// <summary>Radio de la zona de exclusión (nadie vive ni trabaja sin dosímetro dentro): las boyas.</summary>
    public const float ExclusionRadius = 200f;
    /// <summary>Hacia dónde sopla el viento dominante (sureste, como los catabáticos que bajan al mar).</summary>
    public static readonly Vector3 PrevailingWindTo = new Vector3(1f, 0f, -1f).normalized;
    /// <summary>Cara del primer piso: los reactores. Excepción a <see cref="TreeSpec.FirstFloorMin"/>, que deja sitio a los árboles que crecen
    /// en el suelo: en el casquete no crece ninguno. Bajo el piso caben las tornapuntas y la puerta del suelo.</summary>
    public const float FirstFloorY = 12f;
    /// <summary>Hondo de la piscina del TRIGA bajo el piso: baja por el casquete hasta su cajón, con el núcleo bajo el nivel del mar. Los
    /// reales tienen 7,3–8,2 m de agua sobre el núcleo; aquí son casi 19 m: más blindaje.</summary>
    public const float PoolDepth = 20f;
    /// <summary>Cajones sumergidos: el techo (bajo el casquete) y su alto. Nivel del mar: y = 0.</summary>
    public const float CaissonTopY = -2f, CaissonHeight = 16f;
    /// <summary>Potencia térmica total (MW): el TRIGA (1) y el microrreactor (~15).</summary>
    public const float ThermalMW = 16f;
    /// <summary>La referencia pasiva: 72 h sin operador (el depósito por gravedad del AP1000).</summary>
    public const float ReferenceHours = 72f;

    /// <summary>Fracción de la potencia que sigue saliendo como calor residual <paramref name="seconds"/> s tras parar (Way–Wigner simplificada).</summary>
    public static float DecayFraction(float seconds) => 0.066f * Mathf.Pow(Mathf.Max(1f, seconds), -0.2f);

    /// <summary>Agua (m³) del tronco por encima de los reactores: la que baja sola por gravedad.</summary>
    public static float GravityWaterM3(float waterHalf, float waterTop) => 4f * waterHalf * waterHalf * Mathf.Max(0f, waterTop - FirstFloorY);

    /// <summary>Horas que <paramref name="m3"/> de agua absorben el calor residual de <see cref="ThermalMW"/> (calentándose de 4 a 100 °C y
    /// evaporándose la mitad). Es cota de orden de magnitud: se recorta a un año, Way–Wigner no vale para más.</summary>
    public static float PassiveHours(float m3)
    {
        const float rho = 1000f, c = 4186f, latent = 2.26e6f;
        double energy = rho * m3 * (c * 96f + 0.5f * latent), p0 = ThermalMW * 1e6;
        // ∫ P0·0,066·t^-0,2 dt = P0·0,066·t^0,8 / 0,8
        double t = System.Math.Pow(energy * 0.8 / (p0 * 0.066), 1.0 / 0.8);
        return (float)System.Math.Min(t / 3600.0, 8760.0);
    }

    /// <summary>¿<paramref name="p"/> está en la zona de exclusión? (nadie va allí en su tiempo libre, y de ahí se evacúa)</summary>
    public static bool InZone(Vector3 p) { p.y = Center.y; return Vector3.Distance(p, Center) < ExclusionRadius; }

    /// <summary>¿El sitio queda a sotavento del santuario? (el coseno entre el viento y la línea santuario → sitio)</summary>
    public static float DownwindCosine(Vector3 site) { site.y = 0f; return Vector3.Dot(site.normalized, PrevailingWindTo); }
}

/// <summary>
/// EL SITIO NUCLEAR en juego (docs/sitio-nuclear.md): vigilancia, válvulas pasivas que fallan abiertas y el plan de emergencia. Una EMERGENCIA
/// para el reactor de investigación, abre las válvulas (el agua del tronco baja sola) y enciende las balizas; el microrreactor se enfría
/// solo, sin nada que accionar. Mientras dure, quien esté en la zona de exclusión se va al hospital (<see cref="WorkdayDirector.EvacuateSite"/>) y
/// nadie entra. Un SIMULACRO prueba válvulas y balizas sin parar nada.
/// </summary>
public class NuclearSite : MonoBehaviour
{
    public enum Alert { Normal, Emergencia }

    public static NuclearSite Instance { get; private set; }
    public const string PlantArea = "Planta Atómica", PowerArea = "Central Nuclear";

    [Tooltip("Válvulas de la línea pasiva del tronco a la piscina: fallan abiertas.")]
    public List<Transform> valves = new List<Transform>();
    [Tooltip("Estaciones de vigilancia radiológica del borde del casquete.")]
    public List<Transform> monitors = new List<Transform>();
    [Tooltip("Sondas en el agua de mar alrededor del casquete: un cajón roto se vería aquí antes que en ningún otro sitio.")]
    public List<Transform> seaProbes = new List<Transform>();
    [Tooltip("Balizas de alarma (apagadas en normal).")]
    public List<GameObject> beacons = new List<GameObject>();
    [Tooltip("Agua (m³) del tronco por encima de los reactores.")]
    public float gravityWaterM3;

    public Alert State { get; private set; } = Alert.Normal;
    public bool ValvesOpen { get; private set; }
    public int Drills { get; private set; }
    public string LastReason { get; private set; } = "";
    /// <summary>Quién sigue en la zona de exclusión en esta evacuación (al último recuento).</summary>
    public int EvacueesLeft { get; private set; }
    /// <summary>¿Se está evacuando el sitio? (la jornada no manda a nadie allí)</summary>
    public static bool Evacuating => Instance != null && Instance.State == Alert.Emergencia;
    public static bool IsSiteArea(string area) => area == PlantArea || area == PowerArea;
    float _nextCount;

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }

    void Update()
    {
        if (State != Alert.Emergencia || Time.time < _nextCount) return;
        _nextCount = Time.time + 2f;
        EvacueesLeft = WorkdayDirector.Instance != null ? WorkdayDirector.Instance.EvacuateSite() : 0;
    }

    /// <summary>EMERGENCIA: para el reactor, abre las válvulas pasivas y enciende las balizas. No hace falta nadie más.</summary>
    public void DeclareEmergency(string reason)
    {
        State = Alert.Emergencia; LastReason = reason;
        ResearchReactor rr = ResearchReactor.Instance;
        if (rr != null && !rr.scrammed) rr.Scram("emergencia del sitio: " + reason);
        SetValves(true);
        SetBeacons(true);
        EvacueesLeft = WorkdayDirector.Instance != null ? WorkdayDirector.Instance.EvacuateSite() : 0;
        _nextCount = Time.time + 2f;
        Debug.LogWarning($"[Sitio nuclear] EMERGENCIA: {reason}. Reactor parado, válvulas abiertas: {gravityWaterM3:0} m³ de agua por gravedad ({NuclearSiteRule.PassiveHours(gravityWaterM3):0} h de calor residual). Se evacúa a {EvacueesLeft} persona(s) al hospital.");
    }

    /// <summary>Vuelve a normal (las válvulas se cierran a mano: el reactor no arranca solo).</summary>
    public void ClearEmergency()
    {
        State = Alert.Normal;
        SetValves(false);
        SetBeacons(false);
        Debug.Log("[Sitio nuclear] Fin de la emergencia.");
    }

    /// <summary>SIMULACRO: prueba válvulas y balizas sin parar el reactor.</summary>
    public void Drill(string who)
    {
        Drills++;
        SetValves(true); SetBeacons(true);
        SetValves(false); SetBeacons(false);
        Debug.Log($"[Sitio nuclear] {who} dirige el simulacro n.º {Drills}: válvulas y balizas probadas.");
    }

    void SetValves(bool open)
    {
        ValvesOpen = open;
        foreach (Transform v in valves) if (v != null) v.localRotation = Quaternion.Euler(0f, open ? 90f : 0f, 0f);
    }

    void SetBeacons(bool on) { foreach (GameObject b in beacons) if (b != null) b.SetActive(on); }

    /// <summary>Las misiones de la jornada en el sitio (si existe). El simulacro, una vez a la semana.</summary>
    public static List<WorkRoster.Mission> Missions(int day)
    {
        var l = new List<WorkRoster.Mission>();
        if (Object.FindAnyObjectByType<NuclearSite>() == null) return l;
        l.Add(new WorkRoster.Mission(PowerArea, "nuc_vigilancia", "Vigilancia radiológica: leer las estaciones del casquete, dosímetros y filtros de la chimenea", 1f));
        l.Add(new WorkRoster.Mission(PowerArea, "nuc_pasivos", "Revisar lo pasivo: válvulas del tronco a la piscina, toma de agua de mar y recinto de contención", 1f) { also = new[] { PlantArea } });
        if (((day % 7) + 7) % 7 == 0) l.Add(new WorkRoster.Mission(PowerArea, "nuc_simulacro", "Simulacro de emergencia: parada, válvulas, balizas y salida del turno", 1f));
        return l;
    }
}
