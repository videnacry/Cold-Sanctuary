using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The lotus path into the Microcosmos — the endgame alternative to the machine (docs §1, §3).
///
/// Once the player has learned lotus (yoga) AND acquired the "hechizo de misiones-mob" that
/// frees them from the machine, they can sit and meditate anywhere. Casting the spell runs the
/// SAME flow as the machine: sit-in-lotus animation → black screen → mission menu.
///
/// Attach to the player. Gate with hasMobMissionSpell (set true when the spell is learned).
/// Reuses whatever missions the player's current area exposes.
/// </summary>
public class LotusMeditationAbility : MonoBehaviour
{
    [Header("Gate")]
    [Tooltip("True once the player has learned lotus + the spell that frees them from the machine.")]
    public bool hasMobMissionSpell = false;

    [Header("Input")]
    public KeyCode meditateKey = KeyCode.M;

    [Header("References")]
    [Tooltip("Reality-shift driver of the area the player is currently in.")]
    public RealityShiftController shift;

    [Tooltip("Missions available to meditate on right now (typically the current area's).")]
    public MobMission[] missions;

    [Header("Cinematic")]
    [Tooltip("Player sitting into lotus and closing eyes. Plays before the screen goes black.")]
    public UnityEvent onSitLotus;

    void Update()
    {
        if (!Input.GetKeyDown(meditateKey)) return;
        if (!hasMobMissionSpell) return;
        if (shift == null) return;
        if (MeditationSession.Instance.IsInMission || MeditationSession.Instance.IsBusy) return;

        MeditationSession.Instance.Open(
            missions,
            shift,
            enterAnimation: () => onSitLotus?.Invoke());
    }
}
