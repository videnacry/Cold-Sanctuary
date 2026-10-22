using UnityEngine;

/// <summary>
/// EL REACTOR DE LA PLANTA ATÓMICA (docs/atomic-plant.md): un reactor de investigación de PISCINA tipo TRIGA, como los
/// de las universidades (UT-Austin 1,1 MW, Penn State 1 MW). No da electricidad (el santuario es solar): da NEUTRONES,
/// para fabricar isótopos médicos para el hospital (el más cercano está a miles de km), analizar muestras por
/// activación y enseñar física. La física es de verdad, simplificada:
/// - CINÉTICA PUNTUAL con un grupo de neutrones retardados y SALTO INMEDIATO (prompt jump): P = (λ·S + q) / (β − ρ),
///   dS/dt = β·P − λ·S. Por eso el reactor sube «despacio»: los neutrones retardados (β ≈ 0,73 %) mandan.
/// - REACTIVIDAD ρ = exceso del núcleo − barras + realimentación por temperatura del combustible (negativa: el UZrH
///   frena solo la potencia; es la seguridad inherente del TRIGA).
/// - Cuatro BARRAS de carburo de boro (regulación, dos de compensación «shim» y la transitoria), con curva de valor en S.
/// - SUBCRÍTICO con fuente de arranque (Am-Be): sin fuente no se «ve» nada; cerca de crítico la potencia se multiplica.
/// - TÉRMICA: combustible (constante ~10 s) → piscina (~270 m³, se calienta ~3 °C/h a 1 MW sin refrigerar) → bomba e
///   intercambiador con el agua de mar polar.
/// - Química del agua: la CONDUCTIVIDAD sube con el tiempo y hay que cambiar las resinas (≤ 2 µS/cm).
/// - Disparos (SCRAM): sobrepotencia, temperatura del combustible, supercrítico inmediato, piscina caliente, manual.
/// Todos los números van con su fuente en docs/atomic-plant.md.
/// </summary>
public class ResearchReactor : MonoBehaviour
{
    public enum Rod { Regulacion, Shim1, Shim2, Transitoria }

    public static ResearchReactor Instance { get; private set; }

    [Header("Cinética (U-235 en UZrH)")]
    public float beta = 0.0073f;            // fracción de neutrones retardados efectiva
    public float lambda = 0.0767f;          // constante de desintegración de los precursores (un grupo), 1/s
    public float excessReactivity = 0.022f; // exceso del núcleo con todas las barras fuera (≈ 3 $)
    public float sourceTerm = 1e-3f;        // fuente de arranque Am-Be (W·Δk): fija la potencia subcrítica

    [Header("Barras (valor total en Δk/k)")]
    public float[] rodWorth = { 0.008f, 0.020f, 0.020f, 0.017f };
    [Range(0f, 1f)] public float[] rodPos = { 0f, 0f, 0f, 0f };   // 0 = dentro, 1 = fuera
    public float[] rodSpeed = { 0.04f, 0.02f, 0.02f, 0.02f };     // fracción por segundo (mecanismo de motor)

    [Header("Térmica")]
    public float tempCoeff = -8e-5f;         // Δk/k por °C de combustible (negativo: seguridad inherente)
    public float fuelToPool = 5000f;         // W/°C (a 1 MW, el combustible ~200 °C sobre la piscina)
    public float fuelHeatCap = 5e4f;         // J/°C
    public float poolHeatCap = 1.13e9f;      // J/°C (~270 m³ de agua desmineralizada)
    public float exchangerUA = 7e4f;         // W/°C con la bomba en marcha
    public float seaWaterTemp = 4f;          // °C: el agua de mar polar enfría el intercambiador

    [Header("Límites (disparo)")]
    public float maxPower = 1.1e6f;          // W
    public float fuelTempLimit = 550f;       // °C (ajuste del sistema de seguridad, muy por debajo del límite del combustible)
    public float poolTempLimit = 48f;        // °C

    [Header("Estado")]
    public float power;                      // W térmicos
    public float precursors;                 // S (en W): los neutrones retardados «guardados»
    public float fuelTemp = 20f, poolTemp = 20f;
    public bool pumpOn = true;
    public bool scrammed = true;             // arranca parado (barras dentro)
    public string scramReason = "parado";
    public bool autoMode;                    // la barra de regulación sigue a la potencia pedida
    public float setpoint = 1e5f;            // W
    public float conductivity = 0.8f;        // µS/cm
    public float periodSeconds = float.PositiveInfinity;

    [Header("Vista")]
    public Light cherenkov;                  // el brillo azul del núcleo (radiación de Cherenkov)

    float _lastPower;

    void OnEnable() => Instance = this;
    void OnDisable() { if (Instance == this) Instance = null; }

    void Start()
    {
        power = sourceTerm / Mathf.Max(1e-4f, -Reactivity());
        precursors = beta * power / lambda;
        _lastPower = power;
    }

    /// <summary>Valor insertado-fuera de una barra a la posición x (curva en S de la teoría de barras).</summary>
    public float RodReactivity(int i) => rodWorth[i] * (rodPos[i] - Mathf.Sin(2f * Mathf.PI * rodPos[i]) / (2f * Mathf.PI));

    /// <summary>Reactividad total (Δk/k): exceso − barras dentro + temperatura.</summary>
    public float Reactivity()
    {
        float r = excessReactivity;
        for (int i = 0; i < rodWorth.Length; i++) r -= rodWorth[i] - RodReactivity(i);
        return r + tempCoeff * (fuelTemp - 20f);
    }

    public float Dollars => Reactivity() / beta;

    public void MoveRod(Rod rod, float dir, float dt)
    {
        if (scrammed && dir > 0f) return;   // tras un disparo hay que REARMAR antes de sacar barras
        int i = (int)rod;
        rodPos[i] = Mathf.Clamp01(rodPos[i] + dir * rodSpeed[i] * dt);
    }

    public void Scram(string reason)
    {
        if (!scrammed) Debug.Log($"[Reactor] SCRAM: {reason}. Las barras caen por gravedad.");
        scrammed = true; scramReason = reason; autoMode = false;
    }

    /// <summary>Rearme: solo con todas las barras dentro (como en la consola real).</summary>
    public bool Rearm()
    {
        for (int i = 0; i < rodPos.Length; i++) if (rodPos[i] > 0.01f) return false;
        scrammed = false; scramReason = "";
        return true;
    }

    void Update()
    {
        float dt = Mathf.Min(Time.deltaTime, 0.1f);
        int steps = Mathf.Max(1, Mathf.CeilToInt(dt / 0.02f));
        for (int k = 0; k < steps; k++) Step(dt / steps);

        periodSeconds = power > _lastPower * 1.000001f ? dt / Mathf.Log(power / _lastPower) : float.PositiveInfinity;
        _lastPower = power;

        conductivity += 0.0005f * dt;   // los iones del agua se acumulan: hay que cambiar las resinas
        if (cherenkov != null)
        {
            // El brillo crece con el LOGARITMO de la potencia (a 1 W ni se ve; a 1 MW la piscina brilla azul).
            cherenkov.intensity = Mathf.Clamp01((Mathf.Log10(Mathf.Max(power, 1f)) - 1f) / 5f) * 6f;
            cherenkov.enabled = cherenkov.intensity > 0.05f;
        }
    }

    void Step(float dt)
    {
        if (scrammed)
            for (int i = 0; i < rodPos.Length; i++) rodPos[i] = Mathf.Max(0f, rodPos[i] - dt);   // caída por gravedad (~1 s)
        else if (autoMode)
        {
            float err = Mathf.Log(Mathf.Max(setpoint, 1f) / Mathf.Max(power, 1f));
            rodPos[0] = Mathf.Clamp01(rodPos[0] + Mathf.Clamp(err * 0.02f, -rodSpeed[0], rodSpeed[0]) * dt);
        }

        float rho = Reactivity();
        if (rho >= beta * 0.95f) { Scram("supercrítico inmediato (ρ ≥ β)"); rho = Reactivity(); }
        power = Mathf.Max(0f, (lambda * precursors + sourceTerm) / (beta - rho));
        precursors += (beta * power - lambda * precursors) * dt;

        float toPool = fuelToPool * (fuelTemp - poolTemp);
        fuelTemp += (power - toPool) / fuelHeatCap * dt;
        float removed = pumpOn ? exchangerUA * Mathf.Max(0f, poolTemp - seaWaterTemp) : 0f;
        poolTemp += (toPool - removed) / poolHeatCap * dt;
        if (pumpOn) poolTemp = Mathf.Max(poolTemp, seaWaterTemp + 14f);   // el intercambiador no baja la piscina de ~18 °C (control de temperatura)

        if (power > maxPower) Scram("sobrepotencia");
        if (fuelTemp > fuelTempLimit) Scram("temperatura del combustible");
        if (poolTemp > poolTempLimit) Scram("piscina demasiado caliente");
    }

    /// <summary>Flujo de neutrones térmicos en la posición de irradiación (n/cm²·s): ~1e13 a 1 MW (Penn State).</summary>
    public float Flux => 1e13f * power / 1e6f;

    public static string FormatPower(float w)
    {
        if (w >= 1e6f) return $"{w / 1e6f:0.00} MW";
        if (w >= 1e3f) return $"{w / 1e3f:0.0} kW";
        if (w >= 1f) return $"{w:0.0} W";
        return $"{w * 1e3f:0.00} mW";
    }
}
