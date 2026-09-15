using UnityEngine;

public class TermiteBehavior : Animal
{
    protected override string SpeciesArchetype => "Termite";

    // Termita cosechadora/cultivadora de hongos (Hodotermes/Macrotermes): eusocial, montículo = ciudad,
    // obreras casi ciegas (química/tacto), mandíbulas fuertes, agricultura de hongos. Sabana del Pleistoceno.
    // Herbívora/detritívora (corta hierba). Colonia = disciplina y sociabilidad máximas.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.25f;  // soldados defienden el nido; obreras huyen
        armadura = 0.25f;          // cutícula blanda (obrera)
        armament = 0.20f;          // mandíbulas de soldado (defensa)
    }
}
