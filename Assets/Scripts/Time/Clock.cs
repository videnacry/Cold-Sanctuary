using UnityEngine;

/// <summary>
/// Hace correr el RELOJ del juego: cada frame avanza <see cref="TimeController"/> con el delta real. Aquí se CONFIGURA el
/// calendario: cuántos minutos reales dura un día de juego (por defecto 60: un día = 1 hora) y cuántos días dura un año de
/// vida (estaciones comprimidas). Ponlo UNA vez en la escena (SampleSceneBuilder lo añade como Clock_AUTO). Sin él, el
/// reloj se queda en startHour (día) → degradación segura: el fitoplancton fotosintetiza y nada crashea.
/// </summary>
public class Clock : MonoBehaviour
{
    [Tooltip("Hora del día al pulsar Play (0-24). La velocidad del tiempo se encarga del resto.")]
    [Range(0f, 24f)] public float startHour = 8f;
    [Tooltip("Minutos REALES que dura un día de juego (60 = un día por hora). Recalibra hambre, sueño, gestaciones, colmena…")]
    [Min(0.5f)] public float realMinutesPerDay = 60f;
    [Tooltip("Días de juego que dura un año de vida (gestaciones e intervalos entre crías se escalan a este año).")]
    [Min(1f)] public float daysPerYear = 28f;

    void Awake()
    {
        TimeController tc = TimeController.timeController;
        tc.startHour = startHour;
        tc.SetDayLength(realMinutesPerDay);
        tc.DaysPerYear = daysPerYear;
    }

    void Update() => TimeController.timeController.Advance(Time.deltaTime);

    /// <summary>Cambia la duración del día en caliente (consola: <c>dia(N)</c>, N minutos reales).</summary>
    public void SetDayLength(float minutes)
    {
        realMinutesPerDay = Mathf.Max(0.5f, minutes);
        TimeController.timeController.SetDayLength(realMinutesPerDay);
    }
}
