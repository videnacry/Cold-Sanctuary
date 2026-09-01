using UnityEngine;

// Orca (Orcinus orca): APEX marino del hielo. Caza focas y ballenas (belugas) por PROXIMIDAD + STATS (Forager.eatsPrey +
// Predation), no por una tabla fija; depreda a Seal/Whale (relaciones kármicas negativas en Archetypes → ellas la temen).
// Muy inteligente y social: caza en grupo (packFactor alto → más poder efectivo con manada). Acuática pura (indefensa en
// tierra). En el santuario NO-violento su agresividad EMERGE del histórico (confianza-por-uso); parte modesta como el oso.
public class OrcaBehavior : Animal
{
    protected override string SpeciesArchetype => "Orca";

    void Start() => Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0.6f; }
}
