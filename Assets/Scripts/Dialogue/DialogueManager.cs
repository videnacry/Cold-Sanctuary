using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton that drives all dialogue playback in Cold Sanctuary.
///
/// Usage:
///   DialogueManager.Instance.Play(mySequence);
///
/// The manager:
///   1. Checks if the sequence has already played (if playOnce = true).
///   2. Fires the screen effect for the first line.
///   3. Hands each line to DialoguePanel, which handles typewriter + portrait.
///   4. Waits for the player to press the advance key (Space / Enter / click)
///      or for auto-advance if the line has pauseAfter set high enough.
///   5. Fires OnSequenceFinished when all lines are done.
///
/// Blocking: while a sequence plays, no new sequence can start (the new call
/// is silently ignored). Use the IsPlaying property to gate triggers.
///
/// Screen effects (Flash, Shake, Darken) are applied via coroutines on this
/// MonoBehaviour. Wire the camera reference in the Inspector for Shake.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────

    public static DialogueManager Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("References")]
    [Tooltip("The UI panel component that renders each line.")]
    public DialoguePanel panel;

    [Tooltip("Camera reference for the Shake effect. Leave blank to skip shake.")]
    public Camera mainCamera;

    [Header("Input")]
    [Tooltip("Key to advance to the next line (or skip typewriter).")]
    public KeyCode advanceKey = KeyCode.Space;

    [Tooltip("Alternative advance key.")]
    public KeyCode advanceKeyAlt = KeyCode.Return;

    [Header("Screen Effects")]
    [Tooltip("Duration of the Flash white overlay (seconds).")]
    public float flashDuration = 0.3f;

    [Tooltip("Intensity of camera shake (world units).")]
    public float shakeIntensity = 0.08f;

    [Tooltip("Duration of camera shake (seconds).")]
    public float shakeDuration = 0.4f;

    [Tooltip("How dark the screen gets for the Darken effect (0 = black, 1 = unchanged).")]
    [Range(0f, 1f)] public float darkenAlpha = 0.55f;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired when a dialogue sequence begins playing.</summary>
    public event Action<DialogueSequence> OnSequenceStarted;

    /// <summary>Fired when a dialogue sequence completes (all lines done).</summary>
    public event Action<DialogueSequence> OnSequenceFinished;

    // ── State ─────────────────────────────────────────────────────────────────

    /// <summary>True while a sequence is in progress.</summary>
    public bool IsPlaying => _isPlaying;

    bool _isPlaying;
    bool _advanceRequested;
    bool _skipTypewriterRequested;

    // Tracks sequences that have already played (survives scene reloads if DontDestroyOnLoad)
    readonly HashSet<string> _playedOnce = new HashSet<string>();

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (!_isPlaying) return;

        if (Input.GetKeyDown(advanceKey) || Input.GetKeyDown(advanceKeyAlt)
            || Input.GetMouseButtonDown(0))
        {
            if (panel != null && panel.IsTyping)
                _skipTypewriterRequested = true;
            else
                _advanceRequested = true;
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Start playing a dialogue sequence.
    /// Silently ignored if a sequence is already playing, or if this
    /// sequence has already played (when playOnce = true).
    /// </summary>
    public void Play(DialogueSequence sequence)
    {
        if (sequence == null || sequence.lines == null || sequence.lines.Length == 0) return;
        if (_isPlaying) return;
        if (sequence.playOnce && _playedOnce.Contains(sequence.sequenceId)) return;

        StartCoroutine(PlayRoutine(sequence));
    }

    /// <summary>
    /// Force-stop any running sequence and hide the panel.
    /// Use sparingly — prefer letting sequences finish naturally.
    /// </summary>
    public void Stop()
    {
        StopAllCoroutines();
        _isPlaying = false;
        _advanceRequested = false;
        _skipTypewriterRequested = false;
        panel?.Hide();
    }

    // ── Playback coroutine ────────────────────────────────────────────────────

    IEnumerator PlayRoutine(DialogueSequence sequence)
    {
        _isPlaying = true;
        _advanceRequested = false;
        _skipTypewriterRequested = false;

        if (sequence.playOnce && !string.IsNullOrEmpty(sequence.sequenceId))
            _playedOnce.Add(sequence.sequenceId);

        OnSequenceStarted?.Invoke(sequence);
        panel?.Show();

        foreach (var line in sequence.lines)
        {
            // Apply screen effect before revealing the line
            if (line.screenEffect != DialogueScreenEffect.None)
                yield return StartCoroutine(ApplyScreenEffect(line.screenEffect));

            // Hand line to the panel — typewriter begins
            _skipTypewriterRequested = false;
            panel?.ShowLine(line);

            // Wait for typewriter to finish (or player skip)
            while (panel != null && panel.IsTyping)
            {
                if (_skipTypewriterRequested)
                {
                    _skipTypewriterRequested = false;
                    panel.CompleteTyping();
                    break;
                }
                yield return null;
            }

            // Mandatory pause after line completes
            if (line.pauseAfter > 0f)
                yield return new WaitForSeconds(line.pauseAfter);

            // Wait for player to advance
            _advanceRequested = false;
            while (!_advanceRequested)
                yield return null;

            _advanceRequested = false;
        }

        panel?.Hide();
        _isPlaying = false;
        OnSequenceFinished?.Invoke(sequence);
    }

    // ── Screen effects ────────────────────────────────────────────────────────

    IEnumerator ApplyScreenEffect(DialogueScreenEffect effect)
    {
        switch (effect)
        {
            case DialogueScreenEffect.Flash:
                yield return StartCoroutine(FlashRoutine());
                break;
            case DialogueScreenEffect.Shake:
                yield return StartCoroutine(ShakeRoutine());
                break;
            case DialogueScreenEffect.Darken:
                // Darken is persistent until the next line — just set the panel overlay
                panel?.SetDarkenOverlay(true, darkenAlpha);
                break;
        }
    }

    IEnumerator FlashRoutine()
    {
        panel?.SetFlashOverlay(true);
        yield return new WaitForSeconds(flashDuration);
        panel?.SetFlashOverlay(false);
    }

    IEnumerator ShakeRoutine()
    {
        if (mainCamera == null) yield break;

        Vector3 origin = mainCamera.transform.localPosition;
        float elapsed  = 0f;

        while (elapsed < shakeDuration)
        {
            float t = elapsed / shakeDuration;
            float magnitude = shakeIntensity * (1f - t); // decays over time
            Vector3 offset = UnityEngine.Random.insideUnitSphere * magnitude;
            mainCamera.transform.localPosition = origin + offset;
            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = origin;
    }
}
