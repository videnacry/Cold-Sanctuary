using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeerBehavior : Herbivore
{
    protected override string SpeciesArchetype => "Deer";


    // Escala medida contra el mesh crudo (ver AnimalPrefabGenerator > Measure Raw Animal Sizes):
    // altura cruda 5.489m (usa Stag.fbx) -> objetivo realista de altura de hombro adulto ~1.2m.




    // Stages (días de juego)










    // Post-natal species params

    static readonly PostNatalStage[] _postNatalStages =
    {
        // Stage 0 — Nacimiento en campo abierto; madre come placenta para eliminar olores
        new PostNatalStage {
            label = "Nacimiento", durationDays = 0.5f,
            nestType = NestType.OpenField, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            entryActions = new[] { MotherAction.Clean, MotherAction.Stimulate,
                                   MotherAction.GuideTeat, MotherAction.FirstMilk },
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 0.5f } },
        },
        // Stage 1 — Ocultamiento: cría quieta sola; madre pasta a distancia
        // MarkHidingSpot: madre actualiza HomeOrigin de la cría al dejarla
        new PostNatalStage {
            label = "Ocultamiento", durationDays = 14f,
            nestType = NestType.OpenField, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.FrequentVisits,
            feedingMethod = FeedingMethod.Nurse,
            entryActions = new[] { MotherAction.MarkHidingSpot },
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 14f } },
        },
        // Stage 2 — Sigue a la madre; ya camina bien
        new PostNatalStage {
            label = "Sigue a la madre", durationDays = 60f,
            nestType = NestType.OpenField, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 60f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstNestExit },
            },
        },
        // Stage 3 — Introducción a pastos; destete gradual
        new PostNatalStage {
            label = "Introducción sólidos", durationDays = 60f,
            fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.FrequentVisits,
            weaningType = WeaningType.Gradual, feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 60f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstSolidEaten },
            },
        },
        // Stage 4 — Independencia gradual
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

    // Herbívoro: huye ante amenazas, no lucha.

    void Start() => Init();

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0f; t.canHitAndRun = false; }
}
