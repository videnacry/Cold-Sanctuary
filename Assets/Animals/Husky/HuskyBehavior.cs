using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Husky siberiano (Canis lupus familiaris). Datos de referencia: masa adulta
// 16–27 kg, trote sostenido ~12–16 km/h con sprints hasta ~30 km/h, muy social
// y de baja agresividad hacia personas, vida larga gracias al cuidado humano
// (~12–14 años frente a los ~9 de un lobo salvaje). En Cold Sanctuary representa
// a los perros de trineo sueltos del santuario: conservan el instinto de manada
// pero casi no cazan, dependen de que se les alimente.
public class HuskyBehavior : Carnivore
{
    public static Family defaultGroup = new Family(6, 0.3f, Family.maternal);  // el macho no cría, como en el perro doméstico
    public Family group = defaultGroup;
    public override Family Group { get => group; set => group = value; }

    // Escala medida contra el mesh crudo (ver AnimalPrefabGenerator > Measure Raw Animal Sizes):
    // altura cruda 3.388m -> objetivo realista de altura de hombro adulto ~0.58m.
    public static Physiognomy defaultBody = new Physiognomy(new Vector3(0.171f, 0.171f, 0.171f), 25, 0.09f, 0.2f, 0.05f);
    public Physiognomy body = defaultBody;
    public override Physiognomy Body { get => body; set => body = value; }

    public ActionsPrep actsPrep = new ActionsPrep
    (
        new ActionPrep("IdleHusky", 0, 1, -2),
        new ActionPrep("WalkHusky", 4, 3),
        new ActionPrep("RunHusky", 20, 5, 2)   // resistencia + sprint, criado para el frío
    );
    public override ActionsPrep ActsPrep { get => actsPrep; set => actsPrep = value; }

    public Vector3 homeOrigin;
    public override Vector3 HomeOrigin { get => homeOrigin; set => homeOrigin = value; }

    public float homeRadius = 100;   // se queda cerca del santuario, no es territorial como el lobo
    public override float HomeRadius { get => homeRadius; set => homeRadius = value; }

    // Stages (días de juego) — madurez a ~1 año; longevidad de perro cuidado (~12-14 años).
    public Childhood childhood = new Childhood(60, 98, 99);   // destete completo ~8 semanas
    public override Childhood ChildStage { get => childhood; set => childhood = value; }

    public byte[] childPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] ChildPreps { get => childPreparations; set => childPreparations = value; }

    public byte[] childEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound };
    public override byte[] ChildEvents { get => childEvents; set => childEvents = value; }

    public Adolescence adolescence = new Adolescence(300, 70, 85);
    public override Adolescence TeenStage { get => adolescence; set => adolescence = value; }

    public byte[] teenPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] TeenPreps { get => teenPreparations; set => teenPreparations = value; }

    public byte[] teenEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound };
    public override byte[] TeenEvents { get => teenEvents; set => teenEvents = value; }

    public Adulthood adulthood = new Adulthood(4380, 0, 20);  // ~12 años, longevidad de perro cuidado
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
    public override AnimationsName animationsName { get; } = new AnimationsName("Husky");

    // Diet: casi no caza — instinto de presa débil, alta dificultad para que se moleste.
    public static Diet defaultDiet = new Diet(new PreyEntry[]
    {
        new PreyEntry(BunnyBehavior.population, 4f, 40f, 300f),
    });
    public Diet diet = defaultDiet;
    public override Diet Diet { get => diet; set => diet = value; }

    // Post-natal species params
    public override float BaseStressLevel       => 0.2f;   // acostumbrado al contacto humano
    public override float VocalizationThreshold => 2f;     // raza muy vocal, "habla"/aúlla con facilidad
    public override float NestSecurityLevel     => 0.8f;   // nido/kennel mantenido y protegido
    public override float MaxFatReserves        => 10f;    // atleta magro, no depende de grasa acumulada
    public override float FatAccumulationRate   => 0.4f;

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
    public override float Aggressiveness => 0.15f;
    public override bool DefendsCubs => true;
    public override bool CanHitAndRun => false;
    public override float PackFactor => 0.9f;   // fuerte instinto de equipo de trineo
    public override float HarmVsBond => 0.3f;
    public override float BondGrowthRate => 2.2f;  // se vincula muy rápido, la raza más sociable del santuario
    public override float BiteSize => 3f;
    public override float Toughness => 0.6f;

    void Start() => base.Init();

}
