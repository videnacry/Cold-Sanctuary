using UnityEngine;

/// <summary>Andar en el microcosmos: con NavMeshAgent si lo hay (rodea los muebles gigantes); si no, en línea recta.</summary>
public static class MicroMove
{
    /// <summary>Da un paso hacia <paramref name="to"/>; true = ya ha llegado.</summary>
    static readonly System.Collections.Generic.HashSet<Transform> _said = new System.Collections.Generic.HashSet<Transform>();
    sealed class Hand { public Vector3[] c; public int i; public Vector3 to; public float lift, until; }
    static readonly System.Collections.Generic.Dictionary<Transform, Hand> _hand = new System.Collections.Generic.Dictionary<Transform, Hand>();
    static readonly System.Collections.Generic.Dictionary<Transform, float> _still = new System.Collections.Generic.Dictionary<Transform, float>();
    static readonly System.Collections.Generic.Dictionary<Transform, int> _pendingFrom = new System.Collections.Generic.Dictionary<Transform, int>();
    static void EndHand(Transform t, UnityEngine.AI.NavMeshAgent ag)
    {
        _hand.Remove(t);
        if (ag == null) return;
        ag.enabled = true;
        if (UnityEngine.AI.NavMesh.SamplePosition(t.position, out UnityEngine.AI.NavMeshHit h, 3f, UnityEngine.AI.NavMesh.AllAreas)) ag.Warp(h.position);
    }
    public static bool Step(Transform t, Vector3 to, float speed, float arrive = 1.2f)
    {
        Vector3 d = to - t.position; d.y = 0f;
        var ag = t.GetComponent<UnityEngine.AI.NavMeshAgent>();
        // A MANO (ronda 29): el agente se quedaba con velocidad 0 en un camino de una esquina y 0 m (Madre y Tejedora de Kapilavastu), y ni
        // pedirle el camino otra vez lo movía. Si en 3 s no se ha movido y hay camino completo, se le lleva por las esquinas de ese camino con
        // el agente apagado, y al llegar (o si se pierde el destino) se enciende y se le posa en el NavMesh.
        if (_hand.TryGetValue(t, out Hand hd))
        {
            if ((hd.to - to).sqrMagnitude > 1f || d.magnitude <= arrive || hd.i >= hd.c.Length || Time.time > hd.until) { EndHand(t, ag); if (d.magnitude <= arrive) return true; }
            else
            {
                Vector3 next = hd.c[hd.i] + Vector3.up * hd.lift, dn = next - t.position, df = dn; df.y = 0f;
                float stepLen = speed * Time.deltaTime;
                if (df.magnitude <= Mathf.Max(0.3f, stepLen)) { t.position = next; hd.i++; }
                else { t.position += dn.normalized * stepLen; t.rotation = Quaternion.Slerp(t.rotation, Quaternion.LookRotation(df), Time.deltaTime * 6f); }
                return false;
            }
        }
        if (d.magnitude <= arrive) { _still.Remove(t); return true; }
        if (ag != null && ag.enabled && ag.isOnNavMesh)
        {
            ag.speed = speed;
            // (Ronda 30) Un camino PENDIENTE es esperar, no estar parado (el cálculo se reparte por fotogramas): hasta 30 fotogramas; solo si sigue
            // pendiente después, o si no lo está y no se mueve, cuenta para ir a mano.
            bool waiting = false;
            if (ag.pathPending) { if (!_pendingFrom.TryGetValue(t, out int pf)) { pf = Time.frameCount; _pendingFrom[t] = pf; } waiting = Time.frameCount - pf < 30; }
            else _pendingFrom.Remove(t);
            if (waiting || ag.velocity.sqrMagnitude > 0.04f || d.magnitude <= arrive + 1f) _still.Remove(t);
            else if (!_still.TryGetValue(t, out float since)) _still[t] = Time.time;
            else if (Time.time - since > 3f)
            {
                var hp = new UnityEngine.AI.NavMeshPath();
                if (UnityEngine.AI.NavMesh.CalculatePath(ag.nextPosition, to, ag.areaMask, hp) && hp.status == UnityEngine.AI.NavMeshPathStatus.PathComplete && hp.corners.Length > 1)
                {
                    float lift = t.position.y - ag.nextPosition.y;
                    _hand[t] = new Hand { c = hp.corners, i = 1, to = to, lift = lift, until = Time.time + 90f };
                    _still.Remove(t); ag.enabled = false;
                    Debug.Log($"[Horario] «{t.name}»: el agente no avanza ({ag.speed:0.0} m/s pedidos, parado {ag.isStopped}) — va a mano por {hp.corners.Length} esquinas hasta {to}.");
                    return false;
                }
                _still[t] = Time.time;
            }
            // El agente ya «llegó» por dentro y el cuerpo sigue lejos (algo movió o sujetó el transform sin Warp): camino de una esquina,
            // 0 m por andar, velocidad 0, y se quedaba así hasta que le ponían allí (Kapilavastu, ronda 24). Se le devuelve al cuerpo.
            Vector3 gap = ag.nextPosition - t.position; gap.y = 0f;   // (en planta: la altura del agente y la del pivote no son la misma)
            if (gap.sqrMagnitude > 1f) { ag.Warp(t.position); ag.SetDestination(to); return false; }
            // Sin camino en curso pero lejos del destino (un Warp o el fin de otro camino lo vacía; destination sigue siendo la de antes): se pide de nuevo, o se queda quieto para siempre.
            bool idle = !ag.pathPending && (!ag.hasPath || ag.remainingDistance < 0.1f) && Time.frameCount % 15 == 0;
            // «LLEGADO» LEJOS (ronda 26, Madre y Padre de Kapilavastu yendo a casa: alcanzable, camino completo de una esquina, 0 m por andar,
            // velocidad 0, el agente con el cuerpo): pedir el destino otra vez daba el mismo camino vacío. Se calcula desde donde está el agente
            // y se le pone ese camino; si tampoco sale, se dice una vez qué da.
            if (idle && d.magnitude > arrive + 1f)
            {
                var p = new UnityEngine.AI.NavMeshPath();
                bool ok = UnityEngine.AI.NavMesh.CalculatePath(ag.nextPosition, to, ag.areaMask, p);
                if (ok && p.corners.Length > 1 && ag.SetPath(p)) return false;
                if (_said.Add(t)) Debug.LogWarning($"[Horario] «{t.name}» no tiene camino a {to} desde {ag.nextPosition}: CalculatePath {ok}, {p.status}, {p.corners.Length} esquinas (a {d.magnitude:0.0} m).");
            }
            // (Ronda 30) En PLANTA: el destino del agente es el punto del NavMesh y el que se pide va 1 m por encima (OnMesh + arriba); en 3D la
            // diferencia era 1 m justo y, con el redondeo, a veces «otro destino» en cada fotograma: cada SetDestination reiniciaba el cálculo del
            // camino, que a 0,5 FPS no acababa nunca (pathPending, velocidad 0).
            Vector3 dd = ag.destination - to; dd.y = 0f;
            if (idle || dd.sqrMagnitude > 1f) ag.SetDestination(to);
            return false;
        }
        t.position += d.normalized * Mathf.Min(speed * Time.deltaTime, d.magnitude);
        t.rotation = Quaternion.Slerp(t.rotation, Quaternion.LookRotation(d), Time.deltaTime * 6f);
        return false;
    }
}
