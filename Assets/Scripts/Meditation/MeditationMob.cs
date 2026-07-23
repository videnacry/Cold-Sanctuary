using System;
using UnityEngine;

/// <summary>
/// Base for mobs that live inside a meditation / Microcosmos mission (docs §8).
///
/// Deliberately independent of IngredientMob (which needs a baked NavMesh): these use simple
/// kinematic steering so a mission works in any area without NavMesh setup. As archetypes grow
/// (absorbent, posture, heal, protect…) they subclass this.
///
/// No-violence rule (memoria del oso): mobs are never "killed" — they DISSOLVE. Dissolving is a
/// resolution (attention/technique/care), not a defeat.
/// </summary>
public abstract class MeditationMob : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("World units per second. Keep below the player's speed so fleeing is viable.")]
    public float moveSpeed = 3f;

    /// <summary>Fired once when the mob dissolves (right before it is destroyed).</summary>
    public event Action<MeditationMob> OnDissolved;

    public bool IsDissolved { get; private set; }

    protected Transform player;

    protected virtual void Awake()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    /// <summary>Assign the player explicitly (spawner passes it so we don't re-search).</summary>
    public void SetPlayer(Transform t) => player = t;

    /// <summary>Resolve the mob: fire OnDissolved and remove it. Idempotent.</summary>
    protected void Dissolve()
    {
        if (IsDissolved) return;
        IsDissolved = true;
        OnDissolved?.Invoke(this);
        // TODO: dissolve VFX / sound before destroy.
        Destroy(gameObject);
    }
}
