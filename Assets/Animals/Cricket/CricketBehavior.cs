using UnityEngine;

public class CricketBehavior : Animal
{
    protected override string SpeciesArchetype => "Cricket";

    // Grillo: omnívoro oportunista; saltador (agility alta).
    // Canto = comunicación social (vocalizationThreshold bajo en adulto).
    // Primer en huir; muerde solo si acorralado.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.25f;  // huye primero; pelea como último recurso
        armadura = 0.20f;          // quitina ligera
        armament = 0.15f;          // mandíbulas pequeñas
    }
}
