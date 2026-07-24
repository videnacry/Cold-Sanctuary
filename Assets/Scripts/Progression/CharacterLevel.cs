using System;
using UnityEngine;

/// <summary>
/// XP y nivel de un personaje, con **pools DERIVADOS de las aptitudes** (docs/creature-stats.md §Pools
/// derivados; world-topology-and-planes.md §4.1): vida, energía, maná, defensa pasiva y poder de hechizo
/// salen de las aptitudes vía <see cref="DerivedStats"/>, no se ponen a mano.
///
/// El farming da XP → sube nivel → los pools crecen. De momento la vida/energía/maná viven aquí; el
/// combate real y el consumo de energía (asanas/correr/trepar) se enganchan más adelante (y con
/// `NPCBase`, la fuente de aptitudes se unificará entre companions/jugador/animales).
/// </summary>
public class CharacterLevel : MonoBehaviour
{
    [Header("Nivel")]
    [Min(1)] public int level = 1;
    [Min(0f)] public float xp = 0f;

    [Tooltip("XP para pasar de nivel 1 → 2. Cada nivel siguiente cuesta ×xpCurve.")]
    [Min(1f)] public float baseXpToNext = 100f;
    [Min(1f)] public float xpCurve = 1.3f;

    [Header("Aptitudes (derivan los pools). 1.0 = media.")]
    public Aptitudes aptitudes = Aptitudes.Default;

    // Pools máximos derivados (docs §Pools derivados).
    public float MaxHealth      => DerivedStats.MaxHealth(aptitudes, level);
    public float MaxEnergy      => DerivedStats.MaxEnergy(aptitudes, level);
    public float MaxMana        => DerivedStats.MaxMana(aptitudes, level);
    public float PassiveDefense => DerivedStats.PassiveDefense(aptitudes);
    public float SpellPower     => DerivedStats.SpellPower(aptitudes, level);

    [Header("Pools actuales (los llena Awake y al subir de nivel)")]
    public float currentHealth;
    public float currentEnergy;
    public float currentMana;

    /// <summary>XP necesaria para el siguiente nivel desde el nivel actual.</summary>
    public float XpToNext => baseXpToNext * Mathf.Pow(xpCurve, level - 1);

    /// <summary>Disparado al subir de nivel (nuevo nivel).</summary>
    public event Action<int> OnLevelUp;

    void Awake()
    {
        // Si hay un companion en el mismo objeto y ya tiene aptitudes fijadas, deriva de las SUYAS.
        if (TryGetComponent(out CompanionBase comp) && (comp.strength > 0f || comp.endurance > 0f))
            aptitudes = Aptitudes.From(comp);

        RefillAll();
    }

    void RefillAll()
    {
        currentHealth = MaxHealth;
        currentEnergy = MaxEnergy;
        currentMana   = MaxMana;
    }

    public void GainXp(float amount)
    {
        if (amount <= 0f) return;
        xp += amount;

        // Puede subir varios niveles de golpe con recompensas grandes.
        while (xp >= XpToNext)
        {
            xp -= XpToNext;
            level++;
            RefillAll();  // subir de nivel cura del todo (los pools crecen con el nivel)
            Debug.Log($"[Nivel] «{name}» subió a nivel {level} — Vida {MaxHealth:0}, Energía {MaxEnergy:0}, Maná {MaxMana:0}.");
            OnLevelUp?.Invoke(level);
        }
    }

    /// <summary>Daño recibido: se le resta la defensa pasiva (mínimo 0). No baja de 0.</summary>
    public void TakeDamage(float dmg)
    {
        if (dmg <= 0f) return;
        float effective = Mathf.Max(0f, dmg - PassiveDefense);
        if (effective <= 0f)
        {
            Debug.Log($"[Nivel] «{name}» absorbió el golpe (defensa {PassiveDefense:0}).");
            return;
        }
        currentHealth = Mathf.Max(0f, currentHealth - effective);
        Debug.Log($"[Nivel] «{name}» recibió {effective:0.0} (def {PassiveDefense:0}) — Vida {currentHealth:0}/{MaxHealth:0}.");
    }

    /// <summary>Gasta energía (asanas, correr, trepar). Devuelve false si no hay suficiente.</summary>
    public bool SpendEnergy(float amount)
    {
        if (amount <= 0f) return true;
        if (currentEnergy < amount) return false;
        currentEnergy -= amount;
        return true;
    }
}
