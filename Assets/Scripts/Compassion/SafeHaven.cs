using UnityEngine;

/// <summary>
/// DESTINO SEGURO: la frontera suiza a la que se guiaba a algunos refugiados desde Le Chambon, o el convento, el
/// orfanato y las familias que acogían a los niños de Irena Sendler. Con E junto a él, quienes siguen al jugador quedan
/// A SALVO (cuentan para la misión).
/// </summary>
public class SafeHaven : MonoBehaviour
{
    public string label = "Lugar seguro";
    public float radius = 4f;
    [Tooltip("Cuántos puede recibir en total (0 = sin límite).")]
    public int capacity;
    public int received;
    public bool Full => capacity > 0 && received >= capacity;
}
