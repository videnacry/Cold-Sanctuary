using UnityEngine;

/// <summary>Andar en el microcosmos: con NavMeshAgent si lo hay (rodea los muebles gigantes); si no, en línea recta.</summary>
public static class MicroMove
{
    /// <summary>Da un paso hacia <paramref name="to"/>; true = ya ha llegado.</summary>
    public static bool Step(Transform t, Vector3 to, float speed, float arrive = 1.2f)
    {
        Vector3 d = to - t.position; d.y = 0f;
        if (d.magnitude <= arrive) return true;
        var ag = t.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (ag != null && ag.enabled && ag.isOnNavMesh)
        {
            ag.speed = speed;
            if ((ag.destination - to).sqrMagnitude > 1f) ag.SetDestination(to);
            return false;
        }
        t.position += d.normalized * Mathf.Min(speed * Time.deltaTime, d.magnitude);
        t.rotation = Quaternion.Slerp(t.rotation, Quaternion.LookRotation(d), Time.deltaTime * 6f);
        return false;
    }
}
