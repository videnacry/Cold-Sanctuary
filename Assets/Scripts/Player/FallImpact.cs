using UnityEngine;

/// <summary>
/// IMPACTO DE LAS CAÍDAS: mide la altura de cada caída (desde que el cuerpo deja de pisar suelo hasta que vuelve a
/// pisarlo) y, por encima de un umbral seguro, aplica DAÑO según la energía del golpe (∝ altura) menos lo que AMORTIGUE la
/// superficie (<see cref="SoftGround"/>: el colchón de musgo bajo las construcciones elevadas absorbe casi todo) y la
/// defensa pasiva del cuerpo. Caer al AGUA no hace daño. Se usa en el jugador (CharacterController).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FallImpact : MonoBehaviour
{
    [Tooltip("Caídas por debajo de esta altura no hacen daño (saltar, bajar un escalón).")]
    public float safeHeight = 2.5f;
    [Tooltip("Daño por metro por encima de la altura segura.")]
    public float damagePerMeter = 12f;

    CharacterController _cc;
    CharacterLevel _level;
    bool _airborne;
    float _peakY;
    Vector3 _takeoff;
    public Vector3 LastTakeoff => _takeoff;

    public float LastFallHeight { get; private set; }
    public float LastDamage { get; private set; }

    void Awake() { _cc = GetComponent<CharacterController>(); _level = GetComponent<CharacterLevel>(); }

    void LateUpdate()
    {
        if (_cc == null || !_cc.enabled) { _airborne = false; return; }   // teletransporte / escalera / tanque: no cuenta
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null && pc.IsSwimming) { _airborne = false; return; }  // al agua: sin golpe
        if (!_cc.isGrounded)
        {
            if (!_airborne) { _airborne = true; _peakY = transform.position.y; _takeoff = transform.position; }
            else _peakY = Mathf.Max(_peakY, transform.position.y);
            return;
        }
        if (!_airborne) return;
        _airborne = false;
        Land(_peakY - transform.position.y, SoftnessBelow());
    }

    /// <summary>Aterriza tras caer <paramref name="height"/> m sobre una superficie de <paramref name="softness"/> (0–1).</summary>
    public float Land(float height, float softness)
    {
        LastFallHeight = height; LastDamage = 0f;
        if (height <= safeHeight) return 0f;
        float dmg = (height - safeHeight) * damagePerMeter * (1f - Mathf.Clamp01(softness));
        LastDamage = dmg;
        if (dmg > 0.5f)
        {
            if (_level != null) _level.TakeDamage(dmg);
            Debug.Log($"[Caída] «{name}» cae {height:0.0} m desde ({_takeoff.x:0.0}, {_takeoff.y:0.0}, {_takeoff.z:0.0}){(softness > 0f ? $" sobre algo blando ({softness:P0})" : "")}: {dmg:0} de daño.");
        }
        else if (softness > 0f) Debug.Log($"[Caída] «{name}» cae {height:0.0} m sobre el colchón de musgo: sin daño.");
        return dmg;
    }

    float SoftnessBelow()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3f, ~0, QueryTriggerInteraction.Ignore))
        {
            SoftGround s = hit.collider.GetComponentInParent<SoftGround>();
            if (s != null) return s.softness;
        }
        return 0f;
    }
}
