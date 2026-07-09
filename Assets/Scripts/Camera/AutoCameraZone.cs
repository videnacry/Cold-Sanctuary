using UnityEngine;

/// <summary>
/// Trigger volume that automatically switches the camera mode when Kushal enters,
/// then restores the player's preferred mode when he leaves.
///
/// Examples:
///   - Kitchen entrance → force FirstPerson (intimacy with the miniaturized world)
///   - MonsterSection corridor → force ThirdPerson (epic scale reveal)
///   - Cub area → FirstPerson (already handled by CameraManager.OnEnterCubArea,
///     but this is the general-purpose version)
///
/// "Automatic with preference": the player's preferred mode is NOT changed.
/// CameraManager.preferredMode stays intact and is restored on exit.
/// The player cannot manually toggle perspective while inside a forced zone
/// (CameraManager.SwitchTo blocks during robberies).
///
/// Setup:
///   1. Add a trigger Collider to this GameObject.
///   2. Set desiredMode and holdMode.
///   3. Tag the Player as "Player".
/// </summary>
[RequireComponent(typeof(Collider))]
public class AutoCameraZone : MonoBehaviour
{
    [Tooltip("Camera mode to use inside this zone.")]
    public CameraMode desiredMode = CameraMode.FirstPerson;

    [Tooltip("If true, the mode is locked for the entire stay (player cannot toggle). " +
             "If false, it's just a suggestion — the player can still override manually.")]
    public bool lockForDuration = true;

    [Tooltip("Only trigger on objects with this tag.")]
    public string playerTag = "Player";

    // Track entries so nested/overlapping zones work correctly
    int _entryCount;

    void Start()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[AutoCameraZone] Collider en {gameObject.name} forzado a Trigger.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        _entryCount++;

        if (CameraManager.Instance == null) return;

        if (lockForDuration)
        {
            // Use the robbery system — blocks manual toggling, restores on exit
            CameraManager.Instance.RequestRobbery(new CameraRobbery
            {
                type           = RobberyType.SwitchPerspective,
                targetMode     = desiredMode,
                holdDuration   = 0f,       // 0 = hold until we manually exit
                returnOnFinish = false     // we call the exit ourselves
            });
        }
        else
        {
            // Soft suggestion — just switch without locking
            CameraManager.Instance.SwitchTo(desiredMode);
        }

        Debug.Log($"[AutoCameraZone] {other.name} entró. Cámara → {desiredMode}.");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        _entryCount = Mathf.Max(0, _entryCount - 1);
        if (_entryCount > 0) return; // still inside overlapping zone

        if (CameraManager.Instance == null) return;

        // Restore the player's preferred mode
        CameraManager.Instance.SwitchTo(CameraManager.Instance.preferredMode, forced: true);

        Debug.Log($"[AutoCameraZone] {other.name} salió. Cámara → preferredMode.");
    }
}
