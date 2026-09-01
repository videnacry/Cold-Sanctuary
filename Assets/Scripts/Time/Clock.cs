using UnityEngine;

/// <summary>
/// Hace correr el RELOJ del juego: cada frame avanza <see cref="TimeController"/> con el delta real. La VELOCIDAD del
/// tiempo (TimeSpeed) decide cuántas horas pasan; este componente solo aporta el "tic" y la HORA INICIAL (al Play).
/// Ponlo UNA vez en la escena (SampleSceneBuilder lo añade como Clock_AUTO). Sin él, el reloj se queda en startHour
/// (día) → degradación segura: el fitoplancton fotosintetiza y nada crashea.
/// </summary>
public class Clock : MonoBehaviour
{
    [Tooltip("Hora del día al pulsar Play (0-24). La velocidad del tiempo se encarga del resto.")]
    [Range(0f, 24f)] public float startHour = 8f;

    void Awake()  => TimeController.timeController.startHour = startHour;
    void Update() => TimeController.timeController.Advance(Time.deltaTime);
}
