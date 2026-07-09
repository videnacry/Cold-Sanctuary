using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the miniaturization effect when the player enters the kitchen zone.
///
/// When triggered:
///   1. The kitchen environment scales UP (or the player + camera scale DOWN).
///   2. A cinematic transition plays (fade or shader distortion).
///   3. IngredientMob GameObjects are activated.
///   4. On exit, the reverse plays.
///
/// Implementation approach: scale the Kitchen root GameObject up rather than
/// scaling the player down — avoids breaking character controller / physics.
/// The visual result is identical: the player appears tiny inside the kitchen.
///
/// Usage:
///   - Attach to a trigger collider at the kitchen entrance.
///   - Assign kitchenRoot (the parent of all kitchen geometry + mobs).
///   - Assign playerCamera for FOV adjustment during the transition.
/// </summary>
public class KitchenScaleController : MonoBehaviour
{
    // ── References ────────────────────────────────────────────────────────────

    [Header("References")]
    [Tooltip("Root GameObject containing all kitchen geometry and IngredientMobs. " +
             "This is what gets scaled up to create the miniaturization illusion.")]
    public Transform kitchenRoot;

    [Tooltip("Player camera — FOV is adjusted during transition.")]
    public Camera playerCamera;

    // ── Scale settings ────────────────────────────────────────────────────────

    [Header("Scale")]
    [Tooltip("How many times larger the kitchen appears when miniaturized. " +
             "8 makes the player feel ant-sized inside a normal kitchen.")]
    public float miniaturizationScale = 8f;

    [Tooltip("Duration of the scale transition in seconds.")]
    public float transitionDuration = 1.5f;

    // ── FOV ───────────────────────────────────────────────────────────────────

    [Header("Camera FOV")]
    [Tooltip("Normal FOV outside the kitchen.")]
    public float normalFOV  = 70f;

    [Tooltip("FOV when miniaturized (slightly wider for the 'ant perspective').")]
    public float miniatureFOV = 85f;

    // ── Mob activation ────────────────────────────────────────────────────────

    [Header("Mobs")]
    [Tooltip("IngredientMob GameObjects to activate when the player enters. " +
             "They should be inactive in the scene by default.")]
    public IngredientMob[] ingredientMobs;

    // ── Runtime ───────────────────────────────────────────────────────────────

    bool    _isMiniaturized;
    Vector3 _originalKitchenScale;

    /// <summary>True after Miniaturize() completes, false after Restore() completes.</summary>
    public bool IsMiniaturized => _isMiniaturized;

    /// <summary>Enter the kitchen (miniaturize + activate mobs). Called by KitchenEntrance.</summary>
    public void EnterKitchen()
    {
        if (!_isMiniaturized) StartCoroutine(Miniaturize());
    }

    /// <summary>Exit the kitchen (restore scale + deactivate mobs). Called by KitchenEntrance.</summary>
    public void ExitKitchen()
    {
        if (_isMiniaturized) StartCoroutine(Restore());
    }

    void Awake()
    {
        if (kitchenRoot != null)
            _originalKitchenScale = kitchenRoot.localScale;

        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    // ── Trigger ───────────────────────────────────────────────────────────────

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!_isMiniaturized)
            StartCoroutine(Miniaturize());
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_isMiniaturized)
            StartCoroutine(Restore());
    }

    // ── Transitions ───────────────────────────────────────────────────────────

    IEnumerator Miniaturize()
    {
        _isMiniaturized = true;

        // Activate mobs before transition starts (they appear as kitchen scales up)
        foreach (var mob in ingredientMobs)
            if (mob != null) mob.gameObject.SetActive(true);

        // TODO: trigger CameraManager.Instance?.RequestRobbery() for cinematic cut

        yield return ScaleTransition(
            fromScale: _originalKitchenScale,
            toScale:   _originalKitchenScale * miniaturizationScale,
            fromFOV:   normalFOV,
            toFOV:     miniatureFOV);

        Debug.Log("[Kitchen] Miniaturización completa. ¡Bienvenido al mundo de la hormiga!");
    }

    IEnumerator Restore()
    {
        yield return ScaleTransition(
            fromScale: _originalKitchenScale * miniaturizationScale,
            toScale:   _originalKitchenScale,
            fromFOV:   miniatureFOV,
            toFOV:     normalFOV);

        _isMiniaturized = false;

        // Deactivate surviving mobs
        foreach (var mob in ingredientMobs)
            if (mob != null && mob.gameObject != null)
                mob.gameObject.SetActive(false);

        Debug.Log("[Kitchen] Vuelta al tamaño normal.");
    }

    IEnumerator ScaleTransition(Vector3 fromScale, Vector3 toScale,
                                 float fromFOV, float toFOV)
    {
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);

            if (kitchenRoot != null)
                kitchenRoot.localScale = Vector3.Lerp(fromScale, toScale, t);

            if (playerCamera != null)
                playerCamera.fieldOfView = Mathf.Lerp(fromFOV, toFOV, t);

            yield return null;
        }

        // Snap to final values
        if (kitchenRoot != null)  kitchenRoot.localScale       = toScale;
        if (playerCamera != null) playerCamera.fieldOfView = toFOV;
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.2f);
        var col = GetComponent<Collider>();
        if (col != null)
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
}
