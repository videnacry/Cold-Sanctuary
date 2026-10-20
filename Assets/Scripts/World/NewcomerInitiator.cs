using UnityEngine;

/// <summary>
/// INICIADOR DE RECIÉN LLEGADOS (docs/jornada-y-rotacion.md). La iniciación es un hechizo de COOPERACIÓN («iniciar») que
/// lanza quien tenga HOY, A ESTA HORA, la misión «Iniciación de recién llegados y turistas» de la Enfermería: no siempre
/// Linduarda ni la Magnate, sino quien le haya tocado en la rotación (o un clon de la Magnate si faltan manos). Fuera de
/// la jornada (y en las pruebas) lo hace Linduarda.
/// Cuando el recién llegado ya tiene análisis y vacuna, el iniciador lo guía («sígueme») hasta la máquina de
/// virtualización y lo envía a su primera virtualización; mientras no la haga, lo acompaña y se lo recuerda.
/// </summary>
public class NewcomerInitiator : MonoBehaviour
{
    public float reach = 2.5f, moveSpeed = 4f;

    /// <summary>Quien tiene la misión de iniciación ahora (lo fija la jornada) y quien la cubre si no hay nadie.</summary>
    public static NewcomerInitiator Current, Fallback;
    public static NewcomerInitiator Active => Current != null ? Current : Fallback;
    public bool IsActive => Active == this;

    /// <summary>¿Está ocupado con un recién llegado? (la jornada no lo mueve entonces).</summary>
    public bool Busy => IsActive && (_leading != null || Waiting() != null);

    VirtualizationMachine _machine;
    SanctuaryOnboarding _leading;
    float _nextReminder;

    public string Who
    {
        get { Mind m = GetComponent<Mind>(); return m != null && !string.IsNullOrEmpty(m.identity) ? m.identity : name; }
    }

    VirtualizationMachine Machine()
    {
        if (_machine != null) return _machine;
        foreach (VirtualizationMachine vm in SceneCache.All<VirtualizationMachine>(2f))
            if (vm != null && vm.areaName == "Enfermería") return _machine = vm;
        return _machine = FindAnyObjectByType<VirtualizationMachine>();
    }

    // El recién llegado que espera iniciación: sano (análisis y vacuna) y sin iniciar, o iniciado sin su primera virtualización.
    static SanctuaryOnboarding Waiting()
    {
        foreach (SanctuaryOnboarding o in SceneCache.All<SanctuaryOnboarding>(0.5f))
        {
            if (o == null || o.apt) continue;
            HealthRecord hr = o.GetComponent<HealthRecord>();
            if (hr != null && hr.NeedsCare) continue;   // primero la doctora (análisis y vacuna)
            return o;
        }
        return null;
    }

    void Update()
    {
        if (!IsActive) return;
        if (_leading != null) { Lead(); return; }
        SanctuaryOnboarding o = Waiting();
        if (o == null) return;
        if (!o.initiated) { Begin(o); return; }
        Accompany(o);
    }

    // Lanza «iniciar» (cooperación): le pide que lo siga a la máquina.
    void Begin(SanctuaryOnboarding o)
    {
        _leading = o;
        Anima me = GetComponent<Anima>();
        ITarget it = o.GetComponentInParent<ITarget>();
        if (me != null && it != null) this.GetOrAdd<FollowSpell>().Cast(me, it);   // «sígueme»
        Debug.Log($"[Iniciación] {Who} lanza «iniciar» (hechizo de cooperación) sobre «{o.name}»: lo lleva a su primera virtualización.");
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
        else if (vm.CanInteract) vm.Interact();
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
