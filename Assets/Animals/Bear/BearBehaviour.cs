using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class BearBehaviour : Carnivore
{
    protected override string SpeciesArchetype => "Bear";

    // Family creation default values















    // Post-natal species params

    static readonly PostNatalStage[] _postNatalStages =
    {
        // Stage 0 — Nacimiento en letargo (madre semi-inconsciente)
        new PostNatalStage {
            label = "Nacimiento en letargo", durationDays = 1f,
            nestType = NestType.SnowDen, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            entryActions = new[] { MotherAction.Clean, MotherAction.Stimulate,
                                   MotherAction.GuideTeat, MotherAction.FirstMilk },
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 1f } },
        },
        // Stage 1 — Madriguera / madre en letargo profundo (invierno)
        new PostNatalStage {
            label = "Madriguera letargo", durationDays = 60f,
            nestType = NestType.SnowDen, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 60f } },
        },
        // Stage 2 — Primera salida (primavera); madre consume fatReserves
        new PostNatalStage {
            label = "Primera salida", durationDays = 60f,
            nestType = NestType.Burrow, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            entryActions = new[] { MotherAction.MarkHidingSpot },
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 60f } },
        },
        // Stage 3 — Aprendizaje activo (pesca/caza observada)
        new PostNatalStage {
            label = "Aprendizaje", durationDays = 120f,
            fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.FrequentVisits,
            feedingMethod = FeedingMethod.Regurgitate,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 120f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstSolidEaten },
            },
        },
        // Stage 4 — Independencia gradual (madre puede expulsar)
        new PostNatalStage {
            label = "Independencia", durationDays = 180f,
            fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.MinimalVisits,
            weaningType = WeaningType.Gradual, feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 180f },
                new TransitionCondition { kind = TransitionCondition.Kind.MotherFatReservesBelow, threshold = 20f },
            },
        },
    };
    public override PostNatalStage[] PostNatalStages => _postNatalStages;

    // ThreatResponse: solitario pero pesado y agresivo → lucha si tiene ventaja de masa.

    // El umbral 50 de 'difficulty' está por calibrar — ver decisiones abiertas en behavior-system.md.

    void Start()
    {
        base.Init();
    }

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0.6f; t.canHitAndRun = false; }
    protected override void ConfigureForager(Forager f) { base.ConfigureForager(f); f.eatsFish = true; }   // pesca además de cazar
}
