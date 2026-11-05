using UnityEngine;

/// <summary>
/// EL CICLOTRÓN MÉDICO (docs/atomic-plant.md §7), en la planta de arriba de la planta atómica: el complemento del
/// reactor. El reactor da isótopos RICOS EN NEUTRONES (Mo-99 → Tc-99m); el ciclotrón, RICOS EN PROTONES: el FLÚOR-18
/// del PET, por la reacción ¹⁸O(p,n)¹⁸F sobre agua enriquecida en oxígeno-18.
/// Cifras de un ciclotrón compacto de iones negativos tipo PETtrace (WBIC Cambridge): campo medio ≈ 1,8 T, RF 27,2 MHz,
/// protones de 16,5 MeV, extracción por láminas de carbono que arrancan los electrones del H⁻.
/// Física que enseña (todo calculado, no escrito a mano):
/// - frecuencia del ciclotrón f = qB / 2πm (no depende de la velocidad: por eso funciona);
/// - radio de la órbita final r = p / qB, con p = √(2mE) (≈ 0,33 m a 16,5 MeV y 1,8 T);
/// - actividad en el blanco A = Y_sat · I · (1 − e^(−λt)), con Y_sat ≈ 8,3 GBq/µA a 18 MeV (agua, IJRR 2003) y
///   T½(F-18) = 109,77 min. El tiempo de irradiación se comprime como en el conejo neumático (1 s real = 10 min).
/// </summary>
public class Cyclotron : MonoBehaviour
{
    public static Cyclotron Instance { get; private set; }

    public const float FieldTesla = 1.8f, ProtonMeV = 16.5f, DeeKV = 35f;
    public const float SatYieldBqPerMicroA = 8.3e9f;     // ¹⁸O(p,n)¹⁸F, agua enriquecida > 97 %
    public const float F18HalfLifeMin = 109.77f;
    const float ProtonMassKg = 1.6726e-27f, Charge = 1.6022e-19f, ProtonMeVc2 = 938.272f;

    [Tooltip("Corriente del haz en el blanco (µA). Los PETtrace van de 40 a 160 µA según el modelo.")]
    public float beamMicroA = 60f;
    public bool beamOn;
    public float targetBq;          // F-18 en el agua del blanco
    public float irradiatedMin;     // minutos (comprimidos) de esta irradiación
    public RadiationSource neutrons; // neutrones y gammas del blanco mientras hay haz (lo frena el autoblindaje)

    /// <summary>Frecuencia del ciclotrón (Hz): f = qB / 2πm.</summary>
    public static float FrequencyHz => Charge * FieldTesla / (2f * Mathf.PI * ProtonMassKg);
    /// <summary>Radio de la última órbita (m): r = p / qB con p = √(2·m·E) (no relativista; a 16,5 MeV, error &lt; 1 %).</summary>
    public static float FinalRadiusM => Mathf.Sqrt(2f * ProtonMeVc2 * ProtonMeV) / 1000f / (0.2998f * FieldTesla);
    /// <summary>Vueltas que da el protón: cada vuelta gana 2 veces el voltaje de la «D» (dos huecos por vuelta).</summary>
    public static int Turns => Mathf.RoundToInt(ProtonMeV * 1000f / (2f * DeeKV));

    static float Lambda => Mathf.Log(2f) / F18HalfLifeMin;

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }

    void Update()
    {
        float min = Time.deltaTime * PneumaticTransfer.MinutesPerSecond;
        float sat = beamOn ? SatYieldBqPerMicroA * beamMicroA : 0f;
        targetBq += (sat - targetBq) * (1f - Mathf.Exp(-Lambda * min));   // dA/dt = λ(Y·I − A)
        if (beamOn) irradiatedMin += min;
        if (neutrons != null) neutrons.microSvPerHourAt1m = beamOn ? 2e5f : 0f;   // sin autoblindaje sería letal en minutos
    }

    public void StartBeam()
    {
        beamOn = true; irradiatedMin = 0f;
        PlantNotice.Say($"Haz de H⁻ a {beamMicroA:0} µA. RF {FrequencyHz / 1e6f:0.0} MHz = qB/2πm a {FieldTesla:0.0} T: la misma para todas las vueltas. " +
                        $"Unas {Turns} vueltas en espiral hasta r ≈ {FinalRadiusM:0.00} m y {ProtonMeV:0.0} MeV; la lámina de carbono le quita los electrones y sale al blanco.");
    }

    /// <summary>Para el haz y devuelve la actividad que hay en el blanco (va por la línea blindada a la celda de síntesis).</summary>
    public float StopAndDeliver()
    {
        beamOn = false;
        float a = targetBq; targetBq = 0f;
        float frac = 1f - Mathf.Exp(-Lambda * irradiatedMin);
        PlantNotice.Say($"Fin de irradiación: {a / 1e9f:0.0} GBq de F-18 ({irradiatedMin:0} min, {frac:P0} de la saturación). " +
                        "Irradiar más de 2–3 h apenas suma: lo que se crea ya se desintegra casi al mismo ritmo. Va por la línea blindada a la celda de síntesis.");
        return a;
    }
}


