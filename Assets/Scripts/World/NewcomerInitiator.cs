using UnityEngine;

/// <summary>
/// INICIACIÓN DE RECIÉN LLEGADOS (docs/jornada-y-rotacion.md): un HECHIZO COMPUESTO («iniciar») que puede lanzar
/// CUALQUIERA que lo conozca; lo hace quien tenga la misión «Iniciación de recién llegados y turistas» en la jornada (al
/// empezar el juego, quien esté de turno; si nadie la tiene a esta hora, quien la tenga hoy; un clon de la Magnate si
/// faltan manos). Linduarda solo la hace sin jornada (pruebas).
/// Sus pasos, en orden:
/// 1. «Sígueme»: lleva al recién llegado a la SILLA DE EXÁMENES y lo sienta.
/// 2. Constantes (tensión, pulso, temperatura) · 3. pupilas con la linterna · 4. extracción de sangre (análisis) ·
/// 5. vacuna en el hombro · 6. las normas del santuario. Cada prueba con su POSTURA, la del que examina y la del examinado.
/// 7. Lo acompaña al TANQUE: su primera virtualización. Mientras no la haga, lo acompaña y se lo recuerda.
/// </summary>
public class NewcomerInitiator : MonoBehaviour
{
    public float reach = 2.5f, moveSpeed = 4f, stepSeconds = 3f;

    /// <summary>Quien lanza la iniciación ahora (lo fija la jornada) y quien la cubre sin jornada.</summary>
    public static NewcomerInitiator Current, Fallback;
    public static NewcomerInitiator Active => Current != null ? Current : Fallback;
    public bool IsActive => Active == this;

    /// <summary>¿Está ocupado con un recién llegado? (la jornada no lo mueve entonces).</summary>
    public bool Busy => IsActive && (_leading != null || _exam != null || Waiting() != null);

    VirtualizationMachine _machine;
    SanctuaryOnboarding _leading, _exam;
    int _step = -1; float _stepAt;
    float _nextReminder;
    Transform _chair;
    bool _frozePlayer;

    public string Who
    {
        get { Mind m = GetComponent<Mind>(); return m != null && !string.IsNullOrEmpty(m.identity) ? m.identity : name; }
    }

    VirtualizationMachine Machine()
    {
        if (_machine != null) return _machine;
        foreach (VirtualizationMachine vm in SceneCache.All<VirtualizationMachine>(2f))
            if (vm != null && vm.areaName == AreaWork.Hospital) return _machine = vm;
        return _machine = FindAnyObjectByType<VirtualizationMachine>();
    }

    Transform Chair()
    {
        if (_chair == null) { GameObject c = GameObject.Find("Silla_examen"); _chair = c != null ? c.transform : null; }
        return _chair;
    }

    // El recién llegado que espera su iniciación (sin certificar).
    static SanctuaryOnboarding Waiting()
    {
        foreach (SanctuaryOnboarding o in SceneCache.All<SanctuaryOnboarding>(0.5f))
            if (o != null && !o.apt) return o;
        return null;
    }

    void Update()
    {
        if (!IsActive) return;
        if (_exam != null) { Exam(); return; }
        if (_leading != null) { Lead(); return; }
        SanctuaryOnboarding o = Waiting();
        if (o == null) return;
        HealthRecord hr = o.GetComponent<HealthRecord>();
        if (hr != null && hr.NeedsCare) { StartExam(o); return; }
        if (!o.initiated) { Begin(o); return; }
        Accompany(o);
    }

    // ── EL EXAMEN (en la silla de exámenes) ─────────────────────────────────────────────────────────────────────
    void StartExam(SanctuaryOnboarding o)
    {
        _exam = o; _step = 0;
        Anima me = GetComponent<Anima>();
        ITarget it = o.GetComponentInParent<ITarget>();
        if (me != null && it != null) this.GetOrAdd<FollowSpell>().Cast(me, it);   // «sígueme»
        Say(o, "Bienvenido al santuario. Sígueme a la silla de exámenes: empezamos tu iniciación.");
        Debug.Log($"[Iniciación] {Who} lanza «iniciar» (hechizo compuesto) sobre «{o.name}»: primero, el examen en la silla.");
    }

    void Exam()
    {
        Transform chair = Chair();
        Transform p = _exam.transform;
        Anima me = GetComponent<Anima>(), pa = _exam.GetComponentInParent<Anima>();
        HealthRecord hr = _exam.GetComponent<HealthRecord>();
        if (_step == 0)   // ir a la silla (el examinador delante; el recién llegado lo sigue)
        {
            if (chair == null) { _step = 1; _stepAt = Time.time; return; }
            Vector3 stand = chair.position + chair.forward * 1.1f;
            Vector3 d = stand - transform.position; d.y = 0f;
            if (d.magnitude > 0.8f) { PathMover.Of(this).StepToward(stand, moveSpeed); return; }
            Vector3 pd = chair.position - p.position; pd.y = 0f;
            if (pd.magnitude > 2.2f) return;   // espera a que llegue
            Seat(p, chair, pa);
            transform.rotation = Quaternion.LookRotation(Flat(chair.position - transform.position));
            _step = 1; _stepAt = Time.time;
            return;
        }
        if (Time.time < _stepAt) return;   // (la postura se mantiene: las asanas bloquean los huesos)
        _stepAt = Time.time + stepSeconds;
        switch (_step)
        {
            case 1: Pose(me, DailyPoses.ExaminerHands(), pa, DailyPoses.PatientArmOut()); Say(_exam, "Tensión 120/80, pulso 68 y 36,6 °C: todo normal."); break;
            case 2: Pose(me, DailyPoses.ExaminerPenlight(), pa, DailyPoses.PatientHeadUp()); Say(_exam, "Mira a la luz… las pupilas se cierran iguales: bien."); break;
            case 3:
                Pose(me, DailyPoses.ExaminerHands(), pa, DailyPoses.PatientArmOut());
                ITarget it = _exam.GetComponentInParent<ITarget>();
                if (me != null && it != null) this.GetOrAdd<ClinicalAnalysisSpell>().Cast(me, it);
                if (hr != null) hr.analyzed = true;
                Say(_exam, "Un pinchacito en el pliegue del codo… ya está la muestra: va al analizador.");
                break;
            case 4:
                Pose(me, DailyPoses.ExaminerSyringe(), pa, DailyPoses.PatientShoulder());
                ITarget it2 = _exam.GetComponentInParent<ITarget>();
                if (me != null && it2 != null) this.GetOrAdd<VaccinateSpell>().Cast(me, it2);
                if (hr != null) hr.vaccinated = true;
                Say(_exam, "Y la vacuna, en el hombro. ¡Listo!");
                break;
            case 5:
                Stand(me); Stand(pa);
                Say(_exam, "Las normas: no se sale de la enfermería sin iniciación, se respeta a los animales y se sigue la jornada. Ahora, tu primera virtualización: sígueme al tanque.");
                break;
            default:
                Release(p);
                Debug.Log($"[Iniciación] {Who} termina el examen de «{_exam.name}» (constantes, pupilas, sangre, vacuna y normas).");
                SanctuaryOnboarding o = _exam; _exam = null; _step = -1;
                Begin(o);
                return;
        }
        _step++;
    }

    void Seat(Transform p, Transform chair, Anima pa)
    {
        if (p.GetComponent<PlayerController>() != null)
        {
            var pc = p.GetComponent<PlayerController>(); if (pc != null && !_frozePlayer) { pc.HoldBody(true); _frozePlayer = true; }   // sentado: la cámara sigue libre
        }
        WorkdayDirector.Teleport(p, chair.position + Vector3.up * 0.05f);
        p.rotation = chair.rotation;
        if (pa != null) DailyPoses.Sit().ApplyFinal(pa);
    }


    void Release(Transform p)
    {
        if (_frozePlayer) { var pc = p.GetComponent<PlayerController>(); if (pc != null) pc.HoldBody(false); _frozePlayer = false; }
    }

    static void Pose(Anima examiner, AsanaPoseSpell ex, Anima patient, AsanaPoseSpell pt)
    {
        if (examiner != null) ex.ApplyFinal(examiner);
        if (patient != null) pt.ApplyFinal(patient);
    }

    static void Stand(Anima a) { if (a != null) DailyPoses.Stand().ApplyFinal(a); }

    void Say(SanctuaryOnboarding o, string text)
    {
        Debug.Log($"[Iniciación] {Who}: «{text}»");
        if (o != null && o.GetComponent<PlayerController>() != null) LessonPrompt.Show($"{Who}: {text}", null, stepSeconds + 3f);
    }

    static Vector3 Flat(Vector3 v) { v.y = 0f; return v.sqrMagnitude > 1e-4f ? v : Vector3.forward; }

    // ── AL TANQUE: su primera virtualización ──────────────────────────────────────────────────────────────────
    void Begin(SanctuaryOnboarding o)
    {
        _leading = o;
        Anima me = GetComponent<Anima>();
        ITarget it = o.GetComponentInParent<ITarget>();
        if (me != null && it != null) this.GetOrAdd<FollowSpell>().Cast(me, it);   // «sígueme»
        Debug.Log($"[Iniciación] {Who} lleva a «{o.name}» al tanque: su primera virtualización.");
    }

    void Lead()
    {
        Anima t = _leading.GetComponentInParent<Anima>();
        VirtualizationMachine vm = Machine();
        if (t == null || vm == null) { _leading.initiated = true; _leading = null; return; }
        Vector3 to = vm.transform.position - transform.position; to.y = 0f;
        if (to.magnitude > reach) { PathMover.Of(this).StepToward(vm.transform.position, moveSpeed); return; }

        Debug.Log($"[Iniciación] {Who} explica a «{t.name}» el tanque de virtualización (máscara, entrar al agua) y lo envía al Microcosmos por primera vez.");
        if (t.GetComponent<PlayerController>() != null)
        {
            string spell = $"virtualize({{destiny:\"{vm.areaName}\"}})";
            LessonPrompt.Show($"{Who}: ponte la máscara y entra al tanque de virtualización", spell, 40f);
            SpellConsole.Propose(spell, $"{Who} te pide entrar al tanque", 60f);
        }
        FollowSpell.Stop(t);
        _leading.initiated = true;
        _leading = null;
    }

    // ACOMPAÑAR al que aún no ha hecho su primera virtualización: lo sigue y se lo recuerda cada cierto tiempo.
    void Accompany(SanctuaryOnboarding o)
    {
        Vector3 to = o.transform.position - transform.position; to.y = 0f;
        if (to.magnitude > reach + 0.8f) PathMover.Of(this).StepToward(o.transform.position, moveSpeed);
        if (Time.time < _nextReminder || o.GetComponent<PlayerController>() == null) return;
        _nextReminder = Time.time + 25f;
        VirtualizationMachine vm = Machine();
        string spell = vm != null ? $"virtualize({{destiny:\"{vm.areaName}\"}})" : "virtualize";
        LessonPrompt.Show($"{Who}: aún te falta tu primera virtualización. Ven al tanque, ponte la máscara y entra.", spell, 20f);
        SpellConsole.Propose(spell, $"{Who} te espera en el tanque", 25f);
        Debug.Log($"[Iniciación] {Who} recuerda al recién llegado que le falta su primera virtualización (lo acompaña por la enfermería).");
    }

    void OnDestroy() { if (Current == this) Current = null; if (Fallback == this) Fallback = null; }
}
