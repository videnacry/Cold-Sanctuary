using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Loads/unloads a mob-world scene behind the black screen and teleports the player in/out
/// (docs mob-world-architecture §13 — "escena propia por mundo mob").
///
/// Approach: **additive** scene load. The mob scene is authored around a FAR origin (see
/// MobWorldSceneBuilder), so it never overlaps the base world; we just teleport the player to the
/// mob scene's MobSpawnPoint (saving the return transform) and back on exit. The base world stays
/// loaded and keeps simulating (coherent with the FOMO "el mundo avanza sin ti"). The miniaturization
/// is now purely narrative — the fade to black hides the scene load.
///
/// Singleton, DontDestroyOnLoad. Requires ScreenFader (auto-bootstraps).
/// </summary>
public class MobWorldLoader : MonoBehaviour
{
    static MobWorldLoader _instance;
    public static bool HasInstance => _instance != null;
    public static MobWorldLoader Instance => _instance != null ? _instance : Bootstrap();

    public bool IsInMobWorld { get; private set; }
    public bool IsBusy { get; private set; }

    string     _activeScene;
    Transform  _player;
    Vector3    _returnPos;
    Quaternion _returnRot;

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Public API ──────────────────────────────────────────────────────────────

    /// <summary>Enter the mob-world scene <paramref name="sceneName"/> (must be in Build Settings).</summary>
    public void EnterMobWorld(string sceneName)
    {
        if (IsBusy || IsInMobWorld || string.IsNullOrEmpty(sceneName)) return;
        StartCoroutine(EnterRoutine(sceneName));
    }

    /// <summary>Return to the base world (called by the YogaPortal inside the mob scene).</summary>
    public void ExitMobWorld()
    {
        if (IsBusy || !IsInMobWorld) return;
        StartCoroutine(ExitRoutine());
    }

    // ── Routines ────────────────────────────────────────────────────────────────

    IEnumerator EnterRoutine(string sceneName)
    {
        IsBusy = true;
        yield return FadeToBlack();

        // Load additively.
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (op == null)
        {
            Debug.LogWarning($"[MobWorldLoader] No se pudo cargar '{sceneName}' — ¿está en Build Settings? " +
                             "Corre 'Tools → Cold Sanctuary → Build MobWorld…'.");
            ScreenFader.Get().FadeFromBlack(() => IsBusy = false);
            yield break;
        }
        while (!op.isDone) yield return null;

        // Teleport the player to the mob scene's spawn (saving the return transform).
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        _player = playerGO != null ? playerGO.transform : null;
        Transform spawn = FindSpawn(sceneName);
        if (_player != null)
        {
            _returnPos = _player.position;
            _returnRot = _player.rotation;
            if (spawn != null) TeleportPlayer(spawn.position, spawn.rotation);
            else Debug.LogWarning($"[MobWorldLoader] '{sceneName}' sin MobSpawnPoint — el jugador no se teletransportó.");
        }

        _activeScene = sceneName;
        IsInMobWorld = true;
        ScreenFader.Get().FadeFromBlack(() => IsBusy = false);
    }

    IEnumerator ExitRoutine()
    {
        IsBusy = true;
        yield return FadeToBlack();

        if (_player != null) TeleportPlayer(_returnPos, _returnRot);

        AsyncOperation op = SceneManager.UnloadSceneAsync(_activeScene);
        while (op != null && !op.isDone) yield return null;

        IsInMobWorld = false;
        _activeScene = null;
        _player = null;
        ScreenFader.Get().FadeFromBlack(() => IsBusy = false);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    IEnumerator FadeToBlack()
    {
        bool black = false;
        ScreenFader.Get().FadeToBlack(() => black = true);
        while (!black) yield return null;
    }

    void TeleportPlayer(Vector3 pos, Quaternion rot)
    {
        // Disable a CharacterController during the move so it doesn't fight the teleport.
        var cc = _player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        _player.SetPositionAndRotation(pos, rot);
        if (cc != null) cc.enabled = true;
    }

    Transform FindSpawn(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid()) return null;
        foreach (var root in scene.GetRootGameObjects())
        {
            var sp = root.GetComponentInChildren<MobSpawnPoint>(true);
            if (sp != null) return sp.transform;
        }
        return null;
    }

    static MobWorldLoader Bootstrap()
    {
        var go = new GameObject("MobWorldLoader (auto)");
        _instance = go.AddComponent<MobWorldLoader>();
        return _instance;
    }
}
