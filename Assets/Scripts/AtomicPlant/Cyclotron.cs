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

/// <summary>Consola del ciclotrón: arrancar el haz, parar y mandar el F-18, o ver la física. Pide el dosímetro.</summary>
public class CyclotronConsole : MonoBehaviour, IInteractable
{
    public Cyclotron cyclotron;
    public FdgSynthesis synthesis;

    public string InteractLabel => cyclotron != null && cyclotron.beamOn
        ? $"Ciclotrón: {cyclotron.targetBq / 1e9f:0.0} GBq de F-18 en el blanco ({cyclotron.irradiatedMin:0} min)"
        : "Consola del ciclotrón (producir F-18 para el PET)";
    public bool CanInteract => cyclotron != null && !ChoiceDialog.IsOpen;

    public void Interact()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null && p.GetComponent<PersonalDosimeter>() == null) { PlantNotice.Say("Zona controlada: coge antes el dosímetro en la taquilla de la entrada."); return; }
        if (cyclotron.beamOn)
            ChoiceDialog.Show("Ciclotrón: haz encendido",
                new[] { "Parar el haz y mandar el F-18 a la celda de síntesis", "Seguir irradiando" },
                i => { if (i == 0) Deliver(); }, null, transform, 3f);
        else
            ChoiceDialog.Show("Ciclotrón: ¿qué hacemos?",
                new[] { $"Arrancar el haz ({cyclotron.beamMicroA:0} µA sobre agua enriquecida en ¹⁸O)", "Cómo funciona un ciclotrón" },
                i => { if (i == 0) cyclotron.StartBeam(); else Explain(); }, null, transform, 3f);
    }

    void Deliver()
    {
        float a = cyclotron.StopAndDeliver();
        if (synthesis == null) synthesis = FindFirstObjectByType<FdgSynthesis>();
        if (synthesis != null) synthesis.Receive(a);
    }

    static void Explain() => PlantNotice.Say(
        $"Un imán de {Cyclotron.FieldTesla:0.0} T curva los protones en círculo; dos electrodos huecos («Des») a {Cyclotron.DeeKV:0} kV los empujan " +
        $"cada vez que cruzan el hueco. Como f = qB/2πm no depende de la velocidad, una RF fija de {Cyclotron.FrequencyHz / 1e6f:0.0} MHz basta: " +
        "cuanto más rápidos, más grande el círculo, pero tardan lo mismo por vuelta. El reactor hace isótopos con neutrones; el ciclotrón, con protones.");
}

/// <summary>
/// CELDA DE SÍNTESIS DE FDG (fluorodesoxiglucosa): el F-18 llega del blanco y un módulo automático lo une a la glucosa
/// (~25 min, rendimiento radioquímico ~60 %). Cada PET de adulto usa ~370 MBq. Lo producido se apunta en
/// <see cref="MedicalIsotopes.Fdg"/> para el hospital: el F-18 caduca rápido (T½ 110 min), por eso se hace al lado.
/// </summary>
public class FdgSynthesis : MonoBehaviour, IInteractable
{
    public const float SynthesisMin = 25f, RadiochemicalYield = 0.6f, PetDoseBq = 3.7e8f;
    float _bq, _arrived;

    public string InteractLabel => _bq > 0f ? "Celda de síntesis: hacer FDG con el F-18" : "Celda de síntesis de FDG (vacía)";
    public bool CanInteract => true;

    public void Receive(float bq) { _bq = bq; _arrived = Time.time; }

    public void Interact()
    {
        if (_bq <= 0f) { PlantNotice.Say("La celda está vacía: produce F-18 en la consola del ciclotrón y para el haz para mandarlo aquí."); return; }
        float waitedMin = (Time.time - _arrived) * PneumaticTransfer.MinutesPerSecond + SynthesisMin;
        float fdg = _bq * RadiochemicalYield * Mathf.Pow(0.5f, waitedMin / Cyclotron.F18HalfLifeMin);
        MedicalIsotopes.Fdg.Add((fdg, Time.time));
        PlantNotice.Say($"FDG lista: {fdg / 1e9f:0.0} GBq tras {waitedMin:0} min (síntesis {SynthesisMin:0} min, rendimiento {RadiochemicalYield:P0}). " +
                        $"Da para ~{fdg / PetDoseBq:0} PET de 370 MBq si llega pronto: cada 110 min queda la mitad.");
        _bq = 0f;
    }
}
