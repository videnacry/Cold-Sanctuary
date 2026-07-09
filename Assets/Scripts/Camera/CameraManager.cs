using System.Collections;
using UnityEngine;

/// <summary>
/// Central camera controller for Cold Sanctuary.
/// Manages 3rd/1st person switching, player preferences, camera robberies,
/// and post-processing visual effects driven by PlayerStats.
///
/// Setup in Inspector:
///   - thirdPersonAnchor: empty GameObject parented to Player, positioned behind/above
///   - firstPersonAnchor: empty GameObject parented to Player's head bone, at eye level
///   - playerStats: reference to the PlayerStats component on the Player
/// </summary>
public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    // ── Inspector references ─────────────────────────────────────────────────
    [Header("Anchors")]
    [Tooltip("Camera position for 3rd person — parent it to the Player.")]
    public Transform thirdPersonAnchor;

    [Tooltip("Camera position for 1st person — parent it to the Player's head bone.")]
    public Transform firstPersonAnchor;

    [Header("Player")]
    public PlayerStats playerStats;

    [Header("Preferences")]
    [Tooltip("The mode the player has chosen as their base.")]
    public CameraMode preferredMode = CameraMode.ThirdPerson;

    [Tooltip("Allow the game to temporarily steal the camera for cinematic moments.")]
    public bool allowCameraRobberies = true;

    [Header("Transition")]
    [Tooltip("Seconds to smoothly move between camera positions.")]
    public float transitionDuration = 0.4f;

    // ── Post-processing effect controls ──────────────────────────────────────
    [Header("Effect Thresholds")]
    public float fatigueShakeThreshold  = 0.6f;
    public float stressAberrationStart  = 0.5f;
    public float sleepinessBlackoutStart = 0.8f;

    [Header("Effect Intensities")]
    public float maxShakeAmplitude  = 0.05f;
    public float maxFOVConstrict    = 15f;   // FOV tightening under stress (Goluis)

    [Header("Field of view per mode")]
    [Tooltip("3rd person can afford a tighter FOV since the whole body is in frame.")]
    public float thirdPersonFOV = 60f;
    [Tooltip("1st person needs a wider FOV or nearby geometry reads as abnormally small/distant — 60 felt zoomed out once Kushal was rescaled to real human height.")]
    public float firstPersonFOV = 82f;
    float baseFOV => _currentMode == CameraMode.FirstPerson ? firstPersonFOV : thirdPersonFOV;

    // ── Runtime state ────────────────────────────────────────────────────────
    CameraMode   _currentMode;
    bool         _inRobbery;
    Transform    _cam;
    Coroutine    _transitionRoutine;
    Coroutine    _blackoutRoutine;
    float        _shakeOffset;

    // ── Unity lifecycle ──────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _cam = Camera.main.transform;
    }

    void Start()
    {
        ApplyMode(preferredMode, instant: true);
    }

    void Update()
    {
        if (!_inRobbery)
            ApplyStatEffects();
    }

    // Keeps the camera glued to the current anchor every frame — the anchors
    // are parented to the moving Player, but were previously only synced once
    // per mode-switch transition, so the camera never followed movement.
    void LateUpdate()
    {
        if (_inRobbery || _transitionRoutine != null) return;
        Transform anchor = _currentMode == CameraMode.FirstPerson ? firstPersonAnchor : thirdPersonAnchor;
        if (anchor == null) return;
        _cam.position = anchor.position;
        _cam.rotation = anchor.rotation;
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>Switch to a camera mode. Respects the player's preference unless forced.</summary>
    public void SwitchTo(CameraMode mode, bool forced = false)
    {
        if (_inRobbery && !forced) return;
        preferredMode = mode;
        ApplyMode(mode);
    }

    /// <summary>Request a camera robbery (cinematic steal). Ignored if player disabled them.</summary>
    public void RequestRobbery(CameraRobbery robbery)
    {
        if (!allowCameraRobberies) return;
        if (_inRobbery) return;
        StartCoroutine(ExecuteRobbery(robbery));
    }

    /// <summary>Called by zone triggers when entering/leaving the cub area.</summary>
    public void OnEnterCubArea()  => RequestRobbery(new CameraRobbery
    {
        type           = RobberyType.SwitchPerspective,
        targetMode     = CameraMode.FirstPerson,
        holdDuration   = 0f,   // hold indefinitely until OnExitCubArea
        returnOnFinish = false
    });

    public void OnExitCubArea() => SwitchTo(preferredMode, forced: true);

    // ── Mode switching ───────────────────────────────────────────────────────

    void ApplyMode(CameraMode mode, bool instant = false)
    {
        _currentMode = mode;
        Camera.main.fieldOfView = baseFOV; // instant snap — ApplyStatEffects() only nudges gradually
        Transform anchor = mode == CameraMode.FirstPerson ? firstPersonAnchor : thirdPersonAnchor;
        if (anchor == null) return;

        if (_transitionRoutine != null) StopCoroutine(_transitionRoutine);
        _transitionRoutine = instant
            ? null
            : StartCoroutine(TransitionTo(anchor));

        if (instant)
        {
            _cam.position = anchor.position;
            _cam.rotation = anchor.rotation;
        }
    }

    IEnumerator TransitionTo(Transform anchor)
    {
        float t = 0f;
        Vector3    startPos = _cam.position;
        Quaternion startRot = _cam.rotation;

        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / transitionDuration);
            _cam.position = Vector3.Lerp(startPos, anchor.position, p);
            _cam.rotation = Quaternion.Slerp(startRot, anchor.rotation, p);
            yield return null;
        }
        _cam.position = anchor.position;
        _cam.rotation = anchor.rotation;
        _transitionRoutine = null;
    }

    // ── Camera robberies ─────────────────────────────────────────────────────

    IEnumerator ExecuteRobbery(CameraRobbery robbery)
    {
        _inRobbery = true;

        switch (robbery.type)
        {
            case RobberyType.SwitchPerspective:
                ApplyMode(robbery.targetMode);
                if (robbery.holdDuration > 0f)
                    yield return new WaitForSeconds(robbery.holdDuration);
                else
                    yield return new WaitUntil(() => !_inRobbery); // held externally
                break;

            case RobberyType.OrbitTarget:
                if (robbery.orbitTarget != null)
                    yield return OrbitAround(robbery.orbitTarget, robbery.holdDuration);
                break;

            case RobberyType.CutToAndBack:
                Vector3    savedPos = _cam.position;
                Quaternion savedRot = _cam.rotation;
                if (robbery.orbitTarget != null)
                {
                    _cam.position = robbery.orbitTarget.position;
                    _cam.LookAt(robbery.orbitTarget);
                }
                yield return new WaitForSeconds(robbery.holdDuration);
                yield return TransitionBack(savedPos, savedRot);
                break;
        }

        if (robbery.returnOnFinish)
            ApplyMode(preferredMode);

        _inRobbery = false;
    }

    IEnumerator OrbitAround(Transform target, float duration)
    {
        float elapsed = 0f;
        float radius  = Vector3.Distance(_cam.position, target.position);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float angle = elapsed / duration * 360f;
            Vector3 offset = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0.3f, Mathf.Cos(angle * Mathf.Deg2Rad)) * radius;
            _cam.position = target.position + offset;
            _cam.LookAt(target);
            yield return null;
        }
    }

    IEnumerator TransitionBack(Vector3 targetPos, Quaternion targetRot)
    {
        float t = 0f;
        Vector3    startPos = _cam.position;
        Quaternion startRot = _cam.rotation;

        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / transitionDuration);
            _cam.position = Vector3.Lerp(startPos, targetPos, p);
            _cam.rotation = Quaternion.Slerp(startRot, targetRot, p);
            yield return null;
        }
    }

    // ── Stat-driven visual effects ───────────────────────────────────────────

    void ApplyStatEffects()
    {
        if (playerStats == null) return;

        // Shake from fatigue
        if (playerStats.mentalFatigue > fatigueShakeThreshold)
        {
            float intensity = (playerStats.mentalFatigue - fatigueShakeThreshold) / (1f - fatigueShakeThreshold);
            _shakeOffset = Mathf.Sin(Time.time * 20f) * maxShakeAmplitude * intensity;
            _cam.localPosition += Vector3.up * _shakeOffset;
        }

        // FOV constriction from stress (Goluis pressure)
        if (playerStats.stress > stressAberrationStart)
        {
            float t = (playerStats.stress - stressAberrationStart) / (1f - stressAberrationStart);
            Camera.main.fieldOfView = Mathf.Lerp(baseFOV, baseFOV - maxFOVConstrict, t);
        }
        else
        {
            Camera.main.fieldOfView = Mathf.MoveTowards(Camera.main.fieldOfView, baseFOV, Time.deltaTime * 10f);
        }

        // Blackout from sleepiness
        if (playerStats.sleepiness > sleepinessBlackoutStart && _blackoutRoutine == null)
            _blackoutRoutine = StartCoroutine(BlackoutPulse());
    }

    IEnumerator BlackoutPulse()
    {
        // Brief fade to black and back — driven by a fullscreen overlay (assign in a ScreenEffects component)
        // Placeholder: just logs until ScreenEffects is implemented
        Debug.Log("[CameraManager] Blackout pulse — sleepiness critical");
        yield return new WaitForSeconds(Random.Range(2f, 5f));
        _blackoutRoutine = null;
    }
}

// ── Supporting types ─────────────────────────────────────────────────────────

public enum CameraMode
{
    ThirdPerson,
    FirstPerson
}

public enum RobberyType
{
    SwitchPerspective,  // switch to a different mode and optionally return
    OrbitTarget,        // orbit around a transform (cub milestone, etc.)
    CutToAndBack        // hard cut to a position, hold, return
}

[System.Serializable]
public class CameraRobbery
{
    public RobberyType type          = RobberyType.SwitchPerspective;
    public CameraMode  targetMode    = CameraMode.FirstPerson;
    public Transform   orbitTarget;
    public float       holdDuration  = 2f;
    public bool        returnOnFinish = true;
}
