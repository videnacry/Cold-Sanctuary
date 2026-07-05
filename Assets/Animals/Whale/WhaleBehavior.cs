using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Beluga (Delphinapterus leucas). Datos de referencia: masa adulta 600–1600 kg,
// crucero ~3–9 km/h con picos ~22 km/h, gestación ~14.5 meses, lactancia muy
// extendida (~20-32 meses), madurez sexual ~8-9 años, vida silvestre 35-50 años.
// Extremadamente vocal ("canario del mar") y curiosa con los cuidadores — el
// santuario real de belugas de SEA LIFE Trust en Islandia es la referencia
// directa para el tono de "Cold Sanctuary". Se alimenta por filtrado/pesca
// (Herbivore.Feed la simula de forma abstracta; no hay población de peces
// cazable todavía en el juego).
public class WhaleBehavior : Herbivore
{
    public static Family defaultGroup = new Family(10, 0.25f, Family.maternal);
    public Family group = defaultGroup;
    public override Family Group { get => group; set => group = value; }

    public static Physiognomy defaultBody = new Physiognomy(new Vector3(6.0f, 6.0f, 6.0f), 1300, 0.04f, 0.3f, 0.12f);
    public Physiognomy body = defaultBody;
    public override Physiognomy Body { get => body; set => body = value; }

    public ActionsPrep actsPrep = new ActionsPrep(
        new ActionPrep("IdleWhale", 0, 1, -2),
        new ActionPrep("WalkWhale", 3, 2),   // crucero, real ~3-9 km/h
        new ActionPrep("RunWhale", 8, 3, 2)  // pico, real ~22 km/h — no es un nadador veloz
    );
    public override ActionsPrep ActsPrep { get => actsPrep; set => actsPrep = value; }

    public Vector3 homeOrigin;
    public override Vector3 HomeOrigin { get => homeOrigin; set => homeOrigin = value; }

    public float homeRadius = 400;   // bahía-santuario en mar abierto, el rango más grande del roster
    public override float HomeRadius { get => homeRadius; set => homeRadius = value; }

    // Stages (días de juego) — el animal más longevo del santuario.
    public Childhood childhood = new Childhood(730, 90, 99);   // ~2 años, lactancia extendida
    public override Childhood ChildStage { get => childhood; set => childhood = value; }

    public byte[] childPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] ChildPreps { get => childPreparations; set => childPreparations = value; }

    public byte[] childEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound };
    public override byte[] ChildEvents { get => childEvents; set => childEvents = value; }

    public Adolescence adolescence = new Adolescence(2555, 60, 90);   // ~7 años hasta madurez (~9 años totales)
    public override Adolescence TeenStage { get => adolescence; set => adolescence = value; }

    public byte[] teenPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] TeenPreps { get => teenPreparations; set => teenPreparations = value; }

    public byte[] teenEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest };
    public override byte[] TeenEvents { get => teenEvents; set => teenEvents = value; }

    public Adulthood adulthood = new Adulthood(14600, 0, 20);   // ~40 años, longevidad real de beluga
    public override Adulthood AdultStage { get => adulthood; set => adulthood = value; }

    public byte[] adultPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] AdultPreps { get => adultPreparations; set => adultPreparations = value; }

    public byte[] adultEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed };
    public override byte[] AdultEvents { get => adultEvents; set => adultEvents = value; }

    public static HashSet<GameObject> population = new HashSet<GameObject>();
    public override HashSet<GameObject> Population { get => population; set => population = value; }
    public override AnimationsName animationsName { get; } = new AnimationsName("Whale");

    // Post-natal species params
    public override float BaseStressLevel       => 0.35f;
    public override float VocalizationThreshold => 1.5f;   // la especie más vocal del santuario
    public override float NestSecurityLevel     => 0.5f;   // mar abierto, sin nido físico; protección de manada
    public override float MaxFatReserves        => 120f;   // grasa/blubber, la mayor reserva del roster
    public override float FatAccumulationRate   => 1.8f;

    static readonly PostNatalStage[] _postNatalStages =
    {
        // Stage 0 — Nacimiento en mar abierto; la cría nada por sí sola en minutos
        new PostNatalStage {
            label = "Nacimiento", durationDays = 1f,
            nestType = NestType.OpenWater, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            entryActions = new[] { MotherAction.Clean, MotherAction.Stimulate,
                                   MotherAction.GuideTeat, MotherAction.FirstMilk },
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 1f } },
        },
        // Stage 1 — Lactancia extendida junto a la madre (~20 meses reales)
        new PostNatalStage {
            label = "Lactancia extendida", durationDays = 600f,
            nestType = NestType.OpenWater, fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.Continuous,
            feedingMethod = FeedingMethod.Nurse,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 600f } },
        },
        // Stage 2 — Destete gradual; aprende a pescar junto a la madre
        new PostNatalStage {
            label = "Destete gradual", durationDays = 365f,
            fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.FrequentVisits,
            weaningType = WeaningType.Gradual, feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] {
                new TransitionCondition { kind = TransitionCondition.Kind.TimeElapsed, threshold = 365f },
                new TransitionCondition { kind = TransitionCondition.Kind.FirstSolidEaten },
            },
        },
        // Stage 3 — Independencia gradual dentro de la manada/pod
        new PostNatalStage {
            label = "Independencia", durationDays = 365f,
            fatherRole = FatherRole.Absent,
            presencePattern = MotherPresencePattern.MinimalVisits,
            weaningType = WeaningType.Gradual, feedingMethod = FeedingMethod.FoodItem,
            transitions = new[] { new TransitionCondition
                { kind = TransitionCondition.Kind.TimeElapsed, threshold = 365f } },
        },
    };
    public override PostNatalStage[] PostNatalStages => _postNatalStages;

    // Gentil y curiosa; no lucha, se acerca a los cuidadores con facilidad.
    public override float Aggressiveness => 0f;
    public override bool DefendsCubs => true;
    public override bool CanHitAndRun => false;
    public override float PackFactor => 0f;
    public override float HarmVsBond => 0.1f;
    public override float BondGrowthRate => 2.5f;   // el vínculo más rápido del santuario
    public override OrganicMaterial Material => OrganicMaterial.Fish;
    public override float Toughness => 2.5f;   // capa de grasa gruesa, la más resistente del roster

    void Start() => Init();

    public IEnumerator Shooted(GameObject bullet)
    {
        int wait = 5;
        Vector3 bulletPosition;
        do
        {
            bulletPosition = bullet.transform.position;
            wait--;
            float zDist = Vector3.Distance(new Vector3(0, 0, bulletPosition.z), new Vector3(0, 0, transform.position.z));
            if (zDist < 2)
            {
                float xDist = Vector3.Distance(new Vector3(bulletPosition.x, 0), new Vector3(transform.position.x, 0));
                if (xDist < 2)
                {
                    float yDist = Vector3.Distance(new Vector3(0, bulletPosition.y), new Vector3(0, transform.position.y));
                    if (yDist < 3)
                    {
                        exhaustion += 2;
                        StopCoroutine("Feed");
                        StopCoroutine("Escape");
                        busy = false;
                        Debug.Log(gameObject);
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(0.05f);
        } while (wait > 0);
    }
}
