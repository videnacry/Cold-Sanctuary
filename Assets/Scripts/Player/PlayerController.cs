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
    WalkSpell _walk;                 // locomoción universal: provee la velocidad (carga/postura/punta/decae) — PR #91
    float _verticalVelocity;
    float _currentPitch;
    float _pivotYaw;                 // giro SOLO de la cámara mientras el cuerpo está sujeto (escalera, alfombra, diálogo…)
    int   _bodyHolds;
    bool  _isSwimming;

    /// <summary>¿Algo sujeta el cuerpo (trepar una escalera, volar en alfombra, estar sentado en un examen…)? Entonces no
    /// anda, pero la CÁMARA sigue libre: mirar alrededor no cambia la mecánica (Berón 2026-10-23).</summary>
    public bool BodyHeld => _bodyHolds > 0;

    /// <summary>Sujeta (true) o suelta (false) el cuerpo; cada sujeción se suelta con su propia llamada. Sustituye a
    /// desactivar el componente, que bloqueaba también la cámara.</summary>
    public void HoldBody(bool on) => _bodyHolds = Mathf.Max(0, _bodyHolds + (on ? 1 : -1));
    /// <summary>Suelta TODAS las sujeciones (p. ej. al saltarse la iniciación con el hechizo «miembro»).</summary>
    public void ReleaseBody() => _bodyHolds = 0;

    /// <summary>¿Nadando? (agua más honda que el pecho; lo decide la profundidad en cada frame, ver HandleMovement)</summary>
    public bool IsSwimming => _isSwimming;
    /// <summary>Vadeando: de pie en agua que cubre menos que el pecho (anda más despacio y sin saltar). 0 = fuera del agua.</summary>
    public float WadeDepth { get; private set; }
    /// <summary>Entrada simulada para las pruebas: x = lateral (A/D), y = arriba/abajo (Espacio/Ctrl), z = adelante (W/S).
    /// Mientras no es null sustituye al teclado en el movimiento.</summary>
    [System.NonSerialized] public Vector3? scriptedInput;
    SwimBody _swim;

    public void SetSwimming(bool swimming)
    {
        _isSwimming = swimming;
        if (swimming) _verticalVelocity = 0f; // don't carry fall speed into the water
        if (_swim == null) _swim = SwimBody.Of(this);
        if (swimming) _swim.Begin(); else _swim.End();
    }

    float InH => scriptedInput.HasValue ? scriptedInput.Value.x : GameKeys.GetAxisRaw("Horizontal");
    float InV => scriptedInput.HasValue ? scriptedInput.Value.z : GameKeys.GetAxisRaw("Vertical");
    float InUpDown => scriptedInput.HasValue ? scriptedInput.Value.y : (GameKeys.GetKey(jumpKey) ? 1f : 0f) - (GameKeys.GetKey(swimDownKey) ? 1f : 0f);
    /// <summary>Hacia dónde QUIERE moverse ahora (mundo; de su entrada, aunque una pared lo frene): lo miran las puertas del
    /// Árbol de los Campos para sacarlo del agua (<see cref="TreeDoorExit"/>).</summary>
    public Vector3 MoveIntent => transform.right * InH + transform.forward * InV + Vector3.up * InUpDown;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _walk = GetComponent<WalkSpell>();
        _swim = SwimBody.Of(this);
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
        YogaCamera();
        HandlePerspectiveToggle();   // la V también con el cuerpo sujeto (alfombra, yoga, empujes…): antes se quedaba muerta
        if (BodyHeld) return;
        HandleMovement();
    }

    // ── Yoga en 3ª persona ────────────────────────────────────────────────────

    // Al hacer yoga (una clase o una práctica de la alfombra) la cámara pasa a 3ª persona para ver el cuerpo; al acabar
    // vuelve a la preferida (Berón 2026-10-27). La V sigue cambiándola durante la práctica.
    bool _inYoga, _inLesson; float _lessonCheckAt;
    ActionQueue _queue; Anima _anima;
    void YogaCamera()
    {
        if (_queue == null) TryGetComponent(out _queue);
        if (_anima == null) TryGetComponent(out _anima);
        if (Time.time >= _lessonCheckAt) { _lessonCheckAt = Time.time + 0.5f; _inLesson = _anima != null && TeachingSession.For(_anima) != null; }   // busca en la escena: cada medio segundo
        bool now = (_queue != null && ((_queue.Player.Count > 0 && _queue.Player[0] is AsanaPoseSpell) || (_queue.Own.Count > 0 && _queue.Own[0] is AsanaPoseSpell)))
                   || _inLesson;
        if (now == _inYoga || CameraManager.Instance == null) return;
        _inYoga = now;
        if (!now && _anima != null) DailyPoses.Stand().ApplyFinal(_anima);   // acabó: se levanta (el upa-yoga de cuello termina sentado y Kushal seguía así al empujar en el río)
        // Al empezar se recuerda la cámara de antes (SwitchTo cambia la preferida) y al acabar se vuelve a ella, salvo que el
        // jugador la haya cambiado con la V durante la práctica: entonces se queda la suya.
        if (now) { _modeBeforeYoga = CameraManager.Instance.preferredMode; CameraManager.Instance.SwitchTo(CameraMode.ThirdPerson, forced: true); }
        else CameraManager.Instance.SwitchTo(CameraManager.Instance.CurrentMode == CameraMode.ThirdPerson ? _modeBeforeYoga : CameraManager.Instance.CurrentMode, forced: true);
    }
    CameraMode _modeBeforeYoga = CameraMode.ThirdPerson;

    // ── Look ──────────────────────────────────────────────────────────────────

    void HandleLook()
    {
        // En un diálogo o con el cuerpo sujeto (escalera, alfombra…) la cámara sigue libre, pero gira SOLO la cámara: el
        // cuerpo mantiene la orientación que le da la mecánica (de cara a la escalera, al que habla…).
        bool cameraOnly = BodyHeld || (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying);
        if (!cameraOnly && !Mathf.Approximately(_pivotYaw, 0f))
        {
            // Al soltarse, el cuerpo pasa a mirar hacia donde miraba la cámara (sin salto visible).
            transform.Rotate(Vector3.up, _pivotYaw, Space.World);
            _pivotYaw = 0f; ApplyPivot();
        }
        // Block look while typing a virtualization action (las letras no deben mover la cámara).
        if (TypingChallenge.Active) return;
        // Ceder el look libre cuando la cabeza restringida (modo 1ª persona de estación) está activa.
        if (HeadLook.Active) return;
        // Durante el yoga (upa-yoga), IJKL/ratón los consume la sesión: no mover la cámara.
        if (UpaYogaSession.Active) return;

        float yaw   = 0f;
        float pitch = 0f;

        // Mouse look only while cameraLookButton is held (see HandleCursorToggle) — the
        // cursor gets locked for the duration of the hold so the delta doesn't run out at
        // the screen edge, then released so the other button stays free for UI/interact.
        if (GameKeys.GetKey(cameraLookButton))
        {
            yaw   += GameKeys.GetAxis("Mouse X") * mouseSensitivityX;
            pitch -= GameKeys.GetAxis("Mouse Y") * mouseSensitivityY;
        }

        // Keyboard look (arrow keys — additive with mouse)
        if (enableKeyboardLook)
        {
            if (GameKeys.GetKey(lookRightKey)) yaw   += keyboardYawSpeed   * Time.deltaTime;
            if (GameKeys.GetKey(lookLeftKey))  yaw   -= keyboardYawSpeed   * Time.deltaTime;
            if (GameKeys.GetKey(lookDownKey))  pitch += keyboardPitchSpeed  * Time.deltaTime;
            if (GameKeys.GetKey(lookUpKey))    pitch -= keyboardPitchSpeed  * Time.deltaTime;
        }

        // Apply yaw to the player body (or only to the camera while the body is held)
        if (!Mathf.Approximately(yaw, 0f))
        {
            if (cameraOnly && cameraPivot != null)
            {
                _pivotYaw = Mathf.Repeat(_pivotYaw + yaw + 180f, 360f) - 180f;
                if (BodyHeld) _pivotYaw = Mathf.Clamp(_pivotYaw, -(EyeYaw + HeadYaw), EyeYaw + HeadYaw);   // ojos + cuello, no más
                ApplyPivot();
            }
            else transform.Rotate(Vector3.up, yaw, Space.World);
        }

        // Apply pitch to the camera pivot only
        if (cameraPivot != null && !Mathf.Approximately(pitch, 0f))
        {
            _currentPitch = Mathf.Clamp(_currentPitch + pitch, -maxPitchUp, maxPitchDown);
            ApplyPivot();
        }
    }

    // ── Ojos y cabeza con el cuerpo sujeto ────────────────────────────────────
    // Sujeto (sentado en el examen, tumbado en la camilla, en una alfombra…), la cámara son primero los OJOS (±35° a los
    // lados, ±25° arriba y abajo) y, más allá, gira la CABEZA del cuerpo de cubos (hasta ±70° y ±40° más), como una persona.
    const float EyeYaw = 35f, EyePitch = 25f, HeadYaw = 70f, HeadPitch = 40f;
    Transform _head; Quaternion _headBase, _headSet; bool _headTurned; CreatureRig _rig;

    void LateUpdate()
    {
        if (_head == null)
            foreach (CubeJoint j in GetComponentsInChildren<CubeJoint>(true)) if (j.part == BodyPart.Head) { _head = j.transform; break; }
        if (_head == null) return;
        if (!_headTurned || _head.localRotation != _headSet) _headBase = _head.localRotation;   // una postura nueva manda
        // Si una postura mueve ahora la cabeza (upa-yoga de cuello: barbilla al pecho, oreja al hombro…), manda la postura: la
        // mirada no le suma su giro (en la clase, sujeto, la cabeza no seguía las instrucciones; Berón 2026-10-29).
        if (_rig == null) TryGetComponent(out _rig);
        bool poseHead = _rig != null && (_rig.IsPoseLocked(BodyPart.Head) || _rig.IsPoseLocked(BodyPart.Neck));
        if (poseHead) { _headTurned = false; return; }
        float y = BodyHeld ? Beyond(_pivotYaw, EyeYaw, HeadYaw) : 0f;
        float x = BodyHeld ? Beyond(_currentPitch, EyePitch, HeadPitch) : 0f;
        if (Mathf.Approximately(x, 0f) && Mathf.Approximately(y, 0f))
        {
            if (_headTurned) { _head.localRotation = _headBase; _headTurned = false; }
            return;
        }
        _head.localRotation = _headBase * Quaternion.Euler(x, y, 0f);
        _headSet = _head.localRotation; _headTurned = true;
    }

    // Lo que pasa de lo que alcanzan los ojos, hasta lo que da el cuello.
    static float Beyond(float a, float eyes, float head) => Mathf.Sign(a) * Mathf.Min(Mathf.Max(0f, Mathf.Abs(a) - eyes), head);

    void ApplyPivot()
    {
        if (cameraPivot != null) cameraPivot.localRotation = Quaternion.Euler(_currentPitch, _pivotYaw, 0f);
    }

    /// <summary>Gira cuerpo (yaw) y cámara (pitch) por código: lo usan los hechizos `look`/`lookAt` (apuntar sin ratón).</summary>
    public void Look(float yawDelta, float pitchDelta)
    {
        transform.Rotate(Vector3.up, yawDelta, Space.World);
        if (cameraPivot != null)
        {
            _currentPitch = Mathf.Clamp(_currentPitch + pitchDelta, -maxPitchUp, maxPitchDown);
            ApplyPivot();
        }
    }

    /// <summary>Mira hacia un punto del mundo (cuerpo hacia su horizontal, cámara hacia su altura).</summary>
    public void LookAtPoint(Vector3 p)
    {
        Vector3 to = p - transform.position; Vector3 flat = new Vector3(to.x, 0f, to.z);
        if (flat.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(flat);
        if (cameraPivot != null)
        {
            Vector3 fromCam = p - cameraPivot.position;
            float pitch = -Mathf.Atan2(fromCam.y, new Vector2(fromCam.x, fromCam.z).magnitude) * Mathf.Rad2Deg;
            _currentPitch = Mathf.Clamp(pitch, -maxPitchUp, maxPitchDown);
            _pivotYaw = 0f; ApplyPivot();
        }
    }

    // ── Movement ──────────────────────────────────────────────────────────────

    void HandleMovement()
    {
        // Durante el yoga, WASD las consume la sesión (control por partes): el cuerpo no camina.
        if (UpaYogaSession.Active) return;
        // NADAR O VADEAR SEGÚN LA PROFUNDIDAD (Berón 2026-10-29: en el mar Kushal «caminaba o daba saltitos»). Antes lo
        // decidían el borde del trigger y «¿hay suelo a menos de 2,2 m?», y alternaba entre andar y flotar de pie. Ahora, en
        // cada frame y por POSICIÓN (así un teletransporte tampoco lo deja «nadando fantasma»): agua más honda que el pecho →
        // nada (cuerpo horizontal, brazadas); menos → vadea de pie, más despacio y sin saltar.
        SwimBody.Water water = SwimBody.Probe(transform, transform.position, _swim.PivotToFeet, _swim.BodyHeight);
        bool swim = _swim.ShouldSwim(water);
        if (swim != _isSwimming) SetSwimming(swim);
        WadeDepth = water.inWater && !swim ? Mathf.Min(water.feetDepth, water.depth) : 0f;
        if (_isSwimming)
        {
            HandleSwimMovement();
            return;
        }

        // Horizontal (dirección cámara-relativa; la velocidad la da el WalkSpell si está — locomoción universal).
        float h = InH;
        float v = InV;
        Vector3 move = Vector3.ClampMagnitude(transform.right * h + transform.forward * v, 1f);
        float speed;
        if (_walk != null)
        {
            bool charging   = GameKeys.GetKey(_walk.chargeKey);     // LeftShift parado = postura de salida
            bool channeling = GameKeys.GetKey(_walk.channelKey);    // RightShift = subir a la punta
            speed = _walk.StepSpeed(charging, channeling, move.sqrMagnitude > 0.01f, Time.deltaTime);
            if (_walk.IsCharging) move = Vector3.zero;            // cargando: quieto tomando postura (con gravedad)
        }
        else speed = GameKeys.GetKey(sprintKey) ? sprintSpeed : walkSpeed;
        // Vadeando: el agua frena (hasta menos de la mitad con el agua al pecho) y no se salta.
        if (WadeDepth > 0f) speed *= Mathf.Lerp(1f, 0.45f, Mathf.Clamp01(WadeDepth / Mathf.Max(0.1f, water.chest)));

        // Vertical / gravity
        if (_cc.isGrounded)
        {
            _verticalVelocity = -2f; // keep grounded
            if (WadeDepth < 0.3f && GameKeys.GetKeyDown(jumpKey))
                _verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
        }
        else
        {
            _verticalVelocity = Mathf.Max(_verticalVelocity - gravity * Time.deltaTime, -50f);   // velocidad terminal (antes caía sin límite)
        }

        Vector3 velocity = move * speed;
        velocity.y = _verticalVelocity;
        _cc.Move(velocity * Time.deltaTime);
    }

    // Free 3D movement, no gravity — WASD for the horizontal plane, jumpKey/swimDownKey for
    // up/down. Reuses the same left-hand keys as on land (jumpKey doubles as "swim up") so
    // there's nothing new to learn entering the water.
    // SALIR DEL AGUA: nadando hacia el borde de un casquete (o una orilla) que asoma poco, se sube a él (como apoyar las
    // manos y trepar). Sin esto el CharacterController chocaba con el canto del hielo y se quedaba en el agua.
    void TryClimbOut(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;
        dir.Normalize();
        Vector3 ahead = transform.position + dir * (_cc.radius + 0.45f);
        if (!Physics.Raycast(ahead + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 4f, ~0, QueryTriggerInteraction.Ignore)) return;
        float feet = transform.position.y + _cc.center.y - _cc.height * 0.5f;
        // Nadando horizontal, los pies quedan hondos: lo que cuenta es lo que asoma el borde sobre el agua (las manos en el
        // borde y un empujón), no desde los pies.
        if (!float.IsNaN(_swim.Surface)) feet = Mathf.Max(feet, _swim.Surface - 0.45f);
        float climb = hit.point.y - feet;
        if (climb < 0.1f || climb > 1.2f) return;   // nada que subir, o una pared demasiado alta
        if (!_swim.HeadAboveWater) return;           // buceando bajo el borde: no se sale a través del hielo
        // Ni a través de un TECHO o una pared (Berón 2026-10-31: en las ramas del Árbol de los Campos, flotando contra el techo del
        // tubo, el rayo desde 2 m más arriba daba en la cara de ARRIBA del tubo y lo subía encima: «salía por las paredes»). Tiene
        // que poder subir en vertical hasta encima del borde y pasar de ahí, en horizontal, a donde se pone de pie.
        float standY = hit.point.y + 1.2f;
        Vector3 rise = new Vector3(transform.position.x, standY, transform.position.z), over = new Vector3(ahead.x, standY, ahead.z);
        if (Blocked(transform.position, rise) || Blocked(rise, over)) return;
        _cc.enabled = false;
        transform.position = new Vector3(ahead.x, hit.point.y + (_cc.height * 0.5f - _cc.center.y) + 0.05f, ahead.z);
        _cc.enabled = true;
        SetSwimming(false);
        _verticalVelocity = -2f;
    }

    // ¿Hay algo sólido (no él mismo, no un trigger) entre a y b?
    bool Blocked(Vector3 a, Vector3 b)
    {
        Vector3 d = b - a; float len = d.magnitude;
        if (len < 1e-3f) return false;
        foreach (RaycastHit h in Physics.RaycastAll(a, d / len, len, ~0, QueryTriggerInteraction.Ignore))
            if (!h.transform.IsChildOf(transform)) return true;
        return false;
    }

    // NADAR: WASD en el plano y ADELANTE SIGUE LA MIRADA (con la cámara inclinada más de 20°, W nada hacia arriba o de
    // cabeza hacia abajo); Espacio sube y swimDownKey (Ctrl) BUCEA. Sin subir ni bajar, el cuerpo flota boca abajo en la
    // superficie (o, bajo el agua, sube despacio). El cuerpo de nado (SwimBody) mueve, respira y da las brazadas.
    void HandleSwimMovement()
    {
        float h = InH, v = InV;
        float look = Mathf.Abs(_currentPitch) > 20f ? (_currentPitch - Mathf.Sign(_currentPitch) * 20f) * Mathf.Deg2Rad : 0f;
        Vector3 fwd = transform.forward * Mathf.Cos(look) + Vector3.down * Mathf.Sin(look);
        Vector3 move = Vector3.ClampMagnitude(transform.right * h + fwd * v, 1f);
        float speed = swimSpeed * (!scriptedInput.HasValue && GameKeys.GetKey(sprintKey) ? 1.5f : 1f);
        Vector3 velocity = move * speed;
        velocity.y += InUpDown * swimVerticalSpeed;
        if (Mathf.Abs(velocity.y) < 0.3f) velocity.y = 0f;   // una inclinación mínima no hunde: flota
        _swim.Drive(velocity, Time.deltaTime);
        _verticalVelocity = 0f;
        Vector3 flat = new Vector3(move.x, 0f, move.z);
        TryClimbOut(flat.sqrMagnitude > 0.01f ? flat : (_walk != null ? _walk.LastDir : Vector3.zero));
        if (!_isSwimming) _swim.End();
    }

    // ── Perspective toggle ────────────────────────────────────────────────────

    void HandlePerspectiveToggle()
    {
        if (!GameKeys.GetKeyDown(perspectiveToggleKey)) return;
        // V es también "jugar con criatura": junto a una, la tecla es del juego, no de la cámara (antes giraba la vista a cada toque).
        if (TryGetComponent(out PlayController play) && play.playKey == perspectiveToggleKey && play.HasTargetInRange) return;
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
        if (GameKeys.GetKeyDown(cameraLookButton))
            SetCursorLocked(true);
        else if (GameKeys.GetKeyUp(cameraLookButton))
            SetCursorLocked(false);
        else if (GameKeys.GetKeyDown(KeyCode.Escape))
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
