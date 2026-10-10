using UnityEngine;

/// <summary>
/// COMPORTAMIENTO LIBRE vía COLA (docs/typed-spells-and-queues.md §4): cuando el anima está **libre** (necesidades
/// cubiertas, no poseída, cola vacía), se **encola a sí misma** acciones random (merodear) en el carril del jugador —
/// el carril "opcional" que el jugador, al poseerla, puede vaciar/reprogramar. Así el NPC **usa la cola** en vez de
/// actuar directo: "escribe" virtualmente sus hechizos. Pensado para animas por composición (que no tienen Volición
/// conduciéndolas); para la fauna con `Volition`, enrutar TODA la IA por la cola es el refactor mayor pendiente.
/// </summary>
[RequireComponent(typeof(ActionQueue))]
public class IdleBehavior : MonoBehaviour
{
    [Min(0.5f)] public float checkInterval = 2f;
    float _next;
    Anima _a;
    ActionQueue _q;
    AnimaController _ctrl;

    void Awake()
    {
        _a = GetComponent<Anima>();
        _q = GetComponent<ActionQueue>();
        _ctrl = GetComponent<AnimaController>();
    }

    void Update()
    {
        if (_a == null || _a.death || _a.asleep) return;
        if (Time.time < _next) return;
        _next = Time.time + checkInterval;

        if (_ctrl != null && _ctrl.Active is PlayerBrain) return;         // poseída: manda el jugador
        if (_q.Own.Count > 0 || _q.Player.Count > 0) return;              // ya hay algo en cola
        if (_a.stress > 0.6f || _a.sleepiness > 0.7f) return;             // necesidades no cubiertas: que las resuelva su IA/cuerpo

        _q.EnqueuePlayer(new WanderSpell());                              // libre → merodea (acción opcional, borrable al poseer)
    }
}
