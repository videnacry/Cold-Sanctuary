using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>
/// CÉLULAS DE MAGO (docs/magic-metabolism-progression.md): la fracción de células sostenidas por magia no pide comida ni
/// sueño; ese mantenimiento lo paga la reserva mágica. Sin energía mágica, la fracción deja de ayudar (vuelven el
/// hambre y el sueño). La Magnate es fracción 1 con reserva inagotable.
/// </summary>
public class MageCells : MonoBehaviour, IMetabolicModifier
{
    [Range(0f, 1f)] public float fraction = 1f;
    [Tooltip("Sin MagicReserves se considera que la reserva no se agota (p. ej. la Magnate).")]
    public float upkeepEnergyPerHour = 1f;

    MagicReserves _res;
    void Awake() => _res = GetComponent<MagicReserves>();

    public float NeedsScale
    {
        get
        {
            if (_res == null) return 1f - fraction;
            return _res.energy > upkeepEnergyPerHour ? 1f - fraction : 1f;   // sin magia, el cuerpo vuelve a pedir
        }
    }
}
