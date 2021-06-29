using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BunnyBehavior : Animal
{
    // Family creation default values
    public override char ParentalCare { get; set; } = Family.maternal;
    public override float ParentsRate { get; set; } = 0.14f;
    public override byte FamilySize { get; set; } = 6;



    // Stages
    public Childhood childhood = LifeStage.GetChildhood(new Vector3(0.7f, 0.7f, 0.7f), 50, 50, 80);
    public override Childhood ChildStage { get => childhood; set => childhood = value; }

    public byte[] childPreparations = { Childhood.Preparations.SetScale, Childhood.Preparations.SetStageDays };
    public override byte[] ChildPreparations { get => childPreparations; set => childPreparations = value; }

    public byte[] childEvents = { Childhood.Events.LoopGrow };
    public override byte[] ChildEvents { get => childEvents; set => childEvents = value; }
    
    
    public Adolescence adolescence = LifeStage.GetAdolescence(new Vector3(0.7f, 0.7f, 0.7f), 680, 20, 40);
    public override Adolescence TeenStage { get => adolescence; set => adolescence = value; }


    public Adulthood adulthood = LifeStage.GetAdulthood(new Vector3(0.7f, 0.8f, 0.8f), 2190, 0, 20);
    public override Adulthood AdultStage { get => adulthood; set => adulthood = value; }


    



    public static HashSet<GameObject> population = new HashSet<GameObject>();
    public override HashSet<GameObject> Population { get => population; set => population = value; }
    public bool hunting;
    public override AnimationsName animationsName { get; } = new AnimationsName("Bunny");



    public GameObject mom, player;
    public WolfBehavior[] children;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public override IEnumerator Follow()
    {
        while (1 == 1)
        {
            ani.speed = 1;
            ani.Play(animationsName.idle);
            moving = true;
            int i = Random.Range(5, 10);
            int interval = i;
            bird = Respawn.birds[Random.Range(0, Respawn.birds.Count)];
            int rest = (Random.Range(2, 5) * 10);
            if (rest > 29)
            {
                yield return new WaitForSeconds(rest);
            }
            ani.Play(animationsName.run);
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
        moving = false;
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

    public override IEnumerator Escape(bool team, List<GameObject> enemies)
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
                        ani.Play(animationsName.run);
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
        StartCoroutine(Follow());
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
            if (zDistance < 1)
            {
                float xDistance = Vector3.Distance(new Vector3(bulletPosition.x, 0), new Vector3(this.transform.position.x, 0));
                if (xDistance < 1)
                {
                    float yDistance = Vector3.Distance(new Vector3(0, bulletPosition.y), new Vector3(0, this.transform.position.y));
                    if (yDistance < 1)
                    {
                        this.exhaustion += 2;
                        StopCoroutine("Hunt");
                        StopCoroutine("Escape");
                        hunting = false;
                        StartCoroutine("Sleep");
                        Debug.Log(this.gameObject);
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(0.05f);
        } while (wait > 0);
    }
}
