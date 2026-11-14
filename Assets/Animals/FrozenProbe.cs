using System.Text;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// (2026-11-14, rondas 94–95) SONDA DE UN ANIMAL «QUIETO CON CAMINO». ~2000 congelados en 2 días (con el árbol quieto también), sin nadie cerca
/// y lejos de los bordes: o el detector se equivoca, o algo mueve el cuerpo o el agente fuera del NavMeshAgent. Durante <see cref="Frames"/>
/// fotogramas apunta, al principio de cada fotograma (antes que cualquier script) y al final (después del agente y de los LateUpdate): el
/// cuerpo, la posición interna del agente, velocidades, speed, isStopped, updatePosition, enabled, camino, distancia que queda, estado,
/// evitación, deltaTime, timeScale y maximumDeltaTime, y el último que le cambió el camino (NavTrace). Lo dice en una sola línea
/// «[Fauna] sonda …». Si el cuerpo cambia ENTRE el final de un fotograma y el principio del siguiente, o dentro del fotograma sin que el agente
/// avance, lo mueve otro; si el agente no avanza con «deseada» &gt; 0, es el agente.
/// </summary>
[DefaultExecutionOrder(-10000)]
public class FrozenProbe : MonoBehaviour
{
    public const int Frames = 8;
    public static int Started;
    const int MaxProbes = 12;

    Animal _a; NavMeshAgent _n;
    int _frame;
    Vector3 _endPos, _endNext;
    bool _haveEnd;
    readonly StringBuilder _sb = new StringBuilder();
    string _why;

    /// <summary>Pone una sonda (si quedan) al animal <paramref name="a"/>; <paramref name="why"/>: qué la disparó.</summary>
    public static void Attach(Animal a, string why)
    {
        if (a == null || Started >= MaxProbes || a.GetComponent<FrozenProbe>() != null) return;
        Started++;
        FrozenProbe p = a.gameObject.AddComponent<FrozenProbe>();
        p._a = a; p._n = a.nav; p._why = why;
    }

    void Update()
    {
        if (_n == null || _a == null) { Destroy(this); return; }
        Vector3 pos = transform.position;
        if (_haveEnd)
        {
            float between = (pos - _endPos).magnitude, betweenAgent = (_n.nextPosition - _endNext).magnitude;
            if (between > 1e-4f || betweenAgent > 1e-4f) _sb.Append($" | ENTRE fotogramas: cuerpo {between:0.000} m, agente {betweenAgent:0.000} m");
        }
        _sb.Append($" || f{_frame} ini: cuerpo ({pos.x:0.000},{pos.y:0.000},{pos.z:0.000}) interno ({_n.nextPosition.x:0.000},{_n.nextPosition.z:0.000}) dt {Time.deltaTime:0.000} escala {Time.timeScale:0.0} maxDt {Time.maximumDeltaTime:0.00}");
    }

    void LateUpdate()
    {
        if (_n == null || _a == null) return;
        Vector3 pos = transform.position;
        _sb.Append($" · fin: cuerpo ({pos.x:0.000},{pos.z:0.000}) interno ({_n.nextPosition.x:0.000},{_n.nextPosition.z:0.000}) vel {_n.velocity.magnitude:0.00} deseada {_n.desiredVelocity.magnitude:0.00} speed {_n.speed:0.0} acel {_n.acceleration:0.0} parado {_n.isStopped} updPos {_n.updatePosition} activo {_n.isActiveAndEnabled} enNavMesh {_n.isOnNavMesh} camino {_n.hasPath}/{_n.pathStatus}/pend {_n.pathPending} queda {_n.remainingDistance:0.0} rumbo ({_n.steeringTarget.x:0.0},{_n.steeringTarget.z:0.0}) evit {_n.obstacleAvoidanceType} radio {_n.radius:0.00} alto {_n.height:0.00} baseOffset {_n.baseOffset:0.00} busy {_a.busy}");
        _endPos = pos; _endNext = _n.nextPosition; _haveEnd = true;
        if (++_frame >= Frames)
        {
            Debug.Log($"[Fauna] sonda «{name}» ({_why}) · {NavTrace.Last(_a)}{_sb}");
            Destroy(this);
        }
    }
}
