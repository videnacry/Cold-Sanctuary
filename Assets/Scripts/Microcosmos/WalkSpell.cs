using UnityEngine;

/// <summary>
/// Hechizo CAMINAR — la **contraparte de Jalar aplicada a UNO MISMO** (docs/stats-as-truth.md §hechizos), ahora
/// sobre el sistema unificado de bonos de `SpellBase`:
///   • **Forcejeo** (físico): si algo te frena (no te desplazas), `ReportResult(false)` sube el bonus → empujas
///     más fuerte contra el obstáculo/cuesta. Persiste.
///   • **Channeling** (mental): mantener `channelKey` (Shift) al andar = **esprintar** (bonus mental → más
///     velocidad); decae al soltar.
/// La velocidad = `baseSpeed + bonos`, penalizada por el **peso** (`bodyMass`), y **cuesta ATP** ∝ velocidad×masa
/// (un cuerpo pesado o forzado se cansa). Dirección por ejes (WASD). Movimiento por Transform (demo).
/// </summary>
public class WalkSpell : SpellBase
{
    [Header("Caminar (fuerza adaptativa sobre el propio peso)")]
    [Tooltip("Velocidad base al mover el propio peso sin resistencia ni bonos (m/s, antes de dividir por masa).")]
    [Min(0f)] public float baseSpeed = 3f;
    [Tooltip("ATP/s por unidad de (velocidad×masa): más rápido o más pesado → más cansancio.")]
    [Min(0f)] public float energyPerEffort = 0.2f;
    [Tooltip("Desplazamiento (m/frame) por debajo del cual se considera 'bloqueado' y sube el forcejeo.")]
    [Min(0f)] public float blockedEpsilon = 0.001f;

    Anima   _self;
    Vector3 _lastPos;

    void Awake()
    {
        _self = GetComponent<Anima>();
        castMode = CastMode.Channel;
    }

    void Update()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        float dt = Time.deltaTime;
        bool moving = input.sqrMagnitude >= 0.01f;

        bool channeling = moving && Input.GetKey(channelKey);   // Shift al andar = esprintar (channeling mental)
        TickChanneling(channeling, dt);

        if (!moving) { _lastPos = transform.position; return; }
        input.Normalize();

        // ¿bloqueado el frame anterior? sin desplazarte = fallo → sube el forcejeo (físico).
        float moved = (transform.position - _lastPos).magnitude;
        ReportResult(moved >= blockedEpsilon);
        _lastPos = transform.position;

        float mass  = _self != null ? _self.BodyMass : 1f;
        float speed = (baseSpeed + BonusPower(_self)) / Mathf.Max(0.1f, mass);   // bonos ayudan; el peso frena

        // Cansancio ∝ esfuerzo (velocidad×masa). Sin energía, no avanzas.
        if (_self != null && energyPerEffort > 0f)
        {
            CharacterLevel cl = _self.GetComponent<CharacterLevel>();
            if (cl != null && !cl.SpendEnergy(energyPerEffort * speed * mass * dt)) return;
        }

        transform.position += input * speed * dt;
    }

    // El objetivo es siempre uno mismo (no usa la API dirigida).
    public override bool CanCast(Anima caster, ITarget target) => true;
    public override void Cast(Anima caster, ITarget target) { }
}
