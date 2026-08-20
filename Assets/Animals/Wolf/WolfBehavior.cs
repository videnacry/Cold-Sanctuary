using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfBehavior : Carnivore
{
    protected override string SpeciesArchetype => "Wolf";


    // Escala medida contra el mesh crudo (ver AnimalPrefabGenerator > Measure Raw Animal Sizes):
    // altura cruda 2.984m -> objetivo realista de altura de hombro adulto ~0.8m.















    // Post-natal species params

    static readonly PostNatalStage[] _postNatalStages =
    {
        // Stage 0 — Nacimiento: secuencia fija
        new PostNatalStage {
            label = "Nacimiento", durationDays = 1f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Provider,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            entryActions = new[] { MotherAction.Clean, MotherAction.Stimulate,
                                   MotherAction.GuideTeat, MotherAction.FirstMilk },
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 1f } },
        },
        // Stage 1 — Dependencia total (ojos cerrados)
        new PostNatalStage {
            label = "Dependencia total", durationDays = 14f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Provider,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 14f } },
        },
        // Stage 2 — Exploración temprana y juego entre camada
        new PostNatalStage {
            label = "Exploración temprana", durationDays = 30f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Provider,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 30f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstNestExit },
            },
        },
        // Stage 3 — Introducción a regurgitación
        new PostNatalStage {
            label = "Regurgitación", durationDays = 45f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Provider,
            presencePattern = MotherPresencePattern.FrequentVisits,
            feedingMethod = FeedingMethod.Regurgitate,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 45f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstSolidEaten },
            },
        },
        // Stage 4 — Integración a la manada / caza observada
        new PostNatalStage {
            label = "Integración manada", durationDays = 90f,
            nestType = NestType.Burrow, fatherRole = FatherRole.ActiveCaregiver,
            presencePattern = MotherPresencePattern.FrequentVisits,
            feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 90f } },
        },
        // Stage 5 — Independencia gradual
        new PostNatalStage {
            label = "Independencia", durationDays = 120f,
            fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.MinimalVisits,
            weaningType = WeaningType.Gradual, feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 120f } },
        },
    };
    public override PostNatalStage[] PostNatalStages => _postNatalStages;


    void Start() => base.Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0.7f; t.canHitAndRun = false; }
}
