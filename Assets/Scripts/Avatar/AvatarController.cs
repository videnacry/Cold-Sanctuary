using UnityEngine;

/// <summary>
/// Manages which avatar-robot the player is piloting and applies its config to the SurfaceWalker
/// (docs §4). Cycling is gated:
///   - Inside the simulation: always allowed (among unlocked avatars).
///   - Outside the simulation: only if the player has the spell (`canSwitchOutsideSim`).
///
/// Switch key defaults to G to avoid the documented controls (Tab = cycle target, V = camera, etc.).
/// Attach to the avatar-robot alongside a SurfaceWalker.
/// </summary>
[RequireComponent(typeof(SurfaceWalker))]
public class AvatarController : MonoBehaviour
{
    [Header("References")]
    public SurfaceWalker walker;

    [Header("Avatars (orden de progresión: gusano → araña → mosco → …)")]
    public RobotAvatar[] avatars;
    public int currentIndex = 0;

    [Header("Switching")]
    public KeyCode switchKey = KeyCode.G;

    [Tooltip("True mientras el jugador pilota dentro de la simulación de mobs.")]
    public bool inSimulation = true;

    [Tooltip("Hechizo que permite cambiar de avatar también fuera de la simulación.")]
    public bool canSwitchOutsideSim = false;

    void Awake()
    {
        if (walker == null) walker = GetComponent<SurfaceWalker>();
    }

    void Start()
    {
        Apply(currentIndex);
    }

    void Update()
    {
        if (!Input.GetKeyDown(switchKey)) return;
        if (!inSimulation && !canSwitchOutsideSim) return;
        CycleNext();
    }

    /// <summary>Switch to the next UNLOCKED avatar (wraps around).</summary>
    public void CycleNext()
    {
        if (avatars == null || avatars.Length == 0) return;
        for (int step = 1; step <= avatars.Length; step++)
        {
            int idx = (currentIndex + step) % avatars.Length;
            if (avatars[idx].unlocked) { Apply(idx); return; }
        }
    }

    /// <summary>Apply avatar <paramref name="idx"/>'s config to the SurfaceWalker + body scale.</summary>
    public void Apply(int idx)
    {
        if (walker == null || avatars == null || idx < 0 || idx >= avatars.Length) return;
        RobotAvatar a = avatars[idx];
        if (!a.unlocked) return;

        currentIndex = idx;
        walker.moveSpeed = a.moveSpeed;
        walker.turnSpeed = a.turnSpeed;
        walker.canClimb  = a.locomotion == AvatarLocomotion.Climb;
        walker.flight    = a.locomotion == AvatarLocomotion.Flight;
        if (!walker.flight) walker.ResetUp();

        transform.localScale = Vector3.one * a.bodyScale;

        Debug.Log($"[Avatar] Pilotando: {a.name} ({a.locomotion}).");
    }

    /// <summary>Unlock an avatar by name (llamar desde la progresión al aprender el hechizo/stat).</summary>
    public void Unlock(string avatarName)
    {
        if (avatars == null) return;
        foreach (var a in avatars)
            if (a.name == avatarName) { a.unlocked = true; return; }
    }
}
