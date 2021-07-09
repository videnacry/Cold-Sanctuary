using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class Animal : MonoBehaviour, IAnimal, IFactory
{
    #region Family
    /// <summary>
    /// Properties wich determine how is going te be the created family of an instance
    /// </summary>
    public abstract Family Group { get; set; }
    #endregion


    // Stages
    public abstract Childhood ChildStage { get; set; }
    public abstract byte[] ChildPreps { get; set; }
    public abstract byte[] ChildEvents { get; set; }

    public abstract Adolescence TeenStage { get; set; }
    public abstract byte[] TeenPreps { get; set; }
    public abstract byte[] TeenEvents { get; set; }

    public abstract Adulthood AdultStage { get; set; }
    public abstract byte[] AdultPreps { get; set; }
    public abstract byte[] AdultEvents { get; set; }



    // Population
    public static HashSet<GameObject> wholePopulation = new HashSet<GameObject>();
    public abstract HashSet<GameObject> Population { get; set; }


    #region Physiognomy
    /// <summary>
    /// Field with property wich contains the base value for new instances
    /// </summary>
    public char sex;
    public char lifeStage;
    public abstract Physiognomy Body { get; set; }
    public abstract ActionsPrep ActsPrep { get; set; }
    #endregion



    public abstract Vector3 HomeOrigin { get; set; }
    public abstract float HomeRadius { get; set; }



    // State
    public bool asleep = false, death = false, busy = false;
    public float hungry, exhaustion, lp, sensibility;


    // Gameobject components
    public bool aware { get; set; } = false;
    public NavMeshAgent nav;
    public Rigidbody rig;
    public Animator ani;

    public abstract AnimationsName animationsName { get; }
    public GameObject bird;
    public GameObject target;



    public virtual void Init()
    {
        Population.Add(gameObject);
        wholePopulation.Add(gameObject);
        HomeOrigin = transform.position;
        nav = GetComponent<NavMeshAgent>();
        rig = GetComponent<Rigidbody>();
        ChildStage.Fatten()(this, 0);
        ani = GetComponent<Animator>();
        StartCoroutine(Restore());
        LifeStage.Init(this, TimeController.timeController);
    }
    public static GameObject[] StaticGenerateSquareRange(GameObject animal, GameObject area, int quantity)
    {
        Bounds bounds = area.GetComponent<Collider>().bounds;
        Vector3 minPos = bounds.min;
        Vector3 maxPos = bounds.max;
        GameObject[] creatures = new GameObject[quantity];
        for (int idx = 0; quantity > idx; idx++)
        {
            Vector3 pos = new Vector3(Random.Range(minPos.x, maxPos.x), maxPos.y + 1, Random.Range(minPos.z, maxPos.z));
            GameObject creature = Instantiate(animal, pos, animal.transform.rotation);
            Vector3 scale = creature.transform.localScale;
            creature.transform.localScale = new Vector3(scale.x - Random.Range(0.1f, 0.4f), scale.y - Random.Range(0.1f, 0.4f), scale.z - Random.Range(0.1f, 0.4f));
            creatures[idx] = creature;
        }
        return creatures;
    }
    public virtual GameObject[] GenerateSquareRange(GameObject animal, GameObject area, int quantity)
    {
        return Animal.StaticGenerateSquareRange(animal, area, quantity);
    }
    public virtual Animal[] RenderFamily(Vector3 position, float height, int minParentsCount = 0, int familySize = 0)
    {
        familySize = familySize > 0 ? familySize : this.Group.familySize;
        return Family.RenderFamily(this.gameObject, familySize, this.Group.parentsRate, minParentsCount, this.Group.parentalCare, position, height);
    }








    public IEnumerator Restore()
    {
        float interval = TimeController.timeController.TimeSpeedMinuteSecs / Random.Range(0.8f, 1.2f);
        while (1 == 1)
        {
            if (hungry >= 0 && !asleep && !this.busy)
            {
                StartCoroutine("Feed");
            }
            yield return new WaitForSeconds(interval);
        }
    }



    public abstract IEnumerator Feed();



    /// <summary>
    /// It will wair 3 seconds for every calculation
    /// </summary>
    /// <param name="team"> if its prey of a team </param>
    /// <param name="enemies"> the enemies </param>
    /// <returns></returns>

    public virtual IEnumerator Escape(bool team, List<GameObject> enemies)
    {
        GameObject enemy = enemies[0];
        float enemyMass = enemy.GetComponent<Rigidbody>().mass;
        float enemySpeed = enemy.GetComponent<NavMeshAgent>().speed;
        Vector3 enemyPosition = enemy.transform.position;
        do
        {
            if (enemyMass * (enemySpeed / 2) - Vector3.Distance(enemyPosition, transform.position) > sensibility)
            {
                aware = true;

                while (Vector3.Distance(transform.position, enemyPosition) < 620)
                {
                    int afraid = 30;
                    this.ActsPrep.run.Prep(this, (short)(this.ActsPrep.run.energyCost / 10));
                    while (afraid > 0)
                    {
                        afraid--;
                        nav.SetDestination(BirdBehavior.population.ElementAt(Random.Range(0, (BirdBehavior.population.Count - 1))).transform.position);
                        yield return new WaitForSeconds(10);
                    }
                }
            }
            else
            {
                if (Random.Range(1, 3) > 1) this.ActsPrep.walk.Prep(this, (short)(this.ActsPrep.run.energyCost / 10));
                else this.ActsPrep.run.Prep(this, (short)(this.ActsPrep.run.energyCost / 10));
                aware = false;
                yield return new WaitForSeconds(TimeController.timeController.TimeSpeedMinuteSecs / 20);
            }
        } while (!aware);
    }





    /// <summary>
    /// Inflict damage, remove gameObject from population and wholePopulation fields, set lifestage to soul and rig.mass to 0
    /// </summary>
    /// <param name="damage"></param>
    public virtual void Hurt(float damage)
    {
        lp -= damage;
        exhaustion += damage;
        if (!death)
        {
            if (lp < (rig.mass * 0.7))
            {
                transform.Rotate(Vector3.forward, 90);
                death = true;
                StopAllCoroutines();
                ani.enabled = false;
                this.rig.isKinematic = true;
                this.nav.enabled = false;
            }
        }
        else
        {
            this.rig.mass -= damage;
        }
        if (this.rig.mass <= 0.1)
        {
            Population.Remove(this.gameObject);
            wholePopulation.Remove(this.gameObject);
            this.lifeStage = LifeStage.soul;
        }
    }



    // Gizmos
    public float gizmoSphereRadio = 5;
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, gizmoSphereRadio);
    }



    // Collision
    public void OnCollisionEnter(Collision collision)
    {
        nav.enabled = true;
        GetComponent<BoxCollider>().enabled = false;
    }
}