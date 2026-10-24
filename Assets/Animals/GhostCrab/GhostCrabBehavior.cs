using UnityEngine;

public class GhostCrabBehavior : Animal
{
    protected override string SpeciesArchetype => "GhostCrab";

    // Cangrejo fantasma (Ocypode quadrata): de playa; nocturno, vive en una madriguera en la arena a la que vuelve (alta
    // fidelidad; Gül & Griffen 2019, Ecol Evol 9:14205, doi 10.1002/ece3.5853). CORRE DE LADO, de los artrópodos más
    // rápidos (Burrows & Hoyle 1973, J Exp Biol 58:327). Depredador y carroñero: almejas, pulgas de mar, huevos y pollos de
    // aves de playa (Call et al. 2024, doi 10.1371/journal.pone.0307821). Ver SidewaysGait.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.55f;  // planta cara con las pinzas en alto
        armadura = 0.6f;           // caparazón
        armament = 0.55f;          // quelas
    }
}
