using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Componente de LOCOMOCIÓN (docs/anima-dissolving-animal.md, etapa 2). Envuelve el par **NavMesh + gait**
/// (`ActionPrep`: animación + velocidad + cansancio) para que las conductas pidan "muévete a X andando/corriendo"
/// sin tocar `nav`/`ActsPrep` a mano. Es el paso previo a `Forager`/`Predator` (perseguir/pastar es locomoción) y
/// al mover autónomo (`AiBrain`). La velocidad fina la da `WalkSpell` (opt-in, ya integrado).
///
/// De momento el `ActionsPrep` (gaits por especie) vive en `Animal`; `Locomotion` lo lee de ahí. Cuando el
/// `ActsPrep` pase a ser data de arquetipo/componente, `Locomotion` será portable a cualquier ser que navegue.
/// </summary>
public class Locomotion : MonoBehaviour
{
    Animal _animal;
    NavMeshAgent _nav;

    void Awake()
    {
        _animal = GetComponent<Animal>();
        _nav = GetComponent<NavMeshAgent>();
    }

    /// <summary>Ir a `dest` ANDANDO (gait walk de la especie).</summary>
    public void Walk(Vector3 dest, float animTime) => Move(dest, _animal != null ? _animal.ActsPrep?.walk : null, animTime);
    /// <summary>Ir a `dest` CORRIENDO (gait run).</summary>
    public void Run(Vector3 dest, float animTime) => Move(dest, _animal != null ? _animal.ActsPrep?.run : null, animTime);
    /// <summary>Quedarse quieto (gait idle); no cambia el destino.</summary>
    public void Idle(float animTime) { if (_animal != null) _animal.ActsPrep?.idle?.Prep(_animal, animTime); }

    /// <summary>Fija destino (si el agente está sobre NavMesh) y prepara el gait dado.</summary>
    public void Move(Vector3 dest, ActionPrep gait, float animTime)
    {
        if (_nav != null && _nav.isOnNavMesh) _nav.SetDestination(dest);
        if (_animal != null) gait?.Prep(_animal, animTime);
    }
}
