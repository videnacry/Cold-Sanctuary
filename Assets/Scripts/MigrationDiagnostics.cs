using UnityEngine;

/// <summary>
/// Diagnóstico de migración (TEMPORAL). Al entrar en Play vuelca por consola los valores relevantes de
/// todos los seres (`Anima`) y del jugador, para **validar la migración sin depurador**: qué hereda de
/// `Anima`, sus aptitudes (por campo y vía `IAptitudes`), y los puntos del alma/margas del jugador.
/// Quitar o desactivar el GameObject en release.
/// </summary>
public class MigrationDiagnostics : MonoBehaviour
{
    void Start()
    {
        Debug.Log("═══════ [Diag Anima] inicio ═══════");

        // Chequeo de jerarquía (compila-time real: si compiló, la migración está bien tipada).
        Debug.Log($"[Diag] CompanionBase hereda de Anima: {typeof(Anima).IsAssignableFrom(typeof(CompanionBase))}");
        Debug.Log($"[Diag] Animal hereda de Anima: {typeof(Anima).IsAssignableFrom(typeof(Animal))}");

        // Todos los Anima de la escena (Animal + CompanionBase + lo que venga).
        Anima[] seres = FindObjectsOfType<Anima>();
        Debug.Log($"[Diag] Anima en escena: {seres.Length}");
        foreach (Anima s in seres)
        {
            IAptitudes apt = s;   // Anima implementa IAptitudes
            Debug.Log($"[Diag]  · {s.name} · tipo={s.GetType().Name} · companion={s is CompanionBase} · " +
                      $"apt(str={s.strength:0.00} agi={s.agility:0.00} rea={s.reasoning:0.00}) · " +
                      $"IAptitudes(str={apt.Strength:0.00}) · " +
                      $"drives(sat={s.satisfaction:0.00} phys={s.physicalResistance:0.00} obs={s.observationRadius:0.0} vel={s.velocity:0.0})");
        }

        // Mentes (pilar Mind) presentes.
        Mind[] minds = FindObjectsOfType<Mind>();
        Debug.Log($"[Diag] Mind (pilar mente) en escena: {minds.Length}");

        // Jugador: puntos del alma + margas.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.TryGetComponent(out CharacterLevel cl))
            Debug.Log($"[Diag] Kushal · margas Stats L{cl.stats.level}/Yoga L{cl.yoga.level}/Vínc L{cl.bonds.level} · " +
                      $"Vida {cl.MaxHealth:0} Energía {cl.MaxEnergy:0} Maná {cl.MaxMana:0} Def {cl.PassiveDefense:0} · " +
                      $"manáDesbloqueado={cl.ManaUnlocked}");
        else
            Debug.Log("[Diag] Jugador sin CharacterLevel (o sin tag Player).");

        Debug.Log("═══════ [Diag Anima] fin ═══════");
    }
}
