using UnityEngine;

/// <summary>
/// Objetivo universal: cualquier cosa que pueda ser presa o amenaza (animal o jugador).
/// Ver docs/behavior-system.md (Componente C).
/// </summary>
public interface ITarget
{
    Transform transform { get; }
    float Mass { get; }
    float Speed { get; }
    char Faction { get; }
    bool Dead { get; }
    /// <summary> True cuando el cuerpo está completamente consumido y debe ignorarse. </summary>
    bool Consumed { get; }
    void Hurt(float damage);
}
