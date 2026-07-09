using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attached to the Player. Detects IInteractable objects in proximity and
/// dispatches interactions via keyboard or mouse click.
///
/// Key layout (mecanografía — both hands stay in home-row position):
///   F (left hand, near WASD) → interact with nearest/selected interactable
///   Left-click               → raycast-interact with clicked object
///
/// Block behaviour:
///   - Interaction is suspended while DialogueManager.IsPlaying or ConfirmationPanel.IsVisible.
///   - interactKey is configurable in the Inspector.
///
/// Scene setup:
///   Attach to the Player GameObject alongside PlayerController.
///   The on-screen prompt label is created automatically if not assigned.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class InteractionController : MonoBehaviour
{
    // ── Config ────────────────────────────────────────────────────────────────

    [Header("Interaction")]
    [Tooltip("Key to interact with the nearest IInteractable. F is default — left hand, close to WASD.")]
    public KeyCode interactKey = KeyCode.F;

    [Tooltip("Proximity sphere radius (metres) for detecting interactables.")]
    public float interactRange = 3f;

    [Tooltip("Max raycast distance for click interactions. Slightly longer than interactRange.")]
    public float clickRayRange = 5f;

    [Header("UI")]
    [Tooltip("Optional: screen-space Text label for the interaction prompt. " +
             "Created automatically if left null.")]
    public Text promptLabel;

    // ── Runtime ───────────────────────────────────────────────────────────────

    IInteractable _nearest;
    Camera        _cam;

    void Start()
    {
        _cam = Camera.main;
        EnsurePromptLabel();
    }

    void Update()
    {
        bool blocked = (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying)
                    || ConfirmationPanel.IsVisible;

        if (blocked)
        {
            // Hide prompt while blocked
            SetPromptVisible(false);
            return;
        }

        FindNearest();
        HandleKeyboardInteract();
        HandleClickInteract();
        UpdatePrompt();
    }

    // ── Detection ─────────────────────────────────────────────────────────────

    void FindNearest()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange);
        IInteractable best     = null;
        float         bestDist = float.MaxValue;

        foreach (Collider col in hits)
        {
            var ia = col.GetComponentInParent<IInteractable>();
            if (ia == null || !ia.CanInteract) continue;

            float d = Vector3.Distance(transform.position, col.transform.position);
            if (d < bestDist) { bestDist = d; best = ia; }
        }

        _nearest = best;
    }

    // ── Keyboard ──────────────────────────────────────────────────────────────

    void HandleKeyboardInteract()
    {
        if (!Input.GetKeyDown(interactKey)) return;
        _nearest?.Interact();
    }

    // ── Mouse click ───────────────────────────────────────────────────────────

    void HandleClickInteract()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (_cam == null) return;

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, clickRayRange)) return;

        var ia = hit.collider.GetComponentInParent<IInteractable>();
        if (ia == null || !ia.CanInteract) return;

        ia.Interact();
    }

    // ── Prompt ────────────────────────────────────────────────────────────────

    void UpdatePrompt()
    {
        if (_nearest != null && _nearest.CanInteract)
        {
            SetPromptVisible(true);
            if (promptLabel != null)
                promptLabel.text = $"[F / Clic]  {_nearest.InteractLabel}";
        }
        else
        {
            SetPromptVisible(false);
        }
    }

    void SetPromptVisible(bool visible)
    {
        if (promptLabel != null && promptLabel.gameObject.activeSelf != visible)
            promptLabel.gameObject.SetActive(visible);
    }

    // Auto-create a minimal screen-space prompt at bottom-center if none is assigned.
    void EnsurePromptLabel()
    {
        if (promptLabel != null) return;

        GameObject existing = GameObject.Find("InteractionPrompt_AUTO");
        if (existing != null)
        {
            promptLabel = existing.GetComponent<Text>();
            return;
        }

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject go = new GameObject("InteractionPrompt_AUTO",
            typeof(RectTransform), typeof(Text));
        go.transform.SetParent(canvas.transform, worldPositionStays: false);

        RectTransform rt   = go.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.08f);
        rt.anchorMax        = new Vector2(0.5f, 0.08f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = new Vector2(420f, 50f);
        rt.anchoredPosition = Vector2.zero;

        Text t     = go.GetComponent<Text>();
        t.alignment = TextAnchor.MiddleCenter;
        t.color     = Color.white;
        t.fontSize  = 18;

        go.SetActive(false);
        promptLabel = t;
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
