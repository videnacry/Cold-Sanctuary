using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfBehavior : Carnivore
{
    public static Family defaultGroup = new Family(6, 0.3f, Family.biparental);
    public Family group = defaultGroup;
    public override Family Group { get => group; set => group = value; }

    // Escala medida contra el mesh crudo (ver AnimalPrefabGenerator > Measure Raw Animal Sizes):
    // altura cruda 2.984m -> objetivo realista de altura de hombro adulto ~0.8m.
    public static Physiognomy defaultBody = new Physiognomy(new Vector3(0.268f, 0.268f, 0.268f), 45, 0.09f, 0.2f, 0.05f);
    public Physiognomy body = defaultBody;
    public override Physiognomy Body { get => body; set => body = value; }

    public ActionsPrep actsPrep = new ActionsPrep
    (
        new ActionPrep("IdleWolf", 0, 1, -2),
        new ActionPrep("WalkWolf", 3, 3),
        new ActionPrep("RunWolf", 22, 5, 2)
    );
    public override ActionsPrep ActsPrep { get => actsPrep; set => actsPrep = value; }

    public Vector3 homeOrigin;
    public override Vector3 HomeOrigin { get => homeOrigin; set => homeOrigin = value; }

    public float homeRadius = 200;
    public override float HomeRadius { get => homeRadius; set => homeRadius = value; }

    // Stages
    public Childhood childhood = new Childhood(77, 98, 99);
    public override Childhood ChildStage { get => childhood; set => childhood = value; }

    public byte[] childPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] ChildPreps { get => childPreparations; set => childPreparations = value; }

    public byte[] childEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound };
    public override byte[] ChildEvents { get => childEvents; set => childEvents = value; }

    public Adolescence adolescence = new Adolescence(730, 70, 78);
    public override Adolescence TeenStage { get => adolescence; set => adolescence = value; }

    public byte[] teenPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] TeenPreps { get => teenPreparations; set => teenPreparations = value; }

    public byte[] teenEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound };
    public override byte[] TeenEvents { get => teenEvents; set => teenEvents = value; }

    public Adulthood adulthood = new Adulthood(3285, 0, 20);
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

    public static HashSet<GameObject> population = new HashSet<GameObject>();
    public override HashSet<GameObject> Population { get => population; set => population = value; }
    public override AnimationsName animationsName { get; } = new AnimationsName("Wolf");

    // Diet: prefiere venados; conejos como alternativa fácil.
    public static Diet defaultDiet = new Diet(new PreyEntry[]
    {
        new PreyEntry(DeerBehavior.population, 15f, 0f, 700f),
        new PreyEntry(BunnyBehavior.population, 8f, 0f, 700f),
        new PreyEntry(FoxBehavior.population, 5f, 20f, 600f),    // competencia entre cánidos
        new PreyEntry(MalamuteBehavior.population, 4f, 30f, 600f),  // solo con bastante hambre
        new PreyEntry(BearBehaviour.population, 2f, 80f, 500f),  // solo en manada y muerto de hambre; el combate lo decide masa + PackFactor
        new PreyEntry(PlayerTarget.population, 1f, 85f, 500f),   // humano: solo hambriento y sin vínculo (CanHarm)
    });
    public Diet diet = defaultDiet;
    public override Diet Diet { get => diet; set => diet = value; }

    // Post-natal species params
    public override float BaseStressLevel       => 0.3f;
    public override float VocalizationThreshold => 3f;   // cachorros lloran fácil
    public override float NestSecurityLevel     => 0.7f;
    public override float MaxFatReserves        => 15f;
    public override float FatAccumulationRate   => 0.6f;

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

    public override float Aggressiveness => 0.7f;
    public override bool DefendsCubs => true;
    public override bool CanHitAndRun => false;
    public override float PackFactor => 0.8f;
    public override float HarmVsBond => 0.8f;
    public override float BondGrowthRate => 0.5f;
    public override float BiteSize => 5f;
    public override float Toughness => 0.8f;
    public override float BaseAgility    => 1.2f;
    public override float BasePerception => 1.4f;   // cazador de manada, olfato agudo

    void Start() => base.Init();

}
