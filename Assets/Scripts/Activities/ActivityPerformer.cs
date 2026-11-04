using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HACER LA ACTIVIDAD, NO SOLO LLEGAR (docs/actividades-y-uniformes.md; Berón 2026-11-04: «parece que solo van al piso que les toca y
/// no hacen nada, deberían realizar las actividades … también aplican a los insectos-humanos … deben tener sus orquestas/animaciones
/// para completar sus tareas/jornada/ocio/recuperación»). Una ACTUACIÓN (<see cref="Performance"/>) es una partitura de PASOS
/// (<see cref="Step"/>) que se repite mientras dura la misión o la actividad: cada paso tiene su postura base (de pie, sentado,
/// meditando), su GESTO de brazos A ↔ B (<see cref="ActivityKit.Arms"/>), lo que lleva en cada mano (<see cref="ActivityKit.Prop"/>),
/// cuánto dura, su EFECTO visible (vapor, chispas, polvo, gotas…, <see cref="ActivityKit.Burst"/>) y, si toca, algo que hace en el
/// mundo al empezar (<see cref="Step.act"/>).
///
/// Quien lo conduce (la jornada, el horario libre, la jornada del microcosmos) llama a <see cref="Keep"/> CADA FOTOGRAMA mientras el
/// personaje está en su sitio; si deja de llamarlo (se va a comer, al baño, cambia de misión, anda hacia otro sitio), a los
/// <see cref="KeepAlive"/> s el que actúa se para solo: suelta los utensilios y se pone de pie. Así ninguna conducta tiene que acordarse
/// de pararlo y nunca queda nadie con una escoba en la mano camino del comedor. Sin esperas sin tope: todo va por tiempo.
/// Para un cuerpo de cubos humano mueve los huesos (con el candado de postura); para un insecto-humano, sus brazos delanteros
/// (<see cref="CubeInsect.Push"/> / <see cref="CubeInsect.Martial"/>).
/// </summary>
[DisallowMultipleComponent]
public class ActivityPerformer : MonoBehaviour
{
    public enum Base { Stand, Sit, Meditate }

    /// <summary>Un paso de la partitura.</summary>
    public sealed class Step
    {
        public string label;
        public ActivityKit.Arms arms;
        public ActivityKit.Prop right, left;
        public float seconds = 8f;
        public ActivityKit.Fx fx;
        public float fxEvery = 1.6f;
        public bool fxAtFace;            // el efecto sale del sitio de trabajo (el fogón, la cama) y no de la mano
        public Base pose = Base.Stand;
        public System.Action<GameObject> act;   // al empezar el paso (un instante)
        public InsectArm insect = InsectArm.Push;
    }

    /// <summary>Cómo mueve los brazos un insecto-humano en ese paso.</summary>
    public enum InsectArm { Push, Strike, Fine, Raise, Still }

    /// <summary>Una actuación: su nombre y sus pasos (se repiten en bucle).</summary>
    public sealed class Performance
    {
        public string id, title;
        public Step[] steps;
        /// <summary>true = genérica (por la clase de función, sin pasos propios): la lista de pendientes del doc.</summary>
        public bool generic;
        public Performance(string id, string title, params Step[] steps) { this.id = id; this.title = title; this.steps = steps; }
        public override string ToString() => $"{title} ({steps.Length} pasos{(generic ? ", genérica" : "")})";
    }

    public const float KeepAlive = 0.6f;

    /// <summary>Los que actúan ahora (pruebas, registro).</summary>
    public static readonly List<ActivityPerformer> Active = new List<ActivityPerformer>();
    /// <summary>Pasos empezados desde el arranque (pruebas).</summary>
    public static int StepsStarted { get; private set; }

    public Performance performance { get; private set; }
    public int stepIndex { get; private set; }
    public int stepsDone { get; private set; }
    public int cycles { get; private set; }
    public int effects { get; private set; }
    public float startedAt { get; private set; } = -1f;
    /// <summary>Lo que hace ahora («Remover la olla»).</summary>
    public string Doing => performance == null ? null : performance.steps.Length == 0 ? performance.title : performance.steps[stepIndex].label;
    public Step Current => performance == null || performance.steps.Length == 0 ? null : performance.steps[stepIndex];

    float _lastKeep, _stepStart, _nextFx, _nextBase;
    Vector3 _face; bool _hasFace;
    Anima _a; CubeInsect _ci;
    AsanaPoseSpell _stand, _sit, _meditate;

    // ── API ─────────────────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>Que <paramref name="g"/> siga haciendo <paramref name="p"/> (llamar cada fotograma mientras está en su sitio), mirando a
    /// <paramref name="face"/>. Si ya hacía otra, cambia a esta.</summary>
    public static ActivityPerformer Keep(GameObject g, Performance p, Vector3? face = null)
    {
        if (g == null || p == null) return null;
        ActivityPerformer ap = g.GetOrAdd<ActivityPerformer>();
        if (!ap.enabled) ap.enabled = true;
        if (ap.performance != p) ap.Begin(p);
        ap._lastKeep = Time.time;
        if (face.HasValue) { ap._face = face.Value; ap._hasFace = true; }
        return ap;
    }

    /// <summary>¿Está <paramref name="g"/> haciendo una actuación ahora?</summary>
    public static bool Performing(GameObject g) => g != null && g.TryGetComponent(out ActivityPerformer ap) && ap.performance != null;

    /// <summary>La actuación de <paramref name="g"/> (null si ninguna).</summary>
    public static ActivityPerformer Of(GameObject g) => g != null && g.TryGetComponent(out ActivityPerformer ap) && ap.performance != null ? ap : null;

    /// <summary>Para ya (suelta los utensilios y se pone de pie).</summary>
    public static void Stop(GameObject g) { if (g != null && g.TryGetComponent(out ActivityPerformer ap)) ap.End(); }

    // ── La partitura ────────────────────────────────────────────────────────────────────────────────────────────

    void Begin(Performance p)
    {
        if (performance != null) End(quiet: true);
        performance = p; stepIndex = 0; stepsDone = 0; cycles = 0; effects = 0; startedAt = Time.time;
        _a = GetComponent<Anima>(); _ci = GetComponent<CubeInsect>();
        if (!Active.Contains(this)) Active.Add(this);
        Debug.Log($"[Actividad] «{name}» empieza a hacer «{p.title}» ({p.steps.Length} pasos{(p.generic ? ", genérica" : "")}).");
        StartStep();
    }

    void StartStep()
    {
        _stepStart = Time.time; _nextFx = Time.time + 0.6f; _nextBase = 0f;
        Step s = Current; if (s == null) return;
        StepsStarted++;
        ActivityKit.Hold(gameObject, true, Era(s.right));
        ActivityKit.Hold(gameObject, false, Era(s.left));
        try { s.act?.Invoke(gameObject); }
        catch (System.Exception e) { Debug.LogWarning($"[Actividad] «{name}» · «{s.label}»: el efecto del paso falló ({e.Message})."); }
    }

    // El insecto-humano de una era antigua no saca una tableta: lo que había en su época.
    ActivityKit.Prop Era(ActivityKit.Prop p)
    {
        if (_ci == null) return p;
        int y = Eras.Now;
        if (y < 1900)
            switch (p)
            {
                case ActivityKit.Prop.Tablet: case ActivityKit.Prop.Clipboard: return ActivityKit.Prop.Book;
                case ActivityKit.Prop.Flashlight: return ActivityKit.Prop.Bottle;   // (un candil)
                case ActivityKit.Prop.Spray: return ActivityKit.Prop.Cloth;
                case ActivityKit.Prop.Thermometer: case ActivityKit.Prop.Syringe when y < 1850: return ActivityKit.Prop.Bandage;
                case ActivityKit.Prop.Pipette: return ActivityKit.Prop.Flask;
                case ActivityKit.Prop.Mop: return ActivityKit.Prop.Broom;
            }
        return p;
    }

    void End(bool quiet = false)
    {
        if (performance == null) return;
        if (!quiet) Debug.Log($"[Actividad] «{name}» deja «{performance.title}» tras {Time.time - startedAt:0} s ({stepsDone} pasos, {effects} efectos).");
        performance = null;
        Active.Remove(this);
        ActivityKit.ClearProps(gameObject);
        if (_a != null && !_a.asleep && !_a.death && GetComponent<CreatureRig>() != null) DailyPoses.Stand().ApplyFinal(_a);
    }

    void OnDisable() => End(quiet: true);
    void OnDestroy() => Active.Remove(this);

    void LateUpdate()
    {
        if (performance == null) return;
        if (Time.time - _lastKeep > KeepAlive) { End(); return; }
        if (_a != null && (_a.asleep || _a.death)) { End(); return; }
        Step s = Current; if (s == null) return;
        float t = Time.time - _stepStart;
        if (t >= s.seconds)
        {
            stepsDone++;
            stepIndex = (stepIndex + 1) % performance.steps.Length;
            if (stepIndex == 0) cycles++;
            StartStep();
            s = Current; t = 0f;
        }
        // Mirar a su sitio de trabajo.
        if (_hasFace)
        {
            Vector3 d = _face - transform.position; d.y = 0f;
            if (d.sqrMagnitude > 0.04f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(d), Time.deltaTime * 4f);
        }
        // El cuerpo: la postura base (cada 0,25 s: el candado dura 0,3) y el gesto encima, cada fotograma.
        if (_ci != null) Insect(s, t);
        else if (_a != null && GetComponent<CreatureRig>() != null)
        {
            if (Time.time >= _nextBase) { _nextBase = Time.time + 0.25f; BaseOf(s.pose).ApplyFinal(_a); }
            ActivityKit.Apply(gameObject, s.arms, t);
        }
        // El efecto.
        if (s.fx != ActivityKit.Fx.None && Time.time >= _nextFx)
        {
            _nextFx = Time.time + Mathf.Max(0.3f, s.fxEvery);
            Vector3 at = s.fxAtFace && _hasFace ? Vector3.Lerp(transform.position, _face, 0.6f) + Vector3.up * 0.3f * ActivityKit.Scale(gameObject) : ActivityKit.Tip(gameObject, true);
            ActivityKit.Burst(at, s.fx, ActivityKit.Scale(gameObject));
            effects++;
        }
    }

    AsanaPoseSpell BaseOf(Base b)
    {
        switch (b)
        {
            case Base.Sit: return _sit ??= DailyPoses.Sit();
            case Base.Meditate: return _meditate ??= DailyPoses.Meditate();
            default: return _stand ??= DailyPoses.Stand();
        }
    }

    void Insect(Step s, float t)
    {
        float period = s.arms != null ? s.arms.period : 1.4f;
        float k = ActivityKit.Phase(period, t);
        switch (s.insect)
        {
            case InsectArm.Strike: _ci.Martial(0.1f, k, 0.3f + 0.6f * k, 0f, 0.3f); break;
            case InsectArm.Fine: _ci.Martial(0.35f, 0.3f + 0.25f * k, 0.5f, 0f, 0.6f); break;
            case InsectArm.Raise: _ci.Martial(0.1f, 0.6f + 0.4f * k, 0.9f, 0f, 0.3f); break;
            case InsectArm.Still: _ci.Martial(0f, 0f, 0.45f, 0f, 0.1f); break;
            default: _ci.Push(k); break;
        }
    }
}
