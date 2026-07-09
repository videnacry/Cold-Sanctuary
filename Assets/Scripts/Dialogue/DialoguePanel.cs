using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The on-screen UI for dialogue. Displays one DialogueLine at a time.
///
/// Expected Canvas hierarchy (set up in the Inspector):
///
///   [DialoguePanel]         ← this MonoBehaviour + CanvasGroup
///     ├─ Background         ← semi-transparent dark bar at screen bottom
///     ├─ Portrait           ← Image — speaker portrait (left or right side)
///     ├─ TextBlock
///     │    ├─ SpeakerName   ← TMP_Text — character name (bold, accent colour)
///     │    └─ BodyText      ← TMP_Text — the actual line (typewriter animated)
///     ├─ FlashOverlay       ← Image (white, full-screen) for Flash effect
///     └─ DarkenOverlay      ← Image (black) for Darken effect
///
/// DialogueManager is the owner — it calls ShowLine(), CompleteTyping(), Show(), Hide().
/// This component handles only visuals.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class DialoguePanel : MonoBehaviour
{
    // ── Inspector references ──────────────────────────────────────────────────

    [Header("Text")]
    [SerializeField] TMP_Text _speakerNameText;
    [SerializeField] TMP_Text _bodyText;

    [Header("Portrait")]
    [SerializeField] Image _portrait;
    [SerializeField] RectTransform _portraitRect;

    [Header("Overlays")]
    [SerializeField] Image _flashOverlay;
    [SerializeField] Image _darkenOverlay;

    [Header("Panel fade")]
    [Tooltip("Time in seconds to fade the panel in/out.")]
    [Min(0f)] public float fadeDuration = 0.2f;

    // ── State ─────────────────────────────────────────────────────────────────

    /// <summary>True while the typewriter coroutine is revealing characters.</summary>
    public bool IsTyping { get; private set; }

    CanvasGroup _canvasGroup;
    Coroutine   _typewriterRoutine;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha          = 0f;
        _canvasGroup.interactable   = false;
        _canvasGroup.blocksRaycasts = false;

        if (_flashOverlay  != null) _flashOverlay.gameObject.SetActive(false);
        if (_darkenOverlay != null) _darkenOverlay.gameObject.SetActive(false);
    }

    // ── Public API (called by DialogueManager) ────────────────────────────────

    /// <summary>Fade the panel in. Call before the first line of a sequence.</summary>
    public void Show()
    {
        StopAllCoroutines();
        StartCoroutine(FadeCanvasGroup(0f, 1f, fadeDuration));
        _canvasGroup.interactable   = true;
        _canvasGroup.blocksRaycasts = true;
    }

    /// <summary>Fade the panel out. Call after the last line of a sequence.</summary>
    public void Hide()
    {
        StopAllCoroutines();
        IsTyping = false;
        SetFlashOverlay(false);
        SetDarkenOverlay(false, 0f);
        StartCoroutine(FadeCanvasGroup(1f, 0f, fadeDuration));
        _canvasGroup.interactable   = false;
        _canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// Display a new line. Starts the typewriter animation.
    /// DialogueManager waits on IsTyping before advancing.
    /// </summary>
    public void ShowLine(DialogueLine line)
    {
        if (line == null) return;

        // Clear Darken from previous line
        SetDarkenOverlay(false, 0f);

        // Speaker name
        if (_speakerNameText != null)
            _speakerNameText.text = line.speakerName;

        // Portrait
        if (_portrait != null)
        {
            _portrait.sprite  = line.portrait;
            _portrait.enabled = line.portrait != null;

            // Mirror the portrait rect if speaker is on the right
            if (_portraitRect != null)
            {
                float sign = line.side == DialoguePanelSide.Right ? -1f : 1f;
                Vector3 s = _portraitRect.localScale;
                s.x = Mathf.Abs(s.x) * sign;
                _portraitRect.localScale = s;
            }
        }

        // Start typewriter
        if (_typewriterRoutine != null)
            StopCoroutine(_typewriterRoutine);
        _typewriterRoutine = StartCoroutine(TypewriterRoutine(line.text, line.typeSpeed));
    }

    /// <summary>
    /// Instantly finish revealing the current line.
    /// Called when the player presses advance during typewriter animation.
    /// </summary>
    public void CompleteTyping()
    {
        if (_typewriterRoutine != null)
        {
            StopCoroutine(_typewriterRoutine);
            _typewriterRoutine = null;
        }
        // Show all text immediately
        if (_bodyText != null)
        {
            _bodyText.maxVisibleCharacters = int.MaxValue;
        }
        IsTyping = false;
    }

    // ── Screen effect helpers (called by DialogueManager) ─────────────────────

    public void SetFlashOverlay(bool active)
    {
        if (_flashOverlay != null)
            _flashOverlay.gameObject.SetActive(active);
    }

    public void SetDarkenOverlay(bool active, float alpha)
    {
        if (_darkenOverlay == null) return;
        _darkenOverlay.gameObject.SetActive(active);
        if (active)
        {
            Color c = _darkenOverlay.color;
            c.a = 1f - alpha; // alpha parameter is "how dark": 0 = black, 1 = transparent
            _darkenOverlay.color = c;
        }
    }

    // ── Typewriter coroutine ──────────────────────────────────────────────────

    IEnumerator TypewriterRoutine(string text, float typeSpeed)
    {
        IsTyping = true;

        if (_bodyText != null)
        {
            _bodyText.text               = text;
            _bodyText.maxVisibleCharacters = 0;
        }

        int totalChars = text.Length;
        float charsRevealed = 0f;

        while (charsRevealed < totalChars)
        {
            charsRevealed += typeSpeed * Time.deltaTime;
            if (_bodyText != null)
                _bodyText.maxVisibleCharacters = Mathf.FloorToInt(charsRevealed);
            yield return null;
        }

        if (_bodyText != null)
            _bodyText.maxVisibleCharacters = int.MaxValue;

        IsTyping = false;
    }

    // ── Canvas fade ───────────────────────────────────────────────────────────

    IEnumerator FadeCanvasGroup(float from, float to, float duration)
    {
        float elapsed = 0f;
        _canvasGroup.alpha = from;
        while (elapsed < duration)
        {
            _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _canvasGroup.alpha = to;
    }
}
