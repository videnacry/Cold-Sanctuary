using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LOS ANIMALES INGRESADOS COMEN (JornadaTest 2026-10-29: el lobo paciente de la perrera del hospital veterinario no comía
/// nunca; encerrado, no puede ir a buscar comida y nadie se la llevaba). Quien trabaja en el HOSPITAL (el personal
/// veterinario de turno; <see cref="WorkdayDirector"/> enciende esto mientras su puesto es el hospital) mira cada pocos
/// segundos a los animales ingresados —los que tienen historia clínica (<see cref="HealthRecord"/>) o están en la planta
/// veterinaria—; si uno tiene hambre, va andando (PathMover) hasta su perrera y le da su ración de carne cultivada en la
/// mano: el animal come y se le pasa el hambre. Un animal a la vez por persona; dos personas no van al mismo.
/// Sus necesidades (baño, comer, dormir) van antes; todo con tope: si no llega en <see cref="walkTimeout"/> s, lo deja.
/// </summary>
public class VetCare : MonoBehaviour
{
    public float speed = 2.6f;
    public float checkEvery = 4f;
    [Tooltip("Hambre (0..1) a partir de la que se le lleva la ración.")]
    public float hungerAbove = 0.35f;
    [Tooltip("s antes de volver a darle de comer al mismo animal.")]
    public float refeedAfter = 120f;
    public float walkTimeout = 90f;
    public float meal = 0.7f;

    static readonly Dictionary<Animal, float> _lastFed = new Dictionary<Animal, float>();
    static readonly Dictionary<Animal, VetCare> _claimed = new Dictionary<Animal, VetCare>();
    static Bounds? _vetFloor;
    static float _vetFloorAt = -99f;

    Animal _patient;
    float _since, _next, _feedUntil;
    float _best = float.MaxValue, _bestAt;
    Transform _ration;
    public static int Fed { get; private set; }

    /// <summary>Lleva o da de comer a un animal ingresado (la jornada no lo mueve mientras tanto).</summary>
    public bool Busy => enabled && (_patient != null || _ration != null);
    public string Doing => _patient != null ? $"da de comer a «{_patient.name}» (ingresado)" : null;

    void OnDisable() { Drop(); }

    void Update()
    {
        // Solo EN SU TURNO del hospital: fuera de él (o si otra cosa lo usa, como una prueba) no se lo lleva a la perrera.
        WorkdayDirector wd = WorkdayDirector.Instance;
        if (wd == null || !wd.OnShift(gameObject)) { Drop(); enabled = false; return; }
        if (TryGetComponent(out ActionQueue q) && q.Own.Count > 0 && (q.Own[0] is ToiletSpell || q.Own[0] is DineSpell || q.Own[0] is SleepSpell || q.Own[0] is EatSpell)) { Drop(); return; }
        if (TryGetComponent(out Anima me) && (me.asleep || me.death)) { Drop(); return; }

        if (_ration != null)   // comiendo de su mano
        {
            if (Time.time < _feedUntil) return;
            Destroy(_ration.gameObject); _ration = null;
            Drop();
            return;
        }
        if (_patient == null)
        {
            if (Time.time < _next) return;
            _next = Time.time + checkEvery;
            _patient = HungryPatient();
            if (_patient == null) return;
            _claimed[_patient] = this; _since = Time.time;
        }
        if (_patient == null || _patient.death) { Drop(); return; }
        if (Time.time - _since > walkTimeout)
        {
            Debug.Log($"[Veterinaria] «{name}» no llega a «{_patient.name}» en {walkTimeout:0} s: lo deja para luego.");
            _lastFed[_patient] = Time.time - refeedAfter * 0.5f;   // vuelve a intentarlo dentro de un rato
            Drop(); return;
        }
        Vector3 to = _patient.transform.position - transform.position;
        bool level = Mathf.Abs(to.y) < 1.6f; to.y = 0f;
        // En su perrera no se llega a 1,9 m de su centro: sin acercarse 5 s a menos de 5 m, le da la ración por los barrotes
        // (antes rondaba la perrera dando vueltas hasta el tope).
        if (!level) { _best = float.MaxValue; _bestAt = Time.time; }
        else if (to.magnitude < _best - 0.4f) { _best = to.magnitude; _bestAt = Time.time; }
        bool there = level && (to.magnitude <= 1.9f || (to.magnitude < 5f && Time.time - _bestAt > 5f));
        if (!there)
        {
            float left = PathMover.Of(this).StepToward(_patient.transform.position, speed);
            if (!(level && left < 1.9f)) return;
        }
        _best = float.MaxValue;
        Feed(_patient);
    }

    void Feed(Animal a)
    {
        Vector3 d = transform.position - a.transform.position; d.y = 0f; d = d.sqrMagnitude > 1e-4f ? d.normalized : a.transform.forward;
        Vector3 look = -d; if (look.sqrMagnitude > 1e-4f) transform.rotation = Quaternion.LookRotation(look);
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = "Racion_veterinaria";
        Destroy(g.GetComponent<Collider>());
        g.transform.position = a.transform.position + d * 0.45f + Vector3.up * 0.25f;   // en su mano, delante del hocico
        g.transform.localScale = new Vector3(0.22f, 0.08f, 0.16f);
        g.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(new Color(0.72f, 0.3f, 0.3f));
        _ration = g.transform;
        _feedUntil = Time.time + 6f;
        a.hungry = Mathf.Max(0f, a.hungry - meal);
        _lastFed[a] = Time.time; Fed++;
        Debug.Log($"[Veterinaria] «{name}» da su ración de carne cultivada a «{a.name}» (ingresado) en la mano.");
        _claimed.Remove(a);
        _patient = null;
    }

    void Drop()
    {
        _best = float.MaxValue;
        if (_patient != null && _claimed.TryGetValue(_patient, out VetCare c) && c == this) _claimed.Remove(_patient);
        _patient = null;
        if (_ration != null) { Destroy(_ration.gameObject); _ration = null; }
    }

    Animal HungryPatient()
    {
        Animal best = null; float worst = hungerAbove;
        foreach (Animal a in FindObjectsByType<Animal>(FindObjectsSortMode.None))
        {
            if (a == null || a.death || a.hungry < worst) continue;
            if (!Admitted(a)) continue;
            if (_lastFed.TryGetValue(a, out float t) && Time.time - t < refeedAfter) continue;
            if (_claimed.TryGetValue(a, out VetCare other) && other != null && other != this && other.enabled) continue;
            worst = a.hungry; best = a;
        }
        return best;
    }

    /// <summary>¿Está ingresado? Con historia clínica, o en la planta veterinaria del hospital.</summary>
    public static bool Admitted(Animal a)
    {
        if (a == null) return false;
        if (a.GetComponent<HealthRecord>() != null) return true;
        if (Time.time - _vetFloorAt > 10f)
        {
            _vetFloorAt = Time.time; _vetFloor = null;
            GameObject f = GameObject.Find("Suelo_veterinaria");
            if (f != null && f.TryGetComponent(out Renderer r)) { Bounds b = r.bounds; b.Expand(new Vector3(0f, 6f, 0f)); _vetFloor = b; }
        }
        return _vetFloor.HasValue && _vetFloor.Value.Contains(a.transform.position);
    }
}
