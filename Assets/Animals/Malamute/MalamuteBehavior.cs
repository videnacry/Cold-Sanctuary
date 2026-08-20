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




    // Stages (días de juego) — madurez a ~1 año; longevidad de perro cuidado (~12-14 años).











    // Post-natal species params

    static readonly PostNatalStage[] _postNatalStages =
    {
        // Stage 0 — Nacimiento; ciegos, sordos, totalmente dependientes
        new PostNatalStage {
            label = "Nacimiento", durationDays = 1f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            entryActions = new[] { MotherAction.Clean, MotherAction.Stimulate,
                                   MotherAction.GuideTeat, MotherAction.FirstMilk },
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 1f } },
        },
        // Stage 1 — Neonatal: ojos y oídos cerrados (~10-14 días reales)
        new PostNatalStage {
            label = "Neonatal", durationDays = 14f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 14f } },
        },
        // Stage 2 — Transición: ojos/oídos abiertos, primeros pasos fuera del nido
        new PostNatalStage {
            label = "Transición", durationDays = 7f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 7f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstNestExit },
            },
        },
        // Stage 3 — Socialización temprana; destete gradual (~3-8 semanas reales)
        new PostNatalStage {
            label = "Socialización temprana", durationDays = 28f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.FrequentVisits,
            weaningType = WeaningType.Gradual, feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 28f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstSolidEaten },
            },
        },
        // Stage 4 — Independencia (destete completo ~8 semanas)
        new PostNatalStage {
            label = "Independencia", durationDays = 10f,
            fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.MinimalVisits,
            weaningType = WeaningType.Gradual, feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 10f } },
        },
    };
    public override PostNatalStage[] PostNatalStages => _postNatalStages;

    // Domesticado: muy baja agresividad, casi no lucha; se une fácil por vínculo.

    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0.15f; t.canHitAndRun = false; }
}
