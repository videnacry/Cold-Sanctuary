using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeerBehavior : Herbivore
{
    public static Family defaultGroup = new Family(6, 0.3f, Family.maternal);
    public Family group = defaultGroup;
    public override Family Group { get => group; set => group = value; }

    // Escala medida contra el mesh crudo (ver AnimalPrefabGenerator > Measure Raw Animal Sizes):
    // altura cruda 5.489m (usa Stag.fbx) -> objetivo realista de altura de hombro adulto ~1.2m.
    public static Physiognomy defaultBody = new Physiognomy(new Vector3(0.219f, 0.219f, 0.219f), 90, 0.07f, 0.25f, 0.1f);
    public Physiognomy body = defaultBody;
    public override Physiognomy Body { get => body; set => body = value; }

    public ActionsPrep actsPrep = new ActionsPrep(
        new ActionPrep("IdleDeer", 0, 1, -2),
        new ActionPrep("WalkDeer", 5, 2),
        new ActionPrep("RunDeer", 18, 4, 2)
    );
    public override ActionsPrep ActsPrep { get => actsPrep; set => actsPrep = value; }

    public Vector3 homeOrigin;
    public override Vector3 HomeOrigin { get => homeOrigin; set => homeOrigin = value; }

    public float homeRadius = 250;
    public override float HomeRadius { get => homeRadius; set => homeRadius = value; }

    // Stages (días de juego)
    public Childhood childhood = new Childhood(60, 60, 85);
    public override Childhood ChildStage { get => childhood; set => childhood = value; }

    public byte[] childPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] ChildPreps { get => childPreparations; set => childPreparations = value; }

    public byte[] childEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound };
    public override byte[] ChildEvents { get => childEvents; set => childEvents = value; }

    public Adolescence adolescence = new Adolescence(540, 65, 85);
    public override Adolescence TeenStage { get => adolescence; set => adolescence = value; }

    public byte[] teenPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] TeenPreps { get => teenPreparations; set => teenPreparations = value; }

    public byte[] teenEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest };
    public override byte[] TeenEvents { get => teenEvents; set => teenEvents = value; }

    public Adulthood adulthood = new Adulthood(4380, 0, 20);
    public override Adulthood AdultStage { get => adulthood; set => adulthood = value; }

    public byte[] adultPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] AdultPreps { get => adultPreparations; set => adultPreparations = value; }

    public byte[] adultEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed };
    public override byte[] AdultEvents { get => adultEvents; set => adultEvents = value; }

    public static HashSet<GameObject> population = new HashSet<GameObject>();
    public override HashSet<GameObject> Population { get => population; set => population = value; }
    public override AnimationsName animationsName { get; } = new AnimationsName("Deer");

    // Post-natal species params
    public override float BaseStressLevel       => 0.6f;
    public override float VocalizationThreshold => 9f;  // muy alto; cría usa inmovilidad, no llanto
    public override float NestSecurityLevel     => 0.1f; // campo abierto; muy expuesto
    public override float MaxFatReserves        => 10f;
    public override float FatAccumulationRate   => 0.4f;

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
    public override float Aggressiveness => 0f;
    public override bool DefendsCubs => false;
    public override bool CanHitAndRun => false;
    public override float PackFactor => 0f;
    public override float HarmVsBond => 0.1f;
    public override float BondGrowthRate => 1.8f;
    public override float Toughness => 0.4f;

    void Start() => Init();

}
