﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class WolfBehavior : Animal
{
    // Family creation default values
    public override char ParentalCare { get; set; } = Family.biparental;
    public override float ParentsRate { get; set; } = 0.3f;
    public override byte FamilySize { get; set; } = 6;




    // Base Physiognomy
    public float baseMass = 75;
    public override float BaseMass { get => baseMass; set => baseMass = value; }


    public Vector3 baseScale = new Vector3(2.5f, 2.5f, 2.5f);
    public override Vector3 BaseScale { get => baseScale; set => baseScale = value; }
    public ActionsPrep actsPrep = new ActionsPrep
    (
        new ActionPrep("IdleWolf", 0, 1, -2),
        new ActionPrep("WalkWolf", 2, 2),
        new ActionPrep("RunWolf", 22, 5, 3)
    );
    public override ActionsPrep ActsPrep { get => actsPrep; set => actsPrep = value; }




    public Vector3 homeOrigin;
    public override Vector3 HomeOrigin { get => homeOrigin; set => homeOrigin = value; }

    public float homeRadius = 2000;
    public override float HomeRadius { get => homeRadius; set => homeRadius = value; }




    // Stages

    public Childhood childhood = new Childhood(200, 65, 90);
    public override Childhood ChildStage { get => childhood; set => childhood = value; }

    public byte[] childPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] ChildPreps { get => childPreparations; set => childPreparations = value; }

    public byte[] childEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound };
    public override byte[] ChildEvents { get => childEvents; set => childEvents = value; }


    public Adolescence adolescence = new Adolescence(1000, 30, 65);
    public override Adolescence TeenStage { get => adolescence; set => adolescence = value; }

    public byte[] teenPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] TeenPreps { get => teenPreparations; set => teenPreparations = value; }

    public byte[] teenEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound };
    public override byte[] TeenEvents { get => teenEvents; set => teenEvents = value; }


    public Adulthood adulthood = new Adulthood(3285, 0, 20);
    public override Adulthood AdultStage { get => adulthood; set => adulthood = value; }

    public byte[] adultPreparations = { LifeStage.Preps.SetScale, LifeStage.Preps.SetRemainingStageDays };
    public override byte[] AdultPreps { get => adultPreparations; set => adultPreparations = value; }

    public byte[] adultEvents = { LifeStage.Events.LoopGrow, LifeStage.Events.Fatten, LifeStage.Events.Wander, LifeStage.Events.Rest, LifeStage.Events.HomeBound };
    public override byte[] AdultEvents { get => adultEvents; set => adultEvents = value; }




    public static HashSet<GameObject> population = new HashSet<GameObject>();
    public override HashSet<GameObject> Population { get => population; set => population = value; }
    public override AnimationsName animationsName { get; } = new AnimationsName("Wolf");


    // LEGAZY
    public GameObject mom, player;
    public WolfBehavior[] group;
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



    /// <summary>
    /// Stops walking, go to hunt the near rabbit
    /// </summary>
    /// <returns>yield</returns>

    public IEnumerator Feed()
    {
        List<GameObject> wolves = new List<GameObject>
        {
            this.gameObject
        };
        this.busy = true;
        if (adult)
        {
            GameObject prey = BunnyBehavior.population.First();
            Vector3 location = prey.transform.position;
            float distance = Vector3.Distance(transform.position, location);
            foreach (GameObject rabbit in BunnyBehavior.population)
            {
                if (distance > Vector3.Distance(transform.position, rabbit.transform.position))
                {
                    distance = Vector3.Distance(transform.position, rabbit.transform.position);
                    location = rabbit.transform.position;
                    prey = rabbit;
                }
                yield return new WaitForSeconds(1);
            }
            IAnimal victim = prey.GetComponent<IAnimal>();
            StartCoroutine(victim.Escape(false, wolves));
            float cansancio = 0;
            do
            {
                location = prey.transform.position;
                distance = Vector3.Distance(location, transform.position);
                if (distance < 300 || victim.aware)
                {
                    this.ActsPrep.run.Prep(this, (short) (this.ActsPrep.run.energyCost / 30));
                    cansancio += 0.01f;
                    if (distance < 6)
                    {
                        if (hungry < -50) break;
                        if (prey.GetComponent<Animal>().rig.mass <= 0) break;
                        hungry -= 0.2f;
                        victim.Hurt(0.4f);
                    }
                    nav.SetDestination(location);
                    yield return new WaitForSeconds(TimeController.timeController.TimeSpeedMinuteSecs / 60);
                }
                else
                {
                    this.ActsPrep.walk.Prep(this, (short)(this.ActsPrep.run.energyCost / 10));
                    nav.SetDestination(location);
                    yield return new WaitForSeconds(TimeController.timeController.TimeSpeedMinuteSecs / 20);
                }
            } while (distance < 700 && cansancio < 1);
            exhaustion += cansancio;
            this.ActsPrep.idle.Prep(this);
        }
        this.busy = false;
    }
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
