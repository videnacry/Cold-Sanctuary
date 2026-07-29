using UnityEngine;

/// <summary>
/// Limpia manchas (<see cref="DirtSpot"/>) por proximidad (docs/kitchen-simulation.md §5). En el jugador:
/// pulsa <see cref="cleanKey"/> cerca de una mancha. En un NPC/pinche: <see cref="auto"/> = limpia la más
/// cercana cada <see cref="autoInterval"/> (para ver el loop en consola). Limpia **una mancha por vez**.
/// </summary>
public class Cleaner : MonoBehaviour
{
    [Tooltip("Distancia a la que puede limpiar una mancha.")]
    public float reach = 2.5f;
    public KeyCode cleanKey = KeyCode.F;

    [Header("NPC / pinche")]
    public bool auto = false;
    [Min(0.1f)] public float autoInterval = 1.5f;

    float _nextAuto;

    void Update()
    {
        if (auto)
        {
            if (Time.time >= _nextAuto) { _nextAuto = Time.time + autoInterval; TryClean(); }
        }
        else if (Input.GetKeyDown(cleanKey))
        {
            TryClean();
        }
    }

    void TryClean()
    {
        foreach (DirtArea area in FindObjectsOfType<DirtArea>())
            if (area.CleanNearest(transform.position, reach)) return;   // una mancha por acción
    }
}
