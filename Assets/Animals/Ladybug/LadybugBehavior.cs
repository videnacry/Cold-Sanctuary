using UnityEngine;

public class LadybugBehavior : Animal
{
    protected override string SpeciesArchetype => "Ladybug";

    // Mariquita: depredadora de pulgones; élitros duros (alta armadura para su tamaño).
    // Aviso de color rojo = señal de toxicidad (alta armadura, no armament).
    // Semi-solitaria; vuela cuando amenazada.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.45f;  // caza pulgones activamente; evita depredadores mayores
        armadura = 0.55f;          // élitros duros + alcaloides tóxicos → muy difícil de comer
        armament = 0.15f;          // mordida pequeña, pero efectiva en pulgones
    }
}
