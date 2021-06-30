using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class Animal : MonoBehaviour, IAnimal, IFactory
{
    // Family creation default values
    public abstract char ParentalCare { get; set; }
    public abstract float ParentsRate { get; set; }
    public abstract byte FamilySize { get; set; }



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


    // Physiognomy
    public char sex;
    public char lifeStage;
    public bool adult, gender = false;
    public abstract float BaseMass { get; set; }
    public abstract Vector3 BaseScale { get; set; }
    public Vector3 size, sizePotential;


    // State
    public bool moving = false, alone, asleep = false, death = false;
    public float hungry, exhaustion, lp, sensibility;


    // Gameobject components
    public bool aware { get; set; } = false;
    public NavMeshAgent nav;
    public Rigidbody rig;
    public Animator ani;
    public HashSet<GameObject> parents;

    public abstract AnimationsName animationsName { get; }
    public GameObject bird;



    public virtual void Init ()
    {
        nav = GetComponent<NavMeshAgent>();
        rig = GetComponent<Rigidbody>();
        ani = GetComponent<Animator>();
        StartCoroutine("Restore");
        LifeStage.Init(this, TimeController.timeController);
    }
    public static GameObject[] StaticGenerateSquareRange(GameObject animal, int quantity, float range, float respawnHeight)
    {
        GameObject[] creatures = new GameObject[quantity];
        for (int idx = 0; quantity > idx; idx++)
        {
            GameObject creature = Instantiate(animal, new Vector3(Random.Range(0, range), respawnHeight, Random.Range(0, range)), animal.transform.rotation);
            Vector3 scale = creature.transform.localScale;
            creature.transform.localScale = new Vector3(scale.x - Random.Range(0.1f, 0.4f), scale.y - Random.Range(0.1f, 0.4f), scale.z - Random.Range(0.1f, 0.4f));
            creatures[idx] = creature;
            Animal creatureScript = creature.GetComponent<Animal>();
            creatureScript.Population.Add(creature);
            wholePopulation.Add(creature);
        }
        return creatures;
    }
    public virtual GameObject[] GenerateSquareRange(GameObject animal, int quantity, float range, float respawnHeight)
    {
        return Animal.StaticGenerateSquareRange(animal, quantity, range, respawnHeight);
    }
    public virtual GameObject[] RenderFamily (Vector3 position, float height, int minParentsCount = 0, int familySize = 0)
    {
        familySize = familySize > 0 ? familySize : this.FamilySize;
        return Family.RenderFamily(this.gameObject, familySize , this.ParentsRate, minParentsCount, this.ParentalCare, position, height);
    }
    public abstract IEnumerator Escape(bool team, List<GameObject> enemies);
    public abstract IEnumerator Follow();
    public virtual void Hurt(float damage)
    {
        lp -= damage;
        exhaustion += damage;
        if (lp < (rig.mass * 0.7))
        {
            StopAllCoroutines();
            ani.speed = 0;
            this.rig.isKinematic = true;
            this.nav.enabled = false;
            if (!death) transform.Rotate(Vector3.forward, 90);
            death = true;
        }
        if (lp < 0)
        {
            Population.Remove(this.gameObject);
            wholePopulation.Remove(this.gameObject);
            Destroy(this);
        }
    }



    // Gizmos
    public float gizmoSphereRadio = 5;
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, gizmoSphereRadio);
    }
}