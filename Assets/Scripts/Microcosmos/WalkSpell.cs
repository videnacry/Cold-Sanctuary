using UnityEngine;

/// <summary>
/// Hechizo CAMINAR — la **contraparte de Jalar aplicada a UNO MISMO** (docs/stats-as-truth.md §hechizos). El
/// mismo bucle de fuerza adaptativa, pero el objetivo es el propio cuerpo: gastas el **mínimo de fuerza para
/// mover tu propio peso** (`bodyMass`) y, si algo te frena (obstáculo/cuesta/barro → no te desplazas), **subes
/// paulatinamente** la fuerza hasta tu techo (`force + Strength`), gastando **más ATP** cuanto más fuerza usas.
/// Consecuencia (coherente con el metabolismo): un cuerpo pesado o de poca fuerza **se cansa al andar**.
///
/// Dirección por ejes de input (WASD). Movimiento por Transform (demo); si el ser tiene `ImpulseController`,
/// convendría inyectar un impulso de "walk" para participar del sistema emergente (pendiente). `CastMode.Channel`.
/// </summary>
public class WalkSpell : SpellBase
{
    [Header("Caminar (fuerza adaptativa sobre el propio peso)")]
    [Tooltip("Velocidad base al mover el propio peso sin resistencia (m/s a power=masa).")]
    [Min(0f)] public float baseSpeed = 2f;
    [Tooltip("Cuánto sube la fuerza por segundo cuando estás BLOQUEADO (no te desplazas).")]
    [Min(0f)] public float rampRate = 2f;
    [Tooltip("ATP/s por unidad de power empleado (más fuerza → más cansancio).")]
    [Min(0f)] public float casterEnergyPerPower = 0.3f;
    [Tooltip("Desplazamiento (m/frame) por debajo del cual se considera 'bloqueado' y sube la fuerza.")]
    [Min(0f)] public float blockedEpsilon = 0.001f;

    Anima   _self;
    float   _power;
    Vector3 _lastPos;

    void Awake()
    {
        _self = GetComponent<Anima>();
        castMode = CastMode.Channel;
    }

    void Update()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude < 0.01f) { _power = 0f; _lastPos = transform.position; return; }
        input.Normalize();

        float mass = _self != null ? _self.BodyMass : 1f;
        float cap  = force + (_self != null ? _self.Strength : 0f) + mass;   // techo: tu fuerza (+ vencer tu peso)
        if (_power <= 0f) _power = mass;                                     // mínimo para mover tu propio peso

        // ¿Me desplacé el frame anterior? Si no (bloqueado), empuja más fuerte.
        float moved = (transform.position - _lastPos).magnitude;
        if (moved < blockedEpsilon) _power = Mathf.Min(cap, _power + rampRate * Time.deltaTime);
        _lastPos = transform.position;

        // Cansancio: ATP ∝ power. Sin energía, no puedes avanzar.
        if (_self != null && casterEnergyPerPower > 0f)
        {
            CharacterLevel cl = _self.GetComponent<CharacterLevel>();
            if (cl != null && !cl.SpendEnergy(casterEnergyPerPower * _power * Time.deltaTime)) return;
        }

        // Velocidad = base × (fuerza empleada / tu peso): más fuerza relativa = te mueves mejor.
        float speed = baseSpeed * (_power / Mathf.Max(0.1f, mass));
        transform.position += input * speed * Time.deltaTime;
    }

    // No usa la API dirigida (el objetivo es siempre uno mismo).
    public override bool CanCast(Anima caster, ITarget target) => true;
    public override void Cast(Anima caster, ITarget target) { }
}
