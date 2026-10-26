using UnityEngine;

/// <summary>
/// INICIACIÓN DE RECIÉN LLEGADOS (docs/jornada-y-rotacion.md, docs/iniciacion-medica-investigacion.md): un HECHIZO
/// COMPUESTO («iniciar») que puede lanzar CUALQUIERA que lo conozca; lo hace quien tenga la misión «Iniciación de recién
/// llegados y turistas» en la jornada (al empezar el juego, quien esté de turno; si nadie la tiene a esta hora, quien la
/// tenga hoy; un clon de la Magnate si faltan manos). Linduarda solo la hace sin jornada (pruebas).
/// Lo que se ve, como una animación (Berón 2026-10-26):
/// 1. El trabajador VA HACIA el recién llegado (todos los demás ya están iniciados), lo saluda y lo LLEVA de la mano.
/// 2. El EXAMEN DE LLEGADA (<see cref="ArrivalExam.Beats"/>): en cada sitio (la silla, el lavabotas, el lavamanos, la
///    camilla, la mesa) lo coloca sentado, de pie o tumbado, y hace el gesto con su herramienta en la mano, alternando dos
///    posturas; el recién llegado adopta la suya. Sentado, el jugador pasa a la cámara en 1.ª persona.
/// 3. Lo lleva al TANQUE, lo mete dentro y lo envía a su primer microcosmos: Ambrosio (<see cref="FirstMicrocosmos"/>).
/// Mientras no vuelva de él, lo acompaña y se lo recuerda.
/// </summary>
public class NewcomerInitiator : MonoBehaviour
{
    public float reach = 2.5f, moveSpeed = 3.2f;
    /// <summary>Escena del primer microcosmos del recién llegado (la historia de Ambrosio).</summary>
    public const string FirstMicrocosmos = "Microcosmos_Scene1_Ambrosio";

    /// <summary>Quien lanza la iniciación ahora (lo fija la jornada) y quien la cubre sin jornada.</summary>
    public static NewcomerInitiator Current, Fallback;
    /// <summary>Quien la lanza ahora. Quien ya la empezó la TERMINA aunque cambie el turno.</summary>
    public static NewcomerInitiator Active
    {
        get
        {
            if (Current != null && Current.InProgress) return Current;
            if (Fallback != null && Fallback.InProgress) return Fallback;
            return Current != null ? Current : Fallback;
        }
    }
    /// <summary>¿Tiene una iniciación a medias (examen o camino al tanque)?</summary>
    public bool InProgress => _exam != null || _leading != null;
    public bool IsActive => Active == this;
    /// <summary>¿Está ocupado con un recién llegado? (la jornada no lo mueve entonces).</summary>
    public bool Busy => IsActive && (_leading != null || _exam != null || Waiting() != null);

    /// <summary>Para el test de experiencia: el paso del examen en curso (null si no hay), y en qué fase va.</summary>
    public ArrivalExam.Beat CurrentBeat => _exam != null && _beat >= 0 && _beat < ArrivalExam.Beats.Length ? ArrivalExam.Beats[_beat] : null;
    public enum Phase { None, Approach, Escort, Perform, ToTank }
    public Phase Now { get; private set; }
    public int BeatsDone { get; private set; }

    VirtualizationMachine _machine;
    SanctuaryOnboarding _leading, _exam;
    int _beat = -1; float _beatEnd, _nextFrame; bool _frameB;
    float _nextReminder, _sentAt = -999f;
    bool _frozePlayer, _camForced;
    CameraMode _camBefore;

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

    // Los sitios del examen, dentro del hospital (hay «Camilla» y «Lavamanos» en otras áreas).
    static readonly System.Collections.Generic.Dictionary<string, Transform> _places = new System.Collections.Generic.Dictionary<string, Transform>();
    static Transform Place(string name)
    {
        if (_places.TryGetValue(name, out Transform c) && c != null) return c;
        GameObject h = GameObject.Find("Infirmary_Area");
        Transform t = h != null ? VirtualInPlace.FindDeep(h.transform, name) : null;
        if (t != null) return _places[name] = t;
        GameObject g = GameObject.Find(name); return g != null ? g.transform : null;
    }

    // El recién llegado que espera su iniciación (sin certificar).
    static SanctuaryOnboarding Waiting()
    {
        foreach (SanctuaryOnboarding o in SceneCache.All<SanctuaryOnboarding>(0.5f))
            if (o != null && !o.apt) return o;
        return null;
    }

    /// <summary>Con las pruebas corriendo, la iniciación espera (si no, se llevaba al jugador en mitad de otras pruebas); el test
    /// de experiencia la deja actuar.</summary>
    public static bool AllowDuringTests;

    /// <summary>Olvida cualquier iniciación a medias (para empezar de cero, p. ej. en el test de experiencia).</summary>
    public void ResetState()
    {
        if (_exam != null) Hold(_exam.transform, false); else if (_leading != null) Hold(_leading.transform, false);
        _exam = null; _leading = null; _beat = -1; BeatsDone = 0; Now = Phase.None;
    }

    void Update()
    {
        if (!IsActive || (ColdSanctuary.TestRunner.Running && !AllowDuringTests)) return;
        if (_exam != null) { Exam(); return; }
        if (_leading != null) { Lead(); return; }
        SanctuaryOnboarding o = Waiting();
        if (o == null) return;
        HealthRecord hr = o.GetComponent<HealthRecord>();
        if (hr != null && hr.NeedsCare) { StartExam(o); return; }
        if (!o.initiated) { Begin(o); return; }
        Accompany(o);
    }

    // ── 1. IR HACIA ÉL Y LLEVARLO ───────────────────────────────────────────────────────────────────────────────
    void StartExam(SanctuaryOnboarding o)
    {
        _exam = o; _beat = 0; BeatsDone = 0; Now = Phase.Approach;
        Debug.Log($"[Iniciación] {Who} lanza «iniciar» (hechizo compuesto) sobre «{o.name}»: va hacia el recién llegado.");
    }

    /// <summary>Mueve al acompañado detrás del que guía (lo lleva de la mano), sin que el jugador lo controle.</summary>
    void Escort(Transform p, float dt)
    {
        Hold(p, true);
        Vector3 behind = transform.position - transform.forward * 1.0f + transform.right * 0.4f; behind.y = p.position.y;
        Vector3 d = behind - p.position; d.y = 0f;
        if (d.magnitude > 0.05f) WorkdayDirector.Teleport(p, Vector3.MoveTowards(p.position, behind, moveSpeed * 1.1f * dt));
        Vector3 look = transform.position - p.position; look.y = 0f;
        if (look.sqrMagnitude > 0.01f) p.rotation = Quaternion.Slerp(p.rotation, Quaternion.LookRotation(look), dt * 6f);
    }

    void Exam()
    {
        Transform p = _exam.transform;
        Anima me = GetComponent<Anima>(), pa = _exam.GetComponentInParent<Anima>();
        if (Now == Phase.Approach)
        {
            Vector3 d = p.position - transform.position;
            float gap = d.magnitude; d.y = 0f;   // en 3D: en otro piso no está «cerca» aunque esté encima
            if (gap > 1.6f) { PathMover.Of(this).StepToward(p.position, moveSpeed); return; }
            transform.rotation = Quaternion.LookRotation(Flat(d));
            Say(_exam, "Bienvenido al santuario. Soy quien te hace la iniciación: ven conmigo, te llevo yo.", 6f);
            Now = Phase.Escort;
            return;
        }
        if (_beat >= ArrivalExam.Beats.Length) { FinishExam(p, me, pa); return; }
        ArrivalExam.Beat b = ArrivalExam.Beats[_beat];
        Transform place = Place(b.place);
        if (Now == Phase.Escort)
        {
            Stand(pa); Stand(me);
            if (place == null) { Now = Phase.Perform; _beatEnd = Time.time + b.seconds; return; }
            Vector3 fr = Front(place), side = Vector3.Cross(Vector3.up, fr);
            Vector3 stand = b.patient == ArrivalExam.Spot.Stand ? place.position + fr * 0.7f + side * 0.9f : place.position + fr * 0.9f + side * 0.5f;
            Vector3 d = stand - transform.position; d.y = 0f;
            Escort(p, Time.deltaTime);
            if (d.magnitude > 0.6f) { PathMover.Of(this).StepToward(stand, moveSpeed); return; }
            Put(p, pa, place, b.patient);
            transform.rotation = Quaternion.LookRotation(Flat(p.position - transform.position));
            ArrivalExam.Hold(gameObject, b.tool, BodyPart.HandRight, "Herramienta");
            ArrivalExam.Hold(p.gameObject, b.patientTool, b.patientBone, "Herramienta_paciente");
            Say(_exam, b.line, b.seconds);
            Debug.Log($"[Iniciación] {Who} · {b.title} ({b.seconds:0} s, en «{b.place}»; protege a: {b.protects}).");
            Now = Phase.Perform; _beatEnd = Time.time + b.seconds; _nextFrame = 0f; _frameB = false;
            return;
        }
        // Phase.Perform: el gesto, alternando las dos posturas.
        if (Time.time >= _nextFrame)
        {
            _nextFrame = Time.time + b.period;
            if (me != null) (_frameB ? b.exB : b.exA)?.Invoke().ApplyFinal(me);
            PlayerController pc = p.GetComponent<PlayerController>();
            if (pc != null) pc.LookAtPoint(transform.position + Vector3.up * (b.patient == ArrivalExam.Spot.Stand ? 0.8f : 1.1f));   // en 1.ª persona se ve el gesto
            if (pa != null) (_frameB ? b.ptB : b.ptA)?.Invoke().ApplyFinal(pa);
            _frameB = !_frameB;
        }
        if (Time.time < _beatEnd) return;
        HealthRecord hr = _exam.GetComponent<HealthRecord>();
        if (b.id == "hisopo" && hr != null) hr.analyzed = true;
        if (b.id == "vacuna" && hr != null) { hr.vaccinated = true; ITarget it = _exam.GetComponentInParent<ITarget>(); if (me != null && it != null) this.GetOrAdd<VaccinateSpell>().Cast(me, it); }
        ArrivalExam.Hold(gameObject, null, BodyPart.HandRight, "Herramienta");
        ArrivalExam.Hold(p.gameObject, null, b.patientBone, "Herramienta_paciente");
        BeatsDone++; _beat++; Now = Phase.Escort;
    }

    // Coloca al examinado en el sitio del paso: sentado en la silla, tumbado en la camilla o de pie delante.
    void Put(Transform p, Anima pa, Transform place, ArrivalExam.Spot spot)
    {
        Hold(p, true);
        Vector3 fr = Front(place);
        Vector3 at = spot == ArrivalExam.Spot.Stand ? place.position + fr * 0.7f : place.position + Vector3.up * (spot == ArrivalExam.Spot.Lie ? 0.75f : 0.05f);
        WorkdayDirector.Teleport(p, at);
        p.rotation = spot == ArrivalExam.Spot.Stand ? Quaternion.LookRotation(-fr) : spot == ArrivalExam.Spot.Sit ? Quaternion.LookRotation(fr) : place.rotation;
        if (pa != null) (spot == ArrivalExam.Spot.Sit ? DailyPoses.Sit() : spot == ArrivalExam.Spot.Lie ? DailyPoses.Lie() : DailyPoses.Stand()).ApplyFinal(pa);
        // Sentado o tumbado, el jugador lo vive en 1.ª persona (Berón 2026-10-26).
        if (spot != ArrivalExam.Spot.Stand && !_camForced && p.GetComponent<PlayerController>() != null && CameraManager.Instance != null)
        {
            _camBefore = CameraManager.Instance.preferredMode; _camForced = true;
            CameraManager.Instance.SwitchTo(CameraMode.FirstPerson, forced: true);
        }
    }

    void FinishExam(Transform p, Anima me, Anima pa)
    {
        Stand(me); Stand(pa);
        if (_camForced && CameraManager.Instance != null) { CameraManager.Instance.SwitchTo(_camBefore, forced: true); _camForced = false; }
        Debug.Log($"[Iniciación] {Who} termina el examen de llegada de «{_exam.name}» ({BeatsDone} pasos): ahora, al tanque.");
        SanctuaryOnboarding o = _exam; _exam = null; _beat = -1;
        Begin(o);
    }

    void Hold(Transform p, bool on)
    {
        var pc = p != null ? p.GetComponent<PlayerController>() : null;
        if (pc == null) return;
        if (on && !_frozePlayer) { pc.HoldBody(true); _frozePlayer = true; }
        else if (!on && _frozePlayer) { pc.HoldBody(false); _frozePlayer = false; }
    }

    static void Stand(Anima a) { if (a != null) DailyPoses.Stand().ApplyFinal(a); }

    void Say(SanctuaryOnboarding o, string text, float secs)
    {
        Debug.Log($"[Iniciación] {Who}: «{text}»");
        if (o != null && o.GetComponent<PlayerController>() != null) LessonPrompt.Show($"{Who}: {text}", null, secs + 2f);
    }

    static Vector3 Flat(Vector3 v) { v.y = 0f; return v.sqrMagnitude > 1e-4f ? v : Vector3.forward; }

    /// <summary>El lado «de uso» de un mueble: de sus cuatro lados, el que da al interior del área (hacia su centro), que es
    /// por donde se le acerca uno. No depende de cómo se construyó cada mueble.</summary>
    static Vector3 Front(Transform place)
    {
        SanctuaryArea a = place.GetComponentInParent<SanctuaryArea>();
        Vector3 toC = Flat(a != null ? a.transform.position - place.position : -place.forward).normalized;
        Vector3 best = place.forward; float bd = -2f;
        foreach (Vector3 c in new[] { place.forward, -place.forward, place.right, -place.right }) { Vector3 f = Flat(c).normalized; float d = Vector3.Dot(f, toC); if (d > bd) { bd = d; best = f; } }
        return best;
    }

    // ── 3. AL TANQUE: lo mete dentro y lo envía a su primer microcosmos ──────────────────────────────────────────
    void Begin(SanctuaryOnboarding o)
    {
        _leading = o; Now = Phase.ToTank;
        Debug.Log($"[Iniciación] {Who} lleva a «{o.name}» al tanque: su primera virtualización.");
    }

    void Lead()
    {
        Transform p = _leading.transform;
        VirtualizationMachine vm = Machine();
        if (vm == null) { _leading.initiated = true; _leading = null; Hold(p, false); Now = Phase.None; return; }
        Vector3 to = vm.transform.position - transform.position; to.y = 0f;
        if (to.magnitude > reach) { Escort(p, Time.deltaTime); PathMover.Of(this).StepToward(vm.transform.position, moveSpeed); return; }
        Hold(p, false);
        _leading.initiated = true; _leading = null; Now = Phase.None;
        if (p.GetComponent<PlayerController>() != null && MobWorldLoader.HasInstance)
        {
            Say(o: null, $"Ponte la máscara y entra al agua: te espera Ambrosio, en el microcosmos.", 4f);
            LessonPrompt.Show($"{Who}: ponte la máscara y entra al agua: te espera Ambrosio, en el microcosmos.", null, 6f);
            Debug.Log($"[Iniciación] {Who} mete a «{p.name}» en el tanque y lo envía a «{FirstMicrocosmos}».");
            vm.EnterWithTank(p, Application.CanStreamedLevelBeLoaded(FirstMicrocosmos) ? FirstMicrocosmos : null);
            _sentAt = Time.time;
        }
    }

    // ACOMPAÑAR al que aún no ha hecho su primera virtualización: lo sigue y se lo recuerda cada cierto tiempo.
    void Accompany(SanctuaryOnboarding o)
    {
        Vector3 to = o.transform.position - transform.position; to.y = 0f;
        if (to.magnitude > reach + 0.8f) PathMover.Of(this).StepToward(o.transform.position, moveSpeed);
        if (Time.time - _sentAt < 120f || (MobWorldLoader.HasInstance && (MobWorldLoader.Instance.IsInMobWorld || MobWorldLoader.Instance.IsBusy))) return;   // ya va (o está) en el microcosmos
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
