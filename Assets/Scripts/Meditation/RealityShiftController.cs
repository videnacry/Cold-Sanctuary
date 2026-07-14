using UnityEngine;

/// <summary>
/// Generalizes KitchenScaleController: drives the miniaturization (reality shift) for ANY area.
///
/// Scales the environment root UP rather than scaling the player down — same trick as
/// KitchenScaleController, to avoid breaking the CharacterController / physics.
///
/// Key difference (docs/magic-plane-and-meditation.md §3): the transition is NEVER shown, so
/// the shift is a SNAP (no animated lerp, no shader). It happens while ScreenFader is black,
/// so the player only ever sees the world before and after — never the resize.
///
/// One RealityShiftController lives per area (assigned to that area's VirtualizationMachine).
/// </summary>
public class RealityShiftController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Root GameObject of this area's geometry (+ mob sets). Scaled up to miniaturize.")]
    public Transform environmentRoot;

    [Tooltip("Player camera — FOV is widened for the 'ant perspective'. Defaults to Camera.main.")]
    public Camera playerCamera;

    [Header("Scale")]
    [Tooltip("Default miniaturization scale when a mission doesn't override it. " +
             "8 makes the player feel ant-sized.")]
    [Min(1f)] public float defaultScale = 8f;

    [Header("Camera FOV")]
    public float normalFOV   = 70f;
    public float miniatureFOV = 85f;

    /// <summary>True while an area is miniaturized (a mission is active in this area).</summary>
    public bool IsShifted { get; private set; }

    Vector3 _originalScale = Vector3.one;

    void Awake()
    {
        if (environmentRoot != null) _originalScale = environmentRoot.localScale;
        if (playerCamera == null)    playerCamera   = Camera.main;
    }

    /// <summary>
    /// Snap into the miniaturized world for <paramref name="mission"/>. Call while the screen
    /// is black. Activates the mission's mobs and fires its onBegin.
    /// </summary>
    public void ShiftIn(MobMission mission)
    {
        float scale = (mission != null && mission.scaleOverride > 0f)
            ? mission.scaleOverride
            : defaultScale;

        if (environmentRoot != null)
            environmentRoot.localScale = _originalScale * scale;

        if (playerCamera != null)
            playerCamera.fieldOfView = miniatureFOV;

        if (mission != null)
        {
            mission.ActivateMobs(true);
            mission.RaiseBegin();
        }

        IsShifted = true;
        Debug.Log($"[RealityShift] Dentro del plano: «{mission?.missionName}». " +
                  "¡Bienvenido al mundo pequeño!");
    }

    /// <summary>
    /// Snap back to normal size. Call while the screen is black. Deactivates the mission's mobs
    /// and fires its onEnd.
    /// </summary>
    public void ShiftOut(MobMission mission)
    {
        if (mission != null) mission.RaiseEnd();

        if (environmentRoot != null)
            environmentRoot.localScale = _originalScale;

        if (playerCamera != null)
            playerCamera.fieldOfView = normalFOV;

        if (mission != null) mission.ActivateMobs(false);

        IsShifted = false;
        Debug.Log("[RealityShift] De vuelta al tamaño normal.");
    }
}
