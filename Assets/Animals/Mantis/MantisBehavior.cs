using UnityEngine;

public class MantisBehavior : Animal
{
    protected override string SpeciesArchetype => "Mantis";

    // Mantis religiosa: depredador de emboscada con visión estereoscópica (percepción muy alta) y patas
    // raptoras (str). Solitaria, canibalismo sexual (como la araña). Ataca por sorpresa desde la quietud.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.7f;   // depredadora activa desde la emboscada
        armadura = 0.30f;          // exoesqueleto moderado
        armament = 0.65f;          // patas raptoras espinosas: arma principal
    }
}
