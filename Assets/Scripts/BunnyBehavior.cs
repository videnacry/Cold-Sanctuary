using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.AI;

public class BunnyBehavior : MonoBehaviour,DestructibleStats
{
    public bool gender = false, moving = false, adult, hunting, alone = true;
    bool consciente;
    public bool aware
    {
        get => this.consciente;
        set => this.consciente = value;
    }
    public bool asleep = false;
    public float hungry, exhaustion, lp, sensibility;


    public Vector3 size, sizePotential;
    public NavMeshAgent nav;
    public Rigidbody rig;
    public Animator ani;


    GameObject bird;
    public GameObject mom, player;
    public WolfBehavior[] children;

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

    //IEumerator

    public IEnumerator Follow()
    {
        moving = true;
        int i = Random.Range(5, 10);
        int interval = i;
        bird = Respawn.birds[Random.Range(Respawn.birds.Count, 0)];
        int rest = (Random.Range(2, 5) * 10);
        if (rest > 29)
        {
            ani.Play("IdleBunny");
            ani.speed = 0;
            yield return new WaitForSeconds(rest);
        }
        ani.Play("RunBunny");
        nav.speed = 3;
        ani.speed = 3;
        int distance = Random.Range(1, 100);
        Vector3 distancePosition = new Vector3(distance, 0, distance);
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
        int interval = Random.Range(50, 80);
        while (1 == 1)
        {
            exhaustion += 0.5f;
            if (exhaustion > 3 && !hunting)
            {
                StartCoroutine("Sleep");
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
            yield return new WaitForSeconds(80);
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
            if (enemyMass * (enemySpeed/2) - Vector3.Distance(enemyPosition, transform.position) > sensibility)
            {
                StopCoroutine(Follow());
                StopCoroutine(Sleep());
                moving = true;
                asleep = false;
                aware = true;

                while (Vector3.Distance(transform.position, enemyPosition) < 620)
                {
                    int afraid = 30;
                    nav.speed = 10;
                    ani.speed = 10;
                    while (afraid > 0)
                    {
                        ani.Play("RunBunny");
                        afraid--;
                        nav.SetDestination(bird.transform.position);
                        yield return new WaitForSeconds(10);
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

}
