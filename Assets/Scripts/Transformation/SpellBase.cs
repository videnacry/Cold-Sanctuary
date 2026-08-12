using UnityEngine;

/// <summary>
/// Base abstracta para todos los hechizos del juego (docs/stats-as-truth.md §hechizos).
///
/// Un hechizo tiene tres parámetros universales:
///   • <see cref="range"/>    — alcance máximo (0 = tacto; el caster debe estar junto al target).
///   • <see cref="force"/>    — magnitud del efecto (tiro, carga, potencia…).
///   • <see cref="duration"/> — duración del efecto (0 = instantáneo).
///
/// Subclases implementan <see cref="CanCast"/> y <see cref="Cast"/>.
/// El target es un <see cref="ITarget"/> — cualquier Anima, FoodItem, FishSchool u objeto del mundo
/// que implemente la interfaz. Esto permite lanzar sobre uno mismo, sobre otro ser vivo o
/// sobre elementos del entorno (la transformación al mundo entero, p. ej.).
///
/// Para hechizos de MagicReserves (coste elemental/energía) usar <see cref="MagicReserves.Pay"/>
/// en la implementación de <see cref="Cast"/> antes de ejecutar el efecto.
/// </summary>
public abstract class SpellBase : MonoBehaviour
{
    [Header("Parámetros universales del hechizo")]
    [Tooltip("Alcance máximo (m). 0 = tacto (el caster debe estar pegado al target).")]
    [Min(0f)] public float range = 0f;

    [Tooltip("Magnitud del efecto: fuerza de tiro, potencia de curación, intensidad de transformación…")]
    [Min(0f)] public float force = 1f;

    [Tooltip("Duración del efecto en segundos. 0 = instantáneo.")]
    [Min(0f)] public float duration = 0f;

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>
    /// ¿Puede el <paramref name="caster"/> lanzar este hechizo sobre <paramref name="target"/>?
    /// Las subclases deben comprobar al menos que el target no está muerto y que está en rango.
    /// </summary>
    public abstract bool CanCast(Anima caster, ITarget target);

    /// <summary>
    /// Lanza el hechizo. Solo llamar si <see cref="CanCast"/> devuelve true.
    /// </summary>
    public abstract void Cast(Anima caster, ITarget target);

    // ── Utilidades para subclases ─────────────────────────────────────────────

    /// <summary>¿Está <paramref name="target"/> dentro del alcance del hechizo?</summary>
    protected bool InRange(Anima caster, ITarget target)
    {
        if (range <= 0f) return true; // tacto: siempre en rango (validar distancia física en CanCast)
        return Vector3.Distance(caster.transform.position, target.transform.position) <= range;
    }

    /// <summary>
    /// Fuerza efectiva del hechizo contra <paramref name="target"/>: descuenta la masa del objetivo
    /// y la fuerza que ejerce si se resiste (si <paramref name="targetResistForce"/> > 0).
    /// Retorna 0 si no hay fuerza suficiente para moverlo.
    /// </summary>
    protected float EffectiveForce(Anima caster, ITarget target, float targetResistForce = 0f)
    {
        float casterForce = force + caster.Strength;          // aptitud fuerza del lanzador
        float targetWeight = target.Mass + targetResistForce; // peso + resistencia activa
        return Mathf.Max(0f, casterForce - targetWeight);
    }
}
