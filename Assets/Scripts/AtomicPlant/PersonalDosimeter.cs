using UnityEngine;

/// <summary>
/// EL DOSÍMETRO PERSONAL (se recoge en la taquilla de la entrada): mide la tasa de dosis donde estás y la dosis
/// acumulada. Límite de los trabajadores: 20 mSv/año de media (ICRP 103). Sin dosímetro no se entra a la sala del
/// reactor ni se opera la consola. Además lleva la marca de CONTAMINACIÓN (una gota de agua de la piscina o de una
/// muestra en las manos) que detecta el pórtico de la salida.
/// </summary>
public class PersonalDosimeter : MonoBehaviour
{
    public float accumulatedMicroSv;
    public float rateMicroSvPerHour;
    public bool contaminated;
    [Tooltip("Muestra el dosímetro en pantalla (solo cerca de alguna fuente).")]
    public bool showHud = true;

    public const float AnnualLimitMicroSv = 20000f;

    public static PersonalDosimeter Of(Component who) => who == null ? null : who.GetComponent<PersonalDosimeter>();

    void Update()
    {
        rateMicroSvPerHour = RadiationSource.TotalRateAt(transform.position + Vector3.up);
        accumulatedMicroSv += rateMicroSvPerHour * Time.deltaTime / 3600f;
    }

    void OnGUI()
    {
        if (!showHud || rateMicroSvPerHour < 0.2f && !contaminated) return;
        string c = contaminated ? "   ·   ¡contaminado! (pórtico de salida)" : "";
        GUI.Box(new Rect(Screen.width - 330f, 10f, 320f, 46f), "");
        GUI.Label(new Rect(Screen.width - 322f, 13f, 310f, 40f),
            $"Dosímetro: {rateMicroSvPerHour:0.0} µSv/h   ·   acumulada {accumulatedMicroSv:0.00} µSv{c}\n(límite anual: {AnnualLimitMicroSv / 1000f:0} mSv)");
    }
}
