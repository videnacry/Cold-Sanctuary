using UnityEngine;

public class CockroachBehavior : Animal
{
    protected override string SpeciesArchetype => "Cockroach";

    // Cucaracha oriental (Blatta orientalis): nocturna, lucífuga, se agrega en grietas por feromona (tigmotaxis); carroñera
    // omnívora que prefiere carbohidratos y evita dietas ricas en proteína (Sieksmeyer et al. 2022, BMC Ecol Evol 22:67,
    // doi 10.1186/s12862-022-02007-8). Huye con un giro de escape en cuanto nota el aire de un ataque (cercos).

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.05f;  // no caza
        armadura = 0.35f;          // tegminas y pronoto coriáceos
        armament = 0.05f;
    }
}
