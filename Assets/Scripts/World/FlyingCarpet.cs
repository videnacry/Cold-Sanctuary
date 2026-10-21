using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ALFOMBRA VOLADORA (una esterilla grande de la pista de aterrizaje). Automática: al TOCARLA el jugador se le muestran
/// los destinos (las otras pistas) en un diálogo (↑/↓ + Intro o clic); elige y la cámara pasa a 3ª persona, el pasajero
/// se SIENTA y la alfombra lo lleva (sube, cruza y baja). Al llegar se levanta y vuelve a ver las opciones (seguir o
/// bajarse). Los trabajadores la usan solos para cambiar de grupo de áreas (CarpetPad.Route).
/// </summary>
[RequireComponent(typeof(Collider))]
public class FlyingCarpet : MonoBehaviour
{
    public static readonly List<FlyingCarpet> All = new List<FlyingCarpet>();
    static readonly HashSet<GameObject> _riders = new HashSet<GameObject>();
    public static bool Riding(GameObject g) => g != null && _riders.Contains(g);
    public static int Trips { get; private set; }

    public CarpetPad pad;
    public float speed = 18f, cruise = 26f;
    public bool Flying { get; private set; }
    float _ignoreUntil;

    void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void OnTriggerEnter(Collider other)
    {
        if (Flying || Time.time < _ignoreUntil || !other.CompareTag("Player") || ChoiceDialog.IsOpen) return;
        Offer(other.gameObject, false);
    }

    /// <summary>Diálogo de destinos para el jugador.</summary>
    public void Offer(GameObject who, bool arrived)
    {
        var dests = new List<CarpetPad>();
        foreach (CarpetPad p in CarpetPad.All) if (p != pad) dests.Add(p);
        var opts = new List<string>();
        if (arrived) opts.Add("Bajarse aquí");
        foreach (CarpetPad p in dests) opts.Add($"Volar a: {p.displayName}");
        int off = arrived ? 1 : 0;
        ChoiceDialog.Show(arrived ? $"Has llegado a {pad?.displayName}" : "Alfombra voladora · ¿a dónde vamos?", opts.ToArray(),
            i => { if (i >= off) Fly(who, dests[i - off]); else _ignoreUntil = Time.time + 2f; },
            () => _ignoreUntil = Time.time + 2f, transform, 3.5f);   // alejarse lo cierra
    }

    /// <summary>Lleva a <paramref name="who"/> sentado hasta la pista <paramref name="to"/>.</summary>
    public void Fly(GameObject who, CarpetPad to)
    {
        if (Flying || who == null || to == null) return;
        StartCoroutine(Trip(who, to));
    }

    IEnumerator Trip(GameObject who, CarpetPad to)
    {
        Flying = true; _riders.Add(who); Trips++;
        bool player = who.CompareTag("Player");
        Anima a = who.GetComponent<Anima>();
        var agent = who.GetComponent<UnityEngine.AI.NavMeshAgent>();
        var cc = who.GetComponent<CharacterController>();
        var pc = who.GetComponent<PlayerController>();
        bool agentWas = agent != null && agent.enabled, ccWas = cc != null && cc.enabled, pcWas = pc != null && pc.enabled;
        if (agent != null) agent.enabled = false;
        if (cc != null) cc.enabled = false;
        if (pc != null) pc.enabled = false;
        if (player && CameraManager.Instance != null) CameraManager.Instance.SwitchTo(CameraMode.ThirdPerson, forced: true);
        Debug.Log($"[Alfombra] «{who.name}» se sienta en la alfombra y vuela de {pad?.displayName} a {to.displayName}.");

        Vector3 from = transform.position, dest = to.FreeSlot(this);
        float top = Mathf.Max(from.y, dest.y) + cruise;
        float dist = Vector3.Distance(new Vector3(from.x, 0f, from.z), new Vector3(dest.x, 0f, dest.z));
        float T = Mathf.Max(3f, (dist + 2f * cruise) / speed), t = 0f;
        Quaternion face = Quaternion.LookRotation(Flat(dest - from));
        while (t < T)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / T);
            float s = Mathf.SmoothStep(0f, 1f, u);
            Vector3 p = Vector3.Lerp(from, dest, s);
            p.y = Mathf.Lerp(from.y, dest.y, s) + Mathf.Sin(u * Mathf.PI) * (top - Mathf.Max(from.y, dest.y)) + Mathf.Sin(t * 3f) * 0.05f;   // sube, cruza y baja (con un leve vaivén)
            transform.SetPositionAndRotation(p, Quaternion.Slerp(transform.rotation, face, Time.deltaTime * 2f));
            who.transform.SetPositionAndRotation(p + Vector3.up * 0.12f, transform.rotation);
            if (a != null) DailyPoses.Sit().ApplyFinal(a);   // sentado, tranquilo, esperando desembarcar
            yield return null;
        }
        transform.position = dest;
        pad = to;
        // DESEMBARCAR: se levanta al lado de la alfombra.
        if (a != null) DailyPoses.Stand().ApplyFinal(a);
        who.transform.position = dest + transform.right * 1.6f + Vector3.up * 0.1f;
        if (cc != null) cc.enabled = ccWas;
        if (agent != null) { agent.enabled = agentWas; if (agent.enabled && agent.isOnNavMesh) agent.Warp(who.transform.position); }
        if (pc != null) pc.enabled = pcWas;
        if (player && CameraManager.Instance != null) CameraManager.Instance.SwitchTo(CameraManager.Instance.preferredMode, forced: true);
        _riders.Remove(who);
        Flying = false;
        _ignoreUntil = Time.time + 1f;
        Debug.Log($"[Alfombra] «{who.name}» llega a {to.displayName} y se baja.");
        if (player) Offer(who, true);   // por contacto: vuelve a ver las opciones (seguir o bajarse)
    }

    static Vector3 Flat(Vector3 v) { v.y = 0f; return v.sqrMagnitude > 1e-4f ? v : Vector3.forward; }
}
