using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class WolfBehavior : Carnivore
{
    // Family creation default values
    /*
    public override char ParentalCare { get; set; } = Family.biparental;
    public override float ParentsRate { get; set; } = 0.3f;
    public override byte FamilySize { get; set; } = 6;
    */
    public static Family defaultGroup = new Family(6, 0.3f, Family.biparental);
    public Family group = defaultGroup;
    public override Family Group { get => group; set => group = value; }



    // Base Physiognomy
    public static Physiognomy defaultBody = new Physiognomy(new Vector3(2.5f, 2.5f, 2.5f), 45, 0.09f, 0.2f, 0.05f);
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


    // LEGAZY
    public GameObject mom, player;
    public WolfBehavior[] children;

    // Start is called before the first frame update
    void Start()
    {
        base.Init();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public IEnumerable Attack(GameObject threat)
    {
        Vector3 threatPosition;
        IAnimal threatLife = threat.GetComponent<IAnimal>();
        do
        {
            threatLife.Escape(false, new List<GameObject> { this.gameObject });
            threatPosition = threat.transform.position;
            this.nav.SetDestination(threatPosition);
            while (Vector3.Distance(transform.position, threatPosition) < 3)
            {
                threatLife.Hurt((this.rig.mass - this.exhaustion) / 10);
            }
            yield return new WaitForSeconds(10);
        } while (Vector3.Distance(threatPosition, this.transform.position) < 10);
    }

    public IEnumerable Deffend(GameObject threat)
    {
        IAnimal threatLife = threat.GetComponent<IAnimal>();
        Vector3 threatPosition = threat.transform.position;
        do
        {
            threatLife.Escape(false, new List<GameObject> { this.gameObject });
            threatPosition = threat.transform.position;
            this.nav.SetDestination(new Vector3(threatPosition.x, threatPosition.y, threatPosition.z - 1));
            while (Vector3.Distance(transform.position, threatPosition) < 3)
            {
                threatLife.Hurt((this.rig.mass - this.exhaustion) / 10);
            }
            yield return new WaitForSeconds(10);
        } while (Vector3.Distance(threatPosition, this.transform.position) < 10);
    }

    //IEumerator


    /*
    /// <summary>
    /// It will wair 3 seconds for every calculation
    /// It has a foreach that most be changed
    /// </summary>
    /// <param name="team"> if its prey of a team </param>
    /// <param name="enemies"> the enemies </param>
    /// <returns></returns>

    public override IEnumerator Escape(bool team, List<GameObject> enemies)
    {
        GameObject enemy = enemies[0];
        float enemyMass = enemy.GetComponent<Rigidbody>().mass;
        float enemySpeed = enemy.GetComponent<NavMeshAgent>().speed;
        Vector3 enemyPosition = enemy.transform.position;

        do
            {
            if (enemyMass * enemySpeed - Vector3.Distance(enemyPosition, transform.position) > sensibility)
            {
                StopCoroutine(Sleep());
                asleep = false;
                aware = true;
                bool equalized, stronger;
                float mass = this.rig.mass;
                equalized = ((enemyMass + enemyMass / 8) <= mass) ? true : false;
                stronger = ((enemyMass * 3) < mass) ? true : false;
                if (team)
                {
                    if (this.alone)
                    {
                        if (stronger)
                        {
                            Attack(enemy);
                        }
                        else
                        {
                            while (Vector3.Distance(transform.position, enemyPosition) < 100)
                            {
                                int afraid = 30;
                                nav.speed = 10;
                                ani.speed = 10;
                                while (afraid > 0)
                                {
                                    ani.Play(animationsName.run);
                                    afraid--;
                                    nav.SetDestination(bird.transform.position);
                                    yield return new WaitForSeconds(5);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (equalized)
                        {
                            int i = 0;
                            foreach (GameObject threat in enemies)
                            {
                                if (i < group.Length)
                                {
                                    group[i].Attack(threat);
                                    i++;
                                }
                            }
                        }
                        else
                        {
                            int i = 0;
                            foreach (GameObject threat in enemies)
                            {
                                if (i < group.Length)
                                {
                                    group[i].Deffend(threat);
                                    i++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (alone)
                    {

                        if (equalized)
                        {
                            Attack(enemy);
                        }
                        else
                        {
                            while (Vector3.Distance(transform.position, enemyPosition) < 100)
                            {
                                int afraid = 30;
                                nav.speed = 10;
                                ani.speed = 10;
                                while (afraid > 0)
                                {
                                    ani.Play(animationsName.run);
                                    afraid--;
                                    nav.SetDestination(bird.transform.position);
                                    yield return new WaitForSeconds(1);
                                }
                            }

                        }
                    }
                    else
                    {
                        if ((rig.mass * group.Length) > (enemyMass / 1.6))
                        {
                            foreach (WolfBehavior wolf in group)
                            {
                                wolf.Attack(enemy);
                            }
                        }
                    }
                }
            }
            else
            {
                aware = false;
                yield return new WaitForSeconds(3);
            }
        } while (!aware);
    }
    */



    
    public IEnumerator Shooted(Vector3 bulletPosition)
    {
        int wait = 3;
        bulletPosition = transform.InverseTransformPoint(bulletPosition);
        do
        {
            wait--;
            float zDistance = Vector3.Distance(new Vector3(0,0,bulletPosition.z), new Vector3(0,0,this.transform.position.z));
            if (zDistance < 1.5)
            {
                float xDistance = Vector3.Distance(new Vector3(bulletPosition.x,0), new Vector3(this.transform.position.x,0));
                if (xDistance < 0.25)
                {
                    float yDistance = Vector3.Distance(new Vector3(0,bulletPosition.y), new Vector3(0,this.transform.position.y));
                    if (yDistance < 0.5)
                    {
                        this.exhaustion += 2;
                        StopCoroutine("Feed");
                        StopCoroutine("Escape");
                        this.busy = false;
                        Debug.Log(this.gameObject);
                    }
                }
            }
            yield return new WaitForSeconds(0.5f);
        } while (wait > 0);
    }
}
