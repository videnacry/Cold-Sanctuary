using System.Collections.Generic;
using UnityEngine;

public abstract class Family : MonoBehaviour
{
    // Parental care
    public const char paternal = 'p', maternal = 'm', biparental = 'b';



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
        HashSet<GameObject> parents = new HashSet<GameObject>();
        foreach (GameObject creature in creatures)
        {
            Animal creatureScript = creature.GetComponent<Animal>();
            if (minParentsCount > parentsCount || parentsRandomRate > (parentsCount / creatures.Length))
            {
                parents.Add(creature);
                parentsCount++;
                creatureScript.adult = true;
                creatureScript.lifeStage = LifeStage.adult;
                creatureScript.sex = parentalSex;
                if (biparental)
                {
                    parentalSex = Sex.SwitchSex(parentalSex);
                }
            } else
            {
                creatureScript.parents = parents;
                creatureScript.adult = false;
                creatureScript.lifeStage = LifeStage.child;
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
}