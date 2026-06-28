using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SealBehavior : Herbivore
{
    public static Family defaultGroup = new Family(8, 0.3f, Family.maternal);
    public Family group = defaultGroup;
    public override Family Group { get => group; set => group = value; }

    public static Physiognomy defaultBody = new Physiognomy(new Vector3(1.5f, 1.5f, 1.5f), 80, 0.05f, 0.3f, 0.1f);
    public Physiognomy body = defaultBody;
    public override Physiognomy Body { get => body; set => body = value; }

    public ActionsPrep actsPrep = new ActionsPrep(
        new ActionPrep("IdleSeal", 0, 1, -2),
        new ActionPrep("WalkSeal", 4, 2),
        new ActionPrep("RunSeal", 10, 3, 2)
    );
    public override ActionsPrep ActsPrep { get => actsPrep; set => actsPrep = value; }

    public Vector3 homeOrigin;
    public override Vector3 HomeOrigin { get => homeOrigin; set => homeOrigin = value; }

    public float homeRadius = 150;
    public override float HomeRadius { get => homeRadius; set => homeRadius = value; }

    // Stages (días de juego)
    public Childhood childhood = new Childhood(45, 50, 80);
    public override Childhood ChildStage { get => childhood; set => childhood = value; }

    public byte[] childPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] ChildPreps { get => childPreparations; set => childPreparations = value; }

    public byte[] childEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound };
    public override byte[] ChildEvents { get => childEvents; set => childEvents = value; }

    public Adolescence adolescence = new Adolescence(365, 60, 80);
    public override Adolescence TeenStage { get => adolescence; set => adolescence = value; }

    public byte[] teenPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] TeenPreps { get => teenPreparations; set => teenPreparations = value; }

    public byte[] teenEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest };
    public override byte[] TeenEvents { get => teenEvents; set => teenEvents = value; }

    public Adulthood adulthood = new Adulthood(6000, 0, 20);
    public override Adulthood AdultStage { get => adulthood; set => adulthood = value; }

    public byte[] adultPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] AdultPreps { get => adultPreparations; set => adultPreparations = value; }

    public byte[] adultEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound, LifeStage.Events.Feed };
    public override byte[] AdultEvents { get => adultEvents; set => adultEvents = value; }

    public static HashSet<GameObject> population = new HashSet<GameObject>();
    public override HashSet<GameObject> Population { get => population; set => population = value; }
    public override AnimationsName animationsName { get; } = new AnimationsName("Seal");

    public override float HarmVsBond => 0.1f;
    public override float BondGrowthRate => 2.0f;
    public override OrganicMaterial Material => OrganicMaterial.Fish;
    public override float Toughness => 0.5f;

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
