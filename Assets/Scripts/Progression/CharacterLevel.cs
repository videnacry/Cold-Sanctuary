using UnityEngine;

/// <summary>
/// Progresión de un personaje por **margas del alma** (docs/creature-stats.md §Progresión): varios
/// tracks independientes (Stats, Yoga, Vínculos…), cada uno con su pool de XP y nivel. Los **pools**
/// (vida/energía/maná/defensa/poder) se **derivan de las aptitudes** (`DerivedStats`), escalados por el
/// nivel de la marga de **Stats** (la del cuerpo/alma) — subir esa marga = el "incremento de la base".
///
/// El maná existe siempre, pero su barra **se desbloquea al practicar yoga** (marga de Yoga ≥ 2): solo
/// afecta a la VISIBILIDAD/uso, no a cuánto vale (eso son aptitudes+nivel). Ver docs.
///
/// Fuentes de XP hoy: el farming alimenta la marga de Stats (`GainXp`). Yoga/Vínculos se cablearán en
/// sus sistemas (`GainYogaXp`/`GainBondXp`).
/// </summary>
public class CharacterLevel : MonoBehaviour
{
    [Header("Margas del alma (tracks independientes)")]
    public SoulMarga stats = new SoulMarga { name = "Stats" };
    public SoulMarga yoga  = new SoulMarga { name = "Yoga" };
    public SoulMarga bonds = new SoulMarga { name = "Vínculos" };

    [Header("Aptitudes (derivan los pools). 1.0 = media.")]
    public Aptitudes aptitudes = Aptitudes.Default;

    [Tooltip("Si true, deriva las aptitudes del IAptitudes del mismo objeto (LivingEntity/CompanionBase/" +
             "PlayerStats). Si false, usa el campo 'aptitudes' de arriba (p.ej. perfil fijado a mano/builder).")]
    public bool deriveAptitudesFromComponent = false;

    // Pools máximos derivados; escalan por el nivel de la marga de Stats.
    public float MaxHealth      => DerivedStats.MaxHealth(aptitudes, stats.level);
    public float MaxEnergy      => DerivedStats.MaxEnergy(aptitudes, stats.level);
    public float MaxMana        => DerivedStats.MaxMana(aptitudes, stats.level);
    public float PassiveDefense => DerivedStats.PassiveDefense(aptitudes);
    public float SpellPower     => DerivedStats.SpellPower(aptitudes, stats.level);

    /// <summary>La barra de maná se desbloquea al subir la marga de Yoga (solo visibilidad/uso).</summary>
    public bool ManaUnlocked => yoga.level >= 2;

    [Header("Pools actuales (los llena Awake y al subir la marga de Stats)")]
    public float currentHealth;
    public float currentEnergy;
    public float currentMana;

    void Awake()
    {
        // Opt-in: derivar de las aptitudes del ser vivo en el mismo objeto (cualquier IAptitudes).
        if (deriveAptitudesFromComponent)
        {
            IAptitudes src = GetComponent<IAptitudes>();
            if (src != null) aptitudes = Aptitudes.From(src);
        }
        RefillAll();
    }

    void RefillAll()
    {
        currentHealth = MaxHealth;
        currentEnergy = MaxEnergy;
        currentMana   = MaxMana;
    }

    // ── Fuentes de XP por marga ─────────────────────────────────────────────────

    /// <summary>Compatibilidad: el farming alimenta la marga de Stats.</summary>
    public void GainXp(float amount) => GainStatsXp(amount);

    public void GainStatsXp(float amount)
    {
        int levels = stats.GainXp(amount);
        if (levels > 0)
        {
            RefillAll();  // los pools crecieron con el nivel de Stats → cura del todo
            Debug.Log($"[Marga] «{name}» Stats nivel {stats.level} — Vida {MaxHealth:0}, Energía {MaxEnergy:0}, Maná {MaxMana:0}.");
        }
    }

    public void GainYogaXp(float amount)
    {
        int levels = yoga.GainXp(amount);
        if (levels > 0)
            Debug.Log($"[Marga] «{name}» Yoga nivel {yoga.level}" + (ManaUnlocked ? " — barra de maná desbloqueada." : "."));
    }

    public void GainBondXp(float amount)
    {
        int levels = bonds.GainXp(amount);
        if (levels > 0) Debug.Log($"[Marga] «{name}» Vínculos nivel {bonds.level}.");
    }

    // ── Vida / energía ───────────────────────────────────────────────────────────

    /// <summary>Daño recibido: se le resta la defensa pasiva (mínimo 0). No baja de 0.</summary>
    public void TakeDamage(float dmg)
    {
        if (dmg <= 0f) return;
        float effective = Mathf.Max(0f, dmg - PassiveDefense);
        if (effective <= 0f)
        {
            Debug.Log($"[Alma] «{name}» absorbió el golpe (defensa {PassiveDefense:0}).");
            return;
        }
        currentHealth = Mathf.Max(0f, currentHealth - effective);
        Debug.Log($"[Alma] «{name}» recibió {effective:0.0} (def {PassiveDefense:0}) — Vida {currentHealth:0}/{MaxHealth:0}.");
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
