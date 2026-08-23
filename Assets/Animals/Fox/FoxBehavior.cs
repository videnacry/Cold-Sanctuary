using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Zorro ártico (Vulpes lagopus). Datos de referencia: masa adulta 3.2–9.4 kg,
// carreras cortas hasta ~50 km/h, camadas grandes (5–8 crías, hasta 25 en años
// buenos), cuidado biparental, vida silvestre corta (~3–6 años).
public class FoxBehavior : Animal
{
    protected override string SpeciesArchetype => "Fox";

    // Escala medida contra el mesh crudo (ver AnimalPrefabGenerator > Measure Raw Animal Sizes):
    // altura cruda 2.984m -> objetivo realista de altura de hombro adulto ~0.4m (zorro ártico).

    // Evita conflicto; huye de amenazas mayores (lobos, osos).

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0.3f; }
}
