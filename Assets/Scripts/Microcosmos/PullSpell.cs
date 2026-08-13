using UnityEngine;

/// <summary>
/// Hechizo JALAR — Kushal atrae a otro ser hacia sí inyectando un <see cref="MovementImpulse"/> en el
/// <see cref="ImpulseController"/> del objetivo. **Fusión** de las dos versiones previas (docs/stats-as-truth.md
/// §hechizos): arquitectura EMERGENTE por impulsos (mantener tecla = forcejeo) + FÍSICA por stats.
///
/// FORCEJEO ADAPTATIVO:
/// - Al empezar, el power = **mínimo para mover su peso** (`max(force, masa del objetivo)`).
/// - Cada tick se mide el **progreso HACIA el caster** (cuánto se acercó). Si NO progresa (el objetivo resiste
///   con sus propios impulsos Home/Threat), el power **sube** `rampRate`/s hasta el **techo = `force + Strength`
///   del lanzador** (acotado por `maxPullPower`). Si progresa, se mantiene.
/// - **Ambos gastan ATP**: el lanzador ∝ al power que emplea (más fuerza → más energía); el objetivo por resistir.
///   Gana quien tenga más fuerza **o** más aguante: si el objetivo se queda sin ATP, cede.
/// - `CastMode.Channel` (tecla `castKey`, por defecto F). También `Cast(caster, ITarget)` para la IA/targeting.
/// Funciona con `SimpleAnima` (targeting por `ImpulseController`, no requiere `ITarget`).
/// </summary>
public class PullSpell : SpellBase
{
    [Header("Forcejeo (adaptativo)")]
    [Tooltip("Cuánto sube el power por segundo cuando el objetivo NO progresa hacia el caster.")]
    [Min(0f)] public float rampRate = 0.8f;
    [Tooltip("Techo ABSOLUTO del power (además del límite por fuerza del lanzador).")]
    [Min(0.1f)] public float maxPullPower = 8f;
    [Tooltip("ATP/s que gasta el OBJETIVO por resistir el jalón.")]
    [Min(0f)] public float targetEnergyCostPerSecond = 1f;
    [Tooltip("ATP/s del LANZADOR por unidad de power empleado (más fuerza → más energía).")]
    [Min(0f)] public float casterEnergyPerPower = 0.4f;
    [Tooltip("Progreso hacia el caster (m/tick) por debajo del cual se considera 'atascado' y sube la fuerza.")]
    [Min(0f)] public float progressEpsilon = 0.01f;

    float             _power;
    bool              _pulling;
    ImpulseController _targetCtrl;
    CharacterLevel    _targetLevel;
    Anima             _self;
    float             _lastDist;
    float             _castTimer;   // para el jalón programático (IA): dura `duration`.
    bool              _keyDriven;

    const string PULL_TAG = "pull_spell";

    void Awake()
    {
        _self = GetComponent<Anima>();
        castMode = CastMode.Channel;
        if (castKey == KeyCode.None) castKey = KeyCode.F;
    }

    void Update()
    {
        PollInput();                       // ruta por tecla: OnChannelStart/End
        if (_pulling) PullTick(Time.deltaTime);
    }

    // ── Channel (tecla mantenida) ────────────────────────────────────────────
    protected override void OnChannelStart() { _keyDriven = true; BeginPull(FindClosestPullable()); }
    protected override void OnChannelEnd()   { if (_keyDriven) EndPull(); }

    // ── El forcejeo en sí (lo corre Update mientras _pulling) ─────────────────
    void PullTick(float dt)
    {
        if (_targetCtrl == null) { EndPull(); return; }
        float dist = Vector3.Distance(transform.position, _targetCtrl.transform.position);
        if (range > 0f && dist > range) { EndPull(); return; }

        // Programático (IA): expira por `duration`.
        if (!_keyDriven && duration > 0f) { _castTimer -= dt; if (_castTimer <= 0f) { EndPull(); return; } }

        // El lanzador paga ATP ∝ al power; sin energía, se rompe el jalón.
        if (_self != null && casterEnergyPerPower > 0f)
        {
            CharacterLevel cl = _self.GetComponent<CharacterLevel>();
            if (cl != null && !cl.SpendEnergy(casterEnergyPerPower * _power * dt)) { EndPull(); return; }
        }

        // Progreso HACIA el caster este tick → decide si forcejear más fuerte.
        float progress = _lastDist - dist;
        _lastDist = dist;
        float cap = Mathf.Min(maxPullPower, force + (_self != null ? _self.Strength : 0f));
        if (progress < progressEpsilon) _power = Mathf.Min(cap, _power + rampRate * dt);   // atascado → más fuerza

        // Inyectar el impulso de jalón (dirección: del objetivo hacia el caster).
        Vector3 dir = transform.position - _targetCtrl.transform.position;
        _targetCtrl.RemoveByTag(PULL_TAG);
        _targetCtrl.AddImpulse(new MovementImpulse(PULL_TAG, dir, _power, 0f));

        // El objetivo gasta ATP por resistir; si se queda sin energía, ya no puede oponerse.
        if (_targetLevel != null && targetEnergyCostPerSecond > 0f)
            if (!_targetLevel.SpendEnergy(targetEnergyCostPerSecond * dt)) _targetLevel.currentEnergy = 0f;
    }

    void BeginPull(ImpulseController ctrl)
    {
        _targetCtrl = ctrl;
        if (_targetCtrl == null) { _pulling = false; return; }
        _targetLevel = _targetCtrl.GetComponent<CharacterLevel>();
        Anima ta = _targetCtrl.GetComponent<Anima>();
        float targetMass = ta != null ? ta.BodyMass : 1f;
        _power = Mathf.Max(force, targetMass);                 // mínimo para mover su peso
        _lastDist = Vector3.Distance(transform.position, _targetCtrl.transform.position);
        _pulling = true;
    }

    void EndPull()
    {
        _pulling = false;
        _keyDriven = false;
        _power = 0f;
        _targetCtrl?.RemoveByTag(PULL_TAG);
        _targetCtrl = null;
        _targetLevel = null;
    }

    // ── API para IA/targeting (Cast dirigido, dura `duration`) ────────────────
    public override bool CanCast(Anima caster, ITarget target)
    {
        if (target == null || target.Dead || target.Consumed) return false;
        if (!InRange(caster, target)) return false;
        if (!HasEnergy(caster)) return false;
        return target.transform.GetComponent<ImpulseController>() != null;
    }

    public override void Cast(Anima caster, ITarget target)
    {
        if (target == null) return;
        ImpulseController ctrl = target.transform.GetComponent<ImpulseController>();
        if (ctrl == null) return;
        _self = caster;
        _keyDriven = false;
        _castTimer = duration > 0f ? duration : 0.4f;
        BeginPull(ctrl);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    ImpulseController FindClosestPullable()
    {
        float best = float.MaxValue;
        ImpulseController found = null;
        float checkRadius = range > 0f ? range : 5f;
        Collider[] cols = Physics.OverlapSphere(transform.position, checkRadius);
        foreach (Collider col in cols)
        {
            if (col.gameObject == gameObject) continue;
            ImpulseController ctrl = col.GetComponent<ImpulseController>();
            if (ctrl == null) continue;
            float d = Vector3.Distance(transform.position, col.transform.position);
            if (d < best) { best = d; found = ctrl; }
        }
        return found;
    }

    void OnDrawGizmosSelected()
    {
        if (!_pulling || _targetCtrl == null) return;
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.7f);
        Gizmos.DrawLine(transform.position, _targetCtrl.transform.position);
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, range > 0f ? range : 5f);
    }
}
