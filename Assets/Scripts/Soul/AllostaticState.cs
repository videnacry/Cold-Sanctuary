using UnityEngine;

/// <summary>
/// HUB ALOSTÁTICO (docs/apremios-guardian-observacion.md §3) — el punto donde los hechizos-estado EXTERNOS modulan al
/// Guardián (`MoodDynamics`), sin que este tenga que conocerlos:
///   • <see cref="ExtraLoad"/> — carga añadida por MIEDO / DUELO / ABSTINENCIA (se suma a la carga alostática del Guardián).
///   • <see cref="Sedation"/>  — sedación química del ESTUPEFACIENTE (amortigua el sufrimiento, como la observación, pero
///     por química: enmascara el daño → riesgo/adicción).
/// Ambas DECAEN solas. Aditivo/graceful: sin hechizos activos, ExtraLoad=0 y Sedation=0 → el Guardián no cambia.
/// </summary>
public class AllostaticState : MonoBehaviour
{
    [Min(0f)] public float loadDecayPerSecond = 0.2f;
    [Min(0f)] public float sedationDecayPerSecond = 0.15f;

    float _extraLoad;
    float _sedation;

    public float ExtraLoad => Mathf.Clamp01(_extraLoad);
    public float Sedation  => Mathf.Clamp01(_sedation);

    public void AddLoad(float amount)     => _extraLoad = Mathf.Clamp(_extraLoad + amount, 0f, 2f);
    public void AddSedation(float amount) => _sedation  = Mathf.Clamp01(_sedation + amount);

    void Update()
    {
        float dt = Time.deltaTime;
        if (_extraLoad > 0f) _extraLoad = Mathf.Max(0f, _extraLoad - loadDecayPerSecond * dt);
        if (_sedation  > 0f) _sedation  = Mathf.Max(0f, _sedation  - sedationDecayPerSecond * dt);
    }

    /// <summary>Obtiene (o crea) el hub alostático de un ser.</summary>
    public static AllostaticState Of(Component c)
    {
        AllostaticState s = c.GetComponent<AllostaticState>();
        return s != null ? s : c.gameObject.AddComponent<AllostaticState>();
    }
}
