using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to a trigger collider to drive multiple systems when the player enters or exits.
/// Currently wires: CameraManager (mode switch / robbery) + BondActivityManager (context tags).
///
/// Supersedes CameraZoneTrigger — covers all its use-cases plus Bond integration.
/// Add new [Header] blocks here as additional systems need zone awareness.
/// Tag the player GameObject as "Player".
/// </summary>
public class ZoneActivator : MonoBehaviour
{
    // ── Camera ────────────────────────────────────────────────────────────────
    [Header("Camera")]
    public ZoneCameraAction cameraAction;

    // ── Bond context tags ─────────────────────────────────────────────────────
    [Header("Bond Context Tags")]
    [Tooltip("Added when the player enters; removed automatically on exit.")]
    public List<string> contextTags = new List<string>();

    // ── Runtime ───────────────────────────────────────────────────────────────
    BondActivityManager _bondManager;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _bondManager = player.GetComponent<BondActivityManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (CameraManager.Instance != null)
            cameraAction.Apply(CameraManager.Instance);

        if (_bondManager != null)
            foreach (string tag in contextTags)
                _bondManager.SetContextTag(tag);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (CameraManager.Instance != null && cameraAction.revertOnExit)
            CameraManager.Instance.SwitchTo(CameraManager.Instance.preferredMode, forced: true);

        if (_bondManager != null)
            foreach (string tag in contextTags)
                _bondManager.ClearContextTag(tag);
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.25f);
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box)
            Gizmos.DrawCube(transform.TransformPoint(box.center),
                            Vector3.Scale(box.size, transform.lossyScale));
        else if (col is SphereCollider sphere)
            Gizmos.DrawWireSphere(transform.TransformPoint(sphere.center),
                                  sphere.radius * Mathf.Max(transform.lossyScale.x,
                                                             transform.lossyScale.y,
                                                             transform.lossyScale.z));
    }
}

// ── Supporting types ──────────────────────────────────────────────────────────

[System.Serializable]
public class ZoneCameraAction
{
    public enum ActionType { None, SwitchMode, Robbery }

    [Tooltip("What to do with the camera when the player enters this zone.")]
    public ActionType actionType = ActionType.None;

    [Tooltip("Target mode — used when ActionType is SwitchMode.")]
    public CameraMode targetMode = CameraMode.FirstPerson;

    [Tooltip("Robbery config — used when ActionType is Robbery.")]
    public CameraRobbery robbery;

    [Tooltip("On exit: return to the player's preferred camera mode.")]
    public bool revertOnExit = true;

    public void Apply(CameraManager cam)
    {
        switch (actionType)
        {
            case ActionType.SwitchMode: cam.SwitchTo(targetMode);       break;
            case ActionType.Robbery:    cam.RequestRobbery(robbery);     break;
        }
    }
}
