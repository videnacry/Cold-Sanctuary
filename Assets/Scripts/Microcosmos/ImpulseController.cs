using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// CONTROLADOR DE IMPULSOS de dirección — suma todos los <see cref="MovementImpulse"/>
/// activos y dirige el <see cref="NavMeshAgent"/> hacia el vector resultante.
///
/// Diseño:
/// • Varios sistemas (HomeImpulse, ThreatScanner, hechizos) añaden/quitan impulsos con un tag.
/// • Cada tick el controlador suma los impulsos ponderados y mueve al agente.
/// • Si la suma es ~0 (impulsos contrarios o ausentes) el agente se queda quieto.
/// • El decaimiento automático debilita los impulsos transitorios (ej. miedo al escapar
///   de un depredador que ya está lejos) hasta que caen a 0 y se eliminan.
/// • Hechizos de hipnosis / coraje llaman a <see cref="ClearAll"/> o
///   <see cref="RemoveByTag"/> para modificar el comportamiento desde fuera.
///
/// El componente sólo mueve al ser si tiene energía (<see cref="CharacterLevel.SpendEnergy"/>
/// proporcional a la velocidad — la caminata consume ATP).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class ImpulseController : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Distancia proyectada hacia adelante para setear el destino del agente.")]
    [Min(1f)] public float projectionDistance = 4f;

    [Tooltip("Frecuencia de re-evaluación del destino (segundos entre ticks).")]
    [Min(0.05f)] public float tickRate = 0.2f;

    [Header("Coste de caminata (ATP)")]
    [Tooltip("Energía gastada por segundo al moverse (0 = sin coste).")]
    [Min(0f)] public float walkEnergyCostPerSecond = 1f;

    // ── Estado ─────────────────────────────────────────────────────────────

    readonly List<MovementImpulse> _impulses = new List<MovementImpulse>();

    NavMeshAgent   _agent;
    CharacterLevel _level;
    float          _next;

    // ── API pública ────────────────────────────────────────────────────────

    /// <summary>Añade un impulso (con decaimiento opcional).</summary>
    public void AddImpulse(MovementImpulse impulse) => _impulses.Add(impulse);

    /// <summary>Elimina todos los impulsos con el tag indicado.</summary>
    public void RemoveByTag(string tag) => _impulses.RemoveAll(i => i.tag == tag);

    /// <summary>Elimina TODOS los impulsos (hipnosis, aturdimiento, etc.).</summary>
    public void ClearAll() => _impulses.Clear();

    /// <summary>Vector neto actual (suma ponderada). Útil para depuración o hechizos que leen el estado.</summary>
    public Vector3 NetDirection
    {
        get
        {
            var net = Vector3.zero;
            foreach (var imp in _impulses) net += imp.Weighted;
            return net;
        }
    }

    // ── Ciclo de vida ──────────────────────────────────────────────────────

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _level = GetComponent<CharacterLevel>();
    }

    void Update()
    {
        // Decaer impulsos transitorios.
        for (int i = _impulses.Count - 1; i >= 0; i--)
        {
            var imp = _impulses[i];
            if (imp.decayRate <= 0f) continue;
            imp.magnitude -= imp.decayRate * Time.deltaTime;
            if (imp.magnitude <= 0f) { _impulses.RemoveAt(i); continue; }
            _impulses[i] = imp;
        }

        if (Time.time < _next) return;
        _next = Time.time + tickRate;

        if (!_agent.isOnNavMesh) return;

        var netDir = NetDirection;

        if (netDir.sqrMagnitude < 0.001f)
        {
            _agent.ResetPath();
            return;
        }

        // Coste de ATP: si no hay energía no puede moverse.
        if (walkEnergyCostPerSecond > 0f && _level != null)
        {
            float cost = walkEnergyCostPerSecond * tickRate;
            if (_level.currentEnergy < cost)
            {
                _agent.ResetPath(); // sin energía: parado
                return;
            }
            _level.SpendEnergy(cost);
        }

        // Mover hacia la dirección neta.
        Vector3 dest = transform.position + netDir.normalized * projectionDistance;
        _agent.SetDestination(dest);
    }

    void OnDrawGizmosSelected()
    {
        if (_impulses.Count == 0) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, NetDirection.normalized * 2f);
        foreach (var imp in _impulses)
        {
            Gizmos.color = new Color(0.2f, 0.8f, 0.4f, 0.4f);
            Gizmos.DrawRay(transform.position, imp.Weighted);
        }
    }
}
