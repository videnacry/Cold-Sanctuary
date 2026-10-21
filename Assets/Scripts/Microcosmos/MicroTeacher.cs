using UnityEngine;

/// <summary>
/// LA SEMBRADORA (Ruth renacida, microcosmos del Huerto): cuida el bancal de hongos y, cada poco, va hacia una
/// recolectora y le ENSEÑA a cultivar: deja de recoger y se vuelve cultivadora. Así la tribu pasa de recolectar a cultivar.
/// </summary>
public class MicroTeacher : MonoBehaviour
{
    public Transform bed;
    public float every = 15f, speed = 3f;
    public int taught;
    float _next;
    MicroForager _student;

    void Start() { _next = Time.time + 6f; }

    void Update()
    {
        if (_student != null)
        {
            if (!MicroMove.Step(transform, _student.transform.position, speed, 2f)) return;
            var g = _student.gameObject.AddComponent<MicroGardener>(); g.bed = bed;
            _student.enabled = false; taught++;
            Debug.Log($"[Microcosmos] La Sembradora enseña a «{_student.name}» a cultivar el hongo ({taught} ya cultivan).");
            _student = null; _next = Time.time + every;
            return;
        }
        if (Time.time >= _next)
        {
            float bd = float.MaxValue;
            foreach (MicroForager f in FindObjectsByType<MicroForager>(FindObjectsSortMode.None))
                if (f.enabled) { float d = (f.transform.position - transform.position).sqrMagnitude; if (d < bd) { bd = d; _student = f; } }
            if (_student == null) _next = Time.time + every;
            return;
        }
        if (bed != null) MicroMove.Step(transform, bed.position + Vector3.right * 3f, speed);   // cuida el bancal
    }
}
