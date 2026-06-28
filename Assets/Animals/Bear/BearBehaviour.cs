using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class BearBehaviour : Carnivore
{
    // Family creation default values
    public static Family defaultGroup = new Family(3, 0.4f, Family.maternal);
    public Family group = defaultGroup;
    public override Family Group { get => group; set => group = value; }

    // Base Physiognomy
    public static Physiognomy defaultBody = new Physiognomy(new Vector3(3.5f, 3.5f, 3.5f), 300, 0.09f, 0.2f, 0.05f);
    public Physiognomy body = defaultBody;
    public override Physiognomy Body { get => body; set => body = value; }

    public ActionsPrep actsPrep = new ActionsPrep
    (
        new ActionPrep("IdleBear", 0, 1, -2),
        new ActionPrep("WalkBear", 3, 2),
        new ActionPrep("RunBear", 12, 4, 2)
    );
    public override ActionsPrep ActsPrep { get => actsPrep; set => actsPrep = value; }

    public Vector3 homeOrigin;
    public override Vector3 HomeOrigin { get => homeOrigin; set => homeOrigin = value; }

    public float homeRadius = 300;
    public override float HomeRadius { get => homeRadius; set => homeRadius = value; }

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

    public static HashSet<GameObject> population = new HashSet<GameObject>();
    public override HashSet<GameObject> Population { get => population; set => population = value; }
    public override AnimationsName animationsName { get; } = new AnimationsName("Bear");

    // ThreatResponse: solitario pero pesado y agresivo → lucha si tiene ventaja de masa.
    public override float Aggressiveness => 0.6f;
    public override bool DefendsCubs => true;
    public override bool CanHitAndRun => false;
    public override float PackFactor => 0.3f;
    public override float HarmVsBond => 0.7f;
    public override float BondGrowthRate => 0.4f;
    public override float BiteSize => 15f;
    public override float Toughness => 2f;

    // Diet: prefiere focas; conejos como alternativa; lobos solo con mucha hambre.
    // El umbral 50 de 'difficulty' está por calibrar — ver decisiones abiertas en behavior-system.md.
    public static Diet defaultDiet = new Diet(new PreyEntry[]
    {
        new PreyEntry(SealBehavior.population, 15f, 0f, 700f),
        new PreyEntry(BunnyBehavior.population, 10f, 0f, 700f),
        new PreyEntry(WolfBehavior.population, 3f, 50f, 500f),
    });
    public Diet diet = defaultDiet;
    public override Diet Diet { get => diet; set => diet = value; }

    void Start()
    {
        base.Init();
    }

    public IEnumerator Shooted(GameObject bullet)
    {
        int wait = 5;
        Vector3 bulletPosition;
        do
        {
            bulletPosition = bullet.transform.position;
            wait--;
            float zDistance = Vector3.Distance(new Vector3(0, 0, bulletPosition.z), new Vector3(0, 0, this.transform.position.z));
            if (zDistance < 2)
            {
                float xDistance = Vector3.Distance(new Vector3(bulletPosition.x, 0), new Vector3(this.transform.position.x, 0));
                if (xDistance < 2)
                {
                    float yDistance = Vector3.Distance(new Vector3(0, bulletPosition.y), new Vector3(0, this.transform.position.y));
                    if (yDistance < 3)
                    {
                        this.exhaustion += 2;
                        StopCoroutine("Feed");
                        StopCoroutine("Escape");
                        this.busy = false;
                        Debug.Log(this.gameObject);
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(0.05f);
        } while (wait > 0);
    }
}
