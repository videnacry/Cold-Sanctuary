using UnityEngine;

/// <summary>
/// CORRIENTE DE RÍO (docs/microcosmos-sakshi-origin.md §4) — una ZONA (trigger) que **arrastra aguas abajo** a las
/// ánimas que caen dentro, con **combate de stats** (la masa resiste: una hormiga grande aguanta, una cría pequeña
/// como Sakshi no) y **daño por arrastre** (el zarandeo **debilita**: drena ATP + estresa) → quien sale del río llega
/// SIN FUERZAS y solo puede **observar** (no huir) — la ventana de bond del nivel.
///
/// Es el equivalente AMBIENTAL de <see cref="PullSpell"/> (mismo principio: inyecta un <see cref="MovementImpulse"/> +
/// resistencia por masa + coste de ATP), pero **sin lanzador**: es el cauce quien empuja, en una **dirección fija**
/// (aguas abajo) y en un **área**. Reutiliza el `ImpulseController` del ánima si lo tiene; si no, la desplaza directo
/// (ruta sin NavMesh, p.ej. objetos o ánimas simples). Sin `Collider`, el barrido automático no corre (graceful).
/// </summary>
[RequireComponent(typeof(Collider))]
public class RiverCurrent : MonoBehaviour
{
    [Header("Corriente")]
    [Tooltip("Dirección de la corriente (aguas abajo), en mundo. Se normaliza.")]
    public Vector3 flowDirection = Vector3.forward;
    [Tooltip("Fuerza de la corriente. Se arrastra a quien tenga (masa × massResist) por debajo de esto.")]
    [Min(0f)] public float pushPower = 3f;
    [Tooltip("Cuánta masa del ánima RESISTE la corriente (grande aguanta; cría no).")]
    [Min(0f)] public float massResist = 1f;
    [Tooltip("Velocidad de deriva (m/s por unidad de empuje neto) en la ruta directa (ánimas sin ImpulseController).")]
    [Min(0f)] public float driftSpeed = 1.5f;

    [Header("Daño por arrastre (debilita → llega sin fuerzas, solo observa)")]
    [Tooltip("ATP/s drenado mientras la corriente arrastra al ánima.")]
    [Min(0f)] public float dragEnergyPerSecond = 2f;
    [Tooltip("Estrés/s que añade el zarandeo.")]
    [Min(0f)] public float dragStressPerSecond = 0.3f;

    Collider _zone;
    const string TAG = "river";

    void Awake()
    {
        _zone = GetComponent<Collider>();
        if (_zone != null) _zone.isTrigger = true;
    }

    void Update()
    {
        if (_zone == null) return;
        Bounds b = _zone.bounds;
        float dt = Time.deltaTime;
        foreach (Collider c in Physics.OverlapBox(b.center, b.extents, Quaternion.identity))
        {
            Anima a = c.GetComponentInParent<Anima>();
            if (a != null) ApplyTo(a, dt);
        }
    }

    /// <summary>Empuje NETO sobre un ánima (fuerza de la corriente menos lo que resiste su masa). ≤0 = aguanta.</summary>
    public float NetPush(Anima a)
    {
        if (a == null) return 0f;
        return pushPower - a.BodyMass * massResist;
    }

    /// <summary>Aplica la corriente a un ánima durante dt: la arrastra aguas abajo (si su masa no aguanta) y la
    /// DEBILITA (drena ATP + estrés). Público y determinista (lo llama Update y el test).</summary>
    public void ApplyTo(Anima a, float dt)
    {
        if (a == null || a.death) return;
        float net = NetPush(a);
        if (net <= 0f) return;                                   // grande: la corriente no la mueve

        Vector3 flow = flowDirection.sqrMagnitude > 0.0001f ? flowDirection.normalized : Vector3.forward;

        ImpulseController ctrl = a.GetComponent<ImpulseController>();
        if (ctrl != null)
        {
            ctrl.RemoveByTag(TAG);
            ctrl.AddImpulse(new MovementImpulse(TAG, flow, net, 3f));   // decae al salir del cauce
        }
        else
        {
            a.transform.position += flow * (net * driftSpeed * dt);     // ruta directa (sin NavMesh)
        }

        // Daño por arrastre → llega debilitada (sin ATP no puede pagar el movimiento → no huye, solo observa).
        CharacterLevel cl = a.GetComponent<CharacterLevel>();
        if (cl != null && dragEnergyPerSecond > 0f)
            if (!cl.SpendEnergy(dragEnergyPerSecond * dt)) cl.currentEnergy = 0f;
        a.stress = Mathf.Min(1f, a.stress + dragStressPerSecond * dt);
    }

    void OnDrawGizmosSelected()
    {
        Collider z = GetComponent<Collider>();
        if (z == null) return;
        Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.25f);
        Gizmos.DrawCube(z.bounds.center, z.bounds.size);
        Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.9f);
        Vector3 flow = flowDirection.sqrMagnitude > 0.0001f ? flowDirection.normalized : Vector3.forward;
        Gizmos.DrawRay(z.bounds.center, flow * 3f);
    }
}
