using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>
/// LLEVAR A UN SER (paciente o tripulante) dentro de un vehículo: se le quitan, mientras va, las conductas que lo moverían
/// (IA, NavMeshAgent, nado del mar, merodeo) y se cuelga del sitio que le toca (camilla, asiento, pozo médico); al bajar se le
/// devuelven tal cual y se le deja de pie (o tumbado) donde toca, con su agente en el NavMesh. Nada de teletransportes: quien
/// lo lleva lo MUEVE (rampa, camilla, grúa) con <see cref="MoveTowards"/>.
/// </summary>
public class Carried : MonoBehaviour
{
    readonly List<Behaviour> _off = new List<Behaviour>();
    bool _kinematic, _gravity; Rigidbody _rb;
    Transform _oldParent;

    static readonly System.Type[] Keep = { typeof(Anima), typeof(BloodLoss), typeof(WorldLabel), typeof(Carried) };

    public static Carried Take(GameObject g, Transform holder)
    {
        if (g == null) return null;
        Carried c = g.GetComponent<Carried>();
        if (c == null) c = g.AddComponent<Carried>();
        c._oldParent = g.transform.parent;
        c._off.Clear();
        foreach (Behaviour b in g.GetComponents<Behaviour>())
        {
            if (b == null || !b.enabled) continue;
            bool keep = false; foreach (System.Type t in Keep) if (t.IsInstanceOfType(b) && !(b is Animal)) keep = true;
            if (b is Anima && !(b is Animal)) keep = true;   // (las personas: su Anima sigue viva —hambre, enfermedad—; los animales: su IA se para)
            if (keep) continue;
            b.enabled = false; c._off.Add(b);
        }
        c._rb = g.GetComponent<Rigidbody>();
        if (c._rb != null) { c._kinematic = c._rb.isKinematic; c._gravity = c._rb.useGravity; c._rb.isKinematic = true; c._rb.useGravity = false; }
        g.transform.SetParent(holder, true);
        return c;
    }

    /// <summary>Lo baja en <paramref name="at"/> (pies), mirando a <paramref name="facing"/>, y le devuelve sus conductas.</summary>
    public void Release(Vector3 at, Quaternion facing)
    {
        transform.SetParent(_oldParent, true);
        transform.SetPositionAndRotation(at, facing);
        if (_rb != null) { _rb.isKinematic = _kinematic; _rb.useGravity = _gravity; }
        // (2026-11-11) El agente, el último y con NavAgentSafe: encendido donde queda el cuerpo, sin NavMesh debajo, daba «Failed to create agent».
        NavMeshAgent ag = GetComponent<NavMeshAgent>(); bool agWas = false;
        foreach (Behaviour b in _off) if (b != null) { if (b == ag) { agWas = true; continue; } b.enabled = true; }
        if (ag != null && agWas) { if (NavMesh.SamplePosition(at, out NavMeshHit hit, 3f, NavMesh.AllAreas)) transform.position = hit.position + Vector3.up * ag.baseOffset; NavAgentSafe.Enable(ag, "lo bajan"); }
        _off.Clear();
        Destroy(this);
    }

    /// <summary>Un paso de la camilla / la grúa hacia <paramref name="to"/> (mundo). true al llegar.</summary>
    public static bool MoveTowards(Transform t, Vector3 to, float speed)
    {
        t.position = Vector3.MoveTowards(t.position, to, speed * Time.deltaTime);
        return (t.position - to).sqrMagnitude < 0.0025f;
    }
}
