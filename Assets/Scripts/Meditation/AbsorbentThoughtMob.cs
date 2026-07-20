using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The "absorbent / obsessive thought" archetype (docs §8): fast, chases and STICKS to the player
/// as if to absorb them. You can't just flee — it's faster than you. The resolution is to
/// **trace its root**: follow the thread to the memory-source (marker) and, by reaching it, "see
/// through" the thought → it dissolves permanently.
///
/// Mechanic in one line: you can't outrun it; go to where it was born and it loses its grip.
/// Technique: auto-indagación / vichara.
/// </summary>
public class AbsorbentThoughtMob : MeditationMob
{
    [Header("Absorbent (chase & stick)")]
    [Tooltip("Within this distance it is 'absorbing' the player (fires onAbsorbing).")]
    [Min(0.1f)] public float stickRadius = 1.5f;

    [Tooltip("Hook fired each frame while stuck to the player (VFX / satisfaction penalty).")]
    public UnityEvent onAbsorbing;

    [Header("Root (resolve)")]
    [Tooltip("When the PLAYER gets this close to the root, the thought is seen through and dissolves.")]
    [Min(0.1f)] public float rootRadius = 2f;

    public Color markerColor = new Color(0.85f, 0.75f, 0.25f);

    Vector3      _root;
    bool         _hasRoot;
    GameObject   _marker;
    LineRenderer _thread;

    /// <summary>Set the thought's root (its memory-source) and spawn the marker the player must reach.</summary>
    public void SetRoot(Vector3 worldPos)
    {
        _root = worldPos;
        _hasRoot = true;

        _marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _marker.name = "ThoughtRoot";
        _marker.transform.position = worldPos + Vector3.up * 0.5f;
        _marker.transform.localScale = Vector3.one * 0.5f;
        var col = _marker.GetComponent<Collider>(); if (col != null) Destroy(col);
        var rend = _marker.GetComponent<Renderer>(); if (rend != null) rend.material.color = markerColor;

        // Visible thread from the thought to its root (readability). Best-effort — skipped if the
        // built-in sprite shader isn't available.
        var sh = Shader.Find("Sprites/Default");
        if (sh != null)
        {
            _thread = gameObject.AddComponent<LineRenderer>();
            _thread.material = new Material(sh);
            _thread.widthMultiplier = 0.05f;
            _thread.startColor = _thread.endColor = markerColor;
            _thread.positionCount = 2;
        }
    }

    void Update()
    {
        if (player == null) return;

        // Chase & stick (planar).
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;
        if (dist > 0.001f)
        {
            Vector3 dir = toPlayer / dist;
            transform.position += dir * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(
                transform.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);
        }
        if (dist <= stickRadius) onAbsorbing?.Invoke();

        if (!_hasRoot) return;

        if (_thread != null)
        {
            _thread.SetPosition(0, transform.position);
            _thread.SetPosition(1, _root + Vector3.up * 0.5f);
        }

        // Resolve: the player follows the thread back to the root.
        if (Vector3.Distance(player.position, _root) <= rootRadius) Dissolve();
    }

    void OnDestroy()
    {
        if (_marker != null) Destroy(_marker);
    }
}
