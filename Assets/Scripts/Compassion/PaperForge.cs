using UnityEngine;

/// <summary>
/// Donde se hacen PAPELES FALSOS: en Le Chambon, el estudiante Oscar Rosowsky (19 años) falsificaba carnés de identidad y
/// de racionamiento para los refugiados. Con E junto a ella, cada persona que sigue al jugador recibe papeles: desde
/// entonces las patrullas no la detienen. Hay un número limitado de impresos (<see cref="blanks"/>).
/// </summary>
public class PaperForge : MonoBehaviour
{
    public string label = "Imprenta";
    public int blanks = 6;
    public float radius = 3f;
}
