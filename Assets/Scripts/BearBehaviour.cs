using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BearBehaviour : MonoBehaviour, DestructibleStats
{
    public bool gender = false, moving = false, adult, hunting, alone;
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

    public GameObject enemy;
    public Vector3 enemyPosition;
    public float enemySpeed, enemyMass;

    GameObject bird;
    public GameObject mom, player;
    public BearBehaviour[] children = new BearBehaviour[2];

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
        if (!moving&&!asleep&&!hunting)
        {
            StartCoroutine("Follow");
        }
    }

    public void Hurt(float damage)
    {
        lp -= damage;
        exhaustion += damage;
        if (lp < (rig.mass / 3))
        {
            StopAllCoroutines();
            transform.Rotate(Vector3.forward, 90);
        }
    }

    public void Size()
    {
        size = transform.localScale;
    }

    public void Attack(Vector3 threat)
    {
        this.nav.SetDestination(threat);
    }

    public void Deffend(Vector3 threat)
    {
        this.nav.SetDestination(threat);
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
        ani.Play("RunBear");
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
    }

    public IEnumerator Sleep()
    {
        StopCoroutine("Follow");
        asleep = true;
        while (exhaustion > 1)
        {
            ani.speed = 0;
            yield return new WaitForSeconds(130);
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

    public IEnumerator Escape(bool team, List<GameObject> enemies )
    {
        do
        {
            if (enemyMass * enemySpeed - Vector3.Distance(enemyPosition, transform.position) > sensibility)
            {
                StopAllCoroutines();
                moving = true;
                asleep = false;
                aware = true;
                bool equalized = ((this.enemyMass + this.enemyMass / 8) <= this.rig.mass) ? true : false;
                bool stronger = ((this.enemyMass * 3) < this.rig.mass) ? true : false;
                float mass = rig.mass;
                if (alone)
                {

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
        List<GameObject> bears = new List<GameObject>();
        bears.Add(this.gameObject);
        moving = false;
        hunting = true;
        if (adult)
        {
            StopCoroutine("Follow");
            GameObject prey;
            try 
            {
                prey = (mom.transform.position != player.transform.position) ? player : bird;
            }
            catch
            {
                prey = player;   
            }
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
            StartCoroutine(victim.Escape(false, bears));
            float cansancio = 0;
            do
            {
                location = prey.transform.position;
                distance = Vector3.Distance(location, transform.position);
                if (distance < 40 || victim.aware)
                {
                    cansancio += 0.1f;
                    nav.speed = 6;
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
                    nav.speed = 3;
                }
                nav.SetDestination(location);
                yield return new WaitForSeconds(.1f);
            } while (distance < 440 && cansancio > 0);
            exhaustion += cansancio;
            nav.speed = 3;
        }
        hunting = false;
    }
}
