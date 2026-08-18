using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Misión "llevar la comida a la cueva" — Microcosmos de la Cocina (era pre-fuego).
///
/// Flujo:
///   1. Los ítems de comida esparcidos tienen <see cref="ScentEmitter"/> activo.
///   2. Depredadores con <see cref="ScentScanner"/> se acercan al olor.
///   3. Kushal (PullSpell) y las hormigas llevan la comida a la zona "cueva".
///   4. Cuando la comida entra en la zona la misión la registra como recogida
///      (<see cref="RegisterCollected"/>) y desactiva su ScentEmitter.
///   5. Al recoger todas las piezas se dispara <see cref="onMissionComplete"/>.
///
/// Referencia de escena: añadir este componente a un objeto raíz de la misión.
/// Arrastrar los GameObjects de comida a <see cref="foodItems"/>.
/// El trigger de "cueva" llama a <see cref="RegisterCollected"/> vía
/// <see cref="CaveTrigger"/> (componente hermano o hijo).
/// </summary>
public class KitchenFireMission : MonoBehaviour
{
    [Header("Ítems de comida")]
    [Tooltip("Todos los GameObjects de comida de la escena (deben tener ScentEmitter).")]
    public GameObject[] foodItems = System.Array.Empty<GameObject>();

    [Header("Eventos")]
    public UnityEvent onMissionComplete;
    public UnityEvent<int, int> onProgressChanged; // (recogidos, total)

    // ── Estado ────────────────────────────────────────────────────────────────

    int _total;
    int _collected;

    public int Total     => _total;
    public int Collected => _collected;
    public bool IsComplete => _total > 0 && _collected >= _total;

    // ── Ciclo ─────────────────────────────────────────────────────────────────

    void Start()
    {
        _total     = foodItems.Length;
        _collected = 0;
        Debug.Log($"[KitchenFireMission] Misión iniciada: {_total} ítems de comida.");
    }

    // ── API ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Registra un ítem de comida como recogido. Desactiva su ScentEmitter y,
    /// opcionalmente, desactiva el GameObject. Llama desde el trigger de la cueva.
    /// </summary>
    public void RegisterCollected(GameObject foodItem, bool deactivateObject = true)
    {
        if (IsComplete) return;

        ScentEmitter scent = foodItem.GetComponent<ScentEmitter>();
        if (scent != null)
            scent.Deactivate();

        if (deactivateObject)
            foodItem.SetActive(false);

        _collected++;
        Debug.Log($"[KitchenFireMission] Recogido {_collected}/{_total}: {foodItem.name}");
        onProgressChanged?.Invoke(_collected, _total);

        if (IsComplete)
        {
            Debug.Log("[KitchenFireMission] ¡Misión completada! Toda la comida está en la cueva.");
            onMissionComplete?.Invoke();
        }
    }

    void OnGUI()
    {
        if (!Application.isPlaying) return;
        GUI.Label(new Rect(10, 10, 200, 30),
            $"Comida recogida: {_collected} / {_total}");
    }
}
