using UnityEngine;

/// <summary>
/// Attach to a trigger collider surrounding a zone (e.g. the cub area).
/// Notifies CameraManager when the player enters or exits.
/// Tag the player GameObject as "Player".
/// </summary>
public class CameraZoneTrigger : MonoBehaviour
{
    public enum ZoneType { CubArea, Custom }

    [Header("Zone")]
    public ZoneType zoneType = ZoneType.CubArea;

    [Tooltip("Only used when ZoneType is Custom — provide a specific robbery to request.")]
    public CameraRobbery customRobbery;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (CameraManager.Instance == null) return;

        switch (zoneType)
        {
            case ZoneType.CubArea:
                CameraManager.Instance.OnEnterCubArea();
                break;
            case ZoneType.Custom:
                CameraManager.Instance.RequestRobbery(customRobbery);
                break;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (CameraManager.Instance == null) return;

        if (zoneType == ZoneType.CubArea)
            CameraManager.Instance.OnExitCubArea();
    }
}
