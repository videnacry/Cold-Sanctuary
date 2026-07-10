using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Zorro ártico (Vulpes lagopus). Datos de referencia: masa adulta 3.2–9.4 kg,
// carreras cortas hasta ~50 km/h, camadas grandes (5–8 crías, hasta 25 en años
// buenos), cuidado biparental, vida silvestre corta (~3–6 años).
public class FoxBehavior : Carnivore
{
    public static Family defaultGroup = new Family(7, 0.4f, Family.biparental);
    public Family group = defaultGroup;
    public override Family Group { get => group; set => group = value; }

    // Escala medida contra el mesh crudo (ver AnimalPrefabGenerator > Measure Raw Animal Sizes):
    // altura cruda 2.984m -> objetivo realista de altura de hombro adulto ~0.4m (zorro ártico).
    public static Physiognomy defaultBody = new Physiognomy(new Vector3(0.134f, 0.134f, 0.134f), 4, 0.09f, 0.2f, 0.05f);
    public Physiognomy body = defaultBody;
    public override Physiognomy Body { get => body; set => body = value; }

    public ActionsPrep actsPrep = new ActionsPrep
    (
        new ActionPrep("IdleFox", 0, 1, -2),
        new ActionPrep("WalkFox", 3, 3),
        new ActionPrep("RunFox", 14, 5, 2)   // burst ~50 km/h real
    );
    public override ActionsPrep ActsPrep { get => actsPrep; set => actsPrep = value; }

    public Vector3 homeOrigin;
    public override Vector3 HomeOrigin { get => homeOrigin; set => homeOrigin = value; }

    public float homeRadius = 150;
    public override float HomeRadius { get => homeRadius; set => homeRadius = value; }

    // Stages (días de juego) — madurez sexual real ~9-10 meses; vida corta.
    public Childhood childhood = new Childhood(80, 98, 99);
    public override Childhood ChildStage { get => childhood; set => childhood = value; }

    public byte[] childPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] ChildPreps { get => childPreparations; set => childPreparations = value; }

    public byte[] childEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound };
    public override byte[] ChildEvents { get => childEvents; set => childEvents = value; }

    public Adolescence adolescence = new Adolescence(270, 70, 80);
    public override Adolescence TeenStage { get => adolescence; set => adolescence = value; }

    public byte[] teenPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] TeenPreps { get => teenPreparations; set => teenPreparations = value; }

    public byte[] teenEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound };
    public override byte[] TeenEvents { get => teenEvents; set => teenEvents = value; }

    public Adulthood adulthood = new Adulthood(1095, 0, 20);  // ~3 años, vida silvestre corta
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
    public override AnimationsName animationsName { get; } = new AnimationsName("Fox");

    // Diet: oportunista — conejos como presa principal, aves cuando están a mano.
    public static Diet defaultDiet = new Diet(new PreyEntry[]
    {
        new PreyEntry(BunnyBehavior.population, 12f, 0f, 400f),
        new PreyEntry(BirdBehavior.population, 6f, 0f, 300f),
        new PreyEntry(FishSchool.population, 5f, 10f, 250f),     // pesca/roba pescado ocasional
    });
    public Diet diet = defaultDiet;
    public override Diet Diet { get => diet; set => diet = value; }

    // Post-natal species params
    public override float BaseStressLevel       => 0.5f;   // mesodepredador, presa de lobos y osos
    public override float VocalizationThreshold => 4f;
    public override float NestSecurityLevel     => 0.6f;
    public override float MaxFatReserves        => 12f;    // engorda fuerte antes del invierno
    public override float FatAccumulationRate   => 0.8f;

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
    public override float Aggressiveness => 0.3f;
    public override bool DefendsCubs => true;
    public override bool CanHitAndRun => true;
    public override float PackFactor => 0.2f;
    public override float HarmVsBond => 0.6f;
    public override float BondGrowthRate => 0.8f;
    public override float BiteSize => 2.5f;
    public override float Toughness => 0.4f;
    public override float BaseAgility    => 1.4f;   // ágil, escurridizo
    public override float BasePerception => 1.5f;   // sentidos muy afinados

    void Start() => base.Init();

}
