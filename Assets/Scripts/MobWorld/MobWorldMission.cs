using System.Collections;
using UnityEngine;

/// <summary>
/// Drives a MobMission that lives inside a mob-world SCENE (docs mob-world-architecture §13).
///
/// In scene mode there is no MeditationSession / RealityShiftController: the player is brought in by
/// MobWorldLoader (fade → additive load → teleport to MobSpawnPoint → fade in) and the scene itself
/// IS the small world. This component waits until that entry has finished and then begins the
/// mission's spawn loop (MobMission.RaiseBegin), so mobs appear around the player once they're placed.
///
/// The mission it drives should have its MeditationMissionBase.endMode = Standalone so completion
/// cleans up in place instead of calling MeditationSession.EndMission. The player leaves via the
/// YogaPortal in the scene.
///
/// Also works when the scene is played directly in the editor (no MobWorldLoader): it just waits for
/// a tagged Player to exist, then begins.
/// </summary>
[RequireComponent(typeof(MobMission))]
public class MobWorldMission : MonoBehaviour
{
    MobMission _mission;
    bool       _begun;

    void Awake()
    {
        _mission = GetComponent<MobMission>();
    }

    void OnEnable()
    {
        StartCoroutine(BeginWhenEntered());
    }

    IEnumerator BeginWhenEntered()
    {
        // Wait until MobWorldLoader has finished bringing the player in (load + teleport + fade).
        while (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsBusy)
            yield return null;

        // Ensure the player exists (teleported into this scene, or already present when played directly).
        while (GameObject.FindGameObjectWithTag("Player") == null)
            yield return null;

        if (_begun) yield break;
        _begun = true;
        _mission.RaiseBegin();
    }
}
