using UnityEngine;

public class SpiderBehavior : Animal
{
    protected override string SpeciesArchetype => "Spider";

    // Araña lobo (Lycosa): depredadora activa; 8 ojos = percepción excepcional.
    // Cazadora de emboscada → composure altísima en Archetypes.
    // Veneno real: armament elevado. Solitaria.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.75f;  // depredadora activa; ataca si se siente acorralada
        armadura = 0.30f;          // exoesqueleto moderado
        armament = 0.60f;          // veneno + quelíceros: arma principal
    }
}
