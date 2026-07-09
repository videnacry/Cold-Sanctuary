using UnityEngine;

/// <summary>
/// Kushal's player controller for Cold Sanctuary.
///
/// KEY LAYOUT PHILOSOPHY — Mecanografía (touch-typing posture):
///   Left hand  → Movement + sprint + jump (WASD / Shift / Space).
///   Right hand → Mouse look OR arrow-key look + ability hotkeys (1–0) + Tab targeting.
///   Both hands stay in their natural typing positions; no awkward reaches.
///   All keys are configurable in the Inspector so players can remap without code changes.
///
/// Camera architecture:
///   This script rotates the player body (yaw) and a cameraPivot child (pitch).
///   CameraManager follows whichever anchor (ThirdPerson / FirstPerson) is active.
///   V toggles perspective; AutoCameraZone handles automatic per-zone overrides.
///
/// Scene setup:
///   1. CharacterController on this GameObject.
///   2. Create empty child "CameraPivot" at head height.
///        └─ "ThirdPersonAnchor" (e.g. localPos 0, 0.5, -3.5)
///        └─ "FirstPersonAnchor" (e.g. localPos 0, 0, 0.2)
///   3. Assign those to CameraManager.thirdPersonAnchor / firstPersonAnchor.
///   4. Assign cameraPivot here.
///   5. Tag this GameObject "Player".
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ── Movement (left hand) ──────────────────────────────────────────────────

    [Header("Movement — left hand")]
    public float walkSpeed   = 4f;
    public float sprintSpeed = 8f;
    public float jumpHeight  = 1.4f;
    public float gravity     = 20f;

    [Space]
    [Tooltip("Hold to sprint.")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    [Tooltip("Jump. Also swim up while in water — same key, same hand, no new reach.")]
    public KeyCode jumpKey   = KeyCode.Space;

    // ── Swimming (left hand — same keys as land, WaterZone toggles the mode) ───

    [Header("Swimming")]
    public float swimSpeed         = 3f;
    public float swimVerticalSpeed = 2.5f;
    [Tooltip("Swim down. Swim up reuses jumpKey.")]
    public KeyCode swimDownKey = KeyCode.LeftControl;

    // ── Mouse look ────────────────────────────────────────────────────────────

    [Header("Mouse look — right hand (mouse)")]
    public float mouseSensitivityX = 2f;
    public float mouseSensitivityY = 2f;
    [Tooltip("Hold this button to rotate the camera. Right button by default — left click " +
             "(Mouse0) is already PlayerCombat.attackKey and is also how the holographic menu " +
             "gets clicked, so it has to stay free instead of being eaten by a permanent cursor lock.")]
    public KeyCode cameraLookButton = KeyCode.Mouse1;

    // ── Keyboard look (right hand, no mouse) ─────────────────────────────────

    [Header("Keyboard look — right hand (arrows, no mouse)")]
    [Tooltip("Enable arrow-key camera rotation for keyboard-only or gamepad play.")]
    public bool enableKeyboardLook    = true;
    public float keyboardYawSpeed     = 90f;   // deg/s
    public float keyboardPitchSpeed   = 60f;   // deg/s

    [Space]
    [Tooltip("Home-row right hand (Spanish layout): J=left, L=right, I=up, K=down.")]
    public KeyCode lookLeftKey  = KeyCode.J;
    public KeyCode lookRightKey = KeyCode.L;
    public KeyCode lookUpKey    = KeyCode.I;
    public KeyCode lookDownKey  = KeyCode.K;

    // ── Pitch clamp ───────────────────────────────────────────────────────────

    [Header("Pitch limits")]
    [Range(10f, 89f)] public float maxPitchDown = 60f;
    [Range(10f, 89f)] public float maxPitchUp   = 75f;

    // ── Camera ────────────────────────────────────────────────────────────────

    [Header("Camera")]
    [Tooltip("Empty transform at head height — parent of both camera anchors.")]
    public Transform cameraPivot;

    [Tooltip("Key to toggle 3rd ↔ 1st person.")]
    public KeyCode perspectiveToggleKey = KeyCode.V;

    // ── Runtime ───────────────────────────────────────────────────────────────

    CharacterController _cc;
    float _verticalVelocity;
    float _currentPitch;
    bool  _isSwimming;

    /// <summary>Called by WaterZone on trigger enter/exit.</summary>
    public void SetSwimming(bool swimming)
    {
        _isSwimming = swimming;
        if (swimming) _verticalVelocity = 0f; // don't carry fall speed into the water
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    void Start()
    {
        // Cursor starts free (visible, unlocked) — locking is now momentary, only while
        // cameraLookButton is held (see HandleCursorToggle). A permanently-locked cursor
        // made it impossible to click the holographic menu.
        SetCursorLocked(false);

        // Restore saved camera preference
        if (CameraManager.Instance != null)
        {
            int saved = PlayerPrefs.GetInt("CameraMode", (int)CameraMode.ThirdPerson);
            CameraManager.Instance.preferredMode = (CameraMode)saved;
            CameraManager.Instance.SwitchTo(CameraManager.Instance.preferredMode, forced: true);
        }

        // Auto cursor management during dialogue
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnSequenceStarted  += _ => SetCursorLocked(false);
            DialogueManager.Instance.OnSequenceFinished += _ => SetCursorLocked(false);
        }
    }

    void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnSequenceStarted  -= _ => SetCursorLocked(false);
            DialogueManager.Instance.OnSequenceFinished -= _ => SetCursorLocked(false);
        }
    }

    void Update()
    {
        HandleCursorToggle();
        HandleLook();
        HandleMovement();
        HandlePerspectiveToggle();
    }

    // ── Look ──────────────────────────────────────────────────────────────────

    void HandleLook()
    {
        // Block look during dialogue
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) return;

        float yaw   = 0f;
        float pitch = 0f;

        // Mouse look only while cameraLookButton is held (see HandleCursorToggle) — the
        // cursor gets locked for the duration of the hold so the delta doesn't run out at
        // the screen edge, then released so the other button stays free for UI/interact.
        if (Input.GetKey(cameraLookButton))
        {
            yaw   += Input.GetAxis("Mouse X") * mouseSensitivityX;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivityY;
        }

        // Keyboard look (arrow keys — additive with mouse)
        if (enableKeyboardLook)
        {
            if (Input.GetKey(lookRightKey)) yaw   += keyboardYawSpeed   * Time.deltaTime;
            if (Input.GetKey(lookLeftKey))  yaw   -= keyboardYawSpeed   * Time.deltaTime;
            if (Input.GetKey(lookDownKey))  pitch += keyboardPitchSpeed  * Time.deltaTime;
            if (Input.GetKey(lookUpKey))    pitch -= keyboardPitchSpeed  * Time.deltaTime;
        }

        // Apply yaw to the player body
        if (!Mathf.Approximately(yaw, 0f))
            transform.Rotate(Vector3.up, yaw, Space.World);

        // Apply pitch to the camera pivot only
        if (cameraPivot != null && !Mathf.Approximately(pitch, 0f))
        {
            _currentPitch = Mathf.Clamp(_currentPitch + pitch, -maxPitchUp, maxPitchDown);
            cameraPivot.localRotation = Quaternion.Euler(_currentPitch, 0f, 0f);
        }
    }

    // ── Movement ──────────────────────────────────────────────────────────────

    void HandleMovement()
    {
        if (_isSwimming)
        {
            HandleSwimMovement();
            return;
        }

        // Horizontal
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 move = Vector3.ClampMagnitude(transform.right * h + transform.forward * v, 1f);
        float speed = Input.GetKey(sprintKey) ? sprintSpeed : walkSpeed;

        // Vertical / gravity
        if (_cc.isGrounded)
        {
            _verticalVelocity = -2f; // keep grounded
            if (Input.GetKeyDown(jumpKey))
                _verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
        }
        else
        {
            _verticalVelocity -= gravity * Time.deltaTime;
        }

        Vector3 velocity = move * speed;
        velocity.y = _verticalVelocity;
        _cc.Move(velocity * Time.deltaTime);
    }

    // Free 3D movement, no gravity — WASD for the horizontal plane, jumpKey/swimDownKey for
    // up/down. Reuses the same left-hand keys as on land (jumpKey doubles as "swim up") so
    // there's nothing new to learn entering the water.
    void HandleSwimMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 move = Vector3.ClampMagnitude(transform.right * h + transform.forward * v, 1f);

        float vertical = 0f;
        if (Input.GetKey(jumpKey))     vertical += 1f;
        if (Input.GetKey(swimDownKey)) vertical -= 1f;

        Vector3 velocity = move * swimSpeed;
        velocity.y = vertical * swimVerticalSpeed;
        _cc.Move(velocity * Time.deltaTime);
        _verticalVelocity = 0f;
    }

    // ── Perspective toggle ────────────────────────────────────────────────────

    void HandlePerspectiveToggle()
    {
        if (!Input.GetKeyDown(perspectiveToggleKey)) return;
        if (CameraManager.Instance == null) return;

        CameraMode next = CameraManager.Instance.preferredMode == CameraMode.ThirdPerson
            ? CameraMode.FirstPerson
            : CameraMode.ThirdPerson;

        // Blocked internally if a camera robbery (AutoCameraZone) is active
        CameraManager.Instance.SwitchTo(next);
        PlayerPrefs.SetInt("CameraMode", (int)next);
        PlayerPrefs.Save();
    }

    // ── Cursor ────────────────────────────────────────────────────────────────

    void HandleCursorToggle()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) return;

        if (Input.GetKeyDown(cameraLookButton))
            SetCursorLocked(true);
        else if (Input.GetKeyUp(cameraLookButton))
            SetCursorLocked(false);
        else if (Input.GetKeyDown(KeyCode.Escape))
            SetCursorLocked(false);
    }

    static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !locked;
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        if (cameraPivot == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(cameraPivot.position, 0.1f);
        Gizmos.DrawLine(transform.position, cameraPivot.position);
    }
}
