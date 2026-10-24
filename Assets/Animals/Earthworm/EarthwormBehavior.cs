using UnityEngine;

public class EarthwormBehavior : Animal
{
    protected override string SpeciesArchetype => "Earthworm";

    // Lombriz de tierra (Lumbricus terrestris): sale de NOCHE a la superficie y ARRASTRA hojas a su galería vertical (Darwin
    // 1881, The Formation of Vegetable Mould); hermafrodita: se aparea en la superficie, cabeza con cola, sin salir del
    // todo de su galería (Butt & Nuutinen 1998, Can J Zool 76:104). Ante el peligro se RETRAE a la galería.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0f;
        armadura = 0.02f;
        armament = 0f;
    }
}
