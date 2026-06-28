using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeerBehavior : Herbivore
{
    public static Family defaultGroup = new Family(6, 0.3f, Family.maternal);
    public Family group = defaultGroup;
    public override Family Group { get => group; set => group = value; }

    public static Physiognomy defaultBody = new Physiognomy(new Vector3(2.0f, 2.0f, 2.0f), 90, 0.07f, 0.25f, 0.1f);
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

    // Herbívoro: huye ante amenazas, no lucha.
    public override float Aggressiveness => 0f;
    public override bool DefendsCubs => false;
    public override bool CanHitAndRun => false;
    public override float PackFactor => 0f;
    public override float HarmVsBond => 0.1f;
    public override float BondGrowthRate => 1.8f;
    public override float Toughness => 0.4f;

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
