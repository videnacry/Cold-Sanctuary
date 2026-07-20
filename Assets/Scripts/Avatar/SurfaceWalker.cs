using UnityEngine;

/// <summary>
/// Locomotion for the avatar-robot inside the plano mágico (docs §4, Eje A).
/// Gravity is RELATIVE TO THE SURFACE NORMAL: floor, wall and ceiling are the same code — the
/// body's "up" aligns to whatever surface it sticks to, so walking onto a wall just re-parents
/// "down" to that wall. What surfaces are allowed is set by the active Avatar (AvatarController):
///   - Ground only (gusano): sticks to floor-ish surfaces, no climbing.
///   - Climb (araña): sticks to any surface (walls + ceiling).
///   - Flight (mosco): ignores surfaces, free 3D movement.
///
/// Kinematic (moves the Transform directly) — attach to the avatar-robot, NOT to the main player
/// (which uses its own CharacterController outside the simulation). Needs colliders on the
/// walkable geometry. This is a first playable pass — expect to tune the raycast/offset values
/// in-editor.
///
/// Controls (legacy Input, coherente con el proyecto): WASD/flechas = girar + avanzar; en vuelo,
/// Space/LeftControl = subir/bajar.
/// </summary>
public class SurfaceWalker : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float turnSpeed = 120f;

    [Header("Surface sticking")]
    public LayerMask walkable = ~0;
    [Tooltip("How far below the body to search for a surface.")]
    public float stickDistance = 1.5f;
    [Tooltip("How far the body floats above the surface (≈ its radius).")]
    public float surfaceOffset = 0.5f;
    [Tooltip("How fast the body re-aligns to a new surface normal.")]
    public float alignSpeed = 10f;

    [Header("Allowed surfaces (set by Avatar)")]
    [Tooltip("araña: puede trepar paredes y techo.")]
    public bool canClimb = false;
    [Tooltip("mosco: vuela, ignora superficies.")]
    public bool flight = false;

    [Header("Input")]
    public bool controlEnabled = true;
    public KeyCode ascendKey  = KeyCode.Space;
    public KeyCode descendKey = KeyCode.LeftControl;

    const float FloorDot = 0.5f; // gusano: sólo normales "hacia arriba"

    Vector3 _up = Vector3.up;

    void Awake() => _up = transform.up;

    void Update()
    {
        if (flight) FlightMove();
        else        SurfaceMove();
    }

    // ── Surface (gusano / araña) ────────────────────────────────────────────────

    void SurfaceMove()
    {
        float turn = controlEnabled ? Input.GetAxisRaw("Horizontal") : 0f;
        float fwd  = controlEnabled ? Input.GetAxisRaw("Vertical")   : 0f;

        // Turn around the current up.
        if (turn != 0f)
            transform.Rotate(_up, turn * turnSpeed * Time.deltaTime, Space.World);

        // Forward projected onto the surface plane (guarded against looking straight along up).
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, _up);
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.ProjectOnPlane(transform.right, _up);
        forward = forward.normalized;

        if (fwd != 0f)
            transform.position += forward * (fwd * moveSpeed * Time.deltaTime);

        // Wall/ceiling transition: probe ahead for a climbable surface (araña only).
        if (canClimb && fwd != 0f &&
            Physics.Raycast(transform.position, forward, out RaycastHit wall, surfaceOffset + 0.3f, walkable))
        {
            _up = wall.normal;
            transform.position = wall.point + wall.normal * surfaceOffset;
        }

        // Stick to the surface below (gravity along -up).
        if (Physics.Raycast(transform.position + _up * 0.1f, -_up,
                out RaycastHit hit, stickDistance + surfaceOffset, walkable))
        {
            bool allowed = canClimb || Vector3.Dot(hit.normal, Vector3.up) > FloorDot;
            if (allowed)
            {
                _up = Vector3.Slerp(_up, hit.normal, alignSpeed * Time.deltaTime);
                transform.position = Vector3.Lerp(
                    transform.position, hit.point + hit.normal * surfaceOffset, alignSpeed * Time.deltaTime);
            }
        }

        AlignUpTo(_up);
    }

    // ── Flight (mosco) ──────────────────────────────────────────────────────────

    void FlightMove()
    {
        _up = Vector3.up;
        if (!controlEnabled) return;

        float turn = Input.GetAxisRaw("Horizontal");
        float fwd  = Input.GetAxisRaw("Vertical");
        float vert = (Input.GetKey(ascendKey) ? 1f : 0f) - (Input.GetKey(descendKey) ? 1f : 0f);

        if (turn != 0f)
            transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime, Space.World);

        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 move = flatForward * fwd + Vector3.up * vert;
        transform.position += move * (moveSpeed * Time.deltaTime);

        // Keep level in the air.
        Quaternion level = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, level, alignSpeed * Time.deltaTime);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    void AlignUpTo(Vector3 up)
    {
        Quaternion target = Quaternion.FromToRotation(transform.up, up) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, target, alignSpeed * Time.deltaTime);
    }

    /// <summary>Reset the tracked up (e.g. when (re)spawning on the floor).</summary>
    public void ResetUp() => _up = Vector3.up;
}
