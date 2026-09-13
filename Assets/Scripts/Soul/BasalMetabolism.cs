using UnityEngine;

/// <summary>
/// METABOLISMO BASAL (BMR) — el coste de "solo estar vivo" (docs/apremios-guardian-observacion.md §3.1). Escala con la
/// **masa** por la **ley de Kleiber** (`BMR ∝ masa^0.75`): un cuerpo grande gasta más en absoluto, menos por gramo. Drena
/// ATP (`CharacterLevel`) con el tiempo (escalado por la velocidad del juego); lo repone comer (`Metabolism`). Es la línea
/// base del presupuesto energético (TEE = BMR + actividad[`Exertion`] + termorregulación[medio]). Opt-in y graceful
/// (sin `CharacterLevel` no hace nada).
/// </summary>
public class BasalMetabolism : MonoBehaviour
{
    [Tooltip("ATP/s por unidad de (masa^0.75). Sube = metabolismo más caro (endotermo) / baja = más ahorrador (ectotermo).")]
    [Min(0f)] public float bmrCoefficient = 0.05f;

    Anima _anima;
    CharacterLevel _level;

    void Awake()
    {
        _anima = GetComponent<Anima>();
        _level = GetComponent<CharacterLevel>();
    }

    void Update()
    {
        if (_anima == null || _level == null || _anima.death) return;
        int speed = TimeController.timeController != null ? Mathf.Max(1, TimeController.timeController.TimeSpeed) : 1;
        float bmr = bmrCoefficient * Mathf.Pow(Mathf.Max(0.01f, _anima.BodyMass), 0.75f);   // Kleiber
        if (!_level.SpendEnergy(bmr * speed * Time.deltaTime)) _level.currentEnergy = 0f;
    }
}
