using UnityEngine;

/// <summary>
/// Hechizo JALAR — Kushal atrae a una hormiga vieja hacia si mismo inyectando un
/// <see cref="MovementImpulse"/> sobre el <see cref="ImpulseController"/> del objetivo.
///
/// FORCEJEO (tug-of-war):
/// - Al iniciar la jalada, el power empieza en <see cref="SpellBase.force"/>.
/// - Cada segundo sube a razon de <see cref="rampRate"/> hasta <see cref="maxPullPower"/>.
/// - La hormiga resiste con su HomeImpulse + ThreatScanner. Si el jalon supera
///   los impulsos contrarios, la hormiga se mueve hacia Kushal.
/// - Si <see cref="CharacterLevel.currentEnergy"/> del caster baja, el hechizo se rompe.
/// - El objetivo tambien gasta energia (resistir cuesta ATP).
///
/// Targeting: busca el <see cref="ImpulseController"/> mas cercano en rango.
/// Funciona con SimpleAnima (no requiere ITarget).
///
/// Extiende <see cref="SpellBase"/>:
///   range      = radio de alcance
///   force      = magnitud inicial del impulso (startPower)
///   energyCost = coste por tick del caster (CharacterLevel.currentEnergy)
/// </summary>
public class PullSpell : SpellBase
{
    [Header("Forcejeo")]
    [Tooltip("Ritmo al que el power del jalon sube por segundo mientras se mantiene.")]
    [Min(0f)] public float rampRate = 0.6f;

    [Tooltip("Power maximo del jalon (limite del forcejeo).")]
    [Min(0.1f)] public float maxPullPower = 8f;

    [Tooltip("Coste de energia por segundo para el OBJETIVO (resistir cuesta ATP).")]
    [Min(0f)] public float targetEnergyCostPerSecond = 1f;

    [Tooltip("Tecla para mantener el jalon activo.")]
    public KeyCode pullKey = KeyCode.F;

    // -- Estado ---------------------------------------------------------------

    float             _currentPower;
    bool              _pulling;
    ImpulseController _targetCtrl;
    CharacterLevel    _targetLevel;
    Anima             _self;

    const string PULL_TAG = "pull_spell";

    // -- Ciclo ----------------------------------------------------------------

    void Awake() => _self = GetComponent<Anima>();

    void Update()
    {
        if (Input.GetKeyDown(pullKey))
        {
            // Buscar ImpulseController mas cercano en rango (funciona con SimpleAnima).
            _targetCtrl  = FindClosestPullable();
            _targetLevel = _targetCtrl != null ? _targetCtrl.GetComponent<CharacterLevel>() : null;
            if (_targetCtrl != null && HasEnergy(_self))
            {
                _pulling      = true;
                _currentPower = force > 0f ? force : 1f;
            }
        }

        if (Input.GetKey(pullKey) && _pulling)
        {
            if (_targetCtrl == null) { StopPull(); return; }

            // Verificar rango.
            float dist = Vector3.Distance(transform.position, _targetCtrl.transform.position);
            if (range > 0f && dist > range) { StopPull(); return; }

            // Verificar energia del caster.
            if (!PayEnergy(_self)) { StopPull(); return; }

            // Rampar el power del jalon.
            _currentPower = Mathf.Min(_currentPower + rampRate * Time.deltaTime, maxPullPower);

            // Inyectar impulso de jalon en el objetivo.
            Vector3 dir = transform.position - _targetCtrl.transform.position;
            _targetCtrl.RemoveByTag(PULL_TAG);
            _targetCtrl.AddImpulse(new MovementImpulse(PULL_TAG, dir, _currentPower, 0f));

            // Coste de resistencia para el objetivo.
            if (targetEnergyCostPerSecond > 0f && _targetLevel != null)
            {
                float cost = targetEnergyCostPerSecond * Time.deltaTime;
                if (!_targetLevel.SpendEnergy(cost))
                    _targetLevel.currentEnergy = 0f;
            }
        }

        if (!Input.GetKey(pullKey) && _pulling)
            StopPull();
    }

    void StopPull()
    {
        _pulling      = false;
        _currentPower = 0f;
        _targetCtrl?.RemoveByTag(PULL_TAG);
        _targetCtrl  = null;
        _targetLevel = null;
    }

    // -- SpellBase ------------------------------------------------------------

    /// <summary>Usado cuando hay ITarget disponible (contexto Animal completo).</summary>
    public override bool CanCast(Anima caster, ITarget target)
    {
        if (target == null || target.Dead || target.Consumed) return false;
        if (!InRange(caster, target)) return false;
        if (!HasEnergy(caster)) return false;
        return target.transform.GetComponent<ImpulseController>() != null;
    }

    public override void Cast(Anima caster, ITarget target)
    {
        // Logica manejada en Update (mantener tecla).
    }

    // -- Helpers --------------------------------------------------------------

    /// <summary>Busca el ImpulseController mas cercano en rango.</summary>
    ImpulseController FindClosestPullable()
    {
        float best = float.MaxValue;
        ImpulseController found = null;
        float checkRadius = range > 0f ? range : 5f;
        var cols = UnityEngine.Physics.OverlapSphere(transform.position, checkRadius);
        foreach (var col in cols)
        {
            if (col.gameObject == this.gameObject) continue;
            var ctrl = col.GetComponent<ImpulseController>();
            if (ctrl == null) continue;
            float d = Vector3.Distance(transform.position, col.transform.position);
            if (d < best) { best = d; found = ctrl; }
        }
        return found;
    }

    void OnDrawGizmosSelected()
    {
        if (!_pulling || _targetCtrl == null) return;
        UnityEngine.Gizmos.color = new UnityEngine.Color(0.2f, 0.6f, 1f, 0.7f);
        UnityEngine.Gizmos.DrawLine(transform.position, _targetCtrl.transform.position);
        UnityEngine.Gizmos.color = new UnityEngine.Color(0.2f, 0.6f, 1f, 0.25f);
        UnityEngine.Gizmos.DrawWireSphere(transform.position, range > 0f ? range : 5f);
    }
}
