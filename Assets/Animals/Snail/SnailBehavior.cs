using UnityEngine;

public class SnailBehavior : Animal
{
    protected override string SpeciesArchetype => "Snail";

    // Caracol (Helix lucorum en Anatolia/Grecia, Cepaea nemoralis en Europa): hermafrodita simultáneo; nocturno y amigo
    // de la lluvia; ramonea hojas, flores y hongos con la rádula. Ante el peligro se METE en la concha; con calor/sequía
    // ESTIVA sellando la abertura con un epifragma (y de igual modo hiberna). Ver docs/era-species-behavior.md.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0f;
        armadura = 0.8f;           // concha calcárea
        armament = 0f;
    }
}
