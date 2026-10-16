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

    [Tooltip("Máquina de virtualización a la que lleva al recién llegado (iniciación al Micro). Vacío = la busca en escena.")]
    public VirtualizationMachine virtualizationMachine;

    Anima _a;
    VaccinateSpell _vax;
    ClinicalAnalysisSpell _analysis;
    FollowSpell _follow;
    float _next;
    HealthRecord _patient;
    SanctuaryOnboarding _initiate;

    void Awake()
    {
        _a = GetComponent<Anima>();
        _vax = GetComponent<VaccinateSpell>() ?? gameObject.AddComponent<VaccinateSpell>();
        _analysis = GetComponent<ClinicalAnalysisSpell>() ?? gameObject.AddComponent<ClinicalAnalysisSpell>();
        _follow = GetComponent<FollowSpell>() ?? gameObject.AddComponent<FollowSpell>();
        _next = Time.time + 1f;   // al empezar el juego lanza el status global enseguida
    }

    void Update()
    {
        if (_a == null || _a.death) return;
        if (_initiate != null) { Initiate(); return; }   // llevando a un recién llegado al Micro: prioridad
        if (_patient == null && Time.time >= _next) { _next = Time.time + surveyInterval; _patient = Survey(); Certify(); }
        if (_patient != null) GoAttend();
    }

    // INICIACIÓN: tras curar a un recién llegado, lo lleva a la máquina de virtualización y lo envía al Microcosmos.
    // (Parte de su pensamiento de salud: analizar → mientras se procesa, iniciarlo en el Micro.) Usa FollowSpell para guiarlo.
    void Initiate()
    {
        Anima t = _initiate.GetComponentInParent<Anima>();
        if (virtualizationMachine == null) virtualizationMachine = FindObjectOfType<VirtualizationMachine>();
        if (t == null || virtualizationMachine == null) { _initiate.initiated = true; _initiate = null; return; }

        Vector3 to = virtualizationMachine.transform.position - transform.position; to.y = 0f;
        if (to.magnitude > reach) { transform.position += to.normalized * moveSpeed * Time.deltaTime; return; }  // la doctora va a la máquina (el nuevo la sigue)

        Debug.Log($"[Linduarda] inicia a «{t.name}»: le explica la máquina de virtualización y lo envía al Microcosmos.");
        if (virtualizationMachine.CanInteract) virtualizationMachine.Interact();
        FollowSpell.Stop(t);
        _initiate.initiated = true;
        _initiate = null;
    }

    // Segundo impulso de la doctora: CERTIFICAR APTO a quien ya está sano y completó sus 3 misiones introductorias.
    // Instantáneo (es su juicio médico, no requiere desplazarse). De aquí nace que la Magnate lo presente.
    void Certify()
    {
        foreach (SanctuaryOnboarding o in FindObjectsOfType<SanctuaryOnboarding>())
            if (o != null && o.ReadyForApt) o.CertifyApt();
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
        if (!_patient.NeedsCare)
        {
            Debug.Log($"[Linduarda] atendió a «{_patient.name}» (vacunado + analizado).");
            // Si es un recién llegado sin iniciar, nace la iniciación: lo hace seguirla a la máquina de virtualización.
            SanctuaryOnboarding onb = _patient.GetComponent<SanctuaryOnboarding>();
            if (onb != null && !onb.initiated)
            {
                _initiate = onb;
                if (it != null) _follow.Cast(_a, it);   // «sígueme»
            }
            _patient = null;
        }
    }
}
