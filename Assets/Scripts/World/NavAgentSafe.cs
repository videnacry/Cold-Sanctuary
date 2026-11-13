using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// (2026-11-11) VOLVER A ENCENDER UN AGENTE SIN «Failed to create agent because it is not close enough to the NavMesh» (64 por partida en vivo74):
/// la alfombra al aterrizar, la Línea de las cavernas, el muelle aéreo, quien deja de ir en brazos y el paseo «a mano» del microcosmos lo
/// encendían donde el cuerpo estuviera. Si no hay NavMesh a 1 m, lo dice UNA vez por nombre y lo pone en el punto del NavMesh más cercano
/// (hasta 6 m) antes de encenderlo; después, como antes, Warp al cuerpo.
/// </summary>
public static class NavAgentSafe
{
    static readonly HashSet<string> _told = new HashSet<string>();
    public static int Moved { get; private set; }

    public static void Enable(NavMeshAgent ag, string why)
    {
        if (ag == null || ag.enabled) return;
        Vector3 p = ag.transform.position;
        if (!NavMesh.SamplePosition(p, out _, 1f, ag.areaMask))
        {
            if (_told.Add(ag.name)) Debug.Log($"[NavMesh] «{ag.name}» se enciende sin NavMesh a 1 m ({why}, en {p}): se le pone en el punto más cercano.");
            if (NavMesh.SamplePosition(p, out NavMeshHit h, 6f, ag.areaMask)) { ag.transform.position = h.position + Vector3.up * ag.baseOffset; Moved++; }
        }
        ag.enabled = true;
        if (ag.isOnNavMesh) ag.Warp(ag.transform.position);
    }

    /// <summary>Lo mismo, solo si estaba encendido antes (<paramref name="was"/>).</summary>
    public static void Restore(NavMeshAgent ag, bool was, string why) { if (ag != null && was) Enable(ag, why); }
}
