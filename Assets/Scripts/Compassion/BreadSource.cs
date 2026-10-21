using UnityEngine;

/// <summary>
/// De dónde sale la COMIDA de los escondidos: la panadería del pueblo, la cocina del presbiterio de Magda Trocmé, la
/// despensa de una familia de Varsovia. Con E junto a ella el jugador carga raciones (máx. <see cref="carry"/>); con E
/// junto a un <see cref="HidingPlace"/>, las deja en su despensa.
/// </summary>
public class BreadSource : MonoBehaviour
{
    public string label = "Panadería";
    [Min(1)] public int carry = 4;
    public float radius = 3f;
}
