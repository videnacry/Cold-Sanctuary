using UnityEngine;

/// <summary>
/// Placed at the Kitchen_Area entrance. Implements IInteractable so
/// InteractionController detects it via proximity or click.
///
/// Flow:
///   Player approaches → "F / Clic  Entrar a la Cocina" prompt appears
///   Player presses F or clicks → ConfirmationPanel asks "¿Empezar turno?"
///   Confirm  → KitchenScaleController.EnterKitchen() → miniaturización + mobs
///   Cancel   → nothing
///
///   While inside, the same entrance shows "Salir de la Cocina":
///   Interact again → KitchenScaleController.ExitKitchen() → restore + deactivate mobs
///
/// Scene setup:
///   1. Add a trigger Collider (Box) to this GameObject.
///   2. Assign kitchenScaleController.
///   3. Tag the Player as "Player".
///   No OnTriggerEnter — entry is always intentional and gated by the confirmation.
/// </summary>
[RequireComponent(typeof(Collider))]
public class KitchenEntrance : MonoBehaviour, IInteractable
{
    [Header("References")]
    [Tooltip("The KitchenScaleController that drives miniaturization.")]
    public KitchenScaleController kitchenScaleController;

    [Header("Labels")]
    public string areaName = "Cocina";

    // ── IInteractable ─────────────────────────────────────────────────────────

    public string InteractLabel =>
        kitchenScaleController != null && kitchenScaleController.IsMiniaturized
            ? $"Salir de la {areaName}"
            : $"Empezar turno en la {areaName}";

    public bool CanInteract => kitchenScaleController != null;

    public void Interact()
    {
        if (kitchenScaleController == null) return;

        if (kitchenScaleController.IsMiniaturized)
        {
            // Already inside — exit immediately (no confirmation needed to leave)
            kitchenScaleController.ExitKitchen();
        }
        else
        {
            // Gate entry through the confirmation panel
            if (ConfirmationPanel.Instance != null)
            {
                ConfirmationPanel.Instance.Show(
                    title:       areaName,
                    message:     $"¿Empezar turno en la {areaName}?",
                    onConfirm:   () => kitchenScaleController.EnterKitchen(),
                    onCancel:    null,
                    confirmText: "Entrar",
                    cancelText:  "Cancelar"
                );
            }
            else
            {
                // Fallback: no panel in scene — enter directly
                kitchenScaleController.EnterKitchen();
                Debug.LogWarning("[KitchenEntrance] ConfirmationPanel no está en la escena. " +
                                 "Corre 'Tools → Cold Sanctuary → Build Sample Scene Blockout' para crearlo.");
            }
        }
    }

    // ── Startup ───────────────────────────────────────────────────────────────

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[KitchenEntrance] Collider en {gameObject.name} forzado a trigger " +
                             "(debe ser trigger para que el jugador pueda caminar a través).");
        }
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.35f);
        Collider col = GetComponent<Collider>();
        if (col != null) Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        Gizmos.DrawIcon(transform.position + Vector3.up * 2f, "d_SceneViewTools", true);
    }
}
