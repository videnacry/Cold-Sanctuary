using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Category of mental practice a mission belongs to.
/// Mirrors docs/magic-plane-and-meditation.md §7 (visualization is a family, not one mission)
/// and §9 (each technique is its own category of mission).
/// </summary>
public enum MissionCategory
{
    Visualization,   // visualizar postura, elemento, hechizo, curar a un compi/animal/sí mismo…
    BodyScan,        // observar el cuerpo
    Mirror,          // el espejo (mob que copia al jugador)
    NoThinking,      // no pensar en nada / observar sin reaccionar
    RootInquiry,     // buscar la raíz
    TimeSorting,     // categorizar pasado / futuro / ahora
    LovingKindness,  // bondad amorosa (metta)
    Breath,          // respiración como ancla
    Other
}

/// <summary>
/// One selectable mission inside the Microcosmos (shown in the MissionSelectMenu).
///
/// Attach to a GameObject per mission within an area. The mission owns its own mob set
/// (a parent GameObject holding the mobs) which is activated on begin and deactivated on end.
/// The size transition itself is handled by RealityShiftController behind the black screen.
/// </summary>
public class MobMission : MonoBehaviour
{
    [Header("Identity")]
    public string missionName = "Práctica";

    [TextArea(1, 3)]
    public string description;

    public MissionCategory category = MissionCategory.Visualization;

    [Header("Availability")]
    [Tooltip("If false, the mission is hidden from the menu (not yet unlocked by stats/story).")]
    public bool isAvailable = true;

    [Header("Mobs")]
    [Tooltip("Parent GameObject holding this mission's mobs. Inactive by default; " +
             "activated when the mission begins, deactivated when it ends.")]
    public GameObject mobSet;

    [Header("Scale")]
    [Tooltip("Miniaturization scale for this mission. 0 = use the RealityShiftController default. " +
             "Deeper layers (molecular, atomic…) use larger values.")]
    [Min(0f)] public float scaleOverride = 0f;

    [Header("Events")]
    [Tooltip("Fired when the player begins this mission (world already transformed, screen clearing).")]
    public UnityEvent onBegin;

    [Tooltip("Fired when the mission ends (before the world is restored).")]
    public UnityEvent onEnd;

    /// <summary>C# counterparts of onBegin/onEnd, for mission logic that wires itself in code.</summary>
    public event Action OnBegan;
    public event Action OnEnded;

    public void ActivateMobs(bool active)
    {
        if (mobSet != null) mobSet.SetActive(active);
    }

    public void RaiseBegin() { onBegin?.Invoke(); OnBegan?.Invoke(); }
    public void RaiseEnd()   { onEnd?.Invoke();   OnEnded?.Invoke(); }
}
