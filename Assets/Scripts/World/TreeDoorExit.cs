using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SALIR DEL AGUA POR UNA PUERTA DEL ÁRBOL (Berón 2026-10-30: «nadé por la rama, pero no pude salir de ella»). Cada puerta con
/// campo magnético (y la trampilla del techo) tiene delante, en el agua, una zona: quien NADA ahí (el jugador o cualquiera con
/// <see cref="PlayerController"/>) y empuja hacia fuera —hacia la puerta, o hacia arriba en la trampilla— sale: se pone de pie en
/// el umbral de fuera, como al apoyar las manos y trepar a un canto (<c>PlayerController.TryClimbOut</c>). Así no se queda
/// flotando contra el dintel. Los personajes que viajan con <see cref="TreeTrip"/> no lo necesitan (su viaje ya sale andando).
/// La transform de la puerta mira HACIA FUERA (+Z) y está en el umbral (y = 0, el suelo de la puerta).
/// </summary>
public class TreeDoorExit : MonoBehaviour
{
    [Tooltip("Zona (local de la puerta) donde se nada delante de ella: x a lo ancho, y desde el umbral, z hacia fuera (negativo = dentro del agua).")]
    public Vector3 zoneCenter = new Vector3(0f, 1.3f, -1f), zoneSize = new Vector3(2.6f, 4.6f, 3.4f);
    [Tooltip("Donde se queda de pie al salir (los pies).")]
    public Transform landing;
    [Tooltip("Trampilla del techo: se sale empujando hacia arriba o hacia cualquier lado.")]
    public bool hatch;

    public static int Exits { get; private set; }
    static readonly List<PlayerController> _swimmers = new List<PlayerController>();
    static float _scanAt;

    void Update()
    {
        if (landing == null) return;
        if (Time.time >= _scanAt)
        {
            _scanAt = Time.time + 0.5f;
            _swimmers.Clear(); _swimmers.AddRange(FindObjectsByType<PlayerController>(FindObjectsSortMode.None));
        }
        for (int i = 0; i < _swimmers.Count; i++)
        {
            PlayerController pc = _swimmers[i];
            if (pc == null || !pc.isActiveAndEnabled || !pc.IsSwimming || TreeTrip.Busy(pc.gameObject)) continue;
            Vector3 l = transform.InverseTransformPoint(pc.transform.position) - zoneCenter;
            if (Mathf.Abs(l.x) > zoneSize.x * 0.5f || Mathf.Abs(l.y) > zoneSize.y * 0.5f || Mathf.Abs(l.z) > zoneSize.z * 0.5f) continue;
            Vector3 want = pc.MoveIntent;
            Vector3 flat = Vector3.ProjectOnPlane(want, transform.up);
            bool go = hatch ? want.y > 0.3f || flat.magnitude > 0.3f : Vector3.Dot(flat, transform.forward) > 0.3f;
            if (go) Exit(pc);
        }
    }

    void Exit(PlayerController pc)
    {
        SwimBody sb = SwimBody.Of(pc.transform);
        float up = sb != null ? sb.PivotToFeet : 1f;
        CharacterController cc = pc.GetComponent<CharacterController>();
        bool was = cc != null && cc.enabled;
        if (cc != null) cc.enabled = false;
        pc.transform.position = landing.position + Vector3.up * (up + 0.05f);
        Vector3 fw = Vector3.ProjectOnPlane(hatch ? landing.position - transform.position : transform.forward, Vector3.up);
        if (fw.sqrMagnitude > 0.01f) pc.transform.rotation = Quaternion.LookRotation(fw.normalized);
        if (cc != null && was) cc.enabled = true;
        pc.SetSwimming(false);
        Physics.SyncTransforms();
        Exits++;
        Debug.Log($"[Árbol] «{pc.name}» sale del agua por {name} (de pie en {landing.name}).");
    }
}
