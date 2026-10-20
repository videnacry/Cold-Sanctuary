using System.Collections.Generic;
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
    /// <summary>¿Este paso ya está CUMPLIDO antes de lanzarse? (docs/spells-suprahechizo §3: el "check" de un
    /// suprahechizo — p. ej. "estar de pie" cuando ya lo estás → se consume sin animación). Por defecto no.</summary>
    public virtual bool IsSatisfied(Anima self) => false;
}

/// <summary>SUPRAHECHIZO (conjuro) — un hechizo que desencadena una COLA de sub-hechizos (docs/spells-suprahechizo §3).
/// Recorre sus pasos saltando los que ya están CUMPLIDOS (check, sin animación); cada sub-hechizo hace su propio
/// análisis en su `Tick`/`IsSatisfied`. Tiene una POTENCIA impuesta: si recibe un golpe de más potencia (`Destabilize`)
/// PIERDE EL EQUILIBRIO y se REINICIA (lo derribaron → vuelve a empezar por levantarse). Es la base de "la IA lanza
/// una serie de hechizos-recuerdo para resolver una necesidad" (p. ej. comer → analizar → levantarse → buscar → …).</summary>
public class SuperSpell : QueuedSpell
{
    public string name = "conjuro";
    public List<QueuedSpell> steps = new List<QueuedSpell>();
    public float power = 1f;
    int _i;

    public override string Label => $"conjuro {name}";

    public override bool Tick(Anima self)
    {
        while (_i < steps.Count && (steps[_i] == null || steps[_i].IsSatisfied(self))) _i++;   // salta checks ya cumplidos
        if (_i >= steps.Count) return true;
        if (steps[_i].Tick(self)) _i++;
        return _i >= steps.Count;
    }

    /// <summary>Un hechizo entrante de MÁS potencia desequilibra → reinicia el conjuro (lo derribaron físicamente).</summary>
    public void Destabilize(float incomingPower) { if (incomingPower > power) _i = 0; }
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

    [Tooltip("Segundos sin avanzar antes de rendirse (para no bloquear la cola del jugador si no hay camino).")]
    public float giveUpAfter = 4f;
    float _lastProgress = -1f;
    Vector3 _progressPos;

    // RUTA por el NavMesh horneado de la escena: antes iba en línea recta y se clavaba contra la primera pared.
    UnityEngine.AI.NavMeshPath _path;
    int _corner;
    float _repathAt;
    Vector3 _pathGoal, _nextCorner;

    static Vector3 Flat(Vector3 v) { v.y = 0f; return v; }

    // Dirección hacia la siguiente esquina de la ruta (o directa si no hay NavMesh/ruta).
    Vector3 Steer(Vector3 from, Vector3 goal)
    {
        if (_path == null) _path = new UnityEngine.AI.NavMeshPath();
        // Nadando entre islas (fuera del NavMesh) no recalcula: la ruta nueva saldría de la orilla de atrás (ida y vuelta).
        bool onMesh = UnityEngine.AI.NavMesh.SamplePosition(from, out _, 1.5f, UnityEngine.AI.NavMesh.AllAreas);
        if (_path.corners.Length == 0 || (onMesh && (Time.time >= _repathAt || Flat(goal - _pathGoal).sqrMagnitude > 4f)))
        {
            _repathAt = Time.time + 2f; _pathGoal = goal; _corner = 1;
            const int all = UnityEngine.AI.NavMesh.AllAreas;
            bool ok = UnityEngine.AI.NavMesh.SamplePosition(from, out var a, 2f, all)
                   && UnityEngine.AI.NavMesh.SamplePosition(goal, out var b, 3f, all)
                   && UnityEngine.AI.NavMesh.CalculatePath(a.position, b.position, all, _path)
                   && _path.status != UnityEngine.AI.NavMeshPathStatus.PathInvalid;
            if (!ok) _path.ClearCorners();
            else   // no dar media vuelta a una esquina ya pasada (al recalcular tras cruzar agua)
                while (_corner < _path.corners.Length - 1 && Flat(_path.corners[_corner + 1] - from).magnitude < Flat(_path.corners[_corner + 1] - _path.corners[_corner]).magnitude) _corner++;
        }
        Vector3[] c = _path.corners;
        while (_corner < c.Length && Flat(c[_corner] - from).magnitude < 0.4f) _corner++;
        if (c.Length >= 2 && _corner < c.Length) _nextCorner = c[_corner]; else _nextCorner = goal;
        return c.Length >= 2 && _corner < c.Length ? Flat(c[_corner] - from) : Flat(goal - from);
    }

    public override bool Tick(Anima self)
    {
        if (self == null || target == null) return true;
        Vector3 to = target.position - self.transform.position; to.y = 0f;
        // Hasta la SUPERFICIE si el destino es sólido (nevera, plancha…): su centro queda dentro del collider y
        // `stopDistance` nunca se alcanzaba (se quedaba empujando contra la caja).
        Collider col = target.GetComponentInChildren<Collider>();
        float dist = col != null && !col.isTrigger
            ? Vector3.Distance(Flat(self.transform.position), Flat(col.ClosestPoint(self.transform.position)))
            : to.magnitude;
        if (dist <= Mathf.Max(0.1f, stopDistance) + (col != null && !col.isTrigger ? 0.5f : 0f)) return true;   // llegó

        // Atascado: si en `giveUpAfter` s no se ha movido (la ruta puede alejarse en línea recta), se rinde para no
        // bloquear la cola del jugador para siempre.
        if (_lastProgress < 0f || Flat(self.transform.position - _progressPos).magnitude > 0.3f)
        { _progressPos = self.transform.position; _lastProgress = Time.time; }
        else if (Time.time - _lastProgress > giveUpAfter)
        {
            Debug.Log($"[Hechizo] «{self.name}» no puede llegar a {targetName} (atascado a {dist:0.#} m) — se cancela.");
            return true;
        }

        WalkSpell walk = self.GetComponent<WalkSpell>();
        // maxSpeed = subir a la punta (channeling). Antes pasaba `charging`, que es la postura de SALIDA (quieto):
        // walkTo({maxSpeed:true}) dejaba al jugador clavado y la cola bloqueada.
        if (walk != null)
        {
            Vector3 dir = Steer(self.transform.position, target.position).normalized;
            // La ruta sube (o baja) por una escalera vertical: al llegar a su pie, trepa (no cuenta como atasco).
            if (Ladder.AutoClimb(self.transform, _nextCorner)) { _lastProgress = Time.time; return false; }
            // Clavado contra una raíz, una esquina o un mueble que la ruta roza: rodearlo de lado (alternando cada 1,2 s).
            float stuck = Time.time - _lastProgress;
            if (stuck > 0.8f && dir.sqrMagnitude > 0.01f)
            {
                float side = ((int)((stuck - 0.8f) / 1.2f)) % 2 == 0 ? 1f : -1f;
                dir = (dir * 0.35f + Vector3.Cross(Vector3.up, dir) * side).normalized;
            }
            walk.Drive(dir, channeling: maxSpeed);
        }
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
