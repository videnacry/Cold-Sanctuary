using UnityEngine;

/// <summary>
/// LA TURBINA MAREAL PILOTO (docs/energia.md): lenta a propósito, con la punta por debajo del umbral de daño de una foca, y con un sonar que la PARA si un
/// mamífero se acerca a <see cref="EnergyRule.MarineStopRadius"/> m. El sitio le dice qué ha detectado una vez por segundo.
/// </summary>
public class TidalTurbineUnit : MonoBehaviour
{
    public Transform rotor;
    [Tooltip("El sonar activo que vigila el agua.")]
    public Transform sonar;

    public float rpm;
    public bool Stopped { get; private set; }
    public string Why { get; private set; } = "";
    public int Stops { get; private set; }
    float _freeAt = -1f, _targetRpm;

    public float OutputKW(float current) => Stopped ? 0f : EnergyRule.TidalPowerKW(current);

    /// <summary>La punta a la que iría ahora (m/s): el regulador la limita.</summary>
    public float TipSpeed => rpm * 2f * Mathf.PI / 60f * EnergyRule.TidalRotorD * 0.5f;

    public void Hold(string why)
    {
        if (!Stopped) { Stops++; Debug.Log($"[Energía] {name} para: {why}."); }
        Stopped = true; Why = why;
        _freeAt = Time.time + EnergyRule.ResumeSeconds;
    }

    public void Tick(float current)
    {
        if (Stopped && Time.time >= _freeAt) { Stopped = false; Why = ""; Debug.Log($"[Energía] {name} rearma."); }
        _targetRpm = Stopped ? 0f : EnergyRule.TipSpeed(current) / (EnergyRule.TidalRotorD * 0.5f) * 60f / (2f * Mathf.PI);
    }

    public void ClearForTest() { Stopped = false; Why = ""; _freeAt = -1f; }

    void Update()
    {
        rpm = Mathf.MoveTowards(rpm, _targetRpm, 3f * Time.deltaTime);
        if (rotor != null && rpm > 0.01f) rotor.Rotate(Vector3.forward, rpm * 6f * Time.deltaTime, Space.Self);
    }
}
