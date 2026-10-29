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

    /// <summary>Lo que hace ahora por su cuenta (llevar una muestra, atender a alguien), o null. La jornada lo muestra en su cartel.</summary>
    public string Doing => _sample != null ? $"lleva la muestra de «{_sampleOf}» al analizador" : _patient != null ? $"atiende a «{_patient.name}»" : null;

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
        NewcomerInitiator ni = GetComponent<NewcomerInitiator>();
        if (ni != null && ni.InProgress) return;   // está iniciando a un recién llegado: primero eso (si no, se iba a atender a otro)
        // Sus NECESIDADES van antes (comer, baño, dormir): JornadaTest 2026-10-29, Linduarda iba y venía en alfombra entre la
        // barra del restaurante y la perrera del lobo, una vez cada conductor, y nunca llegaba a sentarse a comer.
        if (TryGetComponent(out ActionQueue q) && q.Own.Count > 0 && (q.Own[0] is ToiletSpell || q.Own[0] is DineSpell || q.Own[0] is SleepSpell || q.Own[0] is EatSpell || q.Own[0] is CarpetWalkSpell)) return;
        if (_a.asleep) return;
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

    float _sampleBest = float.MaxValue, _sampleSince;
    void CarrySample()
    {
        ClinicalStation st = FindObjectOfType<ClinicalStation>();
        if (st == null) { Destroy(_sample.gameObject); _sample = null; return; }
        Vector3 to = st.transform.position - transform.position; to.y = 0f;
        bool level = Mathf.Abs(st.transform.position.y - transform.position.y) < 2f;
        if (!level) { _sampleBest = float.MaxValue; _sampleSince = Time.time; }
        else if (to.magnitude < _sampleBest - 0.5f) { _sampleBest = to.magnitude; _sampleSince = Time.time; }
        // (sin acercarse 6 s a menos de 4 m: el analizador está contra la pared y su centro no se alcanza; rondaba el mueble
        // 42 m en 20 s — JornadaTest 2026-10-29. Desde ahí ya llega con la mano.)
        bool there = level && (to.magnitude <= reach || (to.magnitude < 4f && Time.time - _sampleSince > 6f));
        if (!there) { PathMover.Of(this).StepToward(st.transform.position, moveSpeed); return; }
        _sampleBest = float.MaxValue;
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
            if (NewcomerInitiator.Active != null && h.TryGetComponent(out SanctuaryOnboarding ob) && !ob.apt) continue;   // a los recién llegados los examina quien hace la iniciación
            float sq = (h.transform.position - transform.position).sqrMagnitude;
            if (sq < bestSq) { bestSq = sq; best = h; }
        }
        return best;
    }

    // SIN ACERCARSE (el lobo está en su perrera: no se llega a 2,5 m de su centro y rondaba la perrera dando vueltas): a
    // menos de 4 m, si en 6 s no se acerca medio metro, le atiende desde ahí (por los barrotes); más lejos, a los 10 s lo deja
    // para la próxima ronda.
    float _bestGap = float.MaxValue, _gapSince;
    bool CloseEnough(float gap)
    {
        if (gap < _bestGap - 0.5f) { _bestGap = gap; _gapSince = Time.time; return false; }
        if (Time.time - _gapSince < (gap < 4f ? 6f : 10f)) return false;
        if (gap < 4f) return true;
        Debug.Log($"[Linduarda] no llega hasta «{_patient.name}» (a {gap:0.0} m): lo deja para la próxima ronda.");
        _patient = null; _bestGap = float.MaxValue; _next = Time.time + 30f;
        return false;
    }

    void GoAttend()
    {
        if (_patient == null || _patient.gameObject == null) { _patient = null; return; }
        Vector3 to = _patient.transform.position - transform.position; to.y = 0f;
        bool level = Mathf.Abs(_patient.transform.position.y - transform.position.y) < 2f;
        if (!level || (to.magnitude > reach && !CloseEnough(to.magnitude)))
        {
            if (_patient == null) return;   // (no llega: lo dejó para la próxima ronda)
            if (!level) { _bestGap = float.MaxValue; _gapSince = Time.time; }   // (de camino en alfombra: aún no cuenta)
            PathMover.Of(this).StepToward(_patient.transform.position, moveSpeed); return;
        }
        _bestGap = float.MaxValue;

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
