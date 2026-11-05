using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL DRAGÓN DE DRONES (Berón 2026-11-05; docs/energia.md §7): una bandada de drones que dibuja una serpiente en el aire alrededor de la turbina que ha parado. Es un ADORNO DEL JUEGO:
/// **ninguna prueba dice que un dragón de drones ahuyente aves**, y con fauna antártica real sería una molestia que pediría permiso. No forma parte de la seguridad: la turbina para
/// igual con él o sin él. Vuelo acotado (<see cref="FlightSeconds"/>) y con espera entre salidas: nada de bucles sin fin, y nunca sale del parque.
/// </summary>
public class DroneDragon : MonoBehaviour
{
    public const float FlightSeconds = 45f, CooldownSeconds = 300f, Orbit = 60f, Altitude = 30f, Spacing = 0.18f, FadeSeconds = 5f;

    [Tooltip("Los drones, de la cabeza a la cola.")]
    public List<Transform> drones = new List<Transform>();

    public bool Flying { get; private set; }
    public int Launches { get; private set; }
    Vector3[] _parked;
    Vector3 _centre;
    float _t, _readyAt = -1f;

    void Awake() { Park(); }

    void Park()
    {
        _parked = new Vector3[drones.Count];
        for (int i = 0; i < drones.Count; i++) _parked[i] = drones[i] != null ? drones[i].position : transform.position;
    }

    /// <summary>Sale a dar vueltas a <paramref name="around"/> (dentro del parque). False si está en vuelo o cargando.</summary>
    public bool Launch(Vector3 around)
    {
        if (Flying || Time.time < _readyAt || drones.Count == 0) return false;
        if (_parked == null || _parked.Length != drones.Count) Park();
        float m = EnergyRule.CapHalf + 60f;
        _centre = new Vector3(Mathf.Clamp(around.x, EnergyRule.Center.x - m, EnergyRule.Center.x + m), 0f, Mathf.Clamp(around.z, EnergyRule.Center.z - m, EnergyRule.Center.z + m));
        Flying = true; _t = 0f; Launches++;
        _readyAt = Time.time + FlightSeconds + CooldownSeconds;
        return true;
    }

    /// <summary>El punto de la serpiente en el tiempo <paramref name="t"/>.</summary>
    Vector3 Path(float t)
    {
        float a = t * 0.5f;
        return _centre + new Vector3(Mathf.Cos(a) * Orbit, Altitude + Mathf.Sin(t * 1.3f) * 5f, Mathf.Sin(a) * Orbit);
    }

    void Update()
    {
        if (!Flying) return;
        _t += Time.deltaTime;
        float blend = Mathf.Clamp01(Mathf.Min(_t, FlightSeconds - _t) / FadeSeconds);
        for (int i = 0; i < drones.Count; i++)
            if (drones[i] != null) drones[i].position = Vector3.Lerp(_parked[i], Path(_t - i * Spacing), blend);
        if (_t < FlightSeconds) return;
        Flying = false;
        for (int i = 0; i < drones.Count; i++) if (drones[i] != null) drones[i].position = _parked[i];
    }
}
