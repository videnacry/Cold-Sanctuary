using UnityEngine;

/// <summary>
/// Un HECHIZO EN COLA (docs/typed-spells-and-queues.md): la unidad que ejecuta la <see cref="ActionQueue"/>. Como
/// "toda acción es un hechizo", caminar/tomar/agacharse son QueuedSpell. `Tick` avanza el hechizo y devuelve true
/// cuando TERMINA (la cola pasa al siguiente). Base pura (no MonoBehaviour) para poder encolarse/reordenarse.
/// </summary>
public abstract class QueuedSpell
{
    public abstract string Label { get; }
    /// <summary>Ejecuta un paso; devuelve true cuando el hechizo ha terminado. `self` = quien lo lanza.</summary>
    public abstract bool Tick(Anima self);
}

/// <summary>Ir hacia un DESTINO (otra anima, o un área/gameobject por nombre), opcionalmente manteniendo distancia.
/// Reutiliza la locomoción-hechizo (`WalkSpell`) o el NavMeshAgent. Hechizo natural del que nacen correr/teletransporte
/// (spell-genesis §1): aquí es caminar/correr según `maxSpeed`.</summary>
public class WalkToSpell : QueuedSpell
{
    public Transform target;
    public string targetName = "destino";
    public float stopDistance = 0.6f;
    public bool maxSpeed = false;

    public override string Label => $"ir a {targetName}" + (maxSpeed ? " (rápido)" : "");

    public override bool Tick(Anima self)
    {
        if (self == null || target == null) return true;
        Vector3 to = target.position - self.transform.position; to.y = 0f;
        if (to.magnitude <= Mathf.Max(0.1f, stopDistance)) return true;   // llegó

        WalkSpell walk = self.GetComponent<WalkSpell>();
        if (walk != null) walk.Drive(to.normalized, charging: maxSpeed);   // correr = caminar con intensidad (charging)
        else
        {
            var nav = self.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (nav != null && nav.isOnNavMesh) nav.SetDestination(target.position);
        }
        return false;
    }
}
