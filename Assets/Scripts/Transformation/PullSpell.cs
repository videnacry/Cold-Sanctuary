using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Hechizo "JALAR" — arrastra al target hacia el caster.
///
/// Parámetros heredados de <see cref="SpellBase"/>:
///   • <c>range</c>    — distancia máxima desde la que se puede lanzar (0 = tacto).
///   • <c>force</c>    — fuerza base de tiro (se suma a la aptitud fuerza del caster).
///   • <c>duration</c> — cuántos segundos dura el efecto de arrastre.
///
/// Física del hechizo:
///   <c>fuerzaEfectiva = (force + caster.Strength) - (target.Mass + resistencia)</c>
///   Si el target tiene <see cref="NavMeshAgent"/> y se mueve en dirección contraria, su velocidad
///   se cuenta como resistencia adicional. Fuerza efectiva ≤ 0 → el hechizo no lo mueve.
///
/// Implementación: desactiva el NavMeshAgent del target durante <c>duration</c> segundos y aplica
/// una fuerza física hacia el caster (o lo mueve vía Transform si no tiene Rigidbody).
/// Al terminar, reactiva el agente.
///
/// Nivel 1 (Microcosmos): Kushal tiene fuerza baja → puede arrastrar una hormiga vieja que no
/// opone resistencia; NO puede arrastrar una que huya activamente.
/// </summary>
public class PullSpell : SpellBase
{
    [Header("Pull — parámetros extra")]
    [Tooltip("Si el target se mueve en dirección contraria, esta fracción de su speed se suma al peso " +
             "como resistencia (0 = ignorar dirección del target; 1 = sumar su velocidad completa).")]
    [Range(0f, 2f)] public float resistanceFromSpeed = 1f;

    [Tooltip("Si true y el target tiene WeaknessEffect, el arrastre reactiva temporalmente " +
             "su NavMeshAgent al terminar (WeaknessEffect lo controlará de nuevo).")]
    public bool releaseAfterPull = true;

    // ── ISpell ───────────────────────────────────────────────────────────────

    public override bool CanCast(Anima caster, ITarget target)
    {
        if (target == null || target.Dead || target.Consumed) return false;
        if (!InRange(caster, target))
        {
            Debug.Log($"[Jalar] Fuera de rango ({range:0.0} m).");
            return false;
        }
        return true;
    }

    public override void Cast(Anima caster, ITarget target)
    {
        if (!CanCast(caster, target)) return;

        var mb = target as MonoBehaviour;
        if (mb == null) return;

        // Calcular fuerza efectiva.
        float resistance = 0f;
        if (resistanceFromSpeed > 0f)
        {
            // Resistencia = fracción de la velocidad del target si se aleja del caster.
            Vector3 toTarget = (mb.transform.position - caster.transform.position).normalized;
            resistance = Mathf.Max(0f, target.Speed * resistanceFromSpeed *
                         Vector3.Dot(target.transform.forward, toTarget));
        }

        float effectiveForce = EffectiveForce(caster, target, resistance);
        if (effectiveForce <= 0f)
        {
            Debug.Log($"[Jalar] Sin fuerza suficiente para mover a «{mb.name}» " +
                      $"(masa {target.Mass:0.0} + resist {resistance:0.0} vs fuerza {force + caster.Strength:0.0}).");
            return;
        }

        StartCoroutine(DoPull(caster.transform, mb.gameObject, effectiveForce));
    }

    // ── Pull coroutine ───────────────────────────────────────────────────────

    IEnumerator DoPull(Transform casterT, GameObject targetGO, float effectiveForce)
    {
        var agent  = targetGO.GetComponent<NavMeshAgent>();
        var rb     = targetGO.GetComponent<Rigidbody>();
        var weak   = targetGO.GetComponent<WeaknessEffect>();

        // Pausar el agente y el debilitamiento para que no pelee con la física.
        if (agent != null && agent.enabled) agent.enabled = false;
        if (weak  != null) weak.Cancel();

        float elapsed    = 0f;
        float pullTime   = duration > 0f ? duration : 0.4f;

        Debug.Log($"[Jalar] Arrastrando «{targetGO.name}» ({effectiveForce:0.0} N, {pullTime:0.1} s).");

        while (elapsed < pullTime)
        {
            if (targetGO == null) yield break;

            Vector3 dir = (casterT.position - targetGO.transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) break; // ya llegó

            dir.Normalize();

            if (rb != null)
                rb.AddForce(dir * effectiveForce, ForceMode.Force);
            else
                targetGO.transform.position += dir * effectiveForce * Time.deltaTime;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reactivar: si había WeaknessEffect lo reanuda; si no, reactiva el agente.
        if (weak != null)
            weak.Resume();   // WeaknessEffect controlará el agente de nuevo
        else if (releaseAfterPull && agent != null)
            agent.enabled = true;

        Debug.Log($"[Jalar] Arrastre de «{targetGO.name}» completado.");
    }
}
