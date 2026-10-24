using UnityEngine;

public class WoodlouseBehavior : Animal
{
    protected override string SpeciesArchetype => "Woodlouse";

    // Cochinilla de la humedad (Armadillidium vulgare; Porcellio scaber en otras eras): crustáceo terrestre. Nocturna,
    // busca humedad y se AGREGA bajo piedras/hojarasca (higrocinesis); detritívora (hojarasca, hongos). Ante un ataque se
    // hace BOLA (conglobación) y tarda más en desenrollarse si huele depredador (Zamora-Camacho 2023, PeerJ 11:e16696,
    // doi 10.7717/peerj.16696) → PassiveDefence. La hembra lleva los huevos en el MARSUPIO (bolsa incubadora).

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0f;     // nunca ataca
        armadura = 0.6f;           // terguitos calcificados (la bola es una coraza)
        armament = 0f;
    }
}
