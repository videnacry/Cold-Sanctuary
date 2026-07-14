using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen black overlay used to hide the reality-shift transition.
///
/// Design decision (docs/magic-plane-and-meditation.md §3): the size transition is NEVER
/// shown to the player. The scale/FOV snap happens behind this black screen while the mission
/// menu is shown. So this fader is the core piece of the plano-mágico entry flow.
///
/// Singleton with auto-bootstrap: if no ScreenFader exists in the scene, Get() builds a
/// self-contained overlay Canvas at runtime. This lets the meditation system be tested
/// without wiring anything in SampleSceneBuilder.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [Tooltip("Default fade duration in seconds.")]
    [Min(0f)] public float defaultDuration = 0.4f;

    /// <summary>True while the screen is fully black.</summary>
    public bool IsBlack => _group != null && _group.alpha > 0.99f;

    CanvasGroup _group;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _group = GetComponent<CanvasGroup>();
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Fade to solid black, then invoke <paramref name="onBlack"/> (once fully black).</summary>
    public void FadeToBlack(Action onBlack = null, float duration = -1f)
    {
        StopAllCoroutines();
        StartCoroutine(Fade(1f, duration < 0f ? defaultDuration : duration, onBlack));
    }

    /// <summary>Fade from black back to the game, then invoke <paramref name="onClear"/>.</summary>
    public void FadeFromBlack(Action onClear = null, float duration = -1f)
    {
        StopAllCoroutines();
        StartCoroutine(Fade(0f, duration < 0f ? defaultDuration : duration, onClear));
    }

    // ── Fade coroutine (unscaled: meditation may pause the world) ───────────────

    IEnumerator Fade(float target, float duration, Action onDone)
    {
        _group.blocksRaycasts = true; // block input during the transition
        float start = _group.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _group.alpha = Mathf.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        _group.alpha = target;
        _group.blocksRaycasts = target > 0.5f;
        onDone?.Invoke();
    }

    // ── Auto-bootstrap ──────────────────────────────────────────────────────────

    /// <summary>Returns the ScreenFader, creating a runtime overlay Canvas if none exists.</summary>
    public static ScreenFader Get()
    {
        if (Instance != null) return Instance;

        var canvasGO = new GameObject("ScreenFader (auto)");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // above gameplay UI, below the mission menu
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var image = canvasGO.AddComponent<Image>();
        image.color = Color.black;
        var rt = image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Instance = canvasGO.AddComponent<ScreenFader>();
        return Instance;
    }
}
