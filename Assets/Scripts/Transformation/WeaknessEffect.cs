using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Efecto de DEBILITAMIENTO progresivo — drena la energía del ser a ritmo fijo
/// hasta dejarla en 0, momento en que se detiene (el NavMeshAgent se deshabilita).
///
/// Diseñado para las hormigas viejas del Nivel 1 del Microcosmos: el hechizo de vejez
/// reduce su energía de forma continua hasta que alguien (Kushal con la maleza de Ambrosio
/// o con el hechizo Jalar) las ayuda a llegar al checkpoint.
///
/// El drenaje NO para aunque la energía sea rellenada externamente (p. ej. con
/// <see cref="HoneydewSpell"/>): el efecto sigue activo hasta que se destruye este componente
/// o se llama a <see cref="Cancel"/>. Esto fuerza al jugador a actuar en el momento justo.
///
/// Requiere <see cref="CharacterLevel"/> en el mismo GO (para drenar <c>currentEnergy</c>)
/// y opcionalmente un <see cref="NavMeshAgent"/> (para detener el movimiento cuando la
/// energía llega a 0).
/// </summary>
[RequireComponent(typeof(CharacterLevel))]
public class WeaknessEffect : MonoBehaviour
{
    [Header("Drenaje de energía")]
    [Tooltip("Energía drenada por segundo (unidades de currentEnergy).")]
    [Min(0.01f)] public float drainPerSecond = 5f;

    [Tooltip("Si true, el NavMeshAgent se deshabilita al llegar a 0 energía " +
             "y se re-habilita cuando la energía sube de nuevo.")]
    public bool controlAgent = true;

    CharacterLevel _level;
    NavMeshAgent   _agent;
    bool           _active = true;

    void Awake()
    {
        _level = GetComponent<CharacterLevel>();
        _agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (!_active || _level == null) return;

        // Drenar energía (SpendEnergy devuelve false si no hay suficiente → la deja en 0).
        float drain = drainPerSecond * Time.deltaTime;
        if (!_level.SpendEnergy(drain))
            _level.currentEnergy = 0f;

        // Activar/desactivar NavMeshAgent según si hay energía.
        if (controlAgent && _agent != null)
        {
            bool hasEnergy = _level.currentEnergy > 0f;
            if (_agent.enabled != hasEnergy) _agent.enabled = hasEnergy;
        }
    }

    /// <summary>Cancela el efecto de debilitamiento (sin destruir el componente).</summary>
    public void Cancel()
    {
        _active = false;
        if (controlAgent && _agent != null && !_agent.enabled)
            _agent.enabled = true;
        Debug.Log($"[Debilitamiento] Efecto cancelado en «{name}».");
    }

    /// <summary>Reactiva el debilitamiento si fue cancelado.</summary>
    public void Resume() => _active = true;

    /// <summary>Fuerza la energía a 0 de inmediato (inicio del efecto).</summary>
    public void ApplyImmediate()
    {
        if (_level == null) return;
        _level.currentEnergy = 0f;
        if (controlAgent && _agent != null) _agent.enabled = false;
    }
}
