using UnityEngine;

/// <summary>
/// Farming NO-violento (docs/world-topology-and-planes.md §4.1) — MVP.
///
/// El "combate" es jugar con la criatura para **descargar su tensión** (no dañarla). Cuando la tensión
/// llega a 0 queda **serena** (deja de ser jugable), suelta recompensa, y entonces el jugador le **da
/// comida y agua** (interacción F/clic) para que **se sacie y descanse**.
///
/// Reutiliza lo que ya existe: si el GameObject tiene un <see cref="LivingEntity"/>, la tensión ES su
/// <c>stress</c> (0–1) y el cuidado sube su <c>fatReserves</c>. Sin LivingEntity (criatura placeholder),
/// usa una tensión local — así el MVP es probable con capsulitas de prueba.
///
/// Implementa <see cref="IInteractable"/>: solo se puede "dar de comer" una vez serena.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PlayableCreature : MonoBehaviour, IInteractable
{
    public enum State { Playful, Serene, Resting }

    [Header("Tensión")]
    [Tooltip("Cuánta tensión (0–1) descarga cada 'toque de juego'. Criaturas poderosas: menor = más difícil.")]
    [Range(0.01f, 1f)] public float dischargePerTouch = 0.12f;

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

    LivingEntity _living;      // opcional — si está, la tensión ES su stress (0–1)
    float        _localTension = 1f;  // usado solo si no hay LivingEntity
    Renderer     _renderer;

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
        _renderer = GetComponentInChildren<Renderer>();
        Tension = 1f;
    }

    /// <summary>Un 'toque de juego': baja la tensión. Al llegar a 0, la criatura se serena.</summary>
    public void Play() => Discharge(dischargePerTouch);

    public void Discharge(float amount)
    {
        if (CurrentState != State.Playful || amount <= 0f) return;
        Tension -= amount;
        if (Tension <= 0f) BecomeSerene();
    }

    void BecomeSerene()
    {
        CurrentState = State.Serene;
        Debug.Log($"[Farming] «{name}» serena — descargó toda su tensión. Suelta recompensa " +
                  $"({dropAmount:0} {dropResource}" + (dropCoins > 0 ? $" + {dropCoins} monedas" : "") + ").");
        SanctuaryResources.Instance.Add(dropResource, dropAmount);
        if (dropCoins > 0) CoinWallet.Instance?.Earn(dropCoins);
        Tint(new Color(0.55f, 0.85f, 0.55f)); // verde sereno
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
        // Realimentación visual: mientras juega, rojo (tensa) → verde (serena).
        if (CurrentState == State.Playful)
            Tint(Color.Lerp(new Color(0.55f, 0.85f, 0.55f), new Color(0.90f, 0.35f, 0.30f), Tension));
    }

    void Tint(Color c)
    {
        if (_renderer != null) _renderer.material.color = c;
    }
}
