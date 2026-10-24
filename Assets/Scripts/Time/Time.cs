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
        TimeSpeedMinuteSecs = 60f / pTimeSpeed;
    }

    // ── Calendario (configurable en `Clock`) ─────────────────────────────────────
    // DURACIÓN DEL DÍA: minutos REALES que dura un día de juego (por defecto 60: un día = 1 hora). Todo lo que depende del
    // tiempo (hambre por gasto de energía, metabolismo basal, sueño día/noche, digestión, gestaciones, crecimiento de la
    // colmena, producción de las áreas, obrador de la cocina) lee de aquí, así que cambiar este número lo recalibra todo.
    public float RealMinutesPerDay => TimeSpeedMinuteSecs * 1440f / 60f;
    /// <summary>Segundos reales que dura un día de juego.</summary>
    public float SecondsPerGameDay => TimeSpeedMinuteSecs * 1440f;
    /// <summary>Días de juego que dura un AÑO de vida (estaciones comprimidas): los calendarios biológicos (gestación,
    /// intervalo entre crías, madurez) se expresan en meses y se escalan a este año. Por defecto 28 (4 estaciones de 7 días).</summary>
    public float DaysPerYear = 28f;

    /// <summary>Fija cuántos minutos reales dura un día de juego (1 h → un minuto de juego = 2,5 s reales).</summary>
    public void SetDayLength(float realMinutes)
    {
        realMinutes = Mathf.Max(0.5f, realMinutes);
        TimeSpeedMinuteSecs = realMinutes * 60f / 1440f;
        TimeSpeed = (byte)Mathf.Clamp(Mathf.RoundToInt(60f / TimeSpeedMinuteSecs), 1, 255);
    }

    /// <summary>Segundos REALES que duran <paramref name="months"/> meses biológicos en el calendario del santuario.</summary>
    public float SecondsForMonths(float months) => months / 12f * DaysPerYear * SecondsPerGameDay;
    /// <summary>Segundos REALES que duran <paramref name="days"/> días de juego.</summary>
    public float SecondsForDays(float days) => days * SecondsPerGameDay;

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

    /// <summary>DÍA de juego (0 = el del Play): cuántas medianoches han pasado desde startHour.</summary>
    public int Day => Mathf.FloorToInt((startHour + _gameMinutes / 60f) / 24f) + _dayOffset;
    int _dayOffset;

    /// <summary>Hora del día [0,24): parte de startHour y corre con la velocidad.</summary>
    public float Hour => Mod(startHour + _gameMinutes / 60f, 24f);

    /// <summary>Fija la hora del día directamente (tests, o mecánicas como "dormir hasta el amanecer").</summary>
    /// <summary>Pone el reloj en un día Y una hora exactos (puede retroceder). Para tests que fuerzan la hora y luego la
    /// restauran sin adelantar el calendario (la jornada, las edades y los calendarios leen `Day`).</summary>
    public void SetClock(int day, float hour) { startHour = Mod(hour, 24f); _gameMinutes = 0f; _dayOffset = day; }
    public void SetHour(float hour) { int d = Day; float h = Mod(hour, 24f); if (h < Hour) d++; startHour = h; _gameMinutes = 0f; _dayOffset = d; }   // adelantar la hora nunca hace retroceder el día

    /// <summary>¿Es de día? (fotosíntesis del fitoplancton; a futuro, ciclos de sueño). Aprox. 6:00–18:00.</summary>
    public bool IsDay { get { float h = Hour; return h >= 6f && h < 18f; } }

    static float Mod(float a, float b) => a - b * Mathf.Floor(a / b);
}
