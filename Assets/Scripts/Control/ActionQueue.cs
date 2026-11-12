using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// COLA DE ACCIONES de un anima (docs/typed-spells-and-queues.md) — ejecuta <see cref="QueuedSpell"/>s en orden, estilo
/// Sims. Dos carriles:
///   • `own`    = la cola PROPIA del anima: **prioritaria y NO editable por el jugador** (la alimenta su instinto /
///                <see cref="AcuteStressResponse"/> cuando el cuerpo toma el mando).
///   • `player` = la cola del jugador cuando POSEE al anima: **editable** (encolar/quitar/reordenar/vaciar).
/// La `own` siempre tiene prioridad (si hay algo urgente del cuerpo, el jugador espera) → el jugador **nunca** tiene
/// control total: programa sobre lo que el cuerpo le permite. Un anima libre (sin poseer), si sus necesidades están
/// cubiertas, puede encolar acciones random (merodear/explorar) en `player` (que el jugador, al poseer, puede vaciar).
/// </summary>
public class ActionQueue : MonoBehaviour
{
    readonly List<QueuedSpell> _own = new List<QueuedSpell>();
    readonly List<QueuedSpell> _player = new List<QueuedSpell>();
    Anima _self;

    void Awake() { _self = GetComponent<Anima>(); }

    public IReadOnlyList<QueuedSpell> Own => _own;
    public IReadOnlyList<QueuedSpell> Player => _player;

    public void EnqueueOwn(QueuedSpell s)    { if (s != null) _own.Add(s); }
    public void EnqueuePlayer(QueuedSpell s) { if (s != null) _player.Add(s); }
    public void ClearPlayer()                => _player.Clear();
    public void ClearOwnForTest()            => _own.Clear();   // solo tests
    public void ClearOwn()                   => _own.Clear();   // deja lo que hacía por iniciativa propia (otra tarea manda)
    public int RemoveOwnWhere(System.Predicate<QueuedSpell> match) => _own.RemoveAll(match);   // quita solo lo que ya no toca (p. ej. la secuencia de una misión que acabó)
    public void RemovePlayerAt(int i)        { if (i >= 0 && i < _player.Count) _player.RemoveAt(i); }
    public void MovePlayer(int from, int to)
    {
        if (from < 0 || from >= _player.Count || to < 0 || to >= _player.Count) return;
        QueuedSpell s = _player[from]; _player.RemoveAt(from); _player.Insert(to, s);
    }

    void Update()
    {
        if (_self == null || _self.death) return;
        List<QueuedSpell> lane = _own.Count > 0 ? _own : _player;   // la propia manda
        if (lane.Count == 0) return;
        // En una ORQUESTA (iniciación) o guiado por PathMover, su «merodear» se descarta: lo arrastraba a la
        // vez hacia otro sitio (saltos de 10 m a ×8, vueltas en círculo; Berón 2026-10-29).
        if (lane == _player && lane[0] is WanderSpell ws && (Orchestra.Busy(gameObject) || (PathMover.DrivenRecently(gameObject) && !ws.Drives(gameObject)))) { lane.RemoveAt(0); return; }   // (sus propios pasos por el NavMesh no cuentan)
        if (lane[0].Tick(_self)) lane.RemoveAt(0);                  // terminó → siguiente
    }
}
