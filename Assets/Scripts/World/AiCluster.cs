using UnityEngine;

/// <summary>
/// EL CLÚSTER DE IA LOCAL DEL SANTUARIO (piso de Informática, torre de Ciencias; docs/piso-informatica.md; Berón 2026-11-04:
/// «servidores de punta y demás equipo para tener una IA local del santuario … requiere mucha agua o energía? se debe crear con
/// sus mecanismos de abastecimiento»). Un modelo sencillo, con cifras reales a escala, de lo que pide un clúster de GPU:
/// - CÓMPUTO: <see cref="racks"/> racks de <see cref="serversPerRack"/> servidores de 8 GPU (~10,2 kW cada uno a plena carga,
///   como un DGX H100), más red y almacenamiento. ~90 kW de TI: un 2 % de la central nuclear (5 MWe).
/// - REFRIGERACIÓN LÍQUIDA directa al chip en CIRCUITO CERRADO con agua templada (entra a ~35 °C, sale a ~50 °C): sin torres de
///   evaporación, así que casi no gasta agua (WUE ≈ 0; la media de la industria con torres ronda 1,8 L/kWh). El
///   <see cref="fouling"/> (filtros sucios, placas del intercambiador con depósitos) sube con los días y calienta las GPU: si
///   pasan de <see cref="ThrottleC"/> °C bajan la potencia. La misión de mantenimiento lo limpia.
/// - EL CALOR NO SE TIRA: el agua caliente pasa por un intercambiador de placas a la RED DE CALOR DE DISTRITO (la de la central,
///   que ya templa los fotobiorreactores, que quieren 30–35 °C, y los frutales de los techos); lo que sobra va al intercambiador
///   de AGUA DE MAR (el foco frío del reactor de investigación).
/// - ENERGÍA: acometida de la central nuclear (y los paneles solares de día); un SAI con baterías de litio aguanta la carga
///   <see cref="UpsMinutes"/> min si cae la red. No descuenta del recurso Energía del santuario (otra escala, la de la economía).
/// - AGUA de reposición del circuito: del TRONCO de agua de la torre (Torre_Ciencias), a un depósito.
/// Las misiones de Informática (AreaFunctions: «ifo_sysadmin», «ifo_refrigeracion», «ifo_energia») llaman a
/// <see cref="Sysadmin"/>, <see cref="CoolingService"/> y <see cref="PowerCheck"/>.
/// </summary>
public class AiCluster : MonoBehaviour
{
    public static AiCluster Instance { get; private set; }

    [Header("Cómputo")]
    public int racks = 2, serversPerRack = 4, gpusPerServer = 8;
    [Tooltip("kW de un servidor de 8 GPU a plena carga (NVIDIA DGX H100: 10,2 kW máx.).")]
    public float serverKW = 10.2f;
    [Tooltip("kW de la red (InfiniBand/Ethernet) y del almacenamiento.")]
    public float networkStorageKW = 6f;
    [Range(0f, 1f)] public float utilization = 0.6f;
    [Range(0f, 1f)] public float health = 1f;

    [Header("Refrigeración (circuito cerrado de agua templada)")]
    [Range(0f, 1f), Tooltip("Parte del calor que se lleva el agua (directo al chip); el resto, el intercambiador de puerta trasera.")]
    public float liquidFraction = 0.85f;
    public float supplyC = 35f, returnC = 50f;
    [Range(0f, 1f), Tooltip("Suciedad de filtros y placas: sube cada día y calienta las GPU.")]
    public float fouling = 0f;
    public float foulingPerDay = 0.08f;
    public const float ThrottleC = 85f;
    [Tooltip("Litros del circuito cerrado.")]
    public float loopLiters = 1500f;

    [Header("Calor que se aprovecha")]
    [Tooltip("kW de calor que pide la red de distrito (fotobiorreactores y frutales de los techos) a esta temperatura.")]
    public float districtDemandKW = 120f;

    [Header("Energía")]
    public float gridKW = 5000f;   // la central nuclear (5 MWe)
    public float upsKWh = 60f;
    public const float UpsMinutes = 40f;

    // Estado y contadores (los lee el puesto de monitorización y la prueba).
    public float ItKW { get; private set; }
    public float TotalKW { get; private set; }
    public float Pue => ItKW > 0.01f ? TotalKW / ItKW : 0f;
    public float GpuTempC { get; private set; }
    public float HeatToDistrictKW { get; private set; }
    public float HeatToSeaKW { get; private set; }
    public float BatteryKWh { get; private set; }
    public bool GridUp { get; private set; } = true;
    public float ItEnergyKWh { get; private set; }
    public float RecoveredKWh { get; private set; }
    public float MakeupLiters { get; private set; }
    public float Wue => ItEnergyKWh > 0.01f ? MakeupLiters / ItEnergyKWh : 0f;
    public bool Throttled => GpuTempC > ThrottleC;
    public int Gpus => racks * serversPerRack * gpusPerServer;
    public string LastAction { get; private set; } = "";

    [Tooltip("Lo que se ve encima del puesto de monitorización (lo pone el constructor).")]
    public WorldLabel monitor;
    float _labelT;

    void Awake() { BatteryKWh = upsKWh; }
    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }

    void Update() => Step(Time.deltaTime * 24f / 3600f);   // un día de juego = una hora real

    /// <summary>Avanza el modelo <paramref name="hours"/> horas de juego (público para la prueba).</summary>
    public void Step(float hours)
    {
        if (hours <= 0f) return;
        fouling = Mathf.Clamp01(fouling + foulingPerDay * hours / 24f);
        health = Mathf.Clamp01(health - 0.05f * hours / 24f);   // colas atascadas, copias por revisar, alertas sin atender
        // Calor en las GPU: el agua entra a supplyC; con los filtros sucios pasa menos caudal y sube la temperatura.
        GpuTempC = 55f + (supplyC - 35f) + 40f * fouling;
        float load = Mathf.Lerp(0.3f, 1f, utilization) * Mathf.Lerp(0.6f, 1f, health) * (Throttled ? 0.7f : 1f);
        // Energía: la acometida de la central (si está en la escena) o, si cae, el SAI.
        GridUp = gridKW > 0f;
        float servers = racks * serversPerRack;
        ItKW = servers * serverKW * load + networkStorageKW;
        float coolingKW = ItKW * (0.04f + 0.06f * fouling);   // bombas, CDU y ventiladores del intercambiador de puerta trasera
        float lossesKW = 0.03f * (ItKW + coolingKW);           // SAI y distribución
        TotalKW = ItKW + coolingKW + lossesKW;
        if (!GridUp)
        {
            BatteryKWh = Mathf.Max(0f, BatteryKWh - TotalKW * hours);
            if (BatteryKWh <= 0f) { ItKW = 0f; TotalKW = 0f; }   // sin red ni batería: el clúster se apaga ordenadamente
        }
        else BatteryKWh = Mathf.Min(upsKWh, BatteryKWh + 0.25f * upsKWh * hours);   // recarga en ~4 h
        // Calor: el que se lleva el agua va primero a la red de distrito; lo que sobra, al mar.
        float waterHeat = ItKW * liquidFraction;
        HeatToDistrictKW = Mathf.Min(waterHeat, districtDemandKW);
        HeatToSeaKW = waterHeat - HeatToDistrictKW;
        // Agua: circuito cerrado; solo se repone lo que se pierde (purgas, muestreo, el vaso de expansión): ~0,5 % del circuito al
        // mes, más si está sucio (se purga más).
        MakeupLiters += loopLiters * (0.005f / 720f) * (1f + 4f * fouling) * hours;
        ItEnergyKWh += ItKW * hours;
        RecoveredKWh += HeatToDistrictKW * hours;
        _labelT -= Time.deltaTime;
        if (monitor != null && _labelT <= 0f) { _labelT = 2f; monitor.text = Summary(); }
    }

    public string Summary() =>
        $"Clúster de IA · {Gpus} GPU · TI {ItKW:0} kW · total {TotalKW:0} kW · PUE {Pue:0.00} · GPU {GpuTempC:0} °C{(Throttled ? " (¡baja potencia!)" : "")}" +
        $" · calor al distrito {HeatToDistrictKW:0} kW, al mar {HeatToSeaKW:0} kW · agua repuesta {MakeupLiters:0.0} L (WUE {Wue:0.000} L/kWh)" +
        $" · SAI {BatteryKWh:0}/{upsKWh:0} kWh{(GridUp ? "" : " (SIN RED)")} · salud {health * 100f:0}%";

    // ── Lo que hacen las misiones ─────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>«ifo_sysadmin»: vaciar colas, revisar las copias y las alertas, aplicar parches.</summary>
    public void Sysadmin(string who) { health = 1f; LastAction = $"{who}: colas, copias y alertas al día"; }
    /// <summary>«ifo_refrigeracion»: limpiar filtros, mirar fugas y bombas, desincrustar el intercambiador.</summary>
    public void CoolingService(string who) { fouling = 0f; LastAction = $"{who}: refrigeración limpia (filtros, fugas, bombas)"; }
    /// <summary>«ifo_energia»: probar el SAI y la acometida; si la red estaba caída, la repone.</summary>
    public void PowerCheck(string who) { if (gridKW <= 0f) gridKW = 5000f; BatteryKWh = upsKWh; LastAction = $"{who}: acometida y SAI revisados"; }

    /// <summary>Simula un corte de la acometida (para la prueba y para un evento del juego).</summary>
    public void CutGrid() => gridKW = 0f;
}
