using UnityEngine;

public class FireflyBehavior : Animal
{
    protected override string SpeciesArchetype => "Firefly";

    // Luciérnaga (Luciola): los machos vuelan al anochecer DESTELLANDO y la hembra contesta desde la hierba (Lewis &
    // Cratsley 2008, Annu Rev Entomol 53:293). La larva es depredadora de caracoles y babosas (les inyecta enzimas). Las
    // lucibufaginas de la sangre la hacen tóxica (sangrado reflejo). Ver MatingSignal.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.4f;   // la larva caza caracoles; el adulto apenas come
        armadura = 0.15f;          // élitros blandos
        armament = 0.35f;          // mandíbulas huecas inyectoras (larva) + sangre tóxica
    }
}
