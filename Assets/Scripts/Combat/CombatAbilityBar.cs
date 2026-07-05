using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the player's equipped ability bar (up to 10 slots, keys 1–0).
///
/// Two input modes — both do exactly the same thing:
///   Keyboard: 1–0 use the ability in that slot on CombatTargetSelector.CurrentTarget.
///   Mouse:    clicking a mob opens the Palette; ability buttons appear there;
///             clicking one calls UseAbility(index) on the current target.
///
/// Palette integration:
///   OpenAbilityPalette() builds a PaletteConfig.Direct from the ability bar
///   and opens the existing Palette UI. The Palette fires OnElementChosen(index)
///   → UseAbility(index). No new UI code needed.
///
/// Abilities unlock progressively: unlocked ones appear in the bar,
/// locked ones show as greyed-out in the Palette.
/// </summary>
public class CombatAbilityBar : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────

    public static CombatAbilityBar Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Ability Pool")]
    [Tooltip("All abilities available in the game. Unlocked subset appears in the bar.")]
    public CombatAbility[] allAbilities;

    [Header("Palette")]
    [Tooltip("The Palette UI used for mouse-based ability selection. " +
             "Drag the Palette GameObject here from the scene.")]
    public Palette palette;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired when an ability is used. (ability, target, damageDealt)</summary>
    public event Action<CombatAbility, IngredientMob, float> OnAbilityUsed;

    // ── Runtime ───────────────────────────────────────────────────────────────

    PlayerStats _stats;

    // Per-ability cooldown tracking (ability → timeReady)
    readonly Dictionary<CombatAbility, float> _cooldowns = new Dictionary<CombatAbility, float>();

    // Abilities currently shown in the bar (unlocked, ordered by hotkey)
    readonly List<CombatAbility> _bar = new List<CombatAbility>();

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _stats = GetComponent<PlayerStats>();
        RefreshBar();
    }

    void Update()
    {
        HandleKeyboardInput();
    }

    // ── Bar management ────────────────────────────────────────────────────────

    /// <summary>
    /// Rebuilds the active bar from allAbilities based on unlock conditions.
    /// Call when progression level changes or a new element is discovered.
    /// </summary>
    public void RefreshBar()
    {
        _bar.Clear();
        if (allAbilities == null) return;

        var player = GetComponent<WorldCharacter>();
        int level  = player != null ? player.progressionLevel : 0;

        foreach (var ability in allAbilities)
        {
            if (ability == null) continue;
            if (ability.unlockLevel > level) continue;
            if (!string.IsNullOrEmpty(ability.requiredElement)
                && !(PeriodicTableManager.Instance?.IsKnown(ability.requiredElement) ?? false))
                continue;

            _bar.Add(ability);
        }

        _bar.Sort((a, b) => a.hotkey.CompareTo(b.hotkey));
        Debug.Log($"[AbilityBar] Habilidades activas: {_bar.Count}");
    }

    // ── Keyboard input ────────────────────────────────────────────────────────

    void HandleKeyboardInput()
    {
        for (int i = 0; i < _bar.Count && i < 10; i++)
        {
            if (Input.GetKeyDown(_bar[i].hotkey))
            {
                var target = CombatTargetSelector.Instance?.CurrentTarget;
                if (target != null) UseAbility(i, target);
                else Debug.Log("[AbilityBar] Sin target seleccionado.");
                break;
            }
        }
    }

    // ── Palette integration ───────────────────────────────────────────────────

    /// <summary>
    /// Builds a PaletteConfig.Direct from the current bar and opens the Palette UI.
    /// Called by CombatTargetSelector when the player clicks a mob with the mouse.
    ///
    /// Each ability becomes a PaletteElementData (Action type, payload = CombatAbility).
    /// When the player clicks a button, Palette fires OnActionFired → we cast the payload
    /// and call UseAbility on the current target.
    /// </summary>
    public void OpenAbilityPalette()
    {
        if (palette == null)
        {
            Debug.LogWarning("[AbilityBar] No hay referencia a Palette asignada.");
            return;
        }
        if (_bar.Count == 0)
        {
            Debug.Log("[AbilityBar] Sin habilidades desbloqueadas.");
            return;
        }

        // Build one PaletteElementData per ability in the bar
        var elements = new PaletteElementData[_bar.Count];
        for (int i = 0; i < _bar.Count; i++)
        {
            var ability = _bar[i];
            // bondMin trick: Palette locks elements when playerBond < bondMin.
            // We pass playerBond = 0 to Open(), so setting bondMin = 1 shows the
            // lockedOverlay for abilities that are on cooldown.
            bool onCooldown = IsOnCooldown(ability);

            elements[i] = new PaletteElementData
            {
                id       = ability.name,           // ScriptableObject asset name
                label    = ability.abilityName,
                icon     = ability.icon,
                shortcut = ability.hotkey,
                type     = PaletteElementType.Action,
                payload  = ability,
                bondMin  = onCooldown ? 1 : 0,    // 1 = locked overlay, 0 = available
            };
        }

        var config = new PaletteConfig
        {
            mode     = PaletteConfig.Mode.Direct,
            elements = elements,
        };

        // Unsubscribe first to avoid duplicate handlers on repeated opens
        palette.OnActionFired -= HandlePaletteAction;
        palette.OnActionFired += HandlePaletteAction;

        palette.Open(config, gameObject);
        Debug.Log($"[AbilityBar] Palette abierta con {_bar.Count} habilidades.");
    }

    void HandlePaletteAction(PaletteElementData data)
    {
        if (data.payload is CombatAbility ability)
        {
            var target = CombatTargetSelector.Instance?.CurrentTarget;
            if (target != null)
                UseAbility(ability, target);
            else
                Debug.Log("[AbilityBar] Sin target al activar habilidad desde Palette.");
        }
    }

    // ── Core use logic ────────────────────────────────────────────────────────

    /// <summary>Apply a bar-slot ability to a specific target.</summary>
    public void UseAbility(int barIndex, IngredientMob target)
    {
        if (barIndex < 0 || barIndex >= _bar.Count) return;
        var ability = _bar[barIndex];
        UseAbility(ability, target);
    }

    public void UseAbility(CombatAbility ability, IngredientMob target)
    {
        if (ability == null || target == null) return;

        // Cooldown check
        if (IsOnCooldown(ability))
        {
            float remaining = _cooldowns[ability] - Time.time;
            Debug.Log($"[AbilityBar] {ability.abilityName} en cooldown ({remaining:F1}s).");
            return;
        }

        // Energy check
        if (_stats != null && ability.energyCost > 0f)
        {
            if (_stats.mentalFatigue >= 0.95f)
            {
                Debug.Log($"[AbilityBar] Sin energía para {ability.abilityName}.");
                return;
            }
            _stats.DrainMind(ability.energyCost, MindChannel.MentalFatigue);
        }

        // Calculate damage
        float damage = ability.damage;
        if (!string.IsNullOrEmpty(ability.elementAffinity)
            && ability.elementAffinity == target.elementSymbol)
        {
            damage *= ability.affinityBonus;
            Debug.Log($"[AbilityBar] ¡Afinidad elemental! ×{ability.affinityBonus}");
        }

        // Apply damage (primary target)
        target.TakeDamage(damage);

        // AOE
        if (ability.aoeRadius > 0f)
            ApplyAOE(ability, target, damage);

        // Register cooldown
        _cooldowns[ability] = Time.time + ability.cooldown;

        OnAbilityUsed?.Invoke(ability, target, damage);

        // TODO: Animator.SetTrigger("ability_" + barIndex);
        // TODO: Spawn ability VFX at target position
        Debug.Log($"[AbilityBar] {ability.abilityName} → {target.ingredientName} ({damage:F1} daño).");
    }

    void ApplyAOE(CombatAbility ability, IngredientMob primaryTarget, float damage)
    {
        var cols = Physics.OverlapSphere(primaryTarget.transform.position, ability.aoeRadius);
        foreach (var col in cols)
        {
            var mob = col.GetComponentInParent<IngredientMob>();
            if (mob == null || mob == primaryTarget) continue;
            mob.TakeDamage(damage * 0.6f); // AOE hits for 60% of primary damage
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    public bool IsOnCooldown(CombatAbility ability)
        => _cooldowns.TryGetValue(ability, out float ready) && Time.time < ready;

    public float CooldownRemaining(CombatAbility ability)
        => _cooldowns.TryGetValue(ability, out float ready)
            ? Mathf.Max(0f, ready - Time.time)
            : 0f;

    public IReadOnlyList<CombatAbility> Bar => _bar;
}
