using System.Collections.Generic;
using UnityEngine;

public abstract class Family : MonoBehaviour
{
    public const char paternal = 'p', maternal = 'm', biparental = 'b';
    public char sex;
    public bool gender = false, moving = false, adult, alone, asleep = false, death = false;
    public HashSet<GameObject> parents = new HashSet<GameObject>();
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
            Animal.wholePopulation.Add(creature);
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
            if (minParentsCount > parentsCount || parentsRandomRate > (parentsCount / creatures.Length))
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
    public static GameObject[] RenderFamily(GameObject animal, int quantity, float parentsRandomRate, int minParentsCount, char parentalCare, Vector3 position, float height)
    {
        GameObject[] creatures = SetGendersRate(RenderGroup(animal, quantity, position, height), 0.5f, Sex.female);
        creatures = parentalCare switch
        {
            Family.maternal => SetParents(creatures, parentsRandomRate, minParentsCount, false, Sex.female),
            Family.paternal => SetParents(creatures, parentsRandomRate, minParentsCount, false, Sex.male),
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
}