using UnityEngine;

public class ScorpionBehavior : Animal
{
    protected override string SpeciesArchetype => "Scorpion";

    // Escorpión (Leiurus quinquestriatus en Egipto, Heterometrus en el Ganges, Mesobuthus gibbosus en Egina): nocturno,
    // caza al ACECHO a la entrada de su madriguera bajo una piedra (sit-and-wait; Polis 1979, J Zool 188:333) y siente a
    // la presa por las vibraciones del suelo. La madre pare crías vivas que suben a su LOMO hasta la primera muda
    // (Polis & Sissom 1990, The Biology of Scorpions) → BroodRide. Brilla con luz UV (cutícula hialina). Ver
    // docs/era-species-behavior.md.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.6f;   // caza activa; con la cola en alto ante una amenaza
        armadura = 0.55f;          // cutícula muy esclerotizada
        armament = 0.85f;          // aguijón (Leiurus: de los venenos más potentes, ADW 2012) + quelas
    }
}
