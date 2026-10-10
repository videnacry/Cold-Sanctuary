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

/// <summary>MERODEAR: ir a un punto aleatorio cerca (acción "random" que un anima libre se encola a sí misma en el
/// carril opcional, docs/typed-spells-and-queues.md §4). Camina como `WalkToSpell` hacia un destino efímero.</summary>
public class WanderSpell : QueuedSpell
{
    Vector3 _dest; bool _picked; float _until;
    public float radius = 6f, timeout = 8f;
    public override string Label => "merodear";
    public override bool Tick(Anima self)
    {
        if (self == null) return true;
        if (!_picked)
        {
            Vector2 r = Random.insideUnitCircle * radius;
            _dest = self.transform.position + new Vector3(r.x, 0f, r.y);
            _until = Time.time + timeout; _picked = true;
        }
        if (Time.time >= _until) return true;
        Vector3 to = _dest - self.transform.position; to.y = 0f;
        if (to.magnitude <= 0.6f) return true;
        WalkSpell walk = self.GetComponent<WalkSpell>();
        if (walk != null) walk.Drive(to.normalized);
        else { var nav = self.GetComponent<UnityEngine.AI.NavMeshAgent>(); if (nav != null && nav.isOnNavMesh) nav.SetDestination(_dest); }
        return false;
    }
}

/// <summary>REFLEJO DEL CUERPO: ocupa el carril PRIORITARIO (`own`) mientras el cuerpo está en "toma de mando"
/// (<see cref="AcuteStressResponse"/>). No conduce él mismo (lo hace el AiBrain con relevancia alta); su presencia
/// BLOQUEA el carril del jugador → el jugador no puede programar mientras el apremio manda. Termina al bajar el apremio.</summary>
public class BodyReflexSpell : QueuedSpell
{
    readonly AcuteStressResponse _asr;
    public BodyReflexSpell(AcuteStressResponse asr) { _asr = asr; }
    public override string Label => "instinto (el cuerpo manda)";
    public override bool Tick(Anima self) => _asr == null || !_asr.InTakeover;   // se mantiene mientras haya toma de mando
}
