using UnityEngine;

// Pingüino: ave marina NO voladora. Nada muy bien (aletas), torpe en tierra/hielo, donde descansa y anida (haul-out).
// Dieta propia doble: come PECES y KRILL (Forager.ConfigureForSpecies → eatsFish + eatsKrill), ambos por
// MORDISCO-POR-COLISIÓN sobre el banco (Swarm). Presa de foca y orca (relaciones kármicas en Archetypes). A futuro:
// merodea el árbol carnívoro esperando que bajen las abejas al cadáver (docs/ice-sanctuary-ecology §3). Su SUEÑO va en
// tierra/hielo — hoy `asleep` es una compuerta sin disparador (ver §4). Anfibio: afinidad agua alta, tierra media.
public class PenguinBehavior : Animal
{
    protected override string SpeciesArchetype => "Penguin";

    void Start() => Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0f; }   // no lucha: huye del peligro
}
