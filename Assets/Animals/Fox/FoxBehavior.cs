using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Zorro ártico (Vulpes lagopus). Datos de referencia: masa adulta 3.2–9.4 kg,
// carreras cortas hasta ~50 km/h, camadas grandes (5–8 crías, hasta 25 en años
// buenos), cuidado biparental, vida silvestre corta (~3–6 años).
public class FoxBehavior : Carnivore
{
    protected override string SpeciesArchetype => "Fox";


    // Escala medida contra el mesh crudo (ver AnimalPrefabGenerator > Measure Raw Animal Sizes):
    // altura cruda 2.984m -> objetivo realista de altura de hombro adulto ~0.4m (zorro ártico).




    // Stages (días de juego) — madurez sexual real ~9-10 meses; vida corta.











    // Post-natal species params

    static readonly PostNatalStage[] _postNatalStages =
    {
        // Stage 0 — Nacimiento en madriguera
        new PostNatalStage {
            label = "Nacimiento", durationDays = 0.5f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Provider,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            entryActions = new[] { MotherAction.Clean, MotherAction.Stimulate,
                                   MotherAction.GuideTeat, MotherAction.FirstMilk },
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 0.5f } },
        },
        // Stage 1 — Ciegos y sordos, dependientes de la madriguera (~3 semanas)
        new PostNatalStage {
            label = "Dependencia total", durationDays = 21f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Provider,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 21f } },
        },
        // Stage 2 — Primeras salidas a la entrada de la madriguera
        new PostNatalStage {
            label = "Exploración de entrada", durationDays = 14f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Provider,
            presencePattern = MotherPresencePattern.FrequentVisits,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 14f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstNestExit },
            },
        },
        // Stage 3 — Regurgitación; ambos padres proveen comida sólida
        new PostNatalStage {
            label = "Regurgitación", durationDays = 30f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Provider,
            presencePattern = MotherPresencePattern.FrequentVisits,
            feedingMethod = FeedingMethod.Regurgitate,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 30f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstSolidEaten },
            },
        },
        // Stage 4 — Dispersión hacia la independencia (fin de verano/otoño)
        new PostNatalStage {
            label = "Independencia", durationDays = 90f,
            fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.MinimalVisits,
            weaningType = WeaningType.Gradual, feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 90f } },
        },
    };
    public override PostNatalStage[] PostNatalStages => _postNatalStages;

    // Evita conflicto; huye de amenazas mayores (lobos, osos).

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0.3f; t.canHitAndRun = true; }
    protected override void ConfigureForager(Forager f) { base.ConfigureForager(f); f.eatsFish = true; }   // pesca además de cazar
}
