using UnityEngine;

/// <summary>
/// Hechizo CAMINAR — la **contraparte de Jalar aplicada a UNO MISMO** (docs/stats-as-truth.md §hechizos), sobre
/// el sistema unificado de `powerBonus` de `SpellBase`. Direcciones **ESDF** (s=izq, e=adelante, d=atrás, f=der):
///   • **Carga** (`chargeKey`/LeftShift): mantener `e`+LeftShift = **tomar postura de salida** (el personaje se
///     queda quieto acumulando); al **soltar**, arranca con la **velocidad inicial ∝ carga** (como el velocista).
///   • **Channeling** (`channelKey`/RightShift): mantener al correr = **subir a la punta** (hasta maxChanneling)
///     o **sostener** la velocidad que dejó la carga; sin sostener nada, la velocidad **decae** a la de caminar.
///   • **Forcejeo**: si algo te frena (no te desplazas), `ReportResult(false)` sube el bonus → empujas más fuerte.
/// La velocidad = `baseSpeed + powerBonus`, penalizada por el **peso** (`bodyMass`), y **cuesta ATP** ∝ velocidad×masa.
/// </summary>
public class WalkSpell : SpellBase
{
    [Header("Caminar (ESDF; carga=postura de salida, channel=punta sostenida)")]
    [Tooltip("Velocidad base al mover el propio peso sin bonos (m/s, antes de dividir por masa).")]
    [Min(0f)] public float baseSpeed = 3f;
    [Tooltip("ATP/s por unidad de (velocidad×masa): más rápido o más pesado → más cansancio.")]
    [Min(0f)] public float energyPerEffort = 0.2f;
    [Tooltip("Desplazamiento (m/frame) por debajo del cual se considera 'bloqueado' y sube el forcejeo.")]
    [Min(0f)] public float blockedEpsilon = 0.001f;

    [Header("Direcciones ESDF")]
    public KeyCode leftKey = KeyCode.S, forwardKey = KeyCode.E, backKey = KeyCode.D, rightKey = KeyCode.F;

    Anima   _self;
    Vector3 _lastPos;

    void Awake()
    {
        _self = GetComponent<Anima>();
        if (chargeAnimator == null) chargeAnimator = GetComponent<Animator>();   // postura de salida / arranque (nombra los estados en el Inspector)
    }

    void Update()
    {
        float dt = Time.deltaTime;
        TickPowerBonus(_self, dt);   // LeftShift=carga (postura), RightShift=channel (punta), + decaimiento

        Vector3 dir = Vector3.zero;
        if (Input.GetKey(forwardKey)) dir.z += 1f;
        if (Input.GetKey(backKey))    dir.z -= 1f;
        if (Input.GetKey(rightKey))   dir.x += 1f;
        if (Input.GetKey(leftKey))    dir.x -= 1f;
        bool moving = dir.sqrMagnitude >= 0.01f;

        // Cargando la velocidad inicial (LeftShift): el personaje se queda QUIETO tomando postura de salida.
        if (IsCharging) { _lastPos = transform.position; return; }

        if (!moving) { _lastPos = transform.position; return; }
        dir.Normalize();

        // ¿bloqueado? sin desplazarte = fallo → sube el forcejeo (dentro del powerBonus).
        float moved = (transform.position - _lastPos).magnitude;
        ReportResult(_self, moved >= blockedEpsilon);
        _lastPos = transform.position;

        float mass  = _self != null ? _self.BodyMass : 1f;
        float speed = (baseSpeed + PowerBonus) / Mathf.Max(0.1f, mass);   // carga+channel ayudan; el peso frena

        // Cansancio ∝ esfuerzo (velocidad×masa). Sin energía, no avanzas.
        if (_self != null && energyPerEffort > 0f)
        {
            CharacterLevel cl = _self.GetComponent<CharacterLevel>();
            if (cl != null && !cl.SpendEnergy(energyPerEffort * speed * mass * dt)) return;
        }

        transform.position += dir * speed * dt;
    }

    // El objetivo es siempre uno mismo (no usa la API dirigida).
    public override bool CanCast(Anima caster, ITarget target) => true;
    public override void Cast(Anima caster, ITarget target) { }
}
