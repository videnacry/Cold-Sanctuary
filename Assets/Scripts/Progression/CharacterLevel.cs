using System;
using UnityEngine;

/// <summary>
/// XP y nivel de un personaje (docs/world-topology-and-planes.md §4.1: el farming da XP → sube nivel
/// → +vida/+maná). Sistema base NUEVO: mantiene xp/level y deriva `MaxHealth`/`MaxMana`.
///
/// De momento la vida/maná son stats propias de este componente: el combate del jugador aún no las
/// consume (no existían). Se integrará con `PlayerStats.progressionLevel` y el combate más adelante;
/// por ahora es la base sobre la que enganchar esas mecánicas.
/// </summary>
public class CharacterLevel : MonoBehaviour
{
    [Header("Nivel")]
    [Min(1)] public int level = 1;
    [Min(0f)] public float xp = 0f;

    [Tooltip("XP para pasar de nivel 1 → 2. Cada nivel siguiente cuesta ×xpCurve.")]
    [Min(1f)] public float baseXpToNext = 100f;
    [Min(1f)] public float xpCurve = 1.3f;

    [Header("Vida / Maná (crecen por nivel)")]
    [Min(1f)] public float baseMaxHealth = 100f;
    [Min(0f)] public float healthPerLevel = 20f;
    [Min(1f)] public float baseMaxMana = 50f;
    [Min(0f)] public float manaPerLevel = 10f;

    public float MaxHealth => baseMaxHealth + healthPerLevel * (level - 1);
    public float MaxMana   => baseMaxMana   + manaPerLevel   * (level - 1);

    /// <summary>XP necesaria para el siguiente nivel desde el nivel actual.</summary>
    public float XpToNext => baseXpToNext * Mathf.Pow(xpCurve, level - 1);

    /// <summary>Disparado al subir de nivel (nuevo nivel).</summary>
    public event Action<int> OnLevelUp;

    public void GainXp(float amount)
    {
        if (amount <= 0f) return;
        xp += amount;

        // Puede subir varios niveles de golpe con recompensas grandes.
        while (xp >= XpToNext)
        {
            xp -= XpToNext;
            level++;
            Debug.Log($"[Nivel] «{name}» subió a nivel {level} — Vida {MaxHealth:0}, Maná {MaxMana:0}.");
            OnLevelUp?.Invoke(level);
        }
    }
}
