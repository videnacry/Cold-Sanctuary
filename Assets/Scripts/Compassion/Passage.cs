using UnityEngine;

/// <summary>
/// PASO para cruzar sin ser visto: el tribunal de la calle Leszno (una puerta daba al gueto y otra a la ciudad), las
/// alcantarillas, la ambulancia de Irena Sendler. Con E junto a la entrada, el jugador y quienes le siguen aparecen en
/// la salida. Algunos pasos admiten pocos por viaje (<see cref="perTrip"/>) y necesitan un rato entre viajes.
/// </summary>
public class Passage : MonoBehaviour
{
    public string label = "Paso";
    public Transform exit;
    public float radius = 3f;
    [Tooltip("Cuántos acompañantes caben por viaje (0 = todos).")]
    public int perTrip;
    [Tooltip("Segundos entre viajes (la ambulancia tiene que volver).")]
    public float cooldown;
    [Tooltip("Solo cruza el jugador (la puerta del gueto: el pase sanitario de Irena valía para ella, no para los niños).")]
    public bool alone;

    float _ready;
    public bool Ready => Time.time >= _ready;
    public void Used() => _ready = Time.time + cooldown;
}
