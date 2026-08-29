using UnityEngine;

public class AphidBehavior : Animal
{
    protected override string SpeciesArchetype => "Aphid";

    // Pulgón: lento, gordo, presa fácil. Succiona savia; sin defensa activa.
    // Produce melaza (honeydew) — ver HoneydewProducer como componente separado.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.0f;   // sin defensa — solo huye (o ni eso)
        armadura = 0.05f;          // casi sin quitina; cuerpo blando
        armament = 0.0f;           // sin arma ofensiva
    }
}
