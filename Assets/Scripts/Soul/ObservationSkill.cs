using UnityEngine;

/// <summary>
/// La OBSERVACIÓN como habilidad que EVOLUCIONA POR USO (docs/apremios-guardian-observacion.md §4) — como la agilidad/
/// percepción ya suben con el uso (`AptitudeEvolution`). `trained` sube al mirar sostenido (<see cref="ObserveSpell"/>);
/// `boost` es un impulso temporal de ecuanimidad (mirar ahora mismo) que DECAE. <see cref="Observation.LevelOf"/> las suma.
/// </summary>
public class ObservationSkill : MonoBehaviour
{
    [Range(0f, 1f)] public float trained = 0f;   // sube despacio por uso (mirar)
    [Range(0f, 1f)] public float boost   = 0f;   // ecuanimidad temporal (observando ahora); decae
    [Min(0f)] public float boostDecayPerSecond = 0.3f;
    [Min(0f)] public float trainedCap = 1f;

    /// <summary>Nivel efectivo [0,1] = entrenado + impulso temporal.</summary>
    public float Level => Mathf.Clamp01(trained + boost);

    public void Train(float amount) => trained = Mathf.Clamp(trained + amount, 0f, trainedCap);
    public void AddBoost(float amount) => boost = Mathf.Clamp01(boost + amount);

    void Update()
    {
        if (boost > 0f) boost = Mathf.Max(0f, boost - boostDecayPerSecond * Time.deltaTime);
    }
}
