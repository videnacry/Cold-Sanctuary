using UnityEngine;

/// <summary>
/// Un ser DÉBIL al que hay que dar soporte y llevar a un refugio (docs/area-progression.md "Apertura"):
/// el enfermo/anciano/cría que el grupo cuida. Es la marca que cuenta <see cref="CarryToRefuge"/>. Semilla
/// de las misiones de cuidado del área de cría. (El "cuidar al que cae" es anterior al fuego — §Pre-fuego.)
/// </summary>
public class WeakOne : MonoBehaviour
{
    [HideInInspector] public bool safe;
}
