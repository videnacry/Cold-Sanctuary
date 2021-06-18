using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Animal : MonoBehaviour, IAnimal, IFactory
{
    public static HashSet<GameObject> wholePopulation = new HashSet<GameObject>();
    public abstract HashSet<GameObject> Population { get ; set ; }
    public char sex;
    public bool gender = false, moving = false, adult, alone, asleep = false, death = false;
    public float hungry, exhaustion, lp, sensibility;
    public Vector3 size, sizePotential;
    public NavMeshAgent nav;
    public Rigidbody rig;
    public Animator ani;
    public HashSet<GameObject> parents = new HashSet<GameObject>();

    public abstract AnimationsName animationsName { get; }

    public bool aware { get; set; } = false;

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
    public static GameObject[] RenderGroup(GameObject animal, int quantity, Vector3 position, float height)
    {
        GameObject[] creatures = new GameObject[quantity];
        Vector3[] positions = new Vector3[quantity];
        int counter = 0;
        for (int idx = -(quantity / 2); quantity / 2 > idx; idx += counter)
        {
            positions[counter] = new Vector3((position.x * idx) + quantity, height, (position.z * idx) + quantity);
            counter++;
        }
        for (int idx = 0; quantity > idx; idx++)
        {
            GameObject creature = Instantiate(animal, positions[idx], animal.transform.rotation);
            creatures[idx] = creature;
            Animal creatureScript = creature.GetComponent<Animal>();
            creatureScript.Population.Add(creature);
            wholePopulation.Add(creature);
        }
        return creatures;
    }
    public static GameObject[] SetGendersRate (GameObject[] creatures, float rate, char sex)
    {
        foreach (GameObject creature in creatures)
        {
            Animal creatureScript = creature.GetComponent<Animal>();
            creatureScript.sex = Sex.SwitchSex(sex);
            if (Random.Range(0.0f, 1.0f) < rate)
            {
                creatureScript.sex = sex;
            }
        }
        return creatures;
    }
public static GameObject[] SetParents(GameObject[] creatures,float parentsRandomRate, int minParentsCount, bool biparental, char parentalSex)
    {
        float parentsCount = 0;
        foreach (GameObject creature in creatures)
        {
            Animal creatureScript = creature.GetComponent<Animal>();
            if (minParentsCount > parentsCount || parentsRandomRate > parentsCount / creatures.Length)
            {
                parentsCount++;
                creatureScript.adult = true;
                creatureScript.sex = parentalSex;
                RandomRateScale(creature, 0.0f, 0.4f);
                if (biparental)
                {
                    parentalSex = Sex.SwitchSex(parentalSex);
                }
            } else
            {
                creatureScript.adult = false;
                RandomRateScale(creature, 0.3f, 0.9f);
            }
        }
        return creatures;
    }
    public static GameObject[] StaticRenderFamily(GameObject animal, int quantity, float parentsRandomRate, int minParentsCount, string parentalCare, Vector3 position, float height)
    {
        GameObject[] creatures = SetGendersRate(RenderGroup(animal, quantity, position, height), 0.5f, Sex.female);
        creatures = parentalCare switch
        {
            "maternal" => SetParents(creatures, parentsRandomRate, minParentsCount, false, Sex.female),
            "paternal" => SetParents(creatures, parentsRandomRate, minParentsCount, false, Sex.male),
            _ => SetParents(creatures, parentsRandomRate, minParentsCount, true, Sex.female)
        };
        return creatures;
    }
    public static GameObject RandomRateScale(GameObject animal, float minRate, float maxRate)
    {
        Vector3 scale = animal.transform.localScale;
        float scaleBase = Random.Range(minRate + 0.12f, maxRate - 0.12f);
        float minScale = scaleBase - 0.12f;
        float maxScale = scaleBase + 0.12f;
        float scaleX = scale.x - (Random.Range(minScale, maxScale) * scale.x);
        float scaleY = scale.y - (Random.Range(minScale, maxScale) * scale.y);
        float scaleZ = scale.z - (Random.Range(minScale, maxScale) * scale.z);
        animal.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        return animal;
    }
    public static GameObject[] StaticRenderPaternalFamily(GameObject animal, int quantity, float parentsRandomRate, int minParentsCount, Vector3 position, float height)
    {
        return StaticRenderFamily(animal, quantity, parentsRandomRate, minParentsCount, "paternal", position, height);
    }
    public static GameObject[] StaticRenderPaternalFamilyRate(GameObject animal, int quantity, float parentsRandomRate, Vector3 position, float height)
    {
        return StaticRenderPaternalFamily(animal, quantity, parentsRandomRate, 0, position, height);
    }
    public static GameObject[] StaticRenderPaternalFamilyCount(GameObject animal, int quantity, int parentsCount, Vector3 position, float height)
    {
        return StaticRenderPaternalFamily(animal, quantity, 0, parentsCount, position, height);
    }

    public virtual GameObject[] GenerateSquareRange(GameObject animal, int quantity, float range, float respawnHeight)
    {
        return Animal.StaticGenerateSquareRange(animal, quantity, range, respawnHeight);
    }

    public virtual IEnumerator Escape(bool team, List<GameObject> enemies)
    {
        throw new System.NotImplementedException();
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
}