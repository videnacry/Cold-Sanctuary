using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class BearBehaviour : Carnivore
{
    protected override string SpeciesArchetype => "Bear";

    // Family creation default values


    public ActionsPrep actsPrep = new ActionsPrep
    (
        new ActionPrep("IdleBear", 0, 1, -2),
        new ActionPrep("WalkBear", 3, 2),
        new ActionPrep("RunBear", 12, 4, 2)
    );
    public override ActionsPrep ActsPrep { get => actsPrep; set => actsPrep = value; }



    // Stages
    public Childhood childhood = new Childhood(180, 98, 99);
    public override Childhood ChildStage { get => childhood; set => childhood = value; }

    public byte[] childPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] ChildPreps { get => childPreparations; set => childPreparations = value; }

    public byte[] childEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound };
    public override byte[] ChildEvents { get => childEvents; set => childEvents = value; }

    public Adolescence adolescence = new Adolescence(900, 70, 78);
    public override Adolescence TeenStage { get => adolescence; set => adolescence = value; }

    public byte[] teenPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] TeenPreps { get => teenPreparations; set => teenPreparations = value; }

    public byte[] teenEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound };
    public override byte[] TeenEvents { get => teenEvents; set => teenEvents = value; }

    public Adulthood adulthood = new Adulthood(7300, 0, 20);
    public override Adulthood AdultStage { get => adulthood; set => adulthood = value; }

    public byte[] adultPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] AdultPreps { get => adultPreparations; set => adultPreparations = value; }

    public byte[] adultEvents = {
        LifeStage.Events.LoopGrow,
        LifeStage.Events.Fatten,
        LifeStage.Events.Wander,
        LifeStage.Events.Rest,
        LifeStage.Events.HomeBound,
        LifeStage.Events.Feed,
    };
    public override byte[] AdultEvents { get => adultEvents; set => adultEvents = value; }


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
