using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Modal yes/no confirmation panel — separate from the linear DialogueSystem.
/// Use for action gates: "¿Empezar turno en la cocina?", "¿Salir del área?", etc.
///
/// Singleton. Call ConfirmationPanel.Instance.Show(…) from anywhere.
///
/// Navigation (mecanografía-friendly — no mouse required):
///   Enter  → Confirm
///   Escape → Cancel
///   Mouse  → click Confirm/Cancel button
///
/// Keys deliberately avoid Space (jump) and F (interact) so no conflicts arise
/// while the panel is open.
///
/// SampleSceneBuilder creates and wires the UI hierarchy automatically.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ConfirmationPanel : MonoBehaviour
{
    public static ConfirmationPanel Instance  { get; private set; }

    /// <summary>True while the panel is fading in or fully visible.</summary>
    public static bool IsVisible => Instance != null && Instance._canvasGroup.alpha > 0.01f;

    // ── UI References (wired by SampleSceneBuilder or in the Inspector) ───────

    [Header("UI Elements")]
    public Text   titleText;
    public Text   messageText;
    public Button confirmButton;
    public Button cancelButton;
    public Text   confirmLabel;
    public Text   cancelLabel;

    [Header("Animation")]
    [Tooltip("Fade in/out duration in seconds.")]
    [Min(0f)] public float fadeDuration = 0.12f;

    // ── Runtime ───────────────────────────────────────────────────────────────

    CanvasGroup _canvasGroup;
    Action      _onConfirm;
    Action      _onCancel;

    void Awake()
    {
        Instance     = this;
        _canvasGroup = GetComponent<CanvasGroup>();

        _canvasGroup.alpha          = 0f;
        _canvasGroup.interactable   = false;
        _canvasGroup.blocksRaycasts = false;

        gameObject.SetActive(true); // always active — visibility controlled via alpha
    }

    void Update()
    {
        if (!IsVisible) return;

        if (Input.GetKeyDown(KeyCode.Return))
            OnConfirmClicked();
        else if (Input.GetKeyDown(KeyCode.Escape))
            OnCancelClicked();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Show the confirmation panel.
    /// </summary>
    /// <param name="title">Short header, e.g. "Cocina".</param>
    /// <param name="message">Question text, e.g. "¿Empezar turno?".</param>
    /// <param name="onConfirm">Called when the player confirms.</param>
    /// <param name="onCancel">Called when the player cancels (or presses Escape). Can be null.</param>
    /// <param name="confirmText">Label on the confirm button (default "Entrar").</param>
    /// <param name="cancelText">Label on the cancel button (default "Cancelar").</param>
    public void Show(string title, string message,
                     Action onConfirm, Action onCancel   = null,
                     string confirmText = "Entrar",
                     string cancelText  = "Cancelar")
    {
        _onConfirm = onConfirm;
        _onCancel  = onCancel;

        if (titleText    != null) titleText.text    = title;
        if (messageText  != null) messageText.text  = message;
        if (confirmLabel != null) confirmLabel.text = confirmText;
        if (cancelLabel  != null) cancelLabel.text  = cancelText;

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(OnCancelClicked);
        }

        StopAllCoroutines();
        StartCoroutine(FadeTo(1f));
    }

    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(FadeTo(0f));
    }

    // ── Button handlers ───────────────────────────────────────────────────────

    void OnConfirmClicked()
    {
        Action cb  = _onConfirm;
        _onConfirm = null;
        _onCancel  = null;
        Hide();
        cb?.Invoke();
    }

    void OnCancelClicked()
    {
        Action cb  = _onCancel;
        _onConfirm = null;
        _onCancel  = null;
        Hide();
        cb?.Invoke();
    }

    // ── Fade ─────────────────────────────────────────────────────────────────

    IEnumerator FadeTo(float target)
    {
        float start   = _canvasGroup.alpha;
        float elapsed = 0f;

        _canvasGroup.interactable   = target > 0.5f;
        _canvasGroup.blocksRaycasts = target > 0.5f;

        while (elapsed < fadeDuration)
        {
            elapsed            += Time.deltaTime;
            _canvasGroup.alpha  = Mathf.Lerp(start, target, elapsed / fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = target;
    }
}
