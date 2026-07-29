using UnityEngine;

/// <summary>
/// Una mancha de suciedad como OBJETO real (docs/kitchen-simulation.md §5): la cocina se ensucia
/// literalmente y se limpia **mancha por mancha**. La crea y contabiliza un <see cref="DirtArea"/>.
/// (Más adelante, cada mancha del Meso = una región a "extraer" en el MicroKitchen — §7 del doc.)
/// </summary>
public class DirtSpot : MonoBehaviour
{
    [HideInInspector] public DirtArea area;

    /// <summary>Borra esta mancha (avisa a su zona y se destruye).</summary>
    public void Clean()
    {
        if (area != null) area.NotifyCleaned(this);
        Debug.Log($"[Cocina] Mancha «{name}» limpiada.");
        Destroy(gameObject);
    }
}
