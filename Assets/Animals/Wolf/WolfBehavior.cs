using System.Collections;
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



    // Stages
    public Childhood childhood = LifeStage.GetChildhood(new Vector3(0.7f, 0.7f, 0.7f), 50, 50, 80);
    public override Childhood Childhood { get => childhood; set => childhood = value; }


    public Adolescence adolescence = LifeStage.GetAdolescence(new Vector3(0.7f, 0.7f, 0.7f), 680, 20, 40);
    public override Adolescence Adolescence { get => adolescence; set => adolescence = value; }


    public Adulthood adulthood = LifeStage.GetAdulthood(new Vector3(0.7f, 0.8f, 0.8f), 2190, 0, 20);
    public override Adulthood Adulthood { get => adulthood; set => adulthood = value; }




    public static HashSet<GameObject> population = new HashSet<GameObject>();
    public override HashSet<GameObject> Population { get => population; set => population = value; }
    public override AnimationsName animationsName { get; } = new AnimationsName("Wolf");
    public bool hunting;


    public GameObject mom, player;
    public WolfBehavior[] group;
    public WolfBehavior[] children;
    public WolfBehavior alpha;

    // Start is called before the first frame update
    void Start()
    {
        size = transform.localScale;
        nav = GetComponent<NavMeshAgent>();
        rig = GetComponent<Rigidbody>();
        ani = GetComponent<Animator>();
        lp = this.rig.mass;

        StartCoroutine("Grow");
        StartCoroutine("Restore");
    }

    // Update is called once per frame
    void Update()
    {
        if (!moving && !asleep && !hunting)
        {
            StartCoroutine("Follow");
        }
    }
    
    public void Alpha(WolfBehavior[] wolves, WolfBehavior[] cups)
    {
        alpha = GetComponent<WolfBehavior>();
        group = wolves;
        children = cups;
        foreach(WolfBehavior wolf in group)
        {
            wolf.alpha = alpha;
            wolf.alone = false;
        }
    }

    public void Size()
    {
        size = transform.localScale;
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

    public override IEnumerator Follow()
    {
        moving = true;
        int i = UnityEngine.Random.Range(5, 10);
        int interval = i;
        bird = Respawn.birds[UnityEngine.Random.Range(0, Respawn.birds.Count)];
        int rest = (UnityEngine.Random.Range(3, 6) * 10);
        if (rest > 29)
        {
            ani.speed = 1;
            ani.Play(animationsName.idle);
            yield return new WaitForSeconds(rest);
        }
        ani.Play(animationsName.walk);
        nav.speed = 3;
        ani.speed = 1;
        int distance = UnityEngine.Random.Range(1, 100);
        Vector3 distancePosition = new Vector3(distance, 0, distance);
        while (i > 0)
        {
            nav.SetDestination(new Vector3(bird.transform.position.x, transform.position.y, bird.transform.position.z) + distancePosition);
            i--;
            yield return new WaitForSeconds(interval);
        }
        ani.Play(animationsName.idle);
        ani.speed = 1;
        nav.speed = 0;
        moving = false;
    }

    public IEnumerator Restore()
    {
        int interval = UnityEngine.Random.Range(40, 70);
        while (1 == 1)
        {
            hungry += 1f;
            if (!asleep)
            {
                exhaustion += 0.5f;
                if (exhaustion > 3 && !hunting)
                {
                    StartCoroutine("Sleep");
                }
                if (hungry > 3 && !asleep)
                {
                    StartCoroutine("Hunt");
                }
            }
            yield return new WaitForSeconds(interval);
        }
    }

    public IEnumerator Grow()
    {
        int interval = UnityEngine.Random.Range(12, 15);
        while (size.magnitude < sizePotential.magnitude)
        {
            if (Vector3.Distance(transform.position, mom.transform.position) > 5)
            {
                bird = mom;
                asleep = false;
                nav.speed = 5;
                ani.speed = 5;
            }
            size += (Vector3.one * Time.deltaTime);
            yield return new WaitForSeconds(interval);
        }
        adult = true;
        interval *= 2;
        while ( 1 == 1)
        {
            if(Vector3.Distance(transform.position, alpha.transform.position) > 100)
            {
                int distance = UnityEngine.Random.Range(3, 12);
                nav.SetDestination(alpha.transform.position + new Vector3(distance, distance, distance));
                asleep = false;
                nav.speed = 8;
                ani.speed = 8;
            }
            else
            {
                ani.speed = 0;
            }
            yield return new WaitForSeconds(interval);
        }
    }

    public IEnumerator Sleep()
    {
        StopCoroutine("Follow");
        moving = false;
        asleep = true;
        while (exhaustion > 1)
        {
            ani.speed = 0;
            yield return new WaitForSeconds(115);
            exhaustion-=1.5f;
        }
        asleep = false;
    }

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
                StopCoroutine(Follow());
                StopCoroutine(Sleep());
                moving = true;
                asleep = false;
                aware = true;
                bool equalized, stronger;
                float mass;
                if (alone)
                {
                    mass = this.rig.mass;
                }
                else
                {
                    mass = alpha.rig.mass;
                }
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

    /// <summary>
    /// Stops walking, go to hunt the near rabbit
    /// </summary>
    /// <returns>yield</returns>

    public IEnumerator Hunt()
    {
        List<GameObject> wolves = new List<GameObject>
        {
            this.gameObject
        };
        moving = false;
        hunting = true;
        if (adult)
        {
            StopCoroutine("Follow");
            if(bird == null) bird = Respawn.birds[UnityEngine.Random.Range(0, Respawn.birds.Count)];
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
                if (distance < 80 || victim.aware || cansancio < 1)
                {
                    ani.Play(animationsName.run);
                    cansancio += 0.01f;
                    nav.speed = 10;
                    ani.speed = 10;
                    if (distance < 6)
                    {
                        if (hungry < 0) break;
                        if (prey.GetComponent<Animal>().lp <= 0) break;
                        hungry -= 0.2f;
                        victim.Hurt(0.4f);
                    }
                }
                else
                {
                    ani.speed = 3;
                    nav.speed = 3;
                }
                nav.SetDestination(location);
                yield return new WaitForSeconds(3);
            } while (distance < 580 && cansancio < 1);
            exhaustion += cansancio;
            nav.speed = 0;
            ani.Play(animationsName.idle);
            ani.speed = 1;
        }
        hunting = false;
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
                        StopCoroutine("Hunt");
                        StopCoroutine("Escape");
                        hunting = false;
                        StartCoroutine("Sleep");
                        Debug.Log(this.gameObject);
                    }
                }
            }
            yield return new WaitForSeconds(0.5f);
        } while (wait > 0);
    }
}
