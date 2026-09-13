using UnityEngine;

/// <summary>
/// HECHIZO PASIVO DE LOS OJOS: OBSERVAR (docs/apremios-guardian-observacion.md §5) — mirar SOSTENIDO a un ánima (o a la
/// tarea que se tiene delante) produce **observación/ecuanimidad**: entrena la habilidad (<see cref="ObservationSkill"/>,
/// evolución por uso) y da un impulso temporal de calma. Es el pasivo que conceden los OJOS; en el modelo de anatomía se
/// gatea por `CharacterComposition.grants` ("receptor visual") — aquí se añade al ser que tiene ojos (p.ej. Sakshi, que
/// débil solo puede observar). Auto-crea el `ObservationSkill` si falta. Balance-safe: sin objetivo a la vista, no hace nada.
/// </summary>
public class ObserveSpell : MonoBehaviour
{
    [Tooltip("Segundos ATENDIENDO al mismo objetivo para que cuente como 'observar'.")]
    [Min(0f)] public float gazeSeconds = 1.5f;
    [Tooltip("SENTIDOS con los que este ser observa (docs/consciousness-mechanics §3). Por defecto los 5 básicos; un " +
             "topo podría ser {olfato,tacto}, un murciélago {eco,oido}… El alcance sale del mejor sentido (Senses).")]
    public string[] senses = { "vista", "oido", "olfato", "gusto", "tacto" };
    [Tooltip("Radio de respaldo si sus sentidos no dan alcance (0).")]
    [Min(0.5f)] public float fallbackRange = 8f;
    [Tooltip("Cuánto ENTRENA la observación por segundo mientras observa (sube despacio).")]
    [Min(0f)] public float trainPerSecond = 0.02f;
    [Tooltip("Impulso de ECUANIMIDAD por segundo mientras observa (decae luego).")]
    [Min(0f)] public float calmPerSecond = 0.4f;

    Anima _self;
    ObservationSkill _skill;
    Transform _gazed;
    float _gazeTime;

    void Awake()
    {
        _self = GetComponent<Anima>();
        _skill = GetComponent<ObservationSkill>();
        if (_skill == null) _skill = gameObject.AddComponent<ObservationSkill>();
    }

    void Update()
    {
        Transform t = NearestAnima();
        if (t != null && t == _gazed) _gazeTime += Time.deltaTime;
        else { _gazed = t; _gazeTime = 0f; }

        if (_gazed != null && _gazeTime >= gazeSeconds)
        {
            _skill.Train(trainPerSecond * Time.deltaTime);    // mirar sostenido → sube la observación (evolución por uso)
            _skill.AddBoost(calmPerSecond * Time.deltaTime);  // y da ecuanimidad AHORA (amortigua el sufrimiento)
        }
    }

    Transform NearestAnima()
    {
        float reach = Senses.BestReach(senses);            // el alcance sale del MEJOR sentido que tenga
        if (reach <= 0f) reach = fallbackRange;            // sin sentidos exteroceptivos → respaldo
        Transform best = null; float bestSq = reach * reach;
        foreach (Collider c in Physics.OverlapSphere(transform.position, reach))
        {
            Anima a = c.GetComponentInParent<Anima>();
            if (a == null || a == _self) continue;
            float d = (a.transform.position - transform.position).sqrMagnitude;
            if (d < bestSq) { bestSq = d; best = a.transform; }
        }
        return best;
    }
}
