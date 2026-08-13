using UnityEngine;

/// <summary>
/// Hechizo de LA MALEZA DE AMBROSIO — restaura energía al target.
///
/// Es el PRIMER CONSUMIBLE del juego (Nivel 1, Microcosmos). Kushal recolecta las gotas
/// de melaza que produce Ambrosio (<see cref="HoneydewPickup"/>); cada pickup añade una
/// carga (<see cref="AddCharge"/>). Cada uso gasta una carga y restaura
/// <see cref="energyRestore"/> de energía al target (<see cref="ITarget"/>), lo que
/// temporalmente reactiva su <see cref="NavMeshAgent"/> (controlado por
/// <see cref="WeaknessEffect"/>).
///
/// El debilitamiento sigue drenando la energía → el jugador debe usarla en el momento justo
/// (cuando el target está cerca del checkpoint o cuando nadie más puede jalarlo).
///
/// Uso desde teclado: tecla E (configurable) lanza sobre el target del
/// <see cref="CombatTargetSelector"/>. Si no hay target, se aplica al propio caster.
///
/// Produce logs con la energía restaurada para verificar el loop en testing.
/// </summary>
public class HoneydewSpell : SpellBase
{
    [Header("Maleza — parámetros")]
    [Tooltip("Energía restaurada al target por uso (unidades de currentEnergy).")]
    [Min(1f)] public float energyRestore = 30f;

    [Tooltip("Si true y el target tiene WeaknessEffect pausado, Resume() se llama " +
             "automáticamente (la hormiga puede moverse de nuevo aunque sea un momento).")]
    public bool resumeWeakness = true;

    [Header("Cargas (consumible recolectado)")]
    [Tooltip("Cargas disponibles actualmente (cada HoneydewPickup suma 1).")]
    public int charges = 0;

    [Tooltip("Máximo de cargas que Kushal puede acumular.")]
    [Min(1)] public int maxCharges = 10;

    [Tooltip("Tecla para lanzar la maleza sobre el target seleccionado.")]
    public KeyCode castKey = KeyCode.E;

    // ── Ciclo de vida ────────────────────────────────────────────────────────

    Anima _self;

    void Awake() => _self = GetComponent<Anima>();

    void Update()
    {
        if (_self == null || !Input.GetKeyDown(castKey)) return;
        var target = CombatTargetSelector.Instance?.CurrentTarget;
        if (charges <= 0) { Debug.Log("[Maleza] Sin cargas. Recolecta más melaza."); return; }
        if (CanCast(_self, target ?? _self.GetComponent<ITarget>()))
        {
            charges--;
            Cast(_self, target ?? _self.GetComponent<ITarget>());
        }
    }

    // ── Cargas ───────────────────────────────────────────────────────────────

    /// <summary>Añade una carga (llamado por <see cref="HoneydewPickup"/>).</summary>
    public bool AddCharge()
    {
        if (charges >= maxCharges) return false;
        charges++;
        Debug.Log($"[Maleza] Carga recogida. Cargas: {charges}/{maxCharges}.");
        return true;
    }

    // ── ISpell ───────────────────────────────────────────────────────────────

    public override bool CanCast(Anima caster, ITarget target)
    {
        // Se puede lanzar sobre uno mismo (target == caster) o en rango.
        if (target == null) return false;
        if (target == caster.GetComponent<ITarget>()) return true; // auto-uso
        return InRange(caster, target);
    }

    public override void Cast(Anima caster, ITarget target)
    {
        // Fallback: si no hay target, aplica al caster.
        ITarget actual = target ?? caster.GetComponent<ITarget>();
        if (actual == null) return;
        if (!CanCast(caster, actual)) return;

        var mb = actual as MonoBehaviour;
        if (mb == null) return;

        // Restaurar energía via CharacterLevel.
        var cl = mb.GetComponent<CharacterLevel>();
        if (cl != null)
        {
            float before = cl.currentEnergy;
            cl.currentEnergy = Mathf.Min(cl.MaxEnergy, cl.currentEnergy + energyRestore);
            float restored = cl.currentEnergy - before;
            Debug.Log($"[Maleza] «{mb.name}» +{restored:0.0} energía " +
                      $"({cl.currentEnergy:0.0}/{cl.MaxEnergy:0.0}).");
        }
        else
        {
            // Sin CharacterLevel: suma a fatReserves del Anima (placeholder).
            var anima = mb.GetComponent<Anima>();
            if (anima != null)
            {
                anima.fatReserves = Mathf.Min(1f, anima.fatReserves + energyRestore * 0.01f);
                Debug.Log($"[Maleza] «{mb.name}» fatReserves +{energyRestore * 0.01f:0.00}.");
            }
        }

        // Si el WeaknessEffect estaba pausado (por Jalar), reanudarlo para que el Update
        // vuelva a controlar el agente con la energía ya restaurada.
        if (resumeWeakness)
        {
            var weak = mb.GetComponent<WeaknessEffect>();
            weak?.Resume();
        }
    }
}
