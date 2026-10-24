using UnityEngine;

public class ScorpionflyBehavior : Animal
{
    protected override string SpeciesArchetype => "Scorpionfly";

    // Pulga de las nieves (Boreus hyemalis, Mecoptera): activa EN INVIERNO sobre la nieve (así evita depredadores); sin
    // alas de vuelo; salta para escapar (Burrows 2019, J Exp Biol 222:jeb205385); come musgo (Pyszko et al. 2024, Insects
    // 15:878, doi 10.3390/insects15110878). En la cópula el macho sujeta a la hembra sobre su lomo con las alas-gancho.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0f;
        armadura = 0.15f;
        armament = 0f;
    }
}
