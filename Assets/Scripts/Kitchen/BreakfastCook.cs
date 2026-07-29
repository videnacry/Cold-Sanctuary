using UnityEngine;

/// <summary>
/// El loop de cocinar el desayuno (docs/kitchen-simulation.md §3). Recorre la **cadena de huevos
/// revueltos** paso a paso y, al emplatar, deposita una ración en el <see cref="FoodContainer"/>; luego
/// vuelve a empezar → el contenedor **siempre se rellena** mientras cocina.
///
/// MVP: la cadena avanza por temporizador (cada paso se anuncia por consola). El siguiente refinamiento es
/// hacerla **espacial** —caminar a nevera/plancha/contenedor reutilizando `TourStation` + `FollowBrain`,
/// como el paseo— para que "acercarse a la nevera, tomar los huevos…" sea movimiento real.
/// </summary>
public class BreakfastCook : MonoBehaviour
{
    public FoodContainer container;
    [Min(0.1f)] public float stepInterval = 1.2f;

    // La cadena (docs kitchen §3): nevera → huevos → plancha → revolver → especiar → contenedor.
    static readonly string[] STEPS =
    {
        "va a la nevera y la abre",
        "toma los huevos",
        "los lleva a la plancha",
        "los revuelve hasta que sequen",
        "los especia",
        "los pasa al contenedor",
    };

    int _step;
    float _next;

    void Update()
    {
        if (container == null || Time.time < _next) return;
        _next = Time.time + stepInterval;

        Debug.Log($"[Cocina] «{name}» {STEPS[_step]}.");
        if (_step == STEPS.Length - 1) container.Deposit(1);   // emplatar → +1 ración

        _step = (_step + 1) % STEPS.Length;                    // reinicia el ciclo
    }
}
