using UnityEngine;

public class CicadaBehavior : Animal
{
    protected override string SpeciesArchetype => "Cicada";

    // Cigarra del fresno (Cicada orni): la ninfa vive años bajo tierra chupando savia de raíces; el adulto chupa XILEMA de
    // ramas y el macho CANTA con los timbales en las horas de calor, posado bajo, en los troncos (Sueur & Aubin 2003,
    // Naturwissenschaften 90:322, doi 10.1007/s00114-003-0432-5). Solo vive unas semanas de adulto. Ver MatingSignal.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t)
    {
        t.aggressiveness = 0f;
        armadura = 0.3f;           // exoesqueleto robusto
        armament = 0f;             // huye volando (y chilla)
    }
}
