using UnityEngine;

/// <summary>
/// Farming NO-violento (docs/world-topology-and-planes.md §4.1).
///
/// El "combate" es jugar con la criatura para **descargar su tensión** (no dañarla). Cuando la tensión
/// llega a 0 queda **serena** (deja de ser jugable), suelta recompensa, y entonces el jugador le **da
/// comida y agua** (interacción F/clic) para que **se sacie y descanse**.
///
/// V2 — el *feel* gato/perro:
///   - **Excitación/combo:** cada 'toque' sube la excitación; la descarga escala con ella
///     (provocar bien = descargar más rápido). La excitación decae si dejas de jugar.
///   - **Atrapada:** si te quedas pegado (dentro de <see cref="catchRange"/>) más de
///     <see cref="catchGrace"/> segundos, la criatura te "atrapa" (juego, sin daño) y **resetea la
///     excitación** → fuerza el ritmo acercar-provocar-alejar.
///   - **Reacción:** mientras juega te mira y, si está excitada, se acerca (con correa a su sitio).
///
/// Reutiliza lo que ya existe: si el GameObject tiene un <see cref="LivingEntity"/>, la tensión ES su
/// <c>stress</c> (0–1) y el cuidado sube su <c>fatReserves</c>. Sin LivingEntity (placeholder), usa una
/// tensión local. Implementa <see cref="IInteractable"/>: solo se puede "dar de comer" una vez serena.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PlayableCreature : MonoBehaviour, IInteractable
{
    public enum State { Playful, Serene, Resting }

    [Header("Tensión")]
    [Tooltip("Tensión base (0–1) que descarga un 'toque' SIN excitación. Menor = más difícil.")]
    [Range(0.01f, 1f)] public float dischargePerTouch = 0.08f;

    [Header("Juego (V2): excitación y combo")]
    [Tooltip("Cuánta excitación (0–1) sube cada toque.")]
    [Range(0f, 1f)] public float excitementPerTap = 0.2f;
    [Tooltip("Excitación que se pierde por segundo si dejas de jugar.")]
    [Min(0f)] public float excitementDecayPerSec = 0.4f;
    [Tooltip("La descarga se multiplica por (1 + excitación × esto). Combo lleno = mucho más rápido.")]
    [Min(0f)] public float comboMultiplier = 2.5f;

    [Header("Juego (V2): atrapada")]
    [Tooltip("Si te quedas dentro de este radio demasiado tiempo, te atrapa.")]
    [Min(0.1f)] public float catchRange = 1.2f;
    [Tooltip("Segundos pegado antes de que te atrape (y resetee el combo).")]
    [Min(0.1f)] public float catchGrace = 1.0f;

    [Header("Juego (V2): reacción")]
    [Tooltip("Velocidad a la que se acerca al jugador cuando está excitada (m/s a excitación llena).")]
    [Min(0f)] public float approachSpeed = 1.2f;
    [Tooltip("No se aleja más de esto de su sitio de origen (correa).")]
    [Min(0f)] public float leash = 4f;

    [Header("Recompensa al serenarse")]
    public SanctuaryResource dropResource = SanctuaryResource.Materials;
    [Min(0f)] public float dropAmount = 20f;
    [Min(0)]  public int   dropCoins  = 3;

    [Header("Cuidado (dar comida y agua)")]
    [Tooltip("fatReserves que restaura al cuidarla (solo si tiene LivingEntity).")]
    [Min(0f)] public float feedAmount = 1f;

    public State CurrentState { get; private set; } = State.Playful;

    /// <summary>Solo es objetivo de juego mientras aún tiene tensión que descargar.</summary>
    public bool IsPlayable => CurrentState == State.Playful;

    /// <summary>Excitación actual 0..1 (para feedback/depuración).</summary>
    public float Excitement => _excitement;

    LivingEntity _living;      // opcional — si está, la tensión ES su stress (0–1)
    float        _localTension = 1f;  // usado solo si no hay LivingEntity
    Renderer     _renderer;

    float     _excitement;
    float     _closeTimer;
    float     _caughtCooldown;
    Transform _player;
    Vector3   _home;
    Vector3   _baseScale;

    /// <summary>Tensión 0..1 (1 = máxima, 0 = serena).</summary>
    public float Tension
    {
        get => _living != null ? Mathf.Clamp01(_living.stress) : _localTension;
        private set
        {
            float v = Mathf.Clamp01(value);
            if (_living != null) _living.stress = v; else _localTension = v;
        }
    }

    void Awake()
    {
        TryGetComponent(out _living);
        _renderer  = GetComponentInChildren<Renderer>();
        _home      = transform.position;
        _baseScale = transform.localScale;
        Tension    = 1f;
    }

    /// <summary>Un 'toque de juego': sube excitación y descarga tensión (más con más combo).</summary>
    public void Play()
    {
        if (CurrentState != State.Playful || _caughtCooldown > 0f) return;

        float boosted = dischargePerTouch * (1f + _excitement * comboMultiplier);
        Discharge(boosted);
        _excitement = Mathf.Clamp01(_excitement + excitementPerTap);
    }

    public void Discharge(float amount)
    {
        if (CurrentState != State.Playful || amount <= 0f) return;
        Tension -= amount;
        if (Tension <= 0f) BecomeSerene();
    }

    void BecomeSerene()
    {
        CurrentState = State.Serene;
        transform.localScale = _baseScale;
        Debug.Log($"[Farming] «{name}» serena — descargó toda su tensión. Suelta recompensa " +
                  $"({dropAmount:0} {dropResource}" + (dropCoins > 0 ? $" + {dropCoins} monedas" : "") + ").");
        SanctuaryResources.Instance.Add(dropResource, dropAmount);
        if (dropCoins > 0) CoinWallet.Instance?.Earn(dropCoins);
        Tint(new Color(0.55f, 0.85f, 0.55f)); // verde sereno
    }

    void GetCaught()
    {
        _excitement     = 0f;
        _closeTimer     = 0f;
        _caughtCooldown = 0.6f;

        // Salto de retirada hacia su sitio (juego, sin daño).
        Vector3 back = _home - transform.position; back.y = 0f;
        if (back.sqrMagnitude > 0.001f)
            transform.position += back.normalized * 0.5f;

        Debug.Log($"[Farming] «{name}» te atrapó jugando — ¡retrocede y vuelve a provocarla!");
    }

    // ── IInteractable — dar comida y agua (solo cuando está serena) ─────────────

    public string InteractLabel => "Dar comida y agua";
    public bool   CanInteract   => CurrentState == State.Serene;

    public void Interact() => FeedAndRest();

    public void FeedAndRest()
    {
        if (CurrentState != State.Serene) return;
        CurrentState = State.Resting;
        if (_living != null) _living.fatReserves += feedAmount;
        Debug.Log($"[Farming] «{name}» saciada y descansando.");
        Tint(new Color(0.55f, 0.70f, 0.90f)); // azul calmado
    }

    void Update()
    {
        if (CurrentState != State.Playful) return;

        float dt = Time.deltaTime;
        _caughtCooldown = Mathf.Max(0f, _caughtCooldown - dt);
        _excitement     = Mathf.Max(0f, _excitement - excitementDecayPerSec * dt);

        ReactToPlayer(dt);

        // Feedback: tensión por color (verde→rojo) + rebote de escala con la excitación.
        Tint(Color.Lerp(new Color(0.55f, 0.85f, 0.55f), new Color(0.90f, 0.35f, 0.30f), Tension));
        transform.localScale = _baseScale * (1f + 0.12f * _excitement);
    }

    void ReactToPlayer(float dt)
    {
        if (_player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.transform;
            if (_player == null) return;
        }

        Vector3 delta = _player.position - transform.position; delta.y = 0f;
        float dist = delta.magnitude;

        // Mira al jugador.
        if (delta.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(delta), 5f * dt);

        // Atrapada por quedarte pegado demasiado tiempo.
        if (dist < catchRange)
        {
            _closeTimer += dt;
            if (_closeTimer >= catchGrace) GetCaught();
        }
        else
        {
            _closeTimer = 0f;
        }

        // Se acerca cuando está excitada (quiere atraparte), con correa a su sitio.
        if (_excitement > 0.1f && dist > catchRange &&
            Vector3.Distance(_home, transform.position) < leash)
        {
            transform.position += delta.normalized * (approachSpeed * _excitement * dt);
        }
    }

    void Tint(Color c)
    {
        if (_renderer != null) _renderer.material.color = c;
    }
}
