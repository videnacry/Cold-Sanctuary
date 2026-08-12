using UnityEngine;

/// <summary>
/// Hechizo de LA MALEZA DE AMBROSIO — restaura energía al target.
///
/// Es el PRIMER CONSUMIBLE del juego (Nivel 1, Microcosmos). Kushal recolecta las gotas
/// de melaza que produce Ambrosio; cada uso restaura <see cref="energyRestore"/> de energía
/// al target seleccionado (<see cref="ITarget"/>), lo que temporalmente reactiva su
/// <see cref="NavMeshAgent"/> (controlado por <see cref="WeaknessEffect"/>).
///
/// El debilitamiento sigue drenando la energía → el jugador debe usarla en el momento justo
/// (cuando el target está cerca del checkpoint o cuando nadie más puede jalarlo).
///
/// Uso desde el inventario: el ítem llama <see cref="Cast"/> con el target del
/// <see cref="CombatTargetSelector"/>. Si no hay target seleccionado, se aplica al propio
/// caster (lanzar la maleza en uno mismo para recuperar energía).
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
