using UnityEngine;

public class MeerkatBehavior : Animal
{
    protected override string SpeciesArchetype => "Meerkat";

    // Suricata (Suricata suricatta): sabana/semiárido africano. Cría COOPERATIVA con base (madriguera),
    // CENTINELAS que solo vigilan (percepción máxima = el rol de OBSERVACIÓN), enseñan a las crías a manejar
    // presas peligrosas/veneno. Insectívora. EN ESTE JUEGO se usa además como SKIN a escala insecto (render de
    // la máquina de avatares sobre la tribu de hormigas; ver docs/microcosmos-eras-and-observation.md §1b).

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0.35f;  // mobbing cooperativo contra amenazas; si no, huye al refugio
        armadura = 0.15f;          // piel fina
        armament = 0.25f;          // mordida/garras menores
    }
}
