using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Malamute de Alaska (Canis lupus familiaris). Datos de referencia: masa adulta
// ~34–38 kg (más grande y fuerte que el husky), perro de carga/trineo que prioriza
// fuerza y resistencia sobre velocidad; muy social, baja agresividad, instinto de
// manada. En Cold Sanctuary representa a los perros de trabajo del santuario:
// deambulan y se vinculan, pero casi no cazan (dependen de que se les alimente).
// Rol salvaje-vs-mascota aún por decidir (ver docs/refuge-and-adult-behavior.md).
public class MalamuteBehavior : Carnivore
{
    protected override string SpeciesArchetype => "Malamute";

    // Escala medida contra el mesh crudo (ver AnimalPrefabGenerator > Measure Raw Animal Sizes):
    // altura cruda 3.388m -> objetivo realista de altura de hombro adulto ~0.63m (malamute, mayor que el husky).

    // Domesticado: muy baja agresividad, casi no lucha; se une fácil por vínculo.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0.15f; t.canHitAndRun = false; }
}
