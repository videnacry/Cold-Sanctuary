using UnityEngine;

/// <summary>
/// Trepar (docs/creature-stats.md §Pools derivados → "Trepar"). Cerca de un <see cref="Climbable"/>,
/// mantén la tecla de trepar para subir:
///   - **Requiere fuerza mínima** para poder trepar.
///   - **Velocidad ∝ strength · agility.**
///   - **Altura máxima ∝ strength / bodyMass** (potencia-peso), tope en `Climbable.topY`.
///   - **Coste de energía por segundo ∝ bodyMass / strength** (se paga vía `CharacterLevel.SpendEnergy`;
///     sin energía, no sube).
///
/// Autocontenido: mientras trepa, desactiva el `CharacterController` y mueve el transform directamente
/// (evita pelear con la gravedad del `PlayerController`); al soltar, reactiva el CC y la gravedad normal
/// hace el resto. ⚠️ Requiere verificación de feel/física en el editor.
/// </summary>
public class PlayerClimber : MonoBehaviour
{
    [Header("Input / detección")]
    public KeyCode climbKey = KeyCode.Space;
    [Min(0.5f)] public float detectRange = 2f;

    [Header("Requisitos y ritmo")]
    [Tooltip("Fuerza mínima (aptitud) para poder trepar.")]
    public float minStrengthToClimb = 0.8f;
    [Tooltip("Velocidad vertical base (m/s) a aptitudes 1; se multiplica por strength·agility.")]
    [Min(0f)] public float baseClimbSpeed = 1.5f;
    [Tooltip("Alcance base sobre el punto de inicio (m) a strength/bodyMass = 1.")]
    [Min(0f)] public float baseReach = 4f;
    [Tooltip("Coste base de energía por segundo (se escala por bodyMass/strength).")]
    [Min(0f)] public float baseEnergyPerSecond = 4f;

    CharacterController _cc;
    CharacterLevel      _level;
    bool  _climbing;
    float _startY;
    float _maxY;

    void Awake()
    {
        _cc    = GetComponent<CharacterController>();
        _level = GetComponent<CharacterLevel>();
    }

    void Update()
    {
        if (!Input.GetKey(climbKey)) { StopClimb(); return; }

        Climbable target = FindClimbable();
        if (target == null) { StopClimb(); return; }

        float strength = _level != null ? Mathf.Max(0.01f, _level.aptitudes.strength) : 1f;
        float agility  = _level != null ? Mathf.Max(0.01f, _level.aptitudes.agility)  : 1f;
        float mass     = _level != null ? Mathf.Max(0.1f,  _level.aptitudes.bodyMass) : 1f;

        if (strength < minStrengthToClimb) return; // demasiado débil para trepar

        if (!_climbing) BeginClimb(target, strength, mass);

        // Energía: coste por segundo escalado por peso/fuerza. Sin energía, no sube.
        float dt = Time.deltaTime;
        float cost = baseEnergyPerSecond * (mass / strength) * dt;
        if (_level != null && !_level.SpendEnergy(cost)) return;

        // Sube a velocidad ∝ strength·agility, sin pasar del tope.
        float speed = baseClimbSpeed * strength * agility;
        float newY  = Mathf.Min(transform.position.y + speed * dt, _maxY);
        MoveToY(newY);
    }

    void BeginClimb(Climbable target, float strength, float mass)
    {
        _climbing = true;
        _startY   = transform.position.y;
        float reach = baseReach * (strength / mass);
        _maxY = Mathf.Min(_startY + reach, target.topY);
        if (_cc != null) _cc.enabled = false; // evita que la gravedad pelee mientras trepamos
    }

    void StopClimb()
    {
        if (!_climbing) return;
        _climbing = false;
        if (_cc != null) _cc.enabled = true;  // reactiva CC → gravedad normal (bajar/caer)
    }

    void MoveToY(float y)
    {
        Vector3 p = transform.position;
        p.y = y;
        transform.position = p;
    }

    Climbable FindClimbable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRange);
        Climbable best = null;
        float bestSqr = float.MaxValue;
        foreach (Collider col in hits)
        {
            Climbable c = col.GetComponentInParent<Climbable>();
            if (c == null) continue;
            float d = (c.transform.position - transform.position).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; best = c; }
        }
        return best;
    }
}
