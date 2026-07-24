using UnityEngine;

/// <summary>
/// Una "marga del alma" (docs/creature-stats.md §Progresión): un track de progresión **independiente**
/// con su propio **pool de XP** y su **nivel**. Varias margas coexisten (Stats, Yoga, Vínculos, futura
/// Hechizos) y son independientes entre sí — p.ej. nivel 20 en Stats y 1 en Yoga.
///
/// Serializable para tunear la curva por marga en el Inspector.
/// </summary>
[System.Serializable]
public class SoulMarga
{
    public string name = "marga";
    [Min(1)]  public int   level = 1;
    [Min(0f)] public float xp = 0f;

    [Tooltip("XP para pasar de nivel 1 → 2. Cada nivel siguiente cuesta ×xpCurve.")]
    [Min(1f)] public float baseXpToNext = 100f;
    [Min(1f)] public float xpCurve = 1.3f;

    public float XpToNext => baseXpToNext * Mathf.Pow(xpCurve, level - 1);

    /// <summary>Añade XP a esta marga; devuelve cuántos niveles subió (0 si ninguno).</summary>
    public int GainXp(float amount)
    {
        if (amount <= 0f) return 0;
        xp += amount;
        int gained = 0;
        while (xp >= XpToNext) { xp -= XpToNext; level++; gained++; }
        return gained;
    }
}
