using UnityEngine;

// Morsa (Odobenus rosmarus): pinnípedo del Ártico; come bivalvos del fondo y sale a descansar al hielo y a la grava (MarineLife). Ficha: docs/reglas-animales.md (Fichas).
public class WalrusBehavior : Animal
{
    protected override string SpeciesArchetype => "Walrus";

    void Start() => Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0.5f; }   // colmillos y tamaño: se defiende
}
