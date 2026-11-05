using UnityEngine;

/// <summary>
/// UN AEROGENERADOR del Parque Energético (docs/energia.md): gira con el viento y se PARA si un ave se acerca o hay alguien al pie de la torre.
/// El sitio (<see cref="EnergySite"/>) le dice qué ha detectado una vez por segundo; rearma sola <see cref="EnergyRule.ResumeSeconds"/> s después.
/// </summary>
public class WindTurbineUnit : MonoBehaviour
{
    public Transform rotor;
    [Tooltip("La pala pintada de negro (Smøla): rompe el emborronado del giro para las aves.")]
    public Transform blackBlade;
    [Tooltip("Las cámaras y el radar de esta torre.")]
    public Transform detector;

    public float rpm, targetRpm;
    public bool Stopped { get; private set; }
    public string Why { get; private set; } = "";
    public int Stops { get; private set; }
    float _freeAt = -1f;

    /// <summary>Lo que entrega ahora (kW).</summary>
    public float OutputKW(float wind) => Stopped ? 0f : EnergyRule.WindPowerKW(wind);

    /// <summary>Hay un motivo para parar (ave, persona) ahora mismo: para y cuenta el rearme desde que deje de haberlo.</summary>
    public void Hold(string why)
    {
        if (!Stopped) { Stops++; Debug.Log($"[Energía] {name} para: {why}."); }
        Stopped = true; Why = why;
        _freeAt = Time.time + EnergyRule.ResumeSeconds;
    }

    /// <summary>Una vez por segundo: rearma si ya pasó el plazo y fija el giro para este viento.</summary>
    public void Tick(float wind)
    {
        if (Stopped && Time.time >= _freeAt) { Stopped = false; Why = ""; Debug.Log($"[Energía] {name} rearma."); }
        bool turns = !Stopped && wind >= EnergyRule.CutIn && wind <= EnergyRule.CutOut;
        targetRpm = turns ? Mathf.Lerp(18f, 34f, Mathf.InverseLerp(EnergyRule.CutIn, EnergyRule.Rated, wind)) : 0f;   // (la E-33 gira despacio, de 18 a 45 rpm)
    }

    /// <summary>(Pruebas) Rearma ya, sin esperar el plazo.</summary>
    public void ClearForTest() { Stopped = false; Why = ""; _freeAt = -1f; }

    void Update()
    {
        rpm = Mathf.MoveTowards(rpm, targetRpm, 12f * Time.deltaTime);   // frena en unos segundos
        if (rotor != null && rpm > 0.01f) rotor.Rotate(Vector3.forward, rpm * 6f * Time.deltaTime, Space.Self);
    }
}
