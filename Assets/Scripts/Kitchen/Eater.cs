using UnityEngine;

/// <summary>
/// Un personaje que se acerca a un <see cref="FoodContainer"/> a **comer** (docs/kitchen-simulation.md §6).
/// MVP: intenta comer cada <see cref="eatInterval"/> del contenedor con comida más cercano. En el **paso C**
/// la decisión pasará a estar **dirigida por los humores** (elegir el platillo según el estado — glucosa
/// baja → energético, cortisol alto → reconfortante) y comer aplicará los **compuestos** del platillo.
/// </summary>
public class Eater : MonoBehaviour
{
    [Min(0.1f)] public float eatInterval = 3f;
    [Tooltip("Distancia a la que puede tomar del contenedor (MVP amplio para el demo sin moverse).")]
    public float reach = 15f;

    float _next;

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + eatInterval;

        FoodContainer best = null;
        float bestSqr = reach * reach;
        foreach (FoodContainer fc in FindObjectsOfType<FoodContainer>())
        {
            if (fc == null || !fc.HasFood) continue;
            float d = (fc.transform.position - transform.position).sqrMagnitude;
            if (d <= bestSqr) { bestSqr = d; best = fc; }
        }

        if (best != null && best.Take())
            Debug.Log($"[Cocina] «{name}» come {best.dishName} de «{best.name}».");
    }
}
