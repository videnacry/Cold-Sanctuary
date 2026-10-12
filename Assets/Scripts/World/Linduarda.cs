using UnityEngine;

/// <summary>
/// LINDUARDA, la doctora (docs/character-linduarda, sanctuary1 §4) — hechicera avanzada cuyo **pensamiento custom** es
/// **asegurar la salud del santuario**. De él nace, como el guion EMERGE de pensamientos+hechizos (igual que la Magnate
/// enseña al rezagado o los peloteros deciden vivir juntos): cada cierto tiempo lanza un **status global** (sabe al
/// instante, por magia) y, a quien tenga su <see cref="HealthRecord"/> en `false`, **va y le lanza** el hechizo que le
/// falte — `VaccinateSpell` (vacunar) o `ClinicalAnalysisSpell` (muestras/análisis). Así, al empezar el juego, atiende a
/// Kushal (creado con ambos en false). No es scriptado: es su impulso, resuelto por los mismos hechizos de todos.
/// </summary>
[RequireComponent(typeof(Anima))]
public class Linduarda : MonoBehaviour
{
    [Min(1f)] public float surveyInterval = 8f;
    [Min(0.3f)] public float reach = 2.5f;
    [Min(0f)] public float moveSpeed = 4f;

    Anima _a;
    VaccinateSpell _vax;
    ClinicalAnalysisSpell _analysis;
    float _next;
    HealthRecord _patient;

    void Awake()
    {
        _a = GetComponent<Anima>();
        _vax = GetComponent<VaccinateSpell>() ?? gameObject.AddComponent<VaccinateSpell>();
        _analysis = GetComponent<ClinicalAnalysisSpell>() ?? gameObject.AddComponent<ClinicalAnalysisSpell>();
        _next = Time.time + 1f;   // al empezar el juego lanza el status global enseguida
    }

    void Update()
    {
        if (_a == null || _a.death) return;
        if (_patient == null && Time.time >= _next) { _next = Time.time + surveyInterval; _patient = Survey(); }
        if (_patient != null) GoAttend();
    }

    // STATUS GLOBAL (por magia, instantáneo): el HealthRecord más cercano que necesite cuidado.
    HealthRecord Survey()
    {
        HealthRecord best = null; float bestSq = float.MaxValue;
        foreach (HealthRecord h in FindObjectsOfType<HealthRecord>())
        {
            if (h == null || !h.NeedsCare || h.GetComponent<Linduarda>() != null) continue;
            float sq = (h.transform.position - transform.position).sqrMagnitude;
            if (sq < bestSq) { bestSq = sq; best = h; }
        }
        return best;
    }

    void GoAttend()
    {
        if (_patient == null || _patient.gameObject == null) { _patient = null; return; }
        Vector3 to = _patient.transform.position - transform.position; to.y = 0f;
        if (to.magnitude > reach) { transform.position += to.normalized * moveSpeed * Time.deltaTime; return; }

        ITarget it = _patient.GetComponentInParent<ITarget>();
        if (!_patient.analyzed)
        {
            if (it != null) _analysis.Cast(_a, it);   // toma muestras / analiza
            _patient.analyzed = true;
        }
        else if (!_patient.vaccinated)
        {
            if (it != null) _vax.Cast(_a, it);        // vacuna
            _patient.vaccinated = true;
        }
        if (!_patient.NeedsCare) { Debug.Log($"[Linduarda] atendió a «{_patient.name}» (vacunado + analizado)."); _patient = null; }
    }
}
