using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orchestrates the Microcosmos entry flow shared by BOTH triggers (docs §3):
///   machine / lotus animation → fade to black → mission menu → (select) → snap shift →
///   fade from black → play mission. Ending reverses it.
///
/// The size transition is hidden entirely inside the black screen. Neither the
/// VirtualizationMachine nor the lotus ability know anything about scale — they just call Open().
///
/// Singleton with auto-bootstrap so it works without SampleSceneBuilder wiring.
/// </summary>
public class MeditationSession : MonoBehaviour
{
    static MeditationSession _instance;
    public static MeditationSession Instance => _instance != null ? _instance : Bootstrap();

    /// <summary>True from the moment a mission begins until it ends (world is transformed).</summary>
    public bool IsInMission => _current != null;

    /// <summary>True while opening/selecting or shifting — used to block re-entry.</summary>
    public bool IsBusy { get; private set; }

    MobMission              _current;
    RealityShiftController  _currentShift;
    Action                  _exitAnimation;

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
    }

    /// <summary>
    /// Begin the entry flow. <paramref name="enterAnimation"/> plays the machine/lotus animation
    /// (e.g. sitting in lotus, entering the machine + closing eyes); <paramref name="exitAnimation"/>
    /// (optional) plays if the player backs out of the menu without picking a mission.
    /// </summary>
    public void Open(IEnumerable<MobMission> missions,
                     RealityShiftController shift,
                     Action enterAnimation = null,
                     Action exitAnimation  = null)
    {
        if (IsBusy || IsInMission) return;
        if (shift == null) { Debug.LogWarning("[Meditation] Sin RealityShiftController; abortado."); return; }

        IsBusy = true;
        _currentShift  = shift;
        _exitAnimation = exitAnimation;

        enterAnimation?.Invoke();

        // Snapshot the list (in case scene changes mid-flow).
        var list = new List<MobMission>(missions);

        ScreenFader.Get().FadeToBlack(() =>
        {
            MissionSelectMenu.Get().Show(
                list,
                onSelected: Begin,
                onCancel:   AbortToWorld);
        });
    }

    // ── Mission lifecycle ───────────────────────────────────────────────────────

    void Begin(MobMission mission)
    {
        // Shift happens while still black, then we fade back into the transformed world.
        _currentShift.ShiftIn(mission);
        _current = mission;
        ScreenFader.Get().FadeFromBlack(() => IsBusy = false);
    }

    /// <summary>
    /// End the active mission: fade to black, snap back to normal size, fade in.
    /// Call this from mission-completion logic (e.g. MissionTracker) or an "exit" interaction.
    /// </summary>
    public void EndMission()
    {
        if (!IsInMission || IsBusy) return;
        IsBusy = true;

        MobMission mission = _current;
        RealityShiftController shift = _currentShift;
        _current = null;

        ScreenFader.Get().FadeToBlack(() =>
        {
            shift.ShiftOut(mission);
            ScreenFader.Get().FadeFromBlack(() =>
            {
                IsBusy = false;
                _currentShift = null;
            });
        });
    }

    // ── Cancel from the menu (no mission chosen) ─────────────────────────────────

    void AbortToWorld()
    {
        Action anim = _exitAnimation;
        _exitAnimation = null;
        _currentShift  = null;
        ScreenFader.Get().FadeFromBlack(() =>
        {
            IsBusy = false;
            anim?.Invoke();
        });
    }

    // ── Auto-bootstrap ──────────────────────────────────────────────────────────

    static MeditationSession Bootstrap()
    {
        var go = new GameObject("MeditationSession (auto)");
        _instance = go.AddComponent<MeditationSession>();
        return _instance;
    }
}
