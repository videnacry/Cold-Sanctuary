using UnityEngine;

/// <summary>
/// Una parada del paseo guiado (docs/kitchen-simulation.md §1): un punto del área con su nombre y **qué se
/// puede hacer/usar** ahí. El <see cref="GuidedTour"/> las recorre en orden y las va "enseñando".
/// </summary>
public class TourStation : MonoBehaviour
{
    public string stationName = "Estación";
    [TextArea] public string canDoHere = "Aquí puedes…";
}
