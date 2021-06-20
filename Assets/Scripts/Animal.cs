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
    public abstract int FamilySize { get; set; }


    // Population
    public static HashSet<GameObject> wholePopulation = new HashSet<GameObject>();
    public abstract HashSet<GameObject> Population { get ; set ; }


    // Physiognomy
    public char sex;
    public bool adult, gender=false;
    public float mass;
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
    public virtual IEnumerator Grow()
    {
        int interval = Random.Range(12, 16);
        while (size.magnitude < sizePotential.magnitude)
        {
            if (parents.Count > 0)
            {
                GameObject nearParent = parents.First();
                float nearParentDistance = Vector3.Distance(transform.position, nearParent.transform.position);
                foreach (GameObject parent in parents)
                {
                    float parentDistance = Vector3.Distance(transform.position, parent.transform.position);
                    if (nearParentDistance > parentDistance)
                    {
                        nearParent = parent;
                        nearParentDistance = parentDistance;
                    }
                }
                StopCoroutine(Follow());
                if (nearParentDistance > 5)
                {
                    bird = nearParent;
                    nav.SetDestination(nearParent.transform.position);
                    moving = true;
                    asleep = false;
                    nav.speed = 5;
                    ani.speed = 5;
                }
                else
                {
                    StartCoroutine(Follow());
                }
            }
            size += (Vector3.one * Time.deltaTime);
            transform.localScale += (Vector3.one * Time.deltaTime);
            yield return new WaitForSeconds(interval);
        }
        adult = true;
    }

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