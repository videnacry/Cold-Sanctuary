using UnityEngine;

public class VelvetMiteBehavior : Animal
{
    protected override string SpeciesArchetype => "VelvetMite";

    // Ácaro de terciopelo (Trombidium holosericeum): rojo aposemático; el adulto sale tras la lluvia de primavera y caza
    // huevos de insectos y artrópodos diminutos; la LARVA es ectoparásita de insectos (Shatrov & Felska 2017, Exp Appl
    // Acarol 72:339, doi 10.1007/s10493-017-0172-4). Los depredadores lo rechazan (sabor).

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.3f;
        armadura = 0.3f;           // tegumento grueso
        armament = 0.25f;          // quelíceros + sabor que disuade
    }
}
