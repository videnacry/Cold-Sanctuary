using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class WolfBehavior : MonoBehaviour, DestructibleStats
{
    public bool gender = false, moving = false, adult, hunting, alone = false;
    public bool asleep = false;
    public float hungry, exhaustion, lp, sensibility;
    bool consciente;
    public bool aware
    {
        get => this.consciente;
        set => this.consciente = value;
    }

    public Vector3 size, sizePotential;
    public NavMeshAgent nav;
    public Rigidbody rig;
    public Animator ani;


    GameObject bird;
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

    public void Hurt(float damage)
    {
        lp -= damage;
        exhaustion += damage;
        if (lp < (rig.mass / 3))
        {
            StopAllCoroutines();
            ani.speed = 0;
            this.rig.isKinematic = true;
            this.nav.enabled = false;
            transform.Rotate(Vector3.forward, 90);
            Destroy(this);
        }
    }

    public IEnumerable Attack(GameObject threat)
    {
        Vector3 threatPosition;
        DestructibleStats threatLife = threat.GetComponent<DestructibleStats>();
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
        DestructibleStats threatLife = threat.GetComponent<DestructibleStats>();
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

    public IEnumerator Follow()
    {
        moving = true;
        int i = Random.Range(5, 10);
        int interval = i;
        bird = Respawn.birds[Random.Range(Respawn.birds.Count, 0)];
        int rest = (Random.Range(3, 6) * 10);
        if (rest > 29)
        {
            ani.speed = 0;
            yield return new WaitForSeconds(rest);
        }
        nav.speed = 3;
        ani.speed = 3;
        int distance = Random.Range(1, 100);
        Vector3 distancePosition = new Vector3(distance, 0, distance);
        ani.Play("RunWolf");
        while (i > 0)
        {
            nav.SetDestination(new Vector3(bird.transform.position.x, transform.position.y, bird.transform.position.z) + distancePosition);
            i--;
            yield return new WaitForSeconds(interval);
        }
        moving = false;
    }

    public IEnumerator Restore()
    {
        int interval = Random.Range(40, 70);
        while (1 == 1)
        {
            exhaustion += 0.5f;
            hungry += 1f;
            if (exhaustion > 3 && !hunting)
            {
                StartCoroutine("Sleep");
            }
            if (hungry > 3 && !asleep)
            {
                StartCoroutine("Hunt");
            }
            yield return new WaitForSeconds(interval);
        }
    }

    public IEnumerator Grow()
    {
        int interval = Random.Range(12, 15);
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
                int distance = Random.Range(3, 12);
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
        asleep = true;
        while (exhaustion > 1)
        {
            ani.speed = 0;
            yield return new WaitForSeconds(115);
            exhaustion--;
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

    public IEnumerator Escape(bool team, List<GameObject> enemies)
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
                                    ani.Play("RunWolf");
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
                                    ani.Play("WolfRun");
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
        List<GameObject> wolves = new List<GameObject>();
        wolves.Add(this.gameObject);
        moving = false;
        hunting = true;
        if (adult)
        {
            StopCoroutine("Follow");
            GameObject prey = bird;
            Vector3 location = prey.transform.position;
            float distance = Vector3.Distance(transform.position, location);
            foreach (GameObject rabbit in Respawn.rabbits)
            {
                if (distance > Vector3.Distance(transform.position, rabbit.transform.position))
                {
                    distance = Vector3.Distance(transform.position, rabbit.transform.position);
                    location = rabbit.transform.position;
                    prey = rabbit;
                }
                yield return new WaitForSeconds(1);
            }
            BunnyBehavior victim = prey.GetComponent<BunnyBehavior>();
            StartCoroutine(victim.Escape(false, wolves));
            float cansancio = 0;
            do
            {
                location = prey.transform.position;
                distance = Vector3.Distance(location, transform.position);
                if (distance < 80 || victim.aware)
                {
                    cansancio += 0.2f;
                    nav.speed = 10;
                    ani.speed = 10;
                    if (distance < 3)
                    {
                        if (hungry < 0) break;
                        if (!prey.GetComponent<NavMeshAgent>().enabled) break;
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
            ani.speed = 0;
        }
        hunting = false;
    }
}
