using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The virtualization machine — the universal trigger for mob / plano-mágico missions,
/// present in every area (docs §3). Replaces the door-based KitchenEntrance flow.
///
/// Flow:
///   Player approaches → InteractionController shows the prompt.
///   Interact → ConfirmationPanel "¿Entrar a la simulación?"
///   Confirm  → onEnterAnimation (player enters machine + closes eyes) → MeditationSession.Open()
///              → black screen → mission menu → chosen mission plays.
///   Interact again while in a mission → ends it (MeditationSession.EndMission()).
/// </summary>
[RequireComponent(typeof(Collider))]
public class VirtualizationMachine : MonoBehaviour, IInteractable
{
    [Header("Area")]
    public string areaName = "esta zona";

    [Header("References")]
    [Tooltip("This area's reality-shift driver.")]
    public RealityShiftController shift;

    [Tooltip("Missions offered by this machine (only isAvailable ones are listed). Modo misión in-place.")]
    public MobMission[] missions;

    [Header("Mundo mob (escena propia)")]
    [Tooltip("Si se rellena, la máquina carga esta ESCENA de mundo mob (MobWorldLoader) en vez del " +
             "menú de misiones in-place. Debe estar en Build Settings (ver MobWorldSceneBuilder).")]
    public string mobWorldSceneName = "";

    [Header("Cinematic")]
    [Tooltip("Player entering the machine and closing eyes. Plays before the screen goes black.")]
    public UnityEvent onEnterAnimation;

    bool SceneMode => !string.IsNullOrEmpty(mobWorldSceneName);

    // ── IInteractable ─────────────────────────────────────────────────────────

    public string InteractLabel =>
        (!SceneMode && MeditationSession.Instance.IsInMission)
            ? "Salir de la simulación"
            : $"Entrar a la simulación de {areaName}";

    public bool CanInteract =>
        SceneMode
            ? (MobWorldLoader.Instance != null && !MobWorldLoader.Instance.IsBusy && !MobWorldLoader.Instance.IsInMobWorld)
            : (shift != null && !MeditationSession.Instance.IsBusy);

    public void Interact()
    {
        // Modo escena: cargar el mundo mob propio (salida vía YogaPortal dentro de la escena).
        if (SceneMode)
        {
            Confirm(() => MobWorldLoader.Instance.EnterMobWorld(mobWorldSceneName));
            return;
        }

        // Modo in-place (menú de misiones + RealityShiftController).
        if (shift == null) return;
        if (MeditationSession.Instance.IsInMission) { MeditationSession.Instance.EndMission(); return; }
        Confirm(OpenSession);
    }

    void Confirm(System.Action onConfirm)
    {
        if (ConfirmationPanel.Instance != null)
            ConfirmationPanel.Instance.Show(
                title:       "Máquina de virtualización",
                message:     $"¿Entrar a la simulación de {areaName}?",
                onConfirm:   () => onConfirm(),
                onCancel:    null,
                confirmText: "Entrar",
                cancelText:  "Cancelar");
        else
            onConfirm(); // fallback: no panel wired
    }

    void OpenSession()
    {
        MeditationSession.Instance.Open(
            missions,
            shift,
            enterAnimation: () => onEnterAnimation?.Invoke());
    }

    // ── Startup / Gizmos ────────────────────────────────────────────────────────

    void Start()
    {
        var col = GetComponent<Collider>();
        if (col != null && !col.isTrigger) col.isTrigger = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.35f);
        var col = GetComponent<Collider>();
        if (col != null) Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
}
