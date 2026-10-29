using UnityEngine;

/// <summary>
/// INICIACIÓN DE RECIÉN LLEGADOS (docs/jornada-y-rotacion.md, docs/iniciacion-medica-investigacion.md): un HECHIZO
/// COMPUESTO («iniciar») que puede lanzar CUALQUIERA que lo conozca; lo hace quien tenga la misión «Iniciación de recién
/// llegados y turistas» en la jornada (al empezar el juego, quien esté de turno; si nadie la tiene a esta hora, quien la
/// tenga hoy; un clon de la Magnate si faltan manos). Linduarda solo la hace sin jornada (pruebas).
/// Lo que se ve, como una animación (Berón 2026-10-26) y, desde 2026-10-29, como una ORQUESTA (<see cref="Orchestra"/>):
/// 1. El trabajador VA HACIA el recién llegado (todos los demás ya están iniciados) hasta 2 m y lo saluda: ahí empieza la
///    orquesta y cada uno va SOLO a sus sitios (ya no lo lleva de la mano a tirones).
/// 2. El EXAMEN DE LLEGADA (<see cref="ArrivalExam.Beats"/>): en cada paso el recién llegado anda solo a su sitio (la silla,
///    el lavabotas, el lavamanos, la camilla, la mesa), se sienta, se queda de pie o se tumba y da la señal «en su sitio»;
///    el trabajador, ya en el suyo, la espera, hace su gesto con la herramienta en la mano (dos posturas alternas, A/B) y al
///    acabar da «hecho»: entonces los dos pasan al paso siguiente. Sentado, el jugador pasa a la cámara en 1.ª persona.
/// 3. Al TANQUE: los dos andan hasta él; cuando el recién llegado está delante, el trabajador lo mete dentro y lo envía a su
///    primer microcosmos: Ambrosio (<see cref="FirstMicrocosmos"/>). Mientras no vuelva de él, lo acompaña y se lo recuerda.
/// </summary>
public class NewcomerInitiator : MonoBehaviour
{
    public float reach = 2.5f, moveSpeed = 3.2f;
    [Tooltip("Paso de andar dentro de la orquesta (m/s): el ciclo de andar mueve las piernas.")]
    public float walkSpeed = 1.8f;
    /// <summary>A qué distancia (como mucho 2 m) tiene que llegar el trabajador para empezar la orquesta.</summary>
    public const float StartDistance = 1.6f;
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
    public bool InProgress => _subject != null;
    public bool IsActive => Active == this;
    /// <summary>¿Está ocupado con un recién llegado? (la jornada no lo mueve entonces).</summary>
    public bool Busy => InProgress || (IsActive && Waiting() != null);   // (quien la empezó, aunque ya no sea «el activo»)

    /// <summary>Para el test de experiencia: el paso del examen en curso (null si no hay), y en qué fase va.</summary>
    public ArrivalExam.Beat CurrentBeat => _subject != null && _beat >= 0 && _beat < ArrivalExam.Beats.Length ? ArrivalExam.Beats[_beat] : null;
    /// <summary>Approach: va hacia él · Escort: la orquesta, cada uno andando a su sitio · Perform: el trabajador hace el gesto
    /// del paso · ToTank: el último movimiento, al tanque.</summary>
    public enum Phase { None, Approach, Escort, Perform, ToTank }
    public Phase Now { get; private set; }
    public int BeatsDone { get; private set; }
    /// <summary>A qué distancia del recién llegado estaba al saludarlo (−1: aún no).</summary>
    public float GreetedAtDistance { get; private set; } = -1f;
    /// <summary>¿El recién llegado está sentado o tumbado ahora (no de pie)?</summary>
    public bool PatientDown { get; private set; }
    /// <summary>La orquesta en curso (null si no hay).</summary>
    public Orchestra Score => _orch;

    VirtualizationMachine _machine;
    SanctuaryOnboarding _subject;
    bool _withExam;
    int _beat = -1; float _aimUntil;
    float _nextReminder, _sentAt = -999f;
    bool _camForced;
    CameraMode _camBefore;
    Orchestra _orch;
    Orchestra.Role _rWorker, _rNew;

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

    // ¿Ya lo está iniciando otro? (uno a la vez)
    static bool Handled(SanctuaryOnboarding o)
    {
        foreach (NewcomerInitiator n in SceneCache.All<NewcomerInitiator>(0.5f)) if (n != null && n._subject == o) return true;
        return false;
    }

    // El recién llegado que espera su iniciación (sin certificar).
    static SanctuaryOnboarding Waiting()
    {
        foreach (SanctuaryOnboarding o in SceneCache.All<SanctuaryOnboarding>(0.5f))
            if (o != null && !o.apt && !Handled(o)) return o;
        return null;
    }

    /// <summary>Con las pruebas corriendo, la iniciación espera (si no, se llevaba al jugador en mitad de otras pruebas); el test
    /// de experiencia la deja actuar.</summary>
    public static bool AllowDuringTests;

    /// <summary>Olvida cualquier iniciación a medias (para empezar de cero, p. ej. en el test de experiencia).</summary>
    public void ResetState()
    {
        _orch?.Stop(); _orch = null;
        if (_subject != null) { EyesAtHead(_subject.GetComponent<PlayerController>(), false); ArrivalExam.Hold(_subject.gameObject, null, BodyPart.HandRight, "Herramienta_paciente"); }
        if (_camForced && CameraManager.Instance != null) CameraManager.Instance.SwitchTo(_camBefore, forced: true);
        _camForced = false;
        ArrivalExam.Hold(gameObject, null, BodyPart.HandRight, "Herramienta");
        _subject = null; _beat = -1; BeatsDone = 0; Now = Phase.None; PatientDown = false;
    }

    void Update()
    {
        if (ColdSanctuary.TestRunner.Running && !AllowDuringTests) return;
        // Quien ya ha empezado una iniciación la TERMINA aunque acabe su turno (antes se quedaba congelada a medias y el de
        // turno siguiente empezaba otra con el mismo recién llegado: dos a la vez).
        if (!IsActive && !InProgress) return;
        if (_subject != null) { Run(); return; }
        SanctuaryOnboarding o = Waiting();
        if (o == null) return;
        HealthRecord hr = o.GetComponent<HealthRecord>();
        if (hr != null && hr.NeedsCare) { Launch(o, true); return; }
        if (!o.initiated) { Launch(o, false); return; }
        Accompany(o);
    }

    // Quien está quieto en su sitio no se mueve (su jornada o su navegación lo paseaban a mitad del gesto).
    void LateUpdate() { if (_orch != null) _orch.LateTick(); }

    // ── 1. IR HACIA ÉL (hasta 2 m): ahí empieza la orquesta ──────────────────────────────────────────────────────
    void Launch(SanctuaryOnboarding o, bool exam)
    {
        _subject = o; _withExam = exam; _beat = 0; BeatsDone = 0; Now = Phase.Approach; GreetedAtDistance = -1f; PatientDown = false;
        Debug.Log($"[Iniciación] {Who} lanza «iniciar» (hechizo compuesto) sobre «{o.name}»: va hacia el recién llegado{(exam ? "" : " (ya examinado: al tanque)")}.");
    }

    void Run()
    {
        Transform p = _subject.transform;
        if (Now == Phase.Approach)
        {
            Vector3 d = p.position - transform.position;
            float gap = d.magnitude;   // en 3D: en otro piso no está «cerca» aunque esté encima
            if (gap > StartDistance) { Step(p.position); return; }
            transform.rotation = Quaternion.LookRotation(Flat(d));
            GreetedAtDistance = gap;
            Say(_subject, _withExam ? "Bienvenido al santuario. Soy quien te hace la iniciación: ve tú a cada sitio que te diga y espérame allí; yo te sigo."
                                    : "Ya tienes el examen hecho: vamos al tanque, tu primera virtualización.", 6f);
            _orch = _withExam ? BuildExam() : new Orchestra("iniciación", walkSpeed);
            if (!_withExam) { _rNew = _orch.Join("recién llegado", p); _rWorker = _orch.Join(Who, transform); }
            AddTank(_orch);
            Now = Phase.Escort;
            return;
        }
        _orch.Tick();
        if (_orch == null) return;   // (ResetState desde una señal)
        if (Now == Phase.Perform)
        {
            ArrivalExam.Beat b = CurrentBeat;
            // En 1.ª persona se ve el gesto: al empezar cada prueba, la mirada va a su cara (unos fotogramas); después los
            // ojos-cámara son del jugador, que mira donde quiera (PlayerController: ojos y cabeza).
            PlayerController pc = p.GetComponent<PlayerController>();
            EyesAtHead(pc, PatientDown);
            if (b != null && pc != null && Time.time < _aimUntil) { HeadAt(transform, out Vector3 face); pc.LookAtPoint(face - Vector3.up * 0.3f); }
        }
        if (!_orch.Done) return;
        Debug.Log($"[Iniciación] {Who}: la orquesta de «{_subject.name}» termina ({BeatsDone} pasos del examen).");
        _orch = null; _subject = null; _beat = -1; Now = Phase.None;
    }

    // ── 2. EL EXAMEN COMO ORQUESTA ────────────────────────────────────────────────────────────────────────────────
    Orchestra BuildExam()
    {
        var o = new Orchestra("examen de llegada", walkSpeed);
        Transform p = _subject.transform;
        Orchestra.Role nw = _rNew = o.Join("recién llegado", p), wk = _rWorker = o.Join(Who, transform);
        ArrivalExam.Beat prev = null;
        for (int i = 0; i < ArrivalExam.Beats.Length; i++)
        {
            int idx = i;
            ArrivalExam.Beat b = ArrivalExam.Beats[i];
            o.Movement(b.id);
            // EL RECIÉN LLEGADO: va solo a su sitio (si no sigue en el mismo), se coloca, avisa y hace su postura hasta «hecho».
            bool stay = prev != null && prev.place == b.place && prev.patient == b.patient;
            if (!stay)
            {
                if (prev != null && prev.patient != ArrivalExam.Spot.Stand) { string from = prev.place; o.Do(nw, () => Rise(from), "se levanta"); }
                o.GoTo(nw, () => PatientApproach(b, p), () => PlacePos(b, p), $"va solo a «{b.place}»");
                if (b.patient != ArrivalExam.Spot.Stand) o.Do(nw, () => Settle(b), b.patient == ArrivalExam.Spot.Sit ? "se sienta" : "se tumba");
            }
            o.Emit(nw, "en_su_sitio").WaitFor(nw, "empieza");
            o.Pose(nw, b.ptA, b.ptB, b.period, -1f, "hecho", label: $"postura del paso «{b.id}»");
            // EL TRABAJADOR: a su sitio, espera al recién llegado, hace el gesto (sus fotogramas) y da «hecho».
            o.GoTo(wk, () => WorkerSpot(b, p), () => p.position, $"va a su sitio en «{b.place}»");
            o.WaitFor(wk, "en_su_sitio");
            o.Do(wk, () => BeginBeat(idx), $"empieza «{b.title}»").Emit(wk, "empieza");
            o.Pose(wk, b.exA, b.exB, b.period, b.seconds, face: () => p.position, label: $"gesto «{b.id}» ({b.seconds:0} s)");
            o.Do(wk, () => EndBeat(idx), "termina el paso").Emit(wk, "hecho");
            prev = b;
        }
        // Al acabar el examen: de pie, la cámara como estaba.
        o.Movement("fin del examen");
        if (prev != null && prev.patient != ArrivalExam.Spot.Stand) { string from = prev.place; o.Do(nw, () => Rise(from), "se levanta"); }
        o.Do(wk, FinishExam, "termina el examen");
        return o;
    }

    // ── 3. AL TANQUE (último movimiento): los dos andan hasta él; delante, el trabajador lo mete dentro ─────────────
    void AddTank(Orchestra o)
    {
        Transform p = _subject.transform;
        VirtualizationMachine vm = Machine();
        o.Movement("al tanque");
        o.Do(_rWorker, () =>
        {
            Now = Phase.ToTank;
            Debug.Log($"[Iniciación] {Who} y «{p.name}» van al tanque: su primera virtualización (cada uno andando).");
        }, "al tanque");
        if (vm != null)
        {
            o.GoTo(_rNew, () => TankSpot(vm, false), () => vm.transform.position, "va solo al tanque");
            o.GoTo(_rWorker, () => TankSpot(vm, true), () => p.position, "va al tanque");
        }
        o.Emit(_rNew, "en_el_tanque").WaitFor(_rNew, "dentro");
        o.WaitFor(_rWorker, "en_el_tanque").Do(_rWorker, () => Handover(vm), "lo mete en el tanque").Emit(_rWorker, "dentro");
    }

    // Delante del tanque (por el lado que da al área); el trabajador, a un lado.
    static Vector3 TankSpot(VirtualizationMachine vm, bool worker)
    {
        Vector3 fr = ArrivalExam.Front(vm.transform), side = Vector3.Cross(Vector3.up, fr);
        return vm.transform.position + fr * (worker ? 3.0f : 2.4f) + side * (worker ? 1.2f : 0f);
    }

    void Handover(VirtualizationMachine vm)
    {
        Transform p = _subject.transform;
        _orch.Release(_rNew);
        _subject.initiated = true;
        if (vm == null || p.GetComponent<PlayerController>() == null || MobWorldLoader.Instance == null) return;   // (Instance lo crea si aún no existe)
        Say(o: null, $"Ponte la máscara y entra al agua: te espera Ambrosio, en el microcosmos.", 4f);
        LessonPrompt.Show($"{Who}: ponte la máscara y entra al agua: te espera Ambrosio, en el microcosmos.", null, 6f);
        Debug.Log($"[Iniciación] {Who} mete a «{p.name}» en el tanque y lo envía a «{FirstMicrocosmos}».");
        vm.EnterWithTank(p, Application.CanStreamedLevelBeLoaded(FirstMicrocosmos) ? FirstMicrocosmos : null);
        _sentAt = Time.time;
    }

    // ── Los sitios de cada paso ──
    // Donde se pone el recién llegado ANDANDO: delante del asiento de la silla, delante de la camilla o, de pie, delante del
    // lavabotas/lavamanos/mesa (el suelo lo pone PathMover: los pies en el suelo; antes se le colocaba a la altura del pivote
    // del mueble y en el lavabotas quedaban las piernas bajo el suelo).
    static Vector3 PatientApproach(ArrivalExam.Beat b, Transform p)
    {
        Transform place = Place(b.place);
        if (place == null) return p.position;
        Vector3 c = ArrivalExam.Center(place);
        if (b.patient == ArrivalExam.Spot.Sit) return c + Flat(place.forward).normalized * 0.65f;
        return c + ArrivalExam.Front(place) * (b.patient == ArrivalExam.Spot.Stand ? 0.7f : 0.75f);
    }

    static Vector3 PlacePos(ArrivalExam.Beat b, Transform p) { Transform place = Place(b.place); return place != null ? ArrivalExam.Center(place) : p.position + p.forward; }

    // El sitio del trabajador: de pie, a su lado (0,9 m); sentado o tumbado, delante y a un lado (no se pisan).
    static Vector3 WorkerSpot(ArrivalExam.Beat b, Transform p)
    {
        Transform place = Place(b.place);
        if (place == null) return p.position + p.forward * 1.0f;
        Vector3 fr = ArrivalExam.Front(place), side = Vector3.Cross(Vector3.up, fr);
        Vector3 c = ArrivalExam.Center(place);
        return b.patient == ArrivalExam.Spot.Stand ? c + fr * 0.7f + side * 0.9f : c + fr * 1.0f + side * 0.8f;
    }

    // Sentarse o tumbarse: del sitio de delante al mueble (menos de un metro), con su postura.
    void Settle(ArrivalExam.Beat b)
    {
        Transform p = _subject.transform, place = Place(b.place);
        if (place == null) return;
        Anima pa = _subject.GetComponentInParent<Anima>();
        ArrivalExam.PatientSpot(place, b.patient, out Vector3 at, out Quaternion rot, p);
        WorkdayDirector.Teleport(p, at);
        p.rotation = rot;
        if (pa != null) (b.patient == ArrivalExam.Spot.Sit ? DailyPoses.Sit() : DailyPoses.Lie()).ApplyFinal(pa);
        PatientDown = true;
        // Sentado o tumbado, el jugador lo vive en 1.ª persona (Berón 2026-10-26).
        if (!_camForced && p.GetComponent<PlayerController>() != null && CameraManager.Instance != null)
        {
            _camBefore = CameraManager.Instance.preferredMode; _camForced = true;
            CameraManager.Instance.SwitchTo(CameraMode.FirstPerson, forced: true);
        }
    }

    // Levantarse: de pie, con los pies en el suelo delante del mueble (no anda por encima del asiento).
    void Rise(string placeName)
    {
        if (!PatientDown) return;
        Transform p = _subject.transform, place = Place(placeName);
        Anima pa = _subject.GetComponentInParent<Anima>();
        if (pa != null) DailyPoses.Stand().ApplyFinal(pa);
        EyesAtHead(p.GetComponent<PlayerController>(), false);
        if (place != null)
        {
            bool chair = placeName == "Silla_examen";
            Vector3 front = ArrivalExam.Center(place) + (chair ? Flat(place.forward).normalized * 0.65f : ArrivalExam.Front(place) * 0.75f);
            if (UnityEngine.AI.NavMesh.SamplePosition(front, out UnityEngine.AI.NavMeshHit h, 1.5f, UnityEngine.AI.NavMesh.AllAreas)) front = h.position;
            WorkdayDirector.Teleport(p, PathMover.Of(p).OnGround(front));
        }
        PatientDown = false;
    }

    void BeginBeat(int i)
    {
        ArrivalExam.Beat b = ArrivalExam.Beats[i];
        Transform p = _subject.transform;
        _beat = i;
        ArrivalExam.Hold(gameObject, b.tool, BodyPart.HandRight, "Herramienta");
        ArrivalExam.Hold(p.gameObject, b.patientTool, b.patientBone, "Herramienta_paciente");
        Say(_subject, b.line, b.seconds);
        Debug.Log($"[Iniciación] {Who} · {b.title} ({b.seconds:0} s, en «{b.place}»; protege a: {b.protects}).");
        Now = Phase.Perform; _aimUntil = Time.time + 0.6f;
    }

    void EndBeat(int i)
    {
        ArrivalExam.Beat b = ArrivalExam.Beats[i];
        Anima me = GetComponent<Anima>();
        HealthRecord hr = _subject.GetComponent<HealthRecord>();
        if (b.id == "hisopo")   // la muestra del frotis: el ANÁLISIS CLÍNICO (el mismo hechizo que usan Linduarda y el analizador)
        {
            ITarget it = _subject.GetComponentInParent<ITarget>();
            if (me != null && it != null) this.GetOrAdd<ClinicalAnalysisSpell>().Cast(me, it);
            if (hr != null) hr.analyzed = true;
        }
        if (b.id == "vacuna" && hr != null) { hr.vaccinated = true; ITarget it = _subject.GetComponentInParent<ITarget>(); if (me != null && it != null) this.GetOrAdd<VaccinateSpell>().Cast(me, it); }
        ArrivalExam.Hold(gameObject, null, BodyPart.HandRight, "Herramienta");
        ArrivalExam.Hold(_subject.gameObject, null, b.patientBone, "Herramienta_paciente");
        Stand(me);
        BeatsDone++; _beat = i + 1; Now = Phase.Escort;
    }

    // Sentado o tumbado, la 1.ª persona va en la cabeza de verdad: el pivote de la cámara está a la altura de alguien de pie
    // y dejaba la vista medio metro por encima de la propia cabeza. De pie (o al acabar), vuelve a su sitio.
    void EyesAtHead(PlayerController pc, bool low)
    {
        if (pc == null || pc.cameraPivot == null) return;
        if (low && HeadAt(pc.transform, out Vector3 eyes))
        {
            if (!_pivotMoved) { _pivotHome = pc.cameraPivot.localPosition; _pivotMoved = true; }
            pc.cameraPivot.position = eyes + Vector3.up * 0.05f + pc.transform.forward * 0.15f;   // los ojos, delante del cubo
        }
        else if (_pivotMoved) { pc.cameraPivot.localPosition = _pivotHome; _pivotMoved = false; }
    }
    Vector3 _pivotHome; bool _pivotMoved;

    // Dónde está de verdad la cabeza de un cuerpo de cubos (el centro del cubo de la cabeza; el hueso es la articulación del
    // cuello). El origen de los cuerpos no está en los pies, así que «origen + 1,1 m» quedaba por encima de la cabeza.
    static bool HeadAt(Transform body, out Vector3 at)
    {
        Transform head = ArrivalExam.Bone(body, BodyPart.Head);
        Renderer r = head != null ? head.GetComponentInChildren<Renderer>() : null;
        at = r != null ? r.bounds.center : head != null ? head.position : body.position;
        return head != null;
    }

    void FinishExam()
    {
        Transform p = _subject.transform;
        EyesAtHead(p.GetComponent<PlayerController>(), false);
        Stand(GetComponent<Anima>());
        if (_camForced && CameraManager.Instance != null) { CameraManager.Instance.SwitchTo(_camBefore, forced: true); _camForced = false; }
        PlayerController ppc = p.GetComponent<PlayerController>(); if (ppc != null) ppc.LookAtPoint(p.position + p.forward * 10f + Vector3.up * 1.5f);   // la mirada, otra vez al frente
        Debug.Log($"[Iniciación] {Who} termina el examen de llegada de «{_subject.name}» ({BeatsDone} pasos): ahora, al tanque.");
    }

    static void Stand(Anima a) { if (a != null) DailyPoses.Stand().ApplyFinal(a); }

    void Say(SanctuaryOnboarding o, string text, float secs)
    {
        Debug.Log($"[Iniciación] {Who}: «{text}»");
        if (o != null && o.GetComponent<PlayerController>() != null) LessonPrompt.Show($"{Who}: {text}", null, secs + 2f);
    }

    // Andar hacia un sitio (PathMover: si está en otro piso o edificio, por la cadena de alfombras voladoras).
    void Step(Vector3 target) => PathMover.Of(this).StepToward(target, moveSpeed);

    static Vector3 Flat(Vector3 v) { v.y = 0f; return v.sqrMagnitude > 1e-4f ? v : Vector3.forward; }

    // ACOMPAÑAR al que aún no ha hecho su primera virtualización: lo sigue y se lo recuerda cada cierto tiempo.
    void Accompany(SanctuaryOnboarding o)
    {
        Vector3 to = o.transform.position - transform.position; to.y = 0f;
        if (to.magnitude > reach + 0.8f) Step(o.transform.position);
        if (Time.time - _sentAt < 120f || (MobWorldLoader.HasInstance && (MobWorldLoader.Instance.IsInMobWorld || MobWorldLoader.Instance.IsBusy))) return;   // ya va (o está) en el microcosmos
        if (Time.time < _nextReminder || o.GetComponent<PlayerController>() == null) return;
        _nextReminder = Time.time + 25f;
        VirtualizationMachine vm = Machine();
        string spell = vm != null ? $"virtualize({{destiny:\"{vm.areaName}\"}})" : "virtualize";
        LessonPrompt.Show($"{Who}: aún te falta tu primera virtualización. Ven al tanque, ponte la máscara y entra.", spell, 20f);
        SpellConsole.Propose(spell, $"{Who} te espera en el tanque", 25f);
        Debug.Log($"[Iniciación] {Who} recuerda al recién llegado que le falta su primera virtualización (lo acompaña por la enfermería).");
    }

    void OnDestroy() { _orch?.Stop(); if (Current == this) Current = null; if (Fallback == this) Fallback = null; }
}
