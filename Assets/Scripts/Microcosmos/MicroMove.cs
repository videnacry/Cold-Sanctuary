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
            // El agente ya «llegó» por dentro y el cuerpo sigue lejos (algo movió o sujetó el transform sin Warp): camino de una esquina,
            // 0 m por andar, velocidad 0, y se quedaba así hasta que le ponían allí (Kapilavastu, ronda 24). Se le devuelve al cuerpo.
            Vector3 gap = ag.nextPosition - t.position; gap.y = 0f;   // (en planta: la altura del agente y la del pivote no son la misma)
            if (gap.sqrMagnitude > 1f) { ag.Warp(t.position); ag.SetDestination(to); return false; }
            // Sin camino en curso pero lejos del destino (un Warp o el fin de otro camino lo vacía; destination sigue siendo la de antes): se pide de nuevo, o se queda quieto para siempre.
            bool idle = !ag.pathPending && (!ag.hasPath || ag.remainingDistance < 0.1f) && Time.frameCount % 15 == 0;
            if (idle || (ag.destination - to).sqrMagnitude > 1f) ag.SetDestination(to);
            return false;
        }
        t.position += d.normalized * Mathf.Min(speed * Time.deltaTime, d.magnitude);
        t.rotation = Quaternion.Slerp(t.rotation, Quaternion.LookRotation(d), Time.deltaTime * 6f);
        return false;
    }
}
