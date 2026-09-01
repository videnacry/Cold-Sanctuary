using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class TimeController
{
    public static TimeController timeController = new TimeController();

    public byte TimeSpeed { get; private set; } = 1;
    public float TimeSpeedMinuteSecs { get; private set; } = 60 / 1;
    public void SetTimeSpeed (byte pTimeSpeed)
    {
        TimeSpeed = pTimeSpeed;
        TimeSpeedMinuteSecs = 60 / pTimeSpeed;
    }

    // ── Reloj del día ────────────────────────────────────────────────────────────
    // La VELOCIDAD (TimeSpeed) hace correr las horas; solo falta la HORA INICIAL (al Play). El componente `Clock` da el
    // "tic" (llama a Advance cada frame). Sin Clock en escena, el reloj se queda en startHour (día) → degradación segura.
    public float startHour = 8f;    // hora del día al iniciar la partida (la fija Clock desde el inspector)
    float _gameMinutes;             // minutos de juego acumulados desde el Play

    /// <summary>Avanza el reloj con el delta REAL. TimeSpeedMinuteSecs = segundos reales por minuto de juego →
    /// minutos de juego = deltaReal / segsPorMinuto. Cambiar la velocidad reajusta el ritmo automáticamente.</summary>
    public void Advance(float realDeltaSeconds)
    {
        if (TimeSpeedMinuteSecs > 0.0001f) _gameMinutes += realDeltaSeconds / TimeSpeedMinuteSecs;
    }

    /// <summary>Hora del día [0,24): parte de startHour y corre con la velocidad.</summary>
    public float Hour => Mod(startHour + _gameMinutes / 60f, 24f);

    /// <summary>Fija la hora del día directamente (tests, o mecánicas como "dormir hasta el amanecer").</summary>
    public void SetHour(float hour) { startHour = Mod(hour, 24f); _gameMinutes = 0f; }

    /// <summary>¿Es de día? (fotosíntesis del fitoplancton; a futuro, ciclos de sueño). Aprox. 6:00–18:00.</summary>
    public bool IsDay { get { float h = Hour; return h >= 6f && h < 18f; } }

    static float Mod(float a, float b) => a - b * Mathf.Floor(a / b);
}
