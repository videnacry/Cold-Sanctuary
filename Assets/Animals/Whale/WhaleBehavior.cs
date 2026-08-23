using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Beluga (Delphinapterus leucas). Datos de referencia: masa adulta 600–1600 kg,
// crucero ~3–9 km/h con picos ~22 km/h, gestación ~14.5 meses, lactancia muy
// extendida (~20-32 meses), madurez sexual ~8-9 años, vida silvestre 35-50 años.
// Extremadamente vocal ("canario del mar") y curiosa con los cuidadores — el
// santuario real de belugas de SEA LIFE Trust en Islandia es la referencia
// directa para el tono de "Cold Sanctuary". Se alimenta por filtrado/pesca
// (Herbivore.Feed la simula de forma abstracta; no hay población de peces
// cazable todavía en el juego).
public class WhaleBehavior : Animal
{
    protected override string SpeciesArchetype => "Whale";

    // Escala medida contra el mesh crudo (ver AnimalPrefabGenerator > Measure Raw Animal Sizes):
    // longitud cruda 10.372m -> objetivo realista de longitud corporal adulta ~12m.

    // Gentil y curiosa; no lucha, se acerca a los cuidadores con facilidad.

    // Filtra/pesca en mar abierto — no hay pasto que buscar (el flag eatsFish lo fija Forager.ConfigureForSpecies).
    // Afinidad de medio → data del arquetipo "Whale" (Archetypes), aplicada por SpeciesBody (etapa 5).

    void Start() => Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0f; }
}
