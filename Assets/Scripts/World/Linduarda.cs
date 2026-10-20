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
    float _next;
    HealthRecord _patient;

    void Awake()
    {
        _a = GetComponent<Anima>();
        MobWorldLoader.Exited += OnBackFromMicro;
        _vax = this.GetOrAdd<VaccinateSpell>();
        _analysis = this.GetOrAdd<ClinicalAnalysisSpell>();
        NewcomerInitiator.Fallback = this.GetOrAdd<NewcomerInitiator>();   // fuera de la jornada, la iniciación la hace ella
        _next = Time.time + 1f;   // al empezar el juego lanza el status global enseguida
    }

    void OnDestroy() { MobWorldLoader.Exited -= OnBackFromMicro; }

    // Al VOLVER de su primer microcosmos, el recién llegado ha completado la iniciación de la doctora: lo certifica y
    // LLAMA a la Magnate para que lo inicie ella (hasta ahora la Magnate no lo consideraba del santuario).
    void OnBackFromMicro(string scene)
    {
        foreach (SanctuaryOnboarding o in FindObjectsOfType<SanctuaryOnboarding>())
        {
            if (o == null || !o.initiated || o.apt) continue;
            o.CompleteMission($"primer viaje al microcosmos ({scene})");
            if (o.ReadyForApt) { o.CertifyApt(); Debug.Log($"[Linduarda] termina la iniciación de «{o.name}» y llama a la Magnate para que lo inicie."); }
        }
    }

    void Update()
    {
        if (_a == null || _a.death) return;
        if (_sample != null) { CarrySample(); return; }   // llevando la muestra al analizador
        if (_patient == null && Time.time >= _next) { _next = Time.time + surveyInterval; _patient = Survey(); Certify(); }
        if (_patient != null) GoAttend();
    }

    // MUESTRAS: Linduarda extrae sangre (un tubo de ensayo en su mano) y la lleva al analizador clínico de la enfermería.
    Transform _sample;
    string _sampleOf;

    void TakeSample(string patient)
    {
        _sampleOf = patient;
        Transform hand = GetComponent<CreatureRig>()?.Get(BodyPart.HandRight);
        _sample = KitchenProps.Root(hand != null ? hand : transform, "Muestra_sangre", hand != null ? new Vector3(0f, -0.08f, 0.04f) : new Vector3(0.3f, 1f, 0.3f));
        KitchenProps.C(_sample, new Vector3(0f, 0.05f, 0f), new Vector3(0.025f, 0.1f, 0.025f), new Color(0.88f, 0.3f, 0.3f), 0f, "Tubo");
        KitchenProps.C(_sample, new Vector3(0f, 0.11f, 0f), new Vector3(0.03f, 0.02f, 0.03f), new Color(0.6f, 0.75f, 0.95f), 0f, "Tapon");
        Debug.Log($"[Linduarda] toma una muestra de sangre a «{patient}» para analizarla.");
    }

    void CarrySample()
    {
        ClinicalStation st = FindObjectOfType<ClinicalStation>();
        if (st == null) { Destroy(_sample.gameObject); _sample = null; return; }
        Vector3 to = st.transform.position - transform.position; to.y = 0f;
        if (to.magnitude > reach) { PathMover.Of(this).StepToward(st.transform.position, moveSpeed); return; }
        Destroy(_sample.gameObject); _sample = null;
        Debug.Log($"[Clínico] Linduarda analiza la muestra de «{_sampleOf}» en el analizador: apto para seguir (falta la vacuna).");
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
        if (to.magnitude > reach) { PathMover.Of(this).StepToward(_patient.transform.position, moveSpeed); return; }

        ITarget it = _patient.GetComponentInParent<ITarget>();
        if (!_patient.analyzed)
        {
            if (it != null) _analysis.Cast(_a, it);   // toma muestras / analiza
            _patient.analyzed = true;
            TakeSample(_patient.name);                // la muestra (un tubo) va en su mano hasta el analizador
        }
        else if (!_patient.vaccinated)
        {
            if (it != null) _vax.Cast(_a, it);        // vacuna
            _patient.vaccinated = true;
        }
        if (!_patient.NeedsCare)
        {
            Debug.Log($"[Linduarda] atendió a «{_patient.name}» (vacunado + analizado).");
            // Si es un recién llegado, sigue su INICIACIÓN quien tenga hoy esa misión (NewcomerInitiator), no siempre ella.
            _patient = null;
        }
    }
}
