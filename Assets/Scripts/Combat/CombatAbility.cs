using UnityEngine;

/// <summary>
/// Defines one combat ability usable against an IngredientMob.
/// Create via Assets > Create > Cold Sanctuary > Combat Ability.
///
/// Abilities are unlocked progressively (like WoW's action bar growing over time).
/// Each ability has an element affinity — using an ability that matches the mob's
/// element deals bonus damage, creating a strategic layer:
///   Water mob → use Hydrogen ability → 1.5× damage
///   Fire mob  → use Oxygen ability   → 1.5× damage
///
/// Abilities map to keyboard keys (1–0) AND appear as buttons in the Palette
/// when the player clicks a mob with the mouse.
/// </summary>
[CreateAssetMenu(menuName = "Cold Sanctuary/Combat Ability", fileName = "NewAbility")]
public class CombatAbility : ScriptableObject
{
    // ── Identity ──────────────────────────────────────────────────────────────

    [Header("Identity")]
    public string abilityName;

    [TextArea(1, 3)]
    public string description;

    public Sprite icon;

    // ── Key binding ───────────────────────────────────────────────────────────

    [Header("Key Binding")]
    [Tooltip("Keyboard key (1–0 for slots 1–10). Used when a target is selected.")]
    public KeyCode hotkey = KeyCode.Alpha1;

    // ── Combat stats ──────────────────────────────────────────────────────────

    [Header("Combat")]
    [Tooltip("Base damage dealt to the mob.")]
    public float damage = 15f;

    [Tooltip("Element symbol this ability is aligned with (e.g. 'S', 'H', 'C'). " +
             "Deals bonus damage vs mobs that share the element.")]
    public string elementAffinity;

    [Tooltip("Damage multiplier when element affinity matches the mob's element.")]
    public float affinityBonus = 1.5f;

    [Tooltip("Seconds before this ability can be used again.")]
    public float cooldown = 3f;

    [Tooltip("Mental energy cost (drained from MentalFatigue channel). 0 = free.")]
    public float energyCost = 0.02f;

    // ── AOE ───────────────────────────────────────────────────────────────────

    [Header("AOE (optional)")]
    [Tooltip("If > 0, hits all mobs within this radius of the primary target.")]
    public float aoeRadius = 0f;

    // ── Unlock ────────────────────────────────────────────────────────────────

    [Header("Unlock")]
    [Tooltip("Player progression level required to unlock this ability.")]
    public int unlockLevel = 0;

    [Tooltip("Element that must be discovered in PeriodicTableManager before this ability unlocks.")]
    public string requiredElement;
}
