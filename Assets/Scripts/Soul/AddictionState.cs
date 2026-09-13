using UnityEngine;

/// <summary>
/// ADICCIÓN (docs/apremios-guardian-observacion.md §3) — la sombra del estupefaciente. Cada dosis (`Dose`, la llama
/// <see cref="NarcoticSpell"/>) sube el DESEO (craving) y la TOLERANCIA (cada vez seda menos). Con el tiempo, el deseo
/// crece solo; si no se satisface (no hay dosis reciente), llega la ABSTINENCIA: añade carga alostática (sufrimiento) al
/// hub → el ser "necesita" repetir. Modela el bucle real; es un apremio propio. Balance-safe: sin dosis, inerte.
/// </summary>
public class AddictionState : MonoBehaviour
{
    [Range(0f, 1f)] public float craving = 0f;     // deseo de la sustancia
    [Range(0f, 1f)] public float tolerance = 0f;   // cuánto se ha embotado el efecto

    [Tooltip("Cuánto sube el deseo/tolerancia por dosis.")]
    [Min(0f)] public float doseCraving = 0.15f;
    [Min(0f)] public float doseTolerance = 0.08f;
    [Tooltip("Crecimiento del deseo por segundo cuando NO hay dosis (mono).")]
    [Min(0f)] public float cravingGrowthPerSecond = 0.02f;
    [Tooltip("Carga alostática (sufrimiento) por segundo de la abstinencia, escalada por el deseo.")]
    [Min(0f)] public float withdrawalLoadPerSecond = 0.3f;
    [Tooltip("La tolerancia cede lentamente sin consumo (desintoxicación).")]
    [Min(0f)] public float toleranceDecayPerSecond = 0.005f;

    float _lastDose = -999f;
    [Tooltip("Segundos sin dosis a partir de los cuales cuenta como abstinencia.")]
    [Min(0f)] public float withdrawalAfter = 3f;

    public float Tolerance => Mathf.Clamp01(tolerance);

    public void Dose()
    {
        craving   = Mathf.Clamp01(craving + doseCraving);
        tolerance = Mathf.Clamp01(tolerance + doseTolerance);
        _lastDose = Time.time;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        bool inWithdrawal = Time.time - _lastDose >= withdrawalAfter && craving > 0.05f;

        if (inWithdrawal)
        {
            craving = Mathf.Clamp01(craving + cravingGrowthPerSecond * dt);        // el mono crece
            AllostaticState.Of(this).AddLoad(withdrawalLoadPerSecond * craving * dt);   // y hace sufrir (empuja a repetir)
        }
        if (tolerance > 0f) tolerance = Mathf.Max(0f, tolerance - toleranceDecayPerSecond * dt);   // desintoxica despacio
    }
}
